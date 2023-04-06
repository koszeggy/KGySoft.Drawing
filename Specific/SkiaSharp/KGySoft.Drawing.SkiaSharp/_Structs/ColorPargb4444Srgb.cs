#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ColorPargb4444Srgb.cs
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
    internal readonly struct ColorPargb4444Srgb
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

        internal ColorPargb4444Srgb(Color32 c)
        {
            // NOTE: This would be the SkiaSharp-compatible solution. But premultiplying it _before_ converting the color space
            //       ends up in suboptimal gradients for non-black backgrounds
            //var pc64 = c.ToPremultiplied();
            //this = new ColorPargb4444Srgb((byte)(pc64.A >> 4), (byte)(pc64.R >> 4), (byte)(pc64.G >> 4), (byte)(pc64.B >> 4));

            if (c.A == Byte.MinValue)
            {
                value = 0;
                return;
            }

            var straight = new ColorArgb4444Srgb(c);
            if (c.A == Byte.MaxValue)
            {
                value = straight.Value;
                return;
            }

            byte a = straight.A;
            this = new ColorPargb4444Srgb(a,
                (byte)(straight.R * a / alphaMask),
                (byte)(straight.G * a / alphaMask),
                (byte)(straight.B * a / alphaMask));
        }

        #endregion

        #region Private Constructors

        private ColorPargb4444Srgb(byte a, byte r, byte g, byte b)
        {
            Debug.Assert(a <= maxArgb && r <= maxArgb && g <= maxArgb && b <= maxArgb);
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
                (byte)alphaMask => new ColorArgb4444Srgb(value).ToColor32(),
                _ => new ColorArgb4444Srgb(a,
                    (byte)(R * alphaMask / a),
                    (byte)(G * alphaMask / a),
                    (byte)(B * alphaMask / a)).ToColor32()
            };
        }

        #endregion
    }
}