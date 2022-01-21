#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ManagedBitmapDataRow24Rgb.cs
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
    internal sealed class ManagedBitmapDataRow24Rgb<T> : ManagedBitmapDataRowBase<T>
        where T : unmanaged
    {
        #region Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override Color32 DoGetColor32(int x) => DoReadRaw<Color24>(x).ToColor32();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override void DoSetColor32(int x, Color32 c)
            => DoWriteRaw(x, new Color24(c.A == Byte.MaxValue ? c : c.BlendWithBackground(BitmapData.BackColor)));

        #endregion
    }
}