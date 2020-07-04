#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ReadableBitmapDataExtensions.cs
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
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

#endregion

namespace KGySoft.Drawing.Imaging
{
    public static partial class ReadableBitmapDataExtensions
    {
        #region Methods

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
            BitmapDataExtensions.Unwrap(ref source, ref session.SourceRectangle);
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
            BitmapDataExtensions.Unwrap(ref source, ref sourceRectangle);
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
            BitmapDataExtensions.Unwrap(ref source, ref sourceRectangle);
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
            BitmapDataExtensions.Unwrap(ref source, ref sourceRectangle);
            BitmapDataExtensions.Unwrap(ref target, ref targetBounds);

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
    }
}
