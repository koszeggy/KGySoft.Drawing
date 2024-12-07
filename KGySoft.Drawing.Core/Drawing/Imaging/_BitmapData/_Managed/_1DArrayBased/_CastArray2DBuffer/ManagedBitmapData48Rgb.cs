#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ManagedBitmapData48Rgb.cs
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

using KGySoft.Collections;

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal sealed class ManagedBitmapData48Rgb<T> : ManagedBitmapDataCastArray2DBase<T, Color48, ManagedBitmapData48Rgb<T>.Row>
        where T : unmanaged
    {
        #region Row class

        internal sealed class Row : ManagedBitmapDataCastArrayRowBase<T, Color48>
        {
            #region Methods

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override Color32 DoGetColor32(int x) => GetPixelRef(x).ToColor32();

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetColor32(int x, Color32 c) => DoSetColor64(x, new Color64(c));

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override PColor32 DoGetPColor32(int x) => PColor32.FromArgb(GetPixelRef(x).ToColor32().ToArgbUInt32());

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetPColor32(int x, PColor32 c) => DoSetColor64(x, c.ToColor64());

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override Color64 DoGetColor64(int x) => GetPixelRef(x).ToColor64();

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetColor64(int x, Color64 c)
                => GetPixelRef(x) = new Color48(c.A == UInt16.MaxValue ? c : c.BlendWithBackground(((ManagedBitmapData48Rgb<T>)BitmapData).backColor64, BitmapData.LinearWorkingColorSpace));

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override PColor64 DoGetPColor64(int x) => PColor64.FromArgb(GetPixelRef(x).ToColor64().ToArgbUInt64());

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetPColor64(int x, PColor64 c) => DoSetColor64(x, c.ToColor64());

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override ColorF DoGetColorF(int x) => GetPixelRef(x).ToColor64().ToColorF();

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetColorF(int x, ColorF c) => GetPixelRef(x) = new Color48(c.A >= 1f ? c.ToColor64()
                : BitmapData.LinearWorkingColorSpace ? c.BlendWithBackgroundLinear(BitmapData.BackColor.ToColorF()).ToColor64()
                : c.ToColor64().BlendWithBackgroundSrgb(((ManagedBitmapData48Rgb<T>)BitmapData).backColor64));

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override PColorF DoGetPColorF(int x) => GetPixelRef(x).ToColor64().ToPColorF();

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetPColorF(int x, PColorF c) => DoSetColorF(x, c.ToColorF());

            #endregion
        }

        #endregion

        #region Fields

        private readonly Color64 backColor64;

        #endregion

        #region Constructors

        internal ManagedBitmapData48Rgb(in BitmapDataConfig cfg)
            : base(cfg)
        {
            backColor64 = BackColor.ToColor64();
        }

        internal ManagedBitmapData48Rgb(CastArray2D<T, Color48> buffer, in BitmapDataConfig cfg)
            : base(buffer, cfg)
        {
            backColor64 = BackColor.ToColor64();
        }

        #endregion

        #region Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override Color32 DoGetColor32(int x, int y) => GetPixelRef(y, x).ToColor32();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override void DoSetColor32(int x, int y, Color32 c) => DoSetColor64(x, y, new Color64(c));

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override PColor32 DoGetPColor32(int x, int y) => PColor32.FromArgb(GetPixelRef(y, x).ToColor32().ToArgbUInt32());

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override void DoSetPColor32(int x, int y, PColor32 c) => DoSetColor64(x, y, c.ToColor64());

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override Color64 DoGetColor64(int x, int y) => GetPixelRef(y, x).ToColor64();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override void DoSetColor64(int x, int y, Color64 c)
            => GetPixelRef(y, x) = new Color48(c.A == UInt16.MaxValue ? c : c.BlendWithBackground(backColor64, LinearWorkingColorSpace));

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override PColor64 DoGetPColor64(int x, int y) => PColor64.FromArgb(GetPixelRef(y, x).ToColor64().ToArgbUInt64());

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override void DoSetPColor64(int x, int y, PColor64 c) => DoSetColor64(x, y, c.ToColor64());

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override ColorF DoGetColorF(int x, int y) => GetPixelRef(y, x).ToColor64().ToColorF();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override void DoSetColorF(int x, int y, ColorF c) => GetPixelRef(y, x) = new Color48(c.A >= 1f ? c.ToColor64()
            : LinearWorkingColorSpace ? c.BlendWithBackgroundLinear(BackColor.ToColorF()).ToColor64()
            : c.ToColor64().BlendWithBackgroundSrgb(backColor64));

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override PColorF DoGetPColorF(int x, int y) => GetPixelRef(y, x).ToColor64().ToPColorF();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override void DoSetPColorF(int x, int y, PColorF c) => DoSetColorF(x, y, c.ToColorF());

        #endregion
    }
}
