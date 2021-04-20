#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BITMAP.cs
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
