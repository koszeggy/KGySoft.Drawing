#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: OptimizedPaletteQuantizer.Wu.cs
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
#if NETCOREAPP || NETSTANDARD2_1_OR_GREATER
using System.Buffers;
#endif
using System.Collections.Generic;
using System.Security;

using KGySoft.Collections;
using KGySoft.Threading;

#endregion

namespace KGySoft.Drawing.Imaging
{
    public sealed partial class OptimizedPaletteQuantizer
    {
        /// <summary>
        /// Credit to Xiaolin Wu's Color Quantizer published at https://www.ece.mcmaster.ca/~xwu/cq.c
        /// This quantizer is mainly based on his code.
        /// </summary>
        private sealed class WuQuantizer : IOptimizedPaletteQuantizer
        {
            #region Nested types

            #region Enumerations

            private enum Direction { Red = 2, Green = 1, Blue = 0 }

            #endregion

            #region CubicBuffer struct

            /// <summary>
            /// Similar to <see cref="Array3D{T}"/> (it allows both 3D and 1D access) but allows negative indices, which returns default value.
            /// The original algorithm uses 33x33x33 arrays, where 0 indices are never set in any dimensions so they always were 0.
            /// This was acceptable for the original algorithm but with 8 bit resolution (257x257x257 * sizeof(T)) the waste is much more significant,
            /// especially with array pooling, which allocates almost twice as much memory as needed when dimensions are 2^n + 1.
            /// </summary>
            private readonly struct CubicBuffer<T>
#if NETCOREAPP || NETSTANDARD2_1_OR_GREATER
                : IDisposable
#endif
                where T : unmanaged
            {
                #region Fields

                private readonly int bitSize;
                private readonly T[] buf;

                #endregion

                #region Indexers

                internal T this[int index] => index < 0 ? default : buf[index];

                internal T this[int r, int g, int b]
                {
                    get
                    {
                        if (r == -1 || g == -1 || b == -1)
                            return default;

                        return buf[(r << (bitSize << 1)) + (g << bitSize) + b];
                    }
                }

                #endregion

                #region Constructors

                internal CubicBuffer(int bitSize)
                {
                    this.bitSize = bitSize;
#if NETCOREAPP || NETSTANDARD2_1_OR_GREATER
                    int len = 1 << (bitSize * 3);
                    buf = ArrayPool<T>.Shared.Rent(len);
                    Array.Clear(buf, 0, len);
#else
                    buf = new T[1 << (bitSize * 3)];
#endif
                }

                #endregion

                #region Methods

                #region Public Methods

#if NETCOREAPP || NETSTANDARD2_1_OR_GREATER
                public void Dispose() => ArrayPool<T>.Shared.Return(buf);
#endif

                #endregion

                #region Internal Methods

                internal ref T GetRef(int index)
                {
                    Debug.Assert(index >= 0);
                    return ref buf[index];
                }

                #endregion

                #endregion
            }

            #endregion

            #region Box class

            private sealed class Box
            {
                #region Fields

                internal int RMin;
                internal int RMax;
                internal int GMin;
                internal int GMax;
                internal int BMin;
                internal int BMax;
                internal int Vol;

                #endregion

                #region Methods

                /// <summary>
                /// Computes the sum over a box of any given statistic.
                /// </summary>
                internal long Volume(ref CubicBuffer<long> mmt)
                {
                    return mmt[RMax, GMax, BMax]
                        - mmt[RMax, GMax, BMin]
                        - mmt[RMax, GMin, BMax]
                        + mmt[RMax, GMin, BMin]
                        - mmt[RMin, GMax, BMax]
                        + mmt[RMin, GMax, BMin]
                        + mmt[RMin, GMin, BMax]
                        - mmt[RMin, GMin, BMin];
                }

                /// <summary>
                /// Computes the sum over a box of any given statistic (floating point version).
                /// </summary>
                internal float Volume(ref CubicBuffer<float> mmt)
                {
                    return mmt[RMax, GMax, BMax]
                        - mmt[RMax, GMax, BMin]
                        - mmt[RMax, GMin, BMax]
                        + mmt[RMax, GMin, BMin]
                        - mmt[RMin, GMax, BMax]
                        + mmt[RMin, GMax, BMin]
                        + mmt[RMin, GMin, BMax]
                        - mmt[RMin, GMin, BMin];
                }

