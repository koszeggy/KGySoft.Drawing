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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

#endregion

namespace KGySoft.Drawing.Imaging
{
    public sealed class Palette
    {
        #region Constants
        
        private const int minCacheSize = 128;
        private const int maxCacheSize = 1 << 18;
        
        #endregion

        #region Fields

        private readonly int transparentIndex = -1;
        private readonly Dictionary<Color32, int> color32ToIndex;
        private readonly Func<Color32, int> customGetNearestColorIndex;
        private readonly object syncRoot;

        private Dictionary<Color32, int> lockFreeCache;
        private Dictionary<Color32, int> lockingCache;

        #endregion

        #region Properties

        #region Public Properties

        public int Count => Entries.Length;
        public byte AlphaThreshold { get; }
        public Color32 BackColor { get; }

        #endregion

        #region Internal Properties

        internal Color32[] Entries { get; }
        internal bool IsGrayscale { get; }

        #endregion

        #endregion

        #region Constructors

        // if customGetNearestColorIndex is not null, then there is no internal caching
        public Palette(Color32[] entries, Color32 backColor = default, byte alphaThreshold = 128, Func<Color32, int> customGetNearestColorIndex = null)
        {
            Entries = entries ?? throw new ArgumentNullException(nameof(entries), PublicResources.ArgumentNull);
            BackColor = Color32.FromArgb(Byte.MaxValue, backColor);
            AlphaThreshold = alphaThreshold;
            this.customGetNearestColorIndex = customGetNearestColorIndex;
            color32ToIndex = new Dictionary<Color32, int>(entries.Length);
            IsGrayscale = true;

            for (int i = 0; i < entries.Length; i++)
            {
                Color32 c = entries[i];
                if (!color32ToIndex.ContainsKey(c) && !(AlphaThreshold == 0 && c.A == 0))
                    color32ToIndex[c] = i;

                if (c.A == 0)
                {
                    if (transparentIndex < 0)
                        transparentIndex = i;
                    continue;
                }

                if (IsGrayscale)
                    IsGrayscale = c.R == c.G && c.R == c.B;
            }

            if (customGetNearestColorIndex != null)
                return;

            // Caching results is a problem because a true color image can have millions of colors.
            // In order not to run out of memory we need to limit the cache size but that is another problem because:
            // - ConcurrentDictionary is actually quite slow in itself and its Count is ridiculously expensive, too.
            //   Using it without a size limit (so we don't need to read Count) ends up consuming way too much memory.
            // - Cache.GetThreadSafeAccessor uses lock and if colors are never the same caching is nothing but an additional cost.
            // Conclusion: We use two levels of caching (actually 3 with color32ToIndex) where there is a lock-free first level
            // cache and a second level used with locking. Only the locking cache is expanded continuously, which is regularly
            // copied to the lock-free cache if elements count reaches a limit. This makes lookups significantly faster even
            // with single-core processing. We use simple Dictionary instances. Not even a Cache because we handle both capacity and
            // expansion explicitly. Even the most commonly used elements are irrelevant because we copy the cache before clearing it.
            syncRoot = new object();
            lockingCache = new Dictionary<Color32, int>(maxCacheSize);
        }

        public Palette(Color[] entries, Color backColor = default, byte alphaThreshold = 128, Func<Color32, int> customGetNearestColorIndex = null)
            : this(entries?.Select(c => new Color32(c)).ToArray(), new Color32(backColor), alphaThreshold, customGetNearestColorIndex)
        {
        }

        #endregion

        #region Methods

        #region Public Methods

        public Color32 GetColor(int index)
        {
            if ((uint)index >= (uint)Entries.Length)
                ThrowIndexInvalid();
            return Entries[index];
        }

