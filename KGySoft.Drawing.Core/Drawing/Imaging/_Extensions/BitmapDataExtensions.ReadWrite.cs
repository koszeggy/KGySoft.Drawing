#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BitmapDataExtensions.ReadWrite.cs
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
using System.Security;
using System.Threading;
#if !NET35
using System.Threading.Tasks; 
#endif

using KGySoft.Collections;
using KGySoft.CoreLibraries;
using KGySoft.Threading;

#endregion

#if NET35
#pragma warning disable CS1574 // XML comment has cref attribute that could not be resolved - in .NET 3.5 not all members are available
#endif

namespace KGySoft.Drawing.Imaging
{
    partial class BitmapDataExtensions
    {
        #region Fields

        private static IThreadSafeCacheAccessor<float, byte[]>? gammaLookupTableCache32;
        private static IThreadSafeCacheAccessor<float, ushort[]>? gammaLookupTableCache64;

        #endregion

        #region Properties

        private static IThreadSafeCacheAccessor<float, byte[]> GammaLookupTableCache32
        {
            get
            {
                if (gammaLookupTableCache32 == null)
                {
                    var options = new LockFreeCacheOptions { InitialCapacity = 4, ThresholdCapacity = 16, HashingStrategy = HashingStrategy.Modulo, MergeInterval = TimeSpan.FromMilliseconds(100) };
                    Interlocked.CompareExchange(ref gammaLookupTableCache32, ThreadSafeCacheFactory.Create<float, byte[]>(GenerateGammaLookupTable32, options), null);
                }

                return gammaLookupTableCache32;
            }
        }

        private static IThreadSafeCacheAccessor<float, ushort[]> GammaLookupTableCache64
        {
            get
            {
                if (gammaLookupTableCache64 == null)
                {
                    var options = new LockFreeCacheOptions { InitialCapacity = 2, ThresholdCapacity = 2, HashingStrategy = HashingStrategy.Modulo, MergeInterval = TimeSpan.FromMilliseconds(100) };
                    Interlocked.CompareExchange(ref gammaLookupTableCache64, ThreadSafeCacheFactory.Create<float, ushort[]>(GenerateGammaLookupTable64, options), null);
                }

                return gammaLookupTableCache64;
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
        /// </summary>
        /// <param name="source">The source bitmap data to be clipped.</param>
        /// <param name="clippingRegion">A <see cref="Rectangle"/> that specifies a region within the <paramref name="source"/>.</param>
        /// <param name="disposeSource"><see langword="true"/> to dispose <paramref name="source"/> when the result is disposed; otherwise, <see langword="false"/>.</param>
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
            return clippingRegion.Location.IsEmpty && clippingRegion.Size == source.Size
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

        #region Quantizing

        /// <summary>
        /// Quantizes an <see cref="IReadWriteBitmapData"/> using the specified <paramref name="quantizer"/> (reduces the number of colors).
        /// </summary>
        /// <param name="bitmapData">An <see cref="IReadWriteBitmapData"/> instance to be quantized.</param>
        /// <param name="quantizer">An <see cref="IQuantizer"/> implementation to be used for quantizing the specified <paramref name="bitmapData"/>.</param>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. You can use
        /// the <see cref="Quantize(IReadWriteBitmapData, IQuantizer, ParallelConfig)"/> overload to configure these, while still executing the method synchronously. Alternatively, use
        /// the <see cref="BeginQuantize">BeginQuantize</see> or <see cref="QuantizeAsync">QuantizeAsync</see> (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// <para>This method quantizes the specified <paramref name="bitmapData"/> in place (its original content will be overwritten). To return a new instance
        /// use the <see cref="Clone(IReadableBitmapData, KnownPixelFormat, IQuantizer, IDitherer)">Clone</see> extension method instead.</para>
        /// <para>If the <see cref="KnownPixelFormat"/> or the palette of <paramref name="bitmapData"/> is not compatible with the colors of the specified <paramref name="quantizer"/>, then
        /// the result may not be correct.</para>
        /// <para>If <paramref name="bitmapData"/> has already the same set of colors that the specified <paramref name="quantizer"/>, then it can happen
        /// that calling this method does not change the <paramref name="bitmapData"/> at all.</para>
        /// <note type="tip"><list type="bullet">
        /// <item>To use predefined colors or custom quantization functions use the static methods of the <see cref="PredefinedColorsQuantizer"/> class.
        /// <br/>See the <strong>Remarks</strong> section of its members for details and examples.</item>
        /// <item>To use an optimized palette of a specified number of colors adjusted for <paramref name="bitmapData"/> see the <see cref="OptimizedPaletteQuantizer"/> class.</item>
        /// </list></note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> or <paramref name="quantizer"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException">The <paramref name="quantizer"/>'s <see cref="IQuantizer.Initialize">Initialize</see> method returned <see langword="null"/>.</exception>
        public static void Quantize(this IReadWriteBitmapData bitmapData, IQuantizer quantizer) => bitmapData.Quantize(AsyncHelper.DefaultContext, quantizer);

        /// <summary>
        /// Quantizes an <see cref="IReadWriteBitmapData"/> using the specified <paramref name="quantizer"/> (reduces the number of colors).
        /// </summary>
        /// <param name="bitmapData">An <see cref="IReadWriteBitmapData"/> instance to be quantized.</param>
        /// <param name="quantizer">An <see cref="IQuantizer"/> implementation to be used for quantizing the specified <paramref name="bitmapData"/>.</param>
        /// <param name="parallelConfig">The configuration of the operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.
        /// If <see langword="null"/>, then the degree of parallelization is configured automatically.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// of the <paramref name="parallelConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <note>This method blocks the caller as it executes synchronously, though the <paramref name="parallelConfig"/> parameter allows configuring the degree of parallelism,
        /// cancellation and progress reporting. Use the <see cref="BeginQuantize">BeginQuantize</see> or <see cref="QuantizeAsync">QuantizeAsync</see>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// <para>This method quantizes the specified <paramref name="bitmapData"/> in place (its original content will be overwritten). To return a new instance
        /// use the <see cref="Clone(IReadableBitmapData, KnownPixelFormat, IQuantizer, IDitherer, ParallelConfig)">Clone</see> extension method instead.</para>
        /// <para>If the <see cref="KnownPixelFormat"/> or the palette of <paramref name="bitmapData"/> is not compatible with the colors of the specified <paramref name="quantizer"/>, then
        /// the result may not be correct.</para>
        /// <para>If <paramref name="bitmapData"/> has already the same set of colors that the specified <paramref name="quantizer"/>, then it can happen
        /// that calling this method does not change the <paramref name="bitmapData"/> at all.</para>
        /// <note type="tip"><list type="bullet">
        /// <item>To use predefined colors or custom quantization functions use the static methods of the <see cref="PredefinedColorsQuantizer"/> class.
        /// <br/>See the <strong>Remarks</strong> section of its members for details and examples.</item>
        /// <item>To use an optimized palette of a specified number of colors adjusted for <paramref name="bitmapData"/> see the <see cref="OptimizedPaletteQuantizer"/> class.</item>
        /// </list></note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> or <paramref name="quantizer"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException">The <paramref name="quantizer"/>'s <see cref="IQuantizer.Initialize">Initialize</see> method returned <see langword="null"/>.</exception>
        public static bool Quantize(this IReadWriteBitmapData bitmapData, IQuantizer quantizer, ParallelConfig? parallelConfig)
        {
            // NOTE: The parallelConfig parameter could just be an additional optional parameter in the original overload but that would have been a breaking change.
            // Also, this overload has a bool return value, and there is a minimal overhead with the DoOperationSynchronously call.
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            if (quantizer == null)
                throw new ArgumentNullException(nameof(quantizer), PublicResources.ArgumentNull);

            return AsyncHelper.DoOperationSynchronously(ctx => DoQuantize(ctx, bitmapData, quantizer), parallelConfig);
        }

        /// <summary>
        /// Quantizes an <see cref="IReadWriteBitmapData"/> with the specified <paramref name="quantizer"/> (reduces the number of colors),
        /// using a <paramref name="context"/> that may belong to a higher level, possibly asynchronous operation.
        /// </summary>
        /// <param name="bitmapData">An <see cref="IReadWriteBitmapData"/> instance to be quantized.</param>
        /// <param name="context">An <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncContext.htm">IAsyncContext</a> instance
        /// that contains information for asynchronous processing about the current operation.</param>
        /// <param name="quantizer">An <see cref="IQuantizer"/> implementation to be used for quantizing the specified <paramref name="bitmapData"/>.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled.</returns>
        /// <remarks>
        /// <para>This method blocks the caller thread but if <paramref name="context"/> belongs to an async top level method, then the execution may already run
        /// on a pool thread. Degree of parallelism, the ability of cancellation and reporting progress depend on how these were configured at the top level method.
        /// To reconfigure the degree of parallelism of an existing context, you can use the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_AsyncContextWrapper.htm">AsyncContextWrapper</a> class.</para>
        /// <para>Alternatively, you can use this method to specify the degree of parallelism for synchronous execution. For example, by
        /// passing <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncHelper_SingleThreadContext.htm">AsyncHelper.SingleThreadContext</a> to the <paramref name="context"/> parameter
        /// the method will be forced to use a single thread only.</para>
        /// <para>When reporting progress, this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.</para>
        /// <note type="tip">See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_AsyncHelper.htm">AsyncHelper</a>
        /// class for details about how to create a context for possibly async top level methods.</note>
        /// <note>See the <see cref="Quantize(IReadWriteBitmapData, IQuantizer)"/> overload for more details about the other parameters.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> or <paramref name="quantizer"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException">The <paramref name="quantizer"/>'s <see cref="IQuantizer.Initialize">Initialize</see> method returned <see langword="null"/>.</exception>
        public static bool Quantize(this IReadWriteBitmapData bitmapData, IAsyncContext? context, IQuantizer quantizer)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            if (quantizer == null)
                throw new ArgumentNullException(nameof(quantizer), PublicResources.ArgumentNull);

            return DoQuantize(context ?? AsyncHelper.DefaultContext, bitmapData, quantizer);
        }

        /// <summary>
        /// Begins to quantize an <see cref="IReadWriteBitmapData"/> asynchronously, using the specified <paramref name="quantizer"/> (reduces the number of colors).
        /// </summary>
        /// <param name="bitmapData">An <see cref="IReadWriteBitmapData"/> instance to be quantized.</param>
        /// <param name="quantizer">An <see cref="IQuantizer"/> implementation to be used for quantizing the specified <paramref name="bitmapData"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> or <paramref name="quantizer"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="QuantizeAsync">QuantizeAsync</see> method.</para>
        /// <para>To finish the operation and to get the exception that occurred during the operation you have to call the <see cref="EndQuantize">EndQuantize</see> method.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="Quantize(IReadWriteBitmapData, IQuantizer)">Quantize</see> method for more details.</note>
        /// </remarks>
        public static IAsyncResult BeginQuantize(this IReadWriteBitmapData bitmapData, IQuantizer quantizer, AsyncConfig? asyncConfig = null)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            if (quantizer == null)
                throw new ArgumentNullException(nameof(quantizer), PublicResources.ArgumentNull);

            return AsyncHelper.BeginOperation(ctx => DoQuantize(ctx, bitmapData, quantizer), asyncConfig);
        }

        /// <summary>
        /// Waits for the pending asynchronous operation started by the <see cref="BeginQuantize">BeginQuantize</see> method to complete.
        /// In .NET Framework 4.0 and above you can use the <see cref="QuantizeAsync">QuantizeAsync</see> method instead.
        /// </summary>
        /// <param name="asyncResult">The reference to the pending asynchronous request to finish.</param>
        /// <exception cref="InvalidOperationException">The quantizer's <see cref="IQuantizer.Initialize">Initialize</see> method returned <see langword="null"/>.</exception>
        public static void EndQuantize(this IAsyncResult asyncResult)
            // NOTE: the return value could be bool, but it would be a breaking change
            => AsyncHelper.EndOperation<bool>(asyncResult, nameof(BeginQuantize));

#if !NET35
        /// <summary>
        /// Quantizes an <see cref="IReadWriteBitmapData"/> asynchronously, using the specified <paramref name="quantizer"/> (reduces the number of colors).
        /// </summary>
        /// <param name="bitmapData">An <see cref="IReadWriteBitmapData"/> instance to be quantized.</param>
        /// <param name="quantizer">An <see cref="IQuantizer"/> implementation to be used for quantizing the specified <paramref name="bitmapData"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> or <paramref name="quantizer"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException">The <paramref name="quantizer"/>'s <see cref="IQuantizer.Initialize">Initialize</see> method returned <see langword="null"/>.</exception>
        /// <remarks>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="Quantize(IReadWriteBitmapData, IQuantizer)">Quantize</see> method for more details.</note>
        /// </remarks>
        public static Task QuantizeAsync(this IReadWriteBitmapData bitmapData, IQuantizer quantizer, TaskConfig? asyncConfig = null)
        {
            // NOTE: the return value could be Task<bool> but it would be a breaking change
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            if (quantizer == null)
                throw new ArgumentNullException(nameof(quantizer), PublicResources.ArgumentNull);

            return AsyncHelper.DoOperationAsync(ctx => DoQuantize(ctx, bitmapData, quantizer), asyncConfig);
        }
#endif

        #endregion

        #region Dithering

        /// <summary>
        /// Quantizes an <see cref="IReadWriteBitmapData"/> with dithering (reduces the number of colors while trying to preserve details)
        /// using the specified <paramref name="quantizer"/> and <paramref name="ditherer"/>.
        /// </summary>
        /// <param name="bitmapData">An <see cref="IReadWriteBitmapData"/> instance to be quantized.</param>
        /// <param name="quantizer">An <see cref="IQuantizer"/> implementation to be used for quantizing the specified <paramref name="bitmapData"/>.</param>
        /// <param name="ditherer">An <see cref="IDitherer"/> implementation to be used for dithering during the quantization of the specified <paramref name="bitmapData"/>.</param>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. You can use
        /// the <see cref="Dither(IReadWriteBitmapData, IQuantizer, IDitherer, ParallelConfig)"/> overload to configure these, while still executing the method synchronously. Alternatively, use
        /// the <see cref="BeginDither">BeginDither</see> or <see cref="DitherAsync">DitherAsync</see> (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// <para>This method quantizes <paramref name="bitmapData"/> with dithering in place (its original content will be overwritten). To return a new instance
        /// use the <see cref="Clone(IReadableBitmapData, KnownPixelFormat, IQuantizer, IDitherer)">Clone</see> extension method instead.</para>
        /// <para>If the <see cref="KnownPixelFormat"/> or the palette of <paramref name="bitmapData"/> is not compatible with the colors of the specified <paramref name="quantizer"/>, then
        /// the result may not be correct.</para>
        /// <para>If <paramref name="bitmapData"/> has already the same set of colors that the specified <paramref name="quantizer"/>, then it can happen
        /// that calling this method does not change <paramref name="bitmapData"/> at all.</para>
        /// <note type="tip"><list type="bullet">
        /// <item>To use predefined colors or custom quantization functions use the static methods of the <see cref="PredefinedColorsQuantizer"/> class.
        /// <br/>See the <strong>Remarks</strong> section of its members for details and examples.</item>
        /// <item>To use an optimized palette of a specified number of colors adjusted for <paramref name="bitmapData"/> see the <see cref="OptimizedPaletteQuantizer"/> class.</item>
        /// <item>For some built-in dithering solutions see the <see cref="OrderedDitherer"/>, <see cref="ErrorDiffusionDitherer"/>, <see cref="RandomNoiseDitherer"/>
        /// and <see cref="InterleavedGradientNoiseDitherer"/> classes. All of them have several examples in their <strong>Remarks</strong> section.</item>
        /// </list></note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/>, <paramref name="quantizer"/> or <paramref name="ditherer"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException">The <see cref="IQuantizer.Initialize">IQuantizer.Initialize</see> method
        /// or the <see cref="IDitherer.Initialize">IDitherer.Initialize</see> method returned <see langword="null"/>.</exception>
        public static void Dither(this IReadWriteBitmapData bitmapData, IQuantizer quantizer, IDitherer ditherer) => bitmapData.Dither(AsyncHelper.DefaultContext, quantizer, ditherer);

        /// <summary>
        /// Quantizes an <see cref="IReadWriteBitmapData"/> with dithering (reduces the number of colors while trying to preserve details)
        /// using the specified <paramref name="quantizer"/> and <paramref name="ditherer"/>.
        /// </summary>
        /// <param name="bitmapData">An <see cref="IReadWriteBitmapData"/> instance to be quantized.</param>
        /// <param name="quantizer">An <see cref="IQuantizer"/> implementation to be used for quantizing the specified <paramref name="bitmapData"/>.</param>
        /// <param name="ditherer">An <see cref="IDitherer"/> implementation to be used for dithering during the quantization of the specified <paramref name="bitmapData"/>.</param>
        /// <param name="parallelConfig">The configuration of the operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.
        /// If <see langword="null"/>, then the degree of parallelization is configured automatically.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// of the <paramref name="parallelConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <note>This method blocks the caller as it executes synchronously, though the <paramref name="parallelConfig"/> parameter allows configuring the degree of parallelism,
        /// cancellation and progress reporting. Use the <see cref="BeginDither">BeginDither</see> or <see cref="DitherAsync">DitherAsync</see>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// <para>This method quantizes <paramref name="bitmapData"/> with dithering in place (its original content will be overwritten). To return a new instance
        /// use the <see cref="Clone(IReadableBitmapData, KnownPixelFormat, IQuantizer, IDitherer, ParallelConfig)">Clone</see> extension method instead.</para>
        /// <para>If the <see cref="KnownPixelFormat"/> or the palette of <paramref name="bitmapData"/> is not compatible with the colors of the specified <paramref name="quantizer"/>, then
        /// the result may not be correct.</para>
        /// <para>If <paramref name="bitmapData"/> has already the same set of colors that the specified <paramref name="quantizer"/>, then it can happen
        /// that calling this method does not change <paramref name="bitmapData"/> at all.</para>
        /// <note type="tip"><list type="bullet">
        /// <item>To use predefined colors or custom quantization functions use the static methods of the <see cref="PredefinedColorsQuantizer"/> class.
        /// <br/>See the <strong>Remarks</strong> section of its members for details and examples.</item>
        /// <item>To use an optimized palette of a specified number of colors adjusted for <paramref name="bitmapData"/> see the <see cref="OptimizedPaletteQuantizer"/> class.</item>
        /// <item>For some built-in dithering solutions see the <see cref="OrderedDitherer"/>, <see cref="ErrorDiffusionDitherer"/>, <see cref="RandomNoiseDitherer"/>
        /// and <see cref="InterleavedGradientNoiseDitherer"/> classes. All of them have several examples in their <strong>Remarks</strong> section.</item>
        /// </list></note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/>, <paramref name="quantizer"/> or <paramref name="ditherer"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException">The <see cref="IQuantizer.Initialize">IQuantizer.Initialize</see> method
        /// or the <see cref="IDitherer.Initialize">IDitherer.Initialize</see> method returned <see langword="null"/>.</exception>
        public static bool Dither(this IReadWriteBitmapData bitmapData, IQuantizer quantizer, IDitherer ditherer, ParallelConfig? parallelConfig)
        {
            // NOTE: The parallelConfig parameter could just be an additional optional parameter in the original overload but that would have been a breaking change.
            // Also, this overload has a bool return value, and there is a minimal overhead with the DoOperationSynchronously call.
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            if (quantizer == null)
                throw new ArgumentNullException(nameof(quantizer), PublicResources.ArgumentNull);
            if (ditherer == null)
                throw new ArgumentNullException(nameof(ditherer), PublicResources.ArgumentNull);

            return AsyncHelper.DoOperationSynchronously(ctx => DoDither(ctx, bitmapData, quantizer, ditherer), parallelConfig);
        }

