#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ManagedBitmapData1DArrayBase`1.cs
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
using System.Drawing;
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

        #region Internal Fields

        /// <summary>
        /// The pixel buffer where the underlying array is a single dimensional one.
        /// It is a field rather than a property so Dispose allows mutating it.
        /// </summary>
        internal Array2D<T> Buffer;

        #endregion

        #region Private Fields

        private readonly bool ownsBuffer;

        #endregion

        #endregion

        #region Constructors

        protected unsafe ManagedBitmapData1DArrayBase(Size size, KnownPixelFormat pixelFormat, Color32 backColor, byte alphaThreshold, Palette? palette)
            : base(size, pixelFormat.ToInfoInternal(), backColor, alphaThreshold, palette, null, null)
        {
            Debug.Assert(!pixelFormat.IsIndexed() || typeof(T) == typeof(byte), "For indexed pixel formats byte elements are expected");
            Buffer = new Array2D<T>(size.Height, pixelFormat.ToBitsPerPixel() <= 8 ? pixelFormat.GetByteWidth(size.Width) : size.Width);
            ownsBuffer = true;
            RowSize = Buffer.Width * sizeof(T);
        }

        [SecuritySafeCritical]
        protected unsafe ManagedBitmapData1DArrayBase(Array2D<T> buffer, int pixelWidth, PixelFormatInfo pixelFormat, Color32 backColor, byte alphaThreshold,
            Palette? palette, Func<Palette, bool>? trySetPaletteCallback, Action? disposeCallback)
            : base(new Size(pixelWidth, buffer.Height), pixelFormat, backColor, alphaThreshold, palette, trySetPaletteCallback, disposeCallback)
        {
            Buffer = buffer;
            RowSize = buffer.Width * sizeof(T);
        }

        #endregion

        #region Methods

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

        protected override void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;
            if (disposing)
            {
                if (ownsBuffer)
                    Buffer.Dispose();
                else
                    Buffer = default;
            }

            base.Dispose(disposing);
        }

        #endregion

        #endregion
    }
}