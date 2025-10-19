#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ManagedBitmapDataArray2DBase`2.cs
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
    internal abstract class ManagedBitmapDataArray2DBase<T, TRow> : ManagedBitmapDataArray2DBase<T>
        where T : unmanaged
        where TRow : ManagedBitmapDataArraySectionRowBase<T>, new()
    {
        #region Constructors

        protected ManagedBitmapDataArray2DBase(in BitmapDataConfig cfg)
            : base(cfg)
        {
        }

        protected ManagedBitmapDataArray2DBase(Array2D<T> buffer, in BitmapDataConfig cfg)
            : base(buffer, cfg)
        {
        }

        #endregion

        #region Methods

        #region Private Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected sealed override IBitmapDataRowInternal DoGetRow(int y)
        {
            Debug.Assert(Buffer.GetElementReference(y, 0).AsIntPtr() % PixelFormat.AlignmentReq == 0, $"Misaligned {typeof(T).Name} at row {y} - {PixelFormat} {Width}x{Height}");
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