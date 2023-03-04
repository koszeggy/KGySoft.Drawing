#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ColorF.cs
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
#if !(NETCOREAPP || NET46_OR_GREATER || NETSTANDARD2_1_OR_GREATER)
using System.Diagnostics.CodeAnalysis; 
#endif
#if NETCOREAPP || NET46_OR_GREATER || NETSTANDARD2_1_OR_GREATER
using System.Numerics;
#endif
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#endregion

namespace KGySoft.Drawing.Imaging
{
    /// <summary>
    /// Represents a 128-bit linear (not gamma-corrected) color where every color channel is represented by a 32-bit floating point value.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    [Serializable]
    public readonly struct ColorF : IEquatable<ColorF>
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

        #region Public Properties

        /// <summary>
        /// Gets whether this <see cref="ColorF"/> instance represents a valid color.
        /// That is, when <see cref="A"/>, <see cref="R"/>, <see cref="G"/> and <see cref="B"/> fields are all between 0 and 1.
        /// </summary>
#if !NETCOREAPP || NET46_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        public bool IsValid => Clip().Rgba == Rgba;
#else
        public bool IsValid => Clip() == this;
#endif

        #endregion

        #region Internal Properties

#if NETCOREAPP || NET46_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        internal RgbF RgbF => new RgbF(Rgb);
#else
        internal RgbF RgbF => new RgbF(R, G, B);
#endif

        #endregion

        #endregion

        #region Operators

        /// <summary>
        /// Gets whether two <see cref="ColorF"/> structures are equal.
        /// </summary>
        /// <param name="left">The <see cref="ColorF"/> instance that is to the left of the equality operator.</param>
        /// <param name="right">The <see cref="ColorF"/> instance that is to the right of the equality operator.</param>
        /// <returns><see langword="true"/> if the two <see cref="ColorF"/> structures are equal; otherwise, <see langword="false"/>.</returns>
        public static bool operator ==(ColorF left, ColorF right) => Equals(left, right);

        /// <summary>
        /// Gets whether two <see cref="ColorF"/> structures are different.
        /// </summary>
        /// <param name="left">The <see cref="ColorF"/> instance that is to the left of the inequality operator.</param>
        /// <param name="right">The <see cref="ColorF"/> instance that is to the right of the inequality operator.</param>
        /// <returns><see langword="true"/> if the two <see cref="ColorF"/> structures are different; otherwise, <see langword="false"/>.</returns>
        public static bool operator !=(ColorF left, ColorF right) => !(left == right);

        /// <summary>
        /// Multiplies a <see cref="ColorF"/> by the given scalar.
        /// </summary>
        /// <param name="left">The source color.</param>
        /// <param name="right">The scalar value.</param>
        /// <returns>The scaled color.</returns>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static ColorF operator *(ColorF left, float right)
        {
#if NETCOREAPP || NET46_OR_GREATER || NETSTANDARD2_1_OR_GREATER
            return new ColorF(left.Rgba * new Vector4(right));
#else
            return new ColorF(left.A * right, left.R * right, left.G * right, left.B * right);
#endif
        }

        /// <summary>
        /// Divides a <see cref="ColorF"/> by the given scalar.
        /// </summary>
        /// <param name="left">The source color.</param>
        /// <param name="right">The scalar value.</param>
        /// <returns>The scaled color.</returns>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static ColorF operator /(ColorF left, float right)
        {
#if NETCOREAPP || NET46_OR_GREATER || NETSTANDARD2_1_OR_GREATER
            return new ColorF(left.Rgba / new Vector4(right));
#else
            return new ColorF(left.A / right, left.R / right, left.G / right, left.B / right);
#endif
        }

        /// <summary>
        /// Adds two colors together.
        /// </summary>
        /// <param name="left">The first source color.</param>
        /// <param name="right">The second source color.</param>
        /// <returns>The summed color.</returns>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static ColorF operator +(ColorF left, ColorF right)
        {
#if NETCOREAPP || NET46_OR_GREATER || NETSTANDARD2_1_OR_GREATER
            return new ColorF(left.Rgba + right.Rgba);
#else
            return new ColorF(left.A + right.A, left.R + right.R, left.G + right.G, left.B + right.B);
#endif
        }

        /// <summary>
        /// Adds a given scalar to a <see cref="ColorF"/>.
        /// </summary>
        /// <param name="left">The source color.</param>
        /// <param name="right">The scalar value.</param>
        /// <returns>The result color.</returns>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static ColorF operator +(ColorF left, float right)
        {
#if NETCOREAPP || NET46_OR_GREATER || NETSTANDARD2_1_OR_GREATER
            return new ColorF(left.Rgba * new Vector4(right));
#else
            return new ColorF(left.A + right, left.R + right, left.G + right, left.B + right);
#endif
        }

