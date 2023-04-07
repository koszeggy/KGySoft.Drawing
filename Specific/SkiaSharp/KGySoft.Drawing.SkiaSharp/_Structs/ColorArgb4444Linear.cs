#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ColorArgb4444Linear.cs
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
    internal readonly struct ColorArgb4444Linear
    {
        #region Constants

        private const ushort alphaMask = 0x00_0F;
        private const ushort redMask = 0xF0_00;
        private const ushort greenMask = 0x0F_00;
        private const ushort blueMask = 0x00_F0;

        private const int maxArgb = 15;

        #endregion

        #region Fields

        internal readonly ushort Value;

        #endregion

        #region Properties

        internal byte A => (byte)(Value & alphaMask);
        internal byte R => (byte)((Value & redMask) >> 12);
        internal byte G => (byte)((Value & greenMask) >> 8);
        internal byte B => (byte)((Value & blueMask) >> 4);

        #endregion

        #region Constructors

        internal ColorArgb4444Linear(Color32 c) => Value =
            (ushort)((c.A >> 4)
                | ((c.R.ToLinear() >> 4) << 12)
                | ((c.G.ToLinear() >> 4) << 8)
                | ((c.B.ToLinear() >> 4) << 4));

        internal ColorArgb4444Linear(ColorF c) => Value =
            (ushort)((ColorSpaceHelper.ToByte(c.A) >> 4)
                | ((ColorSpaceHelper.ToByte(c.R) >> 4) << 12)
                | ((ColorSpaceHelper.ToByte(c.G) >> 4) << 8)
                | ((ColorSpaceHelper.ToByte(c.B) >> 4) << 4));

        internal ColorArgb4444Linear(byte a, byte r, byte g, byte b)
        {
            Debug.Assert(a <= maxArgb && r <= maxArgb && g <= maxArgb && b <= maxArgb);
            Value = (ushort)(a
                | r << 12
                | g << 8
                | b << 4);
        }

        internal ColorArgb4444Linear(ushort value) => Value = value;

        #endregion

        #region Methods

        // value * 17 is the same as (value | (value << 4))
        internal Color32 ToColor32()
            => new Color32((byte)(A * 17),
                ((byte)(R * 17)).ToSrgb(),
                ((byte)(G * 17)).ToSrgb(),
                ((byte)(B * 17)).ToSrgb());

        #endregion
    }
}