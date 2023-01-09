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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#endregion

namespace KGySoft.Drawing.Imaging
{
    [StructLayout(LayoutKind.Explicit)]
    internal readonly struct ColorF
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

        #endregion
    }
}
