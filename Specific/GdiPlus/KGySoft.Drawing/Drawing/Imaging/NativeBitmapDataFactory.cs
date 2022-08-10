#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: NativeBitmapDataFactory.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2022 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
#if NET7_0_OR_GREATER
using System.Runtime.Versioning;
#endif
using System.Security;

using KGySoft.Drawing.WinApi;

#endregion

namespace KGySoft.Drawing.Imaging
{
#if NET7_0_OR_GREATER
    [SupportedOSPlatform("windows")]
#endif
    internal static class NativeBitmapDataFactory
    {
        #region Methods

        /// <summary>
        /// Creates a native <see cref="IBitmapDataInternal"/> from a <see cref="Bitmap"/>.
        /// </summary>
        [SecuritySafeCritical]
        [SuppressMessage("Microsoft.Maintainability", "CA1502: Avoid excessive complexity",
            Justification = "Very straightforward switch with many conditions. Would be OK without the libgdiplus special handling for 16bpp RGB555/565 formats.")]
        [SuppressMessage("VisualStudio.Style", "IDE0039: Use local function instead of lambda", Justification = "False alarm, it would be converted to a delegate anyway.")]
        internal static IReadWriteBitmapData CreateBitmapData(Bitmap bitmap, ImageLockMode lockMode, Color32 backColor = default, byte alphaThreshold = 128)
        {
            PixelFormat pixelFormat = bitmap.PixelFormat;

            // - On Windows Vista and above 8207 is used for CMYK images by JPEG/TIFF decoders but when accessing as its actual format
            //   the changes are not applied back to the original image so we access it as 24bpp bitmap data.
            //   Even this works only if it wasn't accessed as the actual format before and when the bitmap was created by a decoder, not by Bitmap constructor.
            // - On Linux with libgdiplus 16bpp formats can be accessed only via 24bpp bitmap data
            PixelFormat bitmapDataPixelFormat = OSUtils.IsWindows
                ? pixelFormat is PixelFormatExtensions.Format32bppCmyk
                    ? PixelFormat.Format24bppRgb
                    : pixelFormat
                : pixelFormat is PixelFormat.Format16bppRgb565 or PixelFormat.Format16bppRgb555
                    ? PixelFormat.Format24bppRgb
                    : pixelFormat;

            Size size = bitmap.Size;
            BitmapData bitmapData = bitmap.LockBits(new Rectangle(Point.Empty, size), lockMode, bitmapDataPixelFormat);
            Action dispose = () => bitmap.UnlockBits(bitmapData);
            KnownPixelFormat knownPixelFormat = bitmapDataPixelFormat.ToKnownPixelFormatInternal();

            switch (pixelFormat)
            {
                case PixelFormat.Format32bppArgb:
                case PixelFormat.Format32bppPArgb:
                case PixelFormat.Format32bppRgb:
                case PixelFormat.Format24bppRgb:
                case PixelFormat.Format16bppArgb1555:
                case PixelFormat.Format16bppGrayScale:
                case PixelFormatExtensions.Format32bppCmyk:
                    return BitmapDataFactory.CreateBitmapData(bitmapData.Scan0, size, bitmapData.Stride, knownPixelFormat, backColor, alphaThreshold, dispose);

                case PixelFormat.Format8bppIndexed:
                case PixelFormat.Format4bppIndexed:
                case PixelFormat.Format1bppIndexed:
                    var palette = new Palette(bitmap.Palette.Entries, backColor.ToColor(), alphaThreshold);
                    return BitmapDataFactory.CreateBitmapData(bitmapData.Scan0, size, bitmapData.Stride, knownPixelFormat, palette, bitmap.TrySetPalette, dispose);

                case PixelFormat.Format64bppArgb:
                    return BitmapDataFactory.CreateBitmapData(bitmapData.Scan0, size, bitmapData.Stride, new PixelFormatInfo(64) { HasAlpha = true },
                        (row, x) => row.UnsafeGetRefAs<GdiPColor64>(x).ToColor32(),
                        (row, x, c) => row.UnsafeGetRefAs<GdiPColor64>(x) = new GdiPColor64(c),
                        backColor, alphaThreshold, dispose);

                case PixelFormat.Format64bppPArgb:
                    return BitmapDataFactory.CreateBitmapData(bitmapData.Scan0, size, bitmapData.Stride, new PixelFormatInfo(64) { HasPremultipliedAlpha = true },
                        (row, x) => row.UnsafeGetRefAs<GdiPColor64>(x).ToStraight().ToColor32(),
                        (row, x, c) => row.UnsafeGetRefAs<GdiPColor64>(x) = new GdiPColor64(c).ToPremultiplied(),
                        backColor, alphaThreshold, dispose);

                case PixelFormat.Format48bppRgb:
                    return BitmapDataFactory.CreateBitmapData(bitmapData.Scan0, size, bitmapData.Stride, new PixelFormatInfo(48),
                        (row, x) => row.UnsafeGetRefAs<GdiPColor48>(x).ToColor32(),
                        (row, x, c) => row.UnsafeGetRefAs<GdiPColor48>(x) = new GdiPColor48(c.Blend(row.BitmapData.BackColor)),
                        backColor, alphaThreshold, dispose);

                case PixelFormat.Format16bppRgb565:
                    return pixelFormat == bitmapDataPixelFormat
                        ? BitmapDataFactory.CreateBitmapData(bitmapData.Scan0, size, bitmapData.Stride, knownPixelFormat, backColor, alphaThreshold, dispose)
                        : BitmapDataFactory.CreateBitmapData(bitmapData.Scan0, size, bitmapData.Stride, new PixelFormatInfo((byte)bitmapDataPixelFormat.ToBitsPerPixel()),
                            (row, x) => row.UnsafeGetRefAs<Color16As24>(x).ToColor32(),
                            (row, x, c) => row.UnsafeGetRefAs<Color16As24>(x) = new Color16As24(c.Blend(row.BitmapData.BackColor), true),
                            backColor, alphaThreshold, dispose);

                case PixelFormat.Format16bppRgb555:
                    return pixelFormat == bitmapDataPixelFormat
                        ? BitmapDataFactory.CreateBitmapData(bitmapData.Scan0, size, bitmapData.Stride, knownPixelFormat, backColor, alphaThreshold, dispose)
                        : BitmapDataFactory.CreateBitmapData(bitmapData.Scan0, size, bitmapData.Stride, new PixelFormatInfo((byte)bitmapDataPixelFormat.ToBitsPerPixel()),
                            (row, x) => row.UnsafeGetRefAs<Color16As24>(x).ToColor32(),
                            (row, x, c) => row.UnsafeGetRefAs<Color16As24>(x) = new Color16As24(c.Blend(row.BitmapData.BackColor), false),
                            backColor, alphaThreshold, dispose);

                default:
                    throw new InvalidOperationException(Res.InternalError($"Unexpected pixel format {pixelFormat}"));
            }
        }

        #endregion
    }
}
