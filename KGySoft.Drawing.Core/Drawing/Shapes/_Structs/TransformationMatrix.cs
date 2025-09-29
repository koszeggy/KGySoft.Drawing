#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: TransformationMatrix.cs
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
#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
using System.Numerics;
#endif
#if NET5_0_OR_GREATER
using System.Runtime.CompilerServices;
#endif
using System.Runtime.InteropServices;

#endregion

#region Suppressions

#if !(NETCOREAPP || NET45_OR_GREATER || NETSTANDARD)
#pragma warning disable CS1574 // XML comment has cref attribute that could not be resolved - The documentation uses types that are not available on all platforms
#endif

#endregion

namespace KGySoft.Drawing.Shapes
{
    /// <summary>
    /// Represents a 3x2 matrix for 2D transformations. It's similar to <see cref="Matrix3x2"/>;
    /// in fact, on platforms where it is available, it uses a <see cref="Matrix3x2"/> internally.
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Explicit)]
    public struct TransformationMatrix : IEquatable<TransformationMatrix>
    {
        #region Fields

        #region Static Fields

        /// <summary>
        /// Gets the identity matrix. This field is read-only.
        /// </summary>
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

        /// <summary>
        /// Gets whether this matrix is the identity matrix.
        /// </summary>
#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
        public readonly bool IsIdentity => Matrix.IsIdentity;
#else
        public readonly bool IsIdentity => this == Identity;
#endif

        #endregion

        #region Operators

        /// <summary>
        /// Returns a boolean indicating whether the given matrices are equal.
        /// </summary>
        /// <param name="a">The first source matrix.</param>
        /// <param name="b">The second source matrix.</param>
        /// <returns><see langword="true"/> if the matrices are equal; otherwise, <see langword="false"/>.</returns>
        public static bool operator ==(TransformationMatrix a, TransformationMatrix b) => a.Equals(b);

        /// <summary>
        /// Returns a boolean indicating whether the given matrices are not equal.
        /// </summary>
        /// <param name="a">The first source matrix.</param>
        /// <param name="b">The second source matrix.</param>
        /// <returns><see langword="true"/> if the matrices are not equal; otherwise, <see langword="false"/>.</returns>
        public static bool operator !=(TransformationMatrix a, TransformationMatrix b) => !(a == b);

        /// <summary>
        /// Negates the given matrix by multiplying all values by -1.
        /// </summary>
        /// <param name="value">The source matrix.</param>
        /// <returns>The negated matrix.</returns>
        public static TransformationMatrix operator -(TransformationMatrix value)
        {
#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
            return new TransformationMatrix(-value.Matrix);
#else
            return new TransformationMatrix(-value.M11, -value.M12, -value.M21, -value.M22, -value.M31, -value.M32);
#endif
        }

        /// <summary>
        /// Adds each matrix element in <paramref name="a"/> with its corresponding element in <paramref name="b"/>.
        /// </summary>
        /// <param name="a">The first source matrix.</param>
        /// <param name="b">The second source matrix.</param>
        /// <returns>The matrix containing the summed values.</returns>
        public static TransformationMatrix operator +(TransformationMatrix a, TransformationMatrix b)
        {
#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
            return new TransformationMatrix(a.Matrix + b.Matrix);
#else
            return new TransformationMatrix(
                a.M11 + b.M11,
                a.M12 + b.M12,
                a.M21 + b.M21,
                a.M22 + b.M22,
                a.M31 + b.M31,
                a.M32 + b.M32);
#endif
        }

        /// <summary>
        /// Subtracts each matrix element in <paramref name="b"/> from its corresponding element in <paramref name="a"/>.
        /// </summary>
        /// <param name="a">The first source matrix.</param>
        /// <param name="b">The second source matrix.</param>
        /// <returns>The matrix containing the resulting values.</returns>
        public static TransformationMatrix operator -(TransformationMatrix a, TransformationMatrix b)
        {
#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
            return new TransformationMatrix(a.Matrix - b.Matrix);
#else
            return new TransformationMatrix(
                a.M11 - b.M11,
                a.M12 - b.M12,
                a.M21 - b.M21,
                a.M22 - b.M22,
                a.M31 - b.M31,
                a.M32 - b.M32);
#endif
        }

        /// <summary>
        /// Multiplies two matrices together and returns the resulting matrix.
        /// </summary>
        /// <param name="a">The first source matrix.</param>
        /// <param name="b">The second source matrix.</param>
        /// <returns>The product matrix.</returns>
        public static TransformationMatrix operator *(TransformationMatrix a, TransformationMatrix b)
        {
#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
            return new TransformationMatrix(a.Matrix * b.Matrix);
#else
            return new TransformationMatrix(
                a.M11 * b.M11 + a.M12 * b.M21,
                a.M11 * b.M12 + a.M12 * b.M22,
                a.M21 * b.M11 + a.M22 * b.M21,
                a.M21 * b.M12 + a.M22 * b.M22,
                a.M31 * b.M11 + a.M32 * b.M21 + b.M31,
                a.M31 * b.M12 + a.M32 * b.M22 + b.M32);
#endif
        }

        /// <summary>
        /// Scales all elements in a matrix by the given scalar factor.
        /// </summary>
        /// <param name="matrix">The source matrix.</param>
        /// <param name="scalar">The scaling value to use.</param>
        /// <returns>The resulting matrix.</returns>
        public static TransformationMatrix operator *(TransformationMatrix matrix, float scalar)
        {
#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
            return new TransformationMatrix(matrix.Matrix * scalar);
#else
            return new TransformationMatrix(
                matrix.M11 * scalar,
                matrix.M12 * scalar,
                matrix.M21 * scalar,
                matrix.M22 * scalar,
                matrix.M31 * scalar,
                matrix.M32 * scalar);
#endif
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
        /// <summary>
        /// Creates a <see cref="TransformationMatrix"/> instance from the specified <see cref="Matrix3x2"/> instance.
        /// </summary>
        /// <param name="matrix">The <see cref="Matrix3x2"/> instance to create the <see cref="TransformationMatrix"/> from.</param>
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

        /// <summary>
        /// Creates a translation matrix from the specified offsets.
        /// </summary>
        /// <param name="x">The distance to translate along the X axis.</param>
        /// <param name="y">The distance to translate along the Y axis.</param>
        /// <returns>The translation matrix.</returns>
        public static TransformationMatrix CreateTranslation(float x, float y)
        {
#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
            return new TransformationMatrix(Matrix3x2.CreateTranslation(x, y));
#else
            return new TransformationMatrix(1f, 0f, 0f, 1f, x, y);
#endif
        }

#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
        /// <summary>
        /// Creates a translation matrix from the specified offset.
        /// </summary>
        /// <param name="offset">The distance to translate.</param>
        /// <returns>The translation matrix.</returns>
        public static TransformationMatrix CreateTranslation(Vector2 offset) => new(Matrix3x2.CreateTranslation(offset));
#endif

        /// <summary>
        /// Creates a rotation matrix using the specified angle in radians.
        /// </summary>
        /// <param name="radians">The angle, in radians, by which to rotate the matrix.</param>
        /// <returns>The rotation matrix.</returns>
        public static TransformationMatrix CreateRotation(float radians)
        {
#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
            return new TransformationMatrix(Matrix3x2.CreateRotation(radians));
#else
            radians = MathF.IEEERemainder(radians, MathF.PI * 2f);

            // It would be much better to use degrees only but the original Matrix3x2 implementation also uses radians, so we have this overload, too.
            const float rotationEpsilon = 0.001f * MathF.PI / 180f;
            float cos, sin;
            switch (radians)
            {
                // 0 degrees
                case > -rotationEpsilon and < rotationEpsilon:
                    cos = 1;
                    sin = 0;
                    break;

                // 90 degrees
                case > MathF.PI / 2 - rotationEpsilon and < MathF.PI / 2 + rotationEpsilon:
                    cos = 0;
                    sin = 1;
                    break;

                // 180 degrees
                case > MathF.PI - rotationEpsilon and < MathF.PI + rotationEpsilon:
                    cos = -1;
                    sin = 0;
                    break;
                
                // 270 degrees
                case > -MathF.PI / 2 - rotationEpsilon and < -MathF.PI / 2 + rotationEpsilon:
                    cos = 0;
                    sin = -1;
                    break;
                
                // Arbitrary rotation
                default:
                    cos = MathF.Cos(radians);
                    sin = MathF.Sin(radians);
                    break;
            }

            return new TransformationMatrix(cos, sin, -sin, cos, 0f, 0f);
#endif
        }

        /// <summary>
        /// Creates a rotation matrix using the specified angle and center point.
        /// </summary>
        /// <param name="radians">The angle, in radians, by which to rotate the matrix.</param>
        /// <param name="centerPoint">The center point of the rotation.</param>
        /// <returns>The rotation matrix.</returns>
        public static TransformationMatrix CreateRotation(float radians, PointF centerPoint)
        {
#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
            return new TransformationMatrix(Matrix3x2.CreateRotation(radians, centerPoint.AsVector2()));
#else
            radians = MathF.IEEERemainder(radians, MathF.PI * 2f);

            // It would be much better to use degrees only but the original Matrix3x2 implementation also uses radians, so we have this overload, too.
            const float rotationEpsilon = 0.001f * MathF.PI / 180f;
            float cos, sin;
            switch (radians)
            {
                // 0 degrees
                case > -rotationEpsilon and < rotationEpsilon:
                    cos = 1;
                    sin = 0;
                    break;

                // 90 degrees
                case > MathF.PI / 2 - rotationEpsilon and < MathF.PI / 2 + rotationEpsilon:
                    cos = 0;
                    sin = 1;
                    break;

                // 180 degrees
                case > MathF.PI - rotationEpsilon and < MathF.PI + rotationEpsilon:
                    cos = -1;
                    sin = 0;
                    break;

                // 270 degrees
                case > -MathF.PI / 2 - rotationEpsilon and < -MathF.PI / 2 + rotationEpsilon:
                    cos = 0;
                    sin = -1;
                    break;

                // Arbitrary rotation
                default:
                    cos = MathF.Cos(radians);
                    sin = MathF.Sin(radians);
                    break;
            }

            return new TransformationMatrix(cos, sin, -sin, cos,
                centerPoint.X * (1 - cos) + centerPoint.Y * sin,
                centerPoint.Y * (1 - cos) - centerPoint.X * sin);
#endif
        }

        /// <summary>
        /// Creates a rotation matrix using the specified angle in degrees.
        /// </summary>
        /// <param name="angle">The angle, in degrees, by which to rotate the matrix.</param>
        /// <returns>The rotation matrix.</returns>
        public static TransformationMatrix CreateRotationDegrees(float angle)
        {
            float cos, sin;
            angle = angle is >= 0f and <= 360f ? angle : angle % 360f;
            if (angle < 0)
                angle += 360f;

            switch (angle)
            {
                case 0f:
                    cos = 1;
                    sin = 0;
                    break;

                case 90f:
                    cos = 0;
                    sin = 1;
                    break;

                case 180f:
                    cos = -1;
                    sin = 0;
                    break;

                case 270f:
                    cos = 0;
                    sin = -1;
                    break;

                // Arbitrary rotation
                default:
                    float radians = angle.ToRadian();
                    cos = MathF.Cos(radians);
                    sin = MathF.Sin(radians);
                    break;
            }

            return new TransformationMatrix(cos, sin, -sin, cos, 0f, 0f);
        }

        /// <summary>
        /// Creates a rotation matrix using the specified angle and center point.
        /// </summary>
        /// <param name="angle">The angle, in degrees, by which to rotate the matrix.</param>
        /// <param name="centerPoint">The center point of the rotation.</param>
        /// <returns>The rotation matrix.</returns>
        public static TransformationMatrix CreateRotationDegrees(float angle, PointF centerPoint)
        {
            float cos, sin;
            angle = angle is >= 0f and <= 360f ? angle : angle % 360f;
            if (angle < 0)
                angle += 360f;

            switch (angle)
            {
                case 0f:
                    cos = 1;
                    sin = 0;
                    break;

                case 90f:
                    cos = 0;
                    sin = 1;
                    break;

                case 180f:
                    cos = -1;
                    sin = 0;
                    break;

                case 270f:
                    cos = 0;
                    sin = -1;
                    break;

                // Arbitrary rotation
                default:
                    float radians = angle.ToRadian();
                    cos = MathF.Cos(radians);
                    sin = MathF.Sin(radians);
                    break;
            }

            return new TransformationMatrix(cos, sin, -sin, cos,
                centerPoint.X * (1 - cos) + centerPoint.Y * sin,
                centerPoint.Y * (1 - cos) - centerPoint.X * sin);
        }

        /// <summary>
        /// Creates a scale matrix from the specified <paramref name="x"/> and <paramref name="y"/> components.
        /// </summary>
        /// <param name="x">The value to scale by on the X axis.</param>
        /// <param name="y">The value to scale by on the Y axis.</param>
        /// <returns>The scaling matrix.</returns>
        public static TransformationMatrix CreateScale(float x, float y)
        {
#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
            return new TransformationMatrix(Matrix3x2.CreateScale(x, y));
#else
            return new TransformationMatrix(x, 0f, 0f, y, 0f, 0f);
#endif
        }

#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
        /// <summary>
        /// Creates a scale matrix from the specified <paramref name="scale"/>.
        /// </summary>
        /// <param name="scale">The scale to use.</param>
        /// <returns>The scaling matrix.</returns>
        public static TransformationMatrix CreateScale(Vector2 scale) => new(Matrix3x2.CreateScale(scale));
#endif

        #endregion

        #region Instance Methods

        /// <summary>
        /// Transforms the specified point using the current transformation matrix.
        /// </summary>
        /// <param name="point">The point to transform.</param>
        /// <returns>The transformed point.</returns>
        public PointF Transform(PointF point)
        {
#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
            Vector2 result = Vector2.Transform(point.AsVector2(), Matrix);
            return result.AsPointF();
#else
            return new PointF(
                M11 * point.X + M21 * point.Y + M31,
                M12 * point.X + M22 * point.Y + M32);
#endif
        }

#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
        /// <summary>
        /// Transforms the specified point using the current transformation matrix.
        /// </summary>
        /// <param name="point">The point to transform, represented as a <see cref="Vector2"/> instance.</param>
        /// <returns>A <see cref="Vector2"/> instance representing the transformed point.</returns>
        public Vector2 Transform(Vector2 point) => Vector2.Transform(point, Matrix);
#endif

        /// <summary>
        /// Returns a boolean indicating whether the matrix is equal to the other given matrix.
        /// </summary>
        /// <param name="other">The other matrix to test equality against.</param>
        /// <returns><see langword="true"/> if this matrix is equal to <paramref name="other"/>; otherwise, <see langword="false"/>.</returns>
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

        /// <summary>
        /// Returns a boolean indicating whether the given <see cref="object"/> is equal to this matrix instance.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to compare against.</param>
        /// <returns><see langword="true"/> if the object is equal to this matrix; otherwise, <see langword="false"/>.</returns>
        public override bool Equals(object? obj)
        {
            return obj is TransformationMatrix other && Equals(other);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
            return Matrix.GetHashCode();
#else
            return ((M11, M12), (M21, M22), (M31, M32)).GetHashCode();
#endif
        }

        /// <summary>
        /// Returns a string that represents this matrix.
        /// </summary>
        /// <returns>The string representation of this matrix.</returns>
        /// <remarks>The numeric values in the returned string are formatted by using the conventions of the current culture.</remarks>
        public override readonly string ToString() => IsIdentity
            ? "Identity"
            : $"{{ {{M11:{M11} M12:{M12}}} {{M21:{M21} M22:{M22}}} {{M31:{M31} M32:{M32}}} }}";

        #endregion

        #endregion
    }
}