#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ManagedBitmapDataRow1I.cs
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
    internal sealed class ManagedBitmapDataRow1I : ManagedBitmapDataRowIndexedBase<byte>
    {
        #region Properties

        protected override uint MaxIndex => 1;

        #endregion

        #region Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override int DoGetColorIndex(int x)
        {
            int mask = 128 >> (x & 7);
            int bits = Row[x >> 3];
            return (bits & mask) != 0 ? 1 : 0;
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override void DoSetColorIndex(int x, int colorIndex)
        {
            int pos = x >> 3;
            int mask = 128 >> (x & 7);
            if (colorIndex == 0)
                Row.GetElementReference(pos) &= (byte)~mask;
            else
                Row.GetElementReference(pos) |= (byte)mask;
        }

        #endregion
    }

    internal sealed class ManagedBitmapDataRow1I<T> : ManagedBitmapDataRowIndexedBase<T>
        where T : unmanaged
    {
        #region Properties

        protected override uint MaxIndex => 1;

        #endregion

        #region Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override int DoGetColorIndex(int x)
        {
            int mask = 128 >> (x & 7);
            int bits = DoReadRaw<byte>(x >> 3);
            return (bits & mask) != 0 ? 1 : 0;
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override void DoSetColorIndex(int x, int colorIndex)
        {
            int pos = x >> 3;
            int mask = 128 >> (x & 7);
            byte bits = DoReadRaw<byte>(pos);
            if (colorIndex == 0)
                DoWriteRaw(pos, (byte)(bits & ~mask));
            else
                DoWriteRaw(pos, (byte)(bits | mask));
        }

        #endregion
    }

    internal sealed class ManagedBitmapDataRow1I2D<T> : ManagedBitmapDataRowIndexed2DBase<T>
        where T : unmanaged
    {
        #region Properties

        protected override uint MaxIndex => 1;

        #endregion

        #region Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override int DoGetColorIndex(int x)
        {
            int mask = 128 >> (x & 7);
            int bits = DoReadRaw<byte>(x >> 3);
            return (bits & mask) != 0 ? 1 : 0;
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override void DoSetColorIndex(int x, int colorIndex)
        {
            int pos = x >> 3;
            int mask = 128 >> (x & 7);
            byte bits = DoReadRaw<byte>(pos);
            if (colorIndex == 0)
                DoWriteRaw(pos, (byte)(bits & ~mask));
            else
                DoWriteRaw(pos, (byte)(bits | mask));
        }

        #endregion
    }
}