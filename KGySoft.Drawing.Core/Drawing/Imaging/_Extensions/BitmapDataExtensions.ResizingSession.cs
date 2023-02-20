#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BitmapDataExtensions.ResizingSession.cs
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

using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Security;

using KGySoft.Collections;
using KGySoft.CoreLibraries;
using KGySoft.Threading;

#endregion

namespace KGySoft.Drawing.Imaging
{
    // This partial class contains GDI-independent resizing logic
    // Credit to ImageSharp resize, on which this code partially based on (see https://github.com/SixLabors/ImageSharp/tree/6183dd8c13a9fd044a1a045bafb30bab4f2d6049/src/ImageSharp/Processing/Processors/Transforms/Resize)
    // ImageSharp is under the GNU Affero General Public License v3.0, which is available here: https://www.gnu.org/licenses/agpl-3.0.html
    partial class BitmapDataExtensions
    {
        #region Nested Types

        #region ResizingSessionNearestNeighbor class

        [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Local",
            Justification = "False alarm, Rectangle fields should not be read-only to prevent creating defensive copies.")]
        private sealed class ResizingSessionNearestNeighbor
        {
            #region Fields

            private readonly IAsyncContext context;
            private readonly IBitmapDataInternal source;
            private readonly IBitmapDataInternal target;
            private readonly bool useLinearColorSpace;

            private Rectangle sourceRectangle;
            private Rectangle targetRectangle;

            #endregion

            #region Constructors

            internal ResizingSessionNearestNeighbor(IAsyncContext context, IBitmapDataInternal source, IBitmapDataInternal target,
                Rectangle sourceRectangle, Rectangle targetRectangle, bool useLinearColorSpace)
            {
                this.context = context;
                this.source = source;
                this.target = target;
                this.sourceRectangle = sourceRectangle;
                this.targetRectangle = targetRectangle;
                this.useLinearColorSpace = useLinearColorSpace;
            }

            #endregion

            #region Methods

            #region Internal Methods
            
            internal void PerformResize(IQuantizer? quantizer, IDitherer? ditherer)
            {
                if (quantizer == null)
                {
                    PerformResizeDirect();
                    return;
                }

                IReadableBitmapData initSource = targetRectangle.Size == target.Size
                    ? target
                    : target.Clip(targetRectangle);

                try
                {
                    Debug.Assert(!quantizer.InitializeReliesOnContent, "This method performs resize during quantization but the used quantizer would require two-pass processing");
                    context.Progress?.New(DrawingOperation.InitializingQuantizer);
                    using (IQuantizingSession quantizingSession = quantizer.Initialize(initSource, context))
                    {
                        if (context.IsCancellationRequested)
                            return;
                        if (quantizingSession == null)
                            throw new InvalidOperationException(Res.ImagingQuantizerInitializeNull);

                        // quantizing without dithering
                        if (ditherer == null)
                        {
                            PerformResizeWithQuantizer(quantizingSession);
                            return;
                        }

                        // quantizing with dithering
                        Debug.Assert(!ditherer.InitializeReliesOnContent, "This method performs resize during dithering but the used ditherer would require two-pass processing");
                        context.Progress?.New(DrawingOperation.InitializingDitherer);
                        using IDitheringSession ditheringSession = ditherer.Initialize(initSource, quantizingSession, context);
                        if (context.IsCancellationRequested)
                            return;
                        if (ditheringSession == null)
                            throw new InvalidOperationException(Res.ImagingDithererInitializeNull);

                        PerformResizeWithDithering(quantizingSession, ditheringSession);
                    }
                }
                finally
                {
                    if (!ReferenceEquals(initSource, target))
                        initSource.Dispose();
                }
            }

