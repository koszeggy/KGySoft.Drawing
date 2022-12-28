#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ColorArgb4444.cs
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

using System;
using System.Drawing;

using KGySoft.Drawing.Imaging;

#endregion

namespace KGySoft.Drawing.SkiaSharp
{
    internal readonly struct ColorArgb4444
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

        private byte A => (byte)((value & alphaMask) | ((value & alphaMask) << 4));
        private byte R => (byte)(((value & redMask) >> 8) | ((value & redMask) >> 12));
        private byte G => (byte)(((value & greenMask) >> 4) | ((value & greenMask) >> 8));
        private byte B => (byte)((value & blueMask) | ((value & blueMask) >> 4));

        #endregion

        #region Constructors

        #region Internal Constructors

        internal ColorArgb4444(Color32 c) => value =
            (ushort)((c.A >> 4)
                | ((c.R >> 4) << 12)
                | ((c.G >> 4) << 8)
                | ((c.B >> 4) << 4));

        //internal ColorArgb4444(Color32 c, Color32 backColor)
        //    : this(c.A switch
        //    {
        //        255 or < 16 => c,
        //        >= 240 => c.Blend(backColor),
        //        _ => Color32.FromArgb(c.A , c.Blend(Color32.FromArgb((byte)(c.A / ((256 + 16 - c.A) / 16)), backColor))),
        //    })
        //{
        //}

        #endregion

        #region Private Constructors

        private ColorArgb4444(byte a, byte r, byte g, byte b) => value =
            (ushort)((a >> 4)
                | ((r >> 4) << 12)
                | ((g >> 4) << 8)
                | ((b >> 4) << 4));

        #endregion

        #endregion

        #region Methods

        internal Color32 ToColor32() => new Color32(A, R, G, B);

        internal ColorArgb4444 ToStraight()
        {
            switch (value & alphaMask)
            {
                case alphaMask:
                    return this;
                case 0:
                    return default;
                default:
                    //return new ColorArgb4444(ToColor32().ToStraight());
                    {
                        byte a = (byte)(value & alphaMask);
                        return new ColorArgb4444(A,
                            //(byte)((R << 8) / a),
                            //(byte)((G << 8) / a),
                            //(byte)((B << 8) / a));
                            //(byte)(R * Byte.MaxValue / a),
                            //(byte)(G * Byte.MaxValue / a),
                            //(byte)(B * Byte.MaxValue / a));
                            (byte)(R * alphaMask / a),
                            (byte)(G * alphaMask / a),
                            (byte)(B * alphaMask / a));
                    }
            }
        }

        internal ColorArgb4444 ToPremultiplied()
        {
            switch (value & alphaMask)
            {
                case alphaMask:
                    return this;
                case 0:
                    return default;
                default:
                    //return new ColorArgb4444(ToColor32().ToPremultiplied());
                    {
                        byte a = (byte)(value & alphaMask);
                        return new ColorArgb4444(A,
                            //(byte)((R * a) >> 8),
                            //(byte)((G * a) >> 8),
                            //(byte)((B * a) >> 8));
                            //(byte)(R * a / Byte.MaxValue),
                            //(byte)(G * a / Byte.MaxValue),
                            //(byte)(B * a / Byte.MaxValue));
                            (byte)(R * a / alphaMask),
                            (byte)(G * a / alphaMask),
                            (byte)(B * a / alphaMask));
                    }
            }
        }

        #endregion
    }
}