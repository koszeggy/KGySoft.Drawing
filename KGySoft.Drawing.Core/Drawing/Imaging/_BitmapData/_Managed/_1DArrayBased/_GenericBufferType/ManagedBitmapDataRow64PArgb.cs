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
using System.Runtime.CompilerServices;

using KGySoft.Collections;

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal sealed class ManagedBitmapData64PArgb<T> : ManagedBitmapData1DArrayBase<T, ManagedBitmapData64PArgb<T>.Row>
        where T : unmanaged
    {
        #region Row class

        internal sealed class Row : ManagedBitmapDataRowBase<T>
        {
            #region Methods

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override Color32 DoGetColor32(int x) => DoReadRaw<Color64>(x).ToStraight().ToColor32();

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetColor32(int x, Color32 c) => DoWriteRaw(x, new Color64(c).ToPremultiplied());

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override Color32 DoGetColor32Premultiplied(int x) => DoReadRaw<Color64>(x).ToColor32();

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetColor32Premultiplied(int x, Color32 c) => DoWriteRaw(x, new Color64(c));

            #endregion
        }

        #endregion

        #region Constructors

        internal ManagedBitmapData64PArgb(Array2D<T> buffer, int pixelWidth, Color32 backColor, byte alphaThreshold, Action? disposeCallback)
            : base(buffer, pixelWidth, KnownPixelFormat.Format64bppPArgb.ToInfoInternal(), backColor, alphaThreshold, disposeCallback)
        {
        }

        #endregion

        #region Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override Color32 DoGetPixel(int x, int y) => GetPixelRef<Color64>(y, x).ToStraight().ToColor32();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override void DoSetPixel(int x, int y, Color32 c) => GetPixelRef<Color64>(y, x) = new Color64(c).ToPremultiplied();

        #endregion
    }
}
