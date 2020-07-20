#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BitmapDataExtensions.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2020 - All Rights Reserved
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
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;

using KGySoft.CoreLibraries;

#endregion

namespace KGySoft.Drawing.Imaging
{
    public static partial class BitmapDataExtensions
    {
        #region Constants

        private const int parallelThreshold = 100;

        #endregion

        #region Methods

        #region Public Methods

        #region IReadableBitmapData

        #region Clone

        /// <summary>
        /// Gets the clone of the specified <paramref name="source"/> with identical size and pixel format.
        /// </summary>
        /// <param name="source">An <see cref="IReadableBitmapData"/> instance to be cloned.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance that represents the clone of the specified <paramref name="source"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        public static IReadWriteBitmapData Clone(this IReadableBitmapData source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source), PublicResources.ArgumentNull);

            Size size = source.GetSize();
            var session = new CopySession { SourceRectangle = new Rectangle(Point.Empty, size) };
            Unwrap(ref source, ref session.SourceRectangle);
            session.TargetRectangle = session.SourceRectangle;

            session.Source = source as IBitmapDataInternal ?? new BitmapDataWrapper(source, true, false);
            session.Target = BitmapDataFactory.CreateManagedBitmapData(size, source.PixelFormat, source.BackColor, source.AlphaThreshold, source.Palette);

            // raw copy may fail on Windows if source is a wide color Bitmap because of 13 vs 16 bpp color handling
            session.PerformCopy();

            return session.Target;
        }

        /// <summary>
        /// Gets the clone of the specified <paramref name="source"/> with identical size and the specified <paramref name="pixelFormat"/> and color settings.
        /// This method is similar to <see cref="ImageExtensions.ConvertPixelFormat(Image, PixelFormat, Color[], Color, byte)"/> but as the result is a managed <see cref="IReadWriteBitmapData"/> instance
        /// every <see cref="PixelFormat"/> is supported on any platform.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="source">An <see cref="IReadableBitmapData"/> instance to be cloned.</param>
        /// <param name="pixelFormat">The desired pixel format of the result.</param>
        /// <param name="backColor">If <paramref name="pixelFormat"/> does not support alpha or supports only single-bit alpha, then specifies the color of the background.
        /// Source pixels with alpha, which will be opaque in the result will be blended with this color.
        /// The <see cref="Color32.A">Color32.A</see> property of the background color is ignored. This parameter is optional.
        /// <br/>Default value: The default value of the <see cref="Color32"/> type, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="alphaThreshold">If <paramref name="pixelFormat"/> can represent only single-bit alpha or <paramref name="pixelFormat"/> is an indexed format and the target <paramref name="palette"/> contains a transparent color,
        /// then specifies a threshold value for the <see cref="Color32.A">Color32.A</see> property, under which the color is considered transparent. If 0,
        /// then the result will not have transparent pixels. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <param name="palette">The desired target palette if <paramref name="pixelFormat"/> is an indexed format. If <see langword="null"/>,
        /// then the source palette is taken from <paramref name="source"/> if it also has a palette of no more entries than the target indexed format can have;
        /// otherwise, a default palette will be used based on <paramref name="pixelFormat"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance that represents the clone of the specified <paramref name="source"/>.</returns>
        /// <remarks>
        /// <para>This overload automatically quantizes colors if <paramref name="pixelFormat"/> represents a narrower set of colors than <paramref name="source"/> <see cref="IBitmapData.PixelFormat"/>.
        /// To use a custom quantizer use the overloads with a <c>quantizer</c> parameter.</para>
        /// <para>Color depth of wide-color formats (<see cref="PixelFormat.Format16bppGrayScale"/>, <see cref="PixelFormat.Format48bppRgb"/>, <see cref="PixelFormat.Format64bppArgb"/>, <see cref="PixelFormat.Format64bppPArgb"/>)
        /// can be preserved only between the same pixel formats. If they are different, or <paramref name="source"/> is from a <see cref="Bitmap"/> on Windows, which uses 13 bits-per-pixel channels, then colors might be quantized to 32bpp
        /// ones during the operation.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="pixelFormat"/> does not specify a valid format.</exception>
        /// <exception cref="ArgumentException"><paramref name="palette"/> contains too many colors for the specified <paramref name="pixelFormat"/>.</exception>
        public static IReadWriteBitmapData Clone(this IReadableBitmapData source, PixelFormat pixelFormat, Color32 backColor = default, byte alphaThreshold = 128, Palette palette = null)
            => Clone(source, new Rectangle(Point.Empty, source?.GetSize() ?? default), pixelFormat, backColor, alphaThreshold, palette);

