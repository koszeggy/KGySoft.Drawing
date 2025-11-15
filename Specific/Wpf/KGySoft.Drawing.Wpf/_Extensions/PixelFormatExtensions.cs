#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: PixelFormatExtensions.cs
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

using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using KGySoft.CoreLibraries;
using KGySoft.Drawing.Imaging;

#endregion

namespace KGySoft.Drawing.Wpf
{
    /// <summary>
    /// Contains extension methods for the <see cref="PixelFormat"/> type.
    /// </summary>
    public static class PixelFormatExtensions
    {
        #region Fields

        private static readonly Color32[] indexedDefault2BppPalette = [Color32.FromGray(0), Color32.FromGray(0x80), Color32.FromGray(0xC0), Color32.FromGray(0xFF)];

        #endregion

        #region Methods

        #region Public Methods

        /// <summary>
        /// Gets a <see cref="PixelFormatInfo"/> for this <paramref name="pixelFormat"/>.
        /// </summary>
        /// <param name="pixelFormat">The <see cref="PixelFormat"/> to retrieve a <see cref="PixelFormatInfo"/> for.</param>
        /// <returns>A <see cref="PixelFormatInfo"/> that represents the specified <see cref="PixelFormat"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="pixelFormat"/> must be a non-default valid value.</exception>
        public static PixelFormatInfo GetInfo(this PixelFormat pixelFormat)
        {
            if (pixelFormat == default)
                throw new ArgumentOutOfRangeException(nameof(pixelFormat), PublicResources.ArgumentOutOfRange);

            KnownPixelFormat knownPixelFormat = pixelFormat.AsKnownPixelFormat();
            if (knownPixelFormat != KnownPixelFormat.Undefined)
                return new PixelFormatInfo(knownPixelFormat);

            var result = new PixelFormatInfo((byte)pixelFormat.BitsPerPixel);
            if (pixelFormat.IsIndexed())
                result.Indexed = true;
            else if (pixelFormat.In(PixelFormats.BlackWhite, PixelFormats.Gray2, PixelFormats.Gray4))
                result.Grayscale = true;
            else if (pixelFormat == PixelFormats.Rgba64)
                result.HasAlpha = true;
            else if (pixelFormat == PixelFormats.Prgba64)
                result.HasPremultipliedAlpha = true;

            if (pixelFormat.HasLinearGamma())
            {
                result.LinearGamma = true;
                result.Prefers128BitColors = true;
            }
            else if (pixelFormat.In(PixelFormats.Bgr101010, PixelFormats.Rgb48, PixelFormats.Rgba64, PixelFormats.Prgba64))
                result.Prefers64BitColors = true;

            return result;
        }

        /// <summary>
        /// Converts this <see cref="PixelFormat"/> to a compatible <see cref="KnownPixelFormat"/> value.
        /// </summary>
        /// <param name="pixelFormat">The <see cref="PixelFormat"/> to convert.</param>
        /// <returns>A <see cref="KnownPixelFormat"/> value that fits the to the specified <see cref="PixelFormat"/>.</returns>
        public static KnownPixelFormat ToKnownPixelFormat(this PixelFormat pixelFormat)
            => pixelFormat == default ? KnownPixelFormat.Undefined : pixelFormat.GetInfo().ToKnownPixelFormat();

        /// <summary>
        /// Gets whether this <see cref="PixelFormat"/> represents an indexed format.
        /// </summary>
        /// <param name="pixelFormat">The <see cref="PixelFormat"/> to check.</param>
        /// <returns><see langword="true"/>, if <paramref name="pixelFormat"/> represents an indexed format; otherwise, <see langword="false"/>.</returns>
        public static bool IsIndexed(this PixelFormat pixelFormat)
            => pixelFormat.In(PixelFormats.Indexed8, PixelFormats.Indexed4, PixelFormats.Indexed2, PixelFormats.Indexed1);

