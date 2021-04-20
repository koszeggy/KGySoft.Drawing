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
using System.Diagnostics.CodeAnalysis;
using System.Security; 

using KGySoft.Drawing.WinApi; 

#endregion

namespace KGySoft.Drawing
{
    internal static class MemoryHelper
    {
        #region Methods

        #region Internal Methods

        [SecurityCritical]
        internal static unsafe void CopyMemory(IntPtr source, IntPtr target, int length)
            => CopyMemory((byte*)source, (byte*)target, length);

        [SecurityCritical]
        internal static unsafe void CopyMemory(byte* source, byte* target, int length)
        {
            if (OSUtils.IsWindows)
                Kernel32.CopyMemory(new IntPtr(target), new IntPtr(source), length);
            else
            {
#if NET35 || NET40 || NET45
                DoCopyMemory(source, target, length);
#else
                Buffer.MemoryCopy(source, target, length, length);
#endif
            }
        }

        [SecurityCritical]
        internal static unsafe bool CompareMemory(IntPtr p1, IntPtr p2, int length)
            => CompareMemory((byte*)p1, (byte*)p2, length);

        [SecurityCritical]
        internal static unsafe bool CompareMemory(byte* p1, byte* p2, int length)
            => OSUtils.IsWindows
                ? msvcrt.CompareMemory(new IntPtr(p1),  new IntPtr(p2), length)
                : DoCompareMemory(p1, p2, length);

        #endregion

        #region Private Methods

#if NET35 || NET40 || NET45
        [SecurityCritical]
        private static unsafe void DoCopyMemory(byte* src, byte* dest, int length)
        {
            long* qwDest = (long*)dest;
            long* qwSrc = (long*)src;

            // copying qwords until possible
            for (int len = length >> 3, i = 0; i < len; i++)
            {
                *qwDest = *qwSrc;
                qwDest += 1;
                qwSrc += 1;
            }

            if ((length & 7) == 0)
                return;

            dest = (byte*)qwDest;
            src = (byte*)qwSrc;

            // copying last dword
            if ((length & 4) != 0)
            {
                *(int*)dest = *(int*)src;
                dest += 4;
                src += 4;
            }

            // copying last word
            if ((length & 2) != 0)
            {
                *(short*)dest = *(short*)src;
                dest += 2;
                src += 2;
            }

            // copying last byte
            if ((length & 1) != 0)
                *dest = *src;
        } 
#endif

        [SecurityCritical]
        [SuppressMessage("Microsoft.Maintainability", "CA1502: Avoid excessive complexity",
            Justification = "Optimized for performance, long but very straightforward OR condition")]
        private static unsafe bool DoCompareMemory(byte* p1, byte* p2, int length)
        {
            // we could use Vector<T> but this is actually faster and is available everywhere
            if (p1 == p2)
                return true;

            long* qw1 = (long*)p1;
            long* qw2 = (long*)p2;
            int rest = length % 1024;
            long* end = (long*)(p1 + length - rest);

            // comparing 128 byte chunks as qwords
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

            // comparing last qwords
            for (int len = rest >> 3, i = 0; i < len; i++)
            {
                if (*qw1 != *qw2)
                    return false;
                qw1 += 1;
                qw2 += 1;
            }

            if ((rest & 7) == 0)
                return true;

            p1 = (byte*)qw1;
            p2 = (byte*)qw2;

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

        #endregion

        #endregion
    }
}