#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: RawIcon.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2026 - All Rights Reserved
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
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
#if NET7_0_OR_GREATER
using System.Runtime.Versioning;
#endif
using System.Security;

using KGySoft.CoreLibraries;
using KGySoft.Drawing.Imaging;
using KGySoft.Drawing.WinApi;
using KGySoft.Serialization.Binary;

#endregion

namespace KGySoft.Drawing
{
    /// <summary>
    /// Provides low-level support for an icon. This class is internal because it can process and produce <see cref="Icon"/> and <see cref="Bitmap"/>
    /// instances and every functionality is accessible via extensions for those classes.
    /// </summary>
    [SecuritySafeCritical]
#if NET7_0_OR_GREATER
    [SupportedOSPlatform("windows")]
#endif
    internal sealed class RawIcon : IDisposable
    {
        #region Nested classes

        #region RawIconImageCollection class

        private sealed class RawIconImageCollection : Collection<RawIconImage>, IDisposable
        {
            #region Methods

            #region Public Methods

            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
            {
                foreach (RawIconImage image in this)
                    image.Dispose();

                Clear();
            }

            #endregion

            #region Protected Methods

            /// <summary>
            /// Inserts an element into the <see cref="T:System.Collections.ObjectModel.Collection`1"></see> at the specified index.
            /// </summary>
            /// <param name="index">The zero-based index at which item should be inserted.</param>
            /// <param name="item">The object to insert. The value can be null for reference types.</param>
            /// <exception cref="System.NotSupportedException">There are too many images in the icon collection</exception>
            protected override void InsertItem(int index, RawIconImage item)
            {
                if (Count == UInt16.MaxValue)
                    throw new NotSupportedException(DrawingRes.RawIconTooManyImages);
                base.InsertItem(index, item ?? throw new ArgumentNullException(nameof(item), PublicResources.ArgumentNull));
            }

            #endregion

            #endregion
        }

        #endregion

        #region RawIconImage class

        private sealed class RawIconImage : IDisposable
        {
            #region Fields

            private readonly Color32 transparentColor;

            private Size size;
            private int bpp;
            private HashSet<int>? transparentIndices;

            /// <summary>
            /// Gets whether <see cref="rawColor"/> contains PNG data.
            /// </summary>
            private bool isPng;

            /// <summary>
            /// In: Source image if it is already 32 bit ARGB and Color.Transparent is specified for transparency
            /// Out: Result image of ToBitmap(false)
            /// </summary>
            private Bitmap? bmpComposite;

            /// <summary>
            /// In: Source image if it is non ARGB or when a custom transparent color is specified
            /// Out: Result image of ToBitmap(true)
            /// </summary>
            private Bitmap? bmpColor;

            /// <summary>
            /// Color image or the raw image itself when PNG
            /// </summary>
            private byte[]? rawColor;

            /// <summary>
            /// Mask data (can be null in an icon file even if BMP, though we always generate it). 0: opacity, 1: transparency.
            /// </summary>
            private byte[]? rawMask;

            /// <summary>
            /// Header (only if BMP)
            /// </summary>
            private BITMAPINFOHEADER bmpHeader;

            /// <summary>
            /// Palette (only if indexed BMP)
            /// </summary>
            private RGBQUAD[]? palette;

            #endregion

            #region Properties

            #region Internal Properties

            internal int Bpp => bpp;

            /// <summary>
            /// Gets the size in pixels
            /// </summary>
            internal Size Size => size;

            internal bool IsCompressed => isPng;

            #endregion

            #region Private Properties

            private int PaletteColorCount
            {
                get
                {
                    Debug.Assert(!isPng, "Color count requested for PNG");

                    // from raw data: returning actual size of the palette
                    if (bmpHeader.biSize != 0U)
                    {
                        return (int)(bmpHeader.biClrUsed != 0
                                ? bmpHeader.biClrUsed
                                : bmpHeader.biBitCount <= 8 ? (uint)(1 << bmpHeader.biBitCount) : 0);
                    }

                    // from source image: when image is indexed, always the maximum palette number will be generated without optimization
                    return bpp > 8 ? 0 : 1 << bpp;
                }
            }

            private bool IsPngPreferred => bpp >= 32 && (size.Width >= MinCompressedSize || size.Height >= MinCompressedSize);

            #endregion

            #endregion

            #region Constructors

            /// <summary>
            /// From bitmap
            /// </summary>
            internal RawIconImage(Bitmap bmpColor, Color transparentColor)
            {
                // bitmaps are clones so they can be disposed with the RawIconImage instance
                if (bmpColor.PixelFormat == PixelFormat.Format32bppArgb && transparentColor.A == 0)
                {
                    bmpComposite = bmpColor;
                    this.transparentColor = default;
                }
                else
                {
                    this.bmpColor = bmpColor;
                    this.transparentColor = new Color32(transparentColor);
                }

                InitFromBitmap(bmpColor);
            }

