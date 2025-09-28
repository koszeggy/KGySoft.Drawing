#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ArraySectionExtensions.cs
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
#if NETCOREAPP3_0_OR_GREATER
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
#endif

using KGySoft.Collections;

#endregion

namespace KGySoft.Drawing
{
    internal static class ArraySectionExtensions
    {
        #region Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static unsafe CastArray<byte, T> Allocate<T>(this ref ArraySection<byte> buffer, int elementCount)
            where T : unmanaged
        {
            ArraySection<byte> result = buffer.Slice(0, elementCount * sizeof(T));
            buffer = buffer.Slice(result.Length);
            return result.Cast<byte, T>();
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static void SetBitRange(this ref ArraySection<byte> buffer, int startIndex, int endIndex)
        {
            Debug.Assert(endIndex >= startIndex && startIndex >= 0 && (endIndex >> 3) < buffer.Length);

            int maskPos = startIndex >> 3;
            int endMaskPos = endIndex >> 3;

            // up to 8 pixels on the same byte
            if (maskPos == endMaskPos)
            {
                buffer.GetElementReferenceUnchecked(maskPos).SetBitRange(startIndex, endIndex);
                return;
            }

            // first partial byte
            if ((startIndex & 7) != 0)
            {
                buffer.GetElementReferenceUnchecked(maskPos).SetBitRange(startIndex, ((maskPos + 1) << 3) - 1);
                maskPos += 1;
            }

            // whole bytes
            switch (endMaskPos - maskPos)
            {
                case > 1:
                    buffer.Slice(maskPos, endMaskPos - maskPos).Fill(Byte.MaxValue);
                    break;
                case 1:
                    buffer.SetElementUnchecked(maskPos, Byte.MaxValue);
                    break;
            }

            // last [partial] byte
            if ((endIndex & 7) != 7)
                buffer.GetElementReferenceUnchecked(endMaskPos).SetBitRange(endMaskPos << 3, endIndex);
            else
                buffer.SetElementUnchecked(endMaskPos, Byte.MaxValue);
        }

        // NOTE: the uint type for value is important, because byte + byte would use signed int, ending up in a less performant ClipToByte overload
        [MethodImpl(MethodImpl.AggressiveInlining)]
        [SuppressMessage("ReSharper", "TooWideLocalVariableScope", Justification = "Not on all platform targets")]
        internal static void AddByteSat(this ArraySection<byte> buffer, uint value)
        {
            Debug.Assert(endIndex >= startIndex && startIndex >= 0 && endIndex < buffer.Length && value <= Byte.MaxValue);
            Debug.Assert(value < Byte.MaxValue, "If value is 255, add a branch using buffer.Fill");
            int pos = 0;
            int count = buffer.Length;

#if NETCOREAPP3_0_OR_GREATER
#if NET10_0_OR_GREATER
            if (count < Vector64<byte>.Count)
                goto nonAccelerated;
#endif

            int vectorCount;

            // SSE2+ specific vectorization for vector sizes of 16, 32 and 64 bytes
            if (Sse2.IsSupported)
            {
#if !NET10_0_OR_GREATER
                if (count < Vector128<byte>.Count)
                    goto nonAccelerated;
#endif
#if NET8_0_OR_GREATER
                if (Avx512BW.IsSupported)
                {
                    vectorCount = count >> 6; // count / Vector512<byte>.Count; (64)
                    if (vectorCount > 0)
                    {
                        Vector512<byte> vValue = Vector512.Create((byte)value);
                        for (int i = 0; i < vectorCount; i++)
                        {
                            ref byte itemRef = ref buffer.GetElementReferenceUnchecked(pos);
                            Avx512BW.AddSaturate(Vector512.LoadUnsafe(ref itemRef), vValue).StoreUnsafe(ref itemRef);
                        }

                        pos += vectorCount << 6;
                        count -= vectorCount << 6;
                    }
                }
#endif
#if NETCOREAPP3_0_OR_GREATER
                if (Avx2.IsSupported)
                {
                    vectorCount = count >> 5; // count / Vector256<byte>.Count; (32)
                    if (vectorCount > 0)
                    {
                        Vector256<byte> vValue = Vector256.Create((byte)value);
                        for (int i = 0; i < vectorCount; i++)
                        {
                            // Load/StoreUnsafe are available in .NET 7+ only, so using Read/WriteUnaligned here
                            ref byte itemRef = ref buffer.GetElementReferenceUnchecked(pos);
                            Unsafe.WriteUnaligned(ref itemRef, Avx2.AddSaturate(Unsafe.ReadUnaligned<Vector256<byte>>(ref itemRef), vValue));
                        }

                        pos += vectorCount << 5;
                        count -= vectorCount << 5;
                    }
                }

                vectorCount = count >> 4; // count / Vector128<byte>.Count; (16)
                if (vectorCount > 0)
                {
                    Vector128<byte> vValue = Vector128.Create((byte)value);
                    for (int i = 0; i < vectorCount; i++)
                    {
                        // Load/StoreUnsafe are available in .NET 7+ only, so using Read/WriteUnaligned here
                        ref byte itemRef = ref buffer.GetElementReferenceUnchecked(pos);
                        Unsafe.WriteUnaligned(ref itemRef, Sse2.AddSaturate(Unsafe.ReadUnaligned<Vector128<byte>>(ref itemRef), vValue));
                    }

                    pos += vectorCount << 4;
#if NET10_0_OR_GREATER
                    count -= vectorCount << 4;
#endif
                }
#endif
            }
#endif
#if NET10_0_OR_GREATER
            // if SSE2 is not supported, trying generic Vector*.AddSaturate, which is available for .NET 10+ only
            else
            {
                if (Vector512.IsHardwareAccelerated)
                {
                    vectorCount = count >> 6; // count / Vector512<byte>.Count; (64)
                    if (vectorCount > 0)
                    {
                        Vector512<byte> vValue = Vector512.Create((byte)value);
                        for (int i = 0; i < vectorCount; i++)
                        {
                            ref byte itemRef = ref buffer.GetElementReferenceUnchecked(pos);
                            Vector512.AddSaturate(Vector512.LoadUnsafe(ref itemRef), vValue).StoreUnsafe(ref itemRef);
                        }

                        pos += vectorCount << 6;
                        count -= vectorCount << 6;
                    }
                }

                if (Vector256.IsHardwareAccelerated)
                {
                    vectorCount = count >> 5; // count / Vector256<byte>.Count; (32)
                    if (vectorCount > 0)
                    {
                        Vector256<byte> vValue = Vector256.Create((byte)value);
                        for (int i = 0; i < vectorCount; i++)
                        {
                            ref byte itemRef = ref buffer.GetElementReferenceUnchecked(pos);
                            Vector256.AddSaturate(Vector256.LoadUnsafe(ref itemRef), vValue).StoreUnsafe(ref itemRef);
                        }

                        pos += vectorCount << 5;
                        count -= vectorCount << 5;
                    }
                }

                if (Vector128.IsHardwareAccelerated)
                {
                    vectorCount = count >> 4; // count / Vector128<byte>.Count; (16)
                    if (vectorCount > 0)
                    {
                        Vector128<byte> vValue = Vector128.Create((byte)value);
                        for (int i = 0; i < vectorCount; i++)
                        {
                            ref byte itemRef = ref buffer.GetElementReferenceUnchecked(pos);
                            Vector128.AddSaturate(Vector128.LoadUnsafe(ref itemRef), vValue).StoreUnsafe(ref itemRef);
                        }

                        pos += vectorCount << 4;
                        count -= vectorCount << 4;
                    }
                }
            }

            // Note that the Vector64 block is intentionally outside the else block of SSE2 because SSE does not have 64-bit vector operations.
            if (Vector64.IsHardwareAccelerated)
            {
                vectorCount = count >> 3; // count / Vector64<byte>.Count; (8)
                if (vectorCount > 0)
                {
                    Vector64<byte> vValue = Vector64.Create((byte)value);
                    for (int i = 0; i < vectorCount; i++)
                    {
                        ref byte itemRef = ref buffer.GetElementReferenceUnchecked(pos);
                        Vector64.AddSaturate(Vector64.LoadUnsafe(ref itemRef), vValue).StoreUnsafe(ref itemRef);
                    }

                    pos += vectorCount << 3;
                }
            }
#endif

        // fallback, or remaining bytes
        nonAccelerated:
            byte[] array = buffer.UnderlyingArray!;
            pos += buffer.Offset;
            int end = pos + count;
            for (int i = pos; i < end; i++)
            {
                ref byte itemRef = ref array[i];
                itemRef = (itemRef + value).ClipToByte();
            }
        }

        #endregion
    }
}