            internal void PerformResizeDirect()
            {
                Action<int> processRow = target.IsFastPremultiplied() && !useLinearColorSpace
                    ? ProcessRowPremultipliedSrgb
                    : ProcessRowStraight;

                // Sequential processing
                if (targetRectangle.Width < parallelThreshold)
                {
                    context.Progress?.New(DrawingOperation.ProcessingPixels, targetRectangle.Height);
                    for (int y = 0; y < targetRectangle.Height; y++)
                    {
                        if (context.IsCancellationRequested)
                            return;
                        processRow.Invoke(y);
                        context.Progress?.Increment();
                    }

                    return;
                }

                // Parallel processing
                ParallelHelper.For(context, DrawingOperation.ProcessingPixels, 0, targetRectangle.Height, processRow);

                #region Local Methods

                void ProcessRowStraight(int y)
                {
                    // Scaling factors
                    float widthFactor = sourceRectangle.Width / (float)targetRectangle.Width;
                    float heightFactor = sourceRectangle.Height / (float)targetRectangle.Height;

                    IBitmapDataRowInternal rowSrc = source.GetRowCached((int)(y * heightFactor + sourceRectangle.Y));
                    IBitmapDataRowInternal rowDst = target.GetRowCached(y + targetRectangle.Y);

                    int targetLeft = targetRectangle.Left;
                    int sourceLeft = sourceRectangle.Left;
                    int targetWidth = targetRectangle.Width;
                    byte alphaThreshold = target.PixelFormat.HasMultiLevelAlpha ? (byte)0 : target.AlphaThreshold;
                    bool linearBlending = useLinearColorSpace;
                    for (int x = 0; x < targetWidth; x++)
                    {
                        Color32 colorSrc = rowSrc.DoGetColor32((int)(x * widthFactor + sourceLeft));

                        // fully solid source: overwrite
                        if (colorSrc.A == Byte.MaxValue)
                        {
                            rowDst.DoSetColor32(x + targetLeft, colorSrc);
                            continue;
                        }

                        // fully transparent source: skip
                        if (colorSrc.A == 0)
                            continue;

                        // source here has a partial transparency: we need to read the target color
                        int pos = x + targetLeft;
                        Color32 colorDst = rowDst.DoGetColor32(pos);

                        // non-transparent target: blending
                        if (colorDst.A != 0)
                        {
                            colorSrc = colorDst.A == Byte.MaxValue
                                // target pixel is fully solid: simple blending
                                ? colorSrc.BlendWithBackground(colorDst, linearBlending)
                                // both source and target pixels are partially transparent: complex blending
                                : colorSrc.BlendWith(colorDst, linearBlending);
                        }

                        // overwriting target color only if blended color has high enough alpha
                        if (colorSrc.A < alphaThreshold)
                            continue;

                        rowDst.DoSetColor32(pos, colorSrc);
                    }
                }

                void ProcessRowPremultipliedSrgb(int y)
                {
                    // Scaling factors
                    float widthFactor = sourceRectangle.Width / (float)targetRectangle.Width;
                    float heightFactor = sourceRectangle.Height / (float)targetRectangle.Height;

                    IBitmapDataRowInternal rowSrc = source.GetRowCached((int)(y * heightFactor + sourceRectangle.Y));
                    IBitmapDataRowInternal rowDst = target.GetRowCached(y + targetRectangle.Y);

                    int targetLeft = targetRectangle.Left;
                    int sourceLeft = sourceRectangle.Left;
                    int targetWidth = targetRectangle.Width;
                    for (int x = 0; x < targetWidth; x++)
                    {
                        Color32 colorSrc = rowSrc.DoGetColor32Premultiplied((int)(x * widthFactor + sourceLeft));

                        // fully solid source: overwrite
                        if (colorSrc.A == Byte.MaxValue)
                        {
                            rowDst.DoSetColor32Premultiplied(x + targetLeft, colorSrc);
                            continue;
                        }

                        // fully transparent source: skip
                        if (colorSrc.A == 0)
                            continue;

                        // source here has a partial transparency: we need to read the target color
                        int pos = x + targetLeft;
                        Color32 colorDst = rowDst.DoGetColor32Premultiplied(pos);

                        // non-transparent target: blending
                        if (colorDst.A != 0)
                            colorSrc = colorSrc.BlendWithPremultipliedSrgb(colorDst);

                        rowDst.DoSetColor32Premultiplied(pos, colorSrc);
                    }
                }

                #endregion
            }

            #endregion

            #region Private Methods

            private void PerformResizeWithQuantizer(IQuantizingSession quantizingSession)
            {
                // Sequential processing
                if (targetRectangle.Width < parallelThreshold >> quantizingScale)
                {
                    context.Progress?.New(DrawingOperation.ProcessingPixels, targetRectangle.Height);
                    for (int y = 0; y < targetRectangle.Height; y++)
                    {
                        if (context.IsCancellationRequested)
                            return;
                        ProcessRow(y);
                        context.Progress?.Increment();
                    }

                    return;
                }

                // Parallel processing
                ParallelHelper.For(context, DrawingOperation.ProcessingPixels, 0, targetRectangle.Height, ProcessRow);

                #region Local Methods

                void ProcessRow(int y)
                {
                    // Scaling factors
                    float widthFactor = sourceRectangle.Width / (float)targetRectangle.Width;
                    float heightFactor = sourceRectangle.Height / (float)targetRectangle.Height;

                    IQuantizingSession session = quantizingSession;
                    IBitmapDataRowInternal rowSrc = source.GetRowCached((int)(y * heightFactor + sourceRectangle.Y));
                    IBitmapDataRowInternal rowDst = target.GetRowCached(y + targetRectangle.Y);

                    int targetLeft = targetRectangle.Left;
                    int sourceLeft = sourceRectangle.Left;
                    int targetWidth = targetRectangle.Width;
                    byte alphaThreshold = session.AlphaThreshold;
                    bool linearBlending = session.WorkingColorSpace == WorkingColorSpace.Linear;
                    for (int x = 0; x < targetWidth; x++)
                    {
                        Color32 colorSrc = rowSrc.DoGetColor32((int)(x * widthFactor + sourceLeft));

                        // fully solid source: overwrite
                        if (colorSrc.A == Byte.MaxValue)
                        {
                            rowDst.DoSetColor32(x + targetLeft, session.GetQuantizedColor(colorSrc));
                            continue;
                        }

                        // fully transparent source: skip
                        if (colorSrc.A == 0)
                            continue;

                        // source here has a partial transparency: we need to read the target color
                        int pos = x + targetLeft;
                        Color32 colorDst = rowDst.DoGetColor32(pos);

                        // non-transparent target: blending
                        if (colorDst.A != 0)
                        {
                            colorSrc = colorDst.A == Byte.MaxValue
                                // target pixel is fully solid: simple blending
                                ? colorSrc.BlendWithBackground(colorDst, linearBlending)
                                // both source and target pixels are partially transparent: complex blending
                                : colorSrc.BlendWith(colorDst, linearBlending);
                        }

                        // overwriting target color only if blended color has high enough alpha
                        if (colorSrc.A < alphaThreshold)
                            continue;

                        rowDst.DoSetColor32(pos, session.GetQuantizedColor(colorSrc));
                    }
                }

                #endregion
            }

