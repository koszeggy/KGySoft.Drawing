#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: UnmanagedCustomBitmapData.cs
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

using KGySoft.Collections;

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal sealed class UnmanagedCustomBitmapData : UnmanagedBitmapDataBase, ICustomBitmapData
    {
        #region UnmanagedCustomBitmapDataRow class

        private sealed class UnmanagedCustomBitmapDataRow : UnmanagedBitmapDataRowBase, ICustomBitmapDataRow
        {
            #region Methods

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override Color32 DoGetColor32(int x) => ((UnmanagedCustomBitmapData)BitmapData).rowGetColor.Invoke( this, x);

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetColor32(int x, Color32 c) => ((UnmanagedCustomBitmapData)BitmapData).rowSetColor.Invoke(this, x, c);

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

        private Func<ICustomBitmapDataRow, int, Color32> rowGetColor;
        private Action<ICustomBitmapDataRow, int, Color32> rowSetColor;

        /// <summary>
        /// The cached lastly accessed row. Though may be accessed from multiple threads it is intentionally not volatile
        /// so it has a bit higher chance that every thread sees the last value was set by itself and no recreation is needed.
        /// </summary>
        private UnmanagedCustomBitmapDataRow? lastRow;

        #endregion

        #region Properties

        public override bool IsCustomPixelFormat => true;

        public Func<Size, IBitmapDataInternal> CreateCompatibleBitmapDataFactory
        {
            get
            {
                if (IsDisposed)
                    ThrowDisposed();

                // Creating locals for all used members so self reference will not be captured.
                Func<ICustomBitmapDataRow, int, Color32> getter = rowGetColor;
                Action<ICustomBitmapDataRow, int, Color32> setter = rowSetColor;
                Color32 backColor = BackColor;
                byte alphaThreshold = AlphaThreshold;
                var pixelFormat = PixelFormat;
                int origWidth = Width;
                int origStride = RowSize;
                return size =>
                {
                    Debug.Assert(size.Width > 0 && size.Height > 0);
                    Array2D<byte> newBuffer;

                    // original width: the original stride must be alright
                    if (size.Width == origWidth)
                        newBuffer = new Array2D<byte>(size.Height, origStride);
                    else
                    {
                        // new width: assuming at least 16 byte units for custom ICustomBitmapDataRow casts
                        int stride = pixelFormat.GetByteWidth(size.Width);
                        stride += 16 - stride % 16;
                        newBuffer = new Array2D<byte>(size.Height, stride);
                    }

                    return BitmapDataFactory.CreateManagedCustomBitmapData(newBuffer, size.Width, new PixelFormatInfo(pixelFormat), getter, setter, backColor, alphaThreshold, () => newBuffer.Dispose());
                };
            }
        }

        #endregion

        #region Constructors

        internal UnmanagedCustomBitmapData(IntPtr buffer, Size size, int stride, PixelFormat pixelFormat,
            Func<ICustomBitmapDataRow, int, Color32> rowGetColor, Action<ICustomBitmapDataRow, int, Color32> rowSetColor,
            Color32 backColor, byte alphaThreshold, Action? disposeCallback)
            : base(buffer, size, stride, pixelFormat, backColor, alphaThreshold, null, null, disposeCallback)
        {
            Debug.Assert(!pixelFormat.IsIndexed());
            this.rowGetColor = rowGetColor;
            this.rowSetColor = rowSetColor;
        }

        #endregion

        #region Methods

        #region Public Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override IBitmapDataRowInternal DoGetRow(int y)
        {
            // If the same row is accessed repeatedly we return the cached last row.
            UnmanagedCustomBitmapDataRow? result = lastRow;
            if (result?.Index == y)
                return result;

            // Otherwise, we create and cache the result.
            return lastRow = new UnmanagedCustomBitmapDataRow
            {
                Row = y == 0 ? Scan0 : Scan0 + Stride * y,
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
            rowGetColor = null!;
            rowSetColor = null!;
            base.Dispose(disposing);
        }

        #endregion

        #endregion
    }
}
