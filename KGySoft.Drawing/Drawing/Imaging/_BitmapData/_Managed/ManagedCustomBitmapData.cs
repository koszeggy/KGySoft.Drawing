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
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;

using KGySoft.Collections;

#endregion

namespace KGySoft.Drawing.Imaging
{
    /// <summary>
    /// Represents a managed bitmap data wrapper with custom pixel format for a 1D array (wrapped into an <see cref="Array2D{T}"/>).
    /// </summary>
    internal sealed class ManagedCustomBitmapData<T> : ManagedBitmapData1DArrayBase<T>
        where T : unmanaged
    {
        #region ManagedCustomBitmapDataRow class

        private sealed class ManagedCustomBitmapDataRow : ManagedBitmapDataRowBase<T>
        {
            #region Methods

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override Color32 DoGetColor32(int x) => ((ManagedCustomBitmapData<T>)BitmapData).rowGetColor.Invoke(BitmapData, Row, x);

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetColor32(int x, Color32 c) => ((ManagedCustomBitmapData<T>)BitmapData).rowSetColor.Invoke(BitmapData, Row, x, c);

            #endregion
        }

        #endregion

        #region Fields

        private RowGetColor<ArraySection<T>, Color32> rowGetColor;
        private RowSetColor<ArraySection<T>, Color32> rowSetColor;

        /// <summary>
        /// The cached lastly accessed row. Though may be accessed from multiple threads it is intentionally not volatile
        /// so it has a bit higher chance that every thread sees the last value was set by itself and no recreation is needed.
        /// </summary>
        private ManagedCustomBitmapDataRow? lastRow;

        #endregion

        #region Properties

        public override bool IsCustomPixelFormat => true;

        #endregion

        #region Constructors

        public ManagedCustomBitmapData(Array2D<T> buffer, int pixelWidth, PixelFormat pixelFormat,
            RowGetColor<ArraySection<T>, Color32> rowGetColor, RowSetColor<ArraySection<T>, Color32> rowSetColor,
            Color32 backColor, byte alphaThreshold, Action? disposeCallback)
            : base(buffer, new Size(pixelWidth, buffer.Height), pixelFormat, backColor, alphaThreshold, null, null, disposeCallback)
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
            ManagedCustomBitmapDataRow? result = lastRow;
            if (result?.Index == y)
                return result;

            // Otherwise, we create and cache the result.
            return lastRow = new ManagedCustomBitmapDataRow
            {
                Row = Buffer[y],
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
