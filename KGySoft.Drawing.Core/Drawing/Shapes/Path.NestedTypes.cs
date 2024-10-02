#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: Path.NestedTypes.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2024 - All Rights Reserved
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
using System.Diagnostics.CodeAnalysis;
using System.Drawing;

using KGySoft.CoreLibraries;
using KGySoft.Reflection;

#endregion

namespace KGySoft.Drawing.Shapes
{
    partial class Path
    {
        #region Nested classes

        #region Figure class

        private sealed class Figure
        {
            #region Fields

            private readonly List<PathSegment> segments;

            #endregion

            #region Properties

            internal bool IsClosed { get; set; }
            internal bool IsEmpty => segments.Count == 0;

            #endregion

            #region Constructors

            internal Figure() => segments = new List<PathSegment>();

            internal Figure(Figure other, bool close)
            {
                int count = other.segments.Count;
                segments = new List<PathSegment>(count);
                for (int i = 0; i < count; i++)
                    segments.Add(other.segments[i].Clone());

                IsClosed = close || other.IsClosed;
            }

            #endregion

            #region Methods

            internal void AddSegment(PathSegment segment)
            {
                segments.Add(segment);
            }

            internal IList<PointF> GetPoints()
            {
                switch (segments.Count)
                {
                    case 0:
                        return Reflector.EmptyArray<PointF>();
                    case 1:
                        return segments[0].GetPoints();
                    default:
                        var result = new List<PointF>();
                        foreach (PathSegment segment in segments)
                            result.AddRange(segment.GetPoints());
                        return result;
                }
            }

            internal void Transform(TransformationMatrix matrix)
            {
                foreach (PathSegment segment in segments)
                    segment.Transform(matrix);
            }

            #endregion
        }

        #endregion

        #region PathSegment class

        private abstract class PathSegment
        {
            #region Methods
            
            internal abstract IList<PointF> GetPoints();
            internal abstract void Transform(TransformationMatrix matrix);
            internal abstract PathSegment Clone();

            #endregion
        }

        #endregion

        #region LineSegment class

        private sealed class LineSegment : PathSegment
        {
            #region Fields

            private readonly PointF[] points;

            #endregion

            #region Constructors

            internal LineSegment(params PointF[] points)
            {
                Debug.Assert(points != null! && points.Length >= 2, "points.Length should be >= 2");
                this.points = points!;
            }

            #endregion

            #region Methods

            internal override IList<PointF> GetPoints() => points;

            internal override void Transform(TransformationMatrix matrix)
            {
                Debug.Assert(!matrix.IsIdentity);
                int len = points.Length;
                for (int i = 0; i < len; i++)
                    points[i] = points[i].Transform(matrix);
            }

            internal override PathSegment Clone() => new LineSegment((PointF[])points.Clone());

            #endregion
        }

        #endregion

        #region BezierSegment class

        private sealed class BezierSegment : PathSegment
        {
            #region Constants

            private const float flatnessThreshold = 1f / 64f;
            private const int flattenRecursionLimit = 16;
            private const float tolerance = 1e-4f;

            #endregion

            #region Fields

            private readonly IList<PointF> points;

            #endregion

            #region Constructors

            internal BezierSegment(IList<PointF> points)
            {
                Debug.Assert(points != null! && (points.Count - 1) % 3 == 0);
                this.points = points!;
            }

            #endregion

            #region Methods

            #region Static Methods

            #region Internal Methods

