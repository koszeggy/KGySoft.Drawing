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
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using KGySoft.ComponentModel;
using KGySoft.CoreLibraries;
using KGySoft.Drawing.Examples.Shared.Interfaces;
using KGySoft.Drawing.Examples.Shared.Model;
using KGySoft.Drawing.Imaging;
using KGySoft.Threading;

#endregion

namespace KGySoft.Drawing.Examples.WinForms.ViewModel
{
    internal class MainViewModel : ValidatingObjectBase, IDithererSettings
    {
        #region Nested Types

        #region ProgressUpdater class

        /// <summary>
        /// This class tracks every progress update without immediately updating the bound ViewModel properties,
        /// which are updated only upon request by a timer running on the UI thread. This is much more effective than
        /// invoking a synchronized update in the UI thread for every tiny progress change.
        /// </summary>
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

                owner.ProgressVisible = true;
                owner.IsProgressIndeterminate = true;
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

        #region GenerateParams record

        private record GenerateParams
        {
            #region Properties
            
            internal Bitmap? SourceBitmap { get; private init; }
            internal Bitmap? OverlayBitmap { get; private init; }
            internal bool ShowOverlay { get; private init; }
            internal PixelFormat SelectedFormat { get; private init; }
            internal Color BackColor { get; private init; }
            internal byte AlphaThreshold { get; private init; }
            internal bool OptimizePalette { get; private init; }
            internal bool UseDithering { get; private init; }
            internal DithererDescriptor? SelectedDitherer { get; private init; }

            #endregion

            #region Methods

            internal static GenerateParams Capture(MainViewModel viewModel) => new GenerateParams
            {
                SourceBitmap = viewModel.sourceBitmap,
                OverlayBitmap = viewModel.overlayBitmap,
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

        private Bitmap? sourceBitmap;
        private string? imageFileError;
        private Bitmap? overlayBitmap;
        private string? overlayFileError;
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
            progressUpdater = new ProgressUpdater(this);
            SelectedFormat = PixelFormat.Format8bppIndexed;
            ImageFile = @"..\..\..\..\..\Help\Images\Shield256.png";
            OverlayFile = @"..\..\..\..\..\Help\Images\AlphaGradient.png";
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
            else if (imageFileError != null)
                result.AddError(nameof(ImageFile), imageFileError);
            if (ShowOverlay && String.IsNullOrEmpty(OverlayFile) || !File.Exists(OverlayFile))
                result.AddError(nameof(OverlayFile), "The specified file does not exist");
            else if (overlayFileError != null)
                result.AddError(nameof(OverlayFile), overlayFileError);

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
                        sourceBitmap?.Dispose();
                        sourceBitmap = new Bitmap(file);
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
                        overlayFileError = ex.Message;
                    }

                    break;

                case nameof(SelectedFormat):
                    OptimizePaletteEnabled = SelectedFormat.IsIndexed();
                    break;
            }

            if (e.PropertyName is nameof(SelectedFormat) or nameof(UseDithering) or nameof(OptimizePalette))
            {
                PixelFormatInfo pixelFormatInfo = SelectedFormat.GetInfo();
                AlphaThresholdEnabled = pixelFormatInfo.HasAlpha && UseDithering || (pixelFormatInfo.Indexed && (OptimizePalette || pixelFormatInfo.BitsPerPixel == 8));
                BackColorEnabled = !pixelFormatInfo.HasAlpha || UseDithering;
            }

            if (affectsPreview.Contains(e.PropertyName!))
                await GenerateResult(GenerateParams.Capture(this));
        }

        protected override void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;
            if (disposing)
            {
                cachedOverlay?.Dispose();
                sourceBitmap?.Dispose();
                overlayBitmap?.Dispose();
                cancelGeneratingPreview?.Dispose();
            }

