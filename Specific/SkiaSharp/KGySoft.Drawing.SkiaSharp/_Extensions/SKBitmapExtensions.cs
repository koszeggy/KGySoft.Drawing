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

        public static IReadWriteBitmapData GetReadableBitmapData(this SKBitmap bitmap) => bitmap.GetBitmapDataInternal(true);

        public static IReadWriteBitmapData GetWritableBitmapData(this SKBitmap bitmap, SKColor backColor = default, byte alphaThreshold = 128)
            => bitmap.GetBitmapDataInternal(false, backColor, alphaThreshold, bitmap.NotifyPixelsChanged);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="backColor"></param>
        /// <param name="alphaThreshold">Relevant only when another bitmap is drawn into this one and this bitmap has no alpha support. See <see cref="BitmapDataFactory.CreateBitmapData(Size, KnownPixelFormat, Color32, byte)"/></param>
        /// <returns></returns>
        public static IReadWriteBitmapData GetReadWriteBitmapData(this SKBitmap bitmap, SKColor backColor = default, byte alphaThreshold = 128)
            => bitmap.GetBitmapDataInternal(false, backColor, alphaThreshold, bitmap.NotifyPixelsChanged);

        #endregion

        #region Internal Methods

        internal static IReadWriteBitmapData GetBitmapDataInternal(this SKBitmap bitmap, bool readOnly, SKColor backColor = default, byte alphaThreshold = 128, Action? disposeCallback = null)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap), PublicResources.ArgumentNull);
            SKImageInfo imageInfo = bitmap.Info;

            return imageInfo.IsDirectlySupported()
                ? NativeBitmapDataFactory.CreateBitmapData(bitmap.GetPixels(), imageInfo, bitmap.RowBytes, backColor, alphaThreshold, disposeCallback)
                : bitmap.GetFallbackBitmapData(readOnly, backColor, alphaThreshold, disposeCallback);
        }

        internal static IReadWriteBitmapData GetFallbackBitmapData(this SKBitmap bitmap, bool readOnly, SKColor backColor = default, byte alphaThreshold = 128, Action? disposeCallback = null)
        {
            Debug.Assert(!bitmap.Info.IsDirectlySupported());
            SKImageInfo info = bitmap.Info;

            Action<ICustomBitmapDataRow, int, Color32> rowSetColor;
            if (readOnly)
                rowSetColor = (_, _, _) => { };
            else
            {
                // Though we could use bitmap.SetPixel, it would be slower as it creates and disposes a canvas for each pixel
                var canvas = new SKCanvas(bitmap);
                rowSetColor = (row, x, c) => canvas.DrawPoint(x, row.Index, c.ToSKColor());
                Action? callerDispose = disposeCallback;
                disposeCallback = callerDispose == null
                    ? canvas.Dispose
                    : () =>
                    {
                        canvas.Dispose();
                        callerDispose.Invoke();
                    };
            }

            return BitmapDataFactory.CreateBitmapData(bitmap.GetPixels(), new Size(info.Width, info.Height), info.RowBytes, info.GetInfo(),
                (row, x) => bitmap.GetPixel(x, row.Index).ToColor32(), rowSetColor,
                backColor.ToColor32(), alphaThreshold, disposeCallback);
        }

        #endregion

        #endregion
    }
}
