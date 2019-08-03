#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ImageExtensions.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2019 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution. If not, then this file is considered as
//  an illegal copy.
//
//  Unauthorized copying of this file, via any medium is strictly prohibited.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;

using KGySoft.CoreLibraries;
using KGySoft.Drawing.WinApi;

#endregion

namespace KGySoft.Drawing
{
    /// <summary>
    /// Contains extension methods for the <see cref="Image"/> type.
    /// </summary>
    public static class ImageExtensions
    {
        #region Methods

        #region Public Methods

        /// <summary>
        /// Converts an image to a grayscale one.
        /// </summary>
        /// <param name="image">The image to convert to grayscale.</param>
        /// <returns>The grayscale version of the original <paramref name="image"/>.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The result must not be disposed.")]
        public static Image ToGrayscale(this Image image)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image), PublicResources.ArgumentNull);

            Bitmap result = new Bitmap(image.Width, image.Height);
            using (Graphics g = Graphics.FromImage(result))
            {
                // Grayscale color matrix
                var colorMatrix = new ColorMatrix(new float[][]
                {
                    new float[] { 0.299f, 0.299f, 0.299f, 0, 0 },
                    new float[] { 0.587f, 0.587f, 0.587f, 0, 0 },
                    new float[] { 0.114f, 0.114f, 0.114f, 0, 0 },
                    new float[] { 0, 0, 0, 1, 0 },
                    new float[] { 0, 0, 0, 0, 1 }
                });

                using (var attrs = new ImageAttributes())
                {
                    attrs.SetColorMatrix(colorMatrix);
                    g.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attrs);
                }
            }

            return result;
        }

        /// <summary>
        /// Converts the image to another one with the desired <see cref="PixelFormat"/>.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="image">The original image to convert.</param>
        /// <param name="newPixelFormat">The desired new pixel format. If the requested format is an indexed one, built-in strategies
        /// are used for the generated palette. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="palette">The required palette for the result image. If <see langword="null"/>, the palette will be taken from source or will be generated on demand.
        /// If palette contains transparent color, it might be considered. If it contains too few elements black entries will be added.</param>
        /// <returns>A new <see cref="Image"/> instance with the desired pixel format.</returns>
        /// <remarks>
        /// <para>If <paramref name="newPixelFormat"/> is <see cref="PixelFormat.Format8bppIndexed"/>, <paramref name="image"/> has no palette and <paramref name="palette"/> is <see langword="null"/>, a standard palette will be used. Transparency will be preserved.</para>
        /// <para>If <paramref name="newPixelFormat"/> is <see cref="PixelFormat.Format4bppIndexed"/>, <paramref name="image"/> has no palette and <paramref name="palette"/> is <see langword="null"/>, the standard 16 color palette will be used.</para>
        /// <para>If <paramref name="newPixelFormat"/> is <see cref="PixelFormat.Format1bppIndexed"/>, <paramref name="image"/> has no palette and <paramref name="palette"/> is <see langword="null"/>, black and white colors will be used.</para>
        /// <para>If the target pixel format is indexed, <paramref name="palette"/> contains the transparent color (<see cref="Color.Transparent">Color.Transparent</see>), and the source has transparency, then the result will have transparency for fully transparent pixels.</para>
        /// </remarks>
#if !NET35
        [SecuritySafeCritical]
