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
using System.Runtime.InteropServices;

using KGySoft.CoreLibraries;
using KGySoft.Drawing.Imaging;

using SkiaSharp;

#endregion

namespace KGySoft.Drawing.Examples.Xamarin.Extensions
{
    internal static class SKBitmapExtensions
    {
        #region Nested Structs

        [StructLayout(LayoutKind.Explicit)]
        private struct ColorRgba8888
        {
            #region Fields

            [FieldOffset(0)]private readonly byte r;
            [FieldOffset(1)]private readonly byte g;
            [FieldOffset(2)]private readonly byte b;
            [FieldOffset(3)]private readonly byte a;

            #endregion

            #region Constructors

            internal ColorRgba8888(Color32 c)
            {
                r = c.R;
                g = c.G;
                b = c.B;
                a = c.A;
            }

            internal ColorRgba8888(byte r, byte g, byte b, byte a)
            {
                this.r = r;
                this.g = g;
                this.b = b;
                this.a = a;
            }

            #endregion

            #region Methods

            internal ColorRgba8888 ToStraight() => a switch
            {
                Byte.MaxValue => this,
                0 => default,
                _ => new ColorRgba8888((byte)(r * Byte.MaxValue / a),
                    (byte)(g * Byte.MaxValue / a),
                    (byte)(b * Byte.MaxValue / a),
                    a)
            };

            internal ColorRgba8888 ToPremultiplied() => a switch
            {
                Byte.MaxValue => this,
                0 => default,
                _ => new ColorRgba8888((byte)(r * a / Byte.MaxValue), 
                    (byte)(g * a / Byte.MaxValue),
                    (byte)(b * a / Byte.MaxValue),
                    a)
            };

            internal Color32 ToColor32() => new Color32(a, r, g, b);

            #endregion
        }

        #endregion

        #region Methods

        internal static IReadableBitmapData GetReadableBitmapData(this SKBitmap bitmap, WorkingColorSpace workingColorSpace = default)
            => GetReadWriteBitmapData(bitmap, workingColorSpace);

        internal static IReadWriteBitmapData GetReadWriteBitmapData(this SKBitmap bitmap, WorkingColorSpace workingColorSpace)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap));
            if (!workingColorSpace.IsDefined())
                throw new ArgumentOutOfRangeException(nameof(workingColorSpace));
            var info = bitmap.Info;
            if (info.ColorType is not (SKColorType.Bgra8888 or SKColorType.Rgba8888) || info.AlphaType != SKAlphaType.Premul)
                throw new ArgumentException($"Unexpected pixel format {info.ColorType}/{info.AlphaType}", nameof(bitmap));

            // Same as KnownPixelFormat.Format32bppPArgb
            if (info.ColorType == SKColorType.Bgra8888)
                return BitmapDataFactory.CreateBitmapData(bitmap.GetPixels(), new Size(info.Width, info.Height), info.RowBytes, KnownPixelFormat.Format32bppPArgb, workingColorSpace);

            // Rgba8888 is a custom format (eg. on Android)
            return BitmapDataFactory.CreateBitmapData(bitmap.GetPixels(), new Size(info.Width, info.Height), info.RowBytes,
                new PixelFormatInfo(32) { HasPremultipliedAlpha = true },
                (row, x) => row.UnsafeGetRefAs<ColorRgba8888>(x).ToStraight().ToColor32(),
                (row, x, c) => row.UnsafeGetRefAs<ColorRgba8888>(x) = new ColorRgba8888(c).ToPremultiplied(), workingColorSpace);
        }

        #endregion
    }
}