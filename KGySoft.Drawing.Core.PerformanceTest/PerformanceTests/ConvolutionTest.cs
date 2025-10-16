#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ConvolutionTest.cs
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
using System.Runtime.InteropServices;
#if NETCOREAPP3_0_OR_GREATER
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
#endif
#if NETCOREAPP || NET45_OR_GREATER
using System.Numerics;
#endif

using KGySoft.Collections;
using KGySoft.CoreLibraries;
using KGySoft.Drawing.Imaging;

using NUnit.Framework;

#endregion

namespace KGySoft.Drawing.PerformanceTests
{
    [TestFixture]
    public class ConvolutionTest
    {
        #region Methods

        #region Static Methods

        private static PColorF ConvolveWith1_Vanilla<T>(ref CastArray<T, PColorF> colors, in CastArray<byte, float> kernelBuffer, int startIndex, int length)
            where T : unmanaged
        {
            // The vanilla solution actually may use vectors internally where available
            PColorF result = default;
            for (int i = 0; i < length; i++)
            {
                float weight = kernelBuffer.GetElementUnsafe(i);
                ref PColorF c = ref colors.GetElementReferenceUnsafe(startIndex + i);
                result += c * weight;
            }

            return result;
        }

        private static PColorF ConvolveWith1b_VanillaWeightInlined<T>(ref CastArray<T, PColorF> colors, in CastArray<byte, float> kernelBuffer, int startIndex, int length)
            where T : unmanaged
        {
            // The vanilla solution actually may use vectors internally where available
            PColorF result = default;
            for (int i = 0; i < length; i++)
            {
                ref PColorF c = ref colors.GetElementReferenceUnsafe(startIndex + i);
                result += c * kernelBuffer.GetElementUnsafe(i);
            }

            return result;
        }

        private static PColorF ConvolveWith1b2_VanillaColorInlined<T>(ref CastArray<T, PColorF> colors, in CastArray<byte, float> kernelBuffer, int startIndex, int length)
            where T : unmanaged
        {
            // The vanilla solution actually may use vectors internally where available
            PColorF result = default;
            for (int i = 0; i < length; i++)
            {
                float weight = kernelBuffer.GetElementUnsafe(i);
                result += colors.GetElementReferenceUnsafe(startIndex + i) * weight;
            }

            return result;
        }

        private static PColorF ConvolveWith1c_VanillaOpsInlined<T>(ref CastArray<T, PColorF> colors, in CastArray<byte, float> kernelBuffer, int startIndex, int length)
            where T : unmanaged
        {
            // The vanilla solution actually may use vectors internally where available
            PColorF result = default;
            for (int i = 0; i < length; i++)
                result += colors.GetElementReferenceUnsafe(startIndex + i) * kernelBuffer.GetElementUnsafe(i);

            return result;
        }

#if NETCOREAPP || NET45_OR_GREATER
        private static PColorF ConvolveWith2a_Vector4<T>(ref CastArray<T, PColorF> colors, in CastArray<byte, float> kernelBuffer, int startIndex, int length)
            where T : unmanaged
        {
            Vector4 result = default;
            for (int i = 0; i < length; i++)
            {
                float weight = kernelBuffer.GetElementUnsafe(i);
                ref readonly Vector4 c = ref colors.GetElementReferenceUnsafe(startIndex + i).Rgba;
                result += c * weight;
            }

            return new PColorF(result);
        }

        private static PColorF ConvolveWith2b_Vector4WeightInlined<T>(ref CastArray<T, PColorF> colors, in CastArray<byte, float> kernelBuffer, int startIndex, int length)
            where T : unmanaged
        {
            Vector4 result = default;
            for (int i = 0; i < length; i++)
            {
                ref readonly Vector4 c = ref colors.GetElementReferenceUnsafe(startIndex + i).Rgba;
                result += c * kernelBuffer.GetElementUnsafe(i);
            }

            return new PColorF(result);
        }

        private static PColorF ConvolveWith2c_Vector4ColorInlined<T>(ref CastArray<T, PColorF> colors, in CastArray<byte, float> kernelBuffer, int startIndex, int length)
            where T : unmanaged
        {
            Vector4 result = default;
            for (int i = 0; i < length; i++)
            {
                float weight = kernelBuffer.GetElementUnsafe(i);
                result += colors.GetElementReferenceUnsafe(startIndex + i).Rgba * weight;
            }

            return new PColorF(result);
        }

        private static PColorF ConvolveWith2d_Vector4OpsInlined<T>(ref CastArray<T, PColorF> colors, in CastArray<byte, float> kernelBuffer, int startIndex, int length)
            where T : unmanaged
        {
            Vector4 result = default;
            for (int i = 0; i < length; i++)
                result += colors.GetElementReferenceUnsafe(startIndex + i).Rgba * kernelBuffer.GetElementUnsafe(i);

            return new PColorF(result);
        }

#endif

#if NETCOREAPP2_1_OR_GREATER
        private static PColorF ConvolveWith3a_Vector4Span<T>(ref CastArray<T, PColorF> colors, in CastArray<byte, float> kernelBuffer, int startIndex, int length)
            where T : unmanaged
        {
            Vector4 result = default;
            Span<Vector4> asSpan = MemoryMarshal.Cast<PColorF, Vector4>(colors.AsSpan);
            for (int i = 0; i < length; i++)
            {
                float weight = kernelBuffer.GetElementUnsafe(i);
                ref Vector4 c = ref asSpan[startIndex + i];
                result += c * weight;
            }

            return new PColorF(result);
        }
        private static PColorF ConvolveWith3b_Vector4SpanWeightInlined<T>(ref CastArray<T, PColorF> colors, in CastArray<byte, float> kernelBuffer, int startIndex, int length)
            where T : unmanaged
        {
            Vector4 result = default;
            Span<Vector4> asSpan = MemoryMarshal.Cast<PColorF, Vector4>(colors.AsSpan);
            for (int i = 0; i < length; i++)
            {
                ref Vector4 c = ref asSpan[startIndex + i];
                result += c * kernelBuffer.GetElementUnsafe(i);
            }

            return new PColorF(result);
        }
        private static PColorF ConvolveWith3c_Vector4SpanColorInlined<T>(ref CastArray<T, PColorF> colors, in CastArray<byte, float> kernelBuffer, int startIndex, int length)
            where T : unmanaged
        {
            Vector4 result = default;
            Span<Vector4> asSpan = MemoryMarshal.Cast<PColorF, Vector4>(colors.AsSpan);
            for (int i = 0; i < length; i++)
            {
                float weight = kernelBuffer.GetElementUnsafe(i);
                result += asSpan[startIndex + i] * weight;
            }

            return new PColorF(result);
        }
        private static PColorF ConvolveWith3d_Vector4SpanOpsInlined<T>(ref CastArray<T, PColorF> colors, in CastArray<byte, float> kernelBuffer, int startIndex, int length)
            where T : unmanaged
        {
            Vector4 result = default;
#if NETCOREAPP3_0_OR_GREATER
            Span<Vector4> asSpan = MemoryMarshal.CreateSpan(ref Unsafe.As<PColorF, Vector4>(ref colors.GetElementReferenceUnsafe(0)), colors.Length);
#else
            Span<Vector4> asSpan = MemoryMarshal.Cast<PColorF, Vector4>(colors.AsSpan);
#endif
            for (int i = 0; i < length; i++)
                result += asSpan[startIndex + i] * kernelBuffer.GetElementUnsafe(i);

            return new PColorF(result);
        }
#endif

#if NET9_0_OR_GREATER
        private static PColorF ConvolveWith4a_Vector4SpanFusedMultiplyAdd<T>(ref CastArray<T, PColorF> colors, in CastArray<byte, float> kernelBuffer, int startIndex, int length)
            where T : unmanaged
        {
            Vector4 result = default;
            Span<Vector4> asSpan = MemoryMarshal.CreateSpan(ref Unsafe.As<PColorF, Vector4>(ref colors.GetElementReferenceUnsafe(0)), colors.Length);
            for (int i = 0; i < length; i++)
            {
                float weight = kernelBuffer.GetElementUnsafe(i);
                ref Vector4 c = ref asSpan[startIndex + i];
                result = Vector4.FusedMultiplyAdd(c,  new Vector4(weight), result);
            }

            return new PColorF(result);
        }

