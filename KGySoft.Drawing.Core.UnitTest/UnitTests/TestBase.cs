#region Copyright

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
        #region Properties

        private static bool SaveToFile => false;

        #endregion

        #region Methods

        #region Protected Methods

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

        protected static void SaveBitmapData(string imageName, IReadWriteBitmapData source, [CallerMemberName]string testName = null)
        {
            if (!SaveToFile)
                return;

            using var bmp = new Bitmap(source.Width, source.Height,
                source.PixelFormat.BitsPerPixel <= 8 && source.PixelFormat.Indexed ? PixelFormat.Format8bppIndexed
                : source.PixelFormat.HasAlpha ? PixelFormat.Format32bppArgb
                : PixelFormat.Format24bppRgb);

            // setting palette
            if (source.PixelFormat.Indexed && source.PixelFormat.BitsPerPixel <= 8)
            {
                ColorPalette palette = bmp.Palette;
                Color32[] src = source.Palette!.Entries;
                Color[] dst = palette.Entries;
                for (int i = 0; i < source.Palette!.Count; i++)
                    dst[i] = src[i].ToColor();
                bmp.Palette = palette;
            }

            // copying content
            BitmapData bitmapData = bmp.LockBits(new Rectangle(Point.Empty, bmp.Size), ImageLockMode.WriteOnly, bmp.PixelFormat);
            using (var target = BitmapDataFactory.CreateBitmapData(bitmapData.Scan0, bmp.Size, bitmapData.Stride, (KnownPixelFormat)bmp.PixelFormat,
                source.Palette, disposeCallback: () => bmp.UnlockBits(bitmapData)))
            {
                source.CopyTo(target);
            }

            SaveBitmap(imageName, bmp, testName);
        }

        protected static void EncodeAnimatedGif(AnimatedGifConfiguration config, bool performCompare = true, string streamName = null, [CallerMemberName]string testName = null)
        {
            using var ms = new MemoryStream();
            GifEncoder.EncodeAnimation(config, ms);
            SaveStream(streamName, ms, testName: testName);
            IReadableBitmapData[] sourceFrames = config.Frames.ToArray(); // actually 2nd enumeration
            ms.Position = 0;

            using Bitmap restored = new Bitmap(ms);
            Bitmap[] actualFrames = ExtractBitmaps(restored);
            try
            {
                int expectedLength = sourceFrames.Length + (config.AnimationMode == AnimationMode.PingPong ? Math.Max(0, sourceFrames.Length - 2) : 0);
                Assert.AreEqual(expectedLength, actualFrames.Length);
                if (!performCompare)
                    return;

                var size = restored.Size;
                var quantizer = config.Quantizer ?? OptimizedPaletteQuantizer.Wu();
                for (int i = 0; i < actualFrames.Length; i++)
                {
                    IReadableBitmapData sourceFrame = sourceFrames[i];
                    if (sourceFrame.IsDisposed)
                        continue;
                    Console.Write($"Frame #{i}: ");
                    BitmapData bitmapData = actualFrames[i].LockBits(new Rectangle(Point.Empty, size), ImageLockMode.ReadOnly, actualFrames[i].PixelFormat);
                    using IReadableBitmapData actualFrame = BitmapDataFactory.CreateBitmapData(bitmapData.Scan0, size, bitmapData.Stride, KnownPixelFormat.Format32bppArgb, disposeCallback: () => actualFrames[i].UnlockBits(bitmapData));
                    IReadWriteBitmapData expectedFrame;
                    if (sourceFrame.Size == actualFrame.Size)
                        expectedFrame = sourceFrames[i].Clone(KnownPixelFormat.Format8bppIndexed, quantizer, config.Ditherer);
                    else
                    {
                        Assert.AreNotEqual(AnimationFramesSizeHandling.ErrorIfDiffers, config.SizeHandling);
                        expectedFrame = BitmapDataFactory.CreateBitmapData(actualFrame.Size);
                        if (config.SizeHandling == AnimationFramesSizeHandling.Resize)
                            sourceFrame.DrawInto(expectedFrame, new Rectangle(Point.Empty, expectedFrame.Size), quantizer, config.Ditherer);
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

        protected static Bitmap[] ExtractBitmaps(Bitmap image)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image), PublicResources.ArgumentNull);

            var dimension = FrameDimension.Time;
            int frameCount = image.FrameDimensionsList.Length == 0 ? 1 : image.GetFrameCount(dimension);

            if (frameCount <= 1)
                return new Bitmap[] { image.Clone(new Rectangle(Point.Empty, image.Size), image.PixelFormat) };

            // extracting frames
            Bitmap[] result = new Bitmap[frameCount];
            for (int frame = 0; frame < frameCount; frame++)
            {
                image.SelectActiveFrame(dimension, frame);
                result[frame] = image.Clone(new Rectangle(Point.Empty, image.Size), image.PixelFormat);
            }

            // selecting first frame again
            image.SelectActiveFrame(dimension, 0);

            return result;
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

            var row = bitmapData.GetMovableRow(1);
            ratio = 255f / bitmapData.Height;
            do
            {
                byte a = (255 - row.Index * ratio).ClipToByte();
                for (int x = 0; x < bitmapData.Width; x++)
                    row[x] = Color32.FromArgb(a, firstRow[x]);

            } while (row.MoveNextRow());
        }

        protected static IReadWriteBitmapData GetInfoIcon256() => GetBitmapData(@"..\..\..\..\Help\Images\Information256.png");
        protected static IReadWriteBitmapData GetInfoIcon64() => GetBitmapData(@"..\..\..\..\Help\Images\Information64.png");
        protected static IReadWriteBitmapData GetInfoIcon48() => GetBitmapData(@"..\..\..\..\Help\Images\Information48.png");
        protected static IReadWriteBitmapData GetInfoIcon16() => GetBitmapData(@"..\..\..\..\Help\Images\Information16.png");
        protected static IReadWriteBitmapData[] GetInfoIconImages() => new[]
        {
            GetInfoIcon256(),
            GetInfoIcon64(),
            GetInfoIcon48(),
            GetBitmapData(@"..\..\..\..\Help\Images\Information32.png"),
            GetBitmapData(@"..\..\..\..\Help\Images\Information24.png"),
            GetBitmapData(@"..\..\..\..\Help\Images\Information20.png"),
            GetInfoIcon16()
        };

        protected static IReadWriteBitmapData GetShieldIcon256() => GetBitmapData(@"..\..\..\..\Help\Images\Shield256.png");

        protected static IReadWriteBitmapData ToBitmapData(Bitmap bmp)
        {
            Assert.IsTrue(bmp.PixelFormat.In(PixelFormat.Format32bppArgb, PixelFormat.Format8bppIndexed, PixelFormat.Format24bppRgb));
            Palette palette = ((int)bmp.PixelFormat & (int)PixelFormat.Indexed) != 0
                ? new Palette(bmp.Palette.Entries)
                : null;
            BitmapData bitmapData = bmp.LockBits(new Rectangle(Point.Empty, bmp.Size), ImageLockMode.ReadOnly, bmp.PixelFormat);
            using var src = BitmapDataFactory.CreateBitmapData(bitmapData.Scan0, bmp.Size, bitmapData.Stride, (KnownPixelFormat)bmp.PixelFormat,
                palette, disposeCallback: () => bmp.UnlockBits(bitmapData));
            return src.Clone();
        }

        protected static IReadWriteBitmapData GetBitmapData(string fileName)
        {
            using var bmp = new Bitmap(Path.Combine(Files.GetExecutingPath(), fileName));
            return ToBitmapData(bmp);
        }

        protected static void AssertAreEqual(IReadableBitmapData source, IReadableBitmapData target, bool allowDifferentPixelFormats = false, Rectangle sourceRectangle = default, Point targetLocation = default, int tolerance = 0)
        {
            if (sourceRectangle == default)
                sourceRectangle = new Rectangle(Point.Empty, source.Size);

            Assert.AreEqual(sourceRectangle.Size, target.Size);
            if (!allowDifferentPixelFormats)
                Assert.AreEqual(source.PixelFormat, target.PixelFormat);
            
            IReadableBitmapDataRowMovable rowSrc = source.GetMovableRow(sourceRectangle.Y);
            IReadableBitmapDataRowMovable rowDst = target.GetMovableRow(targetLocation.Y);

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

        #region Private Methods

        private static void SaveBitmap(string imageName, Bitmap bitmap, string testName)
        {
            if (!SaveToFile)
                return;

            string dir = Path.Combine(Files.GetExecutingPath(), "TestResults");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            string fileName = Path.Combine(dir, $"{testName}{(imageName == null ? null : $"_{imageName}")}.{DateTime.Now:yyyyMMddHHmmssffff}");

            int bpp = Image.GetPixelFormatSize(bitmap.PixelFormat);
            if (bpp > 8)
            {
                bitmap.Save($"{fileName}.png", ImageFormat.Png);
                return;
            }

            // to prevent GIF encoder from re-quantizing the image
            Bitmap toSave = bitmap;
            if (bpp < 8)
            {
                toSave = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format8bppIndexed);
                toSave.Palette = bitmap.Palette;
                BitmapData srcBitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
                BitmapData dstBitmapData = toSave.LockBits(new Rectangle(0, 0, toSave.Width, toSave.Height), ImageLockMode.WriteOnly, toSave.PixelFormat);
                using (IReadWriteBitmapData src = BitmapDataFactory.CreateBitmapData(srcBitmapData.Scan0, bitmap.Size, srcBitmapData.Stride, (KnownPixelFormat)bitmap.PixelFormat))
                using (IReadWriteBitmapData dst = BitmapDataFactory.CreateBitmapData(dstBitmapData.Scan0, toSave.Size, dstBitmapData.Stride, KnownPixelFormat.Format8bppIndexed))
                    src.CopyTo(dst);
                bitmap.UnlockBits(srcBitmapData);
                toSave.UnlockBits(dstBitmapData);
            }

            bitmap.Save($"{fileName}.gif", ImageFormat.Gif);
            if (!ReferenceEquals(bitmap, toSave))
                toSave.Dispose();
        }

        #endregion

        #endregion
    }
}