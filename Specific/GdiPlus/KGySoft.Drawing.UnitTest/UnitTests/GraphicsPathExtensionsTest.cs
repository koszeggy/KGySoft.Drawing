#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: GraphicsPathExtensionsTest.cs
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

using System.Drawing;
using System.Drawing.Drawing2D;

using KGySoft.Drawing.Imaging;
using KGySoft.Drawing.Shapes;

using NUnit.Framework;

#endregion

namespace KGySoft.Drawing.UnitTests
{
    [TestFixture]
    public class GraphicsPathExtensionsTest : TestBase
    {
        #region Nested classes

        #region Builder class

        private sealed class Builder
        {
            #region Fields

            private readonly GraphicsPath path = new GraphicsPath();

            #endregion

            #region Properties

            internal GraphicsPath Path => path;

            #endregion

            #region Methods

            internal Builder CloseFigure()
            {
                path.CloseFigure();
                return this;
            }

            internal Builder AddLines(params Point[] points)
            {
                path.AddLines(points);
                return this;
            }

            internal Builder AddArc(RectangleF rect, float startAngle, float sweepAngle)
            {
                path.AddArc(rect, startAngle, sweepAngle);
                return this;
            }

            internal Builder AddEllipse(RectangleF rect)
            {
                path.AddEllipse(rect);
                return this;
            }

            internal Builder AddPolygon(params Point[] points)
            {
                path.AddPolygon(points);
                return this;
            }

            internal Builder AddRoundedRectangle(RectangleF rect, float radius)
            {
                path.AddRoundedRectangle(rect, radius);
                return this;
            }

            internal Builder AddString(string s, Font font, PointF origin, StringFormat format = null)
            {
                path.AddString(s, font.FontFamily, (int)font.Style, font.SizeInPoints * 96f / 72f, origin, format);
                return this;
            }

            #endregion
        }

        #endregion

        #endregion

        #region Properties

        private static object[][] GraphicsPathToPathTestSource =>
        [
            ["Empty", new GraphicsPath()],
            ["Single point", new Builder().AddLines(new Point(0, 0)).Path],
            ["Single point closed", new Builder().AddLines(new Point(0, 0)).CloseFigure().Path],
            ["Single line", new Builder().AddLines(new(0, 0), new(10, 10)).Path],
            ["Single bezier arc", new Builder().AddArc(new Rectangle(0, 0, 100, 100), 90, 90).Path],
            ["Multi bezier arc", new Builder().AddArc(new Rectangle(0, 0, 100, 100), 90, 300).Path],
            ["Point-bezier", new Builder().AddLines(new Point(50, 50)).AddArc(new Rectangle(0, 0, 100, 100), 90, 90).Path],
            ["Bezier-point", new Builder().AddArc(new Rectangle(0, 0, 100, 100), 90, 90).AddLines(new Point(50, 50)).Path],
            ["Open-close", new Builder().AddLines(new(50, 0), new(79, 90), new(2, 35), new(97, 35), new(21, 90)).AddEllipse(new Rectangle(0, 0, 100, 100)).Path],
            ["Closed figures", new Builder().AddPolygon(new(50, 0), new(79, 90), new(2, 35), new(97, 35), new(21, 90)).AddEllipse(new Rectangle(0, 0, 100, 100)).AddRoundedRectangle(new Rectangle(0, 0, 100, 100), 10).Path],
            ["Text", new Builder().AddString("Hello World", SystemFonts.MessageBoxFont, PointF.Empty).Path],
        ];

        private static object[][] PathToGraphicsPathTestSource =>
        [
            ["Single line", new Path().AddLines(new(0, 0), new(10, 10))],
            ["Single bezier arc", new Path().AddArc(new Rectangle(0, 0, 100, 100), 90, 90)],
            ["Multi bezier arc", new Path().AddArc(new Rectangle(0, 0, 100, 100), 90, 300)],
            ["Point-bezier", new Path().AddLines(new Point(50, 50)).AddArc(new Rectangle(0, 0, 100, 100), 90, 90)],
            ["Bezier-point", new Path().AddArc(new Rectangle(0, 0, 100, 100), 90, 90).AddLines(new Point(50, 50))],
            ["Open-close", new Path().AddLines(new(50, 0), new(79, 90), new(2, 35), new(97, 35), new(21, 90)).AddEllipse(new Rectangle(0, 0, 100, 100))],
            ["Closed figures", new Path().AddPolygon(new(50, 0), new(79, 90), new(2, 35), new(97, 35), new(21, 90)).AddEllipse(new Rectangle(0, 0, 100, 100)).AddRoundedRectangle(new Rectangle(0, 0, 100, 100), 10)],
        ];

        #endregion

        #region Methods

        [TestCaseSource(nameof(GraphicsPathToPathTestSource))]
        public void GraphicsPathToPathTest(string name, GraphicsPath graphicsPath)
        {
            var bounds = graphicsPath.GetBounds();
            using var bmpRef = new Bitmap((int)bounds.Width + 2, (int)bounds.Height + 2);
            using (var g = Graphics.FromImage(bmpRef))
            {
                g.Clear(Color.Cyan);
                g.TranslateTransform(-bounds.Left + 1, -bounds.Top + 1);
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.DrawPath(Pens.Blue, graphicsPath);
            }

            SaveImage($"{name}_orig", bmpRef);

            Path path = graphicsPath.ToPath();
            using var bmp = new Bitmap(bmpRef.Width, bmpRef.Height);
            using (var bmpData = bmp.GetReadWriteBitmapData())
            {
                bmpData.Clear(Color.Cyan);
                bmpData.DrawPath(Color.Blue, path, new DrawingOptions { AntiAliasing = true, DrawPathPixelOffset = PixelOffset.Half, Transformation = TransformationMatrix.CreateTranslation(-bounds.Left + 1, -bounds.Top + 1) });
            }

            SaveImage($"{name}_converted", bmp);

            // The equality may not be pixel perfect so it should be compared visually
            //using var refData = bmpRef.GetReadableBitmapData();
            //AssertAreEqual(refData, bmpData);

            graphicsPath.Dispose();
        }

        [TestCaseSource(nameof(PathToGraphicsPathTestSource))]
        public void PathToGraphicsPathTest(string name, Path path)
        {
            var bounds = path.Bounds;
            using var bmpRef = new Bitmap(bounds.Width + 3, bounds.Height + 3);
            using var bmpRefData = bmpRef.GetReadWriteBitmapData();
            bmpRefData.Clear(Color.Cyan);
            bmpRefData.DrawPath(Color.Blue, path, new DrawingOptions { AntiAliasing = true, DrawPathPixelOffset = PixelOffset.Half, Transformation = TransformationMatrix.CreateTranslation(-bounds.Left + 1, -bounds.Top + 1) });

            SaveImage($"{name}_orig", bmpRef);
            using var bmp = new Bitmap(bmpRef.Width, bmpRef.Height);
            using (var graphicsPath = path.ToGraphicsPath())
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Cyan);
                g.TranslateTransform(-bounds.Left + 1, -bounds.Top + 1);
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.DrawPath(Pens.Blue, graphicsPath);
            }

            SaveImage($"{name}_converted", bmp);

            // The equality may not be pixel perfect so it should be compared visually
            //using var bmpData = bmp.GetReadableBitmapData();
            //AssertAreEqual(bmpRefData, bmpData);
        }

        #endregion
    }
}
