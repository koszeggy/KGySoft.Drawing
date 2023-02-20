#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: GifEncoder.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2023 - All Rights Reserved
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
using System.IO;
using System.Linq;

using KGySoft.CoreLibraries;

#endregion

namespace KGySoft.Drawing.Imaging
{
    /// <summary>
    /// Provides an encoder for GIF image format that supports animation. Use the static members for high-level access or create an instance to
    /// control everything manually.
    /// </summary>
    /// <remarks>
    /// <para>The simplest way to create a single-frame GIF image is calling the static <see cref="EncodeImage">EncodeImage</see> method. It can
    /// quantize and dither any input <see cref="IReadableBitmapData"/> source.</para>
    /// <para>The simplest way to create a GIF animation is calling the static <see cref="EncodeAnimation">EncodeAnimation</see> method. It expects
    /// an <see cref="AnimatedGifConfiguration"/> that describes the frames and delays to be used along with numerous optional configuration such as
    /// a specific quantizer and ditherer, looping mode, handling of possible different input image sizes, encoding strategies like allowing
    /// delta images or explicitly encoding transparent borders.
    /// <note type="tip">If you use an <see cref="OptimizedPaletteQuantizer"/> and the <see cref="AnimatedGifConfiguration.AllowDeltaFrames"/> property
    /// is <see langword="true"/>, then you can create really high quality animations allowing more than 256 colors per frame.</note></para>
    /// <para>Alternatively, you can instantiate the <see cref="GifEncoder"/> class, which allows you even more control at lower levels.
    /// The <see cref="RepeatCount"/>, <see cref="GlobalPalette"/> and <see cref="BackColorIndex"/> properties should be set before adding the first frame,
    /// whereas <see cref="CompressionMode"/> can be changed before each frame. The <see cref="AddImage">AddImage</see> method allows specifying a location
    /// for each frame as well as an action to be performed after the delay interval of the corresponding frame is over.
    /// You can even write comments to the serialization stream by the <see cref="AddComments">AddComments</see> method.
    /// <note>When using the <see cref="AddImage">AddImage</see> method to add frames you should use already quantized images with indexed pixel format.
    /// Non-indexed images will be quantized using the default 8-bit "web-safe" palette without dithering.</note></para>
    /// </remarks>
    /// <example>
    /// <para>The following example demonstrates how to use the encoder in a <see langword="using"/> block:
    /// <code lang="C#"><![CDATA[
    /// using (var encoder = new GifEncoder(stream, new Size(48, 48)) { GlobalPalette = palette })
    /// {
    ///     encoder.AddComments("My GIF animation");
    ///     encoder.AddImage(frame1, location1, delay1);
    ///     encoder.AddImage(frame2, location2, delay2);
    /// }]]></code></para>
    /// <para>Or, by using fluent syntax the example above can be re-written like this:
    /// <code lang="C#"><![CDATA[
    /// // Note the last FinalizeEncoding step. In the above example it is called implicitly at the end of the using block.
    /// new GifEncoder(stream, new Size(48, 48)) { GlobalPalette = palette }
    ///     .AddComments("My GIF animation")
    ///     .AddImage(frame1, location1, delay1)
    ///     .AddImage(frame2, location2, delay2)
    ///     .FinalizeEncoding();]]></code></para>
    /// </example>
    public sealed partial class GifEncoder : IDisposable
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
        private const byte trailer = 0x3B;

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

        private bool isDisposed;
        private bool isInitialized;
        private byte backColorIndex;
        private int? repeatCount;
        private Palette? globalPalette;
        private GifCompressionMode compressionMode;
        private int imagesCount;

