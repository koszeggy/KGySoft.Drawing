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
using System.Linq;
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
        /// <seealso cref="ExtractBitmap(Icon,bool)"/>
        /// <seealso cref="ExtractBitmap(Icon,Size,PixelFormat,bool)"/>
        /// <seealso cref="ExtractBitmap(Icon,int,bool)"/>
        /// <returns>A <see cref="Bitmap"/> that represents the converted <see cref="Icon"/>.</returns>
        /// <remarks>
        /// <para><see cref="O:KGySoft.Drawing.IconExtensions.ExtractBitmap">ExtractBitmap</see> and <see cref="ToAlphaBitmap"/> methods may return a different result even if
        /// the <paramref name="icon"/> contains a single image only. The <see cref="O:KGySoft.Drawing.IconExtensions.ExtractBitmap">ExtractBitmap</see> overloads works from the 
        /// saved icon stream in the first place, which is slower than this method.</para>
        /// <para>If the <paramref name="icon"/> contains multiple images consider to use either the <see cref="O:KGySoft.Drawing.IconExtensions.ExtractBitmap">ExtractBitmap</see> overloads to specify the exact image to return,
        /// or the <see cref="ToMultiResBitmap">ToMultiResBitmap</see> method, which returns every images in a single combined <see cref="Bitmap"/>.</para>
        /// </remarks>
#if !NET35
        [SecuritySafeCritical]
