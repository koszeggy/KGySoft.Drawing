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
using System.Drawing;

#endregion

namespace KGySoft.Drawing.Shapes
{
    internal abstract class PathSegment
    {
        #region Properties

        internal abstract PointF StartPoint { get; }
        internal abstract PointF EndPoint { get; }

        #endregion

        #region Methods

        internal abstract IList<PointF> GetFlattenedPoints();
        internal abstract PathSegment Transform(TransformationMatrix matrix);
        internal abstract PathSegment Clone();

        #endregion
    }
}
