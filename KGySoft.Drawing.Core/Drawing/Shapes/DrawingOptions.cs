#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: DrawingOptions.cs
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

using KGySoft.CoreLibraries;
using KGySoft.Drawing.Imaging;

#endregion

namespace KGySoft.Drawing.Shapes
{
    /// <summary>
    /// Provides options for drawing and filling shapes. See the description of the properties for details and examples.
    /// The default options (which is also used when <see langword="null"/> is passed to the drawing methods) uses alpha blending but no anti-aliasing,
    /// and uses fast shape-filling and thin path drawing strategies.
    /// </summary>
    public sealed class DrawingOptions : IEquatable<DrawingOptions>
    {
        #region Fields

        #region Static Fields

        #region Internal Fields
        
        internal static readonly DrawingOptions Default = new DrawingOptions();

        #endregion

        #region Privte Fields

        private static readonly DrawingOptions defaultNonZero = new DrawingOptions { FillMode = ShapeFillMode.NonZero };

        #endregion

        #endregion

        #region Instance Fields

        private TransformationMatrix transformation;
        private ShapeFillMode fillMode;
        private PixelOffset scanPathPixelOffset = PixelOffset.Half;
        private PixelOffset drawPathPixelOffset = PixelOffset.None;

        #endregion

        #endregion

        #region Properties

        #region Public Properties

        /// <summary>
        /// Gets or sets the transformation matrix to apply when drawing shapes.
        /// <br/>Default value: <see cref="TransformationMatrix.Identity">TransformationMatrix.Identity</see>.
        /// </summary>
        /// <remarks>
        /// <para>This property allows applying transformations (e.g. translation, rotation, zoom, etc.) when drawing shapes.
        /// It can be particularly useful when drawing shapes directly, without creating a <see cref="Path"/> instance.
        /// For example, the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.DrawEllipse"/> methods don't offer a parameter for rotation.</para>
        /// <para>Setting this property to a value other than the identity matrix disables path region caching for <see cref="Path"/> instances,
        /// even if <see cref="Path.PreferCaching">Path.PreferCaching</see> is <see langword="true"/>. To achieve the best performance
        /// when drawing the same shape with the same transformation multiple times, use a <see cref="Path"/> instance with caching enabled,
        /// apply the transformation to the <see cref="Path"/>, and use the identity matrix here.</para>
        /// </remarks>
        /// <example>
        /// The following example demonstrates how to draw a rotated ellipse directly without creating a <see cref="Path"/> instance:
        /// <code lang="C#"><![CDATA[
        /// using IReadWriteBitmapData bmp = BitmapDataFactory.CreateBitmapData(100, 100);
        /// bmp.Clear(Color.Cyan);
        ///
        /// // Creating a rotation transformation matrix from the center of the ellipse.
        /// var tr = TransformationMatrix.CreateRotation(45f * MathF.PI / 180f, new PointF(50f, 50f));
        /// var options = new DrawingOptions { AntiAliasing = true, Transformation = tr };
        ///
        /// // Drawing the ellipse with the transformation matrix.
        /// bmp.DrawEllipse(Color.Blue, 0, 25, 100, 50, options);]]></code>
        /// <para>The example above produces the following result:
        /// <br/><img src="../Help/Images/DrawingOptionsTransformation.png" alt="Ellipse drawn with a 45 degree rotation"/></para>
        /// </example>
        public TransformationMatrix Transformation
        {
            get => transformation;
            set => transformation = value;
        }

