#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: SHGSI.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2026 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;
using System.Diagnostics.CodeAnalysis;

#endregion

namespace KGySoft.Drawing.WinApi
{
    [Flags]
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "WinAPI")]
    [SuppressMessage("ReSharper", "IdentifierTypo", Justification = "WinAPI")]
    internal enum SHGSI : uint
    {
        /// <summary>
        /// The szPath and iIcon members of the SHSTOCKICONINFO structure receive the path and icon index of the requested icon, in a format suitable for passing to the ExtractIcon function. The numerical value of this flag is zero, so you always get the icon location regardless of other flags.
        /// </summary>
        ICONLOCATION = 0,

        /// <summary>
        /// The hIcon member of the SHSTOCKICONINFO structure receives a handle to the specified icon.
        /// </summary>
        ICON = 0x000000100,

        /// <summary>
        /// The iSysImageImage member of the SHSTOCKICONINFO structure receives the index of the specified icon in the system imagelist.
        /// </summary>
        SYSICONINDEX = 0x000004000,

        /// <summary>
        /// Modifies the SHGSI_ICON value by causing the function to add the link overlay to the file's icon.
        /// </summary>
        LINKOVERLAY = 0x000008000,

        /// <summary>
        /// Modifies the SHGSI_ICON value by causing the function to blend the icon with the system highlight color.
        /// </summary>
        SELECTED = 0x000010000,

        /// <summary>
        /// Modifies the SHGSI_ICON value by causing the function to retrieve the large version of the icon, as specified by the SM_CXICON and SM_CYICON system metrics.
        /// </summary>
        [SuppressMessage("Design", "CA1069:Enums values should not be duplicated", Justification = "Windows API")]
        LARGEICON = 0x000000000,

        /// <summary>
        /// Modifies the SHGSI_ICON value by causing the function to retrieve the small version of the icon, as specified by the SM_CXSMICON and SM_CYSMICON system metrics.
        /// </summary>
        SMALLICON = 0x000000001,

        /// <summary>
        /// Modifies the SHGSI_LARGEICON or SHGSI_SMALLICON values by causing the function to retrieve the Shell-sized icons rather than the sizes specified by the system metrics.
        /// </summary>
        SHELLICONSIZE = 0x000000004
    }
}