        /// <summary>
        /// Quantizes an <see cref="IReadWriteBitmapData"/> with the specified <paramref name="quantizer"/> and <paramref name="ditherer"/>,
        /// using a <paramref name="context"/> that may belong to a higher level, possibly asynchronous operation.
        /// </summary>
        /// <param name="bitmapData">An <see cref="IReadWriteBitmapData"/> instance to be quantized.</param>
        /// <param name="context">An <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncContext.htm">IAsyncContext</a> instance
        /// that contains information for asynchronous processing about the current operation.</param>
        /// <param name="quantizer">An <see cref="IQuantizer"/> implementation to be used for quantizing the specified <paramref name="bitmapData"/>.</param>
        /// <param name="ditherer">An <see cref="IDitherer"/> implementation to be used for dithering during the quantization of the specified <paramref name="bitmapData"/>.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/>, <paramref name="quantizer"/> or <paramref name="ditherer"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException">The <see cref="IQuantizer.Initialize">IQuantizer.Initialize</see> method
        /// or the <see cref="IDitherer.Initialize">IDitherer.Initialize</see> method returned <see langword="null"/>.</exception>
        /// <remarks>
        /// <para>This method blocks the caller thread but if <paramref name="context"/> belongs to an async top level method, then the execution may already run
        /// on a pool thread. Degree of parallelism, the ability of cancellation and reporting progress depend on how these were configured at the top level method.
        /// To reconfigure the degree of parallelism of an existing context, you can use the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_AsyncContextWrapper.htm">AsyncContextWrapper</a> class.</para>
        /// <para>Alternatively, you can use this method to specify the degree of parallelism for synchronous execution. For example, by
        /// passing <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncHelper_SingleThreadContext.htm">AsyncHelper.SingleThreadContext</a> to the <paramref name="context"/> parameter
        /// the method will be forced to use a single thread only.</para>
        /// <para>When reporting progress, this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.</para>
        /// <note type="tip">See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_AsyncHelper.htm">AsyncHelper</a>
        /// class for details about how to create a context for possibly async top level methods.</note>
        /// <note>See the <see cref="Dither(IReadWriteBitmapData, IQuantizer, IDitherer)"/> overload for more details about the other parameters.</note>
        /// </remarks>
        public static bool Dither(this IReadWriteBitmapData bitmapData, IAsyncContext? context, IQuantizer quantizer, IDitherer ditherer)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            if (quantizer == null)
                throw new ArgumentNullException(nameof(quantizer), PublicResources.ArgumentNull);
            if (ditherer == null)
                throw new ArgumentNullException(nameof(ditherer), PublicResources.ArgumentNull);

            return DoDither(context ?? AsyncHelper.DefaultContext, bitmapData, quantizer, ditherer);
        }

        /// <summary>
        /// Begins to quantize an <see cref="IReadWriteBitmapData"/> with dithering asynchronously (reduces the number of colors while trying to preserve details)
        /// using the specified <paramref name="quantizer"/> and <paramref name="ditherer"/>.
        /// </summary>
        /// <param name="bitmapData">An <see cref="IReadWriteBitmapData"/> instance to be quantized.</param>
        /// <param name="quantizer">An <see cref="IQuantizer"/> implementation to be used for quantizing the specified <paramref name="bitmapData"/>.</param>
        /// <param name="ditherer">An <see cref="IDitherer"/> implementation to be used for dithering during the quantization of the specified <paramref name="bitmapData"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/>, <paramref name="quantizer"/> or <paramref name="ditherer"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="DitherAsync">DitherAsync</see> method.</para>
        /// <para>To finish the operation and to get the exception that occurred during the operation you have to call the <see cref="EndQuantize">EndQuantize</see> method.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="Dither(IReadWriteBitmapData, IQuantizer, IDitherer)">Dither</see> method for more details.</note>
        /// </remarks>
        public static IAsyncResult BeginDither(this IReadWriteBitmapData bitmapData, IQuantizer quantizer, IDitherer ditherer, AsyncConfig? asyncConfig = null)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            if (quantizer == null)
                throw new ArgumentNullException(nameof(quantizer), PublicResources.ArgumentNull);

            return AsyncHelper.BeginOperation(ctx => DoDither(ctx, bitmapData, quantizer, ditherer), asyncConfig);
        }

        /// <summary>
        /// Waits for the pending asynchronous operation started by the <see cref="BeginDither">BeginDither</see> method to complete.
        /// In .NET Framework 4.0 and above you can use the <see cref="DitherAsync">QuantizeAsync</see> method instead.
        /// </summary>
        /// <param name="asyncResult">The reference to the pending asynchronous request to finish.</param>
        /// <exception cref="InvalidOperationException">The <see cref="IQuantizer.Initialize">IQuantizer.Initialize</see> method
        /// or the <see cref="IDitherer.Initialize">IDitherer.Initialize</see> method returned <see langword="null"/>.</exception>
        public static void EndDither(this IAsyncResult asyncResult)
            // NOTE: the return value could be bool, but it would be a breaking change
            => AsyncHelper.EndOperation<bool>(asyncResult, nameof(BeginDither));

#if !NET35
        /// <summary>
        /// Quantizes an <see cref="IReadWriteBitmapData"/> with dithering asynchronously (reduces the number of colors while trying to preserve details)
        /// using the specified <paramref name="quantizer"/> and <paramref name="ditherer"/>.
        /// </summary>
        /// <param name="bitmapData">An <see cref="IReadWriteBitmapData"/> instance to be quantized.</param>
        /// <param name="quantizer">An <see cref="IQuantizer"/> implementation to be used for quantizing the specified <paramref name="bitmapData"/>.</param>
        /// <param name="ditherer">An <see cref="IDitherer"/> implementation to be used for dithering during the quantization of the specified <paramref name="bitmapData"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/>, <paramref name="quantizer"/> or <paramref name="ditherer"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException">The <see cref="IQuantizer.Initialize">IQuantizer.Initialize</see> method
        /// or the <see cref="IDitherer.Initialize">IDitherer.Initialize</see> method returned <see langword="null"/>.</exception>
        /// <remarks>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="Dither(IReadWriteBitmapData, IQuantizer, IDitherer)">Dither</see> method for more details.</note>
        /// </remarks>
        public static Task DitherAsync(this IReadWriteBitmapData bitmapData, IQuantizer quantizer, IDitherer ditherer, TaskConfig? asyncConfig = null)
        {
            // NOTE: the return value could be Task<bool> but it would be a breaking change
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            if (quantizer == null)
                throw new ArgumentNullException(nameof(quantizer), PublicResources.ArgumentNull);

            return AsyncHelper.DoOperationAsync(ctx => DoDither(ctx, bitmapData, quantizer, ditherer), asyncConfig);
        }
#endif

        #endregion

        #region TransformColors

        #region Color32

        /// <summary>
        /// Transforms the colors of this <paramref name="bitmapData"/> using the specified <paramref name="transformFunction"/> delegate.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to be transformed.</param>
        /// <param name="transformFunction">The transform function to be used on the colors of the specified <paramref name="bitmapData"/>. It must be thread-safe.</param>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. You can use
        /// the <see cref="TransformColors(IReadWriteBitmapData, Func{Color32,Color32}, IDitherer, ParallelConfig)"/> overload to configure these, while still executing the method synchronously.
        /// Alternatively, use the <see cref="BeginTransformColors(IReadWriteBitmapData, Func{Color32,Color32}, IDitherer, AsyncConfig)"/>
        /// or <see cref="TransformColorsAsync(IReadWriteBitmapData, Func{Color32,Color32}, IDitherer, TaskConfig)"/> (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// <para>This method transforms the <paramref name="bitmapData"/> in place (its original content will be overwritten). To return a new instance
        /// use the <see cref="Clone(IReadableBitmapData,KnownPixelFormat,IQuantizer,IDitherer)">Clone</see> extension method
        /// with an <see cref="IQuantizer"/> instance created by the <see cref="PredefinedColorsQuantizer.FromCustomFunction(Func{Color32,Color32},KnownPixelFormat)">PredefinedColorsQuantizer.FromCustomFunction</see> method.</para>
        /// <para>If <paramref name="bitmapData"/> has an indexed <see cref="IBitmapData.PixelFormat"/> and it supports setting the <see cref="IBitmapData.Palette"/>, then its palette entries will be transformed instead of the actual pixels.</para>
        /// <para>On multicore systems <paramref name="transformFunction"/> might be called concurrently so it must be thread-safe.</para>
        /// <note type="tip">If <paramref name="transformFunction"/> can return colors incompatible with the pixel format of the specified <paramref name="bitmapData"/>, or you want to transform the actual
        /// pixels of an indexed <paramref name="bitmapData"/> instead of modifying the palette, then use the <see cref="TransformColors(IReadWriteBitmapData,Func{Color32,Color32},IDitherer)"/> overload and specify an <see cref="IDitherer"/> instance.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> or <paramref name="transformFunction"/> is <see langword="null"/>.</exception>
        public static void TransformColors(this IReadWriteBitmapData bitmapData, Func<Color32, Color32> transformFunction)
        {
            ValidateArguments(bitmapData, transformFunction);
            DoTransformColors(AsyncHelper.DefaultContext, bitmapData, transformFunction);
        }

        /// <summary>
        /// Transforms the colors of this <paramref name="bitmapData"/> using the specified <paramref name="transformFunction"/> delegate.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to be transformed.</param>
        /// <param name="transformFunction">The transform function to be used on the colors of the specified <paramref name="bitmapData"/>. It must be thread-safe.</param>
        /// <param name="ditherer">An optional <see cref="IDitherer"/> instance to dither the result of the transformation if <paramref name="transformFunction"/> returns colors
        /// that is not compatible with the <see cref="IBitmapData.PixelFormat"/> of the specified <paramref name="bitmapData"/>.</param>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. You can use
        /// the <see cref="TransformColors(IReadWriteBitmapData, Func{Color32,Color32}, IDitherer, ParallelConfig)"/> overload to configure these, while still executing the method synchronously.
        /// Alternatively, use the <see cref="BeginTransformColors(IReadWriteBitmapData, Func{Color32,Color32}, IDitherer, AsyncConfig)"/>
        /// or <see cref="TransformColorsAsync(IReadWriteBitmapData, Func{Color32,Color32}, IDitherer, TaskConfig)"/> (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// <para>This method transforms the <paramref name="bitmapData"/> in place (its original content will be overwritten). To return a new instance
        /// use the <see cref="Clone(IReadableBitmapData,KnownPixelFormat,IQuantizer,IDitherer)">Clone</see> extension method
        /// with an <see cref="IQuantizer"/> instance created by the <see cref="PredefinedColorsQuantizer.FromCustomFunction(Func{Color32,Color32},KnownPixelFormat)">PredefinedColorsQuantizer.FromCustomFunction</see> method.</para>
        /// <para>If <paramref name="bitmapData"/> has an indexed <see cref="IBitmapData.PixelFormat"/> and <paramref name="ditherer"/> is <see langword="null"/>,
        /// then its palette entries are tried to be transformed instead of the actual pixels in the first place (if it is supported by <paramref name="bitmapData"/>).
        /// To transform the colors of an indexed <see cref="IBitmapData"/> without changing the palette specify a non-<see langword="null"/>&#160;<paramref name="ditherer"/>.
        /// Transforming the palette is both faster and provides a better result.</para>
        /// <para>On multicore systems <paramref name="transformFunction"/> might be called concurrently so it must be thread-safe.</para>
        /// <para>The <paramref name="ditherer"/> is ignored for <see cref="KnownPixelFormat"/>s with more than 16 bits-per-pixel and for grayscale formats.</para>
        /// <note>See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_TransformColors.htm">BitmapExtensions.TransformColors</a> method for an example.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> or <paramref name="transformFunction"/> is <see langword="null"/>.</exception>
        public static void TransformColors(this IReadWriteBitmapData bitmapData, Func<Color32, Color32> transformFunction, IDitherer? ditherer)
            => bitmapData.TransformColors(AsyncHelper.DefaultContext, transformFunction, ditherer);

        /// <summary>
        /// Transforms the colors of this <paramref name="bitmapData"/> using the specified <paramref name="transformFunction"/> delegate.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to be transformed.</param>
        /// <param name="transformFunction">The transform function to be used on the colors of the specified <paramref name="bitmapData"/>. It must be thread-safe.</param>
        /// <param name="ditherer">An optional <see cref="IDitherer"/> instance to dither the result of the transformation if <paramref name="transformFunction"/> returns colors
        /// that is not compatible with the <see cref="IBitmapData.PixelFormat"/> of the specified <paramref name="bitmapData"/>.</param>
        /// <param name="parallelConfig">The configuration of the operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.
        /// If <see langword="null"/>, then the degree of parallelization is configured automatically.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// of the <paramref name="parallelConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <note>This method blocks the caller as it executes synchronously, though the <paramref name="parallelConfig"/> parameter allows configuring the degree of parallelism,
        /// cancellation and progress reporting. Use the <see cref="BeginTransformColors(IReadWriteBitmapData, Func{Color32,Color32}, IDitherer, AsyncConfig)"/>
        /// or <see cref="TransformColorsAsync(IReadWriteBitmapData, Func{Color32,Color32}, IDitherer, TaskConfig)"/> (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// <para>This method transforms the <paramref name="bitmapData"/> in place (its original content will be overwritten). To return a new instance
        /// use the <see cref="Clone(IReadableBitmapData,KnownPixelFormat,IQuantizer,IDitherer)">Clone</see> extension method
        /// with an <see cref="IQuantizer"/> instance created by the <see cref="PredefinedColorsQuantizer.FromCustomFunction(Func{Color32,Color32},KnownPixelFormat)">PredefinedColorsQuantizer.FromCustomFunction</see> method.</para>
        /// <para>If <paramref name="bitmapData"/> has an indexed <see cref="IBitmapData.PixelFormat"/> and <paramref name="ditherer"/> is <see langword="null"/>,
        /// then its palette entries are tried to be transformed instead of the actual pixels in the first place (if it is supported by <paramref name="bitmapData"/>).
        /// To transform the colors of an indexed <see cref="IBitmapData"/> without changing the palette specify a non-<see langword="null"/>&#160;<paramref name="ditherer"/>.
        /// Transforming the palette is both faster and provides a better result.</para>
        /// <para>On multicore systems <paramref name="transformFunction"/> might be called concurrently so it must be thread-safe.</para>
        /// <para>The <paramref name="ditherer"/> is ignored for <see cref="KnownPixelFormat"/>s with more than 16 bits-per-pixel and for grayscale formats.</para>
        /// <note>See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_TransformColors.htm">BitmapExtensions.TransformColors</a> method for an example.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> or <paramref name="transformFunction"/> is <see langword="null"/>.</exception>
        public static bool TransformColors(this IReadWriteBitmapData bitmapData, Func<Color32, Color32> transformFunction, IDitherer? ditherer, ParallelConfig? parallelConfig)
        {
            // NOTE: The parallelConfig parameter could just be an additional optional parameter in the original overloads but that would have been a breaking change.
            // This overload has no default parameters to prevent auto switching the callers to this one instead of the original one.
            // Even though it would be compile-compatible, it's still breaking. Also, this one has a bool return value, and there is a minimal overhead with the DoOperationSynchronously call.
            ValidateArguments(bitmapData, transformFunction);
            return AsyncHelper.DoOperationSynchronously(ctx => DoTransformColors(ctx, bitmapData, transformFunction, ditherer), parallelConfig);
        }

        /// <summary>
        /// Transforms the colors of this <paramref name="bitmapData"/> using the specified <paramref name="transformFunction"/> delegate
        /// and a <paramref name="context"/> that may belong to a higher level, possibly asynchronous operation.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to be transformed.</param>
        /// <param name="context">An <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncContext.htm">IAsyncContext</a> instance
        /// that contains information for asynchronous processing about the current operation.</param>
        /// <param name="transformFunction">The transform function to be used on the colors of the specified <paramref name="bitmapData"/>. It must be thread-safe.</param>
        /// <param name="ditherer">An optional <see cref="IDitherer"/> instance to dither the result of the transformation if <paramref name="transformFunction"/> returns colors
        /// that is not compatible with the <see cref="IBitmapData.PixelFormat"/> of the specified <paramref name="bitmapData"/>.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> or <paramref name="transformFunction"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// <para>This method blocks the caller thread but if <paramref name="context"/> belongs to an async top level method, then the execution may already run
        /// on a pool thread. Degree of parallelism, the ability of cancellation and reporting progress depend on how these were configured at the top level method.
        /// To reconfigure the degree of parallelism of an existing context, you can use the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_AsyncContextWrapper.htm">AsyncContextWrapper</a> class.</para>
        /// <para>Alternatively, you can use this method to specify the degree of parallelism for synchronous execution. For example, by
        /// passing <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncHelper_SingleThreadContext.htm">AsyncHelper.SingleThreadContext</a> to the <paramref name="context"/> parameter
        /// the method will be forced to use a single thread only.</para>
        /// <para>When reporting progress, this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.</para>
        /// <note type="tip">See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_AsyncHelper.htm">AsyncHelper</a>
        /// class for details about how to create a context for possibly async top level methods.</note>
        /// <note>See the <see cref="TransformColors(IReadWriteBitmapData, Func{Color32, Color32}, IDitherer?)"/> overload for more details about the other parameters.</note>
        /// </remarks>
        public static bool TransformColors(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Func<Color32, Color32> transformFunction, IDitherer? ditherer)
        {
            ValidateArguments(bitmapData, transformFunction);
            return DoTransformColors(context ?? AsyncHelper.DefaultContext, bitmapData, transformFunction, ditherer);
        }

        /// <summary>
        /// Begins to transform the colors of this <paramref name="bitmapData"/> asynchronously, using the specified <paramref name="transformFunction"/> delegate.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to be transformed.</param>
        /// <param name="transformFunction">The transform function to be used on the colors of the specified <paramref name="bitmapData"/>. It must be thread-safe.</param>
        /// <param name="ditherer">An optional <see cref="IDitherer"/> instance to dither the result of the transformation if <paramref name="transformFunction"/> returns colors
        /// that is not compatible with the <see cref="IBitmapData.PixelFormat"/> of the specified <paramref name="bitmapData"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> or <paramref name="transformFunction"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="TransformColorsAsync(IReadWriteBitmapData, Func{Color32,Color32}, IDitherer, TaskConfig)"/> method.</para>
        /// <para>To finish the operation and to get the exception that occurred during the operation you have to call the <see cref="EndTransformColors">EndTransformColors</see> method.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="TransformColors(IReadWriteBitmapData, Func{Color32, Color32}, IDitherer)"/> method for more details.</note>
        /// </remarks>
        public static IAsyncResult BeginTransformColors(this IReadWriteBitmapData bitmapData, Func<Color32, Color32> transformFunction, IDitherer? ditherer = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, transformFunction);
            return AsyncHelper.BeginOperation(ctx => DoTransformColors(ctx, bitmapData, transformFunction, ditherer), asyncConfig);
        }

        /// <summary>
        /// Waits for the pending asynchronous operation started by the one of the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.BeginTransformColors">BeginTransformColors</see> overloads to complete.
        /// In .NET Framework 4.0 and above you can use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.TransformColorsAsync">TransformColorsAsync</see> methods instead.
        /// </summary>
        /// <param name="asyncResult">The reference to the pending asynchronous request to finish.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in the <c>asyncConfig</c> parameter was set to <see langword="false"/>.</returns>
        public static bool EndTransformColors(this IAsyncResult asyncResult) => AsyncHelper.EndOperation<bool>(asyncResult, nameof(BeginTransformColors));

#if !NET35
        /// <summary>
        /// Transforms the colors of this <paramref name="bitmapData"/> asynchronously, using the specified <paramref name="transformFunction"/> delegate.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to be transformed.</param>
        /// <param name="transformFunction">The transform function to be used on the colors of the specified <paramref name="bitmapData"/>. It must be thread-safe.</param>
        /// <param name="ditherer">An optional <see cref="IDitherer"/> instance to dither the result of the transformation if <paramref name="transformFunction"/> returns colors
        /// that is not compatible with the <see cref="IBitmapData.PixelFormat"/> of the specified <paramref name="bitmapData"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> or <paramref name="transformFunction"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="TransformColors(IReadWriteBitmapData, Func{Color32, Color32}, IDitherer)"/> method for more details.</note>
        /// </remarks>
        public static Task<bool> TransformColorsAsync(this IReadWriteBitmapData bitmapData, Func<Color32, Color32> transformFunction, IDitherer? ditherer = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, transformFunction);
            return AsyncHelper.DoOperationAsync(ctx => DoTransformColors(ctx, bitmapData, transformFunction, ditherer), asyncConfig);
        }
