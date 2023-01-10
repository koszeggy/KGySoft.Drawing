#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: UnmanagedBitmapData16Gray.cs
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
    internal sealed class UnmanagedBitmapData16Gray : UnmanagedBitmapDataBase<UnmanagedBitmapData16Gray.Row>
    {
        #region Row class

        internal sealed class Row : UnmanagedBitmapDataRowBase
        {
            #region Methods

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override unsafe Color32 DoGetColor32(int x) => ((Color16Gray*)Row)[x].ToColor32();

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override unsafe void DoSetColor32(int x, Color32 c)
                => ((Color16Gray*)Row)[x] = new Color16Gray(c.A == Byte.MaxValue ? c : c.BlendWithBackground(BitmapData.BackColor));

            #endregion
        }

        #endregion

        #region Constructors

        internal UnmanagedBitmapData16Gray(IntPtr buffer, int stride, in BitmapDataConfig cfg)
            : base(buffer, stride, cfg)
        {
        }

        #endregion

        #region Methods

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override unsafe Color32 DoGetPixel(int x, int y) => GetPixelAddress<Color16Gray>(y, x)->ToColor32();

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override unsafe void DoSetPixel(int x, int y, Color32 c)
            => *GetPixelAddress<Color16Gray>(y, x) = new Color16Gray(c.A == Byte.MaxValue ? c : c.BlendWithBackground(BackColor));

        #endregion
    }
}
