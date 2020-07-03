#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ReadWriteBitmapDataExtensions.Resize.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2020 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution. If not, then this file is considered as
//  an illegal copy.
//
//  Unauthorized copying of this file, via any medium is strictly prohibited.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;

using KGySoft.Collections;
using KGySoft.CoreLibraries;

#endregion

namespace KGySoft.Drawing.Imaging
{
    /// <summary>
    /// Contains GDI-independent resizing logic
    /// Credit to ImageSharp resize, on which this code partially based on (see https://github.com/SixLabors/ImageSharp/tree/master/src/ImageSharp/Processing/Processors/Transforms/Resize)
    /// ImageSharp is under the GNU Affero General Public License v3.0, which is available here: https://www.gnu.org/licenses/agpl-3.0.html
    /// </summary>
    public static partial class ReadWriteBitmapDataExtensions
    {
        #region Nested Types
        
        #region ResizingSession class

        /// <summary>
        /// Represents a resizing session.
        /// </summary>
        private sealed class ResizingSession : IDisposable
        {
            #region Fields

            private readonly IBitmapDataInternal source;
            private readonly IBitmapDataInternal target;
            private readonly KernelMap horizontalKernelMap;
            private readonly KernelMap verticalKernelMap;

            private Rectangle sourceRectangle;
            private Rectangle targetRectangle;
            private Array2D<ColorF> transposedFirstPassBuffer;
            private (int Top, int Bottom) currentWindow;

            #endregion

            #region Constructors

