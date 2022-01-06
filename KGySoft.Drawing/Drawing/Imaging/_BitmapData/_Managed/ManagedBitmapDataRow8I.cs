#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ManagedBitmapDataRow8I.cs
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

using System.Runtime.CompilerServices;

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal sealed class ManagedBitmapDataRow8I : ManagedBitmapDataRowIndexedBase<byte>
    {
        #region Properties

        protected override uint MaxIndex => 255;

        #endregion

        #region Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override int DoGetColorIndex(int x) => Row[x];

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override void DoSetColorIndex(int x, int colorIndex) => Row[x] = (byte)colorIndex;

        #endregion
    }

    internal sealed class ManagedBitmapDataRow8I<T> : ManagedBitmapDataRowIndexedBase<T>
        where T : unmanaged
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

    internal sealed class ManagedBitmapDataRow8I2D<T> : ManagedBitmapDataRowIndexed2DBase<T>
        where T : unmanaged
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
}