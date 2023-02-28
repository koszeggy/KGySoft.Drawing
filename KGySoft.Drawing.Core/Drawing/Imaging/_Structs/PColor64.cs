#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: PColor64.cs
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
    /// Represents a 64-bit premultiplied sRGB color where every color channel is represented by a 16-bit integer.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    [Serializable]
    public readonly struct PColor64 : IEquatable<PColor64>
    {
        #region Fields

        #region Public Fields

        /// <summary>
        /// Gets the alpha component value of this <see cref="PColor64"/> structure. This field is read-only.
        /// </summary>
        [FieldOffset(6)]
        [CLSCompliant(false)]
        [NonSerialized]
        public readonly ushort A;

        /// <summary>
        /// Gets the red component value of this <see cref="PColor64"/> structure. This field is read-only.
        /// </summary>
        [FieldOffset(4)]
        [CLSCompliant(false)]
        [NonSerialized]
        public readonly ushort R;

        /// <summary>
        /// Gets the green component value of this <see cref="PColor64"/> structure. This field is read-only.
        /// </summary>
        [FieldOffset(2)]
        [CLSCompliant(false)]
        [NonSerialized]
        public readonly ushort G;

        /// <summary>
        /// Gets the blue component value of this <see cref="PColor64"/> structure. This field is read-only.
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

        #region Public Properties

        /// <summary>
        /// Gets whether this <see cref="PColor64"/> instance represents a valid premultiplied color.
        /// That is, when <see cref="A"/> is greater than or equal to <see cref="R"/>, <see cref="G"/> and <see cref="B"/>.
        /// </summary>
        public bool IsValid => A >= R && A >= G && A >= B;

        #endregion

        #region Internal Properties

        internal ulong Value => value;

        #endregion

        #endregion

        #region Operators

        /// <summary>
        /// Gets whether two <see cref="PColor64"/> structures are equal.
        /// </summary>
        /// <param name="left">The <see cref="PColor64"/> instance that is to the left of the equality operator.</param>
        /// <param name="right">The <see cref="PColor64"/> instance that is to the right of the equality operator.</param>
        /// <returns><see langword="true"/> if the two <see cref="PColor64"/> structures are equal; otherwise, <see langword="false"/>.</returns>
        public static bool operator ==(PColor64 left, PColor64 right) => left.Equals(right);

        /// <summary>
        /// Gets whether two <see cref="PColor64"/> structures are different.
        /// </summary>
        /// <param name="left">The <see cref="PColor64"/> instance that is to the left of the inequality operator.</param>
        /// <param name="right">The <see cref="PColor64"/> instance that is to the right of the inequality operator.</param>
        /// <returns><see langword="true"/> if the two <see cref="PColor64"/> structures are different; otherwise, <see langword="false"/>.</returns>
        public static bool operator !=(PColor64 left, PColor64 right) => !left.Equals(right);

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PColor64"/> struct from ARGB (alpha, red, green, and blue) values.
        /// For performance reasons this overload does not validate the parameters but you can use the <see cref="PColor64(ushort, ushort, ushort, ushort, bool)"/> constructor,
        /// the <see cref="IsValid"/> property for validation or the <see cref="Clip">Clip</see> method to return a valid instance.
        /// </summary>
        /// <param name="a">The alpha component.</param>
        /// <param name="r">The red component.</param>
        /// <param name="g">The green component.</param>
        /// <param name="b">The blue component.</param>
        [CLSCompliant(false)]
        public PColor64(ushort a, ushort r, ushort g, ushort b)
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
        /// Initializes a new instance of the <see cref="PColor64"/> struct from ARGB (alpha, red, green, and blue) values.
        /// </summary>
        /// <param name="a">The alpha component.</param>
        /// <param name="r">The red component.</param>
        /// <param name="g">The green component.</param>
        /// <param name="b">The blue component.</param>
        /// <param name="validate"><see langword="true"/> to validate the parameters; <see langword="false"/> to skip the validation.</param>
        [CLSCompliant(false)]
        public PColor64(ushort a, ushort r, ushort g, ushort b, bool validate)
            : this(a, r, g, b)
        {
            if (validate && (r > a || g > a || b > a))
                ThrowInvalid();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PColor64"/> struct from RGB (red, green, and blue) values.
        /// </summary>
        /// <param name="r">The red component.</param>
        /// <param name="g">The green component.</param>
        /// <param name="b">The blue component.</param>
        [CLSCompliant(false)]
        public PColor64(ushort r, ushort g, ushort b)
            : this(UInt16.MaxValue, r, g, b)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PColor64"/> struct from a <see cref="Color64"/> instance.
        /// </summary>
        /// <param name="c">A <see cref="Color64"/> structure to initialize a new instance of <see cref="PColor64"/> from.</param>
        public PColor64(Color64 c)
#if !NET5_0_OR_GREATER
            : this() // so the compiler does not complain about not initializing value
#endif
        {
#if NET5_0_OR_GREATER
            Unsafe.SkipInit(out this);
#endif
            switch (c.A)
            {
                case UInt16.MaxValue:
                    value = c.Value;
                    break;
                case UInt16.MinValue:
                    value = 0ul;
                    break;
                default:
                    A = c.A;
                    R = (ushort)(c.R * c.A / UInt16.MaxValue);
                    G = (ushort)(c.G * c.A / UInt16.MaxValue);
                    B = (ushort)(c.B * c.A / UInt16.MaxValue);
                    break;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PColor64"/> struct from a <see cref="Color32"/> instance.
        /// </summary>
        /// <param name="c">A <see cref="Color32"/> structure to initialize a new instance of <see cref="PColor64"/> from.</param>
        public PColor64(Color32 c)
            : this(new Color64(c))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PColor64"/> struct from a <see cref="PColor32"/> instance.
        /// </summary>
        /// <param name="c">A <see cref="PColor32"/> structure to initialize a new instance of <see cref="PColor64"/> from.</param>
        public PColor64(PColor32 c)
#if !NET5_0_OR_GREATER
            : this() // so the compiler does not complain about not initializing value
#endif
        {
#if NET5_0_OR_GREATER
            Unsafe.SkipInit(out this);
#endif
            B = ColorSpaceHelper.ToUInt16(c.B);
            G = ColorSpaceHelper.ToUInt16(c.G);
            R = ColorSpaceHelper.ToUInt16(c.R);
            A = ColorSpaceHelper.ToUInt16(c.A);
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
        /// Returns a valid <see cref="PColor64"/> instance by clipping the possibly exceeding original RGB values.
        /// If <see cref="IsValid"/> returns <see langword="true"/>, then the result is the same as the original instance.
        /// </summary>
        /// <returns>A valid <see cref="PColor64"/> instance by clipping the possibly exceeding original RGB values.</returns>
        public PColor64 Clip() => new PColor64(A,
            Math.Min(A, R),
            Math.Min(A, G),
            Math.Min(A, B));

        /// <summary>
        /// Converts this <see cref="PColor64"/> instance to a <see cref="Color64"/> structure.
        /// It's practically the same as calling the <see cref="ColorExtensions.ToStraight(PColor64)"/> method.
        /// </summary>
        /// <returns>A <see cref="Color64"/> structure converted from this <see cref="PColor64"/> instance.</returns>
        public Color64 ToColor64() => A switch
        {
            UInt16.MaxValue => new Color64(value),
            UInt16.MinValue => default,
            _ => new Color64(A,
                (ushort)((uint)R * UInt16.MaxValue / A),
                (ushort)((uint)G * UInt16.MaxValue / A),
                (ushort)((uint)B * UInt16.MaxValue / A))
        };

        /// <summary>
        /// Converts this <see cref="PColor64"/> instance to a <see cref="Color32"/> structure.
        /// </summary>
        /// <returns>A <see cref="Color32"/> structure converted from this <see cref="PColor64"/> instance.</returns>
        public Color32 ToColor32() => A switch
        {
            UInt16.MaxValue => new Color32((byte)(A >> 8), (byte)(R >> 8), (byte)(G >> 8), (byte)(B >> 8)),
            UInt16.MinValue => default,
            _ => new Color32((byte)(A >> 8),
                (byte)(((uint)R * UInt16.MaxValue / A) >> 8),
                (byte)(((uint)G * UInt16.MaxValue / A) >> 8),
                (byte)(((uint)B * UInt16.MaxValue / A) >> 8))
        };

        /// <summary>
        /// Converts this <see cref="PColor64"/> instance to a <see cref="PColor32"/> structure.
        /// </summary>
        /// <returns>A <see cref="PColor32"/> structure converted from this <see cref="PColor64"/> instance.</returns>
        public PColor32 ToPColor32() => new PColor32(ColorSpaceHelper.ToByte(A),
            ColorSpaceHelper.ToByte(R),
            ColorSpaceHelper.ToByte(G),
            ColorSpaceHelper.ToByte(B));

        /// <summary>
        /// Determines whether the current <see cref="PColor64"/> instance is equal to another one.
        /// </summary>
        /// <param name="other">A <see cref="PColor64"/> structure to compare with this <see cref="PColor64"/> instance.</param>
        /// <returns><see langword="true"/>, if the current <see cref="PColor64"/> instance is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.</returns>
        public bool Equals(PColor64 other) => value == other.value;

        /// <summary>
        /// Determines whether the specified <see cref="object"/> is equal to this <see cref="PColor64"/> instance.
        /// </summary>
        /// <param name="obj">The <see cref="object" /> to compare with this <see cref="PColor64"/> instance.</param>
        /// <returns><see langword="true"/>, if the current <see cref="PColor64"/> instance is equal to the <paramref name="obj" /> parameter; otherwise, <see langword="false" />.</returns>
        public override bool Equals(object? obj) => obj is PColor64 other && Equals(other);

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public override int GetHashCode() => value.GetHashCode();

        /// <summary>
        /// Gets the string representation of this <see cref="PColor64"/> instance.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents this <see cref="PColor64"/> instance.</returns>
        public override string ToString() => $"{value:X16} [A={A}; R={R}; G={G}; B={B}]";

        #endregion

        #endregion
    }
}
