#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: SHFILEINFO.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2024 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;

#endregion

namespace KGySoft.Drawing.WinApi
{
    // ReSharper disable once InconsistentNaming
    /// <summary>
    /// Contains information about a file object.
    /// </summary>
    /// <remarks>
    /// This structure is used with the SHGetFileInfo function.
    /// </remarks>
    internal unsafe struct SHFILEINFO
    {
        #region Constants

        // ReSharper disable once InconsistentNaming
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
        internal fixed char szDisplayName[MAX_PATH];

        /// <summary>
        /// Type: TCHAR[80]
        /// A string that describes the type of file.
        /// </summary>
        internal fixed char szTypeName[80];

        #endregion
    }

}
