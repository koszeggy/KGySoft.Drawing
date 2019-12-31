using System;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace KGySoft.Drawing.Imaging
{
    internal struct BitmapLine
    {
        private unsafe byte* line;
        private readonly PixelFormat pixelFormat;

        internal unsafe BitmapLine(BitmapData bitmapData, int y)
        {
            line = (byte*)bitmapData.Scan0 + bitmapData.Stride * y;
            pixelFormat = bitmapData.PixelFormat;
        }

        internal unsafe Color32 GetColor32(int x)
        {
            switch (pixelFormat)
            {
                //case PixelFormat.Format16bppRgb555:
                //    return Color32.FromRgb555(((ushort*)line)[x]);
                //case PixelFormat.Format16bppRgb565:
                //    return Color32.FromRgb565(((ushort*)line)[x]);
                //case PixelFormat.Format16bppArgb1555:
                //    return Color32.FromArgb1555(((ushort*)line)[x]);
                case PixelFormat.Format24bppRgb:
                    return ((Color24*)line)[x].ToColor32();
                case PixelFormat.Format32bppRgb:
                    return Color32.FromRgb(((int*)line)[x]);
                case PixelFormat.Format32bppArgb:
                case PixelFormat.Format32bppPArgb:
                    return ((Color32*)line)[x];
                case PixelFormat.Format16bppGrayScale:
                    return Color32.FromGray16(((ushort*)line)[x]);
                case PixelFormat.Format48bppRgb:
                    return ((Color48*)line)[x].ToColor32();
                case PixelFormat.Format64bppArgb:
                case PixelFormat.Format64bppPArgb:
                    return Color64.FromRgb(((long*)line)[x]).ToColor32();
            }

            throw new InvalidOperationException(Res.InternalError($"Unexpected pixelFormat in {nameof(GetColor32)}: {pixelFormat}"));
        }
    }
}
