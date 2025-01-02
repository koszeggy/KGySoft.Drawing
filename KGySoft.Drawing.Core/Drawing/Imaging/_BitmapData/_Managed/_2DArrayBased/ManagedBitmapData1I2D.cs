#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ManagedBitmapData1I2D.cs
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

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal sealed class ManagedBitmapData1I2D<T> : ManagedBitmapData2DArrayIndexedBase<T, ManagedBitmapData1I2D<T>.Row>
        where T : unmanaged
    {
        #region Row class

        internal sealed class Row : ManagedBitmapDataRowIndexed2DBase<T>
        {
            #region Properties

            protected override uint MaxIndex => 1;

            #endregion

            #region Methods

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override int DoGetColorIndex(int x) => ColorExtensions.Get1bppColorIndex(DoReadRaw<byte>(x >> 3), x);

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetColorIndex(int x, int colorIndex)
                => ColorExtensions.Set1bppColorIndex(ref GetPixelRef<byte>(x >> 3), x, colorIndex);

            #endregion
        }

        #endregion

        #region Properties

        protected override uint MaxIndex => 1;

        #endregion

        #region Constructors

        internal ManagedBitmapData1I2D(T[,] buffer, in BitmapDataConfig cfg)
            : base(buffer, cfg)
        {
        }

        #endregion

        #region Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override int DoGetColorIndex(int x, int y) => ColorExtensions.Get1bppColorIndex(GetPixelRef<byte>(y, x >> 3), x);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override void DoSetColorIndex(int x, int y, int colorIndex)
            => ColorExtensions.Set1bppColorIndex(ref GetPixelRef<byte>(y, x >> 3), x, colorIndex);

        #endregion
    }
}
