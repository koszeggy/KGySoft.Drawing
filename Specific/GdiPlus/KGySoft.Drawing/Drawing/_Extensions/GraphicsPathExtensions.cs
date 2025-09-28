#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: GraphicsPathExtensions.cs
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
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

using KGySoft.CoreLibraries;
using KGySoft.Drawing.Shapes;
#if NET
using System.Runtime.Versioning;
#endif

#endregion

namespace KGySoft.Drawing
{
    /// <summary>
    /// Contains extension methods for the <see cref="GraphicsPath"/> class.
    /// </summary>
#if NET7_0_OR_GREATER
    [SupportedOSPlatform("windows")]
#endif
    public static class GraphicsPathExtensions
    {
        #region Nested Enumerations

        [Flags]
        private enum PathType
        {
            Start = 0,
            Line = 1,
            Bezier = 3,
            TypeMask = 7,
            Close = 1 << 7,
        }

        #endregion

        #region Constants

        private const float tolerance = 1e-4f;

        #endregion

        #region Methods

        /// <summary>
        /// Adds a rounded rectangle to this <see cref="GraphicsPath"/>, applying the same corner radius to all corners.
        /// </summary>
        /// <param name="path">The <see cref="GraphicsPath"/> instance to add the rounded rectangle to.</param>
        /// <param name="bounds">The bounding rectangle that defines the rounded rectangle.</param>
        /// <param name="cornerRadius">The size of the corner radius of the rounded rectangle for all corners.</param>
        /// <remarks>
        /// <para>The rounded rectangle is added as a new closed figure.</para>
        /// <para>If <paramref name="cornerRadius"/> is negative, the absolute value will be used. If it is greater than the half of the smaller side of the bounding rectangle,
        /// it will be adjusted to the half of the smaller side, so the result will be an oval shape. If the <paramref name="cornerRadius"/> is 0, a simple rectangle will be added.</para>
        /// </remarks>
        [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator", Justification = "Intended. Even very small differences are handled properly.")]
        public static void AddRoundedRectangle(this GraphicsPath path, RectangleF bounds, float cornerRadius)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path), PublicResources.ArgumentNull);

            if (cornerRadius == 0f)
            {
                path.AddRectangle(bounds);
                return;
            }

            bounds.Normalize();
            path.StartFigure();
            float diameter = Math.Min(Math.Abs(cornerRadius) * 2f, Math.Min(Math.Abs(bounds.Width), Math.Abs(bounds.Height)));
            var arc = new RectangleF(bounds.Location, new SizeF(diameter, diameter));

            // top left arc
            path.AddArc(arc, 180, 90);

