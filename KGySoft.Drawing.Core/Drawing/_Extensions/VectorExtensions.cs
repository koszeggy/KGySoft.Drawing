#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: VectorExtensions.cs
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

#endregion

namespace KGySoft.Drawing
{
    internal static class VectorExtensions
    {
        #region Methods

#if NETCOREAPP3_0
        // NOTE: Actually these would perform better in .NET Core 3.x if they were this ref + ref return like AsPointF/AsVector2.
        // The .NET 5+ counterparts are intrinsics, so they are faster even in .NET 5 where value-in + value-return was still slower (see VectorCastTests in PerformanceTests).
        internal static Vector128<float> AsVector128(this Vector3 v) => new Vector4(v, default).AsVector128();
        internal static Vector128<float> AsVector128(this Vector4 v) => Unsafe.As<Vector4, Vector128<float>>(ref v);
        internal static Vector3 AsVector3(this Vector128<float> v) => Unsafe.As<Vector128<float>, Vector3>(ref v);
        internal static Vector4 AsVector4(this Vector128<float> v) => Unsafe.As<Vector128<float>, Vector4>(ref v);
#endif

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
                return Sse.Min(Sse.Max(v.AsVector128(), Vector128<float>.Zero), Vector128.Create(1f)).AsVector4();
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
#if NET8_0_OR_GREATER
                return Sse.Min(Sse.Max(v.AsVector128(), Vector128<float>.Zero), Vector128<float>.One).AsVector3();
#else
                return Sse.Min(Sse.Max(v.AsVector128(), Vector128<float>.Zero), Vector128.Create(1f)).AsVector3();
#endif
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

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Vector4 Clip(this Vector4 v, Vector4 min, Vector4 max)
        {
            // Vector*.Min/Max/Clamp are not reliable in handling NaN: https://github.com/dotnet/runtime/discussions/83683
            // But we can use SSE._mm_min_ps/_mm_max_ps if available, which replaces NaN as we need: https://www.intel.com/content/www/us/en/docs/intrinsics-guide/index.html#text=minps&ig_expand=4918,4521
#if NETCOREAPP3_0_OR_GREATER
            if (Sse.IsSupported)
                return Sse.Min(Sse.Max(v.AsVector128(), min.AsVector128()), max.AsVector128()).AsVector4();
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
        internal static ref PointF AsPointF(this ref Vector2 vector) => ref Unsafe.As<Vector2, PointF>(ref vector);
#else
        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static unsafe ref PointF AsPointF(this ref Vector2 vector)
        {
            fixed (Vector2* p = &vector)
                return ref *(PointF*)p;
        }
#endif

#if NETCOREAPP3_0_OR_GREATER
        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static ref Vector2 AsVector2(this ref PointF point) => ref Unsafe.As<PointF, Vector2>(ref point);
#else
        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static unsafe ref Vector2 AsVector2(this ref PointF point)
        {
            fixed (PointF* p = &point)
                return ref *(Vector2*)p;
        }
#endif

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Vector4 RoundTo(this Vector4 vector, float smallestUnit)
            => (vector / smallestUnit + new Vector4(0.5f)).Floor() * smallestUnit;

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Vector4 Floor(this Vector4 vector)
        {
#if NETCOREAPP3_0_OR_GREATER
            if (Sse41.IsSupported)
                return Sse41.Floor(vector.AsVector128()).AsVector4();
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

        #endregion
    }
}
#endif