namespace KGySoft.Drawing.Imaging
{
    public abstract class CustomNonIndexedBitmapDataConfigBase : CustomBitmapDataConfigBase
    {
        internal Color32 BackColor { get; set; }
        internal byte AlphaThreshold { get; set; }
        public WorkingColorSpace WorkingColorSpace { get; set; }
    }
}