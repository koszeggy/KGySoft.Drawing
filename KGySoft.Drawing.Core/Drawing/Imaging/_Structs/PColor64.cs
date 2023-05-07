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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#if NETCOREAPP3_0_OR_GREATER
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
#endif
using System.Security;

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

        #region Static Properties

#if NET5_0_OR_GREATER
        // Inlining Vector128.Create is faster on .NET 5 and above than caching a static field
        private static Vector128<byte> PackLowWordsMask => Vector128.Create(0, 1, 4, 5, 8, 9, 12, 13, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF);
        private static Vector128<byte> PackHighBytesOfLowWordsMask => Vector128.Create(1, 5, 9, 13, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF);
#elif NETCOREAPP3_0_OR_GREATER
        private static Vector128<byte> PackLowWordsMask { get; } = Vector128.Create(0, 1, 4, 5, 8, 9, 12, 13, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF);
        private static Vector128<byte> PackHighBytesOfLowWordsMask { get; } = Vector128.Create(1, 5, 9, 13, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF);
#endif
            
        #endregion

        #region Instance Properties

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

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PColor64"/> struct from ARGB (alpha, red, green, and blue) values.
        /// For performance reasons this overload does not validate the parameters but you can use the <see cref="PColor64(ushort, ushort, ushort, ushort, bool)"/> constructor
        /// or the <see cref="IsValid"/> property for validation, or the <see cref="Clip">Clip</see> method to return a valid instance.
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
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="validate"/> is <see langword="true"/> and <paramref name="r"/>, <paramref name="g"/> or <paramref name="b"/> is not between 0 and <paramref name="a"/>.</exception>
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
        [MethodImpl(MethodImpl.AggressiveInlining)]
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
#if NETCOREAPP3_0_OR_GREATER
                    // Using vectorization if possible. It is faster even with the floating-point conversion than using non-accelerated integer divisions,
                    // but only with hardware intrinsics (so not using Vector3/Vector4 here because it is much slower for some reason).
                    // Using bit-shifting could prevent using floating point calculations but the result would be less accurate.
                    if (Sse2.IsSupported)
                    {
                        // Converting the [A]RGB values to float (order is BGR[A] because we reinterpret the original value as ushorts if supported)
                        Vector128<float> bgrxF = Sse2.ConvertToVector128Single(Sse41.IsSupported
                            // Reinterpreting the ulong value as ushorts and converting them to ints in one step is still faster than converting them separately
                            ? Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(c.Value).AsUInt16())
                            // Cannot do the conversion in one step. Sparing one conversion because A is actually not needed here.
                            : Vector128.Create(c.B, c.G, c.R, default));

                        // Doing the division first to prevent running out of the integer-precision of float, which is 24 bit.
                        // Instead of actual division we use a multiplication with the reciprocal of max value.
                        bgrxF = Sse.Multiply(bgrxF, Vector128.Create(1f / 65535));

                        Vector128<int> aI32;
                        if (Sse41.IsSupported)
                        {
                            // Doing the ushort -> int conversion by SSE 4.1 is faster for some reason even if there is only one conversion
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

                        // Sse2.ConvertToVector128Int32 performs actual rounding instead of the truncating conversion of the
                        // non-accelerated version so the results can be different by 1 shade, but this provides the more correct result.
                        // Unfortunately there is no direct vectorized conversion to ushort so we need to pack the result if possible.
                        Vector128<int> bgrxI32 = Sse2.ConvertToVector128Int32(bgrxF);

                        // Initializing directly from ulong if it is supported to pack the ints to ushorts
                        if (Ssse3.IsSupported)
                        {
                            // Setting A from the original color
                            bgrxI32 = bgrxI32.AsUInt16().WithElement(6, c.A).AsInt32();

                            // Compressing 32-bit values to 16 bit ones and initializing value from the first 64 bit
                            value = (Sse41.IsSupported
                                    ? Sse41.PackUnsignedSaturate(bgrxI32, bgrxI32).AsUInt64()
                                    : Ssse3.Shuffle(bgrxI32.AsByte(), PackLowWordsMask).AsUInt64())
                                .ToScalar();
                            return;
                        }

                        // Casting from the int results one by one. It's still faster than
                        // converting the components from floats without the ConvertToVector128Int32 call.
                        B = (ushort)bgrxI32.GetElement(0);
                        G = (ushort)bgrxI32.GetElement(1);
                        R = (ushort)bgrxI32.GetElement(2);
                        A = c.A;
                        return;
                    }
#endif

                    // The non-accelerated version. Bit-shifting, eg. R = (ushort)(((uint)c.R * c.A) >> 16) would be faster but less accurate.
                    B = (ushort)((uint)c.B * c.A / UInt16.MaxValue);
                    G = (ushort)((uint)c.G * c.A / UInt16.MaxValue);
                    R = (ushort)((uint)c.R * c.A / UInt16.MaxValue);
                    A = c.A;
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
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public PColor64(PColor32 c)
#if !NET5_0_OR_GREATER
            : this() // so the compiler does not complain about not initializing value
