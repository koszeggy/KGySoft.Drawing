#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: GifEncoderTest.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2021 - All Rights Reserved
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
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

using KGySoft.CoreLibraries;
using KGySoft.Drawing.Imaging;
using KGySoft.Reflection;

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

            private readonly Bitmap[] frames;
            private readonly int cancelAfter;

            private int current;

            #endregion

            #region Constructors

            internal TestFramesCollection(Bitmap[] frames, int cancelAfter)
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
                    bitmapData = frames[current].GetReadableBitmapData();
                    yield return bitmapData.Clone(PixelFormat.Format8bppIndexed);
                }

                bitmapData?.Dispose();
            }


            #endregion
        }

        #endregion

        #region TestProgress class

        private class TestProgress : IDrawingProgress
        {
            #region Methods
            
            void IDrawingProgress.Report(DrawingProgress progress) => Console.WriteLine($"{nameof(IDrawingProgress)}.{nameof(IDrawingProgress.Report)}: {progress.OperationType} {progress.CurrentValue}/{progress.MaximumValue}");
            void IDrawingProgress.New(DrawingOperation operationType, int maximumValue, int currentValue) => Console.Write($"{nameof(IDrawingProgress)}.{nameof(IDrawingProgress.New)}: {operationType} {currentValue}/{maximumValue}");
            void IDrawingProgress.Increment() => Console.Write('.');
            void IDrawingProgress.SetProgressValue(int value) => Console.Write($"({value})");
            void IDrawingProgress.Complete() => Console.WriteLine($"{nameof(IDrawingProgress)}.{nameof(IDrawingProgress.Complete)}");
#if !(NET35 || NET40)
            void IProgress<DrawingProgress>.Report(DrawingProgress value) => ((IDrawingProgress)this).Report(value); 
#endif

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
            IReadWriteBitmapData imageData = frame.GetReadableBitmapData().Clone(PixelFormat.Format4bppIndexed, PredefinedColorsQuantizer.FromCustomPalette(palette));

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
            IReadWriteBitmapData imageData = frame.GetReadableBitmapData().Clone(PixelFormat.Format4bppIndexed, PredefinedColorsQuantizer.FromCustomPalette(palette));

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

                using (var imageData = frame.GetReadableBitmapData().Clone(PixelFormat.Format4bppIndexed))
                    encoder.AddImage(imageData, new Point(8, 8), 100, GifGraphicDisposalMethod.RestoreToBackground);
                frame.Dispose();

                frame = new Bitmap(32, 32);
                using (Graphics g = Graphics.FromImage(frame))
                {
                    using var pen = new Pen(Brushes.Red, 3);
                    g.DrawRectangle(pen, 4, 4, 24, 24);
                }

                using (var imageData = frame.GetReadableBitmapData().Clone(PixelFormat.Format4bppIndexed))
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

                using (var imageData = frame.GetReadableBitmapData().Clone(PixelFormat.Format4bppIndexed, palette))
                    encoder.AddImage(imageData, new Point(8, 8), 100, GifGraphicDisposalMethod.RestoreToBackground);
                frame.Dispose();

                frame = new Bitmap(32, 32);
                using (Graphics g = Graphics.FromImage(frame))
                {
                    using var pen = new Pen(Brushes.Red, 3);
                    g.DrawRectangle(pen, 4, 4, 24, 24);
                }

                using (var imageData = frame.GetReadableBitmapData().Clone(PixelFormat.Format4bppIndexed, palette))
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

                using (var imageData = frame.GetReadableBitmapData().Clone(PixelFormat.Format4bppIndexed, new Palette(palette)))
                    encoder.AddImage(imageData, new Point(8, 8), 100, GifGraphicDisposalMethod.RestoreToBackground);
                frame.Dispose();

                frame = new Bitmap(32, 32);
                using (Graphics g = Graphics.FromImage(frame))
                {
                    using var pen = new Pen(Brushes.Red, 3);
                    g.DrawRectangle(pen, 4, 4, 24, 24);
                }

                palette[4] = Color.Transparent;
                using (var imageData = frame.GetReadableBitmapData().Clone(PixelFormat.Format4bppIndexed, new Palette(palette)))
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

                using (var imageData = frame.GetReadableBitmapData().Clone(PixelFormat.Format4bppIndexed, new Palette(palette)))
                    encoder.AddImage(imageData, new Point(8, 8), 100, GifGraphicDisposalMethod.RestoreToBackground);
                frame.Dispose();

                frame = new Bitmap(32, 32);
                using (Graphics g = Graphics.FromImage(frame))
                {
                    using var pen = new Pen(Brushes.Red, 3);
                    g.DrawRectangle(pen, 4, 4, 24, 24);
                }

                palette[4] = Color.White;
                using (var imageData = frame.GetReadableBitmapData().Clone(PixelFormat.Format4bppIndexed, new Palette(palette)))
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

                using (var imageData = frame.GetReadableBitmapData().Clone(PixelFormat.Format4bppIndexed, new Palette(palette)))
                    encoder.AddImage(imageData, new Point(8, 8), 100, GifGraphicDisposalMethod.RestoreToBackground);
                frame.Dispose();

                frame = new Bitmap(32, 32);
                using (Graphics g = Graphics.FromImage(frame))
                {
                    using var pen = new Pen(Brushes.Red, 3);
                    g.DrawRectangle(pen, 4, 4, 24, 24);
                }

                palette[4] = Color.Transparent;
                using (var imageData = frame.GetReadableBitmapData().Clone(PixelFormat.Format4bppIndexed, new Palette(palette)))
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

                using (var imageData = frame.GetReadableBitmapData().Clone(PixelFormat.Format4bppIndexed, new Palette(palette)))
                    encoder.AddImage(imageData, new Point(8, 8), 100, GifGraphicDisposalMethod.RestoreToBackground);
                frame.Dispose();

                frame = new Bitmap(32, 32);
                using (Graphics g = Graphics.FromImage(frame))
                {
                    using var pen = new Pen(Brushes.Red, 3);
                    g.DrawRectangle(pen, 4, 4, 24, 24);
                }

                palette[4] = Color.White;
                using (var imageData = frame.GetReadableBitmapData().Clone(PixelFormat.Format4bppIndexed, new Palette(palette)))
                    encoder.AddImage(imageData, new Point(16, 16), 100, GifGraphicDisposalMethod.DoNotDispose);
                frame.Dispose();
            }

            SaveStream(null, stream);
        }

        [Test, Explicit]
        public void CompareEncodersTest()
        {
            using Bitmap toSave = Icons.Information.ExtractBitmap(new Size(256, 256))!;
            foreach (var bpp in new int[] { 1, 4, 8 })
            {
                using Bitmap quantized = toSave.ConvertPixelFormat(bpp.ToPixelFormat(), PredefinedColorsQuantizer.FromPixelFormat(bpp.ToPixelFormat(), Color.Silver, 0));
                var msOld = new MemoryStream();
                ((Bitmap)quantized.Clone()).SaveAsGif(msOld);

                var msNew = new MemoryStream();
                using (var bitmapData = quantized.GetReadableBitmapData())
                {
                    new GifEncoder(msNew, toSave.Size) { GlobalPalette = bitmapData.Palette }
                        .AddImage(bitmapData)
                        .FinalizeEncoding();
                }

                SaveStream($"{bpp}bpp_Old", msOld);
                SaveStream($"{bpp}bpp_New", msNew);
                SaveImage($"{bpp}bpp_Ref", quantized.ConvertPixelFormat(PixelFormat.Format32bppArgb));
            }
        }

        [TestCase(GifCompressionMode.Auto)]
