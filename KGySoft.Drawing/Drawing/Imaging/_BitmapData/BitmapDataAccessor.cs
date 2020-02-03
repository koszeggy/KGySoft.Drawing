#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BitmapDataAccessor.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2019 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution. If not, then this file is considered as
//  an illegal copy.
//
//  Unauthorized copying of this file, via any medium is strictly prohibited.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System.Drawing;
using System.Drawing.Imaging;

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal class BitmapDataAccessor<TRow> : BitmapDataAccessorBase
        where TRow : BitmapDataRowBase, new()
    {
        #region Fields

        /// <summary>
        /// The cached lastly accessed row. Though may be accessed from multiple threads it is intentionally not volatile
        /// so it has a bit higher chance that every thread sees the last value was set by itself and no recreation is needed.
        /// </summary>
        private TRow lastRow;

        #endregion

        #region Constructors

        internal BitmapDataAccessor(Bitmap bitmap, PixelFormat pixelFormat, ImageLockMode lockMode, Color32 backColor, byte alphaThreshold)
            : base(bitmap, pixelFormat, lockMode, backColor, alphaThreshold, null)
        {
        }

        internal BitmapDataAccessor(Bitmap bitmap, PixelFormat pixelFormat, ImageLockMode lockMode, IQuantizingSession quantizingSession)
            : base(bitmap, pixelFormat, lockMode, quantizingSession.BackColor, quantizingSession.AlphaThreshold, quantizingSession.Palette)
        {
        }

        #endregion

        #region Methods

        internal sealed override unsafe BitmapDataRowBase GetRow(int row)
        {
            // If the same row is accessed repeatedly we return the cached last row.
            TRow result = lastRow;
            if (result?.RowIndex == row)
                return result;

            // Otherwise, we create and cache the result.
            result = new TRow
            {
                Address = row == 0 ? (byte*)Scan0 : (byte*)Scan0 + Stride * row,
                Accessor = this,
                RowIndex = row,
            };

            return lastRow = result;
        }

        #endregion
    }
}