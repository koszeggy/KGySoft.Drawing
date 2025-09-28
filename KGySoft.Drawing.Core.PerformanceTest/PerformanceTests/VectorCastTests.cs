#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: VectorCastTests.cs
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
using System.Numerics;
#if NETCOREAPP3_0_OR_GREATER
using System.Runtime.CompilerServices;
#endif

using NUnit.Framework;

#endregion

namespace KGySoft.Drawing.PerformanceTests
{
    // see also https://sharplab.io/#gist:fae86d86ba19d54a91495b2eaabf2910
    [TestFixture]
    public class VectorCastTests
    {
        #region Methods

        #region Static Methods

        private static PointF AddVanilla(PointF pointF, SizeF offset) => pointF + offset;
        private static float AddVanilla1(PointF pointF, SizeF offset) => (pointF + offset).X;

        internal static PointF AddVal(PointF pointF, SizeF offset) => Vector2.Add(pointF.AsVector2Val(), offset.AsVector2Val()).AsPointFVal();
        internal static float AddVal1(PointF pointF, SizeF offset) => Vector2.Add(pointF.AsVector2Val(), offset.AsVector2Val()).X;
        internal static float AddValV(PointF pointF, Vector2 offset) => Vector2.Add(pointF.AsVector2Val(), offset).X;

#if NETCOREAPP3_0_OR_GREATER
        internal static PointF AddUnsafeRef(PointF pointF, SizeF offset)
        {
            var v = Vector2.Add(pointF.AsVector2UnsafeRef(), offset.AsVector2UnsafeRef());
            return v.AsPointFUnsafeRef();
        }

        internal static PointF AddUnsafeVal(PointF pointF, SizeF offset) => Vector2.Add(pointF.AsVector2UnsafeVal(), offset.AsVector2UnsafeVal()).AsPointFUnsafeVal();
#endif

        internal static PointF AddFixedRef(PointF pointF, SizeF offset)
        {
            var v = Vector2.Add(pointF.AsVector2FixedRef(), offset.AsVector2FixedRef());
            return v.AsPointFFixedRef();
        }

        internal static float AddFixedRef1(PointF pointF, SizeF offset) => Vector2.Add(pointF.AsVector2FixedRef(), offset.AsVector2FixedRef()).X;
        internal static float AddFixedRefV(PointF pointF, Vector2 offset) => Vector2.Add(pointF.AsVector2FixedRef(), offset).X;

        internal static Vector2 AddVector(Vector2 v1, Vector2 v2) => Vector2.Add(v1, v2);
        internal static float AddVector1(Vector2 v1, Vector2 v2) => Vector2.Add(v1, v2).X;

        #endregion

        #region Instance Methods

