#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: TransformationMatrix.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2024 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;
#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
using System.Numerics;
#endif
#if NET5_0_OR_GREATER
using System.Runtime.CompilerServices;
#endif
using System.Runtime.InteropServices;

#endregion

namespace KGySoft.Drawing.Shapes
{
    [Serializable]
    [StructLayout(LayoutKind.Explicit)]
    public struct TransformationMatrix : IEquatable<TransformationMatrix>
    {
        #region Fields

        #region Static Fields

        public static readonly TransformationMatrix Identity =
#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
            new TransformationMatrix(Matrix3x2.Identity);
#else
            new TransformationMatrix(1f, 0f, 0f, 1f, 0f, 0f);
#endif

        #endregion

        #region Instance Fields

        #region Public Fields

        /// <summary>The first element of the first row.</summary>
        [FieldOffset(0)]
        public float M11;

        /// <summary>The second element of the first row.</summary>
        [FieldOffset(4)]
        public float M12;

        /// <summary>The first element of the second row.</summary>
        [FieldOffset(8)]
        public float M21;

        /// <summary>The second element of the second row.</summary>
        [FieldOffset(12)]
        public float M22;

        /// <summary>The first element of the third row.</summary>
        [FieldOffset(16)]
        public float M31;

        /// <summary>The second element of the third row.</summary>
        [FieldOffset(20)]
        public float M32;

        #endregion

        #region Private Fields

#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
        [FieldOffset(0)]
        [NonSerialized]
        internal Matrix3x2 Matrix;
#endif

        #endregion

        #endregion

        #endregion

        #region Properties

#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
        public readonly bool IsIdentity => Matrix.IsIdentity;
#else
        public readonly bool IsIdentity => this == Identity;
#endif

        #endregion

        #region Operators

        public static bool operator ==(TransformationMatrix a, TransformationMatrix b) => a.Equals(b);
        
        public static bool operator !=(TransformationMatrix a, TransformationMatrix b) => !(a == b);
        
        public static TransformationMatrix operator *(TransformationMatrix a, TransformationMatrix b)
        {
            return new TransformationMatrix(a.Matrix * b.Matrix);
        }

        #endregion

        #region Constructors

        /// <summary>Creates a 3x2 matrix from the specified components.</summary>
        /// <param name="m11">The value to assign to the first element in the first row.</param>
        /// <param name="m12">The value to assign to the second element in the first row.</param>
        /// <param name="m21">The value to assign to the first element in the second row.</param>
        /// <param name="m22">The value to assign to the second element in the second row.</param>
        /// <param name="m31">The value to assign to the first element in the third row.</param>
        /// <param name="m32">The value to assign to the second element in the third row.</param>
        public TransformationMatrix(float m11, float m12, float m21, float m22, float m31, float m32)
#if (NETCOREAPP || NET45_OR_GREATER || NETSTANDARD) && !NET5_0_OR_GREATER
            : this() // so the compiler does not complain about not initializing the other fields
#endif
        {
#if NET5_0_OR_GREATER
            Unsafe.SkipInit(out this);
#endif
#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
            Matrix = new Matrix3x2(m11, m12, m21, m22, m31, m32);
#else
            M11 = m11;
            M12 = m12;
            M21 = m21;
            M22 = m22;
            M31 = m31;
            M32 = m32;
#endif
        }

#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
        public TransformationMatrix(Matrix3x2 matrix)
#if !NET5_0_OR_GREATER
            : this() // so the compiler does not complain about not initializing the other fields
#endif
        {
#if NET5_0_OR_GREATER
            Unsafe.SkipInit(out this);
#endif
            Matrix = matrix;
        }
#endif

        #endregion

        #region Methods

        #region Static Methods

        public static TransformationMatrix CreateTranslation(float x, float y)
        {
            return new TransformationMatrix(Matrix3x2.CreateTranslation(x, y));
        }

        public static TransformationMatrix CreateRotation(float radians)
        {
            return new TransformationMatrix(Matrix3x2.CreateRotation(radians));
        }

        public static TransformationMatrix CreateScale(float x, float y)
        {
            return new TransformationMatrix(Matrix3x2.CreateScale(x, y));
        }

        #endregion

        #region Instance Methods

        public bool Equals(TransformationMatrix other)
        {
#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
            return Matrix.Equals(other.Matrix);
#else
            // ReSharper disable CompareOfFloatsByEqualityOperator
            return M11 == other.M11 && M12 == other.M12 && M21 == other.M21 && M22 == other.M22 && M31 == other.M31 && M32 == other.M32;
            // ReSharper restore CompareOfFloatsByEqualityOperator
#endif
        }

        public override bool Equals(object? obj)
        {
            return obj is TransformationMatrix other && Equals(other);
        }

        public override int GetHashCode()
        {
#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
            return Matrix.GetHashCode();
#else
            return ((M11, M12), (M21, M22), (M31, M32)).GetHashCode();
#endif
        }

        #endregion

        #endregion
    }
}