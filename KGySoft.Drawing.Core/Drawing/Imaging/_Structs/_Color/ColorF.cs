#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ColorF.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2026 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;
#if !(NETCOREAPP || NET45_OR_GREATER || NETSTANDARD)
using System.Diagnostics.CodeAnalysis;
#endif
#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
using System.Numerics;
#endif
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#if NETCOREAPP3_0_OR_GREATER
using System.Runtime.Intrinsics.X86;
using System.Runtime.Intrinsics;
#endif

#endregion

namespace KGySoft.Drawing.Imaging
{
    /// <summary>
    /// Represents a 128-bit linear (not gamma-corrected) color where every color channel is represented by a 32-bit floating point value.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    [Serializable]
    public readonly struct ColorF : IEquatable<ColorF>, IColor<ColorF, ColorF>
    {
        #region Fields

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
        [FieldOffset(0)]
        [NonSerialized]
        internal readonly RgbF RgbF;

#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD

        [FieldOffset(0)]
        [NonSerialized]
        internal readonly Vector4 Rgba;

        [FieldOffset(0)]
        [NonSerialized]
        internal readonly Vector3 Rgb;

#if NETCOREAPP3_0_OR_GREATER
        [FieldOffset(0)]
        [NonSerialized]
        internal readonly Vector128<float> RgbaV128;
#endif

#endif
        #endregion

        #endregion

        #region Properties

        #region Public Properties

        /// <summary>
        /// Gets whether this <see cref="ColorF"/> instance represents a valid color.
        /// That is, when <see cref="A"/>, <see cref="R"/>, <see cref="G"/> and <see cref="B"/> fields are all between 0 and 1.
        /// </summary>
#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
        public bool IsValid => Clip().Rgba == Rgba;
#else
        public bool IsValid => Clip() == this;
#endif

        #endregion

        #region Explicitly Implemented Interface Properties

        bool IColor<ColorF>.IsTransparent => A <= 0f;
        bool IColor<ColorF>.IsOpaque => A >= 1f;

        #endregion

        #endregion

        #region Operators

        /// <summary>
        /// Gets whether two <see cref="ColorF"/> structures are equal.
        /// </summary>
        /// <param name="left">The <see cref="ColorF"/> instance that is to the left of the equality operator.</param>
        /// <param name="right">The <see cref="ColorF"/> instance that is to the right of the equality operator.</param>
        /// <returns><see langword="true"/> if the two <see cref="ColorF"/> structures are equal; otherwise, <see langword="false"/>.</returns>
        public static bool operator ==(ColorF left, ColorF right) => left.Equals(right);

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
#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
            return new ColorF(left.Rgba * right);
#else
            return new ColorF(left.A * right, left.R * right, left.G * right, left.B * right);
#endif
        }

        /// <summary>
        /// Divides a <see cref="ColorF"/> by the given scalar.
        /// </summary>
        /// <param name="left">The source color.</param>
        /// <param name="right">The scalar value.</param>
        /// <returns>The result of the division.</returns>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static ColorF operator /(ColorF left, float right)
        {
#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD2_0
            return new ColorF(left.Rgba.Div(right));
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
#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
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
#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
            return new ColorF(left.Rgba + new Vector4(right));
#else
            return new ColorF(left.A + right, left.R + right, left.G + right, left.B + right);
#endif
        }

