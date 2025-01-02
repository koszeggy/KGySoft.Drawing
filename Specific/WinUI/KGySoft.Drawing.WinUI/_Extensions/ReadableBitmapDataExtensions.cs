#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ReadableBitmapDataExtensions.cs
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
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using KGySoft.Drawing.Imaging;
using KGySoft.Threading;

using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Media.Imaging;

#endregion

namespace KGySoft.Drawing.WinUI
{
    /// <summary>
    /// Provides extension methods for the <see cref="IReadableBitmapData"/> type.
    /// </summary>
    public static class ReadableBitmapDataExtensions
    {
        #region ConversionContext class

        private sealed class ConversionContext : IDisposable
        {
            #region Fields

            private readonly DispatcherQueue dispatcher;

            #endregion

            #region Properties

            internal IReadableBitmapData Source { get; }
            internal WriteableBitmap Result { get; }
            internal IWritableBitmapData Target { get; }
            internal IQuantizer? Quantizer { get; }
            internal IDitherer? Ditherer { get; }

            #endregion

            #region Constructors

            internal ConversionContext(IReadableBitmapData source, IQuantizer? quantizer = null, IDitherer? ditherer = null)
            {
                Source = source;
                Result = new WriteableBitmap(source.Width, source.Height);
                dispatcher = Result.DispatcherQueue;
                Quantizer = quantizer;
                Ditherer = ditherer;
                Target = Result.GetWritableBitmapData();
            }

            #endregion

            #region Methods

            #region Public Methods

            public void Dispose() => Invoke(Target.Dispose);

            #endregion

            #region Private Methods

            private void Invoke(DispatcherQueueHandler callback)
            {
                // Running from the UI thread: direct invoke
                if (dispatcher.HasThreadAccess)
                {
                    callback.Invoke();
                    return;
                }

                // Running from a worker thread: beginning invoke and ignoring result
                dispatcher.TryEnqueue(DispatcherQueuePriority.High, callback);
            }

            #endregion

            #endregion
        }

        #endregion

        #region Methods

        #region Public Methods

        /// <summary>
        /// Converts the specified <paramref name="source"/> to a <see cref="WriteableBitmap"/>.
        /// </summary>
        /// <param name="source">The source <see cref="IReadableBitmapData"/> instance to convert.</param>
        /// <returns>A <see cref="WriteableBitmap"/> instance that has the same content as the specified <paramref name="source"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress.
        /// Use the <see cref="ToWriteableBitmapAsync(IReadableBitmapData, TaskConfig?)">ToWriteableBitmapAsync</see> for asynchronous call and to adjust parallelization, set up cancellation and for reporting progress.</note>
        /// </remarks>
        public static WriteableBitmap ToWriteableBitmap(this IReadableBitmapData source)
        {
            ValidateArguments(source);
            return DoConvertToWriteableBitmap(AsyncHelper.DefaultContext, new ConversionContext(source))!;
        }

        /// <summary>
        /// Converts the specified <paramref name="source"/> to a <see cref="WriteableBitmap"/> asynchronously.
        /// </summary>
        /// <param name="source">The source <see cref="IReadableBitmapData"/> instance to convert.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. Its result is a <see cref="WriteableBitmap"/> instance that has the same content as the specified <paramref name="source"/>,
        /// or <see langword="null"/>, if the operation was canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a>property of the <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="ToWriteableBitmap(IReadableBitmapData)">ToWriteableBitmap</see> method for more details.</note>
        /// </remarks>
        public static Task<WriteableBitmap?> ToWriteableBitmapAsync(this IReadableBitmapData source, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(source);
            var context = new ConversionContext(source);
            return AsyncHelper.DoOperationAsync(ctx => DoConvertToWriteableBitmap(ctx, context), asyncConfig);
        }