        /// <summary>
        /// Gets or sets whether anti-aliasing is enabled when drawing shapes.
        /// <br/>Default value: <see langword="false"/>.
        /// </summary>
        /// <remarks>
        /// <para>When anti-aliasing is enabled, the shapes are drawn with smooth edges, which affects both shape filling and path drawing.</para>
        /// <para>Anti-aliasing uses 16x16 subpixels per pixel to achieve smooth edges. This means that anti-aliased drawing is slower than aliased drawing.</para>
        /// <para>The following images provide a few examples regarding anti-aliasing:
        /// <table class="table is-hoverable"><thead><tr><th width="80%">Description</th><th width="20%">Image Example</th></tr></thead><tbody>
        /// <tr><td>Filling a polygon with <c><see cref="AntiAliasing"/> = <see langword="false"/></c>.</td>
        /// <td><img src="../Help/Images/DrawingOptionsFillModeAlternate.png" alt="Polygon fill with AntiAliasing = false."/></td></tr>
        /// <tr><td>Filling a polygon with <c><see cref="AntiAliasing"/> = <see langword="true"/></c>.</td>
        /// <td><img src="../Help/Images/DrawingOptionsFillModeAlternateAntiAliasing.png" alt="Polygon fill with AntiAliasing = true."/></td></tr>
        /// <tr><td>Drawing a <see cref="Path"/> with <c><see cref="AntiAliasing"/> = <see langword="false"/></c>, <c><see cref="FastThinLines"/> = <see langword="true"/></c>. This is the default configuration of these properties.</td>
        /// <td><img src="../Help/Images/DrawingOptionsFastThinLinesEnabled.png" alt="Path drawn with AntiAliasing = false, FastThinLines = true."/></td></tr>
        /// <tr><td>Drawing a <see cref="Path"/> with <c><see cref="AntiAliasing"/> = <see langword="false"/></c>, <c><see cref="FastThinLines"/> = <see langword="false"/></c>,
        /// <c><see cref="DrawPathPixelOffset"/> = <see cref="PixelOffset.Half"/></c>. Note that the lines are more jagged than above.</td>
        /// <td><img src="../Help/Images/DrawingOptionsFastThinLinesDisabled.png" alt="Path drawn with AntiAliasing = false, FastThinLines = false, DrawPathPixelOffset = PixelOffset.Half."/></td></tr>
        /// <tr><td>Drawing a <see cref="Path"/> with <c><see cref="AntiAliasing"/> = <see langword="true"/></c>, <c><see cref="DrawPathPixelOffset"/> = <see cref="PixelOffset.Half"/></c>. The lines are much smoother.</td>
        /// <td><img src="../Help/Images/DrawingOptionsAntiAliasingEnabled.png" alt="Path drawn with AntiAliasing = true, DrawPathPixelOffset = PixelOffset.Half."/></td></tr>
        /// <tr><td>Drawing an ellipse with <c><see cref="AntiAliasing"/> = <see langword="false"/></c>, <c><see cref="FastThinLines"/> = <see langword="true"/></c>. The image is zoomed in for better visibility.</td>
        /// <td><img src="../Help/Images/DrawingOptionsFastThinLinesEnabledZoomed.png" alt="Zoomed ellipse with AntiAliasing = false, FastThinLines = true."/></td></tr>
        /// <tr><td>Drawing an ellipse with <c><see cref="AntiAliasing"/> = <see langword="false"/></c>, <c><see cref="FastThinLines"/> = <see langword="false"/></c>,
        /// <c><see cref="DrawPathPixelOffset"/> = <see cref="PixelOffset.Half"/></c>. The image is zoomed in for better visibility.</td>
        /// <td><img src="../Help/Images/DrawingOptionsFastThinLinesDisabledZoomed.png" alt="Zoomed ellipse with AntiAliasing = false, FastThinLines = false, DrawPathPixelOffset = PixelOffset.Half."/></td></tr>
        /// <tr><td>Drawing an ellipse with <c><see cref="AntiAliasing"/> = <see langword="true"/></c>, <c><see cref="DrawPathPixelOffset"/> = <see cref="PixelOffset.Half"/></c>. The image is zoomed in for better visibility.</td>
        /// <td><img src="../Help/Images/DrawingOptionsAntiAliasingEnabledZoomed.png" alt="Zoomed ellipse with AntiAliasing = true."/></td></tr>
        /// </tbody></table></para>
        /// </remarks>
        public bool AntiAliasing { get; set; }

