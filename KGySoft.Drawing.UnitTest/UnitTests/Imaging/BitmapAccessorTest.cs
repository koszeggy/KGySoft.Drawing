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
using KGySoft.CoreLibraries;
using KGySoft.Diagnostics;
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
            new object[] { "24 bit RGB32 Blue", PixelFormat.Format24bppRgb, Color.Blue, Color.Blue, 0x00_00_FF },
            new object[] { "24 bit RGB32 Transparent", PixelFormat.Format24bppRgb, Color.Transparent, Color.White, 0xFF_FF_FF },
            new object[] { "64 bit ARGB Blue", PixelFormat.Format64bppArgb, Color.Blue, Color.Blue, 0x2000_0000_0000_2000 },
            new object[] { "64 bit ARGB Alpha 50%", PixelFormat.Format64bppArgb, Color.FromArgb(128, Color.Blue), Color.FromArgb(128, Color.Blue), 0x1010_0000_0000_2000 },
            new object[] { "64 bit ARGB Transparent", PixelFormat.Format64bppArgb, Color.Transparent, Color.Transparent, 0x0000_2000_2000_2000 },
            new object[] { "64 bit PARGB Blue", PixelFormat.Format64bppPArgb, Color.Blue, Color.Blue, 0x2000_0000_0000_2000 },
            new object[] { "64 bit PARGB Alpha Blue 50%", PixelFormat.Format64bppPArgb, Color.FromArgb(128, Color.Blue), Color.FromArgb(128, Color.Blue), 0x1010_0000_0000_1010 },
            new object[] { "64 bit PARGB Alpha Green 50%", PixelFormat.Format64bppPArgb, Color.FromArgb(128, Color.Green), Color.FromArgb(128, Color.Green), 0x1010_0000_0377_0000 },
            new object[] { "64 bit PARGB Alpha 1", PixelFormat.Format64bppPArgb, Color.FromArgb(1, Color.Blue), Color.FromArgb(0, Color.Blue), 0x0020_0000_0000_0020 },
            new object[] { "64 bit PARGB Alpha 254", PixelFormat.Format64bppPArgb, Color.FromArgb(254, Color.Blue), Color.FromArgb(254, Color.Blue), 0x1FDF_0000_0000_1FDF },
            new object[] { "64 bit PARGB Transparent", PixelFormat.Format64bppPArgb, Color.Transparent, Color.Empty, 0x0000_0000_0000_0000 },
            new object[] { "48 bit RGB32 Blue", PixelFormat.Format48bppRgb, Color.Blue, Color.Blue, 0x0000_0000_2000 },
            new object[] { "48 bit RGB32 Transparent", PixelFormat.Format48bppRgb, Color.Transparent, Color.White, 0x2000_2000_2000 },
        };

        #endregion

        #region Methods

        #region Static Methods

        private static unsafe long GetRawValue(PixelFormat pixelFormat, IntPtr ptr)
        {
            int size = pixelFormat.ToBitsPerPixel() >> 3;
            switch (size)
            {
                case 8:
                    return *(long*)ptr;
                case 6:
                    return *(uint*)ptr | ((long)(((ushort*)ptr)[2]) << 32);
                case 4:
                    return *(uint*)ptr;
                case 3:
                    return *(ushort*)ptr | (long)(((byte*)ptr)[2] << 16);
                case 2:
                    return *(ushort*)ptr;
                case 1:
                    return *(byte*)ptr;
                default:
                    throw new InvalidOperationException($"Unexpected size: {size}");
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

            using (IBitmapDataAccessor accessor = bmp.GetBitmapDataAccessor(ImageLockMode.ReadWrite))
            {
                // by Accessor Set/GetPixel
                Console.Write("IBitmapDataAccessor.SetPixel/GetPixel: ");
                accessor.SetPixel(0, 0, testColor);
                actualColor = accessor.GetPixel(0, 0);
                Console.WriteLine($"{expectedResult} vs. {actualColor} ({(expectedResult.ToArgb() == actualColor.ToArgb() ? "OK" : "Fail")})");
                Assert.AreEqual(expectedResult.ToArgb(), actualColor.ToArgb());

                actualRawValue = GetRawValue(pixelFormat, accessor.Scan0);
                Console.WriteLine($"  Expected vs. actual raw value: {expectedRawValue:X8} vs. {actualRawValue:X8} ({(expectedRawValue == actualRawValue ? "OK" : "Fail")})");
                Assert.AreEqual(expectedRawValue, actualRawValue);

                // by indexer
                accessor[0][0] = new Color32(testColor);
                Assert.AreEqual(expectedResult.ToArgb(), accessor[0][0].ToArgb());
                Assert.AreEqual(expectedRawValue, GetRawValue(pixelFormat, accessor.Scan0));
            }
        }

        [Test]
        public unsafe void Argb64Test()
        {
            var pixelFormat = PixelFormat.Format64bppPArgb;
            using var bmp = new Bitmap(256, 1, pixelFormat);
            Console.WriteLine(bmp.PixelFormat);
            for (int i = 0; i < 256; i++)
            {
                bmp.SetPixel(i, 0, Color.FromArgb(i, Color.White));
            }

            var data = bmp.LockBits(new Rectangle(0, 0, 256, 1), ImageLockMode.ReadOnly, pixelFormat);
            try
            {
                Color64* p = (Color64*)data.Scan0;
                for (int i = 0; i < 256; i++)
                {
                    Console.WriteLine($"{i:D3} - {p[i]}");
                }
            }
            finally
            {
                bmp.UnlockBits(data);
            }

            using var bmp32 = new Bitmap(bmp);
            Console.WriteLine(bmp32.PixelFormat);
            var data32 = bmp32.LockBits(new Rectangle(0, 0, 256, 1), ImageLockMode.ReadOnly, bmp32.PixelFormat);
            try
            {
                Color32* p = (Color32*)data32.Scan0;
                for (int i = 0; i < 256; i++)
                    Console.WriteLine($"{i:D3} - {p[i]}");
            }
            finally
            {
                bmp32.UnlockBits(data32);
            }



        }

        /*
         var bmp = new Bitmap(5, 1, PixelFormat.Format64bppArgb);
                    bmp.SetPixel(0, 0, Color.Black);
                    bmp.SetPixel(1, 0, Color.Red);
                    bmp.SetPixel(2, 0, Color.Green);
                    bmp.SetPixel(3, 0, Color.Blue);
                    bmp.SetPixel(4, 0, Color.White);
                    image = bmp;
*/
        #endregion

        #endregion
    }
}
