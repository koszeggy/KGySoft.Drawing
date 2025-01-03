#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ColorBgr101010XRSrgb.cs
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

using System.Numerics;
using System.Runtime.CompilerServices;

using KGySoft.Drawing.Imaging;

#endregion

namespace KGySoft.Drawing.SkiaSharp
{
    /// <summary>
    /// https://issues.skia.org/issues/40045149 tells that this is MTLPixelFormatBGR10_XR (NOT MTLPixelFormatBGR10_XR_sRGB, even though this is also an sRGB format, so the range is different)
    /// See https://developer.apple.com/documentation/metal/mtlpixelformat/mtlpixelformatbgr10_xr
    /// </summary>
    internal readonly struct ColorBgr101010XRSrgb
    {
        #region Constants

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

        internal uint R => (value & redMask) >> 20;
        internal uint G => (value & greenMask) >> 10;
        internal uint B => value & blueMask;

        #endregion

        #region Constructors

        #region Internal Constructors
        
        // TODO: vectorize
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ColorBgr101010XRSrgb(Color64 c) => value =
            (uint)(((c.R >> 7) + 384) << 20)
            | (uint)(((c.G >> 7) + 384) << 10)
            | (uint)((c.B >> 7) + 384);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ColorBgr101010XRSrgb(ColorF c) => this = FromSrgb(c.ToSrgb());

        #endregion

        #region Private Constructors

        /// <summary>
        /// Creating from a float sRGB color, where extended range is preserved between -0.752941 and 1.25098.
        /// </summary>
        private ColorBgr101010XRSrgb(Vector3 rgb)
        {
            var uInt10 = (rgb.Clip(min, max) - new Vector3(min)) * (maxEncoded / range) + new Vector3(0.5f);
            value = ((uint)uInt10.X << 20) | ((uint)uInt10.Y << 10) | (uint)uInt10.Z;
        }

        #endregion

        #endregion

        #region Methods

        #region Static Methods

        internal static ColorBgr101010XRSrgb FromSrgb(ColorF c) => new ColorBgr101010XRSrgb(c.ToRgb());

        #endregion

        #region Instance Methods

        // TODO: vectorize
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Color32 ToColor32() => new Color32(
            (byte)((R.Clip(384, 895) - 384) >> 1),
            (byte)((G.Clip(384, 895) - 384) >> 1),
            (byte)((B.Clip(384, 895) - 384) >> 1));

        // TODO: vectorize
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Color64 ToColor64()
        {
            uint r = (R.Clip(384, 895) - 384) << 7;
            uint g = (G.Clip(384, 895) - 384) << 7;
            uint b = (B.Clip(384, 895) - 384) << 7;

            return new Color64((ushort)(r | (r >> 10)),
                (ushort)(g | (g >> 10)),
                (ushort)(b | (b >> 10)));
        }

        /// <summary>
        /// This restores extended range between -0.752941 and 1.25098, so the result ColorF.IsValid can be false
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ColorF ToColorF() => ColorF.FromRgb(new Vector3(R, G, B) * (range / maxEncoded) + new Vector3(min)).ToLinear();

        #endregion

        #endregion
    }
}