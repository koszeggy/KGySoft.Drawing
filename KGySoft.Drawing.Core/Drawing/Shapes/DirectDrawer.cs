#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: DirectDrawer.cs
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
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Security;

using KGySoft.CoreLibraries;
using KGySoft.Drawing.Imaging;
using KGySoft.Threading;

#endregion

namespace KGySoft.Drawing.Shapes
{
    /// <summary>
    /// This class is used to draw thin paths and to fill rectangles with solid colors. These drawing algorithms are actually duplicated in the classes
    /// derived from DrawThinPathSession in <see cref="Brush"/>. These are used in special cases, and are optimized for performance.
    /// See also the comments in <see cref="SolidBrush"/> for more info.
    /// </summary>
    internal static class DirectDrawer
    {
        #region Nested classes

        internal static class GenericDrawer<TAccessor, TColor, TArg>
            where TAccessor : struct, IBitmapDataAccessor<TColor, TArg>
            where TColor : unmanaged
        {
            #region Constants

            private const float roundingUnit = 1f / 32f;

            #endregion

            #region Methods

            #region Internal Methods 

            [MethodImpl(MethodImpl.AggressiveInlining)]
            internal static void DrawLine(IBitmapDataInternal bitmapData, PointF start, PointF end, TColor c, float offset, TArg arg = default!)
            {
                (Point p1, Point p2) = Round(start, end, offset);
                DrawLine(bitmapData, p1, p2, c, arg);
            }

            [SecuritySafeCritical]
            [SuppressMessage("Microsoft.Maintainability", "CA1502: Avoid excessive complexity",
                Justification = "Optimizations for special cases. Not extracting additional methods to prevent placing more frames on the call stack.")]
            internal static void DrawLine(IBitmapDataInternal bitmapData, Point p1, Point p2, TColor c, TArg arg = default!)
            {
                var accessor = new TAccessor();
                Size size = bitmapData.Size;
                int step, x, y, endX, endY;

                // horizontal line (or a single point)
                if (p1.Y == p2.Y)
                {
                    if ((uint)p1.Y >= (uint)(size.Height))
                        return;

                    accessor.InitRow(bitmapData.GetRowCached(p1.Y), arg);
                    if (p1.X > p2.X)
                        (p1.X, p2.X) = (p2.X, p1.X);

                    int max = Math.Min(p2.X, size.Width - 1);
                    for (x = Math.Max(p1.X, 0); x <= max; x++)
                        accessor.SetColor(x, c);

                    return;
                }

                accessor.InitBitmapData(bitmapData, arg);

                // vertical line
                if (p1.X == p2.X)
                {
                    if ((uint)p1.X >= (uint)(size.Width))
                        return;

                    if (p1.Y > p2.Y)
                        (p1.Y, p2.Y) = (p2.Y, p1.Y);

                    int max = Math.Min(p2.Y, size.Height - 1);
                    for (y = Math.Max(p1.Y, 0); y <= max; y++)
                        accessor.SetColor(p1.X, y, c);

                    return;
                }

                // general line
                long width = ((long)p2.X - p1.X).Abs();
                long height = ((long)p2.Y - p1.Y).Abs();
                long numerator;

                // shallow slope: left to right
                if (width >= height)
                {
                    numerator = width >> 1;
                    if (p1.X > p2.X)
                        (p1, p2) = (p2, p1);
                    if (p2.X < 0)
                        return;
                    if (p2.Y > p1.Y)
                    {
                        if (p1.Y >= size.Height)
                            return;
                        step = 1;
                    }
                    else
                    {
                        if (p1.Y < 0)
                            return;
                        step = -1;
                    }

                    x = p1.X;
                    y = p1.Y;

                    // skipping invisible X coordinates to the left
                    if (x < 0)
                    {
                        long sum = numerator + height * -(long)x;
                        numerator = sum % width;
                        y = (int)(y + sum / width * step);
                        x = 0;
                    }

                    endX = Math.Min(p2.X, size.Width - 1);
                    if (step > 0)
                    {
                        endY = Math.Min(p2.Y, size.Height - 1) + 1;
                        if (endY <= y)
                            return;

                        // skipping invisible Y coordinates above
                        if (y < 0)
                        {
                            long dx = (-(long)y * width - numerator + height - 1L) / height;
                            if (x + dx > endX)
                                return;
                            numerator = (numerator + height * dx) % width;
                            x += (int)dx;
                            y = 0;
                        }
                    }
                    else
                    {
                        endY = Math.Max(p2.Y, 0) - 1;
                        if (endY >= y)
                            return;

                        // skipping invisible Y coordinates below
                        if (y >= size.Height)
                        {
                            long dx = ((y - (size.Height - 1L)) * width - numerator + height - 1L) / height;
                            if (x + dx > endX)
                                return;
                            numerator = (numerator + height * dx) % width;
                            x += (int)dx;
                            y = size.Height - 1;
                        }
                    }

                    // drawing the visible part
                    for (; x <= endX; x++)
                    {
                        Debug.Assert((uint)y < (uint)size.Height && (uint)x < (uint)size.Width, $"Attempting to draw invisible pixel ({x}; {y}) for line {p1} -> {p2}");
                        accessor.SetColor(x, y, c);
                        numerator += height;
                        if (numerator < width)
                            continue;

                        y += step;
                        if (y == endY)
                            return;
                        numerator -= width;
                    }

                    return;
                }

                // steep slope: top to bottom
                numerator = height >> 1;
                if (p1.Y > p2.Y)
                    (p1, p2) = (p2, p1);
                if (p2.Y < 0)
                    return;
                if (p2.X > p1.X)
                {
                    if (p1.X >= size.Width)
                        return;
                    step = 1;
                }
                else
                {
                    if (p1.X < 0)
                        return;
                    step = -1;
                }

                x = p1.X;
                y = p1.Y;

                // skipping invisible Y coordinates above
                if (y < 0)
                {
                    long sum = numerator + width * -(long)y;
                    numerator = sum % height;
                    x = (int)(x + sum / height * step);
                    y = 0;
                }

                endY = Math.Min(p2.Y, size.Height - 1);
                if (step > 0)
                {
                    endX = Math.Min(p2.X, size.Width - 1) + 1;
                    if (endX <= x)
                        return;

                    // skipping invisible X coordinates to the left
                    if (x < 0)
                    {
                        long dy = (-(long)x * height - numerator + width - 1L) / width;
                        if (y + dy > endY)
                            return;
                        numerator = (numerator + width * dy) % height;
                        y += (int)dy;
                        x = 0;
                    }
                }
                else
                {
                    endX = Math.Max(p2.X, 0) - 1;
                    if (endX >= x)
                        return;

                    // skipping invisible X coordinates to the right
                    if (x >= size.Width)
                    {
                        long dy = ((x - (size.Width - 1L)) * height - numerator + width - 1L) / width;
                        if (y + dy > endY)
                            return;
                        numerator = (numerator + width * dy) % height;
                        y += (int)dy;
                        x = size.Width - 1;
                    }
                }

                // drawing the visible part
                for (; y <= endY; y++)
                {
                    Debug.Assert((uint)y < (uint)size.Height && (uint)x < (uint)size.Width, $"Attempting to draw invisible pixel ({x}; {y}) for line {p1} -> {p2}");
                    accessor.SetColor(x, y, c);
                    numerator += width;
                    if (numerator < height)
                        continue;

                    x += step;
                    if (x == endX)
                        return;
                    numerator -= height;
                }
            }

            [MethodImpl(MethodImpl.AggressiveInlining)]
            internal static void DrawLines(IBitmapDataInternal bitmapData, IEnumerable<Point> points, TColor c, TArg arg = default!)
            {
                IList<Point> pointList = points as IList<Point> ?? new List<Point>(points);
                int count = pointList.Count;
                switch (count)
                {
                    case < 1:
                        return;
                    case 1:
                        DrawLine(bitmapData, pointList[0], pointList[0], c, arg);
                        return;
                    default:
                        for (int i = 1; i < count; i++)
                            DrawLine(bitmapData, pointList[i - 1], pointList[i], c, arg);
                        return;
                }
            }

            [MethodImpl(MethodImpl.AggressiveInlining)]
            internal static void DrawLines(IBitmapDataInternal bitmapData, IEnumerable<PointF> points, TColor c, float offset, TArg arg = default!)
            {
                IList<PointF> pointList = points as IList<PointF> ?? new List<PointF>(points);
                int count = pointList.Count;
                switch (count)
                {
                    case 0:
                        return;
                    case 1:
                        DrawLine(bitmapData, pointList[0], pointList[0], c, offset, arg);
                        return;
                    default:
                        for (int i = 1; i < count; i++)
                            DrawLine(bitmapData, pointList[i - 1], pointList[i], c, offset, arg);
                        return;
                }
            }

            [MethodImpl(MethodImpl.AggressiveInlining)]
            internal static void DrawBeziers(IBitmapDataInternal bitmapData, IList<PointF> points, TColor c, float offset, TArg arg = default!)
            {
                Debug.Assert((points.Count - 1) % 3 == 0);
                DrawLines(bitmapData, new BezierSegment(points, false).GetFlattenedPointsInternal(), c, offset, arg);
            }

            [MethodImpl(MethodImpl.AggressiveInlining)]
            internal static void DrawRectangle(IBitmapDataInternal bitmapData, RectangleF rectangle, TColor c, float offset, TArg arg = default!)
            {
                (Point p1, Point p2) = Round(rectangle.Location, rectangle.Size.ToPointF(), offset);
                DrawRectangle(bitmapData, new Rectangle(p1.X, p1.Y, p2.X, p2.Y), c, arg);
            }

            [MethodImpl(MethodImpl.AggressiveInlining)]
            internal static void DrawRectangle(IBitmapDataInternal bitmapData, Rectangle rectangle, TColor c, TArg arg = default!)
            {
                int left = rectangle.Left;
                int top = rectangle.Top;
                int right = rectangle.RightChecked();
                int bottom = rectangle.BottomChecked();
                DrawLine(bitmapData, new Point(left, top), new Point(right, top), c, arg);
                DrawLine(bitmapData, new Point(right, top), new Point(right, bottom), c, arg);
                DrawLine(bitmapData, new Point(right, bottom), new Point(left, bottom), c, arg);
                DrawLine(bitmapData, new Point(left, bottom), new Point(left, top), c, arg);
            }

            [MethodImpl(MethodImpl.AggressiveInlining)]
            internal static void DrawEllipse(IBitmapDataInternal bitmapData, RectangleF bounds, TColor c, float offset, TArg arg = default!)
            {
                bounds.Normalize();
                (Point p1, Point p2) = Round(bounds.Location, bounds.Size.ToPointF(), offset);
                var rect = new Rectangle(p1.X, p1.Y, p2.X, p2.Y);
                if (rect.Width > ArcSegment.DrawAsLinesThreshold || rect.Height > ArcSegment.DrawAsLinesThreshold || rect.Width < 2f || rect.Height < 2f)
                {
                    DrawLines(bitmapData, BezierSegment.FromEllipse(bounds).GetFlattenedPointsInternal(), c, offset);
                    return;
                }

                DoDrawEllipse(bitmapData, rect, c, arg);
            }

            [MethodImpl(MethodImpl.AggressiveInlining)]
            internal static void DrawEllipse(IBitmapDataInternal bitmapData, Rectangle bounds, TColor c, TArg arg = default!)
            {
                bounds.Normalize();
                if (bounds.Width > ArcSegment.DrawAsLinesThreshold || bounds.Height > ArcSegment.DrawAsLinesThreshold || bounds.Width < 2 || bounds.Height < 2)
                {
                    DrawLines(bitmapData, BezierSegment.FromEllipse(bounds).GetFlattenedPointsInternal(), c, 0f);
                    return;
                }

                DoDrawEllipse(bitmapData, bounds, c, arg);
            }

            [MethodImpl(MethodImpl.AggressiveInlining)]
            internal static void DrawArc(IBitmapDataInternal bitmapData, ArcSegment arc, TColor c, float offset, TArg arg = default!)
            {
                RectangleF bounds = arc.Bounds;
                Debug.Assert(arc.SweepAngle < 360f);
                Debug.Assert(bounds.Width >= 0 && bounds.Height >= 0, "Normalized bounds are expected here");
                Debug.Assert(bounds.Width is <= ArcSegment.DrawAsLinesThreshold and >= 2f && bounds.Height is <= ArcSegment.DrawAsLinesThreshold and >= 2f);
                (Point p1, Point p2) = Round(bounds.Location, bounds.Location + bounds.Size, offset);
                Size size = bitmapData.Size;

                (int left, int right) = (p1.X, p2.X);
                (int top, int bottom) = (p1.Y, p2.Y);
                int width = right - left; // exclusive: the actual drawn width is width + 1
                int height = bottom - top; // exclusive: the actual drawn height is height + 1

                if (left >= size.Width || top >= size.Height || right < 0 || bottom < 0)
                    return;

                // Not using arc.RadiusX/Y here because that is shorter by a half pixel (even if there is no rounding error)
                // because ArcSegment has no concept of line width, and here we draw a 1px wide path.
                float radiusX = (width + 1) / 2f;
                float radiusY = (height + 1) / 2f;
                (float startRad, float endRad) = arc.GetStartEndRadiansNormalized();

                // To prevent calculating Atan2 for each pixel, we just calculate a valid start/end range once, and apply it based on the current sector attributes.
                if (width >= height)
                {
                    float centerX = (left + right + 1) / 2f;
                    int startX = (int)(centerX + radiusX * MathF.Cos(startRad));
                    int endX = (int)(centerX + radiusX * MathF.Cos(endRad));
                    DoDrawArcHorizontal(bitmapData, left, top, right, bottom, c, arc.GetSectors(), startX, endX, arg);
                    return;
                }

                float centerY = (top + bottom + 1) / 2f;
                int startY = (int)(centerY + radiusY * MathF.Sin(startRad));
                int endY = (int)(centerY + radiusY * MathF.Sin(endRad));
                DoDrawArcVertical(bitmapData, left, top, right, bottom, c, arc.GetSectors(), startY, endY, arg);
            }

            [MethodImpl(MethodImpl.AggressiveInlining)]
            internal static void DrawArc(IBitmapDataInternal bitmapData, RectangleF bounds, float startAngle, float sweepAngle, TColor c, float offset, TArg arg = default!)
            {
                bounds.Normalize();
                (Point p1, Point p2) = Round(bounds.Location, bounds.Size.ToPointF(), offset);
                var rect = new Rectangle(p1.X, p1.Y, p2.X, p2.Y);
                if (rect.Width > ArcSegment.DrawAsLinesThreshold || rect.Height > ArcSegment.DrawAsLinesThreshold || rect.Width < 2 || rect.Height < 2)
                {
                    DrawLines(bitmapData, BezierSegment.FromArc(bounds, startAngle, sweepAngle).GetFlattenedPointsInternal(), c, offset, arg);
                    return;
                }

                ArcSegment.NormalizeAngles(ref startAngle, ref sweepAngle);
                if (sweepAngle >= 360f)
                {
                    DoDrawEllipse(bitmapData, rect, c, arg);
                    return;
                }

                DoDrawArc(bitmapData, rect, startAngle, sweepAngle, c, arg);
            }

            [MethodImpl(MethodImpl.AggressiveInlining)]
            internal static void DrawArc(IBitmapDataInternal bitmapData, Rectangle bounds, float startAngle, float sweepAngle, TColor c, TArg arg = default!)
            {
                bounds.Normalize();
                if (bounds.Width > ArcSegment.DrawAsLinesThreshold || bounds.Height > ArcSegment.DrawAsLinesThreshold || bounds.Width < 2 || bounds.Height < 2)
                {
                    DrawLines(bitmapData, BezierSegment.FromArc(bounds, startAngle, sweepAngle).GetFlattenedPointsInternal(), c, 0f);
                    return;
                }

                ArcSegment.NormalizeAngles(ref startAngle, ref sweepAngle);
                if (sweepAngle >= 360f)
                {
                    DoDrawEllipse(bitmapData, bounds, c, arg);
                    return;
                }

                DoDrawArc(bitmapData, bounds, startAngle, sweepAngle, c, arg);
            }

            internal static void DrawArc(IBitmapDataInternal bitmapData, PointF startPoint, PointF endPoint, float radiusX, float radiusY, float rotationAngle, bool isLargeArc, bool isClockwise, TColor c, float offset, TArg arg = default!)
            {
                const float radiusToleranceBase = 5e-7f;

                // See the comments in the SVG-like Path.AddArc overload
                if (startPoint.TolerantEquals(endPoint, Constants.HighPrecisionTolerance)
                    || radiusX.TolerantIsZero((Math.Abs(endPoint.Y - startPoint.Y) * radiusToleranceBase).Clip(Constants.HighPrecisionTolerance, Constants.PointEqualityTolerance))
                    || radiusY.TolerantIsZero((Math.Abs(endPoint.X - startPoint.X) * radiusToleranceBase).Clip(Constants.HighPrecisionTolerance, Constants.PointEqualityTolerance)))
                {
                    DrawLine(bitmapData, startPoint, endPoint, c, offset, arg);
                    return;
                }

                float rotationRad = rotationAngle.ToRadian();
                float cosPhi = MathF.Cos(rotationRad);
                float sinPhi = MathF.Sin(rotationRad);

#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
                Vector2 startVec = startPoint.AsVector2();
                Vector2 endVec = endPoint.AsVector2();
                Vector2 radius = new Vector2(radiusX, radiusY);
                radius = Vector2.Abs(radius);

                // Moving ellipse to origin and rotating to align with coordinate axes in the ellipse coordinate system
                Vector2 offsetVec = (startVec - endVec) / 2f;
                Vector2 rotated = new Vector2(cosPhi * offsetVec.X + sinPhi * offsetVec.Y, -sinPhi * offsetVec.X + cosPhi * offsetVec.Y);

                // Adjusting radii if necessary
                Vector2 rotatedSquared = rotated * rotated;
                Vector2 lambdaVec = rotatedSquared / (radius * radius);
                float lambda = lambdaVec.X + lambdaVec.Y;
                if (lambda > 1f)
                {
                    float scale = MathF.Sqrt(lambda);
                    radius *= scale;
                }

                Vector2 radiusSquared = radius * radius; // with the possibly adjusted radii

                // Calculating center
                float sign = isLargeArc != isClockwise ? 1 : -1;
                float numerator = Math.Max(0f, radiusSquared.X * radiusSquared.Y - radiusSquared.X * rotatedSquared.Y - radiusSquared.Y * rotatedSquared.X);
                float denominator = radiusSquared.X * rotatedSquared.Y + radiusSquared.Y * rotatedSquared.X;
                float coefficient = sign * MathF.Sqrt(numerator / denominator);
                Vector2 centerRotated = coefficient * new Vector2(radius.X * rotated.Y / radius.Y, -radius.Y * rotated.X / radius.X);
                Vector2 mid = (startVec + endVec) / 2f;
                Vector2 center = new Vector2(cosPhi * centerRotated.X - sinPhi * centerRotated.Y, sinPhi * centerRotated.X + cosPhi * centerRotated.Y) + mid;

                // Calculating start and end angles in the ellipse coordinate system (as if ArcSegment.ToEllipseCoordinates was called)
                Vector2 startVector = (rotated - centerRotated) / radius;
                Vector2 endVector = (-rotated - centerRotated) / radius;

                float startRad = MathF.Atan2(startVector.Y, startVector.X);
                float sweepRad = MathF.Atan2(endVector.Y, endVector.X) - startRad;
#else
                radiusX = Math.Abs(radiusX);
                radiusY = Math.Abs(radiusY);

                // Moving ellipse to origin and rotating to align with coordinate axes in the ellipse coordinate system
                float offsetX = (startPoint.X - endPoint.X) / 2f;
                float offsetY = (startPoint.Y - endPoint.Y) / 2f;
                float rotatedX = cosPhi * offsetX + sinPhi * offsetY;
                float rotatedY = -sinPhi * offsetX + cosPhi * offsetY;

                // Adjusting radii if necessary
                float rotatedXSquared = rotatedX * rotatedX;
                float rotatedYSquared = rotatedY * rotatedY;
                float lambda = rotatedXSquared / (radiusX * radiusX) + rotatedYSquared / (radiusY * radiusY);
                if (lambda > 1f)
                {
                    float scale = MathF.Sqrt(lambda);
                    radiusX *= scale;
                    radiusY *= scale;
                }

                float radiusXSquared = radiusX * radiusX; // with the possibly adjusted radii
                float radiusYSquared = radiusY * radiusY;

                // Calculating center
                float sign = isLargeArc != isClockwise ? 1 : -1;
                float numerator = Math.Max(0f, radiusXSquared * radiusYSquared - radiusXSquared * rotatedYSquared - radiusYSquared * rotatedXSquared);
                float denominator = radiusXSquared * rotatedYSquared + radiusYSquared * rotatedXSquared;
                float coefficient = sign * MathF.Sqrt(numerator / denominator);
                float centerRotatedX = coefficient * radiusX * rotatedY / radiusY;
                float centerRotatedY = coefficient * -radiusY * rotatedX / radiusX;
                float midX = (startPoint.X + endPoint.X) / 2f;
                float midY = (startPoint.Y + endPoint.Y) / 2f;
                float centerX = cosPhi * centerRotatedX - sinPhi * centerRotatedY + midX;
                float centerY = sinPhi * centerRotatedX + cosPhi * centerRotatedY + midY;

                // Calculating start and end angles in the ellipse coordinate system (as if ArcSegment.ToEllipseCoordinates was called)
                float startVectorX = (rotatedX - centerRotatedX) / radiusX;
                float startVectorY = (rotatedY - centerRotatedY) / radiusY;
                float endVectorX = (-rotatedX - centerRotatedX) / radiusX;
                float endVectorY = (-rotatedY - centerRotatedY) / radiusY;

                float startRad = MathF.Atan2(startVectorY, startVectorX);
                float sweepRad = MathF.Atan2(endVectorY, endVectorX) - startRad;
#endif

                // Ensuring that we get the correct arc (large vs small). Can be imprecise if the arc is very close to 180 degrees, which is adjusted below.
                if (isLargeArc != Math.Abs(sweepRad) >= MathF.PI)
                {
                    if (sweepRad > 0)
                        sweepRad -= 2f * MathF.PI;
                    else
                        sweepRad += 2f * MathF.PI;
                }

                // Fixing the possibly wrong direction of an exactly 180 degrees arc.
                if (isClockwise != sweepRad > 0f && Math.Abs(sweepRad).TolerantEquals(MathF.PI, Constants.HighPrecisionTolerance))
                    sweepRad = -sweepRad;

                // If there is no rotation we can use the Bresenham-based arc drawer which is more symmetric than the Bézier approximation.
                if (rotationAngle == 0f)
                {
#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
                    DrawArc(bitmapData, new ArcSegment(center.AsPointF(), radius.X, radius.Y, startRad, startRad + sweepRad), c, offset, arg);
#else
                    DrawArc(bitmapData, new ArcSegment(new PointF(centerX, centerY), radiusX, radiusY, startRad, startRad + sweepRad), c, offset, arg);
#endif
                    return;
                }

                // Otherwise, drawing as cubic Bézier segments.
#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
                DrawBeziers(bitmapData, BezierSegment.GetBezierPointsFromArc(center.AsPointF(), radius.X, radius.Y, startRad, sweepRad, rotationRad), c, offset, arg);
#else
                DrawBeziers(bitmapData, BezierSegment.GetBezierPointsFromArc(new PointF(centerX, centerY), radiusX, radiusY, startRad, sweepRad, rotationRad), c, offset, arg);
#endif
            }

            [MethodImpl(MethodImpl.AggressiveInlining)]
            internal static void DrawPie(IBitmapDataInternal bitmapData, RectangleF bounds, float startAngle, float sweepAngle, TColor c, float offset, TArg arg = default!)
            {
                var arc = new ArcSegment(bounds, startAngle, sweepAngle);
                DrawArc(bitmapData, arc, c, offset, arg);
                DrawLines(bitmapData, new[] { arc.EndPoint, arc.Center, arc.StartPoint }, c, offset, arg);
            }

            internal static void DrawRoundedRectangle(IBitmapDataInternal bitmapData, RectangleF bounds, float cornerRadius, TColor c, float offset, TArg arg = default!)
            {
                (Point p1, Point p2) = Round(bounds.Location, bounds.Size.ToPointF(), offset);
                DrawRoundedRectangle(bitmapData, new Rectangle(p1.X, p1.Y, p2.X, p2.Y), checked((int)MathF.Round(cornerRadius, MidpointRounding.AwayFromZero)), c, arg);
            }

            internal static void DrawRoundedRectangle(IBitmapDataInternal bitmapData, Rectangle bounds, int cornerRadius, TColor c, TArg arg = default!)
            {
                bounds.Normalize();
                if (cornerRadius == 0)
                {
                    DrawRectangle(bitmapData, bounds, c, arg);
                    return;
                }

                int diameter = Math.Min(Math.Abs(cornerRadius) << 1, Math.Min(Math.Abs(bounds.Width), Math.Abs(bounds.Height)));
                var corner = new Rectangle(bounds.Location, new Size(diameter, diameter));
                cornerRadius = diameter >> 1;

                // top-left corner
                DrawArc(bitmapData, corner, 180f, 90f, c, arg);

                // top-right corner
                corner.X = bounds.Right - diameter;
                DrawArc(bitmapData, corner, 270f, 90f, c, arg);

                // bottom-right corner
                corner.Y = bounds.Bottom - diameter;
                DrawArc(bitmapData, corner, 0f, 90f, c, arg);

                // bottom-left corner
                corner.X = bounds.Left;
                DrawArc(bitmapData, corner, 90f, 90f, c, arg);

                int from = bounds.Left + cornerRadius + 1;
                int to = bounds.Right - cornerRadius - 1;

                // top and bottom edges
                if (from <= to)
                {
                    DrawLine(bitmapData, new Point(from, bounds.Top), new Point(to, bounds.Top), c, arg);
                    DrawLine(bitmapData, new Point(from, bounds.Bottom), new Point(to, bounds.Bottom), c, arg);
                }

                from = bounds.Top + cornerRadius + 1;
                to = bounds.Bottom - cornerRadius - 1;

                // right and left edges
                if (from <= to)
                {
                    DrawLine(bitmapData, new Point(bounds.Right, bounds.Top + cornerRadius), new Point(bounds.Right, bounds.Bottom - cornerRadius), c, arg);
                    DrawLine(bitmapData, new Point(bounds.Left, bounds.Top + cornerRadius), new Point(bounds.Left, bounds.Bottom - cornerRadius), c, arg);
                }
            }

            [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator", Justification = "Intended")]
            internal static void DrawRoundedRectangle(IBitmapDataInternal bitmapData, RectangleF bounds,
                float radiusTopLeft, float radiusTopRight, float radiusBottomRight, float radiusBottomLeft, TColor c, float offset, TArg arg = default!)
            {
                // NOTE: Unlike the equal radius version, this method works with floats because of the possible scaling for big radii.
                //       This ensures the same behavior as in Path.AddRoundedRectangle.

                // Adjusting radii to the bounds
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

                // top left
                var corner = new RectangleF(bounds.Location, new SizeF(radiusTopLeft * 2f, radiusTopLeft * 2f));
                if (radiusTopLeft > 0f)
                    DrawArc(bitmapData, corner, 180f, 90f, c, offset, arg);

                // top right
                if (radiusTopRight != radiusTopLeft)
                    corner.Size = new SizeF(radiusTopRight * 2f, radiusTopRight * 2f);
                corner.X = bounds.Right - corner.Width;
                if (radiusTopRight > 0f)
                    DrawArc(bitmapData, corner, 270f, 90f, c, offset, arg);

                // bottom right
                if (radiusBottomRight != radiusTopRight)
                {
                    corner.Size = new SizeF(radiusBottomRight * 2f, radiusBottomRight * 2f);
                    corner.X = bounds.Right - corner.Width;
                }

                corner.Y = bounds.Bottom - corner.Height;
                if (radiusBottomRight > 0f)
                    DrawArc(bitmapData, corner, 0f, 90f, c, offset, arg);

                // bottom left
                if (radiusBottomLeft != radiusBottomRight)
                {
                    corner.Size = new SizeF(radiusBottomLeft * 2f, radiusBottomLeft * 2f);
                    corner.Y = bounds.Bottom - corner.Height;
                }

                corner.X = bounds.Left;
                if (radiusBottomLeft > 0f)
                    DrawArc(bitmapData, corner, 90f, 90f, c, offset, arg);

                // Unlike in the equal radius version, we always draw the edges (without +- 1) because we may have skipped zero-radius corners.
                DrawLine(bitmapData, new PointF(bounds.Left + radiusTopLeft, bounds.Top), new PointF(bounds.Right - radiusTopRight, bounds.Top), c, offset, arg);
                DrawLine(bitmapData, new PointF(bounds.Right, bounds.Top + radiusTopRight), new PointF(bounds.Right, bounds.Bottom - radiusBottomRight), c, offset, arg);
                DrawLine(bitmapData, new PointF(bounds.Left + radiusBottomLeft, bounds.Bottom), new PointF(bounds.Right - radiusBottomRight, bounds.Bottom), c, offset, arg);
                DrawLine(bitmapData, new PointF(bounds.Left, bounds.Top + radiusTopLeft), new PointF(bounds.Left, bounds.Bottom - radiusBottomLeft), c, offset, arg);
            }

            [SecuritySafeCritical]
            internal static bool FillRectangle(IAsyncContext context, IBitmapDataInternal bitmapData, TColor color, Rectangle rectangle)
            {
                Debug.Assert(!rectangle.IsEmpty() && new Rectangle(Point.Empty, bitmapData.Size).Contains(rectangle));
                
                // sequential fill
                if (rectangle.Width < parallelThreshold)
                {
                    IBitmapDataRowInternal row = bitmapData.GetRowCached(rectangle.Top);
                    var accessor = new TAccessor();
                    accessor.InitRow(row);

                    context.Progress?.New(DrawingOperation.ProcessingPixels, bitmapData.Height);
                    for (int y = 0; y < rectangle.Height; y++)
                    {
                        if (context.IsCancellationRequested)
                            return false;

                        int right = rectangle.Right;
                        for (int x = rectangle.Left; x < right; x++)
                            accessor.SetColor(x, color);
                        context.Progress?.Increment();
                        row.MoveNextRow();
                    }

                    return true;
                }

                // parallel fill
                return ParallelHelper.For(context, DrawingOperation.ProcessingPixels, rectangle.Top, rectangle.Bottom, ProcessRow);

                #region Local Methods

                [SecuritySafeCritical]
                void ProcessRow(int y)
                {
                    IBitmapDataRowInternal row = bitmapData.GetRowCached(y);
                    var accessor = new TAccessor();
                    accessor.InitRow(row);
                    TColor c = color;

                    int right = rectangle.Right;
                    for (int x = rectangle.Left; x < right; x++)
                        accessor.SetColor(x, c);
                }

                #endregion
            }

            #endregion

            #region Private Methods

            [MethodImpl(MethodImpl.AggressiveInlining)]
            private static (Point P1, Point P2) Round(PointF p1, PointF p2, float offset)
            {
                // For performance reasons there are no checks in the public BitmapDataExtensions.DrawXXX methods, but here we throw an OverflowException for extreme cases.
#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
                Vector4 result = (new Vector4(p1.X, p1.Y, p2.X, p2.Y).RoundTo(roundingUnit) + new Vector4(offset)).Floor();
                return checked((new Point((int)result.X, (int)result.Y), new Point((int)result.Z, (int)result.W)));
#else
                p1.X = MathF.Floor(p1.X.RoundTo(roundingUnit) + offset);
                p1.Y = MathF.Floor(p1.Y.RoundTo(roundingUnit) + offset);
                p2.X = MathF.Floor(p2.X.RoundTo(roundingUnit) + offset);
                p2.Y = MathF.Floor(p2.Y.RoundTo(roundingUnit) + offset);

                return checked((new Point((int)p1.X, (int)p1.Y), new Point((int)p2.X, (int)p2.Y)));
#endif
            }

            // Based on http://members.chello.at/~easyfilter/bresenham.c
            // Main changes: converting to C#, correcting types, more descriptive variable names
            private static void DoDrawEllipse(IBitmapDataInternal bitmapData, Rectangle bounds, TColor c, TArg arg = default!)
            {
                int width = bounds.Width; // exclusive: the actual drawn width is width + 1
                int height = bounds.Height; // exclusive: the actual drawn height is height + 1
                Debug.Assert(width >= 0 && height >= 0, "Normalized bounds are expected here");
                int top = bounds.Top;
                int left = bounds.Left;
                int right = bounds.RightChecked();
                int bottom = bounds.BottomChecked();
                Size size = bitmapData.Size;

                Debug.Assert(width <= ArcSegment.DrawAsLinesThreshold && height <= ArcSegment.DrawAsLinesThreshold);
                if (left >= size.Width || top >= size.Height || right < 0 || bottom < 0)
                    return;

                int oddHeightCorrection = height & 1;
                long widthSquared = (long)width * width;
                long heightSquared = (long)height * height;
                long stepX = 1L - width;
                stepX = (stepX * heightSquared) << 2; // should be checked(stepX * heightSquared * 4) if height could be larger than 916395
                long stepY = (oddHeightCorrection + 1L) * widthSquared;
                stepY <<= 2; // should be checked(stepY * 4) if width could be larger than 916396
                long err = oddHeightCorrection * widthSquared;
                err += stepX + stepY; //  should be checked(stepX + stepY + err) if size could be larger than 916396 x 916395

                bottom = top + ((height + 1) >> 1);
                top = bottom - oddHeightCorrection;
                long scaledWidth = widthSquared << 3;
                long scaledHeight = heightSquared << 3;

                var accessor = new TAccessor();
                accessor.InitBitmapData(bitmapData, arg);

                do
                {
                    SetPixel(left, top);
                    SetPixel(right, top);
                    SetPixel(left, bottom);
                    SetPixel(right, bottom);

                    long err2 = err << 1; //should be checked(err * 2) if size could be larger than 916396 x 916395
                    if (err2 <= stepY)
                    {
                        top -= 1;
                        bottom += 1;
                        stepY += scaledWidth; //should be checked(stepY + scaledWidth) if width could be larger than 916396
                        err += stepY;
                    }

                    if (err2 >= stepX || err2 > stepY)
                    {
                        left += 1;
                        right -= 1;
                        stepX += scaledHeight; //should be checked(stepX + scaledHeight) if height could be larger than 916395
                        err += stepX;
                    }
                } while (left <= right);

                if (left > size.Width || right < -1 || top < 0 && bottom >= size.Height)
                    return;

                while (bottom - top <= height)
                {
                    SetPixel(left - 1, top);
                    SetPixel(right + 1, top);
                    top -= 1;
                    SetPixel(left - 1, bottom);
                    SetPixel(right + 1, bottom);
                    bottom += 1;
                }

                #region Local Methods

                [MethodImpl(MethodImpl.AggressiveInlining)]
                void SetPixel(int x, int y)
                {
                    if ((uint)x < (uint)size.Width && (uint)y < (uint)size.Height)
                        accessor.SetColor(x, y, c);
                }

                #endregion
            }

            private static void DoDrawArc(IBitmapDataInternal bitmapData, Rectangle bounds, float startAngle, float sweepAngle, TColor c, TArg arg = default!)
            {
                int width = bounds.Width; // exclusive: the actual drawn width is width + 1
                int height = bounds.Height; // exclusive: the actual drawn height is height + 1
                Debug.Assert(width >= 0 && height >= 0, "Normalized bounds are expected here");

                int top = bounds.Top;
                int left = bounds.Left;
                int right = bounds.RightChecked();
                int bottom = bounds.BottomChecked();
                Size size = bitmapData.Size;

                if (left >= size.Width || top >= size.Height || right < 0 || bottom < 0)
                    return;

                float radiusX = (width + 1) / 2f;
                float radiusY = (height + 1) / 2f;
                float startRad = startAngle.ToRadian();
                float endRad = (startAngle + sweepAngle).ToRadian();
                ArcSegment.ToEllipseCoordinates(ref startRad, ref endRad, radiusX, radiusY);

                // To prevent calculating Atan2 for each pixel, we just calculate a valid start/end range once, and apply it based on the current sector attributes.
                if (width >= height)
                {
                    float centerX = (left + right + 1) / 2f;
                    int startX = (int)(centerX + radiusX * MathF.Cos(startRad));
                    int endX = (int)(centerX + radiusX * MathF.Cos(endRad));
                    DoDrawArcHorizontal(bitmapData, left, top, right, bottom, c, ArcSegment.GetSectors(startAngle, sweepAngle), startX, endX, arg);
                    return;
                }

                float centerY = (top + bottom + 1) / 2f;
                int startY = (int)(centerY + radiusY * MathF.Sin(startRad));
                int endY = (int)(centerY + radiusY * MathF.Sin(endRad));
                DoDrawArcVertical(bitmapData, left, top, right, bottom, c, ArcSegment.GetSectors(startAngle, sweepAngle), startY, endY, arg);
            }

            // Based on the combination of http://members.chello.at/~easyfilter/bresenham.c and https://www.scattergood.io/arc-drawing-algorithm/
            [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity",
                Justification = "False alarm, the new analyzer includes the complexity of local methods.")]
            private static void DoDrawArcHorizontal(IBitmapDataInternal bitmapData, int left, int top, int right, int bottom,
                TColor c, BitVector32 sectors, int startX, int endX, TArg arg)
            {
                int width = right - left; // Exclusive: the actual drawn width is width + 1.
                int height = bottom - top; // Exclusive: the actual drawn height is height + 1
                Debug.Assert(width <= ArcSegment.DrawAsLinesThreshold && height <= ArcSegment.DrawAsLinesThreshold && width >= height);
                Debug.Assert(width >= 2 && height >= 2, "Flat arcs should be drawn as flattened lines instead");
                Size size = bitmapData.Size;

                int oddHeightCorrection = height & 1;
                long widthSquared = (long)width * width;
                long heightSquared = (long)height * height;
                long stepX = 1L - width;
                stepX = (stepX * heightSquared) << 2; // should be checked(stepX * heightSquared * 4) if height could be larger than 916395
                long stepY = (oddHeightCorrection + 1L) * widthSquared;
                stepY <<= 2; // should be checked(stepY * 4) if width could be larger than 916396
                long err = oddHeightCorrection * widthSquared;
                err += stepX + stepY; //  should be checked(stepX + stepY + err) if size could be larger than 916396 x 916395

                bottom = top + ((height + 1) >> 1);
                top = bottom - oddHeightCorrection;
                long scaledWidth = widthSquared << 3;
                long scaledHeight = heightSquared << 3;

                var accessor = new TAccessor();
                accessor.InitBitmapData(bitmapData, arg);

                do
                {
                    SetPixel(right, bottom, 0);
                    SetPixel(left, bottom, 1);
                    SetPixel(left, top, 2);
                    SetPixel(right, top, 3);

                    long err2 = err << 1; //should be checked(err * 2) if size could be larger than 916396 x 916395
                    if (err2 <= stepY)
                    {
                        top -= 1;
                        bottom += 1;
                        stepY += scaledWidth; //should be checked(stepY + scaledWidth) if width could be larger than 916396
                        err += stepY;
                    }

                    if (err2 >= stepX || err2 > stepY)
                    {
                        left += 1;
                        right -= 1;
                        stepX += scaledHeight; //should be checked(stepX + scaledHeight) if height could be larger than 916395
                        err += stepX;
                    }
                } while (left <= right);

                if (left > size.Width || right < -1 || top < 0 && bottom >= size.Height)
                    return;

                while (bottom - top <= height)
                {
                    SetPixel(right + 1, bottom, 0);
                    SetPixel(left - 1, bottom, 1);
                    bottom += 1;
                    SetPixel(left - 1, top, 2);
                    SetPixel(right + 1, top, 3);
                    top -= 1;
                }

                #region Local Methods

                void SetPixel(int x, int y, int sector)
                {
                    if ((uint)x >= (uint)size.Width || (uint)y >= (uint)size.Height)
                        return;

                    int sectorType = sectors[ArcSegment.Sectors[sector]];
                    if (sectorType == ArcSegment.SectorNotDrawn)
                        return;

                    if (sectorType == ArcSegment.SectorFullyDrawn
                        || sector > 1 // upper half
                        && (sectorType == ArcSegment.SectorStart && x >= startX
                            || sectorType == ArcSegment.SectorEnd && x <= endX
                            || sectorType == ArcSegment.SectorStartEnd && x >= startX && x <= endX)
                        || sector <= 1 // bottom half
                        && (sectorType == ArcSegment.SectorStart && x <= startX
                            || sectorType == ArcSegment.SectorEnd && x >= endX
                            || sectorType == ArcSegment.SectorStartEnd && x <= startX && x >= endX))
                    {
                        accessor.SetColor(x, y, c);
                    }
                }

                #endregion
            }

            [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity",
                Justification = "False alarm, the new analyzer includes the complexity of local methods.")]
            private static void DoDrawArcVertical(IBitmapDataInternal bitmapData, int left, int top, int right, int bottom,
                TColor c, BitVector32 sectors, int startY, int endY, TArg arg)
            {
                int width = right - left; // Exclusive: the actual drawn width is width + 1.
                int height = bottom - top; // Exclusive: the actual drawn height is height + 1
                Debug.Assert(width <= ArcSegment.DrawAsLinesThreshold && height <= ArcSegment.DrawAsLinesThreshold && height > width);
                Debug.Assert(width >= 2 && height >= 2, "Flat arcs should be drawn as flattened lines instead");
                Size size = bitmapData.Size;

                int oddWidthCorrection = width & 1;
                long widthSquared = (long)width * width;
                long heightSquared = (long)height * height;
                long stepY = 1L - height;
                stepY = (stepY * widthSquared) << 2; // should be checked(stepY * widthSquared * 4) if width could be larger than 916396
                long stepX = (oddWidthCorrection + 1L) * heightSquared;
                stepX <<= 2; // should be checked(stepX * 4) if height could be larger than 916395
                long err = oddWidthCorrection * heightSquared;
                err += stepX + stepY; //  should be checked(stepX + stepY + err) if size could be larger than 916396 x 916395

                right = left + ((width + 1) >> 1);
                left = right - oddWidthCorrection;
                long scaledWidth = widthSquared << 3;
                long scaledHeight = heightSquared << 3;

                var accessor = new TAccessor();
                accessor.InitBitmapData(bitmapData, arg);

                do
                {
                    SetPixel(right, bottom, 0);
                    SetPixel(left, bottom, 1);
                    SetPixel(left, top, 2);
                    SetPixel(right, top, 3);

                    long err2 = err << 1; //should be checked(err * 2) if size could be larger than 916396 x 916395
                    if (err2 <= stepX)
                    {
                        left -= 1;
                        right += 1;
                        stepX += scaledHeight; //should be checked(stepX + scaledHeight) if height could be larger than 916395
                        err += stepX;
                    }

                    if (err2 >= stepY || err2 > stepX)
                    {
                        top += 1;
                        bottom -= 1;
                        stepY += scaledWidth; //should be checked(stepY + scaledWidth) if width could be larger than 916396
                        err += stepY;
                    }
                } while (top <= bottom);

                if (top > size.Height || bottom < -1 || left < 0 && right >= size.Width)
                    return;

                while (right - left <= width)
                {
                    SetPixel(right, bottom + 1, 0);
                    SetPixel(right, top - 1, 3);
                    right += 1;
                    SetPixel(left, bottom + 1, 1);
                    SetPixel(left, top - 1, 2);
                    left -= 1;
                }

                #region Local Methods

                void SetPixel(int x, int y, int sector)
                {
                    if ((uint)x >= (uint)size.Width || (uint)y >= (uint)size.Height)
                        return;

                    int sectorType = sectors[ArcSegment.Sectors[sector]];
                    if (sectorType == ArcSegment.SectorNotDrawn)
                        return;

                    if (sectorType == ArcSegment.SectorFullyDrawn
                        || sector is 0 or 3 // right half
                        && (sectorType == ArcSegment.SectorStart && y >= startY
                            || sectorType == ArcSegment.SectorEnd && y <= endY
                            || sectorType == ArcSegment.SectorStartEnd && y >= startY && y <= endY)
                        || sector is 1 or 2 // left half
                        && (sectorType == ArcSegment.SectorStart && y <= startY
                            || sectorType == ArcSegment.SectorEnd && y >= endY
                            || sectorType == ArcSegment.SectorStartEnd && y <= startY && y >= endY))
                    {
                        accessor.SetColor(x, y, c);
                    }
                }

                #endregion
            }

