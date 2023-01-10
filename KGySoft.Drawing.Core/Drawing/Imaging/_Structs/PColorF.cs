#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: PColorF.cs
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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#endregion

namespace KGySoft.Drawing.Imaging
{
    [StructLayout(LayoutKind.Explicit)]
    internal readonly struct PColorF
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

        //#region Properties

        //public bool IsValid => Clip().Rgba == Rgba; // Clip() == this;

        //#endregion

        #region Operators

        /// <summary>
        /// Multiplies a <see cref="PColorF"/> by the given scalar.
        /// </summary>
        /// <param name="left">The source color.</param>
        /// <param name="right">The scalar value.</param>
        /// <returns>The scaled color.</returns>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static PColorF operator *(PColorF left, float right)
        {
#if NETCOREAPP || NET46_OR_GREATER || NETSTANDARD2_1_OR_GREATER
            return new PColorF(left.Rgba * new Vector4(right));
#else
            return new PColorF(left.A * right, left.R * right, left.G * right, left.B * right);
#endif
        }

        /// <summary>
        /// Adds two colors together.
        /// </summary>
        /// <param name="left">The first source color.</param>
        /// <param name="right">The second source color.</param>
        /// <returns>The summed color.</returns>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static PColorF operator +(PColorF left, PColorF right)
        {
#if NETCOREAPP || NET46_OR_GREATER || NETSTANDARD2_1_OR_GREATER
            return new PColorF(left.Rgba + right.Rgba);
#else
            return new PColorF(left.A + right.A, left.R + right.R, left.G + right.G, left.B + right.B);
#endif
        }

        #endregion

        #region Constructors

        #region Public Constructors

        // does not validate values for performance reasons but you can call IsValid or Clip
        public PColorF(float a, float r, float g, float b)
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

        public PColorF(Color32 c) => this = c.ToColorF().ToPremultiplied();

        #endregion

        #region Internal Constructors

#if NETCOREAPP || NET46_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        internal PColorF(Vector4 vector)
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
        public PColorF Clip() => new PColorF(Vector4.Clamp(Rgba, Vector4.Zero, new Vector4(A.Clip(0f, 1f))));
#else
        public PColorF Clip()
        {
            float a = A.Clip(0f, 1f);
            return new PColorF(a, R.Clip(0f, a), G.Clip(0f, a), B.Clip(0f, a));
        }
#endif

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public Color32 ToColor32() => ToStraight().ToColor32();

#if NETCOREAPP || NET46_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public ColorF ToStraight() => new ColorF(new Vector4(Rgb / A, A));
#else
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public ColorF ToStraight() => new ColorF(A, R / A, G / A, B / A);
#endif

        /// <summary>
        /// Gets the string representation of this <see cref="PColorF"/> instance.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents this <see cref="PColorF"/> instance.</returns>
        public override string ToString() => $"[A={A:N4}; R={R:N4}; G={G:N4}; B={B:N4}]";

        #endregion
    }
}
