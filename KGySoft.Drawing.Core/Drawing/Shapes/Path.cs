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
        private TransformationMatrix transformation;

        #endregion

        #region Properties

        #region Public Properties

        public bool IsEmpty => figures == null && currentFigure.IsEmpty;
        public Rectangle Bounds => RawPath.Bounds;
        public TransformationMatrix Transformation => transformation;

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
            transformation = TransformationMatrix.Identity;
        }

        public Path(Path other) : this(other, false)
        {
        }

        #endregion

        #region Private Constructors

        private Path(Path other, bool close)
        {
            transformation = other.transformation;
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
            result.TransformAdded(matrix);
            return result;
        }

        #endregion

        #region Instance Methods

        #region Public Methods

        // TODO: Point, int, float overloads
        public Path AddLine(PointF p1, PointF p2)
        {
            AddSegment(new LineSegment(p1, p2));
            return this;
        }

        // TODO: Point[] overload
        public Path AddLines(params PointF[] points)
        {
            if (points == null)
                throw new ArgumentNullException(nameof(points), PublicResources.ArgumentNull);
            if (points.Length < 2)
                throw new ArgumentException(nameof(points), Res.ShapesLinePointsInvalid);

            AddSegment(new LineSegment((PointF[])points.Clone()));
            return this;
        }

        // TODO: Rectangle, int, float overloads
        public Path AddRectangle(RectangleF rectangle)
        {
            StartFigure();
            AddSegment(new LineSegment(rectangle.Location,
                new PointF(rectangle.Right, rectangle.Top),
                new PointF(rectangle.Right, rectangle.Bottom),
                new PointF(rectangle.Left, rectangle.Bottom)));
            CloseFigure();
            return this;
        }

        // TODO: Point, float, int overloads
        public Path AddBezier(PointF p1, PointF p2, PointF p3, PointF p4)
        {
            AddSegment(new BezierSegment(new[] { p1, p2, p3, p4 }));
            return this;
        }

        public Path AddBeziers(params PointF[] points)
        {
            if (points == null)
                throw new ArgumentNullException(nameof(points), PublicResources.ArgumentNull);
            if ((points.Length - 1) % 3 != 0)
                throw new ArgumentException(nameof(points), Res.ShapesBezierPointsInvalid);
            AddSegment(new BezierSegment((PointF[])points.Clone()));
            return this;
        }

        // TODO: Rectangle, int, float overloads
        public Path AddArc(RectangleF bounds, float startAngle, float sweepAngle)
        {
            // TODO: validation (bounds width/height, etc)
            AddSegment(BezierSegment.FromArc(bounds, startAngle, sweepAngle));
            return this;
        }

        // TODO: Rectangle, int, float overloads
        public Path AddEllipse(RectangleF bounds)
        {
            // TODO: validation (bounds width/height, etc)
            StartFigure();
            AddSegment(BezierSegment.FromEllipse(bounds));
            CloseFigure();
            return this;
        }

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

        /// <summary>
        /// Transforms the already added items in this <see cref="Path"/> instance by applying the specified <paramref name="matrix"/>.
        /// This method does not change the value of the <see cref="Transformation"/> property.
        /// </summary>
        /// <param name="matrix">The <see cref="TransformationMatrix"/> to apply.</param>
        /// <returns>The current <see cref="Path"/> instance.</returns>
        /// <remarks>
        /// <para>To leave the current instance intact and return a new one, use the static <see cref="Transform(Path,TransformationMatrix)">Transform</see> method instead.</para>
        /// <para>To set the transformation for the items added afterward only, use the <see cref="TransformTranslation">TransformTranslation</see>,
        /// <see cref="TransformRotation">TransformRotation</see>, <see cref="TransformTranslation">TransformTranslation</see> or <see cref="TransformScale">TransformScale</see> methods.</para>
        /// </remarks>
        public Path TransformAdded(TransformationMatrix matrix)
        {
            if (IsEmpty || matrix.IsIdentity)
                return this;

            Invalidate();

            if (figures == null)
                currentFigure.Transform(matrix);
            else
            {
                foreach (Figure figure in figures)
                    figure.Transform(matrix);
            }

            return this;
        }

        /// <summary>
        /// Overwrites the current <see cref="Transformation"/> with a <see cref="TransformationMatrix"/> to be applied
        /// to the items that are added to the current <see cref="Path"/> after calling this method.
        /// </summary>
        /// <param name="matrix">The new <see cref="TransformationMatrix"/> to use.</param>
        /// <returns>The current <see cref="Path"/> instance.</returns>
        /// <remarks>
        /// <para>This method affects the items that are added after calling this method only. To transform the already added items use
        /// the <see cref="TransformAdded">TransformAdded</see> method instead.</para>
        /// </remarks>
        public Path SetTransformation(TransformationMatrix matrix)
        {
            transformation = matrix;
            return this;
        }

        public Path ResetTransformation()
        {
            transformation = TransformationMatrix.Identity;
            return this;
        }

        /// <summary>
        /// Applies a translation (offset) to the origin of the current <see cref="Transformation"/>.
        /// </summary>
        /// <param name="offsetX"></param>
        /// <param name="offsetY"></param>
        /// <returns></returns>
        public Path TransformTranslation(float offsetX, float offsetY)
        {
            transformation = TransformationMatrix.CreateTranslation(offsetX, offsetY) * transformation;
            return this;
        }

        // TODO
        //public Path TransformTranslation(Vector2 offset)
        //{
        //}

        public Path TransformRotation(float angle)
        {
            transformation = TransformationMatrix.CreateRotation(angle.ToRadian()) * transformation;
            return this;
        }

        public Path TransformScale(float scaleX, float scaleY)
        {
            transformation = TransformationMatrix.CreateScale(scaleX, scaleY) * transformation;
            return this;
        }

        #endregion

        #region Private Methods

        private void AddSegment(PathSegment segment)
        {
            Invalidate();
            if (currentFigure.IsClosed)
                StartFigure();
            if (!transformation.IsIdentity)
                segment.Transform(transformation);
            currentFigure.AddSegment(segment);
        }

        private void Invalidate() => rawPath = null;

        private RawPath InitRawPath()
        {
            var result = new RawPath(figures?.Count ?? 1);
            if (figures == null)
                result.AddRawFigure(currentFigure.GetPoints(), currentFigure.IsClosed);
            else
            {
                foreach (Figure figure in figures)
                    result.AddRawFigure(figure.GetPoints(), figure.IsClosed);
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