#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: TestBase.cs
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
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

using KGySoft.CoreLibraries;
using KGySoft.Drawing.Imaging;
using KGySoft.Drawing.WinApi;

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

        protected static void AssertPlatformDependent(Action code, params PlatformID[] platforms)
        {
            try
            {
                code.Invoke();
            }
            catch (Exception e)
            {
                if (Environment.OSVersion.Platform.In(platforms))
                    throw;
                Assert.Inconclusive($"Test failed on platform {Environment.OSVersion.Platform}: {e.Message}");
            }
        }

        protected static void SaveImage(string imageName, Image image, bool origFormat = false, [CallerMemberName] string testName = null)
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

        protected static void SaveIcon(string iconName, Icon icon, [CallerMemberName] string testName = null)
        {
            if (!SaveToFile || icon == null)
                return;

            string dir = Path.Combine(Files.GetExecutingPath(), "TestResults");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            string fileName = Path.Combine(dir, $"{testName}{(iconName == null ? null : $"_{iconName}")}.{DateTime.Now:yyyyMMddHHmmssffff}.ico");
            using (var fs = File.Create(fileName))
                icon.SaveAsIcon(fs);
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

        protected static Bitmap CreateBitmap(int size, PixelFormat pixelFormat)
        {
            try
            {
                return new Bitmap(size, size, pixelFormat);
            }
            catch (Exception e)
            {
                if (OSUtils.IsWindows || pixelFormat.IsSupportedNatively())
                    throw;
                Assert.Inconclusive($"PixelFormat {pixelFormat} is not supported on Linux: {e.Message}");
                throw;
            }
        }

        protected static Bitmap Convert(Bitmap bitmap, PixelFormat pixelFormat, IQuantizer quantizer = null)
        {
            try
            {
                if (bitmap.PixelFormat == pixelFormat)
                    return bitmap;
                return bitmap.ConvertPixelFormat(pixelFormat, quantizer);
            }
            catch (Exception e)
            {
                if (pixelFormat.IsSupportedNatively())
                    throw;
                Assert.Inconclusive($"PixelFormat {pixelFormat} is not supported: {e.Message}");
                throw;
            }
        }

        protected static Bitmap Convert(Bitmap bitmap, PixelFormat pixelFormat, Color backColor)
        {
            try
            {
                if (bitmap.PixelFormat == pixelFormat)
                    return bitmap;
                return bitmap.ConvertPixelFormat(pixelFormat, backColor);
            }
            catch (Exception e)
            {
                if (pixelFormat.IsSupportedNatively())
                    throw;
                Assert.Inconclusive($"PixelFormat {pixelFormat} is not supported: {e.Message}");
                throw;
            }
        }

        protected static Metafile GenerateMetafile()
        {
            //Set up reference Graphic
            Graphics refGraph = Graphics.FromHwnd(IntPtr.Zero);
            IntPtr hdc = refGraph.GetHdc();
            Metafile result = new Metafile(hdc, new Rectangle(0, 0, 100, 100), MetafileFrameUnit.Pixel, EmfType.EmfOnly, "Test");

            //Draw some silly drawing
            using (var g = Graphics.FromImage(result))
            {
                var r = new Rectangle(0, 0, 100, 100);
                var leftEye = new Rectangle(20, 20, 20, 30);
                var rightEye = new Rectangle(60, 20, 20, 30);
                g.FillEllipse(Brushes.Yellow, r);
                g.FillEllipse(Brushes.White, leftEye);
                g.FillEllipse(Brushes.White, rightEye);
                g.DrawEllipse(Pens.Black, leftEye);
                g.DrawEllipse(Pens.Black, rightEye);
                g.DrawBezier(Pens.Red, new Point(10, 50), new Point(10, 100), new Point(90, 100), new Point(90, 50));
            }

            refGraph.ReleaseHdc(hdc); //cleanup
            refGraph.Dispose();
            return result;
        }

        protected static Bitmap GenerateAlphaGradientBitmap(Size size)
        {
            var result = new Bitmap(size.Width, size.Height);
            using var bitmapData = result.GetReadWriteBitmapData();
            GenerateAlphaGradient(bitmapData);
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

        protected static void AssertAreEqual(IReadableBitmapData source, IReadableBitmapData target, bool allowDifferentPixelFormats = false)
        {
            Assert.AreEqual(source.Size, target.Size);
            if (!allowDifferentPixelFormats)
                Assert.AreEqual(source.PixelFormat, target.PixelFormat);

            IReadableBitmapDataRowMovable rowSrc = source.FirstRow;
            IReadableBitmapDataRowMovable rowDst = target.FirstRow;

            do
            {
                for (int x = 0; x < source.Width; x++)
                {
                    Color32 c1 = rowSrc[x];
                    Color32 c2 = rowDst[x];
                    if (!(c1.A == 0 && c2.A == 0) && c1.ToArgb() != c2.ToArgb())
                        Assert.Fail($"Diff at {x}; {rowSrc.Index}: {c1} vs. {c2}");
                    //Assert.AreEqual(rowSrc[x + sourceRectangle.X], rowDst[x + targetLocation.X], $"Diff at {x}; {rowSrc.Index}");
                }
            } while (rowSrc.MoveNextRow() && rowDst.MoveNextRow());
        }

        #endregion
    }
}