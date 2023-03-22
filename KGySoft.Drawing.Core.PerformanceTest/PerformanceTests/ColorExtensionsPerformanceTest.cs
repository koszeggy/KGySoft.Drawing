#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ColorExtensionsPerformanceTest.cs
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
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;
using System.Runtime.Intrinsics;

using KGySoft.CoreLibraries;
using KGySoft.Drawing.Imaging;

using NUnit.Framework;

#endregion

namespace KGySoft.Drawing.PerformanceTests
{
    [TestFixture]
    public class ColorExtensionsPerformanceTest
    {
        #region Methods

        [Test]
        public void GetBrightnessTest_Color32()
        {
            var testColor32 = new Color32(128, 255, 128, 64);
            byte expected = testColor32.GetBrightness();

            Console.WriteLine($"{"Test color:",-80} {testColor32}");
            void DoAssert(Expression<Func<Color32, byte>> e)
            {
                var m = (MethodCallExpression)e.Body;
                byte actual = e.Compile().Invoke(testColor32);
                Console.WriteLine($"{$"{m.Method.Name}:",-80} {actual}");
                Assert.AreEqual(expected, actual);
            }

            DoAssert(_ => testColor32.GetBrightness_0_Vanilla());
            DoAssert(_ => testColor32.GetBrightness_1_Vector());
            DoAssert(_ => testColor32.GetBrightness_2_Intrinsics());
#if NET7_0_OR_GREATER
            DoAssert(_ => testColor32.GetBrightness_2_IntrinsicsSum());
            DoAssert(_ => testColor32.GetBrightness_2_IntrinsicsDot());
#endif

            new PerformanceTest<byte> { TestName = "GetBrightness(Color32)", TestTime = 5000, Iterations = 10_000_000, Repeat = 3 }
                .AddCase(() => testColor32.GetBrightness_0_Vanilla(), nameof(ColorExtensions.GetBrightness_0_Vanilla))
                .AddCase(() => testColor32.GetBrightness_1_Vector(), nameof(ColorExtensions.GetBrightness_1_Vector))
                .AddCase(() => testColor32.GetBrightness_2_Intrinsics(), nameof(ColorExtensions.GetBrightness_2_Intrinsics))
#if NET7_0_OR_GREATER
                .AddCase(() => testColor32.GetBrightness_2_IntrinsicsSum(), nameof(ColorExtensions.GetBrightness_2_IntrinsicsSum))
                .AddCase(() => testColor32.GetBrightness_2_IntrinsicsDot(), nameof(ColorExtensions.GetBrightness_2_IntrinsicsDot))
#endif
                .DoTest()
                .DumpResults(Console.Out);
        }

        [Test]
        public void GetBrightnessTest_ColorF()
        {
            var testColorF = new ColorF(0.5f, 1f, 0.5f, 0.25f);
            float expected = testColorF.GetBrightness();

            Console.WriteLine($"{"Test color:",-80} {testColorF}");
            void DoAssert(Expression<Func<ColorF, float>> e)
            {
                var m = (MethodCallExpression)e.Body;
                float actual = e.Compile().Invoke(testColorF);
                Console.WriteLine($"{$"{m.Method.Name}:",-80} {actual}");
                Assert.AreEqual(expected, actual);
            }

            DoAssert(_ => testColorF.GetBrightness_0_Vanilla());
            DoAssert(_ => testColorF.GetBrightness_1_Vector());
            DoAssert(_ => testColorF.GetBrightness_2_Intrinsics());

            new PerformanceTest<float> { TestName = "GetBrightness(ColorF)", TestTime = 5000, Iterations = 10_000_000, Repeat = 3 }
                .AddCase(() => testColorF.GetBrightness_0_Vanilla(), nameof(ColorExtensions.GetBrightness_0_Vanilla))
                .AddCase(() => testColorF.GetBrightness_1_Vector(), nameof(ColorExtensions.GetBrightness_1_Vector))
                .AddCase(() => testColorF.GetBrightness_2_Intrinsics(), nameof(ColorExtensions.GetBrightness_2_Intrinsics))
                .DoTest()
                .DumpResults(Console.Out);

        }

