#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ManagedBitmapDataRow2DBase.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2022 - All Rights Reserved
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
        internal T[,] Buffer { get; init; }

        #endregion

        #region Methods

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public sealed override TResult DoReadRaw<TResult>(int x)
        {
#if NETCOREAPP3_0_OR_GREATER
            return Unsafe.Add(ref Unsafe.As<T, TResult>(ref Buffer[Index, 0]), x);
#else
            unsafe
            {
                fixed (T* pRow = &Buffer[Index, 0])
                    return ((TResult*)pRow)[x];
            }
#endif
        }

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public sealed override void DoWriteRaw<TValue>(int x, TValue data)
        {
#if NETCOREAPP3_0_OR_GREATER
            Unsafe.Add(ref Unsafe.As<T, TValue>(ref Buffer[Index, 0]), x) = data;
#else
            unsafe
            {
                fixed (T* pRow = &Buffer[Index, 0])
                    ((TValue*)pRow)[x] = data;
            }
#endif
        }

        #endregion
    }
}
