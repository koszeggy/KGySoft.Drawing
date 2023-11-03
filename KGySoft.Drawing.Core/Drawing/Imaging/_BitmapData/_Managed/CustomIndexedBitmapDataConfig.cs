using System;

namespace KGySoft.Drawing.Imaging
{
    public sealed class CustomIndexedBitmapDataConfig<T> : CustomIndexedBitmapDataConfigBase
        where T : unmanaged
    {
        public Func<ICustomBitmapDataRow<T>, int, int>? RowGetColorIndex { get; set; }
        public Action<ICustomBitmapDataRow<T>, int, int>? RowSetColorIndex { get; set; }
    }
}
