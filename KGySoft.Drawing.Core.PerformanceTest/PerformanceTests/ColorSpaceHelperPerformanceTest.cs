#if NETCOREAPP3_0_OR_GREATER
#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ColorSpaceHelperPerformanceTest.cs
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
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

using KGySoft.CoreLibraries;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

using KGySoft.Drawing.Imaging;

using NUnit.Framework;

#endregion

namespace KGySoft.Drawing.PerformanceTests
{
    [TestFixture]
    public class ColorSpaceHelperPerformanceTest
    {
        #region Methods

        [TestCase(0.5f, 0.5f, 0.25f, 0.125f)] // Pow x3
        [TestCase(0.5f, 0.001f, 0.002f, 0.003f)] // Linear conversion, 0 pow
        [TestCase(0.5f, 1f, 0.5f, 0.25f)] // Different channels, Pow x2
        [TestCase(0.5f, 1f, 1f, 0f)] // Different channels, 0 pow
        public void LinearToSrgbTest(float a, float r, float g, float b)
        {
            var linear = Vector128.Create(r, g, b, a);
            var srgb = ColorSpaceHelper.LinearToSrgbVectorRgba(Vector128.Create(r, g, b, a));

            Console.WriteLine($"{"Expected color:",-40} {srgb}");
            void DoAssert(Expression<Func<Vector128<float>, Vector128<float>>> e)
            {
                var m = (MethodCallExpression)e.Body;
                Vector128<float> actual = e.Compile().Invoke(linear);
                Console.WriteLine($"{$"{m.Method.Name}:",-40} {actual}");
                Assert.IsTrue(new ColorF(srgb).TolerantEquals(new ColorF(actual)), $"{m.Method.Name}: {srgb} vs. {actual}");
            }

            DoAssert(_ => linear.ToSrgb_0_Vanilla());
            DoAssert(_ => linear.ToSrgb_1_Vector_a_VanillaCompare());
#if NET7_0_OR_GREATER
            DoAssert(_ => linear.ToSrgb_1_Vector_b_IntrinsicCompare());
#endif
            DoAssert(_ => linear.ToSrgb_2_Intrinsics());

            new PerformanceTest { TestName = "Linear to sRGB", TestTime = 2000, Iterations = 10_000_000, Repeat = 3 }
                .AddCase(() => linear.ToSrgb_0_Vanilla(), nameof(Extensions.ToSrgb_0_Vanilla))
                .AddCase(() => linear.ToSrgb_1_Vector_a_VanillaCompare(), nameof(Extensions.ToSrgb_1_Vector_a_VanillaCompare))
#if NET7_0_OR_GREATER
                .AddCase(() => linear.ToSrgb_1_Vector_b_IntrinsicCompare(), nameof(Extensions.ToSrgb_1_Vector_b_IntrinsicCompare))
#endif
                .AddCase(() => linear.ToSrgb_2_Intrinsics(), nameof(Extensions.ToSrgb_2_Intrinsics))
                .DoTest()
                .DumpResults(Console.Out);

        }

        [TestCase(0.1f, 1f / 2.4f)]
        [TestCase(0.5f, 1f / 2.4f)]
        [TestCase(0.9f, 1f / 2.4f)]
        [TestCase(0.1f, 2.4f)]
        [TestCase(0.5f, 2.4f)]
        [TestCase(0.9f, 2.4f)]
        public void PowTest(float x, float p)
        {
            // Verdict: My Pow is about 1.5x-2x slower, it MAY worth to vectorize it if at least 2 color components are in the Pow range.
            Console.WriteLine($"{x} ^ {p}: {MathF.Pow(x, p)} vs. {x.Pow(p)}");
            Assert.IsTrue(MathF.Pow(x, p).TolerantEquals(x.Pow(p)));
            new PerformanceTest<float> { TestName = "Pow", Iterations = 10_000_000, Repeat = 3 }
                .AddCase(() => MathF.Pow(x, p), "MathF.Pow")
                .AddCase(() => x.Pow(p), "Pow")
                .DoTest()
                .DumpResults(Console.Out);
        }

