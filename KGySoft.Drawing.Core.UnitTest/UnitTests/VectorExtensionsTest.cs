#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: VectorExtensionsTest.cs
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
using System.Runtime.Intrinsics;

using KGySoft.CoreLibraries;

using NUnit.Framework;

#endregion

namespace KGySoft.Drawing.UnitTests
{
    [TestFixture]
    public class VectorExtensionsTest
    {
        #region Methods

#if NETCOREAPP3_0_OR_GREATER

        // The commented out cases would work with a more complete implementation (see PowTest in the PerformanceTest project), but they handle cases that are not needed in this project.
        [TestCase(0.1f, 1f / 2.4f)]
        [TestCase(0.5f, 1f / 2.4f)]
        [TestCase(0.9f, 1f / 2.4f)]
        [TestCase(0.1f, 2.4f)]
        [TestCase(0.5f, 2.4f)]
        [TestCase(0.9f, 2.4f)]
        [TestCase(2f, 2f)]
        [TestCase(2f, -2f)]
        //[TestCase(-2f, 2f)]
        //[TestCase(-2f, -2f)]
        [TestCase(2f, 35f)]
        [TestCase(2f, -35f)]
        //[TestCase(-2f, 35f)]
        //[TestCase(-2f, -35f)]
        [TestCase(2f, 1.5f)]
        [TestCase(2f, -1.5f)]
        [TestCase(-2f, 1.5f)]
        [TestCase(-2f, -1.5f)]
        [TestCase(1.5f, 2f)]
        [TestCase(1.5f, -2f)]
        //[TestCase(-1.5f, 2f)]
        //[TestCase(-1.5f, -2f)]
        [TestCase(1.5f, 1.5f)]
        [TestCase(1.5f, -1.5f)]
        //[TestCase(-1.5f, 1.5f)]
        //[TestCase(-1.5f, -1.5f)]
        //[TestCase(0f, 0f)] // 1 <-> NaN
        [TestCase(0f, 2f)]
        [TestCase(0f, -2f)]
        [TestCase(0f, 1.5f)]
        [TestCase(0f, -1.5f)]
        [TestCase(0f, Single.NegativeInfinity)]
        [TestCase(0f, Single.PositiveInfinity)]
        [TestCase(0f, Single.NaN)]
        [TestCase(1f, 0f)]
        [TestCase(1f, 2f)]
        [TestCase(1f, -2f)]
        [TestCase(1f, 1.5f)]
        [TestCase(1f, -1.5f)]
        [TestCase(1f, Single.NegativeInfinity)]
        [TestCase(1f, Single.PositiveInfinity)]
        [TestCase(1f, Single.NaN)]
        [TestCase(2f, 0f)]
        //[TestCase(-2f, 0f)]
        [TestCase(1.5f, 0f)]
        //[TestCase(-1.5f, 0f)]
        [TestCase(Single.PositiveInfinity, 0f)]
        [TestCase(Single.NegativeInfinity, 0f)]
        //[TestCase(Single.NaN, 0f)] // 1 <-> NaN
        //[TestCase(Single.NaN, -0f)] // 1 <-> NaN
        [TestCase(2.148e+9f, 2f)]
        [TestCase(2.148e+9f, -2f)]
        [TestCase(2.148e+9f, 1.5f)]
        [TestCase(2.148e+9f, -1.5f)]
        //[TestCase(-2.148e+9f, 2f)]
        //[TestCase(-2.148e+9f, -2f)]
        //[TestCase(-2.148e+9f, 1.5f)]
        //[TestCase(-2.148e+9f, -1.5f)]
        [TestCase(0.9999999f, 2.148e+9f)]
        [TestCase(0.9999999f, -2.148e+9f)]
        [TestCase(1.0000001f, 2.148e+9f)]
        [TestCase(1.0000001f, -2.148e+9f)]
        //[TestCase(-0.9999999f, 2.148e+9f)]
        //[TestCase(-0.9999999f, -2.148e+9f)]
        //[TestCase(-1.0000001f, 2.148e+9f)]
        //[TestCase(-1.0000001f, -2.148e+9f)]
        [TestCase(Single.PositiveInfinity, 2f)]
        [TestCase(Single.PositiveInfinity, 1.5f)]
        [TestCase(Single.PositiveInfinity, -2f)]
        [TestCase(Single.PositiveInfinity, -1.5f)]
        [TestCase(Single.NegativeInfinity, 2f)]
        [TestCase(Single.NegativeInfinity, 1.5f)]
        [TestCase(Single.NegativeInfinity, -2f)]
        [TestCase(Single.NegativeInfinity, -1.5f)]
        [TestCase(0.9999999f, Single.PositiveInfinity)]
        //[TestCase(-0.9999999f, Single.PositiveInfinity)]
        [TestCase(0.9999999f, Single.NegativeInfinity)]
        //[TestCase(-0.9999999f, Single.NegativeInfinity)]
        [TestCase(1.0000001f, Single.PositiveInfinity)]
        //[TestCase(-1.0000001f, Single.PositiveInfinity)]
        [TestCase(1.0000001f, Single.NegativeInfinity)]
        //[TestCase(-1.0000001f, Single.NegativeInfinity)]
        [TestCase(Single.NaN, 2f)]
        [TestCase(Single.NaN, 1.5f)]
        [TestCase(Single.NaN, -2f)]
        [TestCase(Single.NaN, -1.5f)]
        [TestCase(0.9999999f, Single.NaN)]
        //[TestCase(-0.9999999f, Single.NaN)]
        [TestCase(1.0000001f, Single.NaN)]
        //[TestCase(-1.0000001f, Single.NaN)]
        public void PowTest(float x, float p)
        {
            // See also the performance tests in the PerformanceTest project.
            float expected = MathF.Pow(x, p);
            DoAssert(Vector128.Create(x).Pow(p).ToScalar());
            DoAssert(Vector256.Create(x).Pow(p).ToScalar());
#if NET8_0_OR_GREATER
            DoAssert(Vector512.Create(x).Pow(p).ToScalar());
#endif

            #region Local Methods

            void DoAssert(float actual)
            {
                Assert.IsTrue(Single.IsNaN(expected) && Single.IsNaN(actual) // both NaN
                    || Single.IsInfinity(expected) && Single.IsInfinity(actual) && MathF.Sign(expected) == MathF.Sign(actual) // both same infinity
                    || Single.IsNaN(p) && Single.IsNaN(actual) // when power is NaN, we return NaN, whereas MathF.Pow returns 1 for base 1
                    || Single.IsNaN(actual) && (Single.IsInfinity(x) || Single.IsInfinity(p)) // we may return NaN when base or power is infinite, e.g. 1^infinity or Infinity^0
                    || expected.TolerantEquals(actual, MathF.Max(1e-6f, Single.IsInfinity(expected) ? 0f : MathF.Pow(10, MathF.Log10(Math.Abs(expected)) - 6))),
                    $"{expected:R} <-> {actual:R}");
            }

            #endregion
        }