            internal static BezierSegment FromArc(RectangleF bounds, float startAngle, float sweepAngle)
            {
                Debug.Assert(bounds.Width > 0f && bounds.Height > 0f);

                if (Math.Abs(sweepAngle) >= 360f)
                    return FromEllipse(bounds);

                // up to 4 arcs, meaning 4, 7, 10 or 13 Bézier points
                var result = new List<PointF>(13);

                float completed = 0f;
                bool finished = false;

                float endAngle = startAngle + sweepAngle;
                float increment = (endAngle < startAngle) ? -90f : 90f;

                while (!finished)
                {
                    float currentStart = startAngle + completed;
                    float currentEnd = endAngle - currentStart;
                    if (Math.Abs(currentEnd) > 90f)
                        currentEnd = increment;
                    else
                    {
                        // for very small remaining section breaking without actually adding it
                        if (currentEnd.TolerantIsZero(tolerance))
                            break;

                        finished = true;
                    }

                    ArcToBezier(bounds, currentStart, currentStart + currentEnd, result);
                    completed += currentEnd;
                }


                return new BezierSegment(result);
            }

            internal static BezierSegment FromEllipse(RectangleF bounds)
            {
                const float c1 = 0.552285f;
                float radiusX = bounds.Width / 2;
                float radiusY = bounds.Height / 2;
                float centerX = bounds.X + radiusX;
                float centerY = bounds.Y + radiusY;

                // 4 Bézier curves (1 + 3 * 4 points)
                return new BezierSegment(new[]
                {
                    // 1st quadrant
                    new PointF(centerX + radiusX, centerY),
                    new PointF(centerX + radiusX, centerY - c1 * radiusY),
                    new PointF(centerX + c1 * radiusX, centerY - radiusY),
                    new PointF(centerX, centerY - radiusY),

                    // 2nd quadrant
                    new PointF(centerX - c1 * radiusX, centerY - radiusY),
                    new PointF(centerX - radiusX, centerY - c1 * radiusY),
                    new PointF(centerX - radiusX, centerY),

                    // 3rd quadrant
                    new PointF(centerX - radiusX, centerY + c1 * radiusY),
                    new PointF(centerX - c1 * radiusX, centerY + radiusY),
                    new PointF(centerX, centerY + radiusY),

                    // 4th quadrant
                    new PointF(centerX + c1 * radiusX, centerY + radiusY),
                    new PointF(centerX + radiusX, centerY + c1 * radiusY),
                    new PointF(centerX + radiusX, centerY)
                });
            }

            #endregion

            #region Private Methods

            // This method originates from mono/libgdiplus (MIT license): https://github.com/mono/libgdiplus/blob/94a49875487e296376f209fe64b921c6020f74c0/src/graphics-path.c#L736
            // Main changes: converting to C#, floats everywhere, using vectors if possible.
            private static void ArcToBezier(RectangleF bounds, float startAngle, float endAngle, List<PointF> result)
            {
                float radiusX = bounds.Width / 2f;
                float radiusY = bounds.Height / 2f;

                float centerX = bounds.X + radiusX;
                float centerY = bounds.Y + radiusY;

                float start = startAngle.ToRadian();
                float end = endAngle.ToRadian();

                // The result of Atan2 is not in the correct quadrant when Atan2 is called with x == 0 and y != 0, so we may need to adjust it.
                // We could also do something similar to the ReactOS solution: https://github.com/reactos/reactos/blob/3dfbe526992849cf53a83fae784be2126319150b/dll/win32/gdiplus/gdiplus.c#L201
                start = MathF.Atan2(radiusX * MathF.Sin(start), radiusY * MathF.Cos(start));
                end = MathF.Atan2(radiusX * MathF.Sin(end), radiusY * MathF.Cos(end));

                if (Math.Abs(end - start) > MathF.PI)
                {
                    if (end > start)
                        end -= 2f * MathF.PI;
                    else
                        start -= 2f * MathF.PI;
                }

                float mid = (end - start) / 2f;
                float controlPoint = 4f / 3f * (1f - MathF.Cos(mid)) / MathF.Sin(mid);

                float sinStart = MathF.Sin(start);
                float sinEnd = MathF.Sin(end);
                float cosStart = MathF.Cos(start);
                float cosEnd = MathF.Cos(end);

                // adding starting point only if we don't have a previous end point
                if (result.Count == 0)
                {
                    float startX = centerX + radiusX * cosStart;
                    float startY = centerY + radiusY * sinStart;
                    result.Add(new PointF(startX, startY));
                }
                 
                result.Add(new PointF(centerX + radiusX * (cosStart - controlPoint * sinStart), centerY + radiusY * (sinStart + controlPoint * cosStart)));
                result.Add(new PointF(centerX + radiusX * (cosEnd + controlPoint * sinEnd), centerY + radiusY * (sinEnd - controlPoint * cosEnd)));
                result.Add(new PointF(centerX + radiusX * cosEnd, centerY + radiusY * sinEnd));
            }

