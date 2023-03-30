#if NETCOREAPP3_0_OR_GREATER
#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: Conversion_Color64_PColor64.cs
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
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

using KGySoft.Drawing.Imaging;

using NUnit.Framework;

#endregion

namespace KGySoft.Drawing.PerformanceTests
{
    [TestFixture]
    public class Conversion_Color64_PColor64
    {
        #region Methods

        [Test]
        public void Color64_PColor64_ConversionTest_Div()
        {
            var testColor64 = new Color64(0x8000, 0xFFFF, 0x8000, 0x4000);
            //var testColor64 = new Color64(0xFFFE, 0xFFFF, 0xFFFE, 0xFFFD);
            var testPColor64 = testColor64.ToPColor64();

            Console.WriteLine($"{"Test color:",-40} {testColor64}");
            void DoAssert(Expression<Func<Color64, Color64>> e)
            {
                var m2 = (MethodCallExpression)e.Body;
                var m1 = (MethodCallExpression)m2.Arguments[0];
                Color64 actual = e.Compile().Invoke(testColor64);
                Console.WriteLine($"{$"{m1.Method.Name}.{m2.Method.Name}:",-40} {actual}");
                Assert.IsTrue(testColor64.TolerantEquals(actual, 1), $"{m1.Method.Name}.{m2.Method.Name}: {testColor64} vs. {actual}");
            }


            DoAssert(_ => testColor64.ToPColor64_0_Div().ToColor64_0_Div());
            DoAssert(_ => testColor64.ToPColor64_1_DivVector3().ToColor64_1_DivVector3());
            // BUG .NET 7: https://github.com/dotnet/runtime/issues/83387
            //DoAssert(_ => testColor64.ToPColor64_2_DivVectorTSpan().ToColor64_2_DivVectorTSpan());
            //DoAssert(_ => testColor64.ToPColor64_3_DivVectorTWiden().ToColor64_3_DivVectorTWiden());
            //DoAssert(_ => testColor64.ToPColor64_4_DivVector128().ToColor64_4_DivVector128());
            DoAssert(_ => testColor64.ToPColor64_5_DivIntrinsicsFloat().ToColor64_5_DivIntrinsicsFloat());
            DoAssert(_ => testColor64.ToPColor64_6_DivIntrinsicsDouble().ToColor64_6_DivIntrinsicsDouble());
            DoAssert(_ => testColor64.ToPColor64_7_RecipIntrinsicsFloat().ToColor64_7_RecipIntrinsicsFloat());
            DoAssert(_ => testColor64.ToPColor64_8_RecipIntrinsicsDouble().ToColor64_8_RecipIntrinsicsDouble());
            DoAssert(_ => testColor64.ToPColor64_9_Final().ToColor64_9_Final());
            DoAssert(_ => testColor64.ToPColor64_9b_Final().ToColor64_9_Final());
            DoAssert(_ => testColor64.ToPColor64_9c_Final().ToColor64_9_Final());
            Console.WriteLine();

            new PerformanceTest<PColor64> { TestName = "Color64 -> PColor64", TestTime = 500, Iterations = 10_000_000, Repeat = 3 }
                .AddCase(() => testColor64.ToPColor64_0_Div(), nameof(Extensions.ToPColor64_0_Div))
                //.AddCase(() => testColor64.ToPColor64_1_DivVector3(), nameof(Extensions.ToPColor64_1_DivVector3))
                ////.AddCase(() => testColor64.ToPColor64_2_DivVectorTSpan(), nameof(Extensions.ToPColor64_2_DivVectorTSpan))
                ////.AddCase(() => testColor64.ToPColor64_3_DivVectorTWiden(), nameof(Extensions.ToPColor64_3_DivVectorTWiden))
                ////.AddCase(() => testColor64.ToPColor64_4_DivVector128(), nameof(Extensions.ToPColor64_4_DivVector128))
                .AddCase(() => testColor64.ToPColor64_5_DivIntrinsicsFloat(), nameof(Extensions.ToPColor64_5_DivIntrinsicsFloat))
                //.AddCase(() => testColor64.ToPColor64_6_DivIntrinsicsDouble(), nameof(Extensions.ToPColor64_6_DivIntrinsicsDouble))
                .AddCase(() => testColor64.ToPColor64_7_RecipIntrinsicsFloat(), nameof(Extensions.ToPColor64_7_RecipIntrinsicsFloat))
                //.AddCase(() => testColor64.ToPColor64_8_RecipIntrinsicsDouble(), nameof(Extensions.ToPColor64_8_RecipIntrinsicsDouble))
                .AddCase(() => testColor64.ToPColor64_9_Final(), nameof(Extensions.ToPColor64_9_Final))
                .AddCase(() => testColor64.ToPColor64_9b_Final(), nameof(Extensions.ToPColor64_9b_Final))
                .AddCase(() => testColor64.ToPColor64_9c_Final(), nameof(Extensions.ToPColor64_9c_Final))
                .DoTest()
                .DumpResults(Console.Out);

            new PerformanceTest<Color64> { TestName = "PColor64 -> Color64", TestTime = 500, Iterations = 10_000_000, Repeat = 3 }
                .AddCase(() => testPColor64.ToColor64_0_Div(), nameof(Extensions.ToColor64_0_Div))
                .AddCase(() => testPColor64.ToColor64_1_DivVector3(), nameof(Extensions.ToColor64_1_DivVector3))
                //.AddCase(() => testPColor64.ToColor64_2_DivVectorTSpan(), nameof(Extensions.ToColor64_2_DivVectorTSpan))
                //.AddCase(() => testPColor64.ToColor64_3_DivVectorTWiden(), nameof(Extensions.ToColor64_3_DivVectorTWiden))
                //.AddCase(() => testPColor64.ToColor64_4_DivVector128(), nameof(Extensions.ToColor64_4_DivVector128))
                .AddCase(() => testPColor64.ToColor64_5_DivIntrinsicsFloat(), nameof(Extensions.ToColor64_5_DivIntrinsicsFloat))
                //.AddCase(() => testPColor64.ToColor64_6_DivIntrinsicsDouble(), nameof(Extensions.ToColor64_6_DivIntrinsicsDouble))
                .AddCase(() => testPColor64.ToColor64_7_RecipIntrinsicsFloat(), nameof(Extensions.ToColor64_7_RecipIntrinsicsFloat))
                //.AddCase(() => testPColor64.ToColor64_8_RecipIntrinsicsDouble(), nameof(Extensions.ToColor64_8_RecipIntrinsicsDouble))
                .AddCase(() => testPColor64.ToColor64_9_Final(), nameof(Extensions.ToColor64_9_Final))
                .DoTest()
                .DumpResults(Console.Out);
        }

