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

using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

using KGySoft.Drawing.Imaging;

using NUnit.Framework;

#endregion

namespace KGySoft.Drawing.UnitTests.Imaging
{
    [TestFixture]
    public class ReadableBitmapDataExtensionsTest : TestBase
    {
        #region Methods

        [TestCase(PixelFormat.Format1bppIndexed)]
        [TestCase(PixelFormat.Format4bppIndexed)]
        [TestCase(PixelFormat.Format8bppIndexed)]
        [TestCase(PixelFormat.Format16bppGrayScale)]
        [TestCase(PixelFormat.Format16bppRgb555)]
        [TestCase(PixelFormat.Format16bppRgb565)]
        [TestCase(PixelFormat.Format16bppArgb1555)]
        [TestCase(PixelFormat.Format24bppRgb)]
        [TestCase(PixelFormat.Format32bppRgb)]
        [TestCase(PixelFormat.Format32bppRgb)]
        [TestCase(PixelFormat.Format32bppArgb)]
        [TestCase(PixelFormat.Format32bppArgb)]
        [TestCase(PixelFormat.Format32bppPArgb)]
        [TestCase(PixelFormat.Format32bppPArgb)]
        [TestCase(PixelFormat.Format48bppRgb)]
        [TestCase(PixelFormat.Format64bppArgb)]
        [TestCase(PixelFormat.Format64bppPArgb)]
        public void SaveReloadNativeTest(PixelFormat pixelFormat)
        {
            if (!pixelFormat.IsSupportedNatively())
                Assert.Inconclusive($"Not supported: {pixelFormat}");

            var size = new Size(13, 10);
            using var bmp = new Bitmap(size.Width, size.Height, pixelFormat);
            using IReadWriteBitmapData orig = bmp.GetReadWriteBitmapData();
            GenerateAlphaGradient(orig);

            using var ms = new MemoryStream();
            orig.Save(ms);
            ms.Position = 0;
            IReadWriteBitmapData clone = BitmapDataFactory.Load(ms);
            AssertAreEqual(orig, clone, true);
            SaveImage($"{pixelFormat}", clone.ToBitmap());
        }

        #endregion
    }
}
