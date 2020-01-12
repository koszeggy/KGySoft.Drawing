#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: Palette.cs
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
using System.Drawing;
using System.Linq;

using KGySoft.Collections;

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal class Palette
    {
        #region Fields

        private readonly Color32[] entries;
        private readonly int transparentIndex = -1;
        private readonly bool isGrayscale;
        private readonly Dictionary<Color32, int> color32ToIndex;
        private readonly IThreadSafeCacheAccessor<Color32, int> nearestColorsCache;

        #endregion

        #region Properties

        internal int Count => entries.Length;
        internal Color32[] Entries => entries;
        internal byte AlphaThreshold { get; set; }
        internal Color32 BackColor { get; set; }

        #endregion

        #region Constructors

        internal Palette(Color32[] entries)
        {
            this.entries = entries;
            color32ToIndex = new Dictionary<Color32, int>(entries.Length);
            isGrayscale = true;

            for (int i = 0; i < entries.Length; i++)
            {
                Color32 c = entries[i];
                if (!color32ToIndex.ContainsKey(c) && !(AlphaThreshold == 0 && c.A == 0))
                    color32ToIndex[c] = i;

                if (c.A == 0)
                {
                    if (transparentIndex < 0)
                        transparentIndex = i;
                }

                if (isGrayscale)
                    isGrayscale = c.R == c.G && c.R == c.B;
            }

            // Caching results is a problem because a true color image can have millions of colors.
            // In order not to run out of memory we need to limit the cache size but that is another problem because:
            // - ConcurrentDictionary.Count is ridiculously expensive. Unbounded cache would consume a lot of memory.
            // - Cache.GetThreadSafeAccessor locks and if colors are never the same caching is nothing but an additional cost.
            // Conclusion: Using Cache with a fairly large capacity without locking the item loader and hoping for the best.
            nearestColorsCache = new Cache<Color32, int>(FindNearestColorIndex, 65536).GetThreadSafeAccessor();
        }

        internal Palette(Color[] entries)
            : this(entries.Select(c => new Color32(c)).ToArray())
        {
        }

        #endregion

        #region Methods

        #region Internal Methods

        internal Color32 GetColor(int index)
        {
            Debug.Assert(index < entries.Length, $"Valid color index expected in {nameof(GetColor)}");
            return entries[index];
        }

        internal int GetColorIndex(Color32 c)
            => color32ToIndex.TryGetValue(c, out int result)
                ? result
                : nearestColorsCache[c];

        internal Color32 GetNearestColor(Color32 c) => entries[GetColorIndex(c)];

        #endregion

        #region Private Methods

        private int FindNearestColorIndex(Color32 color)
        {
            // mapping alpha to full transparency
            if (color.A < AlphaThreshold && transparentIndex != -1)
                return transparentIndex;

            int minDiff = Int32.MaxValue;
            int resultIndex = 0;

            // blending the color with background
            Color32 c = color.A != Byte.MaxValue ? color.BlendWithBackground(BackColor) : color;

            for (int i = 0; i < entries.Length; i++)
            {
                Color32 current = entries[i];

                // Skipping fully transparent palette colors because they were handled above
                if (current.A == 0)
                    continue;

                // Exact match. Since color32ToIndex contains exact matches this can occur after alpha blending.
                if (current == color)
                    return i;

                // Blending also the current palette color (only for translucent palette entries, if any)
                if (current.A != Byte.MaxValue)
                    current = current.BlendWithBackground(BackColor);

                // If the palette is grayscale, then distance is measured by perceived brightness;
                // otherwise, by an Euclidean-like but much faster distance based on RGB components.
                int diff = isGrayscale
                    ? Math.Abs(entries[i].GetBrightness() - c.GetBrightness())
                    : Math.Abs(current.R - c.R) + Math.Abs(current.G - c.G) + Math.Abs(current.B - c.B);

                // new closest match
                if (diff < minDiff)
                {
                    minDiff = diff;
                    resultIndex = i;
                }
            }

            return resultIndex;
        }

        #endregion

        #endregion
    }
}