#endif

        #endregion

        #region Color64

        /// <summary>
        /// Transforms the colors of this <paramref name="bitmapData"/> using the specified <paramref name="transformFunction"/> delegate.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to be transformed.</param>
        /// <param name="transformFunction">The transform function to be used on the colors of the specified <paramref name="bitmapData"/>. It must be thread-safe.</param>
        /// <param name="parallelConfig">The configuration of the operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.
        /// If <see langword="null"/>, then the degree of parallelization is configured automatically. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// of the <paramref name="parallelConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <note>This method blocks the caller as it executes synchronously, though the <paramref name="parallelConfig"/> parameter allows configuring the degree of parallelism,
        /// cancellation and progress reporting. Use the <see cref="BeginTransformColors(IReadWriteBitmapData, Func{Color64,Color64}, AsyncConfig)"/>
        /// or <see cref="TransformColorsAsync(IReadWriteBitmapData, Func{Color64,Color64}, TaskConfig)"/> (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// <para>If <paramref name="bitmapData"/> has an indexed <see cref="IBitmapData.PixelFormat"/> and it supports setting the <see cref="IBitmapData.Palette"/>, then its palette entries will be transformed instead of the actual pixels.
        /// Though for indexed formats it's preferred to use the <see cref="TransformColors(IReadWriteBitmapData,Func{Color32,Color32})"/> overload instead.</para>
        /// <para>On multicore systems <paramref name="transformFunction"/> might be called concurrently so it must be thread-safe.</para>
        /// <note>See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_TransformColors.htm">BitmapExtensions.TransformColors</a> method for an example.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> or <paramref name="transformFunction"/> is <see langword="null"/>.</exception>
        public static bool TransformColors(this IReadWriteBitmapData bitmapData, Func<Color64, Color64> transformFunction, ParallelConfig? parallelConfig = null)
        {
            ValidateArguments(bitmapData, transformFunction);
            return AsyncHelper.DoOperationSynchronously(ctx => DoTransformColors(ctx, bitmapData, transformFunction), parallelConfig);
        }

        /// <summary>
        /// Transforms the colors of this <paramref name="bitmapData"/> using the specified <paramref name="transformFunction"/> delegate
        /// and a <paramref name="context"/> that may belong to a higher level, possibly asynchronous operation.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to be transformed.</param>
        /// <param name="context">An <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncContext.htm">IAsyncContext</a> instance
        /// that contains information for asynchronous processing about the current operation.</param>
        /// <param name="transformFunction">The transform function to be used on the colors of the specified <paramref name="bitmapData"/>. It must be thread-safe.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> or <paramref name="transformFunction"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// <para>This method blocks the caller thread but if <paramref name="context"/> belongs to an async top level method, then the execution may already run
        /// on a pool thread. Degree of parallelism, the ability of cancellation and reporting progress depend on how these were configured at the top level method.
        /// To reconfigure the degree of parallelism of an existing context, you can use the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_AsyncContextWrapper.htm">AsyncContextWrapper</a> class.</para>
        /// <para>Alternatively, you can use this method to specify the degree of parallelism for synchronous execution. For example, by
        /// passing <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncHelper_SingleThreadContext.htm">AsyncHelper.SingleThreadContext</a> to the <paramref name="context"/> parameter
        /// the method will be forced to use a single thread only.</para>
        /// <para>When reporting progress, this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.</para>
        /// <note type="tip">See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_AsyncHelper.htm">AsyncHelper</a>
        /// class for details about how to create a context for possibly async top level methods.</note>
        /// </remarks>
        public static bool TransformColors(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Func<Color64, Color64> transformFunction)
        {
            ValidateArguments(bitmapData, transformFunction);
            return DoTransformColors(context ?? AsyncHelper.DefaultContext, bitmapData, transformFunction);
        }

        /// <summary>
        /// Begins to transform the colors of this <paramref name="bitmapData"/> asynchronously, using the specified <paramref name="transformFunction"/> delegate.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to be transformed.</param>
        /// <param name="transformFunction">The transform function to be used on the colors of the specified <paramref name="bitmapData"/>. It must be thread-safe.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> or <paramref name="transformFunction"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="TransformColorsAsync(IReadWriteBitmapData, Func{Color64,Color64}, TaskConfig)"/> method.</para>
        /// <para>To finish the operation and to get the exception that occurred during the operation you have to call the <see cref="EndTransformColors">EndTransformColors</see> method.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        public static IAsyncResult BeginTransformColors(this IReadWriteBitmapData bitmapData, Func<Color64, Color64> transformFunction, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, transformFunction);
            return AsyncHelper.BeginOperation(ctx => DoTransformColors(ctx, bitmapData, transformFunction), asyncConfig);
        }

#if !NET35
        /// <summary>
        /// Transforms the colors of this <paramref name="bitmapData"/> asynchronously, using the specified <paramref name="transformFunction"/> delegate.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to be transformed.</param>
        /// <param name="transformFunction">The transform function to be used on the colors of the specified <paramref name="bitmapData"/>. It must be thread-safe.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> or <paramref name="transformFunction"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        public static Task<bool> TransformColorsAsync(this IReadWriteBitmapData bitmapData, Func<Color64, Color64> transformFunction, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, transformFunction);
            return AsyncHelper.DoOperationAsync(ctx => DoTransformColors(ctx, bitmapData, transformFunction), asyncConfig);
        }
#endif

        #endregion

        #region ColorF

        /// <summary>
        /// Transforms the colors of this <paramref name="bitmapData"/> using the specified <paramref name="transformFunction"/> delegate.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to be transformed.</param>
        /// <param name="transformFunction">The transform function to be used on the colors of the specified <paramref name="bitmapData"/>. It must be thread-safe.</param>
        /// <param name="parallelConfig">The configuration of the operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.
        /// If <see langword="null"/>, then the degree of parallelization is configured automatically.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// of the <paramref name="parallelConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <note>This method blocks the caller as it executes synchronously, though the <paramref name="parallelConfig"/> parameter allows configuring the degree of parallelism,
        /// cancellation and progress reporting. Use the <see cref="BeginTransformColors(IReadWriteBitmapData, Func{ColorF,ColorF}, AsyncConfig)"/>
        /// or <see cref="TransformColorsAsync(IReadWriteBitmapData, Func{ColorF,ColorF}, TaskConfig)"/> (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// <para>If <paramref name="bitmapData"/> has an indexed <see cref="IBitmapData.PixelFormat"/> and it supports setting the <see cref="IBitmapData.Palette"/>, then its palette entries will be transformed instead of the actual pixels.
        /// Though for indexed formats it's preferred to use the <see cref="TransformColors(IReadWriteBitmapData,Func{Color32,Color32})"/> overload instead.</para>
        /// <para>On multicore systems <paramref name="transformFunction"/> might be called concurrently so it must be thread-safe.</para>
        /// <note>See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_TransformColors.htm">BitmapExtensions.TransformColors</a> method for an example.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> or <paramref name="transformFunction"/> is <see langword="null"/>.</exception>
        public static bool TransformColors(this IReadWriteBitmapData bitmapData, Func<ColorF, ColorF> transformFunction, ParallelConfig? parallelConfig)
        {
            // NOTE: The parallelConfig parameter could just be an additional optional parameter in the original overloads but that would have been a breaking change.
            // This overload has no default parameters to prevent auto switching the callers to this one instead of the original one.
            // Even though it would be compile-compatible, it's still breaking. Also, this one has a bool return value, and there is a minimal overhead with the DoOperationSynchronously call.
            ValidateArguments(bitmapData, transformFunction);
            return AsyncHelper.DoOperationSynchronously(ctx => DoTransformColors(ctx, bitmapData, transformFunction), parallelConfig);
        }

        /// <summary>
        /// Transforms the colors of this <paramref name="bitmapData"/> using the specified <paramref name="transformFunction"/> delegate
        /// and a <paramref name="context"/> that may belong to a higher level, possibly asynchronous operation.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to be transformed.</param>
        /// <param name="context">An <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncContext.htm">IAsyncContext</a> instance
        /// that contains information for asynchronous processing about the current operation.</param>
        /// <param name="transformFunction">The transform function to be used on the colors of the specified <paramref name="bitmapData"/>. It must be thread-safe.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> or <paramref name="transformFunction"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// <para>This method blocks the caller thread but if <paramref name="context"/> belongs to an async top level method, then the execution may already run
        /// on a pool thread. Degree of parallelism, the ability of cancellation and reporting progress depend on how these were configured at the top level method.
        /// To reconfigure the degree of parallelism of an existing context, you can use the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_AsyncContextWrapper.htm">AsyncContextWrapper</a> class.</para>
        /// <para>Alternatively, you can use this method to specify the degree of parallelism for synchronous execution. For example, by
        /// passing <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncHelper_SingleThreadContext.htm">AsyncHelper.SingleThreadContext</a> to the <paramref name="context"/> parameter
        /// the method will be forced to use a single thread only.</para>
        /// <para>When reporting progress, this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.</para>
        /// <note type="tip">See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_AsyncHelper.htm">AsyncHelper</a>
        /// class for details about how to create a context for possibly async top level methods.</note>
        /// </remarks>
        public static bool TransformColors(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Func<ColorF, ColorF> transformFunction)
        {
            ValidateArguments(bitmapData, transformFunction);
            return DoTransformColors(context ?? AsyncHelper.DefaultContext, bitmapData, transformFunction);
        }

        /// <summary>
        /// Begins to transform the colors of this <paramref name="bitmapData"/> asynchronously, using the specified <paramref name="transformFunction"/> delegate.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to be transformed.</param>
        /// <param name="transformFunction">The transform function to be used on the colors of the specified <paramref name="bitmapData"/>. It must be thread-safe.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> or <paramref name="transformFunction"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="TransformColorsAsync(IReadWriteBitmapData, Func{ColorF,ColorF}, TaskConfig)"/> method.</para>
        /// <para>To finish the operation and to get the exception that occurred during the operation you have to call the <see cref="EndTransformColors">EndTransformColors</see> method.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        public static IAsyncResult BeginTransformColors(this IReadWriteBitmapData bitmapData, Func<ColorF, ColorF> transformFunction, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, transformFunction);
            return AsyncHelper.BeginOperation(ctx => DoTransformColors(ctx, bitmapData, transformFunction), asyncConfig);
        }

#if !NET35
        /// <summary>
        /// Transforms the colors of this <paramref name="bitmapData"/> asynchronously, using the specified <paramref name="transformFunction"/> delegate.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to be transformed.</param>
        /// <param name="transformFunction">The transform function to be used on the colors of the specified <paramref name="bitmapData"/>. It must be thread-safe.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> or <paramref name="transformFunction"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        public static Task<bool> TransformColorsAsync(this IReadWriteBitmapData bitmapData, Func<ColorF, ColorF> transformFunction, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, transformFunction);
            return AsyncHelper.DoOperationAsync(ctx => DoTransformColors(ctx, bitmapData, transformFunction), asyncConfig);
        }
#endif

        #endregion

        #endregion

        #region ReplaceColor

        /// <summary>
        /// Replaces every <paramref name="oldColor"/> occurrences to <paramref name="newColor"/> in the specified <paramref name="bitmapData"/>.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to be transformed.</param>
        /// <param name="oldColor">The original color to be replaced.</param>
        /// <param name="newColor">The new color to replace <paramref name="oldColor"/> with.</param>
        /// <param name="ditherer">An optional <see cref="IDitherer"/> instance to dither the result if <paramref name="newColor"/>
        /// is not compatible with the <see cref="IBitmapData.PixelFormat"/> of the specified <paramref name="bitmapData"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. You can use
        /// the <see cref="ReplaceColor(IReadWriteBitmapData, Color32, Color32, IDitherer, ParallelConfig)"/> overload to configure these, while still executing the method synchronously.
        /// Alternatively, use the <see cref="BeginReplaceColor">BeginReplaceColor</see> or <see cref="ReplaceColorAsync">ReplaceColorAsync</see> (in .NET Framework 4.0 and above) methods for asynchronous call and to adjust parallelization, set up cancellation and for reporting progress.</note>
        /// <para>If <paramref name="newColor"/> has alpha, which cannot be represented by <paramref name="bitmapData"/>, then it will be blended with <see cref="IBitmapData.BackColor"/>.</para>
        /// <para>If <paramref name="bitmapData"/> has an indexed <see cref="IBitmapData.PixelFormat"/> and <paramref name="ditherer"/> is <see langword="null"/>,
        /// then its palette entries are tried to be transformed instead of the actual pixels in the first place (if it is supported by <paramref name="bitmapData"/>).
        /// To transform the colors of an indexed <see cref="IBitmapData"/> without changing the palette specify a non-<see langword="null"/>&#160;<paramref name="ditherer"/>.
        /// Transforming the palette is both faster and provides a better result.</para>
        /// <para>If <paramref name="ditherer"/> is <see langword="null"/>, this method attempts to preserve the original color depth, including wide pixel formats.
        /// Otherwise, this method may quantize the colors to the 32-bit color space, even if no occurrence of <paramref name="oldColor"/> is found.
        /// To replace colors that cannot be represented by a <see cref="Color32"/> struct, use the <see cref="TransformColors(IReadWriteBitmapData, Func{Color64, Color64}, ParallelConfig)"/>
        /// or <see cref="TransformColors(IReadWriteBitmapData, Func{ColorF, ColorF}, ParallelConfig)"/> methods instead.</para>
        /// <para>The <paramref name="ditherer"/> may have no effect for <see cref="KnownPixelFormat"/>s with more than 16 bits-per-pixel and for grayscale formats.</para>
        /// </remarks>
        public static void ReplaceColor(this IReadWriteBitmapData bitmapData, Color32 oldColor, Color32 newColor, IDitherer? ditherer = null)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            if (oldColor == newColor)
                return;

            DoReplaceColor(AsyncHelper.DefaultContext, bitmapData, oldColor, newColor, ditherer);
        }

        /// <summary>
        /// Replaces every <paramref name="oldColor"/> occurrences to <paramref name="newColor"/> in the specified <paramref name="bitmapData"/>.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to be transformed.</param>
        /// <param name="oldColor">The original color to be replaced.</param>
        /// <param name="newColor">The new color to replace <paramref name="oldColor"/> with.</param>
        /// <param name="ditherer">An optional <see cref="IDitherer"/> instance to dither the result if <paramref name="newColor"/>
        /// is not compatible with the <see cref="IBitmapData.PixelFormat"/> of the specified <paramref name="bitmapData"/>.</param>
        /// <param name="parallelConfig">The configuration of the operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.
        /// If <see langword="null"/>, then the degree of parallelization is configured automatically.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// of the <paramref name="parallelConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <note>This method blocks the caller as it executes synchronously, though the <paramref name="parallelConfig"/> parameter allows configuring the degree of parallelism,
        /// cancellation and progress reporting. Use the <see cref="BeginReplaceColor">BeginReplaceColor</see> or <see cref="ReplaceColorAsync">ReplaceColorAsync</see> (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// <para>If <paramref name="newColor"/> has alpha, which cannot be represented by <paramref name="bitmapData"/>, then it will be blended with <see cref="IBitmapData.BackColor"/>.</para>
        /// <para>If <paramref name="bitmapData"/> has an indexed <see cref="IBitmapData.PixelFormat"/> and <paramref name="ditherer"/> is <see langword="null"/>,
        /// then its palette entries are tried to be transformed instead of the actual pixels in the first place (if it is supported by <paramref name="bitmapData"/>).
        /// To transform the colors of an indexed <see cref="IBitmapData"/> without changing the palette specify a non-<see langword="null"/>&#160;<paramref name="ditherer"/>.
        /// Transforming the palette is both faster and provides a better result.</para>
        /// <para>If <paramref name="ditherer"/> is <see langword="null"/>, this method attempts to preserve the original color depth, including wide pixel formats.
        /// Otherwise, this method may quantize the colors to the 32-bit color space, even if no occurrence of <paramref name="oldColor"/> is found.
        /// To replace colors that cannot be represented by a <see cref="Color32"/> struct, use the <see cref="TransformColors(IReadWriteBitmapData, Func{Color64, Color64}, ParallelConfig)"/>
        /// or <see cref="TransformColors(IReadWriteBitmapData, Func{ColorF, ColorF}, ParallelConfig)"/> methods instead.</para>
        /// <para>The <paramref name="ditherer"/> may have no effect for <see cref="KnownPixelFormat"/>s with more than 16 bits-per-pixel and for grayscale formats.</para>
        /// </remarks>
        public static bool ReplaceColor(this IReadWriteBitmapData bitmapData, Color32 oldColor, Color32 newColor, IDitherer? ditherer, ParallelConfig? parallelConfig)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            if (oldColor == newColor)
                return AsyncHelper.FromResult(true, parallelConfig);

            return AsyncHelper.DoOperationSynchronously(ctx => DoReplaceColor(ctx, bitmapData, oldColor, newColor, ditherer), parallelConfig);
        }

        /// <summary>
        /// Replaces every <paramref name="oldColor"/> occurrences to <paramref name="newColor"/> in the specified <paramref name="bitmapData"/>, using a <paramref name="context"/> that may belong to a higher level, possibly asynchronous operation.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to be transformed.</param>
        /// <param name="context">An <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncContext.htm">IAsyncContext</a> instance
        /// that contains information for asynchronous processing about the current operation.</param>
        /// <param name="oldColor">The original color to be replaced.</param>
        /// <param name="newColor">The new color to replace <paramref name="oldColor"/> with.</param>
        /// <param name="ditherer">An optional <see cref="IDitherer"/> instance to dither the result if <paramref name="newColor"/>
        /// is not compatible with the <see cref="IBitmapData.PixelFormat"/> of the specified <paramref name="bitmapData"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled.</returns>
        /// <remarks>
        /// <para>This method blocks the caller thread but if <paramref name="context"/> belongs to an async top level method, then the execution may already run
        /// on a pool thread. Degree of parallelism, the ability of cancellation and reporting progress depend on how these were configured at the top level method.
        /// To reconfigure the degree of parallelism of an existing context, you can use the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_AsyncContextWrapper.htm">AsyncContextWrapper</a> class.</para>
        /// <para>Alternatively, you can use this method to specify the degree of parallelism for synchronous execution. For example, by
        /// passing <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncHelper_SingleThreadContext.htm">AsyncHelper.SingleThreadContext</a> to the <paramref name="context"/> parameter
        /// the method will be forced to use a single thread only.</para>
        /// <para>When reporting progress, this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.</para>
        /// <note type="tip">See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_AsyncHelper.htm">AsyncHelper</a>
        /// class for details about how to create a context for possibly async top level methods.</note>
        /// <note>See the <see cref="ReplaceColor(IReadWriteBitmapData, Color32, Color32, IDitherer?)"/> overload for more details about the other parameters.</note>
        /// </remarks>
        public static bool ReplaceColor(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Color32 oldColor, Color32 newColor, IDitherer? ditherer = null)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            if (oldColor == newColor)
                return true;

            return DoReplaceColor(context ?? AsyncHelper.DefaultContext, bitmapData, oldColor, newColor, ditherer);
        }

        /// <summary>
        /// Begins to replace every <paramref name="oldColor"/> occurrences to <paramref name="newColor"/> in the specified <paramref name="bitmapData"/> asynchronously.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to be transformed.</param>
        /// <param name="oldColor">The original color to be replaced.</param>
        /// <param name="newColor">The new color to replace <paramref name="oldColor"/> with.</param>
        /// <param name="ditherer">An optional <see cref="IDitherer"/> instance to dither the result if <paramref name="newColor"/>
        /// is not compatible with the <see cref="IBitmapData.PixelFormat"/> of the specified <paramref name="bitmapData"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="ReplaceColorAsync">ReplaceColorAsync</see> method.</para>
        /// <para>To finish the operation and to get the exception that occurred during the operation you have to call the <see cref="EndReplaceColor">EndReplaceColor</see> method.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="ReplaceColor(IReadWriteBitmapData,Color32,Color32,IDitherer?)">ReplaceColor</see> method for more details.</note>
        /// </remarks>
        public static IAsyncResult BeginReplaceColor(this IReadWriteBitmapData bitmapData, Color32 oldColor, Color32 newColor, IDitherer? ditherer = null, AsyncConfig? asyncConfig = null)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            if (oldColor == newColor)
                return AsyncHelper.FromResult(true, asyncConfig);

            return AsyncHelper.BeginOperation(ctx => DoReplaceColor(ctx, bitmapData, oldColor, newColor, ditherer), asyncConfig);
        }

        /// <summary>
        /// Waits for the pending asynchronous operation started by the <see cref="BeginReplaceColor">BeginReplaceColor</see> method to complete.
        /// In .NET Framework 4.0 and above you can use the <see cref="ReplaceColorAsync">ReplaceColorAsync</see> method instead.
        /// </summary>
        /// <param name="asyncResult">The reference to the pending asynchronous request to finish.</param>
        public static bool EndReplaceColor(this IAsyncResult asyncResult) => AsyncHelper.EndOperation<bool>(asyncResult, nameof(BeginReplaceColor));

#if !NET35
        /// <summary>
        /// Replaces every <paramref name="oldColor"/> occurrences to <paramref name="newColor"/> in the specified <paramref name="bitmapData"/> asynchronously.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to be transformed.</param>
        /// <param name="oldColor">The original color to be replaced.</param>
        /// <param name="newColor">The new color to replace <paramref name="oldColor"/> with.</param>
        /// <param name="ditherer">An optional <see cref="IDitherer"/> instance to dither the result if <paramref name="newColor"/>
        /// is not compatible with the <see cref="IBitmapData.PixelFormat"/> of the specified <paramref name="bitmapData"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="ReplaceColor(IReadWriteBitmapData,Color32,Color32,IDitherer?)">ReplaceColor</see> method for more details.</note>
        /// </remarks>
        public static Task<bool> ReplaceColorAsync(this IReadWriteBitmapData bitmapData, Color32 oldColor, Color32 newColor, IDitherer? ditherer = null, TaskConfig? asyncConfig = null)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            if (oldColor == newColor)
                return AsyncHelper.FromResult(true, asyncConfig);

            return AsyncHelper.DoOperationAsync(ctx => DoReplaceColor(ctx, bitmapData, oldColor, newColor, ditherer), asyncConfig);
        }
