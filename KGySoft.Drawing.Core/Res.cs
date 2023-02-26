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
using System.Globalization;

using KGySoft.CoreLibraries;
using KGySoft.Drawing.Imaging;
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

        private static readonly DynamicResourceManager resourceManager = new DynamicResourceManager("KGySoft.Drawing.Core.Messages", typeof(Res).Assembly)
        {
            SafeMode = true,
            UseLanguageSettings = true,
        };

        #endregion

        #region Properties

        #region General

        /// <summary>The operation was canceled.</summary>
        internal static string OperationCanceled => Get("General_OperationCanceled");

        #endregion

        #region Imaging

        /// <summary>This method can be used only on bitmaps with indexed pixel format.</summary>
        internal static string ImagingInvalidOperationIndexedOnly => Get("Imaging_InvalidOperationIndexedOnly");

        /// <summary>The IQuantizer.Initialize method returned a null reference.</summary>
        internal static string ImagingQuantizerInitializeNull => Get("Imaging_QuantizerInitializeNull");

        /// <summary>The IDitherer.Initialize method returned a null reference.</summary>
        internal static string ImagingDithererInitializeNull => Get("Imaging_DithererInitializeNull");

        /// <summary>Not a valid bitmap data stream.</summary>
        internal static string ImagingNotBitmapDataStream => Get("Imaging_NotBitmapDataStream");

        /// <summary>The bitmap data has an invalid size.</summary>
        internal static string ImagingInvalidBitmapDataSize => Get("Imaging_InvalidBitmapDataSize");

        /// <summary>This method expects a non-indexed pixel format.</summary>
        internal static string ImagingNonIndexedPixelFormatExpected => Get("Imaging_NonIndexedPixelFormatExpected");

        /// <summary>This method expects an indexed pixel format.</summary>
        internal static string ImagingIndexedPixelFormatExpected => Get("Imaging_IndexedPixelFormatExpected");

        /// <summary>The specified width is too large for the given buffer width and pixel format.</summary>
        internal static string ImagingWidthTooLarge => Get("Imaging_WidthTooLarge");

        /// <summary>An indexed pixel format should not be larger than 16 bits per pixel.</summary>
        internal static string ImagingIndexedPixelFormatTooLarge => Get("Imaging_IndexedPixelFormatTooLarge");

        /// <summary>For a premultiplied color the alpha value should not be smaller than the color components.</summary>
        internal static string ImagingInvalidPremultipliedValues => Get("Imaging_InvalidPremultipliedValues");

        #endregion

        #region GifEncoder

        /// <summary>This property cannot be set after adding the first image.</summary>
        internal static string GifEncoderCannotChangeProperty => Get("GifEncoder_CannotChangeProperty");

        /// <summary>The palette must not have more than 256 colors.</summary>
        internal static string GifEncoderPaletteTooLarge => Get("GifEncoder_PaletteTooLarge");

        /// <summary>GIF comments cannot be longer than 255 characters.</summary>
        internal static string GifEncoderCommentTooLong => Get("GifEncoder_CommentTooLong");

        /// <summary>GIF comments can consist of ASCII characters only.</summary>
        internal static string GifEncoderCommentNotAscii => Get("GifEncoder_CommentNotAscii");

        /// <summary>Encoder options did not return any frames.</summary>
        internal static string GifEncoderAnimationContainsNoFrames => Get("GifEncoder_AnimationContainsNoFrames");

        /// <summary>Encoder options returned a null frame. You must initialize GifEncodingOptions with an enumeration that does not have a null element.</summary>
        internal static string GifEncoderNullFrame => Get("GifEncoder_NullFrame");

        /// <summary>A frame had an unexpected size. Set the GifEncodingOptions.SizeHandling property to allow different input sizes.</summary>
        internal static string GifEncoderUnexpectedFrameSize => Get("GifEncoder_UnexpectedFrameSize");

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
        internal static string PixelFormatInvalid(KnownPixelFormat pixelFormat) => Get("General_PixelFormatInvalidFormat", Enum<KnownPixelFormat>.ToString(pixelFormat));

        /// <summary>Either the IAsyncResult object did not come from the corresponding '{0}' method, or the End method was called multiple times with the same IAsyncResult.</summary>
        internal static string InvalidAsyncResult(string beginMethodName) => Get("General_InvalidAsyncResultFormat", beginMethodName);

        #endregion

        #region Imaging

        /// <summary>Palette must not have more than {0} colors for a pixel format of {1} bits per pixel.</summary>
        internal static string ImagingPaletteTooLarge(int max, int bpp) => Get("Imaging_PaletteTooLargeFormat", max, bpp);

        /// <summary>For the given pixel format and width stride must not be less than {0}.</summary>
        internal static string ImagingStrideTooSmall(int min) => Get("Imaging_StrideTooSmallFormat", min);

        /// <summary>For element type '{0}' stride must be a multiple of {1}.</summary>
        internal static string ImagingStrideInvalid(Type t, int size) => Get("Imaging_StrideInvalidFormat", t, size);

        /// <summary>The specified buffer should have at least {0} elements for the specified size, stride and pixel format.</summary>
        internal static string ImagingBufferLengthTooSmall(int minSize) => Get("Imaging_BufferLengthTooSmallFormat", minSize);

        /// <summary>Palette index {0} is invalid. It must be less than palette count {1}.</summary>
        internal static string ImagingInvalidPaletteIndex(int index, int count) => Get("Imaging_InvalidPaletteIndexFormat", index, count);

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
