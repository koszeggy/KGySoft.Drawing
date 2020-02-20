#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BitmapExtensions.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2019 - All Rights Reserved
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Security;

using KGySoft.Collections;
using KGySoft.CoreLibraries;
using KGySoft.Drawing.Imaging;
using KGySoft.Drawing.WinApi;

#endregion

namespace KGySoft.Drawing
{
    /// <summary>
    /// Contains extension methods for the <see cref="Bitmap"/> type.
    /// </summary>
    public static class BitmapExtensions
    {
        #region Constants

        private const int parallelThreshold = 100;

        #endregion

        #region Fields

        private static readonly int[] iconSizes = { 512, 384, 320, 256, 128, 96, 80, 72, 64, 60, 48, 40, 36, 32, 30, 24, 20, 16, 8, 4 };

        private static readonly IThreadSafeCacheAccessor<float, byte[]> gammaLookupTableCache = new Cache<float, byte[]>(GenerateGammaLookupTable, 16).GetThreadSafeAccessor();

        #endregion

        #region Methods

        #region Public Methods

        /// <summary>
        /// Resizes the image with high quality. The result is always a 32 bit ARGB image.
        /// </summary>
        /// <param name="image">The original image to resize</param>
        /// <param name="newSize">The requested new size.</param>
        /// <param name="keepAspectRatio">Determines whether the source <paramref name="image"/> should keep aspect ratio. This parameter is optional.
        /// <br/>Default value: <see langword="false"/>.</param>
        /// <returns>A <see cref="Bitmap"/> instance with the new size.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The result must not be disposed.")]
        public static Bitmap Resize(this Bitmap image, Size newSize, bool keepAspectRatio = false)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image), PublicResources.ArgumentNull);

            if (newSize.Width < 1 || newSize.Height < 1)
                throw new ArgumentOutOfRangeException(nameof(newSize), PublicResources.ArgumentOutOfRange);

            Size targetSize = newSize;
            Size sourceSize = image.Size;
            Point targetLocation = Point.Empty;

            if (keepAspectRatio && newSize != sourceSize)
            {
                float ratio = Math.Min((float)newSize.Width / sourceSize.Width, (float)newSize.Height / sourceSize.Height);
                targetSize = new Size((int)(sourceSize.Width * ratio), (int)(sourceSize.Height * ratio));
                targetLocation = new Point(newSize.Width / 2 - targetSize.Width / 2, newSize.Height / 2 - targetSize.Height / 2);
            }

            Bitmap result = new Bitmap(newSize.Width, newSize.Height);
            if (OSUtils.IsWindows)
                result.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            Bitmap source = image;
            if (!source.PixelFormat.CanBeDrawn())
                source = image.ConvertPixelFormat(PixelFormat.Format32bppArgb);

            try
            {
                using (Graphics g = Graphics.FromImage(result))
                {
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.DrawImage(source, new Rectangle(targetLocation, targetSize), new Rectangle(Point.Empty, sourceSize), GraphicsUnit.Pixel);
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
            FrameDimension dimension = null;
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
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The result must not be disposed.")]
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
                        MemoryHelper.CopyMemory(lineTarget, lineSource, sourceData.Stride * sourceData.Height);
                        return result;
                    }

                    int lineWidth = Math.Abs(sourceData.Stride);
                    for (int y = 0; y < sourceData.Height; y++)
                    {
                        MemoryHelper.CopyMemory(lineTarget, lineSource, lineWidth);
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
        /// </summary>
        /// <param name="bitmap">The bitmap to get its colors. When it is indexed and the <paramref name="forceScanningContent"/> parameter is <see langword="false"/>,
        /// then its palette is returned and <paramref name="maxColors"/> is ignored.</param>
        /// <param name="maxColors">A limit of the returned colors. If <paramref name="forceScanningContent"/> parameter is <see langword="false"/>, then
        /// this parameter is ignored for indexed bitmaps. Use 0 for no limit. This parameter is optional.
        /// <br/>Default value: <c>0</c>.</param>
        /// <param name="forceScanningContent"><see langword="true"/>&#160;to force scanning the actual image content even if the specified <paramref name="bitmap"/> is
        /// indexed and has a palette.</param>
        /// <remarks>The method is optimized for <see cref="PixelFormat.Format32bppRgb"/> and <see cref="PixelFormat.Format32bppArgb"/> formats.</remarks>
        /// <returns>An array of <see cref="Color"/> entries.</returns>
        public static Color[] GetColors(this Bitmap bitmap, int maxColors = 0, bool forceScanningContent = false)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap), PublicResources.ArgumentNull);

            if (bitmap.PixelFormat.IsIndexed() && !forceScanningContent)
                return bitmap.Palette.Entries;

            return DoGetColors(bitmap, maxColors).Select(c => c.ToColor()).ToArray();
        }

        public static int GetColorCount(this Bitmap bitmap)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap), PublicResources.ArgumentNull);
            return DoGetColors(bitmap, 0).Count;
        }

        /// <summary>
        /// Converts the provided <paramref name="bitmap"/> to a <see cref="CursorHandle"/>, which can be passed to the
        /// <a href="https://msdn.microsoft.com/en-us/library/system.windows.forms.cursor.aspx" target="_blank">System.Windows.Forms.Cursor</a> constructor
        /// to create a new cursor.
        /// </summary>
        /// <param name="bitmap">The <see cref="Bitmap"/>, which should be converted to a cursor.</param>
        /// <param name="cursorHotspot">The hotspot coordinates of the cursor. This parameter is optional.
        /// <br/>Default value: <c>0; 0</c> (top-left corner)</param>
        /// <returns>A <see cref="CursorHandle"/> instance that can be used to create a <a href="https://msdn.microsoft.com/en-us/library/system.windows.forms.cursor.aspx" target="_blank">System.Windows.Forms.Cursor</a> instance.</returns>
        /// <remarks>
        /// <note>This method is supported on Windows only.</note>
        /// </remarks>
        /// <exception cref="PlatformNotSupportedException">This method is supported on Windows only.</exception>
