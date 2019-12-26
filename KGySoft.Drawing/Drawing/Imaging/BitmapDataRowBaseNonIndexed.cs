#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BitmapDataRowBaseNonIndexed.cs
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
    internal abstract class BitmapDataRowBaseNonIndexed : BitmapDataRowBase
    {
        #region Methods

        protected override Color64 DoGetColor64(int x) => DoGetColor32(x);
        protected override Color64 DoSetColor64(int x, Color64 c) => DoSetColor32(x, c.ToColor32());
        protected override int DoGetColorIndex(int i) => throw new InvalidOperationException(Res.ImagingInvalidOperationIndexedOnly);
        protected override void DoSetColorIndex(int x, int colorIndex) => throw new InvalidOperationException(Res.ImagingInvalidOperationIndexedOnly);

        #endregion
    }
}