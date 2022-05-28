#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BitmapDataExtensions.ReadWrite.cs
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
using System.Drawing;
using System.Threading;
#if !NET35
using System.Threading.Tasks; 
#endif

using KGySoft.Collections;
using KGySoft.CoreLibraries;

#endregion

#if NET35
#pragma warning disable CS1574 // XML comment has cref attribute that could not be resolved - in .NET 3.5 not all members are available
#endif

namespace KGySoft.Drawing.Imaging
{
    partial class BitmapDataExtensions
    {
        #region Fields

        private static IThreadSafeCacheAccessor<float, byte[]>? gammaLookupTableCache;

        #endregion

        #region Properties

        private static IThreadSafeCacheAccessor<float, byte[]> GammaLookupTableCache
        {
            get
            {
                if (gammaLookupTableCache == null)
                {
                    var options = new LockFreeCacheOptions { InitialCapacity = 4, ThresholdCapacity = 16, HashingStrategy = HashingStrategy.Modulo, MergeInterval = TimeSpan.FromSeconds(1) };
                    Interlocked.CompareExchange(ref gammaLookupTableCache, ThreadSafeCacheFactory.Create<float, byte[]>(GenerateGammaLookupTable, options), null);
                }

                return gammaLookupTableCache;
            }
        }

        #endregion

        #region Methods

        #region Public Methods

        #region Clip

        /// <summary>
        /// Clips the specified <paramref name="source"/> using the specified <paramref name="clippingRegion"/>.
        /// Unlike the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.Clone">Clone</see> methods, this one returns a wrapper,
        /// providing access only to the specified region of the original <paramref name="source"/>.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="source">The source bitmap data to be clipped.</param>
        /// <param name="clippingRegion">A <see cref="Rectangle"/> that specifies a region within the <paramref name="source"/>.</param>
        /// <param name="disposeSource"><see langword="true"/>&#160;to dispose <paramref name="source"/> when the result is disposed; otherwise, <see langword="false"/>.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> that provides access only to the specified region withing the <paramref name="source"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="clippingRegion"/> has no overlapping region with source bounds.</exception>
        /// <remarks>
        /// <para>The <see cref="IBitmapData.RowSize"/> property of the returned instance can be 0, indicating that the <see cref="IReadableBitmapDataRow.ReadRaw{T}">ReadRaw</see>/<see cref="IWritableBitmapDataRow.WriteRaw{T}">WriteRaw</see>
        /// method cannot be used. It can occur if the left edge of the clipping is not zero.</para>
        /// <para>Even if <see cref="IBitmapData.RowSize"/> property of the returned instance is a nonzero value it can happen that it is too low to access all columns
        /// by the <see cref="IReadableBitmapDataRow.ReadRaw{T}">ReadRaw</see>/<see cref="IWritableBitmapDataRow.WriteRaw{T}">WriteRaw</see> methods. It can occur with indexed <see cref="IBitmapData.PixelFormat"/>s if the right edge of the clipping is not on byte boundary.</para>
        /// </remarks>
        public static IReadWriteBitmapData Clip(this IReadWriteBitmapData source, Rectangle clippingRegion, bool disposeSource)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source), PublicResources.ArgumentNull);
            return clippingRegion.Location.IsEmpty && clippingRegion.Size == source.GetSize()
                ? source
                : new ClippedBitmapData(source, clippingRegion, disposeSource);
        }

        /// <summary>
        /// Clips the specified <paramref name="source"/> using the specified <paramref name="clippingRegion"/>.
        /// Unlike the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.Clone">Clone</see> methods, this one returns a wrapper,
        /// providing access only to the specified region of the original <paramref name="source"/>.
        /// This overload does not dispose <paramref name="source"/> when the result is disposed.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="Clip(IReadWriteBitmapData,Rectangle,bool)"/> overload for details.
        /// </summary>
        /// <param name="source">The source bitmap data to be clipped.</param>
        /// <param name="clippingRegion">A <see cref="Rectangle"/> that specifies a region within the <paramref name="source"/>.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> that provides access only to the specified region withing the <paramref name="source"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="clippingRegion"/> has no overlapping region with source bounds.</exception>
        public static IReadWriteBitmapData Clip(this IReadWriteBitmapData source, Rectangle clippingRegion)
            => Clip(source, clippingRegion, false);

        #endregion

        #region Quantizing/Dithering

        /// <summary>
        /// Quantizes an <see cref="IReadWriteBitmapData"/> using the specified <paramref name="quantizer"/> (reduces the number of colors).
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="bitmapData">An <see cref="IReadWriteBitmapData"/> instance to be quantized.</param>
        /// <param name="quantizer">An <see cref="IQuantizer"/> implementation to be used for quantizing the specified <paramref name="bitmapData"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> or <paramref name="quantizer"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException">The <paramref name="quantizer"/>'s <see cref="IQuantizer.Initialize">Initialize</see> method returned <see langword="null"/>.</exception>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. Use the <see cref="BeginQuantize">BeginQuantize</see>
        /// or <see cref="QuantizeAsync">QuantizeAsync</see> (in .NET Framework 4.0 and above) methods for asynchronous call and to adjust parallelization, set up cancellation and for reporting progress.</note>
        /// <para>This method quantizes the specified <paramref name="bitmapData"/> in place (its original content will be overwritten). To return a new instance
        /// use the <see cref="Clone(IReadableBitmapData, KnownPixelFormat, IQuantizer, IDitherer)">Clone</see> extension method instead.</para>
        /// <para>If the <see cref="KnownPixelFormat"/> or the palette of <paramref name="bitmapData"/> is not compatible with the colors of the specified <paramref name="quantizer"/>, then
        /// the result may not be correct.</para>
        /// <para>If <paramref name="bitmapData"/> has already the same set of colors that the specified <paramref name="quantizer"/>, then it can happen
        /// that calling this method does not change the <paramref name="bitmapData"/> at all.</para>
        /// <note type="tip"><list type="bullet">
        /// <item>To use predefined colors or custom quantization functions use the static methods of the <see cref="PredefinedColorsQuantizer"/> class.
        /// <br/>See the <strong>Remarks</strong> section of its members for details and examples.</item>
        /// <item>To use an optimized palette of up to 256 colors adjusted for <paramref name="bitmapData"/> see the <see cref="OptimizedPaletteQuantizer"/> class.</item>
        /// </list></note>
        /// </remarks>
        public static void Quantize(this IReadWriteBitmapData bitmapData, IQuantizer quantizer) => bitmapData.Quantize(AsyncContext.Null, quantizer);

        public static void Quantize(this IReadWriteBitmapData bitmapData, IAsyncContext? context, IQuantizer quantizer)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            if (quantizer == null)
                throw new ArgumentNullException(nameof(quantizer), PublicResources.ArgumentNull);

            DoQuantize(context ?? AsyncContext.Null, bitmapData, quantizer);
        }

        /// <summary>
        /// Quantizes an <see cref="IReadWriteBitmapData"/> with dithering (reduces the number of colors while trying to preserve details)
        /// using the specified <paramref name="quantizer"/> and <paramref name="ditherer"/>.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="bitmapData">An <see cref="IReadWriteBitmapData"/> instance to be quantized.</param>
        /// <param name="quantizer">An <see cref="IQuantizer"/> implementation to be used for quantizing the specified <paramref name="bitmapData"/>.</param>
        /// <param name="ditherer">An <see cref="IDitherer"/> implementation to be used for dithering during the quantization of the specified <paramref name="bitmapData"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/>, <paramref name="quantizer"/> or <paramref name="ditherer"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException">The <see cref="IQuantizer.Initialize">IQuantizer.Initialize</see> method
        /// or the <see cref="IDitherer.Initialize">IDitherer.Initialize</see> method returned <see langword="null"/>.</exception>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. Use the <see cref="BeginDither">BeginDither</see>
        /// or <see cref="DitherAsync">DitherAsync</see> (in .NET Framework 4.0 and above) methods for asynchronous call and to adjust parallelization, set up cancellation and for reporting progress.</note>
        /// <para>This method quantizes <paramref name="bitmapData"/> with dithering in place (its original content will be overwritten). To return a new instance
        /// use the <see cref="Clone(IReadableBitmapData, KnownPixelFormat, IQuantizer, IDitherer)">Clone</see> extension method instead.</para>
        /// <para>If the <see cref="KnownPixelFormat"/> or the palette of <paramref name="bitmapData"/> is not compatible with the colors of the specified <paramref name="quantizer"/>, then
        /// the result may not be correct.</para>
        /// <para>If <paramref name="bitmapData"/> has already the same set of colors that the specified <paramref name="quantizer"/>, then it can happen
        /// that calling this method does not change <paramref name="bitmapData"/> at all.</para>
        /// <note type="tip"><list type="bullet">
        /// <item>To use predefined colors or custom quantization functions use the static methods of the <see cref="PredefinedColorsQuantizer"/> class.
        /// <br/>See the <strong>Remarks</strong> section of its members for details and examples.</item>
        /// <item>To use an optimized palette of up to 256 colors adjusted for <paramref name="bitmapData"/> see the <see cref="OptimizedPaletteQuantizer"/> class.</item>
        /// <item>For some built-in dithering solutions see the <see cref="OrderedDitherer"/>, <see cref="ErrorDiffusionDitherer"/>, <see cref="RandomNoiseDitherer"/>
        /// and <see cref="InterleavedGradientNoiseDitherer"/> classes. All of them have several examples in their <strong>Remarks</strong> section.</item>
        /// </list></note>
        /// </remarks>
        public static void Dither(this IReadWriteBitmapData bitmapData, IQuantizer quantizer, IDitherer ditherer) => bitmapData.Dither(AsyncContext.Null, quantizer, ditherer);

        public static void Dither(this IReadWriteBitmapData bitmapData, IAsyncContext? context, IQuantizer quantizer, IDitherer ditherer)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            if (quantizer == null)
                throw new ArgumentNullException(nameof(quantizer), PublicResources.ArgumentNull);
            if (ditherer == null)
                throw new ArgumentNullException(nameof(ditherer), PublicResources.ArgumentNull);

            DoDither(context ?? AsyncContext.Null, bitmapData, quantizer, ditherer);
        }

        /// <summary>
        /// Begins to quantize an <see cref="IReadWriteBitmapData"/> asynchronously, using the specified <paramref name="quantizer"/> (reduces the number of colors).
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="bitmapData">An <see cref="IReadWriteBitmapData"/> instance to be quantized.</param>
        /// <param name="quantizer">An <see cref="IQuantizer"/> implementation to be used for quantizing the specified <paramref name="bitmapData"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> or <paramref name="quantizer"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="QuantizeAsync">QuantizeAsync</see> method.</para>
        /// <para>To finish the operation and to get the exception that occurred during the operation you have to call the <see cref="EndQuantize">EndQuantize</see> method.</para>
        /// <para>This method is not a blocking call even if the <see cref="AsyncConfigBase.MaxDegreeOfParallelism"/> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="Quantize(IReadWriteBitmapData, IQuantizer)">Quantize</see> method for more details.</note>
        /// </remarks>
        public static IAsyncResult BeginQuantize(this IReadWriteBitmapData bitmapData, IQuantizer quantizer, AsyncConfig? asyncConfig = null)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            if (quantizer == null)
                throw new ArgumentNullException(nameof(quantizer), PublicResources.ArgumentNull);

            return AsyncContext.BeginOperation(ctx => DoQuantize(ctx, bitmapData, quantizer), asyncConfig);
        }

        /// <summary>
        /// Waits for the pending asynchronous operation started by the <see cref="BeginQuantize">BeginQuantize</see> method to complete.
        /// In .NET Framework 4.0 and above you can use the <see cref="QuantizeAsync">QuantizeAsync</see> method instead.
        /// </summary>
        /// <param name="asyncResult">The reference to the pending asynchronous request to finish.</param>
        /// <exception cref="InvalidOperationException">The quantizer's <see cref="IQuantizer.Initialize">Initialize</see> method returned <see langword="null"/>.</exception>
        public static void EndQuantize(this IAsyncResult asyncResult) => AsyncContext.EndOperation(asyncResult, nameof(BeginQuantize));

        /// <summary>
        /// Begins to quantize an <see cref="IReadWriteBitmapData"/> with dithering asynchronously (reduces the number of colors while trying to preserve details)
        /// using the specified <paramref name="quantizer"/> and <paramref name="ditherer"/>.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="bitmapData">An <see cref="IReadWriteBitmapData"/> instance to be quantized.</param>
        /// <param name="quantizer">An <see cref="IQuantizer"/> implementation to be used for quantizing the specified <paramref name="bitmapData"/>.</param>
        /// <param name="ditherer">An <see cref="IDitherer"/> implementation to be used for dithering during the quantization of the specified <paramref name="bitmapData"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/>, <paramref name="quantizer"/> or <paramref name="ditherer"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="DitherAsync">DitherAsync</see> method.</para>
        /// <para>To finish the operation and to get the exception that occurred during the operation you have to call the <see cref="EndQuantize">EndQuantize</see> method.</para>
        /// <para>This method is not a blocking call even if the <see cref="AsyncConfigBase.MaxDegreeOfParallelism"/> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="Dither(IReadWriteBitmapData, IQuantizer, IDitherer)">Dither</see> method for more details.</note>
        /// </remarks>
        public static IAsyncResult BeginDither(this IReadWriteBitmapData bitmapData, IQuantizer quantizer, IDitherer ditherer, AsyncConfig? asyncConfig = null)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            if (quantizer == null)
                throw new ArgumentNullException(nameof(quantizer), PublicResources.ArgumentNull);

            return AsyncContext.BeginOperation(ctx => DoDither(ctx, bitmapData, quantizer, ditherer), asyncConfig);
        }

        /// <summary>
        /// Waits for the pending asynchronous operation started by the <see cref="BeginDither">BeginDither</see> method to complete.
        /// In .NET Framework 4.0 and above you can use the <see cref="DitherAsync">QuantizeAsync</see> method instead.
        /// </summary>
        /// <param name="asyncResult">The reference to the pending asynchronous request to finish.</param>
        /// <exception cref="InvalidOperationException">The <see cref="IQuantizer.Initialize">IQuantizer.Initialize</see> method
        /// or the <see cref="IDitherer.Initialize">IDitherer.Initialize</see> method returned <see langword="null"/>.</exception>
        public static void EndDither(this IAsyncResult asyncResult) => AsyncContext.EndOperation(asyncResult, nameof(BeginDither));

