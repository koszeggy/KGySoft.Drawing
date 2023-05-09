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
using System.Runtime.InteropServices;

using KGySoft.CoreLibraries;
using KGySoft.Drawing.Imaging;

using SkiaSharp;

#endregion

namespace KGySoft.Drawing.Examples.Xamarin.Extensions
{
    /// <summary>
    /// This class demonstrates how expose a 3rd party bitmap implementation as an <see cref="IReadWriteBitmapData"/> using purely the KGySoft.Drawing.Core package.
    /// In fact, SKBitmap has a dedicated support provided by the https://www.nuget.org/packages/KGySoft.Drawing.SkiaSharp package.
    /// To see an example that supports all possible SKBitmap pixel formats see the example at https://github.com/koszeggy/KGySoft.Drawing/tree/master/Examples/SkiaSharp_(Maui)
    /// </summary>
    internal static class SKBitmapExtensions
    {
        #region Nested Structs

        /// <summary>
        /// Represents the same color as <see cref="PColor32"/> with RGBA order.
        /// </summary>
        [StructLayout(LayoutKind.Explicit)]
        private struct ColorPrgba8888
        {
            #region Fields

            [FieldOffset(0)]private readonly byte r;
            [FieldOffset(1)]private readonly byte g;
            [FieldOffset(2)]private readonly byte b;
            [FieldOffset(3)]private readonly byte a;

            #endregion

            #region Constructors

            internal ColorPrgba8888(PColor32 c)
            {
                r = c.R;
                g = c.G;
                b = c.B;
                a = c.A;
            }

            #endregion

            #region Methods

            internal Color32 ToColor32() => new PColor32(a, r, g, b).ToColor32();

            #endregion
        }

        #endregion

        #region Methods

        internal static IReadableBitmapData GetReadableBitmapData(this SKBitmap bitmap, WorkingColorSpace workingColorSpace = default)
            => GetReadWriteBitmapData(bitmap, workingColorSpace);

        /// <summary>
        /// This example supports Bgra8888 and Rgba8888 color types with Premul alpha type only.
        /// See also the example at https://github.com/koszeggy/KGySoft.Drawing/tree/master/Examples/SkiaSharp_(Maui)
        /// </summary>
        internal static IReadWriteBitmapData GetReadWriteBitmapData(this SKBitmap bitmap, WorkingColorSpace workingColorSpace)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap));
            if (!workingColorSpace.IsDefined())
                throw new ArgumentOutOfRangeException(nameof(workingColorSpace));

            var info = bitmap.Info;
            if (info.ColorType is not (SKColorType.Bgra8888 or SKColorType.Rgba8888) || info.AlphaType != SKAlphaType.Premul)
                throw new ArgumentException($"Unexpected pixel format {info.ColorType}/{info.AlphaType}", nameof(bitmap));

            // Known format example: SKColorType.Bgra8888 with SKAlphaType.Premul is the same as KnownPixelFormat.Format32bppPArgb
            if (info.ColorType == SKColorType.Bgra8888)
                return BitmapDataFactory.CreateBitmapData(bitmap.GetPixels(), new Size(info.Width, info.Height), info.RowBytes,
                    KnownPixelFormat.Format32bppPArgb, workingColorSpace);

            // Custom format example: Rgba8888 (used eg. on Android) is not a known format but we can simply specify a couple of delegates to tell how to use it
            return BitmapDataFactory.CreateBitmapData(bitmap.GetPixels(), new Size(info.Width, info.Height), info.RowBytes,
                new PixelFormatInfo(32) { HasPremultipliedAlpha = true },
                (row, x) => row.UnsafeGetRefAs<ColorPrgba8888>(x).ToColor32(),
                (row, x, c) => row.UnsafeGetRefAs<ColorPrgba8888>(x) = new ColorPrgba8888(c.ToPColor32()), workingColorSpace);
        }

        #endregion
    }
}