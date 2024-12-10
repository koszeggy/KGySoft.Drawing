#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: GifEncoder.Encode.cs
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
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Threading;
#if !NET35
using System.Threading.Tasks;
#endif

using KGySoft.CoreLibraries;
using KGySoft.Threading;

#endregion

#region Suppressions

#if NET35
#pragma warning disable CS1574 // XML comment has cref attribute that could not be resolved - in .NET 3.5 not all members are available
#endif

#endregion

namespace KGySoft.Drawing.Imaging
{
    public partial class GifEncoder
    {
        #region Constants

        private const int parallelThreshold = 100;

        #endregion

        #region Methods

        #region Public Methods

        #region EncodeImage

        /// <summary>
        /// Encodes the specified <paramref name="imageData"/> as a GIF image and writes it into the specified <paramref name="stream"/>.
        /// </summary>
        /// <param name="imageData">The image data to write. Non-indexed images will be quantized by using the specified <paramref name="quantizer"/>, or, if that is not set,
        /// by the <see cref="OptimizedPaletteQuantizer.Wu(int,Color32,byte)">Wu</see> quantizer or <see cref="PredefinedColorsQuantizer.Grayscale(Color32,byte)">Grayscale</see> quantizer, depending on the pixel format.</param>
        /// <param name="stream">The stream to save the encoded image into.</param>
        /// <param name="quantizer">An optional <see cref="IQuantizer"/> instance to determine the colors of the result.
        /// If <see langword="null"/> and <paramref name="imageData"/> is not an indexed image or the palette contains multiple alpha entries,
        /// then the <see cref="OptimizedPaletteQuantizer.Wu(int,Color32,byte)">Wu</see> or <see cref="PredefinedColorsQuantizer.Grayscale(Color32,byte)">Grayscale</see> quantizer will be used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="ditherer">The ditherer to be used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="imageData"/> or <paramref name="stream"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. Use the <see cref="BeginEncodeImage">BeginEncodeImage</see>
        /// or <see cref="EncodeImageAsync">EncodeImageAsync</see> (in .NET Framework 4.0 and above) methods for asynchronous call and to set up cancellation or for reporting progress.</note>
        /// <para>To encode an <a href="https://docs.microsoft.com/en-us/dotnet/api/System.Drawing.Image" target="_blank">Image</a> you can use also the <see cref="O:KGySoft.Drawing.ImageExtensions.SaveAsGif">ImageExtensions.SaveAsGif</see>
        /// methods that provide a higher level access.</para>
        /// <para>To create a GIF completely manually you can create a <see cref="GifEncoder"/> instance that provides a lower level access.</para>
        /// <para>If <paramref name="quantizer"/> is specified, then it will be used even for already indexed images.</para>
        /// <para>If <paramref name="quantizer"/> is an <see cref="OptimizedPaletteQuantizer"/>, then the palette of the result image will be adjusted for the actual image content.</para>
        /// </remarks>
        public static void EncodeImage(IReadableBitmapData imageData, Stream stream, IQuantizer? quantizer = null, IDitherer? ditherer = null)
        {
            ValidateArguments(imageData, stream);
            DoEncodeImage(AsyncHelper.DefaultContext, imageData, stream, quantizer, ditherer);
        }