        /// <summary>
        /// Subtracts the second color from the first one.
        /// </summary>
        /// <param name="left">The first source color.</param>
        /// <param name="right">The second source color.</param>
        /// <returns>The summed color.</returns>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static ColorF operator -(ColorF left, ColorF right)
        {
#if NETCOREAPP || NET46_OR_GREATER || NETSTANDARD2_1_OR_GREATER
            return new ColorF(left.Rgba - right.Rgba);
#else
            return new ColorF(left.A - right.A, left.R - right.R, left.G - right.G, left.B - right.B);
#endif
        }

        /// <summary>
        /// Subtracts a given scalar from the first color.
        /// </summary>
        /// <param name="left">The source color.</param>
        /// <param name="right">The scalar value.</param>
        /// <returns>The result color.</returns>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static ColorF operator -(ColorF left, float right)
        {
#if NETCOREAPP || NET46_OR_GREATER || NETSTANDARD2_1_OR_GREATER
            return new ColorF(left.Rgba - new Vector4(right));
#else
            return new ColorF(left.A - right, left.R - right, left.G - right, left.B - right);
#endif
        }

        #endregion

        #region Constructors

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ColorF"/> struct from ARGB (alpha, red, green, and blue) values.
        /// For performance reasons this overload does not validate the parameters but you can use the <see cref="ColorF(float, float, float, float, bool)"/> constructor
        /// or the <see cref="IsValid"/> property for validation, or the <see cref="Clip">Clip</see> method to return a valid instance.
        /// </summary>
        /// <param name="a">The alpha component.</param>
        /// <param name="r">The red component.</param>
        /// <param name="g">The green component.</param>
        /// <param name="b">The blue component.</param>
        public ColorF(float a, float r, float g, float b)
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
        /// Initializes a new instance of the <see cref="ColorF"/> struct from RGB (red, green, and blue) values.
        /// For performance reasons this overload does not validate the parameters but you can use the <see cref="ColorF(float, float, float, bool)"/> constructor
        /// or the <see cref="IsValid"/> property for validation, or the <see cref="Clip">Clip</see> method to return a valid instance.
        /// </summary>
        /// <param name="r">The red component.</param>
        /// <param name="g">The green component.</param>
        /// <param name="b">The blue component.</param>
        public ColorF(float r, float g, float b)
            : this(1f, r, g, b)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ColorF"/> struct from ARGB (alpha, red, green, and blue) values.
        /// </summary>
        /// <param name="a">The alpha component.</param>
        /// <param name="r">The red component.</param>
        /// <param name="g">The green component.</param>
        /// <param name="b">The blue component.</param>
        /// <param name="validate"><see langword="true"/> to validate the parameters; <see langword="false"/> to skip the validation.</param>
        public ColorF(float a, float r, float g, float b, bool validate)
            : this(a, r, g, b)
        {
            if (validate && !IsValid)
                ThrowInvalid();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ColorF"/> struct from RGB (red, green, and blue) values.
        /// </summary>
        /// <param name="r">The red component.</param>
        /// <param name="g">The green component.</param>
        /// <param name="b">The blue component.</param>
        /// <param name="validate"><see langword="true"/> to validate the parameters; <see langword="false"/> to skip the validation.</param>
        public ColorF(float r, float g, float b, bool validate)
            : this(r, g, b)
        {
            if (validate && !IsValid)
                ThrowInvalid();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ColorF"/> struct from a <see cref="Color32"/> instance.
        /// </summary>
        /// <param name="c">A <see cref="Color32"/> structure to initialize a new instance of <see cref="ColorF"/> from.</param>
        public ColorF(Color32 c)
#if (NETCOREAPP || NET46_OR_GREATER || NETSTANDARD2_1_OR_GREATER) && !NET5_0_OR_GREATER
            : this() // so the compiler does not complain about not initializing value field
#endif
        {
#if NET5_0_OR_GREATER
            Unsafe.SkipInit(out this);
#endif
            R = ColorSpaceHelper.SrgbToLinear(c.R);
            G = ColorSpaceHelper.SrgbToLinear(c.G);
            B = ColorSpaceHelper.SrgbToLinear(c.B);
            A = ColorSpaceHelper.ToFloat(c.A);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ColorF"/> struct from a <see cref="Color64"/> instance.
        /// </summary>
        /// <param name="c">A <see cref="Color64"/> structure to initialize a new instance of <see cref="ColorF"/> from.</param>
        public ColorF(Color64 c)
#if (NETCOREAPP || NET46_OR_GREATER || NETSTANDARD2_1_OR_GREATER) && !NET5_0_OR_GREATER
            : this() // so the compiler does not complain about not initializing value field
#endif
        {
#if NET5_0_OR_GREATER
            Unsafe.SkipInit(out this);
#endif
            R = ColorSpaceHelper.SrgbToLinear(c.R);
            G = ColorSpaceHelper.SrgbToLinear(c.G);
            B = ColorSpaceHelper.SrgbToLinear(c.B);
            A = ColorSpaceHelper.ToFloat(c.A);
        }

        #endregion

        #region Internal Constructors

//        internal ColorF(Color32 c, bool adjustGamma)
//#if (NETCOREAPP || NET46_OR_GREATER || NETSTANDARD2_1_OR_GREATER) && !NET5_0_OR_GREATER
//            : this() // so the compiler does not complain about not initializing value field
//#endif
//        {
//#if NET5_0_OR_GREATER
//            Unsafe.SkipInit(out this);
//#endif
//            if (adjustGamma)
//            {
//                this = new ColorF(c);
//                return;
//            }

//#if NETCOREAPP || NET46_OR_GREATER || NETSTANDARD2_1_OR_GREATER
//            Rgba = new Vector4(c.R, c.G, c.B, c.A) / max8Bpp;
//#else
//            R = ColorSpaceHelper.ToFloat(c.R);
//            G = ColorSpaceHelper.ToFloat(c.G);
//            B = ColorSpaceHelper.ToFloat(c.B);
//            A = ColorSpaceHelper.ToFloat(c.A);
//#endif
//        }


#if NETCOREAPP || NET46_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        internal ColorF(Vector4 vector)
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
        /// Creates a <see cref="ColorF"/> structure from a <see cref="Vector4"/> instance mapping <see cref="Vector4.X"/> to <see cref="R"/>,
        /// <see cref="Vector4.Y"/> to <see cref="G"/>, <see cref="Vector4.Z"/> to <see cref="B"/> and <see cref="Vector4.W"/> to <see cref="A"/>.
        /// </summary>
        /// <param name="vector">A <see cref="Vector4"/> representing the RGBA color components. The parameter is not validated but
        /// You can use the <see cref="IsValid"/> property or the <see cref="Clip">Clip</see> method on the created result.</param>
        /// <returns>A <see cref="ColorF"/> structure converted from the specified <see cref="Vector4"/>.</returns>

        public static ColorF FromRgba(Vector4 vector) => new ColorF(vector);

        /// <summary>
        /// Creates a <see cref="ColorF"/> structure from a <see cref="Vector3"/> instance mapping <see cref="Vector3.X"/> to <see cref="R"/>,
        /// <see cref="Vector3.Y"/> to <see cref="G"/> and <see cref="Vector3.Z"/> to <see cref="B"/>. The <see cref="A"/> component of the result will be 1.
        /// </summary>
        /// <param name="vector">A <see cref="Vector3"/> representing the RGB color components. The parameter is not validated but
        /// You can use the <see cref="IsValid"/> property or the <see cref="Clip">Clip</see> method on the created result.</param>
        /// <returns>A <see cref="ColorF"/> structure converted from the specified <see cref="Vector3"/>.</returns>
        public static ColorF FromRgb(Vector3 vector) => new ColorF(new Vector4(vector, 1f));
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
        /// Returns a valid <see cref="ColorF"/> instance by clipping the possibly exceeding ARGB values.
        /// If <see cref="IsValid"/> returns <see langword="true"/>, then the result is the same as the original instance.
        /// </summary>
        /// <returns>A valid <see cref="ColorF"/> instance by clipping the possibly exceeding ARGB values.</returns>
#if NETCOREAPP || NET46_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        public ColorF Clip() => new ColorF(Vector4.Clamp(Rgba, Vector4.Zero, Vector4.One));
#else
        public ColorF Clip() => new ColorF(A.Clip(0f, 1f), R.Clip(0f, 1f), G.Clip(0f, 1f), B.Clip(0f, 1f));
#endif

#if NETCOREAPP || NET46_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        /// <summary>
        /// Converts this <see cref="ColorF"/> instance to a <see cref="Vector4"/> structure
        /// mapping <see cref="R"/> to <see cref="Vector4.X"/>, <see cref="G"/> to <see cref="Vector4.Y"/>,
        /// <see cref="B"/> to <see cref="Vector4.Z"/> and <see cref="A"/> to <see cref="Vector4.W"/>.
        /// </summary>
        /// <returns>A <see cref="Vector4"/> structure converted from this <see cref="ColorF"/> instance.</returns>
        public Vector4 ToRgba() => Rgba;

        /// <summary>
        /// Converts this <see cref="ColorF"/> instance to a <see cref="Vector3"/> structure
        /// mapping <see cref="R"/> to <see cref="Vector3.X"/>, <see cref="G"/> to <see cref="Vector3.Y"/>
        /// and <see cref="B"/> to <see cref="Vector3.Z"/>.
        /// </summary>
        /// <returns>A <see cref="Vector3"/> structure converted from this <see cref="ColorF"/> instance.</returns>
        public Vector3 ToRgb() => Rgb;
#endif

        /// <summary>
        /// Converts this <see cref="ColorF"/> instance to a <see cref="Color32"/> structure.
        /// </summary>
        /// <returns>A <see cref="Color32"/> structure converted from this <see cref="ColorF"/> instance.</returns>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public Color32 ToColor32() => new Color32(ColorSpaceHelper.ToByte(A),
            ColorSpaceHelper.LinearToSrgb8Bit(R),
            ColorSpaceHelper.LinearToSrgb8Bit(G),
            ColorSpaceHelper.LinearToSrgb8Bit(B));

        /// <summary>
        /// Converts this <see cref="ColorF"/> instance to a <see cref="Color64"/> structure.
        /// </summary>
        /// <returns>A <see cref="Color64"/> structure converted from this <see cref="ColorF"/> instance.</returns>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public Color64 ToColor64() => new Color64(ColorSpaceHelper.ToUInt16(A),
            ColorSpaceHelper.LinearToSrgb16Bit(R),
            ColorSpaceHelper.LinearToSrgb16Bit(G),
            ColorSpaceHelper.LinearToSrgb16Bit(B));

        /// <summary>
        /// Gets the string representation of this <see cref="ColorF"/> instance.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents this <see cref="ColorF"/> instance.</returns>
        public override string ToString() => $"[A={A:N4}; R={R:N4}; G={G:N4}; B={B:N4}]";

        /// <summary>
        /// Determines whether the current <see cref="ColorF"/> instance is equal to another one.
        /// </summary>
        /// <param name="other">A <see cref="ColorF"/> structure to compare with this <see cref="ColorF"/> instance.</param>
        /// <returns><see langword="true"/>, if the current <see cref="ColorF"/> instance is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.</returns>
#if NETCOREAPP || NET46_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        public bool Equals(ColorF other) => other.Rgba == Rgba;
#else
        [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator", Justification = "It is intended in Equals")]
        public bool Equals(ColorF other) => other.R == R && other.G == G && other.B == B && other.A == A;
#endif

        /// <summary>
        /// Determines whether the specified <see cref="object"/> is equal to this <see cref="ColorF"/> instance.
        /// </summary>
        /// <param name="obj">The <see cref="object" /> to compare with this <see cref="ColorF"/> instance.</param>
        /// <returns><see langword="true"/>, if the current <see cref="ColorF"/> instance is equal to the <paramref name="obj" /> parameter; otherwise, <see langword="false" />.</returns>
        public override bool Equals(object? obj) => obj is ColorF other && Equals(other);

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
        internal Color32 ToColor32(bool adjustGamma)
        {
            if (adjustGamma)
                return ToColor32();

#if NETCOREAPP || NET46_OR_GREATER || NETSTANDARD2_1_OR_GREATER
            Vector4 result = Vector4.Clamp(Rgba * max8Bpp + half, Vector4.Zero, max8Bpp);
            return new Color32((byte)result.W, (byte)result.X, (byte)result.Y, (byte)result.Z);
#else
            ColorF result = this * Byte.MaxValue + 0.5f;
            return new Color32(result.A.ClipToByte(),
                result.R.ClipToByte(),
                result.G.ClipToByte(),
                result.B.ClipToByte());
#endif
        }

        internal ColorF ToSrgb() => new ColorF(A,
            ColorSpaceHelper.LinearToSrgb(R),
            ColorSpaceHelper.LinearToSrgb(G),
            ColorSpaceHelper.LinearToSrgb(B));

        internal ColorF ToLinear() => new ColorF(A,
            ColorSpaceHelper.SrgbToLinear(R),
            ColorSpaceHelper.SrgbToLinear(G),
            ColorSpaceHelper.SrgbToLinear(B));

        #endregion

        #endregion

        #endregion
    }
}