        public int GetColorIndex(Color32 c)
        {
            // 1st level cache: from the palette
            if (color32ToIndex.TryGetValue(c, out int result))
                return result;

            // We have a custom logic: we do not cache the results of the extern logic
            if (customGetNearestColorIndex != null)
                return customGetNearestColorIndex.Invoke(c);

            // 2nd level cache: from the lock-free cache. This is null until we have enough cached colors.
            var lockFreeCacheInstance = lockFreeCache;
            if (lockFreeCacheInstance != null && lockFreeCacheInstance.TryGetValue(c, out result))
                return result;

            // 3rd level cache: from the locking cache.
            lock (syncRoot)
            {
                if (lockingCache.TryGetValue(c, out result))
                    return result;
            }

            // The color was not found in any cache: a lookup has to be performed
            // This operation is intentionally outside of a lock.
            result = FindNearestColorIndex(c);
            lock (syncRoot)
            {
                // As lookup is outside of the lock now it can happen that an element is added twice or
                // that element count goes a bit above maxCacheSize but it is not a problem.
                lockingCache[c] = result;
                int lockFreeCount = lockFreeCache?.Count ?? 0;
                int lockingCount = lockingCache.Count;

                // We overwrite the lock-free cache if either the minimal size was reached
                // or enough new colors have been added to the locking cache. Even if more threads
                // performed a lookup before entering the lock this operation is performed only by the first thread.
                if (lockFreeCount == 0 && lockingCount >= minCacheSize
                    || lockFreeCount >= minCacheSize && lockingCount >= Math.Min(lockFreeCount << 1, maxCacheSize))
                {
                    lockFreeCache = new Dictionary<Color32, int>(lockingCache);

                    // We clear the locking cache only if it reaches the maximum capacity (by recreating it because Clear is slow)
                    if (lockingCache.Count >= maxCacheSize)
                        lockingCache = new Dictionary<Color32, int>(maxCacheSize);
                }
            }

            return result;
        }

        public Color32 GetNearestColor(Color32 c) => Entries[GetColorIndex(c)];

        // Could be IReadOnlyCollection but that is not available in .NET 3.5/4.0
        public IList<Color32> GetEntries() => new ReadOnlyCollection<Color32>(Entries);

        #endregion

        #region Private Methods

        private int FindNearestColorIndex(Color32 color)
        {
            // mapping alpha to full transparency
            if (color.A < AlphaThreshold && transparentIndex != -1)
                return transparentIndex;

            int minDiff = Int32.MaxValue;
            int resultIndex = 0;

            // blending the color with background and checking if there is an exact match now
            if (color.A != Byte.MaxValue)
            {
                color = color.BlendWithBackground(BackColor);
                if (color32ToIndex.TryGetValue(color, out resultIndex))
                    return resultIndex;
            }

            for (int i = 0; i < Entries.Length; i++)
            {
                Color32 current = Entries[i];

                // Palette color with alpha
                if (current.A != Byte.MaxValue)
                {
                    // Skipping fully transparent palette colors because they were handled above
                    if (current.A == 0)
                        continue;

                    // Blending also the current palette color
                    current = current.BlendWithBackground(BackColor);

                    // Exact match. Since the cache was checked before calling this method this can occur only after alpha blending.
                    if (current == color)
                        return i;
                }

                // If the palette is grayscale, then distance is measured by perceived brightness;
                // otherwise, by an Euclidean-like but much faster distance based on RGB components.
                int diff = IsGrayscale
                    ? Math.Abs(Entries[i].GetBrightness() - color.GetBrightness())
                    : Math.Abs(current.R - color.R) + Math.Abs(current.G - color.G) + Math.Abs(current.B - color.B);

                Debug.Assert(IsGrayscale || diff != 0, "Exact match should have been returned earlier");

                // new closest match
                if (diff < minDiff)
                {
                    minDiff = diff;
                    resultIndex = i;
                }
            }

            return resultIndex;
        }

        private void ThrowIndexInvalid()
        {
#pragma warning disable CA2208
            // ReSharper disable once NotResolvedInText
            throw new ArgumentOutOfRangeException("index", PublicResources.ArgumentMustBeBetween(0, Entries.Length - 1));
#pragma warning restore CA2208
        }

        #endregion

        #endregion
    }
}
