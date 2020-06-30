#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ManagedBitmapDataRow16Argb1555.cs
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
using System.Runtime.CompilerServices;
using System.Security;

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal sealed class ManagedBitmapDataRow16Argb1555 : ManagedBitmapDataRowBase<Color16Argb1555, ManagedBitmapDataRow16Argb1555>
    {
        #region Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override Color32 DoGetColor32(int x) => Row[x].ToColor32();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override void DoSetColor32(int x, Color32 c)
        {
            if (c.A != Byte.MaxValue)
            {
                c = c.A >= BitmapData.AlphaThreshold ? c.BlendWithBackground(BitmapData.BackColor)
                    : c.A < 128 ? c
                    : default;
            }

            Row[x] = new Color16Argb1555(c);
        }

        #endregion
    }
}