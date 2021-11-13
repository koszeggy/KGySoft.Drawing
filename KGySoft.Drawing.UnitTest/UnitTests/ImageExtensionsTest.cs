#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ImageExtensionsTest.cs
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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
#if !NET35
using System.Threading.Tasks; 
#endif

using KGySoft.Drawing.Imaging;
using KGySoft.Drawing.WinApi;

using NUnit.Framework;

#endregion

namespace KGySoft.Drawing.UnitTests
{
    [TestFixture]
    public class ImageExtensionsTest : TestBase
    {
        #region Fields

        private static readonly object[][] convertPixelFormatCustomTestSource =
        {
            new object[] { "To 8bpp 256 color no dithering", PixelFormat.Format8bppIndexed, PredefinedColorsQuantizer.SystemDefault8BppPalette(), null },
            new object[] { "To 8bpp 256 color dithering", PixelFormat.Format8bppIndexed, PredefinedColorsQuantizer.SystemDefault8BppPalette(), OrderedDitherer.Bayer2x2 },
            new object[] { "To 8bpp 16 color no dithering", PixelFormat.Format8bppIndexed, PredefinedColorsQuantizer.SystemDefault4BppPalette(), null },
            new object[] { "To 4bpp 2 color dithering", PixelFormat.Format4bppIndexed, PredefinedColorsQuantizer.BlackAndWhite(), OrderedDitherer.DottedHalftone },
            new object[] { "To ARGB1555 256 color dithering", PixelFormat.Format16bppArgb1555, PredefinedColorsQuantizer.SystemDefault8BppPalette(), new RandomNoiseDitherer(), },
            new object[] { "To ARGB1555 32K color dithering", PixelFormat.Format16bppArgb1555, PredefinedColorsQuantizer.Argb1555(), new RandomNoiseDitherer(), },
            new object[] { "To ARGB1555 16.7M color dithering", PixelFormat.Format16bppArgb1555, PredefinedColorsQuantizer.Rgb888(), new RandomNoiseDitherer(), },
            new object[] { "To 1bpp 2 color dithering auto select quantizer", PixelFormat.Format1bppIndexed, null, OrderedDitherer.Bayer8x8 },
        };

        #endregion

        #region Methods

        [Test]
        public void ToGrayscaleTest()
        {
            using var bmp = Icons.Information.ExtractBitmap(new Size(256, 256));
            using var gray = bmp.ToGrayscale();
            Assert.IsFalse(bmp.EqualsByContent(gray));
            SaveImage("Grayscale", gray);
        }

        [TestCase(PixelFormat.Format8bppIndexed, 0, 0)]
        [TestCase(PixelFormat.Format8bppIndexed, 0, 1)]
        [TestCase(PixelFormat.Format8bppIndexed, 0, 128)]
        [TestCase(PixelFormat.Format8bppIndexed, 0, 255)]
        [TestCase(PixelFormat.Format8bppIndexed, 0xFFFFFF, 1)]
        [TestCase(PixelFormat.Format8bppIndexed, 0x888888, 1)]
        [TestCase(PixelFormat.Format4bppIndexed, 0, 0)]
        [TestCase(PixelFormat.Format4bppIndexed, 0xFFFFFF, 0)]
        [TestCase(PixelFormat.Format16bppRgb565, 0, 0)]
        [TestCase(PixelFormat.Format16bppRgb565, 0xFFFFFF, 0)]
        [TestCase(PixelFormat.Format16bppArgb1555, 0, 0)]
        [TestCase(PixelFormat.Format16bppArgb1555, 0xFFFFFF, 0)]
        [TestCase(PixelFormat.Format16bppArgb1555, 0, 128)]
        [TestCase(PixelFormat.Format16bppArgb1555, 0xFFFFFF, 128)]
        [TestCase(PixelFormat.Format16bppGrayScale, 0, 0)]
        [TestCase(PixelFormat.Format16bppGrayScale, 0xFFFFFF, 0)]
        [TestCase(PixelFormat.Format48bppRgb, 0, 0)]
        [TestCase(PixelFormat.Format48bppRgb, 0xFFFFFF, 0)]
        public void ConvertPixelFormatDirectTest(PixelFormat pixelFormat, int backColorArgb, byte alphaThreshold)
        {
            if (!pixelFormat.IsSupportedNatively())
                Assert.Inconclusive($"Pixel format is not supported: {pixelFormat}");

            using var ref32bpp = Icons.Information.ExtractBitmap(new Size(256, 256));
            Assert.AreEqual(32, ref32bpp.GetBitsPerPixel());

            var backColor = Color.FromArgb(backColorArgb);
            using var converted = ref32bpp.ConvertPixelFormat(pixelFormat, backColor, alphaThreshold);
            Assert.AreEqual(pixelFormat, converted.PixelFormat);
            SaveImage($"{pixelFormat} - {backColor.Name} (A={alphaThreshold})", converted);
        }

        [Test]
        public void BeginEndConvertPixelFormatBlockingWaitTest()
        {
            using var ref32bpp = Icons.Information.ExtractBitmap(new Size(256, 256));
            Assert.AreEqual(32, ref32bpp.GetBitsPerPixel());

            IAsyncResult ar = ref32bpp.BeginConvertPixelFormat(PixelFormat.Format8bppIndexed);
            Assert.IsFalse(ar.IsCompleted);
            Bitmap result = ImageExtensions.EndConvertPixelFormat(ar);
            Assert.IsTrue(ar.IsCompleted);
            Assert.IsFalse(ar.CompletedSynchronously);
            Assert.IsNotNull(result);
        }

