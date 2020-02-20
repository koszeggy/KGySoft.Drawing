#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: PredefinedColorsQuantizer.cs
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
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

#endregion

namespace KGySoft.Drawing.Imaging
{
    /// <summary>
    /// Represents a quantizer with predefined set of colors. Use the static members to retrieve an instance.
    /// <br/>See the <strong>Examples</strong> section of each static member of this class to see some image examples.
    /// <br/>For using optimized colors for a specific source image see also the <see cref="OptimizedPaletteQuantizer"/> class.
    /// </summary>
    /// <seealso cref="IQuantizer" />
    /// <seealso cref="OptimizedPaletteQuantizer"/>
    /// <seealso cref="ImageExtensions.ConvertPixelFormat(Image, PixelFormat, IQuantizer, IDitherer)"/>
    /// <seealso cref="BitmapExtensions.Quantize"/>
    public sealed class PredefinedColorsQuantizer : IQuantizer
    {
        #region Nested classes

        #region QuantizingSessionCustomMapping class

        private sealed class QuantizingSessionCustomMapping : IQuantizingSession
        {
            #region Fields

            private readonly PredefinedColorsQuantizer quantizer;
            private readonly Func<Color32, Color32> quantizingFunction;

            #endregion

            #region Properties

            public Palette Palette => null;
            public Color32 BackColor => quantizer.backColor;
            public byte AlphaThreshold => quantizer.alphaThreshold;

            #endregion

            #region Constructors

            internal QuantizingSessionCustomMapping(PredefinedColorsQuantizer quantizer, Func<Color32, Color32> quantizingFunction)
            {
                this.quantizer = quantizer;
                this.quantizingFunction = quantizingFunction;
            }

            #endregion

            #region Methods

            public void Dispose()
            {
            }

            public Color32 GetQuantizedColor(Color32 c)
                => c.A == Byte.MaxValue || !quantizer.blendAlphaBeforeQuantize
                    ? quantizingFunction.Invoke(c)
                    : c.A < AlphaThreshold
                        ? default
                        : quantizingFunction.Invoke(c.BlendWithBackground(BackColor));

            #endregion
        }

        #endregion

        #region QuantizingSessionIndexed class

        private sealed class QuantizingSessionIndexed : IQuantizingSession
        {
            #region Fields

            private readonly PredefinedColorsQuantizer quantizer;

            #endregion

            #region Properties

            public Palette Palette { get; }
            public Color32 BackColor => quantizer.backColor;
            public byte AlphaThreshold => quantizer.alphaThreshold;

            #endregion

            #region Constructors

            internal QuantizingSessionIndexed(PredefinedColorsQuantizer quantizer, Palette palette)
            {
                this.quantizer = quantizer;
                Palette = palette;
            }

            #endregion

            #region Methods

            public void Dispose()
            {
            }

            public Color32 GetQuantizedColor(Color32 c) => Palette.GetNearestColor(c);

            #endregion
        }

        #endregion

        #endregion

        #region Fields

        #region Static Fields

        private static Color32[] rgb332Palette;
        private static Color32[] grayscale256Palette;
        private static Color32[] grayscale16Palette;
        private static Color32[] grayscale4Palette;
        private static Color32[] blackAndWhitePalette;
        private static Color32[] system8BppPalette;
        private static Color32[] system4BppPalette;
        private static Color32[] system1BppPalette;

        #endregion

        #region Instance Fields

        private readonly Func<Color32, Color32> quantizingFunction;
        private readonly Color32 backColor;
        private readonly byte alphaThreshold;
        private readonly bool blendAlphaBeforeQuantize;
        private readonly Palette palette;

        #endregion

        #endregion

        #region Properties

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

        private static Color32[] System8BppPalette
        {
            get
            {
                if (system8BppPalette != null)
                    return system8BppPalette;

                using (var bmp = new Bitmap(1, 1, PixelFormat.Format8bppIndexed))
                    return system8BppPalette = bmp.Palette.Entries.Select(c => new Color32(c)).ToArray();
            }
        }

        private static Color32[] System4BppPalette
        {
            get
            {
                if (system4BppPalette != null)
                    return system4BppPalette;

                using (var bmp = new Bitmap(1, 1, PixelFormat.Format4bppIndexed))
                    return system4BppPalette = bmp.Palette.Entries.Select(c => new Color32(c)).ToArray();
            }
        }

        private static Color32[] System1BppPalette
        {
            get
            {
                if (system1BppPalette != null)
                    return system1BppPalette;

                using (var bmp = new Bitmap(1, 1, PixelFormat.Format1bppIndexed))
                    return system1BppPalette = bmp.Palette.Entries.Select(c => new Color32(c)).ToArray();
            }
        }

        private static Color32[] BlackAndWhitePalette => blackAndWhitePalette ??= new[] { Color32.Black, Color32.White };

        #endregion

        #region Constructors

        private PredefinedColorsQuantizer(Palette palette)
        {
            this.palette = palette ?? throw new ArgumentNullException(nameof(palette), PublicResources.ArgumentNull);
            backColor = palette.BackColor;
            alphaThreshold = palette.AlphaThreshold;
        }

        private PredefinedColorsQuantizer(Color[] colors, Color backColor, byte alphaThreshold = 0)
            : this(new Palette(colors, backColor, alphaThreshold))
        {
        }

        private PredefinedColorsQuantizer(Color32[] colors, Color32 backColor, byte alphaThreshold = 0)
            : this(new Palette(colors, backColor, alphaThreshold))
        {
        }

        private PredefinedColorsQuantizer(Func<Color32, Color32> quantizingFunction, Color32 backColor, byte alphaThreshold = 0)
            : this(quantizingFunction)
        {
            this.backColor = Color32.FromArgb(Byte.MaxValue, backColor);
            this.alphaThreshold = alphaThreshold;
            blendAlphaBeforeQuantize = true;
        }

        private PredefinedColorsQuantizer(Func<Color32, Color32> quantizingFunction)
            => this.quantizingFunction = quantizingFunction ?? throw new ArgumentNullException(nameof(quantizingFunction), PublicResources.ArgumentNull);

        #endregion

        #region Methods

        #region Static Methods

        #region Public Methods

        /// <summary>
        /// Gets a <see cref="PredefinedColorsQuantizer"/> instance that quantizes colors to 24-bit ones where each color component is encoded in 8 bits.
        /// <br/>See the <strong>Remarks</strong> section for details and some examples.
        /// </summary>
        /// <param name="backColor">Colors with alpha (transparency) will be blended with this color before quantization.
        /// The <see cref="Color.A"/> property of the background color is ignored. This parameter is optional.
        /// <br/>Default value: <see cref="Color.Empty"/>, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <returns>A <see cref="PredefinedColorsQuantizer"/> instance that quantizes colors to 24-bit ones where each color component is encoded in 8 bits.</returns>
        /// <remarks>
        /// <para>The returned <see cref="PredefinedColorsQuantizer"/> instance can return up to 256<sup>3</sup> (16,777,216) colors.
        /// It practically just removes transparency and does not change colors without alpha.</para>
        /// <para>This quantizer fits well for <see cref="Bitmap"/>s with <see cref="PixelFormat.Format24bppRgb"/> pixel format.</para>
        /// <note>For more information about the possible usable <see cref="PixelFormat"/>s on different platforms see the <strong>Remarks</strong> section of the <see cref="ImageExtensions.ConvertPixelFormat(Image,PixelFormat,Color,byte)">ConvertPixelFormat</see> extension method.</note>
        /// </remarks>
        /// <example>
        /// The following example demonstrates how to use the quantizer returned by this method:
        /// <code lang="C#"><![CDATA[
        /// public static Bitmap ToRgb888(Bitmap source, Color backColor = default)
        /// {
        ///     IQuantizer quantizer = PredefinedColorsQuantizer.Rgb888(backColor);
        ///
        ///     // a.) this solution returns a new bitmap and does not change the original one:
        ///     return source.ConvertPixelFormat(PixelFormat.Format24bppRgb, quantizer);
        ///
        ///     // b.) when converting to Format24bppRgb format, this produces the same result:
        ///     return source.ConvertPixelFormat(PixelFormat.Format24bppRgb, backColor);
        ///
        ///     // c.) alternatively, you can perform the quantization directly on the source bitmap:
        ///     source.Quantize(quantizer);
        ///     return source;
        /// }]]></code>
        /// <para>The example above may produce the following results:
        /// <list type="table">
        /// <listheader><term>Original image</term><term>Quantized image</term></listheader>
        /// <item>
        /// <term><div style="text-align:center;width:512px">
        /// <para><img src="../Help/Images/AlphaGradient.png" alt="Color hues with alpha gradient"/>
        /// <br/>Color hues with alpha gradient</para></div></term>
        /// <term>
        /// <div style="text-align:center;width:512px">
        /// <para><img src="../Help/Images/AlphaGradientRgb888Black.png" alt="Color hues with black background"/>
        /// <br/>Default (black) background</para>
        /// <para><img src="../Help/Images/AlphaGradientRgb888Silver.png" alt="Color hues with silver background"/>
        /// <br/>Silver background</para></div></term>
        /// </item>
        /// <item>
        /// <term><div style="text-align:center;width:512px">
        /// <para><img src="../Help/Images/Shield256.png" alt="Shield icon with transparent background"/>
        /// <br/>Shield icon with transparency</para></div></term>
        /// <term><div style="text-align:center;width:512px">
        /// <para><img src="../Help/Images/ShieldRgb888Black.png" alt="Shield icon with black background"/>
        /// <br/>Default (black) background</para>
        /// <para><img src="../Help/Images/ShieldRgb888Silver.png" alt="Shield icon with silver background"/>
        /// <br/>Silver background</para></div></term>
        /// </item>
        /// </list></para>
        /// </example>
        public static PredefinedColorsQuantizer Rgb888(Color backColor = default)
        {
            // just returning the already blended color
            static Color32 Quantize(Color32 c) => c;

            return new PredefinedColorsQuantizer(Quantize, new Color32(backColor));
        }

