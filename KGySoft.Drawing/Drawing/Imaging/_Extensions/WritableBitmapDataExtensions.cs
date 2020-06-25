using KGySoft.CoreLibraries;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.Security;

namespace KGySoft.Drawing.Imaging
{
    /// <summary>
    /// Contains extension methods for the <see cref="IWritableBitmapData"/> type.
    /// </summary>
    internal static partial class WritableBitmapDataExtensions
    {
        #region Constants

        private const int parallelThreshold = 100;

        #endregion

        public static void DrawImage(this IWritableBitmapData target, Image image, Point targetLocation, IDitherer ditherer = null)
            => DrawImage(target, image, new Rectangle(Point.Empty, image?.Size ?? default), targetLocation, ditherer);

        public static void DrawImage(this IWritableBitmapData target, Image image, Rectangle sourceRectangle, Point targetLocation, IDitherer ditherer = null)
        {
            throw new NotImplementedException("TODO");
        }

        public static void DrawImage(this IWritableBitmapData target, Image image, Rectangle targetRectangle, ScalingMode scalingMode = ScalingMode.Auto, IDitherer ditherer = null)
            => DrawImage(target, image, new Rectangle(Point.Empty, image?.Size ?? default), targetRectangle, scalingMode, ditherer);

        public static void DrawImage(this IWritableBitmapData target, Image image, Rectangle sourceRectangle, Rectangle targetRectangle, ScalingMode scalingMode = ScalingMode.Auto, IDitherer ditherer = null)
        {
            throw new NotImplementedException("TODO");
        }

        public static void DrawBitmapData(this IWritableBitmapData target, IReadableBitmapData source, Point targetLocation, IDitherer ditherer = null)
            => DrawBitmapData(target, source, new Rectangle(Point.Empty, new Size(source?.Width ?? default, source?.Height ?? default)), targetLocation, ditherer);

