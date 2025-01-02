#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: PaletteTest.cs
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
using System.Drawing;

using KGySoft.Drawing.Imaging;

using NUnit.Framework;

#endregion

namespace KGySoft.Drawing.UnitTests.Imaging
{
    [TestFixture]
    public class PaletteTest : TestBase
    {
        #region Properties

        private static object[][] PaletteLookupTestSource { get; } =
        [
            ["RGB565 sRGB", new CustomIndexedBitmapDataConfig
            {
                PixelFormat = new PixelFormatInfo(16) { Indexed = true },
                BackBufferIndependentPixelAccess = true,
                RowGetColorIndex = (r, x) => r.UnsafeGetRefAs<ushort>(x),
                RowSetColorIndex = (r, x, i) => r.UnsafeGetRefAs<ushort>(x) = (ushort)i,
                Palette = new Palette(PaletteRgb565, WorkingColorSpace.Srgb, Color.Silver)
            }],
            ["RGB565 linear", new CustomIndexedBitmapDataConfig
            {
                PixelFormat = new PixelFormatInfo(16) { Indexed = true },
                BackBufferIndependentPixelAccess = true,
                RowGetColorIndex = (r, x) => r.UnsafeGetRefAs<ushort>(x),
                RowSetColorIndex = (r, x, i) => r.UnsafeGetRefAs<ushort>(x) = (ushort)i,
                Palette = new Palette(PaletteRgb565, WorkingColorSpace.Linear, Color.Silver)
            }],

            ["ARGB4444 direct", new CustomIndexedBitmapDataConfig
            {
                PixelFormat = new PixelFormatInfo(16) { Indexed = true },
                BackBufferIndependentPixelAccess = true,
                RowGetColorIndex = (r, x) => r.UnsafeGetRefAs<ushort>(x),
                RowSetColorIndex = (r, x, i) => r.UnsafeGetRefAs<ushort>(x) = (ushort)i,
                Palette = new Palette(PaletteArgb4444, default, 16, (c, p) => c.A < p.AlphaThreshold ? UInt16.MaxValue : ((15 - (c.A >> 4)) << 12) | ((c.R >> 4) << 8) | ((c.G >> 4) << 4) | (c.B >> 4))
            }],
            ["ARGB4444 sRGB", new CustomIndexedBitmapDataConfig
            {
                PixelFormat = new PixelFormatInfo(16) { Indexed = true },
                BackBufferIndependentPixelAccess = true,
                RowGetColorIndex = (r, x) => r.UnsafeGetRefAs<ushort>(x),
                RowSetColorIndex = (r, x, i) => r.UnsafeGetRefAs<ushort>(x) = (ushort)i,
                Palette = new Palette(PaletteArgb4444, WorkingColorSpace.Srgb, default, 16)
            }],
            ["ARGB4444 linear", new CustomIndexedBitmapDataConfig
            {
                PixelFormat = new PixelFormatInfo(16) { Indexed = true },
                BackBufferIndependentPixelAccess = true,
                RowGetColorIndex = (r, x) => r.UnsafeGetRefAs<ushort>(x),
                RowSetColorIndex = (r, x, i) => r.UnsafeGetRefAs<ushort>(x) = (ushort)i,
                Palette = new Palette(PaletteArgb4444, WorkingColorSpace.Linear, default, 16)
            }],
            ["ARGB2222 direct", new CustomIndexedBitmapDataConfig
            {
                PixelFormat = new PixelFormatInfo(8) { Indexed = true },
                BackBufferIndependentPixelAccess = true,
                RowGetColorIndex = (r, x) => r.UnsafeGetRefAs<byte>(x),
                RowSetColorIndex = (r, x, i) => r.UnsafeGetRefAs<byte>(x) = (byte)i,
                Palette = new Palette(PaletteArgb2222, default, 16, (c, p) => c.A < p.AlphaThreshold ? Byte.MaxValue : ((3 - (c.A >> 6)) << 6) | ((c.R >> 6) << 4) | ((c.G >> 6) << 2) | (c.B >> 6))
            }],
            ["ARGB2222 sRGB", new CustomIndexedBitmapDataConfig
            {
                PixelFormat = new PixelFormatInfo(8) { Indexed = true },
                BackBufferIndependentPixelAccess = true,
                RowGetColorIndex = (r, x) => r.UnsafeGetRefAs<byte>(x),
                RowSetColorIndex = (r, x, i) => r.UnsafeGetRefAs<byte>(x) = (byte)i,
                Palette = new Palette(PaletteArgb2222, WorkingColorSpace.Srgb, default, 16)
            }],
            ["ARGB2222 linear", new CustomIndexedBitmapDataConfig
            {
                PixelFormat = new PixelFormatInfo(8) { Indexed = true },
                BackBufferIndependentPixelAccess = true,
                RowGetColorIndex = (r, x) => r.UnsafeGetRefAs<byte>(x),
                RowSetColorIndex = (r, x, i) => r.UnsafeGetRefAs<byte>(x) = (byte)i,
                Palette = new Palette(PaletteArgb2222, WorkingColorSpace.Linear, default, 16)
            }],
            ["RGB332 sRGB", new CustomIndexedBitmapDataConfig
            {
                PixelFormat = new PixelFormatInfo(8) { Indexed = true },
                BackBufferIndependentPixelAccess = true,
                RowGetColorIndex = (r, x) => r.UnsafeGetRefAs<byte>(x),
                RowSetColorIndex = (r, x, i) => r.UnsafeGetRefAs<byte>(x) = (byte)i,
                Palette = Palette.Rgb332(Color.Silver)
            }],
            ["RGB332 linear", new CustomIndexedBitmapDataConfig
            {
                PixelFormat = new PixelFormatInfo(8) { Indexed = true },
                BackBufferIndependentPixelAccess = true,
                RowGetColorIndex = (r, x) => r.UnsafeGetRefAs<byte>(x),
                RowSetColorIndex = (r, x, i) => r.UnsafeGetRefAs<byte>(x) = (byte)i,
                Palette = Palette.Rgb332(WorkingColorSpace.Linear, Color.Silver)
            }],
            ["WebSafe sRGB", new CustomIndexedBitmapDataConfig
            {
                PixelFormat = new PixelFormatInfo(8) { Indexed = true },
                BackBufferIndependentPixelAccess = true,
                RowGetColorIndex = (r, x) => r.UnsafeGetRefAs<byte>(x),
                RowSetColorIndex = (r, x, i) => r.UnsafeGetRefAs<byte>(x) = (byte)i,
                Palette = Palette.SystemDefault8BppPalette(WorkingColorSpace.Srgb, Color.Silver, 16)
            }],
            ["WebSafe linear", new CustomIndexedBitmapDataConfig
            {
                PixelFormat = new PixelFormatInfo(8) { Indexed = true },
                BackBufferIndependentPixelAccess = true,
                RowGetColorIndex = (r, x) => r.UnsafeGetRefAs<byte>(x),
                RowSetColorIndex = (r, x, i) => r.UnsafeGetRefAs<byte>(x) = (byte)i,
                Palette = Palette.SystemDefault8BppPalette(WorkingColorSpace.Linear, Color.Silver, 16)
            }],
            ["Gray8Alpha direct", new CustomIndexedBitmapDataConfig
            {
                PixelFormat = new PixelFormatInfo(8) { Indexed = true },
                BackBufferIndependentPixelAccess = true,
                RowGetColorIndex = (r, x) => r.UnsafeGetRefAs<byte>(x),
                RowSetColorIndex = (r, x, i) => r.UnsafeGetRefAs<byte>(x) = (byte)i,
                Palette = new Palette(PaletteGray8Alpha, default, 16, customGetNearestColorIndex: (c, p) => c.A < p.AlphaThreshold ? Byte.MaxValue : ((15 - (c.A >> 4)) << 4) | (c.GetBrightness(p.WorkingColorSpace) >> 4))
            }],
            ["Gray8Alpha sRGB", new CustomIndexedBitmapDataConfig
            {
                PixelFormat = new PixelFormatInfo(8) { Indexed = true },
                BackBufferIndependentPixelAccess = true,
                RowGetColorIndex = (r, x) => r.UnsafeGetRefAs<byte>(x),
                RowSetColorIndex = (r, x, i) => r.UnsafeGetRefAs<byte>(x) = (byte)i,
                Palette = new Palette(PaletteGray8Alpha, WorkingColorSpace.Srgb, default, 16)
            }],
            ["Gray8Alpha linear", new CustomIndexedBitmapDataConfig
            {
                PixelFormat = new PixelFormatInfo(8) { Indexed = true },
                BackBufferIndependentPixelAccess = true,
                RowGetColorIndex = (r, x) => r.UnsafeGetRefAs<byte>(x),
                RowSetColorIndex = (r, x, i) => r.UnsafeGetRefAs<byte>(x) = (byte)i,
                Palette = new Palette(PaletteGray8Alpha, WorkingColorSpace.Linear, default, 16)
            }],
            ["Gray8 sRGB", new CustomIndexedBitmapDataConfig
            {
                PixelFormat = new PixelFormatInfo(8) { Indexed = true },
                BackBufferIndependentPixelAccess = true,
                RowGetColorIndex = (r, x) => r.UnsafeGetRefAs<byte>(x),
                RowSetColorIndex = (r, x, i) => r.UnsafeGetRefAs<byte>(x) = (byte)i,
                Palette = Palette.Grayscale256(Color.Silver)
            }],
            ["Gray8 linear", new CustomIndexedBitmapDataConfig
            {
                PixelFormat = new PixelFormatInfo(8) { Indexed = true },
                BackBufferIndependentPixelAccess = true,
                RowGetColorIndex = (r, x) => r.UnsafeGetRefAs<byte>(x),
                RowSetColorIndex = (r, x, i) => r.UnsafeGetRefAs<byte>(x) = (byte)i,
                Palette = Palette.Grayscale256(WorkingColorSpace.Linear, Color.Silver)
            }],
        ];

        #endregion

        #region Methods

        [TestCaseSource(nameof(PaletteLookupTestSource))]
        public void PaletteLookupTest(string name, CustomIndexedBitmapDataConfig cfg)
        {
            if (!SaveToFile)
                Assert.Inconclusive($"This is a visual test. You need to enable {nameof(SaveToFile)} and check the generated results.");

            using var source = GetShieldIcon256();
            //using var source = GenerateAlphaGradientBitmapData(new Size(512, 256));
            var size = source.Size;

            var buffer = new byte[size.Height, (size.Width * cfg.PixelFormat.BitsPerPixel + 7) >> 3];
            using var target = BitmapDataFactory.CreateBitmapData(buffer, source.Width, cfg);
            source.CopyTo(target);
            SaveBitmapData(name, target);
        }

        #endregion
    }
}
