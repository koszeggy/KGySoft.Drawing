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
using System.Numerics;
using System.Runtime.CompilerServices;

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

        private const float min = -0.752941f;
        private const float max = 1.25098f;
        private const float range = max - min;
        private const float maxEncoded = 1023f;

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

        /// <summary>
        /// Creating from float linear RGB values, where extended range is preserved between -0.752941 and 1.25098.
        /// </summary>
        internal ColorBgr101010XRLinear(ColorF c)
        {
            var uInt10 = (c.ToRgb().Clip(min, max) - new Vector3(min)) * (maxEncoded / range) + new Vector3(0.5f);
            value = ((uint)uInt10.X << 20) | ((uint)uInt10.Y << 10) | (uint)uInt10.Z;
        }

        #endregion

        #region Methods

        internal Color64 ToColor64()
        {
            uint r = (Math.Clamp(R, 384, 895) - 384) << 7;
            uint g = (Math.Clamp(G, 384, 895) - 384) << 7;
            uint b = (Math.Clamp(B, 384, 895) - 384) << 7;
            var linear64 = new Color64((ushort)(r | (r >> 10)),
                (ushort)(g | (g >> 10)),
                (ushort)(b | (b >> 10)));

            return new Color64(linear64.A, linear64.R.ToSrgb(), linear64.G.ToSrgb(), linear64.B.ToSrgb());
        }

        /// <summary>
        /// This restores extended range between -0.752941 and 1.25098, so the result ColorF.IsValid can be false
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ColorF ToColorF() => ColorF.FromRgb(new Vector3(R, G, B) * (range / maxEncoded) + new Vector3(min));

        #endregion
    }
}