#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: TransformColorsTest.cs
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

#if NET45_OR_GREATER || NETCOREAPP
using System.Numerics;
#endif
#if NETCOREAPP3_0_OR_GREATER
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
#endif

using KGySoft.CoreLibraries;
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
            Vector3 result = (new Vector3(255f) - rgbF) * brightness + rgbF;

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

            rgbF = (new Vector3(65535f) - rgbF) * brightness + rgbF;
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
            rgbF = (((rgbF.Div(Byte.MaxValue) - new Vector3(0.5f)) * contrast + new Vector3(0.5f)) * Byte.MaxValue).Clip(Vector3.Zero, new Vector3(255f));

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
            rgbF = (((rgbF.Div(UInt16.MaxValue) - new Vector3(0.5f)) * contrast + new Vector3(0.5f)) * UInt16.MaxValue).Clip(Vector3.Zero, new Vector3(65535f));

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
            rgbF = (rgbF - new Vector3(0.5f)) * contrast + new Vector3(0.5f);
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

        private static Color32 TransformInvert32_1_Vanilla(Color32 c) => new Color32(c.A, (byte)(Byte.MaxValue - c.R), (byte)(Byte.MaxValue - c.G), (byte)(Byte.MaxValue - c.B));
        
        private static Color32 TransformInvert32_2_Vector128(Color32 c)
        {
#if NET7_0_OR_GREATER
            Vector128<byte> bgra8 = Vector128.CreateScalar(c.Value).AsByte();
            return new Color32((VectorExtensions.Max8BitU8 - bgra8).WithElement(3, c.A).AsUInt32().ToScalar());
#else
            return new Color32(c.A, (byte)(Byte.MaxValue - c.R), (byte)(Byte.MaxValue - c.G), (byte)(Byte.MaxValue - c.B));
#endif
        }

        private static Color32 TransformInvert32_3_Vector64(Color32 c)
        {
#if NET7_0_OR_GREATER
            Vector64<byte> bgra8 = Vector64.CreateScalar(c.Value).AsByte();
            return new Color32((Vector64.Create(Byte.MaxValue) - bgra8).WithElement(3, c.A).AsUInt32().ToScalar());
#else
            return new Color32(c.A, (byte)(Byte.MaxValue - c.R), (byte)(Byte.MaxValue - c.G), (byte)(Byte.MaxValue - c.B));
#endif
        }

        private static Color32 TransformInvert32_4_Intrinsics(Color32 c)
        {
#if NETCOREAPP3_0_OR_GREATER
            if (Sse2.IsSupported)
            {
                Vector128<byte> bgra8 = Vector128.CreateScalar(c.Value).AsByte();
                return new Color32(Sse2.Subtract(VectorExtensions.Max8BitU8, bgra8).WithElement(3, c.A).AsUInt32().ToScalar());
            }
#endif

            return new Color32(c.A, (byte)(Byte.MaxValue - c.R), (byte)(Byte.MaxValue - c.G), (byte)(Byte.MaxValue - c.B));
        }

        private static Color64 TransformInvert64_1_Vanilla(Color64 c)
            => new Color64(c.A, (ushort)(UInt16.MaxValue - c.R), (ushort)(UInt16.MaxValue - c.G), (ushort)(UInt16.MaxValue - c.B));

        private static Color64 TransformInvert64_2_Vector128(Color64 c)
        {
#if NET7_0_OR_GREATER
            Vector128<ushort> bgra16 = Vector128.CreateScalar(c.Value).AsUInt16();
            return new Color64((VectorExtensions.Max16BitU16 - bgra16).WithElement(3, c.A).AsUInt64().ToScalar());
#else
            return new Color64(c.A, (ushort)(UInt16.MaxValue - c.R), (ushort)(UInt16.MaxValue - c.G), (ushort)(UInt16.MaxValue - c.B));
#endif
        }

        private static Color64 TransformInvert64_3_Vector64(Color64 c)
        {
#if NET7_0_OR_GREATER
            Vector64<ushort> bgra16 = Vector64.CreateScalar(c.Value).AsUInt16();
            return new Color64((Vector64.Create(UInt16.MaxValue) - bgra16).WithElement(3, c.A).AsUInt64().ToScalar());
#else
            return new Color64(c.A, (ushort)(UInt16.MaxValue - c.R), (ushort)(UInt16.MaxValue - c.G), (ushort)(UInt16.MaxValue - c.B));
#endif
        }

        private static Color64 TransformInvert64_4_Intrinsics(Color64 c)
        {
#if NETCOREAPP3_0_OR_GREATER
            if (Sse2.IsSupported)
            {
                Vector128<ushort> bgra16 = Vector128.CreateScalar(c.Value).AsUInt16();
                return new Color64(Sse2.Subtract(VectorExtensions.Max16BitU16, bgra16).WithElement(3, c.A).AsUInt64().ToScalar());
            }
#endif

            return new Color64(c.A, (ushort)(UInt16.MaxValue - c.R), (ushort)(UInt16.MaxValue - c.G), (ushort)(UInt16.MaxValue - c.B));
        }

        private static ColorF TransformInvertF_1_Vanilla(ColorF c)
        {
            c = c.Clip();
            return new ColorF(c.A, 1f - c.R, 1f - c.G, 1f - c.B);
        }

        private static ColorF TransformInvertF_2_Vector(ColorF c)
        {
#if NET45_OR_GREATER || NETCOREAPP
            return new ColorF(new Vector4(Vector3.One - c.Rgb.ClipF(), c.A));
#else
            c = c.Clip();
            return new ColorF(c.A, 1f - c.R, 1f - c.G, 1f - c.B);
#endif
        }

        private static ColorF TransformInvertF_3_Intrinsics(ColorF c)
        {
#if NETCOREAPP3_0_OR_GREATER
            if (Sse.IsSupported)
                return new ColorF(Sse.Subtract(VectorExtensions.OneF, c.RgbaV128.ClipF()).WithElement(3, c.A));
#endif
            c = c.Clip();
            return new ColorF(c.A, 1f - c.R, 1f - c.G, 1f - c.B);
        }

        private static ColorF TransformGammaPerChannelF(ColorF c, ColorChannels channels, float gamma)
        {
            c = c.Clip();
            float invGamma = 1f / gamma;
            return new ColorF(c.A,
                (channels & ColorChannels.R) == ColorChannels.R ? MathF.Pow(c.R, invGamma) : c.R,
                (channels & ColorChannels.G) == ColorChannels.G ? MathF.Pow(c.G, invGamma) : c.G,
                (channels & ColorChannels.B) == ColorChannels.B ? MathF.Pow(c.B, invGamma) : c.B);
        }

        private static byte[] GenerateGammaLookupTable32_1_Vanilla(float gamma, byte[] table)
        {
            byte[] result = table;
            if (gamma.TolerantIsZero())
            {
                result[255] = 255;
                return result;
            }

            float power = 1f / gamma;
            for (int i = 0; i < 256; i++)
                result[i] = (byte)(255f * MathF.Pow(i / 255f, power) + 0.5f);
            return result;
        }

        private static byte[] GenerateGammaLookupTable32_2_AutoVector128SimpleLoop(float gamma, byte[] table)
        {
            if (gamma.TolerantIsZero())
            {
                table[255] = 255;
                return table;
            }

#if NET7_0_OR_GREATER
            if (Vector128.IsHardwareAccelerated)
            {
                float power = 1f / gamma;
                var current = Vector128.Create(0f, 1f, 2f, 3f);
                for (int i = 0; i < 256; i += 4, current += Vector128.Create(4f))
                {
#if NET9_0_OR_GREATER
                    Vector128<float> resultF = Vector128.FusedMultiplyAdd((current / 255f).Pow(power), VectorExtensions.Max8BitF, VectorExtensions.HalfF);
#else
                    Vector128<float> resultF = (current * (1f / 255f)).Pow(power) * VectorExtensions.Max8BitF + VectorExtensions.HalfF;
#endif
                    Vector128<int> resultI32 = Vector128.Shuffle(Vector128.ConvertToInt32(resultF).AsByte(), VectorExtensions.PackLowBytesMask).AsInt32();
                    table[i].As<byte, int>() = resultI32.ToScalar();
                }

                return table;
            }
#endif
            return GenerateGammaLookupTable32_1_Vanilla(gamma, table);
        }

        private static byte[] GenerateGammaLookupTable32_3_AutoVector128UnrolledNarrowingLoop(float gamma, byte[] table)
        {
            if (gamma.TolerantIsZero())
            {
                table[255] = 255;
                return table;
            }

#if NET9_0_OR_GREATER
            if (Vector128.IsHardwareAccelerated)
            {
                float power = 1f / gamma;
                var current = Vector128.Create(0f, 1f, 2f, 3f);
                for (int i = 0; i < 256; i += Vector128<byte>.Count, current += Vector128.Create(4f))
                {
                    Vector128<float> resultF = Vector128.FusedMultiplyAdd((current / 255f).Pow(power), VectorExtensions.Max8BitF, VectorExtensions.HalfF);
                    Vector128<int> resultI32Left = Vector128.ConvertToInt32(resultF);
                    current += Vector128.Create(4f);
                    resultF = Vector128.FusedMultiplyAdd((current / 255f).Pow(power), VectorExtensions.Max8BitF, VectorExtensions.HalfF);
                    Vector128<int> resultI32Right = Vector128.ConvertToInt32(resultF);
                    Vector128<short> resultI16Left = Vector128.Narrow(resultI32Left, resultI32Right);

                    current += Vector128.Create(4f);
                    resultF = Vector128.FusedMultiplyAdd((current / 255f).Pow(power), VectorExtensions.Max8BitF, VectorExtensions.HalfF);
                    resultI32Left = Vector128.ConvertToInt32(resultF);
                    current += Vector128.Create(4f);
                    resultF = Vector128.FusedMultiplyAdd((current / 255f).Pow(power), VectorExtensions.Max8BitF, VectorExtensions.HalfF);
                    resultI32Right = Vector128.ConvertToInt32(resultF);
                    Vector128<short> resultI16Right = Vector128.Narrow(resultI32Left, resultI32Right);

                    table[i].As<byte, Vector128<byte>>() = Vector128.Narrow(resultI16Left.AsUInt16(), resultI16Right.AsUInt16());
                }

                return table;
            }
#endif
            return GenerateGammaLookupTable32_1_Vanilla(gamma, table);
        }

        private static byte[] GenerateGammaLookupTable32_4_IntrinsicsVector128UnrolledNarrowingLoop(float gamma, byte[] table)
        {
            if (gamma.TolerantIsZero())
            {
                table[255] = 255;
                return table;
            }

#if NETCOREAPP3_0_OR_GREATER
            if (Sse2.IsSupported)
            {
                float power = 1f / gamma;
                var current = Vector128.Create(0f, 1f, 2f, 3f);
                for (int i = 0; i < 256; i += 16, current = Sse.Add(current, Vector128.Create(4f)))
                {
                    // We could spare adding +0.5f by using the slower ConvertToVector128Int32 that rounds the result instead of truncating it,
                    // but if FMA is supported, this is faster. Otherwise, the performance is about the same.
                    Vector128<float> resultF = Sse.Multiply(current, Vector128.Create(1f / 255f)).Pow(power);
                    resultF = Fma.IsSupported
                        ? Fma.MultiplyAdd(resultF, VectorExtensions.Max8BitF, VectorExtensions.HalfF)
                        : Sse.Add(Sse.Multiply(resultF, VectorExtensions.Max8BitF), VectorExtensions.HalfF);
                    Vector128<int> resultI32Left = Sse2.ConvertToVector128Int32WithTruncation(resultF);

                    current = Sse.Add(current, Vector128.Create(4f));
                    resultF = Sse.Multiply(current, Vector128.Create(1f / 255f)).Pow(power);
                    resultF = Fma.IsSupported
                        ? Fma.MultiplyAdd(resultF, VectorExtensions.Max8BitF, VectorExtensions.HalfF)
                        : Sse.Add(Sse.Multiply(resultF, VectorExtensions.Max8BitF), VectorExtensions.HalfF);
                    Vector128<int> resultI32Right = Sse2.ConvertToVector128Int32WithTruncation(resultF);
                    Vector128<short> resultI16Left = Sse2.PackSignedSaturate(resultI32Left, resultI32Right);

                    current = Sse.Add(current, Vector128.Create(4f));
                    resultF = Sse.Multiply(current, Vector128.Create(1f / 255f)).Pow(power);
                    resultF = Fma.IsSupported
                        ? Fma.MultiplyAdd(resultF, VectorExtensions.Max8BitF, VectorExtensions.HalfF)
                        : Sse.Add(Sse.Multiply(resultF, VectorExtensions.Max8BitF), VectorExtensions.HalfF);
                    resultI32Left = Sse2.ConvertToVector128Int32WithTruncation(resultF);

                    current = Sse.Add(current, Vector128.Create(4f));
                    resultF = Sse.Multiply(current, Vector128.Create(1f / 255f)).Pow(power);
                    resultF = Fma.IsSupported
                        ? Fma.MultiplyAdd(resultF, VectorExtensions.Max8BitF, VectorExtensions.HalfF)
                        : Sse.Add(Sse.Multiply(resultF, VectorExtensions.Max8BitF), VectorExtensions.HalfF);
                    resultI32Right = Sse2.ConvertToVector128Int32WithTruncation(resultF);
                    Vector128<short> resultI16Right = Sse2.PackSignedSaturate(resultI32Left, resultI32Right);

                    table[i].As<byte, Vector128<byte>>() = Sse2.PackUnsignedSaturate(resultI16Left, resultI16Right);
                }

                return table;
            }
#endif
            return GenerateGammaLookupTable32_1_Vanilla(gamma, table);
        }

        private static byte[] GenerateGammaLookupTable32_5_AutoVector256SimpleLoop(float gamma, byte[] table)
        {
            if (gamma.TolerantIsZero())
            {
                table[255] = 255;
                return table;
            }

#if NET7_0_OR_GREATER
            if (Vector256.IsHardwareAccelerated)
            {
                float power = 1f / gamma;
                var current = Vector256.Create(0f, 1f, 2f, 3f, 4f, 5f, 6f, 7f);
                for (int i = 0; i < 256; i += 8, current += Vector256.Create(8f))
                {
#if NET9_0_OR_GREATER
                    Vector256<float> resultF = Vector256.FusedMultiplyAdd((current / 255f).Pow(power), VectorExtensions.Max8Bit256F, VectorExtensions.Half256F);
#else
                    Vector256<float> resultF = (current * (1f / 255f)).Pow(power) * VectorExtensions.Max8Bit256F + VectorExtensions.Half256F;
#endif
                    Vector256<long> resultI64 = Vector256.Shuffle(Vector256.ConvertToInt32(resultF).AsByte(), VectorExtensions.PackLowBytes256Mask).AsInt64();
                    table[i].As<byte, long>() = resultI64.ToScalar();
                }

                return table;
            }
#endif
            return GenerateGammaLookupTable32_1_Vanilla(gamma, table);
        }

        private static byte[] GenerateGammaLookupTable32_6_IntrinsicsVector256UnrolledNarrowingLoop(float gamma, byte[] table)
        {
            if (gamma.TolerantIsZero())
            {
                table[255] = 255;
                return table;
            }

#if NETCOREAPP3_0_OR_GREATER
            if (Avx2.IsSupported)
            {
                float power = 1f / gamma;
                var current = Vector256.Create(0f, 1f, 2f, 3f, 4f, 5f, 6f, 7f);
                for (int i = 0; i < 256; i += 32, current = Avx.Add(current, Vector256.Create(8f)))
                {
                    // We could spare adding +0.5f by using the slower ConvertToVector256Int32 that rounds the result instead of truncating it,
                    // but if FMA is supported, this is faster. Otherwise, the performance is about the same.
                    Vector256<float> resultF = Avx.Multiply(current, Vector256.Create(1f / 255f)).Pow(power);
                    resultF = Fma.IsSupported
                        ? Fma.MultiplyAdd(resultF, VectorExtensions.Max8Bit256F, VectorExtensions.Half256F)
                        : Avx.Add(Avx.Multiply(resultF, VectorExtensions.Max8Bit256F), VectorExtensions.Half256F);
                    Vector256<int> resultI32Left = Avx.ConvertToVector256Int32WithTruncation(resultF);

                    current = Avx.Add(current, Vector256.Create(8f));
                    resultF = Avx.Multiply(current, Vector256.Create(1f / 255f)).Pow(power);
                    resultF = Fma.IsSupported
                        ? Fma.MultiplyAdd(resultF, VectorExtensions.Max8Bit256F, VectorExtensions.Half256F)
                        : Avx.Add(Avx.Multiply(resultF, VectorExtensions.Max8Bit256F), VectorExtensions.Half256F);
                    Vector256<int> resultI32Right = Avx.ConvertToVector256Int32WithTruncation(resultF);
                    Vector256<short> resultI16Left = Avx2.PackSignedSaturate(resultI32Left, resultI32Right);

                    current = Avx.Add(current, Vector256.Create(8f));
                    resultF = Avx.Multiply(current, Vector256.Create(1f / 255f)).Pow(power);
                    resultF = Fma.IsSupported
                        ? Fma.MultiplyAdd(resultF, VectorExtensions.Max8Bit256F, VectorExtensions.Half256F)
                        : Avx.Add(Avx.Multiply(resultF, VectorExtensions.Max8Bit256F), VectorExtensions.Half256F);
                    resultI32Left = Avx.ConvertToVector256Int32WithTruncation(resultF);

                    current = Avx.Add(current, Vector256.Create(8f));
                    resultF = Avx.Multiply(current, Vector256.Create(1f / 255f)).Pow(power);
                    resultF = Fma.IsSupported
                        ? Fma.MultiplyAdd(resultF, VectorExtensions.Max8Bit256F, VectorExtensions.Half256F)
                        : Avx.Add(Avx.Multiply(resultF, VectorExtensions.Max8Bit256F), VectorExtensions.Half256F);
                    resultI32Right = Avx.ConvertToVector256Int32WithTruncation(resultF);
                    Vector256<short> resultI16Right = Avx2.PackSignedSaturate(resultI32Left, resultI32Right);

                    // NOTE: Unlike in case of SSE, the PackSignedSaturate methods in AVX interleave the results from left and right vectors, so the order in resultBytes will be as follows:
                    // 0, 1, 2, 3, 8, 9, 10, 11, 16, 17, 18, 19, 24, 25, 26, 27, 4, 5, 6, 7, 12, 13, 14, 15, 20, 21, 22, 23, 28, 29, 30, 31
                    // An apparently obvious solution would be to fix it by Avx2.Shuffle(resultBytes, Vector256.Create((byte)0, 1, 2, 3, 16, 17, 18, 19, 4, 5, 6, 7, 20, 21, 22, 23, 8, 9, 10, 11, 24, 25, 26, 27, 12, 13, 14, 15, 28, 29, 30, 31)),
                    // but it just messes up the result even more, as it does not work across 128-bit lanes. The real solution is to use PermuteVar8x32 on ints, which is 3x slower, but works in the whole 256-bit range.
                    Vector256<byte> resultBytes = Avx2.PackUnsignedSaturate(resultI16Left, resultI16Right);
                    resultBytes = Avx2.PermuteVar8x32(resultBytes.AsInt32(), Vector256.Create(0, 4, 1, 5, 2, 6, 3, 7)).AsByte();
                    table[i].As<byte, Vector256<byte>>() = resultBytes;
                }

                return table;
            }
#endif
            return GenerateGammaLookupTable32_1_Vanilla(gamma, table);
        }