        /// <summary>
        /// Converts the specified <paramref name="source"/> to a <see cref="WriteableBitmap"/> using an optionally specified <paramref name="quantizer"/> and <paramref name="ditherer"/>.
        /// </summary>
        /// <param name="source">The source <see cref="IReadableBitmapData"/> instance to convert.</param>
        /// <param name="quantizer">An <see cref="IQuantizer"/> instance to use for the conversion, or <see langword="null"/> to do the conversion without reducing the colors.</param>
        /// <param name="ditherer">An <see cref="IDitherer"/> instance to use for the conversion or <see langword="null"/> to do the conversion without dithering. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A <see cref="WriteableBitmap"/> instance that has the same content as the specified <paramref name="source"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="COMException">Could not create the result <see cref="WriteableBitmap"/> on the current thread. Note that this method must be called on the UI thread.</exception>
        /// <remarks>
        /// <para>If <paramref name="quantizer"/> is <see langword="null"/>, then the <paramref name="ditherer"/> is also ignored.</para>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress.
        /// Use the <see cref="ToWriteableBitmapAsync(IReadableBitmapData, IQuantizer?, IDitherer?, TaskConfig?)">ToWriteableBitmapAsync</see> for asynchronous call and to adjust parallelization, set up cancellation and for reporting progress.</note>
        /// </remarks>
        public static WriteableBitmap ToWriteableBitmap(this IReadableBitmapData source, IQuantizer? quantizer, IDitherer? ditherer = null)
        {
            ValidateArguments(source);
            return DoConvertToWriteableBitmap(AsyncHelper.DefaultContext, new ConversionContext(source, quantizer, ditherer))!;
        }

        /// <summary>
        /// Converts the specified <paramref name="source"/> to a <see cref="WriteableBitmap"/> asynchronously, using an optionally specified <paramref name="quantizer"/> and <paramref name="ditherer"/>.
        /// </summary>
        /// <param name="source">The source <see cref="IReadableBitmapData"/> instance to convert.</param>
        /// <param name="quantizer">An <see cref="IQuantizer"/> instance to use for the conversion, or <see langword="null"/> to do the conversion without reducing the colors.</param>
        /// <param name="ditherer">An <see cref="IDitherer"/> instance to use for the conversion or <see langword="null"/> to do the conversion without dithering. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. Its result is a <see cref="WriteableBitmap"/> instance that has the same content as the specified <paramref name="source"/>,
        /// or <see langword="null"/>, if the operation was canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a>property of the <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="COMException">Could not create the result <see cref="WriteableBitmap"/> on the current thread. Note that this method must be called on the UI thread.</exception>
        /// <remarks>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="ToWriteableBitmap(IReadableBitmapData, IQuantizer?, IDitherer?)">ToWriteableBitmap</see> method for more details.</note>
        /// </remarks>
        public static Task<WriteableBitmap?> ToWriteableBitmapAsync(this IReadableBitmapData source, IQuantizer? quantizer, IDitherer? ditherer = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(source);
            var context = new ConversionContext(source, quantizer, ditherer);
            return AsyncHelper.DoOperationAsync(ctx => DoConvertToWriteableBitmap(ctx, context), asyncConfig);
        }

        #endregion

        #region Private Methods

        private static void ValidateArguments(IReadableBitmapData source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source), PublicResources.ArgumentNull);
            if (source.Width <= 0)
                throw new ArgumentException(PublicResources.PropertyMustBeGreaterThan(nameof(source.Width), 0), nameof(source));
            if (source.Height <= 0)
                throw new ArgumentException(PublicResources.PropertyMustBeGreaterThan(nameof(source.Height), 0), nameof(source));
        }

        private static WriteableBitmap? DoConvertToWriteableBitmap(IAsyncContext asyncContext, ConversionContext conversionContext)
        {
            using (conversionContext)
            {
                if (asyncContext.IsCancellationRequested)
                    return null;

                IReadableBitmapData source = conversionContext.Source;
                return source.CopyTo(conversionContext.Target, asyncContext, new Rectangle(Point.Empty, source.Size), Point.Empty, conversionContext.Quantizer, conversionContext.Ditherer)
                    ? conversionContext.Result
                    : null;
            }
        }

        #endregion

        #endregion
    }
}
