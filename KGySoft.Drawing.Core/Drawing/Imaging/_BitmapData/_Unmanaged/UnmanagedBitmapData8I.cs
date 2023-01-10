#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: UnmanagedBitmapData8I.cs
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
    internal sealed class UnmanagedBitmapData8I : UnmanagedBitmapDataIndexedBase<UnmanagedBitmapData8I.Row>
    {
        #region Row class

        internal sealed class Row : UnmanagedBitmapDataRowIndexedBase
        {
            #region Properties

            protected override uint MaxIndex => 255;

            #endregion

            #region Methods

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override unsafe int DoGetColorIndex(int x) => ((byte*)Row)[x];

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override unsafe void DoSetColorIndex(int x, int colorIndex) => ((byte*)Row)[x] = (byte)colorIndex;

            #endregion
        }

        #endregion

        #region Constructors

        internal UnmanagedBitmapData8I(IntPtr buffer, int stride, in BitmapDataConfig cfg)
            : base(buffer, stride, cfg)
        {
        }

        #endregion

        #region Methods

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override unsafe int DoGetColorIndex(int x, int y) => *GetPixelAddress<byte>(y, x);

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override unsafe void DoSetColorIndex(int x, int y, int colorIndex) => *GetPixelAddress<byte>(y, x) = (byte)colorIndex;

        #endregion
    }
}