            base.Dispose(disposing);
        }

        #endregion

        #region Private Methods

        // The caller method is async void, so basically fire-and-forget. To prevent parallel generate sessions we store the current task
        // in the generatePreviewTask field, which can be awaited after a cancellation and before starting to generate a new result.
        private async Task GenerateResult(GenerateParams generateParams)
        {
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

            // This is essentially a lock. Achieved by a SemaphoreSlim because an actual lock cannot be used with awaits in the code.
            await syncRoot.WaitAsync();
            try
            {
                // lost race: returning if parameters have been changed since calling this method
                if (generateParams != GenerateParams.Capture(this))
                    return;

                Bitmap? bmpSource = generateParams.SourceBitmap;
                Bitmap? bmpOverlay = generateParams.OverlayBitmap;
                if (IsDisposed || bmpSource == null)
                    return;

                // Creating a manually completable task for the generateResultTask field. Would not be necessary if this method had just one awaitable task.
                var generateTaskCompletion = new TaskCompletionSource();
                CancellationTokenSource tokenSource = cancelGeneratingPreview = new CancellationTokenSource();
                generateResultTask = generateTaskCompletion.Task;
                try
                {
                    progressUpdater.Start();
                    PixelFormat selectedFormat = generateParams.SelectedFormat;
                    CancellationToken token = tokenSource.Token;

                    // Ditherer: feel free to try the other properties in OrderedDitherer and ErrorDiffusionDitherer
                    IDitherer? ditherer = !generateParams.UseDithering
                        ? null
                        : generateParams.SelectedDitherer!.Create(this);

                    // Quantizer: effectively using only when palette optimization is requested.
                    // Otherwise, if ditherer is set, then picking a quantizer that matches the selected pixel format.
                    IQuantizer? quantizer = generateParams.OptimizePalette && selectedFormat.IsIndexed()
                        ? OptimizedPaletteQuantizer.Wu(1 << selectedFormat.ToBitsPerPixel(), generateParams.BackColor, generateParams.AlphaThreshold)
                        : ditherer == null
                            ? null
                            : selectedFormat.GetMatchingQuantizer(generateParams.BackColor, generateParams.AlphaThreshold);

                    Bitmap? result = null;
                    var asyncConfig = new TaskConfig { CancellationToken = token, ThrowIfCanceled = false, Progress = progressUpdater };

                    try
                    {
                        // No overlay: ConvertPixelFormat does everything in a single step for us.
                        if (!generateParams.ShowOverlay || bmpOverlay == null)
                        {
                            result = await (quantizer == null && ditherer == null
                                ? bmpSource.ConvertPixelFormatAsync(selectedFormat, generateParams.BackColor, generateParams.AlphaThreshold, asyncConfig) // without quantizing and dithering
                                : bmpSource.ConvertPixelFormatAsync(selectedFormat, quantizer, ditherer, asyncConfig)); // with quantizing and/or dithering
                            return;
                        }

                        // There is an image overlay: demonstrating how to work directly with IReadWriteBitmapData in System.Drawing
                        using IReadWriteBitmapData resultBitmapData = BitmapDataFactory.CreateBitmapData(new Size(bmpSource.Width, bmpSource.Height),
                            KnownPixelFormat.Format32bppArgb, generateParams.BackColor, generateParams.AlphaThreshold);

                        // 1.) Drawing the source bitmap first. GetReadableBitmapData can be used for any Bitmap.
                        using (var bmpSourceData = bmpSource.GetReadableBitmapData())
                            await bmpSourceData.CopyToAsync(resultBitmapData, asyncConfig: asyncConfig);

                        if (token.IsCancellationRequested)
                            return;

                        // 2.) Drawing the overlay. This time using DrawInto instead of CopyTo, which supports alpha blending
                        IReadableBitmapData overlayBitmapData = CachedOverlay;
                        var targetRectangle = new Rectangle(resultBitmapData.Width / 2 - overlayBitmapData.Width / 2,
                            resultBitmapData.Height / 2 - overlayBitmapData.Height / 2, overlayBitmapData.Width, overlayBitmapData.Height);
                        await overlayBitmapData.DrawIntoAsync(resultBitmapData, new Rectangle(Point.Empty, overlayBitmapData.Size),
                            targetRectangle, asyncConfig: asyncConfig);

                        if (token.IsCancellationRequested)
                            return;

                        // 3.) Converting the result back to a Bitmap of the desired pixel format
                        result = await resultBitmapData.ToBitmapAsync(selectedFormat, quantizer, ditherer, asyncConfig);
                    }
                    finally
                    {
                        progressUpdater.Stop();
                        if (result != null)
                            DisplayImage = result;
                    }
                }
                finally
                {
                    generateTaskCompletion.SetResult();
                }
            }
            finally
            {
                syncRoot.Release();
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

        #region Command Handlers

        private void OnUpdateProgressCommand() => progressUpdater.UpdateProgress();

        #endregion

        #endregion
    }
}
