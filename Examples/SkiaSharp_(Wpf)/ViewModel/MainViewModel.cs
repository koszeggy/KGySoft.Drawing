#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: MainViewModel.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2023 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using KGySoft.Drawing.SkiaSharp;

using SkiaSharp;
using SkiaSharp.Views.WPF;

#region Used Namespaces

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

using KGySoft.ComponentModel;
using KGySoft.Drawing.Examples.Shared.Interfaces;
using KGySoft.Drawing.Examples.Shared.Model;
using KGySoft.Drawing.Imaging;
using KGySoft.Threading;

#endregion

#region Used Aliases

using Point = System.Drawing.Point;
using Rectangle = System.Drawing.Rectangle;
using Size = System.Drawing.Size;
using KGySoft.CoreLibraries;
using KGySoft.Drawing.Examples.Shared;

#endregion

#endregion

namespace KGySoft.Drawing.Examples.SkiaSharp.Wpf.ViewModel
{
    internal class MainViewModel : ValidatingObjectBase
    {
        #region Nested Types
        
        #region ProgressUpdater class

        // Using this class to update progress from a dispatcher timer on the UI thread.
        // Alternatively, our ViewModel could also implement IAsyncProgress and use Dispatcher.Invoke to set the ProgressValue
        // on every update but that would be quite ineffective.
        private sealed class ProgressUpdater : IAsyncProgress, IDisposable
        {
            #region Fields

            private readonly MainViewModel owner;
            private readonly DispatcherTimer timer;

            private (string? Op, int Max, int Value) current;

            #endregion

            #region Constructors

            internal ProgressUpdater(MainViewModel owner)
            {
                this.owner = owner;
                timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(30) };
                timer.Tick += Timer_Tick;
            }

            #endregion

            #region Methods

            #region Public Methods

            public void Report<T>(AsyncProgress<T> progress)
            {
                lock (timer)
                    current = (SeparateWords(progress.OperationType?.ToString()), progress.MaximumValue, progress.CurrentValue);
            }

            public void New<T>(T operationType, int maximumValue = 0, int currentValue = 0)
                => Report(new AsyncProgress<T>(operationType, maximumValue, currentValue));

            public void Increment()
            {
                lock (timer)
                    current.Value++;
            }

            public void SetProgressValue(int value)
            {
                lock (timer)
                    current.Value = value;
            }

            public void Complete()
            {
                lock (timer)
                    current.Value = current.Max;
            }

            public void Dispose()
            {
                timer.Stop();
                timer.Tick -= Timer_Tick;
            }

            #endregion

            #region Internal Methods

            internal void Start()
            {
                lock (timer)
                    current = default;

                timer.Start();
                owner.ProgressVisibility = Visibility.Visible;
                owner.IsProgressIndeterminate = true;
            }

            internal void Stop()
            {
                timer.Stop();
                owner.ProgressVisibility = Visibility.Hidden;
            }

            #endregion

            #region Private Methods

            private static string? SeparateWords(string? s)
            {
                if (s == null)
                    return null;
                var result = new StringBuilder(s);
                for (int i = s.Length - 1; i > 0; i--)
                {
                    if (Char.IsUpper(s[i]))
                        result.Insert(i, ' ');
                }

                return s.Length == result.Length ? s : result.ToString();
            }

            #endregion

            #region Event Handlers

            private void Timer_Tick(object? sender, EventArgs e)
            {
                lock (timer)
                {
                    owner.ProgressText = current.Op;
                    owner.IsProgressIndeterminate = current.Max == 0;
                    owner.ProgressMaxValue = current.Max;
                    owner.ProgressValue = current.Value;
                }
            }

            #endregion

            #endregion
        }

        #endregion

        #region Configuration record

        private record Configuration : IQuantizerSettings, IDithererSettings // as a record so equality check compares the properties
        {
            #region Properties

            #region Public Properties

            public SKBitmap? Source { get; private init; }
            public SKBitmap? Overlay { get; private init; }
            public bool ShowOverlay { get; private init; }
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
                Source = viewModel.sourceBitmap,
                Overlay = viewModel.overlayBitmap,
                ShowOverlay = viewModel.ShowOverlay,
                ColorType = viewModel.SelectedColorType,
                AlphaType = viewModel.SelectedAlphaType,
                ColorSpace = viewModel.SelectedColorSpace,
                ForceLinearWorkingColorSpace = viewModel.ForceLinearWorkingColorSpace,
                UseQuantizer = viewModel.UseQuantizer,
                SelectedQuantizer = viewModel.SelectedQuantizer,
                BackColor = viewModel.BackColor.ToSKColor().ToColor32(),
                AlphaThreshold = (byte)viewModel.AlphaThreshold,
                WhiteThreshold = (byte)viewModel.WhiteThreshold,
                PaletteSize = viewModel.PaletteSize,
                UseDithering = viewModel.UseDithering,
                SelectedDitherer = viewModel.SelectedDitherer,
            };

