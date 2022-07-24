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

using DispatcherPriority = System.Windows.Threading.DispatcherPriority;

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

        /// <summary>
        /// Converts the specified <paramref name="source"/> to a <see cref="WriteableBitmap"/>.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="source">The source <see cref="IReadWriteBitmapData"/> instance to covert.</param>
        /// <returns>A <see cref="WriteableBitmap"/> instance that has the same content as the specified <paramref name="source"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. Use the <see cref="BeginToWriteableBitmap">BeginToWriteableBitmap</see>
        /// or <see cref="ToWriteableBitmapAsync">ToWriteableBitmapAsync</see> (in .NET Framework 4.0 and above) methods for asynchronous call and to adjust parallelization, set up cancellation and for reporting progress.</note>
        /// <para>The result <see cref="WriteableBitmap"/> will have the closest possible <see cref="PixelFormat"/> to <paramref name="source"/>. If the source pixel format is a custom one,
        /// or has no direct representation in WPF, then the result will have either <see cref="PixelFormats.Bgr24"/> or <see cref="PixelFormats.Pbgra32"/> format, depending whether source has transparency.</para>
        /// </remarks>
        public static WriteableBitmap ToWriteableBitmap(this IReadableBitmapData source)
        {
            ValidateArguments(source);
            WriteableBitmap result = CreateCompatibleBitmap(source);
            IWritableBitmapData target = result.GetWritableBitmapData(source.BackColor.ToMediaColor(), source.AlphaThreshold);
            return DoConvertToWriteableBitmap(AsyncHelper.DefaultContext, source, target, result)!;
        }

        /// <summary>
        /// Begins to convert the specified <paramref name="source"/> to a <see cref="WriteableBitmap"/> asynchronously.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="source">The source <see cref="IReadWriteBitmapData"/> instance to covert.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/?topic=html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm" target="_blank">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/?topic=html/T_KGySoft_Threading_IAsyncProgress.htm" target="_blank">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="ToWriteableBitmapAsync">ToWriteableBitmapAsync</see> method.</para>
        /// <para>To get the result or the exception that occurred during the operation you have to call the <see cref="EndToWriteableBitmap">EndToWriteableBitmap</see> method.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/?topic=html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm" target="_blank">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="ToWriteableBitmap">ToWriteableBitmap</see> method for more details.</note>
        /// </remarks>
        public static IAsyncResult BeginToWriteableBitmap(this IReadableBitmapData source, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(source);
            WriteableBitmap result = CreateCompatibleBitmap(source);
            IWritableBitmapData target = result.GetWritableBitmapData(source.BackColor.ToMediaColor(), source.AlphaThreshold);
            return AsyncHelper.BeginOperation(ctx => DoConvertToWriteableBitmap(ctx, source, target, result), asyncConfig);
        }

        /// <summary>
        /// Waits for the pending asynchronous operation started by the <see cref="BeginToWriteableBitmap">BeginToWriteableBitmap</see> method to complete.
        /// In .NET Framework 4.0 and above you can use the <see cref="ToWriteableBitmapAsync">ToWriteableBitmapAsync</see> method instead.
        /// </summary>
        /// <param name="asyncResult">The reference to the pending asynchronous request to finish.</param>
        /// <returns>A <see cref="WriteableBitmap"/> instance that is the result of the operation,
        /// or <see langword="null"/>, if the operation was canceled and the <a href="https://docs.kgysoft.net/corelibraries/?topic=html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm" target="_blank">ThrowIfCanceled</a>property of the <c>asyncConfig</c> parameter was <see langword="false"/>.</returns>
        public static WriteableBitmap? EndToWriteableBitmap(this IAsyncResult asyncResult) => AsyncHelper.EndOperation<WriteableBitmap?>(asyncResult, nameof(BeginToWriteableBitmap));

#if !NET35
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
            WriteableBitmap result = CreateCompatibleBitmap(source);
            IWritableBitmapData target = result.GetWritableBitmapData(source.BackColor.ToMediaColor(), source.AlphaThreshold);
            return AsyncHelper.DoOperationAsync(ctx => DoConvertToWriteableBitmap(ctx, source, target, result), asyncConfig);
        }
#endif

        #endregion

        #region Private Methods

        private static WriteableBitmap CreateCompatibleBitmap(IBitmapData source)
        {
            PixelFormatInfo sourceFormat = source.PixelFormat;
            PixelFormat pixelFormat = sourceFormat.ToKnownPixelFormat().ToPixelFormat();
            Palette? palette = source.Palette;

            // indexed custom formats with >8 bpp: ToKnownPixelFormat returns 32bpp but it can be fine-tuned
            if (sourceFormat.IsCustomFormat && sourceFormat.Indexed && sourceFormat.BitsPerPixel > 8 && palette != null)
                pixelFormat = palette.HasAlpha ? PixelFormats.Bgra32
                    : palette.IsGrayscale ? PixelFormats.Gray16
                    : PixelFormats.Bgr24;

            return new WriteableBitmap(source.Width, source.Height, 96d, 96d, pixelFormat, pixelFormat.IsIndexed() ? palette.ToBitmapPalette() : null);
        }

        private static WriteableBitmap? DoConvertToWriteableBitmap(IAsyncContext context, IReadableBitmapData source, IWritableBitmapData target, WriteableBitmap result)
        {
            // As in WPF WriteableBitmap must be accessed from the same it was created in, we can only put the CopyTo in this method.
            // Here result is used only to access the dispatcher (to dispose target in the same thread result was created in) and to return it on success.
            try
            {
                if (context.IsCancellationRequested)
                    return null;
                return source.CopyTo(target, context, new Rectangle(Point.Empty, source.Size), Point.Empty) ? result : null;
            }
            finally
            {
                if (result.Dispatcher.CheckAccess())
                    target.Dispose();
                else
                    result.Dispatcher.BeginInvoke(DispatcherPriority.Send, target.Dispose);
            }
        }

        private static void ValidateArguments(IReadableBitmapData source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source), PublicResources.ArgumentNull);
            if (source.Width <= 0 || source.Height <= 0)
                throw new ArgumentException(Res.ReadableBitmapDataExtensionsInvalidBitmapDataSize, nameof(source));
        }

        #endregion

        #endregion
    }
}
