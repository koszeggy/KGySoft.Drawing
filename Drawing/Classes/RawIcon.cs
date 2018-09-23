#region Used namespaces

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Collections.ObjectModel;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using KGySoft.Collections;
using KGySoft.Drawing.WinApi;
using KGySoft.Libraries;
using KGySoft.Libraries.Serialization;

#endregion

namespace KGySoft.Drawing
{
    /// <summary>
    /// Provides low-level support for an icon. This class is internal because it can process and produce <see cref="Icon"/> and <see cref="Bitmap"/>
    /// instances and every of its functionality is accessible via extensions on these classes.
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
                    throw new NotSupportedException("Too many images in the icon collection");
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
            /// In: Source image if it is already 32 bit ARGB and Color.Transparent is required for transparency
            /// Out: Result image of ToBitmap(false)
            /// </summary>
            private Bitmap bmpComposit;

            /// <summary>
            /// In: Source image if it is non ARGB or when a transparent color is specified
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

            private bool isPng;

            /// <summary>
            /// The bit count stored by the dir entry. Needed when actual format is PNG.
            /// </summary>
            private int dirEntryBitCountPng;

            private Size size;

            #endregion

            #region Properties

            #region Internal Properties

            /// <summary>
            /// Gets the bpp from raw data.
            /// </summary>
            internal int RawBpp
            {
                get { return isPng ? dirEntryBitCountPng : bmpHeader.biBitCount; }
            }

            /// <summary>
            /// Gets the size in pixels
            /// </summary>
            internal Size Size
            {
                get { return size; }
            }

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
                    Bitmap bmp = bmpColor ?? bmpComposit;
                    if (bmp != null)
                    {
                        int bpp = bmp.PixelFormat.ToBitsPerPixel();
                        return bpp > 8 ? 0 : 1 << bpp;
                    }

