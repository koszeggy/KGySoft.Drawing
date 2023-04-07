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

using System;

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

        private const int maxArgb = 15;

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

        #region Internal Constructors

        internal ColorPargb4444Linear(Color32 c)
        {
            PColorF pF = c.ToPColorF();
            this = new ColorPargb4444Linear((byte)(c.A >> 4),
                (byte)(ColorSpaceHelper.ToByte(pF.R) >> 4),
                (byte)(ColorSpaceHelper.ToByte(pF.G) >> 4),
                (byte)(ColorSpaceHelper.ToByte(pF.B) >> 4));

            //// Premultiplication after quantization (results are more like the non-premultiplied format):
            //if (c.A == Byte.MinValue)
            //{
            //    value = 0;
            //    return;
            //}

            //var straight = new ColorArgb4444Linear(c);
            //if (c.A == Byte.MaxValue)
            //{
            //    value = straight.Value;
            //    return;
            //}

            //byte a = straight.A;
            //this = new ColorPargb4444Linear(a,
            //    (byte)(straight.R * a / maxArgb),
            //    (byte)(straight.G * a / maxArgb),
            //    (byte)(straight.B * a / maxArgb));
        }

        #endregion

        #region Private Constructors

        private ColorPargb4444Linear(byte a, byte r, byte g, byte b)
        {
            Debug.Assert(a <= maxArgb && r <= a && g <= a && b <= a);
            value = (ushort)(a
                | r << 12
                | g << 8
                | b << 4);
        }

        #endregion

        #endregion

        #region Methods

        internal Color32 ToColor32()
        {
            byte a = A;
            return a switch
            {
                0 => default,
                maxArgb => new ColorArgb4444Linear(value).ToColor32(),
                _ => new ColorArgb4444Linear(a,
                    (byte)(R * maxArgb / a),
                    (byte)(G * maxArgb / a),
                    (byte)(B * maxArgb / a)).ToColor32()
            };
        }

        #endregion
    }
}