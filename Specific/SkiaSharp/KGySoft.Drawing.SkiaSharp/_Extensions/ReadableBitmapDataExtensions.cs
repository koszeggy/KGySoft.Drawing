#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ReadableBitmapDataExtensions.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2025 - All Rights Reserved
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
using System.Threading.Tasks;

using KGySoft.CoreLibraries;
using KGySoft.Drawing.Imaging;
using KGySoft.Threading;

using SkiaSharp;

#endregion

namespace KGySoft.Drawing.SkiaSharp
{
    /// <summary>
    /// Provides extension methods for the <see cref="IReadableBitmapData"/> type.
    /// </summary>
    public static class ReadableBitmapDataExtensions
    {
        #region Methods

        #region Public Methods

        /// <summary>
        /// Converts the specified <paramref name="source"/> to an <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap">SKBitmap</a>.
        /// </summary>
        /// <param name="source">The source <see cref="IReadableBitmapData"/> instance to convert.</param>
        /// <returns>An <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap">SKBitmap</a> instance that has the same content as the specified <paramref name="source"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress.
        /// Use the <see cref="ToSKBitmapAsync(IReadableBitmapData, TaskConfig?)">ToSKBitmapAsync</see> for asynchronous call and to adjust parallelization, set up cancellation and for reporting progress.</note>
        /// </remarks>
        public static SKBitmap ToSKBitmap(this IReadableBitmapData source)
        {
            ValidateArguments(source);
            return DoConvertToSKBitmapDirect(AsyncHelper.DefaultContext, source, GetCompatibleImageInfo(source), source.BackColor, source.AlphaThreshold)!;
        }

        /// <summary>
        /// Converts the specified <paramref name="source"/> to an <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap">SKBitmap</a> that has the specified <paramref name="colorType"/>, <paramref name="alphaType"/> and color space.
        /// </summary>
        /// <param name="source">The source <see cref="IReadableBitmapData"/> instance to convert.</param>
        /// <param name="colorType">Determines the <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap.colortype">ColorType</a> property of the result <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap">SKBitmap</a>.
        /// Can be <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcolortype">Unknown</a> to auto select a color type that matches the <paramref name="source"/> pixel format.</param>
        /// <param name="alphaType">Determines the <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap.alphatype">AlphaType</a> property of the result <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap">SKBitmap</a>.
        /// It might be ignored if the <paramref name="colorType"/> cannot have the specified alpha type.
        /// Can be <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skalphatype">Unknown</a> to auto select an alpha type that matches the <paramref name="source"/> pixel format. This parameter is optional.
        /// <br/>Default value: <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skalphatype">Unknown</a>.</param>
        /// <param name="targetColorSpace">Determines both the <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap.colorspace">ColorSpace</a> property of the result <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap">SKBitmap</a>,
        /// and also the working color space if the <paramref name="quantizer"/> is <see langword="null"/>. This parameter is optional.
        /// <br/>Default value: <see cref="WorkingColorSpace.Default"/>.</param>
        /// <param name="quantizer">An optional <see cref="IQuantizer"/> instance to determine the colors of the result. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="ditherer">The ditherer to be used. Might be ignored if <paramref name="quantizer"/> is not specified
        /// and <paramref name="colorType"/> represents a higher bits-per-pixel per color channel format. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap">SKBitmap</a> converted from the specified <paramref name="source"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="colorType"/>, <paramref name="alphaType"/> or <paramref name="targetColorSpace"/> does not specify a defined value.</exception>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress.
        /// Use the <see cref="ToSKBitmapAsync(IReadableBitmapData, SKColorType, SKAlphaType, WorkingColorSpace, IQuantizer?, IDitherer?, TaskConfig?)">ToSKBitmapAsync</see>
        /// method for asynchronous call and to adjust parallelization, set up cancellation and for reporting progress.</note>
        /// <para>To produce an <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap">SKBitmap</a> with the best matching pixel format to <paramref name="source"/>,
        /// use the <see cref="ToSKBitmap(IReadableBitmapData)"/> overload instead.</para>
        /// <para>The <paramref name="targetColorSpace"/> parameter is purposely not an <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcolorspace">SKColorSpace</a> value because only sRGB and linear color spaces are supported directly.
        /// If its value is <see cref="WorkingColorSpace.Linear"/>, then both the actual color space of the result and the working color space of the conversion operation will be in
        /// the linear color space (unless <paramref name="quantizer"/> is specified, which determines the working color space).
        /// To create a result with sRGB color space but perform the conversion in the linear color space either use a <paramref name="quantizer"/> and configure it
        /// working in the linear color space, or create an <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap">SKBitmap</a> manually, obtain an <see cref="IWritableBitmapData"/> for it by
        /// the <see cref="SKBitmapExtensions.GetWritableBitmapData(SKBitmap, WorkingColorSpace, SKColor, byte)"/> method specifying the <see cref="WorkingColorSpace.Linear"/>
        /// working color space, and use the <see cref="BitmapDataExtensions.CopyTo(IReadableBitmapData, IWritableBitmapData, Point, IQuantizer?, IDitherer?)"/>
        /// method to copy <paramref name="source"/> into the manually created <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap">SKBitmap</a>.</para>
        /// </remarks>
        public static SKBitmap ToSKBitmap(this IReadableBitmapData source, SKColorType colorType, SKAlphaType alphaType = SKAlphaType.Unknown, WorkingColorSpace targetColorSpace = WorkingColorSpace.Default, IQuantizer? quantizer = null, IDitherer? ditherer = null)
        {
            ValidateArguments(source, colorType, alphaType, targetColorSpace);
            if (targetColorSpace == WorkingColorSpace.Default)
                targetColorSpace = source.GetPreferredColorSpace();
            
            return DoConvertToSKBitmapByQuantizer(AsyncHelper.DefaultContext, source,
                GetImageInfo(source, colorType, alphaType, targetColorSpace), quantizer, ditherer)!;
        }

