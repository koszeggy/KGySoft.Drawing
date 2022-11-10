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
using System.Windows.Media.Imaging;
using System.Windows.Threading;

using KGySoft.ComponentModel;
using KGySoft.Drawing.Examples.Shared.Interfaces;
using KGySoft.Drawing.Examples.Shared.Model;
using KGySoft.Drawing.Imaging;
using KGySoft.Drawing.Wpf;
using KGySoft.Threading;

#endregion

#region Used Aliases

using Point = System.Drawing.Point;
using Rectangle = System.Drawing.Rectangle;
using Size = System.Drawing.Size;

#endregion

#endregion

namespace KGySoft.Drawing.Examples.Wpf.ViewModel
{
    internal class MainViewModel : ValidatingObjectBase, IDithererSettings
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

        private record Configuration // as a record so equality check compares the properties
        {
            #region Properties

            internal BitmapSource? Source { get; private init; }
            internal BitmapSource? Overlay { get; private init; }
            internal bool ShowOverlay { get; private init; }
            internal PixelFormat SelectedFormat { get; private init; }
            internal Color BackColor { get; private init; }
            internal byte AlphaThreshold { get; private init; }
            internal bool OptimizePalette { get; private init; }
            internal bool UseDithering { get; private init; }
            internal DithererDescriptor? SelectedDitherer { get; private init; }

            #endregion

            #region Methods

            internal static Configuration Capture(MainViewModel viewModel) => new Configuration
            {
                Source = viewModel.sourceBitmap,
                Overlay = viewModel.overlayBitmap,
                ShowOverlay = viewModel.ShowOverlay,
                SelectedFormat = viewModel.SelectedFormat,
                BackColor = viewModel.BackColor,
                AlphaThreshold = viewModel.AlphaThreshold,
                OptimizePalette = viewModel.OptimizePalette,
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
            nameof(SelectedFormat),
            nameof(BackColor),
            nameof(AlphaThreshold),
            nameof(OptimizePalette),
            nameof(UseDithering),
            nameof(SelectedDitherer),
        };

        #endregion

        #region Instance Fields

        private readonly SemaphoreSlim syncRoot = new SemaphoreSlim(1, 1);
        private readonly ProgressUpdater progressUpdater;
        private readonly bool isInitializing;

        private BitmapSource? sourceBitmap;
        private BitmapSource? overlayBitmap;
        private CancellationTokenSource? cancelGeneratingPreview;
        private Task? generateResultTask;
        private IReadableBitmapData? cachedOverlay;

        #endregion

        #endregion

        #region Properties
        
        #region Public Properties

        public PixelFormat[] PixelFormats { get; } = typeof(PixelFormats).GetProperties()
            .Where(p => p.Name != nameof(System.Windows.Media.PixelFormats.Default))
            .Select(p => (PixelFormat)p.GetValue(null, null)!)
            .ToArray();

        public DithererDescriptor[] Ditherers { get; } = DithererDescriptor.Ditherers;
        public string? ImageFile { get => Get<string?>(); set => Set(value); }
        public string? OverlayFile { get => Get<string?>(); set => Set(value); }
        public bool ShowOverlay { get => Get<bool>(); set => Set(value); }
        public PixelFormat SelectedFormat { get => Get<PixelFormat>(); set => Set(value); }
        public string BackColorText { get => Get("Silver"); set => Set(value); }
        public Color BackColor { get => Get(Colors.Silver); set => Set(value); }
        public Brush? BackColorBrush { get => Get<Brush?>(() => new SolidColorBrush(BackColor)); set => Set(value); }
        public bool BackColorEnabled { get => Get<bool>(); set => Set(value); }
        public byte AlphaThreshold { get => Get<byte>(128); set => Set(value); }
        public bool AlphaThresholdEnabled { get => Get<bool>(); set => Set(value); }
        public bool OptimizePalette { get => Get<bool>(); set => Set(value); }
        public bool OptimizePaletteEnabled { get => Get(true); set => Set(value); }
        public bool UseDithering { get => Get<bool>(); set => Set(value); }
        public DithererDescriptor SelectedDitherer { get => Get(Ditherers[0]); set => Set(value); }
        public ImageSource? DisplayImage { get => Get<ImageSource?>(); set => Set(value); }
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
                if (overlayBitmapData.Width <= sourceBitmap.PixelWidth && overlayBitmapData.Height <= sourceBitmap.PixelHeight)
                    return cachedOverlay = overlayBitmapData;

