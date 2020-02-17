#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: IconExtensions.cs
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
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Security;

using KGySoft.Drawing.WinApi;

#endregion

namespace KGySoft.Drawing
{
    /// <summary>
    /// Contains extension methods for the <see cref="Icon"/> type.
    /// </summary>
    public static class IconExtensions
    {
        #region Methods

        #region Public Methods

        /// <summary>
        /// Converts the specified <paramref name="icon"/> to a <see cref="Bitmap"/>. While <see cref="Icon.ToBitmap">Icon.ToBitmap</see> may return a wrong result
        /// when icon contains semi-transparent pixels, this method returns an image, in which alpha channel
        /// is always correctly applied for the image.
        /// </summary>
        /// <param name="icon">The icon optionally with transparency.</param>
        /// <returns>A <see cref="Bitmap"/> that represents the converted <see cref="Icon"/>.</returns>
        /// <remarks>
        /// <para>If <paramref name="icon"/> is not from a native handle, then this method calls the <see cref="ExtractNearestBitmap">ExtractBitmap</see> method using the icon size
        /// and <see cref="PixelFormat.Format32bppArgb"/> pixel format as parameters.</para>
        /// <para>If the <paramref name="icon"/> contains multiple images consider to use either the <see cref="O:KGySoft.Drawing.IconExtensions.ExtractBitmap">ExtractBitmap</see>
        /// or <see cref="ExtractNearestBitmap">ExtractBitmap</see> methods to specify the exact image to return,
        /// or the <see cref="O:KGySoft.Drawing.IconExtensions.ToMultiResBitmap">ToMultiResBitmap</see> methods, which return every images in a single combined <see cref="Bitmap"/>.</para>
        /// </remarks>
        /// <seealso cref="O:KGySoft.Drawing.IconExtensions.ExtractBitmap"/>
#if !NET35
        [SecuritySafeCritical]
#endif
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The result must not be disposed.")]
        public static Bitmap ToAlphaBitmap(this Icon icon)
        {
            if (icon == null)
                throw new ArgumentNullException(nameof(icon), PublicResources.ArgumentNull);

            if (icon.HasRawData())
                return icon.ExtractNearestBitmap(icon.Size, PixelFormat.Format32bppArgb);
            if (!OSUtils.IsWindows)
                return icon.ToBitmap();

            User32.GetIconInfo(icon.Handle, out ICONINFO iconInfo);
            try
            {
                // Getting color depth by GDI. (FromHbitmap always returns 32 bppRgb format)
                // The possible 1 bit transparency is handled by ToBitmap. The code below would return a fully transparent bitmap for non-ARGB sources.
                // GetBitmapColorDepth actually returns the current display BPP for HICONs.
                if (Gdi32.GetBitmapColorDepth(iconInfo.hbmColor) < 32)
                    return icon.ToBitmap();

                // The result bitmap has now black pixels where the icon was transparent
                // because the returned pixel format is Format32bppRgb without alpha.
                Bitmap bmpColor = Image.FromHbitmap(iconInfo.hbmColor);
                Bitmap bmpRedirected = null;

                try
                {
                    // Mapping result data into a new destination bitmap where pixel format is ARGB so background will not be black anymore
                    Rectangle bounds = new Rectangle(0, 0, bmpColor.Width, bmpColor.Height);
                    BitmapData dataColor = bmpColor.LockBits(bounds, ImageLockMode.ReadOnly, bmpColor.PixelFormat);
                    bmpRedirected = new Bitmap(dataColor.Width, dataColor.Height, dataColor.Stride, PixelFormat.Format32bppArgb, dataColor.Scan0);
                    BitmapData dataRedirected = null;
                    try
                    {
                        dataRedirected = bmpRedirected.LockBits(bounds, ImageLockMode.ReadOnly, bmpRedirected.PixelFormat);
                    }
                    finally
                    {
                        bmpColor.UnlockBits(dataColor);
                        if (dataRedirected != null)
                            bmpRedirected.UnlockBits(dataRedirected);
                    }

                    // Cloning result bitmap because original destination uses a temporary memory area
                    // If bmpRedirected was returned, its image could be corrupted at each refresh
                    Bitmap result = new Bitmap(bmpRedirected);
                    return result;
                }
                finally
                {
                    bmpColor.Dispose();
                    bmpRedirected?.Dispose();
                }
            }
            finally
            {
                Gdi32.DeleteObject(iconInfo.hbmColor);
                Gdi32.DeleteObject(iconInfo.hbmMask);
            }
        }

        /// <summary>
        /// Converts the <paramref name="icon"/> to a <see cref="Bitmap"/> instance, which contains every image of the <paramref name="icon"/>.
        /// When the returned <see cref="Bitmap"/> is used to create another <see cref="Bitmap"/> or is drawn into a <see cref="Graphics"/>, the best-fitting image is automatically applied.
        /// </summary>
        /// <param name="icon">The icon to convert to a multi-resolution <see cref="Bitmap"/>.</param>
        /// <returns>A <see cref="Bitmap"/> instance, which contains every image of the <paramref name="icon"/>.</returns>
        /// <remarks>
        /// <para>If the method is executed in a Windows XP or non-Windows environment, the result <see cref="Bitmap"/> will contain only uncompressed images.</para>
        /// <para>Windows XP may display alpha channel incorrectly (semi-transparent pixels may be black).</para>
        /// <note>On some platforms this method may throw a <see cref="PlatformNotSupportedException"/> if <paramref name="icon"/> contains only a very large icon.</note>
        /// </remarks>
#if !NET35
        [SecuritySafeCritical]
#endif
        public static Bitmap ToMultiResBitmap(this Icon icon)
        {
            if (!OSUtils.IsWindows || OSUtils.IsVistaOrLater)
                // Forcing BMP only images also on Linux to prevent possible OutOfMemoryException from the Bitmap constructor
                return ToMultiResBitmap(icon, !OSUtils.IsWindows);

            // In Windows XP replacing 24 bit icons by 32 bit ones to prevent "Parameter is invalid" error in Bitmap ctor.
            using (var result = new RawIcon())
            {
                foreach (Icon iconImage in icon.ExtractIcons())
                {
                    using (iconImage)
                    {
                        if (iconImage.GetBitsPerPixel() == 24)
                        {
                            using (var bmp = iconImage.ToAlphaBitmap())
                                result.Add(bmp);
                        }
                        else
                            result.Add(iconImage);
                    }
                }

                return result.ToBitmap(true);
            }
        }

