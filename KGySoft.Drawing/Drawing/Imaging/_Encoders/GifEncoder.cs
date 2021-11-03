#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: GifEncoder.cs
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
using System.Collections.Specialized;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

#endregion

namespace KGySoft.Drawing.Imaging
{
    /// <summary>
    /// Provides an encoder for GIF image format that supports animation. Use the static members for high-level access or create an
    /// instance to control everything manually.
    /// </summary>
    public sealed partial class GifEncoder
    {
        #region Constants

        private const byte extensionIntroducer = 0x21;
        private const byte applicationExtensionLabel = 0xFF;
        private const byte applicationBlockSize = 11;
        private const byte netscapeSubBlockSize = 3;
        private const byte netscapeLoopBlockId = 1;
        private const byte blockTerminator = 0;
        private const byte graphicControlLabel = 0xF9;
        private const byte graphicControlBlockSize = 4;
        private const byte imageSeparator = 0x2C;
        private const byte commentLabel = 0xFE;

        #endregion

        #region Fields

        #region Static Fields

        private static readonly byte[] header = GetBytes("GIF89a");
        private static readonly byte[] emptyColor = new byte[3];
        private static readonly byte[] netscapeApplicationBlock = GetBytes("NETSCAPE2.0");
        private static readonly byte[] signatureBlock = GetBytes("KGy SOFT GIF Encoder");

        private static readonly BitVector32.Section globalColorTableSizeSection = BitVector32.CreateSection(0b111);
        private static readonly BitVector32.Section globalSortFlag = BitVector32.CreateSection(1, globalColorTableSizeSection);
        private static readonly BitVector32.Section colorResolutionSection = BitVector32.CreateSection(0b111, globalSortFlag);
        private static readonly BitVector32.Section globalColorTableFlag = BitVector32.CreateSection(1, colorResolutionSection);
        private static readonly BitVector32.Section transparentColorFlag = BitVector32.CreateSection(1);
        private static readonly BitVector32.Section userInputFlag = BitVector32.CreateSection(1, transparentColorFlag);
        private static readonly BitVector32.Section disposalMethodSection = BitVector32.CreateSection(0b111, userInputFlag);
        private static readonly BitVector32.Section localColorTableSizeSection = BitVector32.CreateSection(0b111);
        private static readonly BitVector32.Section reservedSection = BitVector32.CreateSection(0b11, localColorTableSizeSection);
        private static readonly BitVector32.Section localSortFlag = BitVector32.CreateSection(1, reservedSection);
        private static readonly BitVector32.Section interlaceFlag = BitVector32.CreateSection(1, localSortFlag);
        private static readonly BitVector32.Section localColorTableFlag = BitVector32.CreateSection(1, interlaceFlag);

        #endregion

        #region Instance Fields

        private readonly BinaryWriter writer;
        private readonly Size logicalScreenSize;

        private bool isFirstImage = true;
        private int? repeatCount;
        private Palette? globalPalette;
        private byte backColorIndex;

