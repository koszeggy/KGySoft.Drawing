#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: KnownPixelFormat.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2025 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System.Diagnostics.CodeAnalysis;

#endregion

namespace KGySoft.Drawing.Imaging
{
    /// <summary>
    /// Represents pixel formats with built-in support.
    /// For custom formats you can use the <see cref="PixelFormatInfo"/> type when applicable.
    /// </summary>
    /// <remarks>
    /// <note>The <see cref="KnownPixelFormat"/> enumeration contains all formats that
    /// the <a href="https://docs.microsoft.com/en-us/dotnet/api/System.Drawing.Imaging.PixelFormat" target="_blank">System.Drawing.Imaging.PixelFormat</a>
    /// enumeration also has, though some fields have different values and the matching names do not necessarily represent the same pixel layout either.
    /// For example, in this library wide formats, such as <see cref="Format64bppArgb"/>, use the full 16 bit per color channel range and the same gamma correction as the
    /// 8-bit per channel formats, whereas <a href="https://docs.microsoft.com/en-us/dotnet/api/System.Drawing.Imaging.PixelFormat" target="_blank">System.Drawing.Imaging.PixelFormat.Format64bppArgb</a>
    /// might represent a different range or gamma correction, depending on the operating system.</note>
    /// </remarks>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Compatibility")]
    public enum KnownPixelFormat
    {
        // NOTE: if new fields are introduced update the To[Known]PixelFormat (as extensions, PixelFormatInfo), FromPixelFormat methods and factories as well

        /// <summary>
        /// The pixel format is undefined.
        /// </summary>
        Undefined = 0,

        /// <summary>
        /// <para>Represents the indexed 1 bit per pixel format. The corresponding color palette can have up to 2 colors.</para>
        /// <para>Bit order: bits are filled up from the most significant bit to the least significant bit within a byte.</para>
        /// <para>Indexed pixel formats are represented by the <see cref="Palette"/> type where every palette entry is a <see cref="Color32"/>.</para>
        /// </summary>
        Format1bppIndexed = PixelFormatInfo.Format1bppIndexed,

        /// <summary>
        /// <para>Represents the indexed 4 bit per pixel format. The corresponding color palette can have up to 16 colors.</para>
        /// <para>Bit order: even pixels occupy the high bits of a byte, whereas odd pixels are in the low bits.</para>
        /// <para>Indexed pixel formats are represented by the <see cref="Palette"/> type where every palette entry is a <see cref="Color32"/>.</para>
        /// </summary>
        Format4bppIndexed = PixelFormatInfo.Format4bppIndexed,

        /// <summary>
        /// <para>Represents the indexed 8 bit per pixel format. The corresponding color palette can have up to 256 colors.</para>
        /// <para>Indexed pixel formats are represented by the <see cref="Palette"/> type where every palette entry is a <see cref="Color32"/>.</para>
        /// </summary>
        Format8bppIndexed = PixelFormatInfo.Format8bppIndexed,

        /// <summary>
        /// <para>Represents a 16 bit per pixel grayscale format. The color information specifies 65536 shades of gray.</para>
        /// <para>The closest color type that can represent this pixel format without losing information is <see cref="Color64"/>.</para>
        /// </summary>
        Format16bppGrayScale = PixelFormatInfo.Format16bppGrayScale,

        /// <summary>
        /// <para>Represents a 16 bit per pixel RGB color format where red, green and blue channels use 5 bits per pixel and 1 bit is unused.</para>
        /// <para>Bit order from LSB to MSB: 5 blue, 5 green, 5 red bits and 1 unused bit. As a binary value a pixel can be specified as <c>0b_X_RRRRR_GGGGG_BBBBB</c>
        /// on little endian CPUs where <c>X</c> is the ignored most significant bit and <c>BBBBB</c> is the blue component at the least significant bits.</para>
        /// <para>The closest color type that can represent this pixel format without losing information is <see cref="Color32"/>.</para>
        /// </summary>
        Format16bppRgb555 = PixelFormatInfo.Format16bppRgb555,

