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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

using KGySoft.CoreLibraries;
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
            public IReadableBitmapDataRowMovable FirstRow => wrapped.FirstRow;
            public bool IsDisposed => wrapped.IsDisposed;
            public Size Size => wrapped.Size;
            public WorkingColorSpace WorkingColorSpace => wrapped.WorkingColorSpace;

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
            public Color32 GetColor32(int x, int y) => wrapped.GetColor32(x, y);
            public PColor32 GetPColor32(int x, int y) => wrapped.GetPColor32(x, y);
            public Color64 GetColor64(int x, int y) => wrapped.GetColor64(x, y);
            public PColor64 GetPColor64(int x, int y) => wrapped.GetPColor64(x, y);
            public ColorF GetColorF(int x, int y) => wrapped.GetColorF(x, y);
            public PColorF GetPColorF(int x, int y) => wrapped.GetPColorF(x, y);
            public int GetColorIndex(int x, int y) => wrapped.GetColorIndex(x, y);
            public T ReadRaw<T>(int x, int y) where T : unmanaged => wrapped.ReadRaw<T>(x, y);
            public IReadableBitmapDataRowMovable GetMovableRow(int y) => wrapped.GetMovableRow(y);

            #endregion
        }

        #endregion

        #endregion

        #region Methods

        [Test]
        public void CloneDecreasingPaletteTest()
        {
            using var source = GetInfoIcon256().Clone(KnownPixelFormat.Format8bppIndexed);
            using var clone = source.Clone(KnownPixelFormat.Format1bppIndexed);
            SaveBitmapData(null, clone);
        }

        [TestCase(KnownPixelFormat.Format4bppIndexed)]
        [TestCase(KnownPixelFormat.Format1bppIndexed)]
        public void CloneLowBppForcedDirectProcessingTest(KnownPixelFormat pixelFormat)
        {
            using var bitmapData = GetInfoIcon256().Clone(pixelFormat);
            var sourceRectangle = new Rectangle(15, 15, 127, 127);
            using IReadWriteBitmapData clone = bitmapData.Clone(sourceRectangle, pixelFormat);
            AssertAreEqual(bitmapData, clone, false, sourceRectangle);
            SaveBitmapData($"{pixelFormat} - Clipped", clone);
        }

        [TestCase(KnownPixelFormat.Format24bppRgb)]
        [TestCase(KnownPixelFormat.Format4bppIndexed)]
        public void CloneDirectPreserveBackColorTest(KnownPixelFormat pixelFormat)
        {
            string file = @"..\..\..\..\Help\Images\Information256.png";
            Color32 backColor = Color.Green;

            using var bitmapData = GetBitmapData(file, backColor:backColor);
            using var clone = bitmapData.Clone(pixelFormat, quantizer: null);

            Assert.AreEqual(backColor, clone.GetColor32(0, 0));
            SaveBitmapData($"{pixelFormat}", clone);
        }

        [TestCase(KnownPixelFormat.Format4bppIndexed)]
        public void CloneByDitheringPreserveBackColorTest(KnownPixelFormat pixelFormat)
        {
            string file = @"..\..\..\..\Help\Images\Information256.png";
            Color32 backColor = Color.Green;

            using var bitmapData = GetBitmapData(file, backColor: backColor);
            using var clone = bitmapData.Clone(pixelFormat, OrderedDitherer.Bayer4x4);

            Assert.AreEqual(backColor, clone.GetColor32(0, 0));
            SaveBitmapData($"{pixelFormat}", clone);
        }

        [Test]
        public void CloneWithWrappedDataTest()
        {
            using var bitmapData = new TestReadableBitmapData(GetInfoIcon256());
            using IReadWriteBitmapData clone = bitmapData.Clone();
            AssertAreEqual(bitmapData, clone, bitmapData.PixelFormat.IsCustomFormat);
            //SaveBitmapData("Clone", clone);
        }

        [TestCase(KnownPixelFormat.Format32bppArgb)]
        [TestCase(KnownPixelFormat.Format16bppArgb1555)]
        [TestCase(KnownPixelFormat.Format8bppGrayScale)]
        [TestCase(KnownPixelFormat.Format16bppGrayScale)]
        [TestCase(KnownPixelFormat.Format32bppGrayScale)]
        [TestCase(KnownPixelFormat.Format8bppIndexed)]
        [TestCase(KnownPixelFormat.Format4bppIndexed)]
        [TestCase(KnownPixelFormat.Format1bppIndexed)]
        public void CloneWithPredefinedQuantizerTest(KnownPixelFormat pixelFormat)
        {
            using var source = GetInfoIcon256();
            using (var clone = source.Clone(pixelFormat, PredefinedColorsQuantizer.FromPixelFormat(pixelFormat)))
                SaveBitmapData($"{pixelFormat} - sRGB blending", clone);
            using (var clone = source.Clone(pixelFormat, PredefinedColorsQuantizer.FromPixelFormat(pixelFormat).ConfigureColorSpace(WorkingColorSpace.Linear)))
                SaveBitmapData($"{pixelFormat} - Linear blending", clone);
        }

        [TestCase(KnownPixelFormat.Format8bppIndexed)]
        [TestCase(KnownPixelFormat.Format4bppIndexed)]
        [TestCase(KnownPixelFormat.Format1bppIndexed)]
        public void CloneWithOptimizedQuantizerTest(KnownPixelFormat pixelFormat)
        {
            using var source = GetInfoIcon256();
            using (var clone = source.Clone(pixelFormat, OptimizedPaletteQuantizer.Wu(1 << pixelFormat.ToBitsPerPixel())))
                SaveBitmapData($"{pixelFormat} - sRGB blending", clone);
            using (var clone = source.Clone(pixelFormat, OptimizedPaletteQuantizer.Wu(1 << pixelFormat.ToBitsPerPixel()).ConfigureColorSpace(WorkingColorSpace.Linear)))
                SaveBitmapData($"{pixelFormat} - Linear blending", clone);
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

            using var source = GetInfoIcon256();
            foreach (var ditherer in ditherers)
            {
                foreach (WorkingColorSpace colorSpace in new[] { WorkingColorSpace.Srgb, WorkingColorSpace.Linear })
                {
                    using var cloneIndexed = source.Clone(pixelFormat, PredefinedColorsQuantizer.FromPixelFormat(pixelFormat).ConfigureColorSpace(colorSpace), ditherer.Value);
                    using var cloneTrueColor = source.Clone(KnownPixelFormat.Format32bppArgb, PredefinedColorsQuantizer.FromPixelFormat(pixelFormat).ConfigureColorSpace(colorSpace), ditherer.Value);
                    AssertAreEqual(cloneIndexed, cloneTrueColor, true);
                    SaveBitmapData($"{pixelFormat} {ditherer.Key} {colorSpace}", cloneIndexed);
                }
            }
        }

        [TestCase(KnownPixelFormat.Format128bppRgba)]
        [TestCase(KnownPixelFormat.Format128bppPRgba)]
        [TestCase(KnownPixelFormat.Format96bppRgb)]
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
        [TestCase(KnownPixelFormat.Format8bppGrayScale)]
        [TestCase(KnownPixelFormat.Format16bppGrayScale)]
        [TestCase(KnownPixelFormat.Format32bppGrayScale)]
        [TestCase(KnownPixelFormat.Format8bppIndexed)]
        [TestCase(KnownPixelFormat.Format4bppIndexed)]
        [TestCase(KnownPixelFormat.Format1bppIndexed)]
        public void CloneVsClipTest(KnownPixelFormat pixelFormat)
        {
            using var bitmapData = GetInfoIcon256().Clone(pixelFormat);
            using (IReadWriteBitmapData clone = bitmapData.Clone())
            {
                AssertAreEqual(bitmapData, clone);
                SaveBitmapData($"{pixelFormat} - Complete clone", clone);
            }

            var sourceRectangle = new Rectangle(16, 16, 128, 128);
            using (IReadWriteBitmapData clone = bitmapData.Clone(sourceRectangle, pixelFormat))
            {
                AssertAreEqual(bitmapData, clone, false, sourceRectangle);
                SaveBitmapData($"{pixelFormat} - Clipped clone", clone);
            }

            using (IReadableBitmapData clip = bitmapData.Clip(sourceRectangle))
            {
                AssertAreEqual(bitmapData, clip, false, sourceRectangle);
                //SaveBitmapData($"{pixelFormat} - Clipping wrapper", clip);
            }
        }

        [TestCase(KnownPixelFormat.Format16bppGrayScale)]
        [TestCase(KnownPixelFormat.Format32bppGrayScale)]
        [TestCase(KnownPixelFormat.Format48bppRgb)]
        [TestCase(KnownPixelFormat.Format64bppArgb)]
        [TestCase(KnownPixelFormat.Format64bppPArgb)]
        [TestCase(KnownPixelFormat.Format96bppRgb)]
        [TestCase(KnownPixelFormat.Format128bppRgba)]
        [TestCase(KnownPixelFormat.Format128bppPRgba)]
        public void CloneDirectTest(KnownPixelFormat sourceFormat)
        {
            Console.WriteLine(sourceFormat);
            var size = new Size(2048, 256);
            using IReadWriteBitmapData source = BitmapDataFactory.CreateBitmapData(size, sourceFormat);
            GenerateAlphaGradientLinear(source);

            int baselineColors;

            // baseline: 8 bpp PARGB
            using (var bmp32 = source.Clone(KnownPixelFormat.Format32bppPArgb))
            {
                baselineColors = bmp32.GetColorCount();
                Console.WriteLine($"As {bmp32.PixelFormat}: {baselineColors} colors");
                SaveBitmapData($"{sourceFormat}_to_{bmp32.PixelFormat}", bmp32);
            }

            foreach (KnownPixelFormat targetFormat in new[] { KnownPixelFormat.Format32bppArgb, KnownPixelFormat.Format64bppArgb, KnownPixelFormat.Format64bppPArgb, KnownPixelFormat.Format128bppRgba, KnownPixelFormat.Format128bppPRgba })
            {
                using (var target = source.Clone(targetFormat))
                {
                    int count = target.GetColorCount();
                    Console.WriteLine($"As {target.PixelFormat}: {count} colors");
                    SaveBitmapData($"{sourceFormat}_to_{target.PixelFormat}", target);
                    Assert.GreaterOrEqual(count, baselineColors);
                }
            }
        }

        [Test]
        public void CopyToSameInstanceOverlappingTest()
        {
            using IReadWriteBitmapData bitmapData = GetInfoIcon256();
            Assert.DoesNotThrow(() => bitmapData.CopyTo(bitmapData, new Point(64, 64)));
            SaveBitmapData(null, bitmapData);
        }

        [Test]
        public void CopyToSameInstanceOverlappingByClippingTest()
        {
            using var bitmapData = GetInfoIcon256();
            using var clipSrc = bitmapData.Clip(new Rectangle(32, 32, 128, 128));
            using var clipDst = bitmapData.Clip(new Rectangle(64, 64, 128, 128));

            Assert.DoesNotThrow(() => clipSrc.CopyTo(clipDst, new Point(32, 32)));

            SaveBitmapData(null, bitmapData);
        }


        [TestCase(KnownPixelFormat.Format32bppArgb)]
        [TestCase(KnownPixelFormat.Format8bppIndexed)]
        [TestCase(KnownPixelFormat.Format1bppIndexed)]
        public void CopyToRawTest(KnownPixelFormat pixelFormat)
        {
            var rect = new Rectangle(128, 128, 128, 128);
            using var source = GetInfoIcon256().Clone(pixelFormat);
            using var targetFull = BitmapDataFactory.CreateBitmapData(source.Size, pixelFormat);
            source.CopyTo(targetFull);
            AssertAreEqual(source, targetFull);

            using var targetClipped = BitmapDataFactory.CreateBitmapData(rect.Size, pixelFormat);
            source.CopyTo(targetClipped, rect, Point.Empty);
            AssertAreEqual(source, targetClipped, false, rect);

            SaveBitmapData($"{pixelFormat} clipped", targetClipped);
        }

        [TestCase(KnownPixelFormat.Format16bppArgb1555)]
        [TestCase(KnownPixelFormat.Format8bppGrayScale)]
        [TestCase(KnownPixelFormat.Format8bppIndexed)]
        [TestCase(KnownPixelFormat.Format4bppIndexed)]
        [TestCase(KnownPixelFormat.Format1bppIndexed)]
        public void CopyToDirectTest(KnownPixelFormat pixelFormat)
        {
            foreach (var colorSpace in new[] { WorkingColorSpace.Srgb, WorkingColorSpace.Linear})
            {
                var rect = new Rectangle(128, 128, 128, 128);
                using var source = GetInfoIcon256();
                using var targetFull = BitmapDataFactory.CreateBitmapData(source.Size, pixelFormat, colorSpace);
                source.CopyTo(targetFull);
                //SaveBitmapData($"{pixelFormat} target", targetFull);

                using var targetClipped = BitmapDataFactory.CreateBitmapData(rect.Size, pixelFormat, colorSpace);
                source.CopyTo(targetClipped, rect, Point.Empty);

                AssertAreEqual(targetFull, targetClipped, false, rect);

                SaveBitmapData($"{pixelFormat} {colorSpace}", targetClipped); 
            }
        }

        [TestCase(KnownPixelFormat.Format16bppArgb1555)]
        [TestCase(KnownPixelFormat.Format8bppGrayScale)]
        [TestCase(KnownPixelFormat.Format8bppIndexed)]
        [TestCase(KnownPixelFormat.Format4bppIndexed)]
        [TestCase(KnownPixelFormat.Format1bppIndexed)]
        public void CopyToWithQuantizerTest(KnownPixelFormat pixelFormat)
        {
            foreach (var colorSpace in new[] { WorkingColorSpace.Srgb, WorkingColorSpace.Linear })
            {
                var rect = new Rectangle(128, 128, 128, 128);
                using var source = GetInfoIcon256();
                using var targetFull = BitmapDataFactory.CreateBitmapData(source.Size);
                var quantizer = PredefinedColorsQuantizer.FromPixelFormat(pixelFormat).ConfigureColorSpace(colorSpace);
                source.CopyTo(targetFull, Point.Empty, quantizer);

                using var targetClipped = BitmapDataFactory.CreateBitmapData(rect.Size);
                source.CopyTo(targetClipped, rect, Point.Empty, quantizer);
                AssertAreEqual(targetFull, targetClipped, false, rect);

                SaveBitmapData($"{pixelFormat} {colorSpace}", targetClipped);
            }
        }

        [TestCase(KnownPixelFormat.Format8bppIndexed)]
        [TestCase(KnownPixelFormat.Format4bppIndexed)]
        [TestCase(KnownPixelFormat.Format1bppIndexed)]
        public void CopyToWithDithererTest(KnownPixelFormat pixelFormat)
        {
            var rect = new Rectangle(128, 128, 128, 128);
            using var source = GetInfoIcon256();
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
                SaveBitmapData($"{pixelFormat} {ditherer.Key}", targetClipped);
            }
        }

        [Test]
        public void DrawIntoWithoutResizeSameInstanceOverlappingTest()
        {
            using var bitmapData = GetInfoIcon256();
            Assert.DoesNotThrow(() => bitmapData.DrawInto(bitmapData, new Point(64, 64)));
            SaveBitmapData(null, bitmapData);
        }

        [TestCase(KnownPixelFormat.Format1bppIndexed)]
        [TestCase(KnownPixelFormat.Format4bppIndexed)]
        [TestCase(KnownPixelFormat.Format8bppIndexed)]
        [TestCase(KnownPixelFormat.Format8bppGrayScale)]
        [TestCase(KnownPixelFormat.Format16bppGrayScale)]
        [TestCase(KnownPixelFormat.Format32bppGrayScale)]
        [TestCase(KnownPixelFormat.Format16bppArgb1555)]
        [TestCase(KnownPixelFormat.Format32bppArgb)]
        [TestCase(KnownPixelFormat.Format32bppPArgb)]
        [TestCase(KnownPixelFormat.Format24bppRgb)]
        [TestCase(KnownPixelFormat.Format64bppArgb)]
        [TestCase(KnownPixelFormat.Format64bppPArgb)]
        [TestCase(KnownPixelFormat.Format48bppRgb)]
        [TestCase(KnownPixelFormat.Format128bppRgba)]
        [TestCase(KnownPixelFormat.Format128bppPRgba)]
        [TestCase(KnownPixelFormat.Format96bppRgb)]
        public void DrawIntoNoResizeDirectTest(KnownPixelFormat pixelFormat)
        {
            foreach (var colorSpace in new[] { WorkingColorSpace.Srgb, WorkingColorSpace.Linear })
            {
                using var target = BitmapDataFactory.CreateBitmapData(new Size(256, 256), pixelFormat, colorSpace);
                using var icon64 = GetInfoIcon64();
                using var icon256 = GetInfoIcon256();
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

                SaveBitmapData($"{pixelFormat} {colorSpace}", target);
            }
        }

        [TestCase(KnownPixelFormat.Format1bppIndexed)]
        [TestCase(KnownPixelFormat.Format4bppIndexed)]
        [TestCase(KnownPixelFormat.Format8bppIndexed)]
        [TestCase(KnownPixelFormat.Format8bppGrayScale)]
        [TestCase(KnownPixelFormat.Format16bppArgb1555)]
        public void DrawIntoNoResizeWithQuantizingTest(KnownPixelFormat pixelFormat)
        {
            foreach (var colorSpace in new[] { WorkingColorSpace.Srgb, WorkingColorSpace.Linear })
            {
                var quantizer = PredefinedColorsQuantizer.FromPixelFormat(pixelFormat).ConfigureColorSpace(colorSpace);

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
                    using var icon64 = GetInfoIcon64();
                    using var icon256 = GetInfoIcon256();
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

                    SaveBitmapData($"{pixelFormat} {ditherer.Key} {colorSpace}", target);
                }

            }
        }

        [TestCase(ScalingMode.Auto)]
        [TestCase(ScalingMode.NearestNeighbor)]
        public void DrawIntoWithResizeSameInstanceOverlappingTest(ScalingMode scalingMode)
        {
            using var bitmapData = GetInfoIcon256();
            Assert.DoesNotThrow(() => bitmapData.DrawInto(bitmapData, new Rectangle(32, 32, 192, 192), new Rectangle(64, 64, 128, 128), scalingMode));
            SaveBitmapData($"{scalingMode}", bitmapData);
        }

        [TestCase(KnownPixelFormat.Format1bppIndexed)]
        [TestCase(KnownPixelFormat.Format4bppIndexed)]
        [TestCase(KnownPixelFormat.Format8bppIndexed)]
        [TestCase(KnownPixelFormat.Format8bppGrayScale)]
        [TestCase(KnownPixelFormat.Format16bppGrayScale)]
        [TestCase(KnownPixelFormat.Format32bppGrayScale)]
        [TestCase(KnownPixelFormat.Format16bppArgb1555)]
        [TestCase(KnownPixelFormat.Format24bppRgb)]
        [TestCase(KnownPixelFormat.Format32bppArgb)]
        [TestCase(KnownPixelFormat.Format32bppPArgb)]
        [TestCase(KnownPixelFormat.Format48bppRgb)]
        [TestCase(KnownPixelFormat.Format64bppArgb)]
        [TestCase(KnownPixelFormat.Format64bppPArgb)]
        [TestCase(KnownPixelFormat.Format96bppRgb)]
        [TestCase(KnownPixelFormat.Format128bppRgba)]
        [TestCase(KnownPixelFormat.Format128bppPRgba)]
        public void DrawIntoWithResizeDirectTest(KnownPixelFormat pixelFormat)
        {
            foreach (ScalingMode scalingMode in new[] { ScalingMode.NearestNeighbor, ScalingMode.Auto })
            {
                foreach (var colorSpace in new[] { WorkingColorSpace.Srgb, WorkingColorSpace.Linear })
                {
                    // target and sources
                    using var target = BitmapDataFactory.CreateBitmapData(new Size(256, 256), pixelFormat, colorSpace, new Color32(Color.Silver));
                    using var icon16 = GetInfoIcon16();
                    using var icon256 = GetInfoIcon256();
                    using var gradient = GenerateAlphaGradientBitmapData(new Size(256, 256));

                    // enlarge solid source
                    var targetRect = new Rectangle(0, 0, 100, 100);
                    icon16.Clone(KnownPixelFormat.Format24bppRgb, new Color32(Color.Black))
                        .DrawInto(target, targetRect, scalingMode);

                    // enlarge alpha source
                    targetRect = new Rectangle(160, 160, 100, 100);
                    icon16.DrawInto(target, targetRect, scalingMode);

                    // shrink single bit alpha source
                    targetRect = new Rectangle(Point.Empty, target.Size);
                    targetRect.Inflate(-32, -32);
                    icon256.Clone(KnownPixelFormat.Format16bppArgb1555)
                        .DrawInto(target, targetRect, scalingMode);

                    // shrink alpha source (gradient overlay)
                    targetRect.Inflate(-10, -10);
                    gradient.DrawInto(target, targetRect, scalingMode);

                    SaveBitmapData($"{pixelFormat} {scalingMode} {colorSpace}", target);
                } 
            }
        }

        [TestCase(KnownPixelFormat.Format1bppIndexed, ScalingMode.NearestNeighbor)]
        [TestCase(KnownPixelFormat.Format1bppIndexed, ScalingMode.Auto)]
        [TestCase(KnownPixelFormat.Format4bppIndexed, ScalingMode.NearestNeighbor)]
        [TestCase(KnownPixelFormat.Format4bppIndexed, ScalingMode.Auto)]
        [TestCase(KnownPixelFormat.Format8bppIndexed, ScalingMode.NearestNeighbor)]
        [TestCase(KnownPixelFormat.Format8bppIndexed, ScalingMode.Auto)]
        [TestCase(KnownPixelFormat.Format16bppArgb1555, ScalingMode.NearestNeighbor)]
        [TestCase(KnownPixelFormat.Format16bppArgb1555, ScalingMode.Auto)]
        [TestCase(KnownPixelFormat.Format24bppRgb, ScalingMode.NearestNeighbor)]
        [TestCase(KnownPixelFormat.Format24bppRgb, ScalingMode.Auto)]
        public void DrawIntoResizeWithQuantizingTest(KnownPixelFormat pixelFormat, ScalingMode scalingMode)
        {
            foreach (var colorSpace in new[] { WorkingColorSpace.Srgb, WorkingColorSpace.Linear })
            {
                var quantizer = PredefinedColorsQuantizer.FromPixelFormat(pixelFormat, Color.Silver).ConfigureColorSpace(colorSpace);

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
                    using var icon16 = GetInfoIcon16();
                    using var icon256 = GetInfoIcon256();
                    using var gradient = GenerateAlphaGradientBitmapData(new Size(256, 256));

                    // enlarge solid source
                    var targetRect = new Rectangle(-10, -10, 100, 100);
                    icon16.Clone(KnownPixelFormat.Format24bppRgb, new Color32(Color.Silver))
                        .DrawInto(target, targetRect, quantizer, ditherer.Value, scalingMode);

                    // enlarge alpha source
                    targetRect = new Rectangle(160, 160, 100, 100);
                    icon16.DrawInto(target, targetRect, quantizer, ditherer.Value, scalingMode);

                    // shrink single bit alpha source
                    targetRect = new Rectangle(Point.Empty, target.Size);
                    targetRect.Inflate(-32, -32);
                    icon256.Clone(KnownPixelFormat.Format16bppArgb1555)
                        .DrawInto(target, targetRect, quantizer, ditherer.Value, scalingMode);

                    // shrink alpha source (gradient overlay)
                    targetRect.Inflate(-10, -10);
                    gradient.DrawInto(target, targetRect, quantizer, ditherer.Value, scalingMode);

                    SaveBitmapData($"{pixelFormat} {ditherer.Key} {scalingMode} {colorSpace}", target);
                } 
            }
        }

        [TestCase(KnownPixelFormat.Format1bppIndexed, 0xFFFFFFFF)]
        [TestCase(KnownPixelFormat.Format4bppIndexed, 0xFF0000FF)]
        [TestCase(KnownPixelFormat.Format8bppIndexed, 0xFF0000FF)]
        [TestCase(KnownPixelFormat.Format8bppGrayScale, 0xFF888888)]
        [TestCase(KnownPixelFormat.Format16bppGrayScale, 0xFF888888)]
        [TestCase(KnownPixelFormat.Format32bppGrayScale, 0xFF888888)]
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
        [TestCase(KnownPixelFormat.Format96bppRgb, 0xFF0000FF)]
        [TestCase(KnownPixelFormat.Format128bppRgba, 0xFF0000FF)]
        [TestCase(KnownPixelFormat.Format128bppPRgba, 0xFF0000FF)]
        public void ClearTest(KnownPixelFormat pixelFormat, uint argb)
        {
            const int size = 17;
            Color32 color = Color32.FromArgb((int)argb);

            foreach (var colorSpace in new[] { WorkingColorSpace.Srgb, WorkingColorSpace.Linear })
            {
                using var bitmapData = BitmapDataFactory.CreateBitmapData(new Size(size, size), pixelFormat, colorSpace);
                (string Name, IReadWriteBitmapData BitmapData)[] sources = new[]
                {
                    ("full", bitmapData),
                    ($"clipped right, width={size - 1}", bitmapData.Clone().Clip(new Rectangle(0, 0, size - 1, 1))),
                    ($"clipped right, width={size - 2}", bitmapData.Clone().Clip(new Rectangle(0, 0, size - 2, 1))),
                    ("clipped left", bitmapData.Clone().Clip(new Rectangle(1, 0, size - 1, 1))),
                };

                foreach ((string name, IReadWriteBitmapData target) in sources)
                {
                    target.Clear(color);

                    IReadableBitmapDataRowMovable row = target.FirstRow;
                    var expected = color;
                    if (!pixelFormat.ToInfoInternal().HasMultiLevelAlpha && expected.A != Byte.MaxValue)
                        expected = expected.BlendWithBackground(Color32.Black, colorSpace);
                    do
                    {
                        for (int x = 0; x < target.Width; x++)
                            Assert.AreEqual(expected, row[x]);
                    } while (row.MoveNextRow());

                    //SaveBitmapData($"{pixelFormat} {name} {colorSpace}", target);
                }
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

                SaveBitmapData($"{pixelFormat} {argb:X8} on {argbBackColor:X8} {ditherer.Key}", bitmapData);
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
            // 128 bit reference
            using var refBmpData = GenerateAlphaGradientBitmapData(new Size(512, 256), true);
            var colorCount = refBmpData.GetColorCount();
            Console.WriteLine($"{refBmpData.PixelFormat} - {colorCount}");
            Assert.LessOrEqual(colorCount, refBmpData.Width * refBmpData.Height);
            SaveBitmapData("128rgba", refBmpData);

            // 96 bit
            using var bmp96bpp = refBmpData.Clone(KnownPixelFormat.Format96bppRgb);
            colorCount = bmp96bpp.GetColorCount();
            Console.WriteLine($"{bmp96bpp.PixelFormat} - {colorCount}");
            Assert.LessOrEqual(colorCount, bmp96bpp.Width * bmp96bpp.Height);
            SaveBitmapData("96rgb", bmp96bpp);

            // 32 bit
            using var bmp32bpp = refBmpData.Clone(KnownPixelFormat.Format32bppArgb);
            colorCount = bmp32bpp.GetColorCount();
            Console.WriteLine($"{bmp32bpp.PixelFormat} - {colorCount}");
            Assert.LessOrEqual(colorCount, bmp32bpp.Width * bmp32bpp.Height);
            SaveBitmapData("32argb", bmp32bpp);

            // 24 bit
            using var bmp24bpp = refBmpData.Clone(KnownPixelFormat.Format24bppRgb);
            colorCount = bmp24bpp.GetColorCount();
            Console.WriteLine($"{bmp24bpp.PixelFormat} - {colorCount}");
            Assert.LessOrEqual(colorCount, bmp24bpp.Width * bmp24bpp.Height);
            SaveBitmapData("24rgb", bmp24bpp);

            // 48 bit
            using var bmp48bpp = refBmpData.Clone(KnownPixelFormat.Format48bppRgb);
            colorCount = bmp48bpp.GetColorCount();
            Console.WriteLine($"{bmp48bpp.PixelFormat} - {colorCount}");
            Assert.LessOrEqual(colorCount, bmp48bpp.Width * bmp48bpp.Height);
            SaveBitmapData("48rgb", bmp48bpp);

            // 64 bit
            using var bmp64bpp = refBmpData.Clone(KnownPixelFormat.Format64bppArgb);
            colorCount = bmp64bpp.GetColorCount();
            Console.WriteLine($"{bmp64bpp.PixelFormat} - {colorCount}");
            Assert.LessOrEqual(colorCount, bmp64bpp.Width * bmp64bpp.Height);
            SaveBitmapData("64argb", bmp64bpp);

            // 8 bit indexed: returning actual palette
            using var bmp8bpp = refBmpData.Clone(KnownPixelFormat.Format8bppIndexed);
            colorCount = bmp8bpp.GetColorCount();
            Console.WriteLine($"{bmp8bpp.PixelFormat} - {colorCount}");
            Assert.LessOrEqual(colorCount, 256);

            // 8 bit gray
            using var bmp8bppGray = refBmpData.Clone(KnownPixelFormat.Format8bppGrayScale);
            colorCount = bmp8bppGray.GetColorCount();
            Console.WriteLine($"{bmp8bppGray.PixelFormat} - {colorCount}");
            Assert.LessOrEqual(colorCount, 256);
            SaveBitmapData("8gray", bmp8bppGray);

            // 16 bit gray
            using var bmp16bppGray = refBmpData.Clone(KnownPixelFormat.Format16bppGrayScale);
            colorCount = bmp16bppGray.GetColorCount();
            Console.WriteLine($"{bmp16bppGray.PixelFormat} - {colorCount}");
            Assert.LessOrEqual(colorCount, 65536);
            SaveBitmapData("16gray", bmp16bppGray);

            // 32 bit gray
            using var bmp32bppGray = refBmpData.Clone(KnownPixelFormat.Format32bppGrayScale);
            colorCount = bmp32bppGray.GetColorCount();
            Console.WriteLine($"{bmp32bppGray.PixelFormat} - {colorCount}");
            Assert.LessOrEqual(colorCount, bmp32bppGray.Width * bmp32bppGray.Height);
            SaveBitmapData("32gray", bmp32bppGray);
        }

        [TestCase(KnownPixelFormat.Format32bppArgb, WorkingColorSpace.Default)]
        [TestCase(KnownPixelFormat.Format32bppArgb, WorkingColorSpace.Linear)]
        [TestCase(KnownPixelFormat.Format24bppRgb, WorkingColorSpace.Default)]
        [TestCase(KnownPixelFormat.Format24bppRgb, WorkingColorSpace.Linear)]
        [TestCase(KnownPixelFormat.Format64bppArgb, WorkingColorSpace.Default)]
        [TestCase(KnownPixelFormat.Format64bppArgb, WorkingColorSpace.Linear)]
        [TestCase(KnownPixelFormat.Format128bppRgba, WorkingColorSpace.Default)]
        [TestCase(KnownPixelFormat.Format128bppRgba, WorkingColorSpace.Srgb)]
        [TestCase(KnownPixelFormat.Format4bppIndexed, WorkingColorSpace.Linear)]
        [TestCase(KnownPixelFormat.Format4bppIndexed, WorkingColorSpace.Srgb)]
        public void ToGrayscaleTest(KnownPixelFormat pixelFormat, WorkingColorSpace colorSpace)
        {
            using var refBmpData = GetInfoIcon256()
                .Clone(pixelFormat, colorSpace, new Color32(Color.Silver));

            using var grayscale = refBmpData.ToGrayscale();

            SaveBitmapData($"{pixelFormat}_{colorSpace}", grayscale);

            refBmpData.MakeGrayscale();
            AssertAreEqual(refBmpData, grayscale, true, tolerance: 1);
        }

        [Test]
        public void ToTransparentTest()
        {
            using var refBmpData = GetInfoIcon256()
                .Clone(KnownPixelFormat.Format24bppRgb, new Color32(Color.Silver));
            SaveBitmapData("reference", refBmpData);

            using var transparentAuto = refBmpData.ToTransparent();
            Assert.AreEqual(default(Color32), transparentAuto[0][0]);

            SaveBitmapData("transparent", transparentAuto);

            using var transparentDirect = refBmpData.ToTransparent(new Color32(Color.Silver));
            AssertAreEqual(transparentAuto, transparentDirect);
        }

        [TestCase(WorkingColorSpace.Srgb)]
        [TestCase(WorkingColorSpace.Linear)]
        public void ResizeTest(WorkingColorSpace colorSpace)
        {
            using var refBmpData = GetBitmapData(@"..\..\..\..\Help\Images\GirlWithAPearlEarringRgb111DitheredB8Linear.gif", colorSpace);
            var newSize = new Size(256, 256);
            using var resized = refBmpData.Resize(newSize);
            Assert.AreEqual(newSize, resized.Size);
            Assert.AreEqual(colorSpace == WorkingColorSpace.Linear ? KnownPixelFormat.Format32bppArgb : KnownPixelFormat.Format32bppPArgb, resized.PixelFormat.ToKnownPixelFormat());
            SaveBitmapData($"{colorSpace}", resized);
        }

        [Test]
        public void ResizeAspectRatioTest()
        {
            using var refBmpData = GetInfoIcon256();
            var newSize = new Size(256, 128);
            using var resized = refBmpData.Resize(newSize);
            Assert.AreEqual(newSize, resized.Size);
            Assert.AreEqual(KnownPixelFormat.Format32bppPArgb, resized.PixelFormat.ToKnownPixelFormat());
            SaveBitmapData("NotKeepingAspectRatio", resized);

            using var resizedKeepAspectRatio = refBmpData.Resize(newSize, keepAspectRatio: true);
            Assert.AreEqual(newSize, resizedKeepAspectRatio.Size);
            Assert.AreEqual(KnownPixelFormat.Format32bppPArgb, resizedKeepAspectRatio.PixelFormat.ToKnownPixelFormat());
            SaveBitmapData("KeepingAspectRatio", resizedKeepAspectRatio);
        }

        [Test]
        public void TrySetPaletteTest()
        {
            using var bmpData = GetInfoIcon256()
                .Clone(KnownPixelFormat.Format1bppIndexed);
            SaveBitmapData("BW", bmpData);

            Assert.IsTrue(bmpData.TrySetPalette(new Palette(new[] { Color.Transparent, Color.Blue })));
            Assert.AreEqual(new Color32(Color.Transparent), bmpData[0][0]);
            SaveBitmapData("transparent-blue", bmpData);

            // too many colors for 1bpp
            Assert.IsFalse(bmpData.TrySetPalette(new Palette(new[] { Color.Transparent, Color.Blue, Color.Red })));
        }

        [TestCase(KnownPixelFormat.Format1bppIndexed)]
        [TestCase(KnownPixelFormat.Format4bppIndexed)]
        [TestCase(KnownPixelFormat.Format8bppIndexed)]
        [TestCase(KnownPixelFormat.Format8bppGrayScale)]
        [TestCase(KnownPixelFormat.Format16bppGrayScale)]
        [TestCase(KnownPixelFormat.Format32bppGrayScale)]
        [TestCase(KnownPixelFormat.Format16bppRgb555)]
        [TestCase(KnownPixelFormat.Format16bppRgb565)]
        [TestCase(KnownPixelFormat.Format16bppArgb1555)]
        [TestCase(KnownPixelFormat.Format24bppRgb)]
        [TestCase(KnownPixelFormat.Format32bppRgb)]
        [TestCase(KnownPixelFormat.Format32bppArgb)]
        [TestCase(KnownPixelFormat.Format32bppPArgb)]
        [TestCase(KnownPixelFormat.Format48bppRgb)]
        [TestCase(KnownPixelFormat.Format64bppArgb)]
        [TestCase(KnownPixelFormat.Format64bppPArgb)]
        [TestCase(KnownPixelFormat.Format96bppRgb)]
        [TestCase(KnownPixelFormat.Format128bppRgba)]
        [TestCase(KnownPixelFormat.Format128bppPRgba)]
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
            SaveBitmapData($"{pixelFormat}", clone);
        }

        [TestCase(KnownPixelFormat.Format1bppIndexed)]
        [TestCase(KnownPixelFormat.Format4bppIndexed)]
        [TestCase(KnownPixelFormat.Format8bppIndexed)]
        [TestCase(KnownPixelFormat.Format8bppGrayScale)]
        [TestCase(KnownPixelFormat.Format16bppGrayScale)]
        [TestCase(KnownPixelFormat.Format32bppGrayScale)]
        [TestCase(KnownPixelFormat.Format16bppRgb555)]
        [TestCase(KnownPixelFormat.Format16bppRgb565)]
        [TestCase(KnownPixelFormat.Format16bppArgb1555)]
        [TestCase(KnownPixelFormat.Format24bppRgb)]
        [TestCase(KnownPixelFormat.Format32bppRgb)]
        [TestCase(KnownPixelFormat.Format32bppArgb)]
        [TestCase(KnownPixelFormat.Format32bppPArgb)]
        [TestCase(KnownPixelFormat.Format48bppRgb)]
        [TestCase(KnownPixelFormat.Format64bppArgb)]
        [TestCase(KnownPixelFormat.Format64bppPArgb)]
        [TestCase(KnownPixelFormat.Format96bppRgb)]
        [TestCase(KnownPixelFormat.Format128bppRgba)]
        [TestCase(KnownPixelFormat.Format128bppPRgba)]
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
            SaveBitmapData($"{pixelFormat}", clone);
        }

        [Test]
        public void AsyncTest()
        {
            var bmpData = GetInfoIcon256();

            using (var bmpRef = bmpData.Clone())
            {
                bmpRef.AdjustBrightness(0.1f);

                using var bmpAsyncApm = bmpData.Clone();
                bmpAsyncApm.BeginAdjustBrightness(0.1f).EndAdjustBrightness();
                AssertAreEqual(bmpRef, bmpAsyncApm);

#if !NET35
                using var bmpAsyncTap = bmpData.Clone();
                bmpAsyncTap.AdjustBrightnessAsync(0.1f).Wait();
                AssertAreEqual(bmpRef, bmpAsyncTap);
#endif
            }

            using (var bmpRef = bmpData.Clone())
            {
                bmpRef.AdjustContrast(0.1f);

                using var bmpAsyncApm = bmpData.Clone();
                bmpAsyncApm.BeginAdjustContrast(0.1f).EndAdjustContrast();
                AssertAreEqual(bmpRef, bmpAsyncApm);

#if !NET35
                using var bmpAsyncTap = bmpData.Clone();
                bmpAsyncTap.AdjustContrastAsync(0.1f).Wait();
                AssertAreEqual(bmpRef, bmpAsyncTap);
#endif
            }

            using (var bmpRef = bmpData.Clone())
            {
                bmpRef.AdjustGamma(0.1f);

                using var bmpAsyncApm = bmpData.Clone();
                bmpAsyncApm.BeginAdjustGamma(0.1f).EndAdjustGamma();
                AssertAreEqual(bmpRef, bmpAsyncApm);

#if !NET35
                using var bmpAsyncTap = bmpData.Clone();
                bmpAsyncTap.AdjustGammaAsync(0.1f).Wait();
                AssertAreEqual(bmpRef, bmpAsyncTap);
#endif
            }
        }

        [Explicit]
        [TestCase(WorkingColorSpace.Srgb)]
        [TestCase(WorkingColorSpace.Linear)]
        public void LinearVsSrgbBlendingBars(WorkingColorSpace colorSpace)
        {
            using var target = BitmapDataFactory.CreateBitmapData(new Size(512, 512), KnownPixelFormat.Format24bppRgb, colorSpace);
            target.Clear(Color32.White);
            var colors = new[] { Color.Red, Color.Lime, Color.Blue, Color.Cyan, Color.Magenta, Color.Yellow, Color.Black, Color.Gray };
            Point point = new Point(16, 0);
            Size offset = new Size(64, 0);
            for (int i = 0; i < colors.Length; i++)
            {
                using (var vertical = new SolidBitmapData(new Size(32, 512), colors[i]))
                    vertical.DrawInto(target, point + new Size(offset.Width * i, offset.Height * i));
            }

            point = new Point(0, 16);
            offset = new Size(0, 64);
            for (int i = 0; i < colors.Length; i++)
            {
                using (var horizontal = new SolidBitmapData(new Size(512, 32), Color.FromArgb(128, colors[i])))
                    horizontal.DrawInto(target, point + new Size(offset.Width * i, offset.Height * i));
            }

            SaveBitmapData($"{colorSpace}", target);
        }

        [Explicit]
        //[TestCase(WorkingColorSpace.Srgb, KnownPixelFormat.Format24bppRgb)]
        //[TestCase(WorkingColorSpace.Linear, KnownPixelFormat.Format24bppRgb)]
        //[TestCase(WorkingColorSpace.Default, KnownPixelFormat.Format32bppArgb)]
        [TestCase(WorkingColorSpace.Default, KnownPixelFormat.Format32bppPArgb)]
        public void LinearVsSrgbBlendingAlphaGradient(WorkingColorSpace colorSpace, KnownPixelFormat pixelFormat)
        {
            using (var target = BitmapDataFactory.CreateBitmapData(new Size(512, 256), pixelFormat, colorSpace))
            {
                GenerateAlphaGradient(target);
                SaveBitmapData($"{pixelFormat} {colorSpace} Black", target);
            }

            //using (var target = BitmapDataFactory.CreateBitmapData(new Size(512, 256), pixelFormat, colorSpace, Color.White))
            //{
            //    GenerateAlphaGradient(target);
            //    SaveBitmapData($"{pixelFormat} {colorSpace} White", target);
            //}
        }

        [Test]
        [Explicit]
        public void GenerateImageForHelp()
        {
            string[] files =
            {
                //@"..\..\..\..\Help\Images\Shield256.png",
                @"..\..\..\..\Help\Images\GrayShades.gif",
                //@"..\..\..\..\Help\Images\GirlWithAPearlEarring.png",
            };

            (PredefinedColorsQuantizer Quantizer, string Name)[] quantizers =
            {
                //(PredefinedColorsQuantizer.BlackAndWhite(), "BW"),
                //(PredefinedColorsQuantizer.FromCustomPalette(new[] { Color.Black, Color.White, Color.Red, Color.Lime, Color.Blue, Color.Cyan, Color.Magenta, Color.Yellow }), "Rgb111"),
                (PredefinedColorsQuantizer.SystemDefault4BppPalette(), "Default4bpp"),
                //(PredefinedColorsQuantizer.SystemDefault8BppPalette(), "Default8bpp"),
            };

            (IDitherer Ditherer, string Name)[] ditherers =
            {
                //(null, String.Empty),
                //(OrderedDitherer.Bayer8x8, "DitheredB8"),
                (OrderedDitherer.Bayer8x8.ConfigureAutoStrengthMode(AutoStrengthMode.Constant), "DitheredB8Constant"),
                (OrderedDitherer.Bayer8x8.ConfigureAutoStrengthMode(AutoStrengthMode.Interpolated), "DitheredB8Interpolated"),
                //(ErrorDiffusionDitherer.FloydSteinberg, "DitheredFS"),
                //(new RandomNoiseDitherer(0, 0), nameof(RandomNoiseDitherer)),
                //(new InterleavedGradientNoiseDitherer(0), nameof(InterleavedGradientNoiseDitherer)),
            };

            foreach (string file in files)
            {
                string fileName = Path.IsPathRooted(file) ? file : Path.Combine(Files.GetExecutingPath(), file);
                using var bitmap = LoadBitmap(File.OpenRead(fileName));
                using var src = ToBitmapData(bitmap);
                foreach (var (quantizer, quantizerName) in quantizers)
                {
                    foreach (var (ditherer, dithererName) in ditherers)
                    {
                        foreach (var colorSpace in new[] { WorkingColorSpace.Srgb, WorkingColorSpace.Linear })
                        {
                            using var result = src.Clone(quantizer.PixelFormatHint, quantizer.ConfigureColorSpace(colorSpace), ditherer);
                            SaveBitmapData($"{Path.GetFileNameWithoutExtension(file)}{quantizerName}{dithererName}{colorSpace}", result);
                        }
                    }
                }
            }
        }

        #endregion
    }
}
