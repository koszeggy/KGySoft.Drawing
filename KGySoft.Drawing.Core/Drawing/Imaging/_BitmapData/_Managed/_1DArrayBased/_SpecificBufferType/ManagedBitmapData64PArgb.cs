#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ManagedBitmapData64PArgb.cs
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

using System.Runtime.CompilerServices;

using KGySoft.Collections;

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal sealed class ManagedBitmapData64PArgb : ManagedBitmapData1DArrayBase<PColor64, ManagedBitmapData64PArgb.Row>
    {
        #region Row class

        internal sealed class Row : ManagedBitmapDataRowBase<PColor64>
        {
            #region Methods

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override Color32 DoGetColor32(int x) => Row[x].ToColor32();

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetColor32(int x, Color32 c) => Row[x] = new PColor64(c);

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override PColor32 DoGetColor32Premultiplied(int x) => Row[x].ToPColor32();

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetColor32Premultiplied(int x, PColor32 c) => Row[x] = new PColor64(c);

            #endregion
        }

        #endregion

        #region Constructors

        internal ManagedBitmapData64PArgb(in BitmapDataConfig cfg)
            : base(cfg)
        {
        }

        internal ManagedBitmapData64PArgb(Array2D<PColor64> buffer, in BitmapDataConfig cfg)
            : base(buffer, cfg)
        {
        }

        #endregion

        #region Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override Color32 DoGetPixel(int x, int y) => Buffer[y, x].ToColor32();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override void DoSetPixel(int x, int y, Color32 c) => Buffer[y, x] = new PColor64(c);

        #endregion
    }
}
