#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BitmapDataExtensions.cs
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

using System.Drawing;

using KGySoft.CoreLibraries;

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

        #region Public Methods

        /// <summary>
        /// Gets a non-default <see cref="WorkingColorSpace"/> that can be used when working with the specified <paramref name="bitmapData"/>.
        /// </summary>
        /// <param name="bitmapData">An <see cref="IBitmapData"/> to determine the result. If <see langword="null"/>, then <see cref="WorkingColorSpace.Srgb"/> is returned.</param>
        /// <returns>A non-default <see cref="WorkingColorSpace"/> that can be used when working with the specified <paramref name="bitmapData"/>.</returns>
        public static WorkingColorSpace GetPreferredColorSpace(this IBitmapData? bitmapData)
            => bitmapData?.WorkingColorSpace switch
            {
                WorkingColorSpace.Linear => WorkingColorSpace.Linear,
                WorkingColorSpace.Srgb or null => WorkingColorSpace.Srgb,
                _ => bitmapData.PixelFormat.LinearGamma ? WorkingColorSpace.Linear : WorkingColorSpace.Srgb
            };

        #endregion

        #region Internal Methods

        /// <summary>
        /// Gets non-default color space if set, or sets linear if pixel format has linear gamma.
        /// Unlike <see cref="GetPreferredColorSpace"/>, it can return Default.
        /// </summary>
        internal static WorkingColorSpace GetPreferredColorSpaceOrDefault(this IBitmapData bitmapData)
            => bitmapData.WorkingColorSpace == WorkingColorSpace.Default && bitmapData.PixelFormat.LinearGamma ? WorkingColorSpace.Linear : bitmapData.WorkingColorSpace;

        internal static bool HasMultiLevelAlpha(this IBitmapData bitmapData)
        {
            PixelFormatInfo pixelFormat = bitmapData.PixelFormat;
            return pixelFormat.HasMultiLevelAlpha || pixelFormat.Indexed && bitmapData.Palette?.HasMultiLevelAlpha == true;
        }

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
            => bitmapData.WorkingColorSpace == WorkingColorSpace.Linear
                || bitmapData.WorkingColorSpace == WorkingColorSpace.Default && bitmapData.PixelFormat.LinearGamma;

        /// <summary>
        /// Almost the same as <see cref="PixelFormatInfo.ToKnownPixelFormat"/> but can consider
        /// HasAlpha and IsGrayscale also by the palette.
        /// </summary>
        internal static KnownPixelFormat GetKnownPixelFormat(this IBitmapData bitmapData)
        {
            PixelFormatInfo info = bitmapData.PixelFormat;
            if (!info.IsCustomFormat)
            {
                Debug.Assert(info.AsKnownPixelFormatInternal.IsDefined());
                return info.AsKnownPixelFormatInternal;
            }

            int bpp = info.BitsPerPixel;
            if (info.Prefers128BitColors || bpp > 64)
                return info.HasPremultipliedAlpha ? KnownPixelFormat.Format128bppPRgba
                    : info.HasAlpha ? KnownPixelFormat.Format128bppRgba
                    : info.Grayscale ? KnownPixelFormat.Format32bppGrayScale
                    : KnownPixelFormat.Format96bppRgb;
     
            if (info.Prefers64BitColors || bpp > 32)
                return info.HasPremultipliedAlpha ? KnownPixelFormat.Format64bppPArgb
                    : info.HasAlpha ? KnownPixelFormat.Format64bppArgb
                    : info.Grayscale ? KnownPixelFormat.Format16bppGrayScale
                    : KnownPixelFormat.Format48bppRgb;
            
            if (bpp > 8 || !info.Indexed)
                return info.HasPremultipliedAlpha ? KnownPixelFormat.Format32bppPArgb
                    : bitmapData.HasAlpha() ? KnownPixelFormat.Format32bppArgb
                    : bitmapData.IsGrayscale() ? (bpp > 16 ? KnownPixelFormat.Format32bppGrayScale
                        : bpp > 8 ? KnownPixelFormat.Format16bppGrayScale
                        : KnownPixelFormat.Format8bppGrayScale)
                    : KnownPixelFormat.Format24bppRgb;

            return bpp switch
            {
                > 4 => KnownPixelFormat.Format8bppIndexed,
                > 1 => KnownPixelFormat.Format4bppIndexed,
                _ => KnownPixelFormat.Format1bppIndexed
            };
        }

        internal static void AdjustQuantizerAndDitherer(this IBitmapData target, ref IQuantizer? quantizer, ref IDitherer? ditherer)
        {
            if (quantizer != null || ditherer == null)
                return;

            if (target.PixelFormat.CanBeDithered)
                quantizer = PredefinedColorsQuantizer.FromBitmapData(target);
            else
                ditherer = null;
        }

        #endregion

        #region Private Methods

        private static KnownPixelFormat GetPreferredFirstPassPixelFormat(this IBitmapData target, WorkingColorSpace quantizerWorkingColorSpace)
            // Multi pass processing is only for quantizers or ditherers that require initialization with the actual image.
            // Therefore, it is always enough to use a 32bpp temp 1st pass buffer because a quantizer is based on Color32 colors.
            // To optimize blending/processing speed we use straight colors if the target is also straight or when blending
            // will use linear color space; otherwise, we can use the premultiplied sRGB pixel format.
            => target.PixelFormat.AsKnownPixelFormatInternal == KnownPixelFormat.Format32bppArgb
                ? KnownPixelFormat.Format32bppArgb
                : quantizerWorkingColorSpace == WorkingColorSpace.Linear ? KnownPixelFormat.Format32bppArgb : KnownPixelFormat.Format32bppPArgb;

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
                        newRectangle = newRectangle.IntersectSafe(region);
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

        #endregion

        #endregion
    }
}
