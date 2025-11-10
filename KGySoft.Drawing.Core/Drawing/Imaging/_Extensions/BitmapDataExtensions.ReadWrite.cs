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
#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
using System.Numerics;
#endif
#if NETCOREAPP3_0_OR_GREATER
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
#endif
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
            ValidateArguments(bitmapData, quantizer);
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
        /// <para>This method blocks the caller thread, but if <paramref name="context"/> belongs to an async top level method, then the execution may already run
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
            ValidateArguments(bitmapData, quantizer);
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
            ValidateArguments(bitmapData, quantizer);
            return AsyncHelper.BeginOperation(ctx => DoQuantize(ctx, bitmapData, quantizer), asyncConfig);
        }

        /// <summary>
        /// Waits for the pending asynchronous operation started by the <see cref="BeginQuantize">BeginQuantize</see> method to complete.
        /// In .NET Framework 4.0 and above you can use the <see cref="QuantizeAsync">QuantizeAsync</see> method instead.
        /// </summary>
        /// <param name="asyncResult">The reference to the pending asynchronous request to finish.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in the <c>asyncConfig</c> parameter was set to <see langword="false"/>.</returns>
        /// <exception cref="InvalidOperationException">The quantizer's <see cref="IQuantizer.Initialize">Initialize</see> method returned <see langword="null"/>.</exception>
        public static bool EndQuantize(this IAsyncResult asyncResult) => AsyncHelper.EndOperation<bool>(asyncResult, nameof(BeginQuantize));

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
        /// <returns>A task that represents the asynchronous operation. Its result is <see langword="true"/>, if the operation completed successfully,
        /// or <see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> or <paramref name="quantizer"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException">The <paramref name="quantizer"/>'s <see cref="IQuantizer.Initialize">Initialize</see> method returned <see langword="null"/>.</exception>
        /// <remarks>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="Quantize(IReadWriteBitmapData, IQuantizer)">Quantize</see> method for more details.</note>
        /// </remarks>
        public static Task<bool> QuantizeAsync(this IReadWriteBitmapData bitmapData, IQuantizer quantizer, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, quantizer);
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
        /// <para>This method blocks the caller thread, but if <paramref name="context"/> belongs to an async top level method, then the execution may already run
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
            ValidateArguments(bitmapData, quantizer, ditherer);
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
            ValidateArguments(bitmapData, quantizer, ditherer);
            return AsyncHelper.BeginOperation(ctx => DoDither(ctx, bitmapData, quantizer, ditherer), asyncConfig);
        }

        /// <summary>
        /// Waits for the pending asynchronous operation started by the <see cref="BeginDither">BeginDither</see> method to complete.
        /// In .NET Framework 4.0 and above you can use the <see cref="DitherAsync">QuantizeAsync</see> method instead.
        /// </summary>
        /// <param name="asyncResult">The reference to the pending asynchronous request to finish.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in the <c>asyncConfig</c> parameter was set to <see langword="false"/>.</returns>
        /// <exception cref="InvalidOperationException">The <see cref="IQuantizer.Initialize">IQuantizer.Initialize</see> method
        /// or the <see cref="IDitherer.Initialize">IDitherer.Initialize</see> method returned <see langword="null"/>.</exception>
        public static bool EndDither(this IAsyncResult asyncResult) => AsyncHelper.EndOperation<bool>(asyncResult, nameof(BeginDither));

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
        /// <returns>A task that represents the asynchronous operation. Its result is <see langword="true"/>, if the operation completed successfully,
        /// or <see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/>, <paramref name="quantizer"/> or <paramref name="ditherer"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException">The <see cref="IQuantizer.Initialize">IQuantizer.Initialize</see> method
        /// or the <see cref="IDitherer.Initialize">IDitherer.Initialize</see> method returned <see langword="null"/>.</exception>
        /// <remarks>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="Dither(IReadWriteBitmapData, IQuantizer, IDitherer)">Dither</see> method for more details.</note>
        /// </remarks>
        public static Task<bool> DitherAsync(this IReadWriteBitmapData bitmapData, IQuantizer quantizer, IDitherer ditherer, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, quantizer, ditherer);
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
        /// <para>On multi-core systems <paramref name="transformFunction"/> might be called concurrently so it must be thread-safe.</para>
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
        /// <para>On multi-core systems <paramref name="transformFunction"/> might be called concurrently so it must be thread-safe.</para>
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
        /// <para>On multi-core systems <paramref name="transformFunction"/> might be called concurrently so it must be thread-safe.</para>
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
        /// <para>This method blocks the caller thread, but if <paramref name="context"/> belongs to an async top level method, then the execution may already run
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
        /// <returns>A task that represents the asynchronous operation. Its result is <see langword="true"/>, if the operation completed successfully,
        /// or <see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
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
        /// <para>On multi-core systems <paramref name="transformFunction"/> might be called concurrently so it must be thread-safe.</para>
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
        /// <para>This method blocks the caller thread, but if <paramref name="context"/> belongs to an async top level method, then the execution may already run
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
        /// <returns>A task that represents the asynchronous operation. Its result is <see langword="true"/>, if the operation completed successfully,
        /// or <see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
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
        /// <para>On multi-core systems <paramref name="transformFunction"/> might be called concurrently so it must be thread-safe.</para>
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
        /// <para>This method blocks the caller thread, but if <paramref name="context"/> belongs to an async top level method, then the execution may already run
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
        /// <returns>A task that represents the asynchronous operation. Its result is <see langword="true"/>, if the operation completed successfully,
        /// or <see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
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
            ValidateArguments(bitmapData);
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
            ValidateArguments(bitmapData);
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
        /// <para>This method blocks the caller thread, but if <paramref name="context"/> belongs to an async top level method, then the execution may already run
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
            ValidateArguments(bitmapData);
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
            ValidateArguments(bitmapData);
            if (oldColor == newColor)
                return AsyncHelper.FromResult(true, asyncConfig);

            return AsyncHelper.BeginOperation(ctx => DoReplaceColor(ctx, bitmapData, oldColor, newColor, ditherer), asyncConfig);
        }

        /// <summary>
        /// Waits for the pending asynchronous operation started by the <see cref="BeginReplaceColor">BeginReplaceColor</see> method to complete.
        /// In .NET Framework 4.0 and above you can use the <see cref="ReplaceColorAsync">ReplaceColorAsync</see> method instead.
        /// </summary>
        /// <param name="asyncResult">The reference to the pending asynchronous request to finish.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in the <c>asyncConfig</c> parameter was set to <see langword="false"/>.</returns>
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
        /// <returns>A task that represents the asynchronous operation. Its result is <see langword="true"/>, if the operation completed successfully,
        /// or <see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="ReplaceColor(IReadWriteBitmapData,Color32,Color32,IDitherer?)">ReplaceColor</see> method for more details.</note>
        /// </remarks>
        public static Task<bool> ReplaceColorAsync(this IReadWriteBitmapData bitmapData, Color32 oldColor, Color32 newColor, IDitherer? ditherer = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData);
            if (oldColor == newColor)
                return AsyncHelper.FromResult(true, asyncConfig);

            return AsyncHelper.DoOperationAsync(ctx => DoReplaceColor(ctx, bitmapData, oldColor, newColor, ditherer), asyncConfig);
        }