        [Test]
        public void Color64_PColor64_ConversionTest_Shift()
        {
            var testColor64 = new Color64(0x8000, 0xFFFF, 0x8000, 0x4000);
            var testPColor64 = testColor64.ToPColor64();

            var expected = testColor64.ToPColor64_0_Shift().ToColor64_0_Shift();
            // BUG .NET 7: https://github.com/dotnet/runtime/issues/83387
            //Assert.AreEqual(expected, testColor64.ToPColor64_1_ShiftVector128().ToColor64_1_ShiftVector128());
            //Assert.AreEqual(expected, testColor64.ToPColor64_2_ShiftIntrinsics().ToColor64_2_ShiftIntrinsics());

            new PerformanceTest<PColor64> { TestName = "Color64 -> PColor64", TestTime = 500, Iterations = 10_000_000, Repeat = 3 }
                .AddCase(() => testColor64.ToPColor64_0_Shift(), nameof(Extensions.ToPColor64_0_Shift))
                //.AddCase(() => testColor64.ToPColor64_1_ShiftVector128(), nameof(Extensions.ToPColor64_1_ShiftVector128))
                .AddCase(() => testColor64.ToPColor64_2_ShiftIntrinsics(), nameof(Extensions.ToPColor64_2_ShiftIntrinsics))
                .DoTest()
                .DumpResults(Console.Out);

            new PerformanceTest<Color64> { TestName = "PColor64 -> Color64", TestTime = 500, Iterations = 10_000_000, Repeat = 3 }
                .AddCase(() => testPColor64.ToColor64_0_Shift(), nameof(Extensions.ToColor64_0_Shift))
                //.AddCase(() => testPColor64.ToColor64_1_ShiftVector128(), nameof(Extensions.ToColor64_1_ShiftVector128))
                .AddCase(() => testPColor64.ToColor64_2_ShiftIntrinsics(), nameof(Extensions.ToColor64_2_ShiftIntrinsics))
                .DoTest()
                .DumpResults(Console.Out);
        }

        #endregion
    }

    internal static partial class Extensions
    {
        #region Methods

        internal static PColor64 ToPColor64_0_Div(this Color64 c) => c.A switch
        {
            UInt16.MaxValue => new PColor64(c.Value),
            UInt16.MinValue => default,
            _ => new PColor64(c.A,
                (ushort)((uint)c.R * c.A / UInt16.MaxValue),
                (ushort)((uint)c.G * c.A / UInt16.MaxValue),
                (ushort)((uint)c.B * c.A / UInt16.MaxValue)),
        };

