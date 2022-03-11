#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: Color64Test.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2021 - All Rights Reserved
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
    public class Color64Test
    {
        #region Methods

        [TestCase(0x1111222233334444U, (ushort)0x1111, (ushort)0x2222, (ushort)0x3333, (ushort)0x4444)]
        public void UnionTest(ulong value, ushort a, ushort r, ushort g, ushort b)
        {
            Color64 c = new Color64(value);
            Assert.AreEqual(a, c.A);
            Assert.AreEqual(r, c.R);
            Assert.AreEqual(g, c.G);
            Assert.AreEqual(b, c.B);

            c = new Color64(a, r, g, b);
            Assert.AreEqual(value, (ulong)c.ToArgb());
        }

        [Test]
        public void ConversionTest()
        {
            Color32 c = Color32.FromArgb(0x11223344);

            Color64 c64 = new Color64(c);
            Assert.AreEqual(c, c64.ToColor32());
        }

        [Test]
        public unsafe void SizeAndAlignmentTest()
        {
            Assert.AreEqual(8, sizeof(Color64));

            Color64* p = stackalloc Color64[2];
            Assert.AreEqual(8, (byte*)&p[1] - (byte*)&p[0]);
        }

        #endregion
    }
}