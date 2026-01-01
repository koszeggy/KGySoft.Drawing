#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: UnmanagedBitmapData96Rgb.cs
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
    internal sealed class UnmanagedBitmapData96Rgb : UnmanagedBitmapDataBase<UnmanagedBitmapData96Rgb.Row>
    {
        #region Row class

        internal sealed class Row : UnmanagedBitmapDataRowBase
        {
            #region Methods

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override unsafe Color32 DoGetColor32(int x) => ((RgbF*)Row)[x].ToColor32();

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override unsafe void DoSetColor32(int x, Color32 c)
                => ((RgbF*)Row)[x] = c.A == Byte.MaxValue
                    ? new RgbF(c)
                    : new RgbF(c.ToColorF().BlendWithBackground(((UnmanagedBitmapData96Rgb)BitmapData).backColorF, BitmapData.LinearWorkingColorSpace));

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override unsafe Color64 DoGetColor64(int x) => ((RgbF*)Row)[x].ToColor64();

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override unsafe void DoSetColor64(int x, Color64 c)
                => ((RgbF*)Row)[x] = c.A == UInt16.MaxValue
                    ? new RgbF(c)
                    : new RgbF(c.ToColorF().BlendWithBackground(((UnmanagedBitmapData96Rgb)BitmapData).backColorF, BitmapData.LinearWorkingColorSpace));

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override unsafe PColor64 DoGetPColor64(int x) => ((RgbF*)Row)[x].ToColor64().ToPColor64();

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetPColor64(int x, PColor64 c) => DoSetColorF(x, c.ToColorF());

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override unsafe ColorF DoGetColorF(int x) => ((RgbF*)Row)[x].ToColorF();

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override unsafe void DoSetColorF(int x, ColorF c)
                => ((RgbF*)Row)[x] = c.A >= 1f
                    ? new RgbF(c)
                    : new RgbF(c.BlendWithBackground(((UnmanagedBitmapData96Rgb)BitmapData).backColorF, BitmapData.LinearWorkingColorSpace));

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override unsafe PColorF DoGetPColorF(int x) => ((RgbF*)Row)[x].ToColorF().ToPColorF();

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetPColorF(int x, PColorF c) => DoSetColorF(x, c.ToColorF());

            #endregion
        }

        #endregion

        #region Fields

        private readonly ColorF backColorF;

        #endregion

        #region Constructors

        internal UnmanagedBitmapData96Rgb(IntPtr buffer, int stride, in BitmapDataConfig cfg)
            : base(buffer, stride, cfg)
        {
            backColorF = BackColor.ToColorF();
        }

        #endregion

        #region Methods

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override unsafe Color32 DoGetColor32(int x, int y) => GetPixelAddress<RgbF>(y, x)->ToColor32();

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override unsafe void DoSetColor32(int x, int y, Color32 c)
            => *GetPixelAddress<RgbF>(y, x) = c.A == Byte.MaxValue
                ? new RgbF(c)
                : new RgbF(c.ToColorF().BlendWithBackground(backColorF, LinearWorkingColorSpace));

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override unsafe Color64 DoGetColor64(int x, int y) => GetPixelAddress<RgbF>(y, x)->ToColor64();

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override unsafe void DoSetColor64(int x, int y, Color64 c)
            => *GetPixelAddress<RgbF>(y, x) = c.A == UInt16.MaxValue
                ? new RgbF(c)
                : new RgbF(c.ToColorF().BlendWithBackground(backColorF, LinearWorkingColorSpace));

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override unsafe PColor64 DoGetPColor64(int x, int y) => GetPixelAddress<RgbF>(y, x)->ToColor64().ToPColor64();

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override void DoSetPColor64(int x, int y, PColor64 c) => DoSetColorF(x, y, c.ToColorF());

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override unsafe ColorF DoGetColorF(int x, int y) => GetPixelAddress<RgbF>(y, x)->ToColorF();

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override unsafe void DoSetColorF(int x, int y, ColorF c)
            => *GetPixelAddress<RgbF>(y, x) = c.A >= 1f
                ? new RgbF(c)
                : new RgbF(c.BlendWithBackground(backColorF, LinearWorkingColorSpace));

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override unsafe PColorF DoGetPColorF(int x, int y) => GetPixelAddress<RgbF>(y, x)->ToColorF().ToPColorF();

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override void DoSetPColorF(int x, int y, PColorF c) => DoSetColorF(x, y, c.ToColorF());

        #endregion
    }
}
