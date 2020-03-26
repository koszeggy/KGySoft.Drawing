#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: TestBase.cs
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
using System.IO;
using System.Linq;
using System.Reflection;
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
        #region Constants
#pragma warning disable CS0162 // to suppress "unreachable code" warning controlled by the constant

        private const bool saveToFile = true;

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

        protected static void SaveIcon(string iconName, Icon icon, [CallerMemberName]string testName = null)
        {
            if (!saveToFile || icon == null)
                return;

            string dir = Path.Combine(Path.GetDirectoryName(Files.GetExecutingPath()), "TestResults");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            string fileName = Path.Combine(dir, $"{testName}_{iconName}.{DateTime.Now:yyyyMMddHHmmssffff}.ico");
            using (var fs = File.Create(fileName))
                icon.SaveHighQuality(fs);
        }

        protected static void SaveImage(string imageName, Image image, bool origFormat = false, [CallerMemberName]string testName = null)
        {
            if (!saveToFile)
                return;

            string dir = Path.Combine(Path.GetDirectoryName(Files.GetExecutingPath()), "TestResults");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            string fileName = Path.Combine(dir, $"{testName}_{imageName}.{DateTime.Now:yyyyMMddHHmmssffff}");
            ImageCodecInfo encoder = null;
            if (origFormat)
            {
                encoder = ImageCodecInfo.GetImageEncoders().FirstOrDefault(e => e.FormatID == image.RawFormat.Guid);
                if (encoder != null)
                    fileName += Path.GetExtension(encoder.FilenameExtension.Split(';').First());
            }

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
                    image.SaveAsTiff($"{fileName}.tiff");
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

        protected static Bitmap CreateBitmap(int size, PixelFormat pixelFormat)
        {
            try
            {
                return new Bitmap(size, size, pixelFormat);
            }
            catch (Exception e)
            {
                if (OSUtils.IsWindows || pixelFormat.IsSupported())
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
                if (pixelFormat.IsSupported())
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
                if (pixelFormat.IsSupported())
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

        #endregion
    }
}