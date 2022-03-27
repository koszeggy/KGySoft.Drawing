#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: WriteableBitmapExtensionsTest.cs
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
using System.IO;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using KGySoft.CoreLibraries;
using KGySoft.Drawing.Imaging;
using KGySoft.Drawing.Wpf;
using KGySoft.Reflection;

using NUnit.Framework;

#endregion

namespace KGySoft.Drawing.UnitTests
{
    [TestFixture]
    public class WriteableBitmapExtensionsTest : TestBase
    {
        #region Fields

        private static Color testColor = Color.FromRgb(0x80, 0xFF, 0x40);
        private static Color testColorAlpha = Color.FromArgb(0x80, 0x80, 0xFF, 0x40);
        private static Color testColorBlended = Color.FromArgb(0xFF, 0x40, 0x80, 0x20);

        private static object[][] setGetPixelTestSource =
        {
            new object[] { "BGRA32", PixelFormats.Bgra32, testColor, testColor, 0xFF_80_FF_40 },
            new object[] { "BGRA32 Alpha", PixelFormats.Bgra32, testColorAlpha, testColorAlpha, 0x80_80_FF_40 },
            new object[] { "BGRA32 Transparent", PixelFormats.Bgra32, Colors.Transparent, Colors.Transparent, 0x00_FF_FF_FF },

            new object[] { "PBGRA32", PixelFormats.Pbgra32, testColor, testColor, 0xFF_80_FF_40 },
            new object[] { "PBGRA32 Alpha 50%", PixelFormats.Pbgra32, testColorAlpha, testColorAlpha, 0x80_40_80_20 },
            new object[] { "PBGRA32 Transparent", PixelFormats.Pbgra32, Colors.Transparent, default(Color), 0x00_00_00_00 },
            new object[] { "PBGRA32 Alpha 1", PixelFormats.Pbgra32, Color.FromArgb(1, 0, 0, 255), Color.FromArgb(1, 0, 0, 255), 0x01_00_00_01 },
            new object[] { "PBGRA32 Alpha 254", PixelFormats.Pbgra32, Color.FromArgb(254, 0, 0, 255), Color.FromArgb(254, 0, 0, 255), 0xFE_00_00_FE },

            new object[] { "BGR32", PixelFormats.Bgr32, testColor, testColor, 0xFF_80_FF_40 },
            new object[] { "BGR32 Alpha", PixelFormats.Bgr32, testColorAlpha, testColorBlended, 0xFF_40_80_20 },
            new object[] { "BGR32 Transparent", PixelFormats.Bgr32, Colors.Transparent, Colors.Black, 0xFF_00_00_00 },

            new object[] { "BGR24", PixelFormats.Bgr24, testColor, testColor, 0x80_FF_40 },
            new object[] { "BGR24 Alpha", PixelFormats.Bgr24, testColorAlpha, testColorBlended, 0x40_80_20 },
            new object[] { "BGR24 Transparent", PixelFormats.Bgr24, Colors.Transparent, Colors.Black, 0x00_00_00 },

            new object[] { "RGB24", PixelFormats.Rgb24, testColor, testColor, 0x40_FF_80 },
            new object[] { "RGB24 Alpha", PixelFormats.Rgb24, testColorAlpha, testColorBlended, 0x20_80_40 },
            new object[] { "RGB24 Transparent", PixelFormats.Rgb24, Colors.Transparent, Colors.Black, 0x00_00_00 },

            new object[] { "I1", PixelFormats.Indexed1, testColor, Colors.White, 1 },
            new object[] { "I1 Alpha", PixelFormats.Indexed1, testColorAlpha, Colors.Black, 0 },
            new object[] { "I1 Transparent", PixelFormats.Indexed1, Colors.Transparent, Colors.Black, 0 },

