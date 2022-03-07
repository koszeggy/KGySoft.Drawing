#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ManagedBitmapDataWrapper2D.cs
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

#endregion

namespace KGySoft.Drawing.Imaging
{
    /// <summary>
    /// Represents a managed bitmap data wrapper for a 2D array
    /// </summary>
    internal sealed class ManagedBitmapDataWrapper2D<T, TRow> : ManagedBitmapData2DArrayBase<T>
        where T : unmanaged
        where TRow : ManagedBitmapDataRow2DBase<T>, new()
    {
        #region Fields

        private TRow? lastRow;

        #endregion

        #region Constructors

        internal ManagedBitmapDataWrapper2D(T[,] buffer, int pixelWidth, PixelFormatInfo pixelFormat, Color32 backColor, byte alphaThreshold,
            Palette? palette, Func<Palette, bool>? trySetPaletteCallback, Action? disposeCallback)
            : base(buffer, new Size(pixelWidth, buffer.GetLength(0)), pixelFormat, backColor, alphaThreshold, palette, trySetPaletteCallback, disposeCallback)
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
                Buffer = Buffer,
                BitmapData = this,
                Index = y,
            };
        }

        #endregion
    }
}