#if NETCOREAPP3_0_OR_GREATER
#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: Conversion_Color32_PColor32.cs
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
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

using KGySoft.Drawing.Imaging;

using NUnit.Framework;

#endregion

namespace KGySoft.Drawing.PerformanceTests
{
    [TestFixture]
    public class Conversion_Color32_PColor32
    {
        #region Methods

        [Test]
        public void Color32ToPColor32Test()
        {
            var testColor32 = new Color32(128, 255, 128, 64);
            //var testColor32 = new Color32(254, 255, 128, 64);
            //var testColor32 = new Color32(1, 255, 128, 64);
            var expected = testColor32.ToPColor32();
            Console.WriteLine($"{"Expected color:",-40} {expected}");

            void DoAssert(Expression<Func<PColor32>> e)
            {
                var m = (MethodCallExpression)e.Body;
                PColor32 actual = e.Compile().Invoke();
                Console.WriteLine($"{$"{m.Method.Name}:",-40} {actual}");
                Assert.IsTrue(expected.TolerantEquals(actual, 1), $"{expected} vs. {actual}");
            }

            DoAssert(() => testColor32.ToPColor32_0_Div());
            DoAssert(() => testColor32.ToPColor32_1_DivVector3());
            DoAssert(() => testColor32.ToPColor32_2_DivVectorTSpan());
            DoAssert(() => testColor32.ToPColor32_3_DivIntrinsicsSse41());
            DoAssert(() => testColor32.ToPColor32_4_DivIntrinsicsSsse3());
            DoAssert(() => testColor32.ToPColor32_5_DivIntrinsicsSse2());
            DoAssert(() => testColor32.ToPColor32_6_Shift());
            DoAssert(() => testColor32.ToPColor32_7_ShiftIntrinsicsSse41());

            new PerformanceTest<PColor32> { TestName = "Color32 -> PColor32", TestTime = 5000, Iterations = 10_000_000, Repeat = 3 }
                .AddCase(() => testColor32.ToPColor32_0_Div(), nameof(Extensions.ToPColor32_0_Div))
                .AddCase(() => testColor32.ToPColor32_1_DivVector3(), nameof(Extensions.ToPColor32_1_DivVector3))
                .AddCase(() => testColor32.ToPColor32_2_DivVectorTSpan(), nameof(Extensions.ToPColor32_2_DivVectorTSpan))
                .AddCase(() => testColor32.ToPColor32_3_DivIntrinsicsSse41(), nameof(Extensions.ToPColor32_3_DivIntrinsicsSse41))
                .AddCase(() => testColor32.ToPColor32_4_DivIntrinsicsSsse3(), nameof(Extensions.ToPColor32_4_DivIntrinsicsSsse3))
                .AddCase(() => testColor32.ToPColor32_5_DivIntrinsicsSse2(), nameof(Extensions.ToPColor32_5_DivIntrinsicsSse2))
                .AddCase(() => testColor32.ToPColor32_6_Shift(), nameof(Extensions.ToPColor32_6_Shift))
                .AddCase(() => testColor32.ToPColor32_7_ShiftIntrinsicsSse41(), nameof(Extensions.ToPColor32_7_ShiftIntrinsicsSse41))
                .DoTest()
                .DumpResults(Console.Out);

            // Verdict: using Div because of its better accuracy. Fallback div is slower than shift but not in the other direction

            // 1. ToPColor32_7_ShiftIntrinsicsSse41: average time: 34,68 ms
            //   #1          34,80 ms	 <---- Worst
            //   #2          34,64 ms
            //   #3          34,59 ms	 <---- Best
            //   Worst-Best difference: 0,21 ms (0,61%)
            // 2. ToPColor32_3_DivIntrinsicsSse41: average time: 35,03 ms (+0,35 ms / 101,01%)
            //   #1          35,33 ms
            //   #2          34,41 ms	 <---- Best
            //   #3          35,36 ms	 <---- Worst
            //   Worst-Best difference: 0,95 ms (2,77%)
            // 3. ToPColor32_4_DivIntrinsicsSsse3: average time: 43,35 ms (+8,66 ms / 124,98%)
            //   #1          43,37 ms
            //   #2          43,40 ms	 <---- Worst
            //   #3          43,26 ms	 <---- Best
            //   Worst-Best difference: 0,14 ms (0,32%)
            // 4. ToPColor32_6_Shift: average time: 71,58 ms (+36,90 ms / 206,41%)
            //   #1          70,97 ms	 <---- Best
            //   #2          71,28 ms
            //   #3          72,50 ms	 <---- Worst
            //   Worst-Best difference: 1,53 ms (2,15%)
            // 5. ToPColor32_5_DivIntrinsicsSse2: average time: 76,84 ms (+42,16 ms / 221,55%)
            //   #1          76,98 ms	 <---- Worst
            //   #2          76,61 ms	 <---- Best
            //   #3          76,92 ms
            //   Worst-Best difference: 0,37 ms (0,48%)
            // 6. ToPColor32_0_Div: average time: 83,52 ms (+48,84 ms / 240,81%)
            //   #1          84,64 ms	 <---- Worst
            //   #2          84,35 ms
            //   #3          81,57 ms	 <---- Best
            //   Worst-Best difference: 3,07 ms (3,76%)
            // 7. ToPColor32_1_DivVector3: average time: 88,12 ms (+53,44 ms / 254,10%)
            //   #1          88,57 ms	 <---- Worst
            //   #2          87,75 ms	 <---- Best
            //   #3          88,05 ms
            //   Worst-Best difference: 0,82 ms (0,93%)
            // 8. ToPColor32_2_DivVectorTSpan: average time: 542,02 ms (+507,34 ms / 1 562,89%)
            //   #1         539,72 ms	 <---- Best
            //   #2         542,92 ms
            //   #3         543,42 ms	 <---- Worst
            //   Worst-Best difference: 3,71 ms (0,69%)
        }

