#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ImageExtensionsTest.cs
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
using System.IO;

using NUnit.Framework;

#endregion

namespace KGySoft.Drawing.UnitTests
{
    [TestFixture]
    public class ImageExtensionsTest : TestBase
    {
        #region Methods

        [Test]
        public void ToGrayscaleTest()
        {
            using var bmp = Icons.Information.ExtractBitmap(new Size(256, 256));
            using var gray = bmp.ToGrayscale();
            Assert.IsFalse(bmp.EqualsByContent(gray));
            SaveImage("Reference", bmp);
            SaveImage("Grayscale", gray);
        }

        [TestCase(PixelFormat.Format8bppIndexed, 0, 0)]
        [TestCase(PixelFormat.Format8bppIndexed, 0, 1)]
        [TestCase(PixelFormat.Format8bppIndexed, 0, 128)]
        [TestCase(PixelFormat.Format8bppIndexed, 0, 255)]
        [TestCase(PixelFormat.Format8bppIndexed, 0xFFFFFF, 1)]
        [TestCase(PixelFormat.Format8bppIndexed, 0x888888, 1)]
        [TestCase(PixelFormat.Format4bppIndexed, 0, 0)]
        [TestCase(PixelFormat.Format4bppIndexed, 0xFFFFFF, 0)]
        [TestCase(PixelFormat.Format16bppRgb565, 0, 0)]
        [TestCase(PixelFormat.Format16bppRgb565, 0xFFFFFF, 0)]
        [TestCase(PixelFormat.Format16bppArgb1555, 0, 0)]
        [TestCase(PixelFormat.Format16bppArgb1555, 0xFFFFFF, 0)]
        [TestCase(PixelFormat.Format16bppArgb1555, 0, 128)]
        [TestCase(PixelFormat.Format16bppArgb1555, 0xFFFFFF, 128)]
        [TestCase(PixelFormat.Format16bppGrayScale, 0, 0)]
        [TestCase(PixelFormat.Format16bppGrayScale, 0xFFFFFF, 0)]
        [TestCase(PixelFormat.Format48bppRgb, 0, 0)]
        [TestCase(PixelFormat.Format48bppRgb, 0xFFFFFF, 0)]
        public void ConvertPixelFormatTest(PixelFormat pixelFormat, int backColorArgb, byte alphaThreshold)
        {
            //using var ref32bpp = new Bitmap(@"D:\Dokumentumok\Képek\Formats\_test\Hue_alpha_falloff.png");
            using var ref32bpp = Icons.Information.ExtractBitmap(new Size(256, 256));
            Assert.AreEqual(32, ref32bpp.GetBitsPerPixel());

            var backColor = Color.FromArgb(backColorArgb);
            using var converted = ref32bpp.ConvertPixelFormat(pixelFormat, backColor, alphaThreshold);
            Assert.AreEqual(pixelFormat, converted.PixelFormat);
            SaveImage($"{pixelFormat} - {backColor.Name} (A={alphaThreshold})", converted);
        }

        [Test]
        public void EqualsByContentTest()
        {
            var large = new Size(256, 256);
            Assert.IsTrue(Icons.Information.ExtractBitmap(large).EqualsByContent(Icons.Information.ExtractBitmap(large)));
            Assert.IsFalse(Icons.Information.ExtractBitmap(large).EqualsByContent(Icons.Question.ExtractBitmap(large)));
        }

        [Test]
        public void ToIconTest()
        {
            using var bmpRef = Icons.Information.ExtractBitmap(new Size(256, 256)).Resize(new Size(256, 128), true);
            SaveImage("Reference", bmpRef);

            using var noKeepRatio128 = bmpRef.ToIcon(128, false);
            Assert.AreEqual(128, noKeepRatio128.Width);
            SaveIcon("noKeepRatio128", noKeepRatio128);

            using var keepRatio128 = bmpRef.ToIcon(128, true);
            Assert.AreEqual(128, keepRatio128.Width);
            SaveIcon("keepRatio128", keepRatio128);
        }

        [Test]
        public void SaveAsMultipageTiffTest()
        {
            AssertPlatformDependent(() =>
            {
                using var ms = new MemoryStream();
                var pages = Icons.Information.ExtractBitmaps();
                pages.SaveAsMultipageTiff(ms);
                ms.Position = 0;
                var tiff = new Bitmap(ms);
                Assert.AreEqual(ImageFormat.Tiff, tiff.RawFormat);
                Assert.AreEqual(pages.Length, tiff.GetFrameCount(FrameDimension.Page));
            }, PlatformID.Win32NT);
        }

        [Test]
        public void SaveAsGifTest()
        {
            var ms = new MemoryStream();
            var refImage = Icons.Information.ExtractBitmap(new Size(256, 256));

            refImage.SaveAsGif(ms);
            ms.Position = 0;
            var gif = new Bitmap(ms);
            Assert.AreEqual(ImageFormat.Gif, gif.RawFormat);
            Assert.AreEqual(8, gif.GetBitsPerPixel());
            SaveImage("default", gif);

            ms = new MemoryStream();
            refImage.SaveAsGif(ms, new[] { Color.Blue, Color.White, Color.Black, Color.Transparent });
            ms.Position = 0;
            gif = new Bitmap(ms);
            Assert.AreEqual(ImageFormat.Gif, gif.RawFormat);
            Assert.AreEqual(4, gif.Palette.Entries.Length);
            SaveImage("customPalette", gif);

            ms = new MemoryStream();
            refImage.SaveAsGif(ms, true);
            ms.Position = 0;
            gif = new Bitmap(ms);
            Assert.AreEqual(ImageFormat.Gif, gif.RawFormat);
            Assert.AreEqual(8, gif.GetBitsPerPixel());
            SaveImage("dithered", gif);
        }

        #endregion
    }
}
