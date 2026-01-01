#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BitmapExtensionsTest.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2026 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using KGySoft.CoreLibraries;
using KGySoft.Drawing.Imaging;
using KGySoft.Drawing.WinApi;

using NUnit.Framework;

#endregion

namespace KGySoft.Drawing.UnitTests
{
    [TestFixture]
    public class BitmapExtensionsTest : TestBase
    {
        #region Properties

        private static object[][] QuantizeTestSource => new[]
        {
            new object[] { "RGB888 Black", PredefinedColorsQuantizer.Rgb888(), 1 << 24 },
            new object[] { "RGB888 White", PredefinedColorsQuantizer.Rgb888(Color.White), 1 << 24 },
            new object[] { "RGB565 Black", PredefinedColorsQuantizer.Rgb565(), 1 << 16 },
            new object[] { "RGB565 White", PredefinedColorsQuantizer.Rgb565(Color.White), 1 << 16 },
            new object[] { "RGB555 Black", PredefinedColorsQuantizer.Rgb555(), 1 << 15 },
            new object[] { "RGB555 White", PredefinedColorsQuantizer.Rgb555(Color.White), 1 << 15 },
            new object[] { "ARGB1555 Black 50%", PredefinedColorsQuantizer.Argb1555(), (1 << 15) + 1 },
            new object[] { "ARGB1555 White 50%", PredefinedColorsQuantizer.Argb1555(Color.White), (1 << 15) + 1 },
            new object[] { "ARGB1555 Black 0", PredefinedColorsQuantizer.Argb1555(default, 0), (1 << 15) + 1 },
            new object[] { "ARGB1555 Black 1", PredefinedColorsQuantizer.Argb1555(default, 1), (1 << 15) + 1 },
            new object[] { "ARGB1555 Black 254", PredefinedColorsQuantizer.Argb1555(default, 254), (1 << 15) + 1 },
            new object[] { "RGB332 Black Slow", PredefinedColorsQuantizer.Rgb332(), 256 },
            new object[] { "RGB332 Black Fast", PredefinedColorsQuantizer.Rgb332(directMapping: true), 256 },
            new object[] { "RGB332 White Slow", PredefinedColorsQuantizer.Rgb332(Color.White), 256 },
            new object[] { "Grayscale256 Black", PredefinedColorsQuantizer.Grayscale(), 256 },
            new object[] { "Grayscale256 White", PredefinedColorsQuantizer.Grayscale(Color.White), 256 },
            new object[] { "Grayscale16 Black Slow", PredefinedColorsQuantizer.Grayscale16(), 16 },
            new object[] { "Grayscale16 Black Fast", PredefinedColorsQuantizer.Grayscale16(directMapping: true), 16 },
            new object[] { "Grayscale4 Black Slow", PredefinedColorsQuantizer.Grayscale4(), 4 },
            new object[] { "Grayscale4 Black Fast", PredefinedColorsQuantizer.Grayscale4(directMapping: true), 4 },
            new object[] { "BW Black", PredefinedColorsQuantizer.BlackAndWhite(), 2 },
            new object[] { "BW White", PredefinedColorsQuantizer.BlackAndWhite(Color.White), 2 },
            new object[] { "BW Lime", PredefinedColorsQuantizer.BlackAndWhite(Color.Lime), 2 },
            new object[] { "BW Blue", PredefinedColorsQuantizer.BlackAndWhite(Color.Blue), 2 },
            new object[] { "Default8Bpp Black", PredefinedColorsQuantizer.SystemDefault8BppPalette(), 256 },
            new object[] { "Default8Bpp White", PredefinedColorsQuantizer.SystemDefault8BppPalette(Color.White), 256 },
            new object[] { "Default4Bpp Black", PredefinedColorsQuantizer.SystemDefault4BppPalette(), 16 },
            new object[] { "Default4Bpp White", PredefinedColorsQuantizer.SystemDefault4BppPalette(Color.White), 16 },
            new object[] { "Default1Bpp Black", PredefinedColorsQuantizer.SystemDefault1BppPalette(), 2 },
            new object[] { "Default1Bpp White", PredefinedColorsQuantizer.SystemDefault1BppPalette(Color.White), 2 },
            new object[] { "Custom Black", PredefinedColorsQuantizer.FromCustomPalette(new[] { Color.Black, Color.White, Color.Red, Color.Blue, Color.Green, Color.Magenta, Color.Yellow, Color.Cyan }), 8 },
            new object[] { "Custom White", PredefinedColorsQuantizer.FromCustomPalette(new[] { Color.Black, Color.White, Color.Red, Color.Blue, Color.Green, Color.Magenta, Color.Yellow, Color.Cyan }, Color.White), 8 },

            new object[] { "Octree 256 Black", OptimizedPaletteQuantizer.Octree(256, Color.Black, 0), 256 },
            new object[] { "Octree 16 Black", OptimizedPaletteQuantizer.Octree(16, Color.Black, 0), 16 },
            new object[] { "Octree 4 Black", OptimizedPaletteQuantizer.Octree(4, Color.Black, 0), 4 },
            new object[] { "Octree 2 Black", OptimizedPaletteQuantizer.Octree(2, Color.Black, 0), 2 },
            new object[] { "Octree 256 White", OptimizedPaletteQuantizer.Octree(256, Color.White, 0), 256 },