        [Test]
        public void GetBrightnessFTest_Color32()
        {
            var testColor32 = new Color32(128, 255, 128, 64);
            float expected = testColor32.GetBrightnessF();

            Console.WriteLine($"{"Test color:",-80} {testColor32}");
            void DoAssert(Expression<Func<Color32, float>> e)
            {
                var m = (MethodCallExpression)e.Body;
                float actual = e.Compile().Invoke(testColor32);
                Console.WriteLine($"{$"{m.Method.Name}:",-80} {actual}");
                //Assert.IsTrue(expected.TolerantEquals(actual), $"{expected} vs. {actual}");
                Assert.AreEqual(expected, actual);
            }

            DoAssert(_ => testColor32.GetBrightnessF_0_Vanilla());
            DoAssert(_ => testColor32.GetBrightnessF_1_Vector());
            DoAssert(_ => testColor32.GetBrightnessF_2_Intrinsics());

            new PerformanceTest<float> { TestName = "GetBrightnessF(Color32)", TestTime = 5000, Iterations = 10_000_000, Repeat = 3 }
                .AddCase(() => testColor32.GetBrightnessF_0_Vanilla(), nameof(ColorExtensions.GetBrightnessF_0_Vanilla))
                .AddCase(() => testColor32.GetBrightnessF_1_Vector(), nameof(ColorExtensions.GetBrightnessF_1_Vector))
                .AddCase(() => testColor32.GetBrightnessF_2_Intrinsics(), nameof(ColorExtensions.GetBrightnessF_2_Intrinsics))
                .DoTest()
                .DumpResults(Console.Out);
        }

        [Test]
        public void GetBrightnessFTest_Color64()
        {
            var testColor64 = new Color32(128, 255, 128, 64).ToColor64();
            float expected = testColor64.GetBrightnessF();

            Console.WriteLine($"{"Test color:",-80} {testColor64}");
            void DoAssert(Expression<Func<Color64, float>> e)
            {
                var m = (MethodCallExpression)e.Body;
                float actual = e.Compile().Invoke(testColor64);
                Console.WriteLine($"{$"{m.Method.Name}:",-80} {actual}");
                Assert.IsTrue(expected.TolerantEquals(actual), $"{expected} vs. {actual}");
                //Assert.AreEqual(expected, actual);
            }

            DoAssert(_ => testColor64.GetBrightnessF_0_Vanilla());
            DoAssert(_ => testColor64.GetBrightnessF_1_Vector());
            DoAssert(_ => testColor64.GetBrightnessF_2_Intrinsics());

            new PerformanceTest<float> { TestName = "GetBrightnessF(Color64)", TestTime = 5000, Iterations = 10_000_000, Repeat = 3 }
                .AddCase(() => testColor64.GetBrightnessF_0_Vanilla(), nameof(ColorExtensions.GetBrightnessF_0_Vanilla))
                .AddCase(() => testColor64.GetBrightnessF_1_Vector(), nameof(ColorExtensions.GetBrightnessF_1_Vector))
                .AddCase(() => testColor64.GetBrightnessF_2_Intrinsics(), nameof(ColorExtensions.GetBrightnessF_2_Intrinsics))
                .DoTest()
                .DumpResults(Console.Out);
        }

        #endregion
    }

    internal static partial class ColorExtensions
    {
        #region Constants

        private const float RLumSrgb = 0.299f;
        private const float GLumSrgb = 0.587f;
        private const float BLumSrgb = 0.114f;

        private const float RLumLinear = 0.2126f;
        private const float GLumLinear = 0.7152f;
        private const float BLumLinear = 0.0722f;

        #endregion

