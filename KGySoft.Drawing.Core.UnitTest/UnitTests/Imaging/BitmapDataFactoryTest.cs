#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BitmapDataFactoryTest.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2023 - All Rights Reserved
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
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

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

                static Color32 GetColor8BppGray(ICustomBitmapDataRow row, int x) => Color32.FromGray(row.UnsafeGetRefAs<byte>(x));
                static void SetColor8BppGray(ICustomBitmapDataRow row, int x, Color32 c) => row.UnsafeGetRefAs<byte>(x) =
                    c.Blend(row.BitmapData.BackColor, row.BitmapData.BlendingMode == BlendingMode.Linear).GetBrightness();

                static Color32 GetColor4BppArgb1111(ICustomBitmapDataRow row, int x)
                {
                    int nibbles = row.GetRefAs<byte>(x >> 1);
                    int color = (x & 1) == 0
                        ? nibbles >> 4
                        : nibbles & 0b00001111;
                    return new Color32((byte)((color & 8) == 0 ? 0 : 255),
                        (byte)((color & 4) == 0 ? 0 : 255),
                        (byte)((color & 2) == 0 ? 0 : 255),
                        (byte)((color & 1) == 0 ? 0 : 255));
                }

                static void SetColor4BppArgb1111(ICustomBitmapDataRow row, int x, Color32 c)
                {
                    ref byte nibbles = ref row.GetRefAs<byte>(x >> 1);
                    if (c.A != 255)
                        c = c.A >= row.BitmapData.AlphaThreshold ? c.Blend(row.BitmapData.BackColor, row.BitmapData.BlendingMode == BlendingMode.Linear) : default;
                    int color = ((c.A & 128) >> 4)
                        | ((c.R & 128) >> 5)
                        | ((c.G & 128) >> 6)
                        | ((c.B & 128) >> 7);
                    if ((x & 1) == 0)
                    {
                        nibbles &= 0b00001111;
                        nibbles |= (byte)(color << 4);
                    }
                    else
                    {
                        nibbles &= 0b11110000;
                        nibbles |= (byte)color;
                    }
                }

                static Color32 GetColor128Bpp(ICustomBitmapDataRow row, int x) => row.UnsafeGetRefAs<ColorF>(x).ToColor32();
                static void SetColor128Bpp(ICustomBitmapDataRow row, int x, Color32 c) => row.UnsafeGetRefAs<ColorF>(x) = new ColorF(c);

                static Color32 GetColor9BppGray(ICustomBitmapDataRow row, int x)
                {
                    int bitPos = x * 9;
                    int bytePos = bitPos >> 3;
                    int bits = row.UnsafeGetRefAs<byte>(bytePos) | (row.UnsafeGetRefAs<byte>(bytePos + 1) << 8);
                    int offset = bitPos % 8;
                    bits = (bits >> offset) & 511;
                    return (bits & 256) == 0 ? Color32.Transparent : Color32.FromGray((byte)bits);
                }

                static void SetColor9BppGray(ICustomBitmapDataRow row, int x, Color32 c)
                {
                    int bitPos = x * 9;
                    int bytePos = bitPos >> 3;
                    int bits = row.UnsafeGetRefAs<byte>(bytePos) | (row.UnsafeGetRefAs<byte>(bytePos + 1) << 8);
                    int offset = bitPos % 8;
                    bits &= ~(511 << offset);
                    if (c.A >= row.BitmapData.AlphaThreshold)
                        bits |= (256 | c.Blend(row.BitmapData.BackColor, row.BitmapData.BlendingMode == BlendingMode.Linear).GetBrightness()) << offset;
                    row.UnsafeGetRefAs<byte>(bytePos) = (byte)bits;
                    row.UnsafeGetRefAs<byte>(bytePos + 1) = (byte)(bits >> 8);
                }

                #endregion

                return new[]
                {
                    new object[] { "8bpp Gray", new PixelFormatInfo(8) { Grayscale = true }, new Func<ICustomBitmapDataRow, int, Color32>(GetColor8BppGray), new Action<ICustomBitmapDataRow, int, Color32>(SetColor8BppGray) },
                    new object[] { "4bpp ARGB1111", new PixelFormatInfo(4) { HasSingleBitAlpha = true }, new Func<ICustomBitmapDataRow, int, Color32>(GetColor4BppArgb1111), new Action<ICustomBitmapDataRow, int, Color32>(SetColor4BppArgb1111) },
                    new object[] { "128bpp ColorF", new PixelFormatInfo(128) { HasAlpha = true }, new Func<ICustomBitmapDataRow, int, Color32>(GetColor128Bpp), new Action<ICustomBitmapDataRow, int, Color32>(SetColor128Bpp) },
                    new object[] { "9bpp Gray", new PixelFormatInfo(9) { Grayscale = true, HasSingleBitAlpha = true }, new Func<ICustomBitmapDataRow, int, Color32>(GetColor9BppGray), new Action<ICustomBitmapDataRow, int, Color32>(SetColor9BppGray) },
                };
            }
        }

        private static object[][] CustomIndexedBitmapDataTestSource
        {
            get
            {
                #region Local Methods

                static int GetColorIndex1Bpp(ICustomBitmapDataRow row, int x)
                {
                    // bit order is not the same as for usual 1/4bpp formats: bits are filled up from LSB to MSB
                    int mask = 1 << (x & 7);
                    int bits = row.GetRefAs<byte>(x >> 3);
                    return (bits & mask) != 0 ? 1 : 0;
                }

                static void SetColorIndex1Bpp(ICustomBitmapDataRow row, int x, int colorIndex)
                {
                    int mask = 1 << (x & 7);
                    ref byte bits = ref row.UnsafeGetRefAs<byte>(x >> 3);
                    if (colorIndex == 0)
                        bits = (byte)(bits & ~mask);
                    else
                        bits = (byte)(bits | mask);

                    Assert.AreEqual(colorIndex, GetColorIndex1Bpp(row, x));
                }

                static int GetColorIndex3Bpp(ICustomBitmapDataRow row, int x)
                {
                    // bit order is not the same as for usual 1/4bpp formats: bits are filled up from LSB to MSB
                    int bitPos = x * 3;
                    int bytePos = bitPos >> 3;
                    int bits = row.UnsafeGetRefAs<byte>(bytePos);
                    int offset = bitPos % 8;

                    // the pixel spans two bytes
                    if (offset >= 6)
                        bits |= row.UnsafeGetRefAs<byte>(bytePos + 1) << 8;

                    return (bits >> offset) & 7;
                }

                static void SetColorIndex3Bpp(ICustomBitmapDataRow row, int x, int colorIndex)
                {
                    Assert.IsTrue(colorIndex is >= 0 and < 8);
                    int bitPos = x * 3;
                    int bytePos = bitPos >> 3;
                    int bits = row.UnsafeGetRefAs<byte>(bytePos);
                    int offset = bitPos % 8;

                    // the pixel spans two bytes
                    if (offset >= 6)
                        bits |= row.UnsafeGetRefAs<byte>(bytePos + 1) << 8;

                    bits &= ~(7 << offset);
                    bits |= colorIndex << offset;
                    row.UnsafeGetRefAs<byte>(bytePos) = (byte)bits;
                    if (offset >= 6)
                        row.UnsafeGetRefAs<byte>(bytePos + 1) = (byte)(bits >> 8);

                    Assert.AreEqual(colorIndex, GetColorIndex3Bpp(row, x));
                }

                static Palette GetPalette3Bpp() => new Palette(new[] { Color.Black, Color.White, Color.Red, Color.Yellow, Color.Lime, Color.Cyan, Color.Blue, Color.Magenta });

                static int GetColorIndex9Bpp(ICustomBitmapDataRow row, int x)
                {
                    // bit order is not the same as for usual 1/4bpp formats: bits are filled up from LSB to MSB
                    int bitPos = x * 9;
                    int bytePos = bitPos >> 3;
                    int bits = row.UnsafeGetRefAs<byte>(bytePos) | (row.UnsafeGetRefAs<byte>(bytePos + 1) << 8);
                    int offset = bitPos % 8;
                    return (bits >> offset) & 511;
                }

                static void SetColorIndex9Bpp(ICustomBitmapDataRow row, int x, int colorIndex)
                {
                    Assert.IsTrue(colorIndex is >= 0 and < 512);
                    int bitPos = x * 9;
                    int bytePos = bitPos >> 3;
                    int bits = row.UnsafeGetRefAs<byte>(bytePos) | (row.UnsafeGetRefAs<byte>(bytePos + 1) << 8);
                    int offset = bitPos % 8;
                    bits &= ~(511 << offset);
                    bits |= colorIndex << offset;
                    row.UnsafeGetRefAs<byte>(bytePos) = (byte)bits;
                    row.UnsafeGetRefAs<byte>(bytePos + 1) = (byte)(bits >> 8);

                    Assert.AreEqual(colorIndex, GetColorIndex9Bpp(row, x));
                }

                static Palette GetPalette9Bpp()
                {
                    var result = new Color32[512];
                    for (int i = 0; i < 512; i++)
                    {
                        byte r = (byte)((i & 0b111_000_000) >> 1);
                        r |= (byte)((r >> 3) | (r >> 6));
                        byte g = (byte)((i & 0b000_111_000) << 2);
                        g |= (byte)((g >> 3) | (g >> 6));
                        byte b = (byte)((i & 0b000_000_111) << 5);
                        b |= (byte)((b >> 3) | (b >> 6));
                        result[i] = new Color32(r, g, b);
                    }

                    return new Palette(result);
                }

                static int GetColorIndex16Bpp(ICustomBitmapDataRow row, int x) => row.UnsafeGetRefAs<short>(x);
                static void SetColorIndex16Bpp(ICustomBitmapDataRow row, int x, int c) => row.UnsafeGetRefAs<short>(x) = (short)c;

                #endregion

                return new[]
                {
                    new object[] { "1bpp Indexed", new PixelFormatInfo(1) { Indexed = true }, new Func<ICustomBitmapDataRow, int, int>(GetColorIndex1Bpp), new Action<ICustomBitmapDataRow, int, int>(SetColorIndex1Bpp), Palette.SystemDefault1BppPalette() },
                    new object[] { "3bpp Indexed", new PixelFormatInfo(3) { Indexed = true }, new Func<ICustomBitmapDataRow, int, int>(GetColorIndex3Bpp), new Action<ICustomBitmapDataRow, int, int>(SetColorIndex3Bpp), GetPalette3Bpp() },
                    new object[] { "9bpp Indexed", new PixelFormatInfo(9) { Indexed = true }, new Func<ICustomBitmapDataRow, int, int>(GetColorIndex9Bpp), new Action<ICustomBitmapDataRow, int, int>(SetColorIndex9Bpp), GetPalette9Bpp() },
                    new object[] { "16bpp Indexed", new PixelFormatInfo(16) { Indexed = true }, new Func<ICustomBitmapDataRow, int, int>(GetColorIndex16Bpp), new Action<ICustomBitmapDataRow, int, int>(SetColorIndex16Bpp), Palette.SystemDefault8BppPalette() },
                };
            }
        }

        #endregion

        #region Methods

        #region Static Methods

        private static void DoCommonCustomBitmapDataTests(string caseName, Size size, IReadWriteBitmapData bitmapDataNonDithered, IReadWriteBitmapData bitmapDataDitheredContentIndependent, IReadWriteBitmapData bitmapDataDitheredContentDependent, [CallerMemberName] string testName = null)
        {
            using IReadWriteBitmapData referenceBitmapData = BitmapDataFactory.CreateBitmapData(size);

            OrderedDitherer contentIndependentDitherer = OrderedDitherer.Bayer8x8;
            ErrorDiffusionDitherer contentDependentDitherer = ErrorDiffusionDitherer.FloydSteinberg.ConfigureProcessingDirection(true);
            PredefinedColorsQuantizer referenceQuantizer = bitmapDataNonDithered.PixelFormat.CanBeDithered || bitmapDataNonDithered.PixelFormat.Grayscale ? PredefinedColorsQuantizer.FromBitmapData(bitmapDataNonDithered) : null;
            Assert.IsTrue(referenceQuantizer == null || referenceQuantizer.PixelFormatHint.IsIndexed() || Reflector.TryGetField(referenceQuantizer, "compatibleBitmapDataFactory", out var _));

            // CopyTo
            using IReadWriteBitmapData alphaGradient = GenerateAlphaGradientBitmapData(size);
            alphaGradient.CopyTo(bitmapDataNonDithered);
            SaveBitmapData($"{caseName} CopyTo", bitmapDataNonDithered, testName);
            alphaGradient.CopyTo(referenceBitmapData, default, referenceQuantizer);
            AssertAreEqual(referenceBitmapData, bitmapDataNonDithered, true);

            alphaGradient.CopyTo(bitmapDataDitheredContentIndependent, default, contentIndependentDitherer);
            SaveBitmapData($"{caseName} CopyTo independent ditherer", bitmapDataDitheredContentIndependent, testName);
            alphaGradient.CopyTo(referenceBitmapData, default, referenceQuantizer, contentIndependentDitherer);
            AssertAreEqual(referenceBitmapData, bitmapDataDitheredContentIndependent, true);

            alphaGradient.CopyTo(bitmapDataDitheredContentDependent, default, contentDependentDitherer);
            SaveBitmapData($"{caseName} CopyTo dependent ditherer", bitmapDataDitheredContentDependent, testName);
            alphaGradient.CopyTo(referenceBitmapData, default, referenceQuantizer, contentDependentDitherer);
            AssertAreEqual(referenceBitmapData, bitmapDataDitheredContentDependent, true);

            // Clone with original or indexed pixel format
            using (IReadWriteBitmapData clone = bitmapDataNonDithered.Clone())
                AssertAreEqual(bitmapDataNonDithered, clone, bitmapDataNonDithered.PixelFormat.Indexed);
            using (IReadWriteBitmapData clone = bitmapDataDitheredContentIndependent.Clone())
                AssertAreEqual(bitmapDataDitheredContentIndependent, clone, bitmapDataDitheredContentIndependent.PixelFormat.Indexed);
            using (IReadWriteBitmapData clone = bitmapDataDitheredContentDependent.Clone())
                AssertAreEqual(bitmapDataDitheredContentDependent, clone, bitmapDataDitheredContentDependent.PixelFormat.Indexed);

            // Clone with known pixel format
            using (IReadWriteBitmapData clone = bitmapDataNonDithered.Clone(bitmapDataNonDithered.GetKnownPixelFormat()))
                AssertAreEqual(bitmapDataNonDithered, clone, true);
            using (IReadWriteBitmapData clone = bitmapDataDitheredContentIndependent.Clone(bitmapDataDitheredContentIndependent.GetKnownPixelFormat(), contentIndependentDitherer))
                AssertAreEqual(bitmapDataDitheredContentIndependent, clone, true);
            using (IReadWriteBitmapData clone = bitmapDataDitheredContentDependent.Clone(bitmapDataDitheredContentDependent.GetKnownPixelFormat(), contentDependentDitherer))
                AssertAreEqual(bitmapDataDitheredContentDependent, clone, true);

            // Clear
            bitmapDataNonDithered.Clear(new Color32(Color.Silver));
            SaveBitmapData($"{caseName} Clear solid", bitmapDataNonDithered, testName);
            bitmapDataNonDithered.CopyTo(referenceBitmapData, default, referenceQuantizer);
            AssertAreEqual(bitmapDataNonDithered, referenceBitmapData, true);

            bitmapDataDitheredContentIndependent.Clear(new Color32(Color.Silver), contentIndependentDitherer);
            SaveBitmapData($"{caseName} Clear independent ditherer", bitmapDataDitheredContentIndependent, testName);
            bitmapDataDitheredContentIndependent.CopyTo(referenceBitmapData, default, referenceQuantizer);
            AssertAreEqual(bitmapDataDitheredContentIndependent, referenceBitmapData, true);

            bitmapDataDitheredContentDependent.Clear(new Color32(Color.Silver), contentDependentDitherer);
            SaveBitmapData($"{caseName} Clear dependent ditherer", bitmapDataDitheredContentDependent, testName);
            bitmapDataDitheredContentDependent.CopyTo(referenceBitmapData, default, referenceQuantizer);
            AssertAreEqual(bitmapDataDitheredContentDependent, referenceBitmapData, true);

            // DrawInto
            using IReadableBitmapData icon48 = GetInfoIcon48();
            Point iconLocation = new Point(10, 10);
            Rectangle gradientRectangle = new Rectangle(60, 10, 50, 42);

            bitmapDataNonDithered.CopyTo(referenceBitmapData, default, referenceQuantizer);
            icon48.DrawInto(referenceBitmapData, iconLocation, referenceQuantizer);
            alphaGradient.DrawInto(referenceBitmapData, gradientRectangle, referenceQuantizer);
            icon48.DrawInto(bitmapDataNonDithered, iconLocation);
            alphaGradient.DrawInto(bitmapDataNonDithered, gradientRectangle);
            SaveBitmapData($"{caseName} DrawInto", bitmapDataNonDithered, testName);
            AssertAreEqual(referenceBitmapData, bitmapDataNonDithered, true);

            bitmapDataDitheredContentIndependent.CopyTo(referenceBitmapData, default, referenceQuantizer);
            icon48.DrawInto(referenceBitmapData, iconLocation, referenceQuantizer, contentIndependentDitherer);
            alphaGradient.DrawInto(referenceBitmapData, gradientRectangle, referenceQuantizer, contentIndependentDitherer);
            icon48.DrawInto(bitmapDataDitheredContentIndependent, iconLocation, contentIndependentDitherer);
            alphaGradient.DrawInto(bitmapDataDitheredContentIndependent, gradientRectangle, contentIndependentDitherer);
            SaveBitmapData($"{caseName} DrawInto independent ditherer", bitmapDataDitheredContentIndependent, testName);
            AssertAreEqual(referenceBitmapData, bitmapDataDitheredContentIndependent, true);

            bitmapDataDitheredContentDependent.CopyTo(referenceBitmapData, default, referenceQuantizer);
            icon48.DrawInto(referenceBitmapData, iconLocation, referenceQuantizer, contentDependentDitherer);
            alphaGradient.DrawInto(referenceBitmapData, gradientRectangle, referenceQuantizer, contentDependentDitherer);
            icon48.DrawInto(bitmapDataDitheredContentDependent, iconLocation, contentDependentDitherer);
            alphaGradient.DrawInto(bitmapDataDitheredContentDependent, gradientRectangle, contentDependentDitherer);
            SaveBitmapData($"{caseName} DrawInto dependent ditherer", bitmapDataDitheredContentDependent, testName);
            //AssertAreEqual(referenceBitmapData, bitmapDataDitheredContentDependent, true); //- Due to serpentine processing the resizing draw can be different on 32bpp reference and actual bitmap data

            // Clip + Clone region
            var clippingRegion = new Rectangle(Point.Empty, new Size(50, 50));
            using (IReadWriteBitmapData clipped = bitmapDataNonDithered.Clip(clippingRegion))
            {
                using IReadWriteBitmapData clonedRegion = bitmapDataNonDithered.Clone(clippingRegion);
                AssertAreEqual(clipped, clonedRegion, bitmapDataDitheredContentDependent.PixelFormat.Indexed);
            }

            using (IReadWriteBitmapData clipped = bitmapDataDitheredContentIndependent.Clip(clippingRegion))
            {
                using IReadWriteBitmapData clonedRegion = bitmapDataDitheredContentIndependent.Clone(clippingRegion);
                AssertAreEqual(clipped, clonedRegion, bitmapDataDitheredContentDependent.PixelFormat.Indexed);
            }

            using (IReadWriteBitmapData clipped = bitmapDataDitheredContentDependent.Clip(clippingRegion))
            {
                using IReadWriteBitmapData clonedRegion = bitmapDataDitheredContentDependent.Clone(clippingRegion);
                AssertAreEqual(clipped, clonedRegion, bitmapDataDitheredContentDependent.PixelFormat.Indexed);
            }

            // TransformColors (Invert)
            bitmapDataNonDithered.CopyTo(referenceBitmapData, default, referenceQuantizer);
            referenceBitmapData.Invert();
            if (referenceQuantizer != null && !bitmapDataNonDithered.PixelFormat.Indexed)
                // not quantizing reference with original indexed palette because without a ditherer the palette entries themselves are inverted
                referenceBitmapData.Quantize(referenceQuantizer);
            bitmapDataNonDithered.Invert();
            SaveBitmapData($"{caseName} TransformColors", bitmapDataNonDithered, testName);
            AssertAreEqual(referenceBitmapData, bitmapDataNonDithered, true);

            bitmapDataDitheredContentIndependent.CopyTo(referenceBitmapData, default, referenceQuantizer);
            referenceBitmapData.Invert(contentIndependentDitherer);
            if (referenceQuantizer != null)
                referenceBitmapData.Dither(referenceQuantizer, contentIndependentDitherer);
            bitmapDataDitheredContentIndependent.Invert(contentIndependentDitherer);
            SaveBitmapData($"{caseName} TransformColors independent ditherer", bitmapDataDitheredContentIndependent, testName);
            AssertAreEqual(referenceBitmapData, bitmapDataDitheredContentIndependent, true);

            bitmapDataDitheredContentDependent.CopyTo(referenceBitmapData, default, referenceQuantizer);
            referenceBitmapData.Invert(contentDependentDitherer);
            if (referenceQuantizer != null)
                referenceBitmapData.Dither(referenceQuantizer, contentDependentDitherer);
            bitmapDataDitheredContentDependent.Invert(contentDependentDitherer);
            SaveBitmapData($"{caseName} TransformColors dependent ditherer", bitmapDataDitheredContentDependent, testName);
            AssertAreEqual(referenceBitmapData, bitmapDataDitheredContentDependent, true);

            // Save/Load
            using var ms = new MemoryStream();
            bitmapDataDitheredContentDependent.Save(ms);
            ms.Position = 0L;
            using IReadWriteBitmapData reloaded = BitmapDataFactory.Load(ms);
            AssertAreEqual(bitmapDataDitheredContentDependent, reloaded, true);
        }

        #endregion

        #region Instance Methods

        [Test]
        public void ValidationTest()
        {
            short[] buffer = null;
            var size = new Size(10, 10);
            var pixelFormat = KnownPixelFormat.Format32bppArgb;
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

            // indexed pixel format is too large
            buffer = new short[2];
            e = Assert.Throws<ArgumentException>(() => BitmapDataFactory.CreateBitmapData(buffer, new Size(1, 1), 4, new PixelFormatInfo(32) {Indexed = true}, (row, x) => row.GetRefAs<int>(x), (row, x, c) => row.GetRefAs<int>(x) = c));
            Assert.IsTrue(e.Message.StartsWith(Res.ImagingIndexedPixelFormatTooLarge, StringComparison.Ordinal));
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

        [Test]
        public void FinalizeTest()
        {
            static void CreateAndAbandon(StrongBox<bool> isDisposed)
            {
                IntPtr mem = Marshal.AllocHGlobal(1);
                BitmapDataFactory.CreateBitmapData(mem, new Size(1, 1), 1, KnownPixelFormat.Format8bppIndexed, disposeCallback: () => Free(mem, isDisposed));
            }

            static void Free(IntPtr ptr, StrongBox<bool> isDisposed)
            {
                Marshal.FreeHGlobal(ptr);
                isDisposed.Value = true;
            }

            var isDisposedTracker = new StrongBox<bool>(false);
            CreateAndAbandon(isDisposedTracker);
            Assert.IsFalse(isDisposedTracker.Value);
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Assert.IsTrue(isDisposedTracker.Value);
        }

        [TestCase(KnownPixelFormat.Format1bppIndexed)]
        [TestCase(KnownPixelFormat.Format4bppIndexed)]
        [TestCase(KnownPixelFormat.Format8bppIndexed)]
        [TestCase(KnownPixelFormat.Format16bppGrayScale)]
        [TestCase(KnownPixelFormat.Format16bppRgb555)]
        [TestCase(KnownPixelFormat.Format16bppRgb565)]
        [TestCase(KnownPixelFormat.Format16bppArgb1555)]
        [TestCase(KnownPixelFormat.Format32bppArgb)]
        [TestCase(KnownPixelFormat.Format32bppPArgb)]
        [TestCase(KnownPixelFormat.Format48bppRgb)]
        [TestCase(KnownPixelFormat.Format64bppArgb)]
        [TestCase(KnownPixelFormat.Format64bppPArgb)]
        public void SupportedFormatsConsistencyTest(KnownPixelFormat pixelFormat)
        {
            // 0.) Reference: native to self-allocating managed bitmap data
            IReadWriteBitmapData reference;
            using (var bmpData = GetInfoIcon256())
            {
                int bpp = pixelFormat.ToBitsPerPixel();
                reference = bmpData.Clone(pixelFormat, bpp <= 8 ? OptimizedPaletteQuantizer.Wu(1 << bpp, Color.Silver, (byte)(bpp == 1 ? 0 : 128)) : null, OrderedDitherer.Bayer8x8);
            }

            SaveBitmapData(pixelFormat.ToString(), reference);

            // 1.) 1D array
            byte[] buf1D = new byte[reference.RowSize * reference.Height];
            using (IReadWriteBitmapData bmpData = BitmapDataFactory.CreateBitmapData(buf1D, reference.Size, reference.RowSize, pixelFormat, reference.Palette))
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
            using (IReadWriteBitmapData bmpData = BitmapDataFactory.CreateBitmapData((IntPtr)memory, reference.Size, reference.RowSize, pixelFormat, reference.Palette, null, DisposeUnmanaged))
            {
                reference.CopyTo(bmpData);
                AssertAreEqual(reference, bmpData);
            }
        }

        [TestCaseSource(nameof(CustomNonIndexedBitmapDataTestSource))]
        public void CustomNonIndexedBitmapDataTest(string testName, PixelFormatInfo pixelFormat, Func<ICustomBitmapDataRow, int, Color32> getColor, Action<ICustomBitmapDataRow, int, Color32> setColor)
        {
            Size size = new Size(128, 64);

#if NET35
            // Creating custom bitmap data with 3 different strides (in .NET 3.5 we cannot create managed bitmap data for these delegates because they are invariant)
            int stride = pixelFormat.GetByteWidth(size.Width);
            IntPtr buffer1 = Marshal.AllocHGlobal(stride * size.Height);
            using IReadWriteBitmapData bitmapDataNonDithered = BitmapDataFactory.CreateBitmapData(buffer1, size, stride, pixelFormat, getColor, setColor, default, 16, () => Marshal.FreeHGlobal(buffer1));
            stride = (stride + 3) / 4 * 4; // stride with 4 bytes boundary
            IntPtr buffer2 = Marshal.AllocHGlobal(stride * size.Height);
            using IReadWriteBitmapData bitmapDataDitheredContentIndependent = BitmapDataFactory.CreateBitmapData(buffer2, size, stride, pixelFormat, getColor, setColor, default, 16, () => Marshal.FreeHGlobal(buffer2));
            stride = (stride + 7) / 8 * 8; // stride with 8 bytes boundary
            IntPtr buffer3 = Marshal.AllocHGlobal(stride * size.Height);
            using IReadWriteBitmapData bitmapDataDitheredContentDependent = BitmapDataFactory.CreateBitmapData(buffer3, size, stride, pixelFormat, getColor, setColor, default, 16, () => Marshal.FreeHGlobal(buffer3));
#else
            // Creating custom bitmap data in 3 different forms (1D/2D arrays and unmanaged memory)
            int stride = pixelFormat.GetByteWidth(size.Width);
            stride = (stride + 7) / 8 * 8; // custom stride with 8 bytes boundary
            var buffer1D = new long[size.Height * stride / sizeof(long)];
            using IReadWriteBitmapData bitmapDataNonDithered = BitmapDataFactory.CreateBitmapData(buffer1D, size, stride, pixelFormat, getColor, setColor, default, 16);
            var buffer2D = new short[size.Height, stride / sizeof(short)];
            using IReadWriteBitmapData bitmapDataDitheredContentIndependent = BitmapDataFactory.CreateBitmapData(buffer2D, size.Width, pixelFormat, getColor, setColor, default, 16);
            IntPtr bufferUnmanaged = Marshal.AllocHGlobal(stride * size.Height);
            using IReadWriteBitmapData bitmapDataDitheredContentDependent = BitmapDataFactory.CreateBitmapData(bufferUnmanaged, size, stride, pixelFormat, getColor, setColor, default, 16, () => Marshal.FreeHGlobal(bufferUnmanaged));
#endif

            DoCommonCustomBitmapDataTests(testName, size, bitmapDataNonDithered, bitmapDataDitheredContentIndependent, bitmapDataDitheredContentDependent);
        }

        [TestCaseSource(nameof(CustomIndexedBitmapDataTestSource))]
        public void CustomIndexedBitmapDataTest(string testName, PixelFormatInfo pixelFormat, Func<ICustomBitmapDataRow, int, int> getColorIndex, Action<ICustomBitmapDataRow, int, int> setColorIndex, Palette palette)
        {
            Size size = new Size(128, 64);

#if NET35
            // Creating custom bitmap data with 3 different strides (in .NET 3.5 we cannot create managed bitmap data for these delegates because they are invariant)
            int stride = pixelFormat.GetByteWidth(size.Width);
            IntPtr buffer1 = Marshal.AllocHGlobal(stride * size.Height);
            using IReadWriteBitmapData bitmapDataNonDithered = BitmapDataFactory.CreateBitmapData(buffer1, size, stride, pixelFormat, getColorIndex, setColorIndex, palette, null, () => Marshal.FreeHGlobal(buffer1));
            stride = (stride + 3) / 4 * 4; // stride with 4 bytes boundary
            IntPtr buffer2 = Marshal.AllocHGlobal(stride * size.Height);
            using IReadWriteBitmapData bitmapDataDitheredContentIndependent = BitmapDataFactory.CreateBitmapData(buffer2, size, stride, pixelFormat, getColorIndex, setColorIndex, palette, null, () => Marshal.FreeHGlobal(buffer2));
            stride = (stride + 7) / 8 * 8; // stride with 8 bytes boundary
            IntPtr buffer3 = Marshal.AllocHGlobal(stride * size.Height);
            using IReadWriteBitmapData bitmapDataDitheredContentDependent = BitmapDataFactory.CreateBitmapData(buffer3, size, stride, pixelFormat, getColorIndex, setColorIndex, palette, null, () => Marshal.FreeHGlobal(buffer3));
#else
            // Creating custom bitmap data in 3 different forms (1D/2D arrays and unmanaged memory)
            int stride = pixelFormat.GetByteWidth(size.Width);
            stride = (stride + 7) / 8 * 8; // custom stride with 8 bytes boundary
            var buffer1D = new long[size.Height * stride / sizeof(long)];
            using IReadWriteBitmapData bitmapDataNonDithered = BitmapDataFactory.CreateBitmapData(buffer1D, size, stride, pixelFormat, getColorIndex, setColorIndex, palette);
            var buffer2D = new short[size.Height, stride / sizeof(short)];
            using IReadWriteBitmapData bitmapDataDitheredContentIndependent = BitmapDataFactory.CreateBitmapData(buffer2D, size.Width, pixelFormat, getColorIndex, setColorIndex, palette);
            IntPtr bufferUnmanaged = Marshal.AllocHGlobal(stride * size.Height);
            using IReadWriteBitmapData bitmapDataDitheredContentDependent = BitmapDataFactory.CreateBitmapData(bufferUnmanaged, size, stride, pixelFormat, getColorIndex, setColorIndex, palette, null, () => Marshal.FreeHGlobal(bufferUnmanaged));
#endif

            // Common custom bitmap data tests
            DoCommonCustomBitmapDataTests(testName, size, bitmapDataNonDithered, bitmapDataDitheredContentIndependent, bitmapDataDitheredContentDependent);

            // Indexed specific tests
            int bpp = pixelFormat.BitsPerPixel;
            int maxColors = 1 << bpp;
            foreach (var getQuantizer in new Func<int, Color, byte, OptimizedPaletteQuantizer>[] { OptimizedPaletteQuantizer.Octree, OptimizedPaletteQuantizer.MedianCut, OptimizedPaletteQuantizer.Wu })
            {
                using IReadableBitmapData optimizedReferenceBitmapData = GetInfoIcon256()
                    .Clone(bpp <= 8 ? KnownPixelFormat.Format8bppIndexed : KnownPixelFormat.Format32bppArgb, getQuantizer.Invoke(maxColors, Color.Silver, (byte)(bpp == 1 ? 0 : 128)), OrderedDitherer.Bayer8x8);
                size = optimizedReferenceBitmapData.Size;
                stride = pixelFormat.GetByteWidth(size.Width);

                // Creating custom bitmap data with optimized palette
                palette = optimizedReferenceBitmapData.Palette ?? new Palette(optimizedReferenceBitmapData.GetColors());
#if NET35
                IntPtr bufferOptimized = Marshal.AllocHGlobal(stride * size.Height);
                using IReadWriteBitmapData bitmapDataOptimizedPalette = BitmapDataFactory.CreateBitmapData(bufferOptimized, size, stride, pixelFormat, getColorIndex, setColorIndex, palette, null, () => Marshal.FreeHGlobal(bufferOptimized));
#else
                using IReadWriteBitmapData bitmapDataOptimizedPalette = BitmapDataFactory.CreateBitmapData(new byte[stride * size.Height], size, stride, pixelFormat, getColorIndex, setColorIndex, palette);
#endif
                Assert.GreaterOrEqual(maxColors, bitmapDataOptimizedPalette.Palette!.Count);
                optimizedReferenceBitmapData.CopyTo(bitmapDataOptimizedPalette);
                SaveBitmapData($"{testName} Optimized palette {getQuantizer.Method.Name}", bitmapDataOptimizedPalette);
                AssertAreEqual(optimizedReferenceBitmapData, bitmapDataOptimizedPalette, true);
            }
        }

        #endregion

        #endregion
    }
}