        /// <summary>
        /// Gets the clone of the specified portion of <paramref name="source"/> with the specified <paramref name="pixelFormat"/> and color settings.
        /// This method is similar to <see cref="ImageExtensions.ConvertPixelFormat(Image, PixelFormat, Color[], Color, byte)"/> but as the result is a managed <see cref="IReadWriteBitmapData"/> instance
        /// every <see cref="PixelFormat"/> is supported on any platform.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="source">An <see cref="IReadableBitmapData"/> instance to be cloned.</param>
        /// <param name="sourceRectangle">A <see cref="Rectangle"/> that specifies the portion of the <paramref name="source"/> to be cloned.</param>
        /// <param name="pixelFormat">The desired pixel format of the result.</param>
        /// <param name="backColor">If <paramref name="pixelFormat"/> does not support alpha or supports only single-bit alpha, then specifies the color of the background.
        /// Source pixels with alpha, which will be opaque in the result will be blended with this color.
        /// The <see cref="Color32.A">Color32.A</see> property of the background color is ignored. This parameter is optional.
        /// <br/>Default value: The default value of the <see cref="Color32"/> type, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="alphaThreshold">If <paramref name="pixelFormat"/> can represent only single-bit alpha or <paramref name="pixelFormat"/> is an indexed format and the target <paramref name="palette"/> contains a transparent color,
        /// then specifies a threshold value for the <see cref="Color32.A">Color32.A</see> property, under which the color is considered transparent. If 0,
        /// then the result will not have transparent pixels. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <param name="palette">The desired target palette if <paramref name="pixelFormat"/> is an indexed format. If <see langword="null"/>, then
        /// then the source palette is taken from <paramref name="source"/> if it also has a palette of no more entries than the target indexed format can have;
        /// otherwise, a default palette will be used based on <paramref name="pixelFormat"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance that represents the clone of the specified <paramref name="source"/>.</returns>
        /// <remarks>
        /// <para>This overload automatically quantizes colors if <paramref name="pixelFormat"/> represents a narrower set of colors than <paramref name="source"/> <see cref="IBitmapData.PixelFormat"/>.
        /// To use a custom quantizer use the overloads with a <c>quantizer</c> parameter.</para>
        /// <para>Color depth of wide-color formats (<see cref="PixelFormat.Format16bppGrayScale"/>, <see cref="PixelFormat.Format48bppRgb"/>, <see cref="PixelFormat.Format64bppArgb"/>, <see cref="PixelFormat.Format64bppPArgb"/>)
        /// can be preserved only between the same pixel formats. If they are different, or <paramref name="source"/> is from a <see cref="Bitmap"/> on Windows, which uses 13 bits-per-pixel channels, then colors might be quantized to 32bpp
        /// ones during the operation.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="pixelFormat"/> does not specify a valid format.
        /// <br/>-or-
        /// <br/><paramref name="sourceRectangle"/> has no overlapping region with source bounds.</exception>
        /// <exception cref="ArgumentException"><paramref name="palette"/> contains too many colors for the specified <paramref name="pixelFormat"/>.</exception>
        public static IReadWriteBitmapData Clone(this IReadableBitmapData source, Rectangle sourceRectangle, PixelFormat pixelFormat, Color32 backColor = default, byte alphaThreshold = 128, Palette palette = null)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source), PublicResources.ArgumentNull);

            var session = new CopySession();
            var sourceBounds = new Rectangle(default, source.GetSize());
            Unwrap(ref source, ref sourceRectangle);
            (session.SourceRectangle, session.TargetRectangle) = GetActualRectangles(sourceBounds, sourceRectangle, new Rectangle(Point.Empty, sourceBounds.Size), Point.Empty);
            if (session.SourceRectangle.IsEmpty || session.TargetRectangle.IsEmpty)
                throw new ArgumentOutOfRangeException(nameof(sourceRectangle), PublicResources.ArgumentOutOfRange);

            if (palette == null)
            {
                int bpp = pixelFormat.ToBitsPerPixel();
                if (bpp <= 8 && source.Palette?.Entries.Length <= (1 << bpp))
                    palette = source.Palette;
            }

            session.Source = source as IBitmapDataInternal ?? new BitmapDataWrapper(source, true, false);

            // using the public factory so pixelFormat and palette will be validated
            session.Target = (IBitmapDataInternal)BitmapDataFactory.CreateBitmapData(session.TargetRectangle.Size, pixelFormat, backColor, alphaThreshold, palette);
            session.PerformCopy();

            return session.Target;
        }

        /// <summary>
        /// Gets the clone of the specified <paramref name="source"/> with identical size and the specified <paramref name="pixelFormat"/>, using an optional <paramref name="quantizer"/> and <paramref name="ditherer"/>.
        /// This method is similar to <see cref="ImageExtensions.ConvertPixelFormat(Image, PixelFormat, IQuantizer, IDitherer)"/> but as the result is a managed <see cref="IReadWriteBitmapData"/> instance
        /// every <see cref="PixelFormat"/> is supported on any platform.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="source">An <see cref="IReadableBitmapData"/> instance to be cloned.</param>
        /// <param name="pixelFormat">The desired pixel format of the result.</param>
        /// <param name="quantizer">An optional <see cref="IQuantizer"/> instance to determine the colors of the result.
        /// If <see langword="null"/>&#160;and <paramref name="pixelFormat"/> is an indexed format, then a default palette and quantization logic will be used.</param>
        /// <param name="ditherer">The ditherer to be used. Might be ignored if <paramref name="quantizer"/> is not specified
        /// and <paramref name="pixelFormat"/> represents an at least 24 bits-per-pixel size. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance that represents the clone of the specified <paramref name="source"/>.</returns>
        /// <remarks>
        /// <para>If <paramref name="pixelFormat"/> can represent a narrower set of colors, then the result will be automatically quantized to its color space,
        /// even if there is no <paramref name="quantizer"/> specified. To use dithering a <paramref name="ditherer"/> must be explicitly specified though.</para>
        /// <para>If <paramref name="quantizer"/> is specified but it uses more/different colors than <paramref name="pixelFormat"/> can represent,
        /// then the result will eventually quantized, though the result may have a poorer quality than expected.</para>
        /// <para>Color depth of wide-color formats (<see cref="PixelFormat.Format16bppGrayScale"/>, <see cref="PixelFormat.Format48bppRgb"/>, <see cref="PixelFormat.Format64bppArgb"/>, <see cref="PixelFormat.Format64bppPArgb"/>)
        /// can be preserved only between the same pixel formats if there is no <paramref name="quantizer"/> specified. If pixel formats are different, or <paramref name="source"/> is from a <see cref="Bitmap"/> on Windows,
        /// which uses 13 bits-per-pixel channels, then colors might be quantized to 32bpp ones during the operation.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="pixelFormat"/> does not specify a valid format.</exception>
        /// <exception cref="ArgumentException"><paramref name="quantizer"/> uses a palette with too many colors for the specified <paramref name="pixelFormat"/>.</exception>
        public static IReadWriteBitmapData Clone(this IReadableBitmapData source, PixelFormat pixelFormat, IQuantizer quantizer, IDitherer ditherer = null)
            => Clone(source, new Rectangle(Point.Empty, source?.GetSize() ?? default), pixelFormat, quantizer, ditherer);

        /// <summary>
        /// Gets the clone of the specified <paramref name="source"/> with identical size and the specified <paramref name="pixelFormat"/>, using an optional <paramref name="ditherer"/>.
        /// This method is similar to <see cref="ImageExtensions.ConvertPixelFormat(Image, PixelFormat, IQuantizer, IDitherer)"/> but as the result is a managed <see cref="IReadWriteBitmapData"/> instance
        /// every <see cref="PixelFormat"/> is supported on any platform.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="source">An <see cref="IReadableBitmapData"/> instance to be cloned.</param>
        /// <param name="pixelFormat">The desired pixel format of the result.</param>
        /// <param name="ditherer">The ditherer to be used. Might be ignored if <paramref name="pixelFormat"/> represents an at least 24 bits-per-pixel size.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance that represents the clone of the specified <paramref name="source"/>.</returns>
        /// <remarks>
        /// <para>If <paramref name="pixelFormat"/> can represent a narrower set of colors, then the result will be automatically quantized to its color space.
        /// To use dithering a <paramref name="ditherer"/> must be explicitly specified.</para>
        /// <para>Color depth of wide-color formats (<see cref="PixelFormat.Format16bppGrayScale"/>, <see cref="PixelFormat.Format48bppRgb"/>, <see cref="PixelFormat.Format64bppArgb"/>, <see cref="PixelFormat.Format64bppPArgb"/>)
        /// can be preserved only between the same pixel formats. If pixel formats are different, or <paramref name="source"/> is from a <see cref="Bitmap"/> on Windows,
        /// which uses 13 bits-per-pixel channels, then colors might be quantized to 32bpp ones during the operation.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="pixelFormat"/> does not specify a valid format.</exception>
        public static IReadWriteBitmapData Clone(this IReadableBitmapData source, PixelFormat pixelFormat, IDitherer ditherer)
            => Clone(source, new Rectangle(Point.Empty, source?.GetSize() ?? default), pixelFormat, null, ditherer);

        /// <summary>
        /// Gets the clone of the specified portion of <paramref name="source"/> with the specified <paramref name="pixelFormat"/>, using an optional <paramref name="ditherer"/>.
        /// This method is similar to <see cref="ImageExtensions.ConvertPixelFormat(Image, PixelFormat, IQuantizer, IDitherer)"/> but as the result is a managed <see cref="IReadWriteBitmapData"/> instance
        /// every <see cref="PixelFormat"/> is supported on any platform.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="source">An <see cref="IReadableBitmapData"/> instance to be cloned.</param>
        /// <param name="sourceRectangle">A <see cref="Rectangle"/> that specifies the portion of the <paramref name="source"/> to be cloned.</param>
        /// <param name="pixelFormat">The desired pixel format of the result.</param>
        /// <param name="ditherer">The ditherer to be used. Might be ignored if <paramref name="pixelFormat"/> represents an at least 24 bits-per-pixel size.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance that represents the clone of the specified <paramref name="source"/>.</returns>
        /// <remarks>
        /// <para>If <paramref name="pixelFormat"/> can represent a narrower set of colors, then the result will be automatically quantized to its color space.
        /// To use dithering a <paramref name="ditherer"/> must be explicitly specified.</para>
        /// <para>Color depth of wide-color formats (<see cref="PixelFormat.Format16bppGrayScale"/>, <see cref="PixelFormat.Format48bppRgb"/>, <see cref="PixelFormat.Format64bppArgb"/>, <see cref="PixelFormat.Format64bppPArgb"/>)
        /// can be preserved only between the same pixel formats. If pixel formats are different, or <paramref name="source"/> is from a <see cref="Bitmap"/> on Windows,
        /// which uses 13 bits-per-pixel channels, then colors might be quantized to 32bpp ones during the operation.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="pixelFormat"/> does not specify a valid format.
        /// <br/>-or-
        /// <br/><paramref name="sourceRectangle"/> has no overlapping region with source bounds.</exception>
        public static IReadWriteBitmapData Clone(this IReadableBitmapData source, Rectangle sourceRectangle, PixelFormat pixelFormat, IDitherer ditherer)
            => Clone(source, sourceRectangle, pixelFormat, null, ditherer);

        /// <summary>
        /// Gets the clone of the specified portion of <paramref name="source"/> with the specified <paramref name="pixelFormat"/>, using an optional <paramref name="quantizer"/> and <paramref name="ditherer"/>.
        /// This method is similar to <see cref="ImageExtensions.ConvertPixelFormat(Image, PixelFormat, IQuantizer, IDitherer)"/> but as the result is a managed <see cref="IReadWriteBitmapData"/> instance
        /// every <see cref="PixelFormat"/> is supported on any platform.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="source">An <see cref="IReadableBitmapData"/> instance to be cloned.</param>
        /// <param name="sourceRectangle">A <see cref="Rectangle"/> that specifies the portion of the <paramref name="source"/> to be cloned.</param>
        /// <param name="pixelFormat">The desired pixel format of the result.</param>
        /// <param name="quantizer">An optional <see cref="IQuantizer"/> instance to determine the colors of the result.
        /// If <see langword="null"/>&#160;and <paramref name="pixelFormat"/> is an indexed format, then a default palette and quantization logic will be used.</param>
        /// <param name="ditherer">The ditherer to be used. Might be ignored if <paramref name="quantizer"/> is not specified
        /// and <paramref name="pixelFormat"/> represents an at least 24 bits-per-pixel size. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance that represents the clone of the specified <paramref name="source"/>.</returns>
        /// <remarks>
        /// <para>If <paramref name="pixelFormat"/> can represent a narrower set of colors, then the result will be automatically quantized to its color space,
        /// even if there is no <paramref name="quantizer"/> specified. To use dithering a <paramref name="ditherer"/> must be explicitly specified though.</para>
        /// <para>If <paramref name="quantizer"/> is specified but it uses more/different colors than <paramref name="pixelFormat"/> can represent,
        /// then the result will eventually quantized, though the result may have a poorer quality than expected.</para>
        /// <para>Color depth of wide-color formats (<see cref="PixelFormat.Format16bppGrayScale"/>, <see cref="PixelFormat.Format48bppRgb"/>, <see cref="PixelFormat.Format64bppArgb"/>, <see cref="PixelFormat.Format64bppPArgb"/>)
        /// can be preserved only between the same pixel formats if there is no <paramref name="quantizer"/> specified. If pixel formats are different, or <paramref name="source"/> is from a <see cref="Bitmap"/> on Windows,
        /// which uses 13 bits-per-pixel channels, then colors might be quantized to 32bpp ones during the operation.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="pixelFormat"/> does not specify a valid format.
        /// <br/>-or-
        /// <br/><paramref name="sourceRectangle"/> has no overlapping region with source bounds.</exception>
        /// <exception cref="ArgumentException"><paramref name="quantizer"/> uses a palette with too many colors for the specified <paramref name="pixelFormat"/>.</exception>
        [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "False alarm, initSource is disposed if needed")]
        public static IReadWriteBitmapData Clone(this IReadableBitmapData source, Rectangle sourceRectangle, PixelFormat pixelFormat, IQuantizer quantizer, IDitherer ditherer = null)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source), PublicResources.ArgumentNull);

            if (quantizer == null)
            {
                // copying without using a quantizer (even if only a ditherer is specified for a high-bpp pixel format)
                // Note: Not using source.BackColor/AlphaThreshold/Palette so the behavior will be compatible with the other overload with default parameters
                if (ditherer == null || !pixelFormat.CanBeDithered())
                    return Clone(source, pixelFormat);

                // here we need to pick a quantizer for the dithering
                int bpp = pixelFormat.ToBitsPerPixel();
                quantizer = bpp <= 8 && source.Palette?.Entries.Length <= (1 << bpp)
                    ? PredefinedColorsQuantizer.FromCustomPalette(source.Palette)
                    : PredefinedColorsQuantizer.FromPixelFormat(pixelFormat);
            }

            var session = new CopySession();
            var sourceBounds = new Rectangle(default, source.GetSize());
            Unwrap(ref source, ref sourceRectangle);
            (session.SourceRectangle, session.TargetRectangle) = GetActualRectangles(sourceBounds, sourceRectangle, new Rectangle(Point.Empty, sourceBounds.Size), Point.Empty);
            if (session.SourceRectangle.IsEmpty || session.TargetRectangle.IsEmpty)
                throw new ArgumentOutOfRangeException(nameof(sourceRectangle), PublicResources.ArgumentOutOfRange);

            // Using a clipped source for quantizer/ditherer if needed. Note: the CopySession uses the original source for the best performance
            IReadableBitmapData initSource = session.SourceRectangle.Size == source.GetSize()
                ? source
                : source.Clip(session.SourceRectangle);

            try
            {
                using (IQuantizingSession quantizingSession = quantizer.Initialize(initSource) ?? throw new InvalidOperationException(Res.ImagingQuantizerInitializeNull))
                {
                    session.Source = source as IBitmapDataInternal ?? new BitmapDataWrapper(source, true, false);
                    session.Target = (IBitmapDataInternal)BitmapDataFactory.CreateBitmapData(session.TargetRectangle.Size, pixelFormat, quantizingSession.BackColor, quantizingSession.AlphaThreshold, quantizingSession.Palette);

                    // quantizing without dithering
                    if (ditherer == null)
                        session.PerformCopyWithQuantizer(quantizingSession, false);
                    else
                    {
                        // quantizing with dithering
                        using IDitheringSession ditheringSession = ditherer.Initialize(initSource, quantizingSession) ?? throw new InvalidOperationException(Res.ImagingDithererInitializeNull);
                        session.PerformCopyWithDithering(quantizingSession, ditheringSession, false);
                    }

                    return session.Target;
                }
            }
            finally
            {
                if (!ReferenceEquals(initSource, source))
                    initSource.Dispose();
            }
        }
        
        #endregion

        #region CopyTo

        /// <summary>
        /// Copies the <paramref name="source"/>&#160;<see cref="IReadableBitmapData"/> into the <paramref name="target"/>&#160;<see cref="IWritableBitmapData"/>
        /// without scaling and blending. This method works between any pair of source and target <see cref="PixelFormat"/>s and supports quantizing and dithering.
        /// To draw a bitmap data into another one with blending use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.DrawInto">DrawInto</see> methods instead.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="source">The source <see cref="IReadableBitmapData"/> to be copied into the <paramref name="target"/>.</param>
        /// <param name="target">The target <see cref="IWritableBitmapData"/> into which <paramref name="source"/> should be copied.</param>
        /// <param name="targetLocation">The target location. Target size will be always the same as the source size. This parameter is optional.
        /// <br/>Default value: <see cref="Point.Empty">Point.Empty</see>.</param>
        /// <param name="quantizer">An <see cref="IQuantizer"/> instance to be used. If not specified, then the copying operation might automatically
        /// pick a quantizer based on <paramref name="target"/> <see cref="IBitmapData.PixelFormat"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="ditherer">The ditherer to be used. Might be ignored if <paramref name="quantizer"/> is not specified
        /// and <paramref name="target"/> <see cref="IBitmapData.PixelFormat"/> format has at least 24 bits-per-pixel size. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <remarks>
        /// <para>The copied area is automatically clipped if its size or <paramref name="targetLocation"/> makes it impossible to completely fit in the <paramref name="target"/>.</para>
        /// <para>If <paramref name="target"/> can represent a narrower set of colors, then the result will be automatically quantized to the colors of the <paramref name="target"/>,
        /// even if there is no <paramref name="quantizer"/> specified. To use dithering a <paramref name="ditherer"/> must be explicitly specified though.</para>
        /// <para>If <paramref name="quantizer"/> is specified but it uses more/different colors than <paramref name="target"/> can represent,
        /// then the result will eventually quantized to <paramref name="target"/>, though the result may have a poorer quality than expected.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="target"/> is <see langword="null"/>.</exception>
        public static void CopyTo(this IReadableBitmapData source, IWritableBitmapData target, Point targetLocation = default, IQuantizer quantizer = null, IDitherer ditherer = null)
            => CopyTo(source, target, new Rectangle(Point.Empty, source?.GetSize() ?? default), targetLocation, quantizer, ditherer);

        /// <summary>
        /// Copies the <paramref name="source"/>&#160;<see cref="IReadableBitmapData"/> into the <paramref name="target"/>&#160;<see cref="IWritableBitmapData"/>
        /// without scaling and blending. This method works between any pair of source and target <see cref="PixelFormat"/>s and supports quantizing and dithering.
        /// To draw a bitmap data into another one with blending use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.DrawInto">DrawInto</see> methods instead.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="source">The source <see cref="IReadableBitmapData"/> to be copied into the <paramref name="target"/>.</param>
        /// <param name="target">The target <see cref="IWritableBitmapData"/> into which <paramref name="source"/> should be copied.</param>
        /// <param name="targetLocation">The target location. Target size will be always the same as the source size.</param>
        /// <param name="ditherer">The ditherer to be used. Might be ignored if <paramref name="target"/> <see cref="IBitmapData.PixelFormat"/> format has at least 24 bits-per-pixel size.</param>
        /// <remarks>
        /// <para>The copied area is automatically clipped if its size or <paramref name="targetLocation"/> makes it impossible to completely fit in the <paramref name="target"/>.</para>
        /// <para>If <paramref name="target"/> can represent a narrower set of colors, then the result will be automatically quantized to the colors of the <paramref name="target"/>.
        /// To use dithering a <paramref name="ditherer"/> must be explicitly specified.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="target"/> is <see langword="null"/>.</exception>
        public static void CopyTo(this IReadableBitmapData source, IWritableBitmapData target, Point targetLocation, IDitherer ditherer)
            => CopyTo(source, target, new Rectangle(Point.Empty, source?.GetSize() ?? default), targetLocation, null, ditherer);

        /// <summary>
        /// Copies the <paramref name="source"/>&#160;<see cref="IReadableBitmapData"/> into the <paramref name="target"/>&#160;<see cref="IWritableBitmapData"/>
        /// without scaling and blending. This method works between any pair of source and target <see cref="PixelFormat"/>s and supports quantizing and dithering.
        /// To draw a bitmap data into another one with blending use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.DrawInto">DrawInto</see> methods instead.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="source">The source <see cref="IReadableBitmapData"/> to be copied into the <paramref name="target"/>.</param>
        /// <param name="target">The target <see cref="IWritableBitmapData"/> into which <paramref name="source"/> should be copied.</param>
        /// <param name="sourceRectangle">A <see cref="Rectangle"/> that specifies the portion of the <paramref name="source"/> to be copied into the <paramref name="target"/>.</param>
        /// <param name="targetLocation">The target location. Target size will be always the same as the source size.</param>
        /// <param name="ditherer">The ditherer to be used. Might be ignored if <paramref name="target"/> <see cref="IBitmapData.PixelFormat"/> format has at least 24 bits-per-pixel size.</param>
        /// <remarks>
        /// <para>The copied area is automatically clipped if its size or <paramref name="targetLocation"/> makes it impossible to completely fit in the <paramref name="target"/>.</para>
        /// <para>If <paramref name="target"/> can represent a narrower set of colors, then the result will be automatically quantized to the colors of the <paramref name="target"/>.
        /// To use dithering a <paramref name="ditherer"/> must be explicitly specified.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="target"/> is <see langword="null"/>.</exception>
        public static void CopyTo(this IReadableBitmapData source, IWritableBitmapData target, Rectangle sourceRectangle, Point targetLocation, IDitherer ditherer)
            => CopyTo(source, target, sourceRectangle, targetLocation, null, ditherer);

        /// <summary>
        /// Copies the <paramref name="source"/>&#160;<see cref="IReadableBitmapData"/> into the <paramref name="target"/>&#160;<see cref="IWritableBitmapData"/>
        /// without scaling and blending. This method works between any pair of source and target <see cref="PixelFormat"/>s and supports quantizing and dithering.
        /// To draw a bitmap data into another one with blending use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.DrawInto">DrawInto</see> methods instead.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="source">The source <see cref="IReadableBitmapData"/> to be copied into the <paramref name="target"/>.</param>
        /// <param name="target">The target <see cref="IWritableBitmapData"/> into which <paramref name="source"/> should be copied.</param>
        /// <param name="sourceRectangle">A <see cref="Rectangle"/> that specifies the portion of the <paramref name="source"/> to be copied into the <paramref name="target"/>.</param>
        /// <param name="targetLocation">The target location. Target size will be always the same as the source size.</param>
        /// <param name="quantizer">An <see cref="IQuantizer"/> instance to be used. If not specified, then the copying operation might automatically
        /// pick a quantizer based on <paramref name="target"/> <see cref="IBitmapData.PixelFormat"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="ditherer">The ditherer to be used. Might be ignored if <paramref name="quantizer"/> is not specified
        /// and <paramref name="target"/> <see cref="IBitmapData.PixelFormat"/> format has at least 24 bits-per-pixel size. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <remarks>
        /// <para>The copied area is automatically clipped if its size or <paramref name="targetLocation"/> makes it impossible to completely fit in the <paramref name="target"/>.</para>
        /// <para>If <paramref name="target"/> can represent a narrower set of colors, then the result will be automatically quantized to the colors of the <paramref name="target"/>,
        /// even if there is no <paramref name="quantizer"/> specified. To use dithering a <paramref name="ditherer"/> must be explicitly specified though.</para>
        /// <para>If <paramref name="quantizer"/> is specified but it uses more/different colors than <paramref name="target"/> can represent,
        /// then the result will eventually quantized to <paramref name="target"/>, though the result may have a poorer quality than expected.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="target"/> is <see langword="null"/>.</exception>
        //[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "False alarm, initSource is disposed if needed")]
        public static void CopyTo(this IReadableBitmapData source, IWritableBitmapData target, Rectangle sourceRectangle, Point targetLocation, IQuantizer quantizer = null, IDitherer ditherer = null)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source), PublicResources.ArgumentNull);
            if (target == null)
                throw new ArgumentNullException(nameof(target), PublicResources.ArgumentNull);

            DoCopy(source, target, sourceRectangle, targetLocation, quantizer, ditherer);
        }

        #endregion

        #region DrawInto

        #region Without resize

        /// <summary>
        /// Draws the <paramref name="source"/>&#160;<see cref="IReadableBitmapData"/> into the <paramref name="target"/>&#160;<see cref="IReadWriteBitmapData"/>
        /// without scaling, using blending. This method is similar to <see cref="Graphics.DrawImage(Image,Point)">Graphics.DrawImage</see>,
        /// methods except that this one always preserves the source size in pixels, works between any pair of source and target <see cref="PixelFormat"/>s and supports quantizing and dithering.
        /// For scaling use the overloads with <c>targetRectangle</c> and <see cref="ScalingMode"/> parameters.
        /// To copy a bitmap data into another one without blending use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.CopyTo">CopyTo</see> methods instead.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="source">The source <see cref="IReadableBitmapData"/> to be drawn into the <paramref name="target"/>.</param>
        /// <param name="target">The target <see cref="IReadWriteBitmapData"/> into which <paramref name="source"/> should be drawn.</param>
        /// <param name="targetLocation">The target location. Target size will be always the same as the source size. This parameter is optional.
        /// <br/>Default value: <see cref="Point.Empty">Point.Empty</see>.</param>
        /// <param name="quantizer">An <see cref="IQuantizer"/> instance to be used for the drawing. If not specified, then the drawing operation might automatically
        /// pick a quantizer based on <paramref name="target"/> <see cref="IBitmapData.PixelFormat"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="ditherer">The ditherer to be used for the drawing. Might be ignored if <paramref name="quantizer"/> is not specified
        /// and <paramref name="target"/> <see cref="IBitmapData.PixelFormat"/> format has at least 24 bits-per-pixel size. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <remarks>
        /// <para>The image to be drawn is automatically clipped if its size or <paramref name="targetLocation"/> makes it impossible to completely fit in the <paramref name="target"/>.</para>
        /// <para><paramref name="target"/> must be an <see cref="IReadWriteBitmapData"/> because it must be readable if blending is necessary. For write-only <see cref="IWritableBitmapData"/> instances
        /// you can use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.CopyTo">CopyTo</see> methods.</para>
        /// <para>If <paramref name="target"/> can represent a narrower set of colors, then the result will be automatically quantized to the colors of the <paramref name="target"/>,
        /// even if there is no <paramref name="quantizer"/> specified. To use dithering a <paramref name="ditherer"/> must be explicitly specified though.</para>
        /// <para>If <paramref name="quantizer"/> is specified but it uses more/different colors than <paramref name="target"/> can represent,
        /// then the result will eventually quantized to <paramref name="target"/>, though the result may have a poorer quality than expected.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="target"/> is <see langword="null"/>.</exception>
        public static void DrawInto(this IReadableBitmapData source, IReadWriteBitmapData target, Point targetLocation = default, IQuantizer quantizer = null, IDitherer ditherer = null)
            => DrawInto(source, target, new Rectangle(Point.Empty, source?.GetSize() ?? default), targetLocation, quantizer, ditherer);

        /// <summary>
        /// Draws the <paramref name="source"/>&#160;<see cref="IReadableBitmapData"/> into the <paramref name="target"/>&#160;<see cref="IReadWriteBitmapData"/>
        /// without scaling, using blending. This method is similar to <see cref="Graphics.DrawImage(Image,Point)">Graphics.DrawImage</see>,
        /// methods except that this one always preserves the source size in pixels, works between any pair of source and target <see cref="PixelFormat"/>s and supports quantizing and dithering.
        /// For scaling use the overloads with <c>targetRectangle</c> and <see cref="ScalingMode"/> parameters.
        /// To copy a bitmap data into another one without blending use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.CopyTo">CopyTo</see> methods instead.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="source">The source <see cref="IReadableBitmapData"/> to be drawn into the <paramref name="target"/>.</param>
        /// <param name="target">The target <see cref="IReadWriteBitmapData"/> into which <paramref name="source"/> should be drawn.</param>
        /// <param name="targetLocation">The target location. Target size will be always the same as the source size.</param>
        /// <param name="ditherer">The ditherer to be used for the drawing. Might be ignored if <paramref name="target"/> <see cref="IBitmapData.PixelFormat"/> format has at least 24 bits-per-pixel size.</param>
        /// <remarks>
        /// <para>The image to be drawn is automatically clipped if its size or <paramref name="targetLocation"/> makes it impossible to completely fit in the <paramref name="target"/>.</para>
        /// <para><paramref name="target"/> must be an <see cref="IReadWriteBitmapData"/> because it must be readable if blending is necessary. For write-only <see cref="IWritableBitmapData"/> instances
        /// you can use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.CopyTo">CopyTo</see> methods.</para>
        /// <para>If <paramref name="target"/> can represent a narrower set of colors, then the result will be automatically quantized to the colors of the <paramref name="target"/>.
        /// To use dithering a <paramref name="ditherer"/> must be explicitly specified.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="target"/> is <see langword="null"/>.</exception>
        public static void DrawInto(this IReadableBitmapData source, IReadWriteBitmapData target, Point targetLocation, IDitherer ditherer)
            => DrawInto(source, target, new Rectangle(Point.Empty, source?.GetSize() ?? default), targetLocation, null, ditherer);

        /// <summary>
        /// Draws the <paramref name="source"/>&#160;<see cref="IReadableBitmapData"/> into the <paramref name="target"/>&#160;<see cref="IReadWriteBitmapData"/>
        /// without scaling, using blending. This method is similar to <see cref="Graphics.DrawImage(Image,Point)">Graphics.DrawImage</see>,
        /// methods except that this one always preserves the source size in pixels, works between any pair of source and target <see cref="PixelFormat"/>s and supports quantizing and dithering.
        /// For scaling use the overloads with <c>targetRectangle</c> and <see cref="ScalingMode"/> parameters.
        /// To copy a bitmap data into another one without blending use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.CopyTo">CopyTo</see> methods instead.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="source">The source <see cref="IReadableBitmapData"/> to be drawn into the <paramref name="target"/>.</param>
        /// <param name="target">The target <see cref="IReadWriteBitmapData"/> into which <paramref name="source"/> should be drawn.</param>
        /// <param name="sourceRectangle">A <see cref="Rectangle"/> that specifies the portion of the <paramref name="source"/> to be drawn into the <paramref name="target"/>.</param>
        /// <param name="targetLocation">The target location. Target size will be always the same as the source size.</param>
        /// <param name="ditherer">The ditherer to be used for the drawing. Might be ignored if <paramref name="target"/> <see cref="IBitmapData.PixelFormat"/> format has at least 24 bits-per-pixel size.</param>
        /// <remarks>
        /// <para>The image to be drawn is automatically clipped if its size or <paramref name="targetLocation"/> makes it impossible to completely fit in the <paramref name="target"/>.</para>
        /// <para><paramref name="target"/> must be an <see cref="IReadWriteBitmapData"/> because it must be readable if blending is necessary. For write-only <see cref="IWritableBitmapData"/> instances
        /// you can use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.CopyTo">CopyTo</see> methods.</para>
        /// <para>If <paramref name="target"/> can represent a narrower set of colors, then the result will be automatically quantized to the colors of the <paramref name="target"/>.
        /// To use dithering a <paramref name="ditherer"/> must be explicitly specified.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="target"/> is <see langword="null"/>.</exception>
        public static void DrawInto(this IReadableBitmapData source, IReadWriteBitmapData target, Rectangle sourceRectangle, Point targetLocation, IDitherer ditherer)
            => DrawInto(source, target, sourceRectangle, targetLocation, null, ditherer);

        /// <summary>
        /// Draws the <paramref name="source"/>&#160;<see cref="IReadableBitmapData"/> into the <paramref name="target"/>&#160;<see cref="IReadWriteBitmapData"/>
        /// without scaling, using blending. This method is similar to <see cref="Graphics.DrawImage(Image,Point)">Graphics.DrawImage</see>,
        /// methods except that this one always preserves the source size in pixels, works between any pair of source and target <see cref="PixelFormat"/>s and supports quantizing and dithering.
        /// For scaling use the overloads with <c>targetRectangle</c> and <see cref="ScalingMode"/> parameters.
        /// To copy a bitmap data into another one without blending use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.CopyTo">CopyTo</see> methods instead.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="source">The source <see cref="IReadableBitmapData"/> to be drawn into the <paramref name="target"/>.</param>
        /// <param name="target">The target <see cref="IReadWriteBitmapData"/> into which <paramref name="source"/> should be drawn.</param>
        /// <param name="sourceRectangle">A <see cref="Rectangle"/> that specifies the portion of the <paramref name="source"/> to be drawn into the <paramref name="target"/>.</param>
        /// <param name="targetLocation">The target location. Target size will be always the same as the source size.</param>
        /// <param name="quantizer">An <see cref="IQuantizer"/> instance to be used for the drawing. If not specified, then the drawing operation might automatically
        /// pick a quantizer based on <paramref name="target"/> <see cref="IBitmapData.PixelFormat"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="ditherer">The ditherer to be used for the drawing. Might be ignored if <paramref name="quantizer"/> is not specified
        /// and <paramref name="target"/> <see cref="IBitmapData.PixelFormat"/> format has at least 24 bits-per-pixel size. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <remarks>
        /// <para>The image to be drawn is automatically clipped if its size or <paramref name="targetLocation"/> makes it impossible to completely fit in the <paramref name="target"/>.</para>
        /// <para><paramref name="target"/> must be an <see cref="IReadWriteBitmapData"/> because it must be readable if blending is necessary. For write-only <see cref="IWritableBitmapData"/> instances
        /// you can use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.CopyTo">CopyTo</see> methods.</para>
        /// <para>If <paramref name="target"/> can represent a narrower set of colors, then the result will be automatically quantized to the colors of the <paramref name="target"/>,
        /// even if there is no <paramref name="quantizer"/> specified. To use dithering a <paramref name="ditherer"/> must be explicitly specified though.</para>
        /// <para>If <paramref name="quantizer"/> is specified but it uses more/different colors than <paramref name="target"/> can represent,
        /// then the result will eventually quantized to <paramref name="target"/>, though the result may have a poorer quality than expected.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="target"/> is <see langword="null"/>.</exception>
        public static void DrawInto(this IReadableBitmapData source, IReadWriteBitmapData target, Rectangle sourceRectangle, Point targetLocation, IQuantizer quantizer = null, IDitherer ditherer = null)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source), PublicResources.ArgumentNull);
            if (target == null)
                throw new ArgumentNullException(nameof(target), PublicResources.ArgumentNull);

            if (source.HasAlpha())
                DoDrawWithoutResize(source, target, sourceRectangle, targetLocation, quantizer, ditherer);
            else
                DoCopy(source, target, sourceRectangle, targetLocation, quantizer, ditherer);
        }

        #endregion

        #region With resize

        /// <summary>
        /// Draws the <paramref name="source"/>&#160;<see cref="IReadableBitmapData"/> into the <paramref name="target"/>&#160;<see cref="IReadWriteBitmapData"/>
        /// using scaling and blending. This method is similar to <see cref="O:System.Drawing.Graphics.DrawImage">Graphics.DrawImage</see>
        /// methods except that this one works between any pair of source and target <see cref="PixelFormat"/>s and supports quantizing and dithering.
        /// To copy a bitmap data into another one without blending use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.CopyTo">CopyTo</see> methods instead.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="source">The source <see cref="IReadableBitmapData"/> to be drawn into the <paramref name="target"/>.</param>
        /// <param name="target">The target <see cref="IReadWriteBitmapData"/> into which <paramref name="source"/> should be drawn.</param>
        /// <param name="targetRectangle">A <see cref="Rectangle"/> that specifies the location and size of the drawn <paramref name="source"/>.</param>
        /// <param name="quantizer">An <see cref="IQuantizer"/> instance to be used for the drawing. If not specified, then the drawing operation might automatically
        /// pick a quantizer based on <paramref name="target"/> <see cref="IBitmapData.PixelFormat"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="ditherer">The ditherer to be used for the drawing. Might be ignored if <paramref name="quantizer"/> is not specified
        /// and <paramref name="target"/> <see cref="IBitmapData.PixelFormat"/> format has at least 24 bits-per-pixel size. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="scalingMode">Specifies the scaling mode if the bitmap data to be drawn needs to be resized. This parameter is optional.
        /// <br/>Default value: <see cref="ScalingMode.Auto"/>.</param>
        /// <remarks>
        /// <para>The method has the best performance if <paramref name="source"/> and <paramref name="targetRectangle"/> have the same size, or when <paramref name="scalingMode"/> is <see cref="ScalingMode.NoScaling"/>.</para>
        /// <para>The image to be drawn is automatically clipped if <paramref name="targetRectangle"/> exceeds bounds, or <paramref name="scalingMode"/> is <see cref="ScalingMode.NoScaling"/>
        /// and <paramref name="source"/> and <paramref name="targetRectangle"/> have different sizes.</para>
        /// <para><paramref name="target"/> must be an <see cref="IReadWriteBitmapData"/> because it must be readable if blending is necessary. For write-only <see cref="IWritableBitmapData"/> instances
        /// you can use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.CopyTo">CopyTo</see> methods (without scaling).</para>
        /// <para>If <paramref name="target"/> can represent a narrower set of colors, then the result will be automatically quantized to the colors of the <paramref name="target"/>,
        /// even if there is no <paramref name="quantizer"/> specified. To use dithering a <paramref name="ditherer"/> must be explicitly specified though.</para>
        /// <para>If <paramref name="quantizer"/> is specified but it uses more/different colors than <paramref name="target"/> can represent,
        /// then the result will eventually quantized to <paramref name="target"/>, though the result may have a poorer quality than expected.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="target"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="scalingMode"/> has an unsupported value.</exception>
        public static void DrawInto(this IReadableBitmapData source, IReadWriteBitmapData target, Rectangle targetRectangle, IQuantizer quantizer = null, IDitherer ditherer = null, ScalingMode scalingMode = ScalingMode.Auto)
            => DrawInto(source, target, new Rectangle(Point.Empty, source?.GetSize() ?? default), targetRectangle, quantizer, ditherer, scalingMode);

        /// <summary>
        /// Draws the <paramref name="source"/>&#160;<see cref="IReadableBitmapData"/> into the <paramref name="target"/>&#160;<see cref="IReadWriteBitmapData"/>
        /// using scaling and blending. This method is similar to <see cref="O:System.Drawing.Graphics.DrawImage">Graphics.DrawImage</see>
        /// methods except that this one works between any pair of source and target <see cref="PixelFormat"/>s and supports quantizing and dithering.
        /// To copy a bitmap data into another one without blending use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.CopyTo">CopyTo</see> methods instead.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="source">The source <see cref="IReadableBitmapData"/> to be drawn into the <paramref name="target"/>.</param>
        /// <param name="target">The target <see cref="IReadWriteBitmapData"/> into which <paramref name="source"/> should be drawn.</param>
        /// <param name="targetRectangle">A <see cref="Rectangle"/> that specifies the location and size of the drawn <paramref name="source"/>.</param>
        /// <param name="ditherer">The ditherer to be used for the drawing. Might be ignored if <paramref name="target"/> <see cref="IBitmapData.PixelFormat"/> format has at least 24 bits-per-pixel size.</param>
        /// <param name="scalingMode">Specifies the scaling mode if the bitmap data to be drawn needs to be resized. This parameter is optional.
        /// <br/>Default value: <see cref="ScalingMode.Auto"/>.</param>
        /// <remarks>
        /// <para>The method has the best performance if <paramref name="source"/> and <paramref name="targetRectangle"/> have the same size, or when <paramref name="scalingMode"/> is <see cref="ScalingMode.NoScaling"/>.</para>
        /// <para>The image to be drawn is automatically clipped if <paramref name="targetRectangle"/> exceeds bounds, or <paramref name="scalingMode"/> is <see cref="ScalingMode.NoScaling"/>
        /// and <paramref name="source"/> and <paramref name="targetRectangle"/> have different sizes.</para>
        /// <para><paramref name="target"/> must be an <see cref="IReadWriteBitmapData"/> because it must be readable if blending is necessary. For write-only <see cref="IWritableBitmapData"/> instances
        /// you can use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.CopyTo">CopyTo</see> methods (without scaling).</para>
        /// <para>If <paramref name="target"/> can represent a narrower set of colors, then the result will be automatically quantized to the colors of the <paramref name="target"/>.
        /// To use dithering a <paramref name="ditherer"/> must be explicitly specified.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="target"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="scalingMode"/> has an unsupported value.</exception>
        public static void DrawInto(this IReadableBitmapData source, IReadWriteBitmapData target, Rectangle targetRectangle, IDitherer ditherer, ScalingMode scalingMode = ScalingMode.Auto)
            => DrawInto(source, target, new Rectangle(Point.Empty, source?.GetSize() ?? default), targetRectangle, null, ditherer, scalingMode);

        /// <summary>
        /// Draws the <paramref name="source"/>&#160;<see cref="IReadableBitmapData"/> into the <paramref name="target"/>&#160;<see cref="IReadWriteBitmapData"/>
        /// using scaling and blending. This method is similar to <see cref="O:System.Drawing.Graphics.DrawImage">Graphics.DrawImage</see>
        /// methods except that this one works between any pair of source and target <see cref="PixelFormat"/>s and supports quantizing.
        /// To copy a bitmap data into another one without blending use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.CopyTo">CopyTo</see> methods instead.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="source">The source <see cref="IReadableBitmapData"/> to be drawn into the <paramref name="target"/>.</param>
        /// <param name="target">The target <see cref="IReadWriteBitmapData"/> into which <paramref name="source"/> should be drawn.</param>
        /// <param name="targetRectangle">A <see cref="Rectangle"/> that specifies the location and size of the drawn <paramref name="source"/>.</param>
        /// <param name="scalingMode">Specifies the scaling mode if the bitmap data to be drawn needs to be resized.</param>
        /// <remarks>
        /// <para>The method has the best performance if <paramref name="source"/> and <paramref name="targetRectangle"/> have the same size, or when <paramref name="scalingMode"/> is <see cref="ScalingMode.NoScaling"/>.</para>
        /// <para>The image to be drawn is automatically clipped if <paramref name="targetRectangle"/> exceeds bounds, or <paramref name="scalingMode"/> is <see cref="ScalingMode.NoScaling"/>
        /// and <paramref name="source"/> and <paramref name="targetRectangle"/> have different sizes.</para>
        /// <para><paramref name="target"/> must be an <see cref="IReadWriteBitmapData"/> because it must be readable if blending is necessary. For write-only <see cref="IWritableBitmapData"/> instances
        /// you can use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.CopyTo">CopyTo</see> methods (without scaling).</para>
        /// <para>If <paramref name="target"/> can represent a narrower set of colors, then the result will be automatically quantized to the colors of the <paramref name="target"/>.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="target"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="scalingMode"/> has an unsupported value.</exception>
        public static void DrawInto(this IReadableBitmapData source, IReadWriteBitmapData target, Rectangle targetRectangle, ScalingMode scalingMode)
            => DrawInto(source, target, new Rectangle(Point.Empty, source?.GetSize() ?? default), targetRectangle, null, null, scalingMode);

        /// <summary>
        /// Draws the <paramref name="source"/>&#160;<see cref="IReadableBitmapData"/> into the <paramref name="target"/>&#160;<see cref="IReadWriteBitmapData"/>
        /// using scaling and blending. This method is similar to <see cref="O:System.Drawing.Graphics.DrawImage">Graphics.DrawImage</see>
        /// methods except that this one works between any pair of source and target <see cref="PixelFormat"/>s and supports quantizing.
        /// To copy a bitmap data into another one without blending use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.CopyTo">CopyTo</see> methods instead.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="source">The source <see cref="IReadableBitmapData"/> to be drawn into the <paramref name="target"/>.</param>
        /// <param name="target">The target <see cref="IReadWriteBitmapData"/> into which <paramref name="source"/> should be drawn.</param>
        /// <param name="sourceRectangle">A <see cref="Rectangle"/> that specifies the portion of the <paramref name="source"/> to be drawn into the <paramref name="target"/>.</param>
        /// <param name="targetRectangle">A <see cref="Rectangle"/> that specifies the location and size of the drawn <paramref name="source"/>.</param>
        /// <param name="scalingMode">Specifies the scaling mode if the bitmap data to be drawn needs to be resized.</param>
        /// <remarks>
        /// <para>The method has the best performance if <paramref name="sourceRectangle"/> and <paramref name="targetRectangle"/> have the same size, or when <paramref name="scalingMode"/> is <see cref="ScalingMode.NoScaling"/>.</para>
        /// <para>The image to be drawn is automatically clipped if <paramref name="sourceRectangle"/> or <paramref name="targetRectangle"/> exceed bounds, or <paramref name="scalingMode"/> is <see cref="ScalingMode.NoScaling"/>
        /// and <paramref name="sourceRectangle"/> and <paramref name="targetRectangle"/> are different.</para>
        /// <para><paramref name="target"/> must be an <see cref="IReadWriteBitmapData"/> because it must be readable if blending is necessary. For write-only <see cref="IWritableBitmapData"/> instances
        /// you can use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.CopyTo">CopyTo</see> methods (without scaling).</para>
        /// <para>If <paramref name="target"/> can represent a narrower set of colors, then the result will be automatically quantized to the colors of the <paramref name="target"/>.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="target"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="scalingMode"/> has an unsupported value.</exception>
        public static void DrawInto(this IReadableBitmapData source, IReadWriteBitmapData target, Rectangle sourceRectangle, Rectangle targetRectangle, ScalingMode scalingMode)
            => DrawInto(source, target, sourceRectangle, targetRectangle, null, null, scalingMode);

        /// <summary>
        /// Draws the <paramref name="source"/>&#160;<see cref="IReadableBitmapData"/> into the <paramref name="target"/>&#160;<see cref="IReadWriteBitmapData"/>
        /// using scaling and blending. This method is similar to <see cref="O:System.Drawing.Graphics.DrawImage">Graphics.DrawImage</see>
        /// methods except that this one works between any pair of source and target <see cref="PixelFormat"/>s and supports quantizing and dithering.
        /// To copy a bitmap data into another one without blending use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.CopyTo">CopyTo</see> methods instead.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="source">The source <see cref="IReadableBitmapData"/> to be drawn into the <paramref name="target"/>.</param>
        /// <param name="target">The target <see cref="IReadWriteBitmapData"/> into which <paramref name="source"/> should be drawn.</param>
        /// <param name="sourceRectangle">A <see cref="Rectangle"/> that specifies the portion of the <paramref name="source"/> to be drawn into the <paramref name="target"/>.</param>
        /// <param name="targetRectangle">A <see cref="Rectangle"/> that specifies the location and size of the drawn <paramref name="source"/>.</param>
        /// <param name="ditherer">The ditherer to be used for the drawing. Might be ignored if <paramref name="target"/> <see cref="IBitmapData.PixelFormat"/>
        /// format has at least 24 bits-per-pixel size.</param>
        /// <param name="scalingMode">Specifies the scaling mode if the bitmap data to be drawn needs to be resized. This parameter is optional.
        /// <br/>Default value: <see cref="ScalingMode.Auto"/>.</param>
        /// <remarks>
        /// <para>The method has the best performance if <paramref name="sourceRectangle"/> and <paramref name="targetRectangle"/> have the same size, or when <paramref name="scalingMode"/> is <see cref="ScalingMode.NoScaling"/>.</para>
        /// <para>The image to be drawn is automatically clipped if <paramref name="sourceRectangle"/> or <paramref name="targetRectangle"/> exceed bounds, or <paramref name="scalingMode"/> is <see cref="ScalingMode.NoScaling"/>
        /// and <paramref name="sourceRectangle"/> and <paramref name="targetRectangle"/> are different.</para>
        /// <para><paramref name="target"/> must be an <see cref="IReadWriteBitmapData"/> because it must be readable if blending is necessary. For write-only <see cref="IWritableBitmapData"/> instances
        /// you can use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.CopyTo">CopyTo</see> methods (without scaling).</para>
        /// <para>If <paramref name="target"/> can represent a narrower set of colors, then the result will be automatically quantized to the colors of the <paramref name="target"/>.
        /// To use dithering a <paramref name="ditherer"/> must be explicitly specified.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="target"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="scalingMode"/> has an unsupported value.</exception>
        public static void DrawInto(this IReadableBitmapData source, IReadWriteBitmapData target, Rectangle sourceRectangle, Rectangle targetRectangle, IDitherer ditherer, ScalingMode scalingMode = ScalingMode.Auto)
            => DrawInto(source, target, sourceRectangle, targetRectangle, null, ditherer, scalingMode);

        /// <summary>
        /// Draws the <paramref name="source"/>&#160;<see cref="IReadableBitmapData"/> into the <paramref name="target"/>&#160;<see cref="IReadWriteBitmapData"/>
        /// using scaling and blending. This method is similar to <see cref="O:System.Drawing.Graphics.DrawImage">Graphics.DrawImage</see>
        /// methods except that this one works between any pair of source and target <see cref="PixelFormat"/>s and supports quantizing and dithering.
        /// To copy a bitmap data into another one without blending use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.CopyTo">CopyTo</see> methods instead.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="source">The source <see cref="IReadableBitmapData"/> to be drawn into the <paramref name="target"/>.</param>
        /// <param name="target">The target <see cref="IReadWriteBitmapData"/> into which <paramref name="source"/> should be drawn.</param>
        /// <param name="sourceRectangle">A <see cref="Rectangle"/> that specifies the portion of the <paramref name="source"/> to be drawn into the <paramref name="target"/>.</param>
        /// <param name="targetRectangle">A <see cref="Rectangle"/> that specifies the location and size of the drawn <paramref name="source"/>.</param>
        /// <param name="quantizer">An <see cref="IQuantizer"/> instance to be used for the drawing. If not specified, then the drawing operation might automatically
        /// pick a quantizer based on <paramref name="target"/> <see cref="IBitmapData.PixelFormat"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="ditherer">The ditherer to be used for the drawing. Might be ignored if <paramref name="quantizer"/> is not specified
        /// and <paramref name="target"/> <see cref="IBitmapData.PixelFormat"/> format has at least 24 bits-per-pixel size. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="scalingMode">Specifies the scaling mode if the bitmap data to be drawn needs to be resized. This parameter is optional.
        /// <br/>Default value: <see cref="ScalingMode.Auto"/>.</param>
        /// <remarks>
        /// <para>The method has the best performance if <paramref name="sourceRectangle"/> and <paramref name="targetRectangle"/> have the same size, or when <paramref name="scalingMode"/> is <see cref="ScalingMode.NoScaling"/>.</para>
        /// <para>The image to be drawn is automatically clipped if <paramref name="sourceRectangle"/> or <paramref name="targetRectangle"/> exceed bounds, or <paramref name="scalingMode"/> is <see cref="ScalingMode.NoScaling"/>
        /// and <paramref name="sourceRectangle"/> and <paramref name="targetRectangle"/> are different.</para>
        /// <para><paramref name="target"/> must be an <see cref="IReadWriteBitmapData"/> because it must be readable if blending is necessary. For write-only <see cref="IWritableBitmapData"/> instances
        /// you can use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.CopyTo">CopyTo</see> methods (without scaling).</para>
        /// <para>If <paramref name="target"/> can represent a narrower set of colors, then the result will be automatically quantized to the colors of the <paramref name="target"/>,
        /// even if there is no <paramref name="quantizer"/> specified. To use dithering a <paramref name="ditherer"/> must be explicitly specified though.</para>
        /// <para>If <paramref name="quantizer"/> is specified but it uses more/different colors than <paramref name="target"/> can represent,
        /// then the result will eventually quantized to <paramref name="target"/>, though the result may have a poorer quality than expected.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="target"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="scalingMode"/> has an unsupported value.</exception>
        public static void DrawInto(this IReadableBitmapData source, IReadWriteBitmapData target, Rectangle sourceRectangle, Rectangle targetRectangle, IQuantizer quantizer = null, IDitherer ditherer = null, ScalingMode scalingMode = ScalingMode.Auto)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source), PublicResources.ArgumentNull);
            if (target == null)
                throw new ArgumentNullException(nameof(target), PublicResources.ArgumentNull);
            if (!scalingMode.IsDefined())
                throw new ArgumentOutOfRangeException(nameof(scalingMode), PublicResources.EnumOutOfRange(scalingMode));

            // no scaling is necessary
            if (sourceRectangle.Size == targetRectangle.Size || scalingMode == ScalingMode.NoScaling)
            {
                if (source.HasAlpha())
                    DoDrawWithoutResize(source, target, sourceRectangle, targetRectangle.Location, quantizer, ditherer);
                else
                    DoCopy(source, target, sourceRectangle, targetRectangle.Location, quantizer, ditherer);
                return;
            }

            DoDrawWithResize(source, target, sourceRectangle, targetRectangle, quantizer, ditherer, scalingMode);
        }

        #endregion

        #endregion

        #region Clip

        /// <summary>
        /// Clips the specified <paramref name="source"/> using the specified <paramref name="clippingRegion"/>.
        /// Unlike the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.Clone">Clone</see> methods, this one returns a wrapper,
        /// providing access only to the specified region of the original <paramref name="source"/>.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="source">The source bitmap data to be clipped.</param>
        /// <param name="clippingRegion">A <see cref="Rectangle"/> that specifies a region within the <paramref name="source"/>.</param>
        /// <returns>An <see cref="IReadableBitmapData"/> that provides access only to the specified region withing the <paramref name="source"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="clippingRegion"/> has no overlapping region with source bounds.</exception>
        /// <remarks>
        /// <para>The <see cref="IBitmapData.RowSize"/> property of the returned instance can be 0, indicating that the <see cref="IReadableBitmapDataRow.ReadRaw{T}">ReadRaw</see>
        /// method cannot be used. It can occur with indexed <see cref="IBitmapData.PixelFormat"/>s if the left edge of the clipping is not on byte boundary.</para>
        /// <para>Even if <see cref="IBitmapData.RowSize"/> property of the returned instance is a nonzero value it can happen that it is too low to access all columns
        /// by the <see cref="IReadableBitmapDataRow.ReadRaw{T}">ReadRaw</see> method. It can occur with indexed <see cref="IBitmapData.PixelFormat"/>s if the right edge of the clipping is not on byte boundary.</para>
        /// </remarks>
        public static IReadableBitmapData Clip(this IReadableBitmapData source, Rectangle clippingRegion)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source), PublicResources.ArgumentNull);
            return clippingRegion.Location.IsEmpty && clippingRegion.Size == source.GetSize()
                ? source
                : new ClippedBitmapData(source, clippingRegion);
        }

        #endregion

        /// <summary>
        /// Converts the specified <paramref name="source"/> to a <see cref="Bitmap"/>.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="source">The source <see cref="IReadWriteBitmapData"/> instance to covert.</param>
        /// <returns>A <see cref="Bitmap"/> instance that has the same content as the specified <paramref name="source"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// <para>If supported on the current platform, the result <see cref="Bitmap"/> will have the same <see cref="PixelFormat"/> as <paramref name="source"/>.
        /// Otherwise, the result will have either <see cref="PixelFormat.Format24bppRgb"/> or <see cref="PixelFormat.Format32bppArgb"/> format, depending whether source has transparency.
        /// <note>On Windows every format is supported with more or less limitations. For details and further information about the possible usable <see cref="PixelFormat"/>s on different platforms
        /// see the <strong>Remarks</strong> section of the <see cref="ImageExtensions.ConvertPixelFormat(Image,PixelFormat,Color,byte)">ConvertPixelFormat</see> method.
        /// </note></para>
        /// </remarks>
        public static Bitmap ToBitmap(this IReadableBitmapData source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source), PublicResources.ArgumentNull);

            PixelFormat pixelFormat = source.PixelFormat.IsSupportedNatively() ? source.PixelFormat
                : source.HasAlpha() ? PixelFormat.Format32bppArgb
                : PixelFormat.Format24bppRgb;

            var result = new Bitmap(source.Width, source.Height, pixelFormat);
            if (pixelFormat.IsIndexed() && source.Palette != null)
                result.SetPalette(source.Palette);

            using (IBitmapDataInternal target = BitmapDataFactory.CreateBitmapData(result, ImageLockMode.WriteOnly, source.BackColor, source.AlphaThreshold, source.Palette))
                source.CopyTo(target);

            return result;
        }

        #endregion

        #region IWritableBitmapData

        /// <summary>
        /// Clips the specified <paramref name="source"/> using the specified <paramref name="clippingRegion"/>.
        /// Unlike the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.Clone">Clone</see> methods, this one returns a wrapper,
        /// providing access only to the specified region of the original <paramref name="source"/>.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="source">The source bitmap data to be clipped.</param>
        /// <param name="clippingRegion">A <see cref="Rectangle"/> that specifies a region within the <paramref name="source"/>.</param>
        /// <returns>An <see cref="IWritableBitmapData"/> that provides access only to the specified region withing the <paramref name="source"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="clippingRegion"/> has no overlapping region with source bounds.</exception>
        /// <remarks>
        /// <para>The <see cref="IBitmapData.RowSize"/> property of the returned instance can be 0, indicating that the <see cref="IWritableBitmapDataRow.WriteRaw{T}">WriteRaw</see>
        /// method cannot be used. It can occur with indexed <see cref="IBitmapData.PixelFormat"/>s if the left edge of the clipping is not on byte boundary.</para>
        /// <para>Even if <see cref="IBitmapData.RowSize"/> property of the returned instance is a nonzero value it can happen that it is too low to access all columns
        /// by the <see cref="IWritableBitmapDataRow.WriteRaw{T}">WriteRaw</see> method. It can occur with indexed <see cref="IBitmapData.PixelFormat"/>s if the right edge of the clipping is not on byte boundary.</para>
        /// </remarks>
        public static IWritableBitmapData Clip(this IWritableBitmapData source, Rectangle clippingRegion)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source), PublicResources.ArgumentNull);
            return clippingRegion.Location.IsEmpty && clippingRegion.Size == source.GetSize()
                ? source
                : new ClippedBitmapData(source, clippingRegion);
        }

        /// <summary>
        /// Clears the content of the specified <paramref name="bitmapData"/> and fills it with the specified <paramref name="color"/>.
        /// <br/>This method is similar to <see cref="Graphics.Clear">Graphics.Clear</see> except that this one supports any <see cref="PixelFormat"/> and also dithering.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IWritableBitmapData"/> to be cleared.</param>
        /// <param name="color">A <see cref="Color32"/> that represents the desired result color of the <paramref name="bitmapData"/>.
        /// If it has transparency, which is not supported by <see cref="IBitmapData.PixelFormat"/> of <paramref name="bitmapData"/>, then the result might be either
        /// completely transparent (depends also on <see cref="IBitmapData.AlphaThreshold"/>), or the color will be blended with <see cref="IBitmapData.BackColor"/>.
        /// </param>
        /// <param name="ditherer">The ditherer to be used for the clearing. Has no effect if <see cref="IBitmapData.PixelFormat"/> of <paramref name="bitmapData"/> has at least 24 bits-per-pixel size. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        public static void Clear(this IWritableBitmapData bitmapData, Color32 color, IDitherer ditherer = null)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);

            IBitmapDataInternal accessor = bitmapData as IBitmapDataInternal ?? new BitmapDataWrapper(bitmapData, false, true);
            try
            {
                if (ditherer == null || !accessor.PixelFormat.CanBeDithered())
                    ClearDirect(accessor, color);
                else
                    ClearWithDithering(accessor, color, ditherer);
            }
            finally
            {
                if (!ReferenceEquals(accessor, bitmapData))
                    accessor.Dispose();
            }
        }

        #endregion

        #region IReadWriteBitmapData

        /// <summary>
        /// Clips the specified <paramref name="source"/> using the specified <paramref name="clippingRegion"/>.
        /// Unlike the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.Clone">Clone</see> methods, this one returns a wrapper,
        /// providing access only to the specified region of the original <paramref name="source"/>.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="source">The source bitmap data to be clipped.</param>
        /// <param name="clippingRegion">A <see cref="Rectangle"/> that specifies a region within the <paramref name="source"/>.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> that provides access only to the specified region withing the <paramref name="source"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="clippingRegion"/> has no overlapping region with source bounds.</exception>
        /// <remarks>
        /// <para>The <see cref="IBitmapData.RowSize"/> property of the returned instance can be 0, indicating that the <see cref="IReadableBitmapDataRow.ReadRaw{T}">ReadRaw</see>/<see cref="IWritableBitmapDataRow.WriteRaw{T}">WriteRaw</see>
        /// methods cannot be used. It can occur with indexed <see cref="IBitmapData.PixelFormat"/>s if the left edge of the clipping is not on byte boundary.</para>
        /// <para>Even if <see cref="IBitmapData.RowSize"/> property of the returned instance is a nonzero value it can happen that it is too low to access all columns
        /// by the <see cref="IReadableBitmapDataRow.ReadRaw{T}">ReadRaw</see>/<see cref="IWritableBitmapDataRow.WriteRaw{T}">WriteRaw</see> methods. It can occur with indexed <see cref="IBitmapData.PixelFormat"/>s if the right edge of the clipping is not on byte boundary.</para>
        /// </remarks>
        public static IReadWriteBitmapData Clip(this IReadWriteBitmapData source, Rectangle clippingRegion)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source), PublicResources.ArgumentNull);
            return clippingRegion.Location.IsEmpty && clippingRegion.Size == source.GetSize()
                ? source
                : new ClippedBitmapData(source, clippingRegion);
        }

        #endregion

        #endregion

        #region Internal Methods

        internal static Size GetSize(this IBitmapData bitmapData) => bitmapData == null ? default : new Size(bitmapData.Width, bitmapData.Height);

        internal static bool HasAlpha(this IBitmapData bitmapData)
        {
            PixelFormat pixelFormat = bitmapData.PixelFormat;
            return pixelFormat.HasAlpha() || pixelFormat.IsIndexed() && bitmapData.Palette?.HasAlpha == true;
        }

        #endregion

        #region Private Methods

        private static void Unwrap<TBitmapData>(ref TBitmapData source, ref Rectangle newRectangle)
            where TBitmapData : IBitmapData
        {
            while (true)
            {
                switch (source)
                {
                    case ClippedBitmapData clipped:
                        source = (TBitmapData)clipped.BitmapData;
                        Rectangle region = clipped.Region;
                        newRectangle.Offset(region.Location);
                        newRectangle.Intersect(region);
                        continue;
                    case BitmapDataWrapper wrapper:
                        Debug.Fail("Wrapper has been leaked out, check call stack");
                        source = (TBitmapData)wrapper.BitmapData;
                        continue;
                    default:
                        return;
                }
            }
        }

        [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "False alarm, initSource is disposed if needed")]
        private static void DoCopy(this IReadableBitmapData source, IWritableBitmapData target, Rectangle sourceRectangle, Point targetLocation, IQuantizer quantizer, IDitherer ditherer, bool skipTransparent = false)
        {
            var session = new CopySession();
            var sourceBounds = new Rectangle(default, source.GetSize());
            var targetBounds = new Rectangle(default, target.GetSize());
            Unwrap(ref source, ref sourceBounds);
            Unwrap(ref target, ref targetBounds);

            (session.SourceRectangle, session.TargetRectangle) = GetActualRectangles(sourceBounds, sourceRectangle, targetBounds, targetLocation);
            if (session.SourceRectangle.IsEmpty || session.TargetRectangle.IsEmpty)
                return;

            AdjustQuantizerAndDitherer(target, ref quantizer, ref ditherer);

            // special handling for same references
            if (ReferenceEquals(source, target))
            {
                // same area without quantizing: nothing to do
                if (quantizer == null && session.SourceRectangle == session.TargetRectangle)
                    return;

                // overlap: clone source
                if (session.SourceRectangle.IntersectsWith(session.TargetRectangle))
                {
                    session.Source = (IBitmapDataInternal)Clone(source, session.SourceRectangle, source.PixelFormat);
                    session.SourceRectangle.Location = Point.Empty;
                }
            }

            if (session.Source == null)
                session.Source = source as IBitmapDataInternal ?? new BitmapDataWrapper(source, true, false);
            session.Target = target as IBitmapDataInternal ?? new BitmapDataWrapper(target, false, true);

            try
            {
                // processing without using a quantizer
                if (quantizer == null)
                {
                    Debug.Assert(!skipTransparent, "Skipping transparent source pixels is not expected without quantizing. Handle it if really needed.");
                    session.PerformCopy();
                    return;
                }

                // Using a clipped source for quantizer/ditherer if needed. Note: the CopySession uses the original source for the best performance
                IReadableBitmapData initSource = session.SourceRectangle.Size == source.GetSize()
                    ? source
                    : source.Clip(session.SourceRectangle);

                try
                {
                    using (IQuantizingSession quantizingSession = quantizer.Initialize(initSource) ?? throw new InvalidOperationException(Res.ImagingQuantizerInitializeNull))
                    {
                        // quantization without dithering
                        if (ditherer == null)
                        {
                            session.PerformCopyWithQuantizer(quantizingSession, skipTransparent);
                            return;
                        }

                        // quantization with dithering
                        using (IDitheringSession ditheringSession = ditherer.Initialize(initSource, quantizingSession) ?? throw new InvalidOperationException(Res.ImagingDithererInitializeNull))
                            session.PerformCopyWithDithering(quantizingSession, ditheringSession, skipTransparent);
                    }
                }
                finally
                {
                    if (!ReferenceEquals(initSource, source))
                        initSource.Dispose();
                }
            }
            finally
            {
                if (!ReferenceEquals(session.Source, source))
                    session.Source.Dispose();
                if (!ReferenceEquals(session.Target, target))
                    session.Target.Dispose();
            }
        }

        [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "False alarm, initSource is disposed if needed")]
        private static void DoDrawWithoutResize(this IReadableBitmapData source, IReadWriteBitmapData target, Rectangle sourceRectangle, Point targetLocation, IQuantizer quantizer, IDitherer ditherer)
        {
            Debug.Assert(source.HasAlpha(), "DoCopy could have been called");

            var sourceBounds = new Rectangle(default, source.GetSize());
            var targetBounds = new Rectangle(default, target.GetSize());
            Unwrap(ref source, ref sourceBounds);
            Unwrap(ref target, ref targetBounds);

            (Rectangle actualSourceRectangle, Rectangle actualTargetRectangle) = GetActualRectangles(sourceBounds, sourceRectangle, targetBounds, targetLocation);
            if (actualSourceRectangle.IsEmpty || actualTargetRectangle.IsEmpty)
                return;

            AdjustQuantizerAndDitherer(target, ref quantizer, ref ditherer);

            IBitmapDataInternal sessionTarget;
            Rectangle sessionTargetRectangle = actualTargetRectangle;
            bool targetCloned = false;
            bool isTwoPass = source.HasMultiLevelAlpha() && (quantizer?.InitializeReliesOnContent == true || ditherer?.InitializeReliesOnContent == true);

            // if two pass is needed we create a temp result where we perform blending before quantizing/dithering
            if (isTwoPass)
            {
                sessionTarget = (IBitmapDataInternal)target.Clone(actualTargetRectangle, PixelFormat.Format32bppArgb);
                sessionTargetRectangle.Location = Point.Empty;
                targetCloned = true;
            }
            else
                sessionTarget = target as IBitmapDataInternal ?? new BitmapDataWrapper(target, false, true);

            IBitmapDataInternal sessionSource = null;

            // special handling for same references
            if (ReferenceEquals(source, target) && !targetCloned)
            {
                // same area without quantizing: nothing to do
                if (quantizer == null && actualSourceRectangle == actualTargetRectangle)
                    return;

                // overlap: clone source
                if (actualSourceRectangle.IntersectsWith(actualTargetRectangle))
                {
                    sessionSource = (IBitmapDataInternal)Clone(source, actualSourceRectangle, source.PixelFormat);
                    actualSourceRectangle.Location = Point.Empty;
                }
            }

            if (sessionSource == null)
                sessionSource = source as IBitmapDataInternal ?? new BitmapDataWrapper(source, true, false);

            try
            {
                var session = new CopySession(sessionSource, sessionTarget, actualSourceRectangle, sessionTargetRectangle);
                if (!isTwoPass)
                {
                    session.PerformDraw(quantizer, ditherer);
                    return;
                }

                // first pass: performing blending into transient result
                session.PerformDrawDirect();

                // second pass: copying the blended transient result to the actual target
                DoCopy(sessionTarget, target, sessionTargetRectangle, actualTargetRectangle.Location, quantizer, ditherer, true);
            }
            finally
            {
                if (!ReferenceEquals(sessionSource, source))
                    sessionSource.Dispose();
                if (!ReferenceEquals(sessionTarget, target))
                    sessionTarget.Dispose();
            }
        }

        [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "False alarm, sessionTarget is disposed if needed")]
        private static void DoDrawWithResize(this IReadableBitmapData source, IReadWriteBitmapData target, Rectangle sourceRectangle, Rectangle targetRectangle, IQuantizer quantizer, IDitherer ditherer, ScalingMode scalingMode)
        {
            Debug.Assert(sourceRectangle.Size != targetRectangle.Size || scalingMode == ScalingMode.NoScaling, $"{nameof(DoDrawWithoutResize)} could have been called");

            var sourceBounds = new Rectangle(default, source.GetSize());
            var targetBounds = new Rectangle(default, target.GetSize());
            Unwrap(ref source, ref sourceBounds);
            Unwrap(ref target, ref targetBounds);

            (Rectangle actualSourceRectangle, Rectangle actualTargetRectangle) = GetActualRectangles(sourceBounds, sourceRectangle, targetBounds, targetRectangle);
            if (actualSourceRectangle.IsEmpty || actualTargetRectangle.IsEmpty)
                return;

            AdjustQuantizerAndDitherer(target, ref quantizer, ref ditherer);

            IBitmapDataInternal sessionTarget;
            Rectangle sessionTargetRectangle = actualTargetRectangle;
            bool targetCloned = false;

            // note: when resizing, we cannot trick the quantizer/ditherer with a single-bit alpha source because the source is needed to be resized
            bool isTwoPass = quantizer?.InitializeReliesOnContent == true || ditherer?.InitializeReliesOnContent == true;

            // if two pass is needed we create a temp result where we perform resize (with or without blending) before quantizing/dithering
            if (isTwoPass)
            {
                sessionTarget = source.HasMultiLevelAlpha()
                    ? (IBitmapDataInternal)target.Clone(actualTargetRectangle, PixelFormat.Format32bppArgb)
                    : (IBitmapDataInternal)BitmapDataFactory.CreateBitmapData(sessionTargetRectangle.Size);
                sessionTargetRectangle.Location = Point.Empty;
                targetCloned = true;
            }
            else
                sessionTarget = target as IBitmapDataInternal ?? new BitmapDataWrapper(target, false, true);

            IBitmapDataInternal sessionSource = null;

            // special handling for same references
            if (ReferenceEquals(source, target) && !targetCloned)
            {
                // same area without quantizing: nothing to do
                if (quantizer == null && actualSourceRectangle == actualTargetRectangle)
                    return;

                // overlap: clone source
                if (actualSourceRectangle.IntersectsWith(actualTargetRectangle))
                {
                    sessionSource = (IBitmapDataInternal)Clone(source, actualSourceRectangle, source.PixelFormat);
                    actualSourceRectangle.Location = Point.Empty;
                }
            }

            if (sessionSource == null)
                sessionSource = source as IBitmapDataInternal ?? new BitmapDataWrapper(source, true, false);

            try
            {
                if (scalingMode == ScalingMode.NearestNeighbor)
                {
                    var session = new ResizingSessionNearestNeighbor(sessionSource, sessionTarget, actualSourceRectangle, sessionTargetRectangle);
                    if (!isTwoPass)
                    {
                        session.PerformResize(quantizer, ditherer);
                        return;
                    }

                    // first pass: performing resizing into a transient result
                    session.PerformResizeDirect();
                }
                else
                {
                    using var session = new ResizingSessionInterpolated(sessionSource, sessionTarget, actualSourceRectangle, sessionTargetRectangle, scalingMode);
                    if (!isTwoPass)
                    {
                        session.PerformResize(quantizer, ditherer);
                        return;
                    }

                    // first pass: performing blending into transient result
                    session.PerformResizeDirect();
                }

                // second pass: copying the possibly blended transient result to the actual target with quantizing/dithering
                DoCopy(sessionTarget, target, sessionTargetRectangle, actualTargetRectangle.Location, quantizer, ditherer, true);
            }
            finally
            {
                if (!ReferenceEquals(sessionSource, source))
                    sessionSource.Dispose();
                if (!ReferenceEquals(sessionTarget, target))
                    sessionTarget.Dispose();
            }
        }

        private static (Rectangle Source, Rectangle Target) GetActualRectangles(Rectangle sourceBounds, Rectangle sourceRectangle, Rectangle targetBounds, Point targetLocation)
        {
            sourceRectangle.Offset(sourceBounds.Location);
            Rectangle actualSourceRectangle = Rectangle.Intersect(sourceRectangle, sourceBounds);
            if (actualSourceRectangle.IsEmpty)
                return default;
            targetLocation.Offset(targetBounds.Location);
            Rectangle targetRectangle = new Rectangle(targetLocation, sourceRectangle.Size);
            Rectangle actualTargetRectangle = Rectangle.Intersect(targetRectangle, targetBounds);
            if (actualTargetRectangle.IsEmpty)
                return default;

            // adjusting source by clipped target
            if (targetRectangle != actualTargetRectangle)
            {
                int x = actualTargetRectangle.X - targetRectangle.X + sourceRectangle.X;
                int y = actualTargetRectangle.Y - targetRectangle.Y + sourceRectangle.Y;
                actualSourceRectangle.Intersect(new Rectangle(x, y, actualTargetRectangle.Width, actualTargetRectangle.Height));
            }

            // adjusting target by clipped source
            if (sourceRectangle != actualSourceRectangle)
            {
                int x = actualSourceRectangle.X - sourceRectangle.X + targetRectangle.X;
                int y = actualSourceRectangle.Y - sourceRectangle.Y + targetRectangle.Y;
                actualTargetRectangle.Intersect(new Rectangle(x, y, actualSourceRectangle.Width, actualSourceRectangle.Height));
            }

            return (actualSourceRectangle, actualTargetRectangle);
        }

        private static (Rectangle Source, Rectangle Target) GetActualRectangles(Rectangle sourceBounds, Rectangle sourceRectangle, Rectangle targetBounds, Rectangle targetRectangle)
        {
            sourceRectangle.Offset(sourceBounds.Location);
            Rectangle actualSourceRectangle = Rectangle.Intersect(sourceRectangle, sourceBounds);
            if (actualSourceRectangle.IsEmpty)
                return default;
            targetRectangle.Offset(targetBounds.Location);
            Rectangle actualTargetRectangle = Rectangle.Intersect(targetRectangle, targetBounds);
            if (actualTargetRectangle.IsEmpty)
                return default;

            float widthRatio = (float)sourceRectangle.Width / targetRectangle.Width;
            float heightRatio = (float)sourceRectangle.Height / targetRectangle.Height;

            // adjusting source by clipped target
            if (targetRectangle != actualTargetRectangle)
            {
                int x = (int)MathF.Round((actualTargetRectangle.X - targetRectangle.X) * widthRatio + sourceRectangle.X);
                int y = (int)MathF.Round((actualTargetRectangle.Y - targetRectangle.Y) * heightRatio + sourceRectangle.Y);
                int w = (int)MathF.Round(actualTargetRectangle.Width * widthRatio);
                int h = (int)MathF.Round(actualTargetRectangle.Height * heightRatio);
                actualSourceRectangle.Intersect(new Rectangle(x, y, w, h));
            }

            // adjusting target by clipped source
            if (sourceRectangle != actualSourceRectangle)
            {
                int x = (int)MathF.Round((actualSourceRectangle.X - sourceRectangle.X) / widthRatio + targetRectangle.X);
                int y = (int)MathF.Round((actualSourceRectangle.Y - sourceRectangle.Y) / heightRatio + targetRectangle.Y);
                int w = (int)MathF.Round(actualSourceRectangle.Width / widthRatio);
                int h = (int)MathF.Round(actualSourceRectangle.Height / heightRatio);
                actualTargetRectangle.Intersect(new Rectangle(x, y, w, h));
            }

            return (actualSourceRectangle, actualTargetRectangle);
        }

        private static void AdjustQuantizerAndDitherer(IBitmapData target, ref IQuantizer quantizer, ref IDitherer ditherer)
        {
            if (quantizer != null || ditherer == null)
                return;

            if (target.PixelFormat.CanBeDithered())
                quantizer = PredefinedColorsQuantizer.FromBitmapData(target);
            else
                ditherer = null;
        }

        private static bool HasMultiLevelAlpha(this IBitmapData bitmapData)
        {
            PixelFormat pixelFormat = bitmapData.PixelFormat;
            return pixelFormat.HasMultiLevelAlpha() || pixelFormat.IsIndexed() && bitmapData.Palette?.HasMultiLevelAlpha == true;
        }

        private static bool Is32BppPremultiplied(this IBitmapData bitmapData)
            => bitmapData.PixelFormat == PixelFormat.Format32bppPArgb && bitmapData.RowSize == bitmapData.Width << 2;

        #endregion

        #endregion
    }
}
