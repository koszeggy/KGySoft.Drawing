#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: Color16Argb1555.cs
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
    internal readonly struct Color16Argb1555 : IEquatable<Color16Argb1555>
    {
        #region Constants

        private const ushort alphaMask = 0b10000000_00000000;
        private const ushort redMask = 0b01111100_00000000;
        private const ushort greenMask = 0b00000011_11100000;
        private const ushort blueMask = 0b00011111;

        #endregion

        #region Fields

        internal readonly ushort Value;

        #endregion

        #region Properties

        internal byte A => (Value & alphaMask) == 0 ? (byte)0 : Byte.MaxValue;
        internal byte R => (byte)(((Value & redMask) >> 7) | ((Value & redMask) >> 12));
        internal byte G => (byte)(((Value & greenMask) >> 2) | ((Value & greenMask) >> 7));
        internal byte B => (byte)(((Value & blueMask) << 3) | ((Value & blueMask) >> 2));

        #endregion

        #region Constructors

        internal Color16Argb1555(Color32 c)
        {
            Value = (ushort)((((uint)c.A << 8) & alphaMask)
                | (((uint)c.R << 7) & redMask)
                | (((uint)c.G << 2) & greenMask)
                | ((uint)c.B >> 3));
        }

        #endregion

        #region Methods

        #region Public Methods

        public override int GetHashCode() => Value;

        public bool Equals(Color16Argb1555 other) => Value == other.Value;

        public override bool Equals(object? obj) => obj is Color16Argb1555 other && Equals(other);

        #endregion

        #region Internal Methods

        internal Color32 ToColor32() => new Color32(A, R, G, B);

        #endregion

        #endregion
    }
}