            internal ResizingSession(IReadableBitmapData source, IReadWriteBitmapData target, Rectangle sourceRectangle, Rectangle targetRectangle, ScalingMode scalingMode)
            {
                this.source = source as IBitmapDataInternal ?? new BitmapDataWrapper(source, true, false);
                this.target = target as IBitmapDataInternal ?? new BitmapDataWrapper(target, true, true);
                this.sourceRectangle = sourceRectangle;
                this.targetRectangle = targetRectangle;
                if (scalingMode == ScalingMode.Auto)
                    scalingMode = GetAutoScalingMode(sourceRectangle.Size, targetRectangle.Size);

                (float radius, Func<float, float> interpolation) = scalingMode.GetInterpolation();
                horizontalKernelMap = KernelMap.Create(radius, interpolation, sourceRectangle.Width, targetRectangle.Width);
                verticalKernelMap = KernelMap.Create(radius, interpolation, sourceRectangle.Height, targetRectangle.Height);

                // Flipping height/width is intended (hence transposed). It contains target width and source height dimensions, which is also intended.
                transposedFirstPassBuffer = new Array2D<ColorF>(height: targetRectangle.Width, width: sourceRectangle.Height);
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

            internal void DoResizeDirect(int top, int bottom)
            {
                ArraySection<ColorF> buffer = transposedFirstPassBuffer.Buffer;
                ParallelHelper.For(top, bottom, y =>
                {
                    var targetRowBuffer = new ArraySection<ColorF>(targetRectangle.Width);

                    ResizeKernel kernel = verticalKernelMap.GetKernel(y - targetRectangle.Y);
                    while (kernel.StartIndex + kernel.Length > currentWindow.Bottom)
                        Slide();

                    int topLine = kernel.StartIndex - currentWindow.Top;

                    int targetWidth = targetRectangle.Width;
                    for (int x = 0; x < targetWidth; x++)
                    {
                        // Destination color components
                        targetRowBuffer.GetElementReference(x) = kernel.ConvolveWith(ref buffer, topLine + x * sourceRectangle.Height);
                    }

                    IBitmapDataRowInternal row = target.GetRow(y);
                    int targetLeft = targetRectangle.Left;
                    for (int x = 0; x < targetWidth; x++)
                    {
                        ref ColorF refColorF = ref targetRowBuffer.GetElementReference(x);

                        // fully transparent source: skip
                        if (refColorF.A <= 0f)
                            continue;

                        Color32 colorSrc = refColorF.ToColor32();

                        // fully solid source: overwrite
                        if (colorSrc.A == Byte.MaxValue)
                        {
                            row.DoSetColor32(x + targetLeft, colorSrc);
                            continue;
                        }

                        // checking full transparency again (means almost zero refColorF.A)
                        if (colorSrc.A == 0)
                            continue;

                        // source here has a partial transparency: we need to read the target color
                        int targetX = x + targetLeft;
                        Color32 colorDst = row.DoGetColor32(targetX);

                        // fully transparent target: we can overwrite with source
                        if (colorDst.A == 0)
                        {
                            row.DoSetColor32(targetX, colorSrc);
                            continue;
                        }

                        colorSrc = colorDst.A == Byte.MaxValue
                            // target pixel is fully solid: simple blending
                            ? colorSrc.BlendWithBackground(colorDst)
                            // both source and target pixels are partially transparent: complex blending
                            : colorSrc.BlendWith(colorDst);

                        row.DoSetColor32(targetX, colorSrc);
                    }

                    targetRowBuffer.Release();
                });
            }

            internal void DoResizeWithDithering(int top, int bottom, IDitherer ditherer)
            {
                // creating a temporal buffer for the non-dithered result because a quantizer/ditherer may need the resized image source
                using var result = BitmapDataFactory.CreateManagedBitmapData(targetRectangle.Size);
                throw new NotImplementedException("TODO: do not blend here, target buffer is always transparent");
                ArraySection<ColorF> buffer = transposedFirstPassBuffer.Buffer;

                ParallelHelper.For(top, bottom, y =>
                {
                    var targetRowBuffer = new ArraySection<ColorF>(targetRectangle.Width);

                    ResizeKernel kernel = verticalKernelMap.GetKernel(y - targetRectangle.Y);
                    while (kernel.StartIndex + kernel.Length > currentWindow.Bottom)
                        Slide();

                    int topLine = kernel.StartIndex - currentWindow.Top;

                    for (int x = 0; x < targetRectangle.Width; x++)
                    {
                        // Destination color components
                        targetRowBuffer.GetElementReference(x) = kernel.ConvolveWith(ref buffer, topLine + x * sourceRectangle.Height);
                    }

                    IBitmapDataRowInternal row = result.GetRow(y - targetRectangle.Y);
                    int targetWidth = targetRectangle.Width;
                    for (int x = 0; x < targetWidth; x++)
                    {
                        ref ColorF refColorF = ref targetRowBuffer.GetElementReference(x);

                        // fully transparent source: skip
                        if (refColorF.A <= 0f)
                            continue;

                        Color32 colorSrc = refColorF.ToColor32();

                        // fully solid source: overwrite
                        if (colorSrc.A == Byte.MaxValue)
                        {
                            row.DoSetColor32(x, colorSrc);
                            continue;
                        }

                        // checking full transparency again (means almost zero refColorF.A)
                        if (colorSrc.A == 0)
                            continue;

                        // source here has a partial transparency: we need to read the target color
                        Color32 colorDst = row.DoGetColor32(x);

                        // fully transparent target: we can overwrite with source
                        if (colorDst.A == 0)
                        {
                            row.DoSetColor32(x, colorSrc);
                            continue;
                        }

                        colorSrc = colorDst.A == Byte.MaxValue
                            // target pixel is fully solid: simple blending
                            ? colorSrc.BlendWithBackground(colorDst)
                            // both source and target pixels are partially transparent: complex blending
                            : colorSrc.BlendWith(colorDst);

                        row.DoSetColor32(x, colorSrc);
                    }

                    targetRowBuffer.Release();
                });

                // Drawing result to actual target with dithering
                target.DrawBitmapData(result, targetRectangle.Location, ditherer);
            }

            internal void DoResizeWithDithering2(int top, int bottom, IDitherer ditherer)
            {
                // TODO: this is now drawing directly to target but initialized with a potentially too large bitmapData
                // TODO: use this with tolerant ditherer, initialize with dummy target with correct backcolor/alphathreshold
                //// creating a temporal buffer for the non-dithered result because a quantizer/ditherer may need the resized image source
                //using var result = new BitmapDataBuffer(targetRectangle.Size);
                ArraySection<ColorF> buffer = transposedFirstPassBuffer.Buffer;
                IQuantizer quantizer = PredefinedColorsQuantizer.FromBitmapData(target);
                using var quantizingSession = quantizer.Initialize(target);
                using var ditheringSession = ditherer.Initialize(target, quantizingSession);

                ParallelHelper.For(top, bottom, y =>
                {
                    var targetRowBuffer = new ArraySection<ColorF>(targetRectangle.Width);

                    ResizeKernel kernel = verticalKernelMap.GetKernel(y - targetRectangle.Y);
                    while (kernel.StartIndex + kernel.Length > currentWindow.Bottom)
                        Slide();

                    int topLine = kernel.StartIndex - currentWindow.Top;

                    for (int x = 0; x < targetRectangle.Width; x++)
                    {
                        // Destination color components
                        targetRowBuffer.GetElementReference(x) = kernel.ConvolveWith(ref buffer, topLine + x * sourceRectangle.Height);
                    }

                    // TODO
                    //IBitmapDataRowInternal row = result.GetRow(y - targetRectangle.Y);
                    IBitmapDataRowInternal row = target.GetRow(y);
                    int targetWidth = targetRectangle.Width;
                    int targetLeft = targetRectangle.Left;
                    for (int x = 0; x < targetWidth; x++)
                    {
                        ref ColorF refColorF = ref targetRowBuffer.GetElementReference(x);

                        // fully transparent source: skip
                        if (refColorF.A <= 0f)
                            continue;

                        Color32 colorSrc = refColorF.ToColor32();
                        int targetX = x + targetLeft;

                        // fully solid source: overwrite
                        if (colorSrc.A == Byte.MaxValue)
                        {
                            row.DoSetColor32(targetX, ditheringSession.GetDitheredColor(colorSrc, targetX, y));
                            continue;
                        }

                        // checking full transparency again (means almost zero refColorF.A)
                        if (colorSrc.A == 0)
                            continue;

                        // source here has a partial transparency: we need to read the target color
                        Color32 colorDst = row.DoGetColor32(targetX);

                        // fully transparent target: we can overwrite with source
                        if (colorDst.A == 0)
                        {
                            row.DoSetColor32(targetX, ditheringSession.GetDitheredColor(colorSrc, targetX, y));
                            continue;
                        }

                        colorSrc = colorDst.A == Byte.MaxValue
                            // target pixel is fully solid: simple blending
                            ? colorSrc.BlendWithBackground(colorDst)
                            // both source and target pixels are partially transparent: complex blending
                            : colorSrc.BlendWith(colorDst);

                        row.DoSetColor32(targetX, ditheringSession.GetDitheredColor(colorSrc, targetX, y));
                    }

                    targetRowBuffer.Release();
                });

                // TODO:
                //// Drawing result to actual target with dithering
                //target.DrawBitmapData(result, targetRectangle.Location, ditherer);
            }

            #endregion

            #region Private Methods

            private void CalculateFirstPassValues(int top, int bottom, bool parallel)
            {
                #region Local Methods
                
                void ProcessRow(int y)
                {
                    var sourceRowBuffer = new ArraySection<ColorF>(sourceRectangle.Width);
                    IBitmapDataRowInternal sourceRow = source.GetRow(y + sourceRectangle.Top);

                    for (int x = 0; x < sourceRectangle.Width; x++)
                        sourceRowBuffer.GetElementReference(x) = new ColorF(sourceRow.DoGetColor32(x + sourceRectangle.Left));

                    int firstPassBaseIndex = y - currentWindow.Top;
                    for (int x = 0; x < targetRectangle.Width; x++)
                    {
                        ResizeKernel kernel = horizontalKernelMap.GetKernel(x);
                        transposedFirstPassBuffer.GetElementReference(x, firstPassBaseIndex) = kernel.ConvolveWith(ref sourceRowBuffer, kernel.StartIndex);
                    }

                    sourceRowBuffer.Release();
                }

                #endregion

                if (parallel)
                    ParallelHelper.For(top, bottom, ProcessRow);
                else
                {
                    for (int y = top; y < bottom; y++)
                        ProcessRow(y);
                }
            }

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
                        float center = (i + 0.5f) * Ratio - 0.5f;
                        int left = (int)(center - radius).TolerantCeiling();
                        ResizeKernel kernel = kernels[i - period];
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

            #region Fields

            private readonly int sourceLength;
            private readonly int targetLength;
            private readonly float scale;
            private readonly int radius;
            private readonly ResizeKernel[] kernels;

            private Array2D<float> data;

            #endregion

            #region Properties

            internal float Ratio { get; }

            /// <summary>
            /// Gets the maximum diameter of the kernels.
            /// </summary>
            internal int MaxDiameter { get; }

            #endregion

            #region Constructors

            private KernelMap(int sourceLength, int targetLength, int bufferHeight, float ratio, float scale, int radius)
            {
                this.Ratio = ratio;
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
                int scaledRadius = (int)(scale * radius).TolerantCeiling();

                // the length of the period in a repeating kernel map
                int period = LeastCommonMultiple(sourceSize, targetSize) / sourceSize;

                float center0 = (ratio - 1f) * 0.5f;
                float firstNonNegativeLeftVal = (scaledRadius - center0 - 1f) / ratio;

                // The number of rows building a "stairway" at the top and the bottom of the kernel map
                // corresponding to the corners of the image.
                // If we do not normalize the kernel values, these rows also fit the periodic logic,
                // however, it's just simpler to calculate them separately.
                int cornerInterval = (int)firstNonNegativeLeftVal.TolerantCeiling();

                // If firstNonNegativeLeftVal was an integral value, we need firstNonNegativeLeftVal+1
                // instead of Ceiling:
                if (firstNonNegativeLeftVal.TolerantEquals(cornerInterval))
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
            private unsafe ResizeKernel BuildKernel(Func<float, float> interpolation, int destRowIndex, int dataRowIndex)
            {
                float center = (destRowIndex + 0.5f) * Ratio - 0.5f;

                int left = (int)(center - radius).TolerantCeiling();
                if (left < 0)
                    left = 0;

                int right = (int)(center + radius).TolerantFloor();
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
        /// Points to a collection of weights allocated in <see cref="ResizingSession"/>.
        /// </summary>
        private struct ResizeKernel
        {
            #region Fields

            [SuppressMessage("Style", "IDE0044:Add readonly modifier",
                Justification = "False alarm, dispose mutates instance, and if it was read-only, a defensive copy would be created")]
            private ArraySection<float> kernelBuffer;

            #endregion

            #region Properties

            internal int StartIndex { get; }

            internal int Length { get; }

            #endregion

            #region Constructors

            internal ResizeKernel(ArraySection<float> kernelMapBuffer, int startIndex, int length)
            {
                this.kernelBuffer = kernelMapBuffer;
                StartIndex = startIndex;
                Length = length;
            }

            #endregion

            #region Methods

            /// <summary>
            /// Computes the sum of colors weighted by weight values, pointed by this <see cref="ResizeKernel"/> instance.
            /// </summary>
            internal ColorF ConvolveWith(ref ArraySection<ColorF> colors, int startIndex)
            {
                ColorF result = default;

                for (int i = 0; i < Length; i++)
                {
                    float weight = kernelBuffer[i];
                    ref ColorF c = ref colors.GetElementReference(startIndex + i);
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

        #region Methods

        private static void ResizeNearestNeighborDirect(IBitmapDataInternal source, IBitmapDataInternal target, Rectangle sourceRectangle, Rectangle targetRectangle)
        {
            // Scaling factors
            float widthFactor = sourceRectangle.Width / (float)targetRectangle.Width;
            float heightFactor = sourceRectangle.Height / (float)targetRectangle.Height;

            #region Local Methods

            void ProcessRowStraight(int y)
            {
                IBitmapDataRowInternal rowSrc = source.GetRow((int)((y - targetRectangle.Y) * heightFactor + sourceRectangle.Y));
                IBitmapDataRowInternal rowDst = target.GetRow(y);

                int targetLeft = targetRectangle.Left;
                int targetRight = targetRectangle.Right;
                int sourceLeft = sourceRectangle.Left;
                for (int x = targetLeft; x < targetRight; x++)
                {
                    Color32 colorSrc = rowSrc.DoGetColor32((int)((x - targetLeft) * widthFactor + sourceLeft));

                    // fully transparent source: skip
                    if (colorSrc.A == 0)
                        continue;

                    // fully solid source: overwrite
                    if (colorSrc.A == Byte.MaxValue)
                    {
                        rowDst.DoSetColor32(x, colorSrc);
                        continue;
                    }

                    // source here has a partial transparency: we need to read the target color
                    Color32 colorDst = rowDst.DoGetColor32(x);

                    // fully transparent target: we can overwrite with source
                    if (colorDst.A == 0)
                    {
                        rowDst.DoSetColor32(x, colorSrc);
                        continue;
                    }

                    colorSrc = colorDst.A == Byte.MaxValue
                        // target pixel is fully solid: simple blending
                        ? colorSrc.BlendWithBackground(colorDst)
                        // both source and target pixels are partially transparent: complex blending
                        : colorSrc.BlendWith(colorDst);

                    rowDst.DoSetColor32(x, colorSrc);
                }
            }

            void ProcessRowPremultiplied(int y)
            {
                IBitmapDataRowInternal rowSrc = source.GetRow((int)((y - targetRectangle.Y) * heightFactor + sourceRectangle.Y));
                IBitmapDataRowInternal rowDst = target.GetRow(y);
                bool isPremultipliedSource = source.PixelFormat == PixelFormat.Format32bppPArgb;

                int targetLeft = targetRectangle.Left;
                int targetRight = targetRectangle.Right;
                int sourceLeft = sourceRectangle.Left;
                for (int x = targetLeft; x < targetRight; x++)
                {
                    Color32 colorSrc = isPremultipliedSource
                        ? rowSrc.DoReadRaw<Color32>((int)((x - targetLeft) * widthFactor + sourceLeft))
                        : rowSrc.DoGetColor32((int)((x - targetLeft) * widthFactor + sourceLeft)).ToPremultiplied();

                    // fully transparent source: skip
                    if (colorSrc.A == 0)
                        continue;

                    // fully solid source: overwrite
                    if (colorSrc.A == Byte.MaxValue)
                    {
                        rowDst.DoWriteRaw(x, colorSrc);
                        continue;
                    }

                    // source here has a partial transparency: we need to read the target color
                    Color32 colorDst = rowDst.DoReadRaw<Color32>(x);

                    // fully transparent target: we can overwrite with source
                    if (colorDst.A == 0)
                    {
                        rowDst.DoWriteRaw(x, colorSrc);
                        continue;
                    }

                    rowDst.DoWriteRaw(x, colorSrc.BlendWithPremultiplied(colorDst));
                }
            }

            #endregion

            Action<int> processRow = target.PixelFormat == PixelFormat.Format32bppPArgb
                ? ProcessRowPremultiplied
                : (Action<int>)ProcessRowStraight;

            // Sequential processing
            if (targetRectangle.Width < parallelThreshold)
            {
                for (int y = targetRectangle.Top; y < targetRectangle.Bottom; y++)
                    processRow.Invoke(y);
                return;
            }

            // Parallel processing
            ParallelHelper.For(targetRectangle.Top, targetRectangle.Bottom, processRow);
        }

        private static void ResizeNearestNeighborWithDithering(IBitmapDataInternal source, IBitmapDataInternal target, Rectangle sourceRectangle, Rectangle targetRectangle, IDitherer ditherer)
        {
            // TODO - this is the intolerant ditherer version. No blending is needed here as target is always transparent
            // TODO: Use direct version if possible - see session
            throw new NotImplementedException("TODO");
            using var result = BitmapDataFactory.CreateManagedBitmapData(targetRectangle.Size);

            // Scaling factors
            float widthFactor = sourceRectangle.Width / (float)targetRectangle.Width;
            float heightFactor = sourceRectangle.Height / (float)targetRectangle.Height;

            #region Local Methods

            void ProcessRow(int y)
            {
                IBitmapDataRowInternal rowSrc = source.GetRow((int)(y * heightFactor + sourceRectangle.Y));
                IBitmapDataRowInternal rowDst = result.GetRow(y);

                int sourceLeft = sourceRectangle.Left;
                int width = result.Width;
                for (int x = 0; x < width; x++)
                {
                    Color32 colorSrc = rowSrc.DoGetColor32((int)(x * widthFactor + sourceLeft));

                    // fully transparent source: skip
                    if (colorSrc.A == 0)
                        continue;

                    // fully solid source: overwrite
                    if (colorSrc.A == Byte.MaxValue)
                    {
                        rowDst.DoSetColor32(x, colorSrc);
                        continue;
                    }

                    // source here has a partial transparency: we need to read the target color
                    Color32 colorDst = rowDst.DoGetColor32(x);

                    // fully transparent target: we can overwrite with source
                    if (colorDst.A == 0)
                    {
                        rowDst.DoSetColor32(x, colorSrc);
                        continue;
                    }

                    colorSrc = colorDst.A == Byte.MaxValue
                        // target pixel is fully solid: simple blending
                        ? colorSrc.BlendWithBackground(colorDst)
                        // both source and target pixels are partially transparent: complex blending
                        : colorSrc.BlendWith(colorDst);

                    rowDst.DoSetColor32(x, colorSrc);
                }
            }

            #endregion

            // Sequential processing
            if (targetRectangle.Width < parallelThreshold)
            {
                for (int y = 0; y < targetRectangle.Height; y++)
                    ProcessRow(y);
            }
            // Parallel processing
            else
                ParallelHelper.For(0, targetRectangle.Height, ProcessRow);

            // Drawing result to actual target with dithering
            target.DrawBitmapData(result, targetRectangle.Location, ditherer);
        }

        #endregion
    }
}
