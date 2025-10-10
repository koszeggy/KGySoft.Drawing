#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: GeometryExtensions.cs
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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;

using KGySoft.Drawing.Shapes;

using Windows.Foundation;
using Windows.UI.Xaml.Media;

#endregion

#region Used Aliases

using UwpArcSegment = Windows.UI.Xaml.Media.ArcSegment;
using UwpBezierSegment = Windows.UI.Xaml.Media.BezierSegment;
using UwpLineSegment = Windows.UI.Xaml.Media.LineSegment;
using UwpPathSegment = Windows.UI.Xaml.Media.PathSegment;

#endregion

#endregion

namespace KGySoft.Drawing.Uwp
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
                            foreach (UwpPathSegment segment in figure.Segments)
                            {
                                switch (segment)
                                {
                                    case UwpLineSegment line:
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

                                    case UwpBezierSegment bezierSegment:
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

                                    case UwpArcSegment arcSegment:
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

        #endregion
    }
}
