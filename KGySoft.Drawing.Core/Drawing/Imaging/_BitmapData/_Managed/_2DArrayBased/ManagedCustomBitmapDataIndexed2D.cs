#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ManagedCustomBitmapDataIndexed2D.cs
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

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal sealed class ManagedCustomBitmapDataIndexed2D<T> : ManagedBitmapData2DArrayBase<T, ManagedCustomBitmapDataIndexed2D<T>.Row>
        where T : unmanaged
    {
        #region Row class

        internal sealed class Row : ManagedBitmapDataRowIndexed2DBase<T>, ICustomBitmapDataRow<T>
        {
            #region Properties and Indexers

            #region Properties

            protected override uint MaxIndex => (1u << BitmapData.PixelFormat.BitsPerPixel) - 1u;

            #endregion

            #region Indexers

            ref T ICustomBitmapDataRow<T>.this[int index] => ref Buffer[Index, index];

            #endregion

            #endregion

            #region Methods

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override int DoGetColorIndex(int x) => ((ManagedCustomBitmapDataIndexed2D<T>)BitmapData).rowGetColorIndex.Invoke(this, x);

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetColorIndex(int x, int colorIndex) => ((ManagedCustomBitmapDataIndexed2D<T>)BitmapData).rowSetColorIndex.Invoke(this, x, colorIndex);

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
                return ref Unsafe.Add(ref Unsafe.As<T, TValue>(ref Buffer[Index, 0]), x);
#else
                unsafe
                {
                    fixed (T* pRow = &Buffer[Index, 0])
                        return ref ((TValue*)pRow)[x];
                }
#endif
            }

            #endregion
        }

        #endregion

        #region Fields

        private Func<ICustomBitmapDataRow<T>, int, int> rowGetColorIndex;
        private Action<ICustomBitmapDataRow<T>, int, int> rowSetColorIndex;

        /// <summary>
        /// The cached lastly accessed row. Though may be accessed from multiple threads it is intentionally not volatile
        /// so it has a bit higher chance that every thread sees the last value was set by itself and no recreation is needed.
        /// </summary>
        private Row? lastRow;

        #endregion

        #region Properties

        public override bool IsCustomPixelFormat => true;

        #endregion

        #region Constructors

        internal ManagedCustomBitmapDataIndexed2D(T[,] buffer, int pixelWidth, PixelFormatInfo pixelFormat,
            Func<ICustomBitmapDataRow<T>, int, int> rowGetColorIndex, Action<ICustomBitmapDataRow<T>, int, int> rowSetColorIndex,
            Palette? palette, Func<Palette, bool>? trySetPaletteCallback, Action? disposeCallback)
            : base(buffer, pixelWidth, pixelFormat, palette?.BackColor ?? default, palette?.AlphaThreshold ?? 128,
                disposeCallback, palette, trySetPaletteCallback)
        {
            Debug.Assert(pixelFormat.Indexed);

            this.rowGetColorIndex = rowGetColorIndex;
            this.rowSetColorIndex = rowSetColorIndex;
        }

        #endregion

        #region Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override Color32 DoGetPixel(int x, int y) => GetRowCached(y).DoGetColor32(x);
    
        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override void DoSetPixel(int x, int y, Color32 color) => GetRowCached(y).DoSetColor32(x, color);

        protected override void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;
            rowGetColorIndex = null!;
            rowSetColorIndex = null!;
            base.Dispose(disposing);
        }

        #endregion
    }
}
