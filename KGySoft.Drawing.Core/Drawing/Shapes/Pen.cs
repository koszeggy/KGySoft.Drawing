#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: Pen.cs
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

using KGySoft.CoreLibraries;
using KGySoft.Drawing.Imaging;
using KGySoft.Threading;

#endregion

namespace KGySoft.Drawing.Shapes
{
    /// <summary>
    /// Represents a pen for drawing operations.
    /// <div style="display: none;"><br/>See the <a href="https://docs.kgysoft.net/drawing/html/T_KGySoft_Drawing_Shapes_Pen.htm">online help</a> for image examples.</div>
    /// </summary>
    /// <remarks>
    /// <para><see cref="Pen"/>s are used to draw the outline of shapes, or to draw primitive lines and curves.</para>
    /// <para>A <see cref="Pen"/> instance is defined by a <see cref="Shapes.Brush"/> (possibly implicitly by a single color) that determines the color or texture of the pen,
    /// and a <see cref="Width"/> that determines the width of the pen. The <see cref="LineJoin"/>, <see cref="MiterLimit"/>, <see cref="StartCap"/>, and <see cref="EndCap"/>
    /// properties determine how the ends and joins of lines are drawn.</para>
    /// </remarks>
    /// <example>
    /// <para>The following table illustrates the behavior of some properties of the <see cref="Pen"/> class:
    /// <table class="table is-hoverable"><thead><tr><th width="80%">Description</th><th width="20%">Image Example</th></tr></thead><tbody>
    /// <tr><td><see cref="LineJoin"/>: Specifies how to join the consecutive line segments. Can be <see cref="LineJoinStyle.Miter"/>, <see cref="LineJoinStyle.Bevel"/> or <see cref="LineJoinStyle.Round"/>.
    /// The example images demonstrate these join types from the top to the bottom. All examples use a 10 pixel wide pen.</td>
    /// <td><img src="../Help/Images/PenLineJoinStyleMiter.png" alt="Polygon drawn with JoinStyle = LineJoinStyle.Miter"/>
    /// <br/><img src="../Help/Images/PenLineJoinStyleBevel.png" alt="Polygon drawn with JoinStyle = LineJoinStyle.Bevel"/>
    /// <br/><img src="../Help/Images/PenLineJoinStyleRound.png" alt="Polygon drawn with JoinStyle = LineJoinStyle.Round"/></td></tr>
    /// <tr><td><see cref="StartCap"/> and <see cref="EndCap"/>: Specify the style of the start and end points of an open figure.
    /// Can be <see cref="LineCapStyle.Flat"/>, <see cref="LineCapStyle.Square"/>, <see cref="LineCapStyle.Round"/> or <see cref="LineCapStyle.Triangle"/>.
    /// The example images demonstrate these cap types from the top to the bottom, using the same cap style at both ends.
    /// Please note that the only difference between <see cref="LineCapStyle.Flat"/> and <see cref="LineCapStyle.Square"/> styles is that the <see cref="LineCapStyle.Flat"/> style
    /// has the originally specified length, whereas the <see cref="LineCapStyle.Square"/> style extends the line by half of the pen width. All examples use a 10 pixel wide pen.</td>
    /// <td><img src="../Help/Images/PenLineCapStyleFlat.png" alt="10 pixel width line drawn with cap style Flat"/>
    /// <br/><img src="../Help/Images/PenLineCapStyleSquare.png" alt="10 pixel width line drawn with cap style Square"/>
    /// <br/><img src="../Help/Images/PenLineCapStyleRound.png" alt="10 pixel width line drawn with cap style Round"/>
    /// <br/><img src="../Help/Images/PenLineCapStyleTriangle.png" alt="10 pixel width line drawn with cap style Triangle"/></td></tr>
    /// <tr><td><see cref="Brush"/>: It allows setting a <see cref="Shapes.Brush"/> explicitly, so the <see cref="Pen"/> can use not only a solid color but also a texture or any other brush.
    /// The example demonstrates a 10 pixel wide pen with a diagonal linear gradient brush using mirrored wrapping mode.</td>
    /// <td><img src="../Help/Images/PenWithBrush.png" alt="Pen with a linear gradient brush"/></td></tr>
    /// </tbody></table></para>
    /// </example>
    public sealed class Pen
    {
        #region Fields