            new object[] { "MedianCut 256 Black", OptimizedPaletteQuantizer.MedianCut(256, Color.Black, 0), 256 },
            new object[] { "MedianCut 16 Black", OptimizedPaletteQuantizer.MedianCut(16, Color.Black, 0), 16 },
            new object[] { "MedianCut 4 Black", OptimizedPaletteQuantizer.MedianCut(4, Color.Black, 0), 4 },
            new object[] { "MedianCut 256 White", OptimizedPaletteQuantizer.MedianCut(256, Color.White, 0), 256 },

            new object[] { "Wu 256 Black", OptimizedPaletteQuantizer.Wu(256, Color.Black, 0), 256 },
            new object[] { "Wu 16 Black", OptimizedPaletteQuantizer.Wu(16, Color.Black, 0), 16 },
            new object[] { "Wu 4 Black", OptimizedPaletteQuantizer.Wu(4, Color.Black, 0), 4 },
            new object[] { "Wu 2 Black", OptimizedPaletteQuantizer.Wu(2, Color.Black, 0), 2 },
            new object[] { "Wu 256 White", OptimizedPaletteQuantizer.Wu(256, Color.White, 0), 256 },
            new object[] { "Wu 256 TR", OptimizedPaletteQuantizer.Wu(256, Color.White), 256 },
        };

        private static object[][] QuantizerBatchTestSource => new[]
        {
            ////new object[] { "ARGB888 Black 50%", PredefinedColorsQuantizer.Argb8888(), Int32.MaxValue },
            ////new object[] { "ARGB888 Silver 1", PredefinedColorsQuantizer.Argb8888(Color.Silver, 1), Int32.MaxValue },
            ////new object[] { "RGB888 Black", PredefinedColorsQuantizer.Rgb888(), 1 << 24 },
            ////new object[] { "RGB888 White", PredefinedColorsQuantizer.Rgb888(Color.White), 1 << 24 },
            ////new object[] { "RGB565 Black", PredefinedColorsQuantizer.Rgb565(), 1 << 16 },
            ////new object[] { "RGB565 White", PredefinedColorsQuantizer.Rgb565(Color.White), 1 << 16 },
            ////new object[] { "RGB555 Black", PredefinedColorsQuantizer.Rgb555(), 1 << 15 },
            ////new object[] { "RGB555 White", PredefinedColorsQuantizer.Rgb555(Color.White), 1 << 15 },
            ////new object[] { "ARGB1555 Black 50%", PredefinedColorsQuantizer.Argb1555(), (1 << 15) + 1 },
            ////new object[] { "ARGB1555 White 50%", PredefinedColorsQuantizer.Argb1555(Color.White), (1 << 15) + 1 },
            ////new object[] { "ARGB1555 Black 0", PredefinedColorsQuantizer.Argb1555(default, 0), (1 << 15) + 1 },
            ////new object[] { "ARGB1555 Black 1", PredefinedColorsQuantizer.Argb1555(default, 1), (1 << 15) + 1 },
            ////new object[] { "ARGB1555 Black 254", PredefinedColorsQuantizer.Argb1555(default, 254), (1 << 15) + 1 },
            new object[] { "RGB332 Black sRGB", PredefinedColorsQuantizer.Rgb332(), 256 },
            new object[] { "RGB332 Black Linear", PredefinedColorsQuantizer.Rgb332().ConfigureColorSpace(WorkingColorSpace.Linear), 256 },
            new object[] { "RGB332 Black Direct", PredefinedColorsQuantizer.Rgb332(directMapping: true), 256 },
            ////new object[] { "RGB332 White Slow", PredefinedColorsQuantizer.Rgb332(Color.White), 256 },
            ////new object[] { "Grayscale256 Black", PredefinedColorsQuantizer.Grayscale(), 256 },
            ////new object[] { "Grayscale256 White", PredefinedColorsQuantizer.Grayscale(Color.White), 256 },
            ////new object[] { "Grayscale16 Black Slow", PredefinedColorsQuantizer.Grayscale16(), 16 },
            ////new object[] { "Grayscale16 Black Fast", PredefinedColorsQuantizer.Grayscale16(directMapping: true), 16 },
            ////new object[] { "Grayscale5 black", PredefinedColorsQuantizer.FromCustomPalette(new[] { Color.Black, Color.FromArgb(64, 64, 64), Color.Gray, Color.FromArgb(192, 192, 192), Color.White }), 5 },
            ////new object[] { "Grayscale4 Black Slow", PredefinedColorsQuantizer.Grayscale4(), 4 },
            ////new object[] { "Grayscale4 Black Fast", PredefinedColorsQuantizer.Grayscale4(directMapping: true), 4 },
            ////new object[] { "Grayscale3 Black", PredefinedColorsQuantizer.FromCustomPalette(new[] { Color.Black, Color.Gray, Color.White }), 3 },
            new object[] { "BW Black sRGB", PredefinedColorsQuantizer.BlackAndWhite(), 2 },
            new object[] { "BW Black Linear", PredefinedColorsQuantizer.BlackAndWhite().ConfigureColorSpace(WorkingColorSpace.Linear), 2 },
            ////new object[] { "BW White", PredefinedColorsQuantizer.BlackAndWhite(Color.White), 2 },
            ////new object[] { "BW Lime", PredefinedColorsQuantizer.BlackAndWhite(Color.Lime), 2 },
            ////new object[] { "BW Blue", PredefinedColorsQuantizer.BlackAndWhite(Color.Blue), 2 },
            new object[] { "Default8Bpp Black sRGB", PredefinedColorsQuantizer.SystemDefault8BppPalette(), 256 },
            new object[] { "Default8Bpp Black Linear", PredefinedColorsQuantizer.SystemDefault8BppPalette().ConfigureColorSpace(WorkingColorSpace.Linear), 256 },
            ////new object[] { "Default8Bpp White", PredefinedColorsQuantizer.SystemDefault8BppPalette(Color.White), 256 },
            new object[] { "Default4Bpp Black sRGB", PredefinedColorsQuantizer.SystemDefault4BppPalette(), 16 },
            new object[] { "Default4Bpp Black Linear", PredefinedColorsQuantizer.SystemDefault4BppPalette().ConfigureColorSpace(WorkingColorSpace.Linear), 16 },
            ////new object[] { "Default4Bpp White", PredefinedColorsQuantizer.SystemDefault4BppPalette(Color.White), 16 },
            //new object[] { "Default1Bpp Black sRGB", PredefinedColorsQuantizer.SystemDefault1BppPalette(), 2 },
            //new object[] { "Default1Bpp Black Linear", PredefinedColorsQuantizer.SystemDefault1BppPalette().ConfigureColorSpace(WorkingColorSpace.Linear), 2 },
            ////new object[] { "Default1Bpp White", PredefinedColorsQuantizer.SystemDefault1BppPalette(Color.White), 2 },
            new object[] { "Custom Black sRGB", PredefinedColorsQuantizer.FromCustomPalette(new[] { Color.Black, Color.White, Color.Red, Color.Blue, Color.Lime, Color.Magenta, Color.Yellow, Color.Cyan }), 8 },
            new object[] { "Custom Black Linear", PredefinedColorsQuantizer.FromCustomPalette(new[] { Color.Black, Color.White, Color.Red, Color.Blue, Color.Lime, Color.Magenta, Color.Yellow, Color.Cyan }).ConfigureColorSpace(WorkingColorSpace.Linear), 8 },
            ////new object[] { "Custom White", PredefinedColorsQuantizer.FromCustomPalette(new[] { Color.Black, Color.White, Color.Red, Color.Blue, Color.Green, Color.Magenta, Color.Yellow, Color.Cyan }, Color.White), 8 },

