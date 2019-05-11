using System;
using System.Runtime.InteropServices;

namespace KGySoft.Drawing.WinApi
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct BITMAP
    {
        internal int bmType;
        internal int bmWidth;
        internal int bmHeight;
        internal int bmWidthBytes;
        internal short bmPlanes;
        internal short bmBitsPixel;
        internal IntPtr bmBits;
    }
}
