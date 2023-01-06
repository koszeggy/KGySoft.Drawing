#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: SKBitmapExtensionsTest.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2022 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;

using KGySoft.CoreLibraries;
using KGySoft.Drawing.Imaging;

using NUnit.Framework;

using SkiaSharp;

#endregion

namespace KGySoft.Drawing.SkiaSharp.UnitTests
{
    [TestFixture]
    public class SKBitmapExtensionsTest : TestBase
    {
        #region Fields

        private static readonly object[] sourceDirectlySupportedSetGetPixelTest =
        {
            new object[] { SKColorType.Bgra8888, SKAlphaType.Unpremul },
            new object[] { SKColorType.Bgra8888, SKAlphaType.Premul },
            new object[] { SKColorType.Bgra8888, SKAlphaType.Opaque },

            //new object[] { SKColorType.Rgba8888, SKAlphaType.Unpremul, testColorAlpha, testColorAlpha },
            //new object[] { SKColorType.Rgba8888, SKAlphaType.Premul, testColorAlpha, testColorAlpha },
            //new object[] { SKColorType.Rgba8888, SKAlphaType.Opaque, testColorAlpha, testColorBlended },

            //new object[] { SKColorType.Rgb888x, SKAlphaType.Opaque, testColorAlpha, testColorBlended },

            //new object[] { SKColorType.Gray8, SKAlphaType.Opaque, testColorAlpha, testColorAlpha.ToColor32().Blend(Color.Black).ToGray().ToColor() },

            //new object[] { SKColorType.Rgb565, SKAlphaType.Opaque, testColorAlpha, Color.FromArgb((testColorBlended.R & 0b11111000) | (testColorBlended.R >> 5), (testColorBlended.G & 0b11111100) | (testColorBlended.G >> 6), (testColorBlended.B & 0b11111000) | (testColorBlended.B >> 5)) },

            //new object[] { SKColorType.Rgba16161616, SKAlphaType.Unpremul, testColorAlpha, testColorAlpha },
            //new object[] { SKColorType.Rgba16161616, SKAlphaType.Premul, testColorAlpha, testColorAlpha },
            //new object[] { SKColorType.Rgba16161616, SKAlphaType.Opaque, testColorAlpha, testColorBlended },

            //new object[] { SKColorType.Bgra1010102, SKAlphaType.Unpremul, testColorAlpha, new ColorBgra1010102(testColorAlpha).ToColor32().ToColor() },
            //new object[] { SKColorType.Bgra1010102, SKAlphaType.Premul, testColorAlpha, new ColorBgra1010102(testColorAlpha).ToColor32().ToColor() },
            //new object[] { SKColorType.Bgra1010102, SKAlphaType.Opaque, testColorAlpha, testColorBlended },

            //new object[] { SKColorType.Bgr101010x, SKAlphaType.Opaque, testColorAlpha, testColorBlended },

            //new object[] { SKColorType.Rgba1010102, SKAlphaType.Unpremul, testColorAlpha, new ColorRgba1010102(testColorAlpha).ToColor32().ToColor() },
            //new object[] { SKColorType.Rgba1010102, SKAlphaType.Premul, testColorAlpha, new ColorRgba1010102(testColorAlpha).ToColor32().ToColor() },
            //new object[] { SKColorType.Rgba1010102, SKAlphaType.Opaque, testColorAlpha, testColorBlended },

            //new object[] { SKColorType.Rgb101010x, SKAlphaType.Opaque, testColorAlpha, testColorBlended },

            //new object[] { SKColorType.Argb4444, SKAlphaType.Unpremul, testColorAlpha, new ColorArgb4444(testColorAlpha).ToColor32().ToColor() },
            //new object[] { SKColorType.Argb4444, SKAlphaType.Premul, testColorAlpha, new ColorArgb4444(testColorAlpha).ToPremultiplied().ToStraight().ToColor32().ToColor() },
            //new object[] { SKColorType.Argb4444, SKAlphaType.Opaque, testColorAlpha, new ColorArgb4444(testColorBlended).ToColor32().ToColor() },

            //new object[] { SKColorType.RgbaF32, SKAlphaType.Unpremul, testColorAlpha, testColorAlpha },
            //new object[] { SKColorType.RgbaF32, SKAlphaType.Premul, testColorAlpha, testColorAlpha },
            //new object[] { SKColorType.RgbaF32, SKAlphaType.Opaque, testColorAlpha, testColorBlended },

            //new object[] { SKColorType.RgbaF16, SKAlphaType.Unpremul, testColorAlpha, testColorAlpha },
            //new object[] { SKColorType.RgbaF16, SKAlphaType.Premul, testColorAlpha, testColorAlpha },
            //new object[] { SKColorType.RgbaF16, SKAlphaType.Opaque, testColorAlpha, testColorBlended },
            //new object[] { SKColorType.RgbaF16Clamped, SKAlphaType.Unpremul, testColorAlpha, testColorAlpha },
            //new object[] { SKColorType.RgbaF16Clamped, SKAlphaType.Premul, testColorAlpha, testColorAlpha },
            //new object[] { SKColorType.RgbaF16Clamped, SKAlphaType.Opaque, testColorAlpha, testColorBlended },

            //new object[] { SKColorType.Alpha8, SKAlphaType.Premul, testColorAlpha, Color.FromArgb(testColorAlpha.A, 0, 0, 0) },
            //new object[] { SKColorType.Alpha8, SKAlphaType.Opaque, testColorAlpha, Color.FromArgb(testColorAlpha.A, 0, 0, 0) },

            //new object[] { SKColorType.Alpha16, SKAlphaType.Premul, testColorAlpha, Color.FromArgb(testColorAlpha.A, 0, 0, 0) },
            //new object[] { SKColorType.Alpha16, SKAlphaType.Opaque, testColorAlpha, Color.FromArgb(testColorAlpha.A, 0, 0, 0) },

            //new object[] { SKColorType.AlphaF16, SKAlphaType.Premul, testColorAlpha, Color.FromArgb(testColorAlpha.A, 0, 0, 0) },
            //new object[] { SKColorType.AlphaF16, SKAlphaType.Opaque, testColorAlpha, Color.FromArgb(testColorAlpha.A, 0, 0, 0) },

            //new object[] { SKColorType.Rg88, SKAlphaType.Opaque, testColorAlpha, Color.FromArgb(testColorBlended.R, testColorBlended.G, 0) },
            //new object[] { SKColorType.Rg1616, SKAlphaType.Opaque, testColorAlpha, Color.FromArgb(testColorBlended.R, testColorBlended.G, 0) },
            //new object[] { SKColorType.RgF16, SKAlphaType.Opaque, testColorAlpha, Color.FromArgb(testColorBlended.R, testColorBlended.G, 0) },
        };