        /// <summary>
        /// Converts the specified <paramref name="source"/> to an <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap">SKBitmap</a> that has the specified <paramref name="colorType"/> and <paramref name="alphaType"/>.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="ToSKBitmap(IReadableBitmapData, SKColorType, SKAlphaType, WorkingColorSpace, IQuantizer?, IDitherer?)"/> overload for details.
        /// </summary>
        /// <param name="source">The source <see cref="IReadableBitmapData"/> instance to convert.</param>
        /// <param name="colorType">Determines the <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap.colortype">ColorType</a> property of the result <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap">SKBitmap</a>.
        /// Can be <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcolortype">Unknown</a> to auto select a color type that matches the <paramref name="source"/> pixel format.</param>
        /// <param name="alphaType">Determines the <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap.alphatype">AlphaType</a> property of the result <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap">SKBitmap</a>.
        /// It might be ignored if the <paramref name="colorType"/> cannot have the specified alpha type.
        /// Can be <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skalphatype">Unknown</a> to auto select an alpha type that matches the <paramref name="source"/> pixel format.</param>
        /// <param name="quantizer">An optional <see cref="IQuantizer"/> instance to determine the colors of the result.</param>
        /// <param name="ditherer">The ditherer to be used. Might be ignored if <paramref name="quantizer"/> is not specified
        /// and <paramref name="colorType"/> represents a higher bits-per-pixel per color channel format. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap">SKBitmap</a> converted from the specified <paramref name="source"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="colorType"/> or <paramref name="alphaType"/> does not specify a defined value.</exception>
        public static SKBitmap ToSKBitmap(this IReadableBitmapData source, SKColorType colorType, SKAlphaType alphaType, IQuantizer? quantizer, IDitherer? ditherer = null)
            => ToSKBitmap(source, colorType, alphaType, WorkingColorSpace.Default, quantizer, ditherer);

        /// <summary>
        /// Converts the specified <paramref name="source"/> to an <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap">SKBitmap</a> that has the specified <paramref name="colorType"/>.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="ToSKBitmap(IReadableBitmapData, SKColorType, SKAlphaType, WorkingColorSpace, IQuantizer?, IDitherer?)"/> overload for details.
        /// </summary>
        /// <param name="source">The source <see cref="IReadableBitmapData"/> instance to convert.</param>
        /// <param name="colorType">Determines the <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap.colortype">ColorType</a> property of the result <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap">SKBitmap</a>.
        /// Can be <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcolortype">Unknown</a> to auto select a color type that matches the <paramref name="source"/> pixel format.</param>
        /// <param name="quantizer">An optional <see cref="IQuantizer"/> instance to determine the colors of the result.</param>
        /// <param name="ditherer">The ditherer to be used. Might be ignored if <paramref name="quantizer"/> is not specified
        /// and <paramref name="colorType"/> represents a higher bits-per-pixel per color channel format. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap">SKBitmap</a> converted from the specified <paramref name="source"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="colorType"/> does not specify a defined value.</exception>
        public static SKBitmap ToSKBitmap(this IReadableBitmapData source, SKColorType colorType, IQuantizer? quantizer, IDitherer? ditherer = null)
            => ToSKBitmap(source, colorType, SKAlphaType.Unknown, WorkingColorSpace.Default, quantizer, ditherer);

