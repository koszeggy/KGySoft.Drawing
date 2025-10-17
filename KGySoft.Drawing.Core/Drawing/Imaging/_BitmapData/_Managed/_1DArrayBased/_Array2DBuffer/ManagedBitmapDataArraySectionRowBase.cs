#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ManagedBitmapDataArraySectionRowBase.cs
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
    internal abstract class ManagedBitmapDataArraySectionRowBase<T> : BitmapDataRowBase
        where T : unmanaged
    {
        #region Fields

        internal ArraySection<T> Row;

        #endregion

        #region Methods

        #region Public Methods

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public sealed override TResult DoReadRaw<TResult>(int x) => Row.GetPinnableReference().At<T, TResult>(x);

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public sealed override void DoWriteRaw<TValue>(int x, TValue data) => Row.GetPinnableReference().At<T, TValue>(x) = data;

        #endregion

        #region Protected Methods

        [SecuritySafeCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected ref T GetPixelRef(int x) => ref Row.GetElementReferenceUnchecked(x);

        protected sealed override void DoMoveToIndex() => Row = ((ManagedBitmapDataArray2DBase<T>)BitmapData).Buffer[Index];

        #endregion

        #endregion
    }
}
