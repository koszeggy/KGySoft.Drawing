#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: UnmanagedBitmapData32Rgb.cs
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
    internal sealed class UnmanagedBitmapData32Rgb : UnmanagedBitmapDataBase<UnmanagedBitmapData32Rgb.Row>
    {
        #region Row class

        internal sealed class Row : UnmanagedBitmapDataRowBase
        {
            #region Methods

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override unsafe Color32 DoGetColor32(int x) => ((Color32*)Row)[x].ToOpaque();

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override unsafe void DoSetColor32(int x, Color32 c)
                => ((Color32*)Row)[x] = c.A == Byte.MaxValue ? c : c.BlendWithBackground(BitmapData.BackColor, BitmapData.LinearBlending);

            #endregion
        }

        #endregion

        #region Constructors

        internal UnmanagedBitmapData32Rgb(IntPtr buffer, int stride, in BitmapDataConfig cfg)
            : base(buffer, stride, cfg)
        {
        }

        #endregion

        #region Methods

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override unsafe Color32 DoGetPixel(int x, int y) => GetPixelAddress<Color32>(y, x)->ToOpaque();

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override unsafe void DoSetPixel(int x, int y, Color32 c)
            => *GetPixelAddress<Color32>(y, x) = c.A == Byte.MaxValue ? c : c.BlendWithBackground(BackColor, LinearBlending);

        #endregion
    }
}
