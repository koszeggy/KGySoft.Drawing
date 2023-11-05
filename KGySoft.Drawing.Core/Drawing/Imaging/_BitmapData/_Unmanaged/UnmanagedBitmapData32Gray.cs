#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: UnmanagedBitmapData32Gray.cs
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

using System;
using System.Runtime.CompilerServices;
using System.Security;

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal sealed class UnmanagedBitmapData32Gray : UnmanagedBitmapDataBase<UnmanagedBitmapData32Gray.Row>
    {
        #region Row class

        internal sealed class Row : UnmanagedBitmapDataRowBase
        {
            #region Methods

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override unsafe Color32 DoGetColor32(int x) => ((GrayF*)Row)[x].ToColor32();

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override unsafe void DoSetColor32(int x, Color32 c) => ((GrayF*)Row)[x] = BitmapData.LinearWorkingColorSpace
                ? new GrayF(c.A == Byte.MaxValue ? c.ToColorF() : c.ToColorF().BlendWithBackgroundLinear(((UnmanagedBitmapData32Gray)BitmapData).backColorF))
                : new GrayF(c.A == Byte.MaxValue ? c : c.BlendWithBackgroundSrgb(BitmapData.BackColor));

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override unsafe Color64 DoGetColor64(int x) => ((GrayF*)Row)[x].ToColor64();

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override unsafe void DoSetColor64(int x, Color64 c) => ((GrayF*)Row)[x] = BitmapData.LinearWorkingColorSpace
                ? new GrayF(c.A == UInt16.MaxValue ? c.ToColorF() : c.ToColorF().BlendWithBackgroundLinear(((UnmanagedBitmapData32Gray)BitmapData).backColorF))
                : new GrayF(c.A == UInt16.MaxValue ? c : c.BlendWithBackgroundSrgb(BitmapData.BackColor.ToColor64()));

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override unsafe PColor64 DoGetPColor64(int x) => PColor64.FromArgb(((GrayF*)Row)[x].ToColor64().ToArgbUInt64());

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetPColor64(int x, PColor64 c) => DoSetColor64(x, c.ToColor64());

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override unsafe ColorF DoGetColorF(int x) => ((GrayF*)Row)[x].ToColor64().ToColorF();

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override unsafe void DoSetColorF(int x, ColorF c) => ((GrayF*)Row)[x] = BitmapData.LinearWorkingColorSpace
                ? new GrayF(c.A >= 1f ? c : c.BlendWithBackgroundLinear(((UnmanagedBitmapData32Gray)BitmapData).backColorF))
                : new GrayF(c.A >= 1f ? c.ToColor64() : c.ToColor64().BlendWithBackgroundSrgb(BitmapData.BackColor.ToColor64()));

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override unsafe PColorF DoGetPColorF(int x) => ((GrayF*)Row)[x].ToColorF().ToPColorF();

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetPColorF(int x, PColorF c) => DoSetColorF(x, c.ToColorF());

            #endregion
        }

        #endregion

        #region Fields

        private readonly ColorF backColorF;

        #endregion

        #region Constructors

        internal UnmanagedBitmapData32Gray(IntPtr buffer, int stride, in BitmapDataConfig cfg)
            : base(buffer, stride, cfg)
        {
            backColorF = BackColor.ToColorF();
        }

        #endregion

        #region Methods

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override unsafe Color32 DoGetColor32(int x, int y) => GetPixelAddress<GrayF>(y, x)->ToColor32();

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override unsafe void DoSetColor32(int x, int y, Color32 c) => *GetPixelAddress<GrayF>(y, x) = LinearWorkingColorSpace
            ? new GrayF(c.A == Byte.MaxValue ? c.ToColorF() : c.ToColorF().BlendWithBackgroundLinear(backColorF))
            : new GrayF(c.A == Byte.MaxValue ? c : c.BlendWithBackgroundSrgb(BackColor));

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override unsafe Color64 DoGetColor64(int x, int y) => GetPixelAddress<GrayF>(y, x)->ToColor64();

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override unsafe void DoSetColor64(int x, int y, Color64 c) => *GetPixelAddress<GrayF>(y, x) = LinearWorkingColorSpace
            ? new GrayF(c.A == UInt16.MaxValue ? c.ToColorF() : c.ToColorF().BlendWithBackgroundLinear(backColorF))
            : new GrayF(c.A == UInt16.MaxValue ? c : c.BlendWithBackgroundSrgb(BackColor.ToColor64()));

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override unsafe PColor64 DoGetPColor64(int x, int y) => PColor64.FromArgb(GetPixelAddress<GrayF>(y, x)->ToColor64().ToArgbUInt64());

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override void DoSetPColor64(int x, int y, PColor64 c) => DoSetColor64(x, y, c.ToColor64());

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override unsafe ColorF DoGetColorF(int x, int y) => GetPixelAddress<GrayF>(y, x)->ToColor64().ToColorF();

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override unsafe void DoSetColorF(int x, int y, ColorF c) => *GetPixelAddress<GrayF>(y, x) = LinearWorkingColorSpace
            ? new GrayF(c.A >= 1f ? c : c.BlendWithBackgroundLinear(backColorF))
            : new GrayF(c.A >= 1f ? c.ToColor64() : c.ToColor64().BlendWithBackgroundSrgb(BackColor.ToColor64()));

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override unsafe PColorF DoGetPColorF(int x, int y) => GetPixelAddress<GrayF>(y, x)->ToColorF().ToPColorF();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override void DoSetPColorF(int x, int y, PColorF c) => DoSetColorF(x, y, c.ToColorF());

        #endregion
    }
}
