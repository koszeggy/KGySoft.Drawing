#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: SHFILEINFO.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2019 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution. If not, then this file is considered as
//  an illegal copy.
//
//  Unauthorized copying of this file, via any medium is strictly prohibited.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;
using System.Runtime.InteropServices;

#endregion

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
        #region Constants

        private const int MAX_PATH = 260;

        #endregion

        #region Fields

        /// <summary>
        /// Type: HICON
        /// A handle to the icon that represents the file. You are responsible for destroying this handle with DestroyIcon when you no longer need it.
        /// </summary>
        internal IntPtr hIcon;

        /// <summary>
        /// Type: int
        /// The index of the icon image within the system image list.
        /// </summary>
        internal IntPtr iIcon;

        /// <summary>
        /// Type: DWORD
        /// An array of values that indicates the attributes of the file object. For information about these values, see the IShellFolder::GetAttributesOf method.
        /// </summary>
        internal uint dwAttributes;

        /// <summary>
        /// Type: TCHAR[MAX_PATH]
        /// A string that contains the name of the file as it appears in the Windows Shell, or the path and file name of the file that contains the icon representing the file.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)]
        internal string szDisplayName;

        /// <summary>
        /// Type: TCHAR[80]
        /// A string that describes the type of file.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
        internal string szTypeName;

        #endregion
    };
}
