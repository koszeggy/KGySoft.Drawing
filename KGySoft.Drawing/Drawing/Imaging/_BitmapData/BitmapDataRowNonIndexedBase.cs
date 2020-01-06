#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BitmapDataRowNonIndexedBase.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2019 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution. If not, then this file is considered as
//  an illegal copy.
//
//  Unauthorized copying of this file, via any medium is strictly prohibited.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal abstract class BitmapDataRowNonIndexedBase : BitmapDataRowBase
    {
        #region Methods

        internal override int DoGetColorIndex(int i) => throw new InvalidOperationException(Res.ImagingInvalidOperationIndexedOnly);
        internal override void DoSetColorIndex(int x, int colorIndex) => throw new InvalidOperationException(Res.ImagingInvalidOperationIndexedOnly);

        #endregion
    }
}