#endif
        {
#if NET5_0_OR_GREATER
            Unsafe.SkipInit(out this);
#endif
#if NETCOREAPP3_0_OR_GREATER
            // Using the same vectorization as in the Color64(Color32) ctor. See more comments there.
            if (Sse2.IsSupported)
            {
                Vector128<ushort> bgraU16 = Sse41.IsSupported
                    ? Sse41.ConvertToVector128Int16(Vector128.CreateScalarUnsafe(c.Value).AsByte()).AsUInt16()
                    : Vector128.Create((ushort)c.B, c.G, c.R, c.A, 0, 0, 0, 0);

                bgraU16 = Sse2.MultiplyLow(bgraU16, Vector128.Create((ushort)257));
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

        internal PColor64(ulong argb)
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
        /// Creates a <see cref="PColor64"/> structure from a 64-bit ARGB value.
        /// </summary>
        /// <param name="argb">A value specifying the 64-bit ARGB value. As a hex value it can be specified as <c>0xAAAA_RRRR_GGGG_BBBB</c>
        /// where <c>AAAA</c> is the highest word and <c>BBBB</c> is the lowest word.</param>
        /// <returns>A <see cref="PColor64"/> structure from the specified 64-bit ARGB value.</returns>
        public static PColor64 FromArgb(long argb) => new PColor64((ulong)argb);

        /// <summary>
        /// Creates a <see cref="PColor64"/> structure from a 64-bit ARGB value.
        /// </summary>
        /// <param name="argb">A value specifying the 64-bit ARGB value. As a hex value it can be specified as <c>0xAAAA_RRRR_GGGG_BBBB</c>
        /// where <c>AAAA</c> is the highest word and <c>BBBB</c> is the lowest word.</param>
        /// <returns>A <see cref="PColor64"/> structure from the specified 64-bit ARGB value.</returns>
        [CLSCompliant(false)]
        public static PColor64 FromArgb(ulong argb) => new PColor64(argb);

        #endregion

        #region Private Methods

        [MethodImpl(MethodImplOptions.NoInlining)]
        [SuppressMessage("ReSharper", "NotResolvedInText", Justification = "Parameter of the caller method")]
        private static void ThrowInvalid() => throw new ArgumentOutOfRangeException("a", Res.ImagingInvalidPremultipliedValues);

        #endregion

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
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public Color64 ToColor64()
        {
            switch (A)
            {
                case UInt16.MaxValue:
                    return new Color64(value);
                case UInt16.MinValue:
                    return default;
                default:
#if NETCOREAPP3_0_OR_GREATER
                    // Using vectorization if possible. It is faster even with the floating-point conversion than using non-accelerated integer divisions,
                    // but only with hardware intrinsics (so not using Vector3/Vector4 here because it is much slower for some reason).
                    if (Sse2.IsSupported)
                    {
                        // Converting the [A]RGB values to float (order is BGR[A] because we reinterpret the original value as ushorts if supported)
                        Vector128<float> bgrxF = Sse2.ConvertToVector128Single(Sse41.IsSupported
                            // Reinterpreting the ulong value as ushorts and converting them to ints in one step is still faster than converting them separately
                            ? Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(value).AsUInt16())
                            // Cannot do the conversion in one step. Sparing one conversion because A is actually not needed here.
                            : Vector128.Create(B, G, R, default));

                        Vector128<int> aI32;
                        if (Sse41.IsSupported)
                        {
                            // Doing the ushort -> int conversion by SSE 4.1 is faster for some reason even if there is only one conversion
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

                        // Doing the division first to prevent running out of the integer-precision of float, which is 24 bit.
                        // Unlike in case of Color64 -> PColor64 conversion multiplication with reciprocal is not that fast because 1f/A is not a constant,
                        // and cannot use Sse.Reciprocal because it has only 1.5*2^-12 precision.
                        bgrxF = Sse.Divide(bgrxF, Sse2.ConvertToVector128Single(aI32));

                        bgrxF = Sse.Multiply(bgrxF, Vector128.Create(65535f));

                        // Sse2.ConvertToVector128Int32 performs actual rounding instead of the truncating conversion of the
                        // non-accelerated version so the results can be different by 1 shade, but this provides the more correct result.
                        // Unfortunately there is no direct vectorized conversion to ushort so we need to pack the result if possible.
                        Vector128<int> bgrxI32 = Sse2.ConvertToVector128Int32(bgrxF);

                        // Initializing directly from ulong if it is supported to pack the ints to ushorts
                        if (Ssse3.IsSupported)
                        {
                            // Setting A from the original color
                            bgrxI32 = bgrxI32.AsUInt16().WithElement(6, A).AsInt32();

                            // Compressing 32-bit values to 16 bit ones and initializing value from the first 64 bit
                            return new Color64((Sse41.IsSupported
                                    ? Sse41.PackUnsignedSaturate(bgrxI32, bgrxI32).AsUInt64()
                                    : Ssse3.Shuffle(bgrxI32.AsByte(), PackLowWordsMask).AsUInt64())
                                .ToScalar());
                        }

                        // Casting from the int results one by one. It's still faster than
                        // converting the components from floats without the ConvertToVector128Int32 call.
                        return new Color64(A,
                            (ushort)bgrxI32.GetElement(2),
                            (ushort)bgrxI32.GetElement(1),
                            (ushort)bgrxI32.GetElement(0));
                    }
#endif

                    // The non-accelerated version. Bit-shifting, eg. r:(ushort)(((uint)R << 16) / A) would not be much faster because it still contains a division.
                    return new Color64(A,
                        (ushort)((uint)R * UInt16.MaxValue / A),
                        (ushort)((uint)G * UInt16.MaxValue / A),
                        (ushort)((uint)B * UInt16.MaxValue / A));
            }
        }

        /// <summary>
        /// Converts this <see cref="PColor64"/> instance to a <see cref="Color32"/> structure.
        /// </summary>
        /// <returns>A <see cref="Color32"/> structure converted from this <see cref="PColor64"/> instance.</returns>
        [SecuritySafeCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public unsafe Color32 ToColor32()
        {
            // The simplest solution could be just returning ToColor64().ToColor32 but by a direct implementation we can spare a few steps
            switch (A)
            {
                case UInt16.MaxValue:
                    // Shortcut: no premultiplied -> straight conversion is needed.
                    // This is the same as Color64.ToColor32. See more comments there.
                    ulong bgraU16 = value;
                    byte* bytes = (byte*)&bgraU16;
                    return new Color32(bytes[7], bytes[5], bytes[3], bytes[1]);

                case UInt16.MinValue:
                    return default;

                default:
#if NETCOREAPP3_0_OR_GREATER
                    // Using vectorization if possible. The fist part is very similar to ToColor64, see more comments there.
                    if (Sse2.IsSupported)
                    {
                        Vector128<float> bgrxF = Sse2.ConvertToVector128Single(Sse41.IsSupported
                            ? Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(value).AsUInt16())
                            : Vector128.Create(B, G, R, default));

                        Vector128<int> aI32;
                        if (Sse41.IsSupported)
                        {
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

                        bgrxF = Sse.Divide(bgrxF, Sse2.ConvertToVector128Single(aI32));
                        bgrxF = Sse.Multiply(bgrxF, Vector128.Create(65535f));

                        Vector128<byte> bgraI32 = Sse2.ConvertToVector128Int32(bgrxF).AsUInt16().WithElement(6, A).AsByte();

                        // Up to this point everything is almost the same as in ToColor64.
                        // But now we need to create a 4x8-bit result from our 4x32-bit vector containing 16-bit values.
                        if (Ssse3.IsSupported)
                        {
                            // Taking the 2nd byte of every 32-bit value (high byte of the 16-bit values), ignoring the rest.
                            return new Color32(Ssse3.Shuffle(bgraI32, PackHighBytesOfLowWordsMask).AsUInt32().ToScalar());
                        }

                        // Casting from the int results one by one. It's still faster than
                        // converting the components from floats without the ConvertToVector128Int32 call.
                        return new Color32(bgraI32.GetElement(13),
                            bgraI32.GetElement(9),
                            bgraI32.GetElement(5),
                            bgraI32.GetElement(1));
                    }
#endif

                    // The non-accelerated version. Almost the same as in ToColor64 except that we use only the high bytes in the result.
                    return new Color32((byte)(A >> 8),
                        (byte)(((uint)R * UInt16.MaxValue / A) >> 8),
                        (byte)(((uint)G * UInt16.MaxValue / A) >> 8),
                        (byte)(((uint)B * UInt16.MaxValue / A) >> 8));
            }
        }

        /// <summary>
        /// Converts this <see cref="PColor64"/> instance to a <see cref="PColor32"/> structure.
        /// </summary>
        /// <returns>A <see cref="PColor32"/> structure converted from this <see cref="PColor64"/> instance.</returns>
        [SecuritySafeCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public unsafe PColor32 ToPColor32()
        {
            // The same conversion as in Color64.ToColor32. See more comments there.
            ulong bgraU16 = value;
            byte* bytes = (byte*)&bgraU16;
            return new PColor32(bytes[7], bytes[5], bytes[3], bytes[1]);
        }

        /// <summary>
        /// Gets the 64-bit ARGB value of this <see cref="PColor64"/> instance.
        /// </summary>
        /// <returns>The 64-bit ARGB value of this <see cref="PColor64"/> instance</returns>
        public long ToArgb() => (long)value;

        /// <summary>
        /// Gets the 64-bit ARGB value of this <see cref="PColor64"/> instance.
        /// </summary>
        /// <returns>The 64-bit ARGB value of this <see cref="PColor64"/> instance</returns>
        [CLSCompliant(false)]
        public ulong ToArgbUInt64() => value;

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
