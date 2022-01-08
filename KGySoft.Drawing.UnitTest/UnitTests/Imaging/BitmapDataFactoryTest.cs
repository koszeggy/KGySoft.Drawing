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
using System.Runtime.InteropServices;
using KGySoft.Drawing.Imaging;

using NUnit.Framework;

#endregion

namespace KGySoft.Drawing.UnitTests.Imaging
{
    [TestFixture]
    internal class BitmapDataFactoryTest : TestBase
    {
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

            using (var image = reference.ToBitmap())
                SaveImage(pixelFormat.ToString(), image);

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

        [Test]
        public void CustomNonIndexedBitmapDataTest()
        {
            throw new NotImplementedException();
        }

        [Test]
        public void CustomIndexedBitmapDataTest()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
