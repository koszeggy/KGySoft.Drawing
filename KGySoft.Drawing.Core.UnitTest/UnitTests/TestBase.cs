﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: TestBase.cs
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
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

using KGySoft.CoreLibraries;
using KGySoft.Drawing.Imaging;

using NUnit.Framework;

#endregion

namespace KGySoft.Drawing.UnitTests
{
    public abstract class TestBase
    {
        #region Constants
#pragma warning disable CS0162 // to suppress "unreachable code" warning controlled by the constant

        protected const bool SaveToFile = false;

        #endregion

        #region Methods

        #region Protected Methods

        protected static void SaveImage(string imageName, Image image, bool origFormat = false, [CallerMemberName]string testName = null)
        {
            if (!SaveToFile)
                return;

            string dir = Path.Combine(Files.GetExecutingPath(), "TestResults");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            string fileName = Path.Combine(dir, $"{testName}{(imageName == null ? null : $"_{imageName}")}.{DateTime.Now:yyyyMMddHHmmssffff}");
            ImageCodecInfo encoder = null;
            if (origFormat)
                encoder = ImageCodecInfo.GetImageEncoders().FirstOrDefault(e => e.FormatID == image.RawFormat.Guid);

            bool toIcon = image.RawFormat.Guid == ImageFormat.Icon.Guid;
            bool toGif = !toIcon && encoder == null && image.GetBitsPerPixel() <= 8;

            if (encoder != null)
            {
                if (encoder.FormatID == ImageFormat.Bmp.Guid)
                    image.SaveAsBmp($"{fileName}.bmp");
                else if (encoder.FormatID == ImageFormat.Jpeg.Guid)
                    image.SaveAsJpeg($"{fileName}.jpeg");
                else if (encoder.FormatID == ImageFormat.Png.Guid)
                    image.SaveAsPng($"{fileName}.png");
                else if (encoder.FormatID == ImageFormat.Gif.Guid)
                    image.SaveAsGif($"{fileName}.gif");
                else if (encoder.FormatID == ImageFormat.Tiff.Guid)
                    image.SaveAsTiff($"{fileName}.tiff", false);
                return;
            }

            if (image is Metafile metafile)
            {
                metafile.SaveAsWmf($"{fileName}.wmf");
                return;
            }

            if (toIcon)
                image.SaveAsIcon($"{fileName}.ico");
            else if (toGif)
                image.SaveAsGif($"{fileName}.gif");
            else
                image.SaveAsPng($"{fileName}.png");
        }

        protected static void SaveStream(string streamName, MemoryStream ms, string extension = "gif", [CallerMemberName]string testName = null)
        {
            if (!SaveToFile)
                return;

            string dir = Path.Combine(Files.GetExecutingPath(), "TestResults");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            string fileName = Path.Combine(dir, $"{testName}{(streamName == null ? null : $"_{streamName}")}.{DateTime.Now:yyyyMMddHHmmssffff}.{extension}");
            using (var fs = File.Create(fileName))
                ms.WriteTo(fs);
        }

        protected static void SaveBitmapData(string imageName, IReadWriteBitmapData bitmapData, [CallerMemberName]string testName = null)
        {
            if (!SaveToFile)
                return;

            using var bmp = bitmapData.ToBitmap();
            SaveImage(imageName, bmp, false, testName);
        }

        protected static void EncodeAnimatedGif(AnimatedGifConfiguration config, bool performCompare = true, string streamName = null, [CallerMemberName]string testName = null)
        {
            using var ms = new MemoryStream();
            GifEncoder.EncodeAnimation(config, ms);
            SaveStream(streamName, ms, testName: testName);
            IReadableBitmapData[] sourceFrames = config.Frames.ToArray(); // actually 2nd enumeration
            ms.Position = 0;

            using Bitmap restored = new Bitmap(ms);
            Bitmap[] actualFrames = restored.ExtractBitmaps();
            try
            {
                int expectedLength = sourceFrames.Length + (config.AnimationMode == AnimationMode.PingPong ? Math.Max(0, sourceFrames.Length - 2) : 0);
                Assert.AreEqual(expectedLength, actualFrames.Length);
                if (!performCompare)
                    return;

                var quantizer = config.Quantizer ?? OptimizedPaletteQuantizer.Wu();
                for (int i = 0; i < actualFrames.Length; i++)
                {
                    IReadableBitmapData sourceFrame = sourceFrames[i];
                    if (sourceFrame.IsDisposed)
                        continue;
                    Console.Write($"Frame #{i}: ");
                    using IReadableBitmapData actualFrame = actualFrames[i].GetReadableBitmapData();
                    IReadWriteBitmapData expectedFrame;
                    if (sourceFrame.GetSize() == actualFrame.GetSize())
                        expectedFrame = sourceFrames[i].Clone(KnownPixelFormat.Format8bppIndexed, quantizer, config.Ditherer);
                    else
                    {
                        Assert.AreNotEqual(AnimationFramesSizeHandling.ErrorIfDiffers, config.SizeHandling);
                        expectedFrame = BitmapDataFactory.CreateBitmapData(actualFrame.GetSize());
                        if (config.SizeHandling == AnimationFramesSizeHandling.Resize)
                            sourceFrame.DrawInto(expectedFrame, new Rectangle(Point.Empty, expectedFrame.GetSize()), quantizer, config.Ditherer);
                        else
                            sourceFrame.DrawInto(expectedFrame, new Point(expectedFrame.Width / 2 - sourceFrame.Width / 2, expectedFrame.Height / 2 - expectedFrame.Width / 2), quantizer, config.Ditherer);
                    }

                    try
                    {
                        AssertAreEqual(expectedFrame, actualFrame, true);
                    }
                    finally
                    {
                        expectedFrame.Dispose();
                    }

                    Console.WriteLine("Equals");
                }
            }
            finally
            {
                actualFrames.ForEach(f => f.Dispose());
            }
        }

