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
            internal IQuantizer? Quantizer { get; }
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

        /// <summary>
        /// Converts the specified <paramref name="bitmap"/> to a <see cref="WriteableBitmap"/> of the desired <see cref="PixelFormat"/>.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="bitmap">The original bitmap to convert.</param>
        /// <param name="newPixelFormat">The desired new pixel format.</param>
        /// <param name="backColor">If <paramref name="newPixelFormat"/> does not have alpha, then specifies the color of the background.
        /// Source pixels with alpha, which will be opaque in the result will be blended with this color.
        /// The <see cref="Color.A">Color.A</see> property of the background color is ignored. This parameter is optional.
        /// <br/>Default value: The bitwise zero instance of <see cref="Color"/>, which has the same RGB values as <see cref="Colors.Black"/>.</param>
        /// <param name="alphaThreshold">If <paramref name="newPixelFormat"/> is an indexed format and the target palette contains a transparent color,
        /// then specifies a threshold value for the <see cref="Color.A">Color.A</see> property, under which the color is considered transparent. If 0,
        /// then the result will not have transparent pixels. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <returns>A new <see cref="WriteableBitmap"/> instance with the desired pixel format.</returns>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. Use the <see cref="BeginConvertPixelFormat(BitmapSource, PixelFormat, Color, byte, AsyncConfig)"/>
        /// or <see cref="ConvertPixelFormatAsync(BitmapSource, PixelFormat, Color, byte, TaskConfig)"/> (in .NET Framework 4.0 and above) methods for asynchronous call and to adjust parallelization, set up cancellation and for reporting progress.</note>
        /// <para>If <paramref name="newPixelFormat"/> is an indexed format, then this overload will either use the palette of the source <paramref name="bitmap"/> if applicable,
        /// or a default palette. To apply a custom palette use the of the <see cref="ConvertPixelFormat(BitmapSource,PixelFormat,Color[],Color,byte)"/> overload.</para>
        /// <para>If <paramref name="newPixelFormat"/> can represent fewer colors than the source format, then a default
        /// quantization will occur during the conversion. To use a specific quantizer (and optionally a ditherer) use the <see cref="ConvertPixelFormat(BitmapSource,PixelFormat,IQuantizer,IDitherer)"/> overload.
        /// To use a quantizer with a specific palette you can use the <see cref="PredefinedColorsQuantizer"/> class.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmap"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="newPixelFormat"/> does not specify a valid format.</exception>
        /// <seealso cref="ConvertPixelFormat(BitmapSource, PixelFormat, IQuantizer, IDitherer)"/>
        /// <seealso cref="BitmapDataExtensions.Clone(IReadableBitmapData, KnownPixelFormat, Color32, byte)"/>
        public static WriteableBitmap ConvertPixelFormat(this BitmapSource bitmap, PixelFormat newPixelFormat, Color backColor = default, byte alphaThreshold = 128)
            => ConvertPixelFormat(bitmap, newPixelFormat, null, backColor, alphaThreshold);

        /// <summary>
        /// Converts the specified <paramref name="bitmap"/> to a <see cref="WriteableBitmap"/> of the desired <see cref="PixelFormat"/>.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="bitmap">The original bitmap to convert.</param>
        /// <param name="newPixelFormat">The desired new pixel format.</param>
        /// <param name="palette">The desired target palette if <paramref name="newPixelFormat"/> is an indexed format. If <see langword="null"/>,
        /// then the source palette is taken from the source image if it also has a palette of no more entries than the target indexed format can have;
        /// otherwise, a default palette will be used based on <paramref name="newPixelFormat"/>.</param>
        /// <param name="backColor">If <paramref name="newPixelFormat"/> does not support alpha, then specifies the color of the background.
        /// Source pixels with alpha, which will be opaque in the result will be blended with this color.
        /// The <see cref="Color.A">Color.A</see> property of the background color is ignored. This parameter is optional.
        /// <br/>Default value: The bitwise zero instance of <see cref="Color"/>, which has the same RGB values as <see cref="Colors.Black"/>.</param>
        /// <param name="alphaThreshold">If <paramref name="newPixelFormat"/> is an indexed format and the target palette contains a transparent color,
        /// then specifies a threshold value for the <see cref="Color.A">Color.A</see> property, under which the color is considered transparent. If 0,
        /// then the result will not have transparent pixels. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <returns>A new <see cref="WriteableBitmap"/> instance with the desired pixel format.</returns>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. Use the <see cref="BeginConvertPixelFormat(BitmapSource, PixelFormat, Color[], Color, byte, AsyncConfig)"/>
        /// or <see cref="ConvertPixelFormatAsync(BitmapSource, PixelFormat, Color[], Color, byte, TaskConfig)"/> (in .NET Framework 4.0 and above) methods for asynchronous call and to adjust parallelization, set up cancellation and for reporting progress.</note>
        /// <para>If <paramref name="newPixelFormat"/> can represent fewer colors than the source format, then a default
        /// quantization will occur during the conversion. To use a specific quantizer (and optionally a ditherer) use the <see cref="ConvertPixelFormat(BitmapSource,PixelFormat,IQuantizer,IDitherer)"/> overload.
        /// To use a quantizer with a specific palette you can use the <see cref="PredefinedColorsQuantizer"/> class.</para>
        /// <para>If <paramref name="newPixelFormat"/> is <see cref="PixelFormats.Indexed8"/>, <paramref name="bitmap"/> has no palette and <paramref name="palette"/> is <see langword="null"/>, then a default 256 color palette will be used containing
        /// the 16 standard <a href="https://www.w3.org/TR/REC-html40/types.html#h-6.5" target="_blank">basic sRGB colors</a>, the "web-safe" palette of 216 colors as well as 24 transparent entries.</para>
        /// <para>If <paramref name="newPixelFormat"/> is <see cref="PixelFormats.Indexed4"/>, <paramref name="bitmap"/> has no palette and <paramref name="palette"/> is <see langword="null"/>,
        /// then the 16 standard <a href="https://www.w3.org/TR/REC-html40/types.html#h-6.5" target="_blank">basic sRGB colors</a> will be used.</para>
        /// <para>If <paramref name="newPixelFormat"/> is <see cref="PixelFormats.Indexed2"/>, <paramref name="bitmap"/> has no palette and <paramref name="palette"/> is <see langword="null"/>, then
        /// the palette will consist of 4 grayscale colors, containing black, white and the two gray entries that present in the default 4-bit palette.</para>
        /// <para>If <paramref name="newPixelFormat"/> is <see cref="PixelFormats.Indexed1"/>, <paramref name="bitmap"/> has no palette and <paramref name="palette"/> is <see langword="null"/>, then black and white colors will be used.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmap"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="newPixelFormat"/> does not specify a valid format.</exception>
        /// <exception cref="ArgumentException"><paramref name="palette"/> contains too many colors for the indexed format specified by <paramref name="newPixelFormat"/>.</exception>
        /// <seealso cref="ConvertPixelFormat(BitmapSource, PixelFormat, IQuantizer, IDitherer)"/>
        /// <seealso cref="BitmapDataExtensions.Clone(IReadableBitmapData, KnownPixelFormat, Palette)"/>
        public static WriteableBitmap ConvertPixelFormat(this BitmapSource bitmap, PixelFormat newPixelFormat, Color[]? palette, Color backColor = default, byte alphaThreshold = 128)
        {
            ValidateConvertPixelFormat(bitmap, newPixelFormat);
            return DoConvertPixelFormatDirect(AsyncHelper.DefaultContext, new ConversionContext(bitmap, newPixelFormat, palette, backColor, alphaThreshold))!;
        }

        /// <summary>
        /// Converts the specified <paramref name="bitmap"/> to a <see cref="WriteableBitmap"/> with the desired <see cref="PixelFormat"/>.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="bitmap">The original bitmap to convert.</param>
        /// <param name="newPixelFormat">The desired new pixel format.</param>
        /// <param name="quantizer">An optional <see cref="IQuantizer"/> instance to determine the colors if the result.
        /// If <see langword="null"/>&#160;and <paramref name="newPixelFormat"/> is an indexed format, then a default palette and quantization logic will be used.</param>
        /// <param name="ditherer">The ditherer to be used. Might be ignored if <paramref name="quantizer"/> is not specified
        /// and <paramref name="newPixelFormat"/> represents an at least 24 bits-per-pixel size. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A new <see cref="WriteableBitmap"/> instance with the desired pixel format.</returns>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. Use the <see cref="BeginConvertPixelFormat(BitmapSource, PixelFormat, IQuantizer, IDitherer, AsyncConfig)"/>
        /// or <see cref="ConvertPixelFormatAsync(BitmapSource, PixelFormat, IQuantizer, IDitherer, TaskConfig)"/> (in .NET Framework 4.0 and above) methods for asynchronous call and to adjust parallelization, set up cancellation and for reporting progress.</note>
        /// <para>An unmatching <paramref name="quantizer"/> and <paramref name="newPixelFormat"/> may cause undesired results.</para>
        /// <para>The <paramref name="ditherer"/> may have no effect if the <paramref name="quantizer"/> uses too many colors.</para>
        /// <para>To produce a result with up to 256 colors best optimized for the source <paramref name="bitmap"/> you can use the <see cref="OptimizedPaletteQuantizer"/> class.</para>
        /// <para>To quantize a <see cref="WriteableBitmap"/> in place, without changing the pixel format you can use the <see cref="BitmapDataExtensions.Quantize(IReadWriteBitmapData, IQuantizer)">BitmapDataExtensions.Quantize</see> method.
        /// You can use the <see cref="WriteableBitmapExtensions.GetReadWriteBitmapData">GetReadWriteBitmapData</see> extension method to obtain an <see cref="IReadWriteBitmapData"/> for a <see cref="WriteableBitmap"/>.</para>
        /// <para>To dither a <see cref="WriteableBitmap"/> in place, without changing the pixel format you can use the <see cref="BitmapDataExtensions.Dither(IReadWriteBitmapData, IQuantizer, IDitherer)">BitmapDataExtensions.Dither</see> method.
        /// You can use the <see cref="WriteableBitmapExtensions.GetReadWriteBitmapData">GetReadWriteBitmapData</see> extension method to obtain an <see cref="IReadWriteBitmapData"/> for a <see cref="WriteableBitmap"/>.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmap"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="newPixelFormat"/> does not specify a valid format.</exception>
        /// <exception cref="ArgumentException">The <paramref name="quantizer"/> palette contains too many colors for the indexed format specified by <paramref name="newPixelFormat"/>.</exception>
        /// <exception cref="PlatformNotSupportedException"><paramref name="newPixelFormat"/> is not supported on the current platform.</exception>
        /// <seealso cref="IQuantizer"/>
        /// <seealso cref="IDitherer"/>
        /// <seealso cref="BitmapDataExtensions.Quantize(IReadWriteBitmapData, IQuantizer)"/>
        /// <seealso cref="BitmapDataExtensions.Dither(IReadWriteBitmapData, IQuantizer, IDitherer)"/>
        /// <seealso cref="BitmapDataExtensions.Clone(IReadableBitmapData, KnownPixelFormat, IQuantizer, IDitherer)"/>
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

        /// <summary>
        /// Begins to convert the specified <paramref name="bitmap"/> to a <see cref="WriteableBitmap"/> of the desired <see cref="PixelFormat"/> asynchronously.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="bitmap">The original image to convert.</param>
        /// <param name="newPixelFormat">The desired new pixel format.</param>
        /// <param name="backColor">If <paramref name="newPixelFormat"/> does not support alpha or supports only single-bit alpha, then specifies the color of the background.
        /// Source pixels with alpha, which will be opaque in the result will be blended with this color.
        /// The <see cref="Color.A">Color.A</see> property of the background color is ignored. This parameter is optional.
        /// <br/>Default value: The bitwise zero instance of <see cref="Color"/>, which has the same RGB values as <see cref="Colors.Black"/>.</param>
        /// <param name="alphaThreshold">If <paramref name="newPixelFormat"/> is an indexed format and the target palette contains a transparent color,
        /// then specifies a threshold value for the <see cref="Color.A">Color.A</see> property, under which the color is considered transparent. If 0,
        /// then the result will not have transparent pixels. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/?topic=html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm" target="_blank">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/?topic=html/T_KGySoft_Threading_IAsyncProgress.htm" target="_blank">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="ConvertPixelFormatAsync(BitmapSource, PixelFormat, Color, byte, TaskConfig)"/> method.</para>
        /// <para>To get the result or the exception that occurred during the operation you have to call the <see cref="EndConvertPixelFormat">EndConvertPixelFormat</see> method.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/?topic=html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm" target="_blank">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="caution">This method can be called from any thread but if it is called from a different one from the thread of the source <paramref name="bitmap"/>'s dispatcher,
        /// then the result <see cref="WriteableBitmap"/> will be created by using a synchronized callback. To avoid possible deadlocks, the thread of the source <paramref name="bitmap"/>
        /// must not be blocked and the dispatcher must run. The result will be usable in the same thread as the source <paramref name="bitmap"/>.
        /// To avoid blocking waiting for the result set the <a href="https://docs.kgysoft.net/corelibraries/?topic=html/P_KGySoft_Threading_AsyncConfig_CompletedCallback.htm" target="_blank">CompletedCallback</a>
        /// parameter of the <paramref name="asyncConfig"/> parameter and call the <see cref="EndConvertPixelFormat">EndConvertPixelFormat</see> method from there.</note>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="ConvertPixelFormat(BitmapSource, PixelFormat, Color, byte)"/> method for more details and image examples.</note>
        /// </remarks>
        public static IAsyncResult BeginConvertPixelFormat(this BitmapSource bitmap, PixelFormat newPixelFormat, Color backColor = default, byte alphaThreshold = 128, AsyncConfig? asyncConfig = null)
            => BeginConvertPixelFormat(bitmap, newPixelFormat, null, backColor, alphaThreshold, asyncConfig);

        /// <summary>
        /// Begins to convert the specified <paramref name="bitmap"/> to a <see cref="WriteableBitmap"/> of the desired <see cref="PixelFormat"/> asynchronously.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="bitmap">The original image to convert.</param>
        /// <param name="newPixelFormat">The desired new pixel format.</param>
        /// <param name="palette">The desired target palette if <paramref name="newPixelFormat"/> is an indexed format. If <see langword="null"/>,
        /// then the source palette is taken from the source image if it also has a palette of no more entries than the target indexed format can have;
        /// otherwise, a default palette will be used based on <paramref name="newPixelFormat"/>.</param>
        /// <param name="backColor">If <paramref name="newPixelFormat"/> does not support alpha or supports only single-bit alpha, then specifies the color of the background.
        /// Source pixels with alpha, which will be opaque in the result will be blended with this color.
        /// The <see cref="Color.A">Color.A</see> property of the background color is ignored. This parameter is optional.
        /// <br/>Default value: The bitwise zero instance of <see cref="Color"/>, which has the same RGB values as <see cref="Colors.Black"/>.</param>
        /// <param name="alphaThreshold">If <paramref name="newPixelFormat"/> is an indexed format and the target palette contains a transparent color,
        /// then specifies a threshold value for the <see cref="Color.A">Color.A</see> property, under which the color is considered transparent. If 0,
        /// then the result will not have transparent pixels. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/?topic=html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm" target="_blank">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/?topic=html/T_KGySoft_Threading_IAsyncProgress.htm" target="_blank">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="ConvertPixelFormatAsync(BitmapSource, PixelFormat, Color[], Color, byte, TaskConfig)"/> method.</para>
        /// <para>To get the result or the exception that occurred during the operation you have to call the <see cref="EndConvertPixelFormat">EndConvertPixelFormat</see> method.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/?topic=html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm" target="_blank">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="caution">This method can be called from any thread but if it is called from a different one from the thread of the source <paramref name="bitmap"/>'s dispatcher,
        /// then the result <see cref="WriteableBitmap"/> will be created by using a synchronized callback. To avoid possible deadlocks, the thread of the source <paramref name="bitmap"/>
        /// must not be blocked and the dispatcher must run. The result will be usable in the same thread as the source <paramref name="bitmap"/>.
        /// To avoid blocking waiting for the result set the <a href="https://docs.kgysoft.net/corelibraries/?topic=html/P_KGySoft_Threading_AsyncConfig_CompletedCallback.htm" target="_blank">CompletedCallback</a>
        /// parameter of the <paramref name="asyncConfig"/> parameter and call the <see cref="EndConvertPixelFormat">EndConvertPixelFormat</see> method from there.</note>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="ConvertPixelFormat(BitmapSource, PixelFormat, Color[], Color, byte)"/> method for more details and image examples.</note>
        /// </remarks>
        public static IAsyncResult BeginConvertPixelFormat(this BitmapSource bitmap, PixelFormat newPixelFormat, Color[]? palette, Color backColor = default, byte alphaThreshold = 128, AsyncConfig? asyncConfig = null)
        {
            ValidateConvertPixelFormat(bitmap, newPixelFormat);
            var context = new ConversionContext(bitmap, newPixelFormat, palette, backColor, alphaThreshold);
            return AsyncHelper.BeginOperation(ctx => DoConvertPixelFormatDirect(ctx, context), asyncConfig);
        }

        /// <summary>
        /// Begins to convert the specified <paramref name="bitmap"/> to a <see cref="WriteableBitmap"/> of the desired <see cref="PixelFormat"/> asynchronously.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="bitmap">The original image to convert.</param>
        /// <param name="newPixelFormat">The desired new pixel format.</param>
        /// <param name="quantizer">An optional <see cref="IQuantizer"/> instance to determine the colors if the result.
        /// If <see langword="null"/>&#160;and <paramref name="newPixelFormat"/> is an indexed format, then a default palette and quantization logic will be used.</param>
        /// <param name="ditherer">The ditherer to be used. Might be ignored if <paramref name="quantizer"/> is not specified
        /// and <paramref name="newPixelFormat"/> represents an at least 24 bits-per-pixel size. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/?topic=html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm" target="_blank">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/?topic=html/T_KGySoft_Threading_IAsyncProgress.htm" target="_blank">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="ConvertPixelFormatAsync(BitmapSource, PixelFormat, IQuantizer, IDitherer, TaskConfig)"/> method.</para>
        /// <para>To get the result or the exception that occurred during the operation you have to call the <see cref="EndConvertPixelFormat">EndConvertPixelFormat</see> method.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/?topic=html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm" target="_blank">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="caution"><list type="bullet"><item>This method can be called from any thread but if it is called from a different one from the thread of the source <paramref name="bitmap"/>'s dispatcher,
        /// then the result <see cref="WriteableBitmap"/> will be created by using a synchronized callback. To avoid possible deadlocks, the thread of the source <paramref name="bitmap"/>
        /// must not be blocked and the dispatcher must run. The result will be usable in the same thread as the source <paramref name="bitmap"/>.
        /// To avoid blocking waiting for the result set the <a href="https://docs.kgysoft.net/corelibraries/?topic=html/P_KGySoft_Threading_AsyncConfig_CompletedCallback.htm" target="_blank">CompletedCallback</a>
        /// parameter of the <paramref name="asyncConfig"/> parameter and call the <see cref="EndConvertPixelFormat">EndConvertPixelFormat</see> method from there.</item>
        /// <item>If <paramref name="quantizer"/> is not a <see cref="PredefinedColorsQuantizer"/>, then the result <see cref="WriteableBitmap"/> is created by a synchronized callback
        /// on the thread of the source <paramref name="bitmap"/> even if you call this method from the same thread. Do not block the thread of the source <paramref name="bitmap"/>;
        /// otherwise, a deadlock may occur.</item></list></note>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="ConvertPixelFormat(BitmapSource, PixelFormat, IQuantizer, IDitherer)"/> method for more details and image examples.</note>
        /// </remarks>
        public static IAsyncResult BeginConvertPixelFormat(this BitmapSource bitmap, PixelFormat newPixelFormat, IQuantizer? quantizer, IDitherer? ditherer = null, AsyncConfig? asyncConfig = null)
        {
            ValidateConvertPixelFormat(bitmap, newPixelFormat);
            var context = new ConversionContext(bitmap, newPixelFormat, quantizer, ditherer);
            return context.Quantizer == null
                ? AsyncHelper.BeginOperation(ctx => DoConvertPixelFormatDirect(ctx, context), asyncConfig)
                : AsyncHelper.BeginOperation(ctx => DoConvertPixelFormatWithQuantizer(ctx, context), asyncConfig);
        }

        /// <summary>
        /// Waits for the pending asynchronous operation started by the <see cref="O:KGySoft.Drawing.Wpf.BitmapSourceExtensions.BeginConvertPixelFormat">BeginConvertPixelFormat</see> methods to complete.
        /// In .NET Framework 4.0 and above you can use the <see cref="O:KGySoft.Drawing.Wpf.BitmapSourceExtensions.ConvertPixelFormatAsync">ConvertPixelFormatAsync</see> methods instead.
        /// To avoid blocking the source thread it is recommended to call this method from the <a href="https://docs.kgysoft.net/corelibraries/?topic=html/P_KGySoft_Threading_AsyncConfig_CompletedCallback.htm" target="_blank">CompletedCallback</a>
        /// delegate of the <c>asyncConfig</c> parameter of the <see cref="O:KGySoft.Drawing.Wpf.BitmapSourceExtensions.BeginConvertPixelFormat">BeginConvertPixelFormat</see> methods.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="BeginConvertPixelFormat(BitmapSource, PixelFormat, IQuantizer?, IDitherer?, AsyncConfig?)"/> method for details.
        /// </summary>
        /// <param name="asyncResult">The reference to the pending asynchronous request to finish.</param>
        /// <returns>A <see cref="WriteableBitmap"/> instance that is the result of the operation,
        /// or <see langword="null"/>, if the operation was canceled and the <a href="https://docs.kgysoft.net/corelibraries/?topic=html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm" target="_blank">ThrowIfCanceled</a>property of the <c>asyncConfig</c> parameter was <see langword="false"/>.</returns>
        public static WriteableBitmap? EndConvertPixelFormat(this IAsyncResult asyncResult) => AsyncHelper.EndOperation<WriteableBitmap?>(asyncResult, nameof(BeginConvertPixelFormat));

        #endregion

        #region Async TAP
