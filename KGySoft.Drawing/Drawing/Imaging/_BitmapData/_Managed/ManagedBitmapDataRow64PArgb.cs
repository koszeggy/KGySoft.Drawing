﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ManagedBitmapDataRow64PArgb.cs
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
    internal sealed class ManagedBitmapDataRow64PArgb : ManagedBitmapDataRowBase<Color64>
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

    internal sealed class ManagedBitmapDataRow64PArgb<T> : ManagedBitmapDataRowBase<T>
        where T : unmanaged
    {
        #region Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override Color32 DoGetColor32(int x) => DoReadRaw<Color64>(x).ToStraight().ToColor32();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override void DoSetColor32(int x, Color32 c) => DoWriteRaw(x, new Color64(c).ToPremultiplied());

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override Color32 DoGetColor32Premultiplied(int x) => DoReadRaw<Color64>(x).ToColor32();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override void DoSetColor32Premultiplied(int x, Color32 c) => DoWriteRaw(x, new Color64(c));

        #endregion
    }

    internal sealed class ManagedBitmapDataRow64PArgb2D<T> : ManagedBitmapDataRow2DBase<T>
        where T : unmanaged
    {
        #region Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override Color32 DoGetColor32(int x) => DoReadRaw<Color64>(x).ToStraight().ToColor32();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override void DoSetColor32(int x, Color32 c) => DoWriteRaw(x, new Color64(c).ToPremultiplied());

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override Color32 DoGetColor32Premultiplied(int x) => DoReadRaw<Color64>(x).ToColor32();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override void DoSetColor32Premultiplied(int x, Color32 c) => DoWriteRaw(x, new Color64(c));

        #endregion
    }
}