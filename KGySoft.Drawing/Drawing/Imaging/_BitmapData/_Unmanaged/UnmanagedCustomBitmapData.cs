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

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal sealed class UnmanagedCustomBitmapData : UnmanagedCustomBitmapDataBase
    {
        #region Nested classes

        #region UnmanagedCustomBitmapDataRow class

        private sealed class UnmanagedCustomBitmapDataRow : NativeBitmapDataRowBase
        {
            #region Properties

            private UnmanagedCustomBitmapData Parent => (UnmanagedCustomBitmapData)BitmapData;

            #endregion

            #region Methods

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override Color32 DoGetColor32(int x) => Parent.rowGetColor.Invoke(BitmapData, Address, x);

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetColor32(int x, Color32 c) => Parent.rowSetColor.Invoke(BitmapData, Address, x, c);

            #endregion
        }

        #endregion

        #endregion

        #region Fields

        private Func<IBitmapData, IntPtr, int, Color32> rowGetColor;
        private Action<IBitmapData, IntPtr, int, Color32> rowSetColor;

        /// <summary>
        /// The cached lastly accessed row. Though may be accessed from multiple threads it is intentionally not volatile
        /// so it has a bit higher chance that every thread sees the last value was set by itself and no recreation is needed.
        /// </summary>
        private UnmanagedCustomBitmapDataRow? lastRow;

        #endregion

        #region Constructors

        internal UnmanagedCustomBitmapData(IntPtr buffer, Size size, int stride, PixelFormat pixelFormat,
            Func<IBitmapData, IntPtr, int, Color32> rowGetColor, Action<IBitmapData, IntPtr, int, Color32> rowSetColor,
            Color32 backColor, byte alphaThreshold, Action? disposeCallback)
            : base(buffer, size, stride, pixelFormat, backColor, alphaThreshold, null, null, disposeCallback)
        {
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
                Address = y == 0 ? Scan0 : Scan0 + Stride * y,
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
