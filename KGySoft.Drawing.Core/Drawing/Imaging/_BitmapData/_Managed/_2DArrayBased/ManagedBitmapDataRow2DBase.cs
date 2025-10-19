#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ManagedBitmapDataRow2DBase.cs
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

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Security;

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal abstract class ManagedBitmapDataRow2DBase<T> : BitmapDataRowBase
        where T : unmanaged
    {
        #region Properties

        [AllowNull]
        internal T[,] Buffer { get; set; }

        #endregion

        #region Methods

        #region Public Methods

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public sealed override TResult DoReadRaw<TResult>(int x) => GetPixelRef<TResult>(x);

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public sealed override void DoWriteRaw<TValue>(int x, TValue data) => GetPixelRef<TValue>(x) = data;

        #endregion

        #region Protected Methods

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected unsafe ref TPixel GetPixelRef<TPixel>(int x) where TPixel : unmanaged
        {
            Debug.Assert(!typeof(TPixel).IsPrimitive || Buffer[Index, 0].At<T, TPixel>(x).AsIntPtr() % sizeof(TPixel) == 0, $"Misaligned raw {typeof(TPixel).Name} access in row {Index} at position {x} - {BitmapData.PixelFormat} {Width}x{BitmapData.Height}");
            return ref Buffer[Index, 0].At<T, TPixel>(x);
        }

        protected sealed override void DoMoveToIndex()
        {
            // Not asserting row alignment here because a 2D array is allowed to use a misaligned stride
            //Debug.Assert(Buffer[Index, 0].AsIntPtr() % BitmapData.PixelFormat.AlignmentReq == 0, $"Misaligned {typeof(T).Name} at row {Index} - {BitmapData.PixelFormat} {Width}x{BitmapData.Height}");
        }

        #endregion

        #endregion
    }
}
