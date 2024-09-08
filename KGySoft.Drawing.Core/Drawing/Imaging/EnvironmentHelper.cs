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
#if !NET6_0_OR_GREATER
using System.Diagnostics;
using System.Security;
#endif
#if NET35 || NET40
using System.Threading;
#endif

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal static class EnvironmentHelper
    {
        #region Properties

#if NET35 || NET40
        internal static int CurrentThreadId => Thread.CurrentThread.ManagedThreadId;
#else
        internal static int CurrentThreadId => Environment.CurrentManagedThreadId;
#endif

        internal static int CoreCount { get; } = GetCoreCount();

        #endregion

        #region Methods
        
        #region Internal Methods

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

        #region Private Methods

#if NET6_0_OR_GREATER
        private static int GetCoreCount() => Environment.ProcessorCount;
#else
        [SecuritySafeCritical]
        private static int GetCoreCount()
        {
            if (Environment.OSVersion.Platform != PlatformID.Win32NT) // TODO: extract to IsWindows if needed somewhere else, too
                return Environment.ProcessorCount;

            // Here we are on Windows, targeting .NET 5 or earlier, where Environment.ProcessorCount doesn't respect affinity or CPU limit:
            // https://learn.microsoft.com/en-us/dotnet/core/compatibility/core-libraries/6.0/environment-processorcount-on-windows

            try
            {
                // We check if DOTNET_PROCESSOR_COUNT is set because it has a priority over any other settings
                string? var = Environment.GetEnvironmentVariable("DOTNET_PROCESSOR_COUNT");
                if (var is not null && Int32.TryParse(var, out int result))
                    return result;

                // Using CPU affinity
                // NOTE: Unlike the latest Environment.ProcessorCount implementations, not checking if multiple CPU groups are available
                // because it's supported on Windows 11+ only, which was released after .NET 5 anyway.
                nint affinity = Process.GetCurrentProcess().ProcessorAffinity;
                return affinity == 0 ? Environment.ProcessorCount : ((ulong)affinity).GetFlagsCount();
            }
            catch (Exception) // Not letting the type initializer to throw anything. Cannot really happen, maybe only partially trusted domains in .NET Framework.
            {
                return Environment.ProcessorCount;
            }
        }
#endif

        #endregion

        #endregion
    }
}