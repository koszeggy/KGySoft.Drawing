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
using System.Threading;

using KGySoft.Collections;

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal class Palette
    {
        #region Nested structs

        #region ColorInfo struct

        private struct ColorInfo
        {
            #region Fields

            internal readonly Color32 Color;
            internal readonly byte Saturation;
            internal readonly byte Brightness;

            #endregion

            #region Constructors

            internal ColorInfo(Color c) : this()
            {
                Color = new Color32(c);
                Saturation = (byte)(c.GetSaturation() * 255);
                Brightness = Color.GetBrightness();
            }

            #endregion
        }

        #endregion

        #endregion

        #region Fields

        private readonly Color[] entries;
        private readonly byte alphaThreshold;
        private readonly Color32 backColor;

        private ColorInfo[] colors;
        private int transparentIndex = -1;
        private bool grayScale;
        private Dictionary<Color32, int> color32ToIndex;
        private IThreadSafeCacheAccessor<Color32, int> nearestColorsCache;

        #endregion

        #region Properties

        internal int Length => entries.Length;

        #endregion

        #region Constructors

        internal Palette(Color[] entries, Color backColor, byte alphaThreshold)
        {
            this.entries = entries;
            this.alphaThreshold = alphaThreshold;
            this.backColor = Color32.FromArgb(Byte.MaxValue, new Color32(backColor));
        }

        #endregion

        #region Methods

        #region Internal Methods

        internal Color GetColor(int index)
        {
            Debug.Assert(index < entries.Length, $"Valid color index expected in {nameof(GetColor)}");
            return entries[index];
        }

        internal Color32 GetColor32(int index)
        {
            Debug.Assert(index < entries.Length, $"Valid color index expected in {nameof(GetColor)}");
            if (colors == null)
                InitColors();
            return colors[index].Color;
        }

        internal int GetColorIndex(Color32 c)
        {
            if (color32ToIndex == null)
                InitColors();
            if (color32ToIndex.TryGetValue(c, out int result))
                return result;

            // Caching results is a problem because a true color image can have millions of colors.
            // In order not to run out of memory we need to limit the cache size but that is another problem because:
            // - ConcurrentDictionary.Count is ridiculously expensive. Unbounded cache would consume a lot of memory.
            // - Cache.GetThreadSafeAccessor locks and if colors are never the same it will be worse than without caching.
            // Conclusion: Using Cache with a fairly large capacity without locking the item loader and hoping for the best.
            Interlocked.CompareExchange(ref nearestColorsCache,
                new Cache<Color32, int>(FindNearestColorIndex, 65536).GetThreadSafeAccessor(), null);
            return nearestColorsCache[c];
        }

        #endregion

        #region Private Methods

        private void InitColors()
        {
            // internal type, cannot be accessed from outside so locking on this is alright
            lock (this)
            {
                // lost race
                if (colors != null)
                    return;

                var info = new ColorInfo[entries.Length];
                var toIndex = new Dictionary<Color32, int>(info.Length);
                grayScale = true;

                for (int i = 0; i < info.Length; i++)
                {
                    var ci = new ColorInfo(entries[i]);
                    var c = ci.Color;
                    info[i] = ci;
                    if (!toIndex.ContainsKey(c) && !(alphaThreshold == 0 && c.A == 0))
                        toIndex[c] = i;

                    if (c.A == 0)
                    {
                        if (transparentIndex < 0)
                            transparentIndex = i;
                    }

                    if (grayScale)
                        grayScale = ci.Saturation == 0;
                }

                color32ToIndex = toIndex;
                colors = info;
            }
        }

        private int FindNearestColorIndex(Color32 color)
        {
            // handling transparency
            if (color.A < alphaThreshold && transparentIndex != -1)
                return transparentIndex;

            int minDiff = Int32.MaxValue;
            int resultIndex = 0;

            // blending the color with background
            Color32 c = color.A != Byte.MaxValue ? color.BlendWithBackground(backColor) : color;

            for (int i = 0; i < colors.Length; i++)
            {
                Color32 current = colors[i].Color;

                // Skipping fully transparent palette colors if they are excluded by threshold
                if (current.A == 0 && alphaThreshold == 0)
                    continue;

                // Exact match
                if (current == color)
                    return i;

                // Blending also the current palette color (only for translucent palette entries, if any)
                if (current.A != Byte.MaxValue)
                    current = current.BlendWithBackground(backColor);

                int diff = grayScale
                    ? Math.Abs(colors[i].Brightness - c.GetBrightness())
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
