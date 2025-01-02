#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: WriteableBitmapExtensionsTest.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2025 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;
using System.Threading.Tasks;

using KGySoft.Drawing.Imaging;

using NUnit.Framework;

using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;

#endregion

namespace KGySoft.Drawing.Uwp.UnitTest
{
    [TestFixture]
    public class WriteableBitmapExtensionsTest : TestBase
    {
        #region Fields

        private static readonly Color testColor = Color.FromArgb(0xFF, 0x80, 0xFF, 0x40);
        private static readonly Color testColorAlpha = Color.FromArgb(0x80, 0x80, 0xFF, 0x40);

        private static readonly object[][] setGetPixelTestSource =
        {
            new object[] { "Solid color", testColor, testColor, 0xFF_80_FF_40u },
            new object[] { "Alpha 50%", testColorAlpha, testColorAlpha, 0x80_40_80_20u },
            new object[] { "Transparent", Colors.Transparent, default(Color), 0u },
            new object[] { "Alpha 1", Color.FromArgb(1, 0, 0, 255), Color.FromArgb(1, 0, 0, 255), 0x01_00_00_01u },
            new object[] { "Alpha 256", Color.FromArgb(254, 0, 0, 255), Color.FromArgb(254, 0, 0, 255), 0xFE_00_00_FEu },
        };

        #endregion

        #region Methods

        [TestCaseSource(nameof(setGetPixelTestSource))]
        public Task SetGetPixelTest(string testName, Color testColor, Color expectedColor, uint expectedRawValue) => ExecuteTest(() =>
        {
            var bmp = new WriteableBitmap(1, 1);
            using (IReadWriteBitmapData bitmapData = bmp.GetReadWriteBitmapData())
            {
                IReadWriteBitmapDataRow row = bitmapData.FirstRow;
                row[0] = testColor.ToColor32();
                Color32 actualColor = row[0];
                Assert.IsTrue(expectedColor.ToColor32().TolerantEquals(actualColor, 1), $"Expected vs. read color: {expectedColor} <=> {actualColor}");
                uint actualRawValue = row.ReadRaw<uint>(0);
                Assert.AreEqual(expectedRawValue, actualRawValue, $"Raw value {expectedRawValue:X8} was expected but it was {actualRawValue:X8}");
            }
        });

        #endregion
    }
}