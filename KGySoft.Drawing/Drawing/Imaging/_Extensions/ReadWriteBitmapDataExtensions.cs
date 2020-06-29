#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ReadWriteBitmapDataExtensions.cs
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
using System.Security;

using KGySoft.CoreLibraries;

#endregion

namespace KGySoft.Drawing.Imaging
{
    /// <summary>
    /// Contains extension methods for the <see cref="IReadWriteBitmapData"/> type.
    /// </summary>
    public static partial class ReadWriteBitmapDataExtensions
    {
        #region Constants

        private const int parallelThreshold = 100;

        #endregion

        #region Methods

        #region Public Methods

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
        public static void DrawBitmapData(this IReadWriteBitmapData target, IReadableBitmapData source, Point targetLocation, IDitherer ditherer = null)
            => DrawBitmapData(target, source, new Rectangle(Point.Empty, new Size(source?.Width ?? default, source?.Height ?? default)), targetLocation, ditherer);

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
        public static void DrawBitmapData(this IReadWriteBitmapData target, IReadableBitmapData source, Rectangle sourceRectangle, Point targetLocation, IDitherer ditherer = null)
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

            // Cloning source if target and source are the same and source/target rectangles overlap
            IBitmapDataInternal src = ReferenceEquals(source, target) && actualSourceRectangle.IntersectsWith(actualTargetRectangle)
                ? new BitmapDataBuffer(source)
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
        public static void DrawBitmapData(this IReadWriteBitmapData target, IReadableBitmapData source, Rectangle targetRectangle, ScalingMode scalingMode = ScalingMode.Auto, IDitherer ditherer = null)
            => DrawBitmapData(target, source, new Rectangle(Point.Empty, new Size(source?.Width ?? default, source?.Height ?? default)), targetRectangle, scalingMode, ditherer);

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
        public static void DrawBitmapData(this IReadWriteBitmapData target, IReadableBitmapData source, Rectangle targetRectangle, IDitherer ditherer)
            => DrawBitmapData(target, source, new Rectangle(Point.Empty, new Size(source?.Width ?? default, source?.Height ?? default)), targetRectangle, ScalingMode.Auto, ditherer);

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
        public static void DrawBitmapData(this IReadWriteBitmapData target, IReadableBitmapData source, Rectangle sourceRectangle, Rectangle targetRectangle, IDitherer ditherer)
            => DrawBitmapData(target, source, sourceRectangle, targetRectangle, ScalingMode.Auto, ditherer);

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
        public static void DrawBitmapData(this IReadWriteBitmapData target, IReadableBitmapData source, Rectangle sourceRectangle, Rectangle targetRectangle, ScalingMode scalingMode = ScalingMode.Auto, IDitherer ditherer = null)
        {
            // no scaling is necessary
            if (sourceRectangle.Size == targetRectangle.Size || scalingMode == ScalingMode.NoScaling)
            {
                if (scalingMode != ScalingMode.NoScaling && !scalingMode.IsDefined())
                    throw new ArgumentOutOfRangeException(nameof(scalingMode), PublicResources.EnumOutOfRange(scalingMode));
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

            // Cloning source if target and source are the same and source/target rectangles overlap
            IBitmapDataInternal src = ReferenceEquals(source, target) && actualSourceRectangle.IntersectsWith(actualTargetRectangle)
                ? new BitmapDataBuffer(source)
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
                    : (Action <int>)ProcessRowStraight;

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

        #endregion

        #endregion
    }
}
