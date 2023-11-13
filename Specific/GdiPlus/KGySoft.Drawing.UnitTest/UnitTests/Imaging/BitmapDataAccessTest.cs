#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BitmapDataAccessTest.cs
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
using System.Drawing;
using System.Drawing.Imaging;

using KGySoft.Drawing.Imaging;

using NUnit.Framework;

#endregion

namespace KGySoft.Drawing.UnitTests.Imaging
{
    [TestFixture]
    public class BitmapDataAccessTest
    {
        #region Fields

        private static readonly object[][] setGetPixelTestSource =
        {
            new object[] { "32 bit ARGB Blue", KnownPixelFormat.Format32bppArgb, Color.Blue, Color.Blue, 0xFF_00_00_FF },
            new object[] { "32 bit ARGB Alpha 50%", KnownPixelFormat.Format32bppArgb, Color.FromArgb(128, Color.Blue), Color.FromArgb(128, Color.Blue), 0x80_00_00_FF },
            new object[] { "32 bit ARGB Transparent", KnownPixelFormat.Format32bppArgb, Color.Transparent, Color.Transparent, 0x00_FF_FF_FF },
            new object[] { "32 bit PARGB Blue", KnownPixelFormat.Format32bppPArgb, Color.Blue, Color.Blue, 0xFF_00_00_FF },
            new object[] { "32 bit PARGB Alpha 50%", KnownPixelFormat.Format32bppPArgb, Color.FromArgb(128, Color.Blue), Color.FromArgb(128, Color.Blue), 0x80_00_00_80 },
            new object[] { "32 bit PARGB Alpha 1", KnownPixelFormat.Format32bppPArgb, Color.FromArgb(1, Color.Blue), Color.FromArgb(1, Color.Blue), 0x01_00_00_01 },
            new object[] { "32 bit PARGB Alpha 254", KnownPixelFormat.Format32bppPArgb, Color.FromArgb(254, Color.Blue), Color.FromArgb(254, Color.Blue), 0xFE_00_00_FE },
            new object[] { "32 bit PARGB Transparent", KnownPixelFormat.Format32bppPArgb, Color.Transparent, Color.Empty, 0x00_00_00_00 },
            new object[] { "32 bit RGB Blue", KnownPixelFormat.Format32bppRgb, Color.Blue, Color.Blue, 0xFF_00_00_FF },
            new object[] { "32 bit RGB Alpha 50%", KnownPixelFormat.Format32bppRgb, Color.FromArgb(128, Color.Blue), Color.FromArgb(0, 0, 127), 0xFF_00_00_7F },
            new object[] { "32 bit RGB Transparent", KnownPixelFormat.Format32bppRgb, Color.Transparent, Color.Black, 0xFF_00_00_00 },
            new object[] { "24 bit RGB Blue", KnownPixelFormat.Format24bppRgb, Color.Blue, Color.Blue, 0x00_00_FF },
            new object[] { "24 bit RGB Transparent", KnownPixelFormat.Format24bppRgb, Color.Transparent, Color.Black, 0x00_00_00 },
            new object[] { "64 bit ARGB Blue", KnownPixelFormat.Format64bppArgb, Color.Blue, Color.Blue, 0x2000_0000_0000_2000 },
            new object[] { "64 bit ARGB Alpha 50%", KnownPixelFormat.Format64bppArgb, Color.FromArgb(128, Color.Blue), Color.FromArgb(128, Color.Blue), 0x1010_0000_0000_2000 },
            new object[] { "64 bit ARGB Transparent", KnownPixelFormat.Format64bppArgb, Color.Transparent, Color.Transparent, 0x0000_2000_2000_2000 },
            new object[] { "64 bit PARGB Blue", KnownPixelFormat.Format64bppPArgb, Color.Blue, Color.Blue, 0x2000_0000_0000_2000 },
            new object[] { "64 bit PARGB Alpha Blue 50%", KnownPixelFormat.Format64bppPArgb, Color.FromArgb(128, Color.Blue), Color.FromArgb(128, Color.Blue), 0x1010_0000_0000_1010 },
            new object[] { "64 bit PARGB Alpha Green 50%", KnownPixelFormat.Format64bppPArgb, Color.FromArgb(128, Color.Green), Color.FromArgb(128, Color.Green), 0x1010_0000_0377_0000 },
            new object[] { "64 bit PARGB Alpha 1", KnownPixelFormat.Format64bppPArgb, Color.FromArgb(1, Color.Blue), Color.FromArgb(1, Color.Blue), 0x0020_0000_0000_0020 },
            new object[] { "64 bit PARGB Alpha 254", KnownPixelFormat.Format64bppPArgb, Color.FromArgb(254, Color.Blue), Color.FromArgb(254, Color.Blue), 0x1FDF_0000_0000_1FDF },
            new object[] { "64 bit PARGB Transparent", KnownPixelFormat.Format64bppPArgb, Color.Transparent, Color.Empty, 0x0000_0000_0000_0000 },
            new object[] { "48 bit RGB Blue", KnownPixelFormat.Format48bppRgb, Color.Blue, Color.Blue, 0x0000_0000_2000 },
            new object[] { "48 bit RGB White", KnownPixelFormat.Format48bppRgb, Color.White, Color.White, 0x2000_2000_2000 },
            new object[] { "48 bit RGB Transparent", KnownPixelFormat.Format48bppRgb, Color.Transparent, Color.Black, 0x0000_0000_0000 },
            new object[] { "16 bit GrayScale White", KnownPixelFormat.Format16bppGrayScale, Color.White, Color.White, 0xFFFF },
            new object[] { "16 bit GrayScale Blue", KnownPixelFormat.Format16bppGrayScale, Color.Blue, Color.FromArgb(0x1D, 0x1D, 0x1D), 0x1D2E },
            new object[] { "16 bit RGB565 Blue", KnownPixelFormat.Format16bppRgb565, Color.Blue, Color.Blue, 0x001F },
            new object[] { "16 bit RGB565 Green", KnownPixelFormat.Format16bppRgb565, Color.Green, Color.FromArgb(0, 130, 0), 0x0400 },
            new object[] { "16 bit RGB565 Transparent", KnownPixelFormat.Format16bppRgb565, Color.Transparent, Color.Black, 0x0000 },
            new object[] { "16 bit RGB565 Empty", KnownPixelFormat.Format16bppRgb565, Color.Empty, Color.Black, 0x0000 },
            new object[] { "16 bit RGB555 Blue", KnownPixelFormat.Format16bppRgb555, Color.Blue, Color.Blue, 0x001F },
            new object[] { "16 bit RGB555 Green", KnownPixelFormat.Format16bppRgb555, Color.Green, Color.FromArgb(0, 132, 0), 0x0200 },
            new object[] { "16 bit RGB555 Transparent", KnownPixelFormat.Format16bppRgb555, Color.Transparent, Color.Black, 0x0000 },
            new object[] { "16 bit RGB555 Empty", KnownPixelFormat.Format16bppRgb555, Color.Empty, Color.Black, 0x0000 },
            new object[] { "16 bit ARGB1555 Blue", KnownPixelFormat.Format16bppArgb1555, Color.Blue, Color.Blue, 0x801F },
            new object[] { "16 bit ARGB1555 Green", KnownPixelFormat.Format16bppArgb1555, Color.Green, Color.FromArgb(0, 132, 0), 0x8200 },
            new object[] { "16 bit ARGB1555 Transparent", KnownPixelFormat.Format16bppArgb1555, Color.Transparent, Color.Transparent, 0x7FFF },
            new object[] { "16 bit ARGB1555 Empty", KnownPixelFormat.Format16bppArgb1555, Color.Empty, Color.Empty, 0x0000 },
            new object[] { "8 bit Indexed Blue", KnownPixelFormat.Format8bppIndexed, Color.Blue, Color.Blue, 12 },
            new object[] { "8 bit Indexed Blue 254", KnownPixelFormat.Format8bppIndexed, Color.FromArgb(0, 0, 254), Color.Blue, 12 },
            new object[] { "8 bit Indexed Transparent", KnownPixelFormat.Format8bppIndexed, Color.Transparent, Color.Empty, 16 },
            new object[] { "4 bit Indexed Blue", KnownPixelFormat.Format4bppIndexed, Color.Blue, Color.Blue, 12 },
            new object[] { "4 bit Indexed Blue 254", KnownPixelFormat.Format4bppIndexed, Color.FromArgb(0, 0, 254), Color.Blue, 12 },
            new object[] { "4 bit Indexed Transparent", KnownPixelFormat.Format4bppIndexed, Color.Transparent, Color.Black, 0 },
            new object[] { "1 bit Indexed Blue", KnownPixelFormat.Format1bppIndexed, Color.Blue, Color.Black, 0 },
            new object[] { "1 bit Indexed Blue 254", KnownPixelFormat.Format1bppIndexed, Color.FromArgb(0, 0, 254), Color.Black, 0 },
            new object[] { "1 bit Indexed Lime", KnownPixelFormat.Format1bppIndexed, Color.Lime, Color.White, 1 },
            new object[] { "1 bit Indexed Transparent", KnownPixelFormat.Format1bppIndexed, Color.Transparent, Color.Black, 0 },
        };

