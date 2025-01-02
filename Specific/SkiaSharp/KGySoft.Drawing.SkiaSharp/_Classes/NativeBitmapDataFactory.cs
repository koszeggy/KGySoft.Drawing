#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: NativeBitmapDataFactory.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2024 - All Rights Reserved
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

using KGySoft.Drawing.Imaging;

using SkiaSharp;

#endregion

namespace KGySoft.Drawing.SkiaSharp
{
    internal static class NativeBitmapDataFactory
    {
        #region Methods

        #region Internal Methods
        
        internal static bool TryCreateBitmapData(IntPtr buffer, SKImageInfo info, int stride, Color32 backColor, byte alphaThreshold,
            WorkingColorSpace workingColorSpace, Action? disposeCallback, [MaybeNullWhen(false)]out IReadWriteBitmapData bitmapData)
        {
            info.GetDirectlySupportedColorSpace(out bool srgb, out bool linear);
            bitmapData = srgb ? CreateBitmapDataSrgb(buffer, info, stride, workingColorSpace, backColor, alphaThreshold, disposeCallback)
                : linear ? CreateBitmapDataLinear(buffer, info, stride, workingColorSpace, backColor, alphaThreshold, disposeCallback)
                : null;
            return bitmapData != null;
        }

        #endregion

        #region Private Methods
        
