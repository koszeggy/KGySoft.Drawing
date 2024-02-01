#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: MemoryHelperTest.cs
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

using System;

using KGySoft.CoreLibraries;

using NUnit.Framework;

#endregion

namespace KGySoft.Drawing.UnitTests
{
    [TestFixture]
    public class MemoryHelperTest
    {
        #region Methods

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

        #endregion
    }
}