        private static PColorF ConvolveWith4b_Vector4SpanFusedMultiplyAddWeightInlined<T>(ref CastArray<T, PColorF> colors, in CastArray<byte, float> kernelBuffer, int startIndex, int length)
            where T : unmanaged
        {
            Vector4 result = default;
            Span<Vector4> asSpan = MemoryMarshal.CreateSpan(ref Unsafe.As<PColorF, Vector4>(ref colors.GetElementReferenceUnsafe(0)), colors.Length);
            for (int i = 0; i < length; i++)
            {
                ref Vector4 c = ref asSpan[startIndex + i];
                result = Vector4.FusedMultiplyAdd(c,  new(kernelBuffer.GetElementUnsafe(i)), result);
            }

            return new PColorF(result);
        }

        private static PColorF ConvolveWith4c_Vector4SpanFusedMultiplyAddColorInlined<T>(ref CastArray<T, PColorF> colors, in CastArray<byte, float> kernelBuffer, int startIndex, int length)
            where T : unmanaged
        {
            Vector4 result = default;
            Span<Vector4> asSpan = MemoryMarshal.CreateSpan(ref Unsafe.As<PColorF, Vector4>(ref colors.GetElementReferenceUnsafe(0)), colors.Length);
            for (int i = 0; i < length; i++)
            {
                float weight = kernelBuffer.GetElementUnsafe(i);
                result = Vector4.FusedMultiplyAdd(asSpan[startIndex + i],  new(weight), result);
            }

            return new PColorF(result);
        }

        private static PColorF ConvolveWith4d_Vector4SpanFusedMultiplyAddOpsInlined<T>(ref CastArray<T, PColorF> colors, in CastArray<byte, float> kernelBuffer, int startIndex, int length)
            where T : unmanaged
        {
            Vector4 result = default;
            Span<Vector4> asSpan = MemoryMarshal.CreateSpan(ref Unsafe.As<PColorF, Vector4>(ref colors.GetElementReferenceUnsafe(0)), colors.Length);
            for (int i = 0; i < length; i++)
                result = Vector4.FusedMultiplyAdd(asSpan[startIndex + i],  new(kernelBuffer.GetElementUnsafe(i)), result);

            return new PColorF(result);
        }
#endif

#if NETCOREAPP3_0_OR_GREATER
        private static PColorF ConvolveWith5a_Vector128FmaMultiplyAdd<T>(ref CastArray<T, PColorF> colors, in CastArray<byte, float> kernelBuffer, int startIndex, int length)
           where T : unmanaged
        {
            Vector128<float> result = default;
            Span<Vector128<float>> asSpan = MemoryMarshal.CreateSpan(ref Unsafe.As<PColorF, Vector128<float>>(ref colors.GetElementReferenceUnsafe(0)), colors.Length);
            for (int i = 0; i < length; i++)
            {
                float weight = kernelBuffer.GetElementUnsafe(i);
                ref Vector128<float> c = ref asSpan[startIndex + i];
                result = Fma.MultiplyAdd(c, Vector128.Create(weight), result);
            }

            return new PColorF(result);
        }

        private static PColorF ConvolveWith5b_Vector128FmaMultiplyAddWeightInlined<T>(ref CastArray<T, PColorF> colors, in CastArray<byte, float> kernelBuffer, int startIndex, int length)
            where T : unmanaged
        {
            Vector128<float> result = default;
            Span<Vector128<float>> asSpan = MemoryMarshal.CreateSpan(ref Unsafe.As<PColorF, Vector128<float>>(ref colors.GetElementReferenceUnsafe(0)), colors.Length);
            for (int i = 0; i < length; i++)
            {
                ref Vector128<float> c = ref asSpan[startIndex + i];
                result = Fma.MultiplyAdd(c, Vector128.Create(kernelBuffer.GetElementUnsafe(i)), result);
            }

            return new PColorF(result);
        }

        private static PColorF ConvolveWith5c_Vector128FmaMultiplyAddColorInlined<T>(ref CastArray<T, PColorF> colors, in CastArray<byte, float> kernelBuffer, int startIndex, int length)
            where T : unmanaged
        {
            Vector128<float> result = default;
            Span<Vector128<float>> asSpan = MemoryMarshal.CreateSpan(ref Unsafe.As<PColorF, Vector128<float>>(ref colors.GetElementReferenceUnsafe(0)), colors.Length);
            for (int i = 0; i < length; i++)
            {
                float weight = kernelBuffer.GetElementUnsafe(i);
                result = Fma.MultiplyAdd(asSpan[startIndex + i], Vector128.Create(weight), result);
            }

            return new PColorF(result);
        }

        private static PColorF ConvolveWith5d_Vector128FmaMultiplyAddOpsInlined<T>(ref CastArray<T, PColorF> colors, in CastArray<byte, float> kernelBuffer, int startIndex, int length)
            where T : unmanaged
        {
            Vector128<float> result = default;
            Span<Vector128<float>> asSpan = MemoryMarshal.CreateSpan(ref Unsafe.As<PColorF, Vector128<float>>(ref colors.GetElementReferenceUnsafe(0)), colors.Length);
            for (int i = 0; i < length; i++)
                result = Fma.MultiplyAdd(asSpan[startIndex + i], Vector128.Create(kernelBuffer.GetElementUnsafe(i)), result);

            return new PColorF(result);
        }

