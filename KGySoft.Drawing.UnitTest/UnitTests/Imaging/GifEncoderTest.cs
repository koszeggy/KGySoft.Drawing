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

        [Test]
        public void SmokeTest()
        {
            using var stream = File.Create(Files.GetNextFileName(@"d:\temp\tmp\test.gif")!);
            var encoder = new GifEncoder(stream, new Size(16, 16))
            {
                RepeatCount = 0,
                BackColorIndex = 2,
                GlobalPalette = Palette.SystemDefault8BppPalette()
            };

            var frame = new Bitmap(13, 13);
            using (Graphics g = Graphics.FromImage(frame))
            {
                g.FillEllipse(Brushes.Cyan, 0, 0, 13, 13);
            }
            using (var imageData = frame.GetReadableBitmapData().Clone(PixelFormat.Format4bppIndexed, PredefinedColorsQuantizer.Grayscale16()))
                encoder.AddImage(imageData, new Point(1, 1), 100, GifGraphicDisposalMethod.RestoreToBackground);
            frame.Dispose();

            frame = new Bitmap(13, 13);
            using (Graphics g = Graphics.FromImage(frame))
            {
                g.FillEllipse(Brushes.Magenta, 0, 0, 13, 13);
            }
            using (var imageData = frame.GetReadableBitmapData())
                encoder.AddImage(imageData, new Point(2, 2), 100, GifGraphicDisposalMethod.RestoreToBackground);
            frame.Dispose();

            frame = new Bitmap(8, 8);
            using (Graphics g = Graphics.FromImage(frame))
            {
                g.DrawRectangle(Pens.Red, 1, 1, 6, 6);
            }
            using (var imageData = frame.GetReadableBitmapData())
                encoder.AddImage(imageData, new Point(5, 5), 100);
            frame.Dispose();

            stream.Flush();

        }

        [Test]
        public void LzwTest()
        {
            //using var bmp = new Bitmap(1, 1, PixelFormat.Format1bppIndexed);
            //using var bmpData = bmp.GetReadWriteBitmapData();
            //bmpData.Clear(new Color32(Color.White));

            using var bmp = GenerateAlphaGradientBitmap(new Size(256, 256));
            using var bmpData = bmp.GetReadWriteBitmapData().Clone(PixelFormat.Format1bppIndexed, OrderedDitherer.Bayer8x8);


            using (var stream = File.Create(Files.GetNextFileName(@"d:\temp\tmp\lzwTest.gif")!))
            {
                var encoder = new GifEncoder(stream, bmp.Size)
                {
                    //RepeatCount = 0,
                    //BackColorIndex = 2,
                    //GlobalPalette = Palette.SystemDefault8BppPalette()
                };

                encoder.AddImage(bmpData);
                stream.Flush();
            }
        }

        #endregion
    }
}
