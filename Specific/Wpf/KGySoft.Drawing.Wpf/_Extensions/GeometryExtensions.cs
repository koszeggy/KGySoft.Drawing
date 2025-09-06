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
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Media;

using KGySoft.Drawing.Shapes;

#endregion

#region Used Aliases

using WpfPoint = System.Windows.Point;

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
                    result.AddPoint(lastPoint);
                    foreach (PathSegment segment in figure.Segments)
                    {
                        switch (segment)
                        {
                            case LineSegment line:
                                result.AddPoint(lastPoint = line.Point.ToPointF());
                                break;

                            case PolyLineSegment polyLine:
                                PointCollection points = polyLine.Points;
                                if (points.Count > 0)
                                {
                                    lastPoint = points[points.Count - 1].ToPointF();
                                    result.AddLines(polyLine.Points.Select(PointExtensions.ToPointF));
                                }

                                break;

                            case BezierSegment bezierSegment:
                                result.AddBezier(lastPoint, bezierSegment.Point1.ToPointF(), bezierSegment.Point2.ToPointF(), lastPoint = bezierSegment.Point3.ToPointF());
                                break;

                            case PolyBezierSegment polyBezierSegment:
                                points = polyBezierSegment.Points;
                                if (points.Count >= 3)
                                {
                                    int validCount = points.Count / 3 * 3;
                                    result.AddBeziers([lastPoint, .. points.Take(validCount).Select(PointExtensions.ToPointF)]);
                                    lastPoint = points[validCount - 1].ToPointF();
                                }

                                break;

                            case QuadraticBezierSegment quadraticBezierSegment:
                                var end = quadraticBezierSegment.Point2.ToPointF();
                                (PointF cp1, PointF cp2) = GetCubicControlPointsByFromQuadraticBezier(lastPoint, quadraticBezierSegment.Point1.ToPointF(), end);
                                result.AddBezier(lastPoint, cp1, cp2, lastPoint = end);
                                break;

                            case PolyQuadraticBezierSegment polyQuadraticBezierSegment:
                                points = polyQuadraticBezierSegment.Points;
                                if (points.Count >= 2)
                                {
                                    int validCount = points.Count / 2 * 2;
                                    var cubicPoints = new List<PointF>(validCount / 2 * 3 + 1) { lastPoint };
                                    for (int i = 0; i < validCount; i += 2)
                                    {
                                        end = points[i + 1].ToPointF();
                                        (cp1, cp2) = GetCubicControlPointsByFromQuadraticBezier(lastPoint, points[i].ToPointF(), end);
                                        cubicPoints.AddRange([cp1, cp2, lastPoint = end]);
                                    }

                                    result.AddBeziers(cubicPoints);
                                }

                                break;

                            case ArcSegment arcSegment:
                                var startPoint = lastPoint.ToWpfPoint();
                                var endPoint = arcSegment.Point;
                                double width = arcSegment.Size.Width;
                                double height = arcSegment.Size.Height;
                                WpfPoint center = GetCenter(startPoint, endPoint, width, height, arcSegment.RotationAngle, arcSegment.IsLargeArc, arcSegment.SweepDirection == SweepDirection.Counterclockwise);
                                double radStart = Math.Atan2(startPoint.Y - center.Y, startPoint.X - center.X);
                                double radEnd = Math.Atan2(endPoint.Y - center.Y, endPoint.X - center.X);
                                if (arcSegment.IsLargeArc == Math.Abs(radEnd - radStart) < Math.PI)
                                {
                                    if (radStart < radEnd)
                                        radStart += 2d * Math.PI;
                                    else
                                        radEnd += 2d * Math.PI;
                                }

                                if (arcSegment.RotationAngle != 0f)
                                    result.SetTransformation(TransformationMatrix.CreateRotationDegrees((float)arcSegment.RotationAngle, center.ToPointF()));

                                result.AddArc((float)(center.X - width), (float)(center.Y - height), (float)(2d * width), (float)(2d * height), (float)ToDegree(Math.Min(radStart, radEnd)), (float)ToDegree(Math.Abs(radStart - radEnd)));
                                result.ResetTransformation();
                                lastPoint = arcSegment.Point.ToPointF();
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

        //public static Geometry ToGeometry(this Path path)
        //{
        //    if (path == null)
        //        throw new ArgumentNullException(nameof(path), PublicResources.ArgumentNull);
        //    var result = new PathGeometry();
        //    if (path.IsEmpty)
        //        return result;
        //}

        #endregion

        #region Private Methods

        private static (PointF ControlPoint1, PointF ControlPoint2) GetCubicControlPointsByFromQuadraticBezier(PointF start, PointF controlPoint, PointF end)
            => (new PointF(start.X + 2f / 3f * (controlPoint.X - start.X),
                    start.Y + 2f / 3f * (controlPoint.Y - start.Y)),
                new PointF(end.X + 2f / 3f * (controlPoint.X - end.X),
                    end.Y + 2f / 3f * (controlPoint.Y - end.Y)));

        private static double ToDegree(double radians) => radians / Math.PI * 180d;

        private static WpfPoint GetCenter(WpfPoint startPoint, WpfPoint endPoint, double width, double height, double rotationAngle, bool isLargeArc, bool counterclockwise)
        {
            Matrix matrix = new Matrix();
            matrix.Rotate(-rotationAngle);
            matrix.Scale(height / width, 1d);
            startPoint = matrix.Transform(startPoint);
            endPoint = matrix.Transform(endPoint);
            WpfPoint midPoint = new((startPoint.X + endPoint.X) / 2d, (startPoint.Y + endPoint.Y) / 2d);
            Vector startEndDistance = endPoint - startPoint;
            double halfDistance = startEndDistance.Length / 2d;
            Vector startEndNormal = isLargeArc != counterclockwise ? new Vector(startEndDistance.Y, -startEndDistance.X) : new Vector(-startEndDistance.Y, startEndDistance.X);
            if (halfDistance > 0d)
                startEndNormal.Normalize();
            Vector centerOffset = Math.Sqrt(Math.Abs(height * height - halfDistance * halfDistance)) * startEndNormal;
            WpfPoint center = midPoint + centerOffset;
            matrix.Invert();
            return matrix.Transform(center);
        }

        #endregion

        #endregion
    }
}
