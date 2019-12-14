#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: Res.cs
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
using System.Drawing.Imaging;
using System.Globalization;

using KGySoft.CoreLibraries;
using KGySoft.Resources;

#endregion

namespace KGySoft
{
    /// <summary>
    /// Contains the string resources of the project.
    /// </summary>
    internal static class Res
    {
        #region Constants

        private const string unavailableResource = "Resource ID not found: {0}";
        private const string invalidResource = "Resource text is not valid for {0} arguments: {1}";

        #endregion

        #region Fields

        private static readonly DynamicResourceManager resourceManager = new DynamicResourceManager("KGySoft.Drawing.Messages", typeof(Res).Assembly)
        {
            SafeMode = true,
            UseLanguageSettings = true,
        };

        #endregion

        #region Properties

        #region Gdi32

        /// <summary>Invalid GDI object handle.</summary>
        internal static string Gdi32InvalidHandle => Get("Gdi32_InvalidHandle");

        /// <summary>Could not retrieve Enhanced Metafile content.</summary>
        internal static string Gdi32GetEmfContentFailed => Get("Gdi32_GetEmfContentFailed");

        /// <summary>Could not retrieve Windows Metafile content.</summary>
        internal static string Gdi32GetWmfContentFailed => Get("Gdi32_GetWmfContentFailed");

        /// <summary>Invalid Enhanced Metafile handle.</summary>
        internal static string Gdi32InvalidEmfHandle => Get("Gdi32_InvalidEmfHandle");

        #endregion

        #region IconExtensions

        /// <summary>Length of images and transparentColors must be the same.</summary>
        internal static string IconExtensionsImagesColorsDifferentLength => Get("IconExtensions_ImagesColorsDifferentLength");

        #endregion

        #region ImageExtensions

        /// <summary>TIFF encoder not found.</summary>
        internal static string ImageExtensionsNoTiffEncoder => Get("ImageExtensions_NoTiffEncoder");

        /// <summary>GIF encoder not found.</summary>
        internal static string ImageExtensionsNoGifEncoder => Get("ImageExtensions_NoGifEncoder");

        #endregion

        #region RawIcon

        /// <summary>Too many images in the icon collection.</summary>
        internal static string RawIconTooManyImages => Get("RawIcon_TooManyImages");

        /// <summary>Bad icon format.</summary>
        internal static string RawIconBadIconFormat => Get("RawIcon_BadIconFormat");

        /// <summary>16/48/64 bpp images are not supported for icons.</summary>
        internal static string RawIconUnsupportedBpp => Get("RawIcon_UnsupportedBpp");
        
        #endregion

        #region User32

        /// <summary>Invalid handle.</summary>
        internal static string User32InvalidHandle => Get("User32_InvalidHandle");

        /// <summary>Could not create icon or cursor.</summary>
        internal static string User32CreateIconIndirectFailed => Get("User32_CreateIconIndirectFailed");

        #endregion

        #endregion

        #region Methods

        #region Internal Methods

        #region Accessors

        /// <summary>Instance field "{0}" not found on type "{1}".</summary>
        internal static string AccessorsInstanceFieldDoesNotExist(string fieldName, Type type) => Get("Accessors_InstanceFieldDoesNotExistFormat", fieldName, type);

        #endregion

        #region ImageExtensions

        /// <summary>Pixel format '{0}' is not supported by GDI+.</summary>
        internal static string ImageExtensionsPixelFormatNotSupported(PixelFormat pixelFormat) => Get("ImageExtensions_PixelFormatNotSupportedFormat", Enum<PixelFormat>.ToString(pixelFormat));

        #endregion

        #endregion

        #region Private Methods

        private static string Get(string id) => resourceManager.GetString(id, LanguageSettings.DisplayLanguage) ?? String.Format(CultureInfo.InvariantCulture, unavailableResource, id);

        private static string Get(string id, params object[] args)
        {
            string format = Get(id);
            return args == null ? format : SafeFormat(format, args);
        }

        private static string SafeFormat(string format, object[] args)
        {
            try
            {
                int i = Array.IndexOf(args, null);
                if (i >= 0)
                {
                    string nullRef = PublicResources.Null;
                    for (; i < args.Length; i++)
                    {
                        if (args[i] == null)
                            args[i] = nullRef;
                    }
                }

                return String.Format(LanguageSettings.FormattingLanguage, format, args);
            }
            catch (FormatException)
            {
                return String.Format(CultureInfo.InvariantCulture, invalidResource, args.Length, format);
            }
        }

        #endregion

        #endregion
    }
}
