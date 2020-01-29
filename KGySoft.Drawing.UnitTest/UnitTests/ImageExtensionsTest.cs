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
using KGySoft.Drawing.Imaging;
using NUnit.Framework;

#endregion

namespace KGySoft.Drawing.UnitTests
{
    [TestFixture]
    public class ImageExtensionsTest : TestBase
    {
        #region Fields

        private static object[][] convertPixelFormatCustomTestSource =
        {
            new object[] { "To 8bpp 256 color no dithering", PixelFormat.Format8bppIndexed, PredefinedColorsQuantizer.SystemDefault8BppPalette(), null }, 
            new object[] { "To 8bpp 256 color dithering", PixelFormat.Format8bppIndexed, PredefinedColorsQuantizer.SystemDefault8BppPalette(), OrderedDitherer.Bayer2x2() },
            new object[] { "To 8bpp 16 color no dithering", PixelFormat.Format8bppIndexed, PredefinedColorsQuantizer.SystemDefault4BppPalette(), null },
            new object[] { "To 4bpp 2 color dithering", PixelFormat.Format4bppIndexed, PredefinedColorsQuantizer.BlackAndWhite(), OrderedDitherer.Halftone5Rectangular() },
            //new object[] { "To 4bpp 256 color dithering", PixelFormat.Format4bppIndexed, PredefinedColorsQuantizer.SystemDefault8BppPalette(), OrderedDitherer.Halftone5Rectangular() },
            new object[] { "To ARGB1555 256 color dithering", PixelFormat.Format16bppArgb1555, PredefinedColorsQuantizer.SystemDefault8BppPalette(), new RandomNoiseDitherer(), }, 
            new object[] { "To ARGB1555 32K color dithering", PixelFormat.Format16bppArgb1555, PredefinedColorsQuantizer.Argb1555(), new RandomNoiseDitherer(), },
            new object[] { "To ARGB1555 16.7M color dithering", PixelFormat.Format16bppArgb1555, PredefinedColorsQuantizer.Rgb888(), new RandomNoiseDitherer(), }, 
        };

        #endregion

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
        public void ConvertPixelFormatDirectTest(PixelFormat pixelFormat, int backColorArgb, byte alphaThreshold)
        {
            //using var ref32bpp = new Bitmap(@"D:\Dokumentumok\Képek\Formats\_test\Hue_alpha_falloff.png");
            using var ref32bpp = Icons.Information.ExtractBitmap(new Size(256, 256));
            Assert.AreEqual(32, ref32bpp.GetBitsPerPixel());

            var backColor = Color.FromArgb(backColorArgb);
            using var converted = ref32bpp.ConvertPixelFormat(pixelFormat, backColor, alphaThreshold);
            Assert.AreEqual(pixelFormat, converted.PixelFormat);
            SaveImage($"{pixelFormat} - {backColor.Name} (A={alphaThreshold})", converted);
        }

        [TestCaseSource(nameof(convertPixelFormatCustomTestSource))]
        public void ConvertPixelFormatCustomTest(string testName, PixelFormat pixelFormat, IQuantizer quantizer, IDitherer ditherer)
        {
            using var source = Icons.Information.ExtractBitmap(new Size(256, 256));
            using var converted = source.ConvertPixelFormat(pixelFormat, quantizer, ditherer);
            Assert.AreEqual(pixelFormat, converted.PixelFormat);
            SaveImage(testName, converted);
        }

