#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ManagedBitmapData128PRgba.cs
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
    internal sealed class ManagedBitmapData128PRgba : ManagedBitmapData1DArrayBase<PColorF, ManagedBitmapData128PRgba.Row>
    {
        #region Row class

        internal sealed class Row : ManagedBitmapDataRowBase<PColorF>
        {
            #region Methods

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override Color32 DoGetColor32(int x) => Row[x].ToColor32();

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetColor32(int x, Color32 c) => Row[x] = new PColorF(c);

            #endregion
        }

        #endregion

        #region Constructors

        internal ManagedBitmapData128PRgba(in BitmapDataConfig cfg)
            : base(cfg)
        {
        }

        internal ManagedBitmapData128PRgba(Array2D<PColorF> buffer, in BitmapDataConfig cfg)
            : base(buffer, cfg)
        {
        }

        #endregion

        #region Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override Color32 DoGetPixel(int x, int y) => Buffer[y, x].ToColor32();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override void DoSetPixel(int x, int y, Color32 c) => Buffer[y, x] = new PColorF(c);

        #endregion
    }
}
