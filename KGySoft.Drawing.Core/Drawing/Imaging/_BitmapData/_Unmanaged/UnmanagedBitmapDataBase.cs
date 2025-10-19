#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: UnmanagedBitmapDataBase.cs
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

using System;
using System.Runtime.CompilerServices;
using System.Security;

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal abstract class UnmanagedBitmapDataBase : BitmapDataBase
    {
        #region Properties

        internal nint Scan0 { get; }
        internal int Stride { get; }

        #endregion

        #region Constructors

        protected UnmanagedBitmapDataBase(IntPtr buffer, int stride, in BitmapDataConfig cfg)
            : base(cfg)
        {
            Debug.Assert(buffer != IntPtr.Zero);
            Debug.Assert(stride.Abs() >= cfg.PixelFormat.GetByteWidth(cfg.Size.Width));

            Scan0 = buffer;
            Stride = stride;
            RowSize = stride.Abs();
        }

        #endregion

        #region Methods

        #region Public Methods

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public sealed override unsafe T DoReadRaw<T>(int x, int y)
        {
            Debug.Assert(!typeof(T).IsPrimitive || (nint)GetPixelAddress<T>(y, x) % sizeof(T) == 0, $"Misaligned raw {typeof(T).Name} access in row {y} at position {x} - {PixelFormat} {Width}x{Height}");
            return *GetPixelAddress<T>(y, x);
        }

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public sealed override unsafe void DoWriteRaw<T>(int x, int y, T data)
        {
            Debug.Assert(!typeof(T).IsPrimitive || (nint)GetPixelAddress<T>(y, x) % sizeof(T) == 0, $"Misaligned raw {typeof(T).Name} access in row {y} at position {x} - {PixelFormat} {Width}x{Height}");
            *GetPixelAddress<T>(y, x) = data;
        }

        #endregion

        #region Protected Methods

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected unsafe TPixel* GetPixelAddress<TPixel>(int rowIndex, int offset) where TPixel : unmanaged
            => (TPixel*)((byte*)Scan0 + (long)rowIndex * Stride) + offset;

        #endregion

        #endregion
    }
}