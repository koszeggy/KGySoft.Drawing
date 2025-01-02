﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: SKImageInfoExtensions.cs
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
using System.Diagnostics.CodeAnalysis;

using KGySoft.CoreLibraries;
using KGySoft.Drawing.Imaging;

using SkiaSharp;

#endregion

namespace KGySoft.Drawing.SkiaSharp
{
    /// <summary>
    /// Contains extension methods for the <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skimageinfo">SKImageInfo</a> type.
    /// </summary>
    public static class SKImageInfoExtensions
    {
        #region Methods

        #region Public Methods

        /// <summary>
        /// Gets a <see cref="PixelFormatInfo"/> for this <paramref name="imageInfo"/>.
        /// </summary>
        /// <param name="imageInfo">The <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skimageinfo">SKImageInfo</a> to retrieve a <see cref="PixelFormatInfo"/> for.</param>
        /// <returns>A <see cref="PixelFormatInfo"/> that represents the specified <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skimageinfo">SKImageInfo</a>.</returns>
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

            // [P]ColorF preference: always if the range demands it (RgbaF32) or when it's simpler (AlphaF16 is just a float -> half conversion with no extra division)
            if (imageInfo.ColorType is SKColorType.RgbaF32 or SKColorType.AlphaF16
                // or when extended range could be lost otherwise
                or SKColorType.Bgr101010xXR
                // or when precision could be lost otherwise (16 bits per channel but in linear color space)
                || info.LinearGamma && imageInfo.ColorType is SKColorType.RgbaF16 or SKColorType.RgbaF16Clamped or SKColorType.RgF16 or SKColorType.Rg1616 or SKColorType.Rgba16161616
                // or when the format is so awkward that it's the fastest if converted from PColorF
                || imageInfo.ColorType is SKColorType.Srgba8888 && info is { LinearGamma: true, HasPremultipliedAlpha: true })
            {
                info.Prefers128BitColors = true;
            }
            // [P]Color64 preference: when the range demands it (>8 bit color channels)
            else if (imageInfo.ColorType is SKColorType.Rgba1010102 or SKColorType.Rgb101010x or SKColorType.RgbaF16 or SKColorType.RgbaF16Clamped or SKColorType.RgF16
                     or SKColorType.Alpha16 or SKColorType.Rg1616 or SKColorType.Rgba16161616 or SKColorType.Bgra1010102 or SKColorType.Bgr101010x
                // or when precision could be lost otherwise (8 bits per channel but in linear) - except Alpha8 because gamma does not affect alpha and < 8 channel per color formats
                || info.LinearGamma && imageInfo.ColorType is not (SKColorType.Alpha8 or SKColorType.Rgb565 or SKColorType.Argb4444 or SKColorType.Srgba8888)
                // or when 8-bit precision could be lost only in sRGB (in linear these are the same as existing sRGB formats with no precision loss)
                || !info.LinearGamma && imageInfo.ColorType is SKColorType.Srgba8888)
            {
                info.Prefers64BitColors = true;
            }

            return info;
        }

        /// <summary>
        /// Gets whether this <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skimageinfo">SKImageInfo</a> instance represents a format with alpha (transparency) without checking
        /// whether <paramref name="imageInfo"/> represents a valid value.
        /// </summary>
        /// <param name="imageInfo">The <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skimageinfo">SKImageInfo</a> to be checked.</param>
        /// <returns><see langword="true"/>, if this <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skimageinfo">SKImageInfo</a> instance represents a format with alpha; otherwise, <see langword="false"/>.</returns>
        public static bool HasAlpha(this SKImageInfo imageInfo)
            => (imageInfo.ColorType is SKColorType.Alpha8 or SKColorType.Alpha16 or SKColorType.AlphaF16)
                || ((imageInfo.AlphaType is SKAlphaType.Unpremul or SKAlphaType.Premul)
                    && (imageInfo.ColorType is SKColorType.Bgra8888 or SKColorType.Rgba8888
                        or SKColorType.Rgba1010102 or SKColorType.Bgra1010102 or SKColorType.Argb4444
                        or SKColorType.RgbaF16 or SKColorType.RgbaF16Clamped or SKColorType.RgbaF32 or SKColorType.Rgba16161616
                        or SKColorType.Srgba8888));

        /// <summary>
        /// Gets a <see cref="PredefinedColorsQuantizer"/> instance that fits for the specified <paramref name="imageInfo"/>.
        /// </summary>
        /// <param name="imageInfo">The <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skimageinfo">SKImageInfo</a> to get a quantizer for.</param>
        /// <param name="backColor">Colors with alpha (transparency), which are considered opaque will be blended with this color before quantization.
        /// The <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcolor.alpha">Alpha</a> property of the background color is ignored. This parameter is optional.
        /// <br/>Default value: The bitwise zero instance of <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcolor">SKColor</a>, which has the same RGB values as <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcolors.black">Black</a>.</param>
        /// <param name="alphaThreshold">Specifies a threshold value for the <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcolor.alpha">Alpha</a> property,
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
                    => PredefinedColorsQuantizer.FromCustomFunction(c => new ColorPargb4444Linear(c.ToPColorF()).ToColor32(), backColor32, alphaThreshold, false),
                (SKColorType.Argb4444, SKAlphaType.Opaque, WorkingColorSpace.Linear)
                    => PredefinedColorsQuantizer.FromCustomFunction(c => new ColorArgb4444Linear(c).ToColor32(), backColor32),
                (SKColorType.Argb4444, SKAlphaType.Unpremul, _)
                    => PredefinedColorsQuantizer.FromCustomFunction(c => new ColorArgb4444Srgb(c).ToColor32(), backColor32, alphaThreshold, false),
                (SKColorType.Argb4444, SKAlphaType.Premul, _)
                    => PredefinedColorsQuantizer.FromCustomFunction(c => new ColorPargb4444Srgb(c.ToPremultiplied()).ToPColor32().ToStraight(), backColor32, alphaThreshold, false),
                (SKColorType.Argb4444, SKAlphaType.Opaque, _)
                    => PredefinedColorsQuantizer.FromCustomFunction(c => new ColorArgb4444Srgb(c).ToColor32(), backColor32),

