#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ICONINFO.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2025 - All Rights Reserved
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
    /// <summary>
    /// Contains information about an icon or a cursor.
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "WinAPI")]
    internal struct ICONINFO
    {
        #region Fields

        /// <summary>
        /// Specifies whether this structure defines an icon or a cursor. A value of TRUE specifies an icon; FALSE specifies a cursor.
        /// </summary>
        internal bool fIcon;

        /// <summary>
        /// The x-coordinate of a cursor's hot spot. If this structure defines an icon, the hot spot is always in the center of the icon, and this member is ignored.
        /// </summary>
        internal int xHotspot;

        /// <summary>
        /// The y-coordinate of the cursor's hot spot. If this structure defines an icon, the hot spot is always in the center of the icon, and this member is ignored.
        /// </summary>
        internal int yHotspot;

        /// <summary>
        /// The icon bitmask bitmap. If this structure defines a black and white icon, this bitmask is formatted so that the upper half is the icon AND bitmask and the lower half is the icon XOR bitmask. Under this condition, the height should be an even multiple of two. If this structure defines a color icon, this mask only defines the AND bitmask of the icon.
        /// </summary>
        internal IntPtr hbmMask;

        /// <summary>
        /// A handle to the icon color bitmap. This member can be optional if this structure defines a black and white icon. The AND bitmask of hbmMask is applied with the SRCAND flag to the destination; subsequently, the color bitmap is applied (using XOR) to the destination by using the SRCINVERT flag.
        /// </summary>
        internal IntPtr hbmColor;

        #endregion
    }
}
