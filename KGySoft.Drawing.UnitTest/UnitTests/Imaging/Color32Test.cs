using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using KGySoft.Diagnostics;
using KGySoft.Drawing.Imaging;
using NUnit.Framework;

namespace KGySoft.Drawing.UnitTests.Imaging
{
    [TestFixture]
    public class Color32Test
    {
        [TestCase(0x11223344U, 0x11, 0x22, 0x33, 0x44)]
        public void UnionTest(uint value, byte a, byte r, byte g, byte b)
        {
            var c = new Color32(value);
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
            var c = Color.FromArgb(0x11223344);
            Color32 c32 = c;

            Assert.AreEqual(c.ToArgb(), c32.ToArgb());

            c = c32;
            Assert.AreEqual(c32.ToArgb(), c.ToArgb());
        }

        [Test]
        public unsafe void SizeAndAlignmentTest()
        {
            Assert.AreEqual(4, sizeof(Color32));

            Color32* p = stackalloc Color32[2];
            Assert.AreEqual(4, (byte*)&p[1] - (byte*)&p[0]);
        }
    }
}