#if WINDOWS
        [TestCase(GifCompressionMode.DoNotClear)]
#endif
        [TestCase(GifCompressionMode.DoNotIncreaseBitSize)]
        [TestCase(GifCompressionMode.Uncompressed)]
        public void LzwTest(GifCompressionMode compressionMode)
        {
            using Bitmap bmp = Icons.Information.ExtractBitmap(new Size(256, 256))!; // GenerateAlphaGradientBitmap(new Size(256, 128));
            using IReadableBitmapData bmpData = bmp.GetReadableBitmapData();
            //int bpp = 8;
            for (int bpp = 1; bpp <= 8; bpp++)
            {
                using IReadWriteBitmapData quantized = bmpData.Clone(PixelFormat.Format8bppIndexed,
                    OptimizedPaletteQuantizer.Wu(1 << bpp, Color.Silver, (byte)(bpp == 1 ? 0 : 128)), ErrorDiffusionDitherer.FloydSteinberg);
                using var ms = new MemoryStream();
                new GifEncoder(ms, bmp.Size) { CompressionMode = compressionMode }
                    .AddImage(quantized)
                    .FinalizeEncoding();

                ms.Position = 0;
                using Bitmap gif = new Bitmap(ms);
                using IReadableBitmapData actual = gif.GetReadableBitmapData();
                SaveStream($"{bpp}bpp_{compressionMode}", ms);
                AssertAreEqual(quantized, actual);
            }
        }

        [TestCase(PixelFormat.Format8bppIndexed)]
        [TestCase(PixelFormat.Format4bppIndexed)]
        [TestCase(PixelFormat.Format1bppIndexed)]
        public void EncodeImageTest(PixelFormat sourcePixelFormat)
        {
            using Bitmap bmp = Icons.Information.ExtractBitmap(new Size(256, 256))!;
            using IReadableBitmapData bitmapData = bmp.GetReadableBitmapData().Clone(sourcePixelFormat);

            using var ms = new MemoryStream();
            GifEncoder.EncodeImage(bitmapData, ms);

            SaveStream(sourcePixelFormat.ToString(), ms);
            ms.Position = 0;

            using Bitmap restored = new Bitmap(ms);
            using IReadableBitmapData actual = restored.GetReadableBitmapData();
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
                    using (var bmpDataNative = bitmap.GetReadableBitmapData())
                        bitmapData = bmpDataNative.Clone(PixelFormat.Format8bppIndexed);
                    yield return bitmapData;
                }

                bitmapData?.Dispose();
                bitmap?.Dispose();
            }

            EncodeAnimatedGif(new AnimatedGifConfiguration(DisposingIterator()));
        }

        [TestCase("32bpp, no delta, center", PixelFormat.Format32bppArgb, false, false, AnimationFramesSizeHandling.Center)]
        [TestCase("32bpp, delta, resize", PixelFormat.Format32bppArgb, false, true, AnimationFramesSizeHandling.Resize)]
        [TestCase("32bpp, no delta, resize", PixelFormat.Format32bppArgb, false, false, AnimationFramesSizeHandling.Resize)]
        [TestCase("32bpp, bad quantizer", PixelFormat.Format32bppArgb, true, false, AnimationFramesSizeHandling.Center)]
        [TestCase("8bpp, delta, center", PixelFormat.Format8bppIndexed, false, true, AnimationFramesSizeHandling.Center)]
        [TestCase("8bpp, delta, resize", PixelFormat.Format8bppIndexed, false, true, AnimationFramesSizeHandling.Resize)]
        [TestCase("4bpp, delta, center", PixelFormat.Format4bppIndexed, false, true, AnimationFramesSizeHandling.Center)]
        [TestCase("4bpp, no delta, center", PixelFormat.Format4bppIndexed, false, false, AnimationFramesSizeHandling.Center)]
        [TestCase("4bpp, no delta, center, fix palette", PixelFormat.Format4bppIndexed, true, false, AnimationFramesSizeHandling.Center)]
        [TestCase("4bpp, delta, resize", PixelFormat.Format4bppIndexed, false, true, AnimationFramesSizeHandling.Resize)]
        public void EncodeAnimationDifferentImageSizes(string name, PixelFormat pixelFormat, bool explicitQuantizer, bool allowDelta, AnimationFramesSizeHandling sizeHandling)
        {
            Bitmap?[] frames = Icons.Information.ExtractBitmaps();

            IEnumerable<IReadableBitmapData> FramesIterator()
            {
                foreach (Bitmap? bitmap in frames)
                {
                    if (bitmap == null)
                        continue;

                    var bitmapDataNative = bitmap.GetReadableBitmapData();
                    IReadableBitmapData currentFrame = pixelFormat == bitmapDataNative.PixelFormat ? bitmapDataNative : bitmapDataNative.Clone(pixelFormat);
                    if (!ReferenceEquals(bitmapDataNative, currentFrame))
                        bitmapDataNative.Dispose();
                    yield return currentFrame;
                    currentFrame.Dispose();
                    //bitmap.Dispose(); // this kills the base member at compare that enumerates the frames again
                }
            }

            using var ms = new MemoryStream();
            var config = new AnimatedGifConfiguration(FramesIterator(), TimeSpan.FromMilliseconds(250))
            {
                Size = new Size(128, 128),
                SizeHandling = sizeHandling,
                Quantizer = explicitQuantizer ? PredefinedColorsQuantizer.FromPixelFormat(pixelFormat) : null,
                AllowDeltaFrames = allowDelta,
                AnimationMode = AnimationMode.PingPong,
               // DeltaTolerance = 32
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

        [TestCase(1, true)]
        [TestCase(-1, null)]
        [TestCase(-1, true)]
        [TestCase(-1, false)]
        public void BeginEncodeAnimationTest(int cancelAfter, bool? reportOverallProgress)
        {
            Bitmap[] frames = Icons.Information.ExtractBitmaps().Where(f => f != null).ToArray()!;
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
        [TestCase(nameof(OptimizedPaletteQuantizer.Wu))]
        //[TestCase(nameof(OptimizedPaletteQuantizer.MedianCut))]
        //[TestCase(nameof(OptimizedPaletteQuantizer.Octree))]
        public void EncodeAnimationHighColorFromFile(string quantizer)
        {
            using var bmp = new Bitmap(@"D:\Dokumentumok\Képek\Formats\GifHighColor_Anim.gif");
            //using var bmp = new Bitmap(@"D:\Dokumentumok\Képek\Formats\GifTrueColor_Anim.gif");
            //using var bmp = new Bitmap(@"D:\Dokumentumok\Képek\Formats\gif4bit_anim.gif");
            Bitmap[] frames = bmp.ExtractBitmaps();

            IEnumerable<IReadableBitmapData> FramesIterator()
            {
                foreach (Bitmap? bitmap in frames)
                {
                    IReadableBitmapData currentFrame = bitmap.GetReadableBitmapData();
                    yield return currentFrame;
                    currentFrame.Dispose();
                    //bitmap.Dispose(); // this kills the base member at compare that enumerates the frames again
                }
            }

            using var ms = new MemoryStream();
            var config = new AnimatedGifConfiguration(FramesIterator())
            {
                Quantizer = (IQuantizer)Reflector.InvokeMethod(typeof(OptimizedPaletteQuantizer), quantizer, 256, Color.Empty, (byte)128)!,
                //DeltaTolerance = 32
            };

            try
            {
                EncodeAnimatedGif(config, false, quantizer);
            }
            finally
            {
                frames.ForEach(f => f.Dispose());
            }
        }

        [Test]
        public void EncodeAnimationTrueColor()
        {
            using IReadWriteBitmapData? bitmapData = GenerateAlphaGradientBitmapData(new Size(256, 64));

            IEnumerable<IReadableBitmapData> FramesIterator()
            {
                using IReadWriteBitmapData currentFrame = BitmapDataFactory.CreateBitmapData(new Size(bitmapData.Width, bitmapData.Height * 2));

                IQuantizer quantizer = PredefinedColorsQuantizer.Rgb888(Color.White);
                for (int y = bitmapData.Height - 1; y >= 0; y--)
                {
                    bitmapData.CopyTo(currentFrame, new Rectangle(0, y, bitmapData.Width, 1), new Point(0, bitmapData.Height - y), quantizer);
                    yield return currentFrame;
                }

                quantizer = PredefinedColorsQuantizer.Rgb888(Color.Black);
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

            EncodeAnimatedGif(config, false);
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
            using var actual = reloaded.GetReadableBitmapData();

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

            using var bitmapData = new Bitmap(@"..\..\..\..\KGySoft.Drawing\Help\Images\Lena.png").GetReadWriteBitmapData();
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
            var e = Assert.Throws<ArgumentException>(() => GifEncoder.EncodeAnimation(new AnimatedGifConfiguration(new[] { Icons.Shield.ExtractBitmap(new Size(256, 256))!.GetReadableBitmapData() })
            {
                Quantizer = PredefinedColorsQuantizer.FromCustomPalette(palette)
            }, new MemoryStream()));

            Assert.IsTrue(e!.Message.StartsWith(Res.ImagingPaletteTooLarge(256, 8), StringComparison.Ordinal));
        }

        #endregion
    }
}
