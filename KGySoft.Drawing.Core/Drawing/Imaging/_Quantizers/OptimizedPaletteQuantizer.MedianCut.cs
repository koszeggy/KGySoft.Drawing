#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: OptimizedPaletteQuantizer.MedianCut.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2024 - All Rights Reserved
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
using System.Runtime.CompilerServices;

using KGySoft.Collections;
using KGySoft.Threading;

#endregion

namespace KGySoft.Drawing.Imaging
{
    public sealed partial class OptimizedPaletteQuantizer
    {
        private sealed class MedianCutQuantizer : IOptimizedPaletteQuantizer
        {
            #region Nested Types

            #region Enumerations

            private enum ColorComponent { R, G, B }

            #endregion

            #region ColorBucket class

            private sealed class ColorBucket
            {
                #region Nested Classes

                #region RedComparer class

                private sealed class RedComparer : IComparer<Color32>
                {
                    #region Methods

                    public int Compare(Color32 a, Color32 b) => a.R - b.R;

                    #endregion
                }

                #endregion

                #region GreenComparer class

                private sealed class GreenComparer : IComparer<Color32>
                {
                    #region Methods

                    public int Compare(Color32 a, Color32 b) => a.G - b.G;

                    #endregion
                }

                #endregion

                #region BlueComparer class

                private sealed class BlueComparer : IComparer<Color32>
                {
                    #region Methods

                    public int Compare(Color32 a, Color32 b) => a.B - b.B;

                    #endregion
                }

                #endregion

                #endregion

                #region Fields

                #region Static Fields

                private static readonly IComparer<Color32> redSorter = new RedComparer();
                private static readonly IComparer<Color32> greenSorter = new GreenComparer();
                private static readonly IComparer<Color32> blueSorter = new BlueComparer();

                #endregion

                #region Instance Fields

                private readonly Color32[] colors;
                private readonly int start;

                private int rMin;
                private int rMax;
                private int gMin;
                private int gMax;
                private int bMin;
                private int bMax;

                #endregion

                #endregion

                #region Properties

                internal int Count { get; private set; }
                internal int RangeR => rMax - rMin;
                internal int RangeG => gMax - gMin;
                internal int RangeB => bMax - bMin;
                internal bool IsSingleColor => RangeR == 0 && RangeG == 0 && RangeB == 0;

                #endregion

                #region Constructors

                #region Internal Constructors

                internal ColorBucket(Color32[] buf)
                {
                    colors = buf;
                    rMin = gMin = bMin = Byte.MaxValue;
                }

                #endregion

                #region Private Constructors

                private ColorBucket(Color32[] buf, int startIndex, int count)
                {
                    Debug.Assert(startIndex >= 0 && startIndex < buf.Length && count > 0 && count < buf.Length, "A valid subrange is expected");

                    colors = buf;
                    start = startIndex;
                    Count = count;
                    rMin = gMin = bMin = Byte.MaxValue;

                    int end = startIndex + count;
                    for (int i = startIndex; i < end; i++)
                        AdjustRanges(colors[i]);
                }

                #endregion

                #endregion

                #region Methods

                #region Internal Methods

                internal void AddColor(Color32 c)
                {
                    Debug.Assert(Count < colors.Length, "Buffer is too small");
                    Debug.Assert(start == 0, "Adding colors is expected for root only");
                    colors[Count] = c;
                    Count += 1;
                    AdjustRanges(c);
                }

                internal Color32 ToColor()
                {
                    int count = Count;
                    Debug.Assert(count != 0, "Empty bucket");
                    if (count <= 0)
                        return count == 0 ? default : colors[start];

                    int rSum = 0, gSum = 0, bSum = 0;
                    int end = start + count;
                    for (int i = start; i < end; i++)
                    {
                        Color32 color = colors[i];
                        rSum += color.R;
                        gSum += color.G;
                        bSum += color.B;
                    }

                    return new Color32((byte)(rSum / count),
                        (byte)(gSum / count),
                        (byte)(bSum / count));
                }

