#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: UnmanagedBitmapData24Rgb.cs
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
using System.Security;

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal sealed class UnmanagedBitmapData24Rgb : UnmanagedBitmapDataBase<UnmanagedBitmapData24Rgb.Row>
    {
        #region Row class

        internal sealed class Row : UnmanagedBitmapDataRowBase
        {
            #region Methods

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override unsafe Color32 DoGetColor32(int x) => ((Color24*)Row)[x].ToColor32();

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override unsafe void DoSetColor32(int x, Color32 c)
                => ((Color24*)Row)[x] = new Color24(c.A == Byte.MaxValue ? c : c.BlendWithBackground(BitmapData.BackColor, BitmapData.LinearWorkingColorSpace));

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override unsafe void DoSetColorF(int x, ColorF c) => ((Color24*)Row)[x] = new Color24(c.A >= 1f ? c.ToColor32()
                : BitmapData.LinearWorkingColorSpace ? c.BlendWithBackgroundLinear(BitmapData.BackColor.ToColorF()).ToColor32()
                : c.ToColor32().BlendWithBackgroundSrgb(BitmapData.BackColor));

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetPColorF(int x, PColorF c) => DoSetColorF(x, c.ToColorF());

            #endregion
        }

        #endregion

        #region Constructors

        internal UnmanagedBitmapData24Rgb(IntPtr buffer, int stride, in BitmapDataConfig cfg)
            : base(buffer, stride, cfg)
        {
        }

        #endregion

        #region Methods

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override unsafe Color32 DoGetColor32(int x, int y) => GetPixelAddress<Color24>(y, x)->ToColor32();

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override unsafe void DoSetColor32(int x, int y, Color32 c)
            => *GetPixelAddress<Color24>(y, x) = new Color24(c.A == Byte.MaxValue ? c : c.BlendWithBackground(BackColor, LinearWorkingColorSpace));

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override unsafe void DoSetColorF(int x, int y, ColorF c) => *GetPixelAddress<Color24>(y, x) = new Color24(c.A >= 1f ? c.ToColor32()
            : LinearWorkingColorSpace ? c.BlendWithBackgroundLinear(BackColor.ToColorF()).ToColor32()
            : c.ToColor32().BlendWithBackgroundSrgb(BackColor));

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override void DoSetPColorF(int x, int y, PColorF c) => DoSetColorF(x, y, c.ToColorF());

        #endregion
    }
}
