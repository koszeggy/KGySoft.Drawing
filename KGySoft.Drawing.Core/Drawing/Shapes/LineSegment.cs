#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: LineSegment.cs
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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;

#endregion

namespace KGySoft.Drawing.Shapes
{
    /// <summary>
    /// Represents a path segment in a <see cref="Figure"/>, defined by a sequence of points connected by straight lines.
    /// </summary>
    /// <remarks>
    /// <note>This class is meant to provide information about a line segment in a <see cref="Figure"/> for interoperability with other libraries.
    /// To add new figures or path segments to a <see cref="Path"/>, use its public <see cref="Path.StartFigure">StartFigure</see> and <c>Add...</c> methods instead.</note>
    /// </remarks>
    public sealed class LineSegment : PathSegment
    {
        #region Fields

        private readonly IList<PointF> points;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the start point of this <see cref="LineSegment"/>.
        /// </summary>
        public override PointF StartPoint => points[0];

        /// <summary>
        /// Gets the end point of this <see cref="LineSegment"/>.
        /// </summary>
        public override PointF EndPoint => points[points.Count - 1];

        /// <summary>
        /// Gets a read-only collection of the points that define this <see cref="LineSegment"/>.
        /// </summary>
        public ReadOnlyCollection<PointF> Points => new(points);

        #endregion

        #region Constructors

        internal LineSegment(IList<PointF> points, bool copy)
        {
            Debug.Assert(points.Count > 0, "At least 1 point is expected");
            this.points = copy ? new List<PointF>(points) : points;
        }

        #endregion

        #region Methods

        internal void Append(IEnumerable<PointF> newPoints)
        {
            Debug.Assert(points is List<PointF>, "Append is expected to be called only when the segment was created with a list or copy = true");
            ((List<PointF>)points).AddRange(newPoints);
        }

        internal override IList<PointF> GetFlattenedPointsInternal() => points;

        internal override PathSegment Transform(TransformationMatrix matrix)
        {
            Debug.Assert(!matrix.IsIdentity);
            int len = points.Count;
            for (int i = 0; i < len; i++)
                points[i] = matrix.Transform(points[i]);

            return this;
        }

        internal override PathSegment Clone() => new LineSegment(points, true);

        #endregion
    }
}
