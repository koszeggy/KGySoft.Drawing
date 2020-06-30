#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BitmapDataFactory.cs
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
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

using KGySoft.CoreLibraries;
using KGySoft.Drawing.WinApi;

#endregion

namespace KGySoft.Drawing.Imaging
{
    public static class BitmapDataFactory
    {
        #region Methods

        #region Public Methods

        /// <summary>
        /// Creates a managed <see cref="IReadWriteBitmapData"/> with the specified <paramref name="size"/> and <paramref name="pixelFormat"/>.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="size"/> or <paramref name="pixelFormat"/> has an invalid value.</exception>
        /// <exception cref="ArgumentException"><paramref name="palette"/> contains too many colors for the indexed format specified by <paramref name="pixelFormat"/>.</exception>
        /// <param name="size">The size of the bitmap data to create.</param>
        /// <param name="pixelFormat">The desired pixel format of the bitmap data to create.</param>
        /// <param name="backColor">Specifies the <see cref="IBitmapData.BackColor"/> value of the returned <see cref="IReadWriteBitmapData"/> instance. It does not affect the actual returned bitmap content.
        /// See the <strong>Remarks</strong> section for details. The alpha value (<see cref="Color32.A">Color32.A</see> field) of the specified background color is ignored. This parameter is optional.
        /// <br/>Default value: The default value of the <see cref="Color32"/> type, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="alphaThreshold">Specifies the <see cref="IBitmapData.AlphaThreshold"/> value of the returned <see cref="IReadWriteBitmapData"/> instance.
        /// See the <strong>Remarks</strong> section for details. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <param name="palette">Specifies the desired <see cref="IBitmapData.Palette"/> of the returned <see cref="IReadWriteBitmapData"/> instance.</param>
        /// <returns>A managed <see cref="IReadWriteBitmapData"/> with the specified <paramref name="size"/> and <paramref name="pixelFormat"/>.</returns>
        /// <remarks>
        /// <para>All possible <see cref="PixelFormat"/>s are supported, regardless of the native <see cref="Bitmap"/> support of the current operating system.
        /// <note>When <paramref name="pixelFormat"/> specifies a wide-color format (48/64 bit or 16 bit grayscale), then the returned instance will use the full 16-bit range of the color channels.
        /// This means a different raw content to Windows' wide-color <see cref="Bitmap"/> instances, which use 13-bit channels. But this difference is transparent in most cases
        /// unless we access actual raw content by the <see cref="IReadableBitmapDataRow.ReadRaw{T}">ReadRaw</see> and <see cref="IWritableBitmapDataRow.WriteRaw{T}">WriteRaw</see> methods.</note></para>
        /// <para>The <paramref name="backColor"/> parameter has no effect if <paramref name="pixelFormat"/> has alpha gradient support and it does not affect the actual content of the returned instance.
        /// To set all pixels to a color use the <see cref="O:KGySoft.Drawing.Imaging.WritableBitmapDataExtensions.Clear">Clear</see> extension methods.</para>
        /// <para>If <paramref name="alphaThreshold"/> is zero, then setting a fully transparent pixel in a bitmap data with indexed or single-bit-alpha pixel format
        /// will blend the pixel to set with <paramref name="backColor"/> even if the bitmap data can handle transparent pixels.</para>
        /// <para>If <paramref name="alphaThreshold"/> is <c>1</c>, then the result color of setting a pixel of a bitmap data with indexed or single-bit-alpha pixel format
        /// will be transparent only if the color to set is completely transparent (has zero alpha).</para>
        /// <para>If <paramref name="alphaThreshold"/> is <c>255</c>, then the result color of setting a pixel of a bitmap data with indexed or single-bit-alpha pixel format
        /// will be opaque only if the color to set is completely opaque (its alpha value is <c>255</c>).</para>
        /// <para>If a pixel of a bitmap data without alpha gradient support is set by the <see cref="IWritableBitmapData.SetPixel">IWritableBitmapData.SetPixel</see>/<see cref="IWritableBitmapDataRow.SetColor">IWritableBitmapDataRow.SetColor</see>
        /// methods or by the <see cref="IReadWriteBitmapDataRow.this">IReadWriteBitmapDataRow indexer</see>, and the pixel has an alpha value that is greater than <paramref name="alphaThreshold"/>,
        /// then the pixel to set will be blended with <paramref name="backColor"/>.</para>
        /// </remarks>
        public static IReadWriteBitmapData CreateBitmapData(Size size, PixelFormat pixelFormat = PixelFormat.Format32bppArgb, Color32 backColor = default, byte alphaThreshold = 128, Palette palette = null)
        {
            if (size.Width < 1 || size.Height < 1)
                throw new ArgumentOutOfRangeException(nameof(size), PublicResources.ArgumentOutOfRange);
            if (!pixelFormat.IsValidFormat())
                throw new ArgumentOutOfRangeException(nameof(pixelFormat), Res.PixelFormatInvalid(pixelFormat));
            if (pixelFormat.IsIndexed() && palette != null)
            {
                int maxColors = 1 << pixelFormat.ToBitsPerPixel();
                if (palette.Count > maxColors)
                    throw new ArgumentException(Res.ImagingPaletteTooLarge(maxColors, pixelFormat), nameof(palette));
            }

            return CreateManagedBitmapData(size, pixelFormat, backColor, alphaThreshold, palette);
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Creates a native <see cref="IBitmapDataInternal"/> from a <see cref="Bitmap"/>.
        /// </summary>
        internal static IBitmapDataInternal CreateBitmapData(Bitmap bitmap, ImageLockMode lockMode, Color32 backColor = default, byte alphaThreshold = 128)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap), PublicResources.ArgumentNull);
            if (!lockMode.IsDefined())
                throw new ArgumentOutOfRangeException(nameof(lockMode), PublicResources.EnumOutOfRange(lockMode));