        [Test]
        public void PColor32ToColor32Test()
        {
            var testPColor32 = new Color32(128, 255, 128, 64).ToPColor32();
            //var testPColor32 = new Color32(254, 255, 128, 64).ToPColor32();
            //var testPColor32 = new Color32(1, 255, 128, 64).ToPColor32();
            var expected = testPColor32.ToColor32();
            Console.WriteLine($"{"Expected color:",-40} {expected}");

            void DoAssert(Expression<Func<Color32>> e)
            {
                var m = (MethodCallExpression)e.Body;
                Color32 actual = e.Compile().Invoke();
                Console.WriteLine($"{$"{m.Method.Name}:",-40} {actual}");
                Assert.IsTrue(expected.TolerantEquals(actual, 1, 0), $"{expected} vs. {actual}");
            }

            DoAssert(() => testPColor32.ToColor32_0_Div());
            DoAssert(() => testPColor32.ToColor32_1_DivVector3());
            DoAssert(() => testPColor32.ToColor32_2_IntrinsicsDivSse41());
            DoAssert(() => testPColor32.ToColor32_3_IntrinsicsDivSsse3());
            DoAssert(() => testPColor32.ToColor32_4_IntrinsicsDivSse2());
            DoAssert(() => testPColor32.ToColor32_5_Shift());
            DoAssert(() => testPColor32.ToColor32_6_ShiftIntrinsicsSse41());
            DoAssert(() => testPColor32.ToColor32_7_ShiftIntrinsicsSsse3());
            DoAssert(() => testPColor32.ToColor32_8_ShiftIntrinsicsSse2());

            new PerformanceTest<Color32> { TestName = "PColor32 -> Color32", TestTime = 5000, Iterations = 10_000_000, Repeat = 3 }
                .AddCase(() => testPColor32.ToColor32_0_Div(), nameof(Extensions.ToColor32_0_Div))
                .AddCase(() => testPColor32.ToColor32_1_DivVector3(), nameof(Extensions.ToColor32_1_DivVector3))
                .AddCase(() => testPColor32.ToColor32_2_IntrinsicsDivSse41(), nameof(Extensions.ToColor32_2_IntrinsicsDivSse41))
                .AddCase(() => testPColor32.ToColor32_3_IntrinsicsDivSsse3(), nameof(Extensions.ToColor32_3_IntrinsicsDivSsse3))
                .AddCase(() => testPColor32.ToColor32_4_IntrinsicsDivSse2(), nameof(Extensions.ToColor32_4_IntrinsicsDivSse2))
                .AddCase(() => testPColor32.ToColor32_5_Shift(), nameof(Extensions.ToColor32_5_Shift))
                .AddCase(() => testPColor32.ToColor32_6_ShiftIntrinsicsSse41(), nameof(Extensions.ToColor32_6_ShiftIntrinsicsSse41))
                .AddCase(() => testPColor32.ToColor32_8_ShiftIntrinsicsSse2(), nameof(Extensions.ToColor32_8_ShiftIntrinsicsSse2))
                .DoTest()
                .DumpResults(Console.Out);

            // Verdict: using Div because of its better performance and accuracy

            // 1. ToColor32_2_IntrinsicsDivSse41: average time: 33,16 ms
            //   #1          33,18 ms
            //   #2          33,02 ms	 <---- Best
            //   #3          33,27 ms	 <---- Worst
            //   Worst-Best difference: 0,25 ms (0,76%)
            // 2. ToColor32_6_ShiftIntrinsicsSse41: average time: 37,32 ms (+4,16 ms / 112,55%)
            //   #1          37,75 ms	 <---- Worst
            //   #2          37,01 ms	 <---- Best
            //   #3          37,19 ms
            //   Worst-Best difference: 0,74 ms (2,00%)
            // 3. ToColor32_3_IntrinsicsDivSsse3: average time: 43,30 ms (+10,14 ms / 130,59%)
            //   #1          43,24 ms
            //   #2          43,98 ms	 <---- Worst
            //   #3          42,67 ms	 <---- Best
            //   Worst-Best difference: 1,30 ms (3,06%)
            // 4. ToColor32_4_IntrinsicsDivSse2: average time: 76,83 ms (+43,68 ms / 231,73%)
            //   #1          76,82 ms
            //   #2          76,34 ms	 <---- Best
            //   #3          77,35 ms	 <---- Worst
            //   Worst-Best difference: 1,00 ms (1,31%)
            // 5. ToColor32_8_ShiftIntrinsicsSse2: average time: 80,18 ms (+47,03 ms / 241,84%)
            //   #1          80,39 ms
            //   #2          80,89 ms	 <---- Worst
            //   #3          79,28 ms	 <---- Best
            //   Worst-Best difference: 1,61 ms (2,04%)
            // 6. ToColor32_1_DivVector3: average time: 88,36 ms (+55,20 ms / 266,49%)
            //   #1          87,11 ms
            //   #2          90,96 ms	 <---- Worst
            //   #3          87,01 ms	 <---- Best
            //   Worst-Best difference: 3,96 ms (4,55%)
            // 7. ToColor32_0_Div: average time: 97,21 ms (+64,06 ms / 293,20%)
            //   #1          93,52 ms	 <---- Best
            //   #2         100,64 ms	 <---- Worst
            //   #3          97,48 ms
            //   Worst-Best difference: 7,12 ms (7,61%)
            // 8. ToColor32_5_Shift: average time: 259,12 ms (+225,96 ms / 781,51%)
            //   #1         259,89 ms	 <---- Worst
            //   #2         257,95 ms	 <---- Best
            //   #3         259,51 ms
            //   Worst-Best difference: 1,94 ms (0,75%)
        }

