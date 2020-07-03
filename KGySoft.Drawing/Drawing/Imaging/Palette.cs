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
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

#endregion

namespace KGySoft.Drawing.Imaging
{
    /// <summary>
    /// Represents an indexed set of colors and provides efficient color lookup with caching.
    /// <br/>See the <strong>Remarks</strong> section for details.
    /// </summary>
    /// <remarks>
    /// <para>The <see cref="Palette"/> class can be used to perform quick lookup operations (see <see cref="GetNearestColor">GetNearestColor</see>
    /// and <see cref="GetNearestColorIndex">GetNearestColorIndex</see> methods) to find the closest matching palette entry to any color.</para>
    /// <para>By default the lookup is performed by a slightly modified euclidean-like search but if the <see cref="Palette"/> contains grayscale entries only,
    /// then it is optimized for finding the best matching gray shade based on human perception. To override this logic a custom lookup routine can be passed to the constructors.</para>
    /// <para>If the <see cref="Palette"/> instance is created without a custom lookup logic, then the search results for non-palette-entry colors are cached.
    /// The cache is optimized for parallel processing and consists of multiple levels where the results are tried to be obtained from a non-locking
    /// storage in the first place. The theoretical maximum of cache size (apart from the actual palette entries) is 2 x 2<sup>18</sup> but
    /// as soon as that limit is reached the amount of stored elements are halved so the cache is somewhat optimized to store the most recently processed colors.</para>
    /// <para>In order to prevent caching you can pass a custom lookup logic to the constructors. It is expected to be fast (applying some direct mapping to a palette index, for example),
    /// or that it uses some custom caching (which should perform well also when queried concurrently).</para>
    /// <para>The palette can have any number of colors but as the typical usage is quantizing colors for indexed bitmaps the typical maximum palette size
    /// is 256. Generally, the more color the palette has the slower are the lookups for non-palette colors that are not cached yet.</para>
    /// </remarks>
    /// <threadsafety instance="false">If there is no custom lookup logic passed to the constructors, then members of this type are guaranteed to be safe for multi-threaded operations.
    /// If this type is initialized with a custom lookup logic, then thread-safety depends on the custom lookup implementation.</threadsafety>
    public sealed class Palette
    {
        #region Constants
        
        private const int minCacheSize = 128;
        private const int maxCacheSize = 1 << 18;

        #endregion

        #region Fields

        #region Static Fields

        private static Color32[] system8BppPalette;
        private static Color32[] system4BppPalette;
        private static Color32[] system1BppPalette;

        #endregion

        #region Instance Fields

        private readonly int transparentIndex = -1;
        private readonly Dictionary<Color32, int> color32ToIndex;
        private readonly Func<Color32, int> customGetNearestColorIndex;
        private readonly object syncRoot;

        private Dictionary<Color32, int> lockFreeCache;
        private Dictionary<Color32, int> lockingCache;

        #endregion

        #endregion

        #region Properties

        #region Static Properties

        #region Static Properties

        internal static Color32[] System8BppPalette
        {
            get
            {
                if (system8BppPalette != null)
                    return system8BppPalette;

                using (var bmp = new Bitmap(1, 1, PixelFormat.Format8bppIndexed))
                    return system8BppPalette = bmp.Palette.Entries.Select(c => new Color32(c)).ToArray();
            }
        }

        internal static Color32[] System4BppPalette
        {
            get
            {
                if (system4BppPalette != null)
                    return system4BppPalette;

                using (var bmp = new Bitmap(1, 1, PixelFormat.Format4bppIndexed))
                    return system4BppPalette = bmp.Palette.Entries.Select(c => new Color32(c)).ToArray();
            }
        }

        internal static Color32[] System1BppPalette
        {
            get
            {
                if (system1BppPalette != null)
                    return system1BppPalette;

                using (var bmp = new Bitmap(1, 1, PixelFormat.Format1bppIndexed))
                    return system1BppPalette = bmp.Palette.Entries.Select(c => new Color32(c)).ToArray();
            }
        }

        #endregion

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the number of color entries in the current <see cref="Palette"/>.
        /// </summary>
        public int Count => Entries.Length;

