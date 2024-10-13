#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ManagedBitmapData8I.cs
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

using System.Runtime.CompilerServices;

using KGySoft.Collections;

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal sealed class ManagedBitmapData8I<T> : ManagedBitmapData1DArrayIndexedBase<T, ManagedBitmapData8I<T>.Row>
        where T : unmanaged
    {
        #region Nested classes

        #region Row class

        internal sealed class Row : ManagedBitmapDataRowIndexedBase<T>
        {
            #region Properties

            protected override uint MaxIndex => 255;

            #endregion

            #region Methods

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override int DoGetColorIndex(int x) => DoReadRaw<byte>(x);

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetColorIndex(int x, int colorIndex) => DoWriteRaw(x, (byte)colorIndex);

            #endregion
        }

        #endregion

        #endregion

        #region Properties

        protected override uint MaxIndex => 255;

        #endregion

        #region Constructors

        internal ManagedBitmapData8I(Array2D<T> buffer, in BitmapDataConfig cfg)
            : base(buffer, cfg)
        {
        }

        #endregion

        #region Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override int DoGetColorIndex(int x, int y) => GetPixelRef<byte>(y, x);
       
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override void DoSetColorIndex(int x, int y, int colorIndex) => GetPixelRef<byte>(y, x) = (byte)colorIndex;

        #endregion
    }
}
