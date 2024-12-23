#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: DirectDrawer.cs
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
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.CompilerServices;

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

            internal static void DrawLine(IBitmapDataInternal bitmapData, Point p1, Point p2, TColor c, TArg arg = default!)
            {
                var accessor = new TAccessor();
                Size size = bitmapData.Size;

                // horizontal line (or a single point)
                if (p1.Y == p2.Y)
                {
                    if ((uint)p1.Y >= (uint)(size.Height))
                        return;

                    accessor.InitRow(bitmapData.GetRowCached(p1.Y), arg);
                    if (p1.X > p2.X)
                        (p1.X, p2.X) = (p2.X, p1.X);

                    int max = Math.Min(p2.X, size.Width - 1);
                    for (int x = Math.Max(p1.X, 0); x <= max; x++)
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
                    for (int y = Math.Max(p1.Y, 0); y <= max; y++)
                        accessor.SetColor(p1.X, y, c);

                    return;
                }

                // general line
                long width = ((long)p2.X - p1.X).Abs();
                long height = ((long)p2.Y - p1.Y).Abs();

                if (width >= height)
                {
                    long numerator = width >> 1;
                    if (p1.X > p2.X)
                        (p1, p2) = (p2, p1);
                    int step = p2.Y > p1.Y ? 1 : -1;
                    int x = p1.X;
                    int y = p1.Y;

                    // skipping invisible X coordinates
                    if (x < 0)
                    {
                        numerator = (numerator - height * x) % width;
                        y -= x * step;
                        x = 0;
                    }

                    int endX = Math.Min(p2.X, size.Width - 1);
                    int offY = step > 0 ? Math.Min(p2.Y, size.Height - 1) + 1 : Math.Max(p2.Y, 0) - 1;
                    for (; x <= endX; x++)
                    {
                        // Drawing only if Y is visible
                        if ((uint)y < (uint)size.Height)
                            accessor.SetColor(x, y, c);
                        numerator += height;
                        if (numerator < width)
                            continue;

                        y += step;
                        if (y == offY)
                            return;
                        numerator -= width;
                    }
                }
                else
                {
                    long numerator = height >> 1;
                    if (p1.Y > p2.Y)
                        (p1, p2) = (p2, p1);
                    int step = p2.X > p1.X ? 1 : -1;
                    int x = p1.X;
                    int y = p1.Y;

                    // skipping invisible Y coordinates
                    if (y < 0)
                    {
                        numerator = (numerator - width * y) % height;
                        x -= y * step;
                        y = 0;
                    }

                    int endY = Math.Min(p2.Y, size.Height - 1);
                    int offX = step > 0 ? Math.Min(p2.X, size.Width - 1) + 1 : Math.Max(p2.X, 0) - 1;
                    for (; y <= endY; y++)
                    {
                        // Drawing only if X is visible
                        if ((uint)x < (uint)size.Width)
                            accessor.SetColor(x, y, c);
                        numerator += width;
                        if (numerator < height)
                            continue;

                        x += step;
                        if (x == offX)
                            return;
                        numerator -= height;
                    }
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
            internal static void DrawBeziers(IBitmapDataInternal bitmapData, List<PointF> points, TColor c, float offset, TArg arg = default!)
            {
                Debug.Assert((points.Count - 1) % 3 == 0);
                DrawLines(bitmapData, new BezierSegment(points).GetFlattenedPoints(), c, offset, arg);
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
                if (bounds.Width > ArcSegment.DrawAsLinesThreshold || bounds.Height > ArcSegment.DrawAsLinesThreshold)
                {
                    DrawLines(bitmapData, new ArcSegment(bounds).GetFlattenedPoints(), c, offset);
                    return;
                }

                (Point p1, Point p2) = Round(bounds.Location, bounds.Size.ToPointF(), offset);
                DoDrawEllipse(bitmapData, new Rectangle(p1.X, p1.Y, p2.X, p2.Y), c, arg);
            }

            [MethodImpl(MethodImpl.AggressiveInlining)]
            internal static void DrawEllipse(IBitmapDataInternal bitmapData, Rectangle bounds, TColor c, TArg arg = default!)
            {
                if (bounds.Width > ArcSegment.DrawAsLinesThreshold || bounds.Height > ArcSegment.DrawAsLinesThreshold)
                {
                    DrawLines(bitmapData, new ArcSegment(bounds).GetFlattenedPoints(), c, 0f);
                    return;
                }

                DoDrawEllipse(bitmapData, bounds, c, arg);
            }

            [MethodImpl(MethodImpl.AggressiveInlining)]
            internal static void DrawArc(IBitmapDataInternal bitmapData, ArcSegment arc, TColor c, float offset, TArg arg = default!)
            {
                Debug.Assert(Math.Abs(arc.SweepAngle) < 360f && arc.Width <= ArcSegment.DrawAsLinesThreshold && arc.Height <= ArcSegment.DrawAsLinesThreshold);
                RectangleF bounds = arc.Bounds;
                (Point p1, Point p2) = Round(bounds.Location, bounds.Location + bounds.Size, offset);
                (int left, int right) = p2.X >= p1.X ? (p1.X, p2.X) : (p2.X, p1.X);
                (int top, int bottom) = p2.Y >= p1.Y ? (p1.Y, p2.Y) : (p2.Y, p1.Y);

                // Not using arc.RadiusX/Y here because that is shorter by a half pixel (even if there is no rounding error)
                // because ArcSegment has no concept of line width, and here we draw a 1px wide path.
                float centerX = (left + right + 1) / 2f;
                float radiusX = ((right - left) + 1) / 2f;
                float radiusY = ((bottom - top) + 1) / 2f;

                (float startRad, float endRad) = arc.GetStartEndRadians();
                ArcSegment.AdjustAngles(ref startRad, ref endRad, radiusX, radiusY);

                DoDrawArc(bitmapData, left, top, right, bottom, c, arc.GetSectors(),
                    (int)(centerX + radiusX * MathF.Cos(startRad)), (int)(centerX + radiusX * MathF.Cos(endRad)), arg);
            }

            [MethodImpl(MethodImpl.AggressiveInlining)]
            internal static void DrawArc(IBitmapDataInternal bitmapData, RectangleF bounds, float startAngle, float sweepAngle, TColor c, float offset, TArg arg = default!)
            {
                if (bounds.Width > ArcSegment.DrawAsLinesThreshold || bounds.Height > ArcSegment.DrawAsLinesThreshold)
                {
                    DrawLines(bitmapData, new ArcSegment(bounds, startAngle, sweepAngle).GetFlattenedPoints(), c, offset);
                    return;
                }

                (Point p1, Point p2) = Round(bounds.Location, bounds.Size.ToPointF(), offset);
                DoDrawArc(bitmapData, new Rectangle(p1.X, p1.Y, p2.X, p2.Y), startAngle, sweepAngle, c, arg);
            }

            [MethodImpl(MethodImpl.AggressiveInlining)]
            internal static void DrawArc(IBitmapDataInternal bitmapData, Rectangle bounds, float startAngle, float sweepAngle, TColor c, TArg arg = default!)
            {
                if (bounds.Width > ArcSegment.DrawAsLinesThreshold || bounds.Height > ArcSegment.DrawAsLinesThreshold)
                {
                    DrawLines(bitmapData, new ArcSegment(bounds, startAngle, sweepAngle).GetFlattenedPoints(), c, 0f);
                    return;
                }

                DoDrawArc(bitmapData, bounds, startAngle, sweepAngle, c, arg);
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
                return ParallelHelper.For(context, DrawingOperation.ProcessingPixels, rectangle.Top, rectangle.Bottom, y =>
                {
                    IBitmapDataRowInternal row = bitmapData.GetRowCached(y);
                    var accessor = new TAccessor();
                    accessor.InitRow(row);
                    TColor c = color;

                    int right = rectangle.Right;
                    for (int x = rectangle.Left; x < right; x++)
                        accessor.SetColor(x, c);
                });
            }

            #endregion

            #region Private Methods

            [MethodImpl(MethodImpl.AggressiveInlining)]
            private static (Point P1, Point P2) Round(PointF p1, PointF p2, float offset)
            {
                p1.X = MathF.Floor(p1.X.RoundTo(roundingUnit) + offset);
                p1.Y = MathF.Floor(p1.Y.RoundTo(roundingUnit) + offset);
                p2.X = MathF.Floor(p2.X.RoundTo(roundingUnit) + offset);
                p2.Y = MathF.Floor(p2.Y.RoundTo(roundingUnit) + offset);

                // For performance reasons there are no checks in the public BitmapDataExtensions.DrawXXX methods, but here we throw an OverflowException for extreme cases.
                return checked((new Point((int)p1.X, (int)p1.Y), new Point((int)p2.X, (int)p2.Y)));
            }

            private static void DoDrawEllipse(IBitmapDataInternal bitmapData, Rectangle bounds, TColor c, TArg arg = default!)
            {
                int top = bounds.Top;
                int left = bounds.Left;
                int right = bounds.RightChecked();
                int bottom = bounds.BottomChecked();
                Size size = bitmapData.Size;

                if (left > right)
                    (left, right) = (right, left);
                if (top > bottom)
                    (top, bottom) = (bottom, top);
                int width = right - left; // Not bounds.Width, because that can be negative. Exclusive: the actual drawn width is width + 1.
                int height = bottom - top; // Not bounds.Height, because that can be negative. Exclusive: the actual drawn height is height + 1

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
                if (bounds.Width > ArcSegment.DrawAsLinesThreshold || bounds.Height > ArcSegment.DrawAsLinesThreshold)
                {
                    DrawLines(bitmapData, new ArcSegment(bounds, startAngle, sweepAngle).GetFlattenedPoints(), c, 0f);
                    return;
                }

                ArcSegment.NormalizeAngles(ref startAngle, ref sweepAngle);
                if (sweepAngle >= 360f)
                {
                    DrawEllipse(bitmapData, bounds, c, arg);
                    return;
                }

                int top = bounds.Top;
                int left = bounds.Left;
                int right = bounds.RightChecked();
                int bottom = bounds.BottomChecked();
                Size size = bitmapData.Size;

                if (left > right)
                    (left, right) = (right, left);
                if (top > bottom)
                    (top, bottom) = (bottom, top);

                if (left >= size.Width || top >= size.Height || right < 0 || bottom < 0)
                    return;

                float centerX = (left + right + 1) / 2f;
                float radiusX = (right - left + 1) / 2f;
                float radiusY = (bottom - top + 1) / 2f;
                float startRad = startAngle.ToRadian();
                float endRad = (startAngle + sweepAngle).ToRadian();
                ArcSegment.AdjustAngles(ref startRad, ref endRad, radiusX, radiusY);

                // To prevent calculating Atan2 for each pixel, we just calculate a valid start/end range once, and apply it based on the current sector attributes.
                DoDrawArc(bitmapData, left, top, right, bottom, c, ArcSegment.GetSectors(startAngle, sweepAngle),
                    (int)(centerX + radiusX * MathF.Cos(startRad)), (int)(centerX + radiusX * MathF.Cos(endRad)), arg);
            }

            // Based on the combination of http://members.chello.at/~easyfilter/bresenham.c and https://www.scattergood.io/arc-drawing-algorithm/
            private static void DoDrawArc(IBitmapDataInternal bitmapData, int left, int top, int right, int bottom,
                TColor c, BitVector32 sectors, int startX, int endX, TArg arg)
            {
                int width = right - left; // Exclusive: the actual drawn width is width + 1.
                int height = bottom - top; // Exclusive: the actual drawn height is height + 1
                Debug.Assert(width <= ArcSegment.DrawAsLinesThreshold && height <= ArcSegment.DrawAsLinesThreshold);
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
                        || sector > 1 // positive sector point
                        && (sectorType == ArcSegment.SectorStart && x >= startX
                            || sectorType == ArcSegment.SectorEnd && x <= endX
                            || sectorType == ArcSegment.SectorStartEnd && x >= startX && x <= endX)
                        || sector <= 1 // negative sector point
                        && (sectorType == ArcSegment.SectorStart && x <= startX
                            || sectorType == ArcSegment.SectorEnd && x >= endX
                            || sectorType == ArcSegment.SectorStartEnd && x <= startX && x >= endX))
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

        internal static void DrawBeziers(IReadWriteBitmapData bitmapData, List<PointF> points, Color32 color, float offset)
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
