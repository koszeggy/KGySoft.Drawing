#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: PixelFormatExtensionsTest.cs
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

using System.Drawing.Imaging;

using KGySoft.Drawing.Imaging;

using NUnit.Framework;

#endregion

namespace KGySoft.Drawing.UnitTests
{
    [TestFixture]
    public class PixelFormatExtensionsTest
    {
        #region Methods

        [TestCase(KnownPixelFormat.Format1bppIndexed, PixelFormat.Format1bppIndexed)]
        [TestCase(KnownPixelFormat.Format4bppIndexed, PixelFormat.Format4bppIndexed)]
        [TestCase(KnownPixelFormat.Format8bppIndexed, PixelFormat.Format8bppIndexed)]
        [TestCase(KnownPixelFormat.Format16bppGrayScale, PixelFormat.Format16bppGrayScale)]
        [TestCase(KnownPixelFormat.Format16bppRgb555, PixelFormat.Format16bppRgb555)]
        [TestCase(KnownPixelFormat.Format16bppRgb565, PixelFormat.Format16bppRgb565)]
        [TestCase(KnownPixelFormat.Format16bppArgb1555, PixelFormat.Format16bppArgb1555)]
        [TestCase(KnownPixelFormat.Format24bppRgb, PixelFormat.Format24bppRgb)]
        [TestCase(KnownPixelFormat.Format32bppRgb, PixelFormat.Format32bppRgb)]
        [TestCase(KnownPixelFormat.Format32bppArgb, PixelFormat.Format32bppArgb)]
        [TestCase(KnownPixelFormat.Format32bppPArgb, PixelFormat.Format32bppPArgb)]
        [TestCase(KnownPixelFormat.Format48bppRgb, PixelFormat.Format48bppRgb)]
        [TestCase(KnownPixelFormat.Format64bppArgb, PixelFormat.Format64bppArgb)]
        [TestCase(KnownPixelFormat.Format64bppPArgb, PixelFormat.Format64bppPArgb)]
        [TestCase(KnownPixelFormat.Format96bppRgb, PixelFormat.Format48bppRgb)]
        [TestCase(KnownPixelFormat.Format128bppRgba, PixelFormat.Format64bppArgb)]
        [TestCase(KnownPixelFormat.Format128bppPRgba, PixelFormat.Format64bppPArgb)]
        [TestCase(KnownPixelFormat.Format8bppGrayScale, PixelFormat.Format16bppGrayScale)]
        [TestCase(KnownPixelFormat.Format32bppGrayScale, PixelFormat.Format16bppGrayScale)]
        public void ToPixelFormatTest(KnownPixelFormat knownPixelFormat, PixelFormat expectedResult)
        {
            Assert.AreEqual(expectedResult, knownPixelFormat.ToPixelFormat());
        }

        [TestCase(PixelFormat.Format1bppIndexed, KnownPixelFormat.Format1bppIndexed)]
        [TestCase(PixelFormat.Format4bppIndexed, KnownPixelFormat.Format4bppIndexed)]
        [TestCase(PixelFormat.Format8bppIndexed, KnownPixelFormat.Format8bppIndexed)]
        [TestCase(PixelFormat.Format16bppGrayScale, KnownPixelFormat.Format16bppGrayScale)]
        [TestCase(PixelFormat.Format16bppRgb555, KnownPixelFormat.Format16bppRgb555)]
        [TestCase(PixelFormat.Format16bppRgb565, KnownPixelFormat.Format16bppRgb565)]
        [TestCase(PixelFormat.Format16bppArgb1555, KnownPixelFormat.Format16bppArgb1555)]
        [TestCase(PixelFormat.Format24bppRgb, KnownPixelFormat.Format24bppRgb)]
        [TestCase(PixelFormat.Format32bppRgb, KnownPixelFormat.Format32bppRgb)]
        [TestCase(PixelFormat.Format32bppArgb, KnownPixelFormat.Format32bppArgb)]
        [TestCase(PixelFormat.Format32bppPArgb, KnownPixelFormat.Format32bppPArgb)]
        [TestCase(PixelFormat.Format48bppRgb, KnownPixelFormat.Format48bppRgb)]
        [TestCase(PixelFormat.Format64bppArgb, KnownPixelFormat.Format64bppArgb)]
        [TestCase(PixelFormat.Format64bppPArgb, KnownPixelFormat.Format64bppPArgb)]
        [TestCase(PixelFormatExtensions.Format32bppCmyk, KnownPixelFormat.Format24bppRgb)]
        public void ToKnownPixelFormatTest(PixelFormat pixelFormat, KnownPixelFormat expectedResult)
        {
            Assert.AreEqual(expectedResult, pixelFormat.ToKnownPixelFormat());

            // the possible difference is documented
            var info = pixelFormat.GetInfo();
            if (info.LinearGamma)
                info.Prefers64BitColors = true;
            Assert.AreEqual(expectedResult, info.ToKnownPixelFormat());
        }

        #endregion
    }
}
