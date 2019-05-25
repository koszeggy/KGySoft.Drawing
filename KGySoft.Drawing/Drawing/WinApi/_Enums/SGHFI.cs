using System;

namespace KGySoft.Drawing.WinApi
{
    [Flags]
    internal enum SGHFI : uint
    {
        /// <summary>
        /// Indicates that the function should not attempt to access the file specified by pszPath.
        /// Rather, it should act as if the file specified by pszPath exists with the file attributes passed in dwFileAttributes.
        /// This flag cannot be combined with the SHGFI_ATTRIBUTES, SHGFI_EXETYPE, or SHGFI_PIDL flags.
        /// </summary>
        SHGFI_USEFILEATTRIBUTES = 0x10,

        /// <summary>
        /// Retrieve the handle to the icon that represents the file and the index of the icon within the system image list.
        /// The handle is copied to the hIcon member of the structure specified by psfi, and the index is copied to the iIcon member.
        /// </summary>
        SHGFI_ICON = 0x100
    }
}
