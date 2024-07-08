#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: Path.NestedTypes.cs
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

using KGySoft.Reflection;

#endregion

namespace KGySoft.Drawing.Shapes
{
    partial class Path
    {
        #region Nested classes

        #region Figure class

        private sealed class Figure
        {
            #region Fields

            private readonly List<PathSegment> segments;

            #endregion

            #region Properties

            internal bool IsClosed { get; set; }
            internal bool IsEmpty => segments.Count == 0;

            #endregion

            #region Constructors

            internal Figure() => segments = new List<PathSegment>();

            internal Figure(Figure other, bool close)
            {
                segments = new List<PathSegment>(other.segments);
                IsClosed = close || other.IsClosed;
            }

            #endregion

            #region Methods

            internal void AddSegment(PathSegment segment)
            {
                segments.Add(segment);
            }

            internal IList<PointF> GetPoints()
            {
                switch (segments.Count)
                {
                    case 0:
                        return Reflector.EmptyArray<PointF>();
                    case 1:
                        return segments[0].GetPoints();
                    default:
                        var result = new List<PointF>();
                        foreach (PathSegment segment in segments)
                            result.AddRange(segment.GetPoints());
                        return result;
                }
            }

            #endregion
        }

        #endregion

        #region PathSegment class

        private abstract class PathSegment
        {
            #region Methods
            
            internal abstract IList<PointF> GetPoints();

            #endregion
        }

        #endregion

        #region LineSegment class

        private sealed class LineSegment : PathSegment
        {
            #region Fields

            private readonly PointF[] points;

            #endregion

            #region Constructors

            internal LineSegment(params PointF[] points)
            {
                Debug.Assert(points != null! && points.Length >= 2, "points.Length should be >= 2");
                this.points = points!;
            }

            #endregion

            #region Methods

            internal override IList<PointF> GetPoints() => points;

            #endregion
        }

        #endregion

        #endregion
    }
}