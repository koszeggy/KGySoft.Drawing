#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ManagedBitmapData32PArgb.cs
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
    internal sealed class ManagedBitmapData32PArgb : ManagedBitmapData1DArrayBase<Color32, ManagedBitmapData32PArgb.Row>
    {
        #region Row class

        internal sealed class Row : ManagedBitmapDataRowBase<Color32>
        {
            #region Methods

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override Color32 DoGetColor32(int x) => Row[x].ToStraight();

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetColor32(int x, Color32 c) => Row[x] = c.ToPremultiplied();

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override Color32 DoGetColor32Premultiplied(int x) => Row[x];

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetColor32Premultiplied(int x, Color32 c) => Row[x] = c;

            #endregion
        }

        #endregion

        #region Constructors

        internal ManagedBitmapData32PArgb(Size size, Color32 backColor, byte alphaThreshold)
            : base(size, KnownPixelFormat.Format32bppPArgb, backColor, alphaThreshold)
        {
        }

        internal ManagedBitmapData32PArgb(Array2D<Color32> buffer, int pixelWidth, Color32 backColor, byte alphaThreshold, Action? disposeCallback)
            : base(buffer, pixelWidth, KnownPixelFormat.Format32bppPArgb.ToInfoInternal(), backColor, alphaThreshold, disposeCallback)
        {
        }

        #endregion

        #region Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override Color32 DoGetPixel(int x, int y) => Buffer[y, x].ToStraight();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override void DoSetPixel(int x, int y, Color32 c) => Buffer[y, x] = c.ToPremultiplied();

        #endregion
    }
}
