#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ManagedBitmapData1DArrayBase`2.cs
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
using System.Drawing;
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

        protected ManagedBitmapData1DArrayBase(Size size, KnownPixelFormat pixelFormat, Color32 backColor = default, byte alphaThreshold = 0, Palette? palette = null)
            : base(size, pixelFormat, backColor, alphaThreshold, palette)
        {
        }

        protected ManagedBitmapData1DArrayBase(Array2D<T> buffer, int pixelWidth, PixelFormatInfo pixelFormat, Color32 backColor, byte alphaThreshold,
            Action? disposeCallback, Palette? palette = null, Func<Palette, bool>? trySetPaletteCallback = null)
            : base(buffer, pixelWidth, pixelFormat, backColor, alphaThreshold, palette, trySetPaletteCallback, disposeCallback)
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