            private void PerformResizeWithDithering(IQuantizingSession quantizingSession, IDitheringSession ditheringSession)
            {
                // Sequential processing
                if (ditheringSession.IsSequential || targetRectangle.Width < parallelThreshold >> ditheringScale)
                {
                    context.Progress?.New(DrawingOperation.ProcessingPixels, targetRectangle.Height);
                    for (int y = 0; y < targetRectangle.Height; y++)
                    {
                        if (context.IsCancellationRequested)
                            return;
                        ProcessRow(y);
                        context.Progress?.Increment();
                    }

                    return;
                }

                // Parallel processing
                ParallelHelper.For(context, DrawingOperation.ProcessingPixels, 0, targetRectangle.Height, ProcessRow);

                #region Local Methods

                void ProcessRow(int y)
                {
                    // Scaling factors
                    float widthFactor = sourceRectangle.Width / (float)targetRectangle.Width;
                    float heightFactor = sourceRectangle.Height / (float)targetRectangle.Height;

                    IDitheringSession session = ditheringSession;
                    IBitmapDataRowInternal rowSrc = source.GetRowCached((int)(y * heightFactor + sourceRectangle.Y));
                    IBitmapDataRowInternal rowDst = target.GetRowCached(y + targetRectangle.Y);

                    int targetLeft = targetRectangle.Left;
                    int sourceLeft = sourceRectangle.Left;
                    int targetWidth = targetRectangle.Width;
                    byte alphaThreshold = quantizingSession.AlphaThreshold;
                    bool lienarBlending = quantizingSession.WorkingColorSpace == WorkingColorSpace.Linear;
                    for (int x = 0; x < targetWidth; x++)
                    {
                        Color32 colorSrc = rowSrc.DoGetColor32((int)(x * widthFactor + sourceLeft));

                        // fully solid source: overwrite
                        if (colorSrc.A == Byte.MaxValue)
                        {
                            rowDst.DoSetColor32(x + targetLeft, session.GetDitheredColor(colorSrc, x, y));
                            continue;
                        }

                        // fully transparent source: skip
                        if (colorSrc.A == 0)
                            continue;

                        // source here has a partial transparency: we need to read the target color
                        int pos = x + targetLeft;
                        Color32 colorDst = rowDst.DoGetColor32(pos);

                        // non-transparent target: blending
                        if (colorDst.A != 0)
                        {
                            colorSrc = colorDst.A == Byte.MaxValue
                                // target pixel is fully solid: simple blending
                                ? colorSrc.BlendWithBackground(colorDst, lienarBlending)
                                // both source and target pixels are partially transparent: complex blending
                                : colorSrc.BlendWith(colorDst, lienarBlending);
                        }

                        // overwriting target color only if blended color has high enough alpha
                        if (colorSrc.A < alphaThreshold)
                            continue;

                        rowDst.DoSetColor32(pos, session.GetDitheredColor(colorSrc, x, y));
                    }
                }

                #endregion
            }

            #endregion

            #endregion
        }

        #endregion

        #region ResizingSessionInterpolated class

