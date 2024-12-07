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
    internal sealed class ManagedBitmapData16Rgb565<T> : ManagedBitmapDataCastArray2DBase<T, Color16Rgb565, ManagedBitmapData16Rgb565<T>.Row>
        where T : unmanaged
    {
        #region Row class

        internal sealed class Row : ManagedBitmapDataCastArrayRowBase<T, Color16Rgb565>
        {
            #region Methods

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override Color32 DoGetColor32(int x) => GetPixelRef(x).ToColor32();

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetColor32(int x, Color32 c)
                => GetPixelRef(x) = new Color16Rgb565(c.A == Byte.MaxValue ? c : c.BlendWithBackground(BitmapData.BackColor, BitmapData.LinearWorkingColorSpace));

            #endregion
        }

        #endregion

        #region Constructors

        internal ManagedBitmapData16Rgb565(in BitmapDataConfig cfg)
            : base(cfg)
        {
        }


        internal ManagedBitmapData16Rgb565(CastArray2D<T, Color16Rgb565> buffer, in BitmapDataConfig cfg)
            : base(buffer, cfg)
        {
        }

        #endregion

        #region Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override Color32 DoGetColor32(int x, int y) => GetPixelRef(y, x).ToColor32();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override void DoSetColor32(int x, int y, Color32 c)
            => GetPixelRef(y, x) = new Color16Rgb565(c.A == Byte.MaxValue ? c : c.BlendWithBackground(BackColor, LinearWorkingColorSpace));

        #endregion
    }
}