#if !NET35
        /// <summary>
        /// Quantizes an <see cref="IReadWriteBitmapData"/> asynchronously, using the specified <paramref name="quantizer"/> (reduces the number of colors).
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="bitmapData">An <see cref="IReadWriteBitmapData"/> instance to be quantized.</param>
        /// <param name="quantizer">An <see cref="IQuantizer"/> implementation to be used for quantizing the specified <paramref name="bitmapData"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> or <paramref name="quantizer"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException">The <paramref name="quantizer"/>'s <see cref="IQuantizer.Initialize">Initialize</see> method returned <see langword="null"/>.</exception>
        /// <remarks>
        /// <para>This method is not a blocking call even if the <see cref="AsyncConfigBase.MaxDegreeOfParallelism"/> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="Quantize(IReadWriteBitmapData, IQuantizer)">Quantize</see> method for more details.</note>
        /// </remarks>
        public static Task QuantizeAsync(this IReadWriteBitmapData bitmapData, IQuantizer quantizer, TaskConfig? asyncConfig = null)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            if (quantizer == null)
                throw new ArgumentNullException(nameof(quantizer), PublicResources.ArgumentNull);

            return AsyncContext.DoOperationAsync(ctx => DoQuantize(ctx, bitmapData, quantizer), asyncConfig);
        }

        /// <summary>
        /// Quantizes an <see cref="IReadWriteBitmapData"/> with dithering asynchronously (reduces the number of colors while trying to preserve details)
        /// using the specified <paramref name="quantizer"/> and <paramref name="ditherer"/>.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="bitmapData">An <see cref="IReadWriteBitmapData"/> instance to be quantized.</param>
        /// <param name="quantizer">An <see cref="IQuantizer"/> implementation to be used for quantizing the specified <paramref name="bitmapData"/>.</param>
        /// <param name="ditherer">An <see cref="IDitherer"/> implementation to be used for dithering during the quantization of the specified <paramref name="bitmapData"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/>, <paramref name="quantizer"/> or <paramref name="ditherer"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException">The <see cref="IQuantizer.Initialize">IQuantizer.Initialize</see> method
        /// or the <see cref="IDitherer.Initialize">IDitherer.Initialize</see> method returned <see langword="null"/>.</exception>
        /// <remarks>
        /// <para>This method is not a blocking call even if the <see cref="AsyncConfigBase.MaxDegreeOfParallelism"/> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="Dither(IReadWriteBitmapData, IQuantizer, IDitherer)">Dither</see> method for more details.</note>
        /// </remarks>
        public static Task DitherAsync(this IReadWriteBitmapData bitmapData, IQuantizer quantizer, IDitherer ditherer, TaskConfig? asyncConfig = null)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            if (quantizer == null)
                throw new ArgumentNullException(nameof(quantizer), PublicResources.ArgumentNull);

            return AsyncContext.DoOperationAsync(ctx => DoDither(ctx, bitmapData, quantizer, ditherer), asyncConfig);
        }
#endif

        #endregion

        #region TransformColors

        /// <summary>
        /// Transforms the colors of this <paramref name="bitmapData"/> using the specified <paramref name="transformFunction"/> delegate.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to be transformed.</param>
        /// <param name="transformFunction">The transform function to be used on the colors of the specified <paramref name="bitmapData"/>. It must be thread-safe.</param>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> or <paramref name="transformFunction"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. Use the <see cref="BeginTransformColors">BeginTransformColors</see>
        /// or <see cref="TransformColorsAsync">TransformColorsAsync</see> (in .NET Framework 4.0 and above) methods for asynchronous call and to adjust parallelization, set up cancellation and for reporting progress.</note>
        /// <para>This method transforms the <paramref name="bitmapData"/> in place (its original content will be overwritten). To return a new instance
        /// use the <see cref="Clone(IReadableBitmapData,KnownPixelFormat,IQuantizer,IDitherer)">Clone</see> extension method
        /// with an <see cref="IQuantizer"/> instance created by the <see cref="PredefinedColorsQuantizer.FromCustomFunction(Func{Color32,Color32},KnownPixelFormat)">PredefinedColorsQuantizer.FromCustomFunction</see> method.</para>
        /// <para>If <paramref name="bitmapData"/> has an indexed <see cref="IBitmapData.PixelFormat"/> and it supports setting the <see cref="IBitmapData.Palette"/>, then its palette entries will be transformed instead of the actual pixels.</para>
        /// <para>On multi-core systems <paramref name="transformFunction"/> might be called concurrently so it must be thread-safe.</para>
        /// <note type="tip">If <paramref name="transformFunction"/> can return colors incompatible with the pixel format of the specified <paramref name="bitmapData"/>, or you want to transform the actual
        /// pixels of an indexed <paramref name="bitmapData"/> instead of modifying the palette, then use the <see cref="TransformColors(IReadWriteBitmapData,Func{Color32,Color32},IDitherer)"/> overload and specify an <see cref="IDitherer"/> instance.</note>
        /// </remarks>
        public static void TransformColors(this IReadWriteBitmapData bitmapData, Func<Color32, Color32> transformFunction)
        {
            ValidateArguments(bitmapData, transformFunction);
            DoTransformColors(AsyncContext.Null, bitmapData, transformFunction);
        }

        /// <summary>
        /// Transforms the colors of this <paramref name="bitmapData"/> using the specified <paramref name="transformFunction"/> delegate.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to be transformed.</param>
        /// <param name="transformFunction">The transform function to be used on the colors of the specified <paramref name="bitmapData"/>. It must be thread-safe.</param>
        /// <param name="ditherer">An optional <see cref="IDitherer"/> instance to dither the result of the transformation if <paramref name="transformFunction"/> returns colors
        /// that is not compatible with the <see cref="IBitmapData.PixelFormat"/> of the specified <paramref name="bitmapData"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> or <paramref name="transformFunction"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. Use the <see cref="BeginTransformColors">BeginTransformColors</see>
        /// or <see cref="TransformColorsAsync">TransformColorsAsync</see> (in .NET Framework 4.0 and above) methods for asynchronous call and to adjust parallelization, set up cancellation and for reporting progress.</note>
        /// <para>This method transforms the <paramref name="bitmapData"/> in place (its original content will be overwritten). To return a new instance
        /// use the <see cref="Clone(IReadableBitmapData,KnownPixelFormat,IQuantizer,IDitherer)">Clone</see> extension method
        /// with an <see cref="IQuantizer"/> instance created by the <see cref="PredefinedColorsQuantizer.FromCustomFunction(Func{Color32,Color32},KnownPixelFormat)">PredefinedColorsQuantizer.FromCustomFunction</see> method.</para>
        /// <para>If <paramref name="bitmapData"/> has an indexed <see cref="IBitmapData.PixelFormat"/> and <paramref name="ditherer"/> is <see langword="null"/>,
        /// then its palette entries are tried to be transformed instead of the actual pixels in the first place (if it is supported by <paramref name="bitmapData"/>).
        /// To transform the colors of an indexed <see cref="IBitmapData"/> without changing the palette specify a non-<see langword="null"/>&#160;<paramref name="ditherer"/>.
        /// Transforming the palette is both faster and provides a better result.</para>
        /// <para>On multi-core systems <paramref name="transformFunction"/> might be called concurrently so it must be thread-safe.</para>
        /// <para>The <paramref name="ditherer"/> is ignored for <see cref="KnownPixelFormat"/>s with more than 16 bits-per-pixel and for the <see cref="KnownPixelFormat.Format16bppGrayScale"/> format.</para>
        /// <note>See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/drawing/?topic=html/M_KGySoft_Drawing_BitmapExtensions_TransformColors.htm" target="_blank">BitmapExtensions.TransformColors</a> method for an example.</note>
        /// </remarks>
        public static void TransformColors(this IReadWriteBitmapData bitmapData, Func<Color32, Color32> transformFunction, IDitherer? ditherer)
            => bitmapData.TransformColors(AsyncContext.Null, transformFunction, ditherer);

        // TODO docs: The call is blocking on the caller thread but as it has a context parameter it makes possible to pass around an already created context from an async call.
        // Alternatively, it allows cancellation, configuring degree of parallelization and reporting progress even for a sync caller.
        // See the AsyncContext example for more details
        public static void TransformColors(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Func<Color32, Color32> transformFunction, IDitherer? ditherer)
        {
            ValidateArguments(bitmapData, transformFunction);
            DoTransformColors(context ?? AsyncContext.Null, bitmapData, transformFunction, ditherer);
        }

        /// <summary>
        /// Begins to transform the colors of this <paramref name="bitmapData"/> asynchronously, using the specified <paramref name="transformFunction"/> delegate.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to be transformed.</param>
        /// <param name="transformFunction">The transform function to be used on the colors of the specified <paramref name="bitmapData"/>. It must be thread-safe.</param>
        /// <param name="ditherer">An optional <see cref="IDitherer"/> instance to dither the result of the transformation if <paramref name="transformFunction"/> returns colors
        /// that is not compatible with the <see cref="IBitmapData.PixelFormat"/> of the specified <paramref name="bitmapData"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> or <paramref name="transformFunction"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="TransformColorsAsync">TransformColorsAsync</see> method.</para>
        /// <para>To finish the operation and to get the exception that occurred during the operation you have to call the <see cref="EndTransformColors">EndTransformColors</see> method.</para>
        /// <para>This method is not a blocking call even if the <see cref="AsyncConfigBase.MaxDegreeOfParallelism"/> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="TransformColors(IReadWriteBitmapData, Func{Color32, Color32}, IDitherer)"/> method for more details.</note>
        /// </remarks>
        public static IAsyncResult BeginTransformColors(this IReadWriteBitmapData bitmapData, Func<Color32, Color32> transformFunction, IDitherer? ditherer = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, transformFunction);
            return AsyncContext.BeginOperation(ctx => DoTransformColors(ctx, bitmapData, transformFunction, ditherer), asyncConfig);
        }

        /// <summary>
        /// Waits for the pending asynchronous operation started by the <see cref="BeginTransformColors">BeginTransformColors</see> method to complete.
        /// In .NET Framework 4.0 and above you can use the <see cref="TransformColorsAsync">TransformColorsAsync</see> method instead.
        /// </summary>
        /// <param name="asyncResult">The reference to the pending asynchronous request to finish.</param>
        public static void EndTransformColors(this IAsyncResult asyncResult) => AsyncContext.EndOperation(asyncResult, nameof(BeginTransformColors));

