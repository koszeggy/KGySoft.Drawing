#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: SKImageInfoExtensions.cs
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

using System;
using System.Diagnostics.CodeAnalysis;

using KGySoft.CoreLibraries;
using KGySoft.Drawing.Imaging;

using SkiaSharp;

#endregion

namespace KGySoft.Drawing.SkiaSharp
{
    public static class SKImageInfoExtensions
    {
        #region Methods

        #region Public Methods

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
                    info.HasPremultipliedAlpha = true;
                    break;
                case SKAlphaType.Unpremul:
                    info.HasAlpha = true;
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

            return info;
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
                SKColorType.Rgb565 => imageInfo.AlphaType == SKAlphaType.Opaque ? KnownPixelFormat.Format16bppRgb565 : KnownPixelFormat.Undefined,
                _ => KnownPixelFormat.Undefined
            };
        }

        [SuppressMessage("ReSharper", "AssignmentInConditionalExpression", Justification = "Intended")]
        internal static void GetDirectlySupportedColorSpace(this SKImageInfo imageInfo, out bool srgb, out bool linear)
        {
            srgb = false;
            linear = false;

            if (imageInfo.ColorType is <= SKColorType.Unknown or > SKColorType.Bgr101010x
                || imageInfo.AlphaType is <= SKAlphaType.Unknown or > SKAlphaType.Unpremul)
            {
                return;
            }

            if (srgb = imageInfo.ColorSpace.IsDefaultSrgb())
                return;

            linear = imageInfo.ColorSpace.IsDefaultLinear();
        }

        internal static bool IsDirectlySupported(this SKImageInfo imageInfo)
            => imageInfo.ColorType is > SKColorType.Unknown and <= SKColorType.Bgr101010x
                && imageInfo.AlphaType is > SKAlphaType.Unknown and <= SKAlphaType.Unpremul
                && (imageInfo.ColorSpace.IsDefaultSrgb() || imageInfo.ColorSpace.IsDefaultLinear());

        #endregion

        #endregion
    }
}