                internal void Split(IAsyncContext context, ColorComponent component, ColorBucketCollection buckets, ref int bucketIndex, ref int buckedEndIndex)
                {
                    Debug.Assert(Count > 1);
                    var sorter = component switch
                    {
                        ColorComponent.R => redSorter,
                        ColorComponent.G => greenSorter,
                        _ => blueSorter,
                    };

                    ParallelHelper.Sort(context, colors, start, Count, sorter);
                    if (context.IsCancellationRequested)
                        return;

                    int medianIndex = Count >> 1;

                    // single color check is correct because we sorted by all the components
                    bool isLeftSingleColor = colors[start] == colors[start + medianIndex - 1];
                    bool isRightSingleColor = colors[start + medianIndex] == colors[start + Count - 1];
                    ColorBucket? left = isLeftSingleColor ? null : new ColorBucket(colors, start, medianIndex);
                    ColorBucket? right = isRightSingleColor ? null : new ColorBucket(colors, start + medianIndex, Count - medianIndex);

                    if (isLeftSingleColor)
                    {
                        buckets.AddFinalColor(colors[start]);

                        // if none of the halves could be added, we remove the current bucket and reduce the number of buckets to scan
                        if (isRightSingleColor)
                        {
                            buckets.AddFinalColor(colors[start + medianIndex]);
                            buckets.RemoveBucket(bucketIndex);
                            buckedEndIndex -= 1;
                            return;
                        }

                        // the right half is assigned back to the original position
                        buckets.ReplaceBucket(bucketIndex, right!);
                        bucketIndex += 1;
                        return;
                    }

                    // the left half is assigned back to the original position
                    buckets.ReplaceBucket(bucketIndex, left!);
                    bucketIndex += 1;

                    if (isRightSingleColor)
                    {
                        buckets.AddFinalColor(colors[start + medianIndex]);
                        return;
                    }

                    // the right half is added as a new bucket
                    buckets.AddBucket(right!);
                }

                #endregion

                #region Private Methods

                [MethodImpl(MethodImpl.AggressiveInlining)]
                private void AdjustRanges(Color32 c)
                {
                    if (c.R < rMin)
                        rMin = c.R;
                    if (c.R > rMax)
                        rMax = c.R;

                    if (c.G < gMin)
                        gMin = c.G;
                    if (c.G > gMax)
                        gMax = c.G;

                    if (c.B < bMin)
                        bMin = c.B;
                    if (c.B > bMax)
                        bMax = c.B;
                }

                #endregion

                #endregion
            }

            #endregion

            #region ColorBucketCollection class

            private sealed class ColorBucketCollection
            {
                #region Fields

                private readonly int maxColors;
                private readonly CircularList<ColorBucket> buckets;
                private readonly HashSet<Color32> finalColors = new HashSet<Color32>();

                #endregion

                #region Properties

                internal int FinalColorsCount => finalColors.Count;
                internal int ColorsCount => buckets.Count + FinalColorsCount;

                #endregion

                #region Constructors

                internal ColorBucketCollection(int maxColors)
                {
                    this.maxColors = maxColors;
                    buckets = new CircularList<ColorBucket>(maxColors);
                }

                #endregion

                #region Methods

                #region Internal Methods

                internal void AddBucket(ColorBucket item)
                {
                    Debug.Assert(!item.IsSingleColor, "Single color should be added to final colors");
                    buckets.Add(item);
                }

                public void ReplaceBucket(int index, ColorBucket bucket) => buckets[index] = bucket;

                internal ColorBucket? RemoveFirstBucket()
                {
                    if (buckets.Count == 0)
                        return null;
                    ColorBucket result = buckets[0];
                    buckets.RemoveFirst();
                    return result;
                }

                public void RemoveBucket(int index) => buckets.RemoveAt(index);