#if !NET35
        /// <summary>
        /// Transforms the colors of this <paramref name="bitmapData"/> asynchronously, using the specified <paramref name="transformFunction"/> delegate.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to be transformed.</param>
        /// <param name="transformFunction">The transform function to be used on the colors of the specified <paramref name="bitmapData"/>. It must be thread-safe.</param>
        /// <param name="ditherer">An optional <see cref="IDitherer"/> instance to dither the result of the transformation if <paramref name="transformFunction"/> returns colors
        /// that is not compatible with the <see cref="IBitmapData.PixelFormat"/> of the specified <paramref name="bitmapData"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> or <paramref name="transformFunction"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// <para>This method is not a blocking call even if the <see cref="AsyncConfigBase.MaxDegreeOfParallelism"/> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="TransformColors(IReadWriteBitmapData, Func{Color32, Color32}, IDitherer)"/> method for more details.</note>
        /// </remarks>
        public static Task TransformColorsAsync(this IReadWriteBitmapData bitmapData, Func<Color32, Color32> transformFunction, IDitherer? ditherer = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, transformFunction);
            return AsyncContext.DoOperationAsync(ctx => DoTransformColors(ctx, bitmapData, transformFunction, ditherer), asyncConfig);
        }
#endif

        #endregion

        #region ReplaceColor

        /// <summary>
        /// Replaces every <paramref name="oldColor"/> occurrences to <paramref name="newColor"/> in the specified <paramref name="bitmapData"/>.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to be transformed.</param>
        /// <param name="oldColor">The original color to be replaced.</param>
        /// <param name="newColor">The new color to replace <paramref name="oldColor"/> with.</param>
        /// <param name="ditherer">An optional <see cref="IDitherer"/> instance to dither the result if <paramref name="newColor"/>
        /// is not compatible with the <see cref="IBitmapData.PixelFormat"/> of the specified <paramref name="bitmapData"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. Use the <see cref="BeginReplaceColor">BeginReplaceColor</see>
        /// or <see cref="ReplaceColorAsync">ReplaceColorAsync</see> (in .NET Framework 4.0 and above) methods for asynchronous call and to adjust parallelization, set up cancellation and for reporting progress.</note>
        /// <para>If <paramref name="newColor"/> has alpha, which cannot be represented by <paramref name="bitmapData"/>, then it will be blended with <see cref="IBitmapData.BackColor"/>.</para>
        /// <para>This method calls the <see cref="TransformColors(IReadWriteBitmapData, Func{Color32, Color32}, IDitherer)">TransformColors</see> method internally. See
        /// the <strong>Remarks</strong> section of the <see cref="TransformColors(IReadWriteBitmapData, Func{Color32, Color32}, IDitherer)">TransformColors</see> method for more details.</para>
        /// <para>If <paramref name="bitmapData"/> has an indexed <see cref="IBitmapData.PixelFormat"/> and <paramref name="ditherer"/> is <see langword="null"/>,
        /// then its palette entries are tried to be transformed instead of the actual pixels in the first place (if it is supported by <paramref name="bitmapData"/>).
        /// To transform the colors of an indexed <see cref="IBitmapData"/> without changing the palette specify a non-<see langword="null"/>&#160;<paramref name="ditherer"/>.
        /// Transforming the palette is both faster and provides a better result.</para>
        /// <para>The <paramref name="ditherer"/> is ignored for <see cref="KnownPixelFormat"/>s with more than 16 bits-per-pixel and for the <see cref="KnownPixelFormat.Format16bppGrayScale"/> format.</para>
        /// </remarks>
        public static void ReplaceColor(this IReadWriteBitmapData bitmapData, Color32 oldColor, Color32 newColor, IDitherer? ditherer = null)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            if (oldColor == newColor)
                return;

            DoTransformColors(AsyncContext.Null, bitmapData, c => TransformReplaceColor(c, oldColor, newColor), ditherer);
        }

        /// <summary>
        /// Begins to replace every <paramref name="oldColor"/> occurrences to <paramref name="newColor"/> in the specified <paramref name="bitmapData"/> asynchronously.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to be transformed.</param>
        /// <param name="oldColor">The original color to be replaced.</param>
        /// <param name="newColor">The new color to replace <paramref name="oldColor"/> with.</param>
        /// <param name="ditherer">An optional <see cref="IDitherer"/> instance to dither the result if <paramref name="newColor"/>
        /// is not compatible with the <see cref="IBitmapData.PixelFormat"/> of the specified <paramref name="bitmapData"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="ReplaceColorAsync">ReplaceColorAsync</see> method.</para>
        /// <para>To finish the operation and to get the exception that occurred during the operation you have to call the <see cref="EndReplaceColor">EndReplaceColor</see> method.</para>
        /// <para>This method is not a blocking call even if the <see cref="AsyncConfigBase.MaxDegreeOfParallelism"/> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="ReplaceColor">ReplaceColor</see> method for more details.</note>
        /// </remarks>
        public static IAsyncResult BeginReplaceColor(this IReadWriteBitmapData bitmapData, Color32 oldColor, Color32 newColor, IDitherer? ditherer = null, AsyncConfig? asyncConfig = null)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            if (oldColor == newColor)
                return AsyncContext.FromCompleted(asyncConfig);

            return AsyncContext.BeginOperation(ctx => DoTransformColors(ctx, bitmapData, c => TransformReplaceColor(c, oldColor, newColor), ditherer), asyncConfig);
        }

        /// <summary>
        /// Waits for the pending asynchronous operation started by the <see cref="BeginReplaceColor">BeginReplaceColor</see> method to complete.
        /// In .NET Framework 4.0 and above you can use the <see cref="ReplaceColorAsync">ReplaceColorAsync</see> method instead.
        /// </summary>
        /// <param name="asyncResult">The reference to the pending asynchronous request to finish.</param>
        public static void EndReplaceColor(this IAsyncResult asyncResult) => AsyncContext.EndOperation(asyncResult, nameof(BeginReplaceColor));

#if !NET35
        /// <summary>
        /// Replaces every <paramref name="oldColor"/> occurrences to <paramref name="newColor"/> in the specified <paramref name="bitmapData"/> asynchronously.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to be transformed.</param>
        /// <param name="oldColor">The original color to be replaced.</param>
        /// <param name="newColor">The new color to replace <paramref name="oldColor"/> with.</param>
        /// <param name="ditherer">An optional <see cref="IDitherer"/> instance to dither the result if <paramref name="newColor"/>
        /// is not compatible with the <see cref="IBitmapData.PixelFormat"/> of the specified <paramref name="bitmapData"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>This method is not a blocking call even if the <see cref="AsyncConfigBase.MaxDegreeOfParallelism"/> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="ReplaceColor">ReplaceColor</see> method for more details.</note>
        /// </remarks>
        public static Task ReplaceColorAsync(this IReadWriteBitmapData bitmapData, Color32 oldColor, Color32 newColor, IDitherer? ditherer = null, TaskConfig? asyncConfig = null)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            if (oldColor == newColor)
                return AsyncContext.FromCompleted(asyncConfig);

            return AsyncContext.DoOperationAsync(ctx => DoTransformColors(ctx, bitmapData, c => TransformReplaceColor(c, oldColor, newColor), ditherer), asyncConfig);
        }
#endif

        #endregion

        #region Invert

        /// <summary>
        /// Inverts the colors of the specified <paramref name="bitmapData"/>.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to be inverted.</param>
        /// <param name="ditherer">An optional <see cref="IDitherer"/> instance to dither the result of the transformation if the inverse of the <paramref name="bitmapData"/>
        /// has no exact representation with its <see cref="IBitmapData.PixelFormat"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. Use the <see cref="BeginInvert">BeginInvert</see>
        /// or <see cref="InvertAsync">InvertAsync</see> (in .NET Framework 4.0 and above) methods for asynchronous call and to adjust parallelization, set up cancellation and for reporting progress.</note>
        /// <para>This method calls the <see cref="TransformColors(IReadWriteBitmapData, Func{Color32, Color32}, IDitherer)">TransformColors</see> method internally. See
        /// the <strong>Remarks</strong> section of the <see cref="TransformColors(IReadWriteBitmapData, Func{Color32, Color32}, IDitherer)">TransformColors</see> method for more details.</para>
        /// <para>If <paramref name="bitmapData"/> has an indexed <see cref="IBitmapData.PixelFormat"/> and <paramref name="ditherer"/> is <see langword="null"/>,
        /// then its palette entries are tried to be transformed instead of the actual pixels in the first place (if it is supported by <paramref name="bitmapData"/>).
        /// To transform the colors of an indexed <see cref="IBitmapData"/> without changing the palette specify a non-<see langword="null"/>&#160;<paramref name="ditherer"/>.
        /// Transforming the palette is both faster and provides a better result.</para>
        /// <para>The <paramref name="ditherer"/> is ignored for <see cref="KnownPixelFormat"/>s with more than 16 bits-per-pixel and for the <see cref="KnownPixelFormat.Format16bppGrayScale"/> format.</para>
        /// </remarks>
        public static void Invert(this IReadWriteBitmapData bitmapData, IDitherer? ditherer = null)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            DoTransformColors(AsyncContext.Null, bitmapData, TransformInvert, ditherer);
        }

        /// <summary>
        /// Begins to Invert the colors of the specified <paramref name="bitmapData"/> asynchronously.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to be inverted.</param>
        /// <param name="ditherer">An optional <see cref="IDitherer"/> instance to dither the result of the transformation if the inverse of the <paramref name="bitmapData"/>
        /// has no exact representation with its <see cref="IBitmapData.PixelFormat"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="InvertAsync">InvertAsync</see> method.</para>
        /// <para>To finish the operation and to get the exception that occurred during the operation you have to call the <see cref="EndInvert">EndInvert</see> method.</para>
        /// <para>This method is not a blocking call even if the <see cref="AsyncConfigBase.MaxDegreeOfParallelism"/> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="Invert">Invert</see> method for more details.</note>
        /// </remarks>
        public static IAsyncResult BeginInvert(this IReadWriteBitmapData bitmapData, IDitherer? ditherer = null, AsyncConfig? asyncConfig = null)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            return AsyncContext.BeginOperation(ctx => DoTransformColors(ctx, bitmapData, TransformInvert, ditherer), asyncConfig);
        }

        /// <summary>
        /// Waits for the pending asynchronous operation started by the <see cref="BeginInvert">BeginInvert</see> method to complete.
        /// In .NET Framework 4.0 and above you can use the <see cref="InvertAsync">InvertAsync</see> method instead.
        /// </summary>
        /// <param name="asyncResult">The reference to the pending asynchronous request to finish.</param>
        public static void EndInvert(this IAsyncResult asyncResult) => AsyncContext.EndOperation(asyncResult, nameof(BeginInvert));

#if !NET35
        /// <summary>
        /// Inverts the colors of the specified <paramref name="bitmapData"/> asynchronously.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to be inverted.</param>
        /// <param name="ditherer">An optional <see cref="IDitherer"/> instance to dither the result of the transformation if the inverse of the <paramref name="bitmapData"/>
        /// has no exact representation with its <see cref="IBitmapData.PixelFormat"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>This method is not a blocking call even if the <see cref="AsyncConfigBase.MaxDegreeOfParallelism"/> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="Invert">Invert</see> method for more details.</note>
        /// </remarks>
        public static Task InvertAsync(this IReadWriteBitmapData bitmapData, IDitherer? ditherer = null, TaskConfig? asyncConfig = null)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            return AsyncContext.DoOperationAsync(ctx => DoTransformColors(ctx, bitmapData, TransformInvert, ditherer), asyncConfig);
        }
