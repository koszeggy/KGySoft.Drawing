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

using KGySoft.CoreLibraries;

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal static class BitmapDataAccessorFactory
    {
        #region Methods

        internal static BitmapDataAccessorBase CreateAccessor(Bitmap bitmap, ImageLockMode lockMode)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap), PublicResources.ArgumentNull);
            if (!lockMode.IsDefined())
                throw new ArgumentOutOfRangeException(nameof(lockMode), PublicResources.EnumOutOfRange(lockMode));

            var pixelFormat = bitmap.PixelFormat;
            switch (pixelFormat)
            {
                case PixelFormat.Format32bppArgb:
                    return new BitmapDataAccessor<BitmapDataRow32Argb>(bitmap, pixelFormat, lockMode);

                case PixelFormat.Format32bppPArgb:
                    return new BitmapDataAccessor<BitmapDataRow32PArgb>(bitmap, pixelFormat, lockMode);

                case PixelFormat.Format32bppRgb:
                    return new BitmapDataAccessor<BitmapDataRow32Rgb>(bitmap, pixelFormat, lockMode);

                case PixelFormat.Format24bppRgb:
                    return new BitmapDataAccessor<BitmapDataRow24Rgb>(bitmap, pixelFormat, lockMode);

                case PixelFormat.Format64bppArgb:
                    return new BitmapDataAccessor<BitmapDataRow64Argb>(bitmap, pixelFormat, lockMode);

                case PixelFormat.Format64bppPArgb:
                    return new BitmapDataAccessor<BitmapDataRow64PArgb>(bitmap, pixelFormat, lockMode);

                case PixelFormat.Format48bppRgb:
                    return new BitmapDataAccessor<BitmapDataRow48Rgb>(bitmap, pixelFormat, lockMode);

                case PixelFormat.Format16bppRgb565:
                    return new BitmapDataAccessor<BitmapDataRow16Rgb565>(bitmap, pixelFormat, lockMode);

                case PixelFormat.Format16bppRgb555:
                    return new BitmapDataAccessor<BitmapDataRow16Rgb555>(bitmap, pixelFormat, lockMode);

                case PixelFormat.Format16bppArgb1555:
                    return new BitmapDataAccessor<BitmapDataRow16Argb1555>(bitmap, pixelFormat, lockMode);

                case PixelFormat.Format16bppGrayScale:
                    return new BitmapDataAccessor<BitmapDataRow16Gray>(bitmap, pixelFormat, lockMode);

                case PixelFormat.Format8bppIndexed:
                    return new BitmapDataAccessorIndexed<BitmapDataRow8I>(bitmap, pixelFormat, lockMode);

                case PixelFormat.Format4bppIndexed:
                    return new BitmapDataAccessorIndexed<BitmapDataRow4I>(bitmap, pixelFormat, lockMode);

                case PixelFormat.Format1bppIndexed:
                    return new BitmapDataAccessorIndexed<BitmapDataRow1I>(bitmap, pixelFormat, lockMode);

                default:
                    throw new ArgumentException(Res.ImagingPixelFormatNotSupported(pixelFormat), nameof(bitmap));
            }
        }

        #endregion
    }
}
