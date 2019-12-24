using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KGySoft.Drawing.Imaging
{
    internal class BitmapDataAccessor<TRow> : BitmapDataAccessorBase
        where TRow : BitmapDataRowBase, new()
    {
        private TRow lastRow;

        protected sealed override BitmapDataRowBase GetRow(int row)
        {
            TRow result = lastRow;
            if (result?.Line == row)
                return result;

            return lastRow = CreateRow(row);
        }

        protected virtual unsafe TRow CreateRow(int row) =>
            new TRow
            {
                Address = row == 0 ? (byte*)Scan0 : (byte*)Scan0 + Stride * row,
                Accessor = this,
                Line = row,
            };

        internal BitmapDataAccessor(Bitmap bitmap, PixelFormat pixelFormat, ImageLockMode lockMode)
            : base(bitmap, pixelFormat, lockMode)
        {
        }
    }
}
