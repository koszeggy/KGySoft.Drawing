#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: GeometryExtensionsTest.cs
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
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using KGySoft.Drawing.Imaging;
using KGySoft.Drawing.Shapes;
using KGySoft.Threading;

using NUnit.Framework;

#endregion

#region Used Aliases

using Color = System.Drawing.Color;
using WpfArcSegment = System.Windows.Media.ArcSegment;
using WpfBezierSegment = System.Windows.Media.BezierSegment;
using WpfLineSegment = System.Windows.Media.LineSegment;
using WpfPen = System.Windows.Media.Pen;
using WpfPoint = System.Windows.Point;

#endregion

#endregion

#region Suppressions

#pragma warning disable CS0618 // Type or member is obsolete - the recommended FormattedText constructor is not available in all target frameworks

#endregion

namespace KGySoft.Drawing.Wpf.UnitTests
{
    [TestFixture]
    public class GeometryExtensionsTest : TestBase
    {
        #region Nested classes

        private sealed class Builder
        {
            #region Fields

            private readonly PathGeometry geometry = new();

            private PathFigure currentFigure = default!;

            #endregion

            #region Properties

            internal Geometry Geometry => geometry;

            #endregion

            #region Constructors

            internal Builder() => StartFigure(default);
            internal Builder(WpfPoint startPoint) => StartFigure(startPoint);

            #endregion

            #region Methods

            internal Builder StartFigure(WpfPoint startPoint)
            {
                geometry.Figures.Add(currentFigure = new PathFigure(startPoint, [], false));
                return this;
            }

            internal Builder CloseFigure()
            {
                currentFigure.IsClosed = true;
                return this;
            }

            internal Builder AddLine(WpfPoint point)
            {
                currentFigure.Segments.Add(new WpfLineSegment(point, true));
                return this;
            }

            internal Builder AddLines(params WpfPoint[] points)
            {
                currentFigure.Segments.Add(new PolyLineSegment(points, true));
                return this;
            }

            internal Builder AddBezier(WpfPoint cp1, WpfPoint cp2, WpfPoint end)
            {
                currentFigure.Segments.Add(new WpfBezierSegment(cp1, cp2, end, true));
                return this;
            }

            internal Builder AddBeziers(params WpfPoint[] points)
            {
                currentFigure.Segments.Add(new PolyBezierSegment(points, true));
                return this;
            }

            internal Builder AddQuadraticBezier(WpfPoint cp, WpfPoint end)
            {
                currentFigure.Segments.Add(new QuadraticBezierSegment(cp, end, true));
                return this;
            }

            internal Builder AddQuadraticBeziers(params WpfPoint[] points)
            {
                currentFigure.Segments.Add(new PolyQuadraticBezierSegment(points, true));
                return this;
            }

            internal Builder AddArc(WpfPoint point, Size size, double angle, bool isLarge, SweepDirection direction)
            {
                currentFigure.Segments.Add(new WpfArcSegment(point, size, angle, isLarge, direction, true));
                return this;
            }

            public Geometry AddString(string s, FontFamily fontFamily, double fontSize, FontStyle fontStyle)
            {
                var text = new FormattedText(s, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface(fontFamily, fontStyle, FontWeights.Normal, FontStretches.Normal), fontSize, Brushes.Black);
                return text.BuildGeometry(default);
            }

            #endregion
        }

        #endregion

        #region Properties