#endif

        #endregion

        #region MakeTransparent

        /// <summary>
        /// If possible, makes the background of this <paramref name="bitmapData"/> transparent, taking the bottom-left pixel as the background color.
        /// If the <paramref name="bitmapData"/> does not support transparency, then the pixels that have the same color as the bottom-left pixel will be set
        /// to the <see cref="IBitmapData.BackColor"/> of <paramref name="bitmapData"/>.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to make transparent.</param>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. Use the <see cref="BeginMakeTransparent(IReadWriteBitmapData, AsyncConfig)"/>
        /// or <see cref="MakeTransparentAsync(IReadWriteBitmapData, TaskConfig)"/> (in .NET Framework 4.0 and above) methods for asynchronous call and to adjust parallelization, set up cancellation and for reporting progress.</note>
        /// <para>This method calls the <see cref="ReplaceColor">ReplaceColor</see> method internally.</para>
        /// <para>Similarly to the <a href="https://docs.microsoft.com/en-us/dotnet/api/system.drawing.bitmap.maketransparent" target="_blank">Bitmap.MakeTransparent</a> method,
        /// this one uses the bottom-left pixel to determine the background color, which must be completely opaque; otherwise, <paramref name="bitmapData"/> will not be changed.</para>
        /// <para>Unlike the <a href="https://docs.microsoft.com/en-us/dotnet/api/system.drawing.bitmap.maketransparent" target="_blank">Bitmap.MakeTransparent</a> method,
        /// this one preserves the original <see cref="IBitmapData.PixelFormat"/>. If <paramref name="bitmapData"/> does not support transparency and cannot set <see cref="IBitmapData.Palette"/>
        /// either, then every occurrence of the color of the bottom-left pixel will be changed to the <see cref="IBitmapData.BackColor"/> of <paramref name="bitmapData"/>.
        /// To make such bitmaps transparent use the <see cref="ToTransparent(IReadableBitmapData)">ToTransparent</see> method instead,
        /// which returns a new instance with <see cref="KnownPixelFormat.Format32bppArgb"/>&#160;<see cref="IBitmapData.PixelFormat"/>.</para>
        /// <para>To force replacing even non-completely opaque pixels use the <see cref="MakeTransparent(IReadWriteBitmapData, Color32)"/> overload instead.</para>
        /// <note>Please note that unlike the <see cref="MakeOpaque">MakeOpaque</see> method, this one changes exactly one color shade without any tolerance.
        /// For any customization use the <see cref="TransformColors(IReadWriteBitmapData,Func{Color32,Color32})">TransformColors</see> method instead.</note>
        /// </remarks>
        /// <seealso cref="ToTransparent(IReadableBitmapData)"/>
        /// <seealso cref="MakeOpaque"/>
        public static void MakeTransparent(this IReadWriteBitmapData bitmapData)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            if (bitmapData.Width < 1 || bitmapData.Height < 1)
                return;
            Color32 transparentColor = bitmapData[bitmapData.Height - 1][0];
            if (transparentColor.A < Byte.MaxValue)
                return;
            DoTransformColors(AsyncContext.Null, bitmapData, c => TransformReplaceColor(c, transparentColor, default));
        }

        /// <summary>
        /// If possible, makes the background of this <paramref name="bitmapData"/> transparent, using <paramref name="transparentColor"/> as the background color.
        /// If the <paramref name="bitmapData"/> does not support transparency, then the pixels that have the same color as <paramref name="transparentColor"/> will be set
        /// to the <see cref="IBitmapData.BackColor"/> of <paramref name="bitmapData"/>.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to make transparent.</param>
        /// <param name="transparentColor">Specifies the color to make transparent.</param>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. Use the <see cref="BeginMakeTransparent(IReadWriteBitmapData, Color32, AsyncConfig)"/>
        /// or <see cref="MakeTransparentAsync(IReadWriteBitmapData, Color32, TaskConfig)"/> (in .NET Framework 4.0 and above) methods for asynchronous call and to adjust parallelization, set up cancellation and for reporting progress.</note>
        /// <para>This method calls the <see cref="ReplaceColor">ReplaceColor</see> method internally.</para>
        /// <para>Unlike the <a href="https://docs.microsoft.com/en-us/dotnet/api/system.drawing.bitmap.maketransparent" target="_blank">Bitmap.MakeTransparent(Color)</a> method,
        /// this one preserves the original <see cref="IBitmapData.PixelFormat"/>. If <paramref name="bitmapData"/> does not support transparency and cannot set <see cref="IBitmapData.Palette"/> either,
        /// then every occurrence of the <paramref name="transparentColor"/> will be changed to the <see cref="IBitmapData.BackColor"/> of <paramref name="bitmapData"/>.
        /// To make such bitmaps transparent use the <see cref="ToTransparent(IReadableBitmapData,Color32)">ToTransparent</see> method instead,
        /// which returns a new instance with <see cref="KnownPixelFormat.Format32bppArgb"/>&#160;<see cref="IBitmapData.PixelFormat"/>.</para>
        /// <para>To auto-detect the background color to be made transparent use the <see cref="MakeTransparent(IReadWriteBitmapData)"/> overload instead.</para>
        /// <note>Please note that unlike the <see cref="MakeOpaque">MakeOpaque</see> method, this one changes exactly one color shade without any tolerance.
        /// For any customization use the <see cref="TransformColors(IReadWriteBitmapData,Func{Color32,Color32})">TransformColors</see> method instead.</note>
        /// </remarks>
        /// <seealso cref="ToTransparent(IReadableBitmapData,Color32)"/>
        /// <seealso cref="MakeOpaque"/>
        public static void MakeTransparent(this IReadWriteBitmapData bitmapData, Color32 transparentColor)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            if (transparentColor.A == 0)
                return;
            DoTransformColors(AsyncContext.Null, bitmapData, c => TransformReplaceColor(c, transparentColor, default));
        }

        /// <summary>
        /// If possible, begins to make the background of this <paramref name="bitmapData"/> transparent asynchronously, taking the bottom-left pixel as the background color.
        /// If the <paramref name="bitmapData"/> does not support transparency, then the pixels that have the same color as the bottom-left pixel will be set
        /// to the <see cref="IBitmapData.BackColor"/> of <paramref name="bitmapData"/>.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to make transparent.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="MakeTransparentAsync(IReadWriteBitmapData, TaskConfig)"/> method.</para>
        /// <para>To finish the operation and to get the exception that occurred during the operation you have to call the <see cref="EndMakeTransparent">EndMakeTransparent</see> method.</para>
        /// <para>This method is not a blocking call even if the <see cref="AsyncConfigBase.MaxDegreeOfParallelism"/> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="MakeTransparent(IReadWriteBitmapData)"/> method for more details.</note>
        /// </remarks>
        /// <seealso cref="BeginToTransparent(IReadableBitmapData, AsyncConfig)"/>
        /// <seealso cref="BeginMakeOpaque"/>
        public static IAsyncResult BeginMakeTransparent(this IReadWriteBitmapData bitmapData, AsyncConfig? asyncConfig = null)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            if (bitmapData.Width < 1 || bitmapData.Height < 1)
                return AsyncContext.FromCompleted(asyncConfig);
            Color32 transparentColor = bitmapData[bitmapData.Height - 1][0];
            if (transparentColor.A < Byte.MaxValue)
                return AsyncContext.FromCompleted(asyncConfig);
            return AsyncContext.BeginOperation(ctx => DoTransformColors(ctx, bitmapData, c => TransformReplaceColor(c, transparentColor, default)), asyncConfig);
        }

        /// <summary>
        /// If possible, begins to make the background of this <paramref name="bitmapData"/> transparent asynchronously, using <paramref name="transparentColor"/> as the background color.
        /// If the <paramref name="bitmapData"/> does not support transparency, then the pixels that have the same color as <paramref name="transparentColor"/> will be set
        /// to the <see cref="IBitmapData.BackColor"/> of <paramref name="bitmapData"/>.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to make transparent.</param>
        /// <param name="transparentColor">Specifies the color to make transparent.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="MakeTransparentAsync(IReadWriteBitmapData,Color32,TaskConfig)"/> method.</para>
        /// <para>To finish the operation and to get the exception that occurred during the operation you have to call the <see cref="EndMakeTransparent">EndMakeTransparent</see> method.</para>
        /// <para>This method is not a blocking call even if the <see cref="AsyncConfigBase.MaxDegreeOfParallelism"/> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="MakeTransparent(IReadWriteBitmapData, Color32)"/> method for more details.</note>
        /// </remarks>
        /// <seealso cref="BeginToTransparent(IReadableBitmapData,Color32,AsyncConfig)"/>
        /// <seealso cref="BeginMakeOpaque"/>
        public static IAsyncResult BeginMakeTransparent(this IReadWriteBitmapData bitmapData, Color32 transparentColor, AsyncConfig? asyncConfig = null)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            if (transparentColor.A == 0)
                return AsyncContext.FromCompleted(asyncConfig);
            return AsyncContext.BeginOperation(ctx => DoTransformColors(ctx, bitmapData, c => TransformReplaceColor(c, transparentColor, default)), asyncConfig);
        }

        /// <summary>
        /// Waits for the pending asynchronous operation started by the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.BeginMakeTransparent">BeginMakeTransparent</see> methods to complete.
        /// In .NET Framework 4.0 and above you can use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.MakeTransparentAsync">MakeTransparentAsync</see> methods instead.
        /// </summary>
        /// <param name="asyncResult">The reference to the pending asynchronous request to finish.</param>
        public static void EndMakeTransparent(this IAsyncResult asyncResult) => AsyncContext.EndOperation(asyncResult, nameof(BeginMakeTransparent));

#if !NET35
        /// <summary>
        /// If possible, makes the background of this <paramref name="bitmapData"/> transparent asynchronously, taking the bottom-left pixel as the background color.
        /// If the <paramref name="bitmapData"/> does not support transparency, then the pixels that have the same color as the bottom-left pixel will be set
        /// to the <see cref="IBitmapData.BackColor"/> of <paramref name="bitmapData"/>.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to make transparent.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>This method is not a blocking call even if the <see cref="AsyncConfigBase.MaxDegreeOfParallelism"/> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="MakeTransparent(IReadWriteBitmapData)"/> method for more details.</note>
        /// </remarks>
        /// <seealso cref="ToTransparentAsync(IReadableBitmapData, TaskConfig)"/>
        /// <seealso cref="MakeOpaqueAsync"/>
        public static Task MakeTransparentAsync(this IReadWriteBitmapData bitmapData, TaskConfig? asyncConfig = null)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            if (bitmapData.Width < 1 || bitmapData.Height < 1)
                return AsyncContext.FromCompleted(asyncConfig);
            Color32 transparentColor = bitmapData[bitmapData.Height - 1][0];
            if (transparentColor.A < Byte.MaxValue)
                return AsyncContext.FromCompleted(asyncConfig);
            return AsyncContext.DoOperationAsync(ctx => DoTransformColors(ctx, bitmapData, c => TransformReplaceColor(c, transparentColor, default)), asyncConfig);
        }

        /// <summary>
        /// If possible, makes the background of this <paramref name="bitmapData"/> transparent asynchronously, using <paramref name="transparentColor"/> as the background color.
        /// If the <paramref name="bitmapData"/> does not support transparency, then the pixels that have the same color as <paramref name="transparentColor"/> will be set
        /// to the <see cref="IBitmapData.BackColor"/> of <paramref name="bitmapData"/>.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to make transparent.</param>
        /// <param name="transparentColor">Specifies the color to make transparent.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>This method is not a blocking call even if the <see cref="AsyncConfigBase.MaxDegreeOfParallelism"/> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="MakeTransparent(IReadWriteBitmapData, Color32)"/> method for more details.</note>
        /// </remarks>
        /// <seealso cref="ToTransparentAsync(IReadableBitmapData,Color32,TaskConfig)"/>
        /// <seealso cref="MakeOpaqueAsync"/>
        public static Task MakeTransparentAsync(this IReadWriteBitmapData bitmapData, Color32 transparentColor, TaskConfig? asyncConfig = null)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            if (transparentColor.A == 0)
                return AsyncContext.FromCompleted(asyncConfig);
            return AsyncContext.DoOperationAsync(ctx => DoTransformColors(ctx, bitmapData, c => TransformReplaceColor(c, transparentColor, default)), asyncConfig);
        }
