#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ColorF.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2021 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;
#if !(NET35 || NET40 || NET45 || NETSTANDARD2_0)
using System.Numerics;
#endif
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#endregion

namespace KGySoft.Drawing.Imaging
{
    [StructLayout(LayoutKind.Explicit)]
    internal readonly struct ColorF
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

        [FieldOffset(0)]
        public readonly float R;

        [FieldOffset(4)]
        public readonly float G;

        [FieldOffset(8)]
        public readonly float B;

        [FieldOffset(12)]
        public readonly float A;

        #endregion

        #region Private Fields

#if !(NET35 || NET40 || NET45 || NETSTANDARD2_0)
        [FieldOffset(0)]
        private readonly Vector4 value;
#endif

        #endregion

        #endregion

        #endregion

        #region Operators

        /// <summary>
        /// Multiplies a <see cref="ColorF"/> by the given scalar.
        /// </summary>
        /// <param name="left">The source color.</param>
        /// <param name="right">The scalar value.</param>
        /// <returns>The scaled color.</returns>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static ColorF operator *(ColorF left, float right)
        {
#if NET35 || NET40 || NET45 || NETSTANDARD2_0
            return new ColorF(left.A * right, left.R * right, left.G * right, left.B * right);
#else
            return new ColorF(left.value * new Vector4(right));
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
#if NET35 || NET40 || NET45 || NETSTANDARD2_0
            return new ColorF(left.A + right, left.R + right, left.G + right, left.B + right);
#else
            return new ColorF(left.value * new Vector4(right));
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
#if NET35 || NET40 || NET45 || NETSTANDARD2_0
            return new ColorF(left.A + right.A, left.R + right.R, left.G + right.G, left.B + right.B);
#else
            return new ColorF(left.value + right.value);
#endif
        }

        #endregion

        #region Constructors

        #region Public Constructors

        public ColorF(float a, float r, float g, float b)
#if !(NET35 || NET40 || NET45 || NETSTANDARD2_0)
            : this() // so the compiler does not complain about not initializing the value field
#endif
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

#if NET35 || NET40 || NET45 || NETSTANDARD2_0
        public ColorF(Color32 c)
        {
            R = (float)c.R / Byte.MaxValue;
            G = (float)c.G / Byte.MaxValue;
            B = (float)c.B / Byte.MaxValue;
            A = (float)c.A / Byte.MaxValue;
        }
#else
        public ColorF(Color32 c)
            : this() // so the compiler does not complain about not initializing ARGB fields
        {
            value = new Vector4(c.R, c.G, c.B, c.A) / max8Bpp;
        }
#endif

        #endregion

        #region Private Constructors

#if !(NET35 || NET40 || NET45 || NETSTANDARD2_0)
        private ColorF(Vector4 vector)
            : this() // so the compiler does not complain about not initializing ARGB fields
        {
            value = vector;
        }
#endif

        #endregion

        #endregion

        #region Methods

        #region Static Methods

#if !(NET35 || NET40 || NET45 || NETSTANDARD2_0)
        [MethodImpl(MethodImpl.AggressiveInlining)]
        private static Vector4 Clip(Vector4 x, Vector4 min, Vector4 max)
            => Vector4.Min(Vector4.Max(x, min), max);
#endif

        #endregion

        #region Instance Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public Color32 ToColor32()
        {
#if NET35 || NET40 || NET45 || NETSTANDARD2_0
            ColorF result = this * Byte.MaxValue + 0.5f;
            return new Color32(result.A.ClipToByte(),
                result.R.ClipToByte(),
                result.G.ClipToByte(),
                result.B.ClipToByte());
#else
            Vector4 result = value * max8Bpp;
            result += half;
            result = Clip(result, Vector4.Zero, max8Bpp);
            return new Color32((byte)result.W, (byte)result.X, (byte)result.Y, (byte)result.Z);
#endif
        }

        /// <summary>
        /// Gets the string representation of this <see cref="ColorF"/> instance.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents this <see cref="ColorF"/> instance.</returns>
        public override string ToString() => $"[A={A:N4}; R={R:N4}; G={G:N4}; B={B:N4}]";

        #endregion

        #endregion
    }
}
