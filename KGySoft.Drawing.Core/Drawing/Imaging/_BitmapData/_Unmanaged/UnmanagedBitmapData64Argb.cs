#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: UnmanagedBitmapData64Argb.cs
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
    internal sealed class UnmanagedBitmapData64Argb : UnmanagedBitmapDataBase<UnmanagedBitmapData64Argb.Row>
    {
        #region Row class

        internal sealed class Row : UnmanagedBitmapDataRowBase
        {
            #region Methods

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override unsafe Color32 DoGetColor32(int x) => ((Color64*)Row)[x].ToColor32();

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override unsafe void DoSetColor32(int x, Color32 c) => ((Color64*)Row)[x] = new Color64(c);

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override unsafe Color64 DoGetColor64(int x) => ((Color64*)Row)[x];

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override unsafe void DoSetColor64(int x, Color64 c) => ((Color64*)Row)[x] = c;

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override unsafe PColor64 DoGetPColor64(int x) => ((Color64*)Row)[x].ToPColor64();

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override unsafe void DoSetPColor64(int x, PColor64 c) => ((Color64*)Row)[x] = c.ToColor64();

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override unsafe ColorF DoGetColorF(int x) => ((Color64*)Row)[x].ToColorF();

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override unsafe void DoSetColorF(int x, ColorF c) => ((Color64*)Row)[x] = c.ToColor64();

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override unsafe PColorF DoGetPColorF(int x) => ((Color64*)Row)[x].ToPColorF();

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override unsafe void DoSetPColorF(int x, PColorF c) => ((Color64*)Row)[x] = c.ToColor64();

            #endregion
        }

        #endregion

        #region Constructors

        internal UnmanagedBitmapData64Argb(IntPtr buffer, int stride, in BitmapDataConfig cfg)
            : base(buffer, stride, cfg)
        {
        }

        #endregion

        #region Methods

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override unsafe Color32 DoGetColor32(int x, int y) => GetPixelAddress<Color64>(y, x)->ToColor32();

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override unsafe void DoSetColor32(int x, int y, Color32 c) => *GetPixelAddress<Color64>(y, x) = new Color64(c);

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override unsafe Color64 DoGetColor64(int x, int y) => *GetPixelAddress<Color64>(y, x);

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override unsafe void DoSetColor64(int x, int y, Color64 c) => *GetPixelAddress<Color64>(y, x) = c;

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override unsafe PColor64 DoGetPColor64(int x, int y) => GetPixelAddress<Color64>(y, x)->ToPColor64();

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override unsafe void DoSetPColor64(int x, int y, PColor64 c) => *GetPixelAddress<Color64>(y, x) = c.ToColor64();

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override unsafe ColorF DoGetColorF(int x, int y) => GetPixelAddress<Color64>(y, x)->ToColorF();

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override unsafe void DoSetColorF(int x, int y, ColorF c) => *GetPixelAddress<Color64>(y, x) = c.ToColor64();

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override unsafe PColorF DoGetPColorF(int x, int y) => GetPixelAddress<Color64>(y, x)->ToPColorF();

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override unsafe void DoSetPColorF(int x, int y, PColorF c) => *GetPixelAddress<Color64>(y, x) = c.ToColor64();

        #endregion
    }
}
