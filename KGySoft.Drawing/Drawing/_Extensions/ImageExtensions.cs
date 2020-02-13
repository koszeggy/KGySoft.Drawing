﻿#region Copyright

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
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security;

using KGySoft.CoreLibraries;
using KGySoft.Drawing.Imaging;

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

        #region Fields

        private static ImageCodecInfo[] encoders = ImageCodecInfo.GetImageEncoders();

        private static readonly Dictionary<Guid, Dictionary<PixelFormat, PixelFormat>> saveConversions = new Dictionary<Guid, Dictionary<PixelFormat, PixelFormat>>
        {
            [ImageFormat.Bmp.Guid] = new Dictionary<PixelFormat, PixelFormat>
            {
                [PixelFormat.Format16bppRgb565] = PixelFormat.Format24bppRgb,
                [PixelFormat.Format16bppRgb555] = PixelFormat.Format24bppRgb,
                [PixelFormat.Format16bppArgb1555] = PixelFormat.Format32bppArgb,
                [PixelFormat.Format16bppGrayScale] = PixelFormat.Format8bppIndexed,
                [PixelFormat.Undefined] = PixelFormat.Format24bppRgb
            },
            [ImageFormat.Gif.Guid] = new Dictionary<PixelFormat, PixelFormat>
            {
                [PixelFormat.Format1bppIndexed] = PixelFormat.Format8bppIndexed,
                [PixelFormat.Format4bppIndexed] = PixelFormat.Format8bppIndexed,
                [PixelFormat.Format16bppGrayScale] = PixelFormat.Format8bppIndexed,
                [PixelFormat.Format16bppArgb1555] = PixelFormat.Format8bppIndexed,
                [PixelFormat.Format16bppRgb555] = PixelFormat.Format8bppIndexed,
                [PixelFormat.Format16bppRgb565] = PixelFormat.Format8bppIndexed,
                [PixelFormat.Format24bppRgb] = PixelFormat.Format8bppIndexed,
                [PixelFormat.Format32bppRgb] = PixelFormat.Format8bppIndexed,
                [PixelFormat.Format32bppPArgb] = PixelFormat.Format8bppIndexed,
                [PixelFormat.Format32bppArgb] = PixelFormat.Format8bppIndexed,
                [PixelFormat.Format48bppRgb] = PixelFormat.Format8bppIndexed,
                [PixelFormat.Format64bppArgb] = PixelFormat.Format8bppIndexed,
                [PixelFormat.Format64bppPArgb] = PixelFormat.Format8bppIndexed,
                [PixelFormat.Undefined] = PixelFormat.Format8bppIndexed
            },
            [ImageFormat.Jpeg.Guid] = new Dictionary<PixelFormat, PixelFormat>
            {
                [PixelFormat.Format16bppArgb1555] = PixelFormat.Format24bppRgb,
                [PixelFormat.Format16bppGrayScale] = PixelFormat.Format24bppRgb,
                [PixelFormat.Format32bppArgb] = PixelFormat.Format24bppRgb,
                [PixelFormat.Format64bppArgb] = PixelFormat.Format24bppRgb,
                [PixelFormat.Format64bppPArgb] = PixelFormat.Format24bppRgb,
                [PixelFormat.Undefined] = PixelFormat.Format24bppRgb
            },
            [ImageFormat.Png.Guid] = new Dictionary<PixelFormat, PixelFormat>
            {
                [PixelFormat.Format16bppRgb565] = PixelFormat.Format24bppRgb,
                [PixelFormat.Format16bppRgb555] = PixelFormat.Format24bppRgb,
                [PixelFormat.Format16bppGrayScale] = PixelFormat.Format24bppRgb,
                [PixelFormat.Format48bppRgb] = PixelFormat.Format24bppRgb,
                [PixelFormat.Format64bppArgb] = PixelFormat.Format32bppArgb,
                [PixelFormat.Format64bppPArgb] = PixelFormat.Format32bppArgb,
                [PixelFormat.Undefined] = PixelFormat.Format32bppArgb
            },
            [ImageFormat.Tiff.Guid] = new Dictionary<PixelFormat, PixelFormat>
            {
                [PixelFormat.Format16bppGrayScale] = PixelFormat.Format8bppIndexed,
                [PixelFormat.Undefined] = PixelFormat.Format32bppArgb
            },
        };

        #endregion

        #region Properties

        private static ImageCodecInfo[] Encoders => encoders ??= ImageCodecInfo.GetImageEncoders();

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
        [SuppressMessage("ReSharper", "AccessToDisposedClosure", Justification = "ParallelHelper.For invokes delegates before returning")]
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
        /// <param name="quantizer">An optional <see cref="IQuantizer"/> instance to determine the conversion of the colors.
        /// If <see langword="null"/>&#160;and <paramref name="newPixelFormat"/> is an indexed format, then a default palette and quantization logic will be used.</param>
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
        /// <exception cref="ArgumentNullException"><paramref name="image"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="newPixelFormat"/> is out of the defined values.</exception>
        /// <exception cref="ArgumentException">The <paramref name="quantizer"/> palette contains too many colors for the indexed format specified by <paramref name="newPixelFormat"/>.</exception>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The result must not be disposed; bmp is disposed if it is not the same as image.")]
        [SuppressMessage("ReSharper", "AccessToDisposedClosure", Justification = "ParallelHelper.For invokes delegates before returning")]
        public static Bitmap ConvertPixelFormat(this Image image, PixelFormat newPixelFormat, IQuantizer quantizer, IDitherer ditherer = null)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image), PublicResources.ArgumentNull);
            if (!Enum<PixelFormat>.IsDefined(newPixelFormat))
                throw new ArgumentOutOfRangeException(nameof(newPixelFormat), PublicResources.EnumOutOfRange(newPixelFormat));
            if (quantizer == null)
                return ConvertPixelFormat(image, newPixelFormat);

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
        /// <param name="size">The required width and height of the icon.</param>
        /// <param name="keepAspectRatio">When source <paramref name="image"/> is not square sized, determines whether the image should keep aspect ratio.</param>
        /// <returns>An <see cref="Icon"/> instance created from the <paramref name="image"/>.</returns>
        /// <remarks>The result icon will be always square sized and will contain only a single image.
        /// To create a possibly non-squared icon, use the <see cref="ToIcon(Image,Color)"/> overload or the <see cref="Icons.Combine(Bitmap[])">Icons.Combine</see> method instead.</remarks>
