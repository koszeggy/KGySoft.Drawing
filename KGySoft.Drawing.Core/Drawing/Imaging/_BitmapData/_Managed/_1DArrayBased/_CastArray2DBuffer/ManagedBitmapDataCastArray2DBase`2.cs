#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ManagedBitmapDataCastArray2DBase`2.cs
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
    internal abstract class ManagedBitmapDataCastArray2DBase<T, TPixel> : ManagedBitmapDataBase
        where T : unmanaged
        where TPixel : unmanaged
    {
        #region Fields

        #region Internal Fields

        internal CastArray2D<T, TPixel> Buffer;

        #endregion

        #region Private Fields

        private readonly bool ownsBuffer;

        private ArraySection<T> underlyingBuffer;

        #endregion

        #endregion

        #region Constructors

        protected unsafe ManagedBitmapDataCastArray2DBase(in BitmapDataConfig cfg)
            : base(cfg)
        {
            Debug.Assert(!cfg.PixelFormat.IsCustomFormat, "In this overload known pixel format is expected");
            Debug.Assert(cfg.DisposeCallback == null, "No dispose callback is expected from the self-allocating constructor");
            Debug.Assert(typeof(T) == typeof(byte) && !cfg.PixelFormat.Indexed, "In the CastArray2D-based self-allocating constructor the underlying type expected to be byte for formats whose actual element type is not byte.");
            Debug.Assert(BitmapDataFactory.PoolingStrategy != ArrayPoolingStrategy.Never, "When pooling is disabled, calling the CastArray-based self allocation is not expected.");

            int byteWidth = cfg.PixelFormat.GetByteWidth(cfg.Size.Width);
            if (BitmapDataFactory.PoolingStrategy >= ArrayPoolingStrategy.IfByteArrayBased)
            {
                // Using the self-allocating constructor that allows array pooling.
                underlyingBuffer = new ArraySection<T>(cfg.Size.Height * byteWidth);
                ownsBuffer = true;
            }
            else
            {
                // Passing an array to the underlying buffer to avoid array pooling. Based on the Assert above, this should not occur.
                underlyingBuffer = new T[cfg.Size.Height * byteWidth];
            }

            Buffer = new CastArray2D<T, TPixel>(underlyingBuffer, cfg.Size.Height, cfg.Size.Width);
            RowSize = Buffer.Width * sizeof(T);
        }

        [SecuritySafeCritical]
        protected unsafe ManagedBitmapDataCastArray2DBase(CastArray2D<T, TPixel> array2D, in BitmapDataConfig cfg)
            : base(cfg)
        {
            underlyingBuffer = array2D.Buffer.Buffer;
            Buffer = array2D;
            RowSize = array2D.Width * sizeof(TPixel);
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
                fixed (byte* pBuf = &GetPinnableReference())
                    return ((TResult*)&pBuf[y * RowSize])[x];
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
                fixed (byte* pBuf = &GetPinnableReference())
                    ((TValue*)&pBuf[y * RowSize])[x] = data;
            }
#endif
        }

        #endregion

        #region Internal Methods

#if NETCOREAPP3_0_OR_GREATER
        internal sealed override ref byte GetPinnableReference() => ref Unsafe.As<T, byte>(ref underlyingBuffer.GetPinnableReference());
#else
        [SecuritySafeCritical]
        internal sealed override unsafe ref byte GetPinnableReference()
        {
            ref T head = ref underlyingBuffer.GetPinnableReference();
            fixed (T* pHead = &head)
                return ref *(byte*)pHead;
        }
#endif

        #endregion

        #region Protected Methods

        [SecuritySafeCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected ref TPixel GetPixelRef(int y, int x) => ref Buffer.GetElementReferenceUnsafe(y, x);

        protected override void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;

            base.Dispose(disposing);
            if (disposing)
            {
                if (ownsBuffer)
                    underlyingBuffer.Release();
                underlyingBuffer = default;
                Buffer = default;
            }
        }

        #endregion

        #endregion
    }
}