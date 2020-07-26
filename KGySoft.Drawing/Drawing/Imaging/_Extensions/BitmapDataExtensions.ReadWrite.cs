﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BitmapDataExtensions.ReadWrite.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2020 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution. If not, then this file is considered as
//  an illegal copy.
//
//  Unauthorized copying of this file, via any medium is strictly prohibited.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;

using KGySoft.Collections;
using KGySoft.CoreLibraries;

#endregion

namespace KGySoft.Drawing.Imaging
{
    partial class BitmapDataExtensions
    {
        #region Fields

        private static readonly IThreadSafeCacheAccessor<float, byte[]> gammaLookupTableCache = new Cache<float, byte[]>(GenerateGammaLookupTable, 16).GetThreadSafeAccessor();

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
        /// <returns>An <see cref="IReadWriteBitmapData"/> that provides access only to the specified region withing the <paramref name="source"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="clippingRegion"/> has no overlapping region with source bounds.</exception>
        /// <remarks>
        /// <para>The <see cref="IBitmapData.RowSize"/> property of the returned instance can be 0, indicating that the <see cref="IReadableBitmapDataRow.ReadRaw{T}">ReadRaw</see>/<see cref="IWritableBitmapDataRow.WriteRaw{T}">WriteRaw</see>
        /// method cannot be used. It can occur if the left edge of the clipping is not zero.</para>
        /// <para>Even if <see cref="IBitmapData.RowSize"/> property of the returned instance is a nonzero value it can happen that it is too low to access all columns
        /// by the <see cref="IReadableBitmapDataRow.ReadRaw{T}">ReadRaw</see>/<see cref="IWritableBitmapDataRow.WriteRaw{T}">WriteRaw</see> methods. It can occur with indexed <see cref="IBitmapData.PixelFormat"/>s if the right edge of the clipping is not on byte boundary.</para>
        /// </remarks>
        public static IReadWriteBitmapData Clip(this IReadWriteBitmapData source, Rectangle clippingRegion)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source), PublicResources.ArgumentNull);
            return clippingRegion.Location.IsEmpty && clippingRegion.Size == source.GetSize()
                ? source
                : new ClippedBitmapData(source, clippingRegion);
        }

        #endregion

        #region Quantizing
        
        /// <summary>
        /// Quantizes an <see cref="IReadWriteBitmapData"/> using the specified <paramref name="quantizer"/> (reduces the number of colors).
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="bitmapData">An <see cref="IReadWriteBitmapData"/> instance to be quantized.</param>
        /// <param name="quantizer">An <see cref="IQuantizer"/> implementation to be used for quantizing the specified <paramref name="bitmapData"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> or <paramref name="quantizer"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException">The <paramref name="quantizer"/>'s <see cref="IQuantizer.Initialize">Initialize</see> method returned <see langword="null"/>.</exception>
        /// <remarks>
        /// <para>This method quantizes the specified <paramref name="bitmapData"/> in place (its original content will be overwritten). To return a new instance
        /// use the <see cref="Clone(IReadableBitmapData, PixelFormat, IQuantizer, IDitherer)">Clone</see> extension method instead.</para>
        /// <para>If the <see cref="PixelFormat"/> or the palette of <paramref name="bitmapData"/> is not compatible with the colors of the specified <paramref name="quantizer"/>, then
        /// the result may not be correct.</para>
        /// <para>If <paramref name="bitmapData"/> has already the same set of colors that the specified <paramref name="quantizer"/>, then it can happen
        /// that calling this method does not change the <paramref name="bitmapData"/> at all.</para>
        /// <note type="tip"><list type="bullet">
        /// <item>To use predefined colors or custom quantization functions use the static methods of the <see cref="PredefinedColorsQuantizer"/> class.
        /// <br/>See the <strong>Remarks</strong> section of its members for details and examples.</item>
        /// <item>To use an optimized palette of up to 256 colors adjusted for <paramref name="bitmapData"/> see the <see cref="OptimizedPaletteQuantizer"/> class.</item>
        /// </list></note>
        /// </remarks>
        /// <seealso cref="BitmapExtensions.Quantize"/>
        [SuppressMessage("ReSharper", "AccessToDisposedClosure", Justification = "ParallelHelper.For invokes delegates before returning")]
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "False alarm, accessor is disposed when needed")]
        public static void Quantize(this IReadWriteBitmapData bitmapData, IQuantizer quantizer)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            if (quantizer == null)
                throw new ArgumentNullException(nameof(quantizer), PublicResources.ArgumentNull);

            IBitmapDataInternal accessor = bitmapData as IBitmapDataInternal ?? new BitmapDataWrapper(bitmapData, true, true);
            try
            {
                using (IQuantizingSession session = quantizer.Initialize(bitmapData))
                {
                    if (session == null)
                        throw new InvalidOperationException(Res.ImagingQuantizerInitializeNull);

                    // Sequential processing
                    if (bitmapData.Width < parallelThreshold >> quantizingScale)
                    {
                        int width = bitmapData.Width;
                        IBitmapDataRowInternal row = accessor.DoGetRow(0);
                        do
                        {
                            for (int x = 0; x < width; x++)
                                row.DoSetColor32(x, session.GetQuantizedColor(row.DoGetColor32(x)));
                        } while (row.MoveNextRow());

                        return;
                    }

                    // Parallel processing
                    ParallelHelper.For(0, bitmapData.Height, y =>
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
        /// <para>This method quantizes <paramref name="bitmapData"/> with dithering in place (its original content will be overwritten). To return a new instance
        /// use the <see cref="Clone(IReadableBitmapData, PixelFormat, IQuantizer, IDitherer)">Clone</see> extension method instead.</para>
        /// <para>If the <see cref="PixelFormat"/> or the palette of <paramref name="bitmapData"/> is not compatible with the colors of the specified <paramref name="quantizer"/>, then
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
        /// <seealso cref="BitmapExtensions.Dither"/>
        [SuppressMessage("ReSharper", "AccessToDisposedClosure", Justification = "ParallelHelper.For invokes delegates before returning")]
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "False alarm, accessor is disposed when needed")]
        public static void Dither(this IReadWriteBitmapData bitmapData, IQuantizer quantizer, IDitherer ditherer)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            if (quantizer == null)
                throw new ArgumentNullException(nameof(quantizer), PublicResources.ArgumentNull);
            if (ditherer == null)
                throw new ArgumentNullException(nameof(ditherer), PublicResources.ArgumentNull);

            IBitmapDataInternal accessor = bitmapData as IBitmapDataInternal ?? new BitmapDataWrapper(bitmapData, true, true);

            try
            {
                using (IQuantizingSession quantizingSession = quantizer.Initialize(bitmapData) ?? throw new InvalidOperationException(Res.ImagingQuantizerInitializeNull))
                using (IDitheringSession ditheringSession = ditherer.Initialize(bitmapData, quantizingSession) ?? throw new InvalidOperationException(Res.ImagingDithererInitializeNull))
                {
                    // Sequential processing
                    if (ditheringSession.IsSequential || bitmapData.Width < parallelThreshold >> ditheringScale)
                    {
                        int width = bitmapData.Width;
                        IBitmapDataRowInternal row = accessor.DoGetRow(0);
                        int y = 0;
                        do
                        {
                            for (int x = 0; x < width; x++)
                                row.DoSetColor32(x, ditheringSession.GetDitheredColor(row.DoGetColor32(x), x, y));

                            y += 1;
                        } while (row.MoveNextRow());

                        return;
                    }

                    // Parallel processing
                    ParallelHelper.For(0, bitmapData.Height, y =>
                    {
                        int width = bitmapData.Width;
                        IBitmapDataRowInternal row = accessor.DoGetRow(y);
                        for (int x = 0; x < width; x++)
                            row.DoSetColor32(x, ditheringSession.GetDitheredColor(row.DoGetColor32(x), x, y));
                    });
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

        /// <summary>
        /// Transforms the colors of this <paramref name="bitmapData"/> using the specified <paramref name="transformFunction"/> delegate.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to be transformed.</param>
        /// <param name="transformFunction">The transform function to be used on the colors of the specified <paramref name="bitmapData"/>. It must be thread-safe.</param>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> or <paramref name="transformFunction"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// <para>This method transforms the <paramref name="bitmapData"/> in place (its original content will be overwritten). To return a new instance
        /// use the <see cref="Clone(IReadableBitmapData,PixelFormat,IQuantizer,IDitherer)">Clone</see> extension method
        /// with an <see cref="IQuantizer"/> instance created by the <see cref="PredefinedColorsQuantizer.FromCustomFunction(Func{Color32,Color32},PixelFormat)">PredefinedColorsQuantizer.FromCustomFunction</see> method.</para>
        /// <para>If <paramref name="bitmapData"/> has an indexed <see cref="IBitmapData.PixelFormat"/> and it supports setting the <see cref="IBitmapData.Palette"/>, then its palette entries will be transformed instead of the actual pixels.</para>
        /// <para>On multi-core systems <paramref name="transformFunction"/> might be called concurrently so it must be thread-safe.</para>
        /// <note type="tip">If <paramref name="transformFunction"/> can return colors incompatible with the pixel format of the specified <paramref name="bitmapData"/>, or you want to transform the actual
        /// pixels of an indexed <paramref name="bitmapData"/> instead of modifying the palette, then use the <see cref="TransformColors(IReadWriteBitmapData,Func{Color32,Color32},IDitherer)"/> overload and specify an <see cref="IDitherer"/> instance.</note>
        /// </remarks>
        /// <seealso cref="BitmapExtensions.TransformColors(Bitmap, Func{Color32, Color32}, Color, byte)"/>
        [SuppressMessage("ReSharper", "AccessToDisposedClosure", Justification = "ParallelHelper.For invokes delegates before returning")]
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "False alarm, accessor is disposed when needed")]
        public static void TransformColors(this IReadWriteBitmapData bitmapData, Func<Color32, Color32> transformFunction)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            if (transformFunction == null)
                throw new ArgumentNullException(nameof(transformFunction), PublicResources.ArgumentNull);

            // Indexed format: processing the palette entries when possible
            if (bitmapData is IBitmapDataInternal bitmapDataInternal && bitmapDataInternal.CanSetPalette)
            {
                Palette palette = bitmapData.Palette;
                Color32[] oldEntries = palette.Entries;
                Color32[] newEntries = new Color32[oldEntries.Length];
                for (int i = 0; i < newEntries.Length; i++)
                    newEntries[i] = transformFunction.Invoke(oldEntries[i]);
                if (bitmapDataInternal.TrySetPalette(new Palette(newEntries, palette.BackColor, palette.AlphaThreshold)))
                    return;
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
                    IBitmapDataRowInternal row = accessor.DoGetRow(0);
                    do
                    {
                        for (int x = 0; x < bitmapData.Width; x++)
                            row.DoSetColor32(x, transformFunction.Invoke(row.DoGetColor32(x)));
                    } while (row.MoveNextRow());

                    return;
                }

                // Parallel processing
                ParallelHelper.For(0, bitmapData.Height, y =>
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
        /// <para>This method transforms the <paramref name="bitmapData"/> in place (its original content will be overwritten). To return a new instance
        /// use the <see cref="Clone(IReadableBitmapData,PixelFormat,IQuantizer,IDitherer)">Clone</see> extension method
        /// with an <see cref="IQuantizer"/> instance created by the <see cref="PredefinedColorsQuantizer.FromCustomFunction(Func{Color32,Color32},PixelFormat)">PredefinedColorsQuantizer.FromCustomFunction</see> method.</para>
        /// <para>If <paramref name="bitmapData"/> has an indexed <see cref="IBitmapData.PixelFormat"/> and <paramref name="ditherer"/> is <see langword="null"/>,
        /// then its palette entries are tried to be transformed instead of the actual pixels in the first place (if it is supported by <paramref name="bitmapData"/>).
        /// To transform the colors of an indexed <see cref="IBitmapData"/> without changing the palette specify a non-<see langword="null"/>&#160;<paramref name="ditherer"/>.
        /// Transforming the palette is both faster and provides a better result.</para>
        /// <para>On multi-core systems <paramref name="transformFunction"/> might be called concurrently so it must be thread-safe.</para>
        /// <para>The <paramref name="ditherer"/> is ignored for <see cref="PixelFormat"/>s with more than 16 bits-per-pixel and for the <see cref="PixelFormat.Format16bppGrayScale"/> format.</para>
        /// <note>See the <strong>Examples</strong> section of the <see cref="BitmapExtensions.TransformColors(Bitmap, Func{Color32, Color32}, IDitherer, Color, byte)">BitmapExtensions.TransformColors</see> method for an example.</note>
        /// </remarks>
        /// <seealso cref="BitmapExtensions.TransformColors(Bitmap, Func{Color32, Color32}, IDitherer, Color, byte)"/>
        [SuppressMessage("ReSharper", "AccessToDisposedClosure", Justification = "ParallelHelper.For invokes delegates before returning")]
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "False alarm, accessor is disposed if needed")]
        public static void TransformColors(this IReadWriteBitmapData bitmapData, Func<Color32, Color32> transformFunction, IDitherer ditherer)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);

            PixelFormat pixelFormat = bitmapData.PixelFormat;
            if (ditherer == null || !pixelFormat.CanBeDithered())
            {
                bitmapData.TransformColors(transformFunction);
                return;
            }

            // Special handling if ditherer relies on actual content: transforming into an ARGB32 result, and dithering that temporary result
            if (ditherer.InitializeReliesOnContent)
            {
                // not using premultiplied format because transformation is faster on simple ARGB32
                using IReadWriteBitmapData tempClone = bitmapData.Clone(PixelFormat.Format32bppArgb);
                tempClone.TransformColors(transformFunction);
                tempClone.CopyTo(bitmapData, Point.Empty, ditherer);
                return;
            }

            if (transformFunction == null)
                throw new ArgumentNullException(nameof(transformFunction), PublicResources.ArgumentNull);

            if (bitmapData.Height < 1)
                return;

            var accessor = bitmapData as IBitmapDataInternal ?? new BitmapDataWrapper(bitmapData, true, true);
            try
            {
                IQuantizer quantizer = PredefinedColorsQuantizer.FromBitmapData(bitmapData);
                Debug.Assert(!quantizer.InitializeReliesOnContent, "A predefined color quantizer should not depend on actual content");

                using (IQuantizingSession quantizingSession = quantizer.Initialize(bitmapData))
                using (IDitheringSession ditheringSession = ditherer.Initialize(bitmapData, quantizingSession) ?? throw new InvalidOperationException(Res.ImagingDithererInitializeNull))
                {
                    // sequential processing
                    if (ditheringSession.IsSequential || bitmapData.Width < parallelThreshold)
                    {
                        IBitmapDataRowInternal row = accessor.DoGetRow(0);
                        int y = 0;
                        do
                        {
                            for (int x = 0; x < bitmapData.Width; x++)
                                row.DoSetColor32(x, ditheringSession.GetDitheredColor(transformFunction.Invoke(row.DoGetColor32(x)), x, y));
                            y += 1;
                        } while (row.MoveNextRow());

                        return;
                    }

                    // parallel processing
                    ParallelHelper.For(0, bitmapData.Height, y =>
                    {
                        IBitmapDataRowInternal row = accessor.DoGetRow(y);
                        for (int x = 0; x < bitmapData.Width; x++)
                            row.DoSetColor32(x, ditheringSession.GetDitheredColor(transformFunction.Invoke(row.DoGetColor32(x)), x, y));
                    });
                }
            }
            finally
            {
                if (!ReferenceEquals(bitmapData, accessor))
                    accessor.Dispose();
            }
        }

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
        /// <para>If <paramref name="newColor"/> has alpha, which cannot be represented by <paramref name="bitmapData"/>, then it will be blended with <see cref="IBitmapData.BackColor"/>.</para>
        /// <para>This method calls the <see cref="TransformColors(IReadWriteBitmapData, Func{Color32, Color32}, IDitherer)">TransformColors</see> method internally. See
        /// the <strong>Remarks</strong> section of the <see cref="TransformColors(IReadWriteBitmapData, Func{Color32, Color32}, IDitherer)">TransformColors</see> method for more details.</para>
        /// <para>If <paramref name="bitmapData"/> has an indexed <see cref="IBitmapData.PixelFormat"/> and <paramref name="ditherer"/> is <see langword="null"/>,
        /// then its palette entries are tried to be transformed instead of the actual pixels in the first place (if it is supported by <paramref name="bitmapData"/>).
        /// To transform the colors of an indexed <see cref="IBitmapData"/> without changing the palette specify a non-<see langword="null"/>&#160;<paramref name="ditherer"/>.
        /// Transforming the palette is both faster and provides a better result.</para>
        /// <para>The <paramref name="ditherer"/> is ignored for <see cref="PixelFormat"/>s with more than 16 bits-per-pixel and for the <see cref="PixelFormat.Format16bppGrayScale"/> format.</para>
        /// </remarks>
        /// <seealso cref="BitmapExtensions.ReplaceColor"/>
        public static void ReplaceColor(this IReadWriteBitmapData bitmapData, Color32 oldColor, Color32 newColor, IDitherer ditherer = null)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            if (oldColor == newColor)
                return;

            Color32 Transform(Color32 c) => c == oldColor ? newColor : c;

            bitmapData.TransformColors(Transform, ditherer);
        }

        /// <summary>
        /// Inverts the colors of the specified <paramref name="bitmapData"/>.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to be inverted.</param>
        /// <param name="ditherer">An optional <see cref="IDitherer"/> instance to dither the result of the transformation if the inverse of the <paramref name="bitmapData"/>
        /// has no exact representation with its <see cref="IBitmapData.PixelFormat"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <remarks>
        /// <para>This method calls the <see cref="TransformColors(IReadWriteBitmapData, Func{Color32, Color32}, IDitherer)">TransformColors</see> method internally. See
        /// the <strong>Remarks</strong> section of the <see cref="TransformColors(IReadWriteBitmapData, Func{Color32, Color32}, IDitherer)">TransformColors</see> method for more details.</para>
        /// <para>If <paramref name="bitmapData"/> has an indexed <see cref="IBitmapData.PixelFormat"/> and <paramref name="ditherer"/> is <see langword="null"/>,
        /// then its palette entries are tried to be transformed instead of the actual pixels in the first place (if it is supported by <paramref name="bitmapData"/>).
        /// To transform the colors of an indexed <see cref="IBitmapData"/> without changing the palette specify a non-<see langword="null"/>&#160;<paramref name="ditherer"/>.
        /// Transforming the palette is both faster and provides a better result.</para>
        /// <para>The <paramref name="ditherer"/> is ignored for <see cref="PixelFormat"/>s with more than 16 bits-per-pixel and for the <see cref="PixelFormat.Format16bppGrayScale"/> format.</para>
        /// </remarks>
        /// <seealso cref="BitmapExtensions.Invert"/>
        public static void Invert(this IReadWriteBitmapData bitmapData, IDitherer ditherer = null)
        {
            static Color32 Transform(Color32 c) => new Color32(c.A, (byte)(255 - c.R), (byte)(255 - c.G), (byte)(255 - c.B));

            bitmapData.TransformColors(Transform, ditherer);
        }

        /// <summary>
        /// If possible, makes the background of this <paramref name="bitmapData"/> transparent, taking the bottom-left pixel as the background color.
        /// If the <paramref name="bitmapData"/> does not support transparency, then the pixels that have the same color as the bottom-left pixel will be set
        /// to the <see cref="IBitmapData.BackColor"/> of <paramref name="bitmapData"/>.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to make transparent.</param>
        /// <remarks>
        /// <para>This method calls the <see cref="ReplaceColor">ReplaceColor</see> method internally.</para>
        /// <para>Similarly to the <see cref="Bitmap.MakeTransparent()">Bitmap.MakeTransparent</see> method, this one uses the bottom-left pixel to determine
        /// the background color, which must be completely opaque; otherwise, <paramref name="bitmapData"/> will not be changed.</para>
        /// <para>Unlike the <see cref="Bitmap.MakeTransparent()">Bitmap.MakeTransparent</see> method, this one preserves the original <see cref="IBitmapData.PixelFormat"/>.
        /// If <paramref name="bitmapData"/> does not support transparency and cannot set <see cref="IBitmapData.Palette"/> either, then every occurrence of the
        /// color of the bottom-left pixel will be changed to the <see cref="IBitmapData.BackColor"/> of <paramref name="bitmapData"/>.
        /// To make such bitmaps transparent use the <see cref="ToTransparent(IReadableBitmapData)">ToTransparent</see> method instead,
        /// which returns a new instance with <see cref="PixelFormat.Format32bppArgb"/>&#160;<see cref="IBitmapData.PixelFormat"/>.</para>
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
            bitmapData.ReplaceColor(transparentColor, default);
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
        /// <para>This method calls the <see cref="ReplaceColor">ReplaceColor</see> method internally.</para>
        /// <para>Unlike the <see cref="Bitmap.MakeTransparent(Color)">Bitmap.MakeTransparent(Color)</see> method, this one preserves the original <see cref="IBitmapData.PixelFormat"/>.
        /// If <paramref name="bitmapData"/> does not support transparency and cannot set <see cref="IBitmapData.Palette"/> either, then every occurrence of the
        /// <paramref name="transparentColor"/> will be changed to the <see cref="IBitmapData.BackColor"/> of <paramref name="bitmapData"/>.
        /// To make such bitmaps transparent use the <see cref="ToTransparent(IReadableBitmapData,Color32)">ToTransparent</see> method instead,
        /// which returns a new instance with <see cref="PixelFormat.Format32bppArgb"/>&#160;<see cref="IBitmapData.PixelFormat"/>.</para>
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
            bitmapData.ReplaceColor(transparentColor, default);
        }

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
        /// <para>This method calls the <see cref="TransformColors(IReadWriteBitmapData, Func{Color32, Color32}, IDitherer)">TransformColors</see> method internally. See
        /// the <strong>Remarks</strong> section of the <see cref="TransformColors(IReadWriteBitmapData, Func{Color32, Color32}, IDitherer)">TransformColors</see> method for more details.</para>
        /// <para>If <paramref name="bitmapData"/> has an indexed <see cref="IBitmapData.PixelFormat"/> and <paramref name="ditherer"/> is <see langword="null"/>,
        /// then its palette entries are tried to be transformed instead of the actual pixels in the first place (if it is supported by <paramref name="bitmapData"/>).
        /// To transform the colors of an indexed <see cref="IBitmapData"/> without changing the palette specify a non-<see langword="null"/>&#160;<paramref name="ditherer"/>.
        /// Transforming the palette is both faster and provides a better result.</para>
        /// <para>The <paramref name="ditherer"/> is ignored for <see cref="PixelFormat"/>s with more than 16 bits-per-pixel and for the <see cref="PixelFormat.Format16bppGrayScale"/> format.</para>
        /// </remarks>
        /// <seealso cref="BitmapExtensions.MakeOpaque"/>
        public static void MakeOpaque(this IReadWriteBitmapData bitmapData, Color32 backColor, IDitherer ditherer = null)
        {
            Color32 color = backColor.ToOpaque();

            Color32 Transform(Color32 c) => c.A == Byte.MaxValue ? c : c.BlendWithBackground(color);

            bitmapData.TransformColors(Transform, ditherer);
        }

        /// <summary>
        /// Makes this <paramref name="bitmapData"/> grayscale.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> to make grayscale.</param>
        /// <param name="ditherer">An optional <see cref="IDitherer"/> instance to dither the result if grayscale colors
        /// cannot be represented by the <see cref="IBitmapData.PixelFormat"/> or the current palette of the specified <paramref name="bitmapData"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <remarks>
        /// <para>This method transforms the <paramref name="bitmapData"/> in place (its original content will be overwritten). To return a new instance
        /// use the <see cref="ToGrayscale">ToGrayscale</see> extension method, which always returns a bitmap data with <see cref="PixelFormat.Format32bppArgb"/> format,
        /// or the <see cref="Clone(IReadableBitmapData, PixelFormat, IQuantizer, IDitherer)">Clone</see> method with a grayscale
        /// quantizer (<see cref="PredefinedColorsQuantizer.Grayscale">PredefinedColorsQuantizer.Grayscale</see>, for example).</para>
        /// <para>This method calls the <see cref="TransformColors(IReadWriteBitmapData, Func{Color32, Color32}, IDitherer)">TransformColors</see> method internally. See
        /// the <strong>Remarks</strong> section of the <see cref="TransformColors(IReadWriteBitmapData, Func{Color32, Color32}, IDitherer)">TransformColors</see> method for more details.</para>
        /// <para>If <paramref name="bitmapData"/> has an indexed <see cref="IBitmapData.PixelFormat"/> and <paramref name="ditherer"/> is <see langword="null"/>,
        /// then its palette entries are tried to be transformed instead of the actual pixels in the first place (if it is supported by <paramref name="bitmapData"/>).
        /// To transform the colors of an indexed <see cref="IBitmapData"/> without changing the palette specify a non-<see langword="null"/>&#160;<paramref name="ditherer"/>.
        /// Transforming the palette is both faster and provides a better result.</para>
        /// <para>The <paramref name="ditherer"/> is ignored for <see cref="PixelFormat"/>s with more than 16 bits-per-pixel and for the <see cref="PixelFormat.Format16bppGrayScale"/> format.</para>
        /// </remarks>
        /// <seealso cref="ToGrayscale"/>
        /// <seealso cref="BitmapExtensions.MakeGrayscale"/>
        /// <seealso cref="ImageExtensions.ToGrayscale"/>
        public static void MakeGrayscale(this IReadWriteBitmapData bitmapData, IDitherer ditherer = null)
        {
            static Color32 Transform(Color32 c)
            {
                byte br = c.GetBrightness();
                return new Color32(c.A, br, br, br);
            }

            bitmapData.TransformColors(Transform, ditherer);
        }

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
        /// <para>This method calls the <see cref="TransformColors(IReadWriteBitmapData, Func{Color32, Color32}, IDitherer)">TransformColors</see> method internally. See
        /// the <strong>Remarks</strong> section of the <see cref="TransformColors(IReadWriteBitmapData, Func{Color32, Color32}, IDitherer)">TransformColors</see> method for more details.</para>
        /// <para>If <paramref name="bitmapData"/> has an indexed <see cref="IBitmapData.PixelFormat"/> and <paramref name="ditherer"/> is <see langword="null"/>,
        /// then its palette entries are tried to be transformed instead of the actual pixels in the first place (if it is supported by <paramref name="bitmapData"/>).
        /// To transform the colors of an indexed <see cref="IBitmapData"/> without changing the palette specify a non-<see langword="null"/>&#160;<paramref name="ditherer"/>.
        /// Transforming the palette is both faster and provides a better result.</para>
        /// <para>The <paramref name="ditherer"/> is ignored for <see cref="PixelFormat"/>s with more than 16 bits-per-pixel and for the <see cref="PixelFormat.Format16bppGrayScale"/> format.</para>
        /// <note>See the <strong>Examples</strong> section of the <see cref="BitmapExtensions.AdjustBrightness">BitmapExtensions.AdjustBrightness</see> method for an example.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="brightness"/> is not between -1 and 1
        /// <br/>-or-
        /// <br/><paramref name="channels"/> is out of the defined flags.</exception>
        /// <seealso cref="BitmapExtensions.AdjustBrightness"/>
        public static void AdjustBrightness(this IReadWriteBitmapData bitmapData, float brightness, IDitherer ditherer = null, ColorChannels channels = ColorChannels.Rgb)
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
                bitmapData.TransformColors(Darken, ditherer);
            }
            else
                bitmapData.TransformColors(Lighten, ditherer);

            #region Local Methods

            Color32 Darken(Color32 c) => new Color32(c.A,
                (channels & ColorChannels.R) == ColorChannels.R ? (byte)(c.R * brightness) : c.R,
                (channels & ColorChannels.G) == ColorChannels.G ? (byte)(c.G * brightness) : c.G,
                (channels & ColorChannels.B) == ColorChannels.B ? (byte)(c.B * brightness) : c.B);

            Color32 Lighten(Color32 c) => new Color32(c.A,
                (channels & ColorChannels.R) == ColorChannels.R ? (byte)((255 - c.R) * brightness + c.R) : c.R,
                (channels & ColorChannels.G) == ColorChannels.G ? (byte)((255 - c.G) * brightness + c.G) : c.G,
                (channels & ColorChannels.B) == ColorChannels.B ? (byte)((255 - c.B) * brightness + c.B) : c.B);

            #endregion
        }

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
        /// <para>This method calls the <see cref="TransformColors(IReadWriteBitmapData, Func{Color32, Color32}, IDitherer)">TransformColors</see> method internally. See
        /// the <strong>Remarks</strong> section of the <see cref="TransformColors(IReadWriteBitmapData, Func{Color32, Color32}, IDitherer)">TransformColors</see> method for more details.</para>
        /// <para>If <paramref name="bitmapData"/> has an indexed <see cref="IBitmapData.PixelFormat"/> and <paramref name="ditherer"/> is <see langword="null"/>,
        /// then its palette entries are tried to be transformed instead of the actual pixels in the first place (if it is supported by <paramref name="bitmapData"/>).
        /// To transform the colors of an indexed <see cref="IBitmapData"/> without changing the palette specify a non-<see langword="null"/>&#160;<paramref name="ditherer"/>.
        /// Transforming the palette is both faster and provides a better result.</para>
        /// <para>The <paramref name="ditherer"/> is ignored for <see cref="PixelFormat"/>s with more than 16 bits-per-pixel and for the <see cref="PixelFormat.Format16bppGrayScale"/> format.</para>
        /// <note>See the <strong>Examples</strong> section of the <see cref="BitmapExtensions.AdjustContrast">BitmapExtensions.AdjustContrast</see> method for an example.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="contrast"/> is not between -1 and 1
        /// <br/>-or-
        /// <br/><paramref name="channels"/> is out of the defined flags.</exception>
        /// <seealso cref="BitmapExtensions.AdjustContrast"/>
        public static void AdjustContrast(this IReadWriteBitmapData bitmapData, float contrast, IDitherer ditherer = null, ColorChannels channels = ColorChannels.Rgb)
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

            Color32 Transform(Color32 c) => new Color32(c.A,
                (channels & ColorChannels.R) == ColorChannels.R ? ((int)(((c.R / 255f - 0.5f) * contrast + 0.5f) * 255f)).ClipToByte() : c.R,
                (channels & ColorChannels.G) == ColorChannels.G ? ((int)(((c.G / 255f - 0.5f) * contrast + 0.5f) * 255f)).ClipToByte() : c.G,
                (channels & ColorChannels.B) == ColorChannels.B ? ((int)(((c.B / 255f - 0.5f) * contrast + 0.5f) * 255f)).ClipToByte() : c.B);

            bitmapData.TransformColors(Transform, ditherer);
        }

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
        /// <para>This method calls the <see cref="TransformColors(IReadWriteBitmapData, Func{Color32, Color32}, IDitherer)">TransformColors</see> method internally. See
        /// the <strong>Remarks</strong> section of the <see cref="TransformColors(IReadWriteBitmapData, Func{Color32, Color32}, IDitherer)">TransformColors</see> method for more details.</para>
        /// <para>If <paramref name="bitmapData"/> has an indexed <see cref="IBitmapData.PixelFormat"/> and <paramref name="ditherer"/> is <see langword="null"/>,
        /// then its palette entries are tried to be transformed instead of the actual pixels in the first place (if it is supported by <paramref name="bitmapData"/>).
        /// To transform the colors of an indexed <see cref="IBitmapData"/> without changing the palette specify a non-<see langword="null"/>&#160;<paramref name="ditherer"/>.
        /// Transforming the palette is both faster and provides a better result.</para>
        /// <para>The <paramref name="ditherer"/> is ignored for <see cref="PixelFormat"/>s with more than 16 bits-per-pixel and for the <see cref="PixelFormat.Format16bppGrayScale"/> format.</para>
        /// <note>See the <strong>Examples</strong> section of the <see cref="BitmapExtensions.AdjustGamma">BitmapExtensions.AdjustGamma</see> method for an example.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="gamma"/> is not between 0 and 10
        /// <br/>-or-
        /// <br/><paramref name="channels"/> is out of the defined flags.</exception>
        /// <seealso cref="BitmapExtensions.AdjustGamma"/>
        public static void AdjustGamma(this IReadWriteBitmapData bitmapData, float gamma, IDitherer ditherer = null, ColorChannels channels = ColorChannels.Rgb)
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

            byte[] table = gammaLookupTableCache[gamma];

            Color32 Transform(Color32 c) => new Color32(c.A,
                (channels & ColorChannels.R) == ColorChannels.R ? table[c.R] : c.R,
                (channels & ColorChannels.G) == ColorChannels.G ? table[c.G] : c.G,
                (channels & ColorChannels.B) == ColorChannels.B ? table[c.B] : c.B);

            bitmapData.TransformColors(Transform, ditherer);
        }

        #endregion

        #endregion

        #region Private Methods

        private static byte[] GenerateGammaLookupTable(float gamma)
        {
            byte[] result = new byte[256];
            for (int i = 0; i < 256; i++)
                result[i] = ((int)(255d * Math.Pow(i / 255d, 1d / gamma) + 0.5d)).ClipToByte();
            return result;
        }

        #endregion

        #endregion
    }
}