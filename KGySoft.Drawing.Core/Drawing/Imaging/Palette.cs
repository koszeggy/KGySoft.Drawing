#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: Palette.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2023 - All Rights Reserved
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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

using KGySoft.Collections;

#endregion

namespace KGySoft.Drawing.Imaging
{
    /// <summary>
    /// Represents an indexed set of colors and provides efficient color lookup with caching.
    /// To create an instance use the static methods or the constructors.
    /// </summary>
    /// <remarks>
    /// <para>The <see cref="Palette"/> class can be used to perform quick lookup operations (see <see cref="GetNearestColor">GetNearestColor</see>
    /// and <see cref="GetNearestColorIndex">GetNearestColorIndex</see> methods) to find the closest matching palette entry to any color.</para>
    /// <para>By default the lookup is performed by a slightly modified euclidean-like search but if the <see cref="Palette"/> contains grayscale entries only,
    /// then it is optimized for finding the best matching gray shade based on human perception. To override this logic a custom lookup routine can be passed to the constructors.</para>
    /// <para>If the <see cref="Palette"/> instance is created without a custom lookup logic, then the search results for non-palette-entry colors are cached.
    /// The cache is optimized for parallel processing. The theoretical maximum of cache size (apart from the actual palette entries) is 2 x 2<sup>18</sup> but
    /// as soon as that limit is reached the amount of stored elements are halved so the cache is somewhat optimized to store the most recently processed colors.</para>
    /// <para>In order to prevent caching you can pass a custom lookup logic to the constructors. It is expected to be fast (applying some direct mapping to a palette index,
    /// for example), or that it uses some custom caching (which should perform well also when queried concurrently).</para>
    /// <para>The palette can have any number of colors but as the typical usage is quantizing colors for indexed bitmaps the typical maximum palette size
    /// is 256. Generally, the more colors the palette has the slower are the lookups for non-palette colors that are not cached yet.</para>
    /// </remarks>
    /// <threadsafety instance="false">If there is no custom lookup logic passed to the constructors, then members of this type are guaranteed to be safe for multi-threaded operations.
    /// If this type is initialized with a custom lookup logic, then thread-safety depends on the custom lookup implementation.</threadsafety>
    [DebuggerDisplay("Count = {" + nameof(Count) + "}")]
    public sealed class Palette : IPalette
    {
        #region Constants
        
        private const int minCacheSize = 128;
        private const int maxCacheSize = 1 << 18;

        #endregion

        #region Fields

        #region Static Fields

        private static readonly LockFreeCacheOptions cacheOptions = new()
        {
            InitialCapacity = minCacheSize,
            ThresholdCapacity = maxCacheSize,
            HashingStrategy = HashingStrategy.Modulo
        };

        private static Color32[]? system8BppPalette;
        private static Color32[]? system4BppPalette;
        private static Color32[]? rgb332Palette;
        private static Color32[]? grayscale256Palette;
        private static Color32[]? grayscale16Palette;
        private static Color32[]? grayscale4Palette;
        private static Color32[]? blackAndWhitePalette;

        #endregion

        #region Instance Fields

        private readonly Dictionary<Color32, int> color32ToIndex;
        private readonly Func<Color32, IPalette, int>? customGetNearestColorIndex;

        private IThreadSafeCacheAccessor<Color32, int>? cache;
        private ColorF[]? entriesF;

        #endregion

        #endregion

        #region Properties and Indexers

        #region Properties

        #region Static Properties

        private static Color32[] System4BppPalette => system4BppPalette ??= new[]
        {
            new Color32(0xFF000000), new Color32(0xFF800000), new Color32(0xFF008000), new Color32(0xFF808000),
            new Color32(0xFF000080), new Color32(0xFF800080), new Color32(0xFF008080), new Color32(0xFF808080),
            new Color32(0xFFC0C0C0), new Color32(0xFFFF0000), new Color32(0xFF00FF00), new Color32(0xFFFFFF00),
            new Color32(0xFF0000FF), new Color32(0xFFFF00FF), new Color32(0xFF00FFFF), new Color32(0xFFFFFFFF)
        };

        private static Color32[] System8BppPalette
        {
            get
            {
                if (system8BppPalette != null)
                    return system8BppPalette;

                var result = new Color32[256];
                Array.Copy(System4BppPalette, 0, result, 0, 16);

                // web-safe colors: 6x6x6 (216) starting at 40
                const int unit = 0x33;
                for (int i = 0; i < 216; i++)
                    result[i + 40] = new Color32((byte)(i / 36 * unit), (byte)(i / 6 % 6 * unit), (byte)(i % 6 * unit));

                return system8BppPalette = result;
            }
        }

        private static Color32[] Rgb332Palette
        {
            get
            {
                if (rgb332Palette != null)
                    return rgb332Palette;

                var result = new Color32[256];
                for (int i = 0; i < 256; i++)
                {
                    byte r = (byte)(i & 0b11100000);
                    r |= (byte)((r >> 3) | (r >> 6));
                    byte g = (byte)((i & 0b00011100) << 3);
                    g |= (byte)((g >> 3) | (g >> 6));
                    byte b = (byte)((i & 0b00000011) << 6);
                    b |= (byte)((b >> 2) | (b >> 4) | (b >> 6));
                    result[i] = new Color32(r, g, b);
                }

                return rgb332Palette = result;
            }
        }

        private static Color32[] Grayscale256Palette
        {
            get
            {
                if (grayscale256Palette != null)
                    return grayscale256Palette;

                var result = new Color32[256];
                for (int i = 0; i < 256; i++)
                    result[i] = Color32.FromGray((byte)i);

                return grayscale256Palette = result;
            }
        }

        private static Color32[] Grayscale16Palette
        {
            get
            {
                if (grayscale16Palette != null)
                    return grayscale16Palette;

                var result = new Color32[16];
                for (int i = 0; i < 16; i++)
                    result[i] = Color32.FromGray((byte)((i << 4) | i));

                return grayscale16Palette = result;
            }
        }

        private static Color32[] Grayscale4Palette
        {
            get
            {
                if (grayscale4Palette != null)
                    return grayscale4Palette;

                var result = new Color32[4];
                for (int i = 0; i < 4; i++)
                {
                    byte br = (byte)((i & 0b00000011) << 6);
                    br |= (byte)((br >> 2) | (br >> 4) | (br >> 6));
                    result[i] = Color32.FromGray(br);
                }

                return grayscale4Palette = result;
            }
        }

        private static Color32[] BlackAndWhitePalette => blackAndWhitePalette ??= new[] { Color32.Black, Color32.White };

        #endregion

        #region Instance Properties
        
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

        /// <summary>
        /// Gets whether the palette consists only of grayscale entries.
        /// </summary>
        public bool IsGrayscale { get; }

        /// <summary>
        /// Gets whether the palette contains at least one entry that is not fully opaque.
        /// </summary>
        public bool HasAlpha { get; }

