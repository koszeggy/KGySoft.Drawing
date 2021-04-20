#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ICONINFO.cs
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
    // ReSharper disable once InconsistentNaming
    /// <summary>
    /// Contains information about an icon or a cursor.
    /// </summary>
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