        #endregion

        #region Methods

        #region Static Methods

        private static long GetRawValue(KnownPixelFormat pixelFormat, IReadWriteBitmapData bitmapData)
        {
            IReadWriteBitmapDataRow row = bitmapData.FirstRow;
            return new PixelFormatInfo(pixelFormat).BitsPerPixel switch
            {
                64 => row.ReadRaw<long>(0),
                48 => row.ReadRaw<uint>(0) | ((long)row.ReadRaw<ushort>(2) << 32),
                32 => row.ReadRaw<uint>(0),
                24 => row.ReadRaw<ushort>(0) | ((long)row.ReadRaw<byte>(2) << 16),
                16 => row.ReadRaw<ushort>(0),
                8 => row.ReadRaw<byte>(0),
                4 => row.ReadRaw<byte>(0) >> 4,
                1 => row.ReadRaw<byte>(0) >> 7,
                _ => throw new InvalidOperationException($"Unexpected pixel format: {pixelFormat}")
            };
        }

        private static unsafe long GetRawValueNative(PixelFormat pixelFormat, IntPtr ptr)
        {
            return pixelFormat.ToBitsPerPixel() switch
            {
                64 => *(long*)ptr,
                48 => *(uint*)ptr | ((long)(((ushort*)ptr)[2]) << 32),
                32 => *(uint*)ptr,
                24 => *(ushort*)ptr | (long)(((byte*)ptr)[2] << 16),
                16 => *(ushort*)ptr,
                8 => *(byte*)ptr,
                4 => *(byte*)ptr >> 4,
                1 => *(byte*)ptr >> 7,
                _ => throw new InvalidOperationException($"Unexpected pixel format: {pixelFormat}")
            };
        }