        /// <summary>
        /// Gets a <see cref="PredefinedColorsQuantizer"/> instance that fits for the specified <paramref name="pixelFormat"/>.
        /// For indexed formats a default palette will be used.
        /// </summary>
        /// <param name="pixelFormat">The <see cref="PixelFormat"/> to get a quantizer for.</param>
        /// <param name="backColor">Colors with alpha (transparency), which are considered opaque will be blended with this color before quantization.
        /// The <see cref="Color.A">Color.A</see> property of the background color is ignored. This parameter is optional.
        /// <br/>Default value: The bitwise zero instance of <see cref="Color"/>, which has the same RGB values as <see cref="Colors.Black"/>.</param>
        /// <param name="alphaThreshold">Specifies a threshold value for the <see cref="Color.A">Color.A</see> property,
        /// under which a quantized color is considered completely transparent. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <returns>A <see cref="PredefinedColorsQuantizer"/> instance that is compatible with the specified <paramref name="pixelFormat"/>.</returns>
        public static PredefinedColorsQuantizer GetMatchingQuantizer(this PixelFormat pixelFormat, Color backColor = default, byte alphaThreshold = 128)
            => (pixelFormat == PixelFormats.BlackWhite ? PredefinedColorsQuantizer.BlackAndWhite(backColor.ToColor32(), alphaThreshold)
                : pixelFormat == PixelFormats.Gray2 ? PredefinedColorsQuantizer.Grayscale4(backColor.ToColor32(), false, alphaThreshold)
                : pixelFormat == PixelFormats.Gray4 ? PredefinedColorsQuantizer.Grayscale16(backColor.ToColor32(), false, alphaThreshold)
                : pixelFormat == PixelFormats.Indexed1 ? PredefinedColorsQuantizer.SystemDefault1BppPalette(backColor.ToColor32(), alphaThreshold)
                : pixelFormat == PixelFormats.Indexed2 ? PredefinedColorsQuantizer.FromCustomPalette(new Palette(indexedDefault2BppPalette, backColor.ToColor32(), alphaThreshold))
                : pixelFormat == PixelFormats.Indexed4 ? PredefinedColorsQuantizer.SystemDefault4BppPalette(backColor.ToColor32(), alphaThreshold)
                : pixelFormat == PixelFormats.Indexed8 ? PredefinedColorsQuantizer.SystemDefault8BppPalette(backColor.ToColor32(), alphaThreshold)
                : pixelFormat.In(PixelFormats.Gray8, PixelFormats.Gray16, PixelFormats.Gray32Float) ? PredefinedColorsQuantizer.Grayscale(backColor.ToColor32(), alphaThreshold)
                : PredefinedColorsQuantizer.FromPixelFormat(pixelFormat.ToKnownPixelFormat(), backColor.ToColor32(), alphaThreshold))
                .ConfigureColorSpace(pixelFormat.HasLinearGamma() ? WorkingColorSpace.Linear : WorkingColorSpace.Default);

        /// <summary>
        /// Converts a <see cref="KnownPixelFormat"/> to the closest <see cref="PixelFormat"/>.
        /// </summary>
        /// <param name="pixelFormat">The source <see cref="KnownPixelFormat"/> to convert to a <see cref="PixelFormat"/>.</param>
        /// <returns>A <see cref="PixelFormat"/> instance. It will be <see cref="PixelFormats.Default"/> value if the source <paramref name="pixelFormat"/> cannot be mapped.</returns>
        public static PixelFormat ToPixelFormat(this KnownPixelFormat pixelFormat) => pixelFormat switch
        {
            KnownPixelFormat.Format1bppIndexed => PixelFormats.Indexed1,
            KnownPixelFormat.Format4bppIndexed => PixelFormats.Indexed4,
            KnownPixelFormat.Format8bppIndexed => PixelFormats.Indexed8,
            KnownPixelFormat.Format8bppGrayScale => PixelFormats.Gray8,
            KnownPixelFormat.Format16bppGrayScale => PixelFormats.Gray16,
            KnownPixelFormat.Format16bppRgb555 => PixelFormats.Bgr555,
            KnownPixelFormat.Format16bppRgb565 => PixelFormats.Bgr565,
            KnownPixelFormat.Format16bppArgb1555 => PixelFormats.Bgra32,
            KnownPixelFormat.Format24bppRgb => PixelFormats.Bgr24,
            KnownPixelFormat.Format32bppRgb => PixelFormats.Bgr32,
            KnownPixelFormat.Format32bppArgb => PixelFormats.Bgra32,
            KnownPixelFormat.Format32bppPArgb => PixelFormats.Pbgra32,
            KnownPixelFormat.Format32bppGrayScale => PixelFormats.Gray32Float,
            KnownPixelFormat.Format48bppRgb => PixelFormats.Rgb48,
            KnownPixelFormat.Format64bppArgb => PixelFormats.Rgba64,
            KnownPixelFormat.Format64bppPArgb => PixelFormats.Prgba64,
            KnownPixelFormat.Format96bppRgb => PixelFormats.Rgb128Float,
            KnownPixelFormat.Format128bppRgba => PixelFormats.Rgba128Float,
            KnownPixelFormat.Format128bppPRgba => PixelFormats.Prgba128Float,
            _ => PixelFormats.Default
        };

