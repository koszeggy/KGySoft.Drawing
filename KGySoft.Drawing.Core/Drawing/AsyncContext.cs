#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: AsyncContext.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2021 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading;
#if !NET35
using System.Threading.Tasks;
#endif

#endregion

namespace KGySoft.Drawing
{
    /// <summary>
    /// A helper class to implement CPU-bound drawing operations returning <see cref="IAsyncResult"/> (.NET Framework 4.0 and above: or <see cref="Task"/>)
    /// that can be configured by an <see cref="AsyncConfig"/> (.NET Framework 4.0 and above: or <see cref="TaskConfig"/>) parameter.
    /// <br/>See the <strong>Examples</strong> section for an example.
    /// </summary>
    /// <example>
    /// The following example demonstrates how to use the <see cref="AsyncContext"/> class to create sync/async versions of a method
    /// sharing the common implementation in a single method.
    /// <note>This example requires to reference the <a href="https://www.nuget.org/packages/KGySoft.Drawing/" target="_blank">KGySoft.Drawing</a> package. When targeting .NET 7 or later it can be executed on Windows only.</note>
    /// <code lang="C#"><![CDATA[
    /// #nullable enable
    /// 
    /// using System;
    /// using System.Drawing;
    /// using System.Threading.Tasks;
    /// 
    /// using KGySoft.Drawing;
    /// using KGySoft.Drawing.Imaging;
    /// 
    /// public static class Example
    /// {
    ///     // The sync version. This method is blocking, cannot be canceled, does not report progress and uses max parallelization.
    ///     public static Bitmap ToGrayscale(Bitmap bitmap)
    ///     {
    ///         ValidateArguments(bitmap);
    /// 
    ///         // Just to demonstrate some immediate return (for the sync version it really straightforward).
    ///         if (IsGrayscale(bitmap))
    ///             return bitmap;
    /// 
    ///         // The actual processing. From the sync version it gets a Null context. The result is never null from here.
    ///         return ProcessToGrayscale(AsyncContext.Null, bitmap)!;
    ///     }
    /// 
    ///     // The Task-returning version. Requires .NET Framework 4.0 or later and can be awaited in .NET Framework 4.5 or later.
    ///     public static Task<Bitmap?> ToGrayscaleAsync(Bitmap bitmap, TaskConfig? asyncConfig = null)
    ///     {
    ///         ValidateArguments(bitmap);
    /// 
    ///         // Use AsyncContext.FromResult for immediate return. It handles asyncConfig.ThrowIfCanceled properly.
    ///         if (IsGrayscale(bitmap))
    ///             return AsyncContext.FromResult(bitmap, asyncConfig);
    /// 
    ///         // The actual processing for Task returning async methods.
    ///         return AsyncContext.DoOperationAsync(ctx => ProcessToGrayscale(ctx, bitmap), asyncConfig);
    ///     }
    /// 
    ///     // The old-style Begin/End methods that work even in .NET Framework 3.5. Can be omitted if not needed.
    ///     public static IAsyncResult BeginToGrayscale(Bitmap bitmap, AsyncConfig? asyncConfig = null)
    ///     {
    ///         ValidateArguments(bitmap);
    /// 
    ///         // Use AsyncContext.FromResult for immediate return.
    ///         // It handles asyncConfig.ThrowIfCanceled and sets IAsyncResult.CompletedSynchronously.
    ///         if (IsGrayscale(bitmap))
    ///             return AsyncContext.FromResult(bitmap, asyncConfig);
    /// 
    ///         // The actual processing for IAsyncResult returning async methods.
    ///         return AsyncContext.BeginOperation(ctx => ProcessToGrayscale(ctx, bitmap), asyncConfig);
    ///     }
    /// 
    ///     // Note that the name of "BeginToGrayscale" is explicitly specified here. Older compilers need it also for AsyncContext.BeginOperation.
    ///     public static Bitmap? EndToGrayscale(IAsyncResult asyncResult) => AsyncContext.EndOperation<Bitmap?>(asyncResult, nameof(BeginToGrayscale));
    /// 
    ///     // The method of the actual processing has the same parameters as the sync version and also an IAsyncContext parameter.
    ///     // The result can be null if the operation is canceled (throwing possible exception due to cancellation is handled by the caller)
    ///     private static Bitmap? ProcessToGrayscale(IAsyncContext context, Bitmap bitmap)
    ///     {
    ///         Bitmap result = new Bitmap(bitmap.Width, bitmap.Height);
    /// 
    ///         using (IReadableBitmapData source = bitmap.GetReadableBitmapData())
    ///         using (IWritableBitmapData target = result.GetWritableBitmapData())
    ///         {
    ///             // NOTE: The BitmapDataExtensions class has many methods with IAsyncContext parameters that can be used from
    ///             // potentially async operations like this one. A single line solution could be:
    ///             //source.CopyTo(target, context, new Rectangle(0, 0, source.Width, source.Height), Point.Empty,
    ///             //    PredefinedColorsQuantizer.FromCustomFunction(c => c.ToGray()));
    /// 
    ///             // We can report progress if the caller configured it. Now we will increment progress for each processed rows.
    ///             context.Progress?.New(DrawingOperation.ProcessingPixels, maximumValue: source.Height);
    /// 
    ///             Parallel.For(0, source.Height,
    ///                 new ParallelOptions { MaxDegreeOfParallelism = context.MaxDegreeOfParallelism <= 0 ? -1 : context.MaxDegreeOfParallelism },
    ///                 (y, state) =>
    ///                 {
    ///                     if (context.IsCancellationRequested)
    ///                     {
    ///                         state.Stop();
    ///                         return;
    ///                     }
    /// 
    ///                     IReadableBitmapDataRow rowSrc = source[y];
    ///                     IWritableBitmapDataRow rowDst = target[y];
    ///                     for (int x = 0; x < rowSrc.Width; x++)
    ///                         rowDst[x] = rowSrc[x].ToGray();
    /// 
    ///                     context.Progress?.Increment();
    ///                 });
    ///         }
    /// 
    ///         // Do not throw OperationCanceledException explicitly: it will be thrown by the caller
    ///         // if the asyncConfig parameter passed to the async overloads was configured to throw an exception.
    ///         if (context.IsCancellationRequested)
    ///         {
    ///             result.Dispose();
    ///             return null;
    ///         }
    /// 
    ///         return result;
    ///     }
    /// 
    ///     private static void ValidateArguments(Bitmap bitmap)
    ///     {
    ///         if (bitmap == null)
    ///             throw new ArgumentNullException(nameof(bitmap));
    ///     }
    /// 
    ///     private static bool IsGrayscale(Bitmap bitmap) => false;
    /// }]]></code>
    /// </example>
    public static class AsyncContext
    {
        #region Nested classes

