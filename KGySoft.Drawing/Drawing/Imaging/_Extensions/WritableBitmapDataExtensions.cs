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

            var sourceSize = new Size(source.Width, source.Height);

            // clipping source rectangle with actual source size
            sourceRectangle.Intersect(new Rectangle(Point.Empty, sourceSize));

            // calculating target rectangle
            var targetSize = new Size(target.Width, target.Height);
            var targetRectangle = new Rectangle(targetLocation, sourceRectangle.Size);
            if (targetRectangle.Right > targetSize.Width)
            {
                targetRectangle.Width -= targetRectangle.Right - targetSize.Width;
                sourceRectangle.Width = targetRectangle.Width;
            }

            if (targetRectangle.Bottom > targetSize.Height)
            {
                targetRectangle.Height -= targetRectangle.Bottom - targetSize.Height;
                sourceRectangle.Height = targetRectangle.Height;
            }

            if (targetRectangle.Left < 0)
            {
                sourceRectangle.Width += targetRectangle.Left;
                sourceRectangle.X -= targetRectangle.Left;
                targetRectangle.Width += targetRectangle.Left;
                targetRectangle.X = 0;
            }

            if (targetRectangle.Top < 0)
            {
                sourceRectangle.Height += targetRectangle.Top;
                sourceRectangle.Y -= targetRectangle.Top;
                targetRectangle.Height += targetRectangle.Top;
                targetRectangle.Y = 0;
            }

            // returning, if there is no remaining source to draw
            if (sourceRectangle.Height <= 0 || sourceRectangle.Width <= 0)
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
                    DrawIntoDirect(src, dst, sourceRectangle, targetRectangle.Location);
                else
                    DrawIntoWithDithering(src, dst, sourceRectangle, targetRectangle.Location, ditherer);
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

            Rectangle actualSrcRect = Rectangle.Intersect(sourceRectangle, new Rectangle(0, 0, source.Width, source.Height));
            Rectangle actualDstRect = Rectangle.Intersect(targetRectangle, new Rectangle(0, 0, target.Width, target.Height));
            if (actualSrcRect.IsEmpty || actualDstRect.IsEmpty)
                return;

            // TODO: del
            //var interest = destinationRectangle; //Rectangle.Intersect(destinationRectangle, destination.Bounds());

            // Nearest neighbor - TODO: extract method
            if (scalingMode == ScalingMode.NearestNeighbor)
            {
                // Scaling factors
                float widthFactor = sourceRectangle.Width / (float)targetRectangle.Width;
                float heightFactor = sourceRectangle.Height / (float)targetRectangle.Height;

                for (int y = actualDstRect.Top; y < actualDstRect.Bottom; y++)
                {
                    // TODO: cache calculated properties, parallel, consider clipping
                    var sourceRow = source[(int)(((y - targetRectangle.Y) * heightFactor) + sourceRectangle.Y)];
                    var targetRow = target[y];

                    for (int x = targetRectangle.Left; x < targetRectangle.Right; x++)
                    {
                        // X coordinates of source points
                        targetRow[x] = sourceRow[(int)(((x - targetRectangle.X) * widthFactor) + sourceRectangle.X)];
                    }
                }

                return;
            }

            using (var resizingSession = new ResizingSession(source, target, sourceRectangle, targetRectangle, actualDstRect, scalingMode))
            {
                // TODO: parallel, ditherer (maybe inside DoResize)
                resizingSession.DoResize(actualDstRect.Top, actualDstRect.Bottom, ditherer);
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

    }
}