        [Test]
        public void BeginEndConvertPixelFormatActiveWaitingTest()
        {
            using var ref32bpp = Icons.Information.ExtractBitmap(new Size(256, 256));
            Assert.AreEqual(32, ref32bpp.GetBitsPerPixel());

            IAsyncResult ar = ref32bpp.BeginConvertPixelFormat(PixelFormat.Format8bppIndexed);
            while (!ar.IsCompleted)
                Thread.Sleep(1);
            Bitmap result = ImageExtensions.EndConvertPixelFormat(ar);
            Assert.IsTrue(ar.IsCompleted);
            Assert.IsFalse(ar.CompletedSynchronously);
            Assert.IsNotNull(result);
        }

        [Test]
        public void BeginEndConvertPixelFormatWithCallbackTest()
        {
            using var ref32bpp = Icons.Information.ExtractBitmap(new Size(256, 256));
            Assert.AreEqual(32, ref32bpp.GetBitsPerPixel());

            var syncRoot = new object();
            Bitmap result = null;
            IAsyncResult ar = ref32bpp.BeginConvertPixelFormat(PixelFormat.Format8bppIndexed,
                asyncConfig: new AsyncConfig(ar =>
                {
                    lock (syncRoot)
                        result = ImageExtensions.EndConvertPixelFormat(ar);
                }));
            Assert.IsFalse(ar.IsCompleted);
            Assert.IsNull(result);
            ar.AsyncWaitHandle.WaitOne();
            Assert.IsTrue(ar.IsCompleted);
            Assert.IsFalse(ar.CompletedSynchronously);
            lock (syncRoot)
                Assert.IsNotNull(result);
        }

        [Test]
        public void BeginEndConvertPixelFormatImmediateCancelTest()
        {
            using var ref32bpp = Icons.Information.ExtractBitmap(new Size(256, 256));
            Assert.AreEqual(32, ref32bpp.GetBitsPerPixel());

            IAsyncResult ar = ref32bpp.BeginConvertPixelFormat(PixelFormat.Format8bppIndexed,
                asyncConfig: new AsyncConfig(null, () => true));
            Assert.IsTrue(ar.IsCompleted);
            Assert.IsTrue(ar.CompletedSynchronously);
            Assert.Throws<OperationCanceledException>(() => ImageExtensions.EndConvertPixelFormat(ar));
        }

        [Test]
        public void BeginEndConvertPixelFormatCancelWithDefaultValueTest()
        {
            using var ref32bpp = Icons.Information.ExtractBitmap(new Size(256, 256));
            Assert.AreEqual(32, ref32bpp.GetBitsPerPixel());

            IAsyncResult ar = ref32bpp.BeginConvertPixelFormat(PixelFormat.Format8bppIndexed,
                asyncConfig: new AsyncConfig(null, () => true) { ThrowIfCanceled = false });
            Assert.IsTrue(ar.IsCompleted);
            Assert.IsTrue(ar.CompletedSynchronously);
            Assert.IsNull(ar.EndConvertPixelFormat());
        }

#if !NET35
        [Test]
        public void ConvertPixelFormatAsyncBlockingWaitTest()
        {
            using var ref32bpp = Icons.Information.ExtractBitmap(new Size(256, 256));
            Assert.AreEqual(32, ref32bpp.GetBitsPerPixel());

            Task<Bitmap> task = ref32bpp.ConvertPixelFormatAsync(PixelFormat.Format8bppIndexed);
            Assert.IsFalse(task.IsCompleted);
            task.Wait();
            Assert.IsTrue(task.IsCompleted);
            Assert.IsNotNull(task.Result);
        }

        [Test]
        public void ConvertPixelFormatAsyncImmediateCancelTest()
        {
            using var ref32bpp = Icons.Information.ExtractBitmap(new Size(256, 256));
            Assert.AreEqual(32, ref32bpp.GetBitsPerPixel());

            Task<Bitmap> task = ref32bpp.ConvertPixelFormatAsync(PixelFormat.Format8bppIndexed,
                asyncConfig: new TaskConfig(new CancellationToken(true)));
            Assert.IsTrue(task.IsCanceled);
            var ex = Assert.Throws<AggregateException>(() => { var _ = task.Result; });
            Assert.IsInstanceOf<OperationCanceledException>(ex.InnerExceptions[0]);
        }

        [Test]
        public void ConvertPixelFormatAsyncReturnDefaultIfCanceledTest()
        {
            using var ref32bpp = Icons.Information.ExtractBitmap(new Size(256, 256));
            Assert.AreEqual(32, ref32bpp.GetBitsPerPixel());

            Task<Bitmap> task = ref32bpp.ConvertPixelFormatAsync(PixelFormat.Format8bppIndexed,
                asyncConfig: new TaskConfig(new CancellationToken(true)) { ThrowIfCanceled = false });
            Assert.IsTrue(task.IsCompleted);
            Assert.IsNull(task.Result);
        } 
#endif


#if !(NET35 || NET40)
        [Test]
        public async Task ConvertPixelFormatAsyncWithAwaitTest()
        {
            using var ref32bpp = Icons.Information.ExtractBitmap(new Size(256, 256));
            Assert.AreEqual(32, ref32bpp.GetBitsPerPixel());

            Bitmap result = await ref32bpp.ConvertPixelFormatAsync(PixelFormat.Format8bppIndexed);
            Assert.IsNotNull(result);
        } 
#endif

        [TestCaseSource(nameof(convertPixelFormatCustomTestSource))]
        public void ConvertPixelFormatCustomTest(string testName, PixelFormat pixelFormat, IQuantizer quantizer, IDitherer ditherer)
        {
            if (!pixelFormat.IsSupportedNatively())
                Assert.Inconclusive($"Pixel format is not supported: {pixelFormat}");

            using var source = Icons.Information.ExtractBitmap(new Size(256, 256));
            using var converted = source.ConvertPixelFormat(pixelFormat, quantizer, ditherer);
            Assert.AreEqual(pixelFormat, converted.PixelFormat);
            SaveImage(testName, converted);
        }

