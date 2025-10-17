#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: RefExtensions.cs
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

#endregion

namespace KGySoft.Drawing
{
    internal static class RefExtensions
    {
        #region Methods

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static ref TTarget As<TSource, TTarget>(this ref TSource source)
            where TSource : unmanaged
            where TTarget : unmanaged
        {
#if NETCOREAPP3_0_OR_GREATER
            return ref Unsafe.As<TSource, TTarget>(ref source);
#else
            unsafe
            {
                fixed (TSource* p = &source)
                    return ref *(TTarget*)p;
            }
#endif
        }

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static ref T At<T>(this ref T source, int index)
            where T : unmanaged
        {
#if NETCOREAPP3_0_OR_GREATER
            return ref Unsafe.Add(ref source, index);
#else
            unsafe
            {
                fixed (T* p = &source)
                    return ref p[index];
            }
#endif
        }

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static ref TTarget At<TSource, TTarget>(this ref TSource source, int targetIndex)
            where TSource : unmanaged
            where TTarget : unmanaged
        {
#if NETCOREAPP3_0_OR_GREATER
            return ref Unsafe.Add(ref Unsafe.As<TSource, TTarget>(ref source), targetIndex);
#else
            unsafe
            {
                fixed (TSource* p = &source)
                    return ref ((TTarget*)p)[targetIndex];
            }
#endif
        }

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static ref TTarget At<TSource, TTarget>(this ref TSource source, int sourceIndex, int targetIndex)
            where TSource : unmanaged
            where TTarget : unmanaged
        {
#if NETCOREAPP3_0_OR_GREATER
            return ref Unsafe.Add(ref Unsafe.As<TSource, TTarget>(ref Unsafe.Add(ref source, sourceIndex)), targetIndex);
#else
            unsafe
            {
                fixed (TSource* p = &source)
                    return ref ((TTarget*)&p[sourceIndex])[targetIndex];
            }
#endif
        }

        #endregion
    }
}