            ////new object[] { "Octree 256 Black", OptimizedPaletteQuantizer.Octree(256, Color.Black, 0), 256 },
            //new object[] { "Octree 16 Black sRGB", OptimizedPaletteQuantizer.Octree(16, Color.Black, 0), 16 },
            //new object[] { "Octree 16 Black Linear", OptimizedPaletteQuantizer.Octree(16, Color.Black, 0).ConfigureColorSpace(WorkingColorSpace.Linear), 16 },
            ////new object[] { "Octree 2 Black", OptimizedPaletteQuantizer.Octree(2, Color.Black, 0), 2 },
            ////new object[] { "Octree 256 Silver", OptimizedPaletteQuantizer.Octree(256, Color.Silver, 0), 256 },
            ////new object[] { "Octree 16 Silver", OptimizedPaletteQuantizer.Octree(16, Color.Silver, 0), 16 },
            ////new object[] { "Octree 2 Silver", OptimizedPaletteQuantizer.Octree(2, Color.Silver, 0), 2 },
            ////new object[] { "Octree 256 TR", OptimizedPaletteQuantizer.Octree(256), 256 },
            ////new object[] { "Octree 16 TR", OptimizedPaletteQuantizer.Octree(16), 16 },
            ////new object[] { "Octree 2 TR", OptimizedPaletteQuantizer.Octree(2), 2 },

            ////new object[] { "MedianCut 256 Black", OptimizedPaletteQuantizer.MedianCut(256, Color.Black, 0), 256 },
            //new object[] { "MedianCut 16 Black sRGB", OptimizedPaletteQuantizer.MedianCut(16, Color.Black, 0), 16 },
            //new object[] { "MedianCut 16 Black Linear", OptimizedPaletteQuantizer.MedianCut(16, Color.Black, 0).ConfigureColorSpace(WorkingColorSpace.Linear), 16 },
            ////new object[] { "MedianCut 2 Black", OptimizedPaletteQuantizer.MedianCut(2, Color.Black, 0), 2 },
            ////new object[] { "MedianCut 256 Silver", OptimizedPaletteQuantizer.MedianCut(256, Color.Silver, 0), 256 },
            ////new object[] { "MedianCut 16 Silver", OptimizedPaletteQuantizer.MedianCut(16, Color.Silver, 0), 16 },
            ////new object[] { "MedianCut 2 Silver", OptimizedPaletteQuantizer.MedianCut(2, Color.Silver, 0), 2 },
            ////new object[] { "MedianCut 256 TR", OptimizedPaletteQuantizer.MedianCut(256), 256 },
            ////new object[] { "MedianCut 16 TR", OptimizedPaletteQuantizer.MedianCut(16), 16 },
            ////new object[] { "MedianCut 2 TR", OptimizedPaletteQuantizer.MedianCut(2), 2 },