        [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Local",
            Justification = "Rectangles are not read-only to prevent creating defensive copies on older platform targets where getters are not read-only.")]
        private sealed class ResizingSessionInterpolated : IDisposable
        {
            #region Fields

            private readonly IAsyncContext context;
            private readonly IBitmapDataInternal source;
            private readonly IBitmapDataInternal target;
            private readonly KernelMap horizontalKernelMap;
            private readonly KernelMap verticalKernelMap;
            private readonly bool useLinearColorSpace;

            private Rectangle sourceRectangle;
            private Rectangle targetRectangle;
            private Array2D<PColorF> transposedFirstPassBuffer;
            private (int Top, int Bottom) currentWindow;

            #endregion

            #region Constructors

            internal ResizingSessionInterpolated(IAsyncContext context, IBitmapDataInternal source, IBitmapDataInternal target,
                Rectangle sourceRectangle, Rectangle targetRectangle, ScalingMode scalingMode, bool useLinearColorSpace)
            {
                this.context = context;
                this.source = source;
                this.target = target;
                this.sourceRectangle = sourceRectangle;
                this.targetRectangle = targetRectangle;
                this.useLinearColorSpace = useLinearColorSpace;
                if (scalingMode == ScalingMode.Auto)
                    scalingMode = GetAutoScalingMode(sourceRectangle.Size, targetRectangle.Size);

                (float radius, Func<float, float> interpolation) = scalingMode.GetInterpolation();
                horizontalKernelMap = KernelMap.Create(radius, interpolation, sourceRectangle.Width, targetRectangle.Width);
                verticalKernelMap = KernelMap.Create(radius, interpolation, sourceRectangle.Height, targetRectangle.Height);

                // Flipping height/width is intended (hence transposed). It contains target width and source height dimensions, which is also intended.
                transposedFirstPassBuffer = new Array2D<PColorF>(height: targetRectangle.Width, width: sourceRectangle.Height);
                currentWindow = (0, sourceRectangle.Height);

                CalculateFirstPassValues(currentWindow.Top, currentWindow.Bottom, true);
            }

            #endregion

            #region Methods

            #region Static Methods

            private static ScalingMode GetAutoScalingMode(Size sourceSize, Size targetSize)
            {
                Debug.Assert(sourceSize != targetSize, "Different sizes are expected");
                return targetSize.Width > sourceSize.Width || targetSize.Height > sourceSize.Height ? ScalingMode.MitchellNetravali : ScalingMode.Bicubic;
            }

            [SecurityCritical]
            private static unsafe void CopyColumns<T>(ref Array2D<T> array2d, int sourceIndex, int destIndex, int columnCount)
                where T : unmanaged
            {
                int elementSize = sizeof(T);
                int width = array2d.Width * elementSize;
                int srcOffset = sourceIndex * elementSize;
                int dstOffset = destIndex * elementSize;
                int count = columnCount * elementSize;

                fixed (T* ptr = array2d)
                {
                    byte* basePtr = (byte*)ptr;
                    for (int y = 0; y < array2d.Height; y++)
                    {
                        MemoryHelper.CopyMemory(basePtr + srcOffset, basePtr + dstOffset, count);
                        basePtr += width;
                    }
                }
            }

            #endregion

            #region Instance Methods

            #region Public Methods

            public void Dispose()
            {
                transposedFirstPassBuffer.Dispose();
                horizontalKernelMap.Dispose();
                verticalKernelMap.Dispose();
            }

            #endregion

            #region Internal Methods

            internal void PerformResize(IQuantizer? quantizer, IDitherer? ditherer)
            {
                if (quantizer == null)
                {
                    PerformResizeDirect();
                    return;
                }

                IReadableBitmapData initSource = targetRectangle.Size == target.Size
                    ? target
                    : target.Clip(targetRectangle);

                try
                {
                    Debug.Assert(!quantizer.InitializeReliesOnContent, "This method performs resize during quantization but the used quantizer would require two-pass processing");
                    context.Progress?.New(DrawingOperation.InitializingQuantizer);
                    using (IQuantizingSession quantizingSession = quantizer.Initialize(initSource, context))
                    {
                        if (context.IsCancellationRequested)
                            return;
                        if (quantizingSession == null)
                            throw new InvalidOperationException(Res.ImagingQuantizerInitializeNull);

                        // quantizing without dithering
                        if (ditherer == null)
                        {
                            PerformResizeWithQuantizer(quantizingSession);
                            return;
                        }

                        // quantizing with dithering
                        Debug.Assert(!ditherer.InitializeReliesOnContent, "This method performs resize during dithering but the used ditherer would require two-pass processing");

                        context.Progress?.New(DrawingOperation.InitializingDitherer);
                        using IDitheringSession ditheringSession = ditherer.Initialize(initSource, quantizingSession, context);
                        if (context.IsCancellationRequested)
                            return;
                        if (ditheringSession == null)
                            throw new InvalidOperationException(Res.ImagingDithererInitializeNull);

                        PerformResizeWithDithering(quantizingSession, ditheringSession);
                    }
                }
                finally
                {
                    if (!ReferenceEquals(initSource, target))
                        initSource.Dispose();
                }
            }

            internal void PerformResizeDirect()
            {
                if (target.IsFastPremultiplied() && !useLinearColorSpace)
                    PerformResizePremultipliedSrgb();
                else
                    PerformResizeDirectStraight();

                #region Local Methods

                void PerformResizeDirectStraight()
                {
                    ArraySection<PColorF> buffer = transposedFirstPassBuffer.Buffer;
                    ParallelHelper.For(context, DrawingOperation.ProcessingPixels, 0, targetRectangle.Height, y =>
                    {
                        ResizeKernel kernel = verticalKernelMap.GetKernel(y);
                        while (kernel.StartIndex + kernel.Length > currentWindow.Bottom)
                            Slide();

                        IBitmapDataRowInternal row = target.GetRowCached(y + targetRectangle.Y);
                        int topLine = kernel.StartIndex - currentWindow.Top;
                        int targetWidth = targetRectangle.Width;
                        int targetLeft = targetRectangle.Left;
                        byte alphaThreshold = target.PixelFormat.HasMultiLevelAlpha ? (byte)0 : target.AlphaThreshold;
                        bool linear = useLinearColorSpace;
                        for (int x = 0; x < targetWidth; x++)
                        {
                            // Destination color components
                            PColorF colorF = kernel.ConvolveWith(ref buffer, topLine + x * sourceRectangle.Height);

                            // fully transparent source: skip
                            if (colorF.A <= 0f)
                                continue;

                            Color32 colorSrc = colorF.ToStraight().ToColor32(linear);

                            // fully solid source: overwrite
                            if (colorSrc.A == Byte.MaxValue)
                            {
                                row.DoSetColor32(x + targetLeft, colorSrc);
                                continue;
                            }

                            // checking full transparency again (means almost zero colorF.A)
                            if (colorSrc.A == 0)
                                continue;

                            // source here has a partial transparency: we need to read the target color
                            int targetX = x + targetLeft;
                            Color32 colorDst = row.DoGetColor32(targetX);

                            // non-transparent target: blending
                            if (colorDst.A != 0)
                            {
                                colorSrc = colorDst.A == Byte.MaxValue
                                    // target pixel is fully solid: simple blending
                                    ? colorSrc.BlendWithBackground(colorDst, linear)
                                    // both source and target pixels are partially transparent: complex blending
                                    : colorSrc.BlendWith(colorDst, linear);
                            }

                            // overwriting target color only if blended color has high enough alpha
                            if (colorSrc.A < alphaThreshold)
                                continue;

                            row.DoSetColor32(targetX, colorSrc);
                        }
                    });
                }

                void PerformResizePremultipliedSrgb()
                {
                    ArraySection<PColorF> buffer = transposedFirstPassBuffer.Buffer;
                    ParallelHelper.For(context, DrawingOperation.ProcessingPixels, 0, targetRectangle.Height, y =>
                    {
                        ResizeKernel kernel = verticalKernelMap.GetKernel(y);
                        while (kernel.StartIndex + kernel.Length > currentWindow.Bottom)
                            Slide();

                        IBitmapDataRowInternal row = target.GetRowCached(y + targetRectangle.Y);
                        int topLine = kernel.StartIndex - currentWindow.Top;
                        int targetWidth = targetRectangle.Width;
                        int targetLeft = targetRectangle.Left;
                        for (int x = 0; x < targetWidth; x++)
                        {
                            // Destination color components
                            PColorF colorF = kernel.ConvolveWith(ref buffer, topLine + x * sourceRectangle.Height);

                            // fully transparent source: skip
                            if (colorF.A <= 0f)
                                continue;

                            Color32 colorSrc = colorF.ToPColor32(false);

                            // fully solid source: overwrite
                            if (colorSrc.A == Byte.MaxValue)
                            {
                                row.DoSetColor32Premultiplied(x + targetLeft, colorSrc);
                                continue;
                            }

                            // checking full transparency again (means almost zero colorF.A)
                            if (colorSrc.A == 0)
                                continue;

                            // source here has a partial transparency: we need to read the target color
                            Debug.Assert(colorSrc == colorSrc.AsValidPremultiplied());
                            int targetX = x + targetLeft;
                            Color32 colorDst = row.DoGetColor32Premultiplied(targetX);

                            // non-transparent target: blending
                            if (colorDst.A != 0)
                                colorSrc = colorSrc.BlendWithPremultipliedSrgb(colorDst);

                            row.DoSetColor32Premultiplied(targetX, colorSrc);
                        }
                    });
                }

                #endregion
            }

            #endregion

            #region Private Methods

            private void CalculateFirstPassValues(int top, int bottom, bool isInitializing)
            {
                #region Local Methods

                void ProcessRow(int y)
                {
                    var sourceRowBuffer = new ArraySection<PColorF>(sourceRectangle.Width);
                    IBitmapDataRowInternal sourceRow = source.GetRowCached(y + sourceRectangle.Top);
                    
                    if (useLinearColorSpace)
                    {
                        for (int x = 0; x < sourceRectangle.Width; x++)
                            sourceRowBuffer.GetElementReference(x) = sourceRow.DoGetColor32(x + sourceRectangle.Left).ToPColorF();
                    }
                    else
                    {
                        for (int x = 0; x < sourceRectangle.Width; x++)
                            sourceRowBuffer.GetElementReference(x) = new PColorF(sourceRow.DoGetColor32Premultiplied(x + sourceRectangle.Left), false);
                    }

                    int firstPassBaseIndex = y - currentWindow.Top;
                    for (int x = 0; x < targetRectangle.Width; x++)
                    {
                        ResizeKernel kernel = horizontalKernelMap.GetKernel(x);
                        transposedFirstPassBuffer.GetElementReference(x, firstPassBaseIndex) = kernel.ConvolveWith(ref sourceRowBuffer, kernel.StartIndex);
                    }

                    sourceRowBuffer.Release();
                }

                #endregion

                if (isInitializing)
                    ParallelHelper.For(context, DrawingOperation.ProcessingPixels, top, bottom, ProcessRow);
                else
                {
                    // We are already in a possibly parallel progress here so not allowing parallel and not reporting progress here
                    for (int y = top; y < bottom; y++)
                    {
                        if (context.IsCancellationRequested)
                            return;
                        ProcessRow(y);
                    }
                }
            }

            [SecuritySafeCritical]
            private void Slide()
            {
                var windowBandHeight = verticalKernelMap.MaxDiameter;
                int minY = currentWindow.Bottom - windowBandHeight;
                int maxY = Math.Min(minY + sourceRectangle.Height, sourceRectangle.Height);

                // Copying previous bottom band to the new top:
                CopyColumns(ref transposedFirstPassBuffer, sourceRectangle.Height - windowBandHeight, 0, windowBandHeight);
                currentWindow = (minY, maxY);

                // Calculating the remainder:
                CalculateFirstPassValues(currentWindow.Top + windowBandHeight, currentWindow.Bottom, false);
            }

            private void PerformResizeWithQuantizer(IQuantizingSession quantizingSession)
            {
                ArraySection<PColorF> buffer = transposedFirstPassBuffer.Buffer;
                ParallelHelper.For(context, DrawingOperation.ProcessingPixels, 0, targetRectangle.Height, y =>
                {
                    ResizeKernel kernel = verticalKernelMap.GetKernel(y);
                    while (kernel.StartIndex + kernel.Length > currentWindow.Bottom)
                        Slide();

                    IQuantizingSession session = quantizingSession;
                    IBitmapDataRowInternal row = target.GetRowCached(y + targetRectangle.Y);
                    int topLine = kernel.StartIndex - currentWindow.Top;
                    int targetWidth = targetRectangle.Width;
                    int targetLeft = targetRectangle.Left;
                    byte alphaThreshold = session.AlphaThreshold;
                    bool linear = useLinearColorSpace;
                    for (int x = 0; x < targetWidth; x++)
                    {
                        // Destination color components
                        PColorF colorF = kernel.ConvolveWith(ref buffer, topLine + x * sourceRectangle.Height);

                        // fully transparent source: skip
                        if (colorF.A <= 0f)
                            continue;

                        Color32 colorSrc = colorF.ToStraight().ToColor32(linear);

                        // fully solid source: overwrite
                        if (colorSrc.A == Byte.MaxValue)
                        {
                            row.DoSetColor32(x + targetLeft, session.GetQuantizedColor(colorSrc));
                            continue;
                        }

                        // checking full transparency again (means almost zero colorF.A)
                        if (colorSrc.A == 0)
                            continue;

                        // source here has a partial transparency: we need to read the target color
                        int targetX = x + targetLeft;
                        Color32 colorDst = row.DoGetColor32(targetX);

                        // non-transparent target: blending
                        if (colorDst.A != 0)
                        {
                            colorSrc = colorDst.A == Byte.MaxValue
                                // target pixel is fully solid: simple blending
                                ? colorSrc.BlendWithBackground(colorDst, linear)
                                // both source and target pixels are partially transparent: complex blending
                                : colorSrc.BlendWith(colorDst, linear);
                        }

                        // overwriting target color only if blended color has high enough alpha
                        if (colorSrc.A < alphaThreshold)
                            continue;

                        row.DoSetColor32(targetX, session.GetQuantizedColor(colorSrc));
                    }
                });
            }

            private void PerformResizeWithDithering(IQuantizingSession quantizingSession, IDitheringSession ditheringSession)
            {
                // Sequential processing (ignoring threshold, always parallel if possible)
                if (ditheringSession.IsSequential)
                {
                    for (int y = 0; y < targetRectangle.Height; y++)
                        ProcessRow(y);
                    return;
                }

                // Parallel processing
                ParallelHelper.For(context, DrawingOperation.ProcessingPixels, 0, targetRectangle.Height, ProcessRow);

                #region Local Methods

                void ProcessRow(int y)
                {
                    ArraySection<PColorF> buffer = transposedFirstPassBuffer.Buffer;
                    ResizeKernel kernel = verticalKernelMap.GetKernel(y);
                    while (kernel.StartIndex + kernel.Length > currentWindow.Bottom)
                        Slide();

                    IDitheringSession session = ditheringSession;
                    IBitmapDataRowInternal row = target.GetRowCached(y + targetRectangle.Y);
                    int topLine = kernel.StartIndex - currentWindow.Top;
                    int targetWidth = targetRectangle.Width;
                    int targetLeft = targetRectangle.Left;
                    byte alphaThreshold = quantizingSession.AlphaThreshold;
                    bool linear = useLinearColorSpace;
                    for (int x = 0; x < targetWidth; x++)
                    {
                        // Destination color components
                        PColorF colorF = kernel.ConvolveWith(ref buffer, topLine + x * sourceRectangle.Height);

                        // fully transparent source: skip
                        if (colorF.A <= 0f)
                            continue;

                        Color32 colorSrc = colorF.ToStraight().ToColor32(linear);

                        // fully solid source: overwrite
                        if (colorSrc.A == Byte.MaxValue)
                        {
                            row.DoSetColor32(x + targetLeft, session.GetDitheredColor(colorSrc, x, y));
                            continue;
                        }

                        // checking full transparency again (means almost zero colorF.A)
                        if (colorSrc.A == 0)
                            continue;

                        // source here has a partial transparency: we need to read the target color
                        int targetX = x + targetLeft;
                        Color32 colorDst = row.DoGetColor32(targetX);

                        // non-transparent target: blending
                        if (colorDst.A != 0)
                        {
                            colorSrc = colorDst.A == Byte.MaxValue
                                // target pixel is fully solid: simple blending
                                ? colorSrc.BlendWithBackground(colorDst, linear)
                                // both source and target pixels are partially transparent: complex blending
                                : colorSrc.BlendWith(colorDst, linear);
                        }

                        // overwriting target color only if blended color has high enough alpha
                        if (colorSrc.A < alphaThreshold)
                            continue;

                        row.DoSetColor32(targetX, session.GetDitheredColor(colorSrc, x, y));
                    }
                }

                #endregion
            }

            #endregion

            #endregion

            #endregion
        }

        #endregion

        #region KernelMap class

        /// <summary>
        /// Represents the kernel map for resizing using a specific interpolation function.
        /// </summary>
        private class KernelMap : IDisposable
        {
            #region Nested classes

            #region PeriodicKernelMap class

            private sealed class PeriodicKernelMap : KernelMap
            {
                #region Fields

                private readonly int period;
                private readonly int cornerInterval;

                #endregion

                #region Constructors

                public PeriodicKernelMap(int sourceLength, int destinationLength, float ratio, float scale, int radius, int period, int cornerInterval)
                    : base(sourceLength, destinationLength, (cornerInterval << 1) + period, ratio, scale, radius)
                {
                    this.cornerInterval = cornerInterval;
                    this.period = period;
                }

                #endregion

                #region Methods

                protected override void Initialize(Func<float, float> interpolation)
                {
                    // Building top corner data + one period of the mosaic data:
                    int startOfFirstRepeatedMosaic = cornerInterval + period;
                    for (int i = 0; i < startOfFirstRepeatedMosaic; i++)
                        kernels[i] = BuildKernel(interpolation, i, i);

                    // Copying mosaics:
                    int bottomStartDest = targetLength - cornerInterval;
                    for (int i = startOfFirstRepeatedMosaic; i < bottomStartDest; i++)
                    {
                        float center = (i + 0.5f) * ratio - 0.5f;
                        int left = (int)(center - radius).TolerantCeiling(tolerance);
                        ResizeKernel kernel = kernels[i - period];
                        if (left + kernel.Length > sourceLength)
                        {
                            Debug.Fail("Is tolerance too small?");
                            left -= 1;
                        }
                        kernels[i] = kernel.Slide(left);
                    }

                    // Building bottom corner data:
                    int bottomStartData = cornerInterval + period;
                    for (int i = 0; i < cornerInterval; i++)
                        kernels[bottomStartDest + i] = BuildKernel(interpolation, bottomStartDest + i, bottomStartData + i);
                }

                #endregion
            }

            #endregion

            #endregion

            #region Constants

            private const float tolerance = 1e-4f;

            #endregion

            #region Fields

            private readonly int sourceLength;
            private readonly int targetLength;
            private readonly float ratio;
            private readonly float scale;
            private readonly int radius;
            private readonly ResizeKernel[] kernels;

            private Array2D<float> data;

            #endregion

            #region Properties

            /// <summary>
            /// Gets the maximum diameter of the kernels.
            /// </summary>
            internal int MaxDiameter { get; }

            #endregion

            #region Constructors

            private KernelMap(int sourceLength, int targetLength, int bufferHeight, float ratio, float scale, int radius)
            {
                this.ratio = ratio;
                this.scale = scale;
                this.radius = radius;
                this.sourceLength = sourceLength;
                this.targetLength = targetLength;
                MaxDiameter = (radius << 1) + 1;
                data = new Array2D<float>(bufferHeight, MaxDiameter);
                kernels = new ResizeKernel[targetLength];
            }

            #endregion

            #region Methods

            #region Static Methods

            internal static KernelMap Create(float radius, Func<float, float> interpolation, int sourceSize, int targetSize)
            {
                #region Local Methods

                static int GreatestCommonDivisor(int a, int b)
                {
                    while (b != 0)
                    {
                        int temp = b;
                        b = a % b;
                        a = temp;
                    }

                    return a;
                }

                static int LeastCommonMultiple(int a, int b) => a / GreatestCommonDivisor(a, b) * b;

                #endregion

                float ratio = (float)sourceSize / targetSize;
                float scale = Math.Max(1f, ratio);
                int scaledRadius = (int)(scale * radius).TolerantCeiling(tolerance);

                // the length of the period in a repeating kernel map
                int period = LeastCommonMultiple(sourceSize, targetSize) / sourceSize;

                float center0 = (ratio - 1f) * 0.5f;
                float firstNonNegativeLeftVal = (scaledRadius - center0 - 1f) / ratio;

                // The number of rows building a "stairway" at the top and the bottom of the kernel map
                // corresponding to the corners of the image.
                // If we do not normalize the kernel values, these rows also fit the periodic logic,
                // however, it's just simpler to calculate them separately.
                int cornerInterval = (int)firstNonNegativeLeftVal.TolerantCeiling(tolerance);

                // If firstNonNegativeLeftVal was an integral value, we need firstNonNegativeLeftVal+1
                // instead of Ceiling:
                if (firstNonNegativeLeftVal.TolerantEquals(cornerInterval, tolerance))
                    cornerInterval += 1;

                // If 'cornerInterval' is too big compared to 'period', we can't apply the periodic optimization.
                // If we don't have at least 2 periods, we go with the basic implementation:
                bool hasAtLeast2Periods = 2 * (cornerInterval + period) < targetSize;

                KernelMap result = hasAtLeast2Periods
                        ? new PeriodicKernelMap(sourceSize, targetSize, ratio, scale, scaledRadius, period, cornerInterval)
                        : new KernelMap(sourceSize, targetSize, targetSize, ratio, scale, scaledRadius);

                result.Initialize(interpolation);
                return result;
            }

            #endregion

            #region Instance Methods

            #region Public Methods

            public void Dispose() => data.Dispose();

            #endregion

            #region Internal Methods

            /// <summary>
            /// Returns a <see cref="ResizeKernel"/> for an index value between 0 and targetLength - 1.
            /// </summary>
            internal ref ResizeKernel GetKernel(int index) => ref kernels[index];

            #endregion

            #region Protected Methods

            protected virtual void Initialize(Func<float, float> interpolation)
            {
                for (int i = 0; i < targetLength; i++)
                    kernels[i] = BuildKernel(interpolation, i, i);
            }

            #endregion

            #region Private Methods

            /// <summary>
            /// Builds a <see cref="ResizeKernel"/> for the row <paramref name="destRowIndex"/> (in <see cref="kernels"/>)
            /// referencing the data at row <paramref name="dataRowIndex"/> within <see cref="data"/>, so the data reusable by other data rows.
            /// </summary>
            [SecuritySafeCritical]
            private unsafe ResizeKernel BuildKernel(Func<float, float> interpolation, int destRowIndex, int dataRowIndex)
            {
                float center = (destRowIndex + 0.5f) * ratio - 0.5f;

                int left = (int)(center - radius).TolerantCeiling(tolerance);
                if (left < 0)
                    left = 0;

                int right = (int)(center + radius).TolerantFloor(tolerance);
                if (right > sourceLength - 1)
                    right = sourceLength - 1;

                int length = right - left + 1;
                ArraySection<float> buffer = data.Buffer.Slice(dataRowIndex * data.Width);
                ResizeKernel kernel = new ResizeKernel(buffer, left, length);

                float* weights = stackalloc float[MaxDiameter];
                float sum = 0;

                for (int j = left; j <= right; j++)
                {
                    float value = interpolation.Invoke((j - center) / scale);
                    sum += value;

                    weights[j - left] = value;
                }

                // Normalizing
                if (sum > 0)
                {
                    for (int j = 0; j < kernel.Length; j++)
                    {
                        ref float value = ref weights[j];
                        value /= sum;
                    }
                }

                // this fills always the values between 0..length, and not left..length, which is intended
                for (int i = 0; i < length; i++)
                    buffer[i] = weights[i];

                return kernel;
            }

            #endregion

            #endregion

            #endregion
        }

        #endregion

        #region ResizeKernel struct

        /// <summary>
        /// Points to a collection of weights allocated in <see cref="ResizingSessionInterpolated"/>.
        /// </summary>
        private struct ResizeKernel
        {
            #region Fields

            [SuppressMessage("Style", "IDE0044:Add readonly modifier",
                Justification = "False alarm, dispose mutates instance, and if it was read-only, a defensive copy would be created")]
            [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Local")]
            private ArraySection<float> kernelBuffer;

            #endregion

            #region Properties

            internal int StartIndex { get; }
            internal int Length { get; }

            #endregion

            #region Constructors

            internal ResizeKernel(ArraySection<float> kernelMapBuffer, int startIndex, int length)
            {
                kernelBuffer = kernelMapBuffer;
                StartIndex = startIndex;
                Length = length;
            }

            #endregion

            #region Methods

            /// <summary>
            /// Computes the sum of colors weighted by weight values, pointed by this <see cref="ResizeKernel"/> instance.
            /// </summary>
            internal PColorF ConvolveWith(ref ArraySection<PColorF> colors, int startIndex)
            {
                PColorF result = default;

                for (int i = 0; i < Length; i++)
                {
                    float weight = kernelBuffer[i];
                    ref PColorF c = ref colors.GetElementReference(startIndex + i);
                    result += c * weight;
                }

                return result;
            }

            /// <summary>
            /// Reinterprets the origin of the current kernel adjusting the destination column index
            /// </summary>
            internal ResizeKernel Slide(int newStartIndex) => new ResizeKernel(kernelBuffer, newStartIndex, Length);

            public override string ToString() => kernelBuffer.IsNull ? PublicResources.Null
                : kernelBuffer.Length < StartIndex + Length ? kernelBuffer.Slice(StartIndex).Join("; ")
                : kernelBuffer.Slice(StartIndex, Length).Join("; ");

            #endregion
        }

        #endregion

        #endregion
    }
}
