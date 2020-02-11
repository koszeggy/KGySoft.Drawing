#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: PixelFormatExtensions.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2019 - All Rights Reserved
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
using System.Drawing.Imaging;

#endregion

namespace KGySoft.Drawing
{
    /// <summary>
    /// Contains extension methods for the <see cref="PixelFormat"/> type.
    /// </summary>
    internal static class PixelFormatExtensions
    {
        #region Methods

        /// <summary>
        /// Gets the bits per pixel (bpp) value of a <see cref="PixelFormat"/> value.
        /// </summary>
        internal static int ToBitsPerPixel(this PixelFormat pixelFormat)
        {
            switch (pixelFormat)
            {
                case PixelFormat.Format1bppIndexed:
                    return 1;
                case PixelFormat.Format4bppIndexed:
                    return 4;
                case PixelFormat.Format8bppIndexed:
                    return 8;
                case PixelFormat.Format16bppArgb1555:
                case PixelFormat.Format16bppGrayScale:
                case PixelFormat.Format16bppRgb555:
                case PixelFormat.Format16bppRgb565:
                    return 16;
                case PixelFormat.Format24bppRgb:
                    return 24;
                case PixelFormat.Format32bppArgb:
                case PixelFormat.Format32bppPArgb:
                case PixelFormat.Format32bppRgb:
                    return 32;
                case PixelFormat.Format48bppRgb:
                    return 48;
                case PixelFormat.Format64bppArgb:
                case PixelFormat.Format64bppPArgb:
                    return 64;
                default:
                    throw new ArgumentOutOfRangeException(nameof(pixelFormat), PublicResources.EnumOutOfRange(pixelFormat));
            }
        }

        internal static bool HasTransparency(this PixelFormat pixelFormat)
            // ReSharper disable once BitwiseOperatorOnEnumWithoutFlags
            => (pixelFormat & PixelFormat.Alpha) == PixelFormat.Alpha;

        internal static bool IsIndexed(this PixelFormat pixelFormat)
            // ReSharper disable once BitwiseOperatorOnEnumWithoutFlags
            => (pixelFormat & PixelFormat.Indexed) == PixelFormat.Indexed;

        internal static bool CanBeDithered(this PixelFormat dstFormat)
            => dstFormat.ToBitsPerPixel() <= 16 && dstFormat != PixelFormat.Format16bppGrayScale;

        internal static PixelFormat ToPixelFormat(this int bpp)
        {
            switch (bpp)
            {
                case 1:
                    return PixelFormat.Format1bppIndexed;
                case 4:
                    return PixelFormat.Format4bppIndexed;
                case 8:
                    return PixelFormat.Format8bppIndexed;
                case 16:
                    return PixelFormat.Format16bppRgb565;
                case 24:
                    return PixelFormat.Format24bppRgb;
                case 32:
                    return PixelFormat.Format32bppArgb;
                case 48:
                    return PixelFormat.Format48bppRgb;
                case 64:
                    return PixelFormat.Format64bppArgb;
                default:
                    throw new ArgumentOutOfRangeException(nameof(bpp), PublicResources.ArgumentOutOfRange);
            }
        }

        #endregion
    }
}
