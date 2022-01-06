#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ManagedBitmapDataRow16Gray.cs
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
    internal sealed class ManagedBitmapDataRow16Gray : ManagedBitmapDataRowBase<Color16Gray>
    {
        #region Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override Color32 DoGetColor32(int x) => Row[x].ToColor32();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override void DoSetColor32(int x, Color32 c)
            => Row[x] = new Color16Gray(c.A == Byte.MaxValue ? c : c.BlendWithBackground(BitmapData.BackColor));

        #endregion
    }

    internal sealed class ManagedBitmapDataRow16Gray<T> : ManagedBitmapDataRowBase<T>
        where T : unmanaged
    {
        #region Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override Color32 DoGetColor32(int x) => DoReadRaw<Color16Gray>(x).ToColor32();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override void DoSetColor32(int x, Color32 c)
            => DoWriteRaw(x, new Color16Gray(c.A == Byte.MaxValue ? c : c.BlendWithBackground(BitmapData.BackColor)));

        #endregion
    }

    internal sealed class ManagedBitmapDataRow16Gray2D<T> : ManagedBitmapDataRow2DBase<T>
        where T : unmanaged
    {
        #region Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override Color32 DoGetColor32(int x) => DoReadRaw<Color16Gray>(x).ToColor32();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override void DoSetColor32(int x, Color32 c)
            => DoWriteRaw(x, new Color16Gray(c.A == Byte.MaxValue ? c : c.BlendWithBackground(BitmapData.BackColor)));

        #endregion
    }
}