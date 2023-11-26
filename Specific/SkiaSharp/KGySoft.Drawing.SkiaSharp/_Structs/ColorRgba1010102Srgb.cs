#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ColorRgba1010102Srgb.cs
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
    internal readonly struct ColorRgba1010102Srgb
    {
        #region Constants

        private const uint alphaMask = 0b11000000_00000000_00000000_00000000;
        private const uint redMask = 0b00000011_11111111;
        private const uint greenMask = 0b00001111_11111100_00000000;
        private const uint blueMask = 0b00111111_11110000_00000000_00000000;

        #endregion

        #region Fields

        private readonly uint value;

        #endregion

        #region Properties

        internal uint A => (value & alphaMask) >> 30;
        internal uint B => (value & blueMask) >> 20;
        internal uint G => (value & greenMask) >> 10;
        internal uint R => value & redMask;

        #endregion

        #region Constructors

        internal ColorRgba1010102Srgb(Color32 c) => value =
            ((uint)(c.A >> 6) << 30)
            | (uint)(((c.B << 2) | (c.B >> 6)) << 20)
            | (uint)(((c.G << 2) | (c.G >> 6)) << 10)
            | (uint)((c.R << 2) | (c.R >> 6));

        internal ColorRgba1010102Srgb(Color64 c) => value =
            ((uint)(c.A >> 14) << 30)
            | (uint)((c.B >> 6) << 20)
            | (uint)((c.G >> 6) << 10)
            | (uint)(c.R >> 6);

        #endregion

        #region Methods

        internal Color32 ToColor32()
            // A * 85 is the same as (byte)((A << 6) | (A << 4) | (A << 2) | A)
            => new Color32((byte)(A * 85), (byte)(R >> 2), (byte)(G >> 2), (byte)(B >> 2));

        internal Color64 ToColor64()
        {
            uint r = R << 6;
            uint g = G << 6;
            uint b = B << 6;

            // A * 85 is the same as (byte)((A << 6) | (A << 4) | (A << 2) | A),
            // whereas * 257 is the same as ((value << 8) | value) for the 8 bit result
            return new Color64((ushort)(A * (85 * 257)),
                (ushort)(r | (r >> 10)),
                (ushort)(g | (g >> 10)),
                (ushort)(b | (b >> 10)));
        }

        #endregion
    }
}