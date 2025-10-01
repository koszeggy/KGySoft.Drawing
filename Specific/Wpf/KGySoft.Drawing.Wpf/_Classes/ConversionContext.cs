#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ConversionContext.cs
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
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

using KGySoft.Drawing.Imaging;
using KGySoft.Reflection;

#endregion

namespace KGySoft.Drawing.Wpf
{
    /// <summary>
    /// A class for <see cref="WriteableBitmap"/> conversions.
    /// Contains all possible prefetched data to avoid most of the sync callbacks from the working thread
    /// </summary>
    internal sealed class ConversionContext : IDisposable
    {
        #region Fields

        #region Static Fields

        private static readonly TimeSpan callbackTimeout = TimeSpan.FromMilliseconds(250);

        #endregion

        #region Instance Fields

        private readonly Dispatcher dispatcher;
        private readonly bool disposeSource;

        #endregion

        #endregion

        #region Properties

        internal BitmapSource? BitmapSource { get; }
        internal PixelFormat PixelFormat { get; }
        internal IReadableBitmapData Source { get; private set; } = default!;
        internal WriteableBitmap? Result { get; set; }
        internal IWritableBitmapData? Target { get; set; }
        internal IQuantizer? Quantizer { get; }
        internal IDitherer? Ditherer { get; }

        #endregion

        #region Constructors

        internal ConversionContext(IReadableBitmapData source)
        {
            dispatcher = Dispatcher.CurrentDispatcher;
            Source = source;
            Result = CreateCompatibleBitmap(source);
            PixelFormat = Result.Format;
            Target = Result.GetWritableBitmapData(source.BackColor.ToMediaColor(), source.AlphaThreshold);
        }

        internal ConversionContext(BitmapSource bitmap, PixelFormat newPixelFormat, Color[]? palette, Color backColor, byte alphaThreshold)
        {
            dispatcher = bitmap.Dispatcher;
            disposeSource = true;
            BitmapSource = bitmap;
            PixelFormat = newPixelFormat;
            InitDirect(palette, backColor, alphaThreshold);
        }

        internal ConversionContext(IReadableBitmapData source, PixelFormat pixelFormat, IQuantizer? quantizer, IDitherer? ditherer)
        {
            dispatcher = Dispatcher.CurrentDispatcher;
            Source = source;
            PixelFormat = pixelFormat;
            Quantizer = quantizer;
            if (quantizer == null)
            {
                // converting without using a quantizer (even if only a ditherer is specified for a high-bpp pixel format)
                if (ditherer == null || !pixelFormat.CanBeDithered())
                {
                    Result = new WriteableBitmap(source.Width, source.Height, 96, 96, pixelFormat, GetTargetPalette(default(Palette)));
                    Target = Result.GetWritableBitmapData(source.BackColor.ToMediaColor(), source.AlphaThreshold);
                    return;
                }

                // here we need to pick a quantizer for the dithering
                int bpp = pixelFormat.BitsPerPixel;
                Palette? palette = source.Palette;
                int paletteCount = palette?.Count ?? 0;
                Quantizer = pixelFormat.IsIndexed() && bpp <= 8 && paletteCount > 0 && paletteCount <= (1 << bpp)
                    ? PredefinedColorsQuantizer.FromCustomPalette(palette!)
                    : pixelFormat.GetMatchingQuantizer();
            }

            Ditherer = ditherer;

            // Precreating the result bitmap only if the quantizer can be initialized effortlessly
            if (Quantizer is not PredefinedColorsQuantizer predefinedColorsQuantizer)
                return;

            Result = new WriteableBitmap(Source.Width, Source.Height, 96, 96, pixelFormat,
                GetTargetPalette(predefinedColorsQuantizer.Palette));
            Target = Result.GetWritableBitmapData(predefinedColorsQuantizer.BackColor.ToMediaColor(), predefinedColorsQuantizer.AlphaThreshold);
        }

        internal ConversionContext(BitmapSource bitmap, PixelFormat newPixelFormat, IQuantizer? quantizer, IDitherer? ditherer)
        {
            dispatcher = bitmap.Dispatcher;
            disposeSource = true;
            BitmapSource = bitmap;
            PixelFormat = newPixelFormat;
            Quantizer = quantizer;
            IList<Color>? sourcePaletteEntries = null;
            if (quantizer == null)
            {
                // converting without using a quantizer (even if only a ditherer is specified for a high-bpp pixel format)
                if (ditherer == null || !newPixelFormat.CanBeDithered())
                {
                    InitDirect(null, default, 128);
                    return;
                }

                // here we need to pick a quantizer for the dithering
                int bpp = newPixelFormat.BitsPerPixel;
                BitmapPalette? sourcePalette = null;
                Invoke(true, () => sourcePalette = bitmap.Palette);

                sourcePaletteEntries = sourcePalette?.Colors ?? Reflector.EmptyArray<Color>();
                if (newPixelFormat.IsIndexed() && bpp <= 8 && sourcePaletteEntries.Count > 0 && sourcePaletteEntries.Count <= (1 << bpp))
                    Quantizer = PredefinedColorsQuantizer.FromCustomPalette(new Palette(sourcePaletteEntries.Select(c => c.ToColor32()).ToArray()));
                else
                {
                    Quantizer = newPixelFormat.GetMatchingQuantizer();
                    sourcePaletteEntries = null;
                }
            }

            Ditherer = ditherer;
            Invoke(true, () =>
            {
                Source = GetSourceBitmapData();

                // Precreating the result bitmap only if the quantizer can be initialized effortlessly
                if (Quantizer is not PredefinedColorsQuantizer predefinedColorsQuantizer)
                    return;

                Result = new WriteableBitmap(Source.Width, Source.Height, bitmap.DpiX, bitmap.DpiY, newPixelFormat,
                    GetTargetPalette(sourcePaletteEntries ?? predefinedColorsQuantizer.Palette?.GetEntries().Select(c => c.ToMediaColor()).ToArray()));
                Target = Result.GetWritableBitmapData(predefinedColorsQuantizer.BackColor.ToMediaColor(), predefinedColorsQuantizer.AlphaThreshold);
            });
        }

