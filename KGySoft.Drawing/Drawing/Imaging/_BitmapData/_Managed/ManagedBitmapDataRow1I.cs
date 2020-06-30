#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ManagedBitmapDataRow1I.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2020 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution. If not, then this file is considered as
//  an illegal copy.
//
//  Unauthorized copying of this file, via any medium is strictly prohibited.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System.Runtime.CompilerServices;
using System.Security;

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal class ManagedBitmapDataRow1I : ManagedBitmapDataRowIndexedBase<ManagedBitmapDataRow1I>
    {
        #region Properties

        protected override uint MaxIndex => 1;

        #endregion

        #region Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal override int DoGetColorIndex(int x)
        {
            int mask = 128 >> (x & 7);
            int bits = Row[x >> 3];
            return (bits & mask) != 0 ? 1 : 0;
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal override void DoSetColorIndex(int x, int colorIndex)
        {
            int pos = x >> 3;
            int mask = 128 >> (x & 7);
            if (colorIndex == 0)
                Row[pos] &= (byte)~mask;
            else
                Row[pos] |= (byte)mask;
        }

        #endregion
    }
}