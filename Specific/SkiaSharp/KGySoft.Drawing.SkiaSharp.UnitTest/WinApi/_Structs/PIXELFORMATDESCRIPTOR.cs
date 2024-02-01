#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: PIXELFORMATDESCRIPTOR.cs
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

using System.Diagnostics.CodeAnalysis;
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
    [StructLayout(LayoutKind.Sequential)]
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Windows API")]
    [SuppressMessage("ReSharper", "IdentifierTypo")]
    internal struct PIXELFORMATDESCRIPTOR
    {
        #region Fields

        internal ushort nSize;
        internal ushort nVersion;
        internal uint dwFlags;
        internal byte iPixelType;
        internal byte cColorBits;
        internal byte cRedBits;
        internal byte cRedShift;
        internal byte cGreenBits;
        internal byte cGreenShift;
        internal byte cBlueBits;
        internal byte cBlueShift;
        internal byte cAlphaBits;
        internal byte cAlphaShift;
        internal byte cAccumBits;
        internal byte cAccumRedBits;
        internal byte cAccumGreenBits;
        internal byte cAccumBlueBits;
        internal byte cAccumAlphaBits;
        internal byte cDepthBits;
        internal byte cStencilBits;
        internal byte cAuxBuffers;
        internal byte iLayerType;
        internal byte bReserved;
        internal int dwLayerMask;
        internal int dwVisibleMask;
        internal int dwDamageMask;

        #endregion
    }
}