        [SuppressMessage("Microsoft.Maintainability", "CA1502: Avoid excessive complexity", Justification = "Long but straightforward cases for the possible pixel formats.")]
        private static IReadWriteBitmapData CreateBitmapDataSrgb(IntPtr buffer, SKImageInfo info, int stride,
            WorkingColorSpace workingColorSpace, Color32 backColor, byte alphaThreshold, Action? disposeCallback = null)
        {
            Debug.Assert(info.IsDirectlySupported());

            var size = new Size(info.Width, info.Height);
            KnownPixelFormat knownPixelFormat = info.AsKnownPixelFormat();

            // Natively supported formats
            if (knownPixelFormat != KnownPixelFormat.Undefined)
                return BitmapDataFactory.CreateBitmapData(buffer, size, stride, knownPixelFormat, workingColorSpace, backColor, alphaThreshold, disposeCallback);

            // Custom formats
            PixelFormatInfo pixelFormatInfo = info.GetInfo();
            var config = new CustomBitmapDataConfig
            {
                PixelFormat = pixelFormatInfo,
                BackColor = backColor,
                AlphaThreshold = alphaThreshold,
                WorkingColorSpace = workingColorSpace,
                DisposeCallback = disposeCallback,
                BackBufferIndependentPixelAccess = true,
            };

            switch ((info.ColorType, info.AlphaType))
            {
                case (SKColorType.Rgba8888, SKAlphaType.Unpremul):
                    config.RowGetColor32 = (row, x) => row.UnsafeGetRefAs<ColorRgba8888Srgb>(x).ToColor32();
                    config.RowSetColor32 = (row, x, c) => row.UnsafeGetRefAs<ColorRgba8888Srgb>(x) = new ColorRgba8888Srgb(c);
                    break;

                case (SKColorType.Rgba8888, SKAlphaType.Premul):
                    config.RowGetPColor32 = (row, x) => row.UnsafeGetRefAs<ColorPrgba8888Srgb>(x).ToPColor32();
                    config.RowSetPColor32 = (row, x, c) => row.UnsafeGetRefAs<ColorPrgba8888Srgb>(x) = new ColorPrgba8888Srgb(c);
                    break;

                case (SKColorType.Rgba8888, SKAlphaType.Opaque):
                case (SKColorType.Rgb888x, _):
                    config.RowGetColor32 = (row, x) => row.UnsafeGetRefAs<ColorRgba8888Srgb>(x).ToColor32().ToOpaque();
                    config.RowSetColor32 = (row, x, c) => row.UnsafeGetRefAs<ColorRgba8888Srgb>(x) =
                        new ColorRgba8888Srgb(c.A == Byte.MaxValue ? c : c.Blend(row.BitmapData.BackColor, row.BitmapData.WorkingColorSpace));
                    config.RowSetColorF = (row, x, c) => row.UnsafeGetRefAs<ColorRgba8888Srgb>(x) = new ColorRgba8888Srgb(c.A >= 1f ? c.ToColor32()
                        : row.BitmapData.WorkingColorSpace == WorkingColorSpace.Linear ? c.Blend(row.BitmapData.BackColor.ToColorF()).ToColor32()
                        : c.ToColor32().Blend(row.BitmapData.BackColor));
                    break;

                case (SKColorType.Rgba16161616, SKAlphaType.Unpremul):
                    config.RowGetColor64 = (row, x) => row.UnsafeGetRefAs<ColorRgba16161616Srgb>(x).ToColor64();
                    config.RowSetColor64 = (row, x, c) => row.UnsafeGetRefAs<ColorRgba16161616Srgb>(x) = new ColorRgba16161616Srgb(c);
                    break;

                case (SKColorType.Rgba16161616, SKAlphaType.Premul):
                    config.RowGetPColor64 = (row, x) => row.UnsafeGetRefAs<ColorPrgba16161616Srgb>(x).ToPColor64();
                    config.RowSetPColor64 = (row, x, c) => row.UnsafeGetRefAs<ColorPrgba16161616Srgb>(x) = new ColorPrgba16161616Srgb(c);
                    break;

                case (SKColorType.Rgba16161616, SKAlphaType.Opaque):
                    config.RowGetColor64 = (row, x) => row.UnsafeGetRefAs<ColorRgba16161616Srgb>(x).ToColor64().ToOpaque();
                    config.RowSetColor64 = (row, x, c) => row.UnsafeGetRefAs<ColorRgba16161616Srgb>(x) =
                        new ColorRgba16161616Srgb(c.A == UInt16.MaxValue ? c : c.Blend(row.BitmapData.BackColor.ToColor64(), row.BitmapData.WorkingColorSpace));
                    config.RowSetColorF = (row, x, c) => row.UnsafeGetRefAs<ColorRgba16161616Srgb>(x) = new ColorRgba16161616Srgb(c.A >= 1f ? c.ToColor64()
                        : row.BitmapData.WorkingColorSpace == WorkingColorSpace.Linear ? c.Blend(row.BitmapData.BackColor.ToColorF()).ToColor64()
                        : c.ToColor64().Blend(row.BitmapData.BackColor.ToColor64()));
                    break;

                case (SKColorType.Bgra1010102, SKAlphaType.Unpremul):
                    config.RowGetColor32 = (row, x) => row.UnsafeGetRefAs<ColorBgra1010102Srgb>(x).ToColor32();
                    config.RowSetColor32 = (row, x, c) => row.UnsafeGetRefAs<ColorBgra1010102Srgb>(x) = new ColorBgra1010102Srgb(c);
                    config.RowGetColor64 = (row, x) => row.UnsafeGetRefAs<ColorBgra1010102Srgb>(x).ToColor64();
                    config.RowSetColor64 = (row, x, c) => row.UnsafeGetRefAs<ColorBgra1010102Srgb>(x) = new ColorBgra1010102Srgb(c);
                    break;

                case (SKColorType.Bgra1010102, SKAlphaType.Premul):
                    // Note that we set from Color64 instead of PColor64 because special premultiplication is needed due to the different depth of A and RGB
                    config.RowGetPColor64 = (row, x) => row.UnsafeGetRefAs<ColorPbgra1010102Srgb>(x).ToPColor64();
                    config.RowSetColor64 = (row, x, c) => row.UnsafeGetRefAs<ColorPbgra1010102Srgb>(x) = new ColorPbgra1010102Srgb(c);
                    break;

                case (SKColorType.Bgra1010102, SKAlphaType.Opaque):
                case (SKColorType.Bgr101010x, _):
                    config.RowGetColor32 = (row, x) => row.UnsafeGetRefAs<ColorBgra1010102Srgb>(x).ToColor32().ToOpaque();
                    config.RowGetColor64 = (row, x) => row.UnsafeGetRefAs<ColorBgra1010102Srgb>(x).ToColor64().ToOpaque();
                    config.RowSetColor64 = (row, x, c) => row.UnsafeGetRefAs<ColorBgra1010102Srgb>(x) =
                        new ColorBgra1010102Srgb(c.A == UInt16.MaxValue ? c : c.Blend(row.BitmapData.BackColor.ToColor64(), row.BitmapData.WorkingColorSpace));
                    config.RowSetColorF = (row, x, c) => row.UnsafeGetRefAs<ColorBgra1010102Srgb>(x) = new ColorBgra1010102Srgb(c.A >= 1f ? c.ToColor64()
                        : row.BitmapData.WorkingColorSpace == WorkingColorSpace.Linear ? c.Blend(row.BitmapData.BackColor.ToColorF()).ToColor64()
                        : c.ToColor64().Blend(row.BitmapData.BackColor.ToColor64()));
                    break;

                case (SKColorType.Rgba1010102, SKAlphaType.Unpremul):
                    config.RowGetColor32 = (row, x) => row.UnsafeGetRefAs<ColorRgba1010102Srgb>(x).ToColor32();
                    config.RowSetColor32 = (row, x, c) => row.UnsafeGetRefAs<ColorRgba1010102Srgb>(x) = new ColorRgba1010102Srgb(c);
                    config.RowGetColor64 = (row, x) => row.UnsafeGetRefAs<ColorRgba1010102Srgb>(x).ToColor64();
                    config.RowSetColor64 = (row, x, c) => row.UnsafeGetRefAs<ColorRgba1010102Srgb>(x) = new ColorRgba1010102Srgb(c);
                    break;

                case (SKColorType.Rgba1010102, SKAlphaType.Premul):
                    // Note that we set from Color64 instead of PColor64 because special premultiplication is needed due to the different depth of A and RGB
                    config.RowGetPColor64 = (row, x) => row.UnsafeGetRefAs<ColorPrgba1010102Srgb>(x).ToPColor64();
                    config.RowSetColor64 = (row, x, c) => row.UnsafeGetRefAs<ColorPrgba1010102Srgb>(x) = new ColorPrgba1010102Srgb(c);
                    break;

                case (SKColorType.Rgba1010102, SKAlphaType.Opaque):
                case (SKColorType.Rgb101010x, _):
                    config.RowGetColor32 = (row, x) => row.UnsafeGetRefAs<ColorRgba1010102Srgb>(x).ToColor32().ToOpaque();
                    config.RowGetColor64 = (row, x) => row.UnsafeGetRefAs<ColorRgba1010102Srgb>(x).ToColor64().ToOpaque();
                    config.RowSetColor64 = (row, x, c) => row.UnsafeGetRefAs<ColorRgba1010102Srgb>(x) =
                        new ColorRgba1010102Srgb(c.A == UInt16.MaxValue ? c : c.Blend(row.BitmapData.BackColor.ToColor64(), row.BitmapData.WorkingColorSpace));
                    config.RowSetColorF = (row, x, c) => row.UnsafeGetRefAs<ColorRgba1010102Srgb>(x) = new ColorRgba1010102Srgb(c.A >= 1f ? c.ToColor64()
                        : row.BitmapData.WorkingColorSpace == WorkingColorSpace.Linear ? c.Blend(row.BitmapData.BackColor.ToColorF()).ToColor64()
                        : c.ToColor64().Blend(row.BitmapData.BackColor.ToColor64()));
                    break;

                case (SKColorType.Argb4444, SKAlphaType.Unpremul):
                    config.RowGetColor32 = (row, x) => row.UnsafeGetRefAs<ColorArgb4444Srgb>(x).ToColor32();
                    config.RowSetColor32 = (row, x, c) => row.UnsafeGetRefAs<ColorArgb4444Srgb>(x) = new ColorArgb4444Srgb(c);
                    break;

                case (SKColorType.Argb4444, SKAlphaType.Premul):
                    config.RowGetPColor32 = (row, x) => row.UnsafeGetRefAs<ColorPargb4444Srgb>(x).ToPColor32();
                    config.RowSetPColor32 = (row, x, c) => row.UnsafeGetRefAs<ColorPargb4444Srgb>(x) = new ColorPargb4444Srgb(c);
                    break;

                case (SKColorType.Argb4444, SKAlphaType.Opaque):
                    config.RowGetColor32 = (row, x) => row.UnsafeGetRefAs<ColorArgb4444Srgb>(x).ToColor32().ToOpaque();
                    config.RowSetColor32 = (row, x, c) => row.UnsafeGetRefAs<ColorArgb4444Srgb>(x) =
                        new ColorArgb4444Srgb(c.A == Byte.MaxValue ? c : c.Blend(row.BitmapData.BackColor, row.BitmapData.WorkingColorSpace));
                    config.RowSetColorF = (row, x, c) => row.UnsafeGetRefAs<ColorArgb4444Srgb>(x) = new ColorArgb4444Srgb(c.A >= 1f ? c.ToColor32()
                        : row.BitmapData.WorkingColorSpace == WorkingColorSpace.Linear ? c.Blend(row.BitmapData.BackColor.ToColorF()).ToColor32()
                        : c.ToColor32().Blend(row.BitmapData.BackColor));
                    break;

                case (SKColorType.RgbaF32, SKAlphaType.Unpremul):
                    config.RowGetColor32 = (row, x) => row.UnsafeGetRefAs<ColorRgbaF32Srgb>(x).ToColor32();
                    config.RowSetColor32 = (row, x, c) => row.UnsafeGetRefAs<ColorRgbaF32Srgb>(x) = new ColorRgbaF32Srgb(c);
                    config.RowGetColor64 = (row, x) => row.UnsafeGetRefAs<ColorRgbaF32Srgb>(x).ToColor64();
                    config.RowSetColor64 = (row, x, c) => row.UnsafeGetRefAs<ColorRgbaF32Srgb>(x) = new ColorRgbaF32Srgb(c);
                    config.RowGetColorF = (row, x) => row.UnsafeGetRefAs<ColorRgbaF32Srgb>(x).ToColorF();
                    config.RowSetColorF = (row, x, c) => row.UnsafeGetRefAs<ColorRgbaF32Srgb>(x) = new ColorRgbaF32Srgb(c);
                    break;

                case (SKColorType.RgbaF32, SKAlphaType.Premul):
                    // Because of the different color spaces we assign the ColorF methods instead of PColorF for performance reasons.
                    // It's still slower than the other colors but it's the preferred format to preserve as much information as possible.
                    config.RowGetPColor32 = (row, x) => row.UnsafeGetRefAs<ColorPrgbaF32Srgb>(x).ToPColor32();
                    config.RowSetPColor32 = (row, x, c) => row.UnsafeGetRefAs<ColorPrgbaF32Srgb>(x) = new ColorPrgbaF32Srgb(c);
                    config.RowGetPColor64 = (row, x) => row.UnsafeGetRefAs<ColorPrgbaF32Srgb>(x).ToPColor64();
                    config.RowSetPColor64 = (row, x, c) => row.UnsafeGetRefAs<ColorPrgbaF32Srgb>(x) = new ColorPrgbaF32Srgb(c);
                    config.RowGetColorF = (row, x) => row.UnsafeGetRefAs<ColorPrgbaF32Srgb>(x).ToColorF();
                    config.RowSetColorF = (row, x, c) => row.UnsafeGetRefAs<ColorPrgbaF32Srgb>(x) = new ColorPrgbaF32Srgb(c);
                    break;

                case (SKColorType.RgbaF32, SKAlphaType.Opaque):
                    config.RowGetColor32 = (row, x) => row.UnsafeGetRefAs<ColorRgbaF32Srgb>(x).ToColor32().ToOpaque();
                    config.RowSetColor32 = (row, x, c) => row.UnsafeGetRefAs<ColorRgbaF32Srgb>(x) = c.A == Byte.MaxValue ? new ColorRgbaF32Srgb(c)
                        : row.BitmapData.WorkingColorSpace == WorkingColorSpace.Linear ? new ColorRgbaF32Srgb(c.ToColorF().Blend(row.BitmapData.BackColor.ToColorF()))
                        : new ColorRgbaF32Srgb(c.ToColor64().Blend(row.BitmapData.BackColor.ToColor64()));
                    config.RowGetColor64 = (row, x) => row.UnsafeGetRefAs<ColorRgbaF32Srgb>(x).ToColor64().ToOpaque();
                    config.RowSetColor64 = (row, x, c) => row.UnsafeGetRefAs<ColorRgbaF32Srgb>(x) = c.A == UInt16.MaxValue ? new ColorRgbaF32Srgb(c)
                        : row.BitmapData.WorkingColorSpace == WorkingColorSpace.Linear ? new ColorRgbaF32Srgb(c.ToColorF().Blend(row.BitmapData.BackColor.ToColorF()))
                        : new ColorRgbaF32Srgb(c.Blend(row.BitmapData.BackColor.ToColor64()));
                    config.RowGetColorF = (row, x) => row.UnsafeGetRefAs<ColorRgbaF32Srgb>(x).ToColorF().ToOpaque();
                    config.RowSetColorF = (row, x, c) => row.UnsafeGetRefAs<ColorRgbaF32Srgb>(x) = c.A >= 1f ? new ColorRgbaF32Srgb(c)
                        : row.BitmapData.WorkingColorSpace == WorkingColorSpace.Linear ? new ColorRgbaF32Srgb(c.Blend(row.BitmapData.BackColor.ToColorF()))
                        : ColorRgbaF32Srgb.FromSrgb(c.ToSrgb().Blend(row.BitmapData.BackColor.ToColorF(false)));
                    break;

                case (SKColorType.RgbaF16, SKAlphaType.Unpremul):
                case (SKColorType.RgbaF16Clamped, SKAlphaType.Unpremul):
                    // Though 64-bit colors are preferred for this format we set also ColorF methods to save a few operations when using with ColorF
                    // Setting from ColorF does not need clipping in case of RgbaF16Clamped because the color space conversion performs it anyway.
                    config.RowGetColor32 = (row, x) => row.UnsafeGetRefAs<ColorRgbaF16Srgb>(x).ToColor32();
                    config.RowSetColor32 = (row, x, c) => row.UnsafeGetRefAs<ColorRgbaF16Srgb>(x) = new ColorRgbaF16Srgb(c);
                    config.RowGetColor64 = (row, x) => row.UnsafeGetRefAs<ColorRgbaF16Srgb>(x).ToColor64();
                    config.RowSetColor64 = (row, x, c) => row.UnsafeGetRefAs<ColorRgbaF16Srgb>(x) = new ColorRgbaF16Srgb(c);
                    config.RowGetColorF = info.ColorType == SKColorType.RgbaF16
                        ? (row, x) => row.UnsafeGetRefAs<ColorRgbaF16Srgb>(x).ToColorF()
                        : (row, x) => row.UnsafeGetRefAs<ColorRgbaF16Srgb>(x).ToColorF().Clip();
                    config.RowSetColorF = (row, x, c) => row.UnsafeGetRefAs<ColorRgbaF16Srgb>(x) = new ColorRgbaF16Srgb(c);
                    break;

                case (SKColorType.RgbaF16, SKAlphaType.Premul):
                case (SKColorType.RgbaF16Clamped, SKAlphaType.Premul):
                    // Though 64-bit colors are preferred for this format we set also ColorF methods to save a few operations when using with [P]ColorF
                    // ColorF operations do not need clipping in case of RgbaF16Clamped because the color space conversion performs it anyway.
                    config.RowGetPColor32 = (row, x) => row.UnsafeGetRefAs<ColorPrgbaF16Srgb>(x).ToPColor32();
                    config.RowSetPColor32 = (row, x, c) => row.UnsafeGetRefAs<ColorPrgbaF16Srgb>(x) = new ColorPrgbaF16Srgb(c);
                    config.RowGetPColor64 = (row, x) => row.UnsafeGetRefAs<ColorPrgbaF16Srgb>(x).ToPColor64();
                    config.RowSetPColor64 = (row, x, c) => row.UnsafeGetRefAs<ColorPrgbaF16Srgb>(x) = new ColorPrgbaF16Srgb(c);
                    config.RowGetColorF = (row, x) => row.UnsafeGetRefAs<ColorPrgbaF16Srgb>(x).ToColorF();
                    config.RowSetColorF = (row, x, c) => row.UnsafeGetRefAs<ColorPrgbaF16Srgb>(x) = new ColorPrgbaF16Srgb(c);
                    break;

                case (SKColorType.RgbaF16, SKAlphaType.Opaque):
                case (SKColorType.RgbaF16Clamped, SKAlphaType.Opaque):
                    // Setting from ColorF does not need clipping in case of RgbaF16Clamped because the color space conversion performs it anyway.
                    config.RowGetColor32 = (row, x) => row.UnsafeGetRefAs<ColorRgbaF16Srgb>(x).ToColor32().ToOpaque();
                    config.RowSetColor32 = (row, x, c) => row.UnsafeGetRefAs<ColorRgbaF16Srgb>(x) = c.A == Byte.MaxValue ? new ColorRgbaF16Srgb(c)
                        : row.BitmapData.WorkingColorSpace == WorkingColorSpace.Linear ? new ColorRgbaF16Srgb(c.ToColorF().Blend(row.BitmapData.BackColor.ToColorF()))
                        : new ColorRgbaF16Srgb(c.ToColor64().Blend(row.BitmapData.BackColor.ToColor64()));
                    config.RowGetColor64 = (row, x) => row.UnsafeGetRefAs<ColorRgbaF16Srgb>(x).ToColor64().ToOpaque();
                    config.RowSetColor64 = (row, x, c) => row.UnsafeGetRefAs<ColorRgbaF16Srgb>(x) = c.A == UInt16.MaxValue ? new ColorRgbaF16Srgb(c)
                        : row.BitmapData.WorkingColorSpace == WorkingColorSpace.Linear ? new ColorRgbaF16Srgb(c.ToColorF().Blend(row.BitmapData.BackColor.ToColorF()))
                        : new ColorRgbaF16Srgb(c.Blend(row.BitmapData.BackColor.ToColor64()));
                    config.RowGetColorF = info.ColorType == SKColorType.RgbaF16
                        ? (row, x) => row.UnsafeGetRefAs<ColorRgbaF16Srgb>(x).ToColorF().ToOpaque()
                        : (row, x) => row.UnsafeGetRefAs<ColorRgbaF16Srgb>(x).ToColorF().ToOpaque().Clip();
                    config.RowSetColorF = (row, x, c) => row.UnsafeGetRefAs<ColorRgbaF16Srgb>(x) = c.A >= 1f ? new ColorRgbaF16Srgb(c)
                        : row.BitmapData.WorkingColorSpace == WorkingColorSpace.Linear ? new ColorRgbaF16Srgb(c.Blend(row.BitmapData.BackColor.ToColorF()))
                        : ColorRgbaF16Srgb.FromSrgb(c.ToSrgb().Blend(row.BitmapData.BackColor.ToColorF(false)));
                    break;

                case (SKColorType.Alpha8, _):
                    // Defining everything to spare a lot of unnecessary conversions (though the preferred color type is Color32)
                    config.RowGetColor32 = (row, x) => row.UnsafeGetRefAs<ColorAlpha8>(x).ToColor32();
                    config.RowSetColor32 = (row, x, c) => row.UnsafeGetRefAs<ColorAlpha8>(x) = new ColorAlpha8(c);
                    config.RowGetPColor32 = (row, x) => row.UnsafeGetRefAs<ColorAlpha8>(x).ToPColor32();
                    config.RowSetPColor32 = (row, x, c) => row.UnsafeGetRefAs<ColorAlpha8>(x) = new ColorAlpha8(c);
                    config.RowGetColor64 = (row, x) => row.UnsafeGetRefAs<ColorAlpha8>(x).ToColor64();
                    config.RowSetColor64 = (row, x, c) => row.UnsafeGetRefAs<ColorAlpha8>(x) = new ColorAlpha8(c);
                    config.RowGetPColor64 = (row, x) => row.UnsafeGetRefAs<ColorAlpha8>(x).ToPColor64();
                    config.RowSetPColor64 = (row, x, c) => row.UnsafeGetRefAs<ColorAlpha8>(x) = new ColorAlpha8(c);
                    config.RowGetColorF = (row, x) => row.UnsafeGetRefAs<ColorAlpha8>(x).ToColorF();
                    config.RowSetColorF = (row, x, c) => row.UnsafeGetRefAs<ColorAlpha8>(x) = new ColorAlpha8(c);
                    config.RowGetPColorF = (row, x) => row.UnsafeGetRefAs<ColorAlpha8>(x).ToPColorF();
                    config.RowSetPColorF = (row, x, c) => row.UnsafeGetRefAs<ColorAlpha8>(x) = new ColorAlpha8(c);
                    break;

                case (SKColorType.Alpha16, _):
                    // Defining everything to spare a lot of unnecessary conversions (though the preferred color type is Color64)
                    config.RowGetColor32 = (row, x) => row.UnsafeGetRefAs<ColorAlpha16>(x).ToColor32();
                    config.RowSetColor32 = (row, x, c) => row.UnsafeGetRefAs<ColorAlpha16>(x) = new ColorAlpha16(c);
                    config.RowGetPColor32 = (row, x) => row.UnsafeGetRefAs<ColorAlpha16>(x).ToPColor32();
                    config.RowSetPColor32 = (row, x, c) => row.UnsafeGetRefAs<ColorAlpha16>(x) = new ColorAlpha16(c);
                    config.RowGetColor64 = (row, x) => row.UnsafeGetRefAs<ColorAlpha16>(x).ToColor64();
                    config.RowSetColor64 = (row, x, c) => row.UnsafeGetRefAs<ColorAlpha16>(x) = new ColorAlpha16(c);
                    config.RowGetPColor64 = (row, x) => row.UnsafeGetRefAs<ColorAlpha16>(x).ToPColor64();
                    config.RowSetPColor64 = (row, x, c) => row.UnsafeGetRefAs<ColorAlpha16>(x) = new ColorAlpha16(c);
                    config.RowGetColorF = (row, x) => row.UnsafeGetRefAs<ColorAlpha16>(x).ToColorF();
                    config.RowSetColorF = (row, x, c) => row.UnsafeGetRefAs<ColorAlpha16>(x) = new ColorAlpha16(c);
                    config.RowGetPColorF = (row, x) => row.UnsafeGetRefAs<ColorAlpha16>(x).ToPColorF();
                    config.RowSetPColorF = (row, x, c) => row.UnsafeGetRefAs<ColorAlpha16>(x) = new ColorAlpha16(c);
                    break;

                case (SKColorType.AlphaF16, _):
                    // Defining everything to spare a lot of unnecessary conversions (though the preferred color type is ColorF)
                    config.RowGetColor32 = (row, x) => row.UnsafeGetRefAs<ColorAlphaF16>(x).ToColor32();
                    config.RowSetColor32 = (row, x, c) => row.UnsafeGetRefAs<ColorAlphaF16>(x) = new ColorAlphaF16(c);
                    config.RowGetPColor32 = (row, x) => row.UnsafeGetRefAs<ColorAlphaF16>(x).ToPColor32();
                    config.RowSetPColor32 = (row, x, c) => row.UnsafeGetRefAs<ColorAlphaF16>(x) = new ColorAlphaF16(c);
                    config.RowGetColor64 = (row, x) => row.UnsafeGetRefAs<ColorAlphaF16>(x).ToColor64();
                    config.RowSetColor64 = (row, x, c) => row.UnsafeGetRefAs<ColorAlphaF16>(x) = new ColorAlphaF16(c);
                    config.RowGetPColor64 = (row, x) => row.UnsafeGetRefAs<ColorAlphaF16>(x).ToPColor64();
                    config.RowSetPColor64 = (row, x, c) => row.UnsafeGetRefAs<ColorAlphaF16>(x) = new ColorAlphaF16(c);
                    config.RowGetColorF = (row, x) => row.UnsafeGetRefAs<ColorAlphaF16>(x).ToColorF();
                    config.RowSetColorF = (row, x, c) => row.UnsafeGetRefAs<ColorAlphaF16>(x) = new ColorAlphaF16(c);
                    config.RowGetPColorF = (row, x) => row.UnsafeGetRefAs<ColorAlphaF16>(x).ToPColorF();
                    config.RowSetPColorF = (row, x, c) => row.UnsafeGetRefAs<ColorAlphaF16>(x) = new ColorAlphaF16(c);
                    break;

                case (SKColorType.Rg88, _):
                    config.RowGetColor32 = (row, x) => row.UnsafeGetRefAs<ColorRg88Srgb>(x).ToColor32();
                    config.RowSetColor32 = (row, x, c) => row.UnsafeGetRefAs<ColorRg88Srgb>(x) =
                        new ColorRg88Srgb(c.A == Byte.MaxValue ? c : c.Blend(row.BitmapData.BackColor, row.BitmapData.WorkingColorSpace));
                    config.RowSetColorF = (row, x, c) => row.UnsafeGetRefAs<ColorRg88Srgb>(x) = new ColorRg88Srgb(c.A >= 1f ? c.ToColor32()
                        : row.BitmapData.WorkingColorSpace == WorkingColorSpace.Linear ? c.Blend(row.BitmapData.BackColor.ToColorF()).ToColor32()
                        : c.ToColor32().Blend(row.BitmapData.BackColor));
                    break;

                case (SKColorType.Rg1616, _):
                    config.RowGetColor64 = (row, x) => row.UnsafeGetRefAs<ColorRg1616Srgb>(x).ToColor64();
                    config.RowSetColor64 = (row, x, c) => row.UnsafeGetRefAs<ColorRg1616Srgb>(x) =
                        new ColorRg1616Srgb(c.A == UInt16.MaxValue ? c : c.Blend(row.BitmapData.BackColor.ToColor64(), row.BitmapData.WorkingColorSpace));
                    config.RowSetColorF = (row, x, c) => row.UnsafeGetRefAs<ColorRg1616Srgb>(x) = new ColorRg1616Srgb(c.A >= 1f ? c.ToColor64()
                        : row.BitmapData.WorkingColorSpace == WorkingColorSpace.Linear ? c.Blend(row.BitmapData.BackColor.ToColorF()).ToColor64()
                        : c.ToColor64().Blend(row.BitmapData.BackColor.ToColor64()));
                    break;

                case (SKColorType.RgF16, _):
                    config.RowGetColor32 = (row, x) => row.UnsafeGetRefAs<ColorRgF16Srgb>(x).ToColor32();
                    config.RowSetColor32 = (row, x, c) => row.UnsafeGetRefAs<ColorRgF16Srgb>(x) = c.A == Byte.MaxValue ? new ColorRgF16Srgb(c)
                        : row.BitmapData.WorkingColorSpace == WorkingColorSpace.Linear ? new ColorRgF16Srgb(c.ToColorF().Blend(row.BitmapData.BackColor.ToColorF()))
                        : new ColorRgF16Srgb(c.ToColor64().Blend(row.BitmapData.BackColor.ToColor64()));
                    config.RowGetColor64 = (row, x) => row.UnsafeGetRefAs<ColorRgF16Srgb>(x).ToColor64();
                    config.RowSetColor64 = (row, x, c) => row.UnsafeGetRefAs<ColorRgF16Srgb>(x) = c.A == UInt16.MaxValue ? new ColorRgF16Srgb(c)
                        : row.BitmapData.WorkingColorSpace == WorkingColorSpace.Linear ? new ColorRgF16Srgb(c.ToColorF().Blend(row.BitmapData.BackColor.ToColorF()))
                        : new ColorRgF16Srgb(c.Blend(row.BitmapData.BackColor.ToColor64()));
                    config.RowGetColorF = (row, x) => row.UnsafeGetRefAs<ColorRgF16Srgb>(x).ToColorF();
                    config.RowSetColorF = (row, x, c) => row.UnsafeGetRefAs<ColorRgF16Srgb>(x) = c.A >= 1f ? new ColorRgF16Srgb(c)
                        : row.BitmapData.WorkingColorSpace == WorkingColorSpace.Linear ? new ColorRgF16Srgb(c.Blend(row.BitmapData.BackColor.ToColorF()))
                        : ColorRgF16Srgb.FromSrgb(c.ToSrgb().Blend(row.BitmapData.BackColor.ToColorF(false)));
                    break;

                case (SKColorType.Bgr101010xXR, _):
                    config.RowGetColor32 = (row, x) => row.UnsafeGetRefAs<ColorBgr101010XRSrgb>(x).ToColor32();
                    config.RowGetColor64 = (row, x) => row.UnsafeGetRefAs<ColorBgr101010XRSrgb>(x).ToColor64();
                    config.RowGetColorF = (row, x) => row.UnsafeGetRefAs<ColorBgr101010XRSrgb>(x).ToColorF();
                    config.RowSetColor64 = (row, x, c) => row.UnsafeGetRefAs<ColorBgr101010XRSrgb>(x) =
                        new ColorBgr101010XRSrgb(c.A == UInt16.MaxValue ? c : c.Blend(row.BitmapData.BackColor.ToColor64(), row.BitmapData.WorkingColorSpace));
                    config.RowSetColorF = (row, x, c) => row.UnsafeGetRefAs<ColorBgr101010XRSrgb>(x) = c.A >= 1f ? new ColorBgr101010XRSrgb(c)
                        : row.BitmapData.WorkingColorSpace == WorkingColorSpace.Linear ? new ColorBgr101010XRSrgb(c.Blend(row.BitmapData.BackColor.ToColorF()))
                        : ColorBgr101010XRSrgb.FromSrgb(c.ToSrgb().Blend(row.BitmapData.BackColor.ToColorF(false)));
                    break;

                case (SKColorType.Srgba8888, SKAlphaType.Unpremul):
                    config.RowGetColor32 = (row, x) => row.UnsafeGetRefAs<ColorSrgba8888Srgb>(x).ToColor32();
                    config.RowSetColor32 = (row, x, c) => row.UnsafeGetRefAs<ColorSrgba8888Srgb>(x) = new ColorSrgba8888Srgb(c);
                    config.RowGetColor64 = (row, x) => row.UnsafeGetRefAs<ColorSrgba8888Srgb>(x).ToColor64();
                    config.RowSetColor64 = (row, x, c) => row.UnsafeGetRefAs<ColorSrgba8888Srgb>(x) = new ColorSrgba8888Srgb(c);
                    break;

                case (SKColorType.Srgba8888, SKAlphaType.Premul):
                    config.RowGetPColor32 = (row, x) => row.UnsafeGetRefAs<ColorPsrgba8888Srgb>(x).ToPColor32();
                    config.RowSetPColor32 = (row, x, c) => row.UnsafeGetRefAs<ColorPsrgba8888Srgb>(x) = new ColorPsrgba8888Srgb(c);
                    config.RowGetPColor64 = (row, x) => row.UnsafeGetRefAs<ColorPsrgba8888Srgb>(x).ToPColor64();
                    config.RowSetPColor64 = (row, x, c) => row.UnsafeGetRefAs<ColorPsrgba8888Srgb>(x) = new ColorPsrgba8888Srgb(c);
                    break;

                case (SKColorType.Srgba8888, SKAlphaType.Opaque):
                    config.RowGetColor32 = (row, x) => row.UnsafeGetRefAs<ColorSrgba8888Srgb>(x).ToColor32().ToOpaque();
                    config.RowGetColor64 = (row, x) => row.UnsafeGetRefAs<ColorSrgba8888Srgb>(x).ToColor64().ToOpaque();
                    config.RowSetColor64 = (row, x, c) => row.UnsafeGetRefAs<ColorSrgba8888Srgb>(x) =
                        new ColorSrgba8888Srgb(c.A == UInt16.MaxValue ? c : c.Blend(row.BitmapData.BackColor.ToColor64(), row.BitmapData.WorkingColorSpace));
                    config.RowSetColorF = (row, x, c) => row.UnsafeGetRefAs<ColorSrgba8888Srgb>(x) = new ColorSrgba8888Srgb(c.A >= 1f ? c.ToColor64()
                        : row.BitmapData.WorkingColorSpace == WorkingColorSpace.Linear ? c.Blend(row.BitmapData.BackColor.ToColorF()).ToColor64()
                        : c.ToColor64().Blend(row.BitmapData.BackColor.ToColor64()));
                    break;

                case (SKColorType.R8Unorm, _):
                    config.RowGetColor32 = (row, x) => row.UnsafeGetRefAs<ColorR8UnormSrgb>(x).ToColor32();
                    config.RowSetColor32 = (row, x, c) => row.UnsafeGetRefAs<ColorR8UnormSrgb>(x) =
                        new ColorR8UnormSrgb(c.A == Byte.MaxValue ? c : c.Blend(row.BitmapData.BackColor, row.BitmapData.WorkingColorSpace));
                    config.RowSetColorF = (row, x, c) => row.UnsafeGetRefAs<ColorR8UnormSrgb>(x) = new ColorR8UnormSrgb(c.A >= 1f ? c.ToColor32()
                        : row.BitmapData.WorkingColorSpace == WorkingColorSpace.Linear ? c.Blend(row.BitmapData.BackColor.ToColorF()).ToColor32()
                        : c.ToColor32().Blend(row.BitmapData.BackColor));
                    break;

                default:
                    throw new InvalidOperationException(Res.InternalError($"{info.ColorType}/{info.AlphaType} is not supported directly in the sRGB color space. {nameof(SKBitmapExtensions.GetFallbackBitmapData)} should have been called from the caller."));
            }

            return BitmapDataFactory.CreateBitmapData(buffer, size, stride, config);
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502: Avoid excessive complexity", Justification = "Long but straightforward cases for the possible pixel formats.")]
        private static IReadWriteBitmapData CreateBitmapDataLinear(IntPtr buffer, SKImageInfo info, int stride,
            WorkingColorSpace workingColorSpace, Color32 backColor, byte alphaThreshold, Action? disposeCallback = null)
        {
            Debug.Assert(info.IsDirectlySupported());

            var size = new Size(info.Width, info.Height);
            KnownPixelFormat knownPixelFormat = info.AsKnownPixelFormat();

            // Natively supported formats
            if (knownPixelFormat != KnownPixelFormat.Undefined)
                return BitmapDataFactory.CreateBitmapData(buffer, size, stride, knownPixelFormat, workingColorSpace, backColor, alphaThreshold, disposeCallback);

            // Custom formats
            PixelFormatInfo pixelFormatInfo = info.GetInfo();
            var config = new CustomBitmapDataConfig
            {
                PixelFormat = pixelFormatInfo,
                BackColor = backColor,
                AlphaThreshold = alphaThreshold,
                WorkingColorSpace = workingColorSpace,
                DisposeCallback = disposeCallback,
                BackBufferIndependentPixelAccess = true,
            };

            switch ((info.ColorType, info.AlphaType))
            {
                case (SKColorType.Bgra8888, SKAlphaType.Unpremul):
                    config.RowGetColor32 = (row, x) => row.UnsafeGetRefAs<ColorBgra8888Linear>(x).ToColor32();
                    config.RowSetColor32 = (row, x, c) => row.UnsafeGetRefAs<ColorBgra8888Linear>(x) = new ColorBgra8888Linear(c);
                    config.RowGetColor64 = (row, x) => row.UnsafeGetRefAs<ColorBgra8888Linear>(x).ToColor64();
                    config.RowSetColor64 = (row, x, c) => row.UnsafeGetRefAs<ColorBgra8888Linear>(x) = new ColorBgra8888Linear(c);
                    config.RowGetColorF = (row, x) => row.UnsafeGetRefAs<ColorBgra8888Linear>(x).ToColorF();
                    config.RowSetColorF = (row, x, c) => row.UnsafeGetRefAs<ColorBgra8888Linear>(x) = new ColorBgra8888Linear(c);
                    break;

                case (SKColorType.Bgra8888, SKAlphaType.Premul):
                    config.RowGetColor32 = (row, x) => row.UnsafeGetRefAs<ColorPbgra8888Linear>(x).ToColor32();
                    config.RowGetColor64 = (row, x) => row.UnsafeGetRefAs<ColorPbgra8888Linear>(x).ToColor64();
                    config.RowGetPColorF = (row, x) => row.UnsafeGetRefAs<ColorPbgra8888Linear>(x).ToPColorF();
                    config.RowSetPColorF = (row, x, c) => row.UnsafeGetRefAs<ColorPbgra8888Linear>(x) = new ColorPbgra8888Linear(c);
                    break;

                case (SKColorType.Bgra8888, SKAlphaType.Opaque):
                    config.RowGetColor32 = (row, x) => row.UnsafeGetRefAs<ColorBgra8888Linear>(x).ToColor32().ToOpaque();
                    config.RowSetColor32 = (row, x, c) => row.UnsafeGetRefAs<ColorBgra8888Linear>(x) = c.A == Byte.MaxValue ? new ColorBgra8888Linear(c)
                        : row.BitmapData.WorkingColorSpace == WorkingColorSpace.Srgb ? new ColorBgra8888Linear(c.ToColor64().Blend(row.BitmapData.BackColor.ToColor64()))
                        : new ColorBgra8888Linear(c.ToColorF().Blend(row.BitmapData.BackColor.ToColorF()));
                    config.RowGetColor64 = (row, x) => row.UnsafeGetRefAs<ColorBgra8888Linear>(x).ToColor64().ToOpaque();
                    config.RowSetColor64 = (row, x, c) => row.UnsafeGetRefAs<ColorBgra8888Linear>(x) = c.A == UInt16.MaxValue ? new ColorBgra8888Linear(c)
                        : row.BitmapData.WorkingColorSpace == WorkingColorSpace.Srgb ? new ColorBgra8888Linear(c.Blend(row.BitmapData.BackColor.ToColor64()))
                        : new ColorBgra8888Linear(c.ToColorF().Blend(row.BitmapData.BackColor.ToColorF()));
                    config.RowGetColorF = (row, x) => row.UnsafeGetRefAs<ColorBgra8888Linear>(x).ToColorF();
                    config.RowSetColorF = (row, x, c) => row.UnsafeGetRefAs<ColorBgra8888Linear>(x) =
                        new ColorBgra8888Linear(c.A >= 1f ? c : c.Blend(row.BitmapData.BackColor.ToColorF(), row.BitmapData.WorkingColorSpace));
                    break;

                case (SKColorType.Rgba8888, SKAlphaType.Unpremul):
                    config.RowGetColor32 = (row, x) => row.UnsafeGetRefAs<ColorRgba8888Linear>(x).ToColor32();
                    config.RowSetColor32 = (row, x, c) => row.UnsafeGetRefAs<ColorRgba8888Linear>(x) = new ColorRgba8888Linear(c);
                    config.RowGetColor64 = (row, x) => row.UnsafeGetRefAs<ColorRgba8888Linear>(x).ToColor64();
                    config.RowSetColor64 = (row, x, c) => row.UnsafeGetRefAs<ColorRgba8888Linear>(x) = new ColorRgba8888Linear(c);
                    config.RowGetColorF = (row, x) => row.UnsafeGetRefAs<ColorRgba8888Linear>(x).ToColorF();
                    config.RowSetColorF = (row, x, c) => row.UnsafeGetRefAs<ColorRgba8888Linear>(x) = new ColorRgba8888Linear(c);
                    break;

                case (SKColorType.Rgba8888, SKAlphaType.Premul):
                    config.RowGetColor32 = (row, x) => row.UnsafeGetRefAs<ColorPrgba8888Linear>(x).ToColor32();
                    config.RowGetColor64 = (row, x) => row.UnsafeGetRefAs<ColorPrgba8888Linear>(x).ToColor64();
                    config.RowGetPColorF = (row, x) => row.UnsafeGetRefAs<ColorPrgba8888Linear>(x).ToPColorF();
                    config.RowSetPColorF = (row, x, c) => row.UnsafeGetRefAs<ColorPrgba8888Linear>(x) = new ColorPrgba8888Linear(c);
                    break;

                case (SKColorType.Rgba8888, SKAlphaType.Opaque):
                case (SKColorType.Rgb888x, _):
                    config.RowGetColor32 = (row, x) => row.UnsafeGetRefAs<ColorRgba8888Linear>(x).ToColor32().ToOpaque();
                    config.RowSetColor32 = (row, x, c) => row.UnsafeGetRefAs<ColorRgba8888Linear>(x) = c.A == Byte.MaxValue ? new ColorRgba8888Linear(c)
                        : row.BitmapData.WorkingColorSpace == WorkingColorSpace.Srgb ? new ColorRgba8888Linear(c.ToColor64().Blend(row.BitmapData.BackColor.ToColor64()))
                        : new ColorRgba8888Linear(c.ToColorF().Blend(row.BitmapData.BackColor.ToColorF()));
                    config.RowGetColor64 = (row, x) => row.UnsafeGetRefAs<ColorRgba8888Linear>(x).ToColor64().ToOpaque();
                    config.RowSetColor64 = (row, x, c) => row.UnsafeGetRefAs<ColorRgba8888Linear>(x) = c.A == UInt16.MaxValue ? new ColorRgba8888Linear(c)
                        : row.BitmapData.WorkingColorSpace == WorkingColorSpace.Srgb ? new ColorRgba8888Linear(c.Blend(row.BitmapData.BackColor.ToColor64()))
                        : new ColorRgba8888Linear(c.ToColorF().Blend(row.BitmapData.BackColor.ToColorF()));
                    config.RowGetColorF = (row, x) => row.UnsafeGetRefAs<ColorRgba8888Linear>(x).ToColorF();
                    config.RowSetColorF = (row, x, c) => row.UnsafeGetRefAs<ColorRgba8888Linear>(x) =
                        new ColorRgba8888Linear(c.A >= 1f ? c : c.Blend(row.BitmapData.BackColor.ToColorF(), row.BitmapData.WorkingColorSpace));
                    break;

                case (SKColorType.Gray8, _):
                    config.RowGetColor32 = (row, x) => row.UnsafeGetRefAs<ColorGray8Linear>(x).ToColor32();
                    config.RowSetColor32 = (row, x, c) => row.UnsafeGetRefAs<ColorGray8Linear>(x) =
                        row.BitmapData.WorkingColorSpace != WorkingColorSpace.Srgb ? new ColorGray8Linear(c.A == Byte.MaxValue ? c.ToColorF() : c.ToColorF().Blend(row.BitmapData.BackColor.ToColorF()))
                        : c.A == Byte.MaxValue ? new ColorGray8Linear(c)
                        : new ColorGray8Linear(c.ToColor64().Blend(row.BitmapData.BackColor.ToColor64()));
                    config.RowGetColor64 = (row, x) => row.UnsafeGetRefAs<ColorGray8Linear>(x).ToColor64();
                    config.RowSetColor64 = (row, x, c) => row.UnsafeGetRefAs<ColorGray8Linear>(x) = row.BitmapData.WorkingColorSpace == WorkingColorSpace.Srgb
                        ? new ColorGray8Linear(c.A == UInt16.MaxValue ? c : c.Blend(row.BitmapData.BackColor.ToColor64()))
                        : new ColorGray8Linear(c.A == UInt16.MaxValue ? c.ToColorF() : c.ToColorF().Blend(row.BitmapData.BackColor.ToColorF()));
                    config.RowGetColorF = (row, x) => row.UnsafeGetRefAs<ColorGray8Linear>(x).ToColorF();
                    config.RowSetColorF = (row, x, c) => row.UnsafeGetRefAs<ColorGray8Linear>(x) = row.BitmapData.WorkingColorSpace == WorkingColorSpace.Srgb
                        ? new ColorGray8Linear(c.A >= 1f ? c.ToColor64() : c.ToColor64().Blend(row.BitmapData.BackColor.ToColor64()))
                        : new ColorGray8Linear(c.A >= 1f ? c : c.Blend(row.BitmapData.BackColor.ToColorF()));
                    break;

                case (SKColorType.Rgb565, _):
                    config.RowGetColor32 = (row, x) => row.UnsafeGetRefAs<ColorRgb565Linear>(x).ToColor32();
                    config.RowSetColor32 = (row, x, c) => row.UnsafeGetRefAs<ColorRgb565Linear>(x) = c.A == Byte.MaxValue ? new ColorRgb565Linear(c)
                        : row.BitmapData.WorkingColorSpace == WorkingColorSpace.Srgb ? new ColorRgb565Linear(c.Blend(row.BitmapData.BackColor))
                        : new ColorRgb565Linear(c.ToColorF().Blend(row.BitmapData.BackColor.ToColorF()));
                    config.RowGetColorF = (row, x) => row.UnsafeGetRefAs<ColorRgb565Linear>(x).ToColorF();
                    config.RowSetColorF = (row, x, c) => row.UnsafeGetRefAs<ColorRgb565Linear>(x) =
                        new ColorRgb565Linear(c.A >= 1f ? c : c.Blend(row.BitmapData.BackColor.ToColorF(), row.BitmapData.WorkingColorSpace));
                    break;

                case (SKColorType.Rgba16161616, SKAlphaType.Unpremul):
                    config.RowGetColor64 = (row, x) => row.UnsafeGetRefAs<ColorRgba16161616Linear>(x).ToColor64();
                    config.RowGetColorF = (row, x) => row.UnsafeGetRefAs<ColorRgba16161616Linear>(x).ToColorF();
                    config.RowSetColorF = (row, x, c) => row.UnsafeGetRefAs<ColorRgba16161616Linear>(x) = new ColorRgba16161616Linear(c);
                break;

                case (SKColorType.Rgba16161616, SKAlphaType.Premul):
                    config.RowGetColor64 = (row, x) => row.UnsafeGetRefAs<ColorPrgba16161616Linear>(x).ToColor64();
                    config.RowGetPColorF = (row, x) => row.UnsafeGetRefAs<ColorPrgba16161616Linear>(x).ToPColorF();
                    config.RowSetPColorF = (row, x, c) => row.UnsafeGetRefAs<ColorPrgba16161616Linear>(x) = new ColorPrgba16161616Linear(c);
                    break;

                case (SKColorType.Rgba16161616, SKAlphaType.Opaque):
                    config.RowGetColor64 = (row, x) => row.UnsafeGetRefAs<ColorRgba16161616Linear>(x).ToColor64().ToOpaque();
                    config.RowSetColor64 = (row, x, c) => row.UnsafeGetRefAs<ColorRgba16161616Linear>(x) = c.A == UInt16.MaxValue ? new ColorRgba16161616Linear(c)
                        : row.BitmapData.WorkingColorSpace == WorkingColorSpace.Srgb ? new ColorRgba16161616Linear(c.Blend(row.BitmapData.BackColor.ToColor64()))
                        : new ColorRgba16161616Linear(c.ToColorF().Blend(row.BitmapData.BackColor.ToColorF()));
                    config.RowGetColorF = (row, x) => row.UnsafeGetRefAs<ColorRgba16161616Linear>(x).ToColorF();
                    config.RowSetColorF = (row, x, c) => row.UnsafeGetRefAs<ColorRgba16161616Linear>(x) =
                        new ColorRgba16161616Linear(c.A >= 1f ? c : c.Blend(row.BitmapData.BackColor.ToColorF(), row.BitmapData.WorkingColorSpace));
                    break;

                case (SKColorType.Bgra1010102, SKAlphaType.Unpremul):
                    config.RowGetColor64 = (row, x) => row.UnsafeGetRefAs<ColorBgra1010102Linear>(x).ToColor64();
                    config.RowSetColor64 = (row, x, c) => row.UnsafeGetRefAs<ColorBgra1010102Linear>(x) = new ColorBgra1010102Linear(c);
                    config.RowGetColorF = (row, x) => row.UnsafeGetRefAs<ColorBgra1010102Linear>(x).ToColorF();
                    config.RowSetColorF = (row, x, c) => row.UnsafeGetRefAs<ColorBgra1010102Linear>(x) = new ColorBgra1010102Linear(c);
                    break;

                case (SKColorType.Bgra1010102, SKAlphaType.Premul):
                    // Note that we set from ColorF instead of PColorF because special premultiplication is needed due to the different depth of A and RGB
                    config.RowGetColor64 = (row, x) => row.UnsafeGetRefAs<ColorPbgra1010102Linear>(x).ToColor64();
                    config.RowSetColor64 = (row, x, c) => row.UnsafeGetRefAs<ColorPbgra1010102Linear>(x) = new ColorPbgra1010102Linear(c);
                    config.RowGetPColorF = (row, x) => row.UnsafeGetRefAs<ColorPbgra1010102Linear>(x).ToPColorF();
                    config.RowSetColorF = (row, x, c) => row.UnsafeGetRefAs<ColorPbgra1010102Linear>(x) = new ColorPbgra1010102Linear(c);
                    break;

                case (SKColorType.Bgra1010102, SKAlphaType.Opaque):
                case (SKColorType.Bgr101010x, _):
                    config.RowGetColor64 = (row, x) => row.UnsafeGetRefAs<ColorBgra1010102Linear>(x).ToColor64().ToOpaque();
                    config.RowSetColor64 = (row, x, c) => row.UnsafeGetRefAs<ColorBgra1010102Linear>(x) = c.A == UInt16.MaxValue ? new ColorBgra1010102Linear(c)
                        : row.BitmapData.WorkingColorSpace == WorkingColorSpace.Srgb ? new ColorBgra1010102Linear(c.Blend(row.BitmapData.BackColor.ToColor64()))
                        : new ColorBgra1010102Linear(c.ToColorF().Blend(row.BitmapData.BackColor.ToColorF()));
                    config.RowGetColorF = (row, x) => row.UnsafeGetRefAs<ColorBgra1010102Linear>(x).ToColorF();
                    config.RowSetColorF = (row, x, c) => row.UnsafeGetRefAs<ColorBgra1010102Linear>(x) =
                        new ColorBgra1010102Linear(c.A >= 1f ? c : c.Blend(row.BitmapData.BackColor.ToColorF(), row.BitmapData.WorkingColorSpace));
                    break;

                case (SKColorType.Rgba1010102, SKAlphaType.Unpremul):
                    config.RowGetColor64 = (row, x) => row.UnsafeGetRefAs<ColorRgba1010102Linear>(x).ToColor64();
                    config.RowSetColor64 = (row, x, c) => row.UnsafeGetRefAs<ColorRgba1010102Linear>(x) = new ColorRgba1010102Linear(c);
                    config.RowGetColorF = (row, x) => row.UnsafeGetRefAs<ColorRgba1010102Linear>(x).ToColorF();
                    config.RowSetColorF = (row, x, c) => row.UnsafeGetRefAs<ColorRgba1010102Linear>(x) = new ColorRgba1010102Linear(c);
                    break;

                case (SKColorType.Rgba1010102, SKAlphaType.Premul):
                    // Note that we set from ColorF instead of PColorF because special premultiplication is needed due to the different depth of A and RGB
                    config.RowGetColor64 = (row, x) => row.UnsafeGetRefAs<ColorPrgba1010102Linear>(x).ToColor64();
                    config.RowSetColor64 = (row, x, c) => row.UnsafeGetRefAs<ColorPrgba1010102Linear>(x) = new ColorPrgba1010102Linear(c);
                    config.RowGetPColorF = (row, x) => row.UnsafeGetRefAs<ColorPrgba1010102Linear>(x).ToPColorF();
                    config.RowSetColorF = (row, x, c) => row.UnsafeGetRefAs<ColorPrgba1010102Linear>(x) = new ColorPrgba1010102Linear(c);
                    break;

                case (SKColorType.Rgba1010102, SKAlphaType.Opaque):
                case (SKColorType.Rgb101010x, _):
                    config.RowGetColor64 = (row, x) => row.UnsafeGetRefAs<ColorRgba1010102Linear>(x).ToColor64().ToOpaque();
                    config.RowSetColor64 = (row, x, c) => row.UnsafeGetRefAs<ColorRgba1010102Linear>(x) = c.A == UInt16.MaxValue ? new ColorRgba1010102Linear(c)
                        : row.BitmapData.WorkingColorSpace == WorkingColorSpace.Srgb ? new ColorRgba1010102Linear(c.Blend(row.BitmapData.BackColor.ToColor64()))
                        : new ColorRgba1010102Linear(c.ToColorF().Blend(row.BitmapData.BackColor.ToColorF()));
                    config.RowGetColorF = (row, x) => row.UnsafeGetRefAs<ColorRgba1010102Linear>(x).ToColorF();
                    config.RowSetColorF = (row, x, c) => row.UnsafeGetRefAs<ColorRgba1010102Linear>(x) =
                        new ColorRgba1010102Linear(c.A >= 1f ? c : c.Blend(row.BitmapData.BackColor.ToColorF(), row.BitmapData.WorkingColorSpace));
                    break;

                case (SKColorType.Argb4444, SKAlphaType.Unpremul):
                    config.RowGetColor32 = (row, x) => row.UnsafeGetRefAs<ColorArgb4444Linear>(x).ToColor32();
                    config.RowSetColor32 = (row, x, c) => row.UnsafeGetRefAs<ColorArgb4444Linear>(x) = new ColorArgb4444Linear(c);
                    config.RowGetColorF = (row, x) => row.UnsafeGetRefAs<ColorArgb4444Linear>(x).ToColorF();
                    config.RowSetColorF = (row, x, c) => row.UnsafeGetRefAs<ColorArgb4444Linear>(x) = new ColorArgb4444Linear(c);
                    break;

                case (SKColorType.Argb4444, SKAlphaType.Premul):
                    config.RowGetColor32 = (row, x) => row.UnsafeGetRefAs<ColorPargb4444Linear>(x).ToColor32();
                    config.RowGetPColorF = (row, x) => row.UnsafeGetRefAs<ColorPargb4444Linear>(x).ToPColorF();
                    config.RowSetPColorF = (row, x, c) => row.UnsafeGetRefAs<ColorPargb4444Linear>(x) = new ColorPargb4444Linear(c);
                    break;

                case (SKColorType.Argb4444, SKAlphaType.Opaque):
                    config.RowGetColor32 = (row, x) => row.UnsafeGetRefAs<ColorArgb4444Linear>(x).ToColor32().ToOpaque();
                    config.RowSetColor32 = (row, x, c) => row.UnsafeGetRefAs<ColorArgb4444Linear>(x) = c.A == Byte.MaxValue ? new ColorArgb4444Linear(c)
                        : row.BitmapData.WorkingColorSpace == WorkingColorSpace.Srgb ? new ColorArgb4444Linear(c.Blend(row.BitmapData.BackColor))
                        : new ColorArgb4444Linear(c.ToColorF().Blend(row.BitmapData.BackColor.ToColorF()));
                    config.RowGetColorF = (row, x) => row.UnsafeGetRefAs<ColorArgb4444Linear>(x).ToColorF().ToOpaque();
                    config.RowSetColorF = (row, x, c) => row.UnsafeGetRefAs<ColorArgb4444Linear>(x) =
                        new ColorArgb4444Linear(c.A >= 1f ? c : c.Blend(row.BitmapData.BackColor.ToColorF(), row.BitmapData.WorkingColorSpace));
                    break;

                case (SKColorType.RgbaF32, SKAlphaType.Opaque):
                    config.RowGetColorF = (row, x) => row.UnsafeGetRefAs<ColorF>(x).ToOpaque();
                    config.RowSetColorF = (row, x, c) => row.UnsafeGetRefAs<ColorF>(x) =
                        c.A >= 1f ? c : c.Blend(row.BitmapData.BackColor.ToColorF(), row.BitmapData.WorkingColorSpace);
                    break;

                case (SKColorType.RgbaF16, SKAlphaType.Unpremul): 
                    config.RowGetColorF = (row, x) => row.UnsafeGetRefAs<ColorRgbaF16Linear>(x).ToColorF();
                    config.RowSetColorF = (row, x, c) => row.UnsafeGetRefAs<ColorRgbaF16Linear>(x) = new ColorRgbaF16Linear(c);
                    break;

                case (SKColorType.RgbaF16, SKAlphaType.Premul):
                    config.RowGetPColorF = (row, x) => row.UnsafeGetRefAs<ColorPrgbaF16Linear>(x).ToPColorF();
                    config.RowSetPColorF = (row, x, c) => row.UnsafeGetRefAs<ColorPrgbaF16Linear>(x) = new ColorPrgbaF16Linear(c);
                    break;

                case (SKColorType.RgbaF16, SKAlphaType.Opaque):
                    config.RowGetColorF = (row, x) => row.UnsafeGetRefAs<ColorRgbaF16Linear>(x).ToColorF().ToOpaque();
                    config.RowSetColorF = (row, x, c) => row.UnsafeGetRefAs<ColorRgbaF16Linear>(x) =
                        new ColorRgbaF16Linear(c.A >= 1f ? c : c.Blend(row.BitmapData.BackColor.ToColorF(), row.BitmapData.WorkingColorSpace));
                    break;

                case (SKColorType.RgbaF16Clamped, SKAlphaType.Unpremul):
                    config.RowGetColorF = (row, x) => row.UnsafeGetRefAs<ColorRgbaF16Linear>(x).ToColorF().Clip();
                    config.RowSetColorF = (row, x, c) => row.UnsafeGetRefAs<ColorRgbaF16Linear>(x) = new ColorRgbaF16Linear(c.Clip());
                    break;

                case (SKColorType.RgbaF16Clamped, SKAlphaType.Premul):
                    config.RowGetPColorF = (row, x) => row.UnsafeGetRefAs<ColorPrgbaF16Linear>(x).ToPColorF().Clip();
                    config.RowSetPColorF = (row, x, c) => row.UnsafeGetRefAs<ColorPrgbaF16Linear>(x) = new ColorPrgbaF16Linear(c.Clip());
                    break;

                case (SKColorType.RgbaF16Clamped, SKAlphaType.Opaque):
                    config.RowGetColorF = (row, x) => row.UnsafeGetRefAs<ColorRgbaF16Linear>(x).ToColorF().ToOpaque().Clip();
                    config.RowSetColorF = (row, x, c) => row.UnsafeGetRefAs<ColorRgbaF16Linear>(x) =
                        new ColorRgbaF16Linear((c.A >= 1f ? c : c.Blend(row.BitmapData.BackColor.ToColorF(), row.BitmapData.WorkingColorSpace)).Clip());
                    break;

                case (SKColorType.Alpha8, _):
                    // Defining everything to spare a lot of unnecessary conversions (though the preferred color type is Color32)
                    config.RowGetColor32 = (row, x) => row.UnsafeGetRefAs<ColorAlpha8>(x).ToColor32();
                    config.RowSetColor32 = (row, x, c) => row.UnsafeGetRefAs<ColorAlpha8>(x) = new ColorAlpha8(c);
                    config.RowGetPColor32 = (row, x) => row.UnsafeGetRefAs<ColorAlpha8>(x).ToPColor32();
                    config.RowSetPColor32 = (row, x, c) => row.UnsafeGetRefAs<ColorAlpha8>(x) = new ColorAlpha8(c);
                    config.RowGetColor64 = (row, x) => row.UnsafeGetRefAs<ColorAlpha8>(x).ToColor64();
                    config.RowSetColor64 = (row, x, c) => row.UnsafeGetRefAs<ColorAlpha8>(x) = new ColorAlpha8(c);
                    config.RowGetPColor64 = (row, x) => row.UnsafeGetRefAs<ColorAlpha8>(x).ToPColor64();
                    config.RowSetPColor64 = (row, x, c) => row.UnsafeGetRefAs<ColorAlpha8>(x) = new ColorAlpha8(c);
                    config.RowGetColorF = (row, x) => row.UnsafeGetRefAs<ColorAlpha8>(x).ToColorF();
                    config.RowSetColorF = (row, x, c) => row.UnsafeGetRefAs<ColorAlpha8>(x) = new ColorAlpha8(c);
                    config.RowGetPColorF = (row, x) => row.UnsafeGetRefAs<ColorAlpha8>(x).ToPColorF();
                    config.RowSetPColorF = (row, x, c) => row.UnsafeGetRefAs<ColorAlpha8>(x) = new ColorAlpha8(c);
                    break;

                case (SKColorType.Alpha16, _):
                    // Defining everything to spare a lot of unnecessary conversions (though the preferred color type is Color64)
                    config.RowGetColor32 = (row, x) => row.UnsafeGetRefAs<ColorAlpha16>(x).ToColor32();
                    config.RowSetColor32 = (row, x, c) => row.UnsafeGetRefAs<ColorAlpha16>(x) = new ColorAlpha16(c);
                    config.RowGetPColor32 = (row, x) => row.UnsafeGetRefAs<ColorAlpha16>(x).ToPColor32();
                    config.RowSetPColor32 = (row, x, c) => row.UnsafeGetRefAs<ColorAlpha16>(x) = new ColorAlpha16(c);
                    config.RowGetColor64 = (row, x) => row.UnsafeGetRefAs<ColorAlpha16>(x).ToColor64();
                    config.RowSetColor64 = (row, x, c) => row.UnsafeGetRefAs<ColorAlpha16>(x) = new ColorAlpha16(c);
                    config.RowGetPColor64 = (row, x) => row.UnsafeGetRefAs<ColorAlpha16>(x).ToPColor64();
                    config.RowSetPColor64 = (row, x, c) => row.UnsafeGetRefAs<ColorAlpha16>(x) = new ColorAlpha16(c);
                    config.RowGetColorF = (row, x) => row.UnsafeGetRefAs<ColorAlpha16>(x).ToColorF();
                    config.RowSetColorF = (row, x, c) => row.UnsafeGetRefAs<ColorAlpha16>(x) = new ColorAlpha16(c);
                    config.RowGetPColorF = (row, x) => row.UnsafeGetRefAs<ColorAlpha16>(x).ToPColorF();
                    config.RowSetPColorF = (row, x, c) => row.UnsafeGetRefAs<ColorAlpha16>(x) = new ColorAlpha16(c);
                    break;

                case (SKColorType.AlphaF16, _):
                    // Defining everything to spare a lot of unnecessary conversions (though the preferred color type is ColorF)
                    config.RowGetColor32 = (row, x) => row.UnsafeGetRefAs<ColorAlphaF16>(x).ToColor32();
                    config.RowSetColor32 = (row, x, c) => row.UnsafeGetRefAs<ColorAlphaF16>(x) = new ColorAlphaF16(c);
                    config.RowGetPColor32 = (row, x) => row.UnsafeGetRefAs<ColorAlphaF16>(x).ToPColor32();
                    config.RowSetPColor32 = (row, x, c) => row.UnsafeGetRefAs<ColorAlphaF16>(x) = new ColorAlphaF16(c);
                    config.RowGetColor64 = (row, x) => row.UnsafeGetRefAs<ColorAlphaF16>(x).ToColor64();
                    config.RowSetColor64 = (row, x, c) => row.UnsafeGetRefAs<ColorAlphaF16>(x) = new ColorAlphaF16(c);
                    config.RowGetPColor64 = (row, x) => row.UnsafeGetRefAs<ColorAlphaF16>(x).ToPColor64();
                    config.RowSetPColor64 = (row, x, c) => row.UnsafeGetRefAs<ColorAlphaF16>(x) = new ColorAlphaF16(c);
                    config.RowGetColorF = (row, x) => row.UnsafeGetRefAs<ColorAlphaF16>(x).ToColorF();
                    config.RowSetColorF = (row, x, c) => row.UnsafeGetRefAs<ColorAlphaF16>(x) = new ColorAlphaF16(c);
                    config.RowGetPColorF = (row, x) => row.UnsafeGetRefAs<ColorAlphaF16>(x).ToPColorF();
                    config.RowSetPColorF = (row, x, c) => row.UnsafeGetRefAs<ColorAlphaF16>(x) = new ColorAlphaF16(c);
                    break;

                case (SKColorType.Rg88, _):
                    config.RowGetColor32 = (row, x) => row.UnsafeGetRefAs<ColorRg88Linear>(x).ToColor32();
                    config.RowSetColor32 = (row, x, c) => row.UnsafeGetRefAs<ColorRg88Linear>(x) = c.A == Byte.MaxValue ? new ColorRg88Linear(c)
                        : row.BitmapData.WorkingColorSpace == WorkingColorSpace.Srgb ? new ColorRg88Linear(c.ToColor64().Blend(row.BitmapData.BackColor.ToColor64()))
                        : new ColorRg88Linear(c.ToColorF().Blend(row.BitmapData.BackColor.ToColorF()));
                    config.RowGetColor64 = (row, x) => row.UnsafeGetRefAs<ColorRg88Linear>(x).ToColor64();
                    config.RowSetColor64 = (row, x, c) => row.UnsafeGetRefAs<ColorRg88Linear>(x) = c.A == UInt16.MaxValue ? new ColorRg88Linear(c)
                        : row.BitmapData.WorkingColorSpace == WorkingColorSpace.Srgb ? new ColorRg88Linear(c.Blend(row.BitmapData.BackColor.ToColor64()))
                        : new ColorRg88Linear(c.ToColorF().Blend(row.BitmapData.BackColor.ToColorF()));
                    config.RowGetColorF = (row, x) => row.UnsafeGetRefAs<ColorRg88Linear>(x).ToColorF();
                    config.RowSetColorF = (row, x, c) => row.UnsafeGetRefAs<ColorRg88Linear>(x) =
                        new ColorRg88Linear(c.A >= 1f ? c : c.Blend(row.BitmapData.BackColor.ToColorF(), row.BitmapData.WorkingColorSpace));
                    break;

                case (SKColorType.Rg1616, _):
                    config.RowGetColor64 = (row, x) => row.UnsafeGetRefAs<ColorRg1616Linear>(x).ToColor64();
                    config.RowSetColor64 = (row, x, c) => row.UnsafeGetRefAs<ColorRg1616Linear>(x) = c.A == UInt16.MaxValue ? new ColorRg1616Linear(c)
                        : row.BitmapData.WorkingColorSpace == WorkingColorSpace.Srgb ? new ColorRg1616Linear(c.Blend(row.BitmapData.BackColor.ToColor64()))
                        : new ColorRg1616Linear(c.ToColorF().Blend(row.BitmapData.BackColor.ToColorF()));
                    config.RowGetColorF = (row, x) => row.UnsafeGetRefAs<ColorRg1616Linear>(x).ToColorF();
                    config.RowSetColorF = (row, x, c) => row.UnsafeGetRefAs<ColorRg1616Linear>(x) =
                        new ColorRg1616Linear(c.A >= 1f ? c : c.Blend(row.BitmapData.BackColor.ToColorF(), row.BitmapData.WorkingColorSpace));
                    break;

                case (SKColorType.RgF16, _):
                    config.RowGetColorF = (row, x) => row.UnsafeGetRefAs<ColorRgF16Linear>(x).ToColorF();
                    config.RowSetColorF = (row, x, c) => row.UnsafeGetRefAs<ColorRgF16Linear>(x) =
                        new ColorRgF16Linear(c.A >= 1f ? c : c.Blend(row.BitmapData.BackColor.ToColorF(), row.BitmapData.WorkingColorSpace));
                    break;

                case (SKColorType.Bgr101010xXR, _):
                    config.RowGetColor64 = (row, x) => row.UnsafeGetRefAs<ColorBgr101010XRLinear>(x).ToColor64().ToOpaque();
                    config.RowSetColor64 = (row, x, c) => row.UnsafeGetRefAs<ColorBgr101010XRLinear>(x) = c.A == UInt16.MaxValue ? new ColorBgr101010XRLinear(c)
                        : row.BitmapData.WorkingColorSpace == WorkingColorSpace.Srgb ? new ColorBgr101010XRLinear(c.Blend(row.BitmapData.BackColor.ToColor64()))
                        : new ColorBgr101010XRLinear(c.ToColorF().Blend(row.BitmapData.BackColor.ToColorF()));
                    config.RowGetColorF = (row, x) => row.UnsafeGetRefAs<ColorBgr101010XRLinear>(x).ToColorF();
                    config.RowSetColorF = (row, x, c) => row.UnsafeGetRefAs<ColorBgr101010XRLinear>(x) =
                        new ColorBgr101010XRLinear(c.A >= 1f ? c : c.Blend(row.BitmapData.BackColor.ToColorF(), row.BitmapData.WorkingColorSpace));
                    break;

                case (SKColorType.Srgba8888, SKAlphaType.Unpremul):
                    // Note that there is no ColorSrgba8888Linear type, because that would be the same as the existing ColorRgba8888Srgb, and that's exactly what we use here
                    config.RowGetColor32 = (row, x) => row.UnsafeGetRefAs<ColorRgba8888Srgb>(x).ToColor32();
                    config.RowSetColor32 = (row, x, c) => row.UnsafeGetRefAs<ColorRgba8888Srgb>(x) = new ColorRgba8888Srgb(c);
                    break;

                case (SKColorType.Srgba8888, SKAlphaType.Premul):
                    config.RowGetPColorF = (row, x) => row.UnsafeGetRefAs<ColorPsrgba8888Linear>(x).ToPColorF();
                    config.RowSetPColorF = (row, x, c) => row.UnsafeGetRefAs<ColorPsrgba8888Linear>(x) = new ColorPsrgba8888Linear(c);
                    break;

                case (SKColorType.Srgba8888, SKAlphaType.Opaque):
                    // Note that there is no ColorSrgba8888Linear type, because that would be the same as the existing ColorRgba8888Srgb, and that's exactly what we use here.
                    // Please note though that the blending is different from the (SKColorType.Rgba8888, SKAlphaType.Opaque) case, because default working color space is linear here.
                    config.RowGetColor32 = (row, x) => row.UnsafeGetRefAs<ColorRgba8888Srgb>(x).ToColor32().ToOpaque();
                    config.RowSetColor32 = (row, x, c) => row.UnsafeGetRefAs<ColorRgba8888Srgb>(x) = new ColorRgba8888Srgb(c.A == Byte.MaxValue ? c
                        : row.BitmapData.WorkingColorSpace != WorkingColorSpace.Srgb ? c.Blend(row.BitmapData.BackColor, WorkingColorSpace.Linear)
                        : c.Blend(row.BitmapData.BackColor));
                    config.RowSetColorF = (row, x, c) => row.UnsafeGetRefAs<ColorRgba8888Srgb>(x) = new ColorRgba8888Srgb(c.A >= 1f ? c.ToColor32()
                        : row.BitmapData.WorkingColorSpace != WorkingColorSpace.Srgb ? c.Blend(row.BitmapData.BackColor.ToColorF()).ToColor32()
                        : c.ToColor32().Blend(row.BitmapData.BackColor));
                    break;

                case (SKColorType.R8Unorm, _):
                    config.RowGetColor32 = (row, x) => row.UnsafeGetRefAs<ColorR8UnormLinear>(x).ToColor32();
                    config.RowSetColor32 = (row, x, c) => row.UnsafeGetRefAs<ColorR8UnormLinear>(x) = c.A == Byte.MaxValue ? new ColorR8UnormLinear(c)
                        : row.BitmapData.WorkingColorSpace == WorkingColorSpace.Srgb ? new ColorR8UnormLinear(c.ToColor64().Blend(row.BitmapData.BackColor.ToColor64()))
                        : new ColorR8UnormLinear(c.ToColorF().Blend(row.BitmapData.BackColor.ToColorF()));
                    config.RowGetColor64 = (row, x) => row.UnsafeGetRefAs<ColorR8UnormLinear>(x).ToColor64();
                    config.RowSetColor64 = (row, x, c) => row.UnsafeGetRefAs<ColorR8UnormLinear>(x) = c.A == UInt16.MaxValue ? new ColorR8UnormLinear(c)
                        : row.BitmapData.WorkingColorSpace == WorkingColorSpace.Srgb ? new ColorR8UnormLinear(c.Blend(row.BitmapData.BackColor.ToColor64()))
                        : new ColorR8UnormLinear(c.ToColorF().Blend(row.BitmapData.BackColor.ToColorF()));
                    config.RowGetColorF = (row, x) => row.UnsafeGetRefAs<ColorR8UnormLinear>(x).ToColorF();
                    config.RowSetColorF = (row, x, c) => row.UnsafeGetRefAs<ColorR8UnormLinear>(x) =
                        new ColorR8UnormLinear(c.A >= 1f ? c : c.Blend(row.BitmapData.BackColor.ToColorF(), row.BitmapData.WorkingColorSpace));
                    break;

                default:
                    throw new InvalidOperationException(Res.InternalError($"{info.ColorType}/{info.AlphaType} is not supported directly in the linear color space. {nameof(SKBitmapExtensions.GetFallbackBitmapData)} should have been called from the caller."));
            }

            return BitmapDataFactory.CreateBitmapData(buffer, size, stride, config);
        }

        #endregion

        #endregion
    }
}