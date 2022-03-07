#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ManagedBitmapDataWrapper.cs
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

using KGySoft.Collections;

#endregion

namespace KGySoft.Drawing.Imaging
{
    /// <summary>
    /// Represents a managed bitmap data wrapper for a 1D array (wrapped into an <see cref="Array2D{T}"/>).
    /// </summary>
    internal sealed class ManagedBitmapDataWrapper<T, TRow> : ManagedBitmapData1DArrayBase<T>
        where T : unmanaged
        where TRow : ManagedBitmapDataRowBase<T>, new()
    {
        #region Fields

        private TRow? lastRow;

        #endregion

        #region Constructors

        internal ManagedBitmapDataWrapper(Array2D<T> buffer, int pixelWidth, PixelFormatInfo pixelFormat, Color32 backColor, byte alphaThreshold,
            Palette? palette, Func<Palette, bool>? trySetPaletteCallback, Action? disposeCallback)
            : base(buffer, new Size(pixelWidth, buffer.Height), pixelFormat, backColor, alphaThreshold, palette, trySetPaletteCallback, disposeCallback)
        {
            Debug.Assert(pixelFormat.IsKnownFormat, "Known format expected");
        }

        #endregion

        #region Methods

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
    }
}