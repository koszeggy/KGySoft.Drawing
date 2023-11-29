#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ColorPbgra1010102Linear.cs
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
using System.Runtime.CompilerServices;

using KGySoft.Drawing.Imaging;

#endregion

namespace KGySoft.Drawing.SkiaSharp
{
    internal readonly struct ColorPbgra1010102Linear
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

        internal ColorPbgra1010102Linear(Color64 c)
        {
            // We can't do a similar initialization from PColor64 as in ColorBgra1010102Linear because A has smaller depth than RGB.
            if (c.A == UInt16.MinValue)
            {
                value = 0u;
                return;
            }

            var straight = new ColorBgra1010102Linear(c);
            if (straight.A == maxAlpha)
            {
                this = Unsafe.As<ColorBgra1010102Linear, ColorPbgra1010102Linear>(ref straight);
                return;
            }

            uint a = straight.A;
            this = new ColorPbgra1010102Linear(a,
                straight.R * a / maxAlpha,
                straight.G * a / maxAlpha,
                straight.B * a / maxAlpha);
            Debug.Assert(ToPColorF().IsValid);
        }

        internal ColorPbgra1010102Linear(ColorF c)
        {
            // We can't do a similar initialization from PColorF as in ColorBgra1010102Linear because A has smaller depth than RGB.
            if (c.A <= 0f)
            {
                value = 0u;
                return;
            }

            var straight = new ColorBgra1010102Linear(c);
            if (straight.A == maxAlpha)
            {
                this = Unsafe.As<ColorBgra1010102Linear, ColorPbgra1010102Linear>(ref straight);
                return;
            }

            uint a = straight.A;
            this = new ColorPbgra1010102Linear(a,
                straight.R * a / maxAlpha,
                straight.G * a / maxAlpha,
                straight.B * a / maxAlpha);
            Debug.Assert(ToPColorF().IsValid);
        }

        #endregion

        #region Private Constructors

        private ColorPbgra1010102Linear(uint a, uint r, uint g, uint b)
        {
            Debug.Assert(a <= maxAlpha && r <= maxRgb && g <= maxRgb && b <= maxRgb);
            value = a << 30
                | r << 20
                | g << 10
                | b;
        }

        #endregion

        #endregion

        #region Methods
     
        #region Internal Methods

        internal Color64 ToColor64()
        {
            Color64 linear64 = ToLinear64().ToStraight();
            return new Color64(linear64.A, linear64.R.ToSrgb(), linear64.G.ToSrgb(), linear64.B.ToSrgb());
        }

        internal PColorF ToPColorF() => ToLinear64().ToPColorF(false);

        #endregion

        #region Private Methods

        private PColor64 ToLinear64()
        {
            uint r = R << 6;
            uint g = G << 6;
            uint b = B << 6;

            // A * 85 is the same as (byte)((A << 6) | (A << 4) | (A << 2) | A),
            // whereas * 257 is the same as ((value << 8) | value) for the 8 bit result
            return new PColor64((ushort)(A * (85 * 257)),
                (ushort)(r | (r >> 10)),
                (ushort)(g | (g >> 10)),
                (ushort)(b | (b >> 10)));
        }

        #endregion

        #endregion
    }
}