        /// <summary>
        /// <para>Represents a 16 bit per pixel RGB color format where red, green and blue channels use 5, 6 and 5 bits per pixel, respectively.</para>
        /// <para>Bit order from LSB to MSB: 5 blue, 6 green and 5 red bits. As a binary value a pixel can be specified as <c>0b_RRRRR_GGGGGG_BBBBB</c>
        /// on little endian CPUs where <c>BBBBB</c> is the blue component at the least significant bits.</para>
        /// <para>The closest color type that can represent this pixel format without losing information is <see cref="Color32"/>.</para>
        /// </summary>
        Format16bppRgb565 = PixelFormatInfo.Format16bppRgb565,

        /// <summary>
        /// <para>Represents a 16 bit per pixel ARGB color format where red, green and blue channels use 5 bits per pixel along with 1 bit for alpha.</para>
        /// <para>Bit order from LSB to MSB: 5 blue, 5 green, 5 red bits and 1 alpha bit. As a binary value a pixel can be specified as <c>0b_A_RRRRR_GGGGG_BBBBB</c>
        /// on little endian CPUs where <c>A</c> is the most significant bit and <c>BBBBB</c> is the blue component at the least significant bits.</para>
        /// <para>The closest color type that can represent this pixel format without losing information is <see cref="Color32"/>.</para>
        /// </summary>
        Format16bppArgb1555 = PixelFormatInfo.Format16bppArgb1555,

        /// <summary>
        /// <para>Represents a 24 bit per pixel RGB color format where red, green and blue channels use 8 bits per pixel.</para>
        /// <para>Bit order from LSB to MSB: 8 blue, 8 green and 8 red bits.</para>
        /// <para>The closest color type that can represent this pixel format without losing information is <see cref="Color32"/>.</para>
        /// </summary>
        Format24bppRgb = PixelFormatInfo.Format24bppRgb,

        /// <summary>
        /// <para>Represents a 32 bit per pixel RGB color format where red, green and blue channels use 8 bits per pixel. The remaining 8 bits are not used.</para>
        /// <para>Bit order from LSB to MSB: 8 blue, 8 green, 8 red and 8 unused bits. As a hex value a pixel can be specified as <c>0x_XX_RR_GG_BB</c>
        /// on little endian CPUs where <c>XX</c> is the unused most significant byte and <c>BB</c> is the blue component at the least significant byte.</para>
        /// <para>The closest color type that can represent this pixel format without losing information is <see cref="Color32"/>.</para>
        /// </summary>
        Format32bppRgb = PixelFormatInfo.Format32bppRgb,

        /// <summary>
        /// <para>Represents a 32 bit per pixel ARGB color format where alpha, red, green and blue channels use 8 bits per pixel.</para>
        /// <para>Bit order from LSB to MSB: 8 blue, 8 green, 8 red and 8 alpha bits. As a hex value a pixel can be specified as <c>0x_AA_RR_GG_BB</c>
        /// on little endian CPUs where <c>BB</c> is the blue component at the least significant byte.</para>
        /// <para>The matching color type that represents this pixel format directly is <see cref="Color32"/>.</para>
        /// </summary>
        Format32bppArgb = PixelFormatInfo.Format32bppArgb,

        /// <summary>
        /// <para>Represents a 32 bit per pixel ARGB color format where alpha, red, green and blue channels use 8 bits per pixel.
        /// The red, green, and blue components are premultiplied, according to the alpha component.</para>
        /// <para>Bit order from LSB to MSB: 8 blue, 8 green, 8 red and 8 alpha bits. As a hex value a pixel can be specified as <c>0x_AA_RR_GG_BB</c>
        /// on little endian CPUs where <c>BB</c> is the blue component at the least significant byte.</para>
        /// <para>The matching color type that represents this pixel format directly is <see cref="PColor32"/>.</para>
        /// </summary>
        Format32bppPArgb = PixelFormatInfo.Format32bppPArgb,

        /// <summary>
        /// <para>Represents a 48 bit per pixel RGB color format where red, green and blue channels use 16 bits per pixel.</para>
        /// <para>Bit order from LSB to MSB: 16 blue, 16 green and 16 red bits.</para>
        /// <para>The closest color type that can represent this pixel format without losing information is <see cref="Color64"/>.</para>
        /// </summary>
        Format48bppRgb = PixelFormatInfo.Format48bppRgb,