            // This algorithm was inspired by the nr_curve_flatten method from the mono/libgdiplus project: https://github.com/mono/libgdiplus/blob/94a49875487e296376f209fe64b921c6020f74c0/src/graphics-path.c#L1612
            // which they took from Sodipodi's libnr project (nr-svp.c/nr_svl_build_curveto method): https://web.archive.org/web/20070305000912/http://www.sodipodi.com/files/sodipodi-0.33-beta.tar.gz
            // Former is under the MIT License, the latter is simply noted as being in the "public domain" and was written by Lauris Kaplinski.
            // Main changes: refactored control flow, more descriptive variable names, simply just omitting subdivision when reaching the recursion limit, using vectors when possible.
            [SuppressMessage("ReSharper", "TailRecursiveCall", Justification = "Could remove only one of the two recursions and would make the code messier.")]
            private static void FlattenBezierCurve(PointF start, PointF controlPoint1, PointF controlPoint2, PointF end, int level, List<PointF> result)
            {
                // Recursion limit reached: the original libgdiplus code returned false in this case (actually not here but before the subdivision),
                // causing a revert in the caller, whereas the Sodipodi code had no built-in limit. We just go on without subdivision in such case.
                if (level == flattenRecursionLimit)
                {
                    result.Add(new PointF(end.X, end.Y));
                    return;
                }

                float diffCtrl1StartX = controlPoint1.X - start.X;
                float diffCtrl1StartY = controlPoint1.Y - start.Y;
                float diffCtrl2StartX = controlPoint2.X - start.X;
                float diffCtrl2StartY = controlPoint2.Y - start.Y;
                float diffEndStartX = end.X - start.X;
                float diffEndStartY = end.Y - start.Y;
                float diffEndCtrl2X = end.X - controlPoint2.X;
                float diffEndCtrl2Y = end.Y - controlPoint2.Y;
                float sqrDistStartEnd = diffEndStartX * diffEndStartX + diffEndStartY * diffEndStartY;

                if (sqrDistStartEnd <= flatnessThreshold)
                {
                    float sqrDistStartCtrl1 = diffCtrl1StartX * diffCtrl1StartX + diffCtrl1StartY * diffCtrl1StartY;
                    float sqrDistStartCtrl2 = diffCtrl2StartX * diffCtrl2StartX + diffCtrl2StartY * diffCtrl2StartY;

                    // No need to subdivide if the control points are close enough
                    if ((sqrDistStartCtrl1 < flatnessThreshold) && (sqrDistStartCtrl2 < flatnessThreshold))
                    {
                        result.Add(new PointF(end.X, end.Y));
                        return;
                    }
                }
                else
                {
                    float subdivideThreshold = flatnessThreshold * sqrDistStartEnd;

                    // Calculating dot and cross products for the control points and the start/end points
                    float dotStartCtrl1StartEnd = diffCtrl1StartX * diffEndStartX + diffCtrl1StartY * diffEndStartY;
                    float crossStartCtrl1StartEnd = diffCtrl1StartY * diffEndStartX - diffCtrl1StartX * diffEndStartY;
                    float dotStartCtrl2StartEnd = diffCtrl2StartX * diffEndStartX + diffCtrl2StartY * diffEndStartY;
                    float crossStartCtrl2StartEnd = diffCtrl2StartY * diffEndStartX - diffCtrl2StartX * diffEndStartY;
                    float dotCtrl2EndStartEnd = diffEndCtrl2X * diffEndStartX + diffEndCtrl2Y * diffEndStartY;

                    // If no need to subdivide, simply just adding the end point
                    if ((crossStartCtrl1StartEnd * crossStartCtrl1StartEnd) <= subdivideThreshold
                        && (crossStartCtrl2StartEnd * crossStartCtrl2StartEnd) <= subdivideThreshold
                        && (dotStartCtrl1StartEnd >= 0f || dotStartCtrl1StartEnd * dotStartCtrl1StartEnd >= subdivideThreshold)
                        && (dotCtrl2EndStartEnd >= 0f || dotCtrl2EndStartEnd * dotCtrl2EndStartEnd >= subdivideThreshold)
                        && dotStartCtrl1StartEnd < dotStartCtrl2StartEnd)
                    {
                        result.Add(new PointF(end.X, end.Y));
                        return;
                    }
                }

                // Calculating the midpoints for subdivision
                float midStartCtrl1X = (start.X + controlPoint1.X) * 0.5f;
                float midStartCtrl1Y = (start.Y + controlPoint1.Y) * 0.5f;
                float midStartCtrl1Ctrl2X = (start.X + 2 * controlPoint1.X + controlPoint2.X) * 0.25f;
                float midStartCtrl1Ctrl2Y = (start.Y + 2 * controlPoint1.Y + controlPoint2.Y) * 0.25f;
                float midCtrl1Ctrl2EndX = (controlPoint1.X + controlPoint2.X * 2f + end.X) * 0.25f;
                float midCtrl1Ctrl2EndY = (controlPoint1.Y + controlPoint2.Y * 2f + end.Y) * 0.25f;
                float midCtrl2EndX = (controlPoint2.X + end.X) * 0.5f;
                float midCtrl2EndY = (controlPoint2.Y + end.Y) * 0.5f;
                float midAllX = (midStartCtrl1Ctrl2X + midCtrl1Ctrl2EndX) * 0.5f;
                float midAllY = (midStartCtrl1Ctrl2Y + midCtrl1Ctrl2EndY) * 0.5f;

                // Going on with recursive subdivision
                FlattenBezierCurve(new PointF(start.X, start.Y), new PointF(midStartCtrl1X, midStartCtrl1Y), new PointF(midStartCtrl1Ctrl2X, midStartCtrl1Ctrl2Y), new PointF(midAllX, midAllY), level + 1, result);
                FlattenBezierCurve(new PointF(midAllX, midAllY), new PointF(midCtrl1Ctrl2EndX, midCtrl1Ctrl2EndY), new PointF(midCtrl2EndX, midCtrl2EndY), new PointF(end.X, end.Y), level + 1, result);
            }

            #endregion

            #endregion

            #region Instance Methods

            internal override IList<PointF> GetPoints()
            {
                Debug.Assert((points.Count - 1) % 3 == 0);
                var result = new List<PointF>();

                // Converting the Bézier segments one by one. The last point of a segment is the first point of the next segment.
                int len = points.Count;
                for (int i = 1; i < len; i += 3)
                    FlattenBezierCurve(points[i - 1], points[i], points[i + 1], points[i + 2], 0, result);

                return result;
            }

            internal override void Transform(TransformationMatrix matrix)
            {
                Debug.Assert(!matrix.IsIdentity);
                int len = points.Count;
                for (int i = 0; i < len; i++)
                    points[i] = points[i].Transform(matrix);
            }

            internal override PathSegment Clone() => new BezierSegment(((IList<PointF>?)(points as PointF[])?.Clone()) ?? new List<PointF>(points));

            #endregion

            #endregion
        }

        #endregion

        #endregion
    }
}