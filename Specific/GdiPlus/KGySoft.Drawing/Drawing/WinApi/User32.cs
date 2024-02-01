#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: User32.cs
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
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.InteropServices;
#if NET
using System.Runtime.Versioning;
#endif
using System.Security;

#endregion

#region Suppressions

// ReSharper disable InconsistentNaming

#endregion

namespace KGySoft.Drawing.WinApi
{
    [SecurityCritical]
#if NET
    [SupportedOSPlatform("windows")]
#endif
    internal static class User32
    {
        #region NativeMethods class

        [SuppressMessage("ReSharper", "MemberHidesStaticFromOuterClass")]
        private static class NativeMethods
        {
            #region Methods

            /// <summary>
            /// Destroys an icon and frees any memory the icon occupied.
            /// </summary>
            /// <param name="handle">[in] (Type: HICON)
            /// A handle to the icon to be destroyed. The icon must not be in use.
            /// </param>
            /// <returns>If the function succeeds, the return value is nonzero.
            /// If the function fails, the return value is zero. To get extended error information, call GetLastError.</returns>
            [DllImport("user32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool DestroyIcon(IntPtr handle);

            /// <summary>
            /// The GetDC function retrieves a handle to a device context(DC) for the client area of a specified window or for the entire screen.You can use the returned handle in subsequent GDI functions to draw in the DC.The device context is an opaque data structure, whose values are used internally by GDI.
            /// </summary>
            /// <param name = "hWnd" > A handle to the window whose DC is to be retrieved.If this value is NULL, GetDC retrieves the DC for the entire screen.</param>
            /// <returns>If the function succeeds, the return value is a handle to the DC for the specified window's client area.
            /// If the function fails, the return value is NULL.</returns>
            [DllImport("user32.dll", CharSet = CharSet.Auto)]
            internal static extern IntPtr GetDC(IntPtr hWnd);

            /// <summary>
            /// The ReleaseDC function releases a device context (DC), freeing it for use by other applications. The effect of the ReleaseDC function depends on the type of DC. It frees only common and window DCs. It has no effect on class or private DCs.
            /// </summary>
            /// <param name="hWnd">A handle to the window whose DC is to be released.</param>
            /// <param name="hDC">A handle to the DC to be released.</param>
            /// <returns>The return value indicates whether the DC was released. If the DC was released, the return value is 1.
            /// If the DC was not released, the return value is zero.</returns>
            /// <remarks>
            /// The application must call the ReleaseDC function for each call to the GetWindowDC function and for each call to the GetDC function that retrieves a common DC.
            /// An application cannot use the ReleaseDC function to release a DC that was created by calling the CreateDC function; instead, it must use the DeleteDC function. ReleaseDC must be called from the same thread that called GetDC.</remarks>
            [DllImport("user32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDC);

            /// <summary>
            /// Retrieves information about the specified icon or cursor.
            /// </summary>
            /// <param name="hIcon">A handle to the icon or cursor. To retrieve information about a standard icon or cursor, specify one of the standard values.</param>
            /// <param name="piconinfo">A pointer to an ICONINFO structure. The function fills in the structure's members.</param>
            /// <returns>If the function succeeds, the return value is nonzero and the function fills in the members of the specified ICONINFO structure. If the function fails, the return value is zero. To get extended error information, call GetLastError.</returns>
            [DllImport("user32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool GetIconInfo(IntPtr hIcon, out ICONINFO piconinfo);

            /// <summary>
            /// The WindowFromDC function returns a handle to the window associated with the specified display device context (DC). Output functions that use the specified device context draw into this window.
            /// </summary>
            /// <param name="hDC">Handle to the device context from which a handle to the associated window is to be retrieved.</param>
            /// <returns>The return value is a handle to the window associated with the specified DC. If no window is associated with the specified DC, the return value is NULL.</returns>
            [DllImport("user32.dll")]
            internal static extern IntPtr WindowFromDC(IntPtr hDC);

            /// <summary>
            /// Retrieves the dimensions of the bounding rectangle of the specified window. The dimensions are given in screen coordinates that are relative to the upper-left corner of the screen.
            /// </summary>
            /// <param name="hWnd">A handle to the window. </param>
            /// <param name="lpRect">A pointer to a RECT structure that receives the screen coordinates of the upper-left and lower-right corners of the window. </param>
            /// <returns>If the function succeeds, the return value is nonzero. If the function fails, the return value is zero. To get extended error information, call GetLastError. </returns>
            [DllImport("user32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

            /// <summary>
            /// Retrieves the coordinates of a window's client area. The client coordinates specify the upper-left and lower-right corners of the client area. Because client coordinates are relative to the upper-left corner of a window's client area, the coordinates of the upper-left corner are (0,0).
            /// </summary>
            /// <param name="hWnd">A handle to the window whose client coordinates are to be retrieved.</param>
            /// <param name="lpRect">A pointer to a RECT structure that receives the client coordinates. The left and top members are zero. The right and bottom members contain the width and height of the window.</param>
            /// <returns>If the function succeeds, the return value is nonzero. If the function fails, the return value is zero. To get extended error information, call GetLastError.</returns>
            [DllImport("user32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

            /// <summary>
            /// Creates an icon or cursor from an <see cref="ICONINFO"/> structure.
            /// </summary>
            /// <param name="piconinfo">A pointer to an <see cref="ICONINFO"/> structure the function uses to create the icon or cursor.</param>
            /// <returns>If the function succeeds, the return value is a handle to the icon or cursor that is created. If the function fails, the return value is NULL. To get extended error information, call GetLastError.</returns>
            /// <remarks>The system copies the bitmaps in the ICONINFO structure before creating the icon or cursor. Because the system may temporarily select the bitmaps in a device context, the hbmMask and hbmColor members of the ICONINFO structure should not already be selected into a device context. The application must continue to manage the original bitmaps and delete them when they are no longer necessary.
            /// When you are finished using the icon, destroy it using the <see cref="DestroyIcon"/> function.</remarks>
            [DllImport("user32.dll", SetLastError = true)]
            internal static extern IntPtr CreateIconIndirect([In] ref ICONINFO piconinfo);

            /// <summary>
            /// Destroys a cursor and frees any memory the cursor occupied. Do not use this function to destroy a shared cursor.
            /// </summary>
            /// <param name="hCursor">A handle to the cursor to be destroyed. The cursor must not be in use.</param>
            /// <returns>If the function succeeds, the return value is nonzero. If the function fails, the return value is zero. To get extended error information, call GetLastError.</returns>
            [DllImport("user32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool DestroyCursor(IntPtr hCursor);

            #endregion
        }

        #endregion

        #region Methods

        internal static void DestroyIcon(IntPtr handle)
        {
            if (!NativeMethods.DestroyIcon(handle))
                throw new ArgumentException(Res.User32InvalidHandle, nameof(handle), new Win32Exception());
        }

        internal static IntPtr GetDC(IntPtr handle)
        {
            IntPtr dc = NativeMethods.GetDC(handle);
            if (dc == IntPtr.Zero)
                throw new ArgumentException(Res.User32InvalidHandle, nameof(handle));
            return dc;
        }

        internal static void ReleaseDC(IntPtr handle, IntPtr dc)
        {
            if (!NativeMethods.ReleaseDC(handle, dc))
                throw new ArgumentException(Res.User32InvalidHandle, nameof(handle));
        }

        internal static void GetIconInfo(IntPtr handle, out ICONINFO iconInfo)
        {
            if (!NativeMethods.GetIconInfo(handle, out iconInfo))
                throw new ArgumentException(Res.User32InvalidHandle, nameof(handle), new Win32Exception());
        }

        internal static IntPtr WindowFromDC(IntPtr dc) => NativeMethods.WindowFromDC(dc);

        internal static Rectangle GetWindowRect(IntPtr handle)
        {
            if (!NativeMethods.GetWindowRect(handle, out RECT rect))
                throw new ArgumentException(Res.User32InvalidHandle, nameof(handle), new Win32Exception());
            return rect.ToRectangle();
        }

        internal static Rectangle GetClientRect(IntPtr handle)
        {
            if (!NativeMethods.GetClientRect(handle, out RECT rect))
                throw new ArgumentException(Res.User32InvalidHandle, nameof(handle), new Win32Exception());
            return rect.ToRectangle();
        }

        internal static IntPtr CreateIconIndirect(ref ICONINFO iconinfo)
        {
            IntPtr result = NativeMethods.CreateIconIndirect(ref iconinfo);
            if (result == IntPtr.Zero)
                throw new ArgumentException(Res.User32CreateIconIndirectFailed, nameof(iconinfo), new Win32Exception());
            return result;
        }

        internal static void DestroyCursor(IntPtr handle)
        {
            if (!NativeMethods.DestroyCursor(handle))
                throw new ArgumentException(Res.User32InvalidHandle, nameof(handle), new Win32Exception());
        }

        #endregion
    }
}
