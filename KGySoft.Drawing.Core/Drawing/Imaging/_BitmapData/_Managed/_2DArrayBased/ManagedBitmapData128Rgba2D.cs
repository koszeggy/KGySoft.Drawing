#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ManagedBitmapData128Rgba2D.cs
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
    internal sealed class ManagedBitmapData128Rgba2D<T> : ManagedBitmapData2DArrayBase<T, ManagedBitmapData128Rgba2D<T>.Row>
        where T : unmanaged
    {
        #region Row class

        internal sealed class Row : ManagedBitmapDataRow2DBase<T>
        {
            #region Methods

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override Color32 DoGetColor32(int x) => DoReadRaw<ColorF>(x).ToColor32();

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetColor32(int x, Color32 c) => DoWriteRaw(x, new ColorF(c));

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override Color64 DoGetColor64(int x) => DoReadRaw<ColorF>(x).ToColor64();

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetColor64(int x, Color64 c) => DoWriteRaw(x, new ColorF(c));

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override PColor64 DoGetPColor64(int x) => DoReadRaw<ColorF>(x).ToPColor64();

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetPColor64(int x, PColor64 c) => DoWriteRaw(x, c.ToColorF());

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override ColorF DoGetColorF(int x) => DoReadRaw<ColorF>(x);

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetColorF(int x, ColorF c) => DoWriteRaw(x, c);

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override PColorF DoGetPColorF(int x) => DoReadRaw<ColorF>(x).ToPColorF();

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetPColorF(int x, PColorF c) => DoWriteRaw(x, c.ToColorF());

            #endregion
        }

        #endregion

        #region Constructors

        internal ManagedBitmapData128Rgba2D(T[,] buffer, in BitmapDataConfig cfg)
            : base(buffer, cfg)
        {
        }

        #endregion

        #region Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override Color32 DoGetColor32(int x, int y) => GetPixelRef<ColorF>(y, x).ToColor32();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override void DoSetColor32(int x, int y, Color32 c) => GetPixelRef<ColorF>(y, x) = new ColorF(c);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override Color64 DoGetColor64(int x, int y) => GetPixelRef<ColorF>(y, x).ToColor64();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override void DoSetColor64(int x, int y, Color64 c) => GetPixelRef<ColorF>(y, x) = new ColorF(c);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override PColor64 DoGetPColor64(int x, int y) => GetPixelRef<ColorF>(y, x).ToPColor64();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override void DoSetPColor64(int x, int y, PColor64 c) => GetPixelRef<ColorF>(y, x) = c.ToColorF();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override ColorF DoGetColorF(int x, int y) => GetPixelRef<ColorF>(y, x);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override void DoSetColorF(int x, int y, ColorF c) => GetPixelRef<ColorF>(y, x) = c;

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override PColorF DoGetPColorF(int x, int y) => GetPixelRef<ColorF>(y, x).ToPColorF();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override void DoSetPColorF(int x, int y, PColorF c) => GetPixelRef<ColorF>(y, x) = c.ToColorF();

        #endregion
    }
}
