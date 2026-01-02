#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: GRPICONDIR.cs
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
    // ReSharper disable once InconsistentNaming
    /// <summary>
    /// No official documentation available, but see here: https://devblogs.microsoft.com/oldnewthing/20120720-00/?p=7083
    /// and here: https://chromium.googlesource.com/chromium/src//+/f3080498facd16066076e7d459ba7cfda11e582c/ui/gfx/icon_util.h?autodive=0%2F%2F%2F%2F%2F%2F#188
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [SuppressMessage("ReSharper", "IdentifierTypo", Justification = "WinAPI")]
    internal struct GRPICONDIR
    {
        #region Fields

        internal ushort idReserved;
        internal ushort idType;
        internal ushort idCount;
        // GRPICONDIRENTRY idEntries[1];

        #endregion
    }
}