        /// <summary>
        /// Begins to encode the specified <paramref name="imageData"/> as a GIF image and to write it into the specified <paramref name="stream"/>.
        /// </summary>
        /// <param name="imageData">The image data to write. Non-indexed images will be quantized by using the specified <paramref name="quantizer"/>, or, if that is not set,
        /// by the <see cref="OptimizedPaletteQuantizer.Wu(int,Color32,byte)">Wu</see> quantizer or <see cref="PredefinedColorsQuantizer.Grayscale(Color32,byte)">Grayscale</see> quantizer, depending on the pixel format.</param>
        /// <param name="stream">The stream to save the encoded image into.</param>
        /// <param name="quantizer">An optional <see cref="IQuantizer"/> instance to determine the colors of the result.
        /// If <see langword="null"/> and <paramref name="imageData"/> is not an indexed image or the palette contains multiple alpha entries,
        /// then the <see cref="OptimizedPaletteQuantizer.Wu(int,Color32,byte)">Wu</see> or <see cref="PredefinedColorsQuantizer.Grayscale(Color32,byte)">Grayscale</see> quantizer will be used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="ditherer">The ditherer to be used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="imageData"/> or <paramref name="stream"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="EncodeImageAsync">EncodeImageAsync</see> method.</para>
        /// <para>To finish the operation and to get the exception that occurred during the operation you have to call the <see cref="EndEncodeImage">EndEncodeImage</see> method.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.
        /// The encoding itself cannot be parallelized. The <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> setting affects only the quantizing session
        /// if <paramref name="imageData"/> has a non-indexed pixel format, or when <paramref name="quantizer"/> is set.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="EncodeImage">EncodeImage</see> method for more details.</note>
        /// </remarks>
        public static IAsyncResult BeginEncodeImage(IReadableBitmapData imageData, Stream stream, IQuantizer? quantizer = null, IDitherer? ditherer = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(imageData, stream);
            return AsyncHelper.BeginOperation(ctx => DoEncodeImage(ctx, imageData, stream, quantizer, ditherer), asyncConfig);
        }

        /// <summary>
        /// Waits for the pending asynchronous operation started by the <see cref="BeginEncodeImage">BeginEncodeImage</see> method to complete.
        /// In .NET Framework 4.0 and above you can use the <see cref="EncodeImageAsync">EncodeImageAsync</see> method instead.
        /// </summary>
        /// <param name="asyncResult">The reference to the pending asynchronous request to finish.</param>
        public static void EndEncodeImage(IAsyncResult asyncResult) => AsyncHelper.EndOperation(asyncResult, nameof(BeginEncodeImage));

#if !NET35
        /// <summary>
        /// Encodes the specified <paramref name="imageData"/> as a GIF image asynchronously, and writes it into the specified <paramref name="stream"/>.
        /// </summary>
        /// <param name="imageData">The image data to write. Non-indexed images will be quantized by using the specified <paramref name="quantizer"/>, or, if that is not set,
        /// by the <see cref="OptimizedPaletteQuantizer.Wu(int,Color32,byte)">Wu</see> quantizer or <see cref="PredefinedColorsQuantizer.Grayscale(Color32,byte)">Grayscale</see> quantizer, depending on the pixel format.</param>
        /// <param name="stream">The stream to save the encoded image into.</param>
        /// <param name="quantizer">An optional <see cref="IQuantizer"/> instance to determine the colors of the result.
        /// If <see langword="null"/> and <paramref name="imageData"/> is not an indexed image or the palette contains multiple alpha entries,
        /// then the <see cref="OptimizedPaletteQuantizer.Wu(int,Color32,byte)">Wu</see> or <see cref="PredefinedColorsQuantizer.Grayscale(Color32,byte)">Grayscale</see> quantizer will be used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="ditherer">The ditherer to be used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="imageData"/> or <paramref name="stream"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.
        /// The encoding itself cannot be parallelized. The <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> setting affects only the quantizing session
        /// if <paramref name="imageData"/> has a non-indexed pixel format, or when <paramref name="quantizer"/> is set.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="EncodeImage">EncodeImage</see> method for more details.</note>
        /// </remarks>
        public static Task EncodeImageAsync(IReadableBitmapData imageData, Stream stream, IQuantizer? quantizer = null, IDitherer? ditherer = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(imageData, stream);
            return AsyncHelper.DoOperationAsync(ctx => DoEncodeImage(ctx, imageData, stream, quantizer, ditherer), asyncConfig);
        }
#endif

        #endregion

        #region EncodeAnimation

