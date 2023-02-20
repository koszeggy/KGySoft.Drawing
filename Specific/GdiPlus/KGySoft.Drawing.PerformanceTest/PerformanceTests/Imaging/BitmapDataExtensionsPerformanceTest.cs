#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BitmapDataExtensionsPerformanceTest.cs
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

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

using KGySoft.Drawing.Imaging;

using NUnit.Framework;

#endregion

namespace KGySoft.Drawing.PerformanceTests.Imaging
{
    [TestFixture]
    public class BitmapDataExtensionsPerformanceTest
    {
        #region Methods


        [TestCase(PixelFormat.Format32bppArgb)]
        [TestCase(PixelFormat.Format32bppPArgb)]
        [TestCase(PixelFormat.Format64bppPArgb)]
        public void CopyToTest(PixelFormat pixelFormat)
        {
            if (!pixelFormat.IsSupportedNatively())
                Assert.Inconclusive($"Pixel format {pixelFormat} is not supported on current platform");

            Size size = new Size(256, 256);
            using var bmpSrc = Icons.Information.ExtractBitmap(size).ConvertPixelFormat(pixelFormat);

            new PerformanceTest { TestName = pixelFormat.ToString(), Iterations = 100, CpuAffinity = null }
                .AddCase(() =>
                {
                    using var bmpDst = new Bitmap(size.Width, size.Height, pixelFormat);
                    using (var g = Graphics.FromImage(bmpDst))
                    {
                        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        g.CompositingMode = CompositingMode.SourceCopy; // so it is the same as CopyTo (and is much faster)
                        g.DrawImage(bmpSrc, Point.Empty);
                    }
                }, "Graphics.DrawImage")
                .AddCase(() =>
                {
                    using var bmpDst = new Bitmap(size.Width, size.Height, pixelFormat);
                    using (var dataSrc = bmpSrc.GetReadableBitmapData())
                    using (var dataDst = bmpDst.GetWritableBitmapData())
                    {
                        dataSrc.CopyTo(dataDst, Point.Empty);
                    }
                }, "ImageExtensions.CopyTo (native to native)")
                .AddCase(() =>
                {
                    using var bitmapDataDst = BitmapDataFactory.CreateBitmapData(size, pixelFormat.ToKnownPixelFormatInternal());
                    using (var dataSrc = bmpSrc.GetReadableBitmapData())
                    {
                        dataSrc.CopyTo(bitmapDataDst, Point.Empty);
                    }
                }, "ImageExtensions.CopyTo (native to managed)")
                .DoTest()
                .DumpResults(Console.Out);
        }

        [TestCase(PixelFormat.Format1bppIndexed)]
        public void CopyToClippedTest(PixelFormat pixelFormat)
        {
            using IReadableBitmapData src = Icons.Information.ExtractBitmap(new Size(256, 256)).ConvertPixelFormat(pixelFormat).GetReadableBitmapData();
            Size targetSize = new Size(128, 128);
            using IReadWriteBitmapData dst = BitmapDataFactory.CreateBitmapData(targetSize, pixelFormat.ToKnownPixelFormatInternal());

            new PerformanceTest { CpuAffinity = null, Iterations = 10_000 }
                .AddCase(() => src.CopyTo(dst, new Rectangle(default, targetSize), Point.Empty), "CopyTo")
                .AddCase(() => src.Clip(new Rectangle(default, targetSize)).CopyTo(dst, Point.Empty), "Clip+CopyTo")
                .DoTest()
                .DumpResults(Console.Out);
        }

        [TestCase(PixelFormat.Format32bppArgb)]
        [TestCase(PixelFormat.Format32bppPArgb)]
        [TestCase(PixelFormat.Format64bppPArgb)]
        [TestCase(PixelFormat.Format24bppRgb)]
        [TestCase(PixelFormat.Format16bppRgb565)]
        public void DrawIntoNoResizeTest(PixelFormat pixelFormat)
        {
            if (!pixelFormat.IsSupportedNatively())
                Assert.Inconclusive($"Pixel format {pixelFormat} is not supported on current platform");

            Size size = new Size(256, 256);
            using var bmpSrc = Icons.Information.ExtractBitmap(size).ConvertPixelFormat(pixelFormat);

            new PerformanceTest { TestName = pixelFormat.ToString(), Iterations = 100, CpuAffinity = null }
                .AddCase(() =>
                {
                    using var bmpDst = new Bitmap(size.Width, size.Height, pixelFormat);
                    using (var g = Graphics.FromImage(bmpDst))
                    {
                        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        g.DrawImage(bmpSrc, Point.Empty);
                    }
                }, "Graphics.DrawImage")
                .AddCase(() =>
                {
                    using var bmpDst = new Bitmap(size.Width, size.Height, pixelFormat);
                    using (var dataSrc = bmpSrc.GetReadableBitmapData())
                    using (var dataDst = bmpDst.GetReadWriteBitmapData())
                    {
                        dataSrc.DrawInto(dataDst, Point.Empty);
                    }
                }, "BitmapDataExtensions.DrawInto (native to native)")
                .AddCase(() =>
                {
                    using var bitmapDataDst = BitmapDataFactory.CreateBitmapData(size, pixelFormat.ToKnownPixelFormatInternal());
                    using (var dataSrc = bmpSrc.GetReadableBitmapData())
                    {
                        dataSrc.DrawInto(bitmapDataDst, Point.Empty);
                    }
                }, "BitmapDataExtensions.DrawInto (native to managed)")
                .DoTest()
                .DumpResults(Console.Out);
        }

        [TestCase(PixelFormat.Format64bppPArgb)]
        [TestCase(PixelFormat.Format32bppPArgb)]
        [TestCase(PixelFormat.Format32bppArgb)]
        [TestCase(PixelFormat.Format24bppRgb)]
        public void DrawIntoWithResizeTest(PixelFormat pixelFormat)
        {
            //using var bmpSrc = new Bitmap(@"D:\Dokumentumok\Képek\Formats\_test\AlphaGradient.png").ConvertPixelFormat(pixelFormat);
            using var bmpSrc = Icons.Information.ExtractBitmap(new Size(256, 256)).ConvertPixelFormat(pixelFormat);
            Rectangle targetRect = new Rectangle(32, 32, 500, 200);

            new PerformanceTest { TestName = pixelFormat.ToString(), Iterations = 100, CpuAffinity = null }
                .AddCase(() =>
                {
                    using var bmpDst = bmpSrc.CloneBitmap();
                    using (var g = Graphics.FromImage(bmpDst))
                    {
                        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        g.DrawImage(bmpSrc, targetRect);
                    }
                }, "Graphics.DrawImage")
                .AddCase(() =>
                {
                    using var bmpDst = bmpSrc.CloneBitmap();
                    using (var dataSrc = bmpSrc.GetReadableBitmapData())
                    using (var dataDst = bmpDst.GetReadWriteBitmapData())
                    {
                        dataSrc.DrawInto(dataDst, targetRect);
                    }
                }, "ImageExtensions.DrawInto")
                .DoTest()
                .DumpResults(Console.Out);
        }

        #endregion
    }
}
