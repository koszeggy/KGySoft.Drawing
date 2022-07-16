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
        
        #region Public Methods

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public sealed override unsafe T DoReadRaw<T>(int x) => ((T*)Row)[x];

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public sealed override unsafe void DoWriteRaw<T>(int x, T data) => ((T*)Row)[x] = data;

        #endregion

        #region Protected Methods

        protected override void DoMoveToIndex()
        {
#if NET35
            Row = new IntPtr(((UnmanagedBitmapDataBase)BitmapData).Scan0.ToInt64() + ((UnmanagedBitmapDataBase)BitmapData).Stride * Index);
#else
            Row = ((UnmanagedBitmapDataBase)BitmapData).Scan0 + ((UnmanagedBitmapDataBase)BitmapData).Stride * Index;
#endif
        }

        #endregion

        #endregion
    }
}