        private static object[][] GeometryToPathTestSource => new object[][]
        {
            ["Empty", () => Geometry.Empty],
            ["Single point", () => new Builder(new WpfPoint(0, 0)).Geometry],
            ["Single line", () => new Builder(new WpfPoint(0, 0)).AddLine(new(10, 10)).Geometry],
            ["Single-point line", () => new Builder(new WpfPoint(10, 10)).AddLine(new(10, 10)).Geometry],
            ["Single-point arc", () => new Builder(new WpfPoint(10, 10)).AddArc(new(10, 10), new(0, 0), 0d, true, SweepDirection.Clockwise).Geometry],
            ["Polyline", () => new Builder(new WpfPoint(50, 0)).AddLines(new(79, 90), new(2, 35), new(97, 35), new(21, 90)).Geometry],
            ["Lines", () => new Builder(new WpfPoint(0, 0)).AddLine(new(10, 10)).AddLines(new(0, 10), new(10, 0)).Geometry],
            ["Bezier", () => new Builder(new(0, 100)).AddBezier(new(50, 100), new(50, 0), new(100, 0)).Geometry],
            ["PolyBezier", () => new Builder(new(10, 100)).AddBeziers(new(0, 0), new(200, 0), new(300, 100), new(300, 0), new(400, 0), new(600, 100)).Geometry],
            ["QuadraticBezier", () => new Builder(new(0, 100)).AddQuadraticBezier(new(0, 50), new(100, 100)).Geometry],
            ["QuadraticPolyBezier", () => new Builder(new(10, 100)).AddQuadraticBeziers(new(200, 200), new(300, 100), new(0, 200), new(30, 400)).Geometry],
            ["Large arc", () => new Builder(new(50, 100)).AddArc(new(100, 100), new(50, 25), 0, true, SweepDirection.Clockwise).Geometry],
            ["Large arc ccw", () => new Builder(new(50, 100)).AddArc(new(100, 100), new(50, 25), 0, true, SweepDirection.Counterclockwise).Geometry],
            ["Small arc", () => new Builder(new(50, 100)).AddArc(new(100, 100), new(50, 25), 0, false, SweepDirection.Clockwise).Geometry],
            ["Non-horizontal arc", () => new Builder(new(50, 100)).AddArc(new(100, 150), new(50, 25), 0, true, SweepDirection.Clockwise).Geometry],
            ["Non-horizontal arc small", () => new Builder(new(50, 100)).AddArc(new(100, 150), new(50, 25), 0, false, SweepDirection.Clockwise).Geometry],
            ["Vertical arc", () => new Builder(new(50, 100)).AddArc(new(100, 100), new(60, 100), 0, true, SweepDirection.Clockwise).Geometry],
            ["Rotated arc", () => new Builder(new(50, 100)).AddArc(new(100, 100), new(50, 25), 45, true, SweepDirection.Clockwise).Geometry],
            ["Closed arc half 1", () => new Builder(new WpfPoint(50, 0)).AddArc(new(50,100), new(50,50), 0, true, SweepDirection.Clockwise).CloseFigure().Geometry],
            ["Closed arc half 2", () => new Builder(new WpfPoint(50, 100)).AddArc(new(50,0), new(50,50), 0, true, SweepDirection.Clockwise).CloseFigure().Geometry],
            ["Closed arc quarter 1", () => new Builder(new WpfPoint(50, 0)).AddArc(new(100,50), new(50,50), 0, false, SweepDirection.Clockwise).CloseFigure().Geometry],
            ["Closed arc quarter 2", () => new Builder(new WpfPoint(100, 50)).AddArc(new(50,100), new(50,50), 0, false, SweepDirection.Clockwise).CloseFigure().Geometry],
            ["Closed arc quarter 3", () => new Builder(new WpfPoint(50, 100)).AddArc(new(0,50), new(50,50), 0, false, SweepDirection.Clockwise).CloseFigure().Geometry],
            ["Closed arc quarter 4", () => new Builder(new WpfPoint(0, 50)).AddArc(new(50,0), new(50,50), 0, false, SweepDirection.Clockwise).CloseFigure().Geometry],
            ["Closed figures", () => new Builder(new WpfPoint(50, 0)).AddLines(new(79, 90), new(2, 35), new(97, 35), new(21, 90)).CloseFigure().StartFigure(new(50, 0)).AddArc(new(50,100), new(50,50), 0, true, SweepDirection.Clockwise).AddArc(new(50, 0), new(50, 50), 0, true, SweepDirection.Clockwise).CloseFigure().Geometry],
            ["Text", () => new Builder().AddString("Hello World", SystemFonts.MessageFontFamily, SystemFonts.MessageFontSize, SystemFonts.MessageFontStyle)],
        };