        #endregion
    }

    internal static partial class Extensions
    {
        #region Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Vector128<float> ToSrgb_0_Vanilla(this Vector128<float> c)
        {
            return Vector128.Create(
                ColorSpaceHelper.LinearToSrgb(c.GetElement(0)),
                ColorSpaceHelper.LinearToSrgb(c.GetElement(1)),
                ColorSpaceHelper.LinearToSrgb(c.GetElement(2)),
                c.GetElement(3).ClipF());
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Vector128<float> ToSrgb_1_Vector_a_VanillaCompare(this Vector128<float> c)
        {
            var rgb = new Vector3(c.GetElement(0), c.GetElement(1), c.GetElement(2));
            if (rgb.GreaterThanAll(0.0031308f))
            {
                if (rgb.LessThanAll(1f))
                {
                    var result = new Vector3(
                        MathF.Pow(rgb.X, 1f / 2.4f),
                        MathF.Pow(rgb.Y, 1f / 2.4f),
                        MathF.Pow(rgb.Z, 1f / 2.4f));
                    result = result * 1.055f - new Vector3(0.055f);
                    return result.AsVector128().WithElement(3, c.GetElement(3));
                }
            }
            else if (rgb.GreaterThanAll(0f))
            {
                rgb *= 12.92f;
                return rgb.AsVector128().WithElement(3, c.GetElement(3));
            }

            return c.ToSrgb_0_Vanilla();
        }

#if NET7_0_OR_GREATER
        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Vector128<float> ToSrgb_1_Vector_b_IntrinsicCompare(this Vector128<float> c)
        {
            Vector128<float> rgbx = c.WithElement(3, 1f);
            //if (c.Rgb.GreaterThanAll(0.0031308f))
            if (Vector128.GreaterThanAll(rgbx, Vector128.Create(0.0031308f)))
            {
                //if (c.Rgb.LessThanAll(1f))
                if (Vector128.LessThanAll(rgbx.WithElement(3, 0f), Vector4.One.AsVector128()))
                {
                    var result = new Vector3(
                        MathF.Pow(c.GetElement(0), 1f / 2.4f),
                        MathF.Pow(c.GetElement(1), 1f / 2.4f),
                        MathF.Pow(c.GetElement(2), 1f / 2.4f));
                    result = result * 1.055f - new Vector3(0.055f);
                    return result.AsVector128().WithElement(3, c.GetElement(3));
                }
            }
            //else if (c.Rgb.GreaterThanAll(0f))
            else if (Vector128.GreaterThanAll(rgbx, Vector128<float>.Zero))
            {
                var result = rgbx.AsVector4() * 12.92f;
                return result.AsVector128().WithElement(3, c.GetElement(3));
            }

            return c.ToSrgb_0_Vanilla();
        }
#endif

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Vector128<float> ToSrgb_2_Intrinsics(this Vector128<float> c)
        {
            // Applying the same formula as in LinearToSrgb(float) but with vectorization if possible
            if (Sse.IsSupported)
            {
                Vector128<float> rgbx = c.WithElement(3, 1f);
#if NET7_0_OR_GREATER
                if (Vector128.GreaterThanAll(rgbx, Vector128.Create(0.0031308f)))
#else
                if (Sse.CompareGreaterThan(rgbx, Vector128.Create(0.0031308f)).AsUInt64().Equals(AllBitsSet))
#endif
                {
#if NET7_0_OR_GREATER
                    if (Vector128.LessThanAll(rgbx.WithElement(3, 0f), Vector128.Create(1f)))
#else
                    if (Sse.CompareNotGreaterThanOrEqual(rgbx.WithElement(3, 0f), Vector128.Create(1f)).AsUInt64().Equals(Vector128.Create(UInt64.MaxValue)))
#endif
                    {
                        // If all of the RGB components are between 0.0031308 and 1 we need to the same operations on them.
                        // Unfortunately, Pow, which is the slowest operation here cannot be vectorized (good enough approximations are too slow).
                        Vector128<float> result = Vector128.Create(
                            MathF.Pow(c.GetElement(0), 1f / 2.4f),
                            MathF.Pow(c.GetElement(1), 1f / 2.4f),
                            MathF.Pow(c.GetElement(2), 1f / 2.4f),
                            0f);
                        
                        if (Fma.IsSupported)
                            result = Fma.MultiplyAdd(result, Vector128.Create(1.055f), Vector128.Create(-0.055f));
                        else
                        {
                            result = Sse.Multiply(result, Vector128.Create(1.055f));
                            result = Sse.Subtract(result, Vector128.Create(0.055f));
                        }

                        return result.WithElement(3, c.GetElement(3).ClipF());
                    }
                }
#if NET7_0_OR_GREATER
                else if (Vector128.GreaterThanAll(rgbx, Vector128<float>.Zero))
#else
                else if (Sse.CompareGreaterThan(rgbx, Vector128<float>.Zero).AsUInt64().Equals(AllBitsSet))
#endif
                {
                    // If all of the RGB components are between 0 and 0.0031308 we need to the same operations on them.
                    // This branch can be vectorized much better though a much lower range of the colors belongs here.
                    return Sse.Multiply(c, Vector128.Create(12.92f)).WithElement(3, c.GetElement(3).ClipF());
                }
            }

            // The non-accelerated version.
            return c.ToSrgb_0_Vanilla();
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        private static float PowI(float value, int power)
        {
            float current = value;
            if (power < 0)
            {
                power = power > Int32.MinValue ? -power : Int32.MaxValue;
                current = 1f / current;
            }

            float result = 1f;
            while (power > 0)
            {
                if ((power & 1) == 1)
                {
                    result = current * result;
                    power -= 1;
                }

                power >>= 1;
                if (power > 0)
                    current *= current;
            }

            return result;
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        private static float Exp(float power)
        {
            int integerPart = 0;
            if (power > 1f)
            {
                float diff = MathF.Floor(power);
                power -= diff;
                integerPart += (int)diff;
            }
            else if (power < 0f)
            {
                float diff = MathF.Floor(MathF.Abs(power));
                if (diff > Int32.MaxValue)
                {
                    diff = Int32.MaxValue;
                    power = 0f;
                }
                else
                    power += diff;
                integerPart -= (int)diff;
            }

            float result = 1f;
            float acc = 1f;
            for (int i = 1; ; i++)
            {
                float prevResult = result;
                acc *= power / i;
                result += acc;
                if (prevResult == result)
                    break;
            }

            if (integerPart != 0)
                result *= PowI(MathF.E, integerPart);

            return result;
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        private static float LogE(float value)
        {
            if (value < 0f)
                return Single.NaN;

            int count = 0;
            while (value >= 1f)
            {
                value *= 1 / MathF.E;
                count += 1;
            }

            while (value <= 1 / MathF.E)
            {
                value *= MathF.E;
                count -= 1;
            }

            value -= 1f;
            if (value == 0f)
                return count;

            // going on with Taylor series
            float result = 0f;
            float acc = 1f;
            for (int i = 1; ; i++)
            {
                float prevResult = result;
                acc *= -value;
                result += acc / i;
                if (prevResult == result)
                    break;
            }

            return count - result;

        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static float Pow(this float value, float power)
        {
            // Faster if we calculate the result for the integer part fist, and then for the fractional
            //if (Math.Abs(power) <= (1 << 24))
            {
                int integerPart = (int)power;
                float diff = power - integerPart;
                float result = PowI(value, integerPart);
                if (diff > 0f)
                    result *= Exp(diff * LogE(value));

                return result;
            }

            //return Exp(power * LogE(value));
        }

        #endregion
    }
}
#endif