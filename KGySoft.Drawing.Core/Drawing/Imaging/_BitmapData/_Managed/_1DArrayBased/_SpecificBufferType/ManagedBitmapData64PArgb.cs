#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ManagedBitmapData64PArgb.cs
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
    internal sealed class ManagedBitmapData64PArgb : ManagedBitmapData1DArrayBase<Color64, ManagedBitmapData64PArgb.Row>
    {
        #region Row class

        internal sealed class Row : ManagedBitmapDataRowBase<Color64>
        {
            #region Methods

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override Color32 DoGetColor32(int x) => Row[x].ToStraight().ToColor32();

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetColor32(int x, Color32 c) => Row[x] = new Color64(c).ToPremultiplied();

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override Color32 DoGetColor32Premultiplied(int x) => Row[x].ToColor32();

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetColor32Premultiplied(int x, Color32 c) => Row[x] = new Color64(c);

            #endregion
        }

        #endregion

        #region Constructors

        internal ManagedBitmapData64PArgb(Size size, Color32 backColor, byte alphaThreshold)
            : base(size, KnownPixelFormat.Format64bppPArgb, backColor, alphaThreshold)
        {
        }

        internal ManagedBitmapData64PArgb(Array2D<Color64> buffer, int pixelWidth, Color32 backColor, byte alphaThreshold, Action? disposeCallback)
            : base(buffer, pixelWidth, KnownPixelFormat.Format64bppPArgb.ToInfoInternal(), backColor, alphaThreshold, disposeCallback)
        {
        }

        #endregion

        #region Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override Color32 DoGetPixel(int x, int y) => Buffer[y, x].ToStraight().ToColor32();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override void DoSetPixel(int x, int y, Color32 c) => Buffer[y, x] = new Color64(c).ToPremultiplied();

        #endregion
    }
}