        private static object[][] PathToGeometryTestSource => new object[][]
        {
            ["Empty", new Path()],
            ["Single point", new Path().AddPoint(new(0, 0))],
            ["Single point bezier", new Path().AddBeziers([new (0, 0)])],
            ["Single point arc", new Path().AddArc(new (0, 0, 100, 100), 0, 0)],
            ["Single line", new Path().AddLine(new(0, 0), new(10, 10))],
            ["Polyline", new Path().AddLines(new(50, 0), new(79, 90), new(2, 35), new(97, 35), new(21, 90))],
            ["Polygon", new Path().AddLines(new(50, 0), new(79, 90), new(2, 35), new(97, 35), new(21, 90)).CloseFigure()],
            ["Single bezier arc", new Path().AddArc(0, 0, 100, 100, 90, 90)],
            ["Multi bezier arc", new Path().AddArc(0, 0, 100, 100, 90, 300)],
            ["Point-bezier", new Path().AddPoint(50, 0).AddArc(0, 0, 100, 100, 90, 90)],
            ["Bezier-point", new Path().AddArc(0, 0, 100, 100, 90, 90).AddPoint(50, 0)],
            ["Point-horizontal small arc-point", new Path().AddPoint(50, 0).AddArc(0, 50, 100, 20, 45, 90).AddPoint(new (50, 100))],
            ["Point-horizontal almost flat small arc-point", new Path().AddPoint(50, 0).AddArc(0, 50, 100, 1e-6f, 45, 90).AddPoint(new (50, 100))],
            ["Point-horizontal flat small arc-point", new Path().AddPoint(50, 0).AddArc(0, 50, 100, 0, 45, 90).AddPoint(new (50, 100))],
            ["Point-vertical small arc-point", new Path().AddPoint(0, 50).AddArc(50, 0, 20, 100, 45, 90).AddPoint(new (100, 50))],
            ["Point-vertical almost flat small arc-point", new Path().AddPoint(0, 50).AddArc(50, 0, 1e-6f, 100, 45, 90).AddPoint(new (100, 50))],
            ["Point-vertical flat small arc-point", new Path().AddPoint(0, 50).AddArc(50, 0, 0, 100, 45, 90).AddPoint(new (100, 50))],
            ["Point-horizontal large arc-point", new Path().AddPoint(50, 0).AddArc(0, 50, 100, 20, 45, 300).AddPoint(new (50, 100))],
            ["Point-horizontal almost flat large arc-point", new Path().AddPoint(50, 0).AddArc(0, 50, 100, 1e-6f, 45, 300).AddPoint(new (50, 100))],
            ["Point-horizontal flat large arc-point", new Path().AddPoint(50, 0).AddArc(0, 50, 100, 0, 45, 300).AddPoint(new (50, 100))],
            ["Point-vertical large arc-point", new Path().AddPoint(0, 50).AddArc(50, 0, 20, 100, 45, 300).AddPoint(new (100, 50))],
            ["Point-vertical almost flat large arc-point", new Path().AddPoint(0, 50).AddArc(50, 0, 1e-6f, 100, 45, 300).AddPoint(new (100, 50))],
            ["Point-vertical flat large arc-point", new Path().AddPoint(0, 50).AddArc(50, 0, 0, 100, 45, 300).AddPoint(new (100, 50))],
            ["Point-horizontal small negative arc-point", new Path().AddPoint(50, 0).AddArc(0, 50, 100, 20, 45, -90).AddPoint(new (50, 100))],
            ["Point-vertical small negative arc-point", new Path().AddPoint(0, 50).AddArc(50, 0, 20, 100, 45, -90).AddPoint(new (100, 50))],
            ["Point-horizontal large negative arc-point", new Path().AddPoint(50, 0).AddArc(0, 50, 100, 20, 45, -300).AddPoint(new (50, 100))],
            ["Point-vertical large negative arc-point", new Path().AddPoint(0, 50).AddArc(50, 0, 20, 100, 45, -300).AddPoint(new (100, 50))],
            ["Ellipse", new Path().AddEllipse(0, 0, 100, 50)],
            ["Rotated ellipse", new Path().TransformRotation(45f).AddEllipse(0, 0, 100, 50)],
            ["Point-horizontal ellipse-point", new Path().AddPoint(50, 0).AddArc(0, 50, 100, 20, 45, 360).AddPoint(new (50, 100))],
            ["Point-horizontal almost flat ellipse-point", new Path().AddPoint(50, 0).AddArc(0, 50, 100, 1e-6f, 45, 360).AddPoint(new (50, 100))],
            ["Point-horizontal flat ellipse-point", new Path().AddPoint(50, 0).AddArc(0, 50, 100, 0, 45, 360).AddPoint(new (50, 100))],
            ["Point-vertical ellipse-point", new Path().AddPoint(0, 50).AddArc(50, 0, 20, 100, 45, 360).AddPoint(new (100, 50))],
            ["Point-vertical almost flat ellipse-point", new Path().AddPoint(0, 50).AddArc(50, 0, 1e-6f, 100, 45, 360).AddPoint(new (100, 50))],
            ["Point-vertical flat ellipse-point", new Path().AddPoint(0, 50).AddArc(50, 0, 0, 100, 45, 360).AddPoint(new (100, 50))],
            ["Open-close", new Path().AddLines(new(50, 0), new(79, 90), new(2, 35), new(97, 35), new(21, 90)).AddEllipse(0, 0, 100, 100)],
            ["Closed figures", new Path().AddPolygon(new(50, 0), new(79, 90), new(2, 35), new(97, 35), new(21, 90)).AddEllipse(0, 0, 100, 100).AddRoundedRectangle(0, 0, 100, 100, 10)],
        };