        #endregion

        #region Instance Methods

        [TestCaseSource(nameof(setGetPixelTestSource))]
        public void SetGetPixelTest(string testName, KnownPixelFormat pixelFormat, Color testColor, Color expectedResult, long expectedRawValueNative)
        {
            Color actualColor;
            long actualRawValue;

            Console.WriteLine($"{testName}: {pixelFormat} + {testColor}{Environment.NewLine}");

            var gdiPixelFormat = pixelFormat.ToPixelFormat();
            if (gdiPixelFormat.IsSupportedNatively())
            {
                using Bitmap bmp = new Bitmap(1, 1, gdiPixelFormat);

                // Reference test by Set/GetPixel
                try
                {
                    Console.Write("Bitmap.SetPixel/GetPixel: ");
                    bmp.SetPixel(0, 0, testColor);
                    actualColor = bmp.GetPixel(0, 0);
                    Console.WriteLine($"{expectedResult} vs. {actualColor} ({(expectedResult.ToArgb() == actualColor.ToArgb() ? "OK" : "Fail")})");
                    BitmapData data = bmp.LockBits(new Rectangle(0, 0, 1, 1), ImageLockMode.ReadOnly, gdiPixelFormat);
                    try
                    {
                        actualRawValue = GetRawValueNative(gdiPixelFormat, data.Scan0);
                        Console.WriteLine($"  Expected vs. actual raw value: {expectedRawValueNative:X8} vs. {actualRawValue:X8} ({(expectedRawValueNative == actualRawValue ? "OK" : "Fail")}){Environment.NewLine}");
                    }
                    finally
                    {
                        bmp.UnlockBits(data);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }

                using (IReadWriteBitmapData nativeBitmapData = bmp.GetReadWriteBitmapData())
                {
                    // by Accessor Set/GetPixel
                    Console.Write("SetPixel/GetPixel native accessor: ");
                    nativeBitmapData.SetPixel(0, 0, testColor);
                    actualColor = nativeBitmapData.GetPixel(0, 0);
                    Console.WriteLine($"{expectedResult} vs. {actualColor} ({(expectedResult.ToArgb() == actualColor.ToArgb() ? "OK" : "Fail")})");
                    Assert.AreEqual(expectedResult.ToArgb(), actualColor.ToArgb());

                    actualRawValue = GetRawValue(pixelFormat, nativeBitmapData);
                    Console.WriteLine($"  Expected vs. actual raw value: {expectedRawValueNative:X8} vs. {actualRawValue:X8} ({(expectedRawValueNative == actualRawValue ? "OK" : "Fail")})");
                    if (pixelFormat == nativeBitmapData.PixelFormat.ToKnownPixelFormat()) // can differ in Linux for 16 bpp formats
                        Assert.AreEqual(expectedRawValueNative, actualRawValue);

                    // by indexer
                    nativeBitmapData[0][0] = new Color32(testColor);
                    Assert.AreEqual(expectedResult.ToArgb(), nativeBitmapData[0][0].ToArgb());
                    if (pixelFormat == nativeBitmapData.PixelFormat.ToKnownPixelFormat()) // can differ in Linux for 16 bpp formats
                        Assert.AreEqual(expectedRawValueNative, GetRawValue(pixelFormat, nativeBitmapData));

                    // as Color64
                    nativeBitmapData.SetColor64(0, 0, testColor.ToColor64());
                    Assert.AreEqual(expectedResult.ToColor64().ToColor32(), nativeBitmapData.GetColor64(0, 0).ToColor32());

                    // as PColor64
                    nativeBitmapData.SetPColor64(0, 0, testColor.ToPColor64());
                    Assert.AreEqual(expectedResult.ToPColor64().ToColor32(), nativeBitmapData.GetPColor64(0, 0).ToColor32());

                    // as ColorF
                    nativeBitmapData.SetColorF(0, 0, testColor.ToColorF());
                    Assert.AreEqual(expectedResult.ToColorF().ToColor32(), nativeBitmapData.GetColorF(0, 0).ToColor32());

                    // as PColorF
                    nativeBitmapData.SetPColorF(0, 0, testColor.ToPColorF());
                    Assert.AreEqual(expectedResult.ToPColorF().ToColor32(), nativeBitmapData.GetPColorF(0, 0).ToColor32());
                }
            }
        }

        [TestCase(PixelFormat.Format16bppGrayScale)]
        [TestCase(PixelFormat.Format24bppRgb)]
        [TestCase(PixelFormat.Format32bppRgb)]
        [TestCase(PixelFormat.Format32bppArgb)]
        [TestCase(PixelFormat.Format32bppPArgb)]
        [TestCase(PixelFormat.Format48bppRgb)]
        [TestCase(PixelFormat.Format64bppArgb)]
        [TestCase(PixelFormat.Format64bppPArgb)]
        public void SetGetAlphaPixelTest(PixelFormat pixelFormat)
        {
            #region Local Methods

            static void AreEqual(Color32 c1, Color32 c2, byte tolerance = 0)
            {
                Assert.IsTrue(c1.A == 0 && c2.A == 0 || c1.TolerantEquals(c2, tolerance), $"{c1} vs. {c2}");
            }

            #endregion

            Console.WriteLine(pixelFormat);

            if (!pixelFormat.IsSupportedNatively())
                Assert.Inconclusive($"PixelFormat {pixelFormat} not supported on current platform");

            var baseColor = Color.FromArgb(0x80, 0xFF, 0x40);

            using Bitmap bmp = new Bitmap(1, 1, pixelFormat);

            foreach (byte a in new byte[] { 0, 1, 127, 128, 129, 254, 255 })
            {
                Color testColor = Color.FromArgb(a, baseColor);
                if (!pixelFormat.HasAlpha())
                    testColor = testColor.ToColor32().Blend(Color.Black);
                if (pixelFormat.GetInfo().Grayscale)
                    testColor = testColor.ToColor32().ToGray();

                Console.WriteLine();
                Console.WriteLine($"Test color: {testColor}");

                if (a == 1 && pixelFormat.GetInfo().HasPremultipliedAlpha)
                {
                    Console.WriteLine($"Skipping a={a} because precision is too low for pixel format {pixelFormat}");
                    continue;
                }

                try
                {
                    Console.Write("Bitmap.SetPixel/GetPixel: ");
                    bmp.SetPixel(0, 0, testColor);
                    Color actualColor = bmp.GetPixel(0, 0);
                    Console.WriteLine($"{testColor} vs. {actualColor} ({(testColor.ToArgb() == actualColor.ToArgb() ? "OK" : "Fail")})");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }

                using (IReadWriteBitmapData bitmapData = bmp.GetReadWriteBitmapData())
                {
                    // by Accessor Set/GetPixel
                    Console.Write("BitmapData.SetPixel/GetPixel: ");
                    bitmapData.SetPixel(0, 0, testColor);
                    Color actualColor = bitmapData.GetPixel(0, 0);
                    Console.WriteLine($"{testColor} vs. {actualColor} ({(testColor.ToArgb() == actualColor.ToArgb() ? "OK" : "Fail")})");
                    AreEqual(testColor.ToColor32(), actualColor.ToColor32(), (byte)(pixelFormat.GetInfo().HasPremultipliedAlpha ? 1 : 0));

                    IReadWriteBitmapDataRow row = bitmapData[0];

                    // as Color32
                    Console.Write("Color32: ");
                    Color32 testColor32 = testColor;
                    row[0] = testColor32;
                    Color32 actualColor32 = row[0];
                    Console.WriteLine($"{testColor32} vs. {actualColor32} ({(testColor32 == actualColor32 ? "OK" : "Fail")})");
                    AreEqual(testColor32, actualColor32, (byte)(pixelFormat.GetInfo().HasPremultipliedAlpha ? 1 : 0));

                    // as Color64
                    Console.Write("Color64: ");
                    Color64 testColor64 = testColor.ToColor64();
                    row.SetColor64(0, testColor64);
                    Color64 actualColor64 = row.GetColor64(0);
                    Console.WriteLine($"{testColor64} vs. {actualColor64} ({(testColor64 == actualColor64 ? "Same" : "Diff")})");
                    AreEqual(testColor64.ToColor32(), actualColor64.ToColor32(), (byte)(pixelFormat.GetInfo().HasPremultipliedAlpha ? 1 : 0));

                    // as PColor64
                    Console.Write("PColor64: ");
                    PColor64 testPColor64 = testColor.ToPColor64();
                    row.SetPColor64(0, testPColor64);
                    PColor64 actualPColor64 = row.GetPColor64(0);
                    Console.WriteLine($"{testPColor64} vs. {actualPColor64} ({(testPColor64 == actualPColor64 ? "Same" : "Diff")})");
                    AreEqual(testPColor64.ToColor32(), actualPColor64.ToColor32(), (byte)(pixelFormat.GetInfo().HasPremultipliedAlpha ? 2 : 1));

                    // as ColorF
                    Console.Write("ColorF: ");
                    ColorF testColorF = testColor.ToColorF();
                    row.SetColorF(0, testColorF);
                    ColorF actualColorF = row.GetColorF(0);
                    Console.WriteLine($"{testColorF} vs. {actualColorF} ({(testColorF == actualColorF ? "Same" : "Diff")})");
                    AreEqual(testColorF.ToColor32(), actualColorF.ToColor32(), (byte)(pixelFormat.GetInfo().HasPremultipliedAlpha ? 1 : 0));

                    // as PColorF
                    Console.Write("PColorF: ");
                    PColorF testPColorF = testColor.ToPColorF();
                    row.SetPColorF(0, testPColorF);
                    PColorF actualPColorF = row.GetPColorF(0);
                    Console.WriteLine($"{testPColorF} vs. {actualPColorF} ({(testPColorF == actualPColorF ? "Same" : "Diff")})");
                    AreEqual(testPColorF.ToColor32(), actualPColorF.ToColor32(), (byte)(pixelFormat.GetInfo().HasPremultipliedAlpha ? 1 : 0));
                }
            }
        }

        #endregion

        #endregion
    }
}