        // The commented out cases would work with a more complete implementation that handle negative values, infinities and NaNs. These were removed for performance reasons as they are not needed in this project.
        [TestCase(0.5f, 0.25f, 0.125f, 0.5f, 2.4f)]
        //[TestCase(2f, -2f, 1.5f, -1.5f, 2f)]
        //[TestCase(2f, -2f, 1.5f, -1.5f, -2f)]
        //[TestCase(2f, -2f, 1.5f, -1.5f, 1.5f)]
        //[TestCase(2f, -2f, 1.5f, -1.5f, -1.5f)]
        //[TestCase(2f, -2f, 1.5f, -1.5f, 0f)]
        //[TestCase(2f, -2f, 1.5f, -1.5f, Single.PositiveInfinity)]
        //[TestCase(2f, -2f, 1.5f, -1.5f, Single.NegativeInfinity)]
        //[TestCase(2f, -2f, 1.5f, -1.5f, 1f)]
        //[TestCase(2f, -2f, 1.5f, -1.5f, Single.NaN)]
        //[TestCase(0.9999999f, -0.9999999f, 1.0000001f, -1.0000001f, 2f)]
        //[TestCase(0.9999999f, -0.9999999f, 1.0000001f, -1.0000001f, -2f)]
        //[TestCase(0.9999999f, -0.9999999f, 1.0000001f, -1.0000001f, 1.5f)]
        //[TestCase(0.9999999f, -0.9999999f, 1.0000001f, -1.0000001f, -1.5f)]
        //[TestCase(0.9999999f, -0.9999999f, 1.0000001f, -1.0000001f, 0f)]
        //[TestCase(0.9999999f, -0.9999999f, 1.0000001f, -1.0000001f, Single.PositiveInfinity)]
        //[TestCase(0.9999999f, -0.9999999f, 1.0000001f, -1.0000001f, Single.NegativeInfinity)]
        //[TestCase(0.9999999f, -0.9999999f, 1.0000001f, -1.0000001f, 1f)]
        //[TestCase(0.9999999f, -0.9999999f, 1.0000001f, -1.0000001f, Single.NaN)]
        //[TestCase(0f, Single.PositiveInfinity, Single.NegativeInfinity, Single.NaN, 2f)]
        //[TestCase(0f, Single.PositiveInfinity, Single.NegativeInfinity, Single.NaN, -2f)]
        //[TestCase(0f, Single.PositiveInfinity, Single.NegativeInfinity, Single.NaN, 1.5f)]
        //[TestCase(0f, Single.PositiveInfinity, Single.NegativeInfinity, Single.NaN, -1.5f)]
        //[TestCase(0f, Single.PositiveInfinity, Single.NegativeInfinity, Single.NaN, 0f)]
        //[TestCase(0f, Single.PositiveInfinity, Single.NegativeInfinity, Single.NaN, Single.PositiveInfinity)]
        //[TestCase(0f, Single.PositiveInfinity, Single.NegativeInfinity, Single.NaN, Single.NegativeInfinity)]
        //[TestCase(0f, Single.PositiveInfinity, Single.NegativeInfinity, Single.NaN, 1f)]
        //[TestCase(0f, Single.PositiveInfinity, Single.NegativeInfinity, Single.NaN, Single.NaN)]
        public void PowVectorTestDifferentValues(float v1, float v2, float v3, float v4, float p)
        {
            #region Local Methods

            static void AssertEqual(Vector128<float> expected, Vector128<float> actual) => Assert.AreEqual(expected.Or(expected.IsNaN()).AsUInt32(), actual.Or(actual.IsNaN()).AsUInt32(), $"{expected} vs. {actual}");

            #endregion

            // Comparing to scalar paths, which is compared to MathF.Pow in PowTest using a much more sophisticated comparison. Therefore, it's enough to compare bitwise equality here.
            Vector128<float> expected = Vector128.Create(
                Vector128.Create(v1).Pow(p).ToScalar(),
                Vector128.Create(v2).Pow(p).ToScalar(),
                Vector128.Create(v3).Pow(p).ToScalar(),
                Vector128.Create(v4).Pow(p).ToScalar());


            Vector128<float> actual = Vector128.Create(v1, v2, v3, v4).Pow(p);
            AssertEqual(expected, actual);

            Vector256<float> actual256 = Vector256.Create(v1, v2, v3, v4, v1, v2, v3, v4).Pow(p);
            AssertEqual(expected, actual256.GetLower());
            AssertEqual(expected, actual256.GetUpper());

#if NET8_0_OR_GREATER
            Vector512<float> actual512 = Vector512.Create(v1, v2, v3, v4, v1, v2, v3, v4, v1, v2, v3, v4, v1, v2, v3, v4).Pow(p);
            AssertEqual(expected, actual512.GetLower().GetLower());
            AssertEqual(expected, actual512.GetLower().GetUpper());
            AssertEqual(expected, actual512.GetUpper().GetLower());
            AssertEqual(expected, actual512.GetUpper().GetUpper());
#endif
        }

#endif

        #endregion
    }
}
