﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ManagedBitmapData32Gray.cs
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

using System;
using System.Runtime.CompilerServices;

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal sealed class ManagedBitmapData32Gray : ManagedBitmapData1DArrayBase<GrayF, ManagedBitmapData32Gray.Row>
    {
        #region Row class

        internal sealed class Row : ManagedBitmapDataRowBase<GrayF>
        {
            #region Methods

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override Color32 DoGetColor32(int x) => Row[x].ToColor32();

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetColor32(int x, Color32 c) => Row[x] = BitmapData.LinearWorkingColorSpace
                ? new GrayF(c.A == Byte.MaxValue ? c.ToColorF() : c.ToColorF().BlendWithBackgroundLinear(((ManagedBitmapData32Gray)BitmapData).backColorF))
                : new GrayF(c.A == Byte.MaxValue ? c : c.BlendWithBackgroundSrgb(BitmapData.BackColor));

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override Color64 DoGetColor64(int x) => Row[x].ToColor64();

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetColor64(int x, Color64 c) => Row[x] = BitmapData.LinearWorkingColorSpace
                ? new GrayF(c.A == UInt16.MaxValue ? c.ToColorF() : c.ToColorF().BlendWithBackgroundLinear(((ManagedBitmapData32Gray)BitmapData).backColorF))
                : new GrayF(c.A == UInt16.MaxValue ? c : c.BlendWithBackgroundSrgb(BitmapData.BackColor.ToColor64()));

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override PColor64 DoGetPColor64(int x) => PColor64.FromArgb(Row[x].ToColor64().ToArgbUInt64());

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetPColor64(int x, PColor64 c) => DoSetColor64(x, c.ToColor64());

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override ColorF DoGetColorF(int x) => Row[x].ToColorF();

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetColorF(int x, ColorF c) => Row[x] = BitmapData.LinearWorkingColorSpace
                ? new GrayF(c.A >= 1f ? c : c.BlendWithBackgroundLinear(((ManagedBitmapData32Gray)BitmapData).backColorF))
                : new GrayF(c.A >= 1f ? c.ToColor64() : c.ToColor64().BlendWithBackgroundSrgb(BitmapData.BackColor.ToColor64()));

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override PColorF DoGetPColorF(int x) => Row[x].ToColorF().ToPColorF();

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetPColorF(int x, PColorF c) => DoSetColorF(x, c.ToColorF());

            #endregion
        }

        #endregion

        #region Fields

        private readonly ColorF backColorF;

        #endregion

        #region Constructors

        internal ManagedBitmapData32Gray(in BitmapDataConfig cfg)
            : base(cfg)
        {
            backColorF = BackColor.ToColorF();
        }

        #endregion

        #region Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override Color32 DoGetColor32(int x, int y) => Buffer[y, x].ToColor32();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override void DoSetColor32(int x, int y, Color32 c) => Buffer[y, x] = LinearWorkingColorSpace
            ? new GrayF(c.A == Byte.MaxValue ? c.ToColorF() : c.ToColorF().BlendWithBackgroundLinear(backColorF))
            : new GrayF(c.A == Byte.MaxValue ? c : c.BlendWithBackgroundSrgb(BackColor));

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override Color64 DoGetColor64(int x, int y) => Buffer[y, x].ToColor64();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override void DoSetColor64(int x, int y, Color64 c) => Buffer[y, x] = LinearWorkingColorSpace
            ? new GrayF(c.A == UInt16.MaxValue ? c.ToColorF() : c.ToColorF().BlendWithBackgroundLinear(backColorF))
            : new GrayF(c.A == UInt16.MaxValue ? c : c.BlendWithBackgroundSrgb(BackColor.ToColor64()));

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override PColor64 DoGetPColor64(int x, int y) => PColor64.FromArgb(Buffer[y, x].ToColor64().ToArgbUInt64());

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override void DoSetPColor64(int x, int y, PColor64 c) => DoSetColor64(x, y, c.ToColor64());

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override ColorF DoGetColorF(int x, int y) => Buffer[y, x].ToColorF();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override void DoSetColorF(int x, int y, ColorF c) => Buffer[y, x] = LinearWorkingColorSpace
            ? new GrayF(c.A >= 1f ? c : c.BlendWithBackgroundLinear(backColorF))
            : new GrayF(c.A >= 1f ? c.ToColor64() : c.ToColor64().BlendWithBackgroundSrgb(BackColor.ToColor64()));

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override PColorF DoGetPColorF(int x, int y) => Buffer[y, x].ToColorF().ToPColorF();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override void DoSetPColorF(int x, int y, PColorF c) => DoSetColorF(x, y, c.ToColorF());

        #endregion
    }
}