        #endregion

        #region Internal Methods

        internal static KnownPixelFormat AsKnownPixelFormat(this PixelFormat pixelFormat)
            => pixelFormat == PixelFormats.Bgra32 ? KnownPixelFormat.Format32bppArgb
             : pixelFormat == PixelFormats.Pbgra32 ? KnownPixelFormat.Format32bppPArgb
             : pixelFormat == PixelFormats.Bgr32 ? KnownPixelFormat.Format32bppRgb
             : pixelFormat == PixelFormats.Bgr24 ? KnownPixelFormat.Format24bppRgb
             : pixelFormat == PixelFormats.Indexed1 ? KnownPixelFormat.Format1bppIndexed
             : pixelFormat == PixelFormats.Indexed4 ? KnownPixelFormat.Format4bppIndexed
             : pixelFormat == PixelFormats.Indexed8 ? KnownPixelFormat.Format8bppIndexed
             : pixelFormat == PixelFormats.Bgr555 ? KnownPixelFormat.Format16bppRgb555
             : pixelFormat == PixelFormats.Bgr565 ? KnownPixelFormat.Format16bppRgb565
             : pixelFormat == PixelFormats.Gray8 ? KnownPixelFormat.Format8bppGrayScale
             : pixelFormat == PixelFormats.Gray16 ? KnownPixelFormat.Format16bppGrayScale
             : pixelFormat == PixelFormats.Gray32Float ? KnownPixelFormat.Format32bppGrayScale
             : pixelFormat == PixelFormats.Rgba128Float ? KnownPixelFormat.Format128bppRgba
             : pixelFormat == PixelFormats.Prgba128Float ? KnownPixelFormat.Format128bppPRgba
             : KnownPixelFormat.Undefined;

        internal static bool CanBeDithered(this PixelFormat dstFormat)
            => dstFormat.BitsPerPixel < 24 && !dstFormat.In(PixelFormats.Gray16, PixelFormats.Gray8);

        internal static BitmapPalette? GetDefaultPalette(this PixelFormat pixelFormat)
        {
            var result = pixelFormat == PixelFormats.Indexed1 ? Palette.BlackAndWhite()
                : pixelFormat == PixelFormats.Indexed2 ? new Palette(indexedDefault2BppPalette)
                : pixelFormat == PixelFormats.Indexed4 ? Palette.SystemDefault4BppPalette()
                : pixelFormat == PixelFormats.Indexed8 ? Palette.SystemDefault8BppPalette()
                : null;
            return result.ToBitmapPalette();
        }

        #endregion

        #region Private Methods

        private static bool HasLinearGamma(this PixelFormat pixelFormat)
            => pixelFormat.In(PixelFormats.Gray32Float, PixelFormats.Rgb128Float, PixelFormats.Rgba128Float, PixelFormats.Rgba128Float);

        #endregion

        #endregion
    }
}