        /// <summary>
        /// Encodes the frames of the specified <paramref name="configuration"/> as an animated GIF image and writes it into the specified <paramref name="stream"/>.
        /// </summary>
        /// <param name="configuration">An <see cref="AnimatedGifConfiguration"/> instance describing the configuration of the encoding.</param>
        /// <param name="stream">The stream to save the encoded animation into.</param>
        /// <exception cref="ArgumentNullException"><paramref name="configuration"/> or <paramref name="stream"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="configuration"/> is invalid.</exception>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. Use the <see cref="BeginEncodeAnimation">BeginEncodeAnimation</see>
        /// or <see cref="EncodeAnimationAsync">EncodeAnimationAsync</see> (in .NET Framework 4.0 and above) methods for asynchronous call and to set up cancellation or for reporting progress.</note>
        /// <para>To encode <a href="https://docs.microsoft.com/en-us/dotnet/api/System.Drawing.Image" target="_blank">Image</a> instances with default configuration you can use the <see cref="O:KGySoft.Drawing.ImageExtensions.SaveAsAnimatedGif">ImageExtensions.SaveAsAnimatedGif</see>
        /// methods that provide a higher level access.</para>
        /// <para>To create an animation completely manually you can create a <see cref="GifEncoder"/> instance that provides a lower level access.</para>
        /// </remarks>
        public static void EncodeAnimation(AnimatedGifConfiguration configuration, Stream stream)
        {
            ValidateArguments(configuration, stream);
            DoEncodeAnimation(AsyncHelper.DefaultContext, configuration, stream);
        }

        /// <summary>
        /// Begins to encode the frames of the specified <paramref name="configuration"/> as an animated GIF image and to write it into the specified <paramref name="stream"/>.
        /// </summary>
        /// <param name="configuration">An <see cref="AnimatedGifConfiguration"/> instance describing the configuration of the encoding.</param>
        /// <param name="stream">The stream to save the encoded animation into.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="configuration"/> or <paramref name="stream"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="configuration"/> is invalid.</exception>
        /// <remarks>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="EncodeAnimationAsync">EncodeAnimationAsync</see> method.</para>
        /// <para>To finish the operation and to get the exception that occurred during the operation you have to call the <see cref="EndEncodeAnimation">EndEncodeAnimation</see> method.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="EncodeAnimation">EncodeAnimation</see> method for more details.</note>
        /// </remarks>
        public static IAsyncResult BeginEncodeAnimation(AnimatedGifConfiguration configuration, Stream stream, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(configuration, stream);
            return AsyncHelper.BeginOperation(ctx => DoEncodeAnimation(ctx, configuration, stream), asyncConfig);
        }

        /// <summary>
        /// Waits for the pending asynchronous operation started by the <see cref="BeginEncodeAnimation">BeginEncodeAnimation</see> method to complete.
        /// In .NET Framework 4.0 and above you can use the <see cref="EncodeAnimationAsync">EncodeAnimationAsync</see> method instead.
        /// </summary>
        /// <param name="asyncResult">The reference to the pending asynchronous request to finish.</param>
        public static void EndEncodeAnimation(IAsyncResult asyncResult) => AsyncHelper.EndOperation(asyncResult, nameof(BeginEncodeAnimation));

#if !NET35
        /// <summary>
        /// Encodes the frames of the specified <paramref name="configuration"/> as an animated GIF image asynchronously, and writes it into the specified <paramref name="stream"/>.
        /// </summary>
        /// <param name="configuration">An <see cref="AnimatedGifConfiguration"/> instance describing the configuration of the encoding.</param>
        /// <param name="stream">The stream to save the encoded animation into.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="configuration"/> or <paramref name="stream"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="configuration"/> is invalid.</exception>
        /// <remarks>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="EncodeAnimation">EncodeAnimation</see> method for more details.</note>
        /// </remarks>
        public static Task EncodeAnimationAsync(AnimatedGifConfiguration configuration, Stream stream, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(configuration, stream);
            return AsyncHelper.DoOperationAsync(ctx => DoEncodeAnimation(ctx, configuration, stream), asyncConfig);
        }
#endif

        #endregion

        #region EncodeHighColorImage

