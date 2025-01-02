#if NETCOREAPP3_0_OR_GREATER
#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: Conversion_Color32_PColor64.cs
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
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

using KGySoft.Drawing.Imaging;

using NUnit.Framework;

#endregion

namespace KGySoft.Drawing.PerformanceTests
{
    [TestFixture]
    public class Conversion_Color32_PColor64
    {
        #region Methods

        [Test]
        public void Color32_PColor64_ConversionTest()
        {
            var testColor32 = new Color32(128, 255, 128, 64);
            //var testColor32 = new Color32(254, 255, 254, 253);
            var testPColor64 = testColor32.ToPColor64();
            Console.WriteLine($"{"Test color:",-40} {testColor32}");

            void DoAssert(Expression<Func<Color32, Color32>> e)
            {
                var m2 = (MethodCallExpression)e.Body;
                var m1 = (MethodCallExpression)m2.Arguments[0];
                Color32 actual = e.Compile().Invoke(testColor32);
                Console.WriteLine($"{$"{m1.Method.Name}.{m2.Method.Name}:",-40} {actual}");
                Assert.IsTrue(testColor32.TolerantEquals(actual, 1, 0), $"{m1.Method.Name}.{m2.Method.Name}: {testColor32} vs. {actual}");
            }

            DoAssert(_ => testColor32.ToPColor64().ToColor32_0_VanillaIndirect());
            DoAssert(_ => testColor32.ToPColor64().ToColor32_1_VanillaDirect());
            DoAssert(_ => testColor32.ToPColor64().ToColor32_2_IntrinsicGetElement());
            DoAssert(_ => testColor32.ToPColor64().ToColor32_3_IntrinsicShuffle());

            new PerformanceTest<Color32> { TestName = "PColor64 -> Color32", TestTime = 500, Iterations = 10_000_000, Repeat = 3 }
                .AddCase(() => testPColor64.ToColor32_0_VanillaIndirect(), nameof(Extensions.ToColor32_0_VanillaIndirect))
                .AddCase(() => testPColor64.ToColor32_1_VanillaDirect(), nameof(Extensions.ToColor32_1_VanillaDirect))
                .AddCase(() => testPColor64.ToColor32_2_IntrinsicGetElement(), nameof(Extensions.ToColor32_2_IntrinsicGetElement))
                .AddCase(() => testPColor64.ToColor32_3_IntrinsicShuffle(), nameof(Extensions.ToColor32_3_IntrinsicShuffle))
                .DoTest()
                .DumpResults(Console.Out);
        }

        #endregion
    }

    internal static partial class Extensions
    {
        #region Methods

        internal static Color32 ToColor32_0_VanillaIndirect(this PColor64 c) => c.ToColor64_0_Div().ToColor32_0_Shift_Truncate();

        internal static unsafe Color32 ToColor32_1_VanillaDirect(this PColor64 c)
        {
            switch (c.A)
            {
                case UInt16.MaxValue:
                    ulong bgraU16 = c.Value;
                    byte* bytes = (byte*)&bgraU16;
                    return new Color32(bytes[7], bytes[5], bytes[3], bytes[1]);

                case UInt16.MinValue:
                    return default;

                default:
                    return new Color32((byte)(c.A >> 8),
                        (byte)(((uint)c.R * UInt16.MaxValue / c.A) >> 8),
                        (byte)(((uint)c.G * UInt16.MaxValue / c.A) >> 8),
                        (byte)(((uint)c.B * UInt16.MaxValue / c.A) >> 8));
            }
        }

        internal static Color32 ToColor32_2_IntrinsicGetElement(this PColor64 c)
        {
            Vector128<float> bgraF = Sse2.ConvertToVector128Single(Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(c.Value).AsUInt16()));

            bgraF = Sse.Divide(bgraF, Sse2.ConvertToVector128Single(Sse41.IsSupported
                ? Sse41.ConvertToVector128Int32(Vector128.Create(c.A))
                : Vector128.Create((int)c.A)));

            bgraF = Sse.Multiply(bgraF, Vector128.Create(65535f));

            Vector128<byte> bgraI32 = Sse2.ConvertToVector128Int32(bgraF).AsUInt16().WithElement(6, c.A).AsByte();

            return new Color32(bgraI32.GetElement(13),
                bgraI32.GetElement(9),
                bgraI32.GetElement(5),
                bgraI32.GetElement(1));
        }

        internal static Color32 ToColor32_3_IntrinsicShuffle(this PColor64 c)
        {
            Vector128<float> bgraF = Sse2.ConvertToVector128Single(Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(c.Value).AsUInt16()));

            bgraF = Sse.Divide(bgraF, Sse2.ConvertToVector128Single(Sse41.IsSupported
                ? Sse41.ConvertToVector128Int32(Vector128.Create(c.A))
                : Vector128.Create((int)c.A)));

            bgraF = Sse.Multiply(bgraF, Vector128.Create(65535f));

            Vector128<byte> bgraI32 = Sse2.ConvertToVector128Int32(bgraF).AsUInt16().WithElement(6, c.A).AsByte();

            return new Color32(Ssse3.Shuffle(bgraI32,
                Vector128.Create(1, 5, 9, 13, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF)).AsUInt32().ToScalar());
        }

        #endregion
    }
}

#endif