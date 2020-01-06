#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BitmapDataAccessorSingleBitAlpha.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2020 - All Rights Reserved
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
    internal class BitmapDataAccessorSingleBitAlpha<TRow> : BitmapDataAccessorNoAlpha<TRow>
        where TRow : BitmapDataRowSingleBitAlphaBase, new()
    {
        #region Fields

        private readonly byte alphaThreshold;

        #endregion

        #region Constructors

        public BitmapDataAccessorSingleBitAlpha(Bitmap bitmap, PixelFormat pixelFormat, ImageLockMode lockMode, Color backColor, byte alphaThreshold)
            : base(bitmap, pixelFormat, lockMode, backColor)
        {
            this.alphaThreshold = alphaThreshold;
        }

        #endregion

        #region Methods

        protected override TRow CreateRow(int row)
        {
            TRow result = base.CreateRow(row);
            result.AlphaThreshold = alphaThreshold;
            return result;
        }

        #endregion
    }
}