#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: EnvironmentHelper.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2024 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;
#if NET35 || NET40
using System.Threading;
#endif

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal static class EnvironmentHelper
    {
        #region Fields

        private static int? coreCount;

        #endregion

        #region Properties

#if NET35 || NET40
        internal static int CurrentThreadId => Thread.CurrentThread.ManagedThreadId;
#else
        internal static int CurrentThreadId => Environment.CurrentManagedThreadId;
#endif

        internal static int CoreCount => coreCount ??= Environment.ProcessorCount;

        #endregion
    }
}