        /// <summary>
        /// Gets a <see cref="PredefinedColorsQuantizer"/> instance that quantizes colors to 16-bit ones where red,
        /// green and blue components are encoded in 5, 6 and 5 bits, respectively.
        /// </summary>
        /// <param name="backColor">Colors with alpha (transparency) will be blended with this color before quantization.
        /// The <see cref="Color.A"/> property of the background color is ignored. This parameter is optional.
        /// <br/>Default value: <see cref="Color.Empty"/>, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <returns>A <see cref="PredefinedColorsQuantizer"/> instance that quantizes colors to 16-bit ones where red,
        /// green and blue components are encoded in 5, 6 and 5 bits, respectively.</returns>
        /// <remarks>
        /// <para>The returned <see cref="PredefinedColorsQuantizer"/> instance can return up to 65,536 colors.</para>
        /// <para>This quantizer fits well for <see cref="Bitmap"/>s with <see cref="PixelFormat.Format16bppRgb565"/> pixel format.</para>
        /// <note>For more information about the possible usable <see cref="PixelFormat"/>s on different platforms see the <strong>Remarks</strong> section of the <see cref="ImageExtensions.ConvertPixelFormat(Image,PixelFormat,Color,byte)">ConvertPixelFormat</see> extension method.</note>
        /// </remarks>
        /// <example>
        /// The following example demonstrates how to use the quantizer returned by this method:
        /// <code lang="C#"><![CDATA[
        /// public static Bitmap ToRgb565(Bitmap source, Color backColor = default, IDitherer ditherer = null)
        /// {
        ///     IQuantizer quantizer = PredefinedColorsQuantizer.Rgb565(backColor);
        ///
        ///     // a.) this solution returns a new bitmap and does not change the original one:
        ///     return source.ConvertPixelFormat(PixelFormat.Format16bppRgb565, quantizer, ditherer);
        ///
        ///     // b.) when converting to Format16bppRgb565 format without dithering, this produces the same result:
        ///     if (ditherer == null)
        ///         return source.ConvertPixelFormat(PixelFormat.Format16bppRgb565, backColor);
        ///
        ///     // c.) alternatively, you can perform the quantization directly on the source bitmap:
        ///     if (ditherer == null)
        ///         source.Quantize(quantizer);
        ///     else
        ///         source.Dither(quantizer, ditherer);
        ///     return source;
        /// }]]></code>
        /// <para>The example above may produce the following results:
        /// <list type="table">
        /// <listheader><term>Original image</term><term>Quantized image</term></listheader>
        /// <item>
        /// <term><div style="text-align:center;width:512px">
        /// <para><img src="../Help/Images/AlphaGradient.png" alt="Color hues with alpha gradient"/>
        /// <br/>Color hues with alpha gradient</para></div></term>
        /// <term>
        /// <div style="text-align:center;width:512px">
        /// <para><img src="../Help/Images/AlphaGradientRgb565Black.png" alt="Color hues with RGB565 pixel format and black background"/>
        /// <br/>Default optional parameter values (black background)</para>
        /// <para><img src="../Help/Images/AlphaGradientRgb565Silver.png" alt="Color hues with RGB565 pixel format and silver background"/>
        /// <br/>Silver background</para>
        /// <para><img src="../Help/Images/AlphaGradientRgb565SilverDithered.png" alt="Color hues with RGB565 pixel format, silver background and Bayer 8x8 ordered dithering"/>
        /// <br/>Silver background, <see cref="OrderedDitherer.Bayer8x8">Bayer 8x8</see> dithering</para></div></term>
        /// </item>
        /// <item>
        /// <term><div style="text-align:center;width:512px">
        /// <para><img src="../Help/Images/Shield256.png" alt="Shield icon with transparent background"/>
        /// <br/>Shield icon with transparency</para></div></term>
        /// <term><div style="text-align:center;width:512px">
        /// <para><img src="../Help/Images/ShieldRgb565Black.png" alt="Shield icon with RGB565 pixel format and black background"/>
        /// <br/>Default optional parameter values (black background)</para>
        /// <para><img src="../Help/Images/ShieldRgb565Silver.png" alt="Shield icon with RGB565 pixel format and silver background"/>
        /// <br/>Silver background</para>
        /// <para><img src="../Help/Images/ShieldRgb565SilverDithered.png" alt="Shield icon with RGB565 pixel format, silver background and Floyd-Steinberg dithering"/>
        /// <br/>Silver background, <see cref="ErrorDiffusionDitherer.FloydSteinberg">Floyd-Steinberg</see> dithering</para></div></term>
        /// </item>
        /// </list></para>
        /// </example>
        public static PredefinedColorsQuantizer Rgb565(Color backColor = default)
        {
            static Color32 Quantize(Color32 c) => new Color16Rgb565(c).ToColor32();

            return new PredefinedColorsQuantizer(Quantize, new Color32(backColor));
        }

        /// <summary>
        /// Gets a <see cref="PredefinedColorsQuantizer"/> instance that quantizes colors to 16-bit ones where each color component is encoded in 5 bits.
        /// </summary>
        /// <param name="backColor">Colors with alpha (transparency) will be blended with this color before quantization.
        /// The <see cref="Color.A"/> property of the background color is ignored. This parameter is optional.
        /// <br/>Default value: <see cref="Color.Empty"/>, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <returns>A <see cref="PredefinedColorsQuantizer"/> instance that quantizes colors to 16-bit ones where each color component is encoded in 5 bits.</returns>
        /// <remarks>
        /// <para>The returned <see cref="PredefinedColorsQuantizer"/> instance can return up to 32,768 colors.</para>
        /// <para>This quantizer fits well for <see cref="Bitmap"/>s with <see cref="PixelFormat.Format16bppRgb555"/> pixel format.</para>
        /// <note>For more information about the possible usable <see cref="PixelFormat"/>s on different platforms see the <strong>Remarks</strong> section of the <see cref="ImageExtensions.ConvertPixelFormat(Image,PixelFormat,Color,byte)">ConvertPixelFormat</see> extension method.</note>
        /// </remarks>
        /// <example>
        /// The following example demonstrates how to use the quantizer returned by this method:
        /// <code lang="C#"><![CDATA[
        /// public static Bitmap ToRgb555(Bitmap source, Color backColor = default, IDitherer ditherer = null)
        /// {
        ///     IQuantizer quantizer = PredefinedColorsQuantizer.Rgb555(backColor);
        ///
        ///     // a.) this solution returns a new bitmap and does not change the original one:
        ///     return source.ConvertPixelFormat(PixelFormat.Format16bppRgb555, quantizer, ditherer);
        ///
        ///     // b.) when converting to Format16bppRgb555 format without dithering, this produces the same result:
        ///     if (ditherer == null)
        ///         return source.ConvertPixelFormat(PixelFormat.Format16bppRgb555, backColor);
        ///
        ///     // c.) alternatively, you can perform the quantization directly on the source bitmap:
        ///     if (ditherer == null)
        ///         source.Quantize(quantizer);
        ///     else
        ///         source.Dither(quantizer, ditherer);
        ///     return source;
        /// }]]></code>
        /// <para>The example above may produce the following results:
        /// <list type="table">
        /// <listheader><term>Original image</term><term>Quantized image</term></listheader>
        /// <item>
        /// <term><div style="text-align:center;width:512px">
        /// <para><img src="../Help/Images/AlphaGradient.png" alt="Color hues with alpha gradient"/>
        /// <br/>Color hues with alpha gradient</para></div></term>
        /// <term>
        /// <div style="text-align:center;width:512px">
        /// <para><img src="../Help/Images/AlphaGradientRgb555Black.png" alt="Color hues with RGB555 pixel format and black background"/>
        /// <br/>Default optional parameter values (black background)</para>
        /// <para><img src="../Help/Images/AlphaGradientRgb555Silver.png" alt="Color hues with RGB555 pixel format and silver background"/>
        /// <br/>Silver background</para>
        /// <para><img src="../Help/Images/AlphaGradientRgb555SilverDithered.png" alt="Color hues with RGB555 pixel format, silver background and Bayer 8x8 ordered dithering"/>
        /// <br/>Silver background, <see cref="OrderedDitherer.Bayer8x8">Bayer 8x8</see> dithering</para></div></term>
        /// </item>
        /// <item>
        /// <term><div style="text-align:center;width:512px">
        /// <para><img src="../Help/Images/Shield256.png" alt="Shield icon with transparent background"/>
        /// <br/>Shield icon with transparency</para></div></term>
        /// <term><div style="text-align:center;width:512px">
        /// <para><img src="../Help/Images/ShieldRgb555Black.png" alt="Shield icon with RGB555 pixel format and black background"/>
        /// <br/>Default optional parameter values (black background)</para>
        /// <para><img src="../Help/Images/ShieldRgb555Silver.png" alt="Shield icon with RGB555 pixel format and silver background"/>
        /// <br/>Silver background</para>
        /// <para><img src="../Help/Images/ShieldRgb555SilverDithered.png" alt="Shield icon with RGB555 pixel format, silver background and Floyd-Steinberg dithering"/>
        /// <br/>Silver background, <see cref="ErrorDiffusionDitherer.FloydSteinberg">Floyd-Steinberg</see> dithering</para></div></term>
        /// </item>
        /// </list></para>
        /// </example>
        public static PredefinedColorsQuantizer Rgb555(Color backColor = default)
        {
            static Color32 Quantize(Color32 c) => new Color16Rgb555(c).ToColor32();

            return new PredefinedColorsQuantizer(Quantize, new Color32(backColor));
        }

        /// <summary>
        /// Gets a <see cref="PredefinedColorsQuantizer"/> instance that quantizes colors to 16-bit ones where alpha, red,
        /// green and blue components are encoded in 1, 5, 5 and 5 bits, respectively.
        /// </summary>
        /// <param name="backColor">Colors with alpha (transparency), whose <see cref="Color.A">Color.A</see> property
        /// is equal to or greater than <paramref name="alphaThreshold"/> will be blended with this color before quantization.
        /// The <see cref="Color.A"/> property of the background color is ignored. This parameter is optional.
        /// <br/>Default value: <see cref="Color.Empty"/>, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="alphaThreshold">Specifies a threshold value for the <see cref="Color.A">Color.A</see> property, under which a quantized color is considered transparent.
        /// If <c>0</c>, then the quantized colors will never be transparent. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <returns>A <see cref="PredefinedColorsQuantizer"/> instance that quantizes colors to 16-bit ones where each color component is encoded in 5 bits.</returns>
        /// <remarks>
        /// <para>The returned <see cref="PredefinedColorsQuantizer"/> instance can return up to 32,768 colors, and a transparent color.</para>
        /// <para>This quantizer fits well for <see cref="Bitmap"/>s with <see cref="PixelFormat.Format16bppArgb1555"/> pixel format.</para>
        /// <note>For more information about the possible usable <see cref="PixelFormat"/>s on different platforms see the <strong>Remarks</strong> section of the <see cref="ImageExtensions.ConvertPixelFormat(Image,PixelFormat,Color,byte)">ConvertPixelFormat</see> extension method.</note>
        /// </remarks>
        /// <example>
        /// The following example demonstrates how to use the quantizer returned by this method:
        /// <code lang="C#"><![CDATA[
        /// public static Bitmap ToArgb1555(Bitmap source, Color backColor = default,
        ///     byte alphaThreshold = 128, IDitherer ditherer = null)
        /// {
        ///     IQuantizer quantizer = PredefinedColorsQuantizer.Argb1555(backColor, alphaThreshold);
        ///
        ///     // a.) this solution returns a new bitmap and does not change the original one:
        ///     return source.ConvertPixelFormat(PixelFormat.Format16bppArgb1555, quantizer, ditherer);
        ///
        ///     // b.) when converting to Format16bppArgb1555 format without dithering, this produces the same result:
        ///     if (ditherer == null)
        ///         return source.ConvertPixelFormat(PixelFormat.Format16bppArgb1555, backColor, alphaThreshold);
        ///
        ///     // c.) alternatively, you can perform the quantization directly on the source bitmap:
        ///     if (ditherer == null)
        ///         source.Quantize(quantizer);
        ///     else
        ///         source.Dither(quantizer, ditherer);
        ///     return source;
        /// }]]></code>
        /// <para>The example above may produce the following results:
        /// <list type="table">
        /// <listheader><term>Original image</term><term>Quantized image</term></listheader>
        /// <item>
        /// <term><div style="text-align:center;width:512px">
        /// <para><img src="../Help/Images/AlphaGradient.png" alt="Color hues with alpha gradient"/>
        /// <br/>Color hues with alpha gradient</para></div></term>
        /// <term>
        /// <div style="text-align:center;width:512px">
        /// <para><img src="../Help/Images/AlphaGradientArgb1555SilverBlackA128.png" alt="Color hues with ARGB1555 pixel format, black background and default alpha threshold"/>
        /// <br/>Default optional parameter values (black background, alpha threshold = 128). The bottom half of the image is transparent.</para>
        /// <para><img src="../Help/Images/AlphaGradientArgb1555SilverSilverA1.png" alt="Color hues with ARGB1555 pixel format, silver background and alpha threshold = 1"/>
        /// <br/>Silver background, alpha threshold = 1. Only the bottom line is transparent.</para>
        /// <para><img src="../Help/Images/AlphaGradientArgb1555SilverSilverDithered.png" alt="Color hues with ARGB1555 pixel format, silver background, default alpha threshold and Bayer 8x8 ordered dithering"/>
        /// <br/>Silver background, default alpha threshold, <see cref="OrderedDitherer.Bayer8x8">Bayer 8x8</see> dithering. The bottom half of the image is transparent.</para></div></term>
        /// </item>
        /// <item>
        /// <term><div style="text-align:center;width:512px">
        /// <para><img src="../Help/Images/Shield256.png" alt="Shield icon with transparent background"/>
        /// <br/>Shield icon with transparency</para></div></term>
        /// <term><div style="text-align:center;width:512px">
        /// <para><img src="../Help/Images/ShieldArgb1555BlackA128.png" alt="Shield icon with ARGB1555 pixel format, black background and default alpha threshold"/>
        /// <br/>Default optional parameter values (black background, alpha threshold = 128)</para>
        /// <para><img src="../Help/Images/ShieldArgb1555SilverA1.png" alt="Shield icon with ARGB1555 pixel format, silver background and alpha threshold = 1"/>
        /// <br/>Silver background, alpha threshold = 1</para>
        /// <para><img src="../Help/Images/ShieldArgb1555SilverA128Dithered.png" alt="Shield icon with ARGB1555 pixel format, silver background, default alpha threshold and Floyd-Steinberg dithering"/>
        /// <br/>Silver background, default alpha threshold, <see cref="ErrorDiffusionDitherer.FloydSteinberg">Floyd-Steinberg</see> dithering</para></div></term>
        /// </item>
        /// </list></para>
        /// </example>
        public static PredefinedColorsQuantizer Argb1555(Color backColor = default, byte alphaThreshold = 128)
        {
            static Color32 Quantize(Color32 c) => new Color16Argb1555(c).ToColor32();

            return new PredefinedColorsQuantizer(Quantize, new Color32(backColor), alphaThreshold);
        }