#endif

        #endregion

        #region Invert

        /// <summary>
        /// Inverts the colors of the specified <paramref name="bitmapData"/>.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to be inverted.</param>
        /// <param name="ditherer">An optional <see cref="IDitherer"/> instance to dither the result of the transformation if the inverse of the <paramref name="bitmapData"/>
        /// has no exact representation with its <see cref="IBitmapData.PixelFormat"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. You can use
        /// the <see cref="Invert(IReadWriteBitmapData, IDitherer, ParallelConfig)"/> overload to configure these, while still executing the method synchronously.
        /// Alternatively, use the <see cref="BeginInvert">BeginInvert</see> or <see cref="InvertAsync">InvertAsync</see> (in .NET Framework 4.0 and above) methods for asynchronous call and to adjust parallelization, set up cancellation and for reporting progress.</note>
        /// <para>If <paramref name="bitmapData"/> has an indexed <see cref="IBitmapData.PixelFormat"/> and <paramref name="ditherer"/> is <see langword="null"/>,
        /// then its palette entries are tried to be transformed instead of the actual pixels in the first place (if it is supported by <paramref name="bitmapData"/>).
        /// To transform the colors of an indexed <see cref="IBitmapData"/> without changing the palette specify a non-<see langword="null"/>&#160;<paramref name="ditherer"/>.
        /// Transforming the palette is both faster and provides a better result.</para>
        /// <para>If <paramref name="ditherer"/> is <see langword="null"/>, this method attempts to preserve the original color depth, including wide pixel formats.</para>
        /// <para>The <paramref name="ditherer"/> may have no effect for <see cref="KnownPixelFormat"/>s with more than 16 bits-per-pixel and for grayscale formats.</para>
        /// </remarks>
        public static void Invert(this IReadWriteBitmapData bitmapData, IDitherer? ditherer = null)
        {
            ValidateArguments(bitmapData);
            DoInvert(AsyncHelper.DefaultContext, bitmapData, ditherer);
        }

        /// <summary>
        /// Inverts the colors of the specified <paramref name="bitmapData"/>.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to be inverted.</param>
        /// <param name="ditherer">An optional <see cref="IDitherer"/> instance to dither the result of the transformation if the inverse of the <paramref name="bitmapData"/>
        /// has no exact representation with its <see cref="IBitmapData.PixelFormat"/>.</param>
        /// <param name="parallelConfig">The configuration of the operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.
        /// If <see langword="null"/>, then the degree of parallelization is configured automatically.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// of the <paramref name="parallelConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <note>This method blocks the caller as it executes synchronously, though the <paramref name="parallelConfig"/> parameter allows configuring the degree of parallelism,
        /// cancellation and progress reporting. Use the <see cref="BeginInvert">BeginInvert</see> or <see cref="InvertAsync">InvertAsync</see> (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// <para>If <paramref name="bitmapData"/> has an indexed <see cref="IBitmapData.PixelFormat"/> and <paramref name="ditherer"/> is <see langword="null"/>,
        /// then its palette entries are tried to be transformed instead of the actual pixels in the first place (if it is supported by <paramref name="bitmapData"/>).
        /// To transform the colors of an indexed <see cref="IBitmapData"/> without changing the palette specify a non-<see langword="null"/>&#160;<paramref name="ditherer"/>.
        /// Transforming the palette is both faster and provides a better result.</para>
        /// <para>If <paramref name="ditherer"/> is <see langword="null"/>, this method attempts to preserve the original color depth, including wide pixel formats.</para>
        /// <para>The <paramref name="ditherer"/> may have no effect for <see cref="KnownPixelFormat"/>s with more than 16 bits-per-pixel and for grayscale formats.</para>
        /// </remarks>
        public static bool Invert(this IReadWriteBitmapData bitmapData, IDitherer? ditherer, ParallelConfig? parallelConfig)
        {
            ValidateArguments(bitmapData);
            return AsyncHelper.DoOperationSynchronously(ctx => DoInvert(ctx, bitmapData, ditherer), parallelConfig);
        }

        /// <summary>
        /// Inverts the colors of the specified <paramref name="bitmapData"/>, using a <paramref name="context"/> that may belong to a higher level, possibly asynchronous operation.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to be inverted.</param>
        /// <param name="context">An <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncContext.htm">IAsyncContext</a> instance
        /// that contains information for asynchronous processing about the current operation.</param>
        /// <param name="ditherer">An optional <see cref="IDitherer"/> instance to dither the result of the transformation if the inverse of the <paramref name="bitmapData"/>
        /// has no exact representation with its <see cref="IBitmapData.PixelFormat"/>.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled.</returns>
        /// <remarks>
        /// <para>This method blocks the caller thread but if <paramref name="context"/> belongs to an async top level method, then the execution may already run
        /// on a pool thread. Degree of parallelism, the ability of cancellation and reporting progress depend on how these were configured at the top level method.
        /// To reconfigure the degree of parallelism of an existing context, you can use the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_AsyncContextWrapper.htm">AsyncContextWrapper</a> class.</para>
        /// <para>Alternatively, you can use this method to specify the degree of parallelism for synchronous execution. For example, by
        /// passing <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncHelper_SingleThreadContext.htm">AsyncHelper.SingleThreadContext</a> to the <paramref name="context"/> parameter
        /// the method will be forced to use a single thread only.</para>
        /// <para>When reporting progress, this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.</para>
        /// <note type="tip">See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_AsyncHelper.htm">AsyncHelper</a>
        /// class for details about how to create a context for possibly async top level methods.</note>
        /// <note>See the <see cref="Invert(IReadWriteBitmapData, IDitherer?)"/> overload for more details about the other parameters.</note>
        /// </remarks>
        public static bool Invert(this IReadWriteBitmapData bitmapData, IAsyncContext? context, IDitherer? ditherer = null)
        {
            ValidateArguments(bitmapData);
            return DoInvert(context ?? AsyncHelper.DefaultContext, bitmapData, ditherer);
        }

        /// <summary>
        /// Begins to Invert the colors of the specified <paramref name="bitmapData"/> asynchronously.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to be inverted.</param>
        /// <param name="ditherer">An optional <see cref="IDitherer"/> instance to dither the result of the transformation if the inverse of the <paramref name="bitmapData"/>
        /// has no exact representation with its <see cref="IBitmapData.PixelFormat"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="InvertAsync">InvertAsync</see> method.</para>
        /// <para>To finish the operation and to get the exception that occurred during the operation you have to call the <see cref="EndInvert">EndInvert</see> method.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="Invert(IReadWriteBitmapData, IDitherer?)">Invert</see> method for more details.</note>
        /// </remarks>
        public static IAsyncResult BeginInvert(this IReadWriteBitmapData bitmapData, IDitherer? ditherer = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData);
            return AsyncHelper.BeginOperation(ctx => DoInvert(ctx, bitmapData, ditherer), asyncConfig);
        }

        /// <summary>
        /// Waits for the pending asynchronous operation started by the <see cref="BeginInvert">BeginInvert</see> method to complete.
        /// In .NET Framework 4.0 and above you can use the <see cref="InvertAsync">InvertAsync</see> method instead.
        /// </summary>
        /// <param name="asyncResult">The reference to the pending asynchronous request to finish.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in the <c>asyncConfig</c> parameter was set to <see langword="false"/>.</returns>
        public static bool EndInvert(this IAsyncResult asyncResult)
            => AsyncHelper.EndOperation<bool>(asyncResult, nameof(BeginInvert));

#if !NET35
        /// <summary>
        /// Inverts the colors of the specified <paramref name="bitmapData"/> asynchronously.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to be inverted.</param>
        /// <param name="ditherer">An optional <see cref="IDitherer"/> instance to dither the result of the transformation if the inverse of the <paramref name="bitmapData"/>
        /// has no exact representation with its <see cref="IBitmapData.PixelFormat"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="Invert(IReadWriteBitmapData, IDitherer?)">Invert</see> method for more details.</note>
        /// </remarks>
        public static Task<bool> InvertAsync(this IReadWriteBitmapData bitmapData, IDitherer? ditherer = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData);
            return AsyncHelper.DoOperationAsync(ctx => DoInvert(ctx, bitmapData, ditherer), asyncConfig);
        }
#endif

        #endregion

        #region MakeTransparent

        /// <summary>
        /// If possible, makes the background of this <paramref name="bitmapData"/> transparent, taking the bottom-left pixel as the background color.
        /// If the <paramref name="bitmapData"/> does not support transparency, then the pixels that have the same color as the bottom-left pixel will be set
        /// to the <see cref="IBitmapData.BackColor"/> of <paramref name="bitmapData"/>.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to make transparent.</param>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. You can use
        /// the <see cref="MakeTransparent(IReadWriteBitmapData, ParallelConfig)"/> overload to configure these, while still executing the method synchronously.
        /// Alternatively, use the <see cref="BeginMakeTransparent(IReadWriteBitmapData, AsyncConfig)"/> or <see cref="MakeTransparentAsync(IReadWriteBitmapData, TaskConfig)"/> (in .NET Framework 4.0 and above) methods for asynchronous call and to adjust parallelization, set up cancellation and for reporting progress.</note>
        /// <para>Similarly to the <a href="https://docs.microsoft.com/en-us/dotnet/api/system.drawing.bitmap.maketransparent" target="_blank">Bitmap.MakeTransparent</a> method,
        /// this one uses the bottom-left pixel to determine the background color, which must be completely opaque; otherwise, <paramref name="bitmapData"/> will not be changed.</para>
        /// <para>Unlike the <a href="https://docs.microsoft.com/en-us/dotnet/api/system.drawing.bitmap.maketransparent" target="_blank">Bitmap.MakeTransparent</a> method,
        /// this one preserves the original <see cref="IBitmapData.PixelFormat"/>. If <paramref name="bitmapData"/> does not support transparency and cannot set <see cref="IBitmapData.Palette"/>
        /// either, then every occurrence of the color of the bottom-left pixel will be changed to the <see cref="IBitmapData.BackColor"/> of <paramref name="bitmapData"/>.
        /// To make such bitmaps transparent use the <see cref="ToTransparent(IReadableBitmapData)">ToTransparent</see> method instead,
        /// which returns a new instance that has a <see cref="IBitmapData.PixelFormat"/> with alpha support.</para>
        /// <para>To force replacing even non-completely opaque pixels use the <see cref="MakeTransparent(IReadWriteBitmapData, Color32)"/> overload instead.</para>
        /// <note>Please note that unlike the <see cref="MakeOpaque">MakeOpaque</see> method, this one changes exactly one color shade without any tolerance.
        /// For customizations use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.TransformColors">TransformColors</see> overloads instead.</note>
        /// </remarks>
        /// <seealso cref="ToTransparent(IReadableBitmapData)"/>
        /// <seealso cref="MakeOpaque"/>
        public static void MakeTransparent(this IReadWriteBitmapData bitmapData)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            if (bitmapData.Width < 1 || bitmapData.Height < 1)
                return;
            DoMakeTransparent(AsyncHelper.DefaultContext, bitmapData);
        }

        /// <summary>
        /// If possible, makes the background of this <paramref name="bitmapData"/> transparent, taking the bottom-left pixel as the background color.
        /// If the <paramref name="bitmapData"/> does not support transparency, then the pixels that have the same color as the bottom-left pixel will be set
        /// to the <see cref="IBitmapData.BackColor"/> of <paramref name="bitmapData"/>.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to make transparent.</param>
        /// <param name="parallelConfig">The configuration of the operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.
        /// If <see langword="null"/>, then the degree of parallelization is configured automatically.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// of the <paramref name="parallelConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <note>This method blocks the caller as it executes synchronously, though the <paramref name="parallelConfig"/> parameter allows configuring the degree of parallelism,
        /// cancellation and progress reporting. Use the <see cref="BeginMakeTransparent(IReadWriteBitmapData,AsyncConfig?)"/>
        /// or <see cref="MakeTransparentAsync(IReadWriteBitmapData,TaskConfig?)"/> (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// <para>Unlike the <a href="https://docs.microsoft.com/en-us/dotnet/api/system.drawing.bitmap.maketransparent" target="_blank">Bitmap.MakeTransparent</a> method,
        /// this one preserves the original <see cref="IBitmapData.PixelFormat"/>. If <paramref name="bitmapData"/> does not support transparency and cannot set <see cref="IBitmapData.Palette"/>
        /// either, then every occurrence of the color of the bottom-left pixel will be changed to the <see cref="IBitmapData.BackColor"/> of <paramref name="bitmapData"/>.
        /// To make such bitmaps transparent use the <see cref="ToTransparent(IReadableBitmapData)">ToTransparent</see> method instead,
        /// which returns a new instance that has a <see cref="IBitmapData.PixelFormat"/> with alpha support.</para>
        /// <para>To force replacing even non-completely opaque pixels use the <see cref="MakeTransparent(IReadWriteBitmapData, Color32)"/> overload instead.</para>
        /// <note>Please note that unlike the <see cref="MakeOpaque">MakeOpaque</see> method, this one changes exactly one color shade without any tolerance.
        /// For customizations use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.TransformColors">TransformColors</see> overloads instead.</note>
        /// </remarks>
        /// <seealso cref="ToTransparent(IReadableBitmapData)"/>
        /// <seealso cref="MakeOpaque"/>
        public static bool MakeTransparent(this IReadWriteBitmapData bitmapData, ParallelConfig? parallelConfig)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            if (bitmapData.Width < 1 || bitmapData.Height < 1)
                return AsyncHelper.FromResult(true, parallelConfig);
            return AsyncHelper.DoOperationSynchronously(ctx => DoMakeTransparent(ctx, bitmapData), parallelConfig);
        }

        /// <summary>
        /// If possible, makes the background of this <paramref name="bitmapData"/> transparent, taking the bottom-left pixel as the background color,
        /// and using a <paramref name="context"/> that may belong to a higher level, possibly asynchronous operation.
        /// If the <paramref name="bitmapData"/> does not support transparency, then the pixels that have the same color as the bottom-left pixel will be set
        /// to the <see cref="IBitmapData.BackColor"/> of <paramref name="bitmapData"/>.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to make transparent.</param>
        /// <param name="context">An <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncContext.htm">IAsyncContext</a> instance
        /// that contains information for asynchronous processing about the current operation.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled.</returns>
        /// <remarks>
        /// <para>This method blocks the caller thread but if <paramref name="context"/> belongs to an async top level method, then the execution may already run
        /// on a pool thread. Degree of parallelism, the ability of cancellation and reporting progress depend on how these were configured at the top level method.
        /// To reconfigure the degree of parallelism of an existing context, you can use the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_AsyncContextWrapper.htm">AsyncContextWrapper</a> class.</para>
        /// <para>Alternatively, you can use this method to specify the degree of parallelism for synchronous execution. For example, by
        /// passing <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncHelper_SingleThreadContext.htm">AsyncHelper.SingleThreadContext</a> to the <paramref name="context"/> parameter
        /// the method will be forced to use a single thread only.</para>
        /// <para>When reporting progress, this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.</para>
        /// <note type="tip">See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_AsyncHelper.htm">AsyncHelper</a>
        /// class for details about how to create a context for possibly async top level methods.</note>
        /// <note>See the <see cref="MakeTransparent(IReadWriteBitmapData)"/> overload for more details.</note>
        /// </remarks>
        /// <seealso cref="ToTransparent(IReadableBitmapData)"/>
        /// <seealso cref="MakeOpaque"/>
        public static bool MakeTransparent(this IReadWriteBitmapData bitmapData, IAsyncContext? context)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            if (bitmapData.Width < 1 || bitmapData.Height < 1)
                return true;
            return DoMakeTransparent(context ?? AsyncHelper.DefaultContext, bitmapData);
        }

        /// <summary>
        /// If possible, makes the background of this <paramref name="bitmapData"/> transparent, using <paramref name="transparentColor"/> as the background color.
        /// If the <paramref name="bitmapData"/> does not support transparency, then the pixels that have the same color as <paramref name="transparentColor"/> will be set
        /// to the <see cref="IBitmapData.BackColor"/> of <paramref name="bitmapData"/>.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to make transparent.</param>
        /// <param name="transparentColor">Specifies the color to make transparent.</param>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. You can use
        /// the <see cref="MakeTransparent(IReadWriteBitmapData, Color32, ParallelConfig)"/> overload to configure these, while still executing the method synchronously.
        /// Alternatively, use the <see cref="BeginMakeTransparent(IReadWriteBitmapData, Color32, AsyncConfig)"/> or <see cref="MakeTransparentAsync(IReadWriteBitmapData, Color32, TaskConfig)"/>
        /// (in .NET Framework 4.0 and above) methods for asynchronous call and to adjust parallelization, set up cancellation and for reporting progress.</note>
        /// <para>Unlike the <a href="https://docs.microsoft.com/en-us/dotnet/api/system.drawing.bitmap.maketransparent" target="_blank">Bitmap.MakeTransparent(Color)</a> method,
        /// this one preserves the original <see cref="IBitmapData.PixelFormat"/>. If <paramref name="bitmapData"/> does not support transparency and cannot set <see cref="IBitmapData.Palette"/> either,
        /// then every occurrence of the <paramref name="transparentColor"/> will be changed to the <see cref="IBitmapData.BackColor"/> of <paramref name="bitmapData"/>.
        /// To make such bitmaps transparent use the <see cref="ToTransparent(IReadableBitmapData,Color32)">ToTransparent</see> method instead,
        /// which returns a new instance that has a <see cref="IBitmapData.PixelFormat"/> with alpha support.</para>
        /// <para>To auto-detect the background color to be made transparent use the <see cref="MakeTransparent(IReadWriteBitmapData)"/> overload instead.
        /// Please note though that this overload allows a non-completely opaque <paramref name="transparentColor"/>.</para>
        /// <para>If the <paramref name="transparentColor"/> cannot be represented by a <see cref="Color32"/> instance,
        /// then use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.TransformColors">TransformColors</see> overloads that work with <see cref="Color64"/> or <see cref="ColorF"/> delegate parameters instead.</para>
        /// <note>Please note that unlike the <see cref="MakeOpaque">MakeOpaque</see> method, this one changes exactly one color shade without any tolerance.
        /// For customizations use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.TransformColors">TransformColors</see> overloads instead.</note>
        /// </remarks>
        /// <seealso cref="ToTransparent(IReadableBitmapData,Color32)"/>
        /// <seealso cref="MakeOpaque"/>
        public static void MakeTransparent(this IReadWriteBitmapData bitmapData, Color32 transparentColor)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            if (transparentColor.A == 0)
                return;
            DoMakeTransparent(AsyncHelper.DefaultContext, bitmapData, transparentColor);
        }

        /// <summary>
        /// If possible, makes the background of this <paramref name="bitmapData"/> transparent, using <paramref name="transparentColor"/> as the background color.
        /// If the <paramref name="bitmapData"/> does not support transparency, then the pixels that have the same color as <paramref name="transparentColor"/> will be set
        /// to the <see cref="IBitmapData.BackColor"/> of <paramref name="bitmapData"/>.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to make transparent.</param>
        /// <param name="transparentColor">Specifies the color to make transparent.</param>
        /// <param name="parallelConfig">The configuration of the operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.
        /// If <see langword="null"/>, then the degree of parallelization is configured automatically.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// of the <paramref name="parallelConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <note>This method blocks the caller as it executes synchronously, though the <paramref name="parallelConfig"/> parameter allows configuring the degree of parallelism,
        /// cancellation and progress reporting. Use the <see cref="BeginMakeTransparent(IReadWriteBitmapData,Color32,AsyncConfig?)"/>
        /// or <see cref="MakeTransparentAsync(IReadWriteBitmapData,Color32,TaskConfig?)"/> (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// <para>Unlike the <a href="https://docs.microsoft.com/en-us/dotnet/api/system.drawing.bitmap.maketransparent" target="_blank">Bitmap.MakeTransparent(Color)</a> method,
        /// this one preserves the original <see cref="IBitmapData.PixelFormat"/>. If <paramref name="bitmapData"/> does not support transparency and cannot set <see cref="IBitmapData.Palette"/> either,
        /// then every occurrence of the <paramref name="transparentColor"/> will be changed to the <see cref="IBitmapData.BackColor"/> of <paramref name="bitmapData"/>.
        /// To make such bitmaps transparent use the <see cref="ToTransparent(IReadableBitmapData,Color32)">ToTransparent</see> method instead,
        /// which returns a new instance that has a <see cref="IBitmapData.PixelFormat"/> with alpha support.</para>
        /// <para>To auto-detect the background color to be made transparent use the <see cref="MakeTransparent(IReadWriteBitmapData,ParallelConfig)"/> overload instead.
        /// Please note though that this overload allows a non-completely opaque <paramref name="transparentColor"/>.</para>
        /// <para>If the <paramref name="transparentColor"/> cannot be represented by a <see cref="Color32"/> instance,
        /// then use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.TransformColors">TransformColors</see> overloads that work with <see cref="Color64"/> or <see cref="ColorF"/> delegate parameters instead.</para>
        /// <note>Please note that unlike the <see cref="MakeOpaque">MakeOpaque</see> method, this one changes exactly one color shade without any tolerance.
        /// For customizations use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.TransformColors">TransformColors</see> overloads instead.</note>
        /// </remarks>
        /// <seealso cref="ToTransparent(IReadableBitmapData,Color32)"/>
        /// <seealso cref="MakeOpaque"/>
        public static bool MakeTransparent(this IReadWriteBitmapData bitmapData, Color32 transparentColor, ParallelConfig? parallelConfig)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            if (transparentColor.A == 0)
                return AsyncHelper.FromResult(true, parallelConfig);
            return AsyncHelper.DoOperationSynchronously(ctx => DoMakeTransparent(ctx, bitmapData, transparentColor), parallelConfig);
        }

        /// <summary>
        /// If possible, makes the background of this <paramref name="bitmapData"/> transparent, using <paramref name="transparentColor"/> as the background color,
        /// and using a <paramref name="context"/> that may belong to a higher level, possibly asynchronous operation.
        /// If the <paramref name="bitmapData"/> does not support transparency, then the pixels that have the same color as <paramref name="transparentColor"/> will be set
        /// to the <see cref="IBitmapData.BackColor"/> of <paramref name="bitmapData"/>.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to make transparent.</param>
        /// <param name="context">An <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncContext.htm">IAsyncContext</a> instance
        /// that contains information for asynchronous processing about the current operation.</param>
        /// <param name="transparentColor">Specifies the color to make transparent.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled.</returns>
        /// <remarks>
        /// <para>This method blocks the caller thread but if <paramref name="context"/> belongs to an async top level method, then the execution may already run
        /// on a pool thread. Degree of parallelism, the ability of cancellation and reporting progress depend on how these were configured at the top level method.
        /// To reconfigure the degree of parallelism of an existing context, you can use the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_AsyncContextWrapper.htm">AsyncContextWrapper</a> class.</para>
        /// <para>Alternatively, you can use this method to specify the degree of parallelism for synchronous execution. For example, by
        /// passing <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncHelper_SingleThreadContext.htm">AsyncHelper.SingleThreadContext</a> to the <paramref name="context"/> parameter
        /// the method will be forced to use a single thread only.</para>
        /// <para>When reporting progress, this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.</para>
        /// <note type="tip">See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_AsyncHelper.htm">AsyncHelper</a>
        /// class for details about how to create a context for possibly async top level methods.</note>
        /// <note>See the <see cref="MakeTransparent(IReadWriteBitmapData,Color32)"/> overload for more details.</note>
        /// </remarks>
        /// <seealso cref="ToTransparent(IReadableBitmapData,Color32)"/>
        /// <seealso cref="MakeOpaque"/>
        public static bool MakeTransparent(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Color32 transparentColor)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            if (transparentColor.A == 0)
                return true;
            return DoMakeTransparent(context ?? AsyncHelper.DefaultContext, bitmapData, transparentColor);
        }

        /// <summary>
        /// If possible, begins to make the background of this <paramref name="bitmapData"/> transparent asynchronously, taking the bottom-left pixel as the background color.
        /// If the <paramref name="bitmapData"/> does not support transparency, then the pixels that have the same color as the bottom-left pixel will be set
        /// to the <see cref="IBitmapData.BackColor"/> of <paramref name="bitmapData"/>.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to make transparent.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="MakeTransparentAsync(IReadWriteBitmapData, TaskConfig)"/> method.</para>
        /// <para>To finish the operation and to get the exception that occurred during the operation you have to call the <see cref="EndMakeTransparent">EndMakeTransparent</see> method.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="MakeTransparent(IReadWriteBitmapData)"/> method for more details.</note>
        /// </remarks>
        /// <seealso cref="BeginToTransparent(IReadableBitmapData, AsyncConfig)"/>
        /// <seealso cref="BeginMakeOpaque"/>
        public static IAsyncResult BeginMakeTransparent(this IReadWriteBitmapData bitmapData, AsyncConfig? asyncConfig = null)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            if (bitmapData.Width < 1 || bitmapData.Height < 1)
                return AsyncHelper.FromResult(true, asyncConfig);
            return AsyncHelper.BeginOperation(ctx => DoMakeTransparent(ctx, bitmapData), asyncConfig);
        }

        /// <summary>
        /// If possible, begins to make the background of this <paramref name="bitmapData"/> transparent asynchronously, using <paramref name="transparentColor"/> as the background color.
        /// If the <paramref name="bitmapData"/> does not support transparency, then the pixels that have the same color as <paramref name="transparentColor"/> will be set
        /// to the <see cref="IBitmapData.BackColor"/> of <paramref name="bitmapData"/>.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to make transparent.</param>
        /// <param name="transparentColor">Specifies the color to make transparent.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="MakeTransparentAsync(IReadWriteBitmapData,Color32,TaskConfig)"/> method.</para>
        /// <para>To finish the operation and to get the exception that occurred during the operation you have to call the <see cref="EndMakeTransparent">EndMakeTransparent</see> method.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="MakeTransparent(IReadWriteBitmapData, Color32)"/> method for more details.</note>
        /// </remarks>
        /// <seealso cref="BeginToTransparent(IReadableBitmapData,Color32,AsyncConfig)"/>
        /// <seealso cref="BeginMakeOpaque"/>
        public static IAsyncResult BeginMakeTransparent(this IReadWriteBitmapData bitmapData, Color32 transparentColor, AsyncConfig? asyncConfig = null)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            if (transparentColor.A == 0)
                return AsyncHelper.FromResult(true, asyncConfig);
            return AsyncHelper.BeginOperation(ctx => DoMakeTransparent(ctx, bitmapData, transparentColor), asyncConfig);
        }

        /// <summary>
        /// Waits for the pending asynchronous operation started by the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.BeginMakeTransparent">BeginMakeTransparent</see> methods to complete.
        /// In .NET Framework 4.0 and above you can use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.MakeTransparentAsync">MakeTransparentAsync</see> methods instead.
        /// </summary>
        /// <param name="asyncResult">The reference to the pending asynchronous request to finish.</param>
        public static void EndMakeTransparent(this IAsyncResult asyncResult)
            // NOTE: the return value could be bool, but it would be a breaking change
            => AsyncHelper.EndOperation<bool>(asyncResult, nameof(BeginMakeTransparent));