#if NET8_0_OR_GREATER
        private static byte[] GenerateGammaLookupTable32_7_AutoVector512SimpleLoop(float gamma, byte[] table)
        {
            if (gamma.TolerantIsZero())
            {
                table[255] = 255;
                return table;
            }

            //if (Vector512.IsHardwareAccelerated)
            {
                float power = 1f / gamma;
                var current = Vector512.Create(0f, 1f, 2f, 3f, 4f, 5f, 6f, 7f, 8f, 9f, 10f, 11f, 12f, 13f, 14f, 15f);
                for (int i = 0; i < 256; i += 16, current += Vector512.Create(16f))
                {
#if NET9_0_OR_GREATER
                    Vector512<float> resultF = Vector512.FusedMultiplyAdd((current / 255f).Pow(power), Vector512.Create(255f), Vector512.Create(0.5f));
#else
                    Vector512<float> resultF = (current / 255f).Pow(power) * Vector512.Create(255f) + Vector512.Create(0.5f);
#endif
                    Vector512<byte> resultBytes = Vector512.Shuffle(Vector512.ConvertToInt32(resultF).AsByte(), VectorExtensions.PackLowBytes512Mask);
                    table[i].As<byte, Vector128<byte>>() = resultBytes.GetLower().GetLower();
                }

                return table;
            }
        }
