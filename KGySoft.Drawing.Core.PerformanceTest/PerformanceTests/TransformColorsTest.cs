#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: TransformColorsTest.cs
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
#if NETCOREAPP3_0_OR_GREATER
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
#endif

using KGySoft.Drawing.Imaging;

using NUnit.Framework;

#endregion

namespace KGySoft.Drawing.PerformanceTests
{
    [TestFixture]
    internal class TransformColorsTest
    {
        #region Methods

        #region Static Methods

        private static Color32 TransformDarkenPerChannel32(Color32 c, float brightness, ColorChannels channels) => new Color32(c.A,
            (channels & ColorChannels.R) == ColorChannels.R ? (byte)(c.R * brightness) : c.R,
            (channels & ColorChannels.G) == ColorChannels.G ? (byte)(c.G * brightness) : c.G,
            (channels & ColorChannels.B) == ColorChannels.B ? (byte)(c.B * brightness) : c.B);

        private static Color32 TransformDarken32_1_Vanilla(Color32 c, float brightness) => new Color32(c.A,
            (byte)(c.R * brightness),
            (byte)(c.G * brightness),
            (byte)(c.B * brightness));

        private static Color32 TransformDarken32_2_Vector(Color32 c, float brightness)
        {
#if NET45_OR_GREATER || NETCOREAPP
            var rgbF = new Vector3(c.R, c.G, c.B) * brightness;
            return new Color32(c.A,
                (byte)rgbF.X,
                (byte)rgbF.Y,
                (byte)rgbF.Z);
#else
            return new Color32(c.A,
                (byte)(c.R * brightness),
                (byte)(c.G * brightness),
                (byte)(c.B * brightness));
#endif
        }

        private static Color32 TransformDarken32_3_IntrinsicsSse41(Color32 c, float brightness)
        {
#if NETCOREAPP3_0_OR_GREATER
            if (Sse41.IsSupported)
            {
                Vector128<float> bgrF = Sse2.ConvertToVector128Single(Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(c.Value).AsByte()));

                // bgrF *= brightness
                bgrF = Sse.Multiply(bgrF, Vector128.Create(brightness));

                Vector128<int> bgraI32 = Sse2.ConvertToVector128Int32WithTruncation(bgrF);

                return new Color32(Ssse3.Shuffle(bgraI32.AsByte(), VectorExtensions.PackLowBytesMask).WithElement(3, c.A).AsUInt32().ToScalar());
            }
#endif

            return new Color32(c.A,
                (byte)(c.R * brightness),
                (byte)(c.G * brightness),
                (byte)(c.B * brightness));
        }

        private static Color32 TransformDarken32_4_IntrinsicsSse3(Color32 c, float brightness)
        {
#if NETCOREAPP3_0_OR_GREATER
            if (Ssse3.IsSupported)
            {
                Vector128<int> bgraI32 = Vector128.Create(c.B, c.G, c.R, default);

                // bgrF = (float)(c.B, c.G, c.R, _)
                Vector128<float> bgrF = Sse2.ConvertToVector128Single(bgraI32);

                // bgrF *= brightness
                bgrF = Sse.Multiply(bgrF, Vector128.Create(brightness));

                bgraI32 = Sse2.ConvertToVector128Int32WithTruncation(bgrF);

                return new Color32(Ssse3.Shuffle(bgraI32.AsByte(), VectorExtensions.PackLowBytesMask).WithElement(3, c.A).AsUInt32().ToScalar());
            }

#endif

            return new Color32(c.A,
                (byte)(c.R * brightness),
                (byte)(c.G * brightness),
                (byte)(c.B * brightness));
        }

        private static Color32 TransformDarken32_5_IntrinsicsSse2(Color32 c, float brightness)
        {
#if NETCOREAPP3_0_OR_GREATER
            if (Sse2.IsSupported)
            {
                Vector128<int> bgraI32 = Vector128.Create(c.B, c.G, c.R, default);

                // bgrF = (float)(c.B, c.G, c.R, _)
                Vector128<float> bgrF = Sse2.ConvertToVector128Single(bgraI32);

                // bgrF *= brightness
                bgrF = Sse.Multiply(bgrF, Vector128.Create(brightness));

                bgraI32 = Sse2.ConvertToVector128Int32WithTruncation(bgrF);

                return new Color32(c.A, bgraI32.AsByte().GetElement(8), bgraI32.AsByte().GetElement(4), bgraI32.AsByte().GetElement(0));
            }

#endif

            return new Color32(c.A,
                (byte)(c.R * brightness),
                (byte)(c.G * brightness),
                (byte)(c.B * brightness));
        }

        private static Color32 TransformLightenPerChannel32(Color32 c, float brightness, ColorChannels channels) => new Color32(c.A,
            (channels & ColorChannels.R) == ColorChannels.R ? (byte)((Byte.MaxValue - c.R) * brightness + c.R) : c.R,
            (channels & ColorChannels.G) == ColorChannels.G ? (byte)((Byte.MaxValue - c.G) * brightness + c.G) : c.G,
            (channels & ColorChannels.B) == ColorChannels.B ? (byte)((Byte.MaxValue - c.B) * brightness + c.B) : c.B);

        private static Color32 TransformLighten32_1_Vanilla(Color32 c, float brightness) => new Color32(c.A,
            (byte)((Byte.MaxValue - c.R) * brightness + c.R),
            (byte)((Byte.MaxValue - c.G) * brightness + c.G),
            (byte)((Byte.MaxValue - c.B) * brightness + c.B));

        private static Color32 TransformLighten32_2_Vector(Color32 c, float brightness)
        {
#if NET45_OR_GREATER || NETCOREAPP
            var rgbF = new Vector3(c.R, c.G, c.B);
            Vector3 result = (VectorExtensions.Max8Bit3 - rgbF) * brightness + rgbF;

            return new Color32(c.A,
                (byte)result.X,
                (byte)result.Y,
                (byte)result.Z);
#else
            return new Color32(c.A,
                (byte)((Byte.MaxValue - c.R) * brightness + c.R),
                (byte)((Byte.MaxValue - c.G) * brightness + c.G),
                (byte)((Byte.MaxValue - c.B) * brightness + c.B));
#endif
        }

        private static Color32 TransformLighten32_3_IntrinsicsSse2(Color32 c, float brightness)
        {
#if NETCOREAPP3_0_OR_GREATER
            if (Sse2.IsSupported)
            {
                Vector128<int> bgraI32 = Vector128.Create(c.B, c.G, c.R, default);

                // bgrF = (float)(c.B, c.G, c.R, _)
                Vector128<float> bgrF = Sse2.ConvertToVector128Single(bgraI32);

                // bgrF = (255 - bgrF) * brightness + bgrF
                bgrF = Sse.Add(Sse.Multiply(Sse.Subtract(VectorExtensions.Max8BitF, bgrF), Vector128.Create(brightness)), bgrF);

                var bgraI32Byte = Sse2.ConvertToVector128Int32WithTruncation(bgrF).AsByte();

                return new Color32(c.A, bgraI32Byte.GetElement(8), bgraI32Byte.GetElement(4), bgraI32Byte.GetElement(0));
            }
#endif

            return new Color32(c.A,
                (byte)((Byte.MaxValue - c.R) * brightness + c.R),
                (byte)((Byte.MaxValue - c.G) * brightness + c.G),
                (byte)((Byte.MaxValue - c.B) * brightness + c.B));
        }

        private static Color32 TransformLighten32_4_IntrinsicsSse3(Color32 c, float brightness)
        {
#if NETCOREAPP3_0_OR_GREATER
            if (Ssse3.IsSupported)
            {
                Vector128<int> bgraI32 = Sse41.IsSupported
                    ? Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(c.Value).AsByte())
                    : Vector128.Create(c.B, c.G, c.R, default);

                // bgrF = (float)(c.B, c.G, c.R, _)
                Vector128<float> bgrF = Sse2.ConvertToVector128Single(bgraI32);

                // bgrF = (255 - bgrF) * brightness + bgrF
                bgrF = Sse.Add(Sse.Multiply(Sse.Subtract(VectorExtensions.Max8BitF, bgrF), Vector128.Create(brightness)), bgrF);

                bgraI32 = Sse2.ConvertToVector128Int32WithTruncation(bgrF);

                return new Color32(Ssse3.Shuffle(bgraI32.AsByte(), VectorExtensions.PackLowBytesMask).WithElement(3, c.A).AsUInt32().ToScalar());
            }
#endif

            return new Color32(c.A,
                (byte)((Byte.MaxValue - c.R) * brightness + c.R),
                (byte)((Byte.MaxValue - c.G) * brightness + c.G),
                (byte)((Byte.MaxValue - c.B) * brightness + c.B));
        }

        private static Color32 TransformLighten32_5_IntrinsicsSse41(Color32 c, float brightness)
        {
#if NETCOREAPP3_0_OR_GREATER
            if (Sse41.IsSupported)
            {
                Vector128<float> bgrF = Sse2.ConvertToVector128Single(Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(c.Value).AsByte()));

                // bgrF = (255 - bgrF) * brightness + bgrF
                bgrF = Sse.Add(Sse.Multiply(Sse.Subtract(VectorExtensions.Max8BitF, bgrF), Vector128.Create(brightness)), bgrF);

                Vector128<int> bgraI32 = Sse2.ConvertToVector128Int32WithTruncation(bgrF);

                return new Color32(Ssse3.Shuffle(bgraI32.AsByte(), VectorExtensions.PackLowBytesMask).WithElement(3, c.A).AsUInt32().ToScalar());
            }
#endif

            return new Color32(c.A,
                (byte)((Byte.MaxValue - c.R) * brightness + c.R),
                (byte)((Byte.MaxValue - c.G) * brightness + c.G),
                (byte)((Byte.MaxValue - c.B) * brightness + c.B));
        }

        private static Color32 TransformLighten32_6_IntrinsicsFma(Color32 c, float brightness)
        {
#if NETCOREAPP3_0_OR_GREATER
            if (Fma.IsSupported)
            {
                Vector128<float> bgrF = Sse2.ConvertToVector128Single(Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(c.Value).AsByte()));

                // bgrF = (255 - bgrF) * brightness + bgrF
                bgrF = Fma.MultiplyAdd(Sse.Subtract(VectorExtensions.Max8BitF, bgrF), Vector128.Create(brightness), bgrF);

                Vector128<int> bgraI32 = Sse2.ConvertToVector128Int32WithTruncation(bgrF);

                return new Color32(Ssse3.Shuffle(bgraI32.AsByte(), VectorExtensions.PackLowBytesMask).WithElement(3, c.A).AsUInt32().ToScalar());
            }
