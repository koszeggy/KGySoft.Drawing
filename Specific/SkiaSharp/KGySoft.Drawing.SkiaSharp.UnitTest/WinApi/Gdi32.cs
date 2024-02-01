#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: Gdi32.cs
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

using System;
using System.Runtime.InteropServices;
#if NET
using System.Runtime.Versioning;
#endif

#endregion

namespace KGySoft.Drawing.SkiaSharp.WinApi
{
#if NET
    [SupportedOSPlatform("windows")]
#endif
    internal static class Gdi32
    {
        #region Constants

        private const string gdi32 = "gdi32.dll";

        #endregion

        #region Methods

        [DllImport(gdi32, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetPixelFormat(IntPtr hdc, int iPixelFormat, [In]ref PIXELFORMATDESCRIPTOR ppfd);

        [DllImport(gdi32, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
        internal static extern int ChoosePixelFormat(IntPtr hdc, [In]ref PIXELFORMATDESCRIPTOR ppfd);

        #endregion
    }
}