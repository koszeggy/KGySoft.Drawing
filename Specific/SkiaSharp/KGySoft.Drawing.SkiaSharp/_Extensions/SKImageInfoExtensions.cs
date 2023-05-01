#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: SKImageInfoExtensions.cs
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

using System;
using System.Diagnostics.CodeAnalysis;

using KGySoft.CoreLibraries;
using KGySoft.Drawing.Imaging;

using SkiaSharp;

#endregion

namespace KGySoft.Drawing.SkiaSharp
{
    /// <summary>
    /// Contains extension methods for the <see cref="SKImageInfo"/> type.
    /// </summary>
    public static class SKImageInfoExtensions
    {
        #region Methods

        #region Public Methods

        /// <summary>
        /// Gets a <see cref="PixelFormatInfo"/> for this <paramref name="imageInfo"/>.
        /// </summary>
        /// <param name="imageInfo">The <see cref="SKImageInfo"/> to retrieve a <see cref="PixelFormatInfo"/> for.</param>
        /// <returns>A <see cref="PixelFormatInfo"/> that represents the specified <see cref="SKImageInfo"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="imageInfo"/> must be a non-default valid value.</exception>
        public static PixelFormatInfo GetInfo(this SKImageInfo imageInfo)
        {
            KnownPixelFormat pixelFormat = imageInfo != SKImageInfo.Empty
                ? imageInfo.AsKnownPixelFormat()
                : throw new ArgumentException(PublicResources.ArgumentEmpty, nameof(imageInfo));

            if (pixelFormat != KnownPixelFormat.Undefined)
                return new PixelFormatInfo(pixelFormat);

            if (imageInfo.ColorType == SKColorType.Unknown || imageInfo.AlphaType == SKAlphaType.Unknown
                || !imageInfo.ColorType.IsDefined() || !imageInfo.AlphaType.IsDefined())
            {
                throw new ArgumentException(Res.ImageInfoInvalid(imageInfo.ColorType, imageInfo.AlphaType), nameof(imageInfo));
            }

            var info = new PixelFormatInfo((byte)imageInfo.BitsPerPixel);
            switch (imageInfo.AlphaType)
            {
                case SKAlphaType.Premul:
                    info.HasPremultipliedAlpha = imageInfo.HasAlpha();
                    break;
                case SKAlphaType.Unpremul:
                    info.HasAlpha = imageInfo.HasAlpha();
                    break;
            }

            switch (imageInfo.ColorType)
            {
                // These types have alpha even with AlphaType.Opaque.
                case SKColorType.Alpha8:
                case SKColorType.Alpha16:
                case SKColorType.AlphaF16:
                    info.HasAlpha = true;
                    break;
                case SKColorType.Gray8:
                    info.Grayscale = true;
                    break;
            }

            if (imageInfo.ColorSpace.IsDefaultLinear())
                info.LinearGamma = true;

            return info;
        }

        /// <summary>
        /// Gets whether this <see cref="SKImageInfo"/> instance represents a format with alpha (transparency) without checking
        /// whether <paramref name="imageInfo"/> represents a valid value.
        /// </summary>
        /// <param name="imageInfo">The <see cref="SKImageInfo"/> to be checked.</param>
        /// <returns><see langword="true"/>, if this <see cref="SKImageInfo"/> instance represents a format with alpha; otherwise, <see langword="false"/>.</returns>
        public static bool HasAlpha(this SKImageInfo imageInfo)
            => (imageInfo.ColorType is SKColorType.Alpha8 or SKColorType.Alpha16 or SKColorType.AlphaF16)
                || ((imageInfo.AlphaType is SKAlphaType.Unpremul or SKAlphaType.Premul)
                    && (imageInfo.ColorType is SKColorType.Bgra8888 or SKColorType.Rgba8888
                        or SKColorType.Rgba1010102 or SKColorType.Bgra1010102 or SKColorType.Argb4444
                        or SKColorType.RgbaF16 or SKColorType.RgbaF16Clamped or SKColorType.RgbaF32 or SKColorType.Rgba16161616));

