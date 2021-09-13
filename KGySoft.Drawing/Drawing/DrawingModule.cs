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
using System.Runtime.CompilerServices;

#if !NETFRAMEWORK
using KGySoft.Drawing.WinApi;
#endif

#endregion

namespace KGySoft.Drawing
{
    /// <summary>
    /// Represents the <c>KGySoft.Drawing</c> module.
    /// </summary>
    public static class DrawingModule
    {
        #region Methods

        /// <summary>
        /// Initializes the <c>KGySoft.Drawing</c> module. On non-Windows platforms it makes possible to use <c>System.Drawing</c> types without
        /// editing the <c>runtimeconfig.json</c> file. Additionally, it initializes the resource manager for string resources and registers its central management
        /// in the <a href="http://docs.kgysoft.net/corelibraries/?topic=html/T_KGySoft_LanguageSettings.htm" target="_blank">LanguageSettings</a> class.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <remarks>
        /// <note>The module initializer is executed automatically when any member is accessed in the module for the first time. This method is public to able
        /// to trigger module initialization without performing any other operation. Normally you don't need to call it explicitly but it can be useful if you use
        /// the KGy SOFT Drawing Libraries in an application and you want to configure resource management on starting the application via
        /// the <a href="http://docs.kgysoft.net/corelibraries/?topic=html/T_KGySoft_LanguageSettings.htm" target="_blank">LanguageSettings</a> class.
        /// In such case you can call this method before configuring language settings to make sure that the resources of
        /// the <c>KGySoft.Drawing.dll</c> are also affected by the settings.</note>
        /// <note type="important">If you use KGy SOFT Drawing Libraries from a non-Windows application, then make sure you call this method
        /// before using any <c>System.Drawing</c> type to prevent a <see cref="TypeInitializationException"/> without editing the <c>runtimeconfig.json</c> file.
        /// It is not necessary if you reference the .NET Framework build under Mono.</note>
        /// </remarks>
        [ModuleInitializer]
        public static void Initialize()
        {
            // Just referencing Res in order to trigger its static constructor and initialize the project resources.
            // Thus configuring LanguageSettings in a consumer project will work for resources of KGySoft.CoreLibraries even if Res was not accessed yet.
            Res.EnsureInitialized();

#if !NETFRAMEWORK
            // Making sure that the static constructor of the NativeMethods.Gdip class does not throw an exception
            if (!OSUtils.IsWindows)
                AppContext.SetSwitch("System.Drawing.EnableUnixSupport", true); 
#endif
        }

        #endregion
    }
}