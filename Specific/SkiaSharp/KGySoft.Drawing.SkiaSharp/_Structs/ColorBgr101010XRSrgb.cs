#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ColorBgr101010XRSrgb.cs
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
using System.Runtime.CompilerServices;

using KGySoft.Drawing.Imaging;

#endregion

namespace KGySoft.Drawing.SkiaSharp
{
    /// <summary>
    /// https://issues.skia.org/issues/40045149 tells that this is MTLPixelFormatBGR10_XR.
    /// See https://developer.apple.com/documentation/metal/mtlpixelformat/mtlpixelformatbgr10_xr_srgb
    /// </summary>
    internal readonly struct ColorBgr101010XRSrgb
    {
        #region Constants

        private const uint redMask = 0b00111111_11110000_00000000_00000000;
        private const uint greenMask = 0b00001111_11111100_00000000;
        private const uint blueMask = 0b00000011_11111111;

        #endregion

        #region Fields

        private readonly uint value;

        #endregion

        #region Properties

        internal uint R => (value & redMask) >> 20;
        internal uint G => (value & greenMask) >> 10;
        internal uint B => value & blueMask;

        #endregion

        #region Constructors

        // TODO: vectorize
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ColorBgr101010XRSrgb(Color64 c) => value =
            (uint)(((c.R >> 7) + 384) << 20)
            | (uint)(((c.G >> 7) + 384) << 10)
            | (uint)((c.B >> 7) + 384);

        #endregion

        #region Methods

        // TODO: vectorize
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Color32 ToColor32() => new Color32(
            (byte)((Math.Clamp(R, 384, 895) - 384) >> 1),
            (byte)((Math.Clamp(G, 384, 895) - 384) >> 1),
            (byte)((Math.Clamp(B, 384, 895) - 384) >> 1));

        // TODO: vectorize
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Color64 ToColor64()
        {
            uint r = (Math.Clamp(R, 384, 895) - 384) << 7;
            uint g = (Math.Clamp(G, 384, 895) - 384) << 7;
            uint b = (Math.Clamp(B, 384, 895) - 384) << 7;

            return new Color64((ushort)(r | (r >> 10)),
                (ushort)(g | (g >> 10)),
                (ushort)(b | (b >> 10)));
        }

        #endregion
    }
}