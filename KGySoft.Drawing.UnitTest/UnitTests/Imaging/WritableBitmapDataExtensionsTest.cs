#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: WritableBitmapDataExtensionsTest.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2020 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution. If not, then this file is considered as
//  an illegal copy.
//
//  Unauthorized copying of this file, via any medium is strictly prohibited.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System.Drawing;

using KGySoft.Drawing.Imaging;

using NUnit.Framework;

#endregion

namespace KGySoft.Drawing.UnitTests.Imaging
{
    [TestFixture]
    public class WritableBitmapDataExtensionsTest : TestBase
    {
        #region Methods

        [Test]
        public void DrawBitmapDataSameInstanceOverlappingTest()
        {
            using var bmp = Icons.Information.ExtractBitmap(new Size(256, 256));
            using (IReadWriteBitmapData bitmapData = bmp.GetReadWriteBitmapData())
            {
                Assert.DoesNotThrow(() => bitmapData.DrawBitmapData(bitmapData, new Point(64, 64)));
            }

            SaveImage("result", bmp);
        }

        [Test]
        public void DrawBitmapDataWithResizeSameInstanceOverlappingTest()
        {
            using var bmp = Icons.Information.ExtractBitmap(new Size(256, 256));
            using (IReadWriteBitmapData bitmapData = bmp.GetReadWriteBitmapData())
            {
                Assert.DoesNotThrow(() => bitmapData.DrawBitmapData(bitmapData, new Rectangle(64, 64, 64, 64)));
            }

            SaveImage("result", bmp);
        }

        #endregion
    }
}   