        private Brush brush;
        private float width;
        private LineJoinStyle lineJoin;
        private float miterLimit = 10f;
        private LineCapStyle startCap;
        private LineCapStyle endCap;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the <see cref="Shapes.Brush"/> that determines the color or texture of the <see cref="Pen"/>.
        /// <br/>Default value: The <see cref="Shapes.Brush"/> instance that was either passed to the corresponding constructor, or was created implicitly from a color by the other constructors.
        /// <br/>See the <strong>Examples</strong> section of the <see cref="Pen"/> class for image examples.
        /// </summary>
        public Brush Brush
        {
            get => brush;
            set => brush = value ?? throw new ArgumentNullException(nameof(value), PublicResources.ArgumentNull);
        }

        /// <summary>
        /// Gets or sets the width of the <see cref="Pen"/>.
        /// <br/>Default value: the width that was passed to the constructor, or 1, if it was not specified.
        /// </summary>
        /// <remarks>
        /// <para>A with of 1/64 or less is not visible. If <see cref="DrawingOptions.FastThinLines"/> is <see langword="true"/> and <see cref="DrawingOptions.AntiAliasing"/> is <see langword="false"/>,
        /// then a width less than 1/4 is not visible.
        /// </para>
        /// </remarks>
        public float Width
        {
            get => width;
            set
            {
                if (value <= 0f)
                    throw new ArgumentOutOfRangeException(nameof(value), PublicResources.ArgumentMustBeGreaterThan(0f));
                width = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="LineJoinStyle"/> that specifies how to join the consecutive line segments.
        /// <br/>Default value: <see cref="LineJoinStyle.Miter"/>.
        /// <br/>See the <strong>Examples</strong> section of the <see cref="Pen"/> class for image examples.
        /// </summary>
        public LineJoinStyle LineJoin
        {
            get => lineJoin;
            set
            {
                if (!value.IsDefined())
                    throw new ArgumentOutOfRangeException(nameof(value), PublicResources.EnumOutOfRange(value));
                lineJoin = value;
            }
        }

        /// <summary>
        /// If the <see cref="LineJoin"/> is <see cref="LineJoinStyle.Miter"/>, then
        /// gets or sets the limit for the ratio of the miter length to half the <see cref="Width"/> that determines whether the join is beveled or mitered.
        /// Makes a difference only if the <see cref="Width"/> is greater than 1.
        /// <br/>Default value: 10.
        /// </summary>
        public float MiterLimit
        {
            get => miterLimit;
            set
            {
                if (value < 0f || Single.IsNaN(value))
                    throw new ArgumentOutOfRangeException(nameof(value), PublicResources.ArgumentMustBeGreaterThanOrEqualTo(0f));
                miterLimit = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="LineCapStyle"/> that specifies the style of the start point of an open figure.
        /// Makes a difference only if the <see cref="Width"/> is greater than 1.
        /// <br/>Default value: <see cref="LineCapStyle.Flat"/>.
        /// <br/>See the <strong>Examples</strong> section of the <see cref="Pen"/> class for image examples.
        /// </summary>
        public LineCapStyle StartCap
        {
            get => startCap;
            set
            {
                if (!value.IsDefined())
                    throw new ArgumentOutOfRangeException(nameof(value), PublicResources.EnumOutOfRange(value));
                startCap = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="LineCapStyle"/> that specifies the style of the end point of an open figure.
        /// Makes a difference only if the <see cref="Width"/> is greater than 1.
        /// <br/>Default value: <see cref="LineCapStyle.Flat"/>.
        /// <br/>See the <strong>Examples</strong> section of the <see cref="Pen"/> class for image examples.
        /// </summary>
        public LineCapStyle EndCap
        {
            get => endCap;
            set
            {
                if (!value.IsDefined())
                    throw new ArgumentOutOfRangeException(nameof(value), PublicResources.EnumOutOfRange(value));
                endCap = value;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Pen"/> class with a black color and a width of 1.
        /// </summary>
        public Pen() : this(Color32.Black)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Pen"/> class with the specified <paramref name="color"/> and <paramref name="width"/>.
        /// </summary>
        /// <param name="color">The color of the <see cref="Pen"/>.</param>
        /// <param name="width">The width of the <see cref="Pen"/>. Must be greater than 0. This parameter is optional.
        /// <br/>Default value: 1.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="width"/> is not greater than 0.</exception>
        /// <remarks>
        /// <para>A width of 1/64 or less is not visible. If <see cref="DrawingOptions.FastThinLines"/> is <see langword="true"/> and <see cref="DrawingOptions.AntiAliasing"/> is <see langword="false"/>,
        /// then a width less than 1/4 is not visible.</para>
        /// <para>If the color depth of a <see cref="Color32"/> instance is not enough to represent the specified <paramref name="color"/>,
        /// then create a solid brush with the desired color depth and use the <see cref="Pen(Shapes.Brush,float)"/> constructor instead.</para>
        /// </remarks>
        public Pen(Color32 color, float width = 1f)
        {
            if (width <= 0f || Single.IsNaN(width))
                throw new ArgumentOutOfRangeException(nameof(width), PublicResources.ArgumentMustBeGreaterThan(0f));
            this.width = width;
            brush = new SolidBrush(color);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Pen"/> class with the specified <paramref name="brush"/> and <paramref name="width"/>.
        /// </summary>
        /// <param name="brush">The <see cref="Shapes.Brush"/> that determines the color or texture of the <see cref="Pen"/>.</param>
        /// <param name="width">The width of the <see cref="Pen"/>. Must be greater than 0. This parameter is optional.
        /// <br/>Default value: 1.</param>
        /// <exception cref="ArgumentNullException"><paramref name="brush"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="width"/> is not greater than 0.</exception>
        /// <remarks>
        /// <para>A width of 1/64 or less is not visible. If <see cref="DrawingOptions.FastThinLines"/> is <see langword="true"/> and <see cref="DrawingOptions.AntiAliasing"/> is <see langword="false"/>,
        /// then a width less than 1/4 is not visible.</para>
        /// </remarks>
        public Pen(Brush brush, float width = 1f)
        {
            this.brush = brush ?? throw new ArgumentNullException(nameof(brush), PublicResources.ArgumentNull);

            if (width <= 0f || Single.IsNaN(width))
                throw new ArgumentOutOfRangeException(nameof(width), PublicResources.ArgumentMustBeGreaterThan(0f));
            this.width = width;
        }

        #endregion

        #region Methods

        internal bool DrawPath(IAsyncContext context, IReadWriteBitmapData bitmapData, Path path, DrawingOptions drawingOptions)
        {
            if (Width <= Constants.PointEqualityTolerance)
                return !context.IsCancellationRequested;

            // special handling for thin paths: not generating a new path but drawing the raw lines of rawPath
            if (!drawingOptions.AntiAliasing && drawingOptions.FastThinLines && Width <= 1f)
                return Width >= 0.25f ? brush.DrawThinPath(context, bitmapData, path, drawingOptions, path.PreferCaching) : !context.IsCancellationRequested;

            RawPath rawPath = path.RawPath;
            RawPath widePath = path.PreferCaching ? rawPath.GetCreateWidePath(this, drawingOptions) : rawPath.WidenPath(this, drawingOptions);
            return brush.FillRawPath(context, bitmapData, widePath, drawingOptions.WithNonZeroFill, path.PreferCaching);
        }

        #endregion
    }
}