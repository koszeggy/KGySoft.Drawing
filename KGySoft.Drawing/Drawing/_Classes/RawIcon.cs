#region Used namespaces

using System;
using System.Drawing;
using System.Collections.ObjectModel;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using KGySoft.Collections;
using KGySoft.CoreLibraries;
using KGySoft.Drawing.WinApi;
using KGySoft.Serialization;

#endregion

namespace KGySoft.Drawing
{
    /// <summary>
    /// Provides low-level support for an icon. This class is internal because it can process and produce <see cref="Icon"/> and <see cref="Bitmap"/>
    /// instances and every functionality is accessible via extensions for those classes.
    /// </summary>
    internal sealed class RawIcon : IDisposable
    {
        #region Nested classes

        #region RawIconImageCollection class

        private sealed class RawIconImageCollection : Collection<RawIconImage>, IDisposable
        {
            #region Explicit Disposing

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

            #region Methods

            /// <summary>
            /// Inserts an element into the <see cref="T:System.Collections.ObjectModel.Collection`1"></see> at the specified index.
            /// </summary>
            /// <param name="index">The zero-based index at which item should be inserted.</param>
            /// <param name="item">The object to insert. The value can be null for reference types.</param>
            /// <exception cref="System.NotSupportedException">Too many images in the icon collection</exception>
            protected override void InsertItem(int index, RawIconImage item)
            {
                if (Count == UInt16.MaxValue)
                    throw new NotSupportedException(Res.RawIconTooManyImages);
                base.InsertItem(index, item);
            }

            #endregion
        }

        #endregion

        #region RawIconImage class

        private sealed class RawIconImage : IDisposable
        {
            #region Fields

            /// <summary>
            /// In: Source image if it is already 32 bit ARGB and Color.Transparent is specified for transparency
            /// Out: Result image of ToBitmap(false)
            /// </summary>
            private Bitmap bmpComposite;

            /// <summary>
            /// In: Source image if it is non ARGB or when a custom transparent color is specified
            /// Out: Result image of ToBitmap(true)
            /// </summary>
            private Bitmap bmpColor;

            private Color transparentColor;

            /// <summary>
            /// Color image or the raw image itself when PNG
            /// </summary>
            private byte[] rawColor;

            /// <summary>
            /// Mask data (only if BMP)
            /// </summary>
            private byte[] rawMask;

            /// <summary>
            /// Header (only if BMP)
            /// </summary>
            private BITMAPINFOHEADER bmpHeader;

            /// <summary>
            /// Palette (only if indexed BMP)
            /// </summary>
            private RGBQUAD[] palette;

            private readonly bool isPng;

            /// <summary>
            /// The bit count stored by the dir entry. Needed when actual format is PNG.
            /// </summary>
            private readonly int dirEntryBitCountPng;

            private readonly Size size;

            #endregion

            #region Properties

            #region Internal Properties

            /// <summary>
            /// Gets the bpp from raw data.
            /// </summary>
            internal int RawBpp => isPng ? dirEntryBitCountPng : bmpHeader.biBitCount;

            /// <summary>
            /// Gets the size in pixels
            /// </summary>
            internal Size Size => size;

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
                    Bitmap bmp = bmpColor ?? bmpComposite;
                    if (bmp != null)
                    {
                        int bpp = bmp.PixelFormat.ToBitsPerPixel();
                        return bpp > 8 ? 0 : 1 << bpp;
                    }

                    throw new ObjectDisposedException(null, Res.ObjectDisposed);
                }
            }

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
                if (bmpColor.PixelFormat == PixelFormat.Format32bppArgb
                    && (transparentColor == Color.Transparent || transparentColor == Color.Empty))
                {
                    bmpComposite = bmpColor;
                    this.transparentColor = Color.Transparent;
                }
                else
                {
                    this.bmpColor = bmpColor;
                    this.transparentColor = transparentColor;
                }

                // setting PNG format arbitrarily
                isPng = bmpColor.PixelFormat.ToBitsPerPixel() >= 32 && bmpColor.Width > 64 && bmpColor.Height > 64;
                if (isPng)
                    dirEntryBitCountPng = 32;

