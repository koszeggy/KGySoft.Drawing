#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BitmapDataExtensions.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2023 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System.Drawing;

#endregion

namespace KGySoft.Drawing.Imaging
{
    /// <summary>
    /// Provides extension methods for the <see cref="IReadableBitmapData"/>, <see cref="IWritableBitmapData"/>
    /// and <see cref="IReadWriteBitmapData"/> types.
    /// </summary>
    public static partial class BitmapDataExtensions
    {
        #region Constants

        private const int parallelThreshold = 100;
        private const int quantizingScale = 1;
        private const int ditheringScale = 2;

        #endregion

        #region Methods

        #region Internal Methods

        internal static bool HasMultiLevelAlpha(this IBitmapData bitmapData)
        {
            PixelFormatInfo pixelFormat = bitmapData.PixelFormat;
            return pixelFormat.HasMultiLevelAlpha || pixelFormat.Indexed && bitmapData.Palette?.HasMultiLevelAlpha == true;
        }

        // TODO: IsFastPremultiplied32: current implementation OR: custom pixel format and has a direct P32 setter
        internal static bool IsFastPremultiplied(this IBitmapData bitmapData)
            => bitmapData.PixelFormat.HasPremultipliedAlpha
                && bitmapData is ManagedBitmapDataBase { IsCustomPixelFormat: false } or UnmanagedBitmapDataBase { IsCustomPixelFormat: false };

        internal static bool HasAlpha(this IBitmapData bitmapData)
        {
            PixelFormatInfo pixelFormat = bitmapData.PixelFormat;
            return pixelFormat.HasAlpha || pixelFormat.Indexed && bitmapData.Palette?.HasAlpha == true;
        }

        internal static bool SupportsTransparency(this IBitmapData bitmapData)
        {
            PixelFormatInfo pixelFormat = bitmapData.PixelFormat;
            return pixelFormat.HasAlpha || pixelFormat.Indexed && bitmapData.Palette?.HasTransparent == true;
        }

        internal static bool IsGrayscale(this IBitmapData bitmapData)
            => bitmapData.Palette?.IsGrayscale ?? bitmapData.PixelFormat.Grayscale;

        internal static bool LinearBlending(this IBitmapData bitmapData)
            => bitmapData.BlendingMode == BlendingMode.Linear || bitmapData.BlendingMode == BlendingMode.Default && bitmapData.PixelFormat.LinearGamma;

        internal static KnownPixelFormat GetKnownPixelFormat(this IBitmapData bitmapData)
        {
            PixelFormatInfo info = bitmapData.PixelFormat;
            if (!info.IsCustomFormat)
                return info.AsKnownPixelFormatInternal;

            int bpp = info.BitsPerPixel;
            if (bpp > 32)
                return info.HasPremultipliedAlpha ? KnownPixelFormat.Format64bppPArgb
                    : bitmapData.HasAlpha() ? KnownPixelFormat.Format64bppArgb
                    : info.Grayscale ? KnownPixelFormat.Format16bppGrayScale
                    : KnownPixelFormat.Format48bppRgb;
            if (bpp > 8 || !info.Indexed)
                return info.HasPremultipliedAlpha ? KnownPixelFormat.Format32bppPArgb
                    : bitmapData.HasAlpha() ? KnownPixelFormat.Format32bppArgb
                    : bitmapData.IsGrayscale() ? KnownPixelFormat.Format16bppGrayScale
                    : KnownPixelFormat.Format24bppRgb;
            return bpp switch
            {
                > 4 => KnownPixelFormat.Format8bppIndexed,
                > 1 => KnownPixelFormat.Format4bppIndexed,
                _ => KnownPixelFormat.Format1bppIndexed
            };
        }

        #endregion

        #region Private Methods

        private static void Unwrap<TBitmapData>(ref TBitmapData source, ref Rectangle newRectangle)
            where TBitmapData : IBitmapData
        {
            while (true)
            {
                switch (source)
                {
                    case ClippedBitmapData clipped:
                        source = (TBitmapData)clipped.BitmapData;
                        Rectangle region = clipped.Region;
                        newRectangle.Offset(region.Location);
                        newRectangle.Intersect(region);
                        continue;
                    case BitmapDataWrapper wrapper:
                        Debug.Fail("Wrapper has been leaked out, check call stack");
                        source = (TBitmapData)wrapper.BitmapData;
                        continue;
                    default:
                        return;
                }
            }
        }

        private static void AdjustQuantizerAndDitherer(IBitmapData target, ref IQuantizer? quantizer, ref IDitherer? ditherer)
        {
            if (quantizer != null || ditherer == null)
                return;

            if (target.PixelFormat.CanBeDithered)
                quantizer = PredefinedColorsQuantizer.FromBitmapData(target);
            else
                ditherer = null;
        }

        private static KnownPixelFormat GetPreferredFirstPassPixelFormat(this IBitmapData target, BlendingMode blendingMode)
            // Multi pass processing is only for quantizers or ditherers that require initialization with the actual image.
            // Therefore it is always enough to use a 32bpp temp 1st pass buffer because a quantizer is based on Color32 colors.
            // To optimize blending/processing speed we use straight colors if the target is also straight or when blending
            // will use linear color space; otherwise, we can use the premultiplied sRGB pixel format.
            => target.PixelFormat.AsKnownPixelFormatInternal == KnownPixelFormat.Format32bppArgb
                ? KnownPixelFormat.Format32bppArgb
                : blendingMode == BlendingMode.Linear ? KnownPixelFormat.Format32bppArgb : KnownPixelFormat.Format32bppPArgb;

        #endregion

        #endregion
    }
}