        [Test]
        public void CastAsVectorTest()
        {
            new PerformanceTest<PointF, Vector2>
                {
                    TestName = nameof(CastAsVectorTest),
                    Arg = new PointF(13, 42),
                    TestTime = 5000,
                    Repeat = 3
                }
                .AddCase(p => p.ToVector2(), nameof(Extensions.ToVector2))
#if NETCOREAPP3_0_OR_GREATER
                .AddCase(p => p.AsVector2UnsafeRef(), nameof(Extensions.AsVector2UnsafeRef))
                .AddCase(p => p.AsVector2UnsafeVal(), nameof(Extensions.AsVector2UnsafeVal))
#endif
                .AddCase(p => p.AsVector2FixedRef(), nameof(Extensions.AsVector2FixedRef))
                .AddCase(p => p.AsVector2Val(), nameof(Extensions.AsVector2Val))
                .DoTest()
                .DumpResults(Console.Out);

            //// Verdict: Use fixed ref in Framework, and Unsafe ref in Core. Unsafe val is bad in older core versions.

            // ==[CastAsVectorTest (.NET Framework Runtime v4.0.30319) Results]================================================
            // Test Time: 5 000 ms
            // Warming up: Yes
            // Test cases: 3
            // Repeats: 3
            // Calling GC.Collect: Yes
            // Forced CPU Affinity: No
            // Cases are sorted by fulfilled iterations (the most first)
            // --------------------------------------------------
            // 1. AsVector2FixedRef: 734 461 622 iterations in 15 000,00 ms. Adjusted for 5 000 ms: 244 820 540,67
            //   #1  244 390 054 iterations in 5 000,00 ms. Adjusted: 244 390 054,00	 <---- Worst
            //   #2  245 674 314 iterations in 5 000,00 ms. Adjusted: 245 674 314,00	 <---- Best
            //   #3  244 397 254 iterations in 5 000,00 ms. Adjusted: 244 397 254,00
            //   Worst-Best difference: 1 284 260,00 (0,53%)
            // 2. ToVector2: 613 314 838 iterations in 15 000,16 ms. Adjusted for 5 000 ms: 204 436 219,70 (-40 384 320,97 / 83,50%)
            //   #1  193 727 847 iterations in 5 000,16 ms. Adjusted: 193 721 818,38	 <---- Worst
            //   #2  208 722 496 iterations in 5 000,00 ms. Adjusted: 208 722 345,72
            //   #3  210 864 495 iterations in 5 000,00 ms. Adjusted: 210 864 495,00	 <---- Best
            //   Worst-Best difference: 17 142 676,62 (8,85%)
            // 3. AsVector2Val: 584 366 982 iterations in 15 000,00 ms. Adjusted for 5 000 ms: 194 788 994,00 (-50 031 546,67 / 79,56%)
            //   #1  195 427 408 iterations in 5 000,00 ms. Adjusted: 195 427 408,00	 <---- Best
            //   #2  194 347 282 iterations in 5 000,00 ms. Adjusted: 194 347 282,00	 <---- Worst
            //   #3  194 592 292 iterations in 5 000,00 ms. Adjusted: 194 592 292,00
            //   Worst-Best difference: 1 080 126,00 (0,56%)

            // ==[CastAsVectorTest (.NET Core 5.0.17) Results]================================================
            // Test Time: 5 000 ms
            // Warming up: Yes
            // Test cases: 5
            // Repeats: 3
            // Calling GC.Collect: Yes
            // Forced CPU Affinity: No
            // Cases are sorted by fulfilled iterations (the most first)
            // --------------------------------------------------
            // 1. AsVector2UnsafeRef: 892 439 734 iterations in 15 000,00 ms. Adjusted for 5 000 ms: 297 479 869,68
            //   #1  297 727 285 iterations in 5 000,00 ms. Adjusted: 297 727 237,36	 <---- Best
            //   #2  297 668 744 iterations in 5 000,00 ms. Adjusted: 297 668 702,33
            //   #3  297 043 705 iterations in 5 000,00 ms. Adjusted: 297 043 669,35	 <---- Worst
            //   Worst-Best difference: 683 568,01 (0,23%)
            // 2. AsVector2FixedRef: 891 778 420 iterations in 15 000,00 ms. Adjusted for 5 000 ms: 297 259 423,79 (-220 445,89 / 99,93%)
            //   #1  298 110 930 iterations in 5 000,00 ms. Adjusted: 298 110 876,34	 <---- Best
            //   #2  296 502 083 iterations in 5 000,00 ms. Adjusted: 296 502 029,63	 <---- Worst
            //   #3  297 165 407 iterations in 5 000,00 ms. Adjusted: 297 165 365,40
            //   Worst-Best difference: 1 608 846,71 (0,54%)
            // 3. ToVector2: 729 560 790 iterations in 15 000,02 ms. Adjusted for 5 000 ms: 243 186 576,32 (-54 293 293,36 / 81,75%)
            //   #1  235 858 668 iterations in 5 000,02 ms. Adjusted: 235 857 705,70	 <---- Worst
            //   #2  242 271 808 iterations in 5 000,00 ms. Adjusted: 242 271 759,55
            //   #3  251 430 314 iterations in 5 000,00 ms. Adjusted: 251 430 263,71	 <---- Best
            //   Worst-Best difference: 15 572 558,01 (6,60%)
            // 4. AsVector2UnsafeVal: 671 787 843 iterations in 15 000,00 ms. Adjusted for 5 000 ms: 223 929 248,15 (-73 550 621,53 / 75,28%)
            //   #1  223 252 511 iterations in 5 000,00 ms. Adjusted: 223 252 484,21	 <---- Worst
            //   #2  224 295 921 iterations in 5 000,00 ms. Adjusted: 224 295 894,08	 <---- Best
            //   #3  224 239 411 iterations in 5 000,00 ms. Adjusted: 224 239 366,15
            //   Worst-Best difference: 1 043 409,87 (0,47%)
            // 5. AsVector2Val: 669 385 965 iterations in 15 000,00 ms. Adjusted for 5 000 ms: 223 128 608,90 (-74 351 260,78 / 75,01%)
            //   #1  222 658 674 iterations in 5 000,00 ms. Adjusted: 222 658 625,02	 <---- Worst
            //   #2  223 004 878 iterations in 5 000,00 ms. Adjusted: 223 004 820,02
            //   #3  223 722 413 iterations in 5 000,00 ms. Adjusted: 223 722 381,68	 <---- Best
            //   Worst-Best difference: 1 063 756,66 (0,48%)

            // ==[CastAsVectorTest (.NET Core 10.0.0-rc.1.25451.107) Results]================================================
            // Test Time: 5 000 ms
            // Warming up: Yes
            // Test cases: 5
            // Repeats: 3
            // Calling GC.Collect: Yes
            // Forced CPU Affinity: No
            // Cases are sorted by fulfilled iterations (the most first)
            // --------------------------------------------------
            // 1. AsVector2UnsafeVal: 1 246 551 734 iterations in 15 000,00 ms. Adjusted for 5 000 ms: 415 517 244,67
            //   #1  415 743 860 iterations in 5 000,00 ms. Adjusted: 415 743 860,00
            //   #2  414 874 860 iterations in 5 000,00 ms. Adjusted: 414 874 860,00	 <---- Worst
            //   #3  415 933 014 iterations in 5 000,00 ms. Adjusted: 415 933 014,00	 <---- Best
            //   Worst-Best difference: 1 058 154,00 (0,26%)
            // 2. AsVector2UnsafeRef: 1 240 290 774 iterations in 15 000,01 ms. Adjusted for 5 000 ms: 413 430 031,96 (-2 087 212,71 / 99,50%)
            //   #1  413 485 661 iterations in 5 000,01 ms. Adjusted: 413 484 982,88
            //   #2  414 222 004 iterations in 5 000,00 ms. Adjusted: 414 222 004,00	 <---- Best
            //   #3  412 583 109 iterations in 5 000,00 ms. Adjusted: 412 583 109,00	 <---- Worst
            //   Worst-Best difference: 1 638 895,00 (0,40%)
            // 3. AsVector2Val: 1 237 219 000 iterations in 15 000,00 ms. Adjusted for 5 000 ms: 412 406 330,58 (-3 110 914,09 / 99,25%)
            //   #1  411 958 951 iterations in 5 000,00 ms. Adjusted: 411 958 951,00	 <---- Worst
            //   #2  413 010 593 iterations in 5 000,00 ms. Adjusted: 413 010 584,74	 <---- Best
            //   #3  412 249 456 iterations in 5 000,00 ms. Adjusted: 412 249 456,00
            //   Worst-Best difference: 1 051 633,74 (0,26%)
            // 4. AsVector2FixedRef: 1 234 598 097 iterations in 15 000,00 ms. Adjusted for 5 000 ms: 411 532 699,00 (-3 984 545,67 / 99,04%)
            //   #1  412 108 130 iterations in 5 000,00 ms. Adjusted: 412 108 130,00
            //   #2  412 608 571 iterations in 5 000,00 ms. Adjusted: 412 608 571,00	 <---- Best
            //   #3  409 881 396 iterations in 5 000,00 ms. Adjusted: 409 881 396,00	 <---- Worst
            //   Worst-Best difference: 2 727 175,00 (0,67%)
            // 5. ToVector2: 1 121 353 196 iterations in 15 000,00 ms. Adjusted for 5 000 ms: 373 784 396,26 (-41 732 848,40 / 89,96%)
            //   #1  362 909 206 iterations in 5 000,00 ms. Adjusted: 362 909 206,00
            //   #2  360 624 994 iterations in 5 000,00 ms. Adjusted: 360 624 986,79	 <---- Worst
            //   #3  397 818 996 iterations in 5 000,00 ms. Adjusted: 397 818 996,00	 <---- Best
            //   Worst-Best difference: 37 194 009,21 (10,31%)
        }

