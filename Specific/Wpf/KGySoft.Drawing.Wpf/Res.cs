#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: Res.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2022 - All Rights Reserved
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
    internal static class Res
    {
        #region Constants

        private const string unavailableResource = "Resource ID not found: {0}";
        private const string invalidResource = "Resource text is not valid for {0} arguments: {1}";

        #endregion

        #region Fields

        private static readonly DynamicResourceManager resourceManager = new DynamicResourceManager("KGySoft.Drawing.Wpf.Messages", typeof(Res).Assembly)
        {
            SafeMode = true,
            UseLanguageSettings = true,
        };

        #endregion

        #region Properties

        /// <summary>The bitmap must not be frozen.</summary>
        internal static string WriteableBitmapExtensionsBitmapFrozen => Get("WriteableBitmapExtensions_BitmapFrozen");

        /// <summary>The IQuantizer.Initialize method returned a null reference.</summary>
        internal static string BitmapSourceExtensionsQuantizerInitializeNull => Get("BitmapSourceExtensions_QuantizerInitializeNull");

        /// <summary>Could not perform a callback on the thread of the source bitmap. It can be due to a blocking wait on the returned task or async result, or because there is no running dispatcher.</summary>
        internal static string BitmapSourceExtensionsDeadlock => Get("BitmapSourceExtensions_Deadlock");

        #endregion

        #region Methods

        #region Internal Methods

        /// <summary>Internal Error: {0}</summary>
        /// <remarks>Use this method to avoid CA1303 for using string literals in internal errors that never supposed to occur.</remarks>
        internal static string InternalError(string msg) => Get("General_InternalErrorFormat", msg);

        /// <summary>Palette must not have more than {0} colors for a pixel format of {1} bits per pixel.</summary>
        internal static string BitmapSourceExtensionsPaletteTooLarge(int max, int bpp) => Get("BitmapSource_ExtensionsPaletteTooLargeFormat", max, bpp);

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
