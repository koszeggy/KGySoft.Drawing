#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ReadableBitmapDataExtensions.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2023 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

#region Used Namespaces

using System;
using System.Drawing;
#if !NET35
using System.Threading.Tasks;
#endif
using System.Windows.Media;
using System.Windows.Media.Imaging;

using KGySoft.Drawing.Imaging;
using KGySoft.Threading;

#endregion

#region Used Aliases

#endregion

#endregion

#region Suppressions

#if NET35
#pragma warning disable CS1574 // XML comment has cref attribute that could not be resolved - in .NET 3.5 not all members are available
#endif

#endregion

namespace KGySoft.Drawing.Wpf
{
    /// <summary>
    /// Provides extension methods for the <see cref="IReadableBitmapData"/> type.
    /// </summary>
    public static class ReadableBitmapDataExtensions
    {
        #region Methods

        #region Public Methods

        #region Sync

        /// <summary>
        /// Converts the specified <paramref name="source"/> to a <see cref="WriteableBitmap"/>.
        /// </summary>
        /// <param name="source">The source <see cref="IReadableBitmapData"/> instance to convert.</param>
        /// <returns>A <see cref="WriteableBitmap"/> instance that has the same content as the specified <paramref name="source"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. Use the <see cref="BeginToWriteableBitmap(IReadableBitmapData, AsyncConfig?)">BeginToWriteableBitmap</see>
        /// or <see cref="ToWriteableBitmapAsync(IReadableBitmapData, TaskConfig?)">ToWriteableBitmapAsync</see> (in .NET Framework 4.0 and above) methods for asynchronous call and to adjust parallelization, set up cancellation and for reporting progress.</note>
        /// <para>The result <see cref="WriteableBitmap"/> will have the closest possible <see cref="PixelFormat"/> to <paramref name="source"/>.
        /// To create a result with a specific pixel format, use the <see cref="ToWriteableBitmap(IReadableBitmapData, PixelFormat, IQuantizer?, IDitherer?)"/> overload instead.</para>
        /// </remarks>
        public static WriteableBitmap ToWriteableBitmap(this IReadableBitmapData source)
        {
            ValidateArguments(source);
            return DoConvertToWriteableBitmapDirect(AsyncHelper.DefaultContext, new ConversionContext(source))!;
        }

        /// <summary>
        /// Converts the specified <paramref name="source"/> to a <see cref="WriteableBitmap"/> that has the specified <see cref="PixelFormat"/>.
        /// </summary>
        /// <param name="source">The source <see cref="IReadableBitmapData"/> instance to convert.</param>
        /// <param name="pixelFormat">The desired result pixel format.</param>
        /// <param name="quantizer">An optional <see cref="IQuantizer"/> instance to determine the colors of the result.
        /// If <see langword="null"/> and <paramref name="pixelFormat"/> is an indexed format, then a default palette and quantization logic will be used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="ditherer">The ditherer to be used. Might be ignored if <paramref name="quantizer"/> is not specified
        /// and <paramref name="pixelFormat"/> represents an at least 24 bits-per-pixel size. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A <see cref="WriteableBitmap"/> converted from the specified <paramref name="source"/>.</returns>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress.
        /// Use the <see cref="BeginToWriteableBitmap(IReadableBitmapData, PixelFormat, IQuantizer?, IDitherer?, AsyncConfig?)">BeginToWriteableBitmap</see>
        /// or <see cref="ToWriteableBitmapAsync(IReadableBitmapData, PixelFormat, IQuantizer?, IDitherer?, TaskConfig?)">ToWriteableBitmapAsync</see> (in .NET Framework 4.0 and above)
        /// methods for asynchronous call and to adjust parallelization, set up cancellation and for reporting progress.</note>
        /// <para>To produce a <see cref="WriteableBitmap"/> with the best matching pixel format to <paramref name="source"/>,
        /// use the <see cref="ToWriteableBitmap(IReadableBitmapData)"/> overload instead.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="pixelFormat"/> does not specify a valid format.</exception>
        /// <exception cref="ArgumentException">The <paramref name="quantizer"/> palette contains too many colors for the indexed format specified by <paramref name="pixelFormat"/>.</exception>
        public static WriteableBitmap ToWriteableBitmap(this IReadableBitmapData source, PixelFormat pixelFormat, IQuantizer? quantizer = null, IDitherer? ditherer = null)
        {
            ValidateArguments(source, pixelFormat);
            var context = new ConversionContext(source, pixelFormat, quantizer, ditherer);
            return context.Quantizer == null
                ? DoConvertToWriteableBitmapDirect(AsyncHelper.DefaultContext, context)!
                : DoConvertToWriteableBitmapWithQuantizer(AsyncHelper.DefaultContext, context)!;
        }

        #endregion

        #region Async APM