        #region NullContext class

        private sealed class NullContext : IAsyncContext
        {
            #region Properties

            public int MaxDegreeOfParallelism => 0;
            public bool IsCancellationRequested => false;
            public bool CanBeCanceled => false;
            public IDrawingProgress? Progress => null;

            #endregion

            #region Methods

            public void ThrowIfCancellationRequested() { }

            #endregion
        }

        #endregion

        #region TaskContext class
#if !NET35

        private sealed class TaskContext : IAsyncContext
        {
            #region Fields

#if !(NETFRAMEWORK || NETSTANDARD2_0 || NETCOREAPP2_0)
            [SuppressMessage("Style", "IDE0044:Add readonly modifier",
                Justification = "CancellationToken is not a readonly struct in every targeted platform so not making it readonly to prevent the creation of a defensive copy.")] 
            // ReSharper disable once FieldCanBeMadeReadOnly.Local
#endif
            private CancellationToken token;

            #endregion

            #region Properties

            #region Public Properties

            public int MaxDegreeOfParallelism { get; }
            public bool IsCancellationRequested => token.IsCancellationRequested;
            public bool CanBeCanceled => token.CanBeCanceled;
            public IDrawingProgress? Progress { get; }

            #endregion

            #region Internal Properties

            internal bool ThrowIfCanceled { get; } = true;
            internal object? State { get; }

            #endregion

            #endregion

            #region Constructors

            internal TaskContext(TaskConfig? asyncConfig)
            {
                if (asyncConfig == null)
                    return;
                token = asyncConfig.CancellationToken;
                MaxDegreeOfParallelism = asyncConfig.MaxDegreeOfParallelism;
                Progress = asyncConfig.Progress;
                ThrowIfCanceled = asyncConfig.ThrowIfCanceled;
                State = asyncConfig.State;
            }

