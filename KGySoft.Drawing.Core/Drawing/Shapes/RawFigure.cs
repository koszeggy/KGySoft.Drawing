#region Copyright

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
using System.Runtime.CompilerServices;
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

        private sealed class VerticesCollection : VirtualCollection<PointF>
        {
            #region Properties

            public override int Count { get; }

            #endregion

            #region Constructors

            internal VerticesCollection(PointF[] points, int count)
                : base(points)
            {
                Count = count;
            }

            #endregion

            #region Methods

            public override IEnumerator<PointF> GetEnumerator() => throw new InvalidOperationException(Res.InternalError("Should not be called. Use CopyTo or ToArray instead."));

            #endregion
        }

        #endregion

        #region Fields

        // an actual List would be more convenient, but array is better for vector operations
        private readonly PointF[] closedVerticesBuffer;
        private readonly int closedVerticesCount;

        private IList<PointF>? closedVertices;
        private IList<PointF>? openVertices;

        #endregion

        #region Properties

        internal bool IsClosed { get; }

        internal IList<PointF> ClosedVertices => closedVertices ??= closedVerticesBuffer.Length == closedVerticesCount
            ? closedVerticesBuffer
            : new VerticesCollection(closedVerticesBuffer, closedVerticesCount);

        internal IList<PointF> OpenVertices => openVertices ??= closedVerticesCount < 3
            ? ClosedVertices
            : new VerticesCollection(closedVerticesBuffer, closedVerticesCount - 1);

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
            #region Local Methods

            [MethodImpl(MethodImpl.AggressiveInlining)]
            static void AddPoint(PointF[] buffer, PointF value, ref int count)
            {
                Debug.Assert(count < buffer.Length);
                buffer[count] = value;
                count += 1;
            }

            #endregion

            Debug.Assert(points.Count > 0);
            Debug.Assert(!offset || isClosed, "offset == true is expected for closed figures only");

            // removing points too close to each other and the ones lying on the same line
            int maxPoints = points.Count + (points.Count > 2 && !points[0].TolerantEquals(points[points.Count - 1], Constants.PointEqualityTolerance) ? 1 : 0);
            var result = closedVerticesBuffer = new PointF[maxPoints];
            int resultCount = 0;
#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
            var min = new Vector2(Single.MaxValue);
            var max = new Vector2(Single.MinValue);
#else
            float minX = Single.MaxValue;
            float minY = Single.MaxValue;
            float maxX = Single.MinValue;
            float maxY = Single.MinValue;
#endif

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
                        AddPoint(result, points[0], ref resultCount);
#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
                        PointF p = points[0];
                        min = max = p.AsVector2();
#else
                        minX = maxX = points[0].X;
                        minY = maxY = points[0].Y;
#endif
                        isClosed = false;
                        VertexCount = closedVerticesCount = 1;
                        return;
                    }
                } while (points[0].TolerantEquals(points[prev], Constants.PointEqualityTolerance));

                count = prev + 1;
                PointF lastPoint = points[0];
                AddPoint(result, lastPoint, ref resultCount);

                for (int i = 1; i < count; i++)
                {
                    // Skipping points that are too close to the previous one
                    if (points[i].TolerantEquals(lastPoint, Constants.PointEqualityTolerance))
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

                    AddPoint(result, lastPoint = points[i], ref resultCount);
                }

                VertexCount = resultCount;
                for (int i = 0; i < resultCount; i++)
                {
#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
                    // Unlike in ColorF, we don't mind possibly inconsistent NaN handling here
                    Vector2 vertex = result[i].AsVector2();
                    min = Vector2.Min(min, vertex);
                    max = Vector2.Max(max, vertex);
#else
                    PointF vertex = result[i];
                    minX = Math.Min(minX, vertex.X);
                    maxX = Math.Max(maxX, vertex.X);
                    minY = Math.Min(minY, vertex.Y);
                    maxY = Math.Max(maxY, vertex.Y);
#endif
                }

                // Forcing open shape below 3 points
                if (resultCount < 3)
                    isClosed = false;
                else
                {
                    // Auto closing (points only, not the IsClosed flag) if not already closed and has at least 3 points.
                    if (!result[0].TolerantEquals(result[resultCount - 1], Constants.PointEqualityTolerance))
                        AddPoint(result, result[0], ref resultCount);
                    else
                        openVertices = CreateVerticesCollection(result, resultCount);

                    // If original points are practically closed but the figure is officially open, then
                    // treating the closing point as the part of the open figure. It makes a difference when drawing thick lines.
                    if (!isClosed && points[0].TolerantEquals(points[points.Count - 1], Constants.PointEqualityTolerance))
                        openVertices ??= CreateVerticesCollection(result, resultCount);
                }

                closedVerticesCount = resultCount;
            }
            finally
            {
                // Offsetting the input points could be simpler, but it may not be a copy in every case and may contain ignored points.
                if (offset)
                {
#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
                    min += new Vector2(0.5f);
                    max += new Vector2(0.5f);
#else
                    minX += 0.5f;
                    minY += 0.5f;
                    maxX += 0.5f;
                    maxY += 0.5f;
#endif
                    result.AsSection(0, resultCount).AddOffset(0.5f);
                }

                // For performance reasons there are no checks in the public Path.AddXXX methods (which actually allows
                // transformations into the valid range before drawing), but here we throw an OverflowException for extreme cases.
                checked
                {
#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
                    int left = (int)min.X.TolerantFloor(Constants.PointEqualityTolerance);
                    int top = (int)min.Y.TolerantFloor(Constants.PointEqualityTolerance);
                    int right = (int)max.X.TolerantCeiling(Constants.PointEqualityTolerance);
                    int bottom = (int)max.Y.TolerantCeiling(Constants.PointEqualityTolerance);
#else
                    int left = (int)minX.TolerantFloor(Constants.PointEqualityTolerance);
                    int top = (int)minY.TolerantFloor(Constants.PointEqualityTolerance);
                    int right = (int)maxX.TolerantCeiling(Constants.PointEqualityTolerance);
                    int bottom = (int)maxY.TolerantCeiling(Constants.PointEqualityTolerance);
#endif

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
#if NET10_0_OR_GREATER
            return Vector2.Cross(slope2, slope1) switch
#else
            return ((slope1.Y * slope2.X) - (slope1.X * slope2.Y)) switch
#endif
            {
                > 0f => 1,
                < 0f => -1,
                _ => 0, // not using tolerance here because we need the exact result to be able to filter collinear points correctly in the caller
            };
        }

        private static IList<PointF> CreateVerticesCollection(PointF[] points, int count) => points.Length == count
            ? points
            : new VerticesCollection(points, count);

        #endregion
    }
}