#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: SKBitmapExtensionsTest.cs
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
        #region Methods

        [TestCase(SKColorType.Bgra8888, SKAlphaType.Unpremul)]
        [TestCase(SKColorType.Bgra8888, SKAlphaType.Premul)]
        [TestCase(SKColorType.Bgra8888, SKAlphaType.Opaque)]

        [TestCase(SKColorType.Rgba8888, SKAlphaType.Unpremul)]
        [TestCase(SKColorType.Rgba8888, SKAlphaType.Premul)]
        [TestCase(SKColorType.Rgba8888, SKAlphaType.Opaque)]

        [TestCase(SKColorType.Rgb888x, SKAlphaType.Opaque)]

        [TestCase(SKColorType.Gray8, SKAlphaType.Opaque)]

        [TestCase(SKColorType.Rgb565, SKAlphaType.Opaque)]

        [TestCase(SKColorType.Rgba16161616, SKAlphaType.Unpremul)]
        [TestCase(SKColorType.Rgba16161616, SKAlphaType.Premul)]
        [TestCase(SKColorType.Rgba16161616, SKAlphaType.Opaque)]

        [TestCase(SKColorType.Bgra1010102, SKAlphaType.Unpremul)]
        [TestCase(SKColorType.Bgra1010102, SKAlphaType.Premul)]
        [TestCase(SKColorType.Bgra1010102, SKAlphaType.Opaque)]

        [TestCase(SKColorType.Bgr101010x, SKAlphaType.Opaque)]

        [TestCase(SKColorType.Rgba1010102, SKAlphaType.Unpremul)]
        [TestCase(SKColorType.Rgba1010102, SKAlphaType.Premul)]
        [TestCase(SKColorType.Rgba1010102, SKAlphaType.Opaque)]

        [TestCase(SKColorType.Rgb101010x, SKAlphaType.Opaque)]

        [TestCase(SKColorType.Argb4444, SKAlphaType.Unpremul)]
        [TestCase(SKColorType.Argb4444, SKAlphaType.Premul)]
        [TestCase(SKColorType.Argb4444, SKAlphaType.Opaque)]

        [TestCase(SKColorType.RgbaF32, SKAlphaType.Unpremul)]
        [TestCase(SKColorType.RgbaF32, SKAlphaType.Premul)]
        [TestCase(SKColorType.RgbaF32, SKAlphaType.Opaque)]

        [TestCase(SKColorType.RgbaF16, SKAlphaType.Unpremul)]
        [TestCase(SKColorType.RgbaF16, SKAlphaType.Premul)]
        [TestCase(SKColorType.RgbaF16, SKAlphaType.Opaque)]
        [TestCase(SKColorType.RgbaF16Clamped, SKAlphaType.Unpremul)]
        [TestCase(SKColorType.RgbaF16Clamped, SKAlphaType.Premul)]
        [TestCase(SKColorType.RgbaF16Clamped, SKAlphaType.Opaque)]

        //[TestCase(SKColorType.Alpha8, SKAlphaType.Unpremul)]
        [TestCase(SKColorType.Alpha8, SKAlphaType.Premul)]
        [TestCase(SKColorType.Alpha8, SKAlphaType.Opaque)]

        //[TestCase(SKColorType.Alpha16, SKAlphaType.Unpremul)]
        [TestCase(SKColorType.Alpha16, SKAlphaType.Premul)]
        [TestCase(SKColorType.Alpha16, SKAlphaType.Opaque)]

        //[TestCase(SKColorType.AlphaF16, SKAlphaType.Unpremul)]
        [TestCase(SKColorType.AlphaF16, SKAlphaType.Premul)]
        [TestCase(SKColorType.AlphaF16, SKAlphaType.Opaque)]

        [TestCase(SKColorType.Rg88, SKAlphaType.Opaque)]
        [TestCase(SKColorType.Rg1616, SKAlphaType.Opaque)]
        [TestCase(SKColorType.RgF16, SKAlphaType.Opaque)]
        public void DirectlySupportedSetGetPixelTest(SKColorType colorType, SKAlphaType alphaType)
        {
            foreach (var colorSpace in new[] { SKColorSpace.CreateSrgb(), /*TODO SKColorSpace.CreateSrgbLinear(),*/ })
            {
                var info = new SKImageInfo(2, 2, colorType, alphaType, colorSpace);
                var testColor = Color.FromArgb(0x80, 0x80, 0xFF, 0x40).ToColor32();

                // Skia and KGy SOFT convert colors to gray a bit differently so pre-converting the color to avoid such differences
                if (info.GetInfo().Grayscale)
                    testColor = testColor.ToGray();
                bool linear = colorSpace.GammaIsLinear;
                var expectedResult = testColor;
                if (colorType is SKColorType.Alpha8 or SKColorType.Alpha16 or SKColorType.AlphaF16)
                    expectedResult = Color32.FromArgb(expectedResult.A, Color.Black);
                else if (alphaType == SKAlphaType.Opaque)
                    expectedResult = expectedResult.Blend(Color.Black, linear ? WorkingColorSpace.Linear : WorkingColorSpace.Srgb);

                if (colorType is SKColorType.Bgra1010102 or SKColorType.Rgba1010102)
                    expectedResult = new ColorBgra1010102Srgb(expectedResult).ToColor32();
                else if (colorType is SKColorType.Rg88 or SKColorType.Rg1616 or SKColorType.RgF16)
                    expectedResult = new Color32(expectedResult.R, expectedResult.G, 0);

                Assert.IsTrue(info.IsDirectlySupported(), $"Format is not supported directly: {colorType}/{alphaType}/{colorSpace.NamedGamma}");

                Console.WriteLine($"{$"{colorType}/{alphaType}/{colorSpace.NamedGamma}",-32}- {testColor}");
                Console.WriteLine($"{"Expected result",-32}- {expectedResult}");
                using var bitmap = new SKBitmap(info, info.RowBytes);
                Assert.AreEqual(colorType, bitmap.ColorType);
                Assert.AreEqual(alphaType, bitmap.AlphaType);

                bitmap.SetPixel(0, 0, testColor.ToSKColor());
                var actualNative = bitmap.GetPixel(0, 0);

                using var bitmapData = bitmap.GetReadWriteBitmapData();
                bitmapData.SetPixel(1, 1, testColor);
                var actual = bitmapData.GetPixel(1, 1);
                byte tolerance = (byte)(colorSpace.IsSrgb
                    ? colorType switch { SKColorType.Argb4444 => 17, SKColorType.Rgb565 => 5, _ => 1 } // allowing 1 shade difference
                    : 2); // TODO: Rgb565: + 2?

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
                // - if colorType supports alpha but alphaType is Opaque, Skia uses premultiplied pixel write and gets the raw value
                // - For non-sRGB color spaces GetPixel does not work: https://github.com/mono/SkiaSharp/issues/2354

                Assert.IsTrue(expectedResult.TolerantEquals(actual.ToColor32(), tolerance), $"KGySoft: {expectedResult} vs. {actual.ToColor32()}");
            }
        }

        [Test]
        public void SetGetPixelCompareTestSrgb()
        {
            foreach (SKColorType colorType in Enum<SKColorType>.GetValues() /*new[] { SKColorType.RgF16 }*/)
            {
                if (colorType == SKColorType.Unknown)
                    continue;

                foreach (SKAlphaType alphaType in Enum<SKAlphaType>.GetValues() /*new[] { SKAlphaType.Opaque }*/)
                {
                    if (alphaType == SKAlphaType.Unknown)
                        continue;
                    using var bitmap = new SKBitmap(new SKImageInfo(512, 256, colorType, alphaType));
                    if (bitmap.AlphaType != alphaType)
                        continue;

                    bool skiaSaved = false;
                    foreach (WorkingColorSpace workingColorSpace in new[] { WorkingColorSpace.Srgb, WorkingColorSpace.Linear })
                    {
                        PixelFormatInfo info = bitmap.Info.GetInfo();
                        var testColor = Color.FromArgb(0x80, 0x80, 0xFF, 0x40).ToColor32();

                        // Skia and KGy SOFT converts colors to gray a bit differently so pre-converting the color to avoid differences
                        if (info.Grayscale)
                            testColor = testColor.ToGray();

                        // Pre-blending the color for opaque types because Skia handles alpha for them oddly:
                        // alpha is preserved while color is premultiplied, but when getting the pixel, the raw value is not converted back to straight color.
                        // Pre-blending also for types with discrete alpha because Skia ignores back color for them or uses an arbitrary transformation
                        if (!info.HasAlpha || colorType is SKColorType.Bgra1010102 or SKColorType.Rgba1010102 or SKColorType.Argb4444)
                            testColor = testColor.Blend(Color.Black, WorkingColorSpace.Srgb);

                        bitmap.SetPixel(2, 3, testColor.ToSKColor());
                        Color32 expected = bitmap.GetPixel(2, 3).ToColor32();
                        Console.Write($"{colorType}/{alphaType}/{workingColorSpace}: {bitmap.Info.AlphaType} ===> {testColor} -> {expected} vs. ");

                        using IReadWriteBitmapData readWriteBitmapData = bitmap.GetReadWriteBitmapData(workingColorSpace, Color.Black.ToSKColor());
                        readWriteBitmapData.SetPixel(3, 4, testColor);
                        Color32 actual = readWriteBitmapData.GetPixel(3, 4).ToColor32();
                        Console.WriteLine(actual);

                        if (SaveToFile)
                        {
                            if (!skiaSaved)
                            {
                                // saving an example by SkiaSharp, only once because it has no different working color spaces
                                GenerateAlphaGradient(bitmap);
                                SaveBitmap($"{colorType}_{alphaType}_Skia", bitmap);
                                skiaSaved = true;
                            }

                            // saving an example by KGySoft
                            GenerateAlphaGradient(readWriteBitmapData);
                            SaveBitmap($"{colorType}_{alphaType}_KGy_{workingColorSpace}", bitmap);
                        }

                        try
                        {
                            Assert.IsTrue(expected.TolerantEquals(actual, 1), $"{colorType}/{alphaType}/{workingColorSpace} - SkiaSharp: {expected} vs. KGySoft: {actual}");
                        }
                        catch (AssertionException)
                        {
                            // To go on with the other tests. The test will fail anyway.
                        }
                    }
                }
            }
        }

        [Test]
        public void SetGetPixelCompareTestLinear()
        {
            foreach (SKColorType colorType in /*Enum<SKColorType>.GetValues()*/ new[] { SKColorType.Gray8 })
            {
                if (colorType == SKColorType.Unknown)
                    continue;

                foreach (SKAlphaType alphaType in/* Enum<SKAlphaType>.GetValues()*/  new[] { SKAlphaType.Opaque, })
                {
                    if (alphaType == SKAlphaType.Unknown)
                        continue;
                    using var bitmap = new SKBitmap(new SKImageInfo(512, 256, colorType, alphaType, SKColorSpace.CreateSrgbLinear()));
                    if (bitmap.AlphaType != alphaType)
                        continue;

                    bool skiaSaved = false;
                    foreach (WorkingColorSpace workingColorSpace in new[] { WorkingColorSpace.Srgb, WorkingColorSpace.Linear })
                    {
                        PixelFormatInfo info = bitmap.Info.GetInfo();
                        var testColor = Color.FromArgb(0x80, 0x80, 0xFF, 0x40).ToColor32();

                        // Skia and KGy SOFT converts colors to gray a bit differently so pre-converting the color to avoid differences
                        if (info.Grayscale)
                            testColor = testColor.ToGray();

                        // Pre-blending the color for opaque types because Skia handles alpha for them oddly:
                        // alpha is preserved while color is premultiplied, but when getting the pixel, the raw value is not converted back to straight color.
                        // Pre-blending also for types with discrete alpha because Skia ignores back color for them or uses an arbitrary transformation
                        if (!info.HasAlpha || colorType is SKColorType.Bgra1010102 or SKColorType.Rgba1010102 or SKColorType.Argb4444)
                            testColor = testColor.Blend(Color.Black, WorkingColorSpace.Linear);

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

                        Console.Write($"{colorType}/{alphaType}/{workingColorSpace}: {bitmap.Info.AlphaType} ===> {testColor} -> Skia vs. KGySoft: {expected} vs. ");

                        using IReadWriteBitmapData readWriteBitmapData = bitmap.GetReadWriteBitmapData(workingColorSpace, Color.Black.ToSKColor());
                        readWriteBitmapData.SetPixel(3, 4, testColor);
                        Color32 actual = readWriteBitmapData.GetPixel(3, 4).ToColor32();
                        Console.WriteLine(actual);

                        if (SaveToFile)
                        {
                            if (!skiaSaved)
                            {
                                // saving an example by SkiaSharp, only once because it has no different working color spaces
                                GenerateAlphaGradient(bitmap);
                                SaveBitmap($"{colorType}_{alphaType}_Skia", bitmap);
                                skiaSaved = true;
                            }

                            // saving an example by KGySoft
                            GenerateAlphaGradient(readWriteBitmapData);
                            SaveBitmap($"{colorType}_{alphaType}_KGy_{workingColorSpace}", bitmap);
                        }

                        try
                        {
                            Assert.IsTrue(expected.TolerantEquals(actual, (byte)(/*alphaType is SKAlphaType.Premul ? 5 :*/ 1)), $"{colorType}/{alphaType}/{workingColorSpace} - SkiaSharp: {expected} vs. KGySoft: {actual}");
                        }
                        catch (AssertionException)
                        {
                            // To go on with the other tests. The test will fail anyway.
                        } 
                    }
                }
            }
        }

        #endregion
    }
}
