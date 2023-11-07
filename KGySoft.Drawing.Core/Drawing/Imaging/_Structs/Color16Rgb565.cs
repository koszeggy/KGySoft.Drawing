#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: Color16Rgb565.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2023 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

using System;

namespace KGySoft.Drawing.Imaging
{
    internal readonly struct Color16Rgb565 : IEquatable<Color16Rgb565>
    {
        #region Constants

        private const ushort redMask = 0b11111000_00000000;
        private const ushort greenMask = 0b00000111_11100000;
        private const ushort blueMask = 0b00011111;

        #endregion

        #region Fields

        internal readonly ushort Value;

        #endregion

        #region Properties

        private byte R => (byte)(((Value & redMask) >> 8) | (Value >> 13));
        private byte G => (byte)(((Value & greenMask) >> 3) | ((Value & greenMask) >> 9));
        private byte B => (byte)(((Value & blueMask) << 3) | ((Value & blueMask) >> 2));

        #endregion

        #region Constructors

        internal Color16Rgb565(Color32 c)
        {
            Value = (ushort)((((uint)c.R << 8) & redMask)
                | (((uint)c.G << 3) & greenMask)
                | ((uint)c.B >> 3));
        }

        #endregion

        #region Methods

        #region Public Methods

        public override int GetHashCode() => Value;

        public bool Equals(Color16Rgb565 other) => Value == other.Value;

        public override bool Equals(object? obj) => obj is Color16Rgb565 other && Equals(other);

        #endregion

        #region Internal Methods

        internal Color32 ToColor32() => new Color32(R, G, B);

        #endregion

        #endregion
    }
}