            new object[] { "I2", PixelFormats.Indexed2, testColor, Colors.Silver, 2 },
            new object[] { "I2 Alpha", PixelFormats.Indexed2, testColorAlpha, Colors.Gray, 1 },
            new object[] { "I2 Transparent", PixelFormats.Indexed2, Colors.Transparent, Colors.Black, 0 },

            new object[] { "I4", PixelFormats.Indexed4, testColor, Colors.Olive, 3 },
            new object[] { "I4 Alpha", PixelFormats.Indexed4, testColorAlpha, Colors.Green, 2 },
            new object[] { "I4 Transparent", PixelFormats.Indexed4, Colors.Transparent, Colors.Black, 0 },

            new object[] { "I8", PixelFormats.Indexed8, testColor, Color.FromRgb(0x99, 0xFF, 0x33), 179 },
            new object[] { "I8 Alpha", PixelFormats.Indexed8, testColorAlpha, Color.FromRgb(0x33, 0x99, 0x33), 95 },
            new object[] { "I8 Transparent", PixelFormats.Indexed8, Colors.Transparent, default(Color), 16 },

            new object[] { "BW", PixelFormats.BlackWhite, testColor, Colors.White, 1 },
            new object[] { "BW Alpha", PixelFormats.BlackWhite, testColorAlpha, Colors.Black, 0 },
            new object[] { "BW Transparent", PixelFormats.BlackWhite, Colors.Transparent, Colors.Black, 0 },

            new object[] { "Gray2", PixelFormats.Gray2, testColor, Color.FromRgb(0xAA, 0xAA, 0xAA), 2 },
            new object[] { "Gray2 Alpha", PixelFormats.Gray2, testColorAlpha, Color.FromRgb(0x55, 0x55, 0x55), 1 },
            new object[] { "Gray2 Transparent", PixelFormats.Gray2, Colors.Transparent, Colors.Black, 0 },

            new object[] { "Gray4", PixelFormats.Gray4, testColor, Color.FromRgb(0xBB, 0xBB, 0xBB), 11 },
            new object[] { "Gray4 Alpha", PixelFormats.Gray4, testColorAlpha, Color.FromRgb(0x66, 0x66, 0x66), 6 },
            new object[] { "Gray4 Transparent", PixelFormats.Gray4, Colors.Transparent, Colors.Black, 0 },

            new object[] { "Gray8", PixelFormats.Gray8, testColor, Color.FromRgb(0xC3, 0xC3, 0xC3), 0xC3 },
            new object[] { "Gray8 Alpha", PixelFormats.Gray8, testColorAlpha, Color.FromRgb(0x61, 0x61, 0x61), 0x61 },
            new object[] { "Gray8 Transparent", PixelFormats.Gray8, Colors.Transparent, Colors.Black, 0 },

            new object[] { "BGR555", PixelFormats.Bgr555, testColor, Color.FromRgb(0b10000100, 0b11111111, 0b01000010), 0b10000_11111_01000 },
            new object[] { "BGR555 Alpha", PixelFormats.Bgr555, testColorAlpha, Color.FromRgb(0b01000010, 0b10000100, 0b00100001), 0b01000_10000_00100 },
            new object[] { "BGR555 Transparent", PixelFormats.Bgr555, Colors.Transparent, Colors.Black, 0 },

            new object[] { "BGR565", PixelFormats.Bgr565, testColor, Color.FromRgb(0b10000100, 0b11111111, 0b01000010), 0b10000_111111_01000 },
            new object[] { "BGR565 Alpha", PixelFormats.Bgr565, testColorAlpha, Color.FromRgb(0b01000010, 0b10000010, 0b00100001), 0b01000_100000_00100 },
            new object[] { "BGR565 Transparent", PixelFormats.Bgr565, Colors.Transparent, Colors.Black, 0 },

