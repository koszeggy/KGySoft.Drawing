#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: Kernel32.cs
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

#if NET
using System.Runtime.Versioning;
#endif

#region Used Namespaces

using System;
using System.Runtime.InteropServices;

#endregion

#endregion

namespace KGySoft.Drawing.SkiaSharp.WinApi
{
#if NET
    [SupportedOSPlatform("windows")]
#endif
    internal static class Kernel32
    {
        #region Constants

        private const string kernel32 = "kernel32.dll";

        #endregion

        #region Properties

        internal static IntPtr CurrentModuleHandle { get; }

        #endregion

        #region Constructors

        static Kernel32()
        {
            CurrentModuleHandle = GetModuleHandle(null);
            if (CurrentModuleHandle == IntPtr.Zero)
            {
                throw new InvalidOperationException("Could not get module handle.");
            }
        }

        #endregion

        #region Methods

        [DllImport(kernel32, CallingConvention = CallingConvention.Winapi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        private static extern IntPtr GetModuleHandle([MarshalAs(UnmanagedType.LPTStr)]string? lpModuleName);

        #endregion
    }
}