                // Shrinking overlay if larger than the actual image.
                IReadWriteBitmapData resizedOverlay = BitmapDataFactory.CreateBitmapData(new Size(Math.Min(sourceBitmap.PixelHeight, overlayBitmapData.Width), Math.Min(sourceBitmap.PixelHeight, overlayBitmapData.Height)), KnownPixelFormat.Format32bppPArgb);
                overlayBitmapData.DrawInto(resizedOverlay, new Rectangle(Point.Empty, resizedOverlay.Size));
                overlayBitmapData.Dispose();
                return cachedOverlay = resizedOverlay;
            }
        }

        #endregion

        #region Explicitly Implemented Interface Properties

        float IDithererSettings.Strength => 0f;
        bool? IDithererSettings.ByBrightness => null;
        bool IDithererSettings.DoSerpentineProcessing => false;
        int? IDithererSettings.Seed => null;

        #endregion

        #endregion

        #region Constructors

        public MainViewModel()
        {
            isInitializing = true;
            progressUpdater = new ProgressUpdater(this);
            SelectedFormat = System.Windows.Media.PixelFormats.Indexed8;
            ImageFile = @"..\..\..\..\..\Help\Images\Information256.png";
            OverlayFile = @"..\..\..\..\..\Help\Images\AlphaGradient.png";
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
                    cachedOverlay?.Dispose();
                    cachedOverlay = null;
                    ImageFileError = null;
                    string? file = e.NewValue as string;
                    if (String.IsNullOrWhiteSpace(file) || !File.Exists(file))
                        break;

                    try
                    {
                        sourceBitmap = new BitmapImage(new Uri(file, UriKind.RelativeOrAbsolute));
                    }
                    catch (Exception ex)
                    {
                        sourceBitmap = null;
                        ImageFileError = $"Failed to load file as a BitmapImage: {ex.Message}";
                    }

                    break;

                case nameof(OverlayFile):
                    OverlayFileError = null;
                    file = e.NewValue as string;
                    cachedOverlay?.Dispose();
                    cachedOverlay = null;
                    if (String.IsNullOrWhiteSpace(file) || !File.Exists(file))
                        break;

                    try
                    {
                        overlayBitmap = new BitmapImage(new Uri(file, UriKind.RelativeOrAbsolute));
                    }
                    catch (Exception ex)
                    {
                        overlayBitmap = null;
                        OverlayFileError = $"Failed to load file as a BitmapImage: {ex.Message}";
                    }

                    break;

                case nameof(SelectedFormat):
                    OptimizePaletteEnabled = SelectedFormat.IsIndexed();
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
            }

            if (e.PropertyName is nameof(SelectedFormat) or nameof(UseDithering) or nameof(OptimizePalette))
            {
                PixelFormatInfo pixelFormatInfo = SelectedFormat.GetInfo();
                AlphaThresholdEnabled = pixelFormatInfo.HasAlpha && UseDithering || (pixelFormatInfo.Indexed && (OptimizePalette || pixelFormatInfo.BitsPerPixel == 8));
                BackColorEnabled = !pixelFormatInfo.HasAlpha || UseDithering;
            }

            if (affectsPreview.Contains(e.PropertyName!))
                await GenerateResult(Configuration.Capture(this));
        }

        protected override void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;
            if (disposing)
            {
                progressUpdater.Dispose();
                cachedOverlay?.Dispose();
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
            if (isInitializing || IsDisposed || cfg.Source == null)
                return;


            if (!IsValid)
            {
                DisplayImage = null;
                return;
            }

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

                progressUpdater.Start();
                generateTaskCompletion = new TaskCompletionSource();
                CancellationTokenSource tokenSource = cancelGeneratingPreview = new CancellationTokenSource();
                generateResultTask = generateTaskCompletion.Task;
                PixelFormat selectedFormat = cfg.SelectedFormat;
                CancellationToken token = tokenSource.Token;

                // Ditherer: feel free to try the other properties in OrderedDitherer and ErrorDiffusionDitherer
                IDitherer? ditherer = !cfg.UseDithering
                    ? null
                    : cfg.SelectedDitherer!.Create(this);

                // Quantizer: effectively using only when palette optimization is requested.
                // Otherwise, if ditherer is set, then picking a quantizer that matches the selected pixel format.
                IQuantizer? quantizer = cfg.OptimizePalette && selectedFormat.IsIndexed()
                    ? OptimizedPaletteQuantizer.Wu(1 << selectedFormat.BitsPerPixel, cfg.BackColor.ToColor32(), cfg.AlphaThreshold)
                    : ditherer == null
                        ? null
                        : selectedFormat.GetMatchingQuantizer(cfg.BackColor, cfg.AlphaThreshold);

                var asyncConfig = new TaskConfig { CancellationToken = token, ThrowIfCanceled = false, Progress = progressUpdater };

                // ===== a.) No overlay: ConvertPixelFormat does everything in a single step for us. =====
                if (!cfg.ShowOverlay || cfg.Overlay == null)
                {
                    result = await (quantizer == null && ditherer == null
                        ? cfg.Source.ConvertPixelFormatAsync(selectedFormat, cfg.BackColor, cfg.AlphaThreshold, asyncConfig) // without quantizing and dithering
                        : cfg.Source.ConvertPixelFormatAsync(selectedFormat, quantizer, ditherer, asyncConfig)); // with quantizing and/or dithering
                    return;
                }

                // ===== b.) There is an image overlay: demonstrating how to work directly with IReadWriteBitmapData in WPF =====
                using IReadWriteBitmapData resultBitmapData = BitmapDataFactory.CreateBitmapData(new Size(cfg.Source.PixelWidth, cfg.Source.PixelHeight),
                    KnownPixelFormat.Format32bppPArgb, cfg.BackColor.ToColor32(), cfg.AlphaThreshold);
                
                // b.1.) Drawing the source bitmap first. GetReadableBitmapData can be used for any BitmapSource.
                using (IReadableBitmapData bmpSourceData = cfg.Source.GetReadableBitmapData())
                    await bmpSourceData.CopyToAsync(resultBitmapData, asyncConfig: asyncConfig);

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

                // b.3.) Converting to WriteableBitmap of the desired pixel format
                result = await resultBitmapData.ToWriteableBitmapAsync(selectedFormat, quantizer, ditherer, asyncConfig);
            }
            finally
            {
                generateTaskCompletion?.SetResult();
                syncRoot.Release();
                progressUpdater.Stop();
                if (result != null)
                    DisplayImage = result;
            }
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
