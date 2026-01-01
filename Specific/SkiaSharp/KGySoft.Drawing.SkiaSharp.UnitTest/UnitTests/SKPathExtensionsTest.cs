#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: SKPathExtensionsTest.cs
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

using System.Drawing;

using KGySoft.Drawing.Imaging;
using KGySoft.Drawing.Shapes;

using NUnit.Framework;

using SkiaSharp;

#endregion

namespace KGySoft.Drawing.SkiaSharp.UnitTests
{
    [TestFixture]
    public class SKPathExtensionsTest : TestBase
    {
        #region Nested classes

        private sealed class Builder
        {
            #region Fields

            private readonly SKPath path = new SKPath { FillType = SKPathFillType.EvenOdd };

            #endregion

            #region Properties

            internal SKPath Path => path;

            #endregion

            #region Constructors

            internal Builder() { }

            internal Builder(SKPoint startPoint) => MoveTo(startPoint);

            #endregion

            #region Methods

            internal Builder Close()
            {
                path.Close();
                return this;
            }

            internal Builder MoveTo(SKPoint p)
            {
                path.MoveTo(p);
                return this;
            }

            internal Builder LineTo(SKPoint p)
            {
                path.LineTo(p);
                return this;
            }

            internal Builder AddLines(params SKPoint[] points)
            {
                foreach (SKPoint p in points)
                    path.LineTo(p);
                return this;
            }

            internal Builder AddBezier(SKPoint cp1, SKPoint cp2, SKPoint end)
            {
                path.CubicTo(cp1, cp2, end);
                return this;
            }

            internal Builder AddQuad(SKPoint cp, SKPoint end)
            {
                path.QuadTo(cp, end);
                return this;
            }

            internal Builder AddConic(SKPoint cp, SKPoint end, float weight)
            {
                path.ConicTo(cp, end, weight);
                return this;
            }

            internal Builder AddArc(SKRect bounds, float startAngle, float sweepAngle)
            {
                path.ArcTo(bounds, startAngle, sweepAngle, false);
                return this;
            }

            internal Builder AddEllipse(SKRect bounds)
            {
                path.AddOval(bounds);
                return this;
            }

            internal Builder AddPolygon(params SKPoint[] points)
            {
                path.AddPoly(points);
                return this;
            }

            internal Builder AddRoundedRectangle(SKRect bounds, float cornerRadius)
            {
                path.AddRoundRect(bounds, cornerRadius, cornerRadius);
                return this;
            }

            internal Builder AddString(string s, SKFont font)
            {
                using (font)
                {
                    using var textPath = font.GetTextPath(s);
                    path.AddPath(textPath);
                }

                return this;
            }

            #endregion
        }

        #endregion

        #region Properties

        private static object[][] SKPathToPathTestSource => new object[][]
        {
            ["Empty", new SKPath()],
            ["Single point", new Builder(new SKPoint(0, 0)).Path],
            ["Single point closed", new Builder(new(1, 1)).Close().Path],
            ["Two points", new Builder(new(0, 0)).MoveTo(new(10, 10)).Path],
            ["Single line", new Builder().LineTo(new(10, 10)).Path],
            ["Lines", new Builder(new SKPoint(0, 0)).LineTo(new(10, 10)).AddLines(new(0, 10), new (10, 0)).Path],
            ["Polyline", new Builder(new SKPoint(50, 0)).AddLines(new(79, 90), new(2, 35), new(97, 35), new(21, 90)).Path],
            ["Cubic Bezier", new Builder(new(0, 100)).AddBezier(new(50, 100), new(50, 0), new(100, 0)).Path],
            ["Quadratic Bezier", new Builder(new(0, 100)).AddQuad(new(0, 50), new(100, 100)).Path],
            ["Conic curve w=1", new Builder(new(0, 100)).AddConic(new(50, 0), new(100, 100), 1f).Path],
            ["Conic curve w=0.5", new Builder(new(0, 100)).AddConic(new(50, 0), new(100, 100), 0.5f).Path],
            ["Conic curve w=2", new Builder(new(0, 100)).AddConic(new(50, 0), new(100, 100), 2f).Path],
            ["Small arc", new Builder().AddArc(new(0, 0, 100, 100), 90, 90).Path],
            ["Large arc", new Builder().AddArc(new (0, 0, 100, 100), 90, 300).Path],
            ["Arcs with implicit line", new Builder().AddArc(new (50, 0, 150, 100), -90, 180).AddArc(new (0, 0, 100, 100), 90, 180).Path],
            ["Open-close", new Builder(new(50, 0)).AddLines(new(79, 90), new(2, 35), new(97, 35), new(21, 90)).AddEllipse(new (0, 0, 100, 100)).Path],
            ["Closed figures", new Builder().AddPolygon(new(50, 0), new(79, 90), new(2, 35), new(97, 35), new(21, 90)).AddEllipse(new (0, 0, 100, 100)).AddRoundedRectangle(new (0, 0, 100, 100), 10).Path],
            ["Text", new Builder().AddString("Hello World", new SKFont(SKTypeface.Default)).Path],
        };

