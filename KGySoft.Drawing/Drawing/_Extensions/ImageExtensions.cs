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
using KGySoft.Drawing.Imaging;
using KGySoft.Drawing.WinApi;

#endregion

namespace KGySoft.Drawing
{
    /// <summary>
    /// Contains extension methods for the <see cref="Image"/> type.
    /// </summary>
    public static class ImageExtensions
    {
        #region Constants

        private const int parallelThreshold = 100;

        #endregion

        #region Methods

        #region Public Methods

        /// <summary>
        /// Converts an image to a grayscale one.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="image">The image to convert to grayscale.</param>
        /// <returns>An <see cref="Image"/> containing the grayscale version of the original <paramref name="image"/>.</returns>
        /// <remarks>
        /// <para>This method always returns a <see cref="Bitmap"/> with <see cref="PixelFormat.Format32bppArgb"/> pixel format.</para>
        /// <para>To return a <see cref="Bitmap"/> with arbitrary <see cref="PixelFormat"/> use the <see cref="O:KGySoft.Drawing.ImageExtensions.ConvertPixelFormat"/> overloads with a grayscale palette, quantizer or pixel format.</para>
        /// <para>To make a <see cref="Bitmap"/> grayscale without creating a new instance use the <see cref="BitmapExtensions.MakeGrayscale">BitmapExtensions.MakeGrayscale</see> method.</para>
        /// </remarks>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The result must not be disposed.")]
        public static Image ToGrayscale(this Image image)
            => image.ConvertPixelFormat(PixelFormat.Format32bppArgb, PredefinedColorsQuantizer.FromCustomFunction(c => c.ToGray()));

