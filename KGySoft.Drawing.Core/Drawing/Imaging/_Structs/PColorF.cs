﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: PColorF.cs
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
#if NETCOREAPP || NET46_OR_GREATER || NETSTANDARD2_1_OR_GREATER
using System.Numerics;
#endif
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#endregion

namespace KGySoft.Drawing.Imaging
{
    /// <summary>
    /// Represents a 128-bit premultiplied linear (not gamma-corrected) color where every color channel is represented by a 32-bit floating point value.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    [Serializable]
    public readonly struct PColorF : IEquatable<PColorF>
    {
        #region Fields

        #region Static Fields

#if !(NET35 || NET40 || NET45 || NETSTANDARD2_0)
        private static readonly Vector4 max8Bpp = new Vector4(Byte.MaxValue);
        private static readonly Vector4 half = new Vector4(0.5f);
#endif

        #endregion

        #region Instance Fields

        #region Public Fields

        /// <summary>
        /// Gets the red component value of this <see cref="ColorF"/> structure. This field is read-only.
        /// </summary>
        [FieldOffset(0)]
        public readonly float R;

        /// <summary>
        /// Gets the green component value of this <see cref="ColorF"/> structure. This field is read-only.
        /// </summary>
        [FieldOffset(4)]
        public readonly float G;

        /// <summary>
        /// Gets the blue component value of this <see cref="ColorF"/> structure. This field is read-only.
        /// </summary>
        [FieldOffset(8)]
        public readonly float B;

        /// <summary>
        /// Gets the alpha component value of this <see cref="ColorF"/> structure. This field is read-only.
        /// </summary>
        [FieldOffset(12)]
        public readonly float A;

        #endregion

        #region Internal Fields

#if NETCOREAPP || NET46_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        [FieldOffset(0)]
        [NonSerialized]
        internal readonly Vector4 Rgba;

        [FieldOffset(0)]
        [NonSerialized]
        internal readonly Vector3 Rgb;
#endif

        #endregion

        #endregion

        #endregion

        #region Properties

        /// <summary>
        /// Gets whether this <see cref="PColorF"/> instance represents a valid color.
        /// That is, when <see cref="A"/>, <see cref="R"/>, <see cref="G"/> and <see cref="B"/> fields are all between 0 and 1,
        /// and <see cref="A"/> is greater than or equal to <see cref="R"/>, <see cref="G"/> and <see cref="B"/>.
        /// </summary>
#if !NETCOREAPP || NET46_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        public bool IsValid => Clip().Rgba == Rgba;
#else
        public bool IsValid => Clip() == this;
#endif

        #endregion

        #region Operators

        /// <summary>
        /// Gets whether two <see cref="PColorF"/> structures are equal.
        /// </summary>
        /// <param name="left">The <see cref="PColorF"/> instance that is to the left of the equality operator.</param>
        /// <param name="right">The <see cref="PColorF"/> instance that is to the right of the equality operator.</param>
        /// <returns><see langword="true"/> if the two <see cref="PColorF"/> structures are equal; otherwise, <see langword="false"/>.</returns>
        public static bool operator ==(PColorF left, PColorF right) => Equals(left, right);

        /// <summary>
        /// Gets whether two <see cref="PColorF"/> structures are different.
        /// </summary>
        /// <param name="left">The <see cref="PColorF"/> instance that is to the left of the inequality operator.</param>
        /// <param name="right">The <see cref="PColorF"/> instance that is to the right of the inequality operator.</param>
        /// <returns><see langword="true"/> if the two <see cref="PColorF"/> structures are different; otherwise, <see langword="false"/>.</returns>
        public static bool operator !=(PColorF left, PColorF right) => !(left == right);

        /// <summary>
        /// Multiplies a <see cref="PColorF"/> by the given scalar.
        /// </summary>
        /// <param name="left">The source color.</param>
        /// <param name="right">The scalar value.</param>
        /// <returns>The scaled color.</returns>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static PColorF operator *(PColorF left, float right)
        {
#if NETCOREAPP || NET46_OR_GREATER || NETSTANDARD2_1_OR_GREATER
            return new PColorF(left.Rgba * new Vector4(right));
#else
            return new PColorF(left.A * right, left.R * right, left.G * right, left.B * right);
#endif
        }

        /// <summary>
        /// Divides  a <see cref="PColorF"/> by the given scalar.
        /// </summary>
        /// <param name="left">The source color.</param>
        /// <param name="right">The scalar value.</param>
        /// <returns>The scaled color.</returns>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static PColorF operator /(PColorF left, float right)
        {
#if NETCOREAPP || NET46_OR_GREATER || NETSTANDARD2_1_OR_GREATER
            return new PColorF(left.Rgba / new Vector4(right));
#else
            return new PColorF(left.A / right, left.R / right, left.G / right, left.B / right);
#endif
        }

        /// <summary>
        /// Adds two colors together.
        /// </summary>
        /// <param name="left">The first source color.</param>
        /// <param name="right">The second source color.</param>
        /// <returns>The summed color.</returns>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static PColorF operator +(PColorF left, PColorF right)
        {
#if NETCOREAPP || NET46_OR_GREATER || NETSTANDARD2_1_OR_GREATER
            return new PColorF(left.Rgba + right.Rgba);
#else
            return new PColorF(left.A + right.A, left.R + right.R, left.G + right.G, left.B + right.B);
#endif
        }

        /// <summary>
        /// Adds a given scalar to a <see cref="PColorF"/>.
        /// </summary>
        /// <param name="left">The source color.</param>
        /// <param name="right">The scalar value.</param>
        /// <returns>The result color.</returns>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static PColorF operator +(PColorF left, float right)
        {
#if NETCOREAPP || NET46_OR_GREATER || NETSTANDARD2_1_OR_GREATER
            return new PColorF(left.Rgba * new Vector4(right));
#else
            return new PColorF(left.A + right, left.R + right, left.G + right, left.B + right);
#endif
        }

        /// <summary>
        /// Subtracts the second color from the first one.
        /// </summary>
        /// <param name="left">The first source color.</param>
        /// <param name="right">The second source color.</param>
        /// <returns>The summed color.</returns>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static PColorF operator -(PColorF left, PColorF right)
        {
#if NETCOREAPP || NET46_OR_GREATER || NETSTANDARD2_1_OR_GREATER
            return new PColorF(left.Rgba - right.Rgba);
#else
            return new PColorF(left.A - right.A, left.R - right.R, left.G - right.G, left.B - right.B);
#endif
        }

        /// <summary>
        /// Subtracts a given scalar from the first color.
        /// </summary>
        /// <param name="left">The source color.</param>
        /// <param name="right">The scalar value.</param>
        /// <returns>The result color.</returns>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static PColorF operator -(PColorF left, float right)
        {
#if NETCOREAPP || NET46_OR_GREATER || NETSTANDARD2_1_OR_GREATER
            return new PColorF(left.Rgba - new Vector4(right));
#else
            return new PColorF(left.A - right, left.R - right, left.G - right, left.B - right);
#endif
        }

        #endregion

        #region Constructors

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PColorF"/> struct from ARGB (alpha, red, green, and blue) values.
        /// For performance reasons this overload does not validate the parameters but you can use the <see cref="PColorF(float, float, float, float, bool)"/> constructor
        /// or the <see cref="IsValid"/> property for validation, or the <see cref="Clip">Clip</see> method to return a valid instance.
        /// </summary>
        /// <param name="a">The alpha component.</param>
        /// <param name="r">The red component.</param>
        /// <param name="g">The green component.</param>
        /// <param name="b">The blue component.</param>
        public PColorF(float a, float r, float g, float b)
#if (NETCOREAPP || NET46_OR_GREATER || NETSTANDARD2_1_OR_GREATER) && !NET5_0_OR_GREATER
            : this() // so the compiler does not complain about not initializing the vector fields
#endif
        {
#if NET5_0_OR_GREATER
            Unsafe.SkipInit(out this);
#endif
            R = r;
            G = g;
            B = b;
            A = a;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PColorF"/> struct from RGB (red, green, and blue) values.
        /// For performance reasons this overload does not validate the parameters but you can use the <see cref="PColorF(float, float, float, bool)"/> constructor
        /// or the <see cref="IsValid"/> property for validation, or the <see cref="Clip">Clip</see> method to return a valid instance.
        /// </summary>
        /// <param name="r">The red component.</param>
        /// <param name="g">The green component.</param>
        /// <param name="b">The blue component.</param>
        public PColorF(float r, float g, float b)
            : this(1f, r, g, b)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PColorF"/> struct from ARGB (alpha, red, green, and blue) values.
        /// </summary>
        /// <param name="a">The alpha component.</param>
        /// <param name="r">The red component.</param>
        /// <param name="g">The green component.</param>
        /// <param name="b">The blue component.</param>
        /// <param name="validate"><see langword="true"/> to validate the parameters; <see langword="false"/> to skip the validation.</param>
        public PColorF(float a, float r, float g, float b, bool validate)
            : this(a, r, g, b)
        {
            if (validate && !IsValid)
                ThrowInvalid();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PColorF"/> struct from RGB (red, green, and blue) values.
        /// </summary>
        /// <param name="r">The red component.</param>
        /// <param name="g">The green component.</param>
        /// <param name="b">The blue component.</param>
        /// <param name="validate"><see langword="true"/> to validate the parameters; <see langword="false"/> to skip the validation.</param>
        public PColorF(float r, float g, float b, bool validate)
            : this(r, g, b)
        {
            if (validate && !IsValid)
                ThrowInvalid();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PColorF"/> struct from a <see cref="ColorF"/> instance.
        /// </summary>
        /// <param name="c">A <see cref="ColorF"/> structure to initialize a new instance of <see cref="PColorF"/> from.</param>
#if NETCOREAPP || NET46_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public PColorF(ColorF c)
#if !NET5_0_OR_GREATER
            : this() // so the compiler does not complain about not initializing ARGB fields
#endif
        {
#if NET5_0_OR_GREATER
            Unsafe.SkipInit(out this);
#endif
            Rgba = new Vector4(c.Rgb * c.A, c.A);
        }
#else
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public PColorF(ColorF c)
            : this(c.A, c.R * c.A, c.G * c.A, c.B * c.A)
        {
        }
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="PColorF"/> struct from a <see cref="Color32"/> instance.
        /// </summary>
        /// <param name="c">A <see cref="Color32"/> structure to initialize a new instance of <see cref="PColorF"/> from.</param>
        public PColorF(Color32 c) => this = new ColorF(c).ToPremultiplied();

        /// <summary>
        /// Initializes a new instance of the <see cref="PColorF"/> struct from a <see cref="Color64"/> instance.
        /// </summary>
        /// <param name="c">A <see cref="Color64"/> structure to initialize a new instance of <see cref="PColorF"/> from.</param>
        public PColorF(Color64 c) => this = new ColorF(c).ToPremultiplied();

        #endregion

        #region Internal Constructors

        internal PColorF(PColor32 c)
#if (NETCOREAPP || NET46_OR_GREATER || NETSTANDARD2_1_OR_GREATER) && !NET5_0_OR_GREATER
            : this() // so the compiler does not complain about not initializing value field
#endif
        {
#if NET5_0_OR_GREATER
            Unsafe.SkipInit(out this);
#endif

#if NETCOREAPP || NET46_OR_GREATER || NETSTANDARD2_1_OR_GREATER
            Rgba = new Vector4(c.R, c.G, c.B, c.A) / max8Bpp;
#else
            R = ColorSpaceHelper.ToFloat(c.R);
            G = ColorSpaceHelper.ToFloat(c.G);
            B = ColorSpaceHelper.ToFloat(c.B);
            A = ColorSpaceHelper.ToFloat(c.A);
#endif
        }

#if NETCOREAPP || NET46_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        internal PColorF(Vector4 vector)
#if !NET5_0_OR_GREATER
            : this() // so the compiler does not complain about not initializing ARGB fields
#endif
        {
#if NET5_0_OR_GREATER
            Unsafe.SkipInit(out this);
#endif
            Rgba = vector;
        }
#endif

        #endregion

        #endregion

        #region Methods

        #region Static Methods

        #region Public Methods

#if NETCOREAPP || NET46_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        /// <summary>
        /// Creates a <see cref="PColorF"/> structure from a <see cref="Vector4"/> instance mapping <see cref="Vector4.X"/> to <see cref="R"/>,
        /// <see cref="Vector4.Y"/> to <see cref="G"/>, <see cref="Vector4.Z"/> to <see cref="B"/> and <see cref="Vector4.W"/> to <see cref="A"/>.
        /// </summary>
        /// <param name="vector">A <see cref="Vector4"/> representing the RGBA color components. The parameter is not validated but
        /// You can use the <see cref="IsValid"/> property or the <see cref="Clip">Clip</see> method on the created result.</param>
        /// <returns>A <see cref="PColorF"/> structure converted from the specified <see cref="Vector4"/>.</returns>

        public static PColorF FromRgba(Vector4 vector) => new PColorF(vector);

        /// <summary>
        /// Creates a <see cref="PColorF"/> structure from a <see cref="Vector3"/> instance mapping <see cref="Vector3.X"/> to <see cref="R"/>,
        /// <see cref="Vector3.Y"/> to <see cref="G"/> and <see cref="Vector3.Z"/> to <see cref="B"/>. The <see cref="A"/> component of the result will be 1.
        /// </summary>
        /// <param name="vector">A <see cref="Vector3"/> representing the RGB color components. The parameter is not validated but
        /// You can use the <see cref="IsValid"/> property or the <see cref="Clip">Clip</see> method on the created result.</param>
        /// <returns>A <see cref="PColorF"/> structure converted from the specified <see cref="Vector3"/>.</returns>
        public static PColorF FromRgb(Vector3 vector) => new PColorF(new Vector4(vector, 1f));
#endif

        #endregion

        #region Private Methods

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowInvalid() => throw new ArgumentOutOfRangeException(null, Res.ImagingInvalidArgbValues);

        #endregion

        #endregion

        #region Instance Methods

        #region Public Methods

        /// <summary>
        /// Returns a valid <see cref="PColorF"/> instance by clipping the possibly exceeding ARGB values.
        /// If <see cref="IsValid"/> returns <see langword="true"/>, then the result is the same as the original instance.
        /// </summary>
        /// <returns>A valid <see cref="ColorF"/> instance by clipping the possibly exceeding ARGB values.</returns>
#if NETCOREAPP || NET46_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        public PColorF Clip() => new PColorF(Vector4.Clamp(Rgba, Vector4.Zero, new Vector4(A.Clip(0f, 1f))));
#else
        public PColorF Clip()
        {
            float a = A.Clip(0f, 1f);
            return new PColorF(a, R.Clip(0f, a), G.Clip(0f, a), B.Clip(0f, a));
        }
#endif

#if NETCOREAPP || NET46_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        /// <summary>
        /// Converts this <see cref="PColorF"/> instance to a <see cref="Vector4"/> structure
        /// mapping <see cref="R"/> to <see cref="Vector4.X"/>, <see cref="G"/> to <see cref="Vector4.Y"/>,
        /// <see cref="B"/> to <see cref="Vector4.Z"/> and <see cref="A"/> to <see cref="Vector4.W"/>.
        /// </summary>
        /// <returns>A <see cref="Vector4"/> structure converted from this <see cref="PColorF"/> instance.</returns>
        public Vector4 ToRgba() => Rgba;

        /// <summary>
        /// Converts this <see cref="PColorF"/> instance to a <see cref="Vector3"/> structure
        /// mapping <see cref="R"/> to <see cref="Vector3.X"/>, <see cref="G"/> to <see cref="Vector3.Y"/>
        /// and <see cref="B"/> to <see cref="Vector3.Z"/>.
        /// </summary>
        /// <returns>A <see cref="Vector3"/> structure converted from this <see cref="PColorF"/> instance.</returns>
        public Vector3 ToRgb() => Rgb;
#endif

        /// <summary>
        /// Converts this <see cref="PColorF"/> instance to a <see cref="ColorF"/> structure.
        /// It's practically the same as calling the <see cref="ColorExtensions.ToStraight(PColorF)"/> method.
        /// </summary>
        /// <returns>A <see cref="ColorF"/> structure converted from this <see cref="PColorF"/> instance.</returns>
#if NETCOREAPP || NET46_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public ColorF ToColorF() => new ColorF(new Vector4(Rgb / A, A));
#else
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public ColorF ToColorF() => new ColorF(A, R / A, G / A, B / A);
#endif

        /// <summary>
        /// Gets the string representation of this <see cref="PColorF"/> instance.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents this <see cref="PColorF"/> instance.</returns>
        public override string ToString() => $"[A={A:N4}; R={R:N4}; G={G:N4}; B={B:N4}]";

        /// <summary>
        /// Determines whether the current <see cref="PColorF"/> instance is equal to another one.
        /// </summary>
        /// <param name="other">A <see cref="PColorF"/> structure to compare with this <see cref="PColorF"/> instance.</param>
        /// <returns><see langword="true"/>, if the current <see cref="PColorF"/> instance is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.</returns>
#if NETCOREAPP || NET46_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        public bool Equals(PColorF other) => other.Rgba == Rgba;
#else
        [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator", Justification = "It is intended in Equals")]
        public bool Equals(PColorF other) => other.R == R && other.G == G && other.B == B && other.A == A;
#endif

        /// <summary>
        /// Determines whether the specified <see cref="object"/> is equal to this <see cref="PColorF"/> instance.
        /// </summary>
        /// <param name="obj">The <see cref="object" /> to compare with this <see cref="PColorF"/> instance.</param>
        /// <returns><see langword="true"/>, if the current <see cref="PColorF"/> instance is equal to the <paramref name="obj" /> parameter; otherwise, <see langword="false" />.</returns>
        public override bool Equals(object? obj) => obj is PColorF other && Equals(other);

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
#if NETCOREAPP || NET46_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        public override int GetHashCode() => Rgba.GetHashCode();
#else
        public override int GetHashCode() => (R, G, B, A).GetHashCode();
#endif

        #endregion

        #region Internal Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal PColor32 ToPColor32(bool adjustGamma)
        {
            Debug.Assert(!adjustGamma);
            //if (adjustGamma)
            //    return ToColor32().ToPremultiplied();

#if NET35 || NET40 || NET45 || NETSTANDARD2_0
            PColorF result = this * Byte.MaxValue + 0.5f;
            byte a = result.A.ClipToByte();
            return new PColor32(a,
                result.R.ClipToByte(a),
                result.G.ClipToByte(a),
                result.B.ClipToByte(a));
#else
            Vector4 result = Rgba * max8Bpp + half;
            result = Vector4.Clamp(result, Vector4.Zero, new Vector4(Math.Min(result.W, Byte.MaxValue)));
            return new PColor32((byte)result.W, (byte)result.X, (byte)result.Y, (byte)result.Z);
#endif
        }

        #endregion

        #endregion

        #endregion
    }
}
