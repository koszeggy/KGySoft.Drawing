﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KGySoft.Drawing.Imaging;

using NUnit.Framework;

using SkiaSharp;

namespace KGySoft.Drawing.SkiaSharp.PerformanceTests
{
    [TestFixture]
    public class SKBitmapExtensionsPerformanceTest
    {
        [TestCase(SKColorType.Bgra8888, SKAlphaType.Unpremul)]
        [TestCase(SKColorType.Bgra8888, SKAlphaType.Premul)]
        [TestCase(SKColorType.Bgra8888, SKAlphaType.Opaque)]
        public void SetGetPixelTest(SKColorType colorType, SKAlphaType alphaType)
        {
            static int Argb(int a, int l) => (a << 24) | (l << 16) | (l << 8) | l;

            var size = new Size(256, 256);
            using var bmp = new SKBitmap(size.Width, size.Height, colorType, alphaType);

            new PerformanceTest { TestName = $"{colorType}/{alphaType}", Iterations = 10 }
                .AddCase(() =>
                {
                    for (int y = 0; y < size.Height; y++)
                    {
                        for (int x = 0; x < size.Width; x++)
                        {
                            bmp.SetPixel(x, y, new SKColor((uint)Argb(y, x)));
                            bmp.GetPixel(x, y);
                        }
                    }
                }, "SKBitmap.SetPixel/GetPixel")
                .AddCase(() =>
                {
                    using IReadWriteBitmapData accessor = bmp.GetReadWriteBitmapData();
                    for (int y = 0; y < size.Height; y++)
                    {
                        for (int x = 0; x < size.Width; x++)
                        {
                            accessor.SetPixel(x, y, Color.FromArgb(Argb(y, x)));
                            accessor.GetPixel(x, y);
                        }
                    }
                }, "IReadWriteBitmapData.SetPixel/GetPixel")
                .AddCase(() =>
                {
                    using IReadWriteBitmapData accessor = bmp.GetReadWriteBitmapData();
                    for (int y = 0; y < size.Height; y++)
                    {
                        for (int x = 0; x < size.Width; x++)
                        {
                            accessor.SetColor32(x, y, Color32.FromArgb(Argb(y, x)));
                            accessor.GetColor32(x, y);
                        }
                    }
                }, "IReadWriteBitmapData.SetColor32/GetColor32 (managed)")
                .AddCase(() =>
                {
                    using IReadWriteBitmapData accessor = bmp.GetReadWriteBitmapData();
                    IReadWriteBitmapDataRowMovable row = accessor.FirstRow;
                    do
                    {
                        for (int x = 0; x < size.Width; x++)
                        {
                            row[x] = Color32.FromArgb(Argb(row.Index, x));
                            var _ = row[x];
                        }
                    } while (row.MoveNextRow());
                }, "IReadWriteBitmapDataRow.this")
                .DoTest()
                .DumpResults(Console.Out);
        }
    }
}