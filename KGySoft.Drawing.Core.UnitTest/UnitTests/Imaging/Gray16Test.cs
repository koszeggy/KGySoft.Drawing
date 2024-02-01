#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: Gray16Test.cs
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

using KGySoft.Drawing.Imaging;

using NUnit.Framework;

#endregion

namespace KGySoft.Drawing.UnitTests.Imaging
{
    [TestFixture]
    public class Gray16Test
    {
        #region Methods

        [Test]
        public void ConversionTest()
        {
            Color32 c = Color32.FromArgb(unchecked((int)0xFF112233));

            Gray16 c16 = new Gray16(c.ToColor64());
            Assert.AreEqual(new Color64(c).GetBrightness(), c16.Value);
        }

        [Test]
        public unsafe void SizeAndAlignmentTest()
        {
            Assert.AreEqual(2, sizeof(Gray16));

            Gray16* p = stackalloc Gray16[2];
            Assert.AreEqual(2, (byte*)&p[1] - (byte*)&p[0]);
        }

        #endregion
    }
}