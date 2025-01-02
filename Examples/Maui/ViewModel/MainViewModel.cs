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
using System.Threading;
using System.Threading.Tasks;

using KGySoft.ComponentModel;
using KGySoft.Drawing.Examples.Maui.Extensions;
using KGySoft.Drawing.Examples.Shared;
using KGySoft.Drawing.Examples.Shared.Interfaces;
using KGySoft.Drawing.Examples.Shared.Model;
using KGySoft.Drawing.Imaging;
using KGySoft.Threading;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

using SkiaSharp;
using SkiaSharp.Views.Maui.Controls;

#endregion

namespace KGySoft.Drawing.Examples.Maui.ViewModel
{
    internal class MainViewModel : ObservableObjectBase
    {
        #region Configuration record

        private sealed record Configuration : IQuantizerSettings, IDithererSettings // as a record so equality check compares the properties
        {
            #region Properties

            #region Public Properties

            public bool ShowOverlay { get; private init; }
            public bool UseLinearColorSpace { get; private init; }
            public bool UseQuantizer { get; private init; }
            public QuantizerDescriptor? SelectedQuantizer { get; private init; }
            public System.Drawing.Color BackColor { get; private init; }
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
                BackColor = viewModel.BackColor.ToDrawingColor(),
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

        private SKBitmap? baseImage;
        private IReadableBitmapData? overlayImageBitmapData;
        private CancellationTokenSource? cancelGeneratingPreview;
        private Task? generateResultTask;
        private SKBitmap? displayImageBitmap;

        #endregion

        #endregion

        #region Properties

        public bool ShowOverlay { get => Get<bool>(); set => Set(value); }
        public bool UseLinearColorSpace { get => Get<bool>(); set => Set(value); }
        public bool UseQuantizer { get => Get<bool>(); set => Set(value); }
        public QuantizerDescriptor[] Quantizers => QuantizerDescriptor.Quantizers;
        public QuantizerDescriptor SelectedQuantizer { get => Get(Quantizers[0]); set => Set(value); }
        public bool IsBackColorVisible { get => Get<bool>(); set => Set(value); }
        public string BackColorText { get => Get("Silver"); set => Set(value); }
        public Color BackColor { get => Get(Colors.Silver); set => Set(value); }
        public Brush? BackColorBrush { get => Get<Brush?>(() => new SolidColorBrush(BackColor)); set => Set(value); }
        public bool IsAlphaThresholdVisible { get => Get<bool>(); set => Set(value); }
        public int AlphaThreshold { get => Get(128); set => Set(value); }
        public bool IsWhiteThresholdVisible { get => Get<bool>(); set => Set(value); }
        public int WhiteThreshold { get => Get(128); set => Set(value); }
        public bool IsMaxColorsVisible { get => Get<bool>(); set => Set(value); }
        public int PaletteSize { get => Get(256); set => Set(value); }
        public bool UseDithering { get => Get<bool>(); set => Set(value); }
        public bool DitheringEnabled { get => Get<bool>(); set => Set(value); }
        public DithererDescriptor[] Ditherers => DithererDescriptor.Ditherers;
        public DithererDescriptor SelectedDitherer { get => Get(Ditherers[0]); set => Set(value); }
        public ImageSource? DisplayImage { get => Get<ImageSource?>(); set => Set(value); }

        #endregion

        #region Constructors

        public MainViewModel()
        {
            SetVisibilities();
            var _ = GenerateDisplayImage(Configuration.Capture(this));
        }

        #endregion

        #region Methods

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
                    if (Color.TryParse(BackColorText, out Color color))
                    {
                        BackColor = color;
                        BackColorBrush = new SolidColorBrush(color);
                    }

                    break;
            }

            if (affectsDisplayImage.Contains(e.PropertyName!))
                await GenerateDisplayImage(Configuration.Capture(this));
        }

        protected override async void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;
            if (disposing)
            {
                CancelRunningGenerate();
                await WaitForPendingGenerate();
                syncRoot.Dispose();
                baseImage?.Dispose();
                overlayImageBitmapData?.Dispose();
                displayImageBitmap?.Dispose();
            }

            base.Dispose(disposing);
        }

        #endregion

        #region Private Methods

        // The caller method is async void, so basically fire-and-forget. To prevent parallel generate sessions we store the current task
        // in the generatePreviewTask field, which can be awaited after a cancellation before starting to generate a new result.
        private async Task GenerateDisplayImage(Configuration cfg)
        {
            // Note that Images are regular .resx based resources. Make sure their Build Action is None so MAUI's "resizetizer" will not throw a build error.
            baseImage ??= SKBitmap.Decode(Images.Information256);

            // The awaits make this method reentrant, and a continuation can be spawn after any await at any time.
            // Therefore it is possible that despite of clearing generatePreviewTask in WaitForPendingGenerate it is not null upon starting the continuation.
            while (generateResultTask != null)
            {
                CancelRunningGenerate();
                await WaitForPendingGenerate();
            }

            // Using a manually completable task for the generateResultTask field. If this method had just one awaitable task we could simply assign that to the field.
            TaskCompletionSource? generateTaskCompletion = null;
            SKBitmap? result = null;

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
                generateTaskCompletion = new TaskCompletionSource();
                CancellationTokenSource tokenSource = cancelGeneratingPreview = new CancellationTokenSource();
                CancellationToken token = tokenSource.Token;
                generateResultTask = generateTaskCompletion.Task;

                var asyncConfig = new TaskConfig { CancellationToken = token, ThrowIfCanceled = false };

                // NOTE: This GetReadableBitmapData is a local implementation using purely the KGySoft.Drawing.Core package.
                //       For complete SkiaSharp support with all possible pixel formats the example at https://github.com/koszeggy/KGySoft.Drawing/tree/master/Examples/SkiaSharp_(Maui)
                using IReadableBitmapData baseImageBitmapData = baseImage.GetReadableBitmapData(workingColorSpace);

                // ===== a.) No overlay: just creating a clone bitmap with the specified quantizer/ditherer =====
                if (!showOverlay)
                {
                    result = await baseImageBitmapData.ToSKBitmapAsync(quantizer, ditherer, asyncConfig);
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

                // b.3.) Converting back to a Skia bitmap using the desired quantizer and ditherer
                result = await blendedResult.ToSKBitmapAsync(quantizer, ditherer, asyncConfig);
            }
            finally
            {
                generateTaskCompletion?.SetResult();
                syncRoot.Release();

                // To make SKBitmapImageSource work .UseSkiaSharp() must be added to MauiProgram.CreateMauiApp
                if (result != null)
                {
                    SKBitmap? previousBitmap = displayImageBitmap;
                    DisplayImage = new SKBitmapImageSource { Bitmap = result };
                    previousBitmap?.Dispose();
                    displayImageBitmap = result;
                }
            }
        }

        private void SetVisibilities()
        {
            bool useQuantizer = UseQuantizer;
            QuantizerDescriptor quantizer = SelectedQuantizer;
            IsBackColorVisible = useQuantizer;
            IsAlphaThresholdVisible = useQuantizer && quantizer.HasAlphaThreshold;
            IsWhiteThresholdVisible = useQuantizer && quantizer.HasWhiteThreshold;
            IsMaxColorsVisible = useQuantizer && quantizer.HasMaxColors;
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
    }
}
