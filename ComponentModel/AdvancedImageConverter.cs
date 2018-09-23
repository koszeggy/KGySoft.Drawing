using System.Drawing;
using System.Drawing.Imaging;
using KGySoft.Reflection;

namespace KGySoft.ComponentModel
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;

    using KGySoft.Drawing;
    using KGySoft.Libraries;

    /// <summary>
    /// Represents a class that can be used to convert <see cref="Bitmap"/>, <see cref="Metafile"/> and <see cref="Icon"/>
    /// images in a better way than <see cref="ImageConverter"/>, which means that the content and the original format of the <see cref="Image"/> instances
    /// will be preserved better than by using <see cref="System.Drawing.ImageConverter"/>.
    /// </summary>
    /// <seealso cref="Reflector.RegisterTypeConverter{T,TC}"/>
    public class AdvancedImageConverter: ImageConverter
    {
        /// <summary>
        /// Converts a specified object to an <see cref="Image" />.
        /// </summary>
        /// <param name="context">An <see cref="ITypeDescriptorContext" /> that provides a format context.</param>
        /// <param name="culture">A <see cref="CultureInfo" /> that holds information about a specific culture.</param>
        /// <param name="value">The <see cref="object" /> to be converted.</param>
        /// <returns>
        /// If this method succeeds, it returns the <see cref="Image" /> that it created by converting the specified object. Otherwise, it throws an exception.
        /// </returns>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            // Base calls just icon.ToBitmap() here, which loses information.
            var icon = value as Icon;
            return icon != null ? icon.ToMultiResBitmap() : base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType != typeof(byte[]) || !(value is Image))
                return base.ConvertTo(context, culture, value, destinationType);

            // 1.) Metafile: Saving as EMF/WMF (base would save a PNG here)
            var metafile = value as Metafile;
            if (metafile != null)
            {
                using (var ms = new MemoryStream())
                {
                    metafile.Save(ms);
                    return ms.ToArray();
                }
            }

            // 2.) Icon bitmap: Preserving images at least in one color depth
            var bitmap = (Bitmap)value;
            if (bitmap.RawFormat.Guid == ImageFormat.Icon.Guid)
            {
                Bitmap[] images = bitmap.ExtractBitmaps();
                using (Icon icon = IconTools.Combine(images))
                {
                    images.ForEach(i => i.Dispose());
                    using (var ms = new MemoryStream())
                    {
                        icon.Save(ms);
                        return ms.ToArray();
                    }
                }
            }

            // 3.) TIFF: Saving every page
            if (bitmap.RawFormat.Guid == ImageFormat.Tiff.Guid)
            {
                Bitmap[] images = bitmap.ExtractBitmaps();
                using (var ms = new MemoryStream())
                {
                    images.SaveAsMultipageTiff(ms);
                    return ms.ToArray();
                }
            }

            // 4.) GIF: TODO: put a SaveAsGif into the Drawing

            // 5.) Any other: base works well.
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
