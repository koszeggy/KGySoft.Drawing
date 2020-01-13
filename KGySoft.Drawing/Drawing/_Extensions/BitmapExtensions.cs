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
        #region Fields

        private static readonly int[] iconSizes = { 512, 384, 320, 256, 128, 96, 80, 72, 64, 60, 48, 40, 36, 32, 30, 24, 20, 16, 8, 4 };

        #endregion

        #region Methods

        #region Public Methods

        /// <summary>
        /// Resizes the image with high quality. The result is always a 32 bit ARGB image.
        /// </summary>
        /// <param name="image">The original image to resize</param>
        /// <param name="newSize">The requested new size.</param>
        /// <param name="keepAspectRatio">Determines whether the source <paramref name="image"/> should keep aspect ratio.</param>
        /// <returns>A <see cref="Bitmap"/> instance with the new size.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The result must not be disposed.")]
        public static Bitmap Resize(this Bitmap image, Size newSize, bool keepAspectRatio)
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
            using (Graphics g = Graphics.FromImage(result))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.DrawImage(image, new Rectangle(targetLocation, targetSize), new Rectangle(Point.Empty, sourceSize), GraphicsUnit.Pixel);
                g.Flush();

                return result;
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
#if !NET35
        [SecuritySafeCritical]
#endif
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
                    }
                    else
                    {
                        int lineWidth = Math.Abs(sourceData.Stride);
                        for (int y = 0; y < sourceData.Height; y++)
                        {
                            MemoryHelper.CopyMemory(lineTarget, lineSource, lineWidth);
                            lineSource = new IntPtr(lineSource.ToInt64() + sourceData.Stride);
                            lineTarget = new IntPtr(lineTarget.ToInt64() + targetData.Stride);
                        }
                    }
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

            return result;
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
#if !NET35
        [SecuritySafeCritical]