        /// <summary>
        /// Converts the <paramref name="icon"/> to a <see cref="Bitmap"/> instance, which contains every image of the <paramref name="icon"/>.
        /// When the returned <see cref="Bitmap"/> is used to create another <see cref="Bitmap"/> or is drawn into a <see cref="Graphics"/>, the best-fitting image is automatically applied.
        /// </summary>
        /// <param name="icon">The icon to convert to a multi-resolution <see cref="Bitmap"/>.</param>
        /// <param name="forceUncompressedResult"><see langword="true"/>&#160;to force returning a <see cref="Bitmap"/> instance with non-compressed images only;
        /// <see langword="false"/>&#160;to allow compressing larger images by PNG encoding, which is supported by Windows Vista and above.</param>
        /// <remarks>
        /// <note>On some platforms this method may throw a <see cref="PlatformNotSupportedException"/> if <paramref name="icon"/> contains only a very large icon.</note>
        /// </remarks>
        /// <returns>A <see cref="Bitmap"/> instance, which contains every image of the <paramref name="icon"/>.</returns>
#if !NET35
        [SecuritySafeCritical]
#endif
        public static Bitmap ToMultiResBitmap(this Icon icon, bool forceUncompressedResult)
        {
            using (RawIcon rawIcon = new RawIcon(icon))
                return rawIcon.ToBitmap(forceUncompressedResult);
        }

        /// <summary>
        /// Gets the number of images in the <paramref name="icon"/>.
        /// </summary>
        /// <param name="icon">The icon to check.</param>
        /// <returns>The number of images in the <paramref name="icon"/>.</returns>
#if !NET35
        [SecuritySafeCritical]
#endif
        public static int GetImagesCount(this Icon icon)
        {
            using (RawIcon rawIcon = new RawIcon(icon))
                return rawIcon.ImageCount;
        }

        /// <summary>
        /// Extracts every image from an <see cref="Icon" /> instance.
        /// </summary>
        /// <param name="icon">The icon that may contain multiple images.</param>
        /// <param name="keepOriginalFormat">If <see langword="true"/>, keeps the original image format stored in the <paramref name="icon"/>. Possible transparent pixels of
        /// non-32 bpp ARGB formats may be black. If <see langword="false"/>, always returns 32 bpp images with transparency. This parameter is optional.
        /// <br/>Default value: <see langword="false"/>.</param>
        /// <returns>
        /// An array of <see cref="Bitmap" /> instances, which were extracted from the <paramref name="icon" />.
        /// </returns>
        /// <remarks>
        /// <note>On some platforms the result may contain <see langword="null"/>&#160;elements in place of very large uncompressed icons.</note>
        /// </remarks>
        /// <seealso cref="ExtractIcons(Icon)" />
        public static Bitmap[] ExtractBitmaps(this Icon icon, bool keepOriginalFormat = false) => ExtractBitmaps(icon, null, null, keepOriginalFormat);

        /// <summary>
        /// Extracts every image of specified size from an <see cref="Icon"/> instance.
        /// </summary>
        /// <param name="icon">The icon that may contain multiple images.</param>
        /// <param name="size">The required icon size to retrieve.</param>
        /// <param name="keepOriginalFormat">If <see langword="true"/>, keeps the original image format stored in the <paramref name="icon"/>. Possible transparent pixels of
        /// non-32 bpp ARGB formats may be black. If <see langword="false"/>, always returns 32 bpp images with transparency. This parameter is optional.
        /// <br/>Default value: <see langword="false"/>.</param>
        /// <returns>An array of <see cref="Bitmap"/> instances, which were extracted from the <paramref name="icon"/>.</returns>
        /// <remarks>
        /// <note>On some platforms the result may contain <see langword="null"/>&#160;elements in place of very large uncompressed icons.</note>
        /// </remarks>
        /// <seealso cref="ExtractIcons(Icon,Size)"/>
        public static Bitmap[] ExtractBitmaps(this Icon icon, Size size, bool keepOriginalFormat = false) => ExtractBitmaps(icon, size, null, keepOriginalFormat);

        /// <summary>
        /// Extracts every image of specified pixel format from an <see cref="Icon"/> instance.
        /// </summary>
        /// <param name="icon">The icon that may contain multiple images.</param>
        /// <param name="pixelFormat">The required pixel format to retrieve</param>
        /// <param name="keepOriginalFormat">If <see langword="true"/>, keeps the original image format stored in the <paramref name="icon"/>. Possible transparent pixels of
        /// non-32 bpp ARGB formats may be black. If <see langword="false"/>, always returns 32 bpp images with transparency. This parameter is optional.
        /// <br/>Default value: <see langword="false"/>.</param>
        /// <returns>An array of <see cref="Bitmap"/> instances, which were extracted from the <paramref name="icon"/>.</returns>
        /// <remarks>
        /// <note>On some platforms the result may contain <see langword="null"/>&#160;elements in place of very large uncompressed icons.</note>
        /// </remarks>
        /// <seealso cref="ExtractIcons(Icon,PixelFormat)"/>
        public static Bitmap[] ExtractBitmaps(this Icon icon, PixelFormat pixelFormat, bool keepOriginalFormat = false)
            => ExtractBitmaps(icon, null, pixelFormat.ToBitsPerPixel(), keepOriginalFormat);

