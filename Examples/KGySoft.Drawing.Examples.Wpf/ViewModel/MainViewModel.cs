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
using KGySoft.CoreLibraries;
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
    internal class MainViewModel : ValidatingObjectBase
    {
        #region ProgressUpdater class

        /// <summary>
        /// Using this class to update progress from a dispatcher timer on the UI thread.
        /// Alternatively, our ViewModel could also implement IAsyncProgress and use Dispatcher.Invoke to set the ProgressValue
        /// on every update but that would be quite ineffective.
        /// </summary>
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

        #region Enumerations

        public enum Ditherer
        {
            Ordered,
            ErrorDiffusion,
            RandomNoise,
            InterleavedGradientNoise
        }

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

        private readonly ProgressUpdater progressUpdater;

        private BitmapSource? sourceBitmap;
        private string? imageFileError;
        private BitmapSource? overlayBitmap;
        private string? overlayFileError;
        private CancellationTokenSource? cancelGeneratingPreview;
        private Task? generateResultTask;

        #endregion

        #endregion

        #region Properties

        public PixelFormat[] PixelFormats { get; } = typeof(PixelFormats).GetProperties()
            .Where(p => p.Name != nameof(System.Windows.Media.PixelFormats.Default))
            .Select(p => (PixelFormat)p.GetValue(null, null)!)
            .ToArray();

        public Ditherer[] Ditherers { get; } = Enum<Ditherer>.GetValues();
        public string? ImageFile { get => Get<string?>(); set => Set(value); }
        public string? OverlayFile { get => Get<string?>(); set => Set(value); }
        public bool ShowOverlay { get => Get<bool>(); set => Set(value); }
        public PixelFormat SelectedFormat { get => Get(System.Windows.Media.PixelFormats.Indexed8); set => Set(value); }
        public string BackColorText { get => Get("Silver"); set => Set(value); }
        public Color BackColor { get => Get(Colors.Silver); set => Set(value); }
        public Brush? BackColorBrush { get => Get<Brush?>(() => new SolidColorBrush(BackColor)); set => Set(value); }
        public byte AlphaThreshold { get => Get<byte>(128); set => Set(value); }
        public bool OptimizePalette { get => Get<bool>(); set => Set(value); }
        public bool OptimizePaletteEnabled { get => Get(true); set => Set(value); }
        public bool UseDithering { get => Get<bool>(); set => Set(value); }
        public Ditherer SelectedDitherer { get => Get<Ditherer>(); set => Set(value); }
        public ImageSource? DisplayImage { get => Get<ImageSource?>(); set => Set(value); }
        public string? ProgressText { get => Get<string?>(); set => Set(value); }
        public Visibility ProgressVisibility { get => Get(Visibility.Hidden); set => Set(value); }
        public bool IsProgressIndeterminate { get => Get<bool>(); set => Set(value); }
        public int ProgressMaxValue { get => Get<int>(); set => Set(value); }
        public int ProgressValue { get => Get<int>(); set => Set(value); }

        #endregion

        #region Constructors

        public MainViewModel()
        {
            progressUpdater = new ProgressUpdater(this);
            ImageFile = @"..\..\..\..\..\Help\Images\Shield256.png";
            OverlayFile = @"..\..\..\..\..\Help\Images\AlphaGradient.png";
        }

        #endregion

        #region Methods

        #region Protected Methods

        protected override ValidationResultsCollection DoValidation()
        {
            var result = new ValidationResultsCollection();
            if (String.IsNullOrEmpty(ImageFile) || !File.Exists(ImageFile))
                result.AddError(nameof(ImageFile), "The specified file does not exist");
            else if (imageFileError != null)
                result.AddError(nameof(ImageFile), imageFileError);
            if (ShowOverlay && String.IsNullOrEmpty(OverlayFile) || !File.Exists(OverlayFile))
                result.AddError(nameof(OverlayFile), "The specified file does not exist");
            else if (overlayFileError != null)
                result.AddError(nameof(OverlayFile), overlayFileError);
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
                    imageFileError = null;
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
                        imageFileError = ex.Message;
                    }

                    break;

                case nameof(OverlayFile):
                    overlayFileError = null;
                    file = e.NewValue as string;
                    if (String.IsNullOrWhiteSpace(file) || !File.Exists(file))
                        break;

                    try
                    {
                        overlayBitmap = new BitmapImage(new Uri(file, UriKind.RelativeOrAbsolute));
                    }
                    catch (Exception ex)
                    {
                        overlayBitmap = null;
                        overlayFileError = ex.Message;
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

            if (affectsPreview.Contains(e.PropertyName!))
                await GenerateResult();
        }

        protected override void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;
            if (disposing)
                progressUpdater.Dispose();
            base.Dispose(disposing);
        }

        #endregion

        #region Private Methods

        // The caller method is async void, so basically fire-and-forget. To prevent parallel generate sessions we store the actual task
        // in the generatePreviewTask field, which can be awaited after a cancellation before starting to generate a new result.
        private async Task GenerateResult()
        {
            if (!IsValid)
            {
                DisplayImage = null;
                return;
            }

            // The awaits make this method reentrant, and a continuation can be spawn after any await at any time.
            // Therefore it is possible that despite of the clearing generatePreviewTask in WaitForPendingGenerate it not null upon starting the continuation.
            while (generateResultTask != null)
            {
                CancelRunningGenerate();
                await WaitForPendingGenerate();
            }

            BitmapSource? bmpSource = sourceBitmap;
            BitmapSource? bmpOverlay = overlayBitmap;
            if (IsDisposed || bmpSource == null)
                return;

            progressUpdater.Start();
            PixelFormat selectedFormat = SelectedFormat;
            CancellationTokenSource tokenSource = cancelGeneratingPreview = new CancellationTokenSource();
            CancellationToken token = tokenSource.Token;

            // Ditherer: feel free to try the other properties in OrderedDitherer and ErrorDiffusionDitherer
            IDitherer? ditherer = !UseDithering
                ? null
                : SelectedDitherer switch
                {
                    Ditherer.Ordered => OrderedDitherer.Bayer8x8,
                    Ditherer.ErrorDiffusion => ErrorDiffusionDitherer.FloydSteinberg,
                    Ditherer.RandomNoise => new RandomNoiseDitherer(),
                    Ditherer.InterleavedGradientNoise => new InterleavedGradientNoiseDitherer(),
                    _ => null
                };

            // Quantizer: effectively using only when palette optimization is requested. Otherwise, if ditherer is set, then picking a quantizer that matches the selected pixel format.
            IQuantizer? quantizer = OptimizePalette && selectedFormat.IsIndexed()
                ? OptimizedPaletteQuantizer.Wu(1 << selectedFormat.BitsPerPixel, BackColor.ToDrawingColor(), AlphaThreshold)
                : ditherer == null
                    ? null
                    : selectedFormat.GetMatchingQuantizer(BackColor, AlphaThreshold);

            WriteableBitmap? result = null;
            var asyncConfig = new TaskConfig { CancellationToken = token, ThrowIfCanceled = false, Progress = progressUpdater };

            try
            {
                // No overlay: ConvertPixelFormat does everything in a single step for us.
                if (!ShowOverlay || bmpOverlay == null)
                {
                    Task<WriteableBitmap?> taskConvert = quantizer == null && ditherer == null
                        ? bmpSource.ConvertPixelFormatAsync(selectedFormat, BackColor, AlphaThreshold, asyncConfig) // without quantizing and dithering
                        : bmpSource.ConvertPixelFormatAsync(selectedFormat, quantizer, ditherer, asyncConfig); // with quantizing and dithering
                    generateResultTask = taskConvert;
                    result = await taskConvert;
                    return;
                }

                // There is an image overlay: demonstrating how to work with IReadWriteBitmapData in WPF
                using (IReadWriteBitmapData resultBitmapData = BitmapDataFactory.CreateBitmapData(new Size(bmpSource.PixelWidth, bmpSource.PixelHeight), KnownPixelFormat.Format32bppPArgb, BackColor.ToColor32(), AlphaThreshold))
                {
                    // 1.) Drawing the source bitmap first. GetReadableBitmapData can be used for any BitmapSource.
                    using (IReadableBitmapData bitmapDataSource = bmpSource.GetReadableBitmapData())
                        await (generateResultTask = bitmapDataSource.CopyToAsync(resultBitmapData, asyncConfig: asyncConfig));

                    if (token.IsCancellationRequested)
                        return;

                    // 2.) Drawing the overlay. This time using DrawInto instead of CopyTo, which uses blending and supports resizing.
                    using (IReadableBitmapData bitmapDataOverlay = bmpOverlay.GetReadableBitmapData())
                    {
                        // Shrinking overlay if larger than the actual image.
                        var targetRectangle = new Rectangle(resultBitmapData.Width / 2 - bitmapDataOverlay.Width / 2,
                            resultBitmapData.Height / 2 - bitmapDataOverlay.Height / 2, bitmapDataOverlay.Width, bitmapDataOverlay.Height);
                        if (targetRectangle.X < 0)
                        {
                            targetRectangle.X = 0;
                            targetRectangle.Width = resultBitmapData.Width;
                        }

                        if (targetRectangle.Y < 0)
                        {
                            targetRectangle.Y = 0;
                            targetRectangle.Height = resultBitmapData.Height;
                        }

                        await (generateResultTask = bitmapDataOverlay.DrawIntoAsync(resultBitmapData, new Rectangle(Point.Empty, bitmapDataOverlay.Size),
                            targetRectangle, asyncConfig: asyncConfig));
                    }

                    if (token.IsCancellationRequested)
                        return;

                    // 3.) Converting to WriteableBitmap of the desired pixel format
                    Task<WriteableBitmap?> taskConvert = resultBitmapData.ToWriteableBitmapAsync(selectedFormat, quantizer, ditherer, asyncConfig);
                    generateResultTask = taskConvert;
                    result = await taskConvert;
                }
            }
            finally
            {
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
