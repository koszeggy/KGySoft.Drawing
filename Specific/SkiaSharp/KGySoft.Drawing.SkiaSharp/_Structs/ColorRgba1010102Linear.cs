#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ColorRgba1010102Linear.cs
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
    internal readonly struct ColorRgba1010102Linear
    {
        #region Constants

        private const uint alphaMask = 0b11000000_00000000_00000000_00000000;
        private const uint redMask = 0b00000011_11111111;
        private const uint greenMask = 0b00001111_11111100_00000000;
        private const uint blueMask = 0b00111111_11110000_00000000_00000000;

        #endregion

        #region Fields

        internal readonly uint Value;

        #endregion

        #region Properties

        #region Internal Properties

        internal uint A => (Value & alphaMask) >> 30;
        internal uint B => (Value & blueMask) >> 20;
        internal uint G => (Value & greenMask) >> 10;
        internal uint R => Value & redMask;

        #endregion

        #region Private Properties

        // A * 85 is the same as (byte)((A << 6) | (A << 4) | (A << 2) | A),
        // whereas * 257 is the same as ((value << 8) | value) for the 8 bit result
        private ushort A16 => (ushort)(A * (85 * 257));
        private ushort R16 => (ushort)((R << 6) | (R >> 2));
        private ushort G16 => (ushort)((G << 6) | (G >> 2));
        private ushort B16 => (ushort)((B << 6) | (B >> 2));

        #endregion

        #endregion

        #region Constructors

        internal ColorRgba1010102Linear(Color32 c)
            : this(new Color64(c))
        {
        }

        internal ColorRgba1010102Linear(Color64 c) => Value =
            ((uint)(c.A >> 14) << 30)
            | (uint)((c.B.ToLinear() >> 6) << 20)
            | (uint)((c.G.ToLinear() >> 6) << 10)
            | (uint)(c.R.ToLinear() >> 6);

        internal ColorRgba1010102Linear(ColorF c)
        {
            uint r = ColorSpaceHelper.ToUInt16(c.R);
            uint g = ColorSpaceHelper.ToUInt16(c.G);
            uint b = ColorSpaceHelper.ToUInt16(c.B);
            uint a = ColorSpaceHelper.ToUInt16(c.A);
            Value = (a >> 14 << 30)
                | (b >> 6) << 20
                | (g >> 6) << 10
                | r >> 6;
        }

        #endregion

        #region Methods

        internal Color32 ToColor32() => new Color64(A16, R16.ToSrgb(), G16.ToSrgb(), B16.ToSrgb()).ToColor32();

        #endregion
    }
}