            // top right arc
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);

            // bottom right arc
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);

            // bottom left arc
            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
        }

        /// <summary>
        /// Adds a rounded rectangle to this <see cref="GraphicsPath"/>, applying a custom corner radius to each corner.
        /// </summary>
        /// <param name="path">The <see cref="GraphicsPath"/> instance to add the rounded rectangle to.</param>
        /// <param name="bounds">The bounding rectangle that defines the rounded rectangle.</param>
        /// <param name="radiusTopLeft">The size of the top-left corner radius of the rounded rectangle.</param>
        /// <param name="radiusTopRight">The size of the top-right corner radius of the rounded rectangle.</param>
        /// <param name="radiusBottomRight">The size of the bottom-right corner radius of the rounded rectangle.</param>
        /// <param name="radiusBottomLeft">The size of the bottom-left corner radius of the rounded rectangle.</param>
        /// <remarks>
        /// <para>The rounded rectangle is added as a new closed figure.</para>
        /// <para>If any of the corner radius parameters is negative, the absolute value will be used. If the sum of any adjacent corner radius parameters is greater
        /// than the corresponding side of the bounding rectangle, then all corner radius parameters will be scaled down proportionally to fit into the bounding rectangle.</para>
        /// </remarks>
        [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator", Justification = "Intended. Even very small differences are handled properly.")]
        public static void AddRoundedRectangle(this GraphicsPath path, RectangleF bounds, float radiusTopLeft, float radiusTopRight, float radiusBottomRight, float radiusBottomLeft)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path), PublicResources.ArgumentNull);

            bounds.Normalize();
            path.StartFigure();

            // Adjusting radii to the bounds of they are too large
            radiusTopLeft = Math.Abs(radiusTopLeft);
            radiusTopRight = Math.Abs(radiusTopRight);
            radiusBottomRight = Math.Abs(radiusBottomRight);
            radiusBottomLeft = Math.Abs(radiusBottomLeft);
            float maxDiameterWidth = Math.Max(radiusTopLeft + radiusTopRight, radiusBottomLeft + radiusBottomRight);
            float maxDiameterHeight = Math.Max(radiusTopLeft + radiusBottomLeft, radiusTopRight + radiusBottomRight);
            if (maxDiameterWidth > bounds.Width || maxDiameterHeight > bounds.Height)
            {
                float scale = Math.Min(bounds.Width / maxDiameterWidth, bounds.Height / maxDiameterHeight);
                radiusTopLeft *= scale;
                radiusTopRight *= scale;
                radiusBottomRight *= scale;
                radiusBottomLeft *= scale;
            }

            // top left arc
            var corner = new RectangleF(bounds.Location, new SizeF(radiusTopLeft * 2f, radiusTopLeft * 2f));
            if (radiusTopLeft > 0f)
                path.AddArc(corner, 180f, 90f);
            else
                path.AddLine(corner.Location, corner.Location);

            // top right
            if (radiusTopRight != radiusTopLeft)
                corner.Size = new SizeF(radiusTopRight * 2f, radiusTopRight * 2f);
            corner.X = bounds.Right - corner.Width;
            if (radiusTopRight > 0f)
                path.AddArc(corner, 270f, 90f);
            else
                path.AddLine(corner.Location, corner.Location);

            // bottom right arc
            if (radiusBottomRight != radiusTopRight)
            {
                corner.Size = new SizeF(radiusBottomRight * 2f, radiusBottomRight * 2f);
                corner.X = bounds.Right - corner.Width;
            }

            corner.Y = bounds.Bottom - corner.Height;
            if (radiusBottomRight > 0f)
                path.AddArc(corner, 0f, 90f);
            else
                path.AddLine(corner.Location, corner.Location);

            // bottom left arc
            if (radiusBottomLeft != radiusBottomRight)
            {
                corner.Size = new SizeF(radiusBottomLeft * 2f, radiusBottomLeft * 2f);
                corner.Y = bounds.Bottom - corner.Height;
            }

            corner.X = bounds.Left;
            if (radiusBottomLeft > 0f)
                path.AddArc(corner, 90f, 90f);
            else
                path.AddLine(corner.Location, corner.Location);

            path.CloseFigure();
        }

        /// <summary>
        /// Converts a <see cref="GraphicsPath"/> instance to a <see cref="Path"/>.
        /// </summary>
        /// <param name="path">The <see cref="GraphicsPath"/> instance to convert to a <see cref="Path"/>.</param>
        /// <returns>A <see cref="Path"/> instance that represents the same geometry as the specified <see cref="GraphicsPath"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <see langword="null"/>.</exception>
        public static Path ToPath(this GraphicsPath path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path), PublicResources.ArgumentNull);

            var result = new Path();
            int count = path.PointCount;
            if (count == 0)
                return result;

            PointF[] points = path.PathPoints;
            byte[] types = path.PathTypes;
            PathType currentType = PathType.Start;
            PathType nextType = currentType;

            int segmentStart = 0;

            while (segmentStart < count)
            {
                int segmentEnd = segmentStart;

                // Finding the end of the current segment and determining the type
                while (segmentEnd < count)
                {
                    // Current segment ends if...
                    // 1.) the figure is closed
                    if (((PathType)types[segmentEnd] & PathType.Close) == PathType.Close)
                        break;

                    // 2.) there are no more points
                    if (segmentEnd + 1 == count)
                        break;

                    // 3.) next point is a new start point
                    nextType = (PathType)types[segmentEnd + 1] & PathType.TypeMask;
                    if (nextType == PathType.Start)
                        break;

                    if (currentType == PathType.Start)
                        currentType = nextType;

                    // 4.) the type changes
                    if (nextType != currentType)
                        break;

                    segmentEnd += 1;
                }

                // Adding the current segment
                if (((PathType)types[segmentStart] & PathType.TypeMask) == PathType.Start)
                    result.StartFigure();
                switch (currentType)
                {
                    case PathType.Line:
                        result.AddLines(points.AsSection(segmentStart, segmentEnd - segmentStart + 1));
                        break;
                    case PathType.Bezier:
                        result.AddBeziers(points.AsSection(segmentStart, segmentEnd - segmentStart + 1));
                        break;
                    default:
                        // Actually KGySoft Path supports single points but GDI+ ignores them, so skipping them.
                        Debug.Assert(currentType == PathType.Start && segmentEnd == segmentStart);
                        break;
                }

                if (((PathType)types[segmentEnd] & PathType.Close) == PathType.Close)
                {
                    result.CloseFigure();
                    currentType = PathType.Start;
                    segmentStart =  segmentEnd + 1;
                    continue;
                }

                if (segmentEnd + 1 == count)
                    break;

                // Start: if next type is also a start here, then the current point has undefined type, so skipping it
                // Line [after a Bézier segment]: the line is a continuation, so no need to repeat the last point the Bézier segment, it will be connected automatically
                // Bezier [after a Line segment]: though the new Bézier segment is a continuation here, we must repeat the last point of the previous line segment as the start point of the Bézier segment
                currentType = nextType;
                segmentStart = currentType is PathType.Start or PathType.Line ? segmentEnd + 1 : segmentEnd;
            }

            return result;
        }

        /// <summary>
        /// Converts a <see cref="Path"/> instance to a <see cref="GraphicsPath"/>.
        /// </summary>
        /// <param name="path">The <see cref="Path"/> instance to convert to a <see cref="GraphicsPath"/>.</param>
        /// <returns>A <see cref="GraphicsPath"/> instance that represents the same geometry as the specified <see cref="Path"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <see langword="null"/>.</exception>
        public static GraphicsPath ToGraphicsPath(this Path path)
        {
            #region Local Methods

            static void AddLines(GraphicsPath graphicsPath, IList<PointF> points)
            {
                switch (points.Count)
                {
                    case 0:
                        Debug.Fail("A KGy SOFT LineSegment should contain at least one point");
                        return;
                    case 2:
                        graphicsPath.AddLine(points[0], points[1]);
                        break;
                    default:
                        graphicsPath.AddLines(points as PointF[] ?? points.ToArray());
                        break;
                }
            }

            static void AddBeziers(GraphicsPath graphicsPath, IList<PointF> points)
            {
                switch (points.Count)
                {
                    case 1:
                        // Matters only if the segment is connected to other segments; otherwise, GDI+ ignores it.
                        graphicsPath.AddLines([points[0]]);
                        break;
                    case 4:
                        // just to avoid array allocation
                        graphicsPath.AddBezier(points[0], points[1], points[2], points[3]);
                        break;
                    default:
                        graphicsPath.AddBeziers(points as PointF[] ?? points.ToArray());
                        break;
                }
            }

            static void AddMockedPoint(GraphicsPath graphicsPath, PointF point)
                => graphicsPath.AddLine(new PointF(point.X - 0.25f, point.Y), new PointF(point.X + 0.25f, point.Y));

            #endregion

            if (path == null)
                throw new ArgumentNullException(nameof(path), PublicResources.ArgumentNull);

            var result = new GraphicsPath();
            if (path.IsEmpty)
                return result;

            foreach (Figure figure in path.Figures)
            {
                result.StartFigure();
                foreach (PathSegment segment in figure.Segments)
                {
                    switch (segment)
                    {
                        case LineSegment lineSegment:
                            // A single-point LineSegment is supported by KGySoft Path but ignored by GDI+,
                            // so adding a single-point line segment only if this segment is connected to other segments.
                            // Otherwise, adding a mocked point to make it visible in a GDI+ path.
                            IList<PointF> points = lineSegment.Points;
                            Debug.Assert(points.Count > 0);
                            if (points.Count > 1 || figure.Segments.Count > 1)
                                AddLines(result, points);
                            else
                                AddMockedPoint(result, points[0]);
                            break;

                        case BezierSegment bezierSegment:
                            points = bezierSegment.Points;
                            Debug.Assert(points.Count > 0);
                            if (points.Count > 1 || figure.Segments.Count > 1)
                                AddBeziers(result, points);
                            else
                                AddMockedPoint(result, points[0]);
                            break;

                        case ArcSegment arcSegment:
                            // GraphicsPath does not support very flat arcs, so adding them as lines.
                            // In fact, 1e-6 is still accepted, but the result is getting inaccurate below 1e-4
                            if (arcSegment.RadiusX.TolerantIsZero(tolerance) || arcSegment.RadiusY.TolerantIsZero(tolerance) || arcSegment.SweepAngle.TolerantIsZero(tolerance))
                            {
                                points = arcSegment.GetFlattenedPoints();
                                if (points.Count > 1 || figure.Segments.Count > 1)
                                    AddLines(result, points);
                                else
                                    AddMockedPoint(result, points[0]);
                                break;
                            }

                            // A full ellipse is the only segment in the figure: just adding an ellipse.
                            if (arcSegment.SweepAngle is 360f && figure.Segments.Count == 1)
                            {
                                result.AddEllipse(arcSegment.Bounds);
                                break;
                            }

                            // GraphicsPath.AddArc implicitly closes the figure when a full ellipse is added by AddArc, so falling back to Bézier curves in such case.
                            if (Math.Abs(arcSegment.SweepAngle).TolerantEquals(360f, tolerance))
                            {
                                points = arcSegment.ToBezierPoints();
                                if (points.Count > 1 || figure.Segments.Count > 1)
                                    AddBeziers(result, points);
                                else
                                    AddMockedPoint(result, points[0]);
                                break;
                            }

                            result.AddArc(arcSegment.Bounds, arcSegment.StartAngle, arcSegment.SweepAngle);
                            break;
                    }
                }

                if (figure.IsClosed)
                    result.CloseFigure();
            }

            return result;
        }

        #endregion
    }
}
