#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: GifEncoderTest.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2023 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#nullable  enable

#region Usings

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

using KGySoft.CoreLibraries;
using KGySoft.Drawing.Imaging;
using KGySoft.Reflection;
using KGySoft.Threading;

using NUnit.Framework;

#endregion

namespace KGySoft.Drawing.UnitTests.Imaging
{
    [TestFixture]
    public class GifEncoderTest : TestBase
    {
        #region Nested Classes

        #region TestFramesCollection class
        
        private class TestFramesCollection : IEnumerable<IReadableBitmapData>
        {
            #region Fields

            private readonly IReadableBitmapData[] frames;
            private readonly int cancelAfter;

            private int current;

            #endregion

            #region Constructors

            internal TestFramesCollection(IReadableBitmapData[] frames, int cancelAfter)
            {
                this.frames = frames;
                this.cancelAfter = cancelAfter;
            }

            #endregion

            #region Methods

            IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<IReadableBitmapData>)this).GetEnumerator();

            IEnumerator<IReadableBitmapData> IEnumerable<IReadableBitmapData>.GetEnumerator()
            {
                IReadableBitmapData? bitmapData = null;
                for (current = 0; current < frames.Length; current++)
                {
                    bitmapData?.Dispose();
                    if (cancelAfter == current)
                        yield break;
                    bitmapData = frames[current].Clone(KnownPixelFormat.Format8bppIndexed);
                    yield return bitmapData;
                }

                bitmapData?.Dispose();
            }


