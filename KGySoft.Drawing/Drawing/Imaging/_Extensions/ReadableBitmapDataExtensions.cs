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

#endregion

namespace KGySoft.Drawing.Imaging
{
    public static partial class ReadableBitmapDataExtensions
    {
        #region Methods

        public static IReadWriteBitmapData Clone(this IReadableBitmapData source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source), PublicResources.ArgumentNull);

            IBitmapDataInternal result = BitmapDataFactory.CreateManagedBitmapData(source.GetSize(), source.PixelFormat, source.BackColor, source.AlphaThreshold, source.Palette);
            source.CopyTo(result, Point.Empty);
            return result;
        }

        public static IReadWriteBitmapData Clone(this IReadableBitmapData source, Rectangle sourceRectangle, PixelFormat pixelFormat)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source), PublicResources.ArgumentNull);
            (Rectangle actualSourceRectangle, Rectangle actualTargetRectangle) = source.GetActualRectangles(sourceRectangle, source.GetSize(), Point.Empty);
            Debug.Assert(actualSourceRectangle.Size == actualTargetRectangle.Size);
            if (actualSourceRectangle.IsEmpty)
                throw new ArgumentOutOfRangeException(nameof(sourceRectangle), PublicResources.ArgumentOutOfRange);
            IBitmapDataInternal result = BitmapDataFactory.CreateManagedBitmapData(actualTargetRectangle.Size, pixelFormat, source.BackColor, source.AlphaThreshold, source.Palette);
            source.CopyTo(result, sourceRectangle, Point.Empty);
            return result;
        }


        public static IReadWriteBitmapData Clone(this IReadableBitmapData source, Rectangle sourceRectangle, PixelFormat pixelFormat, IQuantizer quantizer = null, IDitherer ditherer = null)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source), PublicResources.ArgumentNull);
            if (sourceRectangle.Location.IsEmpty && sourceRectangle.Width == source.Width && sourceRectangle.Height == source.Height && pixelFormat == source.PixelFormat)
                return Clone(source);

            throw new NotImplementedException("TODO");
        }

        public static void CopyTo(this IReadableBitmapData source, IWritableBitmapData target, Point targetLocation)
            => CopyTo(source, target, new Rectangle(0, 0, source?.Width ?? default, source?.Height ?? default), targetLocation);

        public static void CopyTo(this IReadableBitmapData source, IWritableBitmapData target, Point targetLocation, IDitherer ditherer)
            => CopyTo(source, target, new Rectangle(0, 0, source?.Width ?? default, source?.Height ?? default), targetLocation, ditherer);

        public static void CopyTo(this IReadableBitmapData source, IWritableBitmapData target, Rectangle sourceRectangle, Point targetLocation)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source), PublicResources.ArgumentNull);
            if (target == null)
                throw new ArgumentNullException(nameof(target), PublicResources.ArgumentNull);

            // TODO: when to ignore ditherer?
            // - target makes no sense to dither (ok)
            // - same pixel format and palette (todo) - actually it could make difference (eg strong ordered) but that is true also for high BPPs - document behavior!

            var session = new CopySession();
            (session.SourceRectangle, session.TargetRectangle) = source.GetActualRectangles(sourceRectangle, target.GetSize(), targetLocation);
            if (session.SourceRectangle.IsEmpty || session.TargetRectangle.IsEmpty)
                return;

            // special handling for same references
            if (ReferenceEquals(source, target))
            {
                // same area: nothing to do (even with dithering because we use a compatible quantizer with self format)
                if (session.SourceRectangle == session.TargetRectangle)
                    return;

                // overlap: clone source
                if (session.IsOverlap)
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
                // Raw copy if possible
                if (session.TryPerformRawCopy())
                    return;

                // By pixels without dithering
                session.PerformCopyDirect();
            }
            finally
            {
                if (!ReferenceEquals(session.Source, source))
                    session.Source.Dispose();
                if (!ReferenceEquals(session.Target, target))
                    session.Target.Dispose();
            }
        }

        public static void CopyTo(this IReadableBitmapData source, IWritableBitmapData target, Rectangle sourceRectangle, Point targetLocation, IDitherer ditherer)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source), PublicResources.ArgumentNull);
            if (target == null)
                throw new ArgumentNullException(nameof(target), PublicResources.ArgumentNull);

            if (ditherer == null && !target.PixelFormat.CanBeDithered())
            {
                CopyTo(source, target, sourceRectangle, targetLocation);
                return;
            }

            throw new NotImplementedException("TODO");
        }

        public static Bitmap ToBitmap(this IReadableBitmapData source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source), PublicResources.ArgumentNull);

            PixelFormat pixelFormat = source.PixelFormat.IsSupportedNatively() ? source.PixelFormat : PixelFormat.Format32bppArgb;
            var result = new Bitmap(source.Width, source.Height, pixelFormat);
            using (IBitmapDataInternal target = BitmapDataFactory.CreateBitmapData(result, ImageLockMode.WriteOnly, source.BackColor, source.AlphaThreshold, source.Palette))
                source.CopyTo(target, Point.Empty);

            return result;
        }

        #endregion
    }
}
