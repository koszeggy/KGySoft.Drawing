#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BitmapDataAccessorIndexed.cs
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
    internal class BitmapDataAccessorIndexed<TRow> : BitmapDataAccessor<TRow>
        where TRow : BitmapDataRowIndexedBase, new()
    {
        #region Fields

        private readonly Palette palette;

        #endregion

        #region Constructors

        internal BitmapDataAccessorIndexed(Bitmap bitmap, PixelFormat pixelFormat, ImageLockMode lockMode, Color backColor, byte alphaThreshold)
            : base(bitmap, pixelFormat, lockMode)
        {
            palette = new Palette(bitmap.Palette.Entries, backColor, alphaThreshold);
        }

        #endregion

        #region Methods

        protected override TRow CreateRow(int row)
        {
            TRow result = base.CreateRow(row);
            result.Palette = palette;
            return result;
        }

        #endregion
    }
}