#endif

        #endregion

        #region MakeOpaque

        /// <summary>
        /// Makes this <paramref name="bitmapData"/> opaque using the specified <paramref name="backColor"/>.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to make opaque.</param>
        /// <param name="backColor">Pixels with alpha in <paramref name="bitmapData"/> will be blended with this color.
        /// The <see cref="Color32.A">Color32.A</see> property of the specified color is ignored.</param>
        /// <param name="ditherer">An optional <see cref="IDitherer"/> instance to dither the result of the transformation if the inverse of the <paramref name="bitmapData"/>
        /// has no exact representation with its <see cref="IBitmapData.PixelFormat"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. Use the <see cref="BeginMakeOpaque">BeginMakeOpaque</see>
        /// or <see cref="MakeOpaqueAsync">MakeOpaqueAsync</see> (in .NET Framework 4.0 and above) methods for asynchronous call and to adjust parallelization, set up cancellation and for reporting progress.</note>
        /// <para>This method calls the <see cref="TransformColors(IReadWriteBitmapData, Func{Color32, Color32}, IDitherer)">TransformColors</see> method internally. See
        /// the <strong>Remarks</strong> section of the <see cref="TransformColors(IReadWriteBitmapData, Func{Color32, Color32}, IDitherer)">TransformColors</see> method for more details.</para>
        /// <para>If <paramref name="bitmapData"/> has an indexed <see cref="IBitmapData.PixelFormat"/> and <paramref name="ditherer"/> is <see langword="null"/>,
        /// then its palette entries are tried to be transformed instead of the actual pixels in the first place (if it is supported by <paramref name="bitmapData"/>).
        /// To transform the colors of an indexed <see cref="IBitmapData"/> without changing the palette specify a non-<see langword="null"/>&#160;<paramref name="ditherer"/>.
        /// Transforming the palette is both faster and provides a better result.</para>
        /// <para>The <paramref name="ditherer"/> is ignored for <see cref="KnownPixelFormat"/>s with more than 16 bits-per-pixel and for the <see cref="KnownPixelFormat.Format16bppGrayScale"/> format.</para>
        /// </remarks>
        public static void MakeOpaque(this IReadWriteBitmapData bitmapData, Color32 backColor, IDitherer? ditherer = null)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            if (!bitmapData.HasAlpha())
                return;
            DoTransformColors(AsyncContext.Null, bitmapData, c => TransformMakeOpaque(c, backColor), ditherer);
        }

        /// <summary>
        /// Begins to make this <paramref name="bitmapData"/> opaque asynchronously using the specified <paramref name="backColor"/>.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to make opaque.</param>
        /// <param name="backColor">Pixels with alpha in <paramref name="bitmapData"/> will be blended with this color.
        /// The <see cref="Color32.A">Color32.A</see> property of the specified color is ignored.</param>
        /// <param name="ditherer">An optional <see cref="IDitherer"/> instance to dither the result of the transformation if the inverse of the <paramref name="bitmapData"/>
        /// has no exact representation with its <see cref="IBitmapData.PixelFormat"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="MakeOpaqueAsync">MakeOpaqueAsync</see> method.</para>
        /// <para>To finish the operation and to get the exception that occurred during the operation you have to call the <see cref="EndMakeOpaque">EndMakeOpaque</see> method.</para>
        /// <para>This method is not a blocking call even if the <see cref="AsyncConfigBase.MaxDegreeOfParallelism"/> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="MakeOpaque">MakeOpaque</see> method for more details.</note>
        /// <remarks>
        /// </remarks>
        public static IAsyncResult BeginMakeOpaque(this IReadWriteBitmapData bitmapData, Color32 backColor, IDitherer? ditherer = null, AsyncConfig? asyncConfig = null)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            return AsyncContext.BeginOperation(ctx => DoTransformColors(ctx, bitmapData, c => TransformMakeOpaque(c, backColor), ditherer), asyncConfig);
        }

        /// <summary>
        /// Waits for the pending asynchronous operation started by the <see cref="BeginMakeOpaque">BeginMakeOpaque</see> method to complete.
        /// In .NET Framework 4.0 and above you can use the <see cref="MakeOpaqueAsync">MakeOpaqueAsync</see> method instead.
        /// </summary>
        /// <param name="asyncResult">The reference to the pending asynchronous request to finish.</param>
        public static void EndMakeOpaque(this IAsyncResult asyncResult) => AsyncContext.EndOperation(asyncResult, nameof(BeginMakeOpaque));

#if !NET35
        /// <summary>
        /// Makes this <paramref name="bitmapData"/> opaque asynchronously using the specified <paramref name="backColor"/>.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to make opaque.</param>
        /// <param name="backColor">Pixels with alpha in <paramref name="bitmapData"/> will be blended with this color.
        /// The <see cref="Color32.A">Color32.A</see> property of the specified color is ignored.</param>
        /// <param name="ditherer">An optional <see cref="IDitherer"/> instance to dither the result of the transformation if the inverse of the <paramref name="bitmapData"/>
        /// has no exact representation with its <see cref="IBitmapData.PixelFormat"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <para>This method is not a blocking call even if the <see cref="AsyncConfigBase.MaxDegreeOfParallelism"/> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="MakeOpaque">MakeOpaque</see> method for more details.</note>
        /// <remarks>
        /// </remarks>
        public static Task MakeOpaqueAsync(this IReadWriteBitmapData bitmapData, Color32 backColor, IDitherer? ditherer = null, TaskConfig? asyncConfig = null)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            return AsyncContext.DoOperationAsync(ctx => DoTransformColors(ctx, bitmapData, c => TransformMakeOpaque(c, backColor), ditherer), asyncConfig);
        }
#endif

        #endregion

        #region MakeGrayscale

        /// <summary>
        /// Makes this <paramref name="bitmapData"/> grayscale.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to make grayscale.</param>
        /// <param name="ditherer">An optional <see cref="IDitherer"/> instance to dither the result if grayscale colors
        /// cannot be represented by the <see cref="IBitmapData.PixelFormat"/> or the current palette of the specified <paramref name="bitmapData"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. Use the <see cref="BeginMakeGrayscale">BeginMakeGrayscale</see>
        /// or <see cref="MakeGrayscaleAsync">MakeGrayscaleAsync</see> (in .NET Framework 4.0 and above) methods for asynchronous call and to adjust parallelization, set up cancellation and for reporting progress.</note>
        /// <para>This method transforms the <paramref name="bitmapData"/> in place (its original content will be overwritten). To return a new instance
        /// use the <see cref="ToGrayscale">ToGrayscale</see> extension method, which always returns a bitmap data with <see cref="KnownPixelFormat.Format32bppArgb"/> format,
        /// or the <see cref="Clone(IReadableBitmapData, KnownPixelFormat, IQuantizer, IDitherer)">Clone</see> method with a grayscale
        /// quantizer (<see cref="PredefinedColorsQuantizer.Grayscale">PredefinedColorsQuantizer.Grayscale</see>, for example).</para>
        /// <para>This method calls the <see cref="TransformColors(IReadWriteBitmapData, Func{Color32, Color32}, IDitherer)">TransformColors</see> method internally. See
        /// the <strong>Remarks</strong> section of the <see cref="TransformColors(IReadWriteBitmapData, Func{Color32, Color32}, IDitherer)">TransformColors</see> method for more details.</para>
        /// <para>If <paramref name="bitmapData"/> has an indexed <see cref="IBitmapData.PixelFormat"/> and <paramref name="ditherer"/> is <see langword="null"/>,
        /// then its palette entries are tried to be transformed instead of the actual pixels in the first place (if it is supported by <paramref name="bitmapData"/>).
        /// To transform the colors of an indexed <see cref="IBitmapData"/> without changing the palette specify a non-<see langword="null"/>&#160;<paramref name="ditherer"/>.
        /// Transforming the palette is both faster and provides a better result.</para>
        /// <para>The <paramref name="ditherer"/> is ignored for <see cref="KnownPixelFormat"/>s with more than 16 bits-per-pixel and for the <see cref="KnownPixelFormat.Format16bppGrayScale"/> format.</para>
        /// </remarks>
        /// <seealso cref="ToGrayscale"/>
        public static void MakeGrayscale(this IReadWriteBitmapData bitmapData, IDitherer? ditherer = null)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            DoTransformColors(AsyncContext.Null, bitmapData, TransformMakeGrayscale, ditherer);
        }

        /// <summary>
        /// Begins to make this <paramref name="bitmapData"/> grayscale asynchronously.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to make grayscale.</param>
        /// <param name="ditherer">An optional <see cref="IDitherer"/> instance to dither the result if grayscale colors
        /// cannot be represented by the <see cref="IBitmapData.PixelFormat"/> or the current palette of the specified <paramref name="bitmapData"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="MakeGrayscaleAsync">MakeGrayscaleAsync</see> method.</para>
        /// <para>To finish the operation and to get the exception that occurred during the operation you have to call the <see cref="EndMakeGrayscale">EndMakeGrayscale</see> method.</para>
        /// <para>This method is not a blocking call even if the <see cref="AsyncConfigBase.MaxDegreeOfParallelism"/> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="MakeGrayscale">MakeGrayscale</see> method for more details.</note>
        /// </remarks>
        /// <seealso cref="BeginToGrayscale"/>
        public static IAsyncResult BeginMakeGrayscale(this IReadWriteBitmapData bitmapData, IDitherer? ditherer = null, AsyncConfig? asyncConfig = null)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            return AsyncContext.BeginOperation(ctx => DoTransformColors(ctx, bitmapData, TransformMakeGrayscale, ditherer), asyncConfig);
        }

        /// <summary>
        /// Waits for the pending asynchronous operation started by the <see cref="BeginMakeGrayscale">BeginMakeGrayscale</see> method to complete.
        /// In .NET Framework 4.0 and above you can use the <see cref="MakeGrayscaleAsync">MakeGrayscaleAsync</see> method instead.
        /// </summary>
        /// <param name="asyncResult">The reference to the pending asynchronous request to finish.</param>
        public static void EndMakeGrayscale(this IAsyncResult asyncResult) => AsyncContext.EndOperation(asyncResult, nameof(BeginMakeGrayscale));

#if !NET35
        /// <summary>
        /// Makes this <paramref name="bitmapData"/> grayscale asynchronously.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to make grayscale.</param>
        /// <param name="ditherer">An optional <see cref="IDitherer"/> instance to dither the result if grayscale colors
        /// cannot be represented by the <see cref="IBitmapData.PixelFormat"/> or the current palette of the specified <paramref name="bitmapData"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>This method is not a blocking call even if the <see cref="AsyncConfigBase.MaxDegreeOfParallelism"/> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="MakeGrayscale">MakeGrayscale</see> method for more details.</note>
        /// </remarks>
        /// <seealso cref="ToGrayscaleAsync"/>
        public static Task MakeGrayscaleAsync(this IReadWriteBitmapData bitmapData, IDitherer? ditherer = null, TaskConfig? asyncConfig = null)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            return AsyncContext.DoOperationAsync(ctx => DoTransformColors(ctx, bitmapData, TransformMakeGrayscale, ditherer), asyncConfig);
        }
