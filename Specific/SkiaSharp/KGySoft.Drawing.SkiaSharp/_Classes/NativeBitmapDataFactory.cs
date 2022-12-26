#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: NativeBitmapDataFactory.cs
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
    internal static class NativeBitmapDataFactory
    {
        #region Methods

        internal static IReadWriteBitmapData CreateBitmapData(IntPtr buffer, SKImageInfo info, int stride, SKColor backColor, byte alphaThreshold, Action? disposeCallback = null)
        {
            Debug.Assert(info.IsDirectlySupported());

            var size = new Size(info.Width, info.Height);
            KnownPixelFormat knownPixelFormat = info.AsKnownPixelFormat();
            Color32 backColor32 = backColor.ToColor32().ToOpaque();

            // natively supported formats
            if (knownPixelFormat != KnownPixelFormat.Undefined)
                return BitmapDataFactory.CreateBitmapData(buffer, size, stride, knownPixelFormat, backColor32, alphaThreshold, disposeCallback);

            // supported custom formats
            PixelFormatInfo pixelFormatInfo = info.GetInfo();
            return info switch
            {
                // Rgba8888/Unpremul
                { ColorType: SKColorType.Rgba8888, AlphaType: SKAlphaType.Unpremul } => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                    (row, x) => row.UnsafeGetRefAs<ColorRgba8888>(x).ToColor32(),
                    (row, x, c) => row.UnsafeGetRefAs<ColorRgba8888>(x) = new ColorRgba8888(c),
                    backColor32, alphaThreshold, disposeCallback),

                // Rgba8888/Premul
                { ColorType: SKColorType.Rgba8888, AlphaType: SKAlphaType.Premul } => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                    (row, x) => row.UnsafeGetRefAs<ColorRgba8888>(x).ToStraight().ToColor32(),
                    (row, x, c) => row.UnsafeGetRefAs<ColorRgba8888>(x) = new ColorRgba8888(c).ToPremultiplied(),
                    backColor32, alphaThreshold, disposeCallback),

                // Rgba8888/Opaque, Rgb888x
                { ColorType: SKColorType.Rgba8888, AlphaType: SKAlphaType.Opaque } or { ColorType: SKColorType.Rgb888x }
                    => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                        (row, x) => row.UnsafeGetRefAs<ColorRgba8888>(x).ToColor32().ToOpaque(),
                        (row, x, c) => row.UnsafeGetRefAs<ColorRgba8888>(x) = new ColorRgba8888(c.Blend(backColor32)),
                        backColor32, alphaThreshold, disposeCallback),

                // Gray8
                { ColorType: SKColorType.Gray8 } => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                    (row, x) => Color32.FromGray(row.UnsafeGetRefAs<byte>(x)),
                    (row, x, c) => row.UnsafeGetRefAs<byte>(x) = c.Blend(backColor32).GetBrightness(),
                    backColor32, alphaThreshold, disposeCallback),

                // Rgba16161616/Unpremul
                { ColorType: SKColorType.Rgba16161616, AlphaType: SKAlphaType.Unpremul } => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                    (row, x) => row.UnsafeGetRefAs<ColorRgba16161616>(x).ToColor32(),
                    (row, x, c) => row.UnsafeGetRefAs<ColorRgba16161616>(x) = new ColorRgba16161616(c),
                    backColor32, alphaThreshold, disposeCallback),

                // Rgba16161616/Premul
                { ColorType: SKColorType.Rgba16161616, AlphaType: SKAlphaType.Premul } => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                    (row, x) => row.UnsafeGetRefAs<ColorRgba16161616>(x).ToStraight().ToColor32(),
                    (row, x, c) => row.UnsafeGetRefAs<ColorRgba16161616>(x) = new ColorRgba16161616(c).ToPremultiplied(),
                    backColor32, alphaThreshold, disposeCallback),

                // Rgba16161616/Opaque
                { ColorType: SKColorType.Rgba16161616, AlphaType: SKAlphaType.Opaque } => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                    (row, x) => row.UnsafeGetRefAs<ColorRgba16161616>(x).ToColor32().ToOpaque(),
                    (row, x, c) => row.UnsafeGetRefAs<ColorRgba16161616>(x) = new ColorRgba16161616(c.Blend(backColor32)),
                    backColor32, alphaThreshold, disposeCallback),

                // Bgra1010102/Unpremul
                { ColorType: SKColorType.Bgra1010102, AlphaType: SKAlphaType.Unpremul } => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                    (row, x) => row.UnsafeGetRefAs<ColorBgra1010102>(x).ToColor32(),
                    (row, x, c) => row.UnsafeGetRefAs<ColorBgra1010102>(x) = new ColorBgra1010102(c, backColor32),
                    backColor32, alphaThreshold, disposeCallback),

                // Bgra1010102/Premul
                { ColorType: SKColorType.Bgra1010102, AlphaType: SKAlphaType.Premul } => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                    (row, x) => row.UnsafeGetRefAs<ColorBgra1010102>(x).ToStraight().ToColor32(),
                    (row, x, c) => row.UnsafeGetRefAs<ColorBgra1010102>(x) = new ColorBgra1010102(c, backColor32).ToPremultiplied(),
                    backColor32, alphaThreshold, disposeCallback),

                // Bgra1010102/Opaque, Bgr101010x
                { ColorType: SKColorType.Bgra1010102, AlphaType: SKAlphaType.Opaque } or { ColorType: SKColorType.Bgr101010x }
                    => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                        (row, x) => row.UnsafeGetRefAs<ColorBgra1010102>(x).ToColor32().ToOpaque(),
                        (row, x, c) => row.UnsafeGetRefAs<ColorBgra1010102>(x) = new ColorBgra1010102(c.Blend(backColor32)),
                        backColor32, alphaThreshold, disposeCallback),

                // Rgba1010102/Unpremul
                { ColorType: SKColorType.Rgba1010102, AlphaType: SKAlphaType.Unpremul } => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                    (row, x) => row.UnsafeGetRefAs<ColorRgba1010102>(x).ToColor32(),
                    (row, x, c) => row.UnsafeGetRefAs<ColorRgba1010102>(x) = new ColorRgba1010102(c, backColor32),
                    backColor32, alphaThreshold, disposeCallback),

                // Rgba1010102/Premul
                { ColorType: SKColorType.Rgba1010102, AlphaType: SKAlphaType.Premul } => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                    (row, x) => row.UnsafeGetRefAs<ColorRgba1010102>(x).ToStraight().ToColor32(),
                    (row, x, c) => row.UnsafeGetRefAs<ColorRgba1010102>(x) = new ColorRgba1010102(c, backColor32).ToPremultiplied(),
                    backColor32, alphaThreshold, disposeCallback),

                // Rgba1010102/Opaque, Rgb101010x
                { ColorType: SKColorType.Rgba1010102, AlphaType: SKAlphaType.Opaque } or { ColorType: SKColorType.Rgb101010x }
                    => BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                        (row, x) => row.UnsafeGetRefAs<ColorRgba1010102>(x).ToColor32().ToOpaque(),
                        (row, x, c) => row.UnsafeGetRefAs<ColorRgba1010102>(x) = new ColorRgba1010102(c.Blend(backColor32)),
                        backColor32, alphaThreshold, disposeCallback),

                _ => throw new InvalidOperationException(Res.InternalError($"{info.ColorType}/{info.AlphaType} is not supported directly. {nameof(SKBitmapExtensions.GetFallbackBitmapData)} should have been called from the caller."))
            };
        }

        #endregion
    }
}