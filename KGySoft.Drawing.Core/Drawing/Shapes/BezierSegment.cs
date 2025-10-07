#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BezierSegment.cs
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
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
using System.Numerics;
#endif
using System.Runtime.CompilerServices;

#endregion

namespace KGySoft.Drawing.Shapes
{
    /// <summary>
    /// Represents a path segment in a <see cref="Figure"/>, consisting of zero, one, or more cubic Bézier curves.
    /// </summary>
    /// <remarks>
    /// <note>This class is meant to provide information about a series of cubic Bézier segments in a <see cref="Figure"/> for interoperability with other libraries.
    /// To add new figures or path segments to a <see cref="Path"/>, use its public <see cref="Path.StartFigure">StartFigure</see> and <c>Add...</c> methods instead.</note>
    /// </remarks>
    [SuppressMessage("ReSharper", "UseIndexFromEndExpression", Justification = "Targeting older frameworks that don't support indexing from end.")]
    public sealed class BezierSegment : PathSegment
    {
        #region Constants

        private const float flatnessThreshold = 1f / 64f;
        private const int flattenRecursionLimit = 16;

        #endregion

        #region Fields

        private readonly IList<PointF> points;
        
        private List<PointF>? flattenedPoints;

        #endregion

        #region Properties
        
        #region Public Properties

        /// <summary>
        /// Gets the start point of this <see cref="BezierSegment"/>.
        /// </summary>
        public override PointF StartPoint => points[0];

        /// <summary>
        /// Gets the end point of this <see cref="BezierSegment"/>.
        /// </summary>
        public override PointF EndPoint => points[points.Count - 1];

        /// <summary>
        /// Gets a read-only collection of the points that define this <see cref="BezierSegment"/>.
        /// It always contains 1 + 3n points, where n is the number of cubic Bézier curves in this segment (n can be zero).
        /// </summary>
        public ReadOnlyCollection<PointF> Points => new(points);

        #endregion

        #region Internal Properties

        internal IList<PointF> PointsInternal => points;

        #endregion

        #endregion

        #region Constructors

        internal BezierSegment(IList<PointF> points, bool copy)
        {
            Debug.Assert((points.Count - 1) % 3 == 0);
            this.points = copy ? new List<PointF>(points) : points;
        }

        #endregion

        #region Methods

        #region Static Methods

        #region Internal Methods

        internal static BezierSegment FromArc(RectangleF bounds, float startAngle, float sweepAngle)
        {
            if (Math.Abs(sweepAngle) >= 360f && startAngle is 0f)
                return FromEllipse(bounds);

            float radiusX = bounds.Width / 2f;
            float radiusY = bounds.Height / 2f;
            var center = new PointF(bounds.X + radiusX, bounds.Y + radiusY);
            radiusX = Math.Abs(radiusX);
            radiusY = Math.Abs(radiusY);
            ArcSegment.NormalizeAngle(ref startAngle);
            if (Math.Abs(sweepAngle) >= 360f)
                sweepAngle = 360f;
            (float startRadian, float endRadian) = (startAngle, startAngle + sweepAngle);

            startRadian = startRadian.ToRadian();
            endRadian = endRadian.ToRadian();
            ArcSegment.ToEllipseCoordinates(ref startRadian, ref endRadian, radiusX, radiusY);
            return FromArc(center, radiusX, radiusY, startRadian, endRadian - startRadian);
        }

        internal static BezierSegment FromArc(PointF centerPoint, float radiusX, float radiusY, float startRad, float sweepRad)
            => new(GetBezierPointsFromArc(centerPoint, radiusX, radiusY, startRad, sweepRad), false);

        internal static List<PointF> GetBezierPointsFromArc(PointF centerPoint, float radiusX, float radiusY, float startRad, float sweepRad, float rotationRad = 0f)
        {
            int segments = ((int)MathF.Ceiling(Math.Abs(sweepRad) / (MathF.PI / 2f))).Clip(1, 4);
            float segmentAngle = sweepRad / segments;

            var result = new List<PointF>(segments * 3 + 1);
            segments = Math.Max(1, segments); // just in case of very small sweeps
            for (int i = 0; i < segments; i++)
            {
                float segmentStart = startRad + i * segmentAngle;
                float segmentEnd = segmentStart + segmentAngle;
                AppendArcSegment(centerPoint, radiusX, radiusY, segmentStart, segmentEnd, rotationRad, result);
            }

            Debug.Assert((result.Count - 1) % 3 == 0);
            return result;
        }

        internal static BezierSegment FromEllipse(RectangleF bounds)
        {
            float radiusX = bounds.Width / 2f;
            float radiusY = bounds.Height / 2f;
            return FromEllipse(new PointF(bounds.X + radiusX, bounds.Y + radiusY), Math.Abs(radiusX), Math.Abs(radiusY));
        }