#if !NET35
        [SecuritySafeCritical]
#endif
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The result must not be disposed.")]
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
        /// <param name="backColor">In case of a read-only <see cref="IReadableBitmapData"/> affects indexed bitmaps only when <see cref="Palette.GetNearestColorIndex">GetColorIndex</see>
        /// and <see cref="Palette.GetNearestColor">GetNearestColor</see> methods of the <see cref="IBitmapData.Palette"/> property are called with a color with alpha,
        /// in which case determines the background color. Queried colors with alpha, which are considered opaque will be blended with this color before performing a lookup.
        /// The alpha value (<see cref="Color.A"/> property) of the specified background color is ignored. This parameter is optional.
        /// <br/>Default value: <see cref="Color.Empty">Color.Empty</see>, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="alphaThreshold">In case of a read-only <see cref="IReadableBitmapData"/> affects indexed bitmaps with a transparent palette entry when <see cref="Palette.GetNearestColorIndex">GetColorIndex</see>
        /// and <see cref="Palette.GetNearestColor">GetNearestColor</see> methods of the <see cref="IBitmapData.Palette"/> property are called with a color with alpha.
        /// In such case determines the lowest alpha value of a color, which should not be considered as transparent. If <c>0</c>,
        /// then a color lookup will never return a transparent color. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <returns>An <see cref="IReadableBitmapData"/> instance, which provides fast read-only access to the actual data of the specified <paramref name="bitmap"/>.</returns>
        /// <seealso cref="GetWritableBitmapData"/>
        /// <seealso cref="GetReadWriteBitmapData"/>
        public static IReadableBitmapData GetReadableBitmapData(this Bitmap bitmap, Color backColor = default, byte alphaThreshold = 128)
            => BitmapDataAccessorFactory.CreateAccessor(bitmap, ImageLockMode.ReadOnly, new Color32(backColor), alphaThreshold);

        /// <summary>
        /// Gets an <see cref="IWritableBitmapData"/> instance, which provides fast write-only access to the actual data of the specified <paramref name="bitmap"/>.
        /// The <paramref name="bitmap"/> can have any <see cref="PixelFormat"/>.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="GetReadWriteBitmapData">GetReadWriteBitmapData</see> method for details and examples.
        /// </summary>
        /// <param name="bitmap">A <see cref="Bitmap"/> instance, whose data is about to be accessed.</param>
        /// <param name="backColor">When setting pixels of indexed bitmaps, bitmaps without alpha support or with single bit alpha, then specifies the color of the background.
        /// Color values with alpha, which are considered opaque will be blended with this color before setting the pixel in the result bitmap data.
        /// The alpha value (<see cref="Color.A"/> property) of the specified background color is ignored. This parameter is optional.
        /// <br/>Default value: <see cref="Color.Empty">Color.Empty</see>, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="alphaThreshold">When setting pixels of bitmaps with single bit alpha or with a palette that has a transparent color,
        /// then specifies a threshold value for the <see cref="Color.A">Color.A</see> property, under which the color is considered transparent. If <c>0</c>,
        /// then the pixels to be set will never be transparent. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <returns>An <see cref="IWritableBitmapData"/> instance, which provides fast write-only access to the actual data of the specified <paramref name="bitmap"/>.</returns>
        /// <seealso cref="GetReadableBitmapData"/>
        /// <seealso cref="GetReadWriteBitmapData"/>
        public static IWritableBitmapData GetWritableBitmapData(this Bitmap bitmap, Color backColor = default, byte alphaThreshold = 128)
            => BitmapDataAccessorFactory.CreateAccessor(bitmap, ImageLockMode.WriteOnly, new Color32(backColor), alphaThreshold);

        /// <summary>
        /// Gets an <see cref="IReadWriteBitmapData"/> instance, which provides fast read-write access to the actual data of the specified <paramref name="bitmap"/>.
        /// The <paramref name="bitmap"/> can have any <see cref="PixelFormat"/>.
        /// <br/>See the <strong>Remarks</strong> section for details and examples.
        /// </summary>
        /// <param name="bitmap">A <see cref="Bitmap"/> instance, whose data is about to be accessed.</param>
        /// <param name="backColor">When setting pixels of indexed bitmaps, bitmaps without alpha support or with single bit alpha, then specifies the color of the background.
        /// Color values with alpha, which are considered opaque will be blended with this color before setting the pixel in the result bitmap data.
        /// The alpha value (<see cref="Color.A"/> property) of the specified background color is ignored. This parameter is optional.
        /// <br/>Default value: <see cref="Color.Empty">Color.Empty</see>, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="alphaThreshold">When setting pixels of bitmaps with single bit alpha or with a palette that has a transparent color,
        /// then specifies a threshold value for the <see cref="Color.A">Color.A</see> property, under which the color is considered transparent. If <c>0</c>,
        /// then the pixels to be set will never be transparent. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance, which provides fast read-write access to the actual data of the specified <paramref name="bitmap"/>.</returns>
        /// <remarks>
        /// <para>All possible <see cref="PixelFormat"/>s are supported, of which a <see cref="Bitmap"/> can be created in the current operating system.
        /// <note>For information about the possible usable <see cref="PixelFormat"/>s on different platforms see the <strong>Remarks</strong> section of the <see cref="ImageExtensions.ConvertPixelFormat(Image,PixelFormat,Color,byte)">ConvertPixelFormat</see> method.</note>
        /// </para>
        /// <para>If <paramref name="alphaThreshold"/> is zero, then setting a pixel of a bitmap with indexed or single-bit-alpha pixel format
        /// by a fully transparent pixel will be blended with <paramref name="backColor"/> even if the bitmap can handle transparent pixels.</para>
        /// <para>If <paramref name="alphaThreshold"/> is <c>1</c>, then setting a pixel of a bitmap with indexed or single-bit-alpha pixel format
        /// will be transparent only if the color to set is completely transparent (has zero alpha).</para>
        /// <para>If <paramref name="alphaThreshold"/> is <c>255</c>, then setting a pixel of a bitmap with indexed or single-bit-alpha pixel format
        /// will be opaque only if the color to set is completely opaque (its alpha value is <c>255</c>).</para>
        /// <para>If a pixel of a bitmap without alpha gradient support is set by the <see cref="IWritableBitmapData.SetPixel">IWritableBitmapData.SetPixel</see>/<see cref="IWritableBitmapDataRow.SetColor">IWritableBitmapDataRow.SetColor</see>
        /// methods or by the <see cref="IReadWriteBitmapDataRow.this">IReadWriteBitmapDataRow indexer</see>, and the pixel has an alpha value that is greater than <paramref name="alphaThreshold"/>,
        /// then the pixel to set will be blended with <paramref name="backColor"/>.</para>
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
        ///     using (IWritableBitmapData dataDst = bmpDst.GetWritableBitmapData(Color.Silver, 160))
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
        /// <item><term><c>bmpDst.png</c></term><term><img src="../Help/Images/ConvertPixelFormat8bppWebsafe.gif" alt="Shield icon with system default 8 BPP websafe palette"/></term></item>
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
        /// using (Bitmap bmp = Icons.Information.ExtractBitmap(new Size(256, 256)))
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
        public static IReadWriteBitmapData GetReadWriteBitmapData(this Bitmap bitmap, Color backColor = default, byte alphaThreshold = 128)
            => BitmapDataAccessorFactory.CreateAccessor(bitmap, ImageLockMode.ReadWrite, new Color32(backColor), alphaThreshold);

        public static void Quantize(this Bitmap bitmap, IQuantizer quantizer)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap), PublicResources.ArgumentNull);
            if (quantizer == null)
                throw new ArgumentNullException(nameof(quantizer), PublicResources.ArgumentNull);

            using (BitmapDataAccessorBase bitmapData = BitmapDataAccessorFactory.CreateAccessor(bitmap, ImageLockMode.ReadWrite))
            using (IQuantizingSession session = quantizer.Initialize(bitmapData))
            {
                if (session == null)
                    throw new InvalidOperationException(Res.ImagingQuantizerInitializeNull);

                // Sequential processing
                if (bitmapData.Width < parallelThreshold)
                {
                    int width = bitmapData.Width;
                    BitmapDataRowBase row = bitmapData.GetRow(0);
                    do
                    {
                        for (int x = 0; x < width; x++)
                            row.DoSetColor32(x, session.GetQuantizedColor(row.DoGetColor32(x)));
                    } while (row.MoveNextRow());

                    return;
                }

                // Parallel processing
                ParallelHelper.For(0, bitmapData.Height, y =>
                {
                    int width = bitmapData.Width;
                    BitmapDataRowBase row = bitmapData.GetRow(y);
                    for (int x = 0; x < width; x++)
                        row.DoSetColor32(x, session.GetQuantizedColor(row.DoGetColor32(x)));
                });
            }
        }

        public static void Dither(this Bitmap bitmap, IQuantizer quantizer, IDitherer ditherer)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap), PublicResources.ArgumentNull);
            if (quantizer == null)
                throw new ArgumentNullException(nameof(quantizer), PublicResources.ArgumentNull);
            if (ditherer == null)
                throw new ArgumentNullException(nameof(ditherer), PublicResources.ArgumentNull);

            using (BitmapDataAccessorBase bitmapData = BitmapDataAccessorFactory.CreateAccessor(bitmap, ImageLockMode.ReadWrite))
            using (IQuantizingSession quantizingSession = quantizer.Initialize(bitmapData) ?? throw new InvalidOperationException(Res.ImagingQuantizerInitializeNull))
            using (IDitheringSession ditheringSession = ditherer.Initialize(bitmapData, quantizingSession) ?? throw new InvalidOperationException(Res.ImagingDithererInitializeNull))
            {
                // Sequential processing
                if (ditheringSession.IsSequential || bitmapData.Width < parallelThreshold)
                {
                    int width = bitmapData.Width;
                    BitmapDataRowBase row = bitmapData.GetRow(0);
                    int y = 0;
                    do
                    {
                        for (int x = 0; x < width; x++)
                            row.DoSetColor32(x, ditheringSession.GetDitheredColor(row.DoGetColor32(x), x, y));

                        y += 1;
                    } while (row.MoveNextRow());

                    return;
                }

                // Parallel processing
                ParallelHelper.For(0, bitmapData.Height, y =>
                {
                    int width = bitmapData.Width;
                    BitmapDataRowBase row = bitmapData.GetRow(y);
                    for (int x = 0; x < width; x++)
                        row.DoSetColor32(x, ditheringSession.GetDitheredColor(row.DoGetColor32(x), x, y));
                });
            }
        }

        /// <summary>
        /// Clears the complete <paramref name="bitmap"/> and fills it with the specified <paramref name="color"/>.
        /// <br/>This method is similar to <see cref="Graphics.Clear">Graphics.Clear</see> but can be used for <see cref="Bitmap"/>s of any <see cref="PixelFormat"/>.
        /// </summary>
        /// <param name="bitmap">The <see cref="Bitmap"/> to be cleared.</param>
        /// <param name="color">A <see cref="Color"/> that represents the desired result color of the bitmap.</param>
        /// <param name="backColor">If <paramref name="bitmap"/> cannot have alpha or has only single-bit alpha, and <paramref name="color"/> is not fully opaque, then specifies the color of the background.
        /// If <paramref name="color"/> has alpha and it is considered opaque, then it will be blended with <paramref name="backColor"/> to determine the color of the cleared <paramref name="bitmap"/>.
        /// The <see cref="Color.A"/> property of the background color is ignored. This parameter is optional.
        /// <br/>Default value: <see cref="Color.Empty"/>, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="alphaThreshold">If <paramref name="bitmap"/> has only single-bit alpha or its palette contains a transparent color,
        /// then specifies a threshold value for the <see cref="Color.A">Color.A</see> property, under which the specified <paramref name="color"/> is considered transparent. If <c>0</c>,
        /// then the cleared <paramref name="bitmap"/> will not be transparent. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        public static void Clear(this Bitmap bitmap, Color color, Color backColor = default, byte alphaThreshold = 128)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap), PublicResources.ArgumentNull);
            using (var accessor = BitmapDataAccessorFactory.CreateAccessor(bitmap, ImageLockMode.ReadWrite, new Color32(backColor), alphaThreshold))
                ClearDirect(accessor, new Color32(color));
        }

        /// <summary>
        /// Clears the complete <paramref name="bitmap"/> and fills it with the specified <paramref name="color"/>.
        /// <br/>This method is similar to <see cref="Graphics.Clear">Graphics.Clear</see> but can be used for <see cref="Bitmap"/>s of any <see cref="PixelFormat"/>.
        /// </summary>
        /// <param name="bitmap">The <see cref="Bitmap"/> to be cleared.</param>
        /// <param name="color">A <see cref="Color"/> that represents the desired result color of the bitmap.</param>
        /// <param name="backColor">If <paramref name="bitmap"/> cannot have alpha or has only single-bit alpha, and <paramref name="color"/> is not fully opaque, then specifies the color of the background.
        /// If <paramref name="color"/> has alpha and it is considered opaque, then it will be blended with <paramref name="backColor"/> to determine the color of the cleared <paramref name="bitmap"/>.
        /// The <see cref="Color.A"/> property of the background color is ignored. This parameter is optional.
        /// <br/>Default value: <see cref="Color.Empty"/>, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="alphaThreshold">If <paramref name="bitmap"/> has only single-bit alpha or its palette contains a transparent color,
        /// then specifies a threshold value for the <see cref="Color.A">Color.A</see> property, under which the specified <paramref name="color"/> is considered transparent. If <c>0</c>,
        /// then the cleared <paramref name="bitmap"/> will not be transparent. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <param name="ditherer">The ditherer to be used for the clearing. Has no effect if <paramref name="bitmap"/> <see cref="PixelFormat"/> has at least 24 bits-per-pixel size.
        /// If <see langword="null"/>, then the <see cref="Clear(Bitmap,Color,Color,byte)"/> overload will be called.</param>
        public static void Clear(this Bitmap bitmap, Color color, IDitherer ditherer, Color backColor = default, byte alphaThreshold = 128)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap), PublicResources.ArgumentNull);

            Color32 c = new Color32(color);
            using (var accessor = BitmapDataAccessorFactory.CreateAccessor(bitmap, ImageLockMode.ReadWrite, new Color32(backColor), alphaThreshold))
            {
                if (ditherer == null || !accessor.PixelFormat.CanBeDithered())
                    ClearDirect(accessor, c);
                else
                    ClearWithDithering(accessor, c, ditherer);
            }
        }

        /// <param name="backColor">If <paramref name="bitmap"/> cannot have alpha or has only single-bit alpha, and the result value of <paramref name="transformFunction"/> is not fully opaque, then specifies the color of the background.
        /// If <paramref name="transformFunction"/> returns a color with alpha, which is considered opaque in <paramref name="bitmap"/>, then the result will be blended with this color before setting the pixel in <paramref name="bitmap"/>.
        /// The <see cref="Color.A"/> property of the background color is ignored. This parameter is optional.
        /// <br/>Default value: <see cref="Color.Empty"/>, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="alphaThreshold">If <paramref name="bitmap"/> has only single-bit alpha or its palette contains a transparent color,
        /// then specifies a threshold value for the <see cref="Color32.A">Color32.A</see> property, under which the result color is considered transparent. If <c>0</c>,
        /// then the pixels to be set will never be transparent. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        // example: Inverse or grayscale
        // note: indexed
        public static void TransformColors(this Bitmap bitmap, Func<Color32, Color32> transformFunction, Color backColor = default, byte alphaThreshold = 128)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap), PublicResources.ArgumentNull);
            if (transformFunction == null)
                throw new ArgumentNullException(nameof(transformFunction), PublicResources.ArgumentNull);

            PixelFormat pixelFormat = bitmap.PixelFormat;

            // Indexed format: processing the palette entries
            if (pixelFormat.IsIndexed())
            {
                ColorPalette palette = bitmap.Palette;
                Color[] entries = palette.Entries;
                for (int i = 0; i < entries.Length; i++)
                    entries[i] = transformFunction.Invoke(new Color32(entries[i])).ToColor();
                bitmap.Palette = palette;
                return;
            }

            // Non-indexed format: processing the pixels
            using (BitmapDataAccessorBase bitmapData = BitmapDataAccessorFactory.CreateAccessor(bitmap, ImageLockMode.ReadWrite, new Color32(backColor), alphaThreshold))
            {
                // Sequential processing
                if (bitmapData.Width < parallelThreshold)
                {
                    BitmapDataRowBase row = bitmapData.GetRow(0);
                    do
                    {
                        for (int x = 0; x < bitmapData.Width; x++)
                            row.DoSetColor32(x, transformFunction.Invoke(row.DoGetColor32(x)));
                    } while (row.MoveNextRow());

                    return;
                }

                // Parallel processing
                ParallelHelper.For(0, bitmapData.Height, y =>
                {
                    BitmapDataRowBase row = bitmapData.GetRow(y);
                    for (int x = 0; x < bitmapData.Width; x++)
                        row.DoSetColor32(x, transformFunction.Invoke(row.DoGetColor32(x)));
                });
            }
        }

        // note: if bitmap is indexed and ditherer is null, then only the palette entries are processed. This is actually much faster and produces better result than processing the pixels with dithering using the original palette.
        public static void TransformColors(this Bitmap bitmap, Func<Color32, Color32> transformFunction, IDitherer ditherer, Color backColor = default, byte alphaThreshold = 128)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap), PublicResources.ArgumentNull);

            PixelFormat pixelFormat = bitmap.PixelFormat;
            if (ditherer == null || !pixelFormat.CanBeDithered())
            {
                bitmap.TransformColors(transformFunction, backColor, alphaThreshold);
                return;
            }

            if (transformFunction == null)
                throw new ArgumentNullException(nameof(transformFunction), PublicResources.ArgumentNull);

            using (BitmapDataAccessorBase bitmapData = BitmapDataAccessorFactory.CreateAccessor(bitmap, ImageLockMode.ReadWrite, new Color32(backColor), alphaThreshold))
            {
                IQuantizer quantizer = PredefinedColorsQuantizer.FromBitmapData(bitmapData);
                using (IQuantizingSession quantizingSession = quantizer.Initialize(bitmapData))
                using (IDitheringSession ditheringSession = ditherer.Initialize(bitmapData, quantizingSession) ?? throw new InvalidOperationException(Res.ImagingDithererInitializeNull))
                {
                    // sequential processing
                    if (ditheringSession.IsSequential || bitmapData.Width < parallelThreshold)
                    {
                        BitmapDataRowBase row = bitmapData.GetRow(0);
                        int y = 0;
                        do
                        {
                            for (int x = 0; x < bitmapData.Width; x++)
                                row.DoSetColor32(x, ditheringSession.GetDitheredColor(transformFunction.Invoke(row.DoGetColor32(x)), x, y));
                            y += 1;
                        } while (row.MoveNextRow());

                        return;
                    }

                    // parallel processing
                    ParallelHelper.For(0, bitmapData.Height, y =>
                    {
                        BitmapDataRowBase row = bitmapData.GetRow(y);
                        for (int x = 0; x < bitmapData.Width; x++)
                            row.DoSetColor32(x, ditheringSession.GetDitheredColor(transformFunction.Invoke(row.DoGetColor32(x)), x, y));
                    });
                }
            }
        }

        // note: if newColor has alpha, which cannot be represented by bitmap, then it will be blended with black. Call TransformColor to use a custom backColor instead.
        public static void ReplaceColor(this Bitmap bitmap, Color oldColor, Color newColor, IDitherer ditherer = null)
        {
            Color32 from = new Color32(oldColor);
            Color32 to = new Color32(newColor);

            Color32 Transform(Color32 c) => c == from ? to : c;

            bitmap.TransformColors(Transform, ditherer);
        }

        // note: if bitmap is indexed and ditherer is null, then only the palette entries are processed. This is actually much faster and produces better result than processing the pixels with dithering using the original palette.
        public static void Inverse(this Bitmap bitmap, IDitherer ditherer = null)
        {
            static Color32 Transform(Color32 c) => new Color32(c.A, (byte)(255 - c.R), (byte)(255 - c.G), (byte)(255 - c.B));

            bitmap.TransformColors(Transform, ditherer);
        }

        // backColor.A is ignored
        // note: if bitmap is indexed and ditherer is null, then only the (partially) transparent palette entries are processed. This is actually much faster and produces better result than processing the pixels with dithering using the original palette.
        public static void MakeOpaque(this Bitmap bitmap, Color backColor, IDitherer ditherer = null)
        {
            Color32 backColor32 = new Color32(backColor);

            Color32 Transform(Color32 c) => c.A == Byte.MaxValue ? c : c.BlendWithBackground(backColor32);

            bitmap.TransformColors(Transform, ditherer, backColor, 0);
        }

        // note: if bitmap is indexed and ditherer is null, then only the palette entries are processed. This is actually much faster and produces better result than processing the pixels with dithering using the original palette.
        // see also ToGrayscale, which returns a new instance with PixelFormat ARGB32, whereas this method uses the original pixel format (and even palette if ditherer is not null)
        /// <para>To return a <see cref="Bitmap"/> with arbitrary <see cref="PixelFormat"/> use the <see cref="O:KGySoft.Drawing.ImageExtensions.ConvertPixelFormat"/> overloads with a grayscale palette,
        /// quantizer (eg. <see cref="PredefinedColorsQuantizer.Grayscale">PredefinedColorsQuantizer.Grayscale</see>) or pixel format (<see cref="PixelFormat.Format16bppGrayScale"/>).</para>
        public static void MakeGrayscale(this Bitmap bitmap, IDitherer ditherer = null)
        {
            static Color32 Transform(Color32 c)
            {
                byte br = c.GetBrightness();
                return new Color32(c.A, br, br, br);
            }

            bitmap.TransformColors(Transform, ditherer);
        }

        public static void AdjustBrightness(this Bitmap bitmap, float brightness, IDitherer ditherer = null, ColorChannels channels = ColorChannels.Rgb)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap), PublicResources.ArgumentNull);
            if (brightness < -1f || brightness > 1f || Single.IsNaN(brightness))
                throw new ArgumentOutOfRangeException(nameof(brightness), PublicResources.ArgumentMustBeBetween(-1f, 1f));
            if (!channels.AllFlagsDefined())
                throw new ArgumentOutOfRangeException(nameof(channels), PublicResources.FlagsEnumOutOfRange(channels));

            // ReSharper disable once CompareOfFloatsByEqualityOperator - zero has a precise float representation
            if (channels == ColorChannels.None || brightness == 0f)
                return;

            if (brightness < 0f)
            {
                brightness += 1f;
                bitmap.TransformColors(Darken, ditherer);
            }
            else
                bitmap.TransformColors(Lighten, ditherer);

            #region Local Methods

            Color32 Darken(Color32 c) => new Color32(c.A,
                (channels & ColorChannels.R) == ColorChannels.R ? (byte)(c.R * brightness) : c.R,
                (channels & ColorChannels.G) == ColorChannels.G ? (byte)(c.G * brightness) : c.G,
                (channels & ColorChannels.B) == ColorChannels.B ? (byte)(c.B * brightness) : c.B);

            Color32 Lighten(Color32 c) => new Color32(c.A,
                (channels & ColorChannels.R) == ColorChannels.R ? (byte)((255 - c.R) * brightness + c.R) : c.R,
                (channels & ColorChannels.G) == ColorChannels.G ? (byte)((255 - c.G) * brightness + c.G) : c.G,
                (channels & ColorChannels.B) == ColorChannels.B ? (byte)((255 - c.B) * brightness + c.B) : c.B);

            #endregion
        }

        public static void AdjustContrast(this Bitmap bitmap, float contrast, IDitherer ditherer = null, ColorChannels channels = ColorChannels.Rgb)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap), PublicResources.ArgumentNull);
            if (contrast < -1f || contrast > 1f || Single.IsNaN(contrast))
                throw new ArgumentOutOfRangeException(nameof(contrast), PublicResources.ArgumentMustBeBetween(-1f, 1f));
            if (!channels.AllFlagsDefined())
                throw new ArgumentOutOfRangeException(nameof(channels), PublicResources.FlagsEnumOutOfRange(channels));

            // ReSharper disable once CompareOfFloatsByEqualityOperator - zero has a precise float representation
            if (channels == ColorChannels.None || contrast == 0f)
                return;

            contrast += 1f;
            contrast *= contrast;

            Color32 Transform(Color32 c) => new Color32(c.A,
                (channels & ColorChannels.R) == ColorChannels.R ? ((int)(((c.R / 255f - 0.5f) * contrast + 0.5f) * 255f)).ClipToByte() : c.R,
                (channels & ColorChannels.G) == ColorChannels.G ? ((int)(((c.G / 255f - 0.5f) * contrast + 0.5f) * 255f)).ClipToByte() : c.G,
                (channels & ColorChannels.B) == ColorChannels.B ? ((int)(((c.B / 255f - 0.5f) * contrast + 0.5f) * 255f)).ClipToByte() : c.B);

            bitmap.TransformColors(Transform, ditherer);
        }

        public static void AdjustGamma(this Bitmap bitmap, float gamma, IDitherer ditherer = null, ColorChannels channels = ColorChannels.Rgb)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap), PublicResources.ArgumentNull);
            if (gamma < 0f || gamma > 10f || Single.IsNaN(gamma))
                throw new ArgumentOutOfRangeException(nameof(gamma), PublicResources.ArgumentMustBeBetween(0f, 10f));
            if (!channels.AllFlagsDefined())
                throw new ArgumentOutOfRangeException(nameof(channels), PublicResources.FlagsEnumOutOfRange(channels));

            // ReSharper disable once CompareOfFloatsByEqualityOperator - 1 has a precise float representation
            if (channels == ColorChannels.None || gamma == 1f)
                return;

            byte[] table = gammaLookupTableCache[gamma];

            Color32 Transform(Color32 c) => new Color32(c.A,
                (channels & ColorChannels.R) == ColorChannels.R ? table[c.R] : c.R,
                (channels & ColorChannels.G) == ColorChannels.G ? table[c.G] : c.G,
                (channels & ColorChannels.B) == ColorChannels.B ? table[c.B] : c.B);

            bitmap.TransformColors(Transform, ditherer);
        }

        #endregion


        #region Internal Methods

        /// <summary>
        /// Tries to extract the icon images from an image.
        /// </summary>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Non-disposed bitmaps are returned.")]
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

        #endregion

        #region Private Methods

        private static ICollection<Color32> DoGetColors(Bitmap bitmap, int maxColors)
        {
            if (maxColors < 0)
                throw new ArgumentOutOfRangeException(nameof(maxColors), PublicResources.ArgumentOutOfRange);
            if (maxColors == 0)
                maxColors = Int32.MaxValue;

            var colors = new HashSet<Color32>();
            using (BitmapDataAccessorBase data = BitmapDataAccessorFactory.CreateAccessor(bitmap, ImageLockMode.ReadOnly))
            {
                BitmapDataRowBase line = data.GetRow(0);

                do
                {
                    for (int x = 0; x < data.Width; x++)
                    {
                        Color32 c = line.DoGetColor32(x);
                        colors.Add(c.A == 0 ? Color32.Transparent : c);
                        if (colors.Count == maxColors)
                            return colors;
                    }
                } while (line.MoveNextRow());
            }

            return colors;
        }

        private static void ClearDirect(BitmapDataAccessorBase bitmapData, Color32 color)
        {
            BitmapDataRowBase row;
            int bpp = bitmapData.PixelFormat.ToBitsPerPixel();
            switch (bpp)
            {
                // premultiplying only once (if needed), clearing by longs
                case 32:
                    row = bitmapData.GetRow(0);
                    row.DoSetColor32(0, color);
                    var rawColor = row.DoReadRaw<Color32>(0);
                    int longWidth = bitmapData.Stride >> 3;

                    // writing as longs
                    if (longWidth > 0)
                    {
                        uint argb = (uint)rawColor.ToArgb();
                        ClearRaw(bitmapData, longWidth, ((ulong)argb << 32) | argb);
                    }

                    // if width is odd we clear the last column
                    if ((bitmapData.Width & 1) == 1)
                    {
                        int x = bitmapData.Width - 1;
                        row = bitmapData.GetRow(0);
                        do
                        {
                            row.DoWriteRaw(x, rawColor);
                        } while (row.MoveNextRow());
                    }

                    return;

                // converting color to 64-bit [P]ARGB value only once
                case 64:
                    row = bitmapData.GetRow(0);
                    row.DoSetColor32(0, color);
                    var longValue = row.DoReadRaw<long>(0);
                    ClearRaw(bitmapData, bitmapData.Width, longValue);
                    return;

                // converting color to underlying 16 bit pixel only once, clearing by longs
                case 16:
                    row = bitmapData.GetRow(0);
                    row.DoSetColor32(0, color);
                    var shortValue = row.DoReadRaw<ushort>(0);
                    longWidth = bitmapData.Stride >> 3;
                    uint uintValue = (uint)((shortValue << 16) | shortValue);

                    // writing as longs
                    if (longWidth > 0)
                        ClearRaw(bitmapData, longWidth, ((ulong)uintValue << 32) | uintValue);

                    // if stride can be divided by 8, then we are done
                    if ((bitmapData.Stride & 0b111) == 0)
                        return;

                    // otherwise, we clear the last 1..3 columns (on Windows: 1..2 because stride always can be divided by 4)
                    int to = bitmapData.Width;
                    int from = to - (bitmapData.Width & 0b11);
                    row = bitmapData.GetRow(0);
                    do
                    {
                        for (int x = from; x < to; x++)
                            row.DoWriteRaw(x, shortValue);
                    } while (row.MoveNextRow());

                    return;

                // converting color to palette index only once
                case 8:
                case 4:
                case 1:
                    int index = bitmapData.Palette.GetNearestColorIndex(color);
                    byte byteValue = bpp == 8 ? (byte)index
                        : bpp == 4 ? (byte)((index << 4) | index)
                        : index == 1 ? Byte.MaxValue : Byte.MinValue;

                    // writing as 32-bit integers (on Windows Stride is always the multiple of 4)
                    if ((bitmapData.Stride & 0b11) == 0)
                        ClearRaw(bitmapData, bitmapData.Stride >> 2, (byteValue << 24) | (byteValue << 16) | (byteValue << 8) | byteValue);
                    // fallback: writing as bytes (will not occur on Windows)
                    else
                        ClearRaw(bitmapData, bitmapData.Stride, byteValue);
                    return;

                // Direct color-based clear (24/48 bit formats)
                default:
                    // small width: going with sequential clear
                    if (bitmapData.Width < parallelThreshold)
                    {
                        row = bitmapData.GetRow(0);
                        do
                        {
                            for (int x = 0; x < bitmapData.Width; x++)
                                row.DoSetColor32(x, color);
                        } while (row.MoveNextRow());

                        return;
                    }

                    // parallel clear
                    ParallelHelper.For(0, bitmapData.Height, y =>
                    {
                        BitmapDataRowBase row = bitmapData.GetRow(y);
                        for (int x = 0; x < bitmapData.Width; x++)
                            row.DoSetColor32(x, color);
                    });
                    return;
            }
        }

        private static void ClearRaw<T>(BitmapDataAccessorBase bitmapData, int width, T data)
            where T : unmanaged
        {
            // small width: going with sequential clear
            if (width < parallelThreshold)
            {
                var row = bitmapData.GetRow(0);
                do
                {
                    for (int x = 0; x < width; x++)
                        row.DoWriteRaw(x, data);
                } while (row.MoveNextRow());
                return;
            }

            // parallel clear
            ParallelHelper.For(0, bitmapData.Height, y =>
            {
                BitmapDataRowBase row = bitmapData.GetRow(y);
                for (int x = 0; x < width; x++)
                    row.DoWriteRaw(x, data);
            });
        }

        private static void ClearWithDithering(BitmapDataAccessorBase bitmapData, Color32 color, IDitherer ditherer)
        {
            IQuantizer quantizer = PredefinedColorsQuantizer.FromBitmapData(bitmapData);
            using (IQuantizingSession quantizingSession = quantizer.Initialize(bitmapData))
            using (IDitheringSession ditheringSession = ditherer.Initialize(bitmapData, quantizingSession) ?? throw new InvalidOperationException(Res.ImagingDithererInitializeNull))
            {
                // sequential clear
                if (ditheringSession.IsSequential || bitmapData.Width < parallelThreshold)
                {
                    BitmapDataRowBase row = bitmapData.GetRow(0);
                    int y = 0;
                    do
                    {
                        for (int x = 0; x < bitmapData.Width; x++)
                            row.DoSetColor32(x, ditheringSession.GetDitheredColor(color, x, y));
                        y += 1;
                    } while (row.MoveNextRow());

                    return;
                }

                // parallel clear
                ParallelHelper.For(0, bitmapData.Height, y =>
                {
                    BitmapDataRowBase row = bitmapData.GetRow(y);
                    for (int x = 0; x < bitmapData.Width; x++)
                        row.DoSetColor32(x, ditheringSession.GetDitheredColor(color, x, y));
                });
            }
        }

        private static byte[] GenerateGammaLookupTable(float gamma)
        {
            byte[] result = new byte[256];
            for (int i = 0; i < 256; i++)
                result[i] = ((int)(255d * Math.Pow(i / 255d, 1d / gamma) + 0.5d)).ClipToByte();
            return result;
        }

        #endregion

        #endregion
    }
}
