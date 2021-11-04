﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: GifEncoder.LzwEncoder.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2021 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;
#if NETCOREAPP2_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
using System.Buffers;
#endif
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security;

using KGySoft.Collections;

#endregion

namespace KGySoft.Drawing.Imaging
{
    public partial class GifEncoder
    {
        /// <summary>
        /// The LZW Encoder based on the specification as per chapter 22 and Appendix F in https://www.w3.org/Graphics/GIF/spec-gif89a.txt
        /// The detailed LZW algorithm is written here: http://giflib.sourceforge.net/whatsinagif/lzw_image_data.html
        /// </summary>
        // Note: ref struct because it contains a stack-only fixed buffer
        [SecuritySafeCritical]
        private ref struct LzwEncoder
        {
            #region Nested structs

            #region IndexBuffer struct

            /// <summary>
            /// Represents a span of a byte array and has specialized GetHashCode/Equals.
            /// It could also contain a single <see cref="ArraySegment{T}"/> or <see cref="ArraySection{T}"/> field
            /// but it is faster if we can mutate only the length directly.
            /// </summary>
            private struct IndexBuffer : IEquatable<IndexBuffer>
            {
                #region Fields

                internal readonly byte[] Buffer;
                internal readonly int Offset;

                internal int Length;

                #endregion

                #region Constructors

                internal IndexBuffer(byte[] buffer, int offset, int length)
                {
                    Buffer = buffer;
                    Offset = offset;
                    Length = length;
                }

                internal IndexBuffer(byte[] buffer, int offset)
                {
                    Buffer = buffer;
                    Offset = offset;
                    Length = 1;
                }

                #endregion

                #region Methods

                #region Public Methods

                public bool Equals(IndexBuffer other)
                {
#if NETCOREAPP2_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER
                    return new ReadOnlySpan<byte>(Buffer, Offset, Length).SequenceEqual(new ReadOnlySpan<byte>(other.Buffer, other.Offset, other.Length));
#else
                    if (Length != other.Length)
                        return false;

                    unsafe
                    {
                        fixed (byte* pThis = &Buffer[Offset])
                        fixed (byte* pOther = &other.Buffer[other.Offset])
                            return MemoryHelper.CompareMemory(pThis, pOther, Length);
                    }
#endif
                }


                public override bool Equals(object? obj) => obj is IndexBuffer other && Equals(other);

#if NETCOREAPP3_0_OR_GREATER
                public override int GetHashCode()
                {
                    Debug.Assert(Length > 1, "Obtaining hashes are expected for non-single sequences");

                    int result;
                    ref byte pos = ref Buffer[Offset];
                    ref byte end = ref Unsafe.Add(ref pos, Length);

                    // 2 or 3 length
                    if (Length < 4)
                    {
                        result = 13;
                        while (Unsafe.IsAddressLessThan(ref pos, ref end))
                        {
                            result = result * 4099 + pos;
                            pos = ref Unsafe.Add(ref pos, 1);
                        }
                        return result;
                    }

                    // Including only the first and last 4 bytes (overlapping is allowed) to avoid high hash code cost.
                    // Prefixes of large single color areas are still differentiated by length.
                    result = 8209 * Length;
                    result = result * 2053 + Unsafe.ReadUnaligned<int>(ref pos);
                    return Length == 4 ? result : result * 1031 + Unsafe.ReadUnaligned<int>(ref Unsafe.Add(ref end, -4));
                }
#else
                public override unsafe int GetHashCode()
                {
                    Debug.Assert(Length > 1, "Obtaining hashes are expected for non-single sequences");

                    fixed (byte* pBuf = Buffer)
                    {
                        int result;
                        byte* pos = &pBuf[Offset];
                        byte* end = pos + Length;

                        // 2 or 3 length
                        if (Length < 4)
                        {
                            result = 13;
                            while (pos < end)
                            {
                                result = result * 4099 + *pos;
                                pos += 1;
                            }

                            return result;
                        }

                        // Including only the first and last 4 bytes (overlapping is allowed) to avoid high hash code cost.
                        // Prefixes of large single color areas are still differentiated by length.
                        result = 8209 * Length;
                        result = result * 2053 + *(int*)pos;
                        return Length == 4 ? result : result * 1031 + *(int*)(end - 4);
                    }
                }
#endif

                #endregion

                #region Internal Methods

                internal void AddNext() => Length += 1;

                #endregion

                #endregion
            }

            #endregion

            #region CodeTable struct

            private struct CodeTable : IDisposable
            {
                #region Constants

                private const int maxCodeCount = 1 << 12;

                #endregion

                #region Fields

#if NETCOREAPP2_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
                private readonly bool ownBuffer;
#endif
                private readonly Dictionary<IndexBuffer, int> codeTable;
                private readonly IndexBuffer indices;

                private int currentPosition;
                private int nextFreeCode;


                #endregion

                #region Properties

                #region Internal Properties

                internal int MinimumCodeSize { get; }
                internal int ClearCode => 1 << MinimumCodeSize;
                internal int EndInformationCode => ClearCode + 1;
                internal int CurrentCodeSize { get; private set; }
                internal byte CurrentIndex => indices.Buffer[indices.Offset + currentPosition];

                #endregion

                #region Private Properties

                private int FirstAvailableCode => ClearCode + 2;
                private int NextSizeLimit => 1 << CurrentCodeSize;

                #endregion

                #endregion

                #region Constructors

                public CodeTable(IReadableBitmapData imageData) : this()
                {
                    Debug.Assert(imageData.Palette?.Count <= 256);

                    // According to Appendix F in https://www.w3.org/Graphics/GIF/spec-gif89a.txt
                    // the minimum code size is 2 "because of some algorithmic constraints" (preserved code values)
                    MinimumCodeSize = Math.Max(2, imageData.Palette!.Count.ToBitsPerPixel());
                    CurrentCodeSize = MinimumCodeSize + 1;
                    currentPosition = -1;
                    int size = imageData.Width * imageData.Height;
                    codeTable = new Dictionary<IndexBuffer, int>(Math.Min(size, maxCodeCount - FirstAvailableCode));

                    // Trying to re-use the actual buffer if it is an 8-bit managed bitmap data; otherwise, allocating a new buffer.
                    // We need this to prevent allocating a huge memory for the code table keys with the segments: all segments will just be
                    // spans over the original sequence, which often will contain overlapping memory
                    if (imageData is ManagedBitmapData<byte, ManagedBitmapDataRow8I> managed8BitBitmapData)
                    {
                        // TODO: after releasing CoreLibraries 6.0.0 the AsArraySegment is not needed
                        ArraySegment<byte> segment = managed8BitBitmapData.Buffer.Buffer.AsArraySegment;
                        indices = new IndexBuffer(segment.Array!, segment.Offset, segment.Count);
                        return;
                    }

#if NETCOREAPP2_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
                    ownBuffer = true;
                    indices = new IndexBuffer(ArrayPool<byte>.Shared.Rent(size), 0, size);
#else
                    indices = new IndexBuffer(new byte[size], 0, size);
#endif

                    // If we could not obtain the actual buffer, then copying the palette indices into the newly allocated one.
                    // Not using parallel processing at this level (TODO?: use AsyncContext in encoder?)
                    int i = 0;
                    IReadableBitmapDataRow rowSrc = imageData.FirstRow;
                    for (int y = 0; y < imageData.Height; y++, rowSrc.MoveNextRow())
                    {
                        // we can index directly the array here without an offset because we allocated/rented it
                        for (int x = 0; x < imageData.Width; x++)
                            indices.Buffer[i++] = (byte)rowSrc.GetColorIndex(x);
                    }
                }

                #endregion

                #region Methods

                #region Public Method

                public void Dispose()
                {
#if NETCOREAPP2_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
                    if (ownBuffer)
                        ArrayPool<byte>.Shared.Return(indices.Buffer);
#endif
                }

                #endregion

                #region Internal Methods

                [MethodImpl(MethodImpl.AggressiveInlining)]
                internal void Reset()
                {
                    codeTable.Clear();
                    CurrentCodeSize = MinimumCodeSize + 1;
                    nextFreeCode = FirstAvailableCode;
                }

                [MethodImpl(MethodImpl.AggressiveInlining)]
                internal bool MoveNextIndex()
                {
                    if (currentPosition == indices.Length - 1)
                        return false;
                    currentPosition += 1;
                    return true;
                }

                internal IndexBuffer GetInitialSegment() => new IndexBuffer(indices.Buffer, indices.Offset + currentPosition);

                [MethodImpl(MethodImpl.AggressiveInlining)]
                internal bool TryGetCode(IndexBuffer key, out int code) => codeTable.TryGetValue(key, out code);

                [MethodImpl(MethodImpl.AggressiveInlining)]
                internal bool TryAddCode(IndexBuffer key)
                {
                    if (nextFreeCode == maxCodeCount)
                        return false;

                    if (nextFreeCode == NextSizeLimit)
                        CurrentCodeSize += 1;
                    codeTable.Add(key, nextFreeCode);
                    nextFreeCode += 1;
                    return true;
                }

                #endregion

                #endregion
            }

            #endregion

            #region BitWriter struct

            private unsafe ref struct BitWriter
            {
                #region Constants

                private const int bufferCapacity = 255;

                #endregion

                #region Fields

                private readonly BinaryWriter writer;

                private int accumulator;
                private int accumulatorSize;
                private int bufferLength;

#pragma warning disable CS0649 // field is never assigned - false alarm, a fixed buffer should not be assigned
                private fixed byte buffer[bufferCapacity];
#pragma warning restore CS0649

                #endregion

                #region Constructors

                internal BitWriter(BinaryWriter writer) : this() => this.writer = writer;

                #endregion

                #region Methods

                #region Internal Methods

                internal void WriteByte(byte value) => writer.Write(value);

                internal void WriteCode(int code, int bitSize)
                {
                    Debug.Assert(bitSize + accumulatorSize <= sizeof(int) * 8);
                    if (BitConverter.IsLittleEndian)
                    {
                        if (bitSize == 0)
                            accumulator = code;
                        else
                            accumulator |= code << accumulatorSize;
                        accumulatorSize += bitSize;
                    }
                    else
                    {
                        // we must use little endian order regardless of current architecture
                        int remainingSize = bitSize;
                        while (remainingSize > 8)
                        {
                            accumulator |= (code & 0xFF) << accumulatorSize;
                            accumulatorSize += 8;
                            remainingSize -= 8;
                            code >>= 8;
                        }

                        if (remainingSize > 0)
                        {
                            accumulator |= (code & 0xFF) << accumulatorSize;
                            accumulatorSize += remainingSize;
                        }
                    }

                    while (accumulatorSize > 8)
                    {
                        Append((byte)(accumulator & 0xFF));
                        accumulatorSize -= 8;
                        accumulator >>= 8;
                    }
                }

                internal void Flush()
                {
                    if (accumulatorSize > 0)
                        Append((byte)(accumulator & 0xFF));

                    if (bufferLength > 0)
                        DumpImageDataSubBlock();
                }

                #endregion

                #region Private Methods

                private void Append(byte value)
                {
                    Debug.Assert(bufferLength < 255);
                    buffer[bufferLength] = value;
                    bufferLength += 1;
                    if (bufferLength == bufferCapacity)
                        DumpImageDataSubBlock();
                }

                private void DumpImageDataSubBlock()
                {
                    Debug.Assert(bufferLength is > 0 and <= bufferCapacity);
                    WriteByte((byte)bufferLength);

                    // This causes CS1666. TODO: apply when https://github.com/dotnet/roslyn/issues/57583 is fixed
                    //writer.Write(new ReadOnlySpan<byte>(buffer, bufferLength));

                    for (int i = 0; i < bufferLength; i++)
                        WriteByte(buffer[i]);
                    bufferLength = 0;
                }

                #endregion

                #endregion
            }

            #endregion

            #endregion

            #region Fields

            private CodeTable codeTable;
            private BitWriter writer;

            #endregion

            #region Constructors

            internal LzwEncoder(IReadableBitmapData imageData, BinaryWriter writer)
            {
                codeTable = new CodeTable(imageData);
                this.writer = new BitWriter(writer);
            }

            #endregion

            #region Methods

            #region Public Methods

            public void Dispose() => codeTable.Dispose();

            #endregion

            #region Internal Methods

            internal void Encode()
            {
                writer.WriteByte((byte)codeTable.MinimumCodeSize);
                writer.WriteCode(codeTable.ClearCode, codeTable.CurrentCodeSize);
                codeTable.Reset();

                codeTable.MoveNextIndex();
                int previousCode = codeTable.CurrentIndex;
                IndexBuffer indexBuffer = codeTable.GetInitialSegment();

                while (codeTable.MoveNextIndex())
                {
                    indexBuffer.AddNext();
                    if (codeTable.TryGetCode(indexBuffer, out int code))
                    {
                        previousCode = code;
                        continue;
                    }

                    writer.WriteCode(previousCode, codeTable.CurrentCodeSize);
                    previousCode = codeTable.CurrentIndex;

                    if (!codeTable.TryAddCode(indexBuffer))
                    {
                        writer.WriteCode(codeTable.ClearCode, codeTable.CurrentCodeSize);
                        codeTable.Reset();
                    }

                    indexBuffer = codeTable.GetInitialSegment();
                }

                writer.WriteCode(previousCode, codeTable.CurrentCodeSize);
                writer.WriteCode(codeTable.EndInformationCode, codeTable.CurrentCodeSize);
                writer.Flush();
                writer.WriteByte(blockTerminator);
            }

            #endregion

            #endregion
        }
    }
}