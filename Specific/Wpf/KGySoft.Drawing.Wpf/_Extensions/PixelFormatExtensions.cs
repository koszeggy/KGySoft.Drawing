#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: PixelFormatExtensions.cs
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

using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using KGySoft.CoreLibraries;
using KGySoft.Drawing.Imaging;

#endregion

namespace KGySoft.Drawing.Wpf
{
    internal static class PixelFormatExtensions
    {
        #region Fields

        private static Color32[]? indexedDefault2BppPalette;

        #endregion

        #region Properties

        // Contains the 1 bpp colors (black/white) and two more colors from the 4 bpp default palette (gray/silver)
        private static Palette IndexedDefault2BppPalette
            => new Palette(indexedDefault2BppPalette ??= new[] { Color32.FromGray(0), Color32.FromGray(0x80), Color32.FromGray(0xC0), Color32.FromGray(0xFF) });

        #endregion

        #region Methods

        #region Internal Methods

        internal static KnownPixelFormat AsKnownPixelFormat(this PixelFormat pixelFormat)
            => pixelFormat == PixelFormats.Bgra32 ? KnownPixelFormat.Format32bppArgb
             : pixelFormat == PixelFormats.Pbgra32 ? KnownPixelFormat.Format32bppPArgb
             : pixelFormat == PixelFormats.Bgr32 ? KnownPixelFormat.Format32bppRgb
             : pixelFormat == PixelFormats.Bgr24 ? KnownPixelFormat.Format24bppRgb
             : pixelFormat == PixelFormats.Indexed1 ? KnownPixelFormat.Format1bppIndexed
             : pixelFormat == PixelFormats.Indexed4 ? KnownPixelFormat.Format4bppIndexed
             : pixelFormat == PixelFormats.Indexed8 ? KnownPixelFormat.Format8bppIndexed
             : pixelFormat == PixelFormats.Bgr555 ? KnownPixelFormat.Format16bppRgb555
             : pixelFormat == PixelFormats.Bgr565 ? KnownPixelFormat.Format16bppRgb565
             : pixelFormat == PixelFormats.Gray16 ? KnownPixelFormat.Format16bppGrayScale
             : default;

        internal static bool IsIndexed(this PixelFormat pixelFormat)
            => pixelFormat.In(PixelFormats.Indexed8, PixelFormats.Indexed4, PixelFormats.Indexed2, PixelFormats.Indexed1);

        internal static bool CanBeDithered(this PixelFormat dstFormat)
            => dstFormat.BitsPerPixel < 24 && !dstFormat.In(PixelFormats.Gray16, PixelFormats.Gray8);

        internal static IQuantizer GetDefaultQuantizer(this PixelFormat pixelFormat)
            => pixelFormat == PixelFormats.BlackWhite ? PredefinedColorsQuantizer.BlackAndWhite()
             : pixelFormat == PixelFormats.Gray2 ? PredefinedColorsQuantizer.Grayscale4()
             : pixelFormat == PixelFormats.Gray4 ? PredefinedColorsQuantizer.Grayscale16()
             : pixelFormat == PixelFormats.Gray8 ? PredefinedColorsQuantizer.Grayscale()
             : pixelFormat == PixelFormats.Indexed1 ? PredefinedColorsQuantizer.SystemDefault1BppPalette()
             : pixelFormat == PixelFormats.Indexed2 ? PredefinedColorsQuantizer.FromCustomPalette(IndexedDefault2BppPalette)
             : pixelFormat == PixelFormats.Indexed4 ? PredefinedColorsQuantizer.SystemDefault4BppPalette()
             : pixelFormat == PixelFormats.Indexed8 ? PredefinedColorsQuantizer.SystemDefault8BppPalette()
             : PredefinedColorsQuantizer.FromPixelFormat(pixelFormat.ToKnownPixelFormat());

        internal static BitmapPalette? GetDefaultPalette(this PixelFormat pixelFormat)
        {
            var result = pixelFormat == PixelFormats.Indexed1 ? Palette.BlackAndWhite()
                : pixelFormat == PixelFormats.Indexed2 ? IndexedDefault2BppPalette
                : pixelFormat == PixelFormats.Indexed4 ? Palette.SystemDefault4BppPalette()
                : pixelFormat == PixelFormats.Indexed8 ? Palette.SystemDefault8BppPalette()
                : null;
            return result == null ? null : new BitmapPalette(result.GetEntries().Select(c => c.ToMediaColor()).ToArray());
        }

        #endregion

        #region Private Methods

        private static KnownPixelFormat ToKnownPixelFormat(this PixelFormat pixelFormat) => pixelFormat.ToInfo().ToKnownPixelFormat();

        private static PixelFormatInfo ToInfo(this PixelFormat pixelFormat)
        {
            KnownPixelFormat knownPixelFormat = pixelFormat.AsKnownPixelFormat();
            if (knownPixelFormat != KnownPixelFormat.Undefined)
                return new PixelFormatInfo(knownPixelFormat);

            var result = new PixelFormatInfo((byte)pixelFormat.BitsPerPixel);
            if (pixelFormat.IsIndexed())
                result.Indexed = true;
            else if (pixelFormat.In(PixelFormats.BlackWhite, PixelFormats.Gray2, PixelFormats.Gray4, PixelFormats.Gray8, PixelFormats.Gray32Float))
                result.Grayscale = true;
            else if (pixelFormat.In(PixelFormats.Rgba64, PixelFormats.Rgba128Float))
                result.HasAlpha = true;
            else if (pixelFormat.In(PixelFormats.Prgba64, PixelFormats.Prgba128Float))
                result.HasPremultipliedAlpha = true;

            return result;
        }

        #endregion

        #endregion
    }
}