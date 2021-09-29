#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ImageExtensionsPerformanceTest.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2021 - All Rights Reserved
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

using KGySoft.Drawing.Imaging;

using NUnit.Framework;

#endregion

namespace KGySoft.Drawing.PerformanceTests
{
    [TestFixture]
    public class ImageExtensionsPerformanceTest
    {
        #region Methods

        [Test]
        public void ToGrayscaleTest()
        {
            using Bitmap bmp = Icons.Information.ExtractBitmap(new Size(256, 256));

            new PerformanceTest { Iterations = 100 }
                .AddCase(() =>
                {
                    using var result = new Bitmap(bmp.Width, bmp.Height);
                    using (Graphics g = Graphics.FromImage(result))
                    {
                        // Grayscale color matrix
                        var colorMatrix = new ColorMatrix(new float[][]
                        {
                            new float[] { ColorExtensions.RLum, ColorExtensions.RLum, ColorExtensions.RLum, 0, 0 },
                            new float[] { ColorExtensions.GLum, ColorExtensions.GLum, ColorExtensions.GLum, 0, 0 },
                            new float[] { ColorExtensions.BLum, ColorExtensions.BLum, ColorExtensions.BLum, 0, 0 },
                            new float[] { 0, 0, 0, 1, 0 },
                            new float[] { 0, 0, 0, 0, 1 }
                        });

                        using (var attrs = new ImageAttributes())
                        {
                            attrs.SetColorMatrix(colorMatrix);
                            g.DrawImage(bmp, new Rectangle(0, 0, result.Width, result.Height), 0, 0, result.Width, result.Height, GraphicsUnit.Pixel, attrs);
                        }
                    }
                }, "Graphics.DrawImage(..., ImageAttributes)")
                .AddCase(() =>
                {
                    using var result = new Bitmap(bmp.Width, bmp.Height);
                    using IReadableBitmapData src = bmp.GetReadableBitmapData();
                    using IWritableBitmapData dst = result.GetWritableBitmapData();
                    IReadableBitmapDataRow rowSrc = src.FirstRow;
                    IWritableBitmapDataRow rowDst = dst.FirstRow;
                    do
                    {
                        for (int x = 0; x < src.Width; x++)
                            rowDst[x] = rowSrc[x].ToGray();
                    } while (rowSrc.MoveNextRow() && rowDst.MoveNextRow());
                }, "Sequential processing")
                .AddCase(() =>
                {
                    using var result = bmp.ToGrayscale();
                }, "ImageExtensions.ToGrayscale")
                .DoTest()
                .DumpResults(Console.Out);
        }


        [TestCase(PixelFormat.Format32bppArgb)]
        [TestCase(PixelFormat.Format32bppPArgb)]
        [TestCase(PixelFormat.Format64bppArgb)]
        [TestCase(PixelFormat.Format16bppRgb565)]
        [TestCase(PixelFormat.Format16bppArgb1555)]
        [TestCase(PixelFormat.Format16bppGrayScale)]
        [TestCase(PixelFormat.Format8bppIndexed)]
        public void ConvertPixelFormatTest(PixelFormat newFormat)
        {
            using Bitmap bmp = Icons.Information.ExtractBitmap(new Size(256, 256));

            new PerformanceTest { TestName = $"{newFormat}", Iterations = 100, CpuAffinity = null }
                .AddCase(() =>
                {
                    using var result = new Bitmap(bmp.Width, bmp.Height, newFormat);
                    using (var g = Graphics.FromImage(result))
                        g.DrawImage(bmp, new Rectangle(Point.Empty, result.Size));
                }, "Graphics.DrawImage")
                .AddCase(() =>
                {
                    using var result = new Bitmap(bmp.Width, bmp.Height, newFormat);
                    using var src = bmp.GetReadableBitmapData();
                    using var dst = result.GetWritableBitmapData();
                    var rowSrc = src.FirstRow;
                    var rowDst = dst.FirstRow;
                    do
                    {
                        for (int x = 0; x < src.Width; x++)
                            rowDst[x] = rowSrc[x];
                    } while (rowSrc.MoveNextRow() && rowDst.MoveNextRow());
                }, "Sequential processing")
                .AddCase(() =>
                {
                    using var _ = bmp.ConvertPixelFormat(newFormat);
                }, "ImageExtensions.ConvertPixelFormat")
                .DoTest()
                .DumpResults(Console.Out);
        }