        /// <summary>
        /// Extracts the first image from an <see cref="Icon"/> instance. If the icon has only one image consider to use
        /// <see cref="ToAlphaBitmap">ToAlphaBitmap</see>, which is faster.
        /// </summary>
        /// <param name="icon">The icon that may contain multiple images.</param>
        /// <param name="keepOriginalFormat">If <see langword="true"/>, keeps the original image format stored in the <paramref name="icon"/>. Possible transparent pixels of
        /// non-32 bpp ARGB formats may be black. If <see langword="false"/>, always returns 32 bpp images with transparency. This parameter is optional.
        /// <br/>Default value: <see langword="false"/>.</param>
        /// <returns>An <see cref="Bitmap"/> instance, which was extracted from the <paramref name="icon"/>,
        /// or <see langword="null"/>&#160;if no image was found in the <paramref name="icon"/>.</returns>
        /// <remarks>
        /// <note>On some platforms this method may throw a <see cref="PlatformNotSupportedException"/> for very large uncompressed icons.</note>
        /// </remarks>
        /// <seealso cref="ToAlphaBitmap"/>
        public static Bitmap ExtractBitmap(this Icon icon, bool keepOriginalFormat = false) => ExtractFirstBitmap(icon, null, null, keepOriginalFormat);

        /// <summary>
        /// Extracts the first image of specified size from an <see cref="Icon"/> instance.
        /// </summary>
        /// <param name="icon">The icon that may contain multiple images.</param>
        /// <param name="size">The required icon size to retrieve.</param>
        /// <param name="keepOriginalFormat">If <see langword="true"/>, keeps the original image format stored in the <paramref name="icon"/>. Possible transparent pixels of
        /// non-32 bpp ARGB formats may be black. If <see langword="false"/>, always returns 32 bpp images with transparency. This parameter is optional.
        /// <br/>Default value: <see langword="false"/>.</param>
        /// <returns>An <see cref="Bitmap"/> instance, which was extracted from the <paramref name="icon"/>,
        /// or <see langword="null"/>&#160;if no icon found with the specified size.</returns>
        /// <remarks>
        /// <note>On some platforms this method may throw a <see cref="PlatformNotSupportedException"/> for very large uncompressed icons.</note>
        /// </remarks>
        /// <seealso cref="ExtractIcon(Icon,Size)"/>
        public static Bitmap ExtractBitmap(this Icon icon, Size size, bool keepOriginalFormat = false) => ExtractFirstBitmap(icon, size, null, keepOriginalFormat);

        /// <summary>
        /// Extracts the image of specified size and pixel format from an <see cref="Icon"/> instance.
        /// </summary>
        /// <param name="icon">The icon that may contain multiple images.</param>
        /// <param name="size">The required icon size to retrieve.</param>
        /// <param name="pixelFormat">The required pixel format to retrieve</param>
        /// <param name="keepOriginalFormat">If <see langword="true"/>, keeps the original image format stored in the <paramref name="icon"/>. Possible transparent pixels of
        /// non-32 bpp ARGB formats may be black. If <see langword="false"/>, always returns 32 bpp images with transparency. This parameter is optional.
        /// <br/>Default value: <see langword="false"/>.</param>
        /// <returns>An <see cref="Bitmap"/> instance, which was extracted from the <paramref name="icon"/>,
        /// or <see langword="null"/>&#160;if no icon found with the specified size and format.</returns>
        /// <remarks>
        /// <note>On some platforms this method may throw a <see cref="PlatformNotSupportedException"/> for very large uncompressed icons.</note>
        /// </remarks>
        /// <seealso cref="ExtractIcon(Icon,Size,PixelFormat)"/>
        public static Bitmap ExtractBitmap(this Icon icon, Size size, PixelFormat pixelFormat, bool keepOriginalFormat = false)
            => ExtractFirstBitmap(icon, size, pixelFormat.ToBitsPerPixel(), keepOriginalFormat);

        /// <summary>
        /// Extracts the image of specified index from an <see cref="Icon"/> instance.
        /// </summary>
        /// <param name="icon">The icon that may contain multiple images.</param>
        /// <param name="index">The zero-based index of the icon image to retrieve.</param>
        /// <param name="keepOriginalFormat">If <see langword="true"/>, keeps the original image format stored in the <paramref name="icon"/>. Possible transparent pixels of
        /// non-32 bpp ARGB formats may be black. If <see langword="false"/>, always returns 32 bpp images with transparency. This parameter is optional.
        /// <br/>Default value: <see langword="false"/>.</param>
        /// <returns>An <see cref="Bitmap"/> instance, which was extracted from the <paramref name="icon"/>,
        /// or <see langword="null"/>&#160;if the specified <paramref name="index"/> was too large.</returns>
        /// <remarks>
        /// <note>On some platforms this method may throw a <see cref="PlatformNotSupportedException"/> for very large uncompressed icons.</note>
        /// </remarks>
        /// <seealso cref="ExtractIcon(Icon,int)"/>
#if !NET35
        [SecuritySafeCritical]
#endif
        public static Bitmap ExtractBitmap(this Icon icon, int index, bool keepOriginalFormat = false)
        {
            if (icon == null)
                throw new ArgumentNullException(nameof(icon), PublicResources.ArgumentNull);
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), PublicResources.ArgumentMustBeGreaterThanOrEqualTo(0));

            using (RawIcon rawIcon = new RawIcon(icon, null, null, index))
                return index >= rawIcon.ImageCount ? null : rawIcon.ExtractBitmap(index, keepOriginalFormat);
        }

        /// <summary>
        /// Extracts the nearest image of specified size and pixel format from an <see cref="Icon"/> instance.
        /// </summary>
        /// <param name="icon">The icon that may contain multiple images.</param>
        /// <param name="size">The required icon size to retrieve.</param>
        /// <param name="pixelFormat">The required pixel format to retrieve</param>
        /// <param name="keepOriginalFormat">If <see langword="true"/>, keeps the original image format stored in the <paramref name="icon"/>. Possible transparent pixels of
        /// non-32 bpp ARGB formats may be black. If <see langword="false"/>, always returns 32 bpp images with transparency. This parameter is optional.
        /// <br/>Default value: <see langword="false"/>.</param>
        /// <returns>An <see cref="Bitmap"/> instance, which was extracted from the <paramref name="icon"/>. If no
        /// icon was found with the specified size and format the nearest image (<paramref name="pixelFormat"/> matches first, then <paramref name="size"/>) is returned.</returns>
        /// <remarks>
        /// <para>On some platforms it may happen that a smaller result is returned than requested if the requested size and format is not supported.</para>
        /// </remarks>
        /// <seealso cref="ExtractNearestIcon(Icon,Size,PixelFormat)"/>
