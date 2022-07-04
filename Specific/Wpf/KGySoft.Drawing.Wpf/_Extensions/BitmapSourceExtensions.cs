#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BitmapSourceExtensions.cs
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

#region Used Namespaces

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
#if !NET35
using System.Threading.Tasks;
#endif
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

using KGySoft.Collections;
using KGySoft.Drawing.Imaging;
using KGySoft.Reflection;
using KGySoft.Threading;

#endregion

#region Used Aliases

using Color = System.Windows.Media.Color;

#endregion

#endregion

namespace KGySoft.Drawing.Wpf
{
    /// <summary>
    /// Contains extension methods for the <see cref="BitmapSource"/> type.
    /// </summary>
    public static class BitmapSourceExtensions
    {
        #region ConversionContext class

        /// <summary>
        /// Contains all possible prefetched data to avoid most of the sync callbacks from the working thread
        /// </summary>
        private sealed class ConversionContext : IDisposable
        {
            #region Fields

            private static readonly TimeSpan callbackTimeout = TimeSpan.FromMilliseconds(250);

            #endregion

            #region Properties

            internal BitmapSource Bitmap { get; }
            internal PixelFormat NewPixelFormat { get; }
            internal IReadableBitmapData Source { get; private set; } = default!;
            internal WriteableBitmap? Result { get; set; }
            internal IWritableBitmapData? Target { get; set; }
            internal IQuantizer? Quantizer { get; set; }
            internal IDitherer? Ditherer { get; }

            #endregion

            #region Constructors

            #region Internal Constructors

            internal ConversionContext(BitmapSource bitmap, PixelFormat newPixelFormat, Color[]? palette, Color backColor, byte alphaThreshold)
                : this(bitmap, newPixelFormat) => InitDirect(bitmap, palette, backColor, alphaThreshold);

            internal ConversionContext(BitmapSource bitmap, PixelFormat newPixelFormat, IQuantizer? quantizer, IDitherer? ditherer)
                : this(bitmap, newPixelFormat)
            {
                Quantizer = quantizer;
                IList<Color>? sourcePaletteEntries = null;
                if (quantizer == null)
                {
                    // converting without using a quantizer (even if only a ditherer is specified for a high-bpp pixel format)
                    if (ditherer == null || !newPixelFormat.CanBeDithered())
                    {
                        InitDirect(bitmap, null, default, 128);
                        return;
                    }

                    // here we need to pick a quantizer for the dithering
                    int bpp = newPixelFormat.BitsPerPixel;
                    BitmapPalette? sourcePalette = null;
                    Invoke(true, () => sourcePalette = bitmap.Palette);

                    sourcePaletteEntries = sourcePalette?.Colors ?? Reflector.EmptyArray<Color>();
                    if (bpp <= 8 && sourcePaletteEntries.Count > 0 && sourcePaletteEntries.Count <= (1 << bpp))
                        Quantizer = PredefinedColorsQuantizer.FromCustomPalette(new Palette(sourcePaletteEntries.Select(c => c.ToColor32()).ToArray()));
                    else
                    {
                        Quantizer = newPixelFormat.GetDefaultQuantizer();
                        sourcePaletteEntries = null;
                    }
                }

                Ditherer = ditherer;
                Invoke(true, () =>
                {
                    Source = GetSourceBitmapData(bitmap);

                    // Precreating the result bitmap only if the quantizer can be initialized effortlessly
                    if (Quantizer is not PredefinedColorsQuantizer)
                        return;

                    Palette? palette;
                    Color32 backColor;
                    byte alphaThreshold;
                    using (IQuantizingSession session = Quantizer.Initialize(Source))
                    {
                        palette = session.Palette;
                        backColor = session.BackColor;
                        alphaThreshold = session.AlphaThreshold;
                    }

                    Result = new WriteableBitmap(Source.Width, Source.Height, bitmap.DpiX, bitmap.DpiY, newPixelFormat,
                        GetTargetPalette(newPixelFormat, bitmap, sourcePaletteEntries ?? palette?.GetEntries().Select(c => c.ToMediaColor()).ToArray()));
                    Target = Result.GetWritableBitmapData(backColor.ToMediaColor(), alphaThreshold);
                });
            }

            #endregion

            #region Private Constructors

            private ConversionContext(BitmapSource bitmap, PixelFormat newPixelFormat)
            {
                Bitmap = bitmap;
                NewPixelFormat = newPixelFormat;
            }

