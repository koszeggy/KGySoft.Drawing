using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace KGySoft.Drawing.WinApi
{
    internal static class msvcrt
    {
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
    }
}
