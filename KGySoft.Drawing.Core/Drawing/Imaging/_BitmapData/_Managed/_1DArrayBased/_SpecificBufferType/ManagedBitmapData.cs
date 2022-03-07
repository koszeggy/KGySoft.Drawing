#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ManagedBitmapData.cs
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

using System.Drawing;
using System.Drawing.Imaging;

using KGySoft.Collections;

#endregion

namespace KGySoft.Drawing.Imaging
{
    /// <summary>
    /// Represents a self-allocating managed bitmap data
    /// </summary>
    internal sealed class ManagedBitmapData<T, TRow> : ManagedBitmapData1DArrayBase<T>
        where T : unmanaged
        where TRow : ManagedBitmapDataRowBase<T>, new()
    {
        #region Fields

        private TRow? lastRow;

        #endregion

        #region Constructors

        internal ManagedBitmapData(Size size, PixelFormatInfo pixelFormat, Color32 backColor = default, byte alphaThreshold = 0, Palette? palette = null)
            : base(new Array2D<T>(size.Height, pixelFormat.BitsPerPixel <= 8 ? pixelFormat.GetByteWidth(size.Width) : size.Width),
                size, pixelFormat, backColor, alphaThreshold, palette, null, null)
        {
            Debug.Assert(pixelFormat.IsKnownFormat, "Known format expected");
            Debug.Assert(!pixelFormat.Indexed || typeof(T) == typeof(byte), "For indexed pixel formats byte elements are expected");
        }

        #endregion

        #region Methods

        #region Public Methods

        public override IBitmapDataRowInternal DoGetRow(int y)
        {
            // If the same row is accessed repeatedly we return the cached last row.
            TRow? result = lastRow;
            if (result?.Index == y)
                return result;

            // Otherwise, we create and cache the result.
            return lastRow = new TRow
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
            if (disposing)
                Buffer.Dispose();
            base.Dispose(disposing);
        }

        #endregion

        #endregion
    }
}