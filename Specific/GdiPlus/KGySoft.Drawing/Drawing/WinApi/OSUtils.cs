#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: WindowsUtils.cs
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

#endregion

namespace KGySoft.Drawing.WinApi
{
    internal static class OSUtils
    {
        #region Fields

        private static bool? isVistaOrLater;
        private static bool? isWindows;

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

        internal static bool IsXpOrEarlier => IsWindows && !IsVistaOrLater;

        #endregion
    }
}
