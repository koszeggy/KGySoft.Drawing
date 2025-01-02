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

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using KGySoft.ComponentModel;
using KGySoft.CoreLibraries;
using KGySoft.Drawing.Examples.Shared.Interfaces;
using KGySoft.Drawing.Examples.Shared.Model;
using KGySoft.Drawing.Imaging;
using KGySoft.Threading;

#endregion

namespace KGySoft.Drawing.Examples.WinForms.ViewModel
{
    internal class MainViewModel : ValidatingObjectBase
    {
        #region Nested Types

        #region ProgressUpdater class

        // This class tracks every progress update without immediately updating the bound ViewModel properties,
        // which are updated only upon request by a timer running on the UI thread. This is much more effective than
        // invoking a synchronized update in the UI thread for every tiny progress change.
        private sealed class ProgressUpdater : IAsyncProgress
        {
            #region Fields

            private readonly MainViewModel owner;

            private (string? Op, int Max, int Value) current;

            #endregion

            #region Constructors

            internal ProgressUpdater(MainViewModel owner) => this.owner = owner;

            #endregion

            #region Methods

            #region Public Methods

            public void Report<T>(AsyncProgress<T> progress)
            {
                lock (this)
                    current = (SeparateWords(progress.OperationType?.ToString()), progress.MaximumValue, progress.CurrentValue);
            }

            public void New<T>(T operationType, int maximumValue = 0, int currentValue = 0)
                => Report(new AsyncProgress<T>(operationType, maximumValue, currentValue));

            public void Increment()
            {
                lock (this)
                    current.Value++;
            }

            public void SetProgressValue(int value)
            {
                lock (this)
                    current.Value = value;
            }

            public void Complete()
            {
                lock (this)
                    current.Value = current.Max;
            }

            #endregion

            #region Internal Methods

            internal void Start()
            {
                lock (this)
                    current = default;

                owner.IsProgressIndeterminate = true;
                owner.ProgressVisible = true;
            }

            internal void Stop() => owner.ProgressVisible = false;

            internal void UpdateProgress()
            {
                lock (this)
                {
                    owner.ProgressText = current.Op;
                    owner.IsProgressIndeterminate = current.Max == 0;
                    owner.ProgressMaxValue = current.Max;
                    owner.ProgressValue = current.Value;
                }
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

            #endregion
        }

        #endregion

        #region Configuration record

        private record Configuration : IDithererSettings // as a record so equality check compares the properties
        {
            #region Properties

            #region Internal Properties
            
            internal Bitmap? Source { get; private init; }
            internal Bitmap? Overlay { get; private init; }
            internal bool ShowOverlay { get; private init; }
            internal PixelFormat SelectedFormat { get; private init; }
            internal bool ForceLinearColorSpace { get; private init; }
            internal Color BackColor { get; private init; }
            internal byte AlphaThreshold { get; private init; }
            internal bool OptimizePalette { get; private init; }
            internal bool UseDithering { get; private init; }
            internal DithererDescriptor? SelectedDitherer { get; private init; }

            #endregion

            #region Explicitly Implemented Interface Properties

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
                SelectedFormat = viewModel.SelectedFormat,
                ForceLinearColorSpace = viewModel.ForceLinearColorSpace,
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
            nameof(ForceLinearColorSpace),
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

        private bool isViewApplied;
        private Bitmap? sourceBitmap;
        private Bitmap? overlayBitmap;
        private volatile CancellationTokenSource? cancelGeneratingPreview;
        private volatile Task? generateResultTask;
        private IReadableBitmapData? cachedOverlay;

        #endregion

        #endregion

        #region Properties

        #region Public Properties

        [SuppressMessage("ReSharper", "BitwiseOperatorOnEnumWithoutFlags", Justification = "PixelFormat should actually be a Flags enum")]
        public PixelFormat[] PixelFormats { get; } = Enum<PixelFormat>.GetValues()
            .Where(f => f != PixelFormat.Max && (f & PixelFormat.Max) != 0)
            .OrderBy(pf => pf & PixelFormat.Max)
            .ToArray();

        public ICommand UpdateProgressCommand { get => Get<ICommand>(() => new SimpleCommand(OnUpdateProgressCommand)); }