                /// <summary>
                /// Computes part of <see cref="Volume(ref CubicBuffer{long})"/> that doesn't depend on <see cref="RMax"/>, <see cref="GMax"/>
                /// or <see cref="BMax"/>, depending on <paramref name="dir"/>.
                /// </summary>
                internal long Bottom(Direction dir, ref CubicBuffer<long> mmt)
                {
                    switch (dir)
                    {
                        case Direction.Red:
                            return -mmt[RMin, GMax, BMax]
                                + mmt[RMin, GMax, BMin]
                                + mmt[RMin, GMin, BMax]
                                - mmt[RMin, GMin, BMin];

                        case Direction.Green:
                            return -mmt[RMax, GMin, BMax]
                                + mmt[RMax, GMin, BMin]
                                + mmt[RMin, GMin, BMax]
                                - mmt[RMin, GMin, BMin];

                        case Direction.Blue:
                            return -mmt[RMax, GMax, BMin]
                                + mmt[RMax, GMin, BMin]
                                + mmt[RMin, GMax, BMin]
                                - mmt[RMin, GMin, BMin];
                        default:
                            // Just to satisfy the compiler. No resource is needed, cannot occur.
                            throw new ArgumentOutOfRangeException(nameof(dir));
                    }
                }

                /// <summary>
                /// Computes remainder of <see cref="Volume(ref CubicBuffer{long})"/>, substituting <paramref name="pos"/>
                /// for <see cref="RMax"/>, <see cref="GMax"/> or <see cref="BMax"/>, depending on <paramref name="dir"/>.
                /// </summary>
                internal long Top(Direction dir, int pos, ref CubicBuffer<long> mmt)
                {
                    switch (dir)
                    {
                        case Direction.Red:
                            return mmt[pos, GMax, BMax]
                                - mmt[pos, GMax, BMin]
                                - mmt[pos, GMin, BMax]
                                + mmt[pos, GMin, BMin];

                        case Direction.Green:
                            return mmt[RMax, pos, BMax]
                                - mmt[RMax, pos, BMin]
                                - mmt[RMin, pos, BMax]
                                + mmt[RMin, pos, BMin];

                        case Direction.Blue:
                            return mmt[RMax, GMax, pos]
                                - mmt[RMax, GMin, pos]
                                - mmt[RMin, GMax, pos]
                                + mmt[RMin, GMin, pos];

                        default:
                            // Just to satisfy the compiler. No resource is needed, cannot occur.
                            throw new ArgumentOutOfRangeException(nameof(dir));
                    }
                }

                #endregion
            }

            #endregion

            #endregion

            #region Fields

            #region Static Fields

            /// <summary>
            /// Just a lookup table for squared values between 0..255
            /// </summary>
            private static readonly int[] sqrTable = InitSqrTable();

            #endregion

            #region Instance Fields

            private int maxColors;
            private int histBitSize;
            private int histSize;

            /// <summary>
            /// The squared moment values of color RGB values.
            /// After building the histogram by <see cref="AddColor"/> an element of this array can be interpreted as
            /// m2[r, g, b] = sum over voxel of c^2*P(c)
            /// and after <see cref="HistogramToMoments"/> it contains cumulative moments.
            /// The strictly taken Bernoulli probability is actually multiplied by image size.
            /// In Wu's original algorithm effective histogram elements were in 1..<see cref="histSize"/> along each axis,
            /// element 0 was just for base or marginal value. Here values are zero-based, but reading -1 index is allowed, which returns zero.
            /// Values are floats just because of the possible big ranges due to squared values.
            /// </summary>
            private CubicBuffer<float> m2;

            /// <summary>
            /// The counts of voxels of the 3D color cubes in each position.
            /// The same applies as for <see cref="m2"/> except that after <see cref="AddColor"/> values are interpreted as
            /// wt[r, g, b] = sum over voxel of P(c)
            /// </summary>
            private CubicBuffer<long> wt;

            /// <summary>
            /// The moment values of red color components.
            /// The same applies as for <see cref="m2"/> except that after <see cref="AddColor"/> values are interpreted as
            /// wt[r, g, b] = sum over voxel of r*P(c)
            /// </summary>
            private CubicBuffer<long> mr;

