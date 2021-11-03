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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

using KGySoft.Collections;
using KGySoft.CoreLibraries;

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

            private struct IndexBuffer : IEquatable<IndexBuffer>
            {
                #region Fields

                private ArraySection<byte> buffer;

                #endregion

                #region Constructors

                internal IndexBuffer(ArraySection<byte> initialSegment) => buffer = initialSegment;

                #endregion

                #region Methods

                #region Public Methods

                public bool Equals(IndexBuffer other)
                {

#if NETCOREAPP2_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER
                    return buffer.AsSpan.SequenceEqual(other.buffer.AsSpan);
#else
                    if (buffer.Length != other.buffer.Length)
                        return false;
                    ArraySegment<byte> segThis = buffer.AsArraySegment;
                    ArraySegment<byte> segOther = other.buffer.AsArraySegment;
                    unsafe
                    {
                        fixed (byte* pThis = &segThis.Array![segThis.Offset])
                        fixed (byte* pOther = &segOther.Array![segOther.Offset])
                            return MemoryHelper.CompareMemory(pThis, pOther, buffer.Length);
                    }
#endif
                }


                public override bool Equals(object? obj) => obj is IndexBuffer other && Equals(other);

                public override int GetHashCode()
                {
#if NETCOREAPP2_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER
                    var hashCode = new HashCode();
                    hashCode.AddBytes(buffer.AsSpan);
                    return hashCode.ToHashCode();
#else
                    int hashCode = 0;
                    for (int i = 0; i < buffer.Length; i++)
                        hashCode = (hashCode, i).GetHashCode();
                    return hashCode;
#endif
                }

                #endregion

                #region Internal Methods

                // TODO: rename and remove parameter
                internal void Add(byte b)
                {
                    ArraySegment<byte> segment = buffer.AsArraySegment;
                    buffer = segment.Array!.AsSection(segment.Offset, segment.Count + 1);
                    Debug.Assert(buffer[^1] == b);
                }

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

                private readonly bool ownBuffer;
                private readonly Dictionary<IndexBuffer, int> codeTable;

                [SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "False alarm, see ArraySection description.")]
                private ArraySection<byte> indices;

                private int currentPosition;
                private int nextFreeCode;


                #endregion

                #region Properties

                #region Internal Properties

                internal int MinimumCodeSize { get; }
                internal int ClearCode => 1 << MinimumCodeSize;
                internal int EndInformationCode => ClearCode + 1;
                internal int CurrentCodeSize { get; private set; }
                internal byte CurrentIndex => indices[currentPosition];

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
                    var managed8BitBitmapData = imageData as ManagedBitmapData<byte, ManagedBitmapDataRow8I>;
                    ownBuffer = managed8BitBitmapData == null;
                    indices = ownBuffer
                        ? new ArraySection<byte>(imageData.Width * imageData.Height)
                        : managed8BitBitmapData!.Buffer.Buffer;

                    if (!ownBuffer)
                        return;

                    // If we could not obtain the actual buffer, then copying the palette indices into the newly allocated one.
                    // Not using parallel processing at this level (TODO?: use AsyncContext in encoder?)
                    Array2D<byte> as2D = indices.AsArray2D(imageData.Height, imageData.Width);
                    IReadableBitmapDataRow rowSrc = imageData.FirstRow;
                    for (int y = 0; y < as2D.Height; y++, rowSrc.MoveNextRow())
                    {
                        for (int x = 0; x < as2D.Width; x++)
                            as2D[y, x] = (byte)rowSrc.GetColorIndex(x);
                    }
                }

                #endregion

                #region Methods

                #region Public Method

                public void Dispose()
                {
                    if (ownBuffer)
                        indices.Release();
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

                internal IndexBuffer GetInitialSegment() => new IndexBuffer(indices.Slice(currentPosition, 1));


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

            #region Properties


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
                    byte currentIndex = codeTable.CurrentIndex;
                    indexBuffer.Add(currentIndex);

                    if (codeTable.TryGetCode(indexBuffer, out int code))
                    {
                        previousCode = code;
                        continue;
                    }

                    WriteCode(previousCode);
                    previousCode = currentIndex;

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