            #endregion

            #endregion

            #region Methods

            #region Public Methods

            public void Dispose() => Invoke(false, () =>
            {
                Target?.Dispose();
                Source.Dispose();
            });

            #endregion

            #region Internal Methods

            internal void Invoke(bool synchronized, Action callback)
            {
                if (Bitmap.Dispatcher.CheckAccess())
                {
                    callback.Invoke();
                    return;
                }

                DispatcherOperation operation = Bitmap.Dispatcher.BeginInvoke(DispatcherPriority.Send, callback);
                if (!synchronized)
                    return;

                if (operation.Wait(callbackTimeout) != DispatcherOperationStatus.Completed)
                    throw new InvalidOperationException(Res.BitmapSourceExtensionsDeadlock);
            }

            #endregion

            #region Private Methods

            private void InitDirect(BitmapSource bitmap, Color[]? palette, Color backColor, byte alphaThreshold)
                => Invoke(true, () =>
                {
                    Source = GetSourceBitmapData(bitmap);
                    Result = new WriteableBitmap(bitmap.PixelWidth, bitmap.PixelHeight, bitmap.DpiX, bitmap.DpiY, NewPixelFormat,
                        GetTargetPalette(NewPixelFormat, bitmap, palette));
                    Target = Result.GetWritableBitmapData(backColor, alphaThreshold);
                });

            #endregion

            #endregion
        }

        #endregion

        #region Methods

        #region Public Methods

        #region GetReadableBitmapData

