#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: Color64.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2024 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#if NETCOREAPP3_0_OR_GREATER
using System.Runtime.Intrinsics.X86;
using System.Runtime.Intrinsics;
#endif
using System.Security;

#endregion

namespace KGySoft.Drawing.Imaging
{
    /// <summary>
    /// Represents a 64-bit sRGB color where every color channel is represented by a 16-bit integer.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    [Serializable]
    public readonly struct Color64 : IEquatable<Color64>
    {
        #region Constants

        private const ulong alphaMask = 0xFFFF_0000_0000_0000;
        private const ulong rgbMask = 0x0000_FFFF_FFFF_FFFF;

        #endregion

        #region Fields

        #region Public Fields

        /// <summary>
        /// Gets the alpha component value of this <see cref="Color64"/> structure. This field is read-only.
        /// </summary>
        [FieldOffset(6)]
        [CLSCompliant(false)]
        [NonSerialized]
        public readonly ushort A;

        /// <summary>
        /// Gets the red component value of this <see cref="Color64"/> structure. This field is read-only.
        /// </summary>
        [FieldOffset(4)]
        [CLSCompliant(false)]
        [NonSerialized]
        public readonly ushort R;

        /// <summary>
        /// Gets the green component value of this <see cref="Color64"/> structure. This field is read-only.
        /// </summary>
        [FieldOffset(2)]
        [CLSCompliant(false)]
        [NonSerialized]
        public readonly ushort G;

        /// <summary>
        /// Gets the blue component value of this <see cref="Color64"/> structure. This field is read-only.
        /// </summary>
        [FieldOffset(0)]
        [CLSCompliant(false)]
        [NonSerialized]
        public readonly ushort B;

        #endregion

        #region Private Fields

        [FieldOffset(0)]
        private readonly ulong value;

        #endregion

        #endregion

        #region Properties

        internal ulong Value => value;

        #endregion

        #region Operators

        /// <summary>
        /// Gets whether two <see cref="Color64"/> structures are equal.
        /// </summary>
        /// <param name="left">The <see cref="Color64"/> instance that is to the left of the equality operator.</param>
        /// <param name="right">The <see cref="Color64"/> instance that is to the right of the equality operator.</param>
        /// <returns><see langword="true"/> if the two <see cref="Color64"/> structures are equal; otherwise, <see langword="false"/>.</returns>
        public static bool operator ==(Color64 left, Color64 right) => left.Equals(right);

        /// <summary>
        /// Gets whether two <see cref="Color64"/> structures are different.
        /// </summary>
        /// <param name="left">The <see cref="Color64"/> instance that is to the left of the inequality operator.</param>
        /// <param name="right">The <see cref="Color64"/> instance that is to the right of the inequality operator.</param>
        /// <returns><see langword="true"/> if the two <see cref="Color64"/> structures are different; otherwise, <see langword="false"/>.</returns>
        public static bool operator !=(Color64 left, Color64 right) => !left.Equals(right);

        #endregion

        #region Constructors

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Color64"/> struct from ARGB (alpha, red, green, and blue) values.
        /// </summary>
        /// <param name="a">The alpha component.</param>
        /// <param name="r">The red component.</param>
        /// <param name="g">The green component.</param>
        /// <param name="b">The blue component.</param>
        [CLSCompliant(false)]
        public Color64(ushort a, ushort r, ushort g, ushort b)
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
        /// Initializes a new instance of the <see cref="Color64"/> struct from RGB (red, green, and blue) values.
        /// </summary>
        /// <param name="r">The red component.</param>
        /// <param name="g">The green component.</param>
        /// <param name="b">The blue component.</param>
        [CLSCompliant(false)]
        public Color64(ushort r, ushort g, ushort b)
            : this(UInt16.MaxValue, r, g, b)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Color64"/> struct from a <see cref="Color32"/> instance.
        /// </summary>
        /// <param name="c">A <see cref="Color32"/> structure to initialize a new instance of <see cref="Color64"/> from.</param>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public Color64(Color32 c)
#if !NET5_0_OR_GREATER
            : this() // so the compiler does not complain about not initializing value
#endif
        {
#if NET5_0_OR_GREATER
            Unsafe.SkipInit(out this);
#endif
#if NETCOREAPP3_0_OR_GREATER
            // Using vectorization if possible. Fortunately multiplication is supported directly on ushort vectors,
            // though we need to use Vector128 instead of Vector64 for Sse2.MultiplyLow, whose upper half will be ignored.
            if (Sse2.IsSupported)
            {
                // Widening the bytes to ushorts
                Vector128<ushort> bgraU16 = Sse41.IsSupported
                    // Reinterpreting the uint value as bytes and converting them to ushorts in one step is still faster than converting them separately
                    ? Sse41.ConvertToVector128Int16(Vector128.CreateScalarUnsafe(c.Value).AsByte()).AsUInt16()
                    // Cannot do the conversion in one step. Ignoring the upper half of the vector.
                    : Vector128.Create((ushort)c.B, c.G, c.R, c.A, 0, 0, 0, 0);

                // Multiplying the color channels by 257, which is the same as (ushort)((x << 8) | x)
                bgraU16 = Sse2.MultiplyLow(bgraU16, Vector128.Create((ushort)257));

                // Initializing from the lower half 64 bits in a single step.
                value = bgraU16.AsUInt64().ToScalar();
                return;
            }
#endif

            B = ColorSpaceHelper.ToUInt16(c.B);
            G = ColorSpaceHelper.ToUInt16(c.G);
            R = ColorSpaceHelper.ToUInt16(c.R);
            A = ColorSpaceHelper.ToUInt16(c.A);
        }

        #endregion

        #region Internal Constructors

        internal Color64(ulong argb)
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
        /// Creates a <see cref="Color64"/> structure from a 64-bit ARGB value.
        /// </summary>
        /// <param name="argb">A value specifying the 64-bit ARGB value. As a hex value it can be specified as <c>0xAAAA_RRRR_GGGG_BBBB</c>.
        /// where <c>AAAA</c> is the highest word and <c>BBBB</c> is the lowest word.</param>
        /// <returns>A <see cref="Color64"/> structure from the specified 64-bit ARGB value.</returns>
        public static Color64 FromArgb(long argb) => new Color64((ulong)argb);

        /// <summary>
        /// Creates a <see cref="Color64"/> structure from a 64-bit ARGB value.
        /// </summary>
        /// <param name="argb">A value specifying the 64-bit ARGB value. As a hex value it can be specified as <c>0xAAAA_RRRR_GGGG_BBBB</c>
        /// where <c>AAAA</c> is the highest word and <c>BBBB</c> is the lowest word.</param>
        /// <returns>A <see cref="Color64"/> structure from the specified 64-bit ARGB value.</returns>
        [CLSCompliant(false)]
        public static Color64 FromArgb(ulong argb) => new Color64(argb);

        /// <summary>
        /// Creates a <see cref="Color64"/> instance from the specified <see cref="Color64"/> structure, but with the new specified alpha value.
        /// </summary>
        /// <param name="a">The alpha value for the new <see cref="Color64"/> instance.</param>
        /// <param name="baseColor">The <see cref="Color64"/> instance from which to create the new one.</param>
        /// <returns>A <see cref="Color64"/> instance from the specified <see cref="Color64"/> structure and alpha value.</returns>
        [CLSCompliant(false)]
        public static Color64 FromArgb(ushort a, Color64 baseColor)
            => new Color64(((ulong)a << 48) | (baseColor.value & rgbMask));

        /// <summary>
        /// Creates a <see cref="Color64"/> structure from a 48-bit RGB value. The highest two bytes of the specified integer are ignored
        /// and the <see cref="A">A</see> property of the result will be 65535.
        /// </summary>
        /// <param name="rgb">A value specifying the 48-bit RGB value. As a hex value it can be specified as <c>0xRRRR_GGGG_BBBB</c>.
        /// The possibly nonzero alpha component will be ignored.</param>
        /// <returns>A <see cref="Color64"/> structure from a 48-bit RGB value.</returns>
        public static Color64 FromRgb(long rgb) => new Color64(alphaMask | (ulong)rgb);

        /// <summary>
        /// Creates a <see cref="Color64"/> structure from a 48-bit RGB value. The highest two bytes of the specified integer are ignored
        /// and the <see cref="A">A</see> property of the result will be 65535.
        /// </summary>
        /// <param name="rgb">A value specifying the 48-bit RGB value. As a hex value it can be specified as <c>0xRRRR_GGGG_BBBB</c>.
        /// The possibly nonzero alpha component will be ignored.</param>
        /// <returns>A <see cref="Color64"/> structure from a 48-bit RGB value.</returns>
        [CLSCompliant(false)]
        public static Color64 FromRgb(ulong rgb) => new Color64(alphaMask | rgb);

        /// <summary>
        /// Creates a <see cref="Color64"/> structure representing a grayscale color of the specified <paramref name="brightness"/>.
        /// </summary>
        /// <param name="brightness">The brightness of the gray color to be created where 0 represents the black color and 65535 represents the white color.</param>
        /// <returns>A <see cref="Color64"/> structure representing a grayscale color of the specified <paramref name="brightness"/>.</returns>
        [CLSCompliant(false)]
        public static Color64 FromGray(ushort brightness) => new Color64(brightness, brightness, brightness);

        #endregion

        #region Instance Methods

        /// <summary>
        /// Converts this <see cref="Color64"/> instance to a <see cref="Color32"/> structure.
        /// </summary>
        /// <returns>A <see cref="Color32"/> structure converted from this <see cref="Color64"/> instance.</returns>
        [SecuritySafeCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public unsafe Color32 ToColor32()
        {
            // Bit-shifting every component to the right by 8 bits is the same as getting the high bytes of each components.
            // The most elegant solution would be using Vector64<byte> or ReadOnlySpan<byte> to wrap value but they are still slower than this:
            ulong bgraU16 = value;
            byte* bytes = (byte*)&bgraU16;
            return new Color32(bytes[7], bytes[5], bytes[3], bytes[1]);

            // Here there is no need for the fallback version as the default one works everywhere
            //return new Color32(ColorSpaceHelper.ToByte(A),
            //    ColorSpaceHelper.ToByte(R),
            //    ColorSpaceHelper.ToByte(G),
            //    ColorSpaceHelper.ToByte(B));
        }

        /// <summary>
        /// Gets the 64-bit ARGB value of this <see cref="Color64"/> instance.
        /// </summary>
        /// <returns>The 64-bit ARGB value of this <see cref="Color64"/> instance</returns>
        public long ToArgb() => (long)value;

        /// <summary>
        /// Gets the 64-bit ARGB value of this <see cref="Color64"/> instance.
        /// </summary>
        /// <returns>The 64-bit ARGB value of this <see cref="Color64"/> instance</returns>
        [CLSCompliant(false)]
        public ulong ToArgbUInt64() => value;

        /// <summary>
        /// Gets the 48-bit RGB value of this <see cref="Color64"/> instance. The highest two of bytes of the returned integer are zero.
        /// </summary>
        /// <returns>The 48-bit RGB value of this <see cref="Color64"/> instance. The highest two of bytes of the returned integer are zero.</returns>
        public long ToRgb() => (long)(~alphaMask & value);

        /// <summary>
        /// Gets the 48-bit RGB value of this <see cref="Color64"/> instance. The highest two of bytes of the returned integer are zero.
        /// </summary>
        /// <returns>The 48-bit RGB value of this <see cref="Color64"/> instance. The highest two of bytes of the returned integer are zero.</returns>
        [CLSCompliant(false)]
        public ulong ToRgbUInt64() => ~alphaMask & value;

        /// <summary>
        /// Gets a <see cref="Color64"/> instance that represents the matching gray shade of this <see cref="Color64"/> instance based on human perception.
        /// </summary>
        /// <returns>A <see cref="Color64"/> instance that represents the matching gray shade of this <see cref="Color64"/> instance based on human perception.</returns>
        public Color64 ToGray()
        {
            ushort br = this.GetBrightness();
            return new Color64(A, br, br, br);
        }

        /// <summary>
        /// Gets a <see cref="Color64"/> instance that represents this <see cref="Color64"/> without alpha (transparency).
        /// </summary>
        /// <returns>A <see cref="Color64"/> instance that represents this <see cref="Color64"/> without alpha.</returns>
        public Color64 ToOpaque() => A == UInt16.MaxValue ? this : new Color64(UInt16.MaxValue, R, G, B);

        /// <summary>
        /// Determines whether the current <see cref="Color64"/> instance is equal to another one.
        /// </summary>
        /// <param name="other">A <see cref="Color64"/> structure to compare with this <see cref="Color64"/> instance.</param>
        /// <returns><see langword="true"/>, if the current <see cref="Color64"/> instance is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.</returns>
        public bool Equals(Color64 other) => value == other.value;

        /// <summary>
        /// Determines whether the specified <see cref="object"/> is equal to this <see cref="Color64"/> instance.
        /// </summary>
        /// <param name="obj">The <see cref="object" /> to compare with this <see cref="Color64"/> instance.</param>
        /// <returns><see langword="true"/>, if the current <see cref="Color64"/> instance is equal to the <paramref name="obj" /> parameter; otherwise, <see langword="false" />.</returns>
        public override bool Equals(object? obj) => obj is Color64 other && Equals(other);

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public override int GetHashCode() => value.GetHashCode();

        /// <summary>
        /// Gets the string representation of this <see cref="Color64"/> instance.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents this <see cref="Color64"/> instance.</returns>
        public override string ToString() => $"{value:X16} [A={A}; R={R}; G={G}; B={B}]";

        #endregion

        #endregion
    }
}
