#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ManagedBitmapDataRow16Argb1555.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2022 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;
using System.Runtime.CompilerServices;

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal sealed class ManagedBitmapDataRow16Argb1555 : ManagedBitmapDataRowBase<Color16Argb1555>
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

    internal sealed class ManagedBitmapDataRow16Argb1555<T> : ManagedBitmapDataRowBase<T>
        where T : unmanaged
    {
        #region Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override Color32 DoGetColor32(int x) => DoReadRaw<Color16Argb1555>(x).ToColor32();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override void DoSetColor32(int x, Color32 c)
        {
            if (c.A != Byte.MaxValue)
            {
                c = c.A >= BitmapData.AlphaThreshold ? c.BlendWithBackground(BitmapData.BackColor)
                    : c.A < 128 ? c
                    : default;
            }

            DoWriteRaw(x, new Color16Argb1555(c));
        }

        #endregion
    }
}