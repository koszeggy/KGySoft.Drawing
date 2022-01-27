﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ManagedBitmapDataRow32PArgb.cs
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

using System.Runtime.CompilerServices;

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal sealed class ManagedBitmapDataRow32PArgb : ManagedBitmapDataRowBase<Color32>
    {
        #region Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override Color32 DoGetColor32(int x) => Row[x].ToStraight();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override void DoSetColor32(int x, Color32 c) => Row[x] = c.ToPremultiplied();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override Color32 DoGetColor32Premultiplied(int x) => Row[x];

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override void DoSetColor32Premultiplied(int x, Color32 c) => Row[x] = c;

        #endregion
    }
}