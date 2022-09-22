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

#region Suppressions

#if NET35
#pragma warning disable CS1574 // XML comment has cref attribute that could not be resolved - in .NET 3.5 not all members are available
#endif

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
        /// </summary>
        /// <param name="source">The source <see cref="IReadableBitmapData"/> instance to covert.</param>
        /// <returns>A <see cref="Bitmap"/> instance that has the same content as the specified <paramref name="source"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. Use the <see cref="BeginToBitmap(IReadableBitmapData, AsyncConfig?)">BeginToBitmap</see>
        /// or <see cref="ToBitmapAsync(IReadableBitmapData, TaskConfig?)">ToBitmapAsync</see> (in .NET Framework 4.0 and above) methods for asynchronous call and to adjust parallelization, set up cancellation and for reporting progress.</note>
        /// <para>The result <see cref="Bitmap"/> will have the closest possible <see cref="PixelFormat"/> to <paramref name="source"/>. If the source pixel format is not supported on the current platform,
        /// then the result will have either <see cref="PixelFormat.Format24bppRgb"/> or <see cref="PixelFormat.Format32bppArgb"/> format, depending whether source has transparency.
        /// <note>On Windows every format is supported with more or less limitations. For details and further information about the possible usable <see cref="PixelFormat"/>s on different platforms
        /// see the <strong>Remarks</strong> section of the <see cref="ImageExtensions.ConvertPixelFormat(Image,PixelFormat,Color,byte)">ConvertPixelFormat</see> method.
        /// </note></para>
        /// </remarks>
        public static Bitmap ToBitmap(this IReadableBitmapData source)
        {
            ValidateArguments(source);
            return DoConvertToBitmapDirect(AsyncHelper.DefaultContext, source, GetCompatiblePixelFormat(source))!;
        }

        /// <summary>
        /// Converts the specified <paramref name="source"/> to a <see cref="Bitmap"/> that has the specified <see cref="PixelFormat"/>.
        /// </summary>
        /// <param name="source">The source <see cref="IReadWriteBitmapData"/> instance to covert.</param>
        /// <param name="pixelFormat">The desired result pixel format.</param>
        /// <param name="quantizer">An optional <see cref="IQuantizer"/> instance to determine the colors of the result.
        /// If <see langword="null"/> and <paramref name="pixelFormat"/> is an indexed format, then a default palette and quantization logic will be used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="ditherer">The ditherer to be used. Might be ignored if <paramref name="quantizer"/> is not specified
        /// and <paramref name="pixelFormat"/> represents an at least 24 bits-per-pixel size. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A <see cref="Bitmap"/> converted from the specified <paramref name="source"/>.</returns>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress.
        /// Use the <see cref="BeginToBitmap(IReadableBitmapData, PixelFormat, IQuantizer?, IDitherer?, AsyncConfig?)">BeginToBitmap</see>
        /// or <see cref="ToBitmapAsync(IReadableBitmapData, PixelFormat, IQuantizer?, IDitherer?, TaskConfig?)">ToBitmapAsync</see> (in .NET Framework 4.0 and above)
        /// methods for asynchronous call and to adjust parallelization, set up cancellation and for reporting progress.</note>
        /// <para>To produce a <see cref="Bitmap"/> with the best matching pixel format to <paramref name="source"/>,
        /// use the <see cref="ToBitmap(IReadableBitmapData)"/> overload instead.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="pixelFormat"/> does not specify a valid format.</exception>
        /// <exception cref="PlatformNotSupportedException">The specified <paramref name="pixelFormat"/> is not supported on the current platform.</exception>
        /// <exception cref="ArgumentException">The <paramref name="quantizer"/> palette contains too many colors for the indexed format specified by <paramref name="pixelFormat"/>.</exception>
        public static Bitmap ToBitmap(this IReadableBitmapData source, PixelFormat pixelFormat, IQuantizer? quantizer = null, IDitherer? ditherer = null)
        {
            ValidateArguments(source, pixelFormat);
            return DoConvertToBitmapByQuantizer(AsyncHelper.DefaultContext, source, pixelFormat, quantizer, ditherer)!;
        }

        /// <summary>
        /// Begins to convert the specified <paramref name="source"/> to a <see cref="Bitmap"/> asynchronously.
        /// </summary>
        /// <param name="source">The source <see cref="IReadableBitmapData"/> instance to covert.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="ToBitmapAsync(IReadableBitmapData, TaskConfig?)">ToBitmapAsync</see> method.</para>
        /// <para>To get the result or the exception that occurred during the operation you have to call the <see cref="EndToBitmap">EndToBitmap</see> method.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="ToBitmapAsync(IReadableBitmapData, TaskConfig?)">ToBitmap</see> method for more details.</note>
        /// </remarks>
        public static IAsyncResult BeginToBitmap(this IReadableBitmapData source, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(source);
            return AsyncHelper.BeginOperation(ctx => DoConvertToBitmapDirect(ctx, source, GetCompatiblePixelFormat(source)), asyncConfig);
        }

        /// <summary>
        /// Begins to convert the specified <paramref name="source"/> to a <see cref="Bitmap"/> with a specific <see cref="PixelFormat"/> asynchronously.
        /// </summary>
        /// <param name="source">The source <see cref="IReadWriteBitmapData"/> instance to covert.</param>
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
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="ToBitmapAsync(IReadableBitmapData, PixelFormat, IQuantizer?, IDitherer?, TaskConfig?)">ToBitmapAsync</see> method.</para>
        /// <para>To get the result or the exception that occurred during the operation you have to call the <see cref="EndToBitmap">EndToBitmap</see> method.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="pixelFormat"/> does not specify a valid format.</exception>
        /// <exception cref="PlatformNotSupportedException">The specified <paramref name="pixelFormat"/> is not supported on the current platform.</exception>
        public static IAsyncResult BeginToBitmap(this IReadableBitmapData source, PixelFormat pixelFormat, IQuantizer? quantizer = null, IDitherer? ditherer = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(source, pixelFormat);
            return AsyncHelper.BeginOperation(ctx => DoConvertToBitmapByQuantizer(ctx, source, pixelFormat, quantizer, ditherer), asyncConfig);
        }

        /// <summary>
        /// Waits for the pending asynchronous operation started by any of the <see cref="O:KGySoft.Drawing.Imaging.ReadableBitmapDataExtensions.BeginToBitmap">BeginToBitmap</see> methods to complete.
        /// In .NET Framework 4.0 and above you can use the <see cref="O:KGySoft.Drawing.Imaging.ReadableBitmapDataExtensions.ToBitmapAsync">ToBitmapAsync</see> methods instead.
        /// </summary>
        /// <param name="asyncResult">The reference to the pending asynchronous request to finish.</param>
        /// <returns>A <see cref="Bitmap"/> instance that is the result of the operation,
        /// or <see langword="null"/>, if the operation was canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a>property of the <c>asyncConfig</c> parameter was <see langword="false"/>.</returns>
        public static Bitmap? EndToBitmap(this IAsyncResult asyncResult) => AsyncHelper.EndOperation<Bitmap?>(asyncResult, nameof(BeginToBitmap));