#endif

        #endregion

        #region Invert

        /// <summary>
        /// Inverts the colors of the specified <paramref name="bitmapData"/>.
        /// <div style="display: none;"><br/>See the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_Invert.htm">online help</a> of the <c>BitmapExtensions.Invert</c> method for a couple of examples with images.</div>
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
        /// <note type="tip">See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_Invert.htm">BitmapExtensions.Invert</a> method for a couple of examples.</note>
        /// </remarks>
        public static void Invert(this IReadWriteBitmapData bitmapData, IDitherer? ditherer = null)
        {
            ValidateArguments(bitmapData);
            DoInvert(AsyncHelper.DefaultContext, bitmapData, ditherer);
        }

        /// <summary>
        /// Inverts the colors of the specified <paramref name="bitmapData"/>.
        /// <div style="display: none;"><br/>See the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_Invert.htm">online help</a> of the <c>BitmapExtensions.Invert</c> method for a couple of examples with images.</div>
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
        /// <note type="tip">See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_Invert.htm">BitmapExtensions.Invert</a> method for a couple of examples.</note>
        /// </remarks>
        public static bool Invert(this IReadWriteBitmapData bitmapData, IDitherer? ditherer, ParallelConfig? parallelConfig)
        {
            ValidateArguments(bitmapData);
            return AsyncHelper.DoOperationSynchronously(ctx => DoInvert(ctx, bitmapData, ditherer), parallelConfig);
        }

        /// <summary>
        /// Inverts the colors of the specified <paramref name="bitmapData"/>, using a <paramref name="context"/> that may belong to a higher level, possibly asynchronous operation.
        /// <div style="display: none;"><br/>See the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_Invert.htm">online help</a> of the <c>BitmapExtensions.Invert</c> method for a couple of examples with images.</div>
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to be inverted.</param>
        /// <param name="context">An <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncContext.htm">IAsyncContext</a> instance
        /// that contains information for asynchronous processing about the current operation.</param>
        /// <param name="ditherer">An optional <see cref="IDitherer"/> instance to dither the result of the transformation if the inverse of the <paramref name="bitmapData"/>
        /// has no exact representation with its <see cref="IBitmapData.PixelFormat"/>.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled.</returns>
        /// <remarks>
        /// <para>This method blocks the caller thread, but if <paramref name="context"/> belongs to an async top level method, then the execution may already run
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
        /// <div style="display: none;"><br/>See the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_Invert.htm">online help</a> of the <c>BitmapExtensions.Invert</c> method for a couple of examples with images.</div>
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
        /// <div style="display: none;"><br/>See the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_Invert.htm">online help</a> of the <c>BitmapExtensions.Invert</c> method for a couple of examples with images.</div>
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
        /// <returns>A task that represents the asynchronous operation. Its result is <see langword="true"/>, if the operation completed successfully,
        /// or <see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
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
        /// <note>Please note that unlike the <see cref="MakeOpaque(IReadWriteBitmapData,Color32,IDitherer?)">MakeOpaque</see> method, this one changes exactly one color shade without any tolerance.
        /// For customizations use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.TransformColors">TransformColors</see> overloads instead.</note>
        /// </remarks>
        /// <seealso cref="ToTransparent(IReadableBitmapData)"/>
        /// <seealso cref="MakeOpaque(IReadWriteBitmapData,Color32,IDitherer?)"/>
        public static void MakeTransparent(this IReadWriteBitmapData bitmapData)
        {
            ValidateArguments(bitmapData);
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
        /// <note>Please note that unlike the <see cref="MakeOpaque(IReadWriteBitmapData,Color32,IDitherer?,ParallelConfig?)">MakeOpaque</see> method, this one changes exactly one color shade without any tolerance.
        /// For customizations use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.TransformColors">TransformColors</see> overloads instead.</note>
        /// </remarks>
        /// <seealso cref="ToTransparent(IReadableBitmapData)"/>
        /// <seealso cref="MakeOpaque(IReadWriteBitmapData,Color32,IDitherer?,ParallelConfig?)"/>
        public static bool MakeTransparent(this IReadWriteBitmapData bitmapData, ParallelConfig? parallelConfig)
        {
            ValidateArguments(bitmapData);
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
        /// <para>This method blocks the caller thread, but if <paramref name="context"/> belongs to an async top level method, then the execution may already run
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
        /// <seealso cref="MakeOpaque(IReadWriteBitmapData,IAsyncContext?,Color32,IDitherer?)"/>
        public static bool MakeTransparent(this IReadWriteBitmapData bitmapData, IAsyncContext? context)
        {
            ValidateArguments(bitmapData);
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
        /// <note>Please note that unlike the <see cref="MakeOpaque(IReadWriteBitmapData,Color32,IDitherer?)">MakeOpaque</see> method, this one changes exactly one color shade without any tolerance.
        /// For customizations use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.TransformColors">TransformColors</see> overloads instead.</note>
        /// </remarks>
        /// <seealso cref="ToTransparent(IReadableBitmapData,Color32)"/>
        /// <seealso cref="MakeOpaque(IReadWriteBitmapData,Color32,IDitherer?)"/>
        public static void MakeTransparent(this IReadWriteBitmapData bitmapData, Color32 transparentColor)
        {
            ValidateArguments(bitmapData);
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
        /// <note>Please note that unlike the <see cref="MakeOpaque(IReadWriteBitmapData,Color32,IDitherer?,ParallelConfig?)">MakeOpaque</see> method, this one changes exactly one color shade without any tolerance.
        /// For customizations use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.TransformColors">TransformColors</see> overloads instead.</note>
        /// </remarks>
        /// <seealso cref="ToTransparent(IReadableBitmapData,Color32)"/>
        /// <seealso cref="MakeOpaque(IReadWriteBitmapData,Color32,IDitherer?,ParallelConfig?)"/>
        public static bool MakeTransparent(this IReadWriteBitmapData bitmapData, Color32 transparentColor, ParallelConfig? parallelConfig)
        {
            ValidateArguments(bitmapData);
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
        /// <para>This method blocks the caller thread, but if <paramref name="context"/> belongs to an async top level method, then the execution may already run
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
        /// <seealso cref="MakeOpaque(IReadWriteBitmapData,IAsyncContext?,Color32,IDitherer?)"/>
        public static bool MakeTransparent(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Color32 transparentColor)
        {
            ValidateArguments(bitmapData);
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
            ValidateArguments(bitmapData);
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
            ValidateArguments(bitmapData);
            if (transparentColor.A == 0)
                return AsyncHelper.FromResult(true, asyncConfig);
            return AsyncHelper.BeginOperation(ctx => DoMakeTransparent(ctx, bitmapData, transparentColor), asyncConfig);
        }

        /// <summary>
        /// Waits for the pending asynchronous operation started by the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.BeginMakeTransparent">BeginMakeTransparent</see> methods to complete.
        /// In .NET Framework 4.0 and above you can use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.MakeTransparentAsync">MakeTransparentAsync</see> methods instead.
        /// </summary>
        /// <param name="asyncResult">The reference to the pending asynchronous request to finish.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in the <c>asyncConfig</c> parameter was set to <see langword="false"/>.</returns>
        public static bool EndMakeTransparent(this IAsyncResult asyncResult) => AsyncHelper.EndOperation<bool>(asyncResult, nameof(BeginMakeTransparent));

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
        /// <returns>A task that represents the asynchronous operation. Its result is <see langword="true"/>, if the operation completed successfully,
        /// or <see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="MakeTransparent(IReadWriteBitmapData)"/> method for more details.</note>
        /// </remarks>
        /// <seealso cref="ToTransparentAsync(IReadableBitmapData, TaskConfig)"/>
        /// <seealso cref="MakeOpaqueAsync"/>
        public static Task<bool> MakeTransparentAsync(this IReadWriteBitmapData bitmapData, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData);
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
        /// <returns>A task that represents the asynchronous operation. Its result is <see langword="true"/>, if the operation completed successfully,
        /// or <see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="MakeTransparent(IReadWriteBitmapData, Color32)"/> method for more details.</note>
        /// </remarks>
        /// <seealso cref="ToTransparentAsync(IReadableBitmapData,Color32,TaskConfig)"/>
        /// <seealso cref="MakeOpaqueAsync"/>
        public static Task<bool> MakeTransparentAsync(this IReadWriteBitmapData bitmapData, Color32 transparentColor, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData);
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
        /// The <see cref="Color32.A">Color32.A</see> field of the specified color is ignored.</param>
        /// <param name="ditherer">An optional <see cref="IDitherer"/> instance to dither the result of the transformation if the inverse of the <paramref name="bitmapData"/>
        /// has no exact representation with its <see cref="IBitmapData.PixelFormat"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. You can use
        /// the <see cref="MakeOpaque(IReadWriteBitmapData, Color32, IDitherer, ParallelConfig)"/> overload to configure these, while still executing the method synchronously.
        /// Alternatively, use the <see cref="BeginMakeOpaque">BeginMakeOpaque</see> or <see cref="MakeOpaqueAsync">MakeOpaqueAsync</see> (in .NET Framework 4.0 and above) methods for asynchronous call and to adjust parallelization, set up cancellation and for reporting progress.</note>
        /// <para>If <paramref name="bitmapData"/> has an indexed <see cref="IBitmapData.PixelFormat"/> and <paramref name="ditherer"/> is <see langword="null"/>,
        /// then its palette entries are tried to be transformed instead of the actual pixels in the first place (if it is supported by <paramref name="bitmapData"/>).
        /// To transform the colors of an indexed <see cref="IBitmapData"/> without changing the palette specify a non-<see langword="null"/>&#160;<paramref name="ditherer"/>.
        /// Transforming the palette is both faster and provides a better result.</para>
        /// <para>If <paramref name="ditherer"/> is <see langword="null"/>, this method attempts to preserve the original color depth, including wide pixel formats.</para>
        /// <para>The <paramref name="ditherer"/> may have no effect for <see cref="KnownPixelFormat"/>s with more than 16 bits-per-pixel and for grayscale formats.</para>
        /// </remarks>
        public static void MakeOpaque(this IReadWriteBitmapData bitmapData, Color32 backColor, IDitherer? ditherer = null)
        {
            ValidateArguments(bitmapData);
            if (!bitmapData.HasAlpha())
                return;
            DoMakeOpaque(AsyncHelper.DefaultContext, bitmapData, backColor, ditherer);
        }

        /// <summary>
        /// Makes this <paramref name="bitmapData"/> opaque using the specified <paramref name="backColor"/>.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to make opaque.</param>
        /// <param name="backColor">Pixels with alpha in <paramref name="bitmapData"/> will be blended with this color.
        /// The <see cref="Color32.A">Color32.A</see> field of the specified color is ignored.</param>
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
        /// cancellation and progress reporting. Use the <see cref="BeginMakeOpaque">BeginMakeOpaque</see> or <see cref="MakeOpaqueAsync">MakeOpaqueAsync</see> (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// <para>If <paramref name="bitmapData"/> has an indexed <see cref="IBitmapData.PixelFormat"/> and <paramref name="ditherer"/> is <see langword="null"/>,
        /// then its palette entries are tried to be transformed instead of the actual pixels in the first place (if it is supported by <paramref name="bitmapData"/>).
        /// To transform the colors of an indexed <see cref="IBitmapData"/> without changing the palette specify a non-<see langword="null"/>&#160;<paramref name="ditherer"/>.
        /// Transforming the palette is both faster and provides a better result.</para>
        /// <para>If <paramref name="ditherer"/> is <see langword="null"/>, this method attempts to preserve the original color depth, including wide pixel formats.</para>
        /// <para>The <paramref name="ditherer"/> may have no effect for <see cref="KnownPixelFormat"/>s with more than 16 bits-per-pixel and for grayscale formats.</para>
        /// </remarks>
        public static bool MakeOpaque(this IReadWriteBitmapData bitmapData, Color32 backColor, IDitherer? ditherer, ParallelConfig? parallelConfig)
        {
            ValidateArguments(bitmapData);
            if (!bitmapData.HasAlpha())
                return AsyncHelper.FromResult(true, parallelConfig);
            return AsyncHelper.DoOperationSynchronously(ctx => DoMakeOpaque(ctx, bitmapData, backColor, ditherer), parallelConfig);
        }

        /// <summary>
        /// Makes this <paramref name="bitmapData"/> opaque using the specified <paramref name="backColor"/>,
        /// using a <paramref name="context"/> that may belong to a higher level, possibly asynchronous operation.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to make opaque.</param>
        /// <param name="context">An <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncContext.htm">IAsyncContext</a> instance
        /// that contains information for asynchronous processing about the current operation.</param>
        /// <param name="backColor">Pixels with alpha in <paramref name="bitmapData"/> will be blended with this color.
        /// The <see cref="Color32.A">Color32.A</see> field of the specified color is ignored.</param>
        /// <param name="ditherer">An optional <see cref="IDitherer"/> instance to dither the result of the transformation if the inverse of the <paramref name="bitmapData"/>
        /// has no exact representation with its <see cref="IBitmapData.PixelFormat"/>.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled.</returns>
        /// <remarks>
        /// <para>This method blocks the caller thread, but if <paramref name="context"/> belongs to an async top level method, then the execution may already run
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
        public static bool MakeOpaque(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Color32 backColor, IDitherer? ditherer = null)
        {
            ValidateArguments(bitmapData);
            if (!bitmapData.HasAlpha())
                return true;
            return DoMakeOpaque(context ?? AsyncHelper.DefaultContext, bitmapData, backColor, ditherer);
        }

        /// <summary>
        /// Begins to make this <paramref name="bitmapData"/> opaque asynchronously using the specified <paramref name="backColor"/>.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to make opaque.</param>
        /// <param name="backColor">Pixels with alpha in <paramref name="bitmapData"/> will be blended with this color.
        /// The <see cref="Color32.A">Color32.A</see> field of the specified color is ignored.</param>
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
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="MakeOpaque(IReadWriteBitmapData,Color32,IDitherer?)">MakeOpaque</see> method for more details.</note>
        /// <remarks>
        /// </remarks>
        public static IAsyncResult BeginMakeOpaque(this IReadWriteBitmapData bitmapData, Color32 backColor, IDitherer? ditherer = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData);
            if (!bitmapData.HasAlpha())
                return AsyncHelper.FromResult(true, asyncConfig);
            return AsyncHelper.BeginOperation(ctx => DoMakeOpaque(ctx, bitmapData, backColor, ditherer), asyncConfig);
        }

        /// <summary>
        /// Waits for the pending asynchronous operation started by the <see cref="BeginMakeOpaque">BeginMakeOpaque</see> method to complete.
        /// In .NET Framework 4.0 and above you can use the <see cref="MakeOpaqueAsync">MakeOpaqueAsync</see> method instead.
        /// </summary>
        /// <param name="asyncResult">The reference to the pending asynchronous request to finish.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in the <c>asyncConfig</c> parameter was set to <see langword="false"/>.</returns>
        public static bool EndMakeOpaque(this IAsyncResult asyncResult) => AsyncHelper.EndOperation<bool>(asyncResult, nameof(BeginMakeOpaque));

#if !NET35
        /// <summary>
        /// Makes this <paramref name="bitmapData"/> opaque asynchronously using the specified <paramref name="backColor"/>.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to make opaque.</param>
        /// <param name="backColor">Pixels with alpha in <paramref name="bitmapData"/> will be blended with this color.
        /// The <see cref="Color32.A">Color32.A</see> field of the specified color is ignored.</param>
        /// <param name="ditherer">An optional <see cref="IDitherer"/> instance to dither the result of the transformation if the inverse of the <paramref name="bitmapData"/>
        /// has no exact representation with its <see cref="IBitmapData.PixelFormat"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. Its result is <see langword="true"/>, if the operation completed successfully,
        /// or <see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="MakeOpaque(IReadWriteBitmapData,Color32,IDitherer?)">MakeOpaque</see> method for more details.</note>
        /// </remarks>
        public static Task<bool> MakeOpaqueAsync(this IReadWriteBitmapData bitmapData, Color32 backColor, IDitherer? ditherer = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData);
            if (!bitmapData.HasAlpha())
                return AsyncHelper.FromResult(true, asyncConfig);
            return AsyncHelper.DoOperationAsync(ctx => DoMakeOpaque(ctx, bitmapData, backColor, ditherer), asyncConfig);
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
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. You can use
        /// the <see cref="MakeGrayscale(IReadWriteBitmapData, IDitherer, ParallelConfig)"/> overload to configure these, while still executing the method synchronously.
        /// Alternatively, use the <see cref="BeginMakeGrayscale">BeginMakeGrayscale</see> or <see cref="MakeGrayscaleAsync">MakeGrayscaleAsync</see>
        /// (in .NET Framework 4.0 and above) methods for asynchronous call and to adjust parallelization, set up cancellation and for reporting progress.</note>
        /// <para>This method transforms the <paramref name="bitmapData"/> in place (its original content will be overwritten). To return a new instance
        /// use the <see cref="ToGrayscale(IReadableBitmapData)">ToGrayscale</see> extension method, which returns a new instance,
        /// or the <see cref="Clone(IReadableBitmapData, KnownPixelFormat, IQuantizer, IDitherer)">Clone</see> method with a grayscale
        /// quantizer (<see cref="PredefinedColorsQuantizer.Grayscale(Color32,byte)">PredefinedColorsQuantizer.Grayscale</see>, for example).</para>
        /// <para>If <paramref name="bitmapData"/> has an indexed <see cref="IBitmapData.PixelFormat"/> and <paramref name="ditherer"/> is <see langword="null"/>,
        /// then its palette entries are tried to be transformed instead of the actual pixels in the first place (if it is supported by <paramref name="bitmapData"/>).
        /// To transform the colors of an indexed <see cref="IBitmapData"/> without changing the palette specify a non-<see langword="null"/>&#160;<paramref name="ditherer"/>.
        /// Transforming the palette is both faster and provides a better result.</para>
        /// <para>If <paramref name="ditherer"/> is <see langword="null"/>, this method attempts to preserve the original color depth, including wide pixel formats.</para>
        /// <para>The <paramref name="ditherer"/> may have no effect for <see cref="KnownPixelFormat"/>s with more than 16 bits-per-pixel and for grayscale formats.</para>
        /// </remarks>
        /// <seealso cref="ToGrayscale(IReadableBitmapData)"/>
        public static void MakeGrayscale(this IReadWriteBitmapData bitmapData, IDitherer? ditherer = null)
        {
            ValidateArguments(bitmapData);
            DoMakeGrayscale(AsyncHelper.DefaultContext, bitmapData, ditherer);
        }

        /// <summary>
        /// Makes this <paramref name="bitmapData"/> grayscale.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to make grayscale.</param>
        /// <param name="ditherer">An optional <see cref="IDitherer"/> instance to dither the result if grayscale colors
        /// cannot be represented by the <see cref="IBitmapData.PixelFormat"/> or the current palette of the specified <paramref name="bitmapData"/>.</param>
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
        /// cancellation and progress reporting. Use the <see cref="BeginMakeGrayscale">BeginMakeGrayscale</see>
        /// or <see cref="MakeGrayscaleAsync">MakeGrayscaleAsync</see> (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// <para>This method transforms the <paramref name="bitmapData"/> in place (its original content will be overwritten). To return a new instance
        /// use the <see cref="ToGrayscale(IReadableBitmapData,ParallelConfig)">ToGrayscale</see> extension method, which returns a new instance,
        /// or the <see cref="Clone(IReadableBitmapData, KnownPixelFormat, IQuantizer, IDitherer)">Clone</see> method with a grayscale
        /// quantizer (<see cref="PredefinedColorsQuantizer.Grayscale(Color32,byte)">PredefinedColorsQuantizer.Grayscale</see>, for example).</para>
        /// <para>If <paramref name="bitmapData"/> has an indexed <see cref="IBitmapData.PixelFormat"/> and <paramref name="ditherer"/> is <see langword="null"/>,
        /// then its palette entries are tried to be transformed instead of the actual pixels in the first place (if it is supported by <paramref name="bitmapData"/>).
        /// To transform the colors of an indexed <see cref="IBitmapData"/> without changing the palette specify a non-<see langword="null"/>&#160;<paramref name="ditherer"/>.
        /// Transforming the palette is both faster and provides a better result.</para>
        /// <para>If <paramref name="ditherer"/> is <see langword="null"/>, this method attempts to preserve the original color depth, including wide pixel formats.</para>
        /// <para>The <paramref name="ditherer"/> may have no effect for <see cref="KnownPixelFormat"/>s with more than 16 bits-per-pixel and for grayscale formats.</para>
        /// </remarks>
        /// <seealso cref="ToGrayscale(IReadableBitmapData,ParallelConfig)"/>
        public static bool MakeGrayscale(this IReadWriteBitmapData bitmapData, IDitherer? ditherer, ParallelConfig? parallelConfig)
        {
            ValidateArguments(bitmapData);
            return AsyncHelper.DoOperationSynchronously(ctx => DoMakeGrayscale(ctx, bitmapData, ditherer), parallelConfig);
        }

        /// <summary>
        /// Makes this <paramref name="bitmapData"/> grayscale, using a <paramref name="context"/> that may belong to a higher level, possibly asynchronous operation.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to make grayscale.</param>
        /// <param name="context">An <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncContext.htm">IAsyncContext</a> instance
        /// that contains information for asynchronous processing about the current operation.</param>
        /// <param name="ditherer">An optional <see cref="IDitherer"/> instance to dither the result if grayscale colors
        /// cannot be represented by the <see cref="IBitmapData.PixelFormat"/> or the current palette of the specified <paramref name="bitmapData"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled.</returns>
        /// <remarks>
        /// <para>This method blocks the caller thread, but if <paramref name="context"/> belongs to an async top level method, then the execution may already run
        /// on a pool thread. Degree of parallelism, the ability of cancellation and reporting progress depend on how these were configured at the top level method.
        /// To reconfigure the degree of parallelism of an existing context, you can use the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_AsyncContextWrapper.htm">AsyncContextWrapper</a> class.</para>
        /// <para>Alternatively, you can use this method to specify the degree of parallelism for synchronous execution. For example, by
        /// passing <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncHelper_SingleThreadContext.htm">AsyncHelper.SingleThreadContext</a> to the <paramref name="context"/> parameter
        /// the method will be forced to use a single thread only.</para>
        /// <para>When reporting progress, this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.</para>
        /// <note type="tip">See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_AsyncHelper.htm">AsyncHelper</a>
        /// class for details about how to create a context for possibly async top level methods.</note>
        /// <note>See the <see cref="MakeGrayscale(IReadWriteBitmapData,IDitherer?)"/> overload for more details.</note>
        /// </remarks>
        /// <seealso cref="ToGrayscale(IReadableBitmapData,IAsyncContext)"/>
        public static bool MakeGrayscale(this IReadWriteBitmapData bitmapData, IAsyncContext? context, IDitherer? ditherer = null)
        {
            ValidateArguments(bitmapData);
            return DoMakeGrayscale(context ?? AsyncHelper.DefaultContext, bitmapData, ditherer);
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
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="MakeGrayscale(IReadWriteBitmapData,IDitherer?)">MakeGrayscale</see> method for more details.</note>
        /// </remarks>
        /// <seealso cref="BeginToGrayscale"/>
        public static IAsyncResult BeginMakeGrayscale(this IReadWriteBitmapData bitmapData, IDitherer? ditherer = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData);
            return AsyncHelper.BeginOperation(ctx => DoMakeGrayscale(ctx, bitmapData, ditherer), asyncConfig);
        }

        /// <summary>
        /// Waits for the pending asynchronous operation started by the <see cref="BeginMakeGrayscale">BeginMakeGrayscale</see> method to complete.
        /// In .NET Framework 4.0 and above you can use the <see cref="MakeGrayscaleAsync">MakeGrayscaleAsync</see> method instead.
        /// </summary>
        /// <param name="asyncResult">The reference to the pending asynchronous request to finish.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in the <c>asyncConfig</c> parameter was set to <see langword="false"/>.</returns>
        public static bool EndMakeGrayscale(this IAsyncResult asyncResult) => AsyncHelper.EndOperation<bool>(asyncResult, nameof(BeginMakeGrayscale));

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
        /// <returns>A task that represents the asynchronous operation. Its result is <see langword="true"/>, if the operation completed successfully,
        /// or <see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="MakeGrayscale(IReadWriteBitmapData,IDitherer?)">MakeGrayscale</see> method for more details.</note>
        /// </remarks>
        /// <seealso cref="ToGrayscaleAsync"/>
        public static Task<bool> MakeGrayscaleAsync(this IReadWriteBitmapData bitmapData, IDitherer? ditherer = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData);
            return AsyncHelper.DoOperationAsync(ctx => DoMakeGrayscale(ctx, bitmapData, ditherer), asyncConfig);
        }
#endif

        #endregion

        #region AdjustBrightness

        /// <summary>
        /// Adjusts the brightness of the specified <paramref name="bitmapData"/>.
        /// <div style="display: none;"><br/>See the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_AdjustBrightness.htm">online help</a> of the <c>BitmapExtensions.AdjustBrightness</c> method for some examples with images.</div>
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
        /// <note type="tip">See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_AdjustBrightness.htm">BitmapExtensions.AdjustBrightness</a> method for a few examples.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="brightness"/> is not between -1 and 1
        /// <br/>-or-
        /// <br/><paramref name="channels"/> is out of the defined flags.</exception>
        public static void AdjustBrightness(this IReadWriteBitmapData bitmapData, float brightness, IDitherer? ditherer = null, ColorChannels channels = ColorChannels.Rgb)
        {
            ValidateArguments(bitmapData, brightness, nameof(brightness), channels);

            // ReSharper disable once CompareOfFloatsByEqualityOperator - zero has a precise float representation
            if (channels == ColorChannels.None || brightness == 0f)
                return;
            DoAdjustBrightness(AsyncHelper.DefaultContext, bitmapData, brightness, ditherer, channels);
        }

        /// <summary>
        /// Adjusts the brightness of the specified <paramref name="bitmapData"/>.
        /// <div style="display: none;"><br/>See the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_AdjustBrightness.htm">online help</a> of the <c>BitmapExtensions.AdjustBrightness</c> method for some examples with images.</div>
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
        /// <note type="tip">See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_AdjustBrightness.htm">BitmapExtensions.AdjustBrightness</a> method for a few examples.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="brightness"/> is not between -1 and 1
        /// <br/>-or-
        /// <br/><paramref name="channels"/> is out of the defined flags.</exception>
        public static bool AdjustBrightness(this IReadWriteBitmapData bitmapData, float brightness, IDitherer? ditherer, ColorChannels channels, ParallelConfig? parallelConfig)
        {
            ValidateArguments(bitmapData, brightness, nameof(brightness), channels);

            // ReSharper disable once CompareOfFloatsByEqualityOperator - zero has a precise float representation
            if (channels == ColorChannels.None || brightness == 0f)
                return AsyncHelper.FromResult(true, parallelConfig);
            return AsyncHelper.DoOperationSynchronously(ctx => DoAdjustBrightness(ctx, bitmapData, brightness, ditherer, channels), parallelConfig);
        }

        /// <summary>
        /// Adjusts the brightness of the specified <paramref name="bitmapData"/>, using a <paramref name="context"/> that may belong to a higher level, possibly asynchronous operation.
        /// <div style="display: none;"><br/>See the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_AdjustBrightness.htm">online help</a> of the <c>BitmapExtensions.AdjustBrightness</c> method for some examples with images.</div>
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
        /// <para>This method blocks the caller thread, but if <paramref name="context"/> belongs to an async top level method, then the execution may already run
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
            ValidateArguments(bitmapData, brightness, nameof(brightness), channels);

            // ReSharper disable once CompareOfFloatsByEqualityOperator - zero has a precise float representation
            if (channels == ColorChannels.None || brightness == 0f)
                return true;
            return DoAdjustBrightness(context ?? AsyncHelper.DefaultContext, bitmapData, brightness, ditherer, channels);
        }

        /// <summary>
        /// Begins to adjust the brightness of the specified <paramref name="bitmapData"/> asynchronously.
        /// <div style="display: none;"><br/>See the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_AdjustBrightness.htm">online help</a> of the <c>BitmapExtensions.AdjustBrightness</c> method for some examples with images.</div>
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
        /// <note>See the <see cref="AdjustBrightness(IReadWriteBitmapData,float,IDitherer?,ColorChannels)"/> method for more details about the other parameters.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="brightness"/> is not between -1 and 1
        /// <br/>-or-
        /// <br/><paramref name="channels"/> is out of the defined flags.</exception>
        public static IAsyncResult BeginAdjustBrightness(this IReadWriteBitmapData bitmapData, float brightness, IDitherer? ditherer = null, ColorChannels channels = ColorChannels.Rgb, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, brightness, nameof(brightness), channels);

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
        /// <div style="display: none;"><br/>See the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_AdjustBrightness.htm">online help</a> of the <c>BitmapExtensions.AdjustBrightness</c> method for some examples with images.</div>
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
        /// <returns>A task that represents the asynchronous operation. Its result is <see langword="true"/>, if the operation completed successfully,
        /// or <see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <note>See the <see cref="AdjustBrightness(IReadWriteBitmapData,float,IDitherer?,ColorChannels)"/> method for more details about the other parameters.</note>
        /// <remarks>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="brightness"/> is not between -1 and 1
        /// <br/>-or-
        /// <br/><paramref name="channels"/> is out of the defined flags.</exception>
        public static Task<bool> AdjustBrightnessAsync(this IReadWriteBitmapData bitmapData, float brightness, IDitherer? ditherer = null, ColorChannels channels = ColorChannels.Rgb, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, brightness, nameof(brightness), channels);

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
        /// <div style="display: none;"><br/>See the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_AdjustContrast.htm">online help</a> of the <c>BitmapExtensions.AdjustContrast</c> method for some examples with images.</div>
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
        /// <note type="tip">See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_AdjustContrast.htm">BitmapExtensions.AdjustContrast</a> method for a few examples.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="contrast"/> is not between -1 and 1
        /// <br/>-or-
        /// <br/><paramref name="channels"/> is out of the defined flags.</exception>
        public static void AdjustContrast(this IReadWriteBitmapData bitmapData, float contrast, IDitherer? ditherer = null, ColorChannels channels = ColorChannels.Rgb)
        {
            ValidateArguments(bitmapData, contrast, nameof(contrast), channels);

            // ReSharper disable once CompareOfFloatsByEqualityOperator - zero has a precise float representation
            if (channels == ColorChannels.None || contrast == 0f)
                return;

            DoAdjustContrast(AsyncHelper.DefaultContext, bitmapData, contrast, ditherer, channels);
        }

        /// <summary>
        /// Adjusts the contrast of the specified <paramref name="bitmapData"/>.
        /// <div style="display: none;"><br/>See the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_AdjustContrast.htm">online help</a> of the <c>BitmapExtensions.AdjustContrast</c> method for some examples with images.</div>
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
        /// <note type="tip">See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_AdjustContrast.htm">BitmapExtensions.AdjustContrast</a> method for a few examples.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="contrast"/> is not between -1 and 1
        /// <br/>-or-
        /// <br/><paramref name="channels"/> is out of the defined flags.</exception>
        public static bool AdjustContrast(this IReadWriteBitmapData bitmapData, float contrast, IDitherer? ditherer, ColorChannels channels, ParallelConfig? parallelConfig)
        {
            ValidateArguments(bitmapData, contrast, nameof(contrast), channels);

            // ReSharper disable once CompareOfFloatsByEqualityOperator - zero has a precise float representation
            if (channels == ColorChannels.None || contrast == 0f)
                return AsyncHelper.FromResult(true, parallelConfig);

            return AsyncHelper.DoOperationSynchronously(ctx => DoAdjustContrast(ctx, bitmapData, contrast, ditherer, channels), parallelConfig);
        }

        /// <summary>
        /// Adjusts the contrast of the specified <paramref name="bitmapData"/>, using a <paramref name="context"/> that may belong to a higher level, possibly asynchronous operation.
        /// <div style="display: none;"><br/>See the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_AdjustContrast.htm">online help</a> of the <c>BitmapExtensions.AdjustContrast</c> method for some examples with images.</div>
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
        /// <para>This method blocks the caller thread, but if <paramref name="context"/> belongs to an async top level method, then the execution may already run
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
            ValidateArguments(bitmapData, contrast, nameof(contrast), channels);

            // ReSharper disable once CompareOfFloatsByEqualityOperator - zero has a precise float representation
            if (channels == ColorChannels.None || contrast == 0f)
                return true;

            return DoAdjustContrast(context ?? AsyncHelper.DefaultContext, bitmapData, contrast, ditherer, channels);
        }

        /// <summary>
        /// Begins to adjust the contrast of the specified <paramref name="bitmapData"/> asynchronously.
        /// <div style="display: none;"><br/>See the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_AdjustContrast.htm">online help</a> of the <c>BitmapExtensions.AdjustContrast</c> method for some examples with images.</div>
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
        /// <note>See the <see cref="AdjustContrast(IReadWriteBitmapData,float,IDitherer?,ColorChannels)"/> method for more details about the other parameters.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="contrast"/> is not between -1 and 1
        /// <br/>-or-
        /// <br/><paramref name="channels"/> is out of the defined flags.</exception>
        public static IAsyncResult BeginAdjustContrast(this IReadWriteBitmapData bitmapData, float contrast, IDitherer? ditherer = null, ColorChannels channels = ColorChannels.Rgb, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, contrast, nameof(contrast), channels);

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
        /// <div style="display: none;"><br/>See the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_AdjustContrast.htm">online help</a> of the <c>BitmapExtensions.AdjustContrast</c> method for some examples with images.</div>
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
        /// <returns>A task that represents the asynchronous operation. Its result is <see langword="true"/>, if the operation completed successfully,
        /// or <see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note>See the <see cref="AdjustContrast(IReadWriteBitmapData,float,IDitherer?,ColorChannels)"/> method for more details about the other parameters.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="contrast"/> is not between -1 and 1
        /// <br/>-or-
        /// <br/><paramref name="channels"/> is out of the defined flags.</exception>
        public static Task<bool> AdjustContrastAsync(this IReadWriteBitmapData bitmapData, float contrast, IDitherer? ditherer = null, ColorChannels channels = ColorChannels.Rgb, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, contrast, nameof(contrast), channels);

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
        /// <div style="display: none;"><br/>See the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_AdjustGamma.htm">online help</a> of the <c>BitmapExtensions.AdjustGamma</c> method for some examples with images.</div>
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
        /// <note type="tip">See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_AdjustGamma.htm">BitmapExtensions.AdjustGamma</a> method for a few examples.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="gamma"/> is not between 0 and 10
        /// <br/>-or-
        /// <br/><paramref name="channels"/> is out of the defined flags.</exception>
        public static void AdjustGamma(this IReadWriteBitmapData bitmapData, float gamma, IDitherer? ditherer = null, ColorChannels channels = ColorChannels.Rgb)
        {
            ValidateArguments(bitmapData, gamma, nameof(gamma), channels, 0f, 10f);

            // ReSharper disable once CompareOfFloatsByEqualityOperator - 1 has a precise float representation
            if (channels == ColorChannels.None || gamma == 1f)
                return;

            DoAdjustGamma(AsyncHelper.DefaultContext, bitmapData, gamma, ditherer, channels);
        }

        /// <summary>
        /// Adjusts the gamma correction of the specified <paramref name="bitmapData"/>.
        /// <div style="display: none;"><br/>See the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_AdjustGamma.htm">online help</a> of the <c>BitmapExtensions.AdjustGamma</c> method for some examples with images.</div>
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
        /// <note type="tip">See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_AdjustGamma.htm">BitmapExtensions.AdjustGamma</a> method for a few examples.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="gamma"/> is not between 0 and 10
        /// <br/>-or-
        /// <br/><paramref name="channels"/> is out of the defined flags.</exception>
        public static bool AdjustGamma(this IReadWriteBitmapData bitmapData, float gamma, IDitherer? ditherer, ColorChannels channels, ParallelConfig? parallelConfig)
        {
            ValidateArguments(bitmapData, gamma, nameof(gamma), channels, 0f, 10f);

            // ReSharper disable once CompareOfFloatsByEqualityOperator - 1 has a precise float representation
            if (channels == ColorChannels.None || gamma == 1f)
                return AsyncHelper.FromResult(true, parallelConfig);

            return AsyncHelper.DoOperationSynchronously(ctx => DoAdjustGamma(ctx, bitmapData, gamma, ditherer, channels), parallelConfig);
        }

        /// <summary>
        /// Adjusts the gamma correction of the specified <paramref name="bitmapData"/>, using a <paramref name="context"/> that may belong to a higher level, possibly asynchronous operation.
        /// <div style="display: none;"><br/>See the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_AdjustGamma.htm">online help</a> of the <c>BitmapExtensions.AdjustGamma</c> method for some examples with images.</div>
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
        /// <para>This method blocks the caller thread, but if <paramref name="context"/> belongs to an async top level method, then the execution may already run
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
            ValidateArguments(bitmapData, gamma, nameof(gamma), channels, 0f, 10f);

            // ReSharper disable once CompareOfFloatsByEqualityOperator - 1 has a precise float representation
            if (channels == ColorChannels.None || gamma == 1f)
                return true;

            return DoAdjustGamma(context ?? AsyncHelper.DefaultContext, bitmapData, gamma, ditherer, channels);
        }

        /// <summary>
        /// Begins to adjust the gamma correction of the specified <paramref name="bitmapData"/> asynchronously.
        /// <div style="display: none;"><br/>See the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_AdjustGamma.htm">online help</a> of the <c>BitmapExtensions.AdjustGamma</c> method for some examples with images.</div>
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
        /// <note>See the <see cref="AdjustGamma(IReadWriteBitmapData,float,IDitherer?,ColorChannels)"/> method for more details about the other parameters.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="gamma"/> is not between 0 and 10
        /// <br/>-or-
        /// <br/><paramref name="channels"/> is out of the defined flags.</exception>
        public static IAsyncResult BeginAdjustGamma(this IReadWriteBitmapData bitmapData, float gamma, IDitherer? ditherer = null, ColorChannels channels = ColorChannels.Rgb, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, gamma, nameof(gamma), channels, 0f, 10f);

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
        /// <div style="display: none;"><br/>See the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_AdjustGamma.htm">online help</a> of the <c>BitmapExtensions.AdjustGamma</c> method for some examples with images.</div>
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
        /// <returns>A task that represents the asynchronous operation. Its result is <see langword="true"/>, if the operation completed successfully,
        /// or <see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note>See the <see cref="AdjustGamma(IReadWriteBitmapData,float,IDitherer?,ColorChannels)"/> method for more details about the other parameters.</note>
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
            ValidateArguments(bitmapData);
            if (transformFunction == null)
                throw new ArgumentNullException(nameof(transformFunction), PublicResources.ArgumentNull);
        }

        private static void ValidateArguments(IReadWriteBitmapData bitmapData, IQuantizer quantizer)
        {
            ValidateArguments(bitmapData);
            if (quantizer == null)
                throw new ArgumentNullException(nameof(quantizer), PublicResources.ArgumentNull);
        }

        private static void ValidateArguments(IReadWriteBitmapData bitmapData, IQuantizer quantizer, IDitherer ditherer)
        {
            ValidateArguments(bitmapData, quantizer);
            if (ditherer == null)
                throw new ArgumentNullException(nameof(ditherer), PublicResources.ArgumentNull);
        }

        private static void ValidateArguments(IReadWriteBitmapData bitmapData, float value, string valueParamName, ColorChannels channels, float minValue = -1f, float maxValue = 1f)
        {
            ValidateArguments(bitmapData);
            if (value < minValue || value > maxValue || Single.IsNaN(value))
                throw new ArgumentOutOfRangeException(valueParamName, PublicResources.ArgumentMustBeBetween(minValue, maxValue));
            if (!channels.AllFlagsDefined())
                throw new ArgumentOutOfRangeException(nameof(channels), PublicResources.FlagsEnumOutOfRange(channels));
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
                // Not using premultiplied format because transformation is faster on simple ARGB32. Also, default backColor/alpha/colorSpace is fine, because DoCopy uses them from the target.
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

                context.Progress?.New(DrawingOperation.InitializingQuantizer); // predefined will be extreme fast, but in case someone tracks progress...
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

            AdjustDitherer(bitmapData, ref ditherer);
            if (ditherer == null)
            {
                var pixelFormat = bitmapData.PixelFormat;
                if (pixelFormat.Prefers128BitColors)
                {
                    ColorF oldColorF = oldColor.ToColorF();
                    ColorF newColorF = newColor.ToColorF();
                    return DoTransformColors(context, bitmapData, c => TransformReplaceColorF(c, oldColorF, newColorF));
                }

                if (pixelFormat.Prefers64BitColors)
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

            static Color32 TransformInvert32(Color32 c)
            {
#if NETCOREAPP3_0_OR_GREATER
                if (Sse2.IsSupported)
                {
                    Vector128<byte> bgra8 = Vector128.CreateScalar(c.Value).AsByte();
                    return new Color32(Sse2.Subtract(VectorExtensions.Max8BitU8, bgra8).WithElement(3, c.A).AsUInt32().ToScalar());
                }
#endif
#if NET7_0_OR_GREATER
                if (Vector64.IsHardwareAccelerated)
                {
                    Vector64<byte> bgra8 = Vector64.CreateScalar(c.Value).AsByte();
                    return new Color32((Vector64.Create(Byte.MaxValue) - bgra8).WithElement(3, c.A).AsUInt32().ToScalar());
                }

                if (Vector128.IsHardwareAccelerated)
                {
                    Vector128<byte> bgra8 = Vector128.CreateScalar(c.Value).AsByte();
                    return new Color32((VectorExtensions.Max8BitU8 - bgra8).WithElement(3, c.A).AsUInt32().ToScalar());
                }
#endif

                return new Color32(c.A, (byte)(Byte.MaxValue - c.R), (byte)(Byte.MaxValue - c.G), (byte)(Byte.MaxValue - c.B));
            }

            static Color64 TransformInvert64(Color64 c)
            {
#if NETCOREAPP3_0_OR_GREATER
                if (Sse2.IsSupported)
                {
                    Vector128<ushort> bgra16 = Vector128.CreateScalar(c.Value).AsUInt16();
                    return new Color64(Sse2.Subtract(VectorExtensions.Max16BitU16, bgra16).WithElement(3, c.A).AsUInt64().ToScalar());
                }
#endif
#if NET7_0_OR_GREATER
                if (Vector128.IsHardwareAccelerated)
                {
                    Vector128<ushort> bgra16 = Vector128.CreateScalar(c.Value).AsUInt16();
                    return new Color64((VectorExtensions.Max16BitU16 - bgra16).WithElement(3, c.A).AsUInt64().ToScalar());
                }

                if (Vector64.IsHardwareAccelerated)
                {
                    Vector64<ushort> bgra16 = Vector64.CreateScalar(c.Value).AsUInt16();
                    return new Color64((Vector64.Create(UInt16.MaxValue) - bgra16).WithElement(3, c.A).AsUInt64().ToScalar());
                }
#endif

                return new Color64(c.A, (ushort)(UInt16.MaxValue - c.R), (ushort)(UInt16.MaxValue - c.G), (ushort)(UInt16.MaxValue - c.B));
            }

            static ColorF TransformInvertF(ColorF c)
            {
#if NETCOREAPP3_0_OR_GREATER && !NET10_0_OR_GREATER // Starting with .NET 10, the auto-vectorized solution outperforms this one. See TransformInvertFTest in PerformanceTests.
                if (Sse.IsSupported)
                    return new ColorF(Sse.Subtract(VectorExtensions.OneF, c.RgbaV128.ClipF()).WithElement(3, c.A));
#endif
#if NET6_0_OR_GREATER || NET45_OR_GREATER || NETSTANDARD // Note that not using this version in .NET Core 3.x - .NET 5, because it is actually slower than the vanilla version.
                return new ColorF(new Vector4(Vector3.One - c.Rgb.ClipF(), c.A));
#else
                c = c.Clip();
                return new ColorF(c.A, 1f - c.R, 1f - c.G, 1f - c.B);
#endif
            }

            #endregion

            // Determining ColorF usage by IsLinearGamma rather than Prefers128BitColors, because the transform function produces a color space dependent result.
            // When there is dithering, we must use Color32, regardless of the color space.
            AdjustDitherer(bitmapData, ref ditherer);
            bool linear = bitmapData.IsLinearGamma();
            if (ditherer == null)
            {
                if (linear)
                    return DoTransformColors(context, bitmapData, TransformInvertF);
                if (bitmapData.PixelFormat.IsWide)
                    return DoTransformColors(context, bitmapData, TransformInvert64);
            }

            return DoTransformColors(context, bitmapData, linear ? c => TransformInvertF(c.ToColorF()).ToColor32() : TransformInvert32, ditherer);
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity",
            Justification = "False alarm, the new analyzer includes the complexity of local methods. And moving them outside this method would be a bad idea.")]
        private static bool DoAdjustBrightness(IAsyncContext context, IReadWriteBitmapData bitmapData, float brightness, IDitherer? ditherer, ColorChannels channels)
        {
            #region Local Methods

            static Color32 TransformLightenPerChannel32(Color32 c, float brightness, ColorChannels channels) => new Color32(c.A,
                (channels & ColorChannels.R) == ColorChannels.R ? (byte)((Byte.MaxValue - c.R) * brightness + c.R) : c.R,
                (channels & ColorChannels.G) == ColorChannels.G ? (byte)((Byte.MaxValue - c.G) * brightness + c.G) : c.G,
                (channels & ColorChannels.B) == ColorChannels.B ? (byte)((Byte.MaxValue - c.B) * brightness + c.B) : c.B);

            static Color32 TransformDarkenPerChannel32(Color32 c, float brightness, ColorChannels channels) => new Color32(c.A,
                (channels & ColorChannels.R) == ColorChannels.R ? (byte)(c.R * brightness) : c.R,
                (channels & ColorChannels.G) == ColorChannels.G ? (byte)(c.G * brightness) : c.G,
                (channels & ColorChannels.B) == ColorChannels.B ? (byte)(c.B * brightness) : c.B);

            static Color32 TransformLighten32(Color32 c, float brightness)
            {
#if NETCOREAPP3_0_OR_GREATER
                if (Sse2.IsSupported)
                {
                    Vector128<int> bgraI32 = Sse41.IsSupported
                        ? Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(c.Value).AsByte())
                        : Vector128.Create(c.B, c.G, c.R, default);

                    // bgrF = (float)(c.B, c.G, c.R, _)
                    Vector128<float> bgrF = Sse2.ConvertToVector128Single(bgraI32);

                    // bgrF = (255 - bgrF) * brightness + bgrF
                    bgrF = Fma.IsSupported
                        ? Fma.MultiplyAdd(Sse.Subtract(VectorExtensions.Max8BitF, bgrF), Vector128.Create(brightness), bgrF)
                        : Sse.Add(Sse.Multiply(Sse.Subtract(VectorExtensions.Max8BitF, bgrF), Vector128.Create(brightness)), bgrF);

                    bgraI32 = Sse2.ConvertToVector128Int32WithTruncation(bgrF);
                    return Ssse3.IsSupported
                        ? new Color32(Ssse3.Shuffle(bgraI32.AsByte(), VectorExtensions.PackLowBytesMask).WithElement(3, c.A).AsUInt32().ToScalar())
                        : new Color32(c.A, bgraI32.AsByte().GetElement(8), bgraI32.AsByte().GetElement(4), bgraI32.AsByte().GetElement(0));
                }
#endif
                return new Color32(c.A,
                    (byte)((Byte.MaxValue - c.R) * brightness + c.R),
                    (byte)((Byte.MaxValue - c.G) * brightness + c.G),
                    (byte)((Byte.MaxValue - c.B) * brightness + c.B));
            }

            static Color32 TransformDarken32(Color32 c, float brightness)
            {
#if NETCOREAPP3_0_OR_GREATER
                if (Sse2.IsSupported)
                {
                    Vector128<int> bgraI32 = Sse41.IsSupported
                        ? Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(c.Value).AsByte())
                        : Vector128.Create(c.B, c.G, c.R, default);

                    // bgrF = (float)(c.B, c.G, c.R, _)
                    Vector128<float> bgrF = Sse2.ConvertToVector128Single(bgraI32);

                    // bgrF *= brightness
                    bgrF = Sse.Multiply(bgrF, Vector128.Create(brightness));

                    bgraI32 = Sse2.ConvertToVector128Int32WithTruncation(bgrF);
                    return Ssse3.IsSupported
                        ? new Color32(Ssse3.Shuffle(bgraI32.AsByte(), VectorExtensions.PackLowBytesMask).WithElement(3, c.A).AsUInt32().ToScalar())
                        : new Color32(c.A, bgraI32.AsByte().GetElement(8), bgraI32.AsByte().GetElement(4), bgraI32.AsByte().GetElement(0));
                }

