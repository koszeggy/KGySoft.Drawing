using System;

namespace KGySoft.Drawing.Imaging
{
    public sealed class CustomIndexedBitmapDataConfig : CustomIndexedBitmapDataConfigBase
    {
        public Func<ICustomBitmapDataRow, int, int>? RowGetColorIndex { get; set; }
        public Action<ICustomBitmapDataRow, int, int>? RowSetColorIndex { get; set; }
    }
}