#endif
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The result must not be disposed; bmp is disposed if it is not the same as image.")]
        public static Bitmap ConvertPixelFormat(this Image image, PixelFormat newPixelFormat, Color[] palette = null)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image), PublicResources.ArgumentNull);
            if (!Enum<PixelFormat>.IsDefined(newPixelFormat))
                throw new ArgumentOutOfRangeException(nameof(newPixelFormat), PublicResources.EnumOutOfRange(newPixelFormat));

            PixelFormat sourcePixelFormat = image.PixelFormat;
            //if (sourcePixelFormat == newPixelFormat)
            //    return (Image)image.Clone();

            int bpp = newPixelFormat.ToBitsPerPixel();
            if (newPixelFormat.In(PixelFormat.Format16bppArgb1555, PixelFormat.Format16bppGrayScale))
                throw new NotSupportedException(Res.ImageExtensionsPixelFormatNotSupported(newPixelFormat));

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

            // indexed colors: using GDI+ natively
            Bitmap bmp = image as Bitmap ?? new Bitmap(image);
            bool isMetafile = image is Metafile;
            var targetPalette = new RGBQUAD[256];
            int colorCount = InitPalette(targetPalette, bpp, isMetafile ? null : image.Palette, palette, out int transparentIndex);
            var bmi = new BITMAPINFO
            {
                icHeader =
                {
                    biSize = (uint)Marshal.SizeOf(typeof(BITMAPINFOHEADER)),
                    biWidth = image.Width,
                    biHeight = image.Height,
                    biPlanes = 1,
                    biBitCount = (ushort)bpp,
                    biCompression = BitmapCompressionMode.BI_RGB,
                    biSizeImage = (uint)(((image.Width + 7) & 0xFFFFFFF8) * image.Height / (8 / bpp)),
                    biXPelsPerMeter = 0,
                    biYPelsPerMeter = 0,
                    biClrUsed = (uint)colorCount,
                    biClrImportant = (uint)colorCount
                },
                icColors = targetPalette
            };

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
                resultPalette.SetEntries(truncatedPalette);
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
        /// Compares an image to another one by content and returns whether they are equal. Images of different
        /// size or pixel format are considered as difference.
        /// </summary>
        /// <param name="image1">First image instance.</param>
        /// <param name="image2">Second image instance.</param>
        /// <returns><see langword="true"/>&#160;if both images have the same content; otherwise, <see langword="false"/>.</returns>
        /// <remarks>If an image is not a <see cref="Bitmap"/> instance, a temporary <see cref="Bitmap"/> is created for the check.
        /// <note>This method compares images by raw content. If the images have padding in each stride (content row), padding content is considered as well.</note></remarks>
        public static bool EqualsByContent(this Image image1, Image image2) => CompareImages(image1, image2);

        /// <summary>
        /// Creates an <see cref="Icon" /> from an <see cref="Image" />.
        /// </summary>
        /// <param name="image">The image to be converted to an icon.</param>
        /// <param name="size">The required size of the icon.</param>
        /// <param name="keepAspectRatio">When source <paramref name="image"/> is not square sized, determines whether the image should keep aspect ratio.</param>
        /// <returns>An <see cref="Icon"/> instance created from the <paramref name="image"/>.</returns>
        /// <remarks>The result icon will be always square sized. To create a non-squared icon, use the <see cref="Icons.Combine(Bitmap[])">Icons.Combine</see> method instead.</remarks>
#if !NET35
        [SecuritySafeCritical]
