#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: Color32.cs
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
    /// <summary>
    /// Represents the same 32-bit ARGB color as the <see cref="Color"/> structure does but in a more optimized way
    /// for better performance and smaller memory consumption.
    /// </summary>
    // Comment TODO:
    // - Implicit conversion from and to Color exists.
    //   Note: Does not contain known color properties. Just use Color32 c = Color.Blue, for example.
    [StructLayout(LayoutKind.Explicit)]
    public readonly struct Color32 : IEquatable<Color32>
    {
        #region Constants

        private const uint alphaMask = 0xFF_00_00_00;
        private const uint rgbMask = 0x00_FF_FF_FF;

        #endregion

        #region Fields

        #region Static Fields

        internal static readonly Color32 Black = FromGray(Byte.MinValue);
        internal static readonly Color32 White = FromGray(Byte.MaxValue);
        internal static readonly Color32 Transparent = new Color32(Color.Transparent);

        #endregion

        #region Instance Fields

        #region Public Fields

        [FieldOffset(3)]
        public readonly byte A;

        [FieldOffset(2)]
        public readonly byte R;

        [FieldOffset(1)]
        public readonly byte G;

        [FieldOffset(0)]
        public readonly byte B;

        #endregion

        #region Private Fields

        [FieldOffset(0)]
        private readonly uint value;

        #endregion

        #endregion

        #endregion

        #region Operators

        public static bool operator ==(Color32 a, Color32 b) => a.Equals(b);

        public static bool operator !=(Color32 a, Color32 b) => !a.Equals(b);

        #endregion

        #region Constructors

        #region Public Constructors

        public Color32(byte a, byte r, byte g, byte b)
            : this() // so the compiler does not complain about not initializing value
        {
            B = b;
            G = g;
            R = r;
            A = a;
        }

        public Color32(byte r, byte g, byte b)
            : this(Byte.MaxValue, r, g, b)
        {
        }

        public Color32(Color c) : this((uint)c.ToArgb()) { }

        #endregion

        #region Internal Constructors

        internal Color32(uint argb)
            : this() // so the compiler does not complain about not initializing ARGB fields
        {
            value = argb;
        }

        #endregion

        #endregion

        #region Methods

        #region Static Methods

        public static Color32 FromArgb(int argb) => new Color32((uint)argb);

        public static Color32 FromArgb(byte a, Color32 baseColor)
            => new Color32(((uint)a << 24) | (baseColor.value & rgbMask));

        public static Color32 FromRgb(int rgb) => new Color32(alphaMask | (uint)rgb);

        public static Color32 FromGray(byte level) => new Color32(level, level, level);

        #endregion

        #region Instance Methods

        public Color ToColor() => Color.FromArgb(ToArgb());

        public int ToArgb() => (int)value;

        public int ToRgb() => (int)(~alphaMask & value);

        public Color32 ToGray() => FromArgb(A, FromGray(this.GetBrightness()));

        public bool Equals(Color32 other) => value == other.value;

        public override bool Equals(object obj) => obj is Color32 other && Equals(other);

        public override int GetHashCode() => (int)value;

        public override string ToString() => $"{value:X8} [A={A}; R={R}; G={G}; B={B}]";

        #endregion

        #endregion
    }
}
