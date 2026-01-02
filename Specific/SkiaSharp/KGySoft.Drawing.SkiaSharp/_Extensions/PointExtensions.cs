#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: PointExtensions.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2026 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System.Drawing;
using System.Runtime.CompilerServices;
using System.Security;

using SkiaSharp;

#endregion

namespace KGySoft.Drawing.SkiaSharp
{
    internal static class PointExtensions
    {
        #region Methods

        [SecuritySafeCritical]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ref PointF AsPointF(this ref SKPoint point) => ref Unsafe.As<SKPoint, PointF>(ref point);

        #endregion
    }
}
