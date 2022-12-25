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
using System.Collections.Generic;

using KGySoft.CoreLibraries;
using KGySoft.Drawing.Imaging;

using SkiaSharp;

#endregion

namespace KGySoft.Drawing.SkiaSharp
{
    public static class SKImageInfoExtensions
    {
        #region Fields

        private static readonly HashSet<(SKColorType, SKAlphaType)> directlySupportedCustomFormats = new()
        {
            //(SKColorType.Bgra8888, SKAlphaType.Opaque),
        };

        #endregion

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
                // these types have alpha even with AlphaType.Oqaque
                case SKColorType.Alpha8:
                case SKColorType.Alpha16:
                case SKColorType.Bgra8888: // Even Opaque can have alpha: sets as Premul, reads raw value
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
            if (imageInfo.ColorSpace != null || imageInfo.AlphaType == SKAlphaType.Unknown)
                return KnownPixelFormat.Undefined;

            return imageInfo.ColorType switch
            {
                SKColorType.Bgra8888 => imageInfo.AlphaType switch
                {
                    SKAlphaType.Unpremul => KnownPixelFormat.Format32bppArgb,
                    SKAlphaType.Premul => KnownPixelFormat.Format32bppPArgb,
                    _ => KnownPixelFormat.Undefined, // Bgra8888/Opaque: sets as Premul, reads raw value
                },
                _ => KnownPixelFormat.Undefined
            };
        }

        internal static bool IsDirectlySupported(this SKImageInfo imageInfo)
            => imageInfo.ColorSpace == null
                && (imageInfo.AsKnownPixelFormat() != KnownPixelFormat.Undefined || directlySupportedCustomFormats.Contains((imageInfo.ColorType, imageInfo.AlphaType)));

        #endregion

        #endregion
    }
}
