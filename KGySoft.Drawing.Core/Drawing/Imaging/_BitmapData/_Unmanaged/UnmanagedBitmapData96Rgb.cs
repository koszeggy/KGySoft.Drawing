#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: UnmanagedBitmapData96Rgb.cs
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
                    : new RgbF(c.ToColorF().BlendWithBackground(BitmapData.BackColor.ToColorF(), BitmapData.LinearWorkingColorSpace));

            #endregion
        }

        #endregion

        #region Constructors

        internal UnmanagedBitmapData96Rgb(IntPtr buffer, int stride, in BitmapDataConfig cfg)
            : base(buffer, stride, cfg)
        {
        }

        #endregion

        #region Methods

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override unsafe Color32 DoGetColor32(int x, int y) => GetPixelAddress<RgbF>(y, x)->ToColor32();

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override unsafe void DoSetColor32(int x, int y, Color32 c)
            => *GetPixelAddress<RgbF>(y, x) = c.A == Byte.MaxValue
                ? new RgbF(c)
                : new RgbF(c.ToColorF().BlendWithBackground(BackColor.ToColorF(), LinearWorkingColorSpace));

        #endregion
    }
}
