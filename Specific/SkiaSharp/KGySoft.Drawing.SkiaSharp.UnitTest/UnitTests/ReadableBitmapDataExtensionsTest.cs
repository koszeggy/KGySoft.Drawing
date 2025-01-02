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

#region Usings

using System.Drawing;
using System.Threading.Tasks;

using KGySoft.CoreLibraries;
using KGySoft.Drawing.Imaging;

using NUnit.Framework;

using SkiaSharp;

#endregion

namespace KGySoft.Drawing.SkiaSharp.UnitTests
{
    [TestFixture]
    public class ReadableBitmapDataExtensionsTest : TestBase
    {
        #region Methods

        [TestCase(SKColorType.Argb4444, SKAlphaType.Unpremul)]
        [TestCase(SKColorType.Argb4444, SKAlphaType.Premul)]
        [TestCase(SKColorType.Argb4444, SKAlphaType.Opaque)]
        public void ToSKBitmapDirectTest(SKColorType colorType, SKAlphaType alphaType)
        {
            Size size = new Size(512, 256);
            var backColor = new Color32(ThreadSafeRandom.Instance.SampleByte(), ThreadSafeRandom.Instance.SampleByte(), ThreadSafeRandom.Instance.SampleByte());
            using IReadWriteBitmapData bitmapData = BitmapDataFactory.CreateBitmapData(size, backColor: backColor);
            GenerateAlphaGradient(bitmapData);
            using SKBitmap result = bitmapData.ToSKBitmap(colorType, alphaType);
            Assert.AreEqual(colorType, result.ColorType);
            Assert.AreEqual(alphaType, result.AlphaType);
            SaveBitmap($"{colorType} {alphaType}", result);
        }

        [TestCase(SKColorType.Argb4444, SKAlphaType.Unpremul)]
        [TestCase(SKColorType.Argb4444, SKAlphaType.Premul)]
        [TestCase(SKColorType.Argb4444, SKAlphaType.Opaque)]
        public async Task ToSKBitmapDirectTestAsync(SKColorType colorType, SKAlphaType alphaType)
        {
            Size size = new Size(512, 256);
            var backColor = new Color32(ThreadSafeRandom.Instance.SampleByte(), ThreadSafeRandom.Instance.SampleByte(), ThreadSafeRandom.Instance.SampleByte());
            using IReadWriteBitmapData bitmapData = BitmapDataFactory.CreateBitmapData(size, backColor: backColor);
            GenerateAlphaGradient(bitmapData);
            using SKBitmap result = (await bitmapData.ToSKBitmapAsync(colorType, alphaType))!;
            Assert.AreEqual(colorType, result.ColorType);
            Assert.AreEqual(alphaType, result.AlphaType);
            SaveBitmap($"{colorType} {alphaType}", result);
        }

        [TestCase(SKColorType.Argb4444, SKAlphaType.Unpremul)]
        [TestCase(SKColorType.Argb4444, SKAlphaType.Premul)]
        [TestCase(SKColorType.Argb4444, SKAlphaType.Opaque)]
        public void ToSKBitmapWithDithererTest(SKColorType colorType, SKAlphaType alphaType)
        {
            Size size = new Size(512, 256);
            var backColor = new Color32(ThreadSafeRandom.Instance.SampleByte(), ThreadSafeRandom.Instance.SampleByte(), ThreadSafeRandom.Instance.SampleByte());
            using IReadWriteBitmapData bitmapData = BitmapDataFactory.CreateBitmapData(size, backColor: backColor);
            GenerateAlphaGradient(bitmapData);
            using SKBitmap result = bitmapData.ToSKBitmap(colorType, alphaType, ditherer: OrderedDitherer.Bayer8x8);
            Assert.AreEqual(colorType, result.ColorType);
            Assert.AreEqual(alphaType, result.AlphaType);
            SaveBitmap($"{colorType} {alphaType}", result);
        }

        #endregion
    }
}