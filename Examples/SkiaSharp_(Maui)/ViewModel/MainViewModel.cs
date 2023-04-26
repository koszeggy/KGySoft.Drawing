#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: MainViewModel.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2022 - All Rights Reserved
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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using KGySoft.ComponentModel;
using KGySoft.Drawing.Examples.Shared;
using KGySoft.Drawing.Examples.Shared.Interfaces;
using KGySoft.Drawing.Examples.Shared.Model;
using KGySoft.Drawing.Imaging;
using KGySoft.Threading;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

using SkiaSharp;
using SkiaSharp.Views.Maui.Controls;
using KGySoft.CoreLibraries;
using KGySoft.Drawing.Examples.SkiaSharp.Maui.Extensions;
using KGySoft.Drawing.SkiaSharp;

#endregion

namespace KGySoft.Drawing.Examples.SkiaSharp.Maui.ViewModel
{
    internal class MainViewModel : ObservableObjectBase
    {
        #region Configuration record

        private sealed record Configuration : IQuantizerSettings, IDithererSettings // as a record so equality check compares the properties
        {
            #region Properties

            #region Public Properties

            public bool ShowOverlay { get; private set; }
            public SKColorType ColorType { get; private set; }
            public SKAlphaType AlphaType { get; private set; }
            public WorkingColorSpace ColorSpace { get; private set; }
            public bool ForceLinearWorkingColorSpace { get; private set; }
            public bool UseQuantizer { get; private set; }
            public QuantizerDescriptor? SelectedQuantizer { get; private set; }
            public Color32 BackColor { get; private set; }
            public byte AlphaThreshold { get; private set; }
            public byte WhiteThreshold { get; private set; }
            public int PaletteSize { get; private set; }
            public bool UseDithering { get; private set; }
            public DithererDescriptor? SelectedDitherer { get; private set; }

            #endregion

            #region Explicitly Implemented Interface Properties

            bool IQuantizerSettings.DirectMapping => false;
            WorkingColorSpace IQuantizerSettings.WorkingColorSpace => ColorSpace == WorkingColorSpace.Linear || ForceLinearWorkingColorSpace ? WorkingColorSpace.Linear : WorkingColorSpace.Srgb;
            byte? IQuantizerSettings.BitLevel => null;
            System.Drawing.Color IQuantizerSettings.BackColor => BackColor.ToColor();
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
                ColorType = viewModel.SelectedColorType,
                AlphaType = viewModel.SelectedAlphaType,
                ColorSpace = viewModel.SelectedColorSpace,
                ForceLinearWorkingColorSpace = viewModel.ForceLinearWorkingColorSpace,
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
            nameof(SelectedColorType),
            nameof(SelectedAlphaType),
            nameof(SelectedColorSpace),
            nameof(ForceLinearWorkingColorSpace),
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

        public SKColorType[] ColorTypes { get; } = Enum<SKColorType>.GetValues()
            .Where(f => f != SKColorType.Unknown)
            .ToArray();

        public SKAlphaType[] AlphaTypes { get; } = Enum<SKAlphaType>.GetValues()
            .Where(f => f != SKAlphaType.Unknown)
            .ToArray();

        public WorkingColorSpace[] ColorSpaces { get; } = { WorkingColorSpace.Srgb, WorkingColorSpace.Linear };

        public bool ShowOverlay { get => Get<bool>(); set => Set(value); }
        public SKColorType SelectedColorType { get => Get(SKColorType.Argb4444); set => Set(value); }
        public SKAlphaType SelectedAlphaType { get => Get(SKAlphaType.Unpremul); set => Set(value); }
        public WorkingColorSpace SelectedColorSpace { get => Get(WorkingColorSpace.Srgb); set => Set(value); }
        public bool ForceLinearWorkingColorSpace { get => Get<bool>(); set => Set(value); }
        public bool UseQuantizer { get => Get<bool>(); set => Set(value); }
        public QuantizerDescriptor[] Quantizers => QuantizerDescriptor.Quantizers;
        public QuantizerDescriptor SelectedQuantizer { get => Get(Quantizers[0]); set => Set(value); }
        public bool IsBackColorEnabled { get => Get<bool>(); set => Set(value); }
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
        public DithererDescriptor[] Ditherers => DithererDescriptor.Ditherers;
        public DithererDescriptor SelectedDitherer { get => Get(Ditherers[0]); set => Set(value); }
        public ImageSource? DisplayImage { get => Get<ImageSource?>(); set => Set(value); }