        internal static Color64 ToColor64_0_Div(this PColor64 c) => c.A switch
        {
            UInt16.MaxValue => new Color64(c.Value),
            UInt16.MinValue => default,
            _ => new Color64(c.A,
                (ushort)((uint)c.R * UInt16.MaxValue / c.A),
                (ushort)((uint)c.G * UInt16.MaxValue / c.A),
                (ushort)((uint)c.B * UInt16.MaxValue / c.A)),
        };

        internal static PColor64 ToPColor64_1_DivVector3(this Color64 c)
        {
            switch (c.A)
            {
                case UInt16.MaxValue:
                    return new PColor64(c.Value);
                case UInt16.MinValue:
                    return default;
                default:
                    Vector3 v = new Vector3(c.R, c.G, c.B) * c.A / 0xFFFF;
                    return new PColor64(c.A, (ushort)v.X, (ushort)v.Y, (ushort)v.Z);
            }
        }

        internal static Color64 ToColor64_1_DivVector3(this PColor64 c)
        {
            switch (c.A)
            {
                case UInt16.MaxValue:
                    return new Color64(c.Value);
                case UInt16.MinValue:
                    return default;
                default:
                    Vector3 v = new Vector3(c.R, c.G, c.B) * 0xFFFF / c.A;
                    return new Color64(c.A, (ushort)v.X, (ushort)v.Y, (ushort)v.Z);
            }
        }

        internal static PColor64 ToPColor64_2_DivVectorTSpan(this Color64 c)
        {
            switch (c.A)
            {
                case UInt16.MaxValue:
                    return new PColor64(c.Value);
                case UInt16.MinValue:
                    return default;
                default:
                    Span<uint> span = stackalloc uint[Vector<uint>.Count];
                    span[0] = c.R;
                    span[1] = c.G;
                    span[2] = c.B;
                    Vector<uint> v = new Vector<uint>(span) * c.A / new Vector<uint>(0xFFFF);
                    return new PColor64(c.A, (ushort)v[0], (ushort)v[1], (ushort)v[2]);
            }
        }

        internal static Color64 ToColor64_2_DivVectorTSpan(this PColor64 c)
        {
            switch (c.A)
            {
                case UInt16.MaxValue:
                    return new Color64(c.Value);
                case UInt16.MinValue:
                    return default;
                default:
                    Span<uint> span = stackalloc uint[Vector<uint>.Count];
                    span[0] = c.R;
                    span[1] = c.G;
                    span[2] = c.B;
                    Vector<uint> v = new Vector<uint>(span) * 0xFFFF / new Vector<uint>(c.A);
                    return new Color64(c.A, (ushort)v[0], (ushort)v[1], (ushort)v[2]);
            }
        }

        //internal static PColor64 ToPColor64_3_DivVectorTWiden(this Color64 c)
        //{
        //    switch (c.A)
        //    {
        //        case UInt16.MaxValue:
        //            return new PColor64(c.Value);
        //        case UInt16.MinValue:
        //            return default;
        //        default:
        //            Vector.Widen(new Vector<ulong>(c.Value).As<ulong, ushort>(), out Vector<uint> v, out var _);
        //            v = v * c.A / new Vector<uint>(0xFFFF);
        //            return new PColor64(c.A, (ushort)v[0], (ushort)v[1], (ushort)v[2]);
        //    }
        //}

        //internal static Color64 ToColor64_3_DivVectorTWiden(this PColor64 c)
        //{
        //    switch (c.A)
        //    {
        //        case UInt16.MaxValue:
        //            return new Color64(c.Value);
        //        case UInt16.MinValue:
        //            return default;
        //        default:
        //            Vector.Widen(new Vector<ulong>(c.Value).As<ulong, ushort>(), out Vector<uint> v, out var _);
        //            v = v * 0xFFFF / new Vector<uint>(c.A);
        //            return new Color64(c.A, (ushort)v[0], (ushort)v[1], (ushort)v[2]);
        //    }
        //}

        //internal static PColor64 ToPColor64_4_DivVector128(this Color64 c)
        //{
        //    switch (c.A)
        //    {
        //        case UInt16.MaxValue:
        //            return new PColor64(c.Value);
        //        case UInt16.MinValue:
        //            return default;
        //        default:
        //            Vector128<uint> v = Vector128.Create(c.R, c.G, c.B, 0u);
        //            v = v * c.A / Vector128.Create(0xFFFFu);
        //            return new PColor64(c.A, (ushort)v[0], (ushort)v[1], (ushort)v[2]);
        //    }
        //}

