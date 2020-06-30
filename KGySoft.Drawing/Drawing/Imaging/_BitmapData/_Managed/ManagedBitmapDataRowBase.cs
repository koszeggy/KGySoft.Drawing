#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ManagedBitmapDataRowBase.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2020 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution. If not, then this file is considered as
//  an illegal copy.
//
//  Unauthorized copying of this file, via any medium is strictly prohibited.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System.Runtime.CompilerServices;
using System.Security;

using KGySoft.Collections;

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal abstract class ManagedBitmapDataRowBase<TColor, TRow> : BitmapDataRowBase
        where TColor : unmanaged
        where TRow : ManagedBitmapDataRowBase<TColor, TRow>, new()
    {
        #region Fields

        internal ArraySection<TColor> Row;

        #endregion

        #region Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override bool MoveNextRow()
        {
            if (!base.MoveNextRow())
                return false;
            Row = ((ManagedBitmapData<TColor, TRow>)BitmapData).Buffer[Index];
            return true;
        }

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override T DoReadRaw<T>(int x)
        {
#if NETFRAMEWORK
            unsafe
            {
                fixed (TPixel* pRow = Row)
                    return ((T*)pRow)[x];
            }
#else
            return Unsafe.Add(ref Unsafe.As<TColor, T>(ref Row.GetPinnableReference()), x);
#endif
        }

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override void DoWriteRaw<T>(int x, T data)
        {
#if NETFRAMEWORK
            unsafe
            {
                fixed (TPixel* pRow = Row)
                    ((T*)pRow)[x] = data;
            }
#else
            Unsafe.Add(ref Unsafe.As<TColor, T>(ref Row.GetPinnableReference()), x) = data;
#endif
        }

        #endregion
    }
}