            #endregion

            #region Methods

            public void ThrowIfCancellationRequested() => token.ThrowIfCancellationRequested();

            #endregion
        }

#endif
        #endregion

        #region AsyncResultContext class

        private class AsyncResultContext : IAsyncResult, IAsyncContext, IDisposable
        {
            #region Fields

            private volatile bool isCancellationRequested;
            private volatile bool isCompleted;
            private Func<bool>? isCancelRequestedCallback;
            private AsyncCallback? callback;
            private ManualResetEventSlim? waitHandle;
            private volatile Exception? error;

            #endregion

            #region Properties

            #region Public Properties

            public int MaxDegreeOfParallelism { get; }
            public bool IsCancellationRequested => isCancelRequestedCallback != null && (isCancellationRequested || (isCancellationRequested = isCancelRequestedCallback.Invoke()));
            public bool CanBeCanceled => isCancelRequestedCallback != null;
            public IDrawingProgress? Progress { get; }
            public bool IsCompleted => isCompleted;

            public WaitHandle AsyncWaitHandle => InternalWaitHandle.WaitHandle;

            public object? AsyncState { get; }
            public bool CompletedSynchronously { get; internal set; }

            #endregion

            #region Internal Properties

            internal bool ThrowIfCanceled { get; } = true;
            internal string BeginMethodName { get; }
            internal bool IsDisposed { get; private set; }
            internal Action<IAsyncContext>? Operation { get; private set; }

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

            internal AsyncResultContext(string beginMethod, Action<IAsyncContext>? operation, AsyncConfig? asyncConfig)
            {
                Operation = operation;
                BeginMethodName = beginMethod;
                if (asyncConfig == null)
                    return;
                MaxDegreeOfParallelism = asyncConfig.MaxDegreeOfParallelism;
                callback = asyncConfig.CompletedCallback;
                AsyncState = asyncConfig.State;
                ThrowIfCanceled = asyncConfig.ThrowIfCanceled;
                isCancelRequestedCallback = asyncConfig.IsCancelRequestedCallback;
                Progress = asyncConfig.Progress;
            }

            #endregion

            #region Methods

            #region Static Methods

            private static void ThrowOperationCanceled() => throw new OperationCanceledException(Res.OperationCanceled);

            #endregion

            #region Instance Methods

            #region Public Methods

            public void ThrowIfCancellationRequested()
            {
                if (IsCancellationRequested)
                    ThrowOperationCanceled();
            }

            public void Dispose()
            {
                GC.SuppressFinalize(this);
                Dispose(true);
            }

            #endregion

            #region Internal Methods

            internal void SetCompleted()
            {
                Debug.Assert(!isCompleted);
                isCompleted = true;
                callback?.Invoke(this);
                waitHandle?.Set();
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

            internal void WaitForCompletion()
            {
                if (!isCompleted)
                    InternalWaitHandle.Wait();
                if (isCancellationRequested && ThrowIfCanceled)
                    ThrowOperationCanceled();
                if (error != null)
                    ExceptionDispatchInfo.Capture(error).Throw();
            }

            #endregion

            #region Protected Methods

            protected virtual void Dispose(bool disposing)
            {
                if (IsDisposed)
                    return;
                IsDisposed = true;
                Operation = null;
                isCancelRequestedCallback = null;
                callback = null;
                if (waitHandle == null)
                    return;

                if (!waitHandle.IsSet)
                    waitHandle.Set();

                if (disposing)
                    waitHandle.Dispose();
            }

            #endregion

            #endregion

            #endregion
        }

        #endregion

        #region AsyncResultContext<TResult> class

        private sealed class AsyncResultContext<TResult> : AsyncResultContext
        {
            #region Fields

            private TResult result;

            #endregion

            #region Properties

            internal new Func<IAsyncContext, TResult>? Operation { get; private set; }

            internal TResult Result
            {
                get
                {
                    // Though result is not volatile, WaitForCompletion has a volatile read so always a correct value is returned
                    WaitForCompletion();
                    return result;
                }
            }

            #endregion

            #region Constructors