            /// <summary>
            /// The moment values of green color components.
            /// The same applies as for <see cref="m2"/> except that after <see cref="AddColor"/> values are interpreted as
            /// wt[r, g, b] = sum over voxel of g*P(c)
            /// </summary>
            private CubicBuffer<long> mg;

            /// <summary>
            /// The moment values of green color components.
            /// The same applies as for <see cref="m2"/> except that after <see cref="AddColor"/> values are interpreted as
            /// wt[r, g, b] = sum over voxel of b*P(c)
            /// </summary>
            private CubicBuffer<long> mb;

            private bool hasTransparency;

            #endregion

            #endregion

            #region Methods

            #region Static Methods

            private static int[] InitSqrTable()
            {
                var table = new int[256];
                for (int i = 0; i < 256; i++)
                    table[i] = i * i;
                return table;
            }

            #endregion

            #region Instance Methods

            #region Public Methods

            public void Initialize(int requestedColors, byte? bitLevel, IBitmapData source)
            {
                maxColors = requestedColors;

                // unless explicitly specified, not using more than 6 bit levels due to speed and memory requirement
                histBitSize = bitLevel ?? (requestedColors > 256 ? 6 : 5);
                histSize = 1 << histBitSize;
                m2 = new CubicBuffer<float>(histBitSize);
                wt = new CubicBuffer<long>(histBitSize);
                mr = new CubicBuffer<long>(histBitSize);
                mg = new CubicBuffer<long>(histBitSize);
                mb = new CubicBuffer<long>(histBitSize);
            }

            public void AddColor(Color32 c)
            {
                // Transparent pixels are not included into the histogram
                if (c.A == 0)
                {
                    hasTransparency = true;
                    return;
                }

                // Building the 3D color histogram of counts, separate RGB components and c^2

                // Original comment from Xiaolin Wu:
                // At conclusion of the histogram step, we can interpret
                //   wt[r][g][b] = sum over voxel of P(c)
                //   mr[r][g][b] = sum over voxel of r*P(c)  ,  similarly for mg, mb
                //   m2[r][g][b] = sum over voxel of c ^ 2 * P(c)
                // Actually each of these should be divided by 'size' to give the usual
                // interpretation of P() as ranging from 0 to 1, but we needn't do that here.

                // We pre-quantize the color components to histBitSize bits (unless it is 8 bits) to reduce the size of the 3D histogram.
                // Using a reusable 1D index to prevent calculating it multiple times
                int ind = GetFlattenIndex(c.R >> (8 - histBitSize),
                    c.G >> (8 - histBitSize),
                    c.B >> (8 - histBitSize));

                wt.GetRef(ind) += 1;
                mr.GetRef(ind) += c.R;
                mg.GetRef(ind) += c.G;
                mb.GetRef(ind) += c.B;
                m2.GetRef(ind) += sqrTable[c.R] + sqrTable[c.G] + sqrTable[c.B];
            }

            [SecuritySafeCritical]
            public Color32[]? GeneratePalette(IAsyncContext context)
            {
                context.Progress?.New(DrawingOperation.GeneratingPalette, histSize + maxColors - (hasTransparency ? 1 : 0));

                // Original comment from Xiaolin Wu:
                // We now convert histogram into moments so that we can rapidly calculate
                // the sums of the above quantities over any desired box.
                HistogramToMoments(context);
                if (context.IsCancellationRequested)
                    return null;

                List<Box>? cubes = CreatePartitions(context);
                if (context.IsCancellationRequested)
                    return null;

                var result = new Color32[cubes!.Count + (hasTransparency ? 1 : 0)];
                for (int k = 0; k < cubes.Count; k++)
                {
                    // The original algorithm here marks an array of tags but we don't need it because
                    // we don't want to produce an array of quantized pixels just the palette.
                    long weight = cubes[k].Volume(ref wt);
                    if (weight <= 0)
                    {
                        Debug.Assert(cubes.Count == 1 && hasTransparency, $"bogus box {k}");
                        continue;
                    }

                    result[k] = new Color32(
                        (byte)(cubes[k].Volume(ref mr) / weight),
                        (byte)(cubes[k].Volume(ref mg) / weight),
                        (byte)(cubes[k].Volume(ref mb) / weight));
                }

                context.Progress?.Complete();
                return result;
            }

