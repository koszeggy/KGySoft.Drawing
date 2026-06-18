#if WINDOWS
#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: MetafileExtensionsTest.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2026 - All Rights Reserved
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

            using var bmp = metafile.ToBitmap(size, antiAlias, keepAspectRatio);
            Assert.AreEqual(size, bmp.Size);
            SaveImage($"AntiAlias={antiAlias}, KeepRatio={keepAspectRatio}", bmp);
        }

        [Test]
        public void SaveTest() => AssertPlatformDependent(() =>
        {
            using Metafile emf = GenerateMetafile();
            Metafile wmf;

            using (var ms = new MemoryStream())
            {
                emf.Save(ms, false);
                ms.Position = 0;
                using var clone = new Metafile(ms);
                Assert.IsTrue(emf.EqualsByContent(clone));
                SaveImage("Emf", clone);
            }

            using (var ms = new MemoryStream())
            {
                emf.SaveAsWmf(ms);
                ms.Position = 0;
                wmf = new Metafile(ms);
                Assert.IsTrue(emf.EqualsByContent(wmf));
                SaveImage("EmfAsWmf", wmf);
            }

            using (var ms = new MemoryStream())
            {
                wmf.Save(ms, false);
                ms.Position = 0;
                using var clone = new Metafile(ms);
                Assert.IsTrue(wmf.EqualsByContent(clone));
                SaveImage("Wmf", clone);
            }

            using (var ms = new MemoryStream())
            {
                wmf.SaveAsEmf(ms);
                ms.Position = 0;
                using var clone = new Metafile(ms);
                Assert.IsTrue(wmf.EqualsByContent(clone));
                SaveImage("WmfAsEmf", clone);
            }

            wmf.Dispose();
        }, PlatformID.Win32NT);

        [Test]
        public void SaveLargeEmfTest() => AssertPlatformDependent(() =>
        {
            using Metafile emf = GenerateMetafile(new Size(50_000, 50_000));
            using var ms = new MemoryStream();
            Assert.DoesNotThrow(() => emf.SaveAsEmf(ms));
            Assert.Throws<ArgumentException>(() => emf.SaveAsWmf(ms), DrawingRes.Gdi32GetWmfContentFailed);
        }, PlatformID.Win32NT);

        #endregion
    }
}

#endif