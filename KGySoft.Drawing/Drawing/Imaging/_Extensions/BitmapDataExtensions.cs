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

        /// <summary>
        /// Gets a readable and writable clone of the specified <see cref="IReadableBitmapData"/> instance with identical size and pixel format.
        /// </summary>
        /// <param name="source">An <see cref="IReadWriteBitmapData"/> instance to be cloned.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance that represents the clone of the specified <paramref name="source"/>.</returns>
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
            session.PerformDraw(false);

            return session.Target;
        }

        // TODO Docs:
        // - Supports all pixel formats on every platform
        // - Wide colors are preserved only between the same pixel format but if source is from a Bitmap on Windows, which uses 13bpp channels, then colors might be quantized to 32bpp
        // - If palette is not specified but pixel format is an indexed image, then the source palette is used if possible; otherwise, a system palette will be used
        public static IReadWriteBitmapData Clone(this IReadableBitmapData source, PixelFormat pixelFormat, Color32 backColor = default, byte alphaThreshold = 128, Palette palette = null)
            => Clone(source, new Rectangle(Point.Empty, source?.GetSize() ?? default), pixelFormat, backColor, alphaThreshold, palette);

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
            session.PerformDraw(false);

            return session.Target;
        }

        public static IReadWriteBitmapData Clone(this IReadableBitmapData source, PixelFormat pixelFormat, IQuantizer quantizer, IDitherer ditherer = null)
            => Clone(source, new Rectangle(Point.Empty, source?.GetSize() ?? default), pixelFormat, quantizer, ditherer);

        public static IReadWriteBitmapData Clone(this IReadableBitmapData source, PixelFormat pixelFormat, IDitherer ditherer)
            => Clone(source, new Rectangle(Point.Empty, source?.GetSize() ?? default), pixelFormat, null, ditherer);

        public static IReadWriteBitmapData Clone(this IReadableBitmapData source, Rectangle sourceRectangle, PixelFormat pixelFormat, IDitherer ditherer)
            => Clone(source, sourceRectangle, pixelFormat, null, ditherer);

        // TODO Docs:
        // - Using incompatible quantizer and pixelFormat may throw an ArgumentException
        public static IReadWriteBitmapData Clone(this IReadableBitmapData source, Rectangle sourceRectangle, PixelFormat pixelFormat, IQuantizer quantizer, IDitherer ditherer = null)
        {
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

            if (source == null)
                throw new ArgumentNullException(nameof(source), PublicResources.ArgumentNull);

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
                        session.PerformDrawWithQuantizer(quantizingSession, false);
                    else
                    {
                        // quantizing with dithering
                        using IDitheringSession ditheringSession = ditherer.Initialize(initSource, quantizingSession) ?? throw new InvalidOperationException(Res.ImagingDithererInitializeNull);
                        session.PerformDrawWithDithering(quantizingSession, ditheringSession, false);
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

        public static void CopyTo(this IReadableBitmapData source, IWritableBitmapData target, Point targetLocation = default, IQuantizer quantizer = null, IDitherer ditherer = null)
            => DoDrawWithoutResize(source, target, new Rectangle(Point.Empty, source?.GetSize() ?? default), targetLocation, quantizer, ditherer, false);

        public static void CopyTo(this IReadableBitmapData source, IWritableBitmapData target, Point targetLocation, IDitherer ditherer)
            => DoDrawWithoutResize(source, target, new Rectangle(Point.Empty, source?.GetSize() ?? default), targetLocation, null, ditherer, false);

        public static void CopyTo(this IReadableBitmapData source, IWritableBitmapData target, Rectangle sourceRectangle, Point targetLocation, IDitherer ditherer)
            => DoDrawWithoutResize(source, target, sourceRectangle, targetLocation, null, ditherer, false);

        // TODO Docs:
        // - If the quantizer uses more colors than the target can use the result may have a poorer quality than expected
        public static void CopyTo(this IReadableBitmapData source, IWritableBitmapData target, Rectangle sourceRectangle, Point targetLocation = default, IQuantizer quantizer = null, IDitherer ditherer = null)
            => DoDrawWithoutResize(source, target, sourceRectangle, targetLocation, quantizer, ditherer, false);

        #region DrawInto

        #region Without resize

        /// <summary>
        /// Draws the <paramref name="source"/>&#160;<see cref="IReadableBitmapData"/> into the <paramref name="target"/>&#160;<see cref="IReadWriteBitmapData"/> without scaling
        /// (for scaling use the overloads with <c>targetRectangle</c> parameter). This method is similar to <see cref="Graphics.DrawImage(Image,Point)">Graphics.DrawImage</see>
        /// except that this one guarantees that the image preserves its size in pixels and that it works between any pair of source and target <see cref="PixelFormat"/>s.
        /// If <paramref name="target"/> can represent a narrower set of colors, then the result will be automatically quantized to the colors of the target,
        /// and also an optional <paramref name="ditherer"/> can be specified.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="source">The source <see cref="IReadableBitmapData"/> to be drawn into the <paramref name="target"/>.</param>
        /// <param name="target">The target <see cref="IReadWriteBitmapData"/> into which <paramref name="source"/> should be drawn.</param>
        /// <param name="targetLocation">The target location. Target size will be always the same as the source size.</param>
        /// <param name="ditherer">The ditherer to be used for the drawing. Has no effect, if target pixel format has at least 24 bits-per-pixel size. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <remarks>
        /// <para>The method has the best performance if both source and target images have <see cref="PixelFormat.Format32bppPArgb"/> formats
        /// but works between any combinations and it is always faster than the <see cref="Graphics.DrawImage(Image,Point)">Graphics.DrawImage</see> method.</para>
        /// <para>The image to be drawn is automatically clipped if its size or <paramref name="targetLocation"/> makes it impossible to completely fit in the target.</para>
        /// <para>This overload does not resize the image even if <paramref name="source"/> and <paramref name="target"/> have different DPI resolution.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="target"/> is <see langword="null"/>.</exception>
        public static void DrawInto(this IReadableBitmapData source, IReadWriteBitmapData target, Point targetLocation = default, IQuantizer quantizer = null, IDitherer ditherer = null)
            => DoDrawWithoutResize(source, target, new Rectangle(Point.Empty, source?.GetSize() ?? default), targetLocation, quantizer, ditherer, true);

        public static void DrawInto(this IReadableBitmapData source, IReadWriteBitmapData target, Point targetLocation, IDitherer ditherer)
            => DoDrawWithoutResize(source, target, new Rectangle(Point.Empty, source?.GetSize() ?? default), targetLocation, null, ditherer, true);

        public static void DrawInto(this IReadableBitmapData source, IReadWriteBitmapData target, Rectangle sourceRectangle, Point targetLocation, IDitherer ditherer)
            => DoDrawWithoutResize(source, target, sourceRectangle, targetLocation, null, ditherer, true);

        /// <summary>
        /// Draws the <paramref name="source"/>&#160;<see cref="IReadableBitmapData"/> into the <paramref name="target"/>&#160;<see cref="IReadWriteBitmapData"/> without scaling
        /// (for scaling use the overloads with <c>targetRectangle</c> parameter). This method is similar to <see cref="Graphics.DrawImage(Image,Point)">Graphics.DrawImage</see>
        /// except that this one guarantees that the image preserves its size in pixels and that it works between any pair of source and target <see cref="PixelFormat"/>s.
        /// If <paramref name="target"/> can represent a narrower set of colors, then the result will be automatically quantized to the colors of the target,
        /// and also an optional <paramref name="ditherer"/> can be specified.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="target">The target <see cref="IReadWriteBitmapData"/> into which <paramref name="source"/> should be drawn.</param>
        /// <param name="source">The source <see cref="IReadableBitmapData"/> to be drawn into the <paramref name="target"/>.</param>
        /// <param name="sourceRectangle">The source area to be drawn into the <paramref name="target"/>.</param>
        /// <param name="targetLocation">The target location. Target size will be always the same as the source size.</param>
        /// <param name="ditherer">The ditherer to be used for the drawing. Has no effect, if target pixel format has at least 24 bits-per-pixel size. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <remarks>
        /// <para>The method has the best performance if both source and target images have <see cref="PixelFormat.Format32bppPArgb"/> formats
        /// but works between any combinations and it is always faster than the <see cref="Graphics.DrawImage(Image,Point)">Graphics.DrawImage</see> method.</para>
        /// <para>The image to be drawn is automatically clipped if its size or <paramref name="targetLocation"/> makes it impossible to completely fit in the target.</para>
        /// <para>This overload does not resize the image even if <paramref name="source"/> and <paramref name="target"/> have different DPI resolution.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="target"/> is <see langword="null"/>.</exception>
        [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "src and dst are disposed if necessary")]
        public static void DrawInto(this IReadableBitmapData source, IReadWriteBitmapData target, Rectangle sourceRectangle, Point targetLocation = default, IQuantizer quantizer = null, IDitherer ditherer = null)
            => DoDrawWithoutResize(source, target, sourceRectangle, targetLocation, quantizer, ditherer, true);

        #endregion

        #region With resize

        /// <summary>
        /// Draws the <paramref name="source"/>&#160;<see cref="IReadableBitmapData"/> into the <paramref name="target"/>&#160;<see cref="IReadWriteBitmapData"/> with possible scaling.
        /// This method is similar to <see cref="Graphics.DrawImage(Image, Rectangle)">Graphics.DrawImage</see>
        /// except that this one works between any pair of source and target <see cref="PixelFormat"/>s.
        /// If <paramref name="target"/> can represent a narrower set of colors, then the result will be automatically quantized to the colors of the target,
        /// and also an optional <paramref name="ditherer"/> can be specified.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="target">The target <see cref="IReadWriteBitmapData"/> into which <paramref name="source"/> should be drawn.</param>
        /// <param name="source">The source <see cref="IReadableBitmapData"/> to be drawn into the <paramref name="target"/>.</param>
        /// <param name="targetRectangle">The target area to be drawn the source image.</param>
        /// <param name="scalingMode">Specifies the scaling mode if the image to be drawn needs to be resized. This parameter is optional.
        /// <br/>Default value: <see cref="ScalingMode.Auto"/>.</param>
        /// <param name="ditherer">The ditherer to be used for the drawing. Has no effect, if target pixel format has at least 24 bits-per-pixel size. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <remarks>
        /// <para>The method has the best performance if <paramref name="targetRectangle"/> has the same size as the source image, or when <paramref name="scalingMode"/> is <see cref="ScalingMode.NoScaling"/>.</para>
        /// <para>The image to be drawn is automatically clipped if <paramref name="targetRectangle"/> is exceeds target bounds or <paramref name="scalingMode"/> is <see cref="ScalingMode.NoScaling"/>
        /// and <paramref name="targetRectangle"/> is smaller than the source image.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="target"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="scalingMode"/> has an unsupported value.</exception>
        public static void DrawInto(this IReadableBitmapData source, IReadWriteBitmapData target, Rectangle targetRectangle, IQuantizer quantizer, IDitherer ditherer = null, ScalingMode scalingMode = ScalingMode.Auto)
            => DoDrawWithResize(source, target, new Rectangle(Point.Empty, source?.GetSize() ?? default), targetRectangle, quantizer, ditherer, scalingMode, true);

        /// <summary>
        /// Draws the <paramref name="source"/>&#160;<see cref="IReadableBitmapData"/> into the <paramref name="target"/>&#160;<see cref="IReadWriteBitmapData"/> with possible scaling.
        /// This method is similar to <see cref="Graphics.DrawImage(Image, Rectangle)">Graphics.DrawImage</see>
        /// except that this one works between any pair of source and target <see cref="PixelFormat"/>s.
        /// If <paramref name="target"/> can represent a narrower set of colors, then the result will be automatically quantized to the colors of the target,
        /// and also an optional <paramref name="ditherer"/> can be specified.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="target">The target <see cref="IReadWriteBitmapData"/> into which <paramref name="source"/> should be drawn.</param>
        /// <param name="source">The source <see cref="IReadableBitmapData"/> to be drawn into the <paramref name="target"/>.</param>
        /// <param name="targetRectangle">The target area to be drawn the source image.</param>
        /// <param name="ditherer">The ditherer to be used for the drawing. Has no effect, if target pixel format has at least 24 bits-per-pixel size.
        /// If <see langword="null"/>, then no dithering will be used.</param>
        /// <remarks>
        /// <para>The method has the best performance if <paramref name="targetRectangle"/> has the same size as the source image.</para>
        /// <para>The image to be drawn is automatically clipped if <paramref name="targetRectangle"/> is exceeds target bounds.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="target"/> is <see langword="null"/>.</exception>
        public static void DrawInto(this IReadableBitmapData source, IReadWriteBitmapData target, Rectangle targetRectangle, IDitherer ditherer, ScalingMode scalingMode = ScalingMode.Auto)
            => DoDrawWithResize(source, target, new Rectangle(Point.Empty, source?.GetSize() ?? default), targetRectangle, null, ditherer, scalingMode, true);

        public static void DrawInto(this IReadableBitmapData source, IReadWriteBitmapData target, Rectangle targetRectangle, ScalingMode scalingMode)
            => DoDrawWithResize(source, target, new Rectangle(Point.Empty, source?.GetSize() ?? default), targetRectangle, null, null, scalingMode, true);

        public static void DrawInto(this IReadableBitmapData source, IReadWriteBitmapData target, Rectangle sourceRectangle, Rectangle targetRectangle, IDitherer ditherer, ScalingMode scalingMode = ScalingMode.Auto)
            => DoDrawWithResize(source, target, sourceRectangle, targetRectangle, null, ditherer, scalingMode, true);

        /// <summary>
        /// Draws the <paramref name="source"/>&#160;<see cref="IReadableBitmapData"/> into the <paramref name="target"/>&#160;<see cref="IReadWriteBitmapData"/> with possible scaling.
        /// This method is similar to <see cref="Graphics.DrawImage(Image, Rectangle)">Graphics.DrawImage</see>
        /// except that this one works between any pair of source and target <see cref="PixelFormat"/>s.
        /// If <paramref name="target"/> can represent a narrower set of colors, then the result will be automatically quantized to the colors of the target,
        /// and also an optional <paramref name="ditherer"/> can be specified.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="target">The target <see cref="IReadWriteBitmapData"/> into which <paramref name="source"/> should be drawn.</param>
        /// <param name="source">The source <see cref="IReadableBitmapData"/> to be drawn into the <paramref name="target"/>.</param>
        /// <param name="sourceRectangle">The source area to be drawn into the <paramref name="target"/>.</param>
        /// <param name="targetRectangle">The target area to be drawn the source image.</param>
        /// <param name="ditherer">The ditherer to be used for the drawing. Has no effect, if target pixel format has at least 24 bits-per-pixel size.
        /// If <see langword="null"/>, then no dithering will be used.</param>
        /// <remarks>
        /// <para>The method has the best performance if <paramref name="sourceRectangle"/> and <paramref name="targetRectangle"/> have the same size.</para>
        /// <para>The image to be drawn is automatically clipped if <paramref name="sourceRectangle"/> or <paramref name="targetRectangle"/> exceed bounds.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="target"/> is <see langword="null"/>.</exception>
        public static void DrawInto(this IReadableBitmapData source, IReadWriteBitmapData target, Rectangle sourceRectangle, Rectangle targetRectangle, ScalingMode scalingMode)
            => DoDrawWithResize(source, target, sourceRectangle, targetRectangle, null, null, scalingMode, true);

        /// <summary>
        /// Draws the <paramref name="source"/>&#160;<see cref="IReadableBitmapData"/> into the <paramref name="target"/>&#160;<see cref="IReadWriteBitmapData"/> with possible scaling.
        /// This method is similar to <see cref="Graphics.DrawImage(Image, Rectangle)">Graphics.DrawImage</see>
        /// except that this one works between any pair of source and target <see cref="PixelFormat"/>s.
        /// If <paramref name="target"/> can represent a narrower set of colors, then the result will be automatically quantized to the colors of the target,
        /// and also an optional <paramref name="ditherer"/> can be specified.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="target">The target <see cref="IReadWriteBitmapData"/> into which <paramref name="source"/> should be drawn.</param>
        /// <param name="source">The source <see cref="IReadableBitmapData"/> to be drawn into the <paramref name="target"/>.</param>
        /// <param name="sourceRectangle">The source area to be drawn into the <paramref name="target"/>.</param>
        /// <param name="targetRectangle">The target area to be drawn the source image.</param>
        /// <param name="scalingMode">Specifies the scaling mode if the image to be drawn needs to be resized. This parameter is optional.
        /// <br/>Default value: <see cref="ScalingMode.Auto"/>.</param>
        /// <param name="ditherer">The ditherer to be used for the drawing. Has no effect, if target pixel format has at least 24 bits-per-pixel size. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <remarks>
        /// <para>The method has the best performance if <paramref name="sourceRectangle"/> and <paramref name="targetRectangle"/> have the same size, or when <paramref name="scalingMode"/> is <see cref="ScalingMode.NoScaling"/>.</para>
        /// <para>The image to be drawn is automatically clipped if <paramref name="sourceRectangle"/> or <paramref name="targetRectangle"/> exceed bounds or <paramref name="scalingMode"/> is <see cref="ScalingMode.NoScaling"/>,
        /// and <paramref name="sourceRectangle"/> and <paramref name="targetRectangle"/> are different.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="target"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="scalingMode"/> has an unsupported value.</exception>
        [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "src and dst are disposed if necessary")]
        public static void DrawInto(this IReadableBitmapData source, IReadWriteBitmapData target, Rectangle sourceRectangle, Rectangle targetRectangle, IQuantizer quantizer = null, IDitherer ditherer = null, ScalingMode scalingMode = ScalingMode.Auto)
            => DoDrawWithResize(source, target, sourceRectangle, targetRectangle, quantizer, ditherer, scalingMode, true);

        #endregion

        #endregion

        public static IReadableBitmapData Clip(this IReadableBitmapData source, Rectangle clippingRegion)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source), PublicResources.ArgumentNull);
            return clippingRegion.Location.IsEmpty && clippingRegion.Size == source.GetSize()
                ? source
                : new ClippedBitmapData(source, clippingRegion);
        }

        public static Bitmap ToBitmap(this IReadableBitmapData source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source), PublicResources.ArgumentNull);

            PixelFormat pixelFormat = source.PixelFormat.IsSupportedNatively() ? source.PixelFormat : PixelFormat.Format32bppArgb;
            var result = new Bitmap(source.Width, source.Height, pixelFormat);
            if (pixelFormat.IsIndexed() && source.Palette != null)
                result.SetPalette(source.Palette);

            using (IBitmapDataInternal target = BitmapDataFactory.CreateBitmapData(result, ImageLockMode.WriteOnly, source.BackColor, source.AlphaThreshold, source.Palette))
                source.CopyTo(target, Point.Empty);

            return result;
        }

        #endregion

        #region IWritableBitmapData

        public static IWritableBitmapData Clip(this IWritableBitmapData source, Rectangle clippingRegion)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source), PublicResources.ArgumentNull);
            return clippingRegion.Location.IsEmpty && clippingRegion.Size == source.GetSize()
                ? source
                : new ClippedBitmapData(source, clippingRegion);
        }

        #endregion

        #region IReadWriteBitmapData

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

        internal static void Unwrap<TBitmapData>(ref TBitmapData source, ref Rectangle newRectangle)
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

        internal static void DoDrawWithoutResize(this IReadableBitmapData source, IWritableBitmapData target, Rectangle sourceRectangle, Point targetLocation, IQuantizer quantizer, IDitherer ditherer, bool blend)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source), PublicResources.ArgumentNull);
            if (target == null)
                throw new ArgumentNullException(nameof(target), PublicResources.ArgumentNull);
            Debug.Assert(!blend || target is IReadWriteBitmapData, "For blending target has to be readable");

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
                blend &= source.PixelFormat.HasTransparency() && source.Palette?.HasAlpha != false;

                // processing without using a quantizer
                if (quantizer == null)
                {
                    session.PerformDraw(blend);
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
                            session.PerformDrawWithQuantizer(quantizingSession, blend);
                            return;
                        }

                        // quantization with dithering
                        using (IDitheringSession ditheringSession = ditherer.Initialize(initSource, quantizingSession) ?? throw new InvalidOperationException(Res.ImagingDithererInitializeNull))
                            session.PerformDrawWithDithering(quantizingSession, ditheringSession, blend);
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

        internal static void DoDrawWithResize(this IReadableBitmapData source, IReadWriteBitmapData target, Rectangle sourceRectangle, Rectangle targetRectangle, IQuantizer quantizer, IDitherer ditherer, ScalingMode scalingMode, bool blend)
        {
            // no scaling is necessary
            if (sourceRectangle.Size == targetRectangle.Size || scalingMode == ScalingMode.NoScaling)
            {
                if (!scalingMode.IsDefined())
                    throw new ArgumentOutOfRangeException(nameof(scalingMode), PublicResources.EnumOutOfRange(scalingMode));
                DoDrawWithoutResize(source, target, sourceRectangle, targetRectangle.Location, quantizer, ditherer, blend);
                return;
            }

            if (source == null)
                throw new ArgumentNullException(nameof(source), PublicResources.ArgumentNull);
            if (target == null)
                throw new ArgumentNullException(nameof(target), PublicResources.ArgumentNull);
            if (!scalingMode.IsDefined())
                throw new ArgumentOutOfRangeException(nameof(scalingMode), PublicResources.EnumOutOfRange(scalingMode));

            var sourceBounds = new Rectangle(default, source.GetSize());
            var targetBounds = new Rectangle(default, target.GetSize());
            Unwrap(ref source, ref sourceBounds);
            Unwrap(ref target, ref targetBounds);

            (Rectangle actualSourceRectangle, Rectangle actualTargetRectangle) = GetActualRectangles(sourceBounds, sourceRectangle, targetBounds, targetRectangle);
            if (actualSourceRectangle.IsEmpty || actualTargetRectangle.IsEmpty)
                return;

            AdjustQuantizerAndDitherer(target, ref quantizer, ref ditherer);
            IBitmapDataInternal sessionSource = null;

            // special handling for same references
            if (ReferenceEquals(source, target))
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

            IBitmapDataInternal sessionTarget;
            Rectangle sessionTargetRectangle = actualTargetRectangle;
            if (quantizer != null)
            {
                sessionTarget = (IBitmapDataInternal)BitmapDataFactory.CreateBitmapData(sessionTargetRectangle.Size);
                sessionTargetRectangle.Location = Point.Empty;
            }
            else
                sessionTarget = target as IBitmapDataInternal ?? new BitmapDataWrapper(target, false, true);

            try
            {
                blend &= source.PixelFormat.HasTransparency() && source.Palette?.HasAlpha != false;

                // processing without quantizer
                if (quantizer == null)
                {
                    if (scalingMode == ScalingMode.NearestNeighbor)
                    {
                        var session = new ResizingSessionNearestNeighbor(sessionSource, sessionTarget, actualSourceRectangle, actualTargetRectangle);
                        session.PerformResizeNearestNeighbor(blend);
                    }
                    else
                    {
                        using var session = new ResizingSessionInterpolated(sessionSource, sessionTarget, actualSourceRectangle, actualTargetRectangle, scalingMode);
                        session.PerformResize(blend);
                    }

                    return;
                }

                // If there is a quantizer/ditherer we dump the result in a temp bitmap data, which is applied to the actual target
                // This is needed for initializing the quantizer/ditherer with the actual resized source
                if (scalingMode == ScalingMode.NearestNeighbor)
                {
                    var session = new ResizingSessionNearestNeighbor(sessionSource, sessionTarget, actualSourceRectangle, sessionTargetRectangle);
                    session.PerformResizeNearestNeighbor(blend);
                }
                else
                {
                    using var session = new ResizingSessionInterpolated(sessionSource, sessionTarget, actualSourceRectangle, sessionTargetRectangle, scalingMode);
                    session.PerformResize(blend);
                }

                // As a last step we copy the temp target into the actual one
                DoDrawWithoutResize(sessionTarget, target, sessionTargetRectangle, actualTargetRectangle.Location, quantizer, ditherer, blend);
            }
            finally
            {
                if (!ReferenceEquals(sessionSource, source))
                    sessionSource.Dispose();
                if (!ReferenceEquals(sessionTarget, target))
                    sessionTarget.Dispose();
            }
        }

        #endregion

        #region Private Methods

        internal static (Rectangle Source, Rectangle Target) GetActualRectangles(Rectangle sourceBounds, Rectangle sourceRectangle, Rectangle targetBounds, Point targetLocation)
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

        #endregion

        #endregion
    }
}
