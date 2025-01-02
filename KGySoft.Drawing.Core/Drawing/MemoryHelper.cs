#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: MemoryHelper.cs
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

using System;
using System.Security;

#endregion

namespace KGySoft.Drawing
{
    internal static class MemoryHelper
    {
        #region Methods

        #region Internal Methods

        [SecurityCritical]
        internal static unsafe void CopyMemory(byte* source, byte* target, int length)
        {
#if NET35 || NET40 || NET45
            DoCopyMemory(source, target, length);
#else
            Buffer.MemoryCopy(source, target, length, length);
#endif
        }

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

        #endregion

        #endregion
    }
}