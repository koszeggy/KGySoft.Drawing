#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: UnmanagedBitmapDataBase.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2023 - All Rights Reserved
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
    internal abstract class UnmanagedBitmapDataBase : BitmapDataBase
    {
        #region Properties

        internal IntPtr Scan0 { get; }
        internal int Stride { get; }

        #endregion

        #region Constructors

        protected UnmanagedBitmapDataBase(IntPtr buffer, int stride, in BitmapDataConfig cfg)
            : base(cfg)
        {
            Debug.Assert(buffer != IntPtr.Zero);
            Debug.Assert(Math.Abs(stride) >= cfg.PixelFormat.GetByteWidth(cfg.Size.Width));

            Scan0 = buffer;
            Stride = stride;
            RowSize = Math.Abs(stride);
        }

        #endregion

        #region Methods

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected unsafe TPixel* GetPixelAddress<TPixel>(int rowIndex, int offset) where TPixel : unmanaged
            => (TPixel*)((byte*)Scan0 + rowIndex * Stride) + offset;

        #endregion
    }
}