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
using System.Security;

using KGySoft.CoreLibraries;
using KGySoft.Drawing.WinApi;

#endregion

namespace KGySoft.Drawing.Imaging
{
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
        internal static IBitmapDataInternal CreateBitmapData(Bitmap bitmap, ImageLockMode lockMode, Color32 backColor = default, byte alphaThreshold = 128, Palette? palette = null)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap), PublicResources.ArgumentNull);
            if (!lockMode.IsDefined())
                throw new ArgumentOutOfRangeException(nameof(lockMode), PublicResources.EnumOutOfRange(lockMode));

            PixelFormat pixelFormat = bitmap.PixelFormat;

            // On Linux with libgdiplus 16bpp formats can be accessed only via 24bpp bitmap data
            PixelFormat bitmapDataPixelFormat = OSUtils.IsWindows
                ? pixelFormat
                : pixelFormat is PixelFormat.Format16bppRgb565 or PixelFormat.Format16bppRgb555
                    ? PixelFormat.Format24bppRgb
                    : pixelFormat;

            Size size = bitmap.Size;
            BitmapData bitmapData = bitmap.LockBits(new Rectangle(Point.Empty, size), lockMode, bitmapDataPixelFormat);
            Action dispose = () => bitmap.UnlockBits(bitmapData);
            KnownPixelFormat knownPixelFormat = pixelFormat.ToKnownPixelFormatInternal();

            switch (pixelFormat)
            {
                case PixelFormat.Format32bppArgb:
                case PixelFormat.Format32bppPArgb:
                case PixelFormat.Format32bppRgb:
                case PixelFormat.Format24bppRgb:
                case PixelFormat.Format16bppArgb1555:
                case PixelFormat.Format16bppGrayScale:
                    return BitmapDataFactory.CreateUnmanagedBitmapData(bitmapData.Scan0, size, bitmapData.Stride, knownPixelFormat, backColor, alphaThreshold, null, null, dispose);

                case PixelFormat.Format8bppIndexed:
                case PixelFormat.Format4bppIndexed:
                case PixelFormat.Format1bppIndexed:
                    Debug.Assert(palette == null || palette.Equals(bitmap.Palette.Entries), "Non-null palette entries must match actual palette. Expected to be passed to re-use its cache only.");
                    palette ??= new Palette(bitmap.Palette.Entries, backColor.ToColor(), alphaThreshold);
                    return BitmapDataFactory.CreateUnmanagedBitmapData(bitmapData.Scan0, size, bitmapData.Stride, knownPixelFormat, backColor, alphaThreshold, palette, bitmap.TrySetPalette, dispose);

                case PixelFormat.Format64bppArgb:
                    return BitmapDataFactory.CreateUnmanagedCustomBitmapData(bitmapData.Scan0, size, bitmapData.Stride, new PixelFormatInfo(64) { HasAlpha = true },
                        (row, x) => row.UnsafeGetRefAs<Color64>(x).ToColor32PlatformDependent(),
                        (row, x, c) => row.UnsafeGetRefAs<Color64>(x) = c.ToColor64PlatformDependent(),
                        disposeCallback: dispose);

                case PixelFormat.Format64bppPArgb:
                    return BitmapDataFactory.CreateUnmanagedCustomBitmapData(bitmapData.Scan0, size, bitmapData.Stride, new PixelFormatInfo(64) { HasPremultipliedAlpha = true },
                        (row, x) => row.UnsafeGetRefAs<Color64>(x).ToStraight32PlatformDependent(),
                        (row, x, c) => row.UnsafeGetRefAs<Color64>(x) = c.ToPremultiplied64PlatformDependent(),
                        disposeCallback: dispose);

                case PixelFormat.Format48bppRgb:
                    return BitmapDataFactory.CreateUnmanagedCustomBitmapData(bitmapData.Scan0, size, bitmapData.Stride, new PixelFormatInfo(48),
                        (row, x) => row.UnsafeGetRefAs<Color48>(x).ToColor32PlatformDependent(),
                        (row, x, c) => row.UnsafeGetRefAs<Color48>(x) = (c.A == Byte.MaxValue ? c : c.BlendWithBackground(row.BitmapData.BackColor)).ToColor48PlatformDependent(),
                        backColor, alphaThreshold, dispose);

                case PixelFormat.Format16bppRgb565:
                    return pixelFormat == bitmapDataPixelFormat
                        ? BitmapDataFactory.CreateUnmanagedBitmapData(bitmapData.Scan0, size, bitmapData.Stride, knownPixelFormat, backColor, alphaThreshold, null, null, dispose)
                        : BitmapDataFactory.CreateUnmanagedCustomBitmapData(bitmapData.Scan0, size, bitmapData.Stride, new PixelFormatInfo((byte)bitmapDataPixelFormat.ToBitsPerPixel()),
                            (row, x) => new Color16Rgb565(row.UnsafeGetRefAs<Color24>(x).ToColor32()).ToColor32(),
                            (row, x, c) => row.UnsafeGetRefAs<Color24>(x) = new Color24(new Color16Rgb565(c.A == Byte.MaxValue ? c : c.BlendWithBackground(row.BitmapData.BackColor)).ToColor32()),
                            backColor, alphaThreshold, dispose);

                case PixelFormat.Format16bppRgb555:
                    return pixelFormat == bitmapDataPixelFormat
                        ? BitmapDataFactory.CreateUnmanagedBitmapData(bitmapData.Scan0, size, bitmapData.Stride, knownPixelFormat, backColor, alphaThreshold, null, null, dispose)
                        : BitmapDataFactory.CreateUnmanagedCustomBitmapData(bitmapData.Scan0, size, bitmapData.Stride, new PixelFormatInfo((byte)bitmapDataPixelFormat.ToBitsPerPixel()),
                            (row, x) => new Color16Rgb555(row.UnsafeGetRefAs<Color24>(x).ToColor32()).ToColor32(),
                            (row, x, c) => row.UnsafeGetRefAs<Color24>(x) = new Color24(new Color16Rgb555(c.A == Byte.MaxValue ? c : c.BlendWithBackground(row.BitmapData.BackColor)).ToColor32()),
                            backColor, alphaThreshold, dispose);

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
            if (!quantizingSession.Palette.Equals(bitmap.Palette.Entries))
                return CreateBitmapData(bitmap, lockMode, quantizingSession.BackColor, quantizingSession.AlphaThreshold);

            if (!lockMode.IsDefined())
                throw new ArgumentOutOfRangeException(nameof(lockMode), PublicResources.EnumOutOfRange(lockMode));

            // here the quantizer and the target bitmap uses the same palette
            return CreateBitmapData(bitmap, lockMode, quantizingSession.BackColor, quantizingSession.AlphaThreshold, quantizingSession.Palette);
        }

        #endregion
    }
}