        private static PColorF ConvolveWith6_Vector256FmaMultiplyAdd<T>(ref CastArray<T, PColorF> colors, in CastArray<byte, float> kernelBuffer, int startIndex, int length)
           where T : unmanaged
        {
            Vector128<float> result = default;
            int vectorCount = length >> 1; //length & ~1; // (length >> 1) << 1;
            if (vectorCount != 0)
            {
                Vector256<float> result256 = Vector256<float>.Zero;
                ref Vector256<float> vecItem = ref Unsafe.As<PColorF, Vector256<float>>(ref colors.GetElementReferenceUnsafe(startIndex));
                ref float kernelItem = ref kernelBuffer.GetElementReferenceUnsafe(0);
                for (int i = 0; i < vectorCount; i++)
                {
                    result256 = Fma.MultiplyAdd(Unsafe.Add(ref vecItem, i), Vector256.Create(Vector128.Create(kernelItem), Vector128.Create(Unsafe.Add(ref kernelItem, 1))), result256);
                    kernelItem = ref Unsafe.Add(ref kernelItem, 2);
                }

                result = Sse.Add(result256.GetLower(), result256.GetUpper());
            }

            // Handle the remaining one if any
            if ((length & 1) != 0)
                result = Fma.MultiplyAdd(colors.GetElementReferenceUnsafe(startIndex + length - 1).RgbaV128, Vector128.Create(kernelBuffer.GetElementUnsafe(length - 1)), result);

            return new PColorF(result);
        }

        private static PColorF ConvolveWith6b_Vector256FmaMultiplyAddVector4<T>(ref CastArray<T, PColorF> colors, in CastArray<byte, float> kernelBuffer, int startIndex, int length)
           where T : unmanaged
        {
            Vector4 result = default;
            int vectorCount = length >> 1; //length & ~1; // (length >> 1) << 1;
            if (vectorCount != 0)
            {
                Vector256<float> result256 = Vector256<float>.Zero;
                ref Vector256<float> vecItem = ref Unsafe.As<PColorF, Vector256<float>>(ref colors.GetElementReferenceUnsafe(startIndex));
                ref float kernelItem = ref kernelBuffer.GetElementReferenceUnsafe(0);
                for (int i = 0; i < vectorCount; i++)
                {
                    result256 = Fma.MultiplyAdd(Unsafe.Add(ref vecItem, i), Vector256.Create(Vector128.Create(kernelItem), Vector128.Create(Unsafe.Add(ref kernelItem, 1))), result256);
                    kernelItem = ref Unsafe.Add(ref kernelItem, 2);
                }

                result = Sse.Add(result256.GetLower(), result256.GetUpper()).AsVector4();
            }

            // Handle the remaining one as Vector4 if any
            if ((length & 1) != 0)
                result += colors[startIndex + length - 1].Rgba * kernelBuffer.GetElementUnsafe(length - 1);

            return new PColorF(result);
        }

#endif

        #endregion

        #region Instance Methods

