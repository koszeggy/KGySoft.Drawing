#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ManagedBitmapDataCastArrayRowBase.cs
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
using System.Security;

using KGySoft.Collections;

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal abstract class ManagedBitmapDataCastArrayRowBase<T, TPixel> : BitmapDataRowBase
        where T : unmanaged
        where TPixel : unmanaged
    {
        #region Fields

        private CastArray<T, TPixel> row;

        #endregion

        #region Properties

        internal CastArray<T, TPixel> Row
        {
            init => row = value;
        }

        #endregion

        #region Methods

        #region Public Methods

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public sealed override TResult DoReadRaw<TResult>(int x) => row.Buffer.GetPinnableReference().At<T, TResult>(x);

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public sealed override void DoWriteRaw<TValue>(int x, TValue data) => row.Buffer.GetPinnableReference().At<T, TValue>(x) = data;

        #endregion

        #region Protected Methods

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected ref TPixel GetPixelRef(int x) => ref row.GetElementReferenceUnsafe(x);

        protected sealed override void DoMoveToIndex() => row = ((ManagedBitmapDataCastArray2DBase<T, TPixel>)BitmapData).Buffer[Index];

        #endregion

        #endregion
    }
}
