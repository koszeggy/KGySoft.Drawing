#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: Color32Test.cs
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

using System.Drawing;

using KGySoft.Drawing.Imaging;

using NUnit.Framework;

#endregion

namespace KGySoft.Drawing.UnitTests.Imaging
{
    [TestFixture]
    public class Color32Test
    {
        #region Methods

        [TestCase(0x11223344U, 0x11, 0x22, 0x33, 0x44)]
        public void UnionTest(uint value, byte a, byte r, byte g, byte b)
        {
            Color32 c = new Color32(value);
            Assert.AreEqual(a, c.A);
            Assert.AreEqual(r, c.R);
            Assert.AreEqual(g, c.G);
            Assert.AreEqual(b, c.B);

            c = new Color32(a, r, g, b);
            Assert.AreEqual(value, (uint)c.ToArgb());
        }

        [Test]
        public void ConversionTest()
        {
            Color c = Color.FromArgb(0x11223344);

            Color32 c32 = new Color32(c);
            Assert.AreEqual(c.ToArgb(), c32.ToArgb());

            c = c32.ToColor();
            Assert.AreEqual(c32.ToArgb(), c.ToArgb());
        }

        [Test]
        public unsafe void SizeAndAlignmentTest()
        {
            Assert.AreEqual(4, sizeof(Color32));

            Color32* p = stackalloc Color32[2];
            Assert.AreEqual(4, (byte*)&p[1] - (byte*)&p[0]);
        }

        #endregion
    }
}