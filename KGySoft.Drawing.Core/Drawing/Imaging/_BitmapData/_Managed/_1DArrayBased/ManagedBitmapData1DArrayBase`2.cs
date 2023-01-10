#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ManagedBitmapData1DArrayBase`2.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2023 - All Rights Reserved
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
    internal abstract class ManagedBitmapData1DArrayBase<T, TRow> : ManagedBitmapData1DArrayBase<T>
        where T : unmanaged
        where TRow : ManagedBitmapDataRowBase<T>, new()
    {
        #region Constructors

        protected ManagedBitmapData1DArrayBase(in BitmapDataConfig cfg)
            : base(cfg)
        {
        }

        protected ManagedBitmapData1DArrayBase(Array2D<T> buffer, in BitmapDataConfig cfg)
            : base(buffer, cfg)
        {
        }

        #endregion

        #region Methods

        #region Private Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected sealed override IBitmapDataRowInternal DoGetRow(int y) => new TRow
        {
            Row = Buffer[y],
            BitmapData = this,
            Index = y,
        };

        #endregion

        #endregion
    }
}