        /// <summary>
        /// Begins to convert the specified <paramref name="source"/> to a <see cref="WriteableBitmap"/> asynchronously.
        /// </summary>
        /// <param name="source">The source <see cref="IReadableBitmapData"/> instance to convert.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="ToWriteableBitmapAsync(IReadableBitmapData, TaskConfig?)">ToWriteableBitmapAsync</see> method.</para>
        /// <para>To get the result or the exception that occurred during the operation you have to call the <see cref="EndToWriteableBitmap">EndToWriteableBitmap</see> method.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        public static IAsyncResult BeginToWriteableBitmap(this IReadableBitmapData source, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(source);
            var context = new ConversionContext(source);
            return AsyncHelper.BeginOperation(ctx => DoConvertToWriteableBitmapDirect(ctx, context), asyncConfig);
        }

        /// <summary>
        /// Begins to convert the specified <paramref name="source"/> to a <see cref="WriteableBitmap"/> with a specific <see cref="PixelFormat"/> asynchronously.
        /// </summary>
        /// <param name="source">The source <see cref="IReadableBitmapData"/> instance to convert.</param>
        /// <param name="pixelFormat">The desired result pixel format.</param>
        /// <param name="quantizer">An optional <see cref="IQuantizer"/> instance to determine the colors of the result.
        /// If <see langword="null"/> and <paramref name="pixelFormat"/> is an indexed format, then a default palette and quantization logic will be used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="ditherer">The ditherer to be used. Might be ignored if <paramref name="quantizer"/> is not specified
        /// and <paramref name="pixelFormat"/> represents an at least 24 bits-per-pixel size. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="ToWriteableBitmapAsync(IReadableBitmapData, PixelFormat, IQuantizer?, IDitherer?, TaskConfig?)">ToWriteableBitmapAsync</see> method.</para>
        /// <para>To get the result or the exception that occurred during the operation you have to call the <see cref="EndToWriteableBitmap">EndToWriteableBitmap</see> method.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="caution">If <paramref name="quantizer"/> is not a <see cref="PredefinedColorsQuantizer"/>, then the result <see cref="WriteableBitmap"/> is created by a synchronized callback
        /// using the dispatcher of the thread this method was called from. The caller thread must have a running dispatcher and and must not be blocked; otherwise, a deadlock may occur.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="pixelFormat"/> does not specify a valid format.</exception>
        public static IAsyncResult BeginToWriteableBitmap(this IReadableBitmapData source, PixelFormat pixelFormat, IQuantizer? quantizer = null, IDitherer? ditherer = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(source, pixelFormat);
            var context = new ConversionContext(source, pixelFormat, quantizer, ditherer);
            return context.Quantizer == null
                ? AsyncHelper.BeginOperation(ctx => DoConvertToWriteableBitmapDirect(ctx, context), asyncConfig)
                : AsyncHelper.BeginOperation(ctx => DoConvertToWriteableBitmapWithQuantizer(ctx, context), asyncConfig);
        }

        /// <summary>
        /// Waits for the pending asynchronous operation started by any of
        /// the <see cref="O:KGySoft.Drawing.Wpf.ReadableBitmapDataExtensions.BeginToWriteableBitmap">BeginToWriteableBitmap</see> methods to complete.
        /// In .NET Framework 4.0 and above you can use the <see cref="O:KGySoft.Drawing.Wpf.ReadableBitmapDataExtensions.ToWriteableBitmapAsync">ToWriteableBitmapAsync</see> methods instead.
        /// </summary>
        /// <param name="asyncResult">The reference to the pending asynchronous request to finish.</param>
        /// <returns>A <see cref="WriteableBitmap"/> instance that is the result of the operation,
        /// or <see langword="null"/>, if the operation was canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a>property of the <c>asyncConfig</c> parameter was <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="asyncResult"/> is <see langword="null"/>, or was not returned by a <see cref="O:KGySoft.Drawing.Wpf.ReadableBitmapDataExtensions.BeginToWriteableBitmap">BeginToWriteableBitmap</see> overload.</exception>
        /// <exception cref="InvalidOperationException">A deadlock has been detected while attempting to create the result.</exception>
        public static WriteableBitmap? EndToWriteableBitmap(this IAsyncResult asyncResult) => AsyncHelper.EndOperation<WriteableBitmap?>(asyncResult, nameof(BeginToWriteableBitmap));

        #endregion

