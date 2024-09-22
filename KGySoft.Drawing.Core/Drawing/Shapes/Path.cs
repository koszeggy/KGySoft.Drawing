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
using System.Linq;

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

        private List<Figure>? figures;
        private Figure currentFigure;
        private RawPath? rawPath;

        #endregion

        #region Properties

        #region Public Properties

        public bool IsEmpty => figures == null && currentFigure.IsEmpty;
        public Rectangle Bounds => RawPath.Bounds;

        #endregion

        #region Internal Properties

        internal RawPath RawPath => rawPath ??= InitRawPath();

        #endregion

        #endregion

        #region Constructors

        #region Public Constructors

        public Path()
        {
            currentFigure = new Figure();
        }

        public Path(Path other) : this(other, false)
        {
        }

        #endregion

        #region Private Constructors

        private Path(Path other, bool close)
        {
            if (other.figures == null)
            {
                currentFigure = new Figure(other.currentFigure, close);
                return;
            }

            figures = new List<Figure>(other.figures.Select(f => new Figure(f, close)));
            currentFigure = figures[figures.Count - 1];
        }

        #endregion

        #endregion

        #region Methods

        #region Static Methods

        public static Path Transform(Path path, TransformationMatrix matrix)
        {
            var result = new Path(path);
            result.Transform(matrix);
            return result;
        }

        #endregion

        #region Instance Methods

        #region Public Methods

        public Path AddLine(PointF p1, PointF p2)
        {
            AddSegment(new LineSegment(p1, p2));
            return this;
        }

        public Path AddLines(params PointF[] points)
        {
            AddSegment(new LineSegment(points));
            return this;
        }

        // TODO: AddRectangle
        // TODO: AddArc
        // TODO: AddBezier (required for ellipse)
        // TODO: AddEllipse
        // TODO: AddPolygon (same as AddLines but closed)
        // TODO: AddRoundedRectangle

        public Path CloseFigure()
        {
            // not closing if empty because it would be skipped anyway
            if (currentFigure.IsClosed || currentFigure.IsEmpty)
                return this;

            Invalidate();
            currentFigure.IsClosed = true;
            return this;
        }

        public Path StartFigure()
        {
            if (currentFigure.IsEmpty)
            {
                currentFigure.IsClosed = false;
                return this;
            }

            figures ??= new List<Figure>(2) { currentFigure };
            figures.Add(currentFigure = new Figure());
            return this;
        }

        public Path AsClosed()
        {
            if (figures == null)
            {
                if (currentFigure.IsClosed)
                    return this;
            }
            else if (figures.TrueForAll(f => f.IsClosed))
                return this;

            return new Path(this, true);
        }

        // NOTE: not returning this to avoid confusion. Use the static Transform to create a new instance, leaving this unchanged.
        public void Transform(TransformationMatrix matrix)
        {
            if (IsEmpty || matrix.IsIdentity)
                return;

            Invalidate();

            if (figures == null)
                currentFigure.Transform(matrix);
            else
            {
                foreach (Figure figure in figures)
                    figure.Transform(matrix);
            }
        }

        #endregion

        #region Private Methods

        private void AddSegment(PathSegment segment)
        {
            Invalidate();
            if (currentFigure.IsClosed)
                StartFigure();
            currentFigure.AddSegment(segment);
        }

        private void Invalidate() => rawPath = null;

        private RawPath InitRawPath()
        {
            var result = new RawPath(figures?.Count ?? 1);
            if (figures == null)
                result.AddRawFigure(currentFigure.GetPoints(), true);
            else
            {
                foreach (Figure figure in figures)
                    result.AddRawFigure(figure.GetPoints(), true);
            }

            return result;
        }

        #endregion

        // TODO
        //#region Explicitly Implemented Interface Methods

        //void IDisposable.Dispose() => Invalidate();

        //#endregion

        #endregion

        #endregion
    }
}