#endif
                return new Color32(c.A,
                    (byte)(c.R * brightness),
                    (byte)(c.G * brightness),
                    (byte)(c.B * brightness));
            }

            static Color64 TransformLightenPerChannel64(Color64 c, float brightness, ColorChannels channels) => new Color64(c.A,
                (channels & ColorChannels.R) == ColorChannels.R ? (ushort)((UInt16.MaxValue - c.R) * brightness + c.R) : c.R,
                (channels & ColorChannels.G) == ColorChannels.G ? (ushort)((UInt16.MaxValue - c.G) * brightness + c.G) : c.G,
                (channels & ColorChannels.B) == ColorChannels.B ? (ushort)((UInt16.MaxValue - c.B) * brightness + c.B) : c.B);

            static Color64 TransformDarkenPerChannel64(Color64 c, float brightness, ColorChannels channels) => new Color64(c.A,
                (channels & ColorChannels.R) == ColorChannels.R ? (ushort)(c.R * brightness) : c.R,
                (channels & ColorChannels.G) == ColorChannels.G ? (ushort)(c.G * brightness) : c.G,
                (channels & ColorChannels.B) == ColorChannels.B ? (ushort)(c.B * brightness) : c.B);

            static Color64 TransformLighten64(Color64 c, float brightness)
            {
#if NETCOREAPP3_0_OR_GREATER
                if (Sse41.IsSupported)
                {
                    // bgrF = (float)(c.B, c.G, c.R, _)
                    Vector128<float> bgrF = Sse2.ConvertToVector128Single(Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(c.Value).AsUInt16()));

                    // bgrF = (65535 - bgrF) * brightness + bgrF
                    bgrF = Fma.IsSupported
                        ? Fma.MultiplyAdd(Sse.Subtract(VectorExtensions.Max16BitF, bgrF), Vector128.Create(brightness), bgrF)
                        : Sse.Add(Sse.Multiply(Sse.Subtract(VectorExtensions.Max16BitF, bgrF), Vector128.Create(brightness)), bgrF);

                    Vector128<int> bgraI32 = Sse2.ConvertToVector128Int32WithTruncation(bgrF);
                    return new Color64(Sse41.PackUnsignedSaturate(bgraI32, bgraI32).WithElement(3, c.A).AsUInt64().ToScalar());
                }
