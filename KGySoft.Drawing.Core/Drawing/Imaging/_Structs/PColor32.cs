#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: PColor32.cs
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
using System.Diagnostics.CodeAnalysis;
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
    /// Represents a 32-bit premultiplied sRGB color where every color channel is represented by a 8-bit integer.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    [Serializable]
    public readonly struct PColor32 : IEquatable<PColor32>, IColor<PColor32, Color32>
    {
        #region Fields

        #region Public Fields

        /// <summary>
        /// Gets the alpha component value of this <see cref="PColor32"/> structure. This field is read-only.
        /// </summary>
        [FieldOffset(3)]
        [NonSerialized]
        public readonly byte A;

        /// <summary>
        /// Gets the red component value of this <see cref="PColor32"/> structure. This field is read-only.
        /// </summary>
        [FieldOffset(2)]
        [NonSerialized]
        public readonly byte R;

        /// <summary>
        /// Gets the green component value of this <see cref="PColor32"/> structure. This field is read-only.
        /// </summary>
        [FieldOffset(1)]
        [NonSerialized]
        public readonly byte G;

        /// <summary>
        /// Gets the blue component value of this <see cref="PColor32"/> structure. This field is read-only.
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

        #region Static Properties

#if NET5_0_OR_GREATER
        // Inlining Vector128.Create is faster on .NET 5 and above than caching a static field
        private static Vector128<byte> PackLowBytesMask => Vector128.Create(0, 4, 8, 12, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF);
#elif NETCOREAPP3_0_OR_GREATER
        private static Vector128<byte> PackLowBytesMask { get; } = Vector128.Create(0, 4, 8, 12, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF);
#endif

        #endregion

        #region Instance Properties

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

        #region Explicitly Implemented Interface Properties

        bool IColor<PColor32, Color32>.IsOpaque => A == Byte.MaxValue;

        #endregion

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

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PColor32"/> struct from ARGB (alpha, red, green, and blue) values.
        /// For performance reasons this overload does not validate the parameters but you can use the <see cref="PColor32(byte, byte, byte, byte, bool)"/> constructor
        /// or the <see cref="IsValid"/> property for validation, or the <see cref="Clip">Clip</see> method to return a valid instance.
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
        /// <param name="validate"><see langword="true"/> to validate the parameters; <see langword="false"/> to skip the validation.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="validate"/> is <see langword="true"/> and <paramref name="r"/>, <paramref name="g"/> or <paramref name="b"/> is not between 0 and <paramref name="a"/>.</exception>
        public PColor32(byte a, byte r, byte g, byte b, bool validate)
            : this(a, r, g, b)
        {
            if (validate && (r > a || g > a || b > a))
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
        [MethodImpl(MethodImpl.AggressiveInlining)]
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
#if NETCOREAPP3_0_OR_GREATER
                    // Using vectorization if possible. It is faster even with the floating-point conversion than using non-accelerated integer divisions,
                    // but only with hardware intrinsics (so not using Vector3/Vector4 here because it is much slower for some reason).
                    // Using bit-shifting could prevent using floating point calculations but the result would be less accurate.
                    if (Sse2.IsSupported)
                    {
                        // Converting the [A]RGB values to float (order is BGR[A] because we reinterpret the original value as bytes if supported)
                        Vector128<float> bgrxF = Sse2.ConvertToVector128Single(Sse41.IsSupported
                            // Reinterpreting the uint value as bytes and converting them to ints in one step is still faster than converting them separately
                            ? Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(c.Value).AsByte())
                            // Cannot do the conversion in one step. Sparing one conversion because A is actually not needed here.
                            : Vector128.Create(c.B, c.G, c.R, default));

                        Vector128<int> aI32;
                        if (Sse41.IsSupported)
                        {
                            // Doing the byte -> int conversion by SSE 4.1 is faster for some reason even if there is only one conversion
                            aI32 = Sse41.ConvertToVector128Int32(Vector128.Create(c.A));
                        }
                        else
                        {
#if NET8_0_OR_GREATER
                            aI32 = Vector128.Create((int)c.A);
#else
                            // Workaround for bug: https://github.com/dotnet/runtime/issues/83387
                            int a = c.A;
                            aI32 = Vector128.Create(a);
#endif
                        }

                        bgrxF = Sse.Multiply(bgrxF, Sse2.ConvertToVector128Single(aI32));

                        // Instead of division we use a multiplication with the reciprocal of max value
                        bgrxF = Sse.Multiply(bgrxF, Vector128.Create(1f / 255f));

                        // Sse2.ConvertToVector128Int32 performs actual rounding instead of the truncating conversion of the
                        // non-accelerated version so the results can be different by 1 shade, but this provides the more correct result.
                        // Unfortunately there is no direct vectorized conversion to byte so we need to pack the result if possible.
                        Vector128<int> bgrxI32 = Sse2.ConvertToVector128Int32(bgrxF);

                        // Initializing directly from uint if it is supported to shuffle the ints as packed bytes
                        if (Ssse3.IsSupported)
                        {
                            // Compressing 32-bit values to 8 bit ones and initializing value from the first 32 bit
                            value = Ssse3.Shuffle(bgrxI32.AsByte().WithElement(12, c.A), PackLowBytesMask).AsUInt32().ToScalar();
                            return;
                        }

                        // Casting from the int results one by one. It's still faster than
                        // converting the components from floats without the ConvertToVector128Int32 call.
                        B = (byte)bgrxI32.GetElement(0);
                        G = (byte)bgrxI32.GetElement(1);
                        R = (byte)bgrxI32.GetElement(2);
                        A = c.A;
                        return;
                    }
#endif

                    // The non-accelerated version. Bit-shifting, eg. R = (byte)((c.R * c.A) >> 8) would be faster but less accurate.
                    B = (byte)(c.B * c.A / Byte.MaxValue);
                    G = (byte)(c.G * c.A / Byte.MaxValue);
                    R = (byte)(c.R * c.A / Byte.MaxValue);
                    A = c.A;
                    break;
            }
        }

        #endregion

        #region Internal Constructors

        internal PColor32(uint argb)
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

        #region Public Methods

        /// <summary>
        /// Creates a <see cref="PColor32"/> structure from a 32-bit ARGB value.
        /// </summary>
        /// <param name="argb">A value specifying the 32-bit ARGB value. As a hex value it can be specified as <c>0xAA_RR_GG_BB</c> where <c>AA</c>
        /// is the most significant byte (MSB) and <c>BB</c> is the least significant byte (LSB). The parameter is not validated but
        /// you can use the <see cref="IsValid"/> property or the <see cref="Clip">Clip</see> method on the created result.</param>
        /// <returns>A <see cref="PColor32"/> structure from the specified 32-bit ARGB value.</returns>
        public static PColor32 FromArgb(int argb) => new PColor32((uint)argb);

        /// <summary>
        /// Creates a <see cref="PColor32"/> structure from a 32-bit ARGB value.
        /// </summary>
        /// <param name="argb">A value specifying the 32-bit ARGB value. As a hex value it can be specified as <c>0xAA_RR_GG_BB</c> where <c>AA</c>
        /// is the most significant byte (MSB) and <c>BB</c> is the least significant byte (LSB). The parameter is not validated but
        /// you can use the <see cref="IsValid"/> property or the <see cref="Clip">Clip</see> method on the created result.</param>
        /// <returns>A <see cref="PColor32"/> structure from the specified 32-bit ARGB value.</returns>
        [CLSCompliant(false)]
        public static PColor32 FromArgb(uint argb) => new PColor32(argb);

        /// <summary>
        /// Creates a <see cref="PColor32"/> instance from the specified <see cref="Color32"/> structure specifying a custom alpha value.
        /// </summary>
        /// <param name="a">The alpha value for the result <see cref="PColor32"/> instance.</param>
        /// <param name="baseColor">The <see cref="Color32"/> instance to which apply the new alpha.</param>
        /// <returns>A <see cref="PColor32"/> instance from the specified <see cref="Color32"/> structure and alpha value.</returns>
        public static PColor32 FromArgb(byte a, Color32 baseColor) => new PColor32(Color32.FromArgb(a, baseColor));

        #endregion

        #region Private Methods

        [MethodImpl(MethodImplOptions.NoInlining)]
        [SuppressMessage("ReSharper", "NotResolvedInText", Justification = "Parameter of the caller method")]
        private static void ThrowInvalid() => throw new ArgumentOutOfRangeException("a", Res.ImagingInvalidPremultipliedValues);

        #endregion

        #endregion

        #region Instance Methods

        #region Public Methods

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
        /// It's practically the same as calling the <see cref="ColorExtensions.ToStraight(PColor32)"/> method.
        /// </summary>
        /// <returns>A <see cref="Color32"/> structure converted from this <see cref="PColor32"/> instance.</returns>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public Color32 ToColor32()
        {
            switch (A)
            {
                case Byte.MaxValue:
                    return new Color32(value);
                case Byte.MinValue:
                    return default;
                default:
#if NETCOREAPP3_0_OR_GREATER
                    // Using vectorization if possible. It is faster even with the floating-point conversion than using non-accelerated integer divisions,
                    // but only with hardware intrinsics (so not using Vector3/Vector4 here because it is much slower for some reason).
                    if (Sse2.IsSupported)
                    {
                        // Converting the [A]RGB values to float (order is BGR[A] because we reinterpret the original value as bytes if supported)
                        Vector128<float> bgrxF = Sse2.ConvertToVector128Single(Sse41.IsSupported
                            // Reinterpreting the uint value as bytes and converting them to ints in one step is still faster than converting them separately
                            ? Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(value).AsByte())
                            // Cannot do the conversion in one step. Sparing one conversion because A is actually not needed here.
                            : Vector128.Create(B, G, R, default));

                        bgrxF = Sse.Multiply(bgrxF, Vector128.Create(255f));

                        Vector128<int> aI32;
                        if (Sse41.IsSupported)
                        {
                            // Doing the byte -> int conversion by SSE 4.1 is faster for some reason even if there is only one conversion
                            aI32 = Sse41.ConvertToVector128Int32(Vector128.Create(A));
                        }
                        else
                        {
#if NET8_0_OR_GREATER
                            aI32 = Vector128.Create((int)A);
#else
                            // Workaround for bug: https://github.com/dotnet/runtime/issues/83387
                            int a = A;
                            aI32 = Vector128.Create(a);
#endif
                        }

                        // Unlike in the constructor  multiplication with reciprocal is not that fast because 1f/A is not a constant.
                        bgrxF = Sse.Divide(bgrxF, Sse2.ConvertToVector128Single(aI32));

                        // Sse2.ConvertToVector128Int32 performs actual rounding instead of the truncating conversion of the
                        // non-accelerated version so the results can be different by 1 shade, but this provides the more correct result.
                        // Unfortunately there is no direct vectorized conversion to byte so we need to pack the result if possible.
                        Vector128<int> bgrxI32 = Sse2.ConvertToVector128Int32(bgrxF);

                        // Initializing directly from uint if it is supported to shuffle the ints as packed bytes
                        if (Ssse3.IsSupported)
                            return new Color32(Ssse3.Shuffle(bgrxI32.AsByte().WithElement(12, A), PackLowBytesMask).AsUInt32().ToScalar());
                        
                        return new Color32(A,
                            (byte)bgrxI32.GetElement(2),
                            (byte)bgrxI32.GetElement(1),
                            (byte)bgrxI32.GetElement(0));
                    }
#endif

                    // The non-accelerated version. Bit-shifting, eg. r:(byte)((R << 8) / A) would be neither faster (because it still contains a division)
                    // nor accurate enough (because the result of the division can be 256).
                    return new Color32(A,
                        (byte)(R * Byte.MaxValue / A),
                        (byte)(G * Byte.MaxValue / A),
                        (byte)(B * Byte.MaxValue / A));
            }
        }

        /// <summary>
        /// Gets the 32-bit ARGB value of this <see cref="PColor32"/> instance.
        /// </summary>
        /// <returns>The 32-bit ARGB value of this <see cref="PColor32"/> instance</returns>
        public int ToArgb() => (int)value;

        /// <summary>
        /// Gets the 32-bit ARGB value of this <see cref="PColor32"/> instance.
        /// </summary>
        /// <returns>The 32-bit ARGB value of this <see cref="PColor32"/> instance</returns>
        [CLSCompliant(false)]
        public uint ToArgbUInt32() => value;

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

        #region Explicitly Implemented Interface Methods

        PColor32 IColor<PColor32, Color32>.BlendSrgb(PColor32 backColor) => this.Blend(backColor);
        PColor32 IColor<PColor32, Color32>.BlendLinear(PColor32 backColor) => throw new InvalidOperationException(Res.InternalError("PColor32.BlendLinear should not be called by internal IColor<PColor32, Color32> implementations"));
        
        PColor32 IColor<PColor32, Color32>.WithAlpha(byte a, Color32 baseColor)
        {
            Debug.Assert(this == baseColor.ToPColor32());
            Debug.Assert(A == Byte.MaxValue, "Expected to be called on opaque colors");
            return FromArgb(a, baseColor);
        }

        PColor32 IColor<PColor32, Color32>.AdjustAlpha(byte factor, Color32 baseColor)
        {
            Debug.Assert(this == baseColor.ToPColor32());
            return FromArgb(A == Byte.MaxValue ? factor : (byte)(factor * A / Byte.MaxValue), baseColor);
        }

        #endregion

        #endregion

        #endregion
    }
}
