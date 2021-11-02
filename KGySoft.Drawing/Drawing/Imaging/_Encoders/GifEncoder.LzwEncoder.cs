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
using System.IO;
using System.Linq;

#endregion

namespace KGySoft.Drawing.Imaging
{
    public partial class GifEncoder
    {
        #region Nested classes

        #region LzwEncoder class

        /// <summary>
        /// The LZW Encoder based on the specification as per chapter 22 and Appendix F in https://www.w3.org/Graphics/GIF/spec-gif89a.txt
        /// The detailed LZW algorithm is written here: http://giflib.sourceforge.net/whatsinagif/lzw_image_data.html
        /// </summary>
        private class LzwEncoder
        {
            #region Nested structs

            #region IndexBuffer struct

            private struct IndexBuffer : IEquatable<IndexBuffer>
            {
                #region Fields

                // TODO: keep the first 8 bytes in a long (allocation free)
                // TODO: max 255? Fixed buffer? Array?
                private readonly List<byte> buffer;

                #endregion

                #region Properties

                internal int Length => buffer.Count;

                #endregion

                #region Operators

                public static bool operator ==(IndexBuffer left, IndexBuffer right) => left.Equals(right);

                public static bool operator !=(IndexBuffer left, IndexBuffer right)
                {
                    if (!left.Equals(right))
                        return true;
                    return false;
                }

                #endregion

                #region Constructors

                internal IndexBuffer(byte initialByte)
                {
                    buffer = new List<byte> { initialByte };
                }

                #endregion

                #region Methods

                #region Public Methods

                // TODO: as span (faster if array)
                public bool Equals(IndexBuffer other) => buffer.SequenceEqual(other.buffer);

                public override bool Equals(object? obj) => obj is IndexBuffer other && Equals(other);

                public override int GetHashCode()
                {
                    // TODO: Span, and then just AddBytes
                    var hashCode = new HashCode();
                    foreach (byte b in buffer)
                        hashCode.Add(b);
                    return hashCode.ToHashCode();
                }

                #endregion

                #region Internal Methods

                internal void Add(byte b) => buffer.Add(b);

                #endregion

                #endregion
            }

            #endregion

            #endregion

            #region Constants

            private const int maxCodeCount = 1 << 12;

            #endregion

            #region Fields

            private readonly BinaryWriter writer;
            private readonly int minimumCodeSize;
            private readonly Dictionary<IndexBuffer, int> codeTable = new();
            private readonly byte[] buffer = new byte[255];

            private int nextFreeCode;
            private PixelIndexEnumerator pixelEnumerator;
            private int accumulator;
            private int accumulatorSize;
            private int currentCodeSize;
            private int bufferLength;

            #endregion

            #region Properties

            private int ClearCode => 1 << minimumCodeSize;
            private int EndInformationCode => ClearCode + 1;
            private int FirstAvailableCode => ClearCode + 2;
            private int NextSizeLimit => 1 << currentCodeSize;

            #endregion

            #region Constructors

            internal LzwEncoder(IReadableBitmapData imageData, BinaryWriter writer)
            {
                Debug.Assert(imageData.Palette != null);
                this.writer = writer;

                // According to Appendix F in https://www.w3.org/Graphics/GIF/spec-gif89a.txt
                // the minimum code size is 2 "because of some algorithmic constraints" (preserved code values)
                minimumCodeSize = Math.Max(2, imageData.Palette!.Count.ToBitsPerPixel());
                currentCodeSize = minimumCodeSize + 1;
                pixelEnumerator = new PixelIndexEnumerator(imageData);
            }

            #endregion

            #region Methods

            #region Internal Methods

            internal void Encode()
            {
                writer.Write((byte)minimumCodeSize);
                ResetCodeTable();

                pixelEnumerator.MoveNext();
                int previousCode = pixelEnumerator.Current;
                var indexBuffer = new IndexBuffer((byte)previousCode);

                while (pixelEnumerator.MoveNext())
                {
                    byte nextIndex = pixelEnumerator.Current;
                    indexBuffer.Add(nextIndex);

                    if (codeTable.TryGetValue(indexBuffer, out int code))
                    {
                        previousCode = code;
                        continue;
                    }

                    WriteCode(previousCode);
                    previousCode = nextIndex;

                    if (nextFreeCode == maxCodeCount)
                        ResetCodeTable();
                    else
                    {
                        if (nextFreeCode == NextSizeLimit)
                            currentCodeSize += 1;
                        codeTable.Add(indexBuffer, nextFreeCode);
                        nextFreeCode += 1; // TODO: in CodeTable
                    }

                    indexBuffer = new IndexBuffer(nextIndex);
                }

                WriteCode(previousCode);
                WriteCode(EndInformationCode);
                Flush();
                writer.Write(blockTerminator);
            }

            #endregion

            #region Private Methods

            private void ResetCodeTable()
            {
                // TODO: in some new CodeTable?
                WriteCode(ClearCode);
                codeTable.Clear();
                currentCodeSize = minimumCodeSize + 1;
                nextFreeCode = FirstAvailableCode;
            }

            private void WriteCode(int code)
            {
                // TODO: in a BitWriter type?
                Debug.Assert(currentCodeSize + accumulatorSize <= sizeof(int) * 8);
                if (BitConverter.IsLittleEndian)
                {
                    if (currentCodeSize == 0)
                        accumulator = code;
                    else
                        accumulator |= code << accumulatorSize;
                    accumulatorSize += currentCodeSize;
                }
                else
                {
                    // TODO: test this branch
                    // we must use little endian order regardless of current architecture
                    int remainingSize = currentCodeSize;
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

        #endregion

        #endregion
    }
}