                size = bmpColor.Size;
            }

            /// <summary>
            /// From raw data
            /// </summary>
            internal RawIconImage(byte[] rawData)
            {
                int signature = BitConverter.ToInt32(rawData, 0);

                // PNG header: 0x89+"PNG"
                isPng = signature == 0x474E5089;
                if (isPng)
                {
                    rawColor = rawData;

                    // byte 24: bpp per channel
                    dirEntryBitCountPng = rawData[24];
                    switch (rawData[25])
                    {
                        case 2: // RGB
                            dirEntryBitCountPng *= 3;
                            break;
                        case 4: // Alpha Grayscale
                            dirEntryBitCountPng <<= 1;
                            break;
                        case 6: // ARGB
                            dirEntryBitCountPng <<= 2;
                            break;
                    }

                    // size is at 16 and 20 DWORD big endian so reading last 2 bytes only
                    size = new Size(rawData[18] << 8 + rawData[19], rawData[22] << 8 + rawData[23]);
                    return;
                }

                // BMP header: size of the BITMAPINFOHEADER structure
                if (signature != Marshal.SizeOf(typeof(BITMAPINFOHEADER)))
                    throw new ArgumentException(Res.RawIconBadIconFormat, nameof(rawData));

                // header
                bmpHeader = (BITMAPINFOHEADER)BinarySerializer.DeserializeValueType(typeof(BITMAPINFOHEADER), rawData);
                size = new Size(bmpHeader.biWidth, bmpHeader.biHeight >> 1); // height is doubled because of mask
                int offset = signature;

                // palette
                int colorCount = PaletteColorCount;
                if (colorCount > 0)
                {
                    palette = BinarySerializer.DeserializeValueArray<RGBQUAD>(rawData, offset, colorCount);
                    offset += Marshal.SizeOf(typeof(RGBQUAD)) * palette.Length;
                }

                // color image (XOR)
                int stride = ((bmpHeader.biWidth * bmpHeader.biBitCount + 31) & ~31) >> 3;
                rawColor = new byte[stride * size.Height];
                Buffer.BlockCopy(rawData, offset, rawColor, 0, rawColor.Length);
                offset += rawColor.Length;

                // mask image (AND)
                stride = ((bmpHeader.biWidth + 31) & ~31) >> 3;
                rawMask = new byte[stride * size.Height];
                Buffer.BlockCopy(rawData, offset, rawMask, 0, rawMask.Length);
            }

            #endregion

            #region Destructor

            /// <summary>
            /// Finalizes an instance of the <see cref="RawIconImage"/> class.
            /// </summary>
            ~RawIconImage()
            {
                Dispose(false);
            }

            #endregion

            #region Explicit Disposing

            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            /// <summary>
            /// Releases unmanaged and - optionally - managed resources.
            /// </summary>
            /// <param name="disposing"><see langword="true"/>&#160;to release both managed and unmanaged resources; <see langword="false"/>&#160;to release only unmanaged resources.</param>
            private void Dispose(bool disposing)
            {
                if (disposing)
                {
                    if (bmpColor != null)
                    {
                        bmpColor.Dispose();
                        bmpColor = null;
                    }

                    if (bmpComposite != null)
                    {
                        bmpComposite.Dispose();
                        bmpComposite = null;
                    }

                    rawColor = null;
                    rawMask = null;
                    palette = null;
                }
            }

            #endregion

            #endregion

            #region Methods

            #region Static Methods

            [SecurityCritical]
            private static void FlipImageY(Bitmap bitmap)
            {
                if (bitmap.PixelFormat != PixelFormat.Format1bppIndexed)
                {
                    bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
                    return;
                }

                // Workaround: flipping 1bpp image manually because Image.RotateFlip cannot do it properly
                BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format1bppIndexed);

                IntPtr pixelPtr = bitmapData.Scan0;
                byte[] tmpbuffer = new byte[Math.Abs(bitmapData.Stride)];

                for (int i = 0; i < bitmap.Height / 2; i++)
                {
                    Marshal.Copy(new IntPtr(pixelPtr.ToInt64() + (i * bitmapData.Stride)), tmpbuffer, 0, bitmapData.Stride);
                    Kernel32.CopyMemory(new IntPtr(pixelPtr.ToInt64() + (i * bitmapData.Stride)), new IntPtr(pixelPtr.ToInt64() + (((bitmap.Height - 1) - i) * bitmapData.Stride)), bitmapData.Stride);
                    Marshal.Copy(tmpbuffer, 0, new IntPtr(pixelPtr.ToInt64() + (((bitmap.Height - 1) - i) * bitmapData.Stride)), bitmapData.Stride);
                }

                bitmap.UnlockBits(bitmapData);
            }

            #endregion

            #region Instance Methods

            #region Internal Methods

            [SecurityCritical]
            internal void WriteDirEntry(BinaryWriter bw, ref uint offset)
            {
                AssureRawFormatGenerated();
                ICONDIRENTRY entry = new ICONDIRENTRY
                {
                    dwImageOffset = offset
                };

                if (!isPng)
                {
                    entry.bWidth = (byte)bmpHeader.biWidth;
                    entry.bHeight = (byte)(bmpHeader.biHeight >> 1);
                    entry.bColorCount = (byte)bmpHeader.biClrUsed;
                    entry.wPlanes = bmpHeader.biPlanes;
                    entry.wBitCount = bmpHeader.biBitCount;
                    entry.dwBytesInRes = (uint)(Marshal.SizeOf(typeof(BITMAPINFOHEADER)) + Marshal.SizeOf(typeof(RGBQUAD)) * PaletteColorCount +
                        rawColor.Length + rawMask.Length);
                }
                else
                {
                    entry.wPlanes = 1;
                    entry.wBitCount = (ushort)(dirEntryBitCountPng == 0 ? 32 : dirEntryBitCountPng);
                    entry.dwBytesInRes = (uint)rawColor.Length;
                }

                bw.Write(BinarySerializer.SerializeValueType(entry));
                offset += entry.dwBytesInRes;
            }

            [SecurityCritical]
            internal void WriteRawImage(BinaryWriter bw)
            {
                AssureRawFormatGenerated();
                if (isPng)
                {
                    bw.Write(rawColor);
                    return;
                }

                // header
                bw.Write(BinarySerializer.SerializeValueType(bmpHeader));

                // Palette
                if (PaletteColorCount > 0)
                    bw.Write(BinarySerializer.SerializeValueArray(palette));

                // color image (XOR)
                bw.Write(rawColor);

                // mask image (AND)
                bw.Write(rawMask);
            }

            [SecurityCritical]
            [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "False alarm, BinaryWriter leaves the stream open.")]
            internal Icon ToIcon()
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    using (BinaryWriter bw = new BinaryWriter(ms, Encoding.ASCII, true))
                    {
                        // header
                        ICONDIR iconDir = new ICONDIR
                        {
                            idReserved = 0,
                            idType = 1,
                            idCount = 1
                        };

                        bw.Write(BinarySerializer.SerializeValueType(iconDir));

                        // Icon entry
                        uint offset = (uint)(Marshal.SizeOf(typeof(ICONDIR)) + Marshal.SizeOf(typeof(ICONDIRENTRY)));
                        WriteDirEntry(bw, ref offset);

                        // Icon image
                        WriteRawImage(bw);

                        // returning icon
                        ms.Position = 0L;
                        return new Icon(ms);
                    }
                }
            }

            [SecurityCritical]
            internal Bitmap ToBitmap(bool keepOriginalFormat)
            {
                AssureBitmapsGenerated(!keepOriginalFormat);
                if (keepOriginalFormat)
                    return (Bitmap)bmpColor.Clone();

                // When not original format is requested, returning a new bitmap instead of cloning for PNGs,
                // because for PNG images may couse troubles in some cases (eg. OutOfMemoryException when used as background image)
                if (bmpComposite.RawFormat.Guid == ImageFormat.Png.Guid)
                    return new Bitmap(bmpComposite);
                return (Bitmap)bmpComposite.Clone();
            }

            #endregion

            #region Private Methods

            [SecurityCritical]
            private void AssureRawFormatGenerated()
            {
                // exiting, if raw data is already generated
                if (rawColor != null)
                    return;

                // if both raw and image data is null, then object is disposed
                if (bmpColor == null && bmpComposite == null)
                    throw new ObjectDisposedException(null, Res.ObjectDisposed);

                if (isPng)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        // When PNG, using composite image in the first place
                        (bmpComposite ?? bmpColor).Save(ms, ImageFormat.Png);
                        rawColor = ms.ToArray();
                        return;
                    }
                }

                int bpp;
                int strideColor;
                Bitmap bmp = bmpColor ?? bmpComposite;

                // palette
                bpp = bmp.PixelFormat.ToBitsPerPixel();
                if (bpp <= 8)
                {
                    // generating the maximum number of palette entries without optimization
                    // (so PaletteColorCount can return number of colors before generating the palette)
                    palette = new RGBQUAD[1 << bpp];
                    Color[] entries = bmp.Palette.Entries;
                    for (int i = 0; i < entries.Length; i++)
                    {
                        palette[i].rgbRed = entries[i].R;
                        palette[i].rgbGreen = entries[i].G;
                        palette[i].rgbBlue = entries[i].B;
                    }
                }

                // header
                bmpHeader.biSize = (uint)Marshal.SizeOf(typeof(BITMAPINFOHEADER));
                bmpHeader.biWidth = bmp.Width;
                bmpHeader.biHeight = bmp.Height << 1; // because of mask, should be specified as double height image
                bmpHeader.biPlanes = 1;
                bmpHeader.biBitCount = (ushort)bmp.PixelFormat.ToBitsPerPixel();
                bmpHeader.biCompression = BitmapCompressionMode.BI_RGB;
                bmpHeader.biXPelsPerMeter = 0;
                bmpHeader.biYPelsPerMeter = 0;
                bmpHeader.biClrUsed = (uint)PaletteColorCount;
                bmpHeader.biClrImportant = 0;

                // Color image (XOR): copying from input bitmap
                using (bmp = (Bitmap)bmp.Clone())
                {
                    // TODO: remove clone+using and flip rawColor similarly to the fallback in FlipImageY. See the TODO below this block.
                    FlipImageY(bmp);

                    BitmapData dataColor = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, bmp.PixelFormat);
                    try
                    {
                        IntPtr scanColor = dataColor.Scan0;
                        strideColor = dataColor.Stride;
                        rawColor = new byte[Math.Abs(strideColor) * dataColor.Height];
                        Marshal.Copy(scanColor, rawColor, 0, rawColor.Length);
                    }
                    finally
                    {
                        bmp.UnlockBits(dataColor);
                    }
                }

                bmpHeader.biSizeImage = (uint)rawColor.Length;
                if (strideColor > 0)
                {
                    // TODO: Flip rawColor
                }
                else
                    strideColor *= -1;

                // Mask image (AND): Creating from color image and provided transparent color.
                int strideMask = ((size.Width + 31) >> 5) << 2; // Stride = 4 * (Width * BitsPerPixel + 31)/32)
                rawMask = new byte[strideMask * size.Height];

                // If the image bpp is less than 32, transparent color cannot have transparency
                if (bpp < 32)
                    transparentColor = Color.FromArgb(255, transparentColor.R, transparentColor.G, transparentColor.B);

                DoGenerateRawData(bpp, strideColor, strideMask);
            }

            private void DoGenerateRawData(int bpp, int strideColor, int strideMask)
            {
                // rawColor now contains the provided bitmap data with the original background, while rawMask is still totally empty
                for (int y = 0; y < size.Height; y++)
                {
                    int posColorY = strideColor * y;
                    int posMaskY = strideMask * y;
                    for (int x = 0; x < size.Width; x++)
                    {
                        int color;
                        RGBQUAD paletteColor;
                        switch (bpp)
                        {
                            case 1:
                                // TODO: the following code if transparentColor is the first palette entry. If the second one, then negate rawMask. If none of them, 0xFF
                                rawMask[(x >> 3) + posColorY] = rawColor[(x >> 3) + posColorY];
                                x += 7; // otherwise, bytes would be set 8 times
                                continue;
                            case 4:
                                color = rawColor[(x >> 1) + posColorY];
                                paletteColor = palette[(x & 1) == 0 ? color >> 4 : color & 0x0F];
                                if (paletteColor.EqualsWithColor(transparentColor))
                                {
                                    rawMask[(x >> 3) + posMaskY] |= (byte)(0x80 >> (x & 7));
                                    rawColor[(x >> 1) + posColorY] &= (byte)((x & 1) == 0 ? 0x0F : 0xF0);
                                }

                                break;
                            case 8:
                                color = rawColor[x + posColorY];
                                paletteColor = palette[color];
                                if (paletteColor.EqualsWithColor(transparentColor))
                                {
                                    rawMask[(x >> 3) + posMaskY] |= (byte)(0x80 >> (x & 7));
                                    rawColor[x + posColorY] = 0;
                                }

                                break;
                            case 16:
                            case 48:
                            case 64:
                                throw new NotSupportedException(Res.RawIconUnsupportedBpp);
                            case 24:
                                int posCX = x * 3;
                                Color pixelColor = Color.FromArgb(0, rawColor[posCX + posColorY + 0],
                                    rawColor[posCX + posColorY + 1],
                                    rawColor[posCX + posColorY + 2]);
                                if (pixelColor == transparentColor)
                                    rawMask[(x >> 3) + posMaskY] |= (byte)(0x80 >> (x & 7));
                                break;
                            case 32:
                                if (transparentColor == Color.Transparent)
                                {
                                    if (rawColor[(x << 2) + posColorY + 3] == 0)
                                        rawMask[(x >> 3) + posMaskY] |= (byte)(0x80 >> (x & 7));
                                }
                                else
                                {
                                    if (transparentColor != Color.Empty &&
                                        rawColor[(x << 2) + posColorY + 0] == transparentColor.B &&
                                        rawColor[(x << 2) + posColorY + 1] == transparentColor.G &&
                                        rawColor[(x << 2) + posColorY + 2] == transparentColor.R)
                                    {
                                        rawMask[(x >> 3) + posMaskY] |= (byte)(0x80 >> (x & 7));
                                        rawColor[(x << 2) + posColorY + 0] = 0;
                                        rawColor[(x << 2) + posColorY + 1] = 0;
                                        rawColor[(x << 2) + posColorY + 2] = 0;
                                    }
                                    else
                                    {
                                        rawColor[(x << 2) + posColorY + 3] = 255;
                                    }
                                }

                                break;
                        }
                    }
                }
            }

            [SecurityCritical]
            [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The stream must not be disposed if passed to a Bitmap constructor.")]
            private void AssureBitmapsGenerated(bool isCompositRequired)
            {
                if (rawColor == null && bmpColor == null && bmpComposite == null)
                    throw new ObjectDisposedException(null, Res.ObjectDisposed);

                // exiting, if the requested bitmap already exists
                if (isCompositRequired && bmpComposite != null || !isCompositRequired && bmpColor != null)
                    return;

                // PNG format
                if (isPng)
                {
                    Bitmap result = bmpComposite ?? bmpColor
                        // rawColor is available, otherwise, object would be disposed. Note: MemoryStream must not be in a using because that would kill the new bitmap.
                        ?? new Bitmap(new MemoryStream(rawColor));

                    // assignments below will not replace any instance, otherwise, we would have returned above
                    if (isCompositRequired)
                        bmpComposite = result;
                    else
                    {
                        // if png bpp matches the required result
                        if (dirEntryBitCountPng == 0 || dirEntryBitCountPng == 32 || dirEntryBitCountPng == result.PixelFormat.ToBitsPerPixel())
                            bmpColor = result;
                        else
                        {
                            // generating a lower bpp image
                            // note: this code will theoretically never executed because dirEntryBitCountPng is now obtained from the PNG stream and not from the icondirenty
                            Color[] paletteBmpColor = null;
                            if (dirEntryBitCountPng <= 8)
                                paletteBmpColor = result.GetColors(1 << dirEntryBitCountPng);

                            bmpColor = (Bitmap)result.ConvertPixelFormat(dirEntryBitCountPng.ToPixelFormat(), paletteBmpColor);
                            result.Dispose();
                        }

                        //// turning transparent color black
                        //ColorPalette palette = bmpColor.Palette;
                        //if (palette.Entries.Length != 0)
                        //{
                        //    int transparentIndex = Array.FindIndex(palette.Entries, c => c.A == 0);
                        //    if (transparentIndex != -1)
                        //    {
                        //        palette.Entries[transparentIndex] = Color.Black;
                        //        bmpColor.Palette = palette;
                        //    }
                        //}
                    }

                    return;
                }

                // BMP format - composite image
                if (isCompositRequired)
                {
                    Icon icon = ToIcon();

                    // ToBitmap works well here because PNG would have been returned above. Semi-transparent pixels will never be black
                    // because bpp is always set well in icondir entry so ToAlphaBitmap is not required here.
                    bmpComposite = icon.ToBitmap();
                    icon.Dispose();
                    return;
                }

                // BMP format - original image format required
                IntPtr dcScreen = User32.GetDC(IntPtr.Zero);

                // working from raw format. If it doesn't exist, creating from composite image
                if (rawColor == null)
                    AssureRawFormatGenerated();

                // initializing bitmap data
                BITMAPINFO bitmapInfo;
                bitmapInfo.icHeader = bmpHeader;
                bitmapInfo.icHeader.biHeight /= 2;
                bitmapInfo.icColors = null;

                if (PaletteColorCount > 0)
                {
                    bitmapInfo.icColors = new RGBQUAD[256];
                    for (int i = 0; i < palette.Length; i++)
                    {
                        bitmapInfo.icColors[i] = palette[i];
                    }
                }

                // creating color raw bitmap (XOR)
                IntPtr dcColor = Gdi32.CreateCompatibleDC(dcScreen);
                IntPtr hbmpColor = Gdi32.CreateDibSectionRgb(dcColor, ref bitmapInfo, out IntPtr bits);
                Marshal.Copy(rawColor, 0, bits, rawColor.Length);

                // creating bmpColor
                bmpColor = Image.FromHbitmap(hbmpColor);

                User32.ReleaseDC(IntPtr.Zero, dcScreen);
                Gdi32.DeleteObject(hbmpColor);
                Gdi32.DeleteDC(dcColor);
            }

            #endregion

            #endregion

            #endregion
        }

        #endregion

        #endregion

        #region Fields

        private readonly RawIconImageCollection iconImages = new RawIconImageCollection();

        #endregion

        #region Properties

        internal int ImageCount => iconImages.Count;

        #endregion

        #region Construction and Destruction

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
        [SecurityCritical]
        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "False alarm, BinaryReader leaves the stream open.")]
        internal RawIcon(Icon icon, Size? size = null, int? bpp = null, int? index = null)
        {
            if (icon == null)
                throw new ArgumentNullException(nameof(icon), Res.ArgumentNull);

            // there is no icon stream - adding by bitmap
            if (!icon.HasRawData())
            {
                if (index.HasValue && index.Value != 0)
                    return;

                using (Bitmap bmp = icon.ToAlphaBitmap())
                {
                    if ((!size.HasValue || (size.Value.Width == bmp.Size.Width && size.Value.Height == bmp.Size.Height))
                        || (!bpp.HasValue || bpp.Value == bmp.PixelFormat.ToBitsPerPixel()))
                    {
                        Add(bmp);
                    }
                }

                return;
            }

            // initializing from stream
            using (MemoryStream ms = new MemoryStream())
            {
                icon.Save(ms);
                ms.Position = 0L;

                using (BinaryReader br = new BinaryReader(ms, Encoding.ASCII, true))
                {
                    Load(br, size, bpp, index);
                }
            }
        }

        #endregion

        #region Explicit Disposing

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            iconImages.Dispose();
        }

        #endregion

        #endregion

        #region Methods

        #region Internal Methods

        /// <summary>
        /// Adds an icon to the raw icon. <param name="icon"> is deserialized from stream.</param>
        /// </summary>
        [SecurityCritical]
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Disposing the new RawIcon would dispose the added images.")]
        internal void Add(Icon icon)
        {
            if (icon == null)
                throw new ArgumentNullException(nameof(icon), Res.ArgumentNull);

            // not in using so its images will not be disposed after adding them to self images
            foreach (RawIconImage image in new RawIcon(icon).iconImages)
            {
                iconImages.Add(image);
            }
        }

        /// <summary>
        /// Adds an image to the raw icon.
        /// </summary>
        internal void Add(Bitmap image)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image), Res.ArgumentNull);

            Add(image, image.PixelFormat.In(PixelFormat.Format32bppArgb, PixelFormat.Format32bppPArgb, PixelFormat.Format64bppArgb, PixelFormat.Format64bppPArgb) ? Color.Transparent : image.GetPixel(0, 0));
        }

        /// <summary>
        /// Adds an image to the raw icon.
        /// </summary>
        internal void Add(Bitmap image, Color transparentColor)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image), Res.ArgumentNull);

            int bpp = image.PixelFormat.ToBitsPerPixel();
            if (bpp.In(16, 48, 64))
                image = (Bitmap)image.ConvertPixelFormat(PixelFormat.Format32bppArgb, null);
            else
                image = (Bitmap)image.Clone();

            RawIconImage iconImage = new RawIconImage(image, transparentColor);
            iconImages.Add(iconImage);
        }

        /// <summary>
        /// Gets the icons of the <see cref="RawIcon"/> instance as a single, combined <see cref="Icon"/>.
        /// </summary>
        [SecurityCritical]
        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "False alarm, the BinaryWriter leaves the stream open.")]
        internal Icon ToIcon()
        {
            if (iconImages.Count == 0)
                return null;

            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms, Encoding.ASCII, true))
                {
                    Save(bw);
                    ms.Position = 0L;
                    return new Icon(ms);
                }
            }
        }

        /// <summary>
        /// Gets a <see cref="Bitmap"/> instance, which contains every images of the <see cref="RawIcon"/> instance as a single, multi-resolution <see cref="Bitmap"/>.
        /// </summary>
        [SecurityCritical]
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Stream must not be disposed; otherwise, a Generic GDI+ Error will occur when using the result Bitmap.")]
        internal Bitmap ToBitmap()
        {
            if (iconImages.Count == 0)
                return null;

            // not in using because stream must left open during the Bitmap lifetime
            var ms = new MemoryStream();
            {
                using (var bw = new BinaryWriter(ms, Encoding.ASCII, true))
                {
                    Save(bw);
                }

                ms.Position = 0L;
                return new Bitmap(ms);
            }
        }

        /// <summary>
        /// Saves the icon into a stream
        /// </summary>
        [SecurityCritical]
        internal void Save(Stream stream)
        {
            BinaryWriter bw = new BinaryWriter(stream);
            Save(bw);
        }

        /// <summary>
        /// Gets the icons of the <see cref="RawIcon"/> instance as separated <see cref="Icon"/> instances.
        /// </summary>
        [SecurityCritical]
        internal Icon[] ExtractIcons()
        {
            Icon[] result = new Icon[iconImages.Count];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = iconImages[i].ToIcon();
            }

            return result;
        }

        /// <summary>
        /// Gets the images of the <see cref="RawIcon"/> instance as separated <see cref="Bitmap"/> instances.
        /// </summary>
        [SecurityCritical]
        internal Bitmap[] ExtractBitmaps(bool keepOriginalFormat)
        {
            Bitmap[] result = new Bitmap[iconImages.Count];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = iconImages[i].ToBitmap(keepOriginalFormat);
            }

            return result;
        }

        /// <summary>
        /// Gets the nearest bitmap to the specified color depth and size. Bpp is matched first.
        /// </summary>
        [SecurityCritical]
        internal Bitmap ExtractNearestBitmap(int bpp, Size size, bool keepOriginalFormat)
        {
            if (iconImages.Count == 0)
                return null;

            if (iconImages.Count == 1)
                return iconImages[0].ToBitmap(keepOriginalFormat);

            return GetNearestImage(bpp, size).ToBitmap(keepOriginalFormat);
        }

        /// <summary>
        /// Gets the nearest icon to the specified color depth and size. Bpp is matched first.
        /// </summary>
        [SecurityCritical]
        internal Icon ExtractNearestIcon(int bpp, Size size)
        {
            if (iconImages.Count == 0)
                return null;

            if (iconImages.Count == 1)
                return iconImages[0].ToIcon();

            return GetNearestImage(bpp, size).ToIcon();
        }

        #endregion

        #region Private Methods

        [SecurityCritical]
        private void Save(BinaryWriter bw)
        {
            // Icon header
            ICONDIR iconDir = new ICONDIR
            {
                idReserved = 0,
                idType = 1,
                idCount = (ushort)iconImages.Count
            };

            bw.Write(BinarySerializer.SerializeValueType(iconDir));

            // Icon directory entries
            uint offset = (uint)(Marshal.SizeOf(typeof(ICONDIR)) + iconDir.idCount * Marshal.SizeOf(typeof(ICONDIRENTRY)));
            foreach (RawIconImage image in iconImages)
            {
                image.WriteDirEntry(bw, ref offset);
            }

            // Icon images
            foreach (RawIconImage image in iconImages)
            {
                image.WriteRawImage(bw);
            }
        }

        [SecurityCritical]
        private void Load(BinaryReader br, Size? size, int? bpp, int? index)
        {
            byte[] buf = br.ReadBytes(Marshal.SizeOf(typeof(ICONDIR)));
            ICONDIR iconDir = (ICONDIR)BinarySerializer.DeserializeValueType(typeof(ICONDIR), buf);
            if (iconDir.idReserved != 0 || iconDir.idType != 1)
                throw new ArgumentException(Res.RawIconBadIconFormat, nameof(br));

            if (index.HasValue && (index.Value < 0 || index.Value >= iconDir.idCount))
                return;

            int entryOffset = Marshal.SizeOf(typeof(ICONDIR));
            int entrySize = Marshal.SizeOf(typeof(ICONDIRENTRY));
            for (int i = 0; i < iconDir.idCount; i++)
            {
                if (index.HasValue && index.Value != i)
                {
                    entryOffset += entrySize;
                    continue;
                }

                br.BaseStream.Position = entryOffset;
                buf = br.ReadBytes(entrySize);
                ICONDIRENTRY entry = (ICONDIRENTRY)BinarySerializer.DeserializeValueType(typeof(ICONDIRENTRY), buf);
                if (entry.wBitCount > 32)
                    entry.wBitCount = 32;
                Size reqSize = size.GetValueOrDefault();
                int reqBpp = bpp.GetValueOrDefault();
                Size iconSize = new Size(entry.bWidth == 0 ? 256 : entry.bWidth, entry.bHeight == 0 ? 256 : entry.bHeight);
                if ((!size.HasValue || reqSize == iconSize) && (!bpp.HasValue || entry.wBitCount == 0 || reqBpp == entry.wBitCount))
                {
                    br.BaseStream.Position = entry.dwImageOffset;
                    RawIconImage image = new RawIconImage(br.ReadBytes((int)entry.dwBytesInRes));

                    // bpp was explicit defined, though there is 0 in dir entry: post-check
                    if (entry.wBitCount == 0 && reqBpp != 0 && image.RawBpp != reqBpp)
                        image.Dispose();
                    else
                        iconImages.Add(image);
                }

                entryOffset += entrySize;
            }

        }

        private RawIconImage GetNearestImage(int bpp, Size size)
        {
            int minBppDistance = Int32.MaxValue;
            CircularSortedList<int, RawIconImage> imagesBySize = new CircularSortedList<int, RawIconImage>();
            foreach (RawIconImage iconImage in iconImages)
            {
                int bppDistance = Math.Abs(bpp - iconImage.RawBpp);
                if (bppDistance > minBppDistance)
                    continue;

                // clearing list if closer bpp distance has been found
                if (bppDistance < minBppDistance)
                {
                    minBppDistance = bppDistance;
                    if (imagesBySize.Count > 0)
                        imagesBySize.Clear();
                }

                // from the same distances the last occurrence will win
                int sizeDistance = Math.Abs(size.Width - iconImage.Size.Width) + Math.Abs(size.Height - iconImage.Size.Height);
                imagesBySize[sizeDistance] = iconImage;
            }

            return imagesBySize.Values[0];
        }

        #endregion

        #endregion
    }
}
