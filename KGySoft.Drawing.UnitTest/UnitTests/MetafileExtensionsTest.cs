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

        [TestCase(false, false)]
        [TestCase(false, true)]
        [TestCase(true, false)]
        [TestCase(true, true)]
        public void ToBitmapTest(bool antiAlias, bool keepAspectRatio)
        {
            using var metafile = GenerateMetafile();
            var size = new Size(512, 256);

            using (var bmp = metafile.ToBitmap(size, antiAlias, keepAspectRatio))
            {
                Assert.AreEqual(size, bmp.Size);
                SaveImage($"AntiAlias={antiAlias}, KeepRatio={keepAspectRatio}", bmp);
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
    }
}