#if !NET35
        /// <summary>
        /// If possible, makes the background of this <paramref name="bitmapData"/> transparent asynchronously, taking the bottom-left pixel as the background color.
        /// If the <paramref name="bitmapData"/> does not support transparency, then the pixels that have the same color as the bottom-left pixel will be set
        /// to the <see cref="IBitmapData.BackColor"/> of <paramref name="bitmapData"/>.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to make transparent.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="MakeTransparent(IReadWriteBitmapData)"/> method for more details.</note>
        /// </remarks>
        /// <seealso cref="ToTransparentAsync(IReadableBitmapData, TaskConfig)"/>
        /// <seealso cref="MakeOpaqueAsync"/>
        public static Task<bool> MakeTransparentAsync(this IReadWriteBitmapData bitmapData, TaskConfig? asyncConfig = null)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            if (bitmapData.Width < 1 || bitmapData.Height < 1)
                return AsyncHelper.FromResult(true, asyncConfig);
            return AsyncHelper.DoOperationAsync(ctx => DoMakeTransparent(ctx, bitmapData), asyncConfig);
        }

        /// <summary>
        /// If possible, makes the background of this <paramref name="bitmapData"/> transparent asynchronously, using <paramref name="transparentColor"/> as the background color.
        /// If the <paramref name="bitmapData"/> does not support transparency, then the pixels that have the same color as <paramref name="transparentColor"/> will be set
        /// to the <see cref="IBitmapData.BackColor"/> of <paramref name="bitmapData"/>.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to make transparent.</param>
        /// <param name="transparentColor">Specifies the color to make transparent.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="MakeTransparent(IReadWriteBitmapData, Color32)"/> method for more details.</note>
        /// </remarks>
        /// <seealso cref="ToTransparentAsync(IReadableBitmapData,Color32,TaskConfig)"/>
        /// <seealso cref="MakeOpaqueAsync"/>
        public static Task<bool> MakeTransparentAsync(this IReadWriteBitmapData bitmapData, Color32 transparentColor, TaskConfig? asyncConfig = null)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            if (transparentColor.A == 0)
                return AsyncHelper.FromResult(true, asyncConfig);
            return AsyncHelper.DoOperationAsync(ctx => DoMakeTransparent(ctx, bitmapData, transparentColor), asyncConfig);
        }
#endif

        #endregion

        #region MakeOpaque

        /// <summary>
        /// Makes this <paramref name="bitmapData"/> opaque using the specified <paramref name="backColor"/>.
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
        /// <para>The <paramref name="ditherer"/> is ignored for <see cref="KnownPixelFormat"/>s with more than 16 bits-per-pixel and for grayscale formats.</para>
        /// </remarks>
        public static void MakeOpaque(this IReadWriteBitmapData bitmapData, Color32 backColor, IDitherer? ditherer = null)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            if (!bitmapData.HasAlpha())
                return;
            DoTransformColors(AsyncHelper.DefaultContext, bitmapData, c => TransformMakeOpaque(c, backColor), ditherer);
        }

        /// <summary>
        /// Begins to make this <paramref name="bitmapData"/> opaque asynchronously using the specified <paramref name="backColor"/>.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to make opaque.</param>
        /// <param name="backColor">Pixels with alpha in <paramref name="bitmapData"/> will be blended with this color.
        /// The <see cref="Color32.A">Color32.A</see> property of the specified color is ignored.</param>
        /// <param name="ditherer">An optional <see cref="IDitherer"/> instance to dither the result of the transformation if the inverse of the <paramref name="bitmapData"/>
        /// has no exact representation with its <see cref="IBitmapData.PixelFormat"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="MakeOpaqueAsync">MakeOpaqueAsync</see> method.</para>
        /// <para>To finish the operation and to get the exception that occurred during the operation you have to call the <see cref="EndMakeOpaque">EndMakeOpaque</see> method.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="MakeOpaque">MakeOpaque</see> method for more details.</note>
        /// <remarks>
        /// </remarks>
        public static IAsyncResult BeginMakeOpaque(this IReadWriteBitmapData bitmapData, Color32 backColor, IDitherer? ditherer = null, AsyncConfig? asyncConfig = null)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            return AsyncHelper.BeginOperation(ctx => DoTransformColors(ctx, bitmapData, c => TransformMakeOpaque(c, backColor), ditherer), asyncConfig);
        }

        /// <summary>
        /// Waits for the pending asynchronous operation started by the <see cref="BeginMakeOpaque">BeginMakeOpaque</see> method to complete.
        /// In .NET Framework 4.0 and above you can use the <see cref="MakeOpaqueAsync">MakeOpaqueAsync</see> method instead.
        /// </summary>
        /// <param name="asyncResult">The reference to the pending asynchronous request to finish.</param>
        public static void EndMakeOpaque(this IAsyncResult asyncResult)
            // NOTE: the return value could be bool, but it would be a breaking change
            => AsyncHelper.EndOperation<bool>(asyncResult, nameof(BeginMakeOpaque));

#if !NET35
        /// <summary>
        /// Makes this <paramref name="bitmapData"/> opaque asynchronously using the specified <paramref name="backColor"/>.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to make opaque.</param>
        /// <param name="backColor">Pixels with alpha in <paramref name="bitmapData"/> will be blended with this color.
        /// The <see cref="Color32.A">Color32.A</see> property of the specified color is ignored.</param>
        /// <param name="ditherer">An optional <see cref="IDitherer"/> instance to dither the result of the transformation if the inverse of the <paramref name="bitmapData"/>
        /// has no exact representation with its <see cref="IBitmapData.PixelFormat"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="MakeOpaque">MakeOpaque</see> method for more details.</note>
        /// <remarks>
        /// </remarks>
        public static Task MakeOpaqueAsync(this IReadWriteBitmapData bitmapData, Color32 backColor, IDitherer? ditherer = null, TaskConfig? asyncConfig = null)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            return AsyncHelper.DoOperationAsync(ctx => DoTransformColors(ctx, bitmapData, c => TransformMakeOpaque(c, backColor), ditherer), asyncConfig);
        }
#endif

        #endregion

        #region MakeGrayscale

        /// <summary>
        /// Makes this <paramref name="bitmapData"/> grayscale.
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
        /// quantizer (<see cref="PredefinedColorsQuantizer.Grayscale(Color32,byte)">PredefinedColorsQuantizer.Grayscale</see>, for example).</para>
        /// <para>This method calls the <see cref="TransformColors(IReadWriteBitmapData, Func{Color32, Color32}, IDitherer)">TransformColors</see> method internally. See
        /// the <strong>Remarks</strong> section of the <see cref="TransformColors(IReadWriteBitmapData, Func{Color32, Color32}, IDitherer)">TransformColors</see> method for more details.</para>
        /// <para>If <paramref name="bitmapData"/> has an indexed <see cref="IBitmapData.PixelFormat"/> and <paramref name="ditherer"/> is <see langword="null"/>,
        /// then its palette entries are tried to be transformed instead of the actual pixels in the first place (if it is supported by <paramref name="bitmapData"/>).
        /// To transform the colors of an indexed <see cref="IBitmapData"/> without changing the palette specify a non-<see langword="null"/>&#160;<paramref name="ditherer"/>.
        /// Transforming the palette is both faster and provides a better result.</para>
        /// <para>The <paramref name="ditherer"/> is ignored for <see cref="KnownPixelFormat"/>s with more than 16 bits-per-pixel and for grayscale formats.</para>
        /// </remarks>
        /// <seealso cref="ToGrayscale"/>
        public static void MakeGrayscale(this IReadWriteBitmapData bitmapData, IDitherer? ditherer = null)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            DoTransformColors(AsyncHelper.DefaultContext, bitmapData, TransformMakeGrayscale, ditherer);
        }

        /// <summary>
        /// Begins to make this <paramref name="bitmapData"/> grayscale asynchronously.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to make grayscale.</param>
        /// <param name="ditherer">An optional <see cref="IDitherer"/> instance to dither the result if grayscale colors
        /// cannot be represented by the <see cref="IBitmapData.PixelFormat"/> or the current palette of the specified <paramref name="bitmapData"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="MakeGrayscaleAsync">MakeGrayscaleAsync</see> method.</para>
        /// <para>To finish the operation and to get the exception that occurred during the operation you have to call the <see cref="EndMakeGrayscale">EndMakeGrayscale</see> method.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="MakeGrayscale">MakeGrayscale</see> method for more details.</note>
        /// </remarks>
        /// <seealso cref="BeginToGrayscale"/>
        public static IAsyncResult BeginMakeGrayscale(this IReadWriteBitmapData bitmapData, IDitherer? ditherer = null, AsyncConfig? asyncConfig = null)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            return AsyncHelper.BeginOperation(ctx => DoTransformColors(ctx, bitmapData, TransformMakeGrayscale, ditherer), asyncConfig);
        }

        /// <summary>
        /// Waits for the pending asynchronous operation started by the <see cref="BeginMakeGrayscale">BeginMakeGrayscale</see> method to complete.
        /// In .NET Framework 4.0 and above you can use the <see cref="MakeGrayscaleAsync">MakeGrayscaleAsync</see> method instead.
        /// </summary>
        /// <param name="asyncResult">The reference to the pending asynchronous request to finish.</param>
        public static void EndMakeGrayscale(this IAsyncResult asyncResult)
            // NOTE: the return value could be bool, but it would be a breaking change
            => AsyncHelper.EndOperation<bool>(asyncResult, nameof(BeginMakeGrayscale));

#if !NET35
        /// <summary>
        /// Makes this <paramref name="bitmapData"/> grayscale asynchronously.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to make grayscale.</param>
        /// <param name="ditherer">An optional <see cref="IDitherer"/> instance to dither the result if grayscale colors
        /// cannot be represented by the <see cref="IBitmapData.PixelFormat"/> or the current palette of the specified <paramref name="bitmapData"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="MakeGrayscale">MakeGrayscale</see> method for more details.</note>
        /// </remarks>
        /// <seealso cref="ToGrayscaleAsync"/>
        public static Task MakeGrayscaleAsync(this IReadWriteBitmapData bitmapData, IDitherer? ditherer = null, TaskConfig? asyncConfig = null)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            return AsyncHelper.DoOperationAsync(ctx => DoTransformColors(ctx, bitmapData, TransformMakeGrayscale, ditherer), asyncConfig);
        }
