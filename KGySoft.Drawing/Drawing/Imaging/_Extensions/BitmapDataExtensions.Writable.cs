#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BitmapDataExtensions.Writable.cs
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
#if !NET35
using System.Threading.Tasks;
#endif

#endregion

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
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="source">The source bitmap data to be clipped.</param>
        /// <param name="clippingRegion">A <see cref="Rectangle"/> that specifies a region within the <paramref name="source"/>.</param>
        /// <returns>An <see cref="IWritableBitmapData"/> that provides access only to the specified region withing the <paramref name="source"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="clippingRegion"/> has no overlapping region with source bounds.</exception>
        /// <remarks>
        /// <para>The <see cref="IBitmapData.RowSize"/> property of the returned instance can be 0, indicating that the <see cref="IWritableBitmapDataRow.WriteRaw{T}">WriteRaw</see>
        /// method cannot be used. It can occur if the left edge of the clipping is not zero.</para>
        /// <para>Even if <see cref="IBitmapData.RowSize"/> property of the returned instance is a nonzero value it can happen that it is too low to access all columns
        /// by the <see cref="IWritableBitmapDataRow.WriteRaw{T}">WriteRaw</see> method. It can occur with indexed <see cref="IBitmapData.PixelFormat"/>s if the right edge of the clipping is not on byte boundary.</para>
        /// </remarks>
        public static IWritableBitmapData Clip(this IWritableBitmapData source, Rectangle clippingRegion)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source), PublicResources.ArgumentNull);
            return clippingRegion.Location.IsEmpty && clippingRegion.Size == source.GetSize()
                ? source
                : new ClippedBitmapData(source, clippingRegion);
        }

        #endregion

        #region Clear

        /// <summary>
        /// Clears the content of the specified <paramref name="bitmapData"/> and fills it with the specified <paramref name="color"/>.
        /// <br/>This method is similar to <see cref="Graphics.Clear">Graphics.Clear</see> except that this one supports any <see cref="PixelFormat"/> and also dithering.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IWritableBitmapData"/> to be cleared.</param>
        /// <param name="color">A <see cref="Color32"/> that represents the desired result color of the <paramref name="bitmapData"/>.
        /// If it has transparency, which is not supported by <see cref="IBitmapData.PixelFormat"/> of <paramref name="bitmapData"/>, then the result might be either
        /// completely transparent (depends also on <see cref="IBitmapData.AlphaThreshold"/>), or the color will be blended with <see cref="IBitmapData.BackColor"/>.
        /// </param>
        /// <param name="ditherer">The ditherer to be used for the clearing. Has no effect if <see cref="IBitmapData.PixelFormat"/> of <paramref name="bitmapData"/> has at least 24 bits-per-pixel size. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <seealso cref="BitmapExtensions.Clear(Bitmap, Color, IDitherer, Color, byte)"/>
        public static void Clear(this IWritableBitmapData bitmapData, Color32 color, IDitherer ditherer = null)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            DoClear(AsyncContext.Null, bitmapData, color, ditherer);
        }

        public static IAsyncResult BeginClear(this IWritableBitmapData bitmapData, Color32 color, IDitherer ditherer = null, AsyncConfig asyncConfig = null)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            return AsyncContext.BeginOperation(ctx => DoClear(ctx, bitmapData, color, ditherer), asyncConfig);
        }

        public static void EndClear(this IAsyncResult asyncResult) => AsyncContext.EndOperation(asyncResult, nameof(BeginClear));

#if !NET35
        public static Task ClearAsync(this IWritableBitmapData bitmapData, Color32 color, IDitherer ditherer = null, TaskConfig asyncConfig = null)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            return AsyncContext.DoOperationAsync(ctx => DoClear(ctx, bitmapData, color, ditherer), asyncConfig);
        }
