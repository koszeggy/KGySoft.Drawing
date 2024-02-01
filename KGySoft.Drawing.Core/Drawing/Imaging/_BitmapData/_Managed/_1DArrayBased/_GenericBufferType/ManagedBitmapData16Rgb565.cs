#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ManagedBitmapData16Rgb565.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2024 - All Rights Reserved
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

using KGySoft.Collections;

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal sealed class ManagedBitmapData16Rgb565<T> : ManagedBitmapData1DArrayBase<T, ManagedBitmapData16Rgb565<T>.Row>
        where T : unmanaged
    {
        #region Row class

        internal sealed class Row : ManagedBitmapDataRowBase<T>
        {
            #region Methods

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override Color32 DoGetColor32(int x) => DoReadRaw<Color16Rgb565>(x).ToColor32();

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetColor32(int x, Color32 c)
                => DoWriteRaw(x, new Color16Rgb565(c.A == Byte.MaxValue ? c : c.BlendWithBackground(BitmapData.BackColor, BitmapData.LinearWorkingColorSpace)));

            #endregion
        }

        #endregion

        #region Constructors

        internal ManagedBitmapData16Rgb565(in BitmapDataConfig cfg)
            : base(cfg)
        {
        }


        internal ManagedBitmapData16Rgb565(Array2D<T> buffer, in BitmapDataConfig cfg)
            : base(buffer, cfg)
        {
        }

        #endregion

        #region Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override Color32 DoGetColor32(int x, int y) => GetPixelRef<Color16Rgb565>(y, x).ToColor32();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override void DoSetColor32(int x, int y, Color32 c)
            => GetPixelRef<Color16Rgb565>(y, x) = new Color16Rgb565(c.A == Byte.MaxValue ? c : c.BlendWithBackground(BackColor, LinearWorkingColorSpace));

        #endregion
    }
}
