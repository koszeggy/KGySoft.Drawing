#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: Path.cs
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

using KGySoft.Drawing.Imaging;
using KGySoft.Threading;

#endregion

namespace KGySoft.Drawing.Shapes
{
    /// <summary>
    /// Represents the path of a custom shape to be drawn or filled. The path can consist of multiple open or closed figures.
    /// </summary>
    internal sealed partial class Path // TODO: IDisposable, make it public when all general shapes are available
    {
        #region Fields

        private readonly List<Figure>? figures;

        private Figure currentFigure;

        #endregion

        #region Constructors

        public Path()
        {
            currentFigure = new Figure();
        }

        #endregion

        #region Methods

        #region Public Methods

        public Path AddLine(PointF p1, PointF p2)
        {
            AddSegment(new LineSegment(p1, p2));
            return this;
        }

        #endregion

        #region Internal Methods

        internal IReadableBitmapData? GetRegion(IAsyncContext context, Pen pen, DrawingOptions drawingOptions)
        {
            // TODO: try get from a small locking cache that can dispose the dropped items
            // to make it well scalable use a non-locking cache for the last item (non-volatile so different cores have a bigger chance to see their own instance)
            throw new NotImplementedException();
        }

        #endregion

        #region Private Methods

        private void AddSegment(PathSegment segment)
        {
            currentFigure.AddSegment(segment);
        }

        #endregion

        #endregion
    }
}