#if !NET35
        [SecuritySafeCritical]
#endif
        public static Bitmap ExtractNearestBitmap(this Icon icon, Size size, PixelFormat pixelFormat, bool keepOriginalFormat = false)
        {
            if (icon == null)
                throw new ArgumentNullException(nameof(icon), PublicResources.ArgumentNull);
            int bpp = pixelFormat.ToBitsPerPixel();

            using (RawIcon rawIcon = new RawIcon(icon))
                return rawIcon.ExtractNearestBitmap(bpp, size, keepOriginalFormat);
        }

        /// <summary>
        /// Extracts every icon from an <see cref="Icon"/> instance as separated <seealso cref="Icon"/> instances.
        /// </summary>
        /// <param name="icon">The icon that may contain multiple images.</param>
        /// <returns>An array of <see cref="Icon"/> instances, which were extracted from the <paramref name="icon"/>.</returns>
        /// <remarks>
        /// <para>The result <see cref="Icon"/> instances are compatible with Windows XP if the method is executed in a Windows XP environment.</para>
        /// <note>On some platforms the result may contain <see langword="null"/>&#160;elements in place of very large icons.</note>
        /// </remarks>
        /// <seealso cref="ExtractBitmaps(Icon,bool)"/>
        public static Icon[] ExtractIcons(this Icon icon)
            => ExtractIcons(icon, null, null, OSUtils.IsXpOrEarlier || !OSUtils.IsWindows);

        /// <summary>
        /// Extracts every icon from an <see cref="Icon"/> instance as separated <seealso cref="Icon"/> instances.
        /// </summary>
        /// <param name="icon">The icon that may contain multiple images.</param>
        /// <param name="forceUncompressedResult"><see langword="true"/>&#160;to force returning uncompressed icons only;
        /// <see langword="false"/>&#160;to allow PNG compression, which is supported by Windows Vista and above.</param>
        /// <returns>An array of <see cref="Icon"/> instances, which were extracted from the <paramref name="icon"/>.</returns>
        /// <remarks>
        /// <note>On some platforms the result may contain <see langword="null"/>&#160;elements in place of very large icons.</note>
        /// </remarks>
        /// <seealso cref="ExtractBitmaps(Icon,bool)"/>
        public static Icon[] ExtractIcons(this Icon icon, bool forceUncompressedResult) => ExtractIcons(icon, null, null, forceUncompressedResult);

        /// <summary>
        /// Extracts every icon of specified size from an <see cref="Icon"/> instance as separated <seealso cref="Icon"/> instances.
        /// </summary>
        /// <param name="icon">The icon that may contain multiple images.</param>
        /// <param name="size">The required icon size to retrieve.</param>
        /// <returns>An array of <see cref="Icon"/> instances, which were extracted from the <paramref name="icon"/>.</returns>
        /// <remarks>
        /// <para>The result <see cref="Icon"/> instances are compatible with Windows XP if the method is executed in a Windows XP environment.</para>
        /// <note>On some platforms the result may contain <see langword="null"/>&#160;elements in place of very large icons.</note>
        /// </remarks>
        /// <seealso cref="ExtractBitmaps(Icon,Size,bool)"/>
        public static Icon[] ExtractIcons(this Icon icon, Size size)
            => ExtractIcons(icon, size, null, OSUtils.IsXpOrEarlier || !OSUtils.IsWindows);

        /// <summary>
        /// Extracts every icon of specified size from an <see cref="Icon"/> instance as separated <seealso cref="Icon"/> instances.
        /// </summary>
        /// <param name="icon">The icon that may contain multiple images.</param>
        /// <param name="size">The required icon size to retrieve.</param>
        /// <param name="forceUncompressedResult"><see langword="true"/>&#160;to force returning uncompressed icons only;
        /// <see langword="false"/>&#160;to allow PNG compression, which is supported by Windows Vista and above.</param>
        /// <returns>An array of <see cref="Icon"/> instances, which were extracted from the <paramref name="icon"/>.</returns>
        /// <remarks>
        /// <note>On some platforms the result may contain <see langword="null"/>&#160;elements in place of very large icons.</note>
        /// </remarks>
        /// <seealso cref="ExtractBitmaps(Icon,Size,bool)"/>
        public static Icon[] ExtractIcons(this Icon icon, Size size, bool forceUncompressedResult)
            => ExtractIcons(icon, size, null, forceUncompressedResult);

        /// <summary>
        /// Extracts every icon of specified pixel format from an <see cref="Icon"/> instance as separated <seealso cref="Icon"/> instances.
        /// </summary>
        /// <param name="icon">The icon that may contain multiple images.</param>
        /// <param name="pixelFormat">The required pixel format to retrieve</param>
        /// <returns>An array of <see cref="Icon"/> instances, which were extracted from the <paramref name="icon"/>.</returns>
        /// <remarks>
        /// <para>The result <see cref="Icon"/> instances are compatible with Windows XP if the method is executed in a Windows XP environment.</para>
        /// <note>On some platforms the result may contain <see langword="null"/>&#160;elements in place of very large icons.</note>
        /// </remarks>
        /// <seealso cref="ExtractBitmaps(Icon,PixelFormat,bool)"/>
        public static Icon[] ExtractIcons(this Icon icon, PixelFormat pixelFormat)
            => ExtractIcons(icon, null, pixelFormat.ToBitsPerPixel(), OSUtils.IsXpOrEarlier || !OSUtils.IsWindows);

        /// <summary>
        /// Extracts every icon of specified pixel format from an <see cref="Icon"/> instance as separated <seealso cref="Icon"/> instances.
        /// </summary>
        /// <param name="icon">The icon that may contain multiple images.</param>
        /// <param name="pixelFormat">The required pixel format to retrieve</param>
        /// <param name="forceUncompressedResult"><see langword="true"/>&#160;to force returning uncompressed icons only;
        /// <see langword="false"/>&#160;to allow PNG compression, which is supported by Windows Vista and above.</param>
        /// <returns>An array of <see cref="Icon"/> instances, which were extracted from the <paramref name="icon"/>.</returns>
        /// <remarks>
        /// <note>On some platforms the result may contain <see langword="null"/>&#160;elements in place of very large icons.</note>
        /// </remarks>
        /// <seealso cref="ExtractBitmaps(Icon,PixelFormat,bool)"/>
        public static Icon[] ExtractIcons(this Icon icon, PixelFormat pixelFormat, bool forceUncompressedResult)
            => ExtractIcons(icon, null, pixelFormat.ToBitsPerPixel(), forceUncompressedResult);

        /// <summary>
        /// Extracts the first icon of specified size from an <see cref="Icon"/> instance.
        /// Unless the <see cref="Icon"/> constructors, this method works as expected.
        /// </summary>
        /// <param name="icon">The icon that may contain multiple images.</param>
        /// <param name="size">The required icon size to retrieve.</param>
        /// <returns>An <see cref="Icon"/> instance, which contains only a single image,
        /// or <see langword="null"/>&#160;if no icon found with the specified size.</returns>
        /// <remarks>
        /// <para>The result <see cref="Icon"/> instances are compatible with Windows XP if the method is executed in a Windows XP environment.</para>
        /// <note>On some platforms this method may throw a <see cref="PlatformNotSupportedException"/> for very large icons.</note>
        /// </remarks>
        /// <seealso cref="ExtractBitmap(Icon,Size,bool)"/>
        public static Icon ExtractIcon(this Icon icon, Size size)
            => ExtractFirstIcon(icon, size, null, OSUtils.IsXpOrEarlier || !OSUtils.IsWindows);

        /// <summary>
        /// Extracts the first icon of specified size from an <see cref="Icon"/> instance.
        /// Unless the <see cref="Icon"/> constructors, this method works as expected.
        /// </summary>
        /// <param name="icon">The icon that may contain multiple images.</param>
        /// <param name="size">The required icon size to retrieve.</param>
        /// <param name="forceUncompressedResult"><see langword="true"/>&#160;to force returning an uncompressed icon;
        /// <see langword="false"/>&#160;to allow PNG compression, which is supported by Windows Vista and above.</param>
        /// <returns>An <see cref="Icon"/> instance, which contains only a single image,
        /// or <see langword="null"/>&#160;if no icon found with the specified size.</returns>
        /// <remarks>
        /// <note>On some platforms this method may throw a <see cref="PlatformNotSupportedException"/> for very large or compressed icons.</note>
        /// </remarks>
        /// <seealso cref="ExtractBitmap(Icon,Size,bool)"/>
        public static Icon ExtractIcon(this Icon icon, Size size, bool forceUncompressedResult) => ExtractFirstIcon(icon, size, null, forceUncompressedResult);

        /// <summary>
        /// Extracts the icon of specified size and pixel format from an <see cref="Icon"/> instance.
        /// Unless the <see cref="Icon"/> constructors, this method works as expected.
        /// </summary>
        /// <param name="icon">The icon that may contain multiple images.</param>
        /// <param name="size">The required icon size to retrieve.</param>
        /// <param name="pixelFormat">The required pixel format to retrieve</param>
        /// <returns>An <see cref="Icon"/> instance, which contains only a single image,
        /// or <see langword="null"/>&#160;if no icon found with the specified size and format.</returns>
        /// <remarks>
        /// <para>The result <see cref="Icon"/> instances are compatible with Windows XP if the method is executed in a Windows XP environment.</para>
        /// <note>On some platforms this method may throw a <see cref="PlatformNotSupportedException"/> for very large icons.</note>
        /// </remarks>
        /// <seealso cref="ExtractBitmap(Icon,Size,PixelFormat,bool)"/>
        public static Icon ExtractIcon(this Icon icon, Size size, PixelFormat pixelFormat)
            => ExtractFirstIcon(icon, size, pixelFormat.ToBitsPerPixel(), OSUtils.IsXpOrEarlier || !OSUtils.IsWindows);

        /// <summary>
        /// Extracts the icon of specified size and pixel format from an <see cref="Icon"/> instance.
        /// Unless the <see cref="Icon"/> constructors, this method works as expected.
        /// </summary>
        /// <param name="icon">The icon that may contain multiple images.</param>
        /// <param name="size">The required icon size to retrieve.</param>
        /// <param name="pixelFormat">The required pixel format to retrieve</param>
        /// <param name="forceUncompressedResult"><see langword="true"/>&#160;to force returning an uncompressed icon;
        /// <see langword="false"/>&#160;to allow PNG compression, which is supported by Windows Vista and above.</param>
        /// <returns>An <see cref="Icon"/> instance, which contains only a single image,
        /// or <see langword="null"/>&#160;if no icon found with the specified size and format.</returns>
        /// <remarks>
        /// <note>On some platforms this method may throw a <see cref="PlatformNotSupportedException"/> for very large or compressed icons.</note>
        /// </remarks>
        /// <seealso cref="ExtractBitmap(Icon,Size,PixelFormat,bool)"/>
        public static Icon ExtractIcon(this Icon icon, Size size, PixelFormat pixelFormat, bool forceUncompressedResult)
            => ExtractFirstIcon(icon, size, pixelFormat.ToBitsPerPixel(), forceUncompressedResult);

        /// <summary>
        /// Extracts the icon of specified index from an <see cref="Icon"/> instance.
        /// </summary>
        /// <param name="icon">The icon that may contain multiple images.</param>
        /// <param name="index">The zero-based index of the icon image to retrieve.</param>
        /// <returns>An <see cref="Icon"/> instance, which contains only a single image,
        /// or <see langword="null"/>&#160;if the specified <paramref name="index"/> was too large.</returns>
        /// <remarks>
        /// <para>The result <see cref="Icon"/> instances are compatible with Windows XP if the method is executed in a Windows XP environment.</para>
        /// <note>On some platforms this method may throw a <see cref="PlatformNotSupportedException"/> for very large icons.</note>
        /// </remarks>
        /// <seealso cref="ExtractBitmap(Icon,int,bool)"/>
        public static Icon ExtractIcon(this Icon icon, int index) => ExtractIcon(icon, index, OSUtils.IsXpOrEarlier || !OSUtils.IsWindows);

        /// <summary>
        /// Extracts the icon of specified index from an <see cref="Icon"/> instance.
        /// </summary>
        /// <param name="icon">The icon that may contain multiple images.</param>
        /// <param name="index">The zero-based index of the icon image to retrieve.</param>
        /// <param name="forceUncompressedResult"><see langword="true"/>&#160;to force returning an uncompressed icon;
        /// <see langword="false"/>&#160;to allow PNG compression, which is supported by Windows Vista and above.</param>
        /// <returns>An <see cref="Icon"/> instance, which contains only a single image,
        /// or <see langword="null"/>&#160;if the specified <paramref name="index"/> was too large.</returns>
        /// <remarks>
        /// <note>On some platforms this method may throw a <see cref="PlatformNotSupportedException"/> for very large or compressed icons.</note>
        /// </remarks>
        /// <seealso cref="ExtractBitmap(Icon,int,bool)"/>
