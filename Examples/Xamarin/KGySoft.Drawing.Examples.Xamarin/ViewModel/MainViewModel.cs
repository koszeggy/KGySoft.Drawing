#region Copyright

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

#region Used Namespaces

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

using KGySoft.ComponentModel;
using KGySoft.CoreLibraries;
using KGySoft.Drawing.Examples.Shared;
using KGySoft.Drawing.Examples.Shared.Enums;
using KGySoft.Drawing.Examples.Shared.Interfaces;
using KGySoft.Drawing.Examples.Shared.Model;
using KGySoft.Drawing.Examples.Xamarin.Extensions;
using KGySoft.Drawing.Imaging;
using KGySoft.Drawing.Shapes;
using KGySoft.Threading;

using SkiaSharp;
using SkiaSharp.Views.Forms;

using Xamarin.Forms;

#endregion

#region Used Aliases

using Brush = KGySoft.Drawing.Shapes.Brush;
using Rectangle = System.Drawing.Rectangle;
using XamarinBrush = Xamarin.Forms.Brush;

#endregion

#endregion

namespace KGySoft.Drawing.Examples.Xamarin.ViewModel
{
    internal class MainViewModel : ObservableObjectBase
    {
        #region Configuration record

        private sealed record Configuration : IQuantizerSettings, IDithererSettings // as a record so equality check compares the properties
        {
            #region Properties

            #region Public Properties

            public bool ShowOverlay { get; private set; }
            internal PathShape OverlayShape { get; private set; }
            internal int OutlineWidth { get; private set; }
            internal Color32 OutlineColor { get; private set; }
            public bool UseLinearColorSpace { get; private set; }
            public bool UseQuantizer { get; private set; }
            public QuantizerDescriptor? SelectedQuantizer { get; private set; }
            public Color32 BackColor { get; private set; }
            public byte AlphaThreshold { get; private set; }
            public byte WhiteThreshold { get; private set; }
            public int PaletteSize { get; private set; }
            public bool UseDithering { get; private set; }
            public DithererDescriptor? SelectedDitherer { get; private set; }

            // Using Half pixel offset for odd pen with, and None for even width to avoid blurry lines. See more details at https://docs.kgysoft.net/drawing/html/P_KGySoft_Drawing_Shapes_DrawingOptions_DrawPathPixelOffset.htm
            internal DrawingOptions DrawingOptions => new DrawingOptions { AntiAliasing = true, DrawPathPixelOffset = (OutlineWidth & 1) == 1 ? PixelOffset.Half : PixelOffset.None };

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
                OverlayShape = viewModel.OverlayShape,
                OutlineWidth = viewModel.OutlineWidth,
                OutlineColor = ((System.Drawing.Color)viewModel.OutlineColor).ToColor32(),
                UseLinearColorSpace = viewModel.UseLinearColorSpace,
                UseQuantizer = viewModel.UseQuantizer,
                SelectedQuantizer = viewModel.SelectedQuantizer,
                BackColor = ((System.Drawing.Color)viewModel.BackColor).ToColor32(),
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
            nameof(OverlayShape),
            nameof(OutlineWidth),
            nameof(OutlineColor),
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

        public PathShape[] OverlayShapes { get; } = Enum<PathShape>.GetValues();

        public bool ShowOverlay { get => Get<bool>(); set => Set(value); }
        public bool IsOutlineVisible { get => Get<bool>(); set => Set(value); }
        public PathShape OverlayShape { get => Get<PathShape>(); set => Set(value); }
        public int OutlineWidth { get => Get<int>(); set => Set(value); }
        public string OutlineColorText { get => Get("Red"); set => Set(value); }
        public Color OutlineColor { get => Get(Color.Red); set => Set(value); }
        public XamarinBrush? OutlineColorBrush { get => Get<XamarinBrush?>(() => new SolidColorBrush(OutlineColor)); set => Set(value); }
        public bool UseLinearColorSpace { get => Get<bool>(); set => Set(value); }
        public bool UseQuantizer { get => Get<bool>(); set => Set(value); }
        public QuantizerDescriptor[] Quantizers => QuantizerDescriptor.Quantizers;
        public QuantizerDescriptor SelectedQuantizer { get => Get(Quantizers[0]); set => Set(value); }
        public bool IsBackColorVisible { get => Get<bool>(); set => Set(value); }
        public string BackColorText { get => Get("Silver"); set => Set(value); }
        public Color BackColor { get => Get(Color.Silver); set => Set(value); }
        public XamarinBrush? BackColorBrush { get => Get<XamarinBrush?>(() => new SolidColorBrush(BackColor)); set => Set(value); }
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

        internal MainViewModel()
        {
            SetVisibilities();
            var _ = GenerateDisplayImage(Configuration.Capture(this));
        }

        #endregion

        #region Methods

        #region Protected Methods