        /// <summary>
        /// Converts the specified <paramref name="image"/> to a <see cref="Bitmap"/> with the desired <see cref="PixelFormat"/>.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="image">The original image to convert.</param>
        /// <param name="newPixelFormat">The desired new pixel format.</param>
        /// <param name="palette">The desired target palette if <paramref name="newPixelFormat"/> is an indexed format. If <see langword="null"/>, then
        /// and <paramref name="image"/> also has a palette of no more entries than the target indexed format can have, then the source palette will be used.
        /// Otherwise, a default palette will be used based on <paramref name="newPixelFormat"/>.</param>
        /// <param name="backColor">If <paramref name="newPixelFormat"/> does not have alpha or has only single-bit alpha, then specifies the color of the background.
        /// Source pixels with alpha above the <paramref name="alphaThreshold"/> will be blended with this color before setting the pixel in the result image.
        /// The <see cref="Color.A"/> property of the background color is ignored. This parameter is optional.
        /// <br/>Default value: <see cref="Color.Empty"/>, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="alphaThreshold">If <paramref name="newPixelFormat"/> can represent only single-bit alpha or <paramref name="newPixelFormat"/> is an indexed format and the target palette contains a transparent color,
        /// then specifies a threshold value for the <see cref="Color.A">Color.A</see> property, under which the color is considered to be transparent. If <c>0</c>,
        /// then the result will not have transparent pixels. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <returns>A new <see cref="Bitmap"/> instance with the desired pixel format.</returns>
        /// <remarks>
        /// <para>If <paramref name="newPixelFormat"/> can represent fewer colors than the source format, then a default
        /// quantization will occur during the conversion. To use a specific quantizer (and optionally a ditherer) use the <see cref="ConvertPixelFormat(Image,PixelFormat,IQuantizer,IDitherer)"/> overload.
        /// To use a quantizer with a specific palette you can use the <see cref="PredefinedColorsQuantizer"/> class.</para>
        /// <para>If <paramref name="newPixelFormat"/> is <see cref="PixelFormat.Format8bppIndexed"/>, <paramref name="image"/> has no palette and <paramref name="palette"/> is <see langword="null"/>, then the standard 256 color palette will be used.
        /// On Windows this contains the web-safe palette, the standard 16 Windows colors and the transparent color.</para>
        /// <para>If <paramref name="newPixelFormat"/> is <see cref="PixelFormat.Format4bppIndexed"/>, <paramref name="image"/> has no palette and <paramref name="palette"/> is <see langword="null"/>, then the standard 16 color palette will be used.</para>
        /// <para>If <paramref name="newPixelFormat"/> is <see cref="PixelFormat.Format1bppIndexed"/>, <paramref name="image"/> has no palette and <paramref name="palette"/> is <see langword="null"/>, then black and white colors will be used.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="image"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="newPixelFormat"/> is out of the defined values.</exception>
        /// <exception cref="ArgumentException"><paramref name="palette"/> contains too many colors for the indexed format specified by <paramref name="newPixelFormat"/>.</exception>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The result must not be disposed; bmp is disposed if it is not the same as image.")]
        public static Bitmap ConvertPixelFormat(this Image image, PixelFormat newPixelFormat, Color[] palette, Color backColor = default, byte alphaThreshold = 128)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image), PublicResources.ArgumentNull);
            if (!Enum<PixelFormat>.IsDefined(newPixelFormat))
                throw new ArgumentOutOfRangeException(nameof(newPixelFormat), PublicResources.EnumOutOfRange(newPixelFormat));

            Bitmap bmp = image as Bitmap ?? new Bitmap(image);
            Bitmap result = null;

            try
            {
                result = new Bitmap(image.Width, image.Height, newPixelFormat);

                // validating and initializing palette
                if (newPixelFormat.IsIndexed())
                    InitPalette(newPixelFormat, bmp, result, palette);

                using (BitmapDataAccessorBase source = BitmapDataAccessorFactory.CreateAccessor(bmp, ImageLockMode.ReadOnly))
                using (BitmapDataAccessorBase target = BitmapDataAccessorFactory.CreateAccessor(result, ImageLockMode.WriteOnly, new Color32(backColor), alphaThreshold))
                {
                    // Sequential processing
                    if (source.Width < parallelThreshold)
                    {
                        BitmapDataRowBase rowSrc = source.GetRow(0);
                        BitmapDataRowBase rowDst = target.GetRow(0);
                        int width = source.Width;
                        do
                        {
                            for (int x = 0; x < width; x++)
                                rowDst.DoSetColor32(x, rowSrc.DoGetColor32(x));
                        } while (rowSrc.MoveNextRow() && rowDst.MoveNextRow());
                    }
                    // Parallel processing
                    else
                    {
                        ParallelHelper.For(0, source.Height, y =>
                        {
                            BitmapDataRowBase rowSrc = source.GetRow(y);
                            BitmapDataRowBase rowDst = target.GetRow(y);
                            int width = source.Width;
                            for (int x = 0; x < width; x++)
                                rowDst.DoSetColor32(x, rowSrc.DoGetColor32(x));
                        });
                    }
                }

                return result;
            }
            catch (Exception)
            {
                result?.Dispose();
                throw;
            }
            finally
            {
                if (!ReferenceEquals(bmp, image))
                    bmp.Dispose();
            }
        }

        /// <summary>
        /// Converts the specified <paramref name="image"/> to a <see cref="Bitmap"/> with the desired <see cref="PixelFormat"/>.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="image">The original image to convert.</param>
        /// <param name="newPixelFormat">The desired new pixel format.</param>
        /// <param name="backColor">If <paramref name="newPixelFormat"/> does not have alpha or has only single-bit alpha, then specifies the color of the background.
        /// Source pixels with alpha above the <paramref name="alphaThreshold"/> will be blended with this color before setting the pixel in the result image.
        /// The <see cref="Color.A"/> property of the background color is ignored. This parameter is optional.
        /// <br/>Default value: <see cref="Color.Empty"/>, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="alphaThreshold">If <paramref name="newPixelFormat"/> can represent only single-bit alpha or <paramref name="newPixelFormat"/> is an indexed format and the target palette contains a transparent color,
        /// then specifies a threshold value for the <see cref="Color.A">Color.A</see> property, under which the color is considered to be transparent. If <c>0</c>,
        /// then the result will not have transparent pixels. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <returns>A new <see cref="Bitmap"/> instance with the desired pixel format.</returns>
        /// <remarks>
        /// <para>If <paramref name="newPixelFormat"/> is an indexed format, then this overload will either use the palette of the source <paramref name="image"/> if applicable,
        /// or a system default palette. To apply a custom palette use the of the <see cref="ConvertPixelFormat(Image,PixelFormat,Color[],Color,byte)"/> overload.</para>
        /// <para>If <paramref name="newPixelFormat"/> can represent fewer colors than the source format, then a default
        /// quantization will occur during the conversion. To use a specific quantizer (and optionally a ditherer) use the <see cref="ConvertPixelFormat(Image,PixelFormat,IQuantizer,IDitherer)"/> overload.
        /// To use a quantizer with a specific palette you can use the <see cref="PredefinedColorsQuantizer"/> class.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="image"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="newPixelFormat"/> is out of the defined values.</exception>
        public static Bitmap ConvertPixelFormat(this Image image, PixelFormat newPixelFormat, Color backColor = default, byte alphaThreshold = 128)
            => ConvertPixelFormat(image, newPixelFormat, null, backColor, alphaThreshold);

        /// <summary>
        /// Converts the specified <paramref name="image"/> to a <see cref="Bitmap"/> with the desired <see cref="PixelFormat"/>.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="image">The original image to convert.</param>
        /// <param name="newPixelFormat">The desired new pixel format.</param>
        /// <param name="quantizer">An <see cref="IQuantizer"/> instance to determine the conversion of the colors.</param>
        /// <param name="ditherer">An optional <see cref="IDitherer"/> instance for dithering the result image, which usually produces a better result if colors are reduced. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A new <see cref="Bitmap"/> instance with the desired pixel format.</returns>
        /// <remarks>
        /// <para>An unmatching <paramref name="quantizer"/> and <paramref name="newPixelFormat"/> may cause undesired results.</para>
        /// <para>The <paramref name="ditherer"/> may have no effect if the <paramref name="quantizer"/> uses too many colors.</para>
        /// <para>To use a quantizer with predefined colors you can use the <see cref="PredefinedColorsQuantizer"/> class. If you want to use
        /// a quantizer without a ditherer and <paramref name="newPixelFormat"/> represents the same number of reduced colors as the <paramref name="quantizer"/> produces,
        /// then you can use the <see cref="ConvertPixelFormat(Image,PixelFormat,Color[],Color,byte)"/> overload for a slightly better performance.</para>
        /// <para>To produce a result with up to 256 colors best optimized for the source <paramref name="image"/> you can use the <see cref="OptimizedPaletteQuantizer"/> class.</para>
        /// <para>To quantize a <see cref="Bitmap"/> in place, without changing the pixel format you can use the <see cref="BitmapExtensions.Quantize">BitmapExtensions.Quantize</see> method.</para>
        /// <para>To dither a <see cref="Bitmap"/> in place, without changing the pixel format you can use the <see cref="BitmapExtensions.Dither">BitmapExtensions.Dither</see> method.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="image"/> or <paramref name="quantizer"/> are <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="newPixelFormat"/> is out of the defined values.</exception>
        /// <exception cref="ArgumentException">The <paramref name="quantizer"/> palette contains too many colors for the indexed format specified by <paramref name="newPixelFormat"/>.</exception>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The result must not be disposed; bmp is disposed if it is not the same as image.")]
        public static Bitmap ConvertPixelFormat(this Image image, PixelFormat newPixelFormat, IQuantizer quantizer, IDitherer ditherer = null)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image), PublicResources.ArgumentNull);
            if (!Enum<PixelFormat>.IsDefined(newPixelFormat))
                throw new ArgumentOutOfRangeException(nameof(newPixelFormat), PublicResources.EnumOutOfRange(newPixelFormat));
            if (quantizer == null)
                throw new ArgumentNullException(nameof(quantizer), PublicResources.ArgumentNull);

            Bitmap bmp = image as Bitmap ?? new Bitmap(image);
            Bitmap result = null;

            try
            {
                result = new Bitmap(image.Width, image.Height, newPixelFormat);
                using (BitmapDataAccessorBase source = BitmapDataAccessorFactory.CreateAccessor(bmp, ImageLockMode.ReadOnly))
                using (IQuantizingSession quantizingSession = quantizer.Initialize(source) ?? throw new InvalidOperationException(Res.ImagingQuantizerInitializeNull))
                {
                    // validating and initializing palette
                    if (newPixelFormat.IsIndexed())
                        InitPalette(newPixelFormat, bmp, result, quantizingSession.Palette?.Entries?.Select(c => c.ToColor()).ToArray());

                    using (BitmapDataAccessorBase target = BitmapDataAccessorFactory.CreateAccessor(result, ImageLockMode.WriteOnly, quantizingSession))
                    {
                        // quantization without dithering
                        if (ditherer == null)
                        {
                            // Sequential processing
                            if (source.Width < parallelThreshold)
                            {
                                BitmapDataRowBase rowSrc = source.GetRow(0);
                                BitmapDataRowBase rowDst = target.GetRow(0);
                                int width = source.Width;
                                do
                                {
                                    for (int x = 0; x < width; x++)
                                        rowDst.DoSetColor32(x, quantizingSession.GetQuantizedColor(rowSrc.DoGetColor32(x)));
                                } while (rowSrc.MoveNextRow() && rowDst.MoveNextRow());
                            }
                            // Parallel processing
                            else
                            {
                                ParallelHelper.For(0, source.Height, y =>
                                {
                                    BitmapDataRowBase rowSrc = source.GetRow(y);
                                    BitmapDataRowBase rowDst = target.GetRow(y);
                                    int width = source.Width;
                                    for (int x = 0; x < width; x++)
                                        rowDst.DoSetColor32(x, quantizingSession.GetQuantizedColor(rowSrc.DoGetColor32(x)));
                                });
                            }
                        }
                        // quantization with dithering
                        else
                        {
                            using (IDitheringSession ditheringSession = ditherer.Initialize(source, quantizingSession) ?? throw new InvalidOperationException(Res.ImagingDithererInitializeNull))
                            {
                                // Sequential processing
                                if (ditheringSession.IsSequential || source.Width < parallelThreshold)
                                {
                                    BitmapDataRowBase rowSrc = source.GetRow(0);
                                    BitmapDataRowBase rowDst = target.GetRow(0);
                                    int width = source.Width;
                                    do
                                    {
                                        for (int x = 0; x < width; x++)
                                            rowDst.DoSetColor32(x, ditheringSession.GetDitheredColor(rowSrc.DoGetColor32(x), x, rowSrc.RowIndex));
                                    } while (rowSrc.MoveNextRow() && rowDst.MoveNextRow());
                                }
                                // Parallel processing
                                else
                                {
                                    ParallelHelper.For(0, source.Height, y =>
                                    {
                                        BitmapDataRowBase rowSrc = source.GetRow(y);
                                        BitmapDataRowBase rowDst = target.GetRow(y);
                                        int width = source.Width;
                                        for (int x = 0; x < width; x++)
                                            rowDst.DoSetColor32(x, ditheringSession.GetDitheredColor(rowSrc.DoGetColor32(x), x, rowSrc.RowIndex));
                                    });
                                }
                            }
                        }
                    }
                }

                return result;
            }
            catch (Exception)
            {
                result?.Dispose();
                throw;
            }
            finally
            {
                if (!ReferenceEquals(bmp, image))
                    bmp.Dispose();
            }
        }

        public static void DrawInto(this Image source, Bitmap target, Point targetLocation, IDitherer ditherer = null)
            => DrawInto(source, target, new Rectangle(Point.Empty, source?.Size ?? default), targetLocation, ditherer);

        /// <summary>
        /// Draws the <paramref name="source"/> <see cref="Image"/> into the <paramref name="target"/> <see cref="Bitmap"/>.
        /// This method is similar to <see cref="Graphics.DrawImage(Image,Point)">Graphics.DrawImage</see> except that it never scales the images
        /// and that works between any pair of source and target <see cref="PixelFormat"/>s. If <paramref name="target"/> can represent a narrower set
        /// of colors, then the result will be automatically quantized to the colors of the target, and also an optional <paramref name="ditherer"/> can be specified.
        /// </summary>
        /// <param name="source">The source <see cref="Image"/> to be drawn into the <paramref name="target"/>.</param>
        /// <param name="target">The target <see cref="Bitmap"/> into which <paramref name="source"/> should be drawn.</param>
        /// <param name="sourceRect">The source area to be drawn into the <paramref name="target"/>.</param>
        /// <param name="targetLocation">The target location. Target size will be always the same as the source size.</param>
        /// <param name="ditherer">The ditherer to be used for the drawing. Has no effect, if target pixel format has at least 24 bits-per-pixel size. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="target"/> is <see langword="null"/></exception>
        public static void DrawInto(this Image source, Bitmap target, Rectangle sourceRect, Point targetLocation, IDitherer ditherer = null)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source), PublicResources.ArgumentNull);
            if (target == null)
                throw new ArgumentNullException(nameof(target), PublicResources.ArgumentNull);

            // clipping source rectangle with actual source size
            sourceRect.Intersect(new Rectangle(Point.Empty, source.Size));

            // calculating target rectangle
            Size targetSize = target.Size;
            Rectangle targetRect = new Rectangle(targetLocation, sourceRect.Size);
            if (targetRect.Right > targetSize.Width)
            {
                targetRect.Width -= targetRect.Right - targetSize.Width;
                sourceRect.Width = targetRect.Width;
            }

            if (targetRect.Bottom > targetSize.Height)
            {
                targetRect.Height -= targetRect.Bottom - targetSize.Height;
                sourceRect.Height = targetRect.Height;
            }

            if (targetRect.Left < 0)
            {
                sourceRect.Width += targetRect.Left;
                sourceRect.X -= targetRect.Left;
                targetRect.Width += targetRect.Left;
                targetRect.X = 0;
            }

            if (targetRect.Top < 0)
            {
                sourceRect.Height += targetRect.Top;
                sourceRect.Y -= targetRect.Top;
                targetRect.Height += targetRect.Top;
                targetRect.Y = 0;
            }

            // returning, if there is no remaining source to draw
            if (sourceRect.Height <= 0 || sourceRect.Width <= 0)
                return;

            PixelFormat targetPixelFormat = target.PixelFormat;

            // Cloning source if target and source are the same, or creating a new bitmap is source is a metafile
            Bitmap bmp = ReferenceEquals(source, target)
                ? ((Bitmap)source).CloneCurrentFrame()
                : source as Bitmap ?? new Bitmap(source);

            try
            {
                if (ditherer == null || targetPixelFormat.ToBitsPerPixel() >= 24 || targetPixelFormat == PixelFormat.Format16bppGrayScale)
                    DrawIntoDirect(bmp, target, sourceRect, targetRect.Location);
                else
                    DrawIntoWithDithering(bmp, target, sourceRect, targetRect.Location, ditherer);
            }
            finally
            {
                if (!ReferenceEquals(bmp, source))
                    bmp.Dispose();
            }
        }

        /// <summary>
        /// Compares an image to another one by content and returns whether they are equal. Images of different
        /// size or pixel format are considered different.
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
        /// <remarks>
        /// <para>When <paramref name="images"/> contain multi-page instances, this method takes only the current page. You can extract
        /// images by <see cref="BitmapExtensions.ExtractBitmaps">ExtractBitmaps</see> extension method.</para>
        /// <para>Compression mode and bit depth is chosen for each page based on pixel format.</para>
        /// <note>On non-Windows platform this method may throw a <see cref="NotSupportedException"/> if <paramref name="images"/> has multiple elements.</note>
        /// </remarks>
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
            if (type1 != type2 || image1.Size != image2.Size || image1.PixelFormat != image2.PixelFormat)
                return false;

            if (type1 == typeof(Metafile))
            {
                using (MemoryStream ms1 = new MemoryStream())
                using (MemoryStream ms2 = new MemoryStream())
                {
                    bool forceWmf = image1.RawFormat.Equals(ImageFormat.Wmf) || image2.RawFormat.Equals(ImageFormat.Wmf);
                    ((Metafile)image1).Save(ms1, forceWmf);
                    ((Metafile)image2).Save(ms2, forceWmf);

                    if (ms1.Length != ms2.Length)
                        return false;

                    unsafe
                    {
                        fixed (byte* pbuf1 = ms1.GetBuffer())
                        fixed (byte* pbuf2 = ms2.GetBuffer())
                            return MemoryHelper.CompareMemory(new IntPtr(pbuf1), new IntPtr(pbuf2), (int)ms1.Length);
                    }
                }
            }

            Bitmap bmp1 = (Bitmap)image1;
            Bitmap bmp2 = (Bitmap)image2;

            BitmapData data1 = bmp1.LockBits(new Rectangle(new Point(0, 0), bmp1.Size), ImageLockMode.ReadOnly, bmp1.PixelFormat);
            BitmapData data2 = bmp2.LockBits(new Rectangle(new Point(0, 0), bmp2.Size), ImageLockMode.ReadOnly, bmp2.PixelFormat);

            try
            {
                if (Math.Abs(data1.Stride) != Math.Abs(data2.Stride))
                    return false;

                // both are top-down images: can be compared in a whole
                if (data1.Stride > 0 && data2.Stride > 0)
                    return MemoryHelper.CompareMemory(data1.Scan0, data2.Scan0, data1.Stride * image1.Height);

                // at least one of them is a bottom-up image: line by line
                for (int i = 0; i < data1.Height; i++)
                {
                    IntPtr line1 = new IntPtr(data1.Scan0.ToInt64() + data1.Stride * i);
                    IntPtr line2 = new IntPtr(data2.Scan0.ToInt64() + data2.Stride * i);
                    if (!MemoryHelper.CompareMemory(line1, line2, Math.Abs(data1.Stride)))
                        return false;
                }

                return true;
            }
            finally
            {
                bmp1.UnlockBits(data1);
                bmp2.UnlockBits(data2);
            }
        }

        // TODO: delete, no longer needed
        private static Bitmap ToIndexedWindows(Image image, int bpp, Color[] palette = null)
        {
            PixelFormat sourcePixelFormat = image.PixelFormat;

            // using GDI+ natively
            Bitmap bmp = image as Bitmap ?? new Bitmap(image);
            bool isMetafile = image is Metafile;
            var targetPalette = new RGBQUAD[256];
            int colorCount = InitPalette(targetPalette, bpp, bmp.Palette, palette, out int transparentIndex);
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
            Bitmap result = Image.FromHbitmap(hbmResult);
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

        // TODO: delete
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

            // if there is transparency and there are more than 2 colors, then 
            if (transparentIndex != -1)
            {
                // two colors image: preventing the result to be completely blank
                if (maxColors == 2)
                {
                    int nonTrIndex = 1 - transparentIndex;
                    if (targetPalette[nonTrIndex].EqualsWithColor(sourcePalette[transparentIndex]))
                        targetPalette[transparentIndex] = new RGBQUAD(sourcePalette[transparentIndex].G >= 128 ? Color.Black : Color.White);
                }
                // non 2 colors image: making sure the transparent index is not used during the conversion
                else if (transparentIndex == 0)
                {
                    // relocating transparent index to be the 2nd color
                    targetPalette[0] = targetPalette[1];
                    transparentIndex = 1;
                }
                else
                    // otherwise, setting the color of transparent index the same as the previous color
                    targetPalette[transparentIndex] = targetPalette[transparentIndex - 1];
            }

            // if black color is not found in palette, counting 1 extra colors because it can be used in conversion
            if (colorCount < maxColors && !hasBlack)
                colorCount++;

            return colorCount;
        }

        private static void InitPalette(PixelFormat newPixelFormat, Bitmap source, Bitmap target, Color[] palette)
        {
            int bpp = newPixelFormat.ToBitsPerPixel();

            // if the quantized does not have a palette but converting to a higher bpp indexed image, then taking the source palette
            if (palette == null && source.PixelFormat.ToBitsPerPixel() <= bpp)
                palette = source.Palette?.Entries;

            if (palette == null || palette.Length <= 0)
                return;

            // there is a desired palette to apply
            int maxColors = 1 << bpp;
            if (palette.Length > maxColors)
                throw new ArgumentException(Res.ImageExtensionsPaletteTooLarge(maxColors, newPixelFormat), nameof(palette));

            ColorPalette targetPalette = target.Palette;
            bool setEntries = palette.Length != targetPalette.Entries.Length;
            Color[] targetColors = setEntries ? new Color[palette.Length] : targetPalette.Entries;

            // copying even if it could be just set to prevent change of entries
            for (int i = 0; i < palette.Length; i++)
                targetColors[i] = palette[i];

            if (setEntries)
                targetPalette.SetEntries(targetColors);
            target.Palette = targetPalette;
        }

        // TODO: delete
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

        // TODO: delete
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
                                throw new InvalidOperationException(Res.InternalError("Unexpected bits per pixel"));
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

        private static void DrawIntoDirect(Bitmap source, Bitmap target, Rectangle sourceRect, Point targetLocation)
        {
            #region Local Methods

            static void ProcessRowStraight(int y, BitmapDataAccessorBase src, BitmapDataAccessorBase dst, Rectangle rectSrc, Point locDst)
            {
                BitmapDataRowBase rowSrc = src.GetRow(rectSrc.Y + y);
                BitmapDataRowBase rowDst = dst.GetRow(locDst.Y + y);
                for (int x = 0; x < rectSrc.Width; x++)
                {
                    Color32 colorSrc = rowSrc.DoGetColor32(rectSrc.X + x);

                    // fully transparent source: skip
                    if (colorSrc.A == 0)
                        continue;

                    // fully solid source: overwrite
                    if (colorSrc.A == Byte.MaxValue)
                    {
                        rowDst.DoSetColor32(locDst.X + x, colorSrc);
                        continue;
                    }

                    // source here has a partial transparency: we need to read the target color
                    int pos = locDst.X + x;
                    Color32 colorDst = rowDst.DoGetColor32(pos);

                    // fully transparent target: we can overwrite with source
                    if (colorDst.A == 0)
                    {
                        rowDst.DoSetColor32(pos, colorSrc);
                        continue;
                    }

                    colorSrc = colorDst.A == Byte.MaxValue
                        // target pixel is fully solid: simple blending
                        ? colorSrc.BlendWithBackground(colorDst)
                        // both source and target pixels are partially transparent: complex blending
                        : colorSrc.BlendWith(colorDst);

                    rowDst.DoSetColor32(pos, colorSrc);
                }
            }

            static void ProcessRowPremultiplied(int y, BitmapDataAccessorBase src, BitmapDataAccessorBase dst, Rectangle rectSrc, Point locDst)
            {
                BitmapDataRowBase rowSrc = src.GetRow(rectSrc.Y + y);
                BitmapDataRowBase rowDst = dst.GetRow(locDst.Y + y);
                bool isPremultipliedSource = src.PixelFormat == PixelFormat.Format32bppPArgb;
                for (int x = 0; x < rectSrc.Width; x++)
                {
                    Color32 colorSrc = isPremultipliedSource
                        ? rowSrc.DoReadRaw<Color32>(rectSrc.X + x)
                        : rowSrc.DoGetColor32(rectSrc.X + x).ToPremultiplied();

                    // fully transparent source: skip
                    if (colorSrc.A == 0)
                        continue;

                    // fully solid source: overwrite
                    if (colorSrc.A == Byte.MaxValue)
                    {
                        rowDst.DoWriteRaw(locDst.X + x, colorSrc);
                        continue;
                    }

                    // source here has a partial transparency: we need to read the target color
                    int pos = locDst.X + x;
                    Color32 colorDst = rowDst.DoReadRaw<Color32>(pos);

                    // fully transparent target: we can overwrite with source
                    if (colorDst.A == 0)
                    {
                        rowDst.DoWriteRaw(pos, colorSrc);
                        continue;
                    }

                    rowDst.DoWriteRaw(pos, colorSrc.BlendWithPremultiplied(colorDst));
                }
            } 
            
            #endregion

            using (BitmapDataAccessorBase src = BitmapDataAccessorFactory.CreateAccessor(source, ImageLockMode.ReadOnly))
            using (BitmapDataAccessorBase dst = BitmapDataAccessorFactory.CreateAccessor(target, ImageLockMode.ReadWrite))
            {
                Action<int, BitmapDataAccessorBase, BitmapDataAccessorBase, Rectangle, Point> processRow = dst.PixelFormat == PixelFormat.Format32bppPArgb
                    ? (Action<int, BitmapDataAccessorBase, BitmapDataAccessorBase, Rectangle, Point>)ProcessRowPremultiplied
                    : ProcessRowStraight;

                // Sequential processing
                if (sourceRect.Width < parallelThreshold)
                {
                    for (int y = 0; y < sourceRect.Height; y++)
                        processRow.Invoke(y, src, dst, sourceRect, targetLocation);
                    return;
                }

                // Parallel processing
                ParallelHelper.For(0, sourceRect.Height,
                    y => processRow.Invoke(y, src, dst, sourceRect, targetLocation));
            }
        }

        private static void DrawIntoWithDithering(Bitmap source, Bitmap target, Rectangle sourceRect, Point targetLocation, IDitherer ditherer)
        {
            #region Local Methods

            static void ProcessRow(int y, IDitheringSession session, BitmapDataAccessorBase src, BitmapDataAccessorBase dst, Rectangle rectSrc, Point locDst)
            {
                int ySrc = y + rectSrc.Top;
                BitmapDataRowBase rowSrc = src.GetRow(ySrc);
                BitmapDataRowBase rowDst = dst.GetRow(y + locDst.Y);

                for (int x = 0; x < rectSrc.Width; x++)
                {
                    int xSrc = x + rectSrc.Left;
                    rowDst.DoSetColor32(x + locDst.X,
                        session.GetDitheredColor(rowSrc.DoGetColor32(xSrc), xSrc, ySrc));
                }
            }

            #endregion

            using (BitmapDataAccessorBase src = BitmapDataAccessorFactory.CreateAccessor(source, ImageLockMode.ReadOnly))
            using (BitmapDataAccessorBase dst = BitmapDataAccessorFactory.CreateAccessor(target, ImageLockMode.ReadWrite))
            {
                IQuantizer quantizer = PredefinedColorsQuantizer.FromBitmapData(dst);
                using (IQuantizingSession quantizingSession = quantizer.Initialize(src))
                using (IDitheringSession ditheringSession = ditherer.Initialize(src, quantizingSession) ?? throw new InvalidOperationException(Res.ImagingDithererInitializeNull))
                {
                    // sequential processing
                    if (ditheringSession.IsSequential || sourceRect.Width < parallelThreshold)
                    {
                        for (int y = 0; y < sourceRect.Height; y++)
                            ProcessRow(y, ditheringSession, src, dst, sourceRect, targetLocation);
                        return;
                    }

                    // parallel processing
                    ParallelHelper.For(0, sourceRect.Height, y =>
                    {
                        ProcessRow(y, ditheringSession, src, dst, sourceRect, targetLocation);
                    });
                }
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
