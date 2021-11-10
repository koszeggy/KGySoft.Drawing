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
            #region Fields

            #region Static Fields

            private static readonly TimeSpan defaultDelay = new TimeSpan(100 * TimeSpan.TicksPerMillisecond);
            private static readonly TimeSpan minNonzeroDelay = new TimeSpan(10 * TimeSpan.TicksPerMillisecond);
            private static readonly TimeSpan maxDelay = new TimeSpan(minNonzeroDelay.Ticks * UInt16.MaxValue);

            #endregion

            #region Instance Fields

            private readonly AnimationParameters parameters;
            private readonly IQuantizer quantizer;

            private IEnumerator<IReadableBitmapData>? inputFramesEnumerator;
            private IReadableBitmapData? nextUnprocessedInputFrame;
            private IEnumerator<TimeSpan>? delayEnumerator;
            private Stack<(IReadableBitmapData BitmapData, int Delay)>? reversedFramesStack;
            private Size logicalScreenSize;
            private int lastDelay;
            private (IReadableBitmapData? BitmapData, Point Location, int Delay, GifGraphicDisposalMethod DisposalMethod, bool DisposeBitmapData) nextGeneratedFrame;
            private (IReadableBitmapData? BitmapData, Point Location, int Delay, GifGraphicDisposalMethod DisposalMethod, bool DisposeBitmapData) current;
            private (IReadableBitmapData? BitmapData, int Delay, bool DisposeBitmapData) nextPreparedFrame;
            private (IReadWriteBitmapData? BitmapData, bool IsCleared) renderBuffer;
            private (bool Initialized, bool SupportsTransparency, byte AlphaThreshold) quantizerProperties;

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

            private bool QuantizerSupportsTransparency
            {
                get
                {
                    if (quantizerProperties.Initialized)
                        return quantizerProperties.SupportsTransparency;
                    
                    quantizerProperties.Initialized = true;

                    // the default Wu quantizer supports transparency
                    if (parameters.Quantizer == null)
                    {
                        quantizerProperties.SupportsTransparency = true;
                        quantizerProperties.AlphaThreshold = 128;
                        return true;
                    }

                    // we have to test the quantizer with a single pixel
                    using IQuantizingSession session = quantizer.Initialize(new SolidBitmapData(new Size(1, 1), default));
                    quantizerProperties.SupportsTransparency = session.GetQuantizedColor(default).A == 0;
                    quantizerProperties.AlphaThreshold = quantizerProperties.SupportsTransparency ? session.AlphaThreshold : (byte)0;
                    return quantizerProperties.SupportsTransparency;
                }
            }

            #endregion

            #endregion

            #region Constructors

            internal FramesEnumerator(AnimationParameters parameters)
            {
                this.parameters = parameters;
                quantizer = parameters.Quantizer ?? OptimizedPaletteQuantizer.Wu();
            }

            #endregion

            #region Methods

            #region Static Methods

            private static void ClearUnchangedPixels(IReadWriteBitmapData previousFrame, IReadWriteBitmapData deltaFrame)
            {
                Debug.Assert(previousFrame.GetSize() == deltaFrame.GetSize());
                Debug.Assert(deltaFrame.HasAlpha());

                int transparentIndex = deltaFrame.Palette?.TransparentIndex ?? -1;
                IReadWriteBitmapDataRow rowPrev = previousFrame.FirstRow;
                IReadWriteBitmapDataRow rowDelta = deltaFrame.FirstRow;
                int width = deltaFrame.Width;

                // TODO: parallel if used with async context
                do
                {
                    if (transparentIndex >= 0)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            if (rowPrev[x] == rowDelta[x])
                                rowDelta.SetColorIndex(x, transparentIndex);
                        }
                    }
                    else
                    {
                        for (int x = 0; x < width; x++)
                        {
                            if (rowPrev[x] == rowDelta[x])
                                rowDelta[x] = default;
                        }
                    }
                } while (rowPrev.MoveNextRow() && rowDelta.MoveNextRow());
            }

            private static bool HasNewTransparentPixel(IReadableBitmapData currentFrame, IReadableBitmapData nextFrame, byte alphaThreshold)
            {
                Debug.Assert(currentFrame.GetSize() == nextFrame.GetSize());

                if (!nextFrame.HasAlpha())
                    return false;

                IReadableBitmapDataRow rowCurrent = currentFrame.FirstRow;
                IReadableBitmapDataRow rowNext = nextFrame.FirstRow;
                int width = currentFrame.Width;

                // TODO: parallel if used with async context
                do
                {
                    for (int x = 0; x < width; x++)
                    {
                        Debug.Assert(rowCurrent[x].A is 0 or 255, "currentFrame must not have partially transparent pixels");
                        if (rowNext[x].A < alphaThreshold && rowCurrent[x].A != 0)
                            return true;
                    }
                } while (rowCurrent.MoveNextRow() && rowNext.MoveNextRow());

                return false;
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

                renderBuffer.BitmapData?.Dispose();
                if (nextPreparedFrame.DisposeBitmapData)
                    nextPreparedFrame.BitmapData!.Dispose();
                if (nextGeneratedFrame.DisposeBitmapData)
                    nextGeneratedFrame.BitmapData!.Dispose();
                if (current.DisposeBitmapData && !ReferenceEquals(current.BitmapData, nextGeneratedFrame.BitmapData))
                    current.BitmapData!.Dispose();
            }

            #endregion

            #region Internal Methods

            internal GifEncoder CreateEncoder(Stream stream)
            {
                inputFramesEnumerator = parameters.Frames.GetEnumerator();
                delayEnumerator = parameters.Delays.GetEnumerator();

                if (!MoveNextInputFrame())
                    throw new ArgumentException(Res.GifEncoderAnimationContainsNoFrames);

                logicalScreenSize = parameters.Size ?? nextUnprocessedInputFrame.GetSize();

                // this must succeed now because we could move to the first frame
                MoveNextGeneratedFrame();
                IReadableBitmapData firstFrame = nextGeneratedFrame.BitmapData!;

                // Using a global palette only if we know that the quantizer uses always the same colors or when the first frame
                // can have transparency. If the first frame is not transparent, then some decoders (like GDI+) use a solid color when clearing frames.
                Palette? globalPalette = parameters.Quantizer is PredefinedColorsQuantizer || firstFrame.Palette!.HasAlpha ? firstFrame.Palette : null;

                return new GifEncoder(stream, logicalScreenSize)
                {
                    GlobalPalette = globalPalette,
                    BackColorIndex = (byte)(globalPalette?.HasAlpha == true ? globalPalette.TransparentIndex : 0),
                    RepeatCount = parameters.AnimationMode switch
                    {
                        AnimationMode.PlayOnce => null, // could be 1, null simply omits the application extension
                        AnimationMode.Repeat => 0,
                        AnimationMode.PingPong => 0,
                        _ => (int)parameters.AnimationMode
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
                if (current.DisposeBitmapData)
                    current.BitmapData!.Dispose();

                if (nextGeneratedFrame.BitmapData == null && !MoveNextGeneratedFrame())
                {
                    current = default;
                    return false;
                }

                current = nextGeneratedFrame;
                nextGeneratedFrame = default;
                return true;
            }

            #endregion

            #region Private Methods

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
                if (preparedFrame.BitmapData == null)
                    return false;

                nextPreparedFrame = default;
                IReadableBitmapData generatedFrame;
                bool toBeDisposed = preparedFrame.DisposeBitmapData;

                // 1.) Generating delta image if needed
                if (parameters.AllowDeltaFrames && renderBuffer.BitmapData != null && !renderBuffer.IsCleared && QuantizerSupportsTransparency)
                {
                    IReadWriteBitmapData? deltaFrame = null;

                    // we can re-use the prepared frame for the delta frame if the prepared frame is about to be disposed anyway and it has transparency
                    if (preparedFrame.DisposeBitmapData && preparedFrame.BitmapData.HasAlpha())
                        deltaFrame = preparedFrame.BitmapData as IReadWriteBitmapData;
                    if (deltaFrame == null)
                    {
                        // TODO: or the original <32bpp pixel format if has alpha
                        deltaFrame = preparedFrame.BitmapData.Clone(PixelFormat.Format32bppArgb);

                        // disposing prepared frame if we replaced it (we could do it just in the end but this way we can optimize memory usage)
                        if (preparedFrame.DisposeBitmapData)
                        {
                            preparedFrame.BitmapData.Dispose();
                            preparedFrame.DisposeBitmapData = false;
                        }

                        toBeDisposed = true;
                    }

                    ClearUnchangedPixels(renderBuffer.BitmapData, deltaFrame);
                    generatedFrame = deltaFrame;
                }
                else
                    generatedFrame = preparedFrame.BitmapData;

                // 2.) Quantizing if needed (when source is not indexed, quantizer is specified or indexed source uses multiple transparent indices)
                if (!generatedFrame.PixelFormat.IsIndexed() || parameters.Quantizer != null || HasMultipleTransparentIndices(generatedFrame)) // TODO: true even for partial transparency
                {
                    // TODO: parallel if used with async context
                    IReadWriteBitmapData quantized = generatedFrame.Clone(PixelFormat.Format8bppIndexed, quantizer, parameters.Ditherer);
                    if (!ReferenceEquals(preparedFrame.BitmapData, generatedFrame))
                        generatedFrame.Dispose();
                    generatedFrame = quantized;
                    toBeDisposed = true;

                    // disposing prepared frame if needed (we could do it just in the end but this way we can optimize memory usage)
                    if (preparedFrame.DisposeBitmapData)
                    {
                        preparedFrame.BitmapData.Dispose();
                        preparedFrame.DisposeBitmapData = false;
                    }
                }

                // 3.) Trim border (important: after quantizing so possible partially transparent pixels have their final state)
                Point location = Point.Empty;
                if (!parameters.EncodeTransparentBorders && generatedFrame.HasAlpha())
                {
                    Rectangle contentArea = GetContentArea(generatedFrame);

                    // trimming is possible
                    if (contentArea.Size != logicalScreenSize)
                    {
                        // if still using the prepared frame, disabling its disposing because now we created a wrapper for it
                        if (ReferenceEquals(preparedFrame.BitmapData, generatedFrame))
                        {
                            Debug.Assert(preparedFrame.DisposeBitmapData == toBeDisposed, "Original disposal info should be reflected by toBeDisposed");
                            preparedFrame.DisposeBitmapData = false;
                        }

                        generatedFrame = generatedFrame.Clip(contentArea, toBeDisposed);
                    }

                    location = contentArea.Location;
                }

                // 4.) Determining image dispose method
                var disposeMethod = GifGraphicDisposalMethod.DoNotDispose;

                // If frames can be transparent, then clearing might be needed after frames
                if (QuantizerSupportsTransparency)
                {
                    // if delta is allowed, then clearing only if a new transparent pixel appears in the next frame (that wasn't transparent before)
                    if (parameters.AllowDeltaFrames)
                    {
                        // maintaining render buffer
                        if (renderBuffer.BitmapData == null)
                            renderBuffer.BitmapData = BitmapDataFactory.CreateManagedBitmapData(logicalScreenSize);
                        else
                            generatedFrame.DrawInto(renderBuffer.BitmapData, location);

                        if (MoveNextPreparedFrame() && HasNewTransparentPixel(renderBuffer.BitmapData, preparedFrame.BitmapData, quantizerProperties.AlphaThreshold))
                        {
                            disposeMethod = GifGraphicDisposalMethod.RestoreToBackground;

                            // TODO: parallel if used with async context
                            renderBuffer.BitmapData.Clear(default);
                            renderBuffer.IsCleared = true;
                        }
                        else
                            renderBuffer.IsCleared = false;
                    }
                    // if no delta is allowed, then clearing after each frame
                    else
                    {
                        // except the last frame if the animation is not looped indefinitely
                        if (parameters.AnimationMode < AnimationMode.PlayOnce || MoveNextPreparedFrame())
                            disposeMethod = GifGraphicDisposalMethod.RestoreToBackground;
                    }
                }

                if (preparedFrame.DisposeBitmapData)
                    preparedFrame.BitmapData.Dispose();
                nextGeneratedFrame = (generatedFrame, location, preparedFrame.Delay, disposeMethod, toBeDisposed);
                return true;
            }

            /// <summary>
            /// It consumes <see cref="nextUnprocessedInputFrame"/> set by <see cref="MoveNextInputFrame"/>, and sets <see cref="nextPreparedFrame"/>.
            /// Tries to prepare the next frame. Prepared frames are not processed yet but they are already adjusted to the final size.
            /// </summary>
            private bool MoveNextPreparedFrame()
            {
                Debug.Assert(nextPreparedFrame.BitmapData == null, "MoveNextPreparedFrame was called without processing last result by MoveNextGeneratedFrame");
                if (nextUnprocessedInputFrame == null)
                    MoveNextInputFrame();

                IReadableBitmapData? nextInputFrame = nextUnprocessedInputFrame;
                nextUnprocessedInputFrame = null;

                // no more input frame
                if (nextInputFrame == null)
                {
                    // and no more stacked reverse frame
                    if (reversedFramesStack == null)
                        return false;

                    // in reverse phase: just popping the stack
                    Debug.Assert(reversedFramesStack.Count > 0);
                    (nextPreparedFrame.BitmapData, nextPreparedFrame.Delay) = reversedFramesStack.Pop();
                    nextPreparedFrame.DisposeBitmapData = true;

                    if (reversedFramesStack.Count == 0)
                        reversedFramesStack = null;
                    return true;
                }

                Size inputSize = nextInputFrame.GetSize();
                IReadableBitmapData result;

                if (inputSize == logicalScreenSize)
                    // this will use the original input frame also for the first frame in pingpong mode
                    // we could add || !MoveNextInputFrame() to not clone the last either but if there are more frames the current one might be destroyed
                    result = reversedFramesStack == null ? nextInputFrame : nextInputFrame.Clone();
                else
                {
                    var sizeHandling = parameters.SizeHandling;
                    if (sizeHandling == AnimationFramesSizeHandling.ErrorIfDiffers)
                        throw new ArgumentException(Res.GifEncoderUnexpectedFrameSize);
                    // TODO: - preserve possible indexed format if possible (because of palette, and do not introduce mask search with transparency if original was not transparent)
                    // - if original was not transparent and there is resize, use 24bit rgb instead
                    // centering can use original pixel format, if
                    // - image is too large, cutting border
                    // - image is too small, adding transparent border and palette has transparency
                    IBitmapDataInternal preparedFrame = BitmapDataFactory.CreateManagedBitmapData(logicalScreenSize, sizeHandling == AnimationFramesSizeHandling.Center
                        && nextInputFrame.PixelFormat != PixelFormat.Format32bppPArgb ? PixelFormat.Format32bppArgb : PixelFormat.Format32bppPArgb);
                    switch (sizeHandling)
                    {
                        case AnimationFramesSizeHandling.Center:
                            nextInputFrame.CopyTo(preparedFrame, new Point((preparedFrame.Width >> 1) - (nextInputFrame.Width >> 1),
                                (preparedFrame.Height >> 1) - (nextInputFrame.Height >> 1)));
                            break;
                        case AnimationFramesSizeHandling.Resize:
                            nextInputFrame.DrawInto(preparedFrame, new Rectangle(Point.Empty, logicalScreenSize));
                            break;
                    }

                    result = nextInputFrame;
                }

                nextPreparedFrame = (result, GetNextDelay(), !ReferenceEquals(result, nextInputFrame) && reversedFramesStack == null);

                if (parameters.AnimationMode == AnimationMode.PingPong)
                {
                    // for the first time we just create the stack so the first frame is not added
                    if (reversedFramesStack == null)
                        reversedFramesStack = new Stack<(IReadableBitmapData BitmapData, int Delay)>();
                    else
                    {
                        // if this is the last input frame, it is not added to the stack
                        Debug.Assert(!nextPreparedFrame.DisposeBitmapData, "Prepared frame must be a clone if we look forward to next frame");
                        if (MoveNextInputFrame())
                            reversedFramesStack.Push((nextPreparedFrame.BitmapData!, nextPreparedFrame.Delay));
                    }
                }

                return true;
            }

            /// <summary>
            /// Tries to get the next frame from <see cref="inputFramesEnumerator"/>, and sets <see cref="nextUnprocessedInputFrame"/>.
            /// </summary>
            private bool MoveNextInputFrame()
            {
                Debug.Assert(nextUnprocessedInputFrame == null, "MoveNextInputFrame was called without processing last result by MoveNextPreparedFrame");
                if (inputFramesEnumerator == null)
                    return false;

                if (!inputFramesEnumerator.MoveNext())
                {
                    inputFramesEnumerator.Dispose();
                    inputFramesEnumerator = null;
                    return false;
                }

                IReadableBitmapData frame = inputFramesEnumerator.Current;
                if (frame == null!)
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
                else if (delay < TimeSpan.Zero || parameters.ReplaceZeroDelays && delay == TimeSpan.Zero)
                    delay = defaultDelay;
                else if (delay > TimeSpan.Zero && delay < minNonzeroDelay)
                    delay = minNonzeroDelay;

                return lastDelay = (int)(delay.Ticks / minNonzeroDelay.Ticks);
            }

            #endregion

            #endregion

            #endregion
        }
    }
}
