#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: WriteableBitmapExtensions.cs
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

#region Used Namespaces

using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using KGySoft.Drawing.Imaging;

#endregion

#region Used Aliases

using Size = System.Drawing.Size;

#endregion

#endregion

namespace KGySoft.Drawing.Wpf
{
    /// <summary>
    /// Contains extension methods for the <see cref="WriteableBitmap"/> type.
    /// </summary>
    public static class WriteableBitmapExtensions
    {
        #region Fields

        private static Color32 black = Color32.FromGray(Byte.MinValue);
        private static Color32 white = Color32.FromGray(Byte.MaxValue);

        #endregion

        #region Methods

        /// <summary>
        /// Gets a managed read-write accessor for a <see cref="WriteableBitmap"/> instance.
        /// </summary>
        /// <param name="bitmap">The bitmap to get the managed accessor.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance that provides managed access to the specified <see cref="bitmap"/>.</returns>
        public static IReadWriteBitmapData GetReadWriteBitmapData(this WriteableBitmap bitmap, Color backColor = default, byte alphaThreshold = 128)
        {
            #region Local Methods

            Palette? GetPalette()
            {
                BitmapPalette? palette = bitmap.Palette;
                return palette == null ? null : new Palette(palette.Colors.Select(c => c.ToColor32()).ToArray(), backColor.ToColor32(), alphaThreshold);
            }

            static bool TrySetPalette(Palette _) => false;

            static int GetColorIndexI2(ICustomBitmapDataRow row, int x)
            {
                int bits = row.UnsafeGetRefAs<byte>(x >> 2);
                return (x & 3) switch
                {
                    0 => bits >> 6,
                    1 => (bits >> 4) & 3,
                    2 => (bits >> 2) & 3,
                    _ => bits & 3,
                };
            }

            static void SetColorIndexI2(ICustomBitmapDataRow row, int x, int colorIndex)
            {
                int pos = x >> 2;
                ref byte bits = ref row.UnsafeGetRefAs<byte>(pos);
                switch (x & 3)
                {
                    case 0:
                        bits &= 0b00111111;
                        bits |= (byte)(colorIndex << 6);
                        break;
                    case 1:
                        bits &= 0b11001111;
                        bits |= (byte)(colorIndex << 4);
                        break;
                    case 2:
                        bits &= 0b11110011;
                        bits |= (byte)(colorIndex << 2);
                        break;
                    default:
                        bits &= 0b11111100;
                        bits |= (byte)colorIndex;
                        break;
                }
            }

            static Color32 GetColorBlackWhite(ICustomBitmapDataRow row, int x)
            {
                int mask = 128 >> (x & 7);
                int bits = row.UnsafeGetRefAs<byte>(x >> 3);
                return (bits & mask) != 0 ? white : black;
            }

            static void SetColorBlackWhite(ICustomBitmapDataRow row, int x, Color32 c)
            {
                int pos = x >> 3;
                byte brightness = c.Blend(row.BitmapData.BackColor).GetBrightness();
                int mask = 128 >> (x & 7);
                if (brightness < 128)
                    row.UnsafeGetRefAs<byte>(pos) &= (byte)~mask;
                else
                    row.UnsafeGetRefAs<byte>(pos) |= (byte)mask;
            }

            static int GetColorIndexI4(ICustomBitmapDataRow row, int x)
            {
                int nibbles = row.UnsafeGetRefAs<byte>(x >> 1);
                return (x & 1) == 0
                    ? nibbles >> 4
                    : nibbles & 0b00001111;
            }

            static void SetColorIndexI4(ICustomBitmapDataRow row, int x, int colorIndex)
            {
                ref byte nibbles = ref row.UnsafeGetRefAs<byte>(x >> 1);
                if ((x & 1) == 0)
                {
                    nibbles &= 0b00001111;
                    nibbles |= (byte)(colorIndex << 4);
                }
                else
                {
                    nibbles &= 0b11110000;
                    nibbles |= (byte)colorIndex;
                }
            }

            #endregion

            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap), PublicResources.ArgumentNull);
            if (bitmap.IsFrozen)
                throw new ArgumentException(Res.WriteableBitmapFrozen, nameof(bitmap));

            PixelFormat sourceFormat = bitmap.Format;