        [TestCase("32bpp ARGB to 32bpp ARGB", PixelFormat.Format32bppArgb, PixelFormat.Format32bppArgb)]
        [TestCase("32bpp PARGB to 32bpp PARGB", PixelFormat.Format32bppPArgb, PixelFormat.Format32bppPArgb)]
        [TestCase("32bpp ARGB to 32bpp RGB", PixelFormat.Format32bppArgb, PixelFormat.Format32bppRgb)]
        [TestCase("32bpp RGB to 32bpp ARGB", PixelFormat.Format32bppRgb, PixelFormat.Format32bppArgb)]
        [TestCase("32bpp ARGB to 32bpp PARGB", PixelFormat.Format32bppArgb, PixelFormat.Format32bppPArgb)]
        [TestCase("32bpp PARGB to 32bpp ARGB", PixelFormat.Format32bppPArgb, PixelFormat.Format32bppArgb)]
        [TestCase("32bpp ARGB to 16bpp Grayscale", PixelFormat.Format32bppArgb, PixelFormat.Format16bppGrayScale)]
        [TestCase("32bpp ARGB to 16bpp ARGB", PixelFormat.Format32bppArgb, PixelFormat.Format16bppArgb1555)]
        [TestCase("32bpp ARGB to 8bpp Indexed", PixelFormat.Format32bppArgb, PixelFormat.Format8bppIndexed)]
        [TestCase("32bpp ARGB to 4bpp Indexed", PixelFormat.Format32bppArgb, PixelFormat.Format4bppIndexed)]
        [TestCase("32bpp ARGB to 1bpp Indexed", PixelFormat.Format32bppArgb, PixelFormat.Format1bppIndexed)]
        [TestCase("64bpp ARGB to 64bpp ARGB", PixelFormat.Format64bppArgb, PixelFormat.Format64bppArgb)]
        [TestCase("64bpp PARGB to 64bpp PARGB", PixelFormat.Format64bppPArgb, PixelFormat.Format64bppPArgb)]
        public void DrawIntoNoScalingTest(string testName, PixelFormat formatSrc, PixelFormat formatDst)
        {
            if (!formatSrc.IsSupportedNatively())
                Assert.Inconclusive($"Pixel format is not supported: {formatSrc}");
            if (!formatDst.IsSupportedNatively())
                Assert.Inconclusive($"Pixel format is not supported: {formatDst}");

            Size targetSize = new Size(300, 300);
            Size sourceSize = new Size(300, 300);
            Point offset = new Point(-50, -50);

            // creating source images: alpha rectangles
            using var bmpSrc1 = new Bitmap(sourceSize.Width, sourceSize.Height, formatSrc);
            bmpSrc1.Clear(Color.FromArgb(128, Color.Red));
            using var bmpSrc2 = new Bitmap(sourceSize.Width, sourceSize.Height, formatSrc);
            bmpSrc2.Clear(Color.FromArgb(128, Color.Lime));

            // creating target image
            using var bmpDst = new Bitmap(targetSize.Width, targetSize.Height, formatDst);
            if (formatDst == PixelFormat.Format8bppIndexed)
                bmpDst.Clear(Color.Transparent);

            // drawing sources into destination
            Assert.DoesNotThrow(() => bmpSrc1.DrawInto(bmpDst, offset));
            Assert.DoesNotThrow(() => bmpSrc2.DrawInto(bmpDst, new Point(bmpDst.Width - offset.X - bmpSrc2.Width, bmpDst.Height - offset.Y - bmpSrc2.Height)));
            Assert.DoesNotThrow(() => Icons.Information.ExtractBitmap(new Size(256, 256)).DrawInto(bmpDst, new Point(32, 32)));

            using var bmp = GenerateAlphaGradientBitmap(new Size(256, 256));
            bmp.DrawInto(bmpDst, new Point(32, 32));

            SaveImage(testName, bmpDst);
        }

        [TestCase(PixelFormat.Format1bppIndexed)]
        [TestCase(PixelFormat.Format4bppIndexed)]
        [TestCase(PixelFormat.Format8bppIndexed)]
        [TestCase(PixelFormat.Format16bppRgb555)]
#if WINDOWS
        [TestCase(PixelFormat.Format16bppArgb1555)]
#endif
        public void DrawIntoNoScalingWithDitheringTest(PixelFormat formatDst)
        {
            var ditherers = new Dictionary<string, IDitherer>
            {
                ["(no dithering)"] = null,
                ["Ordered"] = OrderedDitherer.Bayer8x8,
                ["Error Diffusion (raster)"] = ErrorDiffusionDitherer.FloydSteinberg,
                ["Error Diffusion (serpentine)"] = ErrorDiffusionDitherer.FloydSteinberg.ConfigureProcessingDirection(true),
            };

            foreach (KeyValuePair<string, IDitherer> ditherer in ditherers)
            {
                Size targetSize = new Size(300, 300);
                Size sourceSize = new Size(300, 300);
                Point offset = new Point(-50, -50);

                // creating source images: alpha rectangles
                using var bmpSrc1 = new Bitmap(sourceSize.Width, sourceSize.Height);
                bmpSrc1.Clear(Color.FromArgb(128, Color.Red));
                using var bmpSrc2 = new Bitmap(sourceSize.Width, sourceSize.Height);
                bmpSrc2.Clear(Color.FromArgb(128, Color.Lime));

                // creating target image
                using var bmpDst = new Bitmap(targetSize.Width, targetSize.Height, formatDst);
                if (formatDst == PixelFormat.Format8bppIndexed)
                    bmpDst.Clear(Color.Transparent);

                // drawing sources into destination
                Assert.DoesNotThrow(() => bmpSrc1.DrawInto(bmpDst, offset, ditherer.Value));
                Assert.DoesNotThrow(() => bmpSrc2.DrawInto(bmpDst, new Point(bmpDst.Width - offset.X - bmpSrc2.Width, bmpDst.Height - offset.Y - bmpSrc2.Height), ditherer.Value));
                Assert.DoesNotThrow(() => Icons.Information.ExtractBitmap(new Size(256, 256)).DrawInto(bmpDst, new Point(32, 32), ditherer.Value));

                using var bmp = GenerateAlphaGradientBitmap(new Size(256, 256));
                bmp.DrawInto(bmpDst, new Point(32, 32), ditherer.Value);

                SaveImage($"{formatDst} {ditherer.Key}", bmpDst);
            }
        }

