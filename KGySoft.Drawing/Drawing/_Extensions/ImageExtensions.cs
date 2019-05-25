#region Used namespaces

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;

using KGySoft.Drawing.WinApi;
using KGySoft.Reflection;

#endregion

namespace KGySoft.Drawing
{
    /// <summary>
    /// Provides extension methods and other <see cref="Image"/> routines for <see cref="Image"/> class.
    /// </summary>
    public static class ImageExtensions
    {
        #region Fields

        private static FieldAccessor fieldColorPalette_entries;
        
        #endregion

        #region Methods

        #region Public Methods

        /// <summary>
        /// Converts an image to grayscale.
        /// </summary>
        /// <param name="image">The image to convert to grayscale</param>
        /// <returns>The grayscale version of the original <paramref name="image"/>.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The result must not be disposed.")]
        public static Image ToGrayscale(this Image image)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));

            //Set up the drawing surface
            Bitmap result = new Bitmap(image.Width, image.Height);
            using (Graphics g = Graphics.FromImage(result))
            {
                //Grayscale Color Matrix
                var colorMatrix = new ColorMatrix(new float[][]
                {
                    new float[] { 0.3f, 0.3f, 0.3f, 0, 0 },
                    new float[] { 0.59f, 0.59f, 0.59f, 0, 0 },
                    new float[] { 0.11f, 0.11f, 0.11f, 0, 0 },
                    new float[] { 0, 0, 0, 1, 0 },
                    new float[] { 0, 0, 0, 0, 1 }
                });

                //Create attributes
                using (var attrs = new ImageAttributes())
                {
                    attrs.SetColorMatrix(colorMatrix);

                    //Draw the image with the new attributes
                    g.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attrs);
                }
            }

            return result;
        }

        /// <summary>
        /// Converts the image to the desired <see cref="PixelFormat"/>.
        /// </summary>
        /// <param name="image">The original image to convert.</param>
        /// <param name="newPixelFormat">The desired new pixel format. If requested format is indexed, built-in strategies
        /// are used for the generated palette (see remarks).</param>
        /// <param name="palette">The required palette for the result image. If <see langword="null"/>, palette will be taken from source or will be generated on demand. If palette contains transparent color, it might be considered. If it contains too few elements,
        /// black entries will be added.</param>
        /// <returns>A new <see cref="Image"/> instance with the desired pixel format.</returns>
        /// <remarks>
        /// <para>If <paramref name="newPixelFormat"/> is <see cref="PixelFormat.Format8bppIndexed"/> and <paramref name="palette"/> is <see langword="null"/>, a standard palette will be used. Transparency will be preserved.</para>
        /// <para>If <paramref name="newPixelFormat"/> is <see cref="PixelFormat.Format4bppIndexed"/> and <paramref name="palette"/> is <see langword="null"/>, the standard 16 color palette will be used.</para>
        /// <para>If <paramref name="newPixelFormat"/> is <see cref="PixelFormat.Format1bppIndexed"/> and <paramref name="palette"/> is <see langword="null"/>, black and white colors will be used.</para>
        /// <para>If <paramref name="palette"/> contains the transparent color (<see cref="Color.Transparent"/>), and the source pixel format is <see cref="PixelFormat.Format32bppArgb"/>, and the target pixel format is indexed, the result will have transparency.</para>
        /// </remarks>
#if !NET35
        [SecuritySafeCritical]
