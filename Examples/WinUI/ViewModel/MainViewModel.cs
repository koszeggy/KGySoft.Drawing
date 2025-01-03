﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: MainViewModel.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2025 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;

using KGySoft.ComponentModel;
using KGySoft.Drawing.Examples.Shared.Interfaces;
using KGySoft.Drawing.Examples.Shared.Model;
using KGySoft.Drawing.Imaging;
using KGySoft.Drawing.WinUI;
using KGySoft.Threading;

using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;

using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;

using KGySoft.Drawing.Examples.Shared;

#endregion

namespace KGySoft.Drawing.Examples.WinUI.ViewModel
{
    internal class MainViewModel : ObservableObjectBase
    {
        #region Configuration record

        private sealed record Configuration : IQuantizerSettings, IDithererSettings // as a record so equality check compares the properties
        {
            #region Properties

            #region Public Properties
            
            public bool ShowOverlay { get; private init; }
            public bool UseLinearColorSpace { get; private set; }
            public bool UseQuantizer { get; private init; }
            public QuantizerDescriptor? SelectedQuantizer { get; private init; }
            public Color32 BackColor { get; private init; }
            public byte AlphaThreshold { get; private init; }
            public byte WhiteThreshold { get; private init; }
            public int PaletteSize { get; private init; }
            public bool UseDithering { get; private init; }
            public DithererDescriptor? SelectedDitherer { get; private init; }

            #endregion

            #region Explicitly Implemented Interface Properties

            bool IQuantizerSettings.DirectMapping => false;
            WorkingColorSpace IQuantizerSettings.WorkingColorSpace => UseLinearColorSpace ? WorkingColorSpace.Linear : WorkingColorSpace.Srgb;
            byte? IQuantizerSettings.BitLevel => null;
            float IDithererSettings.Strength => 0f;
            bool? IDithererSettings.ByBrightness => null;
            bool IDithererSettings.DoSerpentineProcessing => false;
            int? IDithererSettings.Seed => null;

            #endregion

            #endregion

            #region Methods

            internal static Configuration Capture(MainViewModel viewModel) => new Configuration
            {
                ShowOverlay = viewModel.ShowOverlay,
                UseLinearColorSpace = viewModel.UseLinearColorSpace,
                UseQuantizer = viewModel.UseQuantizer,
                SelectedQuantizer = viewModel.SelectedQuantizer,
                BackColor = viewModel.BackColor.ToColor32(),
                AlphaThreshold = (byte)viewModel.AlphaThreshold,
                WhiteThreshold = (byte)viewModel.WhiteThreshold,
                PaletteSize = viewModel.PaletteSize,
                UseDithering = viewModel.UseDithering,
                SelectedDitherer = viewModel.SelectedDitherer,
            };

            #endregion
        }

        #endregion

        #region Fields

        #region Static Fields

        private static readonly HashSet<string> affectsDisplayImage = new()
        {
            nameof(ShowOverlay),
            nameof(UseLinearColorSpace),
            nameof(UseQuantizer),
            nameof(SelectedQuantizer),
            nameof(BackColor),
            nameof(AlphaThreshold),
            nameof(WhiteThreshold),
            nameof(PaletteSize),
            nameof(UseDithering),
            nameof(SelectedDitherer),
        };

        #endregion

        #region Instance Fields

        private readonly SemaphoreSlim syncRoot = new SemaphoreSlim(1, 1);

        private Dictionary<string, Color>? knownColors;
        private WriteableBitmap? baseImage;
        private IReadableBitmapData? overlayImageBitmapData;
        private CancellationTokenSource? cancelGeneratingPreview;
        private Task? generateResultTask;

        #endregion

        #endregion

        #region Properties

        #region Public Properties

        public bool ShowOverlay { get => Get<bool>(); set => Set(value); }
        public bool UseLinearColorSpace { get => Get<bool>(); set => Set(value); }
        public bool UseQuantizer { get => Get<bool>(); set => Set(value); }
        public QuantizerDescriptor[] Quantizers => QuantizerDescriptor.Quantizers;
        public QuantizerDescriptor SelectedQuantizer { get => Get(Quantizers[0]); set => Set(value); }
        public Visibility BackColorVisibility { get => Get<Visibility>(); set => Set(value); }
        public string BackColorText { get => Get("Silver"); set => Set(value); }
        public Color BackColor { get => Get(Colors.Silver); set => Set(value); }
        public Brush? BackColorBrush { get => Get<Brush?>(() => new SolidColorBrush(BackColor)); set => Set(value); }
        public Visibility AlphaThresholdVisibility { get => Get<Visibility>(); set => Set(value); }
        public int AlphaThreshold { get => Get(128); set => Set(value); }
        public Visibility WhiteThresholdVisibility { get => Get<Visibility>(); set => Set(value); }
        public int WhiteThreshold { get => Get(128); set => Set(value); }
        public Visibility MaxColorsVisibility { get => Get<Visibility>(); set => Set(value); }
        public int PaletteSize { get => Get(256); set => Set(value); }
        public bool UseDithering { get => Get<bool>(); set => Set(value); }
        public bool DitheringEnabled { get => Get<bool>(); set => Set(value); }
        public DithererDescriptor[] Ditherers => DithererDescriptor.Ditherers;
        public DithererDescriptor SelectedDitherer { get => Get(Ditherers[0]); set => Set(value); }
        public WriteableBitmap? DisplayImage { get => Get<WriteableBitmap?>(); set => Set(value); }