#endif

        #endregion

        #region AdjustBrightness

        /// <summary>
        /// Adjusts the brightness of the specified <paramref name="bitmapData"/>.
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
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. You can use
        /// the <see cref="AdjustBrightness(IReadWriteBitmapData,float,IDitherer?,ColorChannels,ParallelConfig)"/> overload to configure these, while still executing the method synchronously.
        /// Alternatively, use the <see cref="BeginAdjustBrightness">BeginAdjustBrightness</see> or <see cref="AdjustBrightnessAsync">AdjustBrightnessAsync</see> (in .NET Framework 4.0 and above) methods for asynchronous call and to adjust parallelization, set up cancellation and for reporting progress.</note>
        /// <para>If <paramref name="bitmapData"/> has an indexed <see cref="IBitmapData.PixelFormat"/> and <paramref name="ditherer"/> is <see langword="null"/>,
        /// then its palette entries are tried to be transformed instead of the actual pixels in the first place (if it is supported by <paramref name="bitmapData"/>).
        /// To transform the colors of an indexed <see cref="IBitmapData"/> without changing the palette specify a non-<see langword="null"/>&#160;<paramref name="ditherer"/>.
        /// Transforming the palette is both faster and provides a better result.</para>
        /// <para>If <paramref name="ditherer"/> is <see langword="null"/>, this method attempts to preserve the original color depth, including wide pixel formats.</para>
        /// <para>The <paramref name="ditherer"/> may have no effect for <see cref="KnownPixelFormat"/>s with more than 16 bits-per-pixel and for grayscale formats.</para>
        /// <note type="tip">See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_AdjustBrightness.htm">BitmapExtensions.AdjustBrightness</a> method for an example.</note>
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
            DoAdjustBrightness(AsyncHelper.DefaultContext, bitmapData, brightness, ditherer, channels);
        }

        /// <summary>
        /// Adjusts the brightness of the specified <paramref name="bitmapData"/>.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to be transformed.</param>
        /// <param name="brightness">A float value between -1 and 1, inclusive bounds. Positive values make the <paramref name="bitmapData"/> brighter,
        /// while negative values make it darker.</param>
        /// <param name="ditherer">An optional <see cref="IDitherer"/> instance to dither the result of the transformation if the transformed colors
        /// are not compatible with the <see cref="IBitmapData.PixelFormat"/> of the specified <paramref name="bitmapData"/>.</param>
        /// <param name="channels">The <see cref="ColorChannels"/>, on which the adjustment has to be performed.</param>
        /// <param name="parallelConfig">The configuration of the operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.
        /// If <see langword="null"/>, then the degree of parallelization is configured automatically.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// of the <paramref name="parallelConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <note>This method blocks the caller as it executes synchronously, though the <paramref name="parallelConfig"/> parameter allows configuring the degree of parallelism,
        /// cancellation and progress reporting. Use the <see cref="BeginAdjustBrightness">BeginAdjustBrightness</see> or <see cref="AdjustBrightnessAsync">AdjustBrightnessAsync</see> (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// <para>If <paramref name="bitmapData"/> has an indexed <see cref="IBitmapData.PixelFormat"/> and <paramref name="ditherer"/> is <see langword="null"/>,
        /// then its palette entries are tried to be transformed instead of the actual pixels in the first place (if it is supported by <paramref name="bitmapData"/>).
        /// To transform the colors of an indexed <see cref="IBitmapData"/> without changing the palette specify a non-<see langword="null"/>&#160;<paramref name="ditherer"/>.
        /// Transforming the palette is both faster and provides a better result.</para>
        /// <para>If <paramref name="ditherer"/> is <see langword="null"/>, this method attempts to preserve the original color depth, including wide pixel formats.</para>
        /// <para>The <paramref name="ditherer"/> may have no effect for <see cref="KnownPixelFormat"/>s with more than 16 bits-per-pixel and for grayscale formats.</para>
        /// <note type="tip">See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_AdjustBrightness.htm">BitmapExtensions.AdjustBrightness</a> method for an example.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="brightness"/> is not between -1 and 1
        /// <br/>-or-
        /// <br/><paramref name="channels"/> is out of the defined flags.</exception>
        public static bool AdjustBrightness(this IReadWriteBitmapData bitmapData, float brightness, IDitherer? ditherer, ColorChannels channels, ParallelConfig? parallelConfig)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            if (brightness < -1f || brightness > 1f || Single.IsNaN(brightness))
                throw new ArgumentOutOfRangeException(nameof(brightness), PublicResources.ArgumentMustBeBetween(-1f, 1f));
            if (!channels.AllFlagsDefined())
                throw new ArgumentOutOfRangeException(nameof(channels), PublicResources.FlagsEnumOutOfRange(channels));

            // ReSharper disable once CompareOfFloatsByEqualityOperator - zero has a precise float representation
            if (channels == ColorChannels.None || brightness == 0f)
                return AsyncHelper.FromResult(true, parallelConfig);
            return AsyncHelper.DoOperationSynchronously(ctx => DoAdjustBrightness(ctx, bitmapData, brightness, ditherer, channels), parallelConfig);
        }

        /// <summary>
        /// Adjusts the brightness of the specified <paramref name="bitmapData"/>, using a <paramref name="context"/> that may belong to a higher level, possibly asynchronous operation.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to be transformed.</param>
        /// <param name="context">An <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncContext.htm">IAsyncContext</a> instance
        /// that contains information for asynchronous processing about the current operation.</param>
        /// <param name="brightness">A float value between -1 and 1, inclusive bounds. Positive values make the <paramref name="bitmapData"/> brighter,
        /// while negative values make it darker.</param>
        /// <param name="ditherer">An optional <see cref="IDitherer"/> instance to dither the result of the transformation if the transformed colors
        /// are not compatible with the <see cref="IBitmapData.PixelFormat"/> of the specified <paramref name="bitmapData"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="channels">The <see cref="ColorChannels"/>, on which the adjustment has to be performed. This parameter is optional.
        /// <br/>Default value: <see cref="ColorChannels.Rgb"/>.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled.</returns>
        /// <remarks>
        /// <para>This method blocks the caller thread but if <paramref name="context"/> belongs to an async top level method, then the execution may already run
        /// on a pool thread. Degree of parallelism, the ability of cancellation and reporting progress depend on how these were configured at the top level method.
        /// To reconfigure the degree of parallelism of an existing context, you can use the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_AsyncContextWrapper.htm">AsyncContextWrapper</a> class.</para>
        /// <para>Alternatively, you can use this method to specify the degree of parallelism for synchronous execution. For example, by
        /// passing <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncHelper_SingleThreadContext.htm">AsyncHelper.SingleThreadContext</a> to the <paramref name="context"/> parameter
        /// the method will be forced to use a single thread only.</para>
        /// <para>When reporting progress, this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.</para>
        /// <note type="tip">See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_AsyncHelper.htm">AsyncHelper</a>
        /// class for details about how to create a context for possibly async top level methods.</note>
        /// <note>See the <see cref="AdjustBrightness(IReadWriteBitmapData,float,IDitherer?,ColorChannels)"/> overload for more details about the other parameters.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="brightness"/> is not between -1 and 1
        /// <br/>-or-
        /// <br/><paramref name="channels"/> is out of the defined flags.</exception>
        public static bool AdjustBrightness(this IReadWriteBitmapData bitmapData, IAsyncContext? context, float brightness, IDitherer? ditherer = null, ColorChannels channels = ColorChannels.Rgb)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            if (brightness < -1f || brightness > 1f || Single.IsNaN(brightness))
                throw new ArgumentOutOfRangeException(nameof(brightness), PublicResources.ArgumentMustBeBetween(-1f, 1f));
            if (!channels.AllFlagsDefined())
                throw new ArgumentOutOfRangeException(nameof(channels), PublicResources.FlagsEnumOutOfRange(channels));

            // ReSharper disable once CompareOfFloatsByEqualityOperator - zero has a precise float representation
            if (channels == ColorChannels.None || brightness == 0f)
                return true;
            return DoAdjustBrightness(context ?? AsyncHelper.DefaultContext, bitmapData, brightness, ditherer, channels);
        }

        /// <summary>
        /// Begins to adjust the brightness of the specified <paramref name="bitmapData"/> asynchronously.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to be transformed.</param>
        /// <param name="brightness">A float value between -1 and 1, inclusive bounds. Positive values make the <paramref name="bitmapData"/> brighter,
        /// while negative values make it darker.</param>
        /// <param name="ditherer">An optional <see cref="IDitherer"/> instance to dither the result of the transformation if the transformed colors
        /// are not compatible with the <see cref="IBitmapData.PixelFormat"/> of the specified <paramref name="bitmapData"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="channels">The <see cref="ColorChannels"/>, on which the adjustment has to be performed. This parameter is optional.
        /// <br/>Default value: <see cref="ColorChannels.Rgb"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="AdjustBrightnessAsync">AdjustBrightnessAsync</see> method.</para>
        /// <para>To finish the operation and to get the exception that occurred during the operation you have to call the <see cref="EndAdjustBrightness">EndAdjustBrightness</see> method.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
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
                return AsyncHelper.FromResult(true, asyncConfig);
            return AsyncHelper.BeginOperation(ctx => DoAdjustBrightness(ctx, bitmapData, brightness, ditherer, channels), asyncConfig);
        }

        /// <summary>
        /// Waits for the pending asynchronous operation started by the <see cref="BeginAdjustBrightness">BeginAdjustBrightness</see> method to complete.
        /// In .NET Framework 4.0 and above you can use the <see cref="AdjustBrightnessAsync">AdjustBrightnessAsync</see> method instead.
        /// </summary>
        /// <param name="asyncResult">The reference to the pending asynchronous request to finish.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in the <c>asyncConfig</c> parameter was set to <see langword="false"/>.</returns>
        public static bool EndAdjustBrightness(this IAsyncResult asyncResult)
            => AsyncHelper.EndOperation<bool>(asyncResult, nameof(BeginAdjustBrightness));

#if !NET35
        /// <summary>
        /// Adjusts the brightness of the specified <paramref name="bitmapData"/> asynchronously.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to be transformed.</param>
        /// <param name="brightness">A float value between -1 and 1, inclusive bounds. Positive values make the <paramref name="bitmapData"/> brighter,
        /// while negative values make it darker.</param>
        /// <param name="ditherer">An optional <see cref="IDitherer"/> instance to dither the result of the transformation if the transformed colors
        /// are not compatible with the <see cref="IBitmapData.PixelFormat"/> of the specified <paramref name="bitmapData"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="channels">The <see cref="ColorChannels"/>, on which the adjustment has to be performed. This parameter is optional.
        /// <br/>Default value: <see cref="ColorChannels.Rgb"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="brightness"/> is not between -1 and 1
        /// <br/>-or-
        /// <br/><paramref name="channels"/> is out of the defined flags.</exception>
        public static Task<bool> AdjustBrightnessAsync(this IReadWriteBitmapData bitmapData, float brightness, IDitherer? ditherer = null, ColorChannels channels = ColorChannels.Rgb, TaskConfig? asyncConfig = null)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            if (brightness < -1f || brightness > 1f || Single.IsNaN(brightness))
                throw new ArgumentOutOfRangeException(nameof(brightness), PublicResources.ArgumentMustBeBetween(-1f, 1f));
            if (!channels.AllFlagsDefined())
                throw new ArgumentOutOfRangeException(nameof(channels), PublicResources.FlagsEnumOutOfRange(channels));

            // ReSharper disable once CompareOfFloatsByEqualityOperator - zero has a precise float representation
            if (channels == ColorChannels.None || brightness == 0f)
                return AsyncHelper.FromResult(true, asyncConfig);
            return AsyncHelper.DoOperationAsync(ctx => DoAdjustBrightness(ctx, bitmapData, brightness, ditherer, channels), asyncConfig);
        }
#endif

        #endregion

        #region AdjustContrast

        /// <summary>
        /// Adjusts the contrast of the specified <paramref name="bitmapData"/>.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to be transformed.</param>
        /// <param name="contrast">A float value between -1 and 1, inclusive bounds. Positive values increase the contrast,
        /// while negative values decrease it.</param>
        /// <param name="ditherer">An optional <see cref="IDitherer"/> instance to dither the result of the transformation if the transformed colors
        /// are not compatible with the <see cref="IBitmapData.PixelFormat"/> of the specified <paramref name="bitmapData"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="channels">The <see cref="ColorChannels"/>, on which the adjustment has to be performed. This parameter is optional.
        /// <br/>Default value: <see cref="ColorChannels.Rgb"/>.</param>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. You can use
        /// the <see cref="AdjustContrast(IReadWriteBitmapData,float,IDitherer?,ColorChannels,ParallelConfig)"/> overload to configure these, while still executing the method synchronously.
        /// Alternatively, use the <see cref="BeginAdjustContrast">BeginAdjustContrast</see> or <see cref="AdjustContrastAsync">AdjustContrastAsync</see> (in .NET Framework 4.0 and above) methods for asynchronous call and to adjust parallelization, set up cancellation and for reporting progress.</note>
        /// <para>If <paramref name="bitmapData"/> has an indexed <see cref="IBitmapData.PixelFormat"/> and <paramref name="ditherer"/> is <see langword="null"/>,
        /// then its palette entries are tried to be transformed instead of the actual pixels in the first place (if it is supported by <paramref name="bitmapData"/>).
        /// To transform the colors of an indexed <see cref="IBitmapData"/> without changing the palette specify a non-<see langword="null"/>&#160;<paramref name="ditherer"/>.
        /// Transforming the palette is both faster and provides a better result.</para>
        /// <para>If <paramref name="ditherer"/> is <see langword="null"/>, this method attempts to preserve the original color depth, including wide pixel formats.</para>
        /// <para>The <paramref name="ditherer"/> may have no effect for <see cref="KnownPixelFormat"/>s with more than 16 bits-per-pixel and for grayscale formats.</para>
        /// <note type="tip">See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_AdjustContrast.htm">BitmapExtensions.AdjustContrast</a> method for an example.</note>
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

            DoAdjustContrast(AsyncHelper.DefaultContext, bitmapData, contrast, ditherer, channels);
        }

        /// <summary>
        /// Adjusts the contrast of the specified <paramref name="bitmapData"/>.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to be transformed.</param>
        /// <param name="contrast">A float value between -1 and 1, inclusive bounds. Positive values increase the contrast,
        /// while negative values decrease it.</param>
        /// <param name="ditherer">An optional <see cref="IDitherer"/> instance to dither the result of the transformation if the transformed colors
        /// are not compatible with the <see cref="IBitmapData.PixelFormat"/> of the specified <paramref name="bitmapData"/>.</param>
        /// <param name="channels">The <see cref="ColorChannels"/>, on which the adjustment has to be performed.</param>
        /// <param name="parallelConfig">The configuration of the operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.
        /// If <see langword="null"/>, then the degree of parallelization is configured automatically.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// of the <paramref name="parallelConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <note>This method blocks the caller as it executes synchronously, though the <paramref name="parallelConfig"/> parameter allows configuring the degree of parallelism,
        /// cancellation and progress reporting. Use the <see cref="BeginAdjustContrast">BeginAdjustContrast</see> or <see cref="AdjustContrastAsync">AdjustContrastAsync</see> (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// <para>If <paramref name="bitmapData"/> has an indexed <see cref="IBitmapData.PixelFormat"/> and <paramref name="ditherer"/> is <see langword="null"/>,
        /// then its palette entries are tried to be transformed instead of the actual pixels in the first place (if it is supported by <paramref name="bitmapData"/>).
        /// To transform the colors of an indexed <see cref="IBitmapData"/> without changing the palette specify a non-<see langword="null"/>&#160;<paramref name="ditherer"/>.
        /// Transforming the palette is both faster and provides a better result.</para>
        /// <para>If <paramref name="ditherer"/> is <see langword="null"/>, this method attempts to preserve the original color depth, including wide pixel formats.</para>
        /// <para>The <paramref name="ditherer"/> may have no effect for <see cref="KnownPixelFormat"/>s with more than 16 bits-per-pixel and for grayscale formats.</para>
        /// <note type="tip">See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_AdjustContrast.htm">BitmapExtensions.AdjustContrast</a> method for an example.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="contrast"/> is not between -1 and 1
        /// <br/>-or-
        /// <br/><paramref name="channels"/> is out of the defined flags.</exception>
        public static bool AdjustContrast(this IReadWriteBitmapData bitmapData, float contrast, IDitherer? ditherer, ColorChannels channels, ParallelConfig? parallelConfig)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            if (contrast < -1f || contrast > 1f || Single.IsNaN(contrast))
                throw new ArgumentOutOfRangeException(nameof(contrast), PublicResources.ArgumentMustBeBetween(-1f, 1f));
            if (!channels.AllFlagsDefined())
                throw new ArgumentOutOfRangeException(nameof(channels), PublicResources.FlagsEnumOutOfRange(channels));

            // ReSharper disable once CompareOfFloatsByEqualityOperator - zero has a precise float representation
            if (channels == ColorChannels.None || contrast == 0f)
                return AsyncHelper.FromResult(true, parallelConfig);

            return AsyncHelper.DoOperationSynchronously(ctx => DoAdjustContrast(ctx, bitmapData, contrast, ditherer, channels), parallelConfig);
        }

        /// <summary>
        /// Adjusts the contrast of the specified <paramref name="bitmapData"/>, using a <paramref name="context"/> that may belong to a higher level, possibly asynchronous operation.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to be transformed.</param>
        /// <param name="context">An <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncContext.htm">IAsyncContext</a> instance
        /// that contains information for asynchronous processing about the current operation.</param>
        /// <param name="contrast">A float value between -1 and 1, inclusive bounds. Positive values increase the contrast,
        /// while negative values decrease it.</param>
        /// <param name="ditherer">An optional <see cref="IDitherer"/> instance to dither the result of the transformation if the transformed colors
        /// are not compatible with the <see cref="IBitmapData.PixelFormat"/> of the specified <paramref name="bitmapData"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="channels">The <see cref="ColorChannels"/>, on which the adjustment has to be performed. This parameter is optional.
        /// <br/>Default value: <see cref="ColorChannels.Rgb"/>.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled.</returns>
        /// <remarks>
        /// <para>This method blocks the caller thread but if <paramref name="context"/> belongs to an async top level method, then the execution may already run
        /// on a pool thread. Degree of parallelism, the ability of cancellation and reporting progress depend on how these were configured at the top level method.
        /// To reconfigure the degree of parallelism of an existing context, you can use the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_AsyncContextWrapper.htm">AsyncContextWrapper</a> class.</para>
        /// <para>Alternatively, you can use this method to specify the degree of parallelism for synchronous execution. For example, by
        /// passing <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncHelper_SingleThreadContext.htm">AsyncHelper.SingleThreadContext</a> to the <paramref name="context"/> parameter
        /// the method will be forced to use a single thread only.</para>
        /// <para>When reporting progress, this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.</para>
        /// <note type="tip">See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_AsyncHelper.htm">AsyncHelper</a>
        /// class for details about how to create a context for possibly async top level methods.</note>
        /// <note>See the <see cref="AdjustContrast(IReadWriteBitmapData,float,IDitherer?,ColorChannels)"/> overload for more details about the other parameters.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="contrast"/> is not between -1 and 1
        /// <br/>-or-
        /// <br/><paramref name="channels"/> is out of the defined flags.</exception>
        public static bool AdjustContrast(this IReadWriteBitmapData bitmapData, IAsyncContext? context, float contrast, IDitherer? ditherer = null, ColorChannels channels = ColorChannels.Rgb)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            if (contrast < -1f || contrast > 1f || Single.IsNaN(contrast))
                throw new ArgumentOutOfRangeException(nameof(contrast), PublicResources.ArgumentMustBeBetween(-1f, 1f));
            if (!channels.AllFlagsDefined())
                throw new ArgumentOutOfRangeException(nameof(channels), PublicResources.FlagsEnumOutOfRange(channels));

            // ReSharper disable once CompareOfFloatsByEqualityOperator - zero has a precise float representation
            if (channels == ColorChannels.None || contrast == 0f)
                return true;

            return DoAdjustContrast(context ?? AsyncHelper.DefaultContext, bitmapData, contrast, ditherer, channels);
        }

        /// <summary>
        /// Begins to adjust the contrast of the specified <paramref name="bitmapData"/> asynchronously.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to be transformed.</param>
        /// <param name="contrast">A float value between -1 and 1, inclusive bounds. Positive values increase the contrast,
        /// while negative values decrease it.</param>
        /// <param name="ditherer">An optional <see cref="IDitherer"/> instance to dither the result of the transformation if the transformed colors
        /// are not compatible with the <see cref="IBitmapData.PixelFormat"/> of the specified <paramref name="bitmapData"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="channels">The <see cref="ColorChannels"/>, on which the adjustment has to be performed. This parameter is optional.
        /// <br/>Default value: <see cref="ColorChannels.Rgb"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="AdjustContrastAsync">AdjustContrastAsync</see> method.</para>
        /// <para>To finish the operation and to get the exception that occurred during the operation you have to call the <see cref="EndAdjustContrast">EndAdjustContrast</see> method.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
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
                return AsyncHelper.FromResult(true, asyncConfig);

            return AsyncHelper.BeginOperation(ctx => DoAdjustContrast(ctx, bitmapData, contrast, ditherer, channels), asyncConfig);
        }

        /// <summary>
        /// Waits for the pending asynchronous operation started by the <see cref="BeginAdjustContrast">BeginAdjustContrast</see> method to complete.
        /// In .NET Framework 4.0 and above you can use the <see cref="AdjustContrastAsync">AdjustContrastAsync</see> method instead.
        /// </summary>
        /// <param name="asyncResult">The reference to the pending asynchronous request to finish.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in the <c>asyncConfig</c> parameter was set to <see langword="false"/>.</returns>
        public static bool EndAdjustContrast(this IAsyncResult asyncResult)
            => AsyncHelper.EndOperation<bool>(asyncResult, nameof(BeginAdjustContrast));

#if !NET35
        /// <summary>
        /// Adjusts the contrast of the specified <paramref name="bitmapData"/> asynchronously.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to be transformed.</param>
        /// <param name="contrast">A float value between -1 and 1, inclusive bounds. Positive values increase the contrast,
        /// while negative values decrease it.</param>
        /// <param name="ditherer">An optional <see cref="IDitherer"/> instance to dither the result of the transformation if the transformed colors
        /// are not compatible with the <see cref="IBitmapData.PixelFormat"/> of the specified <paramref name="bitmapData"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="channels">The <see cref="ColorChannels"/>, on which the adjustment has to be performed. This parameter is optional.
        /// <br/>Default value: <see cref="ColorChannels.Rgb"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="contrast"/> is not between -1 and 1
        /// <br/>-or-
        /// <br/><paramref name="channels"/> is out of the defined flags.</exception>
        public static Task<bool> AdjustContrastAsync(this IReadWriteBitmapData bitmapData, float contrast, IDitherer? ditherer = null, ColorChannels channels = ColorChannels.Rgb, TaskConfig? asyncConfig = null)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            if (contrast < -1f || contrast > 1f || Single.IsNaN(contrast))
                throw new ArgumentOutOfRangeException(nameof(contrast), PublicResources.ArgumentMustBeBetween(-1f, 1f));
            if (!channels.AllFlagsDefined())
                throw new ArgumentOutOfRangeException(nameof(channels), PublicResources.FlagsEnumOutOfRange(channels));

            // ReSharper disable once CompareOfFloatsByEqualityOperator - zero has a precise float representation
            if (channels == ColorChannels.None || contrast == 0f)
                return AsyncHelper.FromResult(true, asyncConfig);

            return AsyncHelper.DoOperationAsync(ctx => DoAdjustContrast(ctx, bitmapData, contrast, ditherer, channels), asyncConfig);
        }