            ////new object[] { "Wu 256 Black", OptimizedPaletteQuantizer.Wu(256, Color.Black, 0), 256 },
            new object[] { "Wu 16 Black sRGB", OptimizedPaletteQuantizer.Wu(16, Color.Black, 0), 16 },
            new object[] { "Wu 16 Black Linear", OptimizedPaletteQuantizer.Wu(16, Color.Black, 0).ConfigureColorSpace(WorkingColorSpace.Linear), 16 },
            ////new object[] { "Wu 2 Black", OptimizedPaletteQuantizer.Wu(2, Color.Black, 0), 2 },
            ////new object[] { "Wu 256 Silver", OptimizedPaletteQuantizer.Wu(256, Color.Silver, 0), 256 },
            ////new object[] { "Wu 16 Silver", OptimizedPaletteQuantizer.Wu(16, Color.Silver, 0), 16 },
            ////new object[] { "Wu 2 Silver", OptimizedPaletteQuantizer.Wu(2, Color.Silver, 0), 2 },
            ////new object[] { "Wu 256 TR", OptimizedPaletteQuantizer.Wu(256), 256 },
            ////new object[] { "Wu 16 TR", OptimizedPaletteQuantizer.Wu(16), 16 },
            ////new object[] { "Wu 2 TR", OptimizedPaletteQuantizer.Wu(2), 2 },
        };

        #endregion

        #region Methods

        [Test]
        public void ResizeWithDrawImageTest()
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
        public void ResizeNoDrawImageTest()
        {
            using var bmpRef = Icons.Information.ExtractBitmap(new Size(256, 256));
            var newSize = new Size(256, 64);
            using var resizedNoAspectRatio = bmpRef.Resize(newSize, ScalingMode.Auto, false);
            Assert.AreEqual(newSize, resizedNoAspectRatio.Size);
            SaveImage("NoAspectRatio", resizedNoAspectRatio);

            using var keepAspectRatioShrinkY = bmpRef.Resize(newSize, ScalingMode.Auto, true);
            Assert.AreEqual(newSize, keepAspectRatioShrinkY.Size);
            SaveImage("KeepAspectRatioShrinkY", keepAspectRatioShrinkY);

            newSize = new Size(64, 256);
            using var keepAspectRatioShrinkX = bmpRef.Resize(newSize, ScalingMode.Auto, true);
            Assert.AreEqual(newSize, keepAspectRatioShrinkX.Size);
            SaveImage("KeepAspectRatioShrinkX", keepAspectRatioShrinkX);

            newSize = new Size(300, 400);
            using var keepAspectRatioEnlargeX = bmpRef.Resize(newSize, ScalingMode.Auto, true);
            Assert.AreEqual(newSize, keepAspectRatioEnlargeX.Size);
            SaveImage("KeepAspectRatioEnlargeX", keepAspectRatioEnlargeX);

            newSize = new Size(400, 300);
            using var keepAspectRatioEnlargeY = bmpRef.Resize(newSize, ScalingMode.Auto, true);
            Assert.AreEqual(newSize, keepAspectRatioEnlargeY.Size);
            SaveImage("KeepAspectRatioEnlargeY", keepAspectRatioEnlargeY);
        }


        [TestCase(16, 256)]
        [TestCase(256, 16)]
        public void ResizeScalingModesTest(int sourceSize, int newSize)
        {
            var sizeSrc = new Size(sourceSize, sourceSize);
            var sizeDst = new Size(newSize, newSize);
            using var bmpSource = Icons.Information.ExtractBitmap(sizeSrc);

            using var bmpRef = bmpSource.Resize(sizeDst);
            SaveImage($"{sizeSrc.Width}x{sizeSrc.Height} to {sizeDst.Width}x{sizeDst.Height}  Graphics.DrawImage", bmpRef);

            foreach (ScalingMode scalingMode in Enum<ScalingMode>.GetValues())
            {
                using var bmpResult = bmpSource.Resize(sizeDst, scalingMode);
                SaveImage($"{sizeSrc.Width}x{sizeSrc.Height} to {sizeDst.Width}x{sizeDst.Height} {scalingMode}", bmpResult);
            }
        }

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
        public void ResizeWithDrawImageByFormatTest(PixelFormat pixelFormat)
        {
            using var bmpRef = Convert(Icons.Information.ExtractBitmap(new Size(256, 256)), pixelFormat);
            var newSize = new Size(256, 64);
            using var resized = bmpRef.Resize(newSize, false);
            Assert.AreEqual(newSize, resized.Size);
            SaveImage($"{pixelFormat}", resized);
        }

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
        public void ResizeNoDrawImageByFormatTest(PixelFormat pixelFormat)
        {
            using var bmpRef = Convert(Icons.Information.ExtractBitmap(new Size(256, 256)), pixelFormat);
            var newSize = new Size(256, 64);
            using var resized = bmpRef.Resize(newSize, ScalingMode.Auto);
            Assert.AreEqual(newSize, resized.Size);
            SaveImage($"{pixelFormat}", resized);
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

            // Cloning an Icon (multi frames -> single frame)
            using var icon = Icons.Information.ToMultiResBitmap();
            clone = icon.CloneCurrentFrame();
            Assert.AreEqual(1, clone.ExtractBitmaps().Length);
            Assert.AreEqual(OSUtils.IsWindows ? 7 : 1, icon.ExtractBitmaps().Length);
            SaveImage("IconClone", clone);
        }

