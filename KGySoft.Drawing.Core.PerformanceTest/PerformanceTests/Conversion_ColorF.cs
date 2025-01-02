#if NETCOREAPP3_0_OR_GREATER
#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: Conversion_ColorF.cs
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
    public class Conversion_ColorF
    {
        #region Methods

        [TestCase(0.5f, 0.5f, 0.25f, 0.125f)] // Pow
        //[TestCase(0.5f, 0.001f, 0.002f, 0.003f)] // Linear conversion
        //[TestCase(0.5f, 1f, 0.5f, 0.25f)] // Different channels, 2 pow
        //[TestCase(0.5f, 1f, 1f, 0f)] // Different channels, 0 pow
        public void LinearToSrgbTest(float a, float r, float g, float b)
        {
            var linear = new ColorF(r, g, b, a);
            var srgb = new ColorF(r, g, b, a).ToSrgb();

            Console.WriteLine($"{"Expected color:",-40} {srgb}");
            void DoAssert(Expression<Func<ColorF, ColorF>> e)
            {
                var m = (MethodCallExpression)e.Body;
                ColorF actual = e.Compile().Invoke(linear);
                Console.WriteLine($"{$"{m.Method.Name}:",-40} {actual}");
                Assert.IsTrue(srgb.TolerantEquals(actual), $"{m.Method.Name}: {srgb} vs. {actual}");
            }

            DoAssert(_ => linear.ToSrgb_0_Vanilla());
            DoAssert(_ => linear.ToSrgb_1_Vector_a_VanillaCompare());
#if NET7_0_OR_GREATER
            DoAssert(_ => linear.ToSrgb_1_Vector_b_IntrinsicCompare());
#endif
            DoAssert(_ => linear.ToSrgb_2_Intrinsics());

            new PerformanceTest { TestName = "Linear to sRGB", TestTime = 5000, Iterations = 10_000_000, Repeat = 3 }
                .AddCase(() => linear.ToSrgb_0_Vanilla(), nameof(Extensions.ToSrgb_0_Vanilla))
                .AddCase(() => linear.ToSrgb_1_Vector_a_VanillaCompare(), nameof(Extensions.ToSrgb_1_Vector_a_VanillaCompare))
#if NET7_0_OR_GREATER
                .AddCase(() => linear.ToSrgb_1_Vector_b_IntrinsicCompare(), nameof(Extensions.ToSrgb_1_Vector_b_IntrinsicCompare))
#endif
                .AddCase(() => linear.ToSrgb_2_Intrinsics(), nameof(Extensions.ToSrgb_2_Intrinsics))
                .DoTest()
                .DumpResults(Console.Out);

        }

        [Test]
        public void Color32_ColorF_ConversionTest()
        {
            var testColor32 = new Color32(128, 128, 64, 32);

            Console.WriteLine($"{"Test color:",-40} {testColor32}");
            void DoAssert(Expression<Func<Color32, Color32>> e)
            {
                var m2 = (MethodCallExpression)e.Body;
                var m1 = (MethodCallExpression)m2.Arguments[0];
                Color32 actual = e.Compile().Invoke(testColor32);
                Console.WriteLine($"{$"{m1.Method.Name}.{m2.Method.Name}:",-40} {actual}");
                Assert.IsTrue(testColor32.TolerantEquals(actual, 1, 0), $"{m1.Method.Name}.{m2.Method.Name}: {testColor32} vs. {actual}");
            }

            DoAssert(_ => testColor32.ToColorF_0_VanillaUncached().ToColor32_0_Vanilla());
            DoAssert(_ => testColor32.ToColorF_0b_VanillaCachedFloatConv().ToColor32_0_Vanilla());

            new PerformanceTest<ColorF> { TestName = "Color32 -> ColorF", TestTime = 500, Iterations = 10_000_000, Repeat = 3 }
                .AddCase(() => testColor32.ToColorF_0_VanillaUncached(), nameof(Extensions.ToColorF_0_VanillaUncached))
                .AddCase(() => testColor32.ToColorF_0b_VanillaCachedFloatConv(), nameof(Extensions.ToColorF_0b_VanillaCachedFloatConv))
                .DoTest()
                .DumpResults(Console.Out);
        }

        [TestCase(0.5f, 0.5f, 0.25f, 0.125f)] // Pow x3
        //[TestCase(0.5f, 0.001f, 0.002f, 0.003f)] // Linear conversion
        //[TestCase(0.5f, 1f, 0.5f, 0.25f)] // Different channels, pow x2
        //[TestCase(0.5f, 1f, 1f, 0f)] // Different channels, 0 pow
        public void ColorF_Color32_ConversionTest(float a, float r, float g, float b)
        {
            var testColorF = new ColorF(a, r, g, b);
            var testColor32 = testColorF.ToColor32();

            Console.WriteLine($"{"Expected color:",-40} {testColor32}");
            void DoAssert(Expression<Func<ColorF, Color32>> e)
            {
                var m = (MethodCallExpression)e.Body;
                Color32 actual = e.Compile().Invoke(testColorF);
                Console.WriteLine($"{$"{m.Method.Name}:",-40} {actual}");
                Assert.IsTrue(testColor32.Equals(actual), $"{m.Method.Name}: {testColor32} vs. {actual}");
            }

            DoAssert(_ => testColorF.ToColor32_0_Vanilla());
            DoAssert(_ => testColorF.ToColor32_1_Vector());
            DoAssert(_ => testColorF.ToColor32_2_IntrinsicAll());
            DoAssert(_ => testColorF.ToColor32_3_ToSrgbVanillaFirst());
            DoAssert(_ => testColorF.ToColor32_4_ToSrgbIntrinsicsFirst());

            new PerformanceTest<Color32> { TestName = "ColorF -> Color32", TestTime = 5000, Iterations = 10_000_000, Repeat = 3 }
                .AddCase(() => testColorF.ToColor32_0_Vanilla(), nameof(Extensions.ToColor32_0_Vanilla))
                .AddCase(() => testColorF.ToColor32_1_Vector(), nameof(Extensions.ToColor32_1_Vector))
                .AddCase(() => testColorF.ToColor32_2_IntrinsicAll(), nameof(Extensions.ToColor32_2_IntrinsicAll))
                .AddCase(() => testColorF.ToColor32_3_ToSrgbVanillaFirst(), nameof(Extensions.ToColor32_3_ToSrgbVanillaFirst))
                .AddCase(() => testColorF.ToColor32_4_ToSrgbIntrinsicsFirst(), nameof(Extensions.ToColor32_4_ToSrgbIntrinsicsFirst))
                .DoTest()
                .DumpResults(Console.Out);
        }

        [TestCase(128 * 257, 128 * 257, 64 * 257, 32 * 257)] // Pow x3, Color32 compatible colors
        //[TestCase(128 * 257, 1 * 257, 2 * 257, 3 * 257)] // Linear conversion, Color32 compatible colors
        //[TestCase(254 * 257, 254 * 257, 253 * 257, 252 * 257)]
        //[TestCase(65534, 65534, 65533, 65532)] 
        //[TestCase(32768, 1, 2, 3)]
        public void ColorF_Color64_ConversionTest(int a, int r, int g, int b)
        {
            var testColor64 = new Color64((ushort)a, (ushort)r, (ushort)g, (ushort)b);
            //var testColor32 = new Color32(128, 1, 2, 3);
            var testColorF = testColor64.ToColorF();

            Console.WriteLine($"{"Expected color:",-40} {testColor64}");
            void DoAssert(Expression<Func<ColorF, Color64>> e)
            {
                var m = (MethodCallExpression)e.Body;
                Color64 actual = e.Compile().Invoke(testColorF);
                Console.WriteLine($"{$"{m.Method.Name}:",-40} {actual}");
                Assert.IsTrue(testColor64.Equals(actual), $"{m.Method.Name}: {testColor64} vs. {actual}");
            }

            DoAssert(_ => testColorF.ToColor64_0_Vanilla());
            DoAssert(_ => testColorF.ToColor64_1_Vector());
            DoAssert(_ => testColorF.ToColor64_2_ToSrgbVanillaFirst());

            new PerformanceTest<Color64> { TestName = "ColorF -> Color32", TestTime = 5000, Iterations = 10_000_000, Repeat = 3 }
                .AddCase(() => testColorF.ToColor64_0_Vanilla(), nameof(Extensions.ToColor64_0_Vanilla))
                .AddCase(() => testColorF.ToColor64_1_Vector(), nameof(Extensions.ToColor64_1_Vector))
                .AddCase(() => testColorF.ToColor64_2_ToSrgbVanillaFirst(), nameof(Extensions.ToColor64_2_ToSrgbVanillaFirst))
                .DoTest()
                .DumpResults(Console.Out);
        }

        #endregion
    }

    internal static partial class Extensions
    {
        #region Properties

#if !NET7_0_OR_GREATER
#if NET5_0_OR_GREATER
        private static Vector128<ulong> AllBitsSet => Vector128<ulong>.AllBitsSet;
#else
        private static Vector128<ulong> AllBitsSet => Vector128.Create(UInt64.MaxValue);
#endif
#endif

#if NET5_0_OR_GREATER
        private static Vector128<byte> PackRgbaAsColor32Mask => Vector128.Create(8, 4, 0, 12, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF);
        private static Vector128<byte> PackRgbaAsColor64Mask => Vector128.Create(8, 9, 4, 5, 0, 1, 12, 13, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF);
#else
        private static Vector128<byte> PackRgbaAsColor32Mask { get; } = Vector128.Create(8, 4, 0, 12, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF);
        private static Vector128<byte> PackRgbaAsColor64Mask { get; } = Vector128.Create(8, 9, 4, 5, 0, 1, 12, 13, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF);
#endif

        #endregion

        #region Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static ColorF ToSrgb_0_Vanilla(this ColorF c)
        {
            var rgba = c.RgbaV128;
            return new ColorF(Vector128.Create(ColorSpaceHelper.LinearToSrgb(rgba.GetElement(0)),
                ColorSpaceHelper.LinearToSrgb(rgba.GetElement(1)),
                ColorSpaceHelper.LinearToSrgb(rgba.GetElement(2)),
                rgba.GetElement(3).ClipF()));
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static bool GreaterThanAll(this Vector3 v, float value) => v.X > value && v.Y > value && v.Z > value;

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static bool LessThanAll(this Vector3 v, float value) => v.X < value && v.Y < value && v.Z < value;

        internal static Vector128<float> AsVector128(this Vector3 v) => Vector128.Create(v.X, v.Y, v.Z, default);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static ColorF ToSrgb_1_Vector_a_VanillaCompare(this ColorF c)
        {
            if (c.Rgb.GreaterThanAll(0.0031308f))
            {
                if (c.Rgb.LessThanAll(1f))
                {
                    var result = new Vector3(MathF.Pow(c.R, 1f / 2.4f), MathF.Pow(c.G, 1f / 2.4f), MathF.Pow(c.B, 1f / 2.4f));
                    result = (result * 1.055f - new Vector3(0.055f));
                    return new ColorF(new Vector4(result, c.A));
                }
            }
            else if (c.Rgb.GreaterThanAll(0f))
            {
                return new ColorF(new Vector4(c.Rgb * 12.92f, c.A));
            }

            return c.ToSrgb_0_Vanilla();
        }

#if NET7_0_OR_GREATER
        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static ColorF ToSrgb_1_Vector_b_IntrinsicCompare(this ColorF c)
        {
            Vector128<float> rgbx = c.RgbaV128.WithElement(3, 1f);
            //if (c.Rgb.GreaterThanAll(0.0031308f))
            if (Vector128.GreaterThanAll(rgbx, Vector128.Create(0.0031308f)))
            {
                //if (c.Rgb.LessThanAll(1f))
                if (Vector128.LessThanAll(rgbx.WithElement(3, 0f), Vector4.One.AsVector128()))
                {
                    var result = new Vector3(MathF.Pow(c.R, 1f / 2.4f), MathF.Pow(c.G, 1f / 2.4f), MathF.Pow(c.B, 1f / 2.4f));
                    result = (result * 1.055f - new Vector3(0.055f));
                    return new ColorF(new Vector4(result, c.A));
                }
            }
            //else if (c.Rgb.GreaterThanAll(0f))
            else if (Vector128.GreaterThanAll(rgbx, Vector128<float>.Zero))
            {
                return new ColorF(new Vector4(c.Rgb * 12.92f, c.A));
            }

            return c.ToSrgb_0_Vanilla();
        }
#endif

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static ColorF ToSrgb_2_Intrinsics(this ColorF c)
        {
            if (Sse.IsSupported)
            {
                Vector128<float> rgbx = c.RgbaV128.WithElement(3, 1f);
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
                        Vector128<float> resultF = Vector128.Create(
                            MathF.Pow(c.R, 1f / 2.4f),
                            MathF.Pow(c.G, 1f / 2.4f),
                            MathF.Pow(c.B, 1f / 2.4f),
                            0f);
                        if (Fma.IsSupported)
                        {
                            resultF = Fma.MultiplyAdd(resultF, Vector128.Create(1.055f), Vector128.Create(-0.055f));
                        }
                        else
                        {
                            resultF = Sse.Multiply(resultF, Vector128.Create(1.055f));
                            resultF = Sse.Subtract(resultF, Vector128.Create(0.055f));
                        }

                        return new ColorF(resultF.WithElement(3, c.A.ClipF()));
                    }
                }
#if NET7_0_OR_GREATER
                else if (Vector128.GreaterThanAll(rgbx, Vector128<float>.Zero))
#else
                else if (Sse.CompareGreaterThan(rgbx, Vector128<float>.Zero).AsUInt64().Equals(AllBitsSet))
#endif
                {
                    return new ColorF(Sse.Multiply(c.RgbaV128, Vector128.Create(12.92f)).WithElement(3, c.A.ClipF()));
                }
            }

            return c.ToSrgb_0_Vanilla();
        }

        internal static ColorF ToColorF_0_VanillaUncached(this Color32 c)
            => new ColorF(c.A / 255f,
                ColorSpaceHelper.SrgbToLinear(c.R / 255f),
                ColorSpaceHelper.SrgbToLinear(c.G / 255f),
                ColorSpaceHelper.SrgbToLinear(c.B / 255f));

        internal static ColorF ToColorF_0b_VanillaCachedFloatConv(this Color32 c)
            => new ColorF(ColorSpaceHelper.ToFloat(c.A),
                ColorSpaceHelper.SrgbToLinear(c.R),
                ColorSpaceHelper.SrgbToLinear(c.G),
                ColorSpaceHelper.SrgbToLinear(c.B));

        internal static Color32 ToColor32_0_Vanilla(this ColorF c)
            // NOTE: this is even more vanilla than ToSrgb_0_Vanilla, which uses 128-bit vector creation
            => new Color32(ColorSpaceHelper.ToByte(c.A),
                ColorSpaceHelper.LinearToSrgb8Bit(c.R),
                ColorSpaceHelper.LinearToSrgb8Bit(c.G),
                ColorSpaceHelper.LinearToSrgb8Bit(c.B));

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Color32 ToColor32_1_Vector(this ColorF c)
        {
            // NOTE: On .NET 4.6 it's still slower than vanilla. On .NET Core 2.0 it's just a bit faster.
            if (c.Rgb.GreaterThanAll(0.0031308f))
            {
                if (c.Rgb.LessThanAll(1f))
                {
                    Vector3 result = new Vector3(MathF.Pow(c.R, 1f / 2.4f), MathF.Pow(c.G, 1f / 2.4f), MathF.Pow(c.B, 1f / 2.4f));
                    result = (result * 1.055f - new Vector3(0.055f)) * 255f + new Vector3(0.5f);
                    return new Color32(ColorSpaceHelper.ToByte(c.A), (byte)result.X, (byte)result.Y, (byte)result.Z);
                }
            }
            else if (c.Rgb.GreaterThanAll(0f))
            {
                Vector3 result = c.Rgb * (Byte.MaxValue * 12.92f) + new Vector3(0.5f);
                return new Color32(ColorSpaceHelper.ToByte(c.A), (byte)result.X, (byte)result.Y, (byte)result.Z);
            }

            return new Color32(ColorSpaceHelper.ToByte(c.A),
                ColorSpaceHelper.LinearToSrgb8Bit(c.R),
                ColorSpaceHelper.LinearToSrgb8Bit(c.G),
                ColorSpaceHelper.LinearToSrgb8Bit(c.B));
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Color32 ToColor32_2_IntrinsicAll(this ColorF c)
        {
            if (Sse2.IsSupported)
            {
                Vector128<float> rgbx = c.RgbaV128.WithElement(3, 1f);
                Vector128<byte> result;
#if NET7_0_OR_GREATER
                if (Vector128.GreaterThanAll(rgbx, Vector128.Create(0.0031308f)))
#else
                if (Sse.CompareGreaterThan(rgbx, Vector128.Create(0.0031308f)).AsUInt64().Equals(AllBitsSet))
#endif
                {
#if NET7_0_OR_GREATER
                    if (Vector128.LessThanAll(rgbx.WithElement(3, 0f), Vector4.One.AsVector128()))
#else
                    if (Sse.CompareNotGreaterThanOrEqual(rgbx.WithElement(3, 0f), Vector128.Create(1f)).AsUInt64().Equals(Vector128.Create(UInt64.MaxValue)))
#endif
                    {
                        Vector128<float> resultF = Vector128.Create(MathF.Pow(c.R, 1f / 2.4f), MathF.Pow(c.G, 1f / 2.4f), MathF.Pow(c.B, 1f / 2.4f), 0f);
                        if (Fma.IsSupported)
                        {
                            resultF = Fma.MultiplyAdd(resultF, Vector128.Create(1.055f), Vector128.Create(-0.055f));
                            //resultF = Fma.MultiplyAdd(resultF.WithElement(3, c.A.ClipF()), Vector128.Create(255f), Vector128.Create(0.5f));
                        }
                        else
                        {
                            resultF = Sse.Multiply(resultF, Vector128.Create(1.055f));
                            resultF = Sse.Subtract(resultF, Vector128.Create(0.055f));
                            //resultF = Sse.Multiply(resultF.WithElement(3, c.A.ClipF()), Vector128.Create(255f));
                            //resultF = Sse.Add(resultF, Vector128.Create(0.5f));
                        }

                        resultF = Sse.Multiply(resultF.WithElement(3, c.A.ClipF()), Vector128.Create(255f));

                        result = Sse2.ConvertToVector128Int32(resultF).AsByte();
                        return Ssse3.IsSupported
                            ? new Color32(Ssse3.Shuffle(result, PackRgbaAsColor32Mask).AsUInt32().ToScalar())
                            : new Color32(result.GetElement(12), result.GetElement(0), result.GetElement(4), result.GetElement(8));
                    }
                }
#if NET7_0_OR_GREATER
                else if (Vector128.GreaterThanAll(rgbx, Vector128<float>.Zero))
#else
                else if (Sse.CompareGreaterThan(rgbx, Vector128<float>.Zero).AsUInt64().Equals(AllBitsSet))
#endif
                {
                    Vector128<float> resultF = Sse.Multiply(c.RgbaV128, Vector128.Create(12.92f)).WithElement(3, c.A.ClipF());
                    resultF = Sse.Multiply(resultF, Vector128.Create(255f));

                    result = Sse2.ConvertToVector128Int32(resultF).AsByte();
                    return Ssse3.IsSupported
                        ? new Color32(Ssse3.Shuffle(result, PackRgbaAsColor32Mask).AsUInt32().ToScalar())
                        : new Color32(result.GetElement(12), result.GetElement(0), result.GetElement(4), result.GetElement(8));
                }

                result = Sse2.ConvertToVector128Int32(Sse.Multiply(c.ToSrgb_0_Vanilla().RgbaV128, Vector128.Create(255f))).AsByte();
                return Ssse3.IsSupported
                    ? new Color32(Ssse3.Shuffle(result, PackRgbaAsColor32Mask).AsUInt32().ToScalar())
                    : new Color32(result.GetElement(12), result.GetElement(0), result.GetElement(4), result.GetElement(8));
            }

            return new Color32(ColorSpaceHelper.ToByte(c.A),
                ColorSpaceHelper.LinearToSrgb8Bit(c.R),
                ColorSpaceHelper.LinearToSrgb8Bit(c.G),
                ColorSpaceHelper.LinearToSrgb8Bit(c.B));
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Color32 ToColor32_3_ToSrgbVanillaFirst(this ColorF c)
        {
            if (Sse2.IsSupported)
            {
                var resultF = Sse.Multiply(c.ToSrgb_0_Vanilla().RgbaV128, Vector128.Create(255f));
                var result = Sse2.ConvertToVector128Int32(resultF).AsByte();
                return Ssse3.IsSupported
                    ? new Color32(Ssse3.Shuffle(result, PackRgbaAsColor32Mask).AsUInt32().ToScalar())
                    : new Color32(result.GetElement(12), result.GetElement(0), result.GetElement(4), result.GetElement(8));
            }

            return new Color32(ColorSpaceHelper.ToByte(c.A),
                ColorSpaceHelper.LinearToSrgb8Bit(c.R),
                ColorSpaceHelper.LinearToSrgb8Bit(c.G),
                ColorSpaceHelper.LinearToSrgb8Bit(c.B));
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Color32 ToColor32_4_ToSrgbIntrinsicsFirst(this ColorF c)
        {
            if (Sse2.IsSupported)
            {
                var resultF = Sse.Multiply(c.RgbaV128.ToSrgb_2_Intrinsics(), Vector128.Create(255f));
                var result = Sse2.ConvertToVector128Int32(resultF).AsByte();
                return Ssse3.IsSupported
                    ? new Color32(Ssse3.Shuffle(result, PackRgbaAsColor32Mask).AsUInt32().ToScalar())
                    : new Color32(result.GetElement(12), result.GetElement(0), result.GetElement(4), result.GetElement(8));
            }

            return new Color32(ColorSpaceHelper.ToByte(c.A),
                ColorSpaceHelper.LinearToSrgb8Bit(c.R),
                ColorSpaceHelper.LinearToSrgb8Bit(c.G),
                ColorSpaceHelper.LinearToSrgb8Bit(c.B));
        }

        internal static Color64 ToColor64_0_Vanilla(this ColorF c)
            // NOTE: this is even more vanilla than ToSrgb_0_Vanilla, which uses 128-bit vector creation
            => new Color64(ColorSpaceHelper.ToUInt16(c.A),
                ColorSpaceHelper.LinearToSrgb16Bit(c.R),
                ColorSpaceHelper.LinearToSrgb16Bit(c.G),
                ColorSpaceHelper.LinearToSrgb16Bit(c.B));

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Color64 ToColor64_1_Vector(this ColorF c)
        {
            // NOTE: On .NET 4.6 it's still slower than vanilla. On .NET Core 2.0 it's just a bit faster.
            if (c.Rgb.GreaterThanAll(0.0031308f))
            {
                if (c.Rgb.LessThanAll(1f))
                {
                    Vector3 result = new Vector3(MathF.Pow(c.R, 1f / 2.4f), MathF.Pow(c.G, 1f / 2.4f), MathF.Pow(c.B, 1f / 2.4f));
                    result = (result * 1.055f - new Vector3(0.055f)) * 65535f + new Vector3(0.5f);
                    return new Color64(ColorSpaceHelper.ToUInt16(c.A), (ushort)result.X, (ushort)result.Y, (ushort)result.Z);
                }
            }
            else if (c.Rgb.GreaterThanAll(0f))
            {
                Vector3 result = c.Rgb * (Byte.MaxValue * 12.92f) + new Vector3(0.5f);
                return new Color64(ColorSpaceHelper.ToUInt16(c.A), (ushort)result.X, (ushort)result.Y, (ushort)result.Z);
            }

            return new Color64(ColorSpaceHelper.ToUInt16(c.A),
                ColorSpaceHelper.LinearToSrgb16Bit(c.R),
                ColorSpaceHelper.LinearToSrgb16Bit(c.G),
                ColorSpaceHelper.LinearToSrgb16Bit(c.B));
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Color64 ToColor64_2_ToSrgbVanillaFirst(this ColorF c)
        {
            if (Sse2.IsSupported)
            {
                var rgbaF = Sse.Multiply(c.ToSrgb_0_Vanilla().RgbaV128, Vector128.Create(65535f));
                var rgbaI32 = Sse2.ConvertToVector128Int32(rgbaF).AsUInt16();
                return Ssse3.IsSupported
                    ? new Color64(Ssse3.Shuffle(rgbaI32.AsByte(), PackRgbaAsColor64Mask).AsUInt64().ToScalar())
                    : new Color64(rgbaI32.GetElement(6), rgbaI32.GetElement(0), rgbaI32.GetElement(2), rgbaI32.GetElement(4));
            }

            return new Color64(ColorSpaceHelper.ToUInt16(c.A),
                ColorSpaceHelper.LinearToSrgb16Bit(c.R),
                ColorSpaceHelper.LinearToSrgb16Bit(c.G),
                ColorSpaceHelper.LinearToSrgb16Bit(c.B));
        }

        #endregion
    }
}
#endif