        /// <summary>
        /// Gets a managed read-only accessor for a <see cref="BitmapSource"/> instance.
        /// </summary>
        /// <param name="bitmap">A <see cref="BitmapSource"/> instance, whose data is about to be accessed.</param>
        /// <param name="backColor">For an <see cref="IReadableBitmapData"/> instance the <paramref name="backColor"/> is relevant only for indexed bitmaps
        /// when <see cref="Palette.GetNearestColorIndex">GetNearestColorIndex</see> and <see cref="Palette.GetNearestColor">GetNearestColor</see> methods
        /// are called with an alpha color on the <see cref="IBitmapData.Palette"/> property. Queried colors with alpha, which are considered opaque will be blended
        /// with this color before performing a lookup. The alpha value (<see cref="Color.A">Color.A</see> property) of the specified background color is ignored. This parameter is optional.
        /// <br/>Default value: The bitwise zero instance of <see cref="Color"/>, which has the same RGB values as <see cref="Colors.Black"/>.</param>
        /// <param name="alphaThreshold">Similarly to <paramref name="backColor"/>, for an <see cref="IReadableBitmapData"/> instance the <paramref name="alphaThreshold"/> is relevant
        /// only for indexed bitmaps when <see cref="Palette.GetNearestColorIndex">GetNearestColorIndex</see> and <see cref="Palette.GetNearestColor">GetNearestColor</see> methods
        /// are called with an alpha color on the <see cref="IBitmapData.Palette"/> property. In such case determines the lowest alpha value of a color,
        /// which should not be considered as transparent. If 0, then a color lookup will never return a transparent color. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <returns>An <see cref="IReadableBitmapData"/> instance, which provides fast read-only access to the actual data of the specified <paramref name="bitmap"/>.</returns>
        /// <seealso cref="WriteableBitmapExtensions.GetWritableBitmapData"/>
        /// <seealso cref="WriteableBitmapExtensions.GetReadWriteBitmapData"/>
        [SuppressMessage("VisualStudio.Style", "IDE0039: Use local function instead of lambda", Justification = "False alarm, it would be converted to a delegate anyway.")]
        public static IReadableBitmapData GetReadableBitmapData(this BitmapSource bitmap, Color backColor = default, byte alphaThreshold = 128)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap), PublicResources.ArgumentNull);

            PixelFormat sourceFormat = bitmap.Format;
            KnownPixelFormat knownFormat = sourceFormat.AsKnownPixelFormat();
            var size = new Size(bitmap.PixelWidth, bitmap.PixelHeight);
            Color32 backColor32 = backColor.ToColor32();
            int stride = (size.Width * sourceFormat.BitsPerPixel + 7) >> 3;

            // using an ArraySection as buffer because it can array pooling depending on size and platform
            var buffer = new ArraySection<byte>(size.Height * stride, false);
            Action dispose = () => buffer.Release();

            bitmap.CopyPixels(buffer.UnderlyingArray!, stride, 0);

            // Known pixel formats
            if (knownFormat != KnownPixelFormat.Undefined)
                return knownFormat.IsIndexed()
                    ? BitmapDataFactory.CreateBitmapData(buffer, size, stride, knownFormat, bitmap.GetPalette(backColor, alphaThreshold), IndexedFormatsHelper.TrySetPalette, dispose)
                    : BitmapDataFactory.CreateBitmapData(buffer, size, stride, knownFormat, backColor32, alphaThreshold, dispose);

            // Custom pixel formats
            if (sourceFormat == PixelFormats.Rgb24)
                return BitmapDataFactory.CreateBitmapData(buffer, size, stride, new PixelFormatInfo(24),
                    (row, x) => row.UnsafeGetRefAs<ColorRgb24>(x).ToColor32(),
                    (row, x, c) => row.UnsafeGetRefAs<ColorRgb24>(x) = new ColorRgb24(c.Blend(row.BitmapData.BackColor)),
                    backColor32, alphaThreshold, dispose);

            if (sourceFormat == PixelFormats.Indexed2)
                return BitmapDataFactory.CreateBitmapData(buffer, size, stride, new PixelFormatInfo(2) { Indexed = true },
                    IndexedFormatsHelper.GetColorIndexI2, IndexedFormatsHelper.SetColorIndexI2, bitmap.GetPalette(backColor, alphaThreshold), IndexedFormatsHelper.TrySetPalette, dispose);

            if (sourceFormat == PixelFormats.BlackWhite)
                return BitmapDataFactory.CreateBitmapData(buffer, size, stride, new PixelFormatInfo(1) { Grayscale = true },
                    IndexedFormatsHelper.GetColorBlackWhite, IndexedFormatsHelper.SetColorBlackWhite, backColor32, alphaThreshold, dispose);

            if (sourceFormat == PixelFormats.Gray2)
            {
                Palette colors = Palette.Grayscale4(backColor32);
                return BitmapDataFactory.CreateBitmapData(buffer, size, stride, new PixelFormatInfo(2) { Grayscale = true },
                    (row, x) => colors.GetColor(IndexedFormatsHelper.GetColorIndexI2(row, x)),
                    (row, x, c) => IndexedFormatsHelper.SetColorIndexI2(row, x, colors.GetNearestColorIndex(c)),
                    backColor32, alphaThreshold, dispose);
            }

            if (sourceFormat == PixelFormats.Gray4)
            {
                Palette colors = Palette.Grayscale16(backColor32);
                return BitmapDataFactory.CreateBitmapData(buffer, size, stride, new PixelFormatInfo(4) { Grayscale = true },
                    (row, x) => colors.GetColor(IndexedFormatsHelper.GetColorIndexI4(row, x)),
                    (row, x, c) => IndexedFormatsHelper.SetColorIndexI4(row, x, colors.GetNearestColorIndex(c)),
                    backColor32, alphaThreshold, dispose);
            }

            if (sourceFormat == PixelFormats.Gray8)
                return BitmapDataFactory.CreateBitmapData(buffer, size, stride, new PixelFormatInfo(8) { Grayscale = true },
                    (row, x) => Color32.FromGray(row.UnsafeGetRefAs<byte>(x)),
                    (row, x, c) => row.UnsafeGetRefAs<byte>(x) = c.Blend(row.BitmapData.BackColor).GetBrightness(),
                    backColor32, alphaThreshold, dispose);

            if (sourceFormat == PixelFormats.Gray32Float)
                return BitmapDataFactory.CreateBitmapData(buffer, size, stride, new PixelFormatInfo(32) { Grayscale = true },
                    (row, x) => row.UnsafeGetRefAs<ColorGrayF>(x).ToColor32(),
                    (row, x, c) => row.UnsafeGetRefAs<ColorGrayF>(x) = new ColorGrayF(c.Blend(row.BitmapData.BackColor)),
                    backColor32, alphaThreshold, dispose);

            if (sourceFormat == PixelFormats.Bgr101010)
                return BitmapDataFactory.CreateBitmapData(buffer, size, stride, new PixelFormatInfo(32),
                    (row, x) => row.UnsafeGetRefAs<ColorBgr101010>(x).ToColor32(),
                    (row, x, c) => row.UnsafeGetRefAs<ColorBgr101010>(x) = new ColorBgr101010(c.Blend(row.BitmapData.BackColor)),
                    backColor32, alphaThreshold, dispose);

            if (sourceFormat == PixelFormats.Rgb48)
                return BitmapDataFactory.CreateBitmapData(buffer, size, stride, new PixelFormatInfo(32),
                    (row, x) => row.UnsafeGetRefAs<ColorRgb48>(x).ToColor32(),
                    (row, x, c) => row.UnsafeGetRefAs<ColorRgb48>(x) = new ColorRgb48(c.Blend(row.BitmapData.BackColor)),
                    backColor32, alphaThreshold, dispose);

            if (sourceFormat == PixelFormats.Rgba64)
                return BitmapDataFactory.CreateBitmapData(buffer, size, stride, new PixelFormatInfo(64) { HasAlpha = true },
                    (row, x) => row.UnsafeGetRefAs<ColorRgba64>(x).ToColor32(),
                    (row, x, c) => row.UnsafeGetRefAs<ColorRgba64>(x) = new ColorRgba64(c),
                    backColor32, alphaThreshold, dispose);

            if (sourceFormat == PixelFormats.Prgba64)
                return BitmapDataFactory.CreateBitmapData(buffer, size, stride, new PixelFormatInfo(64) { HasPremultipliedAlpha = true },
                    (row, x) => row.UnsafeGetRefAs<ColorRgba64>(x).ToStraight().ToColor32(),
                    (row, x, c) => row.UnsafeGetRefAs<ColorRgba64>(x) = new ColorRgba64(c).ToPremultiplied(),
                    backColor32, alphaThreshold, dispose);

            if (sourceFormat == PixelFormats.Rgba128Float)
                return BitmapDataFactory.CreateBitmapData(buffer, size, stride, new PixelFormatInfo(128) { HasAlpha = true },
                    (row, x) => row.UnsafeGetRefAs<ColorRgba128>(x).ToColor32(),
                    (row, x, c) => row.UnsafeGetRefAs<ColorRgba128>(x) = new ColorRgba128(c),
                    backColor32, alphaThreshold, dispose);

            if (sourceFormat == PixelFormats.Prgba128Float)
                return BitmapDataFactory.CreateBitmapData(buffer, size, stride, new PixelFormatInfo(128) { HasPremultipliedAlpha = true },
                    (row, x) => row.UnsafeGetRefAs<ColorRgba128>(x).ToStraight().ToColor32(),
                    (row, x, c) => row.UnsafeGetRefAs<ColorRgba128>(x) = new ColorRgba128(c).ToPremultiplied(),
                    backColor32, alphaThreshold, dispose);

            if (sourceFormat == PixelFormats.Rgb128Float)
                return BitmapDataFactory.CreateBitmapData(buffer, size, stride, new PixelFormatInfo(128),
                    (row, x) => row.UnsafeGetRefAs<ColorRgba128>(x).ToColor32().ToOpaque(),
                    (row, x, c) => row.UnsafeGetRefAs<ColorRgba128>(x) = new ColorRgba128(c.Blend(row.BitmapData.BackColor)),
                    backColor32, alphaThreshold, dispose);

            if (sourceFormat == PixelFormats.Cmyk32)
                return BitmapDataFactory.CreateBitmapData(buffer, size, stride, new PixelFormatInfo(32),
                    (row, x) => row.UnsafeGetRefAs<ColorCmyk32>(x).ToColor32(),
                    (row, x, c) => row.UnsafeGetRefAs<ColorCmyk32>(x) = new ColorCmyk32(c.Blend(row.BitmapData.BackColor)),
                    backColor32, alphaThreshold, dispose);

            throw new InvalidOperationException(Res.InternalError($"Unexpected PixelFormat {sourceFormat}"));
        }

        #endregion

        #region ConvertPixelFormat

        #region Sync

        public static WriteableBitmap ConvertPixelFormat(this BitmapSource bitmap, PixelFormat newPixelFormat, Color backColor = default, byte alphaThreshold = 128)
            => ConvertPixelFormat(bitmap, newPixelFormat, null, backColor, alphaThreshold);

        public static WriteableBitmap ConvertPixelFormat(this BitmapSource bitmap, PixelFormat newPixelFormat, Color[]? palette, Color backColor = default, byte alphaThreshold = 128)
        {
            ValidateConvertPixelFormat(bitmap, newPixelFormat);
            return DoConvertPixelFormatDirect(AsyncHelper.DefaultContext, new ConversionContext(bitmap, newPixelFormat, palette, backColor, alphaThreshold))!;
        }

        public static WriteableBitmap ConvertPixelFormat(this BitmapSource bitmap, PixelFormat newPixelFormat, IQuantizer? quantizer, IDitherer? ditherer = null)
        {
            ValidateConvertPixelFormat(bitmap, newPixelFormat);
            var context = new ConversionContext(bitmap, newPixelFormat, quantizer, ditherer);
            return context.Quantizer == null
                ? DoConvertPixelFormatDirect(AsyncHelper.DefaultContext, context)!
                : DoConvertPixelFormatWithQuantizer(AsyncHelper.DefaultContext, context)!;
        }

        #endregion

        #region Async APM

        public static IAsyncResult BeginConvertPixelFormat(this BitmapSource bitmap, PixelFormat newPixelFormat, Color[]? palette, Color backColor = default, byte alphaThreshold = 128, AsyncConfig? asyncConfig = null)
        {
            ValidateConvertPixelFormat(bitmap, newPixelFormat);
            var context = new ConversionContext(bitmap, newPixelFormat, palette, backColor, alphaThreshold);
            return AsyncHelper.BeginOperation(ctx => DoConvertPixelFormatDirect(ctx, context), asyncConfig);
        }

        public static IAsyncResult BeginConvertPixelFormat(this BitmapSource bitmap, PixelFormat newPixelFormat, Color backColor = default, byte alphaThreshold = 128, AsyncConfig? asyncConfig = null)
            => BeginConvertPixelFormat(bitmap, newPixelFormat, null, backColor, alphaThreshold, asyncConfig);

        public static IAsyncResult BeginConvertPixelFormat(this BitmapSource bitmap, PixelFormat newPixelFormat, IQuantizer? quantizer, IDitherer? ditherer = null, AsyncConfig? asyncConfig = null)
        {
            ValidateConvertPixelFormat(bitmap, newPixelFormat);
            var context = new ConversionContext(bitmap, newPixelFormat, quantizer, ditherer);
            return context.Quantizer == null
                ? AsyncHelper.BeginOperation(ctx => DoConvertPixelFormatDirect(ctx, context), asyncConfig)
                : AsyncHelper.BeginOperation(ctx => DoConvertPixelFormatWithQuantizer(ctx, context), asyncConfig);
        }

        public static WriteableBitmap? EndConvertPixelFormat(this IAsyncResult asyncResult) => AsyncHelper.EndOperation<WriteableBitmap?>(asyncResult, nameof(BeginConvertPixelFormat));

        #endregion

        #region Async TAP
