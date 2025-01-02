﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: SKImageExtensions.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2025 - All Rights Reserved
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
    /// Contains extension methods for the <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skimage">SKImage</a> type.
    /// </summary>
    public static class SKImageExtensions
    {
        #region Methods

        #region Public Methods

        /// <summary>
        /// Gets a managed read-only accessor for an <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skimage">SKImage</a> instance.
        /// <br/>See the <strong>Remarks</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_GetReadWriteBitmapData.htm">BitmapExtensions.GetReadWriteBitmapData</a>
        /// method for details and code samples. That method is for the GDI+ <a href="https://docs.microsoft.com/en-us/dotnet/api/system.drawing.bitmap" target="_blank">Bitmap</a> type but the main principles apply for this method, too.
        /// </summary>
        /// <param name="image">An <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skimage">SKImage</a> instance, whose data is about to be accessed.</param>
        /// <param name="backColor">Determines the <see cref="IBitmapData.BackColor"/> property of the result. As SkiaSharp does not support indexed formats
        /// with palette anymore the <paramref name="backColor"/> for the read-only result bitmap data is relevant in very rare cases only, such as cloning by
        /// the <see cref="BitmapDataExtensions.Clone(IReadableBitmapData, KnownPixelFormat, IDitherer?)"/> method or obtaining a quantizer by
        /// the <see cref="PredefinedColorsQuantizer.FromBitmapData"/> method.
        /// The <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcolor.alpha">Alpha</a> property of the specified background color is ignored. This parameter is optional.
        /// <br/>Default value: The bitwise zero instance of <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcolor">SKColor</a>, which has the same RGB values as <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcolors.black">Black</a>.</param>
        /// <param name="alphaThreshold">Determines the <see cref="IBitmapData.AlphaThreshold"/> property of the result.
        /// Similarly to <paramref name="backColor"/>, for an <see cref="IReadableBitmapData"/> instance the <paramref name="alphaThreshold"/> is relevant
        /// in very rare cases such as cloning the result or obtaining a matching quantizer from it. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <returns>An <see cref="IReadableBitmapData"/> instance, which provides fast read-only access to the actual data of the specified <paramref name="image"/>.</returns>
        /// <seealso cref="SKPixmapExtensions.GetReadableBitmapData(SKPixmap, SKColor, byte)"/>
        /// <seealso cref="SKBitmapExtensions.GetReadableBitmapData(SKBitmap, SKColor, byte)"/>
        /// <seealso cref="SKSurfaceExtensions.GetReadableBitmapData(SKSurface, SKColor, byte)"/>
        /// <seealso cref="BitmapDataFactory.CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/>
        public static IReadableBitmapData GetReadableBitmapData(this SKImage image, SKColor backColor = default, byte alphaThreshold = 128)
            => image.GetBitmapDataInternal(WorkingColorSpace.Default, backColor.ToColor32(), alphaThreshold);

        /// <summary>
        /// Gets a managed read-only accessor for an <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skimage">SKImage</a> instance.
        /// <br/>See the <strong>Remarks</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_GetReadWriteBitmapData.htm">BitmapExtensions.GetReadWriteBitmapData</a>
        /// method for details and code samples. That method is for the GDI+ <a href="https://docs.microsoft.com/en-us/dotnet/api/system.drawing.bitmap" target="_blank">Bitmap</a> type but the main principles apply for this method, too.
        /// </summary>
        /// <param name="image">An <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skimage">SKImage</a> instance, whose data is about to be accessed.</param>
        /// <param name="workingColorSpace">Determines the <see cref="IBitmapData.WorkingColorSpace"/> property of the result and
        /// specifies the preferred color space that should be used when working with the result bitmap data. The working color space
        /// can be different from the actual color space of the specified <paramref name="image"/>.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="WorkingColorSpace"/> enumeration for more details.</param>
        /// <param name="backColor">Determines the <see cref="IBitmapData.BackColor"/> property of the result. As SkiaSharp does not support indexed formats
        /// with palette anymore the <paramref name="backColor"/> for the read-only result bitmap data is relevant in very rare cases only, such as cloning by
        /// the <see cref="BitmapDataExtensions.Clone(IReadableBitmapData, KnownPixelFormat, IDitherer?)"/> method or obtaining a quantizer by
        /// the <see cref="PredefinedColorsQuantizer.FromBitmapData"/> method.
        /// The <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcolor.alpha">Alpha</a> property of the specified background color is ignored. This parameter is optional.
        /// <br/>Default value: The bitwise zero instance of <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcolor">SKColor</a>, which has the same RGB values as <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcolors.black">Black</a>.</param>
        /// <param name="alphaThreshold">Determines the <see cref="IBitmapData.AlphaThreshold"/> property of the result.
        /// Similarly to <paramref name="backColor"/>, for an <see cref="IReadableBitmapData"/> instance the <paramref name="alphaThreshold"/> is relevant
        /// in very rare cases such as cloning the result or obtaining a matching quantizer from it. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <returns>An <see cref="IReadableBitmapData"/> instance, which provides fast read-only access to the actual data of the specified <paramref name="image"/>.</returns>
        /// <seealso cref="SKPixmapExtensions.GetReadableBitmapData(SKPixmap, WorkingColorSpace, SKColor, byte)"/>
        /// <seealso cref="SKBitmapExtensions.GetReadableBitmapData(SKBitmap, WorkingColorSpace, SKColor, byte)"/>
        /// <seealso cref="SKSurfaceExtensions.GetReadableBitmapData(SKSurface, WorkingColorSpace, SKColor, byte)"/>
        /// <seealso cref="BitmapDataFactory.CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/>
        public static IReadableBitmapData GetReadableBitmapData(this SKImage image, WorkingColorSpace workingColorSpace, SKColor backColor = default, byte alphaThreshold = 128)
            => image.GetBitmapDataInternal(workingColorSpace, backColor.ToColor32(), alphaThreshold);

        #endregion

        #region Private Methods

        private static IReadableBitmapData GetBitmapDataInternal(this SKImage image, WorkingColorSpace workingColorSpace, Color32 backColor, byte alphaThreshold, Action? disposeCallback = null)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image), PublicResources.ArgumentNull);
            if (workingColorSpace is < WorkingColorSpace.Default or > WorkingColorSpace.Srgb)
                throw new ArgumentOutOfRangeException(nameof(workingColorSpace), PublicResources.EnumOutOfRange(workingColorSpace));

            SKPixmap? pixels = image.PeekPixels();

            // Raster-based image: We can simply get a bitmap data for its pixels
            if (pixels != null)
                return pixels.GetBitmapDataInternal(true, workingColorSpace, backColor, alphaThreshold, disposeCallback: disposeCallback);

            // Other image: converting it to a bitmap
            SKImageInfo imageInfo = image.Info;
            var bitmap = new SKBitmap(imageInfo);
            if (!image.ReadPixels(imageInfo, bitmap.GetPixels()))
            {
                bitmap.Dispose();
                disposeCallback?.Invoke();
                throw new ArgumentException(PublicResources.ArgumentInvalid, nameof(image));
            }

            Action disposeBitmap = disposeCallback == null
                ? bitmap.Dispose
                : () =>
                {
                    bitmap.Dispose();
                    disposeCallback();
                };

            return bitmap.GetBitmapDataInternal(true, workingColorSpace, backColor, alphaThreshold, disposeCallback: disposeBitmap);
        }

        #endregion

        #endregion
    }
}