        /// <summary>
        /// Gets whether this <see cref="Palette"/> prefers blending and measuring distance in the linear color space when looking for a nearest color.
        /// </summary>
        /// <remarks>
        /// <para>If this palette uses a custom nearest color lookup, then it depends on the custom function whether it considers the value of this property.</para>
        /// <para>Please note that palette entries themselves always represent sRGB color values, regardless the value of this property.</para>
        /// </remarks>
        public bool PrefersLinearColorSpace { get; }

        #endregion

        #region Internal Properties

        internal bool HasMultiLevelAlpha { get; }
        internal Color32[] Entries { get; }
        internal int TransparentIndex { get; }
        internal bool HasTransparent => TransparentIndex >= 0;
        internal BlendingMode BlendingMode => PrefersLinearColorSpace ? BlendingMode.Linear : BlendingMode.Srgb;

        #endregion

        #endregion

        #endregion

        #region Indexers

        /// <summary>
        /// Gets the color entry of this <see cref="Palette"/> at the specified <paramref name="index"/>.
        /// </summary>
        /// <param name="index">The index of the color entry to be retrieved.</param>
        /// <returns>A <see cref="Color32"/> instance representing the color entry of the <see cref="Palette"/> at the specified <paramref name="index"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> must be equal to or greater than zero and less <see cref="Count"/>.</exception>
        public Color32 this[int index] => GetColor(index);

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
        /// then the color to be found will be blended with this color before performing the lookup. The <see cref="Color32.A">Color32.A</see> field of the background color is ignored.</param>
        /// <param name="alphaThreshold">If there is at least one completely transparent color among <paramref name="entries"/>,
        /// then specifies a threshold value for the <see cref="Color32.A">Color32.A</see> field, under which lookup operations will return the first transparent color (<see cref="GetNearestColor">GetNearestColor</see>)
        /// or the index of the first transparent color (<see cref="GetNearestColorIndex">GetNearestColorIndex</see>).</param>
        /// <param name="customGetNearestColorIndex">A delegate specifying an optional custom lookup logic to obtain an index from <paramref name="entries"/> by a <see cref="Color32"/> instance.
        /// If specified, it must be thread-safe and it is expected to be fast. The results returned by the specified delegate are not cached. If <see langword="null"/>,
        /// then <see cref="GetNearestColor">GetNearestColor</see> and <see cref="GetNearestColorIndex">GetNearestColorIndex</see> methods will perform a sequential lookup by using a default logic and results will be cached.</param>
        /// <exception cref="ArgumentNullException"><paramref name="entries"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="entries"/> is empty.</exception>
        [Obsolete("This constructor overload is obsolete. Use the overload with (IEnumerable<Color32>, Color32, byte, Func<Color32, IPalette, int>?) parameters instead.")]
        public Palette(Color32[] entries, Color32 backColor, byte alphaThreshold, Func<Color32, int>? customGetNearestColorIndex)
            : this((Color32[]?)entries.Clone()!, backColor, alphaThreshold, false,
                customGetNearestColorIndex == null ? null : (c, _) => customGetNearestColorIndex.Invoke(c))
        {
        }

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
        /// <param name="customGetNearestColorIndex">A delegate specifying an optional custom lookup logic to obtain an index from <paramref name="entries"/>
        /// by a <see cref="Color32"/> and an <see cref="IPalette"/> instance. If specified, it must be thread-safe and it is expected to be fast.
        /// The results returned by the specified delegate are not cached. Make sure you always obtain the palette properties such as <see cref="IPalette.BackColor"/>,
        /// <see cref="IPalette.AlphaThreshold"/> and <see cref="IPalette.PrefersLinearColorSpace"/> from the <see cref="IPalette"/> argument
        /// as these may change when creating a new <see cref="Palette"/> instance by the <see cref="Palette(Palette,bool,Color32,byte)"/> constructor.
        /// If <see langword="null"/>, then <see cref="GetNearestColor">GetNearestColor</see> and <see cref="GetNearestColorIndex">GetNearestColorIndex</see> methods will
        /// perform a sequential lookup by using a default logic and results will be cached. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="entries"/> must not be <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="entries"/> must not be empty.</exception>
        [SuppressMessage("ReSharper", "ConditionalAccessQualifierIsNonNullableAccordingToAPIContract",
            Justification = "It CAN be null, just must not be. Null check is in the called ctor.")]
        public Palette(IEnumerable<Color32> entries, Color32 backColor = default, byte alphaThreshold = 128, Func<Color32, IPalette, int>? customGetNearestColorIndex = null)
            : this(entries?.ToArray()!, backColor, alphaThreshold, false, customGetNearestColorIndex)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Palette"/> class.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="Palette"/> class for details.
        /// </summary>
        /// <param name="entries">The color entries to be stored by this <see cref="Palette"/> instance.</param>
        /// <param name="preferLinearColorSpace"><see langword="true"/> to indicate that <see cref="GetNearestColor">GetNearestColor</see>
        /// and <see cref="GetNearestColorIndex">GetNearestColorIndex</see> methods should perform blending and the nearest color lookup using the
        /// linear color space; otherwise, <see langword="false"/>. If <paramref name="customGetNearestColorIndex"/> is not <see langword="null"/>, then this is just
        /// a preference hint that can be respected by the custom function.</param>
        /// <param name="backColor">Specifies the background color for lookup operations (<see cref="GetNearestColor">GetNearestColor</see>, <see cref="GetNearestColorIndex">GetNearestColorIndex</see>).
        /// When a lookup is performed with a color, whose <see cref="Color32.A">Color32.A</see> field is equal to or greater than <paramref name="alphaThreshold"/>, and there is no exact match among the <paramref name="entries"/>,
        /// then the color to be found will be blended with this color before performing the lookup. The <see cref="Color32.A">Color32.A</see> field of the background color is ignored. This parameter is optional.
        /// <br/>Default value: The default value of the <see cref="Color32"/> type, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="alphaThreshold">If there is at least one completely transparent color among <paramref name="entries"/>,
        /// then specifies a threshold value for the <see cref="Color32.A">Color32.A</see> field, under which lookup operations will return the first transparent color (<see cref="GetNearestColor">GetNearestColor</see>)
        /// or the index of the first transparent color (<see cref="GetNearestColorIndex">GetNearestColorIndex</see>). This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <param name="customGetNearestColorIndex">A delegate specifying an optional custom lookup logic to obtain an index from <paramref name="entries"/>
        /// by a <see cref="Color32"/> and an <see cref="IPalette"/> instance. If specified, it must be thread-safe and it is expected to be fast.
        /// The results returned by the specified delegate are not cached. Make sure you always obtain the palette properties such as <see cref="IPalette.BackColor"/>,
        /// <see cref="IPalette.AlphaThreshold"/> and <see cref="IPalette.PrefersLinearColorSpace"/> from the <see cref="IPalette"/> argument
        /// as these may change when creating a new <see cref="Palette"/> instance by the <see cref="Palette(Palette,bool,Color32,byte)"/> constructor.
        /// If <see langword="null"/>, then <see cref="GetNearestColor">GetNearestColor</see> and <see cref="GetNearestColorIndex">GetNearestColorIndex</see> methods will
        /// perform a sequential lookup by using a default logic and results will be cached. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="entries"/> must not be <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="entries"/> must not be empty.</exception>
        [SuppressMessage("ReSharper", "ConditionalAccessQualifierIsNonNullableAccordingToAPIContract",
            Justification = "It CAN be null, just must not be. Null check is in the called ctor.")]
        public Palette(IEnumerable<Color32> entries, bool preferLinearColorSpace, Color32 backColor = default, byte alphaThreshold = 128, Func<Color32, IPalette, int>? customGetNearestColorIndex = null)
            : this(entries?.ToArray()!, backColor, alphaThreshold, preferLinearColorSpace, customGetNearestColorIndex)
        {
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
        /// then <see cref="GetNearestColor">GetNearestColor</see> and <see cref="GetNearestColorIndex">GetNearestColorIndex</see> methods will perform a sequential lookup by using a default logic and results will be cached. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="entries"/> must not be <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="entries"/> must not be empty.</exception>
        [SuppressMessage("ReSharper", "ConditionalAccessQualifierIsNonNullableAccordingToAPIContract",
            Justification = "It CAN be null, just must not be. Null check is in the called ctor.")]
        [Obsolete("This constructor overload is obsolete. Use the overload with (IEnumerable<Color32>, Color32, byte, Func<Color32, IPalette, int>?) parameters instead.")]
        public Palette(Color[] entries, Color backColor = default, byte alphaThreshold = 128, Func<Color32, int>? customGetNearestColorIndex = null)
            : this(entries?.Select(c => new Color32(c)).ToArray()!, new Color32(backColor), alphaThreshold, false,
                customGetNearestColorIndex == null ? null : (c, _) => customGetNearestColorIndex.Invoke(c))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Palette"/> class from another <paramref name="palette"/> using
        /// new <paramref name="backColor"/> and <paramref name="alphaThreshold"/> values.
        /// </summary>
        /// <param name="palette">The original <see cref="Palette"/> to get the colors from.</param>
        /// <param name="backColor">The desired <see cref="BackColor"/> of the new <see cref="Palette"/>. The <see cref="Color32.A">Color32.A</see> field of the background color is ignored.</param>
        /// <param name="alphaThreshold">The desired <see cref="AlphaThreshold"/> of the new <see cref="Palette"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="palette"/> is <see langword="null"/>.</exception>
        [SuppressMessage("ReSharper", "ConditionalAccessQualifierIsNonNullableAccordingToAPIContract", Justification = "It CAN be null, just must not be. Null check is in the called ctor.")]
        public Palette(Palette palette, Color32 backColor, byte alphaThreshold)
            : this(palette, palette?.PrefersLinearColorSpace ?? default, backColor, alphaThreshold)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Palette"/> class from another <paramref name="palette"/> using
        /// new <paramref name="backColor"/> and <paramref name="alphaThreshold"/> values and color space preference.
        /// </summary>
        /// <param name="palette">The original <see cref="Palette"/> to get the colors from.</param>
        /// <param name="preferLinearColorSpace"><see langword="true"/> to indicate that <see cref="GetNearestColor">GetNearestColor</see>
        /// and <see cref="GetNearestColorIndex">GetNearestColorIndex</see> methods should perform blending and the nearest color lookup using the
        /// linear color space; otherwise, <see langword="false"/>. If the original <paramref name="palette"/> uses a custom lookup function,
        /// then the value of this parameter might be ignored.</param>
        /// <param name="backColor">The desired <see cref="BackColor"/> of the new <see cref="Palette"/>. The <see cref="Color32.A">Color32.A</see> field of the background color is ignored.</param>
        /// <param name="alphaThreshold">The desired <see cref="AlphaThreshold"/> of the new <see cref="Palette"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="palette"/> is <see langword="null"/>.</exception>
        public Palette(Palette palette, bool preferLinearColorSpace, Color32 backColor, byte alphaThreshold)
        {
            if (palette == null)
                throw new ArgumentNullException(nameof(palette), PublicResources.ArgumentNull);
            Entries = palette.Entries;
            TransparentIndex = palette.TransparentIndex;
            BackColor = backColor.ToOpaque();
            AlphaThreshold = alphaThreshold;
            PrefersLinearColorSpace = preferLinearColorSpace;
            color32ToIndex = palette.color32ToIndex;
            IsGrayscale = palette.IsGrayscale;
            HasAlpha = palette.HasAlpha;
            HasMultiLevelAlpha = palette.HasMultiLevelAlpha;
            customGetNearestColorIndex = palette.customGetNearestColorIndex;
        }

        #endregion

        #region Internal Constructors

        internal Palette(Color32[] entries, Color32 backColor, byte alphaThreshold, bool useLinearColorSpace, Func<Color32, IPalette, int>? customGetNearestColorIndex)
        {
            Entries = entries ?? throw new ArgumentNullException(nameof(entries), PublicResources.ArgumentNull);
            if (entries.Length == 0)
                throw new ArgumentException(PublicResources.ArgumentEmpty, nameof(entries));

            TransparentIndex = -1;
            BackColor = backColor.ToOpaque();
            AlphaThreshold = alphaThreshold;
            PrefersLinearColorSpace = useLinearColorSpace;

            // initializing color32ToIndex, which is the 1st level of caching
            color32ToIndex = new Dictionary<Color32, int>(entries.Length);
            IsGrayscale = true;
            for (int i = 0; i < entries.Length; i++)
            {
                Color32 c = entries[i];
                if (!color32ToIndex.ContainsKey(c) && !(AlphaThreshold == 0 && c.A == 0))
                    color32ToIndex[c] = i;

                if (c.A != Byte.MaxValue)
                {
                    HasAlpha = true;
                    if (!HasMultiLevelAlpha)
                        HasMultiLevelAlpha = c.A > 0;

                    if (c.A == 0)
                    {
                        if (TransparentIndex < 0)
                            TransparentIndex = i;
                        continue;
                    }
                }

                if (IsGrayscale)
                    IsGrayscale = c.R == c.G && c.R == c.B;
            }

            this.customGetNearestColorIndex = customGetNearestColorIndex;
        }
        
        #endregion

        #endregion

        #region Methods

        #region Static Methods

        /// <summary>
        /// Gets a <see cref="Palette"/> instance that uses the system default 8-bit palette.
        /// This palette contains the 16 standard <a href="https://www.w3.org/TR/REC-html40/types.html#h-6.5" target="_blank">basic sRGB colors</a>,
        /// the "web-safe" palette of 216 colors as well as 24 transparent entries.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="PredefinedColorsQuantizer.SystemDefault8BppPalette">PredefinedColorsQuantizer.SystemDefault8BppPalette</see> method for details and some examples.
        /// </summary>
        /// <param name="backColor">Specifies the background color for lookup operations (<see cref="GetNearestColor">GetNearestColor</see>, <see cref="GetNearestColorIndex">GetNearestColorIndex</see>).
        /// When a lookup is performed with a color, whose <see cref="Color32.A">Color32.A</see> field is equal to or greater than <paramref name="alphaThreshold"/>, and there is no exact match among the palette entries,
        /// then the color to be found will be blended with this color before performing the lookup. The <see cref="Color32.A">Color32.A</see> field of the background color is ignored. This parameter is optional.
        /// <br/>Default value: The default value of the <see cref="Color32"/> type, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="alphaThreshold">Specifies a threshold value for the <see cref="Color32.A">Color32.A</see> field, under which lookup operations will return the first transparent color (<see cref="GetNearestColor">GetNearestColor</see>)
        /// or the index of the first transparent color (<see cref="GetNearestColorIndex">GetNearestColorIndex</see>). This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <returns>A <see cref="Palette"/> instance that uses the system default 8-bit palette.</returns>
        /// <seealso cref="PredefinedColorsQuantizer.SystemDefault8BppPalette"/>
        public static Palette SystemDefault8BppPalette(Color32 backColor = default, byte alphaThreshold = 128)
            => SystemDefault8BppPalette(false, backColor, alphaThreshold);

        /// <summary>
        /// Gets a <see cref="Palette"/> instance that uses the system default 8-bit palette.
        /// This palette contains the 16 standard <a href="https://www.w3.org/TR/REC-html40/types.html#h-6.5" target="_blank">basic sRGB colors</a>,
        /// the "web-safe" palette of 216 colors as well as 24 transparent entries.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="PredefinedColorsQuantizer.SystemDefault8BppPalette">PredefinedColorsQuantizer.SystemDefault8BppPalette</see> method for details and some examples.
        /// </summary>
        /// <param name="useLinearColorSpace"><see langword="true"/> to make <see cref="GetNearestColor">GetNearestColor</see>
        /// and <see cref="GetNearestColorIndex">GetNearestColorIndex</see> methods perform blending and the nearest color lookup using the
        /// linear color space; otherwise, <see langword="false"/>.</param>
        /// <param name="backColor">Specifies the background color for lookup operations (<see cref="GetNearestColor">GetNearestColor</see>, <see cref="GetNearestColorIndex">GetNearestColorIndex</see>).
        /// When a lookup is performed with a color, whose <see cref="Color32.A">Color32.A</see> field is equal to or greater than <paramref name="alphaThreshold"/>, and there is no exact match among the palette entries,
        /// then the color to be found will be blended with this color before performing the lookup. The <see cref="Color32.A">Color32.A</see> field of the background color is ignored. This parameter is optional.
        /// <br/>Default value: The default value of the <see cref="Color32"/> type, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="alphaThreshold">Specifies a threshold value for the <see cref="Color32.A">Color32.A</see> field, under which lookup operations will return the first transparent color (<see cref="GetNearestColor">GetNearestColor</see>)
        /// or the index of the first transparent color (<see cref="GetNearestColorIndex">GetNearestColorIndex</see>). This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <returns>A <see cref="Palette"/> instance that uses the system default 8-bit palette.</returns>
        /// <seealso cref="PredefinedColorsQuantizer.SystemDefault8BppPalette"/>
        public static Palette SystemDefault8BppPalette(bool useLinearColorSpace, Color32 backColor = default, byte alphaThreshold = 128)
            => new Palette(System8BppPalette, backColor, alphaThreshold, useLinearColorSpace, null);

        /// <summary>
        /// Gets a <see cref="Palette"/> instance that uses the system default 4-bit palette.
        /// This palette consists of the 16 standard <a href="https://www.w3.org/TR/REC-html40/types.html#h-6.5" target="_blank">basic sRGB colors</a>.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="PredefinedColorsQuantizer.SystemDefault4BppPalette">PredefinedColorsQuantizer.SystemDefault4BppPalette</see> method for details and some examples.
        /// </summary>
        /// <param name="backColor">Specifies the background color for lookup operations (<see cref="GetNearestColor">GetNearestColor</see>, <see cref="GetNearestColorIndex">GetNearestColorIndex</see>).
        /// When a lookup is performed with a color with transparency, then the color to be found will be blended with this color before performing the lookup.
        /// The <see cref="Color32.A">Color32.A</see> field of the background color is ignored. This parameter is optional.
        /// <br/>Default value: The default value of the <see cref="Color32"/> type, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <returns>A <see cref="Palette"/> instance that uses the system default 4-bit palette.</returns>
        /// <seealso cref="PredefinedColorsQuantizer.SystemDefault4BppPalette"/>
        public static Palette SystemDefault4BppPalette(Color32 backColor = default)
            => SystemDefault4BppPalette(false, backColor);

        /// <summary>
        /// Gets a <see cref="Palette"/> instance that uses the system default 4-bit palette.
        /// This palette consists of the 16 standard <a href="https://www.w3.org/TR/REC-html40/types.html#h-6.5" target="_blank">basic sRGB colors</a>.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="PredefinedColorsQuantizer.SystemDefault4BppPalette">PredefinedColorsQuantizer.SystemDefault4BppPalette</see> method for details and some examples.
        /// </summary>
        /// <param name="useLinearColorSpace"><see langword="true"/> to make <see cref="GetNearestColor">GetNearestColor</see>
        /// and <see cref="GetNearestColorIndex">GetNearestColorIndex</see> methods perform blending and the nearest color lookup using the
        /// linear color space; otherwise, <see langword="false"/>.</param>
        /// <param name="backColor">Specifies the background color for lookup operations (<see cref="GetNearestColor">GetNearestColor</see>, <see cref="GetNearestColorIndex">GetNearestColorIndex</see>).
        /// When a lookup is performed with a color with transparency, then the color to be found will be blended with this color before performing the lookup.
        /// The <see cref="Color32.A">Color32.A</see> field of the background color is ignored. This parameter is optional.
        /// <br/>Default value: The default value of the <see cref="Color32"/> type, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <returns>A <see cref="Palette"/> instance that uses the system default 4-bit palette.</returns>
        /// <seealso cref="PredefinedColorsQuantizer.SystemDefault4BppPalette"/>
        public static Palette SystemDefault4BppPalette(bool useLinearColorSpace, Color32 backColor = default)
            => new Palette(System4BppPalette, backColor, 128, useLinearColorSpace, null);

        /// <summary>
        /// Gets a <see cref="Palette"/> instance that uses the system default 1-bit palette.
        /// This palette consists of the black and white colors.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="PredefinedColorsQuantizer.SystemDefault1BppPalette">PredefinedColorsQuantizer.SystemDefault1BppPalette</see> method for details.
        /// </summary>
        /// <param name="backColor">Specifies the background color for lookup operations (<see cref="GetNearestColor">GetNearestColor</see>, <see cref="GetNearestColorIndex">GetNearestColorIndex</see>).
        /// When a lookup is performed with a color with transparency, then the color to be found will be blended with this color before performing the lookup.
        /// The <see cref="Color32.A">Color32.A</see> field of the background color is ignored. This parameter is optional.
        /// <br/>Default value: The default value of the <see cref="Color32"/> type, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <returns>A <see cref="Palette"/> instance that uses the system default 1-bit palette.</returns>
        /// <seealso cref="PredefinedColorsQuantizer.SystemDefault1BppPalette"/>
        public static Palette SystemDefault1BppPalette(Color32 backColor = default)
            => SystemDefault1BppPalette(false, backColor);

        /// <summary>
        /// Gets a <see cref="Palette"/> instance that uses the system default 1-bit palette.
        /// This palette consists of the black and white colors.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="PredefinedColorsQuantizer.SystemDefault1BppPalette">PredefinedColorsQuantizer.SystemDefault1BppPalette</see> method for details.
        /// </summary>
        /// <param name="useLinearColorSpace"><see langword="true"/> to make <see cref="GetNearestColor">GetNearestColor</see>
        /// and <see cref="GetNearestColorIndex">GetNearestColorIndex</see> methods perform blending and the nearest color lookup using the
        /// linear color space; otherwise, <see langword="false"/>.</param>
        /// <param name="backColor">Specifies the background color for lookup operations (<see cref="GetNearestColor">GetNearestColor</see>, <see cref="GetNearestColorIndex">GetNearestColorIndex</see>).
        /// When a lookup is performed with a color with transparency, then the color to be found will be blended with this color before performing the lookup.
        /// The <see cref="Color32.A">Color32.A</see> field of the background color is ignored. This parameter is optional.
        /// <br/>Default value: The default value of the <see cref="Color32"/> type, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <returns>A <see cref="Palette"/> instance that uses the system default 1-bit palette.</returns>
        /// <seealso cref="PredefinedColorsQuantizer.SystemDefault1BppPalette"/>
        public static Palette SystemDefault1BppPalette(bool useLinearColorSpace, Color32 backColor = default)
            => new Palette(BlackAndWhitePalette, backColor, 128, useLinearColorSpace, null);

        /// <summary>
        /// Gets a <see cref="Palette"/> instance that uses a 8-bit palette where red, green and blue components are encoded in 3, 3 and 2 bits, respectively.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="PredefinedColorsQuantizer.Rgb332">PredefinedColorsQuantizer.Rgb332</see> method for details and some examples.
        /// </summary>
        /// <param name="backColor">Specifies the background color for lookup operations (<see cref="GetNearestColor">GetNearestColor</see>, <see cref="GetNearestColorIndex">GetNearestColorIndex</see>).
        /// When a lookup is performed with a color with transparency, then the color to be found will be blended with this color before performing the lookup.
        /// The <see cref="Color32.A">Color32.A</see> field of the background color is ignored. This parameter is optional.
        /// <br/>Default value: The default value of the <see cref="Color32"/> type, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="directMapping"><see langword="true"/> to map any color directly to an index instead of searching for a nearest color,
        /// which is very fast but without dithering may end up in a noticeably poorer result and higher contrast;
        /// <see langword="false"/> to perform a lookup to determine nearest colors, which may be slower but more accurate. This parameter is optional.
        /// <br/>Default value: <see langword="false"/>.</param>
        /// <returns>A <see cref="Palette"/> instance that uses a 8-bit palette where red, green and blue components are encoded in 3, 3 and 2 bits, respectively.</returns>
        /// <seealso cref="PredefinedColorsQuantizer.Rgb332"/>
        public static Palette Rgb332(Color32 backColor = default, bool directMapping = false) => Rgb332(false, backColor, directMapping);

        /// <summary>
        /// Gets a <see cref="Palette"/> instance that uses a 8-bit palette where red, green and blue components are encoded in 3, 3 and 2 bits, respectively.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="PredefinedColorsQuantizer.Rgb332">PredefinedColorsQuantizer.Rgb332</see> method for details and some examples.
        /// </summary>
        /// <param name="useLinearColorSpace"><see langword="true"/> to make <see cref="GetNearestColor">GetNearestColor</see>
        /// and <see cref="GetNearestColorIndex">GetNearestColorIndex</see> methods perform blending and the nearest color lookup using the
        /// linear color space; otherwise, <see langword="false"/>. If <paramref name="directMapping"/> is <see langword="true"/>,
        /// then only affects blending possibly partially transparent source colors.</param>
        /// <param name="backColor">Specifies the background color for lookup operations (<see cref="GetNearestColor">GetNearestColor</see>, <see cref="GetNearestColorIndex">GetNearestColorIndex</see>).
        /// When a lookup is performed with a color with transparency, then the color to be found will be blended with this color before performing the lookup.
        /// The <see cref="Color32.A">Color32.A</see> field of the background color is ignored. This parameter is optional.
        /// <br/>Default value: The default value of the <see cref="Color32"/> type, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="directMapping"><see langword="true"/> to map any color directly to an index instead of searching for a nearest color,
        /// which is very fast but without dithering may end up in a noticeably poorer result and higher contrast;
        /// <see langword="false"/> to perform a lookup to determine nearest colors, which may be slower but more accurate. This parameter is optional.
        /// <br/>Default value: <see langword="false"/>.</param>
        /// <returns>A <see cref="Palette"/> instance that uses a 8-bit palette where red, green and blue components are encoded in 3, 3 and 2 bits, respectively.</returns>
        /// <seealso cref="PredefinedColorsQuantizer.Rgb332"/>
        public static Palette Rgb332(bool useLinearColorSpace, Color32 backColor = default, bool directMapping = false)
        {
            static int GetNearestColorIndex(Color32 c, IPalette palette)
            {
                if (c.A < Byte.MaxValue)
                    c = c.BlendWithBackground(palette.BackColor, palette.PrefersLinearColorSpace);

                return (c.R & 0b11100000) | ((c.G & 0b11100000) >> 3) | ((c.B & 0b11000000) >> 6);
            }

            return new Palette(Rgb332Palette, useLinearColorSpace, backColor, 0, directMapping ? GetNearestColorIndex : default);
        }

        /// <summary>
        /// Gets a <see cref="Palette"/> instance that uses a 8-bit grayscale palette of 256 shades.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="PredefinedColorsQuantizer.Grayscale">PredefinedColorsQuantizer.Grayscale</see> method for details and some examples.
        /// </summary>
        /// <param name="backColor">Specifies the background color for lookup operations (<see cref="GetNearestColor">GetNearestColor</see>, <see cref="GetNearestColorIndex">GetNearestColorIndex</see>).
        /// When a lookup is performed with a color with transparency, then the color to be found will be blended with this color before performing the lookup.
        /// The <see cref="Color32.A">Color32.A</see> field of the background color is ignored. This parameter is optional.
        /// <br/>Default value: The default value of the <see cref="Color32"/> type, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <returns>A <see cref="Palette"/> instance that uses a 8-bit grayscale palette of 256 shades.</returns>
        /// <seealso cref="PredefinedColorsQuantizer.Grayscale"/>
        public static Palette Grayscale256(Color32 backColor = default) => Grayscale256(false, backColor);

        /// <summary>
        /// Gets a <see cref="Palette"/> instance that uses a 8-bit grayscale palette of 256 shades.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="PredefinedColorsQuantizer.Grayscale">PredefinedColorsQuantizer.Grayscale</see> method for details and some examples.
        /// </summary>
        /// <param name="useLinearColorSpace"><see langword="true"/> to make <see cref="GetNearestColor">GetNearestColor</see>
        /// and <see cref="GetNearestColorIndex">GetNearestColorIndex</see> methods perform blending and the nearest color lookup using the
        /// linear color space; otherwise, <see langword="false"/>.</param>
        /// <param name="backColor">Specifies the background color for lookup operations (<see cref="GetNearestColor">GetNearestColor</see>, <see cref="GetNearestColorIndex">GetNearestColorIndex</see>).
        /// When a lookup is performed with a color with transparency, then the color to be found will be blended with this color before performing the lookup.
        /// The <see cref="Color32.A">Color32.A</see> field of the background color is ignored. This parameter is optional.
        /// <br/>Default value: The default value of the <see cref="Color32"/> type, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <returns>A <see cref="Palette"/> instance that uses a 8-bit grayscale palette of 256 shades.</returns>
        /// <seealso cref="PredefinedColorsQuantizer.Grayscale"/>
        public static Palette Grayscale256(bool useLinearColorSpace, Color32 backColor = default)
        {
            static int GetNearestColorIndex(Color32 c, IPalette palette)
            {
                if (c.A < Byte.MaxValue)
                    c = c.BlendWithBackground(palette.BackColor, palette.PrefersLinearColorSpace);
                return c.GetBrightness();
            }

            return new Palette(Grayscale256Palette, backColor, 0, useLinearColorSpace, GetNearestColorIndex);
        }

        /// <summary>
        /// Gets a <see cref="Palette"/> instance that uses a 4-bit grayscale palette of 16 shades.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="PredefinedColorsQuantizer.Grayscale16">PredefinedColorsQuantizer.Grayscale16</see> method for details and some examples.
        /// </summary>
        /// <param name="backColor">Specifies the background color for lookup operations (<see cref="GetNearestColor">GetNearestColor</see>, <see cref="GetNearestColorIndex">GetNearestColorIndex</see>).
        /// When a lookup is performed with a color with transparency, then the color to be found will be blended with this color before performing the lookup.
        /// The <see cref="Color32.A">Color32.A</see> field of the background color is ignored. This parameter is optional.
        /// <br/>Default value: The default value of the <see cref="Color32"/> type, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="directMapping"><see langword="true"/> to map any color directly to an index instead of searching for a nearest color,
        /// which is very fast but may end up in a result of a bit higher contrast than the original image;
        /// <see langword="false"/> to perform a lookup to determine nearest colors, which may be slower but more accurate. This parameter is optional.
        /// <br/>Default value: <see langword="false"/>.</param>
        /// <returns>A <see cref="Palette"/> instance that uses a 4-bit grayscale palette of 16 shades.</returns>
        /// <seealso cref="PredefinedColorsQuantizer.Grayscale16"/>
        public static Palette Grayscale16(Color32 backColor = default, bool directMapping = false) => Grayscale16(false, backColor, directMapping);

        /// <summary>
        /// Gets a <see cref="Palette"/> instance that uses a 4-bit grayscale palette of 16 shades.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="PredefinedColorsQuantizer.Grayscale16">PredefinedColorsQuantizer.Grayscale16</see> method for details and some examples.
        /// </summary>
        /// <param name="useLinearColorSpace"><see langword="true"/> to make <see cref="GetNearestColor">GetNearestColor</see>
        /// and <see cref="GetNearestColorIndex">GetNearestColorIndex</see> methods perform blending and the nearest color lookup using the
        /// linear color space; otherwise, <see langword="false"/>. If <paramref name="directMapping"/> is <see langword="true"/>,
        /// then only affects blending possibly partially transparent source colors.</param>
        /// <param name="backColor">Specifies the background color for lookup operations (<see cref="GetNearestColor">GetNearestColor</see>, <see cref="GetNearestColorIndex">GetNearestColorIndex</see>).
        /// When a lookup is performed with a color with transparency, then the color to be found will be blended with this color before performing the lookup.
        /// The <see cref="Color32.A">Color32.A</see> field of the background color is ignored. This parameter is optional.
        /// <br/>Default value: The default value of the <see cref="Color32"/> type, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="directMapping"><see langword="true"/> to map any color directly to an index instead of searching for a nearest color,
        /// which is very fast but may end up in a result of a bit higher contrast than the original image;
        /// <see langword="false"/> to perform a lookup to determine nearest colors, which may be slower but more accurate. This parameter is optional.
        /// <br/>Default value: <see langword="false"/>.</param>
        /// <returns>A <see cref="Palette"/> instance that uses a 4-bit grayscale palette of 16 shades.</returns>
        /// <seealso cref="PredefinedColorsQuantizer.Grayscale16"/>
        public static Palette Grayscale16(bool useLinearColorSpace, Color32 backColor = default, bool directMapping = false)
        {
            static int GetNearestColorIndex(Color32 c, IPalette palette)
            {
                if (c.A < Byte.MaxValue)
                    c = c.BlendWithBackground(palette.BackColor, palette.PrefersLinearColorSpace);
                return c.GetBrightness() >> 4;
            }

            return new Palette(Grayscale16Palette, backColor, 0, useLinearColorSpace, directMapping ? GetNearestColorIndex : default);
        }

        /// <summary>
        /// Gets a <see cref="Palette"/> instance that uses a grayscale palette of 4 shades.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="PredefinedColorsQuantizer.Grayscale4">PredefinedColorsQuantizer.Grayscale4</see> method for details and some examples.
        /// </summary>
        /// <param name="backColor">Specifies the background color for lookup operations (<see cref="GetNearestColor">GetNearestColor</see>, <see cref="GetNearestColorIndex">GetNearestColorIndex</see>).
        /// When a lookup is performed with a color with transparency, then the color to be found will be blended with this color before performing the lookup.
        /// The <see cref="Color32.A">Color32.A</see> field of the background color is ignored. This parameter is optional.
        /// <br/>Default value: The default value of the <see cref="Color32"/> type, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="directMapping"><see langword="true"/> to map any color directly to an index instead of searching for a nearest color,
        /// which is very fast but may end up in a result of a bit higher contrast than the original image;
        /// <see langword="false"/> to perform a lookup to determine nearest colors, which may be slower but more accurate. This parameter is optional.
        /// <br/>Default value: <see langword="false"/>.</param>
        /// <returns>A <see cref="Palette"/> instance that uses a grayscale palette of 4 shades.</returns>
        /// <seealso cref="PredefinedColorsQuantizer.Grayscale4"/>
        public static Palette Grayscale4(Color32 backColor = default, bool directMapping = false) => Grayscale4(false, backColor, directMapping);

        /// <summary>
        /// Gets a <see cref="Palette"/> instance that uses a grayscale palette of 4 shades.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="PredefinedColorsQuantizer.Grayscale4">PredefinedColorsQuantizer.Grayscale4</see> method for details and some examples.
        /// </summary>
        /// <param name="useLinearColorSpace"><see langword="true"/> to make <see cref="GetNearestColor">GetNearestColor</see>
        /// and <see cref="GetNearestColorIndex">GetNearestColorIndex</see> methods perform blending and the nearest color lookup using the
        /// linear color space; otherwise, <see langword="false"/>. If <paramref name="directMapping"/> is <see langword="true"/>,
        /// then only affects blending possibly partially transparent source colors.</param>
        /// <param name="backColor">Specifies the background color for lookup operations (<see cref="GetNearestColor">GetNearestColor</see>, <see cref="GetNearestColorIndex">GetNearestColorIndex</see>).
        /// When a lookup is performed with a color with transparency, then the color to be found will be blended with this color before performing the lookup.
        /// The <see cref="Color32.A">Color32.A</see> field of the background color is ignored. This parameter is optional.
        /// <br/>Default value: The default value of the <see cref="Color32"/> type, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="directMapping"><see langword="true"/> to map any color directly to an index instead of searching for a nearest color,
        /// which is very fast but may end up in a result of a bit higher contrast than the original image;
        /// <see langword="false"/> to perform a lookup to determine nearest colors, which may be slower but more accurate. This parameter is optional.
        /// <br/>Default value: <see langword="false"/>.</param>
        /// <returns>A <see cref="Palette"/> instance that uses a grayscale palette of 4 shades.</returns>
        /// <seealso cref="PredefinedColorsQuantizer.Grayscale4"/>
        public static Palette Grayscale4(bool useLinearColorSpace, Color32 backColor = default, bool directMapping = false)
        {
            static int GetNearestColorIndex(Color32 c, IPalette palette)
            {
                if (c.A < Byte.MaxValue)
                    c = c.BlendWithBackground(palette.BackColor, palette.PrefersLinearColorSpace);
                return c.GetBrightness() >> 6;
            }

            return new Palette(Grayscale4Palette, backColor, 0, useLinearColorSpace, directMapping ? GetNearestColorIndex : default);
        }

        /// <summary>
        /// Gets a <see cref="Palette"/> instance that uses the black and white colors.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="PredefinedColorsQuantizer.BlackAndWhite">PredefinedColorsQuantizer.BlackAndWhite</see> method for details and some examples.
        /// </summary>
        /// <param name="backColor">Specifies the background color for lookup operations (<see cref="GetNearestColor">GetNearestColor</see>, <see cref="GetNearestColorIndex">GetNearestColorIndex</see>).
        /// When a lookup is performed with a color with transparency, then the color to be found will be blended with this color before performing the lookup.
        /// The <see cref="Color32.A">Color32.A</see> field of the background color is ignored. This parameter is optional.
        /// <br/>Default value: The default value of the <see cref="Color32"/> type, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="whiteThreshold">Specifies a threshold value for the brightness of the colors, under which the result of a color lookup is considered black.
        /// If 0, then all colors are mapped to white. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <returns>A <see cref="Palette"/> instance that uses the black and white colors.</returns>
        /// <seealso cref="PredefinedColorsQuantizer.BlackAndWhite"/>
        public static Palette BlackAndWhite(Color32 backColor = default, byte whiteThreshold = 128) => BlackAndWhite(false, backColor, whiteThreshold);

        /// <summary>
        /// Gets a <see cref="Palette"/> instance that uses the black and white colors.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="PredefinedColorsQuantizer.BlackAndWhite">PredefinedColorsQuantizer.BlackAndWhite</see> method for details and some examples.
        /// </summary>
        /// <param name="useLinearColorSpace"><see langword="true"/> to make <see cref="GetNearestColor">GetNearestColor</see>
        /// and <see cref="GetNearestColorIndex">GetNearestColorIndex</see> methods perform blending and the nearest color lookup using the
        /// linear color space; otherwise, <see langword="false"/>.</param>
        /// <param name="backColor">Specifies the background color for lookup operations (<see cref="GetNearestColor">GetNearestColor</see>, <see cref="GetNearestColorIndex">GetNearestColorIndex</see>).
        /// When a lookup is performed with a color with transparency, then the color to be found will be blended with this color before performing the lookup.
        /// The <see cref="Color32.A">Color32.A</see> field of the background color is ignored. This parameter is optional.
        /// <br/>Default value: The default value of the <see cref="Color32"/> type, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="whiteThreshold">Specifies a threshold value for the brightness of the colors, under which the result of a color lookup is considered black.
        /// If 0, then all colors are mapped to white. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <returns>A <see cref="Palette"/> instance that uses the black and white colors.</returns>
        /// <seealso cref="PredefinedColorsQuantizer.BlackAndWhite"/>
        public static Palette BlackAndWhite(bool useLinearColorSpace, Color32 backColor = default, byte whiteThreshold = 128)
        {
            // NOTE: unlike the other custom functions this one captures a parameter, whiteThreshold,
            //       which is not a problem as long as it cannot be modified by the copy constructors
            int GetNearestColorIndex(Color32 c, IPalette palette)
            {
                bool linear = palette.PrefersLinearColorSpace;
                if (c.A < Byte.MaxValue)
                    c = c.BlendWithBackground(palette.BackColor, linear);

                return c == Color32.Black ? 0
                    : c == Color32.White ? 1
                    : linear ? c.ToColorF().GetBrightness() * 255f >= whiteThreshold ? 1 : 0
                    : c.GetBrightness() >= whiteThreshold ? 1 : 0;
            }

            return new Palette(BlackAndWhitePalette, backColor, 0, useLinearColorSpace, GetNearestColorIndex);
        }

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
                ThrowIndexInvalid(index);
            return Entries[index];
        }