        [TestCase("32bpp ARGB to 32bpp ARGB", PixelFormat.Format32bppArgb, PixelFormat.Format32bppArgb)]
        [TestCase("32bpp PARGB to 32bpp PARGB", PixelFormat.Format32bppPArgb, PixelFormat.Format32bppPArgb)]
        [TestCase("32bpp ARGB to 32bpp PARGB", PixelFormat.Format32bppArgb, PixelFormat.Format32bppPArgb)]
        [TestCase("32bpp PARGB to 32bpp ARGB", PixelFormat.Format32bppPArgb, PixelFormat.Format32bppArgb)]
        [TestCase("64bpp PARGB to 64bpp PARGB", PixelFormat.Format64bppPArgb, PixelFormat.Format64bppPArgb)]
        public void DrawIntoTest(string testName, PixelFormat formatSrc, PixelFormat formatDst)
        {
            if (!formatSrc.IsSupportedNatively())
                Assert.Inconclusive($"Pixel format {formatSrc} is not supported on current platform");

            Size targetSize = new Size(300, 300);
            Size sourceSize = new Size(250, 250);
            Point offset = new Point(targetSize - sourceSize);

            // creating source images: alpha rectangles
            using var bmpSrc1 = new Bitmap(sourceSize.Width, sourceSize.Height, formatSrc);
            bmpSrc1.Clear(Color.FromArgb(128, Color.Red));
            using var bmpSrc2 = new Bitmap(sourceSize.Width, sourceSize.Height, formatSrc);
            bmpSrc2.Clear(Color.FromArgb(128, Color.Lime));

            new PerformanceTest { TestName = testName, Iterations = 100, CpuAffinity = null }
                .AddCase(() =>
                {
                    using var bmpDst = new Bitmap(targetSize.Width, targetSize.Height, formatDst);
                    using (var g = Graphics.FromImage(bmpDst))
                    {
                        g.DrawImage(bmpSrc1, Point.Empty);
                        g.DrawImage(bmpSrc2, offset);
                    }
                }, "Graphics.DrawImage")
                .AddCase(() =>
                {
                    using var bmpDst = new Bitmap(targetSize.Width, targetSize.Height, formatDst);
                    bmpSrc1.DrawInto(bmpDst, Point.Empty);
                    bmpSrc2.DrawInto(bmpDst, offset);
                }, "ImageExtensions.DrawInto")
                .DoTest()
                .DumpResults(Console.Out);
        }

        [TestCase("32bpp ARGB to 32bpp ARGB", PixelFormat.Format32bppArgb, PixelFormat.Format32bppArgb)]
        public void DrawIntoWithResizeTest(string testName, PixelFormat formatSrc, PixelFormat formatDst)
        {
            if (!formatSrc.IsSupportedNatively())
                Assert.Inconclusive($"Pixel format {formatSrc} is not supported on current platform");

            using var bmpSrc = Icons.Information.ExtractBitmap(new Size(256, 256)).ConvertPixelFormat(formatSrc);
            using var bmpDst = Icons.Information.ExtractBitmap(new Size(256, 256)).ConvertPixelFormat(formatDst);

            Rectangle targetRectangle = new Rectangle(Point.Empty, bmpDst.Size);
            targetRectangle.Inflate(-32, -32);

            new PerformanceTest { TestName = testName, Iterations = 100, CpuAffinity = null }
                .AddCase(() =>
                {
                    using var dst = bmpDst.CloneBitmap();
                    using (var g = Graphics.FromImage(dst))
                    {
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        g.DrawImage(bmpSrc, targetRectangle);
                    }
                }, "Graphics.DrawImage")
                .AddCase(() =>
                {
                    using var dst = bmpDst.CloneBitmap();
                    bmpSrc.DrawInto(dst, targetRectangle);
                }, "ImageExtensions.DrawInto")
                .DoTest()
                .DumpResults(Console.Out);
        }

        #endregion
    }
}
