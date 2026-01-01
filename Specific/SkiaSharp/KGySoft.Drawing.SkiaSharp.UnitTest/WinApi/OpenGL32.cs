#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: OpenGL32.cs
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

using System;
using System.Runtime.InteropServices;
#if NET
using System.Runtime.Versioning;
#endif

#endregion

#region Suppressions

#if !NETCOREAPP3_0_OR_GREATER
#pragma warning disable CS8602 // Dereference of a possibly null reference
#endif

#endregion

namespace KGySoft.Drawing.SkiaSharp.WinApi
{
#if NET
    [SupportedOSPlatform("windows")]
#endif
    internal static class OpenGL32
    {
        #region Constants

        private const string opengl32 = "opengl32.dll";

        #endregion

        #region Fields

        internal static readonly wglGetExtensionsStringARBDelegate? wglGetExtensionsStringARB;
        internal static readonly wglChoosePixelFormatARBDelegate? wglChoosePixelFormatARB;
        internal static readonly wglCreatePbufferARBDelegate? wglCreatePbufferARB;
        internal static readonly wglDestroyPbufferARBDelegate? wglDestroyPbufferARB;
        internal static readonly wglGetPbufferDCARBDelegate? wglGetPbufferDCARB;
        internal static readonly wglReleasePbufferDCARBDelegate? wglReleasePbufferDCARB;
        internal static readonly wglSwapIntervalEXTDelegate? wglSwapIntervalEXT;

        #endregion

        #region Properties

        internal static string VersionString { get; }

        #endregion

        #region Constructors

        static OpenGL32()
        {
            // save the current GL context
            var prevDC = wglGetCurrentDC();
            var prevGLRC = wglGetCurrentContext();

            // register the dummy window class
            var wc = new WNDCLASS
            {
                style = (Constants.CS_HREDRAW | Constants.CS_VREDRAW | Constants.CS_OWNDC),
                lpfnWndProc = User32.DefWindowProc,
                cbClsExtra = 0,
                cbWndExtra = 0,
                hInstance = Kernel32.CurrentModuleHandle,
                hCursor = User32.LoadCursor(IntPtr.Zero, (int)Constants.IDC_ARROW),
                hIcon = User32.LoadIcon(IntPtr.Zero, (IntPtr)Constants.IDI_WINLOGO),
                hbrBackground = IntPtr.Zero,
                lpszMenuName = null,
                lpszClassName = "DummyClass"
            };
            if (User32.RegisterClass(ref wc) == 0)
            {
                throw new InvalidOperationException("Could not register dummy class.");
            }

            // get the the dummy window bounds
            var windowRect = new RECT { left = 0, right = 8, top = 0, bottom = 8 };
            User32.AdjustWindowRectEx(ref windowRect, Constants.WS_SYSMENU, false, Constants.WS_EX_CLIENTEDGE);

            // create the dummy window
            var dummyWND = User32.CreateWindowEx(
                    Constants.WS_EX_CLIENTEDGE,
                    "DummyClass",
                    "DummyWindow", Constants.WS_CLIPSIBLINGS | Constants.WS_CLIPCHILDREN | Constants.WS_SYSMENU,
                    0, 0,
                    windowRect.right - windowRect.left, windowRect.bottom - windowRect.top,
                    IntPtr.Zero, IntPtr.Zero, Kernel32.CurrentModuleHandle, IntPtr.Zero);
            if (dummyWND == IntPtr.Zero)
            {
                User32.UnregisterClass("DummyClass", Kernel32.CurrentModuleHandle);
                throw new InvalidOperationException("Could not create dummy window.");
            }

            // get the dummy DC
            var dummyDC = User32.GetDC(dummyWND);

            // get the dummy pixel format
            var dummyPFD = new PIXELFORMATDESCRIPTOR();
            dummyPFD.nSize = (ushort)Marshal.SizeOf(dummyPFD);
            dummyPFD.nVersion = 1;
            dummyPFD.dwFlags = Constants.PFD_DRAW_TO_WINDOW | Constants.PFD_SUPPORT_OPENGL;
            dummyPFD.iPixelType = Constants.PFD_TYPE_RGBA;
            dummyPFD.cColorBits = 32;
            dummyPFD.cDepthBits = 24;
            dummyPFD.cStencilBits = 8;
            dummyPFD.iLayerType = Constants.PFD_MAIN_PLANE;
            var dummyFormat = Gdi32.ChoosePixelFormat(dummyDC, ref dummyPFD);
            Gdi32.SetPixelFormat(dummyDC, dummyFormat, ref dummyPFD);

            // get the dummy GL context
            var dummyGLRC = wglCreateContext(dummyDC);
            if (dummyGLRC == IntPtr.Zero)
            {
                throw new InvalidOperationException("Could not create dummy GL context.");
            }
            wglMakeCurrent(dummyDC, dummyGLRC);

            VersionString = GetString(Constants.GL_VERSION);

            // get the extension methods using the dummy context
            wglGetExtensionsStringARB = wglGetProcAddress<wglGetExtensionsStringARBDelegate>("wglGetExtensionsStringARB");
            wglChoosePixelFormatARB = wglGetProcAddress<wglChoosePixelFormatARBDelegate>("wglChoosePixelFormatARB");
            wglCreatePbufferARB = wglGetProcAddress<wglCreatePbufferARBDelegate>("wglCreatePbufferARB");
            wglDestroyPbufferARB = wglGetProcAddress<wglDestroyPbufferARBDelegate>("wglDestroyPbufferARB");
            wglGetPbufferDCARB = wglGetProcAddress<wglGetPbufferDCARBDelegate>("wglGetPbufferDCARB");
            wglReleasePbufferDCARB = wglGetProcAddress<wglReleasePbufferDCARBDelegate>("wglReleasePbufferDCARB");
            wglSwapIntervalEXT = wglGetProcAddress<wglSwapIntervalEXTDelegate>("wglSwapIntervalEXT");

            // destroy the dummy GL context
            wglMakeCurrent(dummyDC, IntPtr.Zero);
            wglDeleteContext(dummyGLRC);

            // destroy the dummy window
            User32.DestroyWindow(dummyWND);
            User32.UnregisterClass("DummyClass", Kernel32.CurrentModuleHandle);

            // reset the initial GL context
            wglMakeCurrent(prevDC, prevGLRC);
        }