#endif
        public static Icon ToIcon(this Image image, int size, bool keepAspectRatio) => Icons.FromImage(image, size, keepAspectRatio);

        /// <summary>
        /// Saves the provided <paramref name="images"/> as a multi-page TIFF into the specified <see cref="Stream"/>.
        /// When <see cref="Image"/> instances in <paramref name="images"/> contain already multiple pages, only the current page is taken.
        /// </summary>
        /// <param name="images">The images to save into the TIFF data stream.</param>
        /// <param name="stream">The stream into the TIFF data is to be saved.</param>
        /// <remarks><para>When <paramref name="images"/> contain multi-page instances, this method takes only the current page. You can extract
        /// images by <see cref="BitmapExtensions.ExtractBitmaps">ExtractBitmaps</see> extension method.</para>
        /// <para>Compression mode and bit depth is chosen for each page based on pixel format.</para></remarks>
        public static void SaveAsMultipageTiff(this IEnumerable<Image> images, Stream stream)
        {
            if (images == null)
                throw new ArgumentNullException(nameof(images), PublicResources.ArgumentNull);
            if (stream == null)
                throw new ArgumentNullException(nameof(stream), PublicResources.ArgumentNull);

            ImageCodecInfo tiffEncoder = ImageCodecInfo.GetImageEncoders().FirstOrDefault(e => e.FormatID == ImageFormat.Tiff.Guid);
            if (tiffEncoder == null)
                throw new InvalidOperationException(Res.ImageExtensionsNoTiffEncoder);

            Image tiff = null;
            foreach (Image page in images)
            {
                if (page == null)
                    throw new ArgumentException(PublicResources.ArgumentContainsNull, nameof(images));

                using (var encoderParams = new EncoderParameters(3))
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
            using (var encoderParams = new EncoderParameters(1))
            {
                encoderParams.Param[0] = new EncoderParameter(Encoder.SaveFlag, (long)EncoderValue.Flush);
                // ReSharper disable once PossibleNullReferenceException
                tiff.SaveAdd(encoderParams);
            }

            stream.Flush();
        }

        /// <summary>
        /// Saves the specified <paramref name="image"/> as a GIF image.
        /// <br/>See the <strong>Remarks</strong> section for the differences compared to the <see cref="Image.Save(Stream,ImageFormat)">Image.Save(Stream,ImageFormat)</see> method.
        /// </summary>
        /// <param name="image">The image to save. If image contains multiple images other than animated GIF frames, then only the current image will be saved.</param>
        /// <param name="stream">The stream to save the image into.</param>
        /// <param name="allowDithering"><see langword="true"/>&#160; to allow dithering high color images using a fix palette; otherwise, <see langword="false"/>. This parameter is optional.
        /// <br/>Default value: <see langword="false"/>.</param>
        /// <remarks>
        /// <para>When an image is saved by the <see cref="Image.Save(Stream,ImageFormat)">Image.Save(Stream,ImageFormat)</see> method using the GIF image format, then
        /// the original palette of an indexed source image and transparency can be lost in many cases.
        /// Unless the source image is already a 8 bpp one, the built-in encoder will use a fixed palette and dithers the image,
        /// while transparency will be lost.</para>
        /// <para>This method preserves transparency of fully transparent pixels even if <paramref name="allowDithering"/> is <see langword="true"/>.</para>
        /// </remarks>
        public static void SaveAsGif(this Image image, Stream stream, bool allowDithering = false)
            => SaveAsGif(image, stream, null, allowDithering);

        /// <summary>
        /// Saves the specified <paramref name="image"/> as a GIF image.
        /// <br/>See the <strong>Remarks</strong> section for the differences compared to the <see cref="Image.Save(Stream,ImageFormat)">Image.Save(Stream,ImageFormat)</see> method.
        /// </summary>
        /// <param name="image">The image to save. If image contains multiple images or frames, then the current image will be saved. Animated GIF can be saved only if <paramref name="palette"/> is <see langword="null"/>.</param>
        /// <param name="stream">The stream to save the image into.</param>
        /// <param name="palette">The desired custom palette to use. If <see langword="null"/>, and a palette cannot be taken from the source image, then a default palette will be used.</param>
        /// <remarks>
        /// <para>When an image is saved by the <see cref="Image.Save(Stream,ImageFormat)">Image.Save(Stream,ImageFormat)</see> method using the GIF image format, then
        /// the original palette of an indexed source image and transparency can be lost in many cases.
        /// Unless the source image is already a 8 bpp one, the built-in encoder will use a fixed palette and dithers the image,
        /// while transparency will be lost.</para>
        /// <para>This method preserves transparency of fully transparent pixels unless <paramref name="palette"/> is specified and does not contain the transparent color.</para>
        /// </remarks>
        public static void SaveAsGif(this Image image, Stream stream, Color[] palette)
            => SaveAsGif(image, stream, palette, false);

        /// <summary>
        /// Gets the bits per pixel (bpp) value of the image.
        /// </summary>
        /// <param name="image">The image to obtain the bits-per-pixel value from.</param>
        /// <returns>The bits per pixel (bpp) value of the image.</returns>
        public static int GetBitsPerPixel(this Image image)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image), PublicResources.ArgumentNull);

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
                            return msvcrt.CompareMemory(new IntPtr(pbuf1), new IntPtr(pbuf2), ms1.Length);
                    }
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
                    sourcePalette = bmpReference.Palette.Entries;
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

                // ReSharper disable PossibleNullReferenceException
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
                // ReSharper restore PossibleNullReferenceException
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

                // ReSharper disable PossibleNullReferenceException
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
                                // internal error, no res is needed
                                throw new InvalidOperationException("Unexpected bits per pixel");
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
                // ReSharper restore PossibleNullReferenceException
            }
            finally
            {
                target.UnlockBits(dataTarget);
                source.UnlockBits(dataSource);
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "MemoryStream in using passed to Bitmap constructor. MemoryStream is not sensitive to multiple closing.")]
        private static void SaveAsGif(Image image, Stream stream, Color[] palette, bool allowDithering)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image), PublicResources.ArgumentNull);
            if (stream == null)
                throw new ArgumentNullException(nameof(stream), PublicResources.ArgumentNull);

            ImageCodecInfo gifEncoder = ImageCodecInfo.GetImageEncoders().FirstOrDefault(e => e.FormatID == ImageFormat.Gif.Guid);
            if (gifEncoder == null)
                throw new InvalidOperationException(Res.ImageExtensionsNoGifEncoder);

            Guid format = image.RawFormat.Guid;
            PixelFormat pixelFormat = image.PixelFormat;
            int bpp = pixelFormat.ToBitsPerPixel();

            // 0.) Metafile: recursion with bitmap
            if (image is Metafile)
            {
                using (var bmp = new Bitmap(image, image.Size))
                {
                    SaveAsGif(bmp, stream, palette, allowDithering);
                    return;
                }
            }

            // 1.) Simply saving by GIF encoder (handles also animated GIFs), if there is no custom palette, and...
            if (palette.IsNullOrEmpty() && (
                // ... image is already a GIF or 8bpp memory BMP...
                (format == ImageFormat.Gif.Guid || (bpp == 8 && format == ImageFormat.MemoryBmp.Guid))
                // ... or image is not an indexed one, dithering is allowed and the source cannot have transparency
                || (bpp > 8 && allowDithering && !Image.IsAlphaPixelFormat(pixelFormat))))
            {
                image.Save(stream, gifEncoder, null);
                return;
            }

            // 2.) Indexed image or hi-color image without dithering: converting to 8bpp with desired or original palette.
            //     Transparency is preserved if the palette has a transparent color.
            if (bpp <= 8 || !allowDithering)
            {
                using (Image image8Bpp = image.ConvertPixelFormat(PixelFormat.Format8bppIndexed, palette))
                {
                    image8Bpp.Save(stream, gifEncoder, null);
                    return;
                }
            }

            // 3.) Hi-color image with dithering and transparency
            using (var ms = new MemoryStream())
            {
                // a.) saving by GIF encoder into a temp stream, which makes a dithered image with no transparency
                image.Save(ms, gifEncoder, null);

                // b.) reloading the stream as a bitmap with the dithered image
                ms.Position = 0L;
                using (var ditheredGif = new Bitmap(ms))
                {
                    int transparentIndex = Array.FindIndex(ditheredGif.Palette.Entries, c => c.A == 0);

                    // c.) restoring transparency in the dithered image
                    if (transparentIndex >= 0)
                        ToIndexedTransparentByArgb(ditheredGif, (Bitmap)image, transparentIndex);

                    // d.) saving the restored transparent image
                    ditheredGif.Save(stream, gifEncoder, null);
                }
            }
        }

        #endregion

        #endregion
    }
}