        #region Properties

#if NET5_0_OR_GREATER
        // In .NET 5.0 and above these perform better as inlined rather than caching a static field
        private static Vector128<float> GrayscaleCoefficientsSrgb => Vector128.Create(BLumSrgb, GLumSrgb, RLumSrgb, default);
#elif NETCOREAPP3_0_OR_GREATER
        private static Vector128<float> GrayscaleCoefficientsSrgb { get; } = Vector128.Create(BLumSrgb, GLumSrgb, RLumSrgb, default);
#endif

        #endregion

        #region Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static byte GetBrightness_0_Vanilla(this Color32 c)
            => c.R == c.G && c.R == c.B
                ? c.R
                : (byte)(c.R * RLumSrgb + c.G * GLumSrgb + c.B * BLumSrgb);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static byte GetBrightness_1_Vector(this Color32 c)
        {
            if (c.R == c.G && c.R == c.B)
                return c.R;

            var rgb = new Vector3(c.R, c.G, c.B) * new Vector3(RLumSrgb, GLumSrgb, BLumSrgb);
            return (byte)(rgb.X + rgb.Y + rgb.Z);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static byte GetBrightness_2_Intrinsics(this Color32 c)
        {
            if (c.R == c.G && c.R == c.B)
                return c.R;

            // Converting the [A]RGB values to float (order is BGRA because we reinterpret the original value as bytes if supported)
            Vector128<float> bgrF = Sse2.ConvertToVector128Single(Sse41.IsSupported
                // Reinterpreting the uint value as bytes and converting them to ints in one step is still faster than converting them separately
                ? Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(c.Value).AsByte())
                // Cannot do the conversion in one step. Sparing one conversion because A is actually not needed here.
                : Vector128.Create(c.B, c.G, c.R, default));

            var result = Sse.Multiply(bgrF, Vector128.Create(BLumSrgb, GLumSrgb, RLumSrgb, default));
            return (byte)(result.GetElement(0) + result.GetElement(1) + result.GetElement(2));
        }

#if NET7_0_OR_GREATER
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static byte GetBrightness_2_IntrinsicsSum(this Color32 c)
        {
            if (c.R == c.G && c.R == c.B)
                return c.R;

            // Converting the [A]RGB values to float (order is BGRA because we reinterpret the original value as bytes if supported)
            Vector128<float> bgrF = Sse2.ConvertToVector128Single(Sse41.IsSupported
                // Reinterpreting the uint value as bytes and converting them to ints in one step is still faster than converting them separately
                ? Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(c.Value).AsByte())
                // Cannot do the conversion in one step. Sparing one conversion because A is actually not needed here.
                : Vector128.Create(c.B, c.G, c.R, default));

            var result = Sse.Multiply(bgrF, Vector128.Create(BLumSrgb, GLumSrgb, RLumSrgb, default));
            return (byte)Vector128.Sum(result);
        }
#endif

#if NET7_0_OR_GREATER
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static byte GetBrightness_2_IntrinsicsDot(this Color32 c)
        {
            if (c.R == c.G && c.R == c.B)
                return c.R;

            // Converting the [A]RGB values to float (order is BGRA because we reinterpret the original value as bytes if supported)
            Vector128<float> bgrF = Sse2.ConvertToVector128Single(Sse41.IsSupported
                // Reinterpreting the uint value as bytes and converting them to ints in one step is still faster than converting them separately
                ? Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(c.Value).AsByte())
                // Cannot do the conversion in one step. Sparing one conversion because A is actually not needed here.
                : Vector128.Create(c.B, c.G, c.R, default));

            return (byte)Vector128.Dot(bgrF, Vector128.Create(BLumSrgb, GLumSrgb, RLumSrgb, default));
        }
