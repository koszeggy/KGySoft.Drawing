#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: WritableBitmapDataExtensions.Resize.cs
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
using System.Drawing;

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
    internal static partial class WritableBitmapDataExtensions
    {
        #region ResizingSession class

        /// <summary>
        /// Represents a resizing session.
        /// </summary>
        private sealed class ResizingSession : IDisposable
        {
            #region Fields

            private readonly IReadableBitmapData source;
            private readonly IWritableBitmapData target;
            private readonly KernelMap horizontalKernelMap;
            private readonly KernelMap verticalKernelMap;

            private Rectangle sourceRectangle;
            private Rectangle targetRectangle;
            private Array2D<ColorF> transposedFirstPassBuffer;
            private (int Top, int Bottom) currentWindow;

            #endregion

            #region Constructors

            internal ResizingSession(IReadableBitmapData source, IWritableBitmapData target, Rectangle sourceRectangle, Rectangle targetRectangle, ScalingMode scalingMode)
            {
                this.source = source;
                this.target = target;
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
                // TODO: handle ScalingMode.Auto
                return ScalingMode.Bicubic;
            }

            private static unsafe void CopyColumns<T>(ref Array2D<T> array2d, int sourceIndex, int destIndex, int columnCount)
                where T : unmanaged
            {
                int elementSize = sizeof(T);
                int width = array2d.Width * elementSize;
                int sOffset = sourceIndex * elementSize;
                int dOffset = destIndex * elementSize;
                int count = columnCount * elementSize;

                fixed (T* ptr = array2d)
                {
                    byte* basePtr = (byte*)ptr;
                    for (int y = 0; y < array2d.Height; y++)
                    {
                        MemoryHelper.CopyMemory(new IntPtr(basePtr + dOffset), new IntPtr(basePtr + sOffset), count);
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

            internal void DoResize(int top, int bottom, IDitherer ditherer)
            {
                ArraySection<ColorF> buffer = transposedFirstPassBuffer.Buffer;

                ParallelHelper.For(top, bottom, y =>
                {
                    var targetColumnBuffer = new ArraySection<ColorF>(targetRectangle.Width);

                    // Ensure offsets are normalized for cropping and padding.
                    ResizeKernel kernel = verticalKernelMap.GetKernel(y - targetRectangle.Y);

                    while (kernel.StartIndex + kernel.Length > currentWindow.Bottom)
                        Slide();

                    int topLine = kernel.StartIndex - currentWindow.Top;

                    for (int x = 0; x < targetRectangle.Width; x++)
                    {
                        // Destination color components
                        targetColumnBuffer.GetElementReference(x) = kernel.ConvolveWith(ref buffer, topLine + x * sourceRectangle.Height);
                    }

                    IWritableBitmapDataRow row = target[y];
                    for (int x = 0; x < targetRectangle.Width; x++)
                    {
                        row[x + targetRectangle.Left] = targetColumnBuffer[x].ToColor32();
                    }

                    targetColumnBuffer.Release();
                });
            }

            #endregion

            #region Private Methods

            private void CalculateFirstPassValues(int top, int bottom, bool parallel)
            {
                void ProcessRow(int y)
                {
                    var sourceRowBuffer = new ArraySection<ColorF>(sourceRectangle.Width);
                    IReadableBitmapDataRow sourceRow = source[y + sourceRectangle.Top];

                    for (int x = 0; x < sourceRectangle.Width; x++)
                    {
                        sourceRowBuffer.GetElementReference(x) = new ColorF(sourceRow[x + sourceRectangle.Left]);
                    }

                    int firstPassBaseIndex = y - currentWindow.Top;
                    for (int x = 0; x < targetRectangle.Width; x++)
                    {
                        ResizeKernel kernel = horizontalKernelMap.GetKernel(x);
                        transposedFirstPassBuffer.GetElementReference(x, firstPassBaseIndex) = kernel.ConvolveWith(ref sourceRowBuffer, kernel.StartIndex);
                    }

                    sourceRowBuffer.Release();
                }

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
    }
}
