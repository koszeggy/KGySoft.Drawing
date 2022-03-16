#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ManagedBitmapDataRow4I.cs
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
    internal sealed class ManagedBitmapDataRow4I : ManagedBitmapDataRowIndexedBase<byte>
    {
        #region Properties

        protected override uint MaxIndex => 15;

        #endregion

        #region Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override int DoGetColorIndex(int x)
        {
            int nibbles = Row[x >> 1];
            return (x & 1) == 0
                ? nibbles >> 4
                : nibbles & 0b00001111;
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override void DoSetColorIndex(int x, int colorIndex)
        {
            ref byte nibbles = ref Row.GetElementReference(x >> 1);
            if ((x & 1) == 0)
            {
                nibbles &= 0b00001111;
                nibbles |= (byte)(colorIndex << 4);
            }
            else
            {
                nibbles &= 0b11110000;
                nibbles |= (byte)colorIndex;
            }
        }

        #endregion
    }
}