        #endregion

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the number of repetitions if creating an animation.
        /// Set a non-<see langword="null"/> value to add the <c>NETSCAPE2.0</c> extension to the stream and to indicate that added images
        /// should be interpreted as animation frames. Use <c>0</c> to loop the animation indefinitely.
        /// If <see langword="null"/>, and images are added with 0 delay, then GDI+ handles image as a multi-layer single frame image,
        /// though some application (including browsers) still may play them as frames.
        /// <br/>Default value: <see langword="null"/>.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is less than 0.</exception>
        /// <exception cref="InvalidOperationException">This property cannot be set after adding the first image.</exception>
        public int? RepeatCount
        {
            get => repeatCount;
            set
            {
                if (!isFirstImage)
                    throw new InvalidOperationException(Res.GifEncoderCannotChangeProperty);
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value), PublicResources.ArgumentOutOfRange);
                repeatCount = value;
            }
        }

        /// <summary>
        /// Gets or sets the global palette. If not set, then each added image will be stored along with their own palette.
        /// If not <see langword="null"/>, then added the palette of the added images are stored only they are different from the global palette.
        /// <br/>Default value: <see langword="null"/>.
        /// </summary>
        /// <exception cref="ArgumentException"><paramref name="value"/> has more than 256 colors.</exception>
        /// <exception cref="InvalidOperationException">This property cannot be set after adding the first image.</exception>
        public Palette? GlobalPalette
        {
            get => globalPalette;
            set
            {
                if (!isFirstImage)
                    throw new InvalidOperationException(Res.GifEncoderCannotChangeProperty);
                if (globalPalette?.Count > 256)
                    throw new ArgumentException(Res.GifEncoderPaletteTooLarge, nameof(value));
                globalPalette = value;
            }
        }

        /// <summary>
        /// Gets or sets the background color index if <see cref="GlobalPalette"/> is set.
        /// It is relevant only if the palette of the first added image has no transparent entry, in which case
        /// determines the initial background color if the first added image does not completely cover the virtual screen,
        /// and also the color of the cleared virtual screen.
        /// </summary>
        public byte BackColorIndex
        {
            get => backColorIndex;
            set
            {
                if (!isFirstImage)
                    throw new InvalidOperationException(Res.GifEncoderCannotChangeProperty);
                backColorIndex = value;
            }
        }

        /// <summary>
        /// Gets or sets whether textual meta info should be added to the result stream.
        /// <br/>Default value: <see langword="true"/>.
        /// </summary>
        public bool AddMetaInfo { get; set; } = true;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="GifEncoder"/> class.
        /// </summary>
        /// <param name="stream">The writable stream to save the image content.</param>
        /// <param name="size">Specifies the logical screen size. It also determines the maximum size of the added images.</param>
        public GifEncoder(Stream stream, Size size)
        {
            if (stream == null!)
                throw new ArgumentNullException(nameof(stream), PublicResources.ArgumentNull);
            if (size.Width <= 0 || size.Height <= 0 || size.Width > UInt16.MaxValue || size.Height > UInt16.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(size), PublicResources.ArgumentEmpty);

            writer = new BinaryWriter(stream);
            logicalScreenSize = size;
        }

        #endregion

        #region Methods

        #region Static Methods

        private static byte[] GetBytes(string chars) => chars.Select(c => (byte)c).ToArray();

        #endregion

        #region Instance Methods

        #region Public Methods

        /// <summary>
        /// Writes an image to the output stream.
        /// </summary>
        /// <param name="imageData">The image data to write. Non-indexed images will be quantized by using the <see cref="GlobalPalette"/>, or, if that is not set,
        /// by <see cref="PredefinedColorsQuantizer.SystemDefault8BppPalette"/> using no dithering.</param>
        /// <param name="location">Specifies the location of the current image within the logical screen.</param>
        /// <param name="delay">Specifies the delay before rendering the next image in hundredths of a second. <c>0</c> usually interpreted as 100ms by browsers (as if 10 was specified),
        /// while GDI+ treats it zero delay only if <see cref="RepeatCount"/> is <see langword="null"/>.</param>
        /// <param name="disposalMethod">Specifies how the decoder should treat the image after being displayed. This parameter is optional.
        /// <br/>Default value: <see cref="GifGraphicDisposalMethod.NotSpecified"/>.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void AddImage(IReadableBitmapData imageData, Point location = default, int delay = 0, GifGraphicDisposalMethod disposalMethod = GifGraphicDisposalMethod.NotSpecified)
        {
            if (imageData == null)
                throw new ArgumentNullException(nameof(imageData), PublicResources.ArgumentNull);
            if (location.X < 0 || location.Y < 0 || location.X + imageData.Width > logicalScreenSize.Width || location.Y + imageData.Height > logicalScreenSize.Height)
                throw new ArgumentOutOfRangeException(nameof(location), PublicResources.ArgumentOutOfRange);
            if ((uint)delay > UInt16.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(delay), PublicResources.ArgumentMustBeBetween(UInt16.MinValue, UInt16.MaxValue));
            if ((uint)disposalMethod > (uint)GifGraphicDisposalMethod.RestoreToPrevious)
                throw new ArgumentOutOfRangeException(nameof(disposalMethod), PublicResources.EnumOutOfRange(disposalMethod));

            IReadableBitmapData actualImageData = imageData.Palette == null || imageData.Palette.Count > 256 ? imageData.Clone(PixelFormat.Format8bppIndexed, globalPalette) : imageData;

            try
            {
                Debug.Assert(actualImageData.Palette != null);
                Palette? localPalette = actualImageData.Palette!;
                if (localPalette.EntriesEqual(globalPalette))
                    localPalette = null;

                Palette usedPalette = (localPalette ?? globalPalette)!;

                if (isFirstImage)
                {
                    isFirstImage = false;
                    WriteHeaderAndLogicalScreenDescriptor(actualImageData.PixelFormat.ToBitsPerPixel());
                    if (globalPalette != null)
                        WritePalette(globalPalette);

                    WriteCommentExtension(usedPalette.TransparentIndex >= 0);
                    if (repeatCount.HasValue)
                        WriteNetscapeLoopBlockApplicationExtension(repeatCount.Value);
                }

                if (delay != 0 || disposalMethod != GifGraphicDisposalMethod.NotSpecified || usedPalette.TransparentIndex >= 0)
                    WriteGraphicControlExtension(delay, disposalMethod, usedPalette.TransparentIndex);

                WriteImageDescriptor(location, actualImageData.GetSize(), localPalette);
                if (localPalette != null)
                    WritePalette(localPalette);

                WriteImageData(actualImageData);
            }
            finally
            {
                if (!ReferenceEquals(actualImageData, imageData))
                    actualImageData.Dispose();
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// See the details in chapter 17-18 in https://www.w3.org/Graphics/GIF/spec-gif89a.txt
        /// </summary>
        private void WriteHeaderAndLogicalScreenDescriptor(int bpp)
        {
            // Signature + Version (3 + 3 bytes)
            writer.Write(header);

            // Logical Screen Width + Height
            writer.Write((ushort)logicalScreenSize.Width);
            writer.Write((ushort)logicalScreenSize.Height);

            // Packed fields (LSB to MSB):
            // Size of Global Color Table    3 Bits
            // Sort Flag                     1 Bit
            // Color Resolution              3 Bits
            // Global Color Table Flag       1 Bit
            var packedFields = new BitVector32();
            packedFields[globalColorTableFlag.GetMask()] = globalPalette != null;
            packedFields[globalColorTableSizeSection] = (globalPalette?.Count.ToBitsPerPixel() ?? bpp) - 1;
            packedFields[colorResolutionSection] = bpp - 1;
            writer.Write((byte)packedFields.Data);

            // Background Color Index
            writer.Write(backColorIndex < globalPalette?.Count ? backColorIndex : (byte)0);

            // Pixel Aspect Ratio: writing always 0 (1:1)
            writer.Write((byte)0);
        }

        /// <summary>
        /// See the details in chapters 19 and 21 in https://www.w3.org/Graphics/GIF/spec-gif89a.txt
        /// </summary>
        private void WritePalette(Palette palette)
        {
            foreach (Color32 color in palette.Entries)
            {
                writer.Write(color.R);
                writer.Write(color.G);
                writer.Write(color.B);
            }

            // filling up the rest of the colors (GIF supports log2 palette sizes)
            int maxColors = ((uint)palette.Count).RoundUpToPowerOf2();
            for (int i = palette.Count + 1; i < maxColors; i++)
                writer.Write(emptyColor);
        }

        /// <summary>
        /// Writing the comment extension as per chapters 24 and 15 in https://www.w3.org/Graphics/GIF/spec-gif89a.txt
        /// </summary>
        private void WriteCommentExtension(bool isTransparent)
        {
            writer.Write(extensionIntroducer);
            writer.Write(commentLabel);

            // signature block
#if NETCOREAPP2_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER
            WriteCommentSubBlock(signatureBlock);
#else
            WriteCommentSubBlock(new ArraySegment<byte>(signatureBlock));
#endif

            // Important: not using resources here because only 7-bit ASCII is supported and because resources could be longer than 255 byte
            if (AddMetaInfo)
            {
#if NETCOREAPP2_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER
                Span<byte> buffer = stackalloc byte[255];
#else
                var buffer = new byte[255];
#endif
                WriteCommentSubBlock(FormatComment(buffer, $"Global Palette: {(globalPalette == null ? "Not present" : $"{globalPalette.Count} colors")}"));
                WriteCommentSubBlock(FormatComment(buffer, $"Back Color: {(isTransparent || globalPalette == null ? "Transparent" : $"{globalPalette.Entries[backColorIndex >= globalPalette.Count ? 0 : backColorIndex].ToRgb():X6}")}"));
                WriteCommentSubBlock(FormatComment(buffer, $"Repeat Count: {(repeatCount switch { null => "Not present", 0 => "Infinite", _ => repeatCount })}"));
            }

            writer.Write(blockTerminator);

            #region Local Methods

#if NETCOREAPP2_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER
            static Span<byte> FormatComment(Span<byte> buffer, string text)
            {
                Debug.Assert(text.Length <= 255);
                for (int i = 0; i < text.Length; i++)
                    buffer[i] = (byte)text[i];
                return buffer.Slice(0, text.Length);
            }

            void WriteCommentSubBlock(Span<byte> buffer)
            {
                writer.Write((byte)buffer.Length);
                writer.Write(buffer);
            }
#else
            static ArraySegment<byte> FormatComment(byte[] buffer, string text)
            {
                Debug.Assert(text.Length <= 255);
                for (int i = 0; i < text.Length; i++)
                    buffer[i] = (byte)text[i];
                return new ArraySegment<byte>(buffer, 0, text.Length);
            }

            void WriteCommentSubBlock(ArraySegment<byte> buffer)
            {
                writer.Write((byte)buffer.Count);
                writer.Write(buffer.Array!, buffer.Offset, buffer.Count);
            }
#endif

            #endregion
        }

        /// <summary>
        /// Writing the nonstandard NETSCAPE2.0 extension as per chapters 26 and 15 in https://www.w3.org/Graphics/GIF/spec-gif89a.txt
        /// </summary>
        private void WriteNetscapeLoopBlockApplicationExtension(int value)
        {
            writer.Write(extensionIntroducer);
            writer.Write(applicationExtensionLabel);
            writer.Write(applicationBlockSize);
            writer.Write(netscapeApplicationBlock); // 8 + 3 bytes Application Identifier + Authentication Code

            // Now the application-specific sub-block. The GIF spec describes only the length and the terminator.
            writer.Write(netscapeSubBlockSize);
            writer.Write(netscapeLoopBlockId);
            writer.Write((short)value);

            writer.Write(blockTerminator);
        }

        /// <summary>
        /// Writing the Graphic Control Extension as per chapter 23 in https://www.w3.org/Graphics/GIF/spec-gif89a.txt
        /// </summary>
        private void WriteGraphicControlExtension(int delay, GifGraphicDisposalMethod disposalMethod, int transparentIndex)
        {
            writer.Write(extensionIntroducer);
            writer.Write(graphicControlLabel);
            writer.Write(graphicControlBlockSize);

            // Packed fields (LSB to MSB):
            // Transparent Color Flag        1 Bit
            // User Input Flag               1 Bit
            // Disposal Method               3 Bits
            // Reserved                      3 Bits
            var packedFields = new BitVector32();
            packedFields[disposalMethodSection] = (int)disposalMethod;
            packedFields[transparentColorFlag.GetMask()] = transparentIndex >= 0;
            writer.Write((byte)packedFields.Data);

            writer.Write((ushort)delay);
            writer.Write((byte)(transparentIndex >= 0 ? transparentIndex : 0));
            writer.Write(blockTerminator);
        }

        /// <summary>
        /// Writing the Graphic Control Extension as per chapter 20 in https://www.w3.org/Graphics/GIF/spec-gif89a.txt
        /// </summary>
        private void WriteImageDescriptor(Point location, Size size, Palette? localPalette)
        {
            writer.Write(imageSeparator);
            writer.Write((ushort)location.X);
            writer.Write((ushort)location.Y);
            writer.Write((ushort)size.Width);
            writer.Write((ushort)size.Height);

            // Packed fields (LSB to MSB):
            // Size of Local Color Table     3 Bits
            // Reserved                      2 Bits
            // Sort Flag                     1 Bit
            // Interlace Flag                1 Bit
            // Local Color Table Flag        1 Bit
            var packedFields = new BitVector32();
            if (localPalette != null)
            {
                packedFields[localColorTableFlag.GetMask()] = true;
                packedFields[localColorTableSizeSection] = localPalette.Count.ToBitsPerPixel() - 1;
            }

            writer.Write((byte)packedFields.Data);
        }

        private void WriteImageData(IReadableBitmapData imageData)
        {
            using var lzwEncoder = new LzwEncoder(imageData, writer);
            lzwEncoder.Encode();
        }

        #endregion

        #endregion

        #endregion
    }
}
