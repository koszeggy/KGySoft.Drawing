#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BitmapAccessorPerformanceTest.cs
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
using KGySoft.CoreLibraries;
using KGySoft.Diagnostics;
using KGySoft.Drawing.Imaging;

using NUnit.Framework;

#endregion

namespace KGySoft.Drawing.PerformanceTests.Imaging
{
    [TestFixture]
    public class BitmapAccessorPerformanceTest
    {
        #region Methods

        [TestCase(PixelFormat.Format32bppArgb)]
        [TestCase(PixelFormat.Format32bppPArgb)]
        [TestCase(PixelFormat.Format64bppArgb)]
        [TestCase(PixelFormat.Format64bppPArgb)]
        [TestCase(PixelFormat.Format24bppRgb)]
        [TestCase(PixelFormat.Format48bppRgb)]
        public void SetGetPixelTest(PixelFormat pixelFormat)
        {
            static int Argb(int a, int l) => (a << 24) | (l << 16) | (l << 8) | l;

            var size = new Size(256, 256);
            using var bmp = new Bitmap(size.Width, size.Height, pixelFormat);

            new PerformanceTest { TestName = pixelFormat.ToString(), Iterations = 10 }
                .AddCase(() =>
                {
                    int diffs = 0;
                    for (int y = 0; y < size.Height; y++)
                    {
                        for (int x = 0; x < size.Width; x++)
                        {
                            int argb = Argb(y, x);
                            bmp.SetPixel(x, y, Color.FromArgb(argb));
                            if (bmp.GetPixel(x, y).ToArgb() != argb)
                                diffs++;
                        }
                    }
                }, "Bitmap.SetPixel/GetPixel")
                .AddCase(() =>
                {
                    int diffs = 0;
                    using IBitmapDataAccessor accessor = bmp.GetBitmapDataAccessor(ImageLockMode.ReadWrite);
                    for (int y = 0; y < size.Height; y++)
                    {
                        for (int x = 0; x < size.Width; x++)
                        {
                            int argb = Argb(y, x);
                            accessor.SetPixel(x, y, Color.FromArgb(argb));
                            if (accessor.GetPixel(x, y).ToArgb() != argb)
                                diffs++;
                        }
                    }
                }, "IBitmapDataAccessor.SetPixel/GetPixel")
                .AddCase(() =>
                {
                    int diffs = 0;
                    using IBitmapDataAccessor accessor = bmp.GetBitmapDataAccessor(ImageLockMode.ReadWrite);
                    IBitmapDataRow row = accessor.FirstRow;
                    do
                    {
                        for (int x = 0; x < size.Width; x++)
                        {
                            int argb = Argb(row.Index, x);
                            row[x] = Color32.FromArgb(argb);
                            if (row[x].ToArgb() != argb)
                                diffs++;
                        }
                    } while (row.MoveNextRow());
                }, "IBitmapDataRow.this")
                .AddCase(() =>
                {
                    int diffs = 0;
                    using IBitmapDataAccessor accessor = bmp.GetBitmapDataAccessor(ImageLockMode.ReadWrite);
                    IBitmapDataRow row = accessor.FirstRow;
                    do
                    {
                        switch (pixelFormat.ToBitsPerPixel())
                        {
                            case 24:
                                for (int x = 0; x < size.Width; x++)
                                {
                                    int argb = Argb(row.Index, x);
                                    row.WriteRaw(x, new Color24(Color32.FromArgb(argb)));
                                    if (row.ReadRaw<Color24>(x).ToColor32().ToArgb() != argb)
                                        diffs++;
                                }
                                break;
                            case 32:
                                for (int x = 0; x < size.Width; x++)
                                {
                                    int argb = Argb(row.Index, x);
                                    row.WriteRaw(x, argb);
                                    if (row.ReadRaw<int>(x) != argb)
                                        diffs++;
                                }
                                break;
                            case 48:
                                for (int x = 0; x < size.Width; x++)
                                {
                                    int argb = Argb(row.Index, x);
                                    row.WriteRaw(x, new Color48(Color32.FromArgb(argb)));
                                    if (row.ReadRaw<Color48>(x).ToColor32().ToArgb() != argb)
                                        diffs++;
                                }
                                break;
                            case 64:
                                for (int x = 0; x < size.Width; x++)
                                {
                                    int argb = Argb(row.Index, x);
                                    row.WriteRaw(x, Color64.FromArgb32(argb));
                                    if (row.ReadRaw<Color64>(x).ToArgb32() != argb)
                                        diffs++;
                                }
                                break;
                            default:
                                throw new NotImplementedException(pixelFormat.ToString());
                        }
                    } while (row.MoveNextRow());
                }, "IBitmapDataRow.WriteRaw/ReadRaw")
                .AddCase(() =>
                {
                    int diffs = 0;
                    using IBitmapDataAccessor accessor = bmp.GetBitmapDataAccessor(ImageLockMode.ReadWrite);
                    IBitmapDataRow row = accessor.FirstRow;
                    do
                    {
                        for (int x = 0; x < size.Width; x++)
                        {
                            int argb = Argb(row.Index, x);
                            row.SetColor(x, Color.FromArgb(argb));
                            if (row.GetColor(x).ToArgb() != argb)
                                diffs++;
                        }
                    } while (row.MoveNextRow());
                }, "IBitmapDataRow.SetColor/GetColor")
                .AddCase(() =>
                {
                    int diffs = 0;
                    using BitmapDataAccessorBase accessor = BitmapDataAccessorFactory.CreateAccessor(bmp, ImageLockMode.ReadWrite, false);
                    BitmapDataRowBase row = accessor.GetRow(0);
                    do
                    {
                        for (int x = 0; x < size.Width; x++)
                        {
                            int argb = Argb(row.Line, x);
                            row.DoSetColor32(x, Color32.FromArgb(argb));
                            if (row.DoGetColor32(x).ToArgb() != argb)
                                diffs++;
                        }
                    } while (row.MoveNextRow());
                }, "BitmapDataRowBase.DoSetColor32/DoGetColor32")
                .DoTest()
                .DumpResults(Console.Out);
        }

        /*
         var bmp = new Bitmap(5, 1, PixelFormat.Format64bppArgb);
                    bmp.SetPixel(0, 0, Color.Black);
                    bmp.SetPixel(1, 0, Color.Red);
                    bmp.SetPixel(2, 0, Color.Green);
                    bmp.SetPixel(3, 0, Color.Blue);
                    bmp.SetPixel(4, 0, Color.White);
                    image = bmp;
*/

        #endregion
    }
}
