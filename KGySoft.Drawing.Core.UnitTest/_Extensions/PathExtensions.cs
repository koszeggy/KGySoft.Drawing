#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: PathExtensions.cs
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

using System.Drawing;

using KGySoft.Drawing.Shapes;

#endregion

namespace KGySoft.Drawing
{
    internal static class PathExtensions
    {
        #region Methods

        internal static Path AddQuad(this Path path, PointF start, PointF cp, PointF end)
        {
            Path.GetCubicBezierControlPointsFromQuadraticBezier(start, cp, end, out PointF cp1, out PointF cp2);
            return path.AddBezier(start, cp1, cp2, end);
        }

        internal static Path AddConic(this Path path, PointF start, PointF cp, PointF end, float weight)
        {
            Path.GetCubicBezierControlPointsFromConicCurve(start, cp, end, weight, out PointF cp1, out PointF cp2);
            return path.AddBezier(start, cp1, cp2, end);
        }

        #endregion
    }
}