#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: SKPixmapExtensions.cs
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

using KGySoft.Drawing.Imaging;

using SkiaSharp;

#endregion

namespace KGySoft.Drawing.SkiaSharp
{
    /// <summary>
    /// Contains extension methods for the <see cref="SKPixmap"/> type.
    /// </summary>
    public static class SKPixmapExtensions
    {
        #region Methods

        #region Public Methods

        public static IReadableBitmapData GetReadableBitmapData(this SKPixmap pixels) => pixels.GetBitmapDataInternal();

        public static IReadWriteBitmapData GetWritableBitmapData(this SKPixmap pixels, SKColor backColor = default, byte alphaThreshold = 128)
            => pixels.GetBitmapDataInternal(backColor, alphaThreshold);

        public static IReadWriteBitmapData GetReadWriteBitmapData(this SKPixmap pixels, SKColor backColor = default, byte alphaThreshold = 128)
            => pixels.GetBitmapDataInternal(backColor, alphaThreshold);

        #endregion

        #region Internal Methods

        internal static IReadWriteBitmapData GetBitmapDataInternal(this SKPixmap pixels, SKColor backColor = default, byte alphaThreshold = 128, Action? disposeCallback = null)
        {
            if (pixels == null)
                throw new ArgumentNullException(nameof(pixels), PublicResources.ArgumentNull);
            SKImageInfo imageInfo = pixels.Info;
            if (imageInfo.IsEmpty)
                throw new ArgumentException(PublicResources.ArgumentEmpty, nameof(pixels));

            // shortcut: if pixel format is directly supported, then we can simply create a bitmap data for its back buffer
            if (imageInfo.IsDirectlySupported())
                return NativeBitmapDataFactory.CreateBitmapData(pixels.GetPixels(), imageInfo, backColor, alphaThreshold, disposeCallback);

            // otherwise, we create an SKBitmap for it, so the fallback manipulation can be used
            var bitmap = new SKBitmap();
            if (!bitmap.InstallPixels(pixels))
            {
                bitmap.Dispose();
                disposeCallback?.Invoke();
                throw new ArgumentException(PublicResources.ArgumentInvalid, nameof(pixels));
            }

            Action disposeBitmap = disposeCallback == null
                ? bitmap.Dispose
                : () =>
                {
                    bitmap.Dispose();
                    disposeCallback();
                };
            return bitmap.GetFallbackBitmapData(backColor, alphaThreshold, disposeBitmap);
        }

        #endregion

        #endregion
    }
}
