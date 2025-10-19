#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: UnmanagedBitmapDataBase`1.cs
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
using System.Runtime.CompilerServices;

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal abstract class UnmanagedBitmapDataBase<TRow> : UnmanagedBitmapDataBase
        where TRow : UnmanagedBitmapDataRowBase, new()
    {
        #region Constructors

        protected UnmanagedBitmapDataBase(IntPtr buffer, int stride, in BitmapDataConfig cfg)
            : base(buffer, stride, cfg)
        {
        }

        #endregion

        #region Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected sealed override IBitmapDataRowInternal DoGetRow(int y)
        {
            // Not asserting row alignment here because a raw buffer is allowed to be misaligned
            //Debug.Assert((Scan0 + Stride * y) % PixelFormat.AlignmentReq == 0, $"Misaligned address  {(Scan0 + Stride * y)} at row {y} - {PixelFormat} {Width}x{Height}");
            return new TRow
            {
                Row = y == 0 ? Scan0 : (nint)(Scan0 + (long)Stride * y),
                BitmapData = this,
                Index = y,
            };
        }

        #endregion
    }
}