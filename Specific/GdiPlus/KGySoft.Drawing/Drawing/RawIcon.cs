#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: RawIcon.cs
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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
                {
                    image.Dispose();
                }

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
                base.InsertItem(index, item);
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
            /// Mask data (can be null even if BMP)
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

            #region Construction and Destruction

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
            internal unsafe RawIconImage(byte[] rawData)
            {
                int signature = BitConverter.ToInt32(rawData, 0);

                // PNG header: 0x89+"PNG"
                isPng = signature == 0x474E5089;
                if (isPng)
                {
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
                    return;
                }

                // BMP header: size of the BITMAPINFOHEADER structure
                if (signature != sizeof(BITMAPINFOHEADER))
                    throw new ArgumentException(DrawingRes.RawIconBadIconFormat, nameof(rawData));

                // header
                bmpHeader = BinarySerializer.DeserializeValueType<BITMAPINFOHEADER>(rawData);
                size = new Size(bmpHeader.biWidth, bmpHeader.biHeight >> 1); // height is doubled because of mask
                bpp = bmpHeader.biBitCount;
                int offset = signature;

                // palette
                int colorCount = PaletteColorCount;
                if (colorCount > 0)
                {
                    palette = BinarySerializer.DeserializeValueArray<RGBQUAD>(rawData, offset, colorCount);
                    offset += sizeof(RGBQUAD) * palette.Length;
                }

                // color image (XOR)
                int strideColor = ((bmpHeader.biWidth * bmpHeader.biBitCount + 31) & ~31) >> 3;
                rawColor = new byte[strideColor * size.Height];
                Buffer.BlockCopy(rawData, offset, rawColor, 0, rawColor.Length);
                offset += rawColor.Length;

                // mask image (AND)
                int strideMask = ((bmpHeader.biWidth + 31) & ~31) >> 3;
                int maskLength = strideMask * size.Height;
                if (offset + maskLength > rawData.Length)
                {
                    // the mask is sometimes omitted for 32bpp images but we still generate it for best compatibility
                    GenerateMask(strideColor, strideMask);
                    return;
                }

                rawMask = new byte[maskLength];
                Buffer.BlockCopy(rawData, offset, rawMask, 0, maskLength);
            }

            #endregion

            #region Destructor

            ~RawIconImage() => Dispose(false);

            #endregion

            #endregion

            #region Methods

            #region Public Methods

            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
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
                using (var ms = new MemoryStream())
                {
                    Save(ms, forceBmpFormat);

                    // returning icon
                    ms.Position = 0L;
                    try
                    {
                        return new Icon(ms);
                    }
                    catch (Exception)
                    {
                        if (OSUtils.IsWindows)
                            throw;

                        // On Linux 256x256 icons may not be supported even with BMP format.
                        if (throwError)
                            throw new PlatformNotSupportedException(DrawingRes.RawIconCannotBeInstantiatedAsIcon);
                        return null;
                    }
                }
            }

            internal Bitmap? ToBitmap(bool keepOriginalFormat, bool throwError)
            {
                AssureBitmapsGenerated(!keepOriginalFormat);
                if (keepOriginalFormat && bmpColor != null)
                    return bmpColor.CloneBitmap();

                if (bmpComposite == null)
                {
                    Debug.Assert(!OSUtils.IsWindows, "Bitmaps should have been able to be generated on Windows");
                    if (bmpColor != null)
                        return bmpColor.CloneBitmap();
                    if (!throwError)
                        return null;
                    throw new PlatformNotSupportedException(DrawingRes.RawIconCannotBeInstantiatedAsBitmap);
                }

                // When not original format is requested, returning a new bitmap instead of cloning for PNGs,
                // because for PNG images may cause troubles in some cases (eg. OutOfMemoryException when used as background image)
                if (bmpComposite.RawFormat.Guid == ImageFormat.Png.Guid)
                    return new Bitmap(bmpComposite);

                // Cloning by Bitmap.Clone instead of Image.Clone because the latter may return a blank result on Linux
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

            private void Dispose(bool disposing)
            {
                if (disposing)
                {
                    bmpColor?.Dispose();
                    bmpComposite?.Dispose();
                }

                bmpColor = null;
                bmpComposite = null;
                rawColor = null;
                rawMask = null;
                palette = null;
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
                    using (MemoryStream ms = new MemoryStream())
                    {
                        // When PNG, using composite image in the first place
                        // ReSharper disable once PossibleNullReferenceException
                        bmp.Save(ms, ImageFormat.Png);
                        rawColor = ms.ToArray();
                        return;
                    }
                }

                // palette
                if (bpp <= 8)
                {
                    // generating the maximum number of palette entries without optimization
                    // (so PaletteColorCount can return number of colors before generating the palette)
                    palette = new RGBQUAD[1 << bpp];
                    // ReSharper disable once PossibleNullReferenceException
                    Color[] entries = bmp.Palette.Entries;
                    for (int i = 0; i < entries.Length; i++)
                    {
                        palette[i].rgbRed = entries[i].R;
                        palette[i].rgbGreen = entries[i].G;
                        palette[i].rgbBlue = entries[i].B;
                    }
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

                // Color image (XOR): copying from input bitmap
                int strideColor;

                // ReSharper disable once PossibleNullReferenceException - bmp cannot be null here
                BitmapData dataColor = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, bmp.PixelFormat);
                try
                {
                    strideColor = dataColor.Stride;
                    rawColor = new byte[Math.Abs(strideColor) * dataColor.Height];
                    bmpHeader.biSizeImage = (uint)rawColor.Length;

                    // Theoretically negative stride cannot occur here because Bitmap fields are clones but just in case...
                    if (strideColor < 0)
                    {
                        IntPtr startAddress = new IntPtr(dataColor.Scan0.ToInt64() + (dataColor.Height - 1) * strideColor);
                        Marshal.Copy(startAddress, rawColor, 0, rawColor.Length);
                        strideColor = -strideColor;
                    }
                    else
                    {
                        // If stride is positive, then flipping the image horizontally because a bottom-up BMP has to be saved
                        for (int i = 0; i < size.Height; i++)
                        {
                            IntPtr offsetSrc = new IntPtr(dataColor.Scan0.ToInt64() + i * strideColor);
                            int offsetDst = (size.Height - 1 - i) * strideColor;
                            Marshal.Copy(offsetSrc, rawColor, offsetDst, strideColor);
                        }
                    }
                }
                finally
                {
                    bmp.UnlockBits(dataColor);
                }

                // Mask image (AND): Creating from color image and provided transparent color.
                int strideMask = ((size.Width + 31) >> 5) << 2; // Stride = 4 * (Width * bpp + 31) / 32)

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

                for (int y = 0; y < size.Height; y++)
                {
                    int posColorY = strideColor * y;
                    int posMaskY = strideMask * y;
                    for (int x = 0; x < size.Width; x++)
                    {
                        int mask = 128 >> (x & 7);
                        switch (bpp)
                        {
                            case 1:
                                int bits = rawColor![(x >> 3) + posColorY];
                                int colorIndex = (bits & mask) != 0 ? 1 : 0;
                                if (transparentIndices!.Contains(colorIndex))
                                    rawMask[(x >> 3) + posMaskY] |= (byte)mask;
                                break;

                            case 4:
                                bits = rawColor![(x >> 1) + posColorY];
                                colorIndex = (x & 1) == 0 ? bits >> 4 : bits & 0b00001111;
                                if (transparentIndices!.Contains(colorIndex))
                                    rawMask[(x >> 3) + posMaskY] |= (byte)mask;
                                break;

                            case 8:
                                if (transparentIndices!.Contains(rawColor![x + posColorY]))
                                    rawMask[(x >> 3) + posMaskY] |= (byte)mask;
                                break;

                            case 24:
                                int pos = posColorY + x * 3;
                                Color32 pixelColor = new Color32(rawColor![pos + 2],
                                    rawColor[pos + 1],
                                    rawColor[pos]);
                                if (pixelColor == transparentColor)
                                    rawMask[(x >> 3) + posMaskY] |= (byte)mask;
                                break;

                            case 32:
                                pos = posColorY + (x << 2);
                                pixelColor = new Color32(rawColor![pos + 3],
                                    rawColor[pos + 2],
                                    rawColor[pos + 1],
                                    rawColor[pos]);

                                if (pixelColor == transparentColor || transparentColor.A == 0 && pixelColor.A < 128)
                                    rawMask[(x >> 3) + posMaskY] |= (byte)mask;

                                break;
                        }
                    }
                }
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

                // BMP format - composite image
                if (isCompositeRequired)
                {
                    using (Icon? icon = ToIcon(true, false))
                    {
                        // On Linux it may return null, in which case we fall back to non-compisite image
                        if (icon != null)
                        {
                            // ToBitmap works well here because PNG would have been returned above. Semi-transparent pixels will never be black
                            // because BPP is always set well in ICONDIRENTRY so ToAlphaBitmap is not required here.
                            bmpComposite = icon.ToBitmap();
                            return;
                        }
                    }
                }

                // Working from raw format. If it doesn't exist, creating from composite image...
                if (rawColor == null)
                {
                    // Unless the we decide to handle the image as if it was PNG. In this case the icon was created
                    // from a large bitmap but isPng should remain false because we don't create rawColor.
                    if (IsPngPreferred)
                    {
                        AssurePngBitmapsGenerated(false);
                        return;
                    }

                    // Now we create rawColor and it will be a BMP for sure.
                    AssureRawFormatGenerated(true);
                }

                if (OSUtils.IsWindows)
                    GenerateColorBitmapWindows();
                else
                    GenerateColorBitmapNonWindows();
            }

            private void GenerateColorBitmapWindows()
            {
                // BMP format - original image format required
                IntPtr dcScreen = User32.GetDC(IntPtr.Zero);

                // initializing bitmap data
                BITMAPINFO bitmapInfo;
                bitmapInfo.icHeader = bmpHeader;
                bitmapInfo.icHeader.biHeight /= 2;
                bitmapInfo.icColors = null;

                if (PaletteColorCount > 0)
                {
                    bitmapInfo.icColors = new RGBQUAD[256];
                    for (int i = 0; i < palette!.Length; i++)
                        bitmapInfo.icColors[i] = palette[i];
                }

                // creating color raw bitmap (XOR)
                IntPtr dcColor = Gdi32.CreateCompatibleDC(dcScreen);
                IntPtr hbmpColor = Gdi32.CreateDibSectionRgb(dcColor, ref bitmapInfo, out IntPtr bits);
                Marshal.Copy(rawColor!, 0, bits, rawColor!.Length);

                // creating bmpColor
                bmpColor = Image.FromHbitmap(hbmpColor);

                User32.ReleaseDC(IntPtr.Zero, dcScreen);
                Gdi32.DeleteObject(hbmpColor);
                Gdi32.DeleteDC(dcColor);
            }

            private void GenerateColorBitmapNonWindows()
            {
                // On non-windows the original format will be the icon itself
                var ms = new MemoryStream();
                Save(ms, true);
                ms.Position = 0;
                try
                {
                    var bmp = new Bitmap(ms);

                    // On Linux an uncompressed 256x256 icon will be instantiated as a 0x0 bitmap
                    if (!bmp.Size.IsEmpty)
                        bmpColor = bmp;
                    else if (bmpColor == null)
                        bmpColor = bmpComposite;
                }
                catch (Exception)
                {
                    if (OSUtils.IsWindows)
                        throw;

                    // As a fallback we use the composite image (if any)
                    bmpColor = bmpComposite;
                }
            }

            private void AssurePngBitmapsGenerated(bool isCompositeRequired)
            {
                // Note: MemoryStream must not be in a using because that would kill the new bitmap.
                Bitmap result = bmpComposite ?? bmpColor
                    // rawColor is available, otherwise, object would be disposed. 
                    ?? new Bitmap(new MemoryStream(rawColor!));

                // assignments below will not replace any instance, otherwise, we would have returned above
                if (isCompositeRequired)
                    bmpComposite = result;
                else
                {
                    // if PNG bpp matches the required result
                    if (bpp == result.GetBitsPerPixel())
                        bmpColor = result;
                    else
                    {
                        // generating a lower bpp image
                        // note: this code theoretically executes only for indexed PNG if the decoder restored it with higher BPP
                        Color[]? paletteBmpColor = null;
                        if (bpp <= 8)
                            paletteBmpColor = result.GetColors(1 << bpp);

                        bmpColor = result.ConvertPixelFormat(bpp.ToPixelFormat(), paletteBmpColor);
                        result.Dispose();
                    }
                }
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
        {
            PixelFormat.Format1bppIndexed,
            PixelFormat.Format4bppIndexed,
            PixelFormat.Format8bppIndexed,
            PixelFormat.Format24bppRgb,
            PixelFormat.Format32bppArgb
        };

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

                using (Bitmap bmp = icon.ToAlphaBitmap())
                {
                    if (!bpp.HasValue || bpp.Value == bmp.GetBitsPerPixel())
                    {
                        Add(bmp);
                    }
                }

                return;
            }

            // initializing from stream
            using (var ms = new MemoryStream())
            {
                icon.Save(ms);
                ms.Position = 0L;

                using (var br = new BinaryReader(ms))
                    Load(br, size, bpp, index);
            }
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
                bitmaps = new[] { image.ConvertPixelFormat(PixelFormat.Format32bppArgb) };
            else if (image.RawFormat.Guid == ImageFormat.Icon.Guid)
                bitmaps = image.ExtractIconImages();
            else
                // Image.Clone() could result in a blank Bitmap on Linux if its content was drawn by Graphics
                bitmaps = new[] { image.CloneBitmap() };

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

            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    Save(bw, forceBmpImages);
                    ms.Position = 0L;
                    try
                    {
                        return new Icon(ms);
                    }
                    catch (Exception e)
                    {
                        if (OSUtils.IsWindows)
                            throw;
                        throw new PlatformNotSupportedException(DrawingRes.RawIconCannotBeInstantiatedAsIcon, e);
                    }
                }
            }
        }

        /// <summary>
        /// Gets a <see cref="Bitmap"/> instance, which contains every images of the <see cref="RawIcon"/> instance as a single, multi-resolution <see cref="Bitmap"/>.
        /// </summary>
        internal Bitmap? ToBitmap()
        {
            if (iconImages.Count == 0)
                return null;

            // not in using because stream must left open during the Bitmap lifetime
            var ms = new MemoryStream();
            var bw = new BinaryWriter(ms);
            Save(bw, true);
            ms.Position = 0L;

            try
            {
                return new Bitmap(ms);
            }
            catch (Exception e)
            {
                throw new PlatformNotSupportedException(DrawingRes.RawIconCannotBeInstantiatedAsBitmap, e);
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
            return iconImages[index].ToBitmap(keepOriginalFormat, true);
        }

        /// <summary>
        /// Gets the images of the <see cref="RawIcon"/> instance as separated <see cref="Bitmap"/> instances.
        /// </summary>
        internal Bitmap?[] ExtractBitmaps(bool keepOriginalFormat)
        {
            Bitmap?[] result = new Bitmap[iconImages.Count];
            for (int i = 0; i < result.Length; i++)
                result[i] = iconImages[i].ToBitmap(keepOriginalFormat, false);

            return result;
        }

        /// <summary>
        /// Gets the nearest bitmap to the specified color depth and size. Bpp is matched first.
        /// If preferLarger is true, the distance the larger images is halved. This is preferable when the extracted bitmap is about to be resized.
        /// </summary>
        internal Bitmap? ExtractNearestBitmap(int bpp, Size size, bool keepOriginalFormat, bool preferLarger)
        {
            if (iconImages.Count == 0)
                return null;

            if (iconImages.Count == 1)
                return iconImages[0].ToBitmap(keepOriginalFormat, false);

            RawIconImage nearestImage = GetNearestImage(bpp, size, preferLarger);
            Bitmap? result = nearestImage.ToBitmap(keepOriginalFormat, false);
            if (result != null)
                return result;

            return GetNextLargestResult(nearestImage, bpp, img => img.ToBitmap(keepOriginalFormat, false));
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
            // On Linux large icons might not be supported. Looking for the next largest one then.
            Debug.Assert(!OSUtils.IsWindows, "null result is not expected on Windows");
            int lastSize, nextSize;

            do
            {
                lastSize = nearestImage.Size.Width;
                nextSize = lastSize;
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