#if !NET35
        /// <summary>
        /// Converts the specified <paramref name="source"/> to a <see cref="Bitmap"/> asynchronously.
        /// </summary>
        /// <param name="source">The source <see cref="IReadableBitmapData"/> instance to covert.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. Its result is a <see cref="Bitmap"/> instance that has the same content as the specified <paramref name="source"/>,
        /// or <see langword="null"/>, if the operation was canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a>property of the <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="ToBitmap(IReadableBitmapData)">ToBitmap</see> method for more details.</note>
        /// </remarks>
        public static Task<Bitmap?> ToBitmapAsync(this IReadableBitmapData source, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(source);
            return AsyncHelper.DoOperationAsync(ctx => DoConvertToBitmapDirect(ctx, source, GetCompatiblePixelFormat(source)), asyncConfig);
        }

        /// <summary>
        /// Converts the specified <paramref name="source"/> to a <see cref="Bitmap"/> asynchronously.
        /// </summary>
        /// <param name="source">The source <see cref="IReadableBitmapData"/> instance to covert.</param>
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
        /// <returns>A task that represents the asynchronous operation. Its result is a <see cref="Bitmap"/> instance that has the same content as the specified <paramref name="source"/>,
        /// or <see langword="null"/>, if the operation was canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a>property of the <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="pixelFormat"/> does not specify a valid format.</exception>
        /// <exception cref="ArgumentException">The <paramref name="quantizer"/> palette contains too many colors for the indexed format specified by <paramref name="pixelFormat"/>.</exception>
        /// <exception cref="InvalidOperationException">A deadlock has been detected while attempting to create the result.</exception>
        public static IAsyncResult ToBitmapAsync(this IReadableBitmapData source, PixelFormat pixelFormat, IQuantizer? quantizer = null, IDitherer? ditherer = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(source, pixelFormat);
            return AsyncHelper.DoOperationAsync(ctx => DoConvertToBitmapByQuantizer(ctx, source, pixelFormat, quantizer, ditherer), asyncConfig);
        }
