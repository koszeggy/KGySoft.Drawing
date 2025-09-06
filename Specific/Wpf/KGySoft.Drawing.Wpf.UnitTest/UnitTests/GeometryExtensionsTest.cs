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

using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using KGySoft.Drawing.Imaging;
using KGySoft.Drawing.Shapes;

using NUnit.Framework;

#endregion

#region Used Aliases

using Color = System.Drawing.Color;
using WpfPen = System.Windows.Media.Pen;
using WpfPoint = System.Windows.Point;

#endregion

#endregion

namespace KGySoft.Drawing.Wpf.UnitTests
{
    [TestFixture]
    public class GeometryExtensionsTest : TestBase
    {
        #region Nested classes

        #region Builder class

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
                currentFigure.Segments.Add(new LineSegment(point, true));
                return this;
            }

            internal Builder AddLines(params WpfPoint[] points)
            {
                currentFigure.Segments.Add(new PolyLineSegment(points, true));
                return this;
            }

            internal Builder AddBezier(WpfPoint cp1, WpfPoint cp2, WpfPoint end)
            {
                currentFigure.Segments.Add(new BezierSegment(cp1, cp2, end, true));
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
                currentFigure.Segments.Add(new ArcSegment(point, size, angle, isLarge, direction, true));
                return this;
            }

