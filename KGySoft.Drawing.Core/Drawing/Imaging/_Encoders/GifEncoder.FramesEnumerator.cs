#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: GifEncoder.FramesEnumerator.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2021 - All Rights Reserved
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

using KGySoft.CoreLibraries;

#endregion

namespace KGySoft.Drawing.Imaging
{
    public partial class GifEncoder
    {
        private sealed class FramesEnumerator : IDisposable
        {
            #region Nested Classes

            private sealed class SuppressSubTaskProgressContext : IAsyncContext
            {
                #region Nested Classes

                private sealed class SavingProgress : IDrawingProgress
                {
                    #region Fields

                    private readonly IDrawingProgress progress;

                    #endregion

                    #region Constructors

                    public SavingProgress(IDrawingProgress progress) => this.progress = progress;

                    #endregion

                    #region Methods

                    #region Public Methods

                    public void Report(DrawingProgress value)
                    {
                        if (value.OperationType == DrawingOperation.Saving)
                            progress.Report(value);
                    }

                    public void New(DrawingOperation operationType, int maximumValue = 0, int currentValue = 0)
                    {
                        if (operationType == DrawingOperation.Saving)
                            progress.New(operationType, maximumValue, currentValue);
                    }

                    public void Increment() { }
                    public void SetProgressValue(int value) { }
                    public void Complete() { }

                    #endregion

                    #region Explicitly Implemented Interface Methods

#if !(NET35 || NET40)
                    void IProgress<DrawingProgress>.Report(DrawingProgress value) => Report(value);
#endif

                    #endregion

                    #endregion
                }

                #endregion

                #region Properties

                public int MaxDegreeOfParallelism => asyncContext.MaxDegreeOfParallelism;
                public bool IsCancellationRequested => asyncContext.IsCancellationRequested;
                public bool CanBeCanceled => asyncContext.CanBeCanceled;
                public IDrawingProgress? Progress { get; }

                #endregion

                #region Fields

                private readonly IAsyncContext asyncContext;

                #endregion

                #region Constructors

                internal SuppressSubTaskProgressContext(IAsyncContext asyncContext)
                {
                    this.asyncContext = asyncContext;
                    Progress = new SavingProgress(asyncContext.Progress!);
                }

                #endregion

                #region Methods

                public void ThrowIfCancellationRequested() => asyncContext.ThrowIfCancellationRequested();

                #endregion
            }

            #endregion

            #region Fields

            #region Static Fields

            private static readonly TimeSpan defaultDelay = new TimeSpan(100 * TimeSpan.TicksPerMillisecond);
            private static readonly TimeSpan minNonzeroDelay = new TimeSpan(10 * TimeSpan.TicksPerMillisecond);
            private static readonly TimeSpan maxDelay = new TimeSpan(minNonzeroDelay.Ticks * UInt16.MaxValue);

            // Needed only for CopyTo with skipping transparency, which expects a quantizer
            private static readonly IQuantizer identityQuantizer = PredefinedColorsQuantizer.FromCustomFunction(c => c);

            #endregion

            #region Instance Fields

            private readonly AnimatedGifConfiguration config;
            private readonly IAsyncContext asyncContext;
            private readonly IQuantizer quantizer;
            private readonly bool reportOverallProgress;

            private IEnumerator<IReadableBitmapData>? inputFramesEnumerator;
            private IEnumerator<TimeSpan>? delayEnumerator;
            private IReadableBitmapData? nextUnprocessedInputFrame;
            private Stack<(IReadWriteBitmapData BitmapData, int Delay, bool IsQuantized)>? reversedFramesStack;
            private Size logicalScreenSize;
            private int lastDelay;
            private (IReadWriteBitmapData? BitmapData, int Delay, bool IsQuantized) nextPreparedFrame;
            private (IReadableBitmapData? BitmapData, Point Location, int Delay, GifGraphicDisposalMethod DisposalMethod) nextGeneratedFrame, current;
            private (IReadWriteBitmapData? BitmapData, bool IsCleared) deltaBuffer;

            private (bool Initialized, bool SupportsTransparency, Color32 BackColor, byte AlphaThreshold) quantizerProperties;
            private IQuantizer? deltaBufferQuantizer;
            private int count;
            private int currentIndex;

            #endregion

            #endregion

            #region Properties

            #region Internal Properties

            internal IReadableBitmapData? Frame => current.BitmapData;
            internal Point Location => current.Location;
            internal int Delay => current.Delay;
            internal GifGraphicDisposalMethod DisposalMethod => current.DisposalMethod;

            #endregion

            #region Private Properties

            private Color32 BackColor
            {
                get
                {
                    Debug.Assert(config.AllowDeltaFrames, "Not expected to be called if delta frames are not allowed");
                    if (!quantizerProperties.Initialized)
                        CanUseDeltaByTransparentMask(null);

                    Debug.Assert(config.Quantizer == null || !quantizerProperties.SupportsTransparency, "Not expected to be called if transparency not supported");
                    return quantizerProperties.BackColor;
                }
            }

            private byte AlphaThreshold
            {
                get
                {
                    Debug.Assert(config.AllowDeltaFrames, "Not expected to be called if delta frames are not allowed");
                    if (!quantizerProperties.Initialized)
                        CanUseDeltaByTransparentMask(null);

                    Debug.Assert(quantizerProperties.SupportsTransparency, "Not expected to be called if transparency is not supported");
                    return quantizerProperties.AlphaThreshold;
                }
            }

            #endregion

            #endregion

            #region Constructors

            internal FramesEnumerator(AnimatedGifConfiguration config, IAsyncContext asyncContext)
            {
                this.config = config;
                reportOverallProgress = asyncContext.Progress != null && config.ReportOverallProgress != false;
                this.asyncContext = asyncContext.Progress != null && config.ReportOverallProgress == true ? new SuppressSubTaskProgressContext(asyncContext) : asyncContext;
                quantizer = config.Quantizer ?? OptimizedPaletteQuantizer.Wu();
            }

