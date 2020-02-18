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
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

using KGySoft.CoreLibraries;
using KGySoft.Drawing.WinApi;

#endregion

namespace KGySoft.Drawing
{
    /// <summary>
    /// Contains extension methods for the <see cref="PixelFormat"/> type.
    /// </summary>
    internal static class PixelFormatExtensions
    {
        #region Fields

        private static Dictionary<PixelFormat, bool> supportedFormats;

        #endregion

        #region Methods

        /// <summary>
        /// Gets the bits per pixel (bpp) value of a <see cref="PixelFormat"/> value.
        /// </summary>
        internal static int ToBitsPerPixel(this PixelFormat pixelFormat) => ((int)pixelFormat >> 8) & 0xFF;

        internal static bool HasTransparency(this PixelFormat pixelFormat)
            // ReSharper disable once BitwiseOperatorOnEnumWithoutFlags
            => (pixelFormat & PixelFormat.Alpha) == PixelFormat.Alpha;

        internal static bool IsIndexed(this PixelFormat pixelFormat)
            // ReSharper disable once BitwiseOperatorOnEnumWithoutFlags
            => (pixelFormat & PixelFormat.Indexed) == PixelFormat.Indexed;

        internal static bool CanBeDithered(this PixelFormat dstFormat)
            => dstFormat.ToBitsPerPixel() <= 16 && dstFormat != PixelFormat.Format16bppGrayScale;

        internal static bool IsValidFormat(this PixelFormat pixelFormat)
            // ReSharper disable once BitwiseOperatorOnEnumWithoutFlags
            => pixelFormat.IsDefined() && (pixelFormat & PixelFormat.Max) != 0;

        internal static bool IsSupported(this PixelFormat pixelFormat)
        {
            if (OSUtils.IsWindows)
                return true;

            Dictionary<PixelFormat, bool> map = supportedFormats;
            if (map != null && map.TryGetValue(pixelFormat, out bool result))
                return result;

            // Format is not in the dictionary yet: we check if the format can be created and replace the dictionary (so no locking is needed)
            try
            {
                using var _ = new Bitmap(1, 1, pixelFormat);
                map = new Dictionary<PixelFormat, bool>(map);
                map[pixelFormat] = true;
                supportedFormats = map;
                return true;
            }
            catch (Exception e) when (!(e is StackOverflowException))
            {
                // catching even OutOfMemoryException because GDI can throw it for unsupported formats
                map = new Dictionary<PixelFormat, bool>(map);
                map[pixelFormat] = false;
                supportedFormats = map;
                return false;
            }
        }

        internal static bool CanBeDrawn(this PixelFormat pixelFormat)
        {
            if (OSUtils.IsWindows)
                return pixelFormat != PixelFormat.Format16bppGrayScale;
            return !pixelFormat.In(PixelFormat.Format16bppRgb555, PixelFormat.Format16bppRgb565) && pixelFormat.IsSupported();
        }

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
