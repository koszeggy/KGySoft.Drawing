﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: NativeBitmapDataFactory.cs
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
                    config.RowGetColor32 = (row, x) => row.UnsafeGetRefAs<ColorRgbaF16Srgb>(x).ToColor32();
                    config.RowSetColor32 = (row, x, c) => row.UnsafeGetRefAs<ColorRgbaF16Srgb>(x) = new ColorRgbaF16Srgb(c);
                    config.RowGetColor64 = (row, x) => row.UnsafeGetRefAs<ColorRgbaF16Srgb>(x).ToColor64();
                    config.RowSetColor64 = (row, x, c) => row.UnsafeGetRefAs<ColorRgbaF16Srgb>(x) = new ColorRgbaF16Srgb(c);
                    config.RowGetColorF = (row, x) => row.UnsafeGetRefAs<ColorRgbaF16Srgb>(x).ToColorF();
                    config.RowSetColorF = (row, x, c) => row.UnsafeGetRefAs<ColorRgbaF16Srgb>(x) = new ColorRgbaF16Srgb(c);
                    break;

                case (SKColorType.RgbaF16, SKAlphaType.Premul):
                case (SKColorType.RgbaF16Clamped, SKAlphaType.Premul):
                    // Though 64-bit colors are preferred for this format we set also ColorF methods to save a few operations when using with [P]ColorF
                    config.RowGetPColor32 = (row, x) => row.UnsafeGetRefAs<ColorPrgbaF16Srgb>(x).ToPColor32();
                    config.RowSetPColor32 = (row, x, c) => row.UnsafeGetRefAs<ColorPrgbaF16Srgb>(x) = new ColorPrgbaF16Srgb(c);
                    config.RowGetPColor64 = (row, x) => row.UnsafeGetRefAs<ColorPrgbaF16Srgb>(x).ToPColor64();
                    config.RowSetPColor64 = (row, x, c) => row.UnsafeGetRefAs<ColorPrgbaF16Srgb>(x) = new ColorPrgbaF16Srgb(c);
                    config.RowGetColorF = (row, x) => row.UnsafeGetRefAs<ColorPrgbaF16Srgb>(x).ToColorF();
                    config.RowSetColorF = (row, x, c) => row.UnsafeGetRefAs<ColorPrgbaF16Srgb>(x) = new ColorPrgbaF16Srgb(c);
                    break;

                case (SKColorType.RgbaF16, SKAlphaType.Opaque):
                case (SKColorType.RgbaF16Clamped, SKAlphaType.Opaque):
                    config.RowGetColor32 = (row, x) => row.UnsafeGetRefAs<ColorRgbaF16Srgb>(x).ToColor32().ToOpaque();
                    config.RowSetColor32 = (row, x, c) => row.UnsafeGetRefAs<ColorRgbaF16Srgb>(x) = c.A == Byte.MaxValue ? new ColorRgbaF16Srgb(c)
                        : row.BitmapData.WorkingColorSpace == WorkingColorSpace.Linear ? new ColorRgbaF16Srgb(c.ToColorF().Blend(row.BitmapData.BackColor.ToColorF()))
                        : new ColorRgbaF16Srgb(c.ToColor64().Blend(row.BitmapData.BackColor.ToColor64()));
                    config.RowGetColor64 = (row, x) => row.UnsafeGetRefAs<ColorRgbaF16Srgb>(x).ToColor64().ToOpaque();
                    config.RowSetColor64 = (row, x, c) => row.UnsafeGetRefAs<ColorRgbaF16Srgb>(x) = c.A == UInt16.MaxValue ? new ColorRgbaF16Srgb(c)
                        : row.BitmapData.WorkingColorSpace == WorkingColorSpace.Linear ? new ColorRgbaF16Srgb(c.ToColorF().Blend(row.BitmapData.BackColor.ToColorF()))
                        : new ColorRgbaF16Srgb(c.Blend(row.BitmapData.BackColor.ToColor64()));
                    config.RowGetColorF = (row, x) => row.UnsafeGetRefAs<ColorRgbaF16Srgb>(x).ToColorF().ToOpaque();
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
                    config.RowGetColor32 = (row, x) => row.UnsafeGetRefAs<ColorRg88Srgb>(x).ToColor32().ToOpaque();
                    config.RowSetColor32 = (row, x, c) => row.UnsafeGetRefAs<ColorRg88Srgb>(x) =
                        new ColorRg88Srgb(c.A == Byte.MaxValue ? c : c.Blend(row.BitmapData.BackColor, row.BitmapData.WorkingColorSpace));
                    config.RowSetColorF = (row, x, c) => row.UnsafeGetRefAs<ColorRg88Srgb>(x) = new ColorRg88Srgb(c.A >= 1f ? c.ToColor32()
                        : row.BitmapData.WorkingColorSpace == WorkingColorSpace.Linear ? c.Blend(row.BitmapData.BackColor.ToColorF()).ToColor32()
                        : c.ToColor32().Blend(row.BitmapData.BackColor));
                    break;

                case (SKColorType.Rg1616, _):
                    config.RowGetColor64 = (row, x) => row.UnsafeGetRefAs<ColorRg1616Srgb>(x).ToColor64().ToOpaque();
                    config.RowSetColor64 = (row, x, c) => row.UnsafeGetRefAs<ColorRg1616Srgb>(x) =
                        new ColorRg1616Srgb(c.A == UInt16.MaxValue ? c : c.Blend(row.BitmapData.BackColor.ToColor64(), row.BitmapData.WorkingColorSpace));
                    config.RowSetColorF = (row, x, c) => row.UnsafeGetRefAs<ColorRg1616Srgb>(x) = new ColorRg1616Srgb(c.A >= 1f ? c.ToColor64()
                        : row.BitmapData.WorkingColorSpace == WorkingColorSpace.Linear ? c.Blend(row.BitmapData.BackColor.ToColorF()).ToColor64()
                        : c.ToColor64().Blend(row.BitmapData.BackColor.ToColor64()));
                    break;

                case (SKColorType.RgF16, _):
                    config.RowGetColor32 = (row, x) => row.UnsafeGetRefAs<ColorRgF16Srgb>(x).ToColor32().ToOpaque();
                    config.RowSetColor32 = (row, x, c) => row.UnsafeGetRefAs<ColorRgF16Srgb>(x) = c.A == Byte.MaxValue ? new ColorRgF16Srgb(c)
                        : row.BitmapData.WorkingColorSpace == WorkingColorSpace.Linear ? new ColorRgF16Srgb(c.ToColorF().Blend(row.BitmapData.BackColor.ToColorF()))
                        : new ColorRgF16Srgb(c.ToColor64().Blend(row.BitmapData.BackColor.ToColor64()));
                    config.RowGetColor64 = (row, x) => row.UnsafeGetRefAs<ColorRgF16Srgb>(x).ToColor64().ToOpaque();
                    config.RowSetColor64 = (row, x, c) => row.UnsafeGetRefAs<ColorRgF16Srgb>(x) = c.A == UInt16.MaxValue ? new ColorRgF16Srgb(c)
                        : row.BitmapData.WorkingColorSpace == WorkingColorSpace.Linear ? new ColorRgF16Srgb(c.ToColorF().Blend(row.BitmapData.BackColor.ToColorF()))
                        : new ColorRgF16Srgb(c.Blend(row.BitmapData.BackColor.ToColor64()));
                    config.RowGetColorF = (row, x) => row.UnsafeGetRefAs<ColorRgF16Srgb>(x).ToColorF().ToOpaque();
                    config.RowSetColorF = (row, x, c) => row.UnsafeGetRefAs<ColorRgF16Srgb>(x) = c.A >= 1f ? new ColorRgF16Srgb(c)
                        : row.BitmapData.WorkingColorSpace == WorkingColorSpace.Linear ? new ColorRgF16Srgb(c.Blend(row.BitmapData.BackColor.ToColorF()))
                        : ColorRgF16Srgb.FromSrgb(c.ToSrgb().Blend(row.BitmapData.BackColor.ToColorF(false)));
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

            PixelFormatInfo pixelFormatInfo = info.GetInfo();
            return info switch
            {
                // Bgra8888/Unpremul
                { ColorType: SKColorType.Bgra8888, AlphaType: SKAlphaType.Unpremul } => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                    (row, x) => row.UnsafeGetRefAs<ColorBgra8888Linear>(x).ToColor32(),
                    (row, x, c) => row.UnsafeGetRefAs<ColorBgra8888Linear>(x) = new ColorBgra8888Linear(c),
                    workingColorSpace, backColor, alphaThreshold, disposeCallback),

                // Bgra8888/Premul
                { ColorType: SKColorType.Bgra8888, AlphaType: SKAlphaType.Premul } => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                    (row, x) => row.UnsafeGetRefAs<ColorPbgra8888Linear>(x).ToColor32(),
                    (row, x, c) => row.UnsafeGetRefAs<ColorPbgra8888Linear>(x) = new ColorPbgra8888Linear(c),
                    workingColorSpace, backColor, alphaThreshold, disposeCallback),

                // Bgra8888/Opaque
                { ColorType: SKColorType.Bgra8888, AlphaType: SKAlphaType.Opaque } => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                    (row, x) => row.UnsafeGetRefAs<ColorBgra8888Linear>(x).ToColor32().ToOpaque(),
                    (row, x, c) => row.UnsafeGetRefAs<ColorBgra8888Linear>(x) = c.A == Byte.MaxValue ? new ColorBgra8888Linear(c)
                        : row.BitmapData.WorkingColorSpace == WorkingColorSpace.Srgb ? new ColorBgra8888Linear(c.ToColor64().Blend(row.BitmapData.BackColor.ToColor64()))
                        : new ColorBgra8888Linear(c.ToColorF().Blend(row.BitmapData.BackColor.ToColorF())),
                    workingColorSpace, backColor, alphaThreshold, disposeCallback),

                // Rgba8888/Unpremul
                { ColorType: SKColorType.Rgba8888, AlphaType: SKAlphaType.Unpremul } => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                    (row, x) => row.UnsafeGetRefAs<ColorRgba8888Linear>(x).ToColor32(),
                    (row, x, c) => row.UnsafeGetRefAs<ColorRgba8888Linear>(x) = new ColorRgba8888Linear(c),
                    workingColorSpace, backColor, alphaThreshold, disposeCallback),

                // Rgba8888/Premul
                { ColorType: SKColorType.Rgba8888, AlphaType: SKAlphaType.Premul } => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                    (row, x) => row.UnsafeGetRefAs<ColorPrgba8888Linear>(x).ToColor32(),
                    (row, x, c) => row.UnsafeGetRefAs<ColorPrgba8888Linear>(x) = new ColorPrgba8888Linear(c),
                    workingColorSpace, backColor, alphaThreshold, disposeCallback),

                // Rgba8888/Opaque, Rgb888x
                { ColorType: SKColorType.Rgba8888, AlphaType: SKAlphaType.Opaque } or { ColorType: SKColorType.Rgb888x }
                    => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                        (row, x) => row.UnsafeGetRefAs<ColorRgba8888Linear>(x).ToColor32().ToOpaque(),
                        (row, x, c) => row.UnsafeGetRefAs<ColorRgba8888Linear>(x) = c.A == Byte.MaxValue ? new ColorRgba8888Linear(c)
                            : row.BitmapData.WorkingColorSpace == WorkingColorSpace.Srgb ? new ColorRgba8888Linear(c.ToColor64().Blend(row.BitmapData.BackColor.ToColor64()))
                            : new ColorRgba8888Linear(c.ToColorF().Blend(row.BitmapData.BackColor.ToColorF())),
                        workingColorSpace, backColor, alphaThreshold, disposeCallback),

                // Gray8
                { ColorType: SKColorType.Gray8 } => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                    (row, x) => row.UnsafeGetRefAs<ColorGray8Linear>(x).ToColor32(),
                    (row, x, c) => row.UnsafeGetRefAs<ColorGray8Linear>(x) = row.BitmapData.WorkingColorSpace == WorkingColorSpace.Srgb
                        ? new ColorGray8Linear(c.A == Byte.MaxValue ? c : c.Blend(row.BitmapData.BackColor))
                        : new ColorGray8Linear(c.A == Byte.MaxValue ? c.ToColorF() : c.ToColorF().Blend(row.BitmapData.BackColor.ToColorF())),
                    workingColorSpace, backColor, alphaThreshold, disposeCallback),

                // Rgb565
                { ColorType: SKColorType.Rgb565 } => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                    (row, x) => row.UnsafeGetRefAs<ColorRgb565Linear>(x).ToColor32(),
                    (row, x, c) => row.UnsafeGetRefAs<ColorRgb565Linear>(x) = row.BitmapData.WorkingColorSpace == WorkingColorSpace.Srgb
                        ? new ColorRgb565Linear(c.A == Byte.MaxValue ? c : c.Blend(row.BitmapData.BackColor))
                        : new ColorRgb565Linear(c.A == Byte.MaxValue ? c.ToColorF() : c.ToColorF().Blend(row.BitmapData.BackColor.ToColorF())),
                    workingColorSpace, backColor, alphaThreshold, disposeCallback),

                // Rgba16161616/Unpremul
                { ColorType: SKColorType.Rgba16161616, AlphaType: SKAlphaType.Unpremul } => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                    (row, x) => row.UnsafeGetRefAs<ColorRgba16161616Linear>(x).ToColor32(),
                    (row, x, c) => row.UnsafeGetRefAs<ColorRgba16161616Linear>(x) = new ColorRgba16161616Linear(c),
                    workingColorSpace, backColor, alphaThreshold, disposeCallback),

                // Rgba16161616/Premul
                { ColorType: SKColorType.Rgba16161616, AlphaType: SKAlphaType.Premul } => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                    (row, x) => row.UnsafeGetRefAs<ColorPrgba16161616Linear>(x).ToColor32(),
                    (row, x, c) => row.UnsafeGetRefAs<ColorPrgba16161616Linear>(x) = new ColorPrgba16161616Linear(c),
                    workingColorSpace, backColor, alphaThreshold, disposeCallback),

                // Rgba16161616/Opaque
                { ColorType: SKColorType.Rgba16161616, AlphaType: SKAlphaType.Opaque } => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                    (row, x) => row.UnsafeGetRefAs<ColorRgba16161616Linear>(x).ToColor32().ToOpaque(),
                    (row, x, c) => row.UnsafeGetRefAs<ColorRgba16161616Linear>(x) = c.A == Byte.MaxValue ? new ColorRgba16161616Linear(c)
                        : row.BitmapData.WorkingColorSpace == WorkingColorSpace.Srgb ? new ColorRgba16161616Linear(c.ToColor64().Blend(row.BitmapData.BackColor.ToColor64()))
                        : new ColorRgba16161616Linear(c.ToColorF().Blend(row.BitmapData.BackColor.ToColorF())),
                    workingColorSpace, backColor, alphaThreshold, disposeCallback),

                // Bgra1010102/Unpremul
                { ColorType: SKColorType.Bgra1010102, AlphaType: SKAlphaType.Unpremul } => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                    (row, x) => row.UnsafeGetRefAs<ColorBgra1010102Linear>(x).ToColor32(),
                    (row, x, c) => row.UnsafeGetRefAs<ColorBgra1010102Linear>(x) = new ColorBgra1010102Linear(c),
                    workingColorSpace, backColor, alphaThreshold, disposeCallback),

                // Bgra1010102/Premul
                { ColorType: SKColorType.Bgra1010102, AlphaType: SKAlphaType.Premul } => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                    (row, x) => row.UnsafeGetRefAs<ColorPbgra1010102Linear>(x).ToColor32(),
                    (row, x, c) => row.UnsafeGetRefAs<ColorPbgra1010102Linear>(x) = new ColorPbgra1010102Linear(c),
                    workingColorSpace, backColor, alphaThreshold, disposeCallback),

                // Bgra1010102/Opaque, Bgr101010x
                { ColorType: SKColorType.Bgra1010102, AlphaType: SKAlphaType.Opaque } or { ColorType: SKColorType.Bgr101010x }
                    => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                        (row, x) => row.UnsafeGetRefAs<ColorBgra1010102Linear>(x).ToColor32().ToOpaque(),
                        (row, x, c) => row.UnsafeGetRefAs<ColorBgra1010102Linear>(x) = c.A == Byte.MaxValue ? new ColorBgra1010102Linear(c)
                            : row.BitmapData.WorkingColorSpace == WorkingColorSpace.Srgb ? new ColorBgra1010102Linear(c.ToColor64().Blend(row.BitmapData.BackColor.ToColor64()))
                            : new ColorBgra1010102Linear(c.ToColorF().Blend(row.BitmapData.BackColor.ToColorF())),
                        workingColorSpace, backColor, alphaThreshold, disposeCallback),

                // Rgba1010102/Unpremul
                { ColorType: SKColorType.Rgba1010102, AlphaType: SKAlphaType.Unpremul } => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                    (row, x) => row.UnsafeGetRefAs<ColorRgba1010102Linear>(x).ToColor32(),
                    (row, x, c) => row.UnsafeGetRefAs<ColorRgba1010102Linear>(x) = new ColorRgba1010102Linear(c),
                    workingColorSpace, backColor, alphaThreshold, disposeCallback),

                // Rgba1010102/Premul
                { ColorType: SKColorType.Rgba1010102, AlphaType: SKAlphaType.Premul } => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                    (row, x) => row.UnsafeGetRefAs<ColorPrgba1010102Linear>(x).ToColor32(),
                    (row, x, c) => row.UnsafeGetRefAs<ColorPrgba1010102Linear>(x) = new ColorPrgba1010102Linear(c),
                    workingColorSpace, backColor, alphaThreshold, disposeCallback),

                // Rgba1010102/Opaque, Rgb101010x
                { ColorType: SKColorType.Rgba1010102, AlphaType: SKAlphaType.Opaque } or { ColorType: SKColorType.Rgb101010x }
                    => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                        (row, x) => row.UnsafeGetRefAs<ColorRgba1010102Linear>(x).ToColor32().ToOpaque(),
                        (row, x, c) => row.UnsafeGetRefAs<ColorRgba1010102Linear>(x) = c.A == Byte.MaxValue ? new ColorRgba1010102Linear(c)
                            : row.BitmapData.WorkingColorSpace == WorkingColorSpace.Srgb ? new ColorRgba1010102Linear(c.ToColor64().Blend(row.BitmapData.BackColor.ToColor64()))
                            : new ColorRgba1010102Linear(c.ToColorF().Blend(row.BitmapData.BackColor.ToColorF())),
                        workingColorSpace, backColor, alphaThreshold, disposeCallback),

                // Argb4444/Unpremul
                { ColorType: SKColorType.Argb4444, AlphaType: SKAlphaType.Unpremul } => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                    (row, x) => row.UnsafeGetRefAs<ColorArgb4444Linear>(x).ToColor32(),
                    (row, x, c) => row.UnsafeGetRefAs<ColorArgb4444Linear>(x) = new ColorArgb4444Linear(c),
                    workingColorSpace, backColor, alphaThreshold, disposeCallback),

                // Argb4444/Premul
                { ColorType: SKColorType.Argb4444, AlphaType: SKAlphaType.Premul } => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                    (row, x) => row.UnsafeGetRefAs<ColorPargb4444Linear>(x).ToColor32(),
                    (row, x, c) => row.UnsafeGetRefAs<ColorPargb4444Linear>(x) = new ColorPargb4444Linear(c),
                    workingColorSpace, backColor, alphaThreshold, disposeCallback),

                // Argb4444/Opaque
                { ColorType: SKColorType.Argb4444, AlphaType: SKAlphaType.Opaque } => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                    (row, x) => row.UnsafeGetRefAs<ColorArgb4444Linear>(x).ToColor32().ToOpaque(),
                    (row, x, c) => row.UnsafeGetRefAs<ColorArgb4444Linear>(x) = row.BitmapData.WorkingColorSpace == WorkingColorSpace.Srgb
                        ? new ColorArgb4444Linear(c.A == Byte.MaxValue ? c : c.Blend(row.BitmapData.BackColor))
                        : new ColorArgb4444Linear(c.A == Byte.MaxValue ? c.ToColorF() : c.ToColorF().Blend(row.BitmapData.BackColor.ToColorF())),
                    workingColorSpace, backColor, alphaThreshold, disposeCallback),

                // RgbaF32/Unpremul
                { ColorType: SKColorType.RgbaF32, AlphaType: SKAlphaType.Unpremul } => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                    (row, x) => row.UnsafeGetRefAs<ColorF>(x).ToColor32(),
                    (row, x, c) => row.UnsafeGetRefAs<ColorF>(x) = new ColorF(c),
                    workingColorSpace, backColor, alphaThreshold, disposeCallback),

                // RgbaF32/Premul
                { ColorType: SKColorType.RgbaF32, AlphaType: SKAlphaType.Premul } => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                    (row, x) => row.UnsafeGetRefAs<PColorF>(x).ToColor32(),
                    (row, x, c) => row.UnsafeGetRefAs<PColorF>(x) = new PColorF(c),
                    workingColorSpace, backColor, alphaThreshold, disposeCallback),

                // RgbaF32/Opaque
                { ColorType: SKColorType.RgbaF32, AlphaType: SKAlphaType.Opaque } => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                    (row, x) => row.UnsafeGetRefAs<ColorF>(x).ToColor32().ToOpaque(),
                    (row, x, c) => row.UnsafeGetRefAs<ColorF>(x) = c.A == Byte.MaxValue ? c.ToColorF()
                        : row.BitmapData.WorkingColorSpace == WorkingColorSpace.Srgb ? c.ToColor64().Blend(row.BitmapData.BackColor.ToColor64()).ToColorF()
                        : c.ToColorF().Blend(row.BitmapData.BackColor.ToColorF()),
                    workingColorSpace, backColor, alphaThreshold, disposeCallback),

                // RgbaF16/Unpremul, RgbaF16Clamped/Unpremul
                { ColorType: SKColorType.RgbaF16 or SKColorType.RgbaF16Clamped, AlphaType: SKAlphaType.Unpremul }
                    => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                        (row, x) => row.UnsafeGetRefAs<ColorRgbaF16Linear>(x).ToColor32(),
                        (row, x, c) => row.UnsafeGetRefAs<ColorRgbaF16Linear>(x) = new ColorRgbaF16Linear(c),
                        workingColorSpace, backColor, alphaThreshold, disposeCallback),

                // RgbaF16/Premul, RgbaF16Clamped/Premul
                { ColorType: SKColorType.RgbaF16 or SKColorType.RgbaF16Clamped, AlphaType: SKAlphaType.Premul }
                    => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                        (row, x) => row.UnsafeGetRefAs<ColorPrgbaF16Linear>(x).ToColor32(),
                        (row, x, c) => row.UnsafeGetRefAs<ColorPrgbaF16Linear>(x) = new ColorPrgbaF16Linear(c),
                        workingColorSpace, backColor, alphaThreshold, disposeCallback),

                // RgbaF16/Opaque, RgbaF16Clamped/Opaque
                { ColorType: SKColorType.RgbaF16 or SKColorType.RgbaF16Clamped, AlphaType: SKAlphaType.Opaque }
                    => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                        (row, x) => row.UnsafeGetRefAs<ColorRgbaF16Linear>(x).ToColor32().ToOpaque(),
                        (row, x, c) => row.UnsafeGetRefAs<ColorRgbaF16Linear>(x) = c.A == Byte.MaxValue ? new ColorRgbaF16Linear(c)
                            : row.BitmapData.WorkingColorSpace == WorkingColorSpace.Srgb ? new ColorRgbaF16Linear(c.ToColor64().Blend(row.BitmapData.BackColor.ToColor64()))
                            : new ColorRgbaF16Linear(c.ToColorF().Blend(row.BitmapData.BackColor.ToColorF())),
                        workingColorSpace, backColor, alphaThreshold, disposeCallback),

                // Alpha8
                { ColorType: SKColorType.Alpha8 } => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                    (row, x) => row.UnsafeGetRefAs<ColorAlpha8>(x).ToColor32(),
                    (row, x, c) => row.UnsafeGetRefAs<ColorAlpha8>(x) = new ColorAlpha8(c),
                    workingColorSpace, backColor, alphaThreshold, disposeCallback),

                // Alpha16
                { ColorType: SKColorType.Alpha16 } => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                    (row, x) => row.UnsafeGetRefAs<ColorAlpha16>(x).ToColor32(),
                    (row, x, c) => row.UnsafeGetRefAs<ColorAlpha16>(x) = new ColorAlpha16(c),
                    workingColorSpace, backColor, alphaThreshold, disposeCallback),

                // AlphaF16
                { ColorType: SKColorType.AlphaF16 } => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                    (row, x) => row.UnsafeGetRefAs<ColorAlphaF16>(x).ToColor32(),
                    (row, x, c) => row.UnsafeGetRefAs<ColorAlphaF16>(x) = new ColorAlphaF16(c),
                    workingColorSpace, backColor, alphaThreshold, disposeCallback),

                // Rg88
                { ColorType: SKColorType.Rg88 } => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                    (row, x) => row.UnsafeGetRefAs<ColorRg88Linear>(x).ToColor32(),
                    (row, x, c) => row.UnsafeGetRefAs<ColorRg88Linear>(x) = c.A == Byte.MaxValue ? new ColorRg88Linear(c)
                        : row.BitmapData.WorkingColorSpace == WorkingColorSpace.Srgb ? new ColorRg88Linear(c.ToColor64().Blend(row.BitmapData.BackColor.ToColor64()))
                        : new ColorRg88Linear(c.ToColorF().Blend(row.BitmapData.BackColor.ToColorF())),
                    workingColorSpace, backColor, alphaThreshold, disposeCallback),

                // Rg1616
                { ColorType: SKColorType.Rg1616 } => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                    (row, x) => row.UnsafeGetRefAs<ColorRg1616Linear>(x).ToColor32(),
                    (row, x, c) => row.UnsafeGetRefAs<ColorRg1616Linear>(x) = c.A == Byte.MaxValue ? new ColorRg1616Linear(c)
                        : row.BitmapData.WorkingColorSpace == WorkingColorSpace.Srgb ? new ColorRg1616Linear(c.ToColor64().Blend(row.BitmapData.BackColor.ToColor64()))
                        : new ColorRg1616Linear(c.ToColorF().Blend(row.BitmapData.BackColor.ToColorF())),
                    workingColorSpace, backColor, alphaThreshold, disposeCallback),

                // Rg16F
                { ColorType: SKColorType.RgF16 } => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                    (row, x) => row.UnsafeGetRefAs<ColorRgF16Linear>(x).ToColor32(),
                    (row, x, c) => row.UnsafeGetRefAs<ColorRgF16Linear>(x) = c.A == Byte.MaxValue ? new ColorRgF16Linear(c)
                        : row.BitmapData.WorkingColorSpace == WorkingColorSpace.Srgb ? new ColorRgF16Linear(c.ToColor64().Blend(row.BitmapData.BackColor.ToColor64()))
                        : new ColorRgF16Linear(c.ToColorF().Blend(row.BitmapData.BackColor.ToColorF())),
                    workingColorSpace, backColor, alphaThreshold, disposeCallback),

                _ => throw new InvalidOperationException(Res.InternalError($"{info.ColorType}/{info.AlphaType} is not supported directly in the linear color space. {nameof(SKBitmapExtensions.GetFallbackBitmapData)} should have been called from the caller."))
            };
        }

        #endregion

        #endregion
    }
}