                // Rgb565: only the linear one because the sRGB one is handled above as known format
                (SKColorType.Rgb565, _, WorkingColorSpace.Linear)
                    => PredefinedColorsQuantizer.FromCustomFunction(c => new ColorRgb565Linear(c).ToColor32(), backColor32),

                // Bgra8888/Rgba8888 with linear color space as they get quantized during the sRGB -> linear conversion so ditherers can improve the result
                (SKColorType.Bgra8888 or SKColorType.Rgba8888, SKAlphaType.Unpremul, WorkingColorSpace.Linear)
                    => PredefinedColorsQuantizer.FromCustomFunction(c => new ColorBgra8888Linear(c).ToColor32(), backColor32, alphaThreshold, false),
                (SKColorType.Bgra8888 or SKColorType.Rgba8888, SKAlphaType.Premul, WorkingColorSpace.Linear)
                    => PredefinedColorsQuantizer.FromCustomFunction(c => new ColorPbgra8888Linear(c.ToPColorF()).ToColor32(), backColor32, alphaThreshold, false),

                // Opaque RG[B] 8bpp/channel color types with linear color spaces: like above
                // NOTE: using the same quantizer for RG88/R8Unorm so no extra compensation will occur for the blue(/green) channel(s)
                (SKColorType.Bgra8888 or SKColorType.Rgba8888, SKAlphaType.Opaque, WorkingColorSpace.Linear)
                    or (SKColorType.Rgb888x or SKColorType.Rg88 or SKColorType.R8Unorm, _, WorkingColorSpace.Linear)
                    => PredefinedColorsQuantizer.FromCustomFunction(c => new ColorBgra8888Linear(c).ToColor32(), backColor32),

                // Gray8 with linear color space as it gets quantized during the sRGB -> linear conversion so ditherers can improve the result
                (SKColorType.Gray8, _, WorkingColorSpace.Linear)
                    => PredefinedColorsQuantizer.FromCustomFunction(c => new ColorGray8Linear(c.ToColorF()).ToColor32(), backColor32, KnownPixelFormat.Format16bppGrayScale),

                // Srgba8888 with sRGB color space as it gets quantized during the sRGB -> "double sRGB" conversion so ditherers can improve the result
                (SKColorType.Srgba8888, SKAlphaType.Unpremul, WorkingColorSpace.Srgb)
                    => PredefinedColorsQuantizer.FromCustomFunction(c => new ColorSrgba8888Srgb(c).ToColor32(), backColor32, alphaThreshold, false),
                (SKColorType.Srgba8888, SKAlphaType.Premul, WorkingColorSpace.Srgb)
                    => PredefinedColorsQuantizer.FromCustomFunction(c => new ColorPsrgba8888Srgb(c.ToPremultiplied()).ToPColor32().ToStraight(), backColor32, alphaThreshold, false),
                (SKColorType.Srgba8888, SKAlphaType.Opaque, WorkingColorSpace.Srgb)
                    => PredefinedColorsQuantizer.FromCustomFunction(c => new ColorSrgba8888Srgb(c).ToColor32(), backColor32),

                // Fallback: some default quantizer from the closest known pixel format
                _ => PredefinedColorsQuantizer.FromPixelFormat(imageInfo.GetInfo().ToKnownPixelFormat(), backColor32, alphaThreshold)
            }).ConfigureColorSpace(imageInfo.GetWorkingColorSpace());
        }

        #endregion

        #region Internal Methods

        internal static KnownPixelFormat AsKnownPixelFormat(this SKImageInfo imageInfo)
        {
            if (imageInfo.ColorSpace?.IsDefaultSrgb() != false)
            {
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
                    SKColorType.Gray8 => (uint)imageInfo.AlphaType <= (uint)ColorExtensions.MaxAlphaType ? KnownPixelFormat.Format8bppGrayScale : KnownPixelFormat.Undefined,
                    _ => KnownPixelFormat.Undefined
                };
            }

            if (imageInfo.ColorSpace.IsDefaultLinear())
            {
                return imageInfo.ColorType switch
                {
                    SKColorType.RgbaF32 => imageInfo.AlphaType switch
                    {
                        SKAlphaType.Unpremul => KnownPixelFormat.Format128bppRgba,
                        SKAlphaType.Premul => KnownPixelFormat.Format128bppPRgba,
                        _ => KnownPixelFormat.Undefined
                    },
                    _ => KnownPixelFormat.Undefined
                };
            }

            return KnownPixelFormat.Undefined;
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
                || imageInfo.ColorType is SKColorType.Rgba8888 or SKColorType.Rgb888x or SKColorType.Bgra8888
                    or SKColorType.Gray8 or SKColorType.Rg88 or SKColorType.R8Unorm
                    && imageInfo.ColorSpace.IsDefaultLinear()
                || imageInfo.ColorType is SKColorType.Srgba8888 && imageInfo.ColorSpace.IsDefaultSrgb();

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
