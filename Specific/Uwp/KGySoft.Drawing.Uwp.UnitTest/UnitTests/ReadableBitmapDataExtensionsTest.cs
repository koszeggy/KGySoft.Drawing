#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ReadableBitmapDataExtensionsTest.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2022 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#nullable enable

#region Usings

using System.Drawing;
using System.Threading.Tasks;

using Windows.UI.Xaml.Media.Imaging;

using KGySoft.Drawing.Imaging;

using NUnit.Framework;

#endregion

namespace KGySoft.Drawing.Uwp.UnitTest
{
    [TestFixture]
    public class ReadableBitmapDataExtensionsTest : TestBase
    {
        #region Methods

        [Test]
        public Task ToWriteableBitmapTest() => ExecuteTest(() =>
        {
            var size = new Size(32, 16);
            using IReadWriteBitmapData src = GenerateAlphaGradientBitmapData(size);
            var result = src.ToWriteableBitmap();

            Assert.AreEqual(size.Width, result.PixelWidth);
            Assert.AreEqual(size.Height, result.PixelHeight);
            using var resultData = result.GetReadableBitmapData();
            AssertAreEqual(src, resultData);
        });

        [Test]
        public Task ToWriteableBitmapAsyncTest() => ExecuteTestAsync(async () =>
        {
            var size = new Size(32, 16);
            using IReadWriteBitmapData src = GenerateAlphaGradientBitmapData(size);
            WriteableBitmap result = (await src.ToWriteableBitmapAsync())!;

            Assert.AreEqual(size.Width, result.PixelWidth);
            Assert.AreEqual(size.Height, result.PixelHeight);
            using var resultData = result.GetReadableBitmapData();
            AssertAreEqual(src, resultData);
        });

        #endregion
    }
}