        /// <summary>
        /// Gets the index of a <see cref="Palette"/> entry that is the nearest color to the specified <see cref="Color32"/> instance.
        /// </summary>
        /// <param name="c">The color for which the nearest palette entry index should be returned.</param>
        /// <returns>The index of a <see cref="Palette"/> entry that is the nearest color to the specified <see cref="Color32"/> instance.</returns>
        /// <remarks>
        /// <para>If the <see cref="Palette"/> does not contain the specified color, then the result may depend on the arguments passed to the constructor.</para>
        /// <para>If <paramref name="c"/> has transparency, then the result may depend on <see cref="BackColor"/> and <see cref="AlphaThreshold"/> values.</para>
        /// <para>The result can be customized by passing a non-<see langword="null"/> delegate to one of the <see cref="Palette"/> constructors.</para>
        /// <note>For more details see the <strong>Remarks</strong> section of the <see cref="Palette"/> class.</note>
        /// </remarks>
        public int GetNearestColorIndex(Color32 c)
        {
            // exact match: from the palette
            if (color32ToIndex.TryGetValue(c, out int result))
                return result;

            // We have a custom logic: we do not cache the results of the extern logic
            if (customGetNearestColorIndex != null)
                return customGetNearestColorIndex.Invoke(c, this);

            // from the lock-free cache
            if (cache == null)
                Interlocked.CompareExchange(ref cache, ThreadSafeCacheFactory.Create<Color32, int>(PrefersLinearColorSpace ? FindNearestColorIndexLinear : FindNearestColorIndexSrgb, cacheOptions), null);
            return cache[c];
        }