            /// <summary>
            /// From raw data
            /// </summary>
            internal RawIconImage(byte[] rawData)
            {
                #region Local Methods

                bool TryInitFromPng()
                {
                    // PNG header: 0x89+"PNG"
                    const int signature = 0x474E5089;
                    if (rawData.Length < 4 || BitConverter.ToInt32(rawData, 0) != signature)
                        return false;

                    isPng = true;
                    rawColor = rawData;

                    // byte 24: bpp per channel
                    // Note: When re-saving from generated data always 32 BPP is used for PNG images because even 24 BPP PNG
                    // bitmaps are not really supported by Windows (neither in Explorer nor by apps) and because transparency would be lost otherwise.
                    bpp = rawData[24];
                    switch (rawData[25])
                    {
                        case 2: // RGB
                            bpp *= 3;
                            break;
                        case 4: // Alpha Grayscale
                            bpp <<= 1;
                            break;
                        case 6: // ARGB
                            bpp <<= 2;
                            break;
                    }

                    // size is at 16 and 20 DWORD big endian so reading last 2 bytes only
                    size = new Size((rawData[18] << 8) + rawData[19], (rawData[22] << 8) + rawData[23]);
                    return true;
                }

                bool TryInitFromBitmap()
                {
                    int headerSize;
                    unsafe { headerSize = sizeof(BITMAPINFOHEADER); }

                    // BMP header: size of the BITMAPINFOHEADER structure
                    if (rawData.Length < 4 || BitConverter.ToInt32(rawData, 0) != headerSize)
                        return false;

                    // header
                    bmpHeader = BinarySerializer.DeserializeValueType<BITMAPINFOHEADER>(rawData);
                    size = new Size(bmpHeader.biWidth, bmpHeader.biHeight >> 1); // height is doubled because of mask
                    bpp = bmpHeader.biBitCount;
                    int offset = headerSize;

                    // palette
                    int colorCount = PaletteColorCount;
                    int colorSize;
                    unsafe { colorSize = sizeof(RGBQUAD); }
                    if (colorCount > 0)
                    {
                        palette = BinarySerializer.DeserializeValueArray<RGBQUAD>(rawData, offset, colorCount);
                        offset += colorSize * palette.Length;
                    }

                    int strideColor = ((bmpHeader.biWidth * bmpHeader.biBitCount + 31) & ~31) >> 3;
                    int rawColorLength = strideColor * size.Height;
                    int strideMask = ((bmpHeader.biWidth + 31) & ~31) >> 3;
                    int maskLength = strideMask * size.Height;

                    // unexpected length
                    if (rawData.Length < offset + rawColorLength)
                        return false;

                    // color image (XOR)
                    rawColor = new byte[strideColor * size.Height];
                    Buffer.BlockCopy(rawData, offset, rawColor, 0, rawColor.Length);

                    offset += rawColor.Length;

                    // mask image (AND)
                    if (offset + maskLength > rawData.Length)
                    {
                        // the mask is sometimes omitted for 32bpp images, but we still generate it for best compatibility
                        GenerateMask(strideColor, strideMask);
                        return true;
                    }

                    rawMask = new byte[maskLength];
                    Buffer.BlockCopy(rawData, offset, rawMask, 0, maskLength);
                    return true;
                }

                #endregion

                if (!TryInitFromPng() && !TryInitFromBitmap())
                    throw new ArgumentException(DrawingRes.RawIconBadIconFormat, nameof(rawData));
            }

            #endregion

            #region Methods

            #region Public Methods

            public void Dispose()
            {
                bmpColor?.Dispose();
                bmpComposite?.Dispose();
                bmpColor = null;
                bmpComposite = null;
                rawColor = null;
                rawMask = null;
                palette = null;
            }

            #endregion

            #region Internal Methods

            internal unsafe void WriteDirEntry(BinaryWriter bw, bool forceBmpFormat, ref uint offset)
            {
                AssureRawFormatGenerated(forceBmpFormat);
                ICONDIRENTRY entry = new ICONDIRENTRY
                {
                    dwImageOffset = offset,
                };

                if (isPng)
                {
                    // For PNGs the header entry size is always 0
                    entry.wPlanes = 1;
                    entry.wBitCount = (ushort)bpp;
                    entry.dwBytesInRes = (uint)rawColor!.Length;
                }
                else
                {
                    // For BMPs the header entry size is set if smaller than 256x256
                    entry.bWidth = size.Width > Byte.MaxValue ? (byte)0 : (byte)size.Width;
                    entry.bHeight = size.Height > Byte.MaxValue ? (byte)0 : (byte)size.Height;
                    entry.bColorCount = (byte)bmpHeader.biClrUsed;
                    entry.wPlanes = bmpHeader.biPlanes;
                    entry.wBitCount = bmpHeader.biBitCount;
                    entry.dwBytesInRes = (uint)(sizeof(BITMAPINFOHEADER) + sizeof(RGBQUAD) * PaletteColorCount +
                        rawColor!.Length + rawMask!.Length);
                }

                bw.Write(BinarySerializer.SerializeValueType(entry));
                offset += entry.dwBytesInRes;
            }

            internal void WriteRawImage(BinaryWriter bw, bool forceBmpFormat)
            {
                AssureRawFormatGenerated(forceBmpFormat);
                if (isPng)
                {
                    bw.Write(rawColor!);
                    return;
                }

                // header
                bw.Write(BinarySerializer.SerializeValueType(bmpHeader));

                // Palette
                if (PaletteColorCount > 0)
                    bw.Write(BinarySerializer.SerializeValueArray(palette!));

                // color image (XOR)
                bw.Write(rawColor!);

                // mask image (AND)
                bw.Write(rawMask!);
            }

            // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
            internal Icon? ToIcon(bool forceBmpFormat, bool throwError)
            {
                using var ms = new MemoryStream();
                Save(ms, forceBmpFormat);

                // returning icon
                ms.Position = 0L;
                try
                {
                    return new Icon(ms);
                }
                catch (Exception)
                {
                    if (OSUtils.IsVistaOrLater && !OSUtils.IsMono)
                        throw;
                    // Wine/Mono supports uncompressed large icons, but Framework Mono cannot handle an icon with a single large image (neither PNG nor BMP).
                    if (throwError)
                        throw new PlatformNotSupportedException(DrawingRes.RawIconCannotBeInstantiatedAsIcon);
                    return null;
                }
            }