        /// <summary>
        /// <para>Represents a 64 bit per pixel ARGB color format where alpha, red, green and blue channels use 16 bits per pixel.</para>
        /// <para>Bit order from LSB to MSB: 16 blue, 16 green, 16 red and 16 alpha bits. As a hex value a pixel can be specified as <c>0x_AAAA_RRRR_GGGG_BBBB</c>
        /// on little endian CPUs where <c>BBBB</c> is the blue component at the least significant couple of bytes.</para>
        /// <para>The matching color type that represents this pixel format directly is <see cref="Color64"/>.</para>
        /// </summary>
        Format64bppArgb = PixelFormatInfo.Format64bppArgb,

        /// <summary>
        /// <para>Represents a 64 bit per pixel ARGB color format where alpha, red, green and blue channels use 16 bits per pixel.
        /// The red, green, and blue components are premultiplied, according to the alpha component.</para>
        /// <para>Bit order from LSB to MSB: 16 blue, 16 green, 16 red and 16 alpha bits. As a hex value a pixel can be specified as <c>0x_AAAA_RRRR_GGGG_BBBB</c>
        /// on little endian CPUs where <c>BBBB</c> is the blue component at the least significant couple of bytes.</para>
        /// <para>The matching color type that represents this pixel format directly is <see cref="PColor64"/>.</para>
        /// </summary>
        Format64bppPArgb = PixelFormatInfo.Format64bppPArgb,

        /// <summary>
        /// <para>Represents a 96 bit per pixel RGB color format where red, green and blue channels use 32 bits per pixel.</para>
        /// <para>Bit order from LSB to MSB: 32 red, 32 green and 32 blue bits.
        /// The color components are floating point values between 0 and 1 in the linear color space.</para>
        /// <para>The closest color type that can represent this pixel format without losing information is <see cref="ColorF"/>.</para>
        /// </summary>
        Format96bppRgb = PixelFormatInfo.Format96bppRgb,

        /// <summary>
        /// <para>Represents a 128 bit per pixel RGBA color format where alpha, red, green and blue channels use 32 bits per pixel.</para>
        /// <para>Bit order from LSB to MSB: 32 red, 32 green, 32 blue and 32 alpha bits.
        /// The color components are floating point values between 0 and 1 in the linear color space.</para>
        /// <para>The matching color type that represents this pixel format directly is <see cref="ColorF"/>.</para>
        /// </summary>
        Format128bppRgba = PixelFormatInfo.Format128bppRgba,

        /// <summary>
        /// <para>Represents a 128 bit per pixel RGBA color format where alpha, red, green and blue channels use 32 bits per pixel.
        /// The red, green, and blue components are premultiplied, according to the alpha component.</para>
        /// <para>Bit order from LSB to MSB: 32 red, 32 green, 32 blue and 32 alpha bits.
        /// The color components are floating point values between 0 and 1 in the linear color space.</para>
        /// <para>The matching color type that represents this pixel format directly is <see cref="PColorF"/>.</para>
        /// </summary>
        Format128bppPRgba = PixelFormatInfo.Format128bppPRgba,

        /// <summary>
        /// <para>Represents an 8 bit per pixel grayscale format. The color information specifies 256 shades of gray.</para>
        /// <para>The closest color type that can represent this pixel format without losing information is <see cref="Color32"/>.</para>
        /// </summary>
        Format8bppGrayScale = PixelFormatInfo.Format8bppGrayScale,

        /// <summary>
        /// <para>Represents a 32 bit per pixel grayscale format. The color information specifies about one billion shades of gray
        /// between 0 and 1 where the largest difference between two representable values is about 0.00000001 (10<sup>-8</sup>).</para>
        /// <para>The closest color type that can represent this pixel format without losing information is <see cref="ColorF"/>.</para>
        /// </summary>
        Format32bppGrayScale = PixelFormatInfo.Format32bppGrayScale,
    }
}
