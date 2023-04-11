#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ReadableBitmapDataExtensions.cs
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
    /// Provides extension methods for the <see cref="IReadableBitmapData"/> type.
    /// </summary>
    public static class ReadableBitmapDataExtensions
    {
        #region Methods

        #region Public Methods

        public static SKBitmap ToSKBitmap(this IReadableBitmapData source)
        {
            ValidateArguments(source);
            return DoConvertToSKBitmapDirect(AsyncHelper.DefaultContext, source, GetCompatibleImageInfo(source), source.BackColor, source.AlphaThreshold)!;
        }

        // TODO: overloads without alphaType/colorSpace
        // targetColorSpace: not an SKColorSpace because only sRGB/Linear color spaces are supported directly and not SKColorSpaceRenderTargetGamma because it is obsolete.
        //                   NOTE: It determines both the actual SKColorSpace of the result AND also the working color space.
        //                         To create eg. an sRGB result while working with linear color space create the desired result, use GetWritableBitmapData + CopyTo
        public static SKBitmap ToSKBitmap(this IReadableBitmapData source, SKColorType colorType, SKAlphaType alphaType = SKAlphaType.Unknown, WorkingColorSpace targetColorSpace = WorkingColorSpace.Default, IQuantizer? quantizer = null, IDitherer? ditherer = null)
        {
            ValidateArguments(source, colorType, alphaType, targetColorSpace);
            if (targetColorSpace == WorkingColorSpace.Default)
                targetColorSpace = source.GetPreferredColorSpace();
            
            return DoConvertToSKBitmapByQuantizer(AsyncHelper.DefaultContext, source,
                GetImageInfo(source, colorType, alphaType, targetColorSpace), quantizer, ditherer)!;
        }

        public static Task<SKBitmap?> ToSKBitmapAsync(this IReadableBitmapData source, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(source);
            return AsyncHelper.DoOperationAsync(ctx => DoConvertToSKBitmapDirect(ctx, source, GetCompatibleImageInfo(source), source.BackColor, source.AlphaThreshold), asyncConfig);
        }

        public static Task<SKBitmap?> ToSKBitmapAsync(this IReadableBitmapData source, SKColorType colorType, SKAlphaType alphaType = SKAlphaType.Unknown, WorkingColorSpace targetColorSpace = WorkingColorSpace.Default, IQuantizer? quantizer = null, IDitherer? ditherer = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(source, colorType, alphaType, targetColorSpace);
            if (targetColorSpace == WorkingColorSpace.Default)
                targetColorSpace = source.GetPreferredColorSpace();

            return AsyncHelper.DoOperationAsync(ctx => DoConvertToSKBitmapByQuantizer(ctx, source,
                GetImageInfo(source, colorType, alphaType, targetColorSpace), quantizer, ditherer), asyncConfig);
        }

        #endregion

        #region Internal Methods

        internal static SKBitmap? ToSKBitmap(this IReadableBitmapData source, IAsyncContext context, SKImageInfo imageInfo, Color32 backColor, byte alphaThreshold)
            => DoConvertToSKBitmapDirect(context, source, imageInfo, backColor, alphaThreshold);

        internal static SKBitmap? ToSKBitmap(this IReadableBitmapData source, IAsyncContext context, SKImageInfo imageInfo, IQuantizer? quantizer, IDitherer? ditherer)
            => DoConvertToSKBitmapByQuantizer(context, source, imageInfo, quantizer, ditherer);

        #endregion

        #region Private Methods

        private static void ValidateArguments(IReadableBitmapData source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source), PublicResources.ArgumentNull);
            if (source.Width <= 0 || source.Height <= 0)
                throw new ArgumentException(Res.ImagingInvalidBitmapDataSize, nameof(source));
        }

        private static void ValidateArguments(IReadableBitmapData source, SKColorType colorType, SKAlphaType alphaType, WorkingColorSpace targetColorSpace)
        {
            ValidateArguments(source);
            if (!colorType.IsDefined())
                throw new ArgumentOutOfRangeException(nameof(colorType), PublicResources.EnumOutOfRange(colorType));
            if (!alphaType.IsDefined())
                throw new ArgumentOutOfRangeException(nameof(alphaType), PublicResources.EnumOutOfRange(alphaType));
            if (!targetColorSpace.IsDefined())
                throw new ArgumentOutOfRangeException(nameof(targetColorSpace), PublicResources.EnumOutOfRange(targetColorSpace));
        }

        [SuppressMessage("ReSharper", "AssignmentInConditionalExpression", Justification = "Intended")]
        private static SKBitmap? DoConvertToSKBitmapDirect(IAsyncContext context, IReadableBitmapData source, SKImageInfo imageInfo, Color32 backColor, byte alphaThreshold)
        {
            if (context.IsCancellationRequested)
                return null;
            bool canceled = false;
            SKBitmap? result = null;

            try
            {
                result = new SKBitmap(imageInfo);
                using (IWritableBitmapData target = result.GetBitmapDataInternal(false, source.WorkingColorSpace, backColor, alphaThreshold))
                    source.CopyTo(target, context, new Rectangle(Point.Empty, source.Size), Point.Empty);
                return (canceled = context.IsCancellationRequested) ? null : result;
            }
            catch (Exception)
            {
                result?.Dispose();
                result = null;
                throw;
            }
            finally
            {
                if (canceled)
                    result?.Dispose();
            }
        }

        [SuppressMessage("ReSharper", "AssignmentInConditionalExpression", Justification = "Intended")]
        private static SKBitmap? DoConvertToSKBitmapByQuantizer(IAsyncContext context, IReadableBitmapData source, SKImageInfo imageInfo, IQuantizer? quantizer, IDitherer? ditherer)
        {
            if (context.IsCancellationRequested)
                return null;

            bool canceled = false;
            SKBitmap? result = null;
            try
            {
                if (quantizer == null)
                {
                    // converting without using a quantizer (even if only a ditherer is specified for a high-bpp or not directly supported pixel format)
                    if (ditherer == null || !imageInfo.CanBeDithered())
                        return DoConvertToSKBitmapDirect(context, source, imageInfo, source.BackColor, source.AlphaThreshold);

                    // here we need to pick a quantizer for the dithering
                    KnownPixelFormat asKnown = imageInfo.AsKnownPixelFormat();
                    if (asKnown != KnownPixelFormat.Undefined)
                        quantizer = PredefinedColorsQuantizer.FromPixelFormat(asKnown, source.BackColor, source.AlphaThreshold);
                    else
                    {
                        using var tempBitmap = new SKBitmap(imageInfo.WithSize(1, 1));
                        using var tempBitmapData = tempBitmap.GetBitmapDataInternal(true, imageInfo.GetWorkingColorSpace(), source.BackColor, source.AlphaThreshold);
                        quantizer = PredefinedColorsQuantizer.FromBitmapData(tempBitmapData);
                    }
                }

                if (canceled = context.IsCancellationRequested)
                    return null;

                result = new SKBitmap(imageInfo);

                // Extracting back color and alpha threshold from quantizer.
                // Palette is not needed because Skia does not support indexed formats anymore.
                Color32 backColor;
                byte alphaThreshold;
                switch (quantizer)
                {
                    // shortcut for predefined quantizers: we can extract everything
                    case PredefinedColorsQuantizer predefinedColorsQuantizer:
                        backColor = predefinedColorsQuantizer.BackColor;
                        alphaThreshold = predefinedColorsQuantizer.AlphaThreshold;
                        break;

                    // shortcut for optimized quantizer
                    case OptimizedPaletteQuantizer optimizedPaletteQuantizer:
                        backColor = optimizedPaletteQuantizer.BackColor;
                        alphaThreshold = optimizedPaletteQuantizer.AlphaThreshold;
                        break;

                    // we explicitly initialize the quantizer just to determine the back color and alpha threshold
                    default:
                        context.Progress?.New(DrawingOperation.InitializingQuantizer);
                        using (IQuantizingSession quantizingSession = quantizer.Initialize(source, context))
                        {
                            if (canceled = context.IsCancellationRequested)
                                return null;
                            if (quantizingSession == null)
                                throw new InvalidOperationException(Res.ImageExtensionsQuantizerInitializeNull);

                            Palette? paletteByQuantizer = quantizingSession.Palette;
                            backColor = quantizingSession.BackColor;
                            alphaThreshold = quantizingSession.AlphaThreshold;

                            // We have a palette from a potentially expensive quantizer: creating a predefined quantizer from the already generated palette to avoid generating it again.
                            if (paletteByQuantizer != null)
                                quantizer = PredefinedColorsQuantizer.FromCustomPalette(paletteByQuantizer);
                        }

                        break;
                }

                if (canceled = context.IsCancellationRequested)
                    return null;

                using IWritableBitmapData target = result.GetWritableBitmapData(source.WorkingColorSpace, backColor.ToSKColor(), alphaThreshold);
                return (canceled = !source.CopyTo(target, context, new Rectangle(Point.Empty, source.Size), Point.Empty, quantizer, ditherer)) ? null : result;

            }
            catch (Exception)
            {
                result?.Dispose();
                result = null;
                throw;
            }
            finally
            {
                if (canceled)
                    result?.Dispose();
            }
        }

        private static SKImageInfo GetCompatibleImageInfo(IReadableBitmapData source)
        {
            PixelFormatInfo pixelFormat = source.PixelFormat;
            var result = new SKImageInfo(source.Width, source.Height);

            // Indexed formats: Skia no longer supports them so the choice is likely an overkill
            if (pixelFormat.Indexed && source.Palette is Palette palette)
            {
                result.AlphaType = palette.HasAlpha ? SKAlphaType.Unpremul : SKAlphaType.Opaque;
                result.ColorType = palette.IsGrayscale && !palette.HasAlpha
                    ? SKColorType.Gray8 // no need to check BPP because the palette cannot have more than 256 different grayscale entries
                    : SKImageInfo.PlatformColorType; // palette entries are 32 bpp colors so this will do it
            }
            else
            {
                // Non-indexed formats
                result.AlphaType = pixelFormat.HasPremultipliedAlpha ? SKAlphaType.Premul
                    : pixelFormat.HasAlpha ? SKAlphaType.Unpremul
                    : SKAlphaType.Opaque;

                result.ColorType = pixelFormat.Grayscale ? pixelFormat switch
                    {
                        // Grayscale formats
                        { BitsPerPixel: > 32 } or { BitsPerPixel: 32, HasAlpha: false } => SKColorType.RgbaF32,
                        { BitsPerPixel: > 8, HasAlpha: true } or { BitsPerPixel: > 10, HasAlpha: false } => SKColorType.Rgba16161616,
                        { BitsPerPixel: > 8, HasAlpha: false } => SKColorType.Bgr101010x, // a 9/10 bpp format is not too likely though
                        { HasAlpha: true } => SKImageInfo.PlatformColorType,
                        _ => SKColorType.Gray8
                    }
                    : pixelFormat.HasAlpha ? pixelFormat.BitsPerPixel switch
                    {
                        // Formats with alpha
                        > 64 => SKColorType.RgbaF32,
                        > 32 => SKColorType.Rgba16161616,
                        _ => SKImageInfo.PlatformColorType
                    }
                    : pixelFormat.BitsPerPixel switch
                    {
                        // Opaque formats
                        > 48 => SKColorType.RgbaF32,
                        > 32 => SKColorType.Rgba16161616,
                        > 24 => SKColorType.Bgr101010x, // this actually turns 888x formats to 101010x but it's better to assume the better quality
                        _ => SKImageInfo.PlatformColorType
                    };
            }

            result.ColorSpace = source.GetPreferredColorSpace() is WorkingColorSpace.Linear
                ? SKColorSpace.CreateSrgbLinear()
                : SKColorSpace.CreateSrgb();

            return result;
        }

        private static SKImageInfo GetImageInfo(IReadableBitmapData source, SKColorType colorType, SKAlphaType alphaType, WorkingColorSpace targetColorSpace)
        {
            var result = new SKImageInfo(source.Width, source.Height, colorType, alphaType, targetColorSpace == WorkingColorSpace.Linear ? SKColorSpace.CreateSrgbLinear() : SKColorSpace.CreateSrgb());
            if (colorType == SKColorType.Unknown || alphaType == SKAlphaType.Unknown)
            {
                var compatibleInfo = GetCompatibleImageInfo(source);
                if (colorType == SKColorType.Unknown)
                    result.ColorType = compatibleInfo.ColorType;
                if (alphaType == SKAlphaType.Unknown)
                    result.AlphaType = compatibleInfo.AlphaType;
            }

            return result;
        }

        #endregion

        #endregion
    }
}