            #endregion

            #region Methods

            #region Static Methods

            private static bool HasSupportedIndexedFormat(IBitmapData bitmapData) => bitmapData.PixelFormat.Indexed && bitmapData.Palette?.Count <= 256;

            private static void ClearUnchangedPixels(IAsyncContext context, IReadWriteBitmapData previousFrame, IReadWriteBitmapData deltaFrame, byte tolerance, byte alphaThreshold)
            {
                #region Local Methods

                static void ProcessRowIndexed(IReadWriteBitmapDataRow rowPrev, IReadWriteBitmapDataRow rowDelta, int transparentIndex)
                {
                    int width = rowDelta.Width;
                    for (int x = 0; x < width; x++)
                    {
                        if (rowPrev[x] == rowDelta[x])
                            rowDelta.SetColorIndex(x, transparentIndex);
                    }
                }

                static void ProcessRowArgb(IReadWriteBitmapDataRow rowPrev, IReadWriteBitmapDataRow rowDelta)
                {
                    int width = rowDelta.Width;
                    for (int x = 0; x < width; x++)
                    {
                        if (rowPrev[x] == rowDelta[x])
                            rowDelta[x] = default;
                    }
                }

                static void ProcessRowIndexedWithTolerance(IReadWriteBitmapDataRow rowPrev, IReadWriteBitmapDataRow rowDelta, int transparentIndex, byte tolerance, byte alphaThreshold)
                {
                    int width = rowDelta.Width;
                    for (int x = 0; x < width; x++)
                    {
                        if (rowPrev[x].TolerantEquals(rowDelta[x], tolerance, alphaThreshold))
                            rowDelta.SetColorIndex(x, transparentIndex);
                    }
                }

                static void ProcessRowArgbWithTolerance(IReadWriteBitmapDataRow rowPrev, IReadWriteBitmapDataRow rowDelta, byte tolerance, byte alphaThreshold)
                {
                    int width = rowDelta.Width;
                    for (int x = 0; x < width; x++)
                    {
                        if (rowPrev[x].TolerantEquals(rowDelta[x], tolerance, alphaThreshold))
                            rowDelta[x] = default;
                    }
                }

                #endregion

                Debug.Assert(previousFrame.GetSize() == deltaFrame.GetSize());
                Debug.Assert(deltaFrame.SupportsTransparency());

                int transparentIndex = deltaFrame.Palette?.TransparentIndex ?? -1;

                // small width: going with sequential clear
                if (deltaFrame.Width < parallelThreshold)
                {
                    IReadWriteBitmapDataRow rowPrev = previousFrame.FirstRow;
                    IReadWriteBitmapDataRow rowDelta = deltaFrame.FirstRow;

                    do
                    {
                        if (context.IsCancellationRequested)
                            return;

                        if (tolerance == 0)
                        {
                            if (transparentIndex >= 0)
                                ProcessRowIndexed(rowPrev, rowDelta, transparentIndex);
                            else
                                ProcessRowArgb(rowPrev, rowDelta);
                        }
                        else if (transparentIndex >= 0)
                            ProcessRowIndexedWithTolerance(rowPrev, rowDelta, transparentIndex, tolerance, alphaThreshold);
                        else
                            ProcessRowArgbWithTolerance(rowPrev, rowDelta, tolerance, alphaThreshold);
                    } while (rowPrev.MoveNextRow() && rowDelta.MoveNextRow());
                    return;
                }

                // parallel clear
                if (tolerance == 0)
                {
                    if (transparentIndex >= 0)
                        ParallelHelper.For(context, DrawingOperation.ProcessingPixels, 0, deltaFrame.Height,
                            y => ProcessRowIndexed(previousFrame[y], deltaFrame[y], transparentIndex));
                    else
                        ParallelHelper.For(context, DrawingOperation.ProcessingPixels, 0, deltaFrame.Height,
                            y => ProcessRowArgb(previousFrame[y], deltaFrame[y]));
                }
                else if (transparentIndex >= 0)
                    ParallelHelper.For(context, DrawingOperation.ProcessingPixels, 0, deltaFrame.Height,
                        y => ProcessRowIndexedWithTolerance(previousFrame[y], deltaFrame[y], transparentIndex, tolerance, alphaThreshold));
                else
                    ParallelHelper.For(context, DrawingOperation.ProcessingPixels, 0, deltaFrame.Height,
                        y => ProcessRowArgbWithTolerance(previousFrame[y], deltaFrame[y], tolerance, alphaThreshold));
            }

            private static bool HasNewTransparentPixel(IReadableBitmapData currentFrame, IReadableBitmapData nextFrame, byte alphaThreshold, out Rectangle region)
            {
                Debug.Assert(currentFrame.GetSize() == nextFrame.GetSize());
                Debug.Assert(nextFrame.SupportsTransparency());
                Debug.Assert(alphaThreshold > 0);
                region = new Rectangle(Point.Empty, currentFrame.GetSize());

                IReadableBitmapDataRow rowCurrent = currentFrame.FirstRow;
                IReadableBitmapDataRow rowNext = nextFrame.FirstRow;
                int width = currentFrame.Width;

                do
                {
                    for (int x = 0; x < width; x++)
                    {
                        if (rowNext[x].A < alphaThreshold && rowCurrent[x].A >= alphaThreshold)
                            goto continueBottom;
                    }

                    region.Y += 1;
                    region.Height -= 1;
                } while (rowCurrent.MoveNextRow() && rowNext.MoveNextRow());

            continueBottom:
                // no new transparent pixels: we are done
                if (region.Height == 0)
                    return false;

                for (int y = region.Bottom - 1; y >= region.Top; y--)
                {
                    rowCurrent = currentFrame[y];
                    rowNext = nextFrame[y];
                    for (int x = 0; x < region.Width; x++)
                    {
                        if (rowNext[x].A < alphaThreshold && rowCurrent[x].A >= alphaThreshold)
                            goto continueLeft;
                    }

                    region.Height -= 1;
                }

            continueLeft:
                Debug.Assert(region.Height > 0);
                for (int x = 0; x < region.Width; x++)
                {
                    for (int y = region.Top; y < region.Bottom; y++)
                    {
                        if (nextFrame[y][x].A < alphaThreshold && currentFrame[y][x].A >= alphaThreshold)
                            goto continueRight;
                    }

                    region.X += 1;
                    region.Width -= 1;
                }

            continueRight:
                Debug.Assert(region.Width > 0);
                for (int x = region.Right - 1; x >= region.Left; x--)
                {
                    for (int y = region.Top; y < region.Bottom; y++)
                    {
                        if (nextFrame[y][x].A < alphaThreshold && currentFrame[y][x].A >= alphaThreshold)
                            return true;
                    }

                    region.Width -= 1;
                }

                throw new InvalidOperationException(Res.InternalError("Empty region is not expected at this point"));
            }