            internal Bitmap ToBitmap(bool keepOriginalFormat)
            {
                AssureBitmapsGenerated(!keepOriginalFormat);
                Debug.Assert(keepOriginalFormat && bmpColor != null || !keepOriginalFormat && bmpComposite != null, "Generated bitmaps are expected here");
                if (keepOriginalFormat)
                    return bmpColor!.CloneBitmap();

                // When not original format is requested, making sure that a MemoryBMP is returned,
                // because e.g. PNG images may cause troubles in some cases (e.g. OutOfMemoryException when used as a background image)
                if (bmpComposite!.RawFormat.Guid != ImageFormat.MemoryBmp.Guid)
                    return new Bitmap(bmpComposite);

                // Cloning by Bitmap.Clone(Rectangle, PixelFormat) instead of Image.Clone because the latter may return a blank result on Linux
                return bmpComposite.CloneBitmap();
            }

            internal IconInfo GetIconInfo()
            {
                var result = new IconInfo
                {
                    Size = size,
                    BitsPerPixel = bpp,
                    IsCompressed = isPng
                };

                if (bpp <= 8)
                {
                    if (palette != null)
                        result.Palette = palette.Select(c => c.ToColor()).ToArray();
                    else
                    {
                        Debug.Assert(isPng, "Palette should exist for non-PNG image here");
                        AssureBitmapsGenerated(false);
                        Debug.Assert(bmpColor != null && bmpColor.GetBitsPerPixel() == bpp);
                        result.Palette = bmpColor!.Palette.Entries;
                    }
                }

                return result;
            }

            #endregion

            #region Private Methods

            private void InitFromBitmap(Bitmap bitmap)
            {
                if (bitmap.PixelFormat.IsIndexed())
                {
                    transparentIndices = new HashSet<int>();
                    Color[] entries = bitmap.Palette.Entries;
                    for (int i = 0; i < entries.Length; i++)
                    {
                        ref Color c = ref entries[i];
                        if (c.ToArgb() == transparentColor.ToArgb() || c.A == 0 && transparentColor.A == 0)
                            transparentIndices.Add(i);
                    }
                }
                else
                    transparentIndices = null;

                size = bitmap.Size;
                bpp = bitmap.GetBitsPerPixel();
            }

            private unsafe void Save(Stream stream, bool forceBmpFormat)
            {
                var bw = new BinaryWriter(stream);

                // header
                var iconDir = new ICONDIR
                {
                    idReserved = 0,
                    idType = 1,
                    idCount = 1
                };

                bw.Write(BinarySerializer.SerializeValueType(iconDir));

                // Icon entry
                uint offset = (uint)(sizeof(ICONDIR) + sizeof(ICONDIRENTRY));
                WriteDirEntry(bw, forceBmpFormat, ref offset);

                // Icon image
                WriteRawImage(bw, forceBmpFormat);
            }

            private unsafe void AssureRawFormatGenerated(bool forceBmpFormat)
            {
                if (rawColor == null && bmpColor == null && bmpComposite == null)
                    throw new ObjectDisposedException(null, PublicResources.ObjectDisposed);

                // Exiting, if raw data of the desired format is already generated.
                // This is the only case when we allow low-res/BPP PNGs (so already existing icons are not converted unless forced).
                if (rawColor != null && !(forceBmpFormat && isPng))
                    return;

                Bitmap bmp;

                // if there is only a raw PNG we create the bitmap from it to create the raw format from
                if (forceBmpFormat && bmpColor == null && bmpComposite == null && isPng)
                {
                    // rawColor cannot be null here, see the first check
                    bmp = new Bitmap(new MemoryStream(rawColor!));
                    if (!bmp.PixelFormat.In(validIconFormats))
                    {
                        // not very likely that we reach this point, at least on Windows PNG decoder does not return 16/48/64 BPP formats...
                        bmpColor = bmp.ConvertPixelFormat(PixelFormat.Format32bppArgb);
                        bmp.Dispose();
                    }
                    else
                        bmpColor = bmp;

                    InitFromBitmap(bmpColor);
                }

                bmp = bmpColor ?? bmpComposite!;

                // When (re)generating we allow PNG only for 32 BPP formats. Even Windows does not support 24 BPP PNG icons correctly.
                isPng = !forceBmpFormat && IsPngPreferred;
                if (isPng)
                {
                    using var ms = new MemoryStream();

                    // When PNG, using composite image in the first place
                    bmp.Save(ms, ImageFormat.Png);
                    rawColor = ms.ToArray();
                    rawMask = null;
                    return;
                }

                // palette
                if (bpp <= 8)
                {
                    // generating the maximum number of palette entries without optimization
                    // (so PaletteColorCount can return number of colors before generating the palette)
                    palette = new RGBQUAD[1 << bpp];
                    Color[] entries = bmp.Palette.Entries;
                    for (int i = 0; i < entries.Length; i++)
                        palette[i] = new RGBQUAD(entries[i]);
                }

                // header
                bmpHeader.biSize = (uint)sizeof(BITMAPINFOHEADER);
                bmpHeader.biWidth = size.Width;
                bmpHeader.biHeight = size.Height << 1; // because of mask, should be specified as double height image
                bmpHeader.biPlanes = 1;
                bmpHeader.biBitCount = (ushort)bmp.GetBitsPerPixel();
                bmpHeader.biCompression = BitmapCompressionMode.BI_RGB;
                bmpHeader.biXPelsPerMeter = 0;
                bmpHeader.biYPelsPerMeter = 0;
                bmpHeader.biClrUsed = (uint)PaletteColorCount;
                bmpHeader.biClrImportant = 0;

                // Color image (XOR): copying from input bitmap while flipping the image vertically.
                // We could use bmp.GetReadableBitmapData, but this way we can invert the original stride to produce a bottom-up result by a simple CopyTo.
                int strideColor;
                BitmapData dataColor = bmp.LockBits(new Rectangle(Point.Empty, bmp.Size), ImageLockMode.ReadOnly, bmp.PixelFormat);
                try
                {
                    strideColor = dataColor.Stride;
                    rawColor = new byte[Math.Abs(strideColor) * dataColor.Height];
                    bmpHeader.biSizeImage = (uint)rawColor.Length;

                    var startAddress = new IntPtr(dataColor.Scan0.ToInt64() + (dataColor.Height - 1) * strideColor);
                    strideColor = -strideColor; // this works both for positive and negative original stride, though negative is not really expected after cloning in Add
                    using IReadableBitmapData bitmapDataSrc = BitmapDataFactory.CreateBitmapData(startAddress, size, strideColor, dataColor.PixelFormat.ToKnownPixelFormatInternal());
                    strideColor = Math.Abs(strideColor); // the stride of our managed buffer is always positive
                    using IWritableBitmapData bitmapDataDst = BitmapDataFactory.CreateBitmapData(rawColor, size, strideColor, bitmapDataSrc.PixelFormat.AsKnownPixelFormatInternal);
                    bitmapDataSrc.CopyTo(bitmapDataDst); // as pixel formats and the palette (which we don't even specify here) are the same, this will perform a raw copy
                }
                finally
                {
                    bmp.UnlockBits(dataColor);
                }

                // Mask image (AND): Creating from color image and provided transparent color.
                int strideMask = ((size.Width + 31) >> 5) << 2; // Stride = 4 * (Width + 31) / 32)
                GenerateMask(strideColor, strideMask);
            }

