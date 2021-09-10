#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BitmapExtensions.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2021 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Security;

using KGySoft.CoreLibraries;
using KGySoft.Drawing.Imaging;
using KGySoft.Drawing.WinApi;

#endregion

#region Suppressions

#if NET35
#pragma warning disable CS1574 // XML comment has cref attribute that could not be resolved - in .NET 3.5 not all members are available
#endif

#endregion

namespace KGySoft.Drawing
{
    /// <summary>
    /// Provides extension methods for the <see cref="Bitmap"/> type.
    /// </summary>
    [SecuritySafeCritical] // for the SecuritySafeCritical methods containing lambdas
    public static class BitmapExtensions
    {
        #region Fields

        private static readonly int[] iconSizes = { 512, 384, 320, 256, 128, 96, 80, 72, 64, 60, 48, 40, 36, 32, 30, 24, 20, 16, 8, 4 };

        #endregion

        #region Methods

        #region Public Methods

        /// <summary>
        /// Resizes the image with high quality. The result always has a <see cref="PixelFormat.Format32bppPArgb"/> pixel format.
        /// <br/>This overload uses <see cref="Graphics.DrawImage(Image, Rectangle, Rectangle, GraphicsUnit)">Graphics.DrawImage</see> internally,
        /// which provides a good quality result but on Windows blocks every parallel <see cref="O:System.Drawing.Graphics.DrawImage">DrawImage</see> call within the same process.
        /// If that might be an issue use the <see cref="Resize(Bitmap, Size, ScalingMode, bool)"/> overload instead.
        /// </summary>
        /// <param name="image">The original image to resize</param>
        /// <param name="newSize">The requested new size.</param>
        /// <param name="keepAspectRatio"><see langword="true"/>&#160;to keep aspect ratio of the source <paramref name="image"/>; otherwise, <see langword="false"/>. This parameter is optional.
        /// <br/>Default value: <see langword="false"/>.</param>
        /// <returns>A <see cref="Bitmap"/> instance with the new size.</returns>
        public static Bitmap Resize(this Bitmap image, Size newSize, bool keepAspectRatio = false)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image), PublicResources.ArgumentNull);

            if (newSize.Width < 1 || newSize.Height < 1)
                throw new ArgumentOutOfRangeException(nameof(newSize), PublicResources.ArgumentOutOfRange);

            Size sourceSize = image.Size;
            Rectangle targetRectangle = keepAspectRatio && newSize != sourceSize
                ? GetTargetRectangleWithPreservedAspectRatio(newSize, sourceSize)
                : new Rectangle(Point.Empty, newSize);

            Bitmap result = new Bitmap(newSize.Width, newSize.Height, PixelFormat.Format32bppPArgb);
            if (OSUtils.IsWindows)
                result.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            Bitmap source = image;
            if (!source.PixelFormat.CanBeDrawn())
                source = image.ConvertPixelFormat(PixelFormat.Format32bppPArgb);

            try
            {
                using (Graphics g = Graphics.FromImage(result))
                {
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.DrawImage(source, targetRectangle, new Rectangle(Point.Empty, sourceSize), GraphicsUnit.Pixel);
                    g.Flush();

                    return result;
                }
            }
            finally
            {
                if (!ReferenceEquals(source, image))
                    source.Dispose();
            }
        }

        /// <summary>
        /// Resizes the image using the specified <paramref name="scalingMode"/>. The result always has a <see cref="PixelFormat.Format32bppPArgb"/> pixel format.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="image">The original image to resize</param>
        /// <param name="newSize">The requested new size.</param>
        /// <param name="scalingMode">A <see cref="ScalingMode"/> value, which determines the quality of the result as well as the processing time.</param>
        /// <param name="keepAspectRatio"><see langword="true"/>&#160;to keep aspect ratio of the source <paramref name="image"/>; otherwise, <see langword="false"/>. This parameter is optional.
        /// <br/>Default value: <see langword="false"/>.</param>
        /// <returns>A <see cref="Bitmap"/> instance with the new size.</returns>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress.
        /// Use the <see cref="BitmapDataExtensions.BeginDrawInto(IReadableBitmapData, IReadWriteBitmapData, Rectangle, Rectangle, IQuantizer, IDitherer, ScalingMode, AsyncConfig)">BitmapDataExtensions.BeginDrawInto</see>
        /// or <see cref="BitmapDataExtensions.DrawIntoAsync(IReadableBitmapData, IReadWriteBitmapData, Rectangle, Rectangle, IQuantizer, IDitherer, ScalingMode, TaskConfig)">BitmapDataExtensions.DrawIntoAsync</see>
        /// (in .NET 4.0 and above) methods for asynchronous call and to adjust parallelization, set up cancellation and for reporting progress.</note>
        /// <para>This method always produces a result with <see cref="PixelFormat.Format32bppPArgb"/>&#160;<see cref="PixelFormat"/>. To resize an image
        /// with a custom pixel format you can create a new <see cref="Bitmap"/> with the <see cref="Bitmap(int, int, PixelFormat)"/> constructor
        /// and use the <see cref="O:KGySoft.Drawing.ImageExtensions.DrawInto">DrawInto</see> extension methods.</para>
        /// <para>Generally, the best quality result can be achieved by the <see cref="Resize(Bitmap, Size, bool)"/> overload, which
        /// uses <see cref="Graphics.DrawImage(Image, Rectangle, Rectangle, GraphicsUnit)">Graphics.DrawImage</see> internally. On Windows some <see cref="Graphics"/>
        /// members use process-wide locks, which prevent them to be called concurrently without blocking. If that can be an issue you should use this overload.</para>
        /// </remarks>
        public static Bitmap Resize(this Bitmap image, Size newSize, ScalingMode scalingMode, bool keepAspectRatio = false)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image), PublicResources.ArgumentNull);
            if (newSize.Width < 1 || newSize.Height < 1)
                throw new ArgumentOutOfRangeException(nameof(newSize), PublicResources.ArgumentOutOfRange);
            if (!scalingMode.IsDefined())
                throw new ArgumentOutOfRangeException(nameof(scalingMode), PublicResources.EnumOutOfRange(scalingMode));

            Size sourceSize = image.Size;
            Rectangle targetRectangle = keepAspectRatio && newSize != sourceSize
                ? GetTargetRectangleWithPreservedAspectRatio(newSize, sourceSize)
                : new Rectangle(Point.Empty, newSize);

            Bitmap result = new Bitmap(newSize.Width, newSize.Height, PixelFormat.Format32bppPArgb);
            if (OSUtils.IsWindows)
                result.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (IReadableBitmapData src = image.GetReadableBitmapData())
            using (IReadWriteBitmapData dst = result.GetReadWriteBitmapData())
                src.DrawInto(dst, new Rectangle(Point.Empty, sourceSize), targetRectangle, null, null, scalingMode);

            return result;
        }

        /// <summary>
        /// When <paramref name="image"/> contains multiple pages, frames or multi-resolution sub-images, returns them as separated <see cref="Bitmap"/> instances.
        /// Otherwise, returns a new <see cref="Bitmap"/> with the copy of the original <paramref name="image"/>.
        /// </summary>
        /// <param name="image">An <see cref="Image"/> instance, which may contain multiple pages, frames or multi-resolution sub-images.</param>
        /// <returns>An array of <see cref="Bitmap"/> instances, which contains the images of the provided <paramref name="image"/>.</returns>
        public static Bitmap[] ExtractBitmaps(this Bitmap image)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image), PublicResources.ArgumentNull);

            // checking if image has multiple frames
            FrameDimension? dimension = null;
            Guid[] dimensions = image.FrameDimensionsList;
            if (dimensions.Length > 0)
            {
                if (dimensions[0] == FrameDimension.Page.Guid)
                    dimension = FrameDimension.Page;
                else if (dimensions[0] == FrameDimension.Time.Guid)
                    dimension = FrameDimension.Time;
                else if (dimensions[0] == FrameDimension.Resolution.Guid)
                    dimension = FrameDimension.Resolution;
            }

            int frameCount = dimension != null ? image.GetFrameCount(dimension) : 0;

            // single image, unknown dimension or one frame only: returning a copy
            if (frameCount <= 1 || dimension == null)
            {
                // Special handling for icons if it didn't have resolution dimensions
                if (image.RawFormat.Guid == ImageFormat.Icon.Guid)
                    return ExtractIconImages(image);

                return new Bitmap[] { image.CloneCurrentFrame() };
            }

            // extracting frames
            Bitmap[] result = new Bitmap[frameCount];
            for (int frame = 0; frame < frameCount; frame++)
            {
                image.SelectActiveFrame(dimension, frame);
                result[frame] = image.CloneCurrentFrame();
            }

            // selecting first frame again
            image.SelectActiveFrame(dimension, 0);

            return result;
        }

        /// <summary>
        /// Creates a clone of the current frame of the provided <see cref="Bitmap"/> instance. Unlike the <see cref="Bitmap(Image)"/> constructor, this method preserves original pixel format,
        /// and unlike <see cref="Bitmap.Clone(Rectangle,PixelFormat)">Bitmap.Clone(Rectangle,PixelFormat)</see> method, this method returns a single frame image.
        /// </summary>
        /// <param name="bitmap">The bitmap to be cloned.</param>
        /// <returns>A single frame <see cref="Bitmap"/> instance that has the same content and has the same pixel format as the current frame of the source bitmap.</returns>
        [SecuritySafeCritical]
        public static Bitmap CloneCurrentFrame(this Bitmap bitmap)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap), PublicResources.ArgumentNull);

            Bitmap result = new Bitmap(bitmap.Width, bitmap.Height, bitmap.PixelFormat);
            ColorPalette palette = bitmap.Palette;
            if (palette.Entries.Length > 0)
                result.Palette = palette;

            Rectangle rect = new Rectangle(Point.Empty, bitmap.Size);
            BitmapData sourceData = bitmap.LockBits(rect, ImageLockMode.ReadOnly, bitmap.PixelFormat);
            try
            {
                BitmapData targetData = result.LockBits(rect, ImageLockMode.WriteOnly, result.PixelFormat);
                try
                {
                    IntPtr lineSource = sourceData.Scan0;
                    IntPtr lineTarget = targetData.Scan0;
                    if (sourceData.Stride > 0)
                    {
                        MemoryHelper.CopyMemory(lineSource, lineTarget, sourceData.Stride * sourceData.Height);
                        return result;
                    }

                    int lineWidth = Math.Abs(sourceData.Stride);
                    for (int y = 0; y < sourceData.Height; y++)
                    {
                        MemoryHelper.CopyMemory(lineSource, lineTarget, lineWidth);
                        lineSource = new IntPtr(lineSource.ToInt64() + sourceData.Stride);
                        lineTarget = new IntPtr(lineTarget.ToInt64() + targetData.Stride);
                    }

                    return result;
                }
                finally
                {
                    result.UnlockBits(targetData);
                }
            }
            finally
            {
                bitmap.UnlockBits(sourceData);
            }
        }

        /// <summary>
        /// Gets the colors used in the defined <paramref name="bitmap"/>. A limit can be defined in <paramref name="maxColors"/>.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="bitmap">The bitmap, whose colors have to be returned. If it is indexed and the <paramref name="forceScanningContent"/> parameter is <see langword="false"/>,
        /// then its palette entries are returned and <paramref name="maxColors"/> is ignored.</param>
        /// <param name="maxColors">A limit of the returned colors. If <paramref name="forceScanningContent"/> parameter is <see langword="false"/>, then
        /// this parameter is ignored for indexed bitmaps. Use 0 for no limit. This parameter is optional.
        /// <br/>Default value: <c>0</c>.</param>
        /// <param name="forceScanningContent"><see langword="true"/>&#160;to force scanning the actual image content even if the specified <paramref name="bitmap"/> is
        /// indexed and has a palette. This parameter is optional.
        /// <br/>Default value: <see langword="false"/>.</param>
        /// <returns>An array of <see cref="Color"/> entries.</returns>
        /// <remarks>
        /// <note>This method blocks the caller, and does not support cancellation or reporting progress. Use the <see cref="BitmapDataExtensions.BeginGetColors">BitmapDataExtensions.BeginGetColors</see>
        /// or <see cref="BitmapDataExtensions.GetColorsAsync">BitmapDataExtensions.GetColorsAsync</see> (in .NET 4.0 and above) methods for asynchronous call and to set up cancellation or for reporting progress.</note>
        /// <para>Completely transparent pixels are considered the same regardless of their color information.</para>
        /// <para>Every <see cref="PixelFormat"/> is supported, though wide color formats (<see cref="PixelFormat.Format16bppGrayScale"/>, <see cref="PixelFormat.Format48bppRgb"/>,
        /// <see cref="PixelFormat.Format64bppArgb"/> and <see cref="PixelFormat.Format64bppPArgb"/>) are quantized to 32 bit during the processing.
        /// To get the actual <em>number</em> of colors, which is accurate even for wide color formats, use the <see cref="GetColorCount">GetColorCount</see> method.
        /// <note>For information about the possible usable <see cref="PixelFormat"/>s on different platforms see the <strong>Remarks</strong> section of the <see cref="ImageExtensions.ConvertPixelFormat(Image,PixelFormat,Color,byte)">ConvertPixelFormat</see> method.</note>
        /// </para>
        /// </remarks>
        public static Color[] GetColors(this Bitmap bitmap, int maxColors = 0, bool forceScanningContent = false)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap), PublicResources.ArgumentNull);

            if (bitmap.PixelFormat.IsIndexed() && !forceScanningContent)
                return bitmap.Palette.Entries;

            using var bitmapData = bitmap.GetReadableBitmapData();
            return bitmapData.GetColors(maxColors, forceScanningContent).Select(c => c.ToColor()).ToArray();
        }

        /// <summary>
        /// Gets the actual number of colors of the specified <paramref name="bitmap"/>. Colors are counted even for indexed bitmaps.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="bitmap">The bitmap, whose colors have to be counted to count its colors.</param>
        /// <returns>The actual number of colors of the specified <paramref name="bitmap"/>.</returns>
        /// <remarks>
        /// <note>This method blocks the caller, and does not support cancellation or reporting progress. Use the <see cref="BitmapDataExtensions.BeginGetColorCount">BitmapDataExtensions.BeginGetColorCount</see>
        /// or <see cref="BitmapDataExtensions.GetColorCountAsync">BitmapDataExtensions.GetColorCountAsync</see> (in .NET 4.0 and above) methods for asynchronous call and to set up cancellation or for reporting progress.</note>
        /// <para>Completely transparent pixels are considered the same regardless of their color information.</para>
        /// <para>Every <see cref="PixelFormat"/> is supported, but an accurate result is returned for wide color formats only
        /// when <see cref="IBitmapData.RowSize"/> is large enough to access all pixels directly (might not be the case for a clipped bitmap data, for example).
        /// Otherwise, colors are quantized to 32 bits-per-pixel values while counting them.
        /// Wide pixel formats are <see cref="PixelFormat.Format16bppGrayScale"/>, <see cref="PixelFormat.Format48bppRgb"/>, <see cref="PixelFormat.Format64bppArgb"/> and <see cref="PixelFormat.Format64bppPArgb"/>.
        /// <note>For information about the possible usable <see cref="PixelFormat"/>s on different platforms see the <strong>Remarks</strong> section of the <see cref="ImageExtensions.ConvertPixelFormat(Image,PixelFormat,Color,byte)">ConvertPixelFormat</see> method.</note>
        /// </para>
        /// </remarks>
        public static int GetColorCount(this Bitmap bitmap)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap), PublicResources.ArgumentNull);
            using var bitmapData = bitmap.GetReadableBitmapData();
            return bitmapData.GetColorCount();
        }

        /// <summary>
        /// Converts the provided <paramref name="bitmap"/> to a <see cref="CursorHandle"/>, which can be passed to the
        /// <a href="https://msdn.microsoft.com/en-us/library/system.windows.forms.cursor.aspx" target="_blank">System.Windows.Forms.Cursor</a> constructor
        /// to create a new cursor.
        /// </summary>
        /// <param name="bitmap">The <see cref="Bitmap"/>, which should be converted to a cursor.</param>
        /// <param name="cursorHotspot">The hotspot coordinates of the cursor. This parameter is optional.
        /// <br/>Default value: <see cref="Point.Empty">Point.Empty</see> (top-left corner)</param>
        /// <returns>A <see cref="CursorHandle"/> instance that can be used to create a <a href="https://msdn.microsoft.com/en-us/library/system.windows.forms.cursor.aspx" target="_blank">System.Windows.Forms.Cursor</a> instance.</returns>
        /// <remarks>
        /// <note>This method is supported on Windows only.</note>
        /// </remarks>
        /// <exception cref="PlatformNotSupportedException">This method is supported on Windows only.</exception>
        [SecuritySafeCritical]
        public static CursorHandle ToCursorHandle(this Bitmap bitmap, Point cursorHotspot = default)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap), PublicResources.ArgumentNull);
            if (!OSUtils.IsWindows)
                throw new PlatformNotSupportedException(Res.RequiresWindows);

            Bitmap source = bitmap;
            if (!bitmap.PixelFormat.CanBeDrawn())
                source = bitmap.ConvertPixelFormat(PixelFormat.Format32bppArgb);
            try
            {
                IntPtr iconHandle = source.GetHicon();
                try
                {
                    return Icons.ToCursorHandle(iconHandle, cursorHotspot);
                }
                finally
                {
                    User32.DestroyIcon(iconHandle);
                }
            }
            finally
            {
                if (!ReferenceEquals(source, bitmap))
                    source.Dispose();
            }
        }

        /// <summary>
        /// Gets an <see cref="IReadableBitmapData"/> instance, which provides fast read-only access to the actual data of the specified <paramref name="bitmap"/>.
        /// The <paramref name="bitmap"/> can have any <see cref="PixelFormat"/>.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="GetReadWriteBitmapData">GetReadWriteBitmapData</see> method for details and examples.
        /// </summary>
        /// <param name="bitmap">A <see cref="Bitmap"/> instance, whose data is about to be accessed.</param>
        /// <param name="backColor">For an <see cref="IReadableBitmapData"/> instance the <paramref name="backColor"/> is relevant only for indexed bitmaps
        /// when <see cref="Palette.GetNearestColorIndex">GetNearestColorIndex</see> and <see cref="Palette.GetNearestColor">GetNearestColor</see> methods
        /// are called with an alpha color on the <see cref="IBitmapData.Palette"/> property. Queried colors with alpha, which are considered opaque will be blended
        /// with this color before performing a lookup. The alpha value (<see cref="Color.A">Color.A</see> property) of the specified background color is ignored. This parameter is optional.
        /// <br/>Default value: <see cref="Color.Empty">Color.Empty</see>, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="alphaThreshold">Similarly to <paramref name="backColor"/>, for an <see cref="IReadableBitmapData"/> instance the <paramref name="alphaThreshold"/> is relevant
        /// only for indexed bitmaps when <see cref="Palette.GetNearestColorIndex">GetNearestColorIndex</see> and <see cref="Palette.GetNearestColor">GetNearestColor</see> methods
        /// are called with an alpha color on the <see cref="IBitmapData.Palette"/> property. In such case determines the lowest alpha value of a color,
        /// which should not be considered as transparent. If 0, then a color lookup will never return a transparent color. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <returns>An <see cref="IReadableBitmapData"/> instance, which provides fast read-only access to the actual data of the specified <paramref name="bitmap"/>.</returns>
        /// <seealso cref="GetWritableBitmapData"/>
        /// <seealso cref="GetReadWriteBitmapData"/>
        /// <seealso cref="BitmapDataFactory.CreateBitmapData(Size, PixelFormat, Color32, byte)"/>
        public static IReadableBitmapData GetReadableBitmapData(this Bitmap bitmap, Color backColor = default, byte alphaThreshold = 128)
            => BitmapDataFactory.CreateBitmapData(bitmap, ImageLockMode.ReadOnly, new Color32(backColor), alphaThreshold);

        /// <summary>
        /// Gets an <see cref="IWritableBitmapData"/> instance, which provides fast write-only access to the actual data of the specified <paramref name="bitmap"/>.
        /// The <paramref name="bitmap"/> can have any <see cref="PixelFormat"/>.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="GetReadWriteBitmapData">GetReadWriteBitmapData</see> method for details and examples.
        /// </summary>
        /// <param name="bitmap">A <see cref="Bitmap"/> instance, whose data is about to be accessed.</param>
        /// <param name="backColor">When setting pixels of indexed bitmaps and bitmaps without alpha support or with single bit alpha, then specifies the color of the background.
        /// Color values with alpha, which are considered opaque will be blended with this color before setting the pixel in the result bitmap data.
        /// The alpha value (<see cref="Color.A">Color.A</see> property) of the specified background color is ignored. This parameter is optional.
        /// <br/>Default value: <see cref="Color.Empty">Color.Empty</see>, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="alphaThreshold">When setting pixels of bitmaps with single bit alpha or with a palette that has a transparent color,
        /// then specifies a threshold value for the <see cref="Color.A">Color.A</see> property, under which the color is considered transparent. If 0,
        /// then the pixels to be set will never be transparent. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <returns>An <see cref="IWritableBitmapData"/> instance, which provides fast write-only access to the actual data of the specified <paramref name="bitmap"/>.</returns>
        /// <seealso cref="GetReadableBitmapData"/>
        /// <seealso cref="GetReadWriteBitmapData"/>
        /// <seealso cref="BitmapDataFactory.CreateBitmapData(Size, PixelFormat, Color32, byte)"/>
        public static IWritableBitmapData GetWritableBitmapData(this Bitmap bitmap, Color backColor = default, byte alphaThreshold = 128)
            => BitmapDataFactory.CreateBitmapData(bitmap, ImageLockMode.WriteOnly, new Color32(backColor), alphaThreshold);

        /// <summary>
        /// Gets an <see cref="IReadWriteBitmapData"/> instance, which provides fast read-write access to the actual data of the specified <paramref name="bitmap"/>.
        /// The <paramref name="bitmap"/> can have any <see cref="PixelFormat"/>.
        /// <br/>See the <strong>Remarks</strong> section for details and examples.
        /// </summary>
        /// <param name="bitmap">A <see cref="Bitmap"/> instance, whose data is about to be accessed.</param>
        /// <param name="backColor">When setting pixels of indexed bitmaps and bitmaps without alpha support or with single bit alpha, then specifies the color of the background.
        /// Color values with alpha, which are considered opaque will be blended with this color before setting the pixel in the result bitmap data.
        /// The alpha value (<see cref="Color.A">Color.A</see> property) of the specified background color is ignored. This parameter is optional.
        /// <br/>Default value: <see cref="Color.Empty">Color.Empty</see>, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="alphaThreshold">When setting pixels of bitmaps with single bit alpha or with a palette that has a transparent color,
        /// then specifies a threshold value for the <see cref="Color.A">Color.A</see> property, under which the color is considered transparent. If 0,
        /// then the pixels to be set will never be transparent. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance, which provides fast read-write access to the actual data of the specified <paramref name="bitmap"/>.</returns>
        /// <remarks>
        /// <para>All possible <see cref="PixelFormat"/>s are supported, of which a <see cref="Bitmap"/> can be created in the current operating system.
        /// <note>For information about the possible usable <see cref="PixelFormat"/>s on different platforms see the <strong>Remarks</strong> section of the <see cref="ImageExtensions.ConvertPixelFormat(Image,PixelFormat,Color,byte)">ConvertPixelFormat</see> method.</note>
        /// </para>
        /// <para>If <paramref name="alphaThreshold"/> is zero, then setting a fully transparent pixel in a bitmap with indexed or single-bit-alpha pixel format
        /// will blend the pixel to set with <paramref name="backColor"/> even if the bitmap can handle transparent pixels.</para>
        /// <para>If <paramref name="alphaThreshold"/> is <c>1</c>, then the result color of setting a pixel of a bitmap with indexed or single-bit-alpha pixel format
        /// will be transparent only if the color to set is completely transparent (has zero alpha).</para>
        /// <para>If <paramref name="alphaThreshold"/> is <c>255</c>, then the result color of setting a pixel of a bitmap with indexed or single-bit-alpha pixel format
        /// will be opaque only if the color to set is completely opaque (its alpha value is <c>255</c>).</para>
        /// <para>If a pixel of a bitmap without alpha gradient support is set by the <see cref="IWritableBitmapData.SetPixel">IWritableBitmapData.SetPixel</see>/<see cref="IWritableBitmapDataRow.SetColor">IWritableBitmapDataRow.SetColor</see>
        /// methods or by the <see cref="IReadWriteBitmapDataRow.this">IReadWriteBitmapDataRow indexer</see>, and the pixel has an alpha value that is greater than <paramref name="alphaThreshold"/>,
        /// then the pixel to set will be blended with <paramref name="backColor"/>.</para>
        /// <note type="tip">To create a managed <see cref="IReadWriteBitmapData"/> instance that supports every <see cref="PixelFormat"/>s on any platform
        /// you can use the <see cref="BitmapDataFactory.CreateBitmapData(Size, PixelFormat, Color32, byte)">BitmapDataFactory.CreateBitmapData</see> method.</note>
        /// </remarks>
        /// <example>
        /// <para>The following example demonstrates how easily you can copy the content of a 32-bit ARGB image into an 8-bit indexed one by
        /// using the <see cref="GetReadableBitmapData">GetReadableBitmapData</see> and <see cref="GetWritableBitmapData">GetWritableBitmapData</see> methods:</para>
        /// <code lang="C#"><![CDATA[
        /// var targetFormat = PixelFormat.Format8bppIndexed; // feel free to try other formats as well
        /// using (Bitmap bmpSrc = Icons.Shield.ExtractBitmap(new Size(256, 256)))
        /// using (Bitmap bmpDst = new Bitmap(256, 256, targetFormat))
        /// {
        ///     using (IReadableBitmapData dataSrc = bmpSrc.GetReadableBitmapData())
        ///     using (IWritableBitmapData dataDst = bmpDst.GetWritableBitmapData())
        ///     {
        ///         for (int y = 0; y < dataSrc.Height; y++)
        ///         {
        ///             for (int x = 0; x < dataSrc.Width; x++)
        ///             {
        ///                 // Please note that bmpDst.SetPixel would not work for indexed formats
        ///                 // and even when it can be used it would be much slower.
        ///                 dataDst.SetPixel(x, y, dataSrc.GetPixel(x, y));
        ///             }
        ///         }
        ///     }
        ///
        ///     bmpSrc.SaveAsPng(@"c:\temp\bmpSrc.png");
        ///     bmpDst.SaveAsPng(@"c:\temp\bmpDst.png"); // or saveAsGif/SaveAsTiff to preserve the indexed format
        /// }]]></code>
        /// <para>The example above produces the following results:
        /// <list type="table">
        /// <item><term><c>bmpSrc.png</c></term><term><img src="../Help/Images/Shield256.png" alt="Shield icon with transparent background"/></term></item>
        /// <item><term><c>bmpDst.png</c></term><term><img src="../Help/Images/ShieldDefault8bppBlack.gif" alt="8 BPP shield icon with system default palette"/></term></item>
        /// </list></para>
        /// <para>If the pixels are not accessed randomly, then the sequential enumeration of rows can be a bit faster:</para>
        /// <code lang="C#"><![CDATA[
        /// // Replace the body of the inner using block of the previous example with the following code:
        /// IReadableBitmapDataRow rowSrc = dataSrc.FirstRow;
        /// IWritableBitmapDataRow rowDst = dataDst.FirstRow;
        /// do
        /// {
        ///     for (int x = 0; x < dataSrc.Width; x++)
        ///         rowDst[x] = rowSrc[x];
        /// } while (rowSrc.MoveNextRow() && rowDst.MoveNextRow());]]></code>
        /// <para>For parallel processing you can retrieve multiple rows by the indexer and process them concurrently.
        /// When targeting .NET Framework 4.0 or newer, the example above can be easily re-written to use parallel processing:</para>
        /// <code lang="C#"><![CDATA[
        /// // The parallel version of same body as in the previous example:
        /// Parallel.For(0, dataSrc.Height, y =>
        /// {
        ///     IReadableBitmapDataRow rowSrc = dataSrc[y];
        ///     IWritableBitmapDataRow rowDst = dataDst[y];
        ///     for (int x = 0; x < dataSrc.Width; x++)
        ///         rowDst[x] = rowSrc[x];
        /// });]]></code>
        /// <note>The examples above are just for demonstration purpose. For the same result use the <see cref="O:KGySoft.Drawing.ImageExtensions.ConvertPixelFormat">ConvertPixelFormat</see>
        /// methods for more flexibility and optimized parallel processing. The <see cref="ImageExtensions.ConvertPixelFormat(Image, PixelFormat, IQuantizer, IDitherer)"/> overload allows using custom quantizer and ditherer implementations as well.</note>
        /// <para>The following example demonstrates how to use the read-write <see cref="IReadWriteBitmapData"/> returned by the <see cref="GetReadWriteBitmapData">GetReadWriteBitmapData</see> method
        /// to manipulate a <see cref="Bitmap"/> in-place:</para>
        /// <code lang="C#"><![CDATA[
        /// // This example produces the same result as the MakeGrayscale extension method without a ditherer:
        /// using (Bitmap bmp = Icons.Shield.ExtractBitmap(new Size(256, 256)))
        /// {
        ///     bmp.SaveAsPng(@"c:\temp\before.png");
        ///
        ///     using (IReadWriteBitmapData bmpData = bmp.GetReadWriteBitmapData())
        ///     {
        ///         IReadWriteBitmapDataRow row = bmpData.FirstRow;
        ///         do
        ///         {
        ///             for (int x = 0; x < bmpData.Width; x++)
        ///                 row[x] = row[x].ToGray();
        ///         } while (row.MoveNextRow());
        ///     }
        ///
        ///     bmp.SaveAsPng(@"c:\temp\after.png");
        /// }]]></code>
        /// <para>The example above produces the following results:
        /// <list type="table">
        /// <item><term><c>before.png</c></term><term><img src="../Help/Images/Shield256.png" alt="Color shield icon"/></term></item>
        /// <item><term><c>after.png</c></term><term><img src="../Help/Images/ShieldGrayscale.png" alt="Grayscale shield icon"/></term></item>
        /// </list></para>
        /// </example>
        /// <seealso cref="GetReadableBitmapData"/>
        /// <seealso cref="GetWritableBitmapData"/>
        /// <seealso cref="BitmapDataFactory.CreateBitmapData(Size, PixelFormat, Color32, byte)"/>
        public static IReadWriteBitmapData GetReadWriteBitmapData(this Bitmap bitmap, Color backColor = default, byte alphaThreshold = 128)
            => BitmapDataFactory.CreateBitmapData(bitmap, ImageLockMode.ReadWrite, new Color32(backColor), alphaThreshold);

        /// <summary>
        /// Quantizes a <paramref name="bitmap"/> using the specified <paramref name="quantizer"/> (reduces the number of colors).
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="bitmap">A <see cref="Bitmap"/> instance to be quantized.</param>
        /// <param name="quantizer">An <see cref="IQuantizer"/> implementation to be used for quantizing the specified <paramref name="bitmap"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="bitmap"/> or <paramref name="quantizer"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException">The <paramref name="quantizer"/>'s <see cref="IQuantizer.Initialize">Initialize</see> method returned <see langword="null"/>.</exception>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. Use the <see cref="BitmapDataExtensions.BeginQuantize">BitmapDataExtensions.BeginQuantize</see>
        /// or <see cref="BitmapDataExtensions.QuantizeAsync">BitmapDataExtensions.QuantizeAsync</see> (in .NET 4.0 and above) methods for asynchronous call and to adjust parallelization, set up cancellation and for reporting progress.</note>
        /// <para>This method quantizes <paramref name="bitmap"/> in place (its original content will be overwritten). To return a new instance
        /// use the <see cref="ImageExtensions.ConvertPixelFormat(Image,PixelFormat,IQuantizer,IDitherer)">ConvertPixelFormat</see> extension method instead.</para>
        /// <para>If the <see cref="PixelFormat"/> or the palette of <paramref name="bitmap"/> is not compatible with the colors of the specified <paramref name="quantizer"/>, then
        /// the result may not be correct.</para>
        /// <para>If <paramref name="bitmap"/> has already the same set of colors that the specified <paramref name="quantizer"/>, then it can happen
        /// that calling this method does not change <paramref name="bitmap"/> at all.</para>
        /// <note type="tip"><list type="bullet">
        /// <item>To use predefined colors or custom quantization functions use the static methods of the <see cref="PredefinedColorsQuantizer"/> class.
        /// <br/>See the <strong>Remarks</strong> section of its members for details and examples.</item>
        /// <item>To use an optimized palette of up to 256 colors adjusted for <paramref name="bitmap"/> see the <see cref="OptimizedPaletteQuantizer"/> class.</item>
        /// </list></note>
        /// </remarks>
        /// <seealso cref="BitmapDataExtensions.Quantize"/>
        public static void Quantize(this Bitmap bitmap, IQuantizer quantizer)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap), PublicResources.ArgumentNull);
            if (quantizer == null)
                throw new ArgumentNullException(nameof(quantizer), PublicResources.ArgumentNull);

            using (IReadWriteBitmapData bitmapData = bitmap.GetReadWriteBitmapData())
                bitmapData.Quantize(quantizer);
        }

        /// <summary>
        /// Quantizes a <paramref name="bitmap"/> with dithering (reduces the number of colors while trying to preserve details)
        /// using the specified <paramref name="quantizer"/> and <paramref name="ditherer"/>.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="bitmap">A <see cref="Bitmap"/> instance to be quantized.</param>
        /// <param name="quantizer">An <see cref="IQuantizer"/> implementation to be used for quantizing the specified <paramref name="bitmap"/>.</param>
        /// <param name="ditherer">An <see cref="IDitherer"/> implementation to be used for dithering during the quantization of the specified <paramref name="bitmap"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="bitmap"/>, <paramref name="quantizer"/> or <paramref name="ditherer"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException">The <see cref="IQuantizer.Initialize">IQuantizer.Initialize</see> method
        /// or the <see cref="IDitherer.Initialize">IDitherer.Initialize</see> method returned <see langword="null"/>.</exception>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. Use the <see cref="BitmapDataExtensions.BeginDither">BitmapDataExtensions.BeginDither</see>
        /// or <see cref="BitmapDataExtensions.DitherAsync">BitmapDataExtensions.DitherAsync</see> (in .NET 4.0 and above) methods for asynchronous call and to adjust parallelization, set up cancellation and for reporting progress.</note>
        /// <para>This method quantizes <paramref name="bitmap"/> with dithering in place (its original content will be overwritten). To return a new instance
        /// use the <see cref="ImageExtensions.ConvertPixelFormat(Image,PixelFormat,IQuantizer,IDitherer)">ConvertPixelFormat</see> extension method instead.</para>
        /// <para>If the <see cref="PixelFormat"/> or the palette of <paramref name="bitmap"/> is not compatible with the colors of the specified <paramref name="quantizer"/>, then
        /// the result may not be correct.</para>
        /// <para>If <paramref name="bitmap"/> has already the same set of colors that the specified <paramref name="quantizer"/>, then it can happen
        /// that calling this method does not change <paramref name="bitmap"/> at all.</para>
        /// <note type="tip"><list type="bullet">
        /// <item>To use predefined colors or custom quantization functions use the static methods of the <see cref="PredefinedColorsQuantizer"/> class.
        /// <br/>See the <strong>Remarks</strong> section of its members for details and examples.</item>
        /// <item>To use an optimized palette of up to 256 colors adjusted for <paramref name="bitmap"/> see the <see cref="OptimizedPaletteQuantizer"/> class.</item>
        /// <item>For some built-in dithering solutions see the <see cref="OrderedDitherer"/>, <see cref="ErrorDiffusionDitherer"/>, <see cref="RandomNoiseDitherer"/>
        /// and <see cref="InterleavedGradientNoiseDitherer"/> classes. All of them have several examples in their <strong>Remarks</strong> section.</item>
        /// </list></note>
        /// </remarks>
        /// <seealso cref="BitmapDataExtensions.Dither"/>
        public static void Dither(this Bitmap bitmap, IQuantizer quantizer, IDitherer ditherer)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap), PublicResources.ArgumentNull);
            if (quantizer == null)
                throw new ArgumentNullException(nameof(quantizer), PublicResources.ArgumentNull);
            if (ditherer == null)
                throw new ArgumentNullException(nameof(ditherer), PublicResources.ArgumentNull);

            using (var bitmapData = bitmap.GetReadWriteBitmapData())
                bitmapData.Dither(quantizer, ditherer);
        }

        /// <summary>
        /// Clears the complete <paramref name="bitmap"/> and fills it with the specified <paramref name="color"/>.
        /// <br/>This method is similar to <see cref="Graphics.Clear">Graphics.Clear</see> but can be used for <see cref="Bitmap"/>s of any <see cref="PixelFormat"/>.
        /// </summary>
        /// <param name="bitmap">The <see cref="Bitmap"/> to be cleared.</param>
        /// <param name="color">A <see cref="Color"/> that represents the desired result color of the bitmap.</param>
        /// <param name="backColor">If <paramref name="bitmap"/> cannot have alpha or has only single-bit alpha, and <paramref name="color"/> is not fully opaque, then specifies the color of the background.
        /// If <paramref name="color"/> has alpha and it is considered opaque, then it will be blended with <paramref name="backColor"/> to determine the color of the cleared <paramref name="bitmap"/>.
        /// The <see cref="Color.A">Color.A</see> property of the background color is ignored. This parameter is optional.
        /// <br/>Default value: <see cref="Color.Empty"/>, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="alphaThreshold">If <paramref name="bitmap"/> has only single-bit alpha or its palette contains a transparent color,
        /// then specifies a threshold value for the <see cref="Color.A">Color.A</see> property, under which the specified <paramref name="color"/> is considered transparent. If 0,
        /// then the cleared <paramref name="bitmap"/> will not be transparent. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. Use the <see cref="BitmapDataExtensions.BeginClear">BitmapDataExtensions.BeginClear</see>
        /// or <see cref="BitmapDataExtensions.ClearAsync">BitmapDataExtensions.ClearAsync</see> (in .NET 4.0 and above) methods for asynchronous call and to adjust parallelization, set up cancellation and for reporting progress.</note>
        /// </remarks>
        /// <seealso cref="BitmapDataExtensions.Clear(IWritableBitmapData, Color32, IDitherer)"/>
        public static void Clear(this Bitmap bitmap, Color color, Color backColor = default, byte alphaThreshold = 128)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap), PublicResources.ArgumentNull);
            using (IBitmapDataInternal accessor = BitmapDataFactory.CreateBitmapData(bitmap, ImageLockMode.ReadWrite, new Color32(backColor), alphaThreshold))
                accessor.Clear(new Color32(color));
        }

        /// <summary>
        /// Clears the complete <paramref name="bitmap"/> and fills it with the specified <paramref name="color"/>.
        /// <br/>This method is similar to <see cref="Graphics.Clear">Graphics.Clear</see> but can be used for <see cref="Bitmap"/>s of any <see cref="PixelFormat"/>.
        /// </summary>
        /// <param name="bitmap">The <see cref="Bitmap"/> to be cleared.</param>
        /// <param name="color">A <see cref="Color"/> that represents the desired result color of the bitmap.</param>
        /// <param name="backColor">If <paramref name="bitmap"/> cannot have alpha or has only single-bit alpha, and <paramref name="color"/> is not fully opaque, then specifies the color of the background.
        /// If <paramref name="color"/> has alpha and it is considered opaque, then it will be blended with <paramref name="backColor"/> to determine the color of the cleared <paramref name="bitmap"/>.
        /// The <see cref="Color.A">Color.A</see> property of the background color is ignored. This parameter is optional.
        /// <br/>Default value: <see cref="Color.Empty"/>, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="alphaThreshold">If <paramref name="bitmap"/> has only single-bit alpha or its palette contains a transparent color,
        /// then specifies a threshold value for the <see cref="Color.A">Color.A</see> property, under which the specified <paramref name="color"/> is considered transparent. If 0,
        /// then the cleared <paramref name="bitmap"/> will not be transparent. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <param name="ditherer">The ditherer to be used for the clearing. Has no effect if <paramref name="bitmap"/>&#160;<see cref="PixelFormat"/> has at least 24 bits-per-pixel size.
        /// If <see langword="null"/>, then the <see cref="Clear(Bitmap,Color,Color,byte)"/> overload will be called.</param>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. Use the <see cref="BitmapDataExtensions.BeginClear">BitmapDataExtensions.BeginClear</see>
        /// or <see cref="BitmapDataExtensions.ClearAsync">BitmapDataExtensions.ClearAsync</see> (in .NET 4.0 and above) methods for asynchronous call and to adjust parallelization, set up cancellation and for reporting progress.</note>
        /// </remarks>
        /// <seealso cref="BitmapDataExtensions.Clear(IWritableBitmapData, Color32, IDitherer)"/>
        public static void Clear(this Bitmap bitmap, Color color, IDitherer? ditherer, Color backColor = default, byte alphaThreshold = 128)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap), PublicResources.ArgumentNull);

            using (IBitmapDataInternal accessor = BitmapDataFactory.CreateBitmapData(bitmap, ImageLockMode.ReadWrite, new Color32(backColor), alphaThreshold))
                accessor.Clear(new Color32(color), ditherer);
        }

        /// <summary>
        /// Transforms the colors of a <paramref name="bitmap"/> using the specified <paramref name="transformFunction"/> delegate.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="bitmap">The <see cref="Bitmap"/> to be transformed.</param>
        /// <param name="transformFunction">The transform function to be used on the colors of the specified <paramref name="bitmap"/>. It must be thread-safe.</param>
        /// <param name="backColor">If <paramref name="transformFunction"/> returns colors with alpha and <paramref name="bitmap"/> has no alpha or supports single bit alpha only,
        /// then specifies the color of the background. Color values with alpha, which are considered opaque will be blended with this color before setting the pixel in the specified <paramref name="bitmap"/>.
        /// The alpha value (<see cref="Color.A">Color.A</see> property) of the specified background color is ignored. This parameter is optional.
        /// <br/>Default value: <see cref="Color.Empty">Color.Empty</see>, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="alphaThreshold">If <paramref name="transformFunction"/> returns colors with alpha and <paramref name="bitmap"/> supports single bit alpha only,
        /// then specifies a threshold value for the <see cref="Color32.A">Color32.A</see> field, under which the color is considered transparent. If 0,
        /// then the pixels to be set will never be transparent. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="bitmap"/> or <paramref name="transformFunction"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. Use the <see cref="BitmapDataExtensions.BeginTransformColors">BitmapDataExtensions.BeginTransformColors</see>
        /// or <see cref="BitmapDataExtensions.TransformColorsAsync">BitmapDataExtensions.TransformColorsAsync</see> (in .NET 4.0 and above) methods for asynchronous call and to adjust parallelization, set up cancellation and for reporting progress.</note>
        /// <para>This method transforms the <paramref name="bitmap"/> in place (its original content will be overwritten). To return a new instance
        /// use the <see cref="ImageExtensions.ConvertPixelFormat(Image,PixelFormat,IQuantizer,IDitherer)">ConvertPixelFormat</see> extension method
        /// with an <see cref="IQuantizer"/> instance created by the <see cref="PredefinedColorsQuantizer.FromCustomFunction(Func{Color32,Color32},PixelFormat)">PredefinedColorsQuantizer.FromCustomFunction</see> method.</para>
        /// <para>If <paramref name="bitmap"/> has an indexed <see cref="PixelFormat"/>, then its palette entries will be transformed instead of the actual pixels.</para>
        /// <para>On multi-core systems <paramref name="transformFunction"/> might be called concurrently so it must be thread-safe.</para>
        /// <note type="tip">If <paramref name="transformFunction"/> can return colors incompatible with the pixel format of the specified <paramref name="bitmap"/>, or you want to transform the actual
        /// pixels of an indexed <see cref="Bitmap"/> instead of modifying the palette, then use the <see cref="TransformColors(Bitmap,Func{Color32,Color32},IDitherer,Color,byte)"/> overload and specify an <see cref="IDitherer"/> instance.</note>
        /// </remarks>
        /// <seealso cref="BitmapDataExtensions.TransformColors(IReadWriteBitmapData, Func{Color32, Color32})"/>
        [SecuritySafeCritical]
        public static void TransformColors(this Bitmap bitmap, Func<Color32, Color32> transformFunction, Color backColor = default, byte alphaThreshold = 128)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap), PublicResources.ArgumentNull);
            if (transformFunction == null)
                throw new ArgumentNullException(nameof(transformFunction), PublicResources.ArgumentNull);

            using IReadWriteBitmapData bitmapData = bitmap.GetReadWriteBitmapData(backColor, alphaThreshold);
            bitmapData.TransformColors(transformFunction);
        }

        /// <summary>
        /// Transforms the colors of a <paramref name="bitmap"/> using the specified <paramref name="transformFunction"/> delegate.
        /// <br/>See the <strong>Remarks</strong> section for details and an example.
        /// </summary>
        /// <param name="bitmap">The <see cref="Bitmap"/> to be transformed.</param>
        /// <param name="transformFunction">The transform function to be used on the colors of the specified <paramref name="bitmap"/>. It must be thread-safe.</param>
        /// <param name="ditherer">An optional <see cref="IDitherer"/> instance to dither the result of the transformation if <paramref name="transformFunction"/> returns colors
        /// that is not compatible with the <see cref="PixelFormat"/> of the specified <paramref name="bitmap"/>.</param>
        /// <param name="backColor">If <paramref name="transformFunction"/> returns colors with alpha and <paramref name="bitmap"/> has no alpha or supports single bit alpha only,
        /// then specifies the color of the background. Color values with alpha, which are considered opaque will be blended with this color before setting the pixel in the specified <paramref name="bitmap"/>.
        /// The alpha value (<see cref="Color.A">Color.A</see> property) of the specified background color is ignored. This parameter is optional.
        /// <br/>Default value: <see cref="Color.Empty">Color.Empty</see>, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="alphaThreshold">If <paramref name="transformFunction"/> returns colors with alpha and <paramref name="bitmap"/> supports single bit alpha only,
        /// then specifies a threshold value for the <see cref="Color32.A">Color32.A</see> field, under which the color is considered transparent. If 0,
        /// then the pixels to be set will never be transparent. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="bitmap"/> or <paramref name="transformFunction"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. Use the <see cref="BitmapDataExtensions.BeginTransformColors">BitmapDataExtensions.BeginTransformColors</see>
        /// or <see cref="BitmapDataExtensions.TransformColorsAsync">BitmapDataExtensions.TransformColorsAsync</see> (in .NET 4.0 and above) methods for asynchronous call and to adjust parallelization, set up cancellation and for reporting progress.</note>
        /// <para>This method transforms the <paramref name="bitmap"/> in place (its original content will be overwritten). To return a new instance
        /// use the <see cref="ImageExtensions.ConvertPixelFormat(Image,PixelFormat,IQuantizer,IDitherer)">ConvertPixelFormat</see> extension method
        /// with an <see cref="IQuantizer"/> instance created by the <see cref="PredefinedColorsQuantizer.FromCustomFunction(Func{Color32,Color32},PixelFormat)">PredefinedColorsQuantizer.FromCustomFunction</see> method.</para>
        /// <para>If <paramref name="bitmap"/> has an indexed <see cref="PixelFormat"/> and <paramref name="ditherer"/> is <see langword="null"/>,
        /// then its palette entries will be transformed instead of the actual pixels. To transform the colors of an indexed <see cref="Bitmap"/> without changing the palette
        /// specify a non-<see langword="null"/>&#160;<paramref name="ditherer"/>. Transforming the palette is both faster and provides a better result.</para>
        /// <para>On multi-core systems <paramref name="transformFunction"/> might be called concurrently so it must be thread-safe.</para>
        /// <para>The <paramref name="ditherer"/> is ignored for <see cref="PixelFormat"/>s with more than 16 bits-per-pixel and for the <see cref="PixelFormat.Format16bppGrayScale"/> format.</para>
        /// </remarks>
        /// <example>
        /// The following example demonstrates how to use this method:
        /// <code lang="C#"><![CDATA[
        /// using Bitmap original = Icons.Shield.ExtractBitmap(new Size(256, 256));
        ///
        /// // starting with an indexed image using an optimized 8 BPP palette
        /// using Bitmap bmp = original.ConvertPixelFormat(PixelFormat.Format8bppIndexed,
        ///     OptimizedPaletteQuantizer.MedianCut());
        /// 
        /// bmp.SaveAsGif(@"c:\temp\before.gif");
        ///
        /// // Transforming colors to grayscale. By specifying a ditherer the original palette is preserved
        /// // (which is not so optimal for the transformed image anymore). The ditherer tries
        /// // to approximate the desired result with the original palette as much as possible.
        /// // Try it also without a ditherer to transform only the palette entries.
        /// bmp.TransformColors(c => c.ToGray(),
        ///     ErrorDiffusionDitherer.FloydSteinberg.ConfigureErrorDiffusionMode(byBrightness: true));
        ///
        /// // As ditherer was not null now the result is generated using the original palette
        /// bmp.SaveAsGif(@"c:\temp\after.gif");]]></code>
        /// <para>The example above produces the following results:
        /// <list type="table">
        /// <item><term><c>before.gif</c></term><term><img src="../Help/Images/ShieldMedianCut256Black.gif" alt="Shield icon quantized to 256 colors using the Median Cut algorithm"/></term></item>
        /// <item><term><c>after.gif</c></term><term><img src="../Help/Images/ShieldMedianCut256TrGrayDitheredFS.gif" alt="Shield icon transformed to grayscale with Floyd-Steinberg dithering while still using an optimized palette for the colored version"/></term></item>
        /// </list></para>
        /// </example>
        /// <seealso cref="BitmapDataExtensions.TransformColors(IReadWriteBitmapData, Func{Color32, Color32}, IDitherer)"/>
        public static void TransformColors(this Bitmap bitmap, Func<Color32, Color32> transformFunction, IDitherer? ditherer, Color backColor = default, byte alphaThreshold = 128)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap), PublicResources.ArgumentNull);

            using IReadWriteBitmapData bitmapData = bitmap.GetReadWriteBitmapData(backColor, alphaThreshold);
            bitmapData.TransformColors(transformFunction, ditherer);
        }

        /// <summary>
        /// Replaces every <paramref name="oldColor"/> occurrences to <paramref name="newColor"/> in the specified <paramref name="bitmap"/>.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="bitmap">The <see cref="Bitmap"/> to be transformed.</param>
        /// <param name="oldColor">The original color to be replaced.</param>
        /// <param name="newColor">The new color to replace <paramref name="oldColor"/> with.</param>
        /// <param name="ditherer">An optional <see cref="IDitherer"/> instance to dither the result if <paramref name="newColor"/>
        /// is not compatible with the <see cref="PixelFormat"/> of the specified <paramref name="bitmap"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. Use the <see cref="BitmapDataExtensions.BeginReplaceColor">BitmapDataExtensions.BeginReplaceColor</see>
        /// or <see cref="BitmapDataExtensions.ReplaceColorAsync">BitmapDataExtensions.ReplaceColorAsync</see> (in .NET 4.0 and above) methods for asynchronous call and to adjust parallelization, set up cancellation and for reporting progress.</note>
        /// <para>If <paramref name="newColor"/> has alpha, which cannot be represented by <paramref name="bitmap"/>, then it will be blended with black.
        /// Call <see cref="TransformColors(Bitmap,Func{Color32,Color32},IDitherer,Color,byte)">TransformColors</see> to use a custom background color instead.</para>
        /// <para>If <paramref name="bitmap"/> has an indexed <see cref="PixelFormat"/> and <paramref name="ditherer"/> is <see langword="null"/>,
        /// then its palette entries will be transformed instead of the actual pixels. To transform the colors of an indexed <see cref="Bitmap"/> without changing the palette
        /// specify a non-<see langword="null"/>&#160;<paramref name="ditherer"/>. Transforming the palette is both faster and provides a better result.</para>
        /// <para>The <paramref name="ditherer"/> is ignored for <see cref="PixelFormat"/>s with more than 16 bits-per-pixel and for the <see cref="PixelFormat.Format16bppGrayScale"/> format.</para>
        /// </remarks>
        /// <seealso cref="BitmapDataExtensions.ReplaceColor"/>
        public static void ReplaceColor(this Bitmap bitmap, Color oldColor, Color newColor, IDitherer? ditherer = null)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap), PublicResources.ArgumentNull);

            using IReadWriteBitmapData bitmapData = bitmap.GetReadWriteBitmapData();
            bitmapData.ReplaceColor(new Color32(oldColor), new Color32(newColor), ditherer);
        }

        /// <summary>
        /// Inverts the colors of the specified <paramref name="bitmap"/>.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="bitmap">The <see cref="Bitmap"/> to be inverted.</param>
        /// <param name="ditherer">An optional <see cref="IDitherer"/> instance to dither the result of the transformation if the inverse of the <paramref name="bitmap"/>
        /// has no exact representation with its <see cref="PixelFormat"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. Use the <see cref="BitmapDataExtensions.BeginInvert">BitmapDataExtensions.BeginInvert</see>
        /// or <see cref="BitmapDataExtensions.InvertAsync">BitmapDataExtensions.InvertAsync</see> (in .NET 4.0 and above) methods for asynchronous call and to adjust parallelization, set up cancellation and for reporting progress.</note>
        /// <para>If <paramref name="bitmap"/> has an indexed <see cref="PixelFormat"/> and <paramref name="ditherer"/> is <see langword="null"/>,
        /// then its palette entries will be transformed instead of the actual pixels. To transform the colors of an indexed <see cref="Bitmap"/> without changing the palette
        /// specify a non-<see langword="null"/>&#160;<paramref name="ditherer"/>. Transforming the palette is both faster and provides a better result.</para>
        /// <para>The <paramref name="ditherer"/> is ignored for <see cref="PixelFormat"/>s with more than 16 bits-per-pixel and for the <see cref="PixelFormat.Format16bppGrayScale"/> format.</para>
        /// </remarks>
        /// <seealso cref="BitmapDataExtensions.Invert"/>
        public static void Invert(this Bitmap bitmap, IDitherer? ditherer = null)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap), PublicResources.ArgumentNull);

            using IReadWriteBitmapData bitmapData = bitmap.GetReadWriteBitmapData();
            bitmapData.Invert(ditherer);
        }

        /// <summary>
        /// Makes a <paramref name="bitmap"/> opaque using the specified <paramref name="backColor"/>.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="bitmap">The <see cref="Bitmap"/> to make opaque.</param>
        /// <param name="backColor">Pixels with alpha in <paramref name="bitmap"/> will be blended with this color.
        /// The <see cref="Color.A">Color.A</see> property of the specified color is ignored.</param>
        /// <param name="ditherer">An optional <see cref="IDitherer"/> instance to dither the result if <paramref name="backColor"/>
        /// is not compatible with the <see cref="PixelFormat"/> of the specified <paramref name="bitmap"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. Use the <see cref="BitmapDataExtensions.BeginMakeOpaque">BitmapDataExtensions.BeginMakeOpaque</see>
        /// or <see cref="BitmapDataExtensions.MakeOpaqueAsync">BitmapDataExtensions.MakeOpaqueAsync</see> (in .NET 4.0 and above) methods for asynchronous call and to adjust parallelization, set up cancellation and for reporting progress.</note>
        /// <para>If <paramref name="bitmap"/> has an indexed <see cref="PixelFormat"/> and <paramref name="ditherer"/> is <see langword="null"/>,
        /// then its palette entries will be transformed instead of the actual pixels. To transform the colors of an indexed <see cref="Bitmap"/> without changing the palette
        /// specify a non-<see langword="null"/>&#160;<paramref name="ditherer"/>. Transforming the palette is both faster and provides a better result.</para>
        /// <para>The <paramref name="ditherer"/> is ignored for <see cref="PixelFormat"/>s with more than 16 bits-per-pixel and for the <see cref="PixelFormat.Format16bppGrayScale"/> format.</para>
        /// </remarks>
        /// <seealso cref="BitmapDataExtensions.MakeOpaque"/>
        public static void MakeOpaque(this Bitmap bitmap, Color backColor, IDitherer? ditherer = null)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap), PublicResources.ArgumentNull);

            using IReadWriteBitmapData bitmapData = bitmap.GetReadWriteBitmapData();
            bitmapData.MakeOpaque(new Color32(backColor), ditherer);
        }

        /// <summary>
        /// Makes a <paramref name="bitmap"/> grayscale.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="bitmap">The <see cref="Bitmap"/> to make grayscale.</param>
        /// <param name="ditherer">An optional <see cref="IDitherer"/> instance to dither the result if grayscale colors
        /// cannot be represented by the <see cref="PixelFormat"/> or the current palette of the specified <paramref name="bitmap"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. Use the <see cref="BitmapDataExtensions.BeginMakeGrayscale">BitmapDataExtensions.BeginMakeGrayscale</see>
        /// or <see cref="BitmapDataExtensions.MakeGrayscaleAsync">BitmapDataExtensions.MakeGrayscaleAsync</see> (in .NET 4.0 and above) methods for asynchronous call and to adjust parallelization, set up cancellation and for reporting progress.</note>
        /// <para>This method transforms the <paramref name="bitmap"/> in place (its original content will be overwritten). To return a new instance
        /// use the <see cref="ImageExtensions.ToGrayscale">ToGrayscale</see> extension method, which always returns a bitmap with <see cref="PixelFormat.Format32bppArgb"/> format,
        /// or the <see cref="ImageExtensions.ConvertPixelFormat(Image, PixelFormat, IQuantizer, IDitherer)">ConvertPixelFormat</see> method with a grayscale
        /// quantizer (<see cref="PredefinedColorsQuantizer.Grayscale">PredefinedColorsQuantizer.Grayscale</see>, for example).</para>
        /// <para>If <paramref name="bitmap"/> has an indexed <see cref="PixelFormat"/> and <paramref name="ditherer"/> is <see langword="null"/>,
        /// then its palette entries will be transformed instead of the actual pixels. To transform the colors of an indexed <see cref="Bitmap"/> without changing the palette
        /// specify a non-<see langword="null"/>&#160;<paramref name="ditherer"/>. Transforming the palette is both faster and provides a better result.</para>
        /// <para>The <paramref name="ditherer"/> is ignored for <see cref="PixelFormat"/>s with more than 16 bits-per-pixel and for the <see cref="PixelFormat.Format16bppGrayScale"/> format.</para>
        /// </remarks>
        /// <seealso cref="BitmapDataExtensions.MakeGrayscale"/>
        public static void MakeGrayscale(this Bitmap bitmap, IDitherer? ditherer = null)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap), PublicResources.ArgumentNull);

            using IReadWriteBitmapData bitmapData = bitmap.GetReadWriteBitmapData();
            bitmapData.MakeGrayscale(ditherer);
        }

        /// <summary>
        /// Adjusts the brightness of the specified <paramref name="bitmap"/>.
        /// <br/>See the <strong>Remarks</strong> section for details and an example.
        /// </summary>
        /// <param name="bitmap">The <see cref="Bitmap"/> to be transformed.</param>
        /// <param name="brightness">A float value between -1 and 1, inclusive bounds. Positive values make the <paramref name="bitmap"/> brighter,
        /// while negative values make it darker.</param>
        /// <param name="ditherer">An optional <see cref="IDitherer"/> instance to dither the result of the transformation if the transformed colors
        /// are not compatible with the <see cref="PixelFormat"/> of the specified <paramref name="bitmap"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="channels">The <see cref="ColorChannels"/>, on which the adjustment has to be performed. This parameter is optional.
        /// <br/>Default value: <see cref="ColorChannels.Rgb"/>.</param>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. Use the <see cref="BitmapDataExtensions.BeginAdjustBrightness">BitmapDataExtensions.BeginAdjustBrightness</see>
        /// or <see cref="BitmapDataExtensions.AdjustBrightnessAsync">BitmapDataExtensions.AdjustBrightnessAsync</see> (in .NET 4.0 and above) methods for asynchronous call and to adjust parallelization, set up cancellation and for reporting progress.</note>
        /// <para>If <paramref name="bitmap"/> has an indexed <see cref="PixelFormat"/> and <paramref name="ditherer"/> is <see langword="null"/>,
        /// then its palette entries will be transformed instead of the actual pixels. To transform the colors of an indexed <see cref="Bitmap"/> without changing the palette
        /// specify a non-<see langword="null"/>&#160;<paramref name="ditherer"/>. Transforming the palette is both faster and provides a better result.</para>
        /// <para>The <paramref name="ditherer"/> is ignored for <see cref="PixelFormat"/>s with more than 16 bits-per-pixel and for the <see cref="PixelFormat.Format16bppGrayScale"/> format.</para>
        /// </remarks>
        /// <example>
        /// The following example demonstrates how to use this method:
        /// <code lang="C#"><![CDATA[
        /// using Bitmap original = Icons.Shield.ExtractBitmap(new Size(256, 256));
        ///
        /// // starting with an indexed image using an optimized 8 BPP palette
        /// using Bitmap bmp = original.ConvertPixelFormat(PixelFormat.Format8bppIndexed,
        ///     OptimizedPaletteQuantizer.MedianCut());
        ///
        /// bmp.SaveAsGif(@"c:\temp\before.gif");
        ///
        /// // Making the image darker. By specifying a ditherer the original palette is preserved
        /// // (which is not so optimal for the transformed image anymore). The ditherer tries
        /// // to approximate the desired result with the original palette as much as possible.
        /// // Try it also without a ditherer to transform only the palette entries.
        /// // Try different brightness values and ColorChannels, too.
        /// bmp.AdjustBrightness(-0.5f, ErrorDiffusionDitherer.FloydSteinberg, ColorChannels.Rgb);
        ///
        /// // As ditherer was not null now the result is generated using the original palette
        /// bmp.SaveAsGif(@"c:\temp\after.gif");]]></code>
        /// <para>The example above produces the following results:
        /// <list type="table">
        /// <item><term><c>before.gif</c></term><term><img src="../Help/Images/ShieldMedianCut256Black.gif" alt="Shield icon quantized to 256 colors using the Median Cut algorithm"/></term></item>
        /// <item><term><c>after.gif</c></term><term><img src="../Help/Images/ShieldMedianCut256TrBrightnessFS.gif" alt="Shield icon transformed to be darker with Floyd-Steinberg dithering while still using a palette optimized for the original image"/></term></item>
        /// </list></para>
        /// </example>
        /// <exception cref="ArgumentNullException"><paramref name="bitmap"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="brightness"/> is not between -1 and 1
        /// <br/>-or-
        /// <br/><paramref name="channels"/> is out of the defined flags.</exception>
        /// <seealso cref="BitmapDataExtensions.AdjustBrightness"/>
        public static void AdjustBrightness(this Bitmap bitmap, float brightness, IDitherer? ditherer = null, ColorChannels channels = ColorChannels.Rgb)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap), PublicResources.ArgumentNull);

            using IReadWriteBitmapData bitmapData = bitmap.GetReadWriteBitmapData();
            bitmapData.AdjustBrightness(brightness, ditherer, channels);
        }

        /// <summary>
        /// Adjusts the contrast of the specified <paramref name="bitmap"/>.
        /// <br/>See the <strong>Remarks</strong> section for details and an example.
        /// </summary>
        /// <param name="bitmap">The <see cref="Bitmap"/> to be transformed.</param>
        /// <param name="contrast">A float value between -1 and 1, inclusive bounds. Positive values increase the contrast,
        /// while negative values decrease the it.</param>
        /// <param name="ditherer">An optional <see cref="IDitherer"/> instance to dither the result of the transformation if the transformed colors
        /// are not compatible with the <see cref="PixelFormat"/> of the specified <paramref name="bitmap"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="channels">The <see cref="ColorChannels"/>, on which the adjustment has to be performed. This parameter is optional.
        /// <br/>Default value: <see cref="ColorChannels.Rgb"/>.</param>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. Use the <see cref="BitmapDataExtensions.BeginAdjustContrast">BitmapDataExtensions.BeginAdjustContrast</see>
        /// or <see cref="BitmapDataExtensions.AdjustContrastAsync">BitmapDataExtensions.AdjustContrastAsync</see> (in .NET 4.0 and above) methods for asynchronous call and to adjust parallelization, set up cancellation and for reporting progress.</note>
        /// <para>If <paramref name="bitmap"/> has an indexed <see cref="PixelFormat"/> and <paramref name="ditherer"/> is <see langword="null"/>,
        /// then its palette entries will be transformed instead of the actual pixels. To transform the colors of an indexed <see cref="Bitmap"/> without changing the palette
        /// specify a non-<see langword="null"/>&#160;<paramref name="ditherer"/>. Transforming the palette is both faster and provides a better result.</para>
        /// <para>The <paramref name="ditherer"/> is ignored for <see cref="PixelFormat"/>s with more than 16 bits-per-pixel and for the <see cref="PixelFormat.Format16bppGrayScale"/> format.</para>
        /// </remarks>
        /// <example>
        /// The following example demonstrates how to use this method:
        /// <code lang="C#"><![CDATA[
        /// using Bitmap original = Icons.Shield.ExtractBitmap(new Size(256, 256));
        ///
        /// // starting with an indexed image using an optimized 8 BPP palette
        /// using Bitmap bmp = original.ConvertPixelFormat(PixelFormat.Format8bppIndexed,
        ///     OptimizedPaletteQuantizer.MedianCut());
        ///
        /// bmp.SaveAsGif(@"c:\temp\before.gif");
        ///
        /// // Decreasing the contrast. By specifying a ditherer the original palette is preserved
        /// // (which is not so optimal for the transformed image anymore). The ditherer tries
        /// // to approximate the desired result with the original palette as much as possible.
        /// // Try it also without a ditherer to transform only the palette entries.
        /// // Try different contrast values and ColorChannels, too.
        /// bmp.AdjustContrast(-0.5f, ErrorDiffusionDitherer.FloydSteinberg, ColorChannels.Rgb);
        ///
        /// // As ditherer was not null now the result is generated using the original palette
        /// bmp.SaveAsGif(@"c:\temp\after.gif");]]></code>
        /// <para>The example above produces the following results:
        /// <list type="table">
        /// <item><term><c>before.gif</c></term><term><img src="../Help/Images/ShieldMedianCut256Black.gif" alt="Shield icon quantized to 256 colors using the Median Cut algorithm"/></term></item>
        /// <item><term><c>after.gif</c></term><term><img src="../Help/Images/ShieldMedianCut256TrContrastFS.gif" alt="Shield icon with decreased contrast using Floyd-Steinberg dithering and a palette optimized for the untransformed image"/></term></item>
        /// </list></para>
        /// </example>
        /// <exception cref="ArgumentNullException"><paramref name="bitmap"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="contrast"/> is not between -1 and 1
        /// <br/>-or-
        /// <br/><paramref name="channels"/> is out of the defined flags.</exception>
        /// <seealso cref="BitmapDataExtensions.AdjustContrast"/>
        public static void AdjustContrast(this Bitmap bitmap, float contrast, IDitherer? ditherer = null, ColorChannels channels = ColorChannels.Rgb)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap), PublicResources.ArgumentNull);

            using IReadWriteBitmapData bitmapData = bitmap.GetReadWriteBitmapData();
            bitmapData.AdjustContrast(contrast, ditherer, channels);
        }

        /// <summary>
        /// Adjusts the gamma correction of the specified <paramref name="bitmap"/>.
        /// <br/>See the <strong>Remarks</strong> section for details and an example.
        /// </summary>
        /// <param name="bitmap">The <see cref="Bitmap"/> to be transformed.</param>
        /// <param name="gamma">A float value between 0 and 10, inclusive bounds. Values less than 1 decrease gamma correction,
        /// while values above 1 increase it.</param>
        /// <param name="ditherer">An optional <see cref="IDitherer"/> instance to dither the result of the transformation if the transformed colors
        /// are not compatible with the <see cref="PixelFormat"/> of the specified <paramref name="bitmap"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="channels">The <see cref="ColorChannels"/>, on which the adjustment has to be performed. This parameter is optional.
        /// <br/>Default value: <see cref="ColorChannels.Rgb"/>.</param>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. Use the <see cref="BitmapDataExtensions.BeginAdjustGamma">BitmapDataExtensions.BeginAdjustGamma</see>
        /// or <see cref="BitmapDataExtensions.AdjustGammaAsync">BitmapDataExtensions.AdjustGammaAsync</see> (in .NET 4.0 and above) methods for asynchronous call and to adjust parallelization, set up cancellation and for reporting progress.</note>
        /// <para>If <paramref name="bitmap"/> has an indexed <see cref="PixelFormat"/> and <paramref name="ditherer"/> is <see langword="null"/>,
        /// then its palette entries will be transformed instead of the actual pixels. To transform the colors of an indexed <see cref="Bitmap"/> without changing the palette
        /// specify a non-<see langword="null"/>&#160;<paramref name="ditherer"/>. Transforming the palette is both faster and provides a better result.</para>
        /// <para>The <paramref name="ditherer"/> is ignored for <see cref="PixelFormat"/>s with more than 16 bits-per-pixel and for the <see cref="PixelFormat.Format16bppGrayScale"/> format.</para>
        /// </remarks>
        /// <example>
        /// The following example demonstrates how to use this method:
        /// <code lang="C#"><![CDATA[
        /// using Bitmap original = Icons.Shield.ExtractBitmap(new Size(256, 256));
        ///
        /// // starting with an indexed image using an optimized 8 BPP palette
        /// using Bitmap bmp = original.ConvertPixelFormat(PixelFormat.Format8bppIndexed,
        ///     OptimizedPaletteQuantizer.MedianCut());
        ///
        /// bmp.SaveAsGif(@"c:\temp\before.gif");
        ///
        /// // Decreasing gamma. By specifying a ditherer the original palette is preserved
        /// // (which is not so optimal for the transformed image anymore). The ditherer tries
        /// // to approximate the desired result with the original palette as much as possible.
        /// // Try it also without a ditherer to transform only the palette entries.
        /// // Try different values and ColorChannels, too.
        /// bmp.AdjustGamma(0.5f, ErrorDiffusionDitherer.FloydSteinberg, ColorChannels.Rgb);
        ///
        /// // As ditherer was not null now the result is generated using the original palette
        /// bmp.SaveAsGif(@"c:\temp\after.gif");]]></code>
        /// <para>The example above produces the following results:
        /// <list type="table">
        /// <item><term><c>before.gif</c></term><term><img src="../Help/Images/ShieldMedianCut256Black.gif" alt="Shield icon quantized to 256 colors using the Median Cut algorithm"/></term></item>
        /// <item><term><c>after.gif</c></term><term><img src="../Help/Images/ShieldMedianCut256TrGammaFS.gif" alt="Shield icon with decreased gamma using Floyd-Steinberg dithering and a palette optimized for the untransformed image"/></term></item>
        /// </list></para>
        /// </example>
        /// <exception cref="ArgumentNullException"><paramref name="bitmap"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="gamma"/> is not between 0 and 10
        /// <br/>-or-
        /// <br/><paramref name="channels"/> is out of the defined flags.</exception>
        /// <seealso cref="BitmapDataExtensions.AdjustGamma"/>
        public static void AdjustGamma(this Bitmap bitmap, float gamma, IDitherer? ditherer = null, ColorChannels channels = ColorChannels.Rgb)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap), PublicResources.ArgumentNull);

            using IReadWriteBitmapData bitmapData = bitmap.GetReadWriteBitmapData();
            bitmapData.AdjustGamma(gamma, ditherer, channels);
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Tries to extract the icon images from an image.
        /// </summary>
        internal static Bitmap[] ExtractIconImages(this Bitmap image)
        {
            Debug.Assert(image.RawFormat.Guid == ImageFormat.Icon.Guid);

            // guessing by official sizes (every size will be extracted with the same pixel format)
            List<Bitmap> result = new List<Bitmap>();
            int nextSize = iconSizes[0];
            HashSet<long> foundSizes = new HashSet<long>();
            HashSet<int> testedSizes = new HashSet<int>();
            do
            {
                Size iconSize = new Size(nextSize, nextSize);
                testedSizes.Add(nextSize);
                Bitmap testImage = new Bitmap(image, iconSize);

                // after drawing the image on a new bmp, its size will be changed to the best image size (does not work on Linux)
                iconSize = image.Size;

                // a new resolution has been found
                if (!foundSizes.Contains((long)iconSize.Width << 32 | (uint)iconSize.Height))
                {
                    if (testImage.Size != iconSize)
                    {
                        testImage.Dispose();
                        testImage = new Bitmap(image, iconSize);
                    }

                    result.Add(testImage);
                    foundSizes.Add((long)iconSize.Width << 32 | (uint)iconSize.Height);
                }
                else
                    testImage.Dispose();

                nextSize = iconSizes.FirstOrDefault(s => s < iconSize.Width && !testedSizes.Contains(s));
            } while (nextSize > 0);

            return result.ToArray();
        }

        /// <summary>
        /// Returns a clone of a bitmap in a way that works also on Linux where Image.Clone may return a fully transparent image.
        /// </summary>
        internal static Bitmap CloneBitmap(this Bitmap bmp) => bmp.Clone(new Rectangle(Point.Empty, bmp.Size), bmp.PixelFormat);

        internal static void SetPalette(this Bitmap target, Color[] palette)
        {
            ColorPalette targetPalette = target.Palette;
            bool setEntries = palette.Length != targetPalette.Entries.Length;
            Color[] targetColors = setEntries ? new Color[palette.Length] : targetPalette.Entries;

            // Flags actually matter on Mono/Linux
            bool hasAlpha = false;
            bool isGrayscale = true;
            for (int i = 0; i < palette.Length; i++)
            {
                targetColors[i] = palette[i];
                if (!hasAlpha)
                    hasAlpha = palette[i].A < Byte.MaxValue;
                if (isGrayscale)
                    isGrayscale = palette[i].R == palette[i].G && palette[i].R == palette[i].B;
            }

            if (setEntries)
                targetPalette.SetEntries(targetColors);
            int flags = (hasAlpha ? 1 : 0) | (isGrayscale ? 2 : 0);
            if (flags != targetPalette.Flags)
                targetPalette.SetFlags(flags);
            target.Palette = targetPalette;
        }

        internal static void SetPalette(this Bitmap target, Palette palette)
        {
            ColorPalette targetPalette = target.Palette;
            Color32[] sourceColors = palette.Entries;
            bool setEntries = sourceColors.Length != targetPalette.Entries.Length;
            Color[] targetColors = setEntries ? new Color[sourceColors.Length] : targetPalette.Entries;

            for (int i = 0; i < sourceColors.Length; i++)
                targetColors[i] = sourceColors[i].ToColor();

            if (setEntries)
                targetPalette.SetEntries(targetColors);
            int flags = (palette.HasAlpha ? 1 : 0) | (palette.IsGrayscale ? 2 : 0);
            if (flags != targetPalette.Flags)
                targetPalette.SetFlags(flags);
            target.Palette = targetPalette;
        }

        #endregion

        #region Private Methods

        private static Rectangle GetTargetRectangleWithPreservedAspectRatio(Size desiredSize, Size sourceSize)
        {
            float ratio = Math.Min((float)desiredSize.Width / sourceSize.Width, (float)desiredSize.Height / sourceSize.Height);
            var targetSize = new Size((int)(sourceSize.Width * ratio), (int)(sourceSize.Height * ratio));
            var targetLocation = new Point((desiredSize.Width >> 1) - (targetSize.Width >> 1), (desiredSize.Height >> 1) - (targetSize.Height >> 1));
            return new Rectangle(targetLocation, targetSize);
        }

        #endregion

        #endregion
    }
}