        /// <summary>
        /// Encodes the specified <paramref name="imageData"/> as a multi-layered, single frame GIF image and writes it into the specified <paramref name="stream"/>, preserving its original color depth.
        /// </summary>
        /// <param name="imageData">The image data to write. Possible alpha pixels might be blended with <paramref name="backColor"/> but otherwise the color depth will be preserved.</param>
        /// <param name="stream">The stream to save the encoded animation into.</param>
        /// <param name="allowFullScan"><see langword="true"/> to allow scanning the whole image for each layers to be able to re-use the local palette of the current layer.
        /// <br/><see langword="false"/> to expand the initial layer area to the local pixels only. This parameter is optional.
        /// <br/>Default value: <see langword="false"/>.</param>
        /// <param name="backColor">Colors with alpha (transparency), whose <see cref="Color32.A">Color32.A</see> field
        /// is equal to or greater than <paramref name="alphaThreshold"/> will be blended with this color during the encoding.
        /// The alpha value (<see cref="Color32.A">Color32.A</see> field) of the specified background color is ignored. This parameter is optional.
        /// <br/>Default value: The default value of the <see cref="Color32"/> type, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="alphaThreshold">Specifies a threshold value for the <see cref="Color32.A">Color32.A</see> field, under which a pixel is considered transparent.
        /// If 0, then the final composite image will not have transparent pixels. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="imageData"/> or <paramref name="stream"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. Use the <see cref="BeginEncodeHighColorImage">BeginEncodeHighColorImage</see>
        /// or <see cref="EncodeHighColorImageAsync">EncodeHighColorImageAsync</see> (in .NET Framework 4.0 and above) methods for asynchronous call and to set up cancellation or for reporting progress.</note>
        /// <note type="caution">This method produces a GIF image that may have compatibility issues. Though the <a href="https://docs.microsoft.com/en-us/dotnet/api/System.Drawing.Image" target="_blank">Image</a>
        /// and <a href="https://docs.microsoft.com/en-us/dotnet/api/System.Drawing.Bitmap" target="_blank">Bitmap</a> types (at least on Windows)
        /// support them as expected as well as applications built on GDI+ (such as Windows Paint), many decoders may treat the result as an animation (including browsers).</note>
        /// <para>If <paramref name="allowFullScan"/> is <see langword="true"/>, then both the processing time and memory usage is higher.
        /// It helps to minimize the number of layers of the final image; however, the final image size will not be necessarily smaller, especially for true color images.</para>
        /// <para>If <paramref name="allowFullScan"/> is <see langword="false"/>, then each layer is attempted to be as compact as possible. It allows a very fast processing with lower memory usage.
        /// Though it usually produces more layers, the final size will not be necessarily larger, especially for true color images.</para>
        /// <note type="tip">You can prequantize true color images using a 16-bit quantizer (with or without dithering) to produce fairly compact, still high color GIF images.
        /// For such images the <paramref name="allowFullScan"/> parameter with <see langword="true"/> value typically produces more compact results.
        /// You can consider using the <see cref="PredefinedColorsQuantizer.Argb1555(Color32,byte)">Argb1555</see> quantizer for images with transparency,
        /// or the <see cref="PredefinedColorsQuantizer.Rgb565(Color32,byte)">Rgb565</see> quantizer for non-transparent images.</note>
        /// <para>To encode an <a href="https://docs.microsoft.com/en-us/dotnet/api/System.Drawing.Image" target="_blank">Image</a> you can use also the <see cref="O:KGySoft.Drawing.ImageExtensions.SaveAsHighColorGif">ImageExtensions.SaveAsHighColorGif</see> methods.</para>
        /// <para>To create a multi-layered image completely manually you can create a <see cref="GifEncoder"/> instance that provides a lower level access.</para>
        /// </remarks>
        public static void EncodeHighColorImage(IReadableBitmapData imageData, Stream stream, bool allowFullScan = false, Color32 backColor = default, byte alphaThreshold = 128)
        {
            ValidateArguments(imageData, stream);
            DoEncodeHighColorImage(AsyncHelper.DefaultContext, imageData, stream, backColor, alphaThreshold, allowFullScan);
        }

