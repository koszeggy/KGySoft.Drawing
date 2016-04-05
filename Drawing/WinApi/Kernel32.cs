using System;
using System.Runtime.InteropServices;

namespace KGySoft.Drawing.WinApi
{
    internal static class Kernel32
    {
        /// <summary>
        /// Copies a block of memory from one location to another.
        /// </summary>
        /// <param name="dest">A pointer to the starting address of the copied block's destination.</param>
        /// <param name="src">A pointer to the starting address of the block of memory to copy.</param>
        /// <param name="length">The size of the block of memory to copy, in bytes.</param>
        [DllImport("kernel32.dll")]
        public static extern void CopyMemory(IntPtr dest, IntPtr src, int length);
    }
}