        /// <summary>
        /// Gets a <see cref="Color32"/> entry of this <see cref="Palette"/> that is the nearest color to the specified <see cref="Color32"/> instance.
        /// </summary>
        /// <param name="c">The color for which the nearest palette entry should be returned.</param>
        /// <returns>The <see cref="Color32"/> entry of this <see cref="Palette"/> that is the nearest color to the specified <see cref="Color32"/> instance.</returns>
        /// <remarks>
        /// <para>If the <see cref="Palette"/> does not contain the specified color, then the result may depend on the arguments passed to the constructor.</para>
        /// <para>If <paramref name="c"/> has transparency, then the result may depend on <see cref="BackColor"/> and <see cref="AlphaThreshold"/> values.</para>
        /// <para>The result can be customized by passing a non-<see langword="null"/> delegate to one of the <see cref="Palette"/> constructors.</para>
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

        internal bool Equals(Palette? other)
        {
            // not a public method because we don't want to adjust GetHashCode to these comparisons
            if (other == null || customGetNearestColorIndex != other.customGetNearestColorIndex || !BackColor.Equals(other.BackColor) || AlphaThreshold != other.AlphaThreshold)
                return false;

            return EntriesEqual(other);
        }

        internal bool EntriesEqual(Palette? other)
        {
            if (other == null)
                return false;

            Color32[] colors = other.Entries;
            if (ReferenceEquals(colors, Entries))
                return true;
            if (Entries.Length != colors.Length)
                return false;

            for (int i = 0; i < colors.Length; i++)
            {
                if (colors[i] != Entries[i])
                    return false;
            }

            return true;
        }