        /// <summary>
        /// Subtracts the second color from the first one.
        /// </summary>
        /// <param name="left">The first source color.</param>
        /// <param name="right">The second source color.</param>
        /// <returns>The color that results from subtracting <paramref name="right"/> from <paramref name="right"/>.</returns>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static ColorF operator -(ColorF left, ColorF right)
        {
#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
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
#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
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
        /// For performance reasons this overload does not validate if the color components are between 0 and 1, but you can use
        /// the <see cref="ColorF(float, float, float, float, bool)"/> constructor or the <see cref="IsValid"/> property for validation,
        /// or the <see cref="Clip">Clip</see> method to return a valid instance.
        /// </summary>
        /// <param name="a">The alpha component.</param>
        /// <param name="r">The red component.</param>
        /// <param name="g">The green component.</param>
        /// <param name="b">The blue component.</param>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public ColorF(float a, float r, float g, float b)
#if (NETCOREAPP || NET45_OR_GREATER || NETSTANDARD) && !NET5_0_OR_GREATER
            : this() // so the compiler does not complain about not initializing the other fields
#endif
        {
#if NET5_0_OR_GREATER
            Unsafe.SkipInit(out this);
#endif
#if NETCOREAPP3_0_OR_GREATER
            RgbaV128 = Vector128.Create(r, g, b, a);
#elif NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
            Rgba = new Vector4(r, g, b, a);
#else
            R = r;
            G = g;
            B = b;
            A = a;
#endif
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ColorF"/> struct from RGB (red, green, and blue) values.
        /// For performance reasons this overload does not validate if the color components are between 0 and 1, but you can use
        /// the <see cref="ColorF(float, float, float, bool)"/> constructor or the <see cref="IsValid"/> property for validation,
        /// or the <see cref="Clip">Clip</see> method to return a valid instance.
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
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="validate"/> is <see langword="true"/> and <paramref name="r"/>, <paramref name="g"/>, <paramref name="b"/> or <paramref name="a"/> is not between 0 and 1.</exception>
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
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="validate"/> is <see langword="true"/> and <paramref name="r"/>, <paramref name="g"/> or <paramref name="b"/> is not between 0 and 1.</exception>
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
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public ColorF(Color32 c)
#if (NETCOREAPP || NET45_OR_GREATER || NETSTANDARD) && !NET5_0_OR_GREATER
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
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public ColorF(Color64 c)
#if (NETCOREAPP || NET45_OR_GREATER || NETSTANDARD) && !NET5_0_OR_GREATER
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

#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
        [MethodImpl(MethodImpl.AggressiveInlining)]
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

#if NETCOREAPP3_0_OR_GREATER
        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal ColorF(Vector128<float> vector)
#if !NET5_0_OR_GREATER
            : this() // so the compiler does not complain about not initializing ARGB fields
#endif
        {
#if NET5_0_OR_GREATER
            Unsafe.SkipInit(out this);
#endif
            RgbaV128 = vector;
        }

#endif

        #endregion

        #endregion

        #region Methods

        #region Static Methods

        #region Public Methods

        /// <summary>
        /// Creates a <see cref="ColorF"/> instance from the specified <see cref="ColorF"/> structure, but with the new specified alpha value.
        /// This method does not validate if the color components are between 0 and 1, but you can use the <see cref="IsValid"/> property
        /// or the <see cref="Clip">Clip</see> method on the result.
        /// </summary>
        /// <param name="a">The alpha value for the new <see cref="ColorF"/> instance.</param>
        /// <param name="baseColor">The <see cref="ColorF"/> instance from which to create the new one.</param>
        /// <returns>A <see cref="ColorF"/> instance from the specified <see cref="ColorF"/> structure and alpha value.</returns>
        public static ColorF FromArgb(float a, ColorF baseColor) =>
#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
            new ColorF(new Vector4(baseColor.Rgb, a));
#else
            new ColorF(a, baseColor.R, baseColor.G, baseColor.B);
#endif

#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
        /// <summary>
        /// Creates a <see cref="ColorF"/> structure from a <see cref="Vector4"/> instance mapping <see cref="Vector4.X"/> to <see cref="R"/>,
        /// <see cref="Vector4.Y"/> to <see cref="G"/>, <see cref="Vector4.Z"/> to <see cref="B"/> and <see cref="Vector4.W"/> to <see cref="A"/>.
        /// </summary>
        /// <param name="vector">A <see cref="Vector4"/> representing the RGBA color components. The parameter is not validated, but
        /// you can use the <see cref="IsValid"/> property or the <see cref="Clip">Clip</see> method on the created result.</param>
        /// <returns>A <see cref="ColorF"/> structure converted from the specified <see cref="Vector4"/>.</returns>
        public static ColorF FromRgba(Vector4 vector) => new ColorF(vector);

        /// <summary>
        /// Creates a <see cref="ColorF"/> structure from a <see cref="Vector3"/> instance mapping <see cref="Vector3.X"/> to <see cref="R"/>,
        /// <see cref="Vector3.Y"/> to <see cref="G"/> and <see cref="Vector3.Z"/> to <see cref="B"/>. The <see cref="A"/> component of the result will be 1.
        /// </summary>
        /// <param name="vector">A <see cref="Vector3"/> representing the RGB color components. The parameter is not validated, but
        /// you can use the <see cref="IsValid"/> property or the <see cref="Clip">Clip</see> method on the created result.</param>
        /// <returns>A <see cref="ColorF"/> structure converted from the specified <see cref="Vector3"/>.</returns>
        public static ColorF FromRgb(Vector3 vector) => new ColorF(new Vector4(vector, 1f));
#endif

        /// <summary>
        /// Creates a <see cref="ColorF"/> structure representing a grayscale color of the specified <paramref name="brightness"/>.
        /// </summary>
        /// <param name="brightness">The brightness of the gray color to be created where 0 represents the black color and 1 represents the white color.
        /// The parameter is not validated, but you can use the &lt;see cref="IsValid"/&gt; property or the &lt;see cref="Clip"&gt;Clip&lt;/see&gt; method on the created result.</param>
        /// <returns>A <see cref="ColorF"/> structure representing a grayscale color of the specified <paramref name="brightness"/>.</returns>
        public static ColorF FromGray(float brightness)
#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
            => FromRgb(new Vector3(brightness));
#else
            => new ColorF(1f, brightness, brightness, brightness);
#endif

        #endregion

        #region Internal Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static ColorF FromColor32NoColorSpaceChange(Color32 c)
        {
#if NETCOREAPP3_0_OR_GREATER
            if (Sse2.IsSupported)
            {
                Vector128<float> result;

                // Converting the ARGB values to float
                if (Sse41.IsSupported)
                {
                    // Order is BGRA because we reinterpret the original value as bytes if supported
                    Vector128<float> bgraF = Sse2.ConvertToVector128Single(Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(c.Value).AsByte()));

                    // Swapping R and B in the final result
                    result = Sse.Shuffle(bgraF, bgraF, 0b_11_00_01_10);
                }
                else
                {
                    // Cannot do the conversion in one step. 4x byte to int + 1x ints to floats is still faster than 4x byte to float in separate steps.
                    result = Sse2.ConvertToVector128Single(Vector128.Create(c.R, c.G, c.B, c.A));
                }

                return new ColorF(Sse.Multiply(result, Vector128.Create(1f / 255f)));
            }
#endif
#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
            return new ColorF(new Vector4(c.R, c.G, c.B, c.A) * VectorExtensions.Max8BitRecip);
#else
            return new ColorF(ColorSpaceHelper.ToFloat(c.A),
                ColorSpaceHelper.ToFloat(c.R),
                ColorSpaceHelper.ToFloat(c.G),
                ColorSpaceHelper.ToFloat(c.B));
#endif
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static ColorF FromColor64NoColorSpaceChange(Color64 c)
        {
#if NETCOREAPP3_0_OR_GREATER
            if (Sse2.IsSupported)
            {
                Vector128<float> result;

                // Converting the ARGB values to float
                if (Sse41.IsSupported)
                {
                    // Order is BGRA because we reinterpret the original value as ushorts if supported
                    Vector128<float> bgraF = Sse2.ConvertToVector128Single(Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(c.Value).AsUInt16()));

                    // Swapping R and B in the final result
                    result = Sse.Shuffle(bgraF, bgraF, 0b_11_00_01_10);
                }
                else
                {
                    // Cannot do the conversion in one step. 4x ushort to int + 1x ints to floats is still faster than 4x byte to float in separate steps.
                    result = Sse2.ConvertToVector128Single(Vector128.Create(c.R, c.G, c.B, c.A));
                }

                return new ColorF(Sse.Multiply(result, Vector128.Create(1f / 65535f)));
            }
#endif
#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
            return new ColorF(new Vector4(c.R, c.G, c.B, c.A) * VectorExtensions.Max16BitRecip);
#else
            return new ColorF(ColorSpaceHelper.ToFloat(c.A),
                ColorSpaceHelper.ToFloat(c.R),
                ColorSpaceHelper.ToFloat(c.G),
                ColorSpaceHelper.ToFloat(c.B));
#endif
        }

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
#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
        public ColorF Clip() => new ColorF(Rgba.ClipF());
#else
        public ColorF Clip() => new ColorF(A.ClipF(), R.ClipF(), G.ClipF(), B.ClipF());
#endif

#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
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
        public Color32 ToColor32()
        {
#if NET9_0_OR_GREATER // NOTE: though this block would compile on .NET 7+, allowing auto-vectorization on .NET9+ only is intended
            if (Vector128.IsHardwareAccelerated)
            {
                // Unlike Sse2.ConvertToVector128Int32, Vector128.ConvertToInt32 does not perform actual rounding, so we need to add +0.5f
                Vector128<float> rgbaF = (ColorSpaceHelper.LinearToSrgbVectorRgba(RgbaV128) * VectorExtensions.Max8BitF + VectorExtensions.HalfF).Clip(Vector128<float>.Zero, VectorExtensions.Max8BitF);
                Vector128<byte> rgbaI32 = Vector128.ConvertToInt32(rgbaF).AsByte();
#if NET10_0_OR_GREATER
                return new Color32(Vector128.ShuffleNative(rgbaI32, VectorExtensions.PackRgbaAsBgraBytesMask).AsUInt32().ToScalar());
#else
                return new Color32(Vector128.Shuffle(rgbaI32, VectorExtensions.PackRgbaAsBgraBytesMask).AsUInt32().ToScalar());
#endif
            }
#elif NETCOREAPP3_0_OR_GREATER                         
            if (Sse2.IsSupported)
            {
                // Sse2.ConvertToVector128Int32 performs actual rounding
                // so we can spare the additional operation +0.5 of the non-accelerated version.
                Vector128<float> rgbaF = Sse.Multiply(ColorSpaceHelper.LinearToSrgbVectorRgba(RgbaV128), VectorExtensions.Max8BitF);
                rgbaF = Sse.Min(Sse.Max(rgbaF, Vector128<float>.Zero), VectorExtensions.Max8BitF);
                Vector128<byte> rgbaI32 = Sse2.ConvertToVector128Int32(rgbaF).AsByte();
                return Ssse3.IsSupported
                    ? new Color32(Ssse3.Shuffle(rgbaI32, VectorExtensions.PackRgbaAsBgraBytesMask).AsUInt32().ToScalar())
                    : new Color32(rgbaI32.GetElement(12), rgbaI32.GetElement(0), rgbaI32.GetElement(4), rgbaI32.GetElement(8));
            }
#endif

            // The non-accelerated version.
            return new Color32(ColorSpaceHelper.ToByte(A),
                ColorSpaceHelper.LinearToSrgb8Bit(R),
                ColorSpaceHelper.LinearToSrgb8Bit(G),
                ColorSpaceHelper.LinearToSrgb8Bit(B));
        }

        /// <summary>
        /// Converts this <see cref="ColorF"/> instance to a <see cref="Color64"/> structure.
        /// </summary>
        /// <returns>A <see cref="Color64"/> structure converted from this <see cref="ColorF"/> instance.</returns>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public Color64 ToColor64()
        {
#if NET9_0_OR_GREATER // NOTE: though this block would compile on .NET 7+, allowing auto-vectorization on .NET9+ only is intended
            if (Vector128.IsHardwareAccelerated)
            {
                // Unlike Sse2.ConvertToVector128Int32, Vector128.ConvertToInt32 does not perform actual rounding, so we need to add +0.5f
                Vector128<float> rgbaF = (ColorSpaceHelper.LinearToSrgbVectorRgba(RgbaV128) * VectorExtensions.Max16BitF + VectorExtensions.HalfF).Clip(Vector128<float>.Zero, VectorExtensions.Max16BitF);
                Vector128<byte> rgbaI32 = Vector128.ConvertToInt32(rgbaF).AsByte();
#if NET10_0_OR_GREATER
                return new Color64(Vector128.ShuffleNative(rgbaI32, VectorExtensions.PackRgbaAsBgraWordsMask).AsUInt64().ToScalar());
#else
                return new Color64(Vector128.Shuffle(rgbaI32, VectorExtensions.PackRgbaAsBgraWordsMask).AsUInt64().ToScalar());
#endif
            }
#elif NETCOREAPP3_0_OR_GREATER
            if (Sse2.IsSupported)
            {
                // Sse2.ConvertToVector128Int32 performs actual rounding
                // so we can spare the additional operation +0.5 of the non-accelerated version.
                Vector128<float> rgbaF = Sse.Multiply(ColorSpaceHelper.LinearToSrgbVectorRgba(RgbaV128), VectorExtensions.Max16BitF);
                rgbaF = Sse.Min(Sse.Max(rgbaF, Vector128<float>.Zero), VectorExtensions.Max16BitF);
                Vector128<ushort> rgbaI32 = Sse2.ConvertToVector128Int32(rgbaF).AsUInt16();
                return Ssse3.IsSupported
                    ? new Color64(Ssse3.Shuffle(rgbaI32.AsByte(), VectorExtensions.PackRgbaAsBgraWordsMask).AsUInt64().ToScalar())
                    : new Color64(rgbaI32.GetElement(6), rgbaI32.GetElement(0), rgbaI32.GetElement(2), rgbaI32.GetElement(4));
            }
#endif

            // The non-accelerated version.
            return new Color64(ColorSpaceHelper.ToUInt16(A),
                ColorSpaceHelper.LinearToSrgb16Bit(R),
                ColorSpaceHelper.LinearToSrgb16Bit(G),
                ColorSpaceHelper.LinearToSrgb16Bit(B));
        }

        /// <summary>
        /// Gets a <see cref="ColorF"/> instance that represents the matching gray shade of this <see cref="ColorF"/> instance based on human perception.
        /// </summary>
        /// <returns>A <see cref="ColorF"/> instance that represents the matching gray shade of this <see cref="ColorF"/> instance based on human perception.</returns>
        public ColorF ToGray()
        {
            float br = this.GetBrightness();
#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
            return new ColorF(new Vector4(new Vector3(br), A));
#else
            return new ColorF(A, br, br, br);
#endif
        }

        /// <summary>
        /// Gets a <see cref="ColorF"/> instance that represents this <see cref="ColorF"/> without alpha (transparency).
        /// </summary>
        /// <returns>A <see cref="ColorF"/> instance that represents this <see cref="ColorF"/> without alpha.</returns>
        public ColorF ToOpaque() => A >= 1f ? this
#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
            : FromRgb(Rgb);
#else
            : new ColorF(1f, R, G, B);
#endif

        /// <summary>
        /// Gets the string representation of this <see cref="ColorF"/> instance.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents this <see cref="ColorF"/> instance.</returns>
        public override string ToString() => $"[A={A:N8}; R={R:N8}; G={G:N8}; B={B:N8}]";

        /// <summary>
        /// Determines whether the current <see cref="ColorF"/> instance is equal to another one.
        /// </summary>
        /// <param name="other">A <see cref="ColorF"/> structure to compare with this <see cref="ColorF"/> instance.</param>
        /// <returns><see langword="true"/>, if the current <see cref="ColorF"/> instance is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.</returns>
#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
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
#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
        public override int GetHashCode() => Rgba.GetHashCode();
#else
        public override int GetHashCode() => (R, G, B, A).GetHashCode();
#endif

        #endregion

        #region Internal Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal Color32 ToColor32NoColorSpaceChange()
        {
#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
            Vector4 result = (Rgba * VectorExtensions.Max8Bit + VectorExtensions.Half).Clip(Vector4.Zero, VectorExtensions.Max8Bit);

#if NETCOREAPP3_0_OR_GREATER
            // Using vectorization also for the float -> int conversion if possible.
            if (Sse2.IsSupported)
            {
                // Using Sse2.ConvertToVector128Int32WithTruncation here because above we already added +0.5
                Vector128<byte> rgbaI32 = Sse2.ConvertToVector128Int32WithTruncation(result.AsVector128()).AsByte();
                return Ssse3.IsSupported
                    ? new Color32(Ssse3.Shuffle(rgbaI32, VectorExtensions.PackRgbaAsBgraBytesMask).AsUInt32().ToScalar())
                    : new Color32(rgbaI32.GetElement(12), rgbaI32.GetElement(0), rgbaI32.GetElement(4), rgbaI32.GetElement(8));
            }
#endif
            return new Color32((byte)result.W, (byte)result.X, (byte)result.Y, (byte)result.Z);
#else
            ColorF result = this * Byte.MaxValue + 0.5f;
            return new Color32(result.A.ClipToByte(),
                result.R.ClipToByte(),
                result.G.ClipToByte(),
                result.B.ClipToByte());
#endif
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal Color64 ToColor64NoColorSpaceChange()
        {
#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
            Vector4 result = (Rgba * VectorExtensions.Max16Bit + VectorExtensions.Half).Clip(Vector4.Zero, VectorExtensions.Max16Bit);

#if NETCOREAPP3_0_OR_GREATER
            // Using vectorization also for the float -> int conversion if possible.
            if (Sse2.IsSupported)
            {
                // Using Sse2.ConvertToVector128Int32WithTruncation here because above we already added +0.5
                Vector128<ushort> rgbaI32 = Sse2.ConvertToVector128Int32WithTruncation(result.AsVector128()).AsUInt16();
                return Ssse3.IsSupported
                    ? new Color64(Ssse3.Shuffle(rgbaI32.AsByte(), VectorExtensions.PackRgbaAsBgraWordsMask).AsUInt64().ToScalar())
                    : new Color64(rgbaI32.GetElement(6), rgbaI32.GetElement(0), rgbaI32.GetElement(2), rgbaI32.GetElement(4));
            }
#endif
            return new Color64((ushort)result.W, (ushort)result.X, (ushort)result.Y, (ushort)result.Z);
#else
            ColorF result = this * UInt16.MaxValue + 0.5f;
            return new Color64(result.A.ClipToUInt16(),
                result.R.ClipToUInt16(),
                result.G.ClipToUInt16(),
                result.B.ClipToUInt16());
#endif
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal ColorF ToSrgb()
        {
#if NETCOREAPP3_0_OR_GREATER
            return new ColorF(ColorSpaceHelper.LinearToSrgbVectorRgba(RgbaV128));
#elif NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
            return new ColorF(ColorSpaceHelper.LinearToSrgbVectorRgba(Rgba));
#else
            return new ColorF(A.ClipF(),
                ColorSpaceHelper.LinearToSrgb(R),
                ColorSpaceHelper.LinearToSrgb(G),
                ColorSpaceHelper.LinearToSrgb(B));
#endif
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal ColorF ToLinear()
        {
#if NETCOREAPP3_0_OR_GREATER
            return new ColorF(ColorSpaceHelper.SrgbToLinearVectorRgba(RgbaV128));
#elif NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
            return new ColorF(ColorSpaceHelper.SrgbToLinearVectorRgba(Rgba));
#else
            return new ColorF(A.ClipF(),
                ColorSpaceHelper.SrgbToLinear(R),
                ColorSpaceHelper.SrgbToLinear(G),
                ColorSpaceHelper.SrgbToLinear(B));
#endif
        }

        #endregion

        #region Explicitly Implemented Interface Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        ColorF IColor<ColorF>.BlendSrgb(ColorF backColor) => A >= 1f ? this
            : backColor.A >= 1f ? this.BlendWithBackgroundSrgb(backColor)
            : A <= 0f ? backColor
            : backColor.A <= 0f ? this
            : this.BlendWithSrgb(backColor);

        ColorF IColor<ColorF>.BlendLinear(ColorF backColor) => this.Blend(backColor);

        ColorF IColor<ColorF, ColorF>.WithAlpha(byte a, ColorF baseColor)
        {
            Debug.Assert(this == baseColor);
            Debug.Assert(A >= 1f, "Expected to be called on opaque colors");
            return FromArgb(ColorSpaceHelper.ToFloat(a), this);
        }

        ColorF IColor<ColorF, ColorF>.AdjustAlpha(byte factor, ColorF baseColor)
        {
            Debug.Assert(this == baseColor);
            return FromArgb(ColorSpaceHelper.ToFloat(factor) * A, this);
        }

        #endregion

        #endregion

        #endregion
    }
}