        #endregion

        #region Methods

        #region Static Methods

        private static BitmapSource ToBitmapSource(ImageSource image, Size? customSize = null)
        {
            var width = Math.Max(1, customSize?.Width ?? image.Width);
            var height = Math.Max(1, customSize?.Height ?? image.Height);
            var visual = new DrawingVisual();
            using (DrawingContext context = visual.RenderOpen())
                context.DrawImage(image, new Rect(0, 0, width, height));
            var bitmap = new RenderTargetBitmap((int)width, (int)height, 96, 96, PixelFormats.Default);
            bitmap.Render(visual);
            return bitmap;
        }

        #endregion

        #region Instance Methods

        [TestCaseSource(nameof(GeometryToPathTestSource))]
        public void GeometryToPathTest(string name, Func<Geometry> geometryFactory)
        {
            // Using a delegate to obtain the geometry in the correct thread
            Geometry geometry = geometryFactory.Invoke();

            var bounds = geometry.Bounds;
            if (bounds.IsEmpty)
                bounds = new Rect(0, 0, 1, 1);
            var brush = new SolidColorBrush(Colors.Yellow);
            var pen = new WpfPen(Brushes.Blue, 1) { StartLineCap = PenLineCap.Square, EndLineCap = PenLineCap.Square }; // cap: to render single-point figures as well
            var drawing = new GeometryDrawing();
            drawing.Geometry = geometry;
            drawing.Brush = brush;
            drawing.Pen = pen;
            DrawingImage vectorImage = new DrawingImage(drawing); // to be able to be observed by KGy SOFT ImageSource Visualizer
            SaveBitmap($"{name}_vector", ToBitmapSource(vectorImage, new Size(bounds.Width * 10, bounds.Height * 10)));

            if (!geometry.IsFrozen)
            {
                geometry.Transform = new TranslateTransform(-bounds.Left + 1, -bounds.Top + 1);
                bounds = geometry.Bounds;
            }

            var visual = new DrawingVisual();
            using (DrawingContext context = visual.RenderOpen())
            {
                context.DrawRectangle(Brushes.Cyan, null, Rect.Inflate(bounds, 4, 4));
                context.DrawGeometry(brush, pen, geometry);
            }

            bounds.Width += 4;
            bounds.Height += 4;
            int width = (int)bounds.Width;
            int height = (int)bounds.Height;
            var bmpRef = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            var matrix = geometry.Transform.Value;
            matrix.Append(new TranslateTransform(-bounds.Left + 1, -bounds.Top + 1).Value);
            if (!geometry.IsFrozen)
                geometry.Transform = new MatrixTransform(matrix);
            bmpRef.Render(visual);
            if (!geometry.IsFrozen)
                geometry.Transform = null;

            SaveBitmap($"{name}_orig", bmpRef);
            bounds = geometry.Bounds;

            Path path = geometry.ToPath();
            var bmp = new WriteableBitmap(bmpRef.PixelWidth, bmpRef.PixelHeight, bmpRef.DpiX, bmpRef.DpiY, bmpRef.Format, null);
            //var bmp = new WriteableBitmap(path.Bounds.Right, path.Bounds.Bottom, bmpRef.DpiX, bmpRef.DpiY, bmpRef.Format, null);
            using (var bmpData = bmp.GetReadWriteBitmapData())
            {
                bmpData.Clear(Color.Cyan);
                var transform = TransformationMatrix.CreateTranslation(-(int)bounds.Left + 1, -(int)bounds.Top + 1);
                var options = new DrawingOptions { AntiAliasing = true, Transformation = transform };
                bmpData.FillPath(Color.Yellow, path, options);
                bmpData.DrawPath(Color.Blue, path, options);
            }

            // The equality is not pixel perfect so it should be compared visually
            //AssertAreEqual(bmpRef, bmp);

            SaveBitmap($"{name}_converted", bmp);
        }

