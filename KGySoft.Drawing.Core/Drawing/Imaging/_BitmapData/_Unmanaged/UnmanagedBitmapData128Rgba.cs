#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: UnmanagedBitmapData128Rgba.cs
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
using System.Security;

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal sealed class UnmanagedBitmapData128Rgba : UnmanagedBitmapDataBase<UnmanagedBitmapData128Rgba.Row>
    {
        #region Row class

        internal sealed class Row : UnmanagedBitmapDataRowBase
        {
            #region Methods

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override unsafe Color32 DoGetColor32(int x) => ((ColorF*)Row)[x].ToColor32();

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override unsafe void DoSetColor32(int x, Color32 c) => ((ColorF*)Row)[x] = new ColorF(c);

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override unsafe Color64 DoGetColor64(int x) => ((ColorF*)Row)[x].ToColor64();

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override unsafe void DoSetColor64(int x, Color64 c) => ((ColorF*)Row)[x] = new ColorF(c);

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override unsafe PColor64 DoGetPColor64(int x) => ((ColorF*)Row)[x].ToPColor64();

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override unsafe void DoSetPColor64(int x, PColor64 c) => ((ColorF*)Row)[x] = c.ToColorF();

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override unsafe ColorF DoGetColorF(int x) => ((ColorF*)Row)[x];

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override unsafe void DoSetColorF(int x, ColorF c) => ((ColorF*)Row)[x] = c;

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override unsafe PColorF DoGetPColorF(int x) => ((ColorF*)Row)[x].ToPColorF();

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override unsafe void DoSetPColorF(int x, PColorF c) => ((ColorF*)Row)[x] = c.ToColorF();

            #endregion
        }

        #endregion

        #region Constructors

        internal UnmanagedBitmapData128Rgba(IntPtr buffer, int stride, in BitmapDataConfig cfg)
            : base(buffer, stride, cfg)
        {
        }

        #endregion

        #region Methods

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override unsafe Color32 DoGetColor32(int x, int y) => GetPixelAddress<ColorF>(y, x)->ToColor32();

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override unsafe void DoSetColor32(int x, int y, Color32 c) => *GetPixelAddress<ColorF>(y, x) = new ColorF(c);

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override unsafe Color64 DoGetColor64(int x, int y) => GetPixelAddress<ColorF>(y, x)->ToColor64();

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override unsafe void DoSetColor64(int x, int y, Color64 c) => *GetPixelAddress<ColorF>(y, x) = new ColorF(c);

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override unsafe PColor64 DoGetPColor64(int x, int y) => GetPixelAddress<ColorF>(y, x)->ToPColor64();

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override unsafe void DoSetPColor64(int x, int y, PColor64 c) => *GetPixelAddress<ColorF>(y, x) = c.ToColorF();

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override unsafe ColorF DoGetColorF(int x, int y) => *GetPixelAddress<ColorF>(y, x);

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override unsafe void DoSetColorF(int x, int y, ColorF c) => *GetPixelAddress<ColorF>(y, x) = c;

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override unsafe PColorF DoGetPColorF(int x, int y) => GetPixelAddress<ColorF>(y, x)->ToPColorF();

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override unsafe void DoSetPColorF(int x, int y, PColorF c) => *GetPixelAddress<ColorF>(y, x) = c.ToColorF();

        #endregion
    }
}
