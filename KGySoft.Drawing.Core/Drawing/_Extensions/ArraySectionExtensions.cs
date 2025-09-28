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
using System.Drawing;
#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
using System.Numerics;
#endif
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
#if NETCOREAPP3_0_OR_GREATER
        nonAccelerated:
#endif
            byte[] array = buffer.UnderlyingArray!;
            pos += buffer.Offset;
            int end = pos + count;
            for (int i = pos; i < end; i++)
            {
                ref byte itemRef = ref array[i];
                itemRef = (itemRef + value).ClipToByte();
            }
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        [SuppressMessage("ReSharper", "TooWideLocalVariableScope", Justification = "Not on all platform targets")]
        internal static void AddOffset(this ArraySection<PointF> buffer, float offset)
        {
            int pos = 0;
            int count = buffer.Length;

#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
            int vectorCount;

#if NETCOREAPP3_0_OR_GREATER
            if (Sse.IsSupported)
            {
#if NET8_0_OR_GREATER
                if (Avx512F.IsSupported)
                {
                    vectorCount = count >> 3; // count / Vector512<"PointF">.Count; (8)
                    if (vectorCount > 0)
                    {
                        Vector512<float> vOffset = Vector512.Create(offset);
                        for (int i = 0; i < vectorCount; i++)
                        {
                            ref float itemRef = ref Unsafe.As<PointF, float>(ref buffer.GetElementReferenceUnchecked(pos));
                            Avx512F.Add(Vector512.LoadUnsafe(ref itemRef), vOffset).StoreUnsafe(ref itemRef);
                        }

                        pos += vectorCount << 3;
                        count -= vectorCount << 3;
                    }
                }
#endif
#if NETCOREAPP3_0_OR_GREATER
                if (Avx.IsSupported)
                {
                    vectorCount = count >> 2; // count / Vector256<"PointF">.Count; (4)
                    if (vectorCount > 0)
                    {
                        Vector256<float> vOffset = Vector256.Create(offset);
                        for (int i = 0; i < vectorCount; i++)
                        {
                            // Load/StoreUnsafe are available in .NET 7+ only, so using Read/WriteUnaligned here
                            ref byte itemRef = ref Unsafe.As<PointF, byte>(ref buffer.GetElementReferenceUnchecked(pos));
                            Unsafe.WriteUnaligned(ref itemRef, Avx.Add(Unsafe.ReadUnaligned<Vector256<float>>(ref itemRef), vOffset));
                        }

                        pos += vectorCount << 2;
                        count -= vectorCount << 2;
                    }
                }

                vectorCount = count >> 1; // count / Vector128<"PointF">.Count; (2)
                if (vectorCount > 0)
                {
                    Vector128<float> vOffset = Vector128.Create(offset);
                    for (int i = 0; i < vectorCount; i++)
                    {
                        // Load/StoreUnsafe are available in .NET 7+ only, so using Read/WriteUnaligned here
                        ref byte itemRef = ref Unsafe.As<PointF, byte>(ref buffer.GetElementReferenceUnchecked(pos));
                        Unsafe.WriteUnaligned(ref itemRef, Sse.Add(Unsafe.ReadUnaligned<Vector128<float>>(ref itemRef), vOffset));
                    }

                    pos += vectorCount << 1;
                    count -= vectorCount << 1;
                }
#endif
            }
            else
#endif
            {
                // Here we are in .NET Framework/Standard or with no SSE support: using Vector4 without checking if it is available
                vectorCount = count >> 1; // 2 PointF instances can fit in a Vector4
                if (vectorCount > 0)
                {
                    Vector4 vOffset = new Vector4(offset);
                    CastArray<PointF, Vector4> bufV4 = buffer.Slice(pos).Cast<PointF, Vector4>();
                    for (int i = 0; i < vectorCount; i++)
                    {
                        ref Vector4 itemRef = ref bufV4.GetElementReferenceUnsafe(i);
                        itemRef = Vector4.Add(itemRef, vOffset);
                    }

                    pos += vectorCount << 1;
#if DEBUG
                    count -= vectorCount << 1;
#endif
                }
            }

            // last point: as Vector2
            Debug.Assert(count == 1);
            {
                ref Vector2 itemRef = ref buffer.GetElementReferenceUnchecked(pos).AsVector2();
                itemRef = Vector2.Add(itemRef, new Vector2(offset));
            }
#else
            PointF[] array = buffer.UnderlyingArray!;
            pos += buffer.Offset;
            int end = pos + count;
            for (int i = pos; i < end; i++)
            {
                ref PointF itemRef = ref array[i];
                itemRef.X += offset;
                itemRef.Y += offset;
            }
#endif
        }

        #endregion
    }
}