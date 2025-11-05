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
#else
        internal static Vector128<float> OneF => Vector128.Create(1f);
#endif
        internal static Vector128<float> Max8BitF => Vector128.Create(255f);
        internal static Vector128<byte> Max8BitU8 => Vector128.Create(Byte.MaxValue);
        internal static Vector128<int> Max8BitI32 => Vector128.Create(255);
        internal static Vector128<float> Max16BitF => Vector128.Create(65535f);
        internal static Vector128<ushort> Max16BitU16 => Vector128.Create(UInt16.MaxValue);
        internal static Vector128<int> Max16BitI32 => Vector128.Create(65535);
        internal static Vector128<float> HalfF => Vector128.Create(0.5f);
        internal static Vector128<float> AllBitsSetF => Vector128<float>.AllBitsSet;
#elif NETCOREAPP3_0_OR_GREATER
        internal static Vector128<byte> PackLowBytesMask { get; } = Vector128.Create(0, 4, 8, 12, default(byte), default, default, default, default, default, default, default, default, default, default, default);
        internal static Vector128<byte> PackLowWordsMask { get; } = Vector128.Create(0, 1, 4, 5, 8, 9, 12, 13, default(byte), default, default, default, default, default, default, default);
        internal static Vector128<byte> PackHighBytesOfLowWordsMask { get; } = Vector128.Create(1, 5, 9, 13, default(byte), default, default, default, default, default, default, default, default, default, default, default);
        internal static Vector128<byte> PackRgbaAsBgraBytesMask { get; } = Vector128.Create(8, 4, 0, 12, default(byte), default, default, default, default, default, default, default, default, default, default, default);
        internal static Vector128<byte> PackRgbaAsBgraWordsMask { get; } = Vector128.Create(8, 9, 4, 5, 0, 1, 12, 13, default(byte), default, default, default, default, default, default, default);
        internal static Vector128<float> OneF { get; } = Vector128.Create(1f);
        internal static Vector128<float> Max8BitF { get; } = Vector128.Create(255f);
        internal static Vector128<byte> Max8BitU8 { get; } = Vector128.Create(Byte.MaxValue);
        internal static Vector128<int> Max8BitI32 { get; } = Vector128.Create(255);
        internal static Vector128<float> Max16BitF { get; } = Vector128.Create(65535f);
        internal static Vector128<ushort> Max16BitU16 { get; } = Vector128.Create(UInt16.MaxValue);
        internal static Vector128<int> Max16BitI32 { get; } = Vector128.Create(65535);
        internal static Vector128<float> HalfF { get; } = Vector128.Create(0.5f);
        internal static Vector128<float> AllBitsSetF { get; } = Vector128.Create(FloatExtensions.AllBitsSetF);
#endif

#if NET5_0_OR_GREATER
        // In .NET 5.0 and above these perform better as inlined rather than caching a static field
        internal static Vector4 Max8Bit => new Vector4(255f);
        internal static Vector4 Max8BitRecip => new Vector4(1f / 255f);
        internal static Vector4 Max16Bit => new Vector4(65535f);
        internal static Vector4 Max16BitRecip => new Vector4(1f / 65535f);
        internal static Vector4 Half => new Vector4(0.5f);
#elif NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
        internal static Vector4 Max8Bit { get; } = new Vector4(Byte.MaxValue);
        internal static Vector4 Max8BitRecip { get; } = new Vector4(1f / Byte.MaxValue);
        internal static Vector4 Max16Bit { get; } = new Vector4(UInt16.MaxValue);
        internal static Vector4 Max16BitRecip { get; } = new Vector4(1f / UInt16.MaxValue);
        internal static Vector4 Half { get; } = new Vector4(0.5f);
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
        /// <summary>
        /// A vectorized Pow function, utilizing that in this library value is always [0..1], and power is always > 0.
        /// </summary>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Vector128<float> Pow(this Vector128<float> value, float power)
        {
            // NOTE: using || in the Debug.Assert, as we allow unsupported values if at least one element is valid. The caller is responsible for masking out invalid results.
            Debug.Assert((value.GetElement(0) >= 0f || value.GetElement(1) >= 0f || value.GetElement(2) >= 0f || value.GetElement(3) >= 0f) && power > 0,
                $"Unexpected value or power: ({value})^{power}");
#if NET11_0_OR_GREATER
#error Check if there is already a (correctly working) Vector128.Pow // see also https://github.com/dotnet/runtime/issues/93513#issuecomment-2226781888
            if (Vector128.IsHardwareAccelerated)
                return Vector128.Pow(value, Vector128.Create(power));
#elif NET9_0_OR_GREATER
            // In .NET 9/10 there is no Vector128.Pow, but we can use the already existing Vector128.Exp and Vector128.Log.
            // It is not as accurate as MathF.Pow, but the result is the same for 6 decimal digits, which is sufficient for our purposes.
            // NOTE: if value or power could be negative or even NaN, this implementation would not be correct. See all the needed cases in Pow_0_ByMathExpLog/Pow_6_ByVector128ExpLog in the PerformanceTests project.
            if (Vector128.IsHardwareAccelerated)
                return Vector128.Exp(Vector128.Create(power) * Vector128.Log(value));
#endif

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
#error Check if there is already a (correctly working) Vector256.Pow // see also https://github.com/dotnet/runtime/issues/93513#issuecomment-2226781888
            if (Vector256.IsHardwareAccelerated)
                return Vector256.Pow(value, Vector256.Create(power));
#elif NET9_0_OR_GREATER
            if (Vector256.IsHardwareAccelerated)
                return Vector256.Exp(Vector256.Create(power) * Vector256.Log(value));
#endif

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
#error Check if there is already a (correctly working) Vector512.Pow // see also https://github.com/dotnet/runtime/issues/93513#issuecomment-2226781888
            if (Vector512.IsHardwareAccelerated)
                return Vector512.Pow(value, Vector512.Create(power));
#elif NET9_0_OR_GREATER
            if (Vector512.IsHardwareAccelerated)
                return Vector512.Exp(Vector512.Create(power) * Vector512.Log(value));
#endif

            // Fallback to the Vector256 overload
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

        internal static Vector128<float> Or(this Vector128<float> left, Vector128<float> right) => Sse.IsSupported
            ? Sse.Or(left, right)
#if NET7_0_OR_GREATER
            : Vector128.BitwiseOr(left, right);
#else
            : Vector128.Create(left.AsUInt64().GetElement(0) | right.AsUInt64().GetElement(0),
                left.AsUInt64().GetElement(1) | right.AsUInt64().GetElement(1)).AsSingle();
#endif

        internal static Vector128<float> IsNaN(this Vector128<float> value) => Sse.IsSupported
            ? Sse.CompareUnordered(value, value)
#if NET9_0_OR_GREATER
            : Vector128.IsNaN(value);
#else
            : Vector128.Create(Single.IsNaN(value.GetElement(0)) ? FloatExtensions.AllBitsSetF : 0f,
                Single.IsNaN(value.GetElement(1)) ? FloatExtensions.AllBitsSetF : 0f,
                Single.IsNaN(value.GetElement(2)) ? FloatExtensions.AllBitsSetF : 0f,
                Single.IsNaN(value.GetElement(3)) ? FloatExtensions.AllBitsSetF : 0f);
#endif

#endif
        #endregion

        #endregion
    }
}
#endif