        [Test]
        public void GetColorsTest()
        {
            // 32 bit ARGB
            using var refBmp = Icons.Information.ToAlphaBitmap();
            var colorCount = refBmp.GetColorCount();
            Assert.LessOrEqual(colorCount, refBmp.Width * refBmp.Height);
            SaveImage("32argb", refBmp);

            // 24 bit
            using var bmp24bpp = refBmp.ConvertPixelFormat(PixelFormat.Format24bppRgb);
            colorCount = bmp24bpp.GetColorCount();
            Assert.LessOrEqual(colorCount, bmp24bpp.Width * bmp24bpp.Height);
            SaveImage("24rgb", bmp24bpp);

            AssertPlatformDependent(() =>
            {
                // 48 bit
                using var bmp48bpp = refBmp.ConvertPixelFormat(PixelFormat.Format48bppRgb);
                colorCount = bmp48bpp.GetColorCount();
                Assert.LessOrEqual(colorCount, bmp48bpp.Width * bmp48bpp.Height);
                SaveImage("48rgb", bmp48bpp);

                // 64 bit
                using var bmp64bpp = refBmp.ConvertPixelFormat(PixelFormat.Format64bppArgb);
                colorCount = bmp64bpp.GetColorCount();
                Assert.LessOrEqual(colorCount, bmp64bpp.Width * bmp64bpp.Height);
                SaveImage("64argb", bmp64bpp);
            }, PlatformID.Win32NT);

            // 8 bit: returning actual palette
            using var bmp8bpp = refBmp.ConvertPixelFormat(PixelFormat.Format8bppIndexed);
            colorCount = bmp8bpp.GetColorCount();
            Assert.LessOrEqual(colorCount, 256);
            SaveImage("8ind", bmp8bpp);
        }

        [Test]
        public void ToCursorHandleTest()
        {
            AssertPlatformDependent(() => Assert.AreNotEqual(IntPtr.Zero, (IntPtr)Icons.Information.ToAlphaBitmap().ToCursorHandle()), PlatformID.Win32NT);
        }

        [TestCaseSource(nameof(QuantizeTestSource))]
        public void QuantizeTest(string testName, IQuantizer quantizer, int maxColors)
        {
            using Bitmap ref32bpp = Icons.Information.ExtractBitmap(new Size(256, 256));
            ref32bpp.Quantize(quantizer);
            int colors = ref32bpp.GetColors(forceScanningContent: true).Length;
            Console.WriteLine($"{testName} - {colors} colors");
            Assert.LessOrEqual(colors, maxColors);
            SaveImage(testName, ref32bpp);
        }

        [TestCaseSource(nameof(QuantizerBatchTestSource)), Explicit]
        public void BatchQuantizeTest(string testName, IQuantizer quantizer, int maxColors)
        {
            var files = new string[]
            {
                @"..\..\..\..\..\..\Help\Images\Shield256.png",
                @"..\..\..\..\..\..\Help\Images\Lena.png",
                @"..\..\..\..\..\..\Help\Images\Cameraman.png",
                //@"..\..\..\..\..\..\Help\Images\AlphaGradient.png",
                //@"..\..\..\..\..\..\Help\Images\GrayShades.gif",
            };

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            foreach (string file in files)
            {
                string fileName = Path.IsPathRooted(file) ? file : Path.Combine(Files.GetExecutingPath(), file);
                using var bitmap = new Bitmap(fileName);
                bitmap.Quantize(quantizer);
                int colors = bitmap.GetColors(forceScanningContent: true).Length;
                Console.WriteLine($"{testName} - {colors} colors");
                Assert.LessOrEqual(colors, maxColors);
                SaveImage($"{Path.GetFileNameWithoutExtension(file)} {maxColors} {testName}", bitmap);
            }
            stopwatch.Stop();
            Console.WriteLine(stopwatch.Elapsed);
        }

        [TestCaseSource(nameof(QuantizerBatchTestSource)), Explicit]
        public void BatchDitherTest(string testName, IQuantizer quantizer, int maxColors)
        {
            string[] files =
            {
                ////@"..\..\..\..\..\..\Help\Images\Information256.png",
                @"..\..\..\..\..\..\Help\Images\Shield256.png",
                //////@"..\..\..\..\..\..\Help\Images\AlphaGradient.png",
                @"..\..\..\..\..\..\Help\Images\Lena.png",
                @"..\..\..\..\..\..\Help\Images\Cameraman.png",
                //@"..\..\..\..\..\..\Help\Images\GrayShades.gif",
                //@"D:\Dokumentumok\Képek\Formats\_test\GrangerRainbow.png",
                //@"D:\Dokumentumok\Képek\Formats\_test\LinearVsSrgbContinuous.png",
            };

            (IDitherer Ditherer, string Name)[] ditherers =
            {
                //(null, " No Dithering"),
                (OrderedDitherer.Bayer2x2, nameof(OrderedDitherer.Bayer2x2)),
                ////(OrderedDitherer.Bayer3x3, nameof(OrderedDitherer.Bayer3x3)),
                ////(OrderedDitherer.Bayer4x4, nameof(OrderedDitherer.Bayer4x4)),
                (OrderedDitherer.Bayer8x8, nameof(OrderedDitherer.Bayer8x8)),
                //(OrderedDitherer.BlueNoise, nameof(OrderedDitherer.BlueNoise)),
                //(OrderedDitherer.DottedHalftone, nameof(OrderedDitherer.DottedHalftone)),
                (ErrorDiffusionDitherer.FloydSteinberg, nameof(ErrorDiffusionDitherer.FloydSteinberg)),
                ////(ErrorDiffusionDitherer.JarvisJudiceNinke, nameof(ErrorDiffusionDitherer.JarvisJudiceNinke)),
                ////(ErrorDiffusionDitherer.Stucki, nameof(ErrorDiffusionDitherer.Stucki)),
                ////(ErrorDiffusionDitherer.Burkes, nameof(ErrorDiffusionDitherer.Burkes)),
                ////(ErrorDiffusionDitherer.Sierra3, nameof(ErrorDiffusionDitherer.Sierra3)),
                ////(ErrorDiffusionDitherer.Sierra2, nameof(ErrorDiffusionDitherer.Sierra2)),
                ////(ErrorDiffusionDitherer.SierraLite, nameof(ErrorDiffusionDitherer.SierraLite)),
                //(ErrorDiffusionDitherer.StevensonArce, nameof(ErrorDiffusionDitherer.StevensonArce)),
                (new RandomNoiseDitherer(0f, 0), nameof(RandomNoiseDitherer)),
                (new InterleavedGradientNoiseDitherer(), nameof(InterleavedGradientNoiseDitherer)),
            };

            foreach (string file in files)
            {
                string fileName = Path.IsPathRooted(file) ? file : Path.Combine(Files.GetExecutingPath(), file);
                using var bitmap = new Bitmap(fileName);
                foreach (var ditherer in ditherers)
                {
                    using var bmp = bitmap.CloneCurrentFrame();
                    if (ditherer.Ditherer == null)
                        bmp.Quantize(quantizer);
                    else
                        bmp.Dither(quantizer, ditherer.Ditherer);
                    //int colors = bmp.GetColors(forceScanningContent: true).Length;
                    //Console.WriteLine($"{testName} - {colors} colors");
                    //Assert.LessOrEqual(colors, maxColors);
                    SaveImage($"{Path.GetFileNameWithoutExtension(file)} {maxColors} {testName} {ditherer.Name}", bmp);
                }
            }
        }