        /// <summary>
        /// Converts the specified <paramref name="source"/> to an <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap">SKBitmap</a> asynchronously.
        /// </summary>
        /// <param name="source">The source <see cref="IReadableBitmapData"/> instance to convert.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. Its result is an <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap">SKBitmap</a> instance that has the same content as the specified <paramref name="source"/>,
        /// or <see langword="null"/>, if the operation was canceled and the <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property of the <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// <para>This method is not a blocking call even if the <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="ToSKBitmap(IReadableBitmapData)">ToSKBitmap</see> method for more details.</note>
        /// </remarks>
        public static Task<SKBitmap?> ToSKBitmapAsync(this IReadableBitmapData source, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(source);
            return AsyncHelper.DoOperationAsync(ctx => DoConvertToSKBitmapDirect(ctx, source, GetCompatibleImageInfo(source), source.BackColor, source.AlphaThreshold), asyncConfig);
        }

        /// <summary>
        /// Converts the specified <paramref name="source"/> to an <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap">SKBitmap</a> asynchronously.
        /// </summary>
        /// <param name="source">The source <see cref="IReadableBitmapData"/> instance to convert.</param>
        /// <param name="colorType">Determines the <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap.colortype">ColorType</a> property of the result <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap">SKBitmap</a>.
        /// Can be <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcolortype">Unknown</a> to auto select a color type that matches the <paramref name="source"/> pixel format.</param>
        /// <param name="alphaType">Determines the <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap.alphatype">AlphaType</a> property of the result <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap">SKBitmap</a>.
        /// It might be ignored if the <paramref name="colorType"/> cannot have the specified alpha type.
        /// Can be <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skalphatype">Unknown</a> to auto select an alpha type that matches the <paramref name="source"/> pixel format. This parameter is optional.
        /// <br/>Default value: <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skalphatype">Unknown</a>.</param>
        /// <param name="targetColorSpace">Determines both the <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap.colorspace">ColorSpace</a> property of the result <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap">SKBitmap</a>,
        /// and also the working color space if the <paramref name="quantizer"/> is <see langword="null"/>. This parameter is optional.
        /// <br/>Default value: <see cref="WorkingColorSpace.Default"/>.</param>
        /// <param name="quantizer">An optional <see cref="IQuantizer"/> instance to determine the colors of the result. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="ditherer">The ditherer to be used. Might be ignored if <paramref name="quantizer"/> is not specified
        /// and <paramref name="colorType"/> represents a higher bits-per-pixel per color channel format. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. Its result is an <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap">SKBitmap</a> instance converted from the specified <paramref name="source"/>,
        /// or <see langword="null"/>, if the operation was canceled and the <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property of the <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="colorType"/>, <paramref name="alphaType"/> or <paramref name="targetColorSpace"/> does not specify a defined value.</exception>
        /// <remarks>
        /// <para>This method is not a blocking call even if the <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="ToSKBitmap(IReadableBitmapData, SKColorType, SKAlphaType, WorkingColorSpace, IQuantizer?, IDitherer?)"/> method for more details.</note>
        /// </remarks>
        public static Task<SKBitmap?> ToSKBitmapAsync(this IReadableBitmapData source, SKColorType colorType, SKAlphaType alphaType = SKAlphaType.Unknown, WorkingColorSpace targetColorSpace = WorkingColorSpace.Default, IQuantizer? quantizer = null, IDitherer? ditherer = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(source, colorType, alphaType, targetColorSpace);
            if (targetColorSpace == WorkingColorSpace.Default)
                targetColorSpace = source.GetPreferredColorSpace();

            return AsyncHelper.DoOperationAsync(ctx => DoConvertToSKBitmapByQuantizer(ctx, source,
                GetImageInfo(source, colorType, alphaType, targetColorSpace), quantizer, ditherer), asyncConfig);
        }

