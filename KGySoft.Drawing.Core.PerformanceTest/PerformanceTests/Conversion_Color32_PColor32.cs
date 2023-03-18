#if NETCOREAPP3_0_OR_GREATER
#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: Conversion_Color32_PColor32.cs
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
        public void Color32_PColor32_ConversionTest_Div()
        {
            var testColor32 = new Color32(128, 255, 128, 64);
            //var testColor32 = new Color32(254, 255, 254, 253);
            var testPColor32 = testColor32.ToPColor32();
            Console.WriteLine($"{"Test color:",-80} {testColor32}");

            void DoAssert(Expression<Func<Color32, Color32>> e)
            {
                var m2 = (MethodCallExpression)e.Body;
                var m1 = (MethodCallExpression)m2.Arguments[0];
                Color32 actual = e.Compile().Invoke(testColor32);
                Console.WriteLine($"{$"{m1.Method.Name}.{m2.Method.Name}:",-80} {actual}");
                Assert.IsTrue(testColor32.TolerantEquals(actual, 1), $"{m1.Method.Name}.{m2.Method.Name}: {testColor32} vs. {actual}");
            }

            DoAssert(_ => testColor32.ToPColor32_0_Div().ToColor32_0_Div());
            DoAssert(_ => testColor32.ToPColor32_1_DivVector3().ToColor32_1_DivVector3());
            DoAssert(_ => testColor32.ToPColor32_2_DivVectorTSpan().ToColor32_2_DivVectorTSpan());
            //DoAssert(_ => testColor32.ToPColor32_3_DivVectorTWiden().ToColor32_3_DivVectorTWiden());
            //DoAssert(_ => testColor32.ToPColor32_4_DivVector64().ToColor32_4_DivVector64());
            DoAssert(_ => testColor32.ToPColor32_5_DivIntrinsicsFloat().ToColor32_5_DivIntrinsicsFloat());
            DoAssert(_ => testColor32.ToPColor32_6_Final().ToColor32_6_Final());

            new PerformanceTest<PColor32> { TestName = "Color32 -> PColor32", TestTime = 500, Iterations = 10_000_000, Repeat = 3 }
                .AddCase(() => testColor32.ToPColor32_0_Div(), nameof(Extensions.ToPColor32_0_Div))
                .AddCase(() => testColor32.ToPColor32_1_DivVector3(), nameof(Extensions.ToPColor32_1_DivVector3))
                //.AddCase(() => testColor32.ToPColor32_2_DivVectorTSpan(), nameof(Extensions.ToPColor32_2_DivVectorTSpan))
                //.AddCase(() => testColor32.ToPColor32_3_DivVectorTWiden(), nameof(Extensions.ToPColor32_3_DivVectorTWiden))
                //.AddCase(() => testColor32.ToPColor32_4_DivVector64(), nameof(Extensions.ToPColor32_4_DivVector64))
                .AddCase(() => testColor32.ToPColor32_5_DivIntrinsicsFloat(), nameof(Extensions.ToPColor32_5_DivIntrinsicsFloat))
                .AddCase(() => testColor32.ToPColor32_6_Final(), nameof(Extensions.ToPColor32_6_Final))
                .DoTest()
                .DumpResults(Console.Out);

            new PerformanceTest<Color32> { TestName = "PColor32 -> Color32", TestTime = 500, Iterations = 10_000_000, Repeat = 3 }
                .AddCase(() => testPColor32.ToColor32_0_Div(), nameof(Extensions.ToColor32_0_Div))
                .AddCase(() => testPColor32.ToColor32_1_DivVector3(), nameof(Extensions.ToColor32_1_DivVector3))
                //.AddCase(() => testPColor32.ToColor32_2_DivVectorTSpan(), nameof(Extensions.ToColor32_2_DivVectorTSpan))
                //.AddCase(() => testPColor32.ToColor32_3_DivVectorTWiden(), nameof(Extensions.ToColor32_3_DivVectorTWiden))
                //.AddCase(() => testPColor32.ToColor32_4_DivVector64(), nameof(Extensions.ToColor32_4_DivVector64))
                .AddCase(() => testPColor32.ToColor32_5_DivIntrinsicsFloat(), nameof(Extensions.ToColor32_5_DivIntrinsicsFloat))
                .AddCase(() => testPColor32.ToColor32_6_Final(), nameof(Extensions.ToColor32_6_Final))
                .DoTest()
                .DumpResults(Console.Out);

            new PerformanceTest<Color32> { TestName = "Color32 <-> PColor32", TestTime = 500, Iterations = 10_000_000, Repeat = 3 }
                .AddCase(() => testColor32.ToPColor32_0_Div().ToColor32_0_Div(), nameof(Extensions.ToPColor32_0_Div))
                .AddCase(() => testColor32.ToPColor32_1_DivVector3().ToColor32_1_DivVector3(), nameof(Extensions.ToPColor32_1_DivVector3))
                .AddCase(() => testColor32.ToPColor32_5_DivIntrinsicsFloat().ToColor32_5_DivIntrinsicsFloat(), nameof(Extensions.ToPColor32_5_DivIntrinsicsFloat))
                .AddCase(() => testColor32.ToPColor32_6_Final().ToColor32_6_Final(), nameof(Extensions.ToPColor32_6_Final))
                .DoTest()
                .DumpResults(Console.Out);
        }

        [Test]
        public void Color32_PColor32_ConversionTest_Shift()
        {
            var testColor32 = new Color32(128, 255, 128, 64);
            var testPColor32 = testColor32.ToPColor32_0_Shift();

            var expected = testColor32.ToPColor32_0_Shift().ToColor32_0_Shift();
            //Assert.AreEqual(expected, testColor32.ToPColor32_1_ShiftVector64().ToColor32_1_ShiftVector64());
            Assert.AreEqual(expected, testColor32.ToPColor32_2_ShiftIntrinsics().ToColor32_2_ShiftIntrinsics());

            new PerformanceTest<PColor32> { TestName = "Color32 -> PColor32", TestTime = 500, Iterations = 10_000_000, Repeat = 3 }
                .AddCase(() => testColor32.ToPColor32_0_Shift(), nameof(Extensions.ToPColor32_0_Shift))
                //.AddCase(() => testColor32.ToPColor32_1_ShiftVector64(), nameof(Extensions.ToPColor32_1_ShiftVector64))
                .AddCase(() => testColor32.ToPColor32_2_ShiftIntrinsics(), nameof(Extensions.ToPColor32_2_ShiftIntrinsics))
                .DoTest()
                .DumpResults(Console.Out);

            new PerformanceTest<Color32> { TestName = "PColor32 -> Color32", TestTime = 500, Iterations = 10_000_000, Repeat = 3 }
                .AddCase(() => testPColor32.ToColor32_0_Shift(), nameof(Extensions.ToColor32_0_Shift))
                //.AddCase(() => testPColor32.ToColor32_1_ShiftVector64(), nameof(Extensions.ToColor32_1_ShiftVector64))
                .AddCase(() => testPColor32.ToColor32_2_ShiftIntrinsics(), nameof(Extensions.ToColor32_2_ShiftIntrinsics))
                .DoTest()
                .DumpResults(Console.Out);

            new PerformanceTest<Color32> { TestName = "Color32 <-> PColor32", TestTime = 500, Iterations = 10_000_000, Repeat = 3 }
                .AddCase(() => testColor32.ToPColor32_0_Shift().ToColor32_0_Shift(), nameof(Extensions.ToPColor32_0_Shift))
                .AddCase(() => testColor32.ToPColor32_2_ShiftIntrinsics().ToColor32_2_ShiftIntrinsics(), nameof(Extensions.ToPColor32_2_ShiftIntrinsics))
                .DoTest()
                .DumpResults(Console.Out);
        }

        #endregion
    }

    internal static partial class Extensions
    {
        #region Methods

        internal static PColor32 ToPColor32_0_Div(this Color32 c) => c.A switch
        {
            255 => new PColor32(c.Value),
            0 => default,
            _ => new PColor32(c.A,
                (byte)((uint)c.R * c.A / Byte.MaxValue),
                (byte)((uint)c.G * c.A / Byte.MaxValue),
                (byte)((uint)c.B * c.A / Byte.MaxValue)),
        };

        internal static Color32 ToColor32_0_Div(this PColor32 c) => c.A switch
        {
            255 => new Color32(c.Value),
            0 => default,
            _ => new Color32(c.A,
                (byte)((uint)c.R * Byte.MaxValue / c.A),
                (byte)((uint)c.G * Byte.MaxValue / c.A),
                (byte)((uint)c.B * Byte.MaxValue / c.A)),
        };

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

        internal static Color32 ToColor32_2_DivVectorTSpan(this PColor32 c)
        {
            switch (c.A)
            {
                case 255:
                    return new Color32(c.Value);
                case 0:
                    return default;
                default:
                    Span<ushort> span = stackalloc ushort[Vector<ushort>.Count];
                    span[0] = c.R;
                    span[1] = c.G;
                    span[2] = c.B;
                    Vector<ushort> v = new Vector<ushort>(span) * 255 / new Vector<ushort>(c.A);
                    return new Color32(c.A, (byte)v[0], (byte)v[1], (byte)v[2]);
            }
        }

        //internal static PColor32 ToPColor32_3_DivVectorTWiden(this Color32 c)
        //{
        //    switch (c.A)
        //    {
        //        case 255:
        //            return new PColor32(c.Value);
        //        case 0:
        //            return default;
        //        default:
        //            Vector.Widen(new Vector<uint>(c.Value).As<uint, byte>(), out Vector<ushort> v, out var _);
        //            v = v * c.A / new Vector<ushort>(255);
        //            return new PColor32(c.A, (byte)v[0], (byte)v[1], (byte)v[2]);
        //    }
        //}

        //internal static Color32 ToColor32_3_DivVectorTWiden(this PColor32 c)
        //{
        //    switch (c.A)
        //    {
        //        case 255:
        //            return new Color32(c.Value);
        //        case 0:
        //            return default;
        //        default:
        //            Vector.Widen(new Vector<uint>(c.Value).As<uint, byte>(), out Vector<ushort> v, out var _);
        //            v = v * 255 / new Vector<ushort>(c.A);
        //            return new Color32(c.A, (byte)v[0], (byte)v[1], (byte)v[2]);
        //    }
        //}

        //internal static PColor32 ToPColor32_4_DivVector64(this Color32 c)
        //{
        //    switch (c.A)
        //    {
        //        case 255:
        //            return new PColor32(c.Value);
        //        case 0:
        //            return default;
        //        default:
        //            Vector64<ushort> v = Vector64.Create(c.R, c.G, c.B, (ushort)0);
        //            v = v * c.A / Vector64.Create((ushort)255);
        //            return new PColor32(c.A, (byte)v[0], (byte)v[1], (byte)v[2]);
        //    }
        //}

        //internal static Color32 ToColor32_4_DivVector64(this PColor32 c)
        //{
        //    switch (c.A)
        //    {
        //        case 255:
        //            return new Color32(c.Value);
        //        case 0:
        //            return default;
        //        default:
        //            Vector64<ushort> v = Vector64.Create(c.R, c.G, c.B, (ushort)0);
        //            v = v * 255 / Vector64.Create((ushort)c.A);
        //            return new Color32(c.A, (byte)v[0], (byte)v[1], (byte)v[2]);
        //    }
        //}

        internal static PColor32 ToPColor32_5_DivIntrinsicsFloat(this Color32 c)
        {
            switch (c.A)
            {
                case 255:
                    return new PColor32(c.Value);
                case 0:
                    return default;
                default:
                    //Vector128<float> v = Vector128.Create(c.R, c.G, c.B, 0f);
                    Vector128<float> v = Sse2.ConvertToVector128Single(Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(c.Value).As<uint, byte>()));
                    //v = Avx.Multiply(v, Vector128.Create((float)c.A));
                    v = Sse.Multiply(v, Sse2.ConvertToVector128Single(Sse41.ConvertToVector128Int32(Vector128.Create(c.A))));
                    v = Sse.Divide(v, Vector128.Create(255f));
                    Vector128<int> vi = Sse2.ConvertToVector128Int32WithTruncation(v);
                    return new PColor32(c.A, (byte)vi.GetElement(2), (byte)vi.GetElement(1), (byte)vi.GetElement(0));
            }
        }

        internal static Color32 ToColor32_5_DivIntrinsicsFloat(this PColor32 c)
        {
            switch (c.A)
            {
                case 255:
                    return new Color32(c.Value);
                case 0:
                    return default;
                default:
                    //Vector128<float> v = Vector128.Create(c.R, c.G, c.B, 0f);
                    Vector128<float> v = Sse2.ConvertToVector128Single(Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(c.Value).As<uint, byte>()));
                    v = Sse.Multiply(v, Vector128.Create(255f));
                    //v = Avx.Divide(v, Vector128.Create((float)c.A));
                    v = Sse.Divide(v, Sse2.ConvertToVector128Single(Sse41.ConvertToVector128Int32(Vector128.Create(c.A))));
                    Vector128<int> vi = Sse2.ConvertToVector128Int32WithTruncation(v);
                    return new Color32(c.A, (byte)vi.GetElement(2), (byte)vi.GetElement(1), (byte)vi.GetElement(0));
            }
        }

        internal static PColor32 ToPColor32_6_Final(this Color32 c)
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
                        Vector128<float> bgraF = Sse2.ConvertToVector128Single(Sse41.IsSupported
                            ? Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(c.Value).AsByte())
                            : Vector128.Create(c.B, c.G, c.R, default));

                        bgraF = Sse.Multiply(bgraF, Sse2.ConvertToVector128Single(Sse41.IsSupported
                            ? Sse41.ConvertToVector128Int32(Vector128.Create(c.A))
                            : Vector128.Create((int)c.A)));

                        bgraF = Sse.Multiply(bgraF, Vector128.Create(1f / 255f));

                        Vector128<int> bgraI32 = Sse2.ConvertToVector128Int32(bgraF);
                        if (Ssse3.IsSupported)
                            return new PColor32(Ssse3.Shuffle(bgraI32.AsByte().WithElement(12, c.A),
                                Vector128.Create(0, 4, 8, 12, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF)).AsUInt32().ToScalar());

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

        internal static Color32 ToColor32_6_Final(this PColor32 c)
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
                        Vector128<float> bgraF = Sse2.ConvertToVector128Single(Sse41.IsSupported
                            ? Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(c.Value).AsByte())
                            : Vector128.Create(c.B, c.G, c.R, default));
                        bgraF = Sse.Multiply(bgraF, Vector128.Create(255f));

                        //bgraF = Sse.Multiply(bgraF, Sse.Reciprocal(Sse2.ConvertToVector128Single(Sse41.IsSupported
                        //    ? Sse41.ConvertToVector128Int32(Vector128.Create(c.A))
                        //    : Vector128.Create((int)c.A))));
                        bgraF = Sse.Divide(bgraF, Sse2.ConvertToVector128Single(Sse41.IsSupported
                            ? Sse41.ConvertToVector128Int32(Vector128.Create(c.A))
                            : Vector128.Create((int)c.A)));

                        Vector128<int> bgraI32 = Sse2.ConvertToVector128Int32(bgraF);
                        if (Ssse3.IsSupported)
                            return new Color32(Ssse3.Shuffle(bgraI32.AsByte().WithElement(12, c.A),
                                Vector128.Create(0, 4, 8, 12, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF)).AsUInt32().ToScalar());
                    }

                    return new Color32(c.A,
                        (byte)(c.R * Byte.MaxValue / c.A),
                        (byte)(c.G * Byte.MaxValue / c.A),
                        (byte)(c.B * Byte.MaxValue / c.A));
            }
        }

        internal static PColor32 ToPColor32_0_Shift(this Color32 c) => c.A switch
        {
            255 => new PColor32(c.Value),
            0 => default,
            _ => new PColor32(c.A,
                (byte)((c.R * c.A) >> 8),
                (byte)((c.G * c.A) >> 8),
                (byte)((c.B * c.A) >> 8)),
        };

        internal static Color32 ToColor32_0_Shift(this PColor32 c) => c.A switch
        {
            255 => new Color32(c.Value),
            0 => default,
            _ => new Color32(c.A,
                (byte)(((uint)c.R << 8) / c.A),
                (byte)(((uint)c.G << 8) / c.A),
                (byte)(((uint)c.B << 8) / c.A)),
        };

        //internal static PColor32 ToPColor32_1_ShiftVector64(this Color32 c)
        //{
        //    switch (c.A)
        //    {
        //        case 255:
        //            return new PColor32(c.Value);
        //        case 0:
        //            return default;
        //        default:
        //            Vector64<ushort> v = Vector64.Create(c.R, c.G, c.B, (ushort)0);
        //            v *= c.A;
        //            v = Vector64.ShiftRightLogical(v, 8);
        //            return new PColor32(c.A, (byte)v[0], (byte)v[1], (byte)v[2]);
        //    }
        //}

        //internal static Color32 ToColor32_1_ShiftVector64(this PColor32 c)
        //{
        //    switch (c.A)
        //    {
        //        case 255:
        //            return new Color32(c.Value);
        //        case 0:
        //            return default;
        //        default:
        //            Vector64<ushort> v = Vector64.Create(c.R, c.G, c.B, (ushort)0);
        //            v = Vector64.ShiftLeft(v, 8);
        //            v /= Vector64.Create((ushort)c.A);
        //            return new Color32(c.A, (byte)v[0], (byte)v[1], (byte)v[2]);
        //    }
        //}

        internal static PColor32 ToPColor32_2_ShiftIntrinsics(this Color32 c)
        {
            switch (c.A)
            {
                case 255:
                    return new PColor32(c.Value);
                case 0:
                    return default;
                default:
                    Vector128<int> v = Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(c.Value).As<uint, byte>());
                    //Vector128<uint> v = Vector128.Create(c.R, c.G, c.B, 0u);
                    v = Sse41.MultiplyLow(v, Vector128.Create((int)c.A));
                    v = Sse2.ShiftRightLogical(v, 8);
                    return new PColor32(c.A, (byte)v.GetElement(2), (byte)v.GetElement(1), (byte)v.GetElement(0));
            }
        }

        internal static Color32 ToColor32_2_ShiftIntrinsics(this PColor32 c)
        {
            switch (c.A)
            {
                case 255:
                    return new Color32(c.Value);
                case 0:
                    return default;
                default:
                    Vector128<int> v = Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(c.Value).As<uint, byte>());
                    //Vector128<uint> v = Vector128.Create(c.R, c.G, c.B, 0u);
                    v = Sse2.ShiftLeftLogical(v, 8);
                    //Vector128<float> vf = Avx.Divide(Vector128.ConvertToSingle(v), Vector128.Create((float)c.A));
                    Vector128<float> vf = Sse.Divide(Sse2.ConvertToVector128Single(v), Sse2.ConvertToVector128Single(Vector128.Create((int)c.A)));
                    //return new Color32(c.A, (byte)vf[0], (byte)vf[1], (byte)vf[2]);
                    Vector128<int> vi = Sse2.ConvertToVector128Int32WithTruncation(vf);
                    return new Color32(c.A, (byte)vi.GetElement(2), (byte)vi.GetElement(1), (byte)vi.GetElement(0));
            }
        }

        #endregion
    }
}

#endif