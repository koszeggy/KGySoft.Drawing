#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BitmapDataFactoryTest.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2022 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using KGySoft.Diagnostics;
using KGySoft.Drawing.Imaging;
using KGySoft.Reflection;
using NUnit.Framework;

#endregion

namespace KGySoft.Drawing.UnitTests.Imaging
{
    [TestFixture]
    internal class BitmapDataFactoryTest : TestBase
    {
        #region Properties

        private static object[][] CustomNonIndexedBitmapDataTestSource
        {
            get
            {
                #region Local Methods

                static Color32 GetColor8bppGray(ICustomBitmapDataRow row, int x) => Color32.FromGray(row.UnsafeGetRefAs<byte>(x));

                static void SetColor8bppGray(ICustomBitmapDataRow row, int x, Color32 c) => row.UnsafeGetRefAs<byte>(x) = c.BlendWithBackground(row.BitmapData.BackColor).GetBrightness();

                static Color32 GetColor4bppArgb1111(ICustomBitmapDataRow<byte> row, int x)
                {
                    throw new NotImplementedException();
                }

                static void SetColor4bppArgb1111(ICustomBitmapDataRow row, int x, Color32 c)
                {
                    throw new NotImplementedException();
                }

                #endregion

                return new[]
                {
                    new object[] { "8bpp Gray", new PixelFormatInfo(8) { Grayscale = true }, new Func<ICustomBitmapDataRow, int, Color32>(GetColor8bppGray), new Action<ICustomBitmapDataRow, int, Color32>(SetColor8bppGray) },
                    //new object[] { "4bpp ARGB1111", new PixelFormatInfo(4) { HasAlpha = true }, new Func<ICustomBitmapDataRow<byte>, int, Color32>(GetColor4bppArgb1111), new Action<ICustomBitmapDataRow, int, Color32>(SetColor4bppArgb1111) },
                    //new object[] { "16bpp A8Gray8" },
                };
            }
        }

        #endregion

        #region Methods

        [Test]
        public void ValidationTest()
        {
            short[] buffer = null;
            var size = new Size(10, 10);
            var pixelFormat = PixelFormat.Format32bppArgb;
            Assert.Throws<ArgumentNullException>(() => BitmapDataFactory.CreateBitmapData(buffer, size, 16));

            // stride too small
            buffer = new short[10];
            int stride = 16;
            Exception e = Assert.Throws<ArgumentOutOfRangeException>(() => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormat));
            Assert.IsTrue(e.Message.StartsWith(Res.ImagingStrideTooSmall(pixelFormat.GetByteWidth(size.Width)), StringComparison.Ordinal));

            // stride is not multiple of element type
            stride = 45;
            e = Assert.Throws<ArgumentException>(() => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormat));
            Assert.IsTrue(e.Message.StartsWith(Res.ImagingStrideInvalid(typeof(short), sizeof(short)), StringComparison.Ordinal));

