﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ManagedBitmapDataRow64PArgb.cs
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

using System.Runtime.CompilerServices;

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal sealed class ManagedBitmapDataRow64PArgb : ManagedBitmapDataRowBase<Color64, ManagedBitmapDataRow64PArgb>
    {
        #region Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override Color32 DoGetColor32(int x) => Row[x].ToStraight().ToColor32();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override void DoSetColor32(int x, Color32 c) => Row[x] = new Color64(c).ToPremultiplied();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override Color32 DoGetColor32Premultiplied(int x) => Row[x].ToColor32();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override void DoSetColor32Premultiplied(int x, Color32 c) => Row[x] = new Color64(c);

        #endregion
    }
}