        #endregion

        #region Methods

        [TestCaseSource(nameof(sourceDirectlySupportedSetGetPixelTest))]
        public void DirectlySupportedSetGetPixelTest(SKColorType colorType, SKAlphaType alphaType)
        {
            foreach (var colorSpace in new[] { SKColorSpace.CreateSrgb(), SKColorSpace.CreateSrgbLinear(), })
            {
                var info = new SKImageInfo(2, 2, colorType, alphaType, colorSpace);
                var testColor = Color.FromArgb(0x80, 0x80, 0xFF, 0x40).ToColor32();
                bool linear = colorSpace.GammaIsLinear;
                var expectedResult = testColor;
                if (alphaType == SKAlphaType.Opaque)
                    expectedResult = linear ? expectedResult.ToLinear().Blend(Color.Black).ToSrgb() : expectedResult.Blend(Color.Black);

                Assert.IsTrue(info.IsDirectlySupported(), $"Format is not supported directly: {colorType}/{alphaType}/{colorSpace.NamedGamma}");

                Console.WriteLine($"{$"{colorType}/{alphaType}/{colorSpace.NamedGamma}",-32}- {testColor} ->");
                using var bitmap = new SKBitmap(info, info.RowBytes);
                Assert.AreEqual(colorType, bitmap.ColorType);
                Assert.AreEqual(alphaType, bitmap.AlphaType);

                bitmap.SetPixel(0, 0, testColor.ToSKColor());
                var actualNative = bitmap.GetPixel(0, 0);

                using var bitmapData = bitmap.GetReadWriteBitmapData();
                bitmapData.SetPixel(1, 1, testColor);
                var actual = bitmapData.GetPixel(1, 1);
                byte tolerance = (byte)(colorSpace.IsSrgb ? 1 : 3);

                var raw = new List<byte>();
                for (int i = 0; i < colorType.GetBytesPerPixel(); i++)
                    raw.Insert(0, bitmapData[0].ReadRaw<byte>(i));
                Console.WriteLine($"{"by SkiaSharp",-32}- {actualNative.ToColor32()} ({raw.Select(b => Convert.ToString(b, 2).PadLeft(8, '0')).Join('_')}) {(expectedResult.TolerantEquals(actualNative.ToColor32(), tolerance) ? "OK" : "!")}");

                raw.Clear();
                int offset = info.BytesPerPixel;
                for (int i = 0; i < colorType.GetBytesPerPixel(); i++)
                    raw.Insert(0, bitmapData[1].ReadRaw<byte>(i + offset));
                Console.WriteLine($"{"by KGySoft",-32}- {actual.ToColor32()} ({raw.Select(b => Convert.ToString(b, 2).PadLeft(8, '0')).Join('_')}) {(expectedResult.TolerantEquals(actual.ToColor32(), tolerance) ? "OK" : "!")}");
                Console.WriteLine();

                // not comparing Skia result to KGySoft because they can be different and this is known:
                // - grayscale conversion is different
                // - if colorType supports alpha but alphaType is Opaque, Skia uses premultiplied pixel write and gets the raw value
                // - Bgra1010102, Rgba1010102: Skia does not use blending
                // - For non-sRGB color spaces GetPixel does not work: https://github.com/mono/SkiaSharp/issues/2354

                Assert.IsTrue(expectedResult.TolerantEquals(actual.ToColor32(), tolerance), $"KGySoft: {expectedResult} vs. {actual.ToColor32()}");
            }
        }

