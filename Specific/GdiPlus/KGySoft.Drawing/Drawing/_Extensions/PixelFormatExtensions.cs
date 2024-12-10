#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: PixelFormatExtensions.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2024 - All Rights Reserved
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

        internal const PixelFormat Format32bppCmyk = (PixelFormat)0x200F;

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
        /// Gets whether this <see cref="PixelFormat"/> instance represents an indexed format without checking
        /// whether <paramref name="pixelFormat"/> represents a valid value.
        /// </summary>
        /// <param name="pixelFormat">The pixel format to be checked.</param>
        /// <returns><see langword="true"/>, if this <see cref="PixelFormat"/> instance represents an indexed format; otherwise, <see langword="false"/>.</returns>
        public static bool IsIndexed(this PixelFormat pixelFormat)
            // ReSharper disable once BitwiseOperatorOnEnumWithoutFlags
            => (pixelFormat & PixelFormat.Indexed) == PixelFormat.Indexed;

        /// <summary>
        /// Gets whether the specified <paramref name="pixelFormat"/> is supported natively on the current operating system.
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

            if (((int)pixelFormat & 0xFF) < (int)PixelFormat.Max)
                return (PixelFormat)((int)pixelFormat & 0xFFFFFF); // direct mapping: just clearing 24..31 bits

            return pixelFormat switch
            {
                KnownPixelFormat.Format8bppGrayScale or KnownPixelFormat.Format32bppGrayScale => PixelFormat.Format16bppGrayScale,
                KnownPixelFormat.Format96bppRgb => PixelFormat.Format48bppRgb,
                KnownPixelFormat.Format128bppRgba => PixelFormat.Format64bppArgb,
                KnownPixelFormat.Format128bppPRgba => PixelFormat.Format64bppPArgb,
                _ => PixelFormat.Undefined
            };
        }

        /// <summary>
        /// Converts a <see cref="PixelFormat"/> to the closest <see cref="KnownPixelFormat"/>. Please note that some formats with identical names may represent different actual pixel layout.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="ImageExtensions.ConvertPixelFormat(Image,PixelFormat,Color,byte)">ImageExtensions.ConvertPixelFormat</see>
        /// method for details about the differences on Windows and Unix platforms.
        /// </summary>
        /// <param name="pixelFormat">The source <see cref="PixelFormat"/> to convert to a <see cref="KnownPixelFormat"/>.</param>
        /// <returns>A <see cref="KnownPixelFormat"/> instance. It will be <see cref="KnownPixelFormat.Undefined"/> if the source <paramref name="pixelFormat"/> cannot be mapped.</returns>
        public static KnownPixelFormat ToKnownPixelFormat(this PixelFormat pixelFormat)
            => pixelFormat == Format32bppCmyk ? KnownPixelFormat.Format24bppRgb
                : !pixelFormat.IsValidFormat() ? KnownPixelFormat.Undefined
                : pixelFormat.ToKnownPixelFormatInternal();

        /// <summary>
        /// Gets a <see cref="PixelFormatInfo"/> for this <paramref name="pixelFormat"/>.
        /// Please note that this may return a different result than calling <see cref="ToKnownPixelFormat">ToKnownPixelFormat()</see>.<see cref="KnownPixelFormatExtensions.GetInfo">GetInfo()</see>
        /// because the <see cref="PixelFormatInfo.IsCustomFormat"/> in the result of this method can be <see langword="true"/> if the actual pixel layout
        /// of the specified <paramref name="pixelFormat"/> differs from the layout of its <see cref="KnownPixelFormat"/> counterpart with the same name.
        /// </summary>
        /// <param name="pixelFormat">The <see cref="PixelFormat"/> to retrieve a <see cref="PixelFormatInfo"/> for.</param>
        /// <returns>A <see cref="PixelFormatInfo"/> representing the specified <paramref name="pixelFormat"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="pixelFormat"/> must be a valid format.</exception>
        public static PixelFormatInfo GetInfo(this PixelFormat pixelFormat) => pixelFormat switch
        {
            PixelFormat.Format48bppRgb => ColorsHelper.LinearWideColors
                ? new PixelFormatInfo(48) { LinearGamma = true, Prefers128BitColors = true }
                : new PixelFormatInfo(KnownPixelFormat.Format48bppRgb),
            PixelFormat.Format64bppArgb => ColorsHelper.LinearWideColors
                ? new PixelFormatInfo(64) { HasAlpha = true, LinearGamma = true, Prefers128BitColors = true }
                : new PixelFormatInfo(KnownPixelFormat.Format64bppArgb),
            PixelFormat.Format64bppPArgb => ColorsHelper.LinearWideColors
                ? new PixelFormatInfo(64) { HasPremultipliedAlpha = true, LinearGamma = true, Prefers128BitColors = true }
                : new PixelFormatInfo(KnownPixelFormat.Format64bppPArgb),
            Format32bppCmyk => new PixelFormatInfo(32),
            _ => pixelFormat.IsValidFormat() ? pixelFormat.ToKnownPixelFormatInternal().GetInfo() : throw new ArgumentOutOfRangeException(Res.PixelFormatInvalid(pixelFormat))
        };

        /// <summary>
        /// Gets a <see cref="PredefinedColorsQuantizer"/> instance that fits for the specified <paramref name="pixelFormat"/>.
        /// For indexed formats a default palette will be used.
        /// </summary>
        /// <param name="pixelFormat">The <see cref="PixelFormat"/> to get a quantizer for.</param>
        /// <param name="backColor">Colors with alpha (transparency), which are considered opaque will be blended with this color before quantization.
        /// The <see cref="Color.A">Color.A</see> property of the background color is ignored. This parameter is optional.
        /// <br/>Default value: The bitwise zero instance of <see cref="Color"/>, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="alphaThreshold">Specifies a threshold value for the <see cref="Color.A">Color.A</see> property,
        /// under which a quantized color is considered completely transparent. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <returns>A <see cref="PredefinedColorsQuantizer"/> instance that is compatible with the specified <paramref name="pixelFormat"/>.</returns>
        public static PredefinedColorsQuantizer GetMatchingQuantizer(this PixelFormat pixelFormat, Color backColor = default, byte alphaThreshold = 128)
            => PredefinedColorsQuantizer.FromPixelFormat(pixelFormat.ToKnownPixelFormat(), backColor.ToColor32(), alphaThreshold)
                .ConfigureColorSpace(pixelFormat.HasLinearGamma() ? WorkingColorSpace.Linear : WorkingColorSpace.Default);

        #endregion

        #region Internal Methods

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
            return pixelFormat is not (PixelFormat.Format16bppRgb555 or PixelFormat.Format16bppRgb565)
                && pixelFormat.IsSupportedNatively();
        }

        #endregion

        #region Private Methods

        private static bool HasLinearGamma(this PixelFormat pixelFormat)
            => (pixelFormat is PixelFormat.Format48bppRgb or PixelFormat.Format64bppArgb or PixelFormat.Format64bppPArgb)
                && ColorsHelper.GetLookupTableSrgb8ToLinear16Bit() != null;

        #endregion

        #endregion
    }
}
