using System;

namespace KGySoft.Drawing.Imaging
{
    public abstract class CustomIndexedBitmapDataConfigBase : CustomBitmapDataConfigBase
    {
        public Palette? Palette { get; set; }
        public Func<Palette, bool>? TrySetPaletteCallback { get; set; }
    }
}