#endif

        #endregion

        #region AdjustBrightness

        /// <summary>
        /// Adjusts the brightness of the specified <paramref name="bitmapData"/>.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to be transformed.</param>
        /// <param name="brightness">A float value between -1 and 1, inclusive bounds. Positive values make the <paramref name="bitmapData"/> brighter,
        /// while negative values make it darker.</param>
        /// <param name="ditherer">An optional <see cref="IDitherer"/> instance to dither the result of the transformation if the transformed colors
        /// are not compatible with the <see cref="IBitmapData.PixelFormat"/> of the specified <paramref name="bitmapData"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="channels">The <see cref="ColorChannels"/>, on which the adjustment has to be performed. This parameter is optional.
        /// <br/>Default value: <see cref="ColorChannels.Rgb"/>.</param>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. Use the <see cref="BeginAdjustBrightness">BeginAdjustBrightness</see>
        /// or <see cref="AdjustBrightnessAsync">AdjustBrightnessAsync</see> (in .NET Framework 4.0 and above) methods for asynchronous call and to adjust parallelization, set up cancellation and for reporting progress.</note>
        /// <para>This method calls the <see cref="TransformColors(IReadWriteBitmapData, Func{Color32, Color32}, IDitherer)">TransformColors</see> method internally. See
        /// the <strong>Remarks</strong> section of the <see cref="TransformColors(IReadWriteBitmapData, Func{Color32, Color32}, IDitherer)">TransformColors</see> method for more details.</para>
        /// <para>If <paramref name="bitmapData"/> has an indexed <see cref="IBitmapData.PixelFormat"/> and <paramref name="ditherer"/> is <see langword="null"/>,
        /// then its palette entries are tried to be transformed instead of the actual pixels in the first place (if it is supported by <paramref name="bitmapData"/>).
        /// To transform the colors of an indexed <see cref="IBitmapData"/> without changing the palette specify a non-<see langword="null"/>&#160;<paramref name="ditherer"/>.
        /// Transforming the palette is both faster and provides a better result.</para>
        /// <para>The <paramref name="ditherer"/> is ignored for <see cref="KnownPixelFormat"/>s with more than 16 bits-per-pixel and for the <see cref="KnownPixelFormat.Format16bppGrayScale"/> format.</para>
        /// <note type="tip">See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/drawing/?topic=html/M_KGySoft_Drawing_BitmapExtensions_AdjustBrightness.htm" target="_blank">BitmapExtensions.AdjustBrightness</a> method for an example.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="brightness"/> is not between -1 and 1
        /// <br/>-or-
        /// <br/><paramref name="channels"/> is out of the defined flags.</exception>
        public static void AdjustBrightness(this IReadWriteBitmapData bitmapData, float brightness, IDitherer? ditherer = null, ColorChannels channels = ColorChannels.Rgb)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            if (brightness < -1f || brightness > 1f || Single.IsNaN(brightness))
                throw new ArgumentOutOfRangeException(nameof(brightness), PublicResources.ArgumentMustBeBetween(-1f, 1f));
            if (!channels.AllFlagsDefined())
                throw new ArgumentOutOfRangeException(nameof(channels), PublicResources.FlagsEnumOutOfRange(channels));

            // ReSharper disable once CompareOfFloatsByEqualityOperator - zero has a precise float representation
            if (channels == ColorChannels.None || brightness == 0f)
                return;

            if (brightness < 0f)
            {
                brightness += 1f;
                DoTransformColors(AsyncContext.Null, bitmapData, c1 => TransformDarken(c1, brightness, channels), ditherer);
            }
            else
                DoTransformColors(AsyncContext.Null, bitmapData, c => TransformLighten(c, brightness, channels), ditherer);
        }

        /// <summary>
        /// Begins to adjust the brightness of the specified <paramref name="bitmapData"/> asynchronously.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to be transformed.</param>
        /// <param name="brightness">A float value between -1 and 1, inclusive bounds. Positive values make the <paramref name="bitmapData"/> brighter,
        /// while negative values make it darker.</param>
        /// <param name="ditherer">An optional <see cref="IDitherer"/> instance to dither the result of the transformation if the transformed colors
        /// are not compatible with the <see cref="IBitmapData.PixelFormat"/> of the specified <paramref name="bitmapData"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="channels">The <see cref="ColorChannels"/>, on which the adjustment has to be performed. This parameter is optional.
        /// <br/>Default value: <see cref="ColorChannels.Rgb"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="AdjustBrightnessAsync">AdjustBrightnessAsync</see> method.</para>
        /// <para>To finish the operation and to get the exception that occurred during the operation you have to call the <see cref="EndAdjustBrightness">EndAdjustBrightness</see> method.</para>
        /// <para>This method is not a blocking call even if the <see cref="AsyncConfigBase.MaxDegreeOfParallelism"/> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="AdjustBrightness">AdjustBrightness</see> method for more details.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="brightness"/> is not between -1 and 1
        /// <br/>-or-
        /// <br/><paramref name="channels"/> is out of the defined flags.</exception>
        public static IAsyncResult BeginAdjustBrightness(this IReadWriteBitmapData bitmapData, float brightness, IDitherer? ditherer = null, ColorChannels channels = ColorChannels.Rgb, AsyncConfig? asyncConfig = null)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            if (brightness < -1f || brightness > 1f || Single.IsNaN(brightness))
                throw new ArgumentOutOfRangeException(nameof(brightness), PublicResources.ArgumentMustBeBetween(-1f, 1f));
            if (!channels.AllFlagsDefined())
                throw new ArgumentOutOfRangeException(nameof(channels), PublicResources.FlagsEnumOutOfRange(channels));

            // ReSharper disable once CompareOfFloatsByEqualityOperator - zero has a precise float representation
            if (channels == ColorChannels.None || brightness == 0f)
                return AsyncContext.FromCompleted(asyncConfig);

            if (brightness >= 0f)
                return AsyncContext.BeginOperation(ctx => DoTransformColors(ctx, bitmapData, c => TransformLighten(c, brightness, channels), ditherer), asyncConfig);
            
            brightness += 1f;
            return AsyncContext.BeginOperation(ctx => DoTransformColors(ctx, bitmapData, c1 => TransformDarken(c1, brightness, channels), ditherer), asyncConfig);
        }

        /// <summary>
        /// Waits for the pending asynchronous operation started by the <see cref="BeginAdjustBrightness">BeginAdjustBrightness</see> method to complete.
        /// In .NET Framework 4.0 and above you can use the <see cref="AdjustBrightnessAsync">AdjustBrightnessAsync</see> method instead.
        /// </summary>
        /// <param name="asyncResult">The reference to the pending asynchronous request to finish.</param>
        public static void EndAdjustBrightness(this IAsyncResult asyncResult) => AsyncContext.EndOperation(asyncResult, nameof(BeginAdjustBrightness));

#if !NET35
        /// <summary>
        /// Adjusts the brightness of the specified <paramref name="bitmapData"/> asynchronously.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to be transformed.</param>
        /// <param name="brightness">A float value between -1 and 1, inclusive bounds. Positive values make the <paramref name="bitmapData"/> brighter,
        /// while negative values make it darker.</param>
        /// <param name="ditherer">An optional <see cref="IDitherer"/> instance to dither the result of the transformation if the transformed colors
        /// are not compatible with the <see cref="IBitmapData.PixelFormat"/> of the specified <paramref name="bitmapData"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="channels">The <see cref="ColorChannels"/>, on which the adjustment has to be performed. This parameter is optional.
        /// <br/>Default value: <see cref="ColorChannels.Rgb"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>This method is not a blocking call even if the <see cref="AsyncConfigBase.MaxDegreeOfParallelism"/> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="AdjustBrightness">AdjustBrightness</see> method for more details.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="brightness"/> is not between -1 and 1
        /// <br/>-or-
        /// <br/><paramref name="channels"/> is out of the defined flags.</exception>
        public static Task AdjustBrightnessAsync(this IReadWriteBitmapData bitmapData, float brightness, IDitherer? ditherer = null, ColorChannels channels = ColorChannels.Rgb, TaskConfig? asyncConfig = null)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            if (brightness < -1f || brightness > 1f || Single.IsNaN(brightness))
                throw new ArgumentOutOfRangeException(nameof(brightness), PublicResources.ArgumentMustBeBetween(-1f, 1f));
            if (!channels.AllFlagsDefined())
                throw new ArgumentOutOfRangeException(nameof(channels), PublicResources.FlagsEnumOutOfRange(channels));

            // ReSharper disable once CompareOfFloatsByEqualityOperator - zero has a precise float representation
            if (channels == ColorChannels.None || brightness == 0f)
                return AsyncContext.FromCompleted(asyncConfig);

            if (brightness >= 0f)
                return AsyncContext.DoOperationAsync(ctx => DoTransformColors(ctx, bitmapData, c => TransformLighten(c, brightness, channels), ditherer), asyncConfig);

            brightness += 1f;
            return AsyncContext.DoOperationAsync(ctx => DoTransformColors(ctx, bitmapData, c1 => TransformDarken(c1, brightness, channels), ditherer), asyncConfig);
        }
#endif

        #endregion

        #region AdjustContras

        /// <summary>
        /// Adjusts the contrast of the specified <paramref name="bitmapData"/>.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to be transformed.</param>
        /// <param name="contrast">A float value between -1 and 1, inclusive bounds. Positive values increase the contrast,
        /// while negative values decrease the it.</param>
        /// <param name="ditherer">An optional <see cref="IDitherer"/> instance to dither the result of the transformation if the transformed colors
        /// are not compatible with the <see cref="IBitmapData.PixelFormat"/> of the specified <paramref name="bitmapData"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="channels">The <see cref="ColorChannels"/>, on which the adjustment has to be performed. This parameter is optional.
        /// <br/>Default value: <see cref="ColorChannels.Rgb"/>.</param>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. Use the <see cref="BeginAdjustContrast">BeginAdjustContrast</see>
        /// or <see cref="AdjustContrastAsync">AdjustContrastAsync</see> (in .NET Framework 4.0 and above) methods for asynchronous call and to adjust parallelization, set up cancellation and for reporting progress.</note>
        /// <para>This method calls the <see cref="TransformColors(IReadWriteBitmapData, Func{Color32, Color32}, IDitherer)">TransformColors</see> method internally. See
        /// the <strong>Remarks</strong> section of the <see cref="TransformColors(IReadWriteBitmapData, Func{Color32, Color32}, IDitherer)">TransformColors</see> method for more details.</para>
        /// <para>If <paramref name="bitmapData"/> has an indexed <see cref="IBitmapData.PixelFormat"/> and <paramref name="ditherer"/> is <see langword="null"/>,
        /// then its palette entries are tried to be transformed instead of the actual pixels in the first place (if it is supported by <paramref name="bitmapData"/>).
        /// To transform the colors of an indexed <see cref="IBitmapData"/> without changing the palette specify a non-<see langword="null"/>&#160;<paramref name="ditherer"/>.
        /// Transforming the palette is both faster and provides a better result.</para>
        /// <para>The <paramref name="ditherer"/> is ignored for <see cref="KnownPixelFormat"/>s with more than 16 bits-per-pixel and for the <see cref="KnownPixelFormat.Format16bppGrayScale"/> format.</para>
        /// <note type="tip">See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/drawing/?topic=html/M_KGySoft_Drawing_BitmapExtensions_AdjustContrast.htm" target="_blank">BitmapExtensions.AdjustContrast</a> method for an example.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="contrast"/> is not between -1 and 1
        /// <br/>-or-
        /// <br/><paramref name="channels"/> is out of the defined flags.</exception>
        public static void AdjustContrast(this IReadWriteBitmapData bitmapData, float contrast, IDitherer? ditherer = null, ColorChannels channels = ColorChannels.Rgb)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            if (contrast < -1f || contrast > 1f || Single.IsNaN(contrast))
                throw new ArgumentOutOfRangeException(nameof(contrast), PublicResources.ArgumentMustBeBetween(-1f, 1f));
            if (!channels.AllFlagsDefined())
                throw new ArgumentOutOfRangeException(nameof(channels), PublicResources.FlagsEnumOutOfRange(channels));

            // ReSharper disable once CompareOfFloatsByEqualityOperator - zero has a precise float representation
            if (channels == ColorChannels.None || contrast == 0f)
                return;

            contrast += 1f;
            contrast *= contrast;

            DoTransformColors(AsyncContext.Null, bitmapData, c => TransformContrast(c, contrast, channels), ditherer);
        }

        /// <summary>
        /// Begins to adjust the contrast of the specified <paramref name="bitmapData"/> asynchronously.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to be transformed.</param>
        /// <param name="contrast">A float value between -1 and 1, inclusive bounds. Positive values increase the contrast,
        /// while negative values decrease the it.</param>
        /// <param name="ditherer">An optional <see cref="IDitherer"/> instance to dither the result of the transformation if the transformed colors
        /// are not compatible with the <see cref="IBitmapData.PixelFormat"/> of the specified <paramref name="bitmapData"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="channels">The <see cref="ColorChannels"/>, on which the adjustment has to be performed. This parameter is optional.
        /// <br/>Default value: <see cref="ColorChannels.Rgb"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="AdjustContrastAsync">AdjustContrastAsync</see> method.</para>
        /// <para>To finish the operation and to get the exception that occurred during the operation you have to call the <see cref="EndAdjustContrast">EndAdjustContrast</see> method.</para>
        /// <para>This method is not a blocking call even if the <see cref="AsyncConfigBase.MaxDegreeOfParallelism"/> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="AdjustContrast">AdjustContrast</see> method for more details.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="contrast"/> is not between -1 and 1
        /// <br/>-or-
        /// <br/><paramref name="channels"/> is out of the defined flags.</exception>
        public static IAsyncResult BeginAdjustContrast(this IReadWriteBitmapData bitmapData, float contrast, IDitherer? ditherer = null, ColorChannels channels = ColorChannels.Rgb, AsyncConfig? asyncConfig = null)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            if (contrast < -1f || contrast > 1f || Single.IsNaN(contrast))
                throw new ArgumentOutOfRangeException(nameof(contrast), PublicResources.ArgumentMustBeBetween(-1f, 1f));
            if (!channels.AllFlagsDefined())
                throw new ArgumentOutOfRangeException(nameof(channels), PublicResources.FlagsEnumOutOfRange(channels));

            // ReSharper disable once CompareOfFloatsByEqualityOperator - zero has a precise float representation
            if (channels == ColorChannels.None || contrast == 0f)
                return AsyncContext.FromCompleted(asyncConfig);

            contrast += 1f;
            contrast *= contrast;

            return AsyncContext.BeginOperation(ctx => DoTransformColors(ctx, bitmapData, c1 => TransformContrast(c1, contrast, channels), ditherer), asyncConfig);
        }

        /// <summary>
        /// Waits for the pending asynchronous operation started by the <see cref="BeginAdjustContrast">BeginAdjustContrast</see> method to complete.
        /// In .NET Framework 4.0 and above you can use the <see cref="AdjustContrastAsync">AdjustContrastAsync</see> method instead.
        /// </summary>
        /// <param name="asyncResult">The reference to the pending asynchronous request to finish.</param>
        public static void EndAdjustContrast(this IAsyncResult asyncResult) => AsyncContext.EndOperation(asyncResult, nameof(BeginAdjustContrast));

