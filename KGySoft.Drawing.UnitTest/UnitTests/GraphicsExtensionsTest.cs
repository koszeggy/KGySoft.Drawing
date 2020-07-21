#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: GraphicsExtensionsTest.cs
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
using System.Drawing.Drawing2D;
using NUnit.Framework;

#endregion

namespace KGySoft.Drawing.UnitTests
{
    [TestFixture]
    public class GraphicsExtensionsTest : TestBase
    {
        #region Methods

        [Test]
        public void DrawRoundedRectangleTest()
        {
            using (var bmp = new Bitmap(20, 15))
            {
                using (var g = Graphics.FromImage(bmp))
                {
                    g.PixelOffsetMode = PixelOffsetMode.Half;
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.DrawRoundedRectangle(Pens.Blue, Rectangle.Round(g.VisibleClipBounds), 5);
                }

                SaveImage("5555", bmp);
            }

            using (var bmp = new Bitmap(20, 15))
            {
                using (var g = Graphics.FromImage(bmp))
                {
                    g.PixelOffsetMode = PixelOffsetMode.Half;
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.DrawRoundedRectangle(Pens.Blue, Rectangle.Round(g.VisibleClipBounds), 5, 0, 0, 0);
                }

                SaveImage("5000", bmp);
            }

            using (var bmp = new Bitmap(20, 15))
            {
                using (var g = Graphics.FromImage(bmp))
                {
                    g.PixelOffsetMode = PixelOffsetMode.Half;
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.DrawRoundedRectangle(Pens.Blue, Rectangle.Round(g.VisibleClipBounds), 0, 5, 0, 0);
                }

                SaveImage("0500", bmp);
            }

            using (var bmp = new Bitmap(20, 15))
            {
                using (var g = Graphics.FromImage(bmp))
                {
                    g.PixelOffsetMode = PixelOffsetMode.Half;
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.DrawRoundedRectangle(Pens.Blue, Rectangle.Round(g.VisibleClipBounds), 0, 0, 5, 0);
                }

                SaveImage("0050", bmp);
            }

            using (var bmp = new Bitmap(20, 15))
            {
                using (var g = Graphics.FromImage(bmp))
                {
                    g.PixelOffsetMode = PixelOffsetMode.Half;
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.DrawRoundedRectangle(Pens.Blue, Rectangle.Round(g.VisibleClipBounds), 0, 0, 0, 5);
                }

                SaveImage("0005", bmp);
            }

            using (var bmp = new Bitmap(20, 15))
            {
                using (var g = Graphics.FromImage(bmp))
                {
                    g.PixelOffsetMode = PixelOffsetMode.Half;
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.DrawRoundedRectangle(Pens.Blue, Rectangle.Round(g.VisibleClipBounds), 2, 4, 6, 8);
                }

                SaveImage("2468", bmp);
            }
        }

        [Test]
        public void FillRoundedRectangleTest()
        {
            using (var bmp = new Bitmap(20, 15))
            {
                using (var g = Graphics.FromImage(bmp))
                {
                    g.PixelOffsetMode = PixelOffsetMode.Half;
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.FillRoundedRectangle(Brushes.Blue, Rectangle.Round(g.VisibleClipBounds), 5);
                }

                SaveImage("5555", bmp);
            }

            using (var bmp = new Bitmap(20, 15))
            {
                using (var g = Graphics.FromImage(bmp))
                {
                    g.PixelOffsetMode = PixelOffsetMode.Half;
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.FillRoundedRectangle(Brushes.Blue, Rectangle.Round(g.VisibleClipBounds), 5, 0, 0, 0);
                }

                SaveImage("5000", bmp);
            }

            using (var bmp = new Bitmap(20, 15))
            {
                using (var g = Graphics.FromImage(bmp))
                {
                    g.PixelOffsetMode = PixelOffsetMode.Half;
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.FillRoundedRectangle(Brushes.Blue, Rectangle.Round(g.VisibleClipBounds), 0, 5, 0, 0);
                }

                SaveImage("0500", bmp);
            }

            using (var bmp = new Bitmap(20, 15))
            {
                using (var g = Graphics.FromImage(bmp))
                {
                    g.PixelOffsetMode = PixelOffsetMode.Half;
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.FillRoundedRectangle(Brushes.Blue, Rectangle.Round(g.VisibleClipBounds), 0, 0, 5, 0);
                }

                SaveImage("0050", bmp);
            }

            using (var bmp = new Bitmap(20, 15))
            {
                using (var g = Graphics.FromImage(bmp))
                {
                    g.PixelOffsetMode = PixelOffsetMode.Half;
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.FillRoundedRectangle(Brushes.Blue, Rectangle.Round(g.VisibleClipBounds), 0, 0, 0, 5);
                }

                SaveImage("0005", bmp);
            }

            using (var bmp = new Bitmap(20, 15))
            {
                using (var g = Graphics.FromImage(bmp))
                {
                    g.PixelOffsetMode = PixelOffsetMode.Half;
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.FillRoundedRectangle(Brushes.Blue, Rectangle.Round(g.VisibleClipBounds), 2, 4, 6, 8);
                }

                SaveImage("2468", bmp);
            }
        }

        [Test]
        public void ToBitmapTest()
        {
            AssertPlatformDependent(() =>
            {
                // from bitmap
                using var refBmp = Icons.Information.ExtractBitmap(new Size(256, 256));
                using (var g = Graphics.FromImage(refBmp))
                {
                    var bmp = g.ToBitmap(false);
                    SaveImage("FromBitmap full", bmp);

                    g.IntersectClip(new Rectangle(32, 32, 192, 192));
                    bmp = g.ToBitmap(true);
                    SaveImage("FromBitmap clipped", bmp);
                }

                // from screen
                using (var g = Graphics.FromHwnd(IntPtr.Zero))
                {
                    var bmp = g.ToBitmap(false);
                    SaveImage("FromFullScreen", bmp);

                    g.IntersectClip(new Rectangle(100, 100, 100, 50));
                    bmp = g.ToBitmap(true);
                    SaveImage("FromFullScreenWithClip", bmp);
                }
            }, PlatformID.Win32NT);
        }

        #endregion
    }
}