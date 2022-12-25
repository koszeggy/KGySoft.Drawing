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
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;

using KGySoft.CoreLibraries;
using KGySoft.Drawing.Imaging;

using NUnit.Framework;

using SkiaSharp;

#endregion

namespace KGySoft.Drawing.SkiaSharp.UnitTests
{
    [TestFixture]
    public class SKBitmapExtensionsTest
    {
        #region Fields

        private static readonly Color testColorAlpha = Color.FromArgb(0x80, 0x80, 0xFF, 0x40);
        private static readonly Color testColorBlended = testColorAlpha.ToColor32().Blend(Color.Black); // Color.FromArgb(0xFF, 0x40, 0x80, 0x20);

        private static readonly object[] sourceDirectlySupportedSetGetPixelTest =
        {
            new object[] { "Bgra8888/Unpremul", SKColorType.Bgra8888, SKAlphaType.Unpremul, testColorAlpha, testColorAlpha },
            new object[] { "Bgra8888/Premul", SKColorType.Bgra8888, SKAlphaType.Premul, testColorAlpha, testColorAlpha },

            //new object[] { "Alpha8" },
            //new object[] { "Rgb565" },
            //new object[] { "Argb4444" },
            //new object[] { "Rgba8888" },
            //new object[] { "Rgb888x" },
            //new object[] { "Rgba1010102" },
            //new object[] { "Rgb101010x" },
            //new object[] { "Gray8" },
            //new object[] { "RgbaF16" },
            //new object[] { "RgbaF16Clamped" },
            //new object[] { "RgbaF32" },
            //new object[] { "Rg88" },
            //new object[] { "AlphaF16" },
            //new object[] { "RgF16" },
            //new object[] { "Alpha16" },
            //new object[] { "Rg1616" },
            //new object[] { "Rgba16161616" },
            //new object[] { "Bgra1010102" },
            //new object[] { "Bgr101010x" },
        };

        #endregion

        #region Methods

        [TestCaseSource(nameof(sourceDirectlySupportedSetGetPixelTest))]
        public void DirectlySupportedSetGetPixelTest(string testName, SKColorType colorType, SKAlphaType alphaType, Color testColor, Color expectedResult)
        {
            var info = new SKImageInfo(2, 2, colorType, alphaType);
            Assert.IsTrue(info.IsDirectlySupported(), $"Format is not supported directly: {colorType}/{alphaType}");

            Console.WriteLine($"{testName,-32}- {testColor.ToColor32()} ->");
            using var bitmap = new SKBitmap(info, info.RowBytes);
            Assert.AreEqual(colorType, bitmap.ColorType);
            Assert.AreEqual(alphaType, bitmap.AlphaType);
            
            bitmap.SetPixel(0, 0, testColor.ToSKColor());
            var actualNative = bitmap.GetPixel(0, 0);

            using var bitmapData = bitmap.GetReadWriteBitmapData();
            bitmapData.SetPixel(1, 1, testColor);
            var actual = bitmapData.GetPixel(1, 1);
            
            var raw = new List<byte>();
            for (int i = 0; i < colorType.GetBytesPerPixel(); i++)
                raw.Insert(0, bitmapData[0].ReadRaw<byte>(i));
            Console.WriteLine($"{"by SkiaSharp",-32}- {actualNative.ToColor32()} ({raw.Select(b => Convert.ToString(b, 2).PadLeft(8, '0')).Join('_')}) {(expectedResult.ToColor32().TolerantEquals(actualNative.ToColor32(), 1) ? "OK" : "!")}");

            raw.Clear();
            int offset = info.BytesPerPixel;
            for (int i = 0; i < colorType.GetBytesPerPixel(); i++)
                raw.Insert(0, bitmapData[1].ReadRaw<byte>(i + offset));
            Console.WriteLine($"{"by KGySoft",-32}- {actual.ToColor32()} ({raw.Select(b => Convert.ToString(b, 2).PadLeft(8, '0')).Join('_')}) {(expectedResult.ToColor32().TolerantEquals(actual.ToColor32(), 1) ? "OK" : "!")}");

            Assert.IsTrue(expectedResult.ToColor32().TolerantEquals(actual.ToColor32(), 1), $"KGySoft: {expectedResult.ToColor32()} vs. {actual.ToColor32()}");
            Assert.IsTrue(expectedResult.ToColor32().TolerantEquals(actualNative.ToColor32(), 1), $"SkiaSharp: {expectedResult.ToColor32()} vs. {actualNative.ToColor32()}");
        }

        [Test]
        public void SetGetPixelTestAllTest()
        {
            var testColor = testColorAlpha;
            //foreach (SKColorType colorType in Enum<SKColorType>.GetValues())
            var colorType = SKColorType.Bgra8888;
            {
                //if (colorType == SKColorType.Unknown)
                //    continue;

                foreach (SKAlphaType alphaType in Enum<SKAlphaType>.GetValues())
                {
                    if (alphaType == SKAlphaType.Unknown)
                        continue;

                    using var bitmap = new SKBitmap(new SKImageInfo(10, 10, colorType, alphaType));
                    bitmap.SetPixel(2, 3, new Color32(testColor).ToSKColor());
                    Color32 expected = bitmap.GetPixel(2, 3).ToColor32();
                    Console.Write($"{colorType}/{alphaType}: {bitmap.Info.AlphaType} ===> {testColor.ToColor32()} -> {expected} vs. ");

                    using IReadWriteBitmapData readWriteBitmapData = bitmap.GetReadWriteBitmapData();
                    readWriteBitmapData.SetPixel(3, 4, testColor);
                    Color32 actual = readWriteBitmapData.GetPixel(3, 4).ToColor32();
                    Console.WriteLine(actual);
                    Assert.IsTrue(expected.TolerantEquals(actual, 1), $"{colorType}/{alphaType} - SkiaSharp: {expected} vs. KGySoft: {actual}");
                }
            }
        }

        #endregion
    }
}
