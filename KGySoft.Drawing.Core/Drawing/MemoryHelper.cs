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
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
#if NETCOREAPP2_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER
using System.Runtime.InteropServices;
#endif
using System.Security;

using KGySoft.Drawing.Imaging;

#endregion

namespace KGySoft.Drawing
{
    internal static class MemoryHelper
    {
        #region Fields

        private static readonly uint pointerSizeMask = (uint)IntPtr.Size - 1u;

        #endregion

        #region Methods

        #region Internal Methods

        [SecurityCritical]
        internal static unsafe void CopyMemory(IntPtr source, IntPtr target, int length)
            => CopyMemory((byte*)source, (byte*)target, length);

        [SecurityCritical]
        internal static unsafe void CopyMemory(byte* source, byte* target, int length)
        {
#if NET35 || NET40 || NET45
            DoCopyMemory(source, target, length);
#else
            Buffer.MemoryCopy(source, target, length, length);
#endif
        }

        [SecurityCritical]
        internal static unsafe bool CompareMemory(IntPtr p1, IntPtr p2, int length)
            => CompareMemory((byte*)p1, (byte*)p2, length);

        [SecurityCritical]
        internal static unsafe bool CompareMemory(byte* p1, byte* p2, int length)
            => DoCompareMemory(p1, p2, length);

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static void FillMemory<T>(ref byte start, int length, T value)
            where T : unmanaged
        {
#if NETCOREAPP2_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER
            MemoryMarshal.CreateSpan(ref start.As<byte, T>(), length).Fill(value);
#else
            unsafe
            {
                fixed (byte* pStart = &start)
                    FillMemory(pStart, (long)length, value);
            }
#endif
        }

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static unsafe void FillMemory<T>(ref byte start, long length, T value)
            where T : unmanaged
        {
#if NETCOREAPP2_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER
            Debug.Assert(IntPtr.Size == 8 || length <= Int32.MaxValue);
            while (length > Int32.MaxValue)
            {
                MemoryMarshal.CreateSpan(ref start.As<byte, T>(), Int32.MaxValue).Fill(value);
                start = Unsafe.AddByteOffset(ref start, (nint)Int32.MaxValue * sizeof(T));
                length -= Int32.MaxValue;
            }

            MemoryMarshal.CreateSpan(ref start.As<byte, T>(), (int)length).Fill(value);
#else
            fixed (byte* pStart = &start)
                FillMemory(pStart, length, value);
#endif
        }

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static unsafe void FillMemory<T>(void* start, int length, T value) where T : unmanaged
        {
#if NETCOREAPP2_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER
            new Span<T>(start, length).Fill(value);
#else
            FillMemory((byte*)start, (long)length, value);
#endif
        }

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static unsafe void FillMemory<T>(void* start, long length, T value) where T : unmanaged
        {
#if NETCOREAPP2_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER
            while (length > Int32.MaxValue)
            {
                new Span<T>(start, Int32.MaxValue).Fill(value);
                start = (byte*)start + (long)Int32.MaxValue * sizeof(T);
                length -= Int32.MaxValue;
            }

            new Span<T>(start, (int)length).Fill(value);
#else
            // For primitive types we have to ensure proper alignment. If not aligned, using a non-primitive substitute type of the same size.
            if (sizeof(T) > 1 && typeof(T).IsPrimitive)
            {
                switch (sizeof(T))
                {
                    case 2 when ((nint)start & 1) != 0:
                        DoFillMemory(start, length, value.As<T, Gray16>());
                        return;

                    case 4 when ((nint)start & 3) != 0:
                        DoFillMemory(start, length, value.As<T, Color32>());
                        return;

                    case 8 when IntPtr.Size == 8 && ((nint)start & 7) != 0 || IntPtr.Size == 4 && ((nint)start & 3) != 0:
                        DoFillMemory(start, length, value.As<T, Color64>());
                        return;
                }
            }

            DoFillMemory(start, length, value);
#endif
        }

        #endregion

