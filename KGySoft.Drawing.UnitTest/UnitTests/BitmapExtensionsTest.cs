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
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using KGySoft.CoreLibraries;
using KGySoft.Diagnostics;
using KGySoft.Drawing.Imaging;
using KGySoft.Drawing.WinApi;

using NUnit.Framework;

#endregion

namespace KGySoft.Drawing.UnitTests
{
    [TestFixture]
    public class BitmapExtensionsTest : TestBase
    {
        #region Fields

        private static readonly object[][] quantizeTestSource =
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

            new object[] { "Wu 256 Black", OptimizedPaletteQuantizer.MedianCut(256, Color.Black, 0), 256 },
            new object[] { "Wu 16 Black", OptimizedPaletteQuantizer.MedianCut(16, Color.Black, 0), 16 },
            new object[] { "Wu 4 Black", OptimizedPaletteQuantizer.MedianCut(4, Color.Black, 0), 4 },
            new object[] { "Wu 2 Black", OptimizedPaletteQuantizer.MedianCut(2, Color.Black, 0), 2 },
            new object[] { "Wu 256 White", OptimizedPaletteQuantizer.MedianCut(256, Color.White, 0), 256 },
            new object[] { "Wu 256 TR", OptimizedPaletteQuantizer.MedianCut(256, Color.White), 256 },
        };

