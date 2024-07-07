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
using System.Numerics;

using KGySoft.CoreLibraries;

#endregion

namespace KGySoft.Drawing.Shapes
{
    internal sealed class RawFigure
    {
        #region Constants

        private const float toleranceEquality = 0.001f;
        private const float tolerancePointDistance = 0.1f;

        #endregion

        #region Properties

        internal PointF[] Vertices { get; }
        internal Rectangle Bounds { get; }

        #endregion

        #region Constructors

        internal RawFigure(IList<PointF> points, bool isClosed)
        {
            Debug.Assert(points.Count > 0);

            // removing points too close to each other and the ones lying on the same line
            var result = new List<PointF>();
            var orientations = new List<sbyte>();
            var lastPoint = points[0];

            int count = points.Count;
            if (!isClosed)
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
            if (isClosed)
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

            // Adding the first point after last, even if after the elimination above it is very close to the last one.
            result.Add(result[0]);

            Vertices = result.ToArray();
            Bounds = Rectangle.FromLTRB((int)minX.TolerantFloor(toleranceEquality), (int)minY.TolerantFloor(toleranceEquality),
                (int)maxX.TolerantCeiling(toleranceEquality), (int)maxY.TolerantCeiling(toleranceEquality));
        }

        #endregion

        #region Methods

        #region Static Methods

        private static sbyte GetOrientation(PointF p1, PointF p2, PointF p3)
        {
            // https://www.tutorialspoint.com/how-to-check-orientation-of-3-ordered-points-in-java
            Vector2 slope1 = p2.ToVector2() - p1.ToVector2();
            Vector2 slope2 = p3.ToVector2() - p2.ToVector2();
            float result = (slope1.Y * slope2.X) - (slope1.X * slope2.Y);
            return (sbyte)(result.TolerantIsZero(toleranceEquality) ? 0
                : result > 0f ? 1
                : -1);
        }

        private static sbyte GetOrientation(PointF[] points)
        {
            float sum = 0f;
            for (int i = 0; i < points.Length - 1; i++)
            {
                PointF curr = points[i];
                PointF next = points[i + 1];
                sum += curr.X * next.Y - next.X * curr.Y;
            }

            return sum switch
            {
                > 0f => 1,
                < 0f => -1,
                _ => 0,
            };
        }

        #endregion

        #region Instance Methods

        internal void EnsurePositiveOrientation()
        {
            if (GetOrientation(Vertices) < 0)
                Array.Reverse(Vertices);
        }

        #endregion

        #endregion
    }
}