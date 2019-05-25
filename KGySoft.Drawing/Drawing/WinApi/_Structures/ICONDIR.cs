using System.IO;
using System.Runtime.InteropServices;

namespace KGySoft.Drawing.WinApi
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct ICONDIR
    {
        /// <summary>
        /// Reserved, must be zero.
        /// </summary>
        internal ushort idReserved;

        /// <summary>
        /// 1 for icons, 2 for cursors.
        /// </summary>
        internal ushort idType;

        /// <summary>
        /// The number of images in the icon.
        /// </summary>
        internal ushort idCount;
    }
}
