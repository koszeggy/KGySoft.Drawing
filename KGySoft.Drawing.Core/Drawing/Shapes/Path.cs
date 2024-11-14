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
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;

using KGySoft.Reflection;

#endregion

namespace KGySoft.Drawing.Shapes
{
    /// <summary>
    /// Represents the path of a custom shape to be drawn or filled. The path can consist of multiple open or closed figures.
    /// </summary>
    public sealed class Path
    {
        #region Nested Classes

        private sealed class Figure
        {
            #region Properties

            internal bool IsClosed { get; set; }
            internal bool IsEmpty => Segments.Count == 0;
            internal List<PathSegment> Segments { get; }

            #endregion

            #region Constructors

            internal Figure() => Segments = new List<PathSegment>();

            internal Figure(Figure other, bool close)
            {
                int count = other.Segments.Count;
                Segments = new List<PathSegment>(count);
                for (int i = 0; i < count; i++)
                    Segments.Add(other.Segments[i].Clone());

                IsClosed = close || other.IsClosed;
            }

            #endregion

            #region Methods

            internal void AddSegment(PathSegment segment) => Segments.Add(segment);

            [SuppressMessage("ReSharper", "UseIndexFromEndExpression", Justification = "Targeting older frameworks that don't support indexing from end.")]
            internal bool TryAppendPoints(IEnumerable<PointF> points)
            {
                if (Segments.Count == 0 || Segments[Segments.Count - 1] is not LineSegment lastSegment)
                    return false;

                if (IsClosed)
                {
                    if (!IsEmpty)
                        return false;
                    IsClosed = false;
                }

                lastSegment.Append(points);
                return true;
            }

            internal IList<PointF> GetPoints()
            {
                switch (Segments.Count)
                {
                    case 0:
                        return Reflector.EmptyArray<PointF>();
                    case 1:
                        return Segments[0].GetFlattenedPoints();
                    default:
                        var result = new List<PointF>();
                        foreach (PathSegment segment in Segments)
                            result.AddRange(segment.GetFlattenedPoints());
                        return result;
                }
            }

            internal void Transform(TransformationMatrix matrix)
            {
                for (int i = 0; i < Segments.Count; i++)
                {
                    PathSegment segment = Segments[i];
                    PathSegment transformedSegment = segment.Transform(matrix);
                    if (!ReferenceEquals(segment, transformedSegment))
                        Segments[i] = transformedSegment;
                }
            }

            #endregion
        }

        #endregion

        #region Fields

        private List<Figure>? figures;
        private Figure currentFigure;
        private RawPath? rawPath;
        private TransformationMatrix transformation;

        #endregion

        #region Properties

        #region Public Properties

        public bool IsEmpty => figures == null && currentFigure.IsEmpty;

        /// <summary>
        /// Gets the bounds of this <see cref="Path"/> in pixels required for filling by a <see cref="Brush"/>.
        /// When drawing, the returned bounds needed to be inflated depending on the corresponding <see cref="Pen"/>&#160;<see cref="Pen.Width"/>.
        /// </summary>
        /// <exception cref="OverflowException">The <see cref="Path"/> contains points that exceed the bounds of a <see cref="Rectangle"/>.</exception>
        public Rectangle Bounds => RawPath.Bounds;

        public TransformationMatrix Transformation => transformation;

        /// <summary>
        /// Gets or sets whether the region of the path is allowed to be cached for faster drawing.
        /// See also the <see cref="DrawingOptions.CacheRegionLimit">DrawingOptions.CacheRegionLimit</see> property.
        /// </summary>
        /// <value><see langword="true"/> to allow caching the region of thins <see cref="Path"/>,
        /// so subsequent fill/draw operation with the same unchanged path will be faster; otherwise, <see langword="false"/>. </value>
        public bool PreferCaching { get; set; }

        #endregion

        #region Internal Properties

        internal RawPath RawPath => rawPath ??= InitRawPath();

        #endregion

        #endregion

        #region Constructors

        #region Public Constructors

        public Path(bool preferCaching = true)
        {
            currentFigure = new Figure();
            transformation = TransformationMatrix.Identity;
            PreferCaching = preferCaching;
        }

