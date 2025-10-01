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

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;

using KGySoft.Drawing.Imaging;
using KGySoft.Drawing.Shapes;

using NUnit.Framework;

using Path = KGySoft.Drawing.Shapes.Path;

#endregion

#nullable enable

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

            internal Builder StartFigure()
            {
                path.StartFigure();
                return this;
            }

            internal Builder AddLine(PointF p1, PointF p2)
            {
                path.AddLine(p1, p2);
                return this;
            }

            internal Builder AddLines(params PointF[] points)
            {
                path.AddLines(points);
                return this;
            }

            internal Builder AddBezier(PointF p1, PointF p2, PointF p3, PointF p4)
            {
                path.AddBezier(p1, p2, p3, p4);
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

            internal Builder AddString(string s, Font font, PointF origin)
            {
                path.AddString(s, font.FontFamily, (int)font.Style, font.SizeInPoints * 96f / 72f, origin, null);
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
            ["Single point", new Builder().AddLines(new PointF(0, 0)).Path],
            ["Single point closed", new Builder().AddLines(new PointF(0, 0)).CloseFigure().Path],
            ["Two points", new Builder().AddLines(new PointF(0, 0)).StartFigure().AddLines(new Point(10, 10)).Path],
            ["Single line", new Builder().AddLines(new(0, 0), new(10, 10)).Path],
            ["Bezier", new Builder().AddBezier(new PointF(0, 100), new PointF(50, 100), new PointF(50, 0), new PointF(100, 0)).Path],
            ["Point-bezier", new Builder().AddLines([new (50, 50)]).AddBezier(new PointF(0, 100), new PointF(0, 50), new PointF(50, 0), new PointF(100, 0)).Path],
            ["Bezier-point", new Builder().AddBezier(new PointF(0, 100), new PointF(100, 50), new PointF(0, 0), new PointF(50, 100)).AddLines([new (50, 50)]).Path],
            ["Small arc", new Builder().AddArc(new Rectangle(0, 0, 100, 100), 90, 90).Path],
            ["Large arc", new Builder().AddArc(new Rectangle(0, 0, 100, 100), 90, 300).Path],
            ["Point-arc", new Builder().AddLines(new Point(50, 50)).AddArc(new Rectangle(0, 0, 100, 100), 90, 90).Path],
            ["Line-arc", new Builder().AddLine(new (50, 50), new (50, 100)).AddArc(new Rectangle(0, 0, 100, 100), 90, 90).Path],
            ["Arc-point", new Builder().AddArc(new Rectangle(0, 0, 100, 100), 90, 90).AddLines([new (50, 50)]).Path],
            ["Arcs with implicit line", new Builder().AddArc(new Rectangle(50, 0, 100, 100), -90, 180).AddArc(new Rectangle(0, 0, 100, 100), 90, 180).Path],
            ["Point-horizontal arc-point", new Builder().AddLines([new (50, 0)]).AddArc(new (0, 50, 100, 20), 45, 300).AddLines([new(50, 100)]).Path],
            ["Point-horizontal flat arc-point", new Builder().AddLines([new(50, 0)]).AddArc(new (0, 50, 100, 1e-6f), 45, 300).AddLines([new(50, 100)]).Path],
            ["Point-vertical arc-point", new Builder().AddLines([new(0, 50)]).AddArc(new (50, 0, 20, 100), 45, 300).AddLines([new(100, 50)]).Path],
            ["Point-vertical flat arc-point", new Builder().AddLines([new(0, 50)]).AddArc(new(50, 0, 1e-6f, 100), 45, 300).AddLines([new(100, 50)]).Path],
            ["Point-horizontal ellipse 0-point", new Builder().AddLines([new(0, 50)]).AddArc(new (0, 50, 100, 20), 0, 360).AddLines([new(100, 50)]).Path],
            ["Point-horizontal ellipse 90-point", new Builder().AddLines([new(0, 50)]).AddArc(new (0, 50, 100, 20), 90, 360).AddLines([new(100, 50)]).Path],
            ["Open-close", new Builder().AddLines(new(50, 0), new(79, 90), new(2, 35), new(97, 35), new(21, 90)).AddEllipse(new Rectangle(0, 0, 100, 100)).Path],
            ["Closed figures", new Builder().AddPolygon(new(50, 0), new(79, 90), new(2, 35), new(97, 35), new(21, 90)).AddEllipse(new Rectangle(0, 0, 100, 100)).AddRoundedRectangle(new Rectangle(0, 0, 100, 100), 10).Path],
            ["Text", new Builder().AddString("Hello World", SystemFonts.MessageBoxFont!, PointF.Empty).Path],
        ];

        private static object[][] PathToGraphicsPathTestSource =>
        [
            ["Empty", new Path()],
            ["Single point", new Path().AddPoint(new(0, 0))],
            ["Single point bezier", new Path().AddBeziers(new PointF(0, 0))],
            ["Single point arc", new Path().AddArc(new Rectangle(0, 0, 100, 100), 0, 0)],
            ["Single line", new Path().AddLines(new(0, 0), new(10, 10))],
            ["Single bezier arc", new Path().AddArc(new Rectangle(0, 0, 100, 100), 90, 90)],
            ["Multi bezier arc", new Path().AddArc(new Rectangle(0, 0, 100, 100), 90, 300)],
            ["Point-bezier", new Path().AddLines(new Point(50, 50)).AddArc(new Rectangle(0, 0, 100, 100), 90, 90)],
            ["Bezier-point", new Path().AddArc(new Rectangle(0, 0, 100, 100), 90, 90).AddLines(new Point(50, 50))],
            ["Point-arc-point", new Path().AddPoint(50, 0).AddArc(0, 50, 100, 20, 45, 90).AddPoint(new (50, 100))],
            ["Point-almost flat arc-point", new Path().AddPoint(50, 0).AddArc(0, 50, 100, 1e-6f, 45, 90).AddPoint(new (50, 100))],
            ["Point-flat arc-point", new Path().AddPoint(50, 0).AddArc(0, 50, 100, 0, 45, 90).AddPoint(new (50, 100))],
            ["Ellipse", new Path().AddEllipse(0, 0, 100, 50)],
            ["Rotated ellipse", new Path().TransformRotation(45f).AddEllipse(0, 0, 100, 50)],
            ["Point-ellipse-point", new Path().AddPoint(50, 0).AddArc(0, 50, 100, 20, 45, 360).AddPoint(new (50, 100))],
            ["Point-almost flat ellipse-point", new Path().AddPoint(50, 0).AddArc(0, 50, 100, 1e-6f, 45, 360).AddPoint(new (50, 100))],
            ["Point-flat ellipse-point", new Path().AddPoint(50, 0).AddArc(0, 50, 100, 0, 45, 360).AddPoint(new (50, 100))],
            ["Open-close", new Path().AddLines(new(50, 0), new(79, 90), new(2, 35), new(97, 35), new(21, 90)).AddEllipse(new Rectangle(0, 0, 100, 100))],
            ["Closed figures", new Path().AddPolygon(new(50, 0), new(79, 90), new(2, 35), new(97, 35), new(21, 90)).AddEllipse(new Rectangle(0, 0, 100, 100)).AddRoundedRectangle(new Rectangle(0, 0, 100, 100), 10)],
        ];

        #endregion

        #region Methods

        [TestCaseSource(nameof(GraphicsPathToPathTestSource))]
        public void GraphicsPathToPathTest(string name, GraphicsPath graphicsPath)
        {
            var bounds = graphicsPath.GetBounds();
            using var bmpRef = new Bitmap((int)bounds.Width + 4, (int)bounds.Height + 4);
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
            using (var bmpRefData = bmpRef.GetReadWriteBitmapData())
            {
                bmpRefData.Clear(Color.Cyan);
                bmpRefData.DrawPath(Color.Blue, path, new DrawingOptions { AntiAliasing = true, DrawPathPixelOffset = PixelOffset.Half, Transformation = TransformationMatrix.CreateTranslation(-bounds.Left + 1, -bounds.Top + 1) });
            }

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

        [Explicit]
        [TestCase(true)]
        [TestCase(false)]
        public void FlatArcsTest(bool antiAliasing)
        {
            Stack<float> diameters = new([1e-6f, 0.1f, 0.5f, 1f, 2f, 5f, 10f, 20f, 50f, 100f]);
            var pen = Pens.Blue;
            using var bmp = new Bitmap(250, 550);
            IReadableBitmapData? bitmapData = null;
            using var ms = new MemoryStream();
            GifEncoder.EncodeAnimation(new AnimatedGifConfiguration(GetNextFrame, () => TimeSpan.FromMilliseconds(500))
            {
                //AnimationMode = AnimationMode.PingPong
            }, ms);
            SaveStream($"{(antiAliasing ? "AA" : "NA")}", ms);

            [SuppressMessage("ReSharper", "AccessToDisposedClosure", Justification = "False alarm, not used after disposed")]
            IReadableBitmapData? GetNextFrame()
            {
                bitmapData?.Dispose();
                if (diameters.Count == 0)
                    return null;
                float sweepAngle = 90f;
                float diameter = diameters.Pop();
                float radius = diameter / 2f;
                using (var g = Graphics.FromImage(bmp))
                {
                    g.SmoothingMode = antiAliasing ? SmoothingMode.AntiAlias : SmoothingMode.None;
                    g.Clear(Color.Cyan);
                    //var path = new Builder()
                    g.TranslateTransform(13, 13);
                    //.AddPoint(50, 0)
                    g.DrawArc(pen, new RectangleF(0, 50 - radius, 100, diameter), -45, sweepAngle);
                    //.AddPoint(50, 100)
                    g.ResetTransform();
                    g.TranslateTransform(125 + 13, 13);
                    //.AddPoint(0, 50)
                    g.DrawArc(pen, new RectangleF(50 - radius, 0, diameter, 100), -45, sweepAngle);
                    //.AddPoint(100, 50)
                    g.ResetTransform();
                    g.TranslateTransform(13, 125 + 13);
                    //.AddPoint(50, 0)
                    g.DrawArc(pen, new RectangleF(0, 50 - radius, 100, diameter), 0, sweepAngle);
                    //.AddPoint(50, 100)
                    g.ResetTransform();
                    g.TranslateTransform(125 + 13, 125 + 13);
                    //.AddPoint(0, 50)
                    g.DrawArc(pen, new RectangleF(50 - radius, 0, diameter, 100), 0, sweepAngle);
                    //.AddPoint(100, 50)
                    g.ResetTransform();
                    g.TranslateTransform(13, 250 + 13);
                    //.AddPoint(50, 0)
                    g.DrawArc(pen, new RectangleF(0, 50 - radius, 100, diameter), 45, sweepAngle);
                    //.AddPoint(50, 100)
                    g.ResetTransform();
                    g.TranslateTransform(125 + 13, 250 + 13);
                    //.AddPoint(0, 50)
                    g.DrawArc(pen, new RectangleF(50 - radius, 0, diameter, 100), 45, sweepAngle);
                    //.AddPoint(100, 50)
                    g.ResetTransform();
                    g.TranslateTransform(13, 375 + 13);
                    //.AddPoint(50, 0)
                    g.DrawArc(pen, new RectangleF(0, 50 - radius, 100, diameter), 90, sweepAngle);
                    //.AddPoint(50, 100)
                    g.ResetTransform();
                    g.TranslateTransform(125 + 13, 375 + 13);
                    //.AddPoint(0, 50)
                    g.DrawArc(pen, new RectangleF(50 - radius, 0, diameter, 100), 90, sweepAngle)
                        //.AddPoint(100, 50)
                        ;
                }

                return bitmapData = bmp.GetReadableBitmapData();
            }
        }


        #endregion
    }
}
