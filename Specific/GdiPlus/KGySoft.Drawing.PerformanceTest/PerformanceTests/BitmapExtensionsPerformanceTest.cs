#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BitmapExtensionsPerformanceTest.cs
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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using KGySoft.CoreLibraries;
using KGySoft.Drawing.Imaging;

using NUnit.Framework;

#endregion

namespace KGySoft.Drawing.PerformanceTests
{
    [TestFixture]
    public class BitmapExtensionsPerformanceTest
    {
        #region Methods

        [TestCase(PixelFormat.Format32bppArgb, 0xFF0000FF)]
        [TestCase(PixelFormat.Format32bppPArgb, 0xFF0000FF)]
        [TestCase(PixelFormat.Format16bppRgb555, 0xFF0000FF)]
        [TestCase(PixelFormat.Format64bppArgb, 0xFF0000FF)]
        [TestCase(PixelFormat.Format8bppIndexed, 0xFF0000FF)]
        [TestCase(PixelFormat.Format4bppIndexed, 0xFF0000FF)]
        [TestCase(PixelFormat.Format1bppIndexed, 0xFFFFFFFF)]
        public void ClearTest(PixelFormat pixelFormat, uint argb)
        {
            const int size = 512;
            Color color = Color.FromArgb((int)argb);

            new PerformanceTest { TestName = $"{pixelFormat} {size}x{size}", Iterations = 10, CpuAffinity = null }
                .AddCase(() =>
                {
                    using var bmp = new Bitmap(size, size, pixelFormat);
                    using var g = Graphics.FromImage(bmp);
                    g.Clear(color);
                }, "Graphics.Clear")
                .AddCase(() =>
                {
                    using var bmp = new Bitmap(size, size, pixelFormat);
                    using IWritableBitmapData acc = bmp.GetWritableBitmapData();
                    var c = new Color32(color);
                    IWritableBitmapDataRowMovable row = acc.FirstRow;
                    do
                    {
                        for (int x = 0; x < acc.Width; x++)
                            row[x] = c;
                    } while (row.MoveNextRow());
                }, "Sequential clear")
                .AddCase(() =>
                {
                    using var bmp = new Bitmap(size, size, pixelFormat);
                    bmp.Clear(color);
                }, "BitmapDataAccessor.Clear")
                .DoTest()
                .DumpResults(Console.Out);

            // V9.2.0
            // == [Format32bppArgb 512x512(.NET Framework Runtime v4.0.30319) Results] ================================================
            // 1. Graphics.Clear: 5 254 iterations in 2 000,42 ms. Adjusted for 2 000 ms: 5 252,88
            // 2. BitmapDataAccessor.Clear: 4 944 iterations in 2 000,19 ms. Adjusted for 2 000 ms: 4 943,52 (-309,37 / 94,11%)
            // 3. Sequential clear: 1 527 iterations in 2 000,93 ms. Adjusted for 2 000 ms: 1 526,29 (-3 726,59 / 29,06%)

            // ==[Format64bppArgb 512x512 (.NET Framework Runtime v4.0.30319) Results]================================================
            // 1. BitmapDataAccessor.Clear: 1 713 iterations in 2 000,02 ms. Adjusted for 2 000 ms: 1 712,98
            // 2. Graphics.Clear: 855 iterations in 2 000,55 ms. Adjusted for 2 000 ms: 854,77 (-858,22 / 49,90%)
            // 3. Sequential clear: 584 iterations in 2 001,54 ms. Adjusted for 2 000 ms: 583,55 (-1 129,43 / 34,07%)

            // ==[Format32bppArgb 512x512 (.NET Core 9.0.10) Results]================================================
            // 1. BitmapDataAccessor.Clear: 6 740 iterations in 2 000,02 ms. Adjusted for 2 000 ms: 6 739,95
            // 2. Sequential clear: 5 492 iterations in 2 000,30 ms. Adjusted for 2 000 ms: 5 491,18 (-1 248,77 / 81,47%)
            // 3. Graphics.Clear: 5 323 iterations in 2 000,18 ms. Adjusted for 2 000 ms: 5 322,52 (-1 417,43 / 78,97%)

            // ==[Format64bppArgb 512x512 (.NET Core 9.0.10) Results]================================================
            // 1. BitmapDataAccessor.Clear: 1 735 iterations in 2 000,62 ms. Adjusted for 2 000 ms: 1 734,46
            // 2. Graphics.Clear: 861 iterations in 2 000,87 ms. Adjusted for 2 000 ms: 860,63 (-873,84 / 49,62%)
            // 3. Sequential clear: 629 iterations in 2 003,12 ms. Adjusted for 2 000 ms: 628,02 (-1 106,45 / 36,21%)
        }

