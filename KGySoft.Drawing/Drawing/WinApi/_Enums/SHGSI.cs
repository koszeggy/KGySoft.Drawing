#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: SHGSI.cs
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

#endregion

namespace KGySoft.Drawing.WinApi
{
    [Flags]
    internal enum SHGSI : uint
    {
        /// <summary>
        /// The szPath and iIcon members of the SHSTOCKICONINFO structure receive the path and icon index of the requested icon, in a format suitable for passing to the ExtractIcon function. The numerical value of this flag is zero, so you always get the icon location regardless of other flags.
        /// </summary>
        SHGSI_ICONLOCATION = 0,

        /// <summary>
        /// The hIcon member of the SHSTOCKICONINFO structure receives a handle to the specified icon.
        /// </summary>
        SHGSI_ICON = 0x000000100,

        /// <summary>
        /// The iSysImageImage member of the SHSTOCKICONINFO structure receives the index of the specified icon in the system imagelist.
        /// </summary>
        SHGSI_SYSICONINDEX = 0x000004000,

        /// <summary>
        /// Modifies the SHGSI_ICON value by causing the function to add the link overlay to the file's icon.
        /// </summary>
        SHGSI_LINKOVERLAY = 0x000008000,

        /// <summary>
        /// Modifies the SHGSI_ICON value by causing the function to blend the icon with the system highlight color.
        /// </summary>
        SHGSI_SELECTED = 0x000010000,

        /// <summary>
        /// Modifies the SHGSI_ICON value by causing the function to retrieve the large version of the icon, as specified by the SM_CXICON and SM_CYICON system metrics.
        /// </summary>
        SHGSI_LARGEICON = 0x000000000,

        /// <summary>
        /// Modifies the SHGSI_ICON value by causing the function to retrieve the small version of the icon, as specified by the SM_CXSMICON and SM_CYSMICON system metrics.
        /// </summary>
        SHGSI_SMALLICON = 0x000000001,

        /// <summary>
        /// Modifies the SHGSI_LARGEICON or SHGSI_SMALLICON values by causing the function to retrieve the Shell-sized icons rather than the sizes specified by the system metrics.
        /// </summary>
        SHGSI_SHELLICONSIZE = 0x000000004
    }
}