#endif
        public static Color[] GetColors(this Bitmap bitmap, int maxColors = 0, bool forceScanningContent = false)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap), PublicResources.ArgumentNull);
            if (maxColors < 0)
                throw new ArgumentOutOfRangeException(nameof(maxColors), PublicResources.ArgumentOutOfRange);
            if (maxColors == 0)
                maxColors = Int32.MaxValue;

            var colors = new HashSet<int>();
            PixelFormat pixelFormat = bitmap.PixelFormat;
            if (pixelFormat.IsIndexed() && !forceScanningContent)
                return bitmap.Palette.Entries;

            int transparent = Color.Transparent.ToArgb();
            using (BitmapDataAccessorBase data = BitmapDataAccessorFactory.CreateAccessor(bitmap, ImageLockMode.ReadOnly))
            {
                BitmapDataRowBase line = data.GetRow(0);

                do
                {
                    for (int x = 0; x < data.Width; x++)
                    {
                        Color32 c = line.DoGetColor32(x);
                        colors.Add(c.A == 0 ? transparent : c.ToArgb());
                        if (colors.Count == maxColors)
                            return colors.Select(Color.FromArgb).ToArray();
                    }
                } while (line.MoveNextRow());
            }

            return colors.Select(Color.FromArgb).ToArray();
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
        public static CursorHandle ToCursorHandle(this Bitmap bitmap, Point cursorHotspot = default)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap), PublicResources.ArgumentNull);
            if (!OSUtils.IsWindows)
                throw new PlatformNotSupportedException(Res.RequiresWindows);

            IntPtr iconHandle = bitmap.GetHicon();
            try
            {
                return Icons.ToCursorHandle(iconHandle, cursorHotspot);
            }
            finally
            {
                User32.DestroyIcon(iconHandle);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="lockMode"></param>
        /// <param name="backColor">When setting pixels of indexed bitmaps, bitmaps without alpha support or with single bit alpha, then specifies the
        /// background color, which will be bended with color to set. The alpha value (<see cref="Color.A"/> property) of the specified background color is ignored. This parameter is optional.
        /// <br/>Default value: <see cref="Color.Empty">Color.Empty</see>, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="alphaThreshold">When setting pixels of bitmaps with single bit alpha or with a palette that has transparent color, then determines the lowest
        /// alpha value of a color, which should not be considered as transparent. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <returns></returns>
        /// <remarks>
        /// <para>If <paramref name="omitTrueAndDeepColorTransformations"/> is <see langword="false"/>, then the following transformations are performed when getting and setting pixels of bitmaps with specific pixel formats:
        /// <list type="definition">
        /// <item><term><see cref="PixelFormat.Format32bppPArgb"/></term><description>When pixels are set, color channels are premultiplied with alpha channel. When pixels are read, converting premultiplied colors to straight ones.</description></item>
        /// <item><term><see cref="PixelFormat.Format48bppRgb"/></term><description>When pix Transforming to and from 13-bit color component range is omitted.</description></item>
        /// <item><term><see cref="PixelFormat.Format64bppArgb"/></term><description>Transforming to and from 13-bit color component range is omitted.</description></item>
        /// <item><term><see cref="PixelFormat.Format64bppPArgb"/></term><description>Converting to premultiplied/straight color and transforming to and from 13-bit color component range are omitted.</description></item>
        /// <item><term>All other pixel formats</term><description>The value of the <paramref name="omitTrueAndDeepColorTransformations"/> parameter is ignored.</description></item>
        /// </list>
        /// <note>Even if the <paramref name="omitTrueAndDeepColorTransformations"/> parameter is <see langword="false"/> the actual underlying data can be read and written
        /// by the <see cref="IBitmapDataRow.ReadRaw{T}"><![CDATA[IBitmapDataRow.ReadDataDirect<T>]]></see> and <see cref="IBitmapDataRow.WriteRaw{T}"><![CDATA[IBitmapDataRow.WriteDataDirect<T>]]></see> methods.</note></para>
        /// <para>If <paramref name="omitTrueAndDeepColorTransformations"/> is <see langword="true"/>, then for the best performance setting pixels of deep color formats by the
        /// <see cref="IBitmapDataRow.SetPixelColor(int,Color64)">IBitmapDataRow.SetPixelColor(int,Color64)</see> method will transform the color components of the <see cref="Color64"/> structure to a 13-bit range by using a lookup table of 256 values.
        /// In order to preserve all the possible 8192 shades per color component either set the <paramref name="omitTrueAndDeepColorTransformations"/> to <see langword="true"/>&#160;or use the
        /// <see cref="IBitmapDataRow.WriteRaw{T}"><![CDATA[IBitmapDataRow.WriteDataDirect<T>]]></see> method with <see cref="Color64"/> type parameter and make sure that all
        /// color components are in the 0..8192 range.
        /// </para>
        ///
        /// <para>If <paramref name="alphaThreshold"/> is zero, then setting a pixel of a bitmap with indexed or single-bit-alpha pixel format
        /// by a fully transparent pixel will be blended by <paramref name="backColor"/> even if the bitmap can handle transparent pixels.</para>
        /// <para>If <paramref name="alphaThreshold"/> is <c>1</c>, then setting a pixel of a bitmap with indexed or single-bit-alpha pixel format
        /// will be transparent only if the color to set is completely transparent (has zero alpha).</para>
        /// <para>If <paramref name="alphaThreshold"/> is <c>255</c>, then setting a pixel of a bitmap with indexed or single-bit-alpha pixel format
        /// will be opaque only if the color to set is completely opaque (its alpha value is <c>255</c>).</para>
        /// <para>If a pixel of a bitmap without alpha gradient support is set by the <see cref="IBitmapDataAccessor.SetPixel">IBitmapDataAccessor.SetPixel</see>/<see cref="IBitmapDataRow.SetColor">IBitmapDataRow.SetColor</see>
        /// methods or by the <see cref="IBitmapDataRow.this">IBitmapDataRow indexer</see>, and the pixel has an alpha value that is greater than <paramref name="alphaThreshold"/>,
        /// then the pixel to set will be blended by <paramref name="backColor"/>.</para>
        /// </remarks>
        public static IBitmapDataAccessor GetBitmapDataAccessor(this Bitmap bitmap, ImageLockMode lockMode/*, Color backColor = default, byte alphaThreshold = 128*/)
            => BitmapDataAccessorFactory.CreateAccessor(bitmap, lockMode);

        public static void Quantize(this Bitmap bitmap, IQuantizer quantizer)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap), PublicResources.ArgumentNull);
            if (quantizer == null)
                throw new ArgumentNullException(nameof(quantizer), PublicResources.ArgumentNull);

            using (IBitmapDataAccessor bitmapData = bitmap.GetBitmapDataAccessor(ImageLockMode.ReadWrite))
            using (IQuantizingSession session = quantizer.Initialize(bitmapData))
            {
                if (session == null)
                    throw new InvalidOperationException(Res.ImagingQuantizerInitializeNull);
                
                // TODO: parallel
                IBitmapDataRow row = bitmapData.FirstRow;
                int width = bitmapData.Width;
                do
                {
                    for (int x = 0; x < width; x++)
                        row[x] = session.GetQuantizedColor(row[x]);
                } while (row.MoveNextRow());
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

            using (IBitmapDataAccessor bitmapData = bitmap.GetBitmapDataAccessor(ImageLockMode.ReadWrite))
            using (IQuantizingSession quantizingSession = quantizer.Initialize(bitmapData) ?? throw new InvalidOperationException(Res.ImagingQuantizerInitializeNull))
            using (IDitheringSession ditheringSession = ditherer.Initialize(bitmapData, quantizingSession) ?? throw new InvalidOperationException(Res.ImagingDithererInitializeNull))
            {
                // TODO: parallel if possible
                IBitmapDataRow row = bitmapData.FirstRow;
                int width = bitmapData.Width;
                int y = 0;
                do
                {
                    for (int x = 0; x < width; x++)
                        row[x] = ditheringSession.GetDitheredColor(row[x], x, y);

                    y += 1;
                } while (row.MoveNextRow());
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Tries to extract the icon images from an image.
        /// </summary>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Non-disposed bitmaps are returned.")]
        private static Bitmap[] ExtractIconImages(Bitmap image)
        {
            Debug.Assert(image.RawFormat.Guid == ImageFormat.Icon.Guid);

            // // first try: trying to save it officially (does not work if there is no icon encoder registered in OS)
            // ImageCodecInfo iconEncoder = ImageCodecInfo.GetImageEncoders().FirstOrDefault(enc => enc.FormatID == ImageFormat.Icon.Guid);
            // if (iconEncoder != null)
            // {
            //    using (var ms = new System.IO.MemoryStream())
            //    {
            //        image.Save(ms, iconEncoder, null);
            //        ms.Position = 0L;
            //        return new Icon(ms).ExtractBitmaps(false);
            //    }
            // }

            // second try: guessing by official sizes (every size will be extracted with the same pixel format)
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

        #endregion
    }
}