#endif

        #endregion

        #region AdjustGamma

        /// <summary>
        /// Adjusts the gamma correction of the specified <paramref name="bitmapData"/>.
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
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. You can use
        /// the <see cref="AdjustGamma(IReadWriteBitmapData,float,IDitherer?,ColorChannels,ParallelConfig)"/> overload to configure these, while still executing the method synchronously.
        /// Alternatively, use the <see cref="BeginAdjustGamma">BeginAdjustGamma</see> or <see cref="AdjustGammaAsync">AdjustGammaAsync</see> (in .NET Framework 4.0 and above) methods for asynchronous call and to adjust parallelization, set up cancellation and for reporting progress.</note>
        /// <para>If <paramref name="bitmapData"/> has an indexed <see cref="IBitmapData.PixelFormat"/> and <paramref name="ditherer"/> is <see langword="null"/>,
        /// then its palette entries are tried to be transformed instead of the actual pixels in the first place (if it is supported by <paramref name="bitmapData"/>).
        /// To transform the colors of an indexed <see cref="IBitmapData"/> without changing the palette specify a non-<see langword="null"/>&#160;<paramref name="ditherer"/>.
        /// Transforming the palette is both faster and provides a better result.</para>
        /// <para>If <paramref name="ditherer"/> is <see langword="null"/>, this method attempts to preserve the original color depth, including wide pixel formats.</para>
        /// <para>The <paramref name="ditherer"/> may have no effect for <see cref="KnownPixelFormat"/>s with more than 16 bits-per-pixel and for grayscale formats.</para>
        /// <note type="tip">See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_AdjustGamma.htm">BitmapExtensions.AdjustGamma</a> method for an example.</note>
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

            DoAdjustGamma(AsyncHelper.DefaultContext, bitmapData, gamma, ditherer, channels);
        }

        /// <summary>
        /// Adjusts the gamma correction of the specified <paramref name="bitmapData"/>.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to be transformed.</param>
        /// <param name="gamma">A float value between 0 and 10, inclusive bounds. Values less than 1 decrease gamma correction,
        /// while values above 1 increase it.</param>
        /// <param name="ditherer">An optional <see cref="IDitherer"/> instance to dither the result of the transformation if the transformed colors
        /// are not compatible with the <see cref="IBitmapData.PixelFormat"/> of the specified <paramref name="bitmapData"/>.</param>
        /// <param name="channels">The <see cref="ColorChannels"/>, on which the adjustment has to be performed.</param>
        /// <param name="parallelConfig">The configuration of the operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.
        /// If <see langword="null"/>, then the degree of parallelization is configured automatically.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// of the <paramref name="parallelConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <note>This method blocks the caller as it executes synchronously, though the <paramref name="parallelConfig"/> parameter allows configuring the degree of parallelism,
        /// cancellation and progress reporting. Use the <see cref="BeginAdjustGamma">BeginAdjustGamma</see> or <see cref="AdjustGammaAsync">AdjustGammaAsync</see> (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// <para>If <paramref name="bitmapData"/> has an indexed <see cref="IBitmapData.PixelFormat"/> and <paramref name="ditherer"/> is <see langword="null"/>,
        /// then its palette entries are tried to be transformed instead of the actual pixels in the first place (if it is supported by <paramref name="bitmapData"/>).
        /// To transform the colors of an indexed <see cref="IBitmapData"/> without changing the palette specify a non-<see langword="null"/>&#160;<paramref name="ditherer"/>.
        /// Transforming the palette is both faster and provides a better result.</para>
        /// <para>If <paramref name="ditherer"/> is <see langword="null"/>, this method attempts to preserve the original color depth, including wide pixel formats.</para>
        /// <para>The <paramref name="ditherer"/> may have no effect for <see cref="KnownPixelFormat"/>s with more than 16 bits-per-pixel and for grayscale formats.</para>
        /// <note type="tip">See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_AdjustGamma.htm">BitmapExtensions.AdjustGamma</a> method for an example.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="gamma"/> is not between 0 and 10
        /// <br/>-or-
        /// <br/><paramref name="channels"/> is out of the defined flags.</exception>
        public static bool AdjustGamma(this IReadWriteBitmapData bitmapData, float gamma, IDitherer? ditherer, ColorChannels channels, ParallelConfig? parallelConfig)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            if (gamma < 0f || gamma > 10f || Single.IsNaN(gamma))
                throw new ArgumentOutOfRangeException(nameof(gamma), PublicResources.ArgumentMustBeBetween(0f, 10f));
            if (!channels.AllFlagsDefined())
                throw new ArgumentOutOfRangeException(nameof(channels), PublicResources.FlagsEnumOutOfRange(channels));

            // ReSharper disable once CompareOfFloatsByEqualityOperator - 1 has a precise float representation
            if (channels == ColorChannels.None || gamma == 1f)
                return AsyncHelper.FromResult(true, parallelConfig);

            return AsyncHelper.DoOperationSynchronously(ctx => DoAdjustGamma(ctx, bitmapData, gamma, ditherer, channels), parallelConfig);
        }

        /// <summary>
        /// Adjusts the gamma correction of the specified <paramref name="bitmapData"/>, using a <paramref name="context"/> that may belong to a higher level, possibly asynchronous operation.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to be transformed.</param>
        /// <param name="context">An <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncContext.htm">IAsyncContext</a> instance
        /// that contains information for asynchronous processing about the current operation.</param>
        /// <param name="gamma">A float value between 0 and 10, inclusive bounds. Values less than 1 decrease gamma correction,
        /// while values above 1 increase it.</param>
        /// <param name="ditherer">An optional <see cref="IDitherer"/> instance to dither the result of the transformation if the transformed colors
        /// are not compatible with the <see cref="IBitmapData.PixelFormat"/> of the specified <paramref name="bitmapData"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="channels">The <see cref="ColorChannels"/>, on which the adjustment has to be performed. This parameter is optional.
        /// <br/>Default value: <see cref="ColorChannels.Rgb"/>.</param>
        /// <remarks>
        /// <para>This method blocks the caller thread but if <paramref name="context"/> belongs to an async top level method, then the execution may already run
        /// on a pool thread. Degree of parallelism, the ability of cancellation and reporting progress depend on how these were configured at the top level method.
        /// To reconfigure the degree of parallelism of an existing context, you can use the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_AsyncContextWrapper.htm">AsyncContextWrapper</a> class.</para>
        /// <para>Alternatively, you can use this method to specify the degree of parallelism for synchronous execution. For example, by
        /// passing <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncHelper_SingleThreadContext.htm">AsyncHelper.SingleThreadContext</a> to the <paramref name="context"/> parameter
        /// the method will be forced to use a single thread only.</para>
        /// <para>When reporting progress, this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.</para>
        /// <note type="tip">See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_AsyncHelper.htm">AsyncHelper</a>
        /// class for details about how to create a context for possibly async top level methods.</note>
        /// <note>See the <see cref="AdjustGamma(IReadWriteBitmapData,float,IDitherer?,ColorChannels)"/> overload for more details about the other parameters.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="gamma"/> is not between 0 and 10
        /// <br/>-or-
        /// <br/><paramref name="channels"/> is out of the defined flags.</exception>
        public static bool AdjustGamma(this IReadWriteBitmapData bitmapData, IAsyncContext? context, float gamma, IDitherer? ditherer = null, ColorChannels channels = ColorChannels.Rgb)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            if (gamma < 0f || gamma > 10f || Single.IsNaN(gamma))
                throw new ArgumentOutOfRangeException(nameof(gamma), PublicResources.ArgumentMustBeBetween(0f, 10f));
            if (!channels.AllFlagsDefined())
                throw new ArgumentOutOfRangeException(nameof(channels), PublicResources.FlagsEnumOutOfRange(channels));

            // ReSharper disable once CompareOfFloatsByEqualityOperator - 1 has a precise float representation
            if (channels == ColorChannels.None || gamma == 1f)
                return true;

            return DoAdjustGamma(context ?? AsyncHelper.DefaultContext, bitmapData, gamma, ditherer, channels);
        }

        /// <summary>
        /// Begins to adjust the gamma correction of the specified <paramref name="bitmapData"/> asynchronously.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to be transformed.</param>
        /// <param name="gamma">A float value between 0 and 10, inclusive bounds. Values less than 1 decrease gamma correction,
        /// while values above 1 increase it.</param>
        /// <param name="ditherer">An optional <see cref="IDitherer"/> instance to dither the result of the transformation if the transformed colors
        /// are not compatible with the <see cref="IBitmapData.PixelFormat"/> of the specified <paramref name="bitmapData"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="channels">The <see cref="ColorChannels"/>, on which the adjustment has to be performed. This parameter is optional.
        /// <br/>Default value: <see cref="ColorChannels.Rgb"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="AdjustGammaAsync">AdjustGammaAsync</see> method.</para>
        /// <para>To finish the operation and to get the exception that occurred during the operation you have to call the <see cref="EndAdjustGamma">EndAdjustGamma</see> method.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
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
                return AsyncHelper.FromResult(true, asyncConfig);

            return AsyncHelper.BeginOperation(ctx => DoAdjustGamma(ctx, bitmapData, gamma, ditherer, channels), asyncConfig);
        }

        /// <summary>
        /// Waits for the pending asynchronous operation started by the <see cref="BeginAdjustGamma">BeginAdjustGamma</see> method to complete.
        /// In .NET Framework 4.0 and above you can use the <see cref="AdjustGammaAsync">AdjustGammaAsync</see> method instead.
        /// </summary>
        /// <param name="asyncResult">The reference to the pending asynchronous request to finish.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in the <c>asyncConfig</c> parameter was set to <see langword="false"/>.</returns>
        public static bool EndAdjustGamma(this IAsyncResult asyncResult)
            => AsyncHelper.EndOperation<bool>(asyncResult, nameof(BeginAdjustGamma));

#if !NET35
        /// <summary>
        /// Adjusts the gamma correction of the specified <paramref name="bitmapData"/> asynchronously.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to be transformed.</param>
        /// <param name="gamma">A float value between 0 and 10, inclusive bounds. Values less than 1 decrease gamma correction,
        /// while values above 1 increase it.</param>
        /// <param name="ditherer">An optional <see cref="IDitherer"/> instance to dither the result of the transformation if the transformed colors
        /// are not compatible with the <see cref="IBitmapData.PixelFormat"/> of the specified <paramref name="bitmapData"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="channels">The <see cref="ColorChannels"/>, on which the adjustment has to be performed. This parameter is optional.
        /// <br/>Default value: <see cref="ColorChannels.Rgb"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="gamma"/> is not between 0 and 10
        /// <br/>-or-
        /// <br/><paramref name="channels"/> is out of the defined flags.</exception>
        public static Task<bool> AdjustGammaAsync(this IReadWriteBitmapData bitmapData, float gamma, IDitherer? ditherer = null, ColorChannels channels = ColorChannels.Rgb, TaskConfig? asyncConfig = null)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            if (gamma < 0f || gamma > 10f || Single.IsNaN(gamma))
                throw new ArgumentOutOfRangeException(nameof(gamma), PublicResources.ArgumentMustBeBetween(0f, 10f));
            if (!channels.AllFlagsDefined())
                throw new ArgumentOutOfRangeException(nameof(channels), PublicResources.FlagsEnumOutOfRange(channels));

            // ReSharper disable once CompareOfFloatsByEqualityOperator - zero has a precise float representation
            if (channels == ColorChannels.None || gamma == 1f)
                return AsyncHelper.FromResult(true, asyncConfig);

            return AsyncHelper.DoOperationAsync(ctx => DoAdjustGamma(ctx, bitmapData, gamma, ditherer, channels), asyncConfig);
        }
