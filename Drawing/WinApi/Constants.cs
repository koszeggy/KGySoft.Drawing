using System;
using System.Runtime.InteropServices;

namespace KGySoft.Drawing.WinApi
{
    static class Constants
    {
        internal const int BI_RGB = 0;
        internal const int DIB_RGB_COLORS = 0;

        /// <summary>
        /// Retrieve the handle to the icon that represents the file and the index of the icon within the system image list.
        /// The handle is copied to the hIcon member of the structure specified by psfi, and the index is copied to the iIcon member.
        /// </summary>
        internal const uint SHGFI_ICON = 0x100;

        /// <summary>
        /// Indicates that the function should not attempt to access the file specified by pszPath.
        /// Rather, it should act as if the file specified by pszPath exists with the file attributes passed in dwFileAttributes.
        /// This flag cannot be combined with the SHGFI_ATTRIBUTES, SHGFI_EXETYPE, or SHGFI_PIDL flags.
        /// </summary>
        internal const uint SHGFI_USEFILEATTRIBUTES = 0x10;

        internal const int MAX_PATH = 260;
    }
}
