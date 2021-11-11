﻿#region Copyright

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

            private readonly AnimGifConfig config;
            private readonly IQuantizer quantizer;

            private IEnumerator<IReadableBitmapData>? inputFramesEnumerator;
            private IReadableBitmapData? nextUnprocessedInputFrame;
            private IEnumerator<TimeSpan>? delayEnumerator;
            private Stack<(IReadWriteBitmapData BitmapData, int Delay)>? reversedFramesStack;
            private Size logicalScreenSize;
            private int lastDelay;
            private (IReadableBitmapData? BitmapData, Point Location, int Delay, GifGraphicDisposalMethod DisposalMethod) nextGeneratedFrame;
            private (IReadableBitmapData? BitmapData, Point Location, int Delay, GifGraphicDisposalMethod DisposalMethod) current;
            private (IReadWriteBitmapData? BitmapData, int Delay) nextPreparedFrame;
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
                    if (config.Quantizer == null)
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

            internal FramesEnumerator(AnimGifConfig config)
            {
                this.config = config;
                quantizer = config.Quantizer ?? OptimizedPaletteQuantizer.Wu();
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
                nextPreparedFrame.BitmapData?.Dispose();
                if (!ReferenceEquals(nextPreparedFrame.BitmapData, nextGeneratedFrame.BitmapData))
                    nextGeneratedFrame.BitmapData?.Dispose();
                if (!ReferenceEquals(current.BitmapData, nextGeneratedFrame.BitmapData))
                    current.BitmapData?.Dispose();
            }

            #endregion

            #region Internal Methods

            internal GifEncoder CreateEncoder(Stream stream)
            {
                inputFramesEnumerator = config.Frames.GetEnumerator();
                delayEnumerator = config.Delays.GetEnumerator();

                if (!MoveNextInputFrame())
                    throw new ArgumentException(Res.GifEncoderAnimationContainsNoFrames);

                logicalScreenSize = config.Size ?? nextUnprocessedInputFrame.GetSize();

                // this must succeed now because we could move to the first frame
                MoveNextGeneratedFrame();
                IReadableBitmapData firstFrame = nextGeneratedFrame.BitmapData!;

                // Using a global palette only if we know that the quantizer uses always the same colors or when the first frame
                // can have transparency. If the first frame is not transparent, then some decoders (like GDI+) use a solid color when clearing frames.
                Palette? globalPalette = config.Quantizer is PredefinedColorsQuantizer || firstFrame.Palette!.HasAlpha ? firstFrame.Palette : null;

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
                IReadWriteBitmapData generatedFrame = preparedFrame.BitmapData;

                // 1.) Generating delta image if needed
                if (config.AllowDeltaFrames && renderBuffer.BitmapData != null && !renderBuffer.IsCleared && QuantizerSupportsTransparency)
                {
                    // we cannot re-use the prepared frame for the delta frame if the prepared frame does not support transparency
                    if (!preparedFrame.BitmapData.HasAlpha())
                    {
                        // TODO: or the original <32bpp pixel format if has alpha
                        // TODO: maybe in prepare create it with alpha support if AllowDeltaFrames is true and the renderBuffer/quantizer conditions above match?
                        IReadWriteBitmapData cloned = preparedFrame.BitmapData.Clone(PixelFormat.Format32bppArgb);
                        generatedFrame.Dispose();
                        generatedFrame = cloned;
                    }

                    ClearUnchangedPixels(renderBuffer.BitmapData, generatedFrame);
                }

                // 2.) Quantizing if needed (when source is not indexed, quantizer is specified or indexed source uses multiple transparent indices)
                if (!generatedFrame.PixelFormat.IsIndexed() || config.Quantizer != null || HasMultipleTransparentIndices(generatedFrame)) // TODO: true even for partial transparency
                {
                    // TODO: parallel if used with async context
                    IReadWriteBitmapData quantized = generatedFrame.Clone(PixelFormat.Format8bppIndexed, quantizer, config.Ditherer);
                    generatedFrame.Dispose();
                    generatedFrame = quantized;
                }

                // 3.) Trim border (important: after quantizing so possible partially transparent pixels have their final state)
                Point location = Point.Empty;
                if (!config.EncodeTransparentBorders && generatedFrame.HasAlpha())
                {
                    // clipping if possible
                    Rectangle contentArea = GetContentArea(generatedFrame);
                    if (contentArea.Size != logicalScreenSize)
                        generatedFrame = generatedFrame.Clip(contentArea, true);

                    location = contentArea.Location;
                }

                // 4.) Determining image dispose method
                var disposeMethod = GifGraphicDisposalMethod.DoNotDispose;

                // If frames can be transparent, then clearing might be needed after frames
                if (QuantizerSupportsTransparency)
                {
                    // if delta is allowed, then clearing only if a new transparent pixel appears in the next frame (that wasn't transparent before)
                    if (config.AllowDeltaFrames)
                    {
                        // maintaining render buffer
                        renderBuffer.BitmapData ??= BitmapDataFactory.CreateManagedBitmapData(logicalScreenSize);
                        generatedFrame.DrawInto(renderBuffer.BitmapData, location);

                        if (MoveNextPreparedFrame() && HasNewTransparentPixel(renderBuffer.BitmapData, nextPreparedFrame.BitmapData!, quantizerProperties.AlphaThreshold))
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
                        if (config.AnimationMode < AnimationMode.PlayOnce || MoveNextPreparedFrame())
                            disposeMethod = GifGraphicDisposalMethod.RestoreToBackground;
                    }
                }

                nextGeneratedFrame = (generatedFrame, location, preparedFrame.Delay, disposeMethod);
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

                IReadableBitmapData? inputFrame = nextUnprocessedInputFrame;
                nextUnprocessedInputFrame = null;

                // no more input frame
                if (inputFrame == null)
                {
                    // and no more stacked reverse frame
                    if (reversedFramesStack == null)
                        return false;

                    // in reverse phase: just popping the stack
                    Debug.Assert(reversedFramesStack.Count > 0);
                    nextPreparedFrame = reversedFramesStack.Pop();

                    if (reversedFramesStack.Count == 0)
                        reversedFramesStack = null;
                    return true;
                }

                Size inputSize = inputFrame.GetSize();
                IReadWriteBitmapData preparedFrame;

                if (inputSize == logicalScreenSize)
                    preparedFrame = inputFrame.Clone();
                else
                {
                    var sizeHandling = config.SizeHandling;
                    if (sizeHandling == AnimationFramesSizeHandling.ErrorIfDiffers)
                        throw new ArgumentException(Res.GifEncoderUnexpectedFrameSize);
                    // TODO: - preserve possible indexed format if possible (because of palette, and do not introduce mask search with transparency if original was not transparent)
                    // - if original was not transparent and there is resize, use 24bit rgb instead
                    // centering can use original pixel format, if
                    // - image is too large, cutting border
                    // - image is too small, adding transparent border and palette has transparency
                    IBitmapDataInternal adjusted = BitmapDataFactory.CreateManagedBitmapData(logicalScreenSize, sizeHandling == AnimationFramesSizeHandling.Center
                        && inputFrame.PixelFormat != PixelFormat.Format32bppPArgb ? PixelFormat.Format32bppArgb : PixelFormat.Format32bppPArgb);
                    switch (sizeHandling)
                    {
                        case AnimationFramesSizeHandling.Center:
                            inputFrame.CopyTo(adjusted, new Point((adjusted.Width >> 1) - (inputFrame.Width >> 1),
                                (adjusted.Height >> 1) - (inputFrame.Height >> 1)));
                            break;
                        case AnimationFramesSizeHandling.Resize:
                            inputFrame.DrawInto(adjusted, new Rectangle(Point.Empty, logicalScreenSize));
                            break;
                    }

                    preparedFrame = adjusted;
                }

                if (config.AnimationMode == AnimationMode.PingPong)
                {
                    // for the first time we just create the stack so the first frame is not added
                    if (reversedFramesStack == null)
                        reversedFramesStack = new Stack<(IReadWriteBitmapData BitmapData, int Delay)>();
                    else
                    {
                        // and if this is the last input frame, it is not added to the stack
                        if (MoveNextInputFrame())
                            reversedFramesStack.Push((preparedFrame.Clone(), nextPreparedFrame.Delay));
                    }
                }

                nextPreparedFrame = (preparedFrame, GetNextDelay());
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
                else if (delay < TimeSpan.Zero || config.ReplaceZeroDelays && delay == TimeSpan.Zero)
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