            private void GenerateMask(int strideColor, int strideMask)
            {
                // rawColor now contains the provided bitmap data with the original background, while rawMask is still null.
                Debug.Assert(rawColor != null && rawMask == null);
                rawMask = new byte[strideMask * size.Height];

                // we know that the icon will not be transparent: returning a solid mask (note: we create it even for 32bpp images for best compatibility)
                if (bpp <= 8 && transparentIndices.IsNullOrEmpty() || bpp == 24 && transparentColor.A != Byte.MaxValue)
                    return;

                // Reinterpreting rawColor and rawMask bytes as bitmaps. Omitting palette initialization, because we only check the palette indices for indexed formats.
                using IReadableBitmapData bmpDataColor = BitmapDataFactory.CreateBitmapData(rawColor!, size, strideColor, bpp.ToPixelFormat().ToKnownPixelFormatInternal());
                using IWritableBitmapData bmpDataMask = BitmapDataFactory.CreateBitmapData(rawMask, size, strideMask, KnownPixelFormat.Format1bppIndexed);

                // We could use Combine (like in GenerateCompositeBitmap) for >= 24 bpp formats, but this way we can handle all formats together
                // (and setting the mask by color index rather than by Color32 is a bit more effective).
                var rowColor = (IBitmapDataRowInternal)bmpDataColor.FirstRow;
                var rowMask = (IBitmapDataRowInternal)bmpDataMask.FirstRow;
                do
                {
                    for (int x = 0; x < size.Width; x++)
                    {
                        bool isTransparent = bpp switch
                        {
                            32 => rowColor.DoGetColor32(x) is Color32 c && (c == transparentColor || transparentColor.A == 0 && c.A < 128),
                            24 => rowColor.DoGetColor32(x) == transparentColor,
                            _ => transparentIndices!.Contains(rowColor.DoGetColorIndex(x))
                        };

                        if (isTransparent)
                            rowMask.DoSetColorIndex(x, 1);
                    }
                } while (rowColor.MoveNextRow() && rowMask.MoveNextRow());
            }

            private void AssureBitmapsGenerated(bool isCompositeRequired)
            {
                if (rawColor == null && bmpColor == null && bmpComposite == null)
                    throw new ObjectDisposedException(null, PublicResources.ObjectDisposed);

                // exiting, if the requested bitmap already exists
                if (isCompositeRequired && bmpComposite != null || !isCompositeRequired && bmpColor != null)
                    return;

                // PNG format
                if (isPng)
                {
                    AssurePngBitmapsGenerated(isCompositeRequired);
                    return;
                }

                // Working from raw format. If it doesn't exist, creating from composite image, forcing BMP format (PNG can be regenerated on next save)
                if (rawColor == null)
                    AssureRawFormatGenerated(true);

                // BMP format - composite image
                if (isCompositeRequired)
                    GenerateCompositeBitmap();
                else
                    GenerateColorBitmap();
            }