#if !NET35
        [SecuritySafeCritical]
#endif
        public static Icon ExtractIcon(this Icon icon, int index, bool forceUncompressedResult)
        {
            if (icon == null)
                throw new ArgumentNullException(nameof(icon), PublicResources.ArgumentNull);
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), PublicResources.ArgumentMustBeGreaterThanOrEqualTo(0));

            using (RawIcon rawIcon = new RawIcon(icon, null, null, index))
                return rawIcon.ToIcon(forceUncompressedResult);
        }

        /// <summary>
        /// Extracts the nearest icon of specified size and pixel format from an <see cref="Icon"/> instance.
        /// Unless the <see cref="Icon"/> constructors, this method works as expected.
        /// </summary>
        /// <param name="icon">The icon that may contain multiple images.</param>
        /// <param name="size">The required icon size to retrieve.</param>
        /// <param name="pixelFormat">The required pixel format to retrieve</param>
        /// <returns>An <see cref="Icon"/> instance, which contains only a single image. If no
        /// icon was found with the specified size and format the nearest icon (<paramref name="size"/> match has preference over <paramref name="pixelFormat"/>) is returned.</returns>
        /// <remarks>
        /// <para>The result <see cref="Icon"/> is compatible with Windows XP if the method is executed in a Windows XP environment.</para>
        /// <para>On some platforms it may happen that a smaller icon is returned than requested if the requested icon size is not supported.</para>
        /// </remarks>
        /// <seealso cref="ExtractNearestBitmap(Icon,Size,PixelFormat,bool)"/>
        public static Icon ExtractNearestIcon(this Icon icon, Size size, PixelFormat pixelFormat) => ExtractNearestIcon(icon, size, pixelFormat, OSUtils.IsXpOrEarlier);

        /// <summary>
        /// Extracts the nearest icon of specified size and pixel format from an <see cref="Icon"/> instance.
        /// Unless the <see cref="Icon"/> constructors, this method works as expected.
        /// </summary>
        /// <param name="icon">The icon that may contain multiple images.</param>
        /// <param name="size">The required icon size to retrieve.</param>
        /// <param name="pixelFormat">The required pixel format to retrieve</param>
        /// <param name="forceUncompressedResult"><see langword="true"/>&#160;to force returning an uncompressed icon;
        /// <see langword="false"/>&#160;to allow PNG compression, which is supported by Windows Vista and above.</param>
        /// <returns>An <see cref="Icon"/> instance, which contains only a single image. If no
        /// icon was found with the specified size and format the nearest icon (<paramref name="size"/> match have preference over <paramref name="pixelFormat"/>) is returned.</returns>
        /// <remarks>
        /// <para>On some platforms it may happen that a smaller icon is returned than requested if the requested icon size is not supported.</para>
        /// </remarks>
        /// <seealso cref="ExtractNearestBitmap(Icon,Size,PixelFormat,bool)"/>
