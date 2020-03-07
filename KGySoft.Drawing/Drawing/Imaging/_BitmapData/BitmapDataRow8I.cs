#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BitmapDataRow8I.cs
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

using System.Security;

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal class BitmapDataRow8I : BitmapDataRowIndexedBase
    {
        #region Properties

        protected override uint MaxIndex => 255;

        #endregion

        #region Methods

        [SecurityCritical]
        internal override unsafe int DoGetColorIndex(int x) => Address[x];

        [SecurityCritical]
        internal override unsafe void DoSetColorIndex(int x, int colorIndex) => Address[x] = (byte)colorIndex;

        #endregion
    }
}