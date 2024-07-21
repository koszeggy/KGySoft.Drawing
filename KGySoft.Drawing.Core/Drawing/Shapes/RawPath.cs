#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: RawPath.cs
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

#endregion

namespace KGySoft.Drawing.Shapes
{
    /// <summary>
    /// The raw version of <see cref="Path"/> where everything is represented by simple points.
    /// </summary>
    internal sealed class RawPath
    {
        #region Fields

        private readonly List<RawFigure> figures;

        private Rectangle bounds;
        private int totalVertices;
        private int maxVertices;

        #endregion

        #region Properties

        internal Rectangle Bounds => bounds;
        internal int TotalVertices => totalVertices;
        internal int MaxVertices => maxVertices;
        internal List<RawFigure> Figures => figures;

        #endregion

        #region Constructors

        internal RawPath(int capacity) => figures = new List<RawFigure>(capacity);

        #endregion

        #region Methods

        internal void AddRawFigure(IList<PointF> points, bool optimize)
        {
            if (points.Count == 0)
                return;
            var figure = new RawFigure(points, optimize);
            bounds = figures.Count == 0 ? figure.Bounds : Rectangle.Union(bounds, figure.Bounds);
            figures.Add(figure);
            totalVertices += figure.Vertices.Length - 1;
            maxVertices = Math.Max(maxVertices, figure.Vertices.Length - 1);
        }

        #endregion
    }
}