#endif

        #endregion

        #endregion

        #region Private Methods

        #region Validation

        private static void ValidateArguments(IReadWriteBitmapData bitmapData, Delegate transformFunction)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            if (transformFunction == null)
                throw new ArgumentNullException(nameof(transformFunction), PublicResources.ArgumentNull);
        }

        #endregion

        #region Quantizing/Dithering

        [SecuritySafeCritical]
        [SuppressMessage("ReSharper", "AccessToDisposedClosure", Justification = "ParallelHelper.For invokes delegates before returning")]
        private static bool DoQuantize(IAsyncContext context, IReadWriteBitmapData bitmapData, IQuantizer quantizer)
        {
            IBitmapDataInternal accessor = bitmapData as IBitmapDataInternal ?? new BitmapDataWrapper(bitmapData, true, true);
            try
            {
                context.Progress?.New(DrawingOperation.InitializingQuantizer);
                using IQuantizingSession session = quantizer.Initialize(bitmapData, context);
                if (context.IsCancellationRequested)
                    return false;
                if (session == null)
                    throw new InvalidOperationException(Res.ImagingQuantizerInitializeNull);

                // Sequential processing
                if (bitmapData.Width < parallelThreshold >> quantizingScale)
                {
                    context.Progress?.New(DrawingOperation.ProcessingPixels, bitmapData.Height);
                    int width = bitmapData.Width;
                    IBitmapDataRowInternal row = accessor.GetRowCached(0);
                    do
                    {
                        if (context.IsCancellationRequested)
                            return false;
                        for (int x = 0; x < width; x++)
                            row.DoSetColor32(x, session.GetQuantizedColor(row.DoGetColor32(x)));
                        context.Progress?.Increment();
                    } while (row.MoveNextRow());

                    return true;
                }

                // Parallel processing
                return ParallelHelper.For(context, DrawingOperation.ProcessingPixels, 0, bitmapData.Height, ProcessRow);

                #region Local Methods

                [SecuritySafeCritical]
                void ProcessRow(int y)
                {
                    int width = bitmapData.Width;
                    IBitmapDataRowInternal row = accessor.GetRowCached(y);
                    for (int x = 0; x < width; x++)
                        row.DoSetColor32(x, session.GetQuantizedColor(row.DoGetColor32(x)));
                }

                #endregion
            }
            finally
            {
                if (!ReferenceEquals(bitmapData, accessor))
                    accessor.Dispose();
            }
        }

        [SecuritySafeCritical]
        [SuppressMessage("ReSharper", "AccessToDisposedClosure", Justification = "ParallelHelper.For invokes delegates before returning")]
        private static bool DoDither(IAsyncContext context, IReadWriteBitmapData bitmapData, IQuantizer quantizer, IDitherer ditherer)
        {
            IBitmapDataInternal accessor = bitmapData as IBitmapDataInternal ?? new BitmapDataWrapper(bitmapData, true, true);

            try
            {
                context.Progress?.New(DrawingOperation.InitializingQuantizer);
                using IQuantizingSession quantizingSession = quantizer.Initialize(bitmapData, context);
                if (context.IsCancellationRequested)
                    return false;
                if (quantizingSession == null)
                    throw new InvalidOperationException(Res.ImagingQuantizerInitializeNull);
               
                context.Progress?.New(DrawingOperation.InitializingDitherer);
                using IDitheringSession ditheringSession = ditherer.Initialize(bitmapData, quantizingSession, context);
                if (context.IsCancellationRequested)
                    return false;
                if (ditheringSession == null)
                    throw new InvalidOperationException(Res.ImagingDithererInitializeNull);

                // Sequential processing
                if (ditheringSession.IsSequential || bitmapData.Width < parallelThreshold >> ditheringScale)
                {
                    context.Progress?.New(DrawingOperation.ProcessingPixels, bitmapData.Height);
                    int width = bitmapData.Width;
                    IBitmapDataRowInternal row = accessor.GetRowCached(0);
                    int y = 0;
                    do
                    {
                        if (context.IsCancellationRequested)
                            return false;
                        for (int x = 0; x < width; x++)
                            row.DoSetColor32(x, ditheringSession.GetDitheredColor(row.DoGetColor32(x), x, y));
                        y += 1;
                        context.Progress?.Increment();
                    } while (row.MoveNextRow());

                    return true;
                }

                // Parallel processing
                return ParallelHelper.For(context, DrawingOperation.ProcessingPixels, 0, bitmapData.Height, ProcessRow);

                #region Local Methods

                [SecuritySafeCritical]
                void ProcessRow(int y)
                {
                    int width = bitmapData.Width;
                    IBitmapDataRowInternal row = accessor.GetRowCached(y);
                    for (int x = 0; x < width; x++)
                        row.DoSetColor32(x, ditheringSession.GetDitheredColor(row.DoGetColor32(x), x, y));
                }

                #endregion
            }
            finally
            {
                if (!ReferenceEquals(bitmapData, accessor))
                    accessor.Dispose();
            }
        }

        #endregion

        #region Color Transformations

        [SecuritySafeCritical]
        [SuppressMessage("ReSharper", "AccessToDisposedClosure", Justification = "ParallelHelper.For invokes delegates before returning")]
        private static bool DoTransformColors(IAsyncContext context, IReadWriteBitmapData bitmapData, Func<Color32, Color32> transformFunction)
        {
            if (context.IsCancellationRequested)
                return false;

            // Indexed format: processing the palette entries when possible
            if (bitmapData is IBitmapDataInternal bitmapDataInternal && bitmapDataInternal.CanSetPalette)
            {
                context.Progress?.New(DrawingOperation.ProcessingPixels, 1);
                Palette palette = bitmapData.Palette!;
                Color32[] oldEntries = palette.Entries;
                Color32[] newEntries = new Color32[oldEntries.Length];
                for (int i = 0; i < newEntries.Length; i++)
                    newEntries[i] = transformFunction.Invoke(oldEntries[i]);
                if (bitmapDataInternal.TrySetPalette(new Palette(newEntries, palette.BackColor, palette.AlphaThreshold, palette.WorkingColorSpace, null)))
                {
                    context.Progress?.Complete();
                    return !context.IsCancellationRequested;
                }

                Debug.Fail("Setting the palette of the same size should work if CanSetPalette is true");
            }

            if (bitmapData.Height < 1)
                return !context.IsCancellationRequested;

            // Non-indexed format or palette cannot be set: processing the pixels
            var accessor = bitmapData as IBitmapDataInternal ?? new BitmapDataWrapper(bitmapData, true, true);
            try
            {
                // Sequential processing
                if (bitmapData.Width < parallelThreshold)
                {
                    context.Progress?.New(DrawingOperation.ProcessingPixels, bitmapData.Height);
                    IBitmapDataRowInternal row = accessor.GetRowCached(0);
                    do
                    {
                        if (context.IsCancellationRequested)
                            return false;
                        for (int x = 0; x < bitmapData.Width; x++)
                            row.DoSetColor32(x, transformFunction.Invoke(row.DoGetColor32(x)));
                        context.Progress?.Increment();
                    } while (row.MoveNextRow());

                    return true;
                }

                // Parallel processing
                return ParallelHelper.For(context, DrawingOperation.ProcessingPixels, 0, bitmapData.Height, ProcessRow);

                #region Local Methods

                [SecuritySafeCritical]
                void ProcessRow(int y)
                {
                    IBitmapDataRowInternal row = accessor.GetRowCached(y);
                    for (int x = 0; x < bitmapData.Width; x++)
                        row.DoSetColor32(x, transformFunction.Invoke(row.DoGetColor32(x)));
                }

                #endregion
            }
            finally
            {
                if (!ReferenceEquals(bitmapData, accessor))
                    accessor.Dispose();
            }
        }

        [SecuritySafeCritical]
        [SuppressMessage("ReSharper", "AccessToDisposedClosure", Justification = "ParallelHelper.For invokes delegates before returning")]
        private static bool DoTransformColors(IAsyncContext context, IReadWriteBitmapData bitmapData, Func<Color32, Color32> transformFunction, IDitherer? ditherer)
        {
            if (ditherer == null || !bitmapData.PixelFormat.CanBeDithered)
                return DoTransformColors(context, bitmapData, transformFunction);

            if (context.IsCancellationRequested)
                return false;

            // Special handling if ditherer relies on actual content: transforming into an ARGB32 result, and dithering that temporary result
            if (ditherer.InitializeReliesOnContent)
            {
                // not using premultiplied format because transformation is faster on simple ARGB32
                using IBitmapDataInternal? tempClone = DoCloneDirect(context, bitmapData, new Rectangle(Point.Empty, bitmapData.Size),
                    KnownPixelFormat.Format32bppArgb, default, 128, WorkingColorSpace.Default, null);
                if (context.IsCancellationRequested)
                    return false;

                Debug.Assert(tempClone != null);
                return DoTransformColors(context, tempClone!, transformFunction)
                    && DoCopy(context, tempClone!, bitmapData, new Rectangle(Point.Empty, tempClone!.Size), Point.Empty, null, ditherer);
            }

            if (bitmapData.Height < 1)
                return !context.IsCancellationRequested;

            IBitmapDataInternal accessor = bitmapData as IBitmapDataInternal ?? new BitmapDataWrapper(bitmapData, true, true);
            try
            {
                IQuantizer quantizer = PredefinedColorsQuantizer.FromBitmapData(bitmapData);
                Debug.Assert(!quantizer.InitializeReliesOnContent, "A predefined color quantizer should not depend on actual content");

                context.Progress?.New(DrawingOperation.InitializingQuantizer); // predefined will be extreme fast bu in case someone tracks progress...
                using IQuantizingSession quantizingSession = quantizer.Initialize(bitmapData, context);
                if (context.IsCancellationRequested)
                    return false;

                context.Progress?.New(DrawingOperation.InitializingDitherer);
                using IDitheringSession ditheringSession = ditherer.Initialize(bitmapData, quantizingSession, context);
                if (context.IsCancellationRequested)
                    return false;

                if (ditheringSession == null)
                    throw new InvalidOperationException(Res.ImagingDithererInitializeNull);

                // sequential processing
                if (ditheringSession.IsSequential || bitmapData.Width < parallelThreshold)
                {
                    context.Progress?.New(DrawingOperation.ProcessingPixels, bitmapData.Height);
                    IBitmapDataRowInternal row = accessor.GetRowCached(0);
                    int y = 0;
                    do
                    {
                        if (context.IsCancellationRequested)
                            return false;
                        for (int x = 0; x < bitmapData.Width; x++)
                            row.DoSetColor32(x, ditheringSession.GetDitheredColor(transformFunction.Invoke(row.DoGetColor32(x)), x, y));
                        y += 1;
                        context.Progress?.Increment();
                    } while (row.MoveNextRow());

                    return true;
                }

                // parallel processing
                return ParallelHelper.For(context, DrawingOperation.ProcessingPixels, 0, bitmapData.Height, ProcessRow);

                #region Local Methods

                [SecuritySafeCritical]
                void ProcessRow(int y)
                {
                    IBitmapDataRowInternal row = accessor.GetRowCached(y);
                    for (int x = 0; x < bitmapData.Width; x++)
                        row.DoSetColor32(x, ditheringSession.GetDitheredColor(transformFunction.Invoke(row.DoGetColor32(x)), x, y));
                }

                #endregion
            }
            finally
            {
                if (!ReferenceEquals(bitmapData, accessor))
                    accessor.Dispose();
            }
        }

        [SecuritySafeCritical]
        [SuppressMessage("ReSharper", "AccessToDisposedClosure", Justification = "ParallelHelper.For invokes delegates before returning")]
        private static bool DoTransformColors(IAsyncContext context, IReadWriteBitmapData bitmapData, Func<Color64, Color64> transformFunction)
        {
            // Indexed format: processing the palette entries when possible
            if (bitmapData is IBitmapDataInternal bitmapDataInternal && bitmapDataInternal.CanSetPalette)
            {
                context.Progress?.New(DrawingOperation.ProcessingPixels, 1);
                Palette palette = bitmapData.Palette!;
                Color32[] oldEntries = palette.Entries;
                Color32[] newEntries = new Color32[oldEntries.Length];
                for (int i = 0; i < newEntries.Length; i++)
                    newEntries[i] = transformFunction.Invoke(oldEntries[i].ToColor64()).ToColor32();
                if (bitmapDataInternal.TrySetPalette(new Palette(newEntries, palette.BackColor, palette.AlphaThreshold, palette.WorkingColorSpace, null)))
                {
                    context.Progress?.Complete();
                    return !context.IsCancellationRequested;
                }

                Debug.Fail("Setting the palette of the same size should work if CanSetPalette is true");
            }

            if (bitmapData.Height < 1)
                return !context.IsCancellationRequested;

            // Non-indexed format or palette cannot be set: processing the pixels
            var accessor = bitmapData as IBitmapDataInternal ?? new BitmapDataWrapper(bitmapData, true, true);
            try
            {
                // Sequential processing
                if (bitmapData.Width < parallelThreshold)
                {
                    context.Progress?.New(DrawingOperation.ProcessingPixels, bitmapData.Height);
                    IBitmapDataRowInternal row = accessor.GetRowCached(0);
                    do
                    {
                        if (context.IsCancellationRequested)
                            return false;
                        for (int x = 0; x < bitmapData.Width; x++)
                            row.DoSetColor64(x, transformFunction.Invoke(row.DoGetColor64(x)));
                        context.Progress?.Increment();
                    } while (row.MoveNextRow());

                    return true;
                }

                // Parallel processing
                return ParallelHelper.For(context, DrawingOperation.ProcessingPixels, 0, bitmapData.Height, ProcessRow);

                #region Local Methods

                [SecuritySafeCritical]
                void ProcessRow(int y)
                {
                    IBitmapDataRowInternal row = accessor.GetRowCached(y);
                    for (int x = 0; x < bitmapData.Width; x++)
                        row.DoSetColor64(x, transformFunction.Invoke(row.DoGetColor64(x)));
                }

                #endregion
            }
            finally
            {
                if (!ReferenceEquals(bitmapData, accessor))
                    accessor.Dispose();
            }
        }

        [SecuritySafeCritical]
        [SuppressMessage("ReSharper", "AccessToDisposedClosure", Justification = "ParallelHelper.For invokes delegates before returning")]
        private static bool DoTransformColors(IAsyncContext context, IReadWriteBitmapData bitmapData, Func<ColorF, ColorF> transformFunction)
        {
            // Indexed format: processing the palette entries when possible
            if (bitmapData is IBitmapDataInternal bitmapDataInternal && bitmapDataInternal.CanSetPalette)
            {
                context.Progress?.New(DrawingOperation.ProcessingPixels, 1);
                Palette palette = bitmapData.Palette!;
                Color32[] oldEntries = palette.Entries;
                Color32[] newEntries = new Color32[oldEntries.Length];
                for (int i = 0; i < newEntries.Length; i++)
                    newEntries[i] = transformFunction.Invoke(oldEntries[i].ToColorF()).ToColor32();
                if (bitmapDataInternal.TrySetPalette(new Palette(newEntries, palette.BackColor, palette.AlphaThreshold, palette.WorkingColorSpace, null)))
                {
                    context.Progress?.Complete();
                    return !context.IsCancellationRequested;
                }

                Debug.Fail("Setting the palette of the same size should work if CanSetPalette is true");
            }

            if (bitmapData.Height < 1)
                return !context.IsCancellationRequested;

            // Non-indexed format or palette cannot be set: processing the pixels
            var accessor = bitmapData as IBitmapDataInternal ?? new BitmapDataWrapper(bitmapData, true, true);
            try
            {
                // Sequential processing
                if (bitmapData.Width < parallelThreshold)
                {
                    context.Progress?.New(DrawingOperation.ProcessingPixels, bitmapData.Height);
                    IBitmapDataRowInternal row = accessor.GetRowCached(0);
                    do
                    {
                        if (context.IsCancellationRequested)
                            return false;
                        for (int x = 0; x < bitmapData.Width; x++)
                            row.DoSetColorF(x, transformFunction.Invoke(row.DoGetColorF(x)));
                        context.Progress?.Increment();
                    } while (row.MoveNextRow());

                    return true;
                }

                // Parallel processing
                return ParallelHelper.For(context, DrawingOperation.ProcessingPixels, 0, bitmapData.Height, ProcessRow);

                #region Local Methods

                [SecuritySafeCritical]
                void ProcessRow(int y)
                {
                    IBitmapDataRowInternal row = accessor.GetRowCached(y);
                    for (int x = 0; x < bitmapData.Width; x++)
                        row.DoSetColorF(x, transformFunction.Invoke(row.DoGetColorF(x)));
                }

                #endregion
            }
            finally
            {
                if (!ReferenceEquals(bitmapData, accessor))
                    accessor.Dispose();
            }
        }

        private static bool DoReplaceColor(IAsyncContext context, IReadWriteBitmapData bitmapData, Color32 oldColor, Color32 newColor, IDitherer? ditherer)
        {
            #region Local Methods

            static Color32 TransformReplaceColor32(Color32 c, Color32 oldColor, Color32 newColor) => c == oldColor ? newColor : c;
            static Color64 TransformReplaceColor64(Color64 c, Color64 oldColor, Color64 newColor) => c == oldColor ? newColor : c;
            static ColorF TransformReplaceColorF(ColorF c, ColorF oldColor, ColorF newColor) => c == oldColor ? newColor : c;

            #endregion

            if (ditherer == null)
            {
                var pixelFormat = bitmapData.PixelFormat;
                if (pixelFormat.Prefers128BitColors && (newColor.A is Byte.MinValue or Byte.MaxValue)
                    || (newColor.A is not (Byte.MinValue or Byte.MaxValue) && (bitmapData.LinearBlending() || pixelFormat.Prefers128BitColors && bitmapData.WorkingColorSpace != WorkingColorSpace.Srgb)))
                {
                    ColorF oldColorF = oldColor.ToColorF();
                    ColorF newColorF = newColor.ToColorF();
                    return DoTransformColors(context, bitmapData, c => TransformReplaceColorF(c, oldColorF, newColorF));
                }

                if (pixelFormat.IsWide)
                {
                    Color64 oldColor64 = oldColor.ToColor64();
                    Color64 newColor64 = newColor.ToColor64();
                    return DoTransformColors(context, bitmapData, c => TransformReplaceColor64(c, oldColor64, newColor64));
                }
            }

            return DoTransformColors(context, bitmapData, c => TransformReplaceColor32(c, oldColor, newColor), ditherer);
        }

        private static bool DoMakeTransparent(IAsyncContext context, IReadWriteBitmapData bitmapData)
        {
            // NOTE: returning when the bottom-left pixel is not completely opaque is intended, this is how also Bitmap.MakeTransparent works
            var pixelFormat = bitmapData.PixelFormat;
            if (pixelFormat.Prefers128BitColors)
            {
                ColorF transparentColorF = bitmapData.GetColorF(0, bitmapData.Height - 1);
                if (transparentColorF.A < 1f)
                    return !context.IsCancellationRequested;
                return DoTransformColors(context, bitmapData, c => c == transparentColorF ? default : c);
            }

            if (pixelFormat.Prefers64BitColors)
            {
                Color64 transparentColor64 = bitmapData.GetColor64(0, bitmapData.Height - 1);
                if (transparentColor64.A < UInt16.MaxValue)
                    return !context.IsCancellationRequested;
                return DoTransformColors(context, bitmapData, c => c == transparentColor64 ? default : c);
            }

            Color32 transparentColor32 = bitmapData[bitmapData.Height - 1][0];
            if (transparentColor32.A < Byte.MaxValue)
                return !context.IsCancellationRequested;
            return DoTransformColors(context, bitmapData, c => c == transparentColor32 ? default : c);
        }

        private static bool DoMakeTransparent(IAsyncContext context, IReadWriteBitmapData bitmapData, Color32 transparentColor)
            => DoReplaceColor(context, bitmapData, transparentColor, default, null);

        private static bool DoInvert(IAsyncContext context, IReadWriteBitmapData bitmapData, IDitherer? ditherer)
        {
            #region Local Methods

            static Color32 TransformInvert32(Color32 c) => new Color32(c.A, (byte)(Byte.MaxValue - c.R), (byte)(Byte.MaxValue - c.G), (byte)(Byte.MaxValue - c.B));
            static Color64 TransformInvert64(Color64 c) => new Color64(c.A, (ushort)(UInt16.MaxValue - c.R), (ushort)(UInt16.MaxValue - c.G), (ushort)(UInt16.MaxValue - c.B));

            static ColorF TransformInvertF(ColorF c)
            {
                c = c.Clip();
                return new ColorF(c.A, 1f - c.R, 1f - c.G, 1f - c.B);
            }

            #endregion

            if (ditherer == null)
            {
                var pixelFormat = bitmapData.PixelFormat;
                if (bitmapData.LinearBlending() || pixelFormat.Prefers128BitColors && bitmapData.WorkingColorSpace != WorkingColorSpace.Srgb)
                    return DoTransformColors(context, bitmapData, TransformInvertF);
                if (pixelFormat.IsWide)
                    return DoTransformColors(context, bitmapData, TransformInvert64);
            }

            return DoTransformColors(context, bitmapData, TransformInvert32, ditherer);
        }

        private static bool DoAdjustBrightness(IAsyncContext context, IReadWriteBitmapData bitmapData, float brightness, IDitherer? ditherer, ColorChannels channels)
        {
            #region Local Methods

            static Color32 TransformLighten32(Color32 c, float brightness, ColorChannels channels) => new Color32(c.A,
                (channels & ColorChannels.R) == ColorChannels.R ? (byte)((Byte.MaxValue - c.R) * brightness + c.R) : c.R,
                (channels & ColorChannels.G) == ColorChannels.G ? (byte)((Byte.MaxValue - c.G) * brightness + c.G) : c.G,
                (channels & ColorChannels.B) == ColorChannels.B ? (byte)((Byte.MaxValue - c.B) * brightness + c.B) : c.B);

            static Color32 TransformDarken32(Color32 c, float brightness, ColorChannels channels) => new Color32(c.A,
                (channels & ColorChannels.R) == ColorChannels.R ? (byte)(c.R * brightness) : c.R,
                (channels & ColorChannels.G) == ColorChannels.G ? (byte)(c.G * brightness) : c.G,
                (channels & ColorChannels.B) == ColorChannels.B ? (byte)(c.B * brightness) : c.B);

            static Color64 TransformLighten64(Color64 c, float brightness, ColorChannels channels) => new Color64(c.A,
                (channels & ColorChannels.R) == ColorChannels.R ? (ushort)((UInt16.MaxValue - c.R) * brightness + c.R) : c.R,
                (channels & ColorChannels.G) == ColorChannels.G ? (ushort)((UInt16.MaxValue - c.G) * brightness + c.G) : c.G,
                (channels & ColorChannels.B) == ColorChannels.B ? (ushort)((UInt16.MaxValue - c.B) * brightness + c.B) : c.B);

            static Color64 TransformDarken64(Color64 c, float brightness, ColorChannels channels) => new Color64(c.A,
                (channels & ColorChannels.R) == ColorChannels.R ? (ushort)(c.R * brightness) : c.R,
                (channels & ColorChannels.G) == ColorChannels.G ? (ushort)(c.G * brightness) : c.G,
                (channels & ColorChannels.B) == ColorChannels.B ? (ushort)(c.B * brightness) : c.B);

            static ColorF TransformLightenF(ColorF c, float brightness, ColorChannels channels)
            {
                c = c.Clip();
                return new ColorF(c.A,
                    (channels & ColorChannels.R) == ColorChannels.R ? (1f - c.R) * brightness + c.R : c.R,
                    (channels & ColorChannels.G) == ColorChannels.G ? (1f - c.G) * brightness + c.G : c.G,
                    (channels & ColorChannels.B) == ColorChannels.B ? (1f - c.B) * brightness + c.B : c.B);
            }

            static ColorF TransformDarkenF(ColorF c, float brightness, ColorChannels channels)
            {
                c = c.Clip();
                return new ColorF(c.A,
                    (channels & ColorChannels.R) == ColorChannels.R ? c.R * brightness : c.R,
                    (channels & ColorChannels.G) == ColorChannels.G ? c.G * brightness : c.G,
                    (channels & ColorChannels.B) == ColorChannels.B ? c.B * brightness : c.B);
            }

            #endregion

            Debug.Assert(channels != ColorChannels.None && brightness != 0f);
            bool darken = false;
            if (brightness < 0f)
            {
                brightness += 1f;
                darken = true;
            }

            if (ditherer == null)
            {
                var pixelFormat = bitmapData.PixelFormat;
                if (bitmapData.LinearBlending() || pixelFormat.Prefers128BitColors && bitmapData.WorkingColorSpace != WorkingColorSpace.Srgb)
                {
                    return darken
                        ? DoTransformColors(context, bitmapData, c => TransformDarkenF(c, brightness, channels))
                        : DoTransformColors(context, bitmapData, c => TransformLightenF(c, brightness, channels));
                }

                if (pixelFormat.IsWide)
                {
                    return darken
                        ? DoTransformColors(context, bitmapData, c => TransformDarken64(c, brightness, channels))
                        : DoTransformColors(context, bitmapData, c => TransformLighten64(c, brightness, channels));
                }
            }

            return darken
                ? DoTransformColors(context, bitmapData, c => TransformDarken32(c, brightness, channels), ditherer)
                : DoTransformColors(context, bitmapData, c => TransformLighten32(c, brightness, channels), ditherer);
        }

        private static bool DoAdjustContrast(IAsyncContext context, IReadWriteBitmapData bitmapData, float contrast, IDitherer? ditherer, ColorChannels channels)
        {
            #region Local Methods

            static Color32 TransformContrast32(Color32 c, float contrast, ColorChannels channels) => new Color32(c.A,
                (channels & ColorChannels.R) == ColorChannels.R ? ((int)((((float)c.R / Byte.MaxValue - 0.5f) * contrast + 0.5f) * Byte.MaxValue)).ClipToByte() : c.R,
                (channels & ColorChannels.G) == ColorChannels.G ? ((int)((((float)c.G / Byte.MaxValue - 0.5f) * contrast + 0.5f) * Byte.MaxValue)).ClipToByte() : c.G,
                (channels & ColorChannels.B) == ColorChannels.B ? ((int)((((float)c.B / Byte.MaxValue - 0.5f) * contrast + 0.5f) * Byte.MaxValue)).ClipToByte() : c.B);

            static Color64 TransformContrast64(Color64 c, float contrast, ColorChannels channels) => new Color64(c.A,
                (channels & ColorChannels.R) == ColorChannels.R ? ((int)((((float)c.R / UInt16.MaxValue - 0.5f) * contrast + 0.5f) * UInt16.MaxValue)).ClipToUInt16() : c.R,
                (channels & ColorChannels.G) == ColorChannels.G ? ((int)((((float)c.G / UInt16.MaxValue - 0.5f) * contrast + 0.5f) * UInt16.MaxValue)).ClipToUInt16() : c.G,
                (channels & ColorChannels.B) == ColorChannels.B ? ((int)((((float)c.B / UInt16.MaxValue - 0.5f) * contrast + 0.5f) * UInt16.MaxValue)).ClipToUInt16() : c.B);

            static ColorF TransformContrastF(ColorF c, float contrast, ColorChannels channels)
            {
                c = c.Clip();
                return new ColorF(c.A,
                    (channels & ColorChannels.R) == ColorChannels.R ? (c.R - 0.5f) * contrast + 0.5f : c.R,
                    (channels & ColorChannels.G) == ColorChannels.G ? (c.G - 0.5f) * contrast + 0.5f : c.G,
                    (channels & ColorChannels.B) == ColorChannels.B ? (c.B - 0.5f) * contrast + 0.5f : c.B);
            }

            #endregion

            contrast += 1f;
            contrast *= contrast;

            if (ditherer == null)
            {
                var pixelFormat = bitmapData.PixelFormat;
                if (bitmapData.LinearBlending() || pixelFormat.Prefers128BitColors && bitmapData.WorkingColorSpace != WorkingColorSpace.Srgb)
                    return DoTransformColors(context, bitmapData, c => TransformContrastF(c, contrast, channels));
                if (pixelFormat.IsWide)
                    return DoTransformColors(context, bitmapData, c => TransformContrast64(c, contrast, channels));
            }

            return DoTransformColors(context, bitmapData, c => TransformContrast32(c, contrast, channels), ditherer);
        }

        private static bool DoAdjustGamma(IAsyncContext context, IReadWriteBitmapData bitmapData, float gamma, IDitherer? ditherer, ColorChannels channels)
        {
            #region Local Methods

            static Color32 TransformGamma32(Color32 c, ColorChannels channels, byte[] table) => new Color32(c.A,
                (channels & ColorChannels.R) == ColorChannels.R ? table[c.R] : c.R,
                (channels & ColorChannels.G) == ColorChannels.G ? table[c.G] : c.G,
                (channels & ColorChannels.B) == ColorChannels.B ? table[c.B] : c.B);

            static Color64 TransformGamma64(Color64 c, ColorChannels channels, ushort[] table) => new Color64(c.A,
                (channels & ColorChannels.R) == ColorChannels.R ? table[c.R] : c.R,
                (channels & ColorChannels.G) == ColorChannels.G ? table[c.G] : c.G,
                (channels & ColorChannels.B) == ColorChannels.B ? table[c.B] : c.B);

            static ColorF TransformGammaF(ColorF c, ColorChannels channels, float gamma)
            {
                c = c.Clip();
                return new ColorF(c.A,
                    (channels & ColorChannels.R) == ColorChannels.R ? MathF.Pow(c.R, 1f / gamma) : c.R,
                    (channels & ColorChannels.G) == ColorChannels.G ? MathF.Pow(c.G, 1f / gamma) : c.G,
                    (channels & ColorChannels.B) == ColorChannels.B ? MathF.Pow(c.B, 1f / gamma) : c.B);
            }

            #endregion

            if (ditherer == null)
            {
                var pixelFormat = bitmapData.PixelFormat;
                if (bitmapData.LinearBlending() || pixelFormat.Prefers128BitColors && bitmapData.WorkingColorSpace != WorkingColorSpace.Srgb)
                    return DoTransformColors(context, bitmapData, c => TransformGammaF(c, channels, gamma));
                if (pixelFormat.IsWide)
                {
                    ushort[] table64 = GammaLookupTableCache64[gamma];
                    return DoTransformColors(context, bitmapData, c => TransformGamma64(c, channels, table64));
                }
            }

            byte[] table32 = GammaLookupTableCache32[gamma];
            return DoTransformColors(context, bitmapData, c => TransformGamma32(c, channels, table32), ditherer);
        }

        private static Color32 TransformMakeOpaque(Color32 c, Color32 backColor) => c.A == Byte.MaxValue ? c : c.BlendWithBackgroundSrgb(backColor);

        private static Color32 TransformMakeGrayscale(Color32 c) => c.ToGray();

        private static byte[] GenerateGammaLookupTable32(float gamma)
        {
            byte[] result = new byte[256];
            for (int i = 0; i < 256; i++)
                result[i] = ((int)(255f * MathF.Pow(i / 255f, 1f / gamma) + 0.5f)).ClipToByte();
            return result;
        }

        private static ushort[] GenerateGammaLookupTable64(float gamma)
        {
            ushort[] result = new ushort[65536];
            for (int i = 0; i < 65536; i++)
                result[i] = ((int)(65535f * MathF.Pow(i / 65535f, 1f / gamma) + 0.5f)).ClipToUInt16();
            return result;
        }

        #endregion

        #endregion

        #endregion
    }
}
