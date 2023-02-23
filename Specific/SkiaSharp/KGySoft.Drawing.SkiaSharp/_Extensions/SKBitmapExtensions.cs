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
        #region Fields

        private static SKPaint? _copySourcePaint;

        #endregion

        #region Properties

        private static SKPaint CopySourcePaint => _copySourcePaint ??= new SKPaint { BlendMode = SKBlendMode.Src };

        #endregion

        #region Methods

        #region Public Methods

        public static IReadWriteBitmapData GetReadableBitmapData(this SKBitmap bitmap, WorkingColorSpace workingColorSpace = WorkingColorSpace.Default) => bitmap.GetBitmapDataInternal(true, workingColorSpace);

        public static IReadWriteBitmapData GetWritableBitmapData(this SKBitmap bitmap, SKColor backColor = default, byte alphaThreshold = 128)
            => bitmap.GetBitmapDataInternal(false, WorkingColorSpace.Default, backColor, alphaThreshold, bitmap.NotifyPixelsChanged);

        public static IReadWriteBitmapData GetWritableBitmapData(this SKBitmap bitmap, WorkingColorSpace workingColorSpace, SKColor backColor = default, byte alphaThreshold = 128)
            => bitmap.GetBitmapDataInternal(false, workingColorSpace, backColor, alphaThreshold, bitmap.NotifyPixelsChanged);

        public static IReadWriteBitmapData GetReadWriteBitmapData(this SKBitmap bitmap, SKColor backColor = default, byte alphaThreshold = 128)
            => bitmap.GetBitmapDataInternal(false, WorkingColorSpace.Default, backColor, alphaThreshold, bitmap.NotifyPixelsChanged);

        /// <param name="alphaThreshold">Relevant only when another bitmap is drawn into this one and this bitmap has no alpha support. See <see cref="BitmapDataFactory.CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/></param>
        public static IReadWriteBitmapData GetReadWriteBitmapData(this SKBitmap bitmap, WorkingColorSpace workingColorSpace, SKColor backColor = default, byte alphaThreshold = 128)
            => bitmap.GetBitmapDataInternal(false, workingColorSpace, backColor, alphaThreshold, bitmap.NotifyPixelsChanged);

        #endregion

        #region Internal Methods

        internal static IReadWriteBitmapData GetBitmapDataInternal(this SKBitmap bitmap, bool readOnly, WorkingColorSpace workingColorSpace, SKColor backColor = default, byte alphaThreshold = 128, Action? disposeCallback = null)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap), PublicResources.ArgumentNull);
            if (workingColorSpace is < WorkingColorSpace.Default or > WorkingColorSpace.Srgb)
                throw new ArgumentOutOfRangeException(nameof(workingColorSpace), PublicResources.EnumOutOfRange(workingColorSpace));
            SKImageInfo imageInfo = bitmap.Info;

            return NativeBitmapDataFactory.TryCreateBitmapData(bitmap.GetPixels(), imageInfo, bitmap.RowBytes, backColor, alphaThreshold, workingColorSpace, disposeCallback, out IReadWriteBitmapData? result)
                ? result
                : bitmap.GetFallbackBitmapData(readOnly, workingColorSpace, backColor, alphaThreshold, disposeCallback);
        }

        internal static IReadWriteBitmapData GetFallbackBitmapData(this SKBitmap bitmap, bool readOnly, WorkingColorSpace workingColorSpace, SKColor backColor = default, byte alphaThreshold = 128, Action? disposeCallback = null)
        {
            Debug.Assert(!bitmap.Info.IsDirectlySupported() && bitmap.ColorSpace != null);
            SKImageInfo info = bitmap.Info;

            var colorType = info.ColorType switch
            {
                // For the fastest native support
                SKColorType.Rgba8888 => SKColorType.Bgra8888,

                // Supported custom formats
                > SKColorType.Unknown and <= SKColorType.Bgr101010x => info.ColorType,

                // Unsupported formats (future compatibility)
                _ => info.ColorType.GetBytesPerPixel() switch
                {
                    > 8 => SKColorType.RgbaF32,
                    > 4 => SKColorType.Rgba16161616,
                    _ => SKColorType.Bgra8888
                }
            };

            var tempBitmapInfo = new SKImageInfo(info.Width, info.Height, colorType, info.AlphaType,
                workingColorSpace == WorkingColorSpace.Linear || workingColorSpace == WorkingColorSpace.Default && bitmap.ColorSpace!.GammaIsLinear ? SKColorSpace.CreateSrgbLinear() : SKColorSpace.CreateSrgb());

            // We could use bitmap.SetPixel/GetPixel as custom handling but it has two issues:
            // - The getter/setter would contain a reference back to the original bitmap, which is not allowed (eg. ruins clone or the fallback quantizer)
            // - SKBitmap.GetPixel fails to return valid colors for non-sRGB images: https://github.com/mono/SkiaSharp/issues/2354
            // Therefore we create a new temp bitmap, which can be handled natively.
            // For non read-only access this is copied back to the original instance in Dispose
            var tempBitmap = new SKBitmap(tempBitmapInfo);
            using (var canvas = new SKCanvas(tempBitmap))
                canvas.DrawBitmap(bitmap, 0, 0, CopySourcePaint);

            return tempBitmap.GetBitmapDataInternal(readOnly, workingColorSpace, backColor, alphaThreshold, () =>
            {
                if (!readOnly)
                {
                    using var canvas = new SKCanvas(bitmap);
                    canvas.DrawBitmap(tempBitmap, 0, 0, CopySourcePaint);
                }

                tempBitmap.Dispose();
                disposeCallback?.Invoke();
            });
        }

        #endregion

        #endregion
    }
}
