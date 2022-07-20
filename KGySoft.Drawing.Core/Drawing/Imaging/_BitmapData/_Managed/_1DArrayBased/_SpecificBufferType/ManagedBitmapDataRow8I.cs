#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ManagedBitmapData8I.cs
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
    internal sealed class ManagedBitmapData8I : ManagedBitmapData1DArrayIndexedBase<byte, ManagedBitmapData8I.Row>
    {
        #region Row class

        internal sealed class Row : ManagedBitmapDataRowIndexedBase<byte>
        {
            #region Properties

            protected override uint MaxIndex => 255;

            #endregion

            #region Methods

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override int DoGetColorIndex(int x) => Row[x];

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetColorIndex(int x, int colorIndex) => Row[x] = (byte)colorIndex;

            #endregion
        }

        #endregion

        #region Constructors

        internal ManagedBitmapData8I(Size size, Color32 backColor, byte alphaThreshold, Palette? palette)
            : base(size, KnownPixelFormat.Format8bppIndexed, backColor, alphaThreshold, palette)
        {
        }

        internal ManagedBitmapData8I(Array2D<byte> buffer, int pixelWidth, Color32 backColor, byte alphaThreshold,
            Palette? palette, Func<Palette, bool>? trySetPaletteCallback, Action? disposeCallback)
            : base(buffer, pixelWidth, KnownPixelFormat.Format8bppIndexed.ToInfoInternal(), backColor, alphaThreshold, disposeCallback, palette, trySetPaletteCallback)
        {
        }

        #endregion

        #region Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override int DoGetColorIndex(int x, int y) => Buffer[y, x];

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override void DoSetColorIndex(int x, int y, int colorIndex) => Buffer[y, x] = (byte)colorIndex;

        #endregion
    }
}
