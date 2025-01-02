#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ColorExtensionsPerformanceTest.cs
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
using System.Linq.Expressions;
#if NET45_OR_GREATER || NETCOREAPP
using System.Numerics; 
#endif
using System.Runtime.CompilerServices;
#if NETCOREAPP3_0_OR_GREATER
using System.Runtime.Intrinsics.X86;
using System.Runtime.Intrinsics;
#endif

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

            Console.WriteLine($"{"Test color:",-40} {testColor32}");

            void DoAssert(Expression<Func<Color32, byte>> e)
            {
                var m = (MethodCallExpression)e.Body;
                byte actual = e.Compile().Invoke(testColor32);
                Console.WriteLine($"{$"{m.Method.Name}:",-40} {actual}");
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

            Console.WriteLine($"{"Test color:",-40} {testColorF}");
            void DoAssert(Expression<Func<ColorF, float>> e)
            {
                var m = (MethodCallExpression)e.Body;
                float actual = e.Compile().Invoke(testColorF);
                Console.WriteLine($"{$"{m.Method.Name}:",-40} {actual}");
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

            Console.WriteLine($"{"Test color:",-40} {testColor32}");
            void DoAssert(Expression<Func<Color32, float>> e)
            {
                var m = (MethodCallExpression)e.Body;
                float actual = e.Compile().Invoke(testColor32);
                Console.WriteLine($"{$"{m.Method.Name}:",-40} {actual}");
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

            Console.WriteLine($"{"Test color:",-40} {testColor64}");
            void DoAssert(Expression<Func<Color64, float>> e)
            {
                var m = (MethodCallExpression)e.Body;
                float actual = e.Compile().Invoke(testColor64);
                Console.WriteLine($"{$"{m.Method.Name}:",-40} {actual}");
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

        [Test]
        public void BlendWithBackgroundSrgbTest_Color32()
        {
            var testColor32 = new Color32(128, 255, 128, 64);
            //var testColor32 = new Color32(254, 255, 128, 64);
            //var testColor32 = new Color32(1, 255, 128, 64);
            Color32 expected = testColor32.BlendWithBackgroundSrgb(Color32.Black);

            Console.WriteLine($"{"Expected color:",-50} {expected}");
            void DoAssert(Expression<Func<Color32>> e)
            {
                var m = (MethodCallExpression)e.Body;
                Color32 actual = e.Compile().Invoke();
                Console.WriteLine($"{$"{m.Method.Name}:",-50} {actual}");
                Assert.IsTrue(expected.TolerantEquals(actual, 1), $"{expected} vs. {actual}");
            }

            DoAssert(() => testColor32.BlendWithBackgroundSrgb_0_VanillaShift(Color32.Black));
            DoAssert(() => testColor32.BlendWithBackgroundSrgb_1_VanillaDiv(Color32.Black));
            DoAssert(() => testColor32.BlendWithBackgroundSrgb_2_IntrinsicsShift(Color32.Black));
            DoAssert(() => testColor32.BlendWithBackgroundSrgb_3_IntrinsicsDivSse41(Color32.Black));
            DoAssert(() => testColor32.BlendWithBackgroundSrgb_4_IntrinsicsDivSsse3(Color32.Black));
            DoAssert(() => testColor32.BlendWithBackgroundSrgb_5_IntrinsicsDivSse2(Color32.Black));

            new PerformanceTest<Color32> { TestName = "BlendWithBackgroundSrgb(Color32,Color32)", TestTime = 5000, Iterations = 10_000_000, Repeat = 3 }
                .AddCase(() => testColor32.BlendWithBackgroundSrgb_0_VanillaShift(Color32.Black), nameof(ColorExtensions.BlendWithBackgroundSrgb_0_VanillaShift))
                .AddCase(() => testColor32.BlendWithBackgroundSrgb_1_VanillaDiv(Color32.Black), nameof(ColorExtensions.BlendWithBackgroundSrgb_1_VanillaDiv))
                .AddCase(() => testColor32.BlendWithBackgroundSrgb_2_IntrinsicsShift(Color32.Black), nameof(ColorExtensions.BlendWithBackgroundSrgb_2_IntrinsicsShift))
                .AddCase(() => testColor32.BlendWithBackgroundSrgb_3_IntrinsicsDivSse41(Color32.Black), nameof(ColorExtensions.BlendWithBackgroundSrgb_3_IntrinsicsDivSse41))
                .AddCase(() => testColor32.BlendWithBackgroundSrgb_4_IntrinsicsDivSsse3(Color32.Black), nameof(ColorExtensions.BlendWithBackgroundSrgb_4_IntrinsicsDivSsse3))
                .AddCase(() => testColor32.BlendWithBackgroundSrgb_5_IntrinsicsDivSse2(Color32.Black), nameof(ColorExtensions.BlendWithBackgroundSrgb_5_IntrinsicsDivSse2))
                .DoTest()
                .DumpResults(Console.Out);

            // Verdict: Using shifting, which is the fastest one both with vanilla and intrinsic operations, and have the same results.
            //          The only drawback: if the CPU supports SSSE3 only, then division would be faster.

            // 1. BlendWithBackgroundSrgb_2_IntrinsicsShift: average time: 40,82 ms
            //   #1          40,14 ms	 <---- Best
            //   #2          40,86 ms
            //   #3          41,48 ms	 <---- Worst
            //   Worst-Best difference: 1,34 ms (3,35%)
            // 2. BlendWithBackgroundSrgb_3_IntrinsicsDivSse41: average time: 44,22 ms (+3,39 ms / 108,31%)
            //   #1          43,86 ms
            //   #2          43,34 ms	 <---- Best
            //   #3          45,44 ms	 <---- Worst
            //   Worst-Best difference: 2,10 ms (4,85%)
            // 3. BlendWithBackgroundSrgb_4_IntrinsicsDivSsse3: average time: 61,42 ms (+20,60 ms / 150,46%)
            //   #1          61,54 ms	 <---- Worst
            //   #2          61,48 ms
            //   #3          61,25 ms	 <---- Best
            //   Worst-Best difference: 0,29 ms (0,48%)
            // 4. BlendWithBackgroundSrgb_0_VanillaShift: average time: 80,55 ms (+39,72 ms / 197,30%)
            //   #1          80,70 ms
            //   #2          81,28 ms	 <---- Worst
            //   #3          79,66 ms	 <---- Best
            //   Worst-Best difference: 1,62 ms (2,04%)
            // 5. BlendWithBackgroundSrgb_5_IntrinsicsDivSse2: average time: 93,96 ms (+53,13 ms / 230,14%)
            //   #1          95,15 ms	 <---- Worst
            //   #2          92,71 ms	 <---- Best
            //   #3          94,01 ms
            //   Worst-Best difference: 2,44 ms (2,63%)
            // 6. BlendWithBackgroundSrgb_1_VanillaDiv: average time: 98,42 ms (+57,59 ms / 241,07%)
            //   #1          98,79 ms
            //   #2          99,60 ms	 <---- Worst
            //   #3          96,86 ms	 <---- Best
            //   Worst-Best difference: 2,73 ms (2,82%)
        }

        [Test]
        public void BlendWithSrgbTest_Color32()
        {
            var src = new Color32(128, 255, 128, 64);
            //var src = new Color32(254, 255, 128, 64);
            //var src = new Color32(1, 255, 128, 64);
            var dst = new Color32(128, 64, 128, 255);
            //var dst = new Color32(128, 0, 0, 0);
            //var dst = new Color32(254, 0, 0, 0);
            //var dst = new Color32(1, 0, 0, 0); // NOTE: BlendWithSrgb_1_VanillaInt fails if A has 1, too 
            Color32 expected = src.BlendWithSrgb(dst);

            Console.WriteLine($"{"Expected color:",-40} {expected}");

            void DoAssert(Expression<Func<Color32>> e)
            {
                var m = (MethodCallExpression)e.Body;
                Color32 actual = e.Compile().Invoke();
                Console.WriteLine($"{$"{m.Method.Name}:",-40} {actual}");
                Assert.IsTrue(expected.TolerantEquals(actual, 1, 0), $"{expected} vs. {actual}");
            }

            DoAssert(() => src.BlendWithSrgb_0_VanillaFloat(dst));
            DoAssert(() => src.BlendWithSrgb_1_VanillaInt(dst));
            DoAssert(() => src.BlendWithSrgb_2_IntrinsicFloatSse41(dst));
            DoAssert(() => src.BlendWithSrgb_3_IntrinsicFloatSsse3(dst));
            DoAssert(() => src.BlendWithSrgb_4_IntrinsicFloatSse2(dst));

            new PerformanceTest<Color32> { TestName = "BlendWithSrgb(Color32,Color32)", TestTime = 5000, Iterations = 10_000_000, Repeat = 3 }
                .AddCase(() => src.BlendWithSrgb_0_VanillaFloat(dst), nameof(ColorExtensions.BlendWithSrgb_0_VanillaFloat))
                .AddCase(() => src.BlendWithSrgb_1_VanillaInt(dst), nameof(ColorExtensions.BlendWithSrgb_1_VanillaInt))
                .AddCase(() => src.BlendWithSrgb_2_IntrinsicFloatSse41(dst), nameof(ColorExtensions.BlendWithSrgb_2_IntrinsicFloatSse41))
                .AddCase(() => src.BlendWithSrgb_3_IntrinsicFloatSsse3(dst), nameof(ColorExtensions.BlendWithSrgb_3_IntrinsicFloatSsse3))
                .AddCase(() => src.BlendWithSrgb_4_IntrinsicFloatSse2(dst), nameof(ColorExtensions.BlendWithSrgb_4_IntrinsicFloatSse2))
                .DoTest()
                .DumpResults(Console.Out);

            // 1. BlendWithSrgb_2_IntrinsicFloatSse41: average time: 49,73 ms
            //   #1          49,60 ms	 <---- Best
            //   #2          49,86 ms	 <---- Worst
            //   #3          49,75 ms
            //   Worst-Best difference: 0,27 ms (0,54%)
            // 2. BlendWithSrgb_0_VanillaFloat: average time: 107,57 ms (+57,83 ms / 216,28%)
            //   #1         105,68 ms	 <---- Best
            //   #2         107,75 ms
            //   #3         109,28 ms	 <---- Worst
            //   Worst-Best difference: 3,60 ms (3,41%)
            // 3. BlendWithSrgb_1_VanillaInt: average time: 108,49 ms (+58,75 ms / 218,13%)
            //   #1         108,93 ms	 <---- Worst
            //   #2         108,24 ms	 <---- Best
            //   #3         108,30 ms
            //   Worst-Best difference: 0,69 ms (0,64%)
            // 4. BlendWithSrgb_3_IntrinsicFloatSsse3: average time: 166,88 ms (+117,15 ms / 335,55%)
            //   #1         167,27 ms
            //   #2         165,84 ms	 <---- Best
            //   #3         167,55 ms	 <---- Worst
            //   Worst-Best difference: 1,71 ms (1,03%)
            // 5. BlendWithSrgb_4_IntrinsicFloatSse2: average time: 206,12 ms (+156,38 ms / 414,43%)
            //   #1         206,66 ms	 <---- Worst
            //   #2         206,07 ms
            //   #3         205,62 ms	 <---- Best
            //   Worst-Best difference: 1,05 ms (0,51%)
        }

        [Test]
        public void BlendWithSrgbTest_PColor32()
        {
            var src = new Color32(128, 255, 128, 64).ToPColor32();
            //var src = new Color32(254, 255, 128, 64).ToPColor32();
            //var src = new Color32(1, 255, 128, 64).ToPColor32();
            var dst = new Color32(128, 64, 128, 255).ToPColor32();
            //var dst = new Color32(128, 0, 0, 0).ToPColor32();
            //var dst = new Color32(254, 0, 0, 0).ToPColor32();
            //var dst = Color32.Black.ToPColor32();
            PColor32 expected = src.BlendWithSrgb(dst);

            Console.WriteLine($"{"Expected color:",-40} {expected}");

            void DoAssert(Expression<Func<PColor32>> e)
            {
                var m = (MethodCallExpression)e.Body;
                PColor32 actual = e.Compile().Invoke();
                Console.WriteLine($"{$"{m.Method.Name}:",-40} {actual}");
                Assert.IsTrue(actual.IsValid, $"Invalid result: {actual}");
                Assert.IsTrue(expected.TolerantEquals(actual, 1), $"{expected} vs. {actual}");
            }

            DoAssert(() => src.BlendWithSrgb_0_VanillaShift(dst));
            DoAssert(() => src.BlendWithSrgb_1_VanillaDiv(dst));
            DoAssert(() => src.BlendWithSrgb_2_IntrinsicsShift(dst));

            new PerformanceTest<PColor32> { TestName = "BlendWithSrgb(PColor32,PColor32)", TestTime = 5000, Iterations = 10_000_000, Repeat = 3 }
                .AddCase(() => src.BlendWithSrgb_0_VanillaShift(dst), nameof(ColorExtensions.BlendWithSrgb_0_VanillaShift))
                .AddCase(() => src.BlendWithSrgb_1_VanillaDiv(dst), nameof(ColorExtensions.BlendWithSrgb_1_VanillaDiv))
                .AddCase(() => src.BlendWithSrgb_2_IntrinsicsShift(dst), nameof(ColorExtensions.BlendWithSrgb_2_IntrinsicsShift))
                .DoTest()
                .DumpResults(Console.Out);

            // 1. BlendWithSrgb_2_IntrinsicsShift: average time: 37,63 ms
            //   #1          37,86 ms	 <---- Worst
            //   #2          37,59 ms
            //   #3          37,44 ms	 <---- Best
            //   Worst-Best difference: 0,42 ms (1,13%)
            // 2. BlendWithSrgb_0_VanillaShift: average time: 77,35 ms (+39,72 ms / 205,56%)
            //   #1          78,05 ms	 <---- Worst
            //   #2          76,95 ms	 <---- Best
            //   #3          77,04 ms
            //   Worst-Best difference: 1,09 ms (1,42%)
            // 3. BlendWithSrgb_1_VanillaDiv: average time: 101,08 ms (+63,45 ms / 268,63%)
            //   #1         101,90 ms	 <---- Worst
            //   #2         100,00 ms	 <---- Best
            //   #3         101,34 ms
            //   Worst-Best difference: 1,90 ms (1,90%)
        }

        [Test]
        public void BlendWithBackgroundSrgbTest_Color64()
        {
            var backColor = Color32.Black.ToColor64();
            var testColor64 = new Color32(128, 255, 128, 64).ToColor64();
            //var testColor64 = new Color64(65534, 65535, 32768, 16384);
            //var testColor64 = new Color64(1, 65535, 32768, 16384);
            Color64 expected = testColor64.BlendWithBackgroundSrgb(backColor);

            Console.WriteLine($"{"Expected color:",-50} {expected}");

            void DoAssert(Expression<Func<Color64>> e)
            {
                var m = (MethodCallExpression)e.Body;
                Color64 actual = e.Compile().Invoke();
                Console.WriteLine($"{$"{m.Method.Name}:",-50} {actual}");
                Assert.IsTrue(expected.TolerantEquals(actual, 1), $"{expected} vs. {actual}");
            }

            DoAssert(() => testColor64.BlendWithBackgroundSrgb_0_VanillaShift(backColor));
            DoAssert(() => testColor64.BlendWithBackgroundSrgb_1_VanillaDiv(backColor));
            DoAssert(() => testColor64.BlendWithBackgroundSrgb_2_IntrinsicsShift(backColor));

            new PerformanceTest<Color64> { TestName = "BlendWithBackgroundSrgb(Color64,Color64)", TestTime = 5000, Iterations = 10_000_000, Repeat = 3 }
                .AddCase(() => testColor64.BlendWithBackgroundSrgb_0_VanillaShift(backColor), nameof(ColorExtensions.BlendWithBackgroundSrgb_0_VanillaShift))
                .AddCase(() => testColor64.BlendWithBackgroundSrgb_1_VanillaDiv(backColor), nameof(ColorExtensions.BlendWithBackgroundSrgb_1_VanillaDiv))
                .AddCase(() => testColor64.BlendWithBackgroundSrgb_2_IntrinsicsShift(backColor), nameof(ColorExtensions.BlendWithBackgroundSrgb_2_IntrinsicsShift))
                .DoTest()
                .DumpResults(Console.Out);

            // 1. BlendWithBackgroundSrgb_2_IntrinsicsShift: average time: 43,72 ms
            //   #1          43,95 ms
            //   #2          42,72 ms	 <---- Best
            //   #3          44,50 ms	 <---- Worst
            //   Worst-Best difference: 1,77 ms (4,15%)
            // 2. BlendWithBackgroundSrgb_0_VanillaShift: average time: 74,77 ms (+31,05 ms / 171,01%)
            //   #1          75,49 ms	 <---- Worst
            //   #2          74,77 ms
            //   #3          74,03 ms	 <---- Best
            //   Worst-Best difference: 1,46 ms (1,97%)
            // 3. BlendWithBackgroundSrgb_1_VanillaDiv: average time: 81,77 ms (+38,05 ms / 187,02%)
            //   #1          81,81 ms
            //   #2          81,55 ms	 <---- Best
            //   #3          81,95 ms	 <---- Worst
            //   Worst-Best difference: 0,40 ms (0,50%)
        }

        [Test]
        public void BlendWithSrgbTest_Color64()
        {
            var src = new Color32(128, 255, 128, 64).ToColor64();
            //var src = new Color64(65534, 65535, 32768, 16384);
            //var src = new Color64(1, 65535, 32768, 16384);
            var dst = new Color32(128, 64, 128, 255).ToColor64();
            //var src = new Color64(32768, 0, 0, 0);
            //var src = new Color64(65534, 0, 0, 0);
            //var src = new Color64(1, 0, 0, 0); // NOTE: BlendWithSrgb_1_VanillaInt fails if A has 1, too 
            Color64 expected = src.BlendWithSrgb(dst);

            Console.WriteLine($"{"Expected color:",-40} {expected}");

            void DoAssert(Expression<Func<Color64>> e)
            {
                var m = (MethodCallExpression)e.Body;
                Color64 actual = e.Compile().Invoke();
                Console.WriteLine($"{$"{m.Method.Name}:",-40} {actual}");
                Assert.IsTrue(expected.TolerantEquals(actual, 1, 0), $"{expected} vs. {actual}");
            }

            DoAssert(() => src.BlendWithSrgb_0_VanillaFloat(dst));
            DoAssert(() => src.BlendWithSrgb_1_VanillaInt(dst));
            DoAssert(() => src.BlendWithSrgb_2_IntrinsicFloatSse41(dst));

            new PerformanceTest<Color64> { TestName = "BlendWithSrgb(Color64,Color64)", TestTime = 5000, Iterations = 10_000_000, Repeat = 3 }
                .AddCase(() => src.BlendWithSrgb_0_VanillaFloat(dst), nameof(ColorExtensions.BlendWithSrgb_0_VanillaFloat))
                .AddCase(() => src.BlendWithSrgb_1_VanillaInt(dst), nameof(ColorExtensions.BlendWithSrgb_1_VanillaInt))
                .AddCase(() => src.BlendWithSrgb_2_IntrinsicFloatSse41(dst), nameof(ColorExtensions.BlendWithSrgb_2_IntrinsicFloatSse41))
                .DoTest()
                .DumpResults(Console.Out);

            // 1. BlendWithSrgb_2_IntrinsicFloatSse41: average time: 51,28 ms
            //   #1          51,67 ms	 <---- Worst
            //   #2          51,64 ms
            //   #3          50,54 ms	 <---- Best
            //   Worst-Best difference: 1,13 ms (2,24%)
            // 2. BlendWithSrgb_0_VanillaFloat: average time: 120,90 ms (+69,62 ms / 235,77%)
            //   #1         116,62 ms	 <---- Best
            //   #2         125,54 ms	 <---- Worst
            //   #3         120,54 ms
            //   Worst-Best difference: 8,92 ms (7,65%)
            // 3. BlendWithSrgb_1_VanillaInt: average time: 235,74 ms (+184,45 ms / 459,70%)
            //   #1         235,57 ms
            //   #2         239,28 ms	 <---- Worst
            //   #3         232,36 ms	 <---- Best
            //   Worst-Best difference: 6,92 ms (2,98%)
        }

        [Test]
        public void BlendWithSrgbTest_PColor64()
        {
            var src = new Color32(128, 255, 128, 64).ToPColor64();
            //var src = new Color64(32768, 65535, 32768, 16384).ToPColor64();
            //var src = new Color64(65534, 65535, 32768, 16384).ToPColor64();
            //var src = new Color64(1, 65535, 32768, 16384).ToPColor64();
            var dst = new Color32(128, 64, 128, 255).ToPColor64();
            //var dst = new PColor64(65534, 0, 0, 0);
            //var dst = new PColor64(32768, 0, 0, 0);
            //var dst = new PColor64(1, 0, 0, 0);
            //var dst = Color32.Black.ToPColor64();
            PColor64 expected = src.BlendWithSrgb(dst);

            Console.WriteLine($"{"Expected color:",-40} {expected}");

            void DoAssert(Expression<Func<PColor64>> e)
            {
                var m = (MethodCallExpression)e.Body;
                PColor64 actual = e.Compile().Invoke();
                Console.WriteLine($"{$"{m.Method.Name}:",-40} {actual}");
                Assert.IsTrue(actual.IsValid, $"Invalid result: {actual}");
                Assert.IsTrue(expected.TolerantEquals(actual, 1), $"{expected} vs. {actual}");
            }

            DoAssert(() => src.BlendWithSrgb_0_VanillaShift(dst));
            DoAssert(() => src.BlendWithSrgb_1_VanillaDiv(dst));
            DoAssert(() => src.BlendWithSrgb_2_IntrinsicsShift(dst));

            new PerformanceTest<PColor64> { TestName = "BlendWithSrgb(PColor64,PColor64)", TestTime = 5000, Iterations = 10_000_000, Repeat = 3 }
                .AddCase(() => src.BlendWithSrgb_0_VanillaShift(dst), nameof(ColorExtensions.BlendWithSrgb_0_VanillaShift))
                .AddCase(() => src.BlendWithSrgb_1_VanillaDiv(dst), nameof(ColorExtensions.BlendWithSrgb_1_VanillaDiv))
                .AddCase(() => src.BlendWithSrgb_2_IntrinsicsShift(dst), nameof(ColorExtensions.BlendWithSrgb_2_IntrinsicsShift))
                .DoTest()
                .DumpResults(Console.Out);

            // 1. BlendWithSrgb_2_IntrinsicsShift: average time: 43,10 ms
            //   #1          42,24 ms	 <---- Best
            //   #2          44,09 ms	 <---- Worst
            //   #3          42,98 ms
            //   Worst-Best difference: 1,85 ms (4,37%)
            // 2. BlendWithSrgb_0_VanillaShift: average time: 76,81 ms (+33,71 ms / 178,21%)
            //   #1          76,81 ms
            //   #2          76,46 ms	 <---- Best
            //   #3          77,17 ms	 <---- Worst
            //   Worst-Best difference: 0,71 ms (0,93%)
            // 3. BlendWithSrgb_1_VanillaDiv: average time: 102,70 ms (+59,60 ms / 238,28%)
            //   #1         100,65 ms	 <---- Best
            //   #2         101,68 ms
            //   #3         105,78 ms	 <---- Worst
            //   Worst-Best difference: 5,13 ms (5,10%)
        }

        [Test]
        public void BlendWithBackgroundLinearTest_ColorF()
        {
            var backColor = Color32.Black.ToColorF();
            var testColorF = new Color32(128, 255, 128, 64).ToColorF();
            //var testColorF = new ColorF(0.99999994f, 1f, 0.5f, 0.25f);
            //var testColorF = new ColorF(0.00000006f, 1f, 0.5f, 0.25f);
            //var testColorF = new ColorF(0.00000002f, 1f, 0.5f, 0.25f);
            ColorF expected = testColorF.BlendWithBackgroundLinear(backColor);

            Console.WriteLine($"{"Expected color:",-50} {expected}");

            void DoAssert(Expression<Func<ColorF>> e)
            {
                var m = (MethodCallExpression)e.Body;
                ColorF actual = e.Compile().Invoke();
                Console.WriteLine($"{$"{m.Method.Name}:",-50} {actual}");
                Assert.IsTrue(expected.TolerantEquals(actual), $"{expected} vs. {actual}");
            }

            DoAssert(() => testColorF.BlendWithBackgroundLinear_0_Vanilla(backColor));
            DoAssert(() => testColorF.BlendWithBackgroundLinear_1_Vector(backColor));
            DoAssert(() => testColorF.BlendWithBackgroundLinear_2_Intrinsic(backColor));

            new PerformanceTest<ColorF> { TestName = "BlendWithBackgroundLinear(ColorF,ColorF)", TestTime = 5000, Iterations = 10_000_000, Repeat = 3 }
                .AddCase(() => testColorF.BlendWithBackgroundLinear_0_Vanilla(backColor), nameof(ColorExtensions.BlendWithBackgroundLinear_0_Vanilla))
                .AddCase(() => testColorF.BlendWithBackgroundLinear_1_Vector(backColor), nameof(ColorExtensions.BlendWithBackgroundLinear_1_Vector))
                .AddCase(() => testColorF.BlendWithBackgroundLinear_2_Intrinsic(backColor), nameof(ColorExtensions.BlendWithBackgroundLinear_2_Intrinsic))
                .DoTest()
                .DumpResults(Console.Out);

            // 1. BlendWithBackgroundLinear_2_Intrinsic: average time: 40,68 ms
            //   #1          40,60 ms	 <---- Best
            //   #2          40,83 ms	 <---- Worst
            //   #3          40,62 ms
            //   Worst-Best difference: 0,22 ms (0,55%)
            // 2. BlendWithBackgroundLinear_1_Vector: average time: 42,82 ms (+2,14 ms / 105,26%)
            //   #1          43,75 ms	 <---- Worst
            //   #2          42,82 ms
            //   #3          41,91 ms	 <---- Best
            //   Worst-Best difference: 1,84 ms (4,39%)
            // 3. BlendWithBackgroundLinear_0_Vanilla: average time: 53,96 ms (+13,28 ms / 132,63%)
            //   #1          53,12 ms	 <---- Best
            //   #2          53,52 ms
            //   #3          55,24 ms	 <---- Worst
            //   Worst-Best difference: 2,12 ms (3,99%)
        }

        [Test]
        public void BlendWithLinearTest_ColorF()
        {
            var src = new Color32(128, 255, 128, 64).ToColorF();
            //var src = new ColorF(0.99999994f, 1f, 0.5f, 0.25f);
            //var src = new ColorF(0.00000006f, 1f, 0.5f, 0.25f);
            //var src = new ColorF(0.00000002f, 1f, 0.5f, 0.25f);
            var dst = new Color32(128, 64, 128, 255).ToColorF();
            //var dst = new ColorF(0.99999994f, 0.25f, 0.5f, 1f);
            //var dst = new ColorF(0.00000006f, 0.25f, 0.5f, 1f);
            //var dst = new ColorF(0.00000002f, 0.25f, 0.5f, 1f);
            //var dst = Color32.Black.ToColorF();
            ColorF expected = src.BlendWithLinear(dst);

            Console.WriteLine($"{"Expected color:",-40} {expected}");

            void DoAssert(Expression<Func<ColorF>> e)
            {
                var m = (MethodCallExpression)e.Body;
                ColorF actual = e.Compile().Invoke();
                Console.WriteLine($"{$"{m.Method.Name}:",-40} {actual}");
                Assert.IsTrue(actual.IsValid, $"Invalid result: {actual}");
                Assert.IsTrue(expected.TolerantEquals(actual), $"{expected} vs. {actual}");
            }

            DoAssert(() => src.BlendWithLinear_0_Vanilla(dst));
            DoAssert(() => src.BlendWithLinear_1_Vector(dst));
            DoAssert(() => src.BlendWithLinear_2_Intrinsic(dst));

            new PerformanceTest<ColorF> { TestName = "BlendWithLinear(ColorF,ColorF)", TestTime = 5000, Iterations = 10_000_000, Repeat = 3 }
                .AddCase(() => src.BlendWithLinear_0_Vanilla(dst), nameof(ColorExtensions.BlendWithLinear_0_Vanilla))
                .AddCase(() => src.BlendWithLinear_1_Vector(dst), nameof(ColorExtensions.BlendWithLinear_1_Vector))
                .AddCase(() => src.BlendWithLinear_2_Intrinsic(dst), nameof(ColorExtensions.BlendWithLinear_2_Intrinsic))
                .DoTest()
                .DumpResults(Console.Out);

            // 1. BlendWithLinear_2_Intrinsic: average time: 40,35 ms
            //   #1          41,56 ms	 <---- Worst
            //   #2          39,76 ms
            //   #3          39,74 ms	 <---- Best
            //   Worst-Best difference: 1,83 ms (4,60%)
            // 2. BlendWithLinear_1_Vector: average time: 47,20 ms (+6,85 ms / 116,98%)
            //   #1          46,38 ms	 <---- Best
            //   #2          46,59 ms
            //   #3          48,64 ms	 <---- Worst
            //   Worst-Best difference: 2,27 ms (4,89%)
            // 3. BlendWithLinear_0_Vanilla: average time: 57,37 ms (+17,02 ms / 142,17%)
            //   #1          56,78 ms
            //   #2          59,47 ms	 <---- Worst
            //   #3          55,86 ms	 <---- Best
            //   Worst-Best difference: 3,61 ms (6,46%)
        }

        [Test]
        public void BlendWithLinearTest_PColorF()
        {
            var src = new Color32(128, 255, 128, 64).ToPColorF();
            //var src = new ColorF(0.99999994f, 1f, 0.5f, 0.25f).ToPColorF();
            //var src = new ColorF(0.00000006f, 1f, 0.5f, 0.25f).ToPColorF();
            //var src = new ColorF(0.00000002f, 1f, 0.5f, 0.25f).ToPColorF();
            var dst = new Color32(128, 64, 128, 255).ToPColorF();
            //var dst = new ColorF(0.99999994f, 0.25f, 0.5f, 1f).ToPColorF();
            //var dst = new ColorF(0.00000006f, 0.25f, 0.5f, 1f).ToPColorF();
            //var dst = new ColorF(0.00000002f, 0.25f, 0.5f, 1f).ToPColorF();
            //var dst = Color32.Black.ToPColorF();
            PColorF expected = src.BlendWithLinear(dst);

            Console.WriteLine($"{"Expected color:",-40} {expected}");

            void DoAssert(Expression<Func<PColorF>> e)
            {
                var m = (MethodCallExpression)e.Body;
                PColorF actual = e.Compile().Invoke();
                Console.WriteLine($"{$"{m.Method.Name}:",-40} {actual}");
                Assert.IsTrue(actual.IsValid, $"Invalid result: {actual}");
                Assert.IsTrue(expected.TolerantEquals(actual), $"{expected} vs. {actual}");
                Assert.IsTrue(expected.Equals(actual), $"{expected} vs. {actual}");
            }

            DoAssert(() => src.BlendWithLinear_0_Vanilla(dst));
            DoAssert(() => src.BlendWithLinear_1_Vector(dst));
            DoAssert(() => src.BlendWithLinear_2_IntrinsicSse(dst));
            DoAssert(() => src.BlendWithLinear_3_IntrinsicFma(dst));

            new PerformanceTest<PColorF> { TestName = "BlendWithLinear(PColorF,PColorF)", TestTime = 5000, Iterations = 10_000_000, Repeat = 3 }
                .AddCase(() => src.BlendWithLinear_0_Vanilla(dst), nameof(ColorExtensions.BlendWithLinear_0_Vanilla))
                .AddCase(() => src.BlendWithLinear_1_Vector(dst), nameof(ColorExtensions.BlendWithLinear_1_Vector))
                .AddCase(() => src.BlendWithLinear_2_IntrinsicSse(dst), nameof(ColorExtensions.BlendWithLinear_2_IntrinsicSse))
                .AddCase(() => src.BlendWithLinear_3_IntrinsicFma(dst), nameof(ColorExtensions.BlendWithLinear_3_IntrinsicFma))
                .DoTest()
                .DumpResults(Console.Out);

            // 1. BlendWithLinear_1_Vector: average time: 26,41 ms
            //   #1          26,27 ms
            //   #2          26,89 ms	 <---- Worst
            //   #3          26,07 ms	 <---- Best
            //   Worst-Best difference: 0,81 ms (3,12%)
            // 2. BlendWithLinear_2_IntrinsicSse: average time: 29,37 ms (+2,96 ms / 111,22%)
            //   #1          29,98 ms	 <---- Worst
            //   #2          28,67 ms	 <---- Best
            //   #3          29,47 ms
            //   Worst-Best difference: 1,31 ms (4,58%)
            // 3. BlendWithLinear_3_IntrinsicFma: average time: 30,29 ms (+3,89 ms / 114,71%)
            //   #1          30,75 ms	 <---- Worst
            //   #2          30,56 ms
            //   #3          29,57 ms	 <---- Best
            //   Worst-Best difference: 1,18 ms (3,99%)
            // 4. BlendWithLinear_0_Vanilla: average time: 63,48 ms (+37,07 ms / 240,37%)
            //   #1          64,57 ms
            //   #2          58,40 ms	 <---- Best
            //   #3          67,46 ms	 <---- Worst
            //   Worst-Best difference: 9,06 ms (15,51%)
        }

        #endregion
    }

    internal static class ColorExtensions
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
        private static Vector128<byte> PackLowBytesMask => Vector128.Create(0, 4, 8, 12, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF);
#elif NETCOREAPP3_0_OR_GREATER
        private static Vector128<byte> PackLowBytesMask { get; } = Vector128.Create(0, 4, 8, 12, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF);
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

#if NET45_OR_GREATER || NETCOREAPP
            var rgb = new Vector3(c.R, c.G, c.B) * new Vector3(RLumSrgb, GLumSrgb, BLumSrgb);
            return (byte)(rgb.X + rgb.Y + rgb.Z);
#else
            throw new PlatformNotSupportedException();
#endif
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static byte GetBrightness_2_Intrinsics(this Color32 c)
        {
            if (c.R == c.G && c.R == c.B)
                return c.R;

#if NETCOREAPP3_0_OR_GREATER
            // Converting the [A]RGB values to float (order is BGRA because we reinterpret the original value as bytes if supported)
            Vector128<float> bgrF = Sse2.ConvertToVector128Single(Sse41.IsSupported
                // Reinterpreting the uint value as bytes and converting them to ints in one step is still faster than converting them separately
                ? Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(c.Value).AsByte())
                // Cannot do the conversion in one step. Sparing one conversion because A is actually not needed here.
                : Vector128.Create(c.B, c.G, c.R, default));

            var result = Sse.Multiply(bgrF, Vector128.Create(BLumSrgb, GLumSrgb, RLumSrgb, default));
            return (byte)(result.GetElement(0) + result.GetElement(1) + result.GetElement(2));
#else
            throw new PlatformNotSupportedException();
#endif
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

#if NET45_OR_GREATER || NETCOREAPP
            return Vector3.Dot(c.Rgb, new Vector3(RLumLinear, GLumLinear, BLumLinear));
#else
            throw new PlatformNotSupportedException();
#endif
            //var rgb = c.Rgb * new Vector3(RLumLinear, GLumLinear, BLumLinear);
            //return rgb.X + rgb.Y + rgb.Z;
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static float GetBrightness_2_Intrinsics(this ColorF c)
        {
            if (c.R.Equals(c.G) && c.R.Equals(c.B))
                return c.R;

#if NETCOREAPP3_0_OR_GREATER
            var result = Sse.Multiply(c.RgbaV128, Vector128.Create(RLumLinear, GLumLinear, BLumLinear, default));
#if NET7_0_OR_GREATER
            return Vector128.Sum(result);
#else
            return result.GetElement(0) + result.GetElement(1) + result.GetElement(2);
#endif
#else
            throw new PlatformNotSupportedException();
#endif
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static float GetBrightnessF_0_Vanilla(this Color32 c)
            => c.R * RLumSrgb / Byte.MaxValue + c.G * GLumSrgb / Byte.MaxValue + c.B * BLumSrgb / Byte.MaxValue;

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static float GetBrightnessF_1_Vector(this Color32 c)
#if NET45_OR_GREATER || NETCOREAPP
            => Vector3.Dot(new Vector3(c.R, c.G, c.B), new Vector3(RLumSrgb / Byte.MaxValue, GLumSrgb / Byte.MaxValue, BLumSrgb / Byte.MaxValue));
#else
            => throw new PlatformNotSupportedException();
#endif

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
#if NET45_OR_GREATER || NETCOREAPP
            => Vector3.Dot(new Vector3(c.R, c.G, c.B), new Vector3(RLumSrgb / UInt16.MaxValue, GLumSrgb / UInt16.MaxValue, BLumSrgb / UInt16.MaxValue));
#else
            => throw new PlatformNotSupportedException();
#endif

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

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Color32 BlendWithBackgroundSrgb_0_VanillaShift(this Color32 c, Color32 backColor)
        {
            Debug.Assert(c.A != Byte.MaxValue, "Partially transparent fore color is expected. Call Blend for better performance.");
            Debug.Assert(backColor.A == Byte.MaxValue, "Totally opaque back color is expected.");

            // The blending is applied only to the color and not the resulting alpha, which will always be opaque
            if (c.A == 0)
                return backColor;
            int inverseAlpha = Byte.MaxValue - c.A;
            return new Color32(Byte.MaxValue,
                (byte)((c.R * c.A + backColor.R * inverseAlpha) >> 8),
                (byte)((c.G * c.A + backColor.G * inverseAlpha) >> 8),
                (byte)((c.B * c.A + backColor.B * inverseAlpha) >> 8));
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Color32 BlendWithBackgroundSrgb_1_VanillaDiv(this Color32 c, Color32 backColor)
        {
            Debug.Assert(c.A != Byte.MaxValue, "Partially transparent fore color is expected. Call Blend for better performance.");
            Debug.Assert(backColor.A == Byte.MaxValue, "Totally opaque back color is expected.");

            // The blending is applied only to the color and not the resulting alpha, which will always be opaque
            if (c.A == 0)
                return backColor;
            int inverseAlpha = Byte.MaxValue - c.A;
            return new Color32(Byte.MaxValue,
                (byte)((c.R * c.A + backColor.R * inverseAlpha) / 255),
                (byte)((c.G * c.A + backColor.G * inverseAlpha) / 255),
                (byte)((c.B * c.A + backColor.B * inverseAlpha) / 255));
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Color32 BlendWithBackgroundSrgb_2_IntrinsicsShift(this Color32 c, Color32 backColor)
        {
            Debug.Assert(c.A != Byte.MaxValue, "Partially transparent fore color is expected. Call Blend for better performance.");
            Debug.Assert(backColor.A == Byte.MaxValue, "Totally opaque back color is expected.");

            // The blending is applied only to the color and not the resulting alpha, which will always be opaque
            if (c.A == 0)
                return backColor;
            int inverseAlpha = Byte.MaxValue - c.A;

#if NETCOREAPP3_0_OR_GREATER
            if (Sse41.IsSupported)
            {
                // c.RGB * c.A
                Vector128<int> bgraI32 = Sse41.MultiplyLow(Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(c.Value).AsByte()), Sse41.ConvertToVector128Int32(Vector128.Create(c.A)));

                // backColor.RGB * inverseAlpha
                Vector128<int> result = Sse41.MultiplyLow(Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(backColor.Value).AsByte()), Vector128.Create(inverseAlpha));

                // result = (bgraI32 + result) >> 8 | a:0xFF;
                result = Sse2.ShiftRightLogical(Sse2.Add(bgraI32, result), 8).WithElement(3, Byte.MaxValue);

                return new Color32(Ssse3.Shuffle(result.AsByte(), PackLowBytesMask).AsUInt32().ToScalar());
            }
#endif

            return new Color32(Byte.MaxValue,
                (byte)((c.R * c.A + backColor.R * inverseAlpha) >> 8),
                (byte)((c.G * c.A + backColor.G * inverseAlpha) >> 8),
                (byte)((c.B * c.A + backColor.B * inverseAlpha) >> 8));
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Color32 BlendWithBackgroundSrgb_3_IntrinsicsDivSse41(this Color32 c, Color32 backColor)
        {
            Debug.Assert(c.A != Byte.MaxValue, "Partially transparent fore color is expected. Call Blend for better performance.");
            Debug.Assert(backColor.A == Byte.MaxValue, "Totally opaque back color is expected.");

            // The blending is applied only to the color and not the resulting alpha, which will always be opaque
            if (c.A == 0)
                return backColor;
            int inverseAlpha = Byte.MaxValue - c.A;

#if NETCOREAPP3_0_OR_GREATER
            if (Sse41.IsSupported)
            {
                Vector128<float> bgrF = Sse2.ConvertToVector128Single(Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(c.Value).AsByte()));

                // bgrF *= c.A
                bgrF = Sse.Multiply(bgrF, Sse2.ConvertToVector128Single(Sse41.ConvertToVector128Int32(Vector128.Create(c.A))));

                Vector128<float> resultF = Sse2.ConvertToVector128Single(Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(backColor.Value).AsByte()));

                // resultF *= inverseAlpha
                resultF = Sse.Multiply(resultF, Sse2.ConvertToVector128Single(Vector128.Create(inverseAlpha)));

                // resultF = (bgrF + resultF) / 255f
                resultF = Sse.Divide(Sse.Add(bgrF, resultF), Vector128.Create(255f));

                Vector128<int> bgraI32 = Sse2.ConvertToVector128Int32(resultF);

                return new Color32(Ssse3.Shuffle(bgraI32.WithElement(3, Byte.MaxValue).AsByte(), PackLowBytesMask).AsUInt32().ToScalar());
            }
#endif

            return new Color32(Byte.MaxValue,
                (byte)((c.R * c.A + backColor.R * inverseAlpha) / 255),
                (byte)((c.G * c.A + backColor.G * inverseAlpha) / 255),
                (byte)((c.B * c.A + backColor.B * inverseAlpha) / 255));
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Color32 BlendWithBackgroundSrgb_4_IntrinsicsDivSsse3(this Color32 c, Color32 backColor)
        {
            Debug.Assert(c.A != Byte.MaxValue, "Partially transparent fore color is expected. Call Blend for better performance.");
            Debug.Assert(backColor.A == Byte.MaxValue, "Totally opaque back color is expected.");

            // The blending is applied only to the color and not the resulting alpha, which will always be opaque
            if (c.A == 0)
                return backColor;
            int inverseAlpha = Byte.MaxValue - c.A;

#if NETCOREAPP3_0_OR_GREATER
            if (Ssse3.IsSupported)
            {
                Vector128<float> bgrF = Sse2.ConvertToVector128Single(Vector128.Create(c.B, c.G, c.R, default));

                // bgrF *= c.A
                int a = c.A;
                bgrF = Sse.Multiply(bgrF, Sse2.ConvertToVector128Single(Vector128.Create(a)));

                // Converting the [A]RGB values to float (order is BGRA because we reinterpret the original value as bytes if supported)
                Vector128<float> resultF = Sse2.ConvertToVector128Single(Vector128.Create(backColor.B, backColor.G, backColor.R, default));

                // resultF *= inverseAlpha
                resultF = Sse.Multiply(resultF, Sse2.ConvertToVector128Single(Vector128.Create(inverseAlpha)));

                // resultF = (bgrF + resultF) / 255f
                resultF = Sse.Divide(Sse.Add(bgrF, resultF), Vector128.Create(255f));


                // Sse2.ConvertToVector128Int32 performs actual rounding instead of the truncating conversion of the
                // non-accelerated version so the results can be different by 1 shade, but this provides the more correct result.
                // Unfortunately there is no direct vectorized conversion to byte so we need to pack the result if possible.
                Vector128<int> bgraI32 = Sse2.ConvertToVector128Int32(resultF);

                // Compressing 32-bit values to 8 bit ones and initializing value from the first 32 bit
                return new Color32(Ssse3.Shuffle(bgraI32.WithElement(3, Byte.MaxValue).AsByte(), PackLowBytesMask).AsUInt32().ToScalar());
            }
#endif

            return new Color32(Byte.MaxValue,
                (byte)((c.R * c.A + backColor.R * inverseAlpha) / 255),
                (byte)((c.G * c.A + backColor.G * inverseAlpha) / 255),
                (byte)((c.B * c.A + backColor.B * inverseAlpha) / 255));
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Color32 BlendWithBackgroundSrgb_5_IntrinsicsDivSse2(this Color32 c, Color32 backColor)
        {
            Debug.Assert(c.A != Byte.MaxValue, "Partially transparent fore color is expected. Call Blend for better performance.");
            Debug.Assert(backColor.A == Byte.MaxValue, "Totally opaque back color is expected.");

            // The blending is applied only to the color and not the resulting alpha, which will always be opaque
            if (c.A == 0)
                return backColor;
            int inverseAlpha = Byte.MaxValue - c.A;

#if NETCOREAPP3_0_OR_GREATER
            if (Sse2.IsSupported)
            {
                Vector128<float> bgrF = Sse2.ConvertToVector128Single(Vector128.Create(c.B, c.G, c.R, default));

                // bgrF *= c.A
                int a = c.A;
                bgrF = Sse.Multiply(bgrF, Sse2.ConvertToVector128Single(Vector128.Create(a)));

                // Converting the [A]RGB values to float (order is BGRA because we reinterpret the original value as bytes if supported)
                Vector128<float> resultF = Sse2.ConvertToVector128Single(Vector128.Create(backColor.B, backColor.G, backColor.R, default));

                // resultF *= inverseAlpha
                resultF = Sse.Multiply(resultF, Sse2.ConvertToVector128Single(Vector128.Create(inverseAlpha)));

                // resultF = (bgrF + resultF) / 255f
                resultF = Sse.Divide(Sse.Add(bgrF, resultF), Vector128.Create(255f));

                // Sse2.ConvertToVector128Int32 performs actual rounding instead of the truncating conversion of the
                // non-accelerated version so the results can be different by 1 shade, but this provides the more correct result.
                // Unfortunately there is no direct vectorized conversion to byte so we need to pack the result if possible.
                Vector128<int> bgraI32 = Sse2.ConvertToVector128Int32(resultF);

                return new Color32(Byte.MaxValue,
                    (byte)bgraI32.GetElement(2),
                    (byte)bgraI32.GetElement(1),
                    (byte)bgraI32.GetElement(0));
            }
#endif

            return new Color32(Byte.MaxValue,
                (byte)((c.R * c.A + backColor.R * inverseAlpha) / 255),
                (byte)((c.G * c.A + backColor.G * inverseAlpha) / 255),
                (byte)((c.B * c.A + backColor.B * inverseAlpha) / 255));
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Color32 BlendWithSrgb_0_VanillaFloat(this Color32 src, Color32 dst)
        {
            Debug.Assert(src.A != 0 && src.A != 255 && dst.A != 0 && dst.A != 255, "Partially transparent colors are expected");

            float alphaSrc = src.A / 255f;
            float alphaDst = dst.A / 255f;
            float inverseAlphaSrc = 1f - alphaSrc;
            float alphaOut = alphaSrc + alphaDst * inverseAlphaSrc;
            float alphaOutRecip = 1f / alphaOut;

            return new Color32((byte)(alphaOut * Byte.MaxValue),
                (byte)((src.R * alphaSrc + dst.R * alphaDst * inverseAlphaSrc) * alphaOutRecip),
                (byte)((src.G * alphaSrc + dst.G * alphaDst * inverseAlphaSrc) * alphaOutRecip),
                (byte)((src.B * alphaSrc + dst.B * alphaDst * inverseAlphaSrc) * alphaOutRecip));
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Color32 BlendWithSrgb_1_VanillaInt(this Color32 src, Color32 dst)
        {
            Debug.Assert(src.A != 0 && src.A != 255 && dst.A != 0 && dst.A != 255, "Partially transparent colors are expected");

            int inverseAlphaSrc = 255 - src.A;
            int alphaOut = src.A + ((dst.A * inverseAlphaSrc) >> 8);

            return new Color32((byte)alphaOut,
                (byte)((src.R * src.A + ((dst.R * dst.A * inverseAlphaSrc) >> 8)) / alphaOut),
                (byte)((src.G * src.A + ((dst.G * dst.A * inverseAlphaSrc) >> 8)) / alphaOut),
                (byte)((src.B * src.A + ((dst.B * dst.A * inverseAlphaSrc) >> 8)) / alphaOut));
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Color32 BlendWithSrgb_2_IntrinsicFloatSse41(this Color32 src, Color32 dst)
        {
            Debug.Assert(src.A != 0 && src.A != 255 && dst.A != 0 && dst.A != 255, "Partially transparent colors are expected");
            float inverseAlphaSrc;
            float alphaOut;

#if NETCOREAPP3_0_OR_GREATER
            if (Sse41.IsSupported)
            {
                // srcAF = (float)src.A / 255f
                // dstAF = (float)dst.A / 255f
                Vector128<float> srcAF = Sse.Multiply(Sse2.ConvertToVector128Single(Sse41.ConvertToVector128Int32(Vector128.Create(src.A))), Vector128.Create(1f / 255f));
                Vector128<float> dstAF = Sse.Multiply(Sse2.ConvertToVector128Single(Sse41.ConvertToVector128Int32(Vector128.Create(dst.A))), Vector128.Create(1f / 255f));
                inverseAlphaSrc = 1f - srcAF.ToScalar();
                alphaOut = srcAF.ToScalar() + dstAF.ToScalar() * inverseAlphaSrc;

                // srcBgrxF = (float)src.RGB * srcAF
                Vector128<float> srcBgrxF = Sse.Multiply(Sse2.ConvertToVector128Single(Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(src.Value).AsByte())), srcAF);

                // dstBgrxF = (float)dst.RGB * dstAF * (1f - srcAF)
                Vector128<float> dstBgrxF = Sse.Multiply(Sse.Multiply(
                    Sse2.ConvertToVector128Single(Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(dst.Value).AsByte())), dstAF), Vector128.Create(inverseAlphaSrc));

                // bgraI32 = {rgb: (srcBgrxF + dstBgrxF) / alphaOut, a: (alphaOut * 255)} as int
                Vector128<int> bgraI32 = Sse2.ConvertToVector128Int32(
                    Sse.Divide(Sse.Add(srcBgrxF, dstBgrxF), Vector128.Create(alphaOut)).WithElement(3, alphaOut * Byte.MaxValue));

                return new Color32(Ssse3.Shuffle(bgraI32.AsByte(), PackLowBytesMask).AsUInt32().ToScalar());
            }
#endif

            float alphaSrc = src.A / 255f;
            float alphaDst = dst.A / 255f;
            inverseAlphaSrc = 1f - alphaSrc;
            alphaOut = alphaSrc + alphaDst * inverseAlphaSrc;
            float alphaOutRecip = 1f / alphaOut;

            return new Color32((byte)(alphaOut * Byte.MaxValue),
                (byte)((src.R * alphaSrc + dst.R * alphaDst * inverseAlphaSrc) * alphaOutRecip),
                (byte)((src.G * alphaSrc + dst.G * alphaDst * inverseAlphaSrc) * alphaOutRecip),
                (byte)((src.B * alphaSrc + dst.B * alphaDst * inverseAlphaSrc) * alphaOutRecip));
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Color32 BlendWithSrgb_3_IntrinsicFloatSsse3(this Color32 src, Color32 dst)
        {
            Debug.Assert(src.A != 0 && src.A != 255 && dst.A != 0 && dst.A != 255, "Partially transparent colors are expected");
            float inverseAlphaSrc;
            float alphaOut;

#if NETCOREAPP3_0_OR_GREATER
            if (Ssse3.IsSupported)
            {
                // srcAF = (float)src.A / 255f
                // dstAF = (float)dst.A / 255f
#if NET8_0_OR_GREATER
                Vector128<float> srcAF = Sse.Multiply(Sse2.ConvertToVector128Single(Vector128.Create((int)src.A)), Vector128.Create(1f / 255f));
                Vector128<float> dstAF = Sse.Multiply(Sse2.ConvertToVector128Single(Vector128.Create((int)dst.A)), Vector128.Create(1f / 255f));
#else
                int srcA = src.A;
                int dstA = dst.A;
                Vector128<float> srcAF = Sse.Multiply(Sse2.ConvertToVector128Single(Vector128.Create(srcA)), Vector128.Create(1f / 255f));
                Vector128<float> dstAF = Sse.Multiply(Sse2.ConvertToVector128Single(Vector128.Create(dstA)), Vector128.Create(1f / 255f));
#endif
                inverseAlphaSrc = 1f - srcAF.ToScalar();
                alphaOut = srcAF.ToScalar() + dstAF.ToScalar() * inverseAlphaSrc;

                // srcBgrxF = (float)src.RGB * srcAF
                Vector128<float> srcBgrxF = Sse.Multiply(Sse2.ConvertToVector128Single(Vector128.Create(src.B, src.G, src.R, default)), srcAF);

                // dstBgrxF = (float)dst.RGB * dstAF * (1f - srcAF)
                Vector128<float> dstBgrxF = Sse.Multiply(Sse.Multiply(
                    Sse2.ConvertToVector128Single(Vector128.Create(dst.B, dst.G, dst.R, default)), dstAF), Vector128.Create(inverseAlphaSrc));

                // bgraI32 = {rgb: (srcBgrxF + dstBgrxF) / alphaOut, a: (alphaOut * 255)} as int
                Vector128<int> bgraI32 = Sse2.ConvertToVector128Int32(
                    Sse.Divide(Sse.Add(srcBgrxF, dstBgrxF), Vector128.Create(alphaOut)).WithElement(3, alphaOut * Byte.MaxValue));

                return new Color32(Ssse3.Shuffle(bgraI32.AsByte(), PackLowBytesMask).AsUInt32().ToScalar());
            }
#endif

            float alphaSrc = src.A / 255f;
            float alphaDst = dst.A / 255f;
            inverseAlphaSrc = 1f - alphaSrc;
            alphaOut = alphaSrc + alphaDst * inverseAlphaSrc;
            float alphaOutRecip = 1f / alphaOut;
            return new Color32((byte)(alphaOut * Byte.MaxValue),
                (byte)((src.R * alphaSrc + dst.R * alphaDst * inverseAlphaSrc) * alphaOutRecip),
                (byte)((src.G * alphaSrc + dst.G * alphaDst * inverseAlphaSrc) * alphaOutRecip),
                (byte)((src.B * alphaSrc + dst.B * alphaDst * inverseAlphaSrc) * alphaOutRecip));
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Color32 BlendWithSrgb_4_IntrinsicFloatSse2(this Color32 src, Color32 dst)
        {
            Debug.Assert(src.A != 0 && src.A != 255 && dst.A != 0 && dst.A != 255, "Partially transparent colors are expected");
            float inverseAlphaSrc;
            float alphaOut;

#if NETCOREAPP3_0_OR_GREATER
            if (Sse2.IsSupported)
            {
                // srcAF = (float)src.A / 255f
                // dstAF = (float)dst.A / 255f
#if NET8_0_OR_GREATER
                Vector128<float> srcAF = Sse.Multiply(Sse2.ConvertToVector128Single(Vector128.Create((int)src.A)), Vector128.Create(1f / 255f));
                Vector128<float> dstAF = Sse.Multiply(Sse2.ConvertToVector128Single(Vector128.Create((int)dst.A)), Vector128.Create(1f / 255f));
#else
                int srcA = src.A;
                int dstA = dst.A;
                Vector128<float> srcAF = Sse.Multiply(Sse2.ConvertToVector128Single(Vector128.Create(srcA)), Vector128.Create(1f / 255f));
                Vector128<float> dstAF = Sse.Multiply(Sse2.ConvertToVector128Single(Vector128.Create(dstA)), Vector128.Create(1f / 255f));
#endif
                inverseAlphaSrc = 1f - srcAF.ToScalar();
                alphaOut = srcAF.ToScalar() + dstAF.ToScalar() * inverseAlphaSrc;

                // srcBgrxF = (float)src.RGB * srcAF
                Vector128<float> srcBgrxF = Sse.Multiply(Sse2.ConvertToVector128Single(Vector128.Create(src.B, src.G, src.R, default)), srcAF);

                // dstBgrxF = (float)dst.RGB * dstAF * (1f - srcAF)
                Vector128<float> dstBgrxF = Sse.Multiply(Sse.Multiply(
                    Sse2.ConvertToVector128Single(Vector128.Create(dst.B, dst.G, dst.R, default)), dstAF), Vector128.Create(inverseAlphaSrc));

                // bgraI32 = {rgb: (srcBgrxF + dstBgrxF) / alphaOut, a: (alphaOut * 255)} as int
                Vector128<int> bgraI32 = Sse2.ConvertToVector128Int32(
                    Sse.Divide(Sse.Add(srcBgrxF, dstBgrxF), Vector128.Create(alphaOut)).WithElement(3, alphaOut * Byte.MaxValue));

                return new Color32((byte)bgraI32.GetElement(3),
                    (byte)bgraI32.GetElement(2),
                    (byte)bgraI32.GetElement(1),
                    (byte)bgraI32.GetElement(0));
            }
#endif

            float alphaSrc = src.A / 255f;
            float alphaDst = dst.A / 255f;
            inverseAlphaSrc = 1f - alphaSrc;
            alphaOut = alphaSrc + alphaDst * inverseAlphaSrc;
            float alphaOutRecip = 1f / alphaOut;
            return new Color32((byte)(alphaOut * Byte.MaxValue),
                (byte)((src.R * alphaSrc + dst.R * alphaDst * inverseAlphaSrc) * alphaOutRecip),
                (byte)((src.G * alphaSrc + dst.G * alphaDst * inverseAlphaSrc) * alphaOutRecip),
                (byte)((src.B * alphaSrc + dst.B * alphaDst * inverseAlphaSrc) * alphaOutRecip));
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static PColor32 BlendWithSrgb_0_VanillaShift(this PColor32 src, PColor32 dst)
        {
            Debug.Assert(src.A != 0 && src.A != 255 && dst.A != 0, "Partially transparent colors are expected");
            int inverseAlphaSrc = Byte.MaxValue - src.A;
            return new PColor32(dst.A == Byte.MaxValue ? Byte.MaxValue : (byte)(src.A + ((dst.A * inverseAlphaSrc) >> 8)),
                (byte)(src.R + ((dst.R * inverseAlphaSrc) >> 8)),
                (byte)(src.G + ((dst.G * inverseAlphaSrc) >> 8)),
                (byte)(src.B + ((dst.B * inverseAlphaSrc) >> 8)));
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static PColor32 BlendWithSrgb_1_VanillaDiv(this PColor32 src, PColor32 dst)
        {
            Debug.Assert(src.A != 0 && src.A != 255 && dst.A != 0, "Partially transparent colors are expected");
            int inverseAlphaSrc = Byte.MaxValue - src.A;
            return new PColor32(dst.A == Byte.MaxValue ? Byte.MaxValue : (byte)(src.A + dst.A * inverseAlphaSrc / Byte.MaxValue),
                (byte)(src.R + dst.R * inverseAlphaSrc / Byte.MaxValue),
                (byte)(src.G + dst.G * inverseAlphaSrc / Byte.MaxValue),
                (byte)(src.B + dst.B * inverseAlphaSrc / Byte.MaxValue));
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static PColor32 BlendWithSrgb_2_IntrinsicsShift(this PColor32 src, PColor32 dst)
        {
            Debug.Assert(src.A != 0 && src.A != 255 && dst.A != 0, "Partially transparent colors are expected");
            int inverseAlphaSrc = Byte.MaxValue - src.A;

#if NETCOREAPP3_0_OR_GREATER
            if (Sse41.IsSupported)
            {
                // bgraI32 = (int)dst.ARGB * inverseAlphaSrc
                Vector128<int> bgraI32 = Sse41.MultiplyLow(Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(dst.Value).AsByte()), Vector128.Create(inverseAlphaSrc));

                // bgraI32 >>= 8
                bgraI32 = Sse2.ShiftRightLogical(bgraI32, 8);

                // bgraI32 += (int)src.ARGB
                bgraI32 = Sse2.Add(bgraI32, Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(src.Value).AsByte()));

                if (dst.A == Byte.MaxValue)
                    bgraI32 = bgraI32.AsByte().WithElement(12, Byte.MaxValue).AsInt32();

                return new PColor32(Ssse3.Shuffle(bgraI32.AsByte(), PackLowBytesMask).AsUInt32().ToScalar());
            }
#endif

            return new PColor32(dst.A == Byte.MaxValue ? Byte.MaxValue : (byte)(src.A + ((dst.A * inverseAlphaSrc) >> 8)),
                (byte)(src.R + ((dst.R * inverseAlphaSrc) >> 8)),
                (byte)(src.G + ((dst.G * inverseAlphaSrc) >> 8)),
                (byte)(src.B + ((dst.B * inverseAlphaSrc) >> 8)));
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Color64 BlendWithBackgroundSrgb_0_VanillaShift(this Color64 c, Color64 backColor)
        {
            Debug.Assert(c.A != UInt16.MaxValue, "Partially transparent fore color is expected. Call Blend for better performance.");
            Debug.Assert(backColor.A == UInt16.MaxValue, "Totally opaque back color is expected.");

            // The blending is applied only to the color and not the resulting alpha, which will always be opaque
            if (c.A == 0)
                return backColor;
            uint inverseAlpha = (uint)UInt16.MaxValue - c.A;
            return new Color64(UInt16.MaxValue,
                (ushort)(((uint)c.R * c.A + backColor.R * inverseAlpha) >> 16),
                (ushort)(((uint)c.G * c.A + backColor.G * inverseAlpha) >> 16),
                (ushort)(((uint)c.B * c.A + backColor.B * inverseAlpha) >> 16));
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Color64 BlendWithBackgroundSrgb_1_VanillaDiv(this Color64 c, Color64 backColor)
        {
            Debug.Assert(c.A != UInt16.MaxValue, "Partially transparent fore color is expected. Call Blend for better performance.");
            Debug.Assert(backColor.A == UInt16.MaxValue, "Totally opaque back color is expected.");

            // The blending is applied only to the color and not the resulting alpha, which will always be opaque
            if (c.A == 0)
                return backColor;
            uint inverseAlpha = (uint)UInt16.MaxValue - c.A;
            return new Color64(UInt16.MaxValue,
                (ushort)(((uint)c.R * c.A + backColor.R * inverseAlpha) / UInt16.MaxValue),
                (ushort)(((uint)c.G * c.A + backColor.G * inverseAlpha) / UInt16.MaxValue),
                (ushort)(((uint)c.B * c.A + backColor.B * inverseAlpha) / UInt16.MaxValue));
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Color64 BlendWithBackgroundSrgb_2_IntrinsicsShift(this Color64 c, Color64 backColor)
        {
            Debug.Assert(c.A != UInt16.MaxValue, "Partially transparent fore color is expected. Call Blend for better performance.");
            Debug.Assert(backColor.A == UInt16.MaxValue, "Totally opaque back color is expected.");

            // The blending is applied only to the color and not the resulting alpha, which will always be opaque
            if (c.A == 0)
                return backColor;
            uint inverseAlpha = (uint)UInt16.MaxValue - c.A;

#if NETCOREAPP3_0_OR_GREATER
            if (Sse41.IsSupported)
            {
                // bgraU32 = (uint)c.RGB * (uint)c.A
                Vector128<uint> bgraU32 = Sse41.MultiplyLow(Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(c.Value).AsUInt16()).AsUInt32(),
                    Sse41.ConvertToVector128Int32(Vector128.Create(c.A)).AsUInt32());

                // resultU32 = (uint)backColor.RGB * inverseAlpha
                Vector128<uint> resultU32 = Sse41.MultiplyLow(Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(backColor.Value).AsUInt16()).AsUInt32(),
                    Vector128.Create(inverseAlpha));

                // resultU32 = (bgraU32 + result) >> 16 | a:0xFFFF;
                resultU32 = Sse2.ShiftRightLogical(Sse2.Add(bgraU32, resultU32), 16).WithElement(3, UInt16.MaxValue);

                return new Color64(Sse41.PackUnsignedSaturate(resultU32.AsInt32(), resultU32.AsInt32()).AsUInt64().ToScalar());
            }
#endif

            return new Color64(UInt16.MaxValue,
                (ushort)(((uint)c.R * c.A + backColor.R * inverseAlpha) >> 16),
                (ushort)(((uint)c.G * c.A + backColor.G * inverseAlpha) >> 16),
                (ushort)(((uint)c.B * c.A + backColor.B * inverseAlpha) >> 16));
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Color64 BlendWithSrgb_0_VanillaFloat(this Color64 src, Color64 dst)
        {
            Debug.Assert(src.A != 0 && src.A != 65535 && dst.A != 0 && dst.A != 65535, "Partially transparent colors are expected");

            float alphaSrc = src.A / 65535f;
            float alphaDst = dst.A / 65535f;
            float inverseAlphaSrc = 1f - alphaSrc;
            float alphaOut = alphaSrc + alphaDst * inverseAlphaSrc;
            float alphaOutRecip = 1f / alphaOut;

            return new Color64((ushort)(alphaOut * UInt16.MaxValue),
                (ushort)((src.R * alphaSrc + dst.R * alphaDst * inverseAlphaSrc) * alphaOutRecip),
                (ushort)((src.G * alphaSrc + dst.G * alphaDst * inverseAlphaSrc) * alphaOutRecip),
                (ushort)((src.B * alphaSrc + dst.B * alphaDst * inverseAlphaSrc) * alphaOutRecip));
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Color64 BlendWithSrgb_1_VanillaInt(this Color64 src, Color64 dst)
        {
            Debug.Assert(src.A != 0 && src.A != 65535 && dst.A != 0 && dst.A != 65535, "Partially transparent colors are expected");

            uint inverseAlphaSrc = 65535u - src.A;
            uint alphaOut = src.A + ((dst.A * inverseAlphaSrc) >> 16);

            return new Color64((ushort)alphaOut,
                (ushort)(((ulong)src.R * src.A + (((ulong)dst.R * dst.A * inverseAlphaSrc) >> 16)) / alphaOut),
                (ushort)(((ulong)src.G * src.A + (((ulong)dst.G * dst.A * inverseAlphaSrc) >> 16)) / alphaOut),
                (ushort)(((ulong)src.B * src.A + (((ulong)dst.B * dst.A * inverseAlphaSrc) >> 16)) / alphaOut));
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Color64 BlendWithSrgb_2_IntrinsicFloatSse41(this Color64 src, Color64 dst)
        {
            Debug.Assert(src.A != 0 && src.A != 65535 && dst.A != 0 && dst.A != 65535, "Partially transparent colors are expected");
            float inverseAlphaSrc;
            float alphaOut;

#if NETCOREAPP3_0_OR_GREATER
            if (Sse41.IsSupported)
            {
                // srcAF = (float)src.A / 65535f
                // dstAF = (float)dst.A / 65535f
                Vector128<float> srcAF = Sse.Multiply(Sse2.ConvertToVector128Single(Sse41.ConvertToVector128Int32(Vector128.Create(src.A))), Vector128.Create(1f / 65535f));
                Vector128<float> dstAF = Sse.Multiply(Sse2.ConvertToVector128Single(Sse41.ConvertToVector128Int32(Vector128.Create(dst.A))), Vector128.Create(1f / 65535f));
                inverseAlphaSrc = 1f - srcAF.ToScalar();
                alphaOut = srcAF.ToScalar() + dstAF.ToScalar() * inverseAlphaSrc;

                // srcBgrxF = (float)src.RGB * srcAF
                Vector128<float> srcBgrxF = Sse.Multiply(Sse2.ConvertToVector128Single(Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(src.Value).AsUInt16())), srcAF);

                // dstBgrxF = (float)dst.RGB * dstAF * (1f - srcAF)
                Vector128<float> dstBgrxF = Sse.Multiply(Sse.Multiply(
                    Sse2.ConvertToVector128Single(Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(dst.Value).AsUInt16())), dstAF), Vector128.Create(inverseAlphaSrc));

                // bgraI32 = {rgb: (srcBgrxF + dstBgrxF) / alphaOut, a: (alphaOut * 65535)} as int
                Vector128<int> bgraI32 = Sse2.ConvertToVector128Int32(
                    Sse.Divide(Sse.Add(srcBgrxF, dstBgrxF), Vector128.Create(alphaOut)).WithElement(3, alphaOut * UInt16.MaxValue));

                return new Color64(Sse41.PackUnsignedSaturate(bgraI32, bgraI32).AsUInt64().ToScalar());
            }
#endif

            float alphaSrc = src.A / 65535f;
            float alphaDst = dst.A / 65535f;
            inverseAlphaSrc = 1f - alphaSrc;
            alphaOut = alphaSrc + alphaDst * inverseAlphaSrc;
            float alphaOutRecip = 1f / alphaOut;
            return new Color64((ushort)(alphaOut * UInt16.MaxValue),
                (ushort)((src.R * alphaSrc + dst.R * alphaDst * inverseAlphaSrc) * alphaOutRecip),
                (ushort)((src.G * alphaSrc + dst.G * alphaDst * inverseAlphaSrc) * alphaOutRecip),
                (ushort)((src.B * alphaSrc + dst.B * alphaDst * inverseAlphaSrc) * alphaOutRecip));
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static PColor64 BlendWithSrgb_0_VanillaShift(this PColor64 src, PColor64 dst)
        {
            Debug.Assert(src.A != 0 && src.A != 65535 && dst.A != 0, "Partially transparent colors are expected");
            uint inverseAlphaSrc = (uint)UInt16.MaxValue - src.A;
            
            return new PColor64(dst.A == UInt16.MaxValue ? UInt16.MaxValue : (ushort)(src.A + ((dst.A * inverseAlphaSrc) >> 16)),
                (ushort)(src.R + ((dst.R * inverseAlphaSrc) >> 16)),
                (ushort)(src.G + ((dst.G * inverseAlphaSrc) >> 16)),
                (ushort)(src.B + ((dst.B * inverseAlphaSrc) >> 16)));
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static PColor64 BlendWithSrgb_1_VanillaDiv(this PColor64 src, PColor64 dst)
        {
            Debug.Assert(src.A != 0 && src.A != 65535 && dst.A != 0, "Partially transparent colors are expected");
            int inverseAlphaSrc = UInt16.MaxValue - src.A;
            
            return new PColor64(dst.A == UInt16.MaxValue ? UInt16.MaxValue : (ushort)(src.A + dst.A * inverseAlphaSrc / UInt16.MaxValue),
                (ushort)(src.R + dst.R * inverseAlphaSrc / UInt16.MaxValue),
                (ushort)(src.G + dst.G * inverseAlphaSrc / UInt16.MaxValue),
                (ushort)(src.B + dst.B * inverseAlphaSrc / UInt16.MaxValue));
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static PColor64 BlendWithSrgb_2_IntrinsicsShift(this PColor64 src, PColor64 dst)
        {
            Debug.Assert(src.A != 0 && src.A != 65535 && dst.A != 0, "Partially transparent colors are expected");
            int inverseAlphaSrc = UInt16.MaxValue - src.A;

#if NETCOREAPP3_0_OR_GREATER
            if (Sse41.IsSupported)
            {
                // bgraI32 = (int)dst.ARGB * inverseAlphaSrc
                Vector128<int> bgraI32 = Sse41.MultiplyLow(Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(dst.Value).AsUInt16()), Vector128.Create(inverseAlphaSrc));

                // bgraI32 >>= 16
                bgraI32 = Sse2.ShiftRightLogical(bgraI32, 16);

                // bgraI32 += (int)src.ARGB
                bgraI32 = Sse2.Add(bgraI32, Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(src.Value).AsUInt16()));

                if (dst.A == UInt16.MaxValue)
                    bgraI32 = bgraI32.AsUInt16().WithElement(6, UInt16.MaxValue).AsInt32();

                return new PColor64(Sse41.PackUnsignedSaturate(bgraI32, bgraI32).AsUInt64().ToScalar());
            }
#endif

            return new PColor64(dst.A == UInt16.MaxValue ? UInt16.MaxValue : (ushort)(src.A + ((dst.A * inverseAlphaSrc) >> 16)),
                (ushort)(src.R + ((dst.R * inverseAlphaSrc) >> 16)),
                (ushort)(src.G + ((dst.G * inverseAlphaSrc) >> 16)),
                (ushort)(src.B + ((dst.B * inverseAlphaSrc) >> 16)));
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static ColorF BlendWithBackgroundLinear_0_Vanilla(this ColorF c, ColorF backColor)
        {
            if (c.A <= 0)
                return backColor;
            float inverseAlpha = 1f - c.A;
            return new ColorF(1f,
                c.R * c.A + backColor.R * inverseAlpha,
                c.G * c.A + backColor.G * inverseAlpha,
                c.B * c.A + backColor.B * inverseAlpha);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static ColorF BlendWithBackgroundLinear_1_Vector(this ColorF c, ColorF backColor)
        {
            if (c.A <= 0)
                return backColor;
#if NET45_OR_GREATER || NETCOREAPP
            return new ColorF(new Vector4(c.Rgb * c.A + backColor.Rgb * (1f - c.A), 1f));
#else
            throw new PlatformNotSupportedException();
#endif
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static ColorF BlendWithBackgroundLinear_2_Intrinsic(this ColorF c, ColorF backColor)
        {
            if (c.A <= 0)
                return backColor;
#if NETCOREAPP3_0_OR_GREATER
            if (Sse.IsSupported)
            {
                // rgbaResultF = c.RGBA * c.A
                Vector128<float> rgbaResultF = Sse.Multiply(c.RgbaV128, Vector128.Create(c.A));

                // rgbaResultF += backColor.RGBA * (1f - c.A)
                rgbaResultF = Sse.Add(rgbaResultF, Sse.Multiply(backColor.RgbaV128, Vector128.Create(c.A * (1f - c.A))));

                return new ColorF(rgbaResultF.WithElement(3, 1f));
            }
#endif

#if NET45_OR_GREATER || NETCOREAPP
            return new ColorF(new Vector4(c.Rgb * c.A + backColor.Rgb * (1f - c.A), 1f));
#else
            throw new PlatformNotSupportedException();
#endif
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static ColorF BlendWithLinear_0_Vanilla(this ColorF src, ColorF dst)
        {
            float inverseAlphaSrc = 1f - src.A;
            float alphaOut = src.A + dst.A * inverseAlphaSrc;
            float alphaDst = dst.A * inverseAlphaSrc;
            float alphaOutRecip = 1f / alphaOut;

            return new ColorF(alphaOut,
                (src.R * src.A + dst.R * alphaDst) * alphaOutRecip,
                (src.G * src.A + dst.G * alphaDst) * alphaOutRecip,
                (src.B * src.A + dst.B * alphaDst) * alphaOutRecip);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static ColorF BlendWithLinear_1_Vector(this ColorF src, ColorF dst)
        {
            float inverseAlphaSrc = 1f - src.A;
            float alphaOut = src.A + dst.A * inverseAlphaSrc;

#if NET45_OR_GREATER || NETCOREAPP
            return new ColorF(new Vector4((src.Rgb * src.A + dst.Rgb * (dst.A * inverseAlphaSrc)) / alphaOut, alphaOut));
#else
            throw new PlatformNotSupportedException();
#endif
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static ColorF BlendWithLinear_2_Intrinsic(this ColorF src, ColorF dst)
        {
            float inverseAlphaSrc = 1f - src.A;
            float alphaOut = src.A + dst.A * inverseAlphaSrc;
#if NETCOREAPP3_0_OR_GREATER
            if (Sse.IsSupported)
            {
                // rgbaResultF = src.RGBA * src.A
                Vector128<float> rgbaResultF = Sse.Multiply(src.RgbaV128, Vector128.Create(src.A));

                // rgbaResultF += dst.RGBA * dst.A * inverseAlphaSrc
                rgbaResultF = Sse.Add(rgbaResultF, Sse.Multiply(dst.RgbaV128, Vector128.Create(dst.A * inverseAlphaSrc)));

                // (rgbaResultF /=  alphaOut) with A:alphaOut
                return new ColorF(Sse.Divide(rgbaResultF, Vector128.Create(alphaOut)).WithElement(3, alphaOut));
            }
#endif

#if NET45_OR_GREATER || NETCOREAPP
            return new ColorF(new Vector4((src.Rgb * src.A + dst.Rgb * (dst.A * inverseAlphaSrc)) / alphaOut, alphaOut));
#else
            throw new PlatformNotSupportedException();
#endif
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static PColorF BlendWithLinear_0_Vanilla(this PColorF src, PColorF dst)
        {
            float inverseAlphaSrc = 1f - src.A;
            return new PColorF(dst.A >= 1f ? 1f : src.A + dst.A * inverseAlphaSrc,
                src.R + dst.R * inverseAlphaSrc,
                src.G + dst.G * inverseAlphaSrc,
                src.B + dst.B * inverseAlphaSrc);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static PColorF BlendWithLinear_1_Vector(this PColorF src, PColorF dst)
        {
#if NET45_OR_GREATER || NETCOREAPP
            return new PColorF(src.Rgba + dst.Rgba * (1f - src.A));
#else
            throw new PlatformNotSupportedException();
#endif
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static PColorF BlendWithLinear_2_IntrinsicSse(this PColorF src, PColorF dst)
        {
#if NETCOREAPP3_0_OR_GREATER
            if (Sse.IsSupported)
            {
                // rgbaResultF = dst.RGBA * (1f - src.A)
                Vector128<float> rgbaResultF = Sse.Multiply(dst.RgbaV128, Vector128.Create(1f - src.A));

                // rgbaResultF += src.RGBA
                rgbaResultF = Sse.Add(rgbaResultF, src.RgbaV128);

                return new PColorF(rgbaResultF);
            }
#endif
#if NET45_OR_GREATER || NETCOREAPP
            return new PColorF(src.Rgba + dst.Rgba * (1f - src.A));
#else
            throw new PlatformNotSupportedException();
#endif
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static PColorF BlendWithLinear_3_IntrinsicFma(this PColorF src, PColorF dst)
        {
#if NETCOREAPP3_0_OR_GREATER
            if (Fma.IsSupported)
            {
                // rgbaResultF = dst.RGBA * (1f - src.A) + src.RGBA
                Vector128<float> rgbaResultF = Fma.MultiplyAdd(dst.RgbaV128, Vector128.Create(1f - src.A), src.RgbaV128);

                return new PColorF(rgbaResultF);
            }
#endif
#if NET45_OR_GREATER || NETCOREAPP
            return new PColorF(src.Rgba + dst.Rgba * (1f - src.A));
#else
            throw new PlatformNotSupportedException();
#endif
        }

#endregion
    }
}