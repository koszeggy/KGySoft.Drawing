#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BitmapDataExtensions.Writable.cs
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
#if NETCOREAPP3_0_OR_GREATER
using System.Runtime.CompilerServices;
#endif
#if !NET35
using System.Threading.Tasks;
#endif

using KGySoft.Threading;

#endregion

#if NET35
#pragma warning disable CS1574 // XML comment has cref attribute that could not be resolved - in .NET 3.5 not all members are available
#endif

namespace KGySoft.Drawing.Imaging
{
    partial class BitmapDataExtensions
    {
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
        /// <returns>An <see cref="IWritableBitmapData"/> that provides access only to the specified region withing the <paramref name="source"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="clippingRegion"/> has no overlapping region with source bounds.</exception>
        /// <remarks>
        /// <para>The <see cref="IBitmapData.RowSize"/> property of the returned instance can be 0, indicating that the <see cref="IWritableBitmapDataRow.WriteRaw{T}">WriteRaw</see>
        /// method cannot be used. It can occur if the left edge of the clipping is not zero.</para>
        /// <para>Even if <see cref="IBitmapData.RowSize"/> property of the returned instance is a nonzero value it can happen that it is too low to access all columns
        /// by the <see cref="IWritableBitmapDataRow.WriteRaw{T}">WriteRaw</see> method. It can occur with indexed <see cref="IBitmapData.PixelFormat"/>s if the right edge of the clipping is not on byte boundary.</para>
        /// </remarks>
        public static IWritableBitmapData Clip(this IWritableBitmapData source, Rectangle clippingRegion, bool disposeSource)
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
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="Clip(IWritableBitmapData,Rectangle,bool)"/> overload for details.
        /// </summary>
        /// <param name="source">The source bitmap data to be clipped.</param>
        /// <param name="clippingRegion">A <see cref="Rectangle"/> that specifies a region within the <paramref name="source"/>.</param>
        /// <returns>An <see cref="IWritableBitmapData"/> that provides access only to the specified region withing the <paramref name="source"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="clippingRegion"/> has no overlapping region with source bounds.</exception>
        public static IWritableBitmapData Clip(this IWritableBitmapData source, Rectangle clippingRegion)
            => Clip(source, clippingRegion, false);

        #endregion

        #region Clear

        /// <summary>
        /// Clears the content of the specified <paramref name="bitmapData"/> and fills it with the specified <paramref name="color"/>.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IWritableBitmapData"/> to be cleared.</param>
        /// <param name="color">A <see cref="Color32"/> that represents the desired result color of the <paramref name="bitmapData"/>.
        /// If it has transparency, which is not supported by <see cref="IBitmapData.PixelFormat"/> of <paramref name="bitmapData"/>, then the result might be either
        /// completely transparent (depends also on <see cref="IBitmapData.AlphaThreshold"/>), or the color will be blended with <see cref="IBitmapData.BackColor"/>.
        /// </param>
        /// <param name="ditherer">The ditherer to be used for the clearing. Has no effect if <see cref="IBitmapData.PixelFormat"/> of <paramref name="bitmapData"/> has at least 24 bits-per-pixel size. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. You can use
        /// the <see cref="Clear(IWritableBitmapData, Color32, IDitherer, ParallelConfig)"/> overload to configure these, while still executing the method synchronously. Alternatively, use
        /// the <see cref="BeginClear">BeginClear</see> or <see cref="ClearAsync">ClearAsync</see> (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// </remarks>
        public static void Clear(this IWritableBitmapData bitmapData, Color32 color, IDitherer? ditherer = null)
            => bitmapData.Clear(AsyncHelper.DefaultContext, color, ditherer);