        /// <summary>
        /// Gets a <see cref="PredefinedColorsQuantizer"/> instance that quantizes colors to 8-bit ones where red,
        /// green and blue components are encoded in 3, 3 and 2 bits, respectively.
        /// <br/>See the <strong>Remarks</strong> section for details and some examples.
        /// </summary>
        /// <param name="backColor">Colors with alpha (transparency) will be blended with this color before quantization.
        /// The <see cref="Color.A"/> property of the background color is ignored. This parameter is optional.
        /// <br/>Default value: <see cref="Color.Empty"/>, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="directMapping"><see langword="true"/>&#160;to map any color directly to an index instead of searching for a nearest color,
        /// which is very fast but without dithering may end up in a noticeably poorer result and higher contrast;
        /// <see langword="false"/>&#160;to perform a lookup to determine nearest colors, which may be slower but more accurate. This parameter is optional.
        /// <br/>Default value: <see langword="false"/>.</param>
        /// <returns>A <see cref="PredefinedColorsQuantizer"/> instance that quantizes colors to 8-bit ones where red,
        /// green and blue components are encoded in 3, 3 and 2 bits, respectively.</returns>
        /// <remarks>
        /// <para>If <paramref name="directMapping"/> is <see langword="true"/>, then the result of the quantization may have a higher contrast than without direct color mapping,
        /// though this can be compensated if the returned quantizer is combined with an <see cref="ErrorDiffusionDitherer"/>. Other ditherers preserve the effect of the <paramref name="directMapping"/> parameter.</para>
        /// <para>The returned <see cref="PredefinedColorsQuantizer"/> instance can return up to 256 colors.</para>
        /// <para>This quantizer fits well for <see cref="Bitmap"/>s with <see cref="PixelFormat.Format8bppIndexed"/> pixel format.</para>
        /// <para>The palette of this quantizer does not contain the transparent color.</para>
        /// <note>For more information about the possible usable <see cref="PixelFormat"/>s on different platforms see the <strong>Remarks</strong> section of the <see cref="ImageExtensions.ConvertPixelFormat(Image,PixelFormat,Color,byte)">ConvertPixelFormat</see> extension method.</note>
        /// </remarks>
        /// <example>
        /// The following example demonstrates how to use the quantizer returned by this method:
        /// <code lang="C#"><![CDATA[
        /// public static Bitmap ToRgb332(Bitmap source, Color backColor = default, bool directMapping = false, IDitherer ditherer = null)
        /// {
        ///     IQuantizer quantizer = PredefinedColorsQuantizer.Rgb332(backColor, directMapping);
        ///
        ///     // a.) this solution returns a new bitmap and does not change the original one:
        ///     return source.ConvertPixelFormat(PixelFormat.Format8bppIndexed, quantizer, ditherer);
        ///
        ///     // b.) alternatively, you can perform the quantization directly on the source bitmap:
        ///     if (ditherer == null)
        ///         source.Quantize(quantizer);
        ///     else
        ///         source.Dither(quantizer, ditherer);
        ///     return source;
        /// }]]></code>
        /// <para>The example above may produce the following results:
        /// <list type="table">
        /// <listheader><term>Original image</term><term>Quantized image</term></listheader>
        /// <item>
        /// <term><div style="text-align:center;width:512px">
        /// <para><img src="../Help/Images/AlphaGradient.png" alt="Color hues with alpha gradient"/>
        /// <br/>Color hues with alpha gradient</para></div></term>
        /// <term>
        /// <div style="text-align:center;width:512px">
        /// <para><img src="../Help/Images/AlphaGradientRgb332Black.gif" alt="Color hues with RGB332 palette and black background using nearest color lookup"/>
        /// <br/>Default optional parameter values (nearest color lookup)</para>
        /// <para><img src="../Help/Images/AlphaGradientRgb332Silver.gif" alt="Color hues with RGB332 palette and silver background using nearest color lookup"/>
        /// <br/>Silver background, nearest color lookup</para>
        /// <para><img src="../Help/Images/AlphaGradientRgb332SilverDM.gif" alt="Color hues with RGB332 palette and silver background using direct color mapping"/>
        /// <br/>Silver background, direct color mapping</para>
        /// <para><img src="../Help/Images/AlphaGradientRgb332SilverDMDithered.gif" alt="Color hues with RGB332 palette, silver background, using direct color mapping and Bayer 8x8 ordered dithering"/>
        /// <br/>Silver background, direct color mapping, <see cref="OrderedDitherer.Bayer8x8">Bayer 8x8</see> dithering</para></div></term>
        /// </item>
        /// <item>
        /// <term><div style="text-align:center;width:512px">
        /// <para><img src="../Help/Images/GrayShades.gif" alt="Grayscale color shades with different bit depths"/>
        /// <br/>Grayscale color shades</para></div></term>
        /// <term>
        /// <div style="text-align:center;width:512px">
        /// <para><img src="../Help/Images/GrayShadesRgb332.gif" alt="Grayscale color shades with RGB332 palette using nearest color lookup"/>
        /// <br/>Nearest color lookup</para>
        /// <para><img src="../Help/Images/GrayShadesRgb332Direct.gif" alt="Grayscale color shades with RGB332 palette using direct color mapping"/>
        /// <br/>Direct color mapping</para>
        /// <para><img src="../Help/Images/GrayShadesRgb332DitheredB8.gif" alt="Grayscale color shades with RGB332 palette, using nearest color lookup and Bayer 8x8 ordered dithering"/>
        /// <br/>Nearest color lookup, <see cref="OrderedDitherer.Bayer8x8">Bayer 8x8</see> dithering</para>
        /// <para><img src="../Help/Images/GrayShadesRgb332DirectDitheredB8.gif" alt="Grayscale color shades with RGB332 palette, using direct color mapping and Bayer 8x8 ordered dithering"/>
        /// <br/>Direct color mapping, <see cref="OrderedDitherer.Bayer8x8">Bayer 8x8</see> dithering</para></div></term>
        /// </item>
        /// <item>
        /// <term><div style="text-align:center;width:512px">
        /// <para><img src="../Help/Images/Shield256.png" alt="Shield icon with transparent background"/>
        /// <br/>Shield icon with transparency</para></div></term>
        /// <term><div style="text-align:center;width:512px">
        /// <para><img src="../Help/Images/ShieldRgb332Black.gif" alt="Shield icon with RGB332 palette and black background using nearest color lookup"/>
        /// <br/>Default optional parameter values (nearest color lookup)</para>
        /// <para><img src="../Help/Images/ShieldRgb332Silver.gif" alt="Shield icon with RGB332 palette and silver background using nearest color lookup"/>
        /// <br/>Silver background, nearest color lookup</para>
        /// <para><img src="../Help/Images/ShieldRgb332SilverDM.gif" alt="Shield icon with RGB332 palette and silver background using direct color mapping"/>
        /// <br/>Silver background, direct color mapping</para>
        /// <para><img src="../Help/Images/ShieldRgb332SilverDMDithered.gif" alt="Shield icon with RGB332 palette, silver background, using direct color mapping and Floyd-Steinberg dithering"/>
        /// <br/>Silver background, direct color mapping, <see cref="ErrorDiffusionDitherer.FloydSteinberg">Floyd-Steinberg</see> dithering</para></div></term>
        /// </item>
        /// <item>
        /// <term><div style="text-align:center;width:512px">
        /// <para><img src="../Help/Images/Lena.png" alt="Test image &quot;Lena&quot;"/>
        /// <br/>Original test image "Lena"</para></div></term>
        /// <term>
        /// <div style="text-align:center;width:512px">
        /// <para><img src="../Help/Images/LenaRgb332.gif" alt="Test image &quot;Lena&quot; with RGB332 palette using nearest color lookup"/>
        /// <br/>Nearest color lookup</para>
        /// <para><img src="../Help/Images/LenaRgb332DM.gif" alt="Test image &quot;Lena&quot; with RGB332 palette using direct color mapping"/>
        /// <br/>Direct color mapping</para>
        /// <para><img src="../Help/Images/LenaRgb332DMFloydSteinberg.gif" alt="Test image &quot;Lena&quot; with RGB332 palette using direct color mapping and Floyd-Steinberg dithering"/>
        /// <br/>Direct color mapping, <see cref="ErrorDiffusionDitherer.FloydSteinberg">Floyd-Steinberg</see> dithering</para></div></term>
        /// </item>
        /// </list></para>
        /// </example>
        public static PredefinedColorsQuantizer Rgb332(Color backColor = default, bool directMapping = false)
        {
            var bg = new Color32(backColor);

            int GetNearestColorIndex(Color32 c)
            {
                if (c.A < Byte.MaxValue)
                    c = c.BlendWithBackground(bg);

                return (c.R & 0b11100000) | ((c.G & 0b11100000) >> 3) | ((c.B & 0b11000000) >> 6);
            }

            return new PredefinedColorsQuantizer(new Palette(Rgb332Palette, bg, 0, directMapping ? GetNearestColorIndex : (Func<Color32, int>)null));
        }

