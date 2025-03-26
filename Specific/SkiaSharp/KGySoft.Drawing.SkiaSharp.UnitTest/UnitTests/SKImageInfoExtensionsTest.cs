#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: SKImageInfoExtensionsTest.cs
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

using KGySoft.Drawing.Imaging;

using NUnit.Framework;

using SkiaSharp;

#endregion

namespace KGySoft.Drawing.SkiaSharp.UnitTests
{
    [TestFixture]
    public class SKImageInfoExtensionsTest
    {
        #region Methods

        [TestCase(SKColorType.Rgb888x, SKAlphaType.Unpremul, WorkingColorSpace.Linear, KnownPixelFormat.Format24bppRgb, WorkingColorSpace.Linear)]
        [TestCase(SKColorType.Rgb888x, SKAlphaType.Unpremul, WorkingColorSpace.Srgb, KnownPixelFormat.Format24bppRgb, WorkingColorSpace.Srgb)]
        [TestCase(SKColorType.Bgra8888, SKAlphaType.Opaque, WorkingColorSpace.Srgb, KnownPixelFormat.Format24bppRgb, WorkingColorSpace.Srgb)]
        public void GetMatchingQuantizerTest(SKColorType colorType, SKAlphaType alphaType, WorkingColorSpace colorSpace, KnownPixelFormat expectedHint, WorkingColorSpace expectedColorSpace)
        {
            var info = new SKImageInfo
            {
                ColorType = colorType,
                AlphaType = alphaType,
                ColorSpace = colorSpace switch
                {
                    WorkingColorSpace.Linear => SKColorSpace.CreateSrgbLinear(),
                    WorkingColorSpace.Srgb => SKColorSpace.CreateSrgb(),
                    _ => null
                }
            };

            PredefinedColorsQuantizer quantizer = info.GetMatchingQuantizer(SKColors.Silver);
            Assert.AreEqual(expectedHint, quantizer.PixelFormatHint);
            Assert.AreEqual(expectedColorSpace, quantizer.WorkingColorSpace);
            Assert.AreEqual(Color.Silver.ToColor32(), quantizer.BackColor);
        }


        #endregion
    }
}