        #region Async TAP
#if !NET35

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
        /// <remarks>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        public static Task<WriteableBitmap?> ToWriteableBitmapAsync(this IReadableBitmapData source, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(source);
            var context = new ConversionContext(source);
            return AsyncHelper.DoOperationAsync(ctx => DoConvertToWriteableBitmapDirect(ctx, context), asyncConfig);
        }

        /// <summary>
        /// Converts the specified <paramref name="source"/> to a <see cref="WriteableBitmap"/> asynchronously.
        /// </summary>
        /// <param name="source">The source <see cref="IReadableBitmapData"/> instance to convert.</param>
        /// <param name="pixelFormat">The desired result pixel format.</param>
        /// <param name="quantizer">An optional <see cref="IQuantizer"/> instance to determine the colors of the result.
        /// If <see langword="null"/> and <paramref name="pixelFormat"/> is an indexed format, then a default palette and quantization logic will be used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="ditherer">The ditherer to be used. Might be ignored if <paramref name="quantizer"/> is not specified
        /// and <paramref name="pixelFormat"/> represents an at least 24 bits-per-pixel size. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. Its result is a <see cref="WriteableBitmap"/> instance that has the same content as the specified <paramref name="source"/>,
        /// or <see langword="null"/>, if the operation was canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a>property of the <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="caution">If <paramref name="quantizer"/> is not a <see cref="PredefinedColorsQuantizer"/>, then the result <see cref="WriteableBitmap"/> is created by a synchronized callback
        /// using the dispatcher of the thread this method was called from. The caller thread must have a running dispatcher and and must not be blocked; otherwise, a deadlock may occur.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="pixelFormat"/> does not specify a valid format.</exception>
        /// <exception cref="ArgumentException">The <paramref name="quantizer"/> palette contains too many colors for the indexed format specified by <paramref name="pixelFormat"/>.</exception>
        /// <exception cref="InvalidOperationException">A deadlock has been detected while attempting to create the result.</exception>
        public static Task<WriteableBitmap?> ToWriteableBitmapAsync(this IReadableBitmapData source, PixelFormat pixelFormat, IQuantizer? quantizer = null, IDitherer? ditherer = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(source, pixelFormat);
            var context = new ConversionContext(source, pixelFormat, quantizer, ditherer);
            return context.Quantizer == null
                ? AsyncHelper.DoOperationAsync(ctx => DoConvertToWriteableBitmapDirect(ctx, context), asyncConfig)
                : AsyncHelper.DoOperationAsync(ctx => DoConvertToWriteableBitmapWithQuantizer(ctx, context), asyncConfig);
        }

#endif
        #endregion

        #endregion

        #region Private Methods

        private static WriteableBitmap? DoConvertToWriteableBitmapDirect(IAsyncContext asyncContext, ConversionContext conversionContext)
        {
            using (conversionContext)
            {
                if (asyncContext.IsCancellationRequested)
                    return null;

                IReadableBitmapData source = conversionContext.Source;
                return source.CopyTo(conversionContext.Target!, asyncContext, new Rectangle(Point.Empty, source.Size), Point.Empty)
                    ? conversionContext.Result
                    : null;
            }
        }

        private static WriteableBitmap? DoConvertToWriteableBitmapWithQuantizer(IAsyncContext asyncContext, ConversionContext conversionContext)
        {
            using (conversionContext)
            {
                if (asyncContext.IsCancellationRequested)
                    return null;

                IReadableBitmapData source = conversionContext.Source;
                IQuantizer quantizer = conversionContext.Quantizer!;

                // we might have an uninitialized result if the quantizer is not a predefined one
                if (conversionContext.Result == null)
                {
                    Palette? palette;
                    Color32 backColor;
                    byte alphaThreshold;
                    asyncContext.Progress?.New(DrawingOperation.InitializingQuantizer);
                    using (IQuantizingSession quantizingSession = quantizer.Initialize(source, asyncContext))
                    {
                        if (asyncContext.IsCancellationRequested)
                            return null;
                        if (quantizingSession == null)
                            throw new InvalidOperationException(Res.QuantizerInitializeNull);

                        palette = quantizingSession.Palette;
                        backColor = quantizingSession.BackColor;
                        alphaThreshold = quantizingSession.AlphaThreshold;
                    }

                    conversionContext.Invoke(true, () =>
                    {
                        conversionContext.Result = new WriteableBitmap(source.Width, source.Height,
                            96, 96, conversionContext.PixelFormat,
                            conversionContext.GetTargetPalette(palette));
                        conversionContext.Target = conversionContext.Result.GetWritableBitmapData(backColor.ToMediaColor(), alphaThreshold);
                    });

                    // We have a palette from a potentially expensive quantizer: creating a predefined quantizer from the already generated palette to avoid generating it again.
                    if (palette != null && quantizer is not PredefinedColorsQuantizer)
                        quantizer = PredefinedColorsQuantizer.FromCustomPalette(palette);
                }

                if (asyncContext.IsCancellationRequested)
                    return null;

                return source.CopyTo(conversionContext.Target!, asyncContext, new Rectangle(Point.Empty, source.Size), Point.Empty, quantizer, conversionContext.Ditherer)
                    ? conversionContext.Result
                    : null;
            }
        }

        private static void ValidateArguments(IReadableBitmapData source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source), PublicResources.ArgumentNull);
            if (source.Width <= 0 || source.Height <= 0)
                throw new ArgumentException(Res.InvalidBitmapDataSize, nameof(source));
        }

        private static void ValidateArguments(IReadableBitmapData source, PixelFormat pixelFormat)
        {
            ValidateArguments(source);
            if (pixelFormat == default)
                throw new ArgumentOutOfRangeException(nameof(pixelFormat), PublicResources.ArgumentOutOfRange);
        }

        #endregion

        #endregion
    }
}
