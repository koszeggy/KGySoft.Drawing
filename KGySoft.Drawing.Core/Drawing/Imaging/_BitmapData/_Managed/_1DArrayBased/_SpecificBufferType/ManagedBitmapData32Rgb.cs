#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ManagedBitmapData32Rgb.cs
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
    internal sealed class ManagedBitmapData32Rgb : ManagedBitmapData1DArrayBase<Color32, ManagedBitmapData32Rgb.Row>
    {
        #region Row class

        internal sealed class Row : ManagedBitmapDataRowBase<Color32>
        {
            #region Methods

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override Color32 DoGetColor32(int x) => Row[x].ToOpaque();

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetColor32(int x, Color32 c)
                => Row[x] = c.A == Byte.MaxValue ? c : c.BlendWithBackground(BitmapData.BackColor, BitmapData.PrefersLinearBlending);

            #endregion
        }

        #endregion

        #region Constructors

        internal ManagedBitmapData32Rgb(in BitmapDataConfig cfg)
            : base(cfg)
        {
        }

        internal ManagedBitmapData32Rgb(Array2D<Color32> buffer, in BitmapDataConfig cfg)
            : base(buffer, cfg)
        {
        }

        #endregion

        #region Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override Color32 DoGetPixel(int x, int y) => Buffer[y, x].ToOpaque();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override void DoSetPixel(int x, int y, Color32 c)
            => Buffer[y, x] = c.A == Byte.MaxValue ? c : c.BlendWithBackground(BackColor, PrefersLinearBlending);

        #endregion
    }
}
