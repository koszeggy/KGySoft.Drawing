#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ColorRgb565Linear.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2025 - All Rights Reserved
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
    internal readonly struct ColorRgb565Linear
    {
        #region Constants

        private const ushort redMask = 0b11111000_00000000;
        private const ushort greenMask = 0b00000111_11100000;
        private const ushort blueMask = 0b00011111;

        #endregion

        #region Fields

        private readonly ushort value;

        #endregion

        #region Properties

        private byte R => (byte)(((value & redMask) >> 8) | (value >> 13));
        private byte G => (byte)(((value & greenMask) >> 3) | ((value & greenMask) >> 9));
        private byte B => (byte)(((value & blueMask) << 3) | ((value & blueMask) >> 2));

        #endregion

        #region Constructors

        internal ColorRgb565Linear(Color32 c)
        {
            Debug.Assert(c.A == Byte.MaxValue);
            value = (ushort)((((uint)c.R.ToLinear() << 8) & redMask)
                | (((uint)c.G.ToLinear() << 3) & greenMask)
                | ((uint)c.B.ToLinear() >> 3));
        }

        internal ColorRgb565Linear(ColorF c)
        {
            Debug.Assert(c.A >= 1f);
            Color32 linear32 = c.ToColor32(false);
            value = (ushort)((((uint)linear32.R << 8) & redMask)
                | (((uint)linear32.G << 3) & greenMask)
                | ((uint)linear32.B >> 3));
        }

        #endregion

        #region Methods

        internal Color32 ToColor32() => new Color32(R.ToSrgb(), G.ToSrgb(), B.ToSrgb());
        internal ColorF ToColorF() => new Color32(R, G, B).ToColorF(false);

        #endregion
    }
}
