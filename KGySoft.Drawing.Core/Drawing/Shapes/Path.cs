#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: Path.cs
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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;

using KGySoft.Drawing.Imaging;

#endregion

namespace KGySoft.Drawing.Shapes
{
    /// <summary>
    /// Represents the path of a custom shape to be drawn or filled. The path can consist of multiple open or closed figures.
    /// <div style="display: none;"><br/>See the <a href="https://docs.kgysoft.net/drawing/html/T_KGySoft_Drawing_Shapes_Path.htm">online help</a> for an example with image.</div>
    /// </summary>
    /// <remarks>
    /// <para>Though you can use the dedicated methods of the <see cref="BitmapDataExtensions"/> class to draw or fill simple shapes, the <see cref="Path"/> class
    /// provides a more effective way to define custom shapes. The <see cref="Path"/> class can be used to define complex shapes consisting of multiple figures, which can be
    /// then drawn or filled in a single operation.</para>
    /// <para>But even if you need to draw or fill a simple shape multiple times, using a <see cref="Path"/> instance can be more effective, because the region of the path
    /// can be cached for faster drawing. The caching can be enabled by setting the <see cref="PreferCaching"/> property to <see langword="true"/>.</para>
    /// <note>Please note that in some cases , the <see cref="Path"/> class can be less effective than using the dedicated methods of the <see cref="BitmapDataExtensions"/> class.
    /// For example, drawing 1 pixel wide lines with no anti-aliasing may use a different algorithm that never uses caching. Also, very large regions may not be cached
    /// (this can be adjusted by the <see cref="DrawingOptions.CacheRegionLimit">DrawingOptions.CacheRegionLimit</see> property).</note>
    /// </remarks>
    /// <example>
    /// <para>The following example demonstrates how to create a <see cref="Path"/> instance to draw a custom shape:
    /// <code lang="C#"><![CDATA[
    /// // It supports flow syntax, so you could even inline it into a Draw/FillPath call:
    /// var path = new Path()
    ///     .TransformTranslation(1, 1)
    ///     .AddPolygon(new(50, 0), new(79, 90), new(2, 35), new(97, 35), new(21, 90))
    ///     .AddEllipse(0, 0, 100, 100)
    ///     .AddRoundedRectangle(0, 0, 100, 100, cornerRadius: 10);
    /// 
    /// // Calculating the required size of the bitmap, adding symmetric padding:
    /// var bounds = path.Bounds;
    /// var size = bounds.Size + new Size(bounds.Location) * 2;
    /// 
    /// // Now creating a managed bitmap data but you can also use the GetReadWriteBitmapData
    /// // extensions of the dedicated packages for a GDI+ Bitmap, WPF WriteableBitmap, SKBitmap, etc.
    /// using var bitmapData = BitmapDataFactory.CreateBitmapData(size);
    /// bitmapData.Clear(Color.Cyan);
    /// 
    /// // Using implicit 1 pixel wide solid pen and default drawing options:
    /// bitmapData.DrawPath(Color.Blue, path);]]></code></para>
    /// <para>The example above produces the following result:
    /// <br/><img src="../Help/Images/DrawingOptionsFastThinLinesEnabled.png" alt="Custom path drawn with a 1 pixel wide pen and default options."/></para>
    /// </example>
    public sealed class Path
    {
        #region Fields

        private List<Figure>? figures;
        private Figure currentFigure;
        private RawPath? rawPath;
        private TransformationMatrix transformation;

        #endregion

        #region Properties

        #region Public Properties

        /// <summary>
        /// Gets whether this <see cref="Path"/> instance is empty.
        /// </summary>
        public bool IsEmpty => figures == null && currentFigure.IsEmpty;

        /// <summary>
        /// Gets the bounds of this <see cref="Path"/> in pixels required for filling it by a <see cref="Brush"/>.
        /// When drawing, the returned bounds needed to be inflated depending on the corresponding <see cref="Pen.Width">Pen.Width</see>.
        /// </summary>
        /// <exception cref="OverflowException">The <see cref="Path"/> contains points that exceed the bounds of a <see cref="Rectangle"/>.</exception>
        public Rectangle Bounds => RawPath.Bounds;

        /// <summary>
        /// Gets the currently active transformation matrix that is applied to the items that are added to this <see cref="Path"/>.
        /// </summary>
        public TransformationMatrix Transformation => transformation;

        /// <summary>
        /// Gets or sets whether the region of the path is allowed to be cached for faster drawing.
        /// <br/>Default value: <see langword="true"/>, unless it was set to <see langword="false"/> in the constructor.
        /// <br/>See also the <see cref="DrawingOptions.CacheRegionLimit">DrawingOptions.CacheRegionLimit</see> property.
        /// </summary>
        /// <value><see langword="true"/> to allow caching the region of this <see cref="Path"/>,
        /// so subsequent fill/draw operation with the same unchanged path will be faster; otherwise, <see langword="false"/>.</value>
        public bool PreferCaching { get; set; }

        /// <summary>
        /// Gets a read-only collection of the figures this <see cref="Path"/> consists of.
        /// </summary>
        /// <remarks>
        /// <para>This property is meant to provide information about the figures of this <see cref="Path"/> instance for interoperability with other libraries, and it cannot be used to modify the figures.
        /// To add new figures or path segments to this <see cref="Path"/>, use the <see cref="StartFigure">StartFigure</see> and <c>Add...</c> methods instead.</para>
        /// <note type="tip">To obtain the figures as series of flattened points, you can also use the <see cref="GetPoints">GetPoints</see> method.</note>
        /// </remarks>
        // NOTE: IReadOnlyList<T> would be a more elegant return type but that is not available in .NET 3.5/4.0
        public ReadOnlyCollection<Figure> Figures => new ReadOnlyCollection<Figure>(figures ?? [currentFigure]);

        #endregion