        [TestCase(PixelFormat.Format1bppIndexed, 0xFF333333, false)]
        [TestCase(PixelFormat.Format1bppIndexed, 0xFF333333, true)]
        [TestCase(PixelFormat.Format8bppIndexed, 0xFF333333, true)]
        [TestCase(PixelFormat.Format16bppRgb565, 0xFF333333, false)]
        public void ClearWithDitheringTest(PixelFormat pixelFormat, uint argb, bool errorDiffusion)
        {
            const int size = 512;
            Color color = Color.FromArgb((int)argb);
            var ditherer = errorDiffusion ? (IDitherer)ErrorDiffusionDitherer.FloydSteinberg : OrderedDitherer.Bayer8x8;

            new PerformanceTest { TestName = $"{pixelFormat} {size}x{size} {(errorDiffusion ? "Error Diffusion" : "Ordered Dithering")}", Iterations = 10, CpuAffinity = null }
                .AddCase(() =>
                {
                    using var bmp = new Bitmap(size, size, pixelFormat);
                    using IReadWriteBitmapData acc = bmp.GetReadWriteBitmapData();
                    IQuantizer quantizer = PredefinedColorsQuantizer.FromBitmapData(acc);
                    var c = new Color32(color);
                    using (IQuantizingSession quantizingSession = quantizer.Initialize(acc))
                    using (IDitheringSession ditheringSession = ditherer.Initialize(acc, quantizingSession))
                    {
                        IReadWriteBitmapDataRowMovable row = acc.FirstRow;
                        do
                        {
                            for (int x = 0; x < acc.Width; x++)
                                row[x] = ditheringSession.GetDitheredColor(c, x, row.Index);
                        } while (row.MoveNextRow());
                    }
                }, "Sequential clear")
                .AddCase(() =>
                {
                    using var bmp = new Bitmap(size, size, pixelFormat);
                    bmp.Clear(color, ditherer);
                }, "BitmapDataAccessor.Clear")
                .DoTest()
                .DumpResults(Console.Out);
        }

        [Test]
        public void QuantizePerformanceTest()
        {
            //using var bmpRef = new Bitmap(@"D:\Letolt\MYSTY8RQER62.jpg");
            using var bmpRef = Icons.Information.ExtractBitmap(new Size(256, 256));
            IQuantizer quantizer = PredefinedColorsQuantizer.SystemDefault8BppPalette();
            new PerformanceTest { TestName = $"{bmpRef.Width}x{bmpRef.Height}@{bmpRef.GetColorCount()}", Iterations = 100, CpuAffinity = null }
                .AddCase(() =>
                {
                    using var result = bmpRef.CloneBitmap();
                    result.Quantize(quantizer);
                }, "BitmapExtensions.Quantize")
                .AddCase(() =>
                {
                    using var result = bmpRef.CloneBitmap();
                    using (IReadWriteBitmapData bitmapData = result.GetReadWriteBitmapData())
                    using (IQuantizingSession session = quantizer.Initialize(bitmapData))
                    {
                        var row = bitmapData.FirstRow;
                        int width = bitmapData.Width;
                        do
                        {
                            for (int x = 0; x < width; x++)
                                row[x] = session.GetQuantizedColor(row[x]);
                        } while (row.MoveNextRow());
                    }
                }, "Sequential quantization")
                .DoTest()
                .DumpResults(Console.Out);
        }