            #endregion
        }

        #endregion

        #region TestProgress class

        private class TestProgress : IAsyncProgress
        {
            #region Methods
            
            void IAsyncProgress.Report<T>(AsyncProgress<T> progress) => Console.WriteLine($"{nameof(IAsyncProgress)}.{nameof(IAsyncProgress.Report)}: {progress.OperationType} {progress.CurrentValue}/{progress.MaximumValue}");
            void IAsyncProgress.New<T>(T operationType, int maximumValue, int currentValue) => Console.Write($"{nameof(IAsyncProgress)}.{nameof(IAsyncProgress.New)}: {operationType} {currentValue}/{maximumValue}");
            void IAsyncProgress.Increment() => Console.Write('.');
            void IAsyncProgress.SetProgressValue(int value) => Console.Write($"({value})");
            void IAsyncProgress.Complete() => Console.WriteLine($"{nameof(IAsyncProgress)}.{nameof(IAsyncProgress.Complete)}");

            #endregion
        }

        #endregion

        #endregion

        #region  Methods

        [Test, Explicit]
        public void ImageLowLevelSolidTest()
        {
            using var stream = new MemoryStream();
            var palette = new Palette(new[] { Color.Black, Color.Cyan, Color.Green, Color.Red });
            var frame = new Bitmap(48, 48);
            using (Graphics g = Graphics.FromImage(frame))
                g.FillEllipse(Brushes.Cyan, 0, 0, 48, 48);
            IReadWriteBitmapData imageData = ToBitmapData(frame).Clone(KnownPixelFormat.Format4bppIndexed, PredefinedColorsQuantizer.FromCustomPalette(palette));

            new GifEncoder(stream, new Size(48, 48))
                {
                    BackColorIndex = 2,
                    GlobalPalette = palette,
                    AddMetaInfo = true
                }
                .AddImage(imageData)
                .FinalizeEncoding();

            SaveStream("GlobalPalette, Full", stream);
            stream.SetLength(0);

            new GifEncoder(stream, new Size(64, 64))
                {
                    BackColorIndex = 2,
                    GlobalPalette = palette,
                    AddMetaInfo = true
                }
                .AddImage(imageData, new Point(8, 8))
                .FinalizeEncoding();
            SaveStream("GlobalPalette, Partial", stream);
            stream.SetLength(0);

            new GifEncoder(stream, new Size(48, 48))
                {
                    BackColorIndex = 2,
                    AddMetaInfo = true
                }
                .AddImage(imageData)
                .FinalizeEncoding();
            SaveStream("LocalPalette, Full", stream);
            stream.SetLength(0);

            new GifEncoder(stream, new Size(64, 64))
                {
                    BackColorIndex = 2,
                    AddMetaInfo = true
                }
                .AddImage(imageData, new Point(8, 8))
                .FinalizeEncoding();
            SaveStream("LocalPalette, Partial", stream);
            stream.SetLength(0);
        }

        [Test, Explicit]
        public void ImageLowLevelTransparentTest()
        {
            using var stream = new MemoryStream();
            var palette = new Palette(new[] { Color.Black, Color.Cyan, Color.Green, Color.Transparent });
            var frame = new Bitmap(48, 48);
            using (Graphics g = Graphics.FromImage(frame))
                g.FillEllipse(Brushes.Cyan, 0, 0, 48, 48);
            IReadWriteBitmapData imageData = ToBitmapData(frame).Clone(KnownPixelFormat.Format4bppIndexed, PredefinedColorsQuantizer.FromCustomPalette(palette));

            new GifEncoder(stream, new Size(48, 48))
                {
                    BackColorIndex = 2,
                    GlobalPalette = palette,
                    AddMetaInfo = true
                }
                .AddImage(imageData)
                .FinalizeEncoding();

            SaveStream("GlobalPalette, Full", stream);
            stream.SetLength(0);

            new GifEncoder(stream, new Size(64, 64))
                {
                    BackColorIndex = 2,
                    GlobalPalette = palette,
                    AddMetaInfo = true
                }
                .AddImage(imageData, new Point(8, 8))
                .FinalizeEncoding();
            SaveStream("GlobalPalette, Partial", stream);
            stream.SetLength(0);

            new GifEncoder(stream, new Size(48, 48))
                {
                    BackColorIndex = 2,
                    AddMetaInfo = true
                }
                .AddImage(imageData)
                .FinalizeEncoding();
            SaveStream("LocalPalette, Full", stream);
            stream.SetLength(0);

            new GifEncoder(stream, new Size(64, 64))
                {
                    BackColorIndex = 2,
                    AddMetaInfo = true
                }
                .AddImage(imageData, new Point(8, 8))
                .FinalizeEncoding();
            SaveStream("LocalPalette, Partial", stream);
            stream.SetLength(0);
        }

        [Test, Explicit]
        public void AnimationLowLevelSolidTest()
        {
            using var stream = new MemoryStream();
            using (var encoder = new GifEncoder(stream, new Size(64, 64))
            {
                RepeatCount = 0,
                BackColorIndex = 2,
                GlobalPalette = Palette.SystemDefault4BppPalette(),
                AddMetaInfo = true
            })
            {
                var frame = new Bitmap(48, 48);
                using (Graphics g = Graphics.FromImage(frame))
                {
                    g.FillEllipse(Brushes.Cyan, 0, 0, 48, 48);
                }

                using (var imageData = ToBitmapData(frame).Clone(KnownPixelFormat.Format4bppIndexed))
                    encoder.AddImage(imageData, new Point(8, 8), 100, GifGraphicDisposalMethod.RestoreToBackground);
                frame.Dispose();

                frame = new Bitmap(32, 32);
                using (Graphics g = Graphics.FromImage(frame))
                {
                    using var pen = new Pen(Brushes.Red, 3);
                    g.DrawRectangle(pen, 4, 4, 24, 24);
                }

                using (var imageData = ToBitmapData(frame).Clone(KnownPixelFormat.Format4bppIndexed))
                    encoder.AddImage(imageData, new Point(16, 16), 100, GifGraphicDisposalMethod.DoNotDispose);
                frame.Dispose();
            }

            SaveStream(null, stream);
        }

        [Test, Explicit]
        public void AnimationLowLevelTransparentTest()
        {
            using var stream = new MemoryStream();
            var palette = new Palette(new[] { Color.Black, Color.Cyan, Color.Green, Color.Red, Color.Transparent });
            using (var encoder = new GifEncoder(stream, new Size(64, 64))
            {
                RepeatCount = 0,
                BackColorIndex = 2,
                GlobalPalette = palette,
                AddMetaInfo = true
            })
            {
                var frame = new Bitmap(48, 48);
                using (Graphics g = Graphics.FromImage(frame))
                {
                    g.FillEllipse(Brushes.Cyan, 0, 0, 48, 48);
                }

                using (var imageData = ToBitmapData(frame).Clone(KnownPixelFormat.Format4bppIndexed, palette))
                    encoder.AddImage(imageData, new Point(8, 8), 100, GifGraphicDisposalMethod.RestoreToBackground);
                frame.Dispose();

                frame = new Bitmap(32, 32);
                using (Graphics g = Graphics.FromImage(frame))
                {
                    using var pen = new Pen(Brushes.Red, 3);
                    g.DrawRectangle(pen, 4, 4, 24, 24);
                }

                using (var imageData = ToBitmapData(frame).Clone(KnownPixelFormat.Format4bppIndexed, palette))
                    encoder.AddImage(imageData, new Point(16, 16), 100, GifGraphicDisposalMethod.DoNotDispose);
                frame.Dispose();
            }

            SaveStream(null, stream);
        }

        [Test, Explicit]
        public void AnimationLowLevelSolidToTransparentTest()
        {
            using var stream = new MemoryStream();
            Color[] palette = { Color.Black, Color.Cyan, Color.Green, Color.Red, Color.White };
            using (var encoder = new GifEncoder(stream, new Size(64, 64))
            {
                RepeatCount = 0,
                BackColorIndex = 2,
                AddMetaInfo = true
            })
            {
                var frame = new Bitmap(48, 48);
                using (Graphics g = Graphics.FromImage(frame))
                {
                    g.FillEllipse(Brushes.Cyan, 0, 0, 48, 48);
                }

                using (var imageData = ToBitmapData(frame).Clone(KnownPixelFormat.Format4bppIndexed, new Palette(palette)))
                    encoder.AddImage(imageData, new Point(8, 8), 100, GifGraphicDisposalMethod.RestoreToBackground);
                frame.Dispose();

                frame = new Bitmap(32, 32);
                using (Graphics g = Graphics.FromImage(frame))
                {
                    using var pen = new Pen(Brushes.Red, 3);
                    g.DrawRectangle(pen, 4, 4, 24, 24);
                }

                palette[4] = Color.Transparent;
                using (var imageData = ToBitmapData(frame).Clone(KnownPixelFormat.Format4bppIndexed, new Palette(palette)))
                    encoder.AddImage(imageData, new Point(16, 16), 100, GifGraphicDisposalMethod.DoNotDispose);
                frame.Dispose();
            }

            SaveStream(null, stream);
        }

        [Test, Explicit]
        public void AnimationLowLevelTransparentToSolidTest()
        {
            using var stream = new MemoryStream();
            Color[] palette = { Color.Black, Color.Cyan, Color.Green, Color.Red, Color.Transparent };
            using (var encoder = new GifEncoder(stream, new Size(64, 64))
            {
                RepeatCount = 0,
                BackColorIndex = 2,
                AddMetaInfo = true
            })
            {
                var frame = new Bitmap(48, 48);
                using (Graphics g = Graphics.FromImage(frame))
                {
                    g.FillEllipse(Brushes.Cyan, 0, 0, 48, 48);
                }

                using (var imageData = ToBitmapData(frame).Clone(KnownPixelFormat.Format4bppIndexed, new Palette(palette)))
                    encoder.AddImage(imageData, new Point(8, 8), 100, GifGraphicDisposalMethod.RestoreToBackground);
                frame.Dispose();

                frame = new Bitmap(32, 32);
                using (Graphics g = Graphics.FromImage(frame))
                {
                    using var pen = new Pen(Brushes.Red, 3);
                    g.DrawRectangle(pen, 4, 4, 24, 24);
                }

                palette[4] = Color.White;
                using (var imageData = ToBitmapData(frame).Clone(KnownPixelFormat.Format4bppIndexed, new Palette(palette)))
                    encoder.AddImage(imageData, new Point(16, 16), 100, GifGraphicDisposalMethod.DoNotDispose);
                frame.Dispose();
            }

            SaveStream(null, stream);
        }

        [Test, Explicit]
        public void AnimationLowLevelSolidToTransparentGlobalTest()
        {
            using var stream = new MemoryStream();
            Color[] palette = { Color.Black, Color.Cyan, Color.Green, Color.Red, Color.White };
            using (var encoder = new GifEncoder(stream, new Size(64, 64))
            {
                RepeatCount = 0,
                BackColorIndex = 2,
                GlobalPalette = new Palette(palette),
                AddMetaInfo = true
            })
            {
                var frame = new Bitmap(48, 48);
                using (Graphics g = Graphics.FromImage(frame))
                {
                    g.FillEllipse(Brushes.Cyan, 0, 0, 48, 48);
                }

                using (var imageData = ToBitmapData(frame).Clone(KnownPixelFormat.Format4bppIndexed, new Palette(palette)))
                    encoder.AddImage(imageData, new Point(8, 8), 100, GifGraphicDisposalMethod.RestoreToBackground);
                frame.Dispose();

                frame = new Bitmap(32, 32);
                using (Graphics g = Graphics.FromImage(frame))
                {
                    using var pen = new Pen(Brushes.Red, 3);
                    g.DrawRectangle(pen, 4, 4, 24, 24);
                }

                palette[4] = Color.Transparent;
                using (var imageData = ToBitmapData(frame).Clone(KnownPixelFormat.Format4bppIndexed, new Palette(palette)))
                    encoder.AddImage(imageData, new Point(16, 16), 100, GifGraphicDisposalMethod.DoNotDispose);
                frame.Dispose();
            }

            SaveStream(null, stream);
        }

        [Test, Explicit]
        public void AnimationLowLevelTransparentToSolidGlobalTest()
        {
            using var stream = new MemoryStream();
            Color[] palette = { Color.Black, Color.Cyan, Color.Green, Color.Red, Color.Transparent };
            using (var encoder = new GifEncoder(stream, new Size(64, 64))
            {
                RepeatCount = 0,
                BackColorIndex = 2,
                GlobalPalette = new Palette(palette),
                AddMetaInfo = true
            })
            {
                var frame = new Bitmap(48, 48);
                using (Graphics g = Graphics.FromImage(frame))
                {
                    g.FillEllipse(Brushes.Cyan, 0, 0, 48, 48);
                }

                using (var imageData = ToBitmapData(frame).Clone(KnownPixelFormat.Format4bppIndexed, new Palette(palette)))
                    encoder.AddImage(imageData, new Point(8, 8), 100, GifGraphicDisposalMethod.RestoreToBackground);
                frame.Dispose();

                frame = new Bitmap(32, 32);
                using (Graphics g = Graphics.FromImage(frame))
                {
                    using var pen = new Pen(Brushes.Red, 3);
                    g.DrawRectangle(pen, 4, 4, 24, 24);
                }

                palette[4] = Color.White;
                using (var imageData = ToBitmapData(frame).Clone(KnownPixelFormat.Format4bppIndexed, new Palette(palette)))
                    encoder.AddImage(imageData, new Point(16, 16), 100, GifGraphicDisposalMethod.DoNotDispose);
                frame.Dispose();
            }

            SaveStream(null, stream);
        }

        [TestCase(GifCompressionMode.Auto)]