        /// <summary>
        /// Gets or sets whether drawing <see cref="Path"/>s and primitive shapes with thin lines are drawn with a faster algorithm when <see cref="AntiAliasing"/> is <see langword="false"/>.
        /// <br/>Default value: <see langword="true"/>.
        /// </summary>
        /// <remarks>
        /// <para>When <see cref="FastThinLines"/> is <see langword="true"/>, thin lines (that is, when <see cref="Pen.Width">Pen.Width</see> is less or equal to 1) and arcs
        /// are drawn with a Bresenham-like algorithm, which is both faster and produces better results than the default algorithm.</para>
        /// <para>This property has no effect when <see cref="AntiAliasing"/> is <see langword="true"/>.</para>
        /// <para>The following images provide a few examples:
        /// <table class="table is-hoverable"><thead><tr><th width="80%">Description</th><th width="20%">Image Example</th></tr></thead><tbody>
        /// <tr><td>Drawing a <see cref="Path"/> with <c><see cref="AntiAliasing"/> = <see langword="false"/></c>, <c><see cref="FastThinLines"/> = <see langword="true"/></c>. This is the default configuration of these properties.</td>
        /// <td><img src="../Help/Images/DrawingOptionsFastThinLinesEnabled.png" alt="Path drawn with AntiAliasing = false, FastThinLines = true."/></td></tr>
        /// <tr><td>Drawing a <see cref="Path"/> with <c><see cref="AntiAliasing"/> = <see langword="false"/></c>, <c><see cref="FastThinLines"/> = <see langword="false"/></c>,
        /// <c><see cref="DrawPathPixelOffset"/> = <see cref="PixelOffset.Half"/></c>. Note that the lines are more jagged than above.</td>
        /// <td><img src="../Help/Images/DrawingOptionsFastThinLinesDisabled.png" alt="Path drawn with AntiAliasing = false, FastThinLines = false, DrawPathPixelOffset = PixelOffset.Half."/></td></tr>
        /// <tr><td>Drawing a <see cref="Path"/> with <c><see cref="AntiAliasing"/> = <see langword="true"/></c>, <c><see cref="DrawPathPixelOffset"/> = <see cref="PixelOffset.Half"/></c>. The lines are much smoother.</td>
        /// <td><img src="../Help/Images/DrawingOptionsAntiAliasingEnabled.png" alt="Path drawn with AntiAliasing = true, DrawPathPixelOffset = PixelOffset.Half."/></td></tr>
        /// <tr><td>Drawing an ellipse with <c><see cref="AntiAliasing"/> = <see langword="false"/></c>, <c><see cref="FastThinLines"/> = <see langword="true"/></c>. The image is zoomed in for better visibility.</td>
        /// <td><img src="../Help/Images/DrawingOptionsFastThinLinesEnabledZoomed.png" alt="Zoomed ellipse with AntiAliasing = false, FastThinLines = true."/></td></tr>
        /// <tr><td>Drawing an ellipse with <c><see cref="AntiAliasing"/> = <see langword="false"/></c>, <c><see cref="FastThinLines"/> = <see langword="false"/></c>,
        /// <c><see cref="DrawPathPixelOffset"/> = <see cref="PixelOffset.Half"/></c>. The image is zoomed in for better visibility.</td>
        /// <td><img src="../Help/Images/DrawingOptionsFastThinLinesDisabledZoomed.png" alt="Zoomed ellipse with AntiAliasing = false, FastThinLines = false, DrawPathPixelOffset = PixelOffset.Half."/></td></tr>
        /// </tbody></table></para>
        /// <para>Though enabling <see cref="FastThinLines"/> usually produces better results, sometimes it still may be beneficial to disable it. For example,
        /// when filling and drawing the same <see cref="Path"/> without anti-aliasing, in some cases the 1-pixel width outline may not be perfectly aligned with the fill area, unless <see cref="FastThinLines"/> is disabled:
        /// <table class="table is-hoverable"><thead><tr><th width="80%">Description</th><th width="20%">Image Example</th></tr></thead><tbody>
        /// <tr><td>Filling and drawing the same polygon with <c><see cref="AntiAliasing"/> = <see langword="false"/></c>, <c><see cref="FastThinLines"/> = <see langword="true"/></c>. You can observe alignment issues.</td>
        /// <td><img src="../Help/Images/DrawingOptionsFillWithFastThinLinesEnabled.png" alt="Polygon fill + draw with AntiAliasing = false, FastThinLines = true."/></td></tr>
        /// <tr><td>Filling and drawing the same polygon with <c><see cref="AntiAliasing"/> = <see langword="false"/></c>, <c><see cref="FastThinLines"/> = <see langword="false"/></c>. Now there is no alignment issue</td>
        /// <td><img src="../Help/Images/DrawingOptionsFillWithFastThinLinesDisabled.png" alt="Polygon fill + draw with AntiAliasing = false, FastThinLines = false."/></td></tr>
        /// </tbody></table></para>
        /// </remarks>
        public bool FastThinLines { get; set; }

