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

#region Usings

using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

using KGySoft.CoreLibraries;
using KGySoft.Drawing.Imaging;

using NUnit.Framework;

#endregion

namespace KGySoft.Drawing.UnitTests.Imaging
{
    [TestFixture]
    public class GifEncoderTest : TestBase
    {
        #region Methods

        [Test, Explicit]
        public void AnimationTest()
        {
            using var stream = File.Create(Files.GetNextFileName(@"d:\temp\tmp\AnimationTest.gif")!);
            var encoder = new GifEncoder(stream, new Size(64, 64))
            {
                RepeatCount = 0,
                BackColorIndex = 2,
                GlobalPalette = Palette.SystemDefault4BppPalette()
            };

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

            stream.Flush();

        }

        [TestCase(GifCompressionMode.Auto)]
        [TestCase(GifCompressionMode.DoNotClear)]
        [TestCase(GifCompressionMode.DoNotIncreaseBitSize)]
        [TestCase(GifCompressionMode.Uncompressed)]
        public void LzwTest(GifCompressionMode compressionMode)
        {
            using Bitmap bmp = Icons.Information.ExtractBitmap(new Size(256, 256)); // GenerateAlphaGradientBitmap(new Size(256, 128));
            using IReadableBitmapData bmpData = bmp.GetReadableBitmapData();
            //int bpp = 8;
            for (int bpp = 1; bpp <= 8; bpp++)
            {
                using IReadWriteBitmapData quantized = bmpData.Clone(PixelFormat.Format8bppIndexed,
                    OptimizedPaletteQuantizer.Wu(1 << bpp, Color.Silver, (byte)(bpp == 1 ? 0 : 128)), ErrorDiffusionDitherer.FloydSteinberg);
                using var ms = new MemoryStream();
                new GifEncoder(ms, bmp.Size) { CompressionMode = compressionMode }
                    .AddImage(quantized);

                ms.Position = 0;
                using Bitmap gif = new Bitmap(ms);
                using IReadableBitmapData actual = gif.GetReadableBitmapData();
                SaveStream($"{bpp}bpp_{compressionMode}", ms, "gif");
                AssertAreEqual(quantized, actual);
            }
        }

        #endregion
    }
}