#endif
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The result must not be disposed.")]
        public static Bitmap ToAlphaBitmap(this Icon icon)
        {
            if (icon == null)
                throw new ArgumentNullException(nameof(icon), PublicResources.ArgumentNull);

            User32.GetIconInfo(icon.Handle, out ICONINFO iconInfo);
            try
            {
                // Getting color depth by GDI. (FromHbitmap always returns 32 bppRgb format)
                // The possible 1 bit transparency is handled by ToBitmap, too. Though GetIconInfo returns always 32 bit image when display settings use 32 bit.
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

                        // Checking if result is fully transparent. This happens when icon is actually not a 32 bit one.
                        // This cannot be checked with BITMAP or PixelFormat because that is always 32 bit with 32 bit display settings.
                        // If image is fully transparent, letting Icon.ToBitmap do the job. RawIcon could also do it but it would build a new icon.
                        if (IsFullyTransparent(dataRedirected))
                            return icon.ToBitmap();
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
#if !NET35
        [SecuritySafeCritical]
#endif
        public static Bitmap ToMultiResBitmap(this Icon icon)
        {
            using (RawIcon rawIcon = new RawIcon(icon))
                return rawIcon.ToBitmap();
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
        /// <seealso cref="ExtractBitmaps(Icon,bool)"/>
        public static Icon[] ExtractIcons(this Icon icon) => ExtractIcons(icon, null, null);

        /// <summary>
        /// Extracts every icon of specified size from an <see cref="Icon"/> instance as separated <seealso cref="Icon"/> instances.
        /// </summary>
        /// <param name="icon">The icon that may contain multiple images.</param>
        /// <param name="size">The required icon size to retrieve.</param>
        /// <returns>An array of <see cref="Icon"/> instances, which were extracted from the <paramref name="icon"/>.</returns>
        /// <seealso cref="ExtractBitmaps(Icon,Size,bool)"/>
        public static Icon[] ExtractIcons(this Icon icon, Size size) => ExtractIcons(icon, size, null);

        /// <summary>
        /// Extracts every icon of specified pixel format from an <see cref="Icon"/> instance as separated <seealso cref="Icon"/> instances.
        /// </summary>
        /// <param name="icon">The icon that may contain multiple images.</param>
        /// <param name="pixelFormat">The required pixel format to retrieve</param>
        /// <returns>An array of <see cref="Icon"/> instances, which were extracted from the <paramref name="icon"/>.</returns>
        /// <seealso cref="ExtractBitmaps(Icon,PixelFormat,bool)"/>
        public static Icon[] ExtractIcons(this Icon icon, PixelFormat pixelFormat) => ExtractIcons(icon, null, pixelFormat.ToBitsPerPixel());

        /// <summary>
        /// Extracts the first icon of specified size from an <see cref="Icon"/> instance.
        /// Unless the <see cref="Icon"/> constructors, this method works as expected.
        /// </summary>
        /// <param name="icon">The icon that may contain multiple images.</param>
        /// <param name="size">The required icon size to retrieve.</param>
        /// <returns>An <see cref="Icon"/> instance, which contains only a single image,
        /// or <see langword="null"/>&#160;if no icon found with the specified size.</returns>
        /// <seealso cref="ExtractBitmap(Icon,Size,bool)"/>
        public static Icon ExtractIcon(this Icon icon, Size size) => ExtractFirstIcon(icon, size, null);

        /// <summary>
        /// Extracts the icon of specified size and pixel format from an <see cref="Icon"/> instance.
        /// Unless the <see cref="Icon"/> constructors, this method works as expected.
        /// </summary>
        /// <param name="icon">The icon that may contain multiple images.</param>
        /// <param name="size">The required icon size to retrieve.</param>
        /// <param name="pixelFormat">The required pixel format to retrieve</param>
        /// <returns>An <see cref="Icon"/> instance, which contains only a single image,
        /// or <see langword="null"/>&#160;if no icon found with the specified size and format.</returns>
        /// <seealso cref="ExtractBitmap(Icon,Size,PixelFormat,bool)"/>
        public static Icon ExtractIcon(this Icon icon, Size size, PixelFormat pixelFormat) => ExtractFirstIcon(icon, size, pixelFormat.ToBitsPerPixel());

        /// <summary>
        /// Extracts the icon of specified index from an <see cref="Icon"/> instance.
        /// </summary>
        /// <param name="icon">The icon that may contain multiple images.</param>
        /// <param name="index">The zero-based index of the icon image to retrieve.</param>
        /// <returns>An <see cref="Icon"/> instance, which contains only a single image,
        /// or <see langword="null"/>&#160;if the specified <paramref name="index"/> was too large.</returns>
        /// <seealso cref="ExtractBitmap(Icon,int,bool)"/>
#if !NET35
        [SecuritySafeCritical]
#endif
        public static Icon ExtractIcon(this Icon icon, int index)
        {
            if (icon == null)
                throw new ArgumentNullException(nameof(icon), PublicResources.ArgumentNull);
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), PublicResources.ArgumentMustBeGreaterThanOrEqualTo(0));

            using (RawIcon rawIcon = new RawIcon(icon, null, null, index))
                return rawIcon.ToIcon();
        }

        /// <summary>
        /// Extracts the nearest icon of specified size and pixel format from an <see cref="Icon"/> instance.
        /// Unless the <see cref="Icon"/> constructors, this method works as expected.
        /// </summary>
        /// <param name="icon">The icon that may contain multiple images.</param>
        /// <param name="size">The required icon size to retrieve.</param>
        /// <param name="pixelFormat">The required pixel format to retrieve</param>
        /// <returns>An <see cref="Icon"/> instance, which contains only a single image. If no
        /// icon was found with the specified size and format the nearest icon (<paramref name="pixelFormat"/> matches first, then <paramref name="size"/>) is returned.</returns>
        /// <seealso cref="ExtractNearestBitmap(Icon,Size,PixelFormat,bool)"/>
#if !NET35
        [SecuritySafeCritical]
#endif
        public static Icon ExtractNearestIcon(this Icon icon, Size size, PixelFormat pixelFormat)
        {
            if (icon == null)
                throw new ArgumentNullException(nameof(icon), PublicResources.ArgumentNull);
            int bpp = pixelFormat.ToBitsPerPixel();

            using (RawIcon rawIcon = new RawIcon(icon))
                return rawIcon.ExtractNearestIcon(bpp, size);
        }

        /// <summary>
        /// Combines an <see cref="Icon"/> instance with the provided <paramref name="icons"/> into a multi-resolution <see cref="Icon"/> instance.
        /// </summary>
        /// <param name="icon">The icon to combine with other icons.</param>
        /// <param name="icons">The icons to be combined with the specified <paramref name="icon"/>.</param>
        /// <returns>An <see cref="Icon"/> instance that contains every image of the source <paramref name="icons"/>.</returns>
        /// <remarks>Both <paramref name="icon"/> and elements of <paramref name="icons"/> may contain multiple icons.</remarks>
#if !NET35
        [SecuritySafeCritical]
#endif
        public static Icon Combine(this Icon icon, params Icon[] icons)
        {
            if (icon == null)
                throw new ArgumentNullException(nameof(icon), PublicResources.ArgumentNull);
            if (icons == null || icons.Length == 0)
                return icon;

            using (RawIcon rawIcon = new RawIcon(icon))
            {
                foreach (Icon item in icons)
                {
                    rawIcon.Add(item);
                }

                return rawIcon.ToIcon();
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
        /// <remarks><paramref name="icon"/> may already contain multiple icons.</remarks>
#if !NET35
        [SecuritySafeCritical]
#endif
        public static Icon Combine(this Icon icon, params Bitmap[] images)
        {
            if (icon == null)
                throw new ArgumentNullException(nameof(icon), PublicResources.ArgumentNull);
            if (images == null || images.Length == 0)
                return icon;

            using (RawIcon rawIcon = new RawIcon(icon))
            {
                foreach (Bitmap image in images)
                {
                    rawIcon.Add(image);
                }

                return rawIcon.ToIcon();
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
        /// <remarks><paramref name="icon"/> may already contain multiple icons.</remarks>
#if !NET35
        [SecuritySafeCritical]
#endif
        public static Icon Combine(this Icon icon, Bitmap image, Color transparentColor)
        {
            if (icon == null)
                throw new ArgumentNullException(nameof(icon), PublicResources.ArgumentNull);
            if (image == null)
                return icon;

            using (RawIcon rawIcon = new RawIcon(icon))
            {
                rawIcon.Add(image, transparentColor);
                return rawIcon.ToIcon();
            }
        }

        /// <summary>
        /// Saves the <paramref name="icon"/> into the specified <paramref name="stream"/>. Unlike <see cref="Icon.Save">Icon.Save</see>, this method can save every icon with high quality, even
        /// <see cref="SystemIcons"/> members, and icons created by the <see cref="Icon.FromHandle">Icon.FromHandle</see> method.
        /// </summary>
        /// <param name="icon">The icon to save</param>
        /// <param name="stream">A stream into which the icon has to be saved.</param>
#if !NET35
        [SecuritySafeCritical]
#endif
        public static void SaveHighQuality(this Icon icon, Stream stream)
        {
            if (icon == null)
                throw new ArgumentNullException(nameof(icon), PublicResources.ArgumentNull);
            if (stream == null)
                throw new ArgumentNullException(nameof(stream), PublicResources.ArgumentNull);

            using (RawIcon rawIcon = new RawIcon(icon))
                rawIcon.Save(stream);
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
#if !NET35
        [SecuritySafeCritical]
#endif
        public static CursorHandle ToCursorHandle(this Icon icon, Point cursorHotspot = default)
        {
            if (icon == null)
                throw new ArgumentNullException(nameof(icon), PublicResources.ArgumentNull);

            return Icons.ToCursorHandle(icon.Handle, cursorHotspot);
        }

        #endregion

        #region Internal Methods

        internal static bool HasRawData(this Icon icon) => icon.GetIconData() != null;

        /// <summary>
        /// Needed for unmanaged icons returned by winapi methods because <see cref="Icon.FromHandle"/> does not
        /// take ownership of handle and does not dispose the icon.
        /// </summary>
#if !NET35
        [SecuritySafeCritical]
#endif
        internal static Icon ToManagedIcon(this Icon unmanagedIcon)
        {
            Icon managedIcon = (Icon)unmanagedIcon.Clone();
            User32.DestroyIcon(unmanagedIcon.Handle);
            return managedIcon;
        }

        #endregion

        #region Private Methods

        [SecurityCritical]
        private static unsafe bool IsFullyTransparent(BitmapData data)
        {
            byte* line = (byte*)data.Scan0;
            for (int y = 0; y < data.Height; y++)
            {
                for (int x = 0; x < data.Width; x++)
                {
                    int c = *((int*)line + x);
                    if ((c >> 24) != 0)
                        return false;
                }

                line += data.Stride;
            }

            return true;
        }

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
        private static Icon[] ExtractIcons(Icon icon, Size? size, int? bpp)
        {
            if (icon == null)
                throw new ArgumentNullException(nameof(icon));

            using (RawIcon rawIcon = new RawIcon(icon, size, bpp, null))
                return rawIcon.ExtractIcons();
        }

#if !NET35
        [SecuritySafeCritical]
#endif
        private static Icon ExtractFirstIcon(Icon icon, Size size, int? bpp)
        {
            if (icon == null)
                throw new ArgumentNullException(nameof(icon));

            using (RawIcon rawIcon = new RawIcon(icon, size, bpp))
                return rawIcon.ExtractIcon(0);
        }

        #endregion

        #endregion
    }
}