        [TestCase(PixelFormat.Format1bppIndexed, 0xFFFFFFFF)]
        [TestCase(PixelFormat.Format4bppIndexed, 0xFF0000FF)]
        [TestCase(PixelFormat.Format8bppIndexed, 0xFF0000FF)]
        [TestCase(PixelFormat.Format16bppRgb555, 0xFF0000FF)]
        [TestCase(PixelFormat.Format16bppRgb565, 0xFF0000FF)]
        [TestCase(PixelFormat.Format24bppRgb, 0xFF0000FF)]
        [TestCase(PixelFormat.Format32bppRgb, 0xFF0000FF)]
        [TestCase(PixelFormat.Format32bppArgb, 0xFF0000FF)]
        [TestCase(PixelFormat.Format32bppArgb, 0x800000FF)]
        [TestCase(PixelFormat.Format32bppRgb, 0x800000FF)]
        [TestCase(PixelFormat.Format32bppPArgb, 0xFF0000FF)]
        [TestCase(PixelFormat.Format32bppPArgb, 0x800000FF)]
        [TestCase(PixelFormat.Format48bppRgb, 0xFF0000FF)]
        [TestCase(PixelFormat.Format64bppArgb, 0xFF0000FF)]
        [TestCase(PixelFormat.Format64bppPArgb, 0xFF0000FF)]
        public void ClearTestNative(PixelFormat pixelFormat, uint argb)
        {
            // note: managed is tested by BitmapDataExtensionsTest
            const int size = 17;
            Color color = Color.FromArgb((int)argb);

            using Bitmap bmp = CreateBitmap(size, pixelFormat);
            bmp.Clear(color);
            using (IReadableBitmapData bitmapData = bmp.GetReadableBitmapData())
            {
                IReadableBitmapDataRowMovable row = bitmapData.FirstRow;
                var c32 = new Color32(color);
                if (!pixelFormat.HasAlpha())
                    c32 = c32.Blend(Color32.FromGray(0));
                do
                {
                    for (int x = 0; x < bitmapData.Width; x++)
                        Assert.AreEqual(c32, row[x]);
                } while (row.MoveNextRow());
            }

            SaveImage(pixelFormat.ToString(), bmp);
        }

        [TestCase(PixelFormat.Format1bppIndexed, 0xFF0000FF, false, false)]
        [TestCase(PixelFormat.Format1bppIndexed, 0xFF0000FF, true, false)]
        [TestCase(PixelFormat.Format1bppIndexed, 0xFF0000FF, true, true)]
        public void ClearWithDitheringTest(PixelFormat pixelFormat, uint argb, bool errorDiffusion, bool serpentine)
        {
            const int size = 17;
            Color color = Color.FromArgb((int)argb);

            using var bmp = new Bitmap(size, size, pixelFormat);
            bmp.Clear(color, errorDiffusion ? (IDitherer)ErrorDiffusionDitherer.FloydSteinberg.ConfigureProcessingDirection(serpentine) : OrderedDitherer.Bayer8x8);
            SaveImage($"{pixelFormat} {(errorDiffusion ? $"Error diffusion {(serpentine ? nameof(serpentine) : "raster")}" : "Ordered")}", bmp);
        }

        [Test]
        public void ChangeColorTest()
        {
            using var bmp = Icons.Information.ExtractBitmap(new Size(256, 256));
            Assert.DoesNotThrow(() => bmp.ReplaceColor(Color.Empty, Color.Blue));
            SaveImage(null, bmp);
        }