#endif
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The result must not be disposed; bmp is disposed if it is not the same as image.")]
        public static Image ConvertPixelFormat(this Image image, PixelFormat newPixelFormat, Color[] palette)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));

            PixelFormat sourcePixelFormat = image.PixelFormat;
            //if (sourcePixelFormat == newPixelFormat)
            //    return (Image)image.Clone();

            int bpp = newPixelFormat.ToBitsPerPixel();
            if (newPixelFormat == PixelFormat.Format16bppArgb1555 || newPixelFormat == PixelFormat.Format16bppGrayScale)
                throw new NotSupportedException("This pixel format is not supported by GDI+");

            Bitmap result;

            // non-indexed target image (transparency preserved automatically)
            if (bpp > 8)
            {
                result = new Bitmap(image.Width, image.Height, newPixelFormat);
                using (Graphics g = Graphics.FromImage(result))
                {
                    g.DrawImage(image, 0, 0, image.Width, image.Height);
                }

                return result;
            }

            // 256 color image: when source has more colors and palette is not defined, saving as GIF so palette will be created internally (and image might be dithered)
            //int sourceBpp = sourcePixelFormat.ToBitsPerPixel();
            //if (bpp == 8 && palette == null && sourceBpp > 8)
            //{
            //    // because of Image.FromStream, the stream must not be diposed during the image lifetime,
            //    // othwerwise exceptions may come with "A generic error occurred in GDI+"
            //    MemoryStream ms = new MemoryStream();
            //    image.Save(ms, ImageFormat.Gif);
            //    ms.Position = 0L;
            //    result = (Bitmap)Image.FromStream(ms);

            //    // if source may have transparency (32/64 bpp), fixing the target image because GIF encoder fails to do it
            //    if (sourcePixelFormat.HasTransparency() || isMetafile)
            //    {
            //        // finding the transparent color in palette
            //        transparentIndex = Array.FindIndex(result.Palette.Entries, c => c.ToArgb() == 0);
            //        if (transparentIndex >= 0)
            //        {
            //            // when metafile, creating a bitmap with result size
            //            bmp = (image as Bitmap) ?? new Bitmap(image, result.Size);

            //            // source is never indexed here so it is enough to use ToIndexedTransparentByRgb
            //            ToIndexedTransparentByArgb(result, bmp, transparentIndex);

            //            if (!ReferenceEquals(image, bmp))
            //                bmp.Dispose();
            //        }
            //    }

            //    return result;
            //}

            // indexed colors: using GDI+ natively
            Bitmap bmp = image as Bitmap ?? new Bitmap(image);
            bool isMetafile = image is Metafile;
            var targetPalette = new RGBQUAD[256];
            int colorCount = InitPalette(targetPalette, bpp, isMetafile ? null : image.Palette, palette, out int transparentIndex);
            var bmi = new BITMAPINFO();
            bmi.icHeader.biSize = (uint)Marshal.SizeOf(typeof(BITMAPINFOHEADER));
            bmi.icHeader.biWidth = image.Width;
            bmi.icHeader.biHeight = image.Height;
            bmi.icHeader.biPlanes = 1;
            bmi.icHeader.biBitCount = (ushort)bpp;
            bmi.icHeader.biCompression = BitmapCompressionMode.BI_RGB;
            bmi.icHeader.biSizeImage = (uint)(((image.Width + 7) & 0xFFFFFFF8) * image.Height / (8 / bpp));
            bmi.icHeader.biXPelsPerMeter = 0;
            bmi.icHeader.biYPelsPerMeter = 0;
            bmi.icHeader.biClrUsed = (uint)colorCount;
            bmi.icHeader.biClrImportant = (uint)colorCount;
            bmi.icColors = targetPalette;

            //PixelFormat sourcePixelFormat = bmp.PixelFormat;

            // Creating the indexed bitmap
            IntPtr hbmResult = Gdi32.CreateDibSectionRgb(IntPtr.Zero, ref bmi, out var _);

            // Obtaining screen DC
            IntPtr dcScreen = User32.GetDC(IntPtr.Zero);

            // DC for the original hbitmap
            IntPtr hbmSource = bmp.GetHbitmap();
            IntPtr dcSource = Gdi32.CreateCompatibleDC(dcScreen);
            Gdi32.SelectObject(dcSource, hbmSource);

            // DC for the indexed hbitmap
            IntPtr dcTarget = Gdi32.CreateCompatibleDC(dcScreen);
            Gdi32.SelectObject(dcTarget, hbmResult);

            // Copy content
            Gdi32.BitBlt(dcTarget, 0, 0, image.Width, image.Height, dcSource, 0, 0);

            // obtaining result
            result = Image.FromHbitmap(hbmResult);
            result.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            // cleanup
            Gdi32.DeleteDC(dcSource);
            Gdi32.DeleteDC(dcTarget);
            User32.ReleaseDC(IntPtr.Zero, dcScreen);
            Gdi32.DeleteObject(hbmSource);
            Gdi32.DeleteObject(hbmResult);

            ColorPalette resultPalette = result.Palette;
            bool resetPalette = false;

            // compacting palette is possible (eg. 8 bpp image uses only 14 colors: truncating to 16 entries)
            if (colorCount <= result.Palette.Entries.Length >> 1)
            {
                Color[] truncatedPalette = resultPalette.Entries;
                int desiredSize = resultPalette.Entries.Length >> 1;
                while (desiredSize >> 1 >= colorCount)
                    desiredSize >>= 1;

                Array.Resize(ref truncatedPalette, desiredSize);
                if (fieldColorPalette_entries == null)
                    fieldColorPalette_entries = FieldAccessor.GetAccessor(typeof(ColorPalette).GetField("entries", BindingFlags.Instance | BindingFlags.NonPublic));

                fieldColorPalette_entries.Set(resultPalette, truncatedPalette);
                resetPalette = true;
            }

            // restoring transparency
            if (transparentIndex >= 0)
            {
                // updating palette if transparent color is not actually transparent
                if (resultPalette.Entries[transparentIndex].A != 0)
                {
                    resultPalette.Entries[transparentIndex] = Color.Transparent;
                    resetPalette = true;
                }

                if (sourcePixelFormat.HasTransparency() || isMetafile)
                    ToIndexedTransparentByArgb(result, bmp, transparentIndex);
                else if (sourcePixelFormat.ToBitsPerPixel() <= 8)
                {
                    int inputTransparentIndex = Array.FindIndex(bmp.Palette.Entries, c => c.A == 0);
                    if (inputTransparentIndex >= 0)
                        ToIndexedTransparentByIndexed(result, bmp, transparentIndex, inputTransparentIndex);
                }
            }

            if (resetPalette)
                result.Palette = resultPalette;

            if (!ReferenceEquals(bmp, image))
                bmp.Dispose();
            return result;
        }

        /// <summary>
        /// Compares an image to another by content and returns whether they are equal. Images of different
        /// size or pixel format are considered as difference.
        /// </summary>
        /// <param name="image1">First image instance.</param>
        /// <param name="image2">Second image instance.</param>
        /// <returns><see langword="true"/>&#160;if both images have the same content; otherwise, <see langword="false"/>.</returns>
        /// <remarks>If an image is not a <see cref="Bitmap"/> instance, a temporaly <see cref="Bitmap"/> is created for the check.
        /// <note>This method compares images by raw content. If an images has a padding in each stride (content row), padding content is considered as well.</note></remarks>
        public static bool EqualsByContent(this Image image1, Image image2)
        {
            return CompareImages(image1, image2);
        }

        /// <summary>
        /// Creates an <see cref="Icon" /> from an <see cref="Image" />.
        /// </summary>
        /// <param name="image">The image to be converted to an icon.</param>
        /// <param name="size">The required size of the icon. Must not be larger than 256.</param>
        /// <param name="keepAspectRatio">When source <paramref name="image"/> is not square sized, determines whether the image should keep aspect ratio.</param>
        /// <returns>An <see cref="Icon"/> instance created from the <paramref name="image"/>.</returns>
        /// <remarks>The result icon will be always square sized. To create a non-square icon, use <see cref="IconExtensions.Combine(Bitmap[])"/> instead.</remarks>
        public static Icon ToIcon(this Image image, int size, bool keepAspectRatio)
        {
            return IconExtensions.IconFromImage(image, size, keepAspectRatio);
        }

        /// <summary>
        /// Creates an <see cref="Icon" /> from an <see cref="Image" />.
        /// </summary>
        /// <param name="image">The image to be converted to an icon.</param>
        /// <param name="size">The required size of the icon.</param>
        /// <returns>An <see cref="Icon"/> instance created from the <paramref name="image"/>.</returns>
        /// <remarks>The result icon will be always sqaure sized. Original aspect ratio of the image is kept.</remarks>
        public static Icon ToIcon(this Image image, SystemIconSize size)
        {
            return IconExtensions.IconFromImage(image, size);
        }

        /// <summary>
        /// Saves the provided <paramref name="images"/> as a multipage TIFF into the specified <see cref="Stream"/>.
        /// When <see cref="Image"/> instances in <paramref name="images"/> contain already multiple pages, only the actual page is taken.
        /// </summary>
        /// <param name="images">The images to save into the TIFF data stream.</param>
        /// <param name="stream">The stream into the TIFF data is saved.</param>
        /// <remarks><para>When <paramref name="images"/> contain multi-page instances, this method takes only the current page. You can extract
        /// images by <see cref="BitmapExtensions.ExtractBitmaps"/> method.</para>
        /// <para>Compression mode and bit depth is chosen for each page based on pixel format.</para></remarks>
        public static void SaveAsMultipageTiff(this IEnumerable<Image> images, Stream stream)
        {
            if (images == null)
                throw new ArgumentNullException(nameof(images));
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            ImageCodecInfo tiffEncoder = ImageCodecInfo.GetImageEncoders().FirstOrDefault(e => e.FormatID == ImageFormat.Tiff.Guid);
            if (tiffEncoder == null)
                throw new InvalidOperationException("TIFF encoder not found");

            Image tiff = null;
            foreach (Image page in images)
            {
                if (page == null)
                    throw new ArgumentException("Collection contains null element", nameof(images));

                using (EncoderParameters encoderParams = new EncoderParameters(3))
                {
                    // LZW is always shorter, and non-BW palette is enabled, too
                    encoderParams.Param[0] = new EncoderParameter(Encoder.Compression, (long)(/*page.PixelFormat == PixelFormat.Format1bppIndexed ? EncoderValue.CompressionCCITT4 : */EncoderValue.CompressionLZW));
                    encoderParams.Param[1] = new EncoderParameter(Encoder.ColorDepth, page.PixelFormat.ToBitsPerPixel());

                    // saving the first page with MultiFrame parameter
                    if (tiff == null)
                    {
                        tiff = page;
                        encoderParams.Param[2] = new EncoderParameter(Encoder.SaveFlag, (long)EncoderValue.MultiFrame);
                        tiff.Save(stream, tiffEncoder, encoderParams);
                    }
                    // saving subsequent pages
                    else
                    {
                        encoderParams.Param[2] = new EncoderParameter(Encoder.SaveFlag, (long)EncoderValue.FrameDimensionPage);
                        tiff.SaveAdd(page, encoderParams);
                    }
                }
            }

            // finishing save
            using (EncoderParameters encoderParams = new EncoderParameters(1))
            {
                encoderParams.Param[0] = new EncoderParameter(Encoder.SaveFlag, (long)EncoderValue.Flush);
                // ReSharper disable PossibleNullReferenceException
                tiff.SaveAdd(encoderParams);
                // ReSharper restore PossibleNullReferenceException
            }

            stream.Flush();
        }

        /// <summary>
        /// Gets the bits per pixel (bpp) value of the image.
        /// </summary>
        public static int GetBitsPerPixel(this Image image)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));

            return image.PixelFormat.ToBitsPerPixel();
        }

        #endregion

        #region Private Methods

