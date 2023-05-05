#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: WindowsOpenGLContext.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2023 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;
using System.Runtime.Versioning;

using KGySoft.Drawing.SkiaSharp.WinApi;

#endregion

namespace KGySoft.Drawing.SkiaSharp
{
    /// <summary>
    /// Represents an OpenGL context under Windows.
    /// This file is the stripped version of Mono's test WglContext file from here: https://github.com/mono/SkiaSharp/blob/b8efe44ae79614d7c0c0739e7144427912fd166a/tests/Tests/GlContexts/Wgl/WglContext.cs
    /// </summary>
#if NET
    [SupportedOSPlatform("windows")]
#endif
    internal sealed class WindowsOpenGLContext : IDisposable
    {
        #region Fields

        #region Static Fields

        private static readonly object fLock = new object();
        private static readonly Win32Window window = new Win32Window("WglContext");

        #endregion

        #region Instance Fields

        private IntPtr pbufferHandle;
        private IntPtr pbufferDeviceContextHandle;
        private IntPtr pbufferGlContextHandle;

        #endregion

        #endregion

        #region Constructors

        internal WindowsOpenGLContext()
        {
            if (!OpenGL32.HasExtension(window.DeviceContextHandle, "WGL_ARB_pixel_format") ||
                !OpenGL32.HasExtension(window.DeviceContextHandle, "WGL_ARB_pbuffer"))
            {
                throw new InvalidOperationException("DC does not have extensions.");
            }

            var iAttrs = new int[]
                {
                    Constants.WGL_ACCELERATION_ARB, Constants.WGL_FULL_ACCELERATION_ARB,
                    Constants.WGL_DRAW_TO_WINDOW_ARB, Constants.TRUE,
                    Constants.WGL_SUPPORT_OPENGL_ARB, Constants.TRUE,
                    Constants.WGL_RED_BITS_ARB, 8,
                    Constants.WGL_GREEN_BITS_ARB, 8,
                    Constants.WGL_BLUE_BITS_ARB, 8,
                    Constants.WGL_ALPHA_BITS_ARB, 8,
                    Constants.WGL_STENCIL_BITS_ARB, 8,
                    Constants.NONE, Constants.NONE
                };
            var piFormats = new int[1];
            uint nFormats = default;
            lock (fLock)
            {
                // HACK: This call seems to cause deadlocks on some systems.
                OpenGL32.wglChoosePixelFormatARB?.Invoke(window.DeviceContextHandle, iAttrs, null, (uint)piFormats.Length, piFormats, out nFormats);
            }

            if (nFormats == 0)
            {
                Destroy();
                throw new InvalidOperationException("Could not get pixel formats.");
            }

            pbufferHandle = OpenGL32.wglCreatePbufferARB?.Invoke(window.DeviceContextHandle, piFormats[0], 1, 1, null) ?? IntPtr.Zero;
            if (pbufferHandle == IntPtr.Zero)
            {
                Destroy();
                throw new InvalidOperationException("Could not create Pbuffer.");
            }

            pbufferDeviceContextHandle = OpenGL32.wglGetPbufferDCARB?.Invoke(pbufferHandle) ?? IntPtr.Zero;
            if (pbufferDeviceContextHandle == IntPtr.Zero)
            {
                Destroy();
                throw new InvalidOperationException("Could not get Pbuffer DC.");
            }

            var prevDC = OpenGL32.wglGetCurrentDC();
            var prevGLRC = OpenGL32.wglGetCurrentContext();

            pbufferGlContextHandle = OpenGL32.wglCreateContext(pbufferDeviceContextHandle);
            OpenGL32.wglMakeCurrent(prevDC, prevGLRC);

            if (pbufferGlContextHandle == IntPtr.Zero)
            {
                Destroy();
                throw new InvalidOperationException("Could not creeate Pbuffer GL context.");
            }
        }

        #endregion

        #region Methods

        #region Internal Methods

        internal void MakeCurrent()
        {
            if (!OpenGL32.wglMakeCurrent(pbufferDeviceContextHandle, pbufferGlContextHandle))
            {
                Destroy();
                throw new InvalidOperationException("Could not set the context.");
            }
        }

        internal void Destroy()
        {
            if (pbufferGlContextHandle != IntPtr.Zero)
            {
                OpenGL32.wglDeleteContext(pbufferGlContextHandle);
                pbufferGlContextHandle = IntPtr.Zero;
            }

            if (pbufferHandle != IntPtr.Zero)
            {
                if (pbufferDeviceContextHandle != IntPtr.Zero)
                {
                    if (!OpenGL32.HasExtension(pbufferDeviceContextHandle, "WGL_ARB_pbuffer"))
                    {
                        // ASSERT
                    }

                    OpenGL32.wglReleasePbufferDCARB?.Invoke(pbufferHandle, pbufferDeviceContextHandle);
                    pbufferDeviceContextHandle = IntPtr.Zero;
                }

                OpenGL32.wglDestroyPbufferARB?.Invoke(pbufferHandle);
                pbufferHandle = IntPtr.Zero;
            }
        }

        #endregion

        #region Explicitly Implemented Interface Methods

        void IDisposable.Dispose() => Destroy();

        #endregion

        #endregion
    }
}