        public string? ImageFile { get => Get<string?>(); set => Set(value); }
        public string? OverlayFile { get => Get<string?>(); set => Set(value); }
        public bool ShowOverlay { get => Get<bool>(); set => Set(value); }
        public PixelFormat SelectedFormat { get => Get<PixelFormat>(); set => Set(value); }
        public bool ForceLinearColorSpace { get => Get<bool>(); set => Set(value); }
        public Color BackColor { get => Get(Color.Silver); set => Set(value); }
        public bool BackColorEnabled { get => Get<bool>(); set => Set(value); }
        public byte AlphaThreshold { get => Get<byte>(128); set => Set(value); }
        public bool AlphaThresholdEnabled { get => Get<bool>(); set => Set(value); }
        public bool OptimizePalette { get => Get<bool>(); set => Set(value); }
        public bool OptimizePaletteEnabled { get => Get(true); set => Set(value); }
        public bool UseDithering { get => Get<bool>(); set => Set(value); }
        public DithererDescriptor[] Ditherers { get; } = DithererDescriptor.Ditherers;
        public DithererDescriptor SelectedDitherer { get => Get(Ditherers[0]); set => Set(value); }
        public Bitmap? DisplayImage { get => Get<Bitmap?>(); set => Set(value); }
        public string? ProgressText { get => Get<string?>(); set => Set(value); }
        public bool ProgressVisible { get => Get<bool>(); set => Set(value); }
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
            progressUpdater = new ProgressUpdater(this);
            SelectedFormat = PixelFormat.Format8bppIndexed;
            bool isWindows = Environment.OSVersion.Platform == PlatformID.Win32NT;
            ImageFile =  isWindows ? @"..\..\..\..\..\Help\Images\Information256.png" : "../../../../../Help/Images/Information256.png";
            OverlayFile = isWindows ? @"..\..\..\..\..\Help\Images\AlphaGradient.png" : "../../../../../Help/Images/AlphaGradient.png";
        }

        #endregion

        #region Methods

        #region Internal Methods

        internal async Task ViewApplied()
        {
            isViewApplied = true;
            await GenerateResult(Configuration.Capture(this));
        }

        #endregion

        #region Protected Methods

        protected override ValidationResultsCollection DoValidation()
        {
            // Validating properties. To display also Warning and Info validation levels see the demo project at https://github.com/koszeggy/KGySoft.ComponentModelDemo
            var result = new ValidationResultsCollection();
            if (String.IsNullOrEmpty(ImageFile) || !File.Exists(ImageFile))
                result.AddError(nameof(ImageFile), "The specified file does not exist");
            else if (ImageFileError != null)
                result.AddError(nameof(ImageFile), ImageFileError);
            if (String.IsNullOrEmpty(OverlayFile) || !File.Exists(OverlayFile))
                result.AddError(nameof(OverlayFile), "The specified file does not exist");
            else if (OverlayFileError != null)
                result.AddError(nameof(OverlayFile), OverlayFileError);

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

                    try
                    {
                        var prevBitmap = sourceBitmap;
                        sourceBitmap = new Bitmap(file);
                        prevBitmap?.Dispose();
                    }
                    catch (Exception ex)
                    {
                        sourceBitmap = null;
                        ImageFileError = $"Failed to load file as a Bitmap: {ex.Message}";
                    }

                    break;

                case nameof(OverlayFile):
                    await CancelAndAwaitPendingGenerate();
                    OverlayFileError = null;
                    file = e.NewValue as string;
                    cachedOverlay?.Dispose();
                    cachedOverlay = null;
                    if (String.IsNullOrWhiteSpace(file) || !File.Exists(file))
                        break;

                    try
                    {
                        overlayBitmap = new Bitmap(file);
                    }
                    catch (Exception ex)
                    {
                        overlayBitmap = null;
                        OverlayFileError = $"Failed to load file as a Bitmap: {ex.Message}";
                    }

                    break;

                case nameof(SelectedFormat):
                    OptimizePaletteEnabled = SelectedFormat.IsIndexed();
                    break;

                case nameof(DisplayImage):
                    (e.OldValue as Bitmap)?.Dispose();
                    break;
            }

            if (e.PropertyName is nameof(SelectedFormat) or nameof(UseDithering) or nameof(OptimizePalette))
            {
                PixelFormatInfo pixelFormatInfo = SelectedFormat.GetInfo();
                AlphaThresholdEnabled = pixelFormatInfo.HasSingleBitAlpha 
                    || pixelFormatInfo.HasAlpha && UseDithering
                    || (pixelFormatInfo.Indexed && (OptimizePalette || pixelFormatInfo.BitsPerPixel == 8));
                BackColorEnabled = !pixelFormatInfo.HasAlpha || pixelFormatInfo.HasSingleBitAlpha || UseDithering;
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
                CancelAndAwaitPendingGenerate().GetAwaiter().GetResult();
                cachedOverlay?.Dispose();
                sourceBitmap?.Dispose();
                overlayBitmap?.Dispose();
                cancelGeneratingPreview?.Dispose();
                syncRoot.Dispose();
            }

            base.Dispose(disposing);
        }

        #endregion

        #region Private Methods

