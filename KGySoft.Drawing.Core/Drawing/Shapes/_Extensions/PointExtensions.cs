#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: PointExtensions.cs
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
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
using System.Numerics;
#endif
using System.Runtime.CompilerServices;

#if !(NETCOREAPP || NET45_OR_GREATER || NETSTANDARD)
using KGySoft.CoreLibraries;
#endif

#endregion

namespace KGySoft.Drawing.Shapes
{
    /// <summary>
    /// Extensions for Point/PointF structs.
    /// </summary>
    internal static class PointExtensions
    {
        #region Methods

        internal static bool TolerantEquals(this PointF p1, PointF p2, float tolerance)
        {
#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
            Vector2 distance = Vector2.Abs(p1.AsVector2() - p2.AsVector2());
            return distance.X < tolerance && distance.Y < tolerance;
#else
            return p1.X.TolerantEquals(p2.X, tolerance) && p1.Y.TolerantEquals(p2.Y, tolerance);
#endif
        }

#if NETCOREAPP3_0_OR_GREATER
        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Size AsSize(this Point point) => Unsafe.As<Point, Size>(ref point);
#else
        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static unsafe Size AsSize(this Point point) => *(Size*)&point;
#endif

        [MethodImpl(MethodImpl.AggressiveInlining)]
        [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator", Justification = "Intended, that's the point of the method.")]
        internal static bool IsInteger(this PointF pointF, out Point point)
        {
            point = new Point((int)pointF.X, (int)pointF.Y);
            return pointF.X == point.X && pointF.Y == point.Y;
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static bool HasNaNOrInfinity(this PointF point)
            => Single.IsNaN(point.X) || Single.IsInfinity(point.X) || Single.IsNaN(point.Y) || Single.IsInfinity(point.Y);

        #endregion
    }
}