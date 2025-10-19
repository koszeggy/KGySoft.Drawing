#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ManagedBitmapDataArray2DBase`1.cs
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
            Debug.Assert(typeof(T) == typeof(byte) && cfg.PixelFormat.Indexed || sizeof(T) * 8 == cfg.PixelFormat.BitsPerPixel, "In the self-allocating constructor the underlying type expected to be a type of the same matching the pixel format. Otherwise, a CastArray2D-based bitmap data should be used.");
            
            int bufferWidth = typeof(T) == typeof(byte) ? cfg.PixelFormat.GetByteWidth(cfg.Size.Width) : cfg.Size.Width;
            if (BitmapDataFactory.PoolingStrategy == ArrayPoolingStrategy.AnyElementType || typeof(T) == typeof(byte) && BitmapDataFactory.PoolingStrategy >= ArrayPoolingStrategy.IfByteArrayBased)
            {
                // Using the self-allocating constructor that allows array pooling.
                Buffer = new Array2D<T>(cfg.Size.Height, bufferWidth);
                ownsBuffer = true;
            }
            else
            {
                // Passing an array to the buffer to avoid array pooling.
                Buffer = new Array2D<T>(new T[checked(cfg.Size.Height * bufferWidth)], cfg.Size.Height, bufferWidth);
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
        public sealed override unsafe TResult DoReadRaw<TResult>(int x, int y)
        {
            Debug.Assert(!typeof(TResult).IsPrimitive || GetPinnableReference().At<byte, TResult>(y * RowSize, x).AsIntPtr() % sizeof(TResult) == 0, $"Misaligned raw {typeof(TResult).Name} access in row {y} at position {x} - {PixelFormat} {Width}x{Height}");
            return GetPinnableReference().At<byte, TResult>(y * RowSize, x);
        }

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public sealed override unsafe void DoWriteRaw<TValue>(int x, int y, TValue data)
        {
            Debug.Assert(!typeof(TValue).IsPrimitive || GetPinnableReference().At<byte, TValue>(y * RowSize, x).AsIntPtr() % sizeof(TValue) == 0, $"Misaligned raw {typeof(TValue).Name} access in row {y} at position {x} - {PixelFormat} {Width}x{Height}");
            GetPinnableReference().At<byte, TValue>(y * RowSize, x) = data;
        }

        #endregion

        #region Internal Methods

        [SecuritySafeCritical]
        internal sealed override ref byte GetPinnableReference() => ref Buffer.GetPinnableReference().As<T, byte>();

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