        [Test]
        public void QuantizersPerformanceTest()
        {
            using var bmpRef = Icons.Information.ExtractBitmap(new Size(256, 256));
            new PerformanceTest { TestName = $"{bmpRef.Width}x{bmpRef.Height}@{bmpRef.GetColorCount()}", Iterations = 25, CpuAffinity = null }
                .AddCase(() =>
                {
                    using var result = bmpRef.CloneBitmap();
                    result.Quantize(PredefinedColorsQuantizer.SystemDefault8BppPalette());
                }, nameof(PredefinedColorsQuantizer.SystemDefault8BppPalette))
                .AddCase(() =>
                {
                    using var result = bmpRef.CloneBitmap();
                    result.Quantize(OptimizedPaletteQuantizer.Octree());
                }, nameof(OptimizedPaletteQuantizer.Octree))
                .AddCase(() =>
                {
                    using var result = bmpRef.CloneBitmap();
                    result.Quantize(OptimizedPaletteQuantizer.MedianCut());
                }, nameof(OptimizedPaletteQuantizer.MedianCut))
                .AddCase(() =>
                {
                    using var result = bmpRef.CloneBitmap();
                    result.Quantize(OptimizedPaletteQuantizer.Wu());
                }, nameof(OptimizedPaletteQuantizer.Wu))
                .DoTest()
                .DumpResults(Console.Out);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void DitherPerformanceTest(bool errorDiffusion)
        {
            using var bmpRef = Icons.Information.ExtractBitmap(new Size(256, 256));
            IQuantizer quantizer = PredefinedColorsQuantizer.SystemDefault8BppPalette();
            IDitherer ditherer = errorDiffusion ? (IDitherer)ErrorDiffusionDitherer.FloydSteinberg : OrderedDitherer.Bayer8x8;
            new PerformanceTest { TestName = $"{bmpRef.Width}x{bmpRef.Height}@{bmpRef.GetColorCount()} {(errorDiffusion ? "Error Diffusion" : "Ordered")}", Iterations = 100, CpuAffinity = null }
                .AddCase(() =>
                {
                    using var result = bmpRef.CloneBitmap();
                    result.Dither(quantizer, ditherer);
                }, "BitmapExtensions.Dither")
                .AddCase(() =>
                {
                    using var result = bmpRef.CloneBitmap();
                    using (IReadWriteBitmapData bitmapData = result.GetReadWriteBitmapData())
                    using (IQuantizingSession quantizingSession = quantizer.Initialize(bitmapData))
                    using (IDitheringSession ditheringSession = ditherer.Initialize(bitmapData, quantizingSession))
                    {
                        var row = bitmapData.FirstRow;
                        int width = bitmapData.Width;
                        do
                        {
                            for (int x = 0; x < width; x++)
                                row[x] = ditheringSession.GetDitheredColor(row[x], x, row.Index);
                        } while (row.MoveNextRow());
                    }
                }, "Sequential dithering")
                .DoTest()
                .DumpResults(Console.Out);
        }

        [Test]
        public void ReplaceColorTest()
        {
            using var bmp = Icons.Shield.ExtractBitmap(new Size(128, 128), true).Resize(new Size(256, 256));
            new PerformanceTest { Iterations = 100, CpuAffinity = null }
                .AddCase(() =>
                {
                    using var result = bmp.CloneBitmap();
                    result.MakeTransparent(Color.Black);
                }, "Bitmap.MakeTransparent")
                .AddCase(() =>
                {
                    using var result = bmp.CloneBitmap();
                    result.ReplaceColor(Color.Black, Color.Transparent);
                }, "BitmapExtensions.ReplaceColor")
                .AddCase(() =>
                {
                    using var result = bmp.CloneBitmap();
                    using (IReadWriteBitmapData bitmapData = result.GetReadWriteBitmapData())
                    {
                        Color32 from = new Color32(Color.Black);
                        Color32 to = new Color32(Color.Transparent);
                        IReadWriteBitmapDataRowMovable row = bitmapData.FirstRow;
                        do
                        {
                            for (int x = 0; x < bitmapData.Width; x++)
                            {
                                if (row[x] != from)
                                    continue;
                                row[x] = to;
                            }
                        } while (row.MoveNextRow());
                    }
                }, "Sequential processing")
                .DoTest()
                .DumpResults(Console.Out);
        }

        [Test]
        public void AdjustBrightnessTest()
        {
            const float brightness = +0.5f;
            using var bmp = Icons.Shield.ExtractBitmap(new Size(256, 256));
            new PerformanceTest { Iterations = 100, CpuAffinity = null }
                .AddCase(() =>
                {
                    using var result = bmp.CloneBitmap();
                    result.AdjustBrightness(brightness);
                }, "BitmapExtensions.AdjustBrightness")
                .AddCase(() =>
                {
                    using var result = bmp.CloneBitmap();
                    using (Graphics g = Graphics.FromImage(result))
                    {
                        var br = brightness + 1;

                        var colorMatrix = new ColorMatrix(new float[][]
                        {
                            new[] { 1f, 0, 0, 0, 0 },
                            new[] { 0, 1f, 0, 0, 0 },
                            new[] { 0, 0, 1f, 0, 0 },
                            new[] { 0, 0, 0, 1f, 0 },
                            new[] { br, br, br, 0, 1f }
                        });

                        using (var attrs = new ImageAttributes())
                        {
                            attrs.SetColorMatrix(colorMatrix);
                            g.DrawImage(result, new Rectangle(Point.Empty, result.Size), 0, 0, result.Width, result.Height, GraphicsUnit.Pixel, attrs);
                        }
                    }
                }, "Graphics.DrawImage(..., ImageAttributes)")
                .DoTest()
                .DumpResults(Console.Out);
        }

        [Test]
        public void AdjustContrastTest()
        {
            const float contrast = +0.5f;
            using var bmp = Icons.Shield.ExtractBitmap(new Size(256, 256));
            new PerformanceTest { Iterations = 100, CpuAffinity = null }
                .AddCase(() =>
                {
                    using var result = bmp.CloneBitmap();
                    result.AdjustContrast(contrast);
                }, "BitmapExtensions.AdjustContrast")
                .AddCase(() =>
                {
                    using var result = bmp.CloneBitmap();
                    using (Graphics g = Graphics.FromImage(result))
                    {
                        var c = contrast + 1;

                        var colorMatrix = new ColorMatrix(new float[][]
                        {
                            new[] { c, 0, 0, 0, 0 },
                            new[] { 0, c, 0, 0, 0 },
                            new[] { 0, 0, c, 0, 0 },
                            new[] { 0, 0, 0, 1f, 0 },
                            new[] { 0, 0, 0, 0, 1f }
                        });

                        using (var attrs = new ImageAttributes())
                        {
                            attrs.SetColorMatrix(colorMatrix);
                            g.DrawImage(result, new Rectangle(Point.Empty, result.Size), 0, 0, result.Width, result.Height, GraphicsUnit.Pixel, attrs);
                        }
                    }
                }, "Graphics.DrawImage(..., ImageAttributes)")
                .DoTest()
                .DumpResults(Console.Out);
        }

        [Test]
        public void AdjustGammaTest()
        {
            const float gamma = 1.6f;
            using var bmp = Icons.Shield.ExtractBitmap(new Size(256, 256));
            new PerformanceTest { Iterations = 100, CpuAffinity = null }
                .AddCase(() =>
                {
                    using var result = bmp.CloneBitmap();
                    result.AdjustGamma(gamma);
                }, "BitmapExtensions.AdjustGamma")
                .AddCase(() =>
                {
                    using var result = bmp.CloneBitmap();
                    using (Graphics g = Graphics.FromImage(result))
                    {
                        using (var attrs = new ImageAttributes())
                        {
                            attrs.SetGamma(gamma);
                            g.DrawImage(result, new Rectangle(Point.Empty, result.Size), 0, 0, result.Width, result.Height, GraphicsUnit.Pixel, attrs);
                        }
                    }
                }, "Graphics.DrawImage(..., ImageAttributes)")
                .DoTest()
                .DumpResults(Console.Out);
        }

        [TestCase(16, 16, 256, 256)]
        [TestCase(256, 256, 16, 16)]
        public void ResizeTest(int sw, int sh, int tw, int th)
        {
            Size sourceSize = new Size(sw, sh);
            Size targetSize = new Size(tw, th);
            using var bmpRef = Icons.Information.ExtractBitmap(sourceSize);

            var perfTest = new PerformanceTest { Iterations = 100, CpuAffinity = null, TestName = $"{sw}x{sh} to {tw}x{th}" };
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

            foreach (ScalingMode scalingMode in Enum<ScalingMode>.GetValues())
            {
                perfTest.AddCase(() =>
                {
                    using var result = bmpRef.Resize(targetSize, scalingMode);
                }, $"Resize/{scalingMode}");
            }

            perfTest.DoTest().DumpResults(Console.Out);

            /*
            // ==[16x16 to 256x256 (.NET Framework Runtime v4.0.30319) Results]================================================
            // Iterations: 100
            // Warming up: Yes
            // Test cases: 15
            // Calling GC.Collect: Yes
            // Forced CPU Affinity: No
            // Cases are sorted by time (quickest first)
            // --------------------------------------------------
            // 1. DrawImage/NearestNeighbor: average time: 1,11 ms
            // 2. DrawImage/Bilinear: average time: 1,96 ms (+0,85 ms / 176,47%)
            // 3. DrawImage/HighQualityBicubic: average time: 2,28 ms (+1,17 ms / 205,93%)
            // 4. Resize/NoScaling: average time: 8,01 ms (+6,90 ms / 722,36%)
            // 5. Resize/NearestNeighbor: average time: 26,86 ms (+25,75 ms / 2 422,59%)
            // 6. Resize/Box: average time: 54,80 ms (+53,69 ms / 4 942,73%)
            // 7. Resize/Bilinear: average time: 55,39 ms (+54,29 ms / 4 996,33%)
            // 8. Resize/Auto: average time: 55,57 ms (+54,46 ms / 5 012,15%)
            // 9. Resize/CatmullRom: average time: 56,92 ms (+55,81 ms / 5 133,64%)
            // 10. Resize/Robidoux: average time: 57,02 ms (+55,91 ms / 5 143,11%)
            // 11. Resize/MitchellNetravali: average time: 58,82 ms (+57,71 ms / 5 305,64%)
            // 12. Resize/Bicubic: average time: 59,03 ms (+57,92 ms / 5 324,22%)
            // 13. Resize/Spline: average time: 62,32 ms (+61,21 ms / 5 621,10%)
            // 14. Resize/Lanczos2: average time: 69,06 ms (+67,95 ms / 6 229,12%)
            // 15. Resize/Lanczos3: average time: 78,88 ms (+77,77 ms / 7 114,21%)           

            // ==[16x16 to 256x256 (.NET Core 10.0.0-rc.1.25451.107) Results]================================================
            // Iterations: 100
            // Warming up: Yes
            // Test cases: 15
            // Calling GC.Collect: Yes
            // Forced CPU Affinity: No
            // Cases are sorted by time (quickest first)
            // --------------------------------------------------
            // 1. DrawImage/NearestNeighbor: average time: 1,05 ms
            // 2. DrawImage/Bilinear: average time: 2,04 ms (+0,99 ms / 194,32%)
            // 3. DrawImage/HighQualityBicubic: average time: 2,47 ms (+1,42 ms / 235,15%)
            // 4. Resize/NoScaling: average time: 10,80 ms (+9,75 ms / 1 027,89%)
            // 5. Resize/NearestNeighbor: average time: 21,56 ms (+20,51 ms / 2 052,03%)
            // 6. Resize/Bilinear: average time: 24,02 ms (+22,97 ms / 2 286,48%)
            // 7. Resize/MitchellNetravali: average time: 24,58 ms (+23,53 ms / 2 339,35%)
            // 8. Resize/Lanczos2: average time: 25,18 ms (+24,13 ms / 2 396,85%)
            // 9. Resize/Box: average time: 25,23 ms (+24,18 ms / 2 401,67%)
            // 10. Resize/Spline: average time: 25,49 ms (+24,43 ms / 2 425,54%)
            // 11. Resize/Robidoux: average time: 25,88 ms (+24,83 ms / 2 463,36%)
            // 12. Resize/Bicubic: average time: 26,63 ms (+25,58 ms / 2 534,39%)
            // 13. Resize/CatmullRom: average time: 26,96 ms (+25,91 ms / 2 566,38%)
            // 14. Resize/Lanczos3: average time: 28,41 ms (+27,36 ms / 2 704,04%)
            // 15. Resize/Auto: average time: 31,56 ms (+30,51 ms / 3 003,39%)

            // ==[256x256 to 16x16 (.NET Framework Runtime v4.0.30319) Results]================================================
            // Iterations: 100
            // Warming up: Yes
            // Test cases: 15
            // Calling GC.Collect: Yes
            // Forced CPU Affinity: No
            // Cases are sorted by time (quickest first)
            // --------------------------------------------------
            // 1. Resize/NoScaling: average time: 1,54 ms
            // 2. Resize/NearestNeighbor: average time: 1,75 ms (+0,21 ms / 113,60%)
            // 3. DrawImage/Bilinear: average time: 17,12 ms (+15,58 ms / 1 114,26%)
            // 4. DrawImage/NearestNeighbor: average time: 17,16 ms (+15,63 ms / 1 117,03%)
            // 5. Resize/Bilinear: average time: 34,62 ms (+33,09 ms / 2 253,48%)
            // 6. Resize/Box: average time: 36,52 ms (+34,98 ms / 2 376,72%)
            // 7. Resize/Auto: average time: 44,08 ms (+42,55 ms / 2 869,20%)
            // 8. Resize/Robidoux: average time: 44,40 ms (+42,86 ms / 2 889,95%)
            // 9. Resize/CatmullRom: average time: 44,58 ms (+43,05 ms / 2 901,80%)
            // 10. Resize/Bicubic: average time: 45,24 ms (+43,70 ms / 2 944,40%)
            // 11. Resize/Spline: average time: 46,51 ms (+44,97 ms / 3 027,23%)
            // 12. Resize/MitchellNetravali: average time: 47,02 ms (+45,49 ms / 3 060,53%)
            // 13. Resize/Lanczos2: average time: 56,48 ms (+54,94 ms / 3 676,06%)
            // 14. DrawImage/HighQualityBicubic: average time: 58,55 ms (+57,01 ms / 3 810,75%)
            // 15. Resize/Lanczos3: average time: 65,77 ms (+64,23 ms / 4 280,53%)

            // ==[256x256 to 16x16 (.NET Core 10.0.0-rc.1.25451.107) Results]================================================
            // Iterations: 100
            // Warming up: Yes
            // Test cases: 15
            // Calling GC.Collect: Yes
            // Forced CPU Affinity: No
            // Cases are sorted by time (quickest first)
            // --------------------------------------------------
            // 1. Resize/NearestNeighbor: average time: 0,73 ms
            // 2. Resize/NoScaling: average time: 0,97 ms (+0,24 ms / 133,17%)
            // 3. Resize/Box: average time: 11,43 ms (+10,69 ms / 1 561,45%)
            // 4. Resize/Bilinear: average time: 12,91 ms (+12,18 ms / 1 764,37%)
            // 5. Resize/Robidoux: average time: 15,19 ms (+14,46 ms / 2 075,89%)
            // 6. Resize/Bicubic: average time: 15,30 ms (+14,57 ms / 2 090,91%)
            // 7. Resize/Auto: average time: 15,33 ms (+14,60 ms / 2 095,07%)
            // 8. Resize/CatmullRom: average time: 15,55 ms (+14,82 ms / 2 124,89%)
            // 9. Resize/MitchellNetravali: average time: 15,69 ms (+14,96 ms / 2 144,39%)
            // 10. Resize/Spline: average time: 16,00 ms (+15,27 ms / 2 187,08%)
            // 11. DrawImage/NearestNeighbor: average time: 17,09 ms (+16,36 ms / 2 335,33%)
            // 12. DrawImage/Bilinear: average time: 18,17 ms (+17,44 ms / 2 483,38%)
            // 13. Resize/Lanczos2: average time: 19,66 ms (+18,92 ms / 2 686,32%)
            // 14. Resize/Lanczos3: average time: 19,88 ms (+19,15 ms / 2 716,86%)
            // 15. DrawImage/HighQualityBicubic: average time: 60,47 ms (+59,74 ms / 8 264,58%)
             */
        }

        #endregion
    }
}
