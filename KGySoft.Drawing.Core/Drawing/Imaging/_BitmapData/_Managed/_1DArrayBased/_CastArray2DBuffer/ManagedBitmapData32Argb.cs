#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ManagedBitmapData32Argb.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2025 - All Rights Reserved
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
    internal sealed class ManagedBitmapData32Argb<T> : ManagedBitmapDataCastArray2DBase<T, Color32, ManagedBitmapData32Argb<T>.Row>
        where T : unmanaged
    {
        #region Row class

        internal sealed class Row : ManagedBitmapDataCastArrayRowBase<T, Color32>
        {
            #region Methods

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override Color32 DoGetColor32(int x) => GetPixelRef(x);

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetColor32(int x, Color32 c) => GetPixelRef(x) = c;

            #endregion
        }

        #endregion

        #region Constructors

        internal ManagedBitmapData32Argb(in BitmapDataConfig cfg)
            : base(cfg)
        {
        }

        internal ManagedBitmapData32Argb(CastArray2D<T, Color32> buffer, in BitmapDataConfig cfg)
            : base(buffer, cfg)
        {
        }

        #endregion

        #region Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override Color32 DoGetColor32(int x, int y) => GetPixelRef(y, x);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override void DoSetColor32(int x, int y, Color32 c) => GetPixelRef(y, x) = c;

        #endregion
    }
}
