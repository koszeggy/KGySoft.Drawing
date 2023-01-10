#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ManagedBitmapData16Argb1555.cs
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
    internal sealed class ManagedBitmapData16Argb1555<T> : ManagedBitmapData1DArrayBase<T, ManagedBitmapData16Argb1555<T>.Row>
        where T : unmanaged
    {
        #region Row class

        internal sealed class Row : ManagedBitmapDataRowBase<T>
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

        #endregion

        #region Constructors

        internal ManagedBitmapData16Argb1555(Array2D<T> buffer, in BitmapDataConfig cfg)
            : base(buffer, cfg)
        {
        }

        #endregion

        #region Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override Color32 DoGetPixel(int x, int y) => GetPixelRef<Color16Argb1555>(y, x).ToColor32();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override void DoSetPixel(int x, int y, Color32 c)
        {
            if (c.A != Byte.MaxValue)
            {
                c = c.A >= AlphaThreshold ? c.BlendWithBackground(BackColor)
                    : c.A < 128 ? c
                    : default;
            }

            GetPixelRef<Color16Argb1555>(y, x) = new Color16Argb1555(c);
        }

        #endregion
    }
}
