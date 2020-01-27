#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: OptimizedPaletteQuantizer.Wu.cs
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

#endregion

namespace KGySoft.Drawing.Imaging
{
    public sealed partial class OptimizedPaletteQuantizer
    {
        /// <summary>
        /// Credits to Xiaolin Wu's Color Quantizer published at https://www.ece.mcmaster.ca/~xwu/cq.c
        /// This quantizer is mainly based on his code.
        /// </summary>
        private sealed class WuQuantizer : IOptimizedPaletteQuantizer
        {
            #region Nested types

            #region Enumerations

            private enum Direction { Red = 2, Green = 1, Blue = 0 }

            #endregion

            #region Box class

            private class Box
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
                internal long Volume(long[,,] mmt)
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
                internal float Volume(float[,,] mmt)
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
                /// Compute part of <see cref="Volume(long[,,])"/> that doesn't depend on <see cref="RMax"/>, <see cref="GMax"/>
                /// or <see cref="BMax"/>, depending on <paramref name="dir"/>.
                /// </summary>
                internal long Bottom(Direction dir, long[,,] mmt)
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
                /// Compute remainder of <see cref="Volume(long[,,])"/> , substituting <paramref name="pos"/>
                /// for <see cref="RMax"/>, <see cref="GMax"/> or <see cref="BMax"/>, depending on <paramref name="dir"/>.
                /// </summary>
                internal long Top(Direction dir, int pos, long[,,] mmt)
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

            #region Constants

            private const int histCount = 33;
            private const int histSize = 32;

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


            /// <summary>
            /// The squared moment values of color RGB values.
            /// After building the histogram by <see cref="AddColor"/> an element of this array can be interpreted as
            /// m2[r, g, b] = sum over voxel of c^2*P(c)
            /// and after <see cref="HistogramToMoments"/> it contains cumulative moments.
            /// The strictly taken Bernoulli probability is actually multiplied by image size.
            /// but it does not matter here.
            /// Effective histogram elements are in 1..<see cref="histSize"/> along each axis,
            /// element 0 is just for base or marginal value.
            /// Values are floats just because of the possible big ranges due to squared values.
            /// </summary>
            [SuppressMessage("Performance", "CA1814:Prefer jagged arrays over multidimensional",
                Justification = "It is really used as a cube, all items are required")]
            private readonly float[,,] m2 = new float[histCount, histCount, histCount];

            /// <summary>
            /// The counts of voxels of the 3D color cubes in each position.
            /// The same applies as for <see cref="m2"/> except that after <see cref="AddColor"/> values are interpreted as
            /// wt[r, g, b] = sum over voxel of P(c)
            /// </summary>
            [SuppressMessage("Performance", "CA1814:Prefer jagged arrays over multidimensional",
                Justification = "It is really used as a cube, all items are required")]
            private readonly long[,,] wt = new long[histCount, histCount, histCount];

            /// <summary>
            /// The moment values of red color components.
            /// The same applies as for <see cref="m2"/> except that after <see cref="AddColor"/> values are interpreted as
            /// wt[r, g, b] = sum over voxel of r*P(c)
            /// </summary>
            [SuppressMessage("Performance", "CA1814:Prefer jagged arrays over multidimensional",
                Justification = "It is really used as a cube, all items are required")]
            private readonly long[,,] mr = new long[histCount, histCount, histCount];

            /// <summary>
            /// The moment values of green color components.
            /// The same applies as for <see cref="m2"/> except that after <see cref="AddColor"/> values are interpreted as
            /// wt[r, g, b] = sum over voxel of g*P(c)
            /// </summary>
            [SuppressMessage("Performance", "CA1814:Prefer jagged arrays over multidimensional",
                Justification = "It is really used as a cube, all items are required")]
            private readonly long[,,] mg = new long[histCount, histCount, histCount];

            /// <summary>
            /// The moment values of green color components.
            /// The same applies as for <see cref="m2"/> except that after <see cref="AddColor"/> values are interpreted as
            /// wt[r, g, b] = sum over voxel of b*P(c)
            /// </summary>
            [SuppressMessage("Performance", "CA1814:Prefer jagged arrays over multidimensional",
                Justification = "It is really used as a cube, all items are required")]
            private readonly long[,,] mb = new long[histCount, histCount, histCount];

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

            public void Initialize(int requestedColors, IBitmapData source) => maxColors = requestedColors;

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

                // We pre-quantize the color components to 5 bit to reduce the size of the 3D histogram.
                int indR = (c.R >> 3) + 1;
                int indG = (c.G >> 3) + 1;
                int indB = (c.B >> 3) + 1;

                // TODO: index by a single ind
                //int ind = (indR << 10) + (indR << 6) + indR + (indG << 5) + indG + indB;
                wt[indR, indG, indB] += 1;
                mr[indR, indG, indB] += c.R;
                mg[indR, indG, indB] += c.G;
                mb[indR, indG, indB] += c.B;
                m2[indR, indG, indB] += sqrTable[c.R] + sqrTable[c.G] + sqrTable[c.B];
            }