#if !NET35
        [SecuritySafeCritical]
#endif
        public static Icon ExtractNearestIcon(this Icon icon, Size size, PixelFormat pixelFormat, bool forceUncompressedResult)
        {
            if (icon == null)
                throw new ArgumentNullException(nameof(icon), PublicResources.ArgumentNull);
            int bpp = pixelFormat.ToBitsPerPixel();

            using (RawIcon rawIcon = new RawIcon(icon))
                return rawIcon.ExtractNearestIcon(bpp, size, forceUncompressedResult);
        }

        /// <summary>
        /// Combines an <see cref="Icon"/> instance with the provided <paramref name="icons"/> into a multi-resolution <see cref="Icon"/> instance.
        /// </summary>
        /// <param name="icon">The icon to combine with other icons.</param>
        /// <param name="icons">The icons to be combined with the specified <paramref name="icon"/>.</param>
        /// <returns>An <see cref="Icon"/> instance that contains every image of the source <paramref name="icons"/>.</returns>
        /// <remarks>
        /// <para>Both <paramref name="icon"/> and elements of <paramref name="icons"/> may contain multiple icons.</para>
        /// <para>The result <see cref="Icon"/> is compatible with Windows XP if the method is executed in a Windows XP environment.</para>
        /// </remarks>
        public static Icon Combine(this Icon icon, params Icon[] icons) => Combine(icon, OSUtils.IsXpOrEarlier, icons);

        /// <summary>
        /// Combines an <see cref="Icon"/> instance with the provided <paramref name="icons"/> into a multi-resolution <see cref="Icon"/> instance.
        /// </summary>
        /// <param name="icon">The icon to combine with other icons.</param>
        /// <param name="forceUncompressedResult"><see langword="true"/>&#160;to force returning an uncompressed icon;
        /// <see langword="false"/>&#160;to allow PNG compression, which is supported by Windows Vista and above.</param>
        /// <param name="icons">The icons to be combined with the specified <paramref name="icon"/>.</param>
        /// <returns>An <see cref="Icon"/> instance that contains every image of the source <paramref name="icons"/>.</returns>
        /// <remarks>Both <paramref name="icon"/> and elements of <paramref name="icons"/> may contain multiple icons.</remarks>
#if !NET35
        [SecuritySafeCritical]
