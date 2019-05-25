using System;
using System.Runtime.InteropServices;

namespace KGySoft.Drawing.WinApi
{
    /// <summary>
    /// Contains information about a file object.
    /// </summary>
    /// <remarks>
    /// This structure is used with the SHGetFileInfo function.
    /// </remarks>
    internal struct SHFILEINFO
    {
        private const int MAX_PATH = 260;

        /// <summary>
        /// Type: HICON
        /// A handle to the icon that represents the file. You are responsible for destroying this handle with DestroyIcon when you no longer need it.
        /// </summary>
        public IntPtr hIcon;

        /// <summary>
        /// Type: int
        /// The index of the icon image within the system image list.
        /// </summary>
        public IntPtr iIcon;

        /// <summary>
        /// Type: DWORD
        /// An array of values that indicates the attributes of the file object. For information about these values, see the IShellFolder::GetAttributesOf method.
        /// </summary>
        public uint dwAttributes;

        /// <summary>
        /// Type: TCHAR[MAX_PATH]
        /// A string that contains the name of the file as it appears in the Windows Shell, or the path and file name of the file that contains the icon representing the file.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)]
        public string szDisplayName;

        /// <summary>
        /// Type: TCHAR[80]
        /// A string that describes the type of file.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
        public string szTypeName;
    };
}