        [Test]
        public void SetGetPixelCompareTestSrgb()
        {
            foreach (SKColorType colorType in Enum<SKColorType>.GetValues() /*new[] { SKColorType.RgbaF32 }*/)
            {
                if (colorType == SKColorType.Unknown)
                    continue;

                foreach (SKAlphaType alphaType in Enum<SKAlphaType>.GetValues() /* new[] { SKAlphaType.Premul,*/)
                {
                    if (alphaType == SKAlphaType.Unknown)
                        continue;
                    using var bitmap = new SKBitmap(new SKImageInfo(512, 256, colorType, alphaType));
                    if (bitmap.AlphaType != alphaType)
                        continue;

                    PixelFormatInfo info = bitmap.Info.GetInfo();
                    var testColor = Color.FromArgb(0x80, 0x80, 0xFF, 0x40).ToColor32();

                    // Skia and KGy SOFT converts colors to gray a bit differently so pre-converting the color to avoid differences
                    if (info.Grayscale)
                        testColor = testColor.ToGray();

                    // Pre-blending the color for opaque types because Skia handles alpha for them oddly:
                    // alpha is preserved while color is premultiplied, but when getting the pixel, the raw value is not converted back to straight color.
                    // Pre-blending also for types with discrete alpha because Skia ignores back color for them or uses an arbitrary transformation
                    if (!info.HasAlpha || colorType is SKColorType.Bgra1010102 or SKColorType.Rgba1010102 or SKColorType.Argb4444)
                        testColor = testColor.Blend(Color.Black);

                    bitmap.SetPixel(2, 3, testColor.ToSKColor());
                    Color32 expected = bitmap.GetPixel(2, 3).ToColor32();
                    Console.Write($"{colorType}/{alphaType}: {bitmap.Info.AlphaType} ===> {testColor} -> {expected} vs. ");

                    using IReadWriteBitmapData readWriteBitmapData = bitmap.GetReadWriteBitmapData(Color.Black.ToSKColor());
                    readWriteBitmapData.SetPixel(3, 4, testColor);
                    Color32 actual = readWriteBitmapData.GetPixel(3, 4).ToColor32();
                    Console.WriteLine(actual);

                    if (SaveToFile)
                    {
                        // saving an example by SkiaSharp
                        GenerateAlphaGradient(bitmap);
                        SaveBitmap($"{colorType}_{alphaType}_Skia", bitmap);

                        // saving an example by KGySoft
                        GenerateAlphaGradient(readWriteBitmapData);
                        SaveBitmap($"{colorType}_{alphaType}_KGy", bitmap);
                    }

                    try
                    {
                        Assert.IsTrue(expected.TolerantEquals(actual, 1), $"{colorType}/{alphaType} - SkiaSharp: {expected} vs. KGySoft: {actual}");
                    }
                    catch (AssertionException)
                    {
                        // To go on with the other tests. The test will fail anyway.
                    }
                }
            }
        }