        /// <summary>
        /// Gets a <see cref="PredefinedColorsQuantizer"/> instance that fits for the specified <paramref name="imageInfo"/>.
        /// </summary>
        /// <param name="imageInfo">The <see cref="SKImageInfo"/> to get a quantizer for.</param>
        /// <param name="backColor">Colors with alpha (transparency), which are considered opaque will be blended with this color before quantization.
        /// The <see cref="SKColor.Alpha"/> property of the background color is ignored. This parameter is optional.
        /// <br/>Default value: The bitwise zero instance of <see cref="SKColor"/>, which has the same RGB values as <see cref="SKColors.Black"/>.</param>
        /// <param name="alphaThreshold">Specifies a threshold value for the <see cref="SKColor.Alpha"/> property,
        /// under which a quantized color is considered completely transparent. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <returns>A <see cref="PredefinedColorsQuantizer"/> instance that is compatible with the specified <paramref name="imageInfo"/>.</returns>
        public static PredefinedColorsQuantizer GetMatchingQuantizer(this SKImageInfo imageInfo, SKColor backColor = default, byte alphaThreshold = 128)
        {
            KnownPixelFormat asKnown = imageInfo.AsKnownPixelFormat();
            Color32 backColor32 = backColor.ToColor32();
            if (asKnown != KnownPixelFormat.Undefined)
            {
                // The alpha threshold of the returned quantizer must be 0 to prevent making alpha gradient fully transparent.
                // It will not be a problem because in SkiaSharp indexed and single-bit formats are not supported.
                Debug.Assert(!asKnown.IsIndexed() && !asKnown.GetInfo().HasSingleBitAlpha);
                return PredefinedColorsQuantizer.FromPixelFormat(asKnown, backColor32, alphaThreshold).ConfigureColorSpace(imageInfo.GetWorkingColorSpace());
            }

            return ((imageInfo.ColorType, imageInfo.AlphaType, imageInfo.GetWorkingColorSpace()) switch
            {
                // Rgba1010102/Bgra1010102: just to quantize alpha
                (SKColorType.Bgra1010102 or SKColorType.Rgba1010102, not SKAlphaType.Opaque, _)
                    => PredefinedColorsQuantizer.FromCustomFunction(c => new ColorBgra1010102Srgb(c).ToColor32(), backColor32, alphaThreshold, false),

                // Argb4444: considering all possible parameters
                // NOTE: We could use explicit blending for the opaque formats for slightly better performance (especially in linear mode)
                //       but letting the quantizer blending allows using a possibly changed working color space after the result is returned.
                (SKColorType.Argb4444, SKAlphaType.Unpremul, WorkingColorSpace.Linear)
                    => PredefinedColorsQuantizer.FromCustomFunction(c => new ColorArgb4444Linear(c).ToColor32(), backColor32, alphaThreshold, false),
                (SKColorType.Argb4444, SKAlphaType.Premul, WorkingColorSpace.Linear)
                    => PredefinedColorsQuantizer.FromCustomFunction(c => new ColorPargb4444Linear(c).ToColor32(), backColor32, alphaThreshold, false),
                (SKColorType.Argb4444, SKAlphaType.Opaque, WorkingColorSpace.Linear)
                    => PredefinedColorsQuantizer.FromCustomFunction(c => new ColorArgb4444Linear(c).ToColor32(), backColor32),
                (SKColorType.Argb4444, SKAlphaType.Unpremul, _)
                    => PredefinedColorsQuantizer.FromCustomFunction(c => new ColorArgb4444Srgb(c).ToColor32(), backColor32, alphaThreshold, false),
                (SKColorType.Argb4444, SKAlphaType.Premul, _)
                    => PredefinedColorsQuantizer.FromCustomFunction(c => new ColorPargb4444Srgb(c).ToColor32(), backColor32, alphaThreshold, false),
                (SKColorType.Argb4444, SKAlphaType.Opaque, _)
                    => PredefinedColorsQuantizer.FromCustomFunction(c => new ColorArgb4444Srgb(c).ToColor32(), backColor32),

                // Rgb565: only the linear one because the sRGB one is handled above as known format
                (SKColorType.Rgb565, _, WorkingColorSpace.Linear)
                    => PredefinedColorsQuantizer.FromCustomFunction(c => new ColorRgb565Linear(c).ToColor32(), backColor32),

                // Bgra8888/Rgba8888 with linear color space as they get quantized during the sRGB -> linear conversion so ditherers can improve the result
                (SKColorType.Bgra8888 or SKColorType.Rgba8888, SKAlphaType.Unpremul, WorkingColorSpace.Linear)
                    => PredefinedColorsQuantizer.FromCustomFunction(c => new ColorBgra8888Linear(c).ToColor32(), backColor32, alphaThreshold, false),
                (SKColorType.Bgra8888 or SKColorType.Rgba8888, SKAlphaType.Premul, WorkingColorSpace.Linear)
                    => PredefinedColorsQuantizer.FromCustomFunction(c => new ColorPbgra8888Linear(c).ToColor32(), backColor32, alphaThreshold, false),

                // Opaque RG[B] 8bpp/channel color types with linear color spaces: like above
                // NOTE: using the same quantizer for RG88 so no extra compensation will occur for the blue channel
                (SKColorType.Bgra8888 or SKColorType.Rgba8888, SKAlphaType.Opaque, WorkingColorSpace.Linear)
                    or (SKColorType.Rgb888x or SKColorType.Rg88, _, WorkingColorSpace.Linear)
                    => PredefinedColorsQuantizer.FromCustomFunction(c => new ColorBgra8888Linear(c).ToColor32(), backColor32),

                // Gray8 with linear color space as it gets quantized during the sRGB -> linear conversion so ditherers can improve the result
                (SKColorType.Gray8, _, WorkingColorSpace.Linear)
                    => PredefinedColorsQuantizer.FromCustomFunction(c => new ColorGray8Linear(c.ToColorF()).ToColor32(), backColor32, KnownPixelFormat.Format16bppGrayScale),

                // Fallback: some default quantizer from the closest known pixel format
                _ => PredefinedColorsQuantizer.FromPixelFormat(imageInfo.GetInfo().ToKnownPixelFormat(), backColor32, alphaThreshold)
            }).ConfigureColorSpace(imageInfo.GetWorkingColorSpace());
        }