            new object[] { "Gray16", PixelFormats.Gray16, testColor, Color.FromRgb(0xC4, 0xC4, 0xC4), 0xC404 },
            new object[] { "Gray16 Alpha", PixelFormats.Gray16, testColorAlpha, Color.FromRgb(0x62, 0x62, 0x62), 0x624D },
            new object[] { "Gray16 Transparent", PixelFormats.Gray16, Colors.Transparent, Colors.Black, 0 },

            new object[] { "Gray32", PixelFormats.Gray32Float, testColor, Color.FromRgb(0xC3, 0xC3, 0xC3), 0x3F0C1C96 },
            new object[] { "Gray32 Alpha", PixelFormats.Gray32Float, testColorAlpha, Color.FromRgb(0x62, 0x62, 0x62), 0x3DF9B636 },
            new object[] { "Gray32 Transparent", PixelFormats.Gray32Float, Colors.Transparent, Colors.Black, 0 },

            new object[] { "BGR101010", PixelFormats.Bgr101010, testColor, testColor, 0b1000000010_1111111111_0100000001 },
            new object[] { "BGR101010 Alpha", PixelFormats.Bgr101010, testColorAlpha, testColorBlended, 0b0100000001_1000000010_0010000000 },
            new object[] { "BGR101010 Transparent", PixelFormats.Bgr101010, Colors.Transparent, Colors.Black, 0 },

            new object[] { "RGB48", PixelFormats.Rgb48, testColor, testColor, 0x4040_FFFF_8080 },
            new object[] { "RGB48 Alpha", PixelFormats.Rgb48, testColorAlpha, testColorBlended, 0x2020_8080_4040 },
            new object[] { "RGB48 Transparent", PixelFormats.Rgb48, Colors.Transparent, Colors.Black, 0 },

            new object[] { "RGBA64", PixelFormats.Rgba64, testColor, testColor, unchecked((long)0xFFFF_4040_FFFF_8080) },
            new object[] { "RGBA64 Alpha", PixelFormats.Rgba64, testColorAlpha, testColorAlpha, unchecked((long)0x8080_4040_FFFF_8080) },
            new object[] { "RGBA64 Transparent", PixelFormats.Rgba64, Colors.Transparent, Colors.Transparent, 0x0000_FFFF_FFFF_FFFF },

            new object[] { "PRGBA64", PixelFormats.Prgba64, testColor, testColor, unchecked((long)0xFFFF_4040_FFFF_8080) },
            new object[] { "PRGBA64 Alpha", PixelFormats.Prgba64, testColorAlpha, testColorAlpha, unchecked((long)0x8080_2040_8080_4080) },
            new object[] { "PRGBA64 Transparent", PixelFormats.Prgba64, Colors.Transparent, default(Color), 0x0000_0000_0000_0000 },

            new object[] { "RGBA128", PixelFormats.Rgba128Float, testColor, testColor, 0x3F800000_3E5D0A8B /*only R and G as float */ },
            new object[] { "RGBA128 Alpha", PixelFormats.Rgba128Float, testColorAlpha, testColorAlpha, 0x3F800000_3E5D0A8B /*only R and G as float */ },
            new object[] { "RGBA128 Transparent", PixelFormats.Rgba128Float, Colors.Transparent, Colors.Transparent, 0x3F800000_3F800000 /*only R and G as float */ },

            new object[] { "PRGBA128", PixelFormats.Prgba128Float, testColor, testColor, 0x3F800000_3E5D0A8B /* only R and G as float */ },
            new object[] { "PRGBA128 Alpha", PixelFormats.Prgba128Float, testColorAlpha, testColorAlpha, 0x3F008081_3DDDE874 /* only R and G as float */ },
            new object[] { "PRGBA128 Transparent", PixelFormats.Prgba128Float, Colors.Transparent, default(Color), 0x00000000_00000000 /* only R and G as float */ },

