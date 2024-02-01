#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ICONDIR.cs
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

using System.Runtime.InteropServices;

#endregion

namespace KGySoft.Drawing.WinApi
{
    // ReSharper disable once InconsistentNaming
    [StructLayout(LayoutKind.Sequential)]
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
