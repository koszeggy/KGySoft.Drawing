#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: NativeBitmapDataRowBase.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2021 - All Rights Reserved
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

namespace KGySoft.Drawing.Imaging
{
    internal abstract class NativeBitmapDataRowBase : BitmapDataRowBase
    {
        #region Fields

        [SecurityCritical]
        internal unsafe byte* Address;

        #endregion

        #region Methods

        [SecuritySafeCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override unsafe bool MoveNextRow()
        {
            if (!base.MoveNextRow())
                return false;

            Address += ((NativeBitmapDataBase)BitmapData).Stride;
            return true;
        }

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override unsafe T DoReadRaw<T>(int x) => ((T*)Address)[x];

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override unsafe void DoWriteRaw<T>(int x, T data) => ((T*)Address)[x] = data;

        #endregion
    }
}
