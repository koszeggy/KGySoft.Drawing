#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: EnvironmentHelper.cs
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

using System;

#endregion

namespace KGySoft.Drawing.Wpf
{
    internal static class EnvironmentHelper
    {
        #region Properties

#if NET6_0_OR_GREATER
        internal static int MaxArrayLength => Array.MaxLength;
#else
        // Based on the internal Array.MaxByteArrayLength constant
        internal static int MaxArrayLength => 0x7FFFFFC7;
#endif

        #endregion
    }
}
