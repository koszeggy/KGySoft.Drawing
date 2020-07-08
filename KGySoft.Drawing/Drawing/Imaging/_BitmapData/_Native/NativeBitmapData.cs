#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: NativeBitmapData.cs
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
using System.Runtime.CompilerServices;
using System.Security;

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal sealed class NativeBitmapData<TRow> : NativeBitmapDataBase
        where TRow : NativeBitmapDataRowBase, new()
    {
        #region Fields

        /// <summary>
        /// The cached lastly accessed row. Though may be accessed from multiple threads it is intentionally not volatile
        /// so it has a bit higher chance that every thread sees the last value was set by itself and no recreation is needed.
        /// </summary>
        private TRow lastRow;

        #endregion

        #region Constructors

        internal NativeBitmapData(Bitmap bitmap, PixelFormat pixelFormat, ImageLockMode lockMode, Color32 backColor, byte alphaThreshold = 0, Palette palette = null)
            : base(bitmap, pixelFormat, lockMode, backColor, alphaThreshold, palette)
        {
        }

        internal NativeBitmapData(Bitmap bitmap, PixelFormat pixelFormat, ImageLockMode lockMode, IQuantizingSession quantizingSession)
            : base(bitmap, pixelFormat, lockMode, quantizingSession.BackColor, quantizingSession.AlphaThreshold, quantizingSession.Palette)
        {
        }

        #endregion

        #region Methods

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override unsafe IBitmapDataRowInternal GetRow(int row)
        {
            // If the same row is accessed repeatedly we return the cached last row.
            TRow result = lastRow;
            if (result?.Index == row)
                return result;

            // Otherwise, we create and cache the result.
            return lastRow = new TRow
            {
                Address = row == 0 ? (byte*)Scan0 : (byte*)Scan0 + Stride * row,
                BitmapData = this,
                Index = row,
            };
        }

        #endregion
    }
}