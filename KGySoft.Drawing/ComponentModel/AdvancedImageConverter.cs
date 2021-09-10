#if !(NETCOREAPP2_0 || NETSTANDARD2_0 || NETSTANDARD2_1)
#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: AdvancedImageConverter.cs
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
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;

using KGySoft.CoreLibraries;
using KGySoft.Drawing;

#endregion

namespace KGySoft.ComponentModel
{
    /// <summary>
    /// Provides a converter for <see cref="Image"/> instances that can preserve the original format of images better than the <see cref="ImageConverter"/> class when converting <see cref="Bitmap"/>,
    /// <see cref="Metafile"/> and <see cref="Icon"/> images.
    /// </summary>
    /// <remarks>
    /// <note>This class is not available in the .NET Core 2.0 and .NET Standard 2.0/2.1 versions.</note>
    /// </remarks>
    public class AdvancedImageConverter : ImageConverter
    {
        #region Methods

        /// <summary>
        /// Converts a specified object to an <see cref="Image" />.
        /// </summary>
        /// <param name="context">An <see cref="ITypeDescriptorContext" /> that provides a format context. In this converter this parameter is ignored.</param>
        /// <param name="culture">A <see cref="CultureInfo" />. In this converter this parameter is ignored.</param>
        /// <param name="value">The <see cref="object" /> to be converted.</param>
        /// <returns>If this method succeeds, it returns the <see cref="Image" /> that it created by converting the specified object. Otherwise, it throws an exception.</returns>
        public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object? value)
            => value is Icon icon
            ? icon.ToMultiResBitmap() // Base calls just icon.ToBitmap() here, which loses information.
            : base.ConvertFrom(context, culture, value!);

        /// <summary>
        /// Converts an <see cref="Image" /> (or an object that can be cast to an <see cref="Image" />) to the specified type.
        /// </summary>
        /// <param name="context">An <see cref="ITypeDescriptorContext" /> that provides a format context. In this converter this parameter is ignored.</param>
        /// <param name="culture">A <see cref="CultureInfo" />. In this converter this parameter is ignored.</param>
        /// <param name="value">The <see cref="Image" /> to convert.</param>
        /// <param name="destinationType">The <see cref="Type" /> to convert the <see cref="Image" /> to.
        /// This type converter supports <see cref="Array">byte[]</see> type.</param>
        /// <returns>An <see cref="object" /> that represents the converted value.</returns>
        public override object ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
        {
            if (destinationType != typeof(byte[]) || !(value is Image))
                return base.ConvertTo(context, culture, value, destinationType);

            // 1.) Metafile: Saving as EMF/WMF (base would save a PNG here)
            if (value is Metafile metafile)
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
                Bitmap[] images = bitmap.ExtractIconImages();
                using (Icon icon = Icons.Combine(images))
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

            // 4.) Any other: base works well
            return base.ConvertTo(context, culture, value, destinationType);
        }

        #endregion
    }
}

#endif