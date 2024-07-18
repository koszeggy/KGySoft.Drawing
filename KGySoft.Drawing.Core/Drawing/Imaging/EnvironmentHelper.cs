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

        // Needed because on some platforms this is re-evaluated on each call. TODO: Under .NET 6 this ignores CPU affinity of the process
        internal static int CoreCount { get; } = Environment.ProcessorCount;

        #endregion

        #region Methods

        internal static int GetThreadBasedCacheSize() => GetThreadBasedCacheSize(CoreCount);

        internal static int GetThreadBasedCacheSize(int maxThreads)
        {
            int size = maxThreads <= 0 ? CoreCount : Math.Min(maxThreads, CoreCount);

            // Up to 32 threads: using at least twice as many entries, rounded up to a power ot two to reduce the chance of hash collisions.
            // > 32 threads: using next power of two entries of the provided size * 1.125.
            // For example, using 32 entries for 9..16 threads, 64 entries for 17..57 threads and 128 entries for 58..114 threads.
            size = size <= 32 ? size << 1 : (int)(size * 1.125f);
            return size <= 8 ? 8 : ((uint)size).RoundUpToPowerOf2();
        }

        #endregion
    }
}