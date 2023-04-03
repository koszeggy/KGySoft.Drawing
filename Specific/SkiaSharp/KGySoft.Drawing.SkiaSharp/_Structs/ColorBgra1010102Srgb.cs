#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ColorBgra1010102Srgb.cs
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

using KGySoft.Drawing.Imaging;

#endregion

namespace KGySoft.Drawing.SkiaSharp
{
    internal readonly struct ColorBgra1010102Srgb
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

        internal readonly uint Value;

        #endregion

        #region Properties

        internal uint A => (Value & alphaMask) >> 30;
        internal uint R => (Value & redMask) >> 20;
        internal uint G => (Value & greenMask) >> 10;
        internal uint B => Value & blueMask;

        #endregion

        #region Constructors

        internal ColorBgra1010102Srgb(Color32 c) => Value =
            ((uint)(c.A >> 6) << 30)
            | (uint)(((c.R << 2) | (c.R >> 6)) << 20)
            | (uint)(((c.G << 2) | (c.G >> 6)) << 10)
            | (uint)((c.B << 2) | (c.B >> 6));

        internal ColorBgra1010102Srgb(uint a, uint r, uint g, uint b) => Value =
            (a & maxAlpha) << 30
            | (r & maxRgb) << 20
            | (g & maxRgb) << 10
            | (b & maxRgb);

        internal ColorBgra1010102Srgb(uint value) => Value = value;

        #endregion

        #region Methods

        internal Color32 ToColor32()
        {
            uint a = A;
            return new Color32((byte)((a << 6) | (a << 4) | (a << 2) | a), (byte)(R >> 2), (byte)(G >> 2), (byte)(B >> 2));
        }

        #endregion
    }
}