        #region Internal Properties

        internal RawPath RawPath => rawPath ??= InitRawPath();

        #endregion

        #endregion

        #region Constructors

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Path"/> class.
        /// </summary>
        /// <param name="preferCaching"><see langword="true"/> to allow caching the region of this <see cref="Path"/>; otherwise, <see langword="false"/>.
        /// See the <see cref="PreferCaching"/> property for details. This parameter is optional.
        /// <br/>Default value: <see langword="true"/>.
        /// </param>
        public Path(bool preferCaching = true)
        {
            currentFigure = new Figure();
            transformation = TransformationMatrix.Identity;
            PreferCaching = preferCaching;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Path"/> class by copying the content of another instance.
        /// </summary>
        /// <param name="other"></param>
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

        /// <summary>
        /// Returns a new <see cref="Path"/> instance, whose figures are transformed by the specified <paramref name="matrix"/>.
        /// This method does not change the original <paramref name="path"/> instance. To transform the original instance, use the <see cref="TransformAdded">TransformAdded</see> method instead.
        /// </summary>
        /// <param name="path">The <see cref="Path"/> instance to transform.</param>
        /// <param name="matrix">The <see cref="TransformationMatrix"/> to apply.</param>
        /// <returns>A new <see cref="Path"/> instance that contains the same figures as the original <paramref name="path"/>, transformed by the specified <paramref name="matrix"/>.</returns>
        public static Path Transform(Path path, TransformationMatrix matrix)
        {
            var result = new Path(path);
            result.TransformAdded(matrix);
            return result;
        }

        #endregion

        #region Instance Methods

        #region Public Methods

        /// <summary>
        /// Adds a point to the current figure of this <see cref="Path"/> instance.
        /// </summary>
        /// <param name="x">The x-coordinate of the point to add.</param>
        /// <param name="y">The y-coordinate of the point to add.</param>
        /// <returns>This <see cref="Path"/> instance.</returns>
        /// <remarks>
        /// <para>If the current figure is empty, the added point will be the starting point of the figure.
        /// Otherwise, the added point will be connected to the last point of the figure.</para>
        /// <para>When the <see cref="Path"/> is filled, the figures require at least 3 non-collinear points to be rendered. When the <see cref="Path"/> is drawn, even a single point is rendered.
        /// If the <see cref="Pen.Width">Pen.Width</see> is 1, it means drawing a single pixel, in which case it's much faster
        /// to use the <see cref="IWritableBitmapData.SetColor32">IWritableBitmapData.SetColor32</see> method instead.</para>
        /// <para>The parameters are not validated here but in the moment of drawing the coordinates of the possibly transformed path points
        /// must fall into the bounds of an <see cref="int">int</see> value; otherwise, an <see cref="OverflowException"/> will be thrown.</para>
        /// </remarks>
        public Path AddPoint(float x, float y) => AddPoint(new PointF(x, y));

        /// <summary>
        /// Adds a point to the current figure of this <see cref="Path"/> instance.
        /// </summary>
        /// <param name="point">The point to add.</param>
        /// <returns>This <see cref="Path"/> instance.</returns>
        /// <remarks>
        /// <para>If the current figure is empty or closed, the added point will be the starting point of a new figure.
        /// Otherwise, the added point will be connected to the last point of the figure.</para>
        /// <para>When the <see cref="Path"/> is filled, the figures require at least 3 non-collinear points to be rendered. When the <see cref="Path"/> is drawn, even a single point is rendered.
        /// If the <see cref="Pen.Width">Pen.Width</see> is 1, it means drawing a single pixel, in which case it's much faster
        /// to use the <see cref="IWritableBitmapData.SetColor32">IWritableBitmapData.SetColor32</see> method instead.</para>
        /// <para>The <paramref name="point"/> parameter is not validated here but in the moment of drawing the coordinates of the possibly transformed path points
        /// must fall into the bounds of an <see cref="int">int</see> value; otherwise, an <see cref="OverflowException"/> will be thrown.</para>
        /// </remarks>
        public Path AddPoint(PointF point)
        {
            AppendPoints([point]);
            return this;
        }

        /// <summary>
        /// Adds a line to the current figure of this <see cref="Path"/> instance.
        /// </summary>
        /// <param name="x1">The x-coordinate of the starting point of the line to add.</param>
        /// <param name="y1">The y-coordinate of the starting point of the line to add.</param>
        /// <param name="x2">The x-coordinate of the end point of the line to add.</param>
        /// <param name="y2">The y-coordinate of the end point of the line to add.</param>
        /// <returns>This <see cref="Path"/> instance.</returns>
        /// <remarks>
        /// <para>If the current figure is not empty or closed, the added line will be connected to the last point of the figure.</para>
        /// <para>When the <see cref="Path"/> is filled, a single line is not rendered. When the <see cref="Path"/> is drawn, the right/bottom values of the coordinates are inclusive,
        /// so even a single pixel line is rendered.</para>
        /// <para>The parameters are not validated here but in the moment of drawing the coordinates of the possibly transformed path points
        /// must fall into the bounds of an <see cref="int">int</see> value; otherwise, an <see cref="OverflowException"/> will be thrown.</para>
        /// </remarks>
        public Path AddLine(float x1, float y1, float x2, float y2) => AddLine(new PointF(x1, y1), new PointF(x2, y2));

        /// <summary>
        /// Adds a line to the current figure of this <see cref="Path"/> instance.
        /// </summary>
        /// <param name="p1">The starting point of the line to add.</param>
        /// <param name="p2">The end point of the line to add.</param>
        /// <returns>This <see cref="Path"/> instance.</returns>
        /// <remarks>
        /// <para>If the current figure is not empty or closed, the added line will be connected to the last point of the figure.</para>
        /// <para>When the <see cref="Path"/> is filled, a single line is not rendered. When the <see cref="Path"/> is drawn, the right/bottom values of the coordinates are inclusive,
        /// so even a single pixel line is rendered.</para>
        /// <para>The parameters are not validated here but in the moment of drawing the coordinates of the possibly transformed path points
        /// must fall into the bounds of an <see cref="int">int</see> value; otherwise, an <see cref="OverflowException"/> will be thrown.</para>
        /// </remarks>
        public Path AddLine(PointF p1, PointF p2)
        {
            AppendPoints([p1, p2]);
            return this;
        }

        /// <summary>
        /// Adds a series of connected lines to the current figure of this <see cref="Path"/> instance.
        /// </summary>
        /// <param name="points">The points of the line segments to add to the current figure of this <see cref="Path"/>.</param>
        /// <returns>This <see cref="Path"/> instance.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="points"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// <para>The first point of the <paramref name="points"/> array will be the starting point of the first line segment.
        /// Each additional point specifies the endpoint of a line segment, whose starting point is the endpoint of the previous line.</para>
        /// <para>If the current figure is not empty or closed, the added lines will be connected to the last point of the figure.</para>
        /// <para>The <paramref name="points"/> are not validated here but in the moment of drawing the coordinates of the possibly transformed path points
        /// must fall into the bounds of an <see cref="int">int</see> value; otherwise, an <see cref="OverflowException"/> will be thrown.</para>
        /// </remarks>
        public Path AddLines(params PointF[] points)
        {
            if (points == null)
                throw new ArgumentNullException(nameof(points), PublicResources.ArgumentNull);

            if (points.Length == 0)
                return this;
            AppendPoints(points);
            return this;
        }

        /// <summary>
        /// Adds a series of connected lines to the current figure of this <see cref="Path"/> instance.
        /// </summary>
        /// <param name="points">The points of the line segments to add to the current figure of this <see cref="Path"/>.</param>
        /// <returns>This <see cref="Path"/> instance.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="points"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// <para>The first point of the <paramref name="points"/> array will be the starting point of the first line segment.
        /// Each additional point specifies the endpoint of a line segment, whose starting point is the endpoint of the previous line.</para>
        /// <para>If the current figure is not empty or closed, the added lines will be connected to the last point of the figure.</para>
        /// <para>The <paramref name="points"/> are not validated here but in the moment of drawing the coordinates of the possibly transformed path points
        /// must fall into the bounds of an <see cref="int">int</see> value; otherwise, an <see cref="OverflowException"/> will be thrown.</para>
        /// </remarks>
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

        /// <summary>
        /// Adds a polygon to this <see cref="Path"/>.
        /// </summary>
        /// <param name="points">The points of the polygon to add to this <see cref="Path"/>.</param>
        /// <returns>This <see cref="Path"/> instance.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="points"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// <para>The <paramref name="points"/> specify the vertices of the polygon.</para>
        /// <para>This method always adds a new closed figure to this <see cref="Path"/>.</para>
        /// <para>The <paramref name="points"/> are not validated here but in the moment of drawing the coordinates of the possibly transformed path points
        /// must fall into the bounds of an <see cref="int">int</see> value; otherwise, an <see cref="OverflowException"/> will be thrown.</para>
        /// </remarks>
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

        /// <summary>
        /// Adds a polygon to this <see cref="Path"/>.
        /// </summary>
        /// <param name="points">The points of the polygon to add to this <see cref="Path"/>.</param>
        /// <returns>This <see cref="Path"/> instance.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="points"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// <para>The <paramref name="points"/> specify the vertices of the polygon.</para>
        /// <para>This method always adds a new closed figure to this <see cref="Path"/>.</para>
        /// <para>The <paramref name="points"/> are not validated here but in the moment of drawing the coordinates of the possibly transformed path points
        /// must fall into the bounds of an <see cref="int">int</see> value; otherwise, an <see cref="OverflowException"/> will be thrown.</para>
        /// </remarks>
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

        /// <summary>
        /// Adds a rectangle to this <see cref="Path"/>.
        /// </summary>
        /// <param name="x">The x-coordinate of the upper-left corner.</param>
        /// <param name="y">The y-coordinate of the upper-left corner.</param>
        /// <param name="width">The width of the rectangle.</param>
        /// <param name="height">The height of the rectangle.</param>
        /// <returns>This <see cref="Path"/> instance.</returns>
        /// <remarks>
        /// <para>The rectangle is added as a new closed figure.</para>
        /// <para>When filling a rectangle (with identity transformation), the <paramref name="width"/> and <paramref name="height"/> parameters specify the size of the rectangle in pixels.</para>
        /// <para>When drawing a rectangle (with identity transformation), the right and bottom values are inclusive. If the width of the <see cref="Pen"/> is 1,
        /// a rectangle with zero width and height will be rendered as a single pixel.</para>
        /// <para>The parameters are not validated here but in the moment of drawing the coordinates of the possibly transformed path points
        /// must fall into the bounds of an <see cref="int">int</see> value; otherwise, an <see cref="OverflowException"/> will be thrown.</para>
        /// </remarks>
        public Path AddRectangle(float x, float y, float width, float height) => AddRectangle(new RectangleF(x, y, width, height));

        /// <summary>
        /// Adds a rectangle to this <see cref="Path"/>.
        /// </summary>
        /// <param name="rectangle">The rectangle to add to this <see cref="Path"/>.</param>
        /// <returns>This <see cref="Path"/> instance.</returns>
        /// <remarks>
        /// <para>The rectangle is added as a new closed figure.</para>
        /// <para>When filling a rectangle (with identity transformation), the <see cref="RectangleF.Width"/> and <see cref="RectangleF.Height"/> properties specify the size of the rectangle in pixels
        /// (<see cref="RectangleF.Right"/> and <see cref="RectangleF.Bottom"/> are exclusive).</para>
        /// <para>When drawing a rectangle (with identity transformation), the <see cref="RectangleF.Right"/> and <see cref="RectangleF.Bottom"/> values are inclusive.
        /// If the width of the <see cref="Pen"/> is 1, a rectangle with zero width and height will be rendered as a single pixel.</para>
        /// <para>The <paramref name="rectangle"/> parameter is not validated here but in the moment of drawing the coordinates of the possibly transformed path points
        /// must fall into the bounds of an <see cref="int">int</see> value; otherwise, an <see cref="OverflowException"/> will be thrown.</para>
        /// </remarks>
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

        /// <summary>
        /// Adds a Bézier curve to this <see cref="Path"/>.
        /// </summary>
        /// <param name="x1">The x-coordinate of the starting point of the Bézier curve.</param>
        /// <param name="y1">The y-coordinate of the starting point of the Bézier curve.</param>
        /// <param name="x2">The x-coordinate of the first control point of the Bézier curve.</param>
        /// <param name="y2">The y-coordinate of the first control point of the Bézier curve.</param>
        /// <param name="x3">The x-coordinate of the second control point of the Bézier curve.</param>
        /// <param name="y3">The y-coordinate of the second control point of the Bézier curve.</param>
        /// <param name="x4">The x-coordinate of the end point of the Bézier curve.</param>
        /// <param name="y4">The y-coordinate of the end point of the Bézier curve.</param>
        /// <returns>This <see cref="Path"/> instance.</returns>
        /// <remarks>
        /// <para>If the current figure is not empty or closed, the first point of the added curve will be connected to the last point of the figure.</para>
        /// <para>The parameters are not validated here but in the moment of drawing the coordinates of the possibly transformed path points
        /// must fall into the bounds of an <see cref="int">int</see> value; otherwise, an <see cref="OverflowException"/> will be thrown.</para>
        /// </remarks>
        public Path AddBezier(float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4)
            => AddBezier(new PointF(x1, y1), new PointF(x2, y2), new PointF(x3, y3), new PointF(x4, y4));

        /// <summary>
        /// Adds a Bézier curve to this <see cref="Path"/>.
        /// </summary>
        /// <param name="p1">The starting point of the Bézier curve.</param>
        /// <param name="p2">The first control point of the Bézier curve.</param>
        /// <param name="p3">The second control point of the Bézier curve.</param>
        /// <param name="p4">The end point of the Bézier curve.</param>
        /// <returns>This <see cref="Path"/> instance.</returns>
        /// <remarks>
        /// <para>If the current figure is not empty or closed, the first point of the added curve will be connected to the last point of the figure.</para>
        /// <para>The parameters are not validated here but in the moment of drawing the coordinates of the possibly transformed path points
        /// must fall into the bounds of an <see cref="int">int</see> value; otherwise, an <see cref="OverflowException"/> will be thrown.</para>
        /// </remarks>
        public Path AddBezier(PointF p1, PointF p2, PointF p3, PointF p4)
        {
            AddSegment(new BezierSegment(new[] { p1, p2, p3, p4 }));
            return this;
        }

        /// <summary>
        /// Adds a series of Bézier curves to this <see cref="Path"/>.
        /// </summary>
        /// <param name="points">The points that define the Bézier curves to add to this <see cref="Path"/>.</param>
        /// <returns>This <see cref="Path"/> instance.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="points"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">The number of points is not a multiple of 3 plus 1.</exception>
        /// <remarks>
        /// <para>The allowed number of points in <paramref name="points"/> is 0, 1, or a multiple of 3 plus 1.</para>
        /// <para>When <paramref name="points"/> has at least four items, the first four points define the first Bézier curve. Each additional three points define a new Bézier curve,
        /// where the last point of the previous curve is the starting point of the next curve.</para>
        /// <para>If the current figure is not empty or closed, the first point of the added curve will be connected to the last point of the figure.</para>
        /// <para>The coordinates of the specified <paramref name="points"/> are not validated here but in the moment of drawing the coordinates of the possibly transformed path points
        /// must fall into the bounds of an <see cref="int">int</see> value; otherwise, an <see cref="OverflowException"/> will be thrown.</para>
        /// </remarks>
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

        /// <summary>
        /// Adds a series of Bézier curves to this <see cref="Path"/>.
        /// </summary>
        /// <param name="points">The points that define the Bézier curves to add to this <see cref="Path"/>.</param>
        /// <returns>This <see cref="Path"/> instance.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="points"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">The number of points is not a multiple of 3 plus 1.</exception>
        /// <remarks>
        /// <para>The allowed number of points in <paramref name="points"/> is 0, 1, or a multiple of 3 plus 1.</para>
        /// <para>When <paramref name="points"/> has at least four items, the first four points define the first Bézier curve. Each additional three points define a new Bézier curve,
        /// where the last point of the previous curve is the starting point of the next curve.</para>
        /// <para>If the current figure is not empty or closed, the first point of the added curve will be connected to the last point of the figure.</para>
        /// <para>The coordinates of the specified <paramref name="points"/> are not validated here but in the moment of drawing the coordinates of the possibly transformed path points
        /// must fall into the bounds of an <see cref="int">int</see> value; otherwise, an <see cref="OverflowException"/> will be thrown.</para>
        /// </remarks>
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

        /// <summary>
        /// Adds an elliptical arc to this <see cref="Path"/>.
        /// </summary>
        /// <param name="x">The x-coordinate of the upper-left corner of the bounding rectangle that defines the ellipse from which the arc is drawn.</param>
        /// <param name="y">The y-coordinate of the upper-left corner of the bounding rectangle that defines the ellipse from which the arc is drawn.</param>
        /// <param name="width">The width of the bounding rectangle that defines the ellipse from which the arc is drawn.</param>
        /// <param name="height">The height of the bounding rectangle that defines the ellipse from which the arc is drawn.</param>
        /// <param name="startAngle">The starting angle of the arc, measured in degrees clockwise from the x-axis.</param>
        /// <param name="sweepAngle">The sweep angle of the arc, measured in degrees clockwise from <paramref name="startAngle"/>.
        /// If its absolute value is greater than or equal to 360, a complete ellipse is added to the path.</param>
        /// <returns>This <see cref="Path"/> instance.</returns>
        /// <remarks>
        /// <para>If the current figure is not empty or closed, the first point of the added arc will be connected to the last point of the figure.</para>
        /// <para>When a complete ellipse is added to the path (that is, when the absolute value of <paramref name="sweepAngle"/> is greater than or equal to 360),
        /// the <see cref="startAngle"/> is ignored as if it was 0. This matters when the ellipse is connected with other path segments.</para>
        /// <para>The coordinates of the specified bounding rectangle are not validated here but in the moment of drawing the coordinates of the possibly transformed path points
        /// must fall into the bounds of an <see cref="int">int</see> value; otherwise, an <see cref="OverflowException"/> will be thrown.</para>
        /// </remarks>
        public Path AddArc(float x, float y, float width, float height, float startAngle, float sweepAngle)
            => AddArc(new RectangleF(x, y, width, height), startAngle, sweepAngle);

        /// <summary>
        /// Adds an elliptical arc to this <see cref="Path"/>.
        /// </summary>
        /// <param name="bounds">The bounding rectangle that defines the ellipse from which the arc is drawn.</param>
        /// <param name="startAngle">The starting angle of the arc, measured in degrees clockwise from the x-axis.</param>
        /// <param name="sweepAngle">The sweep angle of the arc, measured in degrees clockwise from <paramref name="startAngle"/>.
        /// If its absolute value is greater than or equal to 360, a complete ellipse is added to the path.</param>
        /// <returns>This <see cref="Path"/> instance.</returns>
        /// <remarks>
        /// <para>If the current figure is not empty or closed, the first point of the added arc will be connected to the last point of the figure.</para>
        /// <para>When a complete ellipse is added to the path (that is, when the absolute value of <paramref name="sweepAngle"/> is greater than or equal to 360),
        /// the <see cref="startAngle"/> is ignored as if it was 0. This matters when the ellipse is connected with other path segments.</para>
        /// <para>The coordinates of the specified bounding rectangle are not validated here but in the moment of drawing the coordinates of the possibly transformed path points
        /// must fall into the bounds of an <see cref="int">int</see> value; otherwise, an <see cref="OverflowException"/> will be thrown.</para>
        /// </remarks>
        public Path AddArc(RectangleF bounds, float startAngle, float sweepAngle)
        {
            AddSegment(new ArcSegment(bounds, startAngle, sweepAngle));
            return this;
        }

        /// <summary>
        /// Adds a pie shape to this <see cref="Path"/>.
        /// </summary>
        /// <param name="x">The x-coordinate of the upper-left corner of the bounding rectangle that defines the ellipse from which the pie is drawn.</param>
        /// <param name="y">The y-coordinate of the upper-left corner of the bounding rectangle that defines the ellipse from which the pie is drawn.</param>
        /// <param name="width">The width of the bounding rectangle that defines the ellipse from which the pie is drawn.</param>
        /// <param name="height">The height of the bounding rectangle that defines the ellipse from which the pie is drawn.</param>
        /// <param name="startAngle">The starting angle of the pie shape, measured in degrees clockwise from the x-axis.</param>
        /// <param name="sweepAngle">The sweep angle of the pie shape, measured in degrees clockwise from <paramref name="startAngle"/>.</param>
        /// <returns>This <see cref="Path"/> instance.</returns>
        /// <remarks>
        /// <para>The pie shape is defined by a partial outline of an ellipse and the two radial lines that intersect the endpoints of the partial outline.
        /// The pie shape is added as a new closed figure.</para>
        /// <para>The coordinates of the specified bounding rectangle are not validated here but in the moment of drawing the coordinates of the possibly transformed path points
        /// must fall into the bounds of an <see cref="int">int</see> value; otherwise, an <see cref="OverflowException"/> will be thrown.</para>
        /// </remarks>
        public Path AddPie(float x, float y, float width, float height, float startAngle, float sweepAngle)
            => AddPie(new RectangleF(x, y, width, height), startAngle, sweepAngle);

        /// <summary>
        /// Adds a pie shape to this <see cref="Path"/>.
        /// </summary>
        /// <param name="bounds">The bounding rectangle that defines the ellipse from which the pie is drawn.</param>
        /// <param name="startAngle">The starting angle of the pie shape, measured in degrees clockwise from the x-axis.</param>
        /// <param name="sweepAngle">The sweep angle of the pie shape, measured in degrees clockwise from <paramref name="startAngle"/>.</param>
        /// <returns>This <see cref="Path"/> instance.</returns>
        /// <remarks>
        /// <para>The pie shape is defined by a partial outline of an ellipse and the two radial lines that intersect the endpoints of the partial outline.
        /// The pie shape is added as a new closed figure.</para>
        /// <para>The coordinates of the specified bounding rectangle are not validated here but in the moment of drawing the coordinates of the possibly transformed path points
        /// must fall into the bounds of an <see cref="int">int</see> value; otherwise, an <see cref="OverflowException"/> will be thrown.</para>
        /// </remarks>
        public Path AddPie(RectangleF bounds, float startAngle, float sweepAngle)
        {
            StartFigure();
            AddPoint(new PointF(bounds.Left + bounds.Width / 2f, bounds.Top + bounds.Height / 2f));
            AddSegment(new ArcSegment(bounds, startAngle, sweepAngle));
            CloseFigure();
            return this;
        }

        /// <summary>
        /// Adds an ellipse to this <see cref="Path"/>.
        /// </summary>
        /// <param name="x">The x-coordinate of the upper-left corner of the bounding rectangle that defines the ellipse.</param>
        /// <param name="y">The y-coordinate of the upper-left corner of the bounding rectangle that defines the ellipse.</param>
        /// <param name="width">The width of the bounding rectangle that defines the ellipse.</param>
        /// <param name="height">The height of the bounding rectangle that defines the ellipse.</param>
        /// <returns>This <see cref="Path"/> instance.</returns>
        /// <remarks>
        /// <para>The ellipse is added as a new closed figure.</para>
        /// <para>The coordinates of the specified bounding rectangle are not validated here but in the moment of drawing the coordinates of the possibly transformed path points
        /// must fall into the bounds of an <see cref="int">int</see> value; otherwise, an <see cref="OverflowException"/> will be thrown.</para>
        /// </remarks>
        public Path AddEllipse(float x, float y, float width, float height) => AddEllipse(new RectangleF(x, y, width, height));

        /// <summary>
        /// Adds an ellipse to this <see cref="Path"/>.
        /// </summary>
        /// <param name="bounds">The bounding rectangle that defines the ellipse.</param>
        /// <returns>This <see cref="Path"/> instance.</returns>
        /// <remarks>
        /// <para>The ellipse is added as a new closed figure.</para>
        /// <para>The coordinates of the specified bounding rectangle are not validated here but in the moment of drawing the coordinates of the possibly transformed path points
        /// must fall into the bounds of an <see cref="int">int</see> value; otherwise, an <see cref="OverflowException"/> will be thrown.</para>
        /// </remarks>
        public Path AddEllipse(RectangleF bounds)
        {
            StartFigure();
            AddSegment(new ArcSegment(bounds));
            CloseFigure();
            return this;
        }

        /// <summary>
        /// Adds a rounded rectangle to this <see cref="Path"/>, applying the same corner radius to all corners.
        /// </summary>
        /// <param name="x">The x-coordinate of the upper-left corner of the bounding rectangle that defines the rounded rectangle.</param>
        /// <param name="y">The y-coordinate of the upper-left corner of the bounding rectangle that defines the rounded rectangle.</param>
        /// <param name="width">The width of the bounding rectangle that defines the rounded rectangle.</param>
        /// <param name="height">The height of the bounding rectangle that defines the rounded rectangle.</param>
        /// <param name="cornerRadius">The size of the corner radius of the rounded rectangle for all corners.</param>
        /// <returns>This <see cref="Path"/> instance.</returns>
        /// <remarks>
        /// <para>The rounded rectangle is added as a new closed figure.</para>
        /// <para>If <paramref name="cornerRadius"/> is negative, the absolute value will be used. If it is greater than the half of the smaller side of the bounding rectangle,
        /// it will be adjusted to the half of the smaller side, so the result will be an oval shape. If the <paramref name="cornerRadius"/> is 0, a simple rectangle will be added.</para>
        /// <para>The coordinates of the specified bounding rectangle are not validated here but in the moment of drawing the coordinates of the possibly transformed path points
        /// must fall into the bounds of an <see cref="int">int</see> value; otherwise, an <see cref="OverflowException"/> will be thrown.</para>
        /// </remarks>
        public Path AddRoundedRectangle(float x, float y, float width, float height, float cornerRadius)
            => AddRoundedRectangle(new RectangleF(x, y, width, height), cornerRadius);

        /// <summary>
        /// Adds a rounded rectangle to this <see cref="Path"/>, applying the same corner radius to all corners.
        /// </summary>
        /// <param name="bounds">The bounding rectangle that defines the rounded rectangle.</param>
        /// <param name="cornerRadius">The size of the corner radius of the rounded rectangle for all corners.</param>
        /// <returns>This <see cref="Path"/> instance.</returns>
        /// <remarks>
        /// <para>The rounded rectangle is added as a new closed figure.</para>
        /// <para>If <paramref name="cornerRadius"/> is negative, the absolute value will be used. If it is greater than the half of the smaller side of the bounding rectangle,
        /// it will be adjusted to the half of the smaller side, so the result will be an oval shape. If the <paramref name="cornerRadius"/> is 0, a simple rectangle will be added.</para>
        /// <para>The coordinates of the specified bounding rectangle are not validated here but in the moment of drawing the coordinates of the possibly transformed path points
        /// must fall into the bounds of an <see cref="int">int</see> value; otherwise, an <see cref="OverflowException"/> will be thrown.</para>
        /// </remarks>
        public Path AddRoundedRectangle(RectangleF bounds, float cornerRadius)
        {
            if (cornerRadius == 0f) // not using tolerance because the path still can be scaled
                return AddRectangle(bounds);

            bounds.Normalize();
            StartFigure();
            float diameter = Math.Min(Math.Abs(cornerRadius) * 2f, Math.Min(Math.Abs(bounds.Width), Math.Abs(bounds.Height)));
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

        /// <summary>
        /// Adds a rounded rectangle to this <see cref="Path"/>, applying a custom corner radius to each corner.
        /// </summary>
        /// <param name="x">The x-coordinate of the upper-left corner of the bounding rectangle that defines the rounded rectangle.</param>
        /// <param name="y">The y-coordinate of the upper-left corner of the bounding rectangle that defines the rounded rectangle.</param>
        /// <param name="width">The width of the bounding rectangle that defines the rounded rectangle.</param>
        /// <param name="height">The height of the bounding rectangle that defines the rounded rectangle.</param>
        /// <param name="radiusTopLeft">The size of the top-left corner radius of the rounded rectangle.</param>
        /// <param name="radiusTopRight">The size of the top-right corner radius of the rounded rectangle.</param>
        /// <param name="radiusBottomRight">The size of the bottom-right corner radius of the rounded rectangle.</param>
        /// <param name="radiusBottomLeft">The size of the bottom-left corner radius of the rounded rectangle.</param>
        /// <returns>This <see cref="Path"/> instance.</returns>
        /// <remarks>
        /// <para>The rounded rectangle is added as a new closed figure.</para>
        /// <para>If any of the corner radius parameters is negative, the absolute value will be used. If the sum of any adjacent corner radius parameters is greater
        /// than the corresponding side of the bounding rectangle, then all corner radius parameters will be scaled down proportionally to fit into the bounding rectangle.</para>
        /// <para>The coordinates of the specified bounding rectangle are not validated here but in the moment of drawing the coordinates of the possibly transformed path points
        /// must fall into the bounds of an <see cref="int">int</see> value; otherwise, an <see cref="OverflowException"/> will be thrown.</para>
        /// </remarks>
        public Path AddRoundedRectangle(float x, float y, float width, float height, float radiusTopLeft, float radiusTopRight, float radiusBottomRight, float radiusBottomLeft)
            => AddRoundedRectangle(new RectangleF(x, y, width, height), radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft);

        /// <summary>
        /// Adds a rounded rectangle to this <see cref="Path"/>, applying a custom corner radius to each corner.
        /// </summary>
        /// <param name="bounds">The bounding rectangle that defines the rounded rectangle.</param>
        /// <param name="radiusTopLeft">The size of the top-left corner radius of the rounded rectangle.</param>
        /// <param name="radiusTopRight">The size of the top-right corner radius of the rounded rectangle.</param>
        /// <param name="radiusBottomRight">The size of the bottom-right corner radius of the rounded rectangle.</param>
        /// <param name="radiusBottomLeft">The size of the bottom-left corner radius of the rounded rectangle.</param>
        /// <returns>This <see cref="Path"/> instance.</returns>
        /// <remarks>
        /// <para>The rounded rectangle is added as a new closed figure.</para>
        /// <para>If any of the corner radius parameters is negative, the absolute value will be used. If the sum of any adjacent corner radius parameters is greater
        /// than the corresponding side of the bounding rectangle, then all corner radius parameters will be scaled down proportionally to fit into the bounding rectangle.</para>
        /// <para>The coordinates of the specified bounding rectangle are not validated here but in the moment of drawing the coordinates of the possibly transformed path points
        /// must fall into the bounds of an <see cref="int">int</see> value; otherwise, an <see cref="OverflowException"/> will be thrown.</para>
        /// </remarks>
        [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator", Justification = "(In)equality is handled correctly")]
        public Path AddRoundedRectangle(RectangleF bounds, float radiusTopLeft, float radiusTopRight, float radiusBottomRight, float radiusBottomLeft)
        {
            bounds.Normalize();
            StartFigure();

            // Adjusting radii to the bounds
            radiusTopLeft = Math.Abs(radiusTopLeft);
            radiusTopRight = Math.Abs(radiusTopRight);
            radiusBottomRight = Math.Abs(radiusBottomRight);
            radiusBottomLeft = Math.Abs(radiusBottomLeft);
            float maxDiameterWidth = Math.Max(radiusTopLeft + radiusTopRight, radiusBottomLeft + radiusBottomRight);
            float maxDiameterHeight = Math.Max(radiusTopLeft + radiusBottomLeft, radiusTopRight + radiusBottomRight);
            if (maxDiameterWidth > bounds.Width || maxDiameterHeight > bounds.Height)
            {
                float scale = Math.Min(bounds.Width / maxDiameterWidth, bounds.Height / maxDiameterHeight);
                radiusTopLeft *= scale;
                radiusTopRight *= scale;
                radiusBottomRight *= scale;
                radiusBottomLeft *= scale;
            }

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

        /// <summary>
        /// Adds the figures of another <see cref="Path"/> instance to this <see cref="Path"/>.
        /// </summary>
        /// <param name="path">The <see cref="Path"/> instance to add to this <see cref="Path"/>.</param>
        /// <param name="connect"><see langword="true"/> to connect the last figure of this <see cref="Path"/> to the first figure of the added <paramref name="path"/>; otherwise, <see langword="false"/>.
        /// If the last figure of this <see cref="Path"/> is closed or empty, this parameter is ignored.</param>
        /// <returns>This <see cref="Path"/> instance.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// <para>The original <see cref="Path"/> instance remains intact.</para>
        /// <para>The current <see cref="Transformation"/> is applied to the added figures.</para>
        /// </remarks>
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
                    foreach (PathSegment segment in figure.SegmentsInternal)
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

        /// <summary>
        /// Gets the points of the figures in this <see cref="Path"/> instance.
        /// </summary>
        /// <returns>The points of the figures in this <see cref="Path"/> instance.</returns>
        /// <remarks>
        /// <para>The returned list contains the points of the figures in the order they were added to the <see cref="Path"/> instance.
        /// Every figure is represented by an array of flattened points that can be interpreted as a series of connected lines.</para>
        /// <para>If a figure is closed and has at least 3 points, then it is ensured that the last point of the figure is the same as the first point.</para>
        /// <para>This method can be used to provide a bridge between the <see cref="Path"/> class and other graphics libraries or APIs.</para>
        /// <note type="tip">To obtain figures as they were added to the <see cref="Path"/> without flattening, you can also use the <see cref="Figures"/> property.</note>
        /// </remarks>
        public IList<PointF[]> GetPoints()
        {
            if (figures == null)
                return [currentFigure.GetPoints(true).ToArray()];
            var result = new List<PointF[]>(figures.Count);
            foreach (Figure figure in figures)
                result.Add(figure.GetPoints(true).ToArray());
            return result;
        }

        /// <summary>
        /// Closes the current figure. If the current figure is empty or already closed, this method has no effect.
        /// </summary>
        /// <returns>This <see cref="Path"/> instance.</returns>
        /// <remarks>
        /// <para>Closing a figure means that the last point of the figure will be connected to the first point of the figure. This makes a different
        /// from <see cref="StartFigure">StartFigure</see> only when the <see cref="Path"/> is drawn, because filling always treats the figures as if they were closed.</para>
        /// <para>After calling this method the next added element will always start a new figure.</para>
        /// <para>A single point or a line section is always interpreted as an open figure. If the current figure has only a point or a single line,
        /// this method has the same effect as <see cref="StartFigure">StartFigure</see>.</para>
        /// </remarks>
        public Path CloseFigure()
        {
            // not closing if empty because it would be skipped anyway
            if (currentFigure.IsClosed || currentFigure.IsEmpty)
                return this;

            Invalidate();
            currentFigure.Close();
            return this;
        }

        /// <summary>
        /// Starts a new figure without closing the current figure. If the current figure is empty, this method has no effect.
        /// <br/>See also the <strong>Remarks</strong> section of the <see cref="CloseFigure">CloseFigure</see> method for details.
        /// </summary>
        /// <returns>This <see cref="Path"/> instance.</returns>
        public Path StartFigure()
        {
            if (currentFigure.IsEmpty)
            {
                currentFigure.Close();
                return this;
            }

            figures ??= new List<Figure>(2) { currentFigure };
            figures.Add(currentFigure = new Figure());
            return this;
        }

        /// <summary>
        /// Gets a <see cref="Path"/> instance from this <see cref="Path"/>, in which every figure is closed.
        /// </summary>
        /// <returns>This <see cref="Path"/> instance, if every figure is already closed; otherwise,
        /// a new <see cref="Path"/> instance, in which every figure is closed.</returns>
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
        /// <see cref="TransformRotation">TransformRotation</see>, <see cref="TransformScale">TransformScale</see> or <see cref="SetTransformation">SetTransformation</see> methods.</para>
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
        /// <returns>This <see cref="Path"/> instance.</returns>
        /// <remarks>
        /// <para>This method affects the items that are added after calling this method only. To transform the already added items use
        /// the <see cref="TransformAdded">TransformAdded</see> method instead.</para>
        /// </remarks>
        public Path SetTransformation(TransformationMatrix matrix)
        {
            transformation = matrix;
            return this;
        }

        /// <summary>
        /// Resets the current <see cref="Transformation"/> to the identity matrix.
        /// </summary>
        /// <returns>This <see cref="Path"/> instance.</returns>
        /// <remarks>
        /// <para>This method affects the items that are added after calling this method only. To transform the already added items use
        /// the <see cref="TransformAdded">TransformAdded</see> method instead.</para>
        /// </remarks>
        public Path ResetTransformation()
        {
            transformation = TransformationMatrix.Identity;
            return this;
        }

        /// <summary>
        /// Applies a translation (offset) to the origin of the current <see cref="Transformation"/>.
        /// </summary>
        /// <param name="offsetX">The x-coordinate of the translation.</param>
        /// <param name="offsetY">The y-coordinate of the translation.</param>
        /// <returns>This <see cref="Path"/> instance.</returns>
        /// <remarks>
        /// <para>This method affects the items that are added after calling this method only. To transform the already added items use
        /// the <see cref="TransformAdded">TransformAdded</see> method instead.</para>
        /// </remarks>
        public Path TransformTranslation(float offsetX, float offsetY)
        {
            transformation = TransformationMatrix.CreateTranslation(offsetX, offsetY) * transformation;
            return this;
        }

        // TODO
        //public Path TransformTranslation(Vector2 offset)
        //{
        //}

        // NOTE: As opposed to TransformationMatrix.CreateRotation, this method uses degrees to conform with the other methods and also with other popular APIs.
        /// <summary>
        /// Applies a rotation to the current <see cref="Transformation"/>.
        /// </summary>
        /// <param name="angle">The angle of the rotation in degrees.</param>
        /// <returns>This <see cref="Path"/> instance.</returns>
        /// <remarks>
        /// <para>This method affects the items that are added after calling this method only. To transform the already added items use
        /// the <see cref="TransformAdded">TransformAdded</see> method instead.</para>
        /// </remarks>
        public Path TransformRotation(float angle)
        {
            transformation = TransformationMatrix.CreateRotationDegrees(angle) * transformation;
            return this;
        }

        /// <summary>
        /// Applies a scaling (zoom) to the current <see cref="Transformation"/>.
        /// </summary>
        /// <param name="scaleX">The scaling factor in the x-direction.</param>
        /// <param name="scaleY">The scaling factor in the y-direction.</param>
        /// <returns>This <see cref="Path"/> instance.</returns>
        /// <remarks>
        /// <para>This method affects the items that are added after calling this method only. To transform the already added items use
        /// the <see cref="TransformAdded">TransformAdded</see> method instead.</para>
        /// </remarks>
        public Path TransformScale(float scaleX, float scaleY)
        {
            transformation = TransformationMatrix.CreateScale(scaleX, scaleY) * transformation;
            return this;
        }

        #endregion

        #region Internal Methods

        internal Path AddBeziers(List<PointF> points)
        {
            Debug.Assert(points.Count == 0 || (points.Count - 1) % 3 == 0);
            if (points.Count == 0)
                return this;
            AddSegment(new BezierSegment(points));
            return this;
        }

        // Gets the path segments, connecting the open segments if needed.
        internal List<PathSegment> GetSegments()
        {
            var result = new List<PathSegment>();
            foreach (Figure figure in figures ?? [currentFigure])
            {
                if (figure.IsEmpty)
                    continue;

                List<PathSegment> segments = figure.SegmentsInternal;

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
                result.AddRawFigure(currentFigure.GetPoints(false), currentFigure.IsClosed, false);
            else
            {
                foreach (Figure figure in figures)
                    result.AddRawFigure(figure.GetPoints(false), figure.IsClosed, false);
            }

            return result;
        }

        #endregion

        #endregion

        #endregion
    }
}