        #endregion

        #region Private Properties

        private Dictionary<string, Color> KnownColors => knownColors ??= typeof(Colors).GetProperties()
            .ToDictionary(pi => pi.Name, pi => (Color)pi.GetValue(null)!, StringComparer.OrdinalIgnoreCase);

        #endregion

        #endregion

        #region Constructors

        internal MainViewModel()
        {
            SetVisibilities();
            var _ = GenerateDisplayImage(Configuration.Capture(this));
        }

        #endregion

        #region Methods

        #region Static Methods

        private static async Task<WriteableBitmap> LoadResourceBitmap(string fileName)
        {
            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri($"ms-appx:///{fileName}"));
            using IRandomAccessStream imageStream = await file.OpenAsync(FileAccessMode.Read);
            BitmapDecoder bitmapDecoder = await BitmapDecoder.CreateAsync(imageStream);

            PixelDataProvider pixelDataProvider =
                await bitmapDecoder.GetPixelDataAsync(BitmapPixelFormat.Bgra8,
                    BitmapAlphaMode.Premultiplied, new BitmapTransform(),
                    ExifOrientationMode.RespectExifOrientation,
                    ColorManagementMode.ColorManageToSRgb);
            byte[] pixelData = pixelDataProvider.DetachPixelData();

            var result = new WriteableBitmap(
                (int)bitmapDecoder.OrientedPixelWidth,
                (int)bitmapDecoder.OrientedPixelHeight);
            using Stream pixelStream = result.PixelBuffer.AsStream();
            await pixelStream.WriteAsync(pixelData, 0, pixelData.Length);
            return result;
        }

        #endregion

        #region Instance Methods

        #region Protected Methods

        protected override async void OnPropertyChanged(PropertyChangedExtendedEventArgs e)
        {
            base.OnPropertyChanged(e);
            switch (e.PropertyName)
            {
                case nameof(UseQuantizer):
                case nameof(UseDithering):
                case nameof(SelectedQuantizer):
                    DitheringEnabled = UseQuantizer && UseDithering;
                    SetVisibilities();
                    break;

                case nameof(BackColorText):
                    BackColor = TryParseColor(BackColorText, out Color color) ? color : BackColor;
                    BackColorBrush = new SolidColorBrush(BackColor);
                    break;
            }

            if (affectsDisplayImage.Contains(e.PropertyName!))
                await GenerateDisplayImage(Configuration.Capture(this));
        }

        protected override void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;
            if (disposing)
            {
                cancelGeneratingPreview?.Dispose();
                overlayImageBitmapData?.Dispose();
                syncRoot.Dispose();
            }

