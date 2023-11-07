#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: Color32.cs
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
using System.Drawing;
#if NET5_0_OR_GREATER
using System.Runtime.CompilerServices;
#endif
using System.Runtime.InteropServices;

#endregion

namespace KGySoft.Drawing.Imaging
{
    /// <summary>
    /// Represents a 32-bit sRGB color where every color channel is represented by a 8-bit integer.
    /// It represents the same color space as the <see cref="Color"/> structure does but in a more optimized way
    /// for better performance and smaller memory consumption.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    [Serializable]
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

        #endregion

        #region Instance Fields

        #region Public Fields

        /// <summary>
        /// Gets the alpha component value of this <see cref="Color32"/> structure. This field is read-only.
        /// </summary>
        [FieldOffset(3)]
        [NonSerialized]
        public readonly byte A;

        /// <summary>
        /// Gets the red component value of this <see cref="Color32"/> structure. This field is read-only.
        /// </summary>
        [FieldOffset(2)]
        [NonSerialized]
        public readonly byte R;

        /// <summary>
        /// Gets the green component value of this <see cref="Color32"/> structure. This field is read-only.
        /// </summary>
        [FieldOffset(1)]
        [NonSerialized]
        public readonly byte G;

        /// <summary>
        /// Gets the blue component value of this <see cref="Color32"/> structure. This field is read-only.
        /// </summary>
        [FieldOffset(0)]
        [NonSerialized]
        public readonly byte B;

        #endregion

        #region Private Fields

        [FieldOffset(0)]
        private readonly uint value;

        #endregion

        #endregion

        #endregion

        #region Properties

        internal uint Value => value;

        #endregion

        #region Operators

        /// <summary>
        /// Gets whether two <see cref="Color32"/> structures are equal.
        /// </summary>
        /// <param name="left">The <see cref="Color32"/> instance that is to the left of the equality operator.</param>
        /// <param name="right">The <see cref="Color32"/> instance that is to the right of the equality operator.</param>
        /// <returns><see langword="true"/> if the two <see cref="Color32"/> structures are equal; otherwise, <see langword="false"/>.</returns>
        public static bool operator ==(Color32 left, Color32 right) => left.Equals(right);

        /// <summary>
        /// Gets whether two <see cref="Color32"/> structures are different.
        /// </summary>
        /// <param name="left">The <see cref="Color32"/> instance that is to the left of the inequality operator.</param>
        /// <param name="right">The <see cref="Color32"/> instance that is to the right of the inequality operator.</param>
        /// <returns><see langword="true"/> if the two <see cref="Color32"/> structures are different; otherwise, <see langword="false"/>.</returns>
        public static bool operator !=(Color32 left, Color32 right) => !left.Equals(right);

        /// <summary>
        /// Performs an implicit conversion from <see cref="Color"/> to <see cref="Color32"/>.
        /// </summary>
        /// <param name="color">A <see cref="Color"/> instance to convert to <see cref="Color32"/>.</param>
        /// <returns>A <see cref="Color32"/> instance representing the specified <paramref name="color"/>.</returns>
        public static implicit operator Color32(Color color) => new Color32(color);

        /// <summary>
        /// Performs an implicit conversion from <see cref="Color32"/> to <see cref="Color"/>.
        /// </summary>
        /// <param name="color">A <see cref="Color32"/> instance to convert to <see cref="Color"/>.</param>
        /// <returns>A <see cref="Color"/> instance representing the specified <paramref name="color"/>.</returns>
        public static implicit operator Color(Color32 color) => color.ToColor();

        #endregion

