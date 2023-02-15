#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: RgbF.cs
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
#if NET5_0_OR_GREATER
using System.Runtime.CompilerServices;
#endif
using System.Runtime.InteropServices;

#if !(NETCOREAPP || NET46_OR_GREATER || NETSTANDARD2_1_OR_GREATER)
using KGySoft.CoreLibraries;
#endif

#endregion

namespace KGySoft.Drawing.Imaging
{
    [StructLayout(LayoutKind.Explicit)]
    internal readonly struct RgbF : IEquatable<RgbF>
    {
        #region Fields

        #region Static Fields

#if NETCOREAPP || NET46_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        private static readonly Vector3 tolerance = new Vector3(1e-6f);
#endif

        #endregion

        #region Instance Fields

        [FieldOffset(0)]
        internal readonly float R;
        [FieldOffset(4)]
        internal readonly float G;
        [FieldOffset(8)]
        internal readonly float B;

#if NETCOREAPP || NET46_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        [FieldOffset(0)]
        internal readonly Vector3 Rgb;
#endif

        #endregion

        #endregion

        #region Properties

#if NETCOREAPP || NET46_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        internal bool TolerantIsZero => Vector3.Max(tolerance, Vector3.Abs(Rgb)) == tolerance;
#else
        internal bool TolerantIsZero => R.TolerantIsZero() && G.TolerantIsZero() && B.TolerantIsZero();
#endif

        #endregion

        #region Operators

        public static bool operator ==(RgbF left, RgbF right) => Equals(left, right);
        public static bool operator !=(RgbF left, RgbF right) => !(left == right);

        public static RgbF operator +(RgbF left, RgbF right)
        {
#if NETCOREAPP || NET46_OR_GREATER || NETSTANDARD2_1_OR_GREATER
            return new RgbF(left.Rgb + right.Rgb);
#else
            return new RgbF(left.R + right.R, left.G + right.G, left.B + right.B);
#endif
        }

        public static ColorF operator +(ColorF left, RgbF right)
        {
#if NETCOREAPP || NET46_OR_GREATER || NETSTANDARD2_1_OR_GREATER
            return new ColorF(new Vector4(left.Rgb + right.Rgb, left.A));
#else
            return new ColorF(left.A, left.R + right.R, left.G + right.G, left.B + right.B);
#endif
        }

        public static RgbF operator -(RgbF left, RgbF right)
        {
#if NETCOREAPP || NET46_OR_GREATER || NETSTANDARD2_1_OR_GREATER
            return new RgbF(left.Rgb - right.Rgb);
#else
            return new RgbF(left.R - right.R, left.G - right.G, left.B - right.B);
#endif
        }

        public static RgbF operator *(RgbF left, float right)
        {
#if NETCOREAPP || NET46_OR_GREATER || NETSTANDARD2_1_OR_GREATER
            return new RgbF(left.Rgb * new Vector3(right));
#else
            return new RgbF(left.R * right, left.G * right, left.B * right);
#endif
        }

        #endregion

        #region Constructors

        internal RgbF(float r, float g, float b)
#if (NETCOREAPP || NET46_OR_GREATER || NETSTANDARD2_1_OR_GREATER) && !NET5_0_OR_GREATER
            : this() // so the compiler does not complain about not initializing the vector field
#endif
        {
#if NET5_0_OR_GREATER
            Unsafe.SkipInit(out this);
#endif
            R = r;
            G = g;
            B = b;
        }

        internal RgbF(float rgb)
#if (NETCOREAPP || NET46_OR_GREATER || NETSTANDARD2_1_OR_GREATER) && !NET5_0_OR_GREATER
            : this() // so the compiler does not complain about not initializing the vector field
#endif
        {
#if NET5_0_OR_GREATER
            Unsafe.SkipInit(out this);
#endif

#if NETCOREAPP || NET46_OR_GREATER || NETSTANDARD2_1_OR_GREATER
            Rgb = new Vector3(rgb);
#else
            R = G = B = rgb;
#endif
        }

#if NETCOREAPP || NET46_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        internal RgbF(Vector3 vector)
#if !NET5_0_OR_GREATER
            : this() // so the compiler does not complain about not initializing RGB fields
#endif
        {
#if NET5_0_OR_GREATER
            Unsafe.SkipInit(out this);
#endif
            Rgb = vector;
        }
#endif

        #endregion

        #region Methods

        public override string ToString() => $"[R={R:N4}; G={G:N4}; B={B:N4}]";

#if NETCOREAPP || NET46_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        public bool Equals(RgbF other) => other.Rgb == Rgb;
#else
        [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator", Justification = "It is intended in Equals")]
        public bool Equals(RgbF other) => other.R == R && other.G == G && other.B == B;
#endif

        public override bool Equals(object? obj) => obj is ColorF other && Equals(other);

#if NETCOREAPP || NET46_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        public override int GetHashCode() => Rgb.GetHashCode();
#else
        public override int GetHashCode() => (R, G, B).GetHashCode();
#endif

        #endregion
    }
}
