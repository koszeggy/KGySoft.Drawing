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
using KGySoft.Diagnostics;
using KGySoft.Drawing.Imaging;
using KGySoft.Reflection;

using NUnit.Framework;

#endregion

namespace KGySoft.Drawing.UnitTests.Imaging
{
    [TestFixture]
    public class BitmapDataExtensionsTest : TestBase
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
                    SaveImage($"{pixelFormat} - Clipped", clone.ToBitmap());
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
                using var clipSrc = bitmapData.Clip(new Rectangle(32, 32, 128, 128));
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
            source.CopyTo(targetClipped, rect, Point.Empty);
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
            source.CopyTo(targetClipped, rect, Point.Empty);

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
        public void DrawIntoWithoutResizeSameInstanceOverlappingTest()
        {
            using var bmp = Icons.Information.ExtractBitmap(new Size(256, 256));
            using (IReadWriteBitmapData bitmapData = bmp.GetReadWriteBitmapData())
            {
                Assert.DoesNotThrow(() => bitmapData.DrawInto(bitmapData, new Point(64, 64)));
            }

            SaveImage(null, bmp);
        }


        [TestCase(PixelFormat.Format1bppIndexed)]
        [TestCase(PixelFormat.Format4bppIndexed)]
        [TestCase(PixelFormat.Format8bppIndexed)]
        [TestCase(PixelFormat.Format16bppArgb1555)]
        [TestCase(PixelFormat.Format32bppArgb)]
        public void DrawIntoNoResizeDirectTest(PixelFormat pixelFormat)
        {
            using var target = BitmapDataFactory.CreateBitmapData(new Size(256, 256), pixelFormat);
            using var icon64 = Icons.Information.ExtractBitmap(new Size(64, 64)).GetReadWriteBitmapData();
            using var icon256 = Icons.Information.ExtractBitmap(new Size(256, 256)).GetReadWriteBitmapData();
            using var gradient = GenerateAlphaGradientBitmapData(new Size(300, 300));

            // solid source
            icon64.Clone(PixelFormat.Format24bppRgb, new Color32(Color.Silver))
                .DrawInto(target);

            // single bit alpha source
            icon64.Clone(PixelFormat.Format16bppArgb1555, new Color32(Color.Silver))
                .DrawInto(target, new Point(192, 192));

            // alpha source
            icon256.DrawInto(target);

            // alpha gradient source
            gradient.DrawInto(target, new Rectangle(10, 10, 200, 200), new Point(32, 32));

            SaveImage($"{pixelFormat}", target.ToBitmap());
        }

        [TestCase(PixelFormat.Format1bppIndexed)]
        [TestCase(PixelFormat.Format4bppIndexed)]
        [TestCase(PixelFormat.Format8bppIndexed)]
        [TestCase(PixelFormat.Format16bppArgb1555)]
        public void DrawIntoNoResizeWithQuantizingTest(PixelFormat pixelFormat)
        {
            var quantizer = PredefinedColorsQuantizer.FromPixelFormat(pixelFormat);

            var ditherers = new Dictionary<string, IDitherer>
            {
                ["(not dithered)"] = null,
                ["Ordered"] = OrderedDitherer.Bayer8x8,
                ["Error Diffusion (raster)"] = ErrorDiffusionDitherer.FloydSteinberg,
                ["Error Diffusion (serpentine)"] = ErrorDiffusionDitherer.FloydSteinberg.ConfigureProcessingDirection(true),
            };

            foreach (var ditherer in ditherers)
            {
                using var target = BitmapDataFactory.CreateBitmapData(new Size(256, 256));
                using var icon64 = Icons.Information.ExtractBitmap(new Size(64, 64)).GetReadWriteBitmapData();
                using var icon256 = Icons.Information.ExtractBitmap(new Size(256, 256)).GetReadWriteBitmapData();
                using var gradient = GenerateAlphaGradientBitmapData(new Size(300, 300));

                // solid source
                icon64.Clone(PixelFormat.Format24bppRgb, new Color32(Color.Silver))
                    .DrawInto(target, Point.Empty, quantizer, ditherer.Value);

                // single bit alpha source
                icon64.Clone(PixelFormat.Format16bppArgb1555, new Color32(Color.Silver))
                    .DrawInto(target, new Point(192, 192), quantizer, ditherer.Value);

                // alpha source
                icon256.DrawInto(target, Point.Empty, quantizer, ditherer.Value);

                // alpha gradient source
                gradient.DrawInto(target, new Rectangle(10, 10, 200, 200), new Point(32, 32), quantizer, ditherer.Value);

                SaveImage($"{pixelFormat} {ditherer.Key}", target.ToBitmap());
            }
        }

