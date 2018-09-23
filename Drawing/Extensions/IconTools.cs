#region Used namespaces

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

using KGySoft.Libraries;
using KGySoft.Drawing.WinApi;
using KGySoft.Reflection;

#endregion

namespace KGySoft.Drawing
{
    using System.ComponentModel;

    /// <summary>
    /// Contains <see cref="Icon"/> related methods.
    /// </summary>
    public static class IconTools
    {
        #region Fields

        private static FieldAccessor iconData;
        private static readonly Size size16 = new Size(16, 16);
        private static readonly Size size32 = new Size(32, 32);

        #endregion

        #region Methods

        #region Public Methods

        /// <summary>
        /// Extracts icons of the specified <paramref name="size"/> from a file and returns them as separated <see cref="Icon"/> instances.
        /// </summary>
        public static Icon[] IconsFromFile(string filename, SystemIconSize size)
        {
            int iconCount = Shell32.ExtractIconEx(filename, -1,
                null, null, 0); //checks how many icons.
            IntPtr[] iconPtr = new IntPtr[iconCount];

            //extracts the icons by the size that was selected.
            if (size == SystemIconSize.Small)
                Shell32.ExtractIconEx(filename, 0, null, iconPtr, iconCount);
            else
                Shell32.ExtractIconEx(filename, 0, iconPtr, null, iconCount);

            Icon[] iconList = new Icon[iconCount];

            //gets the icons in a list.
            for (int i = 0; i < iconCount; i++)
            {
                iconList[i] = GetManagedIcon(Icon.FromHandle(iconPtr[i]));
            }

            return iconList;
        }

        /// <summary>
        /// Extracts one selected icon from a file by provided zero-based <paramref name="index"/>.
        /// Returns null if icon with specified index does not exist.
        /// </summary>
        public static Icon IconFromFile(string filename, SystemIconSize size, int index)
        {
            // Obtaining icon count.
            int iconCount = Shell32.ExtractIconEx(filename, -1, null, null, 0);
            if (iconCount <= 0 || index >= iconCount)
                return null; // no icons were found.

            IntPtr[] iconPtr = new IntPtr[1];

            //extracts the icon that we want in the selected size.
            if (size == SystemIconSize.Small)
                Shell32.ExtractIconEx(filename, index, null, iconPtr, 1);
            else
                Shell32.ExtractIconEx(filename, index, iconPtr, null, 1);

            return GetManagedIcon(Icon.FromHandle(iconPtr[0]));
        }

        /// <summary>
        /// Gets the system-associated icon of a file extension.
        /// </summary>
        public static Icon IconFromExtension(string extension, SystemIconSize size)
        {
            //add '.' if nessesry
            if (extension[0] != '.')
                extension = '.' + extension;

            //temp struct for getting file shell info
            SHFILEINFO tempFileInfo = new SHFILEINFO();

            Shell32.SHGetFileInfo(extension, 0, ref tempFileInfo, (uint)Marshal.SizeOf(tempFileInfo), Constants.SHGFI_ICON | Constants.SHGFI_USEFILEATTRIBUTES | (uint)size);

            return GetManagedIcon(Icon.FromHandle(tempFileInfo.hIcon));
        }

