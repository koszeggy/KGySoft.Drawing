#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: NativeBitmapDataRow1I.cs
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
    internal class NativeBitmapDataRow1I : NativeBitmapDataRowIndexedBase
    {
        #region Properties

        protected override uint MaxIndex => 1;

        #endregion

        #region Methods

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override unsafe int DoGetColorIndex(int x)
        {
            int mask = 128 >> (x & 7);
            int bits = Address[x >> 3];
            return (bits & mask) != 0 ? 1 : 0;
        }

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override unsafe void DoSetColorIndex(int x, int colorIndex)
        {
            int pos = x >> 3;
            int mask = 128 >> (x & 7);
            if (colorIndex == 0)
                Address[pos] &= (byte)~mask;
            else
                Address[pos] |= (byte)mask;
        }

        #endregion
    }
}