        #endregion
    }

    internal static partial class Extensions
    {
        #region Properties

#if NET5_0_OR_GREATER
        private static Vector128<byte> PackLowBytesMask => Vector128.Create(0, 4, 8, 12, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF);
#elif NETCOREAPP3_0_OR_GREATER
        private static Vector128<byte> PackLowBytesMask { get; } = Vector128.Create(0, 4, 8, 12, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF);
#endif

        #endregion

        #region Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static PColor32 ToPColor32_0_Div(this Color32 c) => c.A switch
        {
            255 => new PColor32(c.Value),
            0 => default,
            _ => new PColor32(c.A,
                (byte)((uint)c.R * c.A / Byte.MaxValue),
                (byte)((uint)c.G * c.A / Byte.MaxValue),
                (byte)((uint)c.B * c.A / Byte.MaxValue)),
        };

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static PColor32 ToPColor32_1_DivVector3(this Color32 c)
        {
            switch (c.A)
            {
                case 255:
                    return new PColor32(c.Value);
                case 0:
                    return default;
                default:
                    Vector3 v = new Vector3(c.R, c.G, c.B) * c.A / 255f;
                    return new PColor32(c.A, (byte)v.X, (byte)v.Y, (byte)v.Z);
            }
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static PColor32 ToPColor32_2_DivVectorTSpan(this Color32 c)
        {
            switch (c.A)
            {
                case 255:
                    return new PColor32(c.Value);
                case 0:
                    return default;
                default:
                    Span<ushort> span = stackalloc ushort[Vector<ushort>.Count];
                    span[0] = c.R;
                    span[1] = c.G;
                    span[2] = c.B;
                    Vector<ushort> v = new Vector<ushort>(span) * c.A / new Vector<ushort>(255);
                    return new PColor32(c.A, (byte)v[0], (byte)v[1], (byte)v[2]);
            }
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static PColor32 ToPColor32_3_DivIntrinsicsSse41(this Color32 c)
        {
            switch (c.A)
            {
                case 255:
                    return new PColor32(c.Value);
                case 0:
                    return default;
                default:
                    if (Sse41.IsSupported)
                    {
                        Vector128<float> bgraF = Sse2.ConvertToVector128Single(Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(c.Value).AsByte()));

                        bgraF = Sse.Multiply(bgraF, Sse2.ConvertToVector128Single(Sse41.ConvertToVector128Int32(Vector128.Create(c.A))));

                        bgraF = Sse.Multiply(bgraF, Vector128.Create(1f / 255f));

                        Vector128<int> bgraI32 = Sse2.ConvertToVector128Int32(bgraF);
                        return new PColor32(Ssse3.Shuffle(bgraI32.AsByte().WithElement(12, c.A),
                            PackLowBytesMask).AsUInt32().ToScalar());
                    }

                    return new PColor32(c.A,
                        (byte)(c.R * c.A / Byte.MaxValue),
                        (byte)(c.G * c.A / Byte.MaxValue),
                        (byte)(c.B * c.A / Byte.MaxValue));
            }
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static PColor32 ToPColor32_4_DivIntrinsicsSsse3(this Color32 c)
        {
            switch (c.A)
            {
                case 255:
                    return new PColor32(c.Value);
                case 0:
                    return default;
                default:
                    if (Ssse3.IsSupported)
                    {
                        Vector128<float> bgraF = Sse2.ConvertToVector128Single(Vector128.Create(c.B, c.G, c.R, default));

                        // cannot just use Create((int)c.A), see https://github.com/dotnet/runtime/issues/83387
                        int a = c.A;
                        bgraF = Sse.Multiply(bgraF, Sse2.ConvertToVector128Single(Vector128.Create(a)));
                        bgraF = Sse.Multiply(bgraF, Vector128.Create(1f / 255f));

                        Vector128<int> bgraI32 = Sse2.ConvertToVector128Int32(bgraF);
                        return new PColor32(Ssse3.Shuffle(bgraI32.AsByte().WithElement(12, c.A),
                            PackLowBytesMask).AsUInt32().ToScalar());
                    }

                    return new PColor32(c.A,
                        (byte)(c.R * c.A / Byte.MaxValue),
                        (byte)(c.G * c.A / Byte.MaxValue),
                        (byte)(c.B * c.A / Byte.MaxValue));
            }
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static PColor32 ToPColor32_5_DivIntrinsicsSse2(this Color32 c)
        {
            switch (c.A)
            {
                case 255:
                    return new PColor32(c.Value);
                case 0:
                    return default;
                default:
                    if (Sse2.IsSupported)
                    {
                        Vector128<float> bgraF = Sse2.ConvertToVector128Single(Vector128.Create(c.B, c.G, c.R, default));

                        // cannot just use Create((int)c.A), see https://github.com/dotnet/runtime/issues/83387
                        int a = c.A;
                        bgraF = Sse.Multiply(bgraF, Sse2.ConvertToVector128Single(Vector128.Create(a)));
                        bgraF = Sse.Multiply(bgraF, Vector128.Create(1f / 255f));

                        Vector128<int> bgraI32 = Sse2.ConvertToVector128Int32(bgraF);

                        return new PColor32(c.A,
                            (byte)bgraI32.GetElement(2),
                            (byte)bgraI32.GetElement(1),
                            (byte)bgraI32.GetElement(0));
                    }

                    return new PColor32(c.A,
                        (byte)(c.R * c.A / Byte.MaxValue),
                        (byte)(c.G * c.A / Byte.MaxValue),
                        (byte)(c.B * c.A / Byte.MaxValue));
            }
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static PColor32 ToPColor32_6_Shift(this Color32 c) => c.A switch
        {
            255 => new PColor32(c.Value),
            0 => default,
            _ => new PColor32(c.A,
                (byte)((c.R * c.A) >> 8),
                (byte)((c.G * c.A) >> 8),
                (byte)((c.B * c.A) >> 8)),
        };

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static PColor32 ToPColor32_7_ShiftIntrinsicsSse41(this Color32 c)
        {
            switch (c.A)
            {
                case 255:
                    return new PColor32(c.Value);
                case 0:
                    return default;
                default:
                    if (Sse41.IsSupported)
                    {
                        Vector128<int> bgraI32 = Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(c.Value).AsByte());

                        // cannot just use Create((int)c.A) instead of ConvertToVector128Int32(Create(byte)), see https://github.com/dotnet/runtime/issues/83387
                        bgraI32 = Sse41.MultiplyLow(bgraI32, Sse41.ConvertToVector128Int32(Vector128.Create(c.A)));

                        return new PColor32(Ssse3.Shuffle(bgraI32.AsByte().WithElement(13, c.A),
                            Vector128.Create(1, 5, 9, 13, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF)).AsUInt32().ToScalar()); 
                    }

                    return new PColor32(c.A,
                        (byte)((c.R * c.A) >> 8),
                        (byte)((c.G * c.A) >> 8),
                        (byte)((c.B * c.A) >> 8));
            }
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Color32 ToColor32_0_Div(this PColor32 c) => c.A switch
        {
            255 => new Color32(c.Value),
            0 => default,
            _ => new Color32(c.A,
                (byte)((uint)c.R * Byte.MaxValue / c.A),
                (byte)((uint)c.G * Byte.MaxValue / c.A),
                (byte)((uint)c.B * Byte.MaxValue / c.A)),
        };

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Color32 ToColor32_1_DivVector3(this PColor32 c)
        {
            switch (c.A)
            {
                case 255:
                    return new Color32(c.Value);
                case 0:
                    return default;
                default:
                    Vector3 v = new Vector3(c.R, c.G, c.B) * 255f / c.A;
                    return new Color32(c.A, (byte)v.X, (byte)v.Y, (byte)v.Z);
            }
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Color32 ToColor32_2_IntrinsicsDivSse41(this PColor32 c)
        {
            switch (c.A)
            {
                case 255:
                    return new Color32(c.Value);
                case 0:
                    return default;
                default:

                    if (Sse41.IsSupported)
                    {
                        Vector128<float> bgraF = Sse2.ConvertToVector128Single(Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(c.Value).AsByte()));

                        bgraF = Sse.Multiply(bgraF, VectorExtensions.Max8BitF);
                        bgraF = Sse.Divide(bgraF, Sse2.ConvertToVector128Single(Sse41.ConvertToVector128Int32(Vector128.Create(c.A))));
                        Vector128<int> bgraI32 = Sse2.ConvertToVector128Int32(bgraF);

                        return new Color32(Ssse3.Shuffle(bgraI32.AsByte().WithElement(12, c.A),
                            PackLowBytesMask).AsUInt32().ToScalar());
                    }

                    return new Color32(c.A,
                        (byte)(c.R * Byte.MaxValue / c.A),
                        (byte)(c.G * Byte.MaxValue / c.A),
                        (byte)(c.B * Byte.MaxValue / c.A));
            }
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Color32 ToColor32_3_IntrinsicsDivSsse3(this PColor32 c)
        {
            switch (c.A)
            {
                case 255:
                    return new Color32(c.Value);
                case 0:
                    return default;
                default:
                    if (Ssse3.IsSupported)
                    {
                        Vector128<float> bgraF = Sse2.ConvertToVector128Single(Vector128.Create(c.B, c.G, c.R, default));

                        bgraF = Sse.Multiply(bgraF, VectorExtensions.Max8BitF);

                        // cannot just use Create((int)c.A), see https://github.com/dotnet/runtime/issues/83387
                        int a = c.A;
                        bgraF = Sse.Divide(bgraF, Sse2.ConvertToVector128Single(Vector128.Create(a)));

                        Vector128<int> bgraI32 = Sse2.ConvertToVector128Int32(bgraF);

                        return new Color32(Ssse3.Shuffle(bgraI32.AsByte().WithElement(12, c.A),
                            PackLowBytesMask).AsUInt32().ToScalar());
                    }

                    return new Color32(c.A,
                        (byte)(c.R * Byte.MaxValue / c.A),
                        (byte)(c.G * Byte.MaxValue / c.A),
                        (byte)(c.B * Byte.MaxValue / c.A));
            }
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Color32 ToColor32_4_IntrinsicsDivSse2(this PColor32 c)
        {
            switch (c.A)
            {
                case 255:
                    return new Color32(c.Value);
                case 0:
                    return default;
                default:
                    if (Ssse3.IsSupported)
                    {
                        Vector128<float> bgraF = Sse2.ConvertToVector128Single(Vector128.Create(c.B, c.G, c.R, default));

                        bgraF = Sse.Multiply(bgraF, VectorExtensions.Max8BitF);

                        // cannot just use Create((int)c.A), see https://github.com/dotnet/runtime/issues/83387
                        int a = c.A;
                        bgraF = Sse.Divide(bgraF, Sse2.ConvertToVector128Single(Vector128.Create(a)));

                        Vector128<int> bgraI32 = Sse2.ConvertToVector128Int32(bgraF);

                        return new Color32(c.A,
                            (byte)bgraI32.GetElement(2),
                            (byte)bgraI32.GetElement(1),
                            (byte)bgraI32.GetElement(0));
                    }

                    return new Color32(c.A,
                        (byte)(c.R * Byte.MaxValue / c.A),
                        (byte)(c.G * Byte.MaxValue / c.A),
                        (byte)(c.B * Byte.MaxValue / c.A));
            }
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Color32 ToColor32_5_Shift(this PColor32 c)
        {
            switch (c.A)
            {
                case 255:
                    return new Color32(c.Value);
                case 0:
                    return default;
                default:
                    // a+1: eg. if ARGB=128,128,x,x, then the result could be 256
                    var a = c.A + 1;
                    return new Color32(c.A, 
                        (byte)(((uint)c.R << 8) / a),
                        (byte)(((uint)c.G << 8) / a),
                        (byte)(((uint)c.B << 8) / a));
            }
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Color32 ToColor32_6_ShiftIntrinsicsSse41(this PColor32 c)
        {
            switch (c.A)
            {
                case 255:
                    return new Color32(c.Value);
                case 0:
                    return default;
                default:
                    if (Sse41.IsSupported)
                    {
                        Vector128<int> bgraI32 = Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(c.Value).AsByte());
                        bgraI32 = Sse2.ShiftLeftLogical(bgraI32, 8);
                        Vector128<float> bgraF = Sse.Divide(Sse2.ConvertToVector128Single(bgraI32),
                            Sse2.ConvertToVector128Single(Vector128.Create(c.A + 1)));
                        bgraI32 = Sse2.ConvertToVector128Int32WithTruncation(bgraF);
                        return new Color32(Ssse3.Shuffle(bgraI32.AsByte().WithElement(12, c.A),
                            PackLowBytesMask).AsUInt32().ToScalar()); 
                    }

                    // a+1: eg. if ARGB=128,128,x,x, then the result could be 256
                    var a = c.A + 1;
                    return new Color32(c.A,
                        (byte)(((uint)c.R << 8) / a),
                        (byte)(((uint)c.G << 8) / a),
                        (byte)(((uint)c.B << 8) / a));
            }
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Color32 ToColor32_7_ShiftIntrinsicsSsse3(this PColor32 c)
        {
            switch (c.A)
            {
                case 255:
                    return new Color32(c.Value);
                case 0:
                    return default;
                default:
                    if (Ssse3.IsSupported)
                    {
                        Vector128<int> bgraI32 = Vector128.Create(c.B, c.G, c.R, default);
                        bgraI32 = Sse2.ShiftLeftLogical(bgraI32, 8);

                        Vector128<float> bgraF = Sse.Divide(Sse2.ConvertToVector128Single(bgraI32),
                            Sse2.ConvertToVector128Single(Vector128.Create(c.A + 1)));

                        bgraI32 = Sse2.ConvertToVector128Int32(bgraF);
                        return new Color32(Ssse3.Shuffle(bgraI32.AsByte().WithElement(12, c.A),
                            PackLowBytesMask).AsUInt32().ToScalar()); 
                    }

                    // a+1: eg. if ARGB=128,128,x,x, then the result could be 256
                    var a = c.A + 1;
                    return new Color32(c.A,
                        (byte)(((uint)c.R << 8) / a),
                        (byte)(((uint)c.G << 8) / a),
                        (byte)(((uint)c.B << 8) / a));

            }
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Color32 ToColor32_8_ShiftIntrinsicsSse2(this PColor32 c)
        {
            switch (c.A)
            {
                case 255:
                    return new Color32(c.Value);
                case 0:
                    return default;
                default:
                    if (Sse2.IsSupported)
                    {
                        Vector128<int> bgraI32 = Vector128.Create(c.B, c.G, c.R, default);
                        bgraI32 = Sse2.ShiftLeftLogical(bgraI32, 8);

                        Vector128<float> bgraF = Sse.Divide(Sse2.ConvertToVector128Single(bgraI32),
                            Sse2.ConvertToVector128Single(Vector128.Create(c.A + 1)));

                        bgraI32 = Sse2.ConvertToVector128Int32(bgraF);

                        return new Color32(c.A,
                            (byte)bgraI32.GetElement(2),
                            (byte)bgraI32.GetElement(1),
                            (byte)bgraI32.GetElement(0));
                    }

                    // a+1: eg. if ARGB=128,128,x,x, then the result could be 256
                    var a = c.A + 1;
                    return new Color32(c.A,
                        (byte)(((uint)c.R << 8) / a),
                        (byte)(((uint)c.G << 8) / a),
                        (byte)(((uint)c.B << 8) / a));

            }
        }

        #endregion
    }
}

#endif