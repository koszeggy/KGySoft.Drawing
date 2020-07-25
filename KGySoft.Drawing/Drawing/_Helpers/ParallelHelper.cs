#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ParallelHelper.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2020 - All Rights Reserved
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
using System.Diagnostics.CodeAnalysis;
#if !NET35
using System.Collections.Concurrent;
using System.Threading.Tasks;
#endif
#if NET35
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Threading;
#endif

#endregion

namespace KGySoft.Drawing
{
#if NET35
#pragma warning disable CS1574 // XML comment has references that cannot not be resolved on all platforms
#endif

    internal static class ParallelHelper
    {
        #region Fields

        private static int? coreCount;

        #endregion

        #region Properties

        private static int CoreCount => coreCount ??= Environment.ProcessorCount;

        #endregion

        #region Methods

        #region Internal Methods

        /// <summary>
        /// Similar to <see cref="Parallel.For(int,int,Action{int})"/> but tries to balance resources and works also in .NET 3.5.
        /// </summary>
        [SuppressMessage("Design", "CA1031:Do not catch general exception types",
            Justification = "Exceptions in pool threads must not be thrown in place but from the caller thread.")]
        internal static void For(int fromInclusive, int toExclusive, Action<int> body)
        {
            int count = toExclusive - fromInclusive;

            // a single iteration: invoke once
            if (count <= 1)
            {
                if (count < 1)
                    return;
                body.Invoke(fromInclusive);
                return;
            }

            // single core: serial invoke
            if (CoreCount == 1)
            {
                for (int i = fromInclusive; i < toExclusive; i++)
                    body.Invoke(i);

                return;
            }

#if NET35
            int busyCount = 0;
            Exception error = null;
            int rangeSize = count / CoreCount;

            // we have enough cores for each iteration
            if (rangeSize <= 1)
            {
                for (int i = fromInclusive; i < toExclusive; i++)
                {
                    // not queuing more tasks than the number of cores
                    while (busyCount >= CoreCount && error == null)
                        Thread.Sleep(0);

                    if (error != null)
                        break;

                    Interlocked.Increment(ref busyCount);

                    int value = i;
                    ThreadPool.QueueUserWorkItem(_ =>
                    {
                        try
                        {
                            body.Invoke(value);
                        }
                        catch (Exception e)
                        {
                            Interlocked.CompareExchange(ref error, e, null);
                        }
                        finally
                        {
                            Interlocked.Decrement(ref busyCount);
                        }
                    });
                }
            }
            // we merge some iterations to be processed by the same core
            else
            {
                var ranges = CreateRanges(fromInclusive, toExclusive, rangeSize);
                foreach (var range in ranges)
                {
                    // not queuing more tasks than the number of cores
                    while (busyCount >= CoreCount && error == null)
                        Thread.Sleep(0);

                    if (error != null)
                        break;
                    Interlocked.Increment(ref busyCount);

                    ThreadPool.QueueUserWorkItem(_ =>
                    {
                        try
                        {
                            for (int i = range.From; i < range.To; i++)
                                body.Invoke(i);
                        }
                        catch (Exception e)
                        {
                            Interlocked.CompareExchange(ref error, e, null);
                        }
                        finally
                        {
                            Interlocked.Decrement(ref busyCount);
                        }
                    });
                }
            }

            // waiting until every task is finished
            while (busyCount > 0 && error == null)
                Thread.Sleep(0);

            if (error != null)
                ExceptionDispatchInfo.Capture(error).Throw();
#else
            // we allow a bit more fine resolution than actual core counts
            int rangeSize = (count / CoreCount) >> 2;

            // we have enough cores for each iteration
            if (rangeSize <= 1)
            {
                Parallel.For(fromInclusive, toExclusive, body);
                return;
            }

            // we merge some iterations to be processed by the same core
            var partitions = Partitioner.Create(fromInclusive, toExclusive, rangeSize);
            Parallel.ForEach(partitions, (range, state) =>
            {
                (int from, int to) = range;
                for (int i = from; i < to; i++)
                    body.Invoke(i);
            });
#endif
        }

        #endregion

        #region Private Methods
#if NET35

        private static IEnumerable<(int From, int To)> CreateRanges(int fromInclusive, int toExclusive, int rangeSize)
        {
            for (int i = fromInclusive; i < toExclusive; i += rangeSize)
                yield return (i, Math.Min(toExclusive, i + rangeSize));
        }

#endif
        #endregion

        #endregion
    }
}
