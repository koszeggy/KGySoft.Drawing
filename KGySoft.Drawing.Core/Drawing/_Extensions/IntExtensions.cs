#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: IntExtensions.cs
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
using System.Collections.Specialized;
#if NETCOREAPP3_0_OR_GREATER
using System.Numerics;
#endif
using System.Runtime.CompilerServices;

#endregion

namespace KGySoft.Drawing
{
    /// <summary>
    /// In fact, Int32, UInt32, UInt64, BitVector32
    /// </summary>
    internal static class IntExtensions
    {
        #region Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static byte ClipToByte(this int value)
            => value < Byte.MinValue ? Byte.MinValue
                : value > Byte.MaxValue ? Byte.MaxValue
                : (byte)value;

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static ushort ClipToUInt16(this int value)
            => value < UInt16.MinValue ? UInt16.MinValue
                : value > UInt16.MaxValue ? UInt16.MaxValue
                : (ushort)value;

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static byte ClipToByte(this uint value)
            => value > Byte.MaxValue ? Byte.MaxValue : (byte)value;

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static int ToBitsPerPixel(this int colorCount)
        {
            if (colorCount == 1)
                return 1;

            // Bits per pixel is actually ceiling of log2(maxColors)
            // We could use BitOperations.Log2 but that returns the floor value so we should combine it with BitOperations.IsPow2,
            // which is available only starting with .NET 6 and in the end it would be slower for typical values not larger than 256.
            int bpp = 0;
            for (int n = colorCount - 1; n > 0; n >>= 1)
                bpp++;

            return bpp;
        }

        internal static int RoundUpToPowerOf2(this uint value)
        {
            // In .NET 6 and above there is a BitOperations.RoundUpToPowerOf2
            --value;
            value |= value >> 1;
            value |= value >> 2;
            value |= value >> 4;
            value |= value >> 8;
            value |= value >> 16;
            return (int)(value + 1);
        }

        internal static int GetMask(this BitVector32.Section section) => section.Mask << section.Offset;

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static int Abs(this int i)
        {
            // Math.Abs is still slower, even after the fix in https://github.com/dotnet/runtime/issues/24626
            Debug.Assert(i != Int32.MinValue);
            return i >= 0 ? i : -i;
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static long Abs(this long i)
        {
            Debug.Assert(i != Int64.MinValue);
            return i >= 0 ? i : -i;
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static int GetFlagsCount(this ulong value)
        {
#if NETCOREAPP3_0_OR_GREATER
            return BitOperations.PopCount(value);
#else
            // There are actually better general solutions than this but the callers cache the result.
            int result = 0;
            while (value != 0)
            {
                result++;
                value &= value - 1;
            }

            return result;
#endif
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static void SetBit(this ref byte bits, int x) => bits |= (byte)(128 >> (x & 7));

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static void SetBitRange(this ref byte bits, int startIndex, int endIndex)
        {
            Debug.Assert(endIndex >= startIndex && startIndex >> 3 == endIndex >> 3);
            bits |= (byte)((Byte.MaxValue << (8 - (endIndex - startIndex + 1)) & Byte.MaxValue) >> (startIndex & 7));
        }

        #endregion
    }
}