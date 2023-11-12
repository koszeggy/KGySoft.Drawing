#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BitmapDataAccessPerformanceTest.cs
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
using System.Drawing.Imaging;

using KGySoft.Drawing.Imaging;

using NUnit.Framework;

#endregion

namespace KGySoft.Drawing.PerformanceTests.Imaging
{
    [TestFixture]
    public class BitmapDataAccessPerformanceTest
    {
        #region Methods

        [TestCase(PixelFormat.Format32bppArgb)]
        [TestCase(PixelFormat.Format32bppPArgb)]
        [TestCase(PixelFormat.Format64bppArgb)]
        [TestCase(PixelFormat.Format64bppPArgb)]
        [TestCase(PixelFormat.Format24bppRgb)]
        [TestCase(PixelFormat.Format48bppRgb)]
        [TestCase(PixelFormat.Format16bppRgb565)]
        [TestCase(PixelFormat.Format16bppRgb555)]
        [TestCase(PixelFormat.Format16bppArgb1555)]
        public void SetGetPixelTest(PixelFormat pixelFormat)
        {
            if (!pixelFormat.IsSupportedNatively())
                Assert.Inconclusive($"Pixel format {pixelFormat} is not supported on current platform");

            var size = new Size(256, 256);
            using var bmp = new Bitmap(size.Width, size.Height, pixelFormat);
            Color testColor = Color.FromArgb(128, 128, 255, 64);

            new PerformanceTest { TestName = pixelFormat.ToString(), Iterations = 10 }
                .AddCase(() =>
                {
                    for (int y = 0; y < size.Height; y++)
                    {
                        for (int x = 0; x < size.Width; x++)
                        {
                            bmp.SetPixel(x, y, testColor);
                            bmp.GetPixel(x, y);
                        }
                    }
                }, "Bitmap.SetPixel/GetPixel")
                .AddCase(() =>
                {
                    using IReadWriteBitmapData accessor = bmp.GetReadWriteBitmapData();
                    for (int y = 0; y < size.Height; y++)
                    {
                        for (int x = 0; x < size.Width; x++)
                        {
                            accessor.SetPixel(x, y, testColor);
                            accessor.GetPixel(x, y);
                        }
                    }
                }, "IReadWriteBitmapData.SetPixel/GetPixel")
                .AddCase(() =>
                {
                    using IReadWriteBitmapData accessor = bmp.GetReadWriteBitmapData();
                    IReadWriteBitmapDataRowMovable row = accessor.FirstRow;
                    do
                    {
                        for (int x = 0; x < size.Width; x++)
                        {
                            row.SetColor(x, testColor);
                            row.GetColor(x);
                        }
                    } while (row.MoveNextRow());
                }, "IReadWriteBitmapDataRow.SetColor/GetColor")
                .AddCase(() =>
                {
                    Color32 color = testColor;
                    using IReadWriteBitmapData accessor = bmp.GetReadWriteBitmapData();
                    IReadWriteBitmapDataRowMovable row = accessor.FirstRow;
                    do
                    {
                        for (int x = 0; x < size.Width; x++)
                        {
                            row.SetColor32(x, color);
                            row.GetColor32(x);
                        }
                    } while (row.MoveNextRow());
                }, "IReadWriteBitmapDataRow.SetColor32/GetColor32")
                .AddCase(() =>
                {
                    Color64 color = testColor.ToColor64();
                    using IReadWriteBitmapData accessor = bmp.GetReadWriteBitmapData();
                    IReadWriteBitmapDataRowMovable row = accessor.FirstRow;
                    do
                    {
                        for (int x = 0; x < size.Width; x++)
                        {
                            row.SetColor64(x, color);
                            row.GetColor64(x);
                        }
                    } while (row.MoveNextRow());
                }, "IReadWriteBitmapDataRow.SetColor64/GetColor64")
                .AddCase(() =>
                {
                    ColorF color = testColor.ToColorF();
                    using IReadWriteBitmapData accessor = bmp.GetReadWriteBitmapData();
                    IReadWriteBitmapDataRowMovable row = accessor.FirstRow;
                    do
                    {
                        for (int x = 0; x < size.Width; x++)
                        {
                            row.SetColorF(x, color);
                            row.GetColorF(x);
                        }
                    } while (row.MoveNextRow());
                }, "IReadWriteBitmapDataRow.SetColorF/GetColorF")
                .DoTest()
                .DumpResults(Console.Out /*, dumpReturnValue: true*/);
        }

        #endregion
    }
}
