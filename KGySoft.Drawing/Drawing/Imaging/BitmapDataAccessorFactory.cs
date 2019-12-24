using System;
using System.Drawing;
using System.Drawing.Imaging;
using KGySoft.CoreLibraries;

namespace KGySoft.Drawing.Imaging
{
    public static class BitmapDataAccessorFactory
    {
        internal static IBitmapDataAccessor CreateAccessor(Bitmap bitmap, ImageLockMode lockMode, bool omitPremultiplication)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap), PublicResources.ArgumentNull);
            if (!lockMode.IsDefined())
                throw new ArgumentOutOfRangeException(nameof(lockMode), PublicResources.EnumOutOfRange(lockMode));

            var pixelFormat = bitmap.PixelFormat;
            switch (pixelFormat)
            {
                case PixelFormat.Format32bppArgb:
                    return new BitmapDataAccessor<BitmapDataRowArgb32>(bitmap, pixelFormat, lockMode);

                case PixelFormat.Format32bppPArgb:
                    return omitPremultiplication
                        ? (IBitmapDataAccessor)new BitmapDataAccessor<BitmapDataRowArgb32>(bitmap, pixelFormat, lockMode)
                        : new BitmapDataAccessor<BitmapDataRowPArgb32>(bitmap, pixelFormat, lockMode);

                case PixelFormat.Indexed:
                case PixelFormat.Gdi:
                case PixelFormat.Alpha:
                case PixelFormat.PAlpha:
                case PixelFormat.Extended:
                case PixelFormat.Canonical:
                case PixelFormat.Undefined:
                case PixelFormat.Format1bppIndexed:
                case PixelFormat.Format4bppIndexed:
                case PixelFormat.Format8bppIndexed:
                case PixelFormat.Format16bppGrayScale:
                case PixelFormat.Format16bppRgb555:
                case PixelFormat.Format16bppRgb565:
                case PixelFormat.Format16bppArgb1555:
                case PixelFormat.Format24bppRgb:
                case PixelFormat.Format32bppRgb:
                case PixelFormat.Format48bppRgb:
                case PixelFormat.Format64bppArgb:
                case PixelFormat.Format64bppPArgb:
                case PixelFormat.Max:
                default:
                    throw new ArgumentException(Res.ImagingPixelFormatNotSupported(pixelFormat), nameof(bitmap));
            }
        }
    }
}
