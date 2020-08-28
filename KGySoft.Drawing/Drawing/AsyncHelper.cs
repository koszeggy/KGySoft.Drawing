#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: AsyncHelper.cs
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
using System.Runtime.ExceptionServices;
using System.Threading;
#if !NET35
using System.Threading.Tasks; 
#endif

#endregion

namespace KGySoft.Drawing
{
    internal static class AsyncHelper
    {
        #region Nested classes

        #region NullContext class

        private sealed class NullContext : IAsyncContext
        {
            #region Properties

            public int MaxDegreeOfParallelism => 0;
            public bool IsCancellationRequested => false;
            public IDrawingProgress Progress => null;

            #endregion
        }

        #endregion

        #region TaskContext class
#if !NET35

        private sealed class TaskContext : IAsyncContext
        {
            #region Fields

            private CancellationToken token;

            #endregion

            #region Properties

            #region Public Properties

            public int MaxDegreeOfParallelism { get; }
            public bool IsCancellationRequested => token.IsCancellationRequested;
            public IDrawingProgress Progress { get; }

            #endregion

            #region Internal Properties

            internal bool ReturnDefaultIfCanceled { get; }
            internal object State { get; }

            #endregion

            #endregion

            #region Constructors

            internal TaskContext(TaskConfig asyncConfig)
            {
                if (asyncConfig == null)
                    return;
                token = asyncConfig.CancellationToken;
                MaxDegreeOfParallelism = asyncConfig.MaxDegreeOfParallelism;
                Progress = asyncConfig.Progress;
                ReturnDefaultIfCanceled = asyncConfig.ReturnDefaultIfCanceled;
                State = asyncConfig.State;
            }

            #endregion
        } 

#endif
        #endregion

        #region AsyncResultContext class

        private sealed class AsyncResultContext<TResult> : IAsyncResult, IAsyncContext, IDisposable
            where TResult : class
        {
            #region Fields

            private readonly bool returnDefaultIfCanceled;

            private volatile bool isCancellationRequested;
            private volatile bool isCompleted;
            private Func<IAsyncContext, TResult> operation;
            private Func<bool> isCancelRequestedCallback;
            private AsyncCallback callback;
            private ManualResetEventSlim waitHandle;
            private volatile TResult result;
            private volatile Exception error;

            #endregion

            #region Properties

            #region Public Properties

            public int MaxDegreeOfParallelism { get; }
            public bool IsCancellationRequested => isCancelRequestedCallback != null && (isCancellationRequested || (isCancellationRequested = isCancelRequestedCallback.Invoke()));
            public IDrawingProgress Progress { get; }
            public bool IsCompleted => isCompleted;

            public WaitHandle AsyncWaitHandle => InternalWaitHandle.WaitHandle;

            public object AsyncState { get; }
            public bool CompletedSynchronously { get; internal set; }

            #endregion

            #region Internal Properties

            internal string BeginMethodName { get; }
            internal bool IsDisposed { get; private set; }

            internal TResult Result
            {
                get
                {
                    if (!isCompleted)
                        InternalWaitHandle.Wait();
                    if (isCancellationRequested && !returnDefaultIfCanceled)
                        throw new OperationCanceledException(Res.OperationCanceled);
                    if (error != null)
                        ExceptionDispatchInfo.Capture(error).Throw();
                    return result;
                }
            }

            #endregion

            #region Private Properties

            private ManualResetEventSlim InternalWaitHandle
            {
                get
                {
                    if (IsDisposed)
                        throw new ObjectDisposedException(PublicResources.ObjectDisposed);
                    if (waitHandle == null)
                    {
                        var newHandle = new ManualResetEventSlim();
                        if (Interlocked.CompareExchange(ref waitHandle, newHandle, null) != null)
                            newHandle.Dispose();
                        else if (isCompleted)
                            waitHandle.Set();
                    }

                    return waitHandle;
                }
            }

            #endregion

            #endregion

            #region Constructors

            internal AsyncResultContext(string beginMethod, Func<IAsyncContext, TResult> operation, AsyncConfig asyncConfig)
            {
                this.operation = operation;
                BeginMethodName = beginMethod;
                if (asyncConfig == null)
                    return;
                MaxDegreeOfParallelism = asyncConfig.MaxDegreeOfParallelism;
                callback = asyncConfig.CompletedCallback;
                AsyncState = asyncConfig.State;
                returnDefaultIfCanceled = asyncConfig.ReturnDefaultIfCanceled;
                isCancelRequestedCallback = asyncConfig.IsCancelRequestedCallback;
                Progress = asyncConfig.Progress;
            }

            #endregion

            #region Methods

            #region Public Methods

            public void Dispose()
            {
                if (IsDisposed)
                    return;
                IsDisposed = true;
                operation = null;
                isCancelRequestedCallback = null;
                callback = null;
                if (waitHandle == null)
                    return;
                if (!waitHandle.IsSet)
                    waitHandle.Set();
                waitHandle.Dispose();
            }

            #endregion

            #region Internal Methods

            internal TResult Execute(IAsyncContext context) => operation.Invoke(context);

            internal void SetResult(TResult value)
            {
                Debug.Assert(!isCompleted);
                this.result = value;
                SetCompleted();
            }

            internal void SetError(Exception e)
            {
                error = e;
                SetCompleted();
            }

            internal void SetCanceled()
            {
                Debug.Assert(isCancellationRequested);
                SetCompleted();
            }

            #endregion

            #region Private Methods

            private void SetCompleted()
            {
                Debug.Assert(!isCompleted);
                isCompleted = true;
                callback?.Invoke(this);
                waitHandle?.Set();
            }

            #endregion

            #endregion
        }

