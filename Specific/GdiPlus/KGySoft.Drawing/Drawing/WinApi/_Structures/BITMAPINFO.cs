#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BITMAPINFO.cs
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
    [StructLayout(LayoutKind.Sequential)]
    [SuppressMessage("ReSharper", "IdentifierTypo", Justification = "WinAPI")]
    internal struct BITMAPINFO
    {
        #region Fields

        internal BITMAPINFOHEADER icHeader;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        internal RGBQUAD[]? icColors;

        #endregion
    }
}
