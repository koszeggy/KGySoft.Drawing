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
using System.Diagnostics;

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
                    if (count == 0)
                        return default;

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

                internal void Split(ColorComponent component, List<ColorBucket> buckets, int index)
                {
                    Debug.Assert(buckets[index] == this);

                    switch (component)
                    {
                        case ColorComponent.R:
                            colors.Sort((a, b) => a.R - b.R);
                            break;

                        case ColorComponent.G:
                            colors.Sort((a, b) => a.G - b.G);
                            break;

                        case ColorComponent.B:
                            colors.Sort((a, b) => a.B - b.B);
                            break;
                    }

                    int medianIndex = colors.Count >> 1;

                    // populating the left and right buckets
                    var left = new ColorBucket(medianIndex);
                    var right = new ColorBucket(colors.Count - medianIndex);
                    for (int i = 0; i < colors.Count; i++)
                    {
                        if (i < medianIndex)
                            left.AddColor(colors[i]);
                        else
                            right.AddColor(colors[i]);
                    }

                    // the left half is assigned back to the original position
                    buckets[index] = left;

                    // while the right half is added as a new bucket
                    buckets.Add(right);
                }

                #endregion
            }

            #endregion

            #endregion

            #region Fields

            private int maxColors;
            private ColorBucket root;

            private bool hasTransparency;

            #endregion

            #region Properties

            private int MaxBuckets => maxColors - (hasTransparency ? 1 : 0);

            #endregion


            #region Methods

            #region Public Methods

            public void Initialize(int requestedColors, IBitmapDataAccessor source)
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

            public Color32[] GeneratePalette()
            {
                // Occurs when bitmap is completely transparent
                if (root.Count == 0)
                {
                    Debug.Assert(hasTransparency);
                    return new Color32[1];
                }

                var buckets = new List<ColorBucket> { root };

                // splitting the initial bucket until no more split can be done or desired color amount is reached
                while (buckets.Count < MaxBuckets)
                {
                    if (!SplitBuckets(buckets))
                        break;
                }

                Debug.Assert(buckets.Count <= MaxBuckets);
                var result = new Color32[buckets.Count + (hasTransparency ? 1 : 0)];
                for (int i = 0; i < buckets.Count; i++)
                    result[i] = buckets[i].ToColor();

                // If transparent color is needed, then it will be automatically the last color in the result
                return result;
            }

            #endregion

            #region Private Methods

            private bool SplitBuckets(List<ColorBucket> buckets)
            {
                bool splitOccurred = false;

                // caching length because we add new buckets during the iteration
                int origLength = buckets.Count;
                for (var index = 0; index < origLength; index++)
                {
                    ColorBucket currentBucket = buckets[index];
                    Debug.Assert(currentBucket.Count > 0, "Empty bucket");
                    if (currentBucket.Count == 1)
                        continue;

                    splitOccurred = true;

                    // on equal distance splitting on the green range in the first place because of human perception
                    if (currentBucket.RangeG >= currentBucket.RangeR && currentBucket.RangeG >= currentBucket.RangeB)
                        currentBucket.Split(ColorComponent.G, buckets, index);
                    else if (currentBucket.RangeR >= currentBucket.RangeB)
                        currentBucket.Split(ColorComponent.R, buckets, index);
                    else
                        currentBucket.Split(ColorComponent.B, buckets, index);

                    // Stopping if we reached maxColors. Note that Split increases buckets.Count.
                    if (buckets.Count == MaxBuckets)
                        return false;
                }

                return splitOccurred;
            }

            #endregion

            #endregion
        }
    }
}
