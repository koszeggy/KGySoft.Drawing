#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: MemoryHelperTest.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2026 - All Rights Reserved
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

        [TestCase(0, 0)] // no misalignment
        [TestCase(4, 0)]
        [TestCase(4, 4)]
        [TestCase(2, 0)]
        [TestCase(2, 2)]
        [TestCase(1, 0)]
        [TestCase(1, 1)]
        public unsafe void CopyAndCompareTest(int offsetSrc, int offsetDst)
        {
            int testLength = 1024 + 8 + 4 + 2 + 1;
            var src = ThreadSafeRandom.Instance.NextBytes(testLength + offsetSrc);
            var dest = new byte[testLength + offsetDst];

            fixed (byte* pSrc = src)
            fixed (byte* pDest = dest)
            {
                MemoryHelper.CopyMemory(pSrc + offsetSrc, pDest + offsetDst, testLength);
                Assert.IsTrue(MemoryHelper.CompareMemory(new IntPtr(pSrc + offsetSrc), new IntPtr(pDest + offsetDst), testLength));
            }
        }

        [TestCase((byte)0x12)]
        [TestCase((ushort)0x1234)]
        [TestCase((uint)0x12345678)]
        [TestCase((ulong)0x01234567_89ABCDEF)]
        public unsafe void FillTest<T>(T value)
            where T : unmanaged, IEquatable<T>
        {
            int length = 1023;
            int count = length / sizeof(T);
            var buf = new byte[length];

            // aligned fill
            MemoryHelper.FillMemory(ref buf[0], count, value);
            var array = new T[count];
            Buffer.BlockCopy(buf, 0, array, 0, count * sizeof(T));
            for (int i = 0; i < count; i++)
                Assert.IsTrue(array[i].Equals(value));

            // unaligned fill
            count -= 1;
            MemoryHelper.FillMemory(ref buf[1], count, value);
            Buffer.BlockCopy(buf, 1, array, 0, count * sizeof(T));
            for (int i = 0; i < count; i++)
                Assert.IsTrue(array[i].Equals(value));
        }

        #endregion
    }
}