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
        // To support other transformations, we should store startPoint/endPoint.
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
            bool isFullEllipse = Math.Abs(sweepAngle) >= 360f;
            StartAngle = startAngle;
            SweepAngle = isFullEllipse ? 360f : sweepAngle;

            // Storing the center and radii rather than bounds, so transformations can be applied easily.
            RadiusX = bounds.Width / 2f;
            RadiusY = bounds.Height / 2f;
            Center = new PointF(bounds.X + RadiusX, bounds.Y + RadiusY);

            // For a full ellipse start/end points are always at 0 degrees.
            // This way the behavior is consistent with thin lines and Bézier curves-conversion.
            if (!isFullEllipse)
            {
                StartRad = startAngle.ToRadian();
                SweepRad = sweepAngle.ToRadian();
            }
        }

        #endregion

        #region Methods

        #region Static Methods

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
        internal static BitVector32 GetSectors(float startAngle, float sweepAngle)
        {
            // Normalizing start angle and sweep angle
            if (sweepAngle < 0)
            {
                startAngle += sweepAngle;
                sweepAngle = -sweepAngle;
            }

            startAngle = startAngle is >= 0f and <= 360f ? startAngle : startAngle % 360f;
            if (startAngle < 0)
                startAngle += 360f;

            // The original code uses an array here but by using BitVector32 we can store the sectors in a single int, avoiding array allocation.
            var result = new BitVector32();
            int startSector = (int)(startAngle / 90);
            int endSector = (int)((startAngle + sweepAngle) / 90) & 3;

            if (startSector == endSector)
                result[Sectors[startSector]] = SectorStartEnd;
            else
            {
                for (int i = startSector + 1; i < endSector; i++)
                    result[Sectors[i]] = SectorFullyDrawn;
                result[Sectors[startSector]] = SectorStart;
                result[Sectors[endSector]] = SectorEnd;
            }

            return result;
        }

        #endregion

        #region Instance Methods
        
        #region Internal Methods

        internal override IList<PointF> GetFlattenedPoints() => Math.Abs(SweepAngle) >= 360f
            ? BezierSegment.FromEllipse(Center, RadiusX, RadiusY).GetFlattenedPoints()
            : BezierSegment.FromArc(Center, RadiusX, RadiusY, StartRad, SweepRad).GetFlattenedPoints();

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

            // Otherwise, converting the arc to a Bézier curve and transforming that.
            return (Math.Abs(SweepAngle) >= 360f
                    ? BezierSegment.FromEllipse(Center, RadiusX, RadiusY)
                    : BezierSegment.FromArc(Center, RadiusX, RadiusY, StartRad, SweepRad))
                .Transform(matrix);
        }

        internal override PathSegment Clone() => new ArcSegment(Center, RadiusX, RadiusY)
        {
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