        /// <summary>
        /// Begins to encode the specified <paramref name="imageData"/> as a multi-layered, single frame GIF image, writing it into the specified <paramref name="stream"/> and preserving its original color depth.
        /// </summary>
        /// <param name="imageData">The image data to write. Possible alpha pixels might be blended with <paramref name="backColor"/> but otherwise the color depth will be preserved.</param>
        /// <param name="stream">The stream to save the encoded animation into.</param>
        /// <param name="allowFullScan"><see langword="true"/> to allow scanning the whole image for each layers to be able to re-use the local palette of the current layer.
        /// <br/><see langword="false"/> to expand the initial layer area to the local pixels only. This parameter is optional.
        /// <br/>Default value: <see langword="false"/>.</param>
        /// <param name="backColor">Colors with alpha (transparency), whose <see cref="Color32.A">Color32.A</see> field
        /// is equal to or greater than <paramref name="alphaThreshold"/> will be blended with this color during the encoding.
        /// The alpha value (<see cref="Color32.A">Color32.A</see> field) of the specified background color is ignored. This parameter is optional.
        /// <br/>Default value: The default value of the <see cref="Color32"/> type, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="alphaThreshold">Specifies a threshold value for the <see cref="Color32.A">Color32.A</see> field, under which a pixel is considered transparent.
        /// If 0, then the final composite image will not have transparent pixels. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="imageData"/> or <paramref name="stream"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="EncodeHighColorImageAsync">EncodeHighColorImageAsync</see> method.</para>
        /// <para>To finish the operation and to get the exception that occurred during the operation you have to call the <see cref="EndEncodeHighColorImage">EndEncodeHighColorImage</see> method.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.
        /// The encoding itself cannot be parallelized. The <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> setting affects only some processing steps if the size of a layer exceeds a threshold.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="EncodeHighColorImage">EncodeHighColorImage</see> method for more details.</note>
        /// </remarks>
        public static IAsyncResult BeginEncodeHighColorImage(IReadableBitmapData imageData, Stream stream, bool allowFullScan = false, Color32 backColor = default, byte alphaThreshold = 128, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(imageData, stream);
            return AsyncHelper.BeginOperation(ctx => DoEncodeHighColorImage(ctx, imageData, stream, backColor, alphaThreshold, allowFullScan), asyncConfig);
        }

        /// <summary>
        /// Waits for the pending asynchronous operation started by the <see cref="BeginEncodeHighColorImage">BeginEncodeHighColorImage</see> method to complete.
        /// In .NET Framework 4.0 and above you can use the <see cref="EncodeHighColorImageAsync">EncodeHighColorImageAsync</see> method instead.
        /// </summary>
        /// <param name="asyncResult">The reference to the pending asynchronous request to finish.</param>
        public static void EndEncodeHighColorImage(IAsyncResult asyncResult) => AsyncHelper.EndOperation(asyncResult, nameof(BeginEncodeHighColorImage));

#if !NET35
        /// <summary>
        /// Encodes the specified <paramref name="imageData"/> as a multi-layered, single frame GIF image asynchronously, ans writes it into the specified <paramref name="stream"/>, preserving its original color depth.
        /// </summary>
        /// <param name="imageData">The image data to write. Possible alpha pixels might be blended with <paramref name="backColor"/> but otherwise the color depth will be preserved.</param>
        /// <param name="stream">The stream to save the encoded animation into.</param>
        /// <param name="allowFullScan"><see langword="true"/> to allow scanning the whole image for each layers to be able to re-use the local palette of the current layer.
        /// <br/><see langword="false"/> to expand the initial layer area to the local pixels only. This parameter is optional.
        /// <br/>Default value: <see langword="false"/>.</param>
        /// <param name="backColor">Colors with alpha (transparency), whose <see cref="Color32.A">Color32.A</see> field
        /// is equal to or greater than <paramref name="alphaThreshold"/> will be blended with this color during the encoding.
        /// The alpha value (<see cref="Color32.A">Color32.A</see> field) of the specified background color is ignored. This parameter is optional.
        /// <br/>Default value: The default value of the <see cref="Color32"/> type, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="alphaThreshold">Specifies a threshold value for the <see cref="Color32.A">Color32.A</see> field, under which a pixel is considered transparent.
        /// If 0, then the final composite image will not have transparent pixels. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="imageData"/> or <paramref name="stream"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.
        /// The encoding itself cannot be parallelized. The <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> setting affects only some processing steps if the size of a layer exceeds a threshold.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="EncodeHighColorImage">EncodeHighColorImage</see> method for more details.</note>
        /// </remarks>
        public static Task EncodeHighColorImageAsync(IReadableBitmapData imageData, Stream stream, bool allowFullScan = false, Color32 backColor = default, byte alphaThreshold = 128, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(imageData, stream);
            return AsyncHelper.DoOperationAsync(ctx => DoEncodeHighColorImage(ctx, imageData, stream, backColor, alphaThreshold, allowFullScan), asyncConfig);
        }
#endif

