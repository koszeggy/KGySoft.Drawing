#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ManagedBitmapData128Rgba.cs
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

using KGySoft.Collections;

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal sealed class ManagedBitmapData128Rgba<T> : ManagedBitmapDataCastArray2DBase<T, ColorF, ManagedBitmapData128Rgba<T>.Row>
        where T : unmanaged
    {
        #region Row class

        internal sealed class Row : ManagedBitmapDataCastArrayRowBase<T, ColorF>
        {
            #region Methods

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override Color32 DoGetColor32(int x) => GetPixelRef(x).ToColor32();

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetColor32(int x, Color32 c) => GetPixelRef(x) = new ColorF(c);

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override Color64 DoGetColor64(int x) => GetPixelRef(x).ToColor64();

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetColor64(int x, Color64 c) => GetPixelRef(x) = new ColorF(c);

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override PColor64 DoGetPColor64(int x) => GetPixelRef(x).ToPColor64();

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetPColor64(int x, PColor64 c) => GetPixelRef(x) = c.ToColorF();

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override ColorF DoGetColorF(int x) => GetPixelRef(x);

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetColorF(int x, ColorF c) => GetPixelRef(x) = c;

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override PColorF DoGetPColorF(int x) => GetPixelRef(x).ToPColorF();

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetPColorF(int x, PColorF c) => GetPixelRef(x) = c.ToColorF();

            #endregion
        }

        #endregion

        #region Constructors

        internal ManagedBitmapData128Rgba(CastArray2D<T, ColorF> buffer, in BitmapDataConfig cfg)
            : base(buffer, cfg)
        {
        }

        #endregion

        #region Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override Color32 DoGetColor32(int x, int y) => GetPixelRef(y, x).ToColor32();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override void DoSetColor32(int x, int y, Color32 c) => GetPixelRef(y, x) = new ColorF(c);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override Color64 DoGetColor64(int x, int y) => GetPixelRef(y, x).ToColor64();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override void DoSetColor64(int x, int y, Color64 c) => GetPixelRef(y, x) = new ColorF(c);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override PColor64 DoGetPColor64(int x, int y) => GetPixelRef(y, x).ToPColor64();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override void DoSetPColor64(int x, int y, PColor64 c) => GetPixelRef(y, x) = c.ToColorF();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override ColorF DoGetColorF(int x, int y) => GetPixelRef(y, x);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override void DoSetColorF(int x, int y, ColorF c) => GetPixelRef(y, x) = c;

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override PColorF DoGetPColorF(int x, int y) => GetPixelRef(y, x).ToPColorF();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override void DoSetPColorF(int x, int y, PColorF c) => GetPixelRef(y, x) = c.ToColorF();

        #endregion
    }
}