        [Test]
        public void DrawIntoNoScalingSameInstanceTest()
        {
            using var bmp = Icons.Information.ExtractBitmap(new Size(256, 256));
            Assert.DoesNotThrow(() => bmp.DrawInto(bmp, new Point(128, 128)));
            SaveImage("result", bmp);
        }

        [TestCase(256, ScalingMode.Bicubic)]
        [TestCase(16, ScalingMode.Bicubic)]
        [TestCase(256, ScalingMode.NearestNeighbor)]
        [TestCase(16, ScalingMode.NearestNeighbor)]
        public void DrawIntoWithResizeTargetOutOfBoundsTest(int size, ScalingMode scalingMode)
        {
            var sourceSize = new Size(size, size);
            var targetSize = new Size(100, 100);
            using var bmpSource = Icons.Information.ExtractBitmap(sourceSize);
            using var bmpRef = new Bitmap(targetSize.Width, targetSize.Height);
            using var bmpResult = new Bitmap(targetSize.Width, targetSize.Height);

            Rectangle srcRect = Rectangle.Round(new RectangleF(size * 0.1f, size * 0.0625f, size * 0.75f, size * 0.85f));

            using (var g = Graphics.FromImage(bmpRef))
            {
                g.InterpolationMode = scalingMode == ScalingMode.NearestNeighbor ? InterpolationMode.NearestNeighbor : InterpolationMode.HighQualityBicubic;

                // no cut
                var targetRect = new Rectangle(30, 30, 30, 30);
                g.DrawImage(bmpSource, targetRect, srcRect, GraphicsUnit.Pixel);
                bmpSource.DrawInto(bmpResult, srcRect, targetRect, scalingMode);

                // cut left
                targetRect = new Rectangle(-20, 5, 30, 30);
                g.DrawImage(bmpSource, targetRect, srcRect, GraphicsUnit.Pixel);
                bmpSource.DrawInto(bmpResult, srcRect, targetRect, scalingMode);

                // cut top
                targetRect = new Rectangle(50, -20, 30, 30);
                g.DrawImage(bmpSource, targetRect, srcRect, GraphicsUnit.Pixel);
                bmpSource.DrawInto(bmpResult, srcRect, targetRect, scalingMode);

                // cut right
                targetRect = new Rectangle(90, 50, 30, 30);
                g.DrawImage(bmpSource, targetRect, srcRect, GraphicsUnit.Pixel);
                bmpSource.DrawInto(bmpResult, srcRect, targetRect, scalingMode);

                // cut bottom
                targetRect = new Rectangle(10, 90, 30, 30);
                g.DrawImage(bmpSource, targetRect, srcRect, GraphicsUnit.Pixel);
                bmpSource.DrawInto(bmpResult, srcRect, targetRect, scalingMode);
            }

            SaveImage($"{scalingMode} {sourceSize.Width}x{sourceSize.Height} to {bmpResult.Width}x{bmpResult.Height} Reference", bmpRef);
            SaveImage($"{scalingMode} {sourceSize.Width}x{sourceSize.Height} to {bmpResult.Width}x{bmpResult.Height}", bmpResult);
        }

        [TestCase(256, ScalingMode.Bicubic)]
        [TestCase(16, ScalingMode.Bicubic)]
        [TestCase(256, ScalingMode.NearestNeighbor)]
        [TestCase(16, ScalingMode.NearestNeighbor)]
        public void DrawIntoWithResizeSourceOutOfBoundsTest(int size, ScalingMode scalingMode)
        {
            var sourceSize = new Size(size, size);
            var targetSize = new Size(256, 256);
            using var bmpSource = Icons.Information.ExtractBitmap(sourceSize);
            using var bmpRef = new Bitmap(targetSize.Width, targetSize.Height);
            using var bmpResult = new Bitmap(targetSize.Width, targetSize.Height);

            Rectangle srcRect = Rectangle.Round(new RectangleF(size * 0.75f, size * 0.25f, size * 0.5f, size * 0.5f));

            using (var g = Graphics.FromImage(bmpRef))
            {
                g.InterpolationMode = scalingMode == ScalingMode.NearestNeighbor ? InterpolationMode.NearestNeighbor : InterpolationMode.HighQualityBicubic;

                var targetRect = new Rectangle(0, 0, 120, 120);
                g.DrawImage(bmpSource, targetRect, srcRect, GraphicsUnit.Pixel);
                bmpSource.DrawInto(bmpResult, srcRect, targetRect, scalingMode);
            }

            SaveImage($"{scalingMode} {sourceSize.Width}x{sourceSize.Height} to {bmpResult.Width}x{bmpResult.Height} Reference", bmpRef);
            SaveImage($"{scalingMode} {sourceSize.Width}x{sourceSize.Height} to {bmpResult.Width}x{bmpResult.Height}", bmpResult);
        }

