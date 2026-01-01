#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: GeometryExtensions.cs
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;

using KGySoft.CoreLibraries;
using KGySoft.Drawing.Shapes;

using Windows.Foundation;
using Microsoft.UI.Xaml.Media;

#endregion

#region Used Aliases

using ArcSegment = KGySoft.Drawing.Shapes.ArcSegment;
using BezierSegment = KGySoft.Drawing.Shapes.BezierSegment;
using LineSegment = KGySoft.Drawing.Shapes.LineSegment;
using PathSegment = KGySoft.Drawing.Shapes.PathSegment;
using WinUIArcSegment = Microsoft.UI.Xaml.Media.ArcSegment;
using WinUIBezierSegment = Microsoft.UI.Xaml.Media.BezierSegment;
using WinUILineSegment = Microsoft.UI.Xaml.Media.LineSegment;
using WinUIPathSegment = Microsoft.UI.Xaml.Media.PathSegment;
using WinUIPoint = Windows.Foundation.Point;
using WinUISize = Windows.Foundation.Size;

#endregion

#endregion

namespace KGySoft.Drawing.WinUI
{
    /// <summary>
    /// Provides extension methods for <see cref="Geometry"/> class.
    /// </summary>
    public static class GeometryExtensions
    {
        #region Methods

        /// <summary>
        /// Converts a <see cref="Geometry"/> to a <see cref="Path"/>.
        /// </summary>
        /// <param name="geometry">The <see cref="Geometry"/> instance to convert to a <see cref="Path"/>.</param>
        /// <returns>A <see cref="Path"/> instance that represents the same geometry as the specified <see cref="Geometry"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="geometry"/> is <see langword="null"/>.</exception>
        [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator", Justification = "Intended")]
        [SuppressMessage("Microsoft.Maintainability", "CA1502: Avoid excessive complexity", Justification = "The cases are better to be not extracted from the method")]
        public static Path ToPath(this Geometry geometry)
        {
            if (geometry == null)
                throw new ArgumentNullException(nameof(geometry), PublicResources.ArgumentNull);

            var result = new Path();
            IList<Geometry> collection = (geometry as GeometryGroup)?.Children as IList<Geometry> ?? [geometry];
            foreach (Geometry item in collection)
            {
                switch (item)
                {
                    // In WPF this was not necessary, because PathGeometry.CreateFromGeometry handled all cases. Here we do a recursive call if a child item is a GeometryGroup.
                    case GeometryGroup group:
                        result.AddPath(group.ToPath(), false);
                        break;

                    case LineGeometry line:
                        PointF startPoint = line.StartPoint.ToPointF();
                        PointF endPoint = line.EndPoint.ToPointF();
                        if (startPoint.TolerantEquals(endPoint, Constants.PointEqualityTolerance))
                            break;
                        result.AddLine(startPoint, endPoint);
                        break;

                    case RectangleGeometry rectangle:
                        Rect rect = rectangle.Rect;
                        result.AddRectangle((float)rect.X, (float)rect.Y, (float)rect.Width, (float)rect.Height);
                        break;

                    case EllipseGeometry ellipse:
                        Rect bounds = ellipse.Bounds;
                        result.AddEllipse((float)bounds.X, (float)bounds.Y, (float)bounds.Width, (float)bounds.Height);
                        break;

                    case PathGeometry pathGeometry:
                        foreach (PathFigure figure in pathGeometry.Figures)
                        {
                            result.StartFigure();
                            PointF lastPoint = figure.StartPoint.ToPointF();
                            bool lastPointAdded = false;
                            foreach (WinUIPathSegment segment in figure.Segments)
                            {
                                switch (segment)
                                {
                                    case WinUILineSegment line:
                                        if (lastPointAdded)
                                            result.AddPoint(lastPoint = line.Point.ToPointF());
                                        else
                                        {
                                            result.AddLine(lastPoint, lastPoint = line.Point.ToPointF());
                                            lastPointAdded = true;
                                        }

                                        break;

                                    case PolyLineSegment polyLine:
                                        PointCollection points = polyLine.Points;
                                        if (points.Count > 0)
                                        {
                                            if (lastPointAdded)
                                                result.AddLines(polyLine.Points.Select(PointExtensions.ToPointF));
                                            else
                                            {
                                                result.AddLines([lastPoint, .. polyLine.Points.Select(PointExtensions.ToPointF)]);
                                                lastPointAdded = true;
                                            }

                                            lastPoint = points[points.Count - 1].ToPointF();
                                        }

                                        break;

                                    case WinUIBezierSegment bezierSegment:
                                        result.AddBezier(lastPoint, bezierSegment.Point1.ToPointF(), bezierSegment.Point2.ToPointF(), lastPoint = bezierSegment.Point3.ToPointF());
                                        lastPointAdded = true;
                                        break;

                                    case PolyBezierSegment polyBezierSegment:
                                        points = polyBezierSegment.Points;
                                        if (points.Count >= 3)
                                        {
                                            int validCount = points.Count / 3 * 3;
                                            result.AddBeziers((IEnumerable<PointF>)[lastPoint, .. points.Take(validCount).Select(PointExtensions.ToPointF)]);
                                            lastPoint = points[validCount - 1].ToPointF();
                                            lastPointAdded = true;
                                        }

                                        break;

                                    case QuadraticBezierSegment quadraticBezierSegment:
                                        result.AddQuadraticCurve(lastPoint, quadraticBezierSegment.Point1.ToPointF(), lastPoint = quadraticBezierSegment.Point2.ToPointF());
                                        lastPointAdded = true;
                                        break;

                                    case PolyQuadraticBezierSegment polyQuadraticBezierSegment:
                                        points = polyQuadraticBezierSegment.Points;
                                        if (points.Count >= 2)
                                        {
                                            int validCount = points.Count / 2 * 2;
                                            result.AddQuadraticCurves((IEnumerable<PointF>)[lastPoint, .. points.Take(validCount).Select(PointExtensions.ToPointF)]);
                                            lastPoint = points[validCount - 1].ToPointF();
                                            lastPointAdded = true;
                                        }

                                        break;

                                    case WinUIArcSegment arcSegment:
                                        result.AddArc(lastPoint, lastPoint = arcSegment.Point.ToPointF(), (float)arcSegment.Size.Width, (float)arcSegment.Size.Height, (float)arcSegment.RotationAngle, arcSegment.IsLargeArc, arcSegment.SweepDirection == SweepDirection.Clockwise);
                                        lastPointAdded = true;
                                        break;

                                    default:
                                        throw new InvalidOperationException(Res.InternalError($"Unexpected segment type: {segment.GetType()}"));
                                }
                            }

                            if (figure.IsClosed)
                                result.CloseFigure();
                        }

                        break;
                }
            }

            return result;
        }

        /// <summary>
        /// Converts a <see cref="Path"/> to a <see cref="Geometry"/>.
        /// </summary>
        /// <param name="path">The <see cref="Path"/> instance to convert to a <see cref="Geometry"/>.</param>
        /// <returns>A <see cref="Geometry"/> instance that represents the same geometry as the specified <see cref="Path"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <see langword="null"/>.</exception>
        [SuppressMessage("Microsoft.Maintainability", "CA1502: Avoid excessive complexity", Justification = "The cases are better to be not extracted from the method")]
        public static Geometry ToGeometry(this Path path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path), PublicResources.ArgumentNull);
            if (path.IsEmpty)
                return Geometry.Empty;
            var result = new PathGeometry();
            foreach (Figure figure in path.Figures)
            {
                if (figure.IsEmpty)
                    continue;

                ReadOnlyCollection<PathSegment> segments = figure.Segments;
                PointF lastPoint = segments[0].StartPoint;
                var winUIFigure = new PathFigure
                {
                    StartPoint = lastPoint.ToWinUIPoint(),
                    IsClosed = figure.IsClosed
                };

                foreach (PathSegment segment in segments)
                {
                    bool isFirstPointAdded = segment.StartPoint == lastPoint;
                    lastPoint = segment.EndPoint;

                    switch (segment)
                    {
                        case LineSegment lineSegment:
                            IList<PointF> points = lineSegment.Points;
                            int count = isFirstPointAdded ? points.Count - 1 : points.Count;
                            switch (count)
                            {
                                case 0:
                                    // WinUI ignores single-point figures, so we add a one-pixel long line in this case
                                    if (segments.Count == 1)
                                        winUIFigure.Segments.Add(new WinUILineSegment { Point = new WinUIPoint(winUIFigure.StartPoint.X + 0.5d, winUIFigure.StartPoint.Y + 0.5d) });
                                    continue;
                                case 1:
                                    winUIFigure.Segments.Add(new WinUILineSegment { Point = lastPoint.ToWinUIPoint() });
                                    continue;
                                default:
                                    winUIFigure.Segments.Add(new PolyLineSegment { Points = [.. (isFirstPointAdded ? points.Skip(1) : points).Select(p => p.ToWinUIPoint())] });
                                    continue;
                            }

                        case BezierSegment bezierSegment:
                            if (!isFirstPointAdded)
                                winUIFigure.Segments.Add(new WinUILineSegment { Point = segment.StartPoint.ToWinUIPoint() });
                            points = bezierSegment.Points;
                            switch (points.Count)
                            {
                                case 1:
                                    // WinUI ignores single-point figures, so we add a one-pixel long line in this case
                                    if (segments.Count == 1)
                                        winUIFigure.Segments.Add(new WinUILineSegment { Point = new WinUIPoint(winUIFigure.StartPoint.X + 0.5d, winUIFigure.StartPoint.Y + 0.5d) });
                                    continue;
                                case 4:
                                    winUIFigure.Segments.Add(new WinUIBezierSegment
                                    {
                                        Point1 = points[1].ToWinUIPoint(),
                                        Point2 = points[2].ToWinUIPoint(),
                                        Point3 = points[3].ToWinUIPoint()
                                    });
                                    continue;
                                default:
                                    winUIFigure.Segments.Add(new PolyBezierSegment { Points = [.. points.Skip(1).Select(p => p.ToWinUIPoint())] });
                                    continue;
                            }

                        case ArcSegment arcSegment:
                            float radiusX = arcSegment.RadiusX;
                            float radiusY = arcSegment.RadiusY;

                            // Special case 1: horizontally or vertically flat arc or zero sweep angle - adding the flattened points instead
                            // (WinUI arc would just connect start/end points without considering the radius)
                            if (radiusX.TolerantIsZero(Constants.NormalEqualityTolerance) || radiusY.TolerantIsZero(Constants.NormalEqualityTolerance)
                                || arcSegment.SweepAngle.TolerantIsZero(Constants.NormalEqualityTolerance))
                            {
                                points = arcSegment.GetFlattenedPoints();
                                if (points.Count == 1 && segments.Count == 1)
                                    winUIFigure.Segments.Add(new WinUILineSegment { Point = new WinUIPoint(winUIFigure.StartPoint.X + 0.5d, winUIFigure.StartPoint.Y + 0.5d) });
                                else
                                    winUIFigure.Segments.Add(new PolyLineSegment { Points = [.. points.Select(p => p.ToWinUIPoint())] });
                                continue;
                            }

                            WinUISize size = new(radiusX, radiusY);
                            if (!isFirstPointAdded)
                                winUIFigure.Segments.Add(new WinUILineSegment { Point = segment.StartPoint.ToWinUIPoint() });

                            // Special case 2: [almost] full ellipse: cannot simply use a WinUI ArcSegment, because it supports different start and end points only
                            if (Math.Abs(arcSegment.SweepAngle) >= 359f)
                            {
                                // full ellipse with zero start angle: breaking it into two half arcs
                                if (arcSegment.SweepAngle.TolerantEquals(360f, Constants.HighPrecisionTolerance) && arcSegment.StartAngle is 0f)
                                {
                                    var halfPoint = new WinUIPoint(arcSegment.StartPoint.X - radiusX * 2f, arcSegment.StartPoint.Y);
                                    winUIFigure.Segments.Add(new WinUIArcSegment
                                    {
                                        Point = halfPoint,
                                        Size = size,
                                        RotationAngle = 0d,
                                        IsLargeArc = false,
                                        SweepDirection = SweepDirection.Clockwise
                                    });
                                    winUIFigure.Segments.Add(new WinUIArcSegment
                                    {
                                        Point = lastPoint.ToWinUIPoint(),
                                        Size = size,
                                        RotationAngle = 0d,
                                        IsLargeArc = false,
                                        SweepDirection = SweepDirection.Clockwise
                                    });
                                    continue;
                                }

                                // Nonzero start angle (or not a quite complete ellipse): converting to Bézier segments. It preserves the original start/end points.
                                points = arcSegment.ToBezierPoints();
                                if (points.Count > 0)
                                    winUIFigure.Segments.Add(new PolyBezierSegment { Points = [.. points.Skip(1).Select(p => p.ToWinUIPoint())] });
                                continue;
                            }

                            // General case: we can convert to WinUI ArcSegment
                            winUIFigure.Segments.Add(new WinUIArcSegment
                            {
                                Point = lastPoint.ToWinUIPoint(),
                                Size = size,
                                RotationAngle = 0d,
                                IsLargeArc = Math.Abs(arcSegment.SweepAngle) >= 180f,
                                SweepDirection = arcSegment.SweepAngle >= 0f ? SweepDirection.Clockwise : SweepDirection.Counterclockwise
                            });
                            continue;
                    }
                }

                result.Figures.Add(winUIFigure);
            }

            return result;
        }

        #endregion
    }
}
