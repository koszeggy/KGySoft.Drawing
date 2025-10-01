#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: SkiaSharpModule.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2025 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

#endregion

namespace KGySoft.Drawing.SkiaSharp
{
    /// <summary>
    /// Represents the <c>KGySoft.Drawing.SkiaSharp</c> module.
    /// <br/>See the <strong>Remarks</strong> section of the <see cref="Initialize">Initialize</see> method for details.
    /// </summary>
    public static class SkiaSharpModule
    {
        #region Methods

        /// <summary>
        /// Initializes the <c>KGySoft.SkiaSharp</c> module (and also the <c>KGySoft.Drawing.Core</c> module).
        /// It initializes the resource manager for string resources and registers its central management
        /// in the <a href="http://docs.kgysoft.net/corelibraries/html/T_KGySoft_LanguageSettings.htm">LanguageSettings</a> class.
        /// </summary>
        /// <remarks>
        /// <note>The module initializer is executed automatically when any member is accessed in the module for the first time. This method is public to be able
        /// to trigger module initialization without performing any other operation. Normally you don't need to call it explicitly, but it can be useful if you use
        /// the KGy SOFT Drawing Libraries in an application, and you want to configure resource management on starting the application via
        /// the <a href="http://docs.kgysoft.net/corelibraries/html/T_KGySoft_LanguageSettings.htm">LanguageSettings</a> class.
        /// In such case you can call this method before configuring language settings to make sure that the resources of
        /// the <c>KGySoft.Drawing.SkiaSharp.dll</c> are also affected by the settings.</note>
        /// </remarks>
        /// <example>
        /// The following example demonstrates how to initialize the <c>KGySoft.Drawing.SkiaSharp</c> module in an application (you don't really need to do this
        /// if you use KGy SOFT Drawing SkiaSharp Libraries from a class library):
        /// <code lang="C#"><![CDATA[
        /// using KGySoft;
        /// using KGySoft.Drawing.SkiaSharp;
        /// using KGySoft.Resources;
        /// 
        /// public class Example
        /// {
        ///     public static void Main()
        ///     {
        ///         // To make sure that configuring LanguageSettings affects also the resources in KGySoft.Drawing.SkiaSharp
        ///         SkiaSharpModule.Initialize();
        ///
        ///         // Opting in to use compiled and .resx resources for the application
        ///         LanguageSettings.DynamicResourceManagersSource = ResourceManagerSources.CompiledAndResX;
        ///         LanguageSettings.DisplayLanguage = MyConfigs.GetLastlyUsedLanguage(); // Get some CultureInfo
        /// 
        ///         // Optional: To add possibly new resource entries to the localization of the current language
        ///         LanguageSettings.EnsureInvariantResourcesMerged();
        ///
        ///         // Now you can launch the actual application
        ///         LaunchMyApplication(); // whatever your app actually does
        ///     }
        /// }]]></code>
        /// </example>
#if !NET5_0
        [SuppressMessage("Usage", "CA2255:The 'ModuleInitializer' attribute should not be used in libraries",
            Justification = "See the comment, it is intended and is important to work properly.")] 
#endif
        [ModuleInitializer]
        public static void Initialize()
        {
            DrawingCoreModule.Initialize();

            // Just referencing Res in order to trigger its static constructor and initialize the project resources.
            // Thus configuring LanguageSettings in a consumer project will work for resources of KGySoft.Drawing even if Res was not accessed yet.
            SkiaSharpRes.EnsureInitialized();
        }

        #endregion
    }
}