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
            if (c.A == Byte.MaxValue)
            {
                value = straight.Value;
                return;
            }

            uint a = straight.A;
            this = new ColorPbgra1010102Srgb(a,
                straight.R * a / maxAlpha,
                straight.G * a / maxAlpha,
                straight.B * a / maxAlpha);
        }

        #endregion

        #region Private Constructors

        private ColorPbgra1010102Srgb(uint a, uint r, uint g, uint b) => value =
            (a & maxAlpha) << 30
            | (r & maxRgb) << 20
            | (g & maxRgb) << 10
            | (b & maxRgb);

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
                    R * maxAlpha / a,
                    G * maxAlpha / a,
                    B * maxAlpha / a).ToColor32()
            };
        }

        #endregion
    }
}
