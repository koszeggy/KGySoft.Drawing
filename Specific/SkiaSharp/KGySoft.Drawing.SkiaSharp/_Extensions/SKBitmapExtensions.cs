#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: SKBitmapExtensions.cs
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
using System.Drawing;

using KGySoft.Drawing.Imaging;

using SkiaSharp;

#endregion

namespace KGySoft.Drawing.SkiaSharp
{
    /// <summary>
    /// Contains extension methods for the <see cref="SKBitmap"/> type.
    /// </summary>
    public static class SKBitmapExtensions
    {
        #region Methods

        #region Public Methods

        public static IReadWriteBitmapData GetReadableBitmapData(this SKBitmap bitmap) => bitmap.GetBitmapDataInternal();

        public static IReadWriteBitmapData GetWritableBitmapData(this SKBitmap bitmap, SKColor backColor = default, byte alphaThreshold = 128)
            => bitmap.GetBitmapDataInternal(backColor, alphaThreshold, bitmap.NotifyPixelsChanged);

        public static IReadWriteBitmapData GetReadWriteBitmapData(this SKBitmap bitmap, SKColor backColor = default, byte alphaThreshold = 128)
            => bitmap.GetBitmapDataInternal(backColor, alphaThreshold, bitmap.NotifyPixelsChanged);

        #endregion

        #region Internal Methods

        internal static IReadWriteBitmapData GetBitmapDataInternal(this SKBitmap bitmap, SKColor backColor = default, byte alphaThreshold = 128, Action? disposeCallback = null)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap), PublicResources.ArgumentNull);
            SKImageInfo imageInfo = bitmap.Info;

            return imageInfo.IsDirectlySupported()
                ? NativeBitmapDataFactory.CreateBitmapData(bitmap.GetPixels(), imageInfo, backColor, alphaThreshold, disposeCallback)
                : bitmap.GetFallbackBitmapData(backColor, alphaThreshold, disposeCallback);
        }

        internal static IReadWriteBitmapData GetFallbackBitmapData(this SKBitmap bitmap, SKColor backColor = default, byte alphaThreshold = 128, Action? disposeCallback = null)
        {
            Debug.Assert(!bitmap.Info.IsDirectlySupported());
            SKImageInfo info = bitmap.Info;

            return BitmapDataFactory.CreateBitmapData(bitmap.GetPixels(), new Size(info.Width, info.Height), info.RowBytes, info.GetInfo(),
                (row, x) => bitmap.GetPixel(x, row.Index).ToColor32(),
                (row, x, c) => bitmap.SetPixel(x, row.Index, c.ToSKColor()), backColor.ToColor32(), alphaThreshold, disposeCallback);
        }

        #endregion

        #endregion
    }
}
