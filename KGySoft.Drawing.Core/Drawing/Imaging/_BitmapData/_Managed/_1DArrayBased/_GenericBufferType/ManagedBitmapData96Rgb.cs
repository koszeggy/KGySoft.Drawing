#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ManagedBitmapData96Rgb.cs
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

using KGySoft.Collections;

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal sealed class ManagedBitmapData96Rgb<T> : ManagedBitmapData1DArrayBase<T, ManagedBitmapData96Rgb<T>.Row>
        where T : unmanaged
    {
        #region Row class

        internal sealed class Row : ManagedBitmapDataRowBase<T>
        {
            #region Methods

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override Color32 DoGetColor32(int x) => DoReadRaw<RgbF>(x).ToColor32();

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetColor32(int x, Color32 c)
                => DoWriteRaw(x, c.A == Byte.MaxValue
                    ? new RgbF(c)
                    : new RgbF(c.ToColorF().BlendWithBackground(BitmapData.BackColor.ToColorF(), BitmapData.LinearWorkingColorSpace)));

            #endregion
        }

        #endregion

        #region Constructors

        internal ManagedBitmapData96Rgb(Array2D<T> buffer, in BitmapDataConfig cfg)
            : base(buffer, cfg)
        {
        }

        #endregion

        #region Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override Color32 DoGetPixel(int x, int y) => GetPixelRef<RgbF>(y, x).ToColor32();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override void DoSetPixel(int x, int y, Color32 c)
            => GetPixelRef<RgbF>(y, x) = c.A == Byte.MaxValue
                ? new RgbF(c)
                : new RgbF(c.ToColorF().BlendWithBackground(BackColor.ToColorF(), LinearWorkingColorSpace));

        #endregion
    }
}
