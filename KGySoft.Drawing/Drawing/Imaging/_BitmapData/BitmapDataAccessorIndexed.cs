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

        #region Properties

        public override Color BackColor
        {
            get => base.BackColor;
            set
            {
                base.BackColor = value;
                palette.BackColor = BackColor32;
            }
        }

        public override byte AlphaThreshold
        {
            get => base.AlphaThreshold;
            set
            {
                base.AlphaThreshold = value;
                palette.AlphaThreshold = value;
            }
        }

        #endregion

        #region Constructors

        internal BitmapDataAccessorIndexed(Bitmap bitmap, PixelFormat pixelFormat, ImageLockMode lockMode)
            : base(bitmap, pixelFormat, lockMode)
        {
            palette = new Palette(bitmap.Palette.Entries);
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