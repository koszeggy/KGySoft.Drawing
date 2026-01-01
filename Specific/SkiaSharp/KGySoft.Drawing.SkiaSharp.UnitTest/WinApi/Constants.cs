#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: Constants.cs
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

#if NET
using System.Runtime.Versioning;
#endif

#endregion

namespace KGySoft.Drawing.SkiaSharp.WinApi
{
#if NET
    [SupportedOSPlatform("windows")]
#endif
    internal static class Constants
    {
        #region Constants

        internal const int NONE = 0;
        internal const int FALSE = 0;
        internal const int TRUE = 1;

        internal const int GL_VERSION = 0x1F02;

        internal const int WGL_DRAW_TO_WINDOW_ARB = 0x2001;
        internal const int WGL_ACCELERATION_ARB = 0x2003;
        internal const int WGL_SUPPORT_OPENGL_ARB = 0x2010;
        internal const int WGL_RED_BITS_ARB = 0x2015;
        internal const int WGL_GREEN_BITS_ARB = 0x2017;
        internal const int WGL_BLUE_BITS_ARB = 0x2019;
        internal const int WGL_ALPHA_BITS_ARB = 0x201B;
        internal const int WGL_STENCIL_BITS_ARB = 0x2023;
        internal const int WGL_FULL_ACCELERATION_ARB = 0x2027;

        internal const byte PFD_TYPE_RGBA = 0;
        internal const byte PFD_MAIN_PLANE = 0;
        internal const uint PFD_DRAW_TO_WINDOW = 0x00000004;
        internal const uint PFD_SUPPORT_OPENGL = 0x00000020;

        internal const uint WS_CAPTION = 0xc00000;
        internal const uint WS_CLIPCHILDREN = 0x2000000;
        internal const uint WS_CLIPSIBLINGS = 0x4000000;
        internal const uint WS_MAXIMIZEBOX = 0x10000;
        internal const uint WS_MINIMIZEBOX = 0x20000;
        internal const uint WS_OVERLAPPED = 0x0;
        internal const uint WS_OVERLAPPEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_SIZEFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX;
        internal const uint WS_SIZEFRAME = 0x40000;
        internal const uint WS_SYSMENU = 0x80000;
        internal const uint WS_EX_CLIENTEDGE = 0x00000200;

        internal const uint IDC_ARROW = 32512;
        internal const uint IDI_APPLICATION = 32512;
        internal const uint IDI_WINLOGO = 32517;

        internal const uint CS_VREDRAW = 0x1;
        internal const uint CS_HREDRAW = 0x2;
        internal const uint CS_OWNDC = 0x20;

        #endregion
    }
}
