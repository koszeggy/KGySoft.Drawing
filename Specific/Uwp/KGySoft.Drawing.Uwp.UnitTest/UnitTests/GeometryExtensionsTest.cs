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

using System;
using System.Threading.Tasks;

using Windows.Foundation;
#if DEBUG
using Windows.UI;
using Windows.UI.WindowManagement;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
#endif
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

using KGySoft.CoreLibraries;
using KGySoft.Drawing.Imaging;
using KGySoft.Drawing.Shapes;

using NUnit.Framework;

using Color = System.Drawing.Color;
using Path = KGySoft.Drawing.Shapes.Path;
using UwpArcSegment = Windows.UI.Xaml.Media.ArcSegment;
using UwpBezierSegment = Windows.UI.Xaml.Media.BezierSegment;
using UwpLineSegment = Windows.UI.Xaml.Media.LineSegment;
#if DEBUG
using UwpPath = Windows.UI.Xaml.Shapes.Path;
#endif

#endregion

#region Suppressions

#pragma warning disable CS0618 // Type or member is obsolete - the recommended FormattedText constructor is not available in all target frameworks

#endregion

namespace KGySoft.Drawing.Uwp.UnitTest
{
    [TestFixture]
    public class GeometryExtensionsTest : TestBase
    {
        #region Nested classes

        public sealed class Builder
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
            internal Builder(Point startPoint) => StartFigure(startPoint);

            #endregion

            #region Methods

            internal Builder StartFigure(Point startPoint)
            {
                geometry.Figures.Add(currentFigure = new PathFigure { StartPoint = startPoint, IsClosed = false });
                return this;
            }

            internal Builder CloseFigure()
            {
                currentFigure.IsClosed = true;
                return this;
            }

            internal Builder AddLine(Point point)
            {
                currentFigure.Segments.Add(new UwpLineSegment { Point = point });
                return this;
            }

            internal Builder AddLines(params Point[] points)
            {
                var segment = new PolyLineSegment();
                segment.Points.AddRange(points);
                currentFigure.Segments.Add(segment);
                return this;
            }

            internal Builder AddBezier(Point cp1, Point cp2, Point end)
            {
                currentFigure.Segments.Add(new UwpBezierSegment { Point1 = cp1, Point2 = cp2, Point3 = end });
                return this;
            }

            internal Builder AddBeziers(params Point[] points)
            {
                var segment = new PolyBezierSegment();
                segment.Points.AddRange(points);
                currentFigure.Segments.Add(segment);
                return this;
            }

            internal Builder AddQuadraticBezier(Point cp, Point end)
            {
                currentFigure.Segments.Add(new QuadraticBezierSegment { Point1 = cp, Point2 = end });
                return this;
            }

            internal Builder AddQuadraticBeziers(params Point[] points)
            {
                var segment = new PolyQuadraticBezierSegment();
                segment.Points.AddRange(points);
                currentFigure.Segments.Add(segment);
                return this;
            }

            internal Builder AddArc(Point point, Size size, double angle, bool isLarge, SweepDirection direction)
            {
                currentFigure.Segments.Add(new UwpArcSegment { Point = point, Size = size, RotationAngle = angle, IsLargeArc = isLarge, SweepDirection = direction });
                return this;
            }

            #endregion
        }

        #endregion

        #region Properties

        private static object[][] GeometryToPathTestSource => new object[][]
        {
            //["Empty", () => Geometry.Empty], // System.Exception: 'Catastrophic failure (Exception from HRESULT: 0x8000FFFF (E_UNEXPECTED))'
            ["Line", () => new LineGeometry { StartPoint = new(0, 0), EndPoint = new Point(10, 10) }],
            ["Empty Line", () => new LineGeometry { StartPoint = new(10, 10), EndPoint = new Point(10, 10) }],
            ["Rectangle", () => new RectangleGeometry() { Rect = new(0, 0, 10, 10) }],
            ["Empty Rectangle", () => new RectangleGeometry() { Rect = new(10, 10, 0, 0) }],
            ["Ellipse", () => new EllipseGeometry() { Center = new(50, 50), RadiusX = 20, RadiusY = 10 }],
            ["Empty Ellipse", () => new EllipseGeometry() { Center = new(50, 50), RadiusX = 0, RadiusY = 0 }],
            ["Flat Ellipse", () => new EllipseGeometry() { Center = new(50, 50), RadiusX = 20, RadiusY = 0 }],
            ["Single point path", () => new Builder(new Point(0, 0)).Geometry],
            ["Single line path", () => new Builder(new Point(0, 0)).AddLine(new(10, 10)).Geometry],
            ["Polyline", () => new Builder(new Point(50, 0)).AddLines(new(79, 90), new(2, 35), new(97, 35), new(21, 90)).Geometry],
            ["Lines", () => new Builder(new Point(0, 0)).AddLine(new(10, 10)).AddLines(new(0, 10), new(10, 0)).Geometry],
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
            ["Closed arc half 1", () => new Builder(new Point(50, 0)).AddArc(new(50,100), new(50,50), 0, true, SweepDirection.Clockwise).CloseFigure().Geometry],
            ["Closed arc half 2", () => new Builder(new Point(50, 100)).AddArc(new(50,0), new(50,50), 0, true, SweepDirection.Clockwise).CloseFigure().Geometry],
            ["Closed arc quarter 1", () => new Builder(new Point(50, 0)).AddArc(new(100,50), new(50,50), 0, false, SweepDirection.Clockwise).CloseFigure().Geometry],
            ["Closed arc quarter 2", () => new Builder(new Point(100, 50)).AddArc(new(50,100), new(50,50), 0, false, SweepDirection.Clockwise).CloseFigure().Geometry],
            ["Closed arc quarter 3", () => new Builder(new Point(50, 100)).AddArc(new(0,50), new(50,50), 0, false, SweepDirection.Clockwise).CloseFigure().Geometry],
            ["Closed arc quarter 4", () => new Builder(new Point(0, 50)).AddArc(new(50,0), new(50,50), 0, false, SweepDirection.Clockwise).CloseFigure().Geometry],
            ["Closed figures", () => new Builder(new Point(50, 0)).AddLines(new(79, 90), new(2, 35), new(97, 35), new(21, 90)).CloseFigure().StartFigure(new(50, 0)).AddArc(new(50,100), new(50,50), 0, true, SweepDirection.Clockwise).AddArc(new(50, 0), new(50, 50), 0, true, SweepDirection.Clockwise).CloseFigure().Geometry],
        };