            internal AsyncResultContext(string beginMethod, Func<IAsyncContext, TResult>? operation, TResult canceledResult, AsyncConfig? asyncConfig)
                : base(beginMethod, null, asyncConfig)
            {
                result = canceledResult;
                Operation = operation;
            }

            #endregion

            #region Methods

            #region Internal Methods

            internal void SetResult(TResult value)
            {
                result = value;
                SetCompleted();
            }

            #endregion

            #region Protected Methods

            protected override void Dispose(bool disposing)
            {
                if (IsDisposed)
                    return;
                Operation = null;
                base.Dispose(disposing);
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
            private ManualResetEvent? nativeHandle;

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
                // this must be called in lock
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

        private static IAsyncContext? nullContext;

        #endregion

        #region Properties

        /// <summary>
        /// Gets a default context for non-async operations.
        /// <br/>See the <strong>Examples</strong> section of the <see cref="AsyncContext"/> class for details.
        /// </summary>
        public static IAsyncContext Null => nullContext ??= new NullContext();

        #endregion

        #region Methods

        /// <summary>
        /// Exposes the specified <paramref name="operation"/> with no return value as an <see cref="IAsyncResult"/>-returning async operation.
        /// The operation can be completed by calling the <see cref="EndOperation">EndOperation</see> method.
        /// <br/>See the <strong>Examples</strong> section of the <see cref="AsyncContext"/> class for details.
        /// </summary>
        /// <param name="operation">The operation to be executed.</param>
        /// <param name="asyncConfig">The configuration for the asynchronous operation.</param>
        /// <param name="beginMethod">The name of the method that represents the <paramref name="operation"/>.
        /// This must be passed also to the <see cref="EndOperation">EndOperation</see> method. This parameter is optional.
        /// <br/>Default value: The name of the caller method when used with a compiler that recognizes <see cref="CallerMemberNameAttribute"/>; otherwise, <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> instance representing the asynchronous operation.
        /// To complete the operation it must be passed to the <see cref="EndOperation">EndOperation</see> method.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="operation"/> or <paramref name="beginMethod"/> is <see langword="null"/>.</exception>
        [SuppressMessage("Design", "CA1031:Do not catch general exception types",
            Justification = "Pool thread exceptions are not suppressed, they will be thrown when calling the EndOperation method.")]
        public static IAsyncResult BeginOperation(Action<IAsyncContext> operation, AsyncConfig? asyncConfig, [CallerMemberName]string beginMethod = null!)
        {
            #region Local Methods

            // this method is executed on a pool thread
            static void DoWork(object state)
            {
                var context = (AsyncResultContext)state;
                if (context.IsCancellationRequested)
                {
                    context.SetCanceled();
                    return;
                }

                try
                {
                    context.Operation!.Invoke(context);
                    if (context.IsCancellationRequested)
                        context.SetCanceled();
                    else
                        context.SetCompleted();
                }
                catch (OperationCanceledException)
                {
                    context.SetCanceled();
                }
                catch (Exception e)
                {
                    context.SetError(e);
                }
            }

            #endregion

            var asyncResult = new AsyncResultContext(beginMethod ?? throw new ArgumentNullException(nameof(beginMethod), PublicResources.ArgumentNull),
                operation ?? throw new ArgumentNullException(nameof(operation), PublicResources.ArgumentNull), asyncConfig);
            if (asyncResult.IsCancellationRequested)
            {
                asyncResult.SetCanceled();
                asyncResult.CompletedSynchronously = true;
            }
            else
                ThreadPool.QueueUserWorkItem(DoWork!, asyncResult);
            return asyncResult;
        }

        /// <summary>
        /// Exposes the specified <paramref name="operation"/> with a return value as an <see cref="IAsyncResult"/>-returning async operation.
        /// To obtain the result the <see cref="EndOperation{TResult}">EndOperation</see> method must be called.
        /// <br/>See the <strong>Examples</strong> section of the <see cref="AsyncContext"/> class for details.
        /// </summary>
        /// <typeparam name="TResult">The type of the result of the specified <paramref name="operation"/>.</typeparam>
        /// <param name="operation">The operation to be executed.</param>
        /// <param name="asyncConfig">The configuration for the asynchronous operation.</param>
        /// <param name="beginMethod">The name of the method that represents the <paramref name="operation"/>.
        /// This must be passed also to the <see cref="EndOperation{TResult}">EndOperation</see> method. This parameter is optional.
        /// <br/>Default value: The name of the caller method when used with a compiler that recognizes <see cref="CallerMemberNameAttribute"/>; otherwise, <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> instance representing the asynchronous operation.
        /// To complete the operation it must be passed to the <see cref="EndOperation{TResult}">EndOperation</see> method.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="operation"/> or <paramref name="beginMethod"/> is <see langword="null"/>.</exception>
        public static IAsyncResult BeginOperation<TResult>(Func<IAsyncContext, TResult> operation, AsyncConfig? asyncConfig, [CallerMemberName]string beginMethod = null!)
            => BeginOperation(operation, default, asyncConfig, beginMethod);

        /// <summary>
        /// Exposes the specified <paramref name="operation"/> with a return value as an <see cref="IAsyncResult"/>-returning async operation.
        /// To obtain the result the <see cref="EndOperation{TResult}">EndOperation</see> method must be called.
        /// <br/>See the <strong>Examples</strong> section of the <see cref="AsyncContext"/> class for details.
        /// </summary>
        /// <typeparam name="TResult">The type of the result of the specified <paramref name="operation"/>.</typeparam>
        /// <param name="operation">The operation to be executed.</param>
        /// <param name="canceledResult">The result to be returned by <see cref="EndOperation{TResult}">EndOperation</see> if the operation is canceled
        /// and <see cref="AsyncConfigBase.ThrowIfCanceled"/> of <paramref name="asyncConfig"/> returns <see langword="false"/>.</param>
        /// <param name="asyncConfig">The configuration for the asynchronous operation.</param>
        /// <param name="beginMethod">The name of the method that represents the <paramref name="operation"/>.
        /// This must be passed also to the <see cref="EndOperation{TResult}">EndOperation</see> method. This parameter is optional.
        /// <br/>Default value: The name of the caller method when used with a compiler that recognizes <see cref="CallerMemberNameAttribute"/>; otherwise, <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> instance representing the asynchronous operation.
        /// To complete the operation it must be passed to the <see cref="EndOperation{TResult}">EndOperation</see> method.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="operation"/> or <paramref name="beginMethod"/> is <see langword="null"/>.</exception>
        [SuppressMessage("Design", "CA1031:Do not catch general exception types",
                Justification = "Pool thread exceptions are not suppressed, they will be thrown when calling the EndOperation method.")]
        public static IAsyncResult BeginOperation<TResult>(Func<IAsyncContext, TResult> operation, TResult canceledResult, AsyncConfig? asyncConfig, [CallerMemberName]string beginMethod = null!)
        {
            #region Local Methods

            // this method is executed on a pool thread
            static void DoWork(object state)
            {
                var context = (AsyncResultContext<TResult>)state;
                if (context.IsCancellationRequested)
                {
                    if (context.ThrowIfCanceled)
                        context.SetCanceled();
                    return;
                }

                try
                {
                    TResult result = context.Operation!.Invoke(context);
                    if (context.IsCancellationRequested)
                        context.SetCanceled();
                    else
                        // a non-nullable TResult will not be null if the operation was not canceled
                        context.SetResult(result);
                }
                catch (OperationCanceledException)
                {
                    context.SetCanceled();
                }
                catch (Exception e)
                {
                    context.SetError(e);
                }
            }

            #endregion

            var asyncResult = new AsyncResultContext<TResult>(beginMethod ?? throw new ArgumentNullException(nameof(beginMethod), PublicResources.ArgumentNull),
                operation ?? throw new ArgumentNullException(nameof(operation), PublicResources.ArgumentNull), canceledResult, asyncConfig);
            if (asyncResult.IsCancellationRequested)
            {
                asyncResult.SetCanceled();
                asyncResult.CompletedSynchronously = true;
            }
            else
                ThreadPool.QueueUserWorkItem(DoWork!, asyncResult);
            return asyncResult;
        }

        /// <summary>
        /// Returns an <see cref="IAsyncResult"/> instance that represents an already completed operation without a result.
        /// The <see cref="EndOperation">EndOperation</see> method still can be called with the result of this method.
        /// <br/>See the <strong>Examples</strong> section of the <see cref="AsyncContext"/> class for details.
        /// The example uses the similarly working <see cref="FromResult{TResult}(TResult,AsyncConfig?,string)">FromResult</see> method.
        /// </summary>
        /// <param name="asyncConfig">The configuration for the asynchronous operation.</param>
        /// <param name="beginMethod">The name of the method that represents the operation.
        /// This must be passed also to the <see cref="EndOperation">EndOperation</see> method. This parameter is optional.
        /// <br/>Default value: The name of the caller method when used with a compiler that recognizes <see cref="CallerMemberNameAttribute"/>; otherwise, <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> instance representing the asynchronous operation.
        /// To complete the operation it must be passed to the <see cref="EndOperation">EndOperation</see> method.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="beginMethod"/> is <see langword="null"/>.</exception>
        public static IAsyncResult FromCompleted(AsyncConfig? asyncConfig, [CallerMemberName]string beginMethod = null!)
        {
            var asyncResult = new AsyncResultContext(beginMethod ?? throw new ArgumentNullException(nameof(beginMethod), PublicResources.ArgumentNull),
                null, asyncConfig);
            asyncResult.SetCompleted();
            asyncResult.CompletedSynchronously = true;
            return asyncResult;
        }

        /// <summary>
        /// Returns an <see cref="IAsyncResult"/> instance that represents an already completed operation with a result.
        /// To obtain the result the <see cref="EndOperation{TResult}">EndOperation</see> method must be called.
        /// <br/>See the <strong>Examples</strong> section of the <see cref="AsyncContext"/> class for details.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="result">The result to be returned by the <see cref="EndOperation{TResult}">EndOperation</see> method if the operation was not canceled.</param>
        /// <param name="asyncConfig">The configuration for the asynchronous operation.</param>
        /// <param name="beginMethod">The name of the method that represents the operation.
        /// This must be passed also to the <see cref="EndOperation">EndOperation</see> method. This parameter is optional.
        /// <br/>Default value: The name of the caller method when used with a compiler that recognizes <see cref="CallerMemberNameAttribute"/>; otherwise, <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> instance representing the asynchronous operation.
        /// To complete the operation it must be passed to the <see cref="EndOperation">EndOperation</see> method.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="beginMethod"/> is <see langword="null"/>.</exception>
        public static IAsyncResult FromResult<TResult>(TResult result, AsyncConfig? asyncConfig, [CallerMemberName]string beginMethod = null!)
            => FromResult(result, default, asyncConfig, beginMethod);

        /// <summary>
        /// Returns an <see cref="IAsyncResult"/> instance that represents an already completed operation with a result.
        /// To obtain the result the <see cref="EndOperation{TResult}">EndOperation</see> method must be called.
        /// <br/>See the <strong>Examples</strong> section of the <see cref="AsyncContext"/> class for details.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="result">The result to be returned by the <see cref="EndOperation{TResult}">EndOperation</see> method if the operation was not canceled.</param>
        /// <param name="canceledResult">The result to be returned by <see cref="EndOperation{TResult}">EndOperation</see> if the operation is canceled
        /// and <see cref="AsyncConfigBase.ThrowIfCanceled"/> of <paramref name="asyncConfig"/> returns <see langword="false"/>.</param>
        /// <param name="asyncConfig">The configuration for the asynchronous operation.</param>
        /// <param name="beginMethod">The name of the method that represents the operation.
        /// This must be passed also to the <see cref="EndOperation">EndOperation</see> method. This parameter is optional.
        /// <br/>Default value: The name of the caller method when used with a compiler that recognizes <see cref="CallerMemberNameAttribute"/>; otherwise, <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> instance representing the asynchronous operation.
        /// To complete the operation it must be passed to the <see cref="EndOperation">EndOperation</see> method.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="beginMethod"/> is <see langword="null"/>.</exception>
        public static IAsyncResult FromResult<TResult>(TResult result, TResult canceledResult, AsyncConfig? asyncConfig, [CallerMemberName]string beginMethod = null!)
        {
            var asyncResult = new AsyncResultContext<TResult>(beginMethod ?? throw new ArgumentNullException(nameof(beginMethod), PublicResources.ArgumentNull),
                null, canceledResult, asyncConfig);
            asyncResult.SetResult(asyncResult.IsCancellationRequested ? canceledResult : result);
            asyncResult.CompletedSynchronously = true;
            return asyncResult;
        }

        /// <summary>
        /// Waits for the completion of an operation started by a corresponding <see cref="BeginOperation">BeginOperation</see> or <see cref="FromCompleted(AsyncConfig?,string)">FromCompleted</see> call.
        /// If the operation is still running, then this method blocks the caller and waits for the completion.
        /// The possibly occurred exceptions are also thrown then this method is called.
        /// </summary>
        /// <param name="asyncResult">The result of a corresponding <see cref="BeginOperation">BeginOperation</see> or <see cref="FromCompleted(AsyncConfig?,string)">FromCompleted</see> call.</param>
        /// <param name="beginMethodName">The same name that was passed to the <see cref="BeginOperation">BeginOperation</see> or <see cref="FromCompleted(AsyncConfig?,string)">FromCompleted</see> method.</param>
        /// <exception cref="ArgumentNullException"><paramref name="asyncResult"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException"><paramref name="asyncResult"/> was not returned by the corresponding <see cref="BeginOperation">BeginOperation</see>
        /// or <see cref="FromCompleted(AsyncConfig?,string)">FromCompleted</see> methods with a matching <paramref name="beginMethodName"/>
        /// <br/>-or-
        /// <br/>this method was already called for this <paramref name="asyncResult"/> instance.</exception>
        public static void EndOperation(IAsyncResult asyncResult, string beginMethodName)
        {
            if (asyncResult == null)
                throw new ArgumentNullException(nameof(asyncResult), PublicResources.ArgumentNull);
            if (asyncResult is not AsyncResultContext result || result.GetType() != typeof(AsyncResultContext) || result.BeginMethodName != beginMethodName || result.IsDisposed)
                throw new InvalidOperationException(Res.InvalidAsyncResult(beginMethodName));
            try
            {
                result.WaitForCompletion();
            }
            finally
            {
                result.Dispose();
            }
        }

        /// <summary>
        /// Waits for the completion of an operation started by a corresponding <see cref="BeginOperation{TResult}(Func{IAsyncContext, TResult}, AsyncConfig?, string)">BeginOperation</see>
        /// or <see cref="FromResult{TResult}(TResult, AsyncConfig?, string)">FromResult</see> call.
        /// If the operation is still running, then this method blocks the caller and waits for the completion.
        /// The possibly occurred exceptions are also thrown then this method is called.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="asyncResult">The result of a corresponding <see cref="BeginOperation{TResult}(Func{IAsyncContext, TResult}, AsyncConfig?, string)">BeginOperation</see>
        /// or <see cref="FromResult{TResult}(TResult, AsyncConfig?, string)">FromResult</see> call.</param>
        /// <param name="beginMethodName">The same name that was passed to the <see cref="BeginOperation{TResult}(Func{IAsyncContext, TResult}, AsyncConfig?, string)">BeginOperation</see>
        /// or <see cref="FromResult{TResult}(TResult, AsyncConfig?, string)">FromResult</see> method.</param>
        /// <exception cref="ArgumentNullException"><paramref name="asyncResult"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException"><paramref name="asyncResult"/> was not returned by the corresponding <see cref="BeginOperation{TResult}(Func{IAsyncContext, TResult}, AsyncConfig?, string)">BeginOperation</see>
        /// or <see cref="FromResult{TResult}(TResult, AsyncConfig?, string)">FromResult</see> methods with a matching <paramref name="beginMethodName"/>
        /// <br/>-or-
        /// <br/>this method was already called for this <paramref name="asyncResult"/> instance.</exception>
        public static TResult EndOperation<TResult>(IAsyncResult asyncResult, string beginMethodName)
        {
            if (asyncResult == null)
                throw new ArgumentNullException(nameof(asyncResult), PublicResources.ArgumentNull);
            if (asyncResult is not AsyncResultContext<TResult> result || result.BeginMethodName != beginMethodName || result.IsDisposed)
                throw new InvalidOperationException(Res.InvalidAsyncResult(beginMethodName));
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
        public static Task<TResult?> DoOperationAsync<TResult>(Func<IAsyncContext, TResult?> operation, TaskConfig? asyncConfig)
            => DoOperationAsync(operation, default, asyncConfig);

        [SuppressMessage("Design", "CA1031:Do not catch general exception types",
            Justification = "Pool thread exceptions are not suppressed, they will be thrown when task is awaited or Result is accessed.")]
        public static Task<TResult?> DoOperationAsync<TResult>(Func<IAsyncContext, TResult?> operation, TResult canceledResult, TaskConfig? asyncConfig)
        {
            #region Local Methods

            // this method is executed on a pool thread
            static void DoWork(object state)
            {
                var (context, completion, func, canceledResult) = ((TaskContext, TaskCompletionSource<TResult?>, Func<IAsyncContext, TResult>, TResult))state;
                try
                {
                    TResult result = func.Invoke(context);
                    if (context.IsCancellationRequested)
                    {
                        if (context.ThrowIfCanceled)
                            completion.SetCanceled();
                        else
                            completion.SetResult(canceledResult);
                    }
                    else
                        completion.SetResult(result);
                }
                catch (OperationCanceledException)
                {
                    if (context.ThrowIfCanceled)
                        completion.SetCanceled();
                    else
                        completion.SetResult(canceledResult);
                }
                catch (Exception e)
                {
                    completion.SetException(e);
                }
            }

            #endregion

            var taskContext = new TaskContext(asyncConfig);
            var completionSource = new TaskCompletionSource<TResult?>(taskContext.State);
            if (taskContext.IsCancellationRequested)
            {
                if (taskContext.ThrowIfCanceled)
                    completionSource.SetCanceled();
                else
                    completionSource.SetResult(default);
            }
            else
                ThreadPool.QueueUserWorkItem(DoWork!, (taskContext, completionSource, operation, canceledResult));

            return completionSource.Task;
        }

        [SuppressMessage("Design", "CA1031:Do not catch general exception types",
            Justification = "Pool thread exceptions are not suppressed, they will be thrown when task is awaited or Result is accessed.")]
        public static Task DoOperationAsync(Action<IAsyncContext> operation, TaskConfig? asyncConfig)
        {
            #region Local Methods

            // this method is executed on a pool thread
            static void DoWork(object state)
            {
                var (context, completion, op) = ((TaskContext, TaskCompletionSource<object?>, Action<IAsyncContext>))state;
                try
                {
                    op.Invoke(context);
                    if (context.IsCancellationRequested && context.ThrowIfCanceled)
                        completion.SetCanceled();
                    else
                        completion.SetResult(default);
                }
                catch (OperationCanceledException)
                {
                    if (context.ThrowIfCanceled)
                        completion.SetCanceled();
                    else
                        completion.SetResult(default);
                }
                catch (Exception e)
                {
                    completion.SetException(e);
                }
            }

            #endregion

            var taskContext = new TaskContext(asyncConfig);
            var completionSource = new TaskCompletionSource<object?>(taskContext.State);
            if (taskContext.IsCancellationRequested)
            {
                if (taskContext.ThrowIfCanceled)
                    completionSource.SetCanceled();
                else
                    completionSource.SetResult(default);
            }
            else
                ThreadPool.QueueUserWorkItem(DoWork!, (taskContext, completionSource, operation));

            return completionSource.Task;
        }

        public static Task FromCompleted(TaskConfig? asyncConfig)
        {
            var taskContext = new TaskContext(asyncConfig);
            var completionSource = new TaskCompletionSource<object?>(taskContext.State);
            if (taskContext.IsCancellationRequested && taskContext.ThrowIfCanceled)
                completionSource.SetCanceled();
            else
                completionSource.SetResult(default);

            return completionSource.Task;
        }

        public static Task<TResult> FromResult<TResult>(TResult result, TaskConfig? asyncConfig)
            => FromResult(result, default!, asyncConfig);

        public static Task<TResult> FromResult<TResult>(TResult result, TResult canceledValue, TaskConfig? asyncConfig)
        {
            var taskContext = new TaskContext(asyncConfig);
            var completionSource = new TaskCompletionSource<TResult>(taskContext.State);
            if (taskContext.IsCancellationRequested)
            {
                if (taskContext.ThrowIfCanceled)
                    completionSource.SetCanceled();
                else
                    completionSource.SetResult(canceledValue);
            }
            else
                completionSource.SetResult(result);

            return completionSource.Task;
        }
#endif

        #endregion
    }
}
