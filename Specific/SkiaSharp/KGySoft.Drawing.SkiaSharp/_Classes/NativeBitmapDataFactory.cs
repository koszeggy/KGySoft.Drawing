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
            Color32 backColor32 = backColor.ToColor32();

            // natively supported formats
            if (knownPixelFormat != KnownPixelFormat.Undefined)
                return BitmapDataFactory.CreateBitmapData(buffer, size, stride, knownPixelFormat, backColor32, alphaThreshold, disposeCallback);

            // supported custom formats
            PixelFormatInfo pixelFormatInfo = info.GetInfo();
            switch (info)
            {
                //case { ColorType: SKColorType.Bgra8888, AlphaType: SKAlphaType.Opaque }:
                //    return BitmapDataFactory.CreateBitmapData(buffer, size, stride, pixelFormatInfo,
                //        (row, x) => row.UnsafeGetRefAs<Color32>(x),
                //        (row, x, c) => row.UnsafeGetRefAs<Color32>(x) = c.ToPremultiplied(),
                //        backColor32, alphaThreshold, disposeCallback);
            }

            throw new InvalidOperationException(Res.InternalError($"{info.ColorType}/{info.AlphaType} is not supported directly. {nameof(SKBitmapExtensions.GetFallbackBitmapData)} should have been called from the caller."));
        }

        #endregion
    }
}