#endif

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static float GetBrightness_0_Vanilla(this ColorF c)
            => c.R.Equals(c.G) && c.R.Equals(c.B)
                ? c.R
                : c.R * RLumLinear + c.G * GLumLinear + c.B * BLumLinear;

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static float GetBrightness_1_Vector(this ColorF c)
        {
            if (c.R.Equals(c.G) && c.R.Equals(c.B))
                return c.R;

            return Vector3.Dot(c.Rgb, new Vector3(RLumLinear, GLumLinear, BLumLinear));
            //var rgb = c.Rgb * new Vector3(RLumLinear, GLumLinear, BLumLinear);
            //return rgb.X + rgb.Y + rgb.Z;
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static float GetBrightness_2_Intrinsics(this ColorF c)
        {
            if (c.R.Equals(c.G) && c.R.Equals(c.B))
                return c.R;

            var result = Sse.Multiply(c.RgbaV128, Vector128.Create(RLumLinear, GLumLinear, BLumLinear, default));
#if NET5_0_OR_GREATER
            return Vector128.Sum(result);
#else
            return result.GetElement(0) + result.GetElement(1) + result.GetElement(2);
#endif
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static float GetBrightnessF_0_Vanilla(this Color32 c)
            => c.R * RLumSrgb / Byte.MaxValue + c.G * GLumSrgb / Byte.MaxValue + c.B * BLumSrgb / Byte.MaxValue;

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static float GetBrightnessF_1_Vector(this Color32 c)
            => Vector3.Dot(new Vector3(c.R, c.G, c.B), new Vector3(RLumSrgb / Byte.MaxValue, GLumSrgb / Byte.MaxValue, BLumSrgb / Byte.MaxValue));

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static float GetBrightnessF_2_Intrinsics(this Color32 c)
        {
#if NETCOREAPP3_0_OR_GREATER
            if (Sse2.IsSupported)
            {
                Vector128<float> bgrF = Sse2.ConvertToVector128Single(Sse41.IsSupported
                    ? Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(c.Value).AsByte())
                    : Vector128.Create(c.B, c.G, c.R, default));

                //if (Sse41.IsSupported)
                //{
                //    return Sse41.DotProduct(bgrF, Vector128.Create(BLumSrgb / Byte.MaxValue, GLumSrgb / Byte.MaxValue, RLumSrgb / Byte.MaxValue, default), 0b_0111_0001).ToScalar();
                //}

                Vector128<float> result = Sse.Multiply(bgrF, Vector128.Create(BLumSrgb / Byte.MaxValue, GLumSrgb / Byte.MaxValue, RLumSrgb / Byte.MaxValue, default));
                return result.GetElement(0) + result.GetElement(1) + result.GetElement(2);
            }
#endif
            return c.GetBrightnessF_0_Vanilla();
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static float GetBrightnessF_0_Vanilla(this Color64 c)
            => c.R * RLumSrgb / UInt16.MaxValue + c.G * GLumSrgb / UInt16.MaxValue + c.B * BLumSrgb / UInt16.MaxValue;

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static float GetBrightnessF_1_Vector(this Color64 c)
            => Vector3.Dot(new Vector3(c.R, c.G, c.B), new Vector3(RLumSrgb / UInt16.MaxValue, GLumSrgb / UInt16.MaxValue, BLumSrgb / UInt16.MaxValue));

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static float GetBrightnessF_2_Intrinsics(this Color64 c)
        {
#if NETCOREAPP3_0_OR_GREATER
            if (Sse2.IsSupported)
            {
                Vector128<float> bgrF = Sse2.ConvertToVector128Single(Sse41.IsSupported
                    ? Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(c.Value).AsUInt16())
                    : Vector128.Create(c.B, c.G, c.R, default));

                //if (Sse41.IsSupported)
                //{
                //    return Sse41.DotProduct(bgrF, Vector128.Create(BLumSrgb / UInt16.MaxValue, GLumSrgb / UInt16.MaxValue, RLumSrgb / UInt16.MaxValue, default), 0b_0111_0001).ToScalar();
                //}

                Vector128<float> result = Sse.Multiply(bgrF, Vector128.Create(BLumSrgb / UInt16.MaxValue, GLumSrgb / UInt16.MaxValue, RLumSrgb / UInt16.MaxValue, default));
                return result.GetElement(0) + result.GetElement(1) + result.GetElement(2);
            }
#endif
            return c.GetBrightnessF_0_Vanilla();
        }

        #endregion
    }
}