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
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;

using KGySoft.Collections;

#endregion


namespace KGySoft.Drawing.Imaging
{
    internal sealed class ManagedCustomBitmapDataIndexed2D<T> : ManagedBitmapData2DArrayBase<T>
        where T : unmanaged
    {
        #region ManagedCustomBitmapDataRow class

        private sealed class ManagedCustomBitmapDataRow2D : ManagedBitmapDataRowIndexed2DBase<T>
        {
            #region Properties

            protected override uint MaxIndex => (1u << BitmapData.PixelFormat.ToBitsPerPixel()) - 1u;

            #endregion

            #region Methods

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override int DoGetColorIndex(int x) => ((ManagedCustomBitmapDataIndexed2D<T>)BitmapData).rowGetColorIndex.Invoke(BitmapData, ref Buffer[Index, 0], x);

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetColorIndex(int x, int colorIndex) => ((ManagedCustomBitmapDataIndexed2D<T>)BitmapData).rowSetColorIndex.Invoke(BitmapData, ref Buffer[Index, 0], x, colorIndex);

            #endregion
        }

        #endregion

        #region Fields

        private RowGetColorIndexByRef<T> rowGetColorIndex;
        private RowSetColorIndexByRef<T> rowSetColorIndex;

        /// <summary>
        /// The cached lastly accessed row. Though may be accessed from multiple threads it is intentionally not volatile
        /// so it has a bit higher chance that every thread sees the last value was set by itself and no recreation is needed.
        /// </summary>
        private ManagedCustomBitmapDataRow2D? lastRow;

        #endregion

        #region Properties

        public override bool IsCustomPixelFormat => true;

        #endregion

        #region Constructors

        public ManagedCustomBitmapDataIndexed2D(T[,] buffer, int pixelWidth, PixelFormat pixelFormat,
            RowGetColorIndexByRef<T> rowGetColorIndex, RowSetColorIndexByRef<T> rowSetColorIndex,
            Palette? palette, Action<Palette>? setPalette, Action? disposeCallback)
            : base(buffer, new Size(pixelWidth, buffer.GetLength(0)), pixelFormat, palette?.BackColor ?? default, palette?.AlphaThreshold ?? 128, palette, setPalette, disposeCallback)
        {
            Debug.Assert(pixelFormat.IsIndexed());

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
            ManagedCustomBitmapDataRow2D? result = lastRow;
            if (result?.Index == y)
                return result;

            // Otherwise, we create and cache the result.
            return lastRow = new ManagedCustomBitmapDataRow2D
            {
                Buffer = Buffer,
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