            [SuppressMessage("Microsoft.Maintainability", "CA1502: Avoid excessive complexity",
                Justification = "False alarm, the new analyzer includes the complexity of local methods")]
            private static Rectangle GetDifferentRegion(IReadableBitmapData previousFrame, IReadableBitmapData currentFrame, byte tolerance, Color32 backColor)
            {
                #region Local Methods

                static bool RowEquals(IReadableBitmapDataRow rowPrev, IReadableBitmapDataRow rowCurrent, byte tolerance)
                {
                    int width = rowCurrent.Width;
                    if (tolerance == 0)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            if (rowPrev[x] != rowCurrent[x])
                                return false;
                        }
                    }
                    else
                    {
                        for (int x = 0; x < width; x++)
                        {
                            if (!rowPrev[x].TolerantEquals(rowCurrent[x], tolerance))
                                return false;
                        }
                    }

                    return true;
                }

                static bool RowEqualsWithAlpha(IReadableBitmapDataRow rowPrev, IReadableBitmapDataRow rowCurrent, byte tolerance, Color32 backColor)
                {
                    int width = rowCurrent.Width;
                    if (tolerance == 0)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            if (rowPrev[x] != rowCurrent[x].BlendWithBackground(backColor))
                                return false;
                        }
                    }
                    else
                    {
                        for (int x = 0; x < width; x++)
                        {
                            if (!rowPrev[x].TolerantEquals(rowCurrent[x], tolerance, backColor))
                                return false;
                        }
                    }