        [Test]
        public void SetGetPixelCompareTestLinear()
        {
            foreach (SKColorType colorType in /*Enum<SKColorType>.GetValues()*/ new[] { SKColorType.Bgra8888 })
            {
                if (colorType == SKColorType.Unknown)
                    continue;

                foreach (SKAlphaType alphaType in /*Enum<SKAlphaType>.GetValues()*/  new[] { SKAlphaType.Premul, })
                {
                    if (alphaType == SKAlphaType.Unknown)
                        continue;
                    using var bitmap = new SKBitmap(new SKImageInfo(512, 256, colorType, alphaType, SKColorSpace.CreateSrgbLinear()));
                    if (bitmap.AlphaType != alphaType)
                        continue;

                    PixelFormatInfo info = bitmap.Info.GetInfo();
                    var testColor = Color.FromArgb(0x80, 0x80, 0xFF, 0x40).ToColor32();

                    // Skia and KGy SOFT converts colors to gray a bit differently so pre-converting the color to avoid differences
                    if (info.Grayscale)
                        testColor = testColor.ToGray();

                    // Pre-blending the color for opaque types because Skia handles alpha for them oddly:
                    // alpha is preserved while color is premultiplied, but when getting the pixel, the raw value is not converted back to straight color.
                    // Pre-blending also for types with discrete alpha because Skia ignores back color for them or uses an arbitrary transformation
                    if (!info.HasAlpha || colorType is SKColorType.Bgra1010102 or SKColorType.Rgba1010102 or SKColorType.Argb4444)
                        testColor = testColor.Blend(Color.Black);

                    bitmap.SetPixel(0, 0, testColor.ToSKColor());
                    
                    // As GetPixel does not work for linear color space (see https://github.com/mono/SkiaSharp/issues/2354) creating an sRGB copy for reading the pixel
                    Color32 expected;
                    using (var temp = new SKBitmap(bitmap.Info.WithColorSpace(SKColorSpace.CreateSrgb()).WithSize(1, 1)))
                    {
                        using var canvas = new SKCanvas(temp);
                        using var paint = new SKPaint { BlendMode = SKBlendMode.Src };
                        canvas.DrawBitmap(bitmap, new SKRect(0, 0, 1, 1), new SKRect(0, 0, 1, 1), paint);
                        expected = temp.GetPixel(0, 0).ToColor32();
                    }

                    Console.Write($"{colorType}/{alphaType}: {bitmap.Info.AlphaType} ===> {testColor} -> {expected} vs. ");

                    using IReadWriteBitmapData readWriteBitmapData = bitmap.GetReadWriteBitmapData(Color.Black.ToSKColor());
                    readWriteBitmapData.SetPixel(3, 4, testColor);
                    Color32 actual = readWriteBitmapData.GetPixel(3, 4).ToColor32();
                    Console.WriteLine(actual);

                    if (SaveToFile)
                    {
                        // saving an example by SkiaSharp
                        GenerateAlphaGradient(bitmap);
                        SaveBitmap($"{colorType}_{alphaType}_Skia", bitmap);

                        // saving an example by KGySoft
                        GenerateAlphaGradient(readWriteBitmapData);
                        SaveBitmap($"{colorType}_{alphaType}_KGy", bitmap);
                    }

                    try
                    {
                        Assert.IsTrue(expected.TolerantEquals(actual, 5), $"{colorType}/{alphaType} - SkiaSharp: {expected} vs. KGySoft: {actual}");
                    }
                    catch (AssertionException)
                    {
                        // To go on with the other tests. The test will fail anyway.
                    }
                }
            }
        }