            #endregion
        }

        #endregion

        #endregion

        #region Properties

        private static object[][] GeometryToPathTestSource => new object[][]
        {
            //["Empty", Geometry.Empty],
            //["Single point", new Builder(new WpfPoint(0, 0)).Geometry],
            //["Single line", new Builder(new WpfPoint(0, 0)).AddLine(new(10, 10)).Geometry],
            //["Polyline", new Builder(new WpfPoint(50, 0)).AddLines(new(79, 90), new(2, 35), new(97, 35), new(21, 90)).Geometry],
            //["Lines", new Builder(new WpfPoint(0, 0)).AddLine(new(10, 10)).AddLines(new(0, 10), new (10, 0)).Geometry],
            //["Bezier", new Builder(new(0, 100)).AddBezier(new(50, 100), new(50, 0), new(100, 0)).Geometry],
            //["PolyBezier", new Builder(new(10, 100)).AddBeziers(new(0, 0), new(200, 0), new(300, 100), new(300, 0), new(400, 0), new(600, 100)).Geometry],
            //["QuadraticBezier", new Builder(new(0, 100)).AddQuadraticBezier(new(0, 50), new(100, 100)).Geometry],
            //["QuadraticPolyBezier", new Builder(new(10, 100)).AddQuadraticBeziers(new(200, 200), new(300, 100), new(0, 200), new(30, 400)).Geometry],
            //["Large arc", new Builder(new(50, 100)).AddArc(new (100, 100), new(50, 25), 0, true, SweepDirection.Clockwise).Geometry],
            //["Small arc", new Builder(new(50, 100)).AddArc(new (100, 100), new(50, 25), 0, false, SweepDirection.Clockwise).Geometry],
            ["Counterclockwise arc - error: closed", new Builder(new(50, 100)).AddArc(new (100, 100), new(50, 25), 0, true, SweepDirection.Counterclockwise).Geometry],
            ["Non-horizontal arc - error: from zero", new Builder(new(50, 100)).AddArc(new (100, 150), new(50, 25), 0, true, SweepDirection.Clockwise).Geometry],
            ["Non-horizontal small arc - error: to zero, small width", new Builder(new(50, 100)).AddArc(new (100, 150), new(50, 25), 0, false, SweepDirection.Clockwise).Geometry],
            //["Rotated arc", new Builder(new(50, 100)).AddArc(new (100, 100), new(50, 25), 45, true, SweepDirection.Clockwise).Geometry],
            ["Closed figures - error: missing half circle", new Builder(new WpfPoint(50, 0)).AddLines(new(79, 90), new(2, 35), new(97, 35), new(21, 90)).CloseFigure().StartFigure(new(50, 0)).AddArc(new(50,100), new(50,50), 0, true, SweepDirection.Clockwise).AddArc(new(50,0), new(50,50), 0, true, SweepDirection.Clockwise).CloseFigure().Geometry],
            //["Text", new Builder().AddString("Hello World", SystemFonts.MessageFontFamily, SystemFonts.MessageFontSize, SystemFonts.MessageFontStyle).Geometry],
        };

        #endregion

        #region Methods

        #region Static Methods

        private static BitmapSource ToBitmapSource(ImageSource image)
        {
            var visual = new DrawingVisual();
            using (DrawingContext context = visual.RenderOpen())
                context.DrawImage(image, new Rect(0, 0, image.Width, image.Height));
            var bitmap = new RenderTargetBitmap((int)image.Width, (int)image.Height, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(visual);
            return bitmap;
        }

        #endregion

        #region Instance Methods

        [TestCaseSource(nameof(GeometryToPathTestSource))]
        public void GeometryToPathTest(string name, Geometry geometry)
        {
            // WORKAROUND: The test execution engine runs the method on a different thread than the one that initialized the test source
            foreach (object[] args in GeometryToPathTestSource)
            {
                if ((string)args[0] != name)
                    continue;
                geometry = (Geometry)args[1];
                break;
            }

            var brush = new SolidColorBrush(Colors.Yellow);
            var pen = new WpfPen(Brushes.Blue, 1);
            var drawing = new GeometryDrawing();
            drawing.Geometry = geometry;
            drawing.Brush = brush;
            drawing.Pen = pen;
            DrawingImage vectorImage = new DrawingImage(drawing); // just to be able to observe by KGy SOFT ImageSource Visualizer
            //SaveBitmap($"{name}_vector", ToBitmapSource(vectorImage));

            var bounds = geometry.Bounds;
            if (bounds.IsEmpty)
                bounds = new Rect(0, 0, 1, 1);
            if (!geometry.IsFrozen)
            {
                geometry.Transform = new TranslateTransform(-bounds.Left + 1, -bounds.Top + 1);
                bounds = geometry.Bounds;
            }

            var visual = new DrawingVisual();
            using (DrawingContext context = visual.RenderOpen())
            {
                context.DrawRectangle(Brushes.Cyan, null, bounds);
                context.DrawGeometry(brush, pen, geometry);
            }

            bounds.Width += 4;
            bounds.Height += 4;
            int width = (int)bounds.Width;
            int height = (int)bounds.Height;
            var bmpRef = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            var matrix = geometry.Transform.Value;
            matrix.Append(new ScaleTransform((bounds.Width - 4) / bounds.Width, (bounds.Height - 4) / bounds.Height, bounds.Width / 2, bounds.Height / 2).Value);
            if (!geometry.IsFrozen)
                geometry.Transform = new MatrixTransform(matrix);
            bmpRef.Render(visual);
            if (!geometry.IsFrozen)
                geometry.Transform = null;

            SaveBitmap($"{name}_orig", bmpRef);
            bounds = geometry.Bounds;

            Path path = geometry.ToPath();
            var bmp = new WriteableBitmap(bmpRef.PixelWidth, bmpRef.PixelHeight, bmpRef.DpiX, bmpRef.DpiY, bmpRef.Format, null);
            using (var bmpData = bmp.GetReadWriteBitmapData())
            {
                bmpData.Clear(Color.Cyan);
                var transform = TransformationMatrix.CreateTranslation(-(int)bounds.Left + 1, -(int)bounds.Top + 1);
                var options = new DrawingOptions { AntiAliasing = true, DrawPathPixelOffset = PixelOffset.Half, Transformation = transform };
                bmpData.FillPath(Color.Yellow, path, options);
                bmpData.DrawPath(Color.Blue, path, options);
            }

            // The equality is not pixel perfect so it should be compared visually
            //AssertAreEqual(bmpRef, bmp);

            SaveBitmap($"{name}_converted", bmp);
        }

        #endregion

        #endregion
    }
}