#if !NET35
        /// <summary>
        /// Adjusts the contrast of the specified <paramref name="bitmapData"/> asynchronously.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to be transformed.</param>
        /// <param name="contrast">A float value between -1 and 1, inclusive bounds. Positive values increase the contrast,
        /// while negative values decrease the it.</param>
        /// <param name="ditherer">An optional <see cref="IDitherer"/> instance to dither the result of the transformation if the transformed colors
        /// are not compatible with the <see cref="IBitmapData.PixelFormat"/> of the specified <paramref name="bitmapData"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="channels">The <see cref="ColorChannels"/>, on which the adjustment has to be performed. This parameter is optional.
        /// <br/>Default value: <see cref="ColorChannels.Rgb"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>This method is not a blocking call even if the <see cref="AsyncConfigBase.MaxDegreeOfParallelism"/> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="AdjustContrast">AdjustContrast</see> method for more details.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="contrast"/> is not between -1 and 1
        /// <br/>-or-
        /// <br/><paramref name="channels"/> is out of the defined flags.</exception>
        public static Task AdjustContrastAsync(this IReadWriteBitmapData bitmapData, float contrast, IDitherer? ditherer = null, ColorChannels channels = ColorChannels.Rgb, TaskConfig? asyncConfig = null)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            if (contrast < -1f || contrast > 1f || Single.IsNaN(contrast))
                throw new ArgumentOutOfRangeException(nameof(contrast), PublicResources.ArgumentMustBeBetween(-1f, 1f));
            if (!channels.AllFlagsDefined())
                throw new ArgumentOutOfRangeException(nameof(channels), PublicResources.FlagsEnumOutOfRange(channels));

            // ReSharper disable once CompareOfFloatsByEqualityOperator - zero has a precise float representation
            if (channels == ColorChannels.None || contrast == 0f)
                return AsyncContext.FromCompleted(asyncConfig);

            contrast += 1f;
            contrast *= contrast;

            return AsyncContext.DoOperationAsync(ctx => DoTransformColors(ctx, bitmapData, c1 => TransformContrast(c1, contrast, channels), ditherer), asyncConfig);
        }
#endif

        #endregion

        #region AdjustGamma

        /// <summary>
        /// Adjusts the gamma correction of the specified <paramref name="bitmapData"/>.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to be transformed.</param>
        /// <param name="gamma">A float value between 0 and 10, inclusive bounds. Values less than 1 decrease gamma correction,
        /// while values above 1 increase it.</param>
        /// <param name="ditherer">An optional <see cref="IDitherer"/> instance to dither the result of the transformation if the transformed colors
        /// are not compatible with the <see cref="IBitmapData.PixelFormat"/> of the specified <paramref name="bitmapData"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="channels">The <see cref="ColorChannels"/>, on which the adjustment has to be performed. This parameter is optional.
        /// <br/>Default value: <see cref="ColorChannels.Rgb"/>.</param>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. Use the <see cref="BeginAdjustGamma">BeginAdjustGamma</see>
        /// or <see cref="AdjustGammaAsync">AdjustGammaAsync</see> (in .NET Framework 4.0 and above) methods for asynchronous call and to adjust parallelization, set up cancellation and for reporting progress.</note>
        /// <para>This method calls the <see cref="TransformColors(IReadWriteBitmapData, Func{Color32, Color32}, IDitherer)">TransformColors</see> method internally. See
        /// the <strong>Remarks</strong> section of the <see cref="TransformColors(IReadWriteBitmapData, Func{Color32, Color32}, IDitherer)">TransformColors</see> method for more details.</para>
        /// <para>If <paramref name="bitmapData"/> has an indexed <see cref="IBitmapData.PixelFormat"/> and <paramref name="ditherer"/> is <see langword="null"/>,
        /// then its palette entries are tried to be transformed instead of the actual pixels in the first place (if it is supported by <paramref name="bitmapData"/>).
        /// To transform the colors of an indexed <see cref="IBitmapData"/> without changing the palette specify a non-<see langword="null"/>&#160;<paramref name="ditherer"/>.
        /// Transforming the palette is both faster and provides a better result.</para>
        /// <para>The <paramref name="ditherer"/> is ignored for <see cref="KnownPixelFormat"/>s with more than 16 bits-per-pixel and for the <see cref="KnownPixelFormat.Format16bppGrayScale"/> format.</para>
        /// <note type="tip">See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/drawing/?topic=html/M_KGySoft_Drawing_BitmapExtensions_AdjustGamma.htm" target="_blank">BitmapExtensions.AdjustGamma</a> method for an example.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="gamma"/> is not between 0 and 10
        /// <br/>-or-
        /// <br/><paramref name="channels"/> is out of the defined flags.</exception>
        public static void AdjustGamma(this IReadWriteBitmapData bitmapData, float gamma, IDitherer? ditherer = null, ColorChannels channels = ColorChannels.Rgb)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            if (gamma < 0f || gamma > 10f || Single.IsNaN(gamma))
                throw new ArgumentOutOfRangeException(nameof(gamma), PublicResources.ArgumentMustBeBetween(0f, 10f));
            if (!channels.AllFlagsDefined())
                throw new ArgumentOutOfRangeException(nameof(channels), PublicResources.FlagsEnumOutOfRange(channels));

            // ReSharper disable once CompareOfFloatsByEqualityOperator - 1 has a precise float representation
            if (channels == ColorChannels.None || gamma == 1f)
                return;

            byte[] table = GammaLookupTableCache[gamma];
            DoTransformColors(AsyncContext.Null, bitmapData, c => TransformGamma(c, channels, table), ditherer);
        }

        /// <summary>
        /// Begins to adjust the gamma correction of the specified <paramref name="bitmapData"/> asynchronously.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to be transformed.</param>
        /// <param name="gamma">A float value between 0 and 10, inclusive bounds. Values less than 1 decrease gamma correction,
        /// while values above 1 increase it.</param>
        /// <param name="ditherer">An optional <see cref="IDitherer"/> instance to dither the result of the transformation if the transformed colors
        /// are not compatible with the <see cref="IBitmapData.PixelFormat"/> of the specified <paramref name="bitmapData"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="channels">The <see cref="ColorChannels"/>, on which the adjustment has to be performed. This parameter is optional.
        /// <br/>Default value: <see cref="ColorChannels.Rgb"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="AdjustGammaAsync">AdjustGammaAsync</see> method.</para>
        /// <para>To finish the operation and to get the exception that occurred during the operation you have to call the <see cref="EndAdjustGamma">EndAdjustGamma</see> method.</para>
        /// <para>This method is not a blocking call even if the <see cref="AsyncConfigBase.MaxDegreeOfParallelism"/> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="AdjustGamma">AdjustGamma</see> method for more details.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="gamma"/> is not between 0 and 10
        /// <br/>-or-
        /// <br/><paramref name="channels"/> is out of the defined flags.</exception>
        public static IAsyncResult BeginAdjustGamma(this IReadWriteBitmapData bitmapData, float gamma, IDitherer? ditherer = null, ColorChannels channels = ColorChannels.Rgb, AsyncConfig? asyncConfig = null)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            if (gamma < 0f || gamma > 10f || Single.IsNaN(gamma))
                throw new ArgumentOutOfRangeException(nameof(gamma), PublicResources.ArgumentMustBeBetween(0f, 10f));
            if (!channels.AllFlagsDefined())
                throw new ArgumentOutOfRangeException(nameof(channels), PublicResources.FlagsEnumOutOfRange(channels));

            // ReSharper disable once CompareOfFloatsByEqualityOperator - zero has a precise float representation
            if (channels == ColorChannels.None || gamma == 1f)
                return AsyncContext.FromCompleted(asyncConfig);

            return AsyncContext.BeginOperation(ctx => DoTransformColors(ctx, bitmapData, c1 => TransformGamma(c1, channels, GammaLookupTableCache[gamma]), ditherer), asyncConfig);
        }

        /// <summary>
        /// Waits for the pending asynchronous operation started by the <see cref="BeginAdjustGamma">BeginAdjustGamma</see> method to complete.
        /// In .NET Framework 4.0 and above you can use the <see cref="AdjustGammaAsync">AdjustGammaAsync</see> method instead.
        /// </summary>
        /// <param name="asyncResult">The reference to the pending asynchronous request to finish.</param>
        public static void EndAdjustGamma(this IAsyncResult asyncResult) => AsyncContext.EndOperation(asyncResult, nameof(BeginAdjustGamma));

#if !NET35
        /// <summary>
        /// Adjusts the gamma correction of the specified <paramref name="bitmapData"/> asynchronously.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to be transformed.</param>
        /// <param name="gamma">A float value between 0 and 10, inclusive bounds. Values less than 1 decrease gamma correction,
        /// while values above 1 increase it.</param>
        /// <param name="ditherer">An optional <see cref="IDitherer"/> instance to dither the result of the transformation if the transformed colors
        /// are not compatible with the <see cref="IBitmapData.PixelFormat"/> of the specified <paramref name="bitmapData"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="channels">The <see cref="ColorChannels"/>, on which the adjustment has to be performed. This parameter is optional.
        /// <br/>Default value: <see cref="ColorChannels.Rgb"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>This method is not a blocking call even if the <see cref="AsyncConfigBase.MaxDegreeOfParallelism"/> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="AdjustGamma">AdjustGamma</see> method for more details.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="gamma"/> is not between 0 and 10
        /// <br/>-or-
        /// <br/><paramref name="channels"/> is out of the defined flags.</exception>
        public static Task AdjustGammaAsync(this IReadWriteBitmapData bitmapData, float gamma, IDitherer? ditherer = null, ColorChannels channels = ColorChannels.Rgb, TaskConfig? asyncConfig = null)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            if (gamma < 0f || gamma > 10f || Single.IsNaN(gamma))
                throw new ArgumentOutOfRangeException(nameof(gamma), PublicResources.ArgumentMustBeBetween(0f, 10f));
            if (!channels.AllFlagsDefined())
                throw new ArgumentOutOfRangeException(nameof(channels), PublicResources.FlagsEnumOutOfRange(channels));

            // ReSharper disable once CompareOfFloatsByEqualityOperator - zero has a precise float representation
            if (channels == ColorChannels.None || gamma == 1f)
                return AsyncContext.FromCompleted(asyncConfig);

            return AsyncContext.DoOperationAsync(ctx => DoTransformColors(ctx, bitmapData, c1 => TransformGamma(c1, channels, GammaLookupTableCache[gamma]), ditherer), asyncConfig);
        }