#endif

        #endregion

        #region Internal Methods

        internal static Bitmap? ToBitmap(this IReadableBitmapData source, IAsyncContext context, PixelFormat pixelFormat, IQuantizer? quantizer, IDitherer? ditherer)
            => quantizer == null && ditherer == null
                ? DoConvertToBitmapDirect(context, source, pixelFormat)
                : DoConvertToBitmapByQuantizer(context, source, pixelFormat, quantizer, ditherer);

        #endregion

        #region Private Methods

        private static void ValidateArguments(IReadableBitmapData source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source), PublicResources.ArgumentNull);
            if (source.Width <= 0 || source.Height <= 0)
                throw new ArgumentException(Res.ImagingInvalidBitmapDataSize, nameof(source));
        }

        private static void ValidateArguments(IReadableBitmapData source, PixelFormat pixelFormat)
        {
            ValidateArguments(source);
            if (!pixelFormat.IsValidFormat())
                throw new ArgumentOutOfRangeException(nameof(pixelFormat), Res.PixelFormatInvalid(pixelFormat));
            if (!pixelFormat.IsSupportedNatively())
                throw new PlatformNotSupportedException(Res.PixelFormatNotSupported(pixelFormat));
        }

        [SuppressMessage("ReSharper", "AssignmentInConditionalExpression", Justification = "Intended")]
        private static Bitmap? DoConvertToBitmapDirect(IAsyncContext context, IReadableBitmapData source, PixelFormat pixelFormat)
        {
            if (context.IsCancellationRequested)
                return null;
            var result = new Bitmap(source.Width, source.Height, pixelFormat);
            bool canceled = false;
            try
            {
                // validating and initializing palette in target bitmap
                if (pixelFormat.IsIndexed())
                    result.TrySetPalette(GetTargetPalette(pixelFormat, source, null));

                if (canceled = context.IsCancellationRequested)
                    return null;
                using (IWritableBitmapData target = NativeBitmapDataFactory.CreateBitmapData(result, ImageLockMode.WriteOnly, source.BackColor, source.AlphaThreshold))
                    source.CopyTo(target, context, new Rectangle(Point.Empty, source.Size), Point.Empty);
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

        [SuppressMessage("ReSharper", "AssignmentInConditionalExpression", Justification = "Intended")]
        private static Bitmap? DoConvertToBitmapByQuantizer(IAsyncContext context, IReadableBitmapData source, PixelFormat pixelFormat, IQuantizer? quantizer, IDitherer? ditherer)
        {
            if (context.IsCancellationRequested)
                return null;
            Bitmap? result = null;
            bool canceled = false;
            try
            {
                Palette? sourcePaletteToApply = null;
                if (quantizer == null)
                {
                    // converting without using a quantizer (even if only a ditherer is specified for a high-bpp pixel format)
                    if (ditherer == null || !pixelFormat.CanBeDithered())
                        return DoConvertToBitmapDirect(context, source, pixelFormat);

                    // here we need to pick a quantizer for the dithering
                    int bpp = pixelFormat.ToBitsPerPixel();

                    sourcePaletteToApply = source.Palette;
                    if (bpp <= 8 && sourcePaletteToApply?.Count > 0 && sourcePaletteToApply.Count <= (1 << bpp))
                        quantizer = PredefinedColorsQuantizer.FromCustomPalette(sourcePaletteToApply);
                    else
                    {
                        quantizer = PredefinedColorsQuantizer.FromPixelFormat(pixelFormat.ToKnownPixelFormatInternal(), source.BackColor, source.AlphaThreshold);
                        sourcePaletteToApply = null;
                    }
                }

                if (canceled = context.IsCancellationRequested)
                    return null;

                result = new Bitmap(source.Width, source.Height, pixelFormat);
                Palette? paletteByQuantizer = null;
                Color32 backColor;
                byte alphaThreshold;

                switch (quantizer)
                {
                    // shortcut for predefined quantizers: we can extract everything
                    case PredefinedColorsQuantizer predefinedColorsQuantizer:
                        backColor = predefinedColorsQuantizer.BackColor;
                        alphaThreshold = predefinedColorsQuantizer.AlphaThreshold;
                        paletteByQuantizer = predefinedColorsQuantizer.Palette;
                        break;

                    // optimized quantizer: shortcut if we don't need the palette to initialize the result
                    case OptimizedPaletteQuantizer optimizedPaletteQuantizer when !pixelFormat.IsIndexed():
                        backColor = optimizedPaletteQuantizer.BackColor;
                        alphaThreshold = optimizedPaletteQuantizer.AlphaThreshold;
                        break;

                    // we explicitly initialize the quantizer just to determine the back color, alpha threshold and possible palette for the result
                    default:
                        context.Progress?.New(DrawingOperation.InitializingQuantizer);
                        using (IQuantizingSession quantizingSession = quantizer.Initialize(source, context))
                        {
                            if (canceled = context.IsCancellationRequested)
                                return null;
                            if (quantizingSession == null)
                                throw new InvalidOperationException(Res.ImageExtensionsQuantizerInitializeNull);

                            Debug.Assert(sourcePaletteToApply == null);
                            paletteByQuantizer = quantizingSession.Palette;
                            backColor = quantizingSession.BackColor;
                            alphaThreshold = quantizingSession.AlphaThreshold;

                            // We have a palette from a potentially expensive quantizer: creating a predefined quantizer from the already generated palette to avoid generating it again.
                            Debug.Assert(quantizer is not PredefinedColorsQuantizer);
                            if (paletteByQuantizer != null)
                                quantizer = PredefinedColorsQuantizer.FromCustomPalette(paletteByQuantizer);
                        }

                        break;
                }

                if (canceled = context.IsCancellationRequested)
                    return null;

                // validating and initializing palette
                if (pixelFormat.IsIndexed())
                    result.TrySetPalette(GetTargetPalette(pixelFormat, source, sourcePaletteToApply ?? paletteByQuantizer));

                using IWritableBitmapData target = NativeBitmapDataFactory.CreateBitmapData(result, ImageLockMode.WriteOnly, backColor, alphaThreshold);
                return (canceled = !source.CopyTo(target, context, new Rectangle(Point.Empty, source.Size), Point.Empty, quantizer, ditherer)) ? null : result;
            }
            catch (Exception)
            {
                result?.Dispose();
                result = null;
                throw;
            }
            finally
            {
                if (canceled)
                    result?.Dispose();
            }
        }

        private static PixelFormat GetCompatiblePixelFormat(IReadableBitmapData source)
        {
            var pixelFormat = source.PixelFormat.ToKnownPixelFormat().ToPixelFormat();
            return pixelFormat.IsSupportedNatively() ? pixelFormat
                : pixelFormat.HasAlpha() ? PixelFormat.Format32bppArgb
                : PixelFormat.Format24bppRgb;
        }

        private static Palette GetTargetPalette(PixelFormat pixelFormat, IReadableBitmapData source, Palette? palette)
        {
            Debug.Assert(pixelFormat.IsIndexed());

            int bpp = pixelFormat.ToBitsPerPixel();
            int maxColors = 1 << bpp;

            // if no desired colors are specified but converting to a higher bpp indexed image, then taking the source palette
            if (palette == null && source.PixelFormat.Indexed && source.Palette is { Count: > 0 } sourcePalette && sourcePalette.Count <= maxColors)
                return sourcePalette;

            if (palette == null || palette.Count == 0)
                return pixelFormat switch
                {
                    PixelFormat.Format8bppIndexed => Palette.SystemDefault8BppPalette(),
                    PixelFormat.Format4bppIndexed => Palette.SystemDefault4BppPalette(),
                    PixelFormat.Format1bppIndexed => Palette.SystemDefault1BppPalette(),
                    _ => throw new ArgumentOutOfRangeException(nameof(pixelFormat), PublicResources.ArgumentOutOfRange)
                };

            // there is a desired palette to apply
            if (palette.Count > maxColors)
                throw new ArgumentException(Res.PaletteTooLarge(maxColors, bpp), nameof(palette));

            return palette;
        }

        #endregion

        #endregion
    }
}