        public static void DrawBitmapData(this IWritableBitmapData target, IReadableBitmapData source, Rectangle sourceRectangle, Point targetLocation, IDitherer ditherer = null)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source), PublicResources.ArgumentNull);
            if (target == null)
                throw new ArgumentNullException(nameof(target), PublicResources.ArgumentNull);

            (Rectangle actualSourceRectangle, Rectangle actualTargetRectangle) = GetActualRectangles(sourceRectangle, source.Width, source.Height,
                targetLocation, target.Width, target.Height);
            if (actualSourceRectangle.IsEmpty || actualTargetRectangle.IsEmpty)
                return;

            PixelFormat targetPixelFormat = target.PixelFormat;

            // TODO: clone if same instance with undesired overlapping (just copy into some custom bitmapdata)
            //// Cloning source if target and source are the same, or creating a new bitmap is source is a metafile
            //Bitmap bmp = ReferenceEquals(source, target)
            //    ? ((Bitmap)source).CloneCurrentFrame()
            //    : source as Bitmap ?? new Bitmap(source);

            try
            {
                IBitmapDataInternal src = source as IBitmapDataInternal ?? new BitmapDataWrapper(source, true);
                IBitmapDataInternal dst = target as IBitmapDataInternal ?? new BitmapDataWrapper(target, false);
                if (ditherer == null || !targetPixelFormat.CanBeDithered())
                    DrawIntoDirect(src, dst, actualSourceRectangle, actualTargetRectangle.Location);
                else
                    DrawIntoWithDithering(src, dst, actualSourceRectangle, actualTargetRectangle.Location, ditherer);
            }
            finally
            {
                // TODO
                //if (!ReferenceEquals(bmp, source))
                //    bmp.Dispose();
            }
        }

        public static void DrawBitmapData(this IWritableBitmapData target, IReadableBitmapData source, Rectangle targetRectangle, ScalingMode scalingMode = ScalingMode.Auto, IDitherer ditherer = null)
            => DrawBitmapData(target, source, new Rectangle(Point.Empty, new Size(source?.Width ?? default, source?.Height ?? default)), targetRectangle, scalingMode, ditherer);

        public static void DrawBitmapData(this IWritableBitmapData target, IReadableBitmapData source, Rectangle sourceRectangle, Rectangle targetRectangle, ScalingMode scalingMode = ScalingMode.Auto, IDitherer ditherer = null)
        {
            // no scaling is necessary
            if (sourceRectangle.Size == targetRectangle.Size || scalingMode == ScalingMode.NoScaling)
            {
                DrawBitmapData(target, source, sourceRectangle, targetRectangle.Location, ditherer);
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

            // Nearest neighbor - shortcut
            if (scalingMode == ScalingMode.NearestNeighbor)
            {
                ResizeNearestNeighborDirect(target, source, actualSourceRectangle, actualTargetRectangle);

                return;
            }

            using (var resizingSession = new ResizingSession(source, target, actualSourceRectangle, actualTargetRectangle, scalingMode))
            {
                resizingSession.DoResize(actualTargetRectangle.Top, actualTargetRectangle.Bottom, ditherer);
            }
        }

        [SecuritySafeCritical]
        [SuppressMessage("ReSharper", "AccessToDisposedClosure", Justification = "ParallelHelper.For invokes delegates before returning")]
        private static void DrawIntoDirect(IBitmapDataInternal source, IBitmapDataInternal target, Rectangle sourceRect, Point targetLocation)
        {
            #region Local Methods

            static void ProcessRowStraight(int y, IBitmapDataInternal src, IBitmapDataInternal dst, Rectangle rectSrc, Point locDst)
            {
                IBitmapDataRowInternal rowSrc = src.GetRow(rectSrc.Y + y);
                IBitmapDataRowInternal rowDst = dst.GetRow(locDst.Y + y);
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

            static void ProcessRowPremultiplied(int y, IBitmapDataInternal src, IBitmapDataInternal dst, Rectangle rectSrc, Point locDst)
            {
                IBitmapDataRowInternal rowSrc = src.GetRow(rectSrc.Y + y);
                IBitmapDataRowInternal rowDst = dst.GetRow(locDst.Y + y);
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

            Action<int, IBitmapDataInternal, IBitmapDataInternal, Rectangle, Point> processRow = target.PixelFormat == PixelFormat.Format32bppPArgb
                ? (Action<int, IBitmapDataInternal, IBitmapDataInternal, Rectangle, Point>)ProcessRowPremultiplied
                : ProcessRowStraight;

            // Sequential processing
            if (sourceRect.Width < parallelThreshold)
            {
                for (int y = 0; y < sourceRect.Height; y++)
                    processRow.Invoke(y, source, target, sourceRect, targetLocation);
                return;
            }

            // Parallel processing
            ParallelHelper.For(0, sourceRect.Height,
                y => processRow.Invoke(y, source, target, sourceRect, targetLocation));
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
                    rowDst.DoSetColor32(x + locDst.X,
                        session.GetDitheredColor(rowSrc.DoGetColor32(xSrc), xSrc, ySrc));
                }
            }

            #endregion

            IQuantizer quantizer = PredefinedColorsQuantizer.FromBitmapData(source);
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

        private static (Rectangle, Rectangle) GetActualRectangles(Rectangle sourceRectangle, int sourceWidth, int sourceHeight, Point targetLocation, int targetWidth, int targetHeight)
        {
            Rectangle sourceBounds = new Rectangle(Point.Empty, new Size(sourceWidth, sourceHeight));
            Rectangle actualSourceRectangle = Rectangle.Intersect(sourceRectangle, sourceBounds);
            if (actualSourceRectangle.IsEmpty)
                return default;
            Rectangle targetRectangle = new Rectangle(targetLocation, sourceRectangle.Size);
            Rectangle targetBounds = new Rectangle(Point.Empty, new Size(targetWidth, targetHeight));
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
    }
}