#if !NET35

        public static Task<WriteableBitmap?> ConvertPixelFormatAsync(this BitmapSource bitmap, PixelFormat newPixelFormat, Color[]? palette, Color backColor = default, byte alphaThreshold = 128, TaskConfig? asyncConfig = null)
        {
            ValidateConvertPixelFormat(bitmap, newPixelFormat);
            var context = new ConversionContext(bitmap, newPixelFormat, palette, backColor, alphaThreshold);
            return AsyncHelper.DoOperationAsync(ctx => DoConvertPixelFormatDirect(ctx, context), asyncConfig);
        }

        public static Task<WriteableBitmap?> ConvertPixelFormatAsync(this BitmapSource bitmap, PixelFormat newPixelFormat, Color backColor = default, byte alphaThreshold = 128, TaskConfig? asyncConfig = null)
            => ConvertPixelFormatAsync(bitmap, newPixelFormat, null, backColor, alphaThreshold, asyncConfig);

        public static Task<WriteableBitmap?> ConvertPixelFormatAsync(this BitmapSource bitmap, PixelFormat newPixelFormat, IQuantizer? quantizer, IDitherer? ditherer = null, TaskConfig? asyncConfig = null)
        {
            ValidateConvertPixelFormat(bitmap, newPixelFormat);
            var context = new ConversionContext(bitmap, newPixelFormat, quantizer, ditherer);
            return context.Quantizer == null
                ? AsyncHelper.DoOperationAsync(ctx => DoConvertPixelFormatDirect(ctx, context), asyncConfig)
                : AsyncHelper.DoOperationAsync(ctx => DoConvertPixelFormatWithQuantizer(ctx, context), asyncConfig);
        }

