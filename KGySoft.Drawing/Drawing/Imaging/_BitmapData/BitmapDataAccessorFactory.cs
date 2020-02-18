#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BitmapDataAccessorFactory.cs
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
    internal static class BitmapDataAccessorFactory
    {
        #region Methods

        internal static BitmapDataAccessorBase CreateAccessor(Bitmap bitmap, ImageLockMode lockMode, Color32 backColor = default, byte alphaThreshold = 128)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap), PublicResources.ArgumentNull);
            if (!lockMode.IsDefined())
                throw new ArgumentOutOfRangeException(nameof(lockMode), PublicResources.EnumOutOfRange(lockMode));

            var pixelFormat = bitmap.PixelFormat;
            switch (pixelFormat)
            {
                case PixelFormat.Format32bppArgb:
                    return new BitmapDataAccessor<BitmapDataRow32Argb>(bitmap, pixelFormat, lockMode, backColor, alphaThreshold);

                case PixelFormat.Format32bppPArgb:
                    return new BitmapDataAccessor<BitmapDataRow32PArgb>(bitmap, pixelFormat, lockMode, backColor, alphaThreshold);

                case PixelFormat.Format32bppRgb:
                    return new BitmapDataAccessor<BitmapDataRow32Rgb>(bitmap, pixelFormat, lockMode, backColor, alphaThreshold);

                case PixelFormat.Format24bppRgb:
                    return new BitmapDataAccessor<BitmapDataRow24Rgb>(bitmap, pixelFormat, lockMode, backColor, alphaThreshold);

                case PixelFormat.Format64bppArgb:
                    return new BitmapDataAccessor<BitmapDataRow64Argb>(bitmap, pixelFormat, lockMode, backColor, alphaThreshold);

                case PixelFormat.Format64bppPArgb:
                    return new BitmapDataAccessor<BitmapDataRow64PArgb>(bitmap, pixelFormat, lockMode, backColor, alphaThreshold);

                case PixelFormat.Format48bppRgb:
                    return new BitmapDataAccessor<BitmapDataRow48Rgb>(bitmap, pixelFormat, lockMode, backColor, alphaThreshold);

                case PixelFormat.Format16bppRgb565:
                    return OSUtils.IsWindows
                        ? (BitmapDataAccessorBase)new BitmapDataAccessor<BitmapDataRow16Rgb565>(bitmap, pixelFormat, lockMode, backColor, alphaThreshold)
                        : new BitmapDataAccessor<BitmapDataRow16Rgb565Via24Bpp>(bitmap, PixelFormat.Format24bppRgb, lockMode, backColor, alphaThreshold);

                case PixelFormat.Format16bppRgb555:
                    return OSUtils.IsWindows
                        ? (BitmapDataAccessorBase)new BitmapDataAccessor<BitmapDataRow16Rgb555>(bitmap, pixelFormat, lockMode, backColor, alphaThreshold)
                        : new BitmapDataAccessor<BitmapDataRow16Rgb555Via24Bpp>(bitmap, PixelFormat.Format24bppRgb, lockMode, backColor, alphaThreshold);

                case PixelFormat.Format16bppArgb1555:
                    return new BitmapDataAccessor<BitmapDataRow16Argb1555>(bitmap, pixelFormat, lockMode, backColor, alphaThreshold);

                case PixelFormat.Format16bppGrayScale:
                    return new BitmapDataAccessor<BitmapDataRow16Gray>(bitmap, pixelFormat, lockMode, backColor, alphaThreshold);

                case PixelFormat.Format8bppIndexed:
                    return new BitmapDataAccessor<BitmapDataRow8I>(bitmap, pixelFormat, lockMode, backColor, alphaThreshold);

                case PixelFormat.Format4bppIndexed:
                    return new BitmapDataAccessor<BitmapDataRow4I>(bitmap, pixelFormat, lockMode, backColor, alphaThreshold);

                case PixelFormat.Format1bppIndexed:
                    return new BitmapDataAccessor<BitmapDataRow1I>(bitmap, pixelFormat, lockMode, backColor, alphaThreshold);

                default:
                    throw new InvalidOperationException(Res.InternalError($"Unexpected pixel format {pixelFormat}"));
            }
        }

        /// <summary>
        /// Creates a <see cref="BitmapDataAccessorBase"/> by a quantizer session re-using its palette if possible.
        /// </summary>
        internal static BitmapDataAccessorBase CreateAccessor(Bitmap bitmap, ImageLockMode lockMode, IQuantizingSession quantizingSession)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap), PublicResources.ArgumentNull);

            var pixelFormat = bitmap.PixelFormat;
            if (!pixelFormat.IsIndexed() || quantizingSession.Palette == null)
                return CreateAccessor(bitmap, lockMode, quantizingSession.BackColor, quantizingSession.AlphaThreshold);

            // checking if bitmap and quantizer palette has the same entries
            var bmpPalette = bitmap.Palette.Entries;
            var quantizerPalette = quantizingSession.Palette.Entries;
            if (bmpPalette.Length != quantizerPalette.Length || bmpPalette.Zip(quantizerPalette, (c1, c2) => new Color32(c1) != c2).Any(b => b))
                return CreateAccessor(bitmap, lockMode, quantizingSession.BackColor, quantizingSession.AlphaThreshold);

            if (!lockMode.IsDefined())
                throw new ArgumentOutOfRangeException(nameof(lockMode), PublicResources.EnumOutOfRange(lockMode));

            // here the quantizer and the target bitmap uses the same palette
            switch (pixelFormat)
            {
                case PixelFormat.Format8bppIndexed:
                    return new BitmapDataAccessor<BitmapDataRow8I>(bitmap, pixelFormat, lockMode, quantizingSession);

                case PixelFormat.Format4bppIndexed:
                    return new BitmapDataAccessor<BitmapDataRow4I>(bitmap, pixelFormat, lockMode, quantizingSession);

                case PixelFormat.Format1bppIndexed:
                    return new BitmapDataAccessor<BitmapDataRow1I>(bitmap, pixelFormat, lockMode, quantizingSession);

                default:
                    throw new InvalidOperationException(Res.InternalError($"Unexpected indexed format: {pixelFormat}"));
            }
        }

        #endregion
    }
}