        #endregion

        #endregion

        #region Private Methods

        [SuppressMessage("ReSharper", "ParameterOnlyUsedForPreconditionCheck.Local", Justification = "That's why it's called Validate")]
        private static void ValidateArguments(AnimatedGifConfiguration configuration, Stream stream)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration), PublicResources.ArgumentNull);
            if (stream == null)
                throw new ArgumentNullException(nameof(stream), PublicResources.ArgumentNull);
            if (configuration.AnimationMode < AnimationMode.PingPong || (int)configuration.AnimationMode > UInt16.MaxValue)
                throw new ArgumentException(PublicResources.PropertyMessage(nameof(configuration.AnimationMode), PublicResources.ArgumentOutOfRange), nameof(configuration));
            if (!configuration.SizeHandling.IsDefined())
                throw new ArgumentException(PublicResources.PropertyMessage(nameof(configuration.SizeHandling), PublicResources.EnumOutOfRange(configuration.SizeHandling)), nameof(configuration));
        }

        [SuppressMessage("ReSharper", "ParameterOnlyUsedForPreconditionCheck.Local", Justification = "That's why it's called Validate")]
        private static void ValidateArguments(IReadableBitmapData imageData, Stream stream)
        {
            if (imageData == null)
                throw new ArgumentNullException(nameof(imageData), PublicResources.ArgumentNull);
            if (stream == null)
                throw new ArgumentNullException(nameof(stream), PublicResources.ArgumentNull);
            if (imageData.Width < 1 || imageData.Height < 1)
                throw new ArgumentException(Res.ImagingInvalidBitmapDataSize, nameof(imageData));
        }

        private static void DoEncodeImage(IAsyncContext context, IReadableBitmapData imageData, Stream stream, IQuantizer? quantizer, IDitherer? ditherer)
        {
            IReadableBitmapData? source = quantizer == null && imageData.PixelFormat.Indexed && !HasMultipleTransparentIndices(context, imageData)
                ? imageData
                : imageData.DoClone(context, KnownPixelFormat.Format8bppIndexed, quantizer
                    ?? (imageData.PixelFormat.Grayscale
                        ? PredefinedColorsQuantizer.Grayscale()
                        : OptimizedPaletteQuantizer.Wu()), ditherer);

            // cancel occurred
            if (source == null)
                return;

            try
            {
                context.Progress?.New(DrawingOperation.Saving);
                Palette palette = source.Palette!;
                new GifEncoder(stream, imageData.Size)
                    {
                        GlobalPalette = palette,
                        BackColorIndex = (byte)(palette.HasAlpha ? palette.TransparentIndex : 0),
#if DEBUG
                        AddMetaInfo = true,
#endif
                    }
                    .AddImage(source)
                    .FinalizeEncoding();
            }
            finally
            {
                if (!ReferenceEquals(source, imageData))
                    source.Dispose();
            }
        }

        private static void DoEncodeAnimation(IAsyncContext context, AnimatedGifConfiguration configuration, Stream stream)
        {
            using var enumerator = new FramesEnumerator(configuration, context);
            using GifEncoder? encoder = enumerator.CreateEncoder(stream);
            if (encoder == null)
                return;

            while (enumerator.MoveNext())
            {
                encoder.AddImage(enumerator.Frame!, enumerator.Location, enumerator.Delay, enumerator.DisposalMethod);
                enumerator.ReportProgress();
            }
        }

        private static void DoEncodeHighColorImage(IAsyncContext context, IReadableBitmapData imageData, Stream stream, Color32 backColor, byte alphaThreshold, bool fullScan)
        {
            // redirecting for an already indexed image
            if (imageData.PixelFormat.BitsPerPixel <= 8 && imageData.Palette != null)
            {
                DoEncodeImage(context, imageData, stream, PredefinedColorsQuantizer.FromCustomPalette(new Palette(imageData.Palette, backColor, alphaThreshold)), null);
                return;
            }

            using var enumerator = new LayersEnumerator(context, imageData, backColor, alphaThreshold, fullScan);
            using GifEncoder encoder = new GifEncoder(stream, imageData.Size)
            {
#if DEBUG
                AddMetaInfo = true
#endif
            };

            while (enumerator.MoveNext())
            {
                if (enumerator.Layer is IReadableBitmapData layer)
                    encoder.AddImage(layer, enumerator.Location);
            }
        }

        private static bool HasMultipleTransparentIndices(IAsyncContext context, IReadableBitmapData imageData)
        {
            Debug.Assert(imageData.PixelFormat.Indexed);
            Palette? palette = imageData.Palette;

            // There is no palette or it is too large: returning true to force a quantization
            if (palette == null || palette.Count > 256)
                return true;

            // no transparency: we are done
            if (!palette.HasAlpha)
                return false;

            // we need to check whether the palette has multiple transparent entries (or entries with partial transparency)
            bool multiAlpha = false;
            int transparentIndex = palette.TransparentIndex;
            for (int i = 0; i < palette.Count; i++)
            {
                if (palette[i].A < Byte.MaxValue && i != transparentIndex)
                {
                    multiAlpha = true;
                    break;
                }
            }

            // no multiple transparent entries
            if (!multiAlpha)
                return false;

            // we need to scan the image to check whether alpha pixels other than transparent index is in use
            int width = imageData.Width;

            // sequential processing
            if (width < parallelThreshold)
            {
                context.Progress?.New(DrawingOperation.ProcessingPixels, imageData.Height);
                IReadableBitmapDataRowMovable row = imageData.FirstRow;
                do
                {
                    if (context.IsCancellationRequested)
                        return false;
                    for (int x = 0; x < imageData.Width; x++)
                    {
                        int index = row.GetColorIndex(x);
                        if (index != transparentIndex && palette[index].A < Byte.MaxValue)
                            return true;
                    }

                    context.Progress?.Increment();
                } while (row.MoveNextRow());

                return false;
            }

            // parallel processing
            bool result = false;
            ParallelHelper.For(context, DrawingOperation.ProcessingPixels, 0, imageData.Height, y =>
            {
                if (Volatile.Read(ref result))
                    return;
                IReadableBitmapDataRow row = imageData[y];
                int w = width;
                int ti = transparentIndex;
                Color32[] paletteEntries = palette.Entries;
                for (int x = 0; x < w; x++)
                {
                    int index = row.GetColorIndex(x);
                    if (index != ti && paletteEntries[index].A < Byte.MaxValue)
                    {
                        Volatile.Write(ref result, true);
                        return;
                    }
                }
            });

            return result;
        }

        private static Rectangle GetContentArea(IReadableBitmapData imageData)
        {
            Rectangle result = new Rectangle(Point.Empty, imageData.Size);
            if (!imageData.HasAlpha())
                return result;

            IReadableBitmapDataRowMovable row = imageData.FirstRow;
            do
            {
                for (int x = 0; x < result.Width; x++)
                {
                    if (row[x].A != 0)
                        goto continueBottom;
                }

                result.Y += 1;
                result.Height -= 1;
            } while (row.MoveNextRow());

        continueBottom:
            // fully transparent image
            if (result.Height == 0)
                return Rectangle.Empty;

            for (int y = result.Bottom - 1; y >= result.Top; y--)
            {
                row.MoveToRow(y);
                for (int x = 0; x < result.Width; x++)
                {
                    if (row[x].A != 0)
                        goto continueLeft;
                }

                result.Height -= 1;
            }

        continueLeft:
            Debug.Assert(result.Height > 0);
            for (int x = 0; x < result.Width; x++)
            {
                for (int y = result.Top; y < result.Bottom; y++)
                {
                    if (imageData.GetColor32(x, y).A != 0)
                        goto continueRight;
                }

                result.X += 1;
                result.Width -= 1;
            }

        continueRight:
            Debug.Assert(result.Width > 0);
            for (int x = result.Right - 1; x >= result.Left; x--)
            {
                for (int y = result.Top; y < result.Bottom; y++)
                {
                    if (imageData.GetColor32(x, y).A != 0)
                        return result;
                }

                result.Width -= 1;
            }

            throw new InvalidOperationException(Res.InternalError("Empty result is not expected at this point"));
        }

        #endregion

        #endregion
    }
}