#endif

                return new Color64(c.A,
                    (ushort)((UInt16.MaxValue - c.R) * brightness + c.R),
                    (ushort)((UInt16.MaxValue - c.G) * brightness + c.G),
                    (ushort)((UInt16.MaxValue - c.B) * brightness + c.B));
            }

            static Color64 TransformDarken64(Color64 c, float brightness)
            {
#if NETCOREAPP3_0_OR_GREATER
                if (Sse41.IsSupported)
                {
                    // bgrF = (float)(c.B, c.G, c.R, _)
                    Vector128<float> bgrF = Sse2.ConvertToVector128Single(Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(c.Value).AsUInt16()));

                    // bgrF *= brightness
                    bgrF = Sse.Multiply(bgrF, Vector128.Create(brightness));

                    Vector128<int> bgraI32 = Sse2.ConvertToVector128Int32WithTruncation(bgrF);
                    return new Color64(Sse41.PackUnsignedSaturate(bgraI32, bgraI32).WithElement(3, c.A).AsUInt64().ToScalar());
                }
#endif

                return new Color64(c.A,
                    (ushort)(c.R * brightness),
                    (ushort)(c.G * brightness),
                    (ushort)(c.B * brightness));
            }

            static ColorF TransformLightenPerChannelF(ColorF c, float brightness, ColorChannels channels)
            {
                c = c.Clip();
                return new ColorF(c.A,
                    (channels & ColorChannels.R) == ColorChannels.R ? (1f - c.R) * brightness + c.R : c.R,
                    (channels & ColorChannels.G) == ColorChannels.G ? (1f - c.G) * brightness + c.G : c.G,
                    (channels & ColorChannels.B) == ColorChannels.B ? (1f - c.B) * brightness + c.B : c.B);
            }

            static ColorF TransformDarkenPerChannelF(ColorF c, float brightness, ColorChannels channels)
            {
                c = c.Clip();
                return new ColorF(c.A,
                    (channels & ColorChannels.R) == ColorChannels.R ? c.R * brightness : c.R,
                    (channels & ColorChannels.G) == ColorChannels.G ? c.G * brightness : c.G,
                    (channels & ColorChannels.B) == ColorChannels.B ? c.B * brightness : c.B);
            }

            static ColorF TransformLightenF(ColorF c, float brightness)
            {
#if NETCOREAPP3_0_OR_GREATER
                // Using native vectorization if possible.
                if (Sse.IsSupported)
                {
                    Vector128<float> rgbaF = c.RgbaV128.ClipF();

                    // rgbaF = (1 - rgbaF) * brightness + rgbaF
                    rgbaF = Fma.IsSupported
                        ? Fma.MultiplyAdd(Sse.Subtract(VectorExtensions.OneF, rgbaF), Vector128.Create(brightness), rgbaF)
                        : Sse.Add(Sse.Multiply(Sse.Subtract(VectorExtensions.OneF, rgbaF), Vector128.Create(brightness)), rgbaF);
                    return new ColorF(rgbaF.WithElement(3, c.A));
                }
#endif

#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
                // The possibly still accelerated auto vectorization
                Vector3 rgbF = c.Rgb.ClipF();
                rgbF = (Vector3.One - rgbF) * brightness + rgbF;
                return new ColorF(new Vector4(rgbF, c.A));
#else
                c = c.Clip();
                return new ColorF(c.A,
                    (1f - c.R) * brightness + c.R,
                    (1f - c.G) * brightness + c.G,
                    (1f - c.B) * brightness + c.B);
#endif
            }

            static ColorF TransformDarkenF(ColorF c, float brightness)
            {
#if NETCOREAPP3_0_OR_GREATER
                // Using native vectorization if possible.
                if (Sse.IsSupported)
                    return new ColorF(Sse.Multiply(c.RgbaV128.ClipF(), Vector128.Create(brightness)).WithElement(3, c.A));
#endif

#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
                // The possibly still accelerated auto vectorization
                Vector3 rgbF = c.Rgb.ClipF() * brightness;
                return new ColorF(new Vector4(rgbF, c.A));
#else
                c = c.Clip();
                return new ColorF(c.A,
                    c.R * brightness,
                    c.G * brightness,
                    c.B * brightness);
#endif
            }

            #endregion

            Debug.Assert(channels != ColorChannels.None && brightness != 0f);
            bool darken = false;
            if (brightness < 0f)
            {
                brightness += 1f;
                darken = true;
            }

            // Determining ColorF usage by IsLinearGamma rather than Prefers128BitColors, because the transform function produces a color space dependent result.
            // When there is dithering, we must use Color32, regardless of the color space.
            AdjustDitherer(bitmapData, ref ditherer);
            bool linear = bitmapData.IsLinearGamma();
            if (ditherer == null)
            {
                if (linear)
                {
                    return DoTransformColors(context, bitmapData, darken
                        ? channels == ColorChannels.Rgb ? c => TransformDarkenF(c, brightness) : c => TransformDarkenPerChannelF(c, brightness, channels)
                        : channels == ColorChannels.Rgb ? c => TransformLightenF(c, brightness) : c => TransformLightenPerChannelF(c, brightness, channels));
                }

                if (bitmapData.PixelFormat.IsWide)
                {
                    return DoTransformColors(context, bitmapData, darken
                        ? channels == ColorChannels.Rgb ? c => TransformDarken64(c, brightness) : c => TransformDarkenPerChannel64(c, brightness, channels)
                        : channels == ColorChannels.Rgb ? c => TransformLighten64(c, brightness) : c => TransformLightenPerChannel64(c, brightness, channels));
                }
            }

            return DoTransformColors(context, bitmapData, darken
                    ? channels == ColorChannels.Rgb
                        ? linear ? c => TransformDarkenF(c.ToColorF(), brightness).ToColor32() : c => TransformDarken32(c, brightness)
                        : linear ? c => TransformDarkenPerChannelF(c.ToColorF(), brightness, channels).ToColor32() : c => TransformDarkenPerChannel32(c, brightness, channels)
                    : channels == ColorChannels.Rgb
                        ? linear ? c => TransformLightenF(c.ToColorF(), brightness).ToColor32() : c => TransformLighten32(c, brightness)
                        : linear ? c => TransformLightenPerChannelF(c.ToColorF(), brightness, channels).ToColor32() : c => TransformLightenPerChannel32(c, brightness, channels),
                ditherer);
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502: Avoid excessive complexity",
            Justification = "False alarm, the new analyzer includes the complexity of local methods")]
        private static bool DoAdjustContrast(IAsyncContext context, IReadWriteBitmapData bitmapData, float contrast, IDitherer? ditherer, ColorChannels channels)
        {
            #region Local Methods

            static Color32 TransformContrastPerChannel32(Color32 c, float contrast, ColorChannels channels) => new Color32(c.A,
                (channels & ColorChannels.R) == ColorChannels.R ? ((int)((((float)c.R / Byte.MaxValue - 0.5f) * contrast + 0.5f) * Byte.MaxValue)).ClipToByte() : c.R,
                (channels & ColorChannels.G) == ColorChannels.G ? ((int)((((float)c.G / Byte.MaxValue - 0.5f) * contrast + 0.5f) * Byte.MaxValue)).ClipToByte() : c.G,
                (channels & ColorChannels.B) == ColorChannels.B ? ((int)((((float)c.B / Byte.MaxValue - 0.5f) * contrast + 0.5f) * Byte.MaxValue)).ClipToByte() : c.B);

            static Color32 TransformContrast32(Color32 c, float contrast)
            {
#if NETCOREAPP3_0_OR_GREATER
                if (Sse2.IsSupported)
                {
                    Vector128<int> bgraI32 = Sse41.IsSupported
                        ? Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(c.Value).AsByte())
                        : Vector128.Create(c.B, c.G, c.R, default);

                    // bgrF = (float)(c.B, c.G, c.R, _)
                    Vector128<float> bgrF = Sse2.ConvertToVector128Single(bgraI32);

                    // bgrF = ((bgrF / 255f - 0.5f) * contrast + 0.5f) * 255f
                    bgrF = Fma.IsSupported
                        ? Sse.Multiply(Fma.MultiplyAdd(Sse.Subtract(Sse.Divide(bgrF, VectorExtensions.Max8BitF), VectorExtensions.HalfF), Vector128.Create(contrast), VectorExtensions.HalfF), VectorExtensions.Max8BitF)
                        : Sse.Multiply(Sse.Add(Sse.Multiply(Sse.Subtract(Sse.Divide(bgrF, VectorExtensions.Max8BitF), VectorExtensions.HalfF), Vector128.Create(contrast)), VectorExtensions.HalfF), VectorExtensions.Max8BitF);

                    // bgraI32 = (int)(bgrF).Clip(0, 255)
                    bgraI32 = Sse41.IsSupported
                        ? Sse2.ConvertToVector128Int32WithTruncation(bgrF).Clip(Vector128<int>.Zero, VectorExtensions.Max8BitI32)
                        : Sse2.ConvertToVector128Int32WithTruncation(bgrF.Clip(Vector128<float>.Zero, VectorExtensions.Max8BitF));

                    return Ssse3.IsSupported
                        ? new Color32(Ssse3.Shuffle(bgraI32.AsByte(), VectorExtensions.PackLowBytesMask).WithElement(3, c.A).AsUInt32().ToScalar())
                        : new Color32(c.A, bgraI32.AsByte().GetElement(8), bgraI32.AsByte().GetElement(4), bgraI32.AsByte().GetElement(0));
                }
#endif

                return new Color32(c.A,
                    ((int)((((float)c.R / Byte.MaxValue - 0.5f) * contrast + 0.5f) * Byte.MaxValue)).ClipToByte(),
                    ((int)((((float)c.G / Byte.MaxValue - 0.5f) * contrast + 0.5f) * Byte.MaxValue)).ClipToByte(),
                    ((int)((((float)c.B / Byte.MaxValue - 0.5f) * contrast + 0.5f) * Byte.MaxValue)).ClipToByte());
            }

            static Color64 TransformContrastPerChannel64(Color64 c, float contrast, ColorChannels channels) => new Color64(c.A,
                (channels & ColorChannels.R) == ColorChannels.R ? ((int)((((float)c.R / UInt16.MaxValue - 0.5f) * contrast + 0.5f) * UInt16.MaxValue)).ClipToUInt16() : c.R,
                (channels & ColorChannels.G) == ColorChannels.G ? ((int)((((float)c.G / UInt16.MaxValue - 0.5f) * contrast + 0.5f) * UInt16.MaxValue)).ClipToUInt16() : c.G,
                (channels & ColorChannels.B) == ColorChannels.B ? ((int)((((float)c.B / UInt16.MaxValue - 0.5f) * contrast + 0.5f) * UInt16.MaxValue)).ClipToUInt16() : c.B);

            static Color64 TransformContrast64(Color64 c, float contrast)
            {
#if NETCOREAPP3_0_OR_GREATER
                if (Sse41.IsSupported)
                {
                    // bgrF = (float)(c.B, c.G, c.R, _)
                    Vector128<float> bgrF = Sse2.ConvertToVector128Single(Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(c.Value).AsUInt16()));

                    // bgrF = ((bgrF / 65535f - 0.5f) * contrast + 0.5f) * 65535f
                    bgrF = Fma.IsSupported
                        ? Sse.Multiply(Fma.MultiplyAdd(Sse.Subtract(Sse.Divide(bgrF, VectorExtensions.Max16BitF), VectorExtensions.HalfF), Vector128.Create(contrast), VectorExtensions.HalfF), VectorExtensions.Max16BitF)
                        : Sse.Multiply(Sse.Add(Sse.Multiply(Sse.Subtract(Sse.Divide(bgrF, VectorExtensions.Max16BitF), VectorExtensions.HalfF), Vector128.Create(contrast)), VectorExtensions.HalfF), VectorExtensions.Max16BitF);

                    // bgraI32 = (int)(bgrF).Clip(0, 65535)
                    Vector128<int> bgraI32 = Sse2.ConvertToVector128Int32WithTruncation(bgrF).Clip(Vector128<int>.Zero, VectorExtensions.Max16BitI32);
                    return new Color64(Sse41.PackUnsignedSaturate(bgraI32, bgraI32).WithElement(3, c.A).AsUInt64().ToScalar());
                }