        #endregion

        #region Constructors

        public MainViewModel()
        {
            SetEnabledAndVisibilities();
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
                case nameof(SelectedColorType):
                case nameof(SelectedAlphaType):
                    SetEnabledAndVisibilities();
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
            TaskCompletionSource<bool>? generateTaskCompletion = null;
            CancellationToken token = default;
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
                IDitherer? ditherer = cfg.UseDithering ? cfg.SelectedDitherer!.Create(cfg) : null;
                var pixelFormat = new SKImageInfo
                {
                    ColorType = cfg.ColorType,
                    AlphaType = cfg.AlphaType,
                    ColorSpace = cfg.ColorSpace == WorkingColorSpace.Linear ? SKColorSpace.CreateSrgbLinear() : SKColorSpace.CreateSrgb()
                };

                // Working color space can be different from actual color space and can be specified for creating IBitmapData, Palette and IQuantizer instances.
                WorkingColorSpace workingColorSpace = cfg.ForceLinearWorkingColorSpace|| pixelFormat.GetInfo().LinearGamma
                    ? WorkingColorSpace.Linear
                    : WorkingColorSpace.Srgb;

                // Picking a quantizer even without a selected quantizer if we want to force working in the linear color space or a ditherer is selected.
                IQuantizer? quantizer = useQuantizer ? cfg.SelectedQuantizer!.Create(cfg)
                    : ditherer == null && !cfg.ForceLinearWorkingColorSpace ? null
                    : pixelFormat.GetMatchingQuantizer(cfg.BackColor.ToSKColor(), ditherer == null ? (byte)0 : cfg.AlphaThreshold).ConfigureColorSpace(workingColorSpace);

                generateTaskCompletion = new TaskCompletionSource<bool>();
                CancellationTokenSource tokenSource = cancelGeneratingPreview = new CancellationTokenSource();
                token = tokenSource.Token;
                generateResultTask = generateTaskCompletion.Task;

                var asyncConfig = new TaskConfig { CancellationToken = token, ThrowIfCanceled = false };

                // ===== a.) No overlay: ConvertPixelFormat does everything in a single step for us. =====
                if (!showOverlay)
                {
                    result = await (quantizer == null && ditherer == null
                        ? baseImage.ConvertPixelFormatAsync(cfg.ColorType, cfg.AlphaType, cfg.ColorSpace, cfg.BackColor.ToSKColor(), cfg.AlphaThreshold, asyncConfig) // without quantizing and dithering
                        : baseImage.ConvertPixelFormatAsync(quantizer, ditherer, cfg.ColorType, cfg.AlphaType, cfg.ColorSpace, asyncConfig)); // with quantizing and/or dithering
                    return;
                }

                // ===== b.) There is an image overlay: demonstrating how to work directly with IReadWriteBitmapData in SkiaSharp =====

                // Creating the temp 32 bpp bitmap data to work with. Will be converted back to SKBitmap in the end.
                // The Format32bppPArgb format is optimized for alpha blending in the sRGB color space but if linear working color space is selected
                // it would just cause an unnecessary overhead. So for working in the linear color space we use a non-premultiplied format.
                using IReadWriteBitmapData resultBitmapData = BitmapDataFactory.CreateBitmapData(new System.Drawing.Size(baseImage.Width, baseImage.Height),
                    workingColorSpace == WorkingColorSpace.Linear ? KnownPixelFormat.Format32bppArgb : KnownPixelFormat.Format32bppPArgb,
                    workingColorSpace, cfg.BackColor, cfg.AlphaThreshold);

                // b.1.) Drawing the source bitmap first. GetReadableBitmapData can be used for any SKBitmap with any actual pixel format.
                //       Note that we don't need to specify working color space here because CopyTo/DrawInto respects the target's color space.
                using (IReadableBitmapData baseImageBitmapData = baseImage.GetReadableBitmapData())
                    await baseImageBitmapData.CopyToAsync(resultBitmapData, asyncConfig: asyncConfig);

                if (token.IsCancellationRequested)
                    return;

                // b.2.) Drawing the overlay. DrawInto supports alpha blending
                overlayImageBitmapData ??= BitmapDataHelper.GenerateAlphaGradient(resultBitmapData.Size);
                await overlayImageBitmapData.DrawIntoAsync(resultBitmapData, asyncConfig: asyncConfig);

                if (token.IsCancellationRequested)
                    return;

                // b.3.) Converting back to a Skia bitmap using the desired format, quantizer and ditherer
                result = await resultBitmapData.ToSKBitmapAsync(cfg.ColorType, cfg.AlphaType, cfg.ColorSpace, quantizer, ditherer, asyncConfig);
            }
            finally
            {
                // special handling for Alpha images: turning them opaque grayscale so they are visible both with dark and light theme
                if (result?.ColorType is SKColorType.Alpha8 or SKColorType.Alpha16 or SKColorType.AlphaF16 && !token.IsCancellationRequested)
                {
                    SKBitmap displayResult = new SKBitmap(result.Info.WithColorType(SKColorType.Gray8).WithColorSpace(null));
                    using IReadableBitmapData src = result.GetReadableBitmapData();
                    using IWritableBitmapData dst = displayResult.GetWritableBitmapData();
                    await src.CopyToAsync(dst, new System.Drawing.Rectangle(System.Drawing.Point.Empty, src.Size), System.Drawing.Point.Empty,
                        PredefinedColorsQuantizer.FromCustomFunction(c => Color32.FromGray(c.A)),
                        asyncConfig: new TaskConfig(token) { ThrowIfCanceled = false });
                    if (!token.IsCancellationRequested)
                    {
                        result.Dispose();
                        result = displayResult;
                    }
                }

                // BUG WORKAROUND: SKBitmapImageSource handles linear color space incorrectly so copying the actual result into an sRGB bitmap
                //                 just to display it correctly. We could also use KGy SOFT's BitmapDataExtensions.CopyToAsync,
                //                 which could maybe even faster (and cancellable).
                //                 Using the 100% Skia solution just to prove that the generated linear result was correct.
                if (result?.ColorSpace?.GammaIsLinear == true && !token.IsCancellationRequested)
                {
                    SKBitmap displayResult = new SKBitmap(result.Info.WithColorSpace(SKColorSpace.CreateSrgb()));
                    using var canvas = new SKCanvas(displayResult);
                    canvas.DrawBitmap(result, SKPoint.Empty);
                    result.Dispose();
                    result = displayResult;
                }

                generateTaskCompletion?.SetResult(default);
                syncRoot.Release();

                if (result != null)
                {
                    if (token.IsCancellationRequested)
                        result.Dispose();
                    else
                    {
                        // To make SKBitmapImageSource work .UseSkiaSharp() must be added to MauiProgram.CreateMauiApp
                        SKBitmap? previousBitmap = displayImageBitmap;
                        DisplayImage = new SKBitmapImageSource { Bitmap = result };
                        previousBitmap?.Dispose();
                        displayImageBitmap = result;
                    }
                }
            }
        }

        private void SetEnabledAndVisibilities()
        {
            bool useQuantizer = UseQuantizer;
            bool useDithering = UseDithering;
            SKColorType colorType = SelectedColorType;
            QuantizerDescriptor quantizer = SelectedQuantizer;
            bool isOpaque = (SelectedAlphaType == SKAlphaType.Opaque && colorType is not (SKColorType.Alpha8 or SKColorType.Alpha16 or SKColorType.AlphaF16))
                || colorType is SKColorType.Bgr101010x or SKColorType.Gray8 or SKColorType.Rgb565 or SKColorType.Rgb888x or SKColorType.Rgb101010x
                    or SKColorType.Rg88 or SKColorType.Rg1616 or SKColorType.RgF16;
            IsBackColorEnabled = useQuantizer || isOpaque || useDithering;
            IsAlphaThresholdVisible = useQuantizer && quantizer.HasAlphaThreshold
                || !useQuantizer && useDithering && !isOpaque;
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