        internal static List<PointF> GetBezierPointsFromEllipse(PointF centerPoint, float radiusX, float radiusY)
        {
            const float c1 = 0.5522848f; // 4/3 * (sqrt(2) - 1)
            float centerX = centerPoint.X;
            float centerY = centerPoint.Y;
            float ctrlPointX = c1 * radiusX;
            float ctrlPointY = c1 * radiusY;

            // 4 Bézier curves (1 + 3 * 4 points)
            return
            [
                // 1st quadrant
                new PointF(centerX + radiusX, centerY),
                new PointF(centerX + radiusX, centerY - ctrlPointY),
                new PointF(centerX + ctrlPointX, centerY - radiusY),
                new PointF(centerX, centerY - radiusY),

                // 2nd quadrant
                new PointF(centerX - ctrlPointX, centerY - radiusY),
                new PointF(centerX - radiusX, centerY - ctrlPointY),
                new PointF(centerX - radiusX, centerY),

                // 3rd quadrant
                new PointF(centerX - radiusX, centerY + ctrlPointY),
                new PointF(centerX - ctrlPointX, centerY + radiusY),
                new PointF(centerX, centerY + radiusY),

                // 4th quadrant
                new PointF(centerX + ctrlPointX, centerY + radiusY),
                new PointF(centerX + radiusX, centerY + ctrlPointY),
                new PointF(centerX + radiusX, centerY)
            ];
        }

        internal static BezierSegment FromEllipse(PointF centerPoint, float radiusX, float radiusY)
            => new(GetBezierPointsFromEllipse(centerPoint, radiusX, radiusY), false);

        internal static void ControlPointsFromQuadratic(PointF start, PointF quadControlPoint, PointF end, out PointF cubicControlPoint1, out PointF cubicControlPoint2)
        {
#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
            ref Vector2 startVec = ref start.AsVector2();
            ref Vector2 cpVec = ref quadControlPoint.AsVector2();
            ref Vector2 endVec = ref end.AsVector2();
            Vector2 cp1Vec = startVec + 2f / 3f * (cpVec - startVec);
            cubicControlPoint1 = cp1Vec.AsPointF();
            Vector2 cp2Vec = endVec + 2f / 3f * (cpVec - endVec);
            cubicControlPoint2 = cp2Vec.AsPointF();
#else
            cubicControlPoint1 = new PointF(start.X + 2f / 3f * (quadControlPoint.X - start.X),
                start.Y + 2f / 3f * (quadControlPoint.Y - start.Y));
            cubicControlPoint2 = new PointF(end.X + 2f / 3f * (quadControlPoint.X - end.X),
                end.Y + 2f / 3f * (quadControlPoint.Y - end.Y));
#endif
        }

        internal static void ControlPointsFromConic(PointF start, PointF conicControlPoint, PointF end, float weight, out PointF cubicControlPoint1, out PointF cubicControlPoint2)
        {
            // Credit to this paper where I managed to find the solution: https://www.mn.uio.no/math/english/people/aca/michaelf/papers/g4.pdf
            float lambda = 4f / 3f * weight / (1f + weight);

            // Instead of checking weight, we check the resulting lambda, because lambda can be infinite for very large weights as well
            if (Single.IsInfinity(lambda) || Single.IsNaN(lambda) || lambda < 0f)
                throw new ArgumentOutOfRangeException(nameof(weight), PublicResources.ArgumentOutOfRange);

            float inverseLambda = 1 - lambda;

#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
            ref Vector2 cpVec = ref conicControlPoint.AsVector2();
            Vector2 cp1Vec = inverseLambda * start.AsVector2() + lambda * cpVec;
            cubicControlPoint1 = cp1Vec.AsPointF();
            Vector2 cp2Vec = inverseLambda * end.AsVector2() + lambda * cpVec;
            cubicControlPoint2 = cp2Vec.AsPointF();
#else
            cubicControlPoint1 = new PointF(inverseLambda * start.X + lambda * conicControlPoint.X,
                inverseLambda * start.Y + lambda * conicControlPoint.Y);
            cubicControlPoint2 = new PointF(inverseLambda * end.X + lambda * conicControlPoint.X,
                inverseLambda * end.Y + lambda * conicControlPoint.Y);
#endif
        }

        #endregion

        #region Private Methods

