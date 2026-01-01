#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: GeometryExtensionsTest.cs
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

using System;
using System.Threading.Tasks;

using KGySoft.CoreLibraries;
using KGySoft.Drawing.Imaging;
using KGySoft.Drawing.Shapes;

#if DEBUG
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
#endif
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;

using NUnit.Framework;

using Windows.Foundation;

#endregion

#region Used Aliases

using Color = System.Drawing.Color;
using Path = KGySoft.Drawing.Shapes.Path;
using WinUIArcSegment = Microsoft.UI.Xaml.Media.ArcSegment;
using WinUIBezierSegment = Microsoft.UI.Xaml.Media.BezierSegment;
using WinUILineSegment = Microsoft.UI.Xaml.Media.LineSegment;
#if DEBUG
using WinUIPath = Microsoft.UI.Xaml.Shapes.Path;
#endif

#endregion

#endregion

namespace KGySoft.Drawing.WinUI.UnitTests
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
                currentFigure.Segments.Add(new WinUILineSegment { Point = point });
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
                currentFigure.Segments.Add(new WinUIBezierSegment { Point1 = cp1, Point2 = cp2, Point3 = end });
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
                currentFigure.Segments.Add(new WinUIArcSegment { Point = point, Size = size, RotationAngle = angle, IsLargeArc = isLarge, SweepDirection = direction });
                return this;
            }

            #endregion
        }

        #endregion

        #region Properties

        private static object[][] GeometryToPathTestSource => new object[][]
        {
            //["Empty", () => Geometry.Empty], // COMException
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

        private static object[][] PathToGeometryTestSource => new object[][]
        {
            //["Empty", new Path()], // System.Runtime.InteropServices.COMException: ''
            ["Single point", new Path().AddPoint(new(0, 0))],
            ["Single point bezier", new Path().AddBeziers([new(0, 0)])],
            ["Single point arc", new Path().AddArc(new(0, 0, 100, 100), 0, 0)],
            ["Single line", new Path().AddLine(new(0, 0), new(10, 10))],
            ["Polyline", new Path().AddLines(new(50, 0), new(79, 90), new(2, 35), new(97, 35), new(21, 90))],
            ["Polygon", new Path().AddLines(new(50, 0), new(79, 90), new(2, 35), new(97, 35), new(21, 90)).CloseFigure()],
            ["Single bezier arc", new Path().AddArc(0, 0, 100, 100, 90, 90)],
            ["Multi bezier arc", new Path().AddArc(0, 0, 100, 100, 90, 300)],
            ["Point-bezier", new Path().AddPoint(50, 0).AddArc(0, 0, 100, 100, 90, 90)],
            ["Bezier-point", new Path().AddArc(0, 0, 100, 100, 90, 90).AddPoint(50, 0)],
            ["Point-horizontal small arc-point", new Path().AddPoint(50, 0).AddArc(0, 50, 100, 20, 45, 90).AddPoint(new(50, 100))],
            ["Point-horizontal almost flat small arc-point", new Path().AddPoint(50, 0).AddArc(0, 50, 100, 1e-6f, 45, 90).AddPoint(new(50, 100))],
            ["Point-horizontal flat small arc-point", new Path().AddPoint(50, 0).AddArc(0, 50, 100, 0, 45, 90).AddPoint(new(50, 100))],
            ["Point-vertical small arc-point", new Path().AddPoint(0, 50).AddArc(50, 0, 20, 100, 45, 90).AddPoint(new(100, 50))],
            ["Point-vertical almost flat small arc-point", new Path().AddPoint(0, 50).AddArc(50, 0, 1e-6f, 100, 45, 90).AddPoint(new(100, 50))],
            ["Point-vertical flat small arc-point", new Path().AddPoint(0, 50).AddArc(50, 0, 0, 100, 45, 90).AddPoint(new(100, 50))],
            ["Point-horizontal large arc-point", new Path().AddPoint(50, 0).AddArc(0, 50, 100, 20, 45, 300).AddPoint(new(50, 100))],
            ["Point-horizontal almost flat large arc-point", new Path().AddPoint(50, 0).AddArc(0, 50, 100, 1e-6f, 45, 300).AddPoint(new(50, 100))],
            ["Point-horizontal flat large arc-point", new Path().AddPoint(50, 0).AddArc(0, 50, 100, 0, 45, 300).AddPoint(new(50, 100))],
            ["Point-vertical large arc-point", new Path().AddPoint(0, 50).AddArc(50, 0, 20, 100, 45, 300).AddPoint(new(100, 50))],
            ["Point-vertical almost flat large arc-point", new Path().AddPoint(0, 50).AddArc(50, 0, 1e-6f, 100, 45, 300).AddPoint(new(100, 50))],
            ["Point-vertical flat large arc-point", new Path().AddPoint(0, 50).AddArc(50, 0, 0, 100, 45, 300).AddPoint(new(100, 50))],
            ["Point-horizontal small negative arc-point", new Path().AddPoint(50, 0).AddArc(0, 50, 100, 20, 45, -90).AddPoint(new(50, 100))],
            ["Point-vertical small negative arc-point", new Path().AddPoint(0, 50).AddArc(50, 0, 20, 100, 45, -90).AddPoint(new(100, 50))],
            ["Point-horizontal large negative arc-point", new Path().AddPoint(50, 0).AddArc(0, 50, 100, 20, 45, -300).AddPoint(new(50, 100))],
            ["Point-vertical large negative arc-point", new Path().AddPoint(0, 50).AddArc(50, 0, 20, 100, 45, -300).AddPoint(new(100, 50))],
            ["Ellipse", new Path().AddEllipse(0, 0, 100, 50)],
            ["Rotated ellipse", new Path().TransformRotation(45f).AddEllipse(0, 0, 100, 50)],
            ["Point-horizontal ellipse-point", new Path().AddPoint(50, 0).AddArc(0, 50, 100, 20, 45, 360).AddPoint(new(50, 100))],
            ["Point-horizontal almost flat ellipse-point", new Path().AddPoint(50, 0).AddArc(0, 50, 100, 1e-6f, 45, 360).AddPoint(new(50, 100))],
            ["Point-horizontal flat ellipse-point", new Path().AddPoint(50, 0).AddArc(0, 50, 100, 0, 45, 360).AddPoint(new(50, 100))],
            ["Point-vertical ellipse-point", new Path().AddPoint(0, 50).AddArc(50, 0, 20, 100, 45, 360).AddPoint(new(100, 50))],
            ["Point-vertical almost flat ellipse-point", new Path().AddPoint(0, 50).AddArc(50, 0, 1e-6f, 100, 45, 360).AddPoint(new(100, 50))],
            ["Point-vertical flat ellipse-point", new Path().AddPoint(0, 50).AddArc(50, 0, 0, 100, 45, 360).AddPoint(new(100, 50))],
            ["Open-close", new Path().AddLines(new(50, 0), new(79, 90), new(2, 35), new(97, 35), new(21, 90)).AddEllipse(0, 0, 100, 100)],
            ["Closed figures", new Path().AddPolygon(new(50, 0), new(79, 90), new(2, 35), new(97, 35), new(21, 90)).AddEllipse(0, 0, 100, 100).AddRoundedRectangle(0, 0, 100, 100, 10)],
        };

        #endregion

        #region Methods

        [TestCaseSource(nameof(GeometryToPathTestSource))]
        public Task GeometryToPathTest(string name, Func<Geometry> geometryFactory) => ExecuteTestAsync(async () =>
        {
            // Using a delegate to obtain the geometry in the correct thread
            Geometry geometry = geometryFactory.Invoke();

            Rect bounds = geometry.Bounds;
            if (bounds.IsEmpty)
                bounds = new Rect(0, 0, 1, 1);

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

            string? fileName = await SaveBitmap($"{name}_converted", bmp);
            if (!SaveToFile)
                Assert.Inconclusive("Set SaveToFile to true to save the bitmaps for visual comparison.");

#if DEBUG
            // this is how we can display the WinUI geometry
            var winUIPath = new WinUIPath
            {
                Data = geometry,
                Fill = new SolidColorBrush(Colors.Yellow),
                Stroke = new SolidColorBrush(Colors.Blue)
            };

            var dialog = new ContentDialog();
            dialog.Content = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Children =
                {
                    new TextBlock { Text = $"Actual result path: {fileName}", TextWrapping = TextWrapping.Wrap },
                    //new TextBox { Text = fileName }, // ISSUE: if there is a TextBox in the dialog, the dialog disappears and the original window remains disabled
                    new Grid { Background = new SolidColorBrush(Colors.Cyan), Children = { winUIPath } } // using a Grid because a Canvas inside a StackPanel ignores the background
                }
            };
            //window.Content = new Canvas { Background = new SolidColorBrush(Colors.Cyan), Children = { winUIPath } };
            dialog.XamlRoot = Program.XamlRoot;
            dialog.Title = $"Reference result - {name}";
            dialog.CloseButtonText = "Continue";
            dialog.FullSizeDesired = true;
            //window.Opened += (s, e) => s.Hide(); // Saving to RenderTargetBitmap does not work even if the path is visible. So leaving the dialog open for visual comparison.
            await dialog.ShowAsync();
#endif

            //// This is how we could render the WinUI geometry to a RenderTargetBitmap. The issue is that the RenderTargetBitmap will always be empty,
            //// even if we make sure the path is visible in a window, so the proper testing now is comparing the displayed path and the saved images visually.
            //var bmpRef = new RenderTargetBitmap();
            //bmpRef.RenderAsync(winUIPath);
            ////SaveBitmap($"{name}_orig", bmpRef); // now for WriteableBitmap only, but RenderTargetBitmap has a GetPixelsAsync method instead of a PixelBuffer property

            //// This is how we could reinterpret the pixels as an IReadableBitmapData if the RenderTargetBitmap was not empty, and debugger visualizers worked for IReadableBitmapData in WinUI.
            //var pixels = await bmpRef.GetPixelsAsync();
            //var pixelsBuffer = new byte[pixels.Length];
            //pixels.CopyTo(pixelsBuffer);
            //using var bmpDataRef = BitmapDataFactory.CreateBitmapData(pixelsBuffer, new (bmpRef.PixelWidth, bmpRef.PixelHeight), bmpRef.PixelWidth * 4, KnownPixelFormat.Format32bppPArgb);

            // The equality is not pixel perfect so it should be compared visually
            //AssertAreEqual(bmpDataRef, bmpData);
        });

        [TestCaseSource(nameof(PathToGeometryTestSource))]
        public Task PathToGeometryTest(string name, Path path) => ExecuteTestAsync(async () =>
        {
            var bounds = path.Bounds;
            int width = Math.Max(1, bounds.Width);
            int height = Math.Max(1, bounds.Height);
            var bmpRef = new WriteableBitmap(width + 2, height + 2);
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

            string? fileName = await SaveBitmap($"{name}_orig", bmpRef);
            if (!SaveToFile)
                Assert.Inconclusive("Set SaveToFile to true to save the bitmaps for visual comparison.");

            Geometry geometry = path.ToGeometry();

#if DEBUG
            // this is how we can display the WinUI geometry
            var winUIPath = new WinUIPath
            {
                Data = geometry,
                Fill = new SolidColorBrush(Colors.Yellow),
                Stroke = new SolidColorBrush(Colors.Blue)
            };

            var dialog = new ContentDialog();
            dialog.Content = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Children =
                {
                    new TextBlock { Text = $"Reference result path: {fileName}", TextWrapping = TextWrapping.Wrap },
                    //new TextBox { Text = fileName }, // ISSUE: if there is a TextBox in the dialog, the dialog disappears and the original window remains disabled
                    new Grid { Background = new SolidColorBrush(Colors.Cyan), Children = { winUIPath } } // using a Grid because a Canvas inside a StackPanel ignores the background
                }
            };
            //window.Content = new Canvas { Background = new SolidColorBrush(Colors.Cyan), Children = { winUIPath } };
            dialog.XamlRoot = Program.XamlRoot;
            dialog.Title = $"Actual result - {name}";
            dialog.CloseButtonText = "Continue";
            dialog.FullSizeDesired = true;
            //window.Opened += (s, e) => s.Hide(); // Saving to RenderTargetBitmap does not work even if the path is visible. So leaving the dialog open for visual comparison.
            await dialog.ShowAsync();

            //// This is how we could render the WinUI geometry to a RenderTargetBitmap. The issue is that the RenderTargetBitmap will always be empty,
            //// even if we make sure the path is visible in a window, so the proper testing now is comparing the displayed path and the saved images visually.
            //var bmp = new RenderTargetBitmap();
            //bmp.RenderAsync(winUIPath);
            ////await SaveBitmap($"{name}_converted", bmp); // now for WriteableBitmap only, but RenderTargetBitmap has a GetPixelsAsync method instead of a PixelBuffer property

            //// This is how we could reinterpret the pixels as an IReadableBitmapData if the RenderTargetBitmap was not empty, and debugger visualizers worked for IReadableBitmapData in WinUI.
            //var pixels = await bmp.GetPixelsAsync();
            //var pixelsBuffer = new byte[pixels.Length];
            //pixels.CopyTo(pixelsBuffer);
            //using var bmpDataRef = BitmapDataFactory.CreateBitmapData(pixelsBuffer, new(bmp.PixelWidth, bmp.PixelHeight), bmp.PixelWidth * 4, KnownPixelFormat.Format32bppPArgb);
#endif

            // The equality is not pixel perfect so it should be compared visually
            //AssertAreEqual(bmpRef.ToWriteableBitmap(), bmp);
        });

        #endregion
    }
}