#endif

            return new Color32(c.A,
                (byte)((Byte.MaxValue - c.R) * brightness + c.R),
                (byte)((Byte.MaxValue - c.G) * brightness + c.G),
                (byte)((Byte.MaxValue - c.B) * brightness + c.B));
        }

        private static Color64 TransformDarkenPerChannel64(Color64 c, float brightness, ColorChannels channels) => new Color64(c.A,
            (channels & ColorChannels.R) == ColorChannels.R ? (ushort)(c.R * brightness) : c.R,
            (channels & ColorChannels.G) == ColorChannels.G ? (ushort)(c.G * brightness) : c.G,
            (channels & ColorChannels.B) == ColorChannels.B ? (ushort)(c.B * brightness) : c.B);

        private static Color64 TransformDarken64_1_Vanilla(Color64 c, float brightness) => new Color64(c.A,
            (ushort)(c.R * brightness),
            (ushort)(c.G * brightness),
            (ushort)(c.B * brightness));

        private static Color64 TransformDarken64_2_Vector(Color64 c, float brightness)
        {
#if NET45_OR_GREATER || NETCOREAPP
            var rgbF = new Vector3(c.R, c.G, c.B) * brightness;
            return new Color64(c.A,
                (ushort)rgbF.X,
                (ushort)rgbF.Y,
                (ushort)rgbF.Z);
#else
            return new Color64(c.A,
                (ushort)(c.R * brightness),
                (ushort)(c.G * brightness),
                (ushort)(c.B * brightness));
#endif
        }

        private static Color64 TransformDarken64_3_IntrinsicsSse41(Color64 c, float brightness)
        {
#if NETCOREAPP3_0_OR_GREATER
            if (Sse41.IsSupported)
            {
                Vector128<float> bgrF = Sse2.ConvertToVector128Single(Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(c.Value).AsUInt16()));

                // bgrF *= brightness
                bgrF = Sse.Multiply(bgrF, Vector128.Create(brightness));

                Vector128<int> bgraI32 = Sse2.ConvertToVector128Int32WithTruncation(bgrF);

                return new Color64(Sse41.PackUnsignedSaturate(bgraI32, bgraI32).WithElement(3, c.A).AsUInt64().ToScalar());
            }
#endif

            return new Color64(c.A,
                (byte)(c.R * brightness),
                (byte)(c.G * brightness),
                (byte)(c.B * brightness));
        }

        private static Color64 TransformLightenPerChannel64(Color64 c, float brightness, ColorChannels channels) => new Color64(c.A,
            (channels & ColorChannels.R) == ColorChannels.R ? (ushort)((UInt16.MaxValue - c.R) * brightness + c.R) : c.R,
            (channels & ColorChannels.G) == ColorChannels.G ? (ushort)((UInt16.MaxValue - c.G) * brightness + c.G) : c.G,
            (channels & ColorChannels.B) == ColorChannels.B ? (ushort)((UInt16.MaxValue - c.B) * brightness + c.B) : c.B);

        private static Color64 TransformLighten64_1_Vanilla(Color64 c, float brightness)
        {
            return new Color64(c.A,
                (ushort)((UInt16.MaxValue - c.R) * brightness + c.R),
                (ushort)((UInt16.MaxValue - c.G) * brightness + c.G),
                (ushort)((UInt16.MaxValue - c.B) * brightness + c.B));
        }

        private static Color64 TransformLighten64_2_Vector(Color64 c, float brightness)
        {
#if NET45_OR_GREATER || NETCOREAPP
            var rgbF = new Vector3(c.R, c.G, c.B);

            rgbF = (VectorExtensions.Max16Bit3 - rgbF) * brightness + rgbF;
            return new Color64(c.A,
                (ushort)rgbF.X,
                (ushort)rgbF.Y,
                (ushort)rgbF.Z);
#else
            return new Color64(c.A,
                (ushort)((UInt16.MaxValue - c.R) * brightness + c.R),
                (ushort)((UInt16.MaxValue - c.G) * brightness + c.G),
                (ushort)((UInt16.MaxValue - c.B) * brightness + c.B));
#endif
        }

        private static Color64 TransformLighten64_3_IntrinsicsSse41(Color64 c, float brightness)
        {
#if NETCOREAPP3_0_OR_GREATER
            if (Sse41.IsSupported)
            {
                // bgrF = (float)(c.B, c.G, c.R, _)
                Vector128<float> bgrF = Sse2.ConvertToVector128Single(Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(c.Value).AsUInt16()));

                // bgrF = (65535 - bgrF) * brightness + bgrF
                bgrF = Sse.Add(Sse.Multiply(Sse.Subtract(VectorExtensions.Max16BitF, bgrF), Vector128.Create(brightness)), bgrF);

                Vector128<int> bgraI32 = Sse2.ConvertToVector128Int32WithTruncation(bgrF);

                return new Color64(Sse41.PackUnsignedSaturate(bgraI32, bgraI32).WithElement(3, c.A).AsUInt64().ToScalar());
            }
#endif

            return new Color64(c.A,
                (ushort)((UInt16.MaxValue - c.R) * brightness + c.R),
                (ushort)((UInt16.MaxValue - c.G) * brightness + c.G),
                (ushort)((UInt16.MaxValue - c.B) * brightness + c.B));
        }

        private static Color64 TransformLighten64_4_IntrinsicsFma(Color64 c, float brightness)
        {
#if NETCOREAPP3_0_OR_GREATER
            if (Fma.IsSupported)
            {
                // bgrF = (float)(c.B, c.G, c.R, _)
                Vector128<float> bgrF = Sse2.ConvertToVector128Single(Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(c.Value).AsUInt16()));

                // bgrF = (65535 - bgrF) * brightness + bgrF
                bgrF = Fma.MultiplyAdd(Sse.Subtract(VectorExtensions.Max16BitF, bgrF), Vector128.Create(brightness), bgrF);

                Vector128<int> bgraI32 = Sse2.ConvertToVector128Int32WithTruncation(bgrF);

                return new Color64(Sse41.PackUnsignedSaturate(bgraI32, bgraI32).WithElement(3, c.A).AsUInt64().ToScalar());
            }
#endif

            return new Color64(c.A,
                (ushort)((UInt16.MaxValue - c.R) * brightness + c.R),
                (ushort)((UInt16.MaxValue - c.G) * brightness + c.G),
                (ushort)((UInt16.MaxValue - c.B) * brightness + c.B));
        }

        private static ColorF TransformDarkenPerChannelF(ColorF c, float brightness, ColorChannels channels)
        {
            c = c.Clip();
            return new ColorF(c.A,
                (channels & ColorChannels.R) == ColorChannels.R ? c.R * brightness : c.R,
                (channels & ColorChannels.G) == ColorChannels.G ? c.G * brightness : c.G,
                (channels & ColorChannels.B) == ColorChannels.B ? c.B * brightness : c.B);
        }

        private static ColorF TransformDarkenF_1_Vanilla(ColorF c, float brightness)
        {
            c = c.Clip();
            return new ColorF(c.A,
                c.R * brightness,
                c.G * brightness,
                c.B * brightness);
        }

        private static ColorF TransformDarkenF_2_Vector(ColorF c, float brightness)
        {
#if NET45_OR_GREATER || NETCOREAPP
            Vector3 rgbF = c.Rgb.ClipF() * brightness;
            return new ColorF(new Vector4(rgbF, c.A));
#else
            c = c.Clip();
            return new ColorF(c.A,
                c.R * brightness,
                c.G * brightness,
                c.B * brightness);
#endif
        }

        private static ColorF TransformDarkenF_3_Intrinsics(ColorF c, float brightness)
        {
#if NETCOREAPP3_0_OR_GREATER
            if (Sse.IsSupported)
                return new ColorF(Sse.Multiply(c.RgbaV128.ClipF(), Vector128.Create(brightness)).WithElement(3, c.A)); 
#endif
            c = c.Clip();
            return new ColorF(c.A,
                c.R * brightness,
                c.G * brightness,
                c.B * brightness);
        }

        private static ColorF TransformLightenPerChannelF(ColorF c, float brightness, ColorChannels channels)
        {
            c = c.Clip();
            return new ColorF(c.A,
                (channels & ColorChannels.R) == ColorChannels.R ? (1f - c.R) * brightness + c.R : c.R,
                (channels & ColorChannels.G) == ColorChannels.G ? (1f - c.G) * brightness + c.G : c.G,
                (channels & ColorChannels.B) == ColorChannels.B ? (1f - c.B) * brightness + c.B : c.B);
        }

        private static ColorF TransformLightenF_1_Vanilla(ColorF c, float brightness)
        {
            c = c.Clip();
            return new ColorF(c.A,
                (1f - c.R) * brightness + c.R,
                (1f - c.G) * brightness + c.G,
                (1f - c.B) * brightness + c.B);
        }

        private static ColorF TransformLightenF_2_Vector(ColorF c, float brightness)
        {
#if NET45_OR_GREATER || NETCOREAPP
            Vector3 rgbF = c.Rgb.ClipF();
            rgbF = (Vector3.One - rgbF) * brightness + rgbF;
            return new ColorF(new Vector4(rgbF, c.A));
#else
            c = c.Clip();
            return new ColorF(c.A,
                (1f - c.R) * brightness + c.R,
                (1f - c.G) * brightness + c.G,
                (1f - c.B) * brightness + c.B);
#endif
        }

        private static ColorF TransformLightenF_3_IntrinsicsSse(ColorF c, float brightness)
        {
#if NETCOREAPP3_0_OR_GREATER
            if (Sse.IsSupported)
            {
                Vector128<float> rgbF = c.RgbaV128.ClipF();

                // rgbF = (1 - rgbF) * brightness + rgbF
                rgbF = Sse.Add(Sse.Multiply(Sse.Subtract(VectorExtensions.OneF, rgbF), Vector128.Create(brightness)), rgbF);
                return new ColorF(rgbF.WithElement(3, c.A));
            }
#endif
            c = c.Clip();
            return new ColorF(c.A,
                (1f - c.R) * brightness + c.R,
                (1f - c.G) * brightness + c.G,
                (1f - c.B) * brightness + c.B);
        }

        private static ColorF TransformLightenF_4_IntrinsicsFma(ColorF c, float brightness)
        {
#if NETCOREAPP3_0_OR_GREATER
            if (Fma.IsSupported)
            {
                Vector128<float> rgbF = c.RgbaV128.ClipF();

                // rgbF = (1 - rgbF) * brightness + rgbF
                rgbF = Fma.MultiplyAdd(Sse.Subtract(VectorExtensions.OneF, rgbF), Vector128.Create(brightness), rgbF);
                return new ColorF(rgbF.WithElement(3, c.A));
            }
#endif
            c = c.Clip();
            return new ColorF(c.A,
                (1f - c.R) * brightness + c.R,
                (1f - c.G) * brightness + c.G,
                (1f - c.B) * brightness + c.B);
        }

        private static Color32 TransformContrastPerChannel32(Color32 c, float contrast, ColorChannels channels) => new Color32(c.A,
            (channels & ColorChannels.R) == ColorChannels.R ? ((int)((((float)c.R / Byte.MaxValue - 0.5f) * contrast + 0.5f) * Byte.MaxValue)).ClipToByte() : c.R,
            (channels & ColorChannels.G) == ColorChannels.G ? ((int)((((float)c.G / Byte.MaxValue - 0.5f) * contrast + 0.5f) * Byte.MaxValue)).ClipToByte() : c.G,
            (channels & ColorChannels.B) == ColorChannels.B ? ((int)((((float)c.B / Byte.MaxValue - 0.5f) * contrast + 0.5f) * Byte.MaxValue)).ClipToByte() : c.B);

        private static Color32 TransformContrast32_1_Vanilla(Color32 c, float contrast)
        {
            return new Color32(c.A,
                ((int)((((float)c.R / Byte.MaxValue - 0.5f) * contrast + 0.5f) * Byte.MaxValue)).ClipToByte(),
                ((int)((((float)c.G / Byte.MaxValue - 0.5f) * contrast + 0.5f) * Byte.MaxValue)).ClipToByte(),
                ((int)((((float)c.B / Byte.MaxValue - 0.5f) * contrast + 0.5f) * Byte.MaxValue)).ClipToByte());
        }

        private static Color32 TransformContrast32_2_Vector(Color32 c, float contrast)
        {
#if NET45_OR_GREATER || NETCOREAPP
            var rgbF = new Vector3(c.R, c.G, c.B);
            rgbF = (((rgbF.Div(Byte.MaxValue) - VectorExtensions.Half3) * contrast + VectorExtensions.Half3) * Byte.MaxValue).Clip(Vector3.Zero, VectorExtensions.Max8Bit3);

            return new Color32(c.A,
                (byte)rgbF.X,
                (byte)rgbF.Y,
                (byte)rgbF.Z);
#else
            return new Color32(c.A,
                ((int)((((float)c.R / Byte.MaxValue - 0.5f) * contrast + 0.5f) * Byte.MaxValue)).ClipToByte(),
                ((int)((((float)c.G / Byte.MaxValue - 0.5f) * contrast + 0.5f) * Byte.MaxValue)).ClipToByte(),
                ((int)((((float)c.B / Byte.MaxValue - 0.5f) * contrast + 0.5f) * Byte.MaxValue)).ClipToByte());
#endif
        }

        private static Color32 TransformContrast32_3_IntrinsicsFma(Color32 c, float contrast)
        {
#if NETCOREAPP3_0_OR_GREATER
            if (Fma.IsSupported)
            {
                // bgrF = (float)(c.B, c.G, c.R, _)
                Vector128<float> bgrF = Sse2.ConvertToVector128Single(Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(c.Value).AsByte()));

                // bgrF = ((bgrF / 255f - 0.5f) * contrast + 0.5f) * 255f
                bgrF = Sse.Multiply(Fma.MultiplyAdd(Sse.Subtract(Sse.Divide(bgrF, VectorExtensions.Max8BitF), VectorExtensions.HalfF), Vector128.Create(contrast), VectorExtensions.HalfF), VectorExtensions.Max8BitF);

                Vector128<int> bgraI32 = Sse2.ConvertToVector128Int32WithTruncation(bgrF).Clip(Vector128<int>.Zero, VectorExtensions.Max8BitI32);

                return new Color32(Ssse3.Shuffle(bgraI32.AsByte(), VectorExtensions.PackLowBytesMask).WithElement(3, c.A).AsUInt32().ToScalar());
            }