        public Path(Path other) : this(other, false)
        {
        }

        #endregion

        #region Private Constructors

        private Path(Path other, bool close)
        {
            transformation = other.transformation;
            PreferCaching = other.PreferCaching;
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

        public Path AddPoint(float x, float y) => AddPoint(new PointF(x, y));

        public Path AddPoint(PointF point)
        {
            AppendPoints([point]);
            return this;
        }

        public Path AddLine(float x1, float y1, float x2, float y2) => AddLine(new PointF(x1, y1), new PointF(x2, y2));

        public Path AddLine(PointF p1, PointF p2)
        {
            AppendPoints([p1, p2]);
            return this;
        }

        public Path AddLines(params PointF[] points)
        {
            if (points == null)
                throw new ArgumentNullException(nameof(points), PublicResources.ArgumentNull);

            if (points.Length == 0)
                return this;
            AppendPoints(points);
            return this;
        }

        public Path AddLines(IEnumerable<PointF> points)
        {
            if (points == null)
                throw new ArgumentNullException(nameof(points), PublicResources.ArgumentNull);

            var pointsList = new List<PointF>(points);
            if (pointsList.Count == 0)
                return this;

            AppendPoints(pointsList);
            return this;
        }

        public Path AddPolygon(params PointF[] points)
        {
            if (points == null)
                throw new ArgumentNullException(nameof(points), PublicResources.ArgumentNull);

            if (points.Length == 0)
                return this;
            StartFigure();
            AddSegment(new LineSegment(points));
            CloseFigure();
            return this;
        }

        public Path AddPolygon(IEnumerable<PointF> points)
        {
            if (points == null)
                throw new ArgumentNullException(nameof(points), PublicResources.ArgumentNull);

            var pointsList = new List<PointF>(points);
            if (pointsList.Count == 0)
                return this;

            StartFigure();
            AddSegment(new LineSegment(pointsList));
            CloseFigure();
            return this;
        }

        public Path AddRectangle(float x, float y, float width, float height) => AddRectangle(new RectangleF(x, y, width, height));

        public Path AddRectangle(RectangleF rectangle)
        {
            StartFigure();
            AddSegment(new LineSegment([
                rectangle.Location,
                new PointF(rectangle.Right, rectangle.Top),
                new PointF(rectangle.Right, rectangle.Bottom),
                new PointF(rectangle.Left, rectangle.Bottom)
            ]));
            CloseFigure();
            return this;
        }

        public Path AddBezier(float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4)
            => AddBezier(new PointF(x1, y1), new PointF(x2, y2), new PointF(x3, y3), new PointF(x4, y4));

        public Path AddBezier(PointF p1, PointF p2, PointF p3, PointF p4)
        {
            AddSegment(new BezierSegment(new[] { p1, p2, p3, p4 }));
            return this;
        }

        // TODO: IEnumerable
        public Path AddBeziers(params PointF[] points)
        {
            if (points == null)
                throw new ArgumentNullException(nameof(points), PublicResources.ArgumentNull);
            if (points.Length == 0)
                return this;
            if ((points.Length - 1) % 3 != 0)
                throw new ArgumentException(nameof(points), Res.ShapesBezierPointsInvalid);
            AddSegment(new BezierSegment((PointF[])points.Clone()));
            return this;
        }

        public Path AddBeziers(IEnumerable<PointF> points)
        {
            if (points == null)
                throw new ArgumentNullException(nameof(points), PublicResources.ArgumentNull);
            var pointsList = new List<PointF>(points);
            if (pointsList.Count == 0)
                return this;
            if ((pointsList.Count - 1) % 3 != 0)
                throw new ArgumentException(nameof(points), Res.ShapesBezierPointsInvalid);
            AddSegment(new BezierSegment(pointsList));
            return this;
        }

        public Path AddArc(float x, float y, float width, float height, float startAngle, float sweepAngle)
            => AddArc(new RectangleF(x, y, width, height), startAngle, sweepAngle);

        public Path AddArc(RectangleF bounds, float startAngle, float sweepAngle)
        {
            // TODO: validation (bounds width/height, etc)
            AddSegment(new ArcSegment(bounds, startAngle, sweepAngle));
            return this;
        }

        public Path AddPie(float x, float y, float width, float height, float startAngle, float sweepAngle)
            => AddPie(new RectangleF(x, y, width, height), startAngle, sweepAngle);

        public Path AddPie(RectangleF bounds, float startAngle, float sweepAngle)
        {
            // TODO: validation (bounds width/height, etc)
            StartFigure();
            AddPoint(new PointF(bounds.Left + bounds.Width / 2f, bounds.Top + bounds.Height / 2f));
            AddSegment(new ArcSegment(bounds, startAngle, sweepAngle));
            CloseFigure();
            return this;
        }

        public Path AddEllipse(float x, float y, float width, float height) => AddEllipse(new RectangleF(x, y, width, height));

        public Path AddEllipse(RectangleF bounds)
        {
            // TODO: validation (bounds width/height, etc)
            StartFigure();
            AddSegment(new ArcSegment(bounds));
            CloseFigure();
            return this;
        }

        public Path AddRoundedRectangle(float x, float y, float width, float height, float radius)
            => AddRoundedRectangle(new RectangleF(x, y, width, height), radius);

        public Path AddRoundedRectangle(RectangleF bounds, float radius)
        {
            // TODO: validation (bounds width/height, etc)
            if (radius <= 0f)
                return AddRectangle(bounds);

            StartFigure();
            float diameter = radius * 2f;
            var corner = new RectangleF(bounds.Location, new SizeF(diameter, diameter));

            // top left
            AddArc(corner, 180f, 90f);
            
            // top right
            corner.X = bounds.Right - diameter;
            AddArc(corner, 270f, 90f);
            
            // bottom right
            corner.Y = bounds.Bottom - diameter;
            AddArc(corner, 0f, 90f);
            
            // bottom left
            corner.X = bounds.Left;
            AddArc(corner, 90f, 90f);

            CloseFigure();
            return this;
        }

        public Path AddRoundedRectangle(float x, float y, float width, float height, float radiusTopLeft, float radiusTopRight, float radiusBottomRight, float radiusBottomLeft)
            => AddRoundedRectangle(new RectangleF(x, y, width, height), radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft);

        [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator", Justification = "(In)equality is handled correctly")]
        public Path AddRoundedRectangle(RectangleF bounds, float radiusTopLeft, float radiusTopRight, float radiusBottomRight, float radiusBottomLeft)
        {
            StartFigure();

            // top left
            var corner = new RectangleF(bounds.Location, new SizeF(radiusTopLeft * 2f, radiusTopLeft * 2f));
            if (radiusTopLeft > 0f)
                AddArc(corner, 180f, 90f);
            else
                AddPoint(corner.Location);

            // top right
            if (radiusTopRight != radiusTopLeft)
                corner.Size = new SizeF(radiusTopRight * 2f, radiusTopRight * 2f);
            corner.X = bounds.Right - corner.Width;
            if (radiusTopRight > 0f)
                AddArc(corner, 270f, 90f);
            else
                AddPoint(corner.Location);

            // bottom right
            if (radiusBottomRight != radiusTopRight)
            {
                corner.Size = new SizeF(radiusBottomRight * 2f, radiusBottomRight * 2f);
                corner.X = bounds.Right - corner.Width;
            }

            corner.Y = bounds.Bottom - corner.Height;
            if (radiusBottomRight > 0f)
                AddArc(corner, 0f, 90f);
            else
                AddPoint(corner.Location);

            // bottom left
            if (radiusBottomLeft != radiusBottomRight)
            {
                corner.Size = new SizeF(radiusBottomLeft * 2f, radiusBottomLeft * 2f);
                corner.Y = bounds.Bottom - corner.Height;
            }

            corner.X = bounds.Left;
            if (radiusBottomLeft > 0f)
                AddArc(corner, 90f, 90f);
            else
                AddPoint(corner.Location);

            CloseFigure();
            return this;
        }

        // connect: only if the last figure of this path is not closed
        // the original path is transformed by the current transformation
        public Path AddPath(Path path, bool connect)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path), PublicResources.ArgumentNull);
            if (path.IsEmpty)
                return this;

