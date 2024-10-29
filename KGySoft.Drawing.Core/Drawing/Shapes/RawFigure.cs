#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: RawFigure.cs
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

            try
            {
                var orientations = new List<sbyte>();
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
                PointF lastPoint = points[prev];

                result.Add(points[0]);
                orientations.Add(GetOrientation(lastPoint, points[0], points[1]));
                lastPoint = points[0];

                for (int i = 1; i < count; i++)
                {
                    int next = i + 1;
                    if (next >= count)
                        next -= count;
                    sbyte orientation = GetOrientation(lastPoint, points[i], points[next]);
                    if (orientation == 0 && next != 0)
                        continue;

                    result.Add(points[i]);
                    orientations.Add(orientation);
                    lastPoint = points[i];
                }

                // removing points lying on the same line from the end
                count = result.Count;
                while (count > 2 && orientations[count - 1] == 0)
                    count -= 1;
                if (count < result.Count)
                    result.RemoveRange(count, result.Count - count);

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

                Bounds = Rectangle.FromLTRB((int)minX.TolerantFloor(Constants.EqualityTolerance), (int)minY.TolerantFloor(Constants.EqualityTolerance),
                    (int)maxX.TolerantCeiling(Constants.EqualityTolerance), (int)maxY.TolerantCeiling(Constants.EqualityTolerance));
                IsClosed = isClosed;
            }
        }

        #endregion

        #region Methods

        private static sbyte GetOrientation(PointF p1, PointF p2, PointF p3)
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
            return (sbyte)(result.TolerantIsZero(Constants.EqualityTolerance) ? 0
                : result > 0f ? 1
                : -1);
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