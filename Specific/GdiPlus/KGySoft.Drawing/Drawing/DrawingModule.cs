#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: DrawingModule.cs
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
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

#if NET5_0 || NET6_0
using KGySoft.Drawing.WinApi;
#endif

#endregion

namespace KGySoft.Drawing
{
    /// <summary>
    /// Represents the <c>KGySoft.Drawing</c> module.
    /// <br/>See the <strong>Remarks</strong> section of the <see cref="Initialize">Initialize</see> method for details.
    /// </summary>
    public static class DrawingModule
    {
        #region Methods

        /// <summary>
        /// Initializes the <c>KGySoft.Drawing</c> module (and also the <c>KGySoft.Drawing.Core</c> module).
        /// It initializes the resource manager for string resources and registers its central management
        /// in the <a href="http://docs.kgysoft.net/corelibraries/html/T_KGySoft_LanguageSettings.htm">LanguageSettings</a> class.
        /// </summary>
        /// <remarks>
        /// <note>The module initializer is executed automatically when any member is accessed in the module for the first time. This method is public to able
        /// to trigger module initialization without performing any other operation. Normally you don't need to call it explicitly but it can be useful if you use
        /// the KGy SOFT Drawing Libraries in an application and you want to configure resource management on starting the application via
        /// the <a href="http://docs.kgysoft.net/corelibraries/html/T_KGySoft_LanguageSettings.htm">LanguageSettings</a> class.
        /// In such case you can call this method before configuring language settings to make sure that the resources of
        /// the <c>KGySoft.Drawing.dll</c> are also affected by the settings.</note>
        /// <note type="important">If you target .NET 5 or 6 and use KGy SOFT Drawing Libraries from a non-Windows application, then make sure you call this method
        /// before using any <c>System.Drawing</c> type to prevent a <see cref="TypeInitializationException"/> without editing the <c>runtimeconfig.json</c> file.
        /// It is not necessary if you reference the .NET Framework build under Mono. When targeting .NET 7 or later, this package can be used only on Windows.
        /// You can use the <a href="https://www.nuget.org/packages/KGySoft.Drawing.Common/" target="_blank">KGySoft.Drawing.Common</a> package on all platforms though.</note>
        /// </remarks>
        /// <example>
        /// The following example demonstrates how to initialize the <c>KGySoft.Drawing</c> module in an application (you don't really need to do this
        /// if you use KGy SOFT Drawing Libraries from a class library):
        /// <code lang="C#"><![CDATA[
        /// using KGySoft;
        /// using KGySoft.Drawing;
        /// using KGySoft.Resources;
        /// 
        /// public class Example
        /// {
        ///     public static void Main()
        ///     {
        ///         // To make sure that configuring LanguageSettings affects also the resources in KGySoft.Drawing
        ///         // For non-Windows applications in .NET 5 and 6 it also enables using System.Drawing types
        ///         DrawingModule.Initialize();
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
            Res.EnsureInitialized();

#if NET5_0 || NET6_0
            // Making sure that the static constructor of the NativeMethods.Gdip class does not throw an exception
            // NOTE: Affects only .NET 5 and 6 because .NET Core 3.x and below an older version is referenced (see the csproj and https://github.com/dotnet/runtime/issues/59032) 
            if (!OSUtils.IsWindows)
                AppContext.SetSwitch("System.Drawing.EnableUnixSupport", true); 
#endif
        }

        #endregion
    }
}