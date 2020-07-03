using System;
using System.Collections.Generic;
using System.Text;
using KGySoft.CoreLibraries;
using NUnit.Framework;

namespace KGySoft.Drawing.UnitTests
{
    [TestFixture]
    public class MemoryHelperTest
    {
        [Test]
        public unsafe void CopyAndCompareTest()
        {
            int testLength = 1024 + 8 + 4 + 2 + 1;
            var src = new Random().NextBytes(testLength);
            var dest = new byte[testLength];

            fixed (byte* pSrc = src)
            fixed (byte* pDest = dest)
            {
                MemoryHelper.CopyMemory(pSrc, pDest, testLength);
                Assert.IsTrue(MemoryHelper.CompareMemory(new IntPtr(pSrc), new IntPtr(pDest), testLength));
            }
        }
    }
}
