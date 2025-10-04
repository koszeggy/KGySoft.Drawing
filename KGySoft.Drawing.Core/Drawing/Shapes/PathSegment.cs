#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: PathSegment.cs
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
    /// Represents a path segment in a <see cref="Figure"/>. This is an abstract class that cannot be inherited outside the declaring assembly,
    /// so its actual type is always one of the derived types: <see cref="LineSegment"/>, <see cref="BezierSegment"/> or <see cref="ArcSegment"/>.
    /// </summary>
    public abstract class PathSegment
    {
        #region Properties

        /// <summary>
        /// Gets the start point of this <see cref="PathSegment"/>.
        /// </summary>
        public abstract PointF StartPoint { get; }

        /// <summary>
        /// Gets the end point of this <see cref="PathSegment"/>.
        /// </summary>
        public abstract PointF EndPoint { get; }

        #endregion

        #region Constructors

        internal PathSegment()
        {
        }

        #endregion

        #region Methods

        #region Public Methods

        /// <summary>
        /// Gets the flattened points that define this <see cref="PathSegment"/> as a polyline.
        /// </summary>
        /// <returns>The list of <see cref="PointF"/> structures that define the flattened points of this segment.</returns>
        public ReadOnlyCollection<PointF> GetFlattenedPoints() => new(GetFlattenedPointsInternal());

        #endregion

        #region Internal Methods

        internal abstract IList<PointF> GetFlattenedPointsInternal();
        internal abstract PathSegment Transform(TransformationMatrix matrix);
        internal abstract PathSegment Clone();

        #endregion
        
        #endregion
    }
}
