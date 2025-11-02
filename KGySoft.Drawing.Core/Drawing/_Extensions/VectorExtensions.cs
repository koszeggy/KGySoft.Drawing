#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2025 - All Rights Reserved
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
using System.Numerics;
using System.Runtime.CompilerServices;
#if NETCOREAPP3_0_OR_GREATER
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
#endif
using System.Security;

using KGySoft.Drawing.Imaging;

#endregion

namespace KGySoft.Drawing
{
    internal static class VectorExtensions
    {
        #region Fields

#if NETCOREAPP3_0_OR_GREATER && !NET9_0_OR_GREATER

        private static readonly float allBitsSetF = BitConverter.Int32BitsToSingle(-1);

#endif

        #endregion

        #region Properties

#if NET5_0_OR_GREATER
        // Inlining Vector128.Create is faster on .NET 5 and above than caching a static field
        internal static Vector128<byte> PackLowBytesMask => Vector128.Create(0, 4, 8, 12, default(byte), default, default, default, default, default, default, default, default, default, default, default);
        internal static Vector128<byte> PackLowWordsMask => Vector128.Create(0, 1, 4, 5, 8, 9, 12, 13, default(byte), default, default, default, default, default, default, default);
        internal static Vector128<byte> PackHighBytesOfLowWordsMask => Vector128.Create(1, 5, 9, 13, default(byte), default, default, default, default, default, default, default, default, default, default, default);
        internal static Vector128<byte> PackRgbaAsBgraBytesMask => Vector128.Create(8, 4, 0, 12, default(byte), default, default, default, default, default, default, default, default, default, default, default);
        internal static Vector128<byte> PackRgbaAsBgraWordsMask => Vector128.Create(8, 9, 4, 5, 0, 1, 12, 13, default(byte), default, default, default, default, default, default, default);

        // Properties for constant vectors - needed only when a constant vector is used both in .NET Core 3.x and .NET 5+
#if NET8_0_OR_GREATER
        internal static Vector128<float> OneF => Vector128<float>.One;
        internal static Vector128<int> OneI32 => Vector128<int>.One;
        internal static Vector256<float> One256F => Vector256<float>.One;
        internal static Vector256<int> One256I32 => Vector256<int>.One;
#else
        internal static Vector128<float> OneF => Vector128.Create(1f);
        internal static Vector128<int> OneI32 => Vector128.Create(1);
        internal static Vector256<float> One256F => Vector256.Create(1f);
        internal static Vector256<int> One256I32 => Vector256.Create(1);
#endif
        internal static Vector128<float> TwoF => Vector128.Create(2f);
        internal static Vector256<float> Two256F => Vector256.Create(2f);
        internal static Vector128<float> Max8BitF => Vector128.Create(255f);
        internal static Vector128<byte> Max8BitU8 => Vector128.Create(Byte.MaxValue);
        internal static Vector128<int> Max8BitI32 => Vector128.Create(255);
        internal static Vector128<float> Max16BitF => Vector128.Create(65535f);
        internal static Vector128<ushort> Max16BitU16 => Vector128.Create(UInt16.MaxValue);
        internal static Vector128<int> Max16BitI32 => Vector128.Create(65535);
        internal static Vector128<float> HalfF => Vector128.Create(0.5f);
        internal static Vector128<float> AllBitsSetF => Vector128<float>.AllBitsSet;
        internal static Vector128<float> NegativeZeroF => Vector128.Create(-0f);
        internal static Vector256<float> NegativeZero256F => Vector256.Create(-0f);
        internal static Vector128<float> NegativeInfinityF => Vector128.Create(Single.NegativeInfinity);
        internal static Vector128<float> PositiveInfinityF => Vector128.Create(Single.PositiveInfinity);
        internal static Vector256<float> NegativeInfinity256F => Vector256.Create(Single.NegativeInfinity);
        internal static Vector256<float> PositiveInfinity256F => Vector256.Create(Single.PositiveInfinity);
        internal static Vector128<float> E128 => Vector128.Create(MathF.E);
        internal static Vector256<float> E256 => Vector256.Create(MathF.E);
        internal static Vector128<float> InvE128 => Vector128.Create(1f / MathF.E);
        internal static Vector256<float> InvE256 => Vector256.Create(1f / MathF.E);
        internal static Vector128<float> InvEPerE128 => Vector128.Create(1f / MathF.E / MathF.E);
        internal static Vector256<float> InvEPerE256 => Vector256.Create(1f / MathF.E / MathF.E);
        internal static Vector128<float> MinPreciseIntAsFloat => Vector128.Create(FloatExtensions.MinPreciseIntAsFloat);
        internal static Vector128<float> MaxPreciseIntAsFloat => Vector128.Create(FloatExtensions.MaxPreciseIntAsFloat);
        internal static Vector256<float> MaxPreciseIntAsFloat256 => Vector256.Create(FloatExtensions.MaxPreciseIntAsFloat);
        internal static Vector128<float> NaN128F => Vector128.Create(Single.NaN);
        internal static Vector256<float> NaN256F => Vector256.Create(Single.NaN);
#elif NETCOREAPP3_0_OR_GREATER
        internal static Vector128<byte> PackLowBytesMask { get; } = Vector128.Create(0, 4, 8, 12, default(byte), default, default, default, default, default, default, default, default, default, default, default);
        internal static Vector128<byte> PackLowWordsMask { get; } = Vector128.Create(0, 1, 4, 5, 8, 9, 12, 13, default(byte), default, default, default, default, default, default, default);
        internal static Vector128<byte> PackHighBytesOfLowWordsMask { get; } = Vector128.Create(1, 5, 9, 13, default(byte), default, default, default, default, default, default, default, default, default, default, default);
        internal static Vector128<byte> PackRgbaAsBgraBytesMask { get; } = Vector128.Create(8, 4, 0, 12, default(byte), default, default, default, default, default, default, default, default, default, default, default);
        internal static Vector128<byte> PackRgbaAsBgraWordsMask { get; } = Vector128.Create(8, 9, 4, 5, 0, 1, 12, 13, default(byte), default, default, default, default, default, default, default);
        internal static Vector128<float> OneF { get; } = Vector128.Create(1f);
        internal static Vector128<int> OneI32 { get; } = Vector128.Create(1);
        internal static Vector256<float> One256F { get; } = Vector256.Create(1f);
        internal static Vector256<int> One256I32 { get; } = Vector256.Create(1);
        internal static Vector128<float> TwoF { get; } = Vector128.Create(2f);
        internal static Vector256<float> Two256F { get; } = Vector256.Create(2f);
        internal static Vector128<float> Max8BitF { get; } = Vector128.Create(255f);
        internal static Vector128<byte> Max8BitU8 { get; } = Vector128.Create(Byte.MaxValue);
        internal static Vector128<int> Max8BitI32 { get; } = Vector128.Create(255);
        internal static Vector128<float> Max16BitF { get; } = Vector128.Create(65535f);
        internal static Vector128<ushort> Max16BitU16 { get; } = Vector128.Create(UInt16.MaxValue);
        internal static Vector128<int> Max16BitI32 { get; } = Vector128.Create(65535);
        internal static Vector128<float> HalfF { get; } = Vector128.Create(0.5f);
        internal static Vector128<float> AllBitsSetF { get; } = Vector128.Create(allBitsSetF);
        internal static Vector128<float> NegativeZeroF { get; } = Vector128.Create(-0f);
        internal static Vector256<float> NegativeZero256F { get; } = Vector256.Create(-0f);
        internal static Vector128<float> NegativeInfinityF { get; } = Vector128.Create(Single.NegativeInfinity);
        internal static Vector128<float> PositiveInfinityF { get; } = Vector128.Create(Single.PositiveInfinity);
        internal static Vector256<float> NegativeInfinity256F { get; } = Vector256.Create(Single.NegativeInfinity);
        internal static Vector256<float> PositiveInfinity256F { get; } = Vector256.Create(Single.PositiveInfinity);
        internal static Vector128<float> E128 { get; } = Vector128.Create(MathF.E);
        internal static Vector256<float> E256 { get; } = Vector256.Create(MathF.E);
        internal static Vector128<float> InvE128 { get; } = Vector128.Create(1f / MathF.E);
        internal static Vector256<float> InvE256 { get; } = Vector256.Create(1f / MathF.E);
        internal static Vector128<float> InvEPerE128 { get; } = Vector128.Create(1f / MathF.E / MathF.E);
        internal static Vector256<float> InvEPerE256 { get; } = Vector256.Create(1f / MathF.E / MathF.E);
        internal static Vector128<float> MinPreciseIntAsFloat { get; } = Vector128.Create(FloatExtensions.MinPreciseIntAsFloat);
        internal static Vector128<float> MaxPreciseIntAsFloat { get; } = Vector128.Create(FloatExtensions.MaxPreciseIntAsFloat);
        internal static Vector256<float> MaxPreciseIntAsFloat256 { get; } = Vector256.Create(FloatExtensions.MaxPreciseIntAsFloat);
        internal static Vector128<float> NaN128F { get; } = Vector128.Create(Single.NaN);
        internal static Vector256<float> NaN256F { get; } = Vector256.Create(Single.NaN);
#endif

#if NET5_0_OR_GREATER
        // In .NET 5.0 and above these perform better as inlined rather than caching a static field
        internal static Vector4 Max8Bit => new Vector4(255f);
        internal static Vector3 Max8Bit3 => new Vector3(255f);
        internal static Vector4 Max8BitRecip => new Vector4(1f / 255f);
        internal static Vector4 Max16Bit => new Vector4(65535f);
        internal static Vector3 Max16Bit3 => new Vector3(65535f);
        internal static Vector4 Max16BitRecip => new Vector4(1f / 65535f);
        internal static Vector4 Half => new Vector4(0.5f);
        internal static Vector3 Half3 => new Vector3(0.5f);
#elif NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
        internal static Vector4 Max8Bit { get; } = new Vector4(Byte.MaxValue);
        internal static Vector3 Max8Bit3 { get; } = new Vector3(Byte.MaxValue);
        internal static Vector4 Max8BitRecip { get; } = new Vector4(1f / Byte.MaxValue);
        internal static Vector4 Max16Bit { get; } = new Vector4(UInt16.MaxValue);
        internal static Vector3 Max16Bit3 { get; } = new Vector3(UInt16.MaxValue);
        internal static Vector4 Max16BitRecip { get; } = new Vector4(1f / UInt16.MaxValue);
        internal static Vector4 Half { get; } = new Vector4(0.5f);
        internal static Vector3 Half3 { get; } = new Vector3(0.5f);
#endif

