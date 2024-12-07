#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: GifEncoder.LzwEncoder.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2024 - All Rights Reserved
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
        [SecuritySafeCritical]
        private static class LzwEncoder
        {
            #region Nested structs

            #region CodeTable struct

            /// <summary>
            /// Provides the LZW code table implementation.
            /// It has been refactored to use open addressing double hashing instead of a regular Dictionary.
            /// Some implementation ideas were inspired by Kevin Weiner's Java encoder from here: http://www.java2s.com/Code/Java/2D-Graphics-GUI/AnimatedGifEncoder.htm
            /// Basically it uses a variant of Knuth's algorithm along with G. Knott's relatively-prime secondary probe.
            /// </summary>
            private ref struct CodeTable : IDisposable
            {
                #region Constants

                private const int maxBits = 12;
                private const int maxCodeCount = 1 << maxBits;

                /// <summary>
                /// A prime that provides about 80% occupancy in the code table considering the max used entries when bit size is 12.
                /// </summary>
                private const int tableSize = 5003;

                #endregion

                #region Fields

                private readonly GifCompressionMode compressionMode;
                private readonly int hashShiftSize;
                private readonly IReadableBitmapDataRowMovable? currentRow;

                /// <summary>
                /// Earlier versions used a dictionary as a code table where the key was a span of palette indices.
                /// Even with shared underlying buffer memory and optimized GetHashCode/Equals implementation it was much less efficient.
                /// Here we can exploit that all prefixes of new codes are already stored so when there is a hash collision we don't need
                /// to perform an equality check for the whole segment repeatedly (this is what Dictionary does when there are more entries in the same bucket).
                /// Instead, we use double hashing and a match key for equality check:
                /// - The primary hash is calculated for the current prefix and is used to select a code table entry
                /// - A match key is used for equality check. It is calculated last code + current index combination and is stored along with prefix codes.
                /// - If equality check by match key fails (collision), then using a secondary hash to jump from entry to entry.
                /// The idea was taken from here: http://www.java2s.com/Code/Java/2D-Graphics-GUI/AnimatedGifEncoder.htm
                /// </summary>
                private readonly CastArray<byte, (int MatchKey, int Value)> entries;

                [SuppressMessage("Style", "IDE0044:Add readonly modifier",
                    Justification = "Though the used members are read-only, pre C# 8 compilers may emit a defensive copy for members access if the field is read-only")]
                [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Local", Justification = "As above")]
                private ArraySection<byte> indices;
                private int currentPosition;
                private int nextFreeCode;
                private int primaryHash;
                private int matchKey;

                #endregion

                #region Properties

                internal int MinimumCodeSize { get; }
                internal int ClearCode => 1 << MinimumCodeSize;
                internal int EndInformationCode => ClearCode + 1;
                internal int FirstAvailableCode => ClearCode + 2;
                internal int CurrentCodeSize { get; private set; }
                internal int NextSizeLimit => 1 << CurrentCodeSize;
                internal byte CurrentIndex { get; private set; }

                #endregion

                #region Constructors

                public unsafe CodeTable(IReadableBitmapData imageData, GifCompressionMode compressionMode) : this()
                {
                    Debug.Assert(imageData.Palette?.Count <= 256);
                    currentPosition = -1;
                    this.compressionMode = compressionMode;

                    // According to Appendix F in https://www.w3.org/Graphics/GIF/spec-gif89a.txt
                    // the minimum code size is 2 "because of some algorithmic constraints" (preserved code values)
                    MinimumCodeSize = Math.Max(2, imageData.Palette!.Count.ToBitsPerPixel());
                    CurrentCodeSize = MinimumCodeSize + 1;

                    // Trying to use the actual 8-bit index buffer if it is an 8-bit managed bitmap data for better performance
                    if (imageData is ManagedBitmapData8I managed8BitBitmapData)
                        indices = managed8BitBitmapData.Buffer.Buffer;
                    else
                        // Otherwise, accessing by BitmapDataRow (non-managed or 1/4 bpp images)
                        currentRow = imageData.FirstRow;

                    // In uncompressed mode we will only need code size. Even pixel enumeration will be different
                    // so we can spare allocating indices and the whole code table.
                    if (compressionMode == GifCompressionMode.Uncompressed)
                        return;

                    entries = new CastArray<byte, (int, int)>(new ArraySection<byte>(tableSize * sizeof((int, int)), false));

                    // Initializing the shift size for hash calculations. Basically we upscale the table size to >16 bits.
                    for (int i = tableSize; i < (1 << 16); i <<= 1)
                        ++hashShiftSize;
                    hashShiftSize = 8 - hashShiftSize;
                }

                #endregion

                #region Methods

                #region Public Method

                public void Dispose() => entries.Buffer.Release();

                #endregion

                #region Internal Methods

                [MethodImpl(MethodImpl.AggressiveInlining)]
                internal void Reset()
                {
                    Debug.Assert(!entries.IsNull, "Should not be called in Uncompressed mode");

                    entries.Clear();
                    CurrentCodeSize = MinimumCodeSize + 1;
                    nextFreeCode = FirstAvailableCode;
                }

                [MethodImpl(MethodImpl.AggressiveInlining)]
                internal bool MoveNextIndex()
                {
                    currentPosition += 1;

                    // fast route for managed 8bpp bitmap data: direct access
                    if (currentRow == null)
                    {
                        if (currentPosition == indices.Length)
                            return false;
                        CurrentIndex = indices[currentPosition];
                        return true;
                    }

                    // fallback for non-managed or 1/4 bpp bitmap data by currentRow.GetColorIndex
                    if (currentPosition == currentRow.Width)
                    {
                        if (!currentRow.MoveNextRow())
                            return false;

                        currentPosition = 0;
                    }

                    CurrentIndex = (byte)currentRow.GetColorIndex(currentPosition);
                    return true;
                }

                [MethodImpl(MethodImpl.AggressiveInlining)]
                [SecuritySafeCritical]
                internal bool TryGetNextCode(int previousCode, out int code)
                {
                    Debug.Assert(!entries.IsNull, "Should not be called in Uncompressed mode");
                    primaryHash = (CurrentIndex << hashShiftSize) ^ previousCode;
                    matchKey = (CurrentIndex << maxBits) + previousCode + 1;

                    Debug.Assert(primaryHash is >= 0 and < tableSize, "The primary hash should be a valid index in code table entries");
                    ref var entry = ref entries.GetElementReferenceUnsafe(primaryHash);

                    // a code for previousCode + CurrentIndex has been found
                    if (entry.MatchKey == matchKey)
                    {
                        code = entry.Value;
                        return true;
                    }

                    // hash collision: using a relatively prime secondary hash to jump from entry to entry
                    if (entry.MatchKey > 0)
                    {
                        int secondaryHash = primaryHash == 0 ? 1 : tableSize - primaryHash;
                        do
                        {
                            if ((primaryHash -= secondaryHash) < 0)
                                primaryHash += tableSize;

                            entry = ref entries.GetElementReferenceUnsafe(primaryHash);
                            if (entry.MatchKey == matchKey)
                            {
                                code = entry.Value;
                                return true;
                            }
                        } while (entry.MatchKey > 0);
                    }

                    code = default;
                    return false;
                }

                [MethodImpl(MethodImpl.AggressiveInlining)]
                internal bool TryAddNextCode()
                {
                    Debug.Assert(!entries.IsNull, "Should not be called in Uncompressed mode");
                    if (nextFreeCode == maxCodeCount || compressionMode == GifCompressionMode.DoNotIncreaseBitSize && nextFreeCode + 1 == NextSizeLimit)
                        return false;

                    if (nextFreeCode == NextSizeLimit)
                        CurrentCodeSize += 1;
                    ref var entry = ref entries.GetElementReferenceUnsafe(primaryHash);
                    entry.Value = nextFreeCode;
                    entry.MatchKey = matchKey;
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
                private fixed byte buffer[bufferCapacity];

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
                        if (accumulatorSize == 0)
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
                    Debug.Assert(bufferLength < bufferCapacity);
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

            #region Methods

            #region Internal Methods

            internal static void Encode(IReadableBitmapData imageData, BinaryWriter bw, GifCompressionMode compressionMode)
            {
                if (compressionMode == GifCompressionMode.Uncompressed)
                {
                    EncodeUncompressed(imageData, bw);
                    return;
                }

                using var codeTable = new CodeTable(imageData, compressionMode);
                var writer = new BitWriter(bw);

                writer.WriteByte((byte)codeTable.MinimumCodeSize);
                writer.WriteCode(codeTable.ClearCode, codeTable.CurrentCodeSize);
                codeTable.Reset();

                codeTable.MoveNextIndex();
                int previousCode = codeTable.CurrentIndex;

                while (codeTable.MoveNextIndex())
                {
                    if (codeTable.TryGetNextCode(previousCode, out int code))
                    {
                        previousCode = code;
                        continue;
                    }

                    writer.WriteCode(previousCode, codeTable.CurrentCodeSize);
                    previousCode = codeTable.CurrentIndex;

                    if (!codeTable.TryAddNextCode() && compressionMode != GifCompressionMode.DoNotClear)
                    {
                        writer.WriteCode(codeTable.ClearCode, codeTable.CurrentCodeSize);
                        codeTable.Reset();
                    }
                }

                writer.WriteCode(previousCode, codeTable.CurrentCodeSize);
                writer.WriteCode(codeTable.EndInformationCode, codeTable.CurrentCodeSize);
                writer.Flush();
                writer.WriteByte(blockTerminator);
            }

            #endregion

            #region Private Methods

            private static void EncodeUncompressed(IReadableBitmapData imageData, BinaryWriter bw)
            {
                using var codeTable = new CodeTable(imageData, GifCompressionMode.Uncompressed);
                var writer = new BitWriter(bw);

                int codeSize = codeTable.CurrentCodeSize;
                writer.WriteByte((byte)codeTable.MinimumCodeSize);
                writer.WriteCode(codeTable.ClearCode, codeSize);

                // Though we do not build the code table the decoder does so we must regularly send a clear to prevent increasing code size
                int maxLength = codeTable.NextSizeLimit - codeTable.FirstAvailableCode;
                int currLength = 0;

                while (codeTable.MoveNextIndex())
                {
                    writer.WriteCode(codeTable.CurrentIndex, codeSize);
                    currLength += 1;
                    if (currLength == maxLength)
                    {
                        currLength = 0;
                        writer.WriteCode(codeTable.ClearCode, codeSize);
                    }
                }

                writer.WriteCode(codeTable.EndInformationCode, codeSize);
                writer.Flush();
                writer.WriteByte(blockTerminator);
            }

            #endregion

            #endregion
        }
    }
}
