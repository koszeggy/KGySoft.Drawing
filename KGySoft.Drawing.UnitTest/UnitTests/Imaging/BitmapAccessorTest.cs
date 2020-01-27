#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BitmapAccessorTest.cs
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
using KGySoft.Drawing.Imaging;

using NUnit.Framework;

#endregion

namespace KGySoft.Drawing.UnitTests.Imaging
{
    [TestFixture]
    public class BitmapAccessorTest
    {
        #region Fields

        private static readonly object[][] setGetPixelTestSource =
        {
            new object[] { "32 bit ARGB Blue", PixelFormat.Format32bppArgb, Color.Blue, Color.Blue, 0xFF_00_00_FF },
            new object[] { "32 bit ARGB Alpha 50%", PixelFormat.Format32bppArgb, Color.FromArgb(128, Color.Blue), Color.FromArgb(128, Color.Blue), 0x80_00_00_FF },
            new object[] { "32 bit ARGB Transparent", PixelFormat.Format32bppArgb, Color.Transparent, Color.Transparent, 0x00_FF_FF_FF },
            new object[] { "32 bit PARGB Blue", PixelFormat.Format32bppPArgb, Color.Blue, Color.Blue, 0xFF_00_00_FF },
            new object[] { "32 bit PARGB Alpha 50%", PixelFormat.Format32bppPArgb, Color.FromArgb(128, Color.Blue), Color.FromArgb(128, Color.Blue), 0x80_00_00_80 },
            new object[] { "32 bit PARGB Alpha 1", PixelFormat.Format32bppPArgb, Color.FromArgb(1, Color.Blue), Color.FromArgb(1, Color.Blue), 0x01_00_00_01 },
            new object[] { "32 bit PARGB Alpha 254", PixelFormat.Format32bppPArgb, Color.FromArgb(254, Color.Blue), Color.FromArgb(254, Color.Blue), 0xFE_00_00_FE },
            new object[] { "32 bit PARGB Transparent", PixelFormat.Format32bppPArgb, Color.Transparent, Color.Empty, 0x00_00_00_00 },
            new object[] { "32 bit RGB Blue", PixelFormat.Format32bppRgb, Color.Blue, Color.Blue, 0xFF_00_00_FF },
            new object[] { "32 bit RGB Alpha 50%", PixelFormat.Format32bppRgb, Color.FromArgb(128, Color.Blue), Color.FromArgb(0, 0, 128), 0xFF_00_00_80 },
            new object[] { "32 bit RGB Transparent", PixelFormat.Format32bppRgb, Color.Transparent, Color.Black, 0xFF_00_00_00 },
            new object[] { "24 bit RGB Blue", PixelFormat.Format24bppRgb, Color.Blue, Color.Blue, 0x00_00_FF },
            new object[] { "24 bit RGB Transparent", PixelFormat.Format24bppRgb, Color.Transparent, Color.Black, 0x00_00_00 },
            new object[] { "64 bit ARGB Blue", PixelFormat.Format64bppArgb, Color.Blue, Color.Blue, 0x2000_0000_0000_2000 },
            new object[] { "64 bit ARGB Alpha 50%", PixelFormat.Format64bppArgb, Color.FromArgb(128, Color.Blue), Color.FromArgb(128, Color.Blue), 0x1010_0000_0000_2000 },
            new object[] { "64 bit ARGB Transparent", PixelFormat.Format64bppArgb, Color.Transparent, Color.Transparent, 0x0000_2000_2000_2000 },
            new object[] { "64 bit PARGB Blue", PixelFormat.Format64bppPArgb, Color.Blue, Color.Blue, 0x2000_0000_0000_2000 },
            new object[] { "64 bit PARGB Alpha Blue 50%", PixelFormat.Format64bppPArgb, Color.FromArgb(128, Color.Blue), Color.FromArgb(128, Color.Blue), 0x1010_0000_0000_1010 },
            new object[] { "64 bit PARGB Alpha Green 50%", PixelFormat.Format64bppPArgb, Color.FromArgb(128, Color.Green), Color.FromArgb(128, Color.Green), 0x1010_0000_0377_0000 },
            new object[] { "64 bit PARGB Alpha 1", PixelFormat.Format64bppPArgb, Color.FromArgb(1, Color.Blue), Color.FromArgb(0, Color.Blue), 0x0020_0000_0000_0020 },
            new object[] { "64 bit PARGB Alpha 254", PixelFormat.Format64bppPArgb, Color.FromArgb(254, Color.Blue), Color.FromArgb(254, Color.Blue), 0x1FDF_0000_0000_1FDF },
            new object[] { "64 bit PARGB Transparent", PixelFormat.Format64bppPArgb, Color.Transparent, Color.Empty, 0x0000_0000_0000_0000 },
            new object[] { "48 bit RGB Blue", PixelFormat.Format48bppRgb, Color.Blue, Color.Blue, 0x0000_0000_2000 },
            new object[] { "48 bit RGB White", PixelFormat.Format48bppRgb, Color.White, Color.White, 0x2000_2000_2000 },
            new object[] { "48 bit RGB Transparent", PixelFormat.Format48bppRgb, Color.Transparent, Color.Black, 0x0000_0000_0000 },
            new object[] { "16 bit GrayScale White", PixelFormat.Format16bppGrayScale, Color.White, Color.White, 0xFFFF },
            new object[] { "16 bit GrayScale Blue", PixelFormat.Format16bppGrayScale, Color.Blue, Color.FromArgb(0x1D, 0x1D, 0x1D), 0x1D2E },
            new object[] { "16 bit RGB565 Blue", PixelFormat.Format16bppRgb565, Color.Blue, Color.Blue, 0x001F },
            new object[] { "16 bit RGB565 Green", PixelFormat.Format16bppRgb565, Color.Green, Color.FromArgb(0, 130, 0), 0x0400 },
            new object[] { "16 bit RGB565 Transparent", PixelFormat.Format16bppRgb565, Color.Transparent, Color.Black, 0x0000 },
            new object[] { "16 bit RGB565 Empty", PixelFormat.Format16bppRgb565, Color.Empty, Color.Black, 0x0000 },
            new object[] { "16 bit RGB555 Blue", PixelFormat.Format16bppRgb555, Color.Blue, Color.Blue, 0x001F },
            new object[] { "16 bit RGB555 Green", PixelFormat.Format16bppRgb555, Color.Green, Color.FromArgb(0, 132, 0), 0x0200 },
            new object[] { "16 bit RGB555 Transparent", PixelFormat.Format16bppRgb555, Color.Transparent, Color.Black, 0x0000 },
            new object[] { "16 bit RGB555 Empty", PixelFormat.Format16bppRgb555, Color.Empty, Color.Black, 0x0000 },
            new object[] { "16 bit ARGB1555 Blue", PixelFormat.Format16bppArgb1555, Color.Blue, Color.Blue, 0x801F },
            new object[] { "16 bit ARGB1555 Green", PixelFormat.Format16bppArgb1555, Color.Green, Color.FromArgb(0, 132, 0), 0x8200 },
            new object[] { "16 bit ARGB1555 Transparent", PixelFormat.Format16bppArgb1555, Color.Transparent, Color.Transparent, 0x7FFF },
            new object[] { "16 bit ARGB1555 Empty", PixelFormat.Format16bppArgb1555, Color.Empty, Color.Empty, 0x0000 },
            new object[] { "8 bit Indexed Blue", PixelFormat.Format8bppIndexed, Color.Blue, Color.Blue, 12 },
            new object[] { "8 bit Indexed Blue 254", PixelFormat.Format8bppIndexed, Color.FromArgb(0, 0, 254), Color.Blue, 12 },
            new object[] { "8 bit Indexed Transparent", PixelFormat.Format8bppIndexed, Color.Transparent, Color.Empty, 16 },
            new object[] { "4 bit Indexed Blue", PixelFormat.Format4bppIndexed, Color.Blue, Color.Blue, 12 },
            new object[] { "4 bit Indexed Blue 254", PixelFormat.Format4bppIndexed, Color.FromArgb(0, 0, 254), Color.Blue, 12 },
            new object[] { "4 bit Indexed Transparent", PixelFormat.Format4bppIndexed, Color.Transparent, Color.Black, 0 },
            new object[] { "1 bit Indexed Blue", PixelFormat.Format1bppIndexed, Color.Blue, Color.Black, 0 },
            new object[] { "1 bit Indexed Blue 254", PixelFormat.Format1bppIndexed, Color.FromArgb(0, 0, 254), Color.Black, 0 },
            new object[] { "1 bit Indexed Lime", PixelFormat.Format1bppIndexed, Color.Lime, Color.White, 1 },
            new object[] { "1 bit Indexed Transparent", PixelFormat.Format1bppIndexed, Color.Transparent, Color.Black, 0 },
        };

