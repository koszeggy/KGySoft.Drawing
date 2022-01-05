#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ManagedBitmapDataRowBase.cs
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

using System.Runtime.CompilerServices;
using System.Security;

using KGySoft.Collections;

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal abstract class ManagedBitmapDataRowBase<T> : BitmapDataRowBase
        where T : unmanaged
    {
        #region Fields

        internal ArraySection<T> Row;

        #endregion

        #region Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public sealed override bool MoveNextRow()
        {
            if (!base.MoveNextRow())
                return false;
            Row = ((ManagedBitmapDataSingleArrayBased<T>)BitmapData).Buffer[Index];
            return true;
        }

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public sealed override TAs DoReadRaw<TAs>(int x)
        {
#if NETCOREAPP3_0_OR_GREATER
            return Unsafe.Add(ref Unsafe.As<T, TAs>(ref Row.GetPinnableReference()), x);
#else
            unsafe
            {
                fixed (T* pRow = Row)
                    return ((TAs*)pRow)[x];
            }
#endif
        }

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public sealed override void DoWriteRaw<TAs>(int x, TAs data)
        {
#if NETCOREAPP3_0_OR_GREATER
            Unsafe.Add(ref Unsafe.As<T, TAs>(ref Row.GetPinnableReference()), x) = data;
#else
            unsafe
            {
                fixed (T* pRow = Row)
                    ((TAs*)pRow)[x] = data;
            }
#endif
        }

        #endregion
    }
}
