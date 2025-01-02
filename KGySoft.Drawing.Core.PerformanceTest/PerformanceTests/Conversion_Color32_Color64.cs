#if NETCOREAPP3_0_OR_GREATER
#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: Conversion_Color32_Color64.cs
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
using System.Runtime.Intrinsics.X86;

using KGySoft.Drawing.Imaging;

using NUnit.Framework;

#endregion

namespace KGySoft.Drawing.PerformanceTests
{
    [TestFixture]
    public class Conversion_Color32_Color64
    {
        #region Methods

        [Test]
        public void Color32_Color64_ConversionTest_Div()
        {
            var testColor32 = new Color32(128, 255, 128, 64);
            var testColor64 = testColor32.ToColor64();

            var expected = testColor32.ToColor64_0_ShiftOr().ToColor32_0_Shift_Truncate();
            Assert.AreEqual(expected, testColor64.ToColor32_1_Division());
            //Assert.AreEqual(expected, testColor64.ToColor32_2_Vector64());
            Assert.AreEqual(expected, testColor64.ToColor32_3_Vector128Intrinsics());
            Assert.AreEqual(expected, testColor64.ToColor32_4_Vector128Cast());
            Assert.AreEqual(expected, testColor64.ToColor32_5_Vector64Cast());
            Assert.AreEqual(expected, testColor64.ToColor32_6_BytePointer());

            new PerformanceTest<Color64> { TestName = "Color32 -> Color64", TestTime = 500, Iterations = 10_000_000, Repeat = 3 }
                .AddCase(() => testColor32.ToColor64_0_ShiftOr(), nameof(Extensions.ToColor64_0_ShiftOr))
                .AddCase(() => testColor32.ToColor64_1_Multiplication(), nameof(Extensions.ToColor64_1_Multiplication))
                //.AddCase(() => testColor32.ToColor64_2_Vector64(), nameof(Extensions.ToColor64_2_Vector64))
                .AddCase(() => testColor32.ToColor64_3_Vector128Intrinsics(), nameof(Extensions.ToColor64_3_Vector128Intrinsics))
                .DoTest()
                .DumpResults(Console.Out);

            new PerformanceTest<Color32> { TestName = "Color64 -> Color32", TestTime = 500, Iterations = 10_000_000, Repeat = 3 }
                .AddCase(() => testColor64.ToColor32_0_Shift_Truncate(), nameof(Extensions.ToColor32_0_Shift_Truncate))
                .AddCase(() => testColor64.ToColor32_1_Division(), nameof(Extensions.ToColor32_1_Division))
                //.AddCase(() => testColor64.ToColor32_2_Vector64(), nameof(Extensions.ToColor32_2_Vector64))
                .AddCase(() => testColor64.ToColor32_3_Vector128Intrinsics(), nameof(Extensions.ToColor32_3_Vector128Intrinsics))
                .AddCase(() => testColor64.ToColor32_4_Vector128Cast(), nameof(Extensions.ToColor32_4_Vector128Cast))
                .AddCase(() => testColor64.ToColor32_5_Vector64Cast(), nameof(Extensions.ToColor32_5_Vector64Cast))
                .AddCase(() => testColor64.ToColor32_6_BytePointer(), nameof(Extensions.ToColor32_6_BytePointer))
                .DoTest()
                .DumpResults(Console.Out);
        }

        #endregion
    }

    internal static partial class Extensions
    {
        #region Methods

        internal static Color64 ToColor64_0_ShiftOr(this Color32 c) => new Color64(
            (ushort)((c.A << 8) | c.A),
            (ushort)((c.R << 8) | c.R),
            (ushort)((c.G << 8) | c.G),
            (ushort)((c.B << 8) | c.B));

        internal static Color64 ToColor64_1_Multiplication(this Color32 c) => new Color64(
            ColorSpaceHelper.ToUInt16(c.A),
            ColorSpaceHelper.ToUInt16(c.R),
            ColorSpaceHelper.ToUInt16(c.G),
            ColorSpaceHelper.ToUInt16(c.B));

        //internal static Color64 ToColor64_2_Vector64(this Color32 c)
        //{
        //    var v = Vector64.Create((ushort)c.B, c.G, c.R, c.A);
        //    v *= 257;
        //    return new Color64(v.As<ushort, ulong>()[0]);
        //}

        internal static Color64 ToColor64_3_Vector128Intrinsics(this Color32 c)
        {
            Vector128<ushort> v = Sse41.ConvertToVector128Int16(Vector128.CreateScalarUnsafe(c.Value).AsByte()).AsUInt16();
            v = Sse2.MultiplyLow(v, Vector128.Create((ushort)257));
            return new Color64(v.AsUInt64().ToScalar());
        }

        ///////////////////////////////////////////////////////////////////////////////////////

        internal static Color32 ToColor32_0_Shift_Truncate(this Color64 c) => new Color32(
            (byte)(c.A >> 8),
            (byte)(c.R >> 8),
            (byte)(c.G >> 8),
            (byte)(c.B >> 8));

        internal static Color32 ToColor32_1_Division(this Color64 c) => new Color32(
            (byte)(c.A * 255 / 65535),
            (byte)(c.R * 255 / 65535),
            (byte)(c.G * 255 / 65535),
            (byte)(c.B * 255 / 65535));

        //internal static Color32 ToColor32_2_Vector64(this Color64 c)
        //{
        //    var v = Vector64.ShiftRightLogical(Vector64.CreateScalar(c.Value).As<ulong, ushort>(), 8);
        //    return new Color32(
        //        (byte)v.GetElement(3),
        //        (byte)v.GetElement(2),
        //        (byte)v.GetElement(1),
        //        (byte)v.GetElement(0));
        //}

        internal static Color32 ToColor32_3_Vector128Intrinsics(this Color64 c)
        {
            Vector128<ushort> v = Vector128.CreateScalarUnsafe(c.Value).AsUInt16();
            v = Sse2.ShiftRightLogical(v, 8);
            return new Color32((byte)v.GetElement(3), (byte)v.GetElement(2), (byte)v.GetElement(1), (byte)v.GetElement(0));
        }

        internal static Color32 ToColor32_4_Vector128Cast(this Color64 c)
        {
            Vector128<byte> v = Vector128.CreateScalar(c.Value).AsByte();
            return new Color32(v.GetElement(7), v.GetElement(5), v.GetElement(3), v.GetElement(1));
        }

        internal static Color32 ToColor32_5_Vector64Cast(this Color64 c)
        {
            Vector64<byte> v = Vector64.CreateScalar(c.Value).AsByte();
            return new Color32(v.GetElement(7), v.GetElement(5), v.GetElement(3), v.GetElement(1));
        }

        internal unsafe static Color32 ToColor32_6_BytePointer(this Color64 c)
        {
            byte* v = (byte*)&c;
            return new Color32(v[7], v[5], v[3], v[1]);
        }

        #endregion
    }
}

#endif