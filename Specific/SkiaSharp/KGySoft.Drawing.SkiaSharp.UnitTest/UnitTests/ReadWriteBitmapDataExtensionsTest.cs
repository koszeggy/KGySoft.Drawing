#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ReadWriteBitmapDataExtensionsTest.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2025 - All Rights Reserved
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
using System.Threading.Tasks;

using KGySoft.Drawing.Imaging;
using KGySoft.Drawing.Shapes;

using NUnit.Framework;

using SkiaSharp;

#endregion

#region Used Aliases

using Pen = KGySoft.Drawing.Shapes.Pen;
using Brush = KGySoft.Drawing.Shapes.Brush;

#endregion

#endregion

namespace KGySoft.Drawing.SkiaSharp.UnitTests
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
            using var bitmap = new SKBitmap(100, 25);
            using (var bitmapData = bitmap.GetReadWriteBitmapData())
            {
                using var font = new SKFont(SKTypeface.Default);
                bitmapData.Clear(Color.Cyan);
                bitmapData.DrawText(Color.Blue, "Hello SkiaSharp", font, 10, 10, antiAliasingOptions);
            }

            SaveBitmap(null, bitmap);
        }

        [Test]
        public void DrawTextOutlineTest()
        {
            using var bitmap = new SKBitmap(320, 120);
            using (var bitmapData = bitmap.GetReadWriteBitmapData())
            {
                using var font = new SKFont(SKTypeface.Default, 45);
                bitmapData.Clear(Color.Cyan);
                bitmapData.DrawTextOutline(Color.Blue, "Non-AA outline", font, 5f, 5f);
                bitmapData.DrawTextOutline(Color.Blue, "AA outline", font, 5f, bitmapData.Height / 2f + 5f, antiAliasingOptions);
            }

            SaveBitmap(null, bitmap);
        }

        [Test, Explicit]
        public void TestThinPathWithOffScreenOffsets()
        {
            using var bitmap = new SKBitmap(320, 50);
            using var font = new SKFont(SKTypeface.Default, 45);
            using var bitmapData = bitmap.GetReadWriteBitmapData();
            var pen = new Pen(Color.Blue); // to test DirectDrawer.GenericDrawer<,,>.DrawLine
            //var pen = new Pen(Color32.FromArgb(128, Color.Blue)); // to test DrawIntoRegionSession.DrawLine
            //var pen = new Pen(Brush.CreateTexture(BitmapDataFactory.CreateBitmapData(1, 1, KnownPixelFormat.Format1bppIndexed))); // to test TextureBasedBrush.DrawLine

            // to the top
            for (int y = 0; y < 50; y++)
            {
                bitmapData.Clear(Color.Cyan);
                int offset = y;
                bitmapData.DrawTextOutline(pen, "Non-AA outline", font, new (5f, -offset));
                SaveBitmap($"t{offset}", bitmap);
            }

            // to the left
            for (int x = 0; x < 300; x++)
            {
                bitmapData.Clear(Color.Cyan);
                int offset = x;
                bitmapData.DrawTextOutline(pen, "Non-AA outline", font, new (-offset, 5f));
                SaveBitmap($"l{offset}", bitmap);
            }

            // to the bottom
            for (int y = 0; y < 50; y++)
            {
                bitmapData.Clear(Color.Cyan);
                int offset = y;
                bitmapData.DrawTextOutline(pen, "Non-AA outline", font, new (5f, offset));
                SaveBitmap($"b{offset}", bitmap);
            }

            // to the right
            for (int x = 0; x < 300; x++)
            {
                bitmapData.Clear(Color.Cyan);
                int offset = x;
                bitmapData.DrawTextOutline(pen, "Non-AA outline", font, new(offset, 5f));
                SaveBitmap($"r{offset}", bitmap);
            }
        }

        [Test]
        public void FillTextWithOutlineTest()
        {
            using var bitmap = new SKBitmap(200, 120);

            using (var bitmapData = bitmap.GetReadWriteBitmapData())
            {
                using var typeface = SKTypeface.FromFamilyName(SKTypeface.Default.FamilyName, SKFontStyle.Bold);
                using var font = new SKFont(typeface, 80);
                var offset = new PointF(10, 10);
                bitmapData.DrawText(Brush.CreateLinearGradient(new(0, 5), new(5, 0), Color.Cyan, Color.Blue, GradientWrapMode.Mirror), "KGy", font, offset, antiAliasingOptions);
                bitmapData.DrawTextOutline(new Pen(Color.Black, 2f), "KGy", font, offset, antiAliasingOptions);
            }

            SaveBitmap(null, bitmap);
        }

        [Test]
        public async Task DrawTextAsyncTest()
        {
            using var bitmap = new SKBitmap(100, 25);
            using (var bitmapData = bitmap.GetReadWriteBitmapData())
            {
                using var font = new SKFont(SKTypeface.Default);
                await bitmapData.ClearAsync(Color.Cyan);
                await bitmapData.DrawTextAsync(Color.Blue, "Hello SkiaSharp", font, new PointF(10, 10), antiAliasingOptions);
            }

            SaveBitmap(null, bitmap);
        }

        #endregion
    }
}