        [SuppressMessage("ReSharper", "AsyncVoidEventHandlerMethod", Justification = "Event handler. See also the comment above GenerateResult.")]
        protected override async void OnPropertyChanged(PropertyChangedExtendedEventArgs e)
        {
            base.OnPropertyChanged(e);
            switch (e.PropertyName)
            {
                case nameof(ShowOverlay) or nameof(OverlayShape):
                    IsOutlineVisible = ShowOverlay && OverlayShape != PathShape.None;
                    break;

                case nameof(UseQuantizer):
                case nameof(UseDithering):
                case nameof(SelectedQuantizer):
                    DitheringEnabled = UseQuantizer && UseDithering;
                    SetVisibilities();
                    break;

                case nameof(OutlineColorText):
                    try
                    {
                        if (new ColorTypeConverter().ConvertFromInvariantString(OutlineColorText) is Color outlineColor)
                        {
                            OutlineColor = outlineColor;
                            OutlineColorBrush = new SolidColorBrush(outlineColor);
                        }
                    }
                    catch (InvalidOperationException)
                    {
                    }

                    break;

                case nameof(BackColorText):
                    try
                    {
                        if (new ColorTypeConverter().ConvertFromInvariantString(BackColorText) is Color backColor)
                        {
                            BackColor = backColor;
                            BackColorBrush = new SolidColorBrush(backColor);
                        }
                    }
                    catch (InvalidOperationException)
                    {
                    }

                    break;
            }

            if (affectsDisplayImage.Contains(e.PropertyName))
                await GenerateDisplayImage(Configuration.Capture(this));
        }

        [SuppressMessage("ReSharper", "AsyncVoidMethod", Justification = "Dispose pattern")]
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

        // NOTE: Do not be afraid of the length of this method. It demonstrates many configurable options and multiple use cases - see the a.) and b.) paths first.
        // The caller method is async void, so basically fire-and-forget. To prevent parallel generate sessions we store the current task
        // in the generateResultTask field, which can be awaited after a cancellation before starting to generate a new result.
        private async Task GenerateDisplayImage(Configuration cfg)
        {
            baseImage ??= SKBitmap.Decode(ImageResources.Information256);

            // Using a while instead of an if, because the awaits make this method reentrant, and a continuation can be spawn after any await at any time.
            // Therefore, it is possible that though we cleared generateResultTask in WaitForPendingGenerate it is not null upon starting the continuation.
            while (generateResultTask != null)
            {
                CancelRunningGenerate();
                await WaitForPendingGenerate();
            }

            // Using a manually completable task for the generateResultTask field. If this method had just one awaitable task we could simply assign that to the field.
            TaskCompletionSource<bool>? generateTaskCompletion = null;
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
                generateTaskCompletion = new TaskCompletionSource<bool>();
                CancellationTokenSource tokenSource = cancelGeneratingPreview = new CancellationTokenSource();
                CancellationToken token = tokenSource.Token;
                generateResultTask = generateTaskCompletion.Task;

                var asyncConfig = new TaskConfig { CancellationToken = token, ThrowIfCanceled = false };

                // NOTE: This GetReadableBitmapData is a local implementation using purely the KGySoft.Drawing.Core package.
                //       For complete SkiaSharp support with all possible pixel formats the example at https://github.com/koszeggy/KGySoft.Drawing/tree/master/Examples/SkiaSharp.Maui
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

                // b.2.) Drawing the overlay.
                overlayImageBitmapData ??= BitmapDataHelper.GenerateAlphaGradient(baseImageBitmapData.Size);
                Path? path = PathFactory.GetPath(new Rectangle(0, 0, overlayImageBitmapData.Width, overlayImageBitmapData.Height), cfg.OverlayShape, cfg.OutlineWidth);

                if (path == null)
                {
                    // When no shape is specified, we just draw the overlay bitmap into the target rectangle.
                    await overlayImageBitmapData.DrawIntoAsync(blendedResult, asyncConfig: asyncConfig);
                }
                else
                {
                    // When a shape is specified, we use the overlay bitmap data as a texture on a brush.
                    var options = cfg.DrawingOptions;
                    var brush = Brush.CreateTexture(overlayImageBitmapData, TextureMapMode.Center);
                    await blendedResult.FillPathAsync(brush, path, options, asyncConfig);

                    if (cfg.OutlineWidth > 0 && !token.IsCancellationRequested)
                    {
                        var pen = new Pen(cfg.OutlineColor, cfg.OutlineWidth) { LineJoin = LineJoinStyle.Round };
                        await blendedResult.DrawPathAsync(pen, path, options, asyncConfig);
                    }
                }

                if (token.IsCancellationRequested)
                    return;

                // b.3.) Converting back to a Skia bitmap using the desired quantizer and ditherer
                result = await blendedResult.ToSKBitmapAsync(quantizer, ditherer, asyncConfig);
            }
            finally
            {
                generateTaskCompletion?.SetResult(default);
                syncRoot.Release();
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
            IsOutlineVisible = ShowOverlay && OverlayShape != PathShape.None;
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