        private static object[] ColorSpaceTestSource { get; } =
        {
            //new object[] { SKColorType.Bgra8888, SKAlphaType.Unpremul, SKColorSpace.CreateSrgb() }, // OK
            //new object[] { SKColorType.Bgra8888, SKAlphaType.Premul, SKColorSpace.CreateSrgb() },   // OK
            //new object[] { SKColorType.RgbaF16, SKAlphaType.Premul, SKColorSpace.CreateSrgb() },   // OK
            //new object[] { SKColorType.Bgra8888, SKAlphaType.Unpremul, SKColorSpace.CreateSrgbLinear() }, // Fail
            //new object[] { SKColorType.Bgra8888, SKAlphaType.Premul, SKColorSpace.CreateRgb(SKColorSpaceTransferFn.Srgb, SKColorSpaceXyz.AdobeRgb) }, // Fail
            //new object[] { SKColorType.Bgra8888, SKAlphaType.Premul, SKColorSpace.CreateRgb(SKColorSpaceTransferFn.Empty, SKColorSpaceXyz.Empty) }, // OK
            //new object[] { SKColorType.Bgra8888, SKAlphaType.Premul, SKColorSpace.CreateRgb(SKColorSpaceTransferFn.Empty, SKColorSpaceXyz.AdobeRgb) }, // Fail
            //new object[] { SKColorType.Bgra8888, SKAlphaType.Premul, SKColorSpace.CreateRgb(SKColorSpaceTransferFn.Srgb, SKColorSpaceXyz.Srgb) }, // OK
            //new object[] { SKColorType.Bgra8888, SKAlphaType.Premul, SKColorSpace.CreateRgb(SKColorSpaceTransferFn.Srgb, SKColorSpaceXyz.Empty) }, // OK
            new object[] { SKColorType.Bgra8888, SKAlphaType.Premul, SKColorSpace.CreateRgb(SKColorSpaceTransferFn.Linear, SKColorSpaceXyz.Srgb) },
            new object[] { SKColorType.Bgra8888, SKAlphaType.Premul, SKColorSpace.CreateRgb(SKColorSpaceTransferFn.Linear, SKColorSpaceXyz.Empty) },
            new object[] { SKColorType.Bgra8888, SKAlphaType.Premul, SKColorSpace.CreateRgb(SKColorSpaceTransferFn.Linear, SKColorSpaceXyz.AdobeRgb) },
        };

        [Explicit, TestCaseSource(nameof(ColorSpaceTestSource))]
        public void ColorSpaceTest(SKColorType colorType, SKAlphaType alphaType, SKColorSpace colorSpace)
        {
            Console.WriteLine($"{colorType}/{alphaType}/{colorSpace.NamedGamma} (sRGB: {colorSpace.IsSrgb}; Close to sRGB: {colorSpace.GammaIsCloseToSrgb}; Linear: {colorSpace.GammaIsLinear})");
            Console.WriteLine($"Tranform FN: {colorSpace.GetNumericalTransferFunction().Values.Join(" ")}");
            Assert.IsTrue(colorSpace.ToColorSpaceXyz(out var xyz));
            Console.WriteLine($"XYZ: {xyz.Values.Join(" ")}");
            Console.WriteLine($"Same as sRGB: {colorSpace == SKColorSpace.CreateSrgb()}");
            Console.WriteLine($"Same as Linear: {colorSpace == SKColorSpace.CreateSrgbLinear()}");
            var testColor = new SKColor(red: 128, green: 255, blue: 64, alpha: 128);
            using var bitmap = new SKBitmap(new SKImageInfo(1, 1, colorType, alphaType, colorSpace));
            bitmap.SetPixel(0, 0, testColor);
            Assert.AreEqual(testColor, bitmap.GetPixel(0, 0));
            //using var tempBitmap = new SKBitmap(new SKImageInfo(1, 1, SKColorType.Bgra8888, SKAlphaType.Unpremul, SKColorSpace.CreateSrgb()));
            //using (var canvas = new SKCanvas(tempBitmap))
            //    canvas.DrawBitmap(bitmap, SKPoint.Empty);
            //Assert.AreEqual(testColor, tempBitmap.GetPixel(0, 0));
        }

        #endregion
    }
}
