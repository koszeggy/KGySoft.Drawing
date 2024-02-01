#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: WNDPROC.cs
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

#endregion

#region Suppressions

// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo

#endregion

namespace KGySoft.Drawing.SkiaSharp.WinApi
{
    #region Delegates

    internal delegate IntPtr WNDPROC(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate IntPtr wglGetExtensionsStringARBDelegate(IntPtr dc);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal delegate bool wglChoosePixelFormatARBDelegate(
        IntPtr dc,
        [In]int[] attribIList,
        [In]float[]? attribFList,
        uint maxFormats,
        [Out]int[] pixelFormats,
        out uint numFormats);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate IntPtr wglCreatePbufferARBDelegate(IntPtr dc, int pixelFormat, int width, int height, [In]int[]? attribList);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal delegate bool wglDestroyPbufferARBDelegate(IntPtr pbuffer);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate IntPtr wglGetPbufferDCARBDelegate(IntPtr pbuffer);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate int wglReleasePbufferDCARBDelegate(IntPtr pbuffer, IntPtr dc);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal delegate bool wglSwapIntervalEXTDelegate(int interval);

    #endregion
}
