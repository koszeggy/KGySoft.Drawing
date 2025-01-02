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
                return Sse.Min(Sse.Max(v.AsVector128(), Vector128<float>.Zero), Vector128.Create(1f)).AsVector4();
#endif
            // The non-accelerated fallback version that returns 0f for NaN
            return new Vector4(v.X.ClipF(), v.Y.ClipF(), v.Z.ClipF(), v.W.ClipF());
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Vector3 ClipF(this Vector3 v)
        {
            // Vector*.Min/Max/Clamp are not reliable in handling NaN: https://github.com/dotnet/runtime/discussions/83683
            // But we can use SSE._mm_min_ps/_mm_max_ps if available, which replaces NaN as we need: https://www.intel.com/content/www/us/en/docs/intrinsics-guide/index.html#text=minps&ig_expand=4918,4521
#if NETCOREAPP3_0_OR_GREATER
            if (Sse.IsSupported)
                return Sse.Min(Sse.Max(v.AsVector128(), Vector128<float>.Zero), Vector128.Create(1f)).AsVector3();
#endif
            // The non-accelerated fallback version that returns 0f for NaN
            return new Vector3(v.X.ClipF(), v.Y.ClipF(), v.Z.ClipF());
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
            // The non-accelerated fallback version that returns 0f for NaN
            return new Vector4(v.X.Clip(min.X, max.X), v.Y.Clip(min.Y, max.Y), v.Z.Clip(min.Z, max.Z), v.W.Clip(min.W, max.W));
        }

#if NETCOREAPP3_0_OR_GREATER
        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static PointF AsPointF(this Vector2 vector) => Unsafe.As<Vector2, PointF>(ref vector);
#else
        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static unsafe PointF AsPointF(this Vector2 vector) => *(PointF*)&vector;
#endif

        #endregion
    }
}
#endif