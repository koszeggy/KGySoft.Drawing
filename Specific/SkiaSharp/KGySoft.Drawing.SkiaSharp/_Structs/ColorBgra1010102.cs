#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ColorBgra1010102.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2022 - All Rights Reserved
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
    internal readonly struct ColorBgra1010102
    {
        #region Constants

        private const uint alphaMask = 0xC0_00_00_00;
        private const uint redMask = 0x3F_F0_00_00;
        private const uint greenMask = 0x00_0F_FC_00;
        private const uint blueMask = 0x00_00_03_FF;

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

        internal ColorBgra1010102(Color32 c) => value =
            ((uint)(c.A >> 6) << 30)
            | (uint)(((c.R << 2) | (c.R >> 6)) << 20)
            | (uint)(((c.G << 2) | (c.G >> 6)) << 10)
            | (uint)((c.B << 2) | (c.B >> 6));

        //internal ColorBgra1010102(Color32 c, Color32 backColor)
        //    : this(c.A switch
        //    {
        //        255 or < 64 => c,
        //        >= 192 => c.Blend(backColor),
        //        >= 128 => Color32.FromArgb(c.A, c.Blend(Color32.FromArgb((byte)(c.A >> 2), backColor))),
        //        _ => Color32.FromArgb(c.A, c.Blend(Color32.FromArgb((byte)(c.A >> 3), backColor))),
        //    })
        //{
        //}

        #endregion

        #region Private Constructors

        private ColorBgra1010102(uint a, uint r, uint g, uint b) => value =
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
            return new Color32((byte)((a << 6) | (a << 4) | (a << 2) | a), (byte)(R >> 2), (byte)(G >> 2), (byte)(B >> 2));
        }

        internal ColorBgra1010102 ToStraight()
        {
            switch (value & alphaMask)
            {
                case alphaMask:
                    return this;
                case 0:
                    return default;
                default:
                    {
                        uint a = A;
                        return new ColorBgra1010102(a,
                            R * maxAlpha / a,
                            G * maxAlpha / a,
                            B * maxAlpha / a);
                    }
            }
        }

        internal ColorBgra1010102 ToPremultiplied()
        {
            switch (value & alphaMask)
            {
                case alphaMask:
                    return this;
                case 0:
                    return default;
                default:
                    {
                        uint a = A;
                        return new ColorBgra1010102(a,
                            R * a / maxAlpha,
                            G * a / maxAlpha,
                            B * a / maxAlpha);
                    }
            }
        }

        #endregion
    }
}