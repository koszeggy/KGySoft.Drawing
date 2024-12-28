#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ArcSegment.cs
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
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;

#endregion

namespace KGySoft.Drawing.Shapes
{
    // NOTE: It would not be necessary to have a separate ArcSegment class because it could be represented by a BezierSegment,
    // but for drawing thin paths, the flatten Bézier curve drawing is both slower and less symmetric.
    internal sealed class ArcSegment : PathSegment
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

        #region Internal Fields

        // Supporting only translate/scale transformations for now, and converting to Bézier curves otherwise.
        // Storing both angles and radians because angles are more accurate when detecting used sectors for drawing.
        internal PointF Center;
        internal float RadiusX;
        internal float RadiusY;
        internal float StartAngle;
        internal float SweepAngle;
        internal float StartRad;
        internal float SweepRad;

        #endregion

        #region Private Fields

        private BitVector32? sectors;
        private PointF? startPoint;
        private PointF? endPoint;

        #endregion

        #endregion

        #endregion

        #region Properties

        internal float Width => Math.Abs(RadiusX) * 2f;
        internal float Height => Math.Abs(RadiusY) * 2f;

        internal override PointF StartPoint
        {
            get
            {
                EnsureStartEndPoints();
                return startPoint!.Value;
            }
        }

        internal override PointF EndPoint
        {
            get
            {
                EnsureStartEndPoints();
                return endPoint!.Value;
            }
        }

        internal RectangleF Bounds => new RectangleF(Center.X - RadiusX, Center.Y - RadiusY, RadiusX * 2f, RadiusY * 2f);

        #endregion

        #region Constructors

        internal ArcSegment(PointF center, float radiusX, float radiusY)
        {
            Center = center;
            RadiusX = radiusX;
            RadiusY = radiusY;
            SweepAngle = 360f;
        }

        internal ArcSegment(RectangleF bounds, float startAngle = 0f, float sweepAngle = 360f)
        {
            // Storing the center and radii rather than bounds, so transformations can be applied easily.
            RadiusX = bounds.Width / 2f;
            RadiusY = bounds.Height / 2f;
            Center = new PointF(bounds.X + RadiusX, bounds.Y + RadiusY);
            bool isFullEllipse = Math.Abs(sweepAngle) >= 360f;

            // For a full ellipse start/end points are always at 0 degrees.
            // This way the behavior is consistent with thin lines and Bézier curves-conversion.
            if (isFullEllipse)
            {
                SweepAngle = 360f;
                return;
            }

            NormalizeAngles(ref startAngle, ref sweepAngle);
            StartAngle = startAngle;
            SweepAngle = sweepAngle;
            StartRad = startAngle.ToRadian();
            SweepRad = sweepAngle.ToRadian();
        }

        #endregion

        #region Methods

        #region Static Methods

        internal static void NormalizeAngles(ref float startAngle, ref float sweepAngle)
        {
            if (sweepAngle < 0)
            {
                startAngle += sweepAngle;
                sweepAngle = -sweepAngle;
            }

            startAngle = startAngle is >= 0f and <= 360f ? startAngle : startAngle % 360f;
            if (startAngle < 0)
                startAngle += 360f;
        }