        #endregion

        #region Internal Methods

        internal static KnownPixelFormat AsKnownPixelFormat(this SKImageInfo imageInfo)
        {
            if (imageInfo.ColorSpace?.IsDefaultSrgb() == false)
                return KnownPixelFormat.Undefined;

            return imageInfo.ColorType switch
            {
                SKColorType.Bgra8888 => imageInfo.AlphaType switch
                {
                    SKAlphaType.Unpremul => KnownPixelFormat.Format32bppArgb,
                    SKAlphaType.Premul => KnownPixelFormat.Format32bppPArgb,
                    SKAlphaType.Opaque => KnownPixelFormat.Format32bppRgb,
                    _ => KnownPixelFormat.Undefined
                },
                SKColorType.Rgb565 => (uint)imageInfo.AlphaType <= (uint)ColorExtensions.MaxAlphaType ? KnownPixelFormat.Format16bppRgb565 : KnownPixelFormat.Undefined,
                _ => KnownPixelFormat.Undefined
            };
        }

        [SuppressMessage("ReSharper", "AssignmentInConditionalExpression", Justification = "Intended")]
        internal static void GetDirectlySupportedColorSpace(this SKImageInfo imageInfo, out bool srgb, out bool linear)
        {
            srgb = false;
            linear = false;

            if (imageInfo.ColorType is <= SKColorType.Unknown or > ColorExtensions.MaxColorType
                || imageInfo.AlphaType is <= SKAlphaType.Unknown or > ColorExtensions.MaxAlphaType)
            {
                return;
            }

            if (srgb = imageInfo.ColorSpace.IsDefaultSrgb())
                return;

            linear = imageInfo.ColorSpace.IsDefaultLinear();
        }

        internal static bool IsDirectlySupported(this SKImageInfo imageInfo)
            => imageInfo.ColorType is > SKColorType.Unknown and <= ColorExtensions.MaxColorType
                && imageInfo.AlphaType is > SKAlphaType.Unknown and <= ColorExtensions.MaxAlphaType
                && (imageInfo.ColorSpace.IsDefaultSrgb() || imageInfo.ColorSpace.IsDefaultLinear());

        internal static bool CanBeDithered(this SKImageInfo imageInfo)
            => imageInfo.ColorType is SKColorType.Rgb565 or SKColorType.Argb4444 && imageInfo.IsDirectlySupported()
                || imageInfo.ColorSpace.IsDefaultLinear() && imageInfo.ColorType
                    is SKColorType.Rgba8888 or SKColorType.Rgb888x or SKColorType.Bgra8888 or SKColorType.Gray8 or SKColorType.Rg88;

        /// <summary>
        /// NOTE: Gets Default both for null and unsupported color spaces whereas imageInfo.ColorSpace.IsDefaultSrgb would return true for null color space.
        /// </summary>
        internal static WorkingColorSpace GetWorkingColorSpace(this SKImageInfo imageInfo)
            => imageInfo.ColorSpace is null ? WorkingColorSpace.Default
                : imageInfo.ColorSpace.IsDefaultLinear() ? WorkingColorSpace.Linear
                : imageInfo.ColorSpace.IsDefaultSrgb() ? WorkingColorSpace.Srgb
                : WorkingColorSpace.Default;

        #endregion

        #endregion
    }
}
