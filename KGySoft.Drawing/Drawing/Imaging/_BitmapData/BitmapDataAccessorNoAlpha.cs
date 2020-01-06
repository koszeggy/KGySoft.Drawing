#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BitmapDataAccessorNoAlpha.cs
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
    internal class BitmapDataAccessorNoAlpha<TRow> : BitmapDataAccessor<TRow>
        where TRow : BitmapDataRowNoAlphaBase, new()
    {
        #region Fields

        private readonly Color32 backColor;

        #endregion

        #region Constructors

        public BitmapDataAccessorNoAlpha(Bitmap bitmap, PixelFormat pixelFormat, ImageLockMode lockMode, Color backColor)
            : base(bitmap, pixelFormat, lockMode)
        {
            this.backColor = new Color32(backColor);
        }

        #endregion

        #region Methods

        protected override TRow CreateRow(int row)
        {
            TRow result = base.CreateRow(row);
            result.BackColor = backColor;
            return result;
        }

        #endregion
    }
}