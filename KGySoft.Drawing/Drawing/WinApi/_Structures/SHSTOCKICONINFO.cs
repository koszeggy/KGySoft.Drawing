﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: SHSTOCKICONINFO.cs
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
    /// Receives information used to retrieve a stock Shell icon. This structure is used in a call SHGetStockIconInfo.
    /// </summary>
    [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct SHSTOCKICONINFO
    {
        #region Constants

        private const int MAX_PATH = 260;

        #endregion

        #region Fields

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
        /// Type: TCHAR[MAX_PATH]
        /// When SHGetStockIconInfo is called with the SHGSI_ICONLOCATION flag, this member receives the path of the resource that contains the icon. The index of the icon within the resource is received in iIcon.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)]
        internal string szPath;

        #endregion
    }
}