                    return true;
                }

                static bool ColumnEquals(IReadableBitmapData bitmapDataPrev, IReadableBitmapData bitmapDataCurrent, int x, int top, int bottom, byte tolerance)
                {
                    if (tolerance == 0)
                    {
                        for (int y = top; y < bottom; y++)
                        {
                            if (bitmapDataPrev[y][x] != bitmapDataCurrent[y][x])
                                return false;
                        }
                    }
                    else
                    {
                        for (int y = top; y < bottom; y++)
                        {
                            if (!bitmapDataPrev[y][x].TolerantEquals(bitmapDataCurrent[y][x], tolerance))
                                return false;
                        }
                    }

                    return true;
                }

                static bool ColumnEqualsWithAlpha(IReadableBitmapData bitmapDataPrev, IReadableBitmapData bitmapDataCurrent, int x, int top, int bottom, byte tolerance, Color32 backColor)
                {
                    if (tolerance == 0)
                    {
                        for (int y = top; y < bottom; y++)
                        {
                            if (bitmapDataPrev[y][x] != bitmapDataCurrent[y][x].BlendWithBackground(backColor))
                                return false;
                        }
                    }
                    else
                    {
                        for (int y = top; y < bottom; y++)
                        {
                            if (!bitmapDataPrev[y][x].TolerantEquals(bitmapDataCurrent[y][x], tolerance, backColor))
                                return false;
                        }
                    }

                    return true;
                }

                #endregion

                Debug.Assert(previousFrame.GetSize() == currentFrame.GetSize());

                bool hasAlpha = currentFrame.HasAlpha();
                var region = new Rectangle(Point.Empty, currentFrame.GetSize());

                IReadableBitmapDataRow rowPrev = previousFrame.FirstRow;
                IReadableBitmapDataRow rowCurrent = currentFrame.FirstRow;

                // 1.) Top
                do
                {
                    if (!hasAlpha && !RowEquals(rowPrev, rowCurrent, tolerance)
                        || hasAlpha && !RowEqualsWithAlpha(rowPrev, rowCurrent, tolerance, backColor))
                    {
                        break;
                    }

                    region.Y += 1;
                    region.Height -= 1;
                } while (rowPrev.MoveNextRow() && rowCurrent.MoveNextRow());

                // no difference: we are done
                if (region.Height == 0)
                    return Rectangle.Empty;

                // 2.) Bottom
                for (int y = region.Bottom - 1; y >= region.Top; y--)
                {
                    if (!hasAlpha && !RowEquals(previousFrame[y], currentFrame[y], tolerance)
                        || hasAlpha && !RowEqualsWithAlpha(previousFrame[y], currentFrame[y], tolerance, backColor))
                    {
                        break;
                    }

                    region.Height -= 1;
                }

                // 3.) Left
                Debug.Assert(region.Height > 0);
                for (int x = 0; x < region.Width; x++)
                {
                    if (!hasAlpha && !ColumnEquals(previousFrame, currentFrame, x, region.Top, region.Bottom, tolerance)
                        || hasAlpha && !ColumnEqualsWithAlpha(previousFrame, currentFrame, x, region.Top, region.Bottom, tolerance, backColor))
                    {
                        break;
                    }

                    region.X += 1;
                    region.Width -= 1;
                }

                // 4.) Right
                Debug.Assert(region.Width > 0);
                for (int x = region.Right - 1; x >= region.Left; x--)
                {
                    if (!hasAlpha && !ColumnEquals(previousFrame, currentFrame, x, region.Top, region.Bottom, tolerance)
                        || hasAlpha && !ColumnEqualsWithAlpha(previousFrame, currentFrame, x, region.Top, region.Bottom, tolerance, backColor))
                    {
                        return region;
                    }

                    region.Width -= 1;
                }

                throw new InvalidOperationException(Res.InternalError("Empty region is not expected at this point"));
            }

            #endregion

            #region Instance Methods

            #region Public Methods

            public void Dispose()
            {
                inputFramesEnumerator?.Dispose();
                delayEnumerator?.Dispose();

                // disposing the possibly not processed but generated frames
                reversedFramesStack?.ForEach(f => f.BitmapData.Dispose());

                deltaBuffer.BitmapData?.Dispose();
                nextPreparedFrame.BitmapData?.Dispose();
                if (!ReferenceEquals(nextPreparedFrame.BitmapData, nextGeneratedFrame.BitmapData))
                    nextGeneratedFrame.BitmapData?.Dispose();
                if (!ReferenceEquals(current.BitmapData, nextGeneratedFrame.BitmapData))
                    current.BitmapData?.Dispose();
            }

            #endregion

            #region Internal Methods

            internal GifEncoder? CreateEncoder(Stream stream)
            {
                if (reportOverallProgress)
                    InitProgress();
                inputFramesEnumerator = config.Frames.GetEnumerator();
                delayEnumerator = config.Delays.GetEnumerator();

                if (!MoveNextInputFrame())
                {
                    if (asyncContext.IsCancellationRequested)
                        return null;
                    throw new ArgumentException(Res.GifEncoderAnimationContainsNoFrames);
                }

                logicalScreenSize = config.Size ?? nextUnprocessedInputFrame.GetSize();

                // this must succeed now because we could move to the first frame, unless a cancellation request occurred
                if (!MoveNextGeneratedFrame())
                    return null;

                IReadableBitmapData firstFrame = nextGeneratedFrame.BitmapData!;

                // Using a global palette only if we know that the quantizer uses always the same colors or when the first frame
                // can have transparency. If the first frame is not transparent, then some decoders (like GDI+) use a solid color when clearing frames.
                Palette? globalPalette = config.Quantizer is PredefinedColorsQuantizer || firstFrame.Palette!.HasTransparent ? firstFrame.Palette : null;

                return new GifEncoder(stream, logicalScreenSize)
                {
                    GlobalPalette = globalPalette,
                    BackColorIndex = (byte)(globalPalette?.HasAlpha == true ? globalPalette.TransparentIndex : 0),
                    RepeatCount = config.AnimationMode switch
                    {
                        AnimationMode.PlayOnce => null, // could be 1, null simply omits the application extension
                        AnimationMode.Repeat => 0,
                        AnimationMode.PingPong => 0,
                        _ => (int)config.AnimationMode
                    },
#if DEBUG
                    AddMetaInfo = true
#endif
                };
            }

            /// <summary>
            /// It consumes <see cref="nextGeneratedFrame"/> set by <see cref="MoveNextGeneratedFrame"/>, and sets <see cref="current"/>.
            /// </summary>
            /// <returns></returns>
            internal bool MoveNext()
            {
                current.BitmapData?.Dispose();
                if (asyncContext.IsCancellationRequested || nextGeneratedFrame.BitmapData == null && !MoveNextGeneratedFrame())
                {
                    current = default;
                    return false;
                }

                current = nextGeneratedFrame;
                nextGeneratedFrame = default;
                return true;
            }

            internal void ReportProgress()
            {
                if (!reportOverallProgress)
                    return;

                // finished
                if (currentIndex == count)
                {
                    Debug.Assert(nextPreparedFrame.BitmapData == null);
                    return;
                }

                currentIndex += 1;
                int max = count;

                // unknown frames count
                if (max < 0)
                {
                    // finished
                    if (nextPreparedFrame.BitmapData == null)
                        max = currentIndex;
                    // no reversed frames
                    else if (reversedFramesStack == null)
                        max = currentIndex + 1;
                    else
                    {
                        // there is a reversed frames stack
                        max = (reversedFramesStack.Count << 1) + 2;

                        // and there are no more input frames: we know the final count
                        if (inputFramesEnumerator == null)
                            count = max;
                    }
                }

                Debug.Assert(max >= currentIndex);
                asyncContext.Progress!.Report(new DrawingProgress(DrawingOperation.Saving, max, currentIndex));
            }

            #endregion

            #region Private Methods

            private bool CanUseDeltaByTransparentMask(IBitmapData? bitmapData)
            {
                if (!config.AllowDeltaFrames)
                    return false;

                // there is no explicit quantizer: depends on current frame because palette of already indexed frames are preserved
                if (config.Quantizer == null && bitmapData != null)
                {
                    // non-indexed frames will be quantized by default Wu quantizer that supports transparency
                    return !HasSupportedIndexedFormat(bitmapData) || bitmapData.SupportsTransparency();
                }

                if (quantizerProperties.Initialized)
                    return quantizerProperties.SupportsTransparency;

                quantizerProperties.Initialized = true;

                // the default Wu quantizer supports transparency and has black back color
                if (config.Quantizer == null)
                {
                    quantizerProperties.SupportsTransparency = true;
                    quantizerProperties.BackColor = Color32.Black;
                    quantizerProperties.AlphaThreshold = 128;
                    return true;
                }

                // we have to test the quantizer with a single pixel
                using IQuantizingSession session = quantizer.Initialize(new SolidBitmapData(new Size(1, 1), default));
                quantizerProperties.BackColor = session.BackColor;
                quantizerProperties.SupportsTransparency = session.GetQuantizedColor(default).A == 0;
                quantizerProperties.AlphaThreshold = quantizerProperties.SupportsTransparency ? session.AlphaThreshold : (byte)0;
                return quantizerProperties.SupportsTransparency;
            }

            private bool CanUseDeltaByClipping() => config.AllowDeltaFrames && config.AllowClippedFrames;

            /// <summary>
            /// It consumes <see cref="nextPreparedFrame"/> set by <see cref="MoveNextPreparedFrame"/>, and sets <see cref="nextGeneratedFrame"/>.
            /// Tries to generate the next frame, but it does not set <see cref="Frame"/>
            /// (it is done by <see cref="MoveNext"/>) so it can look one frame forward.
            /// </summary>
            private bool MoveNextGeneratedFrame()
            {
                Debug.Assert(nextGeneratedFrame.BitmapData == null, "MoveNextGeneratedFrame was called without processing last result by MoveNext");
                if (nextPreparedFrame.BitmapData == null)
                    MoveNextPreparedFrame();

                var preparedFrame = nextPreparedFrame;

                // no more frame
                if (preparedFrame.BitmapData == null || asyncContext.IsCancellationRequested)
                    return false;

                nextPreparedFrame = default;

                IReadWriteBitmapData? generatedFrame;
                Rectangle contentArea;
                GifGraphicDisposalMethod disposeMethod;
                if (deltaBuffer.BitmapData != null && !deltaBuffer.IsCleared)
                {
                    Debug.Assert(!preparedFrame.IsQuantized, "Prepared image must not be quantized yet if delta image might be created");
                    if (CanUseDeltaByTransparentMask(preparedFrame.BitmapData))
                        (generatedFrame, contentArea, disposeMethod) = GetGeneratedFrameByTransparencyDelta(preparedFrame.BitmapData);
                    else if (CanUseDeltaByClipping())
                        (generatedFrame, contentArea, disposeMethod) = GetGeneratedFrameByClippingDelta(preparedFrame.BitmapData);
                    else
                        (generatedFrame, contentArea, disposeMethod) = GetGeneratedFullFrame(preparedFrame.BitmapData, preparedFrame.IsQuantized);
                }
                else
                    (generatedFrame, contentArea, disposeMethod) = GetGeneratedFullFrame(preparedFrame.BitmapData, preparedFrame.IsQuantized);

                if (asyncContext.IsCancellationRequested)
                {
                    preparedFrame.BitmapData.Dispose();
                    generatedFrame?.Dispose();
                    return false;
                }

                // fully transparent image: returning 1x1 at the center
                if (contentArea.IsEmpty)
                    contentArea = new Rectangle(logicalScreenSize.Width >> 1, logicalScreenSize.Height >> 1, 1, 1);

                if (!ReferenceEquals(preparedFrame.BitmapData, generatedFrame))
                    preparedFrame.BitmapData.Dispose();

                if (contentArea.Size != generatedFrame.GetSize())
                {
                    Debug.Assert(generatedFrame.GetSize() == logicalScreenSize);
                    generatedFrame = generatedFrame!.Clip(contentArea, true);
                }

                nextGeneratedFrame = (generatedFrame, contentArea.Location, preparedFrame.Delay, disposeMethod);
                return true;
            }

            private (IReadWriteBitmapData? BitmapData, Rectangle ContentArea, GifGraphicDisposalMethod DisposeMethod) GetGeneratedFrameByTransparencyDelta(IReadWriteBitmapData preparedFrame)
            {
                Debug.Assert(deltaBuffer.BitmapData != null && !deltaBuffer.IsCleared && CanUseDeltaByTransparentMask(preparedFrame));
                Debug.Assert(preparedFrame.SupportsTransparency(), "Frame is not prepared correctly for transparency delta image");

                // 1.) Generating delta image
                ClearUnchangedPixels(asyncContext, deltaBuffer.BitmapData!, preparedFrame, config.DeltaTolerance, AlphaThreshold);
                if (asyncContext.IsCancellationRequested)
                    return default;

                // 2.) Quantizing if needed (when source is not indexed, quantizer is specified or indexed source uses multiple transparent indices)
                IReadWriteBitmapData? quantizedFrame;
                if (HasSupportedIndexedFormat(preparedFrame) && config.Quantizer == null && !HasMultipleTransparentIndices(asyncContext, preparedFrame))
                    quantizedFrame = preparedFrame;
                else
                    quantizedFrame = preparedFrame.DoClone(asyncContext, KnownPixelFormat.Format8bppIndexed, quantizer, config.Ditherer);

                if (asyncContext.IsCancellationRequested)
                    return (quantizedFrame, default, default);

                Debug.Assert(quantizedFrame != null);

                // 3.) Trimming border (important: after quantizing so possible partially transparent pixels have their final state)
                var contentArea = new Rectangle(Point.Empty, logicalScreenSize);
                if (config.AllowClippedFrames && quantizedFrame!.SupportsTransparency())
                {
                    // Determining the actual content without the transparent border.
                    // If delta is allowed and clearing is needed, then this area will be expanded later
                    contentArea = GetContentArea(quantizedFrame!);
                }

                // 4.) Maintaining the delta buffer: copying the processed part of the possibly non-quantized but already masked original image,
                // which helps to determine the unchanged part for the next frame.
                preparedFrame.DoCopyTo(asyncContext, deltaBuffer.BitmapData!, contentArea, contentArea.Location, identityQuantizer, true);
                if (asyncContext.IsCancellationRequested)
                    return (quantizedFrame, default, default);

                // 5.) Determining image dispose method and possibly expanding encoded size if clearing is needed
                GifGraphicDisposalMethod disposeMethod;
                if (MoveNextPreparedFrame() && nextPreparedFrame.BitmapData!.SupportsTransparency()
                    && HasNewTransparentPixel(deltaBuffer.BitmapData!, nextPreparedFrame.BitmapData!, AlphaThreshold, out Rectangle toClearRegion))
                {
                    disposeMethod = GifGraphicDisposalMethod.RestoreToBackground;
                    contentArea = Rectangle.Union(contentArea, toClearRegion);
                    deltaBuffer.BitmapData!.DoClear(asyncContext, default);
                    if (asyncContext.IsCancellationRequested)
                        return (quantizedFrame, default, default);

                    deltaBuffer.IsCleared = true;
                }
                else
                {
                    disposeMethod = GifGraphicDisposalMethod.DoNotDispose;
                    deltaBuffer.IsCleared = false;
                }

                return (quantizedFrame, contentArea, disposeMethod);
            }

            private (IReadWriteBitmapData? QuantizedFrame, Rectangle ContentArea, GifGraphicDisposalMethod DisposeMethod) GetGeneratedFrameByClippingDelta(IReadWriteBitmapData preparedFrame)
            {
                Debug.Assert(deltaBuffer.BitmapData != null && !deltaBuffer.IsCleared && CanUseDeltaByClipping());
                Debug.Assert(config.AllowClippedFrames && !CanUseDeltaByTransparentMask(preparedFrame), "Image with transparency support must be generated by using transparency delta");
                Debug.Assert(config.Quantizer == null || !deltaBuffer.BitmapData!.SupportsTransparency());

                // 1.) Determining smallest changed rectangle
                Rectangle contentArea = GetDifferentRegion(deltaBuffer.BitmapData!, preparedFrame, config.DeltaTolerance, BackColor);

                // 2.) Maintaining the delta buffer: as this frame has no alpha just copying the changed part of the prepared frame.
                preparedFrame.DoCopyTo(asyncContext, deltaBuffer.BitmapData!, contentArea, contentArea.Location, deltaBufferQuantizer);
                if (asyncContext.IsCancellationRequested)
                {
                    Debug.Assert(asyncContext.IsCancellationRequested);
                    return default;
                }

                // 3.) Determining image dispose method and possibly expanding encoded size if clearing is needed
                GifGraphicDisposalMethod disposeMethod;
                if (CanUseDeltaByTransparentMask(null)
                    && MoveNextPreparedFrame() && nextPreparedFrame.BitmapData!.SupportsTransparency()
                    && HasNewTransparentPixel(deltaBuffer.BitmapData!, nextPreparedFrame.BitmapData!, AlphaThreshold, out Rectangle toClearRegion))
                {
                    disposeMethod = GifGraphicDisposalMethod.RestoreToBackground;
                    contentArea = Rectangle.Union(contentArea, toClearRegion);
                    deltaBuffer.BitmapData!.DoClear(asyncContext, default);
                    if (asyncContext.IsCancellationRequested)
                        return default;

                    deltaBuffer.IsCleared = true;
                }
                else
                {
                    disposeMethod = GifGraphicDisposalMethod.DoNotDispose;
                    deltaBuffer.IsCleared = false;
                }

                // 4.) Quantizing if needed
                IReadWriteBitmapData? quantizedFrame;

                // Unchanged image: returning 1x1 pixel of the previous frame in the middle
                // (from the previous because it can be "unchanged" even just because of a nonzero tolerance)
                if (contentArea.IsEmpty)
                {
                    contentArea = new Rectangle(logicalScreenSize.Width >> 1, logicalScreenSize.Height >> 1, 1, 1);
                    quantizedFrame = deltaBuffer.BitmapData!.DoClone(asyncContext, contentArea, KnownPixelFormat.Format8bppIndexed, quantizer, config.Ditherer);
                }
                // original pixel format can be preserved (possible clipping is in caller)
                else if (config.Quantizer == null && HasSupportedIndexedFormat(preparedFrame))
                    quantizedFrame = preparedFrame;
                else
                {
                    quantizedFrame = preparedFrame.DoClone(asyncContext, contentArea, KnownPixelFormat.Format8bppIndexed, quantizer, config.Ditherer);
                    if (quantizedFrame == null)
                    {
                        Debug.Assert(asyncContext.IsCancellationRequested);
                        return default;
                    }
                }

                return (quantizedFrame, contentArea, disposeMethod);
            }

            private (IReadWriteBitmapData? QuantizedFrame, Rectangle ContentArea, GifGraphicDisposalMethod DisposeMethod) GetGeneratedFullFrame(IReadWriteBitmapData preparedFrame, bool isQuantized)
            {
                Debug.Assert(deltaBuffer.BitmapData == null || deltaBuffer.IsCleared || !CanUseDeltaByTransparentMask(preparedFrame) && !CanUseDeltaByClipping());

                // 1.) Quantizing if needed (when source is not indexed, quantizer is specified or indexed source uses multiple transparent indices)
                IReadWriteBitmapData? quantizedFrame;
                if (isQuantized || HasSupportedIndexedFormat(preparedFrame) && config.Quantizer == null && !HasMultipleTransparentIndices(asyncContext, preparedFrame))
                    quantizedFrame = preparedFrame;
                else
                    quantizedFrame = preparedFrame.DoClone(asyncContext, KnownPixelFormat.Format8bppIndexed, quantizer, config.Ditherer);

                if (asyncContext.IsCancellationRequested)
                    return (quantizedFrame, default, default);

                Debug.Assert(quantizedFrame != null);

                // 2.) Trimming border (important: after quantizing so possible partially transparent pixels have their final state)
                var contentArea = new Rectangle(Point.Empty, logicalScreenSize);
                if (config.AllowClippedFrames)
                {
                    // Determining the actual content without the transparent border.
                    // If delta is allowed and clearing is needed, then this area will be expanded later
                    contentArea = GetContentArea(quantizedFrame!);
                }

                // 3.) Determining image dispose method and possibly expanding encoded size if clearing is needed, while maintaining delta buffer
                var disposeMethod = GifGraphicDisposalMethod.DoNotDispose;
                if (CanUseDeltaByTransparentMask(null))
                {
                    Debug.Assert(quantizerProperties.SupportsTransparency);
                    deltaBuffer.BitmapData ??= BitmapDataFactory.CreateManagedBitmapData(logicalScreenSize);
                    preparedFrame.DoCopyTo(asyncContext, deltaBuffer.BitmapData, contentArea, contentArea.Location);
                    if (asyncContext.IsCancellationRequested)
                        return (quantizedFrame, default, default);

                    if (MoveNextPreparedFrame() && nextPreparedFrame.BitmapData!.SupportsTransparency()
                        && HasNewTransparentPixel(deltaBuffer.BitmapData, nextPreparedFrame.BitmapData!, AlphaThreshold, out Rectangle toClearRegion))
                    {
                        disposeMethod = GifGraphicDisposalMethod.RestoreToBackground;
                        contentArea = Rectangle.Union(contentArea, toClearRegion);
                        deltaBuffer.BitmapData.DoClear(asyncContext, default);
                        if (asyncContext.IsCancellationRequested)
                            return (quantizedFrame, default, default);

                        deltaBuffer.IsCleared = true;
                    }
                    else
                        deltaBuffer.IsCleared = false;
                }
                else if (CanUseDeltaByClipping())
                {
                    Debug.Assert(contentArea.Size == logicalScreenSize);
                    deltaBuffer.BitmapData ??= BitmapDataFactory.CreateManagedBitmapData(logicalScreenSize, KnownPixelFormat.Format24bppRgb);
                    preparedFrame.DoCopyTo(asyncContext, deltaBuffer.BitmapData, quantizer: deltaBufferQuantizer ??= PredefinedColorsQuantizer.Rgb888(quantizerProperties.BackColor.ToColor()));
                }
                // if no delta is allowed, then clearing before all frames with transparency
                else if (MoveNextPreparedFrame() && nextPreparedFrame.BitmapData!.SupportsTransparency())
                    disposeMethod = GifGraphicDisposalMethod.RestoreToBackground;

                return (quantizedFrame, contentArea, disposeMethod);
            }

            /// <summary>
            /// It consumes <see cref="nextUnprocessedInputFrame"/> set by <see cref="MoveNextInputFrame"/>, and sets <see cref="nextPreparedFrame"/>.
            /// Tries to prepare the next frame. Prepared frames are adjusted to the final size and might already be quantized if no delta frame can be generated.
            /// </summary>
            private bool MoveNextPreparedFrame()
            {
                Debug.Assert(nextPreparedFrame.BitmapData == null, "MoveNextPreparedFrame was called without processing last result by MoveNextGeneratedFrame");
                if (nextUnprocessedInputFrame == null)
                    MoveNextInputFrame();

                IReadableBitmapData? inputFrame = nextUnprocessedInputFrame;
                nextUnprocessedInputFrame = null;

                // no more input frame
                if (inputFrame == null)
                {
                    // And no more stacked reverse frame (Count can be 0 here only when PingPong was set but there are no more than 1 frames)
                    if (reversedFramesStack == null || asyncContext.IsCancellationRequested || reversedFramesStack.Count == 0)
                        return false;

                    // in reverse phase: just popping the stack
                    nextPreparedFrame = reversedFramesStack.Pop();
                    if (reversedFramesStack.Count == 0)
                        reversedFramesStack = null;
                    return true;
                }

                (IReadWriteBitmapData? preparedFrame, bool isQuantized) = GetPreparedFrame(inputFrame);
                if (preparedFrame == null)
                    return false;
                nextPreparedFrame = (preparedFrame, GetNextDelay(), isQuantized);
                if (config.AnimationMode == AnimationMode.PingPong)
                {
                    // for the first time we just create the stack so the first frame is not added
                    if (reversedFramesStack == null)
                        reversedFramesStack = new Stack<(IReadWriteBitmapData, int, bool)>();
                    else
                    {
                        // and if this is the last input frame, it is not added to the stack
                        if (MoveNextInputFrame())
                        {
                            IReadWriteBitmapData? clone = preparedFrame.DoClone(asyncContext);
                            if (clone != null)
                                reversedFramesStack.Push((clone, nextPreparedFrame.Delay, nextPreparedFrame.IsQuantized));
                        }
                    }
                }

                return !asyncContext.IsCancellationRequested;
            }

            private (IReadWriteBitmapData? BitmapData, bool IsQuantized) GetPreparedFrame(IReadableBitmapData inputFrame)
            {
                IReadWriteBitmapData? preparedFrame;
                KnownPixelFormat preparedPixelFormat = KnownPixelFormat.Format8bppIndexed;
                bool canUseDelta = false;

                // Delta frames can be used if allowed and either the quantizer can use transparent colors or clipping the frames is allowed.
                // We cannot rely on deltaBuffer.IsCleared here because a forward reading is used even to determine whether to clear
                // so if delta can be used in general, then we must delay quantizing (even for the first frame because the buffer uses non-quantized image).
                if (CanUseDeltaByTransparentMask(inputFrame))
                {
                    canUseDelta = true;
                    preparedPixelFormat = inputFrame.SupportsTransparency() && inputFrame.PixelFormat.BitsPerPixel <= 32 && inputFrame.PixelFormat.IsKnownFormat
                        ? inputFrame.PixelFormat.AsKnownPixelFormatInternal // we have transparency: we can use the original format
                        : KnownPixelFormat.Format32bppArgb; // we have to add transparency, reduce bpp or use a known pixel format
                }
                else if (CanUseDeltaByClipping())
                {
                    // Note: not using 24bpp fallback format here because possible input transparency would be blended with black instead of the back color of the quantizer
                    canUseDelta = true;
                    preparedPixelFormat = inputFrame.PixelFormat.BitsPerPixel <= 32 && inputFrame.PixelFormat.IsKnownFormat
                        ? inputFrame.PixelFormat.AsKnownPixelFormatInternal
                        : KnownPixelFormat.Format32bppArgb;
                }

                // If cannot use delta image, then we can already quantize the frame.
                // For already indexed images we preserve the original palette only if no explicit quantizer was specified.
                IQuantizer? preparedQuantizer = !canUseDelta && (config.Quantizer != null || inputFrame.PixelFormat.AsKnownPixelFormatInternal != preparedPixelFormat) ? quantizer : null;
                IDitherer? preparedDitherer = preparedQuantizer == null ? null : config.Ditherer;

                Size inputSize = inputFrame.GetSize();
                if (inputSize == logicalScreenSize)
                    preparedFrame = inputFrame.DoClone(asyncContext, preparedPixelFormat, preparedQuantizer, preparedDitherer);
                else
                {
                    switch (config.SizeHandling)
                    {
                        // Just centering, no actual resizing is needed: we might be able to use the calculated settings
                        case AnimationFramesSizeHandling.Center:
                            // the added frame is larger: we can just cut out the middle so the calculated format can be used
                            if (inputSize.Width >= logicalScreenSize.Width && inputSize.Height >= logicalScreenSize.Height)
                            {
                                Rectangle clonedArea = new Rectangle(new Point((inputSize.Width >> 1) - (logicalScreenSize.Width >> 1),
                                    (inputSize.Height >> 1) - (logicalScreenSize.Height >> 1)), logicalScreenSize);

                                preparedFrame = inputFrame.DoClone(asyncContext, clonedArea, preparedPixelFormat, preparedQuantizer, preparedDitherer);
                                break;
                            }

                            // Here a transparent border has to be added to the frame.
                            Point location = new Point((logicalScreenSize.Width >> 1) - (inputFrame.Width >> 1),
                                (logicalScreenSize.Height >> 1) - (inputFrame.Height >> 1));

                            // We can use the calculated settings if the target pixel format supports alpha.
                            // If the target format is indexed, then using it only if we can re-use the source palette (when there is no quantizer)
                            // because we cannot create a new bitmap data with a palette that is created while quantizing
                            if (preparedPixelFormat.HasAlpha() || preparedQuantizer == null && inputFrame.Palette?.HasTransparent == true)
                            {
                                preparedFrame = BitmapDataFactory.CreateBitmapData(logicalScreenSize, preparedPixelFormat, inputFrame.Palette);

                                // if the source is indexed and transparent index is not 0, then we must clear the indexed image to be transparent
                                if (inputFrame.Palette?.TransparentIndex > 0)
                                {
                                    preparedFrame.DoClear(asyncContext, default);
                                    if (asyncContext.IsCancellationRequested)
                                        break;
                                }

                                inputFrame.DoCopyTo(asyncContext, preparedFrame, location, preparedQuantizer, preparedDitherer);
                                break;
                            }

                            // Here we can't quantize the source: using a 32bpp image data
                            preparedQuantizer = null;
                            preparedFrame = BitmapDataFactory.CreateManagedBitmapData(logicalScreenSize);
                            inputFrame.DoCopyTo(asyncContext, preparedFrame, location);

                            break;

                        // Resizing: due to interpolation this is always performed without quantizing
                        case AnimationFramesSizeHandling.Resize:
                            preparedQuantizer = null;
                            preparedFrame = BitmapDataFactory.CreateManagedBitmapData(logicalScreenSize, KnownPixelFormat.Format32bppPArgb);
                            inputFrame.DoDrawInto(asyncContext, preparedFrame, new Rectangle(Point.Empty, logicalScreenSize));
                            break;

                        default:
                            throw new ArgumentException(Res.GifEncoderUnexpectedFrameSize);
                    }
                }

                return (asyncContext.IsCancellationRequested ? null : preparedFrame, preparedQuantizer != null);
            }

            /// <summary>
            /// Tries to get the next frame from <see cref="inputFramesEnumerator"/>, and sets <see cref="nextUnprocessedInputFrame"/>.
            /// </summary>
            private bool MoveNextInputFrame()
            {
                Debug.Assert(nextUnprocessedInputFrame == null, "MoveNextInputFrame was called without processing last result by MoveNextPreparedFrame");
                if (inputFramesEnumerator == null)
                    return false;

                if (asyncContext.IsCancellationRequested || !inputFramesEnumerator.MoveNext())
                {
                    inputFramesEnumerator.Dispose();
                    inputFramesEnumerator = null;
                    return false;
                }

                IReadableBitmapData? frame = inputFramesEnumerator.Current;
                if (frame == null)
                    throw new ArgumentException(Res.GifEncoderNullFrame);
                if (frame.Width < 1 || frame.Height < 1)
                    throw new ArgumentException(Res.ImagingInvalidBitmapDataSize);
                nextUnprocessedInputFrame = frame;
                return true;
            }

            private int GetNextDelay()
            {
                if (delayEnumerator == null)
                    return lastDelay;

                if (!delayEnumerator.MoveNext())
                {
                    delayEnumerator.Dispose();
                    return lastDelay;
                }

                TimeSpan delay = delayEnumerator.Current;
                if (delay > maxDelay)
                    delay = maxDelay;
                else if (delay < TimeSpan.Zero || config.ReplaceZeroDelays && delay == TimeSpan.Zero)
                    delay = defaultDelay;
                else if (delay > TimeSpan.Zero && delay < minNonzeroDelay)
                    delay = minNonzeroDelay;

                return lastDelay = (int)(delay.Ticks / minNonzeroDelay.Ticks);
            }

            private void InitProgress()
            {
                if (!config.Frames.TryGetCount(out count))
                    count = -1;
                else if (count > 2 && config.AnimationMode == AnimationMode.PingPong)
                    count = (count << 1) - 2;

                asyncContext.Progress!.New(DrawingOperation.Saving, Math.Max(1, count));
            }

            #endregion

            #endregion

            #endregion
        }
    }
}