        [Test]
        public void CalculateWithCastsTest()
        {
            new PerformanceTest
                {
                    TestName = nameof(CalculateWithCastsTest),
                    TestTime = 5000,
                    Repeat = 3
                }
                .AddCase(() => AddVanilla(default, default), nameof(AddVanilla))
                .AddCase(() => AddVal(default, default), nameof(AddVal))
#if NETCOREAPP3_0_OR_GREATER
                .AddCase(() => AddUnsafeRef(default, default), nameof(AddUnsafeRef))
                .AddCase(() => AddUnsafeVal(default, default), nameof(AddUnsafeVal))
#endif
                .AddCase(() => AddFixedRef(default, default), nameof(AddFixedRef))
                .AddCase(() => AddVector(default, default), nameof(AddVector))
                .DoTest()
                .DumpResults(Console.Out);

            //// Verdict: In .NET Framework stay at vanilla if there is only one vector operation between back and forth conversions; in .NET Core everything but vanilla is practically the same.

            // ==[CalculateWithCastsTest (.NET Framework Runtime v4.0.30319) Results]================================================
            // Test Time: 5 000 ms
            // Warming up: Yes
            // Test cases: 4
            // Repeats: 3
            // Calling GC.Collect: Yes
            // Forced CPU Affinity: No
            // Cases are sorted by fulfilled iterations (the most first)
            // --------------------------------------------------
            // 1. AddVector: 672 625 630 iterations in 15 000,00 ms. Adjusted for 5 000 ms: 224 208 543,33
            //   #1  223 502 753 iterations in 5 000,00 ms. Adjusted: 223 502 753,00	 <---- Worst
            //   #2  224 511 388 iterations in 5 000,00 ms. Adjusted: 224 511 388,00
            //   #3  224 611 489 iterations in 5 000,00 ms. Adjusted: 224 611 489,00	 <---- Best
            //   Worst-Best difference: 1 108 736,00 (0,50%)
            // 2. AddVanilla: 520 199 477 iterations in 15 000,17 ms. Adjusted for 5 000 ms: 173 397 841,18 (-50 810 702,15 / 77,34%)
            //   #1  170 714 648 iterations in 5 000,17 ms. Adjusted: 170 708 843,90	 <---- Worst
            //   #2  173 661 185 iterations in 5 000,00 ms. Adjusted: 173 661 035,65
            //   #3  175 823 644 iterations in 5 000,00 ms. Adjusted: 175 823 644,00	 <---- Best
            //   Worst-Best difference: 5 114 800,10 (3,00%)
            // 3. AddFixedRef: 511 063 598 iterations in 15 000,00 ms. Adjusted for 5 000 ms: 170 354 524,72 (-53 854 018,62 / 75,98%)
            //   #1  170 388 216 iterations in 5 000,00 ms. Adjusted: 170 388 212,59
            //   #2  170 449 067 iterations in 5 000,00 ms. Adjusted: 170 449 056,77	 <---- Best
            //   #3  170 226 315 iterations in 5 000,00 ms. Adjusted: 170 226 304,79	 <---- Worst
            //   Worst-Best difference: 222 751,99 (0,13%)
            // 4. AddVal: 374 051 058 iterations in 15 000,00 ms. Adjusted for 5 000 ms: 124 683 685,17 (-99 524 858,17 / 55,61%)
            //   #1  124 761 529 iterations in 5 000,00 ms. Adjusted: 124 761 529,00
            //   #2  125 108 087 iterations in 5 000,00 ms. Adjusted: 125 108 084,50	 <---- Best
            //   #3  124 181 442 iterations in 5 000,00 ms. Adjusted: 124 181 442,00	 <---- Worst
            //   Worst-Best difference: 926 642,50 (0,75%)

            // ==[CalculateWithCastsTest (.NET Core 10.0.0-rc.1.25451.107) Results]================================================
            // Test Time: 5 000 ms
            // Warming up: Yes
            // Test cases: 5
            // Repeats: 3
            // Calling GC.Collect: Yes
            // Forced CPU Affinity: No
            // Cases are sorted by fulfilled iterations (the most first)
            // --------------------------------------------------
            // 1. AddFixedRef: 1 225 436 668 iterations in 15 000,00 ms. Adjusted for 5 000 ms: 408 478 886,61
            //   #1  407 987 740 iterations in 5 000,00 ms. Adjusted: 407 987 740,00	 <---- Worst
            //   #2  409 029 708 iterations in 5 000,00 ms. Adjusted: 409 029 708,00	 <---- Best
            //   #3  408 419 220 iterations in 5 000,00 ms. Adjusted: 408 419 211,83
            //   Worst-Best difference: 1 041 968,00 (0,26%)
            // 2. AddUnsafeRef: 1 224 483 820 iterations in 15 000,00 ms. Adjusted for 5 000 ms: 408 161 273,33 (-317 613,28 / 99,92%)
            //   #1  408 808 655 iterations in 5 000,00 ms. Adjusted: 408 808 655,00	 <---- Best
            //   #2  407 394 689 iterations in 5 000,00 ms. Adjusted: 407 394 689,00	 <---- Worst
            //   #3  408 280 476 iterations in 5 000,00 ms. Adjusted: 408 280 476,00
            //   Worst-Best difference: 1 413 966,00 (0,35%)
            // 3. AddVal: 1 224 375 569 iterations in 15 000,00 ms. Adjusted for 5 000 ms: 408 125 189,67 (-353 696,94 / 99,91%)
            //   #1  408 493 674 iterations in 5 000,00 ms. Adjusted: 408 493 674,00	 <---- Best
            //   #2  407 931 813 iterations in 5 000,00 ms. Adjusted: 407 931 813,00	 <---- Worst
            //   #3  407 950 082 iterations in 5 000,00 ms. Adjusted: 407 950 082,00
            //   Worst-Best difference: 561 861,00 (0,14%)
            // 4. AddVector: 1 224 365 572 iterations in 15 000,00 ms. Adjusted for 5 000 ms: 408 121 857,33 (-357 029,28 / 99,91%)
            //   #1  408 118 772 iterations in 5 000,00 ms. Adjusted: 408 118 772,00
            //   #2  408 449 905 iterations in 5 000,00 ms. Adjusted: 408 449 905,00	 <---- Best
            //   #3  407 796 895 iterations in 5 000,00 ms. Adjusted: 407 796 895,00	 <---- Worst
            //   Worst-Best difference: 653 010,00 (0,16%)
            // 5. AddVanilla: 1 076 498 162 iterations in 15 000,00 ms. Adjusted for 5 000 ms: 358 832 720,67 (-49 646 165,94 / 87,85%)
            //   #1  363 175 549 iterations in 5 000,00 ms. Adjusted: 363 175 549,00
            //   #2  334 666 127 iterations in 5 000,00 ms. Adjusted: 334 666 127,00	 <---- Worst
            //   #3  378 656 486 iterations in 5 000,00 ms. Adjusted: 378 656 486,00	 <---- Best
            //   Worst-Best difference: 43 990 359,00 (13,14%)
        }