        [TestCase(ScalingMode.Auto)]
        [TestCase(ScalingMode.NearestNeighbor)]
        public void DrawIntoWithResizeSameInstanceOverlappingTest(ScalingMode scalingMode)
        {
            using var bmp = Icons.Information.ExtractBitmap(new Size(256, 256));
            using (IReadWriteBitmapData bitmapData = bmp.GetReadWriteBitmapData())
            {
                Assert.DoesNotThrow(() => bitmapData.DrawInto(bitmapData, new Rectangle(32, 32, 192, 192), new Rectangle(64, 64, 128, 128), scalingMode));
            }

            SaveImage($"{scalingMode}", bmp);
        }

        [TestCase(PixelFormat.Format1bppIndexed, ScalingMode.NearestNeighbor)]
        [TestCase(PixelFormat.Format1bppIndexed, ScalingMode.Auto)]
        [TestCase(PixelFormat.Format4bppIndexed, ScalingMode.NearestNeighbor)]
        [TestCase(PixelFormat.Format4bppIndexed, ScalingMode.Auto)]
        [TestCase(PixelFormat.Format8bppIndexed, ScalingMode.NearestNeighbor)]
        [TestCase(PixelFormat.Format8bppIndexed, ScalingMode.Auto)]
        [TestCase(PixelFormat.Format16bppArgb1555, ScalingMode.NearestNeighbor)]
        [TestCase(PixelFormat.Format16bppArgb1555, ScalingMode.Auto)]
        [TestCase(PixelFormat.Format32bppArgb, ScalingMode.NearestNeighbor)]
        [TestCase(PixelFormat.Format32bppArgb, ScalingMode.Auto)]
        public void DrawIntoWithResizeDirectTest(PixelFormat pixelFormat, ScalingMode scalingMode)
        {
            // target and sources
            using var target = BitmapDataFactory.CreateBitmapData(new Size(256, 256), pixelFormat);
            using var icon16 = Icons.Information.ExtractBitmap(new Size(16, 16)).GetReadWriteBitmapData();
            using var icon256 = Icons.Information.ExtractBitmap(new Size(256, 256)).GetReadWriteBitmapData();
            using var gradient = GenerateAlphaGradientBitmapData(new Size(256, 256));

            // enlarge solid source
            var targetRect = new Rectangle(0, 0, 100, 100);
            icon16.Clone(PixelFormat.Format24bppRgb, new Color32(Color.Silver))
                .DrawInto(target, targetRect, scalingMode);

            // enlarge alpha source
            targetRect = new Rectangle(160, 160, 100, 100);
            icon16.DrawInto(target, targetRect, scalingMode);

            // shrink single bit alpha source
            targetRect = new Rectangle(Point.Empty, target.GetSize());
            targetRect.Inflate(-32, -32);
            icon256.Clone(PixelFormat.Format16bppArgb1555)
                .DrawInto(target, targetRect, scalingMode);

            // shrink alpha source (gradient overlay)
            targetRect.Inflate(-10, -10);
            gradient.DrawInto(target, targetRect, scalingMode);

            SaveImage($"{pixelFormat} {scalingMode}", target.ToBitmap());
        }