            #endregion
        }

        #endregion

        #endregion

        #region Fields

        #region Static Fields

        private static readonly HashSet<string> affectsPreview = new()
        {
            nameof(ImageFile),
            nameof(OverlayFile),
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
        private readonly ProgressUpdater progressUpdater;
        private readonly bool isInitializing;

        private SKBitmap? sourceBitmap;
        private SKBitmap? overlayBitmap;
        private CancellationTokenSource? cancelGeneratingPreview;
        private Task? generateResultTask;
        private IReadableBitmapData? cachedOverlay;

        #endregion

        #endregion

        #region Properties

        #region Public Properties

        public SKColorType[] ColorTypes { get; } = Enum<SKColorType>.GetValues()
            .Where(f => f != SKColorType.Unknown)
            .ToArray();

        public SKAlphaType[] AlphaTypes { get; } = Enum<SKAlphaType>.GetValues()
            .Where(f => f != SKAlphaType.Unknown)
            .ToArray();

        public WorkingColorSpace[] ColorSpaces { get; } = { WorkingColorSpace.Srgb, WorkingColorSpace.Linear };

        public string? ImageFile { get => Get<string?>(); set => Set(value); }
        public string? OverlayFile { get => Get<string?>(); set => Set(value); }
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
        public Visibility AlphaThresholdVisibility { get => Get<Visibility>(); set => Set(value); }
        public int AlphaThreshold { get => Get(128); set => Set(value); }
        public Visibility WhiteThresholdVisibility { get => Get<Visibility>(); set => Set(value); }
        public int WhiteThreshold { get => Get(128); set => Set(value); }
        public Visibility MaxColorsVisibility { get => Get<Visibility>(); set => Set(value); }
        public int PaletteSize { get => Get(256); set => Set(value); }
        public bool UseDithering { get => Get<bool>(); set => Set(value); }
        public DithererDescriptor[] Ditherers => DithererDescriptor.Ditherers;
        public DithererDescriptor SelectedDitherer { get => Get(Ditherers[0]); set => Set(value); }
        public SKBitmap? DisplayImageBitmap { get => Get<SKBitmap?>(); set => Set(value); }
        public string? ProgressText { get => Get<string?>(); set => Set(value); }
        public Visibility ProgressVisibility { get => Get(Visibility.Hidden); set => Set(value); }
        public bool IsProgressIndeterminate { get => Get<bool>(); set => Set(value); }
        public int ProgressMaxValue { get => Get<int>(); set => Set(value); }
        public int ProgressValue { get => Get<int>(); set => Set(value); }

        #endregion

        #region Private Properties

        private string? ImageFileError { get => Get<string>(); set => Set(value); }
        private string? OverlayFileError { get => Get<string>(); set => Set(value); }

        private IReadableBitmapData CachedOverlay
        {
            get
            {
                if (cachedOverlay != null)
                    return cachedOverlay;

                if (sourceBitmap == null || overlayBitmap == null)
                    throw new InvalidOperationException("Source and overlay are not expected to be null here");

                IReadableBitmapData overlayBitmapData = overlayBitmap.GetReadableBitmapData();
                if (overlayBitmapData.Width <= sourceBitmap.Width && overlayBitmapData.Height <= sourceBitmap.Height)
                    return cachedOverlay = overlayBitmapData;

                // Shrinking overlay if larger than the actual image.
                IReadWriteBitmapData resizedOverlay = BitmapDataFactory.CreateBitmapData(new Size(Math.Min(sourceBitmap.Height, overlayBitmapData.Width), Math.Min(sourceBitmap.Height, overlayBitmapData.Height)), KnownPixelFormat.Format32bppPArgb);
                overlayBitmapData.DrawInto(resizedOverlay, new Rectangle(Point.Empty, resizedOverlay.Size));
                overlayBitmapData.Dispose();
                return cachedOverlay = resizedOverlay;
            }
        }

        #endregion

        #endregion

        #region Constructors

        public MainViewModel()
        {
            isInitializing = true;
            progressUpdater = new ProgressUpdater(this);
            ImageFile = @"..\..\..\..\..\Help\Images\Information256.png";
            OverlayFile = @"..\..\..\..\..\Help\Images\AlphaGradient.png";
            SetEnabledAndVisibilities();
            isInitializing = false;
            var _ = GenerateResult(Configuration.Capture(this));
        }

        #endregion

        #region Methods

        #region Protected Methods

        protected override ValidationResultsCollection DoValidation()
        {
            // Validating properties. To display also Warning and Info validation levels see the demo project at https://github.com/koszeggy/KGySoft.ComponentModelDemo
            var result = new ValidationResultsCollection();
            if (String.IsNullOrEmpty(ImageFile) || !File.Exists(ImageFile))
                result.AddError(nameof(ImageFile), "The specified file does not exist");
            else if (ImageFileError != null)
                result.AddError(nameof(ImageFile), ImageFileError);
            if (ShowOverlay && (String.IsNullOrEmpty(OverlayFile) || !File.Exists(OverlayFile)))
                result.AddError(nameof(OverlayFile), "The specified file does not exist");
            else if (OverlayFileError != null)
                result.AddError(nameof(OverlayFile), OverlayFileError);
            try
            {
                ColorConverter.ConvertFromString(BackColorText);
            }
            catch (Exception e)
            {
                result.AddError(nameof(BackColorText), e.Message);
            }

            return result;
        }

        protected override async void OnPropertyChanged(PropertyChangedExtendedEventArgs e)
        {
            base.OnPropertyChanged(e);
            switch (e.PropertyName)
            {
                case nameof(ImageFile):
                    await CancelAndAwaitPendingGenerate();
                    cachedOverlay?.Dispose();
                    cachedOverlay = null;
                    ImageFileError = null;
                    string? file = e.NewValue as string;
                    if (String.IsNullOrWhiteSpace(file) || !File.Exists(file))
                        break;

                    sourceBitmap = SKBitmap.Decode(file);
                    if (sourceBitmap == null)
                        ImageFileError = "Failed to load file as an SKBitmap";

                    break;

                case nameof(OverlayFile):
                    await CancelAndAwaitPendingGenerate();
                    OverlayFileError = null;
                    file = e.NewValue as string;
                    cachedOverlay?.Dispose();
                    cachedOverlay = null;
                    if (String.IsNullOrWhiteSpace(file) || !File.Exists(file))
                        break;

                    overlayBitmap = SKBitmap.Decode(file);
                    if (overlayBitmap == null)
                        OverlayFileError = $"Failed to load file as an SKBitmap";

                    break;

                case nameof(UseQuantizer):
                case nameof(UseDithering):
                case nameof(SelectedQuantizer):
                case nameof(SelectedColorType):
                case nameof(SelectedAlphaType):
                    SetEnabledAndVisibilities();
                    break;

                case nameof(BackColorText):
                    try
                    {
                        BackColor = (Color)ColorConverter.ConvertFromString(BackColorText);
                        BackColorBrush = new SolidColorBrush(BackColor);
                    }
                    catch (FormatException)
                    {
                    }

                    break;

                case nameof(DisplayImageBitmap):
                    if (e.OldValue is IDisposable previousBitmap)
                        previousBitmap.Dispose();
                    break;
            }

            if (affectsPreview.Contains(e.PropertyName!))
                await GenerateResult(Configuration.Capture(this));
        }

        protected override async void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;
            if (disposing)
            {
                await CancelAndAwaitPendingGenerate();
                progressUpdater.Dispose();
                sourceBitmap?.Dispose();
                overlayBitmap?.Dispose();
                cachedOverlay?.Dispose();
                DisplayImageBitmap?.Dispose();
                cancelGeneratingPreview?.Dispose();
                syncRoot.Dispose();
            }

            base.Dispose(disposing);
        }

        #endregion

        #region Private Methods

        // The caller method is async void, so basically fire-and-forget. To prevent parallel generate sessions we store the current task
        // in the generatePreviewTask field, which can be awaited after a cancellation before starting to generate a new result.
        private async Task GenerateResult(Configuration cfg)
        {
            if (isInitializing || IsDisposed)
                return;

            if (!IsValid)
            {
                DisplayImageBitmap = null;
                return;
            }

            // The awaits make this method reentrant, and a continuation can be spawn after any await at any time.
            // Therefore it is possible that despite of clearing generatePreviewTask in WaitForPendingGenerate it is not null upon starting the continuation.
            while (generateResultTask != null)
                await CancelAndAwaitPendingGenerate();

            // Using a manually completable task for the generateResultTask field. If this method had just one awaitable task we could simply assign that to the field.
            TaskCompletionSource? generateTaskCompletion = null;
            CancellationToken token = default;
            SKBitmap? result = null;

            // This is essentially a lock. Achieved by a SemaphoreSlim because an actual lock cannot be used with awaits in the code.
            await syncRoot.WaitAsync();
            try
            {
                // lost race: returning if configuration has been changed by the time we entered the lock
                if (cfg != Configuration.Capture(this) || IsDisposed)
                    return;

                progressUpdater.Start();
                bool useQuantizer = cfg.UseQuantizer;
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
                token = tokenSource.Token;
                generateResultTask = generateTaskCompletion.Task;

                var asyncConfig = new TaskConfig { CancellationToken = token, ThrowIfCanceled = false, Progress = progressUpdater };

                // ===== a.) No overlay: ConvertPixelFormat does everything in a single step for us. =====
                if (!cfg.ShowOverlay || cfg.Overlay == null)
                {
                    result = await (quantizer == null && ditherer == null
                        ? cfg.Source.ConvertPixelFormatAsync(cfg.ColorType, cfg.AlphaType, cfg.ColorSpace, cfg.BackColor.ToSKColor(), cfg.AlphaThreshold, asyncConfig) // without quantizing and dithering
                        : cfg.Source.ConvertPixelFormatAsync(quantizer, ditherer, cfg.ColorType, cfg.AlphaType, cfg.ColorSpace, asyncConfig)); // with quantizing and/or dithering
                    return;
                }

                // ===== b.) There is an image overlay: demonstrating how to work directly with IReadWriteBitmapData in SkiaSharp =====

                // Creating the temp 32 bpp bitmap data to work with. Will be converted back to SKBitmap in the end.
                // The Format32bppPArgb format is optimized for alpha blending in the sRGB color space but if linear working color space is selected
                // it would just cause an unnecessary overhead. So for working in the linear color space we use a non-premultiplied format.
                using IReadWriteBitmapData resultBitmapData = BitmapDataFactory.CreateBitmapData(new Size(cfg.Source.Width, cfg.Source.Height),
                    workingColorSpace == WorkingColorSpace.Linear ? KnownPixelFormat.Format32bppArgb : KnownPixelFormat.Format32bppPArgb,
                    workingColorSpace, cfg.BackColor, cfg.AlphaThreshold);

                // b.1.) Drawing the source bitmap first. GetReadableBitmapData can be used for any SKBitmap with any actual pixel format.
                //       Note that we don't need to specify working color space here because CopyTo/DrawInto respects the target's color space.
                using (IReadableBitmapData baseImageBitmapData = cfg.Source.GetReadableBitmapData())
                    await baseImageBitmapData.CopyToAsync(resultBitmapData, asyncConfig: asyncConfig);

                if (token.IsCancellationRequested)
                    return;

                // b.2.) Drawing the overlay. This time using DrawInto instead of CopyTo, which supports alpha blending
                IReadableBitmapData overlayBitmapData = CachedOverlay;
                var targetRectangle = new Rectangle(resultBitmapData.Width / 2 - overlayBitmapData.Width / 2,
                    resultBitmapData.Height / 2 - overlayBitmapData.Height / 2, overlayBitmapData.Width, overlayBitmapData.Height);
                await overlayBitmapData.DrawIntoAsync(resultBitmapData, new Rectangle(Point.Empty, overlayBitmapData.Size),
                    targetRectangle, asyncConfig: asyncConfig);

                if (token.IsCancellationRequested)
                    return;

                // b.3.) Converting back to a Skia bitmap using the desired format, quantizer and ditherer
                result = await resultBitmapData.ToSKBitmapAsync(cfg.ColorType, cfg.AlphaType, cfg.ColorSpace, quantizer, ditherer, asyncConfig);
            }
            finally
            {
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

                generateTaskCompletion?.SetResult();
                syncRoot.Release();
                progressUpdater.Stop();
                if (result != null)
                    DisplayImageBitmap = result;
            }
        }

        private void SetEnabledAndVisibilities()
        {
            static Visibility ToVisibility(bool value) => value ? Visibility.Visible : Visibility.Collapsed;

            bool useQuantizer = UseQuantizer;
            bool useDithering = UseDithering;
            SKColorType colorType = SelectedColorType;
            QuantizerDescriptor quantizer = SelectedQuantizer;
            bool isOpaque = (SelectedAlphaType == SKAlphaType.Opaque && colorType is not (SKColorType.Alpha8 or SKColorType.Alpha16 or SKColorType.AlphaF16))
                || colorType is SKColorType.Bgr101010x or SKColorType.Gray8 or SKColorType.Rgb565 or SKColorType.Rgb888x or SKColorType.Rgb101010x
                    or SKColorType.Rg88 or SKColorType.Rg1616 or SKColorType.RgF16;
            IsBackColorEnabled = useQuantizer || isOpaque || useDithering;
            AlphaThresholdVisibility = ToVisibility(useQuantizer && quantizer.HasAlphaThreshold
                || !useQuantizer && useDithering && !isOpaque);
            WhiteThresholdVisibility = ToVisibility(useQuantizer && quantizer.HasWhiteThreshold);
            MaxColorsVisibility = ToVisibility(useQuantizer && quantizer.HasMaxColors);
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

        private async Task CancelAndAwaitPendingGenerate()
        {
            CancelRunningGenerate();
            await WaitForPendingGenerate();
        }

        #endregion

        #endregion
    }
}
