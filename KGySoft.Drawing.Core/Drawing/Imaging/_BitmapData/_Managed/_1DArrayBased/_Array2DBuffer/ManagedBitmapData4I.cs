#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ManagedBitmapData4I.cs
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
    internal sealed class ManagedBitmapData4I : ManagedBitmapDataArray2DIndexedBase<byte, ManagedBitmapData4I.Row>
    {
        #region Row class

        internal sealed class Row : ManagedBitmapDataArraySectionRowIndexedBase<byte>
        {
            #region Properties

            protected override uint MaxIndex => 15;

            #endregion

            #region Methods

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override int DoGetColorIndex(int x) => ColorExtensions.Get4bppColorIndex(GetPixelRef(x >> 1), x);

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetColorIndex(int x, int colorIndex)
                => ColorExtensions.Set4bppColorIndex(ref GetPixelRef(x >> 1), x, colorIndex);

            #endregion
        }

        #endregion

        #region Properties

        protected override uint MaxIndex => 15;

        #endregion

        #region Constructors

        internal ManagedBitmapData4I(in BitmapDataConfig cfg)
            : base(cfg)
        {
        }

        internal ManagedBitmapData4I(Array2D<byte> buffer, in BitmapDataConfig cfg)
            : base(buffer, cfg)
        {
        }

        #endregion

        #region Methods

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override int DoGetColorIndex(int x, int y) => ColorExtensions.Get4bppColorIndex(GetPixelRef(y, x >> 1), x);

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override void DoSetColorIndex(int x, int y, int colorIndex)
            => ColorExtensions.Set4bppColorIndex(ref GetPixelRef(y, x >> 1), x, colorIndex);

        #endregion
    }
}
