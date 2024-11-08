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

        #endregion

        #endregion

        #endregion

        #region Properties

        // TODO: This is not accurate for ellipse arcs. If cached, it should be recalculated after transformation.
        internal override PointF StartPoint => StartAngle == 0f
            ? new PointF(Center.X + RadiusX, Center.Y)
            : new PointF(Center.X + RadiusX * MathF.Cos(StartRad), Center.Y + RadiusY * MathF.Sin(StartRad));

        [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator", Justification = "Using degrees instead of radians provides reliable comparisons")]
        internal override PointF EndPoint
        {
            get
            {
                if (SweepAngle == 360f)
                    return StartPoint;
                float rad = StartRad + SweepRad;
                return new PointF(Center.X + RadiusX * MathF.Cos(rad), Center.Y + RadiusY * MathF.Sin(rad));
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

        [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator", Justification = "Using degrees instead of radians provides reliable comparisons")]
        internal ArcSegment(RectangleF bounds, float startAngle = 0f, float sweepAngle = 360f)
        {
            Debug.Assert(sweepAngle <= 360f);
            bool isFullEllipse = Math.Abs(sweepAngle) >= 360f;
            StartAngle = startAngle;
            SweepAngle = isFullEllipse ? 360f : sweepAngle;

            // Storing the center and radii rather than bounds, so transformations can be applied easily.
            RadiusX = bounds.Width / 2f;
            RadiusY = bounds.Height / 2f;
            Center = new PointF(bounds.X + RadiusX, bounds.Y + RadiusY);
            if (!isFullEllipse)
            {
                StartRad = startAngle.ToRadian();
                SweepRad = sweepAngle.ToRadian();
            }
        }

        #endregion

        #region Methods

        internal override IList<PointF> GetFlattenedPoints() => Math.Abs(SweepAngle) >= 360f
            ? BezierSegment.FromEllipse(Center, RadiusX, RadiusY).GetFlattenedPoints()
            : BezierSegment.FromArc(Center, RadiusX, RadiusY, StartRad, SweepRad).GetFlattenedPoints();

        internal override PathSegment Transform(TransformationMatrix matrix)
        {
            Debug.Assert(!matrix.IsIdentity);

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
            return (SweepRad >= MathF.PI * 2f
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
            if (sectors.HasValue)
                return sectors.Value;

            float startAngle = StartAngle;
            float sweepAngle = SweepAngle;

            // Normalizing start angle and sweep angle
            if (sweepAngle < 0)
            {
                startAngle += sweepAngle;
                sweepAngle = -sweepAngle;
            }

            startAngle = startAngle is >= 0f and <= 360f ? startAngle : startAngle % 360f;
            if (startAngle < 0)
                startAngle += 360f;

            var result = new BitVector32();
            int startSector = (int)(StartAngle / 90);
            int endSector = (int)((startAngle + sweepAngle) / 90) % 4;

            if (startSector == endSector)
                result[Sectors[startSector]] = SectorStartEnd;
            else
            {
                for (int i = startSector + 1; i < endSector; i++)
                    result[Sectors[i]] = SectorFullyDrawn;
                result[Sectors[startSector]] = SectorStart;
                result[Sectors[endSector]] = SectorEnd;
            }

            return (sectors = result).Value;

        }

        #endregion
    }
}
