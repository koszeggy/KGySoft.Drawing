#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BitmapDataRow32Rgb.cs
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
    internal sealed class BitmapDataRow32Rgb : BitmapDataRowNonIndexedBase
    {
        #region Methods

        #region Static Methods

        private static Color32 ToRgb32(Color32 c)
            => c.A == Byte.MaxValue ? c : new Color32(c.R, c.G, c.B);

        #endregion

        #region Instance Methods

        internal override unsafe Color32 DoGetColor32(int x) => ToRgb32(((Color32*)Address)[x]);

        internal override unsafe void DoSetColor32(int x, Color32 c)
            => ((Color32*)Address)[x] = c.A == Byte.MaxValue ? c : c.BlendWithBackground(Accessor.BackColor32);

        #endregion

        #endregion
    }
}