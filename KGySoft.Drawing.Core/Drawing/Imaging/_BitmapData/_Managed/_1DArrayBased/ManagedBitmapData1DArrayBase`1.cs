﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ManagedBitmapData1DArrayBase`1.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2024 - All Rights Reserved
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
    internal abstract class ManagedBitmapData1DArrayBase<T> : ManagedBitmapDataBase
        where T : unmanaged
    {
        #region Fields

        /// <summary>
        /// The pixel buffer where the underlying array is a single dimensional one.
        /// </summary>
        internal Array2D<T> Buffer;

        #endregion

        #region Constructors

        protected unsafe ManagedBitmapData1DArrayBase(in BitmapDataConfig cfg)
            : base(cfg)
        {
            Debug.Assert(!cfg.PixelFormat.IsCustomFormat, "In this overload known pixel format is expected");
            Debug.Assert(cfg.DisposeCallback == null, "No dispose callback is expected from the self-allocating constructor");

            // Passing an array to the Array2D constructor to avoid array pooling.
            if (typeof(T) == typeof(byte))
            {
                int byteWidth = cfg.PixelFormat.GetByteWidth(cfg.Size.Width);
                Buffer = new Array2D<T>(new T[cfg.Size.Height * byteWidth], cfg.Size.Height, byteWidth);
            }
            else
                Buffer = new Array2D<T>(new T[cfg.Size.Height * cfg.Size.Width], cfg.Size.Height, cfg.Size.Width);

            RowSize = Buffer.Width * sizeof(T);
        }

        [SecuritySafeCritical]
        protected unsafe ManagedBitmapData1DArrayBase(Array2D<T> buffer, in BitmapDataConfig cfg)
            : base(cfg)
        {
            Buffer = buffer;
            RowSize = buffer.Width * sizeof(T);
        }

        #endregion

        #region Methods

        #region Public Methods

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public sealed override TResult DoReadRaw<TResult>(int x, int y) => GetPixelRef<TResult>(y, x);

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public sealed override void DoWriteRaw<TValue>(int x, int y, TValue data) => GetPixelRef<TValue>(y, x) = data;

        #endregion

        #region Internal Methods

#if NETCOREAPP3_0_OR_GREATER
        internal sealed override ref byte GetPinnableReference()
            => ref Unsafe.As<T, byte>(ref Buffer.GetPinnableReference());
#else
        [SecuritySafeCritical]
        internal sealed override unsafe ref byte GetPinnableReference()
        {
            ref T head = ref Buffer.GetPinnableReference();
            fixed (T* pHead = &head)
                return ref *(byte*)pHead;
        }
#endif

        #endregion

        #region Protected Methods

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected ref TPixel GetPixelRef<TPixel>(int y, int x)
            where TPixel : unmanaged
        {
#if NETCOREAPP3_0_OR_GREATER
            return ref Unsafe.Add(ref Unsafe.As<byte, TPixel>(ref Unsafe.Add(ref GetPinnableReference(), y * RowSize)), x);
#else
            unsafe
            {
                fixed (T* pBuf = Buffer)
                    return ref ((TPixel*)&((byte*)pBuf)[y * RowSize])[x];
            }
#endif
        }

        #endregion

        #endregion
    }
}