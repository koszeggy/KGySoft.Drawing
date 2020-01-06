#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BitmapDataRow64Argb.cs
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

namespace KGySoft.Drawing.Imaging
{
    internal sealed class BitmapDataRow64Argb : BitmapDataRowNonIndexedBase
    {
        #region Methods

        internal override unsafe Color32 DoGetColor32(int x) => ColorHelper.ToArgb32(((Color64*)Address)[x]);

        internal override unsafe void DoSetColor32(int x, Color32 c) => ((Color64*)Address)[x] = c.ToArgb64();

        #endregion
    }
}