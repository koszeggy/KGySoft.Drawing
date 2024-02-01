#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ManagedBitmapData64Argb2D.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2024 - All Rights Reserved
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
    internal sealed class ManagedBitmapData64Argb2D<T> : ManagedBitmapData2DArrayBase<T, ManagedBitmapData64Argb2D<T>.Row>
        where T : unmanaged
    {
        #region Row class

        internal sealed class Row : ManagedBitmapDataRow2DBase<T>
        {
            #region Methods

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override Color32 DoGetColor32(int x) => DoReadRaw<Color64>(x).ToColor32();

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetColor32(int x, Color32 c) => DoWriteRaw(x, new Color64(c));

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override Color64 DoGetColor64(int x) => DoReadRaw<Color64>(x);

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetColor64(int x, Color64 c) => DoWriteRaw(x, c);

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override PColor64 DoGetPColor64(int x) => DoReadRaw<Color64>(x).ToPColor64();

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetPColor64(int x, PColor64 c) => DoWriteRaw(x, c.ToColor64());

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override ColorF DoGetColorF(int x) => DoReadRaw<Color64>(x).ToColorF();

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetColorF(int x, ColorF c) => DoWriteRaw(x, c.ToColor64());

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override PColorF DoGetPColorF(int x) => DoReadRaw<Color64>(x).ToPColorF();

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetPColorF(int x, PColorF c) => DoWriteRaw(x, c.ToColor64());

            #endregion
        }

        #endregion

        #region Constructors

        internal ManagedBitmapData64Argb2D(T[,] buffer, in BitmapDataConfig cfg)
            : base(buffer, cfg)
        {
        }

        #endregion

        #region Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override Color32 DoGetColor32(int x, int y) => GetPixelRef<Color64>(y, x).ToColor32();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override void DoSetColor32(int x, int y, Color32 c) => GetPixelRef<Color64>(y, x) = new Color64(c);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override Color64 DoGetColor64(int x, int y) => GetPixelRef<Color64>(y, x);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override void DoSetColor64(int x, int y, Color64 c) => GetPixelRef<Color64>(y, x) = c;

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override PColor64 DoGetPColor64(int x, int y) => GetPixelRef<Color64>(y, x).ToPColor64();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override void DoSetPColor64(int x, int y, PColor64 c) => GetPixelRef<Color64>(y, x) = c.ToColor64();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override ColorF DoGetColorF(int x, int y) => GetPixelRef<Color64>(y, x).ToColorF();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override void DoSetColorF(int x, int y, ColorF c) => GetPixelRef<Color64>(y, x) = c.ToColor64();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override PColorF DoGetPColorF(int x, int y) => GetPixelRef<Color64>(y, x).ToPColorF();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override void DoSetPColorF(int x, int y, PColorF c) => GetPixelRef<Color64>(y, x) = c.ToColor64();

        #endregion
    }
}
