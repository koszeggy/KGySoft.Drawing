#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: UnmanagedBitmapData128PRgba.cs
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
    internal sealed class UnmanagedBitmapData128PRgba : UnmanagedBitmapDataBase<UnmanagedBitmapData128PRgba.Row>
    {
        #region Row class

        internal sealed class Row : UnmanagedBitmapDataRowBase
        {
            #region Methods

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override unsafe Color32 DoGetColor32(int x) => ((PColorF*)Row)[x].ToColor32();

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override unsafe void DoSetColor32(int x, Color32 c) => ((PColorF*)Row)[x] = new PColorF(c);

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override unsafe Color64 DoGetColor64(int x) => ((PColorF*)Row)[x].ToColor64();

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override unsafe void DoSetColor64(int x, Color64 c) => ((PColorF*)Row)[x] = new PColorF(c);

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override unsafe PColor64 DoGetPColor64(int x) => ((PColorF*)Row)[x].ToPColor64();

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override unsafe void DoSetPColor64(int x, PColor64 c) => ((PColorF*)Row)[x] = c.ToPColorF();

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override unsafe ColorF DoGetColorF(int x) => ((PColorF*)Row)[x].ToColorF();

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override unsafe void DoSetColorF(int x, ColorF c) => ((PColorF*)Row)[x] = new PColorF(c);

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override unsafe PColorF DoGetPColorF(int x) => ((PColorF*)Row)[x];

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override unsafe void DoSetPColorF(int x, PColorF c) => ((PColorF*)Row)[x] = c;

            #endregion
        }

        #endregion

        #region Constructors

        internal UnmanagedBitmapData128PRgba(IntPtr buffer, int stride, in BitmapDataConfig cfg)
            : base(buffer, stride, cfg)
        {
        }

        #endregion

        #region Methods

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override unsafe Color32 DoGetColor32(int x, int y) => GetPixelAddress<PColorF>(y, x)->ToColor32();

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override unsafe void DoSetColor32(int x, int y, Color32 c) => *GetPixelAddress<PColorF>(y, x) = new PColorF(c);

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override unsafe Color64 DoGetColor64(int x, int y) => GetPixelAddress<PColorF>(y, x)->ToColor64();

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override unsafe void DoSetColor64(int x, int y, Color64 c) => *GetPixelAddress<PColorF>(y, x) = new PColorF(c);

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override unsafe PColor64 DoGetPColor64(int x, int y) => GetPixelAddress<PColorF>(y, x)->ToPColor64();

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override unsafe void DoSetPColor64(int x, int y, PColor64 c) => *GetPixelAddress<PColorF>(y, x) = c.ToPColorF();

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override unsafe ColorF DoGetColorF(int x, int y) => GetPixelAddress<PColorF>(y, x)->ToColorF();

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override unsafe void DoSetColorF(int x, int y, ColorF c) => *GetPixelAddress<PColorF>(y, x) = new PColorF(c);

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override unsafe PColorF DoGetPColorF(int x, int y) => *GetPixelAddress<PColorF>(y, x);

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override unsafe void DoSetPColorF(int x, int y, PColorF c) => *GetPixelAddress<PColorF>(y, x) = c;

        #endregion
    }
}
