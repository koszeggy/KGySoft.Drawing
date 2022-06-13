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
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
#if NET7_0_OR_GREATER
using System.Runtime.Versioning;
#endif
#if !NET35
using System.Threading.Tasks;
#endif

using KGySoft.Threading;

#endregion

namespace KGySoft.Drawing.Imaging
{
    /// <summary>
    /// Provides extension methods for the <see cref="IReadableBitmapData"/> type.
    /// </summary>
    /// <remarks>
    /// <note>When targeting .NET 7.0 or later versions this class is supported on Windows only.</note>
    /// </remarks>
#if NET7_0_OR_GREATER
    [SupportedOSPlatform("windows")]
#endif
    public static class ReadableBitmapDataExtensions
    {
        #region Methods

        #region Public Methods

        /// <summary>
        /// Converts the specified <paramref name="source"/> to a <see cref="Bitmap"/>.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="source">The source <see cref="IReadWriteBitmapData"/> instance to covert.</param>
        /// <returns>A <see cref="Bitmap"/> instance that has the same content as the specified <paramref name="source"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. Use the <see cref="BeginToBitmap">BeginToBitmap</see>
        /// or <see cref="ToBitmapAsync">ToBitmapAsync</see> (in .NET Framework 4.0 and above) methods for asynchronous call and to adjust parallelization, set up cancellation and for reporting progress.</note>
        /// <para>The result <see cref="Bitmap"/> will have the closest possible <see cref="PixelFormat"/> to <paramref name="source"/>. If the source pixel format is not supported on the current platform,
        /// then the result will have either <see cref="PixelFormat.Format24bppRgb"/> or <see cref="PixelFormat.Format32bppArgb"/> format, depending whether source has transparency.
        /// <note>On Windows every format is supported with more or less limitations. For details and further information about the possible usable <see cref="PixelFormat"/>s on different platforms
        /// see the <strong>Remarks</strong> section of the <see cref="ImageExtensions.ConvertPixelFormat(Image,PixelFormat,Color,byte)">ConvertPixelFormat</see> method.
        /// </note></para>
        /// </remarks>
        public static Bitmap ToBitmap(this IReadableBitmapData source)
        {
            ValidateArguments(source);
            return DoConvertToBitmap(AsyncHelper.DefaultContext, source)!;
        }

        /// <summary>
        /// Begins to convert the specified <paramref name="source"/> to a <see cref="Bitmap"/> asynchronously.
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
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="ToBitmapAsync">ToBitmapAsync</see> method.</para>
        /// <para>To get the result or the exception that occurred during the operation you have to call the <see cref="EndToBitmap">EndToBitmap</see> method.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/?topic=html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm" target="_blank">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="ToBitmap">ToBitmap</see> method for more details.</note>
        /// </remarks>
        public static IAsyncResult BeginToBitmap(this IReadableBitmapData source, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(source);
            return AsyncHelper.BeginOperation(ctx => DoConvertToBitmap(ctx, source), asyncConfig);
        }

        /// <summary>
        /// Waits for the pending asynchronous operation started by the <see cref="BeginToBitmap">BeginToBitmap</see> method to complete.
        /// In .NET Framework 4.0 and above you can use the <see cref="ToBitmapAsync">ToBitmapAsync</see> method instead.
        /// </summary>
        /// <param name="asyncResult">The reference to the pending asynchronous request to finish.</param>
        /// <returns>A <see cref="Bitmap"/> instance that is the result of the operation,
        /// or <see langword="null"/>, if the operation was canceled and the <a href="https://docs.kgysoft.net/corelibraries/?topic=html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm" target="_blank">ThrowIfCanceled</a>property of the <c>asyncConfig</c> parameter was <see langword="false"/>.</returns>
        public static Bitmap? EndToBitmap(this IAsyncResult asyncResult) => AsyncHelper.EndOperation<Bitmap>(asyncResult, nameof(BeginToBitmap));

#if !NET35
        /// <summary>
        /// Converts the specified <paramref name="source"/> to a <see cref="Bitmap"/> asynchronously.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="source">The source <see cref="IReadWriteBitmapData"/> instance to covert.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/?topic=html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm" target="_blank">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/?topic=html/T_KGySoft_Threading_IAsyncProgress.htm" target="_blank">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. Its result is a <see cref="Bitmap"/> instance that has the same content as the specified <paramref name="source"/>,
        /// or <see langword="null"/>, if the operation was canceled and the <a href="https://docs.kgysoft.net/corelibraries/?topic=html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm" target="_blank">ThrowIfCanceled</a>property of the <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/?topic=html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm" target="_blank">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="ToBitmap">ToBitmap</see> method for more details.</note>
        /// </remarks>
        public static Task<Bitmap?> ToBitmapAsync(this IReadableBitmapData source, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(source);
            return AsyncHelper.DoOperationAsync(ctx => DoConvertToBitmap(ctx, source), asyncConfig);
        }
#endif

        #endregion

        #region Private Methods

        private static void ValidateArguments(IReadableBitmapData source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source), PublicResources.ArgumentNull);
            if (source.Width <= 0 || source.Height <= 0)
                throw new ArgumentException(Res.ImagingInvalidBitmapDataSize, nameof(source));
        }

        [SuppressMessage("ReSharper", "AssignmentInConditionalExpression", Justification = "Intended")]
        private static Bitmap? DoConvertToBitmap(IAsyncContext context, IReadableBitmapData source)
        {
            PixelFormatInfo sourceFormat = source.PixelFormat;
            PixelFormat pixelFormat = sourceFormat.ToKnownPixelFormat().ToPixelFormat();
            Palette? palette = source.Palette;

            // indexed custom formats with >8 bpp: ToKnownPixelFormat returns 32bpp but it can be fine-tuned
            if (sourceFormat.IsCustomFormat && sourceFormat.Indexed && sourceFormat.BitsPerPixel > 8 && palette != null)
                pixelFormat = palette.HasAlpha ? PixelFormat.Format32bppArgb
                    : palette.IsGrayscale ? PixelFormat.Format16bppGrayScale
                    : PixelFormat.Format24bppRgb;

            if (!pixelFormat.IsSupportedNatively())
                pixelFormat = pixelFormat.HasAlpha() ? PixelFormat.Format32bppArgb : PixelFormat.Format24bppRgb;

            var result = new Bitmap(source.Width, source.Height, pixelFormat);
            bool canceled = false;
            try
            {
                if (pixelFormat.IsIndexed() && palette != null)
                    result.TrySetPalette(palette);

                if (canceled = context.IsCancellationRequested)
                    return null;
                using (IWritableBitmapData target = NativeBitmapDataFactory.CreateBitmapData(result, ImageLockMode.WriteOnly, source.BackColor, source.AlphaThreshold, source.Palette))
                    source.CopyTo(target, context, new Rectangle(0, 0, source.Width, source.Height), Point.Empty);
                return (canceled = context.IsCancellationRequested) ? null : result;
            }
            catch (Exception)
            {
                result.Dispose();
                result = null;
                throw;
            }
            finally
            {
                if (canceled)
                    result?.Dispose();
            }
        }

        #endregion

        #endregion
    }
}
