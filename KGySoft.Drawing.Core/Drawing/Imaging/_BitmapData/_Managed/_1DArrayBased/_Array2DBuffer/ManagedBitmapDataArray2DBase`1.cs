#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ManagedBitmapDataArray2DBase`1.cs
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
    internal abstract class ManagedBitmapDataArray2DBase<T> : ManagedBitmapDataBase
        where T : unmanaged
    {
        #region Fields

        #region Internal Fields
        
        /// <summary>
        /// The pixel buffer where the underlying array is a single dimensional one.
        /// </summary>
        internal Array2D<T> Buffer;

        #endregion

        #region Private Fields

        private readonly bool ownsBuffer;

        #endregion

        #endregion

        #region Constructors

        protected unsafe ManagedBitmapDataArray2DBase(in BitmapDataConfig cfg)
            : base(cfg)
        {
            Debug.Assert(!cfg.PixelFormat.IsCustomFormat, "In this overload known pixel format is expected");
            Debug.Assert(cfg.DisposeCallback == null, "No dispose callback is expected from the self-allocating constructor");
            Debug.Assert(typeof(T) == typeof(byte) || sizeof(T) * 8 == cfg.PixelFormat.BitsPerPixel, "In the self-allocating constructor the underlying type expected to be a type of the same matching the pixel format.");
            
            if (typeof(T) == typeof(byte))
            {
                Buffer = new Array2D<T>(cfg.Size.Height, cfg.PixelFormat.GetByteWidth(cfg.Size.Width));
                ownsBuffer = true;
            }
            else
            {
                // Passing an array to the buffer to avoid array pooling.
                Buffer = new Array2D<T>(new T[cfg.Size.Height * cfg.Size.Width], cfg.Size.Height, cfg.Size.Width);
            }

            RowSize = Buffer.Width * sizeof(T);
        }

        [SecuritySafeCritical]
        protected unsafe ManagedBitmapDataArray2DBase(Array2D<T> buffer, in BitmapDataConfig cfg)
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
        public sealed override TResult DoReadRaw<TResult>(int x, int y)
        {
#if NETCOREAPP3_0_OR_GREATER
            return Unsafe.Add(ref Unsafe.As<byte, TResult>(ref Unsafe.Add(ref GetPinnableReference(), y * RowSize)), x);
#else
            unsafe
            {
                fixed (T* pBuf = Buffer)
                    return ((TResult*)&((byte*)pBuf)[y * RowSize])[x];
            }
#endif
        }

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public sealed override void DoWriteRaw<TValue>(int x, int y, TValue data)
        {
#if NETCOREAPP3_0_OR_GREATER
            Unsafe.Add(ref Unsafe.As<byte, TValue>(ref Unsafe.Add(ref GetPinnableReference(), y * RowSize)), x) = data;
#else
            unsafe
            {
                fixed (T* pBuf = Buffer)
                    ((TValue*)&((byte*)pBuf)[y * RowSize])[x] = data;
            }
#endif
        }

        #endregion

        #region Internal Methods

#if NETCOREAPP3_0_OR_GREATER
        internal sealed override ref byte GetPinnableReference() => ref Unsafe.As<T, byte>(ref Buffer.GetPinnableReference());
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

        [SecuritySafeCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected ref T GetPixelRef(int y, int x) => ref Buffer.GetElementReferenceUnchecked(y, x);

        protected override void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;

            base.Dispose(disposing);
            if (disposing)
            {
                if (ownsBuffer)
                    Buffer.Dispose();
                Buffer = default;
            }
        }

        #endregion

        #endregion
    }
}