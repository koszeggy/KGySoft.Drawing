#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ManagedBitmapData8I2D.cs
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

using System.Runtime.CompilerServices;

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal sealed class ManagedBitmapData8I2D<T> : ManagedBitmapData2DArrayIndexedBase<T, ManagedBitmapData8I2D<T>.Row>
        where T : unmanaged
    {
        #region Row class

        internal sealed class Row : ManagedBitmapDataRowIndexed2DBase<T>
        {
            #region Properties

            protected override uint MaxIndex => 255;

            #endregion

            #region Methods

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override int DoGetColorIndex(int x) => DoReadRaw<byte>(x);

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetColorIndex(int x, int colorIndex) => DoWriteRaw(x, (byte)colorIndex);

            #endregion
        }

        #endregion

        #region Constructors

        internal ManagedBitmapData8I2D(T[,] buffer, in BitmapDataConfig cfg)
            : base(buffer, cfg)
        {
        }

        #endregion

        #region Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override int DoGetColorIndex(int x, int y) => GetPixelRef<byte>(y, x);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override void DoSetColorIndex(int x, int y, int colorIndex) => GetPixelRef<byte>(y, x) = (byte)colorIndex;

        #endregion
    }
}