        #endregion

        #region Methods

        internal static bool HasExtension(IntPtr dc, string ext)
        {
            if (wglGetExtensionsStringARB == null)
            {
                return false;
            }

            if (ext == "WGL_ARB_extensions_string")
            {
                return true;
            }

            return Array.IndexOf(GetExtensionsARB(dc), ext) != -1;
        }

        internal static string? GetExtensionsStringARB(IntPtr dc) => Marshal.PtrToStringAnsi(wglGetExtensionsStringARB?.Invoke(dc) ?? IntPtr.Zero);

        internal static string[] GetExtensionsARB(IntPtr dc)
        {
            var str = GetExtensionsStringARB(dc);
            if (string.IsNullOrEmpty(str))
            {
                return Array.Empty<string>();
            }
            return str.Split(' ');
        }

        [DllImport(opengl32, CallingConvention = CallingConvention.Winapi)]
        internal static extern IntPtr wglGetCurrentDC();

        [DllImport(opengl32, CallingConvention = CallingConvention.Winapi)]
        internal static extern IntPtr wglGetCurrentContext();

        [DllImport(opengl32, CallingConvention = CallingConvention.Winapi)]
        internal static extern IntPtr wglCreateContext(IntPtr hDC);

        [DllImport(opengl32, CallingConvention = CallingConvention.Winapi)]
        internal static extern bool wglMakeCurrent(IntPtr hDC, IntPtr hRC);

        [DllImport(opengl32, CallingConvention = CallingConvention.Winapi)]
        internal static extern bool wglDeleteContext(IntPtr hRC);

        [DllImport(opengl32, CallingConvention = CallingConvention.Winapi)]
        internal static extern IntPtr wglGetProcAddress([MarshalAs(UnmanagedType.LPStr)] string lpszProc);

        internal static T? wglGetProcAddress<T>(string lpszProc)
            where T : Delegate
        {
            var ptr = wglGetProcAddress(lpszProc);
            if (ptr == IntPtr.Zero)
                return default;

            return (T)Marshal.GetDelegateForFunctionPointer(ptr, typeof(T));
        }

        [DllImport(opengl32, CallingConvention = CallingConvention.Winapi)]
        internal static extern IntPtr glGetString(uint value);

        internal static string GetString(uint value)
        {
            var intPtr = glGetString(value);
            return Marshal.PtrToStringAnsi(intPtr)!;
        }

        #endregion
    }
}
