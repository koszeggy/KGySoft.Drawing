#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BITMAP.cs
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
using System.Runtime.InteropServices;

#endregion

namespace KGySoft.Drawing.WinApi
{
    // ReSharper disable once InconsistentNaming
    [StructLayout(LayoutKind.Sequential)]
    internal struct BITMAP
    {
        #region Fields

        internal int bmType;
        internal int bmWidth;
        internal int bmHeight;
        internal int bmWidthBytes;
        internal short bmPlanes;
        internal short bmBitsPixel;
        internal IntPtr bmBits;

        #endregion
    }
}