#if !NET35
        [SecuritySafeCritical]
#endif
        public static Icon ToIcon(this Image image, int size, bool keepAspectRatio) => Icons.FromImage(image, size, keepAspectRatio);

        /// <summary>
        /// Creates an <see cref="Icon" /> from an <see cref="Image" />.
        /// </summary>
        /// <param name="image">The image to be converted to an icon.</param>
        /// <param name="transparentColor">A color that represents transparent color for the icon to be created. Ignored if the <paramref name="image"/> is large and will be PNG compressed. This parameter is optional.
        /// <br/>Default value: <see cref="Color.Empty"/>, which keeps only already transparent pixels.</param>
        /// <returns>An <see cref="Icon"/> instance created from the <paramref name="image"/> that has the same size as the specified <paramref name="image"/>.</returns>
        /// <remarks>
        /// <para>The result icon will have the same size as the specified <paramref name="image"/>.
        /// To create a squared icon, use the <see cref="ToIcon(Image,int,bool)"/> overload instead.</para>
        /// <para>If the raw format of <paramref name="image"/> is an icon that contains multiple images, then the result will also contain multiple resolutions.</para>
        /// <para>The result <see cref="Icon"/> is compatible with Windows XP if the method is executed in a Windows XP environment.</para>
        /// <para>To disable PNG compression also for large images regardless of the current operating system call the <see cref="Icons.Combine(Bitmap[], Color[], bool)"/> method instead.</para>
        /// </remarks>
        public static Icon ToIcon(this Image image, Color transparentColor = default)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image), PublicResources.ArgumentNull);

            Bitmap bmp = image as Bitmap ?? new Bitmap(image);
            try
            {
                return Icons.Combine(new[] { bmp }, new[] { transparentColor });
            }
            finally
            {
                if (!ReferenceEquals(bmp, image))
                    bmp.Dispose();
            }
        }

        /// <summary>
        /// Saves the specified <paramref name="image"/> using the built-in BMP encoder if available on current operating system.
        /// Unlike the <see cref="Image.Save(Stream,ImageFormat)"/> method, this one supports every <see cref="PixelFormat"/>.
        /// <br/>See the <strong>Remarks</strong> section for details and an example.
        /// </summary>
        /// <param name="image">The image to save. If contains multiple images, then only the current frame will be saved.</param>
        /// <param name="stream">The stream to save the image into.</param>
        /// <remarks>
        /// <para>The <paramref name="image"/> can only be saved if a built-in BMP encoder is available in the current operating system.</para>
        /// <para>The saved BMP image is never RLE compressed.</para>
        /// <para>The BMP format supports transparency only for the 64 BPP formats but the Windows BMP encoder stores alpha information also for the 32 BPP formats, which can be restored (see also the example below).</para>
        /// <para>Images with different <see cref="PixelFormat"/>s are handled as follows (on Windows, unless specified otherwise):
        /// <list type="definition">
        /// <item><term><see cref="PixelFormat.Format1bppIndexed"/></term><description>The pixel format is preserved, though palette entries with alpha are turned opaque.</description></item>
        /// <item><term><see cref="PixelFormat.Format4bppIndexed"/></term><description>The pixel format is preserved, though palette entries with alpha are turned opaque.</description></item>
        /// <item><term><see cref="PixelFormat.Format8bppIndexed"/></term><description>The pixel format is preserved, though palette entries with alpha are turned opaque.</description></item>
        /// <item><term><see cref="PixelFormat.Format16bppRgb565"/></term><description>Before saving the image pixel format will be converted to <see cref="PixelFormat.Format24bppRgb"/>
        /// because the built-in encoder would save a 32 BPP image otherwise, which is just a waste of space.</description></item>
        /// <item><term><see cref="PixelFormat.Format16bppRgb555"/></term><description>Before saving the image pixel format will be converted to <see cref="PixelFormat.Format24bppRgb"/>
        /// because the built-in encoder would save a 32 BPP image otherwise, which is just a waste of space.</description></item>
        /// <item><term><see cref="PixelFormat.Format16bppArgb1555"/></term><description>Before saving the image pixel format will converted to <see cref="PixelFormat.Format32bppArgb"/>.
        /// Though reloading such an image will not have transparency but it can be restored (see also the example below).</description></item>
        /// <item><term><see cref="PixelFormat.Format16bppGrayScale"/></term><description>Before saving the image pixel format will be converted to <see cref="PixelFormat.Format8bppIndexed"/>
        /// using a grayscale palette, because otherwise GDI+ would throw an exception.</description></item>
        /// <item><term><see cref="PixelFormat.Format24bppRgb"/></term><description>When reloading the saved image the pixel format is preserved.</description></item>
        /// <item><term><see cref="PixelFormat.Format32bppRgb"/></term><description>When reloading the saved image the pixel format is preserved.</description></item>
        /// <item><term><see cref="PixelFormat.Format32bppArgb"/></term><description>When the saved image is reloaded by the built-in decoder the pixel format will be <see cref="PixelFormat.Format32bppRgb"/> and the image will have no transparency.
        /// Actually alpha information is preserved and can be restored (see the example below).</description></item>
        /// <item><term><see cref="PixelFormat.Format32bppPArgb"/></term><description>When the saved image is reloaded by the built-in decoder, the pixel format will be <see cref="PixelFormat.Format32bppRgb"/> and the image will have no transparency.
        /// Actually alpha information preserved and can be restored (see the example below).</description></item>
        /// <item><term><see cref="PixelFormat.Format48bppRgb"/></term><description>When reloading the saved image the pixel format will turn <see cref="PixelFormat.Format24bppRgb"/>.</description></item>
        /// <item><term><see cref="PixelFormat.Format64bppArgb"/></term><description>When reloading the saved image the pixel format is preserved. Note that not every application supports or handles BMP format with 64 BPP correctly.</description></item>
        /// <item><term><see cref="PixelFormat.Format64bppPArgb"/></term><description>When reloading the saved image the pixel format will turn <see cref="PixelFormat.Format64bppArgb"/>.</description></item>
        /// </list>
        /// </para>
        /// </remarks>
        /// <example>The following example demonstrates how to restore transparency from 32 BPP bitmaps saved by the <see cref="SaveAsBmp">SaveAsBmp</see> method:
        /// <code lang="C#"><![CDATA[
        /// // this is a 32 BPP ARGB bitmap with transparency:
        /// Bitmap toSave = Icons.Information.ExtractBitmap(new Size(256, 256));
        /// Bitmap reloaded;
        ///
        /// // Saving and reloading the transparent image as BMP:
        /// using (var stream = new MemoryStream())
        /// {
        ///     bmp.SaveAsBmp(stream);
        ///     stream.Position = 0;
        ///
        ///     // realoaded Bitmap has now Format32bppRgb PixelFormat without transparency
        ///     reloaded = new Bitmap(stream);
        /// }
        ///
        /// // Restoring transparency by using fast bitmap data accessors (not needed for 64 BPP images):
        /// Bitmap restored = new Bitmap(reloaded.Width, reloaded.Height, PixelFormat.Format32bppArgb);
        /// using (IReadableBitmapData dataSrc = reloaded.GetReadableBitmapData())
        /// using (IWritableBitmapData dataDst = restored.GetWritableBitmapData())
        /// {
        ///     IReadableBitmapDataRow rowSrc = dataSrc.FirstRow;
        ///     IWritableBitmapDataRow rowDst = dataDst.FirstRow;
        ///     do
        ///     {
        ///         for (int x = 0; x < dataSrc.Width; x++)
        ///         {
        ///             // Note 1: If we used the indexer instead, then the source color would never be transparent.
        ///             // Note 2: We can use any type of the same size so int/uint types would also do the trick.
        ///             rowDst.WriteRaw(x, rowSrc.ReadRaw<Color32>(x));
        ///         }
        ///     } while (rowSrc.MoveNextRow() && rowDst.MoveNextRow());
        /// }]]></code>
        /// </example>
        /// <exception cref="ArgumentNullException"><paramref name="image"/> or <paramref name="stream"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException">No built-in encoder was found or the saving fails on current operating system.</exception>
        public static void SaveAsBmp(this Image image, Stream stream)
            => SaveByEncoder(image, stream, ImageFormat.Bmp, null, false);

        /// <summary>
        /// Saves the specified <paramref name="image"/> using the built-in JPEG encoder if available on current operating system.
        /// Unlike the <see cref="Image.Save(Stream,ImageFormat)"/> method, this one supports every <see cref="PixelFormat"/>.
        /// <br/>See the <strong>Remarks</strong> section for details and an example.
        /// </summary>
        /// <param name="image">The image to save. If contains multiple images, then only the current frame will be saved.</param>
        /// <param name="stream">The stream to save the image into.</param>
        /// <param name="quality">An integer between <c>0</c> and <c>100</c> that determines the quality of the saved image. Higher value means
        /// better quality as well as bigger size. This parameter is optional.
        /// <br/>Default value: <c>90</c>.</param>
        /// <remarks>
        /// <para>The <paramref name="image"/> can only be saved if a built-in JPEG encoder is available in the current operating system.</para>
        /// <para>The saved JPEG image is will have always 24 BPP format.</para>
        /// <para>The JPEG format uses a lossy compression (even using the best quality) and does not support transparency for any <see cref="PixelFormat"/>.</para>
        /// <para>Transparent pixels will be black in the saved image. To use another background color use the <see cref="BitmapExtensions.MakeOpaque">MakeOpaque</see>
        /// or <see cref="ConvertPixelFormat(Image, PixelFormat, Color, byte)">ConvertPixelFormat</see> before saving (see also the example below).</para>
        /// <para>Images with different <see cref="PixelFormat"/>s are handled as follows (on Windows, unless specified otherwise):
        /// <list type="definition">
        /// <item><term><see cref="PixelFormat.Format1bppIndexed"/></term><description>When reloading the saved image the pixel format will turn <see cref="PixelFormat.Format24bppRgb"/>. Transparency will be lost.</description></item>
        /// <item><term><see cref="PixelFormat.Format4bppIndexed"/></term><description>When reloading the saved image the pixel format will turn <see cref="PixelFormat.Format24bppRgb"/>. Transparency will be lost.</description></item>
        /// <item><term><see cref="PixelFormat.Format8bppIndexed"/></term><description>When reloading the saved image the pixel format will turn <see cref="PixelFormat.Format24bppRgb"/>. Transparency will be lost.</description></item>
        /// <item><term><see cref="PixelFormat.Format16bppRgb565"/></term><description>When reloading the saved image the pixel format will turn <see cref="PixelFormat.Format24bppRgb"/>.</description></item>
        /// <item><term><see cref="PixelFormat.Format16bppRgb555"/></term><description>When reloading the saved image the pixel format will turn <see cref="PixelFormat.Format24bppRgb"/>.</description></item>
        /// <item><term><see cref="PixelFormat.Format16bppArgb1555"/></term><description>Before saving the image pixel format will converted to <see cref="PixelFormat.Format24bppRgb"/>;
        /// otherwise, the built-in encoder may save transparent pixels with nonzero color information incorrectly. Transparency will be lost.</description></item>
        /// <item><term><see cref="PixelFormat.Format16bppGrayScale"/></term><description>Before saving the image pixel format will be converted to <see cref="PixelFormat.Format24bppRgb"/>
        /// because otherwise GDI+ would throw an exception.</description></item>
        /// <item><term><see cref="PixelFormat.Format24bppRgb"/></term><description>When reloading the saved image the pixel format is preserved.</description></item>
        /// <item><term><see cref="PixelFormat.Format32bppRgb"/></term><description>When reloading the saved image the pixel format will turn <see cref="PixelFormat.Format24bppRgb"/>.</description></item>
        /// <item><term><see cref="PixelFormat.Format32bppArgb"/></term><description>Before saving the image pixel format will converted to <see cref="PixelFormat.Format24bppRgb"/>;
        /// otherwise, the built-in encoder may save pixels with alpha incorrectly. Transparency will be lost.</description></item>
        /// <item><term><see cref="PixelFormat.Format32bppPArgb"/></term><description>When reloading the saved image the pixel format will turn <see cref="PixelFormat.Format24bppRgb"/>. Transparency will be lost.</description></item>
        /// <item><term><see cref="PixelFormat.Format48bppRgb"/></term><description>When reloading the saved image the pixel format will turn <see cref="PixelFormat.Format24bppRgb"/>.</description></item>
        /// <item><term><see cref="PixelFormat.Format64bppArgb"/></term><description>Before saving the image pixel format will converted to <see cref="PixelFormat.Format24bppRgb"/>;
        /// otherwise, the built-in encoder may save pixels with alpha incorrectly. Transparency will be lost.</description></item>
        /// <item><term><see cref="PixelFormat.Format64bppPArgb"/></term><description>Before saving the image pixel format will converted to <see cref="PixelFormat.Format24bppRgb"/>;
        /// otherwise, the built-in encoder may save pixels with alpha incorrectly. Transparency will be lost.</description></item>
        /// </list>
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="image"/> or <paramref name="stream"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException">No built-in encoder was found or the saving fails on current operating system.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="quality"/> must be between 0 and 100.</exception>
        /// <example>The following example demonstrates how to save an image with custom background color using the <see cref="SaveAsJpeg">SaveAsJpeg</see> method:
        /// <code lang="C#"><![CDATA[
        /// // this is a 32 BPP ARGB bitmap with transparency:
        /// using Bitmap origBmp = Icons.Information.ExtractBitmap(new Size(256, 256));
        ///
        /// // Turning the background white before saving (it would turn black otherwise):
        /// using Bitmap toSave = origBmp.ConvertPixelFormat(PixelFormat.Format24bppRgb, Color.White);
        /// // Or: origBmp.MakeOpaque(Color.White); // changes the original image instead of returning a new one
        ///
        /// // Saving the image with the white background:
        /// toSave.SaveAsJpeg(File.Create(@"C:\myimage.jpg"))]]></code>
        /// </example>
        public static void SaveAsJpeg(this Image image, Stream stream, int quality = 90)
        {
            if ((uint)quality > 100u)
                throw new ArgumentOutOfRangeException(nameof(quality), PublicResources.ArgumentMustBeBetween(0, 100));
            using (var parameters = new EncoderParameters(1))
            {
                parameters.Param[0] = new EncoderParameter(Encoder.Quality, quality);
                SaveByEncoder(image, stream, ImageFormat.Jpeg, parameters, false);
            }
        }

        /// <summary>
        /// Saves the specified <paramref name="image"/> using the built-in PNG encoder if available on current operating system.
        /// Unlike the <see cref="Image.Save(Stream,ImageFormat)"/> method, this one supports every <see cref="PixelFormat"/>.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="image">The image to save. If contains multiple images, then only the current frame will be saved.</param>
        /// <param name="stream">The stream to save the image into.</param>
        /// <remarks>
        /// <para>The <paramref name="image"/> can only be saved if a built-in PNG encoder is available in the current operating system.</para>
        /// <para>The saved PNG image will have 32 BPP format if the source image can have transparency; otherwise, it will have 24 BPP format.</para>
        /// <para>On Windows PNG is never saved with indexed format.</para>
        /// <para>Images with different <see cref="PixelFormat"/>s are handled as follows (on Windows, unless specified otherwise):
        /// <list type="definition">
        /// <item><term><see cref="PixelFormat.Format1bppIndexed"/></term><description>When reloading the saved image the pixel format will turn <see cref="PixelFormat.Format32bppArgb"/>.</description></item>
        /// <item><term><see cref="PixelFormat.Format4bppIndexed"/></term><description>When reloading the saved image the pixel format will turn <see cref="PixelFormat.Format32bppArgb"/>.</description></item>
        /// <item><term><see cref="PixelFormat.Format8bppIndexed"/></term><description>When reloading the saved image the pixel format will turn <see cref="PixelFormat.Format32bppArgb"/>.</description></item>
        /// <item><term><see cref="PixelFormat.Format16bppRgb565"/></term><description>Before saving the image pixel format will be converted to <see cref="PixelFormat.Format24bppRgb"/>
        /// because the built-in encoder would save a 32 BPP image otherwise, which is just a waste of space.</description></item>
        /// <item><term><see cref="PixelFormat.Format16bppRgb555"/></term><description>Before saving the image pixel format will be converted to <see cref="PixelFormat.Format24bppRgb"/>
        /// because the built-in encoder would save a 32 BPP image otherwise, which is just a waste of space.</description></item>
        /// <item><term><see cref="PixelFormat.Format16bppArgb1555"/></term><description>When reloading the saved image the pixel format will turn <see cref="PixelFormat.Format32bppArgb"/>.</description></item>
        /// <item><term><see cref="PixelFormat.Format16bppGrayScale"/></term><description>Before saving the image pixel format will be converted to <see cref="PixelFormat.Format24bppRgb"/>
        /// because otherwise GDI+ would throw an exception.</description></item>
        /// <item><term><see cref="PixelFormat.Format24bppRgb"/></term><description>When reloading the saved image the pixel format is preserved.</description></item>
        /// <item><term><see cref="PixelFormat.Format32bppRgb"/></term><description>When reloading the saved image the pixel format will turn <see cref="PixelFormat.Format32bppArgb"/>.</description></item>
        /// <item><term><see cref="PixelFormat.Format32bppArgb"/></term><description>When reloading the saved image the pixel format is preserved.</description></item>
        /// <item><term><see cref="PixelFormat.Format32bppPArgb"/></term><description>When reloading the saved image the pixel format will turn <see cref="PixelFormat.Format32bppArgb"/>.</description></item>
        /// <item><term><see cref="PixelFormat.Format48bppRgb"/></term><description>Before saving the image pixel format will be converted to <see cref="PixelFormat.Format24bppRgb"/>
        /// because the built-in encoder would save a 32 BPP image with incorrect colors otherwise.</description></item>
        /// <item><term><see cref="PixelFormat.Format64bppArgb"/></term><description>Before saving the image pixel format will converted to <see cref="PixelFormat.Format32bppArgb"/>
        /// because the built-in encoder would save the image incorrectly otherwise.</description></item>
        /// <item><term><see cref="PixelFormat.Format64bppPArgb"/></term><description>Before saving the image pixel format will converted to <see cref="PixelFormat.Format32bppArgb"/>
        /// because the built-in encoder would save the image incorrectly otherwise.</description></item>
        /// </list>
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="image"/> or <paramref name="stream"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException">No built-in encoder was found or the saving fails on current operating system.</exception>
        public static void SaveAsPng(this Image image, Stream stream)
            => SaveByEncoder(image, stream, ImageFormat.Png, null, false);

        /// <summary>
        /// Saves the specified <paramref name="image"/> using the built-in GIF encoder if available on current operating system.
        /// Unlike the <see cref="Image.Save(Stream,ImageFormat)"/> method, this one supports every <see cref="PixelFormat"/>.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="image">The image to save. If image contains multiple images other than animated GIF frames, then only the current image will be saved.</param>
        /// <param name="stream">The stream to save the image into.</param>
        /// <param name="quantizer">If <paramref name="image"/> is a non-indexed one, then specifies the quantizer to be used to determine the colors of the saved image. If <see langword="null"/>,
        /// then the target colors will be optimized for the actual colors in the <paramref name="image"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="ditherer">If a quantization has to be performed can specifies the ditherer to be used. If <see langword="null"/>, then no dithering will be performed. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <remarks>
        /// <para>The <paramref name="image"/> can only be saved if a built-in GIF encoder is available in the current operating system.</para>
        /// <para>If <paramref name="image"/> is an animated GIF, then the whole animation will be saved (can depend on the operating system).</para>
        /// <para>The <paramref name="image"/> will be saved always with a <see cref="PixelFormat.Format8bppIndexed"/> format, though the palette can have less than 256 colors.</para>
        /// <para>The GIF format supports single bit transparency only.</para>
        /// <para>Images with <see cref="PixelFormat"/>s other than <see cref="PixelFormat.Format8bppIndexed"/> are converted to <see cref="PixelFormat.Format8bppIndexed"/> before saving (including other indexed formats);
        /// otherwise, the built-in GIF encoder (at least on Windows) would save the image with a fixed palette and transparency would be lost.</para>
        /// <para>If <paramref name="quantizer"/> is <see langword="null"/>&#160;and <paramref name="image"/> has a non-indexed pixel format, then a quantizer
        /// is automatically selected for optimizing the palette. The auto selected quantizer is obtained by the <see cref="PredefinedColorsQuantizer.Grayscale8bpp">PredefinedColorsQuantizer.Grayscale8bpp</see> method
        /// for the <see cref="PixelFormat.Format16bppGrayScale"/> pixel format, and by the <see cref="OptimizedPaletteQuantizer.Octree">OptimizedPaletteQuantizer.Octree</see> method for any other pixel formats.</para>
        /// <para>If <paramref name="ditherer"/> is <see langword="null"/>, then no ditherer will be auto-selected for the quantization.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="image"/> or <paramref name="stream"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException">No built-in encoder was found or the saving fails on current operating system.</exception>
        public static void SaveAsGif(this Image image, Stream stream, IQuantizer quantizer = null, IDitherer ditherer = null)
        {
            // Shortcut: GIF is saved as a GIF, including animated ones (exploiting the workaround in Image.Save)
            // Without this workaround animated GIFs (which have 32 BPP ARGB format in GDI+) would be converted and a single frame would be saved.
            if (stream != null && image is Bitmap bmp && bmp.RawFormat.Guid == ImageFormat.Gif.Guid && bmp.PixelFormat.ToBitsPerPixel() >= 8)
            {
                ImageCodecInfo encoder = Encoders.FirstOrDefault(e => e.FormatID == ImageFormat.Gif.Guid);
                if (encoder == null)
                    throw new InvalidOperationException(Res.ImageExtensionsNoEncoder(ImageFormat.Gif));
                try
                {
                    bmp.Save(stream, encoder, null);
                    return;
                }
                catch (Exception e)
                {
                    throw new InvalidOperationException(Res.ImageExtensionsEncoderSaveFail(ImageFormat.Gif), e);
                }
            }

            SaveByEncoder(image, stream, ImageFormat.Gif, null, false, quantizer, ditherer);
        }

        /// <summary>
        /// Saves the specified <paramref name="image"/> as a GIF image.
        /// </summary>
        /// <param name="image">The image to save. If image contains multiple images other than animated GIF frames, then only the current image will be saved.</param>
        /// <param name="stream">The stream to save the image into.</param>
        /// <param name="allowDithering"><see langword="true"/>&#160; to allow dithering high color images using a fix palette; otherwise, <see langword="false"/>. This parameter is optional.
        /// <br/>Default value: <see langword="false"/>.</param>
        /// <remarks>
        /// <para>This method is kept for compatibility reasons only and calls the <see cref="SaveAsGif(Image, Stream, IQuantizer, IDitherer)"/> overload with the <see cref="PredefinedColorsQuantizer.SystemDefault8BppPalette">SystemDefault8BppPalette</see> quantizer.</para>
        /// <para>This method no longer relies on the dithering logic of the built-in GIF encoder. Instead, the ditherer is obtained by the <see cref="ErrorDiffusionDitherer.FloydSteinberg">ErrorDiffusionDitherer.FloydSteinberg</see> method if <paramref name="allowDithering"/> is <see langword="true"/>.</para>
        /// </remarks>
        [Obsolete("This overload is kept for compatibility reasons. Use the SaveAsGif(Image, Stream, IQuantizer, IDitherer) overload instead.")]
        public static void SaveAsGif(this Image image, Stream stream, bool allowDithering)
            => SaveAsGif(image, stream,
                image is Bitmap bmp && bmp.PixelFormat.IsIndexed() ? null : PredefinedColorsQuantizer.SystemDefault8BppPalette(),
                allowDithering ? ErrorDiffusionDitherer.FloydSteinberg() : null);

        /// <summary>
        /// Saves the specified <paramref name="image"/> as a GIF image.
        /// </summary>
        /// <param name="image">The image to save. If image contains multiple images other than animated GIF frames, then only the current image will be saved.</param>
        /// <param name="stream">The stream to save the image into.</param>
        /// <param name="palette">The desired custom palette to use. If <see langword="null"/>, and a palette cannot be taken from the source image, then a default palette will be used.
        /// This parameter is ignored if <paramref name="image"/> has already a palette.</param>
        /// <remarks>
        /// <para>This method is kept for compatibility reasons only and calls the <see cref="SaveAsGif(Image, Stream, IQuantizer, IDitherer)"/> overload with a quantizer obtained by the <see cref="PredefinedColorsQuantizer.FromCustomPalette(Color[],Color,byte)">PredefinedColorsQuantizer.FromCustomPalette</see> method.</para>
        /// </remarks>
        [Obsolete("This overload is kept for compatibility reasons. Use the SaveAsGif(Image, Stream, IQuantizer, IDitherer) overload instead.")]
        public static void SaveAsGif(this Image image, Stream stream, Color[] palette)
            => SaveAsGif(image, stream, PredefinedColorsQuantizer.FromCustomPalette(palette));

        /// <summary>
        /// Saves the specified <paramref name="image"/> using the built-in TIFF encoder if available on current operating system.
        /// Unlike the <see cref="Image.Save(Stream,ImageFormat)"/> method, this one supports every <see cref="PixelFormat"/>.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="image">The image to save. If contains multiple images, then the frames to be saved can be specified by the <paramref name="currentFrameOnly"/> parameter.</param>
        /// <param name="stream">The stream to save the image into.</param>
        /// <param name="currentFrameOnly"><see langword="true"/>&#160;to save only the current frame of the specified <paramref name="image"/>;
        /// <see langword="false"/>&#160;to save all frames. The frames can represent pages, animation and resolution dimensions but in any case they will be saved as pages.</param>
        /// <remarks>
        /// <para>The <paramref name="image"/> can only be saved if a built-in TIFF encoder is available in the current operating system.</para>
        /// <para>If <paramref name="currentFrameOnly"/> is <see langword="false"/>&#160;and <paramref name="image"/> is an icon, then images of the same resolution but lower color depth might not be saved.</para>
        /// <para>Images with different <see cref="PixelFormat"/>s are handled as follows (on Windows, unless specified otherwise):
        /// <list type="definition">
        /// <item><term><see cref="PixelFormat.Format1bppIndexed"/></term><description>If palette is black and white (in this order), then pixel format will be preserved.
        /// Otherwise, before saving the image pixel format will be converted to <see cref="PixelFormat.Format4bppIndexed"/> so the built-in encoder will preserve palette.</description></item>
        /// <item><term><see cref="PixelFormat.Format4bppIndexed"/></term><description>When reloading the saved image the pixel format is preserved.</description></item>
        /// <item><term><see cref="PixelFormat.Format8bppIndexed"/></term><description>When reloading the saved image the pixel format is preserved.</description></item>
        /// <item><term><see cref="PixelFormat.Format16bppRgb565"/></term><description>When reloading the saved image the pixel format will turn <see cref="PixelFormat.Format24bppRgb"/>.</description></item>
        /// <item><term><see cref="PixelFormat.Format16bppRgb555"/></term><description>When reloading the saved image the pixel format will turn <see cref="PixelFormat.Format24bppRgb"/>.</description></item>
        /// <item><term><see cref="PixelFormat.Format16bppArgb1555"/></term><description>When reloading the saved image the pixel format will turn <see cref="PixelFormat.Format32bppArgb"/>.</description></item>
        /// <item><term><see cref="PixelFormat.Format16bppGrayScale"/></term><description>Before saving the image pixel format will be converted to <see cref="PixelFormat.Format8bppIndexed"/>
        /// using a grayscale palette, because otherwise GDI+ would throw an exception.</description></item>
        /// <item><term><see cref="PixelFormat.Format24bppRgb"/></term><description>When reloading the saved image the pixel format is preserved.</description></item>
        /// <item><term><see cref="PixelFormat.Format32bppRgb"/></term><description>When reloading the saved image the pixel format will turn <see cref="PixelFormat.Format32bppArgb"/>.</description></item>
        /// <item><term><see cref="PixelFormat.Format32bppArgb"/></term><description>When reloading the saved image the pixel format is preserved.</description></item>
        /// <item><term><see cref="PixelFormat.Format32bppPArgb"/></term><description>When reloading the saved image the pixel format will turn <see cref="PixelFormat.Format32bppArgb"/>.</description></item>
        /// <item><term><see cref="PixelFormat.Format48bppRgb"/></term><description>If the original <paramref name="image"/> is already a 48 BPP TIFF image, then the pixel format is preserved (however,
        /// channels might be quantized using a 13 BPP resolution); otherwise, the image will be saved with <see cref="PixelFormat.Format24bppRgb"/> pixel format.</description></item>
        /// <item><term><see cref="PixelFormat.Format64bppArgb"/></term><description>When reloading the saved image the pixel format will turn <see cref="PixelFormat.Format32bppArgb"/>.</description></item>
        /// <item><term><see cref="PixelFormat.Format64bppPArgb"/></term><description>When reloading the saved image the pixel format will turn <see cref="PixelFormat.Format32bppArgb"/>.</description></item>
        /// </list>
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="image"/> or <paramref name="stream"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException">No built-in encoder was found or the saving fails on current operating system.</exception>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "bmp is disposed if it is not the same as image.")]
        public static void SaveAsTiff(this Image image, Stream stream, bool currentFrameOnly = true)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image), PublicResources.ArgumentNull);
            if (stream == null)
                throw new ArgumentNullException(nameof(stream), PublicResources.ArgumentNull);

            Bitmap bmp = image as Bitmap;
            if (!currentFrameOnly && bmp != null)
            {
                // checking if image has multiple frames
                FrameDimension dimension = null;
                Guid[] dimensions = bmp.FrameDimensionsList;
                if (dimensions.Length > 0)
                {
                    if (dimensions[0] == FrameDimension.Page.Guid)
                        dimension = FrameDimension.Page;
                    else if (dimensions[0] == FrameDimension.Time.Guid)
                        dimension = FrameDimension.Time;
                    else if (dimensions[0] == FrameDimension.Resolution.Guid)
                        dimension = FrameDimension.Resolution;
                }

                int frameCount = dimension != null ? bmp.GetFrameCount(dimension) : 0;
                if (frameCount > 1 || frameCount <= 1 && bmp.RawFormat.Guid == ImageFormat.Icon.Guid)
                {
                    Bitmap[] frames = bmp.ExtractIconImages();
                    try
                    {
                        frames.SaveAsMultipageTiff(stream);
                        return;
                    }
                    finally
                    {
                        frames.ForEach(b => b.Dispose());
                    }
                }
            }

            // converting non BW 1 BPP image to 4 BPP in order to preserve palette colors
            if (bmp != null && bmp.PixelFormat == PixelFormat.Format1bppIndexed)
            {
                var palette = bmp.Palette.Entries;
                if (palette[0].ToArgb() != Color.Black.ToArgb() || palette[1].ToArgb() != Color.White.ToArgb())
                    bmp = bmp.ConvertPixelFormat(PixelFormat.Format4bppIndexed);
            }

            try
            {
                using (var encoderParams = new EncoderParameters(1))
                {
                    // On Windows 10 it doesn't make any difference; otherwise, this provides the best compression
                    encoderParams.Param[0] = new EncoderParameter(Encoder.Compression, (long)EncoderValue.CompressionLZW);
                    SaveByEncoder(bmp != image ? bmp : image, stream, ImageFormat.Tiff, encoderParams, false);
                }
            }
            finally
            {
                if (!ReferenceEquals(image, bmp))
                    bmp?.Dispose();
            }
        }

        /// <summary>
        /// Saves the provided <paramref name="images"/> as a multi-page TIFF into the specified <see cref="Stream"/>.
        /// When <see cref="Image"/> instances in <paramref name="images"/> contain already multiple pages, only the current page is taken.
        /// </summary>
        /// <param name="images">The images to save into the TIFF data stream.</param>
        /// <param name="stream">The stream into the TIFF data is to be saved.</param>
        /// <remarks>
        /// <para>When <paramref name="images"/> contain multi-page instances, this method takes only the current page. You can extract
        /// images by <see cref="BitmapExtensions.ExtractBitmaps">ExtractBitmaps</see> extension method.</para>
        /// <note>On non-Windows platform this method may throw a <see cref="NotSupportedException"/> if <paramref name="images"/> has multiple elements.</note>
        /// </remarks>
        public static void SaveAsMultipageTiff(this IEnumerable<Image> images, Stream stream)
        {
            if (images == null)
                throw new ArgumentNullException(nameof(images), PublicResources.ArgumentNull);
            if (stream == null)
                throw new ArgumentNullException(nameof(stream), PublicResources.ArgumentNull);

            ImageCodecInfo tiffEncoder = Encoders.FirstOrDefault(e => e.FormatID == ImageFormat.Tiff.Guid);
            if (tiffEncoder == null)
                throw new InvalidOperationException(Res.ImageExtensionsNoEncoder(ImageFormat.Tiff));

            Image tiff = null;
            foreach (Image page in images)
            {
                if (page == null)
                    throw new ArgumentException(PublicResources.ArgumentContainsNull, nameof(images));

                using (var encoderParams = new EncoderParameters(2))
                {
                    // LZW is always shorter, and non-BW palette is enabled, too (except on Windows 10 where it makes no difference)
                    encoderParams.Param[0] = new EncoderParameter(Encoder.Compression, (long)EncoderValue.CompressionLZW);

                    // Not setting the color depth any more. Auto selection works fine and also depends on raw format.
                    // For example, 48bpp cannot be set (invalid parameter) but an already 48bpp TIFF is saved with 48bpp rather than 24.
                    //encoderParams.Param[1] = new EncoderParameter(Encoder.ColorDepth, page.PixelFormat.ToBitsPerPixel());

                    // saving the first page with MultiFrame parameter
                    if (tiff == null)
                    {
                        tiff = page;
                        encoderParams.Param[1] = new EncoderParameter(Encoder.SaveFlag, (long)EncoderValue.MultiFrame);
                        tiff.Save(stream, tiffEncoder, encoderParams);
                    }
                    // saving subsequent pages
                    else
                    {
                        encoderParams.Param[1] = new EncoderParameter(Encoder.SaveFlag, (long)EncoderValue.FrameDimensionPage);
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
        /// Saves the specified <paramref name="image"/> with a custom Icon encoder.
        /// Unlike the <see cref="Image.Save(Stream,ImageFormat)"/> method, this one supports every <see cref="PixelFormat"/> and does not save a PNG stream when no Icon encoder can be found.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="image">The image to save. If image contains multiple images other than multi resolution icon bitmaps, then only the current image will be saved.</param>
        /// <param name="stream">The stream to save the image into.</param>
        /// <param name="forceUncompressedResult"><see langword="true"/>&#160;to force saving an uncompressed icon;
        /// <see langword="false"/>&#160;to allow PNG compression, which is supported by Windows Vista and above. This parameter is optional.
        /// <br/>Default value: <see langword="false"/>.</param>
        /// <remarks>
        /// <para>The <paramref name="image"/> can be saved even without a registered Icon encoder in the current operating system.</para>
        /// <para>If the saved image is reloaded by the <see cref="Bitmap(Stream)"/> constructor, then it will have always <see cref="PixelFormat.Format32bppArgb"/> pixel format.
        /// The indexed and 24 BPP pixel formats are preserved though if the saved stream is reloaded by the <see cref="Icon(Stream)"/> constructor.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="image"/> or <paramref name="stream"/> is <see langword="null"/>.</exception>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "bmp is disposed if it is not the same as image.")]
        public static void SaveAsIcon(this Image image, Stream stream, bool forceUncompressedResult = false)
            => SaveAsIcon(new[] { image }, stream, forceUncompressedResult);

        /// <summary>
        /// Saves the specified <paramref name="images"/> with a custom Icon encoder.
        /// Unlike the <see cref="Image.Save(Stream,ImageFormat)"/> method, this one supports every <see cref="PixelFormat"/> and does not save a PNG stream when no Icon encoder can be found.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="images">The images to save as a single icon.</param>
        /// <param name="stream">The stream to save the images into.</param>
        /// <param name="forceUncompressedResult"><see langword="true"/>&#160;to force saving an uncompressed icon;
        /// <see langword="false"/>&#160;to allow PNG compression, which is supported by Windows Vista and above. This parameter is optional.
        /// <br/>Default value: <see langword="false"/>.</param>
        /// <remarks>
        /// <para>The icon can be saved even without a registered Icon encoder in the current operating system.</para>
        /// <para>If the saved image is reloaded by the <see cref="Bitmap(Stream)"/> constructor, then it will have always <see cref="PixelFormat.Format32bppArgb"/> pixel format.
        /// The indexed and 24 BPP pixel formats are preserved though if the saved stream is reloaded by the <see cref="Icon(Stream)"/> constructor.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="images"/> or <paramref name="stream"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="images"/> contains a <see langword="null"/>&#160;element.</exception>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "bmp is disposed if it is not the same as image.")]
#if !NET35
        [SecuritySafeCritical]
#endif
        public static void SaveAsIcon(this IEnumerable<Image> images, Stream stream, bool forceUncompressedResult = false)
        {
            if (images == null)
                throw new ArgumentNullException(nameof(images), PublicResources.ArgumentNull);
            if (stream == null)
                throw new ArgumentNullException(nameof(stream), PublicResources.ArgumentNull);

            using (RawIcon rawIcon = new RawIcon())
            {
                foreach (Image image in images)
                {
                    if (image == null)
                        throw new ArgumentException(PublicResources.ArgumentContainsNull, nameof(images));

                    Bitmap bmp = image as Bitmap ?? new Bitmap(image);

                    try
                    {
                        rawIcon.Add(bmp); // bmp can be an icon with more images
                        rawIcon.Save(stream, forceUncompressedResult);
                    }
                    finally
                    {
                        if (!ReferenceEquals(bmp, image))
                            bmp.Dispose();
                    }
                }
            }
        }

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

        [SuppressMessage("ReSharper", "AccessToDisposedClosure", Justification = "ParallelHelper.For invokes delegates before returning")]
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

        [SuppressMessage("ReSharper", "AccessToDisposedClosure", Justification = "ParallelHelper.For invokes delegates before returning")]
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

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "bmp is disposed if it is not the same as image.")]
        private static void SaveByEncoder(Image image, Stream stream, ImageFormat imageFormat, EncoderParameters encoderParameters, bool isFallback, IQuantizer quantizer = null, IDitherer ditherer = null)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image), PublicResources.ArgumentNull);
            if (stream == null)
                throw new ArgumentNullException(nameof(stream), PublicResources.ArgumentNull);

            Bitmap bmp = image as Bitmap;

            // Metafile: recursion with bitmap
            if (bmp == null)
            {
                using (bmp = new Bitmap(image, image.Size))
                {
                    SaveByEncoder(bmp, stream, imageFormat, encoderParameters, isFallback, quantizer, ditherer);
                    return;
                }
            }

            ImageCodecInfo encoder = Encoders.FirstOrDefault(e => e.FormatID == imageFormat.Guid);
            if (encoder == null)
                throw new InvalidOperationException(Res.ImageExtensionsNoEncoder(imageFormat));

            // To avoid various issues with some encoders and pixel formats we may convert pixel format before saving
            Dictionary<PixelFormat, PixelFormat> transformations = saveConversions[imageFormat.Guid];
            PixelFormat srcPixelFormat = image.PixelFormat;
            if (transformations.TryGetValue(srcPixelFormat, out PixelFormat dstPixelFormat))
            {
                int srcBpp = srcPixelFormat.ToBitsPerPixel();
                int dstBpp = dstPixelFormat.ToBitsPerPixel();
                if (quantizer == null && dstPixelFormat.IsIndexed() && srcBpp > dstBpp)
                {
                    // auto setting quantizer if target is indexed and conversion is from higher BPP
                    quantizer = srcPixelFormat == PixelFormat.Format16bppGrayScale
                        ? PredefinedColorsQuantizer.Grayscale8bpp()
                        : (IQuantizer)OptimizedPaletteQuantizer.Octree();
                }
                else if (quantizer != null && dstPixelFormat.IsIndexed() && srcBpp < dstBpp)
                {
                    // ignoring quantizer if (using source palette) if converting to higher indexed BPP
                    quantizer = null;
                    ditherer = null;
                }

                if (ditherer != null && !dstPixelFormat.CanBeDithered())
                    ditherer = null;

                bmp = ConvertPixelFormat(image, dstPixelFormat, quantizer, ditherer);
            }

            try
            {
                bmp.Save(stream, encoder, encoderParameters);
            }
            catch (Exception e)
            {
                // On failure trying to use a fallback pixel format and omitting all parameters. This should not occur on Windows.
                if (!isFallback && transformations.TryGetValue(PixelFormat.Undefined, out PixelFormat fallbackPixelFormat) && fallbackPixelFormat != bmp.PixelFormat)
                {
                    using (var fallbackBmp = bmp.ConvertPixelFormat(fallbackPixelFormat))
                    {
                        SaveByEncoder(fallbackBmp, stream, imageFormat, null, true, quantizer, ditherer);
                        return;
                    }
                }

                // Otherwise, we give up
                throw new InvalidOperationException(Res.ImageExtensionsEncoderSaveFail(imageFormat), e);
            }
            finally
            {
                if (!ReferenceEquals(image, bmp))
                    bmp.Dispose();
            }
        }

        #endregion

        #endregion
    }
}
