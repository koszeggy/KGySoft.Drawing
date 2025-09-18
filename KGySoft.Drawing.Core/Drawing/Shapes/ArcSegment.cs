#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ArcSegment.cs
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
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;

using KGySoft.CoreLibraries;

#endregion

namespace KGySoft.Drawing.Shapes
{
    // NOTE: It would not be necessary to have a separate ArcSegment class because it could be represented by a BezierSegment,
    // but for drawing thin paths, the flatten Bézier curve drawing is both slower and less symmetric.
    /// <summary>
    /// Represents a non-rotated elliptical arc segment in a <see cref="Figure"/> (or a complete ellipse, if <see cref="SweepAngle"/> is 360).
    /// </summary>
    public sealed class ArcSegment : PathSegment
    {
        #region Constants

        internal const int SectorNotDrawn = 0;
        internal const int SectorFullyDrawn = 1;
        internal const int SectorStart = 2;
        internal const int SectorEnd = 3;
        internal const int SectorStartEnd = 4;

        /// <summary>
        /// The maximum diameter for Bresenham ellipse drawing. Above this threshold, it's faster to draw the ellipse as flattened Bézier curves.
        /// Also, as lines, no overflow will occur for big radii.
        /// </summary>
        internal const int DrawAsLinesThreshold = 1 << 16;

        #endregion

        #region Fields

        #region Static Fields

        private static readonly BitVector32.Section sector0 = BitVector32.CreateSection(SectorStartEnd);
        private static readonly BitVector32.Section sector1 = BitVector32.CreateSection(SectorStartEnd, sector0);
        private static readonly BitVector32.Section sector2 = BitVector32.CreateSection(SectorStartEnd, sector1);
        private static readonly BitVector32.Section sector3 = BitVector32.CreateSection(SectorStartEnd, sector2);

        internal static readonly BitVector32.Section[] Sectors = [sector0, sector1, sector2, sector3];

        #endregion

        #region Instance fields

        // Supporting only translate/scale transformations, and converting to Bézier curves otherwise.
        // Since ArcSegment is public now, we will not even add rotation support because it would be a breaking change.
        // Storing both degrees and radians because degrees are more accurate when detecting used sectors for drawing.
        private PointF center;
        private float radiusX;
        private float radiusY;
        private readonly float startAngle;
        private readonly float sweepAngle;
        private readonly float startAngleRadian;
        private readonly float sweepAngleRadian;
        private BitVector32? sectors;
        private PointF? startPoint;
        private PointF? endPoint;

        private List<PointF>? flattenedPoints;

        #endregion

        #endregion

        #region Properties

        #region Public Properties

        /// <summary>
        /// Gets the center point of this <see cref="ArcSegment"/>.
        /// </summary>
        public PointF Center => center;

        /// <summary>
        /// Gets the horizontal radius of this <see cref="ArcSegment"/>.
        /// </summary>
        public float RadiusX => radiusX;

        /// <summary>
        /// Gets the vertical radius of this <see cref="ArcSegment"/>.
        /// </summary>
        public float RadiusY => radiusY;

        /// <summary>
        /// Gets the start angle of this <see cref="ArcSegment"/> in degrees. The angle is measured clockwise from the x-axis.
        /// </summary>
        public float StartAngle => startAngle;

        /// <summary>
        /// Gets the sweep angle of this <see cref="ArcSegment"/> in degrees. A positive value indicates a clockwise sweep; a negative value indicates a counterclockwise sweep.
        /// For a full ellipse, this property always returns +360.
        /// </summary>
        public float SweepAngle => sweepAngle;

        /// <summary>
        /// Gets the start point of this <see cref="ArcSegment"/>.
        /// </summary>
        public override PointF StartPoint
        {
            get
            {
                EnsureStartEndPoints();
                return startPoint!.Value;
            }
        }

        /// <summary>
        /// Gets the end point of this <see cref="ArcSegment"/>.
        /// </summary>
        public override PointF EndPoint
        {
            get
            {
                EnsureStartEndPoints();
                return endPoint!.Value;
            }
        }

        /// <summary>
        /// Gets the bounding rectangle of this <see cref="ArcSegment"/>.
        /// </summary>
        public RectangleF Bounds => new RectangleF(center.X - radiusX, center.Y - radiusY, radiusX * 2f, radiusY * 2f);

        #endregion

        #region Internal Properties

        internal float Width => radiusX * 2f;
        internal float Height => radiusY * 2f;
        
        #endregion

        #endregion

        #region Constructors
        
        #region Internal Constructors

