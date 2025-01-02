﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: OptimizedPaletteQuantizer.MedianCut.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2025 - All Rights Reserved
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
using System.Runtime.CompilerServices;
using System.Security;

using KGySoft.Collections;
using KGySoft.Threading;

#endregion

namespace KGySoft.Drawing.Imaging
{
    public sealed partial class OptimizedPaletteQuantizer
    {
        private sealed class MedianCutQuantizer<T> : IOptimizedPaletteQuantizer
            where T : unmanaged
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

                private CastArray<T, Color32> colors;
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

                [SecuritySafeCritical]
                internal ColorBucket(CastArray<T, Color32> buf, bool isRoot)
                {
                    colors = buf;
                    rMin = gMin = bMin = Byte.MaxValue;
                    if (isRoot)
                        return;

                    Count = buf.Length;
                    for (int i = 0; i < buf.Length; i++)
                        AdjustRanges(buf.GetElementUnsafe(i));
                }

                #endregion

                #region Methods

                #region Internal Methods

                [SecuritySafeCritical]
                internal void AddColor(Color32 c)
                {
                    Debug.Assert(Count < colors.Length, "Buffer is too small");
                    Debug.Assert(colors.Buffer.Offset == 0, "Adding colors is expected for root only");
                    colors.SetElementUnsafe(Count, c);
                    Count += 1;
                    AdjustRanges(c);
                }

                [SecuritySafeCritical]
                internal Color32 ToColor()
                {
                    int count = Count;
                    Debug.Assert(count != 0, "Empty bucket");
                    if (count <= 0)
                        return default;

                    int rSum = 0, gSum = 0, bSum = 0;
                    for (int i = 0; i < count; i++)
                    {
                        Color32 color = colors.GetElementUnsafe(i);
                        rSum += color.R;
                        gSum += color.G;
                        bSum += color.B;
                    }

                    return new Color32((byte)(rSum / count),
                        (byte)(gSum / count),
                        (byte)(bSum / count));
                }

                internal void Freeze()
                {
                    Debug.Assert(colors.Buffer.Offset == 0, "Freezing expected for root only");
                    if (colors.Length != Count)
                        colors = colors.Slice(0, Count);
                }

                [SecuritySafeCritical]
                internal void Split(IAsyncContext context, ColorComponent component, ColorBucketCollection buckets, ref int bucketIndex, ref int buckedEndIndex)
                {
                    Debug.Assert(Count > 1);
                    Debug.Assert(Count == colors.Length, "Unfrozen root bucket");
                    var sorter = component switch
                    {
                        ColorComponent.R => redSorter,
                        ColorComponent.G => greenSorter,
                        _ => blueSorter,
                    };

                    CastArray<T, Color32> buf = colors;
                    if (!ParallelHelper.Sort(context, buf, sorter))
                        return;

                    int medianIndex = buf.Length >> 1;

                    // single color check is correct because we sorted by all the components
                    bool isLeftSingleColor = buf.GetElementUnsafe(0) == buf.GetElementUnsafe(medianIndex - 1);
                    bool isRightSingleColor = buf.GetElementUnsafe(medianIndex) == buf.GetElementUnsafe(buf.Length - 1);
                    ColorBucket? left = isLeftSingleColor ? null : new ColorBucket(buf.Slice(0, medianIndex), false);
                    ColorBucket? right = isRightSingleColor ? null : new ColorBucket(buf.Slice(medianIndex, buf.Length - medianIndex), false);

                    if (isLeftSingleColor)
                    {
                        buckets.AddFinalColor(buf.GetElementUnsafe(0));

                        // if none of the halves could be added, we remove the current bucket and reduce the number of buckets to scan
                        if (isRightSingleColor)
                        {
                            buckets.AddFinalColor(buf.GetElementUnsafe(medianIndex));
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
                        buckets.AddFinalColor(buf.GetElementUnsafe(medianIndex));
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

                internal void ReplaceBucket(int index, ColorBucket bucket) => buckets[index] = bucket;

                internal ColorBucket? RemoveFirstBucket()
                {
                    if (buckets.Count == 0)
                        return null;
                    ColorBucket result = buckets[0];
                    buckets.RemoveFirst();
                    return result;
                }

                internal void RemoveBucket(int index) => buckets.RemoveAt(index);

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
            private CastArray<T, Color32> colors;
            private ColorBucket root = null!;

            private bool hasTransparency;

            #endregion

            #region Properties

            private int MaxActualColors => maxColors - (hasTransparency ? 1 : 0);

            #endregion

            #region Methods

            public void Initialize(int requestedColors, byte? bitLevel, IBitmapData source)
            {
                Debug.Assert(default(T) is byte or Color32);
                int maxLevels = 1 << (bitLevel ?? 8);
                maxColors = Math.Min(requestedColors, maxLevels * maxLevels * maxLevels);

                // Renting byte arrays only. Allowing dirty arrays because root.Count is 0 initially.
                colors = typeof(T) == typeof(byte)
                    ? new ArraySection<T>((source.Width * source.Height) << 2, false)
                    : new ArraySection<T>(new T[source.Width * source.Height]);
                root = new ColorBucket(colors, true);
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
                // Occurs when the bitmap is completely transparent
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
                root.Freeze();
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
                colors.Buffer.Release();
                colors = CastArray<T, Color32>.Null;
                root = null!;
            }

            #endregion
        }
    }
}
