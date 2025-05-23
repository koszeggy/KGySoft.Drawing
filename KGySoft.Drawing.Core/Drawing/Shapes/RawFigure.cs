﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: RawFigure.cs
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
using System.Drawing;
#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
using System.Numerics;
#endif

using KGySoft.Collections.ObjectModel;
using KGySoft.CoreLibraries;

#endregion

namespace KGySoft.Drawing.Shapes
{
    internal sealed class RawFigure
    {
        #region Nested Classes

        private sealed class OpenVerticesCollection : VirtualCollection<PointF>
        {
            #region Properties

            public override int Count { get; }

            #endregion

            #region Constructors

            internal OpenVerticesCollection(List<PointF> closedVertices)
                : base(closedVertices)
            {
                Count = closedVertices.Count - 1;
            }

            #endregion

            #region Methods

            public override IEnumerator<PointF> GetEnumerator() => throw new InvalidOperationException(Res.InternalError("Should not be called. Use CopyTo or ToArray instead."));

            #endregion
        }

        #endregion

        #region Fields

        private IList<PointF>? openVertices;

        #endregion

        #region Properties

        internal bool IsClosed { get; }
        internal List<PointF> ClosedVertices { get; }
        internal IList<PointF> OpenVertices => openVertices ??= new OpenVerticesCollection(ClosedVertices); 
        internal Rectangle Bounds { get; }

        /// <summary>
        /// Gets the effective vertex count omitting the possibly matching start/end points.
        /// This should be used when filling the path, which is consistent no matter whether the figure is open or closed.
        /// NOTE: This can be different from OpenVertices.Count if an open figure has the same start/end point.
        /// </summary>
        internal int VertexCount { get; }

        #endregion

        #region Constructors

        internal RawFigure(IList<PointF> points, bool isClosed, bool offset)
        {
            Debug.Assert(points.Count > 0);

            // removing points too close to each other and the ones lying on the same line
            var result = new List<PointF>();
            float minX = Single.MaxValue;
            float minY = Single.MaxValue;
            float maxX = Single.MinValue;
            float maxY = Single.MinValue;

            // the try-finally block is just because of the return
            try
            {
                int count = points.Count;
                int prev = count;

                // skipping points from the end that are same as the first point
                do
                {
                    prev -= 1;
                    if (prev == 0)
                    {
                        // All points are practically the same.
                        result.Add(points[0]);
                        openVertices = ClosedVertices = result;
                        minX = maxX = points[0].X;
                        minY = maxY = points[0].Y;
                        isClosed = false;
                        VertexCount = 1;
                        return;
                    }
                } while (points[0].TolerantEquals(points[prev], Constants.EqualityTolerance));

                count = prev + 1;
                PointF lastPoint = points[0];
                result.Add(lastPoint);

                for (int i = 1; i < count; i++)
                {
                    // Skipping points that are too close to the previous one
                    if (points[i].TolerantEquals(lastPoint, Constants.EqualityTolerance))
                        continue;

                    int next = i + 1;
                    if (next >= count)
                        next -= count;
                    int orientation = GetOrientation(lastPoint, points[i], points[next]);

                    if (orientation == 0 && next != 0
                        // Skipping point only if the orientation is 0 also in a shifted order.
                        // This prevents skipping false alarms at coordinates where float precision is less accurate (e.g. very long and thin widened lines).
                        && GetOrientation(points[i], points[next], lastPoint) == 0
                        // And if points[i] is between lastPoint and points[next]. This prevents skipping the side points if the direction changes (e.g. a totally flat ellipse).
                        && points[i].X >= Math.Min(lastPoint.X, points[next].X) && points[i].X <= Math.Max(lastPoint.X, points[next].X)
                        && points[i].Y >= Math.Min(lastPoint.Y, points[next].Y) && points[i].Y <= Math.Max(lastPoint.Y, points[next].Y))
                    {
                        continue;
                    }

                    result.Add(points[i]);
                    lastPoint = points[i];
                }

                VertexCount = result.Count;
                foreach (PointF vertex in result)
                {
                    if (vertex.X < minX)
                        minX = vertex.X;
                    if (vertex.X > maxX)
                        maxX = vertex.X;
                    if (vertex.Y < minY)
                        minY = vertex.Y;
                    if (vertex.Y > maxY)
                        maxY = vertex.Y;
                }

                // Forcing open shape below 3 points
                if (result.Count < 3)
                {
                    isClosed = false;
                    openVertices = ClosedVertices = result;
                }
                else
                {
                    // Auto closing (points only, not the IsClosed flag) if not already closed and has at least 3 points.
                    if (!result[0].TolerantEquals(result[result.Count - 1], Constants.EqualityTolerance))
                        result.Add(result[0]);
                    else
                        openVertices = result;

                    // If original points are practically closed but the figure is officially open, then
                    // treating the closing point as the part of the open figure. It makes a difference when drawing thick lines.
                    if (!isClosed && points[0].TolerantEquals(points[points.Count - 1], Constants.EqualityTolerance))
                        openVertices ??= result;

                    ClosedVertices = result;
                }
            }
            finally
            {
                // Offsetting the input points could be simpler, but it may not be a copy in every case and may contain ignored points.
                if (offset)
                {
                    minX += 0.5f;
                    minY += 0.5f;
                    maxX += 0.5f;
                    maxY += 0.5f;
                    DoOffset(result);
                }

                // For performance reasons there are no checks in the public Path.AddXXX methods (which actually allows
                // transformations into the valid range before drawing), but here we throw an OverflowException for extreme cases.
                checked
                {
                    int left = (int)minX.TolerantFloor(Constants.EqualityTolerance);
                    int top = (int)minY.TolerantFloor(Constants.EqualityTolerance);
                    int right = (int)maxX.TolerantCeiling(Constants.EqualityTolerance);
                    int bottom = (int)maxY.TolerantCeiling(Constants.EqualityTolerance);

                    // Not using Rectangle.FromLTRB because it allows overflow.
                    Bounds = new Rectangle(left, top, right - left, bottom - top);
                }

                IsClosed = isClosed;
            }
        }

        #endregion

        #region Methods

        private static int GetOrientation(PointF p1, PointF p2, PointF p3)
        {
            // https://www.tutorialspoint.com/how-to-check-orientation-of-3-ordered-points-in-java
#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
            Vector2 slope1 = p2.AsVector2() - p1.AsVector2();
            Vector2 slope2 = p3.AsVector2() - p2.AsVector2();
#else
            PointF slope1 = p2 - new SizeF(p1);
            PointF slope2 = p3 - new SizeF(p2);
#endif
            float result = (slope1.Y * slope2.X) - (slope1.X * slope2.Y);
            return result switch
            {
                > 0f => 1,
                < 0f => -1,
                _ => 0, // not using tolerance here because we need the exact result to be able to filter collinear points correctly in the caller
            };
        }

        private static void DoOffset(IList<PointF> points)
        {
            // TODO: vectorization (test cast to Vector4/Vector<float>/explicit SIMD Vector128/256) options.
            // .NET5+: CollectionsMarshal.AsSpan(ClosedVertices); otherwise, Accessor.GetArraySection(ClosedVertices).Cast<Vector4>, and Vector2 to the possible last element.

            // TODO: is it faster to get the internal array first, and then set element's X/Y in place?
            int len = points.Count;
            SizeF offset = new SizeF(0.5f, 0.5f);
            for (int i = 0; i < len; i++)
                points[i] += offset;
        }

        #endregion
    }
}