        /// <summary>
        /// Gets a <see cref="PredefinedColorsQuantizer"/> instance that quantizes colors to 8-bit grayscale ones of 256 shades.
        /// <br/>See the <strong>Remarks</strong> section for details and some examples.
        /// </summary>
        /// <param name="backColor">Colors with alpha (transparency) will be blended with this color before quantization.
        /// The <see cref="Color.A"/> property of the background color is ignored. This parameter is optional.
        /// <br/>Default value: <see cref="Color.Empty"/>, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <returns>A <see cref="PredefinedColorsQuantizer"/> instance that quantizes colors to 8-bit grayscale ones.</returns>
        /// <remarks>
        /// <para>The returned quantizer uses direct mapping to grayscale colors based on human perception, which is makes quantization very fast and is very accurate at the same time.</para>
        /// <para>The returned <see cref="PredefinedColorsQuantizer"/> instance can return up to 256 possible shades of gray.</para>
        /// <para>The palette of this quantizer does not contain the transparent color. To make an image grayscale with transparency you can use the
        /// <see cref="ImageExtensions.ToGrayscale">ToGrayscale</see> and <see cref="BitmapExtensions.MakeGrayscale">MakeGrayscale</see> extension methods.</para>
        /// <para>This quantizer fits well for <see cref="Bitmap"/>s with <see cref="PixelFormat.Format8bppIndexed"/> pixel format.</para>
        /// <note>For more information about the possible usable <see cref="PixelFormat"/>s on different platforms see the <strong>Remarks</strong> section of the <see cref="ImageExtensions.ConvertPixelFormat(Image,PixelFormat,Color,byte)">ConvertPixelFormat</see> extension method.</note>
        /// </remarks>
        /// <example>
        /// The following example demonstrates how to use the quantizer returned by this method:
        /// <code lang="C#"><![CDATA[
        /// public static Bitmap ToGrayscale(Bitmap source, Color backColor = default)
        /// {
        ///     IQuantizer quantizer = PredefinedColorsQuantizer.Grayscale(backColor);
        ///
        ///     // a.) this solution returns a new bitmap and does not change the original one:
        ///     return source.ConvertPixelFormat(PixelFormat.Format8bppIndexed, quantizer);
        ///     
        ///     // b.) alternatively, you can perform the quantization directly on the source bitmap:
        ///     source.Quantize(quantizer);
        ///     return source;
        /// }]]></code>
        /// <para>The example above may produce the following results:
        /// <list type="table">
        /// <listheader><term>Original image</term><term>Quantized image</term></listheader>
        /// <item>
        /// <term><div style="text-align:center;width:512px">
        /// <para><img src="../Help/Images/AlphaGradient.png" alt="Color hues with alpha gradient"/>
        /// <br/>Color hues with alpha gradient</para></div></term>
        /// <term>
        /// <div style="text-align:center;width:512px">
        /// <para><img src="../Help/Images/AlphaGradientGray8bppBlack.gif" alt="Grayscale color hues with 8 BPP grayscale palette and black background"/>
        /// <br/>Default (black) background</para>
        /// <para><img src="../Help/Images/AlphaGradientGray8bppSilver.gif" alt="Graayscale color hues with 8 BPP grayscale palette and silver background"/>
        /// <br/>Silver background</para></div></term>
        /// </item>
        /// <item>
        /// <term><div style="text-align:center;width:512px">
        /// <para><img src="../Help/Images/Shield256.png" alt="Shield icon with transparent background"/>
        /// <br/>Shield icon with transparency</para></div></term>
        /// <term><div style="text-align:center;width:512px">
        /// <para><img src="../Help/Images/ShieldGray8bppBlack.gif" alt="Shield icon with 8 BPP grayscale palette and black background"/>
        /// <br/>Default (black) background</para>
        /// <para><img src="../Help/Images/ShieldGray8bppSilver.gif" alt="Shield icon with 8 BPP grayscale palette and silver background"/>
        /// <br/>Silver background</para></div></term>
        /// </item>
        /// </list></para>
        /// </example>
        public static PredefinedColorsQuantizer Grayscale(Color backColor = default)
        {
            var bg = new Color32(backColor);

            int GetNearestColorIndex(Color32 c)
            {
                if (c.A < Byte.MaxValue)
                    c = c.BlendWithBackground(bg);
                return c.GetBrightness();
            }

            return new PredefinedColorsQuantizer(new Palette(Grayscale256Palette, bg, 0, GetNearestColorIndex));
        }

        /// <summary>
        /// Gets a <see cref="PredefinedColorsQuantizer"/> instance that quantizes colors to 4-bit grayscale ones of 16 shades.
        /// <br/>See the <strong>Remarks</strong> section for details and some examples.
        /// </summary>
        /// <param name="backColor">Colors with alpha (transparency) will be blended with this color before quantization.
        /// The <see cref="Color.A"/> property of the background color is ignored. This parameter is optional.
        /// <br/>Default value: <see cref="Color.Empty"/>, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="directMapping"><see langword="true"/>&#160;to map any color directly to an index instead of searching for a nearest color,
        /// which is very fast but may end up in a result of a bit higher contrast than the original image;
        /// <see langword="false"/>&#160;to perform a lookup to determine nearest colors, which may be slower but more accurate. This parameter is optional.
        /// <br/>Default value: <see langword="false"/>.</param>
        /// <returns>A <see cref="PredefinedColorsQuantizer"/> instance that quantizes colors to 4-bit grayscale ones.</returns>
        /// <remarks>
        /// <para>If <paramref name="directMapping"/> is <see langword="true"/>, then the result of the quantization may have a higher contrast than without direct color mapping,
        /// though this can be compensated if the returned quantizer is combined with an <see cref="ErrorDiffusionDitherer"/>. Other ditherers preserve the effect of the <paramref name="directMapping"/> parameter.</para>
        /// <para>The returned <see cref="PredefinedColorsQuantizer"/> instance can return up to 16 possible shades of gray.</para>
        /// <para>This quantizer fits well for <see cref="Bitmap"/>s with <see cref="PixelFormat.Format4bppIndexed"/> pixel format.</para>
        /// <note>For more information about the possible usable <see cref="PixelFormat"/>s on different platforms see the <strong>Remarks</strong> section of the <see cref="ImageExtensions.ConvertPixelFormat(Image,PixelFormat,Color,byte)">ConvertPixelFormat</see> extension method.</note>
        /// </remarks>
        /// <example>
        /// The following example demonstrates how to use the quantizer returned by this method:
        /// <code lang="C#"><![CDATA[
        /// public static Bitmap ToGrayscale16(Bitmap source, Color backColor = default, bool directMapping = false, IDitherer ditherer = null)
        /// {
        ///     IQuantizer quantizer = PredefinedColorsQuantizer.Grayscale16(backColor, directMapping);
        ///
        ///     // a.) this solution returns a new bitmap and does not change the original one:
        ///     return source.ConvertPixelFormat(PixelFormat.Format4bppIndexed, quantizer, ditherer);
        ///
        ///     // b.) alternatively, you can perform the quantization directly on the source bitmap:
        ///     if (ditherer == null)
        ///         source.Quantize(quantizer);
        ///     else
        ///         source.Dither(quantizer, ditherer);
        ///     return source;
        /// }]]></code>
        /// <para>The example above may produce the following results:
        /// <list type="table">
        /// <listheader><term>Original image</term><term>Quantized image</term></listheader>
        /// <item>
        /// <term><div style="text-align:center;width:512px">
        /// <para><img src="../Help/Images/AlphaGradient.png" alt="Color hues with alpha gradient"/>
        /// <br/>Color hues with alpha gradient</para></div></term>
        /// <term>
        /// <div style="text-align:center;width:512px">
        /// <para><img src="../Help/Images/AlphaGradientGray4bppBlack.gif" alt="Color hues with 4 BPP grayscale palette and black background using nearest color lookup"/>
        /// <br/>Default optional parameter values (nearest color lookup)</para>
        /// <para><img src="../Help/Images/AlphaGradientGray4bppSilver.gif" alt="Color hues with 4 BPP grayscale palette and silver background using nearest color lookup"/>
        /// <br/>Silver background, nearest color lookup</para>
        /// <para><img src="../Help/Images/AlphaGradientGray4bppSilverDirect.gif" alt="Color hues with 4 BPP grayscale palette and silver background using direct color mapping"/>
        /// <br/>Silver background, direct color mapping</para>
        /// <para><img src="../Help/Images/AlphaGradientGray4bppSilverDitheredB8.gif" alt="Color hues with 4 BPP grayscale palette, silver background, using nearest color lookup and Bayer 8x8 ordered dithering"/>
        /// <br/>Silver background, nearest color lookup, <see cref="OrderedDitherer.Bayer8x8">Bayer 8x8</see> dithering</para></div></term>
        /// </item>
        /// <item>
        /// <term><div style="text-align:center;width:512px">
        /// <para><img src="../Help/Images/GrayShades.gif" alt="Grayscale color shades with different bit depths"/>
        /// <br/>Grayscale color shades</para></div></term>
        /// <term>
        /// <div style="text-align:center;width:512px">
        /// <para><img src="../Help/Images/GrayShades4bpp.gif" alt="Grayscale color shades with 4 BPP grayscale palette using nearest color lookup"/>
        /// <br/>Nearest color lookup</para>
        /// <para><img src="../Help/Images/GrayShades4bppDirect.gif" alt="Grayscale color shades with 2 BPP grayscale palette using direct color mapping"/>
        /// <br/>Direct color mapping</para>
        /// <para><img src="../Help/Images/GrayShades4bppDitheredB8.gif" alt="Grayscale color shades with 4 BPP grayscale palette, using nearest color lookup and Bayer 8x8 ordered dithering"/>
        /// <br/>Nearest color lookup, <see cref="OrderedDitherer.Bayer8x8">Bayer 8x8</see> dithering</para></div></term>
        /// </item>
        /// <item>
        /// <term><div style="text-align:center;width:512px">
        /// <para><img src="../Help/Images/Shield256.png" alt="Shield icon with transparent background"/>
        /// <br/>Shield icon with transparency</para></div></term>
        /// <term><div style="text-align:center;width:512px">
        /// <para><img src="../Help/Images/ShieldGray4bppBlack.gif" alt="Shield icon with 4 BPP grayscale palette and black background using nearest color lookup"/>
        /// <br/>Default optional parameter values (nearest color lookup)</para>
        /// <para><img src="../Help/Images/ShieldGray4bppSilver.gif" alt="Shield icon with 4 BPP grayscale palette and silver background using nearest color lookup"/>
        /// <br/>Silver background, nearest color lookup</para>
        /// <para><img src="../Help/Images/ShieldGray4bppSilverDirect.gif" alt="Shield icon with 4 BPP grayscale palette and silver background using direct color mapping"/>
        /// <br/>Silver background, direct color mapping</para>
        /// <para><img src="../Help/Images/ShieldGray4bppSilverDirectDitheredFS.gif" alt="Shield icon with 4 BPP grayscale palette, silver background, using direct color mapping and Floyd-Steinberg dithering"/>
        /// <br/>Silver background, direct color mapping, <see cref="ErrorDiffusionDitherer.FloydSteinberg">Floyd-Steinberg</see> dithering</para></div></term>
        /// </item>
        /// </list></para>
        /// </example>
        public static PredefinedColorsQuantizer Grayscale16(Color backColor = default, bool directMapping = false)
        {
            var bg = new Color32(backColor);

            int GetNearestColorIndex(Color32 c)
            {
                if (c.A < Byte.MaxValue)
                    c = c.BlendWithBackground(bg);
                return c.GetBrightness() >> 4;
            }

            return new PredefinedColorsQuantizer(new Palette(Grayscale16Palette, bg, 0, directMapping ? GetNearestColorIndex : (Func<Color32, int>)null));
        }

