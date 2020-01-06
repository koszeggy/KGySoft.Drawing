#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BitmapDataRow16Rgb565.cs
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
    internal sealed class BitmapDataRow16Rgb565 : BitmapDataRowNonIndexedBase
    {
        #region Methods

        internal override unsafe Color32 DoGetColor32(int x) => ((Color16Rgb565*)Address)[x].ToColor32();

        internal override unsafe void DoSetColor32(int x, Color32 c)
            => ((Color16Rgb565*)Address)[x] = new Color16Rgb565(c.A == Byte.MaxValue ? c : c.BlendWithBackground(Accessor.BackColor32));

        #endregion
    }
}