        /// <summary>
        /// Clears the content of the specified <paramref name="bitmapData"/> and fills it with the specified <paramref name="color"/>.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IWritableBitmapData"/> to be cleared.</param>
        /// <param name="color">A <see cref="Color32"/> that represents the desired result color of the <paramref name="bitmapData"/>.
        /// If it has transparency, which is not supported by <see cref="IBitmapData.PixelFormat"/> of <paramref name="bitmapData"/>, then the result might be either
        /// completely transparent (depends also on <see cref="IBitmapData.AlphaThreshold"/>), or the color will be blended with <see cref="IBitmapData.BackColor"/>.
        /// </param>
        /// <param name="ditherer">The ditherer to be used for the clearing. Has no effect if <see cref="IBitmapData.PixelFormat"/> of <paramref name="bitmapData"/> has at least 24 bits-per-pixel size.</param>
        /// <param name="parallelConfig">The configuration of the operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.
        /// If <see langword="null"/>, then the degree of parallelization is configured automatically.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// of the <paramref name="parallelConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <note>This method blocks the caller as it executes synchronously, though the <paramref name="parallelConfig"/> parameter allows you to configure the degree of parallelism,
        /// cancellation and progress reporting. Use the <see cref="BeginClear">BeginClear</see> or <see cref="ClearAsync">ClearAsync</see>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// </remarks>
        public static bool Clear(this IWritableBitmapData bitmapData, Color32 color, IDitherer? ditherer, ParallelConfig? parallelConfig)
        {
            // NOTE: The parallelConfig parameter could just be an additional optional parameter in the original overload but that would have been a breaking change.
            // This overload has no default parameters to prevent auto switching the callers to this one instead of the original one.
            // Even though it would be compile-compatible, it's still breaking. Also, this one has a bool return value, and there is a minimal overhead with the DoOperationSynchronously call.
            ValidateArguments(bitmapData);
            return AsyncHelper.DoOperationSynchronously(ctx => DoClear(ctx, bitmapData, color, ditherer), parallelConfig);
        }

        /// <summary>
        /// Clears the content of the specified <paramref name="bitmapData"/> and fills it with the specified <paramref name="color"/>,
        /// using a <paramref name="context"/> that may belong to a higher level, possibly asynchronous operation.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IWritableBitmapData"/> to be cleared.</param>
        /// <param name="context">An <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncContext.htm">IAsyncContext</a> instance
        /// that contains information for asynchronous processing about the current operation.</param>
        /// <param name="color">A <see cref="Color32"/> that represents the desired result color of the <paramref name="bitmapData"/>.
        /// If it has transparency, which is not supported by <see cref="IBitmapData.PixelFormat"/> of <paramref name="bitmapData"/>, then the result might be either
        /// completely transparent (depends also on <see cref="IBitmapData.AlphaThreshold"/>), or the color will be blended with <see cref="IBitmapData.BackColor"/>.
        /// </param>
        /// <param name="ditherer">The ditherer to be used for the clearing. Has no effect if <see cref="IBitmapData.PixelFormat"/> of <paramref name="bitmapData"/> has at least 24 bits-per-pixel size. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled.</returns>
        /// <remarks>
        /// <para>This method blocks the caller thread but if <paramref name="context"/> belongs to an async top level method, then the execution may already run
        /// on a pool thread. Degree of parallelism, the ability of cancellation and reporting progress depend on how these were configured at the top level method.</para>
        /// <para>When reporting progress, this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.</para>
        /// <note type="tip">See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_AsyncHelper.htm">AsyncHelper</a>
        /// class for details about how to create a context for possibly async top level methods.</note>
        /// </remarks>
        public static bool Clear(this IWritableBitmapData bitmapData, IAsyncContext? context, Color32 color, IDitherer? ditherer = null)
        {
            ValidateArguments(bitmapData);
            return DoClear(context ?? AsyncHelper.DefaultContext, bitmapData, color, ditherer);
        }

