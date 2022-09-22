#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: KnownPixelFormat.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2022 - All Rights Reserved
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
    /// 8-bit per channel formats; however, <a href="https://docs.microsoft.com/en-us/dotnet/api/System.Drawing.Imaging.PixelFormat" target="_blank">PixelFormat.Format64bppArgb</a>
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
        /// Represents the indexed 1 bit per pixel format. The corresponding color palette can have up to 2 colors.
        /// </summary>
        Format1bppIndexed = PixelFormatInfo.Format1bppIndexed,

        /// <summary>
        /// Represents the indexed 4 bit per pixel format. The corresponding color palette can have up to 16 colors.
        /// </summary>
        Format4bppIndexed = PixelFormatInfo.Format4bppIndexed,

        /// <summary>
        /// Represents the indexed 8 bit per pixel format. The corresponding color palette can have up to 256 colors.
        /// </summary>
        Format8bppIndexed = PixelFormatInfo.Format8bppIndexed,

        /// <summary>
        /// Represents the 16 bit per pixel grayscale format. The color information specifies 65536 shades of gray.
        /// </summary>
        Format16bppGrayScale = PixelFormatInfo.Format16bppGrayScale,

        /// <summary>
        /// Represents a 16 bit per pixel color format where red, green and blue channels use 5 bits per pixel. The remaining bit is not used.
        /// </summary>
        Format16bppRgb555 = PixelFormatInfo.Format16bppRgb555,

        /// <summary>
        /// Represents a 16 bit per pixel color format where red, green and blue channels use 5, 6 and 5 bits per pixel, respectively.
        /// </summary>
        Format16bppRgb565 = PixelFormatInfo.Format16bppRgb565,

        /// <summary>
        /// Represents a 16 bit per pixel color format where red, green and blue channels use 5 bits per pixel along with 1 bit for alpha.
        /// </summary>
        Format16bppArgb1555 = PixelFormatInfo.Format16bppArgb1555,

        /// <summary>
        /// Represents a 16 bit per pixel color format where red, green and blue channels use 8 bits per pixel.
        /// </summary>
        Format24bppRgb = PixelFormatInfo.Format24bppRgb,

        /// <summary>
        /// Represents a 32 bit per pixel color format where red, green and blue channels use 8 bits per pixel. The remaining 8 bits are not used.
        /// </summary>
        Format32bppRgb = PixelFormatInfo.Format32bppRgb,

        /// <summary>
        /// Represents a 32 bit per pixel color format where alpha, red, green and blue channels use 8 bits per pixel.
        /// </summary>
        Format32bppArgb = PixelFormatInfo.Format32bppArgb,

        /// <summary>
        /// Represents a 32 bit per pixel color format where alpha, red, green and blue channels use 8 bits per pixel.
        /// The red, green, and blue components are premultiplied, according to the alpha component.
        /// </summary>
        Format32bppPArgb = PixelFormatInfo.Format32bppPArgb,

        /// <summary>
        /// Represents a 48 bit per pixel color format where red, green and blue channels use 16 bits per pixel.
        /// </summary>
        Format48bppRgb = PixelFormatInfo.Format48bppRgb,

        /// <summary>
        /// Represents a 32 bit per pixel color format where alpha, red, green and blue channels use 16 bits per pixel.
        /// </summary>
        Format64bppArgb = PixelFormatInfo.Format64bppArgb,

        /// <summary>
        /// Represents a 32 bit per pixel color format where alpha, red, green and blue channels use 8 bits per pixel.
        /// The red, green, and blue components are premultiplied, according to the alpha component.
        /// </summary>
        Format64bppPArgb = PixelFormatInfo.Format64bppPArgb
    }
}
