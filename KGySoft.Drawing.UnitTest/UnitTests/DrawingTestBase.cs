#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: DrawingTestBase.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2022 - All Rights Reserved
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
using System.Runtime.CompilerServices;

using KGySoft.CoreLibraries;
using KGySoft.Drawing.Imaging;
using KGySoft.Drawing.WinApi;

using NUnit.Framework;

#endregion

namespace KGySoft.Drawing.UnitTests
{
    public abstract class DrawingTestBase : TestBase
    {
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

        #endregion
    }
}