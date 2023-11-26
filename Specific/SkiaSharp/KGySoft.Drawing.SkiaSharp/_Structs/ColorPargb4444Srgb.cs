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

        internal ColorPargb4444Srgb(PColor32 c) => value =
            (ushort)((c.A >> 4)
                | ((c.R >> 4) << 12)
                | ((c.G >> 4) << 8)
                | ((c.B >> 4) << 4));

        #endregion

        #region Methods

        internal PColor32 ToPColor32()
            // value * 17 is the same as (value | (value << 4))
            => new PColor32((byte)(A * 17), (byte)(R * 17), (byte)(G * 17), (byte)(B * 17));

        #endregion
    }
}