#if !NET35
        [SecuritySafeCritical]
#endif
        private static bool CompareImages(Image image1, Image image2)
        {
            if (ReferenceEquals(image1, image2))
                return true;

            if (image1 == null || image2 == null)
                return false;

            Type type1 = image1.GetType();
            Type type2 = image2.GetType();
            if (type1 != type2 || image1.Size != image2.Size || image1.PixelFormat != image2.PixelFormat || image1.RawFormat.Guid != image2.RawFormat.Guid)
                return false;

            if (type1 == typeof(Metafile))
            {
                using (MemoryStream ms1 = new MemoryStream())
                using (MemoryStream ms2 = new MemoryStream())
                {
                    ((Metafile)image1).Save(ms1);
                    ((Metafile)image2).Save(ms2);

                    if (ms1.Length != ms2.Length)
                        return false;

                    unsafe
                    {
                        fixed (byte* pbuf1 = ms1.GetBuffer())
                        fixed (byte* pbuf2 = ms2.GetBuffer())
                        {
                            return msvcrt.CompareMemory(new IntPtr(pbuf1), new IntPtr(pbuf2), ms1.Length);
                        }
                    }
                    //GCHandle pbuf1 = GCHandle.Alloc(ms1.GetBuffer(), GCHandleType.Pinned);
                    //try
                    //{
                    //    GCHandle pbuf2 = GCHandle.Alloc(ms2.GetBuffer(), GCHandleType.Pinned);
                    //    try
                    //    {
                    //        return msvcrt.CompareMemory(pbuf1.AddrOfPinnedObject(), pbuf2.AddrOfPinnedObject(), ms1.Length);
                    //    }
                    //    finally
                    //    {
                    //        pbuf2.Free();
                    //    }
                    //}
                    //finally
                    //{
                    //    pbuf1.Free();
                    //}
                }
            }

            Bitmap bmp1 = (Bitmap)image1;
            Bitmap bmp2 = (Bitmap)image2;

            BitmapData data1 = bmp1.LockBits(new Rectangle(new Point(0, 0), bmp1.Size), ImageLockMode.ReadOnly, bmp1.PixelFormat);
            BitmapData data2 = bmp2.LockBits(new Rectangle(new Point(0, 0), bmp2.Size), ImageLockMode.ReadOnly, bmp2.PixelFormat);

            try
            {
                if (data1.Stride != data2.Stride)
                    return false;

                // top-down image: can be compared in a whole
                if (data1.Stride > 0)
                    return msvcrt.CompareMemory(data1.Scan0, data2.Scan0, (long)data1.Stride * image1.Height);

                // bottom-up image: line by line
                int offset = 0;
                for (int i = 0; i < data1.Height; i++)
                {
                    IntPtr line1 = new IntPtr(data1.Scan0.ToInt64() + offset);
                    IntPtr line2 = new IntPtr(data2.Scan0.ToInt64() + offset);
                    if (!msvcrt.CompareMemory(line1, line2, -data1.Stride))
                        return false;

                    offset += data1.Stride;
                }

                return true;
            }
            finally
            {
                bmp1.UnlockBits(data1);
                bmp2.UnlockBits(data2);
            }
        }

        private static int InitPalette(RGBQUAD[] targetPalette, int bpp, ColorPalette originalPalette, Color[] desiredPalette, out int transparentIndex)
        {
            int maxColors = 1 << bpp;

            // using desired palette
            Color[] sourcePalette = desiredPalette;

            // or, using original palette if it has fewer or the same amount of colors as requested
            if (sourcePalette == null && originalPalette != null && originalPalette.Entries.Length > 0 && originalPalette.Entries.Length <= maxColors)
                sourcePalette = originalPalette.Entries;

            // or, using default system palette
            if (sourcePalette == null)
            {
                using (Bitmap bmpReference = new Bitmap(1, 1, bpp.ToPixelFormat()))
                {
                    sourcePalette = bmpReference.Palette.Entries;
                }
            }

            // it is ignored if source has too few colors (rest of the entries will be black)
            transparentIndex = -1;
            bool hasBlack = false;
            int colorCount = Math.Min(maxColors, sourcePalette.Length);
            for (int i = 0; i < colorCount; i++)
            {
                targetPalette[i] = new RGBQUAD(sourcePalette[i]);
                if (transparentIndex == -1 && sourcePalette[i].A == 0)
                    transparentIndex = i;
                if (!hasBlack && (sourcePalette[i].ToArgb() & 0xFFFFFF) == 0)
                    hasBlack = true;
            }

            // if transparent index is 0, relocating it and setting transparent index to 1
            if (transparentIndex == 0)
            {
                targetPalette[0] = targetPalette[1];
                transparentIndex = 1;
            }
            // otherwise, setting the color of transparent index the same as the previous color, so it will not be used during the conversion
            else if (transparentIndex != -1)
            {
                targetPalette[transparentIndex] = targetPalette[transparentIndex - 1];
            }

            // if black color is not found in palette, counting 1 extra colors because it can be used in conversion
            if (colorCount < maxColors && !hasBlack)
                colorCount++;

            return colorCount;
        }

        /// <summary>
        /// Makes an indexed bitmap transparent based on a non-indexed source
        /// </summary>