        #endregion

        #region ManualResetEventSlim class
#if NET35

        private sealed class ManualResetEventSlim : IDisposable
        {
            #region Fields

            private readonly object lockObject = new object();

            private bool isDisposed;
            private ManualResetEvent nativeHandle;

            #endregion

            #region Properties

            internal bool IsSet { get; private set; }

            internal WaitHandle WaitHandle
            {
                get
                {
                    if (isDisposed)
                        throw new ObjectDisposedException(PublicResources.ObjectDisposed);
                    if (nativeHandle != null)
                        return nativeHandle;

                    bool originalState = IsSet;
                    var newHandle = new ManualResetEvent(originalState);
                    if (Interlocked.CompareExchange(ref nativeHandle, newHandle, null) != null)
                        newHandle.Close();
                    else
                    {
                        if (IsSet == originalState)
                            return nativeHandle;

                        Debug.Assert(IsSet, "Only set state is expected here");
                        lock (newHandle)
                        {
                            if (nativeHandle == newHandle)
                                newHandle.Set();
                        }
                    }

                    return nativeHandle;
                }
            }

            #endregion

            #region Methods

            #region Public Methods

            public void Dispose()
            {
                lock (lockObject)
                {
                    if (isDisposed)
                        return;
                    isDisposed = true;
                    DoSignal();
                    Monitor.PulseAll(lockObject);
                    nativeHandle?.Close();
                    nativeHandle = null;
                }
            }

            #endregion

            #region Internal Methods

            internal void Set()
            {
                lock (lockObject)
                {
                    if (isDisposed)
                        return;
                    DoSignal();
                    Monitor.PulseAll(lockObject);
                }
            }

            internal void Wait()
            {
                lock (lockObject)
                {
                    if (isDisposed)
                        return;
                    while (!IsSet)
                        Monitor.Wait(lockObject);
                }
            }

            #endregion

            #region Private Methods

            private void DoSignal()
            {
                // this should be called in lock
                IsSet = true;
                nativeHandle?.Set();
            }

            #endregion

            #endregion
        }

#endif
        #endregion

        #endregion

        #region Fields

        private static IAsyncContext nullContext;

        #endregion

        #region Properties

        internal static IAsyncContext Null => nullContext ??= new NullContext();

        #endregion

        #region Methods

        [SuppressMessage("Design", "CA1031:Do not catch general exception types",
                Justification = "Pool thread exceptions are not suppressed, they will be thrown when calling the EndOperation method.")]
        internal static IAsyncResult BeginOperation<TResult>(string beginMethod, Func<IAsyncContext, TResult> operation, AsyncConfig asyncConfig)
            where TResult : class
        {
            #region Local Methods

            // this method is executed on a pool thread
            static void DoWork(object state)
            {
                var context = (AsyncResultContext<TResult>)state;
                if (context.IsCancellationRequested)
                {
                    context.SetCanceled();
                    return;
                }

                try
                {
                    TResult result = context.Execute(context);
                    if (context.IsCancellationRequested)
                        context.SetCanceled();
                    else
                        context.SetResult(result);
                }
                catch (Exception e)
                {
                    context.SetError(e);
                }
            }

            #endregion

            var asyncResult = new AsyncResultContext<TResult>(beginMethod, operation, asyncConfig);
            if (asyncResult.IsCancellationRequested)
            {
                asyncResult.SetCanceled();
                asyncResult.CompletedSynchronously = true;
            }
            else
                ThreadPool.QueueUserWorkItem(DoWork, asyncResult);
            return asyncResult;
        }

        internal static TResult EndOperation<TResult>(IAsyncResult asyncResult, string beginMethodName)
            where TResult : class
        {
            if (asyncResult == null)
                throw new ArgumentNullException(nameof(asyncResult), PublicResources.ArgumentNull);
            if (!(asyncResult is AsyncResultContext<TResult> result) || result.BeginMethodName != beginMethodName || result.IsDisposed)
                throw new InvalidOperationException(Res.InvalidAsyncResult);
            try
            {
                return result.Result;
            }
            finally
            {
                result.Dispose();
            }
        }

#if !NET35
        [SuppressMessage("Design", "CA1031:Do not catch general exception types",
        Justification = "Pool thread exceptions are not suppressed, they will be thrown when task is awaited or Result is accessed.")]
        internal static Task<TResult> DoOperationAsync<TResult>(Func<IAsyncContext, TResult> operation, TaskConfig asyncConfig)
        {
            #region Local Methods

            // this method is executed on a pool thread
            static void DoWork(object state)
            {
                var (context, completion, func) = ((TaskContext, TaskCompletionSource<TResult>, Func<IAsyncContext, TResult>))state;
                try
                {
                    TResult result = func.Invoke(context);
                    if (context.IsCancellationRequested)
                    {
                        if (context.ReturnDefaultIfCanceled)
                            completion.SetResult(default);
                        else
                            completion.SetCanceled();
                    }
                    else
                        completion.SetResult(result);
                }
                catch (Exception e)
                {
                    completion.SetException(e);
                }
            }

            #endregion

            TaskContext taskContext = new TaskContext(asyncConfig);
            var completionSource = new TaskCompletionSource<TResult>(taskContext.State);
            if (taskContext.IsCancellationRequested)
            {
                if (taskContext.ReturnDefaultIfCanceled)
                    completionSource.SetResult(default);
                else
                    completionSource.SetCanceled();
            }
            else
                ThreadPool.QueueUserWorkItem(DoWork, (taskContext, completionSource, operation));
            return completionSource.Task;
        } 
#endif

        #endregion
    }
}
