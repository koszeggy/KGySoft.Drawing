using System;
using System.Drawing;
using System.Drawing.Imaging;
using KGySoft.Drawing.Imaging;
using NUnit.Framework;

namespace KGySoft.Drawing.PerformanceTests
{
    [TestFixture]
    public class ImageExtensionsPerformanceTest
    {
        [TestCase("32bpp ARGB to 32bpp ARGB", PixelFormat.Format32bppArgb, PixelFormat.Format32bppArgb)]
        [TestCase("32bpp PARGB to 32bpp PARGB", PixelFormat.Format32bppPArgb, PixelFormat.Format32bppPArgb)]
        [TestCase("32bpp ARGB to 32bpp PARGB", PixelFormat.Format32bppArgb, PixelFormat.Format32bppPArgb)]
        [TestCase("32bpp PARGB to 32bpp ARGB", PixelFormat.Format32bppPArgb, PixelFormat.Format32bppArgb)]
        [TestCase("64bpp PARGB to 64bpp PARGB", PixelFormat.Format64bppPArgb, PixelFormat.Format64bppPArgb)]
        public void DrawIntoTest(string testName, PixelFormat formatSrc, PixelFormat formatDst)
        {
            Size targetSize = new Size(300, 300);
            Size sourceSize = new Size(250, 250);
            Point offset = new Point(targetSize - sourceSize);

            // creating source images: alpha rectangles
            using var bmpSrc1 = new Bitmap(sourceSize.Width, sourceSize.Height, formatSrc);
            bmpSrc1.Clear(Color.FromArgb(128, Color.Red));
            using var bmpSrc2 = new Bitmap(sourceSize.Width, sourceSize.Height, formatSrc);
            bmpSrc2.Clear(Color.FromArgb(128, Color.Lime));

            new ErrorTolerantPerformanceTest { TestName = testName, Iterations = 100, CpuAffinity = null }
                .AddCase(() =>
                {
                    using var bmpDst = new Bitmap(targetSize.Width, targetSize.Height, formatDst);
                    using (var g = Graphics.FromImage(bmpDst))
                    {
                        g.DrawImage(bmpSrc1, Point.Empty);
                        g.DrawImage(bmpSrc2, offset);
                    }
                }, "Graphics.DrawImage")
                .AddCase(() =>
                {
                    using var bmpDst = new Bitmap(targetSize.Width, targetSize.Height, formatDst);
                    bmpSrc1.DrawInto(bmpDst, Point.Empty);
                    bmpSrc2.DrawInto(bmpDst, offset);
                }, "ImageExtensions.DrawInto")
                .DoTest()
                .DumpResultsAndReturnValues(Console.Out);
        }
    }
}