        /// <summary>
        /// Gets a <see cref="PredefinedColorsQuantizer"/> instance that quantizes colors to 2-bit grayscale ones of 4 shades.
        /// <br/>See the <strong>Remarks</strong> section for details and some examples.
        /// </summary>
        /// <param name="backColor">Colors with alpha (transparency) will be blended with this color before quantization.
        /// The <see cref="Color.A"/> property of the background color is ignored. This parameter is optional.
        /// <br/>Default value: <see cref="Color.Empty"/>, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="directMapping"><see langword="true"/>&#160;to map any color directly to an index instead of searching for a nearest color,
        /// which is very fast but may end up in a result of a bit higher contrast than the original image;
        /// <see langword="false"/>&#160;to perform a lookup to determine nearest colors, which may be slower but more accurate. This parameter is optional.
        /// <br/>Default value: <see langword="false"/>.</param>
        /// <returns>A <see cref="PredefinedColorsQuantizer"/> instance that quantizes colors to 2-bit grayscale ones.</returns>
        /// <remarks>
        /// <para>If <paramref name="directMapping"/> is <see langword="true"/>, then the result of the quantization may have a higher contrast than without direct color mapping,
        /// though this can be compensated if the returned quantizer is combined with an <see cref="ErrorDiffusionDitherer"/>. Other ditherers preserve the effect of the <paramref name="directMapping"/> parameter.</para>
        /// <para>The returned <see cref="PredefinedColorsQuantizer"/> instance can return up to 4 possible shades of gray.</para>
        /// <para>This quantizer fits well for <see cref="Bitmap"/>s with <see cref="PixelFormat.Format4bppIndexed"/> pixel format, though only 4 palette entries are used instead of the possible 16.</para>
        /// <note>For more information about the possible usable <see cref="PixelFormat"/>s on different platforms see the <strong>Remarks</strong> section of the <see cref="ImageExtensions.ConvertPixelFormat(Image,PixelFormat,Color,byte)">ConvertPixelFormat</see> extension method.</note>
        /// </remarks>
        /// <example>
        /// The following example demonstrates how to use the quantizer returned by this method:
        /// <code lang="C#"><![CDATA[
        /// public static Bitmap ToGrayscale4(Bitmap source, Color backColor = default, bool directMapping = false, IDitherer ditherer = null)
        /// {
        ///     IQuantizer quantizer = PredefinedColorsQuantizer.Grayscale4(backColor, directMapping);
        ///
        ///     // a.) this solution returns a new bitmap and does not change the original one:
        ///     return source.ConvertPixelFormat(PixelFormat.Format4bppIndexed, quantizer, ditherer);
        ///
        ///     // b.) alternatively, you can perform the quantization directly on the source bitmap:
        ///     if (ditherer == null)
        ///         source.Quantize(quantizer);
        ///     else
        ///         source.Dither(quantizer, ditherer);
        ///     return source;
        /// }]]></code>
        /// <para>The example above may produce the following results:
        /// <list type="table">
        /// <listheader><term>Original image</term><term>Quantized image</term></listheader>
        /// <item>
        /// <term><div style="text-align:center;width:512px">
        /// <para><img src="../Help/Images/AlphaGradient.png" alt="Color hues with alpha gradient"/>
        /// <br/>Color hues with alpha gradient</para></div></term>
        /// <term>
        /// <div style="text-align:center;width:512px">
        /// <para><img src="../Help/Images/AlphaGradientGray2bppBlack.gif" alt="Color hues with 2 BPP grayscale palette and black background using nearest color lookup"/>
        /// <br/>Default optional parameter values (nearest color lookup)</para>
        /// <para><img src="../Help/Images/AlphaGradientGray2bppSilver.gif" alt="Color hues with 2 BPP grayscale palette and silver background using nearest color lookup"/>
        /// <br/>Silver background, nearest color lookup</para>
        /// <para><img src="../Help/Images/AlphaGradientGray2bppSilverDirect.gif" alt="Color hues with 2 BPP grayscale palette and silver background using direct color mapping"/>
        /// <br/>Silver background, direct color mapping</para>
        /// <para><img src="../Help/Images/AlphaGradientGray2bppSilverDitheredB8.gif" alt="Color hues with 2 BPP grayscale palette, silver background, using nearest color lookup and Bayer 8x8 ordered dithering"/>
        /// <br/>Silver background, nearest color lookup, <see cref="OrderedDitherer.Bayer8x8">Bayer 8x8</see> dithering</para></div></term>
        /// </item>
        /// <item>
        /// <term><div style="text-align:center;width:512px">
        /// <para><img src="../Help/Images/GrayShades.gif" alt="Grayscale color shades with different bit depths"/>
        /// <br/>Grayscale color shades</para></div></term>
        /// <term>
        /// <div style="text-align:center;width:512px">
        /// <para><img src="../Help/Images/GrayShades2bpp.gif" alt="Grayscale color shades with 2 BPP grayscale palette using nearest color lookup"/>
        /// <br/>Nearest color lookup</para>
        /// <para><img src="../Help/Images/GrayShades2bppDirect.gif" alt="Grayscale color shades with 2 BPP grayscale palette using direct color mapping"/>
        /// <br/>Direct color mapping</para>
        /// <para><img src="../Help/Images/GrayShades2bppDitheredB8.gif" alt="Grayscale color shades with 2 BPP grayscale palette, using nearest color lookup and Bayer 8x8 ordered dithering"/>
        /// <br/>Nearest color lookup, <see cref="OrderedDitherer.Bayer8x8">Bayer 8x8</see> dithering</para></div></term>
        /// </item>
        /// <item>
        /// <term><div style="text-align:center;width:512px">
        /// <para><img src="../Help/Images/Shield256.png" alt="Shield icon with transparent background"/>
        /// <br/>Shield icon with transparency</para></div></term>
        /// <term><div style="text-align:center;width:512px">
        /// <para><img src="../Help/Images/ShieldGray2bppBlack.gif" alt="Shield icon with 2 BPP grayscale palette and black background using nearest color lookup"/>
        /// <br/>Default optional parameter values (nearest color lookup)</para>
        /// <para><img src="../Help/Images/ShieldGray2bppSilver.gif" alt="Shield icon with 2 BPP grayscale palette and silver background using nearest color lookup"/>
        /// <br/>Silver background, nearest color lookup</para>
        /// <para><img src="../Help/Images/ShieldGray2bppSilverDirect.gif" alt="Shield icon with 2 BPP grayscale palette and silver background using direct color mapping"/>
        /// <br/>Silver background, direct color mapping</para>
        /// <para><img src="../Help/Images/ShieldGray2bppSilverDirectDitheredFS.gif" alt="Shield icon with 2 BPP grayscale palette, silver background, using direct color mapping and Floyd-Steinberg dithering"/>
        /// <br/>Silver background, direct color mapping, <see cref="ErrorDiffusionDitherer.FloydSteinberg">Floyd-Steinberg</see> dithering</para></div></term>
        /// </item>
        /// <item>
        /// <term><div style="text-align:center;width:512px">
        /// <para><img src="../Help/Images/Cameraman.png" alt="Test image &quot;Cameraman&quot;"/>
        /// <br/>Original test image "Cameraman"</para></div></term>
        /// <term>
        /// <div style="text-align:center;width:512px">
        /// <para><img src="../Help/Images/Cameraman2bpp.gif" alt="Test image &quot;Cameraman&quot; with 2 BPP grayscale palette using nearest color lookup"/>
        /// <br/>Nearest color lookup</para>
        /// <para><img src="../Help/Images/Cameraman2bppDirect.gif" alt="Test image &quot;Cameraman&quot; with 2 BPP grayscale palette using direct color mapping"/>
        /// <br/>Direct color mapping</para>
        /// <para><img src="../Help/Images/Cameraman2bppDirectDitheredFS.gif" alt="Test image &quot;Cameraman&quot; with 2 BPP grayscale palette, silver background, using direct color mapping and Floyd-Steinberg dithering"/>
        /// <br/>Direct color mapping, <see cref="ErrorDiffusionDitherer.FloydSteinberg">Floyd-Steinberg</see> dithering</para></div></term>
        /// </item>
        /// </list></para>
        /// </example>
        public static PredefinedColorsQuantizer Grayscale4(Color backColor = default, bool directMapping = false)
        {
            var bg = new Color32(backColor);

            int GetNearestColorIndex(Color32 c)
            {
                if (c.A < Byte.MaxValue)
                    c = c.BlendWithBackground(bg);
                return c.GetBrightness() >> 6;
            }

            return new PredefinedColorsQuantizer(new Palette(Grayscale4Palette, bg, 0, directMapping ? GetNearestColorIndex : (Func<Color32, int>)null));
        }

        /// <summary>
        /// Gets a <see cref="PredefinedColorsQuantizer"/> instance that quantizes every color to black or white.
        /// </summary>
        /// <param name="backColor">Colors with alpha (transparency) will be blended with the specified <paramref name="backColor"/> before quantization.
        /// The <see cref="Color.A"/> property of the background color is ignored. This parameter is optional.
        /// <br/>Default value: <see cref="Color.Empty"/>, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="whiteThreshold">Specifies a threshold value for the brightness of the colors, under which a quantized color is considered black.
        /// If <c>0</c>, then the complete result will be white. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <returns>A <see cref="PredefinedColorsQuantizer"/> instance that quantizes every color to black or white.</returns>
        /// <remarks>
        /// <para>If the returned quantizer is combined with an <see cref="ErrorDiffusionDitherer"/>, then the effect of the <paramref name="whiteThreshold"/> parameter is
        /// mostly compensated. Other ditherers preserve the effect of the <paramref name="whiteThreshold"/> parameter.</para>
        /// <para>This quantizer fits well for <see cref="Bitmap"/>s with <see cref="PixelFormat.Format1bppIndexed"/> pixel format.</para>
        /// <note>For more information about the possible usable <see cref="PixelFormat"/>s on different platforms see the <strong>Remarks</strong> section of the <see cref="ImageExtensions.ConvertPixelFormat(Image,PixelFormat,Color,byte)">ConvertPixelFormat</see> extension method.</note>
        /// </remarks>
        /// <example>
        /// The following example demonstrates how to use the quantizer returned by this method:
        /// <code lang="C#"><![CDATA[
        /// public static Bitmap ToBlackAndWhite(Bitmap source, Color backColor = default,
        ///     byte whiteThreshold = 128, IDitherer ditherer = null)
        /// {
        ///     IQuantizer quantizer = PredefinedColorsQuantizer.BlackAndWhite(backColor, whiteThreshold);
        ///
        ///     // a.) this solution returns a new bitmap and does not change the original one:
        ///     return source.ConvertPixelFormat(PixelFormat.Format1bppIndexed, quantizer, ditherer);
        ///
        ///     // b.) alternatively, you can perform the quantization directly on the source bitmap:
        ///     if (ditherer == null)
        ///         source.Quantize(quantizer);
        ///     else
        ///         source.Dither(quantizer, ditherer);
        ///     return source;
        /// }]]></code>
        /// <para>The example above may produce the following results:
        /// <list type="table">
        /// <listheader><term>Original image</term><term>Quantized image</term></listheader>
        /// <item>
        /// <term><div style="text-align:center;width:512px">
        /// <para><img src="../Help/Images/AlphaGradient.png" alt="Color hues with alpha gradient"/>
        /// <br/>Color hues with alpha gradient</para></div></term>
        /// <term>
        /// <div style="text-align:center;width:512px">
        /// <para><img src="../Help/Images/AlphaGradientBWBlack.gif" alt="Color hues with black and white palette and black background"/>
        /// <br/>Default optional parameter values (black background)</para>
        /// <para><img src="../Help/Images/AlphaGradientBWSilver.gif" alt="Color hues with black and white palette and silver background"/>
        /// <br/>Silver background</para>
        /// <para><img src="../Help/Images/AlphaGradientBWSilverDitheredB8.gif" alt="Color hues with black and white palette, silver background, using Bayer 8x8 ordered dithering"/>
        /// <br/>Silver background, <see cref="OrderedDitherer.Bayer8x8">Bayer 8x8</see> dithering</para></div></term>
        /// </item>
        /// <item>
        /// <term><div style="text-align:center;width:512px">
        /// <para><img src="../Help/Images/GrayShades.gif" alt="Grayscale color shades with different bit depths"/>
        /// <br/>Grayscale color shades</para></div></term>
        /// <term>
        /// <div style="text-align:center;width:512px">
        /// <para><img src="../Help/Images/GrayShadesBW.gif" alt="Grayscale color shades with black and white palette"/>
        /// <br/>Default optional parameter values</para>
        /// <para><img src="../Help/Images/GrayShadesBWThr32.gif" alt="Grayscale color shades with black and white palette, white threshold = 32"/>
        /// <br/>White threshold = 32</para>
        /// <para><img src="../Help/Images/GrayShadesBWThr224.gif" alt="Grayscale color shades with black and white palette, white threshold = 224"/>
        /// <br/>White threshold = 224</para>
        /// <para><img src="../Help/Images/GrayShadesBWDitheredB8.gif" alt="Grayscale color shades with black and white palette, using Bayer 8x8 ordered dithering"/>
        /// <br/>Default white threshold, <see cref="OrderedDitherer.Bayer8x8">Bayer 8x8</see> dithering</para></div></term>
        /// </item>
        /// <item>
        /// <term><div style="text-align:center;width:512px">
        /// <para><img src="../Help/Images/Shield256.png" alt="Shield icon with transparent background"/>
        /// <br/>Shield icon with transparency</para></div></term>
        /// <term><div style="text-align:center;width:512px">
        /// <para><img src="../Help/Images/ShieldBWBlack.gif" alt="Shield icon with black and white palette and black background"/>
        /// <br/>Default optional parameter values (black background)</para>
        /// <para><img src="../Help/Images/ShieldBWSilver.gif" alt="Shield icon with black and white palette and silver background"/>
        /// <br/>Silver background</para>
        /// <para><img src="../Help/Images/ShieldBWSilverDitheredFS.gif" alt="Shield icon with black and white palette, silver background, using Floyd-Steinberg dithering"/>
        /// <br/>Silver background, <see cref="ErrorDiffusionDitherer.FloydSteinberg">Floyd-Steinberg</see> dithering</para></div></term>
        /// </item>
        /// <item>
        /// <term><div style="text-align:center;width:512px">
        /// <para><img src="../Help/Images/Cameraman.png" alt="Test image &quot;Cameraman&quot;"/>
        /// <br/>Original test image "Cameraman"</para></div></term>
        /// <term>
        /// <div style="text-align:center;width:512px">
        /// <para><img src="../Help/Images/CameramanBW.gif" alt="Test image &quot;Cameraman&quot; with black and white palette"/>
        /// <br/>Default optional parameter values</para>
        /// <para><img src="../Help/Images/CameramanBWThr96.gif" alt="Test image &quot;Cameraman&quot; with black and white palette, white threshold = 96"/>
        /// <br/>White threshold = 96</para>
        /// <para><img src="../Help/Images/CameramanBWThr96DitheredB8.gif" alt="Test image &quot;Cameraman&quot; with black and white palette, using Bayer 8x8 dithering and white threshold = 96"/>
        /// <br/>White threshold = 96, <see cref="OrderedDitherer.Bayer8x8">Bayer 8x8</see> dithering. The ordered dithering preserves the white threshold value.</para>
        /// <para><img src="../Help/Images/CameramanBWThr96DitheredFS.gif" alt="Test image &quot;Cameraman&quot; with black and white palette, using Floyd-Steinberg dithering and white threshold = 96"/>
        /// <br/>White threshold = 96, <see cref="ErrorDiffusionDitherer.FloydSteinberg">Floyd-Steinberg</see> dithering. The error diffusion dithering compensates the white threshold value.</para></div></term>
        /// </item>
        /// </list></para>
        /// </example>
        public static PredefinedColorsQuantizer BlackAndWhite(Color backColor = default, byte whiteThreshold = 128)
        {
            var bg = new Color32(backColor);

            int GetNearestColorIndex(Color32 c)
            {
                if (c.A < Byte.MaxValue)
                    c = c.BlendWithBackground(bg);

                return c == Color32.Black ? 0
                    : c == Color32.White ? 1
                    : c.GetBrightness() >= whiteThreshold ? 1 : 0;
            }

            return new PredefinedColorsQuantizer(new Palette(BlackAndWhitePalette, bg, 0, GetNearestColorIndex));
        }