        #endregion

        #region Methods

        #region Static Methods

        private static WriteableBitmap CreateCompatibleBitmap(IBitmapData source)
        {
            PixelFormatInfo sourceFormat = source.PixelFormat;
            PixelFormat pixelFormat = sourceFormat.ToKnownPixelFormat().ToPixelFormat();
            Palette? palette = source.Palette;

            // indexed custom formats with >8 bpp: ToKnownPixelFormat returns 32bpp but it can be fine-tuned
            if (sourceFormat.IsCustomFormat && sourceFormat.Indexed && sourceFormat.BitsPerPixel > 8 && palette != null)
                pixelFormat = palette.HasAlpha ? PixelFormats.Bgra32
                    : palette.IsGrayscale ? PixelFormats.Gray16
                    : PixelFormats.Bgr24;

            return new WriteableBitmap(source.Width, source.Height, 96d, 96d, pixelFormat, pixelFormat.IsIndexed() ? palette.ToBitmapPalette() : null);
        }

        #endregion

        #region Instance Methods

        #region Public Methods

        public void Dispose() => Invoke(false, () =>
        {
            Target?.Dispose();
            if (disposeSource)
                Source.Dispose();
        });

        #endregion

        #region Internal Methods

        internal void Invoke(bool synchronized, Action callback)
        {
            if (dispatcher.CheckAccess())
            {
                callback.Invoke();
                return;
            }

            DispatcherOperation operation = dispatcher.BeginInvoke(DispatcherPriority.Send, callback);
            if (!synchronized)
                return;

            if (operation.Wait(callbackTimeout) != DispatcherOperationStatus.Completed)
                throw new InvalidOperationException(WpfRes.DispatcherDeadlock);
        }

        internal BitmapPalette? GetTargetPalette(Palette? palette)
        {
            if (!PixelFormat.IsIndexed())
                return null;

            int bpp = PixelFormat.BitsPerPixel;
            int maxColors = 1 << bpp;

            // if no desired colors are specified but converting to a higher bpp indexed image, then taking the source palette
            if (palette == null && Source.PixelFormat.Indexed && Source.Palette is { Count: > 0 } sourcePalette && sourcePalette.Count <= maxColors)
                return sourcePalette.ToBitmapPalette();

            if (palette == null || palette.Count == 0)
                return PixelFormat.GetDefaultPalette();

            // there is a desired palette to apply
            if (palette.Count > maxColors)
                throw new ArgumentException(WpfRes.PaletteTooLarge(maxColors, bpp), nameof(palette));

            return palette.ToBitmapPalette();
        }

        #endregion

        #region Private Methods

        private BitmapPalette? GetTargetPalette(IList<Color>? palette)
        {
            if (!PixelFormat.IsIndexed())
                return null;

            int bpp = PixelFormat.BitsPerPixel;
            var sourceInfo = BitmapSource!.Format.GetInfo();

            // if no desired colors are specified but converting to a higher bpp indexed image, then taking the source palette
            if (palette == null && sourceInfo.Indexed && sourceInfo.BitsPerPixel <= bpp)
                return BitmapSource.Palette;

            if (palette == null || palette.Count == 0)
                return PixelFormat.GetDefaultPalette();

            // there is a desired palette to apply
            int maxColors = 1 << bpp;
            if (palette.Count > maxColors)
                throw new ArgumentException(WpfRes.PaletteTooLarge(maxColors, bpp), nameof(palette));

            return new BitmapPalette(palette);
        }

        private void InitDirect(Color[]? palette, Color backColor, byte alphaThreshold)
            => Invoke(true, () =>
            {
                BitmapSource bitmap = BitmapSource!;
                Source = GetSourceBitmapData();
                Result = new WriteableBitmap(bitmap.PixelWidth, bitmap.PixelHeight, bitmap.DpiX, bitmap.DpiY, PixelFormat,
                    GetTargetPalette(palette));
                Target = Result.GetWritableBitmapData(backColor, alphaThreshold);
            });

        private IReadableBitmapData GetSourceBitmapData()
            => BitmapSource is WriteableBitmap { IsFrozen: false } wb
                ? wb.GetBitmapDataInternal(true)
                : BitmapSource!.GetReadableBitmapData();


        #endregion

        #endregion

        #endregion
    }
}
