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

        [Test]
        public void ConvertPixelFormatTest()
        {
            using var ref32bpp = Icons.Information.ExtractBitmap(new Size(256, 256));
            Assert.AreEqual(32, ref32bpp.GetBitsPerPixel());
            SaveImage("32bppRef", ref32bpp);

            using var bmp32pargb = ref32bpp.ConvertPixelFormat(PixelFormat.Format32bppPArgb);
            SaveImage("32bppPArgb", bmp32pargb);
            Assert.AreEqual(32, bmp32pargb.GetBitsPerPixel());

            using var bmp64argb = ref32bpp.ConvertPixelFormat(PixelFormat.Format64bppArgb);
            SaveImage("64bppArgb", bmp64argb);
            Assert.AreEqual(64, bmp64argb.GetBitsPerPixel());

            using var bmp24bpp = ref32bpp.ConvertPixelFormat(PixelFormat.Format24bppRgb);
            SaveImage("24bpp", bmp24bpp);
            Assert.AreEqual(24, bmp24bpp.GetBitsPerPixel());

            using var bmp16bpp = ref32bpp.ConvertPixelFormat(PixelFormat.Format16bppRgb565);
            Assert.AreEqual(16, bmp16bpp.GetBitsPerPixel());
            SaveImage("16bpp", bmp16bpp);

            using var bmp8bpp = ref32bpp.ConvertPixelFormat(PixelFormat.Format8bppIndexed);
            Assert.AreEqual(8, bmp8bpp.GetBitsPerPixel());
            SaveImage("8bpp", bmp8bpp);

            using var bmp4bpp = ref32bpp.ConvertPixelFormat(PixelFormat.Format4bppIndexed);
            Assert.AreEqual(4, bmp4bpp.GetBitsPerPixel());
            SaveImage("4bpp", bmp4bpp);

            using var bmp1bpp = ref32bpp.ConvertPixelFormat(PixelFormat.Format1bppIndexed);
            Assert.AreEqual(1, bmp1bpp.GetBitsPerPixel());
            SaveImage("1bpp", bmp1bpp);

            // converting to indexed transparent by ARGB
            var pal4bpp = bmp4bpp.Palette.Entries;
            pal4bpp[0] = Color.Transparent;
            using var tr4bpp = ref32bpp.ConvertPixelFormat(PixelFormat.Format4bppIndexed, pal4bpp);
            Assert.AreEqual(4, tr4bpp.GetBitsPerPixel());
            SaveImage("tr4bpp", bmp4bpp);

            var pal1bpp = bmp1bpp.Palette.Entries;
            pal1bpp[1] = Color.Transparent;
            using var tr1bpp = ref32bpp.ConvertPixelFormat(PixelFormat.Format1bppIndexed, pal1bpp);
            Assert.AreEqual(1, tr1bpp.GetBitsPerPixel());
            SaveImage("tr1bpp", tr1bpp);

            // converting 8bpp to 4bpp
            using var tr8to4bpp = bmp8bpp.ConvertPixelFormat(PixelFormat.Format4bppIndexed, pal4bpp);
            Assert.AreEqual(4, tr4bpp.GetBitsPerPixel());
            SaveImage("tr8to4bpp", tr8to4bpp);

            // converting 8bpp to 1bpp
            using var tr8to1bpp = bmp8bpp.ConvertPixelFormat(PixelFormat.Format1bppIndexed, pal1bpp);
            Assert.AreEqual(1, tr8to1bpp.GetBitsPerPixel());
            SaveImage("tr8to1bpp", tr8to1bpp);

            // converting 4bpp to 8bpp
            using var tr4to8bpp = tr4bpp.ConvertPixelFormat(PixelFormat.Format8bppIndexed);
            Assert.AreEqual(8, tr4to8bpp.GetBitsPerPixel());
            SaveImage("tr4to8bpp", tr4to8bpp);

            // converting 4bpp to 1bpp
            using var tr4to1bpp = tr4bpp.ConvertPixelFormat(PixelFormat.Format1bppIndexed, pal1bpp);
            Assert.AreEqual(1, tr4to1bpp.GetBitsPerPixel());
            SaveImage("tr4to1bpp", tr4to1bpp);

            // converting 1bpp to 8bpp
            using var tr1to8bpp = tr1bpp.ConvertPixelFormat(PixelFormat.Format8bppIndexed);
            Assert.AreEqual(8, tr1to8bpp.GetBitsPerPixel());
            SaveImage("tr1to8bpp", tr1to8bpp);

            // converting 1bpp to 4bpp
            using var tr1to4bpp = tr1bpp.ConvertPixelFormat(PixelFormat.Format4bppIndexed, pal4bpp);
            Assert.AreEqual(4, tr1to4bpp.GetBitsPerPixel());
            SaveImage("tr1to4bpp", tr1to4bpp);
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
