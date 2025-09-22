#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: SKPathExtensions.cs
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
using System.Collections.ObjectModel;
using System.Drawing;

using KGySoft.CoreLibraries;
using KGySoft.Drawing.Shapes;

using SkiaSharp;

#endregion

namespace KGySoft.Drawing.SkiaSharp
{
    /// <summary>
    /// Contains extension methods for the <see cref="SKPath"/> class.
    /// </summary>
    public static class SKPathExtensions
    {
        #region Methods

        /// <summary>
        /// Converts the specified <see cref="SKPath"/> to a <see cref="Path"/> object.
        /// </summary>
        /// <param name="path">The <see cref="SKPath"/> instance to convert to a <see cref="Path"/>.</param>
        /// <returns>A <see cref="Path"/> object that represents the same geometric path as the specified <see cref="SKPath"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="path"/> is <see langword="null"/>.</exception>
        public static Path ToPath(this SKPath path)
        {
            #region Local Methods

            static (PointF ControlPoint1, PointF ControlPoint2) GetCubicControlPointsFromQuadraticBezier(PointF start, PointF controlPoint, PointF end)
                => ((start.AsVector2() + 2f / 3f * (controlPoint.AsVector2() - start.AsVector2())).AsPointF(),
                    (end.AsVector2() + 2f / 3f * (controlPoint.AsVector2() - end.AsVector2())).AsPointF());

            static (PointF ControlPoint1, PointF ControlPoint2) GetCubicControlPointsFromConicCurve(PointF start, PointF controlPoint, PointF end, float weight)
            {
                // Though SKPath has a ConvertConicToQuads, there is no need to approximate the curve by quadratic Béziers, because a single cubic Bézier curve always can represent a conic curve.
                // The problem is that SkiaSharp has no API to convert a conic curve to a cubic Bézier.
                // Credit to this paper where I finally managed to find the solution: https://www.mn.uio.no/math/english/people/aca/michaelf/papers/g4.pdf
                float lambda = weight * 4f / 3f / (1 + weight);
                float inverseLambda = 1 - lambda;
                return ((inverseLambda * start.AsVector2() + lambda * controlPoint.AsVector2()).AsPointF(),
                        (inverseLambda * end.AsVector2() + lambda * controlPoint.AsVector2()).AsPointF());
            }

            #endregion

            if (path == null)
                throw new ArgumentNullException(nameof(path), PublicResources.ArgumentNull);

            var result = new Path();
            int count = path.PointCount;
            if (count == 0)
                return result;

            // As usual, the official SkiaSharp documentation is not quite helpful.
            // But this old Xamarin page has a good example for processing SKPath segments: https://learn.microsoft.com/en-us/previous-versions/xamarin/xamarin-forms/user-interface/graphics/skiasharp/curves/information#enumerating-the-path
            bool lastPointAdded = false;
            using SKPath.RawIterator iterator = path.CreateRawIterator();
            SKPoint[] buf = new SKPoint[4];
            SKPathVerb verb;
            while ((verb = iterator.Next(buf)) != SKPathVerb.Done)
            {
                switch (verb)
                {
                    case SKPathVerb.Move:
                        // Not adding the new start point here, because KGy SOFT's Path would render even a single point.
                        // Instead, adding it only when a section is actually added. NOTE: no need to store the start point (buf[0])
                        // because it is always repeated as the first point of the current section.
                        result.StartFigure();
                        lastPointAdded = false;
                        break;

                    case SKPathVerb.Line:
                        if (!lastPointAdded)
                        {
                            result.AddLine(buf[0].AsPointF(), buf[1].AsPointF());
                            lastPointAdded = true;
                        }
                        else
                            result.AddPoint(buf[1].AsPointF());
                        break;

                    case SKPathVerb.Cubic:
                        result.AddBezier(buf[0].AsPointF(), buf[1].AsPointF(), buf[2].AsPointF(), buf[3].AsPointF());
                        lastPointAdded = true;
                        break;

                    case SKPathVerb.Quad:
                        PointF startPoint = buf[0].AsPointF();
                        PointF endPoint = buf[2].AsPointF();
                        (PointF cp1, PointF cp2) = GetCubicControlPointsFromQuadraticBezier(startPoint, buf[1].AsPointF(), endPoint);
                        result.AddBezier(startPoint, cp1, cp2, endPoint);
                        lastPointAdded = true;
                        break;

                    case SKPathVerb.Conic:
                        startPoint = buf[0].AsPointF();
                        endPoint = buf[2].AsPointF();
                        (cp1, cp2) = GetCubicControlPointsFromConicCurve(startPoint, buf[1].AsPointF(), endPoint, iterator.ConicWeight());
                        result.AddBezier(startPoint, cp1, cp2, endPoint);
                        lastPointAdded = true;
                        break;

                    case SKPathVerb.Close:
                        result.CloseFigure();
                        lastPointAdded = false;
                        break;

                    default:
                        throw new InvalidOperationException(Res.InternalError($"Unexpected verb: {verb}"));
                }
            }

            return result;
        }

