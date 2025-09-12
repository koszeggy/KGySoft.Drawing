#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: Figure.cs
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
using System.Diagnostics.CodeAnalysis;
using System.Drawing;

using KGySoft.Reflection;

#endregion

namespace KGySoft.Drawing.Shapes
{
    /// <summary>
    /// Represents a geometric figure in a <see cref="Path"/>, composed of a sequence of path segments.
    /// </summary>
    /// <remarks>
    /// <note>This class is meant to provide information about a figure in a <see cref="Path"/> instance for interoperability with other libraries, and it cannot be used to modify the figure.
    /// To add new figures or path segments to a <see cref="Path"/>, use its public <see cref="Path.StartFigure">StartFigure</see> and <c>Add...</c> methods instead.</note>
    /// <para>To get the path segments in this figure, use the <see cref="Segments"/> property.</para>
    /// <para>To get the flattened points that define this figure, you can also use the <see cref="GetFlattenedPoints">GetFlattenedPoints</see> method.</para>
    /// </remarks>
    public sealed class Figure
    {
        #region Fields

        private bool isClosed;

        #endregion

        #region Properties

        #region Public Properties

        /// <summary>
        /// Gets whether this <see cref="Figure"/> is closed.
        /// </summary>
        public bool IsClosed => isClosed;

        /// <summary>
        /// Gets whether this <see cref="Figure"/> contains no segments.
        /// </summary>
        public bool IsEmpty => SegmentsInternal.Count == 0;

        /// <summary>
        /// Gets a read-only collection of the <see cref="PathSegment"/> objects this <see cref="Figure"/> consists of.
        /// </summary>
        /// <remarks>
        /// <para>Every segment has a start point and an end point. If the <see cref="PathSegment.StartPoint"/> of a <see cref="PathSegment"/> is not the same as the <see cref="PathSegment.EndPoint"/> of
        /// the previous segment, then a straight line is assumed between these two points.</para>
        /// <para>If the <see cref="IsClosed"/> property is true, then a straight line is also assumed between the <see cref="PathSegment.EndPoint"/> of the last segment and the <see cref="PathSegment.StartPoint"/>
        /// of the first segment, if they are not the same.</para>
        /// <para>You can also use the <see cref="GetFlattenedPoints">GetFlattenedPoints</see> method to get all the points as a polyline that define this figure, including those connecting segments.</para>
        /// </remarks>
        public ReadOnlyCollection<PathSegment> Segments => SegmentsInternal.AsReadOnly();

        #endregion

        #region Internal Properties

        internal List<PathSegment> SegmentsInternal { get; }

        #endregion

        #endregion

        #region Constructors

        internal Figure() => SegmentsInternal = new List<PathSegment>();

        internal Figure(Figure other, bool close)
        {
            int count = other.SegmentsInternal.Count;
            SegmentsInternal = new List<PathSegment>(count);
            for (int i = 0; i < count; i++)
                SegmentsInternal.Add(other.SegmentsInternal[i].Clone());

            isClosed = close || other.IsClosed;
        }

        #endregion

        #region Methods

        #region Public Methods

        /// <summary>
        /// Gets the flattened points that define this <see cref="Figure"/> as a polyline or a closed polygon.
        /// </summary>
        /// <remarks>
        /// <para>If the <see cref="IsClosed"/> property is true, and this figure contains at least 3 points,
        /// then the returned list contains the closing point as well, if it is not the same as the first point.</para>
        /// </remarks>
        /// <returns>A list of <see cref="PointF"/> structures that define the flattened points of this figure.</returns>
        public IList<PointF> GetFlattenedPoints() => GetPoints(true);

        #endregion

        #region Internal Methods

        internal void Close() => isClosed = true;

        internal void AddSegment(PathSegment segment) => SegmentsInternal.Add(segment);

        [SuppressMessage("ReSharper", "UseIndexFromEndExpression", Justification = "Targeting older frameworks that don't support indexing from end.")]
        internal bool TryAppendPoints(IEnumerable<PointF> points)
        {
            if (SegmentsInternal.Count == 0 || SegmentsInternal[SegmentsInternal.Count - 1] is not LineSegment lastSegment)
                return false;

            if (IsClosed)
            {
                if (!IsEmpty)
                    return false;
                isClosed = false;
            }

            lastSegment.Append(points);
            return true;
        }

        internal IList<PointF> GetPoints(bool ensureClosed)
        {
            switch (SegmentsInternal.Count)
            {
                case 0:
                    return Reflector.EmptyArray<PointF>();
                case 1:
                    var points = SegmentsInternal[0].GetFlattenedPointsInternal();
                    if (ensureClosed && IsClosed && points.Count > 2 && points[0] != points[points.Count - 1])
                        points = [.. points, points[0]];
                    return points;
                default:
                    var result = new List<PointF>();
                    foreach (PathSegment segment in SegmentsInternal)
                        result.AddRange(segment.GetFlattenedPointsInternal());
                    if (ensureClosed && IsClosed && result.Count > 2 && result[0] != result[result.Count - 1])
                        result.Add(result[0]);
                    return result;
            }
        }

        internal void Transform(TransformationMatrix matrix)
        {
            for (int i = 0; i < SegmentsInternal.Count; i++)
            {
                PathSegment segment = SegmentsInternal[i];
                PathSegment transformedSegment = segment.Transform(matrix);
                if (!ReferenceEquals(segment, transformedSegment))
                    SegmentsInternal[i] = transformedSegment;
            }
        }

        #endregion

        #endregion
    }
}
