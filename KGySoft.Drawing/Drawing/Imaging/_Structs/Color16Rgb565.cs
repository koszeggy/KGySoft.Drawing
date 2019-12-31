#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: Color16Rgb565.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2020 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution. If not, then this file is considered as
//  an illegal copy.
//
//  Unauthorized copying of this file, via any medium is strictly prohibited.
///////////////////////////////////////////////////////////////////////////////

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal readonly struct Color16Rgb565
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

        internal Color16Rgb565(Color32 c)
        {
            value = (ushort)((((uint)c.R << 8) & redMask)
                | (((uint)c.G << 3) & greenMask)
                | ((uint)c.B >> 3));
        }

        #endregion

        #region Methods

        internal Color32 ToColor32() => new Color32(R, G, B);

        #endregion
    }
}