#if !NET35
        [SecuritySafeCritical]
#endif
        private static unsafe void ToIndexedTransparentByArgb(Bitmap target, Bitmap source, int transparentIndex)
        {
            Debug.Assert(target.Size == source.Size, "Sizes are different in ToIndexedTransparentByArgb");
            int sourceBpp = source.PixelFormat.ToBitsPerPixel();
            int targetBpp = target.PixelFormat.ToBitsPerPixel();

            Debug.Assert(sourceBpp >= 32, "Source bpp should be 32 or 64 in ToIndexedTransparentByArgb");
            Debug.Assert(targetBpp <= 8, "Target bpp should be 8 or less in ToIndexedTransparentByArgb");
            Debug.Assert(transparentIndex < (1 << targetBpp), "transparentIndex has too high value in ToIndexedTransparentByArgb");

            BitmapData dataTarget = target.LockBits(new Rectangle(Point.Empty, target.Size), ImageLockMode.ReadWrite, target.PixelFormat);
            BitmapData dataSource = source.LockBits(new Rectangle(Point.Empty, source.Size), ImageLockMode.ReadOnly, source.PixelFormat);
            try
            {
                byte* lineSource = (byte*)dataSource.Scan0;
                byte* lineTarget = (byte*)dataTarget.Scan0;
                bool is32Bpp = sourceBpp == 32;

                // scanning through the lines
                for (int y = 0; y < dataSource.Height; y++)
                {
                    // scanning through the pixels within the line
                    for (int x = 0; x < dataSource.Width; x++)
                    {
                        // testing if pixel is transparent (applies both argb and pargb)
                        if (is32Bpp && ((uint*)lineSource)[x] >> 24 == 0
                            || !is32Bpp && ((ulong*)lineSource)[x] >> 48 == 0UL)
                        {
                            switch (targetBpp)
                            {
                                case 8:
                                    lineTarget[x] = (byte)transparentIndex;
                                    break;
                                case 4:
                                    // First pixel is the high nibble
                                    int pos = x >> 1;
                                    byte nibbles = lineTarget[pos];
                                    if ((x & 1) == 0)
                                    {
                                        nibbles &= 0x0F;
                                        nibbles |= (byte)(transparentIndex << 4);
                                    }
                                    else
                                    {
                                        nibbles &= 0xF0;
                                        nibbles |= (byte)transparentIndex;
                                    }

                                    lineTarget[pos] = nibbles;
                                    break;
                                case 1:
                                    // First pixel is MSB.
                                    pos = x >> 3;
                                    byte mask = (byte)(128 >> (x & 7));
                                    if (transparentIndex == 0)
                                        lineTarget[pos] &= (byte)~mask;
                                    else
                                        lineTarget[pos] |= mask;
                                    break;
                            }
                        }
                    }

                    lineSource += dataSource.Stride;
                    lineTarget += dataTarget.Stride;
                }
            }
            finally
            {
                target.UnlockBits(dataTarget);
                source.UnlockBits(dataSource);
            }
        }