            new object[] { "RGB128", PixelFormats.Rgb128Float, testColor, testColor, 0x3F800000_3E5D0A8B /* only R and G as float */ },
            new object[] { "RGB128 Alpha", PixelFormats.Rgb128Float, testColorAlpha, testColorBlended, 0x3E5D0A8B_3D51FFEF /* only R and G as float */ },
            new object[] { "RGB128 Transparent", PixelFormats.Rgb128Float, Colors.Transparent, Colors.Black, 0x00000000_00000000 /* only R and G as float */ },

            new object[] { "CMYK32", PixelFormats.Cmyk32, testColor, testColor, 0x00_BF_00_7E },
            new object[] { "CMYK32 Alpha", PixelFormats.Cmyk32, testColorAlpha, testColorBlended, 0x7E_BF_00_7F },
            new object[] { "CMYK32 Transparent", PixelFormats.Cmyk32, Colors.Transparent, Colors.Black, 0xFF_00_00_00 },
        };

        private static object[] wpfBehaviorTestSource =
        {
            PixelFormats.Default,
            PixelFormats.Indexed1,
            PixelFormats.Indexed2,
            PixelFormats.Indexed4,
            PixelFormats.Indexed8,
            PixelFormats.BlackWhite,
            PixelFormats.Gray2,
            PixelFormats.Gray4,
            PixelFormats.Gray8,
            PixelFormats.Bgr555,
            PixelFormats.Bgr565,
            PixelFormats.Gray16,
            PixelFormats.Bgr24,
            PixelFormats.Rgb24,
            PixelFormats.Bgr32,
            PixelFormats.Bgra32,
            PixelFormats.Pbgra32,
            PixelFormats.Gray32Float,
            PixelFormats.Bgr101010,
            PixelFormats.Rgb48,
            PixelFormats.Rgba64,
            PixelFormats.Prgba64,
            PixelFormats.Rgba128Float,
            PixelFormats.Prgba128Float,
            PixelFormats.Rgb128Float,
            PixelFormats.Cmyk32,
        };

        #endregion

        #region Methods

        #region Static Methods

        private static BitmapPalette GetDefaultPalette(PixelFormat pixelFormat)
        {
            //return null;
            var result = pixelFormat == PixelFormats.Indexed1 ? Palette.BlackAndWhite()
                : pixelFormat == PixelFormats.Indexed2 ? new Palette(new[] { Color32.FromGray(0), Color32.FromGray(0x80), Color32.FromGray(0xC0), Color32.FromGray(0xFF) })
                : pixelFormat == PixelFormats.Indexed4 ? Palette.SystemDefault4BppPalette()
                : pixelFormat == PixelFormats.Indexed8 ? Palette.SystemDefault8BppPalette()
                : null;
            return result == null ? null : new BitmapPalette(result.GetEntries().Select(c => Color.FromArgb(c.A, c.R, c.G, c.B)).ToArray());
        }

        private static long GetRawValue(PixelFormat pixelFormat, IReadWriteBitmapDataRow row)
        {
            return pixelFormat.BitsPerPixel switch
            {
                64 or 128 => row.ReadRaw<long>(0),
                48 => row.ReadRaw<uint>(0) | ((long)row.ReadRaw<ushort>(2) << 32),
                32 => row.ReadRaw<uint>(0),
                24 => row.ReadRaw<ushort>(0) | ((long)row.ReadRaw<byte>(2) << 16),
                16 => row.ReadRaw<ushort>(0),
                8 => row.ReadRaw<byte>(0),
                4 => row.ReadRaw<byte>(0) >> 4,
                2 => row.ReadRaw<byte>(0) >> 6,
                1 => row.ReadRaw<byte>(0) >> 7,
                _ => throw new InvalidOperationException($"Unexpected pixel format: {pixelFormat}")
            };
        }

        #endregion

        #region Instance Methods

