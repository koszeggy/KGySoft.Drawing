#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BitmapExtensionsTest.cs
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
using KGySoft.Diagnostics;
using KGySoft.Drawing.WinApi;

using NUnit.Framework;

#endregion

namespace KGySoft.Drawing.UnitTests
{
    [TestFixture]
    public class BitmapExtensionsTest : TestBase
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

        [Test]
        public void ResizeTest()
        {
            using var bmpRef = Icons.Information.ExtractBitmap(new Size(256, 256));
            var newSize = new Size(256, 64);
            using var resizedNoAspectRatio = bmpRef.Resize(newSize, false);
            Assert.AreEqual(newSize, resizedNoAspectRatio.Size);
            SaveImage("NoAspectRatio", resizedNoAspectRatio);

            using var keepAspectRatioShrinkY = bmpRef.Resize(newSize, true);
            Assert.AreEqual(newSize, keepAspectRatioShrinkY.Size);
            SaveImage("KeepAspectRatioShrinkY", keepAspectRatioShrinkY);

            newSize = new Size(64, 256);
            using var keepAspectRatioShrinkX = bmpRef.Resize(newSize, true);
            Assert.AreEqual(newSize, keepAspectRatioShrinkX.Size);
            SaveImage("KeepAspectRatioShrinkX", keepAspectRatioShrinkX);

            newSize = new Size(300, 400);
            using var keepAspectRatioEnlargeX = bmpRef.Resize(newSize, true);
            Assert.AreEqual(newSize, keepAspectRatioEnlargeX.Size);
            SaveImage("KeepAspectRatioEnlargeX", keepAspectRatioEnlargeX);

            newSize = new Size(400, 300);
            using var keepAspectRatioEnlargeY = bmpRef.Resize(newSize, true);
            Assert.AreEqual(newSize, keepAspectRatioEnlargeY.Size);
            SaveImage("KeepAspectRatioEnlargeY", keepAspectRatioEnlargeY);
        }

        [Test]
        public void ExtractBitmapsTest()
        {
            AssertPlatformDependent(() => Assert.AreEqual(7, Icons.Information.ToMultiResBitmap().ExtractBitmaps().Length), PlatformID.Win32NT);
        }

        [Test]
        public void CloneCurrentFrameTest()
        {
            // Cloning a BMP (negative stride)
            using var bmp = Icons.Information.ExtractBitmap(new Size(64, 64), true);
            var clone = bmp.CloneCurrentFrame();
            Assert.IsTrue(bmp.EqualsByContent(clone));
            SaveImage("BmpClone", clone);

            // Cloning a PNG (positive stride)
            using var png = Icons.Information.ExtractBitmap(new Size(256, 256), true);
            clone = png.CloneCurrentFrame();
            Assert.IsTrue(png.EqualsByContent(clone));
            SaveImage("PngClone", clone);
        }

        [Test]
        public void GetColorsTest()
        {
            // 32 bit ARGB
            using var refBmp = Icons.Information.ToAlphaBitmap();
            var colors = refBmp.GetColors();
            Assert.LessOrEqual(colors.Length, refBmp.Width * refBmp.Height);
            SaveImage("32argb", refBmp);

            // 24 bit
            using var bmp24bpp = refBmp.ConvertPixelFormat(PixelFormat.Format24bppRgb);
            colors = bmp24bpp.GetColors();
            Assert.LessOrEqual(colors.Length, bmp24bpp.Width * bmp24bpp.Height);
            SaveImage("24rgb", bmp24bpp);

            // 48 bit
            if (OSUtils.IsWindows)
            {
                using var bmp48bpp = refBmp.ConvertPixelFormat(PixelFormat.Format48bppRgb);
                colors = bmp48bpp.GetColors();
                Assert.LessOrEqual(colors.Length, bmp48bpp.Width * bmp48bpp.Height);
                SaveImage("48rgb", bmp48bpp);

                // 64 bit
                using var bmp64bpp = refBmp.ConvertPixelFormat(PixelFormat.Format64bppArgb);
                colors = bmp64bpp.GetColors();
                Assert.LessOrEqual(colors.Length, bmp64bpp.Width * bmp64bpp.Height);
                SaveImage("64argb", bmp64bpp);
            }

            // 8 bit: returning actual palette
            using var bmp8bpp = refBmp.ConvertPixelFormat(PixelFormat.Format8bppIndexed);
            colors = bmp8bpp.GetColors();
            Assert.AreEqual(bmp8bpp.Palette.Entries.Length, colors.Length);
            SaveImage("8ind", bmp8bpp);
        }

        [Test]
        public void ToCursorHandleTest()
        {
            AssertPlatformDependent(() => Assert.AreNotEqual(IntPtr.Zero, (IntPtr)Icons.Information.ToAlphaBitmap().ToCursorHandle()), PlatformID.Win32NT);
        }

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