#endif
        public static Icon Combine(this Icon icon, bool forceUncompressedResult, params Icon[] icons)
        {
            if (icon == null)
                throw new ArgumentNullException(nameof(icon), PublicResources.ArgumentNull);
            if ((icons == null || icons.Length == 0) && !forceUncompressedResult)
                return icon;

            using (RawIcon rawIcon = new RawIcon(icon))
            {
                if (icons != null)
                {
                    foreach (Icon item in icons)
                        rawIcon.Add(item);
                }

                return rawIcon.ToIcon(forceUncompressedResult);
            }
        }

        /// <summary>
        /// Combines an <see cref="Icon" /> instance with the provided <paramref name="images" /> into a multi-resolution <see cref="Icon" /> instance.
        /// </summary>
        /// <param name="icon">The icon to combine with other images.</param>
        /// <param name="images">The images to be added to the <paramref name="icon"/>. Images can be non-squared ones.</param>
        /// <returns>
        /// An <see cref="Icon" /> instance that contains every image of the source <paramref name="images" />.
        /// </returns>
        /// <remarks>
        /// <para>Both <paramref name="icon"/> and elements of <paramref name="images"/> may contain multiple icons.</para>
        /// <para>The result <see cref="Icon"/> is compatible with Windows XP if the method is executed in a Windows XP environment.</para>
        /// </remarks>
        public static Icon Combine(this Icon icon, params Bitmap[] images) => Combine(icon, OSUtils.IsXpOrEarlier, images);

        /// <summary>
        /// Combines an <see cref="Icon" /> instance with the provided <paramref name="images" /> into a multi-resolution <see cref="Icon" /> instance.
        /// </summary>
        /// <param name="icon">The icon to combine with other images.</param>
        /// <param name="forceUncompressedResult"><see langword="true"/>&#160;to force returning an uncompressed icon;
        /// <see langword="false"/>&#160;to allow PNG compression, which is supported by Windows Vista and above.</param>
        /// <param name="images">The images to be added to the <paramref name="icon"/>. Images can be non-squared ones.</param>
        /// <returns>
        /// An <see cref="Icon" /> instance that contains every image of the source <paramref name="images" />.
        /// </returns>
        /// <para>Both <paramref name="icon"/> and elements of <paramref name="images"/> may contain multiple icons.</para>
#if !NET35
        [SecuritySafeCritical]
#endif
        public static Icon Combine(this Icon icon, bool forceUncompressedResult, params Bitmap[] images)
        {
            if (icon == null)
                throw new ArgumentNullException(nameof(icon), PublicResources.ArgumentNull);
            if (images == null || images.Length == 0)
                return icon;

            using (RawIcon rawIcon = new RawIcon(icon))
            {
                foreach (Bitmap image in images)
                    rawIcon.Add(image);
                return rawIcon.ToIcon(forceUncompressedResult);
            }
        }

        /// <summary>
        /// Combines an <see cref="Icon" /> instance with the provided <paramref name="image" /> into a multi-resolution <see cref="Icon" /> instance.
        /// </summary>
        /// <param name="icon">The icon to combine with other images.</param>
        /// <param name="image">The image to be added to the <paramref name="icon"/>. Can be a non-squared one.</param>
        /// <param name="transparentColor">A color that represents the transparent color in <paramref name="image"/>.</param>
        /// <returns>
        /// An <see cref="Icon" /> instance that contains the source <paramref name="image" />.
        /// </returns>
        /// <remarks>
        /// <para>Both <paramref name="icon"/> and <paramref name="image"/> may contain multiple icons.</para>
        /// <para>The result <see cref="Icon"/> is compatible with Windows XP if the method is executed in a Windows XP environment.</para>
        /// </remarks>
        public static Icon Combine(this Icon icon, Bitmap image, Color transparentColor) => Combine(icon, image, transparentColor, OSUtils.IsXpOrEarlier);

        /// <summary>
        /// Combines an <see cref="Icon" /> instance with the provided <paramref name="image" /> into a multi-resolution <see cref="Icon" /> instance.
        /// </summary>
        /// <param name="icon">The icon to combine with other images.</param>
        /// <param name="image">The image to be added to the <paramref name="icon"/>. Can be a non-squared one.</param>
        /// <param name="transparentColor">A color that represents the transparent color in <paramref name="image"/>.</param>
        /// <param name="forceUncompressedResult"><see langword="true"/>&#160;to force returning an uncompressed icon;
        /// <see langword="false"/>&#160;to allow PNG compression, which is supported by Windows Vista and above.</param>
        /// <returns>
        /// An <see cref="Icon" /> instance that contains the source <paramref name="image" />.
        /// </returns>
        /// <remarks>
        /// <para>Both <paramref name="icon"/> and <paramref name="image"/> may contain multiple icons.</para>
        /// </remarks>
#if !NET35
        [SecuritySafeCritical]
#endif
        public static Icon Combine(this Icon icon, Bitmap image, Color transparentColor, bool forceUncompressedResult)
        {
            if (icon == null)
                throw new ArgumentNullException(nameof(icon), PublicResources.ArgumentNull);
            if (image == null)
                return icon;

            using (RawIcon rawIcon = new RawIcon(icon))
            {
                rawIcon.Add(image, transparentColor);
                return rawIcon.ToIcon(forceUncompressedResult);
            }
        }

        /// <summary>
        /// Saves the <paramref name="icon"/> into the specified <paramref name="stream"/>. Unlike <see cref="Icon.Save">Icon.Save</see>, this method can save every icon with high quality, even
        /// <see cref="SystemIcons"/> members, and icons created by the <see cref="Icon.FromHandle">Icon.FromHandle</see> method.
        /// </summary>
        /// <param name="icon">The icon to save</param>
        /// <param name="stream">A stream into which the icon has to be saved.</param>
        /// <param name="forceUncompressedResult"><see langword="true"/>&#160;to force saving an uncompressed icon;
        /// <see langword="false"/>&#160;to allow PNG compression, which is supported by Windows Vista and above. This parameter is optional.
        /// <br/>Default value: <see langword="false"/>.</param>
#if !NET35
        [SecuritySafeCritical]