#endif

        private static ushort[] GenerateGammaLookupTable64_1_Vanilla(float gamma, ushort[] table)
        {
            if (gamma.TolerantIsZero())
            {
                table[65535] = 65535;
                return table;
            }

            ushort[] result = table;
            float power = 1f / gamma;
            for (int i = 0; i < 65536; i++)
                result[i] = (ushort)(65535f * MathF.Pow(i / 65535f, power) + 0.5f);
            return result;
        }

        private static ushort[] GenerateGammaLookupTable64_2_AutoVector128SimpleLoop(float gamma, ushort[] table)
        {
            if (gamma.TolerantIsZero())
            {
                table[65535] = 65535;
                return table;
            }

#if NET7_0_OR_GREATER
            if (Vector128.IsHardwareAccelerated)
            {
                float power = 1f / gamma;
                var current = Vector128.Create(0f, 1f, 2f, 3f);
                for (int i = 0; i < 65536; i += 4, current += Vector128.Create(4f))
                {
#if NET9_0_OR_GREATER
                    Vector128<float> resultF = Vector128.FusedMultiplyAdd((current / 65535f).Pow(power), VectorExtensions.Max16BitF, VectorExtensions.HalfF);
#else
                    Vector128<float> resultF = (current * (1f / 65535f)).Pow(power) * VectorExtensions.Max16BitF + VectorExtensions.HalfF;
#endif
                    Vector128<ulong> resultU64 = Vector128.Shuffle(Vector128.ConvertToInt32(resultF).AsByte(), VectorExtensions.PackLowWordsMask).AsUInt64();
                    table[i].As<ushort, ulong>() = resultU64.ToScalar();
                }

                return table;
            }
#endif
            return GenerateGammaLookupTable64_1_Vanilla(gamma, table);
        }

        private static ushort[] GenerateGammaLookupTable64_3_AutoVector128UnrolledNarrowingLoop(float gamma, ushort[] table)
        {
            if (gamma.TolerantIsZero())
            {
                table[65535] = 65535;
                return table;
            }

#if NET9_0_OR_GREATER
            if (Vector128.IsHardwareAccelerated)
            {
                float power = 1f / gamma;
                var current = Vector128.Create(0f, 1f, 2f, 3f);
                for (int i = 0; i < 65536; i += Vector128<ushort>.Count, current += Vector128.Create(4f))
                {
                    Vector128<float> resultF = Vector128.FusedMultiplyAdd((current / 65535f).Pow(power), VectorExtensions.Max16BitF, VectorExtensions.HalfF);
                    Vector128<int> resultI32Left = Vector128.ConvertToInt32(resultF);
                    current += Vector128.Create(4f);
                    resultF = Vector128.FusedMultiplyAdd((current / 65535f).Pow(power), VectorExtensions.Max16BitF, VectorExtensions.HalfF);
                    Vector128<int> resultI32Right = Vector128.ConvertToInt32(resultF);

                    table[i].As<ushort, Vector128<ushort>>() = Vector128.Narrow(resultI32Left.AsUInt32(), resultI32Right.AsUInt32());
                }

                return table;
            }
#endif
            return GenerateGammaLookupTable64_1_Vanilla(gamma, table);
        }

        private static ushort[] GenerateGammaLookupTable64_4_IntrinsicsVector128UnrolledNarrowingLoop(float gamma, ushort[] table)
        {
            if (gamma.TolerantIsZero())
            {
                table[65535] = 65535;
                return table;
            }

#if NETCOREAPP3_0_OR_GREATER
            if (Sse41.IsSupported)
            {
                float power = 1f / gamma;
                var current = Vector128.Create(0f, 1f, 2f, 3f);
                for (int i = 0; i < 65536; i += 8, current = Sse.Add(current, Vector128.Create(4f)))
                {
                    // We could spare adding +0.5f by using the slower ConvertToVector128Int32 that rounds the result instead of truncating it,
                    // but if FMA is supported, this is faster. Otherwise, the performance is about the same.
                    Vector128<float> resultF = Sse.Multiply(current, Vector128.Create(1f / 65535f)).Pow(power);
                    resultF = Fma.IsSupported
                        ? Fma.MultiplyAdd(resultF, VectorExtensions.Max16BitF, VectorExtensions.HalfF)
                        : Sse.Add(Sse.Multiply(resultF, VectorExtensions.Max16BitF), VectorExtensions.HalfF);
                    Vector128<int> resultI32Left = Sse2.ConvertToVector128Int32WithTruncation(resultF);

                    current = Sse.Add(current, Vector128.Create(4f));
                    resultF = Sse.Multiply(current, Vector128.Create(1f / 65535f)).Pow(power);
                    resultF = Fma.IsSupported
                        ? Fma.MultiplyAdd(resultF, VectorExtensions.Max16BitF, VectorExtensions.HalfF)
                        : Sse.Add(Sse.Multiply(resultF, VectorExtensions.Max16BitF), VectorExtensions.HalfF);
                    Vector128<int> resultI32Right = Sse2.ConvertToVector128Int32WithTruncation(resultF);

                    table[i].As<ushort, Vector128<ushort>>() = Sse41.PackUnsignedSaturate(resultI32Left, resultI32Right);
                }

                return table;
            }
#endif
            return GenerateGammaLookupTable64_1_Vanilla(gamma, table);
        }

        private static ushort[] GenerateGammaLookupTable64_5_AutoVector256SimpleLoop(float gamma, ushort[] table)
        {
            if (gamma.TolerantIsZero())
            {
                table[65535] = 65535;
                return table;
            }

#if NET7_0_OR_GREATER
            if (Vector256.IsHardwareAccelerated)
            {
                float power = 1f / gamma;
                var current = Vector256.Create(0f, 1f, 2f, 3f, 4f, 5f, 6f, 7f);
                for (int i = 0; i < 65536; i += 8, current += Vector256.Create(8f))
                {
#if NET9_0_OR_GREATER
                    Vector256<float> resultF = Vector256.FusedMultiplyAdd((current / 65535f).Pow(power), VectorExtensions.Max16Bit256F, VectorExtensions.Half256F);
#else
                    Vector256<float> resultF = (current * (1f / 65535f)).Pow(power) * VectorExtensions.Max16Bit256F + VectorExtensions.Half256F;
#endif
                    Vector256<ushort> resultU16 = Vector256.Shuffle(Vector256.ConvertToInt32(resultF).AsUInt16(), VectorExtensions.PackLowWords256Mask);
                    table[i].As<ushort, Vector128<ushort>>() = resultU16.GetLower();
                }

                return table;
            }
#endif
            return GenerateGammaLookupTable64_1_Vanilla(gamma, table);
        }

        private static ushort[] GenerateGammaLookupTable64_6_IntrinsicsVector256UnrolledNarrowingLoop(float gamma, ushort[] table)
        {
            if (gamma.TolerantIsZero())
            {
                table[65535] = 65535;
                return table;
            }

#if NETCOREAPP3_0_OR_GREATER
            if (Avx2.IsSupported)
            {
                float power = 1f / gamma;
                var current = Vector256.Create(0f, 1f, 2f, 3f, 4f, 5f, 6f, 7f);
                for (int i = 0; i < 65536; i += 16, current = Avx.Add(current, Vector256.Create(8f)))
                {
                    // We could spare adding +0.5f by using the slower ConvertToVector256Int32 that rounds the result instead of truncating it,
                    // but if FMA is supported, this is faster. Otherwise, the performance is about the same.
                    Vector256<float> resultF = Avx.Multiply(current, Vector256.Create(1f / 65535f)).Pow(power);
                    resultF = Fma.IsSupported
                        ? Fma.MultiplyAdd(resultF, VectorExtensions.Max16Bit256F, VectorExtensions.Half256F)
                        : Avx.Add(Avx.Multiply(resultF, VectorExtensions.Max16Bit256F), VectorExtensions.Half256F);
                    Vector256<int> resultI32Left = Avx.ConvertToVector256Int32WithTruncation(resultF);

                    current = Avx.Add(current, Vector256.Create(8f));
                    resultF = Avx.Multiply(current, Vector256.Create(1f / 65535f)).Pow(power);
                    resultF = Fma.IsSupported
                        ? Fma.MultiplyAdd(resultF, VectorExtensions.Max16Bit256F, VectorExtensions.Half256F)
                        : Avx.Add(Avx.Multiply(resultF, VectorExtensions.Max16Bit256F), VectorExtensions.Half256F);
                    Vector256<int> resultI32Right = Avx.ConvertToVector256Int32WithTruncation(resultF);

                    // NOTE: Unlike in case of SSE, the PackSignedSaturate methods in AVX interleave the results from left and right vectors, so the order in resultWords will be as follows:
                    // 0, 1, 2, 3, 8, 9, 10, 11, 4, 5, 6, 7, 12, 13, 14, 15
                    // To fix this, we have to use PermuteVar8x32.
                    Vector256<ushort> resultWords = Avx2.PackUnsignedSaturate(resultI32Left, resultI32Right);
                    resultWords = Avx2.PermuteVar8x32(resultWords.AsInt32(), Vector256.Create(0, 1, 4, 5, 2, 3, 6, 7)).AsUInt16();
                    table[i].As<ushort, Vector256<ushort>>() = resultWords;
                }

                return table;
            }
#endif
            return GenerateGammaLookupTable64_1_Vanilla(gamma, table);
        }

