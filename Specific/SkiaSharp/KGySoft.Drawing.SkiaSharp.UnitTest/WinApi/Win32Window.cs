#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: Win32Window.cs
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

using System.Diagnostics.CodeAnalysis;
#if NET
using System.Runtime.Versioning;
#endif

#region Used Namespaces

using System;

#endregion

#endregion

namespace KGySoft.Drawing.SkiaSharp.WinApi
{
#if NET
    [SupportedOSPlatform("windows")]
#endif
    [SuppressMessage("ReSharper", "LocalizableElement", Justification = "This is just a test project")]
    internal class Win32Window : IDisposable
    {
        #region Fields

        private ushort classRegistration;

        #endregion

        #region Properties

        internal string WindowClassName { get; }
        internal IntPtr WindowHandle { get; private set; }
        internal IntPtr DeviceContextHandle { get; private set; }

        #endregion

        #region Constructors

        internal Win32Window(string className)
        {
            WindowClassName = className;

            var wc = new WNDCLASS
            {
                cbClsExtra = 0,
                cbWndExtra = 0,
                hbrBackground = IntPtr.Zero,
                hCursor = User32.LoadCursor(IntPtr.Zero, (int)Constants.IDC_ARROW),
                hIcon = User32.LoadIcon(IntPtr.Zero, (IntPtr)Constants.IDI_APPLICATION),
                hInstance = Kernel32.CurrentModuleHandle,
                lpfnWndProc = User32.DefWindowProc,
                lpszClassName = WindowClassName,
                lpszMenuName = null,
                style = Constants.CS_HREDRAW | Constants.CS_VREDRAW | Constants.CS_OWNDC
            };

            classRegistration = User32.RegisterClass(ref wc);
            if (classRegistration == 0)
                throw new ArgumentException($"Could not register window class: {className}", nameof(className));

            WindowHandle = User32.CreateWindow(
                WindowClassName,
                $"The Invisible Man ({className})",
                Constants.WS_OVERLAPPEDWINDOW,
                0, 0,
                1, 1,
                IntPtr.Zero, IntPtr.Zero, Kernel32.CurrentModuleHandle, IntPtr.Zero);
            if (WindowHandle == IntPtr.Zero)
                throw new ArgumentException($"Could not create window: {className}", nameof(className));

            DeviceContextHandle = User32.GetDC(WindowHandle);
            if (DeviceContextHandle == IntPtr.Zero)
            {
                Dispose();
                throw new ArgumentException($"Could not get device context: {className}", nameof(className));
            }
        }

        #endregion

        #region Methods

        public void Dispose()
        {
            if (WindowHandle != IntPtr.Zero)
            {
                if (DeviceContextHandle != IntPtr.Zero)
                {
                    User32.ReleaseDC(WindowHandle, DeviceContextHandle);
                    DeviceContextHandle = IntPtr.Zero;
                }

                User32.DestroyWindow(WindowHandle);
                WindowHandle = IntPtr.Zero;
            }

            if (classRegistration != 0)
            {
                User32.UnregisterClass(WindowClassName, Kernel32.CurrentModuleHandle);
                classRegistration = 0;
            }
        }

        #endregion
    }
}
