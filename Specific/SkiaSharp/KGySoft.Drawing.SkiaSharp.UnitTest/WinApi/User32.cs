#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: User32.cs
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

#if NET
using System.Runtime.Versioning;
#endif

#region Used Namespaces

using System;
using System.Runtime.InteropServices;

#endregion

#endregion

namespace KGySoft.Drawing.SkiaSharp.WinApi
{
#if NET
    [SupportedOSPlatform("windows")]
#endif
    internal static class User32
    {
        #region Constants

        private const string user32 = "user32.dll";

        #endregion

        #region Methods

        [DllImport(user32, CallingConvention = CallingConvention.Winapi, SetLastError = true, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern ushort RegisterClass(ref WNDCLASS lpWndClass);

        [DllImport(user32, CallingConvention = CallingConvention.Winapi, SetLastError = true, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern ushort UnregisterClass([MarshalAs(UnmanagedType.LPTStr)] string lpClassName, IntPtr hInstance);

        [DllImport(user32, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
        internal static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);

        [DllImport(user32, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
        internal static extern IntPtr LoadIcon(IntPtr hInstance, IntPtr lpIconName);

        [DllImport(user32, CallingConvention = CallingConvention.Winapi)]
        internal static extern IntPtr DefWindowProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);

        [DllImport(user32, CallingConvention = CallingConvention.Winapi, SetLastError = true, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern IntPtr CreateWindowEx(uint dwExStyle, [MarshalAs(UnmanagedType.LPTStr)] string lpClassName, [MarshalAs(UnmanagedType.LPTStr)] string lpWindowName, uint dwStyle, int x, int y, int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

        internal static IntPtr CreateWindow(string lpClassName, string lpWindowName, uint dwStyle, int x, int y, int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam)
        {
            return CreateWindowEx(0, lpClassName, lpWindowName, dwStyle, x, y, nWidth, nHeight, hWndParent, hMenu, hInstance, lpParam);
        }

        [DllImport(user32, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
        internal static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport(user32, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport(user32, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DestroyWindow(IntPtr hWnd);

        [DllImport(user32, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool AdjustWindowRectEx(ref RECT lpRect, uint dwStyle, bool bMenu, uint dwExStyle);

        #endregion
    }
}
