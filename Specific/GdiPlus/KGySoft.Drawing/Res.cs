#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: Res.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2023 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;
#if NET7_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif
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
#if NET7_0_OR_GREATER
    [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "PixelFormat is an enum so it is supported on every platform.")]
#endif
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

        #region General

        /// <summary>This operation is supported on Windows only.</summary>
        internal static string RequiresWindows => Get("General_RequiresWindows");

        #endregion

        #region Gdi32

        /// <summary>Invalid GDI object handle.</summary>
        internal static string Gdi32InvalidHandle => Get("Gdi32_InvalidHandle");

        /// <summary>Could not retrieve Enhanced Metafile content.</summary>
        internal static string Gdi32GetEmfContentFailed => Get("Gdi32_GetEmfContentFailed");

        /// <summary>Could not retrieve Windows Metafile content.</summary>
        internal static string Gdi32GetWmfContentFailed => Get("Gdi32_GetWmfContentFailed");

        /// <summary>Invalid Enhanced Metafile handle.</summary>
        internal static string Gdi32InvalidEmfHandle => Get("Gdi32_InvalidEmfHandle");

        /// <summary>Invalid Windows Metafile handle.</summary>
        internal static string Gdi32InvalidWmfHandle => Get("Gdi32_InvalidWmfHandle");

        #endregion

        #region GraphicsExtensions

        /// <summary>A Graphics from Metafile is not supported. Once the Graphics is disposed you can use the MetafileExtensions.ToBitmap methods to convert the Metafile to a Bitmap.</summary>
        internal static string GraphicsExtensionsToBitmapMetafileNotSupported => Get("GraphicsExtensions_ToBitmapMetafileNotSupported");

        #endregion

        #region IconExtensions

        /// <summary>Length of images and transparentColors must be the same.</summary>
        internal static string IconExtensionsImagesColorsDifferentLength => Get("IconExtensions_ImagesColorsDifferentLength");

        #endregion

        #region ImageExtensions

        /// <summary>Saving multi-page TIFF is not supported on the current platform.</summary>
        internal static string ImageExtensionsMultipageTiffSaveNotSupported => Get("ImageExtensions_MultipageTiffSaveNotSupported");

        /// <summary>The IQuantizer.Initialize method returned a null reference.</summary>
        internal static string ImageExtensionsQuantizerInitializeNull => Get("ImageExtensions_QuantizerInitializeNull");

        /// <summary>The IDitherer.Initialize method returned a null reference.</summary>
        internal static string ImageExtensionsDithererInitializeNull => Get("ImageExtensions_DithererInitializeNull");

        #endregion

        #region MetafileExtensions

        /// <summary>The specified metafile can only be saved as WMF.</summary>
        internal static string MetafileExtensionsCannotBeSavedAsEmf => Get("MetafileExtensions_CannotBeSavedAsEmf");

        #endregion

        #region RawIcon

        /// <summary>There are too many images in the icon collection.</summary>
        internal static string RawIconTooManyImages => Get("RawIcon_TooManyImages");

        /// <summary>Bad icon format.</summary>
        internal static string RawIconBadIconFormat => Get("RawIcon_BadIconFormat");

        /// <summary>On this platform this icon cannot be instantiated with the current size or compression.</summary>
        internal static string RawIconCannotBeInstantiatedAsIcon => Get("RawIcon_CannotBeInstantiatedAsIcon");

        /// <summary>On this platform this icon cannot be instantiated as a bitmap with the current size or compression.</summary>
        internal static string RawIconCannotBeInstantiatedAsBitmap => Get("RawIcon_CannotBeInstantiatedAsBitmap");

        #endregion

        #region User32

        /// <summary>Invalid handle.</summary>
        internal static string User32InvalidHandle => Get("User32_InvalidHandle");

        /// <summary>Could not create icon or cursor.</summary>
        internal static string User32CreateIconIndirectFailed => Get("User32_CreateIconIndirectFailed");

        #endregion

        #region Imaging

        /// <summary>The bitmap data has an invalid size.</summary>
        internal static string ImagingInvalidBitmapDataSize => Get("Imaging_InvalidBitmapDataSize");

        #endregion

        #endregion

        #region Methods

        #region Internal Methods

        #region General

        /// <summary>
        /// Just an empty method to be able to trigger the static constructor without running any code other than field initializations.
        /// </summary>
        internal static void EnsureInitialized()
        {
        }

        /// <summary>Internal Error: {0}</summary>
        /// <remarks>Use this method to avoid CA1303 for using string literals in internal errors that never supposed to occur.</remarks>
        internal static string InternalError(string msg) => Get("General_InternalErrorFormat", msg);

        /// <summary>Pixel format '{0}' does not represent an actual format.</summary>
        internal static string PixelFormatInvalid(PixelFormat pixelFormat) => Get("General_PixelFormatInvalidFormat", Enum<PixelFormat>.ToString(pixelFormat));

        /// <summary>Pixel format '{0}' is not supported by native Bitmaps on the current platform.</summary>
        internal static string PixelFormatNotSupported(PixelFormat pixelFormat) => Get("General_PixelFormatNotSupportedFormat", Enum<PixelFormat>.ToString(pixelFormat));

        /// <summary>Palette must not have more than {0} colors for a pixel format of {1} bits per pixel.</summary>
        internal static string PaletteTooLarge(int max, int bpp) => Get("General_PaletteTooLargeFormat", max, bpp);

        #endregion

        #region ImageExtensions

        /// <summary>No encoder was found for the '{0}' format.</summary>
        internal static string ImageExtensionsNoEncoder(ImageFormat imageFormat) => Get("ImageExtensions_NoEncoderFormat", imageFormat);

        /// <summary>Could not save the image by the '{0}' encoder.</summary>
        internal static string ImageExtensionsEncoderSaveFail(ImageFormat imageFormat) => Get("ImageExtensions_EncoderSaveFailFormat", imageFormat);

        #endregion

        #endregion

        #region Private Methods

        private static string Get(string id) => resourceManager.GetString(id, LanguageSettings.DisplayLanguage) ?? String.Format(CultureInfo.InvariantCulture, unavailableResource, id);

        private static string Get(string id, params object?[]? args)
        {
            string format = Get(id);
            return args == null ? format : SafeFormat(format, args);
        }

        private static string SafeFormat(string format, object?[] args)
        {
            try
            {
                int i = Array.IndexOf(args, null);
                if (i >= 0)
                {
                    string nullRef = PublicResources.Null;
                    for (; i < args.Length; i++)
                        args[i] ??= nullRef;
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
