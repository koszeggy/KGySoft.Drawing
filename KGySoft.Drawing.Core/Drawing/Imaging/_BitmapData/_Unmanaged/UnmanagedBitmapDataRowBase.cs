#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: UnmanagedBitmapDataRowBase.cs
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

using System;
using System.Runtime.CompilerServices;
using System.Security;

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal abstract class UnmanagedBitmapDataRowBase : BitmapDataRowBase
    {
        #region Fields

        internal IntPtr Row;

        #endregion

        #region Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public sealed override bool MoveNextRow()
        {
            if (!base.MoveNextRow())
                return false;

#if NET35
            Row = new IntPtr(Row.ToInt64() + ((UnmanagedBitmapDataBase)BitmapData).Stride);
#else
            Row += ((UnmanagedBitmapDataBase)BitmapData).Stride;
#endif
            return true;
        }

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public sealed override unsafe T DoReadRaw<T>(int x) => ((T*)Row)[x];

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public sealed override unsafe void DoWriteRaw<T>(int x, T data) => ((T*)Row)[x] = data;

        #endregion
    }
}
