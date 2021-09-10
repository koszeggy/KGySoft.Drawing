﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ManagedBitmapDataRowIndexedBase.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2021 - All Rights Reserved
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
    internal abstract class ManagedBitmapDataRowIndexedBase<TRow> : ManagedBitmapDataRowBase<byte, TRow>
        where TRow : ManagedBitmapDataRowIndexedBase<TRow>, new()
    {
        #region Properties

        protected abstract uint MaxIndex { get; }

        #endregion

        #region Methods

        #region Static Methods

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowColorIndexOutOfRange()
        {
            // ReSharper disable once NotResolvedInText
            throw new ArgumentOutOfRangeException("colorIndex", PublicResources.ArgumentOutOfRange);
        }

        #endregion

        #region Instance Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override void SetColorIndex(int x, int colorIndex)
        {
            if (colorIndex >= BitmapData.Palette!.Count || (uint)colorIndex > MaxIndex)
                ThrowColorIndexOutOfRange();
            base.SetColorIndex(x, colorIndex);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override Color32 DoGetColor32(int x) => BitmapData.Palette!.GetColor(DoGetColorIndex(x));

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override void DoSetColor32(int x, Color32 c) => DoSetColorIndex(x, BitmapData.Palette!.GetNearestColorIndex(c));

        #endregion

        #endregion
    }
}