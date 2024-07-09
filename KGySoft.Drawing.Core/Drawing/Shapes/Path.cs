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

        internal RawPath RawPath => rawPath ??= InitRawPath();

        #endregion

        #region Constructors

        #region Public Constructors
        
        public Path()
        {
            currentFigure = new Figure();
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
            currentFigure = figures[^1];
        }

        #endregion

        #endregion

        #region Methods

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

        #endregion

        #region Internal Methods

        internal IReadableBitmapData? GetRegion(IAsyncContext context, Pen pen, DrawingOptions drawingOptions)
        {
            // TODO: try get from a small locking cache that can dispose the dropped items
            // to make it well scalable use a non-locking cache for the last item (non-volatile so different cores have a bigger chance to see their own instance)
            throw new NotImplementedException();
        }

        internal IReadableBitmapData? GetRegion(IAsyncContext context, Brush pen, DrawingOptions drawingOptions)
        {
            // TODO: trivial shortcut for a simple filled rectangle (SolidBitmapData) - or just assert if that should be handled earlier


            // TODO: try get from a small locking cache that can dispose the dropped items
            // to make it well scalable use a non-locking cache for the last item (non-volatile so different cores have a bigger chance to see their own instance)
            throw new NotImplementedException();
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

            result.EnsureSingleFigurePositiveOrientation();
            return result;
        }

        #endregion

        // TODO
        //#region Explicitly Implemented Interface Methods

        //void IDisposable.Dispose() => Invalidate();

        //#endregion

        #endregion
    }
}