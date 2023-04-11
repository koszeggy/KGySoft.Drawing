#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: SKBitmapExtensions.cs
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
using System.Drawing;
using System.Threading.Tasks;

using KGySoft.CoreLibraries;
using KGySoft.Drawing.Imaging;
using KGySoft.Threading;

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

        private static SKPaint? copySourcePaint;

        #endregion

        #region Properties

        private static SKPaint CopySourcePaint => copySourcePaint ??= new SKPaint { BlendMode = SKBlendMode.Src };

        #endregion

        #region Methods

        #region Public Methods

        // TODO: add backColor/alphaThreshold. Used at PredefinedColorsQuantizer.FromBitmapData, for example.
        public static IReadableBitmapData GetReadableBitmapData(this SKBitmap bitmap, WorkingColorSpace workingColorSpace = WorkingColorSpace.Default) => bitmap.GetBitmapDataInternal(true, workingColorSpace);

        public static IWritableBitmapData GetWritableBitmapData(this SKBitmap bitmap, SKColor backColor = default, byte alphaThreshold = 128)
            => bitmap.GetBitmapDataInternal(false, WorkingColorSpace.Default, backColor.ToColor32(), alphaThreshold, bitmap.NotifyPixelsChanged);

        public static IWritableBitmapData GetWritableBitmapData(this SKBitmap bitmap, WorkingColorSpace workingColorSpace, SKColor backColor = default, byte alphaThreshold = 128)
            => bitmap.GetBitmapDataInternal(false, workingColorSpace, backColor.ToColor32(), alphaThreshold, bitmap.NotifyPixelsChanged);

        public static IReadWriteBitmapData GetReadWriteBitmapData(this SKBitmap bitmap, SKColor backColor = default, byte alphaThreshold = 128)
            => bitmap.GetBitmapDataInternal(false, WorkingColorSpace.Default, backColor.ToColor32(), alphaThreshold, bitmap.NotifyPixelsChanged);

        /// <param name="alphaThreshold">Relevant only when another bitmap is drawn into this one and this bitmap has no alpha support. See <see cref="BitmapDataFactory.CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/></param>
        public static IReadWriteBitmapData GetReadWriteBitmapData(this SKBitmap bitmap, WorkingColorSpace workingColorSpace, SKColor backColor = default, byte alphaThreshold = 128)
            => bitmap.GetBitmapDataInternal(false, workingColorSpace, backColor.ToColor32(), alphaThreshold, bitmap.NotifyPixelsChanged);

        public static SKBitmap ConvertPixelFormat(this SKBitmap bitmap, SKColorType colorType, SKAlphaType alphaType = SKAlphaType.Unknown,
            WorkingColorSpace targetColorSpace = WorkingColorSpace.Default, SKColor backColor = default, byte alphaThreshold = 128)
        {
            ValidateArguments(bitmap, colorType, alphaType, targetColorSpace);
            return DoConvertPixelFormat(AsyncHelper.DefaultContext, bitmap, GetImageInfo(bitmap, colorType, alphaType, targetColorSpace), backColor.ToColor32(), alphaThreshold)!;
        }

        public static SKBitmap ConvertPixelFormat(this SKBitmap bitmap, IQuantizer? quantizer, IDitherer? ditherer = null,
            SKColorType colorType = SKColorType.Unknown, SKAlphaType alphaType = SKAlphaType.Unknown, WorkingColorSpace targetColorSpace = WorkingColorSpace.Default)
        {
            ValidateArguments(bitmap, colorType, alphaType, targetColorSpace);
            return DoConvertPixelFormat(AsyncHelper.DefaultContext, bitmap, GetImageInfo(bitmap, colorType, alphaType, targetColorSpace), quantizer, ditherer)!;
        }

        public static Task<SKBitmap?> ConvertPixelFormatAsync(this SKBitmap bitmap, SKColorType colorType, SKAlphaType alphaType = SKAlphaType.Unknown,
            WorkingColorSpace targetColorSpace = WorkingColorSpace.Default, SKColor backColor = default, byte alphaThreshold = 128, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmap, colorType, alphaType, targetColorSpace);
            return AsyncHelper.DoOperationAsync(ctx => DoConvertPixelFormat(ctx, bitmap, GetImageInfo(bitmap, colorType, alphaType, targetColorSpace), backColor.ToColor32(), alphaThreshold), asyncConfig);
        }

        public static Task<SKBitmap?> ConvertPixelFormatAsync(this SKBitmap bitmap, IQuantizer? quantizer, IDitherer? ditherer = null,
            SKColorType colorType = SKColorType.Unknown, SKAlphaType alphaType = SKAlphaType.Unknown, WorkingColorSpace targetColorSpace = WorkingColorSpace.Default, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmap, colorType, alphaType, targetColorSpace);
            return AsyncHelper.DoOperationAsync(ctx => DoConvertPixelFormat(ctx, bitmap, GetImageInfo(bitmap, colorType, alphaType, targetColorSpace), quantizer, ditherer), asyncConfig);
        }

        #endregion

        #region Internal Methods

        internal static IReadWriteBitmapData GetBitmapDataInternal(this SKBitmap bitmap, bool readOnly, WorkingColorSpace workingColorSpace, Color32 backColor = default, byte alphaThreshold = 128, Action? disposeCallback = null)
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

        internal static IReadWriteBitmapData GetFallbackBitmapData(this SKBitmap bitmap, bool readOnly, WorkingColorSpace workingColorSpace, Color32 backColor = default, byte alphaThreshold = 128, Action? disposeCallback = null)
        {
            Debug.Assert(!bitmap.Info.IsDirectlySupported() && bitmap.ColorSpace != null);
            SKImageInfo info = bitmap.Info;

            var colorType = info.ColorType switch
            {
                // For the fastest native support
                SKColorType.Rgba8888 => SKColorType.Bgra8888,

                // Supported custom formats
                > SKColorType.Unknown and ColorExtensions.MaxColorType => info.ColorType,

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

        #region Private Methods

        private static void ValidateArguments(SKBitmap bitmap, SKColorType colorType, SKAlphaType alphaType, WorkingColorSpace targetColorSpace)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap), PublicResources.ArgumentNull);
            if (!colorType.IsDefined())
                throw new ArgumentOutOfRangeException(nameof(colorType), PublicResources.EnumOutOfRange(colorType));
            if (!alphaType.IsDefined())
                throw new ArgumentOutOfRangeException(nameof(alphaType), PublicResources.EnumOutOfRange(alphaType));
            if (!targetColorSpace.IsDefined())
                throw new ArgumentOutOfRangeException(nameof(targetColorSpace), PublicResources.EnumOutOfRange(targetColorSpace));
        }

        private static SKBitmap? DoConvertPixelFormat(IAsyncContext context, SKBitmap bitmap, SKImageInfo imageInfo, Color32 backColor, byte alphaThreshold)
        {
            if (context.IsCancellationRequested)
                return null;

            using IReadableBitmapData source = bitmap.GetReadableBitmapData();
            return source.ToSKBitmap(context, imageInfo, backColor, alphaThreshold);
        }

        private static SKBitmap? DoConvertPixelFormat(IAsyncContext context, SKBitmap bitmap, SKImageInfo imageInfo, IQuantizer? quantizer, IDitherer? ditherer)
        {
            if (context.IsCancellationRequested)
                return null;

            using IReadableBitmapData source = bitmap.GetReadableBitmapData();
            return source.ToSKBitmap(context, imageInfo, quantizer, ditherer);
        }

        private static SKImageInfo GetImageInfo(SKBitmap bitmap, SKColorType colorType, SKAlphaType alphaType, WorkingColorSpace targetColorSpace)
        {
            SKImageInfo result = bitmap.Info;
            if (colorType != SKColorType.Unknown)
                result.ColorType = colorType;
            if (alphaType != SKAlphaType.Unknown)
                result.AlphaType = alphaType;

            result.ColorSpace = targetColorSpace != WorkingColorSpace.Default
                ? targetColorSpace == WorkingColorSpace.Linear ? SKColorSpace.CreateSrgbLinear() : SKColorSpace.CreateSrgb()
                : bitmap.ColorSpace.IsDefaultLinear() ? SKColorSpace.CreateSrgbLinear() : SKColorSpace.CreateSrgb();

            return result;
        }

        #endregion

        #endregion
    }
}
