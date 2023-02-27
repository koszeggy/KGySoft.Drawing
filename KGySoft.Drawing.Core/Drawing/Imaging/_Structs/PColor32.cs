#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: PColor32.cs
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
using System.Diagnostics.CodeAnalysis;
#if NET5_0_OR_GREATER
using System.Runtime.CompilerServices;
#endif
using System.Runtime.InteropServices;

#endregion

namespace KGySoft.Drawing.Imaging
{
    /// <summary>
    /// Represents a 32-bit premultiplied sRGB color.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    [Serializable]
    public readonly struct PColor32 : IEquatable<PColor32>
    {
        #region Fields

        #region Public Fields

        /// <summary>
        /// Gets the alpha component value of this <see cref="PColor32"/> structure.
        /// </summary>
        [FieldOffset(3)]
        [NonSerialized]
        public readonly byte A;

        /// <summary>
        /// Gets the red component value of this <see cref="PColor32"/> structure.
        /// </summary>
        [FieldOffset(2)]
        [NonSerialized]
        public readonly byte R;

        /// <summary>
        /// Gets the green component value of this <see cref="PColor32"/> structure.
        /// </summary>
        [FieldOffset(1)]
        [NonSerialized]
        public readonly byte G;

        /// <summary>
        /// Gets the blue component value of this <see cref="PColor32"/> structure.
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

        #region Properties

        #region Public Properties
        
        /// <summary>
        /// Gets whether this <see cref="PColor32"/> instance represents a valid premultiplied color.
        /// That is, when <see cref="A"/> is greater than or equal to <see cref="R"/>, <see cref="G"/> and <see cref="B"/>.
        /// </summary>
        public bool IsValid => A >= R && A >= G && A >= B;

        #endregion

        #region Internal Properties

        internal uint Value => value;

        #endregion

        #endregion

        #region Operators

        /// <summary>
        /// Gets whether two <see cref="PColor32"/> structures are equal.
        /// </summary>
        /// <param name="left">The <see cref="PColor32"/> instance that is to the left of the equality operator.</param>
        /// <param name="right">The <see cref="PColor32"/> instance that is to the right of the equality operator.</param>
        /// <returns><see langword="true"/> if the two <see cref="PColor32"/> structures are equal; otherwise, <see langword="false"/>.</returns>
        public static bool operator ==(PColor32 left, PColor32 right) => left.Equals(right);

        /// <summary>
        /// Gets whether two <see cref="PColor32"/> structures are different.
        /// </summary>
        /// <param name="left">The <see cref="PColor32"/> instance that is to the left of the inequality operator.</param>
        /// <param name="right">The <see cref="PColor32"/> instance that is to the right of the inequality operator.</param>
        /// <returns><see langword="true"/> if the two <see cref="PColor32"/> structures are different; otherwise, <see langword="false"/>.</returns>
        public static bool operator !=(PColor32 left, PColor32 right) => !left.Equals(right);

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PColor32"/> struct from ARGB (alpha, red, green, and blue) values.
        /// For performance reasons this overload does not validate the parameters but you can use the <see cref="PColor32(byte, byte, byte, byte, bool)"/>,
        /// the <see cref="IsValid"/> property for validation or the <see cref="Clip">Clip</see> method to return a valid instance.
        /// </summary>
        /// <param name="a">The alpha component.</param>
        /// <param name="r">The red component.</param>
        /// <param name="g">The green component.</param>
        /// <param name="b">The blue component.</param>
        public PColor32(byte a, byte r, byte g, byte b)
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
        /// Initializes a new instance of the <see cref="PColor32"/> struct from ARGB (alpha, red, green, and blue) values.
        /// </summary>
        /// <param name="a">The alpha component.</param>
        /// <param name="r">The red component.</param>
        /// <param name="g">The green component.</param>
        /// <param name="b">The blue component.</param>
        /// <param name="validateRgb"><see langword="true"/> to validate the parameters; <see langword="false"/> to skip the validation.</param>
        public PColor32(byte a, byte r, byte g, byte b, bool validateRgb)
            : this(a, r, g, b)
        {
            if (validateRgb && (r > a || g > a || b > a))
                ThrowInvalid();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PColor32"/> struct from RGB (red, green, and blue) values.
        /// </summary>
        /// <param name="r">The red component.</param>
        /// <param name="g">The green component.</param>
        /// <param name="b">The blue component.</param>
        public PColor32(byte r, byte g, byte b)
            : this(Byte.MaxValue, r, g, b)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PColor32"/> struct from a <see cref="Color32"/> instance.
        /// </summary>
        /// <param name="c">A <see cref="Color32"/> structure to initialize a new instance of <see cref="PColor32"/> from.</param>
        public PColor32(Color32 c)
#if !NET5_0_OR_GREATER
            : this() // so the compiler does not complain about not initializing value
#endif
        {
#if NET5_0_OR_GREATER
            Unsafe.SkipInit(out this);
#endif
            switch (c.A)
            {
                case Byte.MaxValue:
                    value = c.Value;
                    break;
                case Byte.MinValue:
                    value = 0u;
                    break;
                default:
                    A = c.A;
                    R = (byte)(c.R * c.A / Byte.MaxValue);
                    G = (byte)(c.G * c.A / Byte.MaxValue);
                    B = (byte)(c.B * c.A / Byte.MaxValue);
                    break;
            }
        }

        #endregion

        #region Methods

        #region Static Methods

        [MethodImpl(MethodImplOptions.NoInlining)]
        [SuppressMessage("ReSharper", "NotResolvedInText", Justification = "Parameter of the caller method")]
        private static void ThrowInvalid() => throw new ArgumentOutOfRangeException("a", Res.ImagingInvalidPremultipliedValues);

        #endregion

        #region Instance Methods

        /// <summary>
        /// Returns a valid <see cref="PColor32"/> instance by clipping the possibly exceeding original RGB values.
        /// If <see cref="IsValid"/> returns <see langword="true"/>, then the result is the same as the original instance.
        /// </summary>
        /// <returns>A valid <see cref="PColor32"/> instance by clipping the possibly exceeding original RGB values.</returns>
        public PColor32 Clip() => new PColor32(A,
            Math.Min(A, R),
            Math.Min(A, G),
            Math.Min(A, B));

        /// <summary>
        /// Converts this <see cref="PColor32"/> instance to a <see cref="Color32"/> structure.
        /// </summary>
        /// <returns>A <see cref="Color32"/> structure converted from this <see cref="PColor32"/> instance.</returns>
        public Color32 ToColor32() => A switch
        {
            Byte.MaxValue => new Color32(value),
            Byte.MinValue => default,
            _ => new Color32(A, (byte)(R * Byte.MaxValue / A), (byte)(G * Byte.MaxValue / A), (byte)(B * Byte.MaxValue / A))
        };

        /// <summary>
        /// Determines whether the current <see cref="PColor32"/> instance is equal to another one.
        /// </summary>
        /// <param name="other">A <see cref="PColor32"/> structure to compare with this <see cref="PColor32"/> instance.</param>
        /// <returns><see langword="true"/>, if the current <see cref="PColor32"/> instance is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.</returns>
        public bool Equals(PColor32 other) => value == other.value;

        /// <summary>
        /// Determines whether the specified <see cref="object"/> is equal to this <see cref="PColor32"/> instance.
        /// </summary>
        /// <param name="obj">The <see cref="object" /> to compare with this <see cref="PColor32"/> instance.</param>
        /// <returns><see langword="true"/>, if the current <see cref="PColor32"/> instance is equal to the <paramref name="obj" /> parameter; otherwise, <see langword="false" />.</returns>
        public override bool Equals(object? obj) => obj is PColor32 other && Equals(other);

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public override int GetHashCode() => (int)value;

        /// <summary>
        /// Gets the string representation of this <see cref="PColor32"/> instance.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents this <see cref="PColor32"/> instance.</returns>
        public override string ToString() => $"{value:X8} [A={A}; R={R}; G={G}; B={B}]";

        #endregion

        #endregion
    }
}