        #endregion

        #region Private Methods

        private int FindNearestColorIndexSrgb(Color32 color)
        {
            // mapping alpha to full transparency
            if (color.A < AlphaThreshold && TransparentIndex != -1)
                return TransparentIndex;

            int minDiff = Int32.MaxValue;
            int resultIndex = 0;

            // blending the color with background and checking if there is an exact match now
            if (color.A != Byte.MaxValue)
            {
                color = color.BlendWithBackgroundSrgb(BackColor);
                if (color32ToIndex.TryGetValue(color, out resultIndex))
                    return resultIndex;
            }

            // The two similar lookups could be merged but it is faster to separate them even if some parts are duplicated
            int len = Entries.Length;
            if (IsGrayscale)
            {
                byte brightness = color.GetBrightness();
                for (int i = 0; i < len; i++)
                {
                    Color32 current = Entries[i];

                    // Palette color with alpha
                    if (current.A != Byte.MaxValue)
                    {
                        // Skipping fully transparent palette colors because they were handled above
                        if (current.A == 0)
                            continue;

                        // Blending also the current palette color
                        current = current.BlendWithBackgroundSrgb(BackColor);

                        // Exact match. Since the cache was checked before calling this method this can occur only after alpha blending.
                        if (current == color)
                            return i;
                    }

                    // If the palette is grayscale, then distance is measured by perceived brightness;
                    // otherwise, by an Euclidean-like but much faster distance based on RGB components.
                    int diff = Math.Abs(Entries[i].GetBrightness() - brightness);

                    if (diff >= minDiff)
                        continue;

                    // new closest match
                    if (diff == 0)
                        return i;
                    minDiff = diff;
                    resultIndex = i;
                }
            }
            else
            {
                for (int i = 0; i < len; i++)
                {
                    Color32 current = Entries[i];

                    // Palette color with alpha
                    if (current.A != Byte.MaxValue)
                    {
                        // Skipping fully transparent palette colors because they were handled above
                        if (current.A == 0)
                            continue;

                        // Blending also the current palette color
                        current = current.BlendWithBackgroundSrgb(BackColor);

                        // Exact match. Since the cache was checked before calling this method this can occur only after alpha blending.
                        if (current == color)
                            return i;
                    }

                    // If the palette is grayscale, then distance is measured by perceived brightness;
                    // otherwise, by an Euclidean-like but much faster distance based on RGB components.
                    int diff = Math.Abs(current.R - color.R) + Math.Abs(current.G - color.G) + Math.Abs(current.B - color.B);

                    Debug.Assert(diff != 0, "Exact match should have been returned earlier");

                    // new closest match
                    if (diff < minDiff)
                    {
                        minDiff = diff;
                        resultIndex = i;
                    }
                }
            }

            return resultIndex;
        }

