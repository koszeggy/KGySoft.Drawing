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
#if !NET35
using System.Threading.Tasks;
#endif

#endregion

namespace KGySoft.Drawing.Imaging
{
    /// <summary>
    /// Provides extension methods for the <see cref="IReadableBitmapData"/> type.
    /// </summary>
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
        /// <para>If supported on the current platform, the result <see cref="Bitmap"/> will have the same <see cref="KnownPixelFormat"/> as <paramref name="source"/>.
        /// Otherwise, the result will have either <see cref="KnownPixelFormat.Format24bppRgb"/> or <see cref="KnownPixelFormat.Format32bppArgb"/> format, depending whether source has transparency.
        /// <note>On Windows every format is supported with more or less limitations. For details and further information about the possible usable <see cref="KnownPixelFormat"/>s on different platforms
        /// see the <strong>Remarks</strong> section of the <see cref="ImageExtensions.ConvertPixelFormat(Image,KnownPixelFormat,Color,byte)">ConvertPixelFormat</see> method.
        /// </note></para>
        /// </remarks>
        public static Bitmap ToBitmap(this IReadableBitmapData source)
        {
            ValidateArguments(source);
            return DoConvertToBitmap(AsyncContext.Null, source)!;
        }

        /// <summary>
        /// Begins to convert the specified <paramref name="source"/> to a <see cref="Bitmap"/> asynchronously.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="source">The source <see cref="IReadWriteBitmapData"/> instance to covert.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="ToBitmapAsync">ToBitmapAsync</see> method.</para>
        /// <para>To get the result or the exception that occurred during the operation you have to call the <see cref="EndToBitmap">EndToBitmap</see> method.</para>
        /// <para>This method is not a blocking call even if the <see cref="AsyncConfigBase.MaxDegreeOfParallelism"/> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="ToBitmap">ToBitmap</see> method for more details.</note>
        /// </remarks>
        public static IAsyncResult BeginToBitmap(this IReadableBitmapData source, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(source);
            return AsyncContext.BeginOperation(ctx => DoConvertToBitmap(ctx, source), asyncConfig);
        }

        /// <summary>
        /// Waits for the pending asynchronous operation started by the <see cref="BeginToBitmap">BeginToBitmap</see> method to complete.
        /// In .NET Framework 4.0 and above you can use the <see cref="ToBitmapAsync">ToBitmapAsync</see> method instead.
        /// </summary>
        /// <param name="asyncResult">The reference to the pending asynchronous request to finish.</param>
        /// <returns>A <see cref="Bitmap"/> instance that is the result of the operation,
        /// or <see langword="null"/>, if the operation was canceled and <see cref="AsyncConfigBase.ThrowIfCanceled"/> property of the <c>asyncConfig</c> parameter was <see langword="false"/>.</returns>
        public static Bitmap? EndToBitmap(this IAsyncResult asyncResult) => AsyncContext.EndOperation<Bitmap>(asyncResult, nameof(BeginToBitmap));

#if !NET35
        /// <summary>
        /// Converts the specified <paramref name="source"/> to a <see cref="Bitmap"/> asynchronously.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="source">The source <see cref="IReadWriteBitmapData"/> instance to covert.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. Its result is a <see cref="Bitmap"/> instance that has the same content as the specified <paramref name="source"/>,
        /// or <see langword="null"/>, if the operation was canceled and <see cref="AsyncConfigBase.ThrowIfCanceled"/> property of the <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// <para>This method is not a blocking call even if the <see cref="AsyncConfigBase.MaxDegreeOfParallelism"/> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="ToBitmap">ToBitmap</see> method for more details.</note>
        /// </remarks>
        public static Task<Bitmap?> ToBitmapAsync(this IReadableBitmapData source, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(source);
            return AsyncContext.DoOperationAsync(ctx => DoConvertToBitmap(ctx, source), asyncConfig);
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
            PixelFormat pixelFormat = source.GetKnownPixelFormat().ToPixelFormat();
            if (!pixelFormat.IsSupportedNatively())
                pixelFormat = source.HasAlpha() ? PixelFormat.Format32bppArgb : PixelFormat.Format24bppRgb;

            var result = new Bitmap(source.Width, source.Height, pixelFormat);
            bool canceled = false;
            try
            {
                if (pixelFormat.IsIndexed() && source.Palette != null)
                    result.TrySetPalette(source.Palette);

                if (canceled = context.IsCancellationRequested)
                    return null;
                using (IBitmapDataInternal target = NativeBitmapDataFactory.CreateBitmapData(result, ImageLockMode.WriteOnly, source.BackColor, source.AlphaThreshold, source.Palette))
                    source.DoCopyTo(context, target, new Rectangle(Point.Empty, source.GetSize()), Point.Empty);
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