        [TestCase(PixelFormat.Format8bppIndexed)]
        public void TransformColors(PixelFormat pixelFormat)
        {
            static Color32 Transform(Color32 c) => new Color32(c.A, (byte)(255 - c.R), (byte)(255 - c.G), (byte)(255 - c.B));

            (IDitherer Ditherer, string Name)[] ditherers =
            {
                (null, " No Dithering"),
                (OrderedDitherer.Bayer8x8, nameof(OrderedDitherer.Bayer8x8)),
                (ErrorDiffusionDitherer.FloydSteinberg, $"{nameof(ErrorDiffusionDitherer.FloydSteinberg)} (raster)"),
                (ErrorDiffusionDitherer.FloydSteinberg.ConfigureProcessingDirection(true), $"{nameof(ErrorDiffusionDitherer.FloydSteinberg)} (serpentine)"),
            };

            foreach (var ditherer in ditherers)
            {
                using var bmp = Icons.Information.ExtractBitmap(new Size(256, 256)).ConvertPixelFormat(pixelFormat);
                Assert.DoesNotThrow(() => bmp.TransformColors(Transform, ditherer.Ditherer));
                SaveImage($"{pixelFormat} {ditherer.Name}", bmp);
            }
        }

        [TestCase("32 bit ARGB", PixelFormat.Format32bppArgb, false)]
        [TestCase("8 bit by palette", PixelFormat.Format8bppIndexed, false)]
        [TestCase("8 bit by pixels using dithering", PixelFormat.Format8bppIndexed, true)]
        public void InvertTest(string testName, PixelFormat pixelFormat, bool useDithering)
        {
            using Bitmap bmpOrig = Icons.Information.ExtractBitmap(new Size(256, 256))!.ConvertPixelFormat(pixelFormat);

            using Bitmap srgb = bmpOrig.CloneCurrentFrame();
            Assert.DoesNotThrow(() => srgb.Invert(useDithering ? OrderedDitherer.Bayer8x8 : null));
            SaveImage($"{testName}_sRGB", srgb);

            using Bitmap linear = bmpOrig.CloneCurrentFrame();
            Assert.DoesNotThrow(() =>
            {
                using var bmpData = linear.GetReadWriteBitmapData(WorkingColorSpace.Linear);
                bmpData.Invert(useDithering ? OrderedDitherer.Bayer8x8 : null);
            });
            SaveImage($"{testName}_Linear", linear);
        }

        [TestCase("32 bit ARGB", PixelFormat.Format32bppArgb, false)]
        [TestCase("8 bit by palette", PixelFormat.Format8bppIndexed, false)]
        [TestCase("32 bit by pixels using dithering", PixelFormat.Format8bppIndexed, true)]
        public void MakeOpaqueTest(string testName, PixelFormat pixelFormat, bool useDithering)
        {
            using var bmp = Icons.Information.ExtractBitmap(new Size(256, 256)).ConvertPixelFormat(pixelFormat);
            Assert.DoesNotThrow(() => bmp.MakeOpaque(Color.Blue, useDithering ? OrderedDitherer.Bayer8x8 : null));
            SaveImage(testName, bmp);
        }

        [TestCase("32 bit ARGB", PixelFormat.Format32bppArgb, false)]
        [TestCase("8 bit by palette", PixelFormat.Format8bppIndexed, false)]
        [TestCase("32 bit by pixels using dithering", PixelFormat.Format8bppIndexed, true)]
        public void MakeGrayscaleTest(string testName, PixelFormat pixelFormat, bool useDithering)
        {
            using var bmp = Icons.Information.ExtractBitmap(new Size(256, 256)).ConvertPixelFormat(pixelFormat);
            Assert.DoesNotThrow(() => bmp.MakeGrayscale(useDithering ? OrderedDitherer.Bayer8x8 : null));
            SaveImage(testName, bmp);
        }

        [TestCase("Lighten 100% 32 bit ARGB", PixelFormat.Format32bppArgb, false, 1f)]
        [TestCase("Lighten 100% 8 bit by palette", PixelFormat.Format8bppIndexed, false, 1f)]
        [TestCase("Lighten 100% 8 bit by pixels using dithering", PixelFormat.Format8bppIndexed, true, 1f)]
        [TestCase("Lighten 50% 32 bit ARGB", PixelFormat.Format32bppArgb, false, 0.5f)]
        [TestCase("Lighten 50% 8 bit by palette", PixelFormat.Format8bppIndexed, false, 0.5f)]
        [TestCase("Lighten 50% 8 bit by pixels using dithering", PixelFormat.Format8bppIndexed, true, 0.5f)]
        [TestCase("Darken 100% 32 bit ARGB", PixelFormat.Format32bppArgb, false, -1f)]
        [TestCase("Darken 100% 8 bit by palette", PixelFormat.Format8bppIndexed, false, -1f)]
        [TestCase("Darken 100% 8 bit by pixels using dithering", PixelFormat.Format8bppIndexed, true, -1f)]
        [TestCase("Darken 50% 32 bit ARGB", PixelFormat.Format32bppArgb, false, -0.5f)]
        [TestCase("Darken 50% 8 bit by palette", PixelFormat.Format8bppIndexed, false, -0.5f)]
        [TestCase("Darken 50% 8 bit by pixels using dithering", PixelFormat.Format8bppIndexed, true, -0.5f)]
        public void AdjustBrightnessTest(string testName, PixelFormat pixelFormat, bool useDithering, float brightness)
        {
            using Bitmap bmpOrig = Icons.Information.ExtractBitmap(new Size(256, 256))!.ConvertPixelFormat(pixelFormat);

            using Bitmap srgb = bmpOrig.CloneCurrentFrame();
            Assert.DoesNotThrow(() => srgb.AdjustBrightness(brightness, useDithering ? OrderedDitherer.Bayer8x8 : null));
            SaveImage($"{testName}_sRGB", srgb);

            using Bitmap linear = bmpOrig.CloneCurrentFrame();
            Assert.DoesNotThrow(() =>
            {
                using var bmpData = linear.GetReadWriteBitmapData(WorkingColorSpace.Linear);
                bmpData.AdjustBrightness(brightness, useDithering ? OrderedDitherer.Bayer8x8 : null);
            });
            SaveImage($"{testName}_Linear", linear);
        }

