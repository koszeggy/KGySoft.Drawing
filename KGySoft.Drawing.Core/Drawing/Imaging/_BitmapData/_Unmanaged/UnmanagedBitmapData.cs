﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: UnmanagedBitmapData.cs
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

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal abstract class UnmanagedBitmapData<TRow> : UnmanagedBitmapDataBase
        where TRow : UnmanagedBitmapDataRowBase, new()
    {
        #region Constructors

        protected UnmanagedBitmapData(IntPtr buffer, Size size, int stride, PixelFormatInfo pixelFormat, Color32 backColor, byte alphaThreshold,
            Action? disposeCallback, Palette? palette = null, Func<Palette, bool>? trySetPaletteCallback = null)
            : base(buffer, size, stride, pixelFormat, backColor, alphaThreshold, palette, trySetPaletteCallback, disposeCallback)
        {
            Debug.Assert(pixelFormat.IsKnownFormat);
        }

        #endregion

        #region Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected sealed override IBitmapDataRowInternal DoGetRow(int y) => new TRow
        {
#if NET35
            Row = y == 0 ? Scan0 : new IntPtr(Scan0.ToInt64() + Stride * y),
#else
            Row = y == 0 ? Scan0 : Scan0 + Stride * y,
#endif
            BitmapData = this,
            Index = y,
        };

        #endregion
    }
}