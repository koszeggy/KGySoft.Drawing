#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ReadableBitmapDataExtensionsTest.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2025 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#nullable enable

#region Usings

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using Windows.UI.Xaml.Media.Imaging;

using KGySoft.Drawing.Imaging;

using NUnit.Framework;

#endregion

namespace KGySoft.Drawing.Uwp.UnitTest
{
    [TestFixture]
    public class ReadableBitmapDataExtensionsTest : TestBase
    {
        #region Fields

        private static readonly object?[][] convertWithQuantizingTestSource =
        {
            new object?[] { "System 8bpp palette no dithering", PredefinedColorsQuantizer.SystemDefault8BppPalette(), null },
            new object[] { "System 8bpp palette with dithering", PredefinedColorsQuantizer.SystemDefault8BppPalette(), OrderedDitherer.Bayer2x2 },
            new object?[] { "System 4bpp palette no dithering", PredefinedColorsQuantizer.SystemDefault4BppPalette(), null },
            new object[] { "System 4bpp palette with dithering", OptimizedPaletteQuantizer.Wu(16), OrderedDitherer.BlueNoise },
            new object[] { "Black and white with dithering", PredefinedColorsQuantizer.BlackAndWhite(), OrderedDitherer.DottedHalftone },
            new object[] { "ARGB1555 32K color dithering", PredefinedColorsQuantizer.Argb1555(), new RandomNoiseDitherer(seed: 0), },
            new object[] { "RGB888 16.7M color dithering", PredefinedColorsQuantizer.Rgb888(), new RandomNoiseDitherer(seed: 0), },
        };

        #endregion

        #region Methods

        [Test]
        public void ToWriteableBitmapInvalidThreadTest()
        {
            var size = new Size(32, 16);
            using IReadWriteBitmapData src = GenerateAlphaGradientBitmapData(size);
            Exception e = Assert.Throws<COMException>(() => src.ToWriteableBitmap())!;
            Assert.AreEqual(unchecked((int)0x8001010E), e.HResult);
        }

        [Test]
        public Task ToWriteableBitmapTest() => ExecuteTest(() =>
        {
            var size = new Size(32, 16);
            using IReadWriteBitmapData src = GenerateAlphaGradientBitmapData(size);
            var result = src.ToWriteableBitmap();

            Assert.AreEqual(size.Width, result.PixelWidth);
            Assert.AreEqual(size.Height, result.PixelHeight);
            using var resultData = result.GetReadableBitmapData();
            AssertAreEqual(src, resultData);
        });

        [Test]
        public Task ToWriteableBitmapAsyncTest() => ExecuteTestAsync(async () =>
        {
            var size = new Size(32, 16);
            using IReadWriteBitmapData src = GenerateAlphaGradientBitmapData(size);
            WriteableBitmap result = (await src.ToWriteableBitmapAsync())!;

            Assert.AreEqual(size.Width, result.PixelWidth);
            Assert.AreEqual(size.Height, result.PixelHeight);
            using var resultData = result.GetReadableBitmapData();
            AssertAreEqual(src, resultData);
        });

        [TestCaseSource(nameof(convertWithQuantizingTestSource))]
        public Task ToWriteableBitmapWithQuantizingTest(string name, IQuantizer quantizer, IDitherer? ditherer) => ExecuteTest(() =>
        {
            var size = new Size(32, 16);
            using IReadWriteBitmapData src = GenerateAlphaGradientBitmapData(size);
            var result = src.ToWriteableBitmap(quantizer, ditherer);

            Assert.AreEqual(size.Width, result.PixelWidth);
            Assert.AreEqual(size.Height, result.PixelHeight);
            using var resultData = result.GetReadableBitmapData();
            AssertAreEqual(src.Clone(src.PixelFormat.ToKnownPixelFormat(), quantizer, ditherer), resultData);
        });

        [TestCaseSource(nameof(convertWithQuantizingTestSource))]
        public Task ToWriteableBitmapAsyncWithQuantizingTest(string name, IQuantizer quantizer, IDitherer? ditherer) => ExecuteTestAsync(async () =>
        {
            var size = new Size(32, 16);
            using IReadWriteBitmapData src = GenerateAlphaGradientBitmapData(size);
            WriteableBitmap result = (await src.ToWriteableBitmapAsync(quantizer, ditherer))!;

            Assert.AreEqual(size.Width, result.PixelWidth);
            Assert.AreEqual(size.Height, result.PixelHeight);
            using var resultData = result.GetReadableBitmapData();
            AssertAreEqual(await src.CloneAsync(src.PixelFormat.ToKnownPixelFormat(), quantizer, ditherer), resultData);
        });

        #endregion
    }
}
