#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ManagedBitmapData32PArgb.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2023 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System.Runtime.CompilerServices;

using KGySoft.Collections;

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal sealed class ManagedBitmapData32PArgb<T> : ManagedBitmapData1DArrayBase<T, ManagedBitmapData32PArgb<T>.Row>
        where T : unmanaged
    {
        #region Row class

        internal sealed class Row : ManagedBitmapDataRowBase<T>
        {
            #region Methods

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override Color32 DoGetColor32(int x) => DoReadRaw<PColor32>(x).ToStraight();

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetColor32(int x, Color32 c) => DoWriteRaw(x, c.ToPremultiplied());

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override PColor32 DoGetColor32Premultiplied(int x) => DoReadRaw<PColor32>(x);

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetColor32Premultiplied(int x, PColor32 c) => DoWriteRaw(x, c);

            #endregion
        }

        #endregion

        #region Constructors

        internal ManagedBitmapData32PArgb(Array2D<T> buffer, in BitmapDataConfig cfg)
            : base(buffer, cfg)
        {
        }

        #endregion

        #region Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]

        protected override Color32 DoGetPixel(int x, int y) => GetPixelRef<PColor32>(y, x).ToStraight();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override void DoSetPixel(int x, int y, Color32 c) => GetPixelRef<PColor32>(y, x) = c.ToPremultiplied();

        #endregion
    }
}