        #region Private Methods

#if NET35 || NET40 || NET45
        [SecurityCritical]
        private static unsafe void DoCopyMemory(byte* src, byte* dst, int length)
        {
            // NOTE: Unrolling loops could provide a better performance, but as this is for older frameworks only, we don't optimize it heavily.
            // Alignment is maintained though, even if misalignment is not an issue (apart from performance) on targets supported by these old .NET Framework versions.

            // Trying to copy 8 bytes (qword) at a time if both pointers have the same alignment
            // In a 32-bit process 4-byte alignment is enough for qword copy
            if ((((nuint)src ^ (nuint)dst) & pointerSizeMask) == 0u)
            {
                // Advancing with bytes until both pointers become aligned
                while (((nuint)src & pointerSizeMask) != 0u && length > 0)
                {
                    *dst = *src;
                    dst += 1;
                    src += 1;
                    length -= 1;
                }

                long* qwDst = (long*)dst;
                long* qwSrc = (long*)src;

                // copying qwords as long as possible
                for (int len = length >> 3, i = 0; i < len; i++)
                {
                    *qwDst = *qwSrc;
                    qwDst += 1;
                    qwSrc += 1;
                }

                if ((length & 7) == 0)
                    return;

                dst = (byte*)qwDst;
                src = (byte*)qwSrc;

                // copying last dword
                if ((length & 4) != 0)
                {
                    *(int*)dst = *(int*)src;
                    dst += 4;
                    src += 4;
                }

                // copying last word
                if ((length & 2) != 0)
                {
                    *(short*)dst = *(short*)src;
                    dst += 2;
                    src += 2;
                }

                // copying last byte
                if ((length & 1) != 0)
                    *dst = *src;

                return;
            }

            // 4-byte aligned copy (in a 32-bit process this part is redundant)
            if ((((nuint)src ^ (nuint)dst) & 3u) == 0u)
            {
                // Advancing with bytes until both pointers become aligned
                while (((nuint)src & 3u) != 0u && length > 0)
                {
                    *dst = *src;
                    dst += 1;
                    src += 1;
                    length -= 1;
                }

                int* dwDst = (int*)dst;
                int* dwSrc = (int*)src;

                // copying dwords as long as possible
                for (int len = length >> 2, i = 0; i < len; i++)
                {
                    *dwDst = *dwSrc;
                    dwDst += 1;
                    dwSrc += 1;
                }

                if ((length & 3) == 0)
                    return;

                dst = (byte*)dwDst;
                src = (byte*)dwSrc;

                // copying last word
                if ((length & 2) != 0)
                {
                    *(short*)dst = *(short*)src;
                    dst += 2;
                    src += 2;
                }

                // copying last byte
                if ((length & 1) != 0)
                    *dst = *src;

                return;
            }

            // 2-byte aligned copy
            if ((((nuint)src ^ (nuint)dst) & 1u) == 0u)
            {
                // Advancing one byte if both pointers are odd
                if (((nuint)src & 1u) != 0u && length > 0)
                {
                    *dst = *src;
                    dst += 1;
                    src += 1;
                    length -= 1;
                }

                short* wDst = (short*)dst;
                short* wSrc = (short*)src;

                // copying words as long as possible
                for (int len = length >> 1, i = 0; i < len; i++)
                {
                    *wDst = *wSrc;
                    wDst += 1;
                    wSrc += 1;
                }

                // copying last byte
                if ((length & 1) == 1)
                    *(byte*)wDst = *(byte*)wSrc;

                return;
            }

            // Fallback: byte-wise copy if alignment does not match
            for (int i = 0; i < length; i++)
            {
                *dst = *src;
                dst += 1;
                src += 1;
            }
        }
#endif