            public void Dispose()
            {
#if NETCOREAPP || NETSTANDARD2_1_OR_GREATER
                m2.Dispose();
                wt.Dispose();
                mr.Dispose();
                mg.Dispose();
                mb.Dispose();
#endif
            }

            #endregion

            #region Private Methods

            private int GetFlattenIndex(int r, int g, int b) => r < 0 ? -1 : (r << (histBitSize << 1)) + (g << histBitSize) + b;

            /// <summary>
            /// Computing cumulative moments from the histogram.
            /// </summary>
            [SecurityCritical]
            private unsafe void HistogramToMoments(IAsyncContext context)
            {
                long* area = stackalloc long[histSize];
                long* areaR = stackalloc long[histSize];
                long* areaG = stackalloc long[histSize];
                long* areaB = stackalloc long[histSize];
                float* area2 = stackalloc float[histSize];

                for (int r = 0; r < histSize; r++)
                {
                    if (context.IsCancellationRequested)
                        return;

                    for (int i = 0; i < histSize; i++)
                    {
                        area[i] = areaR[i] = areaG[i] = areaB[i] = 0L;
                        area2[i] = 0f;
                    }

                    for (int g = 0; g < histSize; g++)
                    {
                        float line2 = 0f;
                        long line = 0;
                        long lineR = 0;
                        long lineG = 0;
                        long lineB = 0;

                        for (int b = 0; b < histSize; b++)
                        {
                            int ind1 = GetFlattenIndex(r, g, b);
                            line += wt[ind1];
                            lineR += mr[ind1];
                            lineG += mg[ind1];
                            lineB += mb[ind1];
                            line2 += m2[ind1];

                            area[b] += line;
                            areaR[b] += lineR;
                            areaG[b] += lineG;
                            areaB[b] += lineB;
                            area2[b] += line2;

                            int ind2 = GetFlattenIndex(r - 1, g, b);
                            wt.GetRef(ind1) = wt[ind2] + area[b];
                            mr.GetRef(ind1) = mr[ind2] + areaR[b];
                            mg.GetRef(ind1) = mg[ind2] + areaG[b];
                            mb.GetRef(ind1) = mb[ind2] + areaB[b];
                            m2.GetRef(ind1) = m2[ind2] + area2[b];
                        }
                    }

                    context.Progress?.Increment();
                }
            }

            private List<Box>? CreatePartitions(IAsyncContext context)
            {
                int colorCount = maxColors - (hasTransparency ? 1 : 0);
                var cubes = new List<Box>(colorCount);

                // Adding an initial item with largest possible size. We split it until we
                // have the needed colors or we cannot split further any of the boxes.
                cubes.Add(new Box { RMin = -1, RMax = histSize - 1, GMin = -1, GMax = histSize - 1, BMin = -1, BMax = histSize - 1 });

                float[] vv = new float[colorCount];
                int next = 0;

                for (int i = 1; i < colorCount; i++)
                {
                    if (context.IsCancellationRequested)
                        return null;

                    // we always take an already added box and try to split it into two halves
                    Box firstHalf = cubes[next];
                    Box secondHalf = new Box();

                    // splitting the box only if it is not a single cell
                    if (TryCut(firstHalf, secondHalf))
                    {
                        vv[next] = firstHalf.Vol > 1 ? Var(firstHalf) : 0f;
                        vv[i] = secondHalf.Vol > 1 ? Var(secondHalf) : 0f;
                        cubes.Add(secondHalf);
                        context.Progress?.Increment();
                    }
                    else
                    {
                        // the cut was not possible, reverting the index
                        vv[next] = 0f; // so we won't try to split this box again
                        i--;
                    }

                    next = 0;
                    float temp = vv[0];

                    for (int k = 1; k <= i; k++)
                    {
                        if (vv[k] > temp)
                        {
                            temp = vv[k];
                            next = k;
                        }
                    }

                    // no more boxes (colors)
                    if (temp <= 0f)
                        break;
                }

                return cubes;
            }