        private static object[][] PathToSKPathTestSource => new object[][]
        {
            ["Empty", new Path()],
            ["Single point", new Path().AddPoint(new(0, 0))],
            ["Single point bezier", new Path().AddBeziers([new(0, 0)])],
            ["Single point arc", new Path().AddArc(new Rectangle(0, 0, 100, 100), 0, 0)],
            ["Single line", new Path().AddLines(new(0, 0), new(10, 10))],
            ["Single bezier arc", new Path().AddArc(new Rectangle(0, 0, 100, 100), 90, 90)],
            ["Multi bezier arc", new Path().AddArc(new Rectangle(0, 0, 100, 100), 90, 300)],
            ["Point-bezier", new Path().AddLines(new Point(50, 50)).AddArc(new Rectangle(0, 0, 100, 100), 90, 90)],
            ["Bezier-point", new Path().AddArc(new Rectangle(0, 0, 100, 100), 90, 90).AddLines(new Point(50, 50))],
            ["Point-arc-point", new Path().AddPoint(50, 0).AddArc(0, 50, 100, 20, 45, 90).AddPoint(new(50, 100))],
            ["Point-almost flat arc-point", new Path().AddPoint(50, 0).AddArc(0, 50, 100, 1e-6f, 45, 90).AddPoint(new(50, 100))],
            ["Point-flat arc-point", new Path().AddPoint(50, 0).AddArc(0, 50, 100, 0, 45, 90).AddPoint(new(50, 100))],
            ["Ellipse", new Path().AddEllipse(0, 0, 100, 50)],
            ["Rotated ellipse", new Path().TransformRotation(45f).AddEllipse(0, 0, 100, 50)],
            ["Point-ellipse-point", new Path().AddPoint(50, 0).AddArc(0, 50, 100, 20, 45, 360).AddPoint(new(50, 100))],
            ["Point-circle-point", new Path().AddPoint(50, 0).AddArc(0, 0, 100, 100, 45, 360f).AddPoint(new(50, 100))],
            ["Point-almost flat ellipse-point", new Path().AddPoint(50, 0).AddArc(0, 50, 100, 1e-6f, 45, 360).AddPoint(new(50, 100))],
            ["Point-flat ellipse-point", new Path().AddPoint(50, 0).AddArc(0, 50, 100, 0, 45, 360).AddPoint(new(50, 100))],
            ["Open-close", new Path().AddLines(new(50, 0), new(79, 90), new(2, 35), new(97, 35), new(21, 90)).AddEllipse(new Rectangle(0, 0, 100, 100))],
            ["Closed figures", new Path().AddPolygon(new(50, 0), new(79, 90), new(2, 35), new(97, 35), new(21, 90)).AddEllipse(new Rectangle(0, 0, 100, 100)).AddRoundedRectangle(new Rectangle(0, 0, 100, 100), 10)],
        };

        #endregion

        #region Methods

        [TestCaseSource(nameof(SKPathToPathTestSource))]
        public void SKPathToPathTest(string name, SKPath skiaPath)
        {
            using (skiaPath)
            {
                var bounds = skiaPath.Bounds;
                if (bounds.IsEmpty)
                    bounds = new SKRect(0, 0, 1, 1);

                var bmpRef = new SKBitmap((int)bounds.Width + 4, (int)bounds.Height + 4);
                using (var canvas = new SKCanvas(bmpRef))
                {
                    canvas.Translate(-bounds.Left + 1, -bounds.Top + 1);
                    using var paint = new SKPaint
                    {
                        Color = SKColors.Yellow,
                        IsAntialias = true,
                    };
                    canvas.Clear(SKColors.Cyan);
                    canvas.DrawPath(skiaPath, paint);
                    paint.IsStroke = true;
                    paint.Color = SKColors.Blue;
                    canvas.DrawPath(skiaPath, paint);
                }

                SaveBitmap($"{name}_orig", bmpRef);

                Path path = skiaPath.ToPath();
                var bmp = new SKBitmap(bmpRef.Width, bmpRef.Height);
                using (var bmpData = bmp.GetReadWriteBitmapData())
                {
                    bmpData.Clear(Color.Cyan);
                    var transform = TransformationMatrix.CreateTranslation(-(int)bounds.Left + 1, -(int)bounds.Top + 1);
                    var options = new DrawingOptions
                    {
                        AntiAliasing = true,
                        Transformation = transform,
                    };
                    bmpData.FillPath(Color.Yellow, path, options);
                    bmpData.DrawPath(Color.Blue, path, options);
                }

                // The equality is not pixel perfect so it should be compared visually
                SaveBitmap($"{name}_converted", bmp);
            }
        }

        [TestCaseSource(nameof(PathToSKPathTestSource))]
        public void PathToSKPathTest(string name, Path path)
        {
            var bounds = path.Bounds;
            using var bmpRef = new SKBitmap(bounds.Width + 3, bounds.Height + 3);
            using (var bmpRefData = bmpRef.GetReadWriteBitmapData())
            {
                var options = new DrawingOptions
                {
                    AntiAliasing = true,
                    DrawPathPixelOffset = PixelOffset.Half,
                    Transformation = TransformationMatrix.CreateTranslation(-bounds.Left + 1, -bounds.Top + 1)
                };
                bmpRefData.Clear(Color.Cyan);
                bmpRefData.FillPath(Color.Yellow, path, options);
                bmpRefData.DrawPath(Color.Blue, path, options);
            }

            SaveBitmap($"{name}_orig", bmpRef);

            using var bmp = new SKBitmap(bmpRef.Width, bmpRef.Height);
            using (var skiaPath = path.ToSKPath())
            using (var canvas = new SKCanvas(bmp))
            {
                canvas.Clear(SKColors.Cyan);
                canvas.Translate(-bounds.Left + 1, -bounds.Top + 1);
                using var paint = new SKPaint
                {
                    Color = SKColors.Yellow,
                    IsAntialias = true,
                };
                skiaPath.FillType = SKPathFillType.EvenOdd;
                canvas.DrawPath(skiaPath, paint);
                paint.IsStroke = true;
                paint.Color = SKColors.Blue;
                canvas.DrawPath(skiaPath, paint);
            }

            SaveBitmap($"{name}_converted", bmp);

            // The equality may not be pixel perfect so it should be compared visually
            //using var bmpData = bmp.GetReadableBitmapData();
            //AssertAreEqual(bmpRefData, bmpData);
        }

        #endregion
    }
}
