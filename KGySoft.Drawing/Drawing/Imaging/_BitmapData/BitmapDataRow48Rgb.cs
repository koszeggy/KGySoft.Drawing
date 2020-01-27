#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BitmapDataRow48Rgb.cs
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
    internal sealed class BitmapDataRow48Rgb : BitmapDataRowNonIndexedBase
    {
        #region Methods

        internal override unsafe Color32 DoGetColor32(int x) => ((Color48*)Address)[x].ToArgb32();

        internal override unsafe void DoSetColor32(int x, Color32 c)
            => ((Color48*)Address)[x] = (c.A == Byte.MaxValue ? c : c.BlendWithBackground(Accessor.BackColor)).ToRgb48();

        #endregion
    }
}