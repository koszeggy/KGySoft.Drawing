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
    internal sealed class ManagedBitmapData128Rgba : ManagedBitmapData1DArrayBase<ColorF, ManagedBitmapData128Rgba.Row>
    {
        #region Row class

        internal sealed class Row : ManagedBitmapDataRowBase<ColorF>
        {
            #region Methods

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override Color32 DoGetColor32(int x) => Row[x].ToColor32();

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetColor32(int x, Color32 c) => Row[x] = new ColorF(c);

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override Color64 DoGetColor64(int x) => Row[x].ToColor64();

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetColor64(int x, Color64 c) => Row[x] = new ColorF(c);

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override PColor64 DoGetPColor64(int x) => Row[x].ToPColor64();

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetPColor64(int x, PColor64 c) => Row[x] = c.ToColorF();

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override ColorF DoGetColorF(int x) => Row[x];

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetColorF(int x, ColorF c) => Row[x] = c;

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override PColorF DoGetPColorF(int x) => Row[x].ToPColorF();

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetPColorF(int x, PColorF c) => Row[x] = c.ToColorF();

            #endregion
        }

        #endregion

        #region Constructors

        internal ManagedBitmapData128Rgba(in BitmapDataConfig cfg)
            : base(cfg)
        {
        }

        internal ManagedBitmapData128Rgba(Array2D<ColorF> buffer, in BitmapDataConfig cfg)
            : base(buffer, cfg)
        {
        }

        #endregion

        #region Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override Color32 DoGetColor32(int x, int y) => Buffer[y, x].ToColor32();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override void DoSetColor32(int x, int y, Color32 c) => Buffer[y, x] = new ColorF(c);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override Color64 DoGetColor64(int x, int y) => Buffer[y, x].ToColor64();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override void DoSetColor64(int x, int y, Color64 c) => Buffer[y, x] = new ColorF(c);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override PColor64 DoGetPColor64(int x, int y) => Buffer[y, x].ToPColor64();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override void DoSetPColor64(int x, int y, PColor64 c) => Buffer[y, x] = c.ToColorF();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override ColorF DoGetColorF(int x, int y) => Buffer[y, x];

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override void DoSetColorF(int x, int y, ColorF c) => Buffer[y, x] = c;

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override PColorF DoGetPColorF(int x, int y) => Buffer[y, x].ToPColorF();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override void DoSetPColorF(int x, int y, PColorF c) => Buffer[y, x] = c.ToColorF();

        #endregion
    }
}
