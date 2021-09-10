#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: Color64.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2021 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

#endregion

namespace KGySoft.Drawing.Imaging
{
    /// <summary>
    /// Represents a 64-bit ARGB color.
    /// Implements <see cref="IEquatable{T}"/> because used in a <see cref="HashSet{T}"/> in <see cref="BitmapDataExtensions.GetColorCount{T}"/>
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    internal readonly struct Color64 : IEquatable<Color64>
    {
        #region Constants

        private const ulong alphaMask = 0xFFFF_0000_0000_0000;
        private const ulong rgbMask = 0x0000_FFFF_FFFF_FFFF;

        #endregion

        #region Fields

        #region Public Fields

        [FieldOffset(6)]
        public readonly ushort A;

        [FieldOffset(4)]
        public readonly ushort R;

        [FieldOffset(2)]
        public readonly ushort G;

        [FieldOffset(0)]
        public readonly ushort B;

        #endregion

        #region Private Fields

        [FieldOffset(0)]
        private readonly ulong value;

        #endregion

        #endregion

        #region Operators

        public static bool operator ==(Color64 a, Color64 b) => a.Equals(b);

        public static bool operator !=(Color64 a, Color64 b) => !a.Equals(b);

        #endregion

        #region Constructors

        internal Color64(ulong argb)
            : this() // so the compiler does not complain about not initializing ARGB fields
        {
            value = argb;
        }

        internal Color64(ushort a, ushort r, ushort g, ushort b)
            : this() // so the compiler does not complain about not initializing value
        {
            B = b;
            G = g;
            R = r;
            A = a;
        }

        internal Color64(ushort r, ushort g, ushort b)
            : this() // so the compiler does not complain about not initializing value
        {
            B = b;
            G = g;
            R = r;
            A = UInt16.MaxValue;
        }

        internal Color64(Color32 c)
            : this() // so the compiler does not complain about not initializing value
        {
            B = (ushort)((c.B << 8) | c.B);
            G = (ushort)((c.G << 8) | c.G);
            R = (ushort)((c.R << 8) | c.R);
            A = (ushort)((c.A << 8) | c.A);
        }

        #endregion

        #region Methods

        #region Static Methods

        public static Color64 FromArgb32(int argb) => new Color64(Color32.FromArgb(argb));

        public static Color64 FromArgb(long argb) => new Color64((ulong)argb);

        public static Color64 FromArgb(ushort a, Color64 baseColor)
            => new Color64(((ulong)a << 48) | (baseColor.value & rgbMask));

        public static Color64 FromRgb32(int rgb) => new Color64(Color32.FromRgb(rgb));

        public static Color64 FromRgb(long rgb) => new Color64(alphaMask | (ulong)rgb);

        public static Color64 FromGray(ushort level) => new Color64(level, level, level);

        #endregion

        #region Instance Methods

        public Color32 ToColor32() => new Color32((byte)(A >> 8), (byte)(R >> 8), (byte)(G >> 8), (byte)(B >> 8));

        public long ToArgb() => (long)value;

        public int ToArgb32() => ToColor32().ToArgb();

        public long ToRgb() => (long)(~alphaMask & value);

        public int ToRgb32() => ToColor32().ToRgb();

        public bool Equals(Color64 other) => value == other.value;

        public override bool Equals(object? obj) => obj is Color64 other && Equals(other);

        public override int GetHashCode() => value.GetHashCode();

        public override string ToString() => $"{value:X16} [A={A}; R={R}; G={G}; B={B}]";

        #endregion

        #endregion
    }
}
