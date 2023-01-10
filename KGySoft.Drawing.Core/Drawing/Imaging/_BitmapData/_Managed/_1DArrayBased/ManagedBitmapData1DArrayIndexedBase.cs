#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ManagedBitmapData1DArrayIndexedBase.cs
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
    internal abstract class ManagedBitmapData1DArrayIndexedBase<T, TRow> : ManagedBitmapData1DArrayBase<T, TRow>
        where T : unmanaged
        where TRow : ManagedBitmapDataRowBase<T>, new()
    {
        #region Constructors

        protected ManagedBitmapData1DArrayIndexedBase(in BitmapDataConfig cfg)
            : base(cfg)
        {
        }

        protected ManagedBitmapData1DArrayIndexedBase(Array2D<T> buffer, in BitmapDataConfig cfg)
            : base(buffer, cfg)
        {
        }

        #endregion

        #region Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected sealed override Color32 DoGetPixel(int x, int y) => Palette!.GetColor(DoGetColorIndex(x, y));

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected sealed override void DoSetPixel(int x, int y, Color32 c) => DoSetColorIndex(x, y, Palette!.GetNearestColorIndex(c));

        protected abstract int DoGetColorIndex(int x, int y);
        protected abstract void DoSetColorIndex(int x, int y, int colorIndex);

        #endregion
    }
}
