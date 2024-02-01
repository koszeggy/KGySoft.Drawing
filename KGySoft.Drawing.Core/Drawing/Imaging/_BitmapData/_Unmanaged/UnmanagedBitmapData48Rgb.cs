#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: UnmanagedBitmapData48Rgb.cs
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
    internal sealed class UnmanagedBitmapData48Rgb : UnmanagedBitmapDataBase<UnmanagedBitmapData48Rgb.Row>
    {
        #region Row class

        internal sealed class Row : UnmanagedBitmapDataRowBase
        {
            #region Methods

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override unsafe Color32 DoGetColor32(int x) => ((Color48*)Row)[x].ToColor32();

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetColor32(int x, Color32 c) => DoSetColor64(x, new Color64(c));

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override unsafe PColor32 DoGetPColor32(int x) => PColor32.FromArgb(((Color48*)Row)[x].ToColor32().ToArgbUInt32());

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetPColor32(int x, PColor32 c) => DoSetColor64(x, c.ToColor64());

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override unsafe Color64 DoGetColor64(int x) => ((Color48*)Row)[x].ToColor64();

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override unsafe void DoSetColor64(int x, Color64 c)
                => ((Color48*)Row)[x] = new Color48(c.A == UInt16.MaxValue ? c : c.BlendWithBackground(((UnmanagedBitmapData48Rgb)BitmapData).backColor64, BitmapData.LinearWorkingColorSpace));

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override unsafe PColor64 DoGetPColor64(int x) => PColor64.FromArgb(((Color48*)Row)[x].ToColor64().ToArgbUInt64());

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetPColor64(int x, PColor64 c) => DoSetColor64(x, c.ToColor64());

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override unsafe ColorF DoGetColorF(int x) => ((Color48*)Row)[x].ToColor64().ToColorF();

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override unsafe void DoSetColorF(int x, ColorF c) => ((Color48*)Row)[x] = new Color48(c.A >= 1f ? c.ToColor64()
                : BitmapData.LinearWorkingColorSpace ? c.BlendWithBackgroundLinear(BitmapData.BackColor.ToColorF()).ToColor64()
                : c.ToColor64().BlendWithBackgroundSrgb(((UnmanagedBitmapData48Rgb)BitmapData).backColor64));

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override unsafe PColorF DoGetPColorF(int x) => ((Color48*)Row)[x].ToColor64().ToPColorF();

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetPColorF(int x, PColorF c) => DoSetColorF(x, c.ToColorF());

            #endregion
        }

        #endregion

        #region Fields

        private readonly Color64 backColor64;

        #endregion

        #region Constructors

        internal UnmanagedBitmapData48Rgb(IntPtr buffer, int stride, in BitmapDataConfig cfg)
            : base(buffer, stride, cfg)
        {
            backColor64 = BackColor.ToColor64();
        }

        #endregion

        #region Methods

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override unsafe Color32 DoGetColor32(int x, int y) => GetPixelAddress<Color48>(y, x)->ToColor32();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override void DoSetColor32(int x, int y, Color32 c) => DoSetColor64(x, y, new Color64(c));

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override unsafe PColor32 DoGetPColor32(int x, int y) => PColor32.FromArgb(GetPixelAddress<Color48>(y, x)->ToColor32().ToArgbUInt32());

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override void DoSetPColor32(int x, int y, PColor32 c) => DoSetColor64(x, y, c.ToColor64());

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override unsafe Color64 DoGetColor64(int x, int y) => GetPixelAddress<Color48>(y, x)->ToColor64();

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override unsafe void DoSetColor64(int x, int y, Color64 c)
            => *GetPixelAddress<Color48>(y, x) = new Color48(c.A == UInt16.MaxValue ? c : c.BlendWithBackground(backColor64, LinearWorkingColorSpace));

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override unsafe PColor64 DoGetPColor64(int x, int y) => PColor64.FromArgb(GetPixelAddress<Color48>(y, x)->ToColor64().ToArgbUInt64());

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override void DoSetPColor64(int x, int y, PColor64 c) => DoSetColor64(x, y, c.ToColor64());

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override unsafe ColorF DoGetColorF(int x, int y) => GetPixelAddress<Color48>(y, x)->ToColor64().ToColorF();

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override unsafe void DoSetColorF(int x, int y, ColorF c) => *GetPixelAddress<Color48>(y, x) = new Color48(c.A >= 1f ? c.ToColor64()
            : LinearWorkingColorSpace ? c.BlendWithBackgroundLinear(BackColor.ToColorF()).ToColor64()
            : c.ToColor64().BlendWithBackgroundSrgb(backColor64));

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override unsafe PColorF DoGetPColorF(int x, int y) => GetPixelAddress<Color48>(y, x)->ToColor64().ToPColorF();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override void DoSetPColorF(int x, int y, PColorF c) => DoSetColorF(x, y, c.ToColorF());

        #endregion
    }
}