        [TestCase(PixelFormat.Format32bppArgb, ScalingMode.NearestNeighbor)]
        [TestCase(PixelFormat.Format32bppPArgb, ScalingMode.NearestNeighbor)]
        [TestCase(PixelFormat.Format24bppRgb, ScalingMode.NearestNeighbor)]
        [TestCase(PixelFormat.Format8bppIndexed, ScalingMode.NearestNeighbor)]
        [TestCase(PixelFormat.Format32bppArgb, ScalingMode.Auto)]
        [TestCase(PixelFormat.Format32bppPArgb, ScalingMode.Auto)]
        [TestCase(PixelFormat.Format24bppRgb, ScalingMode.Auto)]
        [TestCase(PixelFormat.Format8bppIndexed, ScalingMode.Auto)]
        public void DrawIntoWithResizeTest(PixelFormat pixelFormat, ScalingMode scalingMode)
        {
            using var bmp = Icons.Information.ExtractBitmap(new Size(256, 256)).ConvertPixelFormat(pixelFormat);
            var targetRect = new Rectangle(Point.Empty, bmp.Size);
            targetRect.Inflate(-32, -32);
            bmp.DrawInto(bmp, targetRect, scalingMode);
            SaveImage($"{pixelFormat}, {scalingMode}", bmp);
        }

        [TestCase(PixelFormat.Format1bppIndexed, ScalingMode.NearestNeighbor)]
        [TestCase(PixelFormat.Format1bppIndexed, ScalingMode.Auto)]
        [TestCase(PixelFormat.Format8bppIndexed, ScalingMode.NearestNeighbor)]
        [TestCase(PixelFormat.Format8bppIndexed, ScalingMode.Auto)]
#if WINDOWS
        [TestCase(PixelFormat.Format16bppArgb1555, ScalingMode.NearestNeighbor)]
        [TestCase(PixelFormat.Format16bppArgb1555, ScalingMode.Auto)]
#endif
        public void DrawIntoWithResizeDitheringTest(PixelFormat formatDst, ScalingMode scalingMode)
        {
            var ditherers = new Dictionary<string, IDitherer>
            {
                ["(no dithering)"] = null,
                ["Ordered"] = OrderedDitherer.Bayer8x8,
                ["Error Diffusion (raster)"] = ErrorDiffusionDitherer.FloydSteinberg,
                ["Error Diffusion (serpentine)"] = ErrorDiffusionDitherer.FloydSteinberg.ConfigureProcessingDirection(true),
            };

            foreach (KeyValuePair<string, IDitherer> ditherer in ditherers)
            {
                using var bmpSrc = Icons.Information.ExtractBitmap(new Size(256, 256));
                using var bmpDst = new Bitmap(bmpSrc.Width, bmpSrc.Height, formatDst);

                if (!formatDst.HasAlpha())
                    bmpDst.Clear(Color.Transparent, ditherer.Value, Color.Silver);

                var targetRect = new Rectangle(Point.Empty, bmpSrc.Size);
                targetRect.Inflate(-32, -32);

                // shrink
                Assert.DoesNotThrow(() => bmpSrc.DrawInto(bmpDst, targetRect, ditherer.Value, scalingMode));

                // enlarge
                targetRect = new Rectangle(160, 160, 100, 100);
                Assert.DoesNotThrow(() => Icons.Information.ExtractBitmap(new Size(16, 16)).DrawInto(bmpDst, targetRect, ditherer.Value, scalingMode));

                // gradient overlay
                using var bmp = GenerateAlphaGradientBitmap(new Size(192, 192));
                bmp.DrawInto(bmpDst, new Point(32, 32), ditherer.Value);

                SaveImage($"{formatDst} {scalingMode} {ditherer.Key}", bmpDst);
            }
        }

        [Test]
        public void DrawIntoWithResizeSameInstanceTest()
        {
            using var bmp = Icons.Information.ExtractBitmap(new Size(256, 256));
            Assert.DoesNotThrow(() => bmp.DrawInto(bmp, new Rectangle(128, 128, 100, 100)));
            SaveImage("result", bmp);
        }