        #endregion

        #region Methods

        #region Casting

#if NETCOREAPP3_0
        internal static Vector128<float> AsVector128(this Vector3 v)
        {
            var result = new Vector4(v, default);
            return result.AsVector128();
        }

        // NOTE: Unlike in .NET 5+, these are ref in/return methods here to perform better (like AsPointF/AsVector2).
        // The .NET 5+ counterparts are intrinsics, that's why they can be faster as value-in + value-return methods (see VectorCastTests in PerformanceTests).
        // Note 2: We can omit the [SecuritySafeCritical] attributes here, as it would be ignored for .NET Core anyway.
        internal static ref Vector128<float> AsVector128(this ref Vector4 v) => ref v.As<Vector4, Vector128<float>>();
        internal static ref Vector3 AsVector3(this ref Vector128<float> v) => ref v.As<Vector128<float>, Vector3>();
        internal static ref Vector4 AsVector4(this ref Vector128<float> v) => ref v.As<Vector128<float>, Vector4>();
#endif

        [SecuritySafeCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static ref PointF AsPointF(this ref Vector2 vector) => ref vector.As<Vector2, PointF>();

        [SecuritySafeCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static ref Vector2 AsVector2(this ref PointF point) => ref point.As<PointF, Vector2>();

        #endregion

        #region Clipping

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Vector4 ClipF(this Vector4 v)
        {
            // Vector*.Min/Max/Clamp are not reliable in handling NaN: https://github.com/dotnet/runtime/discussions/83683
            // But we can use SSE._mm_min_ps/_mm_max_ps if available, which replaces NaN as we need: https://www.intel.com/content/www/us/en/docs/intrinsics-guide/index.html#text=minps&ig_expand=4918,4521
#if NETCOREAPP3_0_OR_GREATER
            if (Sse.IsSupported)
            {
#if NET8_0_OR_GREATER
                return Sse.Min(Sse.Max(v.AsVector128(), Vector128<float>.Zero), Vector128<float>.One).AsVector4();
#else
                Vector128<float> result = Sse.Min(Sse.Max(v.AsVector128(), Vector128<float>.Zero), OneF);
                return result.AsVector4();
#endif
            }
#endif
#if NET9_0_OR_GREATER
            // there is no ClampNumber, but MinNumber and MaxNumber handle NaN as needed
            return Vector4.MinNumber(Vector4.MaxNumber(v, Vector4.Zero), Vector4.One);
#else
            // The non-accelerated fallback version that returns 0f for NaN
            return new Vector4(v.X.ClipF(), v.Y.ClipF(), v.Z.ClipF(), v.W.ClipF());
#endif
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Vector3 ClipF(this Vector3 v)
        {
            // Vector*.Min/Max/Clamp are not reliable in handling NaN: https://github.com/dotnet/runtime/discussions/83683
            // But we can use SSE._mm_min_ps/_mm_max_ps if available, which replaces NaN as we need: https://www.intel.com/content/www/us/en/docs/intrinsics-guide/index.html#text=minps&ig_expand=4918,4521
#if NETCOREAPP3_0_OR_GREATER
            if (Sse.IsSupported)
            {
                Vector128<float> result = Sse.Min(Sse.Max(v.AsVector128(), Vector128<float>.Zero), OneF);
                return result.AsVector3();
            }
#endif
#if NET9_0_OR_GREATER
            // there is no ClampNumber, but MinNumber and MaxNumber handle NaN as needed
            return Vector3.MinNumber(Vector3.MaxNumber(v, Vector3.Zero), Vector3.One);
#else
            // The non-accelerated fallback version that returns 0f for NaN
            return new Vector3(v.X.ClipF(), v.Y.ClipF(), v.Z.ClipF());
#endif
        }

#if NETCOREAPP3_0_OR_GREATER
        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Vector128<float> ClipF(this Vector128<float> v)
        {
            // SSE._mm_min_ps/_mm_max_ps are reliable in replacing NaN values: https://www.intel.com/content/www/us/en/docs/intrinsics-guide/index.html#text=minps&ig_expand=4918,4521
            if (Sse41.IsSupported)
                return Sse.Min(Sse.Max(v, Vector128<float>.Zero), OneF);

            Debug.Fail("Do not call this method when intrinsics are not supported.");
#if NET9_0_OR_GREATER
            // There is no ClampNumber, but MinNumber and MaxNumber handle NaN as needed
            return Vector128.MinNumber(Vector128.MaxNumber(v, Vector128<float>.Zero), OneF);
#else
            return Vector128.Create(v.GetElement(0).ClipF(),
                v.GetElement(1).ClipF(),
                v.GetElement(2).ClipF(),
                v.GetElement(3).ClipF());
#endif
        }
#endif

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Vector4 Clip(this Vector4 v, Vector4 min, Vector4 max)
        {
            // Vector*.Min/Max/Clamp are not reliable in handling NaN: https://github.com/dotnet/runtime/discussions/83683
            // But we can use SSE._mm_min_ps/_mm_max_ps if available, which replaces NaN as we need: https://www.intel.com/content/www/us/en/docs/intrinsics-guide/index.html#text=minps&ig_expand=4918,4521
#if NETCOREAPP3_0_OR_GREATER
            if (Sse.IsSupported)
            {
#if NET5_0_OR_GREATER
                return Sse.Min(Sse.Max(v.AsVector128(), min.AsVector128()), max.AsVector128()).AsVector4();
#else
                Vector128<float> result = Sse.Min(Sse.Max(v.AsVector128(), min.AsVector128()), max.AsVector128());
                return result.AsVector4();
#endif
            }
#endif
#if NET9_0_OR_GREATER
            // there is no ClampNumber, but MinNumber and MaxNumber handle NaN as needed
            return Vector4.MinNumber(Vector4.MaxNumber(v, min), max);
#else
            // The non-accelerated fallback version that returns 0f for NaN
            return new Vector4(v.X.Clip(min.X, max.X), v.Y.Clip(min.Y, max.Y), v.Z.Clip(min.Z, max.Z), v.W.Clip(min.W, max.W));
#endif
        }

#if NETCOREAPP3_0_OR_GREATER
        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Vector128<int> Clip(this Vector128<int> v, Vector128<int> min, Vector128<int> max)
        {
            if (Sse41.IsSupported)
                return Sse41.Min(Sse41.Max(v, min), max);

            Debug.Fail("It is not expected to call this method when SSE 4.1 intrinsics are not supported.");
#if NET9_0_OR_GREATER
            return Vector128.Clamp(v, min, max);
#else
            return Vector128.Create(v.GetElement(0).Clip(min.GetElement(0), max.GetElement(0)),
                v.GetElement(1).Clip(min.GetElement(1), max.GetElement(1)),
                v.GetElement(2).Clip(min.GetElement(2), max.GetElement(2)),
                v.GetElement(3).Clip(min.GetElement(3), max.GetElement(3)));
#endif
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Vector128<float> Clip(this Vector128<float> v, Vector128<float> min, Vector128<float> max)
        {
            // SSE._mm_min_ps/_mm_max_ps are reliable in replacing NaN values: https://www.intel.com/content/www/us/en/docs/intrinsics-guide/index.html#text=minps&ig_expand=4918,4521
            if (Sse.IsSupported)
                return Sse.Min(Sse.Max(v, min), max);

            Debug.Fail("It is not expected to call this method when SSE intrinsics are not supported.");
#if NET9_0_OR_GREATER
            // There is no ClampNumber, but MinNumber and MaxNumber handle NaN as needed
            return Vector128.MinNumber(Vector128.MaxNumber(v, min), max);
#else
            return Vector128.Create(v.GetElement(0).Clip(min.GetElement(0), max.GetElement(0)),
                v.GetElement(1).Clip(min.GetElement(1), max.GetElement(1)),
                v.GetElement(2).Clip(min.GetElement(2), max.GetElement(2)),
                v.GetElement(3).Clip(min.GetElement(3), max.GetElement(3)));
#endif
        }
#endif

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Vector3 Clip(this Vector3 v, Vector3 min, Vector3 max)
        {
            // Vector*.Min/Max/Clamp are not reliable in handling NaN: https://github.com/dotnet/runtime/discussions/83683
            // But we can use SSE._mm_min_ps/_mm_max_ps if available, which replaces NaN as we need: https://www.intel.com/content/www/us/en/docs/intrinsics-guide/index.html#text=minps&ig_expand=4918,4521
#if NETCOREAPP3_0_OR_GREATER
            if (Sse.IsSupported)
            {
#if NET5_0_OR_GREATER
                return Sse.Min(Sse.Max(v.AsVector128(), min.AsVector128()), max.AsVector128()).AsVector3();
#else
                Vector128<float> result = Sse.Min(Sse.Max(v.AsVector128(), min.AsVector128()), max.AsVector128());
                return result.AsVector3();
#endif
            }
#endif
#if NET9_0_OR_GREATER
            // there is no ClampNumber, but MinNumber and MaxNumber handle NaN as needed
            return Vector3.MinNumber(Vector3.MaxNumber(v, min), max);
#else
            // The non-accelerated fallback version that returns 0f for NaN
            return new Vector3(v.X.Clip(min.X, max.X), v.Y.Clip(min.Y, max.Y), v.Z.Clip(min.Z, max.Z));
#endif
        }

        #endregion

        #region Math

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Vector4 RoundTo(this Vector4 vector, float smallestUnit)
            => (vector / smallestUnit + new Vector4(0.5f)).Floor() * smallestUnit;

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Vector4 Floor(this Vector4 vector)
        {
#if NETCOREAPP3_0_OR_GREATER
            if (Sse41.IsSupported)
            {
#if NET5_0_OR_GREATER
                return Sse41.Floor(vector.AsVector128()).AsVector4();
#else
                Vector128<float> result = Sse41.Floor(vector.AsVector128());
                return result.AsVector4();
#endif
            }
#endif
#if NET7_0_OR_GREATER
            if (Vector128.IsHardwareAccelerated)
                return Vector128.Floor(vector.AsVector128()).AsVector4();
#endif
            return new Vector4(MathF.Floor(vector.X), MathF.Floor(vector.Y), MathF.Floor(vector.Z), MathF.Floor(vector.W));
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Vector2 Div(this Vector2 vector, float value)
        {
#if NETCOREAPP3_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
            return vector / value;
#else
            // Vector division with scalar is broken near epsilon in .NET Core 2.x and in .NET Framework because
            // they use one division and 2 multiplications with reciprocal, which may produce NaN and infinite results
            return vector / new Vector2(value);
#endif
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Vector3 Div(this Vector3 vector, float value)
        {
#if NETCOREAPP3_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
            return vector / value;
#else
            // Vector division with scalar is broken near epsilon in .NET Core 2.x and in .NET Framework because
            // they use one division and 3 multiplications with reciprocal, which may produce NaN and infinite results
            return vector / new Vector3(value);
#endif
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Vector4 Div(this Vector4 vector, float value)
        {
#if NETCOREAPP3_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
            return vector / value;
#else
            // Vector division with scalar is broken near epsilon in .NET Core 2.x and in .NET Framework because
            // they use one division and 4 multiplications with reciprocal, which may produce NaN and infinite results
            return vector / new Vector4(value);
#endif
        }

#if NETCOREAPP3_0_OR_GREATER
        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Vector128<float> Pow(this Vector128<float> value, float power)
        {
#if NET11_0_OR_GREATER
#error Check if there is already a (correctly working) Vector128.Pow
            // See also here: https://github.com/dotnet/runtime/issues/93513#issuecomment-2226781888
            // Also, check if it works correctly - https://github.com/dotnet/runtime/issues/100535, or is it broken, like in Tensors: https://github.com/dotnet/runtime/blob/main/src/libraries/System.Numerics.Tensors/src/System/Numerics/Tensors/netcore/TensorPrimitives.Pow.cs#L70

            if (Vector128.IsHardwareAccelerated)
                return Vector128.Pow(value, Vector128.Create(power));
#elif NET9_0_OR_GREATER
            // In .NET 9/10 there is no Vector128.Pow, but we can use the already existing Vector128.Exp and Vector128.Log, which is faster than our custom implementation - see also the PowTest in PerformanceTests.
            // It is not as accurate as my full intrinsics implementation for .NET Core 3+, but is generally faster than that (when power is a fractional value).
            if (Vector128.IsHardwareAccelerated)
            {
                // The comments below contain the scalar logic for better readability. See also Pow_0_ByMathExpLog in PerformanceTests for the full scalar implementation.
                Vector128<float> p = Vector128.Create(power);

                // The most common case: value > 0f. Btw, this is the only case handled in tensors: https://github.com/dotnet/runtime/issues/100535
                // if (value > 0f)
                //     return MathF.Exp(power * MathF.Log(value));
                Vector128<float> mask = Vector128.GreaterThan(value, Vector128<float>.Zero);
                Vector128<float> result = mask.AsUInt32() == Vector128<uint>.Zero
                    ? Vector128<float>.Zero
                    : Vector128.Exp(p * Vector128.Log(value));

                // happy path: all values are > 0f
                if (mask.AsUInt32() == Vector128<uint>.AllBitsSet)
                    return result;

                // value < 0f: Only integer (and infinity) powers are defined.

                // if (value < 0f)
                // {
                //     return (power % 2f) switch
                //     {
                //         0f => MathF.Exp(power * MathF.Log(-value)),
                //         1f or -1f => -MathF.Exp(power * MathF.Log(-value)),
                //         _ => power switch
                //         {
                //             Single.PositiveInfinity => value < -1f ? Single.PositiveInfinity : 0f,
                //             Single.NegativeInfinity => value < -1f ? 0f : Single.PositiveInfinity,
                //             _ => Single.NaN
                //         }
                //     };
                // }
                mask = Vector128.LessThan(value, Vector128<float>.Zero);
                if (mask.AsUInt32() != Vector128<uint>.Zero)
                {
                    Vector128<float> res = (power % 2f) switch
                    {
                        0f => Vector128.Exp(p * Vector128.Log(-value)),
                        1f or -1f => -Vector128.Exp(p * Vector128.Log(-value)),
                        _ => power switch
                        {
                            Single.PositiveInfinity => Vector128.ConditionalSelect(Vector128.LessThan(value, Vector128.Create(-1f)), PositiveInfinityF, Vector128<float>.Zero),
                            Single.NegativeInfinity => Vector128.ConditionalSelect(Vector128.LessThan(value, Vector128.Create(-1f)), Vector128<float>.Zero, PositiveInfinityF),
                            _ => NaN128F // fractional power of a negative number: it is defined only in complex numbers, so returning NaN here
                        }
                    };

                    result = Vector128.ConditionalSelect(mask, res, result);
                }

                // if (value == 0f)
                // {
                //     return power switch
                //     {
                //         > 0f => 0f,
                //         < 0f => Single.PositiveInfinity,
                //         0f => 1f,
                //         _ => Single.NaN
                //     };
                // }
                mask = Vector128.Equals(value, Vector128<float>.Zero);
                if (mask.AsUInt32() != Vector128<uint>.Zero)
                {
                    Vector128<float> res = power switch
                    {
                        > 0f => Vector128<float>.Zero,
                        < 0f => PositiveInfinityF,
                        0f => OneF,
                        _ => NaN128F
                    };

                    result = Vector128.ConditionalSelect(mask, res, result);
                }

                // if (value is NaN)
                //     return power is 0f ? 1f : Single.NaN;
                mask = Vector128.IsNaN(value);
                if (mask.AsUInt32() != Vector128<uint>.Zero)
                    result = Vector128.ConditionalSelect(mask, power is 0f ? Vector128<float>.One : NaN128F, result);

                return result;
            }
#endif

            // Custom intrinsics implementation
            // Note: could be in an #else, because if Vector128.IsHardwareAccelerated is false, Sse41.IsSupported is likely also false.
            // Still, leaving it like this as it is optimized away by the JIT if it cannot be used, so there is no runtime overhead.*
            if (Sse41.IsSupported)
                return value.PowIntrinsics(power);

            // Fallback to scalar implementation
            return Vector128.Create(
                MathF.Pow(value.GetElement(0), power),
                MathF.Pow(value.GetElement(1), power),
                MathF.Pow(value.GetElement(2), power),
                MathF.Pow(value.GetElement(3), power));
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Vector256<float> Pow(this Vector256<float> value, float power)
        {
            // See more comments in the Vector128 overload
#if NET11_0_OR_GREATER
#error Check if there is already a (correctly working) Vector256.Pow
            // See also here: https://github.com/dotnet/runtime/issues/93513#issuecomment-2226781888
            // Also, check if it works correctly - https://github.com/dotnet/runtime/issues/100535, or is it broken, like in Tensors: https://github.com/dotnet/runtime/blob/main/src/libraries/System.Numerics.Tensors/src/System/Numerics/Tensors/netcore/TensorPrimitives.Pow.cs#L83

            if (Vector256.IsHardwareAccelerated)
                return Vector256.Pow(value, Vector256.Create(power));
#elif NET9_0_OR_GREATER
            if (Vector256.IsHardwareAccelerated)
            {
                Vector256<float> p = Vector256.Create(power);

                // value > 0f
                Vector256<float> mask = Vector256.GreaterThan(value, Vector256<float>.Zero);
                Vector256<float> result = mask.AsUInt32() == Vector256<uint>.Zero
                    ? Vector256<float>.Zero
                    : Vector256.Exp(p * Vector256.Log(value));

                // happy path: all values are > 0f
                if (mask.AsUInt32() == Vector256<uint>.AllBitsSet)
                    return result;

                // value < 0f: Only integer (and infinity) powers are defined.
                mask = Vector256.LessThan(value, Vector256<float>.Zero);
                if (mask.AsUInt32() != Vector256<uint>.Zero)
                {
                    Vector256<float> res = (power % 2f) switch
                    {
                        0f => Vector256.Exp(p * Vector256.Log(-value)),
                        1f or -1f => -Vector256.Exp(p * Vector256.Log(-value)),
                        _ => power switch
                        {
                            Single.PositiveInfinity => Vector256.ConditionalSelect(Vector256.LessThan(value, Vector256.Create(-1f)), PositiveInfinity256F, Vector256<float>.Zero),
                            Single.NegativeInfinity => Vector256.ConditionalSelect(Vector256.LessThan(value, Vector256.Create(-1f)), Vector256<float>.Zero, PositiveInfinity256F),
                            _ => NaN256F // fractional power of a negative number: it is defined only in complex numbers, so returning NaN here
                        }
                    };

                    result = Vector256.ConditionalSelect(mask, res, result);
                }

                // value == 0f
                mask = Vector256.Equals(value, Vector256<float>.Zero);
                if (mask.AsUInt32() != Vector256<uint>.Zero)
                {
                    Vector256<float> res = power switch
                    {
                        > 0f => Vector256<float>.Zero,
                        < 0f => PositiveInfinity256F,
                        0f => One256F,
                        _ => NaN256F
                    };

                    result = Vector256.ConditionalSelect(mask, res, result);
                }

                // if (value is NaN)
                //     return power is 0f ? 1f : Single.NaN;
                mask = Vector256.IsNaN(value);
                if (mask.AsUInt32() != Vector256<uint>.Zero)
                    result = Vector256.ConditionalSelect(mask, power is 0f ? One256F : NaN256F, result);

                return result;
            }
#endif

            // Custom intrinsics implementation
            // Note: could be in an #else, because if Vector256.IsHardwareAccelerated is false, Avx2.IsSupported is likely also false.
            // Still, leaving it like this as it is optimized away by the JIT if it cannot be used, so there is no runtime overhead.*
            if (Avx2.IsSupported)
                return value.PowIntrinsics(power);

            // Fallback to the Vector128 overload
            return Vector256.Create(value.GetLower().Pow(power), value.GetUpper().Pow(power));
        }
#endif

#if NET8_0_OR_GREATER
        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Vector512<float> Pow(this Vector512<float> value, float power)
        {
            // See more comments in the Vector128 overload
#if NET11_0_OR_GREATER
#error Check if there is already a (correctly working) Vector256.Pow
            // See also here: https://github.com/dotnet/runtime/issues/93513#issuecomment-2226781888
            // Also, check if it works correctly - https://github.com/dotnet/runtime/issues/100535, or is it broken, like in Tensors: https://github.com/dotnet/runtime/blob/main/src/libraries/System.Numerics.Tensors/src/System/Numerics/Tensors/netcore/TensorPrimitives.Pow.cs#L83

            if (Vector512.IsHardwareAccelerated)
                return Vector512.Pow(value, Vector512.Create(power));
#elif NET9_0_OR_GREATER
            if (Vector512.IsHardwareAccelerated)
            {
                Vector512<float> p = Vector512.Create(power);

                // value > 0f
                Vector512<float> mask = Vector512.GreaterThan(value, Vector512<float>.Zero);
                Vector512<float> result = mask.AsUInt32() == Vector512<uint>.Zero
                    ? Vector512<float>.Zero
                    : Vector512.Exp(p * Vector512.Log(value));

                // happy path: all values are > 0f
                if (mask.AsUInt32() == Vector512<uint>.AllBitsSet)
                    return result;

                // value < 0f: Only integer (and infinity) powers are defined.
                mask = Vector512.LessThan(value, Vector512<float>.Zero);
                if (mask.AsUInt32() != Vector512<uint>.Zero)
                {
                    Vector512<float> res = (power % 2f) switch
                    {
                        0f => Vector512.Exp(p * Vector512.Log(-value)),
                        1f or -1f => -Vector512.Exp(p * Vector512.Log(-value)),
                        _ => power switch
                        {
                            Single.PositiveInfinity => Vector512.ConditionalSelect(Vector512.LessThan(value, Vector512.Create(-1f)), Vector512.Create(Single.PositiveInfinity), Vector512<float>.Zero),
                            Single.NegativeInfinity => Vector512.ConditionalSelect(Vector512.LessThan(value, Vector512.Create(-1f)), Vector512<float>.Zero, Vector512.Create(Single.PositiveInfinity)),
                            _ => Vector512.Create(Single.NaN) // fractional power of a negative number: it is defined only in complex numbers, so returning NaN here
                        }
                    };

                    result = Vector512.ConditionalSelect(mask, res, result);
                }

                // value == 0f
                mask = Vector512.Equals(value, Vector512<float>.Zero);
                if (mask.AsUInt32() != Vector512<uint>.Zero)
                {
                    Vector512<float> res = power switch
                    {
                        > 0f => Vector512<float>.Zero,
                        < 0f => Vector512.Create(Single.PositiveInfinity),
                        0f => Vector512<float>.One,
                        _ => Vector512.Create(Single.NaN)
                    };

                    result = Vector512.ConditionalSelect(mask, res, result);
                }

                // if (value is NaN)
                //     return power is 0f ? 1f : Single.NaN;
                mask = Vector512.IsNaN(value);
                if (mask.AsUInt32() != Vector512<uint>.Zero)
                    result = Vector512.ConditionalSelect(mask, power is 0f ? Vector512<float>.One : Vector512.Create(Single.NaN), result);

                return result;
            }
#endif

            // Fallback to the Vector256 overload (no custom intrinsics implementation for Vector512 yet)
            return Vector512.Create(value.GetLower().Pow(power), value.GetUpper().Pow(power));
        }
#endif

        #endregion

        #region Coloring

        [SecuritySafeCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static ref RgbF AsRgbF(this ref Vector3 vector) => ref vector.As<Vector3, RgbF>();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Color32 ToColor32(this Vector3 vector) => vector.AsRgbF().ToColor32();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static float GetBrightness(this Vector3 vector) => vector.AsRgbF().GetBrightness();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static bool TolerantIsZero(this Vector3 vector) => vector.AsRgbF().TolerantIsZero();

        #endregion

        #region Intrinsics
#if NETCOREAPP3_0_OR_GREATER

        internal static Vector128<float> IsNaN(this Vector128<float> value) => Sse.IsSupported
            ? Sse.CompareUnordered(value, value)
#if NET9_0_OR_GREATER
            : Vector128.IsNaN(value);
#else
            : Vector128.Create(Single.IsNaN(value.GetElement(0)) ? allBitsSetF : 0f,
                Single.IsNaN(value.GetElement(1)) ? allBitsSetF : 0f,
                Single.IsNaN(value.GetElement(2)) ? allBitsSetF : 0f,
                Single.IsNaN(value.GetElement(3)) ? allBitsSetF : 0f);
#endif

        internal static Vector256<float> IsNaN(this Vector256<float> value)
        {
            Debug.Assert(Avx.IsSupported, "Expected to be called when AVX is supported. Otherwise, add fallback paths like in the Vector128 version.");
            return Avx.Compare(value, value, FloatComparisonMode.UnorderedNonSignaling);
        }

        internal static Vector128<float> Or(this Vector128<float> left, Vector128<float> right) => Sse.IsSupported
            ? Sse.Or(left, right)
#if NET7_0_OR_GREATER
            : Vector128.BitwiseOr(left, right);
#else
            : Vector128.Create(left.AsUInt64().GetElement(0) | right.AsUInt64().GetElement(0),
                left.AsUInt64().GetElement(1) | right.AsUInt64().GetElement(1)).AsSingle();
#endif

        internal static Vector128<float> Negate(this Vector128<float> value)
        {
            Debug.Assert(Sse.IsSupported, "Expected to be called when SSE is supported.");
            return Sse.Xor(value, NegativeZeroF); // flipping sign bit
        }

        internal static Vector256<float> Negate(this Vector256<float> value)
        {
            Debug.Assert(Avx.IsSupported, "Expected to be called when AVX is supported.");
            return Avx.Xor(value, NegativeZero256F); // flip sign bit
        }

        /// <summary>
        /// This is a custom SSE-based implementation of the power function for Vector128.
        /// It handles more or less everything like MathF.Pow - see more in PowTest in UnitTests/PerformanceTests.
        /// I used the same approach as in KGySoft.CoreLibraries for decimals: calculating the integer part first, which is both faster and more accurate.
        /// As decimal has no infinity and NaN values, those cases have also been included here. See also https://github.com/koszeggy/KGySoft.CoreLibraries/blob/master/KGySoft.CoreLibraries/CoreLibraries/_Extensions/DecimalExtensions.cs
        /// Note: here we utilize that power is the same for all vector elements. I created a more general version that supports vector powers as well, but it is more complex and slower. See Pow_2_Vector128Full in PerformanceTests for that version.
        /// </summary>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Vector128<float> PowIntrinsics(this Vector128<float> value, float power)
        {
            #region Local Methods

            // Calculates value^power where power is an integer.
            [MethodImpl(MethodImpl.AggressiveInlining)]
            static Vector128<float> PowI(Vector128<float> value, int power)
            {
                if (power < 0)
                {
                    power = -power;

                    // value = 1f / value;
                    value = Sse.Divide(OneF, value); //Sse.Reciprocal(value); - Reciprocal has a terrible precision, e.g. for 2f it returns 0.49987793f
                }
                else if (power == 0)
                    return OneF;

                Vector128<float> result = OneF;
                Vector128<float> current = value;

                while (true)
                {
                    if ((power & 1) == 1)
                    {
                        // result *= current;
                        result = Sse.Multiply(result, current);
                        if (power == 1)
                            return result;
                    }

                    power >>= 1;
                    if (power > 0)
                    {
                        // current *= current;
                        current = Sse.Multiply(current, current);
                    }
                }
            }

            // Calculates the natural logarithm (base e) of the given value.
            // Note: I could have used the same approach as MS did in Vector128 and in tensors, but that one is less accurate,
            // and falls under a different license, see https://github.com/dotnet/runtime/blob/e8812e7419db9137f20b990786a53ed71e27e11e/src/libraries/System.Private.CoreLib/src/System/Runtime/Intrinsics/VectorMath.cs#L1038
            // So this is the vectorized version of the private DecimalExtensions.LogE method in KGySoft.CoreLibraries: https://github.com/koszeggy/KGySoft.CoreLibraries/blob/master/KGySoft.CoreLibraries/CoreLibraries/_Extensions/DecimalExtensions.cs
            [MethodImpl(MethodImpl.AggressiveInlining)]
            static Vector128<float> LogE(Vector128<float> value)
            {
                // invalidMask = value < 0f || IsNaN(value)
                // infinity = value == 0f || IsPositiveInfinity(value)
                Vector128<float> invalidMask = Sse.Or(Sse.CompareLessThan(value, Vector128<float>.Zero), value.IsNaN());
                Vector128<float> infinityMask = Sse.CompareEqual(value, PositiveInfinityF);
                Vector128<float> zeroMask = Sse.CompareEqual(value, Vector128<float>.Zero);
                Vector128<float> zeroOrInfinityOrInvalid = Sse.Or(zeroMask, Sse.Or(infinityMask, invalidMask));

                Vector128<int> count = Vector128<int>.Zero;

                // Scalar version for reference:
                // while (value >= 1f && !Single.IsPositiveInfinity(value))
                // {
                //     value *= 1 / MathF.E;
                //     count += 1;
                // }

                // We could simply implement it like this, but it applies the mask in every iteration:
                //while ((mask = Sse.AndNot(infinityMask, Sse.CompareGreaterThanOrEqual(value, OneF)).AsInt32()) != Vector128<int>.Zero)
                //{
                //    value = Sse41.BlendVariable(value, Sse.Multiply(InvE128, value), mask.AsSingle());
                //    count = Sse2.Add(count, Sse2.And(mask, OneI32));
                //}

                Vector128<int> mask = Sse.AndNot(infinityMask, Sse.CompareGreaterThanOrEqual(value, OneF)).AsInt32();
                if (!mask.Equals(Vector128<int>.Zero))
                {
                    Vector128<float> v = Sse.And(value, mask.AsSingle());

                    // shortcut for one iteration (when 1 <= value < e)
                    if (Sse.CompareGreaterThan(v, E128).AsUInt32().Equals(Vector128<uint>.Zero))
                    {
                        value = Sse41.BlendVariable(value, Sse.Multiply(InvE128, value), mask.AsSingle());
                        count = Sse2.Add(count, Sse2.And(mask, OneI32));
                    }
                    else
                    {
                        v = Sse41.BlendVariable(NaN128F, v, mask.AsSingle());
                        Vector128<int> c = Vector128<int>.Zero;
                        do
                        {
                            v = Sse.Multiply(InvE128, v);
                            c = Sse2.Add(c, OneI32);
                        } while (!Sse.CompareGreaterThanOrEqual(v, OneF).AsUInt32().Equals(Vector128<uint>.Zero));

                        value = Sse41.BlendVariable(value, v, mask.AsSingle());
                        count = Sse2.Add(count, Sse2.And(mask, c));
                    }
                }

                // Scalar version for reference:
                // while (value <= 1 / MathF.E && !zeroOrInfinityOrInvalid)
                // {
                //     value *= MathF.E;
                //     count -= 1;
                // }

                // We could simply implement it like this, but it applies the mask in every iteration:
                // while ((mask = Sse.AndNot(zeroOrInfinityOrInvalid, Sse.CompareLessThanOrEqual(value, InvE128)).AsInt32()) != Vector128<int>.Zero)
                // {
                //     value = Sse41.BlendVariable(value, Sse.Multiply(E128, value), mask.AsSingle());
                //     count = Sse2.Subtract(count, Sse2.And(mask, OneI32));
                // }

                mask = Sse.AndNot(zeroOrInfinityOrInvalid, Sse.CompareLessThanOrEqual(value, InvE128)).AsInt32();
                if (!mask.Equals(Vector128<int>.Zero))
                {
                    Vector128<float> v = Sse.And(value, mask.AsSingle());

                    // shortcut for one iteration (when invE/e < value <= 1)
                    if (Sse.CompareLessThan(v, InvEPerE128).AsUInt32().Equals(Vector128<uint>.Zero))
                    {
                        value = Sse41.BlendVariable(value, Sse.Multiply(E128, value), mask.AsSingle());
                        count = Sse2.Subtract(count, Sse2.And(mask, OneI32));
                    }
                    else
                    {
                        v = Sse41.BlendVariable(NaN128F, v, mask.AsSingle());
                        Vector128<int> c = Vector128<int>.Zero;
                        do
                        {
                            v = Sse.Multiply(E128, v);
                            c = Sse2.Add(c, OneI32);
                        } while (!Sse.CompareLessThanOrEqual(v, InvE128).AsUInt32().Equals(Vector128<uint>.Zero));

                        value = Sse41.BlendVariable(value, v, mask.AsSingle());
                        count = Sse2.Subtract(count, Sse2.And(mask, c));
                    }
                }

                // value -= 1f;
                value = Sse.Subtract(value, OneF);

                // Going on with Taylor series

                // Scalar version for reference:
                // float result = 0f;
                // float acc = 1f;
                // for (int i = 1; ; i++)
                // {
                //     float prevResult = result;
                //     acc *= -value;
                //     result += acc / i;
                //     if (prevResult == result)
                //         break;
                // }

                Vector128<float> result = Sse.Or(Vector128<float>.Zero, zeroOrInfinityOrInvalid);

                // We could implement it like this, but it would do an unnecessary multiplication and division in the first iteration:
                // Vector128<float> acc = OneF;
                // Vector128<float> negativeValue = Negate(value);
                // for (var i = OneF; ; i = Sse.Add(i, OneF))
                // {
                //     Vector128<uint> prevResult = result.AsUInt32();
                //     acc = Sse.Multiply(acc, negativeValue);
                //     result = Sse.Add(result, Sse.Divide(acc, i));
                //     if (prevResult == result.AsUInt32())
                //         break;
                // }

                // first iteration (i == 1)
                Vector128<uint> prevResult = result.AsUInt32();
                Vector128<float> negativeValue = value.Negate();
                Vector128<float> acc = negativeValue;
                result = Sse.Add(result, acc);

                // further iterations (i >= 2)
                if (!prevResult.Equals(result.AsUInt32()))
                {
                    Vector128<float> i = TwoF;
                    while (true)
                    {
                        prevResult = result.AsUInt32();
                        acc = Sse.Multiply(acc, negativeValue);
                        result = Sse.Add(result, Sse.Divide(acc, i));
                        if (prevResult.Equals(result.AsUInt32()))
                            break;
                        i = Sse.Add(i, OneF);
                    }
                }

                // return count - result;
                result = Sse.Subtract(Sse2.ConvertToVector128Single(count), result);
                result = Sse41.BlendVariable(result, PositiveInfinityF, infinityMask);
                result = Sse41.BlendVariable(result, NegativeInfinityF, zeroMask);
                return Sse.Or(invalidMask, result);
            }

            // Calculates e^power.
            // Note: I could have used the same approach as MS did in Vector128 and in tensors, but that one is less accurate,
            // and falls under a different license, see https://github.com/dotnet/runtime/blob/e8812e7419db9137f20b990786a53ed71e27e11e/src/libraries/System.Private.CoreLib/src/System/Runtime/Intrinsics/VectorMath.cs#L476
            // So this is the vectorized version of the DecimalExtensions.Exp method in KGySoft.CoreLibraries: https://github.com/koszeggy/KGySoft.CoreLibraries/blob/master/KGySoft.CoreLibraries/CoreLibraries/_Extensions/DecimalExtensions.cs
            [MethodImpl(MethodImpl.AggressiveInlining)]
            static Vector128<float> ExpE(Vector128<float> power)
            {
                Vector128<int> integerPart = Vector128<int>.Zero;

                // if (power > 1f)
                // {
                //     if (Single.IsPositiveInfinity(power))
                //         power = Single.PositiveInfinity;
                //     else
                //     {
                //         float diff = MathF.Floor(power);
                //         power -= diff;
                //     }
                   
                //     integerPart += (int)diff;
                // }
                Vector128<float> mask = Sse.CompareGreaterThan(power, OneF);
                if (!mask.AsUInt32().Equals(Vector128<uint>.Zero))
                {
                    Vector128<float> finiteMask = Sse.CompareNotEqual(power, PositiveInfinityF);
                    Vector128<float> diff = Sse41.Floor(Sse.And(power, finiteMask));
                    power = Sse.Subtract(power, Sse.And(diff, mask));
                    integerPart = Sse2.Add(integerPart, Sse2.ConvertToVector128Int32WithTruncation(Sse.And(diff, mask)));
                }

                // else if (power < 0f)
                // {
                //     float diff = MathF.Floor(-power);
                //     if (diff > MaxPreciseIntAsFloat)
                //     {
                //         diff = MaxPreciseIntAsFloat;
                //         power = 0f;
                //     }
                //     else
                //         power += diff;
                //     integerPart -= (int)diff;
                // }
                mask = Sse.CompareLessThan(power, OneF);
                if (!mask.AsUInt32().Equals(Vector128<uint>.Zero))
                {
                    Vector128<float> diff = Sse.And(Sse41.Floor(power.Negate()), mask);
                    Vector128<float> diffTooLargeMask = Sse.CompareGreaterThan(diff, MaxPreciseIntAsFloat);
                    diff = Sse41.BlendVariable(diff, MaxPreciseIntAsFloat, diffTooLargeMask);
                    power = Sse41.BlendVariable(power, Vector128<float>.Zero, diffTooLargeMask);
                    power = Sse.Add(power, Sse.AndNot(diffTooLargeMask, Sse.And(diff, mask)));
                    integerPart = Sse2.Subtract(integerPart, Sse2.ConvertToVector128Int32(Sse.And(diff, mask)));
                }

                // float result = 1f;
                // float acc = 1f;
                // for (int i = 1; ; i++)
                // {
                //     float prevResult = result;
                //     acc *= power / i;
                //     result += acc;
                //     if (prevResult == result)
                //         break;
                // }

                // we could implement it like this, but it would do an unnecessary multiplication and division in the first iteration:
                // Vector128<float> invalidMask = power.IsNaN();
                // Vector128<float> result = Sse.Or(OneF, invalidMask);
                // Vector128<float> acc = OneF;
                // for (Vector128<float> i = OneF; ; i = Sse.Add(i, OneF))
                // {
                //     Vector128<float> prevResult = result;
                //     acc = Sse.Multiply(acc, Sse.Divide(power, i));
                //     result = Sse.Add(result, acc);
                //     if (prevResult.AsUInt32() == result.AsUInt32()) // bitwise comparison to handle NaN and infinities
                //         break;
                // }

                // first iteration (i == 1)
                Vector128<float> invalidMask = power.IsNaN();
                Vector128<float> acc = power;
                Vector128<uint> prevResult = Sse.Or(OneF, invalidMask).AsUInt32();
                Vector128<float> result = Sse.Or(Sse.Add(OneF, acc), invalidMask);

                // further iterations (i >= 2)
                if (!prevResult.Equals(result.AsUInt32()))
                {
                    Vector128<float> i = TwoF;
                    while (true)
                    {
                        prevResult = result.AsUInt32();
                        acc = Sse.Multiply(acc, Sse.Divide(power, i));
                        result = Sse.Add(result, acc);
                        if (prevResult.AsUInt32().Equals(result.AsUInt32())) // bitwise int comparison to handle NaN and infinities
                            break;
                        i = Sse.Add(i, OneF);
                    }
                }

                //if (integerPart != 0)
                //    result *= PowI(MathF.E, integerPart);
                integerPart = Sse2.AndNot(invalidMask.AsInt32(), integerPart);
                if (!integerPart.Equals(Vector128<int>.Zero))
                    result = Sse.Multiply(result, PowE(integerPart));

                return result;
            }

            // Functionally the same as ExpE, but logically similar to PowI, except that here the power is the vector instead of the base.
            [MethodImpl(MethodImpl.AggressiveInlining)]
            static Vector128<float> PowE(Vector128<int> power)
            {
                Vector128<float> value = E128;

                // if (power < 0)
                // {
                //     power = -power;
                //     value = 1f / value;
                // }
                Vector128<int> negativePowerMask = Sse2.CompareLessThan(power, Vector128<int>.Zero);
                if (!negativePowerMask.Equals(Vector128<int>.Zero))
                {
                    power = Sse2.Subtract(Sse2.Xor(power, negativePowerMask), negativePowerMask);
                    Vector128<float> inverseValue = Sse.Divide(OneF, value); //Sse.Reciprocal(value); - Reciprocal has a terrible precision, e.g. for 2f it returns 0.49987793f
                    value = Sse41.BlendVariable(value, inverseValue, negativePowerMask.AsSingle());
                }

                Vector128<float> current = value;
                Vector128<float> result = OneF;

                while (true)
                {
                    // if ((power & 1) == 1)
                    //     result = current * result;
                    Vector128<int> powerOddMask = Sse2.CompareEqual(Sse2.And(power, OneI32), OneI32);
                    if (!powerOddMask.Equals(Vector128<int>.Zero))
                        result = Sse41.BlendVariable(result, Sse.Multiply(result, current), powerOddMask.AsSingle());

                    // power >>= 1;
                    // if (power > 0)
                    //     current *= current;
                    power = Sse2.ShiftRightLogical(power, 1);
                    current = Sse.Multiply(current, current);

                    if (power.Equals(Vector128<int>.Zero))
                        return result;
                }
            }

            #endregion

            Debug.Assert(Sse41.IsSupported, "Expected to be called when SSE4.1 is supported.");
            if (Single.IsNaN(power))
                return NaN128F;

            // FloatExtensions.MaxPreciseIntAsFloat is the largest integer that has exactly the same float representation.
            // If the absolute value of power is larger than that, the result will be either 0 or Infinity anyway.
            // But clipping is needed to avoid issues (e.g. Int32.MaxValue is an odd number, so if value is negative, the sign could be flipped).
            power = power.Clip(FloatExtensions.MinPreciseIntAsFloat, FloatExtensions.MaxPreciseIntAsFloat);

            // Faster if we calculate the result for the integer part fist, and then for the fractional
            float integerPart = MathF.Truncate(power);
            float fracPart = power - integerPart; // without clipping power, it should be: Single.IsPositiveInfinity(power) ? 0f : power - integerPart;
            Vector128<float> result = PowI(value, (int)integerPart); // without clipping power, the cast may turn large even numbers into the odd Int32.MaxValue
            if (fracPart != 0f)
                result = Sse.Multiply(result, ExpE(Sse.Multiply(Vector128.Create(fracPart), LogE(value))));

            return result;
        }

        /// <summary>
        /// This is a custom SSE-based implementation of the power function for Vector256.
        /// See more detailed comments in the Vector128 overload.
        /// </summary>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Vector256<float> PowIntrinsics(this Vector256<float> value, float power)
        {
            #region Local Methods

            [MethodImpl(MethodImpl.AggressiveInlining)]
            static Vector256<float> PowI(Vector256<float> value, int power)
            {
                if (power < 0)
                {
                    power = -power;
                    value = Avx.Divide(One256F, value); //Avx.Reciprocal(value); // Reciprocal has a quite low precision
                }
                else if (power == 0)
                    return One256F;

                Vector256<float> result = One256F;
                Vector256<float> current = value;

                while (true)
                {
                    if ((power & 1) == 1)
                    {
                        result = Avx.Multiply(result, current);
                        if (power == 1)
                            return result;
                    }

                    power >>= 1;
                    if (power > 0)
                        current = Avx.Multiply(current, current);
                }
            }

            [MethodImpl(MethodImpl.AggressiveInlining)]
            static Vector256<float> LogE(Vector256<float> value)
            {
                Vector256<float> invalidMask = Avx.Or(Avx.Compare(value, Vector256<float>.Zero, FloatComparisonMode.OrderedLessThanNonSignaling), value.IsNaN());
                Vector256<float> infinityMask = Avx.Compare(value, PositiveInfinity256F, FloatComparisonMode.OrderedEqualNonSignaling);
                Vector256<float> zeroMask = Avx.Compare(value, Vector256<float>.Zero, FloatComparisonMode.OrderedEqualNonSignaling);
                Vector256<float> zeroOrInfinityOrInvalid = Avx.Or(zeroMask, Avx.Or(infinityMask, invalidMask));

                Vector256<int> count = Vector256<int>.Zero;

                // while value >= 1f
                Vector256<int> mask = Avx.AndNot(infinityMask, Avx.Compare(value, One256F, FloatComparisonMode.OrderedGreaterThanOrEqualNonSignaling)).AsInt32();
                if (!mask.Equals(Vector256<int>.Zero))
                {
                    Vector256<float> v = Avx.And(value, mask.AsSingle());

                    // shortcut for one iteration (when 1 <= value < e)
                    if (Avx.Compare(v, E256, FloatComparisonMode.OrderedGreaterThanNonSignaling).AsUInt32().Equals(Vector256<uint>.Zero))
                    {
                        value = Avx.BlendVariable(value, Avx.Multiply(InvE256, value), mask.AsSingle());
                        count = Avx2.Add(count, Avx2.And(mask, One256I32));
                    }
                    else
                    {
                        v = Avx.BlendVariable(NaN256F, v, mask.AsSingle());
                        Vector256<int> c = Vector256<int>.Zero;
                        do
                        {
                            v = Avx.Multiply(InvE256, v);
                            c = Avx2.Add(c, One256I32);
                        } while (!Avx.Compare(v, One256F, FloatComparisonMode.OrderedGreaterThanOrEqualNonSignaling).AsUInt32().Equals(Vector256<uint>.Zero));

                        value = Avx.BlendVariable(value, v, mask.AsSingle());
                        count = Avx2.Add(count, Avx2.And(mask, c));
                    }
                }

                // while value <= 1
                mask = Avx.AndNot(zeroOrInfinityOrInvalid, Avx.Compare(value, InvE256, FloatComparisonMode.OrderedLessThanOrEqualNonSignaling)).AsInt32();
                if (!mask.Equals(Vector256<int>.Zero))
                {
                    Vector256<float> v = Avx.And(value, mask.AsSingle());

                    // shortcut for one iteration (when invE/e < value <= 1)
                    if (Avx.Compare(v, InvEPerE256, FloatComparisonMode.OrderedLessThanNonSignaling).AsUInt32().Equals(Vector256<uint>.Zero))
                    {
                        value = Avx.BlendVariable(value, Avx.Multiply(E256, value), mask.AsSingle());
                        count = Avx2.Subtract(count, Avx2.And(mask, One256I32));
                    }
                    else
                    {
                        v = Avx.BlendVariable(NaN256F, v, mask.AsSingle());
                        Vector256<int> c = Vector256<int>.Zero;
                        do
                        {
                            v = Avx.Multiply(E256, v);
                            c = Avx2.Add(c, One256I32);
                        } while (!Avx.Compare(v, InvE256, FloatComparisonMode.OrderedLessThanOrEqualNonSignaling).AsUInt32().Equals(Vector256<uint>.Zero));

                        value = Avx.BlendVariable(value, v, mask.AsSingle());
                        count = Avx2.Subtract(count, Avx2.And(mask, c));
                    }
                }

                // value -= 1f;
                value = Avx.Subtract(value, One256F);

                // Going on with Taylor series
                Vector256<float> result = Avx.Or(Vector256<float>.Zero, zeroOrInfinityOrInvalid);

                // first iteration (i == 1)
                Vector256<uint> prevResult = result.AsUInt32();
                Vector256<float> negativeValue = value.Negate();
                Vector256<float> acc = negativeValue;
                result = Avx.Add(result, acc);

                // further iterations (i >= 2)
                if (!prevResult.Equals(result.AsUInt32()))
                {
                    Vector256<float> i = Two256F;
                    while (true)
                    {
                        prevResult = result.AsUInt32();
                        acc = Avx.Multiply(acc, negativeValue);
                        result = Avx.Add(result, Avx.Divide(acc, i));
                        if (prevResult.Equals(result.AsUInt32()))
                            break;
                        i = Avx.Add(i, One256F);
                    }
                }

                // return count - result;
                result = Avx.Subtract(Avx.ConvertToVector256Single(count), result);
                result = Avx.BlendVariable(result, PositiveInfinity256F, infinityMask);
                result = Avx.BlendVariable(result, NegativeInfinity256F, zeroMask);
                return Avx.Or(invalidMask, result);
            }

            [MethodImpl(MethodImpl.AggressiveInlining)]
            static Vector256<float> ExpE(Vector256<float> power)
            {
                Vector256<int> integerPart = Vector256<int>.Zero;

                // power > 1f
                Vector256<float> mask = Avx.Compare(power, One256F, FloatComparisonMode.OrderedGreaterThanNonSignaling);
                if (!mask.AsUInt32().Equals(Vector256<uint>.Zero))
                {
                    Vector256<float> finiteMask = Avx.Compare(power, PositiveInfinity256F, FloatComparisonMode.UnorderedNotEqualNonSignaling);
                    Vector256<float> diff = Avx.Floor(Avx.And(power, finiteMask));
                    power = Avx.Subtract(power, Avx.And(diff, mask));
                    integerPart = Avx2.Add(integerPart, Avx.ConvertToVector256Int32WithTruncation(Avx.And(diff, mask)));
                }

                // power < 0f
                mask = Avx.Compare(power, One256F, FloatComparisonMode.OrderedLessThanNonSignaling);
                if (!mask.AsUInt32().Equals(Vector256<uint>.Zero))
                {
                    Vector256<float> diff = Avx.And(Avx.Floor(power.Negate()), mask);
                    Vector256<float> diffTooLargeMask = Avx.Compare(diff, MaxPreciseIntAsFloat256, FloatComparisonMode.OrderedGreaterThanNonSignaling);
                    diff = Avx.BlendVariable(diff, MaxPreciseIntAsFloat256, diffTooLargeMask);
                    power = Avx.BlendVariable(power, Vector256<float>.Zero, diffTooLargeMask);
                    power = Avx.Add(power, Avx.AndNot(diffTooLargeMask, Avx.And(diff, mask)));
                    integerPart = Avx2.Subtract(integerPart, Avx.ConvertToVector256Int32(Avx.And(diff, mask)));
                }

                // first iteration (i == 1)
                Vector256<float> invalidMask = power.IsNaN();
                Vector256<float> acc = power;
                Vector256<uint> prevResult = Avx.Or(One256F, invalidMask).AsUInt32();
                Vector256<float> result = Avx.Or(Avx.Add(One256F, acc), invalidMask);

                // further iterations (i >= 2)
                if (!prevResult.Equals(result.AsUInt32()))
                {
                    Vector256<float> i = Two256F;
                    while (true)
                    {
                        prevResult = result.AsUInt32();
                        acc = Avx.Multiply(acc, Avx.Divide(power, i));
                        result = Avx.Add(result, acc);
                        if (prevResult.AsUInt32().Equals(result.AsUInt32())) // bitwise int comparison to handle NaN and infinities
                            break;
                        i = Avx.Add(i, One256F);
                    }
                }

                // if (integerPart != 0)
                //     result *= PowI(MathF.E, integerPart);
                integerPart = Avx2.AndNot(invalidMask.AsInt32(), integerPart);
                if (!integerPart.Equals(Vector256<int>.Zero))
                    result = Avx.Multiply(result, PowE(integerPart));

                return result;
            }

            [MethodImpl(MethodImpl.AggressiveInlining)]
            static Vector256<float> PowE(Vector256<int> power)
            {
                Vector256<float> value = E256;
                Vector256<int> negativePowerMask = Avx2.ShiftRightArithmetic(power, 31); // same as CompareLessThan(power, Vector256<int>.Zero), which exists only starting with Avx512F.VL for ints.
                if (!negativePowerMask.Equals(Vector256<int>.Zero))
                {
                    power = Avx2.Subtract(Avx2.Xor(power, negativePowerMask), negativePowerMask);
                    Vector256<float> inverseValue = Avx.Divide(One256F, value); // Avx.Reciprocal(value); - Reciprocal has a terrible precision, e.g. for 2f it returns 0.49987793f
                    value = Avx.BlendVariable(value, inverseValue, negativePowerMask.AsSingle());
                }

                Vector256<float> current = value;
                Vector256<float> result = One256F;

                while (true)
                {
                    // if ((power & 1) == 1)
                    //     result = current * result;
                    Vector256<int> powerOddMask = Avx2.CompareEqual(Avx2.And(power, One256I32), One256I32);
                    if (!powerOddMask.Equals(Vector256<int>.Zero))
                        result = Avx.BlendVariable(result, Avx.Multiply(result, current), powerOddMask.AsSingle());

                    // power >>= 1;
                    // if (power > 0)
                    //     current *= current;
                    power = Avx2.ShiftRightLogical(power, 1);
                    current = Avx.Multiply(current, current);

                    if (power.Equals(Vector256<int>.Zero))
                        return result;
                }
            }

            #endregion

            Debug.Assert(Avx2.IsSupported, "Expected to be called when AVX2 is supported.");
            if (Single.IsNaN(power))
                return NaN256F;

            power = power.Clip(FloatExtensions.MinPreciseIntAsFloat, FloatExtensions.MaxPreciseIntAsFloat);

            // Faster if we calculate the result for the integer part fist, and then for the fractional
            float integerPart = MathF.Truncate(power);
            float fracPart = power - integerPart;
            Vector256<float> result = PowI(value, (int)integerPart);
            if (fracPart != 0f)
                result = Avx.Multiply(result, ExpE(Avx.Multiply(Vector256.Create(fracPart), LogE(value))));

            return result;
        }

#endif
        #endregion

        #endregion
    }
}
#endif