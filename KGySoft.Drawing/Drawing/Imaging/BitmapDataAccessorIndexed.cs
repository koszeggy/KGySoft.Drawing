using System.Drawing;
using System.Drawing.Imaging;

namespace KGySoft.Drawing.Imaging
{
    internal class BitmapDataAccessorIndexed<TRow> : BitmapDataAccessor<TRow>
        where TRow : BitmapDataRowBaseIndexed, new()
    {
        private readonly Color[] palette;

        internal BitmapDataAccessorIndexed(Bitmap bitmap, PixelFormat pixelFormat, ImageLockMode lockMode)
            : base(bitmap, pixelFormat, lockMode)
        {
            palette = bitmap.Palette.Entries;
        }

        protected override TRow CreateRow(int row)
        {
            TRow result = base.CreateRow(row);
            result.Palette = palette;
            return result;
        }
    }
}