        // Adjusts the start/end angles for the radii of an ellipse. This is how also GDI+ calculates the start/end points of arcs.
        // The formula is taken from libgdiplus: https://github.com/mono/libgdiplus/blob/94a49875487e296376f209fe64b921c6020f74c0/src/graphics-path.c#L752
        [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator", Justification = "Intended")]
        internal static void AdjustAngles(ref float startRad, ref float endRad, float radiusX, float radiusY)
        {
            if (radiusX == radiusY)
                return;

            startRad = MathF.Atan2(radiusX * MathF.Sin(startRad), radiusY * MathF.Cos(startRad));
            endRad = MathF.Atan2(radiusX * MathF.Sin(endRad), radiusY * MathF.Cos(endRad));

            // The result of Atan2 is not in the correct quadrant when Atan2 is called with x == 0 and y != 0, so we may need to adjust it.
            // Another way would be to use a special Atan2 function similarly to ReactOS: https://github.com/reactos/reactos/blob/3dfbe526992849cf53a83fae784be2126319150b/dll/win32/gdiplus/gdiplus.c#L304
            if (Math.Abs(endRad - startRad) > MathF.PI)
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

        #region Instance Methods
        
        #region Internal Methods

        internal override IList<PointF> GetFlattenedPoints()
        {
            if (SweepAngle >= 360f)
            {
                return RadiusY is 0f ? new[] { new PointF(Center.X - RadiusX, Center.Y), new PointF(Center.X + RadiusX, Center.Y) }
                    : RadiusX is 0f ? new[] { new PointF(Center.X, Center.Y - RadiusY), new PointF(Center.X, Center.Y + RadiusY) }
                    : BezierSegment.FromEllipse(Center, RadiusX, RadiusY).GetFlattenedPoints();
            }

            // Vertically flat arc: special case, because the angle adjustment makes start/and angles equal, so returning just a single point
            if (RadiusX is 0f)
                return new[] { StartPoint };

            if (RadiusY is not 0f)
                return BezierSegment.FromArc(Center, RadiusX, RadiusY, StartRad, SweepRad).GetFlattenedPoints();

            // Horizontally flat arc: special handling because BezierSegment.ArcToBezier would not work here correctly
            float startRad = StartRad;
            float endRad = startRad + SweepRad;
            AdjustAngles(ref startRad, ref endRad, RadiusX, RadiusY);

            PointF start = StartPoint;
            PointF end = EndPoint;

            bool swapped = false;
            if (start.X > end.X || start.Y > end.Y)
            {
                (start, end) = (end, start);
                swapped = true;
            }

            if (startRad < 0f)
                startRad = MathF.PI * 2f + startRad;
            if (endRad < 0f)
                endRad = MathF.PI * 2f + endRad;

            // widening the start/end points to the horizontal bounds of the arc
            for (float currentRad = MathF.PI / 2f; currentRad < endRad; currentRad += MathF.PI / 2f)
            {
                if (currentRad <= startRad)
                    continue;
                float current = Center.X + RadiusX * MathF.Cos(currentRad);
                start.X = Math.Min(start.X, current);
                end.X = Math.Max(end.X, current);
            }

            return swapped ? new[] { end, start } : new[] { start, end };
        }

        internal override PathSegment Transform(TransformationMatrix matrix)
        {
            Debug.Assert(!matrix.IsIdentity);

            startPoint = null;
            endPoint = null;

            // If the transformation is translation or scale (including reflections) only, we can transform the arc directly.
            // See https://en.wikipedia.org/wiki/Transformation_matrix#/media/File:2D_affine_transformation_matrix.svg
            if (matrix is { M12: 0f, M21: 0f })
            {
                Center = Center.Transform(matrix);
                RadiusX *= matrix.M11;
                RadiusY *= matrix.M22;
                return this;
            }

            // Otherwise, converting the arc to a Bézier curve (or to a line if it's flat) and transforming that
            return (RadiusX is 0f || RadiusY is 0f ? (PathSegment)new LineSegment(GetFlattenedPoints())
                    : Math.Abs(SweepAngle) >= 360f ? BezierSegment.FromEllipse(Center, RadiusX, RadiusY)
                    : BezierSegment.FromArc(Center, RadiusX, RadiusY, StartRad, SweepRad))
                .Transform(matrix);
        }

        internal override PathSegment Clone() => new ArcSegment(Center, RadiusX, RadiusY)
        {
            StartAngle = StartAngle,
            SweepAngle = SweepAngle,
            StartRad = StartRad,
            SweepRad = SweepRad,
        };

        internal BitVector32 GetSectors()
        {
            Debug.Assert(Math.Abs(SweepAngle) < 360f, "Don't get the sectors of a full ellipse.");
            sectors ??= GetSectors(StartAngle, SweepAngle);
            return sectors.Value;
        }

        internal (float StartRad, float EndRad) GetStartEndRadians() => SweepRad > 0f
            ? (StartRad, StartRad + SweepRad)
            : (StartRad + SweepRad, StartRad);

        #endregion

        #region Private Methods

        private void EnsureStartEndPoints()
        {
            if (startPoint.HasValue)
                return;

            float startRad = StartRad;
            float endRad = startRad + SweepRad;
            AdjustAngles(ref startRad, ref endRad, RadiusX, RadiusY);

            float sinStart = MathF.Sin(startRad);
            float sinEnd = MathF.Sin(endRad);
            float cosStart = MathF.Cos(startRad);
            float cosEnd = MathF.Cos(endRad);

            startPoint = new PointF(Center.X + RadiusX * cosStart, Center.Y + RadiusY * sinStart);
            endPoint = new PointF(Center.X + RadiusX * cosEnd, Center.Y + RadiusY * sinEnd);
        }

        #endregion

        #endregion

        #endregion
    }
}
