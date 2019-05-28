﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: WindowsUtils.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2019 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution. If not, then this file is considered as
//  an illegal copy.
//
//  Unauthorized copying of this file, via any medium is strictly prohibited.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;

#endregion

namespace KGySoft.Drawing.WinApi
{
    internal static class WindowsUtils
    {
        #region Fields

        private static bool? isVistaOrLater;

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

        #endregion
    }
}