        private static readonly object[][] quantizerBatchTestSource =
        {
            //new object[] { "RGB888 Black", PredefinedColorsQuantizer.Rgb888(), 1 << 24 },
            //new object[] { "RGB888 White", PredefinedColorsQuantizer.Rgb888(Color.White), 1 << 24 },
            //new object[] { "RGB565 Black", PredefinedColorsQuantizer.Rgb565(), 1 << 16 },
            //new object[] { "RGB565 White", PredefinedColorsQuantizer.Rgb565(Color.White), 1 << 16 },
            //new object[] { "RGB555 Black", PredefinedColorsQuantizer.Rgb555(), 1 << 15 },
            //new object[] { "RGB555 White", PredefinedColorsQuantizer.Rgb555(Color.White), 1 << 15 },
            //new object[] { "ARGB1555 Black 50%", PredefinedColorsQuantizer.Argb1555(), (1 << 15) + 1 },
            //new object[] { "ARGB1555 White 50%", PredefinedColorsQuantizer.Argb1555(Color.White), (1 << 15) + 1 },
            //new object[] { "ARGB1555 Black 0", PredefinedColorsQuantizer.Argb1555(default, 0), (1 << 15) + 1 },
            //new object[] { "ARGB1555 Black 1", PredefinedColorsQuantizer.Argb1555(default, 1), (1 << 15) + 1 },
            //new object[] { "ARGB1555 Black 254", PredefinedColorsQuantizer.Argb1555(default, 254), (1 << 15) + 1 },
            //new object[] { "RGB332 Black Slow", PredefinedColorsQuantizer.Rgb332(), 256 },
            //new object[] { "RGB332 Black Fast", PredefinedColorsQuantizer.Rgb332(directMapping: true), 256 },
            //new object[] { "RGB332 White Slow", PredefinedColorsQuantizer.Rgb332(Color.White), 256 },
            //new object[] { "Grayscale256 Black", PredefinedColorsQuantizer.Grayscale(), 256 },
            //new object[] { "Grayscale256 White", PredefinedColorsQuantizer.Grayscale(Color.White), 256 },
            //new object[] { "Grayscale16 Black Slow", PredefinedColorsQuantizer.Grayscale16(), 16 },
            //new object[] { "Grayscale16 Black Fast", PredefinedColorsQuantizer.Grayscale16(directMapping: true), 16 },
            //new object[] { "Grayscale5 black", PredefinedColorsQuantizer.FromCustomPalette(new[] { Color.Black, Color.FromArgb(64, 64, 64), Color.Gray, Color.FromArgb(192, 192, 192), Color.White }), 5 },
            //new object[] { "Grayscale4 Black Slow", PredefinedColorsQuantizer.Grayscale4(), 4 },
            //new object[] { "Grayscale4 Black Fast", PredefinedColorsQuantizer.Grayscale4(directMapping: true), 4 },
            //new object[] { "Grayscale3 Black", PredefinedColorsQuantizer.FromCustomPalette(new[] { Color.Black, Color.Gray, Color.White }), 3 },
            //new object[] { "BW Black", PredefinedColorsQuantizer.BlackAndWhite(), 2 },
            //new object[] { "BW White", PredefinedColorsQuantizer.BlackAndWhite(Color.White), 2 },
            //new object[] { "BW Lime", PredefinedColorsQuantizer.BlackAndWhite(Color.Lime), 2 },
            //new object[] { "BW Blue", PredefinedColorsQuantizer.BlackAndWhite(Color.Blue), 2 },
            //new object[] { "Default8Bpp Black", PredefinedColorsQuantizer.SystemDefault8BppPalette(), 256 },
            //new object[] { "Default8Bpp White", PredefinedColorsQuantizer.SystemDefault8BppPalette(Color.White), 256 },
            //new object[] { "Default4Bpp Black", PredefinedColorsQuantizer.SystemDefault4BppPalette(), 16 },
            //new object[] { "Default4Bpp White", PredefinedColorsQuantizer.SystemDefault4BppPalette(Color.White), 16 },
            //new object[] { "Default1Bpp Black", PredefinedColorsQuantizer.SystemDefault1BppPalette(), 2 },
            //new object[] { "Default1Bpp White", PredefinedColorsQuantizer.SystemDefault1BppPalette(Color.White), 2 },
            //new object[] { "Custom Black", PredefinedColorsQuantizer.FromCustomPalette(new[] { Color.Black, Color.White, Color.Red, Color.Blue, Color.Green, Color.Magenta, Color.Yellow, Color.Cyan }), 8 },
            //new object[] { "Custom White", PredefinedColorsQuantizer.FromCustomPalette(new[] { Color.Black, Color.White, Color.Red, Color.Blue, Color.Green, Color.Magenta, Color.Yellow, Color.Cyan }, Color.White), 8 },

            new object[] { "Octree 256 Black", OptimizedPaletteQuantizer.Octree(256, Color.Black, 0), 256 },
            //new object[] { "Octree 32 Black", OptimizedPaletteQuantizer.Octree(32, Color.Black, 0), 32 },
            //new object[] { "Octree 16 Black", OptimizedPaletteQuantizer.Octree(16, Color.Black, 0), 16 },
            //new object[] { "Octree 8 Black", OptimizedPaletteQuantizer.Octree(8, Color.Black, 0), 8 },
            //new object[] { "Octree 7 Black", OptimizedPaletteQuantizer.Octree(7, Color.Black, 0), 7 },
            //new object[] { "Octree 6 Black", OptimizedPaletteQuantizer.Octree(6, Color.Black, 0), 6 },
            //new object[] { "Octree 5 Black", OptimizedPaletteQuantizer.Octree(5, Color.Black, 0), 5 },
            //new object[] { "Octree 4 Black", OptimizedPaletteQuantizer.Octree(4, Color.Black, 0), 4 },
            //new object[] { "Octree 3 Black", OptimizedPaletteQuantizer.Octree(3, Color.Black, 0), 3 },
            //new object[] { "Octree 2 Black", OptimizedPaletteQuantizer.Octree(2, Color.Black, 0), 2 },
            //new object[] { "Octree 256 Silver", OptimizedPaletteQuantizer.Octree(256, Color.Silver, 0), 256 },
            //new object[] { "Octree 16 Silver", OptimizedPaletteQuantizer.Octree(16, Color.Silver, 0), 16 },
            //new object[] { "Octree 4 Silver", OptimizedPaletteQuantizer.Octree(4, Color.Silver, 0), 4 },
            //new object[] { "Octree 3 Silver", OptimizedPaletteQuantizer.Octree(3, Color.Silver, 0), 3 },
            //new object[] { "Octree 2 Silver", OptimizedPaletteQuantizer.Octree(2, Color.Silver, 0), 2 },
            //new object[] { "Octree 256 TR", OptimizedPaletteQuantizer.Octree(256), 256 },
            //new object[] { "Octree 16 TR", OptimizedPaletteQuantizer.Octree(16), 16 },
            //new object[] { "Octree 4 TR", OptimizedPaletteQuantizer.Octree(4), 4 },
            //new object[] { "Octree 3 TR", OptimizedPaletteQuantizer.Octree(3), 3 },
            //new object[] { "Octree 2 TR", OptimizedPaletteQuantizer.Octree(2), 2 },

            new object[] { "MedianCut 256 Black", OptimizedPaletteQuantizer.MedianCut(256, Color.Black, 0), 256 },
            //new object[] { "MedianCut 16 Black", OptimizedPaletteQuantizer.MedianCut(16, Color.Black, 0), 16 },
            //new object[] { "MedianCut 4 Black", OptimizedPaletteQuantizer.MedianCut(4, Color.Black, 0), 4 },
            //new object[] { "MedianCut 3 Black", OptimizedPaletteQuantizer.MedianCut(3, Color.Black, 0), 3 },
            //new object[] { "MedianCut 2 Black", OptimizedPaletteQuantizer.MedianCut(2, Color.Black, 0), 2 },
            //new object[] { "MedianCut 256 Silver", OptimizedPaletteQuantizer.MedianCut(256, Color.Silver, 0), 256 },
            //new object[] { "MedianCut 16 Silver", OptimizedPaletteQuantizer.MedianCut(16, Color.Silver, 0), 16 },
            //new object[] { "MedianCut 4 Silver", OptimizedPaletteQuantizer.MedianCut(4, Color.Silver, 0), 4 },
            //new object[] { "MedianCut 3 Silver", OptimizedPaletteQuantizer.MedianCut(3, Color.Silver, 0), 3 },
            //new object[] { "MedianCut 2 Silver", OptimizedPaletteQuantizer.MedianCut(2, Color.Silver, 0), 2 },
            //new object[] { "MedianCut 256 TR", OptimizedPaletteQuantizer.MedianCut(256), 256 },
            //new object[] { "MedianCut 16 TR", OptimizedPaletteQuantizer.MedianCut(16), 16 },
            //new object[] { "MedianCut 4 TR", OptimizedPaletteQuantizer.MedianCut(4), 4 },
            //new object[] { "MedianCut 3 TR", OptimizedPaletteQuantizer.MedianCut(3), 3 },
            //new object[] { "MedianCut 2 TR", OptimizedPaletteQuantizer.MedianCut(2), 2 },

            new object[] { "Wu 256 Black", OptimizedPaletteQuantizer.Wu(256, Color.Black, 0), 256 },
            //new object[] { "Wu 16 Black", OptimizedPaletteQuantizer.Wu(16, Color.Black, 0), 16 },
            //new object[] { "Wu 4 Black", OptimizedPaletteQuantizer.Wu(4, Color.Black, 0), 4 },
            //new object[] { "Wu 3 Black", OptimizedPaletteQuantizer.Wu(3, Color.Black, 0), 3 },
            //new object[] { "Wu 2 Black", OptimizedPaletteQuantizer.Wu(2, Color.Black, 0), 2 },
            //new object[] { "Wu 256 Silver", OptimizedPaletteQuantizer.Wu(256, Color.Silver, 0), 256 },
            //new object[] { "Wu 16 Silver", OptimizedPaletteQuantizer.Wu(16, Color.Silver, 0), 16 },
            //new object[] { "Wu 4 Silver", OptimizedPaletteQuantizer.Wu(4, Color.Silver, 0), 4 },
            //new object[] { "Wu 3 Silver", OptimizedPaletteQuantizer.Wu(3, Color.Silver, 0), 3 },
            //new object[] { "Wu 2 Silver", OptimizedPaletteQuantizer.Wu(2, Color.Silver, 0), 2 },
            //new object[] { "Wu 256 TR", OptimizedPaletteQuantizer.Wu(256), 256 },
            //new object[] { "Wu 16 TR", OptimizedPaletteQuantizer.Wu(16), 16 },
            //new object[] { "Wu 4 TR", OptimizedPaletteQuantizer.Wu(4), 4 },
            //new object[] { "Wu 3 TR", OptimizedPaletteQuantizer.Wu(3), 3 },
            //new object[] { "Wu 2 TR", OptimizedPaletteQuantizer.Wu(2), 2 },
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

        [TestCase(16, 16, 256, 256)]
        [TestCase(256, 256, 16, 16)]
        public void Resize12Test(int sw, int sh, int tw, int th)
        {
            Size sourceSize = new Size(sw, sh);
            Size targetSize = new Size(tw, th);
            using var bmpRef = Icons.Information.ExtractBitmap(sourceSize);

            //using var drawImage = bmpRef.Resize(targetSize);
            //using var bicubic1 = bmpRef.Resize1(tw, th);
            using var bicubic2 = bmpRef.Resize2(targetSize);
            //SaveImage($"{sw}x{sh} to {tw}x{th} {nameof(drawImage)}", drawImage);
            //SaveImage($"{sw}x{sh} to {tw}x{th} {nameof(bicubic1)}", bicubic1);
            //SaveImage($"{sw}x{sh} to {tw}x{th} {nameof(bicubic2)}", bicubic2);
        }


        private static (Size Source, Size Target)[] sizes =
        {
            (new Size(16, 16), new Size(256, 256)),
            (new Size(256, 256), new Size(16, 16)),
        };

        private static object[][] resizeSource =
        {
            new object[] { ScalingMode.NearestNeighbor, "NearestNeighbor" },
            new object[] { ScalingMode.Box, "Box" },
            new object[] { ScalingMode.Bicubic, "Bicubic" },
            new object[] { ScalingMode.Bilinear, "Triangle" },
            new object[] { ScalingMode.CatmullRom, "CatmullRom" },
            new object[] { ScalingMode.MitchellNetravali, "MitchellNetravali" },
            new object[] { ScalingMode.Robidoux, "Robidoux" },
            new object[] { ScalingMode.Spline, "Spline" },
            new object[] { ScalingMode.Lanczos2, "Lanczos2" },
            new object[] { ScalingMode.Lanczos3, "Lanczos3" },
        };

        [TestCaseSource(nameof(resizeSource))]
        public void Resize2Test(ScalingMode scalingMode, string name)
        {
            foreach (var size in sizes)
            {
                using var bmpRef = Icons.Information.ExtractBitmap(size.Source);
                using var bmpResult = bmpRef.Resize2(size.Target, false, scalingMode);
                SaveImage($"{size.Source.Width}x{size.Source.Height} to {size.Target.Width}x{size.Target.Height} {name}", bmpResult);
            }
        }

        [TestCase(256, ScalingMode.Bicubic)]
        [TestCase(16, ScalingMode.Bicubic)]
        [TestCase(256, ScalingMode.NearestNeighbor)]
        [TestCase(16, ScalingMode.NearestNeighbor)]
        // TODO: to ImageExtensionsTest
        public void DrawIntoWithResizeTest(int size, ScalingMode scalingMode)
        {
            var sourceSize = new Size(size, size);
            var targetSize = new Size(100, 100);
            using var bmpRef = Icons.Information.ExtractBitmap(sourceSize);
            using var bmpResult = new Bitmap(targetSize.Width, targetSize.Height);

            Rectangle srcRect = Rectangle.Round(new RectangleF(size * 0.1f, size * 0.0625f, size * 0.75f, size * 0.85f));

            // no cut
            bmpRef.DrawInto(bmpResult, srcRect, new Rectangle(30, 30, 30, 30), scalingMode);

            // cut left
            bmpRef.DrawInto(bmpResult, srcRect, new Rectangle(-20, 5, 30, 30), scalingMode);

            // cut top
            bmpRef.DrawInto(bmpResult, srcRect, new Rectangle(50, -20, 30, 30), scalingMode);

            // cut right
            bmpRef.DrawInto(bmpResult, srcRect, new Rectangle(90, 50, 30, 30), scalingMode);

            // cut bottom
            bmpRef.DrawInto(bmpResult, srcRect, new Rectangle(10, 90, 30, 30), scalingMode);

            SaveImage($"{scalingMode} {sourceSize.Width}x{sourceSize.Height} to {bmpResult.Width}x{bmpResult.Height}", bmpResult);
        }

        [TestCase(256, ScalingMode.Bicubic)]
        [TestCase(16, ScalingMode.Bicubic)]
        [TestCase(256, ScalingMode.NearestNeighbor)]
        [TestCase(16, ScalingMode.NearestNeighbor)]
        // TODO: to ImageExtensionsTest
        public void DrawIntoWithResizeTooLargeSourceRectangleTest(int size, ScalingMode scalingMode)
        {
            var sourceSize = new Size(size, size);
            var targetSize = new Size(100, 100);
            using var bmpRef = Icons.Information.ExtractBitmap(sourceSize);
            using var bmpResult = new Bitmap(targetSize.Width, targetSize.Height);

            Rectangle srcRect = Rectangle.Round(new RectangleF(size * 0.1f, size * 0.625f, size * 0.75f, size * 0.85f));

            // no cut
            bmpRef.DrawInto(bmpResult, srcRect, new Rectangle(30, 30, 30, 30), scalingMode);

            // cut left
            bmpRef.DrawInto(bmpResult, srcRect, new Rectangle(-20, 5, 30, 30), scalingMode);

            // cut top
            bmpRef.DrawInto(bmpResult, srcRect, new Rectangle(50, -20, 30, 30), scalingMode);

            // cut right
            bmpRef.DrawInto(bmpResult, srcRect, new Rectangle(90, 50, 30, 30), scalingMode);

            // cut bottom
            bmpRef.DrawInto(bmpResult, srcRect, new Rectangle(10, 90, 30, 30), scalingMode);

            SaveImage($"{scalingMode} {sourceSize.Width}x{sourceSize.Height} to {bmpResult.Width}x{bmpResult.Height}", bmpResult);
        }

        [TestCase(16, 16, 256, 256)]
        [TestCase(256, 256, 16, 16)]
        public void ResizePerfTest(int sw, int sh, int tw, int th)
        {
            Size sourceSize = new Size(sw, sh);
            Size targetSize = new Size(tw, th);
            using var bmpRef = Icons.Information.ExtractBitmap(sourceSize);

            var perfTest = new PerformanceTest { Iterations = 10, CpuAffinity = null, TestName = $"{sw}x{sh} to {tw}x{th}" };
            foreach (var mode in new[] { InterpolationMode.NearestNeighbor, InterpolationMode.Bilinear, InterpolationMode.HighQualityBicubic })
            {
                perfTest.AddCase(() =>
                {
                    using Bitmap result = new Bitmap(sw, sh);
                    using (Graphics g = Graphics.FromImage(result))
                    {
                        g.InterpolationMode = mode;
                        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        g.DrawImage(bmpRef, new Rectangle(Point.Empty, targetSize), new Rectangle(Point.Empty, sourceSize), GraphicsUnit.Pixel);
                        g.Flush();
                    }

                }, $"DrawImage/{mode}");
            }

            foreach (var resampler in resizeSource)
            {
                perfTest.AddCase(() =>
                {
                    using var result = bmpRef.Resize2(targetSize, false, (ScalingMode)resampler[0]);
                }, $"Resize/{resampler[1]}");
            }

            perfTest.DoTest().DumpResults(Console.Out);

            //==[16x16 to 256x256 Results]================================================
            // Iterations: 10
            // Warming up: Yes
            // Test cases: 19
            // Calling GC.Collect: Yes
            // Forced CPU Affinity: No
            // Cases are sorted by time (quickest first)
            // --------------------------------------------------
            // 1. DrawImage/Bilinear: average time: 0,30 ms
            // 2. DrawImage/NearestNeighbor: average time: 0,34 ms (+0,04 ms / 114,13%)
            // 3. DrawImage/HighQualityBicubic: average time: 0,37 ms (+0,07 ms / 123,16%)
            // 4. Resize/NearestNeighbor: average time: 9,73 ms (+9,43 ms / 3 221,05%)
            // 5. Resize/RobidouxSharp: average time: 12,03 ms (+11,73 ms / 3 982,13%)
            // 6. Resize/Spline: average time: 12,56 ms (+12,26 ms / 4 156,15%)
            // 7. Resize/Lanczos2: average time: 12,72 ms (+12,42 ms / 4 209,20%)
            //!8. Resize/Robidoux: average time: 13,33 ms (+13,03 ms / 4 410,19%)
            // 9. Resize/Lanczos3: average time: 13,73 ms (+13,43 ms / 4 544,14%)
            // 10. Resize/Box: average time: 15,29 ms (+14,99 ms / 5 060,36%)
            //x12. Resize/CatmullRom: average time: 16,56 ms (+16,26 ms / 5 480,84%)
            //!13. Resize/MitchellNetravali: average time: 16,87 ms (+16,57 ms / 5 583,36%)
            // 14. Resize/Bicubic: average time: 18,18 ms (+17,88 ms / 6 017,41%)
            //x15. Resize/Welch: average time: 18,28 ms (+17,98 ms / 6 048,08%)
            // 17. Resize/Triangle: average time: 21,14 ms (+20,84 ms / 6 997,02%)

            // ==[256x256 to 16x16 Results]================================================
            // Iterations: 10
            // Warming up: Yes
            // Test cases: 19
            // Calling GC.Collect: Yes
            // Forced CPU Affinity: No
            // Cases are sorted by time (quickest first)
            // --------------------------------------------------
            // 1. Resize/NearestNeighbor: average time: 0,21 ms
            // 3. DrawImage/NearestNeighbor: average time: 2,68 ms (+2,46 ms / 1 259,51%)
            // 4. DrawImage/Bilinear: average time: 2,90 ms (+2,69 ms / 1 364,60%)
            // 5. Resize/Triangle: average time: 6,55 ms (+6,33 ms / 3 081,64%)
            // 6. Resize/Box: average time: 6,62 ms (+6,41 ms / 3 118,69%)
            // 7. DrawImage/HighQualityBicubic: average time: 7,47 ms (+7,25 ms / 3 514,92%)
            // 8. Resize/Robidoux: average time: 8,07 ms (+7,85 ms / 3 797,65%)
            // 9. Resize/RobidouxSharp: average time: 8,09 ms (+7,87 ms / 3 806,59%)
            // 10. Resize/CatmullRom: average time: 8,15 ms (+7,94 ms / 3 835,97%)
            // 11. Resize/Hermite: average time: 8,21 ms (+7,99 ms / 3 863,47%)
            // 12. Resize/Spline: average time: 8,40 ms (+8,19 ms / 3 955,93%)
            // 13. Resize/Lanczos2: average time: 8,60 ms (+8,39 ms / 4 049,01%)
            // 14. Resize/MitchellNetravali: average time: 9,40 ms (+9,19 ms / 4 426,79%)
            // 15. Resize/Lanczos3: average time: 9,94 ms (+9,73 ms / 4 680,27%)
            // 16. Resize/Welch: average time: 11,11 ms (+10,90 ms / 5 231,17%)
            // 17. Resize/Bicubic: average time: 11,67 ms (+11,45 ms / 5 492,84%)
            // 18. Resize/Lanczos5: average time: 15,53 ms (+15,32 ms / 7 311,21%)
            // 19. Resize/Lanczos8: average time: 16,49 ms (+16,28 ms / 7 765,44%)

            /*
            ==[16x16 to 256x256 Results]================================================
            Iterations: 10
            Warming up: Yes
            Test cases: 13
            Calling GC.Collect: Yes
            Forced CPU Affinity: No
            Cases are sorted by time (quickest first)
            --------------------------------------------------
            1. DrawImage/NearestNeighbor: average time: 0,23 ms
            2. DrawImage/Bilinear: average time: 0,32 ms (+0,09 ms / 138,19%)
            3. DrawImage/HighQualityBicubic: average time: 0,37 ms (+0,14 ms / 160,51%)
            4. Resize/NearestNeighbor: average time: 12,93 ms (+12,70 ms / 5 605,50%)
            5. Resize/Triangle: average time: 20,79 ms (+20,56 ms / 9 012,96%)
            6. Resize/Box: average time: 25,69 ms (+25,46 ms / 11 135,02%)
            7. Resize/MitchellNetravali: average time: 27,64 ms (+27,41 ms / 11 981,40%)
            8. Resize/Bicubic: average time: 27,68 ms (+27,45 ms / 11 997,62%)
            9. Resize/CatmullRom: average time: 28,04 ms (+27,81 ms / 12 155,27%)
            10. Resize/Lanczos2: average time: 29,21 ms (+28,97 ms / 12 659,56%)
            11. Resize/Robidoux: average time: 29,47 ms (+29,24 ms / 12 775,51%)
            12. Resize/Lanczos3: average time: 35,74 ms (+35,51 ms / 15 493,28%)
            13. Resize/Spline: average time: 36,55 ms (+36,32 ms / 15 843,87%) 
            
            ==[256x256 to 16x16 Results]================================================
            Iterations: 10
            Warming up: Yes
            Test cases: 13
            Calling GC.Collect: Yes
            Forced CPU Affinity: No
            Cases are sorted by time (quickest first)
            --------------------------------------------------
            1. Resize/NearestNeighbor: average time: 0,40 ms
            2. DrawImage/Bilinear: average time: 2,45 ms (+2,04 ms / 604,20%)
            3. DrawImage/NearestNeighbor: average time: 2,55 ms (+2,15 ms / 629,83%)
            4. DrawImage/HighQualityBicubic: average time: 7,61 ms (+7,20 ms / 1 878,88%)
            5. Resize/Box: average time: 18,04 ms (+17,63 ms / 4 455,37%)
            6. Resize/Triangle: average time: 22,41 ms (+22,00 ms / 5 534,53%)
            7. Resize/MitchellNetravali: average time: 23,62 ms (+23,22 ms / 5 833,56%)
            8. Resize/Spline: average time: 23,80 ms (+23,40 ms / 5 879,18%)
            9. Resize/Robidoux: average time: 23,83 ms (+23,42 ms / 5 885,18%)
            10. Resize/Lanczos2: average time: 26,59 ms (+26,18 ms / 6 566,29%)
            11. Resize/CatmullRom: average time: 30,35 ms (+29,94 ms / 7 494,52%)
            12. Resize/Bicubic: average time: 30,73 ms (+30,33 ms / 7 589,87%)
            13. Resize/Lanczos3: average time: 33,03 ms (+32,63 ms / 8 158,09%)
            */
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
        public void ResizeWithFormatTest(PixelFormat pixelFormat)
        {
            using var bmpRef = Convert(Icons.Information.ExtractBitmap(new Size(256, 256)), pixelFormat);
            var newSize = new Size(256, 64);
            using var resized = bmpRef.Resize(newSize, false);
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

        [TestCaseSource(nameof(quantizeTestSource))]
        public void QuantizeTest(string testName, IQuantizer quantizer, int maxColors)
        {
            //using var ref32bpp = new Bitmap(@"D:\Dokumentumok\Képek\Formats\_test\Quantum_frog.png");
            using Bitmap ref32bpp = Icons.Information.ExtractBitmap(new Size(256, 256));
            ref32bpp.Quantize(quantizer);
            int colors = ref32bpp.GetColors(forceScanningContent: true).Length;
            Console.WriteLine($"{testName} - {colors} colors");
            Assert.LessOrEqual(colors, maxColors);
            SaveImage(testName, ref32bpp);
        }

        [TestCaseSource(nameof(quantizerBatchTestSource)), Explicit]
        public void BatchQuantizeTest(string testName, IQuantizer quantizer, int maxColors)
        {
            var files = new string[]
            {
                @"D:\Dokumentumok\Képek\Formats\_test\Hue_alpha_falloff.png",

                //@"..\..\..\..\KGySoft.Drawing\Help\Images\Shield256.png",
                //@"..\..\..\..\KGySoft.Drawing\Help\Images\Lena.png",
                //@"..\..\..\..\KGySoft.Drawing\Help\Images\Cameraman.png",
                //@"..\..\..\..\KGySoft.Drawing\Help\Images\AlphaGradient.png",
                //@"..\..\..\..\KGySoft.Drawing\Help\Images\GrayShades.gif",
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

        [TestCaseSource(nameof(quantizerBatchTestSource)), Explicit]
        public void BatchDitherTest(string testName, IQuantizer quantizer, int maxColors)
        {
            string[] files =
            {
                //@"..\..\..\..\KGySoft.Drawing\Help\Images\Information256.png",
                //@"..\..\..\..\KGySoft.Drawing\Help\Images\Shield256.png",
                //@"..\..\..\..\KGySoft.Drawing\Help\Images\AlphaGradient.png",
                @"..\..\..\..\KGySoft.Drawing\Help\Images\Lena.png",
                @"..\..\..\..\KGySoft.Drawing\Help\Images\Cameraman.png",
                @"..\..\..\..\KGySoft.Drawing\Help\Images\GrayShades.gif",
            };

            (IDitherer Ditherer, string Name)[] ditherers =
            {
                (null, " No Dithering"),
                //(OrderedDitherer.Bayer2x2, nameof(OrderedDitherer.Bayer2x2)),
                //(OrderedDitherer.Bayer3x3, nameof(OrderedDitherer.Bayer3x3)),
                //(OrderedDitherer.Bayer4x4, nameof(OrderedDitherer.Bayer4x4)),
                (OrderedDitherer.Bayer8x8, nameof(OrderedDitherer.Bayer8x8)),
                //(OrderedDitherer.BlueNoise, nameof(OrderedDitherer.BlueNoise)),
                (ErrorDiffusionDitherer.FloydSteinberg, nameof(ErrorDiffusionDitherer.FloydSteinberg)),
                //(ErrorDiffusionDitherer.JarvisJudiceNinke, nameof(ErrorDiffusionDitherer.JarvisJudiceNinke)),
                //(ErrorDiffusionDitherer.Stucki, nameof(ErrorDiffusionDitherer.Stucki)),
                //(ErrorDiffusionDitherer.Burkes, nameof(ErrorDiffusionDitherer.Burkes)),
                //(ErrorDiffusionDitherer.Sierra3, nameof(ErrorDiffusionDitherer.Sierra3)),
                //(ErrorDiffusionDitherer.Sierra2, nameof(ErrorDiffusionDitherer.Sierra2)),
                //(ErrorDiffusionDitherer.SierraLite, nameof(ErrorDiffusionDitherer.SierraLite)),
                //(ErrorDiffusionDitherer.StevensonArce, nameof(ErrorDiffusionDitherer.StevensonArce)),
                //(new RandomNoiseDitherer(0, 0), nameof(RandomNoiseDitherer)),
                //(new InterleavedGradientNoiseDitherer(0), nameof(InterleavedGradientNoiseDitherer)),
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

        [Test, Explicit]
        public void GenerateBlueNoiseMatrix()
        {
            using Bitmap texture = new Bitmap(@"D:\Dokumentumok\Képek\Formats\BlueNoiseTextures\64\LDR_LLL1_13.png");
            using IReadableBitmapData accessor = texture.GetReadableBitmapData();

            IReadableBitmapDataRow row = accessor.FirstRow;
            Console.WriteLine($"private static readonly byte[,] blueNoise{accessor.Width} =");
            Console.WriteLine("{");
            do
            {
                Console.Write("    { ");
                for (int x = 0; x < accessor.Width; x++)
                {
                    Console.Write(row[x].R);
                    if (x < accessor.Width - 1)
                        Console.Write(", ");
                }
                Console.WriteLine(" }, ");
            } while (row.MoveNextRow());

            Console.WriteLine("};");
        }

        [TestCase(PixelFormat.Format32bppArgb, 0xFF0000FF)]
        [TestCase(PixelFormat.Format32bppArgb, 0x800000FF)]
        [TestCase(PixelFormat.Format32bppRgb, 0xFF0000FF)]
        [TestCase(PixelFormat.Format32bppRgb, 0x800000FF)]
        [TestCase(PixelFormat.Format32bppPArgb, 0xFF0000FF)]
        [TestCase(PixelFormat.Format32bppPArgb, 0x800000FF)]
        [TestCase(PixelFormat.Format16bppRgb555, 0xFF0000FF)]
        [TestCase(PixelFormat.Format64bppArgb, 0xFF0000FF)]
        [TestCase(PixelFormat.Format8bppIndexed, 0xFF0000FF)]
        [TestCase(PixelFormat.Format4bppIndexed, 0xFF0000FF)]
        [TestCase(PixelFormat.Format1bppIndexed, 0xFFFFFFFF)]
        public void ClearTest(PixelFormat pixelFormat, uint argb)
        {
            const int size = 17;
            Color color = Color.FromArgb((int)argb);

            using var bmp = CreateBitmap(size, pixelFormat);
            bmp.Clear(color);
            using (IReadableBitmapData bitmapData = bmp.GetReadableBitmapData())
            {
                IReadableBitmapDataRow row = bitmapData.FirstRow;
                var c32 = new Color32(color);
                if (!pixelFormat.HasTransparency())
                    c32 = c32.BlendWithBackground(default);
                do
                {
                    for (int x = 0; x < bitmapData.Width; x++)
                        Assert.AreEqual(c32, row[x]);
                } while (row.MoveNextRow());
            }

            SaveImage(pixelFormat.ToString(), bmp);
        }

        [TestCase(PixelFormat.Format1bppIndexed, 0xFF0000FF, false)]
        [TestCase(PixelFormat.Format1bppIndexed, 0xFF0000FF, true)]
        public void ClearWithDitheringTest(PixelFormat pixelFormat, uint argb, bool errorDiffusion)
        {
            const int size = 17;
            Color color = Color.FromArgb((int)argb);

            using var bmp = new Bitmap(size, size, pixelFormat);
            bmp.Clear(color, errorDiffusion ? (IDitherer)ErrorDiffusionDitherer.FloydSteinberg : OrderedDitherer.Bayer8x8);
            SaveImage($"{pixelFormat} {(errorDiffusion ? "Error diffusion" : "Ordered")}", bmp);
        }

        [Test]
        public void ChangeColorTest()
        {
            using var bmp = Icons.Information.ExtractBitmap(new Size(256, 256));
            Assert.DoesNotThrow(() => bmp.ReplaceColor(Color.Empty, Color.Blue));
            SaveImage(null, bmp);
        }

        [TestCase("Inverting 32 bit ARGB", PixelFormat.Format32bppArgb, false)]
        [TestCase("Inverting 8 bit by palette", PixelFormat.Format8bppIndexed, false)]
        [TestCase("Inverting 32 bit by pixels using dithering", PixelFormat.Format8bppIndexed, true)]
        public void TransformColors(string testName, PixelFormat pixelFormat, bool useDithering)
        {
            static Color32 Transform(Color32 c) => new Color32(c.A, (byte)(255 - c.R), (byte)(255 - c.G), (byte)(255 - c.B));

            using var bmp = Icons.Information.ExtractBitmap(new Size(256, 256)).ConvertPixelFormat(pixelFormat);
            Assert.DoesNotThrow(() => bmp.TransformColors(Transform, useDithering ? OrderedDitherer.Bayer8x8 : null));
            SaveImage(testName, bmp);
        }

        [TestCase("32 bit ARGB", PixelFormat.Format32bppArgb, false)]
        [TestCase("8 bit by palette", PixelFormat.Format8bppIndexed, false)]
        [TestCase("32 bit by pixels using dithering", PixelFormat.Format8bppIndexed, true)]
        public void InvertTest(string testName, PixelFormat pixelFormat, bool useDithering)
        {
            using var bmp = Icons.Information.ExtractBitmap(new Size(256, 256)).ConvertPixelFormat(pixelFormat);
            Assert.DoesNotThrow(() => bmp.Invert(useDithering ? OrderedDitherer.Bayer8x8 : null));
            SaveImage(testName, bmp);
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
        [TestCase("Lighten 100% 32 bit by pixels using dithering", PixelFormat.Format8bppIndexed, true, 1f)]
        [TestCase("Lighten 50% 32 bit ARGB", PixelFormat.Format32bppArgb, false, 0.5f)]
        [TestCase("Lighten 50% 8 bit by palette", PixelFormat.Format8bppIndexed, false, 0.5f)]
        [TestCase("Lighten 50% 32 bit by pixels using dithering", PixelFormat.Format8bppIndexed, true, 0.5f)]
        [TestCase("Darken 100% 32 bit ARGB", PixelFormat.Format32bppArgb, false, -1f)]
        [TestCase("Darken 100% 8 bit by palette", PixelFormat.Format8bppIndexed, false, -1f)]
        [TestCase("Darken 100% 32 bit by pixels using dithering", PixelFormat.Format8bppIndexed, true, -1f)]
        [TestCase("Darken 50% 32 bit ARGB", PixelFormat.Format32bppArgb, false, -0.5f)]
        [TestCase("Darken 50% 8 bit by palette", PixelFormat.Format8bppIndexed, false, -0.5f)]
        [TestCase("Darken 50% 32 bit by pixels using dithering", PixelFormat.Format8bppIndexed, true, -0.5f)]
        public void AdjustBrightnessTest(string testName, PixelFormat pixelFormat, bool useDithering, float brightness)
        {
            using var bmp = Icons.Information.ExtractBitmap(new Size(256, 256)).ConvertPixelFormat(pixelFormat);
            Assert.DoesNotThrow(() => bmp.AdjustBrightness(brightness, useDithering ? OrderedDitherer.Bayer8x8 : null));
            SaveImage(testName, bmp);
        }

        [TestCase("Increase 100% 32 bit ARGB", PixelFormat.Format32bppArgb, false, 1f)]
        [TestCase("Increase 100% 8 bit by palette", PixelFormat.Format8bppIndexed, false, 1f)]
        [TestCase("Increase 100% 32 bit by pixels using dithering", PixelFormat.Format8bppIndexed, true, 1f)]
        [TestCase("Increase 50% 32 bit ARGB", PixelFormat.Format32bppArgb, false, 0.5f)]
        [TestCase("Increase 50% 8 bit by palette", PixelFormat.Format8bppIndexed, false, 0.5f)]
        [TestCase("Increase 50% 32 bit by pixels using dithering", PixelFormat.Format8bppIndexed, true, 0.5f)]
        [TestCase("Decrease 100% 32 bit ARGB", PixelFormat.Format32bppArgb, false, -1f)]
        [TestCase("Decrease 100% 8 bit by palette", PixelFormat.Format8bppIndexed, false, -1f)]
        [TestCase("Decrease 100% 32 bit by pixels using dithering", PixelFormat.Format8bppIndexed, true, -1f)]
        [TestCase("Decrease 50% 32 bit ARGB", PixelFormat.Format32bppArgb, false, -0.5f)]
        [TestCase("Decrease 50% 8 bit by palette", PixelFormat.Format8bppIndexed, false, -0.5f)]
        [TestCase("Decrease 50% 32 bit by pixels using dithering", PixelFormat.Format8bppIndexed, true, -0.5f)]
        public void AdjustContrastTest(string testName, PixelFormat pixelFormat, bool useDithering, float contrast)
        {
            using var bmp = Icons.Information.ExtractBitmap(new Size(256, 256)).ConvertPixelFormat(pixelFormat);
            Assert.DoesNotThrow(() => bmp.AdjustContrast(contrast, useDithering ? OrderedDitherer.Bayer8x8 : null));
            SaveImage(testName, bmp);
        }

        [TestCase("10 32 bit ARGB", PixelFormat.Format32bppArgb, false, 10f)]
        [TestCase("10 8 bit by palette", PixelFormat.Format8bppIndexed, false, 10f)]
        [TestCase("10 32 bit by pixels using dithering", PixelFormat.Format8bppIndexed, true, 10f)]
        [TestCase("2.5 32 bit ARGB", PixelFormat.Format32bppArgb, false, 2.5f)]
        [TestCase("2.5 8 bit by palette", PixelFormat.Format8bppIndexed, false, 2.5f)]
        [TestCase("2.5 32 bit by pixels using dithering", PixelFormat.Format8bppIndexed, true, 2.5f)]
        [TestCase("0 32 bit ARGB", PixelFormat.Format32bppArgb, false, 0f)]
        [TestCase("0 8 bit by palette", PixelFormat.Format8bppIndexed, false, 0f)]
        [TestCase("0 32 bit by pixels using dithering", PixelFormat.Format8bppIndexed, true, 0f)]
        [TestCase("0.25 32 bit ARGB", PixelFormat.Format32bppArgb, false, 0.25f)]
        [TestCase("0.25 8 bit by palette", PixelFormat.Format8bppIndexed, false, 0.25f)]
        [TestCase("0.25 32 bit by pixels using dithering", PixelFormat.Format8bppIndexed, true, 0.25f)]
        public void AdjustGammaTest(string testName, PixelFormat pixelFormat, bool useDithering, float gamma)
        {
            using var bmp = Icons.Information.ExtractBitmap(new Size(256, 256)).ConvertPixelFormat(pixelFormat);
            Assert.DoesNotThrow(() => bmp.AdjustGamma(gamma, useDithering ? OrderedDitherer.Bayer8x8 : null));
            SaveImage(testName, bmp);
        }

        #endregion
    }
}
