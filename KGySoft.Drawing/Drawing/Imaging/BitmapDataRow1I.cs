#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BitmapDataRow1I.cs
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

namespace KGySoft.Drawing.Imaging
{
    internal class BitmapDataRow1I : BitmapDataRowIndexedBase
    {
        #region Properties

        protected override uint MaxIndex => 1;

        #endregion

        #region Methods

        internal override unsafe int DoGetColorIndex(int x)
        {
            int mask = 128 >> (x & 7);
            int bits = Address[x >> 3];
            return (bits & mask) != 0 ? 1 : 0;
        }

        internal override unsafe void DoSetColorIndex(int x, int colorIndex)
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