        private int FindNearestColorIndexLinear(Color32 color)
        {
            // mapping alpha to full transparency
            if (color.A < AlphaThreshold && TransparentIndex != -1)
                return TransparentIndex;

            int resultIndex = 0;

            // blending the color with background and checking if there is an exact match now
            // NOTE: doing this as Color32 rather than ColorF on purpose so there will be fewer colors in the cache
            if (color.A != Byte.MaxValue)
            {
                color = color.BlendWithBackgroundLinear(BackColor);
                if (color32ToIndex.TryGetValue(color, out resultIndex))
                    return resultIndex;
            }

            float minDiff = Single.MaxValue;

            ColorF colorF = color.ToColorF();
            ColorF? backColor = null;
            entriesF ??= Entries.Select(c => c.ToColorF()).ToArray();

            // The two similar lookups could be merged but it is faster to separate them even if some parts are duplicated
            int len = Entries.Length;
            if (IsGrayscale)
            {
                float brightness = colorF.GetBrightness();
                for (int i = 0; i < len; i++)
                {
                    ColorF current = entriesF[i];

                    // Palette color with alpha
                    if (current.A < 1f)
                    {
                        // Skipping fully transparent palette colors because they were handled above
                        if (current.A == 0f)
                            continue;

                        // Blending also the current palette color
                        current = current.BlendWithBackground(backColor ??= BackColor.ToColorF());

                        // Exact match. Since the cache was checked before calling this method this can occur only after alpha blending.
                        if (current == colorF)
                            return i;
                    }

                    // If the palette is grayscale, then distance is measured by linear brightness;
                    // otherwise, by an Euclidean-like but much faster distance based on RGB components.
                    float diff = Math.Abs(entriesF[i].GetBrightness() - brightness);

                    if (diff >= minDiff)
                        continue;

                    // new closest match
                    if (diff == 0f)
                        return i;
                    minDiff = diff;
                    resultIndex = i;
                }
            }
            else
            {
                for (int i = 0; i < len; i++)
                {
                    ColorF current = entriesF[i];

                    // Palette color with alpha
                    if (current.A < 1f)
                    {
                        // Skipping fully transparent palette colors because they were handled above
                        if (current.A == 0f)
                            continue;

                        // Blending also the current palette color
                        current = current.BlendWithBackground(backColor ??= BackColor.ToColorF());

                        // Exact match. Since the cache was checked before calling this method this can occur only after alpha blending.
                        if (current == colorF)
                            return i;
                    }

                    // If the palette is grayscale, then distance is measured by linear brightness;
                    // otherwise, by an Euclidean-like but much faster distance based on RGB components.
                    float diff = Math.Abs(current.R - colorF.R) + Math.Abs(current.G - colorF.G) + Math.Abs(current.B - colorF.B);

                    Debug.Assert(diff != 0, "Exact match should have been returned earlier");

                    // new closest match
                    if (diff < minDiff)
                    {
                        minDiff = diff;
                        resultIndex = i;
                    }
                }
            }

            return resultIndex;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void ThrowIndexInvalid(int index) => throw new ArgumentOutOfRangeException(nameof(index), Res.ImagingInvalidPaletteIndex(index, Entries.Length));

        #endregion

        #endregion

        #endregion
    }
}