            // buffer is too small
            stride = 42;
            e = Assert.Throws<ArgumentException>(() => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormat));
            Assert.IsTrue(e.Message.StartsWith(Res.ImagingBufferLengthTooSmall(stride / sizeof(short) * size.Height), StringComparison.Ordinal));

            // pixel width is too large for 2D buffer
            var buffer2D = new short[10, 10];
            e = Assert.Throws<ArgumentOutOfRangeException>(() => BitmapDataFactory.CreateBitmapData(buffer2D, size.Width, pixelFormat));
            Assert.IsTrue(e.Message.StartsWith(Res.ImagingWidthTooLarge, StringComparison.Ordinal));
        }

        [Test]
        public void DisposeCallbackTest()
        {
            bool disposeCalled = false;
            void Dispose() => disposeCalled = true;

            using (BitmapDataFactory.CreateBitmapData(new int[10, 10], 10, disposeCallback: Dispose))
            {
            }

            Assert.IsTrue(disposeCalled);
        }

        [TestCase(PixelFormat.Format1bppIndexed)]
        [TestCase(PixelFormat.Format4bppIndexed)]
        [TestCase(PixelFormat.Format8bppIndexed)]
        [TestCase(PixelFormat.Format16bppGrayScale)]
        [TestCase(PixelFormat.Format16bppRgb555)]
        [TestCase(PixelFormat.Format16bppRgb565)]
        [TestCase(PixelFormat.Format16bppArgb1555)]
        [TestCase(PixelFormat.Format32bppArgb)]
        [TestCase(PixelFormat.Format32bppPArgb)]
        [TestCase(PixelFormat.Format48bppRgb)]
        [TestCase(PixelFormat.Format64bppArgb)]
        [TestCase(PixelFormat.Format64bppPArgb)]
        public void SupportedFormatsConsistencyTest(PixelFormat pixelFormat)
        {
            // 0.) Reference: native to self-allocating managed bitmap data
            IReadWriteBitmapData reference;
            using (Bitmap bmp = Icons.Information.ExtractBitmap(new Size(256, 256)))
            {
                using var bmpData = bmp.GetReadWriteBitmapData();
                int bpp = pixelFormat.ToBitsPerPixel();
                reference = bmpData.Clone(pixelFormat, bpp <= 8 ? OptimizedPaletteQuantizer.Wu(1 << bpp, Color.Silver, (byte)(bpp == 1 ? 0 : 128)) : null, OrderedDitherer.Bayer8x8);
            }

            SaveBitmapData(pixelFormat.ToString(), reference);

            // 1.) 1D array
            byte[] buf1D = new byte[reference.RowSize * reference.Height];
            using (IReadWriteBitmapData bmpData = BitmapDataFactory.CreateBitmapData(buf1D, reference.GetSize(), reference.RowSize, pixelFormat, reference.Palette))
            {
                reference.CopyTo(bmpData);
                AssertAreEqual(reference, bmpData);
            }

            // 2.) 2D array
            byte[,] buf2D = new byte[reference.Height, reference.RowSize];
            using (IReadWriteBitmapData bmpData = BitmapDataFactory.CreateBitmapData(buf2D, reference.Width, pixelFormat, reference.Palette))
            {
                reference.CopyTo(bmpData);
                AssertAreEqual(reference, bmpData);
            }

            // 3.) Unmanaged memory (note: DrawInto would cause and error here because allocated memory is not cleared)
            IntPtr memory = Marshal.AllocHGlobal(reference.RowSize * reference.Height);
            void DisposeUnmanaged() => Marshal.FreeHGlobal(memory);
            using (IReadWriteBitmapData bmpData = BitmapDataFactory.CreateBitmapData((IntPtr)memory, reference.GetSize(), reference.RowSize, pixelFormat, reference.Palette, null, DisposeUnmanaged))
            {
                reference.CopyTo(bmpData);
                AssertAreEqual(reference, bmpData);
            }
        }

        [TestCaseSource(nameof(CustomNonIndexedBitmapDataTestSource))]
        public void CustomNonIndexedBitmapDataTest(string testName, PixelFormatInfo pixelFormat, Func<ICustomBitmapDataRow, int, Color32> getColor, Action<ICustomBitmapDataRow, int, Color32> setColor)
        {
            Size size = new Size(128, 64);

            // Creating custom bitmap data in 3 different forms (1D/2D arrays and unmanaged memory)
            int stride = pixelFormat.PixelFormat.GetByteWidth(size.Width);
            stride = (stride + 7) / 8 * 8; // custom stride for 8 bytes boundary
            var buffer1D = new long[size.Height * stride / sizeof(long)];
            using IReadWriteBitmapData bitmapDataNonDithered = BitmapDataFactory.CreateBitmapData(buffer1D, size, stride, pixelFormat, getColor, setColor);
            var buffer2D = new short[size.Height, stride / sizeof(short)];
            using IReadWriteBitmapData bitmapDataDitheredContentIndependent = BitmapDataFactory.CreateBitmapData(buffer2D, size.Width, pixelFormat, getColor, setColor);
            IntPtr bufferUnmanaged = Marshal.AllocHGlobal(Buffer.ByteLength(buffer1D));
            using IReadWriteBitmapData bitmapDataDitheredContentDependent = BitmapDataFactory.CreateBitmapData(bufferUnmanaged, size, stride, pixelFormat, getColor, setColor, disposeCallback: () => Marshal.FreeHGlobal(bufferUnmanaged));

            using IReadWriteBitmapData referenceBitmapData = BitmapDataFactory.CreateBitmapData(size);

            OrderedDitherer contentIndependentDitherer = OrderedDitherer.Bayer8x8;
            ErrorDiffusionDitherer contentDependentDitherer = ErrorDiffusionDitherer.FloydSteinberg.ConfigureProcessingDirection(true);

            // PredefinedColorsQuantizer.FromBitmapData
            PredefinedColorsQuantizer referenceQuantizerNonDithered = PredefinedColorsQuantizer.FromBitmapData(bitmapDataNonDithered);
            Assert.IsNotNull(Reflector.GetField(referenceQuantizerNonDithered, "compatibleBitmapDataFactory"));
            PredefinedColorsQuantizer referenceQuantizerContentIndependentDitherer = PredefinedColorsQuantizer.FromBitmapData(bitmapDataDitheredContentIndependent);
            Assert.IsNotNull(Reflector.GetField(referenceQuantizerContentIndependentDitherer, "compatibleBitmapDataFactory"));
            PredefinedColorsQuantizer referenceQuantizerContentDependentDitherer = PredefinedColorsQuantizer.FromBitmapData(bitmapDataDitheredContentDependent);
            Assert.IsNotNull(Reflector.GetField(referenceQuantizerContentDependentDitherer, "compatibleBitmapDataFactory"));

            // CopyTo
            using IReadWriteBitmapData alphaGradient = GenerateAlphaGradientBitmapData(size);

            alphaGradient.CopyTo(bitmapDataNonDithered);
            SaveBitmapData($"{testName} CopyTo", bitmapDataNonDithered);
            alphaGradient.CopyTo(referenceBitmapData, default, referenceQuantizerNonDithered);
            AssertAreEqual(referenceBitmapData, bitmapDataNonDithered, true);

            alphaGradient.CopyTo(bitmapDataDitheredContentIndependent, default, contentIndependentDitherer);
            SaveBitmapData($"{testName} CopyTo independent ditherer", bitmapDataDitheredContentIndependent);
            alphaGradient.CopyTo(referenceBitmapData, default, referenceQuantizerContentIndependentDitherer, contentIndependentDitherer);
            AssertAreEqual(referenceBitmapData, bitmapDataDitheredContentIndependent, true);

            alphaGradient.CopyTo(bitmapDataDitheredContentDependent, default, contentDependentDitherer);
            SaveBitmapData($"{testName} CopyTo dependent ditherer", bitmapDataDitheredContentDependent);
            alphaGradient.CopyTo(referenceBitmapData, default, referenceQuantizerContentDependentDitherer, contentDependentDitherer);
            AssertAreEqual(referenceBitmapData, bitmapDataDitheredContentDependent, true);

            // Clone with original pixel format
            using (IReadWriteBitmapData clone = bitmapDataNonDithered.Clone())
                AssertAreEqual(bitmapDataNonDithered, clone);
            using (IReadWriteBitmapData clone = bitmapDataDitheredContentIndependent.Clone())
                AssertAreEqual(bitmapDataDitheredContentIndependent, clone);
            using (IReadWriteBitmapData clone = bitmapDataDitheredContentDependent.Clone())
                AssertAreEqual(bitmapDataDitheredContentDependent, clone);

            // Clone with known pixel format
            using (IReadWriteBitmapData clone = bitmapDataNonDithered.Clone(bitmapDataNonDithered.PixelFormat.ToKnownPixelFormat()))
                AssertAreEqual(bitmapDataNonDithered, clone, true);
            using (IReadWriteBitmapData clone = bitmapDataDitheredContentIndependent.Clone(bitmapDataDitheredContentIndependent.PixelFormat.ToKnownPixelFormat(), contentIndependentDitherer))
                AssertAreEqual(bitmapDataDitheredContentIndependent, clone, true);
            using (IReadWriteBitmapData clone = bitmapDataDitheredContentDependent.Clone(bitmapDataDitheredContentDependent.PixelFormat.ToKnownPixelFormat(), contentDependentDitherer))
                AssertAreEqual(bitmapDataDitheredContentDependent, clone, true);

            // Clear
            bitmapDataNonDithered.Clear(new Color32(Color.Silver));
            SaveBitmapData($"{testName} Clear solid", bitmapDataNonDithered);
            bitmapDataNonDithered.CopyTo(referenceBitmapData, default, referenceQuantizerNonDithered);
            AssertAreEqual(bitmapDataNonDithered, referenceBitmapData, true);

            bitmapDataDitheredContentIndependent.Clear(new Color32(Color.Silver), contentIndependentDitherer);
            SaveBitmapData($"{testName} Clear independent ditherer", bitmapDataDitheredContentIndependent);
            bitmapDataDitheredContentIndependent.CopyTo(referenceBitmapData, default, referenceQuantizerContentIndependentDitherer);
            AssertAreEqual(bitmapDataDitheredContentIndependent, referenceBitmapData, true);

            bitmapDataDitheredContentDependent.Clear(new Color32(Color.Silver), contentDependentDitherer);
            SaveBitmapData($"{testName} Clear dependent ditherer", bitmapDataDitheredContentDependent);
            bitmapDataNonDithered.CopyTo(bitmapDataDitheredContentDependent, default, referenceQuantizerContentDependentDitherer);
            AssertAreEqual(bitmapDataDitheredContentDependent, referenceBitmapData, true);

            // DrawInto
            using IReadableBitmapData icon32 = Icons.Information.ExtractBitmap(new Size(48, 48)).GetReadableBitmapData();
            Point iconLocation = new Point(10, 10);
            Rectangle gradientRectangle = new Rectangle(60, 10, 50, 42);

            bitmapDataNonDithered.CopyTo(referenceBitmapData, default, referenceQuantizerNonDithered);
            icon32.DrawInto(referenceBitmapData, iconLocation, referenceQuantizerNonDithered);
            alphaGradient.DrawInto(referenceBitmapData, gradientRectangle, referenceQuantizerNonDithered);
            icon32.DrawInto(bitmapDataNonDithered, iconLocation);
            alphaGradient.DrawInto(bitmapDataNonDithered, gradientRectangle);
            SaveBitmapData($"{testName} DrawInto", bitmapDataNonDithered);
            AssertAreEqual(referenceBitmapData, bitmapDataNonDithered, true);

            bitmapDataDitheredContentIndependent.CopyTo(referenceBitmapData, default, referenceQuantizerContentIndependentDitherer);
            icon32.DrawInto(referenceBitmapData, iconLocation, referenceQuantizerContentIndependentDitherer, contentIndependentDitherer);
            alphaGradient.DrawInto(referenceBitmapData, gradientRectangle, referenceQuantizerContentIndependentDitherer, contentIndependentDitherer);
            icon32.DrawInto(bitmapDataDitheredContentIndependent, iconLocation, contentIndependentDitherer);
            alphaGradient.DrawInto(bitmapDataDitheredContentIndependent, gradientRectangle, contentIndependentDitherer);
            SaveBitmapData($"{testName} DrawInto independent ditherer", bitmapDataDitheredContentIndependent);
            AssertAreEqual(referenceBitmapData, bitmapDataDitheredContentIndependent, true);

            bitmapDataDitheredContentDependent.CopyTo(referenceBitmapData, default, referenceQuantizerContentDependentDitherer);
            icon32.DrawInto(referenceBitmapData, iconLocation, referenceQuantizerContentDependentDitherer, contentDependentDitherer);
            alphaGradient.DrawInto(referenceBitmapData, gradientRectangle, referenceQuantizerContentDependentDitherer, contentDependentDitherer);
            icon32.DrawInto(bitmapDataDitheredContentDependent, iconLocation, contentDependentDitherer);
            alphaGradient.DrawInto(bitmapDataDitheredContentDependent, gradientRectangle, contentDependentDitherer);
            SaveBitmapData($"{testName} DrawInto dependent ditherer", bitmapDataDitheredContentDependent);
            AssertAreEqual(referenceBitmapData, bitmapDataDitheredContentDependent, true, tolerance: 1);

            // Clip + Clone region
            var clippingRegion = new Rectangle(Point.Empty, new Size(50, 50));
            using (IReadWriteBitmapData clipped = bitmapDataNonDithered.Clip(clippingRegion))
            {
                using IReadWriteBitmapData clonedRegion = bitmapDataNonDithered.Clone(clippingRegion);
                AssertAreEqual(clipped, clonedRegion);
            }

            using (IReadWriteBitmapData clipped = bitmapDataDitheredContentIndependent.Clip(clippingRegion))
            {
                using IReadWriteBitmapData clonedRegion = bitmapDataDitheredContentIndependent.Clone(clippingRegion);
                AssertAreEqual(clipped, clonedRegion);
            }

            using (IReadWriteBitmapData clipped = bitmapDataDitheredContentDependent.Clip(clippingRegion))
            {
                using IReadWriteBitmapData clonedRegion = bitmapDataDitheredContentDependent.Clone(clippingRegion);
                AssertAreEqual(clipped, clonedRegion);
            }

            // TransformColors (Invert)
            bitmapDataNonDithered.CopyTo(referenceBitmapData, default, referenceQuantizerNonDithered);
            referenceBitmapData.Invert();
            referenceBitmapData.Quantize(referenceQuantizerNonDithered);
            bitmapDataNonDithered.Invert();
            SaveBitmapData($"{testName} TransformColors", bitmapDataNonDithered);
            AssertAreEqual(referenceBitmapData, bitmapDataNonDithered, true);

            bitmapDataDitheredContentIndependent.CopyTo(referenceBitmapData, default, referenceQuantizerContentIndependentDitherer);
            referenceBitmapData.Invert(contentIndependentDitherer);
            referenceBitmapData.Quantize(referenceQuantizerContentIndependentDitherer);
            bitmapDataDitheredContentIndependent.Invert(contentIndependentDitherer);
            SaveBitmapData($"{testName} TransformColors independent ditherer", bitmapDataDitheredContentIndependent);
            AssertAreEqual(referenceBitmapData, bitmapDataDitheredContentIndependent, true);

            bitmapDataDitheredContentDependent.CopyTo(referenceBitmapData, default, referenceQuantizerContentDependentDitherer);
            referenceBitmapData.Invert(contentDependentDitherer);
            referenceBitmapData.Quantize(referenceQuantizerContentDependentDitherer);
            bitmapDataDitheredContentDependent.Invert(contentDependentDitherer);
            SaveBitmapData($"{testName} TransformColors dependent ditherer", bitmapDataDitheredContentDependent);
            AssertAreEqual(referenceBitmapData, bitmapDataDitheredContentDependent, true);

            // Save/Load
            using var ms = new MemoryStream();
            bitmapDataDitheredContentDependent.Save(ms);
            ms.Position = 0L;
            using IReadWriteBitmapData reloaded = BitmapDataFactory.Load(ms);
            AssertAreEqual(bitmapDataDitheredContentDependent, reloaded, true);
        }

        [Test]
        public void CustomIndexedBitmapDataTest()
        {
            throw new NotImplementedException();

            // TODO
            // - TrySetPalette
            // - Clear (with/out dithering)
            // - Clone (no param: find a close format, or ARGB32) - tests also CopyTo as source / with dithering
            // - Alpha gradient CopyTo - tests CopyTo as a target
            //   - With/out dithering (compatible quantizer?)
            // - Alpha gradient DrawInto
            //   - With/out resize
            //   - With/out dithering
            // - TransformColors (eg. Invert) - uses FromBitmapData when dithering
            // - Quantizer
            //   - FromBitmapData
            //   - FromPixelFormat
            //   - FromPalette
            //   - FromColors
            //   - Optimized (> 256 colors)
            //     - Octree
            //     - MedianCut
            //     - Wu
            // - Save/Load
        }

        #endregion
    }
}