        [SecurityCritical]
        [SuppressMessage("Microsoft.Maintainability", "CA1502: Avoid excessive complexity",
            Justification = "Optimized for performance, long but very straightforward OR condition")]
        private static unsafe bool DoCompareMemory(byte* p1, byte* p2, int length)
        {
            // NOTE: we could use Vector<T> but this is actually faster and is available everywhere.
            // Alignment must be maintained to prevent DataMisalignedException on some platforms (e.g. ARM64).
            // Using the optimized unrolled loop for aligned memory blocks only.
            if (p1 == p2)
                return true;

            // Trying to copy 8 bytes (qword) at a time if both pointers have the same alignment
            // In a 32-bit process 4-byte alignment is enough for qword copy
            if ((((nuint)p1 ^ (nuint)p2) & pointerSizeMask) == 0u)
            {
                // Advancing with bytes until both pointers become aligned
                while (((nuint)p1 & pointerSizeMask) != 0u && length > 0)
                {
                    if (*p1 != *p2)
                        return false;
                    p1 += 1;
                    p2 += 1;
                    length -= 1;
                }

                long* qw1 = (long*)p1;
                long* qw2 = (long*)p2;
                int rest = length & 1023;
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

            // 4-byte aligned compare (in a 32-bit process this part is redundant)
            if ((((nuint)p1 ^ (nuint)p2) & 3u) == 0u)
            {
                // Advancing with bytes until both pointers become aligned
                while (((nuint)p1 & 3u) != 0u && length > 0)
                {
                    if (*p1 != *p2)
                        return false;
                    p1 += 1;
                    p2 += 1;
                    length -= 1;
                }

                int* dw1 = (int*)p1;
                int* dw2 = (int*)p2;

                // comparing as dwords
                for (int len = length >> 2, i = 0; i < len; i++)
                {
                    if (*dw1 != *dw2)
                        return false;
                    dw1 += 1;
                    dw2 += 1;
                }

                if ((length & 3) == 0)
                    return true;

                p1 = (byte*)dw1;
                p2 = (byte*)dw2;

                // comparing last word
                if ((length & 2) != 0)
                {
                    if (*(short*)p1 != *(short*)p2)
                        return false;
                    p1 += 2;
                    p2 += 2;
                }

                // comparing last byte
                if ((length & 1) != 0 && *p1 != *p2)
                    return false;

                // no difference
                return true;
            }

            // 2-byte aligned compare
            if ((((nuint)p1 ^ (nuint)p2) & 1u) == 0u)
            {
                // Advancing one byte if both pointers are odd
                if (((nuint)p1 & 1u) != 0u && length > 0)
                {
                    if (*p1 != *p2)
                        return false;
                    p1 += 1;
                    p2 += 1;
                    length -= 1;
                }

                short* w1 = (short*)p1;
                short* w2 = (short*)p2;

                // comparing as words
                for (int len = length >> 1, i = 0; i < len; i++)
                {
                    if (*w1 != *w2)
                        return false;
                    w1 += 1;
                    w2 += 1;
                }

                // comparing last byte
                if ((length & 1) == 1 && *(byte*)w1 != *(byte*)w2)
                    return false;

                // no difference
                return true;
            }

            // Fallback: byte-wise compare if alignment does not match
            for (int i = 0; i < length; i++)
            {
                if (*p1 != *p2)
                    return false;
                p1 += 1;
                p2 += 1;
            }

            return true;
        }

        [SecurityCritical]
        private static unsafe void DoFillMemory<T>(void* start, long count, T value)
            where T : unmanaged
        {
            // NOTE: No special handling for unaligned addresses because it would require creating a rotated version of 'value'.
            // Instead, the caller ensures non-primitive T replacements for unaligned addresses.
            Debug.Assert(!typeof(T).IsPrimitive || (nint)start % sizeof(T) == 0, $"Unaligned primitive type: {typeof(T).Name}");

            byte* pos = (byte*)start;

            // Trying to unroll the loop just a little bit (writing 16 bytes at once) for better performance.
            // As T can have any size, doing this only when size of T is 1, 2, 4 or 8 bytes.
            if (sizeof(T) is 1 or 2 or 4 or 8)
            {
                long vecCount = (count / (8 / sizeof(T))) >> 1; // number of 16-byte chunks
                if (vecCount > 0L)
                {
                    ulong valueUInt64 = sizeof(T) switch
                    {
                        1 => value.As<T, byte>() * 0x01_01_01_01_01_01_01_01UL, // repeating the byte value 8 times to fill all 8 bytes
                        2 => value.As<T, ushort>() * 0x0001_0001_0001_0001UL, // repeating the ushort value 4 times to fill all 8 bytes
                        4 => value.As<T, uint>() * 0x00000001_00000001UL, // repeating the uint value 2 times to fill all 8 bytes
                        _ => value.As<T, ulong>(),
                    };

                    // Filling 16 bytes at a time. If the address is unaligned, using the non-primitive Color64 type as a substitute to avoid
                    // possible alignment issues on some platforms (e.g. ARM64).
                    if (((nuint)start & pointerSizeMask) != 0u)
                    {
                        ref Color64 vec = ref valueUInt64.As<ulong, Color64>();
                        for (long i = 0; i < vecCount; i++)
                        {
                            *(Color64*)pos = vec;
                            *(Color64*)(pos + 8) = vec;
                            pos += 16;
                        }
                    }
                    else
                    {
                        for (long i = 0; i < vecCount; i++)
                        {
                            *(ulong*)pos = valueUInt64;
                            *(ulong*)(pos + 8) = valueUInt64;
                            pos += 16;
                        }
                    }

                    count -= vecCount * (16 / sizeof(T));
                }
            }

            // Filling remaining items
            T* pEnd = (T*)pos + count;
            for (T* p = (T*)pos; p < pEnd; p++)
                *p = value;
        }

        #endregion

        #endregion
    }
}