        internal ArcSegment(RectangleF bounds, float startAngle = 0f, float sweepAngle = 360f)
        {
            // Storing the center and radii rather than bounds, so transformations can be applied easily.
            radiusX = bounds.Width / 2f;
            radiusY = bounds.Height / 2f;
            center = new PointF(bounds.X + radiusX, bounds.Y + radiusY);
            radiusX = Math.Abs(radiusX);
            radiusY = Math.Abs(radiusY);
            NormalizeAngle(ref startAngle);
            this.startAngle = startAngle;
            if (Math.Abs(sweepAngle) >= 360f)
                sweepAngle = 360f;
            this.sweepAngle = sweepAngle;
            startAngleRadian = startAngle.ToRadian();
            sweepAngleRadian = sweepAngle.ToRadian();
        }

        #endregion

        #region Private Constructors

        private ArcSegment(ArcSegment other)
        {
            center = other.center;
            radiusX = other.radiusX;
            radiusY = other.radiusY;
            startAngle = other.startAngle;
            sweepAngle = other.sweepAngle;
            startAngleRadian = other.startAngleRadian;
            sweepAngleRadian = other.sweepAngleRadian;
        }

        #endregion

        #endregion

        #region Methods

        #region Static Methods
        
        #region Internal Methods

        internal static void NormalizeAngles(ref float startAngle, ref float sweepAngle)
        {
            if (sweepAngle < 0)
            {
                startAngle += sweepAngle;
                sweepAngle = -sweepAngle;
            }

            NormalizeAngle(ref startAngle);
        }