                    throw new ObjectDisposedException(ToString());
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
                if (bmpColor.PixelFormat == PixelFormat.Format32bppArgb &&
                    (transparentColor == Color.Transparent || transparentColor == Color.Empty))
                {
                    bmpComposit = bmpColor;
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
                    throw new ArgumentException("Bad icon format", "rawData");

                // header
                bmpHeader = (BITMAPINFOHEADER)BinarySerializer.DeserializeStruct(typeof(BITMAPINFOHEADER), rawData);
                size = new Size(bmpHeader.biWidth, bmpHeader.biHeight >> 1); // height is doubled because of mask
                int offset = signature;

                // palette
                int colorCount = PaletteColorCount;
                if (colorCount > 0)
                {
                    palette = BinarySerializer.DeserializeStructArray<RGBQUAD>(rawData, offset, colorCount);
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
            /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged resources; <see langword="false"/> to release only unmanaged resources.</param>
            private void Dispose(bool disposing)
            {
                if (disposing)
                {
                    if (bmpColor != null)
                    {
                        bmpColor.Dispose();
                        bmpColor = null;
                    }

                    if (bmpComposit != null)
                    {
                        bmpComposit.Dispose();
                        bmpComposit = null;
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

                bw.Write(BinarySerializer.SerializeStruct(entry));
                offset += entry.dwBytesInRes;
            }

            internal void WriteRawImage(BinaryWriter bw)
            {
                AssureRawFormatGenerated();
                if (isPng)
                {
                    bw.Write(rawColor);
                    return;
                }

                // header
                bw.Write(BinarySerializer.SerializeStruct(bmpHeader));

                // Palette
                if (PaletteColorCount > 0)
                    bw.Write(BinarySerializer.SerializeStructArray(palette));

                // color image (XOR)
                bw.Write(rawColor);

                // mask image (AND)
                bw.Write(rawMask);
            }

            internal Icon ToIcon()
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    using (BinaryWriter bw = new BinaryWriter(ms))
                    {
                        // header
                        ICONDIR iconDir = new ICONDIR
                        {
                            idReserved = 0,
                            idType = 1,
                            idCount = 1
                        };

                        bw.Write(BinarySerializer.SerializeStruct(iconDir));

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

            internal Bitmap ToBitmap(bool keepOriginalFormat)
            {
                AssureBitmapsGenerated(!keepOriginalFormat);
                if (keepOriginalFormat)
                    return (Bitmap)bmpColor.Clone();

                // When not original format is requested, returning a new bitmap instead of cloning for PNGs,
                // because for PNG images may couse troubles in some cases (eg. OutOfMemoryException when used as background image)
                if (bmpComposit.RawFormat.Guid == ImageFormat.Png.Guid)
                    return new Bitmap(bmpComposit);
                return (Bitmap)bmpComposit.Clone();
            }

            #endregion

            #region Private Methods

            private void AssureRawFormatGenerated()
            {
                // exeiting, if raw data is already generated
                if (rawColor != null)
                    return;

                // if both raw and image data is null, then object is disposed
                if (bmpColor == null && bmpComposit == null)
                    throw new ObjectDisposedException(ToString());

                if (isPng)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        // When PNG, using composit image at the first place
                        (bmpComposit ?? bmpColor).Save(ms, ImageFormat.Png);
                        rawColor = ms.ToArray();
                        return;
                    }
                }

                // image should be rotated so cloning
                Bitmap bmp = (Bitmap)(bmpColor ?? bmpComposit).Clone();
                Bitmap mask = null;
                try
                {
                    // rotating
                    FlipImageY(bmp);

                    // palette
                    int bpp = bmp.PixelFormat.ToBitsPerPixel();
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
                    bmpHeader.biCompression = Constants.BI_RGB;
                    bmpHeader.biXPelsPerMeter = 0;
                    bmpHeader.biYPelsPerMeter = 0;
                    bmpHeader.biClrUsed = (uint)PaletteColorCount;
                    bmpHeader.biClrImportant = 0;

                    // Color image (XOR)
                    BitmapData dataColor = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, bmp.PixelFormat);
                    IntPtr scanColor = dataColor.Scan0;
                    rawColor = new byte[Math.Abs(dataColor.Stride) * dataColor.Height];
                    Marshal.Copy(scanColor, rawColor, 0, rawColor.Length);
                    bmp.UnlockBits(dataColor);
                    bmpHeader.biSizeImage = (uint)rawColor.Length;

                    // Mask image (AND): Creting from color image.
                    mask = new Bitmap(bmp.Width, bmp.Height, PixelFormat.Format1bppIndexed);
                    BitmapData dataMask = mask.LockBits(new Rectangle(0, 0, mask.Width, mask.Height), ImageLockMode.ReadWrite, PixelFormat.Format1bppIndexed);
                    rawMask = new byte[Math.Abs(dataMask.Stride) * dataMask.Height];
                    int strideColor = Math.Abs(dataColor.Stride);
                    int strideMask = Math.Abs(dataMask.Stride);

                    // If the image bpp is less than 32, transparent color cannot have transparency
                    if (bpp < 32)
                        transparentColor = Color.FromArgb(255, transparentColor.R, transparentColor.G, transparentColor.B);

                    for (int y = 0; y < dataColor.Height; y++)
                    {
                        int posColorY = strideColor * y;
                        int posMaskY = strideMask * y;
                        for (int x = 0; x < dataColor.Width; x++)
                        {
                            int color;
                            RGBQUAD paletteColor;
                            switch (bpp)
                            {
                                case 1:
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
                                    throw new NotSupportedException("16/48/64 bpp images are not supported for Icons");
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

                    mask.UnlockBits(dataMask);
                }
                finally
                {
                    if (bmp != null)
                        bmp.Dispose();
                    if (mask != null)
                        mask.Dispose();
                }
            }

            private void AssureBitmapsGenerated(bool isCompositRequired)
            {
                if (rawColor == null && bmpColor == null && bmpComposit == null)
                    throw new ObjectDisposedException(ToString());

                // exiting, if the requested bitmap already exists
                if (isCompositRequired && bmpComposit != null || !isCompositRequired && bmpColor != null)
                    return;

                // PNG format
                if (isPng)
                {
                    Bitmap result = bmpComposit ?? bmpColor;

                    if (result == null)
                    {
                        // raw image is available, otherwise, object would be disposed
                        using (MemoryStream ms = new MemoryStream(rawColor))
                        {
                            result = new Bitmap(ms);
                        }
                    }

                    // assignments below will not replace any instace, otherwise, we would have returned above
                    if (isCompositRequired)
                        bmpComposit = result;
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
                    bmpComposit = icon.ToBitmap();
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
                IntPtr bits;
                IntPtr hbmpColor = Gdi32.CreateDIBSection(dcColor, ref bitmapInfo, Constants.DIB_RGB_COLORS, out bits, IntPtr.Zero, 0);
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

        internal int ImageCount
        {
            get { return iconImages.Count; }
        }

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
        internal RawIcon(Icon icon, Size? size, int? bpp, int? index)
        {
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

                using (BinaryReader br = new BinaryReader(ms))
                {
                    Load(br, size, bpp, index);
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RawIcon"/> class from an <see cref="Icon"/>.
        /// </summary>
        internal RawIcon(Icon icon)
            : this(icon, null, null, null)
        {
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
        internal void Add(Icon icon)
        {
            if (icon == null)
                throw new ArgumentNullException("icon");

            // not in using so its images will not be disposed after adding them to self images
            RawIcon rawIconToAdd = new RawIcon(icon, null, null, null);
            foreach (RawIconImage image in rawIconToAdd.iconImages)
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
                throw new ArgumentNullException("image");

            if (image.PixelFormat == PixelFormat.Format32bppArgb || image.PixelFormat == PixelFormat.Format32bppPArgb)
                Add(image, Color.Transparent);
            else
                Add(image, image.GetPixel(0, 0));
        }

        /// <summary>
        /// Adds an image to the raw icon.
        /// </summary>
        internal void Add(Bitmap image, Color transparentColor)
        {
            if (image == null)
                throw new ArgumentNullException("image");

            if (image.Width > 256 || image.Height > 256)
                throw new ArgumentException("Image is too big", "image");

            int bpp = image.PixelFormat.ToBitsPerPixel();
            if (bpp.In(16, 48, 64))
                image = (Bitmap)image.ConvertPixelFormat(PixelFormat.Format32bppArgb, null);

            RawIconImage iconImage = new RawIconImage((Bitmap)image.Clone(), transparentColor);
            iconImages.Add(iconImage);
        }

        /// <summary>
        /// Gets the icons of the <see cref="RawIcon"/> instance as a single, combined <see cref="Icon"/>.
        /// </summary>
        internal Icon ToIcon()
        {
            if (iconImages.Count == 0)
                return null;

            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
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
        internal Bitmap ToBitmap()
        {
            if (iconImages.Count == 0)
                return null;

            // not in using because stream must left open during the Bitmap lifetime
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);
            Save(bw);
            ms.Position = 0L;
            return new Bitmap(ms);
        }

        /// <summary>
        /// Saves the icon into a stream
        /// </summary>
        internal void Save(Stream stream)
        {
            BinaryWriter bw = new BinaryWriter(stream);
            Save(bw);
        }

        /// <summary>
        /// Gets the icons of the <see cref="RawIcon"/> instance as separated <see cref="Icon"/> instances.
        /// </summary>
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

        private void Save(BinaryWriter bw)
        {
            // Icon header
            ICONDIR iconDir = new ICONDIR
            {
                idReserved = 0,
                idType = 1,
                idCount = (ushort)iconImages.Count
            };

            bw.Write(BinarySerializer.SerializeStruct(iconDir));

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

        private void Load(BinaryReader br, Size? size, int? bpp, int? index)
        {
            byte[] buf = br.ReadBytes(Marshal.SizeOf(typeof(ICONDIR)));
            ICONDIR iconDir = (ICONDIR)BinarySerializer.DeserializeStruct(typeof(ICONDIR), buf);
            if (iconDir.idReserved != 0 || iconDir.idType != 1)
                throw new ArgumentException("Bad icon format", "br");

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
                ICONDIRENTRY entry = (ICONDIRENTRY)BinarySerializer.DeserializeStruct(typeof(ICONDIRENTRY), buf);
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

                // from the same distanes the last occurance will win
                int sizeDistance = Math.Abs(size.Width - iconImage.Size.Width) + Math.Abs(size.Height - iconImage.Size.Height);
                imagesBySize[sizeDistance] = iconImage;
            }

            return imagesBySize.Values[0];
        }

        #endregion

        #endregion
    }
}
