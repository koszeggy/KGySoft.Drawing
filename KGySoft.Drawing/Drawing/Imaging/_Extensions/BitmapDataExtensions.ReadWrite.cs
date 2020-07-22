#region Copyright

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

#endregion

namespace KGySoft.Drawing.Imaging
{
    partial class BitmapDataExtensions
    {
        #region Methods

        #region Public Methods

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
        /// methods cannot be used. It can occur with indexed <see cref="IBitmapData.PixelFormat"/>s if the left edge of the clipping is not on byte boundary.</para>
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
                        IBitmapDataRowInternal row = accessor.GetRow(0);
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
                        IBitmapDataRowInternal row = accessor.GetRow(y);
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
                        IBitmapDataRowInternal row = accessor.GetRow(0);
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
                        IBitmapDataRowInternal row = accessor.GetRow(y);
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

        #endregion
    }
}
