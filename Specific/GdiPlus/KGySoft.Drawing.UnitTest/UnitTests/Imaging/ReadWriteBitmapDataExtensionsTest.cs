#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ReadWriteBitmapDataExtensionsTest.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2026 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

#region Used Namespaces

using System.Drawing;
#if NETFRAMEWORK && NET45_OR_GREATER || NETCOREAPP
using System.Threading.Tasks;
#endif

using KGySoft.Drawing.Imaging;
using KGySoft.Drawing.Shapes;

using NUnit.Framework;

#endregion

#region Used Aliases

using Brush = KGySoft.Drawing.Shapes.Brush;
using Pen = KGySoft.Drawing.Shapes.Pen;

#endregion

#endregion

namespace KGySoft.Drawing.UnitTests.Imaging
{
    [TestFixture]
    public class ReadWriteBitmapDataExtensionsTest : TestBase
    {
        #region Fields

        private static readonly DrawingOptions antiAliasingOptions = new DrawingOptions { AntiAliasing = true };

        #endregion

        #region Methods

        [Test]
        public void DrawTextTest()
        {
            using var bitmap = new Bitmap(200, 100);
            using (var bitmapData = bitmap.GetReadWriteBitmapData())
            {
                bitmapData.Clear(Color.Cyan);
                bitmapData.DrawText(Color.Blue, "Hello single line GDI+ text writing", SystemFonts.CaptionFont!, PointF.Empty, null, antiAliasingOptions);

                using var format = new StringFormat { Alignment = StringAlignment.Center };
                var bounds = new Rectangle(0, bitmapData.Height / 2, bitmapData.Width, bitmapData.Height / 2);
                bounds.Inflate(-5, -5);
                bitmapData.DrawText(Color.Blue, "Hello GDI+ text writing with wrapping and center alignment", SystemFonts.MessageBoxFont!, bounds, format, antiAliasingOptions);
            }

            SaveImage(null, bitmap);
        }

        [Test]
        public void DrawTextOutlineTest()
        {
            using var bitmap = new Bitmap(250, 100);
            using (var bitmapData = bitmap.GetReadWriteBitmapData())
            {
                using Font font = new Font("Arial", 45, GraphicsUnit.Pixel);
                bitmapData.Clear(Color.Cyan);
                bitmapData.DrawTextOutline(Color.Blue, "Non-AA outline", font, 1, 1);

                using var format = new StringFormat { Alignment = StringAlignment.Center };
                var bounds = new Rectangle(0, bitmapData.Height / 2, bitmapData.Width, bitmapData.Height / 2);
                bounds.Inflate(-5, -5);
                bitmapData.DrawTextOutline(Color.Blue, "AA outline", font, bounds, format, antiAliasingOptions);
            }

            SaveImage(null, bitmap);
        }

        [Test]
        public void FillTextWithOutlineTest()
        {
            using var bitmap = new Bitmap(200, 100);

            using (var bitmapData = bitmap.GetReadWriteBitmapData())
            {
                using Font font = new Font(SystemFonts.DialogFont.FontFamily, 80, FontStyle.Bold, GraphicsUnit.Pixel);
                using var format = new StringFormat { Alignment = StringAlignment.Center };
                var bounds = new RectangleF(default, bitmapData.Size);
                bitmapData.DrawText(Brush.CreateLinearGradient(new(0, 5), new(5, 0), Color.Cyan, Color.Blue, GradientWrapMode.Mirror), "KGy", font, bounds, format, antiAliasingOptions);
                bitmapData.DrawTextOutline(new Pen(Color.Black, 2f), "KGy", font, bounds, format, antiAliasingOptions);
            }

            SaveImage(null, bitmap);
        }

#if NETFRAMEWORK && NET45_OR_GREATER || NETCOREAPP
        [Test]
        public async Task DrawTextAsyncTest()
        {
            using var bitmap = new Bitmap(200, 100);
            using (var bitmapData = bitmap.GetReadWriteBitmapData())
            {
                await bitmapData.ClearAsync(Color.Cyan);
                await bitmapData.DrawTextAsync(Color.Blue, "Hello single line GDI+ text writing", SystemFonts.CaptionFont!, PointF.Empty, null, antiAliasingOptions);

                using var format = new StringFormat { Alignment = StringAlignment.Center };
                var bounds = new Rectangle(0, bitmapData.Height / 2, bitmapData.Width, bitmapData.Height / 2);
                bounds.Inflate(-5, -5);
                await bitmapData.DrawTextAsync(Color.Blue, "Hello GDI+ text writing with wrapping and center alignment", SystemFonts.MessageBoxFont!, bounds, format, antiAliasingOptions);
            }

            SaveImage(null, bitmap);
        }
#endif

        [Explicit]
        [TestCase(WorkingColorSpace.Srgb, 0xFFFFFF, 0x000000)]
        [TestCase(WorkingColorSpace.Linear, 0xFFFFFF, 0x000000)]
        [TestCase(WorkingColorSpace.Srgb, 0x000000, 0xFFFFFF)]
        [TestCase(WorkingColorSpace.Linear, 0x000000, 0xFFFFFF)]
        [TestCase(WorkingColorSpace.Srgb, 0x00FFFF, 0x0000FF)]
        [TestCase(WorkingColorSpace.Linear, 0x00FFFF, 0x0000FF)]
        [TestCase(WorkingColorSpace.Srgb, 0x0000FF, 0x00FFFF)]
        [TestCase(WorkingColorSpace.Linear, 0x0000FF, 0x00FFFF)]
        [TestCase(WorkingColorSpace.Srgb, 0x008000, 0xFF0000)]
        [TestCase(WorkingColorSpace.Linear, 0x008000, 0xFF0000)]
        [TestCase(WorkingColorSpace.Srgb, 0x800000, 0x00FF00)]
        [TestCase(WorkingColorSpace.Linear, 0x800000, 0x00FF00)]
        public void BlendTextTest(WorkingColorSpace workingColorSpace, int backColor, int foreColor)
        {
            using var bitmap = new Bitmap(250, 25);

            using (var bitmapData = bitmap.GetReadWriteBitmapData(workingColorSpace))
            {
                bitmapData.Clear(Color32.FromRgb(backColor));
                using var format = new StringFormat { Alignment = StringAlignment.Center };
                var bounds = new Rectangle(Point.Empty, bitmapData.Size);
                bitmapData.DrawText(Color32.FromRgb(foreColor), $"This is an anti-aliased sample text using a small font. The blending is performed in the {(workingColorSpace is WorkingColorSpace.Linear ? "linear" : "sRGB")} color space.",
                    SystemFonts.MessageBoxFont, bounds, format, antiAliasingOptions);
            }

            SaveImage($"{backColor:X6} {foreColor:X6} {workingColorSpace}", bitmap);
        }

        #endregion
    }
}
