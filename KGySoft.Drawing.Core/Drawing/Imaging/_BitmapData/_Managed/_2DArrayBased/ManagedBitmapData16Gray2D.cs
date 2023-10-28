#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ManagedBitmapData16Gray2D.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2023 - All Rights Reserved
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
    internal sealed class ManagedBitmapData16Gray2D<T> : ManagedBitmapData2DArrayBase<T, ManagedBitmapData16Gray2D<T>.Row>
        where T : unmanaged
    {
        #region Row class

        internal sealed class Row : ManagedBitmapDataRow2DBase<T>
        {
            #region Methods

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override Color32 DoGetColor32(int x) => DoReadRaw<Color16Gray>(x).ToColor32();

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetColor32(int x, Color32 c) => DoWriteRaw(x, BitmapData.LinearWorkingColorSpace
                ? new Color16Gray(c.A == Byte.MaxValue ? c.ToColorF() : c.ToColorF().BlendWithBackgroundLinear(BitmapData.BackColor.ToColorF()))
                : new Color16Gray(c.A == Byte.MaxValue ? c.ToColor64() : c.ToColor64().BlendWithBackgroundSrgb(BitmapData.BackColor.ToColor64())));

            #endregion
        }

        #endregion

        #region Constructors

        internal ManagedBitmapData16Gray2D(T[,] buffer, in BitmapDataConfig cfg)
            : base(buffer, cfg)
        {
        }

        #endregion

        #region Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override Color32 DoGetColor32(int x, int y) => GetPixelRef<Color16Gray>(y, x).ToColor32();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override void DoSetColor32(int x, int y, Color32 c) => GetPixelRef<Color16Gray>(y, x) = LinearWorkingColorSpace
            ? new Color16Gray(c.A == Byte.MaxValue ? c.ToColorF() : c.ToColorF().BlendWithBackgroundLinear(BackColor.ToColorF()))
            : new Color16Gray(c.A == Byte.MaxValue ? c.ToColor64() : c.ToColor64().BlendWithBackgroundSrgb(BackColor.ToColor64()));

        #endregion
    }
}