#if NET8_0_OR_GREATER
        private static ushort[] GenerateGammaLookupTable64_7_AutoVector512SimpleLoop(float gamma, ushort[] table)
        {
            if (gamma.TolerantIsZero())
            {
                table[65535] = 65535;
                return table;
            }

            //if (Vector512.IsHardwareAccelerated)
            {
                float power = 1f / gamma;
                var current = Vector512.Create(0f, 1f, 2f, 3f, 4f, 5f, 6f, 7f, 8f, 9f, 10f, 11f, 12f, 13f, 14f, 15f);
                for (int i = 0; i < 65536; i += 16, current += Vector512.Create(16f))
                {
#if NET9_0_OR_GREATER
                    Vector512<float> resultF = Vector512.FusedMultiplyAdd((current / 65535f).Pow(power), Vector512.Create(65535f), Vector512.Create(0.5f));
#else
                    Vector512<float> resultF = (current / 65535f).Pow(power) * Vector512.Create(65535f) + Vector512.Create(0.5f);
#endif
                    Vector512<ushort> resultWords = Vector512.Shuffle(Vector512.ConvertToInt32(resultF).AsUInt16(), VectorExtensions.PackLowWords512Mask);
                    table[i].As<ushort, Vector256<ushort>>() = resultWords.GetLower();
                }

                return table;
            }
        }
