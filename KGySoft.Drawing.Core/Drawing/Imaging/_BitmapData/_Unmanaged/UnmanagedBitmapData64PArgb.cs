#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: UnmanagedBitmapData64PArgb.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2022 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Security;

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal sealed class UnmanagedBitmapData64PArgb : UnmanagedBitmapDataBase<UnmanagedBitmapData64PArgb.Row>
    {
        #region Row class

        internal sealed class Row : UnmanagedBitmapDataRowBase
        {
            #region Methods

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override unsafe Color32 DoGetColor32(int x) => ((Color64*)Row)[x].ToStraight().ToColor32();

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override unsafe void DoSetColor32(int x, Color32 c) => ((Color64*)Row)[x] = new Color64(c).ToPremultiplied();

            #endregion
        }

        #endregion

        #region Constructors

        internal UnmanagedBitmapData64PArgb(IntPtr buffer, Size size, int stride, Color32 backColor, byte alphaThreshold, Action? disposeCallback)
            : base(buffer, size, stride, KnownPixelFormat.Format64bppPArgb.ToInfoInternal(), backColor, alphaThreshold, disposeCallback)
        {
        }

        #endregion

        #region Methods

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override unsafe Color32 DoGetPixel(int x, int y) => GetPixelAddress<Color64>(y, x)->ToStraight().ToColor32();

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override unsafe void DoSetPixel(int x, int y, Color32 c) => *GetPixelAddress<Color64>(y, x) = new Color64(c).ToPremultiplied();

        #endregion
    }
}
