﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ManagedBitmapData4I2D.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2022 - All Rights Reserved
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
    internal sealed class ManagedBitmapData4I2D<T> : ManagedBitmapData2DArrayIndexedBase<T, ManagedBitmapData4I2D<T>.Row>
        where T : unmanaged
    {
        #region Row class

        internal sealed class Row : ManagedBitmapDataRowIndexed2DBase<T>
        {
            #region Properties

            protected override uint MaxIndex => 15;

            #endregion

            #region Methods

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override int DoGetColorIndex(int x) => ColorExtensions.Get4bppColorIndex(DoReadRaw<byte>(x >> 1), x);

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetColorIndex(int x, int colorIndex)
                => ColorExtensions.Set4bppColorIndex(ref GetPixelRef<byte>(x >> 1), x, colorIndex);

            #endregion
        }

        #endregion

        #region Constructors

        internal ManagedBitmapData4I2D(T[,] buffer, int pixelWidth, Color32 backColor, byte alphaThreshold,
            Palette? palette, Func<Palette, bool>? trySetPaletteCallback, Action? disposeCallback)
            : base(buffer, pixelWidth, KnownPixelFormat.Format4bppIndexed.ToInfoInternal(), backColor, alphaThreshold, disposeCallback, palette, trySetPaletteCallback)
        {
        }

        #endregion

        #region Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override int DoGetColorIndex(int x, int y) => ColorExtensions.Get4bppColorIndex(GetPixelRef<byte>(y, x >> 1), x);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override void DoSetColorIndex(int x, int y, int colorIndex)
            => ColorExtensions.Set4bppColorIndex(ref GetPixelRef<byte>(y, x >> 1), x, colorIndex);

        #endregion
    }
}