        /// <summary>
        /// Gets or sets whether alpha blending is enabled when drawing shapes.
        /// <br/>Default value: <see langword="true"/>.
        /// </summary>
        /// <remarks>
        /// <para>When this property is <see langword="true"/>, the alpha channel of the colors is blended with the target image.
        /// The used color space for blending is the target's <see cref="IBitmapData.WorkingColorSpace"/>, or the one that the <see cref="Quantizer"/> uses if specified.</para>
        /// <para>Alpha blending may be required in two cases: when the used <see cref="Brush"/> has transparent colors, or when the <see cref="AntiAliasing"/> property is <see langword="true"/>.</para>
        /// <para>If this property is <see langword="false"/>, the alpha channel is written directly to the target image. This is usually not desired with anti-aliased shapes, except for already transparent background images,
        /// because it may produce alpha pixels along the edges of the shapes, even when the background is fully opaque.</para>
        /// <para>When <see cref="AlphaBlending"/> is <see langword="true"/>, drawing or filling a shape with a completely transparent <see cref="Brush"/> will not affect the target image,
        /// whereas when it is <see langword="false"/>, a transparent <see cref="Brush"/> can be used to draw transparent shapes.</para>
        /// <para>The following images provide a few examples regarding alpha blending:
        /// <table class="table is-hoverable"><thead><tr><th width="80%">Description</th><th width="20%">Image Example</th></tr></thead><tbody>
        /// <tr><td>Filling a polygon with <c><see cref="AntiAliasing"/> = <see langword="false"/></c>, <c><see cref="AlphaBlending"/> = <see langword="false"/></c>,
        /// using a solid brush with 50% transparency on an alpha gradient background. Note that no blending occurs, so the polygon is just filled with the specified alpha color.</td>
        /// <td><img src="../Help/Images/DrawingOptionsAlphaBlendingDisabledNoAA.png" alt="Polygon filled with 50% transparency, AntiAliasing = false, AlphaBlending = false."/></td></tr>
        /// <tr><td>Filling a polygon with <c><see cref="AntiAliasing"/> = <see langword="false"/></c>, <c><see cref="AlphaBlending"/> = <see langword="true"/></c>,
        /// using a solid brush with 50% transparency on an alpha gradient background. The blending uses the color space of the target <see cref="IReadWriteBitmapData"/>, which is linear in this case.</td>
        /// <td><img src="../Help/Images/DrawingOptionsAlphaBlendingEnabledNoAA.png" alt="Polygon filled with 50% transparency, AntiAliasing = false, AlphaBlending = true."/></td></tr>
        /// <tr><td>Filling a polygon with <c><see cref="AlphaBlending"/> = <see langword="false"/></c>, using a completely transparent brush.
        /// Note that this way we can "cut" transparent "holes" into bitmap. If the target does not support transparency, the shape will be filled with the <see cref="IBitmapData.BackColor"/> color of the target bitmap data.</td>
        /// <td><img src="../Help/Images/DrawingOptionsAlphaBlendingDisabledTr.png" alt="Polygon filled with transparency, AlphaBlending = false."/></td></tr>
        /// <tr><td>Filling a polygon with <c><see cref="AntiAliasing"/> = <see langword="true"/></c>, <c><see cref="AlphaBlending"/> = <see langword="false"/></c>.
        /// When anti-aliasing is enabled, it's not recommended to turn off alpha blending (unless the background is transparent), because it may produce alpha pixels along the edges of the shapes, even when the background is fully opaque.</td>
        /// <td><img src="../Help/Images/DrawingOptionsAlphaBlendingDisabledAA.png" alt="filling a polygin with AntiAliasing = true, AlphaBlending = false."/></td></tr>
        /// <tr><td>Filling a polygon with <c><see cref="AntiAliasing"/> = <see langword="true"/></c>, <c><see cref="AlphaBlending"/> = <see langword="true"/></c>.</td>
        /// <td><img src="../Help/Images/DrawingOptionsFillModeAlternateAntiAliasing.png" alt="Filling a polygin with AntiAliasing = true, AlphaBlending = true."/></td></tr>
        /// </tbody></table></para>
        /// </remarks>
        public bool AlphaBlending { get; set; }

