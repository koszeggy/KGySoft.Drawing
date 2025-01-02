#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ManagedBitmapData24Rgb.cs
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

using KGySoft.Collections;

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal sealed class ManagedBitmapData24Rgb<T> : ManagedBitmapDataCastArray2DBase<T, Color24, ManagedBitmapData24Rgb<T>.Row>
        where T : unmanaged
    {
        #region Row class

        internal sealed class Row : ManagedBitmapDataCastArrayRowBase<T, Color24>
        {
            #region Methods

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override Color32 DoGetColor32(int x) => GetPixelRef(x).ToColor32();

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetColor32(int x, Color32 c)
                => GetPixelRef(x) = new Color24(c.A == Byte.MaxValue ? c : c.BlendWithBackground(BitmapData.BackColor, BitmapData.LinearWorkingColorSpace));

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetColorF(int x, ColorF c) => GetPixelRef(x) = new Color24(c.A >= 1f ? c.ToColor32()
                : BitmapData.LinearWorkingColorSpace ? c.BlendWithBackgroundLinear(BitmapData.BackColor.ToColorF()).ToColor32()
                : c.ToColor32().BlendWithBackgroundSrgb(BitmapData.BackColor));

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetPColorF(int x, PColorF c) => DoSetColorF(x, c.ToColorF());

            #endregion
        }

        #endregion

        #region Constructors

        internal ManagedBitmapData24Rgb(in BitmapDataConfig cfg)
            : base(cfg)
        {
        }

        internal ManagedBitmapData24Rgb(CastArray2D<T, Color24> buffer, in BitmapDataConfig cfg)
            : base(buffer, cfg)
        {
        }

        #endregion

        #region Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override Color32 DoGetColor32(int x, int y) => GetPixelRef(y, x).ToColor32();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override void DoSetColor32(int x, int y, Color32 c)
            => GetPixelRef(y, x) = new Color24(c.A == Byte.MaxValue ? c : c.BlendWithBackground(BackColor, LinearWorkingColorSpace));

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override void DoSetColorF(int x, int y, ColorF c) => GetPixelRef(y, x) = new Color24(c.A >= 1f ? c.ToColor32()
            : LinearWorkingColorSpace ? c.BlendWithBackgroundLinear(BackColor.ToColorF()).ToColor32()
            : c.ToColor32().BlendWithBackgroundSrgb(BackColor));

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override void DoSetPColorF(int x, int y, PColorF c) => DoSetColorF(x, y, c.ToColorF());

        #endregion
    }
}