        [Test]
        public void CalculateWith1DirectionCastTest()
        {
            new PerformanceTest<float>
                {
                    TestName = nameof(CalculateWith1DirectionCastTest),
                    TestTime = 5000,
                    Repeat = 3
                }
                .AddCase(() => AddVanilla1(default, default), nameof(AddVanilla))
                .AddCase(() => AddVal1(default, default), nameof(AddVal))
                .AddCase(() => AddFixedRef1(default, default), nameof(AddFixedRef))
                .AddCase(() => AddVector1(default, default), nameof(AddVector))
                .DoTest()
                .DumpResults(Console.Out);

            //// Verdict: If there is no conversion back, fixed ref gets outperform vanilla also in .NET Framework (just a little), even if there is only one vector operation.

            // ==[CalculateWith1DirectionCastTest (.NET Framework Runtime v4.0.30319) Results]================================================
            // Test Time: 5 000 ms
            // Warming up: Yes
            // Test cases: 4
            // Repeats: 3
            // Calling GC.Collect: Yes
            // Forced CPU Affinity: No
            // Cases are sorted by fulfilled iterations (the most first)
            // --------------------------------------------------
            // 1. AddVector: 654 114 820 iterations in 15 000,00 ms. Adjusted for 5 000 ms: 218 038 273,33
            //   #1  217 393 399 iterations in 5 000,00 ms. Adjusted: 217 393 399,00	 <---- Worst
            //   #2  218 927 484 iterations in 5 000,00 ms. Adjusted: 218 927 484,00	 <---- Best
            //   #3  217 793 937 iterations in 5 000,00 ms. Adjusted: 217 793 937,00
            //   Worst-Best difference: 1 534 085,00 (0,71%)
            // 2. AddFixedRef: 515 166 288 iterations in 15 000,00 ms. Adjusted for 5 000 ms: 171 722 096,00 (-46 316 177,33 / 78,76%)
            //   #1  171 609 106 iterations in 5 000,00 ms. Adjusted: 171 609 106,00	 <---- Worst
            //   #2  171 784 668 iterations in 5 000,00 ms. Adjusted: 171 784 668,00	 <---- Best
            //   #3  171 772 514 iterations in 5 000,00 ms. Adjusted: 171 772 514,00
            //   Worst-Best difference: 175 562,00 (0,10%)
            // 3. AddVanilla: 505 372 429 iterations in 15 000,98 ms. Adjusted for 5 000 ms: 168 446 399,38 (-49 591 873,96 / 77,26%)
            //   #1  166 539 168 iterations in 5 000,21 ms. Adjusted: 166 532 200,29	 <---- Worst
            //   #2  169 772 639 iterations in 5 000,77 ms. Adjusted: 169 746 375,84	 <---- Best
            //   #3  169 060 622 iterations in 5 000,00 ms. Adjusted: 169 060 622,00
            //   Worst-Best difference: 3 214 175,55 (1,93%)
            // 4. AddVal: 432 768 444 iterations in 15 000,00 ms. Adjusted for 5 000 ms: 144 256 147,06 (-73 782 126,28 / 66,16%)
            //   #1  145 300 146 iterations in 5 000,00 ms. Adjusted: 145 300 146,00
            //   #2  145 873 036 iterations in 5 000,00 ms. Adjusted: 145 873 036,00	 <---- Best
            //   #3  141 595 262 iterations in 5 000,00 ms. Adjusted: 141 595 259,17	 <---- Worst
            //   Worst-Best difference: 4 277 776,83 (3,02%)
        }