            KnownPixelFormat knownFormat = sourceFormat == PixelFormats.Bgra32 ? KnownPixelFormat.Format32bppArgb
                : sourceFormat == PixelFormats.Pbgra32 ? KnownPixelFormat.Format32bppPArgb
                : sourceFormat == PixelFormats.Bgr32 ? KnownPixelFormat.Format32bppRgb
                : sourceFormat == PixelFormats.Bgr24 ? KnownPixelFormat.Format24bppRgb
                : sourceFormat == PixelFormats.Indexed1 ? KnownPixelFormat.Format1bppIndexed
                : sourceFormat == PixelFormats.Indexed4 ? KnownPixelFormat.Format4bppIndexed
                : sourceFormat == PixelFormats.Indexed8 ? KnownPixelFormat.Format8bppIndexed
                : sourceFormat == PixelFormats.Bgr555 ? KnownPixelFormat.Format16bppRgb555
                : sourceFormat == PixelFormats.Bgr565 ? KnownPixelFormat.Format16bppRgb565
                : sourceFormat == PixelFormats.Gray16 ? KnownPixelFormat.Format16bppGrayScale
                : default;

            bitmap.Lock();
            var size = new Size(bitmap.PixelWidth, bitmap.PixelHeight);
            var backColor32 = backColor.ToColor32();
            Action dispose = () =>
            {
                bitmap.AddDirtyRect(new Int32Rect(0, 0, size.Width, size.Height));
                bitmap.Unlock();
            };

            // Known pixel formats
            if (knownFormat != KnownPixelFormat.Undefined)
                return knownFormat.IsIndexed()
                    ? BitmapDataFactory.CreateBitmapData(bitmap.BackBuffer, size, bitmap.BackBufferStride, knownFormat, GetPalette(), TrySetPalette, dispose)
                    : BitmapDataFactory.CreateBitmapData(bitmap.BackBuffer, size, bitmap.BackBufferStride, knownFormat, backColor32, alphaThreshold, dispose);

            // Custom pixel formats
            if (sourceFormat == PixelFormats.Rgb24)
                return BitmapDataFactory.CreateBitmapData(bitmap.BackBuffer, size, bitmap.BackBufferStride, new PixelFormatInfo(24),
                    (row, x) => row.UnsafeGetRefAs<ColorRgb24>(x).ToColor32(),
                    (row, x, c) => row.UnsafeGetRefAs<ColorRgb24>(x) = new ColorRgb24(c.Blend(row.BitmapData.BackColor)),
                    backColor32, alphaThreshold, dispose);

            if (sourceFormat == PixelFormats.Indexed2)
                return BitmapDataFactory.CreateBitmapData(bitmap.BackBuffer, size, bitmap.BackBufferStride, new PixelFormatInfo(2) { Indexed = true },
                    GetColorIndexI2, SetColorIndexI2, GetPalette(), TrySetPalette, dispose);

            if (sourceFormat == PixelFormats.BlackWhite)
                return BitmapDataFactory.CreateBitmapData(bitmap.BackBuffer, size, bitmap.BackBufferStride, new PixelFormatInfo(1) { Grayscale = true },
                    GetColorBlackWhite, SetColorBlackWhite, backColor32, alphaThreshold, dispose);

            if (sourceFormat == PixelFormats.Gray2)
            {
                Palette colors = Palette.Grayscale4(backColor32);
                return BitmapDataFactory.CreateBitmapData(bitmap.BackBuffer, size, bitmap.BackBufferStride, new PixelFormatInfo(2) { Grayscale = true },
                    (row, x) => colors.GetColor(GetColorIndexI2(row, x)),
                    (row, x, c) => SetColorIndexI2(row, x, colors.GetNearestColorIndex(c)),
                    backColor32, alphaThreshold, dispose);
            }

            if (sourceFormat == PixelFormats.Gray4)
            {
                Palette colors = Palette.Grayscale16(backColor32);
                return BitmapDataFactory.CreateBitmapData(bitmap.BackBuffer, size, bitmap.BackBufferStride, new PixelFormatInfo(4) { Grayscale = true },
                    (row, x) => colors.GetColor(GetColorIndexI4(row, x)),
                    (row, x, c) => SetColorIndexI4(row, x, colors.GetNearestColorIndex(c)),
                    backColor32, alphaThreshold, dispose);
            }

            if (sourceFormat == PixelFormats.Gray8)
                return BitmapDataFactory.CreateBitmapData(bitmap.BackBuffer, size, bitmap.BackBufferStride, new PixelFormatInfo(8) { Grayscale = true },
                    (row, x) => Color32.FromGray(row.UnsafeGetRefAs<byte>(x)),
                    (row, x, c) => row.UnsafeGetRefAs<byte>(x) = c.Blend(row.BitmapData.BackColor).GetBrightness(),
                    backColor32, alphaThreshold, dispose);

            if (sourceFormat == PixelFormats.Gray32Float)
                return BitmapDataFactory.CreateBitmapData(bitmap.BackBuffer, size, bitmap.BackBufferStride, new PixelFormatInfo(32) { Grayscale = true },
                    (row, x) => row.UnsafeGetRefAs<ColorGrayF>(x).ToColor32(),
                    (row, x, c) => row.UnsafeGetRefAs<ColorGrayF>(x) = new ColorGrayF(c.Blend(row.BitmapData.BackColor)),
                    backColor32, alphaThreshold, dispose);