        #endregion

        #region Methods

        [TestCaseSource(nameof(GeometryToPathTestSource))]
        public Task GeometryToPathTest(string name, Func<Geometry> geometryFactory) => ExecuteTestAsync(async () =>
        {
            // Using a delegate to obtain the geometry in the correct thread
            Geometry geometry = geometryFactory.Invoke();

            var bounds = geometry.Bounds;
            if (bounds.IsEmpty)
                bounds = new Rect(0, 0, 1, 1);

#if DEBUG
            // this is how we can display the UWP geometry
            var uwpPath = new UwpPath
            {
                Data = geometry,
                Fill = new SolidColorBrush(Colors.Yellow),
                Stroke = new SolidColorBrush(Colors.Blue)
            };
            AppWindow appWindow = await AppWindow.TryCreateAsync();
            var canvas = new Canvas { Background = new SolidColorBrush(Colors.Cyan), Children = { uwpPath } };
            ElementCompositionPreview.SetAppWindowContent(appWindow, canvas);
            await appWindow.TryShowAsync();

            //// This is how we could render the UWP geometry to a RenderTargetBitmap. The issue is that the RenderTargetBitmap will always be empty,
            ////  even if we make sure the path is visible in a window, so the proper testing now is comparing the displayed path and the saved images visually.
            //var bmpRef = new RenderTargetBitmap();
            //bmpRef.RenderAsync(uwpPath);
            //SaveBitmap($"{name}_orig", bmpRef); // now for WriteableBitmap only, but RenderTargetBitmap has a GetPixelsAsync method instead of a PixelBuffer property

            //// This is how we could reinterpret the pixels as an IReadableBitmapData if the RenderTargetBitmap would not be empty, and debugger visualizers worked for IReadableBitmapData in UWP.
            //var pixels = await bmpRef.GetPixelsAsync();
            //var pixelsBuffer = new byte[pixels.Length];
            //pixels.CopyTo(pixelsBuffer);
            //using var bmpDataRef = BitmapDataFactory.CreateBitmapData(pixelsBuffer, new (bmpRef.PixelWidth, bmpRef.PixelHeight), bmpRef.PixelWidth * 4, KnownPixelFormat.Format32bppPArgb); 
#endif

            bounds.Width += 4;
            bounds.Height += 4;
            int width = (int)bounds.Width;
            int height = (int)bounds.Height;

            Path path = geometry.ToPath();
            var bmp = new WriteableBitmap(width, height);
            using (var bmpData = bmp.GetReadWriteBitmapData())
            {
                bmpData.Clear(Color.Cyan);
                var transformation = TransformationMatrix.CreateTranslation(-(int)bounds.Left + 1, -(int)bounds.Top + 1);
                var options = new DrawingOptions { AntiAliasing = true, Transformation = transformation };
                bmpData.FillPath(Color.Yellow, path, options);
                bmpData.DrawPath(Color.Blue, path, options);
            }

            await SaveBitmap($"{name}_converted", bmp);
#if DEBUG
            // For proper visual testing, place a breakpoint here and compare the Path in the window with the saved bitmap, whose name is printed in the console.
            await appWindow.CloseAsync();
#else
            if (!SaveToFile)
                Assert.Inconclusive("Set SaveToFile to true to save the bitmaps for visual comparison.");
#endif

            // The equality is not pixel perfect so it should be compared visually
            //AssertAreEqual(bmpDataRef, bmpData);
        });

        #endregion
    }
}