        [Test]
        public void CalculateWith1VectorArgTest()
        {
            new PerformanceTest<float>
                {
                    TestName = nameof(CalculateWith1VectorArgTest),
                    TestTime = 5000,
                    Repeat = 3
                }
                .AddCase(() => AddVanilla1(default, default), nameof(AddVanilla))
                .AddCase(() => AddValV(default, default), nameof(AddVal))
                .AddCase(() => AddFixedRefV(default, default), nameof(AddFixedRef))
                .AddCase(() => AddVector1(default, default), nameof(AddVector))
                .DoTest()
                .DumpResults(Console.Out);

            //// Verdict: If one of the arguments is already a vector, then it is worth using vectors even for a single operation, even in .NET Framework.

            // ==[CalculateWith1VectorArgTest (.NET Framework Runtime v4.0.30319) Results]================================================
            // Test Time: 5 000 ms
            // Warming up: Yes
            // Test cases: 4
            // Repeats: 3
            // Calling GC.Collect: Yes
            // Forced CPU Affinity: No
            // Cases are sorted by fulfilled iterations (the most first)
            // --------------------------------------------------
            // 1. AddFixedRef: 655 861 752 iterations in 15 000,00 ms. Adjusted for 5 000 ms: 218 620 581,09
            //   #1  218 715 696 iterations in 5 000,00 ms. Adjusted: 218 715 696,00	 <---- Best
            //   #2  218 595 443 iterations in 5 000,00 ms. Adjusted: 218 595 434,26
            //   #3  218 550 613 iterations in 5 000,00 ms. Adjusted: 218 550 613,00	 <---- Worst
            //   Worst-Best difference: 165 083,00 (0,08%)
            // 2. AddVector: 652 819 847 iterations in 15 000,00 ms. Adjusted for 5 000 ms: 217 606 612,77 (-1 013 968,32 / 99,54%)
            //   #1  217 511 590 iterations in 5 000,00 ms. Adjusted: 217 511 581,30
            //   #2  218 099 255 iterations in 5 000,00 ms. Adjusted: 218 099 255,00	 <---- Best
            //   #3  217 209 002 iterations in 5 000,00 ms. Adjusted: 217 209 002,00	 <---- Worst
            //   Worst-Best difference: 890 253,00 (0,41%)
            // 3. AddVal: 620 706 976 iterations in 15 000,00 ms. Adjusted for 5 000 ms: 206 902 323,96 (-11 718 257,13 / 94,64%)
            //   #1  207 476 981 iterations in 5 000,00 ms. Adjusted: 207 476 981,00	 <---- Best
            //   #2  206 691 723 iterations in 5 000,00 ms. Adjusted: 206 691 718,87
            //   #3  206 538 272 iterations in 5 000,00 ms. Adjusted: 206 538 272,00	 <---- Worst
            //   Worst-Best difference: 938 709,00 (0,45%)
            // 4. AddVanilla: 512 620 885 iterations in 15 001,82 ms. Adjusted for 5 000 ms: 170 853 491,92 (-47 767 089,16 / 78,15%)
            //   #1  168 195 681 iterations in 5 000,17 ms. Adjusted: 168 189 986,09
            //   #2  166 174 894 iterations in 5 001,65 ms. Adjusted: 166 120 193,94	 <---- Worst
            //   #3  178 250 310 iterations in 5 000,00 ms. Adjusted: 178 250 295,74	 <---- Best
            //   Worst-Best difference: 12 130 101,80 (7,30%)
        }

