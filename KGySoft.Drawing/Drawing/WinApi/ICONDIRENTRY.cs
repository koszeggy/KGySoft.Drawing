using System.IO;
using System.Runtime.InteropServices;

namespace KGySoft.Drawing.WinApi
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct ICONDIRENTRY
    {
        /// <summary>
        /// The icon width in pixels. 0 for 256 width.
        /// </summary>
        internal byte bWidth;

        /// <summary>
        /// The icon height in pixels. 0 for 256 height.
        /// </summary>
        internal byte bHeight;

        /// <summary>
        /// Number of colors in the image. 0 for 256 or more colors.
        /// </summary>
        internal byte bColorCount;

        /// <summary>
        /// Reserved byte. Always 0.
        /// </summary>
        internal byte bReserved;

        /// <summary>
        /// Color planes. Always 1.
        /// </summary>
        internal ushort wPlanes;

        /// <summary>
        /// Bits per pixel.
        /// </summary>
        internal ushort wBitCount;

        /// <summary>
        /// Length of the image in bytes including header and palette size.
        /// </summary>
        internal uint dwBytesInRes;

        /// <summary>
        /// Start offset of the image insize of the icon in bytes.
        /// </summary>
        internal uint dwImageOffset;
    }
}
