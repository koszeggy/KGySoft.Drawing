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

#if NETCOREAPP || NET46_OR_GREATER || NETSTANDARD2_1_OR_GREATER
using System.Numerics;
#endif
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#endregion

namespace KGySoft.Drawing.Imaging
{
    [StructLayout(LayoutKind.Explicit)]
    internal struct RgbF
    {
        #region Fields

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

        #region Operators

        public static ColorF operator +(ColorF left, RgbF right)
        {
#if NETCOREAPP || NET46_OR_GREATER || NETSTANDARD2_1_OR_GREATER
            return new ColorF(new Vector4(left.Rgb + right.Rgb, left.A));
#else
            return new ColorF(left.A, left.R + right.R, left.G + right.G, left.B + right.B);
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
    }
}