#endif
            return new Color32(c.A,
                ((int)((((float)c.R / Byte.MaxValue - 0.5f) * contrast + 0.5f) * Byte.MaxValue)).ClipToByte(),
                ((int)((((float)c.G / Byte.MaxValue - 0.5f) * contrast + 0.5f) * Byte.MaxValue)).ClipToByte(),
                ((int)((((float)c.B / Byte.MaxValue - 0.5f) * contrast + 0.5f) * Byte.MaxValue)).ClipToByte());
        }

        private static Color32 TransformContrast32_4_IntrinsicsSse41(Color32 c, float contrast)
        {
#if NETCOREAPP3_0_OR_GREATER
            if (Sse41.IsSupported)
            {
                // bgrF = (float)(c.B, c.G, c.R, _)
                Vector128<float> bgrF = Sse2.ConvertToVector128Single(Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(c.Value).AsByte()));

                // bgrF = ((bgrF / 255f - 0.5f) * contrast + 0.5f) * 255f
                bgrF = Sse.Multiply(Sse.Add(Sse.Multiply(Sse.Subtract(Sse.Divide(bgrF, VectorExtensions.Max8BitF), VectorExtensions.HalfF), Vector128.Create(contrast)), VectorExtensions.HalfF), VectorExtensions.Max8BitF);

                Vector128<int> bgraI32 = Sse2.ConvertToVector128Int32WithTruncation(bgrF).Clip(Vector128<int>.Zero, VectorExtensions.Max8BitI32);

                return new Color32(Ssse3.Shuffle(bgraI32.AsByte(), VectorExtensions.PackLowBytesMask).WithElement(3, c.A).AsUInt32().ToScalar());
            }
#endif
            return new Color32(c.A,
                ((int)((((float)c.R / Byte.MaxValue - 0.5f) * contrast + 0.5f) * Byte.MaxValue)).ClipToByte(),
                ((int)((((float)c.G / Byte.MaxValue - 0.5f) * contrast + 0.5f) * Byte.MaxValue)).ClipToByte(),
                ((int)((((float)c.B / Byte.MaxValue - 0.5f) * contrast + 0.5f) * Byte.MaxValue)).ClipToByte());
        }

        private static Color32 TransformContrast32_5_IntrinsicsSse3(Color32 c, float contrast)
        {
#if NETCOREAPP3_0_OR_GREATER
            if (Ssse3.IsSupported)
            {
                // bgrF = (float)(c.B, c.G, c.R, _)
                Vector128<float> bgrF = Sse2.ConvertToVector128Single(Vector128.Create(c.B, c.G, c.R, default));

                // bgrF = ((bgrF / 255f - 0.5f) * contrast + 0.5f) * 255f
                bgrF = Sse.Multiply(Sse.Add(Sse.Multiply(Sse.Subtract(Sse.Divide(bgrF, VectorExtensions.Max8BitF), VectorExtensions.HalfF), Vector128.Create(contrast)), VectorExtensions.HalfF), VectorExtensions.Max8BitF);

                Vector128<int> bgraI32 = Sse2.ConvertToVector128Int32WithTruncation(bgrF.Clip(Vector128<float>.Zero, VectorExtensions.Max8BitF));

                return new Color32(Ssse3.Shuffle(bgraI32.AsByte(), VectorExtensions.PackLowBytesMask).WithElement(3, c.A).AsUInt32().ToScalar());
            }
#endif
            return new Color32(c.A,
                ((int)((((float)c.R / Byte.MaxValue - 0.5f) * contrast + 0.5f) * Byte.MaxValue)).ClipToByte(),
                ((int)((((float)c.G / Byte.MaxValue - 0.5f) * contrast + 0.5f) * Byte.MaxValue)).ClipToByte(),
                ((int)((((float)c.B / Byte.MaxValue - 0.5f) * contrast + 0.5f) * Byte.MaxValue)).ClipToByte());
        }

        private static Color32 TransformContrast32_6_IntrinsicsSse2(Color32 c, float contrast)
        {
#if NETCOREAPP3_0_OR_GREATER
            if (Sse2.IsSupported)
            {
                // bgrF = (float)(c.B, c.G, c.R, _)
                Vector128<float> bgrF = Sse2.ConvertToVector128Single(Vector128.Create(c.B, c.G, c.R, default));

                // bgrF = ((bgrF / 255f - 0.5f) * contrast + 0.5f) * 255f
                bgrF = Sse.Multiply(Sse.Add(Sse.Multiply(Sse.Subtract(Sse.Divide(bgrF, VectorExtensions.Max8BitF), VectorExtensions.HalfF), Vector128.Create(contrast)), VectorExtensions.HalfF), VectorExtensions.Max8BitF);

                Vector128<int> bgraI32 = Sse2.ConvertToVector128Int32WithTruncation(bgrF.Clip(Vector128<float>.Zero, VectorExtensions.Max8BitF));

                return new Color32(c.A, bgraI32.AsByte().GetElement(8), bgraI32.AsByte().GetElement(4), bgraI32.AsByte().GetElement(0));
            }
#endif
            return new Color32(c.A,
                ((int)((((float)c.R / Byte.MaxValue - 0.5f) * contrast + 0.5f) * Byte.MaxValue)).ClipToByte(),
                ((int)((((float)c.G / Byte.MaxValue - 0.5f) * contrast + 0.5f) * Byte.MaxValue)).ClipToByte(),
                ((int)((((float)c.B / Byte.MaxValue - 0.5f) * contrast + 0.5f) * Byte.MaxValue)).ClipToByte());
        }

        private static Color64 TransformContrastPerChannel64(Color64 c, float contrast, ColorChannels channels) => new Color64(c.A,
            (channels & ColorChannels.R) == ColorChannels.R ? ((int)((((float)c.R / UInt16.MaxValue - 0.5f) * contrast + 0.5f) * UInt16.MaxValue)).ClipToUInt16() : c.R,
            (channels & ColorChannels.G) == ColorChannels.G ? ((int)((((float)c.G / UInt16.MaxValue - 0.5f) * contrast + 0.5f) * UInt16.MaxValue)).ClipToUInt16() : c.G,
            (channels & ColorChannels.B) == ColorChannels.B ? ((int)((((float)c.B / UInt16.MaxValue - 0.5f) * contrast + 0.5f) * UInt16.MaxValue)).ClipToUInt16() : c.B);

        private static Color64 TransformContrast64_1_Vanilla(Color64 c, float contrast) => new(c.A,
            ((int)((((float)c.R / UInt16.MaxValue - 0.5f) * contrast + 0.5f) * UInt16.MaxValue)).ClipToUInt16(),
            ((int)((((float)c.G / UInt16.MaxValue - 0.5f) * contrast + 0.5f) * UInt16.MaxValue)).ClipToUInt16(),
            ((int)((((float)c.B / UInt16.MaxValue - 0.5f) * contrast + 0.5f) * UInt16.MaxValue)).ClipToUInt16());

        private static Color64 TransformContrast64_2_Vector(Color64 c, float contrast)
        {
#if NET45_OR_GREATER || NETCOREAPP
            var rgbF = new Vector3(c.R, c.G, c.B);
            rgbF = (((rgbF.Div(UInt16.MaxValue) - VectorExtensions.Half3) * contrast + VectorExtensions.Half3) * UInt16.MaxValue).Clip(Vector3.Zero, VectorExtensions.Max16Bit3);

            return new Color64(c.A,
                (ushort)rgbF.X,
                (ushort)rgbF.Y,
                (ushort)rgbF.Z);
#else
            return new Color64(c.A,
                ((int)((((float)c.R / UInt16.MaxValue - 0.5f) * contrast + 0.5f) * UInt16.MaxValue)).ClipToUInt16(),
                ((int)((((float)c.G / UInt16.MaxValue - 0.5f) * contrast + 0.5f) * UInt16.MaxValue)).ClipToUInt16(),
                ((int)((((float)c.B / UInt16.MaxValue - 0.5f) * contrast + 0.5f) * UInt16.MaxValue)).ClipToUInt16());
#endif
        }

        private static Color64 TransformContrast64_3_IntrinsicsFma(Color64 c, float contrast)
        {
#if NETCOREAPP3_0_OR_GREATER
            if (Fma.IsSupported)
            {
                // bgrF = (float)(c.B, c.G, c.R, _)
                Vector128<float> bgrF = Sse2.ConvertToVector128Single(Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(c.Value).AsUInt16()));

                // bgrF = ((bgrF / 65535f - 0.5f) * contrast + 0.5f) * 65535f
                bgrF = Sse.Multiply(Fma.MultiplyAdd(Sse.Subtract(Sse.Divide(bgrF, VectorExtensions.Max16BitF), VectorExtensions.HalfF), Vector128.Create(contrast), VectorExtensions.HalfF), VectorExtensions.Max16BitF);

                Vector128<int> bgraI32 = Sse2.ConvertToVector128Int32WithTruncation(bgrF).Clip(Vector128<int>.Zero, VectorExtensions.Max16BitI32);

                return new Color64(Sse41.PackUnsignedSaturate(bgraI32, bgraI32).WithElement(3, c.A).AsUInt64().ToScalar());
            }
