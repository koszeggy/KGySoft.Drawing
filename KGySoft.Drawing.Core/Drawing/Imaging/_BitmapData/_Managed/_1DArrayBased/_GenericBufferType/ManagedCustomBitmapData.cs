#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ManagedCustomBitmapData.cs
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
    /// <summary>
    /// Represents a managed bitmap data wrapper with custom pixel format for a 1D array (wrapped into an <see cref="Array2D{T}"/>).
    /// </summary>
    internal sealed class ManagedCustomBitmapData<T> : ManagedBitmapData1DArrayBase<T, ManagedCustomBitmapData<T>.Row>, ICustomBitmapData
        where T : unmanaged
    {
        #region Row class

        internal sealed class Row : ManagedBitmapDataRowBase<T>, ICustomBitmapDataRow<T>
        {
            #region Indexers

            ref T ICustomBitmapDataRow<T>.this[int index] => ref Row.GetElementReference(index);

            #endregion

            #region Methods

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override Color32 DoGetColor32(int x) => ((ManagedCustomBitmapData<T>)BitmapData).rowGetColor.Invoke(this, x);

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetColor32(int x, Color32 c) => ((ManagedCustomBitmapData<T>)BitmapData).rowSetColor.Invoke(this, x, c);

            [SecuritySafeCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public unsafe ref TValue GetRefAs<TValue>(int x) where TValue : unmanaged
            {
                if ((x + 1) * sizeof(TValue) > BitmapData.RowSize)
                    ThrowXOutOfRange();
                return ref UnsafeGetRefAs<TValue>(x);
            }

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public ref TValue UnsafeGetRefAs<TValue>(int x) where TValue : unmanaged
            {
#if NETCOREAPP3_0_OR_GREATER
                return ref Unsafe.Add(ref Unsafe.As<T, TValue>(ref Row.GetPinnableReference()), x);
#else
                unsafe
                {
                    fixed (T* pRow = Row)
                        return ref ((TValue*)pRow)[x];
                }
#endif
            }

            #endregion
        }

        #endregion

        #region Fields

        private Func<ICustomBitmapDataRow<T>, int, Color32> rowGetColor;
        private Action<ICustomBitmapDataRow<T>, int, Color32> rowSetColor;

        #endregion

        #region Properties

        public override bool IsCustomPixelFormat => true;

        public unsafe Func<Size, IBitmapDataInternal> CreateCompatibleBitmapDataFactory
        {
            [SecuritySafeCritical]
            get
            {
                if (IsDisposed)
                    ThrowDisposed();

                // Creating locals for all used members so self reference will not be captured.
                Func<ICustomBitmapDataRow<T>, int, Color32> getter = rowGetColor;
                Action<ICustomBitmapDataRow<T>, int, Color32> setter = rowSetColor;
                Color32 backColor = BackColor;
                byte alphaThreshold = AlphaThreshold;
                var pixelFormat = PixelFormat;
                int origWidth = Width;
                int origBufferWidth = Buffer.Width;
                return size =>
                {
                    Debug.Assert(size.Width > 0 && size.Height > 0);
                    Array2D<T> newBuffer;

                    // original width: the original stride must be alright
                    if (size.Width == origWidth)
                        newBuffer = new Array2D<T>(size.Height, origBufferWidth);
                    else
                    {
                        // new width: assuming at least 16 byte units for custom ICustomBitmapDataRow casts
                        int stride = pixelFormat.GetByteWidth(size.Width);
                        stride += 16 - stride % 16;
                        if (16 % sizeof(T) != 0)
                            stride += sizeof(T) - stride % sizeof(T);
                        newBuffer = new Array2D<T>(size.Height, stride / sizeof(T));
                    }

                    return BitmapDataFactory.CreateManagedCustomBitmapData(newBuffer, size.Width, pixelFormat, getter, setter, backColor, alphaThreshold, () => newBuffer.Dispose());
                };
            }
        }

        #endregion

        #region Constructors

        public ManagedCustomBitmapData(Array2D<T> buffer, int pixelWidth, PixelFormatInfo pixelFormat,
            Func<ICustomBitmapDataRow<T>, int, Color32> rowGetColor, Action<ICustomBitmapDataRow<T>, int, Color32> rowSetColor,
            Color32 backColor, byte alphaThreshold, Action? disposeCallback)
            : base(buffer, pixelWidth, pixelFormat, backColor, alphaThreshold, disposeCallback)
        {
            Debug.Assert(!pixelFormat.Indexed);

            this.rowGetColor = rowGetColor;
            this.rowSetColor = rowSetColor;
        }

        #endregion

        #region Methods

        #region Protected Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override Color32 DoGetPixel(int x, int y) => GetRowCached(y).DoGetColor32(x);
      
        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override void DoSetPixel(int x, int y, Color32 color) => GetRowCached(y).DoSetColor32(x, color);

        protected override void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;
            rowGetColor = null!;
            rowSetColor = null!;
            base.Dispose(disposing);
        }

        #endregion

        #endregion
    }
}