            bool isFirst = true;
            foreach (Figure figure in path.figures ?? [path.currentFigure])
            {
                if (IsEmpty || isFirst && connect)
                {
                    foreach (PathSegment segment in figure.Segments)
                    {
                        PathSegment segmentToAdd = segment.Clone();
                        if (!transformation.IsIdentity)
                            segmentToAdd = segmentToAdd.Transform(transformation);
                        AddSegment(segmentToAdd);
                    }

                    isFirst = false;
                    continue;
                }

                figures ??= new List<Figure>(2) { currentFigure };
                var figureToAdd = new Figure(figure, false);
                if (!transformation.IsIdentity)
                    figureToAdd.Transform(transformation);
                figures.Add(figureToAdd);
            }

            currentFigure = figures![figures.Count - 1];
            return this;
        }

        public IList<PointF[]> GetPoints()
        {
            if (figures == null)
                return [currentFigure.GetPoints().ToArray()];
            var result = new List<PointF[]>(figures.Count);
            foreach (Figure figure in figures)
                result.Add(figure.GetPoints().ToArray());
            return result;
        }

        // The next point will be a new starting point.
        // A single point or a line section is always rendered as an open figure. After a point or a single line this method has the same effect as StartFigure.
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

        #region Internal Methods

        // Gets the path segments, connecting the open segments if needed.
        internal List<PathSegment> GetSegments()
        {
            var result = new List<PathSegment>();
            foreach (Figure figure in figures ?? [currentFigure])
            {
                if (figure.IsEmpty)
                    continue;

                List<PathSegment> segments = figure.Segments;

                int count = segments.Count;
                for (int i = 0; i < count; i++)
                {
                    PathSegment segment = segments[i];

                    // returning the actual segment
                    result.Add(segment);

                    // returning an implicit connecting segment if needed
                    if (i < count - 1)
                    {
                        // connecting the points of two segments if needed
                        if (segment.EndPoint != segments[i + 1].StartPoint)
                            result.Add(new LineSegment([segment.EndPoint, segments[i + 1].StartPoint]));
                    }
                    else if (figure.IsClosed)
                    {
                        // connecting the last and the first point of the figure if it is closed
                        if (segment.EndPoint != segments[0].StartPoint)
                            result.Add(new LineSegment([segment.EndPoint, segments[0].StartPoint]));
                    }
                }
            }

            return result;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// This overload does not copy the points in LineSegment.ctor.
        /// Call from non-collection methods only.
        /// </summary>
        private void AppendPoints(List<PointF> points)
        {
            Debug.Assert(points != null! && points.Count > 0);
            if (transformation.IsIdentity && currentFigure.TryAppendPoints(points!))
            {
                Invalidate();
                return;
            }

            AddSegment(new LineSegment(points!));
        }

        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration", Justification = "TryAppendPoints does not enumerate if it returns false")]
        private void AppendPoints(IEnumerable<PointF> points)
        {
            if (transformation.IsIdentity && currentFigure.TryAppendPoints(points))
            {
                Invalidate();
                return;
            }

            AddSegment(new LineSegment(points));
        }

        private void AddSegment(PathSegment segment)
        {
            Invalidate();
            if (currentFigure.IsClosed)
                StartFigure();
            if (!transformation.IsIdentity)
                segment = segment.Transform(transformation);
            currentFigure.AddSegment(segment);
        }

        private void Invalidate() => rawPath = null;

        private RawPath InitRawPath()
        {
            var result = new RawPath(figures?.Count ?? 1);
            if (figures == null)
                result.AddRawFigure(currentFigure.GetPoints(), currentFigure.IsClosed, false);
            else
            {
                foreach (Figure figure in figures)
                    result.AddRawFigure(figure.GetPoints(), figure.IsClosed, false);
            }

            return result;
        }

        #endregion

        #endregion

        #endregion
    }
}