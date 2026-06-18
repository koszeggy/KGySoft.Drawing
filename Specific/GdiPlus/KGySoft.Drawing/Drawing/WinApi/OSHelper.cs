#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: OSHelper.cs
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

#endregion

namespace KGySoft.Drawing.WinApi
{
    internal static class OSHelper
    {
        #region Fields

        private static bool? isVistaOrLater;
        private static bool? isWindows;
        private static bool? isMono;
        private static bool? isWine;

        #endregion

        #region Properties

        internal static bool IsVistaOrLater
        {
            get
            {
                if (isVistaOrLater.HasValue)
                    return isVistaOrLater.Value;

                OperatingSystem os = Environment.OSVersion;
                if (os.Platform != PlatformID.Win32NT)
                {
                    isVistaOrLater = false;
                    return false;
                }

                isVistaOrLater = os.Version >= new Version(6, 0, 5243);
                return isVistaOrLater.Value;
            }
        }

        internal static bool IsWindows => isWindows ??= Environment.OSVersion.Platform is PlatformID.Win32NT or PlatformID.Win32Windows;
        internal static bool IsMono => isMono ??= Type.GetType("Mono.Runtime") != null;
        internal static bool IsWine => isWine ??= !String.IsNullOrEmpty(Environment.GetEnvironmentVariable("WINELOADER"));
        internal static bool IsFrameworkMono => IsMono && !IsWine;
        internal static bool IsRealWindows => IsWindows && !IsMono && !IsWine;
        internal static bool IsWineMono => IsMono && IsWine;
        internal static bool IsNonWineWindows => IsWindows && !IsWine;

        #endregion
    }
}