            /// <summary>
            /// Compute the weighted variance of a box.
            /// Note: as with the raw statistics, this is actually the variance multiplied by image size
            /// </summary>
            private float Var(Box cube)
            {
                float vr = cube.Volume(ref mr);
                float vg = cube.Volume(ref mg);
                float vb = cube.Volume(ref mb);

                float vm2 = cube.Volume(ref m2);

                return vm2 - (vr * vr + vg * vg + vb * vb) / cube.Volume(ref wt);
            }

            private float Maximize(Box cube, Direction dir, int first, int last, out int cut,
                long wholeR, long wholeG, long wholeB, long wholeW)
            {
                // Original comment from Xiaolin Wu:
                // We want to minimize the sum of the variances of two subboxes.
                // The sum(c^2) terms can be ignored since their sum over both subboxes
                // is the same (the sum for the whole box) no matter where we split.
                // The remaining terms have a minus sign in the variance formula,
                // so we drop the minus sign and MAXIMIZE the sum of the two terms.

                long baseR = cube.Bottom(dir, ref mr);
                long baseG = cube.Bottom(dir, ref mg);
                long baseB = cube.Bottom(dir, ref mb);
                long baseW = cube.Bottom(dir, ref wt);

                float max = 0f;
                cut = -1;

                for (int i = first; i < last; i++)
                {
                    long halfR = baseR + cube.Top(dir, i, ref mr);
                    long halfG = baseG + cube.Top(dir, i, ref mg);
                    long halfB = baseB + cube.Top(dir, i, ref mb);
                    long halfW = baseW + cube.Top(dir, i, ref wt);

                    // now half_x is sum over lower half of box, if split at i

                    // not splitting on an empty box
                    if (halfW == 0)
                        continue;

                    float dist = halfR * halfR + halfG * halfG + halfB * halfB;
                    float temp = dist / halfW;

                    halfR = wholeR - halfR;
                    halfG = wholeG - halfG;
                    halfB = wholeB - halfB;
                    halfW = wholeW - halfW;

                    // not splitting on an empty box
                    if (halfW == 0)
                        continue;

                    dist = halfR * halfR + halfG * halfG + halfB * halfB;
                    temp += dist / halfW;

                    if (temp > max)
                    {
                        max = temp;
                        cut = i;
                    }
                }

                return max;
            }

            private bool TryCut(Box set1, Box set2)
            {
                long wholeR = set1.Volume(ref mr);
                long wholeG = set1.Volume(ref mg);
                long wholeB = set1.Volume(ref mb);
                long wholeW = set1.Volume(ref wt);

                float maxR = Maximize(set1, Direction.Red, set1.RMin + 1, set1.RMax,
                    out int cutR, wholeR, wholeG, wholeB, wholeW);
                float maxG = Maximize(set1, Direction.Green, set1.GMin + 1, set1.GMax,
                    out int cutG, wholeR, wholeG, wholeB, wholeW);
                float maxB = Maximize(set1, Direction.Blue, set1.BMin + 1, set1.BMax,
                    out int cutB, wholeR, wholeG, wholeB, wholeW);

                Direction dir;
                if (maxR >= maxG && maxR >= maxB)
                {
                    dir = Direction.Red;

                    // can't split the box
                    if (cutR < 0)
                        return false;
                }
                else if (maxG >= maxR && maxG >= maxB)
                    dir = Direction.Green;
                else
                    dir = Direction.Blue;

                set2.RMax = set1.RMax;
                set2.GMax = set1.GMax;
                set2.BMax = set1.BMax;

                switch (dir)
                {
                    case Direction.Red:
                        set2.RMin = set1.RMax = cutR;
                        set2.GMin = set1.GMin;
                        set2.BMin = set1.BMin;
                        break;

                    case Direction.Green:
                        set2.GMin = set1.GMax = cutG;
                        set2.RMin = set1.RMin;
                        set2.BMin = set1.BMin;
                        break;

                    case Direction.Blue:
                        set2.BMin = set1.BMax = cutB;
                        set2.RMin = set1.RMin;
                        set2.GMin = set1.GMin;
                        break;
                }

                set1.Vol = (set1.RMax - set1.RMin) * (set1.GMax - set1.GMin) * (set1.BMax - set1.BMin);
                set2.Vol = (set2.RMax - set2.RMin) * (set2.GMax - set2.GMin) * (set2.BMax - set2.BMin);

                return true;
            }

            #endregion

            #endregion

            #endregion
        }
    }
}
