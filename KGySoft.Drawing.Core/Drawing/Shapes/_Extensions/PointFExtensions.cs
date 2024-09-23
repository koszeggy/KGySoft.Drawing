#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: PointFExtensions.cs
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

using System.Drawing;
#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
using System.Numerics;
#endif
#if NETCOREAPP3_0_OR_GREATER
using System.Runtime.CompilerServices;
#endif

#if !(NETCOREAPP || NET45_OR_GREATER || NETSTANDARD)
using KGySoft.CoreLibraries;
#endif

#endregion

namespace KGySoft.Drawing.Shapes
{
    internal static class PointFExtensions
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
        internal static Vector2 AsVector2(this PointF point) => Unsafe.As<PointF, Vector2>(ref point);
#elif NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Vector2 AsVector2(this PointF point) => new Vector2(point.X, point.Y);
#endif

        internal static PointF Transform(this PointF point, TransformationMatrix matrix)
        {
#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
            return Vector2.Transform(point.AsVector2(), matrix.Matrix).AsPointF();
#endif
        }

        #endregion
    }
}