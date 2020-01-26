#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BitmapExtensionsTest.cs
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
    public class BitmapExtensionsTest
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
                    using IBitmapDataAccessor acc = bmp.GetBitmapDataAccessor(ImageLockMode.WriteOnly);
                    var c = new Color32(color);
                    IBitmapDataRow row = acc.FirstRow;
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

        #endregion
    }
}
