#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ICONDIR.cs
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

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

#endregion

namespace KGySoft.Drawing.WinApi
{
    [StructLayout(LayoutKind.Sequential)]
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "WinAPI")]
    [SuppressMessage("ReSharper", "IdentifierTypo", Justification = "WinAPI")]
    internal struct ICONDIR
    {
        #region Fields

        /// <summary>
        /// Reserved, must be zero.
        /// </summary>
        internal ushort idReserved;

        /// <summary>
        /// 1 for icons, 2 for cursors.
        /// </summary>
        internal ushort idType;

        /// <summary>
        /// The number of images in the icon.
        /// </summary>
        internal ushort idCount;

        #endregion
    }
}