        /// <summary>
        /// Gets or sets the fill mode to use when filling shapes.
        /// <br/>Default value: <see cref="ShapeFillMode.Alternate"/>.
        /// </summary>
        /// <remarks>
        /// <para>If a polygon has no self-crossing lines, then both fill modes produce the same result, in which case the default <see cref="ShapeFillMode.Alternate"/> mode can be a better choice because it is faster.</para>
        /// <para>The following examples demonstrate the difference between the two fill modes:
        /// <table class="table is-hoverable"><thead><tr><th width="80%">Description</th><th width="20%">Image Example</th></tr></thead><tbody>
        /// <tr><td><c><see cref="FillMode"/> = <see cref="ShapeFillMode.Alternate">ShapeFillMode.Alternate</see></c> (default): When scanning the region of a polygon to be filled,
        /// a point is considered to be the part of the polygon if the scanline crosses odd number of lines before reaching the point to be drawn,
        /// and is considered not to be the part of the polygon if the scanline crosses even number of lines. This strategy is faster than the <see cref="ShapeFillMode.NonZero"/> mode,
        /// though it may produce "holes" when a polygon has self-crossing lines.</td>
        /// <td><img src="../Help/Images/DrawingOptionsFillModeAlternateAntiAliasing.png" alt="FillMode = ShapeFillMode.Alternate"/></td></tr>
        /// <tr><td><c><see cref="FillMode"/> = <see cref="ShapeFillMode.NonZero">ShapeFillMode.NonZero</see></c>: It considers the direction of the path segments at each intersection,
        /// adding/subtracting one at every clockwise/counterclockwise intersection. The point is considered to be the part of the polygon if the sum is not zero.</td>
        /// <td><img src="../Help/Images/DrawingOptionsFillModeNonZero.png" alt="FillMode = ShapeFillMode.NonZero"/></td></tr>
        /// </tbody></table></para>
        /// </remarks>
        public ShapeFillMode FillMode
        {
            get => fillMode;
            set
            {
                if (!value.IsDefined())
                    throw new ArgumentOutOfRangeException(PublicResources.EnumOutOfRange(value));
                fillMode = value;
            }
        }

        // TODO: add images
        public PixelOffset ScanPathPixelOffset
        {
            get => scanPathPixelOffset;
            set
            {
                if (!value.IsDefined())
                    throw new ArgumentOutOfRangeException(PublicResources.EnumOutOfRange(value));
                scanPathPixelOffset = value;
            }
        }

        public PixelOffset DrawPathPixelOffset
        {
            get => drawPathPixelOffset;
            set
            {
                if (!value.IsDefined())
                    throw new ArgumentOutOfRangeException(PublicResources.EnumOutOfRange(value));
                drawPathPixelOffset = value;
            }
        }

        public IQuantizer? Quantizer { get; set; }

        public IDitherer? Ditherer { get; set; }

        /// <summary>
        /// Gets or sets the maximum pixel size to cache the region of a <see cref="Path"/> instance.
        /// If the region has more pixels than this value, it will be re-scanned in each drawing session.
        /// <br/>Default value: 16777216, which is 16 MB for anti-aliased regions or 2 MB for aliased regions.
        /// </summary>
        public long CacheRegionLimit { get; set; } = 1L << 24; // 16 MB

        #endregion

        #region Internal Properties

        /// <summary>
        /// To avoid the callers use options.Transformation.IsIdentity, which would copy the matrix.
        /// </summary>
        internal bool IsIdentityTransform => transformation.IsIdentity;

        internal DrawingOptions WithNonZeroFill => FillMode is ShapeFillMode.NonZero ? this
            : Equals(Default) ? defaultNonZero
            : new DrawingOptions(this) { FillMode = ShapeFillMode.NonZero };

        #endregion

        #endregion

        #region Constructors

        #region Public Constructors

        public DrawingOptions()
        {
            transformation = TransformationMatrix.Identity;
            AlphaBlending = true;
            FastThinLines = true;
        }

        #endregion
        
        #region Private Constructors

        private DrawingOptions(DrawingOptions other)
        {
            transformation = other.transformation;
            AntiAliasing = other.AntiAliasing;
            AlphaBlending = other.AlphaBlending;
            FillMode = other.FillMode;
            scanPathPixelOffset = other.scanPathPixelOffset;
            drawPathPixelOffset = other.drawPathPixelOffset;
            Quantizer = other.Quantizer;
            Ditherer = other.Ditherer;
        }

        #endregion

        #endregion

        #region Methods

        public bool Equals(DrawingOptions? other)
        {
            if (ReferenceEquals(other, null))
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return Transformation == other.Transformation
                && AntiAliasing == other.AntiAliasing
                && AlphaBlending == other.AlphaBlending
                && FillMode == other.FillMode
                && ScanPathPixelOffset == other.ScanPathPixelOffset
                && DrawPathPixelOffset == other.DrawPathPixelOffset
                && Quantizer == other.Quantizer
                && Ditherer == other.Ditherer
                && CacheRegionLimit == other.CacheRegionLimit;
        }

        public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is DrawingOptions other && Equals(other);

        // CacheRegionLimit is not in hash code, which is intended
        public override int GetHashCode() => (Transformation, Quantizer, Ditherer,
                Convert.ToInt32(AntiAliasing)
                | Convert.ToInt32(AlphaBlending) << 1
                | (int)FillMode << 2
                | (int)ScanPathPixelOffset << 3
                | (int)DrawPathPixelOffset << 4)
            .GetHashCode();

        #endregion
    }
}
