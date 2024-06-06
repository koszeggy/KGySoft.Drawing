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

            private readonly List<PathSegment> segments = new List<PathSegment>();

            #endregion

            #region Methods

            internal void AddSegment(PathSegment segment)
            {
                segments.Add(segment);
            }

            #endregion
        }

        #endregion

        #region PathSegment class

        private abstract class PathSegment
        {
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
        }

        #endregion

        #endregion
    }
}