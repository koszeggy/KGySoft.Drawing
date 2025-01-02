#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ManagedBitmapData16Gray2D.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2025 - All Rights Reserved
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
    internal sealed class ManagedBitmapData16Gray2D<T> : ManagedBitmapData2DArrayBase<T, ManagedBitmapData16Gray2D<T>.Row>
        where T : unmanaged
    {
        #region Row class

        internal sealed class Row : ManagedBitmapDataRow2DBase<T>
        {
            #region Methods

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override Color32 DoGetColor32(int x) => DoReadRaw<Gray16>(x).ToColor32();

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetColor32(int x, Color32 c) => DoSetColor64(x, new Color64(c));

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetPColor32(int x, PColor32 c) => DoSetColor64(x, c.ToColor64());

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override Color64 DoGetColor64(int x) => DoReadRaw<Gray16>(x).ToColor64();

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetColor64(int x, Color64 c) => DoWriteRaw(x, BitmapData.LinearWorkingColorSpace
                ? new Gray16(c.A == UInt16.MaxValue ? c.ToColorF() : c.ToColorF().BlendWithBackgroundLinear(BitmapData.BackColor.ToColorF()))
                : new Gray16(c.A == UInt16.MaxValue ? c : c.BlendWithBackgroundSrgb(((ManagedBitmapData16Gray2D<T>)BitmapData).backColor64)));

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override PColor64 DoGetPColor64(int x) => PColor64.FromArgb(DoReadRaw<Gray16>(x).ToColor64().ToArgbUInt64());

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetPColor64(int x, PColor64 c) => DoSetColor64(x, c.ToColor64());

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override ColorF DoGetColorF(int x) => DoReadRaw<Gray16>(x).ToColor64().ToColorF();

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetColorF(int x, ColorF c) => DoWriteRaw(x, BitmapData.LinearWorkingColorSpace
                ? new Gray16(c.A >= 1f ? c : c.BlendWithBackgroundLinear(BitmapData.BackColor.ToColorF()))
                : new Gray16(c.A >= 1f ? c.ToColor64() : c.ToColor64().BlendWithBackgroundSrgb(((ManagedBitmapData16Gray2D<T>)BitmapData).backColor64)));

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override PColorF DoGetPColorF(int x) => DoReadRaw<Gray16>(x).ToColor64().ToPColorF();

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetPColorF(int x, PColorF c) => DoSetColorF(x, c.ToColorF());

            #endregion
        }

        #endregion

        #region Fields

        private readonly Color64 backColor64;

        #endregion

        #region Constructors

        internal ManagedBitmapData16Gray2D(T[,] buffer, in BitmapDataConfig cfg)
            : base(buffer, cfg)
        {
            backColor64 = BackColor.ToColor64();
        }

        #endregion

        #region Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override Color32 DoGetColor32(int x, int y) => GetPixelRef<Gray16>(y, x).ToColor32();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override void DoSetColor32(int x, int y, Color32 c) => DoSetColor64(x, y, new Color64(c));

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override PColor32 DoGetPColor32(int x, int y) => PColor32.FromArgb(GetPixelRef<Gray16>(y, x).ToColor32().ToArgbUInt32());

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override void DoSetPColor32(int x, int y, PColor32 c) => DoSetColor64(x, y, c.ToColor64());

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override Color64 DoGetColor64(int x, int y) => GetPixelRef<Gray16>(y, x).ToColor64();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override void DoSetColor64(int x, int y, Color64 c) => GetPixelRef<Gray16>(y, x) = LinearWorkingColorSpace
            ? new Gray16(c.A == UInt16.MaxValue ? c.ToColorF() : c.ToColorF().BlendWithBackgroundLinear(BackColor.ToColorF()))
            : new Gray16(c.A == UInt16.MaxValue ? c : c.BlendWithBackgroundSrgb(backColor64));

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override PColor64 DoGetPColor64(int x, int y) => PColor64.FromArgb(GetPixelRef<Gray16>(y, x).ToColor64().ToArgbUInt64());

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override void DoSetPColor64(int x, int y, PColor64 c) => DoSetColor64(x, y, c.ToColor64());

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override ColorF DoGetColorF(int x, int y) => GetPixelRef<Gray16>(y, x).ToColor64().ToColorF();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override void DoSetColorF(int x, int y, ColorF c) => GetPixelRef<Gray16>(y, x) = LinearWorkingColorSpace
            ? new Gray16(c.A >= 1f ? c : c.BlendWithBackgroundLinear(BackColor.ToColorF()))
            : new Gray16(c.A >= 1f ? c.ToColor64() : c.ToColor64().BlendWithBackgroundSrgb(backColor64));

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override PColorF DoGetPColorF(int x, int y) => GetPixelRef<Gray16>(y, x).ToColor64().ToPColorF();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override void DoSetPColorF(int x, int y, PColorF c) => DoSetColorF(x, y, c.ToColorF());

        #endregion
    }
}
