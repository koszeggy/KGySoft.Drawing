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
using System.IO;
using System.Linq;
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

        private static object[][] quantizeTestSource =
        {
            //new object[] { "RGB888 Black", PredefinedColorsQuantizer.Rgb888(), 1 << 24 },
            //new object[] { "RGB888 White", PredefinedColorsQuantizer.Rgb888(Color.White), 1 << 24 },
            //new object[] { "RGB565 Black", PredefinedColorsQuantizer.Rgb565(), 1 << 16 },
            //new object[] { "RGB565 White", PredefinedColorsQuantizer.Rgb565(Color.White), 1 << 16 },
            //new object[] { "RGB555 Black", PredefinedColorsQuantizer.Rgb555(), 1 << 15 },
            //new object[] { "RGB555 White", PredefinedColorsQuantizer.Rgb555(Color.White), 1 << 15 },
            new object[] { "ARGB1555 Black 50%", PredefinedColorsQuantizer.Argb1555(), (1 << 15) + 1 },
            //new object[] { "ARGB1555 White 50%", PredefinedColorsQuantizer.Argb1555(Color.White), (1 << 15) + 1 },
            //new object[] { "ARGB1555 Black 0", PredefinedColorsQuantizer.Argb1555(default, 0), (1 << 15) + 1 },
            //new object[] { "ARGB1555 Black 1", PredefinedColorsQuantizer.Argb1555(default, 1), (1 << 15) + 1 },
            //new object[] { "ARGB1555 Black 254", PredefinedColorsQuantizer.Argb1555(default, 254), (1 << 15) + 1 },
            //new object[] { "RGB332 Black", PredefinedColorsQuantizer.Rgb332(), 256 },
            //new object[] { "RGB332 White", PredefinedColorsQuantizer.Rgb332(Color.White), 256 },
            //new object[] { "Grayscale Black", PredefinedColorsQuantizer.Grayscale(), 256 },
            //new object[] { "Grayscale White", PredefinedColorsQuantizer.Grayscale(Color.White), 256 },
            new object[] { "Grayscale16 Black", PredefinedColorsQuantizer.Grayscale16(), 16 },
            //new object[] { "Grayscale5 black", PredefinedColorsQuantizer.FromCustomPalette(new [] { Color.Black, Color.FromArgb(64, 64, 64), Color.Gray, Color.FromArgb(192, 192, 192), Color.White }), 5 },
            //new object[] { "Grayscale4 Black", PredefinedColorsQuantizer.Grayscale4(), 4 },
            //new object[] { "Grayscale3 Black", PredefinedColorsQuantizer.FromCustomPalette(new [] { Color.Black, Color.Gray, Color.White }), 3 },
            new object[] { "BW Black", PredefinedColorsQuantizer.BlackAndWhite(), 2 },
            //new object[] { "BW White", PredefinedColorsQuantizer.BlackAndWhite(Color.White), 2 },
            //new object[] { "BW Lime", PredefinedColorsQuantizer.BlackAndWhite(Color.Lime), 2 },
            //new object[] { "BW Blue", PredefinedColorsQuantizer.BlackAndWhite(Color.Blue), 2 },
            new object[] { "Default8Bpp Black", PredefinedColorsQuantizer.SystemDefault8BppPalette(), 256 },
            //new object[] { "Default8Bpp White", PredefinedColorsQuantizer.SystemDefault8BppPalette(Color.White), 256 },
            new object[] { "Default4Bpp Black", PredefinedColorsQuantizer.SystemDefault4BppPalette(), 16 },
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
            //new object[] { "Octree 256 White", OptimizedPaletteQuantizer.Octree(256, Color.White, 0), 256 },
            //new object[] { "Octree 16 White", OptimizedPaletteQuantizer.Octree(16, Color.White, 0), 16 },
            //new object[] { "Octree 4 White", OptimizedPaletteQuantizer.Octree(4, Color.White, 0), 4 },
            //new object[] { "Octree 3 White", OptimizedPaletteQuantizer.Octree(3, Color.White, 0), 3 },
            //new object[] { "Octree 2 White", OptimizedPaletteQuantizer.Octree(2, Color.White, 0), 2 },
            //new object[] { "Octree 256 TR", OptimizedPaletteQuantizer.Octree(256, Color.White), 256 },
            //new object[] { "Octree 16 TR", OptimizedPaletteQuantizer.Octree(16, Color.White), 16 },
            //new object[] { "Octree 4 TR", OptimizedPaletteQuantizer.Octree(4, Color.White), 4 },
            //new object[] { "Octree 3 TR", OptimizedPaletteQuantizer.Octree(3, Color.White), 3 },
            //new object[] { "Octree 2 TR", OptimizedPaletteQuantizer.Octree(2, Color.White), 2 },

            //new object[] { "MedianCut 256 Black", OptimizedPaletteQuantizer.MedianCut(256, Color.Black, 0), 256 },
            //new object[] { "MedianCut 16 Black", OptimizedPaletteQuantizer.MedianCut(16, Color.Black, 0), 16 },
            //new object[] { "MedianCut 4 Black", OptimizedPaletteQuantizer.MedianCut(4, Color.Black, 0), 4 },
            //new object[] { "MedianCut 3 Black", OptimizedPaletteQuantizer.MedianCut(3, Color.Black, 0), 3 },
            //new object[] { "MedianCut 2 Black", OptimizedPaletteQuantizer.MedianCut(2, Color.Black, 0), 2 },
            //new object[] { "MedianCut 256 White", OptimizedPaletteQuantizer.MedianCut(256, Color.White, 0), 256 },
            //new object[] { "MedianCut 16 White", OptimizedPaletteQuantizer.MedianCut(16, Color.White, 0), 16 },
            //new object[] { "MedianCut 4 White", OptimizedPaletteQuantizer.MedianCut(4, Color.White, 0), 4 },
            //new object[] { "MedianCut 3 White", OptimizedPaletteQuantizer.MedianCut(3, Color.White, 0), 3 },
            //new object[] { "MedianCut 2 White", OptimizedPaletteQuantizer.MedianCut(2, Color.White, 0), 2 },
            //new object[] { "MedianCut 256 TR", OptimizedPaletteQuantizer.MedianCut(256, Color.White), 256 },
            //new object[] { "MedianCut 16 TR", OptimizedPaletteQuantizer.MedianCut(16, Color.White), 16 },
            //new object[] { "MedianCut 4 TR", OptimizedPaletteQuantizer.MedianCut(4, Color.White), 4 },
            //new object[] { "MedianCut 3 TR", OptimizedPaletteQuantizer.MedianCut(3, Color.White), 3 },
            //new object[] { "MedianCut 2 TR", OptimizedPaletteQuantizer.MedianCut(2, Color.White), 2 },

            //new object[] { "Wu 256 Black", OptimizedPaletteQuantizer.MedianCut(256, Color.Black, 0), 256 },
            //new object[] { "Wu 16 Black", OptimizedPaletteQuantizer.MedianCut(16, Color.Black, 0), 16 },
            //new object[] { "Wu 4 Black", OptimizedPaletteQuantizer.MedianCut(4, Color.Black, 0), 4 },
            //new object[] { "Wu 3 Black", OptimizedPaletteQuantizer.MedianCut(3, Color.Black, 0), 3 },
            //new object[] { "Wu 2 Black", OptimizedPaletteQuantizer.MedianCut(2, Color.Black, 0), 2 },
            //new object[] { "Wu 256 White", OptimizedPaletteQuantizer.MedianCut(256, Color.White, 0), 256 },
            //new object[] { "Wu 16 White", OptimizedPaletteQuantizer.MedianCut(16, Color.White, 0), 16 },
            //new object[] { "Wu 4 White", OptimizedPaletteQuantizer.MedianCut(4, Color.White, 0), 4 },
            //new object[] { "Wu 3 White", OptimizedPaletteQuantizer.MedianCut(3, Color.White, 0), 3 },
            //new object[] { "Wu 2 White", OptimizedPaletteQuantizer.MedianCut(2, Color.White, 0), 2 },
            //new object[] { "Wu 256 TR", OptimizedPaletteQuantizer.MedianCut(256, Color.White), 256 },
            //new object[] { "Wu 16 TR", OptimizedPaletteQuantizer.MedianCut(16, Color.White), 16 },
            //new object[] { "Wu 4 TR", OptimizedPaletteQuantizer.MedianCut(4, Color.White), 4 },
            //new object[] { "Wu 3 TR", OptimizedPaletteQuantizer.MedianCut(3, Color.White), 3 },
            //new object[] { "Wu 2 TR", OptimizedPaletteQuantizer.MedianCut(2, Color.White), 2 },
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

        [TestCaseSource(nameof(quantizeTestSource)), Explicit]
        public void BatchQuantizeTest(string testName, IQuantizer quantizer, int maxColors)
        {
            var files = new string[]
            {
                @"D:\Dokumentumok\Képek\Formats\_test\Information.png",
                @"D:\Dokumentumok\Képek\Formats\_test\Shield.png",
                @"D:\Dokumentumok\Képek\Formats\_test\Hue_alpha_falloff.png",
                @"D:\Dokumentumok\Képek\Formats\_test\color_wheel.png",
                @"D:\Dokumentumok\Képek\Formats\_test\baboon.bmp",
                @"D:\Dokumentumok\Képek\Formats\_test\barbara.bmp",
                @"D:\Dokumentumok\Képek\Formats\_test\Quantum_frog.png",
                @"D:\Dokumentumok\Képek\Formats\_test\lena.png",
                @"D:\Dokumentumok\Képek\Formats\_test\Earth.bmp",
                @"D:\Dokumentumok\Képek\Formats\_test\pens.bmp",
                @"D:\Dokumentumok\Képek\Formats\_test\peppers.png",
                @"D:\Letolt\MYSTY8RQER62.jpg",
            };

            foreach (string file in files)
            {
                using var bitmap = new Bitmap(file);
                bitmap.Quantize(quantizer);
                int colors = bitmap.GetColors(forceScanningContent: true).Length;
                Console.WriteLine($"{testName} - {colors} colors");
                Assert.LessOrEqual(colors, maxColors);
                SaveImage($"{Path.GetFileNameWithoutExtension(file)} {maxColors} {testName}", bitmap);
            }
        }

        [Test, Explicit]
        public void QuantizerPerformanceTest()
        {
            using var ref32bpp = new Bitmap(@"D:\Dokumentumok\Képek\Formats\_test\Hue_alpha_falloff.png");
            new PerformanceTest { Iterations = 1 }
                .AddCase(() => ref32bpp.CloneCurrentFrame().Quantize(OptimizedPaletteQuantizer.Octree()))
                .AddCase(() => ref32bpp.CloneCurrentFrame().Quantize(OptimizedPaletteQuantizer.MedianCut()))
                .AddCase(() => ref32bpp.CloneCurrentFrame().Quantize(OptimizedPaletteQuantizer.MedianCut()))
                .DoTest()
                .DumpResults(Console.Out);
        }

        [TestCaseSource(nameof(quantizeTestSource)), Explicit]
        public void BatchDitherTest(string testName, IQuantizer quantizer, int maxColors)
        {
            string[] files =
            {
                //@"D:\Dokumentumok\Képek\Formats\_test\Information.png",
                //@"D:\Dokumentumok\Képek\Formats\_test\Shield.png",
                //@"D:\Dokumentumok\Képek\Formats\_test\Hue_alpha_falloff.png",
                //@"D:\Dokumentumok\Képek\Formats\_test\color_wheel.png",
                @"D:\Dokumentumok\Képek\Formats\_test\grayshades.png",

                ////@"D:\Dokumentumok\Képek\Formats\_test\baboon.bmp",
                //////@"D:\Dokumentumok\Képek\Formats\_test\barbara.bmp",
                //////@"D:\Dokumentumok\Képek\Formats\_test\Quantum_frog.png",
                @"D:\Dokumentumok\Képek\Formats\_test\lena.png",
                //////@"D:\Dokumentumok\Képek\Formats\_test\Earth.bmp",
                //////@"D:\Dokumentumok\Képek\Formats\_test\pens.bmp",
                //////@"D:\Dokumentumok\Képek\Formats\_test\peppers.png",
                //////@"D:\Letolt\MYSTY8RQER62.jpg",
                ////@"D:\Dokumentumok\Képek\Formats\_test\Portal_Companion_Cube.jpg",

                @"D:\Dokumentumok\Képek\Formats\_test\gradients.png",
                @"D:\Dokumentumok\Képek\Formats\_test\cameraman.png",
                ////@"D:\Dokumentumok\Képek\Formats\_test\clown.bmp",
                ////@"D:\Dokumentumok\Képek\Formats\_test\Michelangelo's_David.png",
            };

            (IDitherer Ditherer, string Name)[] ditherers =
            {
                //(null, " No Dithering"),
                //(OrderedDitherer.Bayer2x2(), nameof(OrderedDitherer.Bayer2x2)),
                //(OrderedDitherer.Bayer3x3(), nameof(OrderedDitherer.Bayer3x3)),
                //(OrderedDitherer.Bayer4x4(), nameof(OrderedDitherer.Bayer4x4)),
                //(OrderedDitherer.Bayer8x8(), nameof(OrderedDitherer.Bayer8x8)),
                //(OrderedDitherer.Halftone5(), nameof(OrderedDitherer.Halftone5)),
                //(OrderedDitherer.Halftone7(), nameof(OrderedDitherer.Halftone7)),
                //(OrderedDitherer.BlueNoise64x64(), nameof(OrderedDitherer.BlueNoise64x64)),
                //(OrderedDitherer.BlueNoise(new Size(256, 256)), nameof(OrderedDitherer.BlueNoise)),
                //(OrderedDitherer.Bayer2x2(), nameof(OrderedDitherer.Bayer2x2)),
                //(OrderedDitherer.Bayer3x3(), nameof(OrderedDitherer.Bayer3x3)),
                //(OrderedDitherer.Bayer4x4(), nameof(OrderedDitherer.Bayer4x4)),
                //(OrderedDitherer.Bayer8x8(), nameof(OrderedDitherer.Bayer8x8)),
                //(OrderedDitherer.Halftone5(), nameof(OrderedDitherer.Halftone5)),
                //(OrderedDitherer.Halftone6Hex(), nameof(OrderedDitherer.Halftone6Hex)),
                //(OrderedDitherer.Halftone6Diagonal(), nameof(OrderedDitherer.Halftone6Diagonal)),
                //(OrderedDitherer.Halftone7(), nameof(OrderedDitherer.Halftone7)),
                //(OrderedDitherer.Halftone8Hex(), nameof(OrderedDitherer.Halftone8Hex)),
                //(OrderedDitherer.Halftone8Diagonal(), nameof(OrderedDitherer.Halftone8Diagonal)),
                (OrderedDitherer.BlueNoise(), nameof(OrderedDitherer.BlueNoise)),
                //(OrderedDitherer.BlueNoise(new Size(256, 256)), nameof(OrderedDitherer.BlueNoise)),
                //(ErrorDiffusionDitherer.FloydSteinberg, nameof(ErrorDiffusionDitherer.FloydSteinberg)),
                //(ErrorDiffusionDitherer.JarvisJudiceNinke, nameof(ErrorDiffusionDitherer.JarvisJudiceNinke)),
                //(ErrorDiffusionDitherer.Stucki, nameof(ErrorDiffusionDitherer.Stucki)),
                //(ErrorDiffusionDitherer.Burkes, nameof(ErrorDiffusionDitherer.Burkes)),
                //(ErrorDiffusionDitherer.Sierra3, nameof(ErrorDiffusionDitherer.Sierra3)),
                //(ErrorDiffusionDitherer.Sierra2, nameof(ErrorDiffusionDitherer.Sierra2)),
                //(ErrorDiffusionDitherer.SierraLite, nameof(ErrorDiffusionDitherer.SierraLite)),
                //(ErrorDiffusionDitherer.StevensonArce, nameof(ErrorDiffusionDitherer.StevensonArce)),
                (new RandomNoiseDitherer(0, 0), nameof(RandomNoiseDitherer)),
                (new InterleavedGradientNoiseDitherer(0), nameof(InterleavedGradientNoiseDitherer)),
            };

            foreach (string file in files)
            {
                using var bitmap = new Bitmap(file);
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
        public void DithererPerformanceTest()
        {
            using var ref32bpp = new Bitmap(@"D:\Dokumentumok\Képek\Formats\_test\GrangerRainbow.png");
            new PerformanceTest { Iterations = 10 }
                .AddCase(() => ref32bpp.CloneCurrentFrame().Dither(PredefinedColorsQuantizer.BlackAndWhite(), ErrorDiffusionDitherer.FloydSteinberg), "NoClip, int")
                .AddCase(() => ref32bpp.CloneCurrentFrame().Dither(PredefinedColorsQuantizer.BlackAndWhite(), ErrorDiffusionDitherer.FloydSteinberg), "Clip")
                .AddCase(() => ref32bpp.CloneCurrentFrame().Dither(PredefinedColorsQuantizer.BlackAndWhite(), ErrorDiffusionDitherer.FloydSteinberg), "float")
                .DoTest()
                .DumpResults(Console.Out);
        }

        [Test, Explicit]
        public void GenerateBlueNoiseMatrix()
        {
            using Bitmap texture = new Bitmap(@"D:\Dokumentumok\Képek\Formats\BlueNoiseTextures\64\LDR_LLL1_13.png");
            using IBitmapDataAccessor accessor = texture.GetBitmapDataAccessor(ImageLockMode.ReadOnly);

            IBitmapDataRow row = accessor.FirstRow;
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
        [TestCase(PixelFormat.Format32bppPArgb, 0xFF0000FF)]
        [TestCase(PixelFormat.Format16bppRgb555, 0xFF0000FF)]
        [TestCase(PixelFormat.Format64bppArgb, 0xFF0000FF)]
        [TestCase(PixelFormat.Format8bppIndexed, 0xFF0000FF)]
        [TestCase(PixelFormat.Format4bppIndexed, 0xFF0000FF)]
        [TestCase(PixelFormat.Format1bppIndexed, 0xFFFFFFFF)]
        public void ClearTest(PixelFormat pixelFormat, uint argb)
        {
            const int size = 17;
            Color color = Color.FromArgb((int)argb);

            using var bmp = new Bitmap(size, size, pixelFormat);
            bmp.Clear(color);
            using (var bitmapData = bmp.GetBitmapDataAccessor(ImageLockMode.ReadOnly))
            {
                var row = bitmapData.FirstRow;
                var c32 = new Color32(color);
                do
                {
                    for (int x = 0; x < bitmapData.Width; x++)
                        Assert.AreEqual(c32, row[x]);
                } while (row.MoveNextRow());
            }

            SaveImage(pixelFormat.ToString(), bmp);
        }

        [TestCase(PixelFormat.Format1bppIndexed, 0xFF333333, false)]
        [TestCase(PixelFormat.Format1bppIndexed, 0xFF333333, true)]
        public void ClearWithDitheringTest(PixelFormat pixelFormat, uint argb, bool errorDiffusion)
        {
            const int size = 17;
            Color color = Color.FromArgb((int)argb);

            using var bmp = new Bitmap(size, size, pixelFormat);
            bmp.Clear(color, errorDiffusion ? (IDitherer)ErrorDiffusionDitherer.FloydSteinberg : OrderedDitherer.Bayer8x8());
            SaveImage($"{pixelFormat} {(errorDiffusion ? "Error diffusion" : "Ordered")}", bmp);
        }

        [Test]
        public void ChangeColorTest()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