        [TestCase("Increase 100% 32 bit ARGB", PixelFormat.Format32bppArgb, false, 1f)]
        [TestCase("Increase 100% 8 bit by palette", PixelFormat.Format8bppIndexed, false, 1f)]
        [TestCase("Increase 100% 8 bit by pixels using dithering", PixelFormat.Format8bppIndexed, true, 1f)]
        [TestCase("Increase 50% 32 bit ARGB", PixelFormat.Format32bppArgb, false, 0.5f)]
        [TestCase("Increase 50% 8 bit by palette", PixelFormat.Format8bppIndexed, false, 0.5f)]
        [TestCase("Increase 50% 8 bit by pixels using dithering", PixelFormat.Format8bppIndexed, true, 0.5f)]
        [TestCase("Decrease 100% 32 bit ARGB", PixelFormat.Format32bppArgb, false, -1f)]
        [TestCase("Decrease 100% 8 bit by palette", PixelFormat.Format8bppIndexed, false, -1f)]
        [TestCase("Decrease 100% 8 bit by pixels using dithering", PixelFormat.Format8bppIndexed, true, -1f)]
        [TestCase("Decrease 50% 32 bit ARGB", PixelFormat.Format32bppArgb, false, -0.5f)]
        [TestCase("Decrease 50% 8 bit by palette", PixelFormat.Format8bppIndexed, false, -0.5f)]
        [TestCase("Decrease 50% 8 bit by pixels using dithering", PixelFormat.Format8bppIndexed, true, -0.5f)]
        public void AdjustContrastTest(string testName, PixelFormat pixelFormat, bool useDithering, float contrast)
        {
            using Bitmap bmpOrig = Icons.Information.ExtractBitmap(new Size(256, 256))!.ConvertPixelFormat(pixelFormat);

            using Bitmap srgb = bmpOrig.CloneCurrentFrame();
            Assert.DoesNotThrow(() => srgb.AdjustContrast(contrast, useDithering ? OrderedDitherer.Bayer8x8 : null));
            SaveImage($"{testName}_sRGB", srgb);

            using Bitmap linear = bmpOrig.CloneCurrentFrame();
            Assert.DoesNotThrow(() =>
            {
                using var bmpData = linear.GetReadWriteBitmapData(WorkingColorSpace.Linear);
                bmpData.AdjustContrast(contrast, useDithering ? OrderedDitherer.Bayer8x8 : null);
            });
            SaveImage($"{testName}_Linear", linear);
        }

        [TestCase("10 32 bit ARGB", PixelFormat.Format32bppArgb, false, 10f)]
        [TestCase("10 8 bit by palette", PixelFormat.Format8bppIndexed, false, 10f)]
        [TestCase("10 8 bit by pixels using dithering", PixelFormat.Format8bppIndexed, true, 10f)]
        [TestCase("2.5 32 bit ARGB", PixelFormat.Format32bppArgb, false, 2.5f)]
        [TestCase("2.5 8 bit by palette", PixelFormat.Format8bppIndexed, false, 2.5f)]
        [TestCase("2.5 8 bit by pixels using dithering", PixelFormat.Format8bppIndexed, true, 2.5f)]
        [TestCase("0 32 bit ARGB", PixelFormat.Format32bppArgb, false, 0f)]
        [TestCase("0 8 bit by palette", PixelFormat.Format8bppIndexed, false, 0f)]
        [TestCase("0 8 bit by pixels using dithering", PixelFormat.Format8bppIndexed, true, 0f)]
        [TestCase("0.25 32 bit ARGB", PixelFormat.Format32bppArgb, false, 0.25f)]
        [TestCase("0.25 8 bit by palette", PixelFormat.Format8bppIndexed, false, 0.25f)]
        [TestCase("0.25 8 bit by pixels using dithering", PixelFormat.Format8bppIndexed, true, 0.25f)]
        public void AdjustGammaTest(string testName, PixelFormat pixelFormat, bool useDithering, float gamma)
        {
            using Bitmap bmpOrig = Icons.Information.ExtractBitmap(new Size(256, 256))!.ConvertPixelFormat(pixelFormat);

            using Bitmap srgb = bmpOrig.CloneCurrentFrame();
            Assert.DoesNotThrow(() => srgb.AdjustGamma(gamma, useDithering ? OrderedDitherer.Bayer8x8 : null));
            SaveImage($"{testName}_sRGB", srgb);

            using Bitmap linear = bmpOrig.CloneCurrentFrame();
            Assert.DoesNotThrow(() =>
            {
                using var bmpData = linear.GetReadWriteBitmapData(WorkingColorSpace.Linear);
                bmpData.AdjustGamma(gamma, useDithering ? OrderedDitherer.Bayer8x8 : null);
            });
            SaveImage($"{testName}_Linear", linear);
        }

        #endregion
    }
}