        /// <summary>
        /// Gets the background color. If a lookup operation (<see cref="GetNearestColor">GetNearestColor</see>, <see cref="GetNearestColorIndex">GetNearestColorIndex</see>)
        /// is performed with a color whose <see cref="Color32.A">Color32.A</see> field is equal to or greater than <see cref="AlphaThreshold"/>, and there is no exact match among the entries of this <see cref="Palette"/>,
        /// then the color will be blended with this color before performing the lookup.
        /// </summary>
        public Color32 BackColor { get; }

        /// <summary>
        /// If this <see cref="Palette"/> has a transparent entry, then gets a threshold value for the <see cref="Color32.A">Color32.A</see> field,
        /// under which lookup operations will return the first transparent color (<see cref="GetNearestColor">GetNearestColor</see>)
        /// or the index of the first transparent color (<see cref="GetNearestColorIndex">GetNearestColorIndex</see>) in the palette.
        /// </summary>
        public byte AlphaThreshold { get; }

        #endregion

        #region Internal Properties

        internal Color32[] Entries { get; }
        internal bool IsGrayscale { get; }

        #endregion

        #endregion

        #region Constructors

        #region Public Constructors
        
        /// <summary>
        /// Initializes a new instance of the <see cref="Palette"/> class.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="Palette"/> class for details.
        /// </summary>
        /// <param name="entries">The color entries to be stored by this <see cref="Palette"/> instance.</param>
        /// <param name="backColor">Specifies the background color for lookup operations (<see cref="GetNearestColor">GetNearestColor</see>, <see cref="GetNearestColorIndex">GetNearestColorIndex</see>).
        /// When a lookup is performed with a color, whose <see cref="Color32.A">Color32.A</see> field is equal to or greater than <paramref name="alphaThreshold"/>, and there is no exact match among the <paramref name="entries"/>,
        /// then the color to be found will be blended with this color before performing the lookup. The <see cref="Color32.A">Color32.A</see> field of the background color is ignored. This parameter is optional.
        /// <br/>Default value: The default value of the <see cref="Color32"/> type, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="alphaThreshold">If there is at least one completely transparent color among <paramref name="entries"/>,
        /// then specifies a threshold value for the <see cref="Color32.A">Color32.A</see> field, under which lookup operations will return the first transparent color (<see cref="GetNearestColor">GetNearestColor</see>)
        /// or the index of the first transparent color (<see cref="GetNearestColorIndex">GetNearestColorIndex</see>). This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <param name="customGetNearestColorIndex">A delegate specifying an optional custom lookup logic to obtain an index from <paramref name="entries"/> by a <see cref="Color32"/> instance.
        /// If specified, it must be thread-safe and it is expected to be fast. The results returned by the specified delegate are not cached. If <see langword="null"/>,
        /// then <see cref="GetNearestColor">GetNearestColor</see>, <see cref="GetNearestColorIndex">GetNearestColorIndex</see> will perform a sequential lookup by using a default logic and results will be cached. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="entries"/> must not be <see langword="null"/>.</exception>
        public Palette(Color32[] entries, Color32 backColor = default, byte alphaThreshold = 128, Func<Color32, int> customGetNearestColorIndex = null)
        {
            Entries = entries ?? throw new ArgumentNullException(nameof(entries), PublicResources.ArgumentNull);
            BackColor = backColor.ToOpaque();
            AlphaThreshold = alphaThreshold;

            // initializing color32ToIndex, which is the 1st level of caching
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

            this.customGetNearestColorIndex = customGetNearestColorIndex;
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
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Palette"/> class.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="Palette"/> class for details.
        /// </summary>
        /// <param name="entries">The color entries to be stored by this <see cref="Palette"/> instance. They will be converted to <see cref="Color32"/> instances internally.</param>
        /// <param name="backColor">Specifies the background color for lookup operations (<see cref="GetNearestColor">GetNearestColor</see>, <see cref="GetNearestColorIndex">GetNearestColorIndex</see>).
        /// When a lookup is performed with a color, whose <see cref="Color32.A">Color32.A</see> field is equal to or greater than <paramref name="alphaThreshold"/>, and there is no exact match among the <paramref name="entries"/>,
        /// then the color to be found will be blended with this color before performing the lookup. The <see cref="Color.A">Color.A</see> property of the background color is ignored. This parameter is optional.
        /// <br/>Default value: <see cref="Color.Empty"/>, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="alphaThreshold">If there is at least one completely transparent color among <paramref name="entries"/>,
        /// then specifies a threshold value for the <see cref="Color32.A">Color32.A</see> field, under which lookup operations will return the first transparent color (<see cref="GetNearestColor">GetNearestColor</see>)
        /// or the index of the first transparent color (<see cref="GetNearestColorIndex">GetNearestColorIndex</see>). This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <param name="customGetNearestColorIndex">A delegate specifying an optional custom lookup logic to obtain an index from <paramref name="entries"/> by a <see cref="Color32"/> instance.
        /// If specified, it must be thread-safe and it is expected to be fast. The results returned by the specified delegate are not cached. If <see langword="null"/>,
        /// then <see cref="GetNearestColor">GetNearestColor</see>, <see cref="GetNearestColorIndex">GetNearestColorIndex</see> will perform a sequential lookup by using a default logic and results will be cached. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="entries"/> must not be <see langword="null"/>.</exception>
        public Palette(Color[] entries, Color backColor = default, byte alphaThreshold = 128, Func<Color32, int> customGetNearestColorIndex = null)
            : this(entries?.Select(c => new Color32(c)).ToArray(), new Color32(backColor), alphaThreshold, customGetNearestColorIndex)
        {
        }

        #endregion

        #region Internal Constructors

        internal Palette(PixelFormat pixelFormat, Color32 backColor, byte alphaThreshold)
            : this(GetColorsByPixelFormat(pixelFormat), backColor, alphaThreshold)
        {
        }

        #endregion

        #endregion

        #region Methods

        #region Static Methods

        private static Color32[] GetColorsByPixelFormat(PixelFormat pixelFormat) => pixelFormat switch
        {
            PixelFormat.Format8bppIndexed => System8BppPalette,
            PixelFormat.Format4bppIndexed => System4BppPalette,
            PixelFormat.Format1bppIndexed => System1BppPalette,
            _ => throw new ArgumentOutOfRangeException(nameof(pixelFormat), PublicResources.ArgumentOutOfRange)
        };

        #endregion

        #region Instance Methods

        #region Public Methods

        /// <summary>
        /// Gets the color entry of this <see cref="Palette"/> at the specified <paramref name="index"/>.
        /// </summary>
        /// <param name="index">The index of the color entry to be retrieved.</param>
        /// <returns>A <see cref="Color32"/> instance representing the color entry of the <see cref="Palette"/> at the specified <paramref name="index"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> must be equal to or greater than zero and less <see cref="Count"/>.</exception>
        public Color32 GetColor(int index)
        {
            if ((uint)index >= (uint)Entries.Length)
                ThrowIndexInvalid();
            return Entries[index];
        }

        /// <summary>
        /// Gets the index of a <see cref="Palette"/> entry that is the nearest color to the specified <see cref="Color32"/> instance.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="c">The color for which the nearest palette entry index should be returned.</param>
        /// <returns>The index of a <see cref="Palette"/> entry that is the nearest color to the specified <see cref="Color32"/> instance.</returns>
        /// <remarks>
        /// <para>If the <see cref="Palette"/> does not contain the specified color, then the result may depend on the arguments passed to the constructor.</para>
        /// <para>If <paramref name="c"/> has transparency, then the result may depend on <see cref="BackColor"/> and <see cref="AlphaThreshold"/> values.</para>
        /// <para>The result can be customized by passing a non-<see langword="null"/>&#160;delegate to one of the <see cref="Palette"/> constructors.</para>
        /// <note>For more details see the <strong>Remarks</strong> section of the <see cref="Palette"/> class.</note>
        /// </remarks>
        public int GetNearestColorIndex(Color32 c)
        {
            // 1st level cache: from the palette
            if (color32ToIndex.TryGetValue(c, out int result))
                return result;

            // We have a custom logic: we do not cache the results of the extern logic
            if (customGetNearestColorIndex != null)
                return customGetNearestColorIndex.Invoke(c);

            // 2nd level cache: from the lock-free cache. This is null until we have enough cached colors.
            Dictionary<Color32, int> lockFreeCacheInstance = lockFreeCache;
            if (lockFreeCacheInstance != null && lockFreeCacheInstance.TryGetValue(c, out result))
                return result;

            // 3rd level cache: from the locking cache. This is null until the first non-palette color is queried.
            lock (syncRoot)
            {
                if (lockingCache == null)
                    lockingCache = new Dictionary<Color32, int>(minCacheSize);
                else if (lockingCache.TryGetValue(c, out result))
                    return result;
            }

            // The color was not found in any cache: a lookup has to be performed.
            // This operation is intentionally outside of a lock.
            result = FindNearestColorIndex(c);
            lock (syncRoot)
            {
                // As the lookup is outside of the lock now it can happen that an element is added twice or
                // that element count goes a bit above maxCacheSize but this is not a problem.
                lockingCache[c] = result;
                int lockFreeCount = lockFreeCacheInstance?.Count ?? 0;
                int lockingCount = lockingCache.Count;

                // We overwrite the lock-free cache if either the minimal size was reached
                // or enough new colors have been added to the locking cache. Even if more threads
                // performed a lookup before entering the lock this operation is performed only by the first thread.
                if (lockFreeCount == 0 && lockingCount >= minCacheSize
                    || lockFreeCount >= minCacheSize && lockingCount >= Math.Min(lockFreeCount << 1, maxCacheSize))
                {
                    // We clear (reinitialize) the locking cache only if it reaches the maximum capacity
                    if (lockingCache.Count >= maxCacheSize)
                    {
                        lockingCache = new Dictionary<Color32, int>(maxCacheSize);
                        lockFreeCache = lockingCache;
                    }
                    else
                        lockFreeCache = new Dictionary<Color32, int>(lockingCache);
                }
            }

            return result;
        }

        /// <summary>
        /// Gets a <see cref="Color32"/> entry of this <see cref="Palette"/> that is the nearest color to the specified <see cref="Color32"/> instance.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="c">The color for which the nearest palette entry should be returned.</param>
        /// <returns>The <see cref="Color32"/> entry of this <see cref="Palette"/> that is the nearest color to the specified <see cref="Color32"/> instance.</returns>
        /// <remarks>
        /// <para>If the <see cref="Palette"/> does not contain the specified color, then the result may depend on the arguments passed to the constructor.</para>
        /// <para>If <paramref name="c"/> has transparency, then the result may depend on <see cref="BackColor"/> and <see cref="AlphaThreshold"/> values.</para>
        /// <para>The result can be customized by passing a non-<see langword="null"/>&#160;delegate to one of the <see cref="Palette"/> constructors.</para>
        /// <note>For more details see the <strong>Remarks</strong> section of the <see cref="Palette"/> class.</note>
        /// </remarks>
        /// <exception cref="IndexOutOfRangeException">The <see cref="Palette"/> class was initialized by a custom lookup delegate, which returned an invalid index.</exception>
        public Color32 GetNearestColor(Color32 c) => Entries[GetNearestColorIndex(c)];

        /// <summary>
        /// Gets a read-only wrapper of the entries of this <see cref="Palette"/> instance.
        /// </summary>
        /// <returns>The entries of this <see cref="Palette"/>.</returns>
        public IList<Color32> GetEntries()
            // Return type could be IReadOnlyList but that is not available in .NET 3.5/4.0
            => new ReadOnlyCollection<Color32>(Entries);

        #endregion

        #region Internal Methods

        internal bool Equals(Color[] colors)
        {
            if (customGetNearestColorIndex != null || Entries.Length != colors.Length)
                return false;
            for (int i = 0; i < colors.Length; i++)
            {
                if (colors[i].ToArgb() != Entries[i].ToArgb())
                    return false;
            }

            return true;
        }

        internal bool Equals(Palette other, bool quickCheck)
        {
            if (other == null || customGetNearestColorIndex != other.customGetNearestColorIndex)
                return false;

            if (ReferenceEquals(other.Entries, Entries))
                return true;

            if (quickCheck)
                return false;

            Color32[] colors = other.Entries;
            if (colors.Length != Entries.Length)
                return false;

            // ReSharper disable once LoopCanBeConvertedToQuery - performance
            for (int i = 0; i < colors.Length; i++)
            {
                if (!colors[i].Equals(Entries[i]))
                    return false;
            }

            return true;
        }


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

        #endregion
    }
}