        // Adjusts the start/end angles for the radii of an ellipse. This is how also GDI+ calculates the start/end points of arcs.
        // The formula is taken from libgdiplus: https://github.com/mono/libgdiplus/blob/94a49875487e296376f209fe64b921c6020f74c0/src/graphics-path.c#L752
        [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator", Justification = "Intended")]
        internal static void AdjustAngles(ref float startRad, ref float endRad, float radiusX, float radiusY)
        {
            if (radiusX == radiusY)
                return;

            bool isPositive = endRad >= startRad;

            // Ensuring nonzero parameters for the Atan2 calculation
            if (radiusX.TolerantIsZero(Constants.ZeroTolerance))
                radiusX = radiusX >= 0f && !radiusX.IsNegativeZero() ? Constants.ZeroTolerance : -Constants.ZeroTolerance;
            if (radiusY.TolerantIsZero(Constants.ZeroTolerance))
                radiusY = radiusY >= 0f && !radiusY.IsNegativeZero() ? Constants.ZeroTolerance : -Constants.ZeroTolerance;

            startRad = MathF.Atan2(radiusX * MathF.Sin(startRad), radiusY * MathF.Cos(startRad));
            endRad = MathF.Atan2(radiusX * MathF.Sin(endRad), radiusY * MathF.Cos(endRad));

            // preventing swapping the direction
            if (endRad >= startRad != isPositive)
            {
                if (endRad > startRad)
                    endRad -= 2f * MathF.PI;
                else
                    startRad -= 2f * MathF.PI;
            }
        }

        // Returns the sectors where the arc should be drawn. The idea is taken from https://www.scattergood.io/arc-drawing-algorithm/
        // It prevents the Atan2 calculation for each point of the arc.
        // The original circle drawing code uses 8 sectors but when generalizing it for ellipses we should use 4.
        [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator", Justification = "That's one of the reasons why we use degrees instead of radians here.")]
        internal static BitVector32 GetSectors(float startAngle, float sweepAngle)
        {
            NormalizeAngles(ref startAngle, ref sweepAngle);

            // For NaN an OverflowException is thrown at the checked block; otherwise, we expect normalized angles here.
            Debug.Assert(startAngle is Single.NaN or >= 0f and <= 360f && sweepAngle is Single.NaN or >= 0f and < 360f);

            // The original code uses an array here but by using BitVector32 we can store the sectors in a single int, avoiding array allocation.
            // Angles are not checked in the public methods, so this is a good place to do it because this method is called in the moment of drawing.
            var result = new BitVector32();
            int startSector = checked((int)(startAngle / 90));
            float endAngle = startAngle + sweepAngle;
            if (endAngle >= 360f)
                endAngle -= 360f;
            int endSector = checked((int)(endAngle / 90));

            if (startSector == endSector)
                result[Sectors[startSector]] = SectorStartEnd;
            else
            {
                result[Sectors[startSector]] = startAngle == startSector * 90f ? SectorFullyDrawn : SectorStart;
                for (int i = (startSector + 1) & 3; i != endSector; i = (i + 1) & 3)
                    result[Sectors[i]] = SectorFullyDrawn;
                result[Sectors[endSector]] = endAngle == endSector * 90f ? SectorNotDrawn : SectorEnd;
            }

            return result;
        }

        #endregion

        #region Private Methods

        private static void NormalizeAngle(ref float startAngle)
        {
            startAngle = startAngle is >= 0f and <= 360f ? startAngle : startAngle % 360f;
            if (startAngle < 0)
                startAngle += 360f;
        }

        #endregion

        #endregion

        #region Instance Methods

        #region Public Methods

        /// <summary>
        /// Gets the points of this <see cref="ArcSegment"/> as a collection of Bézier points.
        /// </summary>
        /// <returns>The points of this <see cref="ArcSegment"/> as a collection of Bézier points.</returns>
        /// <remarks>
        /// <para>The result has 1 + 3n points, where n is the number of cubic Bézier curves in this segment.
        /// As an <see cref="ArcSegment"/> with nonzero <see cref="SweepAngle"/> can be approximated by 1 to 4 Bézier curves, the result normally contains 4, 7, 10 or 13 points.</para>
        /// <para>If <see cref="SweepAngle"/> is zero, the result contains a single point, which is equal to <see cref="StartPoint"/>.</para>
        /// <para>The result of this method can be used as a valid parameter for the <see cref="Path.AddBeziers(IEnumerable{PointF})">Path.AddBeziers</see> method.</para>
        /// </remarks>
        public IList<PointF> ToBezierPoints()
        {
            // Arc, or a full ellipse with nonzero start angle
            if (sweepAngle < 360f || startAngleRadian is not 0f) // This check is alright, a full ellipse always has +360 degrees sweep angle
                return sweepAngleRadian is 0f // not using TolerantZero because for very large radii the result can be more than just a single point
                    ? new[] { StartPoint }
                    : BezierSegment.FromArc(center, radiusX, radiusY, startAngleRadian, sweepAngleRadian).PointsInternal;

            // Full ellipse with zero start angle: simple conversion to Bézier curves
            return BezierSegment.FromEllipse(center, radiusX, radiusY).PointsInternal;
        }

        #endregion

        #region Internal Methods

        internal override List<PointF> GetFlattenedPointsInternal()
        {
            if (flattenedPoints is List<PointF> result)
                return result;

            // Arc, or a full ellipse with nonzero start angle
            if (sweepAngle < 360f || startAngleRadian is not 0f) // This check is alright, a full ellipse always has +360 degrees sweep angle
                return flattenedPoints = sweepAngleRadian is 0f // not using TolerantZero because for very large radii the result can be more than just a single point
                    ? [StartPoint]
                    : BezierSegment.FromArc(center, radiusX, radiusY, startAngleRadian, sweepAngleRadian).GetFlattenedPointsInternal();

            // Full ellipse with zero start angle: simple conversion to Bézier curves
            return flattenedPoints = BezierSegment.FromEllipse(center, radiusX, radiusY).GetFlattenedPointsInternal();
        }

        internal override PathSegment Transform(TransformationMatrix matrix)
        {
            Debug.Assert(!matrix.IsIdentity);

            // If the transformation is translation or scale (including reflections) only, we can transform the arc directly.
            // See https://en.wikipedia.org/wiki/Transformation_matrix#/media/File:2D_affine_transformation_matrix.svg
            if (matrix is { M12: 0f, M21: 0f })
            {
                startPoint = null;
                endPoint = null;
                flattenedPoints = null;

                center = center.Transform(matrix);
                radiusX *= matrix.M11;
                radiusY *= matrix.M22;
                return this;
            }

            // Otherwise, converting the arc to a Bézier curve (or to a line if it's flat) and transforming that
            return (radiusX is 0f || radiusY is 0f ? new LineSegment(GetFlattenedPointsInternal())
                    : sweepAngle < 360f || startAngleRadian is not 0f ? BezierSegment.FromArc(center, radiusX, radiusY, startAngleRadian, sweepAngleRadian)
                    : BezierSegment.FromEllipse(center, radiusX, radiusY)
                .Transform(matrix));
        }

        internal override PathSegment Clone() => new ArcSegment(this);

        internal BitVector32 GetSectors()
        {
            Debug.Assert(Math.Abs(sweepAngle) < 360f, "Don't get the sectors of a full ellipse.");
            sectors ??= GetSectors(startAngle, sweepAngle);
            return sectors.Value;
        }

        internal (float StartRad, float EndRad) GetStartEndRadians() => sweepAngleRadian > 0f
            ? (startAngleRadian, startAngleRadian + sweepAngleRadian)
            : (startAngleRadian + sweepAngleRadian, startAngleRadian);

        #endregion

        #region Private Methods

        private void EnsureStartEndPoints()
        {
            if (startPoint.HasValue)
                return;

            float startRad = startAngleRadian;
            float endRad = startRad + sweepAngleRadian;
            AdjustAngles(ref startRad, ref endRad, radiusX, radiusY);

            float sinStart = MathF.Sin(startRad);
            float sinEnd = MathF.Sin(endRad);
            float cosStart = MathF.Cos(startRad);
            float cosEnd = MathF.Cos(endRad);

            startPoint = new PointF(center.X + radiusX * cosStart, center.Y + radiusY * sinStart);
            endPoint = new PointF(center.X + radiusX * cosEnd, center.Y + radiusY * sinEnd);
        }

        #endregion

        #endregion

        #endregion
    }
}
