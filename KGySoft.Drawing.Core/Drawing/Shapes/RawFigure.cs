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

using KGySoft.CoreLibraries;

#endregion

namespace KGySoft.Drawing.Shapes
{
    internal sealed class RawFigure
    {
        #region Constants

        private const float toleranceEquality = 1f / 64f;
        private const float tolerancePointDistance = 1f / 32f;

        #endregion

        #region Properties

        internal PointF[] Vertices { get; }
        internal Rectangle Bounds { get; }

        #endregion

        #region Constructors

        internal RawFigure(IList<PointF> points, bool optimize)
        {
            Debug.Assert(points.Count > 0);

            // removing points too close to each other and the ones lying on the same line
            var result = new List<PointF>();
            var orientations = new List<sbyte>();
            var lastPoint = points[0];

            int count = points.Count;
            if (!optimize)
            {
                result.Add(points[0]);
                orientations.Add(0);
            }
            else
            {
                int prev = count;
                do
                {
                    prev -= 1;
                    if (prev == 0)
                    {
                        // all points are practically the same
                        result.Add(points[0]);
                        Vertices = result.ToArray();
                        return;
                    }
                } while (points[0].TolerantEquals(points[prev], tolerancePointDistance));

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
            if (optimize)
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
            if (!result[0].TolerantEquals(result[result.Count - 1], toleranceEquality))
                result.Add(result[0]);

            Vertices = result.ToArray();
            Bounds = Rectangle.FromLTRB((int)minX.TolerantFloor(toleranceEquality), (int)minY.TolerantFloor(toleranceEquality),
                (int)maxX.TolerantCeiling(toleranceEquality), (int)maxY.TolerantCeiling(toleranceEquality));
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
            return (sbyte)(result.TolerantIsZero(toleranceEquality) ? 0
                : result > 0f ? 1
                : -1);
        }

        #endregion
    }
}