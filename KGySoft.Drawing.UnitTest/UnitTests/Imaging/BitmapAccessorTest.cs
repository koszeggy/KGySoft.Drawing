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

        private static readonly object[][] bitmapAccessor32TestSource =
        {
            new object[] { "ARGB32 Blue", PixelFormat.Format32bppArgb, Color.Blue, Color.Blue, 0xFF_00_00_FFU },
            new object[] { "ARGB32 Alpha 50%", PixelFormat.Format32bppArgb, Color.FromArgb(128, Color.Blue), Color.FromArgb(128, Color.Blue), 0x80_00_00_FFU },
            new object[] { "ARGB32 Transparent", PixelFormat.Format32bppArgb, Color.Transparent, Color.Transparent, 0x00_FF_FF_FFU },
            new object[] { "PARGB32 Blue", PixelFormat.Format32bppPArgb, Color.Blue, Color.Blue, 0xFF_00_00_FFU },
            new object[] { "PARGB32 Alpha 50%", PixelFormat.Format32bppPArgb, Color.FromArgb(128, Color.Blue), Color.FromArgb(128, Color.Blue), 0x80_00_00_80U },
            new object[] { "PARGB32 Alpha 1", PixelFormat.Format32bppPArgb, Color.FromArgb(1, Color.Blue), Color.FromArgb(1, Color.Blue), 0x01_00_00_01U },
            new object[] { "PARGB32 Alpha 254", PixelFormat.Format32bppPArgb, Color.FromArgb(254, Color.Blue), Color.FromArgb(254, Color.Blue), 0xFE_00_00_FEU },
            new object[] { "PARGB32 Transparent", PixelFormat.Format32bppPArgb, Color.Transparent, Color.Empty, 0x00_00_00_00U },
        };

        #endregion

        #region Methods

        [TestCaseSource(nameof(bitmapAccessor32TestSource))]
        public unsafe void BitmapAccessor32Test(string testName, PixelFormat pixelFormat, Color testColor, Color expectedResult, uint expectedRawValue)
        {
            Color actualColor;
            uint actualRawValue;

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
                    actualRawValue = *(uint*)data.Scan0;
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

            using IBitmapDataAccessor accessor = bmp.GetBitmapDataAccessor(ImageLockMode.ReadWrite);

            // by Accessor Set/GetPixel
            Console.Write("IBitmapDataAccessor.SetPixel/GetPixel: ");
            accessor.SetPixel(0, 0, testColor);
            actualColor = accessor.GetPixel(0, 0);
            Console.WriteLine($"{expectedResult} vs. {actualColor} ({(expectedResult.ToArgb() == actualColor.ToArgb() ? "OK" : "Fail")})");
            Assert.AreEqual(expectedResult.ToArgb(), actualColor.ToArgb());

            actualRawValue = *(uint*)accessor.Scan0;
            Console.WriteLine($"  Expected vs. actual raw value: {expectedRawValue:X8} vs. {actualRawValue:X8} ({(expectedRawValue == actualRawValue ? "OK" : "Fail")})");
            Assert.AreEqual(expectedRawValue, *(uint*)accessor.Scan0);

            // by indexer
            accessor[0][0] = testColor;
            Assert.AreEqual(expectedResult.ToArgb(), accessor[0][0].ToArgb());
            Assert.AreEqual(expectedRawValue, *(uint*)accessor.Scan0);

            // by row/Color32
            var row = accessor.FirstRow;
            row.SetPixelColor(0, testColor);
            Assert.AreEqual(Color32.FromColor(expectedResult), row.GetPixelColor32(0));
            Assert.AreEqual(expectedRawValue, *(uint*)accessor.Scan0);
        }

        #endregion
    }
}
