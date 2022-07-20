#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ManagedBitmapData16Rgb565.cs
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
    internal sealed class ManagedBitmapData16Rgb565 : ManagedBitmapData1DArrayBase<Color16Rgb565, ManagedBitmapData16Rgb565.Row>
    {
        #region Row class

        internal sealed class Row : ManagedBitmapDataRowBase<Color16Rgb565>
        {
            #region Methods

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override Color32 DoGetColor32(int x) => Row[x].ToColor32();

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetColor32(int x, Color32 c)
                => Row[x] = new Color16Rgb565(c.A == Byte.MaxValue ? c : c.BlendWithBackground(BitmapData.BackColor));

            #endregion
        }

        #endregion

        #region Constructors

        internal ManagedBitmapData16Rgb565(Size size, Color32 backColor, byte alphaThreshold)
            : base(size, KnownPixelFormat.Format16bppRgb565, backColor, alphaThreshold)
        {
        }

        internal ManagedBitmapData16Rgb565(Array2D<Color16Rgb565> buffer, int pixelWidth, Color32 backColor, byte alphaThreshold, Action? disposeCallback)
            : base(buffer, pixelWidth, KnownPixelFormat.Format16bppRgb565.ToInfoInternal(), backColor, alphaThreshold, disposeCallback)
        {
        }

        #endregion

        #region Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override Color32 DoGetPixel(int x, int y) => Buffer[y, x].ToColor32();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override void DoSetPixel(int x, int y, Color32 c)
            => Buffer[y, x] = new Color16Rgb565(c.A == Byte.MaxValue ? c : c.BlendWithBackground(BackColor));

        #endregion
    }
}