        [Test]
        public void ConvolveTest()
        {
            const int bufferSize = 16;
            const int startIndex = 1;
            const int length = 8;

            PColorF[] colorValues = new PColorF[bufferSize];
            var rnd = new FastRandom(42);
            for (int i = 0; i < colorValues.Length; i++)
                colorValues[i] = new PColorF(rnd.NextSingle(), rnd.NextSingle(), rnd.NextSingle());

            float[] kernelValues = [0.4644352f, 0.4644352f, 0.1213389f, -0.0376569f, -0.0125523f, -0.03474903f, 0.1119691f, 0.4285714f, 0.4285714f, 0.1119691f, -0.03474903f, -0.01158301f, -0.01171875f, -0.03515625f, 0.1132813f, 0.4335938f, 0.4335938f, 0.1132813f, -0.03515625f, -0.01171875f, -0.01158301f, -0.03474903f, 0.1119691f, 0.4285714f, 0.4285714f, 0.1119691f, -0.03474903f, -0.0125523f, -0.0376569f, 0.1213389f, 0.4644352f, 0.4644352f];
            byte[] buf = new byte[kernelValues.Length * sizeof(float)];
            Buffer.BlockCopy(kernelValues, 0, buf, 0, buf.Length);
            CastArray<byte, float> kernelBuffer = buf.Cast<byte, float>();

#if NETFRAMEWORK || NETCOREAPP && !NETCOREAPP3_0_OR_GREATER
            CastArray<PColorF, PColorF> colors = colorValues.Cast<PColorF, PColorF>();
#else
            var colorBuf = new byte[colorValues.Length * sizeof(float) * 4];
            CastArray<byte, PColorF> colors = colorBuf.Cast<byte, PColorF>();
            for (int i = 0; i < colorValues.Length; i++)
                colors.SetElementUnsafe(i, colorValues[i]);
#endif

            Assert.IsTrue(kernelBuffer.Length >= length && colors.Length >= startIndex + length);

            PColorF expected = ConvolveWith1_Vanilla(ref colors, in kernelBuffer, startIndex, length);
            Assert.AreEqual(expected, ConvolveWith1b_VanillaWeightInlined(ref colors, in kernelBuffer, startIndex, length));
            Assert.AreEqual(expected, ConvolveWith1b2_VanillaColorInlined(ref colors, in kernelBuffer, startIndex, length));
            Assert.AreEqual(expected, ConvolveWith1c_VanillaOpsInlined(ref colors, in kernelBuffer, startIndex, length));
#if NETCOREAPP || NET45_OR_GREATER
            Assert.AreEqual(expected, ConvolveWith2a_Vector4(ref colors, in kernelBuffer, startIndex, length));
            Assert.AreEqual(expected, ConvolveWith2b_Vector4WeightInlined(ref colors, in kernelBuffer, startIndex, length));
            Assert.AreEqual(expected, ConvolveWith2c_Vector4ColorInlined(ref colors, in kernelBuffer, startIndex, length));
            Assert.AreEqual(expected, ConvolveWith2d_Vector4OpsInlined(ref colors, in kernelBuffer, startIndex, length));
#endif
#if NETCOREAPP2_1_OR_GREATER
            Assert.AreEqual(expected, ConvolveWith3a_Vector4Span(ref colors, in kernelBuffer, startIndex, length));
            Assert.AreEqual(expected, ConvolveWith3b_Vector4SpanWeightInlined(ref colors, in kernelBuffer, startIndex, length));
            Assert.AreEqual(expected, ConvolveWith3c_Vector4SpanColorInlined(ref colors, in kernelBuffer, startIndex, length));
            Assert.AreEqual(expected, ConvolveWith3d_Vector4SpanOpsInlined(ref colors, in kernelBuffer, startIndex, length));
#endif
#if NETCOREAPP3_0_OR_GREATER
            PColorF actual = ConvolveWith5a_Vector128FmaMultiplyAdd(ref colors, in kernelBuffer, startIndex, length);
            Assert.IsTrue(expected.TolerantEquals(actual), $"{expected} vs {actual}");
            actual = ConvolveWith5b_Vector128FmaMultiplyAddWeightInlined(ref colors, in kernelBuffer, startIndex, length);
            Assert.IsTrue(expected.TolerantEquals(actual), $"{expected} vs {actual}");
            actual = ConvolveWith5c_Vector128FmaMultiplyAddColorInlined(ref colors, in kernelBuffer, startIndex, length);
            Assert.IsTrue(expected.TolerantEquals(actual), $"{expected} vs {actual}");
            actual = ConvolveWith5d_Vector128FmaMultiplyAddOpsInlined(ref colors, in kernelBuffer, startIndex, length);
            Assert.IsTrue(expected.TolerantEquals(actual), $"{expected} vs {actual}");
            actual = ConvolveWith6_Vector256FmaMultiplyAdd(ref colors, in kernelBuffer, startIndex, length);
            Assert.IsTrue(expected.TolerantEquals(actual), $"{expected} vs {actual}");
            actual = ConvolveWith6b_Vector256FmaMultiplyAddVector4(ref colors, in kernelBuffer, startIndex, length);
            Assert.IsTrue(expected.TolerantEquals(actual), $"{expected} vs {actual}");
#endif
#if NET9_0_OR_GREATER
            //Assert.AreEqual(expected, ConvolveWith4_Vector4SpanFusedMultiplyAdd(ref colors, in kernelBuffer, startIndex, length));
            actual = ConvolveWith4a_Vector4SpanFusedMultiplyAdd(ref colors, in kernelBuffer, startIndex, length);
            Assert.IsTrue(expected.TolerantEquals(actual), $"{expected} vs {actual}");
            actual = ConvolveWith4b_Vector4SpanFusedMultiplyAddWeightInlined(ref colors, in kernelBuffer, startIndex, length);
            Assert.IsTrue(expected.TolerantEquals(actual), $"{expected} vs {actual}");
            actual = ConvolveWith4c_Vector4SpanFusedMultiplyAddColorInlined(ref colors, in kernelBuffer, startIndex, length);
            Assert.IsTrue(expected.TolerantEquals(actual), $"{expected} vs {actual}");
            actual = ConvolveWith4d_Vector4SpanFusedMultiplyAddOpsInlined(ref colors, in kernelBuffer, startIndex, length);
            Assert.IsTrue(expected.TolerantEquals(actual), $"{expected} vs {actual}");
#endif

            new PerformanceTest<PColorF>
                {
                    TestName = "ConvolveTest",
                    TestTime = 5000,
                    //Iterations = 1_000_000,
                    Repeat = 3
                }
                //.AddCase(() => ConvolveWith1_Vanilla(ref colors, in kernelBuffer, startIndex, length), nameof(ConvolveWith1_Vanilla))
                //.AddCase(() => ConvolveWith1b_VanillaWeightInlined(ref colors, in kernelBuffer, startIndex, length), nameof(ConvolveWith1b_VanillaWeightInlined))
                .AddCase(() => ConvolveWith1b2_VanillaColorInlined(ref colors, in kernelBuffer, startIndex, length), nameof(ConvolveWith1b2_VanillaColorInlined))
                //.AddCase(() => ConvolveWith1c_VanillaOpsInlined(ref colors, in kernelBuffer, startIndex, length), nameof(ConvolveWith1c_VanillaOpsInlined))
#if NETCOREAPP || NET45_OR_GREATER
                //.AddCase(() => ConvolveWith2a_Vector4(ref colors, in kernelBuffer, startIndex, length), nameof(ConvolveWith2a_Vector4))
                //.AddCase(() => ConvolveWith2b_Vector4WeightInlined(ref colors, in kernelBuffer, startIndex, length), nameof(ConvolveWith2b_Vector4WeightInlined))
                .AddCase(() => ConvolveWith2c_Vector4ColorInlined(ref colors, in kernelBuffer, startIndex, length), nameof(ConvolveWith2c_Vector4ColorInlined))
                //.AddCase(() => ConvolveWith2d_Vector4OpsInlined(ref colors, in kernelBuffer, startIndex, length), nameof(ConvolveWith2d_Vector4OpsInlined))
#endif
#if NETCOREAPP2_1_OR_GREATER
                //.AddCase(() => ConvolveWith3a_Vector4Span(ref colors, in kernelBuffer, startIndex, length), nameof(ConvolveWith3a_Vector4Span))
                //.AddCase(() => ConvolveWith3b_Vector4SpanWeightInlined(ref colors, in kernelBuffer, startIndex, length), nameof(ConvolveWith3b_Vector4SpanWeightInlined))
                //.AddCase(() => ConvolveWith3c_Vector4SpanColorInlined(ref colors, in kernelBuffer, startIndex, length), nameof(ConvolveWith3c_Vector4SpanColorInlined))
                .AddCase(() => ConvolveWith3d_Vector4SpanOpsInlined(ref colors, in kernelBuffer, startIndex, length), nameof(ConvolveWith3d_Vector4SpanOpsInlined))
#endif
#if NET9_0_OR_GREATER
                //.AddCase(() => ConvolveWith4a_Vector4SpanFusedMultiplyAdd(ref colors, in kernelBuffer, startIndex, length), nameof(ConvolveWith4a_Vector4SpanFusedMultiplyAdd))
                //.AddCase(() => ConvolveWith4b_Vector4SpanFusedMultiplyAddWeightInlined(ref colors, in kernelBuffer, startIndex, length), nameof(ConvolveWith4b_Vector4SpanFusedMultiplyAddWeightInlined))
                //.AddCase(() => ConvolveWith4c_Vector4SpanFusedMultiplyAddColorInlined(ref colors, in kernelBuffer, startIndex, length), nameof(ConvolveWith4c_Vector4SpanFusedMultiplyAddColorInlined))
                .AddCase(() => ConvolveWith4d_Vector4SpanFusedMultiplyAddOpsInlined(ref colors, in kernelBuffer, startIndex, length), nameof(ConvolveWith4d_Vector4SpanFusedMultiplyAddOpsInlined))
#endif
#if NETCOREAPP3_0_OR_GREATER
                //.AddCase(() => ConvolveWith5a_Vector128FmaMultiplyAdd(ref colors, in kernelBuffer, startIndex, length), nameof(ConvolveWith5a_Vector128FmaMultiplyAdd))
                //.AddCase(() => ConvolveWith5b_Vector128FmaMultiplyAddWeightInlined(ref colors, in kernelBuffer, startIndex, length), nameof(ConvolveWith5b_Vector128FmaMultiplyAddWeightInlined))
                //.AddCase(() => ConvolveWith5c_Vector128FmaMultiplyAddColorInlined(ref colors, in kernelBuffer, startIndex, length), nameof(ConvolveWith5c_Vector128FmaMultiplyAddColorInlined))
                .AddCase(() => ConvolveWith5d_Vector128FmaMultiplyAddOpsInlined(ref colors, in kernelBuffer, startIndex, length), nameof(ConvolveWith5d_Vector128FmaMultiplyAddOpsInlined))
                .AddCase(() => ConvolveWith6_Vector256FmaMultiplyAdd(ref colors, in kernelBuffer, startIndex, length), nameof(ConvolveWith6_Vector256FmaMultiplyAdd))
                .AddCase(() => ConvolveWith6b_Vector256FmaMultiplyAddVector4(ref colors, in kernelBuffer, startIndex, length), nameof(ConvolveWith6b_Vector256FmaMultiplyAddVector4))
#endif
                .DoTest()
                .DumpResults(Console.Out);

            // Verdict:
            // - Vector4: FusedMultiplyAdd has no advantage, and is even less precise. Using SpanOpsInlined if spans area available; otherwise, ColorInlined (on framework).
            // - Vector128 (with FMA): is not better than Vector4SpanOpsInlined, and is less precise - not using it
            // - Vector256 (with FMA): is getting better only when length is at least 8 (except for 9, and not in older frameworks, but it's still close)

            // 4.0
            // 1. ConvolveWith1b2_VanillaColorInlined: 182 608 901 iterations in 15 000,00 ms. Adjusted for 5 000 ms: 60 869 632,86
            //   #1  61 122 360 iterations in 5 000,00 ms. Adjusted: 61 122 360,00	 <---- Best
            //   #2  60 904 654 iterations in 5 000,00 ms. Adjusted: 60 904 652,78
            //   #3  60 581 887 iterations in 5 000,00 ms. Adjusted: 60 581 885,79	 <---- Worst
            //   Worst-Best difference: 540 474,21 (0,89%)
            // 2. ConvolveWith1_Vanilla: 174 788 658 iterations in 15 002,80 ms. Adjusted for 5 000 ms: 58 252 419,39 (-2 617 213,46 / 95,70%)
            //   #1  56 065 294 iterations in 5 002,75 ms. Adjusted: 56 034 505,28	 <---- Worst
            //   #2  58 987 484 iterations in 5 000,05 ms. Adjusted: 58 986 872,90
            //   #3  59 735 880 iterations in 5 000,00 ms. Adjusted: 59 735 880,00	 <---- Best
            //   Worst-Best difference: 3 701 374,72 (6,61%)
            // 3. ConvolveWith1b_VanillaWeightInlined: 172 673 335 iterations in 15 000,11 ms. Adjusted for 5 000 ms: 57 557 357,43 (-3 312 275,43 / 94,56%)
            //   #1  57 511 096 iterations in 5 000,00 ms. Adjusted: 57 511 096,00
            //   #2  57 501 311 iterations in 5 000,11 ms. Adjusted: 57 500 051,75	 <---- Worst
            //   #3  57 660 928 iterations in 5 000,00 ms. Adjusted: 57 660 924,54	 <---- Best
            //   Worst-Best difference: 160 872,79 (0,28%)
            // 4. ConvolveWith1c_VanillaOpsInlined: 166 896 762 iterations in 15 000,01 ms. Adjusted for 5 000 ms: 55 632 201,74 (-5 237 431,11 / 91,40%)
            //   #1  55 865 158 iterations in 5 000,00 ms. Adjusted: 55 865 156,88	 <---- Best
            //   #2  55 440 864 iterations in 5 000,00 ms. Adjusted: 55 440 862,89	 <---- Worst
            //   #3  55 590 740 iterations in 5 000,01 ms. Adjusted: 55 590 585,46
            //   Worst-Best difference: 424 293,99 (0,77%)

            // 4.5
            // 1. ConvolveWith2c_Vector4ColorInlined: 334 650 517 iterations in 15 000,00 ms. Adjusted for 5 000 ms: 111 550 154,56
            //   #1  111 057 547 iterations in 5 000,00 ms. Adjusted: 111 057 493,69	 <---- Worst
            //   #2  111 537 002 iterations in 5 000,00 ms. Adjusted: 111 537 002,00
            //   #3  112 055 968 iterations in 5 000,00 ms. Adjusted: 112 055 968,00	 <---- Best
            //   Worst-Best difference: 998 474,31 (0,90%)
            // 2. ConvolveWith2d_Vector4OpsInlined: 314 947 758 iterations in 15 000,00 ms. Adjusted for 5 000 ms: 104 982 585,30 (-6 567 569,26 / 94,11%)
            //   #1  105 253 540 iterations in 5 000,00 ms. Adjusted: 105 253 540,00	 <---- Best
            //   #2  104 763 018 iterations in 5 000,00 ms. Adjusted: 104 763 015,90	 <---- Worst
            //   #3  104 931 200 iterations in 5 000,00 ms. Adjusted: 104 931 200,00
            //   Worst-Best difference: 490 524,10 (0,47%)
            // 3. ConvolveWith2b_Vector4WeightInlined: 310 584 865 iterations in 15 000,00 ms. Adjusted for 5 000 ms: 103 528 288,33 (-8 021 866,23 / 92,81%)
            //   #1  103 325 903 iterations in 5 000,00 ms. Adjusted: 103 325 903,00	 <---- Worst
            //   #2  103 694 256 iterations in 5 000,00 ms. Adjusted: 103 694 256,00	 <---- Best
            //   #3  103 564 706 iterations in 5 000,00 ms. Adjusted: 103 564 706,00
            //   Worst-Best difference: 368 353,00 (0,36%)
            // 4. ConvolveWith2a_Vector4: 301 639 667 iterations in 15 000,19 ms. Adjusted for 5 000 ms: 100 545 335,30 (-11 004 819,26 / 90,13%)
            //   #1  96 865 121 iterations in 5 000,18 ms. Adjusted: 96 861 539,06	 <---- Worst
            //   #2  98 943 327 iterations in 5 000,00 ms. Adjusted: 98 943 247,85
            //   #3  105 831 219 iterations in 5 000,00 ms. Adjusted: 105 831 219,00	 <---- Best
            //   Worst-Best difference: 8 969 679,94 (9,26%)

            // .NET Core 3.0 (length = 7)
            // 1. ConvolveWith3d_Vector4SpanOpsInlined: 704 448 622 iterations in 15 000,02 ms. Adjusted for 5 000 ms: 234 815 822,34
            //   #1  235 410 802 iterations in 5 000,01 ms. Adjusted: 235 410 463,01    <---- Best
            //   #2  234 693 322 iterations in 5 000,01 ms. Adjusted: 234 692 998,12
            //   #3  234 344 498 iterations in 5 000,01 ms. Adjusted: 234 344 005,88    <---- Worst
            //   Worst-Best difference: 1 066 457,13 (0,46%)
            // 2. ConvolveWith2c_Vector4ColorInlined: 688 668 461 iterations in 15 000,03 ms. Adjusted for 5 000 ms: 229 555 682,40 (-5 260 139,94 / 97,76%)
            //   #1  230 266 240 iterations in 5 000,01 ms. Adjusted: 230 265 807,10    <---- Best
            //   #2  229 505 854 iterations in 5 000,01 ms. Adjusted: 229 505 390,40
            //   #3  228 896 367 iterations in 5 000,01 ms. Adjusted: 228 895 849,70    <---- Worst
            //   Worst-Best difference: 1 369 957,40 (0,60%)
            // 3. ConvolveWith5d_Vector128FmaMultiplyAddOpsInlined: 685 761 163 iterations in 15 000,03 ms. Adjusted for 5 000 ms: 228 586 616,95 (-6 229 205,39 / 97,35%)
            //   #1  229 153 704 iterations in 5 000,01 ms. Adjusted: 229 153 245,69    <---- Best
            //   #2  228 442 826 iterations in 5 000,01 ms. Adjusted: 228 442 405,67
            //   #3  228 164 633 iterations in 5 000,01 ms. Adjusted: 228 164 199,49    <---- Worst
            //   Worst-Best difference: 989 046,21 (0,43%)
            // 4. ConvolveWith6b_Vector256FmaMultiplyAddVector4: 683 511 115 iterations in 15 000,03 ms. Adjusted for 5 000 ms: 227 836 588,75 (-6 979 233,59 / 97,03%)
            //   #1  227 490 761 iterations in 5 000,01 ms. Adjusted: 227 490 296,92    <---- Worst
            //   #2  227 780 300 iterations in 5 000,01 ms. Adjusted: 227 779 853,55
            //   #3  228 240 054 iterations in 5 000,01 ms. Adjusted: 228 239 615,78    <---- Best
            //   Worst-Best difference: 749 318,86 (0,33%)
            // 5. ConvolveWith6_Vector256FmaMultiplyAdd: 669 408 256 iterations in 15 000,03 ms. Adjusted for 5 000 ms: 223 135 634,59 (-11 680 187,75 / 95,03%)
            //   #1  223 261 245 iterations in 5 000,01 ms. Adjusted: 223 260 919,04
            //   #2  223 271 158 iterations in 5 000,01 ms. Adjusted: 223 270 564,10    <---- Best
            //   #3  222 875 853 iterations in 5 000,01 ms. Adjusted: 222 875 420,62    <---- Worst
            //   Worst-Best difference: 395 143,48 (0,18%)
            // 6. ConvolveWith1b2_VanillaColorInlined: 234 554 184 iterations in 15 000,04 ms. Adjusted for 5 000 ms: 78 184 504,37 (-156 631 317,97 / 33,30%)
            //   #1  76 999 482 iterations in 5 000,02 ms. Adjusted: 76 999 177,08      <---- Worst
            //   #2  77 271 848 iterations in 5 000,01 ms. Adjusted: 77 271 639,37
            //   #3  80 282 854 iterations in 5 000,01 ms. Adjusted: 80 282 696,65      <---- Best
            //   Worst-Best difference: 3 283 519,56 (4,26%)

            // .NET 10.0 (length = 6)
            // 1. ConvolveWith3d_Vector4SpanOpsInlined: 915 146 388 iterations in 15 000,00 ms. Adjusted for 5 000 ms: 305 048 796,00
            //   #1  305 946 233 iterations in 5 000,00 ms. Adjusted: 305 946 233,00    <---- Best
            //   #2  304 970 117 iterations in 5 000,00 ms. Adjusted: 304 970 117,00
            //   #3  304 230 038 iterations in 5 000,00 ms. Adjusted: 304 230 038,00    <---- Worst
            //   Worst-Best difference: 1 716 195,00 (0,56%)
            // 2. ConvolveWith6_Vector256FmaMultiplyAdd: 902 265 554 iterations in 15 000,00 ms. Adjusted for 5 000 ms: 300 755 184,67 (-4 293 611,33 / 98,59%)
            //   #1  300 313 601 iterations in 5 000,00 ms. Adjusted: 300 313 601,00    <---- Worst
            //   #2  301 004 540 iterations in 5 000,00 ms. Adjusted: 301 004 540,00    <---- Best
            //   #3  300 947 413 iterations in 5 000,00 ms. Adjusted: 300 947 413,00
            //   Worst-Best difference: 690 939,00 (0,23%)
            // 3. ConvolveWith6b_Vector256FmaMultiplyAddVector4: 900 505 757 iterations in 15 000,00 ms. Adjusted for 5 000 ms: 300 168 585,67 (-4 880 210,33 / 98,40%)
            //   #1  299 828 042 iterations in 5 000,00 ms. Adjusted: 299 828 042,00    <---- Worst
            //   #2  300 704 105 iterations in 5 000,00 ms. Adjusted: 300 704 105,00    <---- Best
            //   #3  299 973 610 iterations in 5 000,00 ms. Adjusted: 299 973 610,00
            //   Worst-Best difference: 876 063,00 (0,29%)
            // 4. ConvolveWith4d_Vector4SpanFusedMultiplyAddOpsInlined: 866 132 756 iterations in 15 000,00 ms. Adjusted for 5 000 ms: 288 710 918,67 (-16 337 877,33 / 94,64%)
            //   #1  288 865 045 iterations in 5 000,00 ms. Adjusted: 288 865 045,00    <---- Best
            //   #2  288 601 542 iterations in 5 000,00 ms. Adjusted: 288 601 542,00    <---- Worst
            //   #3  288 666 169 iterations in 5 000,00 ms. Adjusted: 288 666 169,00
            //   Worst-Best difference: 263 503,00 (0,09%)
            // 5. ConvolveWith5d_Vector128FmaMultiplyAddOpsInlined: 861 482 393 iterations in 15 000,00 ms. Adjusted for 5 000 ms: 287 160 797,67 (-17 887 998,33 / 94,14%)
            //   #1  287 449 342 iterations in 5 000,00 ms. Adjusted: 287 449 342,00    <---- Best
            //   #2  287 347 874 iterations in 5 000,00 ms. Adjusted: 287 347 874,00
            //   #3  286 685 177 iterations in 5 000,00 ms. Adjusted: 286 685 177,00    <---- Worst
            //   Worst-Best difference: 764 165,00 (0,27%)
            // 6. ConvolveWith1b2_VanillaColorInlined: 836 330 276 iterations in 15 000,00 ms. Adjusted for 5 000 ms: 278 776 758,67 (-26 272 037,33 / 91,39%)
            //   #1  270 235 157 iterations in 5 000,00 ms. Adjusted: 270 235 157,00    <---- Worst
            //   #2  276 859 300 iterations in 5 000,00 ms. Adjusted: 276 859 300,00
            //   #3  289 235 819 iterations in 5 000,00 ms. Adjusted: 289 235 819,00    <---- Best
            //   Worst-Best difference: 19 000 662,00 (7,03%)

            // .NET 10.0 (length = 7)
            // 1. ConvolveWith3d_Vector4SpanOpsInlined: 875 515 354 iterations in 15 000,00 ms. Adjusted for 5 000 ms: 291 838 447,45
            //   #1  291 112 478 iterations in 5 000,00 ms. Adjusted: 291 112 466,36    <---- Worst
            //   #2  292 252 152 iterations in 5 000,00 ms. Adjusted: 292 252 152,00    <---- Best
            //   #3  292 150 724 iterations in 5 000,00 ms. Adjusted: 292 150 724,00
            //   Worst-Best difference: 1 139 685,64 (0,39%)
            // 2. ConvolveWith6b_Vector256FmaMultiplyAddVector4: 859 825 074 iterations in 15 000,00 ms. Adjusted for 5 000 ms: 286 608 358,00 (-5 230 089,45 / 98,21%)
            //   #1  287 592 045 iterations in 5 000,00 ms. Adjusted: 287 592 045,00    <---- Best
            //   #2  286 213 785 iterations in 5 000,00 ms. Adjusted: 286 213 785,00
            //   #3  286 019 244 iterations in 5 000,00 ms. Adjusted: 286 019 244,00    <---- Worst
            //   Worst-Best difference: 1 572 801,00 (0,55%)
            // 3. ConvolveWith6_Vector256FmaMultiplyAdd: 836 517 709 iterations in 15 000,00 ms. Adjusted for 5 000 ms: 278 839 236,33 (-12 999 211,12 / 95,55%)
            //   #1  278 748 662 iterations in 5 000,00 ms. Adjusted: 278 748 662,00
            //   #2  278 647 618 iterations in 5 000,00 ms. Adjusted: 278 647 618,00    <---- Worst
            //   #3  279 121 429 iterations in 5 000,00 ms. Adjusted: 279 121 429,00    <---- Best
            //   Worst-Best difference: 473 811,00 (0,17%)
            // 4. ConvolveWith4d_Vector4SpanFusedMultiplyAddOpsInlined: 822 486 474 iterations in 15 000,00 ms. Adjusted for 5 000 ms: 274 162 158,00 (-17 676 289,45 / 93,94%)
            //   #1  273 859 165 iterations in 5 000,00 ms. Adjusted: 273 859 165,00    <---- Worst
            //   #2  274 308 866 iterations in 5 000,00 ms. Adjusted: 274 308 866,00
            //   #3  274 318 443 iterations in 5 000,00 ms. Adjusted: 274 318 443,00    <---- Best
            //   Worst-Best difference: 459 278,00 (0,17%)
            // 5. ConvolveWith5d_Vector128FmaMultiplyAddOpsInlined: 822 068 825 iterations in 15 000,00 ms. Adjusted for 5 000 ms: 274 022 941,67 (-17 815 505,79 / 93,90%)
            //   #1  273 633 548 iterations in 5 000,00 ms. Adjusted: 273 633 548,00    <---- Worst
            //   #2  274 269 985 iterations in 5 000,00 ms. Adjusted: 274 269 985,00    <---- Best
            //   #3  274 165 292 iterations in 5 000,00 ms. Adjusted: 274 165 292,00
            //   Worst-Best difference: 636 437,00 (0,23%)
            // 6. ConvolveWith1b2_VanillaColorInlined: 763 616 437 iterations in 15 000,00 ms. Adjusted for 5 000 ms: 254 538 812,33 (-37 299 635,12 / 87,22%)
            //   #1  240 045 682 iterations in 5 000,00 ms. Adjusted: 240 045 682,00    <---- Worst
            //   #2  258 065 008 iterations in 5 000,00 ms. Adjusted: 258 065 008,00
            //   #3  265 505 747 iterations in 5 000,00 ms. Adjusted: 265 505 747,00    <---- Best
            //   Worst-Best difference: 25 460 065,00 (10,61%)

            // .NET 10.0 (length = 8)
            // 1. ConvolveWith6b_Vector256FmaMultiplyAddVector4: 839 910 485 iterations in 15 000,00 ms. Adjusted for 5 000 ms: 279 970 161,67
            //   #1  280 173 262 iterations in 5 000,00 ms. Adjusted: 280 173 262,00
            //   #2  277 840 031 iterations in 5 000,00 ms. Adjusted: 277 840 031,00    <---- Worst
            //   #3  281 897 192 iterations in 5 000,00 ms. Adjusted: 281 897 192,00    <---- Best
            //   Worst-Best difference: 4 057 161,00 (1,46%)
            // 2. ConvolveWith6_Vector256FmaMultiplyAdd: 839 712 113 iterations in 15 000,00 ms. Adjusted for 5 000 ms: 279 904 037,67 (-66 124,00 / 99,98%)
            //   #1  279 575 137 iterations in 5 000,00 ms. Adjusted: 279 575 137,00    <---- Worst
            //   #2  279 933 047 iterations in 5 000,00 ms. Adjusted: 279 933 047,00
            //   #3  280 203 929 iterations in 5 000,00 ms. Adjusted: 280 203 929,00    <---- Best
            //   Worst-Best difference: 628 792,00 (0,22%)
            // 3. ConvolveWith3d_Vector4SpanOpsInlined: 838 302 022 iterations in 15 000,00 ms. Adjusted for 5 000 ms: 279 434 007,33 (-536 154,33 / 99,81%)
            //   #1  279 024 585 iterations in 5 000,00 ms. Adjusted: 279 024 585,00    <---- Worst
            //   #2  279 116 893 iterations in 5 000,00 ms. Adjusted: 279 116 893,00
            //   #3  280 160 544 iterations in 5 000,00 ms. Adjusted: 280 160 544,00    <---- Best
            //   Worst-Best difference: 1 135 959,00 (0,41%)
            // 4. ConvolveWith2c_Vector4ColorInlined: 788 960 174 iterations in 15 000,00 ms. Adjusted for 5 000 ms: 262 986 724,67 (-16 983 437,00 / 93,93%)
            //   #1  263 212 848 iterations in 5 000,00 ms. Adjusted: 263 212 848,00    <---- Best
            //   #2  262 835 052 iterations in 5 000,00 ms. Adjusted: 262 835 052,00    <---- Worst
            //   #3  262 912 274 iterations in 5 000,00 ms. Adjusted: 262 912 274,00
            //   Worst-Best difference: 377 796,00 (0,14%)
            // 5. ConvolveWith4d_Vector4SpanFusedMultiplyAddOpsInlined: 778 833 014 iterations in 15 000,00 ms. Adjusted for 5 000 ms: 259 610 999,46 (-20 359 162,20 / 92,73%)
            //   #1  260 244 509 iterations in 5 000,00 ms. Adjusted: 260 244 503,80    <---- Best
            //   #2  260 136 147 iterations in 5 000,00 ms. Adjusted: 260 136 136,59
            //   #3  258 452 358 iterations in 5 000,00 ms. Adjusted: 258 452 358,00    <---- Worst
            //   Worst-Best difference: 1 792 145,80 (0,69%)
            // 6. ConvolveWith5d_Vector128FmaMultiplyAddOpsInlined: 772 048 202 iterations in 15 000,00 ms. Adjusted for 5 000 ms: 257 349 400,67 (-22 620 761,00 / 91,92%)
            //   #1  256 628 111 iterations in 5 000,00 ms. Adjusted: 256 628 111,00    <---- Worst
            //   #2  257 640 355 iterations in 5 000,00 ms. Adjusted: 257 640 355,00
            //   #3  257 779 736 iterations in 5 000,00 ms. Adjusted: 257 779 736,00    <---- Best
            //   Worst-Best difference: 1 151 625,00 (0,45%)
            // 7. ConvolveWith1b2_VanillaColorInlined: 756 908 391 iterations in 15 000,00 ms. Adjusted for 5 000 ms: 252 302 797,00 (-27 667 364,67 / 90,12%)
            //   #1  238 665 884 iterations in 5 000,00 ms. Adjusted: 238 665 884,00    <---- Worst
            //   #2  255 868 443 iterations in 5 000,00 ms. Adjusted: 255 868 443,00
            //   #3  262 374 064 iterations in 5 000,00 ms. Adjusted: 262 374 064,00    <---- Best
            //   Worst-Best difference: 23 708 180,00 (9,93%)

            // .NET 10.0 (length = 9)
            // 1. ConvolveWith3d_Vector4SpanOpsInlined: 820 438 248 iterations in 15 000,00 ms. Adjusted for 5 000 ms: 273 479 416,00
            //   #1  273 119 483 iterations in 5 000,00 ms. Adjusted: 273 119 483,00    <---- Worst
            //   #2  273 767 781 iterations in 5 000,00 ms. Adjusted: 273 767 781,00    <---- Best
            //   #3  273 550 984 iterations in 5 000,00 ms. Adjusted: 273 550 984,00
            //   Worst-Best difference: 648 298,00 (0,24%)
            // 2. ConvolveWith6b_Vector256FmaMultiplyAddVector4: 814 664 180 iterations in 15 000,00 ms. Adjusted for 5 000 ms: 271 554 726,67 (-1 924 689,33 / 99,30%)
            //   #1  271 775 261 iterations in 5 000,00 ms. Adjusted: 271 775 261,00    <---- Best
            //   #2  271 550 733 iterations in 5 000,00 ms. Adjusted: 271 550 733,00
            //   #3  271 338 186 iterations in 5 000,00 ms. Adjusted: 271 338 186,00    <---- Worst
            //   Worst-Best difference: 437 075,00 (0,16%)
            // 3. ConvolveWith6_Vector256FmaMultiplyAdd: 798 118 448 iterations in 15 000,00 ms. Adjusted for 5 000 ms: 266 039 482,67 (-7 439 933,33 / 97,28%)
            //   #1  266 184 544 iterations in 5 000,00 ms. Adjusted: 266 184 544,00
            //   #2  265 325 183 iterations in 5 000,00 ms. Adjusted: 265 325 183,00    <---- Worst
            //   #3  266 608 721 iterations in 5 000,00 ms. Adjusted: 266 608 721,00    <---- Best
            //   Worst-Best difference: 1 283 538,00 (0,48%)
            // 4. ConvolveWith5d_Vector128FmaMultiplyAddOpsInlined: 745 678 465 iterations in 15 000,00 ms. Adjusted for 5 000 ms: 248 559 488,33 (-24 919 927,67 / 90,89%)
            //   #1  248 491 640 iterations in 5 000,00 ms. Adjusted: 248 491 640,00    <---- Worst
            //   #2  248 510 868 iterations in 5 000,00 ms. Adjusted: 248 510 868,00
            //   #3  248 675 957 iterations in 5 000,00 ms. Adjusted: 248 675 957,00    <---- Best
            //   Worst-Best difference: 184 317,00 (0,07%)
            // 5. ConvolveWith4d_Vector4SpanFusedMultiplyAddOpsInlined: 744 228 748 iterations in 15 000,00 ms. Adjusted for 5 000 ms: 248 076 249,33 (-25 403 166,67 / 90,71%)
            //   #1  247 480 605 iterations in 5 000,00 ms. Adjusted: 247 480 605,00    <---- Worst
            //   #2  249 055 890 iterations in 5 000,00 ms. Adjusted: 249 055 890,00    <---- Best
            //   #3  247 692 253 iterations in 5 000,00 ms. Adjusted: 247 692 253,00
            //   Worst-Best difference: 1 575 285,00 (0,64%)
            // 6. ConvolveWith1b2_VanillaColorInlined: 717 744 750 iterations in 15 000,00 ms. Adjusted for 5 000 ms: 239 248 250,00 (-34 231 166,00 / 87,48%)
            //   #1  233 205 516 iterations in 5 000,00 ms. Adjusted: 233 205 516,00    <---- Worst
            //   #2  236 945 408 iterations in 5 000,00 ms. Adjusted: 236 945 408,00
            //   #3  247 593 826 iterations in 5 000,00 ms. Adjusted: 247 593 826,00    <---- Best
            //   Worst-Best difference: 14 388 310,00 (6,17%)

            // .NET 10.0 (length = 11)
            // 1. ConvolveWith6b_Vector256FmaMultiplyAddVector4: 750 253 494 iterations in 15 000,00 ms. Adjusted for 5 000 ms: 250 084 496,35
            //   #1  249 090 299 iterations in 5 000,00 ms. Adjusted: 249 090 299,00
            //   #2  247 895 523 iterations in 5 000,00 ms. Adjusted: 247 895 518,04    <---- Worst
            //   #3  253 267 672 iterations in 5 000,00 ms. Adjusted: 253 267 672,00    <---- Best
            //   Worst-Best difference: 5 372 153,96 (2,17%)
            // 2. ConvolveWith3d_Vector4SpanOpsInlined: 737 750 102 iterations in 15 000,00 ms. Adjusted for 5 000 ms: 245 916 700,67 (-4 167 795,68 / 98,33%)
            //   #1  245 202 308 iterations in 5 000,00 ms. Adjusted: 245 202 308,00    <---- Worst
            //   #2  245 469 973 iterations in 5 000,00 ms. Adjusted: 245 469 973,00
            //   #3  247 077 821 iterations in 5 000,00 ms. Adjusted: 247 077 821,00    <---- Best
            //   Worst-Best difference: 1 875 513,00 (0,76%)
            // 3. ConvolveWith6_Vector256FmaMultiplyAdd: 733 327 573 iterations in 15 000,00 ms. Adjusted for 5 000 ms: 244 442 524,33 (-5 641 972,01 / 97,74%)
            //   #1  244 635 591 iterations in 5 000,00 ms. Adjusted: 244 635 591,00
            //   #2  245 522 565 iterations in 5 000,00 ms. Adjusted: 245 522 565,00    <---- Best
            //   #3  243 169 417 iterations in 5 000,00 ms. Adjusted: 243 169 417,00    <---- Worst
            //   Worst-Best difference: 2 353 148,00 (0,97%)
            // 4. ConvolveWith2c_Vector4ColorInlined: 675 644 146 iterations in 15 000,00 ms. Adjusted for 5 000 ms: 225 214 715,33 (-24 869 781,01 / 90,06%)
            //   #1  225 333 708 iterations in 5 000,00 ms. Adjusted: 225 333 708,00
            //   #2  225 371 877 iterations in 5 000,00 ms. Adjusted: 225 371 877,00    <---- Best
            //   #3  224 938 561 iterations in 5 000,00 ms. Adjusted: 224 938 561,00    <---- Worst
            //   Worst-Best difference: 433 316,00 (0,19%)
            // 5. ConvolveWith5d_Vector128FmaMultiplyAddOpsInlined: 669 809 223 iterations in 15 000,00 ms. Adjusted for 5 000 ms: 223 269 741,00 (-26 814 755,35 / 89,28%)
            //   #1  223 216 212 iterations in 5 000,00 ms. Adjusted: 223 216 212,00
            //   #2  223 636 030 iterations in 5 000,00 ms. Adjusted: 223 636 030,00    <---- Best
            //   #3  222 956 981 iterations in 5 000,00 ms. Adjusted: 222 956 981,00    <---- Worst
            //   Worst-Best difference: 679 049,00 (0,30%)
            // 6. ConvolveWith1b2_VanillaColorInlined: 651 403 938 iterations in 15 000,00 ms. Adjusted for 5 000 ms: 217 134 646,00 (-32 949 850,35 / 86,82%)
            //   #1  209 040 689 iterations in 5 000,00 ms. Adjusted: 209 040 689,00    <---- Worst
            //   #2  216 464 767 iterations in 5 000,00 ms. Adjusted: 216 464 767,00
            //   #3  225 898 482 iterations in 5 000,00 ms. Adjusted: 225 898 482,00    <---- Best
            //   Worst-Best difference: 16 857 793,00 (8,06%)
        }

        #endregion

        #endregion
    }
}
