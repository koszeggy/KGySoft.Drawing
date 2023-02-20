#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: RawIconTest.cs
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

using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

using KGySoft.Drawing.Imaging;
using KGySoft.Drawing.WinApi;

using NUnit.Framework;

#endregion

namespace KGySoft.Drawing.UnitTests
{
    [TestFixture]
    public class RawIconTest : TestBase
    {
        #region Methods

        [TestCase(PixelFormat.Format64bppArgb)]
        [TestCase(PixelFormat.Format64bppPArgb)]
        [TestCase(PixelFormat.Format48bppRgb)]
        [TestCase(PixelFormat.Format32bppArgb)]
        [TestCase(PixelFormat.Format32bppPArgb)]
        [TestCase(PixelFormat.Format32bppRgb)]
        [TestCase(PixelFormat.Format24bppRgb)]
        [TestCase(PixelFormat.Format16bppRgb565)]
        [TestCase(PixelFormat.Format16bppRgb555)]
        [TestCase(PixelFormat.Format16bppArgb1555)]
        [TestCase(PixelFormat.Format16bppGrayScale)]
        [TestCase(PixelFormat.Format8bppIndexed)]
        [TestCase(PixelFormat.Format4bppIndexed)]
        [TestCase(PixelFormat.Format1bppIndexed)]
        public void AddBitmapSaveUncompressedTest(PixelFormat pixelFormat)
        {
            var ms = new MemoryStream();
            IQuantizer quantizer = pixelFormat.IsIndexed() ? OptimizedPaletteQuantizer.Octree(1 << pixelFormat.ToBitsPerPixel()) : null;
            var size = OSUtils.IsWindows ? new Size(256, 256) : new Size(64, 64);
            var refImage = Convert(Icons.Information.ExtractBitmap(size), pixelFormat, quantizer);

            using (RawIcon icon = new RawIcon())
            {
                icon.Add(refImage);
                icon.Save(ms, true);
            }

            ms.Position = 0;
            var bmp = new Bitmap(ms);

            Assert.AreEqual(ImageFormat.Icon, bmp.RawFormat);
            Assert.AreEqual(PixelFormat.Format32bppArgb, bmp.PixelFormat);
            Assert.IsTrue(!pixelFormat.IsIndexed() && !pixelFormat.HasAlpha() || bmp.GetPixel(0, 0).A == 0, "Transparency expected");
            SaveImage($"{pixelFormat}", bmp, true);
        }

        [TestCase("32bpp White", PixelFormat.Format32bppArgb, 0xFFFFFFFF)]
        [TestCase("32bpp Black", PixelFormat.Format32bppArgb, 0xFF000000)]
        [TestCase("24bpp White", PixelFormat.Format24bppRgb, 0xFFFFFFFF)]
        [TestCase("24bpp Black", PixelFormat.Format24bppRgb, 0xFF000000)]
        [TestCase("8bpp White", PixelFormat.Format8bppIndexed, 0xFFFFFFFF)]
        [TestCase("8bpp Black", PixelFormat.Format8bppIndexed, 0xFF000000)]
        [TestCase("4bpp White", PixelFormat.Format4bppIndexed, 0xFFFFFFFF)]
        [TestCase("4bpp Black", PixelFormat.Format4bppIndexed, 0xFF000000)]
        [TestCase("1bpp White", PixelFormat.Format1bppIndexed, 0xFFFFFFFF)]
        [TestCase("1bpp Black", PixelFormat.Format1bppIndexed, 0xFF000000)]
        public void AddBitmapCustomBackgroundSaveUncompressedTest(string testName, PixelFormat pixelFormat, uint backColor)
        {
            var ms = new MemoryStream();
            var size = OSUtils.IsWindows ? new Size(256, 256) : new Size(64, 64);
            var refImage = Icons.Information.ExtractBitmap(size).ConvertPixelFormat(pixelFormat);

            using (RawIcon icon = new RawIcon())
            {
                icon.Add(refImage, Color.FromArgb((int)backColor));
                icon.Save(ms, true);
            }

            ms.Position = 0;
            var bmp = new Bitmap(ms);

            Assert.AreEqual(ImageFormat.Icon, bmp.RawFormat);
            Assert.AreEqual(PixelFormat.Format32bppArgb, bmp.PixelFormat);

            SaveImage(testName, bmp, true);
        }

        [Test]
        public void AddMultiResBitmapTest()
        {
            var ms = new MemoryStream();
            var refImage = Icons.Information.ToMultiResBitmap();

            using (RawIcon icon = new RawIcon())
            {
                icon.Add(refImage);
                icon.Save(ms, true);
            }

            ms.Position = 0;
            var bmp = new Bitmap(ms);

            Assert.AreEqual(ImageFormat.Icon, bmp.RawFormat);
            Assert.AreEqual(PixelFormat.Format32bppArgb, bmp.PixelFormat);
            Assert.AreEqual(OSUtils.IsWindows ? 7 : 1, bmp.ExtractIconImages().Length);
            SaveImage("result", bmp, true);
        }

        [Test]
        public void AddLarge24BppBitmapTest()
        {
            var ms = new MemoryStream();
            using (var raw = new RawIcon())
            {
                raw.Add(Icons.Information.ExtractBitmap(new Size(256, 256)).ConvertPixelFormat(PixelFormat.Format24bppRgb), Color.Black);
                raw.Save(ms, false);
            }

            // <32 BPP icons are always saved uncompressed
            ms.Position = 0;

            AssertPlatformDependent(() =>
            {
                var icon = new Icon(ms);
                Assert.AreEqual(24, icon.GetBitsPerPixel());
                Assert.IsFalse(icon.IsCompressed());
                SaveIcon("result", icon);
            });
        }

        #endregion
    }
}