#endif
        public static void SaveHighQuality(this Icon icon, Stream stream, bool forceUncompressedResult = false)
        {
            if (icon == null)
                throw new ArgumentNullException(nameof(icon), PublicResources.ArgumentNull);
            if (stream == null)
                throw new ArgumentNullException(nameof(stream), PublicResources.ArgumentNull);

            using (RawIcon rawIcon = new RawIcon(icon))
                rawIcon.Save(stream, forceUncompressedResult);
        }

        /// <summary>
        /// Converts the <paramref name="icon"/> to an uncompressed one.
        /// </summary>
        /// <param name="icon">The icon to convert.</param>
        /// <returns>An <see cref="Icon"/> instance that contains only uncompressed images.</returns>
        /// <remarks>Compressed icons (which contain PNG images) cannot be displayed by the standard ways in Windows XP.
        /// Calling this method is supported though in any operating system if there is registered built-in PNG decoder.</remarks>
        public static Icon ToUncompressedIcon(this Icon icon) => Icons.Combine(true, icon);

        /// <summary>
        /// Determines whether the icon or its image at the specified index is compressed.
        /// </summary>
        /// <param name="icon">The icon.</param>
        /// <param name="index">The index to check. If <see langword="null"/>, then the result determines whether the <paramref name="icon"/> has at least one compressed image. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns><see langword="true"/>&#160;if the icon or its image at the specified index is compressed.</returns>
#if !NET35
        [SecuritySafeCritical]
#endif
        public static bool IsCompressed(this Icon icon, int? index = null)
        {
            if (icon == null)
                throw new ArgumentNullException(nameof(icon), PublicResources.ArgumentNull);
            using (var rawIcon = new RawIcon(icon, null, null, index))
            {
                if (index != null && rawIcon.ImageCount == 0)
                    throw new ArgumentOutOfRangeException(nameof(index), PublicResources.ArgumentOutOfRange);
                return rawIcon.IsCompressed;
            }
        }

        /// <summary>
        /// Gets the bits per pixel (bpp) value of the icon.
        /// </summary>
        /// <param name="icon">The icon.</param>
        /// <param name="index">The index to check. If <see langword="null"/>, then the result determines the highest bpp value if the icon images. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>The bits per pixel (bpp) value of the icon.</returns>
#if !NET35
        [SecuritySafeCritical]
#endif
        public static int GetBitsPerPixel(this Icon icon, int? index = null)
        {
            if (icon == null)
                throw new ArgumentNullException(nameof(icon), PublicResources.ArgumentNull);
            using (var rawIcon = new RawIcon(icon, null, null, index))
            {
                if (index != null && rawIcon.ImageCount == 0)
                    throw new ArgumentOutOfRangeException(nameof(index), PublicResources.ArgumentOutOfRange);
                return rawIcon.Bpp;
            }
        }

        /// <summary>
        /// Converts the provided <paramref name="icon"/> to a <see cref="CursorHandle"/>, which can be passed to the
        /// <a href="https://msdn.microsoft.com/en-us/library/system.windows.forms.cursor.aspx" target="_blank">System.Windows.Forms.Cursor</a> constructor
        /// to create a new cursor.
        /// </summary>
        /// <param name="icon">The <see cref="Icon"/>, which should be converted to a cursor.</param>
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
        public static CursorHandle ToCursorHandle(this Icon icon, Point cursorHotspot = default)
        {
            if (icon == null)
                throw new ArgumentNullException(nameof(icon), PublicResources.ArgumentNull);
            if (!OSUtils.IsWindows)
                throw new PlatformNotSupportedException(Res.RequiresWindows);

            return Icons.ToCursorHandle(icon.Handle, cursorHotspot);
        }

        #endregion

        #region Internal Methods

        internal static bool HasRawData(this Icon icon) => icon.HasIconData();

        /// <summary>
        /// Needed for unmanaged icons returned by winapi methods because <see cref="Icon.FromHandle"/> does not
        /// take ownership of handle and does not dispose the icon.
        /// </summary>
#if !NET35
        [SecuritySafeCritical]
#endif
        internal static Icon ToManagedIcon(this Icon unmanagedIcon)
        {
            if (!OSUtils.IsWindows)
                return unmanagedIcon;
            Icon managedIcon = (Icon)unmanagedIcon.Clone();
            User32.DestroyIcon(unmanagedIcon.Handle);
            return managedIcon;
        }

        #endregion

        #region Private Methods

#if !NET35
        [SecuritySafeCritical]
#endif
        private static Bitmap[] ExtractBitmaps(Icon icon, Size? size, int? bpp, bool keepOriginalFormat)
        {
            if (icon == null)
                throw new ArgumentNullException(nameof(icon), PublicResources.ArgumentNull);

            using (RawIcon rawIcon = new RawIcon(icon, size, bpp, null))
                return rawIcon.ExtractBitmaps(keepOriginalFormat);
        }

#if !NET35
        [SecuritySafeCritical]
#endif
        private static Bitmap ExtractFirstBitmap(Icon icon, Size? size, int? bpp, bool keepOriginalFormat)
        {
            if (icon == null)
                throw new ArgumentNullException(nameof(icon), PublicResources.ArgumentNull);

            using (RawIcon rawIcon = new RawIcon(icon, size, bpp))
                return rawIcon.ExtractBitmap(0, keepOriginalFormat);
        }

#if !NET35
        [SecuritySafeCritical]
#endif
        private static Icon[] ExtractIcons(Icon icon, Size? size, int? bpp, bool forceUncompressedResult)
        {
            if (icon == null)
                throw new ArgumentNullException(nameof(icon));

            using (RawIcon rawIcon = new RawIcon(icon, size, bpp, null))
                return rawIcon.ExtractIcons(forceUncompressedResult);
        }

#if !NET35
        [SecuritySafeCritical]
#endif
        private static Icon ExtractFirstIcon(Icon icon, Size size, int? bpp, bool forceUncompressedResult)
        {
            if (icon == null)
                throw new ArgumentNullException(nameof(icon));

            using (RawIcon rawIcon = new RawIcon(icon, size, bpp))
                return rawIcon.ExtractIcon(0, forceUncompressedResult);
        }

        #endregion

        #endregion
    }
}