        #endregion

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the number of repetitions if creating an animation.
        /// Set a non-<see langword="null"/> value to add the <c>NETSCAPE2.0</c> extension to the stream and to indicate that added images
        /// should be interpreted as animation frames. Use <c>0</c> to loop the animation indefinitely.
        /// If <see langword="null"/>, and images are added with 0 delay, then GDI+ handles image as a multi-layer single frame image,
        /// though some application (including browsers) still may play them as individual frames.
        /// <br/>Default value: <see langword="null"/>.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is less than 0.</exception>
        /// <exception cref="InvalidOperationException">This property cannot be set after adding the first image.</exception>
        public int? RepeatCount
        {
            get => repeatCount;
            set
            {
                if (isInitialized)
                    throw new InvalidOperationException(Res.GifEncoderCannotChangeProperty);
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value), PublicResources.ArgumentOutOfRange);
                repeatCount = value;
            }
        }

        /// <summary>
        /// Gets or sets the global palette. If not set, then each added image will be stored along with their own palette.
        /// If not <see langword="null"/>, then the palette of the added images are stored only when they are different from the global palette.
        /// <br/>Default value: <see langword="null"/>.
        /// </summary>
        /// <exception cref="ArgumentException"><paramref name="value"/> has more than 256 colors.</exception>
        /// <exception cref="InvalidOperationException">This property cannot be set after adding the first image.</exception>
        public Palette? GlobalPalette
        {
            get => globalPalette;
            set
            {
                if (isInitialized)
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
        /// <exception cref="InvalidOperationException">This property cannot be set after adding the first image.</exception>
        public byte BackColorIndex
        {
            get => backColorIndex;
            set
            {
                if (isInitialized)
                    throw new InvalidOperationException(Res.GifEncoderCannotChangeProperty);
                backColorIndex = value;
            }
        }

        /// <summary>
        /// Gets or sets whether textual meta info should be added to the result stream.
        /// <br/>Default value: <see langword="false"/>.
        /// </summary>
        public bool AddMetaInfo { get; set; }

        /// <summary>
        /// Gets or sets the compression mode to be used when adding images by the <see cref="AddImage">AddImage</see> method.
        /// This property can be changed at any time.
        /// <br/>Default value: <see cref="GifCompressionMode.Auto"/>.
        /// </summary>
        public GifCompressionMode CompressionMode
        {
            get => compressionMode;
            set
            {
                if (!Enum<GifCompressionMode>.IsDefined(value))
                    throw new ArgumentOutOfRangeException(nameof(value), PublicResources.EnumOutOfRange(value));
                compressionMode = value;
            }
        }

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
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="GifEncoder"/> class for details and examples.
        /// </summary>
        /// <param name="imageData">The image data to write. Non-indexed images will be quantized by using the <see cref="GlobalPalette"/>, or, if that is not set,
        /// by the system default 8-bpp "web-safe" palette (see also <see cref="PredefinedColorsQuantizer.SystemDefault8BppPalette">PredefinedColorsQuantizer.SystemDefault8BppPalette</see>)
        /// using no dithering.</param>
        /// <param name="location">Specifies the location of the current image within the logical screen.</param>
        /// <param name="delay">Specifies the delay before rendering the next image in hundredths of a second. <c>0</c> is usually interpreted as 100ms by browsers (as if 10 was specified),
        /// while GDI+ treats it zero delay only if <see cref="RepeatCount"/> is <see langword="null"/>.</param>
        /// <param name="disposalMethod">Specifies how the decoder should treat the image after being displayed. This parameter is optional.
        /// <br/>Default value: <see cref="GifGraphicDisposalMethod.NotSpecified"/>.</param>
        /// <returns>The self <see cref="GifEncoder"/> instance allowing adding multiple images by fluent syntax.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public GifEncoder AddImage(IReadableBitmapData imageData, Point location = default, int delay = 0, GifGraphicDisposalMethod disposalMethod = GifGraphicDisposalMethod.NotSpecified)
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(GifEncoder), PublicResources.ObjectDisposed);
            if (imageData == null)
                throw new ArgumentNullException(nameof(imageData), PublicResources.ArgumentNull);
            if (location.X < 0 || location.Y < 0|| location.X + imageData.Width > logicalScreenSize.Width || location.Y + imageData.Height > logicalScreenSize.Height)
                throw new ArgumentOutOfRangeException(nameof(location), PublicResources.ArgumentOutOfRange);
            if (imageData.Width <= 0 || imageData.Height <= 0)
                throw new ArgumentException(PublicResources.ArgumentEmpty, nameof(imageData));
            if ((uint)delay > UInt16.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(delay), PublicResources.ArgumentMustBeBetween(UInt16.MinValue, UInt16.MaxValue));
            if ((uint)disposalMethod > (uint)GifGraphicDisposalMethod.RestoreToPrevious)
                throw new ArgumentOutOfRangeException(nameof(disposalMethod), PublicResources.EnumOutOfRange(disposalMethod));

            // Possible cloning with unlimited degree of parallelization is not an issue here.
            // When using the async static members the input imageData is always an indexed image.
            IReadableBitmapData actualImageData = imageData.Palette == null || imageData.Palette.Count > 256 ? imageData.Clone(KnownPixelFormat.Format8bppIndexed, globalPalette) : imageData;

            try
            {
                Debug.Assert(actualImageData.Palette != null);
                Palette? localPalette = actualImageData.Palette!;
                if (localPalette.EntriesEqual(globalPalette))
                    localPalette = null;

                Palette usedPalette = (localPalette ?? globalPalette)!;

                if (!isInitialized)
                    Initialize(Math.Max(actualImageData.PixelFormat.BitsPerPixel, usedPalette.Count.ToBitsPerPixel()));

                // Important: not using resources here because they could use non-ASCII characters or too long texts
                if (AddMetaInfo)
                    WriteCommentExtension($"Image #{imagesCount}: {actualImageData.Width}x{actualImageData.Height}",
                        $"Location: {location.X},{location.Y}",
                        $"Local Palette: {(localPalette == null ? "Not present" : $"{localPalette.Count} colors")}",
                        $"Disposal Method: {Enum<GifGraphicDisposalMethod>.ToString(disposalMethod)}",
                        $"Transparent Index: {(usedPalette.TransparentIndex >= 0 ? usedPalette.TransparentIndex : "Not set")}",
                        $"Delay: {delay}",
                        $"Compression Mode: {compressionMode}");

                // The GraphicControlExtension would be optional but always adding it to non-first images to provide better compatibility with decoders.
                // For example, the spec contains "The scope of this extension is the first graphic rendering block to follow";
                // still, many decoders (including GDI+ and many browsers) keep reusing the lastly specified values until they are changed.
                if (imagesCount > 0 || delay != 0 || disposalMethod != GifGraphicDisposalMethod.NotSpecified || usedPalette.TransparentIndex >= 0)
                    WriteGraphicControlExtension(delay, disposalMethod, usedPalette.TransparentIndex);

                WriteImageDescriptor(location, actualImageData.Size, localPalette);
                if (localPalette != null)
                    WritePalette(localPalette);

                WriteImageData(actualImageData);
                imagesCount += 1;
                return this;
            }
            finally
            {
                if (!ReferenceEquals(actualImageData, imageData))
                    actualImageData.Dispose();
            }
        }

        /// <summary>
        /// Writes textual comments to the output stream.
        /// </summary>
        /// <param name="comments">The comments to write. They must not be longer than 255 characters and must consist of ASCII characters.</param>
        /// <returns>The self <see cref="GifEncoder"/> instance allowing fluent syntax.</returns>
        /// <exception cref="ArgumentException"><paramref name="comments"/> contain a comment longer than 255 characters or that is not of ASCII characters only.</exception>
        public GifEncoder AddComments(params string?[]? comments)
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(GifEncoder), PublicResources.ObjectDisposed);
            if (comments == null || comments.Length == 0)
                return this;

            foreach (string? comment in comments)
            {
                if (String.IsNullOrEmpty(comment))
                    continue;

                if (comment!.Length > 255)
                    throw new ArgumentException(Res.GifEncoderCommentTooLong, nameof(comments));
                if (comment.Any(c => c >= 128))
                    throw new ArgumentException(Res.GifEncoderCommentNotAscii, nameof(comments));
            }

            if (!isInitialized)
                Initialize(globalPalette?.Count.ToBitsPerPixel() ?? 8);

            WriteCommentExtension(comments);
            return this;
        }

        /// <summary>
        /// Finalizes the encoding. It should be called after adding the last image.
        /// It is implicitly called when this <see cref="GifEncoder"/> instance is disposed.
        /// </summary>
        /// <param name="leaveStreamOpen"><see langword="true"/> to leave the underlying stream open; otherwise, <see langword="false"/>. This parameter is optional.
        /// <br/>Default value: <see langword="true"/>.</param>
        public void FinalizeEncoding(bool leaveStreamOpen = true)
        {
            if (isDisposed)
                return;

            // Important: not using resources here because they could use non-ASCII characters or too long texts
            if (isInitialized && AddMetaInfo)
                WriteCommentExtension($"Images Count: {imagesCount}");

            isDisposed = true;
            globalPalette = null;
            if (isInitialized)
                writer.Write(trailer);
            if (leaveStreamOpen)
                writer.Flush();
            else
                writer.Close();
        }

        #endregion

        #region Private Methods

        private void Initialize(int bpp)
        {
            Debug.Assert(!isInitialized);
            isInitialized = true;
            WriteHeaderAndLogicalScreenDescriptor(bpp);
            if (globalPalette != null)
                WritePalette(globalPalette);

            WriteCommentExtension(signatureBlock);
            if (AddMetaInfo)
            {
                // Important: not using resources here because they could use non-ASCII characters or too long texts
                WriteCommentExtension($"Logical Screen Size: {logicalScreenSize.Width}x{logicalScreenSize.Height}",
                    $"Global Palette: {(globalPalette == null ? "Not present" : $"{globalPalette.Count} colors")}",
                    $"Color Resolution: {bpp}bpp",
                    $"Background Color Index: {(backColorIndex < globalPalette?.Count ? backColorIndex : 0)}",
                    $"Repeat Count: {repeatCount switch { null => "Not present", 0 => "Infinite", _ => repeatCount }}");
            }

            if (repeatCount.HasValue)
                WriteNetscapeLoopBlockApplicationExtension(repeatCount.Value);
        }

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

            // filling up the rest of the colors but at least 2 (GIF supports log2 palette sizes)
            int maxColors = Math.Max(2,  ((uint)palette.Count).RoundUpToPowerOf2());
            for (int i = palette.Count; i < maxColors; i++)
                writer.Write(emptyColor);
        }

        /// <summary>
        /// Writing the comment extension from direct data as per chapters 24 and 15 in https://www.w3.org/Graphics/GIF/spec-gif89a.txt
        /// </summary>
        private void WriteCommentExtension(byte[] buffer)
        {
            writer.Write(extensionIntroducer);
            writer.Write(commentLabel);

            // Comment sub block
            writer.Write((byte)buffer.Length);
            writer.Write(buffer);

            writer.Write(blockTerminator);

            #region Local Methods

            #endregion
        }

        /// <summary>
        /// Writing the comment extension from strings as per chapters 24 and 15 in https://www.w3.org/Graphics/GIF/spec-gif89a.txt
        /// </summary>
        private void WriteCommentExtension(params string?[] comments)
        {
            writer.Write(extensionIntroducer);
            writer.Write(commentLabel);

            // Comment sub blocks
            foreach (string? comment in comments)
            {
                if (String.IsNullOrEmpty(comment))
                    continue;

                writer.Write((byte)comment!.Length);

                // ReSharper disable once ForCanBeConvertedToForeach - performance
                for (int i = 0; i < comment.Length; i++)
                    writer.Write((byte)comment[i]);
            }

            writer.Write(blockTerminator);
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

        private void WriteImageData(IReadableBitmapData imageData) => LzwEncoder.Encode(imageData, writer, compressionMode);

        #endregion

        #region Explicitly Implemented Interface Methods

        void IDisposable.Dispose() => FinalizeEncoding();

        #endregion

        #endregion

        #endregion
    }
}
