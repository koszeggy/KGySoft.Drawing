using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;

namespace KGySoft.Drawing.Imaging
{
    public static class ReadableBitmapDataExtensions
    {
        public static IReadWriteBitmapData Clone(this IReadableBitmapData source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source), PublicResources.ArgumentNull);
            throw new NotImplementedException("TODO");
        }

        public static IReadWriteBitmapData Clone(this IReadableBitmapData source, Rectangle sourceRectangle, PixelFormat pixelFormat, IDitherer ditherer)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source), PublicResources.ArgumentNull);
            if (sourceRectangle.Location.IsEmpty && sourceRectangle.Width == source.Width && sourceRectangle.Height == source.Height && pixelFormat == source.PixelFormat)
                return Clone(source);

            throw new NotImplementedException("TODO");
        }

        public static void CopyTo(this IReadableBitmapData source, IWritableBitmapData target, Point targetLocation, IDitherer ditherer = null)
            => CopyTo(source, target, new Rectangle(0, 0, source?.Width ?? default, source?.Height ?? default), targetLocation, ditherer);

        //[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "src and dst are disposed if necessary")]
        public static void CopyTo(this IReadableBitmapData source, IWritableBitmapData target, Rectangle sourceRectangle, Point targetLocation, IDitherer ditherer = null)
        {
            throw new NotImplementedException("TODO");
            //if (source == null)
            //    throw new ArgumentNullException(nameof(source), PublicResources.ArgumentNull);
            //if (target == null)
            //    throw new ArgumentNullException(nameof(target), PublicResources.ArgumentNull);

            //(Rectangle actualSourceRectangle, Rectangle actualTargetRectangle) = GetActualRectangles(sourceRectangle, source.Width, source.Height,
            //    targetLocation, target.Width, target.Height);
            //if (actualSourceRectangle.IsEmpty || actualTargetRectangle.IsEmpty)
            //    return;

            //// TODO
            //// - Same references
            ////   - same area: return
            ////   - overlap: clone (can be avoided if target is in negative range from source (min 8 bytes) and processing is sequential) - for positive strides only to avoid errors
            //// - same size and full area and positive stride
            ////   - copy memory
            //// - Different formats, or both are 24/48bpp ARGB32
            ////   - By Color32
            ////   - Test whether this is faster for ARBB32 than by raw longs if possible
            //// - Optimizations for same formats
            ////   By raw, similar to Clear

            //PixelFormat targetPixelFormat = target.PixelFormat;

            //// Cloning source if target and source are the same and source/target rectangles overlap
            //IBitmapDataInternal src = ReferenceEquals(source, target) && actualSourceRectangle.IntersectsWith(actualTargetRectangle)
            //    ? new BitmapDataBuffer(source)
            //    : source as IBitmapDataInternal ?? new BitmapDataWrapper(source, true, false);
            //IBitmapDataInternal dst = target as IBitmapDataInternal ?? new BitmapDataWrapper(target, true, true);

            //try
            //{
            //    if (ditherer == null || !targetPixelFormat.CanBeDithered())
            //        DrawIntoDirect(src, dst, actualSourceRectangle, actualTargetRectangle.Location);
            //    else
            //        DrawIntoWithDithering(src, dst, actualSourceRectangle, actualTargetRectangle.Location, ditherer);
            //}
            //finally
            //{
            //    if (!ReferenceEquals(src, source))
            //        src.Dispose();
            //    if (!ReferenceEquals(dst, target))
            //        dst.Dispose();
            //}
        }

    }
}
