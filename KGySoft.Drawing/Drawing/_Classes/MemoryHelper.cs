#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: MemoryHelper.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2019 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution. If not, then this file is considered as
//  an illegal copy.
//
//  Unauthorized copying of this file, via any medium is strictly prohibited.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;
#if !NET35
using System.Security; 
#endif
#if WIN
using KGySoft.Drawing.WinApi; 
#endif

#endregion

namespace KGySoft.Drawing
{
    internal static class MemoryHelper
    {
        #region Methods

        #region Internal Methods

#if !NET35
        [SecuritySafeCritical]
#endif
        internal static unsafe void CopyMemory(IntPtr dest, IntPtr src, int length)
        {
#if WIN
            Kernel32.CopyMemory(dest, src, length);
#else
            Buffer.MemoryCopy(src.ToPointer(), dest.ToPointer(), length, length);
#endif
        }

#if !NET35
        [SecuritySafeCritical]
#endif
        internal static unsafe bool CompareMemory(IntPtr p1, IntPtr p2, int length)
        {
#if WIN
            return msvcrt.CompareMemory(p1, p2, length);
#else
            return CompareMemory((byte*)p1, (byte*)p2, length);
#endif
        }

        #endregion

        #region Private Methods
#if !WIN

        private static unsafe bool CompareMemory(byte* p1, byte* p2, int length)
        {
            // we could use Vector<T> but this is actually faster and is available everywhere
            if (p1 == p2)
                return true;

            long* qw1 = (long*)p1;
            long* qw2 = (long*)p2;
            int rest = length % 1024;
            long* end = (long*)(p1 + length - rest);

            // comparing 1024 byte chunks as qwords
            while (qw1 < end)
            {
                if (*qw1 != *qw2
                    || *(qw1 + 1) != *(qw2 + 1)
                    || *(qw1 + 2) != *(qw2 + 2)
                    || *(qw1 + 3) != *(qw2 + 3)
                    || *(qw1 + 4) != *(qw2 + 4)
                    || *(qw1 + 5) != *(qw2 + 5)
                    || *(qw1 + 6) != *(qw2 + 6)
                    || *(qw1 + 7) != *(qw2 + 7)
                    || *(qw1 + 8) != *(qw2 + 8)
                    || *(qw1 + 9) != *(qw2 + 9)
                    || *(qw1 + 10) != *(qw2 + 10)
                    || *(qw1 + 11) != *(qw2 + 11)
                    || *(qw1 + 12) != *(qw2 + 12)
                    || *(qw1 + 13) != *(qw2 + 13)
                    || *(qw1 + 14) != *(qw2 + 14)
                    || *(qw1 + 15) != *(qw2 + 15))
                {
                    return false;
                }

                qw1 += 16;
                qw2 += 16;
            }

            if (rest == 0)
                return true;

            p1 = (byte*)end;
            p2 = p2 + length - rest;

            // comparing last qwords (up to 15)
            for (int len = rest >> 3, i = 0; i < len; i++)
            {
                if (*(long*)p1 != *(long*)p2)
                    return false;
                p1 += 8;
                p2 += 8;
            }

            // comparing last dword
            if ((rest & 4) != 0)
            {
                if (*(int*)p1 != *(int*)p2)
                    return false;
                p1 += 4;
                p2 += 4;
            }

            // comparing last word
            if ((rest & 2) != 0)
            {
                if (*(short*)p1 != *(short*)p2)
                    return false;
                p1 += 2;
                p2 += 2;
            }

            // comparing last byte
            if ((rest & 1) != 0 && *p1 != *p2)
                return false;

            // no difference
            return true;
        }

#endif
        #endregion

        #endregion
    }
}