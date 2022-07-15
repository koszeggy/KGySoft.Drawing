#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: PixelFormatExtensions.cs
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
#if NET7_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif
using System.Drawing;
using System.Drawing.Imaging;

using KGySoft.CoreLibraries;
using KGySoft.Drawing.Imaging;
using KGySoft.Drawing.WinApi;

#endregion

namespace KGySoft.Drawing
{
    /// <summary>
    /// Contains extension methods for the <see cref="PixelFormat"/> type.
    /// </summary>
#if NET7_0_OR_GREATER
    [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "These methods are supported on every platform.")]
#endif
    public static class PixelFormatExtensions
    {
        #region Constants

        internal const PixelFormat Format32bppCmyk = (PixelFormat)8207;

        #endregion

        #region Fields

        private static Dictionary<PixelFormat, bool>? supportedFormats;

        #endregion

        #region Methods

        #region Public Methods

        /// <summary>
        /// Gets the bits per pixel (BPP) value of a <see cref="PixelFormat"/> value without checking
        /// whether <paramref name="pixelFormat"/> represents a valid value.
        /// </summary>
        /// <param name="pixelFormat">The pixel format to convert.</param>
        /// <returns>The bits per pixel (BPP) value of a <see cref="PixelFormat"/> value.</returns>
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

            Dictionary<PixelFormat, bool>? map = supportedFormats;
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
            catch (Exception e) when (e is not StackOverflowException)
            {
                // catching even OutOfMemoryException because GDI can throw it for unsupported formats
                map = map == null ? new Dictionary<PixelFormat, bool>() : new Dictionary<PixelFormat, bool>(map);
                map[pixelFormat] = false;
                supportedFormats = map;
                return false;
            }
        }

        /// <summary>
        /// Converts a <see cref="KnownPixelFormat"/> to the closest <see cref="PixelFormat"/>. Please note that some formats with identical names may represent different actual pixel layout.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="ImageExtensions.ConvertPixelFormat(Image,PixelFormat,Color,byte)">ImageExtensions.ConvertPixelFormat</see>
        /// method for details about the differences on Windows and Unix platforms.
        /// </summary>
        /// <param name="pixelFormat">The source <see cref="KnownPixelFormat"/> to convert to a <see cref="PixelFormat"/>.</param>
        /// <returns>A <see cref="PixelFormat"/> instance. It will be <see cref="PixelFormat.Undefined"/> if the source <paramref name="pixelFormat"/> cannot be mapped.</returns>
        public static PixelFormat ToPixelFormat(this KnownPixelFormat pixelFormat)
        {
            if (pixelFormat == KnownPixelFormat.Undefined || !pixelFormat.IsDefined())
                return PixelFormat.Undefined;

            Debug.Assert(((int)pixelFormat & (int)PixelFormat.Max) != 0, "Unexpected known pixel format. Replace return expression to ((int)pixelFormat & (int)PixelFormat.Max) != 0 ? (PixelFormat)((int)pixelFormat & 0xFFFFFF) : pixelFormat switch {...}");
            return (PixelFormat)((int)pixelFormat & 0xFFFFFF); // direct mapping: just clearing 24..31 bits
        }

        /// <summary>
        /// Converts a <see cref="PixelFormat"/> to the closest <see cref="KnownPixelFormat"/>. Please note that some formats with identical names may represent different actual pixel layout.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="ImageExtensions.ConvertPixelFormat(Image,PixelFormat,Color,byte)">ImageExtensions.ConvertPixelFormat</see>
        /// method for details about the differences on Windows and Unix platforms.
        /// </summary>
        /// <param name="pixelFormat">The source <see cref="PixelFormat"/> to convert to a <see cref="KnownPixelFormat"/>.</param>
        /// <returns>A <see cref="KnownPixelFormat"/> instance. It will be <see cref="KnownPixelFormat.Undefined"/> if the source <paramref name="pixelFormat"/> cannot be mapped.</returns>
        public static KnownPixelFormat ToKnownPixelFormat(this PixelFormat pixelFormat)
            => !pixelFormat.IsValidFormat() ? KnownPixelFormat.Undefined : pixelFormat.ToKnownPixelFormatInternal();

        #endregion

        #region Internal Methods

        internal static bool IsIndexed(this PixelFormat pixelFormat)
            // ReSharper disable once BitwiseOperatorOnEnumWithoutFlags
            => (pixelFormat & PixelFormat.Indexed) == PixelFormat.Indexed;

        internal static bool HasAlpha(this PixelFormat pixelFormat)
            // ReSharper disable once BitwiseOperatorOnEnumWithoutFlags
            => (pixelFormat & PixelFormat.Alpha) == PixelFormat.Alpha;

        internal static KnownPixelFormat ToKnownPixelFormatInternal(this PixelFormat pixelFormat) => pixelFormat switch
        {
            // These formats use additional flags
            PixelFormat.Format16bppArgb1555 => KnownPixelFormat.Format16bppArgb1555,
            PixelFormat.Format16bppGrayScale => KnownPixelFormat.Format16bppGrayScale,
            _ => (KnownPixelFormat)pixelFormat
        };

        internal static bool CanBeDithered(this PixelFormat dstFormat)
            => dstFormat.ToBitsPerPixel() < 24 && dstFormat != PixelFormat.Format16bppGrayScale;

        internal static bool CanBeDrawn(this PixelFormat pixelFormat)
        {
            if (OSUtils.IsWindows)
                return pixelFormat != PixelFormat.Format16bppGrayScale;
            return pixelFormat is not PixelFormat.Format16bppRgb555 or PixelFormat.Format16bppRgb565
                && pixelFormat.IsSupportedNatively();
        }

        #endregion

        #endregion
    }
}
