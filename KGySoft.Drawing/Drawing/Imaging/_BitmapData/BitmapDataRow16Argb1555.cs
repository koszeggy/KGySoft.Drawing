#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BitmapDataRow16Argb1555.cs
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

using System;
using System.Security;

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal sealed class BitmapDataRow16Argb1555 : BitmapDataRowNonIndexedBase
    {
        #region Methods

        [SecurityCritical]
        internal override unsafe Color32 DoGetColor32(int x) => ((Color16Argb1555*)Address)[x].ToColor32();

        [SecurityCritical]
        internal override unsafe void DoSetColor32(int x, Color32 c)
        {
            if (c.A != Byte.MaxValue)
            {
                c = c.A >= Accessor.AlphaThreshold ? c.BlendWithBackground(Accessor.BackColor)
                    : c.A < 128 ? c
                    : default;
            }

            ((Color16Argb1555*)Address)[x] = new Color16Argb1555(c);
        }

        #endregion
    }
}