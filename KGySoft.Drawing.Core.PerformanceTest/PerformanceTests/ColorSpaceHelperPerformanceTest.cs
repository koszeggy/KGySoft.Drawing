#if NETCOREAPP3_0_OR_GREATER
#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ColorSpaceHelperPerformanceTest.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2026 - All Rights Reserved
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
#if NET7_0_OR_GREATER
using System.Numerics;
#endif
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
        [TestCase(0.5f, 0f, 1f, 0.001f)] // Different channels, 0 pow
        [TestCase(0.5f, 1.5f, -1f, Single.NaN)] // Out-of-range values
        public void LinearToSrgbTest(float a, float r, float g, float b)
        {
            var linear = Vector128.Create(r, g, b, a);
            var srgb = linear.ToSrgb_0_Vanilla();

            Console.WriteLine($"{"Original color:",-40} {new ColorF(linear)}");
            Console.WriteLine($"{"Expected color:",-40} {new ColorF(srgb)}");

#if NET7_0_OR_GREATER
            DoAssert(_ => linear.ToSrgb_1_AutoVectorization());
#endif
            DoAssert(_ => linear.ToSrgb_2_Intrinsics());

            new PerformanceTest<Vector128<float>>
                {
                    TestName = "Linear to sRGB",
                    TestTime = 2000,
                    //Iterations = 10_000_000,
                    Repeat = 3
                }
                .AddCase(() => linear.ToSrgb_0_Vanilla(), nameof(Extensions.ToSrgb_0_Vanilla))
#if NET7_0_OR_GREATER
                .AddCase(() => linear.ToSrgb_1_AutoVectorization(), nameof(Extensions.ToSrgb_1_AutoVectorization))
#endif
                .AddCase(() => linear.ToSrgb_2_Intrinsics(), nameof(Extensions.ToSrgb_2_Intrinsics))
                .DoTest()
                .DumpResults(Console.Out);

            #region Local Methods

            void DoAssert(Expression<Func<Vector128<float>, Vector128<float>>> e)
            {
                var m = (MethodCallExpression)e.Body;
                if (m.Method.Name == "ToScalar")
                    m = (MethodCallExpression)m.Arguments[0];
                Vector128<float> actual = e.Compile().Invoke(linear);
                Console.WriteLine($"{$"{m.Method.Name}:",-40} {actual}");
                Assert.IsTrue(new ColorF(srgb).TolerantEquals(new ColorF(actual)), $"{m.Method.Name}: {srgb} vs. {actual}");
            }

            #endregion

            // Verdict: It is worth vectorizing even when color components fall into different ranges.
            // Custom intrinsics implementation does not add much value in .NET9+ where Pow can be vectorized.

            // All components > 0.0031308 and < 1: (Pow range)
            // Original color:                          [A=0,50000000; R=0,50000000; G=0,25000000; B=0,12500000]
            // Expected color:                          [A=0,50000000; R=0,73535693; G=0,53709871; B=0,38857284]
            // ToSrgb_1_AutoVectorization:              <0.73535705, 0.5370987, 0.38857278, 0.5>
            // ToSrgb_2_Intrinsics:                     <0.73535705, 0.5370987, 0.38857278, 0.5>
            // ==[Linear to sRGB (.NET Core 10.0.0-rc.2.25502.107) Results]================================================
            // Test Time: 2 000 ms
            // Warming up: Yes
            // Test cases: 3
            // Repeats: 3
            // Calling GC.Collect: Yes
            // Forced CPU Affinity: No
            // Cases are sorted by fulfilled iterations (the most first)
            // --------------------------------------------------
            // 1. ToSrgb_2_Intrinsics: 173 129 274 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 57 709 758,00
            //   #1  57 845 352 iterations in 2 000,00 ms. Adjusted: 57 845 352,00
            //   #2  57 918 299 iterations in 2 000,00 ms. Adjusted: 57 918 299,00	 <---- Best
            //   #3  57 365 623 iterations in 2 000,00 ms. Adjusted: 57 365 623,00	 <---- Worst
            //   Worst-Best difference: 552 676,00 (0,96%)
            // 2. ToSrgb_1_AutoVectorization: 166 133 673 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 55 377 890,05 (-2 331 867,95 / 95,96%)
            //   #1  52 993 640 iterations in 2 000,00 ms. Adjusted: 52 993 640,00	 <---- Worst
            //   #2  56 834 355 iterations in 2 000,00 ms. Adjusted: 56 834 352,16	 <---- Best
            //   #3  56 305 678 iterations in 2 000,00 ms. Adjusted: 56 305 678,00
            //   Worst-Best difference: 3 840 712,16 (7,25%)
            // 3. ToSrgb_0_Vanilla: 132 066 168 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 44 022 056,00 (-13 687 702,00 / 76,28%)
            //   #1  42 716 470 iterations in 2 000,00 ms. Adjusted: 42 716 470,00	 <---- Worst
            //   #2  44 329 640 iterations in 2 000,00 ms. Adjusted: 44 329 640,00
            //   #3  45 020 058 iterations in 2 000,00 ms. Adjusted: 45 020 058,00	 <---- Best
            //   Worst-Best difference: 2 303 588,00 (5,39%)

            // All components <= 0.0031308: (Linear range)
            // Original color:                          [A=0,50000000; R=0,00100000; G=0,00200000; B=0,00300000]
            // Expected color:                          [A=0,50000000; R=0,01292000; G=0,02584000; B=0,03876000]
            // ToSrgb_1_AutoVectorization:              <0.012920001, 0.025840001, 0.03876, 0.5>
            // ToSrgb_2_Intrinsics:                     <0.012920001, 0.025840001, 0.03876, 0.5>
            // ==[Linear to sRGB (.NET Core 10.0.0-rc.2.25502.107) Results]================================================
            // Test Time: 2 000 ms
            // Warming up: Yes
            // Test cases: 3
            // Repeats: 3
            // Calling GC.Collect: Yes
            // Forced CPU Affinity: No
            // Cases are sorted by fulfilled iterations (the most first)
            // --------------------------------------------------
            // 1. ToSrgb_2_Intrinsics: 422 405 202 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 140 801 734,00
            //   #1  140 395 713 iterations in 2 000,00 ms. Adjusted: 140 395 713,00	 <---- Worst
            //   #2  141 137 165 iterations in 2 000,00 ms. Adjusted: 141 137 165,00	 <---- Best
            //   #3  140 872 324 iterations in 2 000,00 ms. Adjusted: 140 872 324,00
            //   Worst-Best difference: 741 452,00 (0,53%)
            // 2. ToSrgb_1_AutoVectorization: 419 960 613 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 139 986 871,00 (-814 863,00 / 99,42%)
            //   #1  140 001 200 iterations in 2 000,00 ms. Adjusted: 140 001 200,00
            //   #2  140 394 391 iterations in 2 000,00 ms. Adjusted: 140 394 391,00	 <---- Best
            //   #3  139 565 022 iterations in 2 000,00 ms. Adjusted: 139 565 022,00	 <---- Worst
            //   Worst-Best difference: 829 369,00 (0,59%)
            // 3. ToSrgb_0_Vanilla: 412 010 074 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 137 336 691,33 (-3 465 042,67 / 97,54%)
            //   #1  136 099 834 iterations in 2 000,00 ms. Adjusted: 136 099 834,00	 <---- Worst
            //   #2  138 056 222 iterations in 2 000,00 ms. Adjusted: 138 056 222,00	 <---- Best
            //   #3  137 854 018 iterations in 2 000,00 ms. Adjusted: 137 854 018,00
            //   Worst-Best difference: 1 956 388,00 (1,44%)

            // Mixed ranges:
            // Original color:                          [A=0,50000000; R=1,00000000; G=0,50000000; B=0,25000000]
            // Expected color:                          [A=0,50000000; R=1,00000000; G=0,73535693; B=0,53709871]
            // ToSrgb_1_AutoVectorization:              <1, 0.73535705, 0.5370987, 0.5>
            // ToSrgb_2_Intrinsics:                     <1, 0.73535705, 0.5370987, 0.5>
            // ==[Linear to sRGB (.NET Core 10.0.0-rc.2.25502.107) Results]================================================
            // Test Time: 2 000 ms
            // Warming up: Yes
            // Test cases: 3
            // Repeats: 3
            // Calling GC.Collect: Yes
            // Forced CPU Affinity: No
            // Cases are sorted by fulfilled iterations (the most first)
            // --------------------------------------------------
            // 1. ToSrgb_2_Intrinsics: 168 355 752 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 56 118 584,00
            //   #1  56 068 982 iterations in 2 000,00 ms. Adjusted: 56 068 982,00
            //   #2  56 042 830 iterations in 2 000,00 ms. Adjusted: 56 042 830,00	 <---- Worst
            //   #3  56 243 940 iterations in 2 000,00 ms. Adjusted: 56 243 940,00	 <---- Best
            //   Worst-Best difference: 201 110,00 (0,36%)
            // 2. ToSrgb_1_AutoVectorization: 164 826 695 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 54 942 231,67 (-1 176 352,33 / 97,90%)
            //   #1  55 106 757 iterations in 2 000,00 ms. Adjusted: 55 106 757,00	 <---- Best
            //   #2  54 674 121 iterations in 2 000,00 ms. Adjusted: 54 674 121,00	 <---- Worst
            //   #3  55 045 817 iterations in 2 000,00 ms. Adjusted: 55 045 817,00
            //   Worst-Best difference: 432 636,00 (0,79%)
            // 3. ToSrgb_0_Vanilla: 158 827 602 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 52 942 534,00 (-3 176 050,00 / 94,34%)
            //   #1  52 949 868 iterations in 2 000,00 ms. Adjusted: 52 949 868,00
            //   #2  52 688 091 iterations in 2 000,00 ms. Adjusted: 52 688 091,00	 <---- Worst
            //   #3  53 189 643 iterations in 2 000,00 ms. Adjusted: 53 189 643,00	 <---- Best
            //   Worst-Best difference: 501 552,00 (0,95%)

            // Out-of-range values:
            // Original color:                          [A=0,50000000; R=1,50000000; G=-1,00000000; B=NaN]
            // Expected color:                          [A=0,50000000; R=1,00000000; G=0,00000000; B=0,00000000]
            // ToSrgb_1_AutoVectorization:              <1, 0, 0, 0.5>
            // ToSrgb_2_Intrinsics:                     <1, 0, 0, 0.5>
            // ==[Linear to sRGB (.NET Core 10.0.0-rc.2.25502.107) Results]================================================
            // Test Time: 2 000 ms
            // Warming up: Yes
            // Test cases: 3
            // Repeats: 3
            // Calling GC.Collect: Yes
            // Forced CPU Affinity: No
            // Cases are sorted by fulfilled iterations (the most first)
            // --------------------------------------------------
            // 1. ToSrgb_0_Vanilla: 423 523 013 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 141 174 337,67
            //   #1  140 791 715 iterations in 2 000,00 ms. Adjusted: 140 791 715,00	 <---- Worst
            //   #2  141 492 266 iterations in 2 000,00 ms. Adjusted: 141 492 266,00	 <---- Best
            //   #3  141 239 032 iterations in 2 000,00 ms. Adjusted: 141 239 032,00
            //   Worst-Best difference: 700 551,00 (0,50%)
            // 2. ToSrgb_2_Intrinsics: 416 380 397 iterations in 6 000,01 ms. Adjusted for 2 000 ms: 138 793 248,73 (-2 381 088,94 / 98,31%)
            //   #1  139 435 679 iterations in 2 000,00 ms. Adjusted: 139 435 679,00	 <---- Best
            //   #2  138 471 448 iterations in 2 000,00 ms. Adjusted: 138 471 448,00	 <---- Worst
            //   #3  138 473 270 iterations in 2 000,01 ms. Adjusted: 138 472 619,18
            //   Worst-Best difference: 964 231,00 (0,70%)
            // 3. ToSrgb_1_AutoVectorization: 415 243 493 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 138 414 497,67 (-2 759 840,00 / 98,05%)
            //   #1  138 093 417 iterations in 2 000,00 ms. Adjusted: 138 093 417,00	 <---- Worst
            //   #2  138 439 271 iterations in 2 000,00 ms. Adjusted: 138 439 271,00
            //   #3  138 710 805 iterations in 2 000,00 ms. Adjusted: 138 710 805,00	 <---- Best
            //   Worst-Best difference: 617 388,00 (0,45%)
        }

        [TestCase(0.1f, 1f / 2.4f)]
        [TestCase(0.5f, 1f / 2.4f)]
        [TestCase(0.9f, 1f / 2.4f)]
        [TestCase(0.1f, 2.4f)]
        [TestCase(0.5f, 2.4f)]
        [TestCase(0.9f, 2.4f)]
        public void PowTest(float x, float p)
        {
            float expected = MathF.Pow(x, p);
            Console.WriteLine($"{$"Expected result of Pow({x}; {p}):",-50} {expected:R}");

            DoAssert(() => x.Pow_0_ByMathExpLog(p));
            DoAssert(() => x.Pow_1_Float(p));
            DoAssert(() => Vector128.Create(x).Pow_2_Vector128Full(Vector128.Create(p)).ToScalar());
            DoAssert(() => Vector128.Create(x).Pow_3_Vector128SamePower(p).ToScalar());
            if (x is >= 0 and < Single.PositiveInfinity && p > 0)
                DoAssert(() => Vector128.Create(x).Pow_3b_Vector128SamePower_Specialized(p).ToScalar());
            DoAssert(() => Vector256.Create(x).Pow_4_Vector256SamePower(p).ToScalar());
#if NET9_0_OR_GREATER
            DoAssert(() => x.Pow_5_ByVector128ExpLog_Scalar(p));
            DoAssert(() => Vector128.Create(x).Pow_6_ByVector128ExpLog(p).ToScalar());
            if (x >= 0 && p > 0)
                DoAssert(() => Vector128.Create(x).Pow_6b_ByVector128ExpLog_Specialized(p).ToScalar());
#endif

            new PerformanceTest<float>
                {
                    TestName = nameof(PowTest),
                    //Iterations = 10_000_000,
                    Repeat = 3
                }
                .AddCase(() => MathF.Pow(x, p), "MathF.Pow")
                //.AddCase(() => x.Pow_0_ByMathExpLog(p), nameof(Extensions.Pow_0_ByMathExpLog))
                //.AddCase(() => x.Pow_1_Float(p), nameof(Extensions.Pow_1_Float))
                .AddCase(() => Vector128.Create(x).Pow_2_Vector128Full(Vector128.Create(p)).ToScalar(), nameof(Extensions.Pow_2_Vector128Full))
                .AddCase(() => Vector128.Create(x).Pow_3_Vector128SamePower(p).ToScalar(), nameof(Extensions.Pow_3_Vector128SamePower))
                //.AddCase(() => Vector256.Create(x).Pow_4_Vector256SamePower(p).ToScalar(), nameof(Extensions.Pow_4_Vector256SamePower))
#if NET9_0_OR_GREATER
                //.AddCase(() => x.Pow_5_ByVector128ExpLog_Scalar(p), nameof(Extensions.Pow_5_ByVector128ExpLog_Scalar))
                .AddCase(() => Vector128.Create(x).Pow_6_ByVector128ExpLog(p).ToScalar(), nameof(Extensions.Pow_6_ByVector128ExpLog))
#endif
                .DoTest()
                .DumpResults(Console.Out);

            #region Local Methods
            
            void DoAssert(Expression<Func<float>> e)
            {
                var m = (MethodCallExpression)e.Body;
                if (m.Method.Name == "ToScalar")
                    m = (MethodCallExpression)m.Arguments[0];
                float actual = e.Compile().Invoke();
                Console.WriteLine($"{$"{m.Method.Name}:",-50} {actual:R}");
                Assert.IsTrue(Single.IsNaN(expected) && Single.IsNaN(actual) // both NaN
                    || Single.IsInfinity(expected) && Single.IsInfinity(actual) && MathF.Sign(expected) == MathF.Sign(actual) // both same infinity
                    || Single.IsNaN(p) && Single.IsNaN(actual) // when power is NaN, we return NaN, whereas Math.Pow returns 1 for base 1
                    || Single.IsNaN(actual) && (Single.IsInfinity(x) || Single.IsInfinity(p)) // we may return NaN when base or power is infinite, e.g. 1^infinity or Infinity^0
                    || expected.TolerantEquals(actual, MathF.Max(1e-6f, Single.IsInfinity(expected) ? 0f : MathF.Pow(10, MathF.Log10(Math.Abs(expected)) - 6))),
                    $"{expected:R} <-> {actual:R}");
            }

            #endregion

            // Note: To actually get usable comparisons, see the PowVectorTestSamePower test below. 

            // Expected result of Pow(0,9; 2,4):                  0,77657247
            // ==[PowTest (.NET Core 10.0.0-rc.2.25502.107) Results]================================================
            // Test Time: 2 000 ms
            // Warming up: Yes
            // Test cases: 8
            // Repeats: 3
            // Calling GC.Collect: Yes
            // Forced CPU Affinity: No
            // Cases are sorted by fulfilled iterations (the most first)
            // --------------------------------------------------
            // 1. Pow_0_ByMathExpLog: 193 841 510 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 64 613 836,67
            //   #1  64 655 612 iterations in 2 000,00 ms. Adjusted: 64 655 612,00
            //   #2  64 468 956 iterations in 2 000,00 ms. Adjusted: 64 468 956,00	 <---- Worst
            //   #3  64 716 942 iterations in 2 000,00 ms. Adjusted: 64 716 942,00	 <---- Best
            //   Worst-Best difference: 247 986,00 (0,38%)
            // 2. MathF.Pow: 191 700 623 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 63 900 207,67 (-713 629,00 / 98,90%)
            //   #1  63 632 364 iterations in 2 000,00 ms. Adjusted: 63 632 364,00	 <---- Worst
            //   #2  64 188 700 iterations in 2 000,00 ms. Adjusted: 64 188 700,00	 <---- Best
            //   #3  63 879 559 iterations in 2 000,00 ms. Adjusted: 63 879 559,00
            //   Worst-Best difference: 556 336,00 (0,87%)
            // 3. Pow_5_ByVector128ExpLog_Scalar: 181 505 954 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 60 501 984,67 (-4 111 852,00 / 93,64%)
            //   #1  60 667 242 iterations in 2 000,00 ms. Adjusted: 60 667 242,00	 <---- Best
            //   #2  60 397 891 iterations in 2 000,00 ms. Adjusted: 60 397 891,00	 <---- Worst
            //   #3  60 440 821 iterations in 2 000,00 ms. Adjusted: 60 440 821,00
            //   Worst-Best difference: 269 351,00 (0,45%)
            // 4. Pow_6_ByVector128ExpLog: 175 660 750 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 58 553 583,33 (-6 060 253,33 / 90,62%)
            //   #1  58 506 513 iterations in 2 000,00 ms. Adjusted: 58 506 513,00
            //   #2  58 693 607 iterations in 2 000,00 ms. Adjusted: 58 693 607,00	 <---- Best
            //   #3  58 460 630 iterations in 2 000,00 ms. Adjusted: 58 460 630,00	 <---- Worst
            //   Worst-Best difference: 232 977,00 (0,40%)
            // 5. Pow_1_Float: 159 393 535 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 53 131 178,33 (-11 482 658,33 / 82,23%)
            //   #1  53 659 158 iterations in 2 000,00 ms. Adjusted: 53 659 158,00	 <---- Best
            //   #2  52 849 305 iterations in 2 000,00 ms. Adjusted: 52 849 305,00	 <---- Worst
            //   #3  52 885 072 iterations in 2 000,00 ms. Adjusted: 52 885 072,00
            //   Worst-Best difference: 809 853,00 (1,53%)
            // 6. Pow_3_Vector128SamePower: 145 919 835 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 48 639 945,00 (-15 973 891,67 / 75,28%)
            //   #1  48 440 149 iterations in 2 000,00 ms. Adjusted: 48 440 149,00	 <---- Worst
            //   #2  48 868 652 iterations in 2 000,00 ms. Adjusted: 48 868 652,00	 <---- Best
            //   #3  48 611 034 iterations in 2 000,00 ms. Adjusted: 48 611 034,00
            //   Worst-Best difference: 428 503,00 (0,88%)
            // 7. Pow_4_Vector256SamePower: 136 608 681 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 45 536 227,00 (-19 077 609,67 / 70,47%)
            //   #1  45 519 135 iterations in 2 000,00 ms. Adjusted: 45 519 135,00	 <---- Worst
            //   #2  45 563 302 iterations in 2 000,00 ms. Adjusted: 45 563 302,00	 <---- Best
            //   #3  45 526 244 iterations in 2 000,00 ms. Adjusted: 45 526 244,00
            //   Worst-Best difference: 44 167,00 (0,10%)
            // 8. Pow_2_Vector128Full: 108 393 945 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 36 131 315,00 (-28 482 521,67 / 55,92%)
            //   #1  36 119 373 iterations in 2 000,00 ms. Adjusted: 36 119 373,00
            //   #2  36 114 881 iterations in 2 000,00 ms. Adjusted: 36 114 881,00	 <---- Worst
            //   #3  36 159 691 iterations in 2 000,00 ms. Adjusted: 36 159 691,00	 <---- Best
            //   Worst-Best difference: 44 810,00 (0,12%)

            // Expected result of Pow(1,5; 2):                    2,25
            // ==[PowTest (.NET Core 10.0.0-rc.2.25502.107) Results]================================================
            // Test Time: 2 000 ms
            // Warming up: Yes
            // Test cases: 8
            // Repeats: 3
            // Calling GC.Collect: Yes
            // Forced CPU Affinity: No
            // Cases are sorted by fulfilled iterations (the most first)
            // --------------------------------------------------
            // 1. Pow_4_Vector256SamePower: 365 132 643 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 121 710 881,00
            //   #1  122 002 936 iterations in 2 000,00 ms. Adjusted: 122 002 936,00	 <---- Best
            //   #2  121 692 235 iterations in 2 000,00 ms. Adjusted: 121 692 235,00
            //   #3  121 437 472 iterations in 2 000,00 ms. Adjusted: 121 437 472,00	 <---- Worst
            //   Worst-Best difference: 565 464,00 (0,47%)
            // 2. Pow_3_Vector128SamePower: 362 331 122 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 120 777 040,67 (-933 840,33 / 99,23%)
            //   #1  120 604 894 iterations in 2 000,00 ms. Adjusted: 120 604 894,00	 <---- Worst
            //   #2  121 056 720 iterations in 2 000,00 ms. Adjusted: 121 056 720,00	 <---- Best
            //   #3  120 669 508 iterations in 2 000,00 ms. Adjusted: 120 669 508,00
            //   Worst-Best difference: 451 826,00 (0,37%)
            // 3. Pow_1_Float: 335 630 810 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 111 876 936,67 (-9 833 944,33 / 91,92%)
            //   #1  113 009 767 iterations in 2 000,00 ms. Adjusted: 113 009 767,00
            //   #2  113 278 170 iterations in 2 000,00 ms. Adjusted: 113 278 170,00	 <---- Best
            //   #3  109 342 873 iterations in 2 000,00 ms. Adjusted: 109 342 873,00	 <---- Worst
            //   Worst-Best difference: 3 935 297,00 (3,60%)
            // 4. Pow_2_Vector128Full: 288 085 699 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 96 028 566,33 (-25 682 314,67 / 78,90%)
            //   #1  95 905 089 iterations in 2 000,00 ms. Adjusted: 95 905 089,00	 <---- Worst
            //   #2  96 166 319 iterations in 2 000,00 ms. Adjusted: 96 166 319,00	 <---- Best
            //   #3  96 014 291 iterations in 2 000,00 ms. Adjusted: 96 014 291,00
            //   Worst-Best difference: 261 230,00 (0,27%)
            // 5. Pow_0_ByMathExpLog: 184 832 796 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 61 610 930,96 (-60 099 950,04 / 50,62%)
            //   #1  58 119 592 iterations in 2 000,00 ms. Adjusted: 58 119 592,00	 <---- Worst
            //   #2  62 106 816 iterations in 2 000,00 ms. Adjusted: 62 106 812,89
            //   #3  64 606 388 iterations in 2 000,00 ms. Adjusted: 64 606 388,00	 <---- Best
            //   Worst-Best difference: 6 486 796,00 (11,16%)
            // 6. Pow_5_ByVector128ExpLog_Scalar: 181 330 399 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 60 443 466,33 (-61 267 414,67 / 49,66%)
            //   #1  60 178 511 iterations in 2 000,00 ms. Adjusted: 60 178 511,00	 <---- Worst
            //   #2  60 555 069 iterations in 2 000,00 ms. Adjusted: 60 555 069,00
            //   #3  60 596 819 iterations in 2 000,00 ms. Adjusted: 60 596 819,00	 <---- Best
            //   Worst-Best difference: 418 308,00 (0,70%)
            // 7. Pow_6_ByVector128ExpLog: 178 454 568 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 59 484 855,01 (-62 226 025,99 / 48,87%)
            //   #1  59 302 784 iterations in 2 000,00 ms. Adjusted: 59 302 781,03
            //   #2  59 204 689 iterations in 2 000,00 ms. Adjusted: 59 204 689,00	 <---- Worst
            //   #3  59 947 095 iterations in 2 000,00 ms. Adjusted: 59 947 095,00	 <---- Best
            //   Worst-Best difference: 742 406,00 (1,25%)
            // 8. MathF.Pow: 171 310 195 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 57 103 397,36 (-64 607 483,64 / 46,92%)
            //   #1  58 516 376 iterations in 2 000,00 ms. Adjusted: 58 516 373,07	 <---- Best
            //   #2  56 638 081 iterations in 2 000,00 ms. Adjusted: 56 638 081,00
            //   #3  56 155 738 iterations in 2 000,00 ms. Adjusted: 56 155 738,00	 <---- Worst
            //   Worst-Best difference: 2 360 635,07 (4,20%)
        }

        // TODO: Move this into UnitTests when a Pow method with non-scalar power will be required
        [TestCase(2f, 2f, 2f, -2f, -2f, 2f, -2f, -2f)]
        [TestCase(2f, 35f, 2f, -35f, -2f, 35f, -2f, -35f)]
        [TestCase(2f, 1.5f, 2f, -1.5f, -2f, 1.5f, -2f, -1.5f)]
        [TestCase(1.5f, 2f, 1.5f, -2f, -1.5f, 2f, -1.5f, -2f)]
        [TestCase(1.5f, 1.5f, 1.5f, -1.5f, -1.5f, 1.5f, -1.5f, -1.5f)]
        [TestCase(0f, 0f, 0f, 2f, 0f, -2f, 0f, 1.5f)]
        [TestCase(0f, -1.5f, 0f, Single.NegativeInfinity, 0f, Single.PositiveInfinity, 0f, Single.NaN)]
        [TestCase(2f, 0f, -2f, 0f, 1.5f, 0f, -1.5f, 0f)]
        [TestCase(Single.PositiveInfinity, 0f, Single.NegativeInfinity, 0f, Single.NaN, 0f, Single.NaN, -0f)]
        [TestCase(2.148e+9f, 2f, 2.148e+9f, -2f, 2.148e+9f, 1.5f, 2.148e+9f, -1.5f)]
        [TestCase(-2.148e+9f, 2f, -2.148e+9f, -2f, -2.148e+9f, 1.5f, -2.148e+9f, -1.5f)]
        [TestCase(0.9999999f, 2.148e+9f, 0.9999999f, -2.148e+9f, 1.0000001f, 2.148e+9f, 1.0000001f, -2.148e+9f)]
        [TestCase(-0.9999999f, 2.148e+9f, -0.9999999f, -2.148e+9f, -1.0000001f, 2.148e+9f, -1.0000001f, -2.148e+9f)]
        [TestCase(Single.PositiveInfinity, 2f, Single.PositiveInfinity, 1.5f, Single.PositiveInfinity, -2f, Single.PositiveInfinity, -1.5f)]
        [TestCase(Single.NegativeInfinity, 2f, Single.NegativeInfinity, 1.5f, Single.NegativeInfinity, -2f, Single.NegativeInfinity, -1.5f)]
        [TestCase(0.9999999f, Single.PositiveInfinity, -0.9999999f, Single.PositiveInfinity, 0.9999999f, Single.NegativeInfinity, -0.9999999f, Single.NegativeInfinity)]
        [TestCase(1.0000001f, Single.PositiveInfinity, -1.0000001f, Single.PositiveInfinity, 1.0000001f, Single.NegativeInfinity, -1.0000001f, Single.NegativeInfinity)]
        [TestCase(Single.NaN, 2f, Single.NaN, 1.5f, Single.NaN, -2f, Single.NaN, -1.5f)]
        [TestCase(0.9999999f, Single.NaN, -0.9999999f, Single.NaN, 1.0000001f, Single.NaN, -1.0000001f, Single.NaN)]
        [TestCase(0.1f, 1f / 2.4f, 0.5f, 2.4f, 2f, -1.5f, -2f, 5f)]
        [TestCase(0.5f, 1f / 2.4f, 2.148e+9f, 2f, 0.9999999f, 2.148e+9f, 0.9999999f, Single.NaN)]
        [TestCase(0.9f, 1f / 2.4f, Single.NaN, 2f, Single.PositiveInfinity, 2f, 0.9999999f, Single.NegativeInfinity)]
        [TestCase(0.1f, 2.4f, Single.NegativeInfinity, 2f, 0.9999999f, Single.PositiveInfinity, 1.0000001f, -2.148e+9f)]
        public void PowVectorTest(float v1, float p1, float v2, float p2, float v3, float p3, float v4, float p4)
        {
            var vecExpected = Vector128.Create(
                v1.Pow_1_Float(p1),
                v2.Pow_1_Float(p2),
                v3.Pow_1_Float(p3),
                v4.Pow_1_Float(p4));
            var vecActual = Vector128.Create(v1, v2, v3, v4).Pow_2_Vector128Full(Vector128.Create(p1, p2, p3, p4));

            Console.WriteLine($"Expected: {vecExpected}");
            Console.WriteLine($"Actual:   {vecActual}");
            Assert.AreEqual(vecExpected.Or(vecExpected.IsNaN()).AsUInt32(), vecActual.Or(vecActual.IsNaN()).AsUInt32(), $"{vecExpected} vs. {vecActual}");
        }

        [TestCase(2f, -2f, 1.5f, -1.5f, 1.5f)]
        [TestCase(0.5f, 0.25f, 0.125f, Single.NaN, 2.4f)] // LinearToSrgb Pow x3 range
        [TestCase(0.5f, 0.25f, 0.125f, 0.5f, 2.4f)] // LinearToSrgb Pow x4 range
        [TestCase(0.5f, 0.5f, 0.5f, 0.5f, 2.4f)] // same value
        public void PowVectorTestSamePower(float v1, float v2, float v3, float v4, float p)
        {
            #region Local Methods

            static void AssertEqual(Vector128<float> expected, Vector128<float> actual)
            {
                //Assert.AreEqual(expected.Or(expected.IsNaN()).AsUInt32(), actual.Or(actual.IsNaN()).AsUInt32(), $"{expected} vs. {actual}");
                Assert.IsTrue(new ColorF(expected).Clip().TolerantEquals(new ColorF(actual).Clip()), $"{expected} vs. {actual}");
            }

            #endregion

            Vector128<float> expected = Vector128.Create(
                MathF.Pow(v1, p),
                MathF.Pow(v2, p),
                MathF.Pow(v3, p),
                MathF.Pow(v4, p));

            bool isInSpecializedRange = v1 is >= 0 and < Single.PositiveInfinity && v2 is >= 0 and < Single.PositiveInfinity && v3 is >= 0 and < Single.PositiveInfinity && v4 is >= 0 and < Single.PositiveInfinity && p > 0;
            AssertEqual(expected, Vector128.Create(v1, v2, v3, v4).Pow_2_Vector128Full(Vector128.Create(p)));
            AssertEqual(expected, Vector128.Create(v1, v2, v3, v4).Pow_3_Vector128SamePower(p));
            if (isInSpecializedRange)
                AssertEqual(expected, Vector128.Create(v1, v2, v3, v4).Pow_3b_Vector128SamePower_Specialized(p));

#if NET9_0_OR_GREATER
            AssertEqual(expected, Vector128.Create(v1, v2, v3, v4).Pow_6_ByVector128ExpLog(p));
            if (isInSpecializedRange)
                AssertEqual(expected, Vector128.Create(v1, v2, v3, v4).Pow_6b_ByVector128ExpLog_Specialized(p));
#endif

            Vector256<float> actual256 = Vector256.Create(v1, v2, v3, v4, v1, v2, v3, v4).Pow_4_Vector256SamePower(p);
            AssertEqual(expected, actual256.GetLower());
            AssertEqual(expected, actual256.GetUpper());

            var test = new PerformanceTest<Vector128<float>>
                {
                    TestName = $"Linear to sRGB ({v1}, {v2}, {v3}, {v4})^{p}",
                    TestTime = 2000,
                    //Iterations = 10_000_000,
                    Repeat = 3
                }
                .AddCase(() => Vector128.Create(MathF.Pow(v1, p), MathF.Pow(v2, p), MathF.Pow(v3, p), v4.ClipF()), "Math.PowF x3")
                .AddCase(() => Vector128.Create(v1, v2, v3, v4).Pow_2_Vector128Full(Vector128.Create(p)), nameof(Extensions.Pow_2_Vector128Full))
                .AddCase(() => Vector128.Create(v1, v2, v3, v4).Pow_3_Vector128SamePower(p), nameof(Extensions.Pow_3_Vector128SamePower))
                .AddCase(() => Vector128.Create(v1, v2, v3, v4).Pow_3b_Vector128SamePower_Specialized(p), nameof(Extensions.Pow_3b_Vector128SamePower_Specialized))
#if NET9_0_OR_GREATER
                .AddCase(() => Vector128.Create(v1, v2, v3, v4).Pow_6_ByVector128ExpLog(p), nameof(Extensions.Pow_6_ByVector128ExpLog))
#endif
                ;

            if (isInSpecializedRange)
            {
                test.AddCase(() => Vector128.Create(v1, v2, v3, v4).Pow_3b_Vector128SamePower_Specialized(p), nameof(Extensions.Pow_3b_Vector128SamePower_Specialized));
#if NET9_0_OR_GREATER
                test.AddCase(() => Vector128.Create(v1, v2, v3, v4).Pow_6b_ByVector128ExpLog_Specialized(p), nameof(Extensions.Pow_6b_ByVector128ExpLog_Specialized));
#endif
            }

            test.DoTest().DumpResults(Console.Out);

            // Verdict: the custom implementation is slower even than 3x Math.PowF, but the auto-vectorized one can be used on .NET 9+

            // ==[Linear to sRGB (0.5, 0.25, 0.125, 0.5)^2.4 (.NET Core 10.0.0-rc.2.25502.107) Results]================================================
            // Test Time: 2 000 ms
            // Warming up: Yes
            // Test cases: 7
            // Repeats: 3
            // Calling GC.Collect: Yes
            // Forced CPU Affinity: No
            // Cases are sorted by fulfilled iterations (the most first)
            // --------------------------------------------------
            // 1. Pow_6b_ByVector128ExpLog_Specialized: 184 840 207 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 61 613 402,33
            //   #1  62 116 289 iterations in 2 000,00 ms. Adjusted: 62 116 289,00	 <---- Best
            //   #2  61 128 141 iterations in 2 000,00 ms. Adjusted: 61 128 141,00	 <---- Worst
            //   #3  61 595 777 iterations in 2 000,00 ms. Adjusted: 61 595 777,00
            //   Worst-Best difference: 988 148,00 (1,62%)
            // 2. Pow_6_ByVector128ExpLog: 174 613 978 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 58 204 659,33 (-3 408 743,00 / 94,47%)
            //   #1  56 533 298 iterations in 2 000,00 ms. Adjusted: 56 533 298,00	 <---- Worst
            //   #2  58 310 317 iterations in 2 000,00 ms. Adjusted: 58 310 317,00
            //   #3  59 770 363 iterations in 2 000,00 ms. Adjusted: 59 770 363,00	 <---- Best
            //   Worst-Best difference: 3 237 065,00 (5,73%)
            // 3. Math.PowF x3: 141 955 735 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 47 318 578,33 (-14 294 824,00 / 76,80%)
            //   #1  44 303 902 iterations in 2 000,00 ms. Adjusted: 44 303 902,00	 <---- Worst
            //   #2  47 998 551 iterations in 2 000,00 ms. Adjusted: 47 998 551,00
            //   #3  49 653 282 iterations in 2 000,00 ms. Adjusted: 49 653 282,00	 <---- Best
            //   Worst-Best difference: 5 349 380,00 (12,07%)
            // 4. Pow_3b_Vector128SamePower_Specialized: 96 465 543 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 32 155 180,47 (-29 458 221,87 / 52,19%)
            //   #1  32 070 258 iterations in 2 000,00 ms. Adjusted: 32 070 256,40
            //   #2  31 926 899 iterations in 2 000,00 ms. Adjusted: 31 926 899,00	 <---- Worst
            //   #3  32 468 386 iterations in 2 000,00 ms. Adjusted: 32 468 386,00	 <---- Best
            //   Worst-Best difference: 541 487,00 (1,70%)
            // 5. Pow_3b_Vector128SamePower_Specialized: 85 652 585 iterations in 6 000,16 ms. Adjusted for 2 000 ms: 28 550 019,89 (-33 063 382,44 / 46,34%)
            //   #1  32 323 480 iterations in 2 000,16 ms. Adjusted: 32 320 957,35	 <---- Best
            //   #2  27 317 633 iterations in 2 000,00 ms. Adjusted: 27 317 631,63
            //   #3  26 011 472 iterations in 2 000,00 ms. Adjusted: 26 011 470,70	 <---- Worst
            //   Worst-Best difference: 6 309 486,65 (24,26%)
            // 6. Pow_3_Vector128SamePower: 79 341 635 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 26 447 211,31 (-35 166 191,03 / 42,92%)
            //   #1  21 528 671 iterations in 2 000,00 ms. Adjusted: 21 528 669,92	 <---- Worst
            //   #2  28 757 568 iterations in 2 000,00 ms. Adjusted: 28 757 568,00
            //   #3  29 055 396 iterations in 2 000,00 ms. Adjusted: 29 055 396,00	 <---- Best
            //   Worst-Best difference: 7 526 726,08 (34,96%)
            // 7. Pow_2_Vector128Full: 70 805 466 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 23 601 822,00 (-38 011 580,33 / 38,31%)
            //   #1  23 791 493 iterations in 2 000,00 ms. Adjusted: 23 791 493,00
            //   #2  24 341 284 iterations in 2 000,00 ms. Adjusted: 24 341 284,00	 <---- Best
            //   #3  22 672 689 iterations in 2 000,00 ms. Adjusted: 22 672 689,00	 <---- Worst
            //   Worst-Best difference: 1 668 595,00 (7,36%)
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

#if NET7_0_OR_GREATER
        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Vector128<float> ToSrgb_1_AutoVectorization(this Vector128<float> c)
        {
            // replacing alpha with red to improve the chance of falling into the same range
            Vector128<float> rgb = c.WithElement(3, c.GetElement(0));
            Vector128<float> result;

            // value > 0.0031308f
            Vector128<float> maskGreaterThanPowLimit = Vector128.GreaterThan(rgb, Vector128.Create(0.0031308f));
            if (maskGreaterThanPowLimit.AsUInt32() != Vector128<uint>.Zero)
            {
                // 0.0031308f < value < 1f
                Vector128<float> maskPowRange = Vector128.BitwiseAnd(maskGreaterThanPowLimit, Vector128.LessThan(rgb, VectorExtensions.OneF));
                if (maskPowRange.AsUInt32() != Vector128<uint>.Zero)
                {
                    result = rgb.Pow(1f / 2.4f) * Vector128.Create(1.055f) - Vector128.Create(0.055f);

                    // Happy path: if all components are in the pow range, we can return immediately
                    if (maskPowRange.AsUInt32() == Vector128<uint>.AllBitsSet)
                        return result.WithElement(3, c.GetElement(3).ClipF());

                    // Here some values are in the pow range, others are not. Assuming value >= 1f for the out-of-range values for now that can be refined later.
                    result = Vector128.ConditionalSelect(maskPowRange, result, VectorExtensions.OneF);
                }
                else
                {
                    // Here all values are outside the pow range (>= 1f or NaN). Assuming value >= 1f for now that can be refined later.
                    result = VectorExtensions.OneF;
                }
            }
            else
            {
                // value <= 0.0031308f or NaN
                result = Vector128<float>.Zero;
            }

            // 0 < value <= 0.0031308f
            Vector128<float> maskGreaterThanZero = Vector128.GreaterThan(rgb, Vector128<float>.Zero);
            Vector128<float> maskLinearRange = Vector128.AndNot(maskGreaterThanZero, maskGreaterThanPowLimit);
            if (maskLinearRange.AsUInt32() != Vector128<uint>.Zero)
                result = Vector128.ConditionalSelect(maskLinearRange, rgb * Vector128.Create(12.92f), result);

            // value <= 0f or NaN
            result = Vector128.ConditionalSelect(Vector128.OnesComplement(maskGreaterThanZero), Vector128<float>.Zero, result);

            return result.WithElement(3, c.GetElement(3).ClipF());
        }
#endif

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Vector128<float> ToSrgb_2_Intrinsics(this Vector128<float> c)
        {
            // The non-accelerated version
            if (!Sse41.IsSupported)
                return c.ToSrgb_0_Vanilla();

            // replacing alpha with red to improve the chance of falling into the same range
            Vector128<float> rgb = c.WithElement(3, c.GetElement(0));
            Vector128<float> result;

            // value > 0.0031308f
            Vector128<float> maskGreaterThanPowLimit = Sse.CompareGreaterThan(rgb, Vector128.Create(0.0031308f));
            if (!maskGreaterThanPowLimit.AsUInt32().Equals(Vector128<uint>.Zero))
            {
                // 0.0031308f < value < 1f
                Vector128<float> maskPowRange = Sse.And(maskGreaterThanPowLimit, Sse.CompareLessThan(rgb, VectorExtensions.OneF));
                if (!maskPowRange.AsUInt32().Equals(Vector128<uint>.Zero))
                {
                    result = rgb.Pow(1f / 2.4f);
                    result = Fma.IsSupported
                        ? Fma.MultiplySubtract(result, Vector128.Create(1.055f), Vector128.Create(0.055f))
                        : Sse.Subtract(Sse.Multiply(result, Vector128.Create(1.055f)), Vector128.Create(0.055f));

                    // Happy path: if all components are in the pow range, we can return immediately
                    if (maskPowRange.AsUInt32().Equals(VectorExtensions.AllBitsSetF.AsUInt32()))
                        return result.WithElement(3, c.GetElement(3).ClipF());

                    // Here some values are in the pow range, others are not. Assuming value >= 1f for the out-of-range values for now that can be refined later.
                    result = Sse41.BlendVariable(VectorExtensions.OneF, result, maskPowRange);
                }
                else
                {
                    // Here all values are outside the pow range, assuming value >= 1f for now that can be refined later.
                    result = VectorExtensions.OneF;
                }
            }
            else
            {
                // value <= 0.0031308f or NaN
                result = Vector128<float>.Zero;
            }

            // 0 < value <= 0.0031308f
            Vector128<float> maskGreaterThanZero = Sse.CompareGreaterThan(rgb, Vector128<float>.Zero);
            Vector128<float> maskLinearRange = Sse.AndNot(maskGreaterThanPowLimit, maskGreaterThanZero);
            if (!maskLinearRange.AsUInt32().Equals(Vector128<uint>.Zero))
                result = Sse41.BlendVariable(result, Sse.Multiply(rgb, Vector128.Create(12.92f)), maskLinearRange);

            // value <= 0f or NaN
            result = Sse41.BlendVariable(result, Vector128<float>.Zero, Sse.Xor(maskGreaterThanZero, VectorExtensions.AllBitsSetF));

            return result.WithElement(3, c.GetElement(3).ClipF());
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static float Pow_0_ByMathExpLog(this float value, float power) => value switch
        {
            > 0f => MathF.Exp(power * MathF.Log(value)),
            < 0f => (power % 2f) switch
            {
                0f => MathF.Exp(power * MathF.Log(-value)),
                1f or -1f => -MathF.Exp(power * MathF.Log(-value)),
                _ => power switch
                {
                    Single.PositiveInfinity => value < -1f ? Single.PositiveInfinity : 0f,
                    Single.NegativeInfinity => value < -1f ? 0f : Single.PositiveInfinity,
                    _ => Single.NaN // fractional power of a negative number: it is defined only in complex numbers, so returning NaN here
                } 
            },
            0f => power switch
            {
                > 0f => 0f,
                < 0f => Single.PositiveInfinity,
                0f => 1f,
                _ => Single.NaN
            },
            _ => power is 0f ? 1f : Single.NaN // to be conform with MathF.Pow
        };

        // This is the same SW implementation as the one I used for decimals in KGySoft.CoreLibraries, extended with float specific handling (e.g. NaN, Infinity).
        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static float Pow_1_Float(this float value, float power)
        {
            #region Local Methods

            [MethodImpl(MethodImpl.AggressiveInlining)]
            static float PowI(float value, int power)
            {
                if (power < 0)
                {
                    power = -power;
                    value = 1f / value;
                }
                else if (power == 0)
                    return 1f;

                float result = 1f;
                float current = value;
                while (true)
                {
                    if ((power & 1) == 1)
                    {
                        result = current * result;
                        if (power == 1)
                            return result;
                    }

                    power >>>= 1;
                    if (power > 0)
                        current *= current;
                }
            }

            [MethodImpl(MethodImpl.AggressiveInlining)]
            static float ExpE(float power)
            {
                int integerPart = 0;
                if (power > 1f)
                {
                    if (Single.IsPositiveInfinity(power))
                        return Single.PositiveInfinity;
                    float diff = MathF.Floor(power);
                    power -= diff;
                    integerPart = (int)diff;
                }
                else if (power < 0f)
                {
                    float diff = MathF.Floor(MathF.Abs(power));
                    if (diff > FloatExtensions.MaxPreciseIntAsFloat)
                    {
                        diff = FloatExtensions.MaxPreciseIntAsFloat;
                        power = 0f;
                    }
                    else
                        power += diff;
                    integerPart = -(int)diff;
                }

                float result = 1f;
                float acc = 1f;
                for (int i = 1; ; i++)
                {
                    float prevResult = result;
                    acc *= power / i;
                    result += acc;
                    if (prevResult.Equals(result)) // == would cause an infinite loop for NaN
                        break;
                }

                if (integerPart != 0)
                    result *= PowI(MathF.E, integerPart);

                return result;
            }

            [MethodImpl(MethodImpl.AggressiveInlining)]
            static float LogE(float value)
            {
                if (value <= 0f)
                    return value is 0f ? Single.NegativeInfinity : Single.NaN;

                int count = 0;
                while (value >= 1f)
                {
                    if (Single.IsPositiveInfinity(value))
                        return Single.PositiveInfinity;
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
                    if (prevResult.Equals(result)) // == would cause an infinite loop for NaN
                        break;
                }

                return count - result;
            }

            #endregion

            if (Single.IsNaN(power))
                return Single.NaN;

            // FloatExtensions.MaxPreciseIntAsFloat is the largest integer that has exactly the same float representation.
            // If the absolute value of power is larger than that, the result will be either 0 or Infinity anyway.
            // But clipping is needed to avoid other issues as well (e.g. Int32.MaxValue is an odd number, so if value is negative, the sign could be flipped).
            power = power.Clip(FloatExtensions.MinPreciseIntAsFloat, FloatExtensions.MaxPreciseIntAsFloat);

            // Faster if we calculate the result for the integer part fist, and then for the fractional
            float integerPart = MathF.Truncate(power);
            float fracPart = power - integerPart; // without clipping power, it should be: Single.IsPositiveInfinity(power) ? 0f : power - integerPart;
            float result = PowI(value, (int)integerPart); // without clipping power, the cast may turn large even numbers the odd Int32.MaxValue
            if (fracPart != 0f)
                result *= ExpE(fracPart * LogE(value));

            return result;
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Vector128<float> Pow_2_Vector128Full(this Vector128<float> value, Vector128<float> power)
        {
            // Full version means it handles all cases (NaN, Infinity, negative bases, etc.) with vectorization where possible
            #region Local Methods

            [MethodImpl(MethodImpl.AggressiveInlining)]
            static Vector128<float> PowI(Vector128<float> value, Vector128<int> power)
            {
                //if (power < 0)
                //{
                //    power = -power;
                //    value = 1f / value;
                //}

                Vector128<int> negativePowerMask = Sse2.CompareLessThan(power, Vector128<int>.Zero);
                if (!negativePowerMask.Equals(Vector128<int>.Zero))
                {
                    power = Sse2.Subtract(Sse2.Xor(power, negativePowerMask), negativePowerMask);
                    Vector128<float> inverseValue = Sse.Divide(VectorExtensions.OneF, value); //Sse.Reciprocal(value); - Reciprocal has a terrible precision, e.g. for 2f it returns 0.49987793f
                    value = Sse41.BlendVariable(value, inverseValue, negativePowerMask.AsSingle());
                }

                Vector128<float> current = value;
                Vector128<float> result = VectorExtensions.OneF;

                while (true)
                {
                    //if ((power & 1) == 1)
                    //    result = current * result;

                    Vector128<int> powerOddMask = Sse2.CompareEqual(Sse2.And(power, Vector128.Create(1)), Vector128.Create(1));
                    if (!powerOddMask.Equals(Vector128<int>.Zero))
                        result = Sse41.BlendVariable(result, Sse.Multiply(result, current), powerOddMask.AsSingle());

                    //power >>= 1;
                    //if (power > 0)
                    //    current *= current;

                    power = Sse2.ShiftRightLogical(power, 1);
                    current = Sse.Multiply(current, current);

                    if (power.Equals(Vector128<int>.Zero))
                        return result;
                }
            }

            // vectorized LogE (natural log) for base 'value'
            [MethodImpl(MethodImpl.AggressiveInlining)]
            static Vector128<float> LogE(Vector128<float> value)
            {
                // invalidMask = value < 0f || IsNaN(value)
                // infinity = value == 0f || IsPositiveInfinity(value)
                Vector128<float> invalidMask = Sse.Or(Sse.CompareLessThan(value, Vector128<float>.Zero), value.IsNaN());
                Vector128<float> infinityMask = Sse.CompareEqual(value, Vector128.Create(Single.PositiveInfinity));
                Vector128<float> zeroMask = Sse.CompareEqual(value, Vector128<float>.Zero);
                Vector128<float> zeroOrInfinityOrInvalid = Sse.Or(zeroMask, Sse.Or(infinityMask, invalidMask));

                var count = Vector128<int>.Zero;

                // while (value >= 1f && !Single.IsPositiveInfinity(value))
                // {
                //     value *= 1 / MathF.E;
                //     count += 1;
                // }
                Vector128<int> mask;
                while (!(mask = Sse.AndNot(infinityMask, Sse.CompareGreaterThanOrEqual(value, VectorExtensions.OneF)).AsInt32()).Equals(Vector128<int>.Zero))
                {
                    value = Sse41.BlendVariable(value, Sse.Multiply(Vector128.Create(1f / MathF.E), value), mask.AsSingle());
                    count = Sse2.Add(count, Sse2.And(mask, Vector128.Create(1)));
                }

                //while (value <= 1 / MathF.E && !zeroOrInfinityOrInvalid)
                //{
                //    value *= MathF.E;
                //    count -= 1;
                //}
                while (!(mask = Sse.AndNot(zeroOrInfinityOrInvalid, Sse.CompareLessThanOrEqual(value, Vector128.Create(1f / MathF.E))).AsInt32()).Equals(Vector128<int>.Zero))
                {
                    value = Sse41.BlendVariable(value, Sse.Multiply(Vector128.Create(MathF.E), value), mask.AsSingle());
                    count = Sse2.Subtract(count, Sse2.And(mask, Vector128.Create(1)));
                }

                //value -= 1f;
                //if (value == 0f)
                //    return count;

                value = Sse.Subtract(value, VectorExtensions.OneF);

                // going on with Taylor series
                //float result = 0f;
                //float acc = 1f;
                //for (int i = 1; ; i++)
                //{
                //    float prevResult = result;
                //    acc *= -value;
                //    result += acc / i;
                //    if (prevResult == result)
                //        break;
                //}

                Vector128<float> result = Sse.Or(Vector128<float>.Zero, zeroOrInfinityOrInvalid);

                // we could implement it like this, but it would do an unnecessary multiplication and division in the first iteration:
                //Vector128<float> acc = VectorExtensions.OneF;
                //Vector128<float> negativeValue = Negate(value);
                //for (var i = VectorExtensions.OneF; ; i = Sse.Add(i, VectorExtensions.OneF))
                //{
                //    Vector128<uint> prevResult = result.AsUInt32();
                //    acc = Sse.Multiply(acc, negativeValue);
                //    result = Sse.Add(result, Sse.Divide(acc, i));
                //    if (prevResult == result.AsUInt32())
                //        break;
                //}

                // first iteration (i == 1)
                Vector128<uint> prevResult = result.AsUInt32();
                Vector128<float> negativeValue = value.Negate();
                Vector128<float> acc = negativeValue;
                result = Sse.Add(result, acc);

                // further iterations (i >= 2)
                if (!prevResult.Equals(result.AsUInt32()))
                {
                    var i = Vector128.Create(2f);
                    while (true)
                    {
                        prevResult = result.AsUInt32();
                        acc = Sse.Multiply(acc, negativeValue);
                        result = Sse.Add(result, Sse.Divide(acc, i));
                        if (prevResult.Equals(result.AsUInt32()))
                            break;
                        i = Sse.Add(i, VectorExtensions.OneF);
                    }
                }

                //return count - result;
                result = Sse.Subtract(Sse2.ConvertToVector128Single(count), result);
                result = Sse41.BlendVariable(result, Vector128.Create(Single.PositiveInfinity), infinityMask);
                result = Sse41.BlendVariable(result, Vector128.Create(Single.NegativeInfinity), zeroMask);
                return Sse.Or(invalidMask, result);
            }

            // vectorized Exp implementation (copied/recreated algorithmically)
            [MethodImpl(MethodImpl.AggressiveInlining)]
            static Vector128<float> Exp(Vector128<float> power)
            {
                Vector128<int> integerPart = Vector128<int>.Zero;

                //if (power > 1f)
                //{
                //    if (Single.IsPositiveInfinity(power))
                //        power = Single.PositiveInfinity;
                //    else
                //    {
                //        float diff = MathF.Floor(power);
                //        power -= diff;
                //    }

                //    integerPart += (int)diff;
                //}
                Vector128<float> mask = Sse.CompareGreaterThan(power, VectorExtensions.OneF);
                if (!mask.AsUInt32().Equals(Vector128<uint>.Zero))
                {
                    Vector128<float> finiteMask = Sse.CompareNotEqual(power, Vector128.Create(Single.PositiveInfinity));
                    Vector128<float> diff = Sse41.Floor(Sse.And(power, finiteMask));
                    power = Sse.Subtract(power, Sse.And(diff, mask));
                    integerPart = Sse2.Add(integerPart, Sse2.ConvertToVector128Int32WithTruncation(Sse.And(diff, mask)));
                }

                //else if (power < 0f)
                //{
                //    float diff = MathF.Floor(-power);
                //    if (diff > Int32.MaxValue)
                //    {
                //        diff = Int32.MaxValue;
                //        power = 0f;
                //    }
                //    else
                //        power += diff;
                //    integerPart -= (int)diff;
                //}

                mask = Sse.CompareLessThan(power, VectorExtensions.OneF);
                if (!mask.AsUInt32().Equals(Vector128<uint>.Zero))
                {
                    Vector128<float> diff = Sse.And(Sse41.Floor(power.Negate()), mask);
                    Vector128<float> diffTooLargeMask = Sse.CompareGreaterThan(diff, Vector128.Create(FloatExtensions.MaxPreciseIntAsFloat));
                    diff = Sse41.BlendVariable(diff, Vector128.Create(FloatExtensions.MaxPreciseIntAsFloat), diffTooLargeMask);
                    power = Sse41.BlendVariable(power, Vector128<float>.Zero, diffTooLargeMask);
                    power = Sse.Add(power, Sse.AndNot(diffTooLargeMask, Sse.And(diff, mask)));
                    integerPart = Sse2.Subtract(integerPart, Sse2.ConvertToVector128Int32(Sse.And(diff, mask)));
                }

                //float result = 1f;
                //float acc = 1f;
                //for (int i = 1; ; i++)
                //{
                //    float prevResult = result;
                //    acc *= power / i;
                //    result += acc;
                //    if (prevResult == result)
                //        break;
                //}

                // we could implement it like this, but it would do an unnecessary multiplication and division in the first iteration:
                //Vector128<float> invalidMask = power.IsNaN();
                //Vector128<float> result = Sse.Or(VectorExtensions.OneF, invalidMask);
                //Vector128<float> acc = VectorExtensions.OneF;
                //for (Vector128<float> i = VectorExtensions.OneF; ; i = Sse.Add(i, VectorExtensions.OneF))
                //{
                //    Vector128<float> prevResult = result;
                //    acc = Sse.Multiply(acc, Sse.Divide(power, i));
                //    result = Sse.Add(result, acc);
                //    if (prevResult.AsUInt32() == result.AsUInt32()) // bitwise comparison to handle NaN and infinities
                //        break;
                //}

                // first iteration (i == 1)
                Vector128<float> invalidMask = power.IsNaN();
                Vector128<float> acc = power;
                Vector128<uint> prevResult = Sse.Or(VectorExtensions.OneF, invalidMask).AsUInt32();
                Vector128<float> result = Sse.Or(Sse.Add(VectorExtensions.OneF, acc), invalidMask);

                // further iterations (i >= 2)
                if (!prevResult.Equals(result.AsUInt32()))
                {
                    var i = Vector128.Create(2f);
                    while (true)
                    {
                        prevResult = result.AsUInt32();
                        acc = Sse.Multiply(acc, Sse.Divide(power, i));
                        result = Sse.Add(result, acc);
                        if (prevResult.AsUInt32().Equals(result.AsUInt32())) // bitwise int comparison to handle NaN and infinities
                            break;
                        i = Sse.Add(i, VectorExtensions.OneF);
                    }
                }

                //if (integerPart != 0)
                //    result *= PowI(MathF.E, integerPart);
                integerPart = Sse2.AndNot(invalidMask.AsInt32(), integerPart);
                if (!integerPart.Equals(Vector128<int>.Zero))
                    result = Sse.Multiply(result, PowI(Vector128.Create(MathF.E), integerPart));

                return result;
            }

            #endregion

            // Clipping is needed, because unlike (int)floatValue, ConvertToVector128Int32WithTruncation turns too large values into Int32.MinValue rather than MaxValue.
            // But Int32.MaxValue is not a good choice here either, because representable float values around this value are all even, and using an odd power may turn the result negative.
            // Also, blending is needed to preserve NaN values that would be otherwise replaced by Clip.
            //power = power.Clip(Vector128.Create(FloatExtensions.MinPreciseIntAsFloat), FloatExtensions.MaxPreciseIntAsFloat);
#if NET9_0_OR_GREATER // Clamp is intended instead of Clip so NaNs are propagated
            power = Vector128.Clamp(power, Vector128.Create(FloatExtensions.MinPreciseIntAsFloat), Vector128.Create(FloatExtensions.MaxPreciseIntAsFloat));
#else
            power = Sse41.BlendVariable(power.Clip(Vector128.Create(FloatExtensions.MinPreciseIntAsFloat), Vector128.Create(FloatExtensions.MaxPreciseIntAsFloat)), Vector128.Create(Single.NaN), power.IsNaN());
#endif

            //int integerPart = (int)power;
            //float diff = power - integerPart;
            Vector128<int> integerPart = Sse2.ConvertToVector128Int32WithTruncation(power);
            Vector128<float> fracPart = Sse.Subtract(power, Sse2.ConvertToVector128Single(integerPart));

            Vector128<float> intResult = PowI(value, integerPart);
            if (fracPart.AsUInt32().Equals(Vector128<uint>.Zero))
                return intResult;

            // result = Exp(fracPart * LogE(value))
            Vector128<float> fracIsZero = Sse.CompareEqual(fracPart, Vector128<float>.Zero);
            Vector128<float> log = LogE(Sse41.BlendVariable(value, VectorExtensions.AllBitsSetF, fracIsZero));
            Vector128<float> fracResult = Exp(Sse.Multiply(fracPart, Sse41.BlendVariable(log, VectorExtensions.AllBitsSetF, fracIsZero)));
            Vector128<float> result = Sse.Multiply(intResult, fracResult);

            return Sse41.BlendVariable(result, intResult, fracIsZero);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Vector128<float> Pow_3_Vector128SamePower(this Vector128<float> value, float power)
        {
            #region Local Methods

            [MethodImpl(MethodImpl.AggressiveInlining)]
            static Vector128<float> PowI(Vector128<float> value, int power)
            {
                if (power < 0)
                {
                    power = -power;
                    value = Sse.Divide(VectorExtensions.OneF, value); //Sse.Reciprocal(value); - Reciprocal has a terrible precision, e.g. for 2f it returns 0.49987793f
                }
                else if (power == 0)
                    return VectorExtensions.OneF;

                Vector128<float> result = VectorExtensions.OneF;
                Vector128<float> current = value;

                while (true)
                {
                    if ((power & 1) == 1)
                    {
                        result = Sse.Multiply(result, current);
                        if (power == 1)
                            return result;
                    }

                    power >>= 1;
                    if (power > 0)
                        current = Sse.Multiply(current, current);
                }
            }

            [MethodImpl(MethodImpl.AggressiveInlining)]
            static Vector128<float> LogE(Vector128<float> value)
            {
                // invalidMask = value < 0f || IsNaN(value)
                // infinity = value == 0f || IsPositiveInfinity(value)
                Vector128<float> invalidMask = Sse.Or(Sse.CompareLessThan(value, Vector128<float>.Zero), value.IsNaN());
                Vector128<float> infinityMask = Sse.CompareEqual(value, Vector128.Create(Single.PositiveInfinity));
                Vector128<float> zeroMask = Sse.CompareEqual(value, Vector128<float>.Zero);
                Vector128<float> zeroOrInfinityOrInvalid = Sse.Or(zeroMask, Sse.Or(infinityMask, invalidMask));

                var count = Vector128<int>.Zero;

                // while (value >= 1f && !Single.IsPositiveInfinity(value))
                // {
                //     value *= 1 / MathF.E;
                //     count += 1;
                // }
                Vector128<int> mask;
                while (!(mask = Sse.AndNot(infinityMask, Sse.CompareGreaterThanOrEqual(value, VectorExtensions.OneF)).AsInt32()).Equals(Vector128<int>.Zero))
                {
                    value = Sse41.BlendVariable(value, Sse.Multiply(Vector128.Create(1f / MathF.E), value), mask.AsSingle());
                    count = Sse2.Add(count, Sse2.And(mask, Vector128.Create(1)));
                }

                //while (value <= 1 / MathF.E && !zeroOrInfinityOrInvalid)
                //{
                //    value *= MathF.E;
                //    count -= 1;
                //}
                while (!(mask = Sse.AndNot(zeroOrInfinityOrInvalid, Sse.CompareLessThanOrEqual(value, Vector128.Create(1f / MathF.E))).AsInt32()).Equals(Vector128<int>.Zero))
                {
                    value = Sse41.BlendVariable(value, Sse.Multiply(Vector128.Create(MathF.E), value), mask.AsSingle());
                    count = Sse2.Subtract(count, Sse2.And(mask, Vector128.Create(1)));
                }

                //value -= 1f;
                //if (value == 0f)
                //    return count;

                value = Sse.Subtract(value, VectorExtensions.OneF);

                // going on with Taylor series
                //float result = 0f;
                //float acc = 1f;
                //for (int i = 1; ; i++)
                //{
                //    float prevResult = result;
                //    acc *= -value;
                //    result += acc / i;
                //    if (prevResult == result)
                //        break;
                //}

                Vector128<float> result = Sse.Or(Vector128<float>.Zero, zeroOrInfinityOrInvalid);

                // we could implement it like this, but it would do an unnecessary multiplication and division in the first iteration:
                //Vector128<float> acc = VectorExtensions.OneF;
                //Vector128<float> negativeValue = Negate(value);
                //for (var i = VectorExtensions.OneF; ; i = Sse.Add(i, VectorExtensions.OneF))
                //{
                //    Vector128<uint> prevResult = result.AsUInt32();
                //    acc = Sse.Multiply(acc, negativeValue);
                //    result = Sse.Add(result, Sse.Divide(acc, i));
                //    if (prevResult == result.AsUInt32())
                //        break;
                //}

                // first iteration (i == 1)
                Vector128<uint> prevResult = result.AsUInt32();
                Vector128<float> negativeValue = value.Negate();
                Vector128<float> acc = negativeValue;
                result = Sse.Add(result, acc);

                // further iterations (i >= 2)
                if (!prevResult.Equals(result.AsUInt32()))
                {
                    Vector128<float> i = Vector128.Create(2f);
                    //while (true)
                    for (int j = 0; j < 15; j++)
                    {
                        prevResult = result.AsUInt32();
                        acc = Sse.Multiply(acc, negativeValue);
                        result = Sse.Add(result, Sse.Divide(acc, i));
                        if (prevResult.Equals(result.AsUInt32()))
                            break;
                        i = Sse.Add(i, VectorExtensions.OneF);
                    }
                }

                //return count - result;
                result = Sse.Subtract(Sse2.ConvertToVector128Single(count), result);
                result = Sse41.BlendVariable(result, Vector128.Create(Single.PositiveInfinity), infinityMask);
                result = Sse41.BlendVariable(result, Vector128.Create(Single.NegativeInfinity), zeroMask);
                return Sse.Or(invalidMask, result);
            }

            [MethodImpl(MethodImpl.AggressiveInlining)]
            static Vector128<float> ExpE(Vector128<float> power)
            {
                Vector128<int> integerPart = Vector128<int>.Zero;

                //if (power > 1f)
                //{
                //    if (Single.IsPositiveInfinity(power))
                //        power = Single.PositiveInfinity;
                //    else
                //    {
                //        float diff = MathF.Floor(power);
                //        power -= diff;
                //    }

                //    integerPart += (int)diff;
                //}
                Vector128<float> mask = Sse.CompareGreaterThan(power, VectorExtensions.OneF);
                if (!mask.AsUInt32().Equals(Vector128<uint>.Zero))
                {
                    Vector128<float> finiteMask = Sse.CompareNotEqual(power, Vector128.Create(Single.PositiveInfinity));
                    Vector128<float> diff = Sse41.Floor(Sse.And(power, finiteMask));
                    power = Sse.Subtract(power, Sse.And(diff, mask));
                    integerPart = Sse2.Add(integerPart, Sse2.ConvertToVector128Int32WithTruncation(Sse.And(diff, mask)));
                }

                //else if (power < 0f)
                //{
                //    float diff = MathF.Floor(-power);
                //    if (diff > FloatExtensions.MaxPreciseIntAsFloat)
                //    {
                //        diff = FloatExtensions.MaxPreciseIntAsFloat;
                //        power = 0f;
                //    }
                //    else
                //        power += diff;
                //    integerPart -= (int)diff;
                //}
                mask = Sse.CompareLessThan(power, Vector128<float>.Zero);
                if (!mask.AsUInt32().Equals(Vector128<uint>.Zero))
                {
                    Vector128<float> diff = Sse.And(Sse41.Floor(power.Negate()), mask);
                    Vector128<float> diffTooLargeMask = Sse.CompareGreaterThan(diff, Vector128.Create(FloatExtensions.MaxPreciseIntAsFloat));
                    diff = Sse41.BlendVariable(diff, Vector128.Create(FloatExtensions.MaxPreciseIntAsFloat), diffTooLargeMask);
                    power = Sse41.BlendVariable(power, Vector128<float>.Zero, diffTooLargeMask);
                    power = Sse.Add(power, Sse.AndNot(diffTooLargeMask, Sse.And(diff, mask)));
                    integerPart = Sse2.Subtract(integerPart, Sse2.ConvertToVector128Int32(Sse.And(diff, mask)));
                }

                //float result = 1f;
                //float acc = 1f;
                //for (int i = 1; ; i++)
                //{
                //    float prevResult = result;
                //    acc *= power / i;
                //    result += acc;
                //    if (prevResult == result)
                //        break;
                //}

                // we could implement it like this, but it would do an unnecessary multiplication and division in the first iteration:
                //Vector128<float> invalidMask = power.IsNaN();
                //Vector128<float> result = Sse.Or(VectorExtensions.OneF, invalidMask);
                //Vector128<float> acc = VectorExtensions.OneF;
                //for (Vector128<float> i = VectorExtensions.OneF; ; i = Sse.Add(i, VectorExtensions.OneF))
                //{
                //    Vector128<float> prevResult = result;
                //    acc = Sse.Multiply(acc, Sse.Divide(power, i));
                //    result = Sse.Add(result, acc);
                //    if (prevResult.AsUInt32() == result.AsUInt32()) // bitwise comparison to handle NaN and infinities
                //        break;
                //}

                // first iteration (i == 1)
                Vector128<float> invalidMask = power.IsNaN();
                Vector128<float> acc = power;
                Vector128<uint> prevResult = Sse.Or(VectorExtensions.OneF, invalidMask).AsUInt32();
                Vector128<float> result = Sse.Or(Sse.Add(VectorExtensions.OneF, acc), invalidMask);
                
                // further iterations (i >= 2)
                if (!prevResult.Equals(result.AsUInt32()))
                {
                    var i = Vector128.Create(2f);
                    while (true)
                    {
                        prevResult = result.AsUInt32();
                        acc = Sse.Multiply(acc, Sse.Divide(power, i));
                        result = Sse.Add(result, acc);
                        if (prevResult.AsUInt32().Equals(result.AsUInt32())) // bitwise int comparison to handle NaN and infinities
                            break;
                        i = Sse.Add(i, VectorExtensions.OneF);
                    }
                }

                //if (integerPart != 0)
                //    result *= PowI(MathF.E, integerPart);
                integerPart = Sse2.AndNot(invalidMask.AsInt32(), integerPart);
                if (!integerPart.Equals(Vector128<int>.Zero))
                    result = Sse.Multiply(result, PowE(integerPart));

                return result;
            }

            [MethodImpl(MethodImpl.AggressiveInlining)]
            static Vector128<float> PowE(Vector128<int> power)
            {
                Vector128<float> value = Vector128.Create(MathF.E);
                //if (power < 0)
                //{
                //    power = -power;
                //    value = 1f / value;
                //}

                Vector128<int> negativePowerMask = Sse2.CompareLessThan(power, Vector128<int>.Zero);
                if (!negativePowerMask.Equals(Vector128<int>.Zero))
                {
                    power = Sse2.Subtract(Sse2.Xor(power, negativePowerMask), negativePowerMask);
                    Vector128<float> inverseValue = Sse.Divide(VectorExtensions.OneF, value); //Sse.Reciprocal(value); - Reciprocal has a terrible precision, e.g. for 2f it returns 0.49987793f
                    value = Sse41.BlendVariable(value, inverseValue, negativePowerMask.AsSingle());
                }

                Vector128<float> current = value;
                Vector128<float> result = VectorExtensions.OneF;

                while (true)
                {
                    //if ((power & 1) == 1)
                    //    result = current * result;

                    Vector128<int> powerOddMask = Sse2.CompareEqual(Sse2.And(power, Vector128.Create(1)), Vector128.Create(1));
                    if (!powerOddMask.Equals(Vector128<int>.Zero))
                        result = Sse41.BlendVariable(result, Sse.Multiply(result, current), powerOddMask.AsSingle());

                    //power >>= 1;
                    //if (power > 0)
                    //    current *= current;

                    power = Sse2.ShiftRightLogical(power, 1);
                    current = Sse.Multiply(current, current);

                    if (power.Equals(Vector128<int>.Zero))
                        return result;
                }
            }

            #endregion

            if (Single.IsNaN(power))
                return Vector128.Create(Single.NaN);

            // FloatExtensions.MaxPreciseIntAsFloat is the largest integer that has exactly the same float representation.
            // If the absolute value of power is larger than that, the result will be either 0 or Infinity anyway.
            // But clipping is needed to avoid issues (e.g. Int32.MaxValue is an odd number, so if value is negative, the sign could be flipped).
            power = power.Clip(FloatExtensions.MinPreciseIntAsFloat, FloatExtensions.MaxPreciseIntAsFloat);

            // Faster if we calculate the result for the integer part fist, and then for the fractional
            float integerPart = MathF.Truncate(power);
            float fracPart = power - integerPart; // without clipping power, it should be: Single.IsPositiveInfinity(power) ? 0f : power - integerPart;
            var result = PowI(value, (int)integerPart); // without clipping power, the cast may turn large even numbers into the odd Int32.MaxValue
            if (fracPart != 0f)
                result = Sse.Multiply(result,  ExpE(Sse.Multiply(Vector128.Create(fracPart), LogE(value))));

            return result;
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Vector128<float> Pow_3b_Vector128SamePower_Specialized(this Vector128<float> value, float power) // value: [0..1], power: (0..Infinity]
        {
            #region Local Methods

            [MethodImpl(MethodImpl.AggressiveInlining)]
            static Vector128<float> PowI(Vector128<float> value, int power) // value: [0..1], power: [0..MaxPreciseIntAsFloat] => [0..1]
            {
                //if (power < 0)
                //{
                //    power = -power;
                //    value = Sse.Divide(VectorExtensions.OneF, value); //Sse.Reciprocal(value); - Reciprocal has a terrible precision, e.g. for 2f it returns 0.49987793f
                //}
                if (power == 0)
                    return VectorExtensions.OneF;

                Vector128<float> result = VectorExtensions.OneF;
                Vector128<float> current = value;

                while (true)
                {
                    if ((power & 1) == 1)
                    {
                        result = Sse.Multiply(result, current);
                        if (power == 1)
                            return result;
                    }

                    power >>= 1;
                    if (power > 0)
                        current = Sse.Multiply(current, current);
                }
            }

            [MethodImpl(MethodImpl.AggressiveInlining)]
            static Vector128<float> LogE(Vector128<float> value) // value: [0..1] => [-infinity..0]
            {
                // invalidMask = value < 0f || IsNaN(value)
                // infinity = value == 0f || IsPositiveInfinity(value)
                //Vector128<float> invalidMask = Sse.Or(Sse.CompareLessThan(value, Vector128<float>.Zero), value.IsNaN());
                //Vector128<float> infinityMask = Sse.CompareEqual(value, Vector128.Create(Single.PositiveInfinity));
                Vector128<float> zeroMask = Sse.CompareEqual(value, Vector128<float>.Zero);
                //Vector128<float> zeroOrInfinityOrInvalid = Sse.Or(zeroMask, Sse.Or(infinityMask, invalidMask));

                var count = Vector128<int>.Zero;

                // while (value >= 1f && !Single.IsPositiveInfinity(value))
                // {
                //     value *= 1 / MathF.E;
                //     count += 1;
                // }
                Vector128<int> mask;
                while (!(mask = Sse.AndNot(zeroMask, Sse.CompareGreaterThanOrEqual(value, VectorExtensions.OneF)).AsInt32()).Equals(Vector128<int>.Zero))
                {
                    value = Sse41.BlendVariable(value, Sse.Multiply(Vector128.Create(1f / MathF.E), value), mask.AsSingle());
                    count = Sse2.Add(count, Sse2.And(mask, Vector128.Create(1)));
                }

                //while (value <= 1 / MathF.E && !zeroOrInfinityOrInvalid)
                //{
                //    value *= MathF.E;
                //    count -= 1;
                //}
                while (!(mask = Sse.AndNot(zeroMask, Sse.CompareLessThanOrEqual(value, Vector128.Create(1f / MathF.E))).AsInt32()).Equals(Vector128<int>.Zero))
                {
                    value = Sse41.BlendVariable(value, Sse.Multiply(Vector128.Create(MathF.E), value), mask.AsSingle());
                    count = Sse2.Subtract(count, Sse2.And(mask, Vector128.Create(1)));
                }

                //value -= 1f;
                //if (value == 0f)
                //    return count;

                value = Sse.Subtract(value, VectorExtensions.OneF);

                // going on with Taylor series
                //float result = 0f;
                //float acc = 1f;
                //for (int i = 1; ; i++)
                //{
                //    float prevResult = result;
                //    acc *= -value;
                //    result += acc / i;
                //    if (prevResult == result)
                //        break;
                //}

                Vector128<float> result = Sse.Or(Vector128<float>.Zero, zeroMask);

                // we could implement it like this, but it would do an unnecessary multiplication and division in the first iteration:
                //Vector128<float> acc = VectorExtensions.OneF;
                //Vector128<float> negativeValue = Negate(value);
                //for (var i = VectorExtensions.OneF; ; i = Sse.Add(i, VectorExtensions.OneF))
                //{
                //    Vector128<uint> prevResult = result.AsUInt32();
                //    acc = Sse.Multiply(acc, negativeValue);
                //    result = Sse.Add(result, Sse.Divide(acc, i));
                //    if (prevResult == result.AsUInt32())
                //        break;
                //}

                // first iteration (i == 1)
                Vector128<uint> prevResult = result.AsUInt32();
                Vector128<float> negativeValue = value.Negate();
                Vector128<float> acc = negativeValue;
                result = Sse.Add(result, acc);

                // further iterations (i >= 2)
                if (!prevResult.Equals(result.AsUInt32()))
                {
                    Vector128<float> i = Vector128.Create(2f);
                    //while (true)
                    for (int j = 0; j < 15; j++)
                    {
                        prevResult = result.AsUInt32();
                        acc = Sse.Multiply(acc, negativeValue);
                        result = Sse.Add(result, Sse.Divide(acc, i));
                        if (prevResult.Equals(result.AsUInt32()))
                            break;
                        i = Sse.Add(i, VectorExtensions.OneF);
                    }
                }

                //return count - result;
                result = Sse.Subtract(Sse2.ConvertToVector128Single(count), result);
                //result = Sse41.BlendVariable(result, Vector128.Create(Single.PositiveInfinity), infinityMask);
                result = Sse41.BlendVariable(result, Vector128.Create(Single.NegativeInfinity), zeroMask);
                //return Sse.Or(invalidMask, result);
                return result;
            }

            [MethodImpl(MethodImpl.AggressiveInlining)]
            static Vector128<float> ExpE(Vector128<float> power) // power: [-infinity..0] => [0..1]
            {
                Vector128<int> integerPart = Vector128<int>.Zero;

                //if (power > 1f)
                //{
                //    if (Single.IsPositiveInfinity(power))
                //        power = Single.PositiveInfinity;
                //    else
                //    {
                //        float diff = MathF.Floor(power);
                //        power -= diff;
                //    }

                //    integerPart += (int)diff;
                //}
                //Vector128<float> mask = Sse.CompareGreaterThan(power, VectorExtensions.OneF);
                //if (!mask.AsUInt32().Equals(Vector128<uint>.Zero))
                //{
                //    Vector128<float> finiteMask = Sse.CompareNotEqual(power, Vector128.Create(Single.PositiveInfinity));
                //    Vector128<float> diff = Sse41.Floor(Sse.And(power, finiteMask));
                //    power = Sse.Subtract(power, Sse.And(diff, mask));
                //    integerPart = Sse2.Add(integerPart, Sse2.ConvertToVector128Int32WithTruncation(Sse.And(diff, mask)));
                //}

                //else if (power < 0f)
                //{
                //    float diff = MathF.Floor(-power);
                //    if (diff > FloatExtensions.MaxPreciseIntAsFloat)
                //    {
                //        diff = FloatExtensions.MaxPreciseIntAsFloat;
                //        power = 0f;
                //    }
                //    else
                //        power += diff;
                //    integerPart -= (int)diff;
                //}

                Vector128<float> mask = Sse.CompareLessThan(power, Vector128<float>.Zero);
                if (!mask.AsUInt32().Equals(Vector128<uint>.Zero))
                {
                    Vector128<float> diff = Sse.And(Sse41.Floor(power.Negate()), mask);
                    Vector128<float> diffTooLargeMask = Sse.CompareGreaterThan(diff, Vector128.Create(FloatExtensions.MaxPreciseIntAsFloat));
                    diff = Sse41.BlendVariable(diff, Vector128.Create(FloatExtensions.MaxPreciseIntAsFloat), diffTooLargeMask);
                    power = Sse41.BlendVariable(power, Vector128<float>.Zero, diffTooLargeMask);
                    power = Sse.Add(power, Sse.AndNot(diffTooLargeMask, Sse.And(diff, mask)));
                    integerPart = Sse2.Subtract(integerPart, Sse2.ConvertToVector128Int32(Sse.And(diff, mask)));
                }

                //float result = 1f;
                //float acc = 1f;
                //for (int i = 1; ; i++)
                //{
                //    float prevResult = result;
                //    acc *= power / i;
                //    result += acc;
                //    if (prevResult == result)
                //        break;
                //}

                // we could implement it like this, but it would do an unnecessary multiplication and division in the first iteration:
                //Vector128<float> invalidMask = power.IsNaN();
                //Vector128<float> result = Sse.Or(VectorExtensions.OneF, invalidMask);
                //Vector128<float> acc = VectorExtensions.OneF;
                //for (Vector128<float> i = VectorExtensions.OneF; ; i = Sse.Add(i, VectorExtensions.OneF))
                //{
                //    Vector128<float> prevResult = result;
                //    acc = Sse.Multiply(acc, Sse.Divide(power, i));
                //    result = Sse.Add(result, acc);
                //    if (prevResult.AsUInt32() == result.AsUInt32()) // bitwise comparison to handle NaN and infinities
                //        break;
                //}

                // first iteration (i == 1)
                Vector128<float> invalidMask = power.IsNaN();
                Vector128<float> acc = power;
                Vector128<uint> prevResult = Sse.Or(VectorExtensions.OneF, invalidMask).AsUInt32();
                Vector128<float> result = Sse.Or(Sse.Add(VectorExtensions.OneF, acc), invalidMask);
                
                // further iterations (i >= 2)
                if (!prevResult.Equals(result.AsUInt32()))
                {
                    var i = Vector128.Create(2f);
                    while (true)
                    {
                        prevResult = result.AsUInt32();
                        acc = Sse.Multiply(acc, Sse.Divide(power, i));
                        result = Sse.Add(result, acc);
                        if (prevResult.AsUInt32().Equals(result.AsUInt32())) // bitwise int comparison to handle NaN and infinities
                            break;
                        i = Sse.Add(i, VectorExtensions.OneF);
                    }
                }

                //if (integerPart != 0)
                //    result *= PowI(MathF.E, integerPart);
                integerPart = Sse2.AndNot(invalidMask.AsInt32(), integerPart);
                if (!integerPart.Equals(Vector128<int>.Zero))
                    result = Sse.Multiply(result, PowE(integerPart));

                return result;
            }

            [MethodImpl(MethodImpl.AggressiveInlining)]
            static Vector128<float> PowE(Vector128<int> power) // power: [-MaxPreciseIntAsFloat..0] => [0..1]
            {
                Vector128<float> value = Vector128.Create(MathF.E);
                //if (power < 0)
                //{
                //    power = -power;
                //    value = 1f / value;
                //}

                Vector128<int> negativePowerMask = Sse2.CompareLessThan(power, Vector128<int>.Zero);
                if (!negativePowerMask.Equals(Vector128<int>.Zero))
                {
                    power = Sse2.Subtract(Sse2.Xor(power, negativePowerMask), negativePowerMask);
                    Vector128<float> inverseValue = Sse.Divide(VectorExtensions.OneF, value); //Sse.Reciprocal(value); - Reciprocal has a terrible precision, e.g. for 2f it returns 0.49987793f
                    value = Sse41.BlendVariable(value, inverseValue, negativePowerMask.AsSingle());
                }

                Vector128<float> current = value;
                Vector128<float> result = VectorExtensions.OneF;

                while (true)
                {
                    //if ((power & 1) == 1)
                    //    result = current * result;

                    Vector128<int> powerOddMask = Sse2.CompareEqual(Sse2.And(power, Vector128.Create(1)), Vector128.Create(1));
                    if (!powerOddMask.Equals(Vector128<int>.Zero))
                        result = Sse41.BlendVariable(result, Sse.Multiply(result, current), powerOddMask.AsSingle());

                    //power >>= 1;
                    //if (power > 0)
                    //    current *= current;

                    power = Sse2.ShiftRightLogical(power, 1);
                    current = Sse.Multiply(current, current);

                    if (power.Equals(Vector128<int>.Zero))
                        return result;
                }
            }

            #endregion

            //if (Single.IsNaN(power))
            //    return Vector128.Create(Single.NaN);

            // FloatExtensions.MaxPreciseIntAsFloat is the largest integer that has exactly the same float representation.
            // If the absolute value of power is larger than that, the result will be either 0 or Infinity anyway.
            // But clipping is needed to avoid issues (e.g. Int32.MaxValue is an odd number, so if value is negative, the sign could be flipped).
            //power = power.Clip(Vector128.Create(FloatExtensions.MinPreciseIntAsFloat), FloatExtensions.MaxPreciseIntAsFloat);
            power = Math.Min(power, FloatExtensions.MaxPreciseIntAsFloat);

            // Faster if we calculate the result for the integer part fist, and then for the fractional
            float integerPart = MathF.Truncate(power);
            float fracPart = power - integerPart; // without clipping power, it should be: Single.IsPositiveInfinity(power) ? 0f : power - integerPart;
            var result = PowI(value, (int)integerPart); // without clipping power, the cast may turn large even numbers into the odd Int32.MaxValue
            if (fracPart != 0f)
                result = Sse.Multiply(result,  ExpE(Sse.Multiply(Vector128.Create(fracPart), LogE(value))));

            return result;
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Vector256<float> Pow_4_Vector256SamePower(this Vector256<float> value, float power)
        {
            #region Local Methods

            [MethodImpl(MethodImpl.AggressiveInlining)]
            static Vector256<float> PowI(Vector256<float> value, int power)
            {
                if (power < 0)
                {
                    power = -power;
                    value = Avx.Divide(Vector256.Create(1f), value); //Avx.Reciprocal(value); // Reciprocal has a quite low precision
                }
                else if (power == 0)
                    return Vector256.Create(1f);

                Vector256<float> result = Vector256.Create(1f);
                Vector256<float> current = value;

                while (true)
                {
                    if ((power & 1) == 1)
                    {
                        result = Avx.Multiply(result, current);
                        if (power == 1)
                            return result;
                    }

                    power >>= 1;
                    if (power > 0)
                        current = Avx.Multiply(current, current);
                }
            }

            [MethodImpl(MethodImpl.AggressiveInlining)]
            static Vector256<float> LogE(Vector256<float> value)
            {
                // invalidMask = value < 0f || IsNaN(value)
                // infinity = value == 0f || IsPositiveInfinity(value)
                Vector256<float> invalidMask = Avx.Or(Avx.Compare(value, Vector256<float>.Zero, FloatComparisonMode.OrderedLessThanNonSignaling), value.IsNaN());
                Vector256<float> infinityMask = Avx.Compare(value, Vector256.Create(Single.PositiveInfinity), FloatComparisonMode.OrderedEqualNonSignaling);
                Vector256<float> zeroMask = Avx.Compare(value, Vector256<float>.Zero, FloatComparisonMode.OrderedEqualNonSignaling);
                Vector256<float> zeroOrInfinityOrInvalid = Avx.Or(zeroMask, Avx.Or(infinityMask, invalidMask));

                Vector256<int> count = Vector256<int>.Zero;

                // while (value >= 1f && !Single.IsPositiveInfinity(value))
                // {
                //     value *= 1 / MathF.E;
                //     count += 1;
                // }
                Vector256<int> mask;
                while (!(mask = Avx.AndNot(infinityMask, Avx.Compare(value, Vector256.Create(1f), FloatComparisonMode.OrderedGreaterThanOrEqualNonSignaling)).AsInt32()).Equals(Vector256<int>.Zero))
                {
                    value = Avx.BlendVariable(value, Avx.Multiply(Vector256.Create(1f / MathF.E), value), mask.AsSingle());
                    count = Avx2.Add(count, Avx2.And(mask, Vector256.Create(1)));
                }

                //while (value <= 1 / MathF.E && !zeroOrInfinityOrInvalid)
                //{
                //    value *= MathF.E;
                //    count -= 1;
                //}
                while (!(mask = Avx.AndNot(zeroOrInfinityOrInvalid, Avx.Compare(value, Vector256.Create(1f / MathF.E), FloatComparisonMode.OrderedLessThanOrEqualNonSignaling)).AsInt32()).Equals(Vector256<int>.Zero))
                {
                    value = Avx.BlendVariable(value, Avx.Multiply(Vector256.Create(MathF.E), value), mask.AsSingle());
                    count = Avx2.Subtract(count, Avx2.And(mask, Vector256.Create(1)));
                }

                //value -= 1f;
                //if (value == 0f)
                //    return count;

                value = Avx.Subtract(value, Vector256.Create(1f));

                // going on with Taylor series
                //float result = 0f;
                //float acc = 1f;
                //for (int i = 1; ; i++)
                //{
                //    float prevResult = result;
                //    acc *= -value;
                //    result += acc / i;
                //    if (prevResult == result)
                //        break;
                //}

                Vector256<float> result = Avx.Or(Vector256<float>.Zero, zeroOrInfinityOrInvalid);

                // we could implement it like this, but it would do an unnecessary multiplication and division in the first iteration:
                //Vector256<float> acc = VectorExtensions.OneF256;
                //Vector256<float> negativeValue = Negate(value);
                //for (var i = VectorExtensions.OneF256; ; i = Avx.Add(i, VectorExtensions.OneF256))
                //{
                //    Vector256<uint> prevResult = result.AsUInt32();
                //    acc = Avx.Multiply(acc, negativeValue);
                //    result = Avx.Add(result, Avx.Divide(acc, i));
                //    if (prevResult == result.AsUInt32())
                //        break;
                //}

                // first iteration (i == 1)
                Vector256<uint> prevResult = result.AsUInt32();
                Vector256<float> negativeValue = value.Negate();
                Vector256<float> acc = negativeValue;
                result = Avx.Add(result, acc);

                // further iterations (i >= 2)
                if (!prevResult.Equals(result.AsUInt32()))
                {
                    var i = Vector256.Create(2f);
                    while (true)
                    {
                        prevResult = result.AsUInt32();
                        acc = Avx.Multiply(acc, negativeValue);
                        result = Avx.Add(result, Avx.Divide(acc, i));
                        if (prevResult.Equals(result.AsUInt32()))
                            break;
                        i = Avx.Add(i, Vector256.Create(1f));
                    }
                }

                //return count - result;
                result = Avx.Subtract(Avx.ConvertToVector256Single(count), result);
                result = Avx.BlendVariable(result, Vector256.Create(Single.PositiveInfinity), infinityMask);
                result = Avx.BlendVariable(result, Vector256.Create(Single.NegativeInfinity), zeroMask);
                return Avx.Or(invalidMask, result);
            }

            [MethodImpl(MethodImpl.AggressiveInlining)]
            static Vector256<float> ExpE(Vector256<float> power)
            {
                Vector256<int> integerPart = Vector256<int>.Zero;

                //if (power > 1f)
                //{
                //    if (Single.IsPositiveInfinity(power))
                //        power = Single.PositiveInfinity;
                //    else
                //    {
                //        float diff = MathF.Floor(power);
                //        power -= diff;
                //    }

                //    integerPart += (int)diff;
                //}
                Vector256<float> mask = Avx.Compare(power, Vector256.Create(1f), FloatComparisonMode.OrderedGreaterThanNonSignaling);
                if (!mask.AsUInt32().Equals(Vector256<uint>.Zero))
                {
                    Vector256<float> finiteMask = Avx.Compare(power, Vector256.Create(Single.PositiveInfinity), FloatComparisonMode.UnorderedNotEqualNonSignaling);
                    Vector256<float> diff = Avx.Floor(Avx.And(power, finiteMask));
                    power = Avx.Subtract(power, Avx.And(diff, mask));
                    integerPart = Avx2.Add(integerPart, Avx.ConvertToVector256Int32WithTruncation(Avx.And(diff, mask)));
                }

                //else if (power < 0f)
                //{
                //    float diff = MathF.Floor(-power);
                //    if (diff > FloatExtensions.MaxPreciseIntAsFloat)
                //    {
                //        diff = FloatExtensions.MaxPreciseIntAsFloat;
                //        power = 0f;
                //    }
                //    else
                //        power += diff;
                //    integerPart -= (int)diff;
                //}
                mask = Avx.Compare(power, Vector256<float>.Zero, FloatComparisonMode.OrderedLessThanNonSignaling);
                if (!mask.AsUInt32().Equals(Vector256<uint>.Zero))
                {
                    Vector256<float> diff = Avx.And(Avx.Floor(power.Negate()), mask);
                    Vector256<float> diffTooLargeMask = Avx.Compare(diff, Vector256.Create(FloatExtensions.MaxPreciseIntAsFloat), FloatComparisonMode.OrderedGreaterThanNonSignaling);
                    diff = Avx.BlendVariable(diff, Vector256.Create(FloatExtensions.MaxPreciseIntAsFloat), diffTooLargeMask);
                    power = Avx.BlendVariable(power, Vector256<float>.Zero, diffTooLargeMask);
                    power = Avx.Add(power, Avx.AndNot(diffTooLargeMask, Avx.And(diff, mask)));
                    integerPart = Avx2.Subtract(integerPart, Avx.ConvertToVector256Int32(Avx.And(diff, mask)));
                }

                //float result = 1f;
                //float acc = 1f;
                //for (int i = 1; ; i++)
                //{
                //    float prevResult = result;
                //    acc *= power / i;
                //    result += acc;
                //    if (prevResult == result)
                //        break;
                //}

                // we could implement it like this, but it would do an unnecessary multiplication and division in the first iteration:
                //Vector256<float> invalidMask = power.IsNaN();
                //Vector256<float> result = Avx.Or(VectorExtensions.OneF256, invalidMask);
                //Vector256<float> acc = VectorExtensions.OneF256;
                //for (Vector256<float> i = VectorExtensions.OneF256; ; i = Avx.Add(i, VectorExtensions.OneF256))
                //{
                //    Vector256<float> prevResult = result;
                //    acc = Avx.Multiply(acc, Avx.Divide(power, i));
                //    result = Avx.Add(result, acc);
                //    if (prevResult.AsUInt32() == result.AsUInt32()) // bitwise comparison to handle NaN and infinities
                //        break;
                //}

                // first iteration (i == 1)
                Vector256<float> invalidMask = power.IsNaN();
                Vector256<float> acc = power;
                Vector256<uint> prevResult = Avx.Or(Vector256.Create(1f), invalidMask).AsUInt32();
                Vector256<float> result = Avx.Or(Avx.Add(Vector256.Create(1f), acc), invalidMask);
                
                // further iterations (i >= 2)
                if (!prevResult.Equals(result.AsUInt32()))
                {
                    var i = Vector256.Create(2f);
                    while (true)
                    {
                        prevResult = result.AsUInt32();
                        acc = Avx.Multiply(acc, Avx.Divide(power, i));
                        result = Avx.Add(result, acc);
                        if (prevResult.AsUInt32().Equals(result.AsUInt32())) // bitwise int comparison to handle NaN and infinities
                            break;
                        i = Avx.Add(i, Vector256.Create(1f));
                    }
                }

                //if (integerPart != 0)
                //    result *= PowI(MathF.E, integerPart);
                integerPart = Avx2.AndNot(invalidMask.AsInt32(), integerPart);
                if (!integerPart.Equals(Vector256<int>.Zero))
                    result = Avx.Multiply(result, PowE(integerPart));

                return result;
            }

            [MethodImpl(MethodImpl.AggressiveInlining)]
            static Vector256<float> PowE(Vector256<int> power)
            {
                Vector256<float> value = Vector256.Create(MathF.E);
                //if (power < 0)
                //{
                //    power = -power;
                //    value = 1f / value;
                //}

                Vector256<int> negativePowerMask = Avx2.ShiftRightArithmetic(power, 31); // same as CompareLessThan(power, Vector256<int>.Zero), which exists only starting with Avx512F.VL for ints.
                if (!negativePowerMask.Equals(Vector256<int>.Zero))
                {
                    power = Avx2.Subtract(Avx2.Xor(power, negativePowerMask), negativePowerMask);
                    Vector256<float> inverseValue = Avx.Divide(Vector256.Create(1f), value); //Avx.Reciprocal(value); - Reciprocal has a terrible precision, e.g. for 2f it returns 0.49987793f
                    value = Avx.BlendVariable(value, inverseValue, negativePowerMask.AsSingle());
                }

                Vector256<float> current = value;
                Vector256<float> result = Vector256.Create(1f);

                while (true)
                {
                    //if ((power & 1) == 1)
                    //    result = current * result;

                    Vector256<int> powerOddMask = Avx2.CompareEqual(Avx2.And(power, Vector256.Create(1)), Vector256.Create(1));
                    if (!powerOddMask.Equals(Vector256<int>.Zero))
                        result = Avx.BlendVariable(result, Avx.Multiply(result, current), powerOddMask.AsSingle());

                    //power >>= 1;
                    //if (power > 0)
                    //    current *= current;

                    power = Avx2.ShiftRightLogical(power, 1);
                    current = Avx.Multiply(current, current);

                    if (power.Equals(Vector256<int>.Zero))
                        return result;
                }
            }

            #endregion

            if (Single.IsNaN(power))
                return Vector256.Create(Single.NaN);

            // FloatExtensions.MaxPreciseIntAsFloat is the largest integer that has exactly the same float representation.
            // If the absolute value of power is larger than that, the result will be either 0 or Infinity anyway.
            // But clipping is needed to avoid issues (e.g. Int32.MaxValue is an odd number, so if value is negative, the sign could be flipped).
            power = power.Clip(FloatExtensions.MinPreciseIntAsFloat, FloatExtensions.MaxPreciseIntAsFloat);

            // Faster if we calculate the result for the integer part fist, and then for the fractional
            float integerPart = MathF.Truncate(power);
            float fracPart = power - integerPart; // without clipping power, it should be: Single.IsPositiveInfinity(power) ? 0f : power - integerPart;
            var result = PowI(value, (int)integerPart); // without clipping power, the cast may turn large even numbers into the odd Int32.MaxValue
            if (fracPart != 0f)
                result = Avx.Multiply(result,  ExpE(Avx.Multiply(Vector256.Create(fracPart), LogE(value))));

            return result;
        }

#if NET9_0_OR_GREATER
        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static float Pow_5_ByVector128ExpLog_Scalar(this float value, float power)
        {
            return value switch
            {
                > 0f => Vector128.Exp(power * Vector128.Log(Vector128.Create(value))).ToScalar(),
                < 0f => (power % 2f) switch
                {
                    0f => Vector128.Exp(power * Vector128.Log(Vector128.Create(-value))).ToScalar(),
                    1f or -1f => -Vector128.Exp(power * Vector128.Log(Vector128.Create(-value))).ToScalar(),
                    _ => power switch
                    {
                        Single.PositiveInfinity => value < -1f ? Single.PositiveInfinity : 0f,
                        Single.NegativeInfinity => value < -1f ? 0f : Single.PositiveInfinity,
                        _ => Single.NaN // fractional power of a negative number: it is defined only in complex numbers, so returning NaN here
                    }
                },
                0f => power switch
                {
                    > 0f => 0f,
                    < 0f => Single.PositiveInfinity,
                    0f => 1f,
                    _ => Single.NaN
                },
                _ => power is 0f ? 1f : Single.NaN
            };
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Vector128<float> Pow_6_ByVector128ExpLog(this Vector128<float> value, float power)
        {
            Vector128<float> p = Vector128.Create(power);

            // if (value > 0f)
            //     return MathF.Exp(power * MathF.Log(value));

            Vector128<float> mask = Vector128.GreaterThan(value, Vector128<float>.Zero);
            Vector128<float> result = mask.AsUInt32() == Vector128<uint>.Zero
                ? Vector128<float>.Zero
                : Vector128.Exp(p * Vector128.Log(value));

            if (mask.AsUInt32() == Vector128<uint>.AllBitsSet)
                return result;

            //if (value < 0f)
            //{
            //    return (power % 2f) switch
            //    {
            //        0f => MathF.Exp(power * MathF.Log(-value)),
            //        1f or -1f => -MathF.Exp(power * MathF.Log(-value)),
            //        _ => power switch
            //        {
            //            Single.PositiveInfinity => value < -1f ? Single.PositiveInfinity : 0f,
            //            Single.NegativeInfinity => value < -1f ? 0f : Single.PositiveInfinity,
            //            _ => Single.NaN
            //        }
            //    };
            //}

            mask = Vector128.LessThan(value, Vector128<float>.Zero);
            if (mask.AsUInt32() != Vector128<uint>.Zero)
            {
                Vector128<float> res = (power % 2f) switch
                {
                    0f => Vector128.Exp(p * Vector128.Log(-value)),
                    1f or -1f => -Vector128.Exp(p * Vector128.Log(-value)),
                    _ => power switch
                    {
                        Single.PositiveInfinity => Vector128.ConditionalSelect(Vector128.LessThan(value, Vector128.Create(-1f)), Vector128.Create(Single.PositiveInfinity), Vector128<float>.Zero),
                        Single.NegativeInfinity => Vector128.ConditionalSelect(Vector128.LessThan(value, Vector128.Create(-1f)), Vector128<float>.Zero, Vector128.Create(Single.PositiveInfinity)),
                        _ => Vector128.Create(Single.NaN) // fractional power of a negative number: it is defined only in complex numbers, so returning NaN here
                    }
                };

                result = Vector128.ConditionalSelect(mask, res, result);
            }

            //if (value == 0f)
            //{
            //    return power switch
            //    {
            //        > 0f => 0f,
            //        < 0f => Single.PositiveInfinity,
            //        0f => 1f,
            //        _ => Single.NaN
            //    };
            //}

            mask = Vector128.Equals(value, Vector128<float>.Zero);
            if (mask.AsUInt32() != Vector128<uint>.Zero)
            {
                Vector128<float> res = power switch
                {
                    > 0f => Vector128<float>.Zero,
                    < 0f => Vector128.Create(Single.PositiveInfinity),
                    0f => VectorExtensions.OneF,
                    _ => Vector128.Create(Single.NaN)
                };

                result = Vector128.ConditionalSelect(mask, res, result);
            }

            // if (value is NaN)
            //     return power is 0f ? 1f : Single.NaN;
            mask = Vector128.IsNaN(value);
            if (mask.AsUInt32() != Vector128<uint>.Zero)
                result = Vector128.ConditionalSelect(mask, power is 0f ? Vector128<float>.One : Vector128.Create(Single.NaN), result);

            return result;
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Vector128<float> Pow_6b_ByVector128ExpLog_Specialized(this Vector128<float> value, float power) // value: [0..1], power: (0..Infinity]
        {
            Vector128<float> p = Vector128.Create(power);

            // if (value > 0f)
            //     return MathF.Exp(power * MathF.Log(value));

            //Vector128<float> mask = Vector128.GreaterThan(value, Vector128<float>.Zero);
            //Vector128<float> result = mask.AsUInt32() == Vector128<uint>.Zero
            //    ? Vector128<float>.Zero
            //    : Vector128.Exp(p * Vector128.Log(value));
            return Vector128.Exp(p * Vector128.Log(value));

            //if (mask.AsUInt32() == Vector128<uint>.AllBitsSet)
            //    return result;

            ////if (value < 0f)
            ////{
            ////    return (power % 2f) switch
            ////    {
            ////        0f => MathF.Exp(power * MathF.Log(-value)),
            ////        1f or -1f => -MathF.Exp(power * MathF.Log(-value)),
            ////        _ => power switch
            ////        {
            ////            Single.PositiveInfinity => value < -1f ? Single.PositiveInfinity : 0f,
            ////            Single.NegativeInfinity => value < -1f ? 0f : Single.PositiveInfinity,
            ////            _ => Single.NaN
            ////        }
            ////    };
            ////}

            //mask = Vector128.LessThan(value, Vector128<float>.Zero);
            //if (mask.AsUInt32() != Vector128<uint>.Zero)
            //{
            //    Vector128<float> res = (power % 2f) switch
            //    {
            //        0f => Vector128.Exp(p * Vector128.Log(-value)),
            //        1f or -1f => -Vector128.Exp(p * Vector128.Log(-value)),
            //        _ => power switch
            //        {
            //            Single.PositiveInfinity => Vector128.ConditionalSelect(Vector128.LessThan(value, Vector128.Create(-1f)), Vector128.Create(Single.PositiveInfinity), Vector128<float>.Zero),
            //            Single.NegativeInfinity => Vector128.ConditionalSelect(Vector128.LessThan(value, Vector128.Create(-1f)), Vector128<float>.Zero, Vector128.Create(Single.PositiveInfinity)),
            //            _ => Vector128.Create(Single.NaN)
            //        }
            //    };

            //    result = Vector128.ConditionalSelect(mask, res, result);
            //}

            ////if (value == 0f)
            ////{
            ////    return power switch
            ////    {
            ////        > 0f => 0f,
            ////        < 0f => Single.PositiveInfinity,
            ////        0f => 1f,
            ////        _ => Single.NaN
            ////    };
            ////}

            //mask = Vector128.Equals(value, Vector128<float>.Zero);
            //if (mask.AsUInt32() != Vector128<uint>.Zero)
            //{
            //    Vector128<float> res = power switch
            //    {
            //        > 0f => Vector128<float>.Zero,
            //        < 0f => Vector128.Create(Single.PositiveInfinity),
            //        0f => VectorExtensions.OneF,
            //        _ => Vector128.Create(Single.NaN)
            //    };

            //    result = Vector128.ConditionalSelect(mask, res, result);
            //}

            //// if (value is NaN)
            ////     return power is 0f ? 1f : Single.NaN;
            //mask = Vector128.IsNaN(value);
            //if (mask.AsUInt32() != Vector128<uint>.Zero)
            //    result = Vector128.ConditionalSelect(mask, power is 0f ? Vector128<float>.One : Vector128.Create(Single.NaN), result);

            //return result;
        }
#endif

        internal static Vector128<float> Negate(this Vector128<float> value)
        {
            Debug.Assert(Sse.IsSupported, "Expected to be called when SSE is supported.");
            return Sse.Xor(value, Vector128.Create(-0f)); // flipping sign bit
        }

        internal static Vector256<float> Negate(this Vector256<float> value)
        {
            Debug.Assert(Avx.IsSupported, "Expected to be called when AVX is supported.");
            return Avx.Xor(value, Vector256.Create(-0f)); // flip sign bit
        }

        internal static Vector256<float> IsNaN(this Vector256<float> value)
        {
            Debug.Assert(Avx.IsSupported, "Expected to be called when AVX is supported. Otherwise, add fallback paths like in the Vector128 version.");
            return Avx.Compare(value, value, FloatComparisonMode.UnorderedNonSignaling);
        }

        #endregion
    }
}
#endif