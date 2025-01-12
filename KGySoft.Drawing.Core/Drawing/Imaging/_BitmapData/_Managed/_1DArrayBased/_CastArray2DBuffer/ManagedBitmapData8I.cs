﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ManagedBitmapData8I.cs
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

using System.Runtime.CompilerServices;
using System.Security;

using KGySoft.Collections;

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal sealed class ManagedBitmapData8I<T> : ManagedBitmapDataCastArray2DIndexedBase<T, ManagedBitmapData8I<T>.Row>
        where T : unmanaged
    {
        #region Row class

        internal sealed class Row : ManagedBitmapDataCastArrayRowIndexedBase<T>
        {
            #region Properties

            protected override uint MaxIndex => 255;

            #endregion

            #region Methods

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override int DoGetColorIndex(int x) => GetPixelRef(x);

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetColorIndex(int x, int colorIndex) => GetPixelRef(x) = (byte)colorIndex;

            #endregion
        }

        #endregion

        #region Properties

        protected override uint MaxIndex => 255;

        #endregion

        #region Constructors

        internal ManagedBitmapData8I(CastArray2D<T, byte> buffer, in BitmapDataConfig cfg)
            : base(buffer, cfg)
        {
        }

        #endregion

        #region Methods

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override int DoGetColorIndex(int x, int y) => GetPixelRef(y, x);

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override void DoSetColorIndex(int x, int y, int colorIndex) => GetPixelRef(y, x) = (byte)colorIndex;

        #endregion
    }
}
