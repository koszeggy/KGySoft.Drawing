#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: OptimizedPaletteQuantizer.MedianCut.cs
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
using System.Diagnostics.CodeAnalysis;

using KGySoft.Collections;

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
                private static readonly IComparer<Color32> redSorter = new RedComparer();
                private static readonly IComparer<Color32> greenSorter = new GreenComparer();
                private static readonly IComparer<Color32> blueSorter = new BlueComparer();

                private sealed class RedComparer : IComparer<Color32>
                {
                    public int Compare(Color32 a, Color32 b) => ((a.R << 16) | (a.G << 8) | a.B) - ((b.R << 16) | (b.G << 8) | b.B);
                }

                private sealed class GreenComparer : IComparer<Color32>
                {
                    public int Compare(Color32 a, Color32 b) => ((a.G << 16) | (a.R << 8) | a.B) - ((b.G << 16) | (b.R << 8) | b.B);
                }

                private sealed class BlueComparer : IComparer<Color32>
                {
                    public int Compare(Color32 a, Color32 b) => ((a.B << 16) | (a.G << 8) | a.R) - ((b.B << 16) | (b.G << 8) | b.R);
                }


                #region Fields

                private readonly List<Color32> colors;

                private int rMin;
                private int rMax;
                private int gMin;
                private int gMax;
                private int bMin;
                private int bMax;

                #endregion

                #region Properties

                internal int Count => colors.Count;
                internal int RangeR => rMax - rMin;
                internal int RangeG => gMax - gMin;
                internal int RangeB => bMax - bMin;
                internal bool IsSingleColor => RangeR == 0 && RangeG == 0 && RangeB == 0;

                #endregion

                #region Constructors

                internal ColorBucket(int size)
                {
                    colors = new List<Color32>(size);
                    rMin = gMin = bMin = Byte.MaxValue;
                }

                #endregion

                #region Methods

                internal void AddColor(Color32 c)
                {
                    colors.Add(c);

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

                internal Color32 ToColor()
                {
                    int count = colors.Count;
                    Debug.Assert(count != 0, "Empty bucket");
                    if (count <= 0)
                        return count == 0 ? default : colors[0];

                    int rSum = 0, gSum = 0, bSum = 0;
                    foreach (Color32 color in colors)
                    {
                        rSum += color.R;
                        gSum += color.G;
                        bSum += color.B;
                    }

                    return new Color32((byte)(rSum / count),
                        (byte)(gSum / count),
                        (byte)(bSum / count));
                }

                internal void Split(ColorComponent component, ColorBucketCollection buckets, ref int index, ref int endIndex)
                {
                    Debug.Assert(colors.Count > 1);

                    // always sorting by all of the components so then we can eliminate same color groups easily
                    switch (component)
                    {
                        case ColorComponent.R:
                            colors.Sort(redSorter);
                            break;

                        case ColorComponent.G:
                            colors.Sort(greenSorter);
                            break;

                        case ColorComponent.B:
                            colors.Sort(blueSorter);
                            break;
                    }

                    int medianIndex = colors.Count >> 1;

                    // single color check is correct because we sorted by all of the components
                    bool isLeftSingleColor = colors[0] == colors[medianIndex - 1]; 
                    bool isRightSingleColor = colors[medianIndex] == colors[colors.Count - 1];
                    ColorBucket? left = isLeftSingleColor ? null : new ColorBucket(medianIndex);
                    ColorBucket? right = isRightSingleColor ? null : new ColorBucket(colors.Count - medianIndex);

                    // populating the left and right buckets
                    int from = isLeftSingleColor ? (isRightSingleColor ? Int32.MaxValue : medianIndex) : 0;
                    int to = isRightSingleColor ? (isLeftSingleColor ? 0 : medianIndex) : colors.Count;
                    for (int i = from; i < to; i++)
                    {
                        if (i < medianIndex)
                            left!.AddColor(colors[i]);
                        else
                            right!.AddColor(colors[i]);
                    }

                    if (isLeftSingleColor)
                    {
                        buckets.AddFinalColor(colors[0]);

                        // if none of the halves could be added, we remove the current bucket and reduce the number of buckets to scan
                        if (isRightSingleColor)
                        {
                            buckets.AddFinalColor(colors[medianIndex]);
                            buckets.RemoveBucket(index);
                            endIndex -= 1;

                            return;
                        }

                        // the right half is assigned back to the original position
                        buckets.ReplaceBucket(index, right!);
                        index += 1;
                        return;
                    }

                    // the left half is assigned back to the original position
                    buckets.ReplaceBucket(index, left!);
                    index += 1;

                    if (isRightSingleColor)
                    {
                        buckets.AddFinalColor(colors[medianIndex]);
                        return;
                    }

                    // the right half is added as a new bucket
                    buckets.AddBucket(right!);
                }

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

                internal bool SplitBuckets(IAsyncContext context, ref int index)
                {
                    bool splitOccurred = false;

                    // saving length because we add new buckets during the iteration
                    int endIndex = buckets.Count;
                    if (index >= endIndex)
                        index = 0;
                    while (index < endIndex)
                    {
                        ColorBucket currentBucket = buckets[index];
                        Debug.Assert(currentBucket.Count > 0, "Empty bucket");
                        Debug.Assert(!currentBucket.IsSingleColor);

                        if (context.IsCancellationRequested)
                            return false;

                        splitOccurred = true;

                        // on equal distance splitting on the green range in the first place because of human perception
                        if (currentBucket.RangeG >= currentBucket.RangeR && currentBucket.RangeG >= currentBucket.RangeB)
                            currentBucket.Split(ColorComponent.G, this, ref index, ref endIndex);
                        else if (currentBucket.RangeR >= currentBucket.RangeB)
                            currentBucket.Split(ColorComponent.R, this, ref index, ref endIndex);
                        else
                            currentBucket.Split(ColorComponent.B, this, ref index, ref endIndex);

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
            [AllowNull]private ColorBucket root;

            private bool hasTransparency;

            #endregion

            #region Properties

            private int MaxActualColors => maxColors - (hasTransparency ? 1 : 0);

            #endregion


            #region Methods

            public void Initialize(int requestedColors, IBitmapData source)
            {
                maxColors = requestedColors;
                root = new ColorBucket(source.Width * source.Height);
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

            public void Dispose() => root = null;

            #endregion
        }
    }
}