#if WINDOWS
        [TestCase(GifCompressionMode.DoNotClear)]
#endif
        [TestCase(GifCompressionMode.DoNotIncreaseBitSize)]
        [TestCase(GifCompressionMode.Uncompressed)]
        public void LzwTest(GifCompressionMode compressionMode)
        {
            using IReadableBitmapData bmpData = GetInfoIcon256();
            //int bpp = 8;
            for (int bpp = 1; bpp <= 8; bpp++)
            {
                using IReadWriteBitmapData quantized = bmpData.Clone(KnownPixelFormat.Format8bppIndexed,
                    OptimizedPaletteQuantizer.Wu(1 << bpp, Color.Silver, (byte)(bpp == 1 ? 0 : 128)), ErrorDiffusionDitherer.FloydSteinberg);
                using var ms = new MemoryStream();
                new GifEncoder(ms, new Size(bmpData.Width, bmpData.Height)) { CompressionMode = compressionMode }
                    .AddImage(quantized)
                    .FinalizeEncoding();

                ms.Position = 0;
                using Bitmap gif = new Bitmap(ms);
                using IReadableBitmapData actual = ToBitmapData(gif);
                SaveStream($"{bpp}bpp_{compressionMode}", ms);
                AssertAreEqual(quantized, actual);
            }
        }

        [TestCase(KnownPixelFormat.Format8bppIndexed)]
        [TestCase(KnownPixelFormat.Format4bppIndexed)]
        [TestCase(KnownPixelFormat.Format1bppIndexed)]
        public void EncodeImageTest(KnownPixelFormat sourcePixelFormat)
        {
            using IReadableBitmapData bitmapData = GetInfoIcon256().Clone(sourcePixelFormat);

            using var ms = new MemoryStream();
            GifEncoder.EncodeImage(bitmapData, ms);

            SaveStream(sourcePixelFormat.ToString(), ms);
            ms.Position = 0;

            using Bitmap restored = new Bitmap(ms);
            using IReadableBitmapData actual = ToBitmapData(restored);
            AssertAreEqual(bitmapData, actual, true);
        }

        [Test]
        public void EncodeAnimationSolidFramesDefaultSettings()
        {
            var size = new Size(16, 16);
            IReadWriteBitmapData[] frames = new IReadWriteBitmapData[2];
            frames[0] = BitmapDataFactory.CreateBitmapData(size);
            frames[0].Clear(new Color32(255, 0, 0));
            frames[1] = BitmapDataFactory.CreateBitmapData(size);
            frames[1].Clear(new Color32(0, 255, 0));

            using var ms = new MemoryStream();
            var options = new AnimatedGifConfiguration(frames);

            EncodeAnimatedGif(options);
            frames.ForEach(f => f.Dispose());
        }

        [Test]
        public void EncodeAnimationDisposeFrames()
        {
            static IEnumerable<IReadableBitmapData> DisposingIterator()
            {
                IReadableBitmapData? bitmapData = null;
                Bitmap? bitmap = null;
                for (int i = 0; i < 3; i++)
                {
                    bitmapData?.Dispose();
                    bitmap?.Dispose();
                    bitmap = new Bitmap(16, 16);
                    using (var g = Graphics.FromImage(bitmap))
                        g.DrawString(i.ToString(), SystemFonts.DefaultFont, Brushes.Black, 0, 0);
                    using (var bmpDataNative = ToBitmapData(bitmap))
                        bitmapData = bmpDataNative.Clone(KnownPixelFormat.Format8bppIndexed);
                    yield return bitmapData;
                }

                bitmapData?.Dispose();
                bitmap?.Dispose();
            }

            EncodeAnimatedGif(new AnimatedGifConfiguration(DisposingIterator()));
        }

        [TestCase("32bpp, no delta, center", KnownPixelFormat.Format32bppArgb, false, false, AnimationFramesSizeHandling.Center, 0)]
        [TestCase("32bpp, delta, resize", KnownPixelFormat.Format32bppArgb, false, true, AnimationFramesSizeHandling.Resize, 0)]
        [TestCase("32bpp, no delta, resize", KnownPixelFormat.Format32bppArgb, false, false, AnimationFramesSizeHandling.Resize, 0)]
        [TestCase("32bpp, bad quantizer", KnownPixelFormat.Format32bppArgb, true, false, AnimationFramesSizeHandling.Center, 0)]
        [TestCase("8bpp, delta, center", KnownPixelFormat.Format8bppIndexed, false, true, AnimationFramesSizeHandling.Center, 0)]
        [TestCase("8bpp, delta, center, high tolerance", KnownPixelFormat.Format8bppIndexed, false, true, AnimationFramesSizeHandling.Center, 255)]
        [TestCase("8bpp, delta, resize", KnownPixelFormat.Format8bppIndexed, false, true, AnimationFramesSizeHandling.Resize, 0)]
        [TestCase("4bpp, delta, center", KnownPixelFormat.Format4bppIndexed, false, true, AnimationFramesSizeHandling.Center, 0)]
        [TestCase("4bpp, no delta, center", KnownPixelFormat.Format4bppIndexed, false, false, AnimationFramesSizeHandling.Center, 0)]
        [TestCase("4bpp, no delta, center, fix palette", KnownPixelFormat.Format4bppIndexed, true, false, AnimationFramesSizeHandling.Center, 0)]
        [TestCase("4bpp, delta, center, fix palette", KnownPixelFormat.Format4bppIndexed, true, true, AnimationFramesSizeHandling.Center, 0)]
        [TestCase("4bpp, delta, center, fix palette high tolerance", KnownPixelFormat.Format4bppIndexed, true, true, AnimationFramesSizeHandling.Center, 255)]
        [TestCase("4bpp, delta, resize", KnownPixelFormat.Format4bppIndexed, false, true, AnimationFramesSizeHandling.Resize, 0)]
        public void EncodeAnimationDifferentImageSizes(string name, KnownPixelFormat pixelFormat, bool explicitQuantizer, bool allowDelta, AnimationFramesSizeHandling sizeHandling, byte tolerance)
        {
            IReadWriteBitmapData[]? frames = GetInfoIconImages();
            IEnumerable<IReadableBitmapData> FramesIterator()
            {
                foreach (IReadWriteBitmapData frame in frames)
                {
                    IReadableBitmapData currentFrame = pixelFormat == frame.PixelFormat.AsKnownPixelFormatInternal ? frame : frame.Clone(pixelFormat);
                    yield return currentFrame;
                    if (!ReferenceEquals(frame, currentFrame))
                        currentFrame.Dispose();
                }
            }

            var config = new AnimatedGifConfiguration(FramesIterator(), TimeSpan.FromMilliseconds(250))
            {
                Size = new Size(128, 128),
                SizeHandling = sizeHandling,
                Quantizer = explicitQuantizer ? PredefinedColorsQuantizer.FromBitmapData(BitmapDataFactory.CreateBitmapData(new Size(1, 1), pixelFormat, new Color32(Color.Green))) : null,
                AllowDeltaFrames = allowDelta,
                AnimationMode = AnimationMode.PingPong,
                DeltaTolerance = tolerance
            };

            try
            {
                EncodeAnimatedGif(config, false, name);
            }
            finally
            {
                frames.ForEach(f => f?.Dispose());
            }
        }

        [TestCase("Not quantized, delta off", null, false, 0, false)]
        [TestCase("Not quantized", null, false, 0, true)]
        [TestCase("Optimized, delta off", nameof(OptimizedPaletteQuantizer.Octree), false, 0, false)]
        [TestCase("Optimized", nameof(OptimizedPaletteQuantizer.Octree), false, 0, true)]
        [TestCase("Optimized, mixed", nameof(OptimizedPaletteQuantizer.Octree), true, 0, true)]
        [TestCase("Optimized, high tolerance", nameof(OptimizedPaletteQuantizer.Octree), false, 255, true)]
        [TestCase("Grayscale, delta off", nameof(PredefinedColorsQuantizer.Grayscale), false, 0, false)]
        [TestCase("Grayscale", nameof(PredefinedColorsQuantizer.Grayscale), false, 0, true)]
        [TestCase("Grayscale, mixed", nameof(PredefinedColorsQuantizer.Grayscale), true, 0, true)]
        [TestCase("Grayscale, high tolerance", nameof(PredefinedColorsQuantizer.Grayscale), false, 255, true)]
        [TestCase("Default 4bpp", nameof(PredefinedColorsQuantizer.SystemDefault4BppPalette), false, 0, true)]
        [TestCase("Default 4bpp, mixed", nameof(PredefinedColorsQuantizer.SystemDefault4BppPalette), true, 0, true)]
        [TestCase("Default 4bpp, high tolerance", nameof(PredefinedColorsQuantizer.SystemDefault4BppPalette), false, 255, true)]
        [TestCase("Default 8bpp", nameof(PredefinedColorsQuantizer.SystemDefault8BppPalette), false, 0, true)]
        [TestCase("Default 8bpp, mixed", nameof(PredefinedColorsQuantizer.SystemDefault8BppPalette), true, 0, true)]
        [TestCase("Default 8bpp, high tolerance", nameof(PredefinedColorsQuantizer.SystemDefault8BppPalette), false, 255, true)]
        public void PreserveQuantizedSourceTest(string name, string quantizer, bool mixed, byte tolerance, bool allowDelta)
        {
            IReadWriteBitmapData[] frames =
            {
                GetInfoIcon256(),
                GetBitmapData(@"..\..\..\..\Help\Images\Question256.png"),
                GetBitmapData(@"..\..\..\..\Help\Images\Error256.png"),
                GetBitmapData(@"..\..\..\..\Help\Images\Warning256.png"),
                GetShieldIcon256(),
                GetBitmapData(@"..\..\..\..\Help\Images\Application256.png"),
                GetBitmapData(@"..\..\..\..\Help\Images\SecurityShield256.png"),
                GetBitmapData(@"..\..\..\..\Help\Images\SecurityError256.png"),
                GetBitmapData(@"..\..\..\..\Help\Images\SecuritySuccess256.png"),
                GetBitmapData(@"..\..\..\..\Help\Images\SecurityQuestion256.png"),
                GetBitmapData(@"..\..\..\..\Help\Images\SecurityWarning256.png"),
            };

            IQuantizer GetQuantizer() => quantizer switch
            {
                nameof(OptimizedPaletteQuantizer.Octree) => OptimizedPaletteQuantizer.Octree(),
                nameof(PredefinedColorsQuantizer.Grayscale) => PredefinedColorsQuantizer.Grayscale(Color.Green),
                nameof(PredefinedColorsQuantizer.SystemDefault4BppPalette) => PredefinedColorsQuantizer.SystemDefault4BppPalette(Color.Green),
                nameof(PredefinedColorsQuantizer.SystemDefault8BppPalette) => PredefinedColorsQuantizer.SystemDefault8BppPalette(),
                _ => throw new ArgumentException("Unexpected quantizer")
            };

            IEnumerable<IReadableBitmapData> FramesIterator()
            {
                int frameCount = 0;
                foreach (IReadWriteBitmapData frame in frames)
                {
                    IReadableBitmapData currentFrame = quantizer == null || mixed && (++frameCount & 1) == 0
                        ? frame
                        : frame.Clone(KnownPixelFormat.Format8bppIndexed, GetQuantizer());
                    yield return currentFrame;
                    if (!ReferenceEquals(frame, currentFrame))
                        currentFrame.Dispose();
                    //frame.Dispose(); // this kills the base member at compare that enumerates the frames again
                }
            }

            using var ms = new MemoryStream();
            var config = new AnimatedGifConfiguration(FramesIterator(), TimeSpan.FromMilliseconds(250))
            {
                AllowDeltaFrames = allowDelta,
                DeltaTolerance = tolerance
            };

            try
            {
                EncodeAnimatedGif(config, false, name);
            }
            finally
            {
                frames.ForEach(f => f?.Dispose());
            }
        }

        [Explicit]
        [TestCase(@"D:\Dokumentumok\Képek\Formats\apng\Balls", true, false)]
        [TestCase(@"D:\Dokumentumok\Képek\Formats\apng\Balls", false, false)]
        [TestCase(@"D:\Dokumentumok\Képek\Formats\apng\Balls", true, true)]
        [TestCase(@"D:\Dokumentumok\Képek\Formats\apng\Balls", false, true)]
        public void ApngToGifPrequantizeVsQuantizeTest(string dir, bool allowDelta, bool prequantize)
        {
            IEnumerable<IReadableBitmapData> FramesIterator()
            {
                foreach (string file in Directory.GetFiles(dir, "*.png"))
                {
                    using IReadableBitmapData bitmapData = GetBitmapData(file);
                    yield return prequantize ? bitmapData.Clone(KnownPixelFormat.Format8bppIndexed, PredefinedColorsQuantizer.Grayscale(Color.Silver)) : bitmapData;
                }
            }

            var config = new AnimatedGifConfiguration(FramesIterator())
            {
                AllowDeltaFrames = allowDelta,
                Quantizer = prequantize ? null : PredefinedColorsQuantizer.Grayscale(Color.Silver)
            };

            EncodeAnimatedGif(config, false, $"{Path.GetFileName(dir)} {(allowDelta ? "delta" : "no delta")} {(prequantize ? "quantized" : "original")}");
        }

        [Explicit]
        [TestCase(@"D:\Dokumentumok\Képek\Formats\apng\Balls", false)]
        [TestCase(@"D:\Dokumentumok\Képek\Formats\apng\Balls", true)]
        [TestCase(@"D:\Dokumentumok\Képek\Formats\apng\Cube", false)]
        [TestCase(@"D:\Dokumentumok\Képek\Formats\apng\Cube", true)]
        public void ApngToGifTest(string dir, bool linear)
        {
            IEnumerable<IReadableBitmapData> FramesIterator()
            {
                foreach (string file in Directory.GetFiles(dir, "*.png"))
                {
                    using IReadableBitmapData bitmapData = GetBitmapData(file);
                    yield return bitmapData;
                }
            }

            var config = new AnimatedGifConfiguration(FramesIterator())
            {
                Quantizer = OptimizedPaletteQuantizer.Wu(256, Color.Black, 0).ConfigureColorSpace(linear)
            };

            EncodeAnimatedGif(config, false, $"{Path.GetFileName(dir)} linear={linear}");
        }

        [TestCase(1, true)]
        [TestCase(-1, null)]
        [TestCase(-1, true)]
        [TestCase(-1, false)]
        public void BeginEncodeAnimationTest(int cancelAfter, bool? reportOverallProgress)
        {
            IReadWriteBitmapData[] frames = GetInfoIconImages();
            using var ms = new MemoryStream();
            var framesCollection = new TestFramesCollection(frames, cancelAfter);
            var config = new AnimatedGifConfiguration(framesCollection, TimeSpan.FromMilliseconds(250))
            {
                SizeHandling = AnimationFramesSizeHandling.Center,
                ReportOverallProgress = reportOverallProgress,
                AnimationMode = AnimationMode.PingPong
            };

            IAsyncResult asyncResult = GifEncoder.BeginEncodeAnimation(config, ms, new AsyncConfig
            {
                ThrowIfCanceled = true,
                Progress = new TestProgress(),
            });
            try
            {
                GifEncoder.EndEncodeAnimation(asyncResult);
            }
            catch (OperationCanceledException)
            {
                Assert.IsTrue(cancelAfter >= 0);
            }
            finally
            {
                frames.ForEach(f => f.Dispose());
            }

            SaveStream($"Canceled={cancelAfter >= 0}, ReportOverallProgress={reportOverallProgress?.ToString() ?? "null"}", ms);
        }

        [Explicit]
        [TestCase(null, 0)]
        [TestCase(nameof(OptimizedPaletteQuantizer.Wu), 0)]
        [TestCase(nameof(OptimizedPaletteQuantizer.MedianCut), 0)]
        [TestCase(nameof(OptimizedPaletteQuantizer.Octree), 0)]
        public void EncodeAnimationHighColorFromFile(string? quantizer, byte tolerance)
        {
            //using var bmp = new Bitmap(@"D:\Dokumentumok\Képek\Formats\GifHighColor_Anim.gif");
            //using var bmp = new Bitmap(@"D:\Dokumentumok\Képek\Formats\GifTrueColor_Anim.gif");
            //using var bmp = new Bitmap(@"D:\Dokumentumok\Képek\Formats\gif4bit_anim.gif");
            using var bmp = new Bitmap(@"..\..\..\..\Help\Images\GifAnimationTrueColor.gif");
            Bitmap[] frames = ExtractBitmaps(bmp);

            IEnumerable<IReadableBitmapData> FramesIterator()
            {
                foreach (Bitmap? bitmap in frames)
                {
                    IReadableBitmapData currentFrame = ToBitmapData(bitmap);
                    yield return currentFrame;
                    currentFrame.Dispose();
                    //bitmap.Dispose(); // this kills the base member at compare that enumerates the frames again
                }
            }

            using var ms = new MemoryStream();
            var config = new AnimatedGifConfiguration(FramesIterator())
            {
                Quantizer = quantizer == null ? null : (IQuantizer)Reflector.InvokeMethod(typeof(OptimizedPaletteQuantizer), quantizer, 256, Color.Empty, (byte)128)!,
                DeltaTolerance = tolerance
            };

            try
            {
                EncodeAnimatedGif(config, false, $"{quantizer ?? "default"}, Tolerance={tolerance}");
            }
            finally
            {
                frames.ForEach(f => f.Dispose());
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void EncodeAnimationTrueColor(bool linear)
        {
            using IReadWriteBitmapData? bitmapData = GenerateAlphaGradientBitmapData(new Size(256, 64), linear);

            IEnumerable<IReadableBitmapData> FramesIterator()
            {
                using IReadWriteBitmapData currentFrame = BitmapDataFactory.CreateBitmapData(new Size(bitmapData.Width, bitmapData.Height * 2));

                IQuantizer quantizer = PredefinedColorsQuantizer.Rgb888(Color.White).ConfigureColorSpace(linear);
                for (int y = bitmapData.Height - 1; y >= 0; y--)
                {
                    bitmapData.CopyTo(currentFrame, new Rectangle(0, y, bitmapData.Width, 1), new Point(0, bitmapData.Height - y), quantizer);
                    yield return currentFrame;
                }

                quantizer = PredefinedColorsQuantizer.Rgb888(Color.Black).ConfigureColorSpace(linear);
                for (int y = 0; y < bitmapData.Height; y++)
                {
                    bitmapData.CopyTo(currentFrame, new Rectangle(0, y, bitmapData.Width, 1), new Point(0, y + bitmapData.Height), quantizer);
                    yield return currentFrame;
                }
            }

            IEnumerable<TimeSpan> DelaysIterator()
            {
                for (int i = 0; i < bitmapData.Height * 2 - 1; i++)
                    yield return TimeSpan.FromMilliseconds(20);
                yield return TimeSpan.FromSeconds(3);
            }

            using var ms = new MemoryStream();
            var config = new AnimatedGifConfiguration(FramesIterator(), DelaysIterator())
            {
                Quantizer = OptimizedPaletteQuantizer.Octree()
            };

            EncodeAnimatedGif(config, false, $"linear={linear}");
        }

        [Test]
        public void EncodeHighColorImage()
        {
            using IReadWriteBitmapData? bitmapData = GenerateAlphaGradientBitmapData(new Size(256, 128));
            bitmapData.Quantize(PredefinedColorsQuantizer.Rgb888(Color.Silver));
            using var ms = new MemoryStream();
            GifEncoder.EncodeHighColorImage(bitmapData, ms);
            SaveStream(null, ms);
            ms.Position = 0;
            
            using var reloaded = new Bitmap(ms);
            using var actual = ToBitmapData(reloaded);

#if !DEBUG // in debug it is animated on purpose
            AssertAreEqual(bitmapData, actual);
#endif
        }

        [Test, Explicit]
        public void EncodeHighColorImageMultiTest()
        {
            Color32 backColor = new Color32(Color.Silver);
            byte alphaThreshold = 100;

            //using IReadWriteBitmapData? bitmapData = GenerateAlphaGradientBitmapData(new Size(255, 200));
            //bitmapData.Quantize(PredefinedColorsQuantizer.Rgb332(Color.Silver));
            //bitmapData.Quantize(OptimizedPaletteQuantizer.Wu(256, Color.Silver, 0));
            //bitmapData.Quantize(PredefinedColorsQuantizer.Rgb888(Color.Silver));
            //bitmapData.Quantize(PredefinedColorsQuantizer.Argb8888(Color.Magenta, 255));

            //using var bitmapData = Icons.Warning.ExtractBitmap(0)!.GetReadWriteBitmapData();
            //bitmapData.Quantize(PredefinedColorsQuantizer.Argb8888(Color.Silver));
            //bitmapData.Quantize(PredefinedColorsQuantizer.Rgb888(Color.Silver));

            using var bitmapData = GetBitmapData(@"..\..\..\..\Help\Images\Lena.png");
            bitmapData.Dither(PredefinedColorsQuantizer.Rgb565(Color.Silver), ErrorDiffusionDitherer.FloydSteinberg);

            var ms = new MemoryStream();
            GifEncoder.EncodeHighColorImage(bitmapData, ms, false, backColor, alphaThreshold);
            SaveStream("FullScan=False", ms);

            ms = new MemoryStream();
            GifEncoder.EncodeHighColorImage(bitmapData, ms, true, backColor, alphaThreshold);
            SaveStream("FullScan=True", ms);
        }

        [Test]
        public void QuantizerWithLargePaletteTest()
        {
            var colors = new Color32[512];
            ((ICollection<Color32>)Palette.Grayscale256().Entries).CopyTo(colors, 0);
            var palette = new Palette(colors);
            var e = Assert.Throws<ArgumentException>(() => GifEncoder.EncodeAnimation(new AnimatedGifConfiguration(new[] { GetShieldIcon256() })
            {
                Quantizer = PredefinedColorsQuantizer.FromCustomPalette(palette)
            }, new MemoryStream()));

            Assert.IsTrue(e!.Message.StartsWith(Res.ImagingPaletteTooLarge(256, 8), StringComparison.Ordinal));
        }

        #endregion
    }
}
