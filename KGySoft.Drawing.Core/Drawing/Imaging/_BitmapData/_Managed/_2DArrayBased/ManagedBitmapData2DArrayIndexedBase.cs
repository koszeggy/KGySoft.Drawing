﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ManagedBitmapData2DArrayIndexedBase.cs
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
using System.Security;

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal abstract class ManagedBitmapData2DArrayIndexedBase<T, TRow> : ManagedBitmapData2DArrayBase<T, TRow>
        where T : unmanaged
        where TRow : ManagedBitmapDataRow2DBase<T>, new()
    {
        #region Properties

        protected abstract uint MaxIndex { get; }

        #endregion

        #region Constructors

        protected ManagedBitmapData2DArrayIndexedBase(T[,] buffer, in BitmapDataConfig cfg)
            : base(buffer, cfg)
        {
        }

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

        [SecuritySafeCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public sealed override void SetColorIndex(int x, int y, int colorIndex)
        {
            if (colorIndex >= Palette!.Count || (uint)colorIndex > MaxIndex)
                ThrowColorIndexOutOfRange();
            base.SetColorIndex(x, y, colorIndex);
        }

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public sealed override Color32 DoGetColor32(int x, int y) => Palette!.GetColor(DoGetColorIndex(x, y));

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public sealed override void DoSetColor32(int x, int y, Color32 c) => DoSetColorIndex(x, y, Palette!.GetNearestColorIndex(c));

        #endregion

        #endregion
    }
}