        [TestCaseSource(nameof(setGetPixelTestSource))]
        public void SetGetPixelTest(string testName, PixelFormat pixelFormat, Color testColor, Color expectedResult, long expectedRawValue)
        {
            Console.WriteLine($"{testName}: {pixelFormat} + {testColor} => {expectedResult} (0x{expectedRawValue:X8})");

            var bmp = new WriteableBitmap(1, 1, 96, 96, pixelFormat, GetDefaultPalette(pixelFormat));
            Color32 expectedColor = expectedResult.ToColor32();
            using (IReadWriteBitmapData bitmapData = bmp.GetReadWriteBitmapData())
            {
                IReadWriteBitmapDataRow row = bitmapData.FirstRow;
                row[0] = testColor.ToColor32();
                Color32 actualColor = row[0];
                Assert.IsTrue(expectedColor.TolerantEquals(actualColor, 1), $"Expected vs. read color: {expectedColor} <=> {actualColor}");
                long actualRawValue = GetRawValue(pixelFormat, row);
                Assert.AreEqual(expectedRawValue, actualRawValue, $"Raw value {expectedRawValue:X8} was expected but it was {actualRawValue:X8}");
            }

            // The code above just tests self-consistency. To test whether WPF handles the color the same way we convert the bitmap in WPF
            // CMYK is handled by WPF oddly: it uses a non-reversible transformation (but above test provs that out transformation is 100% reversible)
            var converted = new WriteableBitmap(new FormatConvertedBitmap(bmp, PixelFormats.Bgra32, null, default));
            using IReadWriteBitmapData convertedBitmapData = converted.GetReadWriteBitmapData();
            Color32 convertedColor = convertedBitmapData[0][0];
            Assert.IsTrue(pixelFormat == PixelFormats.Cmyk32 || expectedColor.TolerantEquals(convertedColor, 1), $"Expected vs. converted color: {expectedColor} <=> {convertedColor}");
        }

        [Explicit]
        [TestCaseSource(nameof(wpfBehaviorTestSource))]
        public void WpfBehaviorTest(PixelFormat pixelFormat)
        {
            var source = new BitmapImage(new Uri(@"D:\Dokumentumok\Képek\Formats\System4BitColors.png"));
            //var source = new BitmapImage(new Uri(@"..\..\..\..\KGySoft.Drawing\Help\Images\AlphaGradient.png", UriKind.Relative));
            var bmp = new WriteableBitmap(source);
            //var bmp = new WriteableBitmap(1, 1, 96, 96, pixelFormat, GetDefaultPalette(pixelFormat));
            //using (IReadWriteBitmapData bitmapData = bmp.GetReadWriteBitmapData())
            //    bitmapData[0][0] = Color32.FromGray(128);


            var converted = new WriteableBitmap(new FormatConvertedBitmap(bmp, pixelFormat, GetDefaultPalette(pixelFormat), 0.5));
            //var converted = new WriteableBitmap(16, 1, 100, 100, pixelFormat, null);
            Console.WriteLine($"{converted.Format}: {converted.Format.BitsPerPixel}bpp {Reflector.GetProperty(converted.Format, "FormatFlags")}");
            Console.WriteLine("Masks:");
            var masks = converted.Format.Masks;
            foreach (PixelFormatChannelMask mask in masks)
                Console.WriteLine($"    {mask.Mask.Reverse().Select(b => b.ToString("X2")).Join("")}");
            var palette = converted.Palette?.Colors;
            if (palette != null)
            {
                Console.WriteLine($"Palette: {palette.Count} colors");
                for (int i = 0; i < palette.Count; i++)
                {
                    if (i % 16 == 0)
                    {
                        if (i > 0)
                            Console.WriteLine();
                        Console.Write("    ");
                    }

                    Console.Write(palette[i]);
                    if (i < palette.Count - 1)
                        Console.Write("; ");
                }

                Console.WriteLine();
            }

            return;
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(converted));
            var stream = new MemoryStream();
            encoder.Save(stream);
            SaveStream($"{pixelFormat}", stream, "png");
        }

        #endregion

        #endregion
    }
}
