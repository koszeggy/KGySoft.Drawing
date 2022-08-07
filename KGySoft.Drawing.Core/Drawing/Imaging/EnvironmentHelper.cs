﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: EnvironmentHelper.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2022 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System.Threading;

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal static class EnvironmentHelper
    {
        #region Methods

#if NET35 || NET40
        internal static int CurrentThreadId => Thread.CurrentThread.ManagedThreadId;
#else
        internal static int CurrentThreadId => Environment.CurrentManagedThreadId;
#endif

        #endregion
    }
}