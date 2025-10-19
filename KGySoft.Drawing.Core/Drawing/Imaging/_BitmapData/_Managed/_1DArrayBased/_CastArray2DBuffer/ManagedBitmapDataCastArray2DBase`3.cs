#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ManagedBitmapDataCastArray2DBase`3.cs
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

using System.Runtime.CompilerServices;

using KGySoft.Collections;

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal abstract class ManagedBitmapDataCastArray2DBase<T, TPixel, TRow> : ManagedBitmapDataCastArray2DBase<T, TPixel>
        where T : unmanaged
        where TPixel : unmanaged
        where TRow : ManagedBitmapDataCastArrayRowBase<T, TPixel>, new()
    {
        #region Constructors

        protected ManagedBitmapDataCastArray2DBase(in BitmapDataConfig cfg)
            : base(cfg)
        {
        }

        protected ManagedBitmapDataCastArray2DBase(CastArray2D<T, TPixel> buffer, in BitmapDataConfig cfg)
            : base(buffer, cfg)
        {
        }

        #endregion

        #region Methods

        #region Private Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected sealed override IBitmapDataRowInternal DoGetRow(int y)
        {
            // Not asserting row alignment here because a CastArray is allowed to use a misaligned underlying buffer
            //Debug.Assert(Buffer.GetElementReference(y, 0).AsIntPtr() % PixelFormat.AlignmentReq == 0, $"Misaligned {typeof(TPixel).Name} at row {y} - {PixelFormat} {Width}x{Height}");
            return new TRow
            {
                Row = Buffer[y],
                BitmapData = this,
                Index = y,
            };
        }

        #endregion

        #endregion
    }
}