#endif
            return new Color64(c.A,
                ((int)((((float)c.R / UInt16.MaxValue - 0.5f) * contrast + 0.5f) * UInt16.MaxValue)).ClipToUInt16(),
                ((int)((((float)c.G / UInt16.MaxValue - 0.5f) * contrast + 0.5f) * UInt16.MaxValue)).ClipToUInt16(),
                ((int)((((float)c.B / UInt16.MaxValue - 0.5f) * contrast + 0.5f) * UInt16.MaxValue)).ClipToUInt16());
        }

        private static Color64 TransformContrast64_4_IntrinsicsSse41(Color64 c, float contrast)
        {
#if NETCOREAPP3_0_OR_GREATER
            if (Sse41.IsSupported)
            {
                // bgrF = (float)(c.B, c.G, c.R, _)
                Vector128<float> bgrF = Sse2.ConvertToVector128Single(Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(c.Value).AsUInt16()));

                // bgrF = ((bgrF / 65535f - 0.5f) * contrast + 0.5f) * 65535f
                bgrF = Sse.Multiply(Sse.Add(Sse.Multiply(Sse.Subtract(Sse.Divide(bgrF, VectorExtensions.Max16BitF), VectorExtensions.HalfF), Vector128.Create(contrast)), VectorExtensions.HalfF), VectorExtensions.Max16BitF);

                Vector128<int> bgraI32 = Sse2.ConvertToVector128Int32WithTruncation(bgrF).Clip(Vector128<int>.Zero, VectorExtensions.Max16BitI32);

                return new Color64(Sse41.PackUnsignedSaturate(bgraI32, bgraI32).WithElement(3, c.A).AsUInt64().ToScalar());
            }