            private void GenerateColorBitmap()
            {
                Debug.Assert(bmpColor == null, "Unnecessary GenerateColorBitmap call");
                Debug.Assert(rawColor != null, "AssureRawFormatGenerated was not called");
                PixelFormat pixelFormat = bpp.ToPixelFormat();

                // reinterpreting rawColor as a bitmap
                int stride = ((size.Width * bpp + 31) & ~31) >> 3;
                using IReadableBitmapData bmpDataSrc = BitmapDataFactory.CreateBitmapData(rawColor!, size, stride, pixelFormat.ToKnownPixelFormatInternal());

                // We could simply return bmpDataSrc.ToBitmap() here (after specifying a palette as well), but we must flip the image vertically, so creating the result manually.
                var result = new Bitmap(size.Width, size.Height, pixelFormat);
                BitmapData bitmapDataColor = result.LockBits(new Rectangle(Point.Empty, size), ImageLockMode.WriteOnly, pixelFormat);
                try
                {
                    // Instead of using result.GetWritableBitmapData, we use the factory here as well, so we can invert the stride. This way a simple CopyTo flips the image for us.
                    var startAddress = new IntPtr(bitmapDataColor.Scan0.ToInt64() + (size.Height - 1) * bitmapDataColor.Stride);
                    using IWritableBitmapData bmpDataDst = BitmapDataFactory.CreateBitmapData(startAddress, size, -bitmapDataColor.Stride, pixelFormat.ToKnownPixelFormatInternal());
                    bmpDataSrc.CopyTo(bmpDataDst); // as pixel formats and the palette (which we didn't even specify yet) are the same, this will perform a raw copy
                }
                finally
                {
                    result.UnlockBits(bitmapDataColor);
                }

                // Trick: we set the palette only after the copying. Otherwise, we must have set it both for the source and the target to make CopyTo work correctly.
                if (PaletteColorCount > 0)
                {
                    ColorPalette resultPalette = result.Palette;
                    Color[] colors = resultPalette.Entries;
                    for (int i = 0; i < palette!.Length; i++)
                        colors[i] = palette[i].ToColor();

                    // we must reassign it to take effect
                    result.Palette = resultPalette;
                }

                bmpColor = result;
            }

            private void GenerateCompositeBitmap()
            {
                Debug.Assert(bmpComposite == null, "Unnecessary GenerateCompositeBitmap call");
                Debug.Assert(rawColor != null && rawMask != null, "AssureRawFormatGenerated was not called");
                KnownPixelFormat pixelFormat = bpp.ToPixelFormat().ToKnownPixelFormatInternal();

                // Reinterpreting rawColor (XOR) and rawMask (AND) bytes as bitmaps, so we can easily combine them into an actual alpha bitmap.
                int strideColor = ((size.Width * bpp + 31) & ~31) >> 3;
                Palette? paletteColor = palette == null ? null : new Palette(palette.Select(c => c.ToColor()));
                using IReadableBitmapData bmpDataColor = BitmapDataFactory.CreateBitmapData(rawColor!, size, strideColor, pixelFormat, paletteColor);
                int strideMask = ((size.Width + 31) & ~31) >> 3;
                using IReadableBitmapData bmpDataMask = BitmapDataFactory.CreateBitmapData(rawMask!, size, strideMask, KnownPixelFormat.Format1bppIndexed);
                var result = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppArgb);
                BitmapData resultBitmapData = result.LockBits(new Rectangle(Point.Empty, size), ImageLockMode.WriteOnly, result.PixelFormat);
                try
                {
                    // Special handling for 32 bpp icons: we ignore the mask, unless it indicates transparency for totally opaque pixels.
                    // Mask 0 (opacity) and 1 (transparency) bits are mapped to the black and white colors of the default 1 bpp palette.
                    Func<Color32, Color32, Color32> combineFunction = bpp == 32 && transparentColor.A == 0
                        ? (color, mask) => color.A != 0 && (color.A < Byte.MaxValue || mask == Color32.Black) ? color : default
                        : (color, mask) => mask == Color32.Black ? color : default;

                    // NOTE: The sources are bottom-up, whereas the result bitmap is top-bottom. But negative strides can be used for unmanaged buffers only,
                    //       so inverting it for the result rather than the sources. This way a simple Combine call will also flip the result vertically while applying the mask.
                    //       If we didn't need to do the flip, we simply could use result.GetWritableBitmapData without Un/LockBits.
                    var startAddress = new IntPtr(resultBitmapData.Scan0.ToInt64() + (size.Height - 1) * resultBitmapData.Stride);
                    using IWritableBitmapData bmpDataComposite = BitmapDataFactory.CreateBitmapData(startAddress, size, -resultBitmapData.Stride, KnownPixelFormat.Format32bppArgb);
                    bmpDataColor.Combine(bmpDataMask, bmpDataComposite, combineFunction);
                }
                finally
                {
                    result.UnlockBits(resultBitmapData);
                }

                bmpComposite = result;
            }

            private void AssurePngBitmapsGenerated(bool isCompositeRequired)
            {
                Debug.Assert(isPng && rawColor != null);

                // Note: MemoryStream must not be in a using because that would kill the new bitmap.
                var result = new Bitmap(new MemoryStream(rawColor!));

                if (isCompositeRequired)
                {
                    Debug.Assert(bmpComposite == null, "Calling generate was not necessary");
                    if (result.PixelFormat == PixelFormat.Format32bppArgb)
                    {
                        bmpComposite = result;
                        return;
                    }

                    // If the decoder returned a non-alpha or indexed PNG
                    bmpComposite = result.ConvertPixelFormat(PixelFormat.Format32bppArgb);
                    result.Dispose();
                    return;
                }

                // if PNG bpp matches the required result
                Debug.Assert(bmpColor == null, "Calling generate was not necessary");
                if (bpp == result.GetBitsPerPixel())
                {
                    bmpColor = result;
                    return;
                }

                // generating a lower bpp image
                // note: this code theoretically executes only for indexed PNG if the decoder restored it with higher BPP
                Color[]? paletteBmpColor = null;
                if (bpp <= 8)
                    paletteBmpColor = result.GetColors(1 << bpp);

                bmpColor = result.ConvertPixelFormat(bpp.ToPixelFormat(), paletteBmpColor);
                result.Dispose();
            }