        /// <summary>
        /// Converts the specified <paramref name="source"/> to an <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap">SKBitmap</a> asynchronously.
        /// </summary>
        /// <param name="source">The source <see cref="IReadableBitmapData"/> instance to convert.</param>
        /// <param name="colorType">Determines the <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap.colortype">ColorType</a> property of the result <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap">SKBitmap</a>.
        /// Can be <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcolortype">Unknown</a> to auto select a color type that matches the <paramref name="source"/> pixel format.</param>
        /// <param name="alphaType">Determines the <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap.alphatype">AlphaType</a> property of the result <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap">SKBitmap</a>.
        /// It might be ignored if the <paramref name="colorType"/> cannot have the specified alpha type.
        /// Can be <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skalphatype">Unknown</a> to auto select an alpha type that matches the <paramref name="source"/> pixel format.</param>
        /// <param name="quantizer">An optional <see cref="IQuantizer"/> instance to determine the colors of the result.</param>
        /// <param name="ditherer">The ditherer to be used. Might be ignored if <paramref name="quantizer"/> is not specified
        /// and <paramref name="colorType"/> represents a higher bits-per-pixel per color channel format. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. Its result is an <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap">SKBitmap</a> instance converted from the specified <paramref name="source"/>,
        /// or <see langword="null"/>, if the operation was canceled and the <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property of the <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="colorType"/> or <paramref name="alphaType"/> does not specify a defined value.</exception>
        /// <remarks>
        /// <para>This method is not a blocking call even if the <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="ToSKBitmap(IReadableBitmapData, SKColorType, SKAlphaType, WorkingColorSpace, IQuantizer?, IDitherer?)"/> method for more details.</note>
        /// </remarks>
        public static Task<SKBitmap?> ToSKBitmapAsync(this IReadableBitmapData source, SKColorType colorType, SKAlphaType alphaType, IQuantizer? quantizer, IDitherer? ditherer = null, TaskConfig? asyncConfig = null)
            => ToSKBitmapAsync(source, colorType, alphaType, WorkingColorSpace.Default, quantizer, ditherer, asyncConfig);

        /// <summary>
        /// Converts the specified <paramref name="source"/> to an <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap">SKBitmap</a> asynchronously.
        /// </summary>
        /// <param name="source">The source <see cref="IReadableBitmapData"/> instance to convert.</param>
        /// <param name="colorType">Determines the <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap.colortype">ColorType</a> property of the result <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap">SKBitmap</a>.
        /// Can be <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcolortype">Unknown</a> to auto select a color type that matches the <paramref name="source"/> pixel format.</param>
        /// <param name="quantizer">An optional <see cref="IQuantizer"/> instance to determine the colors of the result.</param>
        /// <param name="ditherer">The ditherer to be used. Might be ignored if <paramref name="quantizer"/> is not specified
        /// and <paramref name="colorType"/> represents a higher bits-per-pixel per color channel format. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. Its result is an <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap">SKBitmap</a> instance converted from the specified <paramref name="source"/>,
        /// or <see langword="null"/>, if the operation was canceled and the <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property of the <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="colorType"/> does not specify a defined value.</exception>
        /// <remarks>
        /// <para>This method is not a blocking call even if the <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="ToSKBitmap(IReadableBitmapData, SKColorType, SKAlphaType, WorkingColorSpace, IQuantizer?, IDitherer?)"/> method for more details.</note>
        /// </remarks>
        public static Task<SKBitmap?> ToSKBitmapAsync(this IReadableBitmapData source, SKColorType colorType, IQuantizer? quantizer, IDitherer? ditherer = null, TaskConfig? asyncConfig = null)
            => ToSKBitmapAsync(source, colorType, SKAlphaType.Unknown, WorkingColorSpace.Default, quantizer, ditherer, asyncConfig);

        #endregion

        #region Internal Methods

        internal static SKBitmap? ToSKBitmap(this IReadableBitmapData source, IAsyncContext context, SKImageInfo imageInfo, Color32 backColor, byte alphaThreshold)
            => DoConvertToSKBitmapDirect(context, source, imageInfo, backColor, alphaThreshold);

        internal static SKBitmap? ToSKBitmap(this IReadableBitmapData source, IAsyncContext context, SKImageInfo imageInfo, IQuantizer? quantizer, IDitherer? ditherer)
            => DoConvertToSKBitmapByQuantizer(context, source, imageInfo, quantizer, ditherer);

        #endregion

        #region Private Methods