            if (sourceFormat == PixelFormats.Bgr101010)
                return BitmapDataFactory.CreateBitmapData(bitmap.BackBuffer, size, bitmap.BackBufferStride, new PixelFormatInfo(32),
                    (row, x) => row.UnsafeGetRefAs<ColorBgr101010>(x).ToColor32(),
                    (row, x, c) => row.UnsafeGetRefAs<ColorBgr101010>(x) = new ColorBgr101010(c.Blend(row.BitmapData.BackColor)),
                    backColor32, alphaThreshold, dispose);

            if (sourceFormat == PixelFormats.Rgb48)
                return BitmapDataFactory.CreateBitmapData(bitmap.BackBuffer, size, bitmap.BackBufferStride, new PixelFormatInfo(32),
                    (row, x) => row.UnsafeGetRefAs<ColorRgb48>(x).ToColor32(),
                    (row, x, c) => row.UnsafeGetRefAs<ColorRgb48>(x) = new ColorRgb48(c.Blend(row.BitmapData.BackColor)),
                    backColor32, alphaThreshold, dispose);

            if (sourceFormat == PixelFormats.Rgba64)
                return BitmapDataFactory.CreateBitmapData(bitmap.BackBuffer, size, bitmap.BackBufferStride, new PixelFormatInfo(64) { HasAlpha = true },
                    (row, x) => row.UnsafeGetRefAs<ColorRgba64>(x).ToColor32(),
                    (row, x, c) => row.UnsafeGetRefAs<ColorRgba64>(x) = new ColorRgba64(c),
                    backColor32, alphaThreshold, dispose);

            if (sourceFormat == PixelFormats.Prgba64)
                return BitmapDataFactory.CreateBitmapData(bitmap.BackBuffer, size, bitmap.BackBufferStride, new PixelFormatInfo(64) { HasPremultipliedAlpha = true },
                    (row, x) => row.UnsafeGetRefAs<ColorRgba64>(x).ToStraight().ToColor32(),
                    (row, x, c) => row.UnsafeGetRefAs<ColorRgba64>(x) = new ColorRgba64(c).ToPremultiplied(),
                    backColor32, alphaThreshold, dispose);

            if (sourceFormat == PixelFormats.Rgba128Float)
                return BitmapDataFactory.CreateBitmapData(bitmap.BackBuffer, size, bitmap.BackBufferStride, new PixelFormatInfo(128) { HasAlpha = true },
                    (row, x) => row.UnsafeGetRefAs<ColorRgba128>(x).ToColor32(),
                    (row, x, c) => row.UnsafeGetRefAs<ColorRgba128>(x) = new ColorRgba128(c),
                    backColor32, alphaThreshold, dispose);

            if (sourceFormat == PixelFormats.Prgba128Float)
                return BitmapDataFactory.CreateBitmapData(bitmap.BackBuffer, size, bitmap.BackBufferStride, new PixelFormatInfo(128) { HasPremultipliedAlpha = true },
                    (row, x) => row.UnsafeGetRefAs<ColorRgba128>(x).ToStraight().ToColor32(),
                    (row, x, c) => row.UnsafeGetRefAs<ColorRgba128>(x) = new ColorRgba128(c).ToPremultiplied(),
                    backColor32, alphaThreshold, dispose);

            if (sourceFormat == PixelFormats.Rgb128Float)
                return BitmapDataFactory.CreateBitmapData(bitmap.BackBuffer, size, bitmap.BackBufferStride, new PixelFormatInfo(128),
                    (row, x) => row.UnsafeGetRefAs<ColorRgba128>(x).ToColor32().ToOpaque(),
                    (row, x, c) => row.UnsafeGetRefAs<ColorRgba128>(x) = new ColorRgba128(c.Blend(row.BitmapData.BackColor)),
                    backColor32, alphaThreshold, dispose);

            if (sourceFormat == PixelFormats.Cmyk32)
                return BitmapDataFactory.CreateBitmapData(bitmap.BackBuffer, size, bitmap.BackBufferStride, new PixelFormatInfo(32),
                    (row, x) => row.UnsafeGetRefAs<ColorCmyk32>(x).ToColor32(),
                    (row, x, c) => row.UnsafeGetRefAs<ColorCmyk32>(x) = new ColorCmyk32(c.Blend(row.BitmapData.BackColor)),
                    backColor32, alphaThreshold, dispose);

            bitmap.Unlock();
            throw new InvalidOperationException(Res.InternalError($"Unexpected PixelFormat {sourceFormat}"));
        }

        #endregion
    }
}