            #endregion

            #endregion
        }

        #endregion

        #region Constants

        private const int parallelThreshold = 100;

        #endregion

        #region Methods
        // These methods are specifically for Color32, but use GenericDrawer with the actual preferred color type.
        // Other color types call the GenericDrawer methods from SolidBrush.

        internal static void DrawLine(IReadWriteBitmapData bitmapData, Point p1, Point p2, Color32 color)
        {
            PixelFormatInfo pixelFormat = bitmapData.PixelFormat;
            IBitmapDataInternal bitmap = bitmapData as IBitmapDataInternal ?? new BitmapDataWrapper(bitmapData, false, true);

            if (bitmapData is ICustomBitmapData)
            {
                // For linear gamma assuming the best performance with [P]ColorF even if the preferred color type is smaller.
                if (pixelFormat.Prefers128BitColors || pixelFormat.LinearGamma)
                {
                    if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: true })
                        GenericDrawer<CustomBitmapDataAccessorPColorF, PColorF, _>.DrawLine(bitmap, p1, p2, color.ToPColorF());
                    else
                        GenericDrawer<CustomBitmapDataAccessorColorF, ColorF, _>.DrawLine(bitmap, p1, p2, color.ToColorF());
                    return;
                }

                if (pixelFormat.Prefers64BitColors)
                {
                    if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                        GenericDrawer<CustomBitmapDataAccessorPColor64, PColor64, _>.DrawLine(bitmap, p1, p2, color.ToPColor64());
                    else
                        GenericDrawer<CustomBitmapDataAccessorColor64, Color64, _>.DrawLine(bitmap, p1, p2, color.ToColor64());
                    return;
                }

                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                {
                    GenericDrawer<CustomBitmapDataAccessorPColor32, PColor32, _>.DrawLine(bitmap, p1, p2, color.ToPColor32());
                    return;
                }

