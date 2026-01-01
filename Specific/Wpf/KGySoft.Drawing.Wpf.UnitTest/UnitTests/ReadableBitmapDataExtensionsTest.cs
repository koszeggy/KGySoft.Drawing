#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ReadableBitmapDataExtensionsTest.cs
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

using KGySoft.Drawing.Imaging;

using NUnit.Framework;

#endregion

namespace KGySoft.Drawing.Wpf.UnitTests
{
    [TestFixture]
    public class ReadableBitmapDataExtensionsTest : TestBase
    {
        #region Methods

        [TestCase(KnownPixelFormat.Format1bppIndexed)]
        [TestCase(KnownPixelFormat.Format4bppIndexed)]
        [TestCase(KnownPixelFormat.Format8bppIndexed)]
        [TestCase(KnownPixelFormat.Format16bppGrayScale)]
        [TestCase(KnownPixelFormat.Format16bppRgb555)]
        [TestCase(KnownPixelFormat.Format16bppRgb565)]
        [TestCase(KnownPixelFormat.Format16bppArgb1555)]
        [TestCase(KnownPixelFormat.Format24bppRgb)]
        [TestCase(KnownPixelFormat.Format32bppRgb)]
        [TestCase(KnownPixelFormat.Format32bppRgb)]
        [TestCase(KnownPixelFormat.Format32bppArgb)]
        [TestCase(KnownPixelFormat.Format32bppArgb)]
        [TestCase(KnownPixelFormat.Format32bppPArgb)]
        [TestCase(KnownPixelFormat.Format32bppPArgb)]
        [TestCase(KnownPixelFormat.Format48bppRgb)]
        [TestCase(KnownPixelFormat.Format64bppArgb)]
        [TestCase(KnownPixelFormat.Format64bppPArgb)]
        public void ToWriteableBitmapTest(KnownPixelFormat pixelFormat)
        {
            using IReadWriteBitmapData src = GetInfoIcon256().GetReadableBitmapData().Clone(pixelFormat);
            var result = src.ToWriteableBitmap();

            Assert.AreEqual(pixelFormat.ToPixelFormat(), result.Format);
            Assert.AreEqual(src.Width, result.PixelWidth);
            Assert.AreEqual(src.Height, result.PixelHeight);
            SaveBitmap($"{pixelFormat}", result);
        }

        #endregion
    }
}
