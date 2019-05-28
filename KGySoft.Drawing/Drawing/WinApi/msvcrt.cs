#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: msvcrt.cs
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
using System.Runtime.InteropServices;
using System.Security;

#endregion

namespace KGySoft.Drawing.WinApi
{
    [SecurityCritical]
    internal static class msvcrt
    {
        #region NativeMethods class

        private static class NativeMethods
        {
            #region Methods

            /// <summary>
            /// Compares characters in two buffers.
            /// </summary>
            /// <param name="b1">First buffer.</param>
            /// <param name="b2">Second buffer.</param>
            /// <param name="count">Number of characters to compare. (Compares bytes for memcmp, wide characters for wmemcmp).</param>
            /// <returns>The return value indicates the relationship between the buffers.</returns>
            /// <remarks>Compares the first count characters of buf1 and buf2 and returns a value that indicates their relationship. The sign of a non-zero return value is the sign of the difference between the first differing pair of values in the buffers. The values are interpreted as unsigned char for memcmp, and as wchar_t for wmemcmp.</remarks>
            [DllImport("msvcrt.dll")]
            internal static extern int memcmp(IntPtr b1, IntPtr b2, long count);

            #endregion
        }

        #endregion

        #region Methods

        internal static bool CompareMemory(IntPtr b1, IntPtr b2, long count) => NativeMethods.memcmp(b1, b2, count) == 0;

        #endregion
    }
}
