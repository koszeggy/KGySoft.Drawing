#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: WpfRes.cs
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

using KGySoft.Resources;

#endregion

namespace KGySoft.Drawing.Wpf
{
    /// <summary>
    /// Contains the string resources of the project.
    /// </summary>
    internal static class WpfRes
    {
        #region Constants

        private const string unavailableResource = "Resource ID not found: {0}";
        private const string invalidResource = "Resource text is not valid for {0} arguments: {1}";

        #endregion

        #region Fields

        private static readonly DynamicResourceManager resourceManager = new DynamicResourceManager("KGySoft.Drawing.Wpf.Messages", typeof(WpfRes).Assembly)
        {
            SafeMode = true,
            UseLanguageSettings = true,
        };

        #endregion

        #region Properties

        /// <summary>The bitmap must not be frozen.</summary>
        internal static string BitmapFrozen => Get("BitmapFrozen");

        /// <summary>The IQuantizer.Initialize method returned a null reference.</summary>
        internal static string QuantizerInitializeNull => Get("QuantizerInitializeNull");

        /// <summary>Could not perform a callback on the thread of the source bitmap. It can be due to a blocking wait on the returned task or async result, or because there is no running dispatcher.</summary>
        internal static string DispatcherDeadlock => Get("DispatcherDeadlock");

        /// <summary>The bitmap data has an invalid size.</summary>
        internal static string InvalidBitmapDataSize => Get("InvalidBitmapDataSize");

        /// <summary>The size of the BitmapSource is too large.</summary>
        internal static string BitmapSourceDataTooLarge => Get("BitmapSourceDataTooLarge");

        #endregion

        #region Methods

        #region Internal Methods

        /// <summary>
        /// Just an empty method to be able to trigger the static constructor without running any code other than field initializations.
        /// </summary>
        internal static void EnsureInitialized()
        {
        }

        /// <summary>Palette must not have more than {0} colors for a pixel format of {1} bits per pixel.</summary>
        internal static string PaletteTooLarge(int max, int bpp) => Get("PaletteTooLargeFormat", max, bpp);

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