        [Test]
        public void EqualsByContentTest()
        {
            var large = new Size(256, 256);
            Assert.IsTrue(Icons.Information.ExtractBitmap(large).EqualsByContent(Icons.Information.ExtractBitmap(large)));
            Assert.IsFalse(Icons.Information.ExtractBitmap(large).EqualsByContent(Icons.Question.ExtractBitmap(large)));
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
        public void ToIconSquaredTest(PixelFormat pixelFormat)
        {
            using var bmpRef = Convert(Icons.Information.ExtractBitmap(new Size(256, 256)).Resize(new Size(256, 128), true), pixelFormat);
            SaveImage($"{pixelFormat} Reference", bmpRef);

            using var noKeepRatio128 = bmpRef.ToIcon(128, false);
            Assert.AreEqual(new Size(128, 128), noKeepRatio128.Size);
            SaveIcon($"{pixelFormat} noKeepRatio128", noKeepRatio128);

            using var keepRatio128 = bmpRef.ToIcon(128, true);
            Assert.AreEqual(new Size(128, 128), keepRatio128.Size);
            SaveIcon($"{pixelFormat} keepRatio128", keepRatio128);
        }

        [TestCase(PixelFormat.Format64bppArgb, 0xFF000000)]
        [TestCase(PixelFormat.Format64bppArgb, 0xFFFFFFFF)]
        [TestCase(PixelFormat.Format64bppPArgb, 0xFF000000)]
        [TestCase(PixelFormat.Format64bppPArgb, 0xFFFFFFFF)]
        [TestCase(PixelFormat.Format48bppRgb, 0xFF000000)]
        [TestCase(PixelFormat.Format48bppRgb, 0xFFFFFFFF)]
        [TestCase(PixelFormat.Format32bppArgb, 0xFF000000)]
        [TestCase(PixelFormat.Format32bppArgb, 0xFFFFFFFF)]
        [TestCase(PixelFormat.Format32bppPArgb, 0xFF000000)]
        [TestCase(PixelFormat.Format32bppPArgb, 0xFFFFFFFF)]
        [TestCase(PixelFormat.Format32bppRgb, 0xFF000000)]
        [TestCase(PixelFormat.Format32bppRgb, 0xFFFFFFFF)]
        [TestCase(PixelFormat.Format24bppRgb, 0xFF000000)]
        [TestCase(PixelFormat.Format24bppRgb, 0xFFFFFFFF)]
        [TestCase(PixelFormat.Format16bppRgb565, 0xFF000000)]
        [TestCase(PixelFormat.Format16bppRgb565, 0xFFFFFFFF)]
        [TestCase(PixelFormat.Format16bppRgb555, 0xFF000000)]
        [TestCase(PixelFormat.Format16bppRgb555, 0xFFFFFFFF)]
        [TestCase(PixelFormat.Format16bppArgb1555, 0xFF000000)]
        [TestCase(PixelFormat.Format16bppArgb1555, 0xFFFFFFFF)]
        [TestCase(PixelFormat.Format16bppGrayScale, 0xFF000000)]
        [TestCase(PixelFormat.Format16bppGrayScale, 0xFFFFFFFF)]
        [TestCase(PixelFormat.Format8bppIndexed, 0xFF000000)]
        [TestCase(PixelFormat.Format8bppIndexed, 0xFFFFFFFF)]
        [TestCase(PixelFormat.Format4bppIndexed, 0xFF000000)]
        [TestCase(PixelFormat.Format4bppIndexed, 0xFFFFFFFF)]
        [TestCase(PixelFormat.Format1bppIndexed, 0xFF000000)]
        [TestCase(PixelFormat.Format1bppIndexed, 0xFFFFFFFF)]
        public void ToIconWithCustomBackColorTest(PixelFormat pixelFormat, uint transparentColor)
        {
            Color backColor = Color.FromArgb((int)transparentColor);
            using var bmpRef = Convert(Icons.Information.ExtractBitmap(new Size(64, 64)), pixelFormat, backColor);

            using Icon icon = bmpRef.ToIcon(backColor);
            SaveIcon($"{pixelFormat} {backColor.ToArgb():X8}", icon);
        }

        [Test]
        public void ToIconFromIconBitmapTest()
        {
            using var bmpRef = Icons.Information.ToMultiResBitmap();

            using Icon icon = bmpRef.ToIcon();
            Assert.AreEqual(OSUtils.IsWindows ? 7 : 1, icon.GetImagesCount());
            SaveIcon(null, icon);
        }

        [Test]
        public void SaveAsGifObsoleteTest()
        {
#pragma warning disable 618 // obsolete methods are tested here
            var ms = new MemoryStream();
            var refImage = Icons.Information.ExtractBitmap(new Size(256, 256));

            refImage.SaveAsGif(ms, false);
            ms.Position = 0;
            var gif = new Bitmap(ms);
            Assert.AreEqual(ImageFormat.Gif, gif.RawFormat);
            Assert.AreEqual(8, gif.GetBitsPerPixel());
            SaveImage("default", gif);

            ms = new MemoryStream();
            refImage.SaveAsGif(ms, new[] { Color.Blue, Color.White, Color.Black, Color.Transparent });
            ms.Position = 0;
            gif = new Bitmap(ms);
            Assert.AreEqual(ImageFormat.Gif, gif.RawFormat);
            Assert.AreEqual(OSUtils.IsWindows ? 4 : 256, gif.Palette.Entries.Length);
            SaveImage("customPalette", gif);

            ms = new MemoryStream();
            refImage.SaveAsGif(ms, true);
            ms.Position = 0;
            gif = new Bitmap(ms);
            Assert.AreEqual(ImageFormat.Gif, gif.RawFormat);
            Assert.AreEqual(8, gif.GetBitsPerPixel());
            SaveImage("dithered", gif);
#pragma warning restore 618
        }

        [TestCase(PixelFormat.Format64bppArgb, PixelFormat.Format64bppArgb)]
        [TestCase(PixelFormat.Format64bppPArgb, PixelFormat.Format64bppArgb)]
        [TestCase(PixelFormat.Format48bppRgb, PixelFormat.Format24bppRgb)]
        [TestCase(PixelFormat.Format32bppArgb, PixelFormat.Format32bppRgb)]
        [TestCase(PixelFormat.Format32bppPArgb, PixelFormat.Format32bppRgb)]
        [TestCase(PixelFormat.Format32bppRgb, PixelFormat.Format32bppRgb)]
        [TestCase(PixelFormat.Format24bppRgb, PixelFormat.Format24bppRgb)]
        [TestCase(PixelFormat.Format16bppRgb565, PixelFormat.Format24bppRgb)]
        [TestCase(PixelFormat.Format16bppRgb555, PixelFormat.Format24bppRgb)]
        [TestCase(PixelFormat.Format16bppArgb1555, PixelFormat.Format32bppRgb)]
        [TestCase(PixelFormat.Format16bppGrayScale, PixelFormat.Format8bppIndexed)]
        [TestCase(PixelFormat.Format8bppIndexed, PixelFormat.Format8bppIndexed)]
        [TestCase(PixelFormat.Format4bppIndexed, PixelFormat.Format4bppIndexed)]
        [TestCase(PixelFormat.Format1bppIndexed, PixelFormat.Format1bppIndexed)]
        public void SaveAsBmpTest(PixelFormat pixelFormat, PixelFormat savedFormat)
        {
            var ms = new MemoryStream();
            IQuantizer quantizer = pixelFormat.IsIndexed() ? OptimizedPaletteQuantizer.Octree(1 << pixelFormat.ToBitsPerPixel()) : null;
            var refImage = Convert(Icons.Information.ExtractBitmap(new Size(256, 256)), pixelFormat, quantizer);

            refImage.SaveAsBmp(ms);
            ms.Position = 0;
            var bmp = new Bitmap(ms);

            Assert.AreEqual(ImageFormat.Bmp, bmp.RawFormat);
            Assert.AreEqual(savedFormat, bmp.PixelFormat);
            SaveImage($"{pixelFormat}", bmp, true);
        }

        [Test]
        public void SaveMetafileAsBmpTest()
        {
            var ms = new MemoryStream();
            GenerateMetafile().SaveAsBmp(ms);
            ms.Position = 0;
            var bmp = new Bitmap(ms);
            Assert.AreEqual(ImageFormat.Bmp, bmp.RawFormat);
            SaveImage(null, bmp, true);
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
        public void SaveAsGifTest(PixelFormat pixelFormat)
        {
            var ms = new MemoryStream();
            IQuantizer quantizer = pixelFormat.IsIndexed() ? OptimizedPaletteQuantizer.Octree(1 << pixelFormat.ToBitsPerPixel()) : null;
            var refImage = Convert(Icons.Information.ExtractBitmap(new Size(256, 256)), pixelFormat, quantizer);

            refImage.SaveAsGif(ms);
            ms.Position = 0;
            var bmp = new Bitmap(ms);

            Assert.AreEqual(ImageFormat.Gif, bmp.RawFormat);
            Assert.AreEqual(PixelFormat.Format8bppIndexed, bmp.PixelFormat);
            SaveStream($"{pixelFormat}", ms);
        }

        [TestCase(PixelFormat.Format8bppIndexed)]
        [TestCase(PixelFormat.Format4bppIndexed)]
        [TestCase(PixelFormat.Format1bppIndexed)]
        public void SaveAsAnimatedGifTest(PixelFormat pixelFormat)
        {
            var ms = new MemoryStream();
            int bpp = pixelFormat.ToBitsPerPixel();
            IQuantizer quantizer = pixelFormat.IsIndexed() ? OptimizedPaletteQuantizer.Wu(1 << bpp, Color.Silver, (byte)(bpp == 1 ? 0: 128)) : null;
            var sourceImages = Icons.Information.ExtractBitmaps()
#if NET35
                .Cast<Image>()
#endif
                .Where(b => b != null);
            sourceImages.SaveAsAnimatedGif(ms, TimeSpan.FromMilliseconds(250), quantizer, OrderedDitherer.Bayer8x8);

            ms.Position = 0;
            var bmp = new Bitmap(ms);

            Assert.AreEqual(ImageFormat.Gif, bmp.RawFormat);
            Assert.AreEqual(PixelFormat.Format32bppArgb, bmp.PixelFormat);
            SaveImage($"{pixelFormat}", bmp, true);
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
        public void SaveAsJpegTest(PixelFormat pixelFormat)
        {
            var ms = new MemoryStream();
            IQuantizer quantizer = pixelFormat.IsIndexed() ? OptimizedPaletteQuantizer.Octree(1 << pixelFormat.ToBitsPerPixel()) : null;
            var refImage = Convert(Icons.Information.ExtractBitmap(new Size(256, 256)), pixelFormat, quantizer);

            refImage.SaveAsJpeg(ms);
            ms.Position = 0;
            var bmp = new Bitmap(ms);

            Assert.AreEqual(ImageFormat.Jpeg, bmp.RawFormat);
            Assert.AreEqual(PixelFormat.Format24bppRgb, bmp.PixelFormat);
            SaveImage($"{pixelFormat}", bmp, true);
        }

        [TestCase(PixelFormat.Format64bppArgb, PixelFormat.Format32bppArgb)]
        [TestCase(PixelFormat.Format64bppPArgb, PixelFormat.Format32bppArgb)]
        [TestCase(PixelFormat.Format48bppRgb, PixelFormat.Format24bppRgb)]
        [TestCase(PixelFormat.Format32bppArgb, PixelFormat.Format32bppArgb)]
        [TestCase(PixelFormat.Format32bppPArgb, PixelFormat.Format32bppArgb)]
        [TestCase(PixelFormat.Format32bppRgb, PixelFormat.Format32bppArgb)]
        [TestCase(PixelFormat.Format24bppRgb, PixelFormat.Format24bppRgb)]
        [TestCase(PixelFormat.Format16bppRgb565, PixelFormat.Format24bppRgb)]
        [TestCase(PixelFormat.Format16bppRgb555, PixelFormat.Format24bppRgb)]
        [TestCase(PixelFormat.Format16bppArgb1555, PixelFormat.Format32bppArgb)]
        [TestCase(PixelFormat.Format16bppGrayScale, PixelFormat.Format24bppRgb)]
        [TestCase(PixelFormat.Format8bppIndexed, PixelFormat.Format32bppArgb, PixelFormat.Format8bppIndexed)]
        [TestCase(PixelFormat.Format4bppIndexed, PixelFormat.Format32bppArgb, PixelFormat.Format4bppIndexed)]
        [TestCase(PixelFormat.Format1bppIndexed, PixelFormat.Format32bppArgb, PixelFormat.Format1bppIndexed)]
        public void SaveAsPngTest(PixelFormat pixelFormat, PixelFormat savedFormat, PixelFormat? savedFormatLinux = null)
        {
            var ms = new MemoryStream();
            IQuantizer quantizer = pixelFormat.IsIndexed() ? OptimizedPaletteQuantizer.Octree(1 << pixelFormat.ToBitsPerPixel()) : null;
            var refImage = Convert(Icons.Information.ExtractBitmap(new Size(256, 256)), pixelFormat, quantizer);

            refImage.SaveAsPng(ms);
            ms.Position = 0;
            var bmp = new Bitmap(ms);

            Assert.AreEqual(ImageFormat.Png, bmp.RawFormat);
            Assert.AreEqual(OSUtils.IsWindows ? savedFormat : savedFormatLinux ?? savedFormat, bmp.PixelFormat);
            SaveImage($"{pixelFormat}", bmp, true);
        }

        [TestCase(PixelFormat.Format64bppArgb, PixelFormat.Format32bppArgb)]
        [TestCase(PixelFormat.Format64bppPArgb, PixelFormat.Format32bppArgb)]
        [TestCase(PixelFormat.Format48bppRgb, PixelFormat.Format24bppRgb)]
        [TestCase(PixelFormat.Format32bppArgb, PixelFormat.Format32bppArgb)]
        [TestCase(PixelFormat.Format32bppPArgb, PixelFormat.Format32bppArgb)]
        [TestCase(PixelFormat.Format32bppRgb, PixelFormat.Format32bppArgb)]
        [TestCase(PixelFormat.Format24bppRgb, PixelFormat.Format24bppRgb)]
        [TestCase(PixelFormat.Format16bppRgb565, PixelFormat.Format24bppRgb)]
        [TestCase(PixelFormat.Format16bppRgb555, PixelFormat.Format24bppRgb)]
        [TestCase(PixelFormat.Format16bppArgb1555, PixelFormat.Format32bppArgb)]
        [TestCase(PixelFormat.Format16bppGrayScale, PixelFormat.Format8bppIndexed)]
        [TestCase(PixelFormat.Format8bppIndexed, PixelFormat.Format8bppIndexed, PixelFormat.Format32bppArgb)]
        [TestCase(PixelFormat.Format4bppIndexed, PixelFormat.Format4bppIndexed, PixelFormat.Format32bppArgb)]
        [TestCase(PixelFormat.Format1bppIndexed, PixelFormat.Format4bppIndexed, PixelFormat.Format32bppArgb)]
        public void SaveAsTiffTest(PixelFormat pixelFormat, PixelFormat savedFormat, PixelFormat? savedFormatLinux = null)
        {
            var ms = new MemoryStream();
            IQuantizer quantizer = pixelFormat.IsIndexed() ? OptimizedPaletteQuantizer.Octree(1 << pixelFormat.ToBitsPerPixel(), Color.Silver, 0) : null;
            var refImage = Convert(Icons.Information.ExtractBitmap(new Size(256, 256)), pixelFormat, quantizer);

            refImage.SaveAsTiff(ms);
            ms.Position = 0;
            var bmp = new Bitmap(ms);

            Assert.AreEqual(ImageFormat.Tiff, bmp.RawFormat);
            Assert.AreEqual(OSUtils.IsWindows ? savedFormat : savedFormatLinux ?? savedFormat, bmp.PixelFormat);
            SaveImage($"{pixelFormat}", bmp, true);
        }

        [Test]
        public void SaveAsTiffAllFramesTest()
        {
            AssertPlatformDependent(() =>
            {
                using var ms = new MemoryStream();
                using var refImage = Icons.Information.ToMultiResBitmap();

                refImage.SaveAsTiff(ms, false);
                ms.Position = 0;
                var bmp = new Bitmap(ms);

                Assert.AreEqual(ImageFormat.Tiff, bmp.RawFormat);
                Assert.AreEqual(Icons.Information.GetImagesCount(), bmp.GetFrameCount(FrameDimension.Page));

                SaveImage(null, bmp, true);
            }, PlatformID.Win32NT);
        }

        [Test]
        public void SaveAsMultipageTiffTest()
        {
            AssertPlatformDependent(() =>
            {
                using var ms = new MemoryStream();
                var pages = Icons.Information.ExtractBitmaps();
                pages.SaveAsMultipageTiff(ms);
                ms.Position = 0;
                var tiff = new Bitmap(ms);
                Assert.AreEqual(ImageFormat.Tiff, tiff.RawFormat);
                Assert.AreEqual(pages.Length, tiff.GetFrameCount(FrameDimension.Page));
            }, PlatformID.Win32NT);
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
        public void SaveAsIconTest(PixelFormat pixelFormat)
        {
            var ms = new MemoryStream();
            IQuantizer quantizer = pixelFormat.IsIndexed() ? OptimizedPaletteQuantizer.Octree(1 << pixelFormat.ToBitsPerPixel()) : null;
            var refImage = Convert(Icons.Information.ExtractBitmap(new Size(64, 64)), pixelFormat, quantizer);

            refImage.SaveAsIcon(ms);
            ms.Position = 0;
            var bmp = new Bitmap(ms);

            Assert.AreEqual(ImageFormat.Icon, bmp.RawFormat);
            Assert.AreEqual(PixelFormat.Format32bppArgb, bmp.PixelFormat);
            SaveImage($"{pixelFormat}", bmp, true);
        }

        #endregion
    }
}
