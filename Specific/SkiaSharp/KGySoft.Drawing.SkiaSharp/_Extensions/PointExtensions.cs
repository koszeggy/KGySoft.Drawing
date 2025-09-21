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

using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;

using SkiaSharp;

#endregion

namespace KGySoft.Drawing.SkiaSharp
{
    internal static class PointExtensions
    {
        #region Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static PointF AsPointF(this SKPoint point) => Unsafe.As<SKPoint, PointF>(ref point);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Vector2 AsVector2(this PointF point) => Unsafe.As<PointF, Vector2>(ref point);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static PointF AsPointF(this Vector2 vector) => Unsafe.As<Vector2, PointF>(ref vector);

        #endregion
    }
}
