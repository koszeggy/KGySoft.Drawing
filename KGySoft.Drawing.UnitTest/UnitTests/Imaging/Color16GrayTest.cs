#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: Color16GrayTest.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2021 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Extern Aliases

extern alias core;

#endregion

#region Usings

#region Used Namespaces

using KGySoft.Drawing.Imaging;

using NUnit.Framework;

#endregion

#region Used Aliases

using ColorExt = core::KGySoft.Drawing.Imaging.ColorExtensions;

#endregion

#endregion

namespace KGySoft.Drawing.UnitTests.Imaging
{
    [TestFixture]
    public class Color16GrayTest
    {
        #region Methods

        [Test]
        public void ConversionTest()
        {
            Color32 c = Color32.FromArgb(0x11223344);

            Color16Gray c16 = new Color16Gray(c);
            Assert.AreEqual((ushort)(0x2222 * ColorExt.RLum + 0x3333 * ColorExt.GLum + 0x4444 * ColorExt.BLum), c16.Value);
        }

        [Test]
        public unsafe void SizeAndAlignmentTest()
        {
            Assert.AreEqual(2, sizeof(Color16Gray));

            Color16Gray* p = stackalloc Color16Gray[2];
            Assert.AreEqual(2, (byte*)&p[1] - (byte*)&p[0]);
        }

        #endregion
    }
}