        /// <summary>
        /// Converts the specified <see cref="Path"/> instance to an <see cref="SKPath"/> object.
        /// </summary>
        /// <param name="path">The <see cref="Path"/> instance to convert to an <see cref="SKPath"/>.</param>
        /// <returns>An <see cref="SKPath"/> instance that represents the same geometry as the specified <see cref="Path"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <see langword="null"/>.</exception>
        public static SKPath ToSKPath(this Path path)
        {
            #region Local Methods

            static void AddLines(SKPath skiaPath, IList<PointF> points, int startIndex)
            {
                int count = points.Count;
                for (int i = startIndex; i < count; i++)
                    skiaPath.LineTo(points[i].X, points[i].Y);
            }

            static void AddBeziers(SKPath skiaPath, IList<PointF> points)
            {
                Debug.Assert(points.Count >= 4);
                for (int i = 1; i < points.Count; i += 3)
                    skiaPath.CubicTo(points[i].X, points[i].Y, points[i + 1].X, points[i + 1].Y, points[i + 2].X, points[i + 2].Y);
            }

            static void AddMockedPoint(SKPath skiaPath)
            {
                var lastPoint = skiaPath.LastPoint;
                skiaPath.LineTo(lastPoint.X + 0.5f, lastPoint.Y + 0.5f);
            }

            #endregion

            if (path == null)
                throw new ArgumentNullException(nameof(path), PublicResources.ArgumentNull);

            var result = new SKPath();
            if (path.IsEmpty)
                return result;

            foreach (Figure figure in path.Figures)
            {
                if (figure.IsEmpty)
                    continue;

                ReadOnlyCollection<PathSegment> segments = figure.Segments;
                PointF lastPoint = segments[0].StartPoint;
                result.MoveTo(lastPoint.X, lastPoint.Y);

                foreach (PathSegment segment in segments)
                {
                    PointF startPoint = segment.StartPoint;
                    bool isStartPointAdded = startPoint == lastPoint;
                    lastPoint = segment.EndPoint;

                    switch (segment)
                    {
                        case LineSegment lineSegment:
                            // cannot use AddPoly because it always starts a new figure
                            IList<PointF> points = lineSegment.Points;
                            int toAddCount = isStartPointAdded ? points.Count - 1 : points.Count;

                            // SkiaSharp ignores single-point figures, so we add a one-pixel long line in this case
                            if (toAddCount > 0)
                                AddLines(result, points, isStartPointAdded ? 1 : 0);
                            else if (segments.Count == 1)
                                AddMockedPoint(result);
                            continue;

                        case BezierSegment bezierSegment:
                            if (!isStartPointAdded)
                                result.LineTo(startPoint.X, startPoint.Y);
                            points = bezierSegment.Points;
                            Debug.Assert(points.Count > 0);
                            if (points.Count > 1)
                                AddBeziers(result, points);
                            else if (figure.Segments.Count == 1)
                                AddMockedPoint(result);
                            continue;

                        case ArcSegment arcSegment:
                            RectangleF bounds = arcSegment.Bounds;
                            float radiusX = arcSegment.RadiusX;
                            float radiusY = arcSegment.RadiusY;

                            // Special case 1: horizontally or vertically flat arc or zero sweep angle - adding the flattened points instead
                            // (SkiaSharp actually handles flat ellipses, but if it consists of a single point, that is not rendered)
                            if (radiusX <= 0.5f || radiusY <= 0.5f || arcSegment.SweepAngle.TolerantIsZero(1e-4f))
                            {
                                points = arcSegment.GetFlattenedPoints();
                                if (points.Count == 1 && segments.Count == 1)
                                    AddMockedPoint(result);
                                else
                                    AddLines(result, points, isStartPointAdded ? 1 : 0);
                                continue;
                            }

                            // Special case 2: full ellipse - only if standalone figure, or start point is at zero angle. We could use MoveTo to move to the actual end point,
                            // but it works only for drawing, whereas filling with EvenOdd rule would not differently than without jumping by MoveTo.
                            if (arcSegment.SweepAngle is 360f && (segments.Count == 1 || arcSegment.StartAngle is 0f))
                            {
                                //if (!isStartPointAdded)
                                //    result.LineTo(startPoint.X, startPoint.Y);
                                result.AddOval(new SKRect(bounds.X, bounds.Y, bounds.Right, bounds.Bottom));
                                continue;
                            }

                            // Special case 3: circular, non-complete arc
                            if (radiusX.Equals(radiusY) && !arcSegment.SweepAngle.TolerantEquals(360f))
                            {
                                result.ArcTo(new SKRect(bounds.X, bounds.Y, bounds.Right, bounds.Bottom), arcSegment.StartAngle, arcSegment.SweepAngle, false);
                                continue;
                            }

                            // General case: elliptical arc or complete ellipse with nonzero start angle - converting to Bézier curves.
                            // Cannot use ArcTo here, because it SkiaSharp interprets the angles differently, and it simply does not draw complete ellipses.
                            if (!isStartPointAdded)
                                result.LineTo(startPoint.X, startPoint.Y);
                            AddBeziers(result, arcSegment.ToBezierPoints());
                            continue;
                    }
                }

                if (figure.IsClosed)
                    result.Close();
            }

            return result;
        }

        #endregion
    }
}