        //internal static Color64 ToColor64_4_DivVector128(this PColor64 c)
        //{
        //    switch (c.A)
        //    {
        //        case UInt16.MaxValue:
        //            return new Color64(c.Value);
        //        case UInt16.MinValue:
        //            return default;
        //        default:
        //            Vector128<uint> v = Vector128.Create(c.R, c.G, c.B, 0u);
        //            v = v * 0xFFFFu / Vector128.Create((uint)c.A);
        //            return new Color64(c.A, (ushort)v[0], (ushort)v[1], (ushort)v[2]);
        //    }
        //}

        internal static PColor64 ToPColor64_5_DivIntrinsicsFloat(this Color64 c)
        {
            switch (c.A)
            {
                case UInt16.MaxValue:
                    return new PColor64(c.Value);
                case UInt16.MinValue:
                    return default;
                default:
                    //Vector128<float> v = Vector128.Create(c.R, c.G, c.B, 0f);
                    Vector128<float> v = Sse2.ConvertToVector128Single(Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(c.Value).AsUInt16()));
                    //v = Avx.Multiply(v, Vector128.Create((float)c.A));
                    v = Sse.Multiply(v, Sse2.ConvertToVector128Single(Sse41.ConvertToVector128Int32(Vector128.Create(c.A))));
                    v = Sse.Divide(v, Vector128.Create(65535f));
                    Vector128<int> vi = Sse2.ConvertToVector128Int32WithTruncation(v);
                    return new PColor64(c.A, (ushort)vi.GetElement(2), (ushort)vi.GetElement(1), (ushort)vi.GetElement(0));
            }
        }