#endif

                return new Color64(c.A,
                    ((int)((((float)c.R / UInt16.MaxValue - 0.5f) * contrast + 0.5f) * UInt16.MaxValue)).ClipToUInt16(),
                    ((int)((((float)c.G / UInt16.MaxValue - 0.5f) * contrast + 0.5f) * UInt16.MaxValue)).ClipToUInt16(),
                    ((int)((((float)c.B / UInt16.MaxValue - 0.5f) * contrast + 0.5f) * UInt16.MaxValue)).ClipToUInt16());
            }

            static ColorF TransformContrastPerChannelF(ColorF c, float contrast, ColorChannels channels)
            {
                c = c.Clip();
                return new ColorF(c.A,
                    (channels & ColorChannels.R) == ColorChannels.R ? (c.R - 0.5f) * contrast + 0.5f : c.R,
                    (channels & ColorChannels.G) == ColorChannels.G ? (c.G - 0.5f) * contrast + 0.5f : c.G,
                    (channels & ColorChannels.B) == ColorChannels.B ? (c.B - 0.5f) * contrast + 0.5f : c.B);
            }

            static ColorF TransformContrastF(ColorF c, float contrast)
            {
#if NETCOREAPP3_0_OR_GREATER
                if (Sse.IsSupported)
                {
                    // rgbaF = (c - 0.5f) * contrast + 0.5f
                    Vector128<float> rgbaF = Fma.IsSupported
                        ? Fma.MultiplyAdd(Sse.Subtract(c.RgbaV128.ClipF(), VectorExtensions.HalfF), Vector128.Create(contrast), VectorExtensions.HalfF)
                        : Sse.Add(Sse.Multiply(Sse.Subtract(c.RgbaV128.ClipF(), VectorExtensions.HalfF), Vector128.Create(contrast)), VectorExtensions.HalfF);
                    return new ColorF(rgbaF.WithElement(3, c.A));
                }
#endif
                c = c.Clip();
                return new ColorF(c.A,
                    (c.R - 0.5f) * contrast + 0.5f,
                    (c.G - 0.5f) * contrast + 0.5f,
                    (c.B - 0.5f) * contrast + 0.5f);
            }

            #endregion

            contrast += 1f;
            contrast *= contrast;

            // Determining ColorF usage by IsLinearGamma rather than Prefers128BitColors, because the transform function produces a color space dependent result.
            // When there is dithering, we must use Color32, regardless of the color space.
            AdjustDitherer(bitmapData, ref ditherer);
            bool linear = bitmapData.IsLinearGamma();
            if (ditherer == null)
            {
                if (linear)
                    return DoTransformColors(context, bitmapData, channels == ColorChannels.Rgb ? c => TransformContrastF(c, contrast) : c => TransformContrastPerChannelF(c, contrast, channels));
                if (bitmapData.PixelFormat.IsWide)
                    return DoTransformColors(context, bitmapData, channels == ColorChannels.Rgb ? c => TransformContrast64(c, contrast) : c => TransformContrastPerChannel64(c, contrast, channels));
            }

            return DoTransformColors(context, bitmapData, channels == ColorChannels.Rgb
                    ? linear ? c => TransformContrastF(c.ToColorF(), contrast).ToColor32() : c => TransformContrast32(c, contrast)
                    : linear ? c => TransformContrastPerChannelF(c.ToColorF(), contrast, channels).ToColor32() : c => TransformContrastPerChannel32(c, contrast, channels),
                ditherer);
        }

        private static bool DoAdjustGamma(IAsyncContext context, IReadWriteBitmapData bitmapData, float gamma, IDitherer? ditherer, ColorChannels channels)
        {
            #region Local Methods

            static Color32 TransformGammaPerChannel32(Color32 c, ColorChannels channels, byte[] table) => new Color32(c.A,
                (channels & ColorChannels.R) == ColorChannels.R ? table[c.R] : c.R,
                (channels & ColorChannels.G) == ColorChannels.G ? table[c.G] : c.G,
                (channels & ColorChannels.B) == ColorChannels.B ? table[c.B] : c.B);

            static Color32 TransformGamma32(Color32 c, byte[] table) => new Color32(c.A, table[c.R], table[c.G], table[c.B]);

            static Color64 TransformGammaPerChannel64(Color64 c, ColorChannels channels, ushort[] table) => new Color64(c.A,
                (channels & ColorChannels.R) == ColorChannels.R ? table[c.R] : c.R,
                (channels & ColorChannels.G) == ColorChannels.G ? table[c.G] : c.G,
                (channels & ColorChannels.B) == ColorChannels.B ? table[c.B] : c.B);

            static Color64 TransformGamma64(Color64 c, ushort[] table) => new Color64(c.A, table[c.R], table[c.G], table[c.B]);

            static ColorF TransformGammaPerChannelF(ColorF c, ColorChannels channels, float gamma)
            {
                c = c.Clip();
                float invGamma = 1f / gamma;
                return new ColorF(c.A,
                    (channels & ColorChannels.R) == ColorChannels.R ? MathF.Pow(c.R, invGamma) : c.R,
                    (channels & ColorChannels.G) == ColorChannels.G ? MathF.Pow(c.G, invGamma) : c.G,
                    (channels & ColorChannels.B) == ColorChannels.B ? MathF.Pow(c.B, invGamma) : c.B);
            }

            static ColorF TransformGammaF(ColorF c, float gamma)
            {
#if NET9_0_OR_GREATER
                if (Vector128.IsHardwareAccelerated)
                    return new ColorF(c.RgbaV128.ClipF().Pow(1f / gamma).WithElement(3, c.A));
#endif

                c = c.Clip();
                float invGamma = 1f / gamma;
                return new ColorF(c.A,
                    MathF.Pow(c.R, invGamma),
                    MathF.Pow(c.G, invGamma),
                    MathF.Pow(c.B, invGamma));
            }

            #endregion

            // Determining ColorF usage by IsLinearGamma rather than Prefers128BitColors, because the transform function produces a color space dependent result.
            // When there is dithering, we must use Color32, regardless of the color space.
            AdjustDitherer(bitmapData, ref ditherer);
            bool linear = bitmapData.IsLinearGamma();
            if (ditherer == null)
            {
                if (linear)
                    return DoTransformColors(context, bitmapData, channels == ColorChannels.Rgb ? c => TransformGammaF(c, gamma) : c => TransformGammaPerChannelF(c, channels, gamma));
                if (bitmapData.PixelFormat.IsWide)
                {
                    ushort[] table64 = GammaLookupTableCache64[gamma];
                    return DoTransformColors(context, bitmapData, channels == ColorChannels.Rgb ? c => TransformGamma64(c, table64) : c => TransformGammaPerChannel64(c, channels, table64));
                }
            }

            if (linear)
                return DoTransformColors(context, bitmapData, channels == ColorChannels.Rgb ? c => TransformGammaF(c.ToColorF(), gamma).ToColor32() : c => TransformGammaPerChannelF(c.ToColorF(), channels, gamma).ToColor32(), ditherer);

            byte[] table32 = GammaLookupTableCache32[gamma];
            return DoTransformColors(context, bitmapData, channels == ColorChannels.Rgb ? c => TransformGamma32(c, table32) : c => TransformGammaPerChannel32(c, channels, table32), ditherer);
        }

        private static bool DoMakeOpaque(IAsyncContext context, IReadWriteBitmapData bitmapData, Color32 backColor, IDitherer? ditherer)
        {
            #region Local Methods
            
            static Color32 TransformMakeOpaque32(Color32 c, Color32 backColor) => c.A == Byte.MaxValue ? c : c.BlendWithBackgroundSrgb(backColor);
            static Color64 TransformMakeOpaque64(Color64 c, Color64 backColor) => c.A == UInt16.MaxValue ? c : c.BlendWithBackgroundSrgb(backColor);
            static ColorF TransformMakeOpaqueF(ColorF c, ColorF backColor) => c.A >= 1f ? c : c.BlendWithBackgroundLinear(backColor);

            #endregion

            // Determining ColorF usage by IsLinearGamma rather than Prefers128BitColors, because the transform function produces a color space dependent result.
            // When there is dithering, we must use Color32, regardless of the color space.
            AdjustDitherer(bitmapData, ref ditherer);
            bool linear = bitmapData.IsLinearGamma();
            backColor = backColor.ToOpaque();
            if (ditherer == null)
            {
                if (linear)
                {
                    ColorF backColorF = backColor.ToColorF();
                    return DoTransformColors(context, bitmapData, c => TransformMakeOpaqueF(c, backColorF));
                }

                if (bitmapData.PixelFormat.IsWide)
                {
                    Color64 backColor64 = backColor.ToColor64();
                    return DoTransformColors(context, bitmapData, c => TransformMakeOpaque64(c, backColor64));
                }
            }

            if (linear)
            {
                // We could use a TransformMakeOpaque32Linear method like in DoMakeGrayscale and call BlendWithBackgroundLinear for Color32 from there,
                // but that would convert backColor to ColorF for every pixel, which is less efficient.
                ColorF backColorF = backColor.ToColorF();
                return DoTransformColors(context, bitmapData, c => TransformMakeOpaqueF(c.ToColorF(), backColorF).ToColor32(), ditherer);
            }

            return DoTransformColors(context, bitmapData, c => TransformMakeOpaque32(c, backColor), ditherer);
        }

        private static bool DoMakeGrayscale(IAsyncContext context, IReadWriteBitmapData bitmapData, IDitherer? ditherer)
        {
            #region Local Methods
            
            static Color32 TransformMakeGrayscale32(Color32 c) => c.ToGray();
            static Color64 TransformMakeGrayscale64(Color64 c) => c.ToGray();
            static ColorF TransformMakeGrayscaleF(ColorF c) => c.ToGray();
            static Color32 TransformMakeGrayscale32Linear(Color32 c) => c.ToGray(WorkingColorSpace.Linear);

            #endregion

            // Determining ColorF usage by IsLinearGamma rather than Prefers128BitColors, because the transform function produces a color space dependent result.
            // When there is dithering, we must use Color32, regardless of the color space.
            AdjustDitherer(bitmapData, ref ditherer);
            bool linear = bitmapData.IsLinearGamma();
            if (ditherer == null)
            {
                if (linear)
                    return DoTransformColors(context, bitmapData, TransformMakeGrayscaleF);
                if (bitmapData.PixelFormat.IsWide)
                    return DoTransformColors(context, bitmapData, TransformMakeGrayscale64);
            }

            return DoTransformColors(context, bitmapData, linear ? TransformMakeGrayscale32Linear : TransformMakeGrayscale32, ditherer);
        }

        private static byte[] GenerateGammaLookupTable32(float gamma)
        {
            #region Local Methods

#if NET7_0_OR_GREATER
            [MethodImpl(MethodImpl.AggressiveInlining)]
            static void GenerateTableAutoVector128(float gamma, byte[] table)
            {
                Debug.Assert(Vector128.IsHardwareAccelerated);
                float power = 1f / gamma;
                var current = Vector128.Create(0f, 1f, 2f, 3f);
                for (int i = 0; i < 256; i += 4, current += Vector128.Create(4f))
                {
#if NET9_0_OR_GREATER
                    Vector128<float> resultF = Vector128.FusedMultiplyAdd((current / 255f).Pow(power), VectorExtensions.Max8BitF, VectorExtensions.HalfF);
#else
                    Vector128<float> resultF = (current * (1f / 255f)).Pow(power) * VectorExtensions.Max8BitF + VectorExtensions.HalfF;
#endif
                    Vector128<int> resultI32 = Vector128.Shuffle(Vector128.ConvertToInt32(resultF).AsByte(), VectorExtensions.PackLowBytesMask).AsInt32();
                    table[i].As<byte, int>() = resultI32.ToScalar();
                }
            }
#endif

#if NETCOREAPP3_0_OR_GREATER
            [MethodImpl(MethodImpl.AggressiveInlining)]
            static void GenerateTableVector128Sse2(float gamma, byte[] table)
            {
                Debug.Assert(Sse2.IsSupported);
                float power = 1f / gamma;
                var current = Vector128.Create(0f, 1f, 2f, 3f);
                for (int i = 0; i < 256; i += 16, current = Sse.Add(current, Vector128.Create(4f)))
                {
                    // We could spare adding +0.5f by using the slower ConvertToVector128Int32 that rounds the result instead of truncating it,
                    // but if FMA is supported, this is faster. Otherwise, the performance is about the same.
                    Vector128<float> resultF = Sse.Multiply(current, VectorExtensions.Max8BitRecipF).Pow(power);
                    resultF = Fma.IsSupported
                        ? Fma.MultiplyAdd(resultF, VectorExtensions.Max8BitF, VectorExtensions.HalfF)
                        : Sse.Add(Sse.Multiply(resultF, VectorExtensions.Max8BitF), VectorExtensions.HalfF);
                    Vector128<int> resultI32Left = Sse2.ConvertToVector128Int32WithTruncation(resultF);

                    current = Sse.Add(current, Vector128.Create(4f));
                    resultF = Sse.Multiply(current, VectorExtensions.Max8BitRecipF).Pow(power);
                    resultF = Fma.IsSupported
                        ? Fma.MultiplyAdd(resultF, VectorExtensions.Max8BitF, VectorExtensions.HalfF)
                        : Sse.Add(Sse.Multiply(resultF, VectorExtensions.Max8BitF), VectorExtensions.HalfF);
                    Vector128<int> resultI32Right = Sse2.ConvertToVector128Int32WithTruncation(resultF);
                    Vector128<short> resultI16Left = Sse2.PackSignedSaturate(resultI32Left, resultI32Right);

                    current = Sse.Add(current, Vector128.Create(4f));
                    resultF = Sse.Multiply(current, VectorExtensions.Max8BitRecipF).Pow(power);
                    resultF = Fma.IsSupported
                        ? Fma.MultiplyAdd(resultF, VectorExtensions.Max8BitF, VectorExtensions.HalfF)
                        : Sse.Add(Sse.Multiply(resultF, VectorExtensions.Max8BitF), VectorExtensions.HalfF);
                    resultI32Left = Sse2.ConvertToVector128Int32WithTruncation(resultF);

                    current = Sse.Add(current, Vector128.Create(4f));
                    resultF = Sse.Multiply(current, VectorExtensions.Max8BitRecipF).Pow(power);
                    resultF = Fma.IsSupported
                        ? Fma.MultiplyAdd(resultF, VectorExtensions.Max8BitF, VectorExtensions.HalfF)
                        : Sse.Add(Sse.Multiply(resultF, VectorExtensions.Max8BitF), VectorExtensions.HalfF);
                    resultI32Right = Sse2.ConvertToVector128Int32WithTruncation(resultF);
                    Vector128<short> resultI16Right = Sse2.PackSignedSaturate(resultI32Left, resultI32Right);

                    table[i].As<byte, Vector128<byte>>() = Sse2.PackUnsignedSaturate(resultI16Left, resultI16Right);
                }
            }
#endif

#if NET9_0_OR_GREATER
            [MethodImpl(MethodImpl.AggressiveInlining)]
            static void GenerateTableAutoVector256(float gamma, byte[] table)
            {
                Debug.Assert(Vector256.IsHardwareAccelerated);
                float power = 1f / gamma;
                var current = Vector256.Create(0f, 1f, 2f, 3f, 4f, 5f, 6f, 7f);
                for (int i = 0; i < 256; i += 8, current += Vector256.Create(8f))
                {
                    Vector256<float> resultF = Vector256.FusedMultiplyAdd((current / 255f).Pow(power), VectorExtensions.Max8Bit256F, VectorExtensions.Half256F);
                    Vector256<long> resultI64 = Vector256.Shuffle(Vector256.ConvertToInt32(resultF).AsByte(), VectorExtensions.PackLowBytes256Mask).AsInt64();
                    table[i].As<byte, long>() = resultI64.ToScalar();
                }
            }

            [MethodImpl(MethodImpl.AggressiveInlining)]
            static void GenerateTableVector256Avx2(float gamma, byte[] table)
            {
                Debug.Assert(Avx2.IsSupported);
                float power = 1f / gamma;
                var current = Vector256.Create(0f, 1f, 2f, 3f, 4f, 5f, 6f, 7f);
                for (int i = 0; i < 256; i += 32, current = Avx.Add(current, Vector256.Create(8f)))
                {
                    // We could spare adding +0.5f by using the slower ConvertToVector256Int32 that rounds the result instead of truncating it,
                    // but if FMA is supported, this is faster. Otherwise, the performance is about the same.
                    Vector256<float> resultF = Avx.Multiply(current, Vector256.Create(1f / 255f)).Pow(power);
                    resultF = Fma.IsSupported
                        ? Fma.MultiplyAdd(resultF, VectorExtensions.Max8Bit256F, VectorExtensions.Half256F)
                        : Avx.Add(Avx.Multiply(resultF, VectorExtensions.Max8Bit256F), VectorExtensions.Half256F);
                    Vector256<int> resultI32Left = Avx.ConvertToVector256Int32WithTruncation(resultF);

                    current = Avx.Add(current, Vector256.Create(8f));
                    resultF = Avx.Multiply(current, Vector256.Create(1f / 255f)).Pow(power);
                    resultF = Fma.IsSupported
                        ? Fma.MultiplyAdd(resultF, VectorExtensions.Max8Bit256F, VectorExtensions.Half256F)
                        : Avx.Add(Avx.Multiply(resultF, VectorExtensions.Max8Bit256F), VectorExtensions.Half256F);
                    Vector256<int> resultI32Right = Avx.ConvertToVector256Int32WithTruncation(resultF);
                    Vector256<short> resultI16Left = Avx2.PackSignedSaturate(resultI32Left, resultI32Right);

                    current = Avx.Add(current, Vector256.Create(8f));
                    resultF = Avx.Multiply(current, Vector256.Create(1f / 255f)).Pow(power);
                    resultF = Fma.IsSupported
                        ? Fma.MultiplyAdd(resultF, VectorExtensions.Max8Bit256F, VectorExtensions.Half256F)
                        : Avx.Add(Avx.Multiply(resultF, VectorExtensions.Max8Bit256F), VectorExtensions.Half256F);
                    resultI32Left = Avx.ConvertToVector256Int32WithTruncation(resultF);

                    current = Avx.Add(current, Vector256.Create(8f));
                    resultF = Avx.Multiply(current, Vector256.Create(1f / 255f)).Pow(power);
                    resultF = Fma.IsSupported
                        ? Fma.MultiplyAdd(resultF, VectorExtensions.Max8Bit256F, VectorExtensions.Half256F)
                        : Avx.Add(Avx.Multiply(resultF, VectorExtensions.Max8Bit256F), VectorExtensions.Half256F);
                    resultI32Right = Avx.ConvertToVector256Int32WithTruncation(resultF);
                    Vector256<short> resultI16Right = Avx2.PackSignedSaturate(resultI32Left, resultI32Right);

                    // NOTE: Unlike in case of SSE, the PackSignedSaturate methods in AVX interleave the results from left and right vectors, so the order in resultBytes will be as follows:
                    // 0, 1, 2, 3, 8, 9, 10, 11, 16, 17, 18, 19, 24, 25, 26, 27, 4, 5, 6, 7, 12, 13, 14, 15, 20, 21, 22, 23, 28, 29, 30, 31
                    // An apparently obvious solution would be to fix it by Avx2.Shuffle(resultBytes, Vector256.Create((byte)0, 1, 2, 3, 16, 17, 18, 19, 4, 5, 6, 7, 20, 21, 22, 23, 8, 9, 10, 11, 24, 25, 26, 27, 12, 13, 14, 15, 28, 29, 30, 31)),
                    // but it just messes up the result even more, as it does not work across 128-bit lanes. The real solution is to use PermuteVar8x32 as int, which is 3x slower, but works in the whole 256-bit range.
                    Vector256<byte> resultBytes = Avx2.PackUnsignedSaturate(resultI16Left, resultI16Right);
                    resultBytes = Avx2.PermuteVar8x32(resultBytes.AsInt32(), Vector256.Create(0, 4, 1, 5, 2, 6, 3, 7)).AsByte();
                    table[i].As<byte, Vector256<byte>>() = resultBytes;
                }
            }

            [MethodImpl(MethodImpl.AggressiveInlining)]
            static void GenerateTableAutoVector512(float gamma, byte[] table)
            {
                Debug.Assert(Vector512.IsHardwareAccelerated);
                float power = 1f / gamma;
                var current = Vector512.Create(0f, 1f, 2f, 3f, 4f, 5f, 6f, 7f, 8f, 9f, 10f, 11f, 12f, 13f, 14f, 15f);
                for (int i = 0; i < 256; i += 16, current += Vector512.Create(16f))
                {
                    Vector512<float> resultF = Vector512.FusedMultiplyAdd((current / 255f).Pow(power), Vector512.Create(255f), Vector512.Create(0.5f));
                    Vector512<byte> resultBytes = Vector512.Shuffle(Vector512.ConvertToInt32(resultF).AsByte(), VectorExtensions.PackLowBytes512Mask);
                    table[i].As<byte, Vector128<byte>>() = resultBytes.GetLower().GetLower();
                }
            }
#endif

            #endregion

            byte[] result = new byte[256];
            if (gamma.TolerantIsZero())
            {
                result[255] = 255;
                return result;
            }

            // Vector256/512: Using only in .NET9+, because Pow is not vectorized below that, causing that even the fallback version is faster than Vector256/512 in .NET 8 and earlier.
            // But the Vector128 version is faster than the vanilla version even in .NET8-, maybe because of the double packing. See also GenerateGammaLookupTable32Test in PerformanceTests.
#if NET9_0_OR_GREATER
            if (Vector512.IsHardwareAccelerated)
            {
                GenerateTableAutoVector512(gamma, result);
                return result;
            }

            if (Avx2.IsSupported)
            {
                GenerateTableVector256Avx2(gamma, result);
                return result;
            }

            if (Vector256.IsHardwareAccelerated)
            {
                GenerateTableAutoVector256(gamma, result);
                return result;
            }
#endif
#if NETCOREAPP3_0_OR_GREATER
            if (Sse2.IsSupported)
            {
                GenerateTableVector128Sse2(gamma, result);
                return result;
            }
#endif
#if NET7_0_OR_GREATER
            if (Vector128.IsHardwareAccelerated)
            {
                GenerateTableAutoVector128(gamma, result);
                return result;
            }
#endif

            // The fallback implementation
            float power = 1f / gamma;
            for (int i = 0; i < 256; i++)
                result[i] = (byte)(255f * MathF.Pow(i / 255f, power) + 0.5f);
            return result;
        }

        private static ushort[] GenerateGammaLookupTable64(float gamma)
        {
            #region Local Methods

#if NET7_0_OR_GREATER
            [MethodImpl(MethodImpl.AggressiveInlining)]
            static void GenerateTableAutoVector128(float gamma, ushort[] table)
            {
                Debug.Assert(Vector128.IsHardwareAccelerated);
                float power = 1f / gamma;
                var current = Vector128.Create(0f, 1f, 2f, 3f);
                for (int i = 0; i < 65536; i += 4, current += Vector128.Create(4f))
                {
#if NET9_0_OR_GREATER
                    Vector128<float> resultF = Vector128.FusedMultiplyAdd((current / 65535f).Pow(power), VectorExtensions.Max16BitF, VectorExtensions.HalfF);
#else
                    Vector128<float> resultF = (current * (1f / 65535f)).Pow(power) * VectorExtensions.Max16BitF + VectorExtensions.HalfF;
#endif
                    Vector128<ulong> resultU64 = Vector128.Shuffle(Vector128.ConvertToInt32(resultF).AsByte(), VectorExtensions.PackLowWordsMask).AsUInt64();
                    table[i].As<ushort, ulong>() = resultU64.ToScalar();
                }
            }
#endif

#if NETCOREAPP3_0_OR_GREATER
            [MethodImpl(MethodImpl.AggressiveInlining)]
            static void GenerateTableVector128Sse2(float gamma, ushort[] table)
            {
                Debug.Assert(Sse2.IsSupported);
                float power = 1f / gamma;
                var current = Vector128.Create(0f, 1f, 2f, 3f);
                for (int i = 0; i < 65536; i += 8, current = Sse.Add(current, Vector128.Create(4f)))
                {
                    // We could spare adding +0.5f by using the slower ConvertToVector128Int32 that rounds the result instead of truncating it,
                    // but if FMA is supported, this is faster. Otherwise, the performance is about the same.
                    Vector128<float> resultF = Sse.Multiply(current, Vector128.Create(1f / 65535f)).Pow(power);
                    resultF = Fma.IsSupported
                        ? Fma.MultiplyAdd(resultF, VectorExtensions.Max16BitF, VectorExtensions.HalfF)
                        : Sse.Add(Sse.Multiply(resultF, VectorExtensions.Max16BitF), VectorExtensions.HalfF);
                    Vector128<int> resultI32Left = Sse2.ConvertToVector128Int32WithTruncation(resultF);

                    current = Sse.Add(current, Vector128.Create(4f));
                    resultF = Sse.Multiply(current, Vector128.Create(1f / 65535f)).Pow(power);
                    resultF = Fma.IsSupported
                        ? Fma.MultiplyAdd(resultF, VectorExtensions.Max16BitF, VectorExtensions.HalfF)
                        : Sse.Add(Sse.Multiply(resultF, VectorExtensions.Max16BitF), VectorExtensions.HalfF);
                    Vector128<int> resultI32Right = Sse2.ConvertToVector128Int32WithTruncation(resultF);

                    table[i].As<ushort, Vector128<ushort>>() = Sse41.PackUnsignedSaturate(resultI32Left, resultI32Right);
                }
            }
#endif

#if NET9_0_OR_GREATER
            [MethodImpl(MethodImpl.AggressiveInlining)]
            static void GenerateTableAutoVector256(float gamma, ushort[] table)
            {
                Debug.Assert(Vector256.IsHardwareAccelerated);
                float power = 1f / gamma;
                var current = Vector256.Create(0f, 1f, 2f, 3f, 4f, 5f, 6f, 7f);
                for (int i = 0; i < 65536; i += 8, current += Vector256.Create(8f))
                {
                    Vector256<float> resultF = Vector256.FusedMultiplyAdd((current / 65535f).Pow(power), VectorExtensions.Max16Bit256F, VectorExtensions.Half256F);
                    Vector256<ushort> resultU16 = Vector256.Shuffle(Vector256.ConvertToInt32(resultF).AsUInt16(), VectorExtensions.PackLowWords256Mask);
                    table[i].As<ushort, Vector128<ushort>>() = resultU16.GetLower();
                }
            }

            [MethodImpl(MethodImpl.AggressiveInlining)]
            static void GenerateTableVector256Avx2(float gamma, ushort[] table)
            {
                Debug.Assert(Avx2.IsSupported);
                float power = 1f / gamma;
                var current = Vector256.Create(0f, 1f, 2f, 3f, 4f, 5f, 6f, 7f);
                for (int i = 0; i < 65536; i += 16, current = Avx.Add(current, Vector256.Create(8f)))
                {
                    // We could spare adding +0.5f by using the slower ConvertToVector256Int32 that rounds the result instead of truncating it,
                    // but if FMA is supported, this is faster. Otherwise, the performance is about the same.
                    Vector256<float> resultF = Avx.Multiply(current, Vector256.Create(1f / 65535f)).Pow(power);
                    resultF = Fma.IsSupported
                        ? Fma.MultiplyAdd(resultF, VectorExtensions.Max16Bit256F, VectorExtensions.Half256F)
                        : Avx.Add(Avx.Multiply(resultF, VectorExtensions.Max16Bit256F), VectorExtensions.Half256F);
                    Vector256<int> resultI32Left = Avx.ConvertToVector256Int32WithTruncation(resultF);

                    current = Avx.Add(current, Vector256.Create(8f));
                    resultF = Avx.Multiply(current, Vector256.Create(1f / 65535f)).Pow(power);
                    resultF = Fma.IsSupported
                        ? Fma.MultiplyAdd(resultF, VectorExtensions.Max16Bit256F, VectorExtensions.Half256F)
                        : Avx.Add(Avx.Multiply(resultF, VectorExtensions.Max16Bit256F), VectorExtensions.Half256F);
                    Vector256<int> resultI32Right = Avx.ConvertToVector256Int32WithTruncation(resultF);

                    // NOTE: Unlike in case of SSE, the PackSignedSaturate methods in AVX interleave the results from left and right vectors, so the order in resultWords will be as follows:
                    // 0, 1, 2, 3, 8, 9, 10, 11, 4, 5, 6, 7, 12, 13, 14, 15
                    // To fix this, we have to use PermuteVar8x32.
                    Vector256<ushort> resultWords = Avx2.PackUnsignedSaturate(resultI32Left, resultI32Right);
                    resultWords = Avx2.PermuteVar8x32(resultWords.AsInt32(), Vector256.Create(0, 1, 4, 5, 2, 3, 6, 7)).AsUInt16();
                    table[i].As<ushort, Vector256<ushort>>() = resultWords;
                }
            }

            [MethodImpl(MethodImpl.AggressiveInlining)]
            static void GenerateTableAutoVector512(float gamma, ushort[] table)
            {
                Debug.Assert(Vector512.IsHardwareAccelerated);
                float power = 1f / gamma;
                var current = Vector512.Create(0f, 1f, 2f, 3f, 4f, 5f, 6f, 7f, 8f, 9f, 10f, 11f, 12f, 13f, 14f, 15f);
                for (int i = 0; i < 65536; i += 16, current += Vector512.Create(16f))
                {
                    Vector512<float> resultF = Vector512.FusedMultiplyAdd((current / 65535f).Pow(power), Vector512.Create(65535f), Vector512.Create(0.5f));
                    Vector512<ushort> resultWords = Vector512.Shuffle(Vector512.ConvertToInt32(resultF).AsUInt16(), VectorExtensions.PackLowWords512Mask);
                    table[i].As<ushort, Vector256<ushort>>() = resultWords.GetLower();
                }
            }
#endif

            #endregion

            ushort[] result = new ushort[65536];
            if (gamma.TolerantIsZero())
            {
                result[65535] = 65535;
                return result;
            }

            // Vector256/512: Using only in .NET9+, because Pow is not vectorized below that, causing that even the fallback version is faster than Vector256/512 in .NET 8 and earlier.
            // But the Vector128 version is faster than the vanilla version even in .NET8-, maybe because of the double packing. See also GenerateGammaLookupTable64Test in PerformanceTests.
#if NET9_0_OR_GREATER
            if (Vector512.IsHardwareAccelerated)
            {
                GenerateTableAutoVector512(gamma, result);
                return result;
            }

            if (Avx2.IsSupported)
            {
                GenerateTableVector256Avx2(gamma, result);
                return result;
            }

            if (Vector256.IsHardwareAccelerated)
            {
                GenerateTableAutoVector256(gamma, result);
                return result;
            }
#endif
#if NETCOREAPP3_0_OR_GREATER
            if (Sse2.IsSupported)
            {
                GenerateTableVector128Sse2(gamma, result);
                return result;
            }
#endif
#if NET7_0_OR_GREATER
            if (Vector128.IsHardwareAccelerated)
            {
                GenerateTableAutoVector128(gamma, result);
                return result;
            }
#endif

            float power = 1f / gamma;
            for (int i = 0; i < 65536; i++)
                result[i] = (ushort)(65535f * MathF.Pow(i / 65535f, power) + 0.5f);
            return result;
        }

        #endregion

        #endregion

        #endregion
    }
}
