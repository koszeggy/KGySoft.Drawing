#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ManagedBitmapDataCastArrayRowBase.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2026 - All Rights Reserved
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
        public sealed override unsafe TResult DoReadRaw<TResult>(int x)
        {
            Debug.Assert(!typeof(TResult).IsPrimitive || row.Buffer.GetPinnableReference().At<T, TResult>(x).AsIntPtr() % sizeof(TResult) == 0, $"Misaligned raw {typeof(TResult).Name} access in row {Index} at position {x} - {BitmapData.PixelFormat} {Width}x{BitmapData.Height}");
            return row.Buffer.GetPinnableReference().At<T, TResult>(x);
        }

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public sealed override unsafe void DoWriteRaw<TValue>(int x, TValue data)
        {
            Debug.Assert(!typeof(TValue).IsPrimitive || row.Buffer.GetPinnableReference().At<T, TValue>(x).AsIntPtr() % sizeof(TValue) == 0, $"Misaligned raw {typeof(TValue).Name} access in row {Index} at position {x} - {BitmapData.PixelFormat} {Width}x{BitmapData.Height}");
            row.Buffer.GetPinnableReference().At<T, TValue>(x) = data;
        }

        #endregion

        #region Protected Methods

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected ref TPixel GetPixelRef(int x) => ref row.GetElementReferenceUnsafe(x);

        protected sealed override void DoMoveToIndex()
        {
            row = ((ManagedBitmapDataCastArray2DBase<T, TPixel>)BitmapData).Buffer[Index];

            // Not asserting row alignment here because a CastArray is allowed to use a misaligned underlying buffer
            //Debug.Assert(row.GetElementReference(0).AsIntPtr() % BitmapData.PixelFormat.AlignmentReq == 0, $"Misaligned {typeof(T).Name} at row {Index} - {BitmapData.PixelFormat} {Width}x{BitmapData.Height}");
        }

        #endregion

        #endregion
    }
}
