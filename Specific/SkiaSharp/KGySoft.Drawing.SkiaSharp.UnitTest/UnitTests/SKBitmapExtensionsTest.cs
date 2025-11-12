#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: SKBitmapExtensionsTest.cs
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
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Linq.Expressions;

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

        [TestCase(SKColorType.Bgr101010xXR, SKAlphaType.Opaque)]

        [TestCase(SKColorType.Srgba8888, SKAlphaType.Unpremul)]
        [TestCase(SKColorType.Srgba8888, SKAlphaType.Premul)]
        [TestCase(SKColorType.Srgba8888, SKAlphaType.Opaque)]

        [TestCase(SKColorType.R8Unorm, SKAlphaType.Opaque)]

        [TestCase(SKColorType.Rgba10x6, SKAlphaType.Unpremul)]
        [TestCase(SKColorType.Rgba10x6, SKAlphaType.Premul)]
        [TestCase(SKColorType.Rgba10x6, SKAlphaType.Opaque)]
        public void DirectlySupportedSetGetPixelTest(SKColorType colorType, SKAlphaType alphaType)
        {
            #region Local Methods

            static void AssertEqual(string name, Color32 expected, Color32 actual, byte tolerance)
            {
                Console.WriteLine($"{name,-32}- {expected} vs. {actual}");
                Assert.IsTrue(expected.TolerantEquals(actual, tolerance), $"{name}: {expected} vs. {actual}");
            }

            #endregion

            foreach (var colorSpace in new[] { SKColorSpace.CreateSrgb(), SKColorSpace.CreateSrgbLinear() })
            {
                var info = new SKImageInfo(2, 2, colorType, alphaType, colorSpace);
                var testColor = Color.FromArgb(0x80, 0x80, 0xFF, 0x40).ToColor32();

                // Skia and KGy SOFT convert colors to gray a bit differently so pre-converting the color to avoid such differences
                if (info.GetInfo().Grayscale)
                    testColor = testColor.ToGray();
                bool linear = colorSpace.GammaIsLinear;
                Color32 expectedResult = testColor;
                if (colorType is SKColorType.Alpha8 or SKColorType.Alpha16 or SKColorType.AlphaF16)
                    expectedResult = Color32.FromArgb(expectedResult.A, Color.Black);
                else if (alphaType == SKAlphaType.Opaque)
                    expectedResult = expectedResult.Blend(Color.Black, linear ? WorkingColorSpace.Linear : WorkingColorSpace.Srgb);

                expectedResult = colorType switch
                {
                    SKColorType.Bgra1010102 or SKColorType.Rgba1010102 => new ColorBgra1010102Srgb(expectedResult).ToColor32(),
                    SKColorType.Rg88 or SKColorType.Rg1616 or SKColorType.RgF16 => new Color32(expectedResult.R, expectedResult.G, 0),
                    SKColorType.R8Unorm => new Color32(expectedResult.R, 0, 0),
                    _ => expectedResult
                };

                Assert.IsTrue(info.IsDirectlySupported(), $"Format is not supported directly: {colorType}/{alphaType}/{(colorSpace.GammaIsLinear ? nameof(WorkingColorSpace.Linear) : nameof(WorkingColorSpace.Srgb))}");
                Console.WriteLine($"{$"{colorType}/{alphaType}/{(colorSpace.GammaIsLinear ? nameof(WorkingColorSpace.Linear) : nameof(WorkingColorSpace.Srgb))}",-32}- {testColor}");
                Console.WriteLine($"{"Expected result",-32}- {expectedResult}");
                using var bitmap = new SKBitmap(info, info.RowBytes);
                Assert.AreEqual(colorType, bitmap.ColorType);
                Assert.AreEqual(alphaType, bitmap.AlphaType);

                bitmap.SetPixel(0, 0, testColor.ToSKColor());
                SKColor actualNative = bitmap.GetPixel(0, 0);

                using IReadWriteBitmapData bitmapData = bitmap.GetReadWriteBitmapData();
                var raw = new List<byte>();
                for (int i = 0; i < colorType.GetBytesPerPixel(); i++)
                    raw.Insert(0, bitmapData.ReadRaw<byte>(i, 0));

                byte tolerance = (byte)(colorSpace.IsSrgb
                    ? colorType switch { SKColorType.Argb4444 => 17, SKColorType.Rgb565 => 5, _ => 1 } // allowing 1 shade difference
                    : colorType switch
                    {
                        SKColorType.Rgb565 => 45, // for 5 bit linear colors the first non-black shade is 48
                        SKColorType.Argb4444 => 64, // for 4 bit it's 70
#if !NETCOREAPP3_0_OR_GREATER
                        SKColorType.Rgba8888 or SKColorType.Bgra8888 => 3,
#endif
                        _ => 2
                    });
                Console.WriteLine($"{"by SkiaSharp",-32}- {actualNative.ToColor32(),-40} ({raw.Select(b => Convert.ToString(b, 2).PadLeft(8, '0')).Join('_')}) {(expectedResult.TolerantEquals(actualNative.ToColor32(), tolerance) ? "OK" : "!")}");

                bitmapData.SetPixel(1, 1, testColor);
                Color actual = bitmapData.GetPixel(1, 1);

                raw.Clear();
                int offset = info.BytesPerPixel;
                for (int i = 0; i < colorType.GetBytesPerPixel(); i++)
                    raw.Insert(0, bitmapData.ReadRaw<byte>(i + offset, 1));
                Console.WriteLine($"{"by KGySoft",-32}- {actual.ToColor32(),-40} ({raw.Select(b => Convert.ToString(b, 2).PadLeft(8, '0')).Join('_')}) {(expectedResult.TolerantEquals(actual.ToColor32(), tolerance) ? "OK" : "!")}");
                Console.WriteLine();

                // not comparing Skia result to KGySoft because they can be different and this is known:
                // - if colorType supports alpha but alphaType is Opaque, Skia uses premultiplied pixel write and gets the raw value
                // - For non-sRGB color spaces GetPixel does not work: https://github.com/mono/SkiaSharp/issues/2354

                AssertEqual("by testColor", expectedResult, actual.ToColor32(), tolerance);

                // Setting/getting all color types, comparing result to the actual Color result
                expectedResult = actual.ToColor32();
#if NETCOREAPP3_0_OR_GREATER // Keeping the original tolerance if a shade difference can occur due to non-accelerated truncating conversions (especially for premultiplied colors)
                tolerance = alphaType is SKAlphaType.Opaque
                    || alphaType is SKAlphaType.Premul or SKAlphaType.Unpremul && colorType is SKColorType.Bgra1010102 or SKColorType.Rgba1010102 or SKColorType.Rgba10x6
                    || alphaType is SKAlphaType.Premul && colorType is SKColorType.Srgba8888
                        ? (byte)1
                        : (byte)0;
#endif

                // as Color32
                bitmapData.SetColor32(0, 0, testColor);
                AssertEqual(nameof(Color32), expectedResult, bitmapData.GetColor32(0, 0), tolerance);

                // as PColor32
                bitmapData.SetPColor32(0, 0, testColor.ToPColor32());
#if NETCOREAPP3_0_OR_GREATER
                AssertEqual(nameof(PColor32), expectedResult.ToPColor32().ToColor32(), bitmapData.GetPColor32(0, 0).ToColor32(), tolerance);
#else
                AssertEqual(nameof(PColor32), expectedResult.ToPColor32().ToColor32(), bitmapData.GetPColor32(0, 0).ToColor32(), (byte)(tolerance * 2));
#endif

                // as Color64
                bitmapData.SetColor64(0, 0, testColor.ToColor64());
                AssertEqual(nameof(Color64), expectedResult.ToColor64().ToColor32(), bitmapData.GetColor64(0, 0).ToColor32(), tolerance);

                // as PColor64
                bitmapData.SetPColor64(0, 0, testColor.ToPColor64());
                AssertEqual(nameof(PColor64), expectedResult.ToPColor64().ToColor32(), bitmapData.GetPColor64(0, 0).ToColor32(), tolerance);

                // as ColorF
                bitmapData.SetColorF(0, 0, testColor.ToColorF());
                AssertEqual(nameof(ColorF), expectedResult.ToColorF().ToColor32(), bitmapData.GetColorF(0, 0).ToColor32(), tolerance);

                // as PColorF
                bitmapData.SetPColorF(0, 0, testColor.ToPColorF());
                AssertEqual(nameof(PColorF), expectedResult.ToPColorF().ToColor32(), bitmapData.GetPColorF(0, 0).ToColor32(), tolerance);
                Console.WriteLine();
            }
        }

        [Test]
        public void SetGetPixelCompareTestSrgb()
        {
            string? fileName = null;
            //string? fileName = @"..\..\..\..\..\..\Help\Images\Information256.png";
            //string? fileName = @"..\..\..\..\..\..\Help\Images\GrayShades.gif";
            //string? fileName = @"D:\Dokumentumok\Képek\Formats\APNG-balls.png";
            //string? fileName = @"D:\Dokumentumok\Képek\Formats\webp_losless.webp";
            if (!File.Exists(fileName))
                fileName = null;
            SKSizeI size = fileName == null ? new SKSizeI(512, 256) : SKBitmap.DecodeBounds(fileName).Size;

            foreach (SKColorType colorType in Enum<SKColorType>.GetValues() /*new[] { SKColorType.Rgba10x6 }*/)
            {
                if (colorType == SKColorType.Unknown)
                    continue;

                foreach (SKAlphaType alphaType in Enum<SKAlphaType>.GetValues() /*new[] { SKAlphaType.Opaque }*/)
                {
                    if (alphaType == SKAlphaType.Unknown)
                        continue;
                    using var bitmap = new SKBitmap(new SKImageInfo(size.Width, size.Height, colorType, alphaType));
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

                        // Pre-blending the color for opaque types because Skia handles alpha of such types oddly:
                        // alpha is preserved while color is premultiplied, but when getting the pixel, the raw value is not converted back to straight color.
                        // Pre-blending also for types with discrete alpha because Skia uses an arbitrary transformation
                        if (!info.HasAlpha)
                            testColor = testColor.Blend(Color.Black, WorkingColorSpace.Srgb);

                        bitmap.SetPixel(2, 3, testColor.ToSKColor());
                        Color32 expected = bitmap.GetPixel(2, 3).ToColor32();
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
                                if (fileName == null)
                                    GenerateAlphaGradient(bitmap);
                                else
                                    LoadInto(bitmap, fileName);

                                SaveBitmap($"{colorType}_{alphaType}_Skia", bitmap);
                                skiaSaved = true;
                            }

                            // saving an example by KGySoft
                            if (fileName == null)
                                GenerateAlphaGradient(readWriteBitmapData);
                            else
                                LoadInto(readWriteBitmapData, fileName);
                            SaveBitmap($"{colorType}_{alphaType}_KGy_{workingColorSpace}", bitmap);
                        }

                        byte tolerance = colorType switch
                        {
                            SKColorType.Argb4444 => 17, // the increment of one shade in 8 bits (255/15)
                            SKColorType.Bgra1010102 or SKColorType.Rgba1010102 => 85, // 255/3 due to A channel
                            SKColorType.Bgr101010xXR => 255, // SkiaSharp bug (3.116.1/119.0): SKBitmap.GetPixel returns default color if the color type is Bgr101010xXR // TODO: check in newer versions
                            SKColorType.Rgba10x6 when alphaType is SKAlphaType.Premul => 128, // SkiaSharp bug (3.119.0): SKBitmap.GetPixel returns the premultiplied RGB colors as if the result was SKPMColor rather than SKColor if color type is Rgba10x6 and alpha type is Premul // TODO: check in newer versions
                            _ => 1
                        };

                        try
                        {
                            Assert.IsTrue(expected.TolerantEquals(actual, tolerance), $"{colorType}/{alphaType}/{workingColorSpace} - SkiaSharp: {expected} vs. KGySoft: {actual}");
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
            string? fileName = null;
            //string? fileName = @"..\..\..\..\..\..\Help\Images\Information256.png";
            //string? fileName = @"..\..\..\..\..\..\Help\Images\GrayShades.gif";
            //string? fileName = @"D:\Dokumentumok\Képek\Formats\APNG-balls.png";
            //string? fileName = @"D:\Dokumentumok\Képek\Formats\webp_losless.webp";
            if (!File.Exists(fileName))
                fileName = null;
            SKSizeI size = fileName == null ? new SKSizeI(512, 256) : SKBitmap.DecodeBounds(fileName).Size;

            foreach (SKColorType colorType in Enum<SKColorType>.GetValues() /*new[] { SKColorType.Rgba16161616 }*/)
            {
                if (colorType == SKColorType.Unknown)
                    continue;

                foreach (SKAlphaType alphaType in Enum<SKAlphaType>.GetValues() /*new[] { SKAlphaType.Unpremul }*/)
                {
                    if (alphaType == SKAlphaType.Unknown)
                        continue;
                    using var bitmap = new SKBitmap(new SKImageInfo(size.Width, size.Height, colorType, alphaType, SKColorSpace.CreateSrgbLinear()));
                    if (bitmap.AlphaType != alphaType)
                        continue;

                    bool skiaSaved = false;
                    foreach (WorkingColorSpace workingColorSpace in new[] { WorkingColorSpace.Linear, WorkingColorSpace.Srgb })
                    {
                        PixelFormatInfo info = bitmap.Info.GetInfo();
                        var testColor = Color.FromArgb(0x80, 0x80, 0xFF, 0x40).ToColor32();

                        // Skia and KGy SOFT converts colors to gray a bit differently so pre-converting the color to avoid differences
                        if (info.Grayscale)
                            testColor = testColor.ToGray();

                        // Pre-blending the color for opaque types because Skia handles alpha of such types oddly:
                        // alpha is preserved while color is premultiplied, but when getting the pixel, the raw value is not converted back to straight color.
                        // Pre-blending also for types with discrete alpha because Skia uses an arbitrary transformation
                        if (!info.HasAlpha)
                            testColor = testColor.Blend(Color.Black, WorkingColorSpace.Linear);

                        bitmap.SetPixel(0, 0, testColor.ToSKColor());

                        // As GetPixel does not work for linear color space (see https://github.com/mono/SkiaSharp/issues/2354) creating an sRGB copy for reading the pixel
                        Color32 expected;
                        using (var temp = new SKBitmap(bitmap.Info.WithColorSpace(SKColorSpace.CreateSrgb()).WithSize(1, 1)))
                        {
                            using var canvas = new SKCanvas(temp);
                            canvas.DrawBitmap(bitmap, new SKRect(0, 0, 1, 1), new SKRect(0, 0, 1, 1), SKBitmapExtensions.CopySourcePaint);
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
                                if (fileName == null)
                                    GenerateAlphaGradient(bitmap);
                                else
                                    LoadInto(bitmap, fileName);

                                SaveBitmap($"{colorType}_{alphaType}_Skia", bitmap);
                                skiaSaved = true;
                            }

                            // saving an example by KGySoft
                            if (fileName == null)
                                GenerateAlphaGradient(readWriteBitmapData);
                            else
                                LoadInto(readWriteBitmapData, fileName);
                            SaveBitmap($"{colorType}_{alphaType}_KGy_{workingColorSpace}", bitmap);
                        }

                        byte tolerance = colorType switch
                        {
                            SKColorType.Rgb565 => 49, // for 5 bit linear colors the first non-black shade is 49
                            SKColorType.Argb4444 => 68, // for 4 bit linear colors the first non-black shade is 68
                            SKColorType.Bgra1010102 or SKColorType.Rgba1010102 => 85, // 255/3 due to A channel
                            SKColorType.Bgr101010xXR => 255, // SkiaSharp bug (3.116.1): SKBitmap.GetPixel returns default color if the color type is Bgr101010xXR // TODO: check in newer versions
                            SKColorType.Rgba10x6 when alphaType is SKAlphaType.Premul => 128, // SkiaSharp bug (3.119.0): SKBitmap.GetPixel returns the premultiplied RGB colors as if the result was SKPMColor rather than SKColor if color type is Rgba10x6 and alpha type is Premul // TODO: check in newer versions
                            _ => 2
                        };

                        try
                        {
                            Assert.IsTrue(expected.TolerantEquals(actual, tolerance), $"{colorType}/{alphaType}/{workingColorSpace} - SkiaSharp: {expected} vs. KGySoft: {actual}");
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
        public void ConvertPixelFormatDirectTest()
        {
            var size = new SKSizeI(512, 256);
            var info = new SKImageInfo(size.Width, size.Height);

            foreach (SKColorType colorType in Enum<SKColorType>.GetValues() /*new[] { SKColorType.Rgba10x6 }*/)
            foreach (var colorSpace in new[] { SKColorSpace.CreateSrgb(), SKColorSpace.CreateSrgbLinear() })
            {
                if (colorType == SKColorType.Unknown)
                    continue;

                using var bitmap = new SKBitmap(info.WithColorSpace(colorSpace));
                GenerateAlphaGradient(bitmap);
                bool linear = colorSpace.GammaIsLinear;

                // by KGySoft
                using var result = bitmap.ConvertPixelFormat(colorType/*, backColor: SKColors.Green*/);
                Assert.AreEqual(colorType, result.ColorType);
                SaveBitmap($"{colorType}_KGy_{(linear ? "Linear" : "Srgb")}", result);

                // by SkiaSharp
                using var resultSkia = new SKBitmap(bitmap.Info.WithColorType(colorType));
                using var canvas = new SKCanvas(resultSkia);
                canvas.DrawBitmap(bitmap, SKPoint.Empty);
                Assert.AreEqual(colorType, resultSkia.ColorType);
                SaveBitmap($"{colorType}_Skia_{(linear ? "Linear" : "Srgb")}", resultSkia);
            }

            if (!SaveToFile)
                Assert.Inconclusive("This test is mainly for visual inspection");
        }

        [Test]
        public void ConvertPixelFormatWithDitheringTest()
        {
            var size = new SKSizeI(512, 256);
            using var bitmap = new SKBitmap(new SKImageInfo(size.Width, size.Height));
            GenerateAlphaGradient(bitmap);

            // Srgba8888: Makes difference only for bigger image width (e.g. 2048)
            foreach (SKColorType colorType in new[]{ SKColorType.Argb4444, SKColorType.Rgb565, SKColorType.Rg88, SKColorType.Rgba8888, SKColorType.Srgba8888, SKColorType.R8Unorm })
            {
                if (colorType == SKColorType.Unknown)
                    continue;

                foreach (var colorSpace in new[] { WorkingColorSpace.Srgb, WorkingColorSpace.Linear })
                {
                    using var result = bitmap.ConvertPixelFormat(null, ditherer: ErrorDiffusionDitherer.FloydSteinberg/*OrderedDitherer.BlueNoise*/, colorType, targetColorSpace: colorSpace);
                    Assert.AreEqual(colorType, result.ColorType);
                    SaveBitmap($"{colorType} {colorSpace}", result);
                }
            }

            if (!SaveToFile)
                Assert.Inconclusive("This test is mainly for visual inspection");
        }

        [Test]
        public void EmptyBitmapTest()
        {
            using var bitmap = new SKBitmap();
            Assert.Throws<ArgumentException>(() => bitmap.GetReadableBitmapData());
        }

        #endregion
    }
}
