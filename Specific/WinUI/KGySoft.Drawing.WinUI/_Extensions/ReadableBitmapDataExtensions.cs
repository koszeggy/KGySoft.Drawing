#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ReadableBitmapDataExtensions.cs
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

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

using KGySoft.Drawing.Imaging;
using KGySoft.Threading;

using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Media.Imaging;

using Windows.UI.Core;

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
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="source">The source <see cref="IReadWriteBitmapData"/> instance to covert.</param>
        /// <returns>A <see cref="WriteableBitmap"/> instance that has the same content as the specified <paramref name="source"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress.
        /// Use the <see cref="ToWriteableBitmapAsync">ToWriteableBitmapAsync</see> for asynchronous call and to adjust parallelization, set up cancellation and for reporting progress.</note>
        /// </remarks>
        public static WriteableBitmap ToWriteableBitmap(this IReadableBitmapData source)
        {
            ValidateArguments(source);
            return DoConvertToWriteableBitmap(AsyncHelper.DefaultContext, new ConversionContext(source))!;
        }

        /// <summary>
        /// Converts the specified <paramref name="source"/> to a <see cref="WriteableBitmap"/> asynchronously.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="source">The source <see cref="IReadableBitmapData"/> instance to covert.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/?topic=html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm" target="_blank">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/?topic=html/T_KGySoft_Threading_IAsyncProgress.htm" target="_blank">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. Its result is a <see cref="WriteableBitmap"/> instance that has the same content as the specified <paramref name="source"/>,
        /// or <see langword="null"/>, if the operation was canceled and the <a href="https://docs.kgysoft.net/corelibraries/?topic=html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm" target="_blank">ThrowIfCanceled</a>property of the <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/?topic=html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm" target="_blank">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="ToWriteableBitmap">ToWriteableBitmap</see> method for more details.</note>
        /// </remarks>
        public static Task<WriteableBitmap?> ToWriteableBitmapAsync(this IReadableBitmapData source, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(source);
            var context = new ConversionContext(source);
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
