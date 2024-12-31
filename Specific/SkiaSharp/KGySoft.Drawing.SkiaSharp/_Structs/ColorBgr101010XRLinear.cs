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

using KGySoft.Drawing.Imaging;

#endregion

namespace KGySoft.Drawing.SkiaSharp
{
    /// <summary>
    /// https://issues.skia.org/issues/40045149 tells that this is MTLPixelFormatBGR10_XR.
    /// See https://developer.apple.com/documentation/metal/mtlpixelformat/mtlpixelformatbgr10_xr_srgb
    /// </summary>
    internal readonly struct ColorBgr101010XRLinear
    {
        #region Constants

        private const uint alphaMask = 0b11000000_00000000_00000000_00000000;
        private const uint redMask = 0b00111111_11110000_00000000_00000000;
        private const uint greenMask = 0b00001111_11111100_00000000;
        private const uint blueMask = 0b00000011_11111111;

        #endregion

        #region Fields

        private readonly uint value;

        #endregion

        #region Properties

        internal uint A => (value & alphaMask) >> 30;
        internal uint R => (value & redMask) >> 20;
        internal uint G => (value & greenMask) >> 10;
        internal uint B => value & blueMask;

        #endregion

        #region Constructors

        internal ColorBgr101010XRLinear(Color64 c) => value =
            (uint)(((c.R.ToLinear() >> 7) + 384) << 20)
            | (uint)(((c.G.ToLinear() >> 7) + 384) << 10)
            | (uint)((c.B.ToLinear() >> 7) + 384);

        internal ColorBgr101010XRLinear(ColorF c)
        {
            Color64 linear64 = c.ToColor64(false);
            value = (uint)(((linear64.R >> 7) + 384) << 20)
                | (uint)(((linear64.G >> 7) + 384) << 10)
                | (uint)((linear64.B >> 7) + 384);
        }

        #endregion

        #region Methods

        #region Internal Methods

        internal Color64 ToColor64()
        {
            Color64 linear64 = ToLinear64();
            return new Color64(linear64.A, linear64.R.ToSrgb(), linear64.G.ToSrgb(), linear64.B.ToSrgb());
        }

        internal ColorF ToColorF() => ToLinear64().ToColorF(false);

        #endregion

        #region Private Methods

        private Color64 ToLinear64()
        {
            uint r = (Math.Clamp(R, 384, 895) - 384) << 7;
            uint g = (Math.Clamp(G, 384, 895) - 384) << 7;
            uint b = (Math.Clamp(B, 384, 895) - 384) << 7;

            return new Color64((ushort)(r | (r >> 10)),
                (ushort)(g | (g >> 10)),
                (ushort)(b | (b >> 10)));
        }

        #endregion

        #endregion
    }
}