        internal static Color64 ToColor64_5_DivIntrinsicsFloat(this PColor64 c)
        {
            switch (c.A)
            {
                case UInt16.MaxValue:
                    return new Color64(c.Value);
                case UInt16.MinValue:
                    return default;
                default:
                    //Vector128<float> v = Vector128.Create(c.R, c.G, c.B, 0f);
                    Vector128<float> v = Sse2.ConvertToVector128Single(Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(c.Value).AsUInt16()));
                    v = Sse.Multiply(v, Vector128.Create(65535f));
                    //v = Avx.Divide(v, Vector128.Create((float)c.A));
                    v = Sse.Divide(v, Sse2.ConvertToVector128Single(Sse41.ConvertToVector128Int32(Vector128.Create(c.A))));
                    Vector128<int> vi = Sse2.ConvertToVector128Int32WithTruncation(v);
                    return new Color64(c.A, (ushort)vi.GetElement(2), (ushort)vi.GetElement(1), (ushort)vi.GetElement(0));
            }
        }

        internal static PColor64 ToPColor64_6_DivIntrinsicsDouble(this Color64 c)
        {
            switch (c.A)
            {
                case UInt16.MaxValue:
                    return new PColor64(c.Value);
                case UInt16.MinValue:
                    return default;
                default:
                    //Vector256<double> v = Vector256.Create(c.R, c.G, c.B, 0d);
                    Vector256<double> v = Avx.ConvertToVector256Double(Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(c.Value).AsUInt16()));
                    //v = Avx.Multiply(v, Vector256.Create((double)c.A));
                    v = Avx.Multiply(v, Avx.ConvertToVector256Double(Sse41.ConvertToVector128Int32(Vector128.Create(c.A))));
                    v = Avx.Divide(v, Vector256.Create(65535d));
                    Vector128<int> vi = Avx.ConvertToVector128Int32WithTruncation(v);
                    return new PColor64(c.A, (ushort)vi.GetElement(2), (ushort)vi.GetElement(1), (ushort)vi.GetElement(0));
            }
        }

        internal static Color64 ToColor64_6_DivIntrinsicsDouble(this PColor64 c)
        {
            switch (c.A)
            {
                case UInt16.MaxValue:
                    return new Color64(c.Value);
                case UInt16.MinValue:
                    return default;
                default:
                    //Vector256<double> v = Vector256.Create(c.R, c.G, c.B, 0d);
                    Vector256<double> v = Avx.ConvertToVector256Double(Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(c.Value).AsUInt16()));
                    v = Avx.Multiply(v, Vector256.Create(65535d));
                    //v = Avx.Divide(v, Vector256.Create((double)c.A));
                    v = Avx.Divide(v, Avx.ConvertToVector256Double(Sse41.ConvertToVector128Int32(Vector128.Create(c.A))));
                    Vector128<int> vi = Avx.ConvertToVector128Int32WithTruncation(v);
                    return new Color64(c.A, (ushort)vi.GetElement(2), (ushort)vi.GetElement(1), (ushort)vi.GetElement(0));
            }
        }

        internal static PColor64 ToPColor64_7_RecipIntrinsicsFloat(this Color64 c)
        {
            switch (c.A)
            {
                case UInt16.MaxValue:
                    return new PColor64(c.Value);
                case UInt16.MinValue:
                    return default;
                default:
                    //Vector128<float> v = Vector128.Create(c.R, c.G, c.B, 0f);
                    Vector128<float> v = Sse2.ConvertToVector128Single(Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(c.Value).AsUInt16()));
                    v = Sse.Multiply(v, Vector128.Create(1f / 65535));
                    //v = Avx.Multiply(v, Vector128.Create((float)c.A));
                    v = Sse.Multiply(v, Sse2.ConvertToVector128Single(Sse41.ConvertToVector128Int32(Vector128.Create(c.A))));
                    //var asU16 = Sse2.ConvertToVector128Int32(v).AsUInt16();
                    //var u16WithA = asU16.WithElement(6, c.A);
                    var asInt32 = Sse2.ConvertToVector128Int32(v).AsUInt16().WithElement(6, c.A).AsInt32();
                    //var packed = Sse41.PackUnsignedSaturate(asInt32, asInt32);
                    //Vector128<ulong> vi = Sse41.PackUnsignedSaturate(asInt32, asInt32).AsUInt64();
                    //var shuffled = Ssse3.Shuffle(asBytes, Vector128.Create((byte)0, 1, 4, 5, 8, 9, 12, 13, 0, 0, 0, 0, 0, 0, 0, 0));
                    return new PColor64(Sse41.PackUnsignedSaturate(asInt32, asInt32).AsUInt64().ToScalar());
            }
        }

        internal static Color64 ToColor64_7_RecipIntrinsicsFloat(this PColor64 c)
        {
            switch (c.A)
            {
                case UInt16.MaxValue:
                    return new Color64(c.Value);
                case UInt16.MinValue:
                    return default;
                default:
                    //Vector128<float> v = Vector128.Create(c.R, c.G, c.B, 0f);
                    Vector128<float> v = Sse2.ConvertToVector128Single(Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(c.Value).AsUInt16()));
                    //v = Sse.Multiply(v, Sse.Reciprocal(Sse2.ConvertToVector128Single(Sse41.ConvertToVector128Int32(Vector128.Create(c.A)))));
                    v = Sse.Multiply(v, Vector128.Create(1f / c.A)); // Sse.Reciprocal has only 1.5*2^-12 precision
                    v = Sse.Multiply(v, Vector128.Create(65535f));
                    //v = Avx.Divide(v, Vector128.Create((float)c.A));
                    //v = Sse.Divide(v, Sse2.ConvertToVector128Single(Sse41.ConvertToVector128Int32(Vector128.Create(c.A))));
                    Vector128<int> vi = Sse2.ConvertToVector128Int32WithTruncation(v);
                    return new Color64(c.A, (ushort)vi.GetElement(2), (ushort)vi.GetElement(1), (ushort)vi.GetElement(0));
            }
        }

        internal static PColor64 ToPColor64_8_RecipIntrinsicsDouble(this Color64 c)
        {
            switch (c.A)
            {
                case UInt16.MaxValue:
                    return new PColor64(c.Value);
                case UInt16.MinValue:
                    return default;
                default:
                    //Vector256<double> v = Vector256.Create(c.R, c.G, c.B, 0d);
                    Vector256<double> v = Avx.ConvertToVector256Double(Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(c.Value).AsUInt16()));
                    v = Avx.Multiply(v, Vector256.Create(1 / 65535d));
                    //v = Avx.Multiply(v, Vector256.Create((double)c.A));
                    v = Avx.Multiply(v, Avx.ConvertToVector256Double(Sse41.ConvertToVector128Int32(Vector128.Create(c.A))));
                    //v = Avx.Divide(v, Vector256.Create(65535d));
                    Vector128<int> vi = Avx.ConvertToVector128Int32WithTruncation(v);
                    return new PColor64(c.A, (ushort)vi.GetElement(2), (ushort)vi.GetElement(1), (ushort)vi.GetElement(0));
            }
        }

        internal static Color64 ToColor64_8_RecipIntrinsicsDouble(this PColor64 c)
        {
            switch (c.A)
            {
                case UInt16.MaxValue:
                    return new Color64(c.Value);
                case UInt16.MinValue:
                    return default;
                default:
                    //Vector256<double> v = Vector256.Create(c.R, c.G, c.B, 0d);
                    Vector256<double> v = Avx.ConvertToVector256Double(Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(c.Value).AsUInt16()));
                    //v = Avx.Multiply(v, Avx2.Reciprocal(Avx.ConvertToVector256Double(Sse41.ConvertToVector128Int32(Vector128.Create(c.A)))));
                    v = Avx.Multiply(v, Vector256.Create(1d / c.A)); // there is no Avx2.Reciprocal for _mm256_rcp14_pd but that also has just 2*2^-14 precision
                    v = Avx.Multiply(v, Vector256.Create(65535d));
                    //v = Avx.Divide(v, Vector256.Create((double)c.A));
                    //v = Avx.Divide(v, Avx.ConvertToVector256Double(Sse41.ConvertToVector128Int32(Vector128.Create(c.A))));
                    Vector128<int> vi = Avx.ConvertToVector128Int32WithTruncation(v);
                    return new Color64(c.A, (ushort)vi.GetElement(2), (ushort)vi.GetElement(1), (ushort)vi.GetElement(0));
            }
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static PColor64 ToPColor64_9_Final(this Color64 c)
        {
            switch (c.A)
            {
                case UInt16.MaxValue:
                    return new PColor64(c.Value);
                case UInt16.MinValue:
                    return default;
                default:
                    if (Sse2.IsSupported)
                    {
                        Vector128<float> bgraF = Sse2.ConvertToVector128Single(Sse41.IsSupported
                            ? Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(c.Value).AsUInt16())
                            : Vector128.Create(c.B, c.G, c.R, default));

                        bgraF = Sse.Multiply(bgraF, Vector128.Create(1f / 65535));

                        bgraF = Sse.Multiply(bgraF, Sse2.ConvertToVector128Single(Sse41.IsSupported
                            ? Sse41.ConvertToVector128Int32(Vector128.Create(c.A))
                            : Vector128.Create((int)c.A)));

                        Vector128<int> bgraI32 = Sse2.ConvertToVector128Int32(bgraF);

                        if (Ssse3.IsSupported)
                        {
                            bgraI32 = bgraI32.AsUInt16().WithElement(6, c.A).AsInt32();

                            return new PColor64((Sse41.IsSupported
                                    ? Sse41.PackUnsignedSaturate(bgraI32, bgraI32).AsUInt64()
                                    : Ssse3.Shuffle(bgraI32.AsByte(),
                                        Vector128.Create(0, 1, 4, 5, 8, 9, 12, 13, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF)).AsUInt64())
                                .ToScalar());
                        }

                        return new PColor64(c.A,
                            (ushort)bgraI32.GetElement(2),
                            (ushort)bgraI32.GetElement(1),
                            (ushort)bgraI32.GetElement(0));
                    }

                    return new PColor64(c.A,
                        (ushort)((uint)c.R * c.A / UInt16.MaxValue),
                        (ushort)((uint)c.G * c.A / UInt16.MaxValue),
                        (ushort)((uint)c.B * c.A / UInt16.MaxValue));
            }
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static PColor64 ToPColor64_9b_Final(this Color64 c)
        {
            switch (c.A)
            {
                case UInt16.MaxValue:
                    return new PColor64(c.Value);
                case UInt16.MinValue:
                    return default;
                default:
                    if (Sse2.IsSupported)
                    {
                        Vector128<float> bgraF = Sse2.ConvertToVector128Single(Sse41.IsSupported
                            ? Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(c.Value).AsUInt16())
                            : Vector128.Create(c.B, c.G, c.R, default));

                        bgraF = Sse.Multiply(bgraF, Vector128.Create(1f / 65535));

                        bgraF = Sse.Multiply(bgraF, Sse2.ConvertToVector128Single(Sse41.IsSupported
                            ? Sse41.ConvertToVector128Int32(Vector128.Create(c.A))
                            : Vector128.Create((int)c.A)));

                        Vector128<int> bgraI32 = Sse2.ConvertToVector128Int32(bgraF);

                        if (Ssse3.IsSupported)
                        {
                            bgraI32 = bgraI32.AsUInt16().WithElement(6, c.A).AsInt32();

                            return new PColor64((/*Sse41.IsSupported
                                    ? Sse41.PackUnsignedSaturate(bgraI32, bgraI32).AsUInt64()
                                    : */Ssse3.Shuffle(bgraI32.AsByte(),
                                        Vector128.Create(0, 1, 4, 5, 8, 9, 12, 13, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF)).AsUInt64())
                                .ToScalar());
                        }

                        return new PColor64(c.A,
                            (ushort)bgraI32.GetElement(2),
                            (ushort)bgraI32.GetElement(1),
                            (ushort)bgraI32.GetElement(0));
                    }

                    return new PColor64(c.A,
                        (ushort)((uint)c.R * c.A / UInt16.MaxValue),
                        (ushort)((uint)c.G * c.A / UInt16.MaxValue),
                        (ushort)((uint)c.B * c.A / UInt16.MaxValue));
            }
        }

        private static Vector128<byte> mask = Vector128.Create(0, 1, 4, 5, 8, 9, 12, 13, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF);
        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static PColor64 ToPColor64_9c_Final(this Color64 c)
        {
            switch (c.A)
            {
                case UInt16.MaxValue:
                    return new PColor64(c.Value);
                case UInt16.MinValue:
                    return default;
                default:
                    if (Sse2.IsSupported)
                    {
                        Vector128<float> bgraF = Sse2.ConvertToVector128Single(Sse41.IsSupported
                            ? Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(c.Value).AsUInt16())
                            : Vector128.Create(c.B, c.G, c.R, default));

                        bgraF = Sse.Multiply(bgraF, Vector128.Create(1f / 65535));

                        bgraF = Sse.Multiply(bgraF, Sse2.ConvertToVector128Single(Sse41.IsSupported
                            ? Sse41.ConvertToVector128Int32(Vector128.Create(c.A))
                            : Vector128.Create((int)c.A)));

                        Vector128<int> bgraI32 = Sse2.ConvertToVector128Int32(bgraF);

                        if (Ssse3.IsSupported)
                        {
                            bgraI32 = bgraI32.AsUInt16().WithElement(6, c.A).AsInt32();

                            return new PColor64((/*Sse41.IsSupported
                                    ? Sse41.PackUnsignedSaturate(bgraI32, bgraI32).AsUInt64()
                                    : */Ssse3.Shuffle(bgraI32.AsByte(), mask).AsUInt64())
                                .ToScalar());
                        }

                        return new PColor64(c.A,
                            (ushort)bgraI32.GetElement(2),
                            (ushort)bgraI32.GetElement(1),
                            (ushort)bgraI32.GetElement(0));
                    }

                    return new PColor64(c.A,
                        (ushort)((uint)c.R * c.A / UInt16.MaxValue),
                        (ushort)((uint)c.G * c.A / UInt16.MaxValue),
                        (ushort)((uint)c.B * c.A / UInt16.MaxValue));
            }
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Color64 ToColor64_9_Final(this PColor64 c)
        {
            switch (c.A)
            {
                case UInt16.MaxValue:
                    return new Color64(c.Value);
                case UInt16.MinValue:
                    return default;
                default:
                    if (Sse2.IsSupported)
                    {
                        Vector128<float> bgraF = Sse2.ConvertToVector128Single(Sse41.IsSupported
                            ? Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(c.Value).AsUInt16())
                            : Vector128.Create(c.B, c.G, c.R, default));

                        bgraF = Sse.Divide(bgraF, Sse2.ConvertToVector128Single(Sse41.IsSupported
                            ? Sse41.ConvertToVector128Int32(Vector128.Create(c.A))
                            : Vector128.Create((int)c.A)));

                        bgraF = Sse.Multiply(bgraF, Vector128.Create(65535f));

                        Vector128<int> bgraI32 = Sse2.ConvertToVector128Int32(bgraF);

                        if (Ssse3.IsSupported)
                        {
                            bgraI32 = bgraI32.AsUInt16().WithElement(6, c.A).AsInt32();

                            return new Color64((Sse41.IsSupported
                                    ? Sse41.PackUnsignedSaturate(bgraI32, bgraI32).AsUInt64()
                                    : Ssse3.Shuffle(bgraI32.AsByte(),
                                        Vector128.Create(0, 1, 4, 5, 8, 9, 12, 13, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF)).AsUInt64())
                                .ToScalar());
                        }

                        return new Color64(c.A,
                            (ushort)bgraI32.GetElement(2),
                            (ushort)bgraI32.GetElement(1),
                            (ushort)bgraI32.GetElement(0));
                    }

                    return new Color64(c.A,
                        (ushort)((uint)c.R * UInt16.MaxValue / c.A),
                        (ushort)((uint)c.G * UInt16.MaxValue / c.A),
                        (ushort)((uint)c.B * UInt16.MaxValue / c.A));
            }
        }

        internal static PColor64 ToPColor64_0_Shift(this Color64 c) => c.A switch
        {
            UInt16.MaxValue => new PColor64(c.Value),
            UInt16.MinValue => default,
            _ => new PColor64(c.A,
                (ushort)((c.R * c.A) >> 16),
                (ushort)((c.G * c.A) >> 16),
                (ushort)((c.B * c.A) >> 16)),
        };

        internal static Color64 ToColor64_0_Shift(this PColor64 c) => c.A switch
        {
            UInt16.MaxValue => new Color64(c.Value),
            UInt16.MinValue => default,
            _ => new Color64(c.A,
                (ushort)(((ulong)c.R << 16) / c.A),
                (ushort)(((ulong)c.G << 16) / c.A),
                (ushort)(((ulong)c.B << 16) / c.A)),
        };

        //internal static PColor64 ToPColor64_1_ShiftVector128(this Color64 c)
        //{
        //    switch (c.A)
        //    {
        //        case UInt16.MaxValue:
        //            return new PColor64(c.Value);
        //        case UInt16.MinValue:
        //            return default;
        //        default:
        //            Vector128<uint> v = Vector128.Create(c.R, c.G, c.B, 0u);
        //            v *= c.A;
        //            v = Vector128.ShiftRightLogical(v, 16);
        //            return new PColor64(c.A, (ushort)v[0], (ushort)v[1], (ushort)v[2]);
        //    }
        //}

        //internal static Color64 ToColor64_1_ShiftVector128(this PColor64 c)
        //{
        //    switch (c.A)
        //    {
        //        case UInt16.MaxValue:
        //            return new Color64(c.Value);
        //        case UInt16.MinValue:
        //            return default;
        //        default:
        //            Vector128<uint> v = Vector128.Create(c.R, c.G, c.B, 0u);
        //            v = Vector128.ShiftLeft(v, 16);
        //            v /= Vector128.Create((uint)c.A);
        //            return new Color64(c.A, (ushort)v[0], (ushort)v[1], (ushort)v[2]);
        //    }
        //}

        internal static PColor64 ToPColor64_2_ShiftIntrinsics(this Color64 c)
        {
            switch (c.A)
            {
                case UInt16.MaxValue:
                    return new PColor64(c.Value);
                case UInt16.MinValue:
                    return default;
                default:
                    //Vector128<uint> v = Vector128.Create(c.R, c.G, c.B, 0u);
                    Vector128<uint> v = Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(c.Value).AsUInt16()).AsUInt32();
                    v = Sse41.MultiplyLow(v, Vector128.Create((uint)c.A));
                    v = Sse2.ShiftRightLogical(v, 16);
                    return new PColor64(c.A, (ushort)v.GetElement(2), (ushort)v.GetElement(1), (ushort)v.GetElement(0));
            }
        }

        internal static Color64 ToColor64_2_ShiftIntrinsics(this PColor64 c)
        {
            switch (c.A)
            {
                case UInt16.MaxValue:
                    return new Color64(c.Value);
                case UInt16.MinValue:
                    return default;
                default:
                    Vector128<uint> v = Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(c.Value).AsUInt16()).AsUInt32();
                    //Vector128<uint> v = Vector128.Create(c.R, c.G, c.B, 0u);
                    v = Sse2.ShiftLeftLogical(v, 16);
                    //Vector128<float> vf = Avx.Divide(Vector128.ConvertToSingle(v), Vector128.Create((float)c.A));
                    var left = Sse2.ConvertToVector128Single(v.AsInt32()); //Vector128.ConvertToSingle(v);
                    var a = Vector128.Create((int)c.A, (int)c.A, (int)c.A, (int)c.A); //BUG: Vector128.Create((int)c.A);
                    var right = Sse2.ConvertToVector128Single(a);
                    Vector128<float> vf = Sse.Divide(Sse2.ConvertToVector128Single(v.AsInt32())/*Vector128.ConvertToSingle(v)*/, Sse2.ConvertToVector128Single(Vector128.Create((int)c.A)));
                    var res = Sse.Divide(left, right);
                    //return new Color64(c.A, (ushort)vf[0], (ushort)vf[1], (ushort)vf[2]);
                    Vector128<int> vi = Sse2.ConvertToVector128Int32WithTruncation(vf);
                    return new Color64(c.A, (ushort)vi.GetElement(2), (ushort)vi.GetElement(1), (ushort)vi.GetElement(0));
            }
        }

        #endregion
    }
}

#endif