#endif
        #endregion

        #endregion

        #endregion

        #region Internal Methods

        internal static Palette? GetPalette(this BitmapSource bitmap, Color backColor, byte alphaThreshold)
        {
            BitmapPalette? palette = bitmap.Palette;
            return palette == null ? null : new Palette(palette.Colors.Select(c => c.ToColor32()).ToArray(), backColor.ToColor32(), alphaThreshold);
        }

        #endregion

        #region Private Methods

        private static void ValidateConvertPixelFormat(BitmapSource bitmap, PixelFormat newPixelFormat)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap), PublicResources.ArgumentNull);
            if (newPixelFormat == default)
                throw new ArgumentOutOfRangeException(nameof(newPixelFormat), PublicResources.ArgumentOutOfRange);
        }

        private static IReadableBitmapData GetSourceBitmapData(BitmapSource source)
            => source is WriteableBitmap { IsFrozen: false } wb
                ? wb.GetBitmapDataInternal(true)
                : source.GetReadableBitmapData();

        private static WriteableBitmap? DoConvertPixelFormatDirect(IAsyncContext asyncContext, ConversionContext conversionContext)
        {
            using (conversionContext)
            {
                if (asyncContext.IsCancellationRequested)
                    return null;

                IReadableBitmapData source = conversionContext.Source;
                source.CopyTo(conversionContext.Target!, asyncContext, new Rectangle(0, 0, source.Width, source.Height), Point.Empty);
                return asyncContext.IsCancellationRequested ? null : conversionContext.Result;
            }
        }

        private static WriteableBitmap? DoConvertPixelFormatWithQuantizer(IAsyncContext asyncContext, ConversionContext conversionContext)
        {
            using (conversionContext)
            {
                if (asyncContext.IsCancellationRequested)
                    return null;

                IReadableBitmapData source = conversionContext.Source;
                IQuantizer quantizer = conversionContext.Quantizer!;

                // we might have an uninitialized result if the quantizer is not a predefined one
                if (conversionContext.Result == null)
                {
                    Palette? palette;
                    Color32 backColor;
                    byte alphaThreshold;
                    using (IQuantizingSession quantizingSession = quantizer.Initialize(source, asyncContext))
                    {
                        if (asyncContext.IsCancellationRequested)
                            return null;
                        if (quantizingSession == null)
                            throw new InvalidOperationException(Res.BitmapSourceExtensionsQuantizerInitializeNull);

                        palette = quantizingSession.Palette;
                        backColor = quantizingSession.BackColor;
                        alphaThreshold = quantizingSession.AlphaThreshold;
                    }

                    conversionContext.Invoke(true, () =>
                    {
                        conversionContext.Result = new WriteableBitmap(source.Width, source.Height,
                            conversionContext.Bitmap.DpiX, conversionContext.Bitmap.DpiX, conversionContext.NewPixelFormat,
                            GetTargetPalette(conversionContext.NewPixelFormat, conversionContext.Bitmap, palette?.GetEntries().Select(c => c.ToMediaColor()).ToArray()));
                        conversionContext.Target = conversionContext.Result.GetWritableBitmapData(backColor.ToMediaColor(), alphaThreshold);
                    });

                    // We have a palette from a potentially expensive quantizer: creating a predefined quantizer from the already generated palette to avoid generating it again.
                    if (palette != null && quantizer is not PredefinedColorsQuantizer)
                        quantizer = PredefinedColorsQuantizer.FromCustomPalette(palette);
                }

                if (asyncContext.IsCancellationRequested)
                    return null;

                source.CopyTo(conversionContext.Target!, asyncContext, new Rectangle(0, 0, source.Width, source.Height), Point.Empty, quantizer, conversionContext.Ditherer);
                return asyncContext.IsCancellationRequested ? null : conversionContext.Result;
            }
        }

        private static BitmapPalette? GetTargetPalette(PixelFormat newPixelFormat, BitmapSource source, IList<Color>? palette)
        {
            if (!newPixelFormat.IsIndexed())
                return null;

            int bpp = newPixelFormat.BitsPerPixel;

            // if no desired colors are specified but converting to a higher bpp indexed image, then taking the source palette
            if (palette == null && source.Format.BitsPerPixel <= bpp)
                return source.Palette;

            if (palette == null || palette.Count == 0)
                return newPixelFormat.GetDefaultPalette();

            // there is a desired palette to apply
            int maxColors = 1 << bpp;
            if (palette.Count > maxColors)
                throw new ArgumentException(Res.BitmapSourceExtensionsPaletteTooLarge(maxColors, bpp), nameof(palette));

            return new BitmapPalette(palette);
        }

        #endregion

        #endregion
    }
}