        private static void ValidateArguments(IReadableBitmapData source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source), PublicResources.ArgumentNull);
            if (source.Width <= 0 || source.Height <= 0)
                throw new ArgumentException(SkiaSharpRes.ImagingInvalidBitmapDataSize, nameof(source));
        }

        private static void ValidateArguments(IReadableBitmapData source, SKColorType colorType, SKAlphaType alphaType, WorkingColorSpace targetColorSpace)
        {
            ValidateArguments(source);
            if (!colorType.IsDefined())
                throw new ArgumentOutOfRangeException(nameof(colorType), PublicResources.EnumOutOfRange(colorType));
            if (!alphaType.IsDefined())
                throw new ArgumentOutOfRangeException(nameof(alphaType), PublicResources.EnumOutOfRange(alphaType));
            if (!targetColorSpace.IsDefined())
                throw new ArgumentOutOfRangeException(nameof(targetColorSpace), PublicResources.EnumOutOfRange(targetColorSpace));
        }

        [SuppressMessage("ReSharper", "AssignmentInConditionalExpression", Justification = "Intended")]
        private static SKBitmap? DoConvertToSKBitmapDirect(IAsyncContext context, IReadableBitmapData source, SKImageInfo imageInfo, Color32 backColor, byte alphaThreshold)
        {
            if (context.IsCancellationRequested)
                return null;
            bool canceled = false;
            SKBitmap? result = null;

            try
            {
                result = new SKBitmap(imageInfo);
                using (IWritableBitmapData target = result.GetBitmapDataInternal(false, source.WorkingColorSpace, backColor, alphaThreshold))
                    source.CopyTo(target, context, new Rectangle(Point.Empty, source.Size), Point.Empty);
                return (canceled = context.IsCancellationRequested) ? null : result;
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

        [SuppressMessage("ReSharper", "AssignmentInConditionalExpression", Justification = "Intended")]
        private static SKBitmap? DoConvertToSKBitmapByQuantizer(IAsyncContext context, IReadableBitmapData source, SKImageInfo imageInfo, IQuantizer? quantizer, IDitherer? ditherer)
        {
            if (context.IsCancellationRequested)
                return null;

            bool canceled = false;
            SKBitmap? result = null;
            try
            {
                if (quantizer == null)
                {
                    // converting without using a quantizer (even if only a ditherer is specified for a high-bpp or not directly supported pixel format)
                    if (ditherer == null || !imageInfo.CanBeDithered())
                        return DoConvertToSKBitmapDirect(context, source, imageInfo, source.BackColor, source.AlphaThreshold);

                    // here we need to pick a quantizer for the dithering
                    KnownPixelFormat asKnown = imageInfo.AsKnownPixelFormat();
                    if (asKnown != KnownPixelFormat.Undefined)
                        quantizer = PredefinedColorsQuantizer.FromPixelFormat(asKnown, source.BackColor, source.AlphaThreshold);
                    else
                    {
                        using var tempBitmap = new SKBitmap(imageInfo.WithSize(1, 1));
                        using var tempBitmapData = tempBitmap.GetBitmapDataInternal(true, imageInfo.GetWorkingColorSpace(), source.BackColor, source.AlphaThreshold);
                        quantizer = PredefinedColorsQuantizer.FromBitmapData(tempBitmapData);
                    }
                }

                if (canceled = context.IsCancellationRequested)
                    return null;

                result = new SKBitmap(imageInfo);

                // Extracting back color and alpha threshold from quantizer.
                // Palette is not needed because Skia does not support indexed formats anymore.
                Color32 backColor;
                byte alphaThreshold;
                switch (quantizer)
                {
                    // shortcut for predefined quantizers: we can extract everything
                    case PredefinedColorsQuantizer predefinedColorsQuantizer:
                        backColor = predefinedColorsQuantizer.BackColor;
                        alphaThreshold = predefinedColorsQuantizer.AlphaThreshold;
                        break;

                    // shortcut for optimized quantizer
                    case OptimizedPaletteQuantizer optimizedPaletteQuantizer:
                        backColor = optimizedPaletteQuantizer.BackColor;
                        alphaThreshold = optimizedPaletteQuantizer.AlphaThreshold;
                        break;

                    // we explicitly initialize the quantizer just to determine the back color and alpha threshold
                    default:
                        context.Progress?.New(DrawingOperation.InitializingQuantizer);
                        using (IQuantizingSession quantizingSession = quantizer.Initialize(source, context))
                        {
                            if (canceled = context.IsCancellationRequested)
                                return null;
                            if (quantizingSession == null)
                                throw new InvalidOperationException(SkiaSharpRes.ImageExtensionsQuantizerInitializeNull);

                            Palette? paletteByQuantizer = quantizingSession.Palette;
                            backColor = quantizingSession.BackColor;
                            alphaThreshold = quantizingSession.AlphaThreshold;

                            // We have a palette from a potentially expensive quantizer: creating a predefined quantizer from the already generated palette to avoid generating it again.
                            if (paletteByQuantizer != null)
                                quantizer = PredefinedColorsQuantizer.FromCustomPalette(paletteByQuantizer);
                        }

                        break;
                }

                if (canceled = context.IsCancellationRequested)
                    return null;

                using IWritableBitmapData target = result.GetWritableBitmapData(source.WorkingColorSpace, backColor.ToSKColor(), alphaThreshold);
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

        private static SKImageInfo GetCompatibleImageInfo(IReadableBitmapData source)
        {
            PixelFormatInfo pixelFormat = source.PixelFormat;
            var result = new SKImageInfo(source.Width, source.Height);

            // Indexed formats: Skia no longer supports them so the choice is likely an overkill
            if (pixelFormat.Indexed && source.Palette is Palette palette)
            {
                result.AlphaType = palette.HasAlpha ? SKAlphaType.Unpremul : SKAlphaType.Opaque;
                result.ColorType = palette.IsGrayscale && !palette.HasAlpha
                    ? SKColorType.Gray8 // no need to check BPP because the palette cannot have more than 256 different grayscale entries
                    : SKImageInfo.PlatformColorType; // palette entries are 32 bpp colors so this will do it
            }
            else
            {
                // Non-indexed formats
                result.AlphaType = pixelFormat.HasPremultipliedAlpha ? SKAlphaType.Premul
                    : pixelFormat.HasAlpha ? SKAlphaType.Unpremul
                    : SKAlphaType.Opaque;

                result.ColorType = pixelFormat.Grayscale ? pixelFormat switch
                    {
                        // Grayscale formats
                        { BitsPerPixel: > 32 } or { BitsPerPixel: 32, HasAlpha: false } => SKColorType.RgbaF32,
                        { BitsPerPixel: > 8, HasAlpha: true } or { BitsPerPixel: > 10, HasAlpha: false } => SKColorType.Rgba16161616,
                        { BitsPerPixel: > 8, HasAlpha: false } => SKColorType.Bgr101010x, // a 9/10 bpp format is not too likely though
                        { HasAlpha: true } => SKImageInfo.PlatformColorType,
                        _ => SKColorType.Gray8
                    }
                    : pixelFormat.HasAlpha ? pixelFormat.BitsPerPixel switch
                    {
                        // Formats with alpha
                        > 64 => SKColorType.RgbaF32,
                        > 32 => SKColorType.Rgba16161616,
                        _ => SKImageInfo.PlatformColorType
                    }
                    : pixelFormat.BitsPerPixel switch
                    {
                        // Opaque formats
                        > 48 => SKColorType.RgbaF32,
                        > 32 => SKColorType.Rgba16161616,
                        > 24 => SKColorType.Bgr101010x, // this actually turns 888x formats to 101010x, but it's better to assume the better quality
                        _ => SKImageInfo.PlatformColorType
                    };
            }

            result.ColorSpace = source.GetPreferredColorSpace() is WorkingColorSpace.Linear
                ? SKColorSpace.CreateSrgbLinear()
                : SKColorSpace.CreateSrgb();

            return result;
        }

        private static SKImageInfo GetImageInfo(IReadableBitmapData source, SKColorType colorType, SKAlphaType alphaType, WorkingColorSpace targetColorSpace)
        {
            var result = new SKImageInfo(source.Width, source.Height, colorType, alphaType, targetColorSpace == WorkingColorSpace.Linear ? SKColorSpace.CreateSrgbLinear() : SKColorSpace.CreateSrgb());
            if (colorType == SKColorType.Unknown || alphaType == SKAlphaType.Unknown)
            {
                var compatibleInfo = GetCompatibleImageInfo(source);
                if (colorType == SKColorType.Unknown)
                    result.ColorType = compatibleInfo.ColorType;
                if (alphaType == SKAlphaType.Unknown)
                    result.AlphaType = compatibleInfo.AlphaType;
            }

            return result;
        }

        #endregion

        #endregion
    }
}
