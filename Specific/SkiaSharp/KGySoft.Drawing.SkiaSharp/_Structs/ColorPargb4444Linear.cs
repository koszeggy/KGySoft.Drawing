#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ColorPargb4444Linear.cs
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
    internal readonly struct ColorPargb4444Linear
    {
        #region Constants

        private const ushort alphaMask = 0x00_0F;
        private const ushort redMask = 0xF0_00;
        private const ushort greenMask = 0x0F_00;
        private const ushort blueMask = 0x00_F0;

        #endregion

        #region Fields

        private readonly ushort value;

        #endregion

        #region Properties

        private byte A => (byte)(value & alphaMask);
        private byte R => (byte)((value & redMask) >> 12);
        private byte G => (byte)((value & greenMask) >> 8);
        private byte B => (byte)((value & blueMask) >> 4);

        #endregion

        #region Constructors

        internal ColorPargb4444Linear(PColorF c)
        {
            PColor32 linear32 = c.ToPColor32(false);
            value = (ushort)((linear32.A >> 4)
                | ((linear32.R >> 4) << 12)
                | ((linear32.G >> 4) << 8)
                | ((linear32.B >> 4) << 4));
        }

        #endregion

        #region Methods
        
        #region Internal Methods

        internal Color32 ToColor32()
        {
            // value * 17 is the same as (value | (value << 4))
            Color32 linear32 = ToLinear32().ToStraight();
            return new Color32(linear32.A, linear32.R.ToSrgb(), linear32.G.ToSrgb(), linear32.B.ToSrgb());
        }

        internal PColorF ToPColorF() => ToLinear32().ToPColorF(false);

        #endregion

        #region Pivate Methods

        // value * 17 is the same as (value | (value << 4))
        private PColor32 ToLinear32() => new PColor32((byte)(A * 17),
            ((byte)(R * 17)),
            ((byte)(G * 17)),
            ((byte)(B * 17)));

        #endregion

        #endregion
    }
}