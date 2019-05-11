using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace KGySoft.Drawing.WinApi
{
    /// <summary>
    /// Receives information used to retrieve a stock Shell icon. This structure is used in a call SHGetStockIconInfo.
    /// </summary>
    [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct SHSTOCKICONINFO
    {
        /// <summary>
        /// The size of this structure, in bytes.
        /// </summary>
        internal uint cbSize;

        /// <summary>
        /// When SHGetStockIconInfo is called with the SHGSI_ICON flag, this member receives a handle to the icon.
        /// </summary>
        internal IntPtr hIcon;

        /// <summary>
        /// When SHGetStockIconInfo is called with the SHGSI_SYSICONINDEX flag, this member receives the index of the image in the system icon cache.
        /// </summary>
        internal int iSysIconIndex;

        /// <summary>
        /// When SHGetStockIconInfo is called with the SHGSI_ICONLOCATION flag, this member receives the index of the icon in the resource whose path is received in szPath.
        /// </summary>
        internal int iIcon;

        /// <summary>
        /// When SHGetStockIconInfo is called with the SHGSI_ICONLOCATION flag, this member receives the path of the resource that contains the icon. The index of the icon within the resource is received in iIcon.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = Constants.MAX_PATH)]
        internal string szPath;
    }
}
