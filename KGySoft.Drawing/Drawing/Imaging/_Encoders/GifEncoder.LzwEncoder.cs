#region Copyright

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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;

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
        private class LzwEncoder : IDisposable
        {
            #region Nested structs

            #region IndexBuffer struct

            /// <summary>
            /// Represents a segment of a byte array. It could also be an <see cref="ArraySegment{T}"/>
            /// or <see cref="ArraySection{T}"/> but it is faster if we can directly mutate only the length.
            /// </summary>
            private struct IndexBuffer : IEquatable<IndexBuffer>
            {
                #region Fields

                private readonly byte[] buffer;
                private readonly int offset;

                private int length;

                #endregion

                #region Constructors

                internal IndexBuffer(byte[] buffer, int offset)
                {
                    this.buffer = buffer;
                    this.offset = offset;
                    length = 1;
                }

                #endregion

                #region Methods

                #region Public Methods

                public bool Equals(IndexBuffer other)
                {
#if NETCOREAPP2_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER
                    return new ReadOnlySpan<byte>(buffer, offset, length).SequenceEqual(new ReadOnlySpan<byte>(other.buffer, other.offset, other.length));
#else
                    if (length != other.length)
                        return false;

                    unsafe
                    {
                        fixed (byte* pThis = &buffer[offset])
                        fixed (byte* pOther = &other.buffer[other.offset])
                            return MemoryHelper.CompareMemory(pThis, pOther, length);
                    }
#endif
                }


                public override bool Equals(object? obj) => obj is IndexBuffer other && Equals(other);

#if NETCOREAPP3_0_OR_GREATER
                public override int GetHashCode()
                {
                    Debug.Assert(length > 1, "Obtaining hashes are expected for non-single sequences");

                    int result;
                    ref byte pos = ref buffer[offset];
                    ref byte end = ref Unsafe.Add(ref pos, length);

                    // 2 or 3 length
                    if (length < 4)
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
                    result = 8209 * length;
                    result = result * 2053 + Unsafe.ReadUnaligned<int>(ref pos);
                    return length == 4 ? result : result * 1031 + Unsafe.ReadUnaligned<int>(ref Unsafe.Add(ref end, -4));
                }
#else
                public override unsafe int GetHashCode()
                {
                    Debug.Assert(length > 1, "Obtaining hashes are expected for non-single sequences");

                    fixed (byte* pBuf = buffer)
                    {
                        int result;
                        byte* pos = &pBuf[offset];
                        byte* end = pos + length;

                        // 2 or 3 length
                        if (length < 4)
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
                        result = 8209 * length;
                        result = result * 2053 + *(int*)pos;
                        return length == 4 ? result : result * 1031 + *(int*)(end - 4);
                    }
                }
#endif

                #endregion

                #region Internal Methods

                internal void AddNext() => length += 1;

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

                [SuppressMessage("Style", "IDE0044:Add readonly modifier",
                    Justification = "It is not readonly in all targeted platforms so we need to prevent creating defensive copies.")]
                // ReSharper disable once FieldCanBeMadeReadOnly.Local
                private ArraySegment<byte> indices;

                private int currentPosition;
                private int nextFreeCode;


                #endregion

                #region Properties

                #region Internal Properties

                internal int MinimumCodeSize { get; }
                internal int ClearCode => 1 << MinimumCodeSize;
                internal int EndInformationCode => ClearCode + 1;
                internal int CurrentCodeSize { get; private set; }
                internal byte CurrentIndex => indices.Array![indices.Offset + currentPosition];

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
                    codeTable = new Dictionary<IndexBuffer, int>(maxCodeCount - FirstAvailableCode);

                    // Trying to re-use the actual buffer if it is an 8-bit managed bitmap data; otherwise, allocating a new buffer
                    // We need this to prevent allocating a huge memory for the code table keys with the segments: all segments will just be
                    // spans over the original sequence, which often will contain overlapping memory
                    if (imageData is ManagedBitmapData<byte, ManagedBitmapDataRow8I> managed8BitBitmapData)
                    {
                        indices = managed8BitBitmapData.Buffer.Buffer.AsArraySegment;
                        return;
                    }

                    int size = imageData.Width * imageData.Height;
#if NETCOREAPP2_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
                    ownBuffer = true;
                    indices = new ArraySegment<byte>(ArrayPool<byte>.Shared.Rent(size), 0, size);
#else
                    indices = new ArraySegment<byte>(new byte[size], 0, size);
#endif

                    // If we could not obtain the actual buffer, then copying the palette indices into the newly allocated one.
                    // Not using parallel processing at this level (TODO?: use AsyncContext in encoder?)
                    int i = 0;
                    IReadableBitmapDataRow rowSrc = imageData.FirstRow;
                    for (int y = 0; y < imageData.Height; y++, rowSrc.MoveNextRow())
                    {
                        // we can index directly the array here without an offset because we allocated/rented it
                        for (int x = 0; x < imageData.Width; x++)
                            indices.Array![i++] = (byte)rowSrc.GetColorIndex(x);
                    }
                }

                #endregion

                #region Methods

                #region Public Method

                public void Dispose()
                {
#if NETCOREAPP2_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
                    if (ownBuffer)
                        ArrayPool<byte>.Shared.Return(indices.Array!);
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
                    if (currentPosition == indices.Count - 1)
                        return false;
                    currentPosition += 1;
                    return true;
                }

                internal IndexBuffer GetInitialSegment() => new IndexBuffer(indices.Array!, indices.Offset + currentPosition);


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

            #endregion

            #region Fields

            private readonly BinaryWriter writer;
            private readonly byte[] buffer = new byte[255];

            private CodeTable codeTable;

            private int accumulator;
            private int accumulatorSize;
            private int bufferLength;

            #endregion

            #region Constructors

            internal LzwEncoder(IReadableBitmapData imageData, BinaryWriter writer)
            {
                Debug.Assert(imageData.Palette != null);
                this.writer = writer;
                codeTable = new CodeTable(imageData);
            }

            #endregion

            #region Methods

            #region Public Methods

            public void Dispose() => codeTable.Dispose();

            #endregion

            #region Internal Methods

            internal void Encode()
            {
                writer.Write((byte)codeTable.MinimumCodeSize);
                WriteCode(codeTable.ClearCode);
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

                    WriteCode(previousCode);
                    previousCode = codeTable.CurrentIndex;

                    if (!codeTable.TryAddCode(indexBuffer))
                    {
                        WriteCode(codeTable.ClearCode);
                        codeTable.Reset();
                    }

                    indexBuffer = codeTable.GetInitialSegment();
                }

                WriteCode(previousCode);
                WriteCode(codeTable.EndInformationCode);
                Flush();
                writer.Write(blockTerminator);
            }

            #endregion

            #region Private Methods

            private void WriteCode(int code)
            {
                // TODO: in a BitWriter type?
                Debug.Assert(codeTable.CurrentCodeSize + accumulatorSize <= sizeof(int) * 8);
                if (BitConverter.IsLittleEndian)
                {
                    if (codeTable.CurrentCodeSize == 0)
                        accumulator = code;
                    else
                        accumulator |= code << accumulatorSize;
                    accumulatorSize += codeTable.CurrentCodeSize;
                }
                else
                {
                    // TODO: test this branch
                    // we must use little endian order regardless of current architecture
                    int remainingSize = codeTable.CurrentCodeSize;
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
                    WriteByte((byte)(accumulator & 0xFF));
                    accumulatorSize -= 8;
                    accumulator >>= 8;
                }
            }

            private void WriteByte(byte b)
            {
                Debug.Assert(bufferLength < 255);
                buffer[bufferLength] = b;
                bufferLength += 1;
                if (bufferLength == 255)
                    WriteImageDataSubBlock();
            }

            private void WriteImageDataSubBlock()
            {
                Debug.Assert(bufferLength is > 0 and <= 255);
                writer.Write((byte)bufferLength);
                writer.Write(buffer, 0, bufferLength);
                bufferLength = 0;
            }

            private void Flush()
            {
                if (accumulatorSize > 0)
                    WriteByte((byte)(accumulator & 0xFF));

                if (bufferLength > 0)
                    WriteImageDataSubBlock();
            }

            #endregion

            #endregion
        }
    }
}
