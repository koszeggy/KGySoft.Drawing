#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: UnmanagedBitmapDataRowBase.cs
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

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal abstract class UnmanagedBitmapDataRowBase : BitmapDataRowBase
    {
        #region Fields

        internal nint Row;

        #endregion

        #region Methods
        
        #region Public Methods

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public sealed override unsafe T DoReadRaw<T>(int x)
        {
            Debug.Assert(!typeof(T).IsPrimitive || Row % sizeof(T) == 0, $"Misaligned raw {typeof(T).Name} access in row {Index} at position {x} - {BitmapData.PixelFormat} {Width}x{BitmapData.Height}");
            return ((T*)Row)[x];
        }

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public sealed override unsafe void DoWriteRaw<T>(int x, T data)
        {
            Debug.Assert(!typeof(T).IsPrimitive || Row % sizeof(T) == 0, $"Misaligned raw {typeof(T).Name} access in row {Index} at position {x} - {BitmapData.PixelFormat} {Width}x{BitmapData.Height}");
            ((T*)Row)[x] = data;
        }

        #endregion

        #region Protected Methods

        protected override void DoMoveToIndex()
        {
            Row = (nint)(((UnmanagedBitmapDataBase)BitmapData).Scan0 + (long)((UnmanagedBitmapDataBase)BitmapData).Stride * Index);
      
            // Not asserting row alignment here because a raw buffer is allowed to be misaligned
            //Debug.Assert(Row % BitmapData.PixelFormat.AlignmentReq == 0, $"Misaligned address {Row} at row {Index} - {BitmapData.PixelFormat} {Width}x{BitmapData.Height}");
        }

        #endregion

        #endregion
    }
}
