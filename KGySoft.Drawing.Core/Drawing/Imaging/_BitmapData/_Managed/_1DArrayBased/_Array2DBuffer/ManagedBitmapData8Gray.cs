#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ManagedBitmapData8Gray.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2026 - All Rights Reserved
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
using System.Security;

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal sealed class ManagedBitmapData8Gray : ManagedBitmapDataArray2DBase<Gray8, ManagedBitmapData8Gray.Row>
    {
        #region Row class

        internal sealed class Row : ManagedBitmapDataArraySectionRowBase<Gray8>
        {
            #region Methods

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override Color32 DoGetColor32(int x) => GetPixelRef(x).ToColor32();

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetColor32(int x, Color32 c) => GetPixelRef(x) = BitmapData.LinearWorkingColorSpace
                ? new Gray8(c.A == Byte.MaxValue ? c.ToColorF() : c.ToColorF().BlendWithBackgroundLinear(BitmapData.BackColor.ToColorF()))
                : new Gray8(c.A == Byte.MaxValue ? c : c.BlendWithBackgroundSrgb(BitmapData.BackColor));

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetColorF(int x, ColorF c) => GetPixelRef(x) = BitmapData.LinearWorkingColorSpace
                ? new Gray8(c.A >= 1f ? c : c.BlendWithBackgroundLinear(BitmapData.BackColor.ToColorF()))
                : new Gray8(c.A >= 1f ? c.ToColor32() : c.ToColor32().BlendWithBackgroundSrgb(BitmapData.BackColor));

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetPColorF(int x, PColorF c) => DoSetColor64(x, c.ToColor64());

            #endregion
        }

        #endregion

        #region Constructors

        internal ManagedBitmapData8Gray(in BitmapDataConfig cfg)
            : base(cfg)
        {
        }

        #endregion

        #region Methods

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override Color32 DoGetColor32(int x, int y) => GetPixelRef(y, x).ToColor32();

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override void DoSetColor32(int x, int y, Color32 c) => GetPixelRef(y, x) = LinearWorkingColorSpace
            ? new Gray8(c.A == Byte.MaxValue ? c.ToColorF() : c.ToColorF().BlendWithBackgroundLinear(BackColor.ToColorF()))
            : new Gray8(c.A == Byte.MaxValue ? c : c.BlendWithBackgroundSrgb(BackColor));

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override void DoSetColorF(int x, int y, ColorF c) => GetPixelRef(y, x) = LinearWorkingColorSpace
            ? new Gray8(c.A >= 1f ? c : c.BlendWithBackgroundLinear(BackColor.ToColorF()))
            : new Gray8(c.A >= 1f ? c.ToColor32() : c.ToColor32().BlendWithBackgroundSrgb(BackColor));

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override void DoSetPColorF(int x, int y, PColorF c) => DoSetColorF(x, y, c.ToColorF());

        #endregion
    }
}