            #endregion

            #endregion
        }

        #endregion

        #endregion

        #region Constants

        /// <summary>
        /// The minimum size for compressed icons. If smaller than 256, then loading the icon as Bitmap fails even on Windows 10,
        /// though in Explorer the icons with sizes 49-255 look better with PNG compression.
        /// </summary>
        internal const int MinCompressedSize = 256;

        #endregion

        #region Fields

        #region Static Fields

        private static readonly PixelFormat[] validIconFormats =
        [
            PixelFormat.Format1bppIndexed,
            PixelFormat.Format4bppIndexed,
            PixelFormat.Format8bppIndexed,
            PixelFormat.Format24bppRgb,
            PixelFormat.Format32bppArgb
        ];

        #endregion

        #region Instance Fields

        private readonly RawIconImageCollection iconImages = new RawIconImageCollection();

        #endregion

        #endregion

        #region Properties

        internal int ImageCount => iconImages.Count;
        internal bool IsCompressed => iconImages.Any(img => img.IsCompressed);
        internal int Bpp => iconImages.Max(img => img.Bpp);

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RawIcon"/> class.
        /// </summary>
        internal RawIcon()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RawIcon"/> class from an <see cref="Icon"/>.
        /// </summary>
        internal RawIcon(Icon icon, Size? size = null, int? bpp = null, int? index = null)
        {
            if (icon == null)
                throw new ArgumentNullException(nameof(icon), PublicResources.ArgumentNull);

            // there is no icon stream - adding by bitmap
            if (!icon.HasRawData())
            {
                if (index.HasValue && index.Value != 0 || size.HasValue && size.Value != icon.Size)
                    return;

                using Bitmap bmp = icon.ToAlphaBitmap();
                if (!bpp.HasValue || bpp.Value == bmp.GetBitsPerPixel())
                    Add(bmp);
                return;
            }

            // initializing from stream
            using var ms = new MemoryStream();
            icon.Save(ms);
            ms.Position = 0L;

            using var br = new BinaryReader(ms);
            Load(br, size, bpp, index);
        }

