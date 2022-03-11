#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: Palette.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2021 - All Rights Reserved
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
    /// <br/>See the <strong>Remarks</strong> section for details.
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
    /// is 256. Generally, the more color the palette has the slower are the lookups for non-palette colors that are not cached yet.</para>
    /// </remarks>
    /// <threadsafety instance="false">If there is no custom lookup logic passed to the constructors, then members of this type are guaranteed to be safe for multi-threaded operations.
    /// If this type is initialized with a custom lookup logic, then thread-safety depends on the custom lookup implementation.</threadsafety>
    [DebuggerDisplay("Count = {" + nameof(Count) + "}")]
    public sealed class Palette
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
        private readonly Func<Color32, int>? customGetNearestColorIndex;

        private IThreadSafeCacheAccessor<Color32, int>? cache;

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
        internal bool HasAlpha { get; }
        internal bool HasMultiLevelAlpha { get; }
        internal int TransparentIndex { get; }
        internal bool HasTransparent => TransparentIndex >= 0;

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
        /// <exception cref="ArgumentException"><paramref name="entries"/> must not be empty.</exception>
        public Palette(Color32[] entries, Color32 backColor = default, byte alphaThreshold = 128, Func<Color32, int>? customGetNearestColorIndex = null)
        {
            Entries = entries ?? throw new ArgumentNullException(nameof(entries), PublicResources.ArgumentNull);
            if (entries.Length == 0)
                throw new ArgumentException(PublicResources.ArgumentEmpty, nameof(entries));

            TransparentIndex = -1;
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
        /// <exception cref="ArgumentException"><paramref name="entries"/> must not be empty.</exception>
        public Palette(Color[] entries, Color backColor = default, byte alphaThreshold = 128, Func<Color32, int>? customGetNearestColorIndex = null)
            // ReSharper disable once ConstantConditionalAccessQualifier - needed to avoid NullReferenceException so the ArgumentNullException can be thrown by the other overload
            : this(entries?.Select(c => new Color32(c)).ToArray()!, new Color32(backColor), alphaThreshold, customGetNearestColorIndex)
        {
        }

        #endregion

        #region Internal Constructors

        internal Palette(KnownPixelFormat pixelFormat, Color32 backColor, byte alphaThreshold)
            : this(GetColorsByPixelFormat(pixelFormat), backColor, alphaThreshold)
        {
        }

        internal Palette(Palette palette, Color32 backColor, byte alphaThreshold)
            : this(palette.Entries, backColor, alphaThreshold, palette.customGetNearestColorIndex)
        {
        }

        #endregion

        #endregion

        #region Methods

        #region Static Methods

        #region Public Methods

        /// <summary>
        /// Gets a <see cref="Palette"/> instance that uses the system default 8-bit palette.
        /// On Windows this palette contains the 16 standard <a href="https://www.w3.org/TR/REC-html40/types.html#h-6.5" target="_blank">basic sRGB colors</a>,
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
            => new Palette(System8BppPalette, backColor, alphaThreshold);

        /// <summary>
        /// Gets a <see cref="Palette"/> instance that uses the system default 4-bit palette.
        /// On Windows this palette consists of the 16 standard <a href="https://www.w3.org/TR/REC-html40/types.html#h-6.5" target="_blank">basic sRGB colors</a>.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="PredefinedColorsQuantizer.SystemDefault4BppPalette">PredefinedColorsQuantizer.SystemDefault4BppPalette</see> method for details and some examples.
        /// </summary>
        /// <param name="backColor">Specifies the background color for lookup operations (<see cref="GetNearestColor">GetNearestColor</see>, <see cref="GetNearestColorIndex">GetNearestColorIndex</see>).
        /// When a lookup is performed with a color with transparency, then the color to be found will be blended with this color before performing the lookup.
        /// The <see cref="Color32.A">Color32.A</see> field of the background color is ignored. This parameter is optional.
        /// <br/>Default value: The default value of the <see cref="Color32"/> type, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <returns>A <see cref="Palette"/> instance that uses the system default 4-bit palette.</returns>
        /// <seealso cref="PredefinedColorsQuantizer.SystemDefault4BppPalette"/>
        public static Palette SystemDefault4BppPalette(Color32 backColor = default)
            => new Palette(System4BppPalette, backColor);

        /// <summary>
        /// Gets a <see cref="Palette"/> instance that uses the system default 1-bit palette.
        /// On Windows this palette consists of the black and white colors.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="PredefinedColorsQuantizer.SystemDefault1BppPalette">PredefinedColorsQuantizer.SystemDefault1BppPalette</see> method for details.
        /// </summary>
        /// <param name="backColor">Specifies the background color for lookup operations (<see cref="GetNearestColor">GetNearestColor</see>, <see cref="GetNearestColorIndex">GetNearestColorIndex</see>).
        /// When a lookup is performed with a color with transparency, then the color to be found will be blended with this color before performing the lookup.
        /// The <see cref="Color32.A">Color32.A</see> field of the background color is ignored. This parameter is optional.
        /// <br/>Default value: The default value of the <see cref="Color32"/> type, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <returns>A <see cref="Palette"/> instance that uses the system default 1-bit palette.</returns>
        /// <seealso cref="PredefinedColorsQuantizer.SystemDefault1BppPalette"/>
        public static Palette SystemDefault1BppPalette(Color32 backColor = default)
            => new Palette(BlackAndWhitePalette, backColor);

        /// <summary>
        /// Gets a <see cref="Palette"/> instance that uses a 8-bit palette where red, green and blue components are encoded in 3, 3 and 2 bits, respectively.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="PredefinedColorsQuantizer.Rgb332">PredefinedColorsQuantizer.Rgb332</see> method for details and some examples.
        /// </summary>
        /// <param name="backColor">Specifies the background color for lookup operations (<see cref="GetNearestColor">GetNearestColor</see>, <see cref="GetNearestColorIndex">GetNearestColorIndex</see>).
        /// When a lookup is performed with a color with transparency, then the color to be found will be blended with this color before performing the lookup.
        /// The <see cref="Color32.A">Color32.A</see> field of the background color is ignored. This parameter is optional.
        /// <br/>Default value: The default value of the <see cref="Color32"/> type, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="directMapping"><see langword="true"/>&#160;to map any color directly to an index instead of searching for a nearest color,
        /// which is very fast but without dithering may end up in a noticeably poorer result and higher contrast;
        /// <see langword="false"/>&#160;to perform a lookup to determine nearest colors, which may be slower but more accurate. This parameter is optional.
        /// <br/>Default value: <see langword="false"/>.</param>
        /// <returns>A <see cref="Palette"/> instance that uses a 8-bit palette where red, green and blue components are encoded in 3, 3 and 2 bits, respectively.</returns>
        /// <seealso cref="PredefinedColorsQuantizer.Rgb332"/>
        public static Palette Rgb332(Color32 backColor = default, bool directMapping = false)
        {
            int GetNearestColorIndex(Color32 c)
            {
                if (c.A < Byte.MaxValue)
                    c = c.BlendWithBackground(backColor);

                return (c.R & 0b11100000) | ((c.G & 0b11100000) >> 3) | ((c.B & 0b11000000) >> 6);
            }

            return new Palette(Rgb332Palette, backColor, 0, directMapping ? GetNearestColorIndex : default);
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
        public static Palette Grayscale256(Color32 backColor = default)
        {
            int GetNearestColorIndex(Color32 c)
            {
                if (c.A < Byte.MaxValue)
                    c = c.BlendWithBackground(backColor);
                return c.GetBrightness();
            }

            return new Palette(Grayscale256Palette, backColor, 0, GetNearestColorIndex);
        }

        /// <summary>
        /// Gets a <see cref="Palette"/> instance that uses a 4-bit grayscale palette of 16 shades.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="PredefinedColorsQuantizer.Grayscale16">PredefinedColorsQuantizer.Grayscale16</see> method for details and some examples.
        /// </summary>
        /// <param name="backColor">Specifies the background color for lookup operations (<see cref="GetNearestColor">GetNearestColor</see>, <see cref="GetNearestColorIndex">GetNearestColorIndex</see>).
        /// When a lookup is performed with a color with transparency, then the color to be found will be blended with this color before performing the lookup.
        /// The <see cref="Color32.A">Color32.A</see> field of the background color is ignored. This parameter is optional.
        /// <br/>Default value: The default value of the <see cref="Color32"/> type, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="directMapping"><see langword="true"/>&#160;to map any color directly to an index instead of searching for a nearest color,
        /// which is very fast but may end up in a result of a bit higher contrast than the original image;
        /// <see langword="false"/>&#160;to perform a lookup to determine nearest colors, which may be slower but more accurate. This parameter is optional.
        /// <br/>Default value: <see langword="false"/>.</param>
        /// <returns>A <see cref="Palette"/> instance that uses a 4-bit grayscale palette of 16 shades.</returns>
        /// <seealso cref="PredefinedColorsQuantizer.Grayscale16"/>
        public static Palette Grayscale16(Color32 backColor = default, bool directMapping = false)
        {
            int GetNearestColorIndex(Color32 c)
            {
                if (c.A < Byte.MaxValue)
                    c = c.BlendWithBackground(backColor);
                return c.GetBrightness() >> 4;
            }

            return new Palette(Grayscale16Palette, backColor, 0, directMapping ? GetNearestColorIndex : default);
        }

        /// <summary>
        /// Gets a <see cref="Palette"/> instance that uses a grayscale palette of 4 shades.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="PredefinedColorsQuantizer.Grayscale4">PredefinedColorsQuantizer.Grayscale4</see> method for details and some examples.
        /// </summary>
        /// <param name="backColor">Specifies the background color for lookup operations (<see cref="GetNearestColor">GetNearestColor</see>, <see cref="GetNearestColorIndex">GetNearestColorIndex</see>).
        /// When a lookup is performed with a color with transparency, then the color to be found will be blended with this color before performing the lookup.
        /// The <see cref="Color32.A">Color32.A</see> field of the background color is ignored. This parameter is optional.
        /// <br/>Default value: The default value of the <see cref="Color32"/> type, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="directMapping"><see langword="true"/>&#160;to map any color directly to an index instead of searching for a nearest color,
        /// which is very fast but may end up in a result of a bit higher contrast than the original image;
        /// <see langword="false"/>&#160;to perform a lookup to determine nearest colors, which may be slower but more accurate. This parameter is optional.
        /// <br/>Default value: <see langword="false"/>.</param>
        /// <returns>A <see cref="Palette"/> instance that uses a grayscale palette of 4 shades.</returns>
        /// <seealso cref="PredefinedColorsQuantizer.Grayscale4"/>
        public static Palette Grayscale4(Color32 backColor = default, bool directMapping = false)
        {
            int GetNearestColorIndex(Color32 c)
            {
                if (c.A < Byte.MaxValue)
                    c = c.BlendWithBackground(backColor);
                return c.GetBrightness() >> 6;
            }

            return new Palette(Grayscale4Palette, backColor, 0, directMapping ? GetNearestColorIndex : default);
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
        public static Palette BlackAndWhite(Color32 backColor = default, byte whiteThreshold = 128)
        {
            int GetNearestColorIndex(Color32 c)
            {
                if (c.A < Byte.MaxValue)
                    c = c.BlendWithBackground(backColor);

                return c == Color32.Black ? 0
                    : c == Color32.White ? 1
                    : c.GetBrightness() >= whiteThreshold ? 1 : 0;
            }

            return new Palette(BlackAndWhitePalette, backColor, 0, GetNearestColorIndex);
        }

        #endregion

        #region Private Methods

        private static Color32[] GetColorsByPixelFormat(KnownPixelFormat pixelFormat) => pixelFormat switch
        {
            KnownPixelFormat.Format8bppIndexed => System8BppPalette,
            KnownPixelFormat.Format4bppIndexed => System4BppPalette,
            KnownPixelFormat.Format1bppIndexed => BlackAndWhitePalette,
            _ => throw new ArgumentOutOfRangeException(nameof(pixelFormat), PublicResources.ArgumentOutOfRange)
        };

        #endregion

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
            // exact match: from the palette
            if (color32ToIndex.TryGetValue(c, out int result))
                return result;

            // We have a custom logic: we do not cache the results of the extern logic
            if (customGetNearestColorIndex != null)
                return customGetNearestColorIndex.Invoke(c);

            // from the lock-free cache
            if (cache == null)
                Interlocked.CompareExchange(ref cache, ThreadSafeCacheFactory.Create<Color32, int>(FindNearestColorIndex, cacheOptions), null);
            return cache[c];
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

        internal bool Equals(Palette? other)
        {
            // not a public method because we don't want to adjust GetHashCode to these comparisons
            if (other == null || customGetNearestColorIndex != other.customGetNearestColorIndex || !BackColor.Equals(other.BackColor) || AlphaThreshold != other.AlphaThreshold)
                return false;

            if (ReferenceEquals(other.Entries, Entries))
                return true;

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

        private int FindNearestColorIndex(Color32 color)
        {
            // mapping alpha to full transparency
            if (color.A < AlphaThreshold && TransparentIndex != -1)
                return TransparentIndex;

            int minDiff = Int32.MaxValue;
            int resultIndex = 0;

            // blending the color with background and checking if there is an exact match now
            if (color.A != Byte.MaxValue)
            {
                color = color.BlendWithBackground(BackColor);
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
                        current = current.BlendWithBackground(BackColor);

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
                        current = current.BlendWithBackground(BackColor);

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

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void ThrowIndexInvalid(int index) => throw new ArgumentOutOfRangeException(nameof(index), Res.ImagingInvalidPaletteIndex(index, Entries.Length));

        #endregion

        #endregion

        #endregion
    }
}
