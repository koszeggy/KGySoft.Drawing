﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BitmapDataAccessPerformanceTest.cs
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
            static int Argb(int a, int l) => (a << 24) | (l << 16) | (l << 8) | l;

            if (!pixelFormat.IsSupportedNatively())
                Assert.Inconclusive($"Pixel format {pixelFormat} is not supported on current platform");

            var size = new Size(256, 256);
            using var bmp = new Bitmap(size.Width, size.Height, pixelFormat);

            new PerformanceTest<int> { TestName = pixelFormat.ToString(), Iterations = 10 }
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

                    return diffs;
                }, "Bitmap.SetPixel/GetPixel")
                .AddCase(() =>
                {
                    int diffs = 0;
                    using IReadWriteBitmapData accessor = bmp.GetReadWriteBitmapData();
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

                    return diffs;
                }, "IReadWriteBitmapData.SetPixel/GetPixel (native)")
                .AddCase(() =>
                {
                    int diffs = 0;
                    using IReadWriteBitmapData accessor = BitmapDataFactory.CreateBitmapData(size, pixelFormat);
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

                    return diffs;
                }, "IReadWriteBitmapData.SetPixel/GetPixel (managed)")
                .AddCase(() =>
                {
                    int diffs = 0;
                    using IReadWriteBitmapData accessor = bmp.GetReadWriteBitmapData();
                    IReadWriteBitmapDataRow row = accessor.FirstRow;
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

                    return diffs;
                }, "IReadWriteBitmapDataRow.this (native)")
                .AddCase(() =>
                {
                    int diffs = 0;
                    using IReadWriteBitmapData accessor = BitmapDataFactory.CreateBitmapData(size, pixelFormat);
                    IReadWriteBitmapDataRow row = accessor.FirstRow;
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

                    return diffs;
                }, "IReadWriteBitmapDataRow.this (managed)")
                .AddCase(() =>
                {
                    int diffs = 0;
                    using IReadWriteBitmapData accessor = bmp.GetReadWriteBitmapData();
                    IReadWriteBitmapDataRow row = accessor.FirstRow;
                    do
                    {
                        switch (pixelFormat.ToBitsPerPixel())
                        {
                            case 16:
                                for (int x = 0; x < size.Width; x++)
                                {
                                    int argb = Argb(row.Index, x);
                                    row.WriteRaw(x, new Color16Rgb565(Color32.FromArgb(argb)));
                                    if (row.ReadRaw<Color16Rgb565>(x).ToColor32().ToArgb() != argb)
                                        diffs++;
                                }

                                break;
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

                    return diffs;
                }, "IReadWriteBitmapDataRow.WriteRaw/ReadRaw (native)")
                .AddCase(() =>
                {
                    int diffs = 0;
                    using IReadWriteBitmapData accessor = BitmapDataFactory.CreateBitmapData(size, pixelFormat);
                    IReadWriteBitmapDataRow row = accessor.FirstRow;
                    do
                    {
                        switch (pixelFormat.ToBitsPerPixel())
                        {
                            case 16:
                                for (int x = 0; x < size.Width; x++)
                                {
                                    int argb = Argb(row.Index, x);
                                    row.WriteRaw(x, new Color16Rgb565(Color32.FromArgb(argb)));
                                    if (row.ReadRaw<Color16Rgb565>(x).ToColor32().ToArgb() != argb)
                                        diffs++;
                                }

                                break;
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

                    return diffs;
                }, "IReadWriteBitmapDataRow.WriteRaw/ReadRaw (managed)")
                .AddCase(() =>
                {
                    int diffs = 0;
                    using IReadWriteBitmapData accessor = bmp.GetReadWriteBitmapData();
                    IReadWriteBitmapDataRow row = accessor.FirstRow;
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

                    return diffs;
                }, "IReadWriteBitmapDataRow.SetColor/GetColor (native)")
                .AddCase(() =>
                {
                    int diffs = 0;
                    using IReadWriteBitmapData accessor = BitmapDataFactory.CreateBitmapData(size, pixelFormat);
                    IReadWriteBitmapDataRow row = accessor.FirstRow;
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

                    return diffs;
                }, "IReadWriteBitmapDataRow.SetColor/GetColor (managed)")
                .AddCase(() =>
                {
                    int diffs = 0;
                    using IBitmapDataInternal accessor = BitmapDataFactory.CreateBitmapData(bmp, ImageLockMode.ReadWrite);
                    IBitmapDataRowInternal row = accessor.DoGetRow(0);
                    do
                    {
                        for (int x = 0; x < size.Width; x++)
                        {
                            int argb = Argb(row.Index, x);
                            row.DoSetColor32(x, Color32.FromArgb(argb));
                            if (row.DoGetColor32(x).ToArgb() != argb)
                                diffs++;
                        }
                    } while (row.MoveNextRow());

                    return diffs;
                }, "IBitmapDataInternal.DoSetColor32/DoGetColor32 (native)")
                .AddCase(() =>
                {
                    int diffs = 0;
                    using IBitmapDataInternal accessor = BitmapDataFactory.CreateManagedBitmapData(size, pixelFormat);
                    IBitmapDataRowInternal row = accessor.DoGetRow(0);
                    do
                    {
                        for (int x = 0; x < size.Width; x++)
                        {
                            int argb = Argb(row.Index, x);
                            row.DoSetColor32(x, Color32.FromArgb(argb));
                            if (row.DoGetColor32(x).ToArgb() != argb)
                                diffs++;
                        }
                    } while (row.MoveNextRow());

                    return diffs;
                }, "IBitmapDataInternal.DoSetColor32/DoGetColor32 (managed)")
                .DoTest()
                .DumpResults(Console.Out /*, dumpReturnValue: true*/);
        }

        #endregion
    }
}