        internal RawIcon(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream), PublicResources.ArgumentNull);
            Load(new BinaryReader(stream), null, null, null);
        }

        #endregion

        #region Methods

        #region Public Methods

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose() => iconImages.Dispose();

        #endregion

        #region Internal Methods

        /// <summary>
        /// Adds an icon to the raw icon. <param name="icon"> is deserialized from stream.</param>
        /// </summary>
        internal void Add(Icon icon)
        {
            if (icon == null)
                throw new ArgumentNullException(nameof(icon), PublicResources.ArgumentNull);

            // not in using so its images will not be disposed after adding them to self images
            foreach (RawIconImage image in new RawIcon(icon).iconImages)
                iconImages.Add(image);
        }

        /// <summary>
        /// Adds an image to the raw icon. If it contains icons, all images are added.
        /// </summary>
        internal void Add(Bitmap image)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image), PublicResources.ArgumentNull);

            Add(image, Color.Transparent);
        }

        /// <summary>
        /// Adds an image to the raw icon. If it contains icons, all images are added.
        /// </summary>
        internal void Add(Bitmap image, Color transparentColor)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image), PublicResources.ArgumentNull);

            PixelFormat pixelFormat = image.PixelFormat;
            Bitmap[] bitmaps;

            if (!pixelFormat.In(validIconFormats))
                bitmaps = [image.ConvertPixelFormat(PixelFormat.Format32bppArgb)];
            else if (image.RawFormat.Guid == ImageFormat.Icon.Guid)
                bitmaps = image.ExtractIconImages();
            else
                // Image.Clone() could result in a blank Bitmap on Linux if its content was drawn by Graphics
                bitmaps = [image.CloneBitmap()];

            foreach (Bitmap bitmap in bitmaps)
                iconImages.Add(new RawIconImage(bitmap, transparentColor));
        }

        /// <summary>
        /// Gets the icons of the <see cref="RawIcon"/> instance as a single, combined <see cref="Icon"/>.
        /// </summary>
        internal Icon? ToIcon(bool forceBmpImages)
        {
            if (iconImages.Count == 0)
                return null;

            using MemoryStream ms = new MemoryStream();
            using BinaryWriter bw = new BinaryWriter(ms);
            Save(bw, forceBmpImages);
            ms.Position = 0L;
            try
            {
                return new Icon(ms);
            }
            catch (Exception e)
            {
                if (!OSUtils.IsVistaOrLater || OSUtils.IsMono)
                    throw new PlatformNotSupportedException(DrawingRes.RawIconCannotBeInstantiatedAsIcon, e);
                throw;
            }
        }

        /// <summary>
        /// Gets a <see cref="Bitmap"/> instance, which contains every image of the <see cref="RawIcon"/> instance as a single, multi-resolution <see cref="Bitmap"/>.
        /// On platforms where multi-resolutions bitmaps are not supported, the largest bitmap is returned.
        /// </summary>
        internal Bitmap ToBitmap()
        {
            if (iconImages.Count == 0)
                throw new InvalidOperationException(DrawingRes.RawIconEmpty);

            // For the best compatibility, returning pure 32 bpp uncompressed images.
            // For example, Windows XP does not support PNG images, and a 24 bpp icon may cause a "Parameter is invalid" error in Bitmap ctor.
            try
            {
                var ms = new MemoryStream();
                var bw = new BinaryWriter(ms);

                // Simple case: just save and return
                if (iconImages.All(i => i.Bpp == 32))
                {
                    // not in using because stream must be left open during the Bitmap lifetime
                    Save(bw, true);
                    ms.Position = 0L;
                    return new Bitmap(ms);
                }

                // Mixed color depths: taking the first highest bpp image of all sizes
                using var result = new RawIcon();
                foreach (Size size in iconImages.Select(i => i.Size).Distinct())
                {
                    RawIconImage image = GetNearestImage(32, size, true);
                    if (image.Bpp == 32)
                        result.Add(image);
                    else
                        result.Add(image.ToBitmap(false));
                }

                result.Save(bw, true);
                ms.Position = 0L;
                return new Bitmap(ms);
            }
            catch (Exception e) when (!e.IsCriticalGdi())
            {
                return ExtractNearestBitmap(32, new Size(Int32.MaxValue, Int32.MaxValue), false, true)!;
            }
        }

        /// <summary>
        /// Saves the icon into a stream
        /// </summary>
        internal void Save(Stream stream, bool forceBmpImages)
        {
            BinaryWriter bw = new BinaryWriter(stream);
            Save(bw, forceBmpImages);
        }

        internal Icon? ExtractIcon(int index, bool forceBmpFormat)
        {
            if (index < 0 || index >= iconImages.Count)
                return null;
            return iconImages[index].ToIcon(forceBmpFormat, true);
        }

        /// <summary>
        /// Gets the icons of the <see cref="RawIcon"/> instance as separated <see cref="Icon"/> instances.
        /// </summary>
        internal Icon?[] ExtractIcons(bool forceBmpFormat)
        {
            Icon?[] result = new Icon[iconImages.Count];
            for (int i = 0; i < result.Length; i++)
                result[i] = iconImages[i].ToIcon(forceBmpFormat, false);

            return result;
        }

        internal Bitmap? ExtractBitmap(int index, bool keepOriginalFormat)
        {
            if (index < 0 || index >= iconImages.Count)
                return null;
            return iconImages[index].ToBitmap(keepOriginalFormat);
        }

        /// <summary>
        /// Gets the images of the <see cref="RawIcon"/> instance as separated <see cref="Bitmap"/> instances.
        /// </summary>
        internal Bitmap[] ExtractBitmaps(bool keepOriginalFormat)
        {
            Bitmap[] result = new Bitmap[iconImages.Count];
            for (int i = 0; i < result.Length; i++)
                result[i] = iconImages[i].ToBitmap(keepOriginalFormat);

            return result;
        }

        /// <summary>
        /// Gets the nearest bitmap to the specified color depth and size. Bpp is matched first.
        /// If preferLarger is true, the distance to the larger images is halved. This is preferable when the extracted bitmap is about to be resized.
        /// </summary>
        internal Bitmap? ExtractNearestBitmap(int bpp, Size size, bool keepOriginalFormat, bool preferLarger)
        {
            if (iconImages.Count == 0)
                return null;

            if (iconImages.Count == 1)
                return iconImages[0].ToBitmap(keepOriginalFormat);

            RawIconImage nearestImage = GetNearestImage(bpp, size, preferLarger);
            return nearestImage.ToBitmap(keepOriginalFormat);
        }

        /// <summary>
        /// Gets the nearest icon to the specified color depth and size.
        /// </summary>
        internal Icon? ExtractNearestIcon(int bpp, Size size, bool forceBmpFormat)
        {
            if (iconImages.Count == 0)
                return null;

            if (iconImages.Count == 1)
                return iconImages[0].ToIcon(forceBmpFormat, false);

            RawIconImage nearestImage = GetNearestImage(bpp, size, false);
            Icon? result = nearestImage.ToIcon(forceBmpFormat, false);
            if (result != null)
                return result;

            return GetNextLargestResult(nearestImage, bpp, img => img.ToIcon(forceBmpFormat, false));
        }

        internal IconInfo GetIconInfo(int index) => iconImages[index].GetIconInfo();

        #endregion

        #region Private Methods

        private void Add(RawIconImage image) => iconImages.Add(image);

        private unsafe void Save(BinaryWriter bw, bool forceBmpImages)
        {
            // Icon header
            var iconDir = new ICONDIR
            {
                idReserved = 0,
                idType = 1,
                idCount = (ushort)iconImages.Count
            };

            bw.Write(BinarySerializer.SerializeValueType(iconDir));

            // Icon directory entries
            uint offset = (uint)(sizeof(ICONDIR) + iconDir.idCount * sizeof(ICONDIRENTRY));
            foreach (RawIconImage image in iconImages)
                image.WriteDirEntry(bw, forceBmpImages, ref offset);

            // Icon images
            foreach (RawIconImage image in iconImages)
                image.WriteRawImage(bw, forceBmpImages);
        }

        private unsafe void Load(BinaryReader br, Size? size, int? bpp, int? index)
        {
            byte[] buf = br.ReadBytes(sizeof(ICONDIR));
            ICONDIR iconDir = BinarySerializer.DeserializeValueType<ICONDIR>(buf);
            if (iconDir.idReserved != 0 || iconDir.idType != 1)
                throw new ArgumentException(DrawingRes.RawIconBadIconFormat, nameof(br));

            if (index.HasValue && (index.Value < 0 || index.Value >= iconDir.idCount))
                return;

            int entryOffset = sizeof(ICONDIR);
            int entrySize = sizeof(ICONDIRENTRY);
            for (int i = 0; i < iconDir.idCount; i++)
            {
                if (index.HasValue && index.Value != i)
                {
                    entryOffset += entrySize;
                    continue;
                }

                br.BaseStream.Position = entryOffset;
                buf = br.ReadBytes(entrySize);
                var entry = BinarySerializer.DeserializeValueType<ICONDIRENTRY>(buf);
                if (entry.wBitCount > 32)
                    entry.wBitCount = 32;

                Size reqSize = size.GetValueOrDefault();
                int reqBpp = bpp.GetValueOrDefault();

                // in entry header icons can have byte size only
                Size iconSizeByEntry = new Size(entry.bWidth, entry.bHeight);
                bool largeIconRequested = reqSize.Width > Byte.MaxValue && reqSize.Height > Byte.MaxValue;

                if ((size == null || reqSize == iconSizeByEntry || largeIconRequested || iconSizeByEntry.IsEmpty)
                    && (bpp == null || entry.wBitCount == 0 || reqBpp == entry.wBitCount))
                {
                    br.BaseStream.Position = entry.dwImageOffset;
                    var image = new RawIconImage(br.ReadBytes((int)entry.dwBytesInRes));

                    // bpp was explicitly defined, though there is 0 in dir entry: post-check for BPP
                    if (bpp != null && entry.wBitCount == 0 && image.Bpp != reqBpp
                        // similarly, post check for size
                        || size != null && image.Size != reqSize)
                    {
                        image.Dispose();
                    }
                    else
                        iconImages.Add(image);
                }

                entryOffset += entrySize;
            }
        }

        private RawIconImage GetNearestImage(int desiredBpp, Size desiredSize, bool preferLarger)
        {
            int desiredWidth = Math.Max(desiredSize.Width, 1);
            // Short solution: (but it does not stop on exact match and does not have preference on equal distances)
            //return iconImages.Aggregate((acc, i) => Math.Abs(i.Size.Width - desiredWidth) < Math.Abs(acc.Size.Width - desiredWidth)
            //    || i.Size == acc.Size && Math.Abs(i.Bpp - desiredBpp) < Math.Abs(acc.Bpp - desiredBpp) ? i : acc);

            RawIconImage preferredImage = iconImages[0];
            int preferredWidthDiff = preferLarger
                ? preferredImage.Size.Width > desiredSize.Width ? (preferredImage.Size.Width - desiredWidth) / 2 : desiredWidth - preferredImage.Size.Width
                : Math.Abs(preferredImage.Size.Width - desiredWidth);
            int preferredBppDiff = Math.Abs(preferredImage.Bpp - desiredBpp);
            for (var i = 1; i < iconImages.Count; i++)
            {
                // exact match: immediate return
                if (preferredImage.Size == desiredSize && preferredBppDiff == 0)
                    return preferredImage;

                // Size first, then BPP. On equal distance the higher value is preferred.
                RawIconImage currentImage = iconImages[i];
                int currentWidthDiff = preferLarger
                    ? currentImage.Size.Width > desiredSize.Width ? (currentImage.Size.Width - desiredWidth) / 2 : desiredWidth - currentImage.Size.Width
                    : Math.Abs(currentImage.Size.Width - desiredWidth);
                int currentBppDiff = Math.Abs(currentImage.Bpp - desiredBpp);
                if (currentWidthDiff < preferredWidthDiff // closer size
                    || (currentWidthDiff == preferredWidthDiff && currentImage.Size.Width > preferredImage.Size.Width) // same size difference and current is larger
                    || (currentImage.Size == preferredImage.Size
                        && (currentBppDiff < preferredBppDiff // same size but closer bpp 
                            || (currentBppDiff == preferredBppDiff && currentImage.Bpp > preferredImage.Bpp)))) // same size and same bpp difference but current has higher bpp
                {
                    preferredImage = currentImage;
                    preferredWidthDiff = currentWidthDiff;
                    preferredBppDiff = currentBppDiff;
                }
            }

            return preferredImage;
        }

        private TResult? GetNextLargestResult<TResult>(RawIconImage nearestImage, int bpp, Func<RawIconImage, TResult?> getResult)
            where TResult : class?
        {
            // On XP or Mono large icons might not be supported. Looking for the next largest one then.
            Debug.Assert(!OSUtils.IsVistaOrLater || OSUtils.IsMono, "null result is not expected on Windows Vista+");
            int lastSize;

            do
            {
                lastSize = nearestImage.Size.Width;
                int nextSize = lastSize;
                foreach (RawIconImage image in iconImages)
                {
                    // looking for the next largest size
                    if (image.Size.Width < lastSize && (nextSize == lastSize || nextSize < image.Size.Width))
                        nextSize = image.Size.Width;
                }

                // no smaller icon was found
                if (nextSize == lastSize)
                    return null;

                // trying the next size
                nearestImage = GetNearestImage(bpp, new Size(nextSize, nextSize), false);
                TResult? result = getResult.Invoke(nearestImage);
                if (result != null)
                    return result;

            } while (nearestImage.Size.Width < lastSize);

            return null;
        }

        #endregion

        #endregion
    }
}