#endif

        private static ColorF TransformGammaF_1_Vanilla(ColorF c, float gamma)
        {
            c = c.Clip();
            float invGamma = 1f / gamma;
            return new ColorF(c.A,
                MathF.Pow(c.R, invGamma),
                MathF.Pow(c.G, invGamma),
                MathF.Pow(c.B, invGamma));
        }

        private static ColorF TransformGammaF_2_Vector(ColorF c, float gamma)
        {
#if NET9_0_OR_GREATER
            return new ColorF(c.RgbaV128.ClipF().Pow(1f / gamma).WithElement(3, c.A));
#else
            c = c.Clip();
            float invGamma = 1f / gamma;
            return new ColorF(c.A,
                MathF.Pow(c.R, invGamma),
                MathF.Pow(c.G, invGamma),
                MathF.Pow(c.B, invGamma));
#endif
        }

        #endregion

        #region Instance Methods

        [Test]
        public void TransformDarken32Test()
        {
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

        [Test]
        public void TransformInvert32Test()
        {
            Color32 color = new Color32(128, 255, 64);

            Color32 expected = new Color32(color.A, (byte)(Byte.MaxValue - color.R), (byte)(Byte.MaxValue - color.G), (byte)(Byte.MaxValue - color.B));
            Console.WriteLine($"{"Expected color:",-50} {expected}");

            void DoAssert(Expression<Func<Color32>> e)
            {
                var m = (MethodCallExpression)e.Body;
                Color32 actual = e.Compile().Invoke();
                Console.WriteLine($"{$"{m.Method.Name}:",-50} {actual}");
                Assert.AreEqual(expected, actual);
            }

            DoAssert(() => TransformInvert32_1_Vanilla(color));
            DoAssert(() => TransformInvert32_2_Vector128(color));
            DoAssert(() => TransformInvert32_3_Vector64(color));
            DoAssert(() => TransformInvert32_4_Intrinsics(color));

            new PerformanceTest<Color32>
                {
                    TestName = nameof(TransformInvert32Test),
                    TestTime = 2000,
                    //Iterations = 10_000_000,
                    Repeat = 3
                }
                .AddCase(() => TransformInvert32_1_Vanilla(color), nameof(TransformInvert32_1_Vanilla))
                .AddCase(() => TransformInvert32_2_Vector128(color), nameof(TransformInvert32_2_Vector128))
                .AddCase(() => TransformInvert32_3_Vector64(color), nameof(TransformInvert32_3_Vector64))
                .AddCase(() => TransformInvert32_4_Intrinsics(color), nameof(TransformInvert32_4_Intrinsics))
                .DoTest()
                .DumpResults(Console.Out);

            // .NET 10:
            // 1. TransformInvert32_4_Intrinsics: 438 571 117 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 146 190 362,61
            //   #1  148 147 690 iterations in 2 000,00 ms. Adjusted: 148 147 682,59    <---- Best
            //   #2  144 906 911 iterations in 2 000,00 ms. Adjusted: 144 906 896,51    <---- Worst
            //   #3  145 516 516 iterations in 2 000,00 ms. Adjusted: 145 516 508,72
            //   Worst-Best difference: 3 240 786,08 (2,24%)
            // 2. TransformInvert32_2_Vector128: 436 162 770 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 145 387 587,61 (-802 775,00 / 99,45%)
            //   #1  143 218 521 iterations in 2 000,00 ms. Adjusted: 143 218 513,84    <---- Worst
            //   #2  147 604 727 iterations in 2 000,00 ms. Adjusted: 147 604 727,00    <---- Best
            //   #3  145 339 522 iterations in 2 000,00 ms. Adjusted: 145 339 522,00
            //   Worst-Best difference: 4 386 213,16 (3,06%)
            // 3. TransformInvert32_1_Vanilla: 432 352 026 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 144 117 337,30 (-2 073 025,31 / 98,58%)
            //   #1  133 754 855 iterations in 2 000,00 ms. Adjusted: 133 754 848,31    <---- Worst
            //   #2  148 321 852 iterations in 2 000,00 ms. Adjusted: 148 321 844,58
            //   #3  150 275 319 iterations in 2 000,00 ms. Adjusted: 150 275 319,00    <---- Best
            //   Worst-Best difference: 16 520 470,69 (12,35%)
            // 4. TransformInvert32_3_Vector64: 188 008 019 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 62 669 338,61 (-83 521 024,00 / 42,87%)
            //   #1  63 324 217 iterations in 2 000,00 ms. Adjusted: 63 324 213,83      <---- Best
            //   #2  62 625 619 iterations in 2 000,00 ms. Adjusted: 62 625 619,00
            //   #3  62 058 183 iterations in 2 000,00 ms. Adjusted: 62 058 183,00      <---- Worst
            //   Worst-Best difference: 1 266 030,83 (2,04%)
        }

        [Test]
        public void TransformInvert64Test()
        {
            Color64 color = new Color32(128, 255, 64).ToColor64();

            Color64 expected = new Color64(color.A, (ushort)(UInt16.MaxValue - color.R), (ushort)(UInt16.MaxValue - color.G), (ushort)(UInt16.MaxValue - color.B));
            Console.WriteLine($"{"Expected color:",-50} {expected}");

            void DoAssert(Expression<Func<Color64>> e)
            {
                var m = (MethodCallExpression)e.Body;
                Color64 actual = e.Compile().Invoke();
                Console.WriteLine($"{$"{m.Method.Name}:",-50} {actual}");
                Assert.AreEqual(expected, actual);
            }

            DoAssert(() => TransformInvert64_1_Vanilla(color));
            DoAssert(() => TransformInvert64_2_Vector128(color));
            DoAssert(() => TransformInvert64_3_Vector64(color));
            DoAssert(() => TransformInvert64_4_Intrinsics(color));

            new PerformanceTest<Color64>
                {
                    TestName = nameof(TransformInvert64Test),
                    TestTime = 2000,
                    //Iterations = 10_000_000,
                    Repeat = 3
                }
                .AddCase(() => TransformInvert64_1_Vanilla(color), nameof(TransformInvert64_1_Vanilla))
                .AddCase(() => TransformInvert64_2_Vector128(color), nameof(TransformInvert64_2_Vector128))
                .AddCase(() => TransformInvert64_3_Vector64(color), nameof(TransformInvert64_3_Vector64))
                .AddCase(() => TransformInvert64_4_Intrinsics(color), nameof(TransformInvert64_4_Intrinsics))
                .DoTest()
                .DumpResults(Console.Out);

            // .NET 10:
            // 1. TransformInvert64_4_Intrinsics: 440 379 053 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 146 793 015,08
            //   #1  155 276 174 iterations in 2 000,00 ms. Adjusted: 155 276 166,24	 <---- Best
            //   #2  153 587 675 iterations in 2 000,00 ms. Adjusted: 153 587 675,00
            //   #3  131 515 204 iterations in 2 000,00 ms. Adjusted: 131 515 204,00	 <---- Worst
            //   Worst-Best difference: 23 760 962,24 (18,07%)
            // 2. TransformInvert64_2_Vector128: 398 431 113 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 132 810 371,00 (-13 982 644,08 / 90,47%)
            //   #1  155 717 335 iterations in 2 000,00 ms. Adjusted: 155 717 335,00	 <---- Best
            //   #2  136 447 777 iterations in 2 000,00 ms. Adjusted: 136 447 777,00
            //   #3  106 266 001 iterations in 2 000,00 ms. Adjusted: 106 266 001,00	 <---- Worst
            //   Worst-Best difference: 49 451 334,00 (46,54%)
            // 3. TransformInvert64_1_Vanilla: 388 432 068 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 129 477 356,00 (-17 315 659,08 / 88,20%)
            //   #1  127 206 506 iterations in 2 000,00 ms. Adjusted: 127 206 506,00
            //   #2  123 920 349 iterations in 2 000,00 ms. Adjusted: 123 920 349,00	 <---- Worst
            //   #3  137 305 213 iterations in 2 000,00 ms. Adjusted: 137 305 213,00	 <---- Best
            //   Worst-Best difference: 13 384 864,00 (10,80%)
            // 4. TransformInvert64_3_Vector64: 191 504 275 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 63 834 757,25 (-82 958 257,83 / 43,49%)
            //   Worst-Best difference: 7 688 659,00 (12,97%)
            //   #1  65 247 250 iterations in 2 000,00 ms. Adjusted: 65 247 246,74
            //   #2  59 284 183 iterations in 2 000,00 ms. Adjusted: 59 284 183,00	 <---- Worst
            //   #3  66 972 842 iterations in 2 000,00 ms. Adjusted: 66 972 842,00	 <---- Best
        }

        [Test]
        public void TransformInvertFTest()
        {
            ColorF color = new Color32(128, 255, 64).ToColorF();

            ColorF expected = new ColorF(color.A, 1f - color.R, 1f - color.G, 1f - color.B);
            Console.WriteLine($"{"Expected color:",-50} {expected}");

            void DoAssert(Expression<Func<ColorF>> e)
            {
                var m = (MethodCallExpression)e.Body;
                ColorF actual = e.Compile().Invoke();
                Console.WriteLine($"{$"{m.Method.Name}:",-50} {actual}");
                Assert.AreEqual(expected, actual);
            }

            DoAssert(() => TransformInvertF_1_Vanilla(color));
            DoAssert(() => TransformInvertF_2_Vector(color));
            DoAssert(() => TransformInvertF_3_Intrinsics(color));

            new PerformanceTest<ColorF>
                {
                    TestName = nameof(TransformInvertFTest),
                    TestTime = 2000,
                    //Iterations = 10_000_000,
                    Repeat = 3
                }
                .AddCase(() => TransformInvertF_1_Vanilla(color), nameof(TransformInvertF_1_Vanilla))
                .AddCase(() => TransformInvertF_2_Vector(color), nameof(TransformInvertF_2_Vector))
                .AddCase(() => TransformInvertF_3_Intrinsics(color), nameof(TransformInvertF_3_Intrinsics))
                .DoTest()
                .DumpResults(Console.Out);

            // .NET 10:
            // 1. TransformInvertF_2_Vector: 447 381 001 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 149 126 997,82
            //   #1  146 034 837 iterations in 2 000,00 ms. Adjusted: 146 034 837,00    <---- Worst
            //   #2  150 786 125 iterations in 2 000,00 ms. Adjusted: 150 786 117,46    <---- Best
            //   #3  150 560 039 iterations in 2 000,00 ms. Adjusted: 150 560 039,00
            //   Worst-Best difference: 4 751 280,46 (3,25%)
            // 2. TransformInvertF_3_Intrinsics: 404 309 740 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 134 769 904,34 (-14 357 093,48 / 90,37%)
            //   #1  133 559 760 iterations in 2 000,00 ms. Adjusted: 133 559 753,32    <---- Worst
            //   #2  135 220 560 iterations in 2 000,00 ms. Adjusted: 135 220 546,48
            //   #3  135 529 420 iterations in 2 000,00 ms. Adjusted: 135 529 413,22    <---- Best
            //   Worst-Best difference: 1 969 659,90 (1,47%)
            // 3. TransformInvertF_1_Vanilla: 335 409 711 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 111 803 231,41 (-37 323 766,41 / 74,97%)
            //   #1  110 332 559 iterations in 2 000,00 ms. Adjusted: 110 332 553,48    <---- Worst
            //   #2  111 744 087 iterations in 2 000,00 ms. Adjusted: 111 744 081,41
            //   #3  113 333 065 iterations in 2 000,00 ms. Adjusted: 113 333 059,33    <---- Best
            //   Worst-Best difference: 3 000 505,85 (2,72%)

            // .NET 6: (same order in .NET 7/8/9)
            // 1. TransformInvertF_3_Intrinsics: 362 113 628 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 120 704 506,53
            //   #1  119 152 033 iterations in 2 000,00 ms. Adjusted: 119 151 985,34    <---- Worst
            //   #2  120 298 511 iterations in 2 000,00 ms. Adjusted: 120 298 480,93
            //   #3  122 663 084 iterations in 2 000,00 ms. Adjusted: 122 663 053,33    <---- Best
            //   Worst-Best difference: 3 511 068,00 (2,95%)
            // 2. TransformInvertF_2_Vector: 324 705 287 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 108 235 052,41 (-12 469 454,12 / 89,67%)
            //   #1  109 460 494 iterations in 2 000,00 ms. Adjusted: 109 460 461,16    <---- Best
            //   #2  107 276 136 iterations in 2 000,00 ms. Adjusted: 107 276 098,45    <---- Worst
            //   #3  107 968 657 iterations in 2 000,00 ms. Adjusted: 107 968 597,62
            //   Worst-Best difference: 2 184 362,71 (2,04%)
            // 3. TransformInvertF_1_Vanilla: 285 190 341 iterations in 6 000,02 ms. Adjusted for 2 000 ms: 95 063 183,58 (-25 641 322,96 / 78,76%)
            //   #1  90 884 264 iterations in 2 000,02 ms. Adjusted: 90 883 541,48      <---- Worst
            //   #2  94 530 748 iterations in 2 000,00 ms. Adjusted: 94 530 710,19
            //   #3  99 775 329 iterations in 2 000,00 ms. Adjusted: 99 775 299,07      <---- Best
            //   Worst-Best difference: 8 891 757,59 (9,78%)

            // .NET Core 3.0: (same order in .NET 5)
            // 1. TransformInvertF_3_Intrinsics: 303 206 082 iterations in 6 000,02 ms. Adjusted for 2 000 ms: 101 068 338,55
            //   #1  101 427 756 iterations in 2 000,01 ms. Adjusted: 101 427 390,86
            //   #2  100 160 198 iterations in 2 000,01 ms. Adjusted: 100 159 852,45    <---- Worst
            //   #3  101 618 128 iterations in 2 000,01 ms. Adjusted: 101 617 772,34    <---- Best
            //   Worst-Best difference: 1 457 919,89 (1,46%)
            // 2. TransformInvertF_1_Vanilla: 250 756 114 iterations in 6 000,04 ms. Adjusted for 2 000 ms: 83 584 844,93 (-17 483 493,61 / 82,70%)
            //   #1  80 218 063 iterations in 2 000,02 ms. Adjusted: 80 217 140,50      <---- Worst
            //   #2  83 963 958 iterations in 2 000,01 ms. Adjusted: 83 963 638,94
            //   #3  86 574 093 iterations in 2 000,01 ms. Adjusted: 86 573 755,36      <---- Best
            //   Worst-Best difference: 6 356 614,86 (7,92%)
            // 3. TransformInvertF_2_Vector: 227 589 824 iterations in 6 000,02 ms. Adjusted for 2 000 ms: 75 863 022,97 (-25 205 315,58 / 75,06%)
            //   #1  74 293 556 iterations in 2 000,01 ms. Adjusted: 74 293 318,26      <---- Worst
            //   #2  76 882 775 iterations in 2 000,01 ms. Adjusted: 76 882 517,44      <---- Best
            //   #3  76 413 493 iterations in 2 000,01 ms. Adjusted: 76 413 233,20
            //   Worst-Best difference: 2 589 199,18 (3,49%)

            // .NET Framework 4.5+:
            // 1. TransformInvertF_2_Vector: 294 083 928 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 98 027 972,83
            //   #1  95 223 085 iterations in 2 000,00 ms. Adjusted: 95 223 075,48      <---- Worst
            //   #2  99 140 525 iterations in 2 000,00 ms. Adjusted: 99 140 525,00
            //   #3  99 720 318 iterations in 2 000,00 ms. Adjusted: 99 720 318,00      <---- Best
            //   Worst-Best difference: 4 497 242,52 (4,72%)
            // 2. TransformInvertF_1_Vanilla: 238 730 660 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 79 576 879,93 (-18 451 092,90 / 81,18%)
            //   #1  80 301 422 iterations in 2 000,00 ms. Adjusted: 80 301 417,98
            //   #2  80 980 743 iterations in 2 000,00 ms. Adjusted: 80 980 726,80      <---- Best
            //   #3  77 448 495 iterations in 2 000,00 ms. Adjusted: 77 448 495,00      <---- Worst
            //   Worst-Best difference: 3 532 231,80 (4,56%)
        }

        [Test]
        public void GenerateGammaLookupTable32Test()
        {
            float gamma = 2.4f;
            byte[] tableRef = new byte[256];
            byte[] tableActual = new byte[256];

            GenerateGammaLookupTable32_1_Vanilla(gamma, tableRef);
            GenerateGammaLookupTable32_2_AutoVector128SimpleLoop(gamma, tableActual);
            CollectionAssert.AreEqual(tableRef, tableActual);
            
            Array.Clear(tableActual, 0, tableActual.Length);
            GenerateGammaLookupTable32_3_AutoVector128UnrolledNarrowingLoop(gamma, tableActual);
            CollectionAssert.AreEqual(tableRef, tableActual);
            
            Array.Clear(tableActual, 0, tableActual.Length);
            GenerateGammaLookupTable32_4_IntrinsicsVector128UnrolledNarrowingLoop(gamma, tableActual);
            CollectionAssert.AreEqual(tableRef, tableActual);
            
            Array.Clear(tableActual, 0, tableActual.Length);
            GenerateGammaLookupTable32_5_AutoVector256SimpleLoop(gamma, tableActual);
            CollectionAssert.AreEqual(tableRef, tableActual);

            Array.Clear(tableActual, 0, tableActual.Length);
            GenerateGammaLookupTable32_6_IntrinsicsVector256UnrolledNarrowingLoop(gamma, tableActual);
            CollectionAssert.AreEqual(tableRef, tableActual);

#if NET8_0_OR_GREATER
            Array.Clear(tableActual, 0, tableActual.Length);
            GenerateGammaLookupTable32_7_AutoVector512SimpleLoop(gamma, tableActual);
            CollectionAssert.AreEqual(tableRef, tableActual);
#endif

            new PerformanceTest<byte[]>
                {
                    TestName = nameof(GenerateGammaLookupTable32Test),
                    TestTime = 2000,
                    //Iterations = 10_000_000,
                    Repeat = 3
                }
                .AddCase(() => GenerateGammaLookupTable32_1_Vanilla(gamma, tableRef), nameof(GenerateGammaLookupTable32_1_Vanilla))
                .AddCase(() => GenerateGammaLookupTable32_2_AutoVector128SimpleLoop(gamma, tableActual), nameof(GenerateGammaLookupTable32_2_AutoVector128SimpleLoop))
                .AddCase(() => GenerateGammaLookupTable32_3_AutoVector128UnrolledNarrowingLoop(gamma, tableActual), nameof(GenerateGammaLookupTable32_3_AutoVector128UnrolledNarrowingLoop))
                .AddCase(() => GenerateGammaLookupTable32_4_IntrinsicsVector128UnrolledNarrowingLoop(gamma, tableActual), nameof(GenerateGammaLookupTable32_4_IntrinsicsVector128UnrolledNarrowingLoop))
                .AddCase(() => GenerateGammaLookupTable32_5_AutoVector256SimpleLoop(gamma, tableActual), nameof(GenerateGammaLookupTable32_5_AutoVector256SimpleLoop))
                .AddCase(() => GenerateGammaLookupTable32_6_IntrinsicsVector256UnrolledNarrowingLoop(gamma, tableActual), nameof(GenerateGammaLookupTable32_6_IntrinsicsVector256UnrolledNarrowingLoop))
#if NET8_0_OR_GREATER
                //.AddCase(() => GenerateGammaLookupTable32_7_AutoVector512SimpleLoop(gamma, tableActual), nameof(GenerateGammaLookupTable32_7_AutoVector512SimpleLoop))
#endif
                .DoTest()
                .DumpResults(Console.Out);

            // Verdict: Auto-vectorization is not good at narrowing even on .NET 10, but the simple loop can be auto-vectorized quite well.
            // So using intrinsics narrowing in the first place, then the auto-vectorized simple loop on .NET 9+, before falling back to the vanilla version.
            // Pow is not vectorized in .NET 8 and lower. Still, the intrinsics Vector128 vectorization is faster than vanilla (but not with Vector256).
            // Assuming that Vector512 auto-vectorization is faster than Vector256, though not providing an intrinsics version for it (for now).

            // .NET 10:
            // 1. GenerateGammaLookupTable32_6_IntrinsicsVector256UnrolledNarrowingLoop: 13,984,489 iterations in 6,000.00 ms. Adjusted for 2,000 ms: 4,661,495.95
            //   #1  4,694,172 iterations in 2,000.00 ms. Adjusted: 4,694,172.00	 <---- Best
            //   #2  4,643,744 iterations in 2,000.00 ms. Adjusted: 4,643,743.77	 <---- Worst
            //   #3  4,646,573 iterations in 2,000.00 ms. Adjusted: 4,646,572.07
            //   Worst-Best difference: 50,428.23 (1.09%)
            // 2. GenerateGammaLookupTable32_5_AutoVector256SimpleLoop: 10,573,049 iterations in 6,000.00 ms. Adjusted for 2,000 ms: 3,524,348.85 (-1,137,147.10 / 75.61%)
            //   #1  3,505,559 iterations in 2,000.00 ms. Adjusted: 3,505,557.42	 <---- Worst
            //   #2  3,550,117 iterations in 2,000.00 ms. Adjusted: 3,550,116.47	 <---- Best
            //   #3  3,517,373 iterations in 2,000.00 ms. Adjusted: 3,517,372.65
            //   Worst-Best difference: 44,559.04 (1.27%)
            // 3. GenerateGammaLookupTable32_4_IntrinsicsVector128UnrolledNarrowingLoop: 9,505,094 iterations in 6,000.00 ms. Adjusted for 2,000 ms: 3,168,364.29 (-1,493,131.66 / 67.97%)
            //   #1  2,310,428 iterations in 2,000.00 ms. Adjusted: 2,310,427.77	 <---- Worst
            //   #2  3,523,508 iterations in 2,000.00 ms. Adjusted: 3,523,507.47
            //   #3  3,671,158 iterations in 2,000.00 ms. Adjusted: 3,671,157.63	 <---- Best
            //   Worst-Best difference: 1,360,729.86 (58.90%)
            // 4. GenerateGammaLookupTable32_2_AutoVector128SimpleLoop: 7,669,888 iterations in 6,000.00 ms. Adjusted for 2,000 ms: 2,556,628.82 (-2,104,867.12 / 54.85%)
            //   #1  2,083,682 iterations in 2,000.00 ms. Adjusted: 2,083,681.58	 <---- Worst
            //   #2  2,772,789 iterations in 2,000.00 ms. Adjusted: 2,772,788.45
            //   #3  2,813,417 iterations in 2,000.00 ms. Adjusted: 2,813,416.44	 <---- Best
            //   Worst-Best difference: 729,734.85 (35.02%)
            // 5. GenerateGammaLookupTable32_3_AutoVector128UnrolledNarrowingLoop: 7,392,427 iterations in 6,000.00 ms. Adjusted for 2,000 ms: 2,464,141.70 (-2,197,354.24 / 52.86%)
            //   #1  2,954,207 iterations in 2,000.00 ms. Adjusted: 2,954,206.26	 <---- Best
            //   #2  2,606,737 iterations in 2,000.00 ms. Adjusted: 2,606,736.22
            //   #3  1,831,483 iterations in 2,000.00 ms. Adjusted: 1,831,482.63	 <---- Worst
            //   Worst-Best difference: 1,122,723.63 (61.30%)
            // 6. GenerateGammaLookupTable32_1_Vanilla: 2,100,455 iterations in 6,000.00 ms. Adjusted for 2,000 ms: 700,151.26 (-3,961,344.68 / 15.02%)
            //   #1  731,370 iterations in 2,000.00 ms. Adjusted: 731,369.71
            //   #2  772,199 iterations in 2,000.00 ms. Adjusted: 772,198.50	 <---- Best
            //   #3  596,886 iterations in 2,000.00 ms. Adjusted: 596,885.58	 <---- Worst
            //   Worst-Best difference: 175,312.92 (29.37%)

            // .NET 8:
            // 1. GenerateGammaLookupTable32_4_IntrinsicsVector128UnrolledNarrowingLoop: 3,141,886 iterations in 6,000.00 ms. Adjusted for 2,000 ms: 1,047,294.53
            //   #1  1,073,801 iterations in 2,000.00 ms. Adjusted: 1,073,800.41	 <---- Best
            //   #2  1,044,057 iterations in 2,000.00 ms. Adjusted: 1,044,056.11
            //   #3  1,024,028 iterations in 2,000.00 ms. Adjusted: 1,024,027.08	 <---- Worst
            //   Worst-Best difference: 49,773.33 (4.86%)
            // 2. GenerateGammaLookupTable32_3_AutoVector128UnrolledNarrowingLoop: 2,536,835 iterations in 6,000.01 ms. Adjusted for 2,000 ms: 845,610.68 (-201,683.86 / 80.74%)
            //   #1  870,747 iterations in 2,000.00 ms. Adjusted: 870,746.17
            //   #2  871,095 iterations in 2,000.00 ms. Adjusted: 871,094.09	 <---- Best
            //   #3  794,993 iterations in 2,000.00 ms. Adjusted: 794,991.77	 <---- Worst
            //   Worst-Best difference: 76,102.32 (9.57%)
            // 3. GenerateGammaLookupTable32_2_AutoVector128SimpleLoop: 2,499,003 iterations in 6,000.01 ms. Adjusted for 2,000 ms: 832,999.46 (-214,295.07 / 79.54%)
            //   #1  830,920 iterations in 2,000.01 ms. Adjusted: 830,917.47
            //   #2  853,322 iterations in 2,000.00 ms. Adjusted: 853,320.93	 <---- Best
            //   #3  814,761 iterations in 2,000.00 ms. Adjusted: 814,759.98	 <---- Worst
            //   Worst-Best difference: 38,560.95 (4.73%)
            // 4. GenerateGammaLookupTable32_1_Vanilla: 2,299,454 iterations in 6,000.02 ms. Adjusted for 2,000 ms: 766,482.67 (-280,811.86 / 73.19%)
            //   #1  723,930 iterations in 2,000.01 ms. Adjusted: 723,925.19	 <---- Worst
            //   #2  822,449 iterations in 2,000.00 ms. Adjusted: 822,448.59	 <---- Best
            //   #3  753,075 iterations in 2,000.00 ms. Adjusted: 753,074.25
            //   Worst-Best difference: 98,523.40 (13.61%)
            // 5. GenerateGammaLookupTable32_6_IntrinsicsVector256UnrolledNarrowingLoop: 1,640,178 iterations in 6,000.01 ms. Adjusted for 2,000 ms: 546,725.33 (-500,569.21 / 52.20%)
            //   #1  548,295 iterations in 2,000.00 ms. Adjusted: 548,294.26	 <---- Best
            //   #2  545,174 iterations in 2,000.00 ms. Adjusted: 545,173.18	 <---- Worst
            //   #3  546,709 iterations in 2,000.00 ms. Adjusted: 546,708.54
            //   Worst-Best difference: 3,121.08 (0.57%)
            // 6. GenerateGammaLookupTable32_5_AutoVector256SimpleLoop: 1,318,256 iterations in 6,000.01 ms. Adjusted for 2,000 ms: 439,417.90 (-607,876.63 / 41.96%)
            //   #1  412,009 iterations in 2,000.00 ms. Adjusted: 412,008.09	 <---- Worst
            //   #2  446,494 iterations in 2,000.00 ms. Adjusted: 446,493.29
            //   #3  459,753 iterations in 2,000.00 ms. Adjusted: 459,752.33	 <---- Best
            //   Worst-Best difference: 47,744.24 (11.59%)
        }

        [Test]
        public void GenerateGammaLookupTable64Test()
        {
            #region Local Methods

            static void DoAssert(ushort[] expected, ushort[] actual)
            {
                for (int i = 0; i < expected.Length; i++)
                {
                    // the vectorized Pow may differ by one in 16 bit range
                    if ((expected[i] - actual[i]).Abs() > 1)
                        Assert.Fail($"Tables differ at index {i}: expected={expected[i]}, actual={actual[i]}");
                }
            }

            #endregion

            float gamma = 2.4f;
            ushort[] tableRef = new ushort[65536];
            ushort[] tableActual = new ushort[65536];

            GenerateGammaLookupTable64_1_Vanilla(gamma, tableRef);
            GenerateGammaLookupTable64_2_AutoVector128SimpleLoop(gamma, tableActual);
            DoAssert(tableRef, tableActual);

            Array.Clear(tableActual, 0, tableActual.Length);
            GenerateGammaLookupTable64_3_AutoVector128UnrolledNarrowingLoop(gamma, tableActual);
            DoAssert(tableRef, tableActual);

            Array.Clear(tableActual, 0, tableActual.Length);
            GenerateGammaLookupTable64_4_IntrinsicsVector128UnrolledNarrowingLoop(gamma, tableActual);
            DoAssert(tableRef, tableActual);

            Array.Clear(tableActual, 0, tableActual.Length);
            GenerateGammaLookupTable64_5_AutoVector256SimpleLoop(gamma, tableActual);
            DoAssert(tableRef, tableActual);

            Array.Clear(tableActual, 0, tableActual.Length);
            GenerateGammaLookupTable64_6_IntrinsicsVector256UnrolledNarrowingLoop(gamma, tableActual);
            DoAssert(tableRef, tableActual);

#if NET9_0_OR_GREATER
            Array.Clear(tableActual, 0, tableActual.Length);
            GenerateGammaLookupTable64_7_AutoVector512SimpleLoop(gamma, tableActual);
            DoAssert(tableRef, tableActual);
#endif

            new PerformanceTest<ushort[]>
                {
                    TestName = nameof(GenerateGammaLookupTable64Test),
                    TestTime = 2000,
                    //Iterations = 10_000_000,
                    Repeat = 3
                }
                .AddCase(() => GenerateGammaLookupTable64_1_Vanilla(gamma, tableRef), nameof(GenerateGammaLookupTable64_1_Vanilla))
                .AddCase(() => GenerateGammaLookupTable64_2_AutoVector128SimpleLoop(gamma, tableActual), nameof(GenerateGammaLookupTable64_2_AutoVector128SimpleLoop))
                .AddCase(() => GenerateGammaLookupTable64_3_AutoVector128UnrolledNarrowingLoop(gamma, tableActual), nameof(GenerateGammaLookupTable64_3_AutoVector128UnrolledNarrowingLoop))
                .AddCase(() => GenerateGammaLookupTable64_4_IntrinsicsVector128UnrolledNarrowingLoop(gamma, tableActual), nameof(GenerateGammaLookupTable64_4_IntrinsicsVector128UnrolledNarrowingLoop))
                .AddCase(() => GenerateGammaLookupTable64_5_AutoVector256SimpleLoop(gamma, tableActual), nameof(GenerateGammaLookupTable64_5_AutoVector256SimpleLoop))
                .AddCase(() => GenerateGammaLookupTable64_6_IntrinsicsVector256UnrolledNarrowingLoop(gamma, tableActual), nameof(GenerateGammaLookupTable64_6_IntrinsicsVector256UnrolledNarrowingLoop))
#if NET8_0_OR_GREATER
                //.AddCase(() => GenerateGammaLookupTable64_7_AutoVector512SimpleLoop(gamma, tableActual), nameof(GenerateGammaLookupTable64_7_AutoVector512SimpleLoop))
#endif
                .DoTest()
                .DumpResults(Console.Out);

            // Verdict: The same as in GenerateGammaLookupTable32Test.

            // .NET 10:
            // 1. GenerateGammaLookupTable64_6_IntrinsicsVector256UnrolledNarrowingLoop: 61,023 iterations in 6,000.15 ms. Adjusted for 2,000 ms: 20,340.51
            //   #1  20,488 iterations in 2,000.00 ms. Adjusted: 20,487.95	 <---- Best
            //   #2  20,352 iterations in 2,000.07 ms. Adjusted: 20,351.31
            //   #3  20,183 iterations in 2,000.07 ms. Adjusted: 20,182.26	 <---- Worst
            //   Worst-Best difference: 305.69 (1.51%)
            // 2. GenerateGammaLookupTable64_5_AutoVector256SimpleLoop: 50,232 iterations in 6,000.14 ms. Adjusted for 2,000 ms: 16,743.62 (-3,596.89 / 82.32%)
            //   #1  16,723 iterations in 2,000.01 ms. Adjusted: 16,722.94
            //   #2  16,704 iterations in 2,000.08 ms. Adjusted: 16,703.35	 <---- Worst
            //   #3  16,805 iterations in 2,000.05 ms. Adjusted: 16,804.56	 <---- Best
            //   Worst-Best difference: 101.21 (0.61%)
            // 3. GenerateGammaLookupTable64_4_IntrinsicsVector128UnrolledNarrowingLoop: 41,937 iterations in 6,000.20 ms. Adjusted for 2,000 ms: 13,978.52 (-6,361.99 / 68.72%)
            //   #1  12,905 iterations in 2,000.04 ms. Adjusted: 12,904.77	 <---- Worst
            //   #2  13,118 iterations in 2,000.06 ms. Adjusted: 13,117.61
            //   #3  15,914 iterations in 2,000.10 ms. Adjusted: 15,913.19	 <---- Best
            //   Worst-Best difference: 3,008.42 (23.31%)
            // 4. GenerateGammaLookupTable64_2_AutoVector128SimpleLoop: 39,163 iterations in 6,000.14 ms. Adjusted for 2,000 ms: 13,054.04 (-7,286.46 / 64.18%)
            //   #1  14,054 iterations in 2,000.05 ms. Adjusted: 14,053.66
            //   #2  14,102 iterations in 2,000.01 ms. Adjusted: 14,101.96	 <---- Best
            //   #3  11,007 iterations in 2,000.09 ms. Adjusted: 11,006.52	 <---- Worst
            //   Worst-Best difference: 3,095.44 (28.12%)
            // 5. GenerateGammaLookupTable64_3_AutoVector128UnrolledNarrowingLoop: 29,983 iterations in 6,000.38 ms. Adjusted for 2,000 ms: 9,993.70 (-10,346.80 / 49.13%)
            //   #1  10,883 iterations in 2,000.10 ms. Adjusted: 10,882.46	 <---- Best
            //   #2  8,367 iterations in 2,000.12 ms. Adjusted: 8,366.48	 <---- Worst
            //   #3  10,733 iterations in 2,000.15 ms. Adjusted: 10,732.17
            //   Worst-Best difference: 2,515.97 (30.07%)
            // 6. GenerateGammaLookupTable64_1_Vanilla: 7,912 iterations in 6,001.16 ms. Adjusted for 2,000 ms: 2,636.83 (-17,703.68 / 12.96%)
            //   #1  2,850 iterations in 2,000.09 ms. Adjusted: 2,849.88	 <---- Best
            //   #2  2,652 iterations in 2,000.87 ms. Adjusted: 2,650.84
            //   #3  2,410 iterations in 2,000.20 ms. Adjusted: 2,409.76	 <---- Worst
            //   Worst-Best difference: 440.12 (18.26%)

            // .NET 8:
            // 1. GenerateGammaLookupTable64_4_IntrinsicsVector128UnrolledNarrowingLoop: 11,805 iterations in 6,001.06 ms. Adjusted for 2,000 ms: 3,934.32
            //   #1  3,784 iterations in 2,000.61 ms. Adjusted: 3,782.84	 <---- Worst
            //   #2  3,793 iterations in 2,000.29 ms. Adjusted: 3,792.44
            //   #3  4,228 iterations in 2,000.15 ms. Adjusted: 4,227.68	 <---- Best
            //   Worst-Best difference: 444.84 (11.76%)
            // 2. GenerateGammaLookupTable64_2_AutoVector128SimpleLoop: 10,381 iterations in 6,001.84 ms. Adjusted for 2,000 ms: 3,459.29 (-475.03 / 87.93%)
            //   #1  3,379 iterations in 2,001.30 ms. Adjusted: 3,376.80
            //   #2  3,271 iterations in 2,000.27 ms. Adjusted: 3,270.56	 <---- Worst
            //   #3  3,731 iterations in 2,000.26 ms. Adjusted: 3,730.51	 <---- Best
            //   Worst-Best difference: 459.96 (14.06%)
            // 3. GenerateGammaLookupTable64_3_AutoVector128UnrolledNarrowingLoop: 9,036 iterations in 6,001.35 ms. Adjusted for 2,000 ms: 3,011.31 (-923.01 / 76.54%)
            //   #1  3,472 iterations in 2,000.53 ms. Adjusted: 3,471.09	 <---- Best
            //   #2  2,906 iterations in 2,000.37 ms. Adjusted: 2,905.46
            //   #3  2,658 iterations in 2,000.45 ms. Adjusted: 2,657.40	 <---- Worst
            //   Worst-Best difference: 813.69 (30.62%)
            // 4. GenerateGammaLookupTable64_1_Vanilla: 8,500 iterations in 6,000.81 ms. Adjusted for 2,000 ms: 2,832.96 (-1,101.36 / 72.01%)
            //   #1  3,018 iterations in 2,000.03 ms. Adjusted: 3,017.95	 <---- Best
            //   #2  2,788 iterations in 2,000.35 ms. Adjusted: 2,787.51
            //   #3  2,694 iterations in 2,000.43 ms. Adjusted: 2,693.42	 <---- Worst
            //   Worst-Best difference: 324.53 (12.05%)
            // 5. GenerateGammaLookupTable64_6_IntrinsicsVector256UnrolledNarrowingLoop: 6,603 iterations in 6,001.96 ms. Adjusted for 2,000 ms: 2,200.28 (-1,734.04 / 55.93%)
            //   #1  2,202 iterations in 2,000.67 ms. Adjusted: 2,201.26
            //   #2  2,198 iterations in 2,000.84 ms. Adjusted: 2,197.08	 <---- Worst
            //   #3  2,203 iterations in 2,000.45 ms. Adjusted: 2,202.51	 <---- Best
            //   Worst-Best difference: 5.43 (0.25%)
            // 6. GenerateGammaLookupTable64_5_AutoVector256SimpleLoop: 6,243 iterations in 6,000.87 ms. Adjusted for 2,000 ms: 2,080.70 (-1,853.63 / 52.89%)
            //   #1  2,086 iterations in 2,000.26 ms. Adjusted: 2,085.73
            //   #2  2,087 iterations in 2,000.49 ms. Adjusted: 2,086.49	 <---- Best
            //   #3  2,070 iterations in 2,000.12 ms. Adjusted: 2,069.87	 <---- Worst
            //   Worst-Best difference: 16.61 (0.80%)
        }

        [Test]
        public void TransformGammaFTest()
        {
            ColorF color = new Color32(128, 255, 64).ToColorF();
            const float gamma = 2.4f;

            ColorF expected = TransformGammaPerChannelF(color, ColorChannels.Rgb, gamma);
            Console.WriteLine($"{"Expected color:",-50} {expected}");

            void DoAssert(Expression<Func<ColorF>> e)
            {
                var m = (MethodCallExpression)e.Body;
                ColorF actual = e.Compile().Invoke();
                Console.WriteLine($"{$"{m.Method.Name}:",-50} {actual}");
                Assert.IsTrue(expected.TolerantEquals(actual));
            }

            DoAssert(() => TransformGammaF_1_Vanilla(color, gamma));
            DoAssert(() => TransformGammaF_2_Vector(color, gamma));

            new PerformanceTest<ColorF>
                {
                    TestName = nameof(TransformContrastFTest),
                    TestTime = 2000,
                    //Iterations = 10_000_000,
                    Repeat = 3
                }
                .AddCase(() => TransformGammaPerChannelF(color, ColorChannels.Rgb, gamma), nameof(TransformGammaPerChannelF))
                .AddCase(() => TransformGammaF_1_Vanilla(color, gamma), nameof(TransformGammaF_1_Vanilla))
                .AddCase(() => TransformGammaF_2_Vector(color, gamma), nameof(TransformGammaF_2_Vector))
                .DoTest()
                .DumpResults(Console.Out);

            // .NET 10:
            // 1. TransformGammaF_2_Vector: 147,921,390 iterations in 6,000.00 ms. Adjusted for 2,000 ms: 49,307,130.00
            //   #1  47,139,830 iterations in 2,000.00 ms. Adjusted: 47,139,830.00
            //   #2  46,413,256 iterations in 2,000.00 ms. Adjusted: 46,413,256.00	 <---- Worst
            //   #3  54,368,304 iterations in 2,000.00 ms. Adjusted: 54,368,304.00	 <---- Best
            //   Worst-Best difference: 7,955,048.00 (17.14%)
            // 2. TransformGammaPerChannelF: 125,676,671 iterations in 6,000.00 ms. Adjusted for 2,000 ms: 41,892,223.67 (-7,414,906.33 / 84.96%)
            //   #1  40,546,701 iterations in 2,000.00 ms. Adjusted: 40,546,701.00	 <---- Worst
            //   #2  42,654,478 iterations in 2,000.00 ms. Adjusted: 42,654,478.00	 <---- Best
            //   #3  42,475,492 iterations in 2,000.00 ms. Adjusted: 42,475,492.00
            //   Worst-Best difference: 2,107,777.00 (5.20%)
            // 3. TransformGammaF_1_Vanilla: 123,985,555 iterations in 6,000.00 ms. Adjusted for 2,000 ms: 41,328,504.53 (-7,978,625.47 / 83.82%)
            //   #1  40,581,126 iterations in 2,000.00 ms. Adjusted: 40,581,126.00	 <---- Worst
            //   #2  41,401,944 iterations in 2,000.00 ms. Adjusted: 41,401,902.60
            //   #3  42,002,485 iterations in 2,000.00 ms. Adjusted: 42,002,485.00	 <---- Best
            //   Worst-Best difference: 1,421,359.00 (3.50%)
        }

        #endregion

        #endregion
    }
}