        /// <summary>
        /// Begins to clear the content of the specified <paramref name="bitmapData"/> and fills it with the specified <paramref name="color"/> asynchronously.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IWritableBitmapData"/> to be cleared.</param>
        /// <param name="color">A <see cref="Color32"/> that represents the desired result color of the <paramref name="bitmapData"/>.
        /// If it has transparency, which is not supported by <see cref="IBitmapData.PixelFormat"/> of <paramref name="bitmapData"/>, then the result might be either
        /// completely transparent (depends also on <see cref="IBitmapData.AlphaThreshold"/>), or the color will be blended with <see cref="IBitmapData.BackColor"/>.
        /// </param>
        /// <param name="ditherer">The ditherer to be used for the clearing. Has no effect if <see cref="IBitmapData.PixelFormat"/> of <paramref name="bitmapData"/> has at least 24 bits-per-pixel size. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="ClearAsync">ClearAsync</see> method.</para>
        /// <para>To finish the operation and to get the exception that occurred during the operation you have to call the <see cref="EndClear">EndClear</see> method.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        public static IAsyncResult BeginClear(this IWritableBitmapData bitmapData, Color32 color, IDitherer? ditherer = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData);
            return AsyncHelper.BeginOperation(ctx => DoClear(ctx, bitmapData, color, ditherer), asyncConfig);
        }

        /// <summary>
        /// Waits for the pending asynchronous operation started by the <see cref="BeginClear">BeginClear</see> method to complete.
        /// In .NET Framework 4.0 and above you can use the <see cref="ClearAsync">ClearAsync</see> method instead.
        /// </summary>
        /// <param name="asyncResult">The reference to the pending asynchronous request to finish.</param>
        public static void EndClear(this IAsyncResult asyncResult)
            // NOTE: the return value could be bool, but it would be a breaking change
            => AsyncHelper.EndOperation(asyncResult, nameof(BeginClear));

#if !NET35
        /// <summary>
        /// Begins to clear the content of the specified <paramref name="bitmapData"/> and fills it with the specified <paramref name="color"/> asynchronously.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IWritableBitmapData"/> to be cleared.</param>
        /// <param name="color">A <see cref="Color32"/> that represents the desired result color of the <paramref name="bitmapData"/>.
        /// If it has transparency, which is not supported by <see cref="IBitmapData.PixelFormat"/> of <paramref name="bitmapData"/>, then the result might be either
        /// completely transparent (depends also on <see cref="IBitmapData.AlphaThreshold"/>), or the color will be blended with <see cref="IBitmapData.BackColor"/>.
        /// </param>
        /// <param name="ditherer">The ditherer to be used for the clearing. Has no effect if <see cref="IBitmapData.PixelFormat"/> of <paramref name="bitmapData"/> has at least 24 bits-per-pixel size. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        public static Task ClearAsync(this IWritableBitmapData bitmapData, Color32 color, IDitherer? ditherer = null, TaskConfig? asyncConfig = null)
        {
            // NOTE: the return value could be Task<bool> but it would be a breaking change
            ValidateArguments(bitmapData);
            return AsyncHelper.DoOperationAsync(ctx => DoClear(ctx, bitmapData, color, ditherer), asyncConfig);
        }
#endif

        #endregion

        #region TrySetPalette

