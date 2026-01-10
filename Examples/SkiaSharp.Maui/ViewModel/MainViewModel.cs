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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using KGySoft.ComponentModel;
using KGySoft.CoreLibraries;
using KGySoft.Drawing.Examples.Shared;
using KGySoft.Drawing.Examples.Shared.Enums;
using KGySoft.Drawing.Examples.Shared.Interfaces;
using KGySoft.Drawing.Examples.Shared.Model;
using KGySoft.Drawing.Examples.SkiaSharp.Maui.Extensions;
using KGySoft.Drawing.Imaging;
using KGySoft.Drawing.Shapes;
using KGySoft.Drawing.SkiaSharp;
using KGySoft.Threading;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;

using SkiaSharp;
using SkiaSharp.Views.Maui.Controls;

#endregion

#region Used Aliases

using Brush = KGySoft.Drawing.Shapes.Brush;
using Point = System.Drawing.Point;
using Rectangle = System.Drawing.Rectangle;

#endregion

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

            public bool ShowOverlay { get; private init; }
            internal PathShape OverlayShape { get; private init; }
            internal int OutlineWidth { get; private init; }
            internal Color32 OutlineColor { get; private init; }
            public SKColorType ColorType { get; private init; }
            public SKAlphaType AlphaType { get; private init; }
            public WorkingColorSpace ColorSpace { get; private init; }
            public bool ForceLinearWorkingColorSpace { get; private init; }
            public bool UseQuantizer { get; private init; }
            public QuantizerDescriptor? SelectedQuantizer { get; private init; }
            public Color32 BackColor { get; private init; }
            public byte AlphaThreshold { get; private init; }
            public byte WhiteThreshold { get; private init; }
            public int PaletteSize { get; private init; }
            public bool UseDithering { get; private init; }
            public DithererDescriptor? SelectedDitherer { get; private init; }

            // Using Half pixel offset for odd pen with, and None for even width to avoid blurry lines. See more details at https://docs.kgysoft.net/drawing/html/P_KGySoft_Drawing_Shapes_DrawingOptions_DrawPathPixelOffset.htm
            internal DrawingOptions DrawingOptions => new DrawingOptions { AntiAliasing = true, DrawPathPixelOffset = (OutlineWidth & 1) == 1 ? PixelOffset.Half : PixelOffset.None };

            #endregion

            #region Explicitly Implemented Interface Properties

            bool IQuantizerSettings.DirectMapping => false;
            WorkingColorSpace IQuantizerSettings.WorkingColorSpace => ColorSpace == WorkingColorSpace.Linear || ForceLinearWorkingColorSpace ? WorkingColorSpace.Linear : WorkingColorSpace.Srgb;
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
                OutlineColor = viewModel.OutlineColor.ToColor32(),
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
            nameof(OverlayShape),
            nameof(OutlineWidth),
            nameof(OutlineColor),
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
            nameof(SelectedDitherer)
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
        public PathShape[] OverlayShapes { get; } = Enum<PathShape>.GetValues();

        public bool ShowOverlay { get => Get<bool>(); set => Set(value); }
        public bool IsOutlineVisible { get => Get<bool>(); set => Set(value); }
        public PathShape OverlayShape { get => Get<PathShape>(); set => Set(value); }
        public int OutlineWidth { get => Get<int>(); set => Set(value); }
        public string OutlineColorText { get => Get("Red"); set => Set(value); }
        public Color OutlineColor { get => Get(Colors.Red); set => Set(value); }
        public Microsoft.Maui.Controls.Brush? OutlineColorBrush { get => Get<Microsoft.Maui.Controls.Brush?>(() => new SolidColorBrush(OutlineColor)); set => Set(value); }
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
        public Microsoft.Maui.Controls.Brush? BackColorBrush { get => Get<Microsoft.Maui.Controls.Brush?>(() => new SolidColorBrush(BackColor)); set => Set(value); }
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
            PathFactory.GetTextPathCallback = GetTextPath;
            SetEnabledAndVisibilities();
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
                case nameof(SelectedColorType):
                case nameof(SelectedAlphaType):
                    SetEnabledAndVisibilities();
                    break;

                case nameof(OutlineColorText):
                    if (Color.TryParse(OutlineColorText, out Color color))
                    {
                        OutlineColor = color;
                        OutlineColorBrush = new SolidColorBrush(color);
                    }

                    break;

                case nameof(BackColorText):
                    if (Color.TryParse(BackColorText, out color))
                    {
                        BackColor = color;
                        BackColorBrush = new SolidColorBrush(color);
                    }

                    break;
            }

            if (affectsDisplayImage.Contains(e.PropertyName!))
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
            // Note that Images are regular .resx based resources. Make sure their Build Action is None so MAUI's "resizetizer" will not throw a build error.
            baseImage ??= SKBitmap.Decode(Images.Information256);

            // Using a while instead of an if, because the awaits make this method reentrant, and a continuation can be spawn after any await at any time.
            // Therefore, it is possible that though we cleared generateResultTask in WaitForPendingGenerate it is not null upon starting the continuation.
            while (generateResultTask != null)
            {
                CancelRunningGenerate();
                await WaitForPendingGenerate();
            }

            // Using a manually completable task for the generateResultTask field. If this method had just one awaitable task we could simply assign that to the field.
            TaskCompletionSource? generateTaskCompletion = null;
            CancellationToken token = default;
            SKBitmap? result = null;
            var asyncConfig = new TaskConfig { ThrowIfCanceled = false };

            // ReSharper disable once MethodSupportsCancellation - ok but our token is not for this one (which is btw. default at this point)
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
                WorkingColorSpace workingColorSpace = cfg.ForceLinearWorkingColorSpace || pixelFormat.GetInfo().LinearGamma
                    ? WorkingColorSpace.Linear
                    : WorkingColorSpace.Srgb;

                // Picking a quantizer even without a selected quantizer if we want to force working in the linear color space or a ditherer is selected.
                IQuantizer? quantizer = useQuantizer ? cfg.SelectedQuantizer!.Create(cfg)
                    : ditherer == null && !cfg.ForceLinearWorkingColorSpace ? null
                    : pixelFormat.GetMatchingQuantizer(cfg.BackColor.ToSKColor(), ditherer == null ? (byte)0 : cfg.AlphaThreshold).ConfigureColorSpace(workingColorSpace);

                generateTaskCompletion = new TaskCompletionSource();
                CancellationTokenSource tokenSource = cancelGeneratingPreview = new CancellationTokenSource();
                asyncConfig.CancellationToken = token = tokenSource.Token;
                generateResultTask = generateTaskCompletion.Task;

                // ===== a.) No overlay: ConvertPixelFormat does everything in a single step for us. =====
                if (!showOverlay)
                {
                    result = await (quantizer == null && ditherer == null
                        ? baseImage.ConvertPixelFormatAsync(cfg.ColorType, cfg.AlphaType, cfg.ColorSpace, cfg.BackColor.ToSKColor(), cfg.AlphaThreshold, asyncConfig) // without quantizing and dithering
                        : baseImage.ConvertPixelFormatAsync(quantizer, ditherer, cfg.ColorType, cfg.AlphaType, cfg.ColorSpace, asyncConfig)); // with quantizing and/or dithering
                    return;
                }

                // ===== b.) There is an image overlay: demonstrating how to work directly with IReadWriteBitmapData in SkiaSharp =====

                // Creating the temp bitmap data to work with. Will be converted back to SKBitmap in the end.
                // The Format128bppPRgba format is optimized for alpha blending in the linear color space, whereas Format64bppPArgb in the sRGB color space.
                // Any other format with enough colors would be alright, though.
                using IReadWriteBitmapData resultBitmapData = BitmapDataFactory.CreateBitmapData(new System.Drawing.Size(baseImage.Width, baseImage.Height),
                    workingColorSpace == WorkingColorSpace.Linear ? KnownPixelFormat.Format128bppPRgba : KnownPixelFormat.Format64bppPArgb,
                    workingColorSpace, cfg.BackColor, cfg.AlphaThreshold);

                // b.1.) Drawing the source bitmap first. GetReadableBitmapData can be used for any SKBitmap with any actual pixel format.
                //       Note that we don't need to specify working color space here because CopyTo/DrawInto respects the target's color space.
                using (IReadableBitmapData baseImageBitmapData = baseImage.GetReadableBitmapData())
                    await baseImageBitmapData.CopyToAsync(resultBitmapData, asyncConfig: asyncConfig);

                if (token.IsCancellationRequested)
                    return;

                // b.2.) Drawing the overlay.
                overlayImageBitmapData ??= BitmapDataHelper.GenerateAlphaGradient(resultBitmapData.Size);
                Path? path = PathFactory.GetPath(new Rectangle(0, 0, overlayImageBitmapData.Width, overlayImageBitmapData.Height), cfg.OverlayShape, cfg.OutlineWidth);

                if (path == null)
                {
                    // When no shape is specified, we just draw the overlay bitmap into the target rectangle.
                    await overlayImageBitmapData.DrawIntoAsync(resultBitmapData, asyncConfig: asyncConfig);
                }
                else
                {
                    // When a shape is specified, we use the overlay bitmap data as a texture on a brush.
                    var options = cfg.DrawingOptions;
                    var brush = Brush.CreateTexture(overlayImageBitmapData, TextureMapMode.Center);
                    await resultBitmapData.FillPathAsync(brush, path, options, asyncConfig);

                    if (cfg.OutlineWidth > 0 && !token.IsCancellationRequested)
                    {
                        var pen = new Pen(cfg.OutlineColor, cfg.OutlineWidth) { LineJoin = LineJoinStyle.Round };
                        await resultBitmapData.DrawPathAsync(pen, path, options, asyncConfig);
                    }
                }

                if (token.IsCancellationRequested)
                    return;

                // b.3.) Converting back to a Skia bitmap using the desired format, quantizer and ditherer
                result = await resultBitmapData.ToSKBitmapAsync(cfg.ColorType, cfg.AlphaType, cfg.ColorSpace, quantizer, ditherer, asyncConfig);
            }
            finally
            {
                bool isWindowsOrAndroid = DeviceInfo.Current.Platform.In(DevicePlatform.WinUI, DevicePlatform.Android);

                // special handling for Alpha images: turning them opaque grayscale so they are visible both with dark and light theme
                if (result?.ColorType is SKColorType.Alpha8 or SKColorType.Alpha16 or SKColorType.AlphaF16 && !token.IsCancellationRequested)
                {
                    SKBitmap displayResult = new SKBitmap(result.Info
                        .WithColorType(isWindowsOrAndroid ? SKColorType.Gray8 : SKImageInfo.PlatformColorType)
                        .WithColorSpace(null));
                    
                    using (IReadableBitmapData src = result.GetReadableBitmapData())
                    using (IWritableBitmapData dst = displayResult.GetWritableBitmapData())
                    {
                        await src.CopyToAsync(dst, new Rectangle(Point.Empty, src.Size), Point.Empty,
                            PredefinedColorsQuantizer.FromCustomFunction(c => Color32.FromGray(c.A)), // the more opaque the more white
                            asyncConfig: asyncConfig);
                    }
                    
                    if (!token.IsCancellationRequested)
                    {
                        result.Dispose();
                        result = displayResult;
                    }
                }
                // BUG WORKAROUND: SKBitmapImageSource handles SKBitmap incorrectly under a lot of conditions, which are also platform dependent - https://github.com/mono/SkiaSharp/issues/2466
                // - Everywhere, including Windows: only sRGB color space is displayed correctly so results with linear actual color space must be converted to sRGB
                // - On Android: ColorType.Argb4444 is displayed incorrectly
                // - On Mac/iOS: Everything but Rgba8888/Premul works incorrectly. Actually applying the workaround for all non-Windows/Android systems just for sure.
                else if (result != null && (result.ColorSpace?.GammaIsLinear == true
                             || DeviceInfo.Current.Platform == DevicePlatform.Android && result.ColorType == SKColorType.Argb4444
                             || !isWindowsOrAndroid && (result.ColorType != SKImageInfo.PlatformColorType || result.AlphaType == SKAlphaType.Unpremul)))
                {
                    SKBitmap displayResult = new SKBitmap(result.Info
                        .WithColorType(SKImageInfo.PlatformColorType)
                        .WithAlphaType(SKAlphaType.Premul)
                        .WithColorSpace(SKColorSpace.CreateSrgb()));

                    using (IReadableBitmapData src = result.GetReadableBitmapData())
                    using (IWritableBitmapData dst = displayResult.GetWritableBitmapData())
                    {
                        await src.CopyToAsync(dst, new Rectangle(Point.Empty, src.Size), Point.Empty,
                            asyncConfig: asyncConfig);
                    }

                    if (!token.IsCancellationRequested)
                    {
                        result.Dispose();
                        result = displayResult;
                    }
                }

                generateTaskCompletion?.SetResult();
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

        // This project uses this custom, technology-specific logic when "Text" overlay shape is selected.
        // To draw text directly into an IReadWriteBitmapData, you can also use the DrawText/DrawTextOutline extensions.
        // At a lower level it does the same as we do here: gets the text as an SKPath, and converts it to a KGy SOFT Path.
        private static Path GetTextPath(Rectangle bounds)
        {
            Path result;
            using (var font = new SKFont(SKTypeface.FromFamilyName(SKTypeface.Default.FamilyName, SKFontStyle.Bold)))
            {
                // converting the two rows of text to Path instances
                using SKPath skiaPathFirstRow = font.GetTextPath("KGy");
                result = skiaPathFirstRow.ToPath();
                using SKPath skiaPathSecondRow = font.GetTextPath("SOFT");
                Path secondRow = skiaPathSecondRow.ToPath();

                // adding the 2nd row to result with center alignment
                var boundsRow1 = result.Bounds;
                var boundsRow2 = secondRow.Bounds;
                result.TransformTranslation(boundsRow1.Width / 2f - boundsRow2.Width / 2f, boundsRow1.Height);
                result.AddPath(secondRow, false);
                result.ResetTransformation();

                // moving the combined result to the origin so scaling will be alright
                result.TransformAdded(TransformationMatrix.CreateTranslation(-result.Bounds.Left, -result.Bounds.Y));
            }

            // scaling and centering the combined result in the required bounds
            Rectangle pathBounds = result.Bounds;
            float ratio = (float)Math.Min(bounds.Width, bounds.Height) / Math.Max(pathBounds.Width, pathBounds.Height);
            result.TransformAdded(TransformationMatrix.CreateScale(ratio, ratio));
            pathBounds = result.Bounds;
            result.TransformAdded(TransformationMatrix.CreateTranslation(Math.Max(bounds.Width, pathBounds.Width) / 2f - Math.Min(bounds.Width, pathBounds.Width) / 2f - pathBounds.Left,
                Math.Max(bounds.Height, pathBounds.Height) / 2f - Math.Min(bounds.Height, pathBounds.Height) / 2f - pathBounds.Top));
            return result;
        }

        private void SetEnabledAndVisibilities()
        {
            bool useQuantizer = UseQuantizer;
            bool useDithering = UseDithering;
            IsOutlineVisible = ShowOverlay && OverlayShape != PathShape.None;
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
