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
using System.Security;

using KGySoft.Collections;

#endregion

namespace KGySoft.Drawing.Imaging
{
    /// <summary>
    /// Represents a managed bitmap data wrapper backed by a 1D array (wrapped into an <see cref="Array2D{T}"/>).
    /// </summary>
    internal sealed class ManagedBitmapDataWrapper<T, TRow> : ManagedBitmapDataSingleArrayBased<T>
        where T : unmanaged
        where TRow : ManagedBitmapDataRowBase<T>, new()
    {
        #region Fields

        private TRow? lastRow;

        #endregion

        #region Constructors

        [SecuritySafeCritical]
        internal unsafe ManagedBitmapDataWrapper(Array2D<T> buffer, int pixelWidth, PixelFormat pixelFormat, Color32 backColor, byte alphaThreshold,
            Palette? palette, Action<Palette>? setPalette, Action? disposeCallback)
            : base(new Size(pixelWidth, buffer.Height), pixelFormat, backColor, alphaThreshold, palette, setPalette, disposeCallback)
        {
            Debug.Assert(pixelFormat.IsValidFormat(), "Valid format expected");

            Buffer = buffer;
            RowSize = buffer.Width * sizeof(T);
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