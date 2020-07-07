﻿#region Copyright

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
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security;
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
            if (!session.TryPerformRawCopy())
                session.PerformCopyDirect();

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
            Unwrap(ref source, ref sourceRectangle);
            (session.SourceRectangle, session.TargetRectangle) = source.GetActualRectangles(sourceRectangle, source.GetSize(), Point.Empty);
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
            if (!session.TryPerformRawCopy())
                session.PerformCopyDirect();

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
            Unwrap(ref source, ref sourceRectangle);
            (session.SourceRectangle, session.TargetRectangle) = source.GetActualRectangles(sourceRectangle, source.GetSize(), Point.Empty);
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
                        session.PerformCopyWithQuantizer(quantizingSession);
                    else
                    {
                        // quantizing with dithering
                        using IDitheringSession ditheringSession = ditherer.Initialize(initSource, quantizingSession) ?? throw new InvalidOperationException(Res.ImagingDithererInitializeNull);
                        session.PerformCopyWithDithering(ditheringSession);
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
            => CopyTo(source, target, new Rectangle(Point.Empty, source?.GetSize() ?? default), targetLocation, quantizer, ditherer);

        public static void CopyTo(this IReadableBitmapData source, IWritableBitmapData target, Point targetLocation, IDitherer ditherer)
            => CopyTo(source, target, new Rectangle(Point.Empty, source?.GetSize() ?? default), targetLocation, null, ditherer);

        public static void CopyTo(this IReadableBitmapData source, IWritableBitmapData target, Rectangle sourceRectangle, Point targetLocation, IDitherer ditherer)
            => CopyTo(source, target, sourceRectangle, targetLocation, null, ditherer);

        // TODO Docs:
        // - If the quantizer uses more colors than the target can use the result may have a poorer quality than expected
        public static void CopyTo(this IReadableBitmapData source, IWritableBitmapData target, Rectangle sourceRectangle, Point targetLocation = default, IQuantizer quantizer = null, IDitherer ditherer = null)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source), PublicResources.ArgumentNull);
            if (target == null)
                throw new ArgumentNullException(nameof(target), PublicResources.ArgumentNull);

            var session = new CopySession();
            var targetBounds = new Rectangle(default, target.GetSize());
            Unwrap(ref source, ref sourceRectangle);
            Unwrap(ref target, ref targetBounds);

            (session.SourceRectangle, session.TargetRectangle) = source.GetActualRectangles(sourceRectangle, targetBounds, targetLocation);
            if (session.SourceRectangle.IsEmpty || session.TargetRectangle.IsEmpty)
                return;

            // special handling for same references
            if (ReferenceEquals(source, target))
            {
                // same area: nothing to do (even with dithering because we use a compatible quantizer with self format)
                if (session.SourceRectangle == session.TargetRectangle)
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
                if (quantizer == null)
                {
                    // copying without using a quantizer (even if only a ditherer is specified for a high-bpp pixel format)
                    if (ditherer == null || !target.PixelFormat.CanBeDithered())
                    {
                        // Raw copy if possible
                        if (session.TryPerformRawCopy())
                            return;

                        // By pixels without dithering
                        session.PerformCopyDirect();
                        return;
                    }

                    // if there is only a ditherer specified we pick a quantizer that matches target
                    quantizer = PredefinedColorsQuantizer.FromBitmapData(target);
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
                            session.PerformCopyWithQuantizer(quantizingSession);
                            return;
                        }

                        // quantization with dithering
                        using (IDitheringSession ditheringSession = ditherer.Initialize(initSource, quantizingSession) ?? throw new InvalidOperationException(Res.ImagingDithererInitializeNull))
                            session.PerformCopyWithDithering(ditheringSession);
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
            => DrawInto(source, target, new Rectangle(Point.Empty, source?.GetSize() ?? default), targetLocation, quantizer, ditherer);

        public static void DrawInto(this IReadableBitmapData source, IReadWriteBitmapData target, Point targetLocation, IDitherer ditherer)
            => DrawInto(source, target, new Rectangle(Point.Empty, source?.GetSize() ?? default), targetLocation, null, ditherer);

        public static void DrawInto(this IReadableBitmapData source, IReadWriteBitmapData target, Rectangle sourceRectangle, Point targetLocation, IDitherer ditherer)
            => DrawInto(source, target, sourceRectangle, targetLocation, null, ditherer);

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
        {
            throw new NotImplementedException("TODO: shortcut to Copy if source has no alpha");
            if (source == null)
                throw new ArgumentNullException(nameof(source), PublicResources.ArgumentNull);
            if (target == null)
                throw new ArgumentNullException(nameof(target), PublicResources.ArgumentNull);

            (Rectangle actualSourceRectangle, Rectangle actualTargetRectangle) = source.GetActualRectangles(sourceRectangle, target.GetSize(), targetLocation);
            if (actualSourceRectangle.IsEmpty || actualTargetRectangle.IsEmpty)
                return;

            PixelFormat targetPixelFormat = target.PixelFormat;

            // Cloning source if target and source are the same and source/target rectangles overlap
            IBitmapDataInternal src = ReferenceEquals(source, target) && actualSourceRectangle.IntersectsWith(actualTargetRectangle)
                ? (IBitmapDataInternal)source.Clone()
                : source as IBitmapDataInternal ?? new BitmapDataWrapper(source, true, false);
            IBitmapDataInternal dst = target as IBitmapDataInternal ?? new BitmapDataWrapper(target, true, true);

            try
            {
                if (ditherer == null || !targetPixelFormat.CanBeDithered())
                    DrawIntoDirect(src, dst, actualSourceRectangle, actualTargetRectangle.Location);
                else
                    DrawIntoWithDithering(src, dst, actualSourceRectangle, actualTargetRectangle.Location, ditherer);
            }
            finally
            {
                if (!ReferenceEquals(src, source))
                    src.Dispose();
                if (!ReferenceEquals(dst, target))
                    dst.Dispose();
            }
        }

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
            => DrawInto(source, target, new Rectangle(Point.Empty, source?.GetSize() ?? default), targetRectangle, quantizer, ditherer, scalingMode, true);

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
            => DrawInto(source, target, new Rectangle(Point.Empty, source?.GetSize() ?? default), targetRectangle, null, ditherer, scalingMode, true);

        public static void DrawInto(this IReadableBitmapData source, IReadWriteBitmapData target, Rectangle targetRectangle, ScalingMode scalingMode)
            => DrawInto(source, target, new Rectangle(Point.Empty, source?.GetSize() ?? default), targetRectangle, null, null, scalingMode, true);

        public static void DrawInto(this IReadableBitmapData source, IReadWriteBitmapData target, Rectangle sourceRectangle, Rectangle targetRectangle, IDitherer ditherer, ScalingMode scalingMode = ScalingMode.Auto)
            => DrawInto(source, target, sourceRectangle, targetRectangle, null, ditherer, scalingMode, true);

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
            => DrawInto(source, target, sourceRectangle, targetRectangle, null, null, scalingMode, true);

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
            => DrawInto(source, target, sourceRectangle, targetRectangle, quantizer, ditherer, scalingMode, true);

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

        //public static void DrawImage(this IReadWriteBitmapData target, Image image, Point targetLocation, IDitherer ditherer = null)
        //    => DrawImage(target, image, new Rectangle(Point.Empty, image?.Size ?? default), targetLocation, ditherer);

        //public static void DrawImage(this IReadWriteBitmapData target, Image image, Rectangle sourceRectangle, Point targetLocation, IDitherer ditherer = null)
        //{
        //    throw new NotImplementedException("TODO");
        //}

        //public static void DrawImage(this IReadWriteBitmapData target, Image image, Rectangle targetRectangle, ScalingMode scalingMode = ScalingMode.Auto, IDitherer ditherer = null)
        //    => DrawImage(target, image, new Rectangle(Point.Empty, image?.Size ?? default), targetRectangle, scalingMode, ditherer);

        //public static void DrawImage(this IReadWriteBitmapData target, Image image, Rectangle sourceRectangle, Rectangle targetRectangle, ScalingMode scalingMode = ScalingMode.Auto, IDitherer ditherer = null)
        //{
        //    throw new NotImplementedException("TODO");
        //}

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

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static (Rectangle Source, Rectangle Target) GetActualRectangles(this IBitmapData source, Rectangle sourceRectangle, Size targetSize, Point targetLocation)
            => GetActualRectangles(source, sourceRectangle, new Rectangle(default, targetSize), targetLocation);

        internal static (Rectangle Source, Rectangle Target) GetActualRectangles(this IBitmapData source, Rectangle sourceRectangle, Rectangle targetBounds, Point targetLocation)
        {
            Rectangle sourceBounds = new Rectangle(Point.Empty, source.GetSize());
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

        internal static void DrawInto(this IReadableBitmapData source, IReadWriteBitmapData target, Rectangle sourceRectangle, Rectangle targetRectangle, IQuantizer quantizer, IDitherer ditherer, ScalingMode scalingMode, bool blend)
        {
            // no scaling is necessary
            if (sourceRectangle.Size == targetRectangle.Size || scalingMode == ScalingMode.NoScaling)
            {
                if (scalingMode != ScalingMode.NoScaling && !scalingMode.IsDefined())
                    throw new ArgumentOutOfRangeException(nameof(scalingMode), PublicResources.EnumOutOfRange(scalingMode));
                DrawInto(source, target, sourceRectangle, targetRectangle.Location, quantizer, ditherer);
                return;
            }

            if (source == null)
                throw new ArgumentNullException(nameof(source), PublicResources.ArgumentNull);
            if (target == null)
                throw new ArgumentNullException(nameof(target), PublicResources.ArgumentNull);
            if (!scalingMode.IsDefined())
                throw new ArgumentOutOfRangeException(nameof(scalingMode), PublicResources.EnumOutOfRange(scalingMode));

            (Rectangle actualSourceRectangle, Rectangle actualTargetRectangle) = GetActualRectangles(sourceRectangle, source.Width, source.Height, targetRectangle, target.Width, target.Height);
            if (actualSourceRectangle.IsEmpty || actualTargetRectangle.IsEmpty)
                return;

            // Cloning source if target and source are the same and source/target rectangles overlap
            IBitmapDataInternal src = ReferenceEquals(source, target) && actualSourceRectangle.IntersectsWith(actualTargetRectangle)
                ? (IBitmapDataInternal)source.Clone()
                : source as IBitmapDataInternal ?? new BitmapDataWrapper(source, true, false);
            IBitmapDataInternal dst = target as IBitmapDataInternal ?? new BitmapDataWrapper(target, true, true);

            try
            {
                // Nearest neighbor - shortcut
                if (scalingMode == ScalingMode.NearestNeighbor)
                {
                    if (ditherer == null || !target.PixelFormat.CanBeDithered())
                        ResizeNearestNeighborDirect(src, dst, actualSourceRectangle, actualTargetRectangle);
                    else
                        ResizeNearestNeighborWithDithering(src, dst, actualSourceRectangle, actualTargetRectangle, ditherer);

                    return;
                }

                using (var resizingSession = new ResizingSession(source, target, actualSourceRectangle, actualTargetRectangle, scalingMode))
                {
                    if (ditherer == null || !target.PixelFormat.CanBeDithered())
                        resizingSession.DoResizeDirect(actualTargetRectangle.Top, actualTargetRectangle.Bottom);
                    else
                        resizingSession.DoResizeWithDithering(actualTargetRectangle.Top, actualTargetRectangle.Bottom, ditherer);
                }
            }
            finally
            {
                if (!ReferenceEquals(src, source))
                    src.Dispose();
                if (!ReferenceEquals(dst, target))
                    dst.Dispose();
            }
        }

        [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "src and dst are disposed if necessary")]
        internal static void DrawInto2(this IReadableBitmapData source, IReadWriteBitmapData target, Rectangle sourceRectangle, Rectangle targetRectangle, IQuantizer quantizer, IDitherer ditherer, ScalingMode scalingMode, bool blend)
        {
            // no scaling is necessary
            if (sourceRectangle.Size == targetRectangle.Size || scalingMode == ScalingMode.NoScaling)
            {
                if (scalingMode != ScalingMode.NoScaling && !scalingMode.IsDefined())
                    throw new ArgumentOutOfRangeException(nameof(scalingMode), PublicResources.EnumOutOfRange(scalingMode));
                DrawInto(source, target, sourceRectangle, targetRectangle.Location, quantizer, ditherer);
                return;
            }

            if (source == null)
                throw new ArgumentNullException(nameof(source), PublicResources.ArgumentNull);
            if (target == null)
                throw new ArgumentNullException(nameof(target), PublicResources.ArgumentNull);
            if (!scalingMode.IsDefined())
                throw new ArgumentOutOfRangeException(nameof(scalingMode), PublicResources.EnumOutOfRange(scalingMode));

            (Rectangle actualSourceRectangle, Rectangle actualTargetRectangle) = GetActualRectangles(sourceRectangle, source.Width, source.Height, targetRectangle, target.Width, target.Height);
            if (actualSourceRectangle.IsEmpty || actualTargetRectangle.IsEmpty)
                return;

            // Cloning source if target and source are the same and source/target rectangles overlap
            IBitmapDataInternal src = ReferenceEquals(source, target) && actualSourceRectangle.IntersectsWith(actualTargetRectangle)
                ? (IBitmapDataInternal)source.Clone()
                : source as IBitmapDataInternal ?? new BitmapDataWrapper(source, true, false);
            IBitmapDataInternal dst = target as IBitmapDataInternal ?? new BitmapDataWrapper(target, true, true);

            try
            {
                // Nearest neighbor - shortcut
                if (scalingMode == ScalingMode.NearestNeighbor)
                {
                    if (ditherer == null || !target.PixelFormat.CanBeDithered())
                        ResizeNearestNeighborDirect(src, dst, actualSourceRectangle, actualTargetRectangle);
                    else
                        ResizeNearestNeighborWithDithering(src, dst, actualSourceRectangle, actualTargetRectangle, ditherer);

                    return;
                }

                using (var resizingSession = new ResizingSession(source, target, actualSourceRectangle, actualTargetRectangle, scalingMode))
                {
                    if (ditherer == null || !target.PixelFormat.CanBeDithered())
                        resizingSession.DoResizeDirect(actualTargetRectangle.Top, actualTargetRectangle.Bottom);
                    else
                        resizingSession.DoResizeWithDithering2(actualTargetRectangle.Top, actualTargetRectangle.Bottom, ditherer);
                }
            }
            finally
            {
                if (!ReferenceEquals(src, source))
                    src.Dispose();
                if (!ReferenceEquals(dst, target))
                    dst.Dispose();
            }
        }

        #endregion

        #region Private Methods

        [SecuritySafeCritical]
        [SuppressMessage("ReSharper", "AccessToDisposedClosure", Justification = "ParallelHelper.For invokes delegates before returning")]
        private static void DrawIntoDirect(IBitmapDataInternal source, IBitmapDataInternal target, Rectangle sourceRect, Point targetLocation)
        {
            #region Local Methods

            void ProcessRowStraight(int y)
            {
                IBitmapDataRowInternal rowSrc = source.GetRow(sourceRect.Y + y);
                IBitmapDataRowInternal rowDst = target.GetRow(targetLocation.Y + y);
                int sourceLeft = sourceRect.Left;
                int targetLeft = targetLocation.X;
                int sourceWidth = sourceRect.Width;
                for (int x = 0; x < sourceWidth; x++)
                {
                    Color32 colorSrc = rowSrc.DoGetColor32(sourceLeft + x);

                    // fully transparent source: skip
                    if (colorSrc.A == 0)
                        continue;

                    // fully solid source: overwrite
                    if (colorSrc.A == Byte.MaxValue)
                    {
                        rowDst.DoSetColor32(targetLeft + x, colorSrc);
                        continue;
                    }

                    // source here has a partial transparency: we need to read the target color
                    int pos = targetLeft + x;
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

            void ProcessRowPremultiplied(int y)
            {
                IBitmapDataRowInternal rowSrc = source.GetRow(sourceRect.Y + y);
                IBitmapDataRowInternal rowDst = target.GetRow(targetLocation.Y + y);
                int sourceLeft = sourceRect.Left;
                int targetLeft = targetLocation.X;
                int sourceWidth = sourceRect.Width;
                bool isPremultipliedSource = source.PixelFormat == PixelFormat.Format32bppPArgb;
                for (int x = 0; x < sourceWidth; x++)
                {
                    Color32 colorSrc = isPremultipliedSource
                            ? rowSrc.DoReadRaw<Color32>(sourceLeft + x)
                            : rowSrc.DoGetColor32(sourceLeft + x).ToPremultiplied();

                    // fully transparent source: skip
                    if (colorSrc.A == 0)
                        continue;

                    // fully solid source: overwrite
                    if (colorSrc.A == Byte.MaxValue)
                    {
                        rowDst.DoWriteRaw(targetLeft + x, colorSrc);
                        continue;
                    }

                    // source here has a partial transparency: we need to read the target color
                    int pos = targetLeft + x;
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

            Action<int> processRow = target.PixelFormat == PixelFormat.Format32bppPArgb
                    ? ProcessRowPremultiplied
                    : (Action<int>)ProcessRowStraight;

            // Sequential processing
            if (sourceRect.Width < parallelThreshold)
            {
                for (int y = 0; y < sourceRect.Height; y++)
                    processRow.Invoke(y);
                return;
            }

            // Parallel processing
            ParallelHelper.For(0, sourceRect.Height, processRow);
        }

        [SuppressMessage("ReSharper", "AccessToDisposedClosure", Justification = "ParallelHelper.For invokes delegates before returning")]
        private static void DrawIntoWithDithering(IBitmapDataInternal source, IBitmapDataInternal target, Rectangle sourceRect, Point targetLocation, IDitherer ditherer)
        {
            #region Local Methods

            static void ProcessRow(int y, IDitheringSession session, IBitmapDataInternal src, IBitmapDataInternal dst, Rectangle rectSrc, Point locDst)
            {
                int ySrc = y + rectSrc.Top;
                IBitmapDataRowInternal rowSrc = src.GetRow(ySrc);
                IBitmapDataRowInternal rowDst = dst.GetRow(y + locDst.Y);

                for (int x = 0; x < rectSrc.Width; x++)
                {
                    int xSrc = x + rectSrc.Left;
                    Color32 colorSrc = rowSrc.DoGetColor32(xSrc);

                    // fully transparent source: skip
                    if (colorSrc.A == 0)
                        continue;

                    // fully solid source: overwrite
                    if (colorSrc.A == Byte.MaxValue)
                    {
                        rowDst.DoSetColor32(x + locDst.X, session.GetDitheredColor(colorSrc, xSrc, ySrc));
                        continue;
                    }

                    // source here has a partial transparency: we need to read the target color
                    int xDst = locDst.X + x;
                    Color32 colorDst = rowDst.DoGetColor32(xDst);

                    // fully transparent target: we can overwrite with source
                    if (colorDst.A == 0)
                    {
                        rowDst.DoSetColor32(xDst, session.GetDitheredColor(colorSrc, xSrc, ySrc));
                        continue;
                    }

                    colorSrc = colorDst.A == Byte.MaxValue
                        // target pixel is fully solid: simple blending
                        ? colorSrc.BlendWithBackground(colorDst)
                        // both source and target pixels are partially transparent: complex blending
                        : colorSrc.BlendWith(colorDst);

                    rowDst.DoSetColor32(xDst, session.GetDitheredColor(colorSrc, xSrc, ySrc));
                }
            }

            #endregion

            IQuantizer quantizer = PredefinedColorsQuantizer.FromBitmapData(target);
            using (IQuantizingSession quantizingSession = quantizer.Initialize(source))
            using (IDitheringSession ditheringSession = ditherer.Initialize(source, quantizingSession) ?? throw new InvalidOperationException(Res.ImagingDithererInitializeNull))
            {
                // sequential processing
                if (ditheringSession.IsSequential || sourceRect.Width < parallelThreshold)
                {
                    for (int y = 0; y < sourceRect.Height; y++)
                        ProcessRow(y, ditheringSession, source, target, sourceRect, targetLocation);
                    return;
                }

                // parallel processing
                ParallelHelper.For(0, sourceRect.Height, y =>
                {
                    ProcessRow(y, ditheringSession, source, target, sourceRect, targetLocation);
                });
            }
        }

        private static (Rectangle, Rectangle) GetActualRectangles(Rectangle sourceRectangle, int sourceWidth, int sourceHeight, Rectangle targetRectangle, int targetWidth, int targetHeight)
        {
            Rectangle sourceBounds = new Rectangle(Point.Empty, new Size(sourceWidth, sourceHeight));
            Rectangle actualSourceRectangle = Rectangle.Intersect(sourceRectangle, sourceBounds);
            if (actualSourceRectangle.IsEmpty)
                return default;
            Rectangle targetBounds = new Rectangle(Point.Empty, new Size(targetWidth, targetHeight));
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

        private static void ResizeNearestNeighborDirect(IBitmapDataInternal source, IBitmapDataInternal target, Rectangle sourceRectangle, Rectangle targetRectangle)
        {
            // Scaling factors
            float widthFactor = sourceRectangle.Width / (float)targetRectangle.Width;
            float heightFactor = sourceRectangle.Height / (float)targetRectangle.Height;

            #region Local Methods

            void ProcessRowStraight(int y)
            {
                IBitmapDataRowInternal rowSrc = source.GetRow((int)((y - targetRectangle.Y) * heightFactor + sourceRectangle.Y));
                IBitmapDataRowInternal rowDst = target.GetRow(y);

                int targetLeft = targetRectangle.Left;
                int targetRight = targetRectangle.Right;
                int sourceLeft = sourceRectangle.Left;
                for (int x = targetLeft; x < targetRight; x++)
                {
                    Color32 colorSrc = rowSrc.DoGetColor32((int)((x - targetLeft) * widthFactor + sourceLeft));

                    // fully transparent source: skip
                    if (colorSrc.A == 0)
                        continue;

                    // fully solid source: overwrite
                    if (colorSrc.A == Byte.MaxValue)
                    {
                        rowDst.DoSetColor32(x, colorSrc);
                        continue;
                    }

                    // source here has a partial transparency: we need to read the target color
                    Color32 colorDst = rowDst.DoGetColor32(x);

                    // fully transparent target: we can overwrite with source
                    if (colorDst.A == 0)
                    {
                        rowDst.DoSetColor32(x, colorSrc);
                        continue;
                    }

                    colorSrc = colorDst.A == Byte.MaxValue
                        // target pixel is fully solid: simple blending
                        ? colorSrc.BlendWithBackground(colorDst)
                        // both source and target pixels are partially transparent: complex blending
                        : colorSrc.BlendWith(colorDst);

                    rowDst.DoSetColor32(x, colorSrc);
                }
            }

            void ProcessRowPremultiplied(int y)
            {
                IBitmapDataRowInternal rowSrc = source.GetRow((int)((y - targetRectangle.Y) * heightFactor + sourceRectangle.Y));
                IBitmapDataRowInternal rowDst = target.GetRow(y);
                bool isPremultipliedSource = source.PixelFormat == PixelFormat.Format32bppPArgb;

                int targetLeft = targetRectangle.Left;
                int targetRight = targetRectangle.Right;
                int sourceLeft = sourceRectangle.Left;
                for (int x = targetLeft; x < targetRight; x++)
                {
                    Color32 colorSrc = isPremultipliedSource
                        ? rowSrc.DoReadRaw<Color32>((int)((x - targetLeft) * widthFactor + sourceLeft))
                        : rowSrc.DoGetColor32((int)((x - targetLeft) * widthFactor + sourceLeft)).ToPremultiplied();

                    // fully transparent source: skip
                    if (colorSrc.A == 0)
                        continue;

                    // fully solid source: overwrite
                    if (colorSrc.A == Byte.MaxValue)
                    {
                        rowDst.DoWriteRaw(x, colorSrc);
                        continue;
                    }

                    // source here has a partial transparency: we need to read the target color
                    Color32 colorDst = rowDst.DoReadRaw<Color32>(x);

                    // fully transparent target: we can overwrite with source
                    if (colorDst.A == 0)
                    {
                        rowDst.DoWriteRaw(x, colorSrc);
                        continue;
                    }

                    rowDst.DoWriteRaw(x, colorSrc.BlendWithPremultiplied(colorDst));
                }
            }

            #endregion

            Action<int> processRow = target.PixelFormat == PixelFormat.Format32bppPArgb
                ? ProcessRowPremultiplied
                : (Action<int>)ProcessRowStraight;

            // Sequential processing
            if (targetRectangle.Width < parallelThreshold)
            {
                for (int y = targetRectangle.Top; y < targetRectangle.Bottom; y++)
                    processRow.Invoke(y);
                return;
            }

            // Parallel processing
            ParallelHelper.For(targetRectangle.Top, targetRectangle.Bottom, processRow);
        }

        private static void ResizeNearestNeighborWithDithering(IBitmapDataInternal source, IBitmapDataInternal target, Rectangle sourceRectangle, Rectangle targetRectangle, IDitherer ditherer)
        {
            // TODO - this is the intolerant ditherer version. No blending is needed here as target is always transparent
            // TODO: Use direct version if possible - see session
            throw new NotImplementedException("TODO");
            using var result = BitmapDataFactory.CreateManagedBitmapData(targetRectangle.Size);

            // Scaling factors
            float widthFactor = sourceRectangle.Width / (float)targetRectangle.Width;
            float heightFactor = sourceRectangle.Height / (float)targetRectangle.Height;

            #region Local Methods

            void ProcessRow(int y)
            {
                IBitmapDataRowInternal rowSrc = source.GetRow((int)(y * heightFactor + sourceRectangle.Y));
                IBitmapDataRowInternal rowDst = result.GetRow(y);

                int sourceLeft = sourceRectangle.Left;
                int width = result.Width;
                for (int x = 0; x < width; x++)
                {
                    Color32 colorSrc = rowSrc.DoGetColor32((int)(x * widthFactor + sourceLeft));

                    // fully transparent source: skip
                    if (colorSrc.A == 0)
                        continue;

                    // fully solid source: overwrite
                    if (colorSrc.A == Byte.MaxValue)
                    {
                        rowDst.DoSetColor32(x, colorSrc);
                        continue;
                    }

                    // source here has a partial transparency: we need to read the target color
                    Color32 colorDst = rowDst.DoGetColor32(x);

                    // fully transparent target: we can overwrite with source
                    if (colorDst.A == 0)
                    {
                        rowDst.DoSetColor32(x, colorSrc);
                        continue;
                    }

                    colorSrc = colorDst.A == Byte.MaxValue
                        // target pixel is fully solid: simple blending
                        ? colorSrc.BlendWithBackground(colorDst)
                        // both source and target pixels are partially transparent: complex blending
                        : colorSrc.BlendWith(colorDst);

                    rowDst.DoSetColor32(x, colorSrc);
                }
            }

            #endregion

            // Sequential processing
            if (targetRectangle.Width < parallelThreshold)
            {
                for (int y = 0; y < targetRectangle.Height; y++)
                    ProcessRow(y);
            }
            // Parallel processing
            else
                ParallelHelper.For(0, targetRectangle.Height, ProcessRow);

            // Drawing result to actual target with dithering
            result.DrawInto(target, targetRectangle.Location, ditherer);
        }

        #endregion

        #endregion
    }
}