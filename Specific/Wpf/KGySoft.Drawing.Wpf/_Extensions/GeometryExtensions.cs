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

using KGySoft.CoreLibraries;

#region Used Namespaces

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Windows.Media;

using KGySoft.Drawing.Shapes;

#endregion

#region Used Aliases

using ArcSegment = KGySoft.Drawing.Shapes.ArcSegment;
using BezierSegment = KGySoft.Drawing.Shapes.BezierSegment;
using LineSegment = KGySoft.Drawing.Shapes.LineSegment;
using PathSegment = KGySoft.Drawing.Shapes.PathSegment;
using WpfArcSegment = System.Windows.Media.ArcSegment;
using WpfBezierSegment = System.Windows.Media.BezierSegment;
using WpfLineSegment = System.Windows.Media.LineSegment;
using WpfPathSegment = System.Windows.Media.PathSegment;
using WpfPoint = System.Windows.Point;
using WpfSize = System.Windows.Size;

#endregion

#endregion

namespace KGySoft.Drawing.Wpf
{
    /// <summary>
    /// Provides extension methods for <see cref="Geometry"/> class.
    /// </summary>
    public static class GeometryExtensions
    {
        #region Methods

        #region Public Methods

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
            if (geometry.IsEmpty())
                return result;

            IList<Geometry> collection = (geometry as GeometryGroup)?.Children as IList<Geometry> ?? [geometry];
            foreach (Geometry item in collection)
            {
                PathGeometry pathGeometry = item as PathGeometry ?? PathGeometry.CreateFromGeometry(item);
                foreach (PathFigure figure in pathGeometry.Figures)
                {
                    result.StartFigure();
                    PointF lastPoint = figure.StartPoint.ToPointF();
                    bool lastPointAdded = false;
                    foreach (WpfPathSegment segment in figure.Segments)
                    {
                        switch (segment)
                        {
                            case WpfLineSegment line:
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
                                        result.AddLines([lastPoint, ..polyLine.Points.Select(PointExtensions.ToPointF)]);
                                        lastPointAdded = true;
                                    }

                                    lastPoint = points[points.Count - 1].ToPointF();
                                }

                                break;

                            case WpfBezierSegment bezierSegment:
                                result.AddBezier(lastPoint, bezierSegment.Point1.ToPointF(), bezierSegment.Point2.ToPointF(), lastPoint = bezierSegment.Point3.ToPointF());
                                lastPointAdded = true;
                                break;

                            case PolyBezierSegment polyBezierSegment:
                                points = polyBezierSegment.Points;
                                if (points.Count >= 3)
                                {
                                    int validCount = points.Count / 3 * 3;
                                    result.AddBeziers((IEnumerable<PointF>)[lastPoint, ..points.Take(validCount).Select(PointExtensions.ToPointF)]);
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

                            case WpfArcSegment arcSegment:
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
                var wpfFigure = new PathFigure
                {
                    StartPoint = lastPoint.ToWpfPoint(),
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
                                    // WPF ignores single-point figures, so we add a one-pixel long line in this case
                                    if (segments.Count == 1)
                                        wpfFigure.Segments.Add(new WpfLineSegment(new WpfPoint(wpfFigure.StartPoint.X + 0.5d, wpfFigure.StartPoint.Y + 0.5d), true));
                                    continue;
                                case 1:
                                    wpfFigure.Segments.Add(new WpfLineSegment(lastPoint.ToWpfPoint(), true));
                                    continue;
                                default:
                                    wpfFigure.Segments.Add(new PolyLineSegment((isFirstPointAdded ? points.Skip(1) : points).Select(p => p.ToWpfPoint()), true));
                                    continue;
                            }

                        case BezierSegment bezierSegment:
                            if (!isFirstPointAdded)
                                wpfFigure.Segments.Add(new WpfLineSegment(segment.StartPoint.ToWpfPoint(), true));
                            points = bezierSegment.Points;
                            switch (points.Count)
                            {
                                case 1:
                                    // WPF ignores single-point figures, so we add a one-pixel long line in this case
                                    if (segments.Count == 1)
                                        wpfFigure.Segments.Add(new WpfLineSegment(new WpfPoint(wpfFigure.StartPoint.X + 0.5d, wpfFigure.StartPoint.Y + 0.5d), true));
                                    continue;
                                case 4:
                                    wpfFigure.Segments.Add(new WpfBezierSegment(points[1].ToWpfPoint(), points[2].ToWpfPoint(), points[3].ToWpfPoint(), true));
                                    continue;
                                default:
                                    wpfFigure.Segments.Add(new PolyBezierSegment(points.Skip(1).Select(p => p.ToWpfPoint()), true));
                                    continue;
                            }

                        case ArcSegment arcSegment:
                            float radiusX = arcSegment.RadiusX;
                            float radiusY = arcSegment.RadiusY;

                            // Special case 1: horizontally or vertically flat arc or zero sweep angle - adding the flattened points instead
                            // (WPF arc would just connect start/end points without considering the radius)
                            if (radiusX.TolerantIsZero(Constants.NormalEqualityTolerance) || radiusY.TolerantIsZero(Constants.NormalEqualityTolerance)
                                || arcSegment.SweepAngle.TolerantIsZero(Constants.NormalEqualityTolerance))
                            {
                                points = arcSegment.GetFlattenedPoints();
                                if (points.Count == 1 && segments.Count == 1)
                                    wpfFigure.Segments.Add(new WpfLineSegment(new WpfPoint(wpfFigure.StartPoint.X + 0.5d, wpfFigure.StartPoint.Y + 0.5d), true));
                                else
                                    wpfFigure.Segments.Add(new PolyLineSegment(points.Select(p => p.ToWpfPoint()), true));
                                continue;
                            }

                            WpfSize size = new(radiusX, radiusY);
                            if (!isFirstPointAdded)
                                wpfFigure.Segments.Add(new WpfLineSegment(segment.StartPoint.ToWpfPoint(), true));

                            // Special case 2: [almost] full ellipse: cannot simply use a WPF ArcSegment, because it supports different start and end points only
                            if (Math.Abs(arcSegment.SweepAngle) >= 359f)
                            {
                                // full ellipse with zero start angle: breaking it into two half arcs
                                if (arcSegment.SweepAngle.TolerantEquals(360f, Constants.HighPrecisionTolerance) && arcSegment.StartAngle is 0f)
                                {
                                    var halfPoint = new WpfPoint(arcSegment.StartPoint.X - radiusX * 2f, arcSegment.StartPoint.Y);
                                    wpfFigure.Segments.Add(new WpfArcSegment(halfPoint, size, 0d, true, SweepDirection.Clockwise, true));
                                    wpfFigure.Segments.Add(new WpfArcSegment(lastPoint.ToWpfPoint(), size, 0d, true, SweepDirection.Clockwise, true));
                                    continue;
                                }

                                // Nonzero start angle (or not a quite complete ellipse): converting to Bézier segments. It preserves the original start/end points.
                                points = arcSegment.ToBezierPoints();
                                if (points.Count > 0)
                                    wpfFigure.Segments.Add(new PolyBezierSegment(points.Skip(1).Select(p => p.ToWpfPoint()), true));
                                continue;
                            }

                            // General case: we can convert to WPF ArcSegment
                            wpfFigure.Segments.Add(new WpfArcSegment(lastPoint.ToWpfPoint(),
                                size,
                                0d,
                                Math.Abs(arcSegment.SweepAngle) >= 180f,
                                arcSegment.SweepAngle >= 0f ? SweepDirection.Clockwise : SweepDirection.Counterclockwise,
                                true));
                            continue;
                    }
                }

                result.Figures.Add(wpfFigure);
            }

            return result;
        }

        #endregion

        #endregion
    }
}