#endif

        #endregion

        #endregion

        #region Private Methods

        #region Validation

        private static void ValidateArguments(IReadWriteBitmapData bitmapData, Func<Color32, Color32> transformFunction)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            if (transformFunction == null)
                throw new ArgumentNullException(nameof(transformFunction), PublicResources.ArgumentNull);
        }

        #endregion

        #region Quantizing/Dithering

        [SuppressMessage("ReSharper", "AccessToDisposedClosure", Justification = "ParallelHelper.For invokes delegates before returning")]
        private static void DoQuantize(IAsyncContext context, IReadWriteBitmapData bitmapData, IQuantizer quantizer)
        {
            IBitmapDataInternal accessor = bitmapData as IBitmapDataInternal ?? new BitmapDataWrapper(bitmapData, true, true);
            try
            {
                context.Progress?.New(DrawingOperation.InitializingQuantizer);
                using (IQuantizingSession session = quantizer.Initialize(bitmapData, context))
                {
                    if (context.IsCancellationRequested)
                        return;
                    if (session == null)
                        throw new InvalidOperationException(Res.ImagingQuantizerInitializeNull);

                    // Sequential processing
                    if (bitmapData.Width < parallelThreshold >> quantizingScale)
                    {
                        context.Progress?.New(DrawingOperation.ProcessingPixels, bitmapData.Height);
                        int width = bitmapData.Width;
                        IBitmapDataRowInternal row = accessor.DoGetRow(0);
                        do
                        {
                            if (context.IsCancellationRequested)
                                return;
                            for (int x = 0; x < width; x++)
                                row.DoSetColor32(x, session.GetQuantizedColor(row.DoGetColor32(x)));
                            context.Progress?.Increment();
                        } while (row.MoveNextRow());

                        return;
                    }

                    // Parallel processing
                    ParallelHelper.For(context, DrawingOperation.ProcessingPixels, 0, bitmapData.Height, y =>
                    {
                        int width = bitmapData.Width;
                        IBitmapDataRowInternal row = accessor.DoGetRow(y);
                        for (int x = 0; x < width; x++)
                            row.DoSetColor32(x, session.GetQuantizedColor(row.DoGetColor32(x)));
                    });
                }
            }
            finally
            {
                if (!ReferenceEquals(bitmapData, accessor))
                    accessor.Dispose();
            }
        }

        [SuppressMessage("ReSharper", "AccessToDisposedClosure", Justification = "ParallelHelper.For invokes delegates before returning")]
        private static void DoDither(IAsyncContext context, IReadWriteBitmapData bitmapData, IQuantizer quantizer, IDitherer ditherer)
        {
            IBitmapDataInternal accessor = bitmapData as IBitmapDataInternal ?? new BitmapDataWrapper(bitmapData, true, true);

            try
            {
                context.Progress?.New(DrawingOperation.InitializingQuantizer);
                using (IQuantizingSession quantizingSession = quantizer.Initialize(bitmapData, context))
                {
                    if (context.IsCancellationRequested)
                        return;
                    if (quantizingSession == null)
                        throw new InvalidOperationException(Res.ImagingQuantizerInitializeNull);
                    context.Progress?.New(DrawingOperation.InitializingDitherer);
                    using (IDitheringSession ditheringSession = ditherer.Initialize(bitmapData, quantizingSession, context))
                    {
                        if (context.IsCancellationRequested)
                            return;
                        if (ditheringSession == null)
                            throw new InvalidOperationException(Res.ImagingDithererInitializeNull);

                        // Sequential processing
                        if (ditheringSession.IsSequential || bitmapData.Width < parallelThreshold >> ditheringScale)
                        {
                            context.Progress?.New(DrawingOperation.ProcessingPixels, bitmapData.Height);
                            int width = bitmapData.Width;
                            IBitmapDataRowInternal row = accessor.DoGetRow(0);
                            int y = 0;
                            do
                            {
                                if (context.IsCancellationRequested)
                                    return;
                                for (int x = 0; x < width; x++)
                                    row.DoSetColor32(x, ditheringSession.GetDitheredColor(row.DoGetColor32(x), x, y));
                                y += 1;
                                context.Progress?.Increment();
                            } while (row.MoveNextRow());

                            return;
                        }

                        // Parallel processing
                        ParallelHelper.For(context, DrawingOperation.ProcessingPixels, 0, bitmapData.Height, y =>
                        {
                            int width = bitmapData.Width;
                            IBitmapDataRowInternal row = accessor.DoGetRow(y);
                            for (int x = 0; x < width; x++)
                                row.DoSetColor32(x, ditheringSession.GetDitheredColor(row.DoGetColor32(x), x, y));
                        });
                    }
                }
            }
            finally
            {
                if (!ReferenceEquals(bitmapData, accessor))
                    accessor.Dispose();
            }
        }

        #endregion

        #region Color Transformations

        [SuppressMessage("ReSharper", "AccessToDisposedClosure", Justification = "ParallelHelper.For invokes delegates before returning")]
        private static void DoTransformColors(IAsyncContext context, IReadWriteBitmapData bitmapData, Func<Color32, Color32> transformFunction)
        {
            // Indexed format: processing the palette entries when possible
            if (bitmapData is IBitmapDataInternal bitmapDataInternal && bitmapDataInternal.CanSetPalette)
            {
                context.Progress?.New(DrawingOperation.ProcessingPixels, 1);
                Palette palette = bitmapData.Palette!;
                Color32[] oldEntries = palette.Entries;
                Color32[] newEntries = new Color32[oldEntries.Length];
                for (int i = 0; i < newEntries.Length; i++)
                    newEntries[i] = transformFunction.Invoke(oldEntries[i]);
                if (bitmapDataInternal.TrySetPalette(new Palette(newEntries, palette.BackColor, palette.AlphaThreshold)))
                {
                    context.Progress?.Complete();
                    return;
                }

                Debug.Fail("Setting the palette of the same size should work if CanSetPalette is true");
            }

            if (bitmapData.Height < 1)
                return;

            // Non-indexed format or palette cannot be set: processing the pixels
            var accessor = bitmapData as IBitmapDataInternal ?? new BitmapDataWrapper(bitmapData, true, true);
            try
            {
                // Sequential processing
                if (bitmapData.Width < parallelThreshold)
                {
                    context.Progress?.New(DrawingOperation.ProcessingPixels, bitmapData.Height);
                    IBitmapDataRowInternal row = accessor.DoGetRow(0);
                    do
                    {
                        if (context.IsCancellationRequested)
                            return;
                        for (int x = 0; x < bitmapData.Width; x++)
                            row.DoSetColor32(x, transformFunction.Invoke(row.DoGetColor32(x)));
                        context.Progress?.Increment();
                    } while (row.MoveNextRow());

                    return;
                }

                // Parallel processing
                ParallelHelper.For(context, DrawingOperation.ProcessingPixels, 0, bitmapData.Height, y =>
                {
                    IBitmapDataRowInternal row = accessor.DoGetRow(y);
                    for (int x = 0; x < bitmapData.Width; x++)
                        row.DoSetColor32(x, transformFunction.Invoke(row.DoGetColor32(x)));
                });
            }
            finally
            {
                if (!ReferenceEquals(bitmapData, accessor))
                    accessor.Dispose();
            }
        }

        [SuppressMessage("ReSharper", "AccessToDisposedClosure", Justification = "ParallelHelper.For invokes delegates before returning")]
        private static void DoTransformColors(IAsyncContext context, IReadWriteBitmapData bitmapData, Func<Color32, Color32> transformFunction, IDitherer? ditherer)
        {
            if (ditherer == null || !bitmapData.PixelFormat.CanBeDithered)
            {
                DoTransformColors(context, bitmapData, transformFunction);
                return;
            }

            // Special handling if ditherer relies on actual content: transforming into an ARGB32 result, and dithering that temporary result
            if (ditherer.InitializeReliesOnContent)
            {
                // not using premultiplied format because transformation is faster on simple ARGB32
                using IBitmapDataInternal? tempClone = DoCloneDirect(context, bitmapData, new Rectangle(Point.Empty, bitmapData.GetSize()), KnownPixelFormat.Format32bppArgb);
                if (context.IsCancellationRequested)
                    return;

                Debug.Assert(tempClone != null);
                DoTransformColors(context, tempClone!, transformFunction);
                if (context.IsCancellationRequested)
                    return;

                DoCopy(context, tempClone!, bitmapData, new Rectangle(Point.Empty, tempClone.GetSize()), Point.Empty, null, ditherer);
                return;
            }

            if (bitmapData.Height < 1)
                return;

            IBitmapDataInternal accessor = bitmapData as IBitmapDataInternal ?? new BitmapDataWrapper(bitmapData, true, true);
            try
            {
                IQuantizer quantizer = PredefinedColorsQuantizer.FromBitmapData(bitmapData);
                Debug.Assert(!quantizer.InitializeReliesOnContent, "A predefined color quantizer should not depend on actual content");
                context.Progress?.New(DrawingOperation.InitializingQuantizer); // predefined will be extreme fast bu in case someone tracks progress...
                using (IQuantizingSession quantizingSession = quantizer.Initialize(bitmapData, context))
                {
                    if (context.IsCancellationRequested)
                        return;
                    context.Progress?.New(DrawingOperation.InitializingDitherer);
                    using (IDitheringSession ditheringSession = ditherer.Initialize(bitmapData, quantizingSession, context))
                    {
                        if (context.IsCancellationRequested)
                            return;
                        if (ditheringSession == null)
                            throw new InvalidOperationException(Res.ImagingDithererInitializeNull);

                        // sequential processing
                        if (ditheringSession.IsSequential || bitmapData.Width < parallelThreshold)
                        {
                            context.Progress?.New(DrawingOperation.ProcessingPixels, bitmapData.Height);
                            IBitmapDataRowInternal row = accessor.DoGetRow(0);
                            int y = 0;
                            do
                            {
                                if (context.IsCancellationRequested)
                                    return;
                                for (int x = 0; x < bitmapData.Width; x++)
                                    row.DoSetColor32(x, ditheringSession.GetDitheredColor(transformFunction.Invoke(row.DoGetColor32(x)), x, y));
                                y += 1;
                                context.Progress?.Increment();
                            } while (row.MoveNextRow());

                            return;
                        }

                        // parallel processing
                        ParallelHelper.For(context, DrawingOperation.ProcessingPixels, 0, bitmapData.Height, y =>
                        {
                            IBitmapDataRowInternal row = accessor.DoGetRow(y);
                            for (int x = 0; x < bitmapData.Width; x++)
                                row.DoSetColor32(x, ditheringSession.GetDitheredColor(transformFunction.Invoke(row.DoGetColor32(x)), x, y));
                        });
                    }
                }
            }
            finally
            {
                if (!ReferenceEquals(bitmapData, accessor))
                    accessor.Dispose();
            }
        }

        private static Color32 TransformReplaceColor(Color32 c, Color32 oldColor, Color32 newColor) => c == oldColor ? newColor : c;

        private static Color32 TransformInvert(Color32 c) => new Color32(c.A, (byte)(255 - c.R), (byte)(255 - c.G), (byte)(255 - c.B));

        private static Color32 TransformMakeOpaque(Color32 c, Color32 backColor) => c.A == Byte.MaxValue ? c : c.BlendWithBackground(backColor);

        private static Color32 TransformMakeGrayscale(Color32 c) => c.ToGray();

        private static Color32 TransformLighten(Color32 c, float brightness, ColorChannels channels) => new Color32(c.A,
            (channels & ColorChannels.R) == ColorChannels.R ? (byte)((255 - c.R) * brightness + c.R) : c.R,
            (channels & ColorChannels.G) == ColorChannels.G ? (byte)((255 - c.G) * brightness + c.G) : c.G,
            (channels & ColorChannels.B) == ColorChannels.B ? (byte)((255 - c.B) * brightness + c.B) : c.B);

        private static Color32 TransformDarken(Color32 c, float brightness, ColorChannels channels) => new Color32(c.A,
            (channels & ColorChannels.R) == ColorChannels.R ? (byte)(c.R * brightness) : c.R,
            (channels & ColorChannels.G) == ColorChannels.G ? (byte)(c.G * brightness) : c.G,
            (channels & ColorChannels.B) == ColorChannels.B ? (byte)(c.B * brightness) : c.B);

        private static Color32 TransformContrast(Color32 c, float contrast, ColorChannels channels) => new Color32(c.A,
            (channels & ColorChannels.R) == ColorChannels.R ? ((int)(((c.R / 255f - 0.5f) * contrast + 0.5f) * 255f)).ClipToByte() : c.R,
            (channels & ColorChannels.G) == ColorChannels.G ? ((int)(((c.G / 255f - 0.5f) * contrast + 0.5f) * 255f)).ClipToByte() : c.G,
            (channels & ColorChannels.B) == ColorChannels.B ? ((int)(((c.B / 255f - 0.5f) * contrast + 0.5f) * 255f)).ClipToByte() : c.B);

        private static Color32 TransformGamma(Color32 c, ColorChannels channels, byte[] table) => new Color32(c.A,
            (channels & ColorChannels.R) == ColorChannels.R ? table[c.R] : c.R,
            (channels & ColorChannels.G) == ColorChannels.G ? table[c.G] : c.G,
            (channels & ColorChannels.B) == ColorChannels.B ? table[c.B] : c.B);

        private static byte[] GenerateGammaLookupTable(float gamma)
        {
            byte[] result = new byte[256];
            for (int i = 0; i < 256; i++)
#if NETFRAMEWORK || NETSTANDARD2_0
                result[i] = ((int)(255d * Math.Pow(i / 255d, 1d / gamma) + 0.5d)).ClipToByte();
#else
                result[i] = ((int)(255f * MathF.Pow(i / 255f, 1f / gamma) + 0.5f)).ClipToByte();
#endif
            return result;
        }

        #endregion

        #endregion

        #endregion
    }
}
