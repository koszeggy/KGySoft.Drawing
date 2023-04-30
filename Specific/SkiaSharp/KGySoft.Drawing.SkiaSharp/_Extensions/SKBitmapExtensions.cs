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
using System.Diagnostics.CodeAnalysis;
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

        internal static SKPaint CopySourcePaint => copySourcePaint ??= new SKPaint { BlendMode = SKBlendMode.Src };

        #endregion

        #region Methods

        #region Public Methods

        #region Obtaining IBitmapData

        /// <summary>
        /// Gets a managed read-only accessor for an <see cref="SKBitmap"/> instance.
        /// <br/>See the <strong>Remarks</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_GetReadWriteBitmapData.htm">BitmapExtensions.GetReadWriteBitmapData</a>
        /// method for details and code samples. That method is for the GDI+ <a href="https://docs.microsoft.com/en-us/dotnet/api/system.drawing.bitmap" target="_blank">Bitmap</a> type but the main principles apply for this method, too.
        /// </summary>
        /// <param name="bitmap">An <see cref="SKBitmap"/> instance, whose data is about to be accessed.</param>
        /// <param name="backColor">Determines the <see cref="IBitmapData.BackColor"/> property of the result. As SkiaSharp does not support indexed formats
        /// with palette anymore the <paramref name="backColor"/> for the read-only result bitmap data is relevant in very rare cases only, such as cloning by
        /// the <see cref="BitmapDataExtensions.Clone(IReadableBitmapData, KnownPixelFormat, IDitherer?)"/> method or obtaining a quantizer by
        /// the <see cref="PredefinedColorsQuantizer.FromBitmapData"/> method.
        /// The <see cref="SKColor.Alpha"/> property of the specified background color is ignored. This parameter is optional.
        /// <br/>Default value: The bitwise zero instance of <see cref="SKColor"/>, which has the same RGB values as <see cref="SKColors.Black"/>.</param>
        /// <param name="alphaThreshold">Determines the <see cref="IBitmapData.AlphaThreshold"/> property of the result.
        /// Similarly to <paramref name="backColor"/>, for an <see cref="IReadableBitmapData"/> instance the <paramref name="alphaThreshold"/> is relevant
        /// in very rare cases such as cloning the result or obtaining a matching quantizer from it. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <returns>An <see cref="IReadableBitmapData"/> instance, which provides fast read-only access to the actual data of the specified <paramref name="bitmap"/>.</returns>
        /// <seealso cref="SKPixmapExtensions.GetReadableBitmapData(SKPixmap, SKColor, byte)"/>
        /// <seealso cref="SKImageExtensions.GetReadableBitmapData(SKImage, SKColor, byte)"/>
        /// <seealso cref="SKSurfaceExtensions.GetReadableBitmapData(SKSurface, SKColor, byte)"/>
        /// <seealso cref="BitmapDataFactory.CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/>
        public static IReadableBitmapData GetReadableBitmapData(this SKBitmap bitmap, SKColor backColor = default, byte alphaThreshold = 128)
            => bitmap.GetBitmapDataInternal(true, WorkingColorSpace.Default, backColor.ToColor32(), alphaThreshold);

        /// <summary>
        /// Gets a managed read-only accessor for an <see cref="SKBitmap"/> instance.
        /// <br/>See the <strong>Remarks</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_GetReadWriteBitmapData.htm">BitmapExtensions.GetReadWriteBitmapData</a>
        /// method for details and code samples. That method is for the GDI+ <a href="https://docs.microsoft.com/en-us/dotnet/api/system.drawing.bitmap" target="_blank">Bitmap</a> type but the main principles apply for this method, too.
        /// </summary>
        /// <param name="bitmap">An <see cref="SKBitmap"/> instance, whose data is about to be accessed.</param>
        /// <param name="workingColorSpace">Determines the <see cref="IBitmapData.WorkingColorSpace"/> property of the result and
        /// specifies the preferred color space that should be used when working with the result bitmap data. The working color space
        /// can be different from the actual color space of the specified <paramref name="bitmap"/>.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="WorkingColorSpace"/> enumeration for more details.</param>
        /// <param name="backColor">Determines the <see cref="IBitmapData.BackColor"/> property of the result. As SkiaSharp does not support indexed formats
        /// with palette anymore the <paramref name="backColor"/> for the read-only result bitmap data is relevant in very rare cases only, such as cloning by
        /// the <see cref="BitmapDataExtensions.Clone(IReadableBitmapData, KnownPixelFormat, IDitherer?)"/> method or obtaining a quantizer by
        /// the <see cref="PredefinedColorsQuantizer.FromBitmapData"/> method.
        /// The <see cref="SKColor.Alpha"/> property of the specified background color is ignored. This parameter is optional.
        /// <br/>Default value: The bitwise zero instance of <see cref="SKColor"/>, which has the same RGB values as <see cref="SKColors.Black"/>.</param>
        /// <param name="alphaThreshold">Determines the <see cref="IBitmapData.AlphaThreshold"/> property of the result.
        /// Similarly to <paramref name="backColor"/>, for an <see cref="IReadableBitmapData"/> instance the <paramref name="alphaThreshold"/> is relevant
        /// in very rare cases such as cloning the result or obtaining a matching quantizer from it. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <returns>An <see cref="IReadableBitmapData"/> instance, which provides fast read-only access to the actual data of the specified <paramref name="bitmap"/>.</returns>
        /// <seealso cref="SKPixmapExtensions.GetReadableBitmapData(SKPixmap, WorkingColorSpace, SKColor, byte)"/>
        /// <seealso cref="SKImageExtensions.GetReadableBitmapData(SKImage, WorkingColorSpace, SKColor, byte)"/>
        /// <seealso cref="SKSurfaceExtensions.GetReadableBitmapData(SKSurface, WorkingColorSpace, SKColor, byte)"/>
        /// <seealso cref="BitmapDataFactory.CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/>
        public static IReadableBitmapData GetReadableBitmapData(this SKBitmap bitmap, WorkingColorSpace workingColorSpace, SKColor backColor = default, byte alphaThreshold = 128)
            => bitmap.GetBitmapDataInternal(true, workingColorSpace, backColor.ToColor32(), alphaThreshold);

        /// <summary>
        /// Gets a managed write-only accessor for an <see cref="SKBitmap"/> instance.
        /// <br/>See the <strong>Remarks</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_GetReadWriteBitmapData.htm">BitmapExtensions.GetReadWriteBitmapData</a>
        /// method for details and code samples. That method is for the GDI+ <a href="https://docs.microsoft.com/en-us/dotnet/api/system.drawing.bitmap" target="_blank">Bitmap</a> type but the main principles apply for this method, too.
        /// </summary>
        /// <param name="bitmap">An <see cref="SKBitmap"/> instance, whose data is about to be accessed.</param>
        /// <param name="backColor">Determines the <see cref="IBitmapData.BackColor"/> property of the result.
        /// When setting pixels of bitmaps without alpha support, specifies the color of the background.
        /// Color values with alpha, which are considered opaque will be blended with this color before setting the pixel in the result bitmap data.
        /// The <see cref="SKColor.Alpha"/> property of the specified background color is ignored. This parameter is optional.
        /// <br/>Default value: The bitwise zero instance of <see cref="SKColor"/>, which has the same RGB values as <see cref="SKColors.Black"/>.</param>
        /// <param name="alphaThreshold">Determines the <see cref="IBitmapData.AlphaThreshold"/> property of the result.
        /// As SkiaSharp does not support indexed pixel formats with palette anymore, this parameter is relevant in very rare cases only, such as
        /// obtaining a quantizer by the <see cref="PredefinedColorsQuantizer.FromBitmapData"/> method. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <returns>An <see cref="IWritableBitmapData"/> instance, which provides fast write-only access to the actual data of the specified <paramref name="bitmap"/>.</returns>
        /// <seealso cref="SKPixmapExtensions.GetWritableBitmapData(SKPixmap, SKColor, byte)"/>
        /// <seealso cref="SKSurfaceExtensions.GetWritableBitmapData(SKSurface, SKColor, byte)"/>
        /// <seealso cref="BitmapDataFactory.CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/>
        public static IWritableBitmapData GetWritableBitmapData(this SKBitmap bitmap, SKColor backColor = default, byte alphaThreshold = 128)
            => bitmap.GetBitmapDataInternal(false, WorkingColorSpace.Default, backColor.ToColor32(), alphaThreshold, bitmap.NotifyPixelsChanged);

        /// <summary>
        /// Gets a managed write-only accessor for an <see cref="SKBitmap"/> instance.
        /// <br/>See the <strong>Remarks</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_GetReadWriteBitmapData.htm">BitmapExtensions.GetReadWriteBitmapData</a>
        /// method for details and code samples. That method is for the GDI+ <a href="https://docs.microsoft.com/en-us/dotnet/api/system.drawing.bitmap" target="_blank">Bitmap</a> type but the main principles apply for this method, too.
        /// </summary>
        /// <param name="bitmap">An <see cref="SKBitmap"/> instance, whose data is about to be accessed.</param>
        /// <param name="workingColorSpace">Determines the <see cref="IBitmapData.WorkingColorSpace"/> property of the result and
        /// specifies the preferred color space that should be used when working with the result bitmap data. The working color space
        /// can be different from the actual color space of the specified <paramref name="bitmap"/>.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="WorkingColorSpace"/> enumeration for more details.</param>
        /// <param name="backColor">Determines the <see cref="IBitmapData.BackColor"/> property of the result.
        /// When setting pixels of bitmaps without alpha support, specifies the color of the background.
        /// Color values with alpha, which are considered opaque will be blended with this color before setting the pixel in the result bitmap data.
        /// The <see cref="SKColor.Alpha"/> property of the specified background color is ignored. This parameter is optional.
        /// <br/>Default value: The bitwise zero instance of <see cref="SKColor"/>, which has the same RGB values as <see cref="SKColors.Black"/>.</param>
        /// <param name="alphaThreshold">Determines the <see cref="IBitmapData.AlphaThreshold"/> property of the result.
        /// As SkiaSharp does not support indexed pixel formats with palette anymore, this parameter is relevant in very rare cases only, such as
        /// obtaining a quantizer by the <see cref="PredefinedColorsQuantizer.FromBitmapData"/> method. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <returns>An <see cref="IWritableBitmapData"/> instance, which provides fast write-only access to the actual data of the specified <paramref name="bitmap"/>.</returns>
        /// <seealso cref="SKPixmapExtensions.GetWritableBitmapData(SKPixmap, WorkingColorSpace, SKColor, byte)"/>
        /// <seealso cref="SKSurfaceExtensions.GetWritableBitmapData(SKSurface, WorkingColorSpace, SKColor, byte)"/>
        /// <seealso cref="BitmapDataFactory.CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/>
        public static IWritableBitmapData GetWritableBitmapData(this SKBitmap bitmap, WorkingColorSpace workingColorSpace, SKColor backColor = default, byte alphaThreshold = 128)
            => bitmap.GetBitmapDataInternal(false, workingColorSpace, backColor.ToColor32(), alphaThreshold, bitmap.NotifyPixelsChanged);

        /// <summary>
        /// Gets a managed read-write accessor for an <see cref="SKBitmap"/> instance.
        /// <br/>See the <strong>Remarks</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_GetReadWriteBitmapData.htm">BitmapExtensions.GetReadWriteBitmapData</a>
        /// method for details and code samples. That method is for the GDI+ <a href="https://docs.microsoft.com/en-us/dotnet/api/system.drawing.bitmap" target="_blank">Bitmap</a> type but the main principles apply for this method, too.
        /// </summary>
        /// <param name="bitmap">An <see cref="SKBitmap"/> instance, whose data is about to be accessed.</param>
        /// <param name="backColor">Determines the <see cref="IBitmapData.BackColor"/> property of the result.
        /// When setting pixels of bitmaps without alpha support, specifies the color of the background.
        /// Color values with alpha, which are considered opaque will be blended with this color before setting the pixel in the result bitmap data.
        /// The <see cref="SKColor.Alpha"/> property of the specified background color is ignored. This parameter is optional.
        /// <br/>Default value: The bitwise zero instance of <see cref="SKColor"/>, which has the same RGB values as <see cref="SKColors.Black"/>.</param>
        /// <param name="alphaThreshold">Determines the <see cref="IBitmapData.AlphaThreshold"/> property of the result.
        /// Can be relevant is some operations such as when drawing an <see cref="IReadableBitmapData"/> instance with alpha in the returned instance
        /// by the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.DrawInto">DrawInto</see> extensions and the specified <paramref name="bitmap"/>
        /// has no alpha support. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance, which provides fast read-write access to the actual data of the specified <paramref name="bitmap"/>.</returns>
        /// <seealso cref="SKPixmapExtensions.GetReadWriteBitmapData(SKPixmap, SKColor, byte)"/>
        /// <seealso cref="SKSurfaceExtensions.GetReadWriteBitmapData(SKSurface, SKColor, byte)"/>
        /// <seealso cref="BitmapDataFactory.CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/>
        public static IReadWriteBitmapData GetReadWriteBitmapData(this SKBitmap bitmap, SKColor backColor = default, byte alphaThreshold = 128)
            => bitmap.GetBitmapDataInternal(false, WorkingColorSpace.Default, backColor.ToColor32(), alphaThreshold, bitmap.NotifyPixelsChanged);

        /// <summary>
        /// Gets a managed read-write accessor for an <see cref="SKBitmap"/> instance.
        /// <br/>See the <strong>Remarks</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_GetReadWriteBitmapData.htm">BitmapExtensions.GetReadWriteBitmapData</a>
        /// method for details and code samples. That method is for the GDI+ <a href="https://docs.microsoft.com/en-us/dotnet/api/system.drawing.bitmap" target="_blank">Bitmap</a> type but the main principles apply for this method, too.
        /// </summary>
        /// <param name="bitmap">An <see cref="SKBitmap"/> instance, whose data is about to be accessed.</param>
        /// <param name="workingColorSpace">Determines the <see cref="IBitmapData.WorkingColorSpace"/> property of the result and
        /// specifies the preferred color space that should be used when working with the result bitmap data. The working color space
        /// can be different from the actual color space of the specified <paramref name="bitmap"/>.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="WorkingColorSpace"/> enumeration for more details.</param>
        /// <param name="backColor">Determines the <see cref="IBitmapData.BackColor"/> property of the result.
        /// When setting pixels of bitmaps without alpha support, specifies the color of the background.
        /// Color values with alpha, which are considered opaque will be blended with this color before setting the pixel in the result bitmap data.
        /// The <see cref="SKColor.Alpha"/> property of the specified background color is ignored. This parameter is optional.
        /// <br/>Default value: The bitwise zero instance of <see cref="SKColor"/>, which has the same RGB values as <see cref="SKColors.Black"/>.</param>
        /// <param name="alphaThreshold">Determines the <see cref="IBitmapData.AlphaThreshold"/> property of the result.
        /// Can be relevant is some operations such as when drawing an <see cref="IReadableBitmapData"/> instance with alpha in the returned instance
        /// by the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.DrawInto">DrawInto</see> extensions and the specified <paramref name="bitmap"/>
        /// has no alpha support. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance, which provides fast read-write access to the actual data of the specified <paramref name="bitmap"/>.</returns>
        /// <seealso cref="SKPixmapExtensions.GetReadWriteBitmapData(SKPixmap, WorkingColorSpace, SKColor, byte)"/>
        /// <seealso cref="SKSurfaceExtensions.GetReadWriteBitmapData(SKSurface, WorkingColorSpace, SKColor, byte)"/>
        /// <seealso cref="BitmapDataFactory.CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/>
        public static IReadWriteBitmapData GetReadWriteBitmapData(this SKBitmap bitmap, WorkingColorSpace workingColorSpace, SKColor backColor = default, byte alphaThreshold = 128)
            => bitmap.GetBitmapDataInternal(false, workingColorSpace, backColor.ToColor32(), alphaThreshold, bitmap.NotifyPixelsChanged);

        #endregion

        #region ConvertPixelFormat

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

        [SuppressMessage("ReSharper", "ParameterOnlyUsedForPreconditionCheck.Local", Justification = "Validation method")]
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
