#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: UnmanagedBitmapData1I.cs
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
    internal sealed class UnmanagedBitmapData1I : UnmanagedBitmapDataIndexedBase<UnmanagedBitmapData1I.Row>
    {
        #region Row class

        internal sealed class Row : UnmanagedBitmapDataRowIndexedBase
        {
            #region Properties

            protected override uint MaxIndex => 1;

            #endregion

            #region Methods

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override unsafe int DoGetColorIndex(int x) => ColorExtensions.Get1bppColorIndex(((byte*)Row)[x >> 3], x);

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override unsafe void DoSetColorIndex(int x, int colorIndex)
                => ColorExtensions.Set1bppColorIndex(ref ((byte*)Row)[x >> 3], x, colorIndex);

            #endregion
        }

        #endregion

        #region Constructors

        internal UnmanagedBitmapData1I(IntPtr buffer, int stride, in BitmapDataConfig cfg)
            : base(buffer, stride, cfg)
        {
        }

        #endregion

        #region Methods

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected unsafe override int DoGetColorIndex(int x, int y) => ColorExtensions.Get1bppColorIndex(*GetPixelAddress<byte>(y, x >> 3), x);

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected unsafe override void DoSetColorIndex(int x, int y, int colorIndex)
            => ColorExtensions.Set1bppColorIndex(ref *GetPixelAddress<byte>(y, x >> 3), x, colorIndex);

        #endregion
    }
}
