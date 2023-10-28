#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ManagedBitmapData2DArrayIndexedBase.cs
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

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal abstract class ManagedBitmapData2DArrayIndexedBase<T, TRow> : ManagedBitmapData2DArrayBase<T, TRow>
        where T : unmanaged
        where TRow : ManagedBitmapDataRow2DBase<T>, new()
    {
        #region Constructors

        protected ManagedBitmapData2DArrayIndexedBase(T[,] buffer, in BitmapDataConfig cfg)
            : base(buffer, cfg)
        {
        }

        #endregion

        #region Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected sealed override Color32 DoGetColor32(int x, int y) => Palette!.GetColor(DoGetColorIndex(x, y));

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected sealed override void DoSetColor32(int x, int y, Color32 c) => DoSetColorIndex(x, y, Palette!.GetNearestColorIndex(c));

        protected abstract int DoGetColorIndex(int x, int y);
        protected abstract void DoSetColorIndex(int x, int y, int colorIndex);

        #endregion
    }
}