        /// <summary>
        /// Gets a <see cref="PredefinedColorsQuantizer"/> instance that quantizes colors using the system default 8-bit palette.
        /// On Windows this palette contains the 16 standard <a href="https://www.w3.org/TR/REC-html40/types.html#h-6.5" target="_blank">basic sRGB colors</a>,
        /// the "web-safe" palette of 216 colors as well as 24 transparent entries.
        /// <br/>See the <strong>Remarks</strong> section for details and some examples.
        /// </summary>
        /// <param name="backColor">Colors with alpha (transparency), which are considered opaque will be blended with this color before quantization.
        /// The <see cref="Color.A"/> property of the background color is ignored. This parameter is optional.
        /// <br/>Default value: <see cref="Color.Empty"/>, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="alphaThreshold">If the system default 8-bit palette contains a transparent color on the current operating system,
        /// then specifies a threshold value for the <see cref="Color.A">Color.A</see> property, under which a quantized color is considered transparent.
        /// If <c>0</c>, then the quantized colors will never be transparent. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <returns>A <see cref="PredefinedColorsQuantizer"/> instance that quantizes colors using the system default 8-bit palette.</returns>
        /// <remarks>
        /// <para>The returned <see cref="PredefinedColorsQuantizer"/> instance can return up to 256 colors.
        /// On Windows this amount is somewhat smaller because of redundant entries in the palette.</para>
        /// <para>This quantizer fits well for <see cref="Bitmap"/>s with <see cref="PixelFormat.Format8bppIndexed"/> pixel format.</para>
        /// <para>On Windows the palette of this quantizer contains transparent entries.</para>
        /// <note>For more information about the possible usable <see cref="PixelFormat"/>s on different platforms see the <strong>Remarks</strong> section of the <see cref="ImageExtensions.ConvertPixelFormat(Image,PixelFormat,Color,byte)">ConvertPixelFormat</see> extension method.</note>
        /// </remarks>
        /// <example>
        /// The following example demonstrates how to use the quantizer returned by this method:
        /// <code lang="C#"><![CDATA[
        /// public static Bitmap ToDefault8Bpp(Bitmap source, Color backColor = default,
        ///     byte alphaThreshold = 128, IDitherer ditherer = null)
        /// {
        ///     IQuantizer quantizer = PredefinedColorsQuantizer.SystemDefault8BppPalette(backColor, alphaThreshold);
        ///
        ///     // a.) this solution returns a new bitmap and does not change the original one:
        ///     return source.ConvertPixelFormat(PixelFormat.Format8bppIndexed, quantizer, ditherer);
        ///
        ///     // b.) when converting to Format8bppIndexed format without dithering, this produces the same result:
        ///     if (ditherer == null)
        ///         return source.ConvertPixelFormat(PixelFormat.Format8bppIndexed, backColor, alphaThreshold);
        ///
        ///     // c.) alternatively, you can perform the quantization directly on the source bitmap:
        ///     if (ditherer == null)
        ///         source.Quantize(quantizer);
        ///     else
        ///         source.Dither(quantizer, ditherer);
        ///     return source;
        /// }]]></code>
        /// <para>The example above may produce the following results:
        /// <list type="table">
        /// <listheader><term>Original image</term><term>Quantized image</term></listheader>
        /// <item>
        /// <term><div style="text-align:center;width:512px">
        /// <para><img src="../Help/Images/AlphaGradient.png" alt="Color hues with alpha gradient"/>
        /// <br/>Color hues with alpha gradient</para></div></term>
        /// <term>
        /// <div style="text-align:center;width:512px">
        /// <para><img src="../Help/Images/AlphaGradientDefault8bppBlackA128.gif" alt="Color hues with system default 8 BPP palette, black background and default alpha threshold"/>
        /// <br/>Default optional parameter values (black background, alpha threshold = 128). The bottom half of the image is transparent.</para>
        /// <para><img src="../Help/Images/AlphaGradientDefault8bppSilverA1.gif" alt="Color hues with system default 8 BPP palette, silver background and alpha threshold = 1"/>
        /// <br/>Silver background, alpha threshold = 1. Only the bottom line is transparent.</para>
        /// <para><img src="../Help/Images/AlphaGradientDefault8bppSilverDitheredB8.gif" alt="Color hues with system default 8 BPP palette, silver background, default alpha threshold and Bayer 8x8 ordered dithering"/>
        /// <br/>Silver background, default alpha threshold, <see cref="OrderedDitherer.Bayer8x8">Bayer 8x8</see> dithering. The bottom half of the image is transparent.</para></div></term>
        /// </item>
        /// <item>
        /// <term><div style="text-align:center;width:512px">
        /// <para><img src="../Help/Images/GrayShades.gif" alt="Grayscale color shades with different bit depths"/>
        /// <br/>Grayscale color shades</para></div></term>
        /// <term>
        /// <div style="text-align:center;width:512px">
        /// <para><img src="../Help/Images/GrayShadesDefault8bpp.gif" alt="Grayscale color shades with system default 8 BPP palette"/>
        /// <br/>Default optional parameter values</para>
        /// <para><img src="../Help/Images/GrayShadesDefault8bppDitheredB8.gif" alt="Grayscale color shades with system default 8 BPP palette using Bayer 8x8 ordered dithering"/>
        /// <br/><see cref="OrderedDitherer.Bayer8x8">Bayer 8x8</see> dithering</para></div></term>
        /// </item>
        /// <item>
        /// <term><div style="text-align:center;width:512px">
        /// <para><img src="../Help/Images/Shield256.png" alt="Shield icon with transparent background"/>
        /// <br/>Shield icon with transparency</para></div></term>
        /// <term><div style="text-align:center;width:512px">
        /// <para><img src="../Help/Images/ShieldDefault8bppBlack.gif" alt="Shield icon with system default 8 BPP palette"/>
        /// <br/>Default optional parameter values (black background, alpha threshold = 128)</para>
        /// <para><img src="../Help/Images/ShieldDefault8bppBlackDitheredB8.gif" alt="Shield icon with system default 8 BPP palette using Bayer 8x8 ordered dithering"/>
        /// <br/>Default background and alpha threshold, <see cref="OrderedDitherer.Bayer8x8">Bayer 8x8</see> dithering</para>
        /// <para><img src="../Help/Images/ShieldDefault8bppSilverA1DitheredFS.gif" alt="Shield icon with system default 8 BPP palette using silver background, alpha threshold = 1 and Floyd-Steinberg dithering"/>
        /// <br/>Silver background, alpha threshold = 1, <see cref="ErrorDiffusionDitherer.FloydSteinberg">Floyd-Steinberg</see> dithering</para></div></term>
        /// </item>
        /// <item>
        /// <term><div style="text-align:center;width:512px">
        /// <para><img src="../Help/Images/Lena.png" alt="Test image &quot;Lena&quot;"/>
        /// <br/>Original test image "Lena"</para></div></term>
        /// <term>
        /// <div style="text-align:center;width:512px">
        /// <para><img src="../Help/Images/LenaDefault8bpp.gif" alt="Test image &quot;Lena&quot; with system default 8 BPP palette"/>
        /// <br/>Default optional parameter values</para>
        /// <para><img src="../Help/Images/LenaDefault8bppDitheredFS.gif" alt="Test image &quot;Lena&quot; with system default 8 BPP palette using Floyd-Steinberg dithering"/>
        /// <br/><see cref="ErrorDiffusionDitherer.FloydSteinberg">Floyd-Steinberg</see> dithering</para></div></term>
        /// </item>
        /// </list></para>
        /// </example>
        public static PredefinedColorsQuantizer SystemDefault8BppPalette(Color backColor = default, byte alphaThreshold = 128)
            => new PredefinedColorsQuantizer(System8BppPalette, new Color32(backColor), alphaThreshold);