        [TestCase(PixelFormat.Format1bppIndexed, ScalingMode.NearestNeighbor)]
        [TestCase(PixelFormat.Format1bppIndexed, ScalingMode.Auto)]
        [TestCase(PixelFormat.Format4bppIndexed, ScalingMode.NearestNeighbor)]
        [TestCase(PixelFormat.Format4bppIndexed, ScalingMode.Auto)]
        [TestCase(PixelFormat.Format8bppIndexed, ScalingMode.NearestNeighbor)]
        [TestCase(PixelFormat.Format8bppIndexed, ScalingMode.Auto)]
        [TestCase(PixelFormat.Format16bppArgb1555, ScalingMode.NearestNeighbor)]
        [TestCase(PixelFormat.Format16bppArgb1555, ScalingMode.Auto)]
        public void DrawIntoResizeWithQuantizingTest(PixelFormat pixelFormat, ScalingMode scalingMode)
        {
            var quantizer = PredefinedColorsQuantizer.FromPixelFormat(pixelFormat);

            var ditherers = new Dictionary<string, IDitherer>
            {
                ["(no dithering)"] = null,
                ["Ordered"] = OrderedDitherer.Bayer8x8,
                ["Error Diffusion (raster)"] = ErrorDiffusionDitherer.FloydSteinberg,
                ["Error Diffusion (serpentine)"] = ErrorDiffusionDitherer.FloydSteinberg.ConfigureProcessingDirection(true),
            };

            foreach (KeyValuePair<string, IDitherer> ditherer in ditherers)
            {
                // 32bpp argb target and sources
                using var target = BitmapDataFactory.CreateBitmapData(new Size(256, 256));
                using var icon16 = Icons.Information.ExtractBitmap(new Size(16, 16)).GetReadWriteBitmapData();
                using var icon256 = Icons.Information.ExtractBitmap(new Size(256, 256)).GetReadWriteBitmapData();
                using var gradient = GenerateAlphaGradientBitmapData(new Size(256, 256));

                // enlarge solid source
                var targetRect = new Rectangle(-10, -10, 100, 100);
                icon16.Clone(PixelFormat.Format24bppRgb, new Color32(Color.Silver))
                    .DrawInto(target, targetRect, quantizer, ditherer.Value, scalingMode);

                // enlarge alpha source
                targetRect = new Rectangle(160, 160, 100, 100);
                icon16.DrawInto(target, targetRect, quantizer, ditherer.Value, scalingMode);

                // shrink single bit alpha source
                targetRect = new Rectangle(Point.Empty, target.GetSize());
                targetRect.Inflate(-32, -32);
                icon256.Clone(PixelFormat.Format16bppArgb1555)
                    .DrawInto(target, targetRect, quantizer, ditherer.Value, scalingMode);

                // shrink alpha source (gradient overlay)
                targetRect.Inflate(-10, -10);
                gradient.DrawInto(target, targetRect, quantizer, ditherer.Value, scalingMode);

                SaveImage($"{pixelFormat} {ditherer.Key} {scalingMode}", target.ToBitmap());
            }
        }

