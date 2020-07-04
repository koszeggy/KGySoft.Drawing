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
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

using KGySoft.Drawing.Imaging;
using KGySoft.Reflection;

using NUnit.Framework;

#endregion

namespace KGySoft.Drawing.UnitTests.Imaging
{
    [TestFixture]
    public class ReadableBitmapDataExtensionsTest : TestBase
    {
        #region Nested classes

        #region TestReadableBitmapData class

        private class TestReadableBitmapData : IReadableBitmapData
        {
            #region Fields

            private readonly IReadableBitmapData wrapped;

            #endregion

            #region Properties and Indexers

            #region Properties

            public int Height => wrapped.Height;
            public int Width => wrapped.Width;
            public PixelFormat PixelFormat => wrapped.PixelFormat;
            public Palette Palette => wrapped.Palette;
            public int RowSize => wrapped.RowSize;
            public Color32 BackColor => wrapped.BackColor;
            public byte AlphaThreshold => wrapped.AlphaThreshold;
            public IReadableBitmapDataRow FirstRow => wrapped.FirstRow;

            #endregion

            #region Indexers

            public IReadableBitmapDataRow this[int y] => wrapped[y];

            #endregion

            #endregion

            #region Constructors

            internal TestReadableBitmapData(IReadableBitmapData wrapped) => this.wrapped = wrapped;

            #endregion

            #region Methods

            public void Dispose() => wrapped.Dispose();
            public Color GetPixel(int x, int y) => wrapped.GetPixel(x, y);

            #endregion
        }

        #endregion

        #endregion

        #region Methods

