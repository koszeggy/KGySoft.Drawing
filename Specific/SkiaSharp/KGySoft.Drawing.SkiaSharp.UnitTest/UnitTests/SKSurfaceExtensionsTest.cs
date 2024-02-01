#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: SKSurfaceExtensionsTest.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2024 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System.Runtime.Versioning;

using NUnit.Framework;

using SkiaSharp;

#endregion

namespace KGySoft.Drawing.SkiaSharp.UnitTests
{
    [TestFixture]
    public class SKSurfaceExtensionsTest
    {
        #region Methods

        [Test]
        public void SetGetPixelTestCpu()
        {
            var testColor = SKColors.Blue.ToColor32();
            using var bitmap = new SKBitmap(10, 10);
            using var surface = SKSurface.Create(bitmap.PeekPixels());

            using (var bitmapData = surface.GetReadWriteBitmapData())
            {
                bitmapData.SetColor32(2, 3, testColor);
                Assert.AreEqual(testColor, bitmapData.GetColor32(2, 3));
            }

            using (var bitmapData = surface.GetReadableBitmapData())
            {
                Assert.AreEqual(testColor, bitmapData.GetColor32(2, 3));
            }
        }

#if WINDOWS
        [Test]
#if NET
        [SupportedOSPlatform("windows")]
#endif
        public void SetGetPixelTestGpu()
        {
            using var ctx = new WindowsOpenGLContext();
            ctx.MakeCurrent();

            using var grContext = GRContext.CreateGl();
            var info = new SKImageInfo(10, 10, SKColorType.Argb4444);
            using var surface = SKSurface.Create(grContext, true, info);
            Assert.IsNotNull(surface);
            Assert.AreEqual(GRBackend.OpenGL, surface.Context.Backend);

            var testColor = SKColors.Blue.ToColor32();
            using (var bitmapData = surface.GetReadWriteBitmapData())
            {
                bitmapData.SetColor32(2, 3, testColor);
                Assert.AreEqual(testColor, bitmapData.GetColor32(2, 3));
            }

            using (var bitmapData = surface.GetReadableBitmapData())
            {
                Assert.AreEqual(testColor, bitmapData.GetColor32(2, 3));
            }
        }
#endif


        #endregion
    }
}
