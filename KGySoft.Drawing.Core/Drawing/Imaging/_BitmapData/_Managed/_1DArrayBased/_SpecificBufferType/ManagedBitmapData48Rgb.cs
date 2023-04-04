#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ManagedBitmapData48Rgb.cs
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
    internal sealed class ManagedBitmapData48Rgb : ManagedBitmapData1DArrayBase<Color48, ManagedBitmapData48Rgb.Row>
    {
        #region Row class

        internal sealed class Row : ManagedBitmapDataRowBase<Color48>
        {
            #region Methods

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override Color32 DoGetColor32(int x) => Row[x].ToColor32();

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetColor32(int x, Color32 c)
                => Row[x] = c.A == Byte.MaxValue
                    ? new Color48(c)
                    : new Color48(c.ToColor64().BlendWithBackground(BitmapData.BackColor.ToColor64(), BitmapData.LinearWorkingColorSpace));

            #endregion
        }

        #endregion

        #region Constructors

        internal ManagedBitmapData48Rgb(in BitmapDataConfig cfg)
            : base(cfg)
        {
        }

        internal ManagedBitmapData48Rgb(Array2D<Color48> buffer, in BitmapDataConfig cfg)
            : base(buffer, cfg)
        {
        }

        #endregion

        #region Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override Color32 DoGetPixel(int x, int y) => Buffer[y, x].ToColor32();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override void DoSetPixel(int x, int y, Color32 c)
            => Buffer[y, x] = c.A == Byte.MaxValue
                ? new Color48(c)
                : new Color48(c.ToColor64().BlendWithBackground(BackColor.ToColor64(), LinearWorkingColorSpace));

        #endregion
    }
}
