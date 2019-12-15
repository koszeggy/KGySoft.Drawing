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
using NUnit.Framework;

#endregion

namespace KGySoft.Drawing.UnitTests
{
    [TestFixture]
    public class BitmapExtensionsTest : TestBase
    {
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
            Assert.AreEqual(7, Icons.Information.ToMultiResBitmap().ExtractBitmaps().Length);
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
            // 32 bit ARGB: count by raw data
            using var refBmp = Icons.Information.ToAlphaBitmap();
            var colors = refBmp.GetColors();
            Assert.LessOrEqual(colors.Length, refBmp.Width * refBmp.Height);
            SaveImage("32argb", refBmp);

            // 24 bit: Fallback to GetPixel (TODO)
            using var bmp24bpp = refBmp.ConvertPixelFormat(PixelFormat.Format24bppRgb);
            colors = bmp24bpp.GetColors();
            Assert.LessOrEqual(colors.Length, bmp24bpp.Width * bmp24bpp.Height);
            SaveImage("24rgb", bmp24bpp);

            // 48 bit: Fallback to GetPixel (TODO)
            using var bmp48bpp = refBmp.ConvertPixelFormat(PixelFormat.Format48bppRgb);
            colors = bmp48bpp.GetColors();
            Assert.LessOrEqual(colors.Length, bmp48bpp.Width * bmp48bpp.Height);
            SaveImage("48rgb", bmp48bpp);

            // 64 bit: Fallback to GetPixel (TODO)
            using var bmp64bpp = refBmp.ConvertPixelFormat(PixelFormat.Format64bppArgb);
            colors = bmp64bpp.GetColors();
            Assert.LessOrEqual(colors.Length, bmp64bpp.Width * bmp64bpp.Height);
            SaveImage("64argb", bmp64bpp);

            // 8 bit: returning actual palette
            using var bmp8bpp = refBmp.ConvertPixelFormat(PixelFormat.Format8bppIndexed);
            colors = bmp8bpp.GetColors();
            Assert.AreEqual(bmp8bpp.Palette.Entries.Length, colors.Length);
            SaveImage("8ind", bmp8bpp);
        }

        [Test]
        public void ToCursorHandleTest()
        {
            Assert.AreNotEqual(IntPtr.Zero, (IntPtr)Icons.Information.ToAlphaBitmap().ToCursorHandle());
        }

        #endregion
    }
}
