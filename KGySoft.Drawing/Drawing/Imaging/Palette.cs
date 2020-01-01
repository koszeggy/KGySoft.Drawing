using KGySoft.CoreLibraries;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using KGySoft.Collections;

namespace KGySoft.Drawing.Imaging
{
    internal class Palette
    {
        private readonly Color[] entries;
        private Color32[] indexToColor32;
        private Dictionary<Color32, int> color32ToIndex;

        private IThreadSafeCacheAccessor<Color32, int> nearestColorsCache;

        internal int Length => entries.Length;

        internal Palette(Color[] entries) => this.entries = entries;

        internal Color GetColor(int index)
        {
            Debug.Assert(index < entries.Length, $"Valid color index expected in {nameof(GetColor)}");
            return entries[index];
        }

        internal Color32 GetColor32(int index)
        {
            Debug.Assert(index < entries.Length, $"Valid color index expected in {nameof(GetColor)}");
            if (indexToColor32 == null)
                InitColorMaps();
            return indexToColor32[index];
        }

        internal int GetColorIndex(Color32 c)
        {
            if (color32ToIndex == null)
                InitColorMaps();
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

        private int FindNearestColorIndex(Color32 c) => c.GetNearestIndex(indexToColor32);

        private void InitColorMaps()
        {
            // internal type, cannot be accessed from outside so locking on this is alright
            lock (this)
            {
                // lost race
                if (indexToColor32 != null)
                    return;

                var toColor = new Color32[entries.Length];
                var toIndex = new Dictionary<Color32, int>(toColor.Length);

                for (int i = 0; i < toColor.Length; i++)
                {
                    var c32 = new Color32(entries[i]);
                    toColor[i] = c32;
                    if (!toIndex.ContainsKey(c32))
                        toIndex[c32] = i;
                }

                color32ToIndex = toIndex;
                indexToColor32 = toColor;
            }
        }
    }
}
