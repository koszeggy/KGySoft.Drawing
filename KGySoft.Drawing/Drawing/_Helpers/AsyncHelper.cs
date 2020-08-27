using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using KGySoft.CoreLibraries;

namespace KGySoft.Drawing
{
    internal static class AsyncHelper
    {
        private static IAsyncContext nullContext;

        internal static IAsyncContext Null => nullContext ??= new NullContext();

        [SuppressMessage("Design", "CA1031:Do not catch general exception types",
            Justification = "Exceptions are not suppressed, they will be thrown when calling the EndOperation method.")]
        internal static IAsyncDrawingResult BeginOperation<TResult>(string beginMethod, Func<IAsyncContext, TResult> operation, int maxDegreeOfParallelism, AsyncCallback callback = null, object state = null)
            where TResult : class
        {
            #region Local Methods

            static void DoWork(object state)
            {
                var context = (AsyncDrawingResultContext<TResult>)state;
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

            var result = new AsyncDrawingResultContext<TResult>(beginMethod, operation, maxDegreeOfParallelism, callback, state);
            ThreadPool.QueueUserWorkItem(DoWork, result);
            return result;
        }

        internal static TResult EndOperation<TResult>(IAsyncResult asyncResult, string beginMethodName)
            where TResult : class
        {
            if (asyncResult == null)
                throw new ArgumentNullException(nameof(asyncResult), PublicResources.ArgumentNull);
            if (!(asyncResult is AsyncDrawingResultContext<TResult> result) || result.BeginMethodName != beginMethodName || result.IsDisposed)
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

        [SuppressMessage("Design", "CA1031:Do not catch general exception types",
            Justification = "Exceptions are not suppressed, they will be thrown when task is awaited.")]
        internal static Task<TResult> DoOperationAsync<TResult>(Func<IAsyncContext, TResult> operation, CancellationToken cancellationToken = default, IProgress<DrawingProgress> progress = null, int maxDegreeOfParallelism = 0)
        {
            #region Local Methods

            static void DoWork(object state)
            {
                var (asyncContext, completionSource, operation) = ((IAsyncContext, TaskCompletionSource<TResult>, Func<IAsyncContext, TResult>))state;
                try
                {
                    TResult result = operation.Invoke(asyncContext);
                    if (asyncContext.IsCancellationRequested)
                        completionSource.SetCanceled();
                    else
                        completionSource.SetResult(result);
                }
                catch (Exception e)
                {
                    completionSource.SetException(e);
                }
            }

            #endregion

            IAsyncContext asyncContext = new TaskContext { Token = cancellationToken, ProgressReporter = progress, MaxDegreeOfParallelism = maxDegreeOfParallelism };
            var completionSource = new TaskCompletionSource<TResult>();
            ThreadPool.QueueUserWorkItem(DoWork, (asyncContext, completionSource, operation));
            return completionSource.Task;
        }

        private sealed class NullContext : IAsyncContext
        {
            public int MaxDegreeOfParallelism => 0;
            public bool IsCancellationRequested => false;
            public IProgress<DrawingProgress> ProgressReporter => null;
        }

        private sealed class TaskContext : IAsyncContext
        {
            internal CancellationToken Token { get; set; }

            public int MaxDegreeOfParallelism { get; set; }
            public bool IsCancellationRequested => Token.IsCancellationRequested;
            public IProgress<DrawingProgress> ProgressReporter { get; set; }
        }

        private sealed class AsyncDrawingResultContext<TResult> : IAsyncDrawingResult, IAsyncContext, IProgress<DrawingProgress>
            where TResult : class
        {
            private volatile bool isCancellationRequested;
            private volatile bool isCompleted;
            private DrawingProgress progress;
            private Func<IAsyncContext, TResult> operation;
            private EventHandler<EventArgs<DrawingProgress>> progressChangedHandler;
            private AsyncCallback callback;
            private ManualResetEventSlim waitHandle;
            private volatile TResult result;
            private volatile Exception error;

            internal AsyncDrawingResultContext(string beginMethod, Func<IAsyncContext, TResult> operation, int maxDegreeOfParallelism, AsyncCallback callback = null, object state = null)
            {
                this.operation = operation;
                BeginMethodName = beginMethod;
                MaxDegreeOfParallelism = maxDegreeOfParallelism;
                this.callback = callback;
                AsyncState = state;
            }

            internal string BeginMethodName { get; }
            internal bool IsDisposed { get; private set; }

            public int MaxDegreeOfParallelism { get; }
            public bool IsCancellationRequested => isCancellationRequested;
            public IProgress<DrawingProgress> ProgressReporter => this;

            public bool IsCanceled => isCancellationRequested && isCompleted;

            public void RequestCancel() => isCancellationRequested = true;

            internal TResult Execute(IAsyncContext context) => operation.Invoke(context);

            public event EventHandler<EventArgs<DrawingProgress>> ProgressChanged
            {
                add => progressChangedHandler += value;
                remove => progressChangedHandler -= value;
            }

            public DrawingProgress Progress => progress;

            public bool IsCompleted => isCompleted;

            public WaitHandle AsyncWaitHandle
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

                    return waitHandle.WaitHandle;
                }
            }

            public object AsyncState { get; }

            public bool CompletedSynchronously => false;

            public void Report(DrawingProgress value)
            {
                progress = value;
                progressChangedHandler?.Invoke(this, new EventArgs<DrawingProgress>(value));
            }

            public void Dispose()
            {
                if (IsDisposed)
                    return;
                IsDisposed = true;
                operation = null;
                progressChangedHandler = null;
                callback = null;
                if (waitHandle == null)
                    return;
                if (!waitHandle.IsSet)
                    waitHandle.Set();
                waitHandle.Dispose();
            }

            public TResult Result
            {
                get
                {
                    if (!isCompleted)
                        AsyncWaitHandle.WaitOne();
                    if (isCancellationRequested)
                        throw new OperationCanceledException(Res.OperationCanceled);
                    if (error != null)
                        ExceptionDispatchInfo.Capture(error).Throw();
                    return result;
                }
            }

            public void SetResult(TResult result)
            {
                Debug.Assert(!isCompleted);
                this.result = result;
                SetCompleted();
            }

            public void SetError(Exception e)
            {
                error = e;
                SetCompleted();
            }

            private void SetCompleted()
            {
                Debug.Assert(!isCompleted);
                isCompleted = true;
                callback?.Invoke(this);
                waitHandle?.Set();
            }

            public void SetCanceled()
            {
                Debug.Assert(isCancellationRequested);
                SetCompleted();
            }
        }
    }
}