            var pixelFormat = bitmap.PixelFormat;
            switch (pixelFormat)
            {
                case PixelFormat.Format32bppArgb:
                    return new NativeBitmapData<NativeBitmapDataRow32Argb>(bitmap, pixelFormat, lockMode, backColor, alphaThreshold);

                case PixelFormat.Format32bppPArgb:
                    return new NativeBitmapData<NativeBitmapDataRow32PArgb>(bitmap, pixelFormat, lockMode, backColor, alphaThreshold);

                case PixelFormat.Format32bppRgb:
                    return new NativeBitmapData<NativeBitmapDataRow32Rgb>(bitmap, pixelFormat, lockMode, backColor, alphaThreshold);

                case PixelFormat.Format24bppRgb:
                    return new NativeBitmapData<NativeBitmapDataRow24Rgb>(bitmap, pixelFormat, lockMode, backColor, alphaThreshold);

                case PixelFormat.Format8bppIndexed:
                    return new NativeBitmapData<NativeBitmapDataRow8I>(bitmap, pixelFormat, lockMode, backColor, alphaThreshold);

                case PixelFormat.Format4bppIndexed:
                    return new NativeBitmapData<NativeBitmapDataRow4I>(bitmap, pixelFormat, lockMode, backColor, alphaThreshold);

                case PixelFormat.Format1bppIndexed:
                    return new NativeBitmapData<NativeBitmapDataRow1I>(bitmap, pixelFormat, lockMode, backColor, alphaThreshold);

                case PixelFormat.Format64bppArgb:
                    return new NativeBitmapData<NativeBitmapDataRow64Argb>(bitmap, pixelFormat, lockMode, backColor, alphaThreshold);

                case PixelFormat.Format64bppPArgb:
                    return new NativeBitmapData<NativeBitmapDataRow64PArgb>(bitmap, pixelFormat, lockMode, backColor, alphaThreshold);

                case PixelFormat.Format48bppRgb:
                    return new NativeBitmapData<NativeBitmapDataRow48Rgb>(bitmap, pixelFormat, lockMode, backColor, alphaThreshold);

                case PixelFormat.Format16bppRgb565:
                    return OSUtils.IsWindows
                        ? (NativeBitmapDataBase)new NativeBitmapData<NativeBitmapDataRow16Rgb565>(bitmap, pixelFormat, lockMode, backColor, alphaThreshold)
                        : new NativeBitmapData<NativeBitmapDataRow16Rgb565Via24Bpp>(bitmap, PixelFormat.Format24bppRgb, lockMode, backColor, alphaThreshold);

                case PixelFormat.Format16bppRgb555:
                    return OSUtils.IsWindows
                        ? (NativeBitmapDataBase)new NativeBitmapData<NativeBitmapDataRow16Rgb555>(bitmap, pixelFormat, lockMode, backColor, alphaThreshold)
                        : new NativeBitmapData<NativeBitmapDataRow16Rgb555Via24Bpp>(bitmap, PixelFormat.Format24bppRgb, lockMode, backColor, alphaThreshold);

                case PixelFormat.Format16bppArgb1555:
                    return new NativeBitmapData<NativeBitmapDataRow16Argb1555>(bitmap, pixelFormat, lockMode, backColor, alphaThreshold);

                case PixelFormat.Format16bppGrayScale:
                    return new NativeBitmapData<NativeBitmapDataRow16Gray>(bitmap, pixelFormat, lockMode, backColor, alphaThreshold);

                default:
                    throw new InvalidOperationException(Res.InternalError($"Unexpected pixel format {pixelFormat}"));
            }
        }

        /// <summary>
        /// Creates a native <see cref="IBitmapDataInternal"/> by a quantizer session re-using its palette if possible.
        /// </summary>
        internal static IBitmapDataInternal CreateBitmapData(Bitmap bitmap, ImageLockMode lockMode, IQuantizingSession quantizingSession)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap), PublicResources.ArgumentNull);

            var pixelFormat = bitmap.PixelFormat;
            if (!pixelFormat.IsIndexed() || quantizingSession.Palette == null)
                return CreateBitmapData(bitmap, lockMode, quantizingSession.BackColor, quantizingSession.AlphaThreshold);

            // checking if bitmap and quantizer palette has the same entries
            var bmpPalette = bitmap.Palette.Entries;
            var quantizerPalette = quantizingSession.Palette.Entries;
            if (bmpPalette.Length != quantizerPalette.Length || bmpPalette.Zip(quantizerPalette, (c1, c2) => new Color32(c1) != c2).Any(b => b))
                return CreateBitmapData(bitmap, lockMode, quantizingSession.BackColor, quantizingSession.AlphaThreshold);

            if (!lockMode.IsDefined())
                throw new ArgumentOutOfRangeException(nameof(lockMode), PublicResources.EnumOutOfRange(lockMode));

            // here the quantizer and the target bitmap uses the same palette
            switch (pixelFormat)
            {
                case PixelFormat.Format8bppIndexed:
                    return new NativeBitmapData<NativeBitmapDataRow8I>(bitmap, pixelFormat, lockMode, quantizingSession);

                case PixelFormat.Format4bppIndexed:
                    return new NativeBitmapData<NativeBitmapDataRow4I>(bitmap, pixelFormat, lockMode, quantizingSession);

                case PixelFormat.Format1bppIndexed:
                    return new NativeBitmapData<NativeBitmapDataRow1I>(bitmap, pixelFormat, lockMode, quantizingSession);

                default:
                    throw new InvalidOperationException(Res.InternalError($"Unexpected indexed format: {pixelFormat}"));
            }
        }

        /// <summary>
        /// Creates a managed <see cref="IBitmapDataInternal"/> with the specified <paramref name="size"/> and <paramref name="pixelFormat"/>.
        /// </summary>
        internal static IBitmapDataInternal CreateManagedBitmapData(Size size, PixelFormat pixelFormat = PixelFormat.Format32bppArgb, Color32 backColor = default, byte alphaThreshold = 128, Palette palette = null)
        {
            switch (pixelFormat)
            {
                case PixelFormat.Format32bppArgb:
                    return new ManagedBitmapData<Color32, ManagedBitmapDataRow32Argb>(size, pixelFormat, backColor, alphaThreshold);

                case PixelFormat.Format32bppPArgb:
                    return new ManagedBitmapData<Color32, ManagedBitmapDataRow32PArgb>(size, pixelFormat, backColor, alphaThreshold);

                case PixelFormat.Format32bppRgb:
                    return new ManagedBitmapData<Color32, ManagedBitmapDataRow32Rgb>(size, pixelFormat, backColor, alphaThreshold);

                case PixelFormat.Format24bppRgb:
                    return new ManagedBitmapData<Color24, ManagedBitmapDataRow24Rgb>(size, pixelFormat, backColor, alphaThreshold);

                case PixelFormat.Format8bppIndexed:
                    return new ManagedBitmapData<byte, ManagedBitmapDataRow8I>(size, pixelFormat, backColor, alphaThreshold, palette);

                case PixelFormat.Format4bppIndexed:
                    return new ManagedBitmapData<byte, ManagedBitmapDataRow4I>(size, pixelFormat, backColor, alphaThreshold, palette);

                case PixelFormat.Format1bppIndexed:
                    return new ManagedBitmapData<byte, ManagedBitmapDataRow1I>(size, pixelFormat, backColor, alphaThreshold, palette);

                case PixelFormat.Format64bppArgb:
                    return new ManagedBitmapData<Color64, ManagedBitmapDataRow64Argb>(size, pixelFormat, backColor, alphaThreshold);

                case PixelFormat.Format64bppPArgb:
                    return new ManagedBitmapData<Color64, ManagedBitmapDataRow64PArgb>(size, pixelFormat, backColor, alphaThreshold);

                case PixelFormat.Format48bppRgb:
                    return new ManagedBitmapData<Color48, ManagedBitmapDataRow48Rgb>(size, pixelFormat, backColor, alphaThreshold);

                case PixelFormat.Format16bppRgb565:
                    return new ManagedBitmapData<Color16Rgb565, ManagedBitmapDataRow16Rgb565>(size, pixelFormat, backColor, alphaThreshold);

                case PixelFormat.Format16bppRgb555:
                    return new ManagedBitmapData<Color16Rgb555, ManagedBitmapDataRow16Rgb555>(size, pixelFormat, backColor, alphaThreshold);

                case PixelFormat.Format16bppArgb1555:
                    return new ManagedBitmapData<Color16Argb1555, ManagedBitmapDataRow16Argb1555>(size, pixelFormat, backColor, alphaThreshold);

                case PixelFormat.Format16bppGrayScale:
                    return new ManagedBitmapData<Color16Gray, ManagedBitmapDataRow16Gray>(size, pixelFormat, backColor, alphaThreshold);

                default:
                    throw new InvalidOperationException(Res.InternalError($"Unexpected pixel format {pixelFormat}"));
            }
        }

        #endregion

        #endregion
    }
}
