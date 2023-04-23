using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KGySoft.Drawing.Imaging;

using NUnit.Framework;

using SkiaSharp;

namespace KGySoft.Drawing.SkiaSharp.UnitTests
{
    [TestFixture]
    public class SKImageInfoExtensionsTest
    {
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
    }
}
