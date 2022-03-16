#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ReadableBitmapDataExtensionsTest.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2021 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

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
            public PixelFormatInfo PixelFormat => wrapped.PixelFormat;
            public Palette Palette => wrapped.Palette;
            public int RowSize => wrapped.RowSize;
            public Color32 BackColor => wrapped.BackColor;
            public byte AlphaThreshold => wrapped.AlphaThreshold;
            public IReadableBitmapDataRow FirstRow => wrapped.FirstRow;
            public bool IsDisposed => wrapped.IsDisposed;

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
            using var clone = source.Clone(KnownPixelFormat.Format1bppIndexed);
            SaveImage(null, clone.ToBitmap());
        }

        [TestCase(KnownPixelFormat.Format4bppIndexed)]
        [TestCase(KnownPixelFormat.Format1bppIndexed)]
        public void CloneLowBppForcedDirectProcessingTest(KnownPixelFormat pixelFormat)
        {
            using var bmp = Icons.Information.ExtractBitmap(new Size(256, 256)).ConvertPixelFormat(pixelFormat.ToPixelFormat());
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

        [TestCase(KnownPixelFormat.Format32bppArgb)]
        [TestCase(KnownPixelFormat.Format16bppArgb1555)]
        [TestCase(KnownPixelFormat.Format16bppGrayScale)]
        [TestCase(KnownPixelFormat.Format8bppIndexed)]
        [TestCase(KnownPixelFormat.Format4bppIndexed)]
        [TestCase(KnownPixelFormat.Format1bppIndexed)]
        public void CloneWithPredefinedQuantizerTest(KnownPixelFormat pixelFormat)
        {
            using var bmp = Icons.Information.ExtractBitmap(new Size(256, 256));
            using var source = bmp.GetReadableBitmapData();
            using var clone = source.Clone(pixelFormat, PredefinedColorsQuantizer.FromPixelFormat(pixelFormat));
            SaveImage($"{pixelFormat}", clone.ToBitmap());
        }

        [TestCase(KnownPixelFormat.Format8bppIndexed)]
        [TestCase(KnownPixelFormat.Format4bppIndexed)]
        [TestCase(KnownPixelFormat.Format1bppIndexed)]
        public void CloneWithOptimizedQuantizerTest(KnownPixelFormat pixelFormat)
        {
            using var bmp = Icons.Information.ExtractBitmap(new Size(256, 256));
            using var source = bmp.GetReadableBitmapData();
            using var clone = source.Clone(pixelFormat, OptimizedPaletteQuantizer.Wu(1 << pixelFormat.ToBitsPerPixel()));
            SaveImage($"{pixelFormat}", clone.ToBitmap());
        }

        [TestCase(KnownPixelFormat.Format16bppArgb1555)]
        [TestCase(KnownPixelFormat.Format8bppIndexed)]
        [TestCase(KnownPixelFormat.Format4bppIndexed)]
        [TestCase(KnownPixelFormat.Format1bppIndexed)]
        public void CloneWithDithererTest(KnownPixelFormat pixelFormat)
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
                using var cloneTrueColor = source.Clone(KnownPixelFormat.Format32bppArgb, PredefinedColorsQuantizer.FromPixelFormat(pixelFormat), ditherer.Value);
                AssertAreEqual(cloneIndexed, cloneTrueColor, true);
                SaveImage($"{pixelFormat} {ditherer.Key}", cloneIndexed.ToBitmap());
            }
        }

        [TestCase(KnownPixelFormat.Format64bppArgb)]
        [TestCase(KnownPixelFormat.Format64bppPArgb)]
        [TestCase(KnownPixelFormat.Format48bppRgb)]
        [TestCase(KnownPixelFormat.Format32bppArgb)]
        [TestCase(KnownPixelFormat.Format32bppPArgb)]
        [TestCase(KnownPixelFormat.Format32bppRgb)]
        [TestCase(KnownPixelFormat.Format24bppRgb)]
        [TestCase(KnownPixelFormat.Format16bppRgb565)]
        [TestCase(KnownPixelFormat.Format16bppRgb555)]
        [TestCase(KnownPixelFormat.Format16bppArgb1555)]
        [TestCase(KnownPixelFormat.Format16bppGrayScale)]
        [TestCase(KnownPixelFormat.Format8bppIndexed)]
        [TestCase(KnownPixelFormat.Format4bppIndexed)]
        [TestCase(KnownPixelFormat.Format1bppIndexed)]
        public void CloneVsClipTest(KnownPixelFormat pixelFormat)
        {
            using var bitmapData = Icons.Information.ExtractBitmap(new Size(256, 256)).GetReadableBitmapData().Clone(pixelFormat);
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


        [TestCase(KnownPixelFormat.Format32bppArgb)]
        [TestCase(KnownPixelFormat.Format8bppIndexed)]
        [TestCase(KnownPixelFormat.Format1bppIndexed)]
        public void CopyToRawTest(KnownPixelFormat pixelFormat)
        {
            var rect = new Rectangle(128, 128, 128, 128);
            using var source = Icons.Information.ExtractBitmap(new Size(256, 256)).ConvertPixelFormat(pixelFormat.ToPixelFormat()).GetReadWriteBitmapData();
            using var targetFull = BitmapDataFactory.CreateBitmapData(source.GetSize(), pixelFormat);
            source.CopyTo(targetFull);
            AssertAreEqual(source, targetFull);

            using var targetClipped = BitmapDataFactory.CreateBitmapData(rect.Size, pixelFormat);
            source.CopyTo(targetClipped, rect, Point.Empty);
            AssertAreEqual(source, targetClipped, false, rect);

            SaveImage($"{pixelFormat} clipped", targetClipped.ToBitmap());
        }

        [TestCase(KnownPixelFormat.Format8bppIndexed)]
        [TestCase(KnownPixelFormat.Format4bppIndexed)]
        [TestCase(KnownPixelFormat.Format1bppIndexed)]
        public void CopyToDirectTest(KnownPixelFormat pixelFormat)
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

        [TestCase(KnownPixelFormat.Format8bppIndexed)]
        [TestCase(KnownPixelFormat.Format4bppIndexed)]
        [TestCase(KnownPixelFormat.Format1bppIndexed)]
        public void CopyToWithQuantizerTest(KnownPixelFormat pixelFormat)
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

        [TestCase(KnownPixelFormat.Format8bppIndexed)]
        [TestCase(KnownPixelFormat.Format4bppIndexed)]
        [TestCase(KnownPixelFormat.Format1bppIndexed)]
        public void CopyToWithDithererTest(KnownPixelFormat pixelFormat)
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


        [TestCase(KnownPixelFormat.Format1bppIndexed)]
        [TestCase(KnownPixelFormat.Format4bppIndexed)]
        [TestCase(KnownPixelFormat.Format8bppIndexed)]
        [TestCase(KnownPixelFormat.Format16bppArgb1555)]
        [TestCase(KnownPixelFormat.Format32bppArgb)]
        public void DrawIntoNoResizeDirectTest(KnownPixelFormat pixelFormat)
        {
            using var target = BitmapDataFactory.CreateBitmapData(new Size(256, 256), pixelFormat);
            using var icon64 = Icons.Information.ExtractBitmap(new Size(64, 64)).GetReadWriteBitmapData();
            using var icon256 = Icons.Information.ExtractBitmap(new Size(256, 256)).GetReadWriteBitmapData();
            using var gradient = GenerateAlphaGradientBitmapData(new Size(300, 300));

            // solid source
            icon64.Clone(KnownPixelFormat.Format24bppRgb, new Color32(Color.Silver))
                .DrawInto(target);

            // single bit alpha source
            icon64.Clone(KnownPixelFormat.Format16bppArgb1555, new Color32(Color.Silver))
                .DrawInto(target, new Point(192, 192));

            // alpha source
            icon256.DrawInto(target);

            // alpha gradient source
            gradient.DrawInto(target, new Rectangle(10, 10, 200, 200), new Point(32, 32));

            SaveImage($"{pixelFormat}", target.ToBitmap());
        }

        [TestCase(KnownPixelFormat.Format1bppIndexed)]
        [TestCase(KnownPixelFormat.Format4bppIndexed)]
        [TestCase(KnownPixelFormat.Format8bppIndexed)]
        [TestCase(KnownPixelFormat.Format16bppArgb1555)]
        public void DrawIntoNoResizeWithQuantizingTest(KnownPixelFormat pixelFormat)
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
                icon64.Clone(KnownPixelFormat.Format24bppRgb, new Color32(Color.Silver))
                    .DrawInto(target, Point.Empty, quantizer, ditherer.Value);

                // single bit alpha source
                icon64.Clone(KnownPixelFormat.Format16bppArgb1555, new Color32(Color.Silver))
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

        [TestCase(KnownPixelFormat.Format1bppIndexed, ScalingMode.NearestNeighbor)]
        [TestCase(KnownPixelFormat.Format1bppIndexed, ScalingMode.Auto)]
        [TestCase(KnownPixelFormat.Format4bppIndexed, ScalingMode.NearestNeighbor)]
        [TestCase(KnownPixelFormat.Format4bppIndexed, ScalingMode.Auto)]
        [TestCase(KnownPixelFormat.Format8bppIndexed, ScalingMode.NearestNeighbor)]
        [TestCase(KnownPixelFormat.Format8bppIndexed, ScalingMode.Auto)]
        [TestCase(KnownPixelFormat.Format16bppArgb1555, ScalingMode.NearestNeighbor)]
        [TestCase(KnownPixelFormat.Format16bppArgb1555, ScalingMode.Auto)]
        [TestCase(KnownPixelFormat.Format32bppArgb, ScalingMode.NearestNeighbor)]
        [TestCase(KnownPixelFormat.Format32bppArgb, ScalingMode.Auto)]
        [TestCase(KnownPixelFormat.Format32bppPArgb, ScalingMode.NearestNeighbor)]
        [TestCase(KnownPixelFormat.Format32bppPArgb, ScalingMode.Auto)]
        [TestCase(KnownPixelFormat.Format64bppArgb, ScalingMode.NearestNeighbor)]
        [TestCase(KnownPixelFormat.Format64bppArgb, ScalingMode.Auto)]
        [TestCase(KnownPixelFormat.Format64bppPArgb, ScalingMode.NearestNeighbor)]
        [TestCase(KnownPixelFormat.Format64bppPArgb, ScalingMode.Auto)]
        public void DrawIntoWithResizeDirectTest(KnownPixelFormat pixelFormat, ScalingMode scalingMode)
        {
            // target and sources
            using var target = BitmapDataFactory.CreateBitmapData(new Size(256, 256), pixelFormat, new Color32(Color.Silver));
            using var icon16 = Icons.Information.ExtractBitmap(new Size(16, 16)).GetReadWriteBitmapData();
            using var icon256 = Icons.Information.ExtractBitmap(new Size(256, 256)).GetReadWriteBitmapData();
            using var gradient = GenerateAlphaGradientBitmapData(new Size(256, 256));

            // enlarge solid source
            var targetRect = new Rectangle(0, 0, 100, 100);
            icon16.Clone(KnownPixelFormat.Format24bppRgb, new Color32(Color.Silver))
                .DrawInto(target, targetRect, scalingMode);

            // enlarge alpha source
            targetRect = new Rectangle(160, 160, 100, 100);
            icon16.DrawInto(target, targetRect, scalingMode);

            // shrink single bit alpha source
            targetRect = new Rectangle(Point.Empty, target.GetSize());
            targetRect.Inflate(-32, -32);
            icon256.Clone(KnownPixelFormat.Format16bppArgb1555)
                .DrawInto(target, targetRect, scalingMode);

            // shrink alpha source (gradient overlay)
            targetRect.Inflate(-10, -10);
            gradient.DrawInto(target, targetRect, scalingMode);

            SaveImage($"{pixelFormat} {scalingMode}", target.ToBitmap());
        }

        [TestCase(KnownPixelFormat.Format1bppIndexed, ScalingMode.NearestNeighbor)]
        [TestCase(KnownPixelFormat.Format1bppIndexed, ScalingMode.Auto)]
        [TestCase(KnownPixelFormat.Format4bppIndexed, ScalingMode.NearestNeighbor)]
        [TestCase(KnownPixelFormat.Format4bppIndexed, ScalingMode.Auto)]
        [TestCase(KnownPixelFormat.Format8bppIndexed, ScalingMode.NearestNeighbor)]
        [TestCase(KnownPixelFormat.Format8bppIndexed, ScalingMode.Auto)]
        [TestCase(KnownPixelFormat.Format16bppArgb1555, ScalingMode.NearestNeighbor)]
        [TestCase(KnownPixelFormat.Format16bppArgb1555, ScalingMode.Auto)]
        public void DrawIntoResizeWithQuantizingTest(KnownPixelFormat pixelFormat, ScalingMode scalingMode)
        {
            var quantizer = PredefinedColorsQuantizer.FromPixelFormat(pixelFormat, Color.Silver);

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
                icon16.Clone(KnownPixelFormat.Format24bppRgb, new Color32(Color.Silver))
                    .DrawInto(target, targetRect, quantizer, ditherer.Value, scalingMode);

                // enlarge alpha source
                targetRect = new Rectangle(160, 160, 100, 100);
                icon16.DrawInto(target, targetRect, quantizer, ditherer.Value, scalingMode);

                // shrink single bit alpha source
                targetRect = new Rectangle(Point.Empty, target.GetSize());
                targetRect.Inflate(-32, -32);
                icon256.Clone(KnownPixelFormat.Format16bppArgb1555)
                    .DrawInto(target, targetRect, quantizer, ditherer.Value, scalingMode);

                // shrink alpha source (gradient overlay)
                targetRect.Inflate(-10, -10);
                gradient.DrawInto(target, targetRect, quantizer, ditherer.Value, scalingMode);

                SaveImage($"{pixelFormat} {ditherer.Key} {scalingMode}", target.ToBitmap());
            }
        }

        [TestCase(KnownPixelFormat.Format1bppIndexed, 0xFFFFFFFF)]
        [TestCase(KnownPixelFormat.Format4bppIndexed, 0xFF0000FF)]
        [TestCase(KnownPixelFormat.Format8bppIndexed, 0xFF0000FF)]
        [TestCase(KnownPixelFormat.Format16bppGrayScale, 0xFF888888)]
        [TestCase(KnownPixelFormat.Format16bppRgb555, 0xFF0000FF)]
        [TestCase(KnownPixelFormat.Format16bppRgb565, 0xFF0000FF)]
        [TestCase(KnownPixelFormat.Format16bppArgb1555, 0xFF0000FF)]
        [TestCase(KnownPixelFormat.Format24bppRgb, 0xFF0000FF)]
        [TestCase(KnownPixelFormat.Format32bppRgb, 0xFF0000FF)]
        [TestCase(KnownPixelFormat.Format32bppRgb, 0x800000FF)]
        [TestCase(KnownPixelFormat.Format32bppArgb, 0xFF0000FF)]
        [TestCase(KnownPixelFormat.Format32bppArgb, 0x800000FF)]
        [TestCase(KnownPixelFormat.Format32bppPArgb, 0xFF0000FF)]
        [TestCase(KnownPixelFormat.Format32bppPArgb, 0x800000FF)]
        [TestCase(KnownPixelFormat.Format48bppRgb, 0xFF0000FF)]
        [TestCase(KnownPixelFormat.Format64bppArgb, 0xFF0000FF)]
        [TestCase(KnownPixelFormat.Format64bppPArgb, 0xFF0000FF)]
        public void ClearTest(KnownPixelFormat pixelFormat, uint argb)
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
                if (!pixelFormat.ToInfoInternal().HasMultiLevelAlpha)
                    expected = expected.BlendWithBackground(default);
                do
                {
                    for (int x = 0; x < source.BitmapData.Width; x++)
                        Assert.AreEqual(expected, row[x]);
                } while (row.MoveNextRow());

                //SaveImage($"{pixelFormat} {source.Name}", source.BitmapData.ToBitmap());
            }
        }

        [TestCase(KnownPixelFormat.Format1bppIndexed, 0xFF0000FF, 0U)]
        [TestCase(KnownPixelFormat.Format1bppIndexed, 0U, 0x88888888)]
        [TestCase(KnownPixelFormat.Format4bppIndexed, 0xFFABCDEF, 0U)]
        [TestCase(KnownPixelFormat.Format4bppIndexed, 0U, 0x88888888)]
        public void ClearWithDitheringTest(KnownPixelFormat pixelFormat, uint argb, uint argbBackColor)
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
            Assert.AreEqual(new Rectangle(50, 80, 50, 20), Reflector.GetProperty(clipped, "Region"));
            using var subClipped = clipped.Clip(new Rectangle(20, -10, 20, 50));
            Assert.AreEqual(new Rectangle(70, 80, 20, 20), Reflector.GetProperty(subClipped, "Region"));
        }

        [Test]
        public void ClippingIndexedTest()
        {
            using IReadWriteBitmapData bitmapData = BitmapDataFactory.CreateBitmapData(new Size(127, 1), KnownPixelFormat.Format1bppIndexed);

            // original
            Assert.AreEqual(16, bitmapData.RowSize);

            // clipped region makes last columns unavailable by raw data to protect the cut-out part from being overwritten
            Assert.AreEqual(15, bitmapData.Clip(new Rectangle(0, 0, 126, 1)).RowSize);

            // raw access is completely disabled if clipping does not start on left edge
            Assert.AreEqual(0, bitmapData.Clip(new Rectangle(1, 0, 127, 1)).RowSize);
            Assert.AreEqual(0, bitmapData.Clip(new Rectangle(8, 0, 120, 1)).RowSize);
        }

        [Test]
        public void GetColorsTest()
        {
            // 32 bit ARGB
            using var refBmpData = GenerateAlphaGradientBitmapData(new Size(512, 256));
            var colorCount = refBmpData.GetColorCount();
            Assert.LessOrEqual(colorCount, refBmpData.Width * refBmpData.Height);
            SaveImage("32argb", refBmpData.ToBitmap());

            // 24 bit
            using var bmp24bpp = refBmpData.Clone(KnownPixelFormat.Format24bppRgb);
            colorCount = bmp24bpp.GetColorCount();
            Assert.LessOrEqual(colorCount, bmp24bpp.Width * bmp24bpp.Height);
            SaveImage("24rgb", bmp24bpp.ToBitmap());

            // 48 bit
            using var bmp48bpp = refBmpData.Clone(KnownPixelFormat.Format48bppRgb);
            colorCount = bmp48bpp.GetColorCount();
            Assert.LessOrEqual(colorCount, bmp48bpp.Width * bmp48bpp.Height);
            SaveImage("48rgb", bmp48bpp.ToBitmap());

            // 64 bit
            using var bmp64bpp = refBmpData.Clone(KnownPixelFormat.Format64bppArgb);
            colorCount = bmp64bpp.GetColorCount();
            Assert.LessOrEqual(colorCount, bmp64bpp.Width * bmp64bpp.Height);
            SaveImage("64argb", bmp64bpp.ToBitmap());

            // 8 bit: returning actual palette
            using var bmp8bpp = refBmpData.Clone(KnownPixelFormat.Format8bppIndexed);
            colorCount = bmp8bpp.GetColorCount();
            Assert.LessOrEqual(colorCount, 256);
            SaveImage("8ind", bmp8bpp.ToBitmap());
        }

        [Test]
        public void ToTransparentTest()
        {
            using var refBmpData = Icons.Information.ExtractBitmap(new Size(256, 256))
                .GetReadableBitmapData()
                .Clone(KnownPixelFormat.Format24bppRgb, new Color32(Color.Silver));
            SaveImage("reference", refBmpData.ToBitmap());

            using var transparentAuto = refBmpData.ToTransparent();
            Assert.AreEqual(default(Color32), transparentAuto[0][0]);

            SaveImage("transparent", transparentAuto.ToBitmap());

            using var transparentDirect = refBmpData.ToTransparent(new Color32(Color.Silver));
            AssertAreEqual(transparentAuto, transparentDirect);
        }

        [Test]
        public void TrySetPaletteTest()
        {
            using var bmpData = Icons.Information.ExtractBitmap(new Size(256, 256))
                .GetReadableBitmapData()
                .Clone(KnownPixelFormat.Format1bppIndexed);
            SaveImage("BW", bmpData.ToBitmap());

            Assert.IsTrue(bmpData.TrySetPalette(new Palette(new[] { Color.Transparent, Color.Blue })));
            Assert.AreEqual(new Color32(Color.Transparent), bmpData[0][0]);
            SaveImage("transparent-blue", bmpData.ToBitmap());

            // too many colors for 1bpp
            Assert.IsFalse(bmpData.TrySetPalette(new Palette(new[] { Color.Transparent, Color.Blue, Color.Red })));
        }

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
        public void SaveReloadManagedTest(KnownPixelFormat pixelFormat)
        {
            var size = new Size(13, 10);
            using IReadWriteBitmapData orig = BitmapDataFactory.CreateBitmapData(size, pixelFormat);
            GenerateAlphaGradient(orig);

            using var ms = new MemoryStream();
            orig.Save(ms);
            ms.Position = 0;
            IReadWriteBitmapData clone = BitmapDataFactory.Load(ms);
            AssertAreEqual(orig, clone);
            SaveImage($"{pixelFormat}", clone.ToBitmap());
        }

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
        public void SaveReloadClippedTest(KnownPixelFormat pixelFormat)
        {
            var size = new Size(16, 16);
            using IReadWriteBitmapData orig = BitmapDataFactory.CreateBitmapData(size, pixelFormat).Clip(new Rectangle(1, 1, 13, 10));
            GenerateAlphaGradient(orig);

            using var ms = new MemoryStream();
            orig.Save(ms);
            ms.Position = 0;
            IReadWriteBitmapData clone = BitmapDataFactory.Load(ms);
            AssertAreEqual(orig, clone);
            SaveImage($"{pixelFormat}", clone.ToBitmap());
        }

        #endregion
    }
}