        /// <summary>
        /// Tries to the set the specified <paramref name="palette"/> for this <see cref="IWritableBitmapData"/>.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IWritableBitmapData"/> whose <see cref="IBitmapData.Palette"/> should be set.</param>
        /// <param name="palette">A <see cref="Palette"/> instance to set.</param>
        /// <returns><see langword="true"/>&#160;<paramref name="palette"/> can be set as the <see cref="IBitmapData.Palette"/> of this <paramref name="bitmapData"/>; otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>Setting may fail if <paramref name="bitmapData"/>&#160;<see cref="IBitmapData.PixelFormat"/> is not an indexed one,
        /// the number of entries in <paramref name="palette"/> is less than <see cref="Palette.Count"/> of the current <see cref="IBitmapData.Palette"/>,
        /// the number of entries in <paramref name="palette"/> is larger than the possible maximum number of colors of the current <see cref="IBitmapData.PixelFormat"/>,
        /// or when the current <see cref="IWritableBitmapData"/> does not support setting the palette.</para>
        /// <para>The <see cref="Palette.BackColor">Palette.BackColor</see> and <see cref="Palette.AlphaThreshold">Palette.AlphaThreshold</see> properties of the <see cref="IBitmapData.Palette"/> property will
        /// continue to return the same value as the original <see cref="IBitmapData.BackColor"/> and <see cref="IBitmapData.AlphaThreshold"/> values of this <paramref name="bitmapData"/>.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        public static bool TrySetPalette(this IWritableBitmapData bitmapData, Palette palette)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData));
            if (palette == null)
                throw new ArgumentNullException(nameof(palette));
            return bitmapData is IBitmapDataInternal internalBitmapData && internalBitmapData.TrySetPalette(palette);
        }

        #endregion

        #endregion

        #region Internal Methods

        internal static void DoClear(this IWritableBitmapData bitmapData, IAsyncContext asyncContext, Color32 color)
            => DoClear(asyncContext, bitmapData, color, null);

        #endregion

        #region Private Methods

        private static void ValidateArguments(IWritableBitmapData bitmapData)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
        }

        private static bool DoClear(IAsyncContext context, IWritableBitmapData bitmapData, Color32 color, IDitherer? ditherer)
        {
            IBitmapDataInternal accessor = bitmapData as IBitmapDataInternal ?? new BitmapDataWrapper(bitmapData, false, true);
            try
            {
                if (ditherer == null || !accessor.PixelFormat.CanBeDithered)
                    ClearDirect(context, accessor, color);
                else
                    ClearWithDithering(context, accessor, color, ditherer);
                return !context.IsCancellationRequested;
            }
            finally
            {
                if (!ReferenceEquals(accessor, bitmapData))
                    accessor.Dispose();
            }
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502: Avoid excessive complexity",
            Justification = "False alarm, the new analyzer includes the complexity of local methods")] 
        private static void ClearDirect(IAsyncContext context, IBitmapDataInternal bitmapData, Color32 color)
        {
            #region Local Methods to Reduce Complexity

            static void Clear64Bpp(IAsyncContext context, IBitmapDataInternal bitmapData, Color32 color, int width)
            {
                Debug.Assert(bitmapData.PixelFormat.IsKnownFormat);
                int longWidth = bitmapData.RowSize >> 3;

                // writing as longs
                if (longWidth > 0)
                {
                    ulong value = bitmapData.PixelFormat.AsKnownPixelFormatInternal is KnownPixelFormat.Format64bppArgb
                        ? new Color64(color).Value
                        : new PColor64(color).Value;
                    ClearRaw(context, bitmapData, longWidth, value);
                }

                // handling the rest (can be even the whole content if RowSize is 0)
                int left = longWidth;
                if (left < width && !context.IsCancellationRequested)
                    ClearDirectFallback(context, bitmapData, color, left);
            }

            static void Clear32Bpp(IAsyncContext context, IBitmapDataInternal bitmapData, Color32 color, int width)
            {
                Debug.Assert(bitmapData.PixelFormat.IsKnownFormat);
                int longWidth = bitmapData.RowSize >> 3;

                // writing as longs
                if (longWidth > 0)
                {
                    uint argb;
                    switch (bitmapData.PixelFormat.AsKnownPixelFormatInternal)
                    {
                        case KnownPixelFormat.Format32bppPArgb:
                            argb = color.ToPremultiplied().Value;
                            break;
                        case KnownPixelFormat.Format32bppRgb:
                            argb = (color.A == Byte.MaxValue ? color : color.BlendWithBackground(bitmapData.BackColor, bitmapData.LinearBlending())).Value;
                            break;
                        case KnownPixelFormat.Format32bppGrayScale:
                            float f = (bitmapData.LinearBlending()
                                ? new GrayF(color.A == Byte.MaxValue ? color.ToColorF() : color.ToColorF().BlendWithBackgroundLinear(bitmapData.BackColor.ToColorF()))
                                : new GrayF(color.A == Byte.MaxValue ? color.ToColor64() : color.ToColor64().BlendWithBackgroundSrgb(bitmapData.BackColor.ToColor64()))).Value;
#if NETCOREAPP3_0_OR_GREATER
                            argb = Unsafe.As<float, uint>(ref f);
#else
                            unsafe { argb = *(uint*)&f; }
#endif
                            break;

                        default:
                            argb = color.Value;
                            break;
                    }

                    ClearRaw(context, bitmapData, longWidth, ((ulong)argb << 32) | argb);
                }

                // handling the rest (can be either the last column if width is odd, or even the whole content if RowSize is 0)
                int left = longWidth << 1;
                if (left < width && !context.IsCancellationRequested)
                    ClearDirectFallback(context, bitmapData, color, left);
            }

            static void Clear16Bpp(IAsyncContext context, IBitmapDataInternal bitmapData, Color32 color, int width)
            {
                Debug.Assert(bitmapData.PixelFormat.IsKnownFormat);
                int longWidth = bitmapData.RowSize >> 3;

                // writing as longs
                if (longWidth > 0)
                {
                    ushort shortValue = bitmapData.PixelFormat.AsKnownPixelFormatInternal switch
                    {
                        KnownPixelFormat.Format16bppArgb1555 => new Color16Argb1555(
                            color.A == Byte.MaxValue ? color
                            : color.A >= bitmapData.AlphaThreshold ? color.BlendWithBackground(bitmapData.BackColor, bitmapData.WorkingColorSpace)
                            : default).Value,
                        KnownPixelFormat.Format16bppRgb565 => new Color16Rgb565(color.A == Byte.MaxValue ? color : color.BlendWithBackground(bitmapData.BackColor, bitmapData.WorkingColorSpace)).Value,
                        KnownPixelFormat.Format16bppRgb555 => new Color16Rgb555(color.A == Byte.MaxValue ? color : color.BlendWithBackground(bitmapData.BackColor, bitmapData.WorkingColorSpace)).Value,
                        _ => (bitmapData.LinearBlending()
                            ? new Gray16(color.A == Byte.MaxValue ? color.ToColorF() : color.ToColorF().BlendWithBackgroundLinear(bitmapData.BackColor.ToColorF()))
                            : new Gray16(color.A == Byte.MaxValue ? color.ToColor64() : color.ToColor64().BlendWithBackgroundSrgb(bitmapData.BackColor.ToColor64()))).Value
                    };

                    uint uintValue = (uint)((shortValue << 16) | shortValue);
                    ClearRaw(context, bitmapData, longWidth, ((ulong)uintValue << 32) | uintValue);
                }

                // handling the rest (or even the whole content if RowSize is 0)
                int left = longWidth << 2;
                if (left < width && !context.IsCancellationRequested)
                    ClearDirectFallback(context, bitmapData, color, left);
            }

            static void Clear8Bpp(IAsyncContext context, IBitmapDataInternal bitmapData, Color32 color, int width)
            {
                Debug.Assert(bitmapData.PixelFormat.AsKnownPixelFormatInternal == KnownPixelFormat.Format8bppGrayScale);
                int longWidth = bitmapData.RowSize >> 3;

                // writing as longs
                if (longWidth > 0)
                {
                    byte byteValue = (bitmapData.LinearBlending()
                        ? new Gray8(color.A == Byte.MaxValue ? color.ToColorF() : color.ToColorF().BlendWithBackgroundLinear(bitmapData.BackColor.ToColorF()))
                        : new Gray8(color.A == Byte.MaxValue ? color : color.BlendWithBackgroundSrgb(bitmapData.BackColor))).Value;

                    ushort shortValue = (ushort)((byteValue << 8) | byteValue);
                    uint uintValue = (uint)((shortValue << 16) | shortValue);
                    ClearRaw(context, bitmapData, longWidth, ((ulong)uintValue << 32) | uintValue);
                }

                // handling the rest (or even the whole content if RowSize is 0)
                int left = longWidth << 2;
                if (left < width && !context.IsCancellationRequested)
                    ClearDirectFallback(context, bitmapData, color, left);
            }

            static void ClearIndexed(IAsyncContext context, IBitmapDataInternal bitmapData, int bpp, Color32 color, int width)
            {
                int index = bitmapData.Palette?.GetNearestColorIndex(color) ?? 0;
                byte byteValue = bpp == 8 ? (byte)index
                    : bpp == 4 ? (byte)((index << 4) | index)
                    : index == 1 ? Byte.MaxValue : Byte.MinValue;
                int left = 0;

                if (bitmapData.RowSize > 0)
                {
                    int factor = bpp == 8 ? 0
                        : bpp == 4 ? 1
                        : 3;

                    // writing as longs
                    if ((bitmapData.RowSize & 0b111) == 0)
                    {
                        int longWidth = bitmapData.RowSize >> 3;
                        uint intValue = (uint)((byteValue << 24) | (byteValue << 16) | (byteValue << 8) | byteValue);
                        ClearRaw(context, bitmapData, longWidth, ((ulong)intValue << 32) | intValue);
                        left = (longWidth << 3) << factor;
                    }
                    // writing as integers
                    else if ((bitmapData.RowSize & 0b11) == 0)
                    {
                        int intWidth = bitmapData.RowSize >> 2;
                        ClearRaw(context, bitmapData, intWidth, (byteValue << 24) | (byteValue << 16) | (byteValue << 8) | byteValue);
                        left = (intWidth << 2) << factor;
                    }
                    // writing as bytes
                    else
                    {
                        ClearRaw(context, bitmapData, bitmapData.RowSize, byteValue);
                        left = bitmapData.RowSize << factor;
                    }
                }

                if (left >= width || context.IsCancellationRequested)
                    return;

                // handling the rest if needed (occurs if RowSize is 0 or right edge does not fall on byte boundary, eg. clipped bitmap data)
                // we could simply jump to default here but as we already know the palette index we can optimize it a bit
                if (width - left < parallelThreshold)
                {
                    context.Progress?.New(DrawingOperation.ProcessingPixels, bitmapData.Height);
                    IBitmapDataRowInternal row = bitmapData.GetRowCached(0);
                    do
                    {
                        if (context.IsCancellationRequested)
                            return;
                        for (int x = left; x < width; x++)
                            row.DoSetColorIndex(x, index);
                        context.Progress?.Increment();
                    } while (row.MoveNextRow());

                    return;
                }

                // parallel clear
                ParallelHelper.For(context, DrawingOperation.ProcessingPixels, 0, bitmapData.Height, y =>
                {
                    IBitmapDataRowInternal row = bitmapData.GetRowCached(y);
                    int l = left;
                    int w = width;
                    int c = index;
                    for (int x = l; x < w; x++)
                        row.DoSetColorIndex(x, c);
                });
            }

            static void ClearDirectFallback(IAsyncContext context, IBitmapDataInternal bitmapData, Color32 color, int offsetLeft)
            {
                int width = bitmapData.Width;
                if (width - offsetLeft < parallelThreshold)
                {
                    context.Progress?.New(DrawingOperation.ProcessingPixels, bitmapData.Height);
                    IBitmapDataRowInternal row = bitmapData.GetRowCached(0);
                    do
                    {
                        if (context.IsCancellationRequested)
                            return;
                        for (int x = offsetLeft; x < width; x++)
                            row.DoSetColor32(x, color);
                        context.Progress?.Increment();
                    } while (row.MoveNextRow());

                    return;
                }

                // parallel clear
                ParallelHelper.For(context, DrawingOperation.ProcessingPixels, 0, bitmapData.Height, y =>
                {
                    IBitmapDataRowInternal row = bitmapData.GetRowCached(y);
                    int l = offsetLeft;
                    int w = width;
                    Color32 c = color;
                    for (int x = l; x < w; x++)
                        row.DoSetColor32(x, c);
                });
            }

            #endregion

            if (!bitmapData.IsCustomPixelFormat && bitmapData.RowSize > 0)
            {
                int bpp = bitmapData.PixelFormat.BitsPerPixel;
                int width = bitmapData.Width;
                switch (bpp)
                {
                    // note: not using a spacial case for 128 bpp because with 1 pixel already 16 bytes are written
                    //       and since a Color32 is specified for clearing the fallback is just good enough
                    case 64:
                        Clear64Bpp(context, bitmapData, color, width);
                        return;

                    case 32:
                        Clear32Bpp(context, bitmapData, color, width);
                        return;

                    case 16:
                        Clear16Bpp(context, bitmapData, color, width);
                        return;

                    case 8 when !bitmapData.PixelFormat.Indexed:
                        Clear8Bpp(context, bitmapData, color, width);
                        return;

                    case <= 8 when bitmapData.PixelFormat.Indexed:
                        ClearIndexed(context, bitmapData, bpp, color, width);
                        return;
                }
            }

            // Direct color-based clear (24/48/96/128 bit formats as well as raw-inaccessible and custom bitmap data)
            ClearDirectFallback(context, bitmapData, color, 0);
        }

        private static void ClearRaw<T>(IAsyncContext context, IBitmapDataInternal bitmapData, int width, T data)
            where T : unmanaged
        {
            // small width: going with sequential clear
            if (width < parallelThreshold)
            {
                context.Progress?.New(DrawingOperation.ProcessingPixels, bitmapData.Height);
                IBitmapDataRowInternal row = bitmapData.GetRowCached(0);
                do
                {
                    if (context.IsCancellationRequested)
                        return;
                    for (int x = 0; x < width; x++)
                        row.DoWriteRaw(x, data);
                    context.Progress?.Increment();
                } while (row.MoveNextRow());
                return;
            }

            // parallel clear
            ParallelHelper.For(context, DrawingOperation.ProcessingPixels, 0, bitmapData.Height, y =>
            {
                IBitmapDataRowInternal row = bitmapData.GetRowCached(y);
                int w = width;
                T raw = data;
                for (int x = 0; x < w; x++)
                    row.DoWriteRaw(x, raw);
            });
        }

        [SuppressMessage("ReSharper", "AccessToDisposedClosure", Justification = "ParallelHelper.For invokes delegates before returning")]
        private static void ClearWithDithering(IAsyncContext context, IBitmapDataInternal bitmapData, Color32 color, IDitherer ditherer)
        {
            IQuantizer quantizer = PredefinedColorsQuantizer.FromBitmapData(bitmapData);
            context.Progress?.New(DrawingOperation.InitializingQuantizer); // predefined will be extreme fast bu in case someone tracks progress...
            using (IQuantizingSession quantizingSession = quantizer.Initialize(bitmapData, context))
            {
                if (context.IsCancellationRequested)
                    return;
                IReadableBitmapData initSource = ditherer.InitializeReliesOnContent
                    ? new SolidBitmapData(bitmapData.Size, color)
                    : bitmapData;

                try
                {
                    context.Progress?.New(DrawingOperation.InitializingDitherer);
                    using (IDitheringSession ditheringSession = ditherer.Initialize(initSource, quantizingSession, context))
                    {
                        if (context.IsCancellationRequested)
                            return;
                        if (ditheringSession == null)
                            throw new InvalidOperationException(Res.ImagingDithererInitializeNull);

                        // sequential clear
                        if (ditheringSession.IsSequential || bitmapData.Width < parallelThreshold >> ditheringScale)
                        {
                            context.Progress?.New(DrawingOperation.ProcessingPixels, bitmapData.Height);
                            IBitmapDataRowInternal row = bitmapData.GetRowCached(0);
                            int y = 0;
                            do
                            {
                                if (context.IsCancellationRequested)
                                    return;
                                for (int x = 0; x < bitmapData.Width; x++)
                                    row.DoSetColor32(x, ditheringSession.GetDitheredColor(color, x, y));
                                y += 1;
                                context.Progress?.Increment();
                            } while (row.MoveNextRow());

                            return;
                        }

                        // parallel clear
                        ParallelHelper.For(context, DrawingOperation.ProcessingPixels, 0, bitmapData.Height, y =>
                        {
                            IBitmapDataRowInternal row = bitmapData.GetRowCached(y);
                            for (int x = 0; x < bitmapData.Width; x++)
                                row.DoSetColor32(x, ditheringSession.GetDitheredColor(color, x, y));
                        });
                    }
                }
                finally
                {
                    if (!ReferenceEquals(initSource, bitmapData))
                        initSource.Dispose();
                }
            }
        }

        #endregion

        #endregion
    }
}
