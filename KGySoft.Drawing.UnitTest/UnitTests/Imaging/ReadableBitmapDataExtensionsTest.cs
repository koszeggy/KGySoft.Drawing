#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ReadableBitmapDataExtensionsTest.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2020 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution. If not, then this file is considered as
//  an illegal copy.
//
//  Unauthorized copying of this file, via any medium is strictly prohibited.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;
using System.Drawing;
using System.Drawing.Imaging;
using KGySoft.Diagnostics;
using KGySoft.Drawing.Imaging;

using NUnit.Framework;

#endregion

namespace KGySoft.Drawing.UnitTests.Imaging
{
    [TestFixture]
    public class ReadableBitmapDataExtensionsTest : TestBase
    {
        #region Methods

        [TestCase(PixelFormat.Format64bppArgb)]
        [TestCase(PixelFormat.Format64bppPArgb)]
        [TestCase(PixelFormat.Format48bppRgb)]
        [TestCase(PixelFormat.Format32bppArgb)]
        [TestCase(PixelFormat.Format32bppPArgb)]
        [TestCase(PixelFormat.Format32bppRgb)]
        [TestCase(PixelFormat.Format24bppRgb)]
        [TestCase(PixelFormat.Format16bppRgb565)]
        [TestCase(PixelFormat.Format16bppRgb555)]
        [TestCase(PixelFormat.Format16bppArgb1555)]
        [TestCase(PixelFormat.Format16bppGrayScale)]
        [TestCase(PixelFormat.Format8bppIndexed)]
        [TestCase(PixelFormat.Format4bppIndexed)]
        [TestCase(PixelFormat.Format1bppIndexed)]
        public void CloneTest(PixelFormat pixelFormat)
        {
            using var bmp = Icons.Information.ExtractBitmap(new Size(256, 256)).ConvertPixelFormat(pixelFormat);
            using (var bitmapData = bmp.GetReadableBitmapData())
            {
                using (IReadWriteBitmapData clone = bitmapData.Clone())
                {
                    AssertAreEqual(bitmapData, clone);
                    SaveImage($"{pixelFormat} - Complete", clone.ToBitmap());
                }

                var sourceRectangle = new Rectangle(16, 16, 128, 128);
                using (IReadWriteBitmapData clone = bitmapData.Clone(sourceRectangle, pixelFormat))
                {
                    AssertAreEqual(bitmapData, clone, sourceRectangle);
                    SaveImage($"{pixelFormat} - Clipped", clone.ToBitmap());
                }
            }
        }

        [TestCase(PixelFormat.Format4bppIndexed)]
        [TestCase(PixelFormat.Format1bppIndexed)]
        public void CloneLowBppForcedDirectProcessingTest(PixelFormat pixelFormat)
        {
            using var bmp = Icons.Information.ExtractBitmap(new Size(256, 256)).ConvertPixelFormat(pixelFormat);
            using (var bitmapData = bmp.GetReadableBitmapData())
            {
                var sourceRectangle = new Rectangle(15, 15, 127, 127);
                using (IReadWriteBitmapData clone = bitmapData.Clone(sourceRectangle, pixelFormat))
                {
                    AssertAreEqual(bitmapData, clone, sourceRectangle);
                    SaveImage($"{pixelFormat} - Clipped", clone.ToBitmap());
                }
            }
        }

        [Test]
        public void CopyToSameInstanceOverlappingTest()
        {
            using var bmp = Icons.Information.ExtractBitmap(new Size(256, 256));
            using (IReadWriteBitmapData bitmapData = bmp.GetReadWriteBitmapData())
            {
                Assert.DoesNotThrow(() => bitmapData.CopyTo(bitmapData, new Point(64, 64)));
            }

            SaveImage("result", bmp);
        }

        #endregion
    }
}