#endif

        #endregion

        #region TrySetPalette

        /// <summary>
        /// Tries to the set the specified <paramref name="palette"/> for this <see cref="IWritableBitmapData"/>.
        /// <br/>See the <strong>Remarks</strong> section for details.
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
            return bitmapData is IBitmapDataInternal internalBitmapData && internalBitmapData.TrySetPalette(palette);
        }

        #endregion

        #endregion

        #region Private Methods

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "False alarm, accessor is disposed when needed")]
        private static void DoClear(IAsyncContext context, IWritableBitmapData bitmapData, Color32 color, IDitherer ditherer)
        {
            IBitmapDataInternal accessor = bitmapData as IBitmapDataInternal ?? new BitmapDataWrapper(bitmapData, false, true);
            try
            {
                if (ditherer == null || !accessor.PixelFormat.CanBeDithered())
                    ClearDirect(context, accessor, color);
                else
                    ClearWithDithering(context, accessor, color, ditherer);
            }
            finally
            {
                if (!ReferenceEquals(accessor, bitmapData))
                    accessor.Dispose();
            }
        }

        private static void ClearDirect(IAsyncContext context, IBitmapDataInternal bitmapData, Color32 color)
        {
            int bpp = bitmapData.PixelFormat.ToBitsPerPixel();
            int left = 0;
            int width = bitmapData.Width;
            switch (bpp)
            {
                case 32:
                    int longWidth = bitmapData.RowSize >> 3;

                    // writing as longs
                    if (longWidth > 0)
                    {
                        Color32 rawColor = bitmapData.PixelFormat switch
                        {
                            PixelFormat.Format32bppPArgb => color.ToPremultiplied(),
                            PixelFormat.Format32bppRgb => color.BlendWithBackground(bitmapData.BackColor),
                            _ => color,
                        };

                        uint argb = (uint)rawColor.ToArgb();
                        ClearRaw(context, bitmapData, longWidth, ((ulong)argb << 32) | argb);
                    }

                    // handling the rest (can be either the last column if width is odd, or even the whole content if RowSize is 0)
                    left = longWidth << 1;
                    if (left < width && !context.IsCancellationRequested)
                        ClearDirectFallback(context, bitmapData, color, left);

                    return;

                case 16:
                    longWidth = bitmapData.RowSize >> 3;

                    // writing as longs
                    if (longWidth > 0)
                    {
                        ushort shortValue = bitmapData.PixelFormat switch
                        {
                            PixelFormat.Format16bppArgb1555 => new Color16Argb1555(color).Value,
                            PixelFormat.Format16bppRgb565 => new Color16Rgb565(color).Value,
                            PixelFormat.Format16bppRgb555 => new Color16Rgb555(color).Value,
                            _ => new Color16Gray(color).Value
                        };

                        uint uintValue = (uint)((shortValue << 16) | shortValue);
                        ClearRaw(context, bitmapData, longWidth, ((ulong)uintValue << 32) | uintValue);
                    }

                    // handling the rest (or even the whole content if RowSize is 0)
                    left = longWidth << 2;
                    if (left < width && !context.IsCancellationRequested)
                        ClearDirectFallback(context, bitmapData, color, left);

                    return;

                case 8:
                case 4:
                case 1:
                    int index = bitmapData.Palette.GetNearestColorIndex(color);
                    byte byteValue = bpp == 8 ? (byte)index
                        : bpp == 4 ? (byte)((index << 4) | index)
                        : index == 1 ? Byte.MaxValue : Byte.MinValue;

                    if (bitmapData.RowSize > 0)
                    {
                        int factor = bpp == 8 ? 0 
                            : bpp == 4 ? 1 
                            : 3;

                        // writing as longs
                        if ((bitmapData.RowSize & 0b111) == 0)
                        {
                            longWidth = bitmapData.RowSize >> 3;
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
                        IBitmapDataRowInternal row = bitmapData.DoGetRow(0);
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
                        // ReSharper disable once VariableHidesOuterVariable
                        IBitmapDataRowInternal row = bitmapData.DoGetRow(y);
                        int l = left;
                        int w = width;
                        int c = index;
                        for (int x = l; x < w; x++)
                            row.DoSetColorIndex(x, c);
                    });
                    return;

                // Direct color-based clear (24/48/64 bit formats as well as raw-inaccessible bitmap data)
                // 64 bit is not handled above because its actual format may depend on actual bitmap data type
                default:
                    // small width: going with sequential clear
                    ClearDirectFallback(context, bitmapData, color, 0);
                    return;
            }
        }

        private static void ClearDirectFallback(IAsyncContext context, IBitmapDataInternal bitmapData, Color32 color, int offsetLeft)
        {
            int width = bitmapData.Width;
            if (width - offsetLeft < parallelThreshold)
            {
                context.Progress?.New(DrawingOperation.ProcessingPixels, bitmapData.Height);
                IBitmapDataRowInternal row = bitmapData.DoGetRow(0);
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
                // ReSharper disable once VariableHidesOuterVariable
                IBitmapDataRowInternal row = bitmapData.DoGetRow(y);
                int l = offsetLeft;
                int w = width;
                Color32 c = color;
                for (int x = l; x < w; x++)
                    row.DoSetColor32(x, c);
            });
        }

        private static void ClearRaw<T>(IAsyncContext context, IBitmapDataInternal bitmapData, int width, T data)
            where T : unmanaged
        {
            // small width: going with sequential clear
            if (width < parallelThreshold)
            {
                context.Progress?.New(DrawingOperation.ProcessingPixels, bitmapData.Height);
                IBitmapDataRowInternal row = bitmapData.DoGetRow(0);
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
                IBitmapDataRowInternal row = bitmapData.DoGetRow(y);
                int w = width;
                T raw = data;
                for (int x = 0; x < w; x++)
                    row.DoWriteRaw(x, raw);
            });
        }

        [SuppressMessage("ReSharper", "AccessToDisposedClosure", Justification = "ParallelHelper.For invokes delegates before returning")]
        [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "False alarm, initSource is disposed if needed")]
        private static void ClearWithDithering(IAsyncContext context, IBitmapDataInternal bitmapData, Color32 color, IDitherer ditherer)
        {
            IQuantizer quantizer = PredefinedColorsQuantizer.FromBitmapData(bitmapData);
            context.Progress?.New(DrawingOperation.InitializingQuantizer); // predefined will be extreme fast bu in case someone tracks progress...
            using (IQuantizingSession quantizingSession = quantizer.Initialize(bitmapData, context))
            {
                if (context.IsCancellationRequested)
                    return;
                IReadableBitmapData initSource = ditherer.InitializeReliesOnContent
                    ? new SolidBitmapData(bitmapData.GetSize(), color)
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
                            IBitmapDataRowInternal row = bitmapData.DoGetRow(0);
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
                            IBitmapDataRowInternal row = bitmapData.DoGetRow(y);
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