        /// <summary>
        /// Gets a <see cref="PredefinedColorsQuantizer"/> instance that quantizes colors using the system default 4-bit palette.
        /// On Windows this palette consists of the 16 standard <a href="https://www.w3.org/TR/REC-html40/types.html#h-6.5" target="_blank">basic sRGB colors</a>.
        /// <br/>See the <strong>Remarks</strong> section for details and some examples.
        /// </summary>
        /// <param name="backColor">Colors with alpha (transparency) will be blended with this color before quantization.
        /// The <see cref="Color.A"/> property of the background color is ignored. This parameter is optional.
        /// <br/>Default value: <see cref="Color.Empty"/>, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <returns>A <see cref="PredefinedColorsQuantizer"/> instance that quantizes colors using the system default 4-bit palette.</returns>
        /// <remarks>
        /// <para>The returned <see cref="PredefinedColorsQuantizer"/> instance can return up to 16 colors.</para>
        /// <para>This quantizer fits well for <see cref="Bitmap"/>s with <see cref="PixelFormat.Format4bppIndexed"/> pixel format.</para>
        /// <para>The palette of this quantizer is not expected to contain transparent entries.
        /// On Windows the palette consists of the 16 standard <a href="https://www.w3.org/TR/REC-html40/types.html#h-6.5" target="_blank">basic sRGB colors</a></para>
        /// <note>For more information about the possible usable <see cref="PixelFormat"/>s on different platforms see the <strong>Remarks</strong> section of the <see cref="ImageExtensions.ConvertPixelFormat(Image,PixelFormat,Color,byte)">ConvertPixelFormat</see> extension method.</note>
        /// </remarks>
        /// <example>
        /// The following example demonstrates how to use the quantizer returned by this method:
        /// <code lang="C#"><![CDATA[
        /// public static Bitmap ToDefault4Bpp(Bitmap source, Color backColor = default, IDitherer ditherer = null)
        /// {
        ///     IQuantizer quantizer = PredefinedColorsQuantizer.SystemDefault4BppPalette(backColor);
        ///
        ///     // a.) this solution returns a new bitmap and does not change the original one:
        ///     return source.ConvertPixelFormat(PixelFormat.Format8bppIndexed, quantizer, ditherer);
        ///
        ///     // b.) when converting to Format4bppIndexed format without dithering, this produces the same result:
        ///     if (ditherer == null)
        ///         return source.ConvertPixelFormat(PixelFormat.Format4bppIndexed, backColor);
        ///
        ///     // c.) alternatively, you can perform the quantization directly on the source bitmap:
        ///     if (ditherer == null)
        ///         source.Quantize(quantizer);
        ///     else
        ///         source.Dither(quantizer, ditherer);
        ///     return source;
        /// } ]]></code>
        /// <para>The example above may produce the following results:
        /// <list type="table">
        /// <listheader><term>Original image</term><term>Quantized image</term></listheader>
        /// <item>
        /// <term><div style="text-align:center;width:512px">
        /// <para><img src="../Help/Images/AlphaGradient.png" alt="Color hues with alpha gradient"/>
        /// <br/>Color hues with alpha gradient</para></div></term>
        /// <term>
        /// <div style="text-align:center;width:512px">
        /// <para><img src="../Help/Images/AlphaGradientDefault4bppBlack.gif" alt="Color hues with system default 4 BPP palette and black background"/>
        /// <br/>Default optional parameter values (black background)</para>
        /// <para><img src="../Help/Images/AlphaGradientDefault4bppSilver.gif" alt="Color hues with system default 4 BPP palette and silver background"/>
        /// <br/>Silver background</para>
        /// <para><img src="../Help/Images/AlphaGradientDefault4bppSilverDitheredB8.gif" alt="Color hues with system default 4 BPP palette, using silver background and a stronger Bayer 8x8 ordered dithering"/>
        /// <br/>Silver background, <see cref="OrderedDitherer.Bayer8x8">Bayer 8x8</see> dithering with strength = 0.5</para></div></term>
        /// </item>
        /// <item>
        /// <term><div style="text-align:center;width:512px">
        /// <para><img src="../Help/Images/GrayShades.gif" alt="Grayscale color shades with different bit depths"/>
        /// <br/>Grayscale color shades</para></div></term>
        /// <term>
        /// <div style="text-align:center;width:512px">
        /// <para><img src="../Help/Images/GrayShadesDefault4bpp.gif" alt="Grayscale color shades with system default 4 BPP palette"/>
        /// <br/>Default optional parameter values. The asymmetry is due to the uneven distribution of gray shades of this palette.</para>
        /// <para><img src="../Help/Images/GrayShadesDefault4bppDitheredB8.gif" alt="Grayscale color shades with system default 4 BPP palette using Bayer 8x8 ordered dithering"/>
        /// <br/><see cref="OrderedDitherer.Bayer8x8">Bayer 8x8</see> dithering with auto strength. Darker shades have banding.</para>
        /// <para><img src="../Help/Images/GrayShadesDefault4bppDitheredB8Str-5.gif" alt="Grayscale color shades with system default 4 BPP palette using a stronger Bayer 8x8 ordered dithering"/>
        /// <br/><see cref="OrderedDitherer.Bayer8x8">Bayer 8x8</see> dithering with strength = 0.5. Now there is no banding but white suffers from overdithering.</para></div></term>
        /// </item>
        /// <item>
        /// <term><div style="text-align:center;width:512px">
        /// <para><img src="../Help/Images/Shield256.png" alt="Shield icon with transparent background"/>
        /// <br/>Shield icon with transparency</para></div></term>
        /// <term><div style="text-align:center;width:512px">
        /// <para><img src="../Help/Images/ShieldDefault4bppBlack.gif" alt="Shield icon with system default 4 BPP palette and black background"/>
        /// <br/>Default optional parameter values (black background)</para>
        /// <para><img src="../Help/Images/ShieldDefault4bppSilver.gif" alt="Shield icon with system default 4 BPP palette and silver background"/>
        /// <br/>Silver background</para>
        /// <para><img src="../Help/Images/ShieldDefault4bppSilverDitheredFS.gif" alt="Shield icon with system default 4 BPP palette using silver background and Floyd-Steinberg dithering"/>
        /// <br/>Silver background, <see cref="ErrorDiffusionDitherer.FloydSteinberg">Floyd-Steinberg</see> dithering</para></div></term>
        /// </item>
        /// </list></para>
        /// </example>
        public static PredefinedColorsQuantizer SystemDefault4BppPalette(Color backColor = default)
            => new PredefinedColorsQuantizer(System4BppPalette, new Color32(backColor));

        /// <summary>
        /// Gets a <see cref="PredefinedColorsQuantizer"/> instance that quantizes colors using the system default 1-bit palette.
        /// On Windows this palette consists of the black and white colors.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="backColor">Colors with alpha (transparency) will be blended with this color before quantization.
        /// The <see cref="Color.A"/> property of the background color is ignored. This parameter is optional.
        /// <br/>Default value: <see cref="Color.Empty"/>, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <returns>A <see cref="PredefinedColorsQuantizer"/> instance that quantizes colors using the system default 1-bit palette.</returns>
        /// <remarks>
        /// <para>The returned <see cref="PredefinedColorsQuantizer"/> instance can return up to 2 colors.
        /// The system 1-bit palette expected to have the black and white colors on most operating systems.
        /// <note type="tip">To make sure that you use a black and white palette use the <see cref="BlackAndWhite">BlackAndWhite</see> method instead, which provides white threshold adjustment as well.
        /// <br/>For more details and examples see the <strong>Examples</strong> section of the <see cref="BlackAndWhite">BlackAndWhite</see> method.</note></para>
        /// <para>This quantizer fits well for <see cref="Bitmap"/>s with <see cref="PixelFormat.Format4bppIndexed"/> pixel format.</para>
        /// <note>For more information about the possible usable <see cref="PixelFormat"/>s on different platforms see the <strong>Remarks</strong> section of the <see cref="ImageExtensions.ConvertPixelFormat(Image,PixelFormat,Color,byte)">ConvertPixelFormat</see> extension method.</note>
        /// </remarks>
        public static PredefinedColorsQuantizer SystemDefault1BppPalette(Color backColor = default)
            => new PredefinedColorsQuantizer(System1BppPalette, new Color32(backColor));

        /// <summary>
        /// Gets a <see cref="PredefinedColorsQuantizer"/> instance that quantizes colors using the colors in the specified <paramref name="palette"/>.
        /// <br/>See the <strong>Remarks</strong> section for details and some examples.
        /// </summary>
        /// <param name="palette">The array of colors to be used by the returned instance.</param>
        /// <param name="backColor">Colors with alpha (transparency), which are considered opaque will be blended with this color before quantization.
        /// The <see cref="Color.A"/> property of the background color is ignored. This parameter is optional.
        /// <br/>Default value: <see cref="Color.Empty"/>, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="alphaThreshold">If the specified <paramref name="palette"/> contains a transparent color,
        /// then specifies a threshold value for the <see cref="Color.A">Color.A</see> property, under which a quantized color is considered transparent.
        /// If <c>0</c>, then the quantized colors will never be transparent. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <returns>A <see cref="PredefinedColorsQuantizer"/> instance that quantizes colors using the colors in the specified <paramref name="palette"/>.</returns>
        /// <remarks>
        /// <para>The <see cref="PredefinedColorsQuantizer"/> instance returned by this method will use a <see cref="Palette"/> internally, created from
        /// the colors specified in the <see cref="palette"/> parameter. When quantizing, best matching colors might be looked up sequentially and results
        /// might be cached.</para>
        /// <para>If a color to be quantized can be mapped to a color index directly, then create a <see cref="Palette"/> instance explicitly,
        /// specifying the custom mapping logic and use the <see cref="FromCustomPalette(Palette)"/> overload instead.</para>
        /// <para>If a color to be quantized can be transformed to a result color directly, and the quantized result is not needed to be an indexed image,
        /// then use the <see cref="O:KGySoft.Drawing.Imaging.PredefinedColorsQuantizer.FromCustomFunction">FromCustomFunction</see> overloads instead.</para>
        /// </remarks>
        /// <example>
        /// The following example demonstrates how to use the quantizer returned by this method:
        /// <code lang="C#"><![CDATA[
        /// public static Bitmap ToRgb111(Bitmap source, Color backColor = default, IDitherer ditherer = null)
        /// {
        ///     Color[] colors =
        ///     {
        ///         Color.Black, Color.Red, Color.Lime, Color.Blue,
        ///         Color.Magenta, Color.Yellow, Color.Cyan, Color.White
        ///     };
        ///
        ///     IQuantizer quantizer = PredefinedColorsQuantizer.FromCustomPalette(colors, backColor);
        ///     // or: quantizer = PredefinedColorsQuantizer.FromCustomPalette(new Palette(colors, backColor));
        ///
        ///     // a.) this solution returns a new bitmap and does not change the original one:
        ///     return source.ConvertPixelFormat(PixelFormat.Format4bppIndexed, quantizer, ditherer);
        ///
        ///     // b.) alternatively, you can perform the quantization directly on the source bitmap:
        ///     if (ditherer == null)
        ///         source.Quantize(quantizer);
        ///     else
        ///         source.Dither(quantizer, ditherer);
        ///     return source;
        /// }]]></code>
        /// <para>The example above may produce the following results:
        /// <list type="table">
        /// <listheader><term>Original image</term><term>Quantized image</term></listheader>
        /// <item>
        /// <term><div style="text-align:center;width:512px">
        /// <para><img src="../Help/Images/AlphaGradient.png" alt="Color hues with alpha gradient"/>
        /// <br/>Color hues with alpha gradient</para></div></term>
        /// <term>
        /// <div style="text-align:center;width:512px">
        /// <para><img src="../Help/Images/AlphaGradientRgb111Black.gif" alt="Color hues with RGB111 palette and black background"/>
        /// <br/>Default optional parameter values (black background). The bottom half of the result is black.</para>
        /// <para><img src="../Help/Images/AlphaGradientRgb111Silver.gif" alt="Color hues with RGB111 palette and silver background"/>
        /// <br/>Silver background. The bottom part of the result is white.</para>
        /// <para><img src="../Help/Images/AlphaGradientRgb111SilverDitheredB8.gif" alt="Color hues with RGB111 palette and silver background, using Bayer 8x8 ordered dithering"/>
        /// <br/>Silver background, <see cref="OrderedDitherer.Bayer8x8">Bayer 8x8</see> dithering</para></div></term>
        /// </item>
        /// <item>
        /// <term><div style="text-align:center;width:512px">
        /// <para><img src="../Help/Images/GrayShades.gif" alt="Grayscale color shades with different bit depths"/>
        /// <br/>Grayscale color shades</para></div></term>
        /// <term>
        /// <div style="text-align:center;width:512px">
        /// <para><img src="../Help/Images/GrayShadesBW.gif" alt="Grayscale color shades with RGB111 palette"/>
        /// <br/>Default optional parameter values</para>
        /// <para><img src="../Help/Images/GrayShadesBWDitheredB8.gif" alt="Grayscale color shades with RGB111 palette, using Bayer 8x8 ordered dithering"/>
        /// <br/><see cref="OrderedDitherer.Bayer8x8">Bayer 8x8</see> dithering</para></div></term>
        /// </item>
        /// <item>
        /// <term><div style="text-align:center;width:512px">
        /// <para><img src="../Help/Images/Shield256.png" alt="Shield icon with transparent background"/>
        /// <br/>Shield icon with transparency</para></div></term>
        /// <term><div style="text-align:center;width:512px">
        /// <para><img src="../Help/Images/ShieldRgb111Black.gif" alt="Shield icon with RGB111 palette and black background"/>
        /// <br/>Default optional parameter values (black background)</para>
        /// <para><img src="../Help/Images/ShieldRgb111Silver.gif" alt="Shield icon with RGB111 palette and silver background"/>
        /// <br/>Silver background</para>
        /// <para><img src="../Help/Images/ShieldRgb111SilverDitheredFS.gif" alt="Shield icon with RGB111 palette, silver background, using Floyd-Steinberg dithering"/>
        /// <br/>Silver background, <see cref="ErrorDiffusionDitherer.FloydSteinberg">Floyd-Steinberg</see> dithering</para></div></term>
        /// </item>
        /// <item>
        /// <term><div style="text-align:center;width:512px">
        /// <para><img src="../Help/Images/Lena.png" alt="Test image &quot;Lena&quot;"/>
        /// <br/>Original test image "Lena"</para></div></term>
        /// <term>
        /// <div style="text-align:center;width:512px">
        /// <para><img src="../Help/Images/LenaRgb111.gif" alt="Test image &quot;Lena&quot; with RGB111 palette"/>
        /// <br/>Default optional parameter values</para>
        /// <para><img src="../Help/Images/LenaRgb111DitheredFS.gif" alt="Test image &quot;Lena&quot; with RGB111 palette and Floyd-Steinberg dithering"/>
        /// <br/><see cref="ErrorDiffusionDitherer.FloydSteinberg">Floyd-Steinberg</see> dithering</para></div></term>
        /// </item>
        /// </list></para>
        /// </example>
        public static PredefinedColorsQuantizer FromCustomPalette(Color[] palette, Color backColor = default, byte alphaThreshold = 128)
            => new PredefinedColorsQuantizer(palette, backColor, alphaThreshold);

