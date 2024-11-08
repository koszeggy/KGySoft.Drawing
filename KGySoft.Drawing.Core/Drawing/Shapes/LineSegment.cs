#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: LineSegment.cs
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

using System.Collections.Generic;
using System.Drawing;

#endregion

namespace KGySoft.Drawing.Shapes
{
    internal sealed class LineSegment : PathSegment
    {
        #region Fields

        private readonly List<PointF> points;

        #endregion

        #region Properties

        internal override PointF StartPoint => points[0];
        internal override PointF EndPoint => points[points.Count - 1];

        #endregion

        #region Constructors

        internal LineSegment(List<PointF> points)
        {
            // This overload does not copy the elements. Make sure it's called internally only.
            Debug.Assert(points != null! && points.Count > 0, "At least 1 point is expected");
            this.points = points!;
        }

        internal LineSegment(ICollection<PointF> points)
            : this(new List<PointF>(points))
        {
            // This overload exists for copying the elements.
        }

        #endregion

        #region Methods

        internal void Append(ICollection<PointF> newPoints) => points.AddRange(newPoints);

        internal override IList<PointF> GetFlattenedPoints() => points;

        internal override PathSegment Transform(TransformationMatrix matrix)
        {
            Debug.Assert(!matrix.IsIdentity);
            int len = points.Count;
            for (int i = 0; i < len; i++)
                points[i] = points[i].Transform(matrix);

            return this;
        }

        internal override PathSegment Clone() => new LineSegment(new List<PointF>(points));

        #endregion
    }
}