        #region Constructors

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Color32"/> struct from ARGB (alpha, red, green, and blue) values.
        /// </summary>
        /// <param name="a">The alpha component.</param>
        /// <param name="r">The red component.</param>
        /// <param name="g">The green component.</param>
        /// <param name="b">The blue component.</param>
        public Color32(byte a, byte r, byte g, byte b)
#if !NET5_0_OR_GREATER
            : this() // so the compiler does not complain about not initializing value
#endif
        {
#if NET5_0_OR_GREATER
            Unsafe.SkipInit(out this);
#endif
            B = b;
            G = g;
            R = r;
            A = a;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Color32"/> struct from RGB (red, green, and blue) values.
        /// </summary>
        /// <param name="r">The red component.</param>
        /// <param name="g">The green component.</param>
        /// <param name="b">The blue component.</param>
        public Color32(byte r, byte g, byte b)
            : this(Byte.MaxValue, r, g, b)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Color32"/> struct from a <see cref="Color"/> instance.
        /// </summary>
        /// <param name="c">A <see cref="Color"/> structure to initialize a new instance of <see cref="Color32"/> from.</param>
        public Color32(Color c) : this((uint)c.ToArgb()) { }

        #endregion

        #region Internal Constructors

        internal Color32(uint argb)
#if !NET5_0_OR_GREATER
            : this() // so the compiler does not complain about not initializing value
#endif
        {
#if NET5_0_OR_GREATER
            Unsafe.SkipInit(out this);
#endif
            value = argb;
        }

        #endregion

        #endregion

        #region Methods

        #region Static Methods

        /// <summary>
        /// Creates a <see cref="Color32"/> structure from a 32-bit ARGB value.
        /// </summary>
        /// <param name="argb">A value specifying the 32-bit ARGB value. As a hex value it can be specified as <c>0xAA_RR_GG_BB</c> where <c>AA</c>
        /// is the most significant byte (MSB) and <c>BB</c> is the least significant byte (LSB).</param>
        /// <returns>A <see cref="Color32"/> structure from the specified 32-bit ARGB value.</returns>
        public static Color32 FromArgb(int argb) => new Color32((uint)argb);

        /// <summary>
        /// Creates a <see cref="Color32"/> structure from a 32-bit ARGB value.
        /// </summary>
        /// <param name="argb">A value specifying the 32-bit ARGB value. As a hex value it can be specified as <c>0xAA_RR_GG_BB</c> where <c>AA</c>
        /// is the most significant byte (MSB) and <c>BB</c> is the least significant byte (LSB).</param>
        /// <returns>A <see cref="Color32"/> structure from the specified 32-bit ARGB value.</returns>
        [CLSCompliant(false)]
        public static Color32 FromArgb(uint argb) => new Color32(argb);

        /// <summary>
        /// Creates a <see cref="Color32"/> instance from the specified <see cref="Color32"/> structure, but with the new specified alpha value.
        /// </summary>
        /// <param name="a">The alpha value for the new <see cref="Color32"/> instance.</param>
        /// <param name="baseColor">The <see cref="Color32"/> instance from which to create the new one.</param>
        /// <returns>A <see cref="Color32"/> instance from the specified <see cref="Color32"/> structure and alpha value.</returns>
        public static Color32 FromArgb(byte a, Color32 baseColor)
            => new Color32(((uint)a << 24) | (baseColor.value & rgbMask));

        /// <summary>
        /// Creates a <see cref="Color32"/> structure from a 24-bit RGB value. The highest byte of the specified integer is ignored
        /// and the <see cref="A">A</see> property of the result will be 255.
        /// </summary>
        /// <param name="rgb">A value specifying the 24-bit RGB value. As a hex value it can be specified as <c>0xRR_GG_BB</c>.
        /// The possibly nonzero alpha component will be ignored.</param>
        /// <returns>A <see cref="Color32"/> structure from a 24-bit RGB value.</returns>
        public static Color32 FromRgb(int rgb) => new Color32(alphaMask | (uint)rgb);

        /// <summary>
        /// Creates a <see cref="Color32"/> structure from a 24-bit RGB value. The highest byte of the specified integer is ignored
        /// and the <see cref="A">A</see> property of the result will be 255.
        /// </summary>
        /// <param name="rgb">A value specifying the 24-bit RGB value. As a hex value it can be specified as <c>0xRR_GG_BB</c>.
        /// The possibly nonzero alpha component will be ignored.</param>
        /// <returns>A <see cref="Color32"/> structure from a 24-bit RGB value.</returns>
        [CLSCompliant(false)]
        public static Color32 FromRgb(uint rgb) => new Color32(alphaMask | rgb);

        /// <summary>
        /// Creates a <see cref="Color32"/> structure representing a grayscale color of the specified <paramref name="brightness"/>.
        /// </summary>
        /// <param name="brightness">The brightness of the gray color to be created where 0 represents the black color and 255 represents the white color.</param>
        /// <returns>A <see cref="Color32"/> structure representing a grayscale color of the specified <paramref name="brightness"/>.</returns>
        public static Color32 FromGray(byte brightness) => new Color32(brightness, brightness, brightness);

        #endregion

        #region Instance Methods

        /// <summary>
        /// Converts this <see cref="Color32"/> instance to a <see cref="Color"/> structure.
        /// </summary>
        /// <returns>A <see cref="Color"/> structure converted from this <see cref="Color32"/> instance.</returns>
        public Color ToColor() => Color.FromArgb(ToArgb());

        /// <summary>
        /// Gets the 32-bit ARGB value of this <see cref="Color32"/> instance.
        /// </summary>
        /// <returns>The 32-bit ARGB value of this <see cref="Color32"/> instance</returns>
        public int ToArgb() => (int)value;

        /// <summary>
        /// Gets the 32-bit ARGB value of this <see cref="Color32"/> instance.
        /// </summary>
        /// <returns>The 32-bit ARGB value of this <see cref="Color32"/> instance</returns>
        [CLSCompliant(false)]
        public uint ToArgbUInt32() => value;

        /// <summary>
        /// Gets the 24-bit RGB value of this <see cref="Color32"/> instance. The most significant byte of the returned integer is zero.
        /// </summary>
        /// <returns>The 24-bit RGB value of this <see cref="Color32"/> instance. The most significant byte of the returned integer is zero.</returns>
        public int ToRgb() => (int)(~alphaMask & value);

        /// <summary>
        /// Gets the 24-bit RGB value of this <see cref="Color32"/> instance. The most significant byte of the returned integer is zero.
        /// </summary>
        /// <returns>The 24-bit RGB value of this <see cref="Color32"/> instance. The most significant byte of the returned integer is zero.</returns>
        [CLSCompliant(false)]
        public uint ToRgbUInt32() => ~alphaMask & value;

        /// <summary>
        /// Gets a <see cref="Color32"/> instance that represents the matching gray shade of this <see cref="Color32"/> instance based on human perception.
        /// </summary>
        /// <returns>A <see cref="Color32"/> instance that represents the matching gray shade of this <see cref="Color32"/> instance based on human perception.</returns>
        public Color32 ToGray()
        {
            byte br = this.GetBrightness();
            return new Color32(A, br, br, br);
        }

        /// <summary>
        /// Gets a <see cref="Color32"/> instance that represents this <see cref="Color32"/> without alpha (transparency).
        /// </summary>
        /// <returns>A <see cref="Color32"/> instance that represents this <see cref="Color32"/> without alpha.</returns>
        public Color32 ToOpaque() => A == Byte.MaxValue ? this : new Color32(Byte.MaxValue, R, G, B);

        /// <summary>
        /// Determines whether the current <see cref="Color32"/> instance is equal to another one.
        /// </summary>
        /// <param name="other">A <see cref="Color32"/> structure to compare with this <see cref="Color32"/> instance.</param>
        /// <returns><see langword="true"/>, if the current <see cref="Color32"/> instance is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.</returns>
        public bool Equals(Color32 other) => value == other.value;

        /// <summary>
        /// Determines whether the specified <see cref="object"/> is equal to this <see cref="Color32"/> instance.
        /// </summary>
        /// <param name="obj">The <see cref="object" /> to compare with this <see cref="Color32"/> instance.</param>
        /// <returns><see langword="true"/>, if the current <see cref="Color32"/> instance is equal to the <paramref name="obj" /> parameter; otherwise, <see langword="false" />.</returns>
        public override bool Equals(object? obj) => obj is Color32 other && Equals(other);

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public override int GetHashCode() => (int)value;

        /// <summary>
        /// Gets the string representation of this <see cref="Color32"/> instance.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents this <see cref="Color32"/> instance.</returns>
        public override string ToString() => $"{value:X8} [A={A}; R={R}; G={G}; B={B}]";

        #endregion

        #endregion
    }
}
