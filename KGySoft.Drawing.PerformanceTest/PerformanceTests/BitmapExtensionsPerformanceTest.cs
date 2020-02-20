﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BitmapExtensionsPerformanceTest.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2020 - All Rights Reserved
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

            new ErrorTolerantPerformanceTest { TestName = $"{pixelFormat} {size}x{size}", Iterations = 10, CpuAffinity = null }
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
                    IWritableBitmapDataRow row = acc.FirstRow;
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
                .DumpResultsAndReturnValues(Console.Out);
        }

        [TestCase(PixelFormat.Format1bppIndexed, 0xFF333333, false)]
        [TestCase(PixelFormat.Format1bppIndexed, 0xFF333333, true)]
        [TestCase(PixelFormat.Format8bppIndexed, 0xFF333333, true)]
        [TestCase(PixelFormat.Format16bppRgb565, 0xFF333333, false)]
        public void ClearWithDitheringTest(PixelFormat pixelFormat, uint argb, bool errorDiffusion)
        {
            const int size = 512;
            Color color = Color.FromArgb((int)argb);
            var ditherer = errorDiffusion ? (IDitherer)ErrorDiffusionDitherer.FloydSteinberg() : OrderedDitherer.Bayer8x8();

            new PerformanceTest { TestName = $"{pixelFormat} {size}x{size} {(errorDiffusion ? "Error Diffusion" : "Ordered Dithering")}", Iterations = 10, CpuAffinity = null }
                .AddCase(() =>
                {
                    using var bmp = new Bitmap(size, size, pixelFormat);
                    using BitmapDataAccessorBase acc = BitmapDataAccessorFactory.CreateAccessor(bmp, ImageLockMode.ReadWrite);
                    IQuantizer quantizer = PredefinedColorsQuantizer.FromBitmapData(acc);
                    var c = new Color32(color);
                    using (IQuantizingSession quantizingSession = quantizer.Initialize(acc))
                    using (IDitheringSession ditheringSession = ditherer.Initialize(acc, quantizingSession))
                    {
                        IReadWriteBitmapDataRow row = acc.GetRow(0);
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
                    using var result = (Bitmap)bmpRef.Clone();
                    result.Quantize(quantizer);
                }, "BitmapExtensions.Quantize")
                .AddCase(() =>
                {
                    using var result = (Bitmap)bmpRef.Clone();
                    using (BitmapDataAccessorBase bitmapData = BitmapDataAccessorFactory.CreateAccessor(result, ImageLockMode.ReadWrite))
                    using (IQuantizingSession session = quantizer.Initialize(bitmapData))
                    {
                        var row = bitmapData.GetRow(0);
                        int width = bitmapData.Width;
                        do
                        {
                            for (int x = 0; x < width; x++)
                                row.DoSetColor32(x, session.GetQuantizedColor(row.DoGetColor32(x)));
                        } while (row.MoveNextRow());
                    }
                }, "Sequential quantization")
                .DoTest()
                .DumpResults(Console.Out);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void DitherPerformanceTest(bool errorDiffusion)
        {
            using var bmpRef = Icons.Information.ExtractBitmap(new Size(256, 256));
            IQuantizer quantizer = PredefinedColorsQuantizer.SystemDefault8BppPalette();
            IDitherer ditherer = errorDiffusion ? (IDitherer)ErrorDiffusionDitherer.FloydSteinberg() : OrderedDitherer.Bayer8x8();
            new PerformanceTest { TestName = $"{bmpRef.Width}x{bmpRef.Height}@{bmpRef.GetColorCount()} {(errorDiffusion ? "Error Diffusion" : "Ordered")}", Iterations = 100, CpuAffinity = null }
                .AddCase(() =>
                {
                    using var result = (Bitmap)bmpRef.Clone();
                    result.Dither(quantizer, ditherer);
                }, "BitmapExtensions.Dither")
                .AddCase(() =>
                {
                    using var result = (Bitmap)bmpRef.Clone();
                    using (BitmapDataAccessorBase bitmapData = BitmapDataAccessorFactory.CreateAccessor(result, ImageLockMode.ReadWrite))
                    using (IQuantizingSession quantizingSession = quantizer.Initialize(bitmapData))
                    using (IDitheringSession ditheringSession = ditherer.Initialize(bitmapData, quantizingSession))
                    {
                        var row = bitmapData.GetRow(0);
                        int width = bitmapData.Width;
                        do
                        {
                            for (int x = 0; x < width; x++)
                                row.DoSetColor32(x, ditheringSession.GetDitheredColor(row.DoGetColor32(x), x, row.RowIndex));
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
                    using var result = (Bitmap)bmp.Clone();
                    result.MakeTransparent(Color.Black);
                }, "Bitmap.MakeTransparent")
                .AddCase(() =>
                {
                    using var result = (Bitmap)bmp.Clone();
                    result.ReplaceColor(Color.Black, Color.Transparent);
                }, "BitmapExtensions.ReplaceColor")
                .AddCase(() =>
                {
                    using var result = (Bitmap)bmp.Clone();
                    using (BitmapDataAccessorBase bitmapData = BitmapDataAccessorFactory.CreateAccessor(result, ImageLockMode.ReadWrite))
                    {
                        Color32 from = new Color32(Color.Black);
                        Color32 to = new Color32(Color.Transparent);
                        BitmapDataRowBase row = bitmapData.GetRow(0);
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
                    using var result = (Bitmap)bmp.Clone();
                    result.AdjustBrightness(brightness);
                }, "BitmapExtensions.AdjustBrightness")
                .AddCase(() =>
                {
                    using var result = (Bitmap)bmp.Clone();
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
                    using var result = (Bitmap)bmp.Clone();
                    result.AdjustContrast(contrast);
                }, "BitmapExtensions.AdjustContrast")
                .AddCase(() =>
                {
                    using var result = (Bitmap)bmp.Clone();
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
                    using var result = (Bitmap)bmp.Clone();
                    result.AdjustGamma(gamma);
                }, "BitmapExtensions.AdjustGamma")
                .AddCase(() =>
                {
                    using var result = (Bitmap)bmp.Clone();
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

        #endregion
    }
}
