using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using KGySoft.CoreLibraries;
using KGySoft.Reflection;
using KGySoft.Resources;

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

        #region General

        /// <summary>&lt;null&gt;</summary>
        internal static string NullReference => Get("General_NullReference");

        /// <summary>Value cannot be null.</summary>
        internal static string ArgumentNull => Get("General_ArgumentNull");

        /// <summary>Input string contains an invalid value.</summary>
        internal static string ArgumentInvalidString => Get("General_ArgumentInvalidString");

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

        #region General

        /// <summary>Enum instance of '{0}' type must be one of the defined values.</summary>
        internal static string EnumOutOfRange<TEnum>(TEnum value = default) where TEnum : struct, IConvertible => Get("General_EnumOutOfRangeFormat", value.GetType().Name);

        ///// <summary>Enum instance of '{0}' type must consist of the defined flags.</summary>
        //internal static string FlagsEnumOutOfRange<TEnum>(TEnum value = default) where TEnum : struct, IConvertible => Get("General_FlagsEnumOutOfRangeFormat", value.GetType().Name);

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
                    string nullRef = NullReference;
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