        #endregion

        #endregion
    }

    internal static partial class Extensions
    {
        #region Methods

#if NETCOREAPP3_0_OR_GREATER
        internal static ref PointF AsPointFUnsafeRef(this ref Vector2 vector) => ref Unsafe.As<Vector2, PointF>(ref vector);
        internal static ref Vector2 AsVector2UnsafeRef(this ref PointF point) => ref Unsafe.As<PointF, Vector2>(ref point);
        internal static ref Vector2 AsVector2UnsafeRef(this ref SizeF size) => ref Unsafe.As<SizeF, Vector2>(ref size);
        internal static PointF AsPointFUnsafeVal(this Vector2 vector) => Unsafe.As<Vector2, PointF>(ref vector);
        internal static Vector2 AsVector2UnsafeVal(this PointF point) => Unsafe.As<PointF, Vector2>(ref point);
        internal static Vector2 AsVector2UnsafeVal(this SizeF size) => Unsafe.As<SizeF, Vector2>(ref size);
#endif

        internal static unsafe ref Vector2 AsVector2FixedRef(this ref PointF point)
        {
            fixed (PointF* p = &point)
                return ref *(Vector2*)p;
        }

        internal static unsafe ref Vector2 AsVector2FixedRef(this ref SizeF point)
        {
            fixed (SizeF* p = &point)
                return ref *(Vector2*)p;
        }

        internal static unsafe ref PointF AsPointFFixedRef(this ref Vector2 point)
        {
            fixed (Vector2* p = &point)
                return ref *(PointF*)p;
        }

        internal static unsafe PointF AsPointFVal(this Vector2 vector) => *(PointF*)&vector;
        internal static unsafe Vector2 AsVector2Val(this PointF point) => *(Vector2*)&point;
        internal static unsafe Vector2 AsVector2Val(this SizeF size) => *(Vector2*)&size;


        internal static PointF ToPointF(this Vector2 vector) => new PointF(vector.X, vector.Y);
        internal static Vector2 ToVector2(this PointF point) => new Vector2(point.X, point.Y);
        internal static Vector2 ToVector2(this SizeF size) => new Vector2(size.Width, size.Height);


        #endregion
    }
}
#endif