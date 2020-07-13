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
    public static class PixelFormatExtensions
    {
        #region Fields

        private static Dictionary<PixelFormat, bool> supportedFormats;

        #endregion

        #region Methods

        #region Public Methods

        /// <summary>
        /// Gets the bits per pixel (BPP) value of a <see cref="PixelFormat"/> value.
        /// </summary>
        /// <param name="pixelFormat">The pixel format to convert.</param>
        /// <returns>The bits per pixel (BPP) value of a <see cref="PixelFormat"/> value.</returns>
        /// <remarks>
        /// <note>This method does not check whether the specified <paramref name="pixelFormat"/> represents a valid value.</note>
        /// </remarks>
        public static int ToBitsPerPixel(this PixelFormat pixelFormat) => ((int)pixelFormat >> 8) & 0xFF;

        /// <summary>
        /// Gets whether this <see cref="PixelFormat"/> instance represents a valid format.
        /// The valid format values are the ones, whose name starts with <c>Format</c>.
        /// </summary>
        /// <param name="pixelFormat">The pixel format to be checked.</param>
        /// <returns><see langword="true"/>, if this <see cref="PixelFormat"/> instance represents a valid format; otherwise, <see langword="false"/>.</returns>
        public static bool IsValidFormat(this PixelFormat pixelFormat)
            // ReSharper disable once BitwiseOperatorOnEnumWithoutFlags
            => pixelFormat != PixelFormat.Max && (pixelFormat & PixelFormat.Max) != 0 && pixelFormat.IsDefined();

        /// <summary>
        /// Gets whether the specified <paramref name="pixelFormat"/> is supported natively on the current operating system.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="pixelFormat">The pixel format to check.</param>
        /// <returns><see langword="true"/>, if the specified <paramref name="pixelFormat"/> is supported on the current operating system; otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>If <paramref name="pixelFormat"/> does not represent a valid format (the <see cref="IsValidFormat">IsValidFormat</see> method returns <see langword="false"/>), then
        /// this method returns <see langword="false"/>.</para>
        /// <para>This method returns <see langword="true"/>, if a <see cref="Bitmap"/> can be created with the specified <paramref name="pixelFormat"/>.
        /// Even in such case there might be some limitations on the current operating system when using some <see cref="PixelFormat"/>s.</para>
        /// <note>For information about the possible usable <see cref="PixelFormat"/>s on different platforms see the <strong>Remarks</strong>
        /// section of the <see cref="ImageExtensions.ConvertPixelFormat(Image,PixelFormat,Color,byte)">ConvertPixelFormat</see> extension method.</note>
        /// </remarks>
        public static bool IsSupportedNatively(this PixelFormat pixelFormat)
        {
            if (!pixelFormat.IsValidFormat())
                return false;

            if (OSUtils.IsWindows)
                return true;

            Dictionary<PixelFormat, bool> map = supportedFormats;
            if (map != null && map.TryGetValue(pixelFormat, out bool result))
                return result;

            // Format is not in the dictionary yet: we check if the format can be created and replace the dictionary (so no locking is needed)
            try
            {
                using var _ = new Bitmap(1, 1, pixelFormat);
                map = map == null ? new Dictionary<PixelFormat, bool>() : new Dictionary<PixelFormat, bool>(map);
                map[pixelFormat] = true;
                supportedFormats = map;
                return true;
            }
            catch (Exception e) when (!(e is StackOverflowException))
            {
                // catching even OutOfMemoryException because GDI can throw it for unsupported formats
                map = map == null ? new Dictionary<PixelFormat, bool>() : new Dictionary<PixelFormat, bool>(map);
                map[pixelFormat] = false;
                supportedFormats = map;
                return false;
            }
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Gets whether this <see cref="PixelFormat"/> instance represents an indexed format.
        /// </summary>
        /// <param name="pixelFormat">The pixel format to be checked.</param>
        /// <returns><see langword="true"/>, if this <see cref="PixelFormat"/> instance represents an indexed format; otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// <note>This method does not check whether the specified <paramref name="pixelFormat"/> represents a valid value.</note>
        /// </remarks>
        internal static bool IsIndexed(this PixelFormat pixelFormat)
            // ReSharper disable once BitwiseOperatorOnEnumWithoutFlags
            => (pixelFormat & PixelFormat.Indexed) == PixelFormat.Indexed;

        internal static bool HasAlpha(this PixelFormat pixelFormat)
            // ReSharper disable once BitwiseOperatorOnEnumWithoutFlags
            => (pixelFormat & PixelFormat.Alpha) == PixelFormat.Alpha;

        internal static bool CanBeDithered(this PixelFormat dstFormat)
            => dstFormat.ToBitsPerPixel() <= 16 && dstFormat != PixelFormat.Format16bppGrayScale;

        internal static bool CanBeDrawn(this PixelFormat pixelFormat)
        {
            if (OSUtils.IsWindows)
                return pixelFormat != PixelFormat.Format16bppGrayScale;
            return !pixelFormat.In(PixelFormat.Format16bppRgb555, PixelFormat.Format16bppRgb565) && pixelFormat.IsSupportedNatively();
        }

        #endregion

        #endregion
    }
}