            public Color32[] GeneratePalette()
            {
                // Original comment from Xiaolin Wu:
                // We now convert histogram into moments so that we can rapidly calculate
                // the sums of the above quantities over any desired box.
                HistogramToMoments();

                List<Box> cubes = CreatePartitions();
                var result = new Color32[cubes.Count + (hasTransparency ? 1 : 0)];

                for (int k = 0; k < cubes.Count; k++)
                {
                    // The original algorithm here marks an array of tags but we don't need it because
                    // we don't want to produce an array of quantized pixels but just the palette.
                    long weight = cubes[k].Volume(wt);
                    if (weight <= 0)
                    {
                        Debug.Assert(cubes.Count == 1 && hasTransparency, $"bogus box {k}");

                        continue;
                    }

                    result[k] = new Color32(
                        (byte)(cubes[k].Volume(mr) / weight),
                        (byte)(cubes[k].Volume(mg) / weight),
                        (byte)(cubes[k].Volume(mb) / weight));
                }

                return result;
            }

            #endregion

            #region Private Methods

            /// <summary>
            /// Computing cumulative moments from the histogram.
            /// </summary>
            private void HistogramToMoments(/*vwt, vmr, vmg, vmb, m2*/)
            {
                long[] area = new long[histCount];
                long[] areaR = new long[histCount];
                long[] areaG = new long[histCount];
                long[] areaB = new long[histCount];
                float[] area2 = new float[histCount];

                for (int r = 1; r <= histSize; r++)
                {
                    // TODO: is it faster if we just initialize these here instead of zeroing?
                    for (int i = 0; i <= histSize; i++)
                        area2[i] = area[i] = areaR[i] = areaG[i] = areaB[i] = 0;

                    for (int g = 1; g <= histSize; g++)
                    {
                        float line2 = 0f;
                        long line = 0;
                        long lineR = 0;
                        long lineG = 0;
                        long lineB = 0;

                        for (int b = 1; b <= histSize; b++)
                        {
                            // int ind1 = (r << 10) + (r << 6) + r + (g << 5) + g + b; /* [r][g][b] */
                            line += wt[r, g, b];
                            lineR += mr[r, g, b];
                            lineG += mg[r, g, b];
                            lineB += mb[r, g, b];
                            line2 += m2[r, g, b];

                            area[b] += line;
                            areaR[b] += lineR;
                            areaG[b] += lineG;
                            areaB[b] += lineB;
                            area2[b] += line2;

                            // int ind2 = ind1 - 1089; /* [r-1][g][b] */
                            wt[r, g, b] = wt[r - 1, g, b] + area[b];
                            mr[r, g, b] = mr[r - 1, g, b] + areaR[b];
                            mg[r, g, b] = mg[r - 1, g, b] + areaG[b];
                            mb[r, g, b] = mb[r - 1, g, b] + areaB[b];
                            m2[r, g, b] = m2[r - 1, g, b] + area2[b];
                        }
                    }
                }
            }

            private List<Box> CreatePartitions()
            {
                int colorCount = maxColors - (hasTransparency ? 1 : 0);
                var cubes = new List<Box>(colorCount);

                // Adding an initial item with largest possible size. We split it until we
                // have the needed colors or we cannot split further any of the boxes.
                cubes.Add(new Box { RMax = histSize, GMax = histSize, BMax = histSize });

                float[] vv = new float[colorCount];
                int next = 0;

                for (int i = 1; i < colorCount; i++)
                { 
                    // we always take an already added box and try to split it into two halves
                    Box firstHalf = cubes[next];
                    Box secondHalf = new Box();

                    // splitting the box only if it is not a single cell
                    if (TryCut(firstHalf, secondHalf))
                    {
                        vv[next] = firstHalf.Vol > 1 ? Var(firstHalf) : 0f;
                        vv[i] = secondHalf.Vol > 1 ? Var(secondHalf) : 0f;
                        cubes.Add(secondHalf);
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
                float vr = cube.Volume(mr);
                float vg = cube.Volume(mg);
                float vb = cube.Volume(mb);

                float vm2 = cube.Volume(m2);

                return vm2 - (vr * vr + vg * vg + vb * vb) / cube.Volume(wt);
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

                long baseR = cube.Bottom(dir, mr);
                long baseG = cube.Bottom(dir, mg);
                long baseB = cube.Bottom(dir, mb);
                long baseW = cube.Bottom(dir, wt);

                float max = 0f;
                cut = -1;

                for (int i = first; i < last; i++)
                {
                    long halfR = baseR + cube.Top(dir, i, mr);
                    long halfG = baseG + cube.Top(dir, i, mg);
                    long halfB = baseB + cube.Top(dir, i, mb);
                    long halfW = baseW + cube.Top(dir, i, wt);

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
                long wholeR = set1.Volume(mr);
                long wholeG = set1.Volume(mg);
                long wholeB = set1.Volume(mb);
                long wholeW = set1.Volume(wt);

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