        protected static IReadWriteBitmapData GenerateAlphaGradientBitmapData(Size size)
        {
            var result = BitmapDataFactory.CreateBitmapData(size);
            GenerateAlphaGradient(result);
            return result;
        }

        protected static void GenerateAlphaGradient(IReadWriteBitmapData bitmapData)
        {
            var firstRow = bitmapData.FirstRow;
            float ratio = 255f / (bitmapData.Width / 6f);
            float limit = bitmapData.Width / 6f;

            for (int x = 0; x < bitmapData.Width; x++)
            {
                // red -> yellow
                if (x < limit)
                    firstRow[x] = new Color32(255, (x * ratio).ClipToByte(), 0);
                // yellow -> green
                else if (x < limit * 2)
                    firstRow[x] = new Color32((255 - (x - limit) * ratio).ClipToByte(), 255, 0);
                // green -> cyan
                else if (x < limit * 3)
                    firstRow[x] = new Color32(0, 255, ((x - limit * 2) * ratio).ClipToByte());
                // cyan -> blue
                else if (x < limit * 4)
                    firstRow[x] = new Color32(0, (255 - (x - limit * 3) * ratio).ClipToByte(), 255);
                // blue -> magenta
                else if (x < limit * 5)
                    firstRow[x] = new Color32(((x - limit * 4) * ratio).ClipToByte(), 0, 255);
                // magenta -> red
                else
                    firstRow[x] = new Color32(255, 0, (255 - (x - limit * 5) * ratio).ClipToByte());
            }

            if (bitmapData.Height < 2)
                return;

            var row = bitmapData[1];
            ratio = 255f / bitmapData.Height;
            do
            {
                byte a = (255 - row.Index * ratio).ClipToByte();
                for (int x = 0; x < bitmapData.Width; x++)
                    row[x] = Color32.FromArgb(a, firstRow[x]);

            } while (row.MoveNextRow());
        }

        protected static void AssertAreEqual(IReadableBitmapData source, IReadableBitmapData target, bool allowDifferentPixelFormats = false, Rectangle sourceRectangle = default, Point targetLocation = default, int tolerance = 0)
        {
            if (sourceRectangle == default)
                sourceRectangle = new Rectangle(Point.Empty, source.GetSize());

            Assert.AreEqual(sourceRectangle.Size, target.GetSize());
            if (!allowDifferentPixelFormats)
                Assert.AreEqual(source.PixelFormat, target.PixelFormat);
            
            IReadableBitmapDataRow rowSrc = source[sourceRectangle.Y];
            IReadableBitmapDataRow rowDst = target[targetLocation.Y];

            do
            {
                if (tolerance > 0)
                {
                    for (int x = 0; x < sourceRectangle.Width; x++)
                    {
                        Color32 c1 = rowSrc[x + sourceRectangle.X];
                        Color32 c2 = rowDst[x + targetLocation.X];

                        // this is faster than the asserts below
                        if (!(c1.A == 0 && c2.A == 0) &&
                            (c1.A != c2.A
                            || Math.Abs(c1.R - c2.R) > tolerance
                            || Math.Abs(c1.G - c2.G) > tolerance
                            || Math.Abs(c1.B - c2.B) > tolerance))
                            Assert.Fail($"Diff at {x}; {rowSrc.Index}: {c1} vs. {c2}");

                        //Assert.AreEqual(c1.A, c2.A, $"Diff at {x}; {rowSrc.Index}");
                        //Assert.That(() => Math.Abs(c1.R - c2.R), new LessThanOrEqualConstraint(tolerance), $"Diff at {x}; {rowSrc.Index}");
                        //Assert.That(() => Math.Abs(c1.G - c2.G), new LessThanOrEqualConstraint(tolerance), $"Diff at {x}; {rowSrc.Index}");
                        //Assert.That(() => Math.Abs(c1.B - c2.B), new LessThanOrEqualConstraint(tolerance), $"Diff at {x}; {rowSrc.Index}");
                    }

                    continue;
                }

                for (int x = 0; x < sourceRectangle.Width; x++)
                {
                    Color32 c1 = rowSrc[x + sourceRectangle.X];
                    Color32 c2 = rowDst[x + targetLocation.X];
                    if (!(c1.A == 0 && c2.A == 0) && c1.ToArgb() != c2.ToArgb())
                        Assert.Fail($"Diff at {x}; {rowSrc.Index}: {c1} vs. {c2}");
                    //Assert.AreEqual(rowSrc[x + sourceRectangle.X], rowDst[x + targetLocation.X], $"Diff at {x}; {rowSrc.Index}");
                }
            } while (rowSrc.MoveNextRow() && rowDst.MoveNextRow());
        }

        #endregion

        #endregion
    }
}