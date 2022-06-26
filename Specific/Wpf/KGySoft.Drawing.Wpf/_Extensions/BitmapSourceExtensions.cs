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
using System.Windows.Media;
using System.Windows.Media.Imaging;

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
        #region Methods

        #region Public Methods

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

        public static WriteableBitmap ConvertPixelFormat(this BitmapSource bitmap, PixelFormat newPixelFormat, Color backColor = default, byte alphaThreshold = 128)
            => ConvertPixelFormat(bitmap, newPixelFormat, null, backColor, alphaThreshold);

        public static WriteableBitmap ConvertPixelFormat(this BitmapSource bitmap, PixelFormat newPixelFormat, Color[]? palette, Color backColor = default, byte alphaThreshold = 128)
        {
            ValidateConvertPixelFormat(bitmap, newPixelFormat);
            return DoConvertPixelFormat(AsyncHelper.DefaultContext, bitmap, newPixelFormat, palette, backColor, alphaThreshold)!;
        }

        public static WriteableBitmap ConvertPixelFormat(this BitmapSource bitmap, PixelFormat newPixelFormat, IQuantizer? quantizer, IDitherer? ditherer = null)
        {
            ValidateConvertPixelFormat(bitmap, newPixelFormat);
            return DoConvertPixelFormat(AsyncHelper.DefaultContext, bitmap, newPixelFormat, quantizer, ditherer)!;
        }

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

        private static WriteableBitmap? DoConvertPixelFormat(IAsyncContext context, BitmapSource bitmap, PixelFormat newPixelFormat, Color[]? palette, Color backColor, byte alphaThreshold)
        {
            if (context.IsCancellationRequested)
                return null;

            var result = new WriteableBitmap(bitmap.PixelWidth, bitmap.PixelHeight, bitmap.DpiX, bitmap.DpiY, newPixelFormat,
                GetTargetPalette(newPixelFormat, bitmap, palette));

            using (IReadableBitmapData source = bitmap.GetReadableBitmapData())
            using (IWritableBitmapData target = result.GetWritableBitmapData(backColor, alphaThreshold))
                source.CopyTo(target, context, new Rectangle(0, 0, source.Width, source.Height), Point.Empty);

            return context.IsCancellationRequested ? null : result;
        }

        private static WriteableBitmap? DoConvertPixelFormat(IAsyncContext context, BitmapSource bitmap, PixelFormat newPixelFormat, IQuantizer? quantizer, IDitherer? ditherer)
        {
            if (context.IsCancellationRequested)
                return null;

            IList<Color>? paletteEntries = null;
            if (quantizer == null)
            {
                // converting without using a quantizer (even if only a ditherer is specified for a high-bpp pixel format)
                if (ditherer == null || !newPixelFormat.CanBeDithered())
                    return DoConvertPixelFormat(context, bitmap, newPixelFormat, null, default, 128);

                // here we need to pick a quantizer for the dithering
                int bpp = newPixelFormat.BitsPerPixel;

                paletteEntries = bitmap.Palette?.Colors ?? Reflector.EmptyArray<Color>();
                if (bpp <= 8 && paletteEntries.Count > 0 && paletteEntries.Count <= (1 << bpp))
                    quantizer = PredefinedColorsQuantizer.FromCustomPalette(new Palette(paletteEntries.Select(c => c.ToColor32()).ToArray()));
                else
                {
                    quantizer = newPixelFormat.GetDefaultQuantizer();
                    paletteEntries = null;
                }
            }

            if (context.IsCancellationRequested)
                return null;

            using IReadableBitmapData source = bitmap.GetReadableBitmapData();

            // We explicitly initialize the quantizer just to determine the palette colors for the result Bitmap.
            context.Progress?.New(DrawingOperation.InitializingQuantizer);
            Palette? palette = null;
            Color32 backColor;
            byte alphaThreshold;
            WriteableBitmap result;
            using (IQuantizingSession quantizingSession = quantizer.Initialize(source, context))
            {
                if (context.IsCancellationRequested)
                    return null;
                if (quantizingSession == null)
                    throw new InvalidOperationException(Res.BitmapSourceExtensionsQuantizerInitializeNull);

                result = new WriteableBitmap(bitmap.PixelWidth, bitmap.PixelHeight, bitmap.DpiX, bitmap.DpiY, newPixelFormat,
                    GetTargetPalette(newPixelFormat, bitmap, paletteEntries ?? (palette = quantizingSession.Palette)?.GetEntries().Select(c => c.ToMediaColor()).ToArray()));

                backColor = quantizingSession.BackColor;
                alphaThreshold = quantizingSession.AlphaThreshold;
            }

            if (context.IsCancellationRequested)
                return null;

            // We have a palette from a potentially expensive quantizer: creating a predefined quantizer from the already generated palette to avoid generating it again.
            if (palette != null && quantizer is not PredefinedColorsQuantizer)
                quantizer = PredefinedColorsQuantizer.FromCustomPalette(palette);

            // Note: palette is purposely not passed here so a new instance will be created from the colors, without any possible delegate (which is still used by the quantizer).
            using (IWritableBitmapData target = result.GetWritableBitmapData(backColor.ToMediaColor(), alphaThreshold))
                source.CopyTo(target, context, new Rectangle(0, 0, source.Width, source.Height), Point.Empty, quantizer, ditherer);

            return context.IsCancellationRequested ? null : result;
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
