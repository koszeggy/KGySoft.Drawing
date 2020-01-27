#region Copyright

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
            var ditherer = errorDiffusion ? (IDitherer)ErrorDiffusionDitherer.FloydSteinberg : OrderedDitherer.Bayer8x8();

            new PerformanceTest { TestName = $"{pixelFormat} {size}x{size} {(errorDiffusion ? "Error Diffusion" : "Ordered Dithering")}", Iterations = 10, CpuAffinity = null }
                .AddCase(() =>
                {
                    using var bmp = new Bitmap(size, size, pixelFormat);
                    IQuantizer quantizer = PredefinedColorsQuantizer.FromBitmap(bmp);
                    var c = new Color32(color);
                    using (IReadWriteBitmapData acc = bmp.GetReadWriteBitmapData())
                    using (IQuantizingSession quantizingSession = quantizer.Initialize(acc))
                    using (IDitheringSession ditheringSession = ditherer.Initialize(acc, quantizingSession))
                    {
                        IReadWriteBitmapDataRow row = acc.FirstRow;
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


        #endregion
    }
}
