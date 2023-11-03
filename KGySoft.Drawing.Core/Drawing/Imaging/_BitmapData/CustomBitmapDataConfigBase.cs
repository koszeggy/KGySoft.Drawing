using System;

namespace KGySoft.Drawing.Imaging
{
    public abstract class CustomBitmapDataConfigBase
    {
        public PixelFormatInfo PixelFormat { get; set; }
        public Action? DisposeCallback { get; set; }
        //public bool InstanceIndependentPixelAccess { get; set; }
    }
}