#if !NET35

        /// <summary>
        /// Converts the specified <paramref name="bitmap"/> to a <see cref="WriteableBitmap"/> of the desired <see cref="PixelFormat"/> asynchronously.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="bitmap">The original bitmap to convert.</param>
        /// <param name="newPixelFormat">The desired new pixel format.</param>
        /// <param name="backColor">If <paramref name="newPixelFormat"/> does not support alpha or supports only single-bit alpha, then specifies the color of the background.
        /// Source pixels with alpha, which will be opaque in the result will be blended with this color.
        /// The <see cref="Color.A">Color.A</see> property of the background color is ignored. This parameter is optional.
        /// <br/>Default value: The bitwise zero instance of <see cref="Color"/>, which has the same RGB values as <see cref="Colors.Black"/>.</param>
        /// <param name="alphaThreshold">If <paramref name="newPixelFormat"/> can represent only single-bit alpha or <paramref name="newPixelFormat"/> is an indexed format and the target palette contains a transparent color,
        /// then specifies a threshold value for the <see cref="Color.A">Color.A</see> property, under which the color is considered transparent. If 0,
        /// then the result will not have transparent pixels. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/?topic=html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm" target="_blank">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/?topic=html/T_KGySoft_Threading_IAsyncProgress.htm" target="_blank">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. Its result is the new <see cref="WriteableBitmap"/> instance with the desired pixel format,
        /// or <see langword="null"/>, if the operation was canceled and the <a href="https://docs.kgysoft.net/corelibraries/?topic=html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm" target="_blank">ThrowIfCanceled</a>property of the <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>Alternatively, you can also use the <see cref="BeginConvertPixelFormat(BitmapSource, PixelFormat, Color, byte, AsyncConfig)"/> method, which is available on every platform.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/?topic=html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm" target="_blank">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="caution">This method can be called from any thread but if it is called from a different one from the thread of the source <paramref name="bitmap"/>'s dispatcher,
        /// then the result <see cref="WriteableBitmap"/> will be created by using a synchronized callback. To avoid possible deadlocks, the thread of the source <paramref name="bitmap"/>
        /// must not be blocked and the dispatcher must run. The result will be usable in the same thread as the source <paramref name="bitmap"/>.
        /// To avoid blocking waiting for the result use the <see langword="await"/>&#160;keyword in C# (when using .NET Framework 4.5 or newer),
        /// or use the <see cref="Task{TResult}.ContinueWith(Action{Task{TResult}})">Task.ContinueWith</see> method to access
        /// the <see cref="Task{TResult}.Result">Result</see> of the completed task from there.</note>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="ConvertPixelFormat(BitmapSource, PixelFormat, Color, byte)"/> method for more details and image examples.</note>
        /// </remarks>
        public static Task<WriteableBitmap?> ConvertPixelFormatAsync(this BitmapSource bitmap, PixelFormat newPixelFormat, Color backColor = default, byte alphaThreshold = 128, TaskConfig? asyncConfig = null)
            => ConvertPixelFormatAsync(bitmap, newPixelFormat, null, backColor, alphaThreshold, asyncConfig);

        /// <summary>
        /// Converts the specified <paramref name="bitmap"/> to a <see cref="WriteableBitmap"/> of the desired <see cref="PixelFormat"/> asynchronously.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="bitmap">The original bitmap to convert.</param>
        /// <param name="newPixelFormat">The desired new pixel format.</param>
        /// <param name="palette">The desired target palette if <paramref name="newPixelFormat"/> is an indexed format. If <see langword="null"/>,
        /// then the source palette is taken from the source image if it also has a palette of no more entries than the target indexed format can have;
        /// otherwise, a default palette will be used based on <paramref name="newPixelFormat"/>.</param>
        /// <param name="backColor">If <paramref name="newPixelFormat"/> does not support alpha or supports only single-bit alpha, then specifies the color of the background.
        /// Source pixels with alpha, which will be opaque in the result will be blended with this color.
        /// The <see cref="Color.A">Color.A</see> property of the background color is ignored. This parameter is optional.
        /// <br/>Default value: The bitwise zero instance of <see cref="Color"/>, which has the same RGB values as <see cref="Colors.Black"/>.</param>
        /// <param name="alphaThreshold">If <paramref name="newPixelFormat"/> can represent only single-bit alpha or <paramref name="newPixelFormat"/> is an indexed format and the target palette contains a transparent color,
        /// then specifies a threshold value for the <see cref="Color.A">Color.A</see> property, under which the color is considered transparent. If 0,
        /// then the result will not have transparent pixels. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/?topic=html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm" target="_blank">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/?topic=html/T_KGySoft_Threading_IAsyncProgress.htm" target="_blank">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. Its result is the new <see cref="WriteableBitmap"/> instance with the desired pixel format,
        /// or <see langword="null"/>, if the operation was canceled and the <a href="https://docs.kgysoft.net/corelibraries/?topic=html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm" target="_blank">ThrowIfCanceled</a>property of the <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>Alternatively, you can also use the <see cref="BeginConvertPixelFormat(BitmapSource, PixelFormat, Color, byte, AsyncConfig)"/> method, which is available on every platform.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/?topic=html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm" target="_blank">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="caution">This method can be called from any thread but if it is called from a different one from the thread of the source <paramref name="bitmap"/>'s dispatcher,
        /// then the result <see cref="WriteableBitmap"/> will be created by using a synchronized callback. To avoid possible deadlocks, the thread of the source <paramref name="bitmap"/>
        /// must not be blocked and the dispatcher must run. The result will be usable in the same thread as the source <paramref name="bitmap"/>.
        /// To avoid blocking waiting for the result use the <see langword="await"/>&#160;keyword in C# (when using .NET Framework 4.5 or newer),
        /// or use the <see cref="Task{TResult}.ContinueWith(Action{Task{TResult}})">Task.ContinueWith</see> method to access
        /// the <see cref="Task{TResult}.Result">Result</see> of the completed task from there.</note>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="ConvertPixelFormat(BitmapSource, PixelFormat, Color, byte)"/> method for more details and image examples.</note>
        /// </remarks>
        public static Task<WriteableBitmap?> ConvertPixelFormatAsync(this BitmapSource bitmap, PixelFormat newPixelFormat, Color[]? palette, Color backColor = default, byte alphaThreshold = 128, TaskConfig? asyncConfig = null)
        {
            ValidateConvertPixelFormat(bitmap, newPixelFormat);
            var context = new ConversionContext(bitmap, newPixelFormat, palette, backColor, alphaThreshold);
            return AsyncHelper.DoOperationAsync(ctx => DoConvertPixelFormatDirect(ctx, context), asyncConfig);
        }

        /// <summary>
        /// Converts the specified <paramref name="bitmap"/> to a <see cref="WriteableBitmap"/> of the desired <see cref="PixelFormat"/> asynchronously.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="bitmap">The original bitmap to convert.</param>
        /// <param name="newPixelFormat">The desired new pixel format.</param>
        /// <param name="quantizer">An optional <see cref="IQuantizer"/> instance to determine the colors if the result.
        /// If <see langword="null"/>&#160;and <paramref name="newPixelFormat"/> is an indexed format, then a default palette and quantization logic will be used.</param>
        /// <param name="ditherer">The ditherer to be used. Might be ignored if <paramref name="quantizer"/> is not specified
        /// and <paramref name="newPixelFormat"/> represents an at least 24 bits-per-pixel size. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/?topic=html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm" target="_blank">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/?topic=html/T_KGySoft_Threading_IAsyncProgress.htm" target="_blank">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. Its result is the new <see cref="WriteableBitmap"/> instance with the desired pixel format,
        /// or <see langword="null"/>, if the operation was canceled and the <a href="https://docs.kgysoft.net/corelibraries/?topic=html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm" target="_blank">ThrowIfCanceled</a>property of the <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>Alternatively, you can also use the <see cref="BeginConvertPixelFormat(BitmapSource, PixelFormat, Color, byte, AsyncConfig)"/> method, which is available on every platform.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/?topic=html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm" target="_blank">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="caution"><list type="bullet"><item>This method can be called from any thread but if it is called from a different one from the thread of the source <paramref name="bitmap"/>'s dispatcher,
        /// then the result <see cref="WriteableBitmap"/> will be created by using a synchronized callback. To avoid possible deadlocks, the thread of the source <paramref name="bitmap"/>
        /// must not be blocked and the dispatcher must run. The result will be usable in the same thread as the source <paramref name="bitmap"/>.
        /// To avoid blocking waiting for the result use the <see langword="await"/>&#160;keyword in C# (when using .NET Framework 4.5 or newer),
        /// or use the <see cref="Task{TResult}.ContinueWith(Action{Task{TResult}})">Task.ContinueWith</see> method to access
        /// the <see cref="Task{TResult}.Result">Result</see> of the completed task from there.</item>
        /// <item>If <paramref name="quantizer"/> is not a <see cref="PredefinedColorsQuantizer"/>, then the result <see cref="WriteableBitmap"/> is created by a synchronized callback
        /// on the thread of the source <paramref name="bitmap"/> even if you call this method from the same thread. Do not block the thread of the source <paramref name="bitmap"/>;
        /// otherwise, a deadlock may occur.</item></list></note>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="ConvertPixelFormat(BitmapSource, PixelFormat, Color, byte)"/> method for more details and image examples.</note>
        /// </remarks>
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
