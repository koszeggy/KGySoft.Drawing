#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: SkiaSharpRes.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2026 - All Rights Reserved
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
using KGySoft.Resources;

using SkiaSharp;

#endregion

namespace KGySoft.Drawing.SkiaSharp
{
    /// <summary>
    /// Contains the string resources of the project.
    /// </summary>
    internal static class SkiaSharpRes
    {
        #region Constants

        private const string unavailableResource = "Resource ID not found: {0}";
        private const string invalidResource = "Resource text is not valid for {0} arguments: {1}";

        #endregion

        #region Fields

        private static readonly DynamicResourceManager resourceManager = new DynamicResourceManager("KGySoft.Drawing.SkiaSharp.Messages", typeof(SkiaSharpRes).Assembly)
        {
            SafeMode = true,
            UseLanguageSettings = true,
        };

        #endregion

        #region Properties

        /// <summary>The bitmap data has an invalid size.</summary>
        internal static string ImagingInvalidBitmapDataSize => Get("Imaging_InvalidBitmapDataSize");

        /// <summary>The IQuantizer.Initialize method returned a null reference.</summary>
        internal static string ImageExtensionsQuantizerInitializeNull => Get("ImageExtensions_QuantizerInitializeNull");

        #endregion

        #region Methods

        #region Internal Methods

        /// <summary>
        /// Just an empty method to be able to trigger the static constructor without running any code other than field initializations.
        /// </summary>
        internal static void EnsureInitialized()
        {
        }

        /// <summary>The color type '{0}' and alpha type '{1}' do not represent a valid image info.</summary>
        internal static string ImageInfoInvalid(SKColorType colorType, SKAlphaType alphaType) => Get("General_ImageInfoInvalidFormat", Enum<SKColorType>.ToString(colorType), Enum<SKAlphaType>.ToString(alphaType));

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