        /// <summary>
        /// Creates an <see cref="Icon" /> from an <see cref="Image" />.
        /// </summary>
        /// <param name="image">The image to be converted to an icon.</param>
        /// <param name="size">The required size of the icon. Must not be larger than 256.</param>
        /// <param name="keepAspectRatio">When source <paramref name="image"/> is not square sized, determines whether the image should keep aspect ratio.</param>
        /// <returns>An <see cref="Icon"/> instance created from the <paramref name="image"/>.</returns>
        /// <remarks>The result icon will be always sqaure sized. To create a non-square icon, use <see cref="Combine(Bitmap[])"/> instead.</remarks>
        public static Icon IconFromImage(Image image, int size, bool keepAspectRatio)
        {
            if (image == null)
                throw new ArgumentNullException("image");

            if (size < 1 || size > 256)
                throw new ArgumentOutOfRangeException("size");

            Bitmap bitmap;
            if (size == image.Width && size == image.Height && (bitmap = image as Bitmap) != null)
            {
                return GetManagedIcon(Icon.FromHandle(bitmap.GetHicon()));
            }

            using (bitmap = new Bitmap(size, size))
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    int x, y, w, h; // dimensions for new image

                    if (!keepAspectRatio || image.Height == image.Width)
                    {
                        // just fill the square
                        x = y = 0; // set x and y to 0
                        w = h = size; // set width and height to size
                    }
                    else
                    {
                        // work out the aspect ratio
                        float r = (float)image.Width / image.Height;
                        // set dimensions accordingly to fit inside size^2 square

                        if (r > 1)
                        { // w is bigger, so divide h by r
                            w = size;
                            h = (int)(size / r);
                            x = 0;
                            y = (size - h) / 2; // center the image
                        }
                        else
                        { // h is bigger, so multiply w by r
                            w = (int)(size * r);
                            h = size;
                            y = 0;
                            x = (size - w) / 2; // center the image
                        }
                    }
                    // make the image shrink nicely by using HighQualityBicubic mode
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.PixelOffsetMode = PixelOffsetMode.Half;
                    g.DrawImage(image, x, y, w, h); // draw image with specified dimensions
                    g.Flush(); // make sure all drawing operations complete before we get the icon

                    return GetManagedIcon(Icon.FromHandle(bitmap.GetHicon()));
                }
            }
        }

        /// <summary>
        /// Creates an <see cref="Icon" /> from an <see cref="Image" />.
        /// </summary>
        /// <param name="image">The image to be converted to an icon.</param>
        /// <param name="size">The required size of the icon.</param>
        /// <returns>An <see cref="Icon"/> instance created from the <paramref name="image"/>.</returns>
        /// <remarks>The result icon will be always sqaure sized. Original aspect ratio of the image is kept.</remarks>
        public static Icon IconFromImage(Image image, SystemIconSize size)
        {
            return IconFromImage(image, size == SystemIconSize.Small ? 16 : 32, true);
        }

        /// <summary>
        /// Similarly to <see cref="Icon.ToBitmap"/>, converts the icon to a <see cref="Bitmap"/>. While <see cref="Icon.ToBitmap"/> may return a wrong result
        /// when icon contains semi-transparent pixels, this method returns an image, in which alpha channel
        /// is always correctly applied for the image. This method supports large PNG icons, too. If the <paramref name="icon"/>
        /// contains multiple images, use the <see cref="ExtractBitmap(Icon,Size,PixelFormat,bool)"/> or <see cref="ExtractBitmap(Icon,int,bool)"/> method to specify the exact image to return.
        /// </summary>
        /// <param name="icon">The icon optionally with transparency.</param>
        /// <seealso cref="ExtractBitmap(Icon,bool)"/>
        /// <seealso cref="ExtractBitmap(Icon,Size,PixelFormat,bool)"/>
        /// <seealso cref="ExtractBitmap(Icon,int,bool)"/>
        /// <remarks><see cref="ExtractBitmap(Icon,bool)"/> and <see cref="ToAlphaBitmap"/> methods may have a different result even if
        /// the <paramref name="icon"/> contains a single image only. <see cref="ExtractBitmap(Icon,bool)"/> method works from the
        /// saved icon stream, which is slower than this method.
        /// </remarks>
        public static Bitmap ToAlphaBitmap(this Icon icon)
        {
            if (icon == null)
                throw new ArgumentNullException("icon");

            ICONINFO iconInfo;
            if (!User32.GetIconInfo(icon.Handle, out iconInfo))
                throw new ArgumentException("Invalid icon", "icon", new Win32Exception(Marshal.GetLastWin32Error()));

            try
            {
                // getting color depth (FromHbitmap always returns 32 bppRgb format)
                BITMAP bitmapInfo;
                if (Gdi32.GetObject(iconInfo.hbmColor, Marshal.SizeOf(typeof(BITMAP)), out bitmapInfo) == 0)
                    throw new ArgumentException("Invalid icon", "icon");

                // The possible 1 bit transparency is handled by ToBitmap, too. Though GetIconInfo returns always 32 bit image when display settings use 32 bit.
                if (bitmapInfo.bmBitsPixel < 32)
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
                    if (bmpRedirected != null)
                        bmpRedirected.Dispose();
                }
            }
            finally
            {
                Gdi32.DeleteObject(iconInfo.hbmColor);
                Gdi32.DeleteObject(iconInfo.hbmMask);
            }
        }

        /// <summary>
        /// Converts the <paramref name="icon"/> to a <see cref="Bitmap"/> instace, which contains every image of the <paramref name="icon"/>.
        /// When the returned <see cref="Bitmap"/> is used to create another <see cref="Bitmap"/> or is drawn into a <see cref="Graphics"/>, the best-fitting image is automatically applied.
        /// </summary>
        /// <param name="icon">The icon to convert to a multi-resolution <see cref="Bitmap"/>.</param>
        /// <returns>A <see cref="Bitmap"/> instance, which contains every image of the <paramref name="icon"/>.</returns>
        public static Bitmap ToMultiResBitmap(this Icon icon)
        {
            using (RawIcon rawIcon = new RawIcon(icon))
            {
                return rawIcon.ToBitmap();
            }
        }

        /// <summary>
        /// Extracts every image from an <see cref="Icon" /> instance.
        /// </summary>
        /// <param name="icon">The icon that may contain multiple images.</param>
        /// <param name="keepOriginalFormat">If <see langword="true"/>, keeps the original image format. Possible transparent pixels of
        /// non-32 bpp ARGB formats may be black. If <see langword="false"/>, always returns 32 bpp images with transparency.</param>
        /// <returns>
        /// An array of <see cref="Bitmap" /> instances, which were extracted from the <paramref name="icon" />.
        /// </returns>
        /// <seealso cref="ExtractIcons(Icon)" />
        public static Bitmap[] ExtractBitmaps(this Icon icon, bool keepOriginalFormat)
        {
            return ExtractBitmaps(icon, null, null, keepOriginalFormat);
        }

        /// <summary>
        /// Extracts every image of specified size from an <see cref="Icon"/> instance.
        /// </summary>
        /// <param name="icon">The icon that may contain multiple images.</param>
        /// <param name="size">The required icon size to retrieve</param>
        /// <param name="keepOriginalFormat">If <see langword="true"/>, keeps the original image format. Possible transparent pixels of
        /// non-32 bpp ARGB formats may be black. If <see langword="false"/>, always returns 32 bpp images with transparency.</param>
        /// <returns>An array of <see cref="Bitmap"/> instances, which were extracted from the <paramref name="icon"/>.</returns>
        /// <seealso cref="ExtractIcons(Icon,SystemIconSize)"/>
        public static Bitmap[] ExtractBitmaps(this Icon icon, SystemIconSize size, bool keepOriginalFormat)
        {
            if (!Enum<SystemIconSize>.IsDefined(size))
                throw new ArgumentOutOfRangeException("size");

            return ExtractBitmaps(icon, size == SystemIconSize.Small ? size16 : size32, null, keepOriginalFormat);
        }

        /// <summary>
        /// Extracts every image of specified size from an <see cref="Icon"/> instance.
        /// </summary>
        /// <param name="icon">The icon that may contain multiple images.</param>
        /// <param name="size">The required icon size to retrieve.</param>
        /// <param name="keepOriginalFormat">If <see langword="true"/>, keeps the original image format. Possible transparent pixels of
        /// non-32 bpp ARGB formats may be black. If <see langword="false"/>, always returns 32 bpp images with transparency.</param>
        /// <returns>An array of <see cref="Bitmap"/> instances, which were extracted from the <paramref name="icon"/>.</returns>
        /// <seealso cref="ExtractIcons(Icon,Size)"/>
        public static Bitmap[] ExtractBitmaps(this Icon icon, Size size, bool keepOriginalFormat)
        {
            return ExtractBitmaps(icon, size, null, keepOriginalFormat);
        }

        /// <summary>
        /// Extracts every image of specified pixel format from an <see cref="Icon"/> instance.
        /// </summary>
        /// <param name="icon">The icon that may contain multiple images.</param>
        /// <param name="pixelFormat">The required pixel format to retrieve</param>
        /// <param name="keepOriginalFormat">If <see langword="true"/>, keeps the original image format. Possible transparent pixels of
        /// non-32 bpp ARGB formats may be black. If <see langword="false"/>, always returns 32 bpp images with transparency.</param>
        /// <returns>An array of <see cref="Bitmap"/> instances, which were extracted from the <paramref name="icon"/>.</returns>
        /// <seealso cref="ExtractIcons(Icon,PixelFormat)"/>
        public static Bitmap[] ExtractBitmaps(this Icon icon, PixelFormat pixelFormat, bool keepOriginalFormat)
        {
            return ExtractBitmaps(icon, null, pixelFormat.ToBitsPerPixel(), keepOriginalFormat);
        }

        /// <summary>
        /// Extracts the first image from an <see cref="Icon"/> instance. If the icon has only one image,
        /// using <see cref="ToAlphaBitmap"/> is faster.
        /// </summary>
        /// <param name="icon">The icon that may contain multiple images.</param>
        /// <param name="keepOriginalFormat">If <see langword="true"/>, keeps the original image format. Possible transparent pixels of
        /// non-32 bpp ARGB formats may be black. If <see langword="false"/>, always returns 32 bpp images with transparency.</param>
        /// <returns>An <see cref="Bitmap"/> instance, which was extracted from the <paramref name="icon"/>,
        /// or <see langword="null"/> if no icon found.</returns>
        /// <seealso cref="ToAlphaBitmap"/>
        public static Bitmap ExtractBitmap(this Icon icon, bool keepOriginalFormat)
        {
            return ExtractBitmaps(icon, null, null, keepOriginalFormat).FirstOrDefault();
        }

        /// <summary>
        /// Extracts the first image of specified size from an <see cref="Icon"/> instance.
        /// </summary>
        /// <param name="icon">The icon that may contain multiple images.</param>
        /// <param name="size">The required icon size to retrieve</param>
        /// <param name="keepOriginalFormat">If <see langword="true"/>, keeps the original image format. Possible transparent pixels of
        /// non-32 bpp ARGB formats may be black. If <see langword="false"/>, always returns 32 bpp images with transparency.</param>
        /// <returns>An <see cref="Bitmap"/> instance, which was extracted from the <paramref name="icon"/>,
        /// or <see langword="null"/> if no icon found of the specified size.</returns>
        /// <seealso cref="ExtractIcon(Icon,SystemIconSize)"/>
        public static Bitmap ExtractBitmap(this Icon icon, SystemIconSize size, bool keepOriginalFormat)
        {
            if (!Enum<SystemIconSize>.IsDefined(size))
                throw new ArgumentOutOfRangeException("size");

            return ExtractBitmaps(icon, size == SystemIconSize.Small ? size16 : size32, null, keepOriginalFormat).FirstOrDefault();
        }

        /// <summary>
        /// Extracts the first image of specified size from an <see cref="Icon"/> instance.
        /// </summary>
        /// <param name="icon">The icon that may contain multiple images.</param>
        /// <param name="size">The required icon size to retrieve.</param>
        /// <param name="keepOriginalFormat">If <see langword="true"/>, keeps the original image format. Possible transparent pixels of
        /// non-32 bpp ARGB formats may be black. If <see langword="false"/>, always returns 32 bpp images with transparency.</param>
        /// <returns>An <see cref="Bitmap"/> instance, which was extracted from the <paramref name="icon"/>,
        /// or <see langword="null"/> if no icon found of the specified size.</returns>
        /// <seealso cref="ExtractIcon(Icon,Size)"/>
        public static Bitmap ExtractBitmap(this Icon icon, Size size, bool keepOriginalFormat)
        {
            return ExtractBitmaps(icon, size, null, keepOriginalFormat).FirstOrDefault();
        }

        /// <summary>
        /// Extracts the image of specified size and pixel format from an <see cref="Icon"/> instance.
        /// </summary>
        /// <param name="icon">The icon that may contain multiple images.</param>
        /// <param name="size">The required icon size to retrieve</param>
        /// <param name="pixelFormat">The required pixel format to retrieve</param>
        /// <param name="keepOriginalFormat">If <see langword="true"/>, keeps the original image format. Possible transparent pixels of
        /// non-32 bpp ARGB formats may be black. If <see langword="false"/>, always returns 32 bpp images with transparency.</param>
        /// <returns>An <see cref="Bitmap"/> instance, which was extracted from the <paramref name="icon"/>,
        /// or <see langword="null"/> if no icon found of the specified size and format.</returns>
        /// <seealso cref="ExtractIcon(Icon,SystemIconSize,PixelFormat)"/>
        public static Bitmap ExtractBitmap(this Icon icon, SystemIconSize size, PixelFormat pixelFormat, bool keepOriginalFormat)
        {
            return ExtractBitmaps(icon, size == SystemIconSize.Small ? size16 : size32, pixelFormat.ToBitsPerPixel(), keepOriginalFormat).FirstOrDefault();
        }

        /// <summary>
        /// Extracts the image of specified size and pixel format from an <see cref="Icon"/> instance.
        /// </summary>
        /// <param name="icon">The icon that may contain multiple images.</param>
        /// <param name="size">The required icon size to retrieve.</param>
        /// <param name="pixelFormat">The required pixel format to retrieve</param>
        /// <param name="keepOriginalFormat">If <see langword="true"/>, keeps the original image format. Possible transparent pixels of
        /// non-32 bpp ARGB formats may be black. If <see langword="false"/>, always returns 32 bpp images with transparency.</param>
        /// <returns>An <see cref="Bitmap"/> instance, which was extracted from the <paramref name="icon"/>,
        /// or <see langword="null"/> if no icon found of the specified size and format.</returns>
        /// <seealso cref="ExtractIcon(Icon,Size,PixelFormat)"/>
        public static Bitmap ExtractBitmap(this Icon icon, Size size, PixelFormat pixelFormat, bool keepOriginalFormat)
        {
            return ExtractBitmaps(icon, size, pixelFormat.ToBitsPerPixel(), keepOriginalFormat).FirstOrDefault();
        }

        /// <summary>
        /// Extracts the image of specified index from an <see cref="Icon"/> instance.
        /// </summary>
        /// <param name="icon">The icon that may contain multiple images.</param>
        /// <param name="index">The zero-based index of the icon image to retrieve.</param>
        /// <param name="keepOriginalFormat">If <see langword="true"/>, keeps the original image format. Possible transparent pixels of
        /// non-32 bpp ARGB formats may be black. If <see langword="false"/>, always returns 32 bpp images with transparency.</param>
        /// <returns>An <see cref="Bitmap"/> instance, which was extracted from the <paramref name="icon"/>,
        /// or <see langword="null"/> if no icon found of the specified size.</returns>
        /// <seealso cref="ExtractIcon(Icon,int)"/>
        public static Bitmap ExtractBitmap(this Icon icon, int index, bool keepOriginalFormat)
        {
            if (icon == null)
                throw new ArgumentNullException("icon");
            if (index < 0)
                throw new ArgumentOutOfRangeException("index");

            using (RawIcon rawIcon = new RawIcon(icon, null, null, index))
            {
                return rawIcon.ExtractBitmaps(keepOriginalFormat).FirstOrDefault();
            }
        }

        /// <summary>
        /// Extracts the nearest image of specified size and pixel format from an <see cref="Icon"/> instance.
        /// </summary>
        /// <param name="icon">The icon that may contain multiple images.</param>
        /// <param name="size">The required icon size to retrieve.</param>
        /// <param name="pixelFormat">The required pixel format to retrieve</param>
        /// <param name="keepOriginalFormat">If <see langword="true"/>, keeps the original image format. Possible transparent pixels of
        /// non-32 bpp ARGB formats may be black. If <see langword="false"/>, always returns 32 bpp images with transparency.</param>
        /// <returns>An <see cref="Bitmap"/> instance, which was extracted from the <paramref name="icon"/>. If no
        /// icon found of the specified size and format, the nearest image (<paramref name="pixelFormat"/> matches first, then <paramref name="size"/>) is returned.</returns>
        /// <seealso cref="ExtractNearestIcon(Icon,Size,PixelFormat)"/>
        public static Bitmap ExtractNearestBitmap(this Icon icon, Size size, PixelFormat pixelFormat, bool keepOriginalFormat)
        {
            if (icon == null)
                throw new ArgumentNullException("icon");
            int bpp = pixelFormat.ToBitsPerPixel();

            using (RawIcon rawIcon = new RawIcon(icon))
            {
                return rawIcon.ExtractNearestBitmap(bpp, size, keepOriginalFormat);
            }
        }

        /// <summary>
        /// Extracts every icon from an <see cref="Icon"/> instance as separated <seealso cref="Icon"/> instances.
        /// </summary>
        /// <param name="icon">The icon that may contain multiple images.</param>
        /// <returns>An array of <see cref="Icon"/> instances, which were extracted from the <paramref name="icon"/>.</returns>
        /// <seealso cref="ExtractBitmaps(Icon,bool)"/>
        public static Icon[] ExtractIcons(this Icon icon)
        {
            return ExtractIcons(icon, null, null);
        }

        /// <summary>
        /// Extracts every icon of specified size from an <see cref="Icon"/> instance as separated <seealso cref="Icon"/> instances.
        /// </summary>
        /// <param name="icon">The icon that may contain multiple images.</param>
        /// <param name="size">The required icon size to retrieve</param>
        /// <returns>An array of <see cref="Icon"/> instances, which were extracted from the <paramref name="icon"/>.</returns>
        /// <seealso cref="ExtractBitmaps(Icon,SystemIconSize,bool)"/>
        public static Icon[] ExtractIcons(this Icon icon, SystemIconSize size)
        {
            if (!Enum<SystemIconSize>.IsDefined(size))
                throw new ArgumentOutOfRangeException("size");

            return ExtractIcons(icon, size == SystemIconSize.Small ? size16 : size32, null);
        }

        /// <summary>
        /// Extracts every icon of specified size from an <see cref="Icon"/> instance as separated <seealso cref="Icon"/> instances.
        /// </summary>
        /// <param name="icon">The icon that may contain multiple images.</param>
        /// <param name="size">The required icon size to retrieve.</param>
        /// <returns>An array of <see cref="Icon"/> instances, which were extracted from the <paramref name="icon"/>.</returns>
        /// <seealso cref="ExtractBitmaps(Icon,Size,bool)"/>
        public static Icon[] ExtractIcons(this Icon icon, Size size)
        {
            return ExtractIcons(icon, size, null);
        }

        /// <summary>
        /// Extracts every icon of specified pixel format from an <see cref="Icon"/> instance as separated <seealso cref="Icon"/> instances.
        /// </summary>
        /// <param name="icon">The icon that may contain multiple images.</param>
        /// <param name="pixelFormat">The required pixel format to retrieve</param>
        /// <returns>An array of <see cref="Icon"/> instances, which were extracted from the <paramref name="icon"/>.</returns>
        /// <seealso cref="ExtractBitmaps(Icon,PixelFormat,bool)"/>
        public static Icon[] ExtractIcons(this Icon icon, PixelFormat pixelFormat)
        {
            return ExtractIcons(icon, null, pixelFormat.ToBitsPerPixel());
        }

        /// <summary>
        /// Extracts the first icon of specified size from an <see cref="Icon"/> instance.
        /// Unless constructors of <seealso cref="Icon"/> class, this method really works.
        /// </summary>
        /// <param name="icon">The icon that may contain multiple images.</param>
        /// <param name="size">The required icon size to retrieve</param>
        /// <returns>An <see cref="Icon"/> instance, which contains only a single image,
        /// or <see langword="null"/> if no icon found of the specified size.</returns>
        /// <seealso cref="ExtractBitmap(Icon,SystemIconSize,bool)"/>
        public static Icon ExtractIcon(this Icon icon, SystemIconSize size)
        {
            if (!Enum<SystemIconSize>.IsDefined(size))
                throw new ArgumentOutOfRangeException("size");

            return ExtractIcons(icon, size == SystemIconSize.Small ? size16 : size32, null).FirstOrDefault();
        }

        /// <summary>
        /// Extracts the first icon of specified size from an <see cref="Icon"/> instance.
        /// Unless constructors of <seealso cref="Icon"/> class, this method really works.
        /// </summary>
        /// <param name="icon">The icon that may contain multiple images.</param>
        /// <param name="size">The required icon size to retrieve.</param>
        /// <returns>An <see cref="Icon"/> instance, which contains only a single image,
        /// or <see langword="null"/> if no icon found of the specified size.</returns>
        /// <seealso cref="ExtractBitmap(Icon,Size,bool)"/>
        public static Icon ExtractIcon(this Icon icon, Size size)
        {
            return ExtractIcons(icon, size, null).FirstOrDefault();
        }

        /// <summary>
        /// Extracts the icon of specified size and pixel format from an <see cref="Icon"/> instance.
        /// Unless constructors of <seealso cref="Icon"/> class, this method really works.
        /// </summary>
        /// <param name="icon">The icon that may contain multiple images.</param>
        /// <param name="size">The required icon size to retrieve</param>
        /// <param name="pixelFormat">The required pixel format to retrieve</param>
        /// <returns>An <see cref="Icon"/> instance, which contains only a single image,
        /// or <see langword="null"/> if no icon found of the specified size and format.</returns>
        /// <seealso cref="ExtractBitmap(Icon,SystemIconSize,PixelFormat,bool)"/>
        public static Icon ExtractIcon(this Icon icon, SystemIconSize size, PixelFormat pixelFormat)
        {
            return ExtractIcons(icon, size == SystemIconSize.Small ? size16 : size32, pixelFormat.ToBitsPerPixel()).FirstOrDefault();
        }

        /// <summary>
        /// Extracts the icon of specified size and pixel format from an <see cref="Icon"/> instance.
        /// Unless constructors of <seealso cref="Icon"/> class, this method really works.
        /// </summary>
        /// <param name="icon">The icon that may contain multiple images.</param>
        /// <param name="size">The required icon size to retrieve.</param>
        /// <param name="pixelFormat">The required pixel format to retrieve</param>
        /// <returns>An <see cref="Icon"/> instance, which contains only a single image,
        /// or <see langword="null"/> if no icon found of the specified size and format.</returns>
        /// <seealso cref="ExtractBitmap(Icon,Size,PixelFormat,bool)"/>
        public static Icon ExtractIcon(this Icon icon, Size size, PixelFormat pixelFormat)
        {
            return ExtractIcons(icon, size, pixelFormat.ToBitsPerPixel()).FirstOrDefault();
        }

        /// <summary>
        /// Extracts the icon of specified index from an <see cref="Icon"/> instance.
        /// </summary>
        /// <param name="icon">The icon that may contain multiple images.</param>
        /// <param name="index">The zero-based index of the icon image to retrieve.</param>
        /// <returns>An <see cref="Icon"/> instance, which contains only a single image,
        /// or <see langword="null"/> if no icon found with the specified index.</returns>
        /// <seealso cref="ExtractBitmap(Icon,int,bool)"/>
        public static Icon ExtractIcon(this Icon icon, int index)
        {
            if (icon == null)
                throw new ArgumentNullException("icon");
            if (index < 0)
                throw new ArgumentOutOfRangeException("index");

            using (RawIcon rawIcon = new RawIcon(icon, null, null, index))
            {
                return rawIcon.ToIcon();
            }
        }

        /// <summary>
        /// Extracts the nearest icon of specified size and pixel format from an <see cref="Icon"/> instance.
        /// Unless constructors of <seealso cref="Icon"/> class, this method really works.
        /// </summary>
        /// <param name="icon">The icon that may contain multiple images.</param>
        /// <param name="size">The required icon size to retrieve.</param>
        /// <param name="pixelFormat">The required pixel format to retrieve</param>
        /// <returns>An <see cref="Icon"/> instance, which contains only a single image. If no
        /// icon found of the specified size and format, the nearest icon (<paramref name="pixelFormat"/> matches first, then <paramref name="size"/>) is returned.</returns>
        /// <seealso cref="ExtractNearestBitmap(Icon,Size,PixelFormat,bool)"/>
        public static Icon ExtractNearestIcon(this Icon icon, Size size, PixelFormat pixelFormat)
        {
            if (icon == null)
                throw new ArgumentNullException("icon");
            int bpp = pixelFormat.ToBitsPerPixel();

            using (RawIcon rawIcon = new RawIcon(icon))
            {
                return rawIcon.ExtractNearestIcon(bpp, size);
            }
        }

        /// <summary>
        /// Combines an <see cref="Icon"/> instance with the provided <paramref name="icons"/> into a multi-resolution <see cref="Icon"/> instance.
        /// </summary>
        /// <returns>An <see cref="Icon"/> instace that contains every image of the source <paramref name="icons"/>.</returns>
        /// <remarks>Both <paramref name="icon"/> and elements of <paramref name="icons"/> may contain multiple icons.</remarks>
        public static Icon Combine(this Icon icon, params Icon[] icons)
        {
            if (icon == null)
                throw new ArgumentNullException("icon");
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
        /// Combines the provided <paramref name="icons"/> into a multi-resolution <see cref="Icon"/> instance.
        /// </summary>
        /// <param name="icons">The icons to be combined.</param>
        /// <returns>An <see cref="Icon"/> instance that contains every image of the source <paramref name="icons"/>.</returns>
        /// <remarks>The elements of <paramref name="icons"/> may contain multiple icons.</remarks>
        public static Icon Combine(params Icon[] icons)
        {
            if (icons == null || icons.Length == 0)
                return null;

            using (RawIcon rawIcon = new RawIcon())
            {
                foreach (Icon icon in icons)
                {
                    rawIcon.Add(icon);
                }

                return rawIcon.ToIcon();
            }
        }

        /// <summary>
        /// Combines and <see cref="Icon" /> instance with the provided <paramref name="images" /> into a multi-resolution <see cref="Icon" /> instance.
        /// </summary>
        /// <param name="icon">The icon to combine with other images.</param>
        /// <param name="images">The images to be added to the icon. Images can be non-square ones, but cannot be larger than 256x256.</param>
        /// <returns>
        /// An <see cref="Icon" /> instace that contains every image of the source <paramref name="images" />.
        /// </returns>
        /// <remarks><paramref name="icon"/> may already contain multiple icons.</remarks>
        public static Icon Combine(this Icon icon, params Bitmap[] images)
        {
            if (icon == null)
                throw new ArgumentNullException("icon");
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
        /// Combines the provided <paramref name="images"/> into a multi-resolution <see cref="Icon"/> instance.
        /// </summary>
        /// <param name="images">The images to be added to the icon. Images can be non-square ones, but cannot be larger than 256x256.
        /// Transparency is determined automatically by image format.</param>
        /// <returns>An <see cref="Icon"/> instance that contains every image of the source <paramref name="images"/>.</returns>
        public static Icon Combine(params Bitmap[] images)
        {
            if (images == null || images.Length == 0)
                return null;

            using (RawIcon rawIcon = new RawIcon())
            {
                foreach (Bitmap image in images)
                {
                    rawIcon.Add(image);
                }

                return rawIcon.ToIcon();
            }
        }

        /// <summary>
        /// Combines the provided <paramref name="images" /> into a multi-resolution <see cref="Icon" /> instance.
        /// </summary>
        /// <param name="images">The images to be added to the icon. Images can be non-square ones, but cannot be larger than 256x256.</param>
        /// <param name="transparentColors">An array of transparent colors of the images. The array must have as many elements as <paramref name="images"/>.</param>
        /// <returns>
        /// An <see cref="Icon" /> instace that contains every image of the source <paramref name="images" />.
        /// </returns>
        public static Icon Combine(Bitmap[] images, Color[] transparentColors)
        {
            int imageCount = images == null ? 0 : images.Length;
            int colorCount = transparentColors == null ? 0 : transparentColors.Length;
            if (imageCount != colorCount)
                throw new ArgumentException("Length of images and transparentColors must be the same");

            if (images == null || transparentColors == null || imageCount == 0)
                return null;

            using (RawIcon rawIcon = new RawIcon())
            {
                for (int i = 0; i < imageCount; i++)
                {
                    rawIcon.Add(images[i], transparentColors[i]);
                }

                return rawIcon.ToIcon();
            }
        }

        /// <summary>
        /// Saves the icon into the specified <paramref name="stream"/>. Unlike <see cref="Icon.Save"/>, this method can save every icon with high quality, even
        /// <see cref="SystemIcons"/> members, and icons were created by <see cref="Icon.FromHandle"/>.
        /// </summary>
        /// <param name="icon">The icon to save</param>
        /// <param name="stream">A stream into which the icon has to be saved.</param>
        public static void SaveHighQuality(this Icon icon, Stream stream)
        {
            if (icon == null)
                throw new ArgumentNullException("icon");
            if (stream == null)
                throw new ArgumentNullException("stream");

            using (RawIcon rawIcon = new RawIcon(icon))
            {
                rawIcon.Save(stream);
            }
        }

        /// <summary>
        /// Converts the provided <paramref name="icon"/> to a <see cref="CursorHandle"/>, which can be passed to the
        /// <a href="https://msdn.microsoft.com/en-us/library/system.windows.forms.cursor.aspx" target="_blank">System.Windows.Forms.Cursor</a> constructor
        /// to create a new cursor.
        /// </summary>
        /// <param name="icon">The <see cref="Icon"/>, which should be converted to a cursor.</param>
        /// <param name="cursorHotspot">The hotspot coordinates of the cursor.
        /// <br/>Default value: 0; 0 (top-left corner)</param>
        /// <returns>A <see cref="CursorHandle"/> instance that can be used to create a <a href="https://msdn.microsoft.com/en-us/library/system.windows.forms.cursor.aspx" target="_blank">System.Windows.Forms.Cursor</a> instance.</returns>
        public static CursorHandle ToCursorHandle(this Icon icon, Point cursorHotspot = default(Point))
        {
            if (icon == null)
                throw new ArgumentNullException("icon");

            return ToCursorHandle(icon.Handle, cursorHotspot);
        }

        #endregion

        #region Internal Methods

        internal static CursorHandle ToCursorHandle(IntPtr iconHandle, Point cursorHotspot)
        {
            ICONINFO iconInfo;
            if (!User32.GetIconInfo(iconHandle, out iconInfo))
                throw new ArgumentException("Invalid icon", "icon", new Win32Exception(Marshal.GetLastWin32Error()));
            iconInfo.xHotspot = cursorHotspot.X;
            iconInfo.yHotspot = cursorHotspot.Y;
            iconInfo.fIcon = false;
            return new CursorHandle(User32.CreateIconIndirect(ref iconInfo));
        }

        internal static bool HasRawData(this Icon icon)
        {
            if (iconData == null)
            {
                iconData = FieldAccessor.GetFieldAccessor(typeof(Icon).GetField("iconData", BindingFlags.Instance | BindingFlags.NonPublic));
            }

            return iconData.Get(icon) != null;
        }

        #endregion

        #region Private Methods

        private unsafe static bool IsFullyTransparent(BitmapData data)
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

        /// <summary>
        /// Needed for unmanaged icons returned by winapi methods because <see cref="Icon.FromHandle"/> does not
        /// take ownership of handle and does not dispose the icon.
        /// </summary>
        private static Icon GetManagedIcon(Icon unmanagedIcon)
        {
            Icon managedIcon = (Icon)unmanagedIcon.Clone();
            User32.DestroyIcon(unmanagedIcon.Handle);
            return managedIcon;
        }

        private static Bitmap[] ExtractBitmaps(Icon icon, Size? size, int? bpp, bool keepOriginalFormat)
        {
            if (icon == null)
                throw new ArgumentNullException("icon");

            using (RawIcon rawIcon = new RawIcon(icon, size, bpp, null))
            {
                return rawIcon.ExtractBitmaps(keepOriginalFormat);
            }
        }

        private static Icon[] ExtractIcons(Icon icon, Size? size, int? bpp)
        {
            if (icon == null)
                throw new ArgumentNullException("icon");

            using (RawIcon rawIcon = new RawIcon(icon, size, bpp, null))
            {
                return rawIcon.ExtractIcons();
            }
        }

        #endregion

        #endregion
    }
}