        [TestCase(PixelFormat.Format1bppIndexed, 0xFFFFFFFF)]
        [TestCase(PixelFormat.Format4bppIndexed, 0xFF0000FF)]
        [TestCase(PixelFormat.Format8bppIndexed, 0xFF0000FF)]
        [TestCase(PixelFormat.Format16bppGrayScale, 0xFF888888)]
        [TestCase(PixelFormat.Format16bppRgb555, 0xFF0000FF)]
        [TestCase(PixelFormat.Format16bppRgb565, 0xFF0000FF)]
        [TestCase(PixelFormat.Format16bppArgb1555, 0xFF0000FF)]
        [TestCase(PixelFormat.Format24bppRgb, 0xFF0000FF)]
        [TestCase(PixelFormat.Format32bppRgb, 0xFF0000FF)]
        [TestCase(PixelFormat.Format32bppRgb, 0x800000FF)]
        [TestCase(PixelFormat.Format32bppArgb, 0xFF0000FF)]
        [TestCase(PixelFormat.Format32bppArgb, 0x800000FF)]
        [TestCase(PixelFormat.Format32bppPArgb, 0xFF0000FF)]
        [TestCase(PixelFormat.Format32bppPArgb, 0x800000FF)]
        [TestCase(PixelFormat.Format48bppRgb, 0xFF0000FF)]
        [TestCase(PixelFormat.Format64bppArgb, 0xFF0000FF)]
        [TestCase(PixelFormat.Format64bppPArgb, 0xFF0000FF)]
        public void ClearTest(PixelFormat pixelFormat, uint argb)
        {
            const int size = 17;
            Color32 color = Color32.FromArgb((int)argb);

            using var bitmapData = BitmapDataFactory.CreateBitmapData(new Size(size, size), pixelFormat);
            (string Name, IReadWriteBitmapData BitmapData)[] sources = new[]
            {
                ("full", bitmapData),
                ($"clipped right, width={size - 1}", bitmapData.Clone().Clip(new Rectangle(0, 0, size - 1, 1))),
                ($"clipped right, width={size - 2}", bitmapData.Clone().Clip(new Rectangle(0, 0, size - 2, 1))),
                ("clipped left", bitmapData.Clone().Clip(new Rectangle(1, 0, size - 1, 1))),
            };

            foreach (var source in sources)
            {
                source.BitmapData.Clear(color);

                IReadableBitmapDataRow row = source.BitmapData.FirstRow;
                var expected = color;
                if (!pixelFormat.HasMultiLevelAlpha())
                    expected = expected.BlendWithBackground(default);
                do
                {
                    for (int x = 0; x < source.BitmapData.Width; x++)
                        Assert.AreEqual(expected, row[x]);
                } while (row.MoveNextRow());

                //SaveImage($"{pixelFormat} {source.Name}", source.BitmapData.ToBitmap());
            }
        }

        [TestCase(PixelFormat.Format1bppIndexed, 0xFF0000FF, 0U)]
        [TestCase(PixelFormat.Format1bppIndexed, 0U, 0x88888888)]
        [TestCase(PixelFormat.Format4bppIndexed, 0xFFABCDEF, 0U)]
        [TestCase(PixelFormat.Format4bppIndexed, 0U, 0x88888888)]
        public void ClearWithDitheringTest(PixelFormat pixelFormat, uint argb, uint argbBackColor)
        {
            const int size = 17;

            var ditherers = new Dictionary<string, IDitherer>
            {
                ["(no dithering)"] = null,
                ["Ordered"] = OrderedDitherer.Bayer8x8,
                ["Error Diffusion (raster)"] = ErrorDiffusionDitherer.FloydSteinberg,
                ["Error Diffusion (serpentine)"] = ErrorDiffusionDitherer.FloydSteinberg.ConfigureProcessingDirection(true),
            };

            foreach (var ditherer in ditherers)
            {
                var color = Color32.FromArgb((int)argb);
                var backColor = Color32.FromArgb((int)argbBackColor);
                using var bitmapData = BitmapDataFactory.CreateBitmapData(new Size(size, size), pixelFormat, backColor);
                bitmapData.Clear(color, ditherer.Value);

                SaveImage($"{pixelFormat} {argb:X8} on {argbBackColor:X8} {ditherer.Key}", bitmapData.ToBitmap());
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

            // raw access is completely disabled if clipping does not start on left edge
            Assert.AreEqual(0, bitmapData.Clip(new Rectangle(1, 0, 127, 1)).RowSize);
            Assert.AreEqual(0, bitmapData.Clip(new Rectangle(8, 0, 120, 1)).RowSize);
        }

        #endregion
    }
}
