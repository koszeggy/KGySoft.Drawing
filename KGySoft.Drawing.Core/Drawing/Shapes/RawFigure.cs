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

using KGySoft.Collections.ObjectModel;
#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
using System.Numerics;
#endif

using KGySoft.CoreLibraries;

#endregion

namespace KGySoft.Drawing.Shapes
{
    internal sealed class RawFigure
    {
        #region Nested Classes

        private sealed class OpenVerticesCollection(IList<PointF> closedVertices) : VirtualCollection<PointF>(closedVertices)
        {
            #region Properties

            public override int Count => Items.Count - 1;

            #endregion

            #region Methods

            public override IEnumerator<PointF> GetEnumerator() => throw new InvalidOperationException(Res.InternalError("Should not be called. Use CopyTo or ToArray instead."));

            #endregion
        }

        #endregion

        #region Constants

        private const float equalityTolerance = 1f / 64f;

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
        internal int VertexCount => ClosedVertices.Count - 1;

        #endregion

        #region Constructors

        internal RawFigure(IList<PointF> points, bool isClosed)
        {
            Debug.Assert(points.Count > 0);

            // TODO: delete
            //// overriding isClosed if actually closed
            //if (!isClosed && points.Count > 2 && points[0].TolerantEquals(points[points.Count - 1], equalityTolerance))
            //    isClosed = true;

            IsClosed = isClosed;

            // removing points too close to each other and the ones lying on the same line
            var result = new List<PointF>();
            var orientations = new List<sbyte>();
            var lastPoint = points[0];

            int count = points.Count;
            //if (!optimize) // TODO
            //{
            //    result.Add(points[0]);
            //    orientations.Add(0);
            //}
            //else
            {
                int prev = count;
                do
                {
                    prev -= 1;
                    if (prev == 0)
                    {
                        // all points are practically the same
                        result.Add(points[0]);
                        openVertices = ClosedVertices = result;
                        return;
                    }
                } while (points[0].TolerantEquals(points[prev], equalityTolerance));

                count = prev + 1;
                lastPoint = points[prev];

                result.Add(points[0]);
                orientations.Add(GetOrientation(lastPoint, points[0], points[1]));
                lastPoint = points[0];
            }

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
            //if (optimize) // TODO
            {
                count = result.Count;
                while (count > 2 && orientations[count - 1] == 0)
                    count -= 1;
                if (count < result.Count)
                    result.RemoveRange(count, result.Count - count);
            }

            float minX = Single.MaxValue;
            float minY = Single.MaxValue;
            float maxX = Single.MinValue;
            float maxY = Single.MinValue;
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

            // Auto closing if not already closed
            if (!result[0].TolerantEquals(result[result.Count - 1], equalityTolerance))
                result.Add(result[0]);
            else
                openVertices = result;

            if (!isClosed && points[0].TolerantEquals(points[points.Count - 1], equalityTolerance))
                openVertices ??= result;

            ClosedVertices = result;
            Bounds = Rectangle.FromLTRB((int)minX.TolerantFloor(equalityTolerance), (int)minY.TolerantFloor(equalityTolerance),
                (int)maxX.TolerantCeiling(equalityTolerance), (int)maxY.TolerantCeiling(equalityTolerance));
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
            return (sbyte)(result.TolerantIsZero(equalityTolerance) ? 0
                : result > 0f ? 1
                : -1);
        }

        #endregion
    }
}