#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ImageExtensionsPerformanceTest.cs
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
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using KGySoft.Drawing.Imaging;
using NUnit.Framework;

#endregion

namespace KGySoft.Drawing.PerformanceTests.Imaging
{
    [TestFixture]
    public class ReadableBitmapDataExtensionsPerformanceTest
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
            using var bmpDst = new Bitmap(size.Width, size.Height, pixelFormat);
            using var bitmapDataDst = BitmapDataFactory.CreateBitmapData(size, pixelFormat);

                new PerformanceTest { TestName = pixelFormat.ToString(), Iterations = 100, CpuAffinity = null }
                .AddCase(() =>
                {
                    using (var g = Graphics.FromImage(bmpDst))
                    {
                        g.CompositingMode = CompositingMode.SourceCopy; // so it is the same as CopyTo (and is much faster)
                        g.DrawImage(bmpSrc, Point.Empty);
                    }
                }, "Graphics.DrawImage")
                .AddCase(() =>
                {
                    using (var dataSrc = bmpSrc.GetReadableBitmapData())
                    using (var dataDst = bmpDst.GetWritableBitmapData())
                    {
                        dataSrc.CopyTo(dataDst, Point.Empty);
                    }
                }, "ImageExtensions.CopyTo (native to native)")
                .AddCase(() =>
                {
                    using (var dataSrc = bmpSrc.GetReadableBitmapData())
                    {
                        dataSrc.CopyTo(bitmapDataDst, Point.Empty);
                    }
                }, "ImageExtensions.CopyTo (native to managed)")
                .DoTest()
                .DumpResults(Console.Out);
        }

        #endregion
    }
}
