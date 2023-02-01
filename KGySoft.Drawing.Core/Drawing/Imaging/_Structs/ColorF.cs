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

#if NETCOREAPP || NET46_OR_GREATER || NETSTANDARD2_1_OR_GREATER
using System.Numerics;
#endif
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#endregion

namespace KGySoft.Drawing.Imaging
{
    [StructLayout(LayoutKind.Explicit)]
    internal readonly struct ColorF : IEquatable<ColorF>
    {
        #region Fields

        #region Public Fields

        [FieldOffset(0)]
        public readonly float R;

        [FieldOffset(4)]
        public readonly float G;

        [FieldOffset(8)]
        public readonly float B;

        [FieldOffset(12)]
        public readonly float A;

        #endregion

        #region Internal Fields

#if NETCOREAPP || NET46_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        [FieldOffset(0)]
        internal readonly Vector4 Rgba;

        [FieldOffset(0)]
        internal readonly Vector3 Rgb;
#endif

        #endregion

        #endregion

        #region Properties

        //public bool IsValid => Clip().Rgba == Rgba; // Clip() == this;
#if NETCOREAPP || NET46_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        internal RgbF RgbF => new RgbF(Rgb);
#else
        internal RgbF RgbF => new RgbF(R, G, B);
#endif

        #endregion

        #region Operators

        public static bool operator ==(ColorF left, ColorF right) => Equals(left, right);
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

        #endregion

        #region Constructors

        #region Public Constructors

        // does not validate values for performance reasons but you can call IsValid or Clip
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

        #endregion

        #region Internal Constructors

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

#if NETCOREAPP || NET46_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        public ColorF Clip() => new ColorF(Vector4.Clamp(Rgba, Vector4.Zero, Vector4.One));
#else
        public ColorF Clip() => new ColorF(A.Clip(0f, 1f), R.Clip(0f, 1f), G.Clip(0f, 1f), B.Clip(0f, 1f));
#endif

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public Color32 ToColor32() => new Color32(ColorSpaceHelper.ToByte(A),
            ColorSpaceHelper.LinearToSrgb8Bit(R),
            ColorSpaceHelper.LinearToSrgb8Bit(G),
            ColorSpaceHelper.LinearToSrgb8Bit(B));

#if NETCOREAPP || NET46_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public PColorF ToPremultiplied() => new PColorF(new Vector4(Rgb * A, A));
#else
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public PColorF ToPremultiplied() => new PColorF(A, R * A, G * A, B * A);
#endif

        /// <summary>
        /// Gets the string representation of this <see cref="ColorF"/> instance.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents this <see cref="ColorF"/> instance.</returns>
        public override string ToString() => $"[A={A:N4}; R={R:N4}; G={G:N4}; B={B:N4}]";

        public bool Equals(ColorF other) => other.Rgba == Rgba;

        public override bool Equals(object? obj) => obj is ColorF other && Equals(other);

        public override int GetHashCode() => Rgba.GetHashCode();

        #endregion
    }
}
