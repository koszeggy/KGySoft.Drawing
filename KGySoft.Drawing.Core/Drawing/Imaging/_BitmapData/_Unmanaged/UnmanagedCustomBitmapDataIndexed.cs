#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: UnmanagedCustomBitmapDataIndexed.cs
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
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;
using System.Security;

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal sealed class UnmanagedCustomBitmapDataIndexed : UnmanagedBitmapDataBase
    {
        #region UnmanagedCustomBitmapDataRowIndexed class

        private sealed class UnmanagedCustomBitmapDataRowIndexed : UnmanagedBitmapDataRowIndexedBase, ICustomBitmapDataRow
        {
            #region Properties

            protected override uint MaxIndex => (1u << BitmapData.PixelFormat.BitsPerPixel) - 1u;

            #endregion

            #region Methods

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override int DoGetColorIndex(int x) => ((UnmanagedCustomBitmapDataIndexed)BitmapData).rowGetColorIndex.Invoke(this, x);

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetColorIndex(int x, int colorIndex) => ((UnmanagedCustomBitmapDataIndexed)BitmapData).rowSetColorIndex.Invoke(this, x, colorIndex);

            [SecuritySafeCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public unsafe ref T GetRefAs<T>(int x) where T : unmanaged
            {
                if ((x + 1) * sizeof(T) > BitmapData.RowSize)
                    ThrowXOutOfRange();
                return ref UnsafeGetRefAs<T>(x);
            }

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public unsafe ref T UnsafeGetRefAs<T>(int x) where T : unmanaged => ref ((T*)Row)[x];

            #endregion
        }

        #endregion

        #region Fields

        private Func<ICustomBitmapDataRow, int, int> rowGetColorIndex;
        private Action<ICustomBitmapDataRow, int, int> rowSetColorIndex;

        /// <summary>
        /// The cached lastly accessed row. Though may be accessed from multiple threads it is intentionally not volatile
        /// so it has a bit higher chance that every thread sees the last value was set by itself and no recreation is needed.
        /// </summary>
        private UnmanagedCustomBitmapDataRowIndexed? lastRow;

        #endregion

        #region Properties

        public override bool IsCustomPixelFormat => true;

        #endregion

        #region Constructors

        [SecurityCritical]
        public UnmanagedCustomBitmapDataIndexed(IntPtr buffer, Size size, int stride, PixelFormatInfo pixelFormat,
            Func<ICustomBitmapDataRow, int, int> rowGetColorIndex, Action<ICustomBitmapDataRow, int, int> rowSetColorIndex,
            Palette? palette, Func<Palette, bool>? trySetPaletteCallback, Action? disposeCallback)
            : base(buffer, size, stride, pixelFormat, palette?.BackColor ?? default, palette?.AlphaThreshold ?? 128, palette, trySetPaletteCallback, disposeCallback)
        {
            Debug.Assert(pixelFormat.Indexed);

            this.rowGetColorIndex = rowGetColorIndex;
            this.rowSetColorIndex = rowSetColorIndex;
        }

        #endregion

        #region Methods

        #region Public Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override IBitmapDataRowInternal DoGetRow(int y)
        {
            // If the same row is accessed repeatedly we return the cached last row.
            UnmanagedCustomBitmapDataRowIndexed? result = lastRow;
            if (result?.Index == y)
                return result;

            // Otherwise, we create and cache the result.
            return lastRow = new UnmanagedCustomBitmapDataRowIndexed
            {
#if NET35
                Row = y == 0 ? Scan0 : new IntPtr(Scan0.ToInt64() + Stride * y),
#else
                Row = y == 0 ? Scan0 : Scan0 + Stride * y,
#endif
                BitmapData = this,
                Index = y,
            };
        }

        #endregion

        #region Protected Methods

        protected override void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;
            rowGetColorIndex = null!;
            rowSetColorIndex = null!;
            base.Dispose(disposing);
        }

        #endregion

        #endregion
    }
}