        [Test]
        public void CloneDecreasingPaletteTest()
        {
            using var source = Icons.Information.ExtractBitmap(new Size(256, 256)).ConvertPixelFormat(PixelFormat.Format8bppIndexed).GetReadableBitmapData();
            using var clone = source.Clone(PixelFormat.Format1bppIndexed);
            SaveImage(null, clone.ToBitmap());
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
                    AssertAreEqual(bitmapData, clone, false, sourceRectangle);
                    //SaveImage($"{pixelFormat} - Clipped", clone.ToBitmap());
                }
            }
        }

        [Test]
        public void CloneWithWrappedDataTest()
        {
            using var bmp = Icons.Information.ExtractBitmap(new Size(256, 256));
            using (var bitmapData = new TestReadableBitmapData(bmp.GetReadableBitmapData()))
            {
                using (IReadWriteBitmapData clone = bitmapData.Clone())
                {
                    AssertAreEqual(bitmapData, clone);
                    //SaveImage("Clone", clone.ToBitmap());
                }
            }
        }

        [TestCase(PixelFormat.Format32bppArgb)]
        [TestCase(PixelFormat.Format16bppArgb1555)]
        [TestCase(PixelFormat.Format16bppGrayScale)]
        [TestCase(PixelFormat.Format8bppIndexed)]
        [TestCase(PixelFormat.Format4bppIndexed)]
        [TestCase(PixelFormat.Format1bppIndexed)]
        public void CloneWithPredefinedQuantizerTest(PixelFormat pixelFormat)
        {
            using var bmp = Icons.Information.ExtractBitmap(new Size(256, 256));
            using var source = bmp.GetReadableBitmapData();
            using var clone = source.Clone(pixelFormat, PredefinedColorsQuantizer.FromPixelFormat(pixelFormat));
            SaveImage($"{pixelFormat}", clone.ToBitmap());
        }

        [TestCase(PixelFormat.Format8bppIndexed)]
        [TestCase(PixelFormat.Format4bppIndexed)]
        [TestCase(PixelFormat.Format1bppIndexed)]
        public void CloneWithOptimizedQuantizerTest(PixelFormat pixelFormat)
        {
            using var bmp = Icons.Information.ExtractBitmap(new Size(256, 256));
            using var source = bmp.GetReadableBitmapData();
            using var clone = source.Clone(pixelFormat, OptimizedPaletteQuantizer.Wu(1 << pixelFormat.ToBitsPerPixel()));
            SaveImage($"{pixelFormat}", clone.ToBitmap());
        }

        [TestCase(PixelFormat.Format16bppArgb1555)]
        [TestCase(PixelFormat.Format8bppIndexed)]
        [TestCase(PixelFormat.Format4bppIndexed)]
        [TestCase(PixelFormat.Format1bppIndexed)]
        public void CloneWithDithererTest(PixelFormat pixelFormat)
        {
            var ditherers = new Dictionary<string, IDitherer>
            {
                ["Ordered"] = OrderedDitherer.Bayer8x8,
                ["Error Diffusion (raster)"] = ErrorDiffusionDitherer.FloydSteinberg,
                ["Error Diffusion (serpentine)"] = ErrorDiffusionDitherer.FloydSteinberg.ConfigureProcessingDirection(true),
            };

            using var source = Icons.Information.ExtractBitmap(new Size(256, 256)).GetReadWriteBitmapData();
            foreach (var ditherer in ditherers)
            {
                using var cloneIndexed = source.Clone(pixelFormat, ditherer.Value);
                using var cloneTrueColor = source.Clone(PixelFormat.Format32bppArgb, PredefinedColorsQuantizer.FromPixelFormat(pixelFormat), ditherer.Value);
                AssertAreEqual(cloneIndexed, cloneTrueColor, true);
                SaveImage($"{pixelFormat} {ditherer.Key}", cloneIndexed.ToBitmap());
            }
        }

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
        public void CloneVsClipTest(PixelFormat pixelFormat)
        {
            using var bmp = Icons.Information.ExtractBitmap(new Size(256, 256)).ConvertPixelFormat(pixelFormat);
            using (var bitmapData = bmp.GetReadableBitmapData())
            {
                using (IReadWriteBitmapData clone = bitmapData.Clone())
                {
                    AssertAreEqual(bitmapData, clone);
                    SaveImage($"{pixelFormat} - Complete clone", clone.ToBitmap());
                }

                var sourceRectangle = new Rectangle(16, 16, 128, 128);
                using (IReadWriteBitmapData clone = bitmapData.Clone(sourceRectangle, pixelFormat))
                {
                    AssertAreEqual(bitmapData, clone, false, sourceRectangle);
                    SaveImage($"{pixelFormat} - Clipped clone", clone.ToBitmap());
                }

                using (IReadableBitmapData clip = bitmapData.Clip(sourceRectangle))
                {
                    AssertAreEqual(bitmapData, clip, false, sourceRectangle);
                    //SaveImage($"{pixelFormat} - Clipping wrapper", clip.ToBitmap());
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

            SaveImage(null, bmp);
        }

        [Test]
        public void CopyToSameInstanceOverlappingByClippingTest()
        {
            using var bmp = Icons.Information.ExtractBitmap(new Size(256, 256));
            using (IReadWriteBitmapData bitmapData = bmp.GetReadWriteBitmapData())
            {
                using var clipSrc = bitmapData.Clip(new Rectangle(64, 64, 128, 128));
                using var clipDst = bitmapData.Clip(new Rectangle(64, 64, 128, 128));

                Assert.DoesNotThrow(() => clipSrc.CopyTo(clipDst, new Point(32, 32)));
            }

            SaveImage(null, bmp);
        }


        [TestCase(PixelFormat.Format32bppArgb)]
        [TestCase(PixelFormat.Format8bppIndexed)]
        [TestCase(PixelFormat.Format1bppIndexed)]
        public void CopyToRawTest(PixelFormat pixelFormat)
        {
            var rect = new Rectangle(128, 128, 128, 128);
            using var source = Icons.Information.ExtractBitmap(new Size(256, 256)).ConvertPixelFormat(pixelFormat).GetReadWriteBitmapData();
            using var targetFull = BitmapDataFactory.CreateBitmapData(source.GetSize(), pixelFormat);
            source.CopyTo(targetFull);
            AssertAreEqual(source, targetFull);

            using var targetClipped = BitmapDataFactory.CreateBitmapData(rect.Size, pixelFormat);
            source.CopyTo(targetClipped, rect);
            AssertAreEqual(source, targetClipped, false, rect);

            SaveImage($"{pixelFormat} clipped", targetClipped.ToBitmap());
        }

        [TestCase(PixelFormat.Format8bppIndexed)]
        [TestCase(PixelFormat.Format4bppIndexed)]
        [TestCase(PixelFormat.Format1bppIndexed)]
        public void CopyToDirectTest(PixelFormat pixelFormat)
        {
            var rect = new Rectangle(128, 128, 128, 128);
            using var source = Icons.Information.ExtractBitmap(new Size(256, 256)).GetReadWriteBitmapData();
            using var targetFull = BitmapDataFactory.CreateBitmapData(source.GetSize(), pixelFormat);
            source.CopyTo(targetFull);

            using var targetClipped = BitmapDataFactory.CreateBitmapData(rect.Size, pixelFormat);
            source.CopyTo(targetClipped, rect);

            AssertAreEqual(targetFull, targetClipped, false, rect);

            SaveImage($"{pixelFormat} clipped", targetClipped.ToBitmap());
        }

        [TestCase(PixelFormat.Format8bppIndexed)]
        [TestCase(PixelFormat.Format4bppIndexed)]
        [TestCase(PixelFormat.Format1bppIndexed)]
        public void CopyToWithQuantizerTest(PixelFormat pixelFormat)
        {
            var rect = new Rectangle(128, 128, 128, 128);
            using var source = Icons.Information.ExtractBitmap(new Size(256, 256)).GetReadWriteBitmapData();
            using var targetFull = BitmapDataFactory.CreateBitmapData(source.GetSize());
            var quantizer = PredefinedColorsQuantizer.FromPixelFormat(pixelFormat);
            source.CopyTo(targetFull, Point.Empty, quantizer);

            using var targetClipped = BitmapDataFactory.CreateBitmapData(rect.Size);
            source.CopyTo(targetClipped, rect, Point.Empty, quantizer);
            AssertAreEqual(targetFull, targetClipped, false, rect);

            SaveImage($"{pixelFormat} clipped", targetClipped.ToBitmap());
        }

        [TestCase(PixelFormat.Format8bppIndexed)]
        [TestCase(PixelFormat.Format4bppIndexed)]
        [TestCase(PixelFormat.Format1bppIndexed)]
        public void CopyToWithDithererTest(PixelFormat pixelFormat)
        {
            var rect = new Rectangle(128, 128, 128, 128);
            using var source = Icons.Information.ExtractBitmap(new Size(256, 256)).GetReadWriteBitmapData();
            var ditherers = new Dictionary<string, IDitherer>
            {
                ["Ordered"] = OrderedDitherer.Bayer8x8,
                ["Error Diffusion (raster)"] = ErrorDiffusionDitherer.FloydSteinberg,
                ["Error Diffusion (serpentine)"] = ErrorDiffusionDitherer.FloydSteinberg.ConfigureProcessingDirection(true),
            };

            foreach (var ditherer in ditherers)
            {
                using var targetClipped = BitmapDataFactory.CreateBitmapData(rect.Size, pixelFormat);
                source.CopyTo(targetClipped, rect, Point.Empty, ditherer.Value);
                SaveImage($"{pixelFormat} {ditherer.Key}", targetClipped.ToBitmap());
            }
        }

        [Test]
        public void ClippingClippedBitmapDataTest()
        {
            using IReadWriteBitmapData bitmapData = BitmapDataFactory.CreateBitmapData(new Size(100, 100));

            // clipping a clipped region
            using var clipped = bitmapData.Clip(new Rectangle(50, 80, 100, 100));
            Assert.AreEqual(new Rectangle(50, 80, 50, 20), Reflector.GetField(clipped, "region"));
            using var subClipped = clipped.Clip(new Rectangle(20, -10, 20, 50));
            Assert.AreEqual(new Rectangle(70, 80, 20, 20), Reflector.GetField(subClipped, "region"));
        }

        [Test]
        public void ClippingIndexedTest()
        {
            using IReadWriteBitmapData bitmapData = BitmapDataFactory.CreateBitmapData(new Size(127, 1), PixelFormat.Format1bppIndexed);

            // original
            Assert.AreEqual(16, bitmapData.RowSize);

            // clipped region makes last columns unavailable by raw data to protect the cut-out part from being overwritten
            Assert.AreEqual(15, bitmapData.Clip(new Rectangle(0, 0, 126, 1)).RowSize);

            // raw access is completely disabled if clipping does not start on byte boundary
            Assert.AreEqual(0, bitmapData.Clip(new Rectangle(1, 0, 127, 1)).RowSize);
            Assert.AreEqual(15, bitmapData.Clip(new Rectangle(8, 0, 120, 1)).RowSize);

            // last column does not have to end on byte boundary because of the padding at the end of the lines
            Assert.AreEqual(15, bitmapData.Clip(new Rectangle(8, 0, 119, 1)).RowSize);

            // but if we shrink the area by 8 pixels from the right the available raw area shrinks by 2 bytes
            Assert.AreEqual(13, bitmapData.Clip(new Rectangle(8, 0, 111, 1)).RowSize);
        }

        #endregion
    }
}