        #endregion

        #region Methods

        #region Static Methods

        private static unsafe long GetRawValue(PixelFormat pixelFormat, IntPtr ptr)
        {
            switch (pixelFormat.ToBitsPerPixel())
            {
                case 64:
                    return *(long*)ptr;
                case 48:
                    return *(uint*)ptr | ((long)(((ushort*)ptr)[2]) << 32);
                case 32:
                    return *(uint*)ptr;
                case 24:
                    return *(ushort*)ptr | (long)(((byte*)ptr)[2] << 16);
                case 16:
                    return *(ushort*)ptr;
                case 8:
                    return *(byte*)ptr;
                case 4:
                    return *(byte*)ptr >> 4;
                case 1:
                    return *(byte*)ptr >> 7;
                default:
                    throw new InvalidOperationException($"Unexpected pixel format: {pixelFormat}");
            }
        }

        #endregion

        #region Instance Methods

        [TestCaseSource(nameof(setGetPixelTestSource))]
        public void SetGetPixelTest(string testName, PixelFormat pixelFormat, Color testColor, Color expectedResult, long expectedRawValue)
        {
            Color actualColor;
            long actualRawValue;

            Console.WriteLine($"{testName}: {pixelFormat} + {testColor}{Environment.NewLine}");
            using Bitmap bmp = new Bitmap(1, 1, pixelFormat);

            // Reference test by Set/GetPixel
            try
            {
                Console.Write("Bitmap.SetPixel/GetPixel: ");
                bmp.SetPixel(0, 0, testColor);
                actualColor = bmp.GetPixel(0, 0);
                Console.WriteLine($"{expectedResult} vs. {actualColor} ({(expectedResult.ToArgb() == actualColor.ToArgb() ? "OK" : "Fail")})");
                var data = bmp.LockBits(new Rectangle(0, 0, 1, 1), ImageLockMode.ReadOnly, pixelFormat);
                try
                {
                    actualRawValue = GetRawValue(pixelFormat, data.Scan0);
                    Console.WriteLine($"  Expected vs. actual raw value: {expectedRawValue:X8} vs. {actualRawValue:X8} ({(expectedRawValue == actualRawValue ? "OK" : "Fail")}){Environment.NewLine}");
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

            using (BitmapDataAccessorBase accessor = BitmapDataAccessorFactory.CreateAccessor(bmp, ImageLockMode.ReadWrite))
            {
                // by Accessor Set/GetPixel
                Console.Write("BitmapDataAccessorBase.SetPixel/GetPixel: ");
                accessor.SetPixel(0, 0, testColor);
                actualColor = accessor.GetPixel(0, 0);
                Console.WriteLine($"{expectedResult} vs. {actualColor} ({(expectedResult.ToArgb() == actualColor.ToArgb() ? "OK" : "Fail")})");
                Assert.AreEqual(expectedResult.ToArgb(), actualColor.ToArgb());

                actualRawValue = GetRawValue(pixelFormat, accessor.Scan0);
                Console.WriteLine($"  Expected vs. actual raw value: {expectedRawValue:X8} vs. {actualRawValue:X8} ({(expectedRawValue == actualRawValue ? "OK" : "Fail")})");
                Assert.AreEqual(expectedRawValue, actualRawValue);

                // by indexer
                accessor.GetRow(0)[0] = new Color32(testColor);
                Assert.AreEqual(expectedResult.ToArgb(), accessor.GetRow(0)[0].ToArgb());
                Assert.AreEqual(expectedRawValue, GetRawValue(pixelFormat, accessor.Scan0));
            }
        }

        #endregion

        #endregion
    }
}
