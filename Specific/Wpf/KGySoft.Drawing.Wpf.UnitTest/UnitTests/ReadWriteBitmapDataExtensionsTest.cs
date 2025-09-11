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

using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using KGySoft.Drawing.Imaging;
using KGySoft.Drawing.Shapes;

using NUnit.Framework;

#endregion

#region Used Aliases

using Brushes = System.Windows.Media.Brushes;

#endregion

#endregion


namespace KGySoft.Drawing.Wpf.UnitTests
{
    [TestFixture]
    public class ReadWriteBitmapDataExtensionsTest : TestBase
    {
        #region Methods

        [Test]
        public void DrawFormattedText()
        {
            var text = new FormattedText("Hello FormattedText", CultureInfo.InvariantCulture, FlowDirection.LeftToRight, new Typeface("Arial"), 20, Brushes.Black);
            WriteableBitmap bitmap = new WriteableBitmap(200, 50, 96, 96, PixelFormats.Bgr101010, null);

            using var bitmapData = bitmap.GetReadWriteBitmapData();
            bitmapData.Clear(Colors.Cyan.ToColor32());
            bitmapData.DrawText(Colors.Blue.ToColor32(), text, PointF.Empty, new DrawingOptions { AntiAliasing = true });
        }

        [Test]
        public void DrawGlyphRun() => ExecuteTestWithDispatcher(finished =>
        {
            var text = new Glyphs
            {
                UnicodeString = "Hello GlyphRun",
                FontUri = new Uri(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "Arial.ttf")),
                FontRenderingEmSize = 20,
                OriginY = 20
            }.ToGlyphRun();
            WriteableBitmap bitmap = new WriteableBitmap(200, 50, 96, 96, PixelFormats.Rgb24, null);

            using var bitmapData = bitmap.GetReadWriteBitmapData();
            bitmapData.Clear(Colors.Cyan.ToColor32());
            bitmapData.DrawText(Colors.Blue.ToColor32(), text, PointF.Empty, new DrawingOptions { AntiAliasing = true });
            finished.Set();
        });

#if NETFRAMEWORK && NET45_OR_GREATER || NETCOREAPP
        [Test]
        [SuppressMessage("ReSharper", "AsyncVoidLambda", Justification = "False alarm, the finished parameter signals completion")]
        public void DrawFormattedTextOutlineAsync() => ExecuteTestWithDispatcher(async finished =>
        {
            var text = new FormattedText("Hello WPF async", CultureInfo.InvariantCulture, FlowDirection.LeftToRight, new Typeface("Arial"), 26, Brushes.Black);
            WriteableBitmap bitmap = new WriteableBitmap(200, 50, 96, 96, PixelFormats.Bgr32, null);

            using (var bitmapData = bitmap.GetReadWriteBitmapData())
            {
                await bitmapData.ClearAsync(Colors.Cyan.ToColor32());
                await bitmapData.DrawTextOutlineAsync(Colors.Blue.ToColor32(), text, PointF.Empty, new DrawingOptions { AntiAliasing = true });
            }

            SaveBitmap(null, bitmap);
            finished.Set();
        });
#endif

        #endregion
    }
}
