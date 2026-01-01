#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: Constants.cs
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

using System.Diagnostics.CodeAnalysis;

#endregion

namespace KGySoft.Drawing.WinApi
{
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "WinAPI")]
    internal static class Constants
    {
        #region Constants

        internal const int MAX_PATH = 260;

        internal const uint LOAD_LIBRARY_AS_DATAFILE = 0x00000002;

        internal const int ERROR_RESOURCE_ENUM_USER_STOP = 0x3B02;

        internal const nint RT_ICON = 3;
        internal const nint RT_GROUP_ICON = 14;

        #endregion
    }
}