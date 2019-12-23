#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: MetafileExtensionsTest.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2019 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution. If not, then this file is considered as
//  an illegal copy.
//
//  Unauthorized copying of this file, via any medium is strictly prohibited.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using NUnit.Framework;

#endregion

namespace KGySoft.Drawing.UnitTests
{
    [TestFixture]
    public class MetafileExtensionsTest : TestBase
    {
        #region Methods

        #region Static Methods

        private static Metafile GenerateMetafile()
        {
            //Set up reference Graphic
            Graphics refGraph = Graphics.FromHwnd(IntPtr.Zero);
            IntPtr hdc = refGraph.GetHdc();
            Metafile result = new Metafile(hdc, new Rectangle(0, 0, 100, 100), MetafileFrameUnit.Pixel, EmfType.EmfOnly, "Test");

            //Draw some silly drawing
            using (var g = Graphics.FromImage(result))
            {
                var r = new Rectangle(0, 0, 100, 100);
                var leftEye = new Rectangle(20, 20, 20, 30);
                var rightEye = new Rectangle(60, 20, 20, 30);
                g.FillEllipse(Brushes.Yellow, r);
                g.FillEllipse(Brushes.White, leftEye);
                g.FillEllipse(Brushes.White, rightEye);
                g.DrawEllipse(Pens.Black, leftEye);
                g.DrawEllipse(Pens.Black, rightEye);
                g.DrawBezier(Pens.Red, new Point(10, 50), new Point(10, 100), new Point(90, 100), new Point(90, 50));
            }

            refGraph.ReleaseHdc(hdc); //cleanup
            refGraph.Dispose();
            return result;
        }

        #endregion

        #region Instance Methods

        [Test]
        public void ToBitmapTest()
        {
            using var metafile = GenerateMetafile();
            var size = new Size(256, 256);

            using (var bmp = metafile.ToBitmap(size))
            {
                Assert.AreEqual(size, bmp.Size);
                SaveImage("NoAntiAlias", bmp);
            }

            using (var bmp = metafile.ToBitmap(size, true))
            {
                Assert.AreEqual(size, bmp.Size);
                SaveImage("AntiAlias", bmp);
            }
        }

        [Test]
        public void SaveTest()
        {
            using var metafile = GenerateMetafile();

            AssertPlatformDependent(() =>
            {
                using (var ms = new MemoryStream())
                {
                    metafile.Save(ms, false);
                    ms.Position = 0;
                    var clone = new Metafile(ms);
                    Assert.IsTrue(metafile.EqualsByContent(clone));
                    SaveImage("EMF", clone);
                }

                using (var ms = new MemoryStream())
                {
                    metafile.Save(ms, true);
                    ms.Position = 0;
                    var clone = Image.FromStream(ms);
                    Assert.IsTrue(metafile.EqualsByContent(clone));
                    SaveImage("WMF", clone);
                }
            }, PlatformID.Win32NT);
        }

        #endregion

        #endregion
    }
}
