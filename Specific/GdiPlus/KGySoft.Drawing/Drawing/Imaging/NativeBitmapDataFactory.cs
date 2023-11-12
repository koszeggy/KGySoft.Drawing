#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: NativeBitmapDataFactory.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2023 - All Rights Reserved
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

using KGySoft.CoreLibraries;
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
        internal static IReadWriteBitmapData CreateBitmapData(Bitmap bitmap, ImageLockMode lockMode, Color32 backColor, byte alphaThreshold, WorkingColorSpace workingColorSpace)
        {
            if (workingColorSpace is < WorkingColorSpace.Default or > WorkingColorSpace.Srgb)
                throw new ArgumentOutOfRangeException(nameof(workingColorSpace), PublicResources.EnumOutOfRange(workingColorSpace));
            PixelFormat pixelFormat = bitmap.PixelFormat;

            // - On Windows Vista and above 8207 is used for CMYK images by JPEG/TIFF decoders but when accessing as its actual format
            //   the changes are not applied back to the original image so we access it as 24bpp bitmap data.
            //   Even this works only if it wasn't accessed as the actual format before and when the bitmap was created by a decoder, not by Bitmap constructor with pixel format parameter.
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
            Action dispose = () =>
            {
                try
                {
                    bitmap.UnlockBits(bitmapData);
                }
                catch (ArgumentException)
                {
                    // An Invalid parameter error may be thrown here by GDI+ for CMYK bitmaps or when the bitmap was disposed first.
                    // We just ignore these cases.
                }
            };
            KnownPixelFormat knownPixelFormat = bitmapDataPixelFormat.ToKnownPixelFormatInternal();
            Debug.Assert(knownPixelFormat != KnownPixelFormat.Undefined && knownPixelFormat.IsDefined());

            switch (pixelFormat)
            {
                case PixelFormat.Format32bppArgb:
                case PixelFormat.Format32bppPArgb:
                case PixelFormat.Format32bppRgb:
                case PixelFormat.Format24bppRgb:
                case PixelFormat.Format16bppArgb1555:
                case PixelFormat.Format16bppGrayScale:
                case PixelFormatExtensions.Format32bppCmyk:
                    return BitmapDataFactory.CreateBitmapData(bitmapData.Scan0, size, bitmapData.Stride, knownPixelFormat, workingColorSpace, backColor, alphaThreshold, dispose);

                case PixelFormat.Format8bppIndexed:
                case PixelFormat.Format4bppIndexed:
                case PixelFormat.Format1bppIndexed:
                    var palette = new Palette(bitmap.Palette.Entries.Select(c => c.ToColor32()), workingColorSpace, backColor, alphaThreshold);
                    return BitmapDataFactory.CreateBitmapData(bitmapData.Scan0, size, bitmapData.Stride, knownPixelFormat, palette, bitmap.TrySetPalette, dispose);

                case PixelFormat.Format64bppArgb:
                    return !ColorsHelper.LinearWideColors
                        ? BitmapDataFactory.CreateBitmapData(bitmapData.Scan0, size, bitmapData.Stride, knownPixelFormat, backColor, alphaThreshold, dispose)
                        : BitmapDataFactory.CreateBitmapData(bitmapData.Scan0, size, bitmapData.Stride, new CustomBitmapDataConfig
                        {
                            // Prefers64BitColors would be enough for precision but conversion from ColorF does not require a table for the better performance
                            PixelFormat = new PixelFormatInfo(64) { HasAlpha = true, Prefers128BitColors = true, LinearGamma = true },
                            BackColor = backColor,
                            AlphaThreshold = alphaThreshold,
                            WorkingColorSpace = workingColorSpace,
                            DisposeCallback = dispose,
                            BackBufferIndependentPixelAccess = true,
                            RowGetColor32 = (row, x) => row.UnsafeGetRefAs<GdiPlusColor64>(x).ToColor32(),
                            RowSetColor32 = (row, x, c) => row.UnsafeGetRefAs<GdiPlusColor64>(x) = new GdiPlusColor64(c),
                            RowGetColor64 = (row, x) => row.UnsafeGetRefAs<GdiPlusColor64>(x).ToColor64(),
                            RowSetColor64 = (row, x, c) => row.UnsafeGetRefAs<GdiPlusColor64>(x) = new GdiPlusColor64(c),
                            RowGetColorF = (row, x) => row.UnsafeGetRefAs<GdiPlusColor64>(x).ToColorF(),
                            RowSetColorF = (row, x, c) => row.UnsafeGetRefAs<GdiPlusColor64>(x) = new GdiPlusColor64(c),
                        });

                case PixelFormat.Format64bppPArgb:
                    return !ColorsHelper.LinearWideColors
                        ? BitmapDataFactory.CreateBitmapData(bitmapData.Scan0, size, bitmapData.Stride, knownPixelFormat, backColor, alphaThreshold, dispose)
                        : BitmapDataFactory.CreateBitmapData(bitmapData.Scan0, size, bitmapData.Stride, new CustomBitmapDataConfig
                        {
                            // Prefers64BitColors would be enough for precision but conversion from PColorF does not require a table for the better performance
                            PixelFormat = new PixelFormatInfo(64) { HasPremultipliedAlpha = true, Prefers128BitColors = true, LinearGamma = true },
                            BackColor = backColor,
                            AlphaThreshold = alphaThreshold,
                            WorkingColorSpace = workingColorSpace,
                            DisposeCallback = dispose,
                            BackBufferIndependentPixelAccess = true,
                            RowGetColor32 = (row, x) => row.UnsafeGetRefAs<GdiPlusPColor64>(x).ToColor32(),
                            RowSetColor32 = (row, x, c) => row.UnsafeGetRefAs<GdiPlusPColor64>(x) = new GdiPlusPColor64(c),
                            RowGetColor64 = (row, x) => row.UnsafeGetRefAs<GdiPlusPColor64>(x).ToColor64(),
                            RowSetColor64 = (row, x, c) => row.UnsafeGetRefAs<GdiPlusPColor64>(x) = new GdiPlusPColor64(c),
                            RowGetColorF = (row, x) => row.UnsafeGetRefAs<GdiPlusPColor64>(x).ToPColorF().ToColorF(),
                            RowSetColorF = (row, x, c) => row.UnsafeGetRefAs<GdiPlusPColor64>(x) = new GdiPlusPColor64(c.ToPColorF()),
                            RowGetPColorF = (row, x) => row.UnsafeGetRefAs<GdiPlusPColor64>(x).ToPColorF(),
                            RowSetPColorF = (row, x, c) => row.UnsafeGetRefAs<GdiPlusPColor64>(x) = new GdiPlusPColor64(c),
                        });

                case PixelFormat.Format48bppRgb:
                    return !ColorsHelper.LinearWideColors
                        ? BitmapDataFactory.CreateBitmapData(bitmapData.Scan0, size, bitmapData.Stride, knownPixelFormat, backColor, alphaThreshold, dispose)
                        : BitmapDataFactory.CreateBitmapData(bitmapData.Scan0, size, bitmapData.Stride, new PixelFormatInfo(48) { LinearGamma = true },
                        (row, x) => row.UnsafeGetRefAs<GdiPlusColor48>(x).ToColor32(),
                        (row, x, c) => row.UnsafeGetRefAs<GdiPlusColor48>(x) =
                            new GdiPlusColor48(c.A == Byte.MaxValue ? c : c.Blend(row.BitmapData.BackColor, row.BitmapData.WorkingColorSpace.GetValueOrLinear())),
                        workingColorSpace, backColor, alphaThreshold, dispose);

                case PixelFormat.Format16bppRgb565:
                    return pixelFormat == bitmapDataPixelFormat
                        ? BitmapDataFactory.CreateBitmapData(bitmapData.Scan0, size, bitmapData.Stride, knownPixelFormat, backColor, alphaThreshold, dispose)
                        : BitmapDataFactory.CreateBitmapData(bitmapData.Scan0, size, bitmapData.Stride, new PixelFormatInfo((byte)bitmapDataPixelFormat.ToBitsPerPixel()),
                            (row, x) => row.UnsafeGetRefAs<Color16As24>(x).ToColor32(),
                            (row, x, c) => row.UnsafeGetRefAs<Color16As24>(x) =
                                new Color16As24(c.Blend(row.BitmapData.BackColor, row.BitmapData.WorkingColorSpace), true),
                            workingColorSpace, backColor, alphaThreshold, dispose);

                case PixelFormat.Format16bppRgb555:
                    return pixelFormat == bitmapDataPixelFormat
                        ? BitmapDataFactory.CreateBitmapData(bitmapData.Scan0, size, bitmapData.Stride, knownPixelFormat, backColor, alphaThreshold, dispose)
                        : BitmapDataFactory.CreateBitmapData(bitmapData.Scan0, size, bitmapData.Stride, new PixelFormatInfo((byte)bitmapDataPixelFormat.ToBitsPerPixel()),
                            (row, x) => row.UnsafeGetRefAs<Color16As24>(x).ToColor32(),
                            (row, x, c) => row.UnsafeGetRefAs<Color16As24>(x) =
                                new Color16As24(c.Blend(row.BitmapData.BackColor, row.BitmapData.WorkingColorSpace), false),
                            workingColorSpace, backColor, alphaThreshold, dispose);

                default:
                    throw new InvalidOperationException(Res.InternalError($"Unexpected pixel format {pixelFormat}"));
            }
        }

        #endregion
    }
}