#if !NET35
        [SecuritySafeCritical]
#endif
        private static unsafe void ToIndexedTransparentByIndexed(Bitmap target, Bitmap source, int targetTransparentIndex, int sourceTransparentIndex)
        {
            Debug.Assert(target.Size == source.Size, "Sizes are different in ToIndexedTransparentByIndexed");
            int sourceBpp = source.PixelFormat.ToBitsPerPixel();
            int targetBpp = target.PixelFormat.ToBitsPerPixel();

            Debug.Assert(sourceBpp <= 8 && targetBpp <= 8, "Target and source bpp should be 8 or less in ToIndexedTransparentByIndexed");
            Debug.Assert(targetTransparentIndex < (1 << targetBpp) && sourceTransparentIndex < (1 << sourceBpp), "Target or source transparentIndex has too high value in ToIndexedTransparentByArgb");

            BitmapData dataTarget = target.LockBits(new Rectangle(Point.Empty, target.Size), ImageLockMode.ReadWrite, target.PixelFormat);
            BitmapData dataSource = source.LockBits(new Rectangle(Point.Empty, source.Size), ImageLockMode.ReadOnly, source.PixelFormat);
            try
            {
                byte* lineSource = (byte*)dataSource.Scan0;
                byte* lineTarget = (byte*)dataTarget.Scan0;

                // scanning through the lines
                for (int y = 0; y < dataSource.Height; y++)
                {
                    // scanning through the pixels within the line
                    for (int x = 0; x < dataSource.Width; x++)
                    {
                        bool transparent;
                        switch (sourceBpp)
                        {
                            case 8:
                                transparent = lineSource[x] == sourceTransparentIndex;
                                break;
                            case 4:
                                // First pixel is the high nibble
                                byte nibbles = lineSource[x >> 1];
                                transparent = (x & 1) == 0
                                    ? nibbles >> 4 == sourceTransparentIndex
                                    : (nibbles & 0x0F) == sourceTransparentIndex;
                                break;
                            case 1:
                                // First pixel is MSB.
                                byte mask = (byte)(128 >> (x & 7));
                                byte bits = lineSource[x >> 3];
                                transparent = sourceTransparentIndex == 0 ^ (bits & mask) != 0;
                                break;
                            default:
                                throw new ArgumentException("Source bitmap is not indexed", nameof(source));
                        }

                        // transparent pixel found
                        if (transparent)
                        {
                            switch (targetBpp)
                            {
                                case 8:
                                    lineTarget[x] = (byte)targetTransparentIndex;
                                    break;
                                case 4:
                                    // First pixel is the high nibble
                                    int pos = x >> 1;
                                    byte nibbles = lineTarget[pos];
                                    if ((x & 1) == 0)
                                    {
                                        nibbles &= 0x0F;
                                        nibbles |= (byte)(targetTransparentIndex << 4);
                                    }
                                    else
                                    {
                                        nibbles &= 0xF0;
                                        nibbles |= (byte)targetTransparentIndex;
                                    }

                                    lineTarget[pos] = nibbles;
                                    break;
                                case 1:
                                    // First pixel is MSB.
                                    pos = x >> 3;
                                    byte mask = (byte)(128 >> (x & 7));
                                    if (targetTransparentIndex == 0)
                                        lineTarget[pos] &= (byte)~mask;
                                    else
                                        lineTarget[pos] |= mask;
                                    break;
                            }
                        }
                    }

                    lineSource += dataSource.Stride;
                    lineTarget += dataTarget.Stride;
                }
            }
            finally
            {
                target.UnlockBits(dataTarget);
                source.UnlockBits(dataSource);
            }
        }

        #endregion

        #endregion
    }
}
