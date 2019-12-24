using System;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace KGySoft.Drawing
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
                case PixelFormat.Format16bppRgb555:
                    return Color32.FromRgb555(((ushort*)line)[x]);
                case PixelFormat.Format16bppRgb565:
                    return Color32.FromRgb565(((ushort*)line)[x]);
                case PixelFormat.Format16bppArgb1555:
                    return Color32.FromArgb1555(((ushort*)line)[x]);
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

    [StructLayout(LayoutKind.Explicit, Size = 3)]
    internal readonly struct Color24
    {
        [FieldOffset(0)]
        internal readonly byte B;

        [FieldOffset(1)]
        internal readonly byte G;

        [FieldOffset(2)]
        internal readonly byte R;

        internal Color32 ToColor32() => new Color32(R, G, B);
    }

    [StructLayout(LayoutKind.Explicit, Size = 6)]
    internal readonly struct Color48
    {
        [FieldOffset(0)]
        internal readonly ushort B;

        [FieldOffset(2)]
        internal readonly ushort G;

        [FieldOffset(4)]
        internal readonly ushort R;

        internal Color32 ToColor32() => new Color32((byte)(R >> 8), (byte)(G >> 8), (byte)(B >> 8));
    }
}
