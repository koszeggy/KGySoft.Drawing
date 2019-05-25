using System.Runtime.InteropServices;

namespace KGySoft.Drawing.WinApi
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct BITMAPINFO
    {
        public BITMAPINFOHEADER icHeader;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        public RGBQUAD[] icColors;
    }
}
