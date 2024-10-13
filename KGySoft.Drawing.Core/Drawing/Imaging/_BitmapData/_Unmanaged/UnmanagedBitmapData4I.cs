﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: UnmanagedBitmapData4I.cs
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
    internal sealed class UnmanagedBitmapData4I : UnmanagedBitmapDataIndexedBase<UnmanagedBitmapData4I.Row>
    {
        #region Row class

        internal sealed class Row : UnmanagedBitmapDataRowIndexedBase
        {
            #region Properties

            protected override uint MaxIndex => 15;

            #endregion

            #region Methods

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override unsafe int DoGetColorIndex(int x) => ColorExtensions.Get4bppColorIndex(((byte*)Row)[x >> 1], x);

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override unsafe void DoSetColorIndex(int x, int colorIndex)
                => ColorExtensions.Set4bppColorIndex(ref ((byte*)Row)[x >> 1], x, colorIndex);

            #endregion
        }

        #endregion

        #region Properties

        protected override uint MaxIndex => 15;

        #endregion

        #region Constructors

        internal UnmanagedBitmapData4I(IntPtr buffer, int stride, in BitmapDataConfig cfg)
            : base(buffer, stride, cfg)
        {
        }

        #endregion

        #region Methods

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public unsafe override int DoGetColorIndex(int x, int y) => ColorExtensions.Get4bppColorIndex(*GetPixelAddress<byte>(y, x >> 1), x);

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public unsafe override void DoSetColorIndex(int x, int y, int colorIndex)
            => ColorExtensions.Set4bppColorIndex(ref *GetPixelAddress<byte>(y, x >> 1), x, colorIndex);

        #endregion
    }
}