#endif
            return new Color64(c.A,
                ((int)((((float)c.R / UInt16.MaxValue - 0.5f) * contrast + 0.5f) * UInt16.MaxValue)).ClipToUInt16(),
                ((int)((((float)c.G / UInt16.MaxValue - 0.5f) * contrast + 0.5f) * UInt16.MaxValue)).ClipToUInt16(),
                ((int)((((float)c.B / UInt16.MaxValue - 0.5f) * contrast + 0.5f) * UInt16.MaxValue)).ClipToUInt16());
        }

        private static ColorF TransformContrastPerChannelF(ColorF c, float contrast, ColorChannels channels)
        {
            c = c.Clip();
            return new ColorF(c.A,
                (channels & ColorChannels.R) == ColorChannels.R ? (c.R - 0.5f) * contrast + 0.5f : c.R,
                (channels & ColorChannels.G) == ColorChannels.G ? (c.G - 0.5f) * contrast + 0.5f : c.G,
                (channels & ColorChannels.B) == ColorChannels.B ? (c.B - 0.5f) * contrast + 0.5f : c.B);
        }

        private static ColorF TransformContrastF_1_Vanilla(ColorF c, float contrast)
        {
            c = c.Clip();
            return new ColorF(c.A,
                (c.R - 0.5f) * contrast + 0.5f,
                (c.G - 0.5f) * contrast + 0.5f,
                (c.B - 0.5f) * contrast + 0.5f);
        }

        private static ColorF TransformContrastF_2_Vector(ColorF c, float contrast)
        {
#if NET45_OR_GREATER || NETCOREAPP
            Vector3 rgbF = c.Rgb.ClipF();
            rgbF = (rgbF - VectorExtensions.Half3) * contrast + VectorExtensions.Half3;
            return new ColorF(new Vector4(rgbF, c.A));
#else
            c = c.Clip();
            return new ColorF(c.A,
                (c.R - 0.5f) * contrast + 0.5f,
                (c.G - 0.5f) * contrast + 0.5f,
                (c.B - 0.5f) * contrast + 0.5f);
#endif
        }

        private static ColorF TransformContrastF_3_IntrinsicsFma(ColorF c, float contrast)
        {
#if NETCOREAPP3_0_OR_GREATER
            if (Fma.IsSupported)
            {
                // rgbaF = (c - 0.5f) * contrast + 0.5f
                Vector128<float> rgbaF = Fma.MultiplyAdd(Sse.Subtract(c.RgbaV128.ClipF(), VectorExtensions.HalfF), Vector128.Create(contrast), VectorExtensions.HalfF);
                return new ColorF(rgbaF.WithElement(3, c.A));
            }
#endif
            c = c.Clip();
            return new ColorF(c.A,
                (c.R - 0.5f) * contrast + 0.5f,
                (c.G - 0.5f) * contrast + 0.5f,
                (c.B - 0.5f) * contrast + 0.5f);
        }

        private static ColorF TransformContrastF_4_IntrinsicsSse(ColorF c, float contrast)
        {
#if NETCOREAPP3_0_OR_GREATER
            if (Sse.IsSupported)
            {
                // rgbaF = (c - 0.5f) * contrast + 0.5f
                Vector128<float> rgbaF = Sse.Add(Sse.Multiply(Sse.Subtract(c.RgbaV128.ClipF(), VectorExtensions.HalfF), Vector128.Create(contrast)), VectorExtensions.HalfF);
                return new ColorF(rgbaF.WithElement(3, c.A));
            }
#endif
            c = c.Clip();
            return new ColorF(c.A,
                (c.R - 0.5f) * contrast + 0.5f,
                (c.G - 0.5f) * contrast + 0.5f,
                (c.B - 0.5f) * contrast + 0.5f);
        }

        #endregion

        #region Instance Methods

        [Test]
        public void TransformDarken32Test()
        {
            const int iterations = 10_000_000;
            Color32 color = new Color32(128, 255, 64);
            const float factor = 0.5f;

            Color32 expected = TransformDarkenPerChannel32(color, factor, ColorChannels.Rgb);
            Console.WriteLine($"{"Expected color:",-50} {expected}");

            void DoAssert(Expression<Func<Color32>> e)
            {
                var m = (MethodCallExpression)e.Body;
                Color32 actual = e.Compile().Invoke();
                Console.WriteLine($"{$"{m.Method.Name}:",-50} {actual}");
                Assert.AreEqual(expected, actual);
            }

            DoAssert(() => TransformDarken32_1_Vanilla(color, factor));
            DoAssert(() => TransformDarken32_2_Vector(color, factor));
            DoAssert(() => TransformDarken32_3_IntrinsicsSse41(color, factor));
            DoAssert(() => TransformDarken32_4_IntrinsicsSse3(color, factor));
            DoAssert(() => TransformDarken32_5_IntrinsicsSse2(color, factor));

            new PerformanceTest<Color32>
                {
                    TestName = nameof(TransformDarken32Test),
                    TestTime = 2000,
                    //Iterations = 10_000_000,
                    Repeat = 3
                }
                .AddCase(() => TransformDarkenPerChannel32(color, factor, ColorChannels.Rgb), nameof(TransformDarkenPerChannel32))
                .AddCase(() => TransformDarken32_1_Vanilla(color, factor), nameof(TransformDarken32_1_Vanilla))
                .AddCase(() => TransformDarken32_2_Vector(color, factor), nameof(TransformDarken32_2_Vector))
                .AddCase(() => TransformDarken32_3_IntrinsicsSse41(color, factor), nameof(TransformDarken32_3_IntrinsicsSse41))
                .AddCase(() => TransformDarken32_4_IntrinsicsSse3(color, factor), nameof(TransformDarken32_4_IntrinsicsSse3))
                .AddCase(() => TransformDarken32_5_IntrinsicsSse2(color, factor), nameof(TransformDarken32_5_IntrinsicsSse2))
                .DoTest()
                .DumpResults(Console.Out);

            // .NET 10:
            //  1. TransformDarken32_3_IntrinsicsSse41: 392 997 992 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 130 999 330,67
            //    #1  131 054 334 iterations in 2 000,00 ms. Adjusted: 131 054 334,00
            //    #2  130 399 709 iterations in 2 000,00 ms. Adjusted: 130 399 709,00	 <---- Worst
            //    #3  131 543 949 iterations in 2 000,00 ms. Adjusted: 131 543 949,00	 <---- Best
            //    Worst-Best difference: 1 144 240,00 (0,88%)
            //  2. TransformDarken32_4_IntrinsicsSse3: 377 112 546 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 125 704 182,00 (-5 295 148,67 / 95,96%)
            //    #1  125 935 519 iterations in 2 000,00 ms. Adjusted: 125 935 519,00	 <---- Best
            //    #2  125 613 568 iterations in 2 000,00 ms. Adjusted: 125 613 568,00
            //    #3  125 563 459 iterations in 2 000,00 ms. Adjusted: 125 563 459,00	 <---- Worst
            //    Worst-Best difference: 372 060,00 (0,30%)
            //  3. TransformDarken32_5_IntrinsicsSse2: 359 144 952 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 119 714 982,01 (-11 284 348,66 / 91,39%)
            //    #1  119 881 440 iterations in 2 000,00 ms. Adjusted: 119 881 440,00
            //    #2  119 365 187 iterations in 2 000,00 ms. Adjusted: 119 365 181,03	 <---- Worst
            //    #3  119 898 325 iterations in 2 000,00 ms. Adjusted: 119 898 325,00	 <---- Best
            //    Worst-Best difference: 533 143,97 (0,45%)
            //  4. TransformDarken32_1_Vanilla: 338 729 904 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 112 909 913,58 (-18 089 417,09 / 86,19%)
            //    #1  112 591 546 iterations in 2 000,00 ms. Adjusted: 112 591 382,74	 <---- Worst
            //    #2  113 300 311 iterations in 2 000,00 ms. Adjusted: 113 300 311,00	 <---- Best
            //    #3  112 838 047 iterations in 2 000,00 ms. Adjusted: 112 838 047,00
            //    Worst-Best difference: 708 928,26 (0,63%)
            //  5. TransformDarken32_2_Vector: 313 788 317 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 104 596 105,67 (-26 403 225,00 / 79,84%)
            //    #1  102 028 154 iterations in 2 000,00 ms. Adjusted: 102 028 154,00	 <---- Worst
            //    #2  104 892 734 iterations in 2 000,00 ms. Adjusted: 104 892 734,00
            //    #3  106 867 429 iterations in 2 000,00 ms. Adjusted: 106 867 429,00	 <---- Best
            //    Worst-Best difference: 4 839 275,00 (4,74%)
            //  6. TransformDarken32PerChannel: 286 434 078 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 95 478 024,43 (-35 521 306,24 / 72,88%)
            //    #1  87 619 203 iterations in 2 000,00 ms. Adjusted: 87 619 203,00	 <---- Worst
            //    #2  94 440 101 iterations in 2 000,00 ms. Adjusted: 94 440 096,28
            //    #3  104 374 774 iterations in 2 000,00 ms. Adjusted: 104 374 774,00	 <---- Best
            //    Worst-Best difference: 16 755 571,00 (19,12%)

            // .NET 8:
            // 1. TransformDarken32_3_IntrinsicsSse41: 392 204 274 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 130 734 758,00
            //   #1  130 353 661 iterations in 2 000,00 ms. Adjusted: 130 353 661,00	 <---- Worst
            //   #2  130 580 560 iterations in 2 000,00 ms. Adjusted: 130 580 560,00
            //   #3  131 270 053 iterations in 2 000,00 ms. Adjusted: 131 270 053,00	 <---- Best
            //   Worst-Best difference: 916 392,00 (0,70%)
            // 2. TransformDarken32_4_IntrinsicsSse3: 376 198 775 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 125 399 589,58 (-5 335 168,42 / 95,92%)
            //   #1  125 287 643 iterations in 2 000,00 ms. Adjusted: 125 287 643,00
            //   #2  124 964 354 iterations in 2 000,00 ms. Adjusted: 124 964 347,75	 <---- Worst
            //   #3  125 946 778 iterations in 2 000,00 ms. Adjusted: 125 946 778,00	 <---- Best
            //   Worst-Best difference: 982 430,25 (0,79%)
            // 3. TransformDarken32_5_IntrinsicsSse2: 358 365 710 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 119 455 236,67 (-11 279 521,33 / 91,37%)
            //   #1  119 612 408 iterations in 2 000,00 ms. Adjusted: 119 612 408,00	 <---- Best
            //   #2  119 472 077 iterations in 2 000,00 ms. Adjusted: 119 472 077,00
            //   #3  119 281 225 iterations in 2 000,00 ms. Adjusted: 119 281 225,00	 <---- Worst
            //   Worst-Best difference: 331 183,00 (0,28%)
            // 4. TransformDarken32_1_Vanilla: 334 681 824 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 111 560 606,15 (-19 174 151,85 / 85,33%)
            //   #1  112 294 055 iterations in 2 000,00 ms. Adjusted: 112 294 055,00	 <---- Best
            //   #2  111 107 509 iterations in 2 000,00 ms. Adjusted: 111 107 503,44	 <---- Worst
            //   #3  111 280 260 iterations in 2 000,00 ms. Adjusted: 111 280 260,00
            //   Worst-Best difference: 1 186 551,56 (1,07%)
            // 5. TransformDarken32_2_Vector: 318 601 746 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 106 200 582,00 (-24 534 176,00 / 81,23%)
            //   #1  106 034 869 iterations in 2 000,00 ms. Adjusted: 106 034 869,00
            //   #2  105 859 686 iterations in 2 000,00 ms. Adjusted: 105 859 686,00	 <---- Worst
            //   #3  106 707 191 iterations in 2 000,00 ms. Adjusted: 106 707 191,00	 <---- Best
            //   Worst-Best difference: 847 505,00 (0,80%)
            // 6. TransformDarken32PerChannel: 311 715 361 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 103 905 118,63 (-26 829 639,37 / 79,48%)
            //   #1  97 870 787 iterations in 2 000,00 ms. Adjusted: 97 870 787,00	 <---- Worst
            //   #2  101 999 276 iterations in 2 000,00 ms. Adjusted: 101 999 270,90
            //   #3  111 845 298 iterations in 2 000,00 ms. Adjusted: 111 845 298,00	 <---- Best
            //   Worst-Best difference: 13 974 511,00 (14,28%)
        }

        [Test]
        public void TransformLighten32Test()
        {
            const int iterations = 10_000_000;
            Color32 color = new Color32(128, 255, 64);
            const float factor = 0.5f;

            Color32 expected = TransformLightenPerChannel32(color, factor, ColorChannels.Rgb);
            Console.WriteLine($"{"Expected color:",-50} {expected}");

            void DoAssert(Expression<Func<Color32>> e)
            {
                var m = (MethodCallExpression)e.Body;
                Color32 actual = e.Compile().Invoke();
                Console.WriteLine($"{$"{m.Method.Name}:",-50} {actual}");
                Assert.AreEqual(expected, actual);
            }

            DoAssert(() => TransformLighten32_1_Vanilla(color, factor));
            DoAssert(() => TransformLighten32_2_Vector(color, factor));
            DoAssert(() => TransformLighten32_3_IntrinsicsSse2(color, factor));
            DoAssert(() => TransformLighten32_4_IntrinsicsSse3(color, factor));
            DoAssert(() => TransformLighten32_5_IntrinsicsSse41(color, factor));
            DoAssert(() => TransformLighten32_6_IntrinsicsFma(color, factor));

            new PerformanceTest<Color32>
                {
                    TestName = nameof(TransformLighten32Test),
                    TestTime = 2000,
                    //Iterations = 10_000_000,
                    Repeat = 3
                }
                .AddCase(() => TransformLightenPerChannel32(color, factor, ColorChannels.Rgb), nameof(TransformLightenPerChannel32))
                .AddCase(() => TransformLighten32_1_Vanilla(color, factor), nameof(TransformLighten32_1_Vanilla))
                .AddCase(() => TransformLighten32_2_Vector(color, factor), nameof(TransformLighten32_2_Vector))
                .AddCase(() => TransformLighten32_3_IntrinsicsSse2(color, factor), nameof(TransformLighten32_3_IntrinsicsSse2))
                .AddCase(() => TransformLighten32_4_IntrinsicsSse3(color, factor), nameof(TransformLighten32_4_IntrinsicsSse3))
                .AddCase(() => TransformLighten32_5_IntrinsicsSse41(color, factor), nameof(TransformLighten32_5_IntrinsicsSse41))
                .AddCase(() => TransformLighten32_6_IntrinsicsFma(color, factor), nameof(TransformLighten32_6_IntrinsicsFma))
                .DoTest()
                .DumpResults(Console.Out);

            // .NET 10:
            // 1. TransformLighten32_6_IntrinsicsFma: 373 493 072 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 124 497 690,67
            //   #1  124 790 872 iterations in 2 000,00 ms. Adjusted: 124 790 872,00	 <---- Best
            //   #2  124 282 981 iterations in 2 000,00 ms. Adjusted: 124 282 981,00	 <---- Worst
            //   #3  124 419 219 iterations in 2 000,00 ms. Adjusted: 124 419 219,00
            //   Worst-Best difference: 507 891,00 (0,41%)
            // 2. TransformLighten32_5_IntrinsicsSse41: 356 824 413 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 118 941 465,07 (-5 556 225,59 / 95,54%)
            //   #1  118 520 568 iterations in 2 000,00 ms. Adjusted: 118 520 550,22	 <---- Worst
            //   #2  119 112 014 iterations in 2 000,00 ms. Adjusted: 119 112 014,00
            //   #3  119 191 831 iterations in 2 000,00 ms. Adjusted: 119 191 831,00	 <---- Best
            //   Worst-Best difference: 671 280,78 (0,57%)
            // 3. TransformLighten32_4_IntrinsicsSse3: 355 267 236 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 118 422 412,00 (-6 075 278,67 / 95,12%)
            //   #1  118 590 747 iterations in 2 000,00 ms. Adjusted: 118 590 747,00
            //   #2  119 015 994 iterations in 2 000,00 ms. Adjusted: 119 015 994,00	 <---- Best
            //   #3  117 660 495 iterations in 2 000,00 ms. Adjusted: 117 660 495,00	 <---- Worst
            //   Worst-Best difference: 1 355 499,00 (1,15%)
            // 4. TransformLighten32_3_IntrinsicsSse2: 323 727 497 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 107 909 163,87 (-16 588 526,80 / 86,68%)
            //   #1  107 915 474 iterations in 2 000,00 ms. Adjusted: 107 915 468,60
            //   #2  108 262 080 iterations in 2 000,00 ms. Adjusted: 108 262 080,00	 <---- Best
            //   #3  107 549 943 iterations in 2 000,00 ms. Adjusted: 107 549 943,00	 <---- Worst
            //   Worst-Best difference: 712 137,00 (0,66%)
            // 5. TransformLighten32_1_Vanilla: 305 464 704 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 101 821 568,00 (-22 676 122,67 / 81,79%)
            //   #1  101 632 144 iterations in 2 000,00 ms. Adjusted: 101 632 144,00	 <---- Worst
            //   #2  102 154 020 iterations in 2 000,00 ms. Adjusted: 102 154 020,00	 <---- Best
            //   #3  101 678 540 iterations in 2 000,00 ms. Adjusted: 101 678 540,00
            //   Worst-Best difference: 521 876,00 (0,51%)
            // 6. TransformLighten32_2_Vector: 279 605 746 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 93 201 913,79 (-31 295 776,87 / 74,86%)
            //   #1  94 262 369 iterations in 2 000,00 ms. Adjusted: 94 262 369,00	 <---- Best
            //   #2  92 376 447 iterations in 2 000,00 ms. Adjusted: 92 376 442,38	 <---- Worst
            //   #3  92 966 930 iterations in 2 000,00 ms. Adjusted: 92 966 930,00
            //   Worst-Best difference: 1 885 926,62 (2,04%)
            // 7. TransformLightenPerChannel32: 278 569 436 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 92 856 475,69 (-31 641 214,97 / 74,58%)
            //   #1  87 077 031 iterations in 2 000,00 ms. Adjusted: 87 077 026,65	 <---- Worst
            //   #2  91 313 513 iterations in 2 000,00 ms. Adjusted: 91 313 508,43
            //   #3  100 178 892 iterations in 2 000,00 ms. Adjusted: 100 178 892,00	 <---- Best
            //   Worst-Best difference: 13 101 865,35 (15,05%)
        }

        [Test]
        public void TransformDarken64Test()
        {
            Color64 color = new Color32(128, 255, 64).ToColor64();
            const float factor = 0.5f;

            Color64 expected = TransformDarkenPerChannel64(color, factor, ColorChannels.Rgb);
            Console.WriteLine($"{"Expected color:",-50} {expected}");

            void DoAssert(Expression<Func<Color64>> e)
            {
                var m = (MethodCallExpression)e.Body;
                Color64 actual = e.Compile().Invoke();
                Console.WriteLine($"{$"{m.Method.Name}:",-50} {actual}");
                Assert.AreEqual(expected, actual);
            }

            DoAssert(() => TransformDarken64_1_Vanilla(color, factor));
            DoAssert(() => TransformDarken64_2_Vector(color, factor));
            DoAssert(() => TransformDarken64_3_IntrinsicsSse41(color, factor));

            new PerformanceTest<Color64>
                {
                    TestName = nameof(TransformDarken64Test),
                    TestTime = 2000,
                    //Iterations = 10_000_000,
                    Repeat = 3
                }
                .AddCase(() => TransformDarkenPerChannel64(color, factor, ColorChannels.Rgb), nameof(TransformDarkenPerChannel64))
                .AddCase(() => TransformDarken64_1_Vanilla(color, factor), nameof(TransformDarken64_1_Vanilla))
                .AddCase(() => TransformDarken64_2_Vector(color, factor), nameof(TransformDarken64_2_Vector))
                .AddCase(() => TransformDarken64_3_IntrinsicsSse41(color, factor), nameof(TransformDarken64_3_IntrinsicsSse41))
                .DoTest()
                .DumpResults(Console.Out);

            // .NET 10:
            // 1. TransformDarken64_3_IntrinsicsSse41: 380 337 045 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 126 779 015,00
            //   #1  127 442 061 iterations in 2 000,00 ms. Adjusted: 127 442 061,00	 <---- Best
            //   #2  126 811 311 iterations in 2 000,00 ms. Adjusted: 126 811 311,00
            //   #3  126 083 673 iterations in 2 000,00 ms. Adjusted: 126 083 673,00	 <---- Worst
            //   Worst-Best difference: 1 358 388,00 (1,08%)
            // 2. TransformDarken64_2_Vector: 326 005 871 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 108 668 623,67 (-18 110 391,33 / 85,71%)
            //   #1  105 520 168 iterations in 2 000,00 ms. Adjusted: 105 520 168,00	 <---- Worst
            //   #2  110 851 997 iterations in 2 000,00 ms. Adjusted: 110 851 997,00	 <---- Best
            //   #3  109 633 706 iterations in 2 000,00 ms. Adjusted: 109 633 706,00
            //   Worst-Best difference: 5 331 829,00 (5,05%)
            // 3. TransformDarken64_1_Vanilla: 325 357 648 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 108 452 549,33 (-18 326 465,67 / 85,54%)
            //   #1  114 533 151 iterations in 2 000,00 ms. Adjusted: 114 533 151,00	 <---- Best
            //   #2  110 642 607 iterations in 2 000,00 ms. Adjusted: 110 642 607,00
            //   #3  100 181 890 iterations in 2 000,00 ms. Adjusted: 100 181 890,00	 <---- Worst
            //   Worst-Best difference: 14 351 261,00 (14,33%)
            // 4. TransformDarken64PerChannel: 308 937 228 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 102 979 076,00 (-23 799 939,00 / 81,23%)
            //   #1  104 435 803 iterations in 2 000,00 ms. Adjusted: 104 435 803,00	 <---- Best
            //   #2  100 922 744 iterations in 2 000,00 ms. Adjusted: 100 922 744,00	 <---- Worst
            //   #3  103 578 681 iterations in 2 000,00 ms. Adjusted: 103 578 681,00
            //   Worst-Best difference: 3 513 059,00 (3,48%)
        }

        [Test]
        public void TransformLighten64Test()
        {
            const int iterations = 10_000_000;
            Color64 color = new Color32(128, 255, 64).ToColor64();
            const float factor = 0.5f;

            Color64 expected = TransformLightenPerChannel64(color, factor, ColorChannels.Rgb);
            Console.WriteLine($"{"Expected color:",-50} {expected}");

            void DoAssert(Expression<Func<Color64>> e)
            {
                var m = (MethodCallExpression)e.Body;
                Color64 actual = e.Compile().Invoke();
                Console.WriteLine($"{$"{m.Method.Name}:",-50} {actual}");
                Assert.AreEqual(expected, actual);
            }

            DoAssert(() => TransformLighten64_1_Vanilla(color, factor));
            DoAssert(() => TransformLighten64_2_Vector(color, factor));
            DoAssert(() => TransformLighten64_3_IntrinsicsSse41(color, factor));
            DoAssert(() => TransformLighten64_4_IntrinsicsFma(color, factor));

            new PerformanceTest<Color64>
                {
                    TestName = nameof(TransformLighten64Test),
                    TestTime = 2000,
                    //Iterations = 10_000_000,
                    Repeat = 3
                }
                .AddCase(() => TransformLightenPerChannel64(color, factor, ColorChannels.Rgb), nameof(TransformLightenPerChannel64))
                .AddCase(() => TransformLighten64_1_Vanilla(color, factor), nameof(TransformLighten64_1_Vanilla))
                .AddCase(() => TransformLighten64_2_Vector(color, factor), nameof(TransformLighten64_2_Vector))
                .AddCase(() => TransformLighten64_3_IntrinsicsSse41(color, factor), nameof(TransformLighten64_3_IntrinsicsSse41))
                .AddCase(() => TransformLighten64_4_IntrinsicsFma(color, factor), nameof(TransformLighten64_4_IntrinsicsFma))
                .DoTest()
                .DumpResults(Console.Out);

            // .NET 10:
            // 1. TransformLighten64_5_IntrinsicsFma: 362 045 095 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 120 681 694,31
            //   #1  120 965 348 iterations in 2 000,00 ms. Adjusted: 120 965 341,95	 <---- Best
            //   #2  120 478 236 iterations in 2 000,00 ms. Adjusted: 120 478 229,98	 <---- Worst
            //   #3  120 601 511 iterations in 2 000,00 ms. Adjusted: 120 601 511,00
            //   Worst-Best difference: 487 111,98 (0,40%)
            // 2. TransformLighten64_3_IntrinsicsSse41: 331 966 146 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 110 655 378,20 (-10 026 316,11 / 91,69%)
            //   #1  104 141 673 iterations in 2 000,00 ms. Adjusted: 104 141 673,00	 <---- Worst
            //   #2  110 614 649 iterations in 2 000,00 ms. Adjusted: 110 614 643,47
            //   #3  117 209 824 iterations in 2 000,00 ms. Adjusted: 117 209 818,14	 <---- Best
            //   Worst-Best difference: 13 068 145,14 (12,55%)
            // 3. TransformLighten64_1_Vanilla: 295 575 913 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 98 525 304,33 (-22 156 389,98 / 81,64%)
            //   #1  89 809 452 iterations in 2 000,00 ms. Adjusted: 89 809 452,00	 <---- Worst
            //   #2  103 160 026 iterations in 2 000,00 ms. Adjusted: 103 160 026,00	 <---- Best
            //   #3  102 606 435 iterations in 2 000,00 ms. Adjusted: 102 606 435,00
            //   Worst-Best difference: 13 350 574,00 (14,87%)
            // 4. TransformLightenPerChannel64: 287 602 361 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 95 867 452,17 (-24 814 242,14 / 79,44%)
            //   #1  89 766 582 iterations in 2 000,00 ms. Adjusted: 89 766 577,51	 <---- Worst
            //   #2  94 752 587 iterations in 2 000,00 ms. Adjusted: 94 752 587,00
            //   #3  103 083 192 iterations in 2 000,00 ms. Adjusted: 103 083 192,00	 <---- Best
            //   Worst-Best difference: 13 316 614,49 (14,83%)
            // 5. TransformLighten64_2_Vector: 263 884 652 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 87 961 549,08 (-32 720 145,23 / 72,89%)
            //   #1  95 258 536 iterations in 2 000,00 ms. Adjusted: 95 258 531,24	 <---- Best
            //   #2  84 727 482 iterations in 2 000,00 ms. Adjusted: 84 727 482,00
            //   #3  83 898 634 iterations in 2 000,00 ms. Adjusted: 83 898 634,00	 <---- Worst
            //   Worst-Best difference: 11 359 897,24 (13,54%)
        }

        [Test]
        public void TransformDarkenFTest()
        {
            ColorF color = new Color32(128, 255, 64).ToColorF();
            const float factor = 0.5f;

            ColorF expected = TransformDarkenPerChannelF(color, factor, ColorChannels.Rgb);
            Console.WriteLine($"{"Expected color:",-50} {expected}");

            void DoAssert(Expression<Func<ColorF>> e)
            {
                var m = (MethodCallExpression)e.Body;
                ColorF actual = e.Compile().Invoke();
                Console.WriteLine($"{$"{m.Method.Name}:",-50} {actual}");
                Assert.AreEqual(expected, actual);
            }

            DoAssert(() => TransformDarkenF_1_Vanilla(color, factor));
            DoAssert(() => TransformDarkenF_2_Vector(color, factor));
            DoAssert(() => TransformDarkenF_3_Intrinsics(color, factor));

            new PerformanceTest<ColorF>
                {
                    TestName = nameof(TransformDarken32Test),
                    TestTime = 2000,
                    //Iterations = 10_000_000,
                    Repeat = 3
                }
                .AddCase(() => TransformDarkenPerChannelF(color, factor, ColorChannels.Rgb), nameof(TransformDarkenPerChannelF))
                .AddCase(() => TransformDarkenF_1_Vanilla(color, factor), nameof(TransformDarkenF_1_Vanilla))
                .AddCase(() => TransformDarkenF_2_Vector(color, factor), nameof(TransformDarkenF_2_Vector))
                .AddCase(() => TransformDarkenF_3_Intrinsics(color, factor), nameof(TransformDarkenF_3_Intrinsics))
                .DoTest()
                .DumpResults(Console.Out);

            // .NET 10:
            // 1. TransformDarkenF_3_Intrinsics: 422 849 053 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 140 949 684,33
            //   #1  142 049 607 iterations in 2 000,00 ms. Adjusted: 142 049 607,00	 <---- Best
            //   #2  139 171 740 iterations in 2 000,00 ms. Adjusted: 139 171 740,00	 <---- Worst
            //   #3  141 627 706 iterations in 2 000,00 ms. Adjusted: 141 627 706,00
            //   Worst-Best difference: 2 877 867,00 (2,07%)
            // 2. TransformDarkenF_2_Vector: 390 416 441 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 130 138 813,67 (-10 810 870,67 / 92,33%)
            //   #1  143 023 683 iterations in 2 000,00 ms. Adjusted: 143 023 683,00	 <---- Best
            //   #2  118 357 872 iterations in 2 000,00 ms. Adjusted: 118 357 872,00	 <---- Worst
            //   #3  129 034 886 iterations in 2 000,00 ms. Adjusted: 129 034 886,00
            //   Worst-Best difference: 24 665 811,00 (20,84%)
            // 3. TransformDarkenF_1_Vanilla: 351 328 638 iterations in 6 000,05 ms. Adjusted for 2 000 ms: 117 108 581,08 (-23 841 103,25 / 83,09%)
            //   #1  120 783 964 iterations in 2 000,00 ms. Adjusted: 120 783 964,00
            //   #2  121 305 406 iterations in 2 000,00 ms. Adjusted: 121 305 406,00	 <---- Best
            //   #3  109 239 268 iterations in 2 000,05 ms. Adjusted: 109 236 373,24	 <---- Worst
            //   Worst-Best difference: 12 069 032,76 (11,05%)
            // 4. TransformDarkenPerChannelF: 332 971 688 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 110 990 557,12 (-29 959 127,21 / 78,74%)
            //   #1  110 873 906 iterations in 2 000,00 ms. Adjusted: 110 873 889,37
            //   #2  120 481 780 iterations in 2 000,00 ms. Adjusted: 120 481 780,00	 <---- Best
            //   #3  101 616 002 iterations in 2 000,00 ms. Adjusted: 101 616 002,00	 <---- Worst
            //   Worst-Best difference: 18 865 778,00 (18,57%)
        }

        [Test]
        public void TransformLightenFTest()
        {
            ColorF color = new Color32(128, 255, 64).ToColorF();
            const float factor = 0.5f;

            ColorF expected = TransformLightenPerChannelF(color, factor, ColorChannels.Rgb);
            Console.WriteLine($"{"Expected color:",-50} {expected}");

            void DoAssert(Expression<Func<ColorF>> e)
            {
                var m = (MethodCallExpression)e.Body;
                ColorF actual = e.Compile().Invoke();
                Console.WriteLine($"{$"{m.Method.Name}:",-50} {actual}");
                Assert.AreEqual(expected, actual);
            }

            DoAssert(() => TransformLightenF_1_Vanilla(color, factor));
            DoAssert(() => TransformLightenF_2_Vector(color, factor));
            DoAssert(() => TransformLightenF_3_IntrinsicsSse(color, factor));
            DoAssert(() => TransformLightenF_4_IntrinsicsFma(color, factor));

            new PerformanceTest<ColorF>
                {
                    TestName = nameof(TransformLighten32Test),
                    TestTime = 2000,
                    //Iterations = 10_000_000,
                    Repeat = 3
                }
                .AddCase(() => TransformLightenPerChannelF(color, factor, ColorChannels.Rgb), nameof(TransformLightenPerChannelF))
                .AddCase(() => TransformLightenF_1_Vanilla(color, factor), nameof(TransformLightenF_1_Vanilla))
                .AddCase(() => TransformLightenF_2_Vector(color, factor), nameof(TransformLightenF_2_Vector))
                .AddCase(() => TransformLightenF_3_IntrinsicsSse(color, factor), nameof(TransformLightenF_3_IntrinsicsSse))
                .AddCase(() => TransformLightenF_4_IntrinsicsFma(color, factor), nameof(TransformLightenF_4_IntrinsicsFma))
                .DoTest()
                .DumpResults(Console.Out);

            // .NET 10:
            // 1. TransformLightenF_4_IntrinsicsFma: 399 016 684 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 133 005 561,33
            //   #1  132 451 088 iterations in 2 000,00 ms. Adjusted: 132 451 088,00	 <---- Worst
            //   #2  133 101 245 iterations in 2 000,00 ms. Adjusted: 133 101 245,00
            //   #3  133 464 351 iterations in 2 000,00 ms. Adjusted: 133 464 351,00	 <---- Best
            //   Worst-Best difference: 1 013 263,00 (0,77%)
            // 2. TransformLightenF_3_IntrinsicsSse: 358 526 792 iterations in 6 000,01 ms. Adjusted for 2 000 ms: 119 508 804,69 (-13 496 756,64 / 89,85%)
            //   #1  106 453 636 iterations in 2 000,00 ms. Adjusted: 106 453 636,00	 <---- Worst
            //   #2  126 097 157 iterations in 2 000,00 ms. Adjusted: 126 097 157,00	 <---- Best
            //   #3  125 975 999 iterations in 2 000,01 ms. Adjusted: 125 975 621,07
            //   Worst-Best difference: 19 643 521,00 (18,45%)
            // 3. TransformLightenF_2_Vector: 322 136 978 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 107 378 992,67 (-25 626 568,67 / 80,73%)
            //   #1  103 220 880 iterations in 2 000,00 ms. Adjusted: 103 220 880,00	 <---- Worst
            //   #2  106 338 752 iterations in 2 000,00 ms. Adjusted: 106 338 752,00
            //   #3  112 577 346 iterations in 2 000,00 ms. Adjusted: 112 577 346,00	 <---- Best
            //   Worst-Best difference: 9 356 466,00 (9,06%)
            // 4. TransformLightenF_1_Vanilla: 321 389 745 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 107 129 915,00 (-25 875 646,33 / 80,55%)
            //   #1  107 644 352 iterations in 2 000,00 ms. Adjusted: 107 644 352,00
            //   #2  108 056 642 iterations in 2 000,00 ms. Adjusted: 108 056 642,00	 <---- Best
            //   #3  105 688 751 iterations in 2 000,00 ms. Adjusted: 105 688 751,00	 <---- Worst
            //   Worst-Best difference: 2 367 891,00 (2,24%)
            // 5. TransformLightenPerChannelF: 272 912 312 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 90 970 770,67 (-42 034 790,67 / 68,40%)
            //   #1  87 336 637 iterations in 2 000,00 ms. Adjusted: 87 336 637,00	 <---- Worst
            //   #2  93 921 713 iterations in 2 000,00 ms. Adjusted: 93 921 713,00	 <---- Best
            //   #3  91 653 962 iterations in 2 000,00 ms. Adjusted: 91 653 962,00
            //   Worst-Best difference: 6 585 076,00 (7,54%)
        }

        [Test]
        public void TransformContrast32Test()
        {
            Color32 color = new Color32(128, 255, 64);
            const float factor = 0.5f;

            Color32 expected = TransformContrastPerChannel32(color, factor, ColorChannels.Rgb);
            Console.WriteLine($"{"Expected color:",-50} {expected}");

            void DoAssert(Expression<Func<Color32>> e)
            {
                var m = (MethodCallExpression)e.Body;
                Color32 actual = e.Compile().Invoke();
                Console.WriteLine($"{$"{m.Method.Name}:",-50} {actual}");
                Assert.AreEqual(expected, actual);
            }

            DoAssert(() => TransformContrast32_1_Vanilla(color, factor));
            DoAssert(() => TransformContrast32_2_Vector(color, factor));
            DoAssert(() => TransformContrast32_3_IntrinsicsFma(color, factor));
            DoAssert(() => TransformContrast32_4_IntrinsicsSse41(color, factor));
            DoAssert(() => TransformContrast32_5_IntrinsicsSse3(color, factor));
            DoAssert(() => TransformContrast32_6_IntrinsicsSse2(color, factor));

            new PerformanceTest<Color32>
                {
                    TestName = nameof(TransformContrast32Test),
                    TestTime = 2000,
                    //Iterations = 10_000_000,
                    Repeat = 3
                }
                .AddCase(() => TransformContrastPerChannel32(color, factor, ColorChannels.Rgb), nameof(TransformContrastPerChannel32))
                .AddCase(() => TransformContrast32_1_Vanilla(color, factor), nameof(TransformContrast32_1_Vanilla))
                .AddCase(() => TransformContrast32_2_Vector(color, factor), nameof(TransformContrast32_2_Vector))
                .AddCase(() => TransformContrast32_3_IntrinsicsFma(color, factor), nameof(TransformContrast32_3_IntrinsicsFma))
                .AddCase(() => TransformContrast32_4_IntrinsicsSse41(color, factor), nameof(TransformContrast32_4_IntrinsicsSse41))
                .AddCase(() => TransformContrast32_5_IntrinsicsSse3(color, factor), nameof(TransformContrast32_5_IntrinsicsSse3))
                .AddCase(() => TransformContrast32_6_IntrinsicsSse2(color, factor), nameof(TransformContrast32_6_IntrinsicsSse2))
                .DoTest()
                .DumpResults(Console.Out);

            // .NET 10:
            // 1. TransformContrast32_3_IntrinsicsFma: 294 125 617 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 98 041 872,33
            //   #1  97 866 065 iterations in 2 000,00 ms. Adjusted: 97 866 065,00
            //   #2  97 865 546 iterations in 2 000,00 ms. Adjusted: 97 865 546,00	 <---- Worst
            //   #3  98 394 006 iterations in 2 000,00 ms. Adjusted: 98 394 006,00	 <---- Best
            //   Worst-Best difference: 528 460,00 (0,54%)
            // 2. TransformContrast32_4_IntrinsicsSse41: 285 940 659 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 95 313 551,41 (-2 728 320,92 / 97,22%)
            //   #1  95 436 815 iterations in 2 000,00 ms. Adjusted: 95 436 815,00	 <---- Best
            //   #2  95 258 082 iterations in 2 000,00 ms. Adjusted: 95 258 082,00
            //   #3  95 245 762 iterations in 2 000,00 ms. Adjusted: 95 245 757,24	 <---- Worst
            //   Worst-Best difference: 191 057,76 (0,20%)
            // 3. TransformContrast32_5_IntrinsicsSse3: 264 229 466 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 88 076 488,67 (-9 965 383,67 / 89,84%)
            //   #1  88 167 762 iterations in 2 000,00 ms. Adjusted: 88 167 762,00	 <---- Best
            //   #2  87 925 768 iterations in 2 000,00 ms. Adjusted: 87 925 768,00	 <---- Worst
            //   #3  88 135 936 iterations in 2 000,00 ms. Adjusted: 88 135 936,00
            //   Worst-Best difference: 241 994,00 (0,28%)
            // 4. TransformContrast32_6_IntrinsicsSse2: 253 048 370 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 84 349 456,67 (-13 692 415,67 / 86,03%)
            //   #1  84 356 868 iterations in 2 000,00 ms. Adjusted: 84 356 868,00
            //   #2  85 028 692 iterations in 2 000,00 ms. Adjusted: 85 028 692,00	 <---- Best
            //   #3  83 662 810 iterations in 2 000,00 ms. Adjusted: 83 662 810,00	 <---- Worst
            //   Worst-Best difference: 1 365 882,00 (1,63%)
            // 5. TransformContrast32_1_Vanilla: 249 561 800 iterations in 6 000,01 ms. Adjusted for 2 000 ms: 83 187 184,31 (-14 854 688,03 / 84,85%)
            //   #1  81 812 435 iterations in 2 000,00 ms. Adjusted: 81 812 435,00	 <---- Worst
            //   #2  83 994 693 iterations in 2 000,00 ms. Adjusted: 83 994 693,00	 <---- Best
            //   #3  83 754 672 iterations in 2 000,01 ms. Adjusted: 83 754 424,92
            //   Worst-Best difference: 2 182 258,00 (2,67%)
            // 6. TransformContrastPerChannel32: 221 736 420 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 73 912 140,00 (-24 129 732,33 / 75,39%)
            //   #1  76 503 418 iterations in 2 000,00 ms. Adjusted: 76 503 418,00	 <---- Best
            //   #2  71 798 691 iterations in 2 000,00 ms. Adjusted: 71 798 691,00	 <---- Worst
            //   #3  73 434 311 iterations in 2 000,00 ms. Adjusted: 73 434 311,00
            //   Worst-Best difference: 4 704 727,00 (6,55%)
            // 7. TransformContrast32_2_Vector: 199 762 577 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 66 587 523,45 (-31 454 348,88 / 67,92%)
            //   #1  64 607 815 iterations in 2 000,00 ms. Adjusted: 64 607 811,77	 <---- Worst
            //   #2  68 162 776 iterations in 2 000,00 ms. Adjusted: 68 162 772,59	 <---- Best
            //   #3  66 991 986 iterations in 2 000,00 ms. Adjusted: 66 991 986,00
            //   Worst-Best difference: 3 554 960,82 (5,50%)
        }

        [Test]
        public void TransformContrast64Test()
        {
            Color64 color = new Color32(128, 255, 64).ToColor64();
            const float factor = 0.5f;

            Color64 expected = TransformContrastPerChannel64(color, factor, ColorChannels.Rgb);
            Console.WriteLine($"{"Expected color:",-50} {expected}");

            void DoAssert(Expression<Func<Color64>> e)
            {
                var m = (MethodCallExpression)e.Body;
                Color64 actual = e.Compile().Invoke();
                Console.WriteLine($"{$"{m.Method.Name}:",-50} {actual}");
                Assert.AreEqual(expected, actual);
            }

            DoAssert(() => TransformContrast64_1_Vanilla(color, factor));
            DoAssert(() => TransformContrast64_2_Vector(color, factor));
            DoAssert(() => TransformContrast64_3_IntrinsicsFma(color, factor));
            DoAssert(() => TransformContrast64_4_IntrinsicsSse41(color, factor));

            new PerformanceTest<Color64>
                {
                    TestName = nameof(TransformContrast64Test),
                    TestTime = 2000,
                    //Iterations = 10_000_000,
                    Repeat = 3
                }
                .AddCase(() => TransformContrastPerChannel64(color, factor, ColorChannels.Rgb), nameof(TransformContrastPerChannel64))
                .AddCase(() => TransformContrast64_1_Vanilla(color, factor), nameof(TransformContrast64_1_Vanilla))
                .AddCase(() => TransformContrast64_2_Vector(color, factor), nameof(TransformContrast64_2_Vector))
                .AddCase(() => TransformContrast64_3_IntrinsicsFma(color, factor), nameof(TransformContrast64_3_IntrinsicsFma))
                .AddCase(() => TransformContrast64_4_IntrinsicsSse41(color, factor), nameof(TransformContrast64_4_IntrinsicsSse41))
                .DoTest()
                .DumpResults(Console.Out);

            // .NET 10:
            // 1. TransformContrast64_3_IntrinsicsFma: 283 164 023 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 94 388 007,67
            //   #1  94 753 948 iterations in 2 000,00 ms. Adjusted: 94 753 948,00	 <---- Best
            //   #2  93 807 116 iterations in 2 000,00 ms. Adjusted: 93 807 116,00	 <---- Worst
            //   #3  94 602 959 iterations in 2 000,00 ms. Adjusted: 94 602 959,00
            //   Worst-Best difference: 946 832,00 (1,01%)
            // 2. TransformContrast64_4_IntrinsicsSse41: 276 491 459 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 92 163 819,67 (-2 224 188,00 / 97,64%)
            //   #1  92 152 308 iterations in 2 000,00 ms. Adjusted: 92 152 308,00
            //   #2  92 200 745 iterations in 2 000,00 ms. Adjusted: 92 200 745,00	 <---- Best
            //   #3  92 138 406 iterations in 2 000,00 ms. Adjusted: 92 138 406,00	 <---- Worst
            //   Worst-Best difference: 62 339,00 (0,07%)
            // 3. TransformContrast64_1_Vanilla: 249 477 881 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 83 159 293,67 (-11 228 714,00 / 88,10%)
            //   #1  84 664 812 iterations in 2 000,00 ms. Adjusted: 84 664 812,00	 <---- Best
            //   #2  84 661 780 iterations in 2 000,00 ms. Adjusted: 84 661 780,00
            //   #3  80 151 289 iterations in 2 000,00 ms. Adjusted: 80 151 289,00	 <---- Worst
            //   Worst-Best difference: 4 513 523,00 (5,63%)
            // 4. TransformContrastPerChannel64: 220 681 351 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 73 560 449,14 (-20 827 558,53 / 77,93%)
            //   #1  75 486 816 iterations in 2 000,00 ms. Adjusted: 75 486 816,00	 <---- Best
            //   #2  73 577 211 iterations in 2 000,00 ms. Adjusted: 73 577 211,00
            //   #3  71 617 324 iterations in 2 000,00 ms. Adjusted: 71 617 320,42	 <---- Worst
            //   Worst-Best difference: 3 869 495,58 (5,40%)
            // 5. TransformContrast64_2_Vector: 207 369 256 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 69 123 085,33 (-25 264 922,33 / 73,23%)
            //   #1  74 540 239 iterations in 2 000,00 ms. Adjusted: 74 540 239,00	 <---- Best
            //   #2  66 193 685 iterations in 2 000,00 ms. Adjusted: 66 193 685,00	 <---- Worst
            //   #3  66 635 332 iterations in 2 000,00 ms. Adjusted: 66 635 332,00
            //   Worst-Best difference: 8 346 554,00 (12,61%)
        }

        [Test]
        public void TransformContrastFTest()
        {
            ColorF color = new Color32(128, 255, 64).ToColorF();
            const float factor = 0.5f;

            ColorF expected = TransformContrastPerChannelF(color, factor, ColorChannels.Rgb);
            Console.WriteLine($"{"Expected color:",-50} {expected}");

            void DoAssert(Expression<Func<ColorF>> e)
            {
                var m = (MethodCallExpression)e.Body;
                ColorF actual = e.Compile().Invoke();
                Console.WriteLine($"{$"{m.Method.Name}:",-50} {actual}");
                Assert.AreEqual(expected, actual);
            }

            DoAssert(() => TransformContrastF_1_Vanilla(color, factor));
            DoAssert(() => TransformContrastF_2_Vector(color, factor));
            DoAssert(() => TransformContrastF_3_IntrinsicsFma(color, factor));
            DoAssert(() => TransformContrastF_4_IntrinsicsSse(color, factor));

            new PerformanceTest<ColorF>
                {
                    TestName = nameof(TransformContrastFTest),
                    TestTime = 2000,
                    //Iterations = 10_000_000,
                    Repeat = 3
                }
                .AddCase(() => TransformContrastPerChannelF(color, factor, ColorChannels.Rgb), nameof(TransformContrastPerChannelF))
                .AddCase(() => TransformContrastF_1_Vanilla(color, factor), nameof(TransformContrastF_1_Vanilla))
                .AddCase(() => TransformContrastF_2_Vector(color, factor), nameof(TransformContrastF_2_Vector))
                .AddCase(() => TransformContrastF_3_IntrinsicsFma(color, factor), nameof(TransformContrastF_3_IntrinsicsFma))
                .AddCase(() => TransformContrastF_4_IntrinsicsSse(color, factor), nameof(TransformContrastF_4_IntrinsicsSse))
                .DoTest()
                .DumpResults(Console.Out);

            // .NET 10:
            // 1. TransformContrastF_3_IntrinsicsFma: 399 939 703 iterations in 6 000,07 ms. Adjusted for 2 000 ms: 133 311 745,77
            //   #1  133 105 471 iterations in 2 000,00 ms. Adjusted: 133 105 471,00	 <---- Worst
            //   #2  133 525 609 iterations in 2 000,00 ms. Adjusted: 133 525 609,00	 <---- Best
            //   #3  133 308 623 iterations in 2 000,07 ms. Adjusted: 133 304 157,31
            //   Worst-Best difference: 420 138,00 (0,32%)
            // 2. TransformContrastF_4_IntrinsicsSse: 379 796 286 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 126 598 762,00 (-6 712 983,77 / 94,96%)
            //   #1  126 644 779 iterations in 2 000,00 ms. Adjusted: 126 644 779,00
            //   #2  126 488 470 iterations in 2 000,00 ms. Adjusted: 126 488 470,00	 <---- Worst
            //   #3  126 663 037 iterations in 2 000,00 ms. Adjusted: 126 663 037,00	 <---- Best
            //   Worst-Best difference: 174 567,00 (0,14%)
            // 3. TransformContrastF_2_Vector: 376 818 323 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 125 606 105,58 (-7 705 640,19 / 94,22%)
            //   #1  130 090 593 iterations in 2 000,00 ms. Adjusted: 130 090 593,00	 <---- Best
            //   #2  125 292 562 iterations in 2 000,00 ms. Adjusted: 125 292 555,74
            //   #3  121 435 168 iterations in 2 000,00 ms. Adjusted: 121 435 168,00	 <---- Worst
            //   Worst-Best difference: 8 655 425,00 (7,13%)
            // 4. TransformContrastF_1_Vanilla: 310 243 600 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 103 414 531,54 (-29 897 214,23 / 77,57%)
            //   #1  107 357 503 iterations in 2 000,00 ms. Adjusted: 107 357 497,63	 <---- Best
            //   #2  106 208 877 iterations in 2 000,00 ms. Adjusted: 106 208 877,00
            //   #3  96 677 220 iterations in 2 000,00 ms. Adjusted: 96 677 220,00	 <---- Worst
            //   Worst-Best difference: 10 680 277,63 (11,05%)
            // 5. TransformContrastPerChannelF: 290 030 725 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 96 676 908,33 (-36 634 837,44 / 72,52%)
            //   #1  95 533 595 iterations in 2 000,00 ms. Adjusted: 95 533 595,00	 <---- Worst
            //   #2  98 674 324 iterations in 2 000,00 ms. Adjusted: 98 674 324,00	 <---- Best
            //   #3  95 822 806 iterations in 2 000,00 ms. Adjusted: 95 822 806,00
            //   Worst-Best difference: 3 140 729,00 (3,29%)
        }

        #endregion

        #endregion
    }
}