        [TestCase("32bpp ARGB to 32bpp ARGB", PixelFormat.Format32bppArgb, PixelFormat.Format32bppArgb)]
        [TestCase("32bpp PARGB to 32bpp PARGB", PixelFormat.Format32bppPArgb, PixelFormat.Format32bppPArgb)]
        [TestCase("32bpp ARGB to 32bpp RGB", PixelFormat.Format32bppArgb, PixelFormat.Format32bppRgb)]
        [TestCase("32bpp RGB to 32bpp ARGB", PixelFormat.Format32bppRgb, PixelFormat.Format32bppArgb)]
        [TestCase("32bpp ARGB to 32bpp PARGB", PixelFormat.Format32bppArgb, PixelFormat.Format32bppPArgb)]
        [TestCase("32bpp PARGB to 32bpp ARGB", PixelFormat.Format32bppPArgb, PixelFormat.Format32bppArgb)]
        [TestCase("32bpp ARGB to 16bpp Grayscale", PixelFormat.Format32bppArgb, PixelFormat.Format16bppGrayScale)]
        [TestCase("32bpp ARGB to 16bpp ARGB", PixelFormat.Format32bppArgb, PixelFormat.Format16bppArgb1555)]
        [TestCase("32bpp ARGB to 8bpp Indexed", PixelFormat.Format32bppArgb, PixelFormat.Format8bppIndexed)]
        [TestCase("32bpp ARGB to 4bpp Indexed", PixelFormat.Format32bppArgb, PixelFormat.Format4bppIndexed)]
        [TestCase("32bpp ARGB to 1bpp Indexed", PixelFormat.Format32bppArgb, PixelFormat.Format1bppIndexed)]
        [TestCase("64bpp ARGB to 64bpp ARGB", PixelFormat.Format64bppArgb, PixelFormat.Format64bppArgb)]
        [TestCase("64bpp PARGB to 64bpp PARGB", PixelFormat.Format64bppPArgb, PixelFormat.Format64bppPArgb)]
        public void DrawIntoTest(string testName, PixelFormat formatSrc, PixelFormat formatDst)
        {
            Size targetSize = new Size(300, 300);
            Size sourceSize = new Size(300, 300);
            Point offset = new Point(-50, -50);

            // creating source images: alpha rectangles
            using var bmpSrc1 = new Bitmap(sourceSize.Width, sourceSize.Height, formatSrc);
            bmpSrc1.Clear(Color.FromArgb(128, Color.Red));
            using var bmpSrc2 = new Bitmap(sourceSize.Width, sourceSize.Height, formatSrc);
            bmpSrc2.Clear(Color.FromArgb(128, Color.Lime));

            // creating target image
            using var bmpDst = new Bitmap(targetSize.Width, targetSize.Height, formatDst);

            // drawing sources into destination
            Assert.DoesNotThrow(() => bmpSrc1.DrawInto(bmpDst, offset));
            Assert.DoesNotThrow(() => bmpSrc2.DrawInto(bmpDst, new Point(bmpDst.Width - offset.X - bmpSrc2.Width, bmpDst.Height - offset.Y - bmpSrc2.Height)));

            SaveImage(testName, bmpDst);
        }

        [TestCase("32bpp ARGB to 1bpp Indexed ErrorDiffusion", PixelFormat.Format32bppArgb, PixelFormat.Format1bppIndexed, true)]
        [TestCase("32bpp ARGB to 1bpp Indexed Ordered", PixelFormat.Format32bppArgb, PixelFormat.Format1bppIndexed, false)]
        public void DrawIntoWithDitheringTest(string testName, PixelFormat formatSrc, PixelFormat formatDst, bool errorDiffusion)
        {
            Size targetSize = new Size(300, 300);
            Size sourceSize = new Size(300, 300);
            Point offset = new Point(-50, -50);

            // creating source images: alpha rectangles
            using var bmpSrc1 = new Bitmap(sourceSize.Width, sourceSize.Height, formatSrc);
            bmpSrc1.Clear(Color.FromArgb(128, Color.Red));
            using var bmpSrc2 = new Bitmap(sourceSize.Width, sourceSize.Height, formatSrc);
            bmpSrc2.Clear(Color.FromArgb(128, Color.Lime));

            // creating target image
            using var bmpDst = new Bitmap(targetSize.Width, targetSize.Height, formatDst);

            // drawing sources into destination
            IDitherer ditherer = errorDiffusion ? (IDitherer)ErrorDiffusionDitherer.FloydSteinberg : OrderedDitherer.Bayer8x8();
            Assert.DoesNotThrow(() => bmpSrc1.DrawInto(bmpDst, offset, ditherer));
            Assert.DoesNotThrow(() => bmpSrc2.DrawInto(bmpDst, new Point(bmpDst.Width - offset.X - bmpSrc2.Width, bmpDst.Height - offset.Y - bmpSrc2.Height), ditherer));

            SaveImage(testName, bmpDst);
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
