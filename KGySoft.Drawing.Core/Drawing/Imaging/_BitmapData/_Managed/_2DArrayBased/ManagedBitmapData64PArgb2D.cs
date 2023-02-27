#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ManagedBitmapData64PArgb2D.cs
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

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal sealed class ManagedBitmapData64PArgb2D<T> : ManagedBitmapData2DArrayBase<T, ManagedBitmapData64PArgb2D<T>.Row>
        where T : unmanaged
    {
        #region Row class

        internal sealed class Row : ManagedBitmapDataRow2DBase<T>
        {
            #region Methods

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override Color32 DoGetColor32(int x) => DoReadRaw<PColor64>(x).ToColor32();

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetColor32(int x, Color32 c) => DoWriteRaw(x, new PColor64(c));

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override PColor32 DoGetColor32Premultiplied(int x) => DoReadRaw<PColor64>(x).ToPColor32();

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetColor32Premultiplied(int x, PColor32 c) => DoWriteRaw(x, new PColor64(c));

            #endregion
        }

        #endregion

        #region Constructors

        internal ManagedBitmapData64PArgb2D(T[,] buffer, in BitmapDataConfig cfg)
            : base(buffer, cfg)
        {
        }

        #endregion

        #region Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override Color32 DoGetPixel(int x, int y) => GetPixelRef<PColor64>(y, x).ToColor32();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override void DoSetPixel(int x, int y, Color32 c) => GetPixelRef<PColor64>(y, x) = new PColor64(c);

        #endregion
    }
}