                internal bool SplitBuckets(IAsyncContext context, ref int bucketIndex)
                {
                    bool splitOccurred = false;

                    // saving length because we add new, halved buckets during the iteration,
                    // and we only want to go to the end of the original length
                    int endIndex = buckets.Count;
                    if (bucketIndex >= endIndex)
                        bucketIndex = 0;
                    while (bucketIndex < endIndex)
                    {
                        ColorBucket currentBucket = buckets[bucketIndex];
                        Debug.Assert(currentBucket.Count > 0, "Empty bucket");
                        Debug.Assert(!currentBucket.IsSingleColor);

                        if (context.IsCancellationRequested)
                            return false;

                        splitOccurred = true;

                        // on equal distance splitting on the green range in the first place because of human perception
                        if (currentBucket.RangeG >= currentBucket.RangeR && currentBucket.RangeG >= currentBucket.RangeB)
                            currentBucket.Split(context, ColorComponent.G, this, ref bucketIndex, ref endIndex);
                        else if (currentBucket.RangeR >= currentBucket.RangeB)
                            currentBucket.Split(context, ColorComponent.R, this, ref bucketIndex, ref endIndex);
                        else
                            currentBucket.Split(context, ColorComponent.B, this, ref bucketIndex, ref endIndex);

                        context.Progress?.SetProgressValue(ColorsCount);

                        // Stopping if we reached maxColors. Note that Split may increase ColorsCount.
                        if (ColorsCount == maxColors)
                            return false;
                    }

                    return splitOccurred;
                }

                internal bool AddFinalColor(Color32 color) => finalColors.Count != maxColors && finalColors.Add(color);
                internal void CopyFinalColorsTo(Color32[] result) => finalColors.CopyTo(result);

                #endregion

                #endregion
            }

            #endregion

            #endregion

            #region Fields

            private int maxColors;
            private Color32[] colors = null!;
            private ColorBucket root = null!;

            private bool hasTransparency;

            #endregion

            #region Properties

            private int MaxActualColors => maxColors - (hasTransparency ? 1 : 0);

            #endregion

            #region Methods

            public void Initialize(int requestedColors, byte? bitLevel, IBitmapData source)
            {
                int maxLevels = 1 << (bitLevel ?? 8);
                maxColors = Math.Min(requestedColors, maxLevels * maxLevels * maxLevels);
#if NETCOREAPP || NETSTANDARD2_1_OR_GREATER
                // note: rented arrays can be dirty, but it's alright because count is 0 initially.
                colors = ArrayPool<Color32>.Shared.Rent(source.Width * source.Height);
#else
                colors = new Color32[source.Width * source.Height];
#endif
                root = new ColorBucket(colors);
            }

            public void AddColor(Color32 color)
            {
                if (color.A == 0)
                    hasTransparency = true;
                else
                    root.AddColor(color);
            }
            
            public Color32[]? GeneratePalette(IAsyncContext context)
            {
                // Occurs when bitmap is completely transparent
                if (root.Count == 0)
                {
                    Debug.Assert(hasTransparency);
                    return new Color32[1];
                }

                Color32[] result;
                if (root.IsSingleColor)
                {
                    result = new Color32[hasTransparency ? 2 : 1];
                    result[0] = root.ToColor();
                    return result;
                }

                context.Progress?.New(DrawingOperation.GeneratingPalette, MaxActualColors, 1);
                var buckets = new ColorBucketCollection(MaxActualColors);
                buckets.AddBucket(root);

                // splitting the initial bucket until no more split can be done or desired color amount is reached
                int startIndex = 0;
                while (buckets.ColorsCount < MaxActualColors)
                {
                    if (buckets.SplitBuckets(context, ref startIndex))
                        startIndex = 0;
                    else
                        break;
                }

                // finalizing colors and continuing splitting if some buckets map to the same colors
                while (buckets.FinalColorsCount < MaxActualColors)
                {
                    ColorBucket? first = buckets.RemoveFirstBucket();
                    if (first == null)
                        break;
                    if (startIndex > 0)
                        startIndex -= 1;
                    if (!buckets.AddFinalColor(first.ToColor()))
                        buckets.SplitBuckets(context, ref startIndex);
                }

                if (context.IsCancellationRequested)
                    return null;

                Debug.Assert(buckets.FinalColorsCount <= MaxActualColors);
                result = new Color32[buckets.FinalColorsCount + (hasTransparency ? 1 : 0)];
                buckets.CopyFinalColorsTo(result);
                context.Progress?.Complete();

                // If transparent color is needed, then it will be automatically the last color in the result
                return result;
            }

            public void Dispose()
            {
#if NETCOREAPP || NETSTANDARD2_1_OR_GREATER
                ArrayPool<Color32>.Shared.Return(colors);
#endif
                colors = null!;
                root = null!;
            }

            #endregion
        }
    }
}
