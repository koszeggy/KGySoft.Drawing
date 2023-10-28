#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ManagedBitmapData128Rgba.cs
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
    internal sealed class ManagedBitmapData128Rgba : ManagedBitmapData1DArrayBase<ColorF, ManagedBitmapData128Rgba.Row>
    {
        #region Row class

        internal sealed class Row : ManagedBitmapDataRowBase<ColorF>
        {
            #region Methods

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override Color32 DoGetColor32(int x) => Row[x].ToColor32();

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetColor32(int x, Color32 c) => Row[x] = new ColorF(c);

            #endregion
        }

        #endregion

        #region Constructors

        internal ManagedBitmapData128Rgba(in BitmapDataConfig cfg)
            : base(cfg)
        {
        }

        internal ManagedBitmapData128Rgba(Array2D<ColorF> buffer, in BitmapDataConfig cfg)
            : base(buffer, cfg)
        {
        }

        #endregion

        #region Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override Color32 DoGetColor32(int x, int y) => Buffer[y, x].ToColor32();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override void DoSetColor32(int x, int y, Color32 c) => Buffer[y, x] = new ColorF(c);

        #endregion
    }
}