        [TestCaseSource(nameof(PathToGeometryTestSource))]
        public void PathToGeometryTest(string name, Path path)
        {
            var bounds = path.Bounds;
            int width = Math.Max(1, bounds.Width);
            int height = Math.Max(1, bounds.Height);
            var bmpRef = new WriteableBitmap(width + 2, height + 2, 96, 96, PixelFormats.Rgb24, null);
            using (var bitmapData = bmpRef.GetReadWriteBitmapData())
            {
                bitmapData.Clear(Color.Cyan);
                var options = new DrawingOptions
                {
                    AntiAliasing = true,
                    DrawPathPixelOffset = PixelOffset.Half,
                    Transformation = TransformationMatrix.CreateTranslation(-bounds.Left + 1, -bounds.Top + 1)
                };

                bitmapData.FillPath(Color.Yellow, path, options);
                bitmapData.DrawPath(Color.Blue, path, options);
            }

            SaveBitmap($"{name}_orig", bmpRef);

            Geometry geometry = path.ToGeometry();
            var visual = new DrawingVisual();
            //RenderOptions.SetEdgeMode(visual, EdgeMode.Aliased); // does not work, despite the example at https://learn.microsoft.com/en-us/dotnet/api/system.windows.media.renderoptions.setedgemode
            using (DrawingContext context = visual.RenderOpen())
            {
                context.DrawRectangle(Brushes.Cyan, null, Rect.Inflate(new Rect(0, 0, width, height), 2, 2));
                context.DrawGeometry(Brushes.Yellow, new WpfPen(Brushes.Blue, 1), geometry);
            }

            var bmp = new RenderTargetBitmap(width + 2, height + 2, 96, 96, PixelFormats.Default);
            var matrix = geometry.Transform.Value;
            matrix.Append(new TranslateTransform(-bounds.Left + 1, -bounds.Top + 1).Value);
            if (!geometry.IsFrozen)
                geometry.Transform = new MatrixTransform(matrix);

            //var bmp = new RenderTargetBitmap((int)visual.Drawing.Bounds.Right, (int)visual.Drawing.Bounds.Height, 96, 96, PixelFormats.Default);
            bmp.Render(visual);
            SaveBitmap($"{name}_converted", bmp);

            // The equality is not pixel perfect so it should be compared visually
            //AssertAreEqual(bmpRef.ToWriteableBitmap(), bmp);
        }

        #endregion

        #endregion
    }
}