        // The caller method is async void, so basically fire-and-forget. To prevent parallel generate sessions we store the current task
        // in the generatePreviewTask field, which can be awaited after a cancellation and before starting to generate a new result.
        private async Task GenerateResult(Configuration cfg)
        {
            if (!isViewApplied || IsDisposed)
                return;

            if (!IsValid)
            {
                DisplayImage = null;
                return;
            }

            // The awaits make this method reentrant, and a continuation can be spawn after any await at any time.
            // Therefore it is possible that despite of clearing generatePreviewTask in WaitForPendingGenerate it is not null upon starting the continuation.
            while (generateResultTask != null)
                await CancelAndAwaitPendingGenerate();

            // Using a manually completable task for the generateResultTask field. If this method had just one awaitable task we could simply assign that to the field.
            TaskCompletionSource? generateTaskCompletion = null;
            Bitmap? result = null;

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

                // Ditherer: feel free to try the properties in OrderedDitherer and ErrorDiffusionDitherer
                IDitherer? ditherer = !cfg.UseDithering
                    ? null
                    : cfg.SelectedDitherer!.Create(cfg);

                // Color space can be specified for creating IBitmapData, Palette and IQuantizer instances.
                WorkingColorSpace workingColorSpace = cfg.ForceLinearColorSpace || selectedFormat.GetInfo().LinearGamma ? WorkingColorSpace.Linear : WorkingColorSpace.Srgb;

                // Quantizer: for this demo, effectively using only when palette optimization is requested.
                //            Otherwise, using a non-null quantizer only if a ditherer is selected or when forcing linear color space.
                IQuantizer? quantizer = cfg.OptimizePalette && selectedFormat.IsIndexed()
                    ? OptimizedPaletteQuantizer.Wu(1 << selectedFormat.ToBitsPerPixel(), cfg.BackColor, cfg.AlphaThreshold)
                        .ConfigureColorSpace(workingColorSpace)
                    : ditherer == null && !cfg.ForceLinearColorSpace
                        ? null
                        : selectedFormat.GetMatchingQuantizer(cfg.BackColor, AlphaThresholdEnabled ? cfg.AlphaThreshold : (byte)0).ConfigureColorSpace(workingColorSpace);

                var asyncConfig = new TaskConfig { CancellationToken = token, ThrowIfCanceled = false, Progress = progressUpdater };

                // ===== a.) No overlay: ConvertPixelFormat does everything in a single step for us. =====
                if (!cfg.ShowOverlay || cfg.Overlay == null)
                {
                    // ConvertPixelFormatAsync does not support selecting the working color space directly, so if linear color space is selected
                    // we have a non-null quantizer here. If quantizer is null, the linear color is space is used only for 48/64 bpp formats,
                    // but please note that the transparency of a 64 bpp result will be blended with the view's background by the rendering engine.
                    // See the option b.) for the low-level solutions with more flexibility.
                    result = await (quantizer == null && ditherer == null
                        ? cfg.Source!.ConvertPixelFormatAsync(selectedFormat, cfg.BackColor, cfg.AlphaThreshold, asyncConfig) // without quantizing and dithering
                        : cfg.Source!.ConvertPixelFormatAsync(selectedFormat, quantizer, ditherer, asyncConfig)); // with quantizing and/or dithering
                    return;
                }

                // ===== b.) There is an image overlay: demonstrating how to work directly with IReadWriteBitmapData in System.Drawing =====

                // Creating the temp bitmap data to work with. Will be converted back to Bitmap in the end.
                // The Format128bppPRgba format is optimized for alpha blending in the linear color space, whereas Format64bppPArgb in the sRGB color space.
                // Any other format with enough colors would be alright, though.
                using IReadWriteBitmapData tempBitmapData = BitmapDataFactory.CreateBitmapData(new Size(cfg.Source!.Width, cfg.Source.Height),
                    workingColorSpace == WorkingColorSpace.Linear ? KnownPixelFormat.Format128bppPRgba : KnownPixelFormat.Format64bppPArgb,
                    workingColorSpace, cfg.BackColor, cfg.AlphaThreshold);

                // b.1.) Drawing the source bitmap first. GetReadableBitmapData can be used for any Bitmap with any actual pixel format.
                //       Note that we don't need to specify working color space here because CopyTo/DrawInto respects the target's color space.
                using (IReadableBitmapData bmpSourceData = cfg.Source.GetReadableBitmapData())
                    await bmpSourceData.CopyToAsync(tempBitmapData, asyncConfig: asyncConfig);

                if (token.IsCancellationRequested)
                    return;

                // b.2.) Drawing the overlay. This time using DrawInto instead of CopyTo, which supports alpha blending
                IReadableBitmapData overlayBitmapData = CachedOverlay;
                var targetRectangle = new Rectangle(tempBitmapData.Width / 2 - overlayBitmapData.Width / 2,
                    tempBitmapData.Height / 2 - overlayBitmapData.Height / 2, overlayBitmapData.Width, overlayBitmapData.Height);
                await overlayBitmapData.DrawIntoAsync(tempBitmapData, new Rectangle(Point.Empty, overlayBitmapData.Size),
                    targetRectangle, asyncConfig: asyncConfig);

                if (token.IsCancellationRequested)
                    return;

                // b.3.) Converting to a Bitmap of the desired pixel format
                result = await tempBitmapData.ToBitmapAsync(selectedFormat, quantizer, ditherer, asyncConfig);
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

        private async Task CancelAndAwaitPendingGenerate()
        {
            CancelRunningGenerate();
            await WaitForPendingGenerate();
        }

        #endregion

        #region Command Handlers

        private void OnUpdateProgressCommand() => progressUpdater.UpdateProgress();

        #endregion

        #endregion
    }
}
