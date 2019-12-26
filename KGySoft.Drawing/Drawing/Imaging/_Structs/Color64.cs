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
using System.Runtime.InteropServices;

#endregion

namespace KGySoft.Drawing.Imaging
{
    [StructLayout(LayoutKind.Explicit)]
    public readonly struct Color64 : IEquatable<Color64>
    {
        #region Constants

        private const ulong alphaMask = 0xFFFF_0000_0000_0000;
        private const ulong rgbMask = 0x0000_FFFF_FFFF_FFFF;

        #endregion

        #region Fields

        #region Public Fields

        [CLSCompliant(false)]
        [FieldOffset(6)]
        public readonly ushort A;

        [CLSCompliant(false)]
        [FieldOffset(4)]
        public readonly ushort R;

        [CLSCompliant(false)]
        [FieldOffset(2)]
        public readonly ushort G;

        [CLSCompliant(false)]
        [FieldOffset(0)]
        public readonly ushort B;

        #endregion

        #region Private Fields

        [FieldOffset(0)]
        private readonly ulong value;

        #endregion

        #endregion

        #region Operators

        public static implicit operator Color64(Color32 c) => FromColor32(c);

        public static explicit operator Color32(Color64 c) => c.ToColor32();

        public static explicit operator Color(Color64 c) => c.ToColor();

        #endregion

        #region Constructors

        #region Public Constructors

        [CLSCompliant(false)]
        public Color64(ushort a, ushort r, ushort g, ushort b)
            : this() // so the compiler does not complain about not initializing value
        {
            B = b;
            G = g;
            R = r;
            A = a;
        }

        [CLSCompliant(false)]
        public Color64(ushort r, ushort g, ushort b)
            : this() // so the compiler does not complain about not initializing value
        {
            B = b;
            G = g;
            R = r;
            A = UInt16.MaxValue;
        }

        #endregion

        #region Internal Constructors

        internal Color64(ulong argb)
            : this() // so the compiler does not complain about not initializing ARGB fields
        {
            value = argb;
        }

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