                if (pixelFormat.Indexed)
                {
                    GenericDrawer<CustomBitmapDataAccessorIndexed, int, _>.DrawLine(bitmap, p1, p2, bitmapData.Palette!.GetNearestColorIndex(color));
                    return;
                }

                GenericDrawer<CustomBitmapDataAccessorColor32, Color32, _>.DrawLine(bitmap, p1, p2, color);
                return;
            }

            // For linear gamma assuming the best performance with [P]ColorF even if the preferred color type is smaller.
            if (pixelFormat.Prefers128BitColors || pixelFormat.LinearGamma)
            {
                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: true })
                    GenericDrawer<BitmapDataAccessorPColorF, PColorF, _>.DrawLine(bitmap, p1, p2, color.ToPColorF());
                else
                    GenericDrawer<BitmapDataAccessorColorF, ColorF, _>.DrawLine(bitmap, p1, p2, color.ToColorF());
                return;
            }

            if (pixelFormat.Prefers64BitColors)
            {
                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                    GenericDrawer<BitmapDataAccessorPColor64, PColor64, _>.DrawLine(bitmap, p1, p2, color.ToPColor64());
                else
                    GenericDrawer<BitmapDataAccessorColor64, Color64, _>.DrawLine(bitmap, p1, p2, color.ToColor64());
                return;
            }

            if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
            {
                GenericDrawer<BitmapDataAccessorPColor32, PColor32, _>.DrawLine(bitmap, p1, p2, color.ToPColor32());
                return;
            }

            if (pixelFormat.Indexed)
            {
                GenericDrawer<BitmapDataAccessorIndexed, int, _>.DrawLine(bitmap, p1, p2, bitmapData.Palette!.GetNearestColorIndex(color));
                return;
            }

            GenericDrawer<BitmapDataAccessorColor32, Color32, _>.DrawLine(bitmap, p1, p2, color);
        }

        internal static void DrawLine(IReadWriteBitmapData bitmapData, PointF p1, PointF p2, Color32 color, float offset)
        {
            PixelFormatInfo pixelFormat = bitmapData.PixelFormat;
            IBitmapDataInternal bitmap = bitmapData as IBitmapDataInternal ?? new BitmapDataWrapper(bitmapData, false, true);

            if (bitmapData is ICustomBitmapData)
            {
                // For linear gamma assuming the best performance with [P]ColorF even if the preferred color type is smaller.
                if (pixelFormat.Prefers128BitColors || pixelFormat.LinearGamma)
                {
                    if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: true })
                        GenericDrawer<CustomBitmapDataAccessorPColorF, PColorF, _>.DrawLine(bitmap, p1, p2, color.ToPColorF(), offset);
                    else
                        GenericDrawer<CustomBitmapDataAccessorColorF, ColorF, _>.DrawLine(bitmap, p1, p2, color.ToColorF(), offset);
                    return;
                }

                if (pixelFormat.Prefers64BitColors)
                {
                    if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                        GenericDrawer<CustomBitmapDataAccessorPColor64, PColor64, _>.DrawLine(bitmap, p1, p2, color.ToPColor64(), offset);
                    else
                        GenericDrawer<CustomBitmapDataAccessorColor64, Color64, _>.DrawLine(bitmap, p1, p2, color.ToColor64(), offset);
                    return;
                }

                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                {
                    GenericDrawer<CustomBitmapDataAccessorPColor32, PColor32, _>.DrawLine(bitmap, p1, p2, color.ToPColor32(), offset);
                    return;
                }

                if (pixelFormat.Indexed)
                {
                    GenericDrawer<CustomBitmapDataAccessorIndexed, int, _>.DrawLine(bitmap, p1, p2, bitmapData.Palette!.GetNearestColorIndex(color), offset);
                    return;
                }

                GenericDrawer<CustomBitmapDataAccessorColor32, Color32, _>.DrawLine(bitmap, p1, p2, color, offset);
                return;
            }

            // For linear gamma assuming the best performance with [P]ColorF even if the preferred color type is smaller.
            if (pixelFormat.Prefers128BitColors || pixelFormat.LinearGamma)
            {
                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: true })
                    GenericDrawer<BitmapDataAccessorPColorF, PColorF, _>.DrawLine(bitmap, p1, p2, color.ToPColorF(), offset);
                else
                    GenericDrawer<BitmapDataAccessorColorF, ColorF, _>.DrawLine(bitmap, p1, p2, color.ToColorF(), offset);
                return;
            }

            if (pixelFormat.Prefers64BitColors)
            {
                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                    GenericDrawer<BitmapDataAccessorPColor64, PColor64, _>.DrawLine(bitmap, p1, p2, color.ToPColor64(), offset);
                else
                    GenericDrawer<BitmapDataAccessorColor64, Color64, _>.DrawLine(bitmap, p1, p2, color.ToColor64(), offset);
                return;
            }

            if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
            {
                GenericDrawer<BitmapDataAccessorPColor32, PColor32, _>.DrawLine(bitmap, p1, p2, color.ToPColor32(), offset);
                return;
            }

            if (pixelFormat.Indexed)
            {
                GenericDrawer<BitmapDataAccessorIndexed, int, _>.DrawLine(bitmap, p1, p2, bitmapData.Palette!.GetNearestColorIndex(color), offset);
                return;
            }

            GenericDrawer<BitmapDataAccessorColor32, Color32, _>.DrawLine(bitmap, p1, p2, color, offset);
        }

        internal static void DrawLines(IReadWriteBitmapData bitmapData, IEnumerable<Point> points, Color32 color)
        {
            PixelFormatInfo pixelFormat = bitmapData.PixelFormat;
            IBitmapDataInternal bitmap = bitmapData as IBitmapDataInternal ?? new BitmapDataWrapper(bitmapData, false, true);

            if (bitmapData is ICustomBitmapData)
            {
                // For linear gamma assuming the best performance with [P]ColorF even if the preferred color type is smaller.
                if (pixelFormat.Prefers128BitColors || pixelFormat.LinearGamma)
                {
                    if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: true })
                        GenericDrawer<CustomBitmapDataAccessorPColorF, PColorF, _>.DrawLines(bitmap, points, color.ToPColorF());
                    else
                        GenericDrawer<CustomBitmapDataAccessorColorF, ColorF, _>.DrawLines(bitmap, points, color.ToColorF());
                    return;
                }

                if (pixelFormat.Prefers64BitColors)
                {
                    if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                        GenericDrawer<CustomBitmapDataAccessorPColor64, PColor64, _>.DrawLines(bitmap, points, color.ToPColor64());
                    else
                        GenericDrawer<CustomBitmapDataAccessorColor64, Color64, _>.DrawLines(bitmap, points, color.ToColor64());
                    return;
                }

                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                {
                    GenericDrawer<CustomBitmapDataAccessorPColor32, PColor32, _>.DrawLines(bitmap, points, color.ToPColor32());
                    return;
                }

                if (pixelFormat.Indexed)
                {
                    GenericDrawer<CustomBitmapDataAccessorIndexed, int, _>.DrawLines(bitmap, points, bitmapData.Palette!.GetNearestColorIndex(color));
                    return;
                }

                GenericDrawer<CustomBitmapDataAccessorColor32, Color32, _>.DrawLines(bitmap, points, color);
                return;
            }

            // For linear gamma assuming the best performance with [P]ColorF even if the preferred color type is smaller.
            if (pixelFormat.Prefers128BitColors || pixelFormat.LinearGamma)
            {
                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: true })
                    GenericDrawer<BitmapDataAccessorPColorF, PColorF, _>.DrawLines(bitmap, points, color.ToPColorF());
                else
                    GenericDrawer<BitmapDataAccessorColorF, ColorF, _>.DrawLines(bitmap, points, color.ToColorF());
                return;
            }

            if (pixelFormat.Prefers64BitColors)
            {
                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                    GenericDrawer<BitmapDataAccessorPColor64, PColor64, _>.DrawLines(bitmap, points, color.ToPColor64());
                else
                    GenericDrawer<BitmapDataAccessorColor64, Color64, _>.DrawLines(bitmap, points, color.ToColor64());
                return;
            }

            if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
            {
                GenericDrawer<BitmapDataAccessorPColor32, PColor32, _>.DrawLines(bitmap, points, color.ToPColor32());
                return;
            }

            if (pixelFormat.Indexed)
            {
                GenericDrawer<BitmapDataAccessorIndexed, int, _>.DrawLines(bitmap, points, bitmapData.Palette!.GetNearestColorIndex(color));
                return;
            }

            GenericDrawer<BitmapDataAccessorColor32, Color32, _>.DrawLines(bitmap, points, color);
        }

        internal static void DrawLines(IReadWriteBitmapData bitmapData, IEnumerable<PointF> points, Color32 color, float offset)
        {
            PixelFormatInfo pixelFormat = bitmapData.PixelFormat;
            IBitmapDataInternal bitmap = bitmapData as IBitmapDataInternal ?? new BitmapDataWrapper(bitmapData, false, true);

            if (bitmapData is ICustomBitmapData)
            {
                // For linear gamma assuming the best performance with [P]ColorF even if the preferred color type is smaller.
                if (pixelFormat.Prefers128BitColors || pixelFormat.LinearGamma)
                {
                    if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: true })
                        GenericDrawer<CustomBitmapDataAccessorPColorF, PColorF, _>.DrawLines(bitmap, points, color.ToPColorF(), offset);
                    else
                        GenericDrawer<CustomBitmapDataAccessorColorF, ColorF, _>.DrawLines(bitmap, points, color.ToColorF(), offset);
                    return;
                }

                if (pixelFormat.Prefers64BitColors)
                {
                    if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                        GenericDrawer<CustomBitmapDataAccessorPColor64, PColor64, _>.DrawLines(bitmap, points, color.ToPColor64(), offset);
                    else
                        GenericDrawer<CustomBitmapDataAccessorColor64, Color64, _>.DrawLines(bitmap, points, color.ToColor64(), offset);
                    return;
                }

                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                {
                    GenericDrawer<CustomBitmapDataAccessorPColor32, PColor32, _>.DrawLines(bitmap, points, color.ToPColor32(), offset);
                    return;
                }

                if (pixelFormat.Indexed)
                {
                    GenericDrawer<CustomBitmapDataAccessorIndexed, int, _>.DrawLines(bitmap, points, bitmapData.Palette!.GetNearestColorIndex(color), offset);
                    return;
                }

                GenericDrawer<CustomBitmapDataAccessorColor32, Color32, _>.DrawLines(bitmap, points, color, offset);
                return;
            }

            // For linear gamma assuming the best performance with [P]ColorF even if the preferred color type is smaller.
            if (pixelFormat.Prefers128BitColors || pixelFormat.LinearGamma)
            {
                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: true })
                    GenericDrawer<BitmapDataAccessorPColorF, PColorF, _>.DrawLines(bitmap, points, color.ToPColorF(), offset);
                else
                    GenericDrawer<BitmapDataAccessorColorF, ColorF, _>.DrawLines(bitmap, points, color.ToColorF(), offset);
                return;
            }

            if (pixelFormat.Prefers64BitColors)
            {
                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                    GenericDrawer<BitmapDataAccessorPColor64, PColor64, _>.DrawLines(bitmap, points, color.ToPColor64(), offset);
                else
                    GenericDrawer<BitmapDataAccessorColor64, Color64, _>.DrawLines(bitmap, points, color.ToColor64(), offset);
                return;
            }

            if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
            {
                GenericDrawer<BitmapDataAccessorPColor32, PColor32, _>.DrawLines(bitmap, points, color.ToPColor32(), offset);
                return;
            }

            if (pixelFormat.Indexed)
            {
                GenericDrawer<BitmapDataAccessorIndexed, int, _>.DrawLines(bitmap, points, bitmapData.Palette!.GetNearestColorIndex(color), offset);
                return;
            }

            GenericDrawer<BitmapDataAccessorColor32, Color32, _>.DrawLines(bitmap, points, color, offset);
        }

        internal static void DrawBeziers(IReadWriteBitmapData bitmapData, IList<PointF> points, Color32 color, float offset)
        {
            Debug.Assert(points.Count == 0 || (points.Count - 1) % 3 == 0);
            if (points.Count == 0)
                return;

            PixelFormatInfo pixelFormat = bitmapData.PixelFormat;
            IBitmapDataInternal bitmap = bitmapData as IBitmapDataInternal ?? new BitmapDataWrapper(bitmapData, false, true);

            if (bitmapData is ICustomBitmapData)
            {
                // For linear gamma assuming the best performance with [P]ColorF even if the preferred color type is smaller.
                if (pixelFormat.Prefers128BitColors || pixelFormat.LinearGamma)
                {
                    if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: true })
                        GenericDrawer<CustomBitmapDataAccessorPColorF, PColorF, _>.DrawBeziers(bitmap, points, color.ToPColorF(), offset);
                    else
                        GenericDrawer<CustomBitmapDataAccessorColorF, ColorF, _>.DrawBeziers(bitmap, points, color.ToColorF(), offset);
                    return;
                }

                if (pixelFormat.Prefers64BitColors)
                {
                    if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                        GenericDrawer<CustomBitmapDataAccessorPColor64, PColor64, _>.DrawBeziers(bitmap, points, color.ToPColor64(), offset);
                    else
                        GenericDrawer<CustomBitmapDataAccessorColor64, Color64, _>.DrawBeziers(bitmap, points, color.ToColor64(), offset);
                    return;
                }

                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                {
                    GenericDrawer<CustomBitmapDataAccessorPColor32, PColor32, _>.DrawBeziers(bitmap, points, color.ToPColor32(), offset);
                    return;
                }

                if (pixelFormat.Indexed)
                {
                    GenericDrawer<CustomBitmapDataAccessorIndexed, int, _>.DrawBeziers(bitmap, points, bitmapData.Palette!.GetNearestColorIndex(color), offset);
                    return;
                }

                GenericDrawer<CustomBitmapDataAccessorColor32, Color32, _>.DrawBeziers(bitmap, points, color, offset);
                return;
            }

            // For linear gamma assuming the best performance with [P]ColorF even if the preferred color type is smaller.
            if (pixelFormat.Prefers128BitColors || pixelFormat.LinearGamma)
            {
                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: true })
                    GenericDrawer<BitmapDataAccessorPColorF, PColorF, _>.DrawBeziers(bitmap, points, color.ToPColorF(), offset);
                else
                    GenericDrawer<BitmapDataAccessorColorF, ColorF, _>.DrawBeziers(bitmap, points, color.ToColorF(), offset);
                return;
            }

            if (pixelFormat.Prefers64BitColors)
            {
                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                    GenericDrawer<BitmapDataAccessorPColor64, PColor64, _>.DrawBeziers(bitmap, points, color.ToPColor64(), offset);
                else
                    GenericDrawer<BitmapDataAccessorColor64, Color64, _>.DrawBeziers(bitmap, points, color.ToColor64(), offset);
                return;
            }

            if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
            {
                GenericDrawer<BitmapDataAccessorPColor32, PColor32, _>.DrawBeziers(bitmap, points, color.ToPColor32(), offset);
                return;
            }

            if (pixelFormat.Indexed)
            {
                GenericDrawer<BitmapDataAccessorIndexed, int, _>.DrawBeziers(bitmap, points, bitmapData.Palette!.GetNearestColorIndex(color), offset);
                return;
            }

            GenericDrawer<BitmapDataAccessorColor32, Color32, _>.DrawBeziers(bitmap, points, color, offset);
        }

        internal static void DrawPolygon(IReadWriteBitmapData bitmapData, IEnumerable<Point> points, Color32 color)
        {
            IList<Point> pointList = points as IList<Point> ?? new List<Point>(points);
            if (pointList.Count == 0)
                return;

            if (pointList[0] != pointList[pointList.Count - 1])
            {
                if (ReferenceEquals(points, pointList))
                    pointList = new List<Point>(pointList) { pointList[0] };
                else
                    pointList.Add(pointList[0]);
            }

            DrawLines(bitmapData, pointList, color);
        }

        internal static void DrawPolygon(IReadWriteBitmapData bitmapData, IEnumerable<PointF> points, Color32 color, float offset)
        {
            IList<PointF> pointList = points as IList<PointF> ?? new List<PointF>(points);
            if (pointList.Count == 0)
                return;

            if (pointList[0] != pointList[pointList.Count - 1])
            {
                if (ReferenceEquals(points, pointList))
                    pointList = new List<PointF>(pointList) { pointList[0] };
                else
                    pointList.Add(pointList[0]);
            }

            DrawLines(bitmapData, pointList, color, offset);
        }

        internal static void DrawRectangle(IReadWriteBitmapData bitmapData, Rectangle rectangle, Color32 color)
        {
            PixelFormatInfo pixelFormat = bitmapData.PixelFormat;
            IBitmapDataInternal bitmap = bitmapData as IBitmapDataInternal ?? new BitmapDataWrapper(bitmapData, false, true);

            if (bitmapData is ICustomBitmapData)
            {
                // For linear gamma assuming the best performance with [P]ColorF even if the preferred color type is smaller.
                if (pixelFormat.Prefers128BitColors || pixelFormat.LinearGamma)
                {
                    if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: true })
                        GenericDrawer<CustomBitmapDataAccessorPColorF, PColorF, _>.DrawRectangle(bitmap, rectangle, color.ToPColorF());
                    else
                        GenericDrawer<CustomBitmapDataAccessorColorF, ColorF, _>.DrawRectangle(bitmap, rectangle, color.ToColorF());
                    return;
                }

                if (pixelFormat.Prefers64BitColors)
                {
                    if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                        GenericDrawer<CustomBitmapDataAccessorPColor64, PColor64, _>.DrawRectangle(bitmap, rectangle, color.ToPColor64());
                    else
                        GenericDrawer<CustomBitmapDataAccessorColor64, Color64, _>.DrawRectangle(bitmap, rectangle, color.ToColor64());
                    return;
                }

                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                {
                    GenericDrawer<CustomBitmapDataAccessorPColor32, PColor32, _>.DrawRectangle(bitmap, rectangle, color.ToPColor32());
                    return;
                }

                if (pixelFormat.Indexed)
                {
                    GenericDrawer<CustomBitmapDataAccessorIndexed, int, _>.DrawRectangle(bitmap, rectangle, bitmapData.Palette!.GetNearestColorIndex(color));
                    return;
                }

                GenericDrawer<CustomBitmapDataAccessorColor32, Color32, _>.DrawRectangle(bitmap, rectangle, color);
                return;
            }

            // For linear gamma assuming the best performance with [P]ColorF even if the preferred color type is smaller.
            if (pixelFormat.Prefers128BitColors || pixelFormat.LinearGamma)
            {
                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: true })
                    GenericDrawer<BitmapDataAccessorPColorF, PColorF, _>.DrawRectangle(bitmap, rectangle, color.ToPColorF());
                else
                    GenericDrawer<BitmapDataAccessorColorF, ColorF, _>.DrawRectangle(bitmap, rectangle, color.ToColorF());
                return;
            }

            if (pixelFormat.Prefers64BitColors)
            {
                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                    GenericDrawer<BitmapDataAccessorPColor64, PColor64, _>.DrawRectangle(bitmap, rectangle, color.ToPColor64());
                else
                    GenericDrawer<BitmapDataAccessorColor64, Color64, _>.DrawRectangle(bitmap, rectangle, color.ToColor64());
                return;
            }

            if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
            {
                GenericDrawer<BitmapDataAccessorPColor32, PColor32, _>.DrawRectangle(bitmap, rectangle, color.ToPColor32());
                return;
            }

            if (pixelFormat.Indexed)
            {
                GenericDrawer<BitmapDataAccessorIndexed, int, _>.DrawRectangle(bitmap, rectangle, bitmapData.Palette!.GetNearestColorIndex(color));
                return;
            }

            GenericDrawer<BitmapDataAccessorColor32, Color32, _>.DrawRectangle(bitmap, rectangle, color);
        }

        internal static void DrawRectangle(IReadWriteBitmapData bitmapData, RectangleF rectangle, Color32 color, float offset)
        {
            PixelFormatInfo pixelFormat = bitmapData.PixelFormat;
            IBitmapDataInternal bitmap = bitmapData as IBitmapDataInternal ?? new BitmapDataWrapper(bitmapData, false, true);

            if (bitmapData is ICustomBitmapData)
            {
                // For linear gamma assuming the best performance with [P]ColorF even if the preferred color type is smaller.
                if (pixelFormat.Prefers128BitColors || pixelFormat.LinearGamma)
                {
                    if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: true })
                        GenericDrawer<CustomBitmapDataAccessorPColorF, PColorF, _>.DrawRectangle(bitmap, rectangle, color.ToPColorF(), offset);
                    else
                        GenericDrawer<CustomBitmapDataAccessorColorF, ColorF, _>.DrawRectangle(bitmap, rectangle, color.ToColorF(), offset);
                    return;
                }

                if (pixelFormat.Prefers64BitColors)
                {
                    if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                        GenericDrawer<CustomBitmapDataAccessorPColor64, PColor64, _>.DrawRectangle(bitmap, rectangle, color.ToPColor64(), offset);
                    else
                        GenericDrawer<CustomBitmapDataAccessorColor64, Color64, _>.DrawRectangle(bitmap, rectangle, color.ToColor64(), offset);
                    return;
                }

                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                {
                    GenericDrawer<CustomBitmapDataAccessorPColor32, PColor32, _>.DrawRectangle(bitmap, rectangle, color.ToPColor32(), offset);
                    return;
                }

                if (pixelFormat.Indexed)
                {
                    GenericDrawer<CustomBitmapDataAccessorIndexed, int, _>.DrawRectangle(bitmap, rectangle, bitmapData.Palette!.GetNearestColorIndex(color), offset);
                    return;
                }

                GenericDrawer<CustomBitmapDataAccessorColor32, Color32, _>.DrawRectangle(bitmap, rectangle, color, offset);
                return;
            }

            // For linear gamma assuming the best performance with [P]ColorF even if the preferred color type is smaller.
            if (pixelFormat.Prefers128BitColors || pixelFormat.LinearGamma)
            {
                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: true })
                    GenericDrawer<BitmapDataAccessorPColorF, PColorF, _>.DrawRectangle(bitmap, rectangle, color.ToPColorF(), offset);
                else
                    GenericDrawer<BitmapDataAccessorColorF, ColorF, _>.DrawRectangle(bitmap, rectangle, color.ToColorF(), offset);
                return;
            }

            if (pixelFormat.Prefers64BitColors)
            {
                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                    GenericDrawer<BitmapDataAccessorPColor64, PColor64, _>.DrawRectangle(bitmap, rectangle, color.ToPColor64(), offset);
                else
                    GenericDrawer<BitmapDataAccessorColor64, Color64, _>.DrawRectangle(bitmap, rectangle, color.ToColor64(), offset);
                return;
            }

            if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
            {
                GenericDrawer<BitmapDataAccessorPColor32, PColor32, _>.DrawRectangle(bitmap, rectangle, color.ToPColor32(), offset);
                return;
            }

            if (pixelFormat.Indexed)
            {
                GenericDrawer<BitmapDataAccessorIndexed, int, _>.DrawRectangle(bitmap, rectangle, bitmapData.Palette!.GetNearestColorIndex(color), offset);
                return;
            }

            GenericDrawer<BitmapDataAccessorColor32, Color32, _>.DrawRectangle(bitmap, rectangle, color, offset);
        }

        internal static void DrawEllipse(IReadWriteBitmapData bitmapData, Rectangle bounds, Color32 color)
        {
            PixelFormatInfo pixelFormat = bitmapData.PixelFormat;
            IBitmapDataInternal bitmap = bitmapData as IBitmapDataInternal ?? new BitmapDataWrapper(bitmapData, false, true);

            if (bitmapData is ICustomBitmapData)
            {
                // For linear gamma assuming the best performance with [P]ColorF even if the preferred color type is smaller.
                if (pixelFormat.Prefers128BitColors || pixelFormat.LinearGamma)
                {
                    if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: true })
                        GenericDrawer<CustomBitmapDataAccessorPColorF, PColorF, _>.DrawEllipse(bitmap, bounds, color.ToPColorF());
                    else
                        GenericDrawer<CustomBitmapDataAccessorColorF, ColorF, _>.DrawEllipse(bitmap, bounds, color.ToColorF());
                    return;
                }

                if (pixelFormat.Prefers64BitColors)
                {
                    if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                        GenericDrawer<CustomBitmapDataAccessorPColor64, PColor64, _>.DrawEllipse(bitmap, bounds, color.ToPColor64());
                    else
                        GenericDrawer<CustomBitmapDataAccessorColor64, Color64, _>.DrawEllipse(bitmap, bounds, color.ToColor64());
                    return;
                }

                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                {
                    GenericDrawer<CustomBitmapDataAccessorPColor32, PColor32, _>.DrawEllipse(bitmap, bounds, color.ToPColor32());
                    return;
                }

                if (pixelFormat.Indexed)
                {
                    GenericDrawer<CustomBitmapDataAccessorIndexed, int, _>.DrawEllipse(bitmap, bounds, bitmapData.Palette!.GetNearestColorIndex(color));
                    return;
                }

                GenericDrawer<CustomBitmapDataAccessorColor32, Color32, _>.DrawEllipse(bitmap, bounds, color);
                return;
            }

            // For linear gamma assuming the best performance with [P]ColorF even if the preferred color type is smaller.
            if (pixelFormat.Prefers128BitColors || pixelFormat.LinearGamma)
            {
                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: true })
                    GenericDrawer<BitmapDataAccessorPColorF, PColorF, _>.DrawEllipse(bitmap, bounds, color.ToPColorF());
                else
                    GenericDrawer<BitmapDataAccessorColorF, ColorF, _>.DrawEllipse(bitmap, bounds, color.ToColorF());
                return;
            }

            if (pixelFormat.Prefers64BitColors)
            {
                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                    GenericDrawer<BitmapDataAccessorPColor64, PColor64, _>.DrawEllipse(bitmap, bounds, color.ToPColor64());
                else
                    GenericDrawer<BitmapDataAccessorColor64, Color64, _>.DrawEllipse(bitmap, bounds, color.ToColor64());
                return;
            }

            if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
            {
                GenericDrawer<BitmapDataAccessorPColor32, PColor32, _>.DrawEllipse(bitmap, bounds, color.ToPColor32());
                return;
            }

            if (pixelFormat.Indexed)
            {
                GenericDrawer<BitmapDataAccessorIndexed, int, _>.DrawEllipse(bitmap, bounds, bitmapData.Palette!.GetNearestColorIndex(color));
                return;
            }

            GenericDrawer<BitmapDataAccessorColor32, Color32, _>.DrawEllipse(bitmap, bounds, color);
        }

        internal static void DrawEllipse(IReadWriteBitmapData bitmapData, RectangleF bounds, Color32 color, float offset)
        {
            PixelFormatInfo pixelFormat = bitmapData.PixelFormat;
            IBitmapDataInternal bitmap = bitmapData as IBitmapDataInternal ?? new BitmapDataWrapper(bitmapData, false, true);

            if (bitmapData is ICustomBitmapData)
            {
                // For linear gamma assuming the best performance with [P]ColorF even if the preferred color type is smaller.
                if (pixelFormat.Prefers128BitColors || pixelFormat.LinearGamma)
                {
                    if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: true })
                        GenericDrawer<CustomBitmapDataAccessorPColorF, PColorF, _>.DrawEllipse(bitmap, bounds, color.ToPColorF(), offset);
                    else
                        GenericDrawer<CustomBitmapDataAccessorColorF, ColorF, _>.DrawEllipse(bitmap, bounds, color.ToColorF(), offset);
                    return;
                }

                if (pixelFormat.Prefers64BitColors)
                {
                    if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                        GenericDrawer<CustomBitmapDataAccessorPColor64, PColor64, _>.DrawEllipse(bitmap, bounds, color.ToPColor64(), offset);
                    else
                        GenericDrawer<CustomBitmapDataAccessorColor64, Color64, _>.DrawEllipse(bitmap, bounds, color.ToColor64(), offset);
                    return;
                }

                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                {
                    GenericDrawer<CustomBitmapDataAccessorPColor32, PColor32, _>.DrawEllipse(bitmap, bounds, color.ToPColor32(), offset);
                    return;
                }

                if (pixelFormat.Indexed)
                {
                    GenericDrawer<CustomBitmapDataAccessorIndexed, int, _>.DrawEllipse(bitmap, bounds, bitmapData.Palette!.GetNearestColorIndex(color), offset);
                    return;
                }

                GenericDrawer<CustomBitmapDataAccessorColor32, Color32, _>.DrawEllipse(bitmap, bounds, color, offset);
                return;
            }

            // For linear gamma assuming the best performance with [P]ColorF even if the preferred color type is smaller.
            if (pixelFormat.Prefers128BitColors || pixelFormat.LinearGamma)
            {
                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: true })
                    GenericDrawer<BitmapDataAccessorPColorF, PColorF, _>.DrawEllipse(bitmap, bounds, color.ToPColorF(), offset);
                else
                    GenericDrawer<BitmapDataAccessorColorF, ColorF, _>.DrawEllipse(bitmap, bounds, color.ToColorF(), offset);
                return;
            }

            if (pixelFormat.Prefers64BitColors)
            {
                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                    GenericDrawer<BitmapDataAccessorPColor64, PColor64, _>.DrawEllipse(bitmap, bounds, color.ToPColor64(), offset);
                else
                    GenericDrawer<BitmapDataAccessorColor64, Color64, _>.DrawEllipse(bitmap, bounds, color.ToColor64(), offset);
                return;
            }

            if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
            {
                GenericDrawer<BitmapDataAccessorPColor32, PColor32, _>.DrawEllipse(bitmap, bounds, color.ToPColor32(), offset);
                return;
            }

            if (pixelFormat.Indexed)
            {
                GenericDrawer<BitmapDataAccessorIndexed, int, _>.DrawEllipse(bitmap, bounds, bitmapData.Palette!.GetNearestColorIndex(color), offset);
                return;
            }

            GenericDrawer<BitmapDataAccessorColor32, Color32, _>.DrawEllipse(bitmap, bounds, color, offset);
        }

        internal static void DrawArc(IReadWriteBitmapData bitmapData, Rectangle bounds, float startAngle, float sweepAngle, Color32 color)
        {
            PixelFormatInfo pixelFormat = bitmapData.PixelFormat;
            IBitmapDataInternal bitmap = bitmapData as IBitmapDataInternal ?? new BitmapDataWrapper(bitmapData, false, true);

            if (bitmapData is ICustomBitmapData)
            {
                // For linear gamma assuming the best performance with [P]ColorF even if the preferred color type is smaller.
                if (pixelFormat.Prefers128BitColors || pixelFormat.LinearGamma)
                {
                    if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: true })
                        GenericDrawer<CustomBitmapDataAccessorPColorF, PColorF, _>.DrawArc(bitmap, bounds, startAngle, sweepAngle, color.ToPColorF());
                    else
                        GenericDrawer<CustomBitmapDataAccessorColorF, ColorF, _>.DrawArc(bitmap, bounds, startAngle, sweepAngle, color.ToColorF());
                    return;
                }

                if (pixelFormat.Prefers64BitColors)
                {
                    if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                        GenericDrawer<CustomBitmapDataAccessorPColor64, PColor64, _>.DrawArc(bitmap, bounds, startAngle, sweepAngle, color.ToPColor64());
                    else
                        GenericDrawer<CustomBitmapDataAccessorColor64, Color64, _>.DrawArc(bitmap, bounds, startAngle, sweepAngle, color.ToColor64());
                    return;
                }

                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                {
                    GenericDrawer<CustomBitmapDataAccessorPColor32, PColor32, _>.DrawArc(bitmap, bounds, startAngle, sweepAngle, color.ToPColor32());
                    return;
                }

                if (pixelFormat.Indexed)
                {
                    GenericDrawer<CustomBitmapDataAccessorIndexed, int, _>.DrawArc(bitmap, bounds, startAngle, sweepAngle, bitmapData.Palette!.GetNearestColorIndex(color));
                    return;
                }

                GenericDrawer<CustomBitmapDataAccessorColor32, Color32, _>.DrawArc(bitmap, bounds, startAngle, sweepAngle, color);
                return;
            }

            // For linear gamma assuming the best performance with [P]ColorF even if the preferred color type is smaller.
            if (pixelFormat.Prefers128BitColors || pixelFormat.LinearGamma)
            {
                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: true })
                    GenericDrawer<BitmapDataAccessorPColorF, PColorF, _>.DrawArc(bitmap, bounds, startAngle, sweepAngle, color.ToPColorF());
                else
                    GenericDrawer<BitmapDataAccessorColorF, ColorF, _>.DrawArc(bitmap, bounds, startAngle, sweepAngle, color.ToColorF());
                return;
            }

            if (pixelFormat.Prefers64BitColors)
            {
                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                    GenericDrawer<BitmapDataAccessorPColor64, PColor64, _>.DrawArc(bitmap, bounds, startAngle, sweepAngle, color.ToPColor64());
                else
                    GenericDrawer<BitmapDataAccessorColor64, Color64, _>.DrawArc(bitmap, bounds, startAngle, sweepAngle, color.ToColor64());
                return;
            }

            if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
            {
                GenericDrawer<BitmapDataAccessorPColor32, PColor32, _>.DrawArc(bitmap, bounds, startAngle, sweepAngle, color.ToPColor32());
                return;
            }

            if (pixelFormat.Indexed)
            {
                GenericDrawer<BitmapDataAccessorIndexed, int, _>.DrawArc(bitmap, bounds, startAngle, sweepAngle, bitmapData.Palette!.GetNearestColorIndex(color));
                return;
            }

            GenericDrawer<BitmapDataAccessorColor32, Color32, _>.DrawArc(bitmap, bounds, startAngle, sweepAngle, color);
        }

        internal static void DrawArc(IReadWriteBitmapData bitmapData, RectangleF bounds, float startAngle, float sweepAngle, Color32 color, float offset)
        {
            PixelFormatInfo pixelFormat = bitmapData.PixelFormat;
            IBitmapDataInternal bitmap = bitmapData as IBitmapDataInternal ?? new BitmapDataWrapper(bitmapData, false, true);

            if (bitmapData is ICustomBitmapData)
            {
                // For linear gamma assuming the best performance with [P]ColorF even if the preferred color type is smaller.
                if (pixelFormat.Prefers128BitColors || pixelFormat.LinearGamma)
                {
                    if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: true })
                        GenericDrawer<CustomBitmapDataAccessorPColorF, PColorF, _>.DrawArc(bitmap, bounds, startAngle, sweepAngle, color.ToPColorF(), offset);
                    else
                        GenericDrawer<CustomBitmapDataAccessorColorF, ColorF, _>.DrawArc(bitmap, bounds, startAngle, sweepAngle, color.ToColorF(), offset);
                    return;
                }

                if (pixelFormat.Prefers64BitColors)
                {
                    if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                        GenericDrawer<CustomBitmapDataAccessorPColor64, PColor64, _>.DrawArc(bitmap, bounds, startAngle, sweepAngle, color.ToPColor64(), offset);
                    else
                        GenericDrawer<CustomBitmapDataAccessorColor64, Color64, _>.DrawArc(bitmap, bounds, startAngle, sweepAngle, color.ToColor64(), offset);
                    return;
                }

                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                {
                    GenericDrawer<CustomBitmapDataAccessorPColor32, PColor32, _>.DrawArc(bitmap, bounds, startAngle, sweepAngle, color.ToPColor32(), offset);
                    return;
                }

                if (pixelFormat.Indexed)
                {
                    GenericDrawer<CustomBitmapDataAccessorIndexed, int, _>.DrawArc(bitmap, bounds, startAngle, sweepAngle, bitmapData.Palette!.GetNearestColorIndex(color), offset);
                    return;
                }

                GenericDrawer<CustomBitmapDataAccessorColor32, Color32, _>.DrawArc(bitmap, bounds, startAngle, sweepAngle, color, offset);
                return;
            }

            // For linear gamma assuming the best performance with [P]ColorF even if the preferred color type is smaller.
            if (pixelFormat.Prefers128BitColors || pixelFormat.LinearGamma)
            {
                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: true })
                    GenericDrawer<BitmapDataAccessorPColorF, PColorF, _>.DrawArc(bitmap, bounds, startAngle, sweepAngle, color.ToPColorF(), offset);
                else
                    GenericDrawer<BitmapDataAccessorColorF, ColorF, _>.DrawArc(bitmap, bounds, startAngle, sweepAngle, color.ToColorF(), offset);
                return;
            }

            if (pixelFormat.Prefers64BitColors)
            {
                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                    GenericDrawer<BitmapDataAccessorPColor64, PColor64, _>.DrawArc(bitmap, bounds, startAngle, sweepAngle, color.ToPColor64(), offset);
                else
                    GenericDrawer<BitmapDataAccessorColor64, Color64, _>.DrawArc(bitmap, bounds, startAngle, sweepAngle, color.ToColor64(), offset);
                return;
            }

            if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
            {
                GenericDrawer<BitmapDataAccessorPColor32, PColor32, _>.DrawArc(bitmap, bounds, startAngle, sweepAngle, color.ToPColor32(), offset);
                return;
            }

            if (pixelFormat.Indexed)
            {
                GenericDrawer<BitmapDataAccessorIndexed, int, _>.DrawArc(bitmap, bounds, startAngle, sweepAngle, bitmapData.Palette!.GetNearestColorIndex(color), offset);
                return;
            }

            GenericDrawer<BitmapDataAccessorColor32, Color32, _>.DrawArc(bitmap, bounds, startAngle, sweepAngle, color, offset);
        }

        internal static void DrawArc(IReadWriteBitmapData bitmapData, PointF startPoint, PointF endPoint, float radiusX, float radiusY, float rotationAngle, bool isLargeArc, bool isClockwise, Color32 color, float offset)
        {
            PixelFormatInfo pixelFormat = bitmapData.PixelFormat;
            IBitmapDataInternal bitmap = bitmapData as IBitmapDataInternal ?? new BitmapDataWrapper(bitmapData, false, true);

            if (bitmapData is ICustomBitmapData)
            {
                // For linear gamma assuming the best performance with [P]ColorF even if the preferred color type is smaller.
                if (pixelFormat.Prefers128BitColors || pixelFormat.LinearGamma)
                {
                    if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: true })
                        GenericDrawer<CustomBitmapDataAccessorPColorF, PColorF, _>.DrawArc(bitmap, startPoint, endPoint, radiusX, radiusY, rotationAngle, isLargeArc, isClockwise, color.ToPColorF(), offset);
                    else
                        GenericDrawer<CustomBitmapDataAccessorColorF, ColorF, _>.DrawArc(bitmap, startPoint, endPoint, radiusX, radiusY, rotationAngle, isLargeArc, isClockwise, color.ToColorF(), offset);
                    return;
                }

                if (pixelFormat.Prefers64BitColors)
                {
                    if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                        GenericDrawer<CustomBitmapDataAccessorPColor64, PColor64, _>.DrawArc(bitmap, startPoint, endPoint, radiusX, radiusY, rotationAngle, isLargeArc, isClockwise, color.ToPColor64(), offset);
                    else
                        GenericDrawer<CustomBitmapDataAccessorColor64, Color64, _>.DrawArc(bitmap, startPoint, endPoint, radiusX, radiusY, rotationAngle, isLargeArc, isClockwise, color.ToColor64(), offset);
                    return;
                }

                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                {
                    GenericDrawer<CustomBitmapDataAccessorPColor32, PColor32, _>.DrawArc(bitmap, startPoint, endPoint, radiusX, radiusY, rotationAngle, isLargeArc, isClockwise, color.ToPColor32(), offset);
                    return;
                }

                if (pixelFormat.Indexed)
                {
                    GenericDrawer<CustomBitmapDataAccessorIndexed, int, _>.DrawArc(bitmap, startPoint, endPoint, radiusX, radiusY, rotationAngle, isLargeArc, isClockwise, bitmapData.Palette!.GetNearestColorIndex(color), offset);
                    return;
                }

                GenericDrawer<CustomBitmapDataAccessorColor32, Color32, _>.DrawArc(bitmap, startPoint, endPoint, radiusX, radiusY, rotationAngle, isLargeArc, isClockwise, color, offset);
                return;
            }

            // For linear gamma assuming the best performance with [P]ColorF even if the preferred color type is smaller.
            if (pixelFormat.Prefers128BitColors || pixelFormat.LinearGamma)
            {
                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: true })
                    GenericDrawer<BitmapDataAccessorPColorF, PColorF, _>.DrawArc(bitmap, startPoint, endPoint, radiusX, radiusY, rotationAngle, isLargeArc, isClockwise, color.ToPColorF(), offset);
                else
                    GenericDrawer<BitmapDataAccessorColorF, ColorF, _>.DrawArc(bitmap, startPoint, endPoint, radiusX, radiusY, rotationAngle, isLargeArc, isClockwise, color.ToColorF(), offset);
                return;
            }

            if (pixelFormat.Prefers64BitColors)
            {
                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                    GenericDrawer<BitmapDataAccessorPColor64, PColor64, _>.DrawArc(bitmap, startPoint, endPoint, radiusX, radiusY, rotationAngle, isLargeArc, isClockwise, color.ToPColor64(), offset);
                else
                    GenericDrawer<BitmapDataAccessorColor64, Color64, _>.DrawArc(bitmap, startPoint, endPoint, radiusX, radiusY, rotationAngle, isLargeArc, isClockwise, color.ToColor64(), offset);
                return;
            }

            if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
            {
                GenericDrawer<BitmapDataAccessorPColor32, PColor32, _>.DrawArc(bitmap, startPoint, endPoint, radiusX, radiusY, rotationAngle, isLargeArc, isClockwise, color.ToPColor32(), offset);
                return;
            }

            if (pixelFormat.Indexed)
            {
                GenericDrawer<BitmapDataAccessorIndexed, int, _>.DrawArc(bitmap, startPoint, endPoint, radiusX, radiusY, rotationAngle, isLargeArc, isClockwise, bitmapData.Palette!.GetNearestColorIndex(color), offset);
                return;
            }

            GenericDrawer<BitmapDataAccessorColor32, Color32, _>.DrawArc(bitmap, startPoint, endPoint, radiusX, radiusY, rotationAngle, isLargeArc, isClockwise, color, offset);
        }

        internal static void DrawPie(IReadWriteBitmapData bitmapData, RectangleF bounds, float startAngle, float sweepAngle, Color32 color, float offset)
        {
            PixelFormatInfo pixelFormat = bitmapData.PixelFormat;
            IBitmapDataInternal bitmap = bitmapData as IBitmapDataInternal ?? new BitmapDataWrapper(bitmapData, false, true);

            if (bitmapData is ICustomBitmapData)
            {
                // For linear gamma assuming the best performance with [P]ColorF even if the preferred color type is smaller.
                if (pixelFormat.Prefers128BitColors || pixelFormat.LinearGamma)
                {
                    if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: true })
                        GenericDrawer<CustomBitmapDataAccessorPColorF, PColorF, _>.DrawPie(bitmap, bounds, startAngle, sweepAngle, color.ToPColorF(), offset);
                    else
                        GenericDrawer<CustomBitmapDataAccessorColorF, ColorF, _>.DrawPie(bitmap, bounds, startAngle, sweepAngle, color.ToColorF(), offset);
                    return;
                }

                if (pixelFormat.Prefers64BitColors)
                {
                    if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                        GenericDrawer<CustomBitmapDataAccessorPColor64, PColor64, _>.DrawPie(bitmap, bounds, startAngle, sweepAngle, color.ToPColor64(), offset);
                    else
                        GenericDrawer<CustomBitmapDataAccessorColor64, Color64, _>.DrawPie(bitmap, bounds, startAngle, sweepAngle, color.ToColor64(), offset);
                    return;
                }

                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                {
                    GenericDrawer<CustomBitmapDataAccessorPColor32, PColor32, _>.DrawPie(bitmap, bounds, startAngle, sweepAngle, color.ToPColor32(), offset);
                    return;
                }

                if (pixelFormat.Indexed)
                {
                    GenericDrawer<CustomBitmapDataAccessorIndexed, int, _>.DrawPie(bitmap, bounds, startAngle, sweepAngle, bitmapData.Palette!.GetNearestColorIndex(color), offset);
                    return;
                }

                GenericDrawer<CustomBitmapDataAccessorColor32, Color32, _>.DrawPie(bitmap, bounds, startAngle, sweepAngle, color, offset);
                return;
            }

            // For linear gamma assuming the best performance with [P]ColorF even if the preferred color type is smaller.
            if (pixelFormat.Prefers128BitColors || pixelFormat.LinearGamma)
            {
                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: true })
                    GenericDrawer<BitmapDataAccessorPColorF, PColorF, _>.DrawPie(bitmap, bounds, startAngle, sweepAngle, color.ToPColorF(), offset);
                else
                    GenericDrawer<BitmapDataAccessorColorF, ColorF, _>.DrawPie(bitmap, bounds, startAngle, sweepAngle, color.ToColorF(), offset);
                return;
            }

            if (pixelFormat.Prefers64BitColors)
            {
                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                    GenericDrawer<BitmapDataAccessorPColor64, PColor64, _>.DrawPie(bitmap, bounds, startAngle, sweepAngle, color.ToPColor64(), offset);
                else
                    GenericDrawer<BitmapDataAccessorColor64, Color64, _>.DrawPie(bitmap, bounds, startAngle, sweepAngle, color.ToColor64(), offset);
                return;
            }

            if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
            {
                GenericDrawer<BitmapDataAccessorPColor32, PColor32, _>.DrawPie(bitmap, bounds, startAngle, sweepAngle, color.ToPColor32(), offset);
                return;
            }

            if (pixelFormat.Indexed)
            {
                GenericDrawer<BitmapDataAccessorIndexed, int, _>.DrawPie(bitmap, bounds, startAngle, sweepAngle, bitmapData.Palette!.GetNearestColorIndex(color), offset);
                return;
            }

            GenericDrawer<BitmapDataAccessorColor32, Color32, _>.DrawPie(bitmap, bounds, startAngle, sweepAngle, color, offset);
        }

        internal static void DrawRoundedRectangle(IReadWriteBitmapData bitmapData, Rectangle bounds, int cornerRadius, Color32 color)
        {
            PixelFormatInfo pixelFormat = bitmapData.PixelFormat;
            IBitmapDataInternal bitmap = bitmapData as IBitmapDataInternal ?? new BitmapDataWrapper(bitmapData, false, true);

            if (bitmapData is ICustomBitmapData)
            {
                // For linear gamma assuming the best performance with [P]ColorF even if the preferred color type is smaller.
                if (pixelFormat.Prefers128BitColors || pixelFormat.LinearGamma)
                {
                    if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: true })
                        GenericDrawer<CustomBitmapDataAccessorPColorF, PColorF, _>.DrawRoundedRectangle(bitmap, bounds, cornerRadius, color.ToPColorF());
                    else
                        GenericDrawer<CustomBitmapDataAccessorColorF, ColorF, _>.DrawRoundedRectangle(bitmap, bounds, cornerRadius, color.ToColorF());
                    return;
                }

                if (pixelFormat.Prefers64BitColors)
                {
                    if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                        GenericDrawer<CustomBitmapDataAccessorPColor64, PColor64, _>.DrawRoundedRectangle(bitmap, bounds, cornerRadius, color.ToPColor64());
                    else
                        GenericDrawer<CustomBitmapDataAccessorColor64, Color64, _>.DrawRoundedRectangle(bitmap, bounds, cornerRadius, color.ToColor64());
                    return;
                }

                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                {
                    GenericDrawer<CustomBitmapDataAccessorPColor32, PColor32, _>.DrawRoundedRectangle(bitmap, bounds, cornerRadius, color.ToPColor32());
                    return;
                }

                if (pixelFormat.Indexed)
                {
                    GenericDrawer<CustomBitmapDataAccessorIndexed, int, _>.DrawRoundedRectangle(bitmap, bounds, cornerRadius, bitmapData.Palette!.GetNearestColorIndex(color));
                    return;
                }

                GenericDrawer<CustomBitmapDataAccessorColor32, Color32, _>.DrawRoundedRectangle(bitmap, bounds, cornerRadius, color);
                return;
            }

            // For linear gamma assuming the best performance with [P]ColorF even if the preferred color type is smaller.
            if (pixelFormat.Prefers128BitColors || pixelFormat.LinearGamma)
            {
                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: true })
                    GenericDrawer<BitmapDataAccessorPColorF, PColorF, _>.DrawRoundedRectangle(bitmap, bounds, cornerRadius, color.ToPColorF());
                else
                    GenericDrawer<BitmapDataAccessorColorF, ColorF, _>.DrawRoundedRectangle(bitmap, bounds, cornerRadius, color.ToColorF());
                return;
            }

            if (pixelFormat.Prefers64BitColors)
            {
                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                    GenericDrawer<BitmapDataAccessorPColor64, PColor64, _>.DrawRoundedRectangle(bitmap, bounds, cornerRadius, color.ToPColor64());
                else
                    GenericDrawer<BitmapDataAccessorColor64, Color64, _>.DrawRoundedRectangle(bitmap, bounds, cornerRadius, color.ToColor64());
                return;
            }

            if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
            {
                GenericDrawer<BitmapDataAccessorPColor32, PColor32, _>.DrawRoundedRectangle(bitmap, bounds, cornerRadius, color.ToPColor32());
                return;
            }

            if (pixelFormat.Indexed)
            {
                GenericDrawer<BitmapDataAccessorIndexed, int, _>.DrawRoundedRectangle(bitmap, bounds, cornerRadius, bitmapData.Palette!.GetNearestColorIndex(color));
                return;
            }

            GenericDrawer<BitmapDataAccessorColor32, Color32, _>.DrawRoundedRectangle(bitmap, bounds, cornerRadius, color);
        }

        internal static void DrawRoundedRectangle(IReadWriteBitmapData bitmapData, RectangleF bounds, float cornerRadius, Color32 color, float offset)
        {
            PixelFormatInfo pixelFormat = bitmapData.PixelFormat;
            IBitmapDataInternal bitmap = bitmapData as IBitmapDataInternal ?? new BitmapDataWrapper(bitmapData, false, true);

            if (bitmapData is ICustomBitmapData)
            {
                // For linear gamma assuming the best performance with [P]ColorF even if the preferred color type is smaller.
                if (pixelFormat.Prefers128BitColors || pixelFormat.LinearGamma)
                {
                    if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: true })
                        GenericDrawer<CustomBitmapDataAccessorPColorF, PColorF, _>.DrawRoundedRectangle(bitmap, bounds, cornerRadius, color.ToPColorF(), offset);
                    else
                        GenericDrawer<CustomBitmapDataAccessorColorF, ColorF, _>.DrawRoundedRectangle(bitmap, bounds, cornerRadius, color.ToColorF(), offset);
                    return;
                }

                if (pixelFormat.Prefers64BitColors)
                {
                    if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                        GenericDrawer<CustomBitmapDataAccessorPColor64, PColor64, _>.DrawRoundedRectangle(bitmap, bounds, cornerRadius, color.ToPColor64(), offset);
                    else
                        GenericDrawer<CustomBitmapDataAccessorColor64, Color64, _>.DrawRoundedRectangle(bitmap, bounds, cornerRadius, color.ToColor64(), offset);
                    return;
                }

                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                {
                    GenericDrawer<CustomBitmapDataAccessorPColor32, PColor32, _>.DrawRoundedRectangle(bitmap, bounds, cornerRadius, color.ToPColor32(), offset);
                    return;
                }

                if (pixelFormat.Indexed)
                {
                    GenericDrawer<CustomBitmapDataAccessorIndexed, int, _>.DrawRoundedRectangle(bitmap, bounds, cornerRadius, bitmapData.Palette!.GetNearestColorIndex(color), offset);
                    return;
                }

                GenericDrawer<CustomBitmapDataAccessorColor32, Color32, _>.DrawRoundedRectangle(bitmap, bounds, cornerRadius, color, offset);
                return;
            }

            // For linear gamma assuming the best performance with [P]ColorF even if the preferred color type is smaller.
            if (pixelFormat.Prefers128BitColors || pixelFormat.LinearGamma)
            {
                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: true })
                    GenericDrawer<BitmapDataAccessorPColorF, PColorF, _>.DrawRoundedRectangle(bitmap, bounds, cornerRadius, color.ToPColorF(), offset);
                else
                    GenericDrawer<BitmapDataAccessorColorF, ColorF, _>.DrawRoundedRectangle(bitmap, bounds, cornerRadius, color.ToColorF(), offset);
                return;
            }

            if (pixelFormat.Prefers64BitColors)
            {
                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                    GenericDrawer<BitmapDataAccessorPColor64, PColor64, _>.DrawRoundedRectangle(bitmap, bounds, cornerRadius, color.ToPColor64(), offset);
                else
                    GenericDrawer<BitmapDataAccessorColor64, Color64, _>.DrawRoundedRectangle(bitmap, bounds, cornerRadius, color.ToColor64(), offset);
                return;
            }

            if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
            {
                GenericDrawer<BitmapDataAccessorPColor32, PColor32, _>.DrawRoundedRectangle(bitmap, bounds, cornerRadius, color.ToPColor32(), offset);
                return;
            }

            if (pixelFormat.Indexed)
            {
                GenericDrawer<BitmapDataAccessorIndexed, int, _>.DrawRoundedRectangle(bitmap, bounds, cornerRadius, bitmapData.Palette!.GetNearestColorIndex(color), offset);
                return;
            }

            GenericDrawer<BitmapDataAccessorColor32, Color32, _>.DrawRoundedRectangle(bitmap, bounds, cornerRadius, color, offset);
        }

        internal static void DrawRoundedRectangle(IReadWriteBitmapData bitmapData, RectangleF bounds,
            float radiusTopLeft, float radiusTopRight, float radiusBottomRight, float radiusBottomLeft, Color32 color, float offset)
        {
            PixelFormatInfo pixelFormat = bitmapData.PixelFormat;
            IBitmapDataInternal bitmap = bitmapData as IBitmapDataInternal ?? new BitmapDataWrapper(bitmapData, false, true);

            if (bitmapData is ICustomBitmapData)
            {
                // For linear gamma assuming the best performance with [P]ColorF even if the preferred color type is smaller.
                if (pixelFormat.Prefers128BitColors || pixelFormat.LinearGamma)
                {
                    if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: true })
                        GenericDrawer<CustomBitmapDataAccessorPColorF, PColorF, _>.DrawRoundedRectangle(bitmap, bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, color.ToPColorF(), offset);
                    else
                        GenericDrawer<CustomBitmapDataAccessorColorF, ColorF, _>.DrawRoundedRectangle(bitmap, bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, color.ToColorF(), offset);
                    return;
                }

                if (pixelFormat.Prefers64BitColors)
                {
                    if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                        GenericDrawer<CustomBitmapDataAccessorPColor64, PColor64, _>.DrawRoundedRectangle(bitmap, bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, color.ToPColor64(), offset);
                    else
                        GenericDrawer<CustomBitmapDataAccessorColor64, Color64, _>.DrawRoundedRectangle(bitmap, bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, color.ToColor64(), offset);
                    return;
                }

                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                {
                    GenericDrawer<CustomBitmapDataAccessorPColor32, PColor32, _>.DrawRoundedRectangle(bitmap, bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, color.ToPColor32(), offset);
                    return;
                }

                if (pixelFormat.Indexed)
                {
                    GenericDrawer<CustomBitmapDataAccessorIndexed, int, _>.DrawRoundedRectangle(bitmap, bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, bitmapData.Palette!.GetNearestColorIndex(color), offset);
                    return;
                }

                GenericDrawer<CustomBitmapDataAccessorColor32, Color32, _>.DrawRoundedRectangle(bitmap, bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, color, offset);
                return;
            }

            // For linear gamma assuming the best performance with [P]ColorF even if the preferred color type is smaller.
            if (pixelFormat.Prefers128BitColors || pixelFormat.LinearGamma)
            {
                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: true })
                    GenericDrawer<BitmapDataAccessorPColorF, PColorF, _>.DrawRoundedRectangle(bitmap, bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, color.ToPColorF(), offset);
                else
                    GenericDrawer<BitmapDataAccessorColorF, ColorF, _>.DrawRoundedRectangle(bitmap, bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, color.ToColorF(), offset);
                return;
            }

            if (pixelFormat.Prefers64BitColors)
            {
                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                    GenericDrawer<BitmapDataAccessorPColor64, PColor64, _>.DrawRoundedRectangle(bitmap, bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, color.ToPColor64(), offset);
                else
                    GenericDrawer<BitmapDataAccessorColor64, Color64, _>.DrawRoundedRectangle(bitmap, bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, color.ToColor64(), offset);
                return;
            }

            if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
            {
                GenericDrawer<BitmapDataAccessorPColor32, PColor32, _>.DrawRoundedRectangle(bitmap, bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, color.ToPColor32(), offset);
                return;
            }

            if (pixelFormat.Indexed)
            {
                GenericDrawer<BitmapDataAccessorIndexed, int, _>.DrawRoundedRectangle(bitmap, bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, bitmapData.Palette!.GetNearestColorIndex(color), offset);
                return;
            }

            GenericDrawer<BitmapDataAccessorColor32, Color32, _>.DrawRoundedRectangle(bitmap, bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, color, offset);
        }

        internal static bool FillRectangle(IAsyncContext context, IReadWriteBitmapData bitmapData, Rectangle rectangle, Color32 color)
        {
            rectangle = rectangle.IntersectSafe(new Rectangle(Point.Empty, bitmapData.Size));
            if (rectangle.IsEmpty())
                return !context.IsCancellationRequested;

            // we could use a ClippedBitmapData here, but it would be slower because of the extra wrapping
            PixelFormatInfo pixelFormat = bitmapData.PixelFormat;
            IBitmapDataInternal bitmap = bitmapData as IBitmapDataInternal ?? new BitmapDataWrapper(bitmapData, false, true);

            // For linear gamma assuming the best performance with [P]ColorF even if the preferred color type is smaller.
            if (pixelFormat.Prefers128BitColors || pixelFormat.LinearGamma)
            {
                return pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: true }
                    ? GenericDrawer<BitmapDataAccessorPColorF, PColorF, _>.FillRectangle(context, bitmap, color.ToPColorF(), rectangle)
                    : GenericDrawer<BitmapDataAccessorColorF, ColorF, _>.FillRectangle(context, bitmap, color.ToColorF(), rectangle);
            }

            if (pixelFormat.Prefers64BitColors)
            {
                return pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false }
                    ? GenericDrawer<BitmapDataAccessorPColor64, PColor64, _>.FillRectangle(context, bitmap, color.ToPColor64(), rectangle)
                    : GenericDrawer<BitmapDataAccessorColor64, Color64, _>.FillRectangle(context, bitmap, color.ToColor64(), rectangle);
            }

            return pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false }
                ? GenericDrawer<BitmapDataAccessorPColor32, PColor32, _>.FillRectangle(context, bitmap, color.ToPColor32(), rectangle)
                : pixelFormat.Indexed
                    ? GenericDrawer<BitmapDataAccessorIndexed, int, _>.FillRectangle(context, bitmap, bitmapData.Palette!.GetNearestColorIndex(color), rectangle)
                    : GenericDrawer<BitmapDataAccessorColor32, Color32, _>.FillRectangle(context, bitmap, color, rectangle);
        }

        #endregion
    }
}
