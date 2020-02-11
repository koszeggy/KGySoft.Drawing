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
using System.Runtime.CompilerServices;
using KGySoft.CoreLibraries;
using NUnit.Framework;

#endregion

namespace KGySoft.Drawing.UnitTests
{
    public abstract class TestBase
    {
        #region Constants

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

            string dir = Path.Combine(Files.GetExecutingPath(), "TestResults");
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

            string dir = Path.Combine(Files.GetExecutingPath(), "TestResults");
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
            if (encoder == null)
                fileName += $".{(toIcon ? "ico" : toGif ? "gif" : "png")}";

            using (var fs = File.Create(fileName))
            {
                if (encoder != null)
                {
                    image.Save(fs, encoder, null);
                    return;
                }

                if (image is Metafile metafile)
                {
                    metafile.Save(fs);
                    return;
                }

                if (toIcon)
                    image.SaveAsIcon(fs);
                else if (toGif)
                    image.SaveAsGif(fs);
                else if (image.PixelFormat == PixelFormat.Format16bppGrayScale || image.PixelFormat.ToBitsPerPixel() > 32)
                {
                    using var toSave = image.ConvertPixelFormat(PixelFormat.Format32bppArgb);
                    toSave.Save(fs, ImageFormat.Png);
                }
                else
                    image.Save(fs, ImageFormat.Png);
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