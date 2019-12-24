#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: Color64.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2019 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution. If not, then this file is considered as
//  an illegal copy.
//
//  Unauthorized copying of this file, via any medium is strictly prohibited.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;
using System.Drawing;

#endregion

namespace KGySoft.Drawing
{
    public readonly struct Color64 : IEquatable<Color64>
    {
        #region Constants

        private const ulong alphaMask = 0xFFFFuL << 48;
        private const ulong redMask = 0xFFFFuL << 32;
        private const ulong greenMask = 0xFFFFuL << 16;
        private const ulong blueMask = 0xFFFF;
        private const ulong rgbMask = redMask | greenMask | blueMask;

        #endregion

        #region Fields

        private readonly ulong value;

        #endregion

        #region Properties

        public int A => (int)(value >> 48);

        public int R => (int)((value & redMask) >> 32);

        public int G => (int)((value & greenMask) >> 16);

        public int B => (int)(value & blueMask);

        #endregion

        #region Operators

        public static implicit operator Color64(Color32 c) => FromColor32(c);

        public static explicit operator Color32(Color64 c) => c.ToColor32();

        public static explicit operator Color(Color64 c) => c.ToColor();

        #endregion

        #region Constructors

        #region Public Constructors

        [CLSCompliant(false)]
        public Color64(ushort r, ushort g, ushort b)
            => value = alphaMask
            | ((ulong)r << 32)
            | ((ulong)g << 16)
            | b;

        [CLSCompliant(false)]
        public Color64(ushort a, ushort r, ushort g, ushort b)
            => value = ((ulong)a << 48)
            | ((ulong)r << 32)
            | ((ulong)g << 16)
            | b;

        #endregion

        #region Internal Constructors

        internal Color64(ulong argb) => value = argb;

        #endregion

        #endregion

        #region Methods

        #region Static Methods

        public static Color64 FromColor(Color c) => FromArgb32(c.ToArgb());

        public static Color64 FromColor32(Color32 c) => new Color64((ushort)(c.A << 8), (ushort)(c.R << 8), (ushort)(c.G << 8), (ushort)(c.B << 8));

        public static Color64 FromArgb32(int argb) => FromColor32(Color32.FromArgb(argb));

        public static Color64 FromArgb(long argb) => new Color64((ulong)argb);

        [CLSCompliant(false)]
        public static Color64 FromArgb(ushort a, Color64 baseColor)
            => new Color64(((ulong)a << 48) | (baseColor.value & rgbMask));

        public static Color64 FromRgb32(int rgb) => FromColor32(Color32.FromRgb(rgb));

        public static Color64 FromRgb(long rgb) => new Color64(alphaMask | (ulong)rgb);

        [CLSCompliant(false)]
        public static Color64 FromGray(ushort level) => new Color64(level, level, level);

        #endregion

        #region Instance Methods

        public Color ToColor() => ToColor32().ToColor();
        
        public Color32 ToColor32() => new Color32((byte)(A >> 8), (byte)(R >> 8), (byte)(G >> 8), (byte)(B >> 8));

        public long ToArgb() => (long)value;

        public int ToArgb32() => ToColor32().ToArgb();

        public bool Equals(Color64 other) => value == other.value;

        public override bool Equals(object obj) => obj is Color64 other && Equals(other);

        public override int GetHashCode() => value.GetHashCode();

        public override string ToString() => $"{value:X16} [A={A}; R={R}; G={G}; B={B}]";

        #endregion

        #endregion
    }
}