        /// <summary>
        /// Gets a <see cref="PredefinedColorsQuantizer"/> instance that quantizes colors using the specified <paramref name="palette"/>.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="palette">The <see cref="Palette"/> to be used by the returned instance.</param>
        /// <returns>A <see cref="PredefinedColorsQuantizer"/> instance that quantizes colors using the specified <paramref name="palette"/>.</returns>
        /// <remarks>
        /// <para>If a color to be quantized can be transformed to a result color directly, and the quantized result is not needed to be an indexed image,
        /// then use the <see cref="O:KGySoft.Drawing.Imaging.PredefinedColorsQuantizer.FromCustomFunction">FromCustomFunction</see> overloads instead.</para>
        /// <note>For examples see the <strong>Examples</strong> section of the <see cref="FromCustomPalette(Color[], Color, byte)"/> overload.</note>
        /// </remarks>
        public static PredefinedColorsQuantizer FromCustomPalette(Palette palette) => new PredefinedColorsQuantizer(palette);

        /// <summary>
        /// Gets a <see cref="PredefinedColorsQuantizer"/> instance that quantizes colors using the custom quantizer function specified in the <paramref name="quantizingFunction"/> parameter.
        /// <br/>See the <strong>Remarks</strong> section for details and some examples.
        /// </summary>
        /// <param name="quantizingFunction">A delegate that specifies the custom quantization logic. It must be thread-safe for parallel invoking and it is expected to be fast.
        /// The results returned by the delegate are not cached.</param>
        /// <param name="backColor">Colors with alpha (transparency), whose <see cref="Color.A">Color.A</see> property
        /// is equal to or greater than <paramref name="alphaThreshold"/> will be blended with this color before invoking the <paramref name="quantizingFunction"/> delegate.
        /// The <see cref="Color.A"/> property of the background color is ignored.</param>
        /// <param name="alphaThreshold">Specifies a threshold value for the <see cref="Color.A">Color.A</see> property, under which a quantized color is considered transparent.
        /// If <c>0</c>, then even the completely transparent colors will be blended with <paramref name="backColor"/> before invoking the <paramref name="quantizingFunction"/> delegate. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <returns>A <see cref="PredefinedColorsQuantizer"/> instance that quantizes colors using the custom quantizer function specified in the <paramref name="quantizingFunction"/> parameter.</returns>
        /// <remarks>
        /// <para>The quantizer returned by this method does not have a palette. If you need to create indexed using a custom mapping function that
        /// uses up to 256 different colors, then create a <see cref="Palette"/> instance specifying a custom function and call the <see cref="FromCustomPalette(Palette)"/> method instead.</para>
        /// <para>This overload never calls the <paramref name="quantizingFunction"/> delegate with a color with alpha. Depending on <paramref name="alphaThreshold"/> either a completely
        /// transparent color will be returned or the color will be blended with <paramref name="backColor"/> before invoking the delegate.
        /// In order to invoke <paramref name="quantizingFunction"/> alpha colors use the <see cref="FromCustomFunction(Func{Color32, Color32})"/> overload instead.</para>
        /// </remarks>
        /// <example>
        /// The following example demonstrates how to use the quantizer returned by this method:
        /// <code lang="C#"><![CDATA[
        /// public static Bitmap ToGrayscaleOpaque(Bitmap source, Color backColor = default)
        /// {
        ///     IQuantizer quantizer = PredefinedColorsQuantizer.FromCustomFunction(c => c.ToGray(), backColor, 0);
        ///
        ///     // a.) this solution returns a new bitmap and does not change the original one:
        ///     return source.ConvertPixelFormat(PixelFormat.Format24bppRgb, quantizer);
        ///
        ///     // b.) alternatively, you can perform the quantization directly on the source bitmap:
        ///     source.Quantize(quantizer);
        ///     return source;
        /// }]]></code>
        /// <para>The example above may produce the following results:
        /// <list type="table">
        /// <listheader><term>Original image</term><term>Quantized image</term></listheader>
        /// <item>
        /// <term><div style="text-align:center;width:512px">
        /// <para><img src="../Help/Images/AlphaGradient.png" alt="Color hues with alpha gradient"/>
        /// <br/>Color hues with alpha gradient</para></div></term>
        /// <term>
        /// <div style="text-align:center;width:512px">
        /// <para><img src="../Help/Images/AlphaGradientGray8bppBlack.gif" alt="Grayscale color hues with black background"/>
        /// <br/>Default (black) background</para>
        /// <para><img src="../Help/Images/AlphaGradientGray8bppSilver.gif" alt="Graayscale color hues with silver background"/>
        /// <br/>Silver background</para></div></term>
        /// </item>
        /// <item>
        /// <term><div style="text-align:center;width:512px">
        /// <para><img src="../Help/Images/Shield256.png" alt="Shield icon with transparent background"/>
        /// <br/>Shield icon with transparency</para></div></term>
        /// <term><div style="text-align:center;width:512px">
        /// <para><img src="../Help/Images/ShieldGray8bppBlack.gif" alt="Grayscale shield icon with black background"/>
        /// <br/>Default (black) background</para>
        /// <para><img src="../Help/Images/ShieldGray8bppSilver.gif" alt="Grayscale shield icon with silver background"/>
        /// <br/>Silver background</para></div></term>
        /// </item>
        /// </list></para>
        /// </example>
        public static PredefinedColorsQuantizer FromCustomFunction(Func<Color32, Color32> quantizingFunction, Color backColor, byte alphaThreshold = 0)
            => new PredefinedColorsQuantizer(quantizingFunction, new Color32(backColor), alphaThreshold);

        /// <summary>
        /// Gets a <see cref="PredefinedColorsQuantizer"/> instance that quantizes colors using the custom quantizer function specified in the <paramref name="quantizingFunction"/> parameter.
        /// <br/>See the <strong>Remarks</strong> section for details and some examples.
        /// </summary>
        /// <param name="quantizingFunction">A delegate that specifies the custom quantization logic. It must be thread-safe for parallel invoking and it is expected to be fast.
        /// The results returned by the delegate are not cached.</param>
        /// <returns>A <see cref="PredefinedColorsQuantizer"/> instance that quantizes colors using the custom quantizer function specified in the <paramref name="quantizingFunction"/> parameter.</returns>
        /// <remarks>
        /// <para>The quantizer returned by this method does not have a palette. If you need to create indexed using a custom mapping function that
        /// uses up to 256 different colors, then create a <see cref="Palette"/> instance specifying a custom function and call the <see cref="FromCustomPalette(Palette)"/> method instead.</para>
        /// <para>This overload always calls the <paramref name="quantizingFunction"/> delegate without preprocessing the input colors.
        /// In order to pass blended colors only to the <paramref name="quantizingFunction"/> delegate use the <see cref="FromCustomFunction(Func{Color32, Color32}, Color, byte)"/> overload instead.</para>
        /// </remarks>
        /// <example>
        /// The following example demonstrates how to use the quantizer returned by this method:
        /// <code lang="C#"><![CDATA[
        /// public static Bitmap ToGrayscalePreserveAlpha(Bitmap source)
        /// {
        ///     IQuantizer quantizer = PredefinedColorsQuantizer.FromCustomFunction(c => c.ToGray());
        ///
        ///     // a.) this solution returns a new bitmap and does not change the original one:
        ///     return source.ConvertPixelFormat(PixelFormat.Format32bppArgb, quantizer);
        ///
        ///     // b.) alternatively, you can perform the quantization directly on the source bitmap:
        ///     source.Quantize(quantizer);
        ///     return source;
        /// }]]></code>
        /// <para>The example above may produce the following results:
        /// <list type="table">
        /// <listheader><term>Original image</term><term>Quantized image</term></listheader>
        /// <item>
        /// <term><div style="text-align:center;width:512px">
        /// <para><img src="../Help/Images/AlphaGradient.png" alt="Color hues with alpha gradient"/>
        /// <br/>Color hues with alpha gradient</para></div></term>
        /// <term>
        /// <div style="text-align:center;width:512px">
        /// <para><img src="../Help/Images/AlphaGradientGrayscale.png" alt="Grayscale color hues with alpha preserved"/>
        /// <br/>Alpha has been preserved</para></div></term>
        /// </item>
        /// <item>
        /// <term><div style="text-align:center;width:512px">
        /// <para><img src="../Help/Images/Shield256.png" alt="Shield icon with transparent background"/>
        /// <br/>Shield icon with transparency</para></div></term>
        /// <term><div style="text-align:center;width:512px">
        /// <para><img src="../Help/Images/ShieldGrayscale.png" alt="Grayscale shield icon with alpha preserved"/>
        /// <br/>Alpha has been preserved</para></div></term>
        /// </item>
        /// </list></para>
        /// </example>
        public static PredefinedColorsQuantizer FromCustomFunction(Func<Color32, Color32> quantizingFunction)
            => new PredefinedColorsQuantizer(quantizingFunction);

        #endregion

        #region Internal Methods

        internal static PredefinedColorsQuantizer FromBitmapData(BitmapDataAccessorBase bitmapData)
        {
            switch (bitmapData.PixelFormat)
            {
                case PixelFormat.Format16bppArgb1555:
                    return Argb1555(bitmapData.BackColor.ToColor(), bitmapData.AlphaThreshold);
                case PixelFormat.Format16bppRgb565:
                    return Rgb565(bitmapData.BackColor.ToColor());
                case PixelFormat.Format16bppRgb555:
                    return Rgb555(bitmapData.BackColor.ToColor());
                case PixelFormat.Format16bppGrayScale:
                    return Grayscale(bitmapData.BackColor.ToColor());
                case PixelFormat.Format8bppIndexed:
                case PixelFormat.Format4bppIndexed:
                case PixelFormat.Format1bppIndexed:
                    return FromCustomPalette(bitmapData.Palette);
                default:
                    return Rgb888(bitmapData.BackColor.ToColor());
            }
        }

        #endregion

        #endregion

        #region Instance Methods

        IQuantizingSession IQuantizer.Initialize(IReadableBitmapData source)
            => palette != null
                ? (IQuantizingSession)new QuantizingSessionIndexed(this, palette)
                : new QuantizingSessionCustomMapping(this, quantizingFunction);

        #endregion

        #endregion
    }
}