            base.Dispose(disposing);
        }

        #endregion

        #region Private Methods

        // The caller method is async void, so basically fire-and-forget. To prevent parallel generate sessions we store the current task
        // in the generatePreviewTask field, which can be awaited after a cancellation before starting to generate a new result.
        private async Task GenerateDisplayImage(Configuration cfg)
        {
            baseImage ??= await LoadResourceBitmap("Information256.png");

            // The awaits make this method reentrant, and a continuation can be spawn after any await at any time.
            // Therefore it is possible that despite of clearing generatePreviewTask in WaitForPendingGenerate it is not null upon starting the continuation.
            while (generateResultTask != null)
            {
                CancelRunningGenerate();
                await WaitForPendingGenerate();
            }

            // Using a manually completable task for the generateResultTask field. If this method had just one awaitable task we could simply assign that to the field.
            TaskCompletionSource? generateTaskCompletion = null;
            WriteableBitmap? result = null;

            // This is essentially a lock. Achieved by a SemaphoreSlim because an actual lock cannot be used with awaits in the code.
            await syncRoot.WaitAsync();
            try
            {
                // lost race: returning if configuration has been changed by the time we entered the lock
                if (cfg != Configuration.Capture(this) || IsDisposed)
                    return;

                bool useQuantizer = cfg.UseQuantizer;
                bool showOverlay = cfg.ShowOverlay;
                IQuantizer? quantizer = useQuantizer ? cfg.SelectedQuantizer!.Create(cfg) : null;
                IDitherer? ditherer = useQuantizer && cfg.UseDithering ? cfg.SelectedDitherer!.Create(cfg) : null;
                WorkingColorSpace workingColorSpace = cfg.UseLinearColorSpace ? WorkingColorSpace.Linear : WorkingColorSpace.Srgb;

                // Shortcut: displaying the base image only
                if (!useQuantizer && !showOverlay)
                {
                    result = baseImage;
                    return;
                }

                generateTaskCompletion = new TaskCompletionSource();
                CancellationTokenSource tokenSource = cancelGeneratingPreview = new CancellationTokenSource();
                CancellationToken token = tokenSource.Token;
                generateResultTask = generateTaskCompletion.Task;

                var asyncConfig = new TaskConfig { CancellationToken = token, ThrowIfCanceled = false };

                using IReadableBitmapData baseImageBitmapData = baseImage.GetReadableBitmapData(workingColorSpace);

                // ===== a.) No overlay: just creating a clone bitmap with the specified quantizer/ditherer =====
                if (!showOverlay)
                {
                    result = await baseImageBitmapData.ToWriteableBitmapAsync(quantizer, ditherer, asyncConfig);
                    return;
                }

                // ===== b.) There is an image overlay: working on the bitmap data directly =====
                // b.1.) Cloning the source bitmap first. blendedResult = BitmapDataFactory.Create and then baseImageBitmapData.CopyTo(blendedResult) would also work
                using IReadWriteBitmapData? blendedResult = await baseImageBitmapData.CloneAsync(workingColorSpace, asyncConfig);
                
                if (blendedResult == null || token.IsCancellationRequested)
                    return;

                // b.2.) Drawing the overlay. DrawInto supports alpha blending
                overlayImageBitmapData ??= BitmapDataHelper.GenerateAlphaGradient(baseImageBitmapData.Size);
                await overlayImageBitmapData.DrawIntoAsync(blendedResult, asyncConfig: asyncConfig);

                if (token.IsCancellationRequested)
                    return;

                // b.3.) Converting back to a WinUI bitmap using the desired quantizer and ditherer
                result = await blendedResult.ToWriteableBitmapAsync(quantizer, ditherer, asyncConfig);
            }
            finally
            {
                generateTaskCompletion?.SetResult();
                syncRoot.Release();
                if (result != null)
                    DisplayImage = result;
            }
        }

        private void SetVisibilities()
        {
            bool useQuantizer = UseQuantizer;
            QuantizerDescriptor quantizer = SelectedQuantizer;
            BackColorVisibility = useQuantizer ? Visibility.Visible : Visibility.Collapsed;
            AlphaThresholdVisibility = useQuantizer && quantizer.HasAlphaThreshold ? Visibility.Visible : Visibility.Collapsed;
            WhiteThresholdVisibility = useQuantizer && quantizer.HasWhiteThreshold ? Visibility.Visible : Visibility.Collapsed;
            MaxColorsVisibility = useQuantizer && quantizer.HasMaxColors ? Visibility.Visible : Visibility.Collapsed;
        }

        private bool TryParseColor(string text, out Color color)
        {
            color = default;
            text = text.Trim();
            if (text.Length == 0)
                return false;

            if (text[0] != '#')
                return KnownColors.TryGetValue(text, out color);

            if (text.Length is not (9 or 7 or 5 or 4))
                return false;
            text = text.Substring(1);
            if (!UInt32.TryParse(text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint value))
                return false;

            color = text.Length switch
            {
                8 => Color.FromArgb((byte)(value >> 24),
                    (byte)((value >> 16) & 0xFF),
                    (byte)((value >> 8) & 0xFF),
                    (byte)(value & 0xFF)),
                6 => Color.FromArgb(0xFF,
                    (byte)((value >> 16) & 0xFF),
                    (byte)((value >> 8) & 0xFF),
                    (byte)(value & 0xFF)),
                4 => Color.FromArgb((byte)((value & 0xF000) >> 8 | (value & 0xF000) >> 12),
                    (byte)((value & 0x0F00) >> 4 | (value & 0x0F00) >> 8),
                    (byte)((value & 0x00F0) | (value & 0x00F0) >> 4),
                    (byte)((value & 0x000F) << 4 | (value & 0x000F))),
                3 => Color.FromArgb(0xFF,
                    (byte)((value & 0x0F00) >> 4 | (value & 0x0F00) >> 8),
                    (byte)((value & 0x00F0) | (value & 0x00F0) >> 4),
                    (byte)((value & 0x000F) << 4 | (value & 0x000F))),
                _ => default
            };

            return true;
        }

        private void CancelRunningGenerate()
        {
            var tokenSource = cancelGeneratingPreview;
            if (tokenSource == null)
                return;
            tokenSource.Cancel();
            tokenSource.Dispose();
            cancelGeneratingPreview = null;
        }

        private async Task WaitForPendingGenerate()
        {
            var runningTask = generateResultTask;
            if (runningTask == null)
                return;

            generateResultTask = null;

            try
            {
                await runningTask;
            }
            catch (Exception)
            {
                // pending generate is always awaited after cancellation so ignoring everything from here
            }
        }

        #endregion

        #endregion

        #endregion
    }
}