        // This method originates from mono/libgdiplus (MIT license): https://github.com/mono/libgdiplus/blob/94a49875487e296376f209fe64b921c6020f74c0/src/graphics-path.c#L736
        // Main changes: converting to C#, originating from center+radius instead of bounds, angles are already in radians adjusted to ellipse coordinates, adding rotation support
        private static void AppendArcSegment(PointF center, float radiusX, float radiusY, float startRad, float endRad, float rotationRad, List<PointF> result)
        {
            #region Local Methods

            [MethodImpl(MethodImpl.AggressiveInlining)]
            static PointF MapFromEllipseSpace(float ux, float uy, float radiusX, float radiusY, float cosPhi, float sinPhi, PointF center)
            {
                float x = radiusX * ux;
                float y = radiusY * uy;
                return new PointF(cosPhi * x - sinPhi * y + center.X, sinPhi * x + cosPhi * y + center.Y);
            }

            #endregion

            float sweepRad = endRad - startRad;

            // The original formula returns the same, though it was more complex, and did not tolerate small sweeps well
            // 4f / 3f * (1f - MathF.Cos(sweepRad / 2f)) / MathF.Sin(sweepRad / 2f);
            float controlLengthFactor = (4f / 3f) * MathF.Tan(sweepRad / 4f);

            float sinStart = MathF.Sin(startRad);
            float sinEnd = MathF.Sin(endRad);
            float cosStart = MathF.Cos(startRad);
            float cosEnd = MathF.Cos(endRad);

            // splitting just for performance reasons: if rotation is 0, we can skip some calculations
            if (rotationRad is 0f)
            {
                // adding starting point only if we don't have a previous end point
                if (result.Count == 0)
                {
                    result.Add(new PointF(center.X + radiusX * cosStart, center.Y + radiusY * sinStart));
                    if (startRad.Equals(endRad))
                        return;
                }

                result.Add(new PointF(center.X + radiusX * (cosStart - controlLengthFactor * sinStart), center.Y + radiusY * (sinStart + controlLengthFactor * cosStart)));
                result.Add(new PointF(center.X + radiusX * (cosEnd + controlLengthFactor * sinEnd), center.Y + radiusY * (sinEnd - controlLengthFactor * cosEnd)));
                result.Add(new PointF(center.X + radiusX * cosEnd, center.Y + radiusY * sinEnd));
                return;
            }

            float cosPhi = MathF.Cos(rotationRad);
            float sinPhi = MathF.Sin(rotationRad);

            // adding starting point only if we don't have a previous end point
            if (result.Count == 0)
            {
                result.Add(MapFromEllipseSpace(cosStart, sinStart, radiusX, radiusY, cosPhi, sinPhi, center));
                if (startRad.Equals(endRad))
                    return;
            }

            result.Add(MapFromEllipseSpace(cosStart - controlLengthFactor * sinStart, sinStart + controlLengthFactor * cosStart, radiusX, radiusY, cosPhi, sinPhi, center));
            result.Add(MapFromEllipseSpace(cosEnd + controlLengthFactor * sinEnd, sinEnd - controlLengthFactor * cosEnd, radiusX, radiusY, cosPhi, sinPhi, center));
            result.Add(MapFromEllipseSpace(cosEnd, sinEnd, radiusX, radiusY, cosPhi, sinPhi, center));
        }

        // This algorithm was inspired by the nr_curve_flatten method from the mono/libgdiplus project: https://github.com/mono/libgdiplus/blob/94a49875487e296376f209fe64b921c6020f74c0/src/graphics-path.c#L1612
        // which they took from Sodipodi's libnr project (nr-svp.c/nr_svl_build_curveto method): https://web.archive.org/web/20070305000912/http://www.sodipodi.com/files/sodipodi-0.33-beta.tar.gz
        // Former is under the MIT License, the latter is simply noted as being in the "public domain" and was written by Lauris Kaplinski.
        // Main changes: refactored control flow, more descriptive variable names, simply just omitting subdivision when reaching the recursion limit, using vectors when possible (TODO).
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

        internal bool TryAppend(IList<PointF> newPoints)
        {
            Debug.Assert(points is List<PointF>, "TryAppend is expected to be called only when the segment was created with a list or copy = true");
            Debug.Assert(newPoints.Count > 0 && (points.Count - 1) % 3 == 0);

             // can only be appended if the first new point is the same as the last point of the existing segment
            if (points[points.Count - 1] != newPoints[0])
                return false;

            if (newPoints.Count > 1)
            {
                ((List<PointF>)points).AddRange(newPoints.Skip(1));
                flattenedPoints = null;
            }

            return true;
        }

        internal override IList<PointF> GetFlattenedPointsInternal()
        {
            if (flattenedPoints is List<PointF> result)
                return result;

            Debug.Assert((points.Count - 1) % 3 == 0);
            result = new List<PointF>(points.Count) { points[0] };

            // Converting the Bézier segments one by one. The last point of a segment is the first point of the next segment.
            int len = points.Count;
            for (int i = 1; i < len; i += 3)
                FlattenBezierCurve(points[i - 1], points[i], points[i + 1], points[i + 2], 0, result);

            return flattenedPoints = result;
        }

        internal override PathSegment Transform(TransformationMatrix matrix)
        {
            Debug.Assert(!matrix.IsIdentity);
            flattenedPoints = null;
            int len = points.Count;
            for (int i = 0; i < len; i++)
                points[i] = matrix.Transform(points[i]);

            return this;
        }

        internal override PathSegment Clone() => new BezierSegment(points, true);

        #endregion

        #endregion
    }
}
