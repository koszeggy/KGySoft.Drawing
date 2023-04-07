#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ColorPbgra1010102Srgb.cs
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

using KGySoft.Drawing.Imaging;

#endregion

namespace KGySoft.Drawing.SkiaSharp
{
    internal readonly struct ColorPbgra1010102Srgb
    {
        #region Constants

        private const uint alphaMask = 0b11000000_00000000_00000000_00000000;
        private const uint redMask = 0b00111111_11110000_00000000_00000000;
        private const uint greenMask = 0b00001111_11111100_00000000;
        private const uint blueMask = 0b00000011_11111111;

        private const int maxAlpha = 3;
        private const int maxRgb = 1023;

        #endregion

        #region Fields

        private readonly uint value;

        #endregion

        #region Properties

        private uint A => (value & alphaMask) >> 30;
        private uint R => (value & redMask) >> 20;
        private uint G => (value & greenMask) >> 10;
        private uint B => value & blueMask;

        // A * 85 is the same as (byte)((A << 6) | (A << 4) | (A << 2) | A),
        // whereas * 257 is the same as ((value << 8) | value) for the 8 bit result
        private ushort A16 => (ushort)(A * (85 * 257));
        private ushort R16 => (ushort)((R << 6) | (R >> 2));
        private ushort G16 => (ushort)((G << 6) | (G >> 2));
        private ushort B16 => (ushort)((B << 6) | (B >> 2));

        #endregion

        #region Constructors

        #region Internal Constructors

        internal ColorPbgra1010102Srgb(Color32 c)
        {
            if (c.A == Byte.MinValue)
            {
                value = 0u;
                return;
            }

            var straight = new ColorBgra1010102Srgb(c);
            if (straight.A == maxAlpha)
            {
                value = straight.Value;
                return;
            }

            uint a = straight.A;
            this = new ColorPbgra1010102Srgb(a,
                straight.R * a / maxAlpha,
                straight.G * a / maxAlpha,
                straight.B * a / maxAlpha);
            Debug.Assert(R16 <= A16 && G16 <= A16 && B16 <= A16);
        }

        #endregion

        #region Private Constructors

        private ColorPbgra1010102Srgb(uint a, uint r, uint g, uint b)
        {
            Debug.Assert(a <= maxAlpha && r <= maxRgb && g <= maxRgb && b <= maxRgb);
            value = a << 30
                | r << 20
                | g << 10
                | b;
            Debug.Assert(R16 <= A16 && G16 <= A16 && B16 <= A16);
        }

        #endregion

        #endregion

        #region Methods

        internal Color32 ToColor32()
        {
            uint a = A;
            return a switch
            {
                0u => default,
                maxAlpha => new ColorBgra1010102Srgb(value).ToColor32(),
                _ => new ColorBgra1010102Srgb(a,
                    Math.Min(maxRgb, R * maxAlpha / a),
                    Math.Min(maxRgb, G * maxAlpha / a),
                    Math.Min(maxRgb, B * maxAlpha / a)).ToColor32()
            };
        }

        #endregion
    }
}
