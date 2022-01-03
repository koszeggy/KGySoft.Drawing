#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: NativeBitmapDataRow4I.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2021 - All Rights Reserved
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

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal class NativeBitmapDataRow4I : NativeBitmapDataRowIndexedBase
    {
        #region Properties

        protected override uint MaxIndex => 15;

        #endregion

        #region Methods

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override unsafe int DoGetColorIndex(int x)
        {
            int nibbles = ((byte*)Address)[x >> 1];
            return (x & 1) == 0
                ? nibbles >> 4
                : nibbles & 0b00001111;
        }

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override unsafe void DoSetColorIndex(int x, int colorIndex)
        {
            int pos = x >> 1;
            int nibbles = ((byte*)Address)[pos];
            if ((x & 1) == 0)
            {
                nibbles &= 0b00001111;
                nibbles |= colorIndex << 4;
            }
            else
            {
                nibbles &= 0b11110000;
                nibbles |= colorIndex;
            }

            ((byte*)Address)[pos] = (byte)nibbles;
        }

        #endregion
    }
}