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

        private TRow lastRow;

        #endregion

        #region Constructors

        internal BitmapDataAccessor(Bitmap bitmap, PixelFormat pixelFormat, ImageLockMode lockMode)
            : base(bitmap, pixelFormat, lockMode)
        {
        }

        #endregion

        #region Methods

        #region Internal Methods

        internal sealed override BitmapDataRowBase GetRow(int row)
        {
            TRow result = lastRow;
            if (result?.Line == row)
                return result;

            return lastRow = CreateRow(row);
        }

        #endregion

        #region Protected Methods

        protected virtual unsafe TRow CreateRow(int row) =>
            new TRow
            {
                Address = row == 0 ? (byte*)Scan0 : (byte*)Scan0 + Stride * row,
                Accessor = this,
                Line = row,
            };

        #endregion

        #endregion
    }
}