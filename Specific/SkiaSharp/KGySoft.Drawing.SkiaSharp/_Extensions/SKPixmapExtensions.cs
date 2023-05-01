#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: SKPixmapExtensions.cs
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

using KGySoft.Drawing.Imaging;

using SkiaSharp;

#endregion

namespace KGySoft.Drawing.SkiaSharp
{
    /// <summary>
    /// Contains extension methods for the <see cref="SKPixmap"/> type.
    /// </summary>
    public static class SKPixmapExtensions
    {
        #region Methods

        #region Public Methods

        /// <summary>
        /// Gets a managed read-only accessor for an <see cref="SKPixmap"/> instance.
        /// <br/>See the <strong>Remarks</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_GetReadWriteBitmapData.htm">BitmapExtensions.GetReadWriteBitmapData</a>
        /// method for details and code samples. That method is for the GDI+ <a href="https://docs.microsoft.com/en-us/dotnet/api/system.drawing.bitmap" target="_blank">Bitmap</a> type but the main principles apply for this method, too.
        /// </summary>
        /// <param name="pixels">An <see cref="SKPixmap"/> instance, whose data is about to be accessed.</param>
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
        /// <returns>An <see cref="IReadableBitmapData"/> instance, which provides fast read-only access to the actual data of the specified <paramref name="pixels"/>.</returns>
        /// <seealso cref="SKBitmapExtensions.GetReadableBitmapData(SKBitmap, SKColor, byte)"/>
        /// <seealso cref="SKImageExtensions.GetReadableBitmapData(SKImage, SKColor, byte)"/>
        /// <seealso cref="SKSurfaceExtensions.GetReadableBitmapData(SKSurface, SKColor, byte)"/>
        /// <seealso cref="BitmapDataFactory.CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/>
        public static IReadableBitmapData GetReadableBitmapData(this SKPixmap pixels, SKColor backColor = default, byte alphaThreshold = 128)
            => pixels.GetBitmapDataInternal(true, WorkingColorSpace.Default, backColor.ToColor32(), alphaThreshold);

        /// <summary>
        /// Gets a managed read-only accessor for an <see cref="SKPixmap"/> instance.
        /// <br/>See the <strong>Remarks</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_GetReadWriteBitmapData.htm">BitmapExtensions.GetReadWriteBitmapData</a>
        /// method for details and code samples. That method is for the GDI+ <a href="https://docs.microsoft.com/en-us/dotnet/api/system.drawing.bitmap" target="_blank">Bitmap</a> type but the main principles apply for this method, too.
        /// </summary>
        /// <param name="pixels">An <see cref="SKPixmap"/> instance, whose data is about to be accessed.</param>
        /// <param name="workingColorSpace">Determines the <see cref="IBitmapData.WorkingColorSpace"/> property of the result and
        /// specifies the preferred color space that should be used when working with the result bitmap data. The working color space
        /// can be different from the actual color space of the specified <paramref name="pixels"/>.
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
        /// <returns>An <see cref="IReadableBitmapData"/> instance, which provides fast read-only access to the actual data of the specified <paramref name="pixels"/>.</returns>
        /// <seealso cref="SKBitmapExtensions.GetReadableBitmapData(SKBitmap, WorkingColorSpace, SKColor, byte)"/>
        /// <seealso cref="SKImageExtensions.GetReadableBitmapData(SKImage, WorkingColorSpace, SKColor, byte)"/>
        /// <seealso cref="SKSurfaceExtensions.GetReadableBitmapData(SKSurface, WorkingColorSpace, SKColor, byte)"/>
        /// <seealso cref="BitmapDataFactory.CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/>
        public static IReadableBitmapData GetReadableBitmapData(this SKPixmap pixels, WorkingColorSpace workingColorSpace, SKColor backColor = default, byte alphaThreshold = 128)
            => pixels.GetBitmapDataInternal(true, workingColorSpace, backColor.ToColor32(), alphaThreshold);

        /// <summary>
        /// Gets a managed write-only accessor for an <see cref="SKPixmap"/> instance.
        /// <br/>See the <strong>Remarks</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_GetReadWriteBitmapData.htm">BitmapExtensions.GetReadWriteBitmapData</a>
        /// method for details and code samples. That method is for the GDI+ <a href="https://docs.microsoft.com/en-us/dotnet/api/system.drawing.bitmap" target="_blank">Bitmap</a> type but the main principles apply for this method, too.
        /// </summary>
        /// <param name="pixels">An <see cref="SKPixmap"/> instance, whose data is about to be accessed.</param>
        /// <param name="backColor">Determines the <see cref="IBitmapData.BackColor"/> property of the result.
        /// When setting pixels of bitmaps without alpha support, specifies the color of the background.
        /// Color values with alpha will be blended with this color before setting the pixel in the result bitmap data.
        /// The <see cref="SKColor.Alpha"/> property of the specified background color is ignored. This parameter is optional.
        /// <br/>Default value: The bitwise zero instance of <see cref="SKColor"/>, which has the same RGB values as <see cref="SKColors.Black"/>.</param>
        /// <param name="alphaThreshold">Determines the <see cref="IBitmapData.AlphaThreshold"/> property of the result.
        /// As SkiaSharp does not support indexed pixel formats with palette anymore, this parameter is relevant in very rare cases only, such as
        /// obtaining a quantizer by the <see cref="PredefinedColorsQuantizer.FromBitmapData"/> method. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <returns>An <see cref="IWritableBitmapData"/> instance, which provides fast write-only access to the actual data of the specified <paramref name="pixels"/>.</returns>
        /// <seealso cref="SKBitmapExtensions.GetWritableBitmapData(SKBitmap, SKColor, byte)"/>
        /// <seealso cref="SKSurfaceExtensions.GetWritableBitmapData(SKSurface, SKColor, byte)"/>
        /// <seealso cref="BitmapDataFactory.CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/>
        public static IWritableBitmapData GetWritableBitmapData(this SKPixmap pixels, SKColor backColor = default, byte alphaThreshold = 128)
            => pixels.GetBitmapDataInternal(false, WorkingColorSpace.Default, backColor.ToColor32(), alphaThreshold);

        /// <summary>
        /// Gets a managed write-only accessor for an <see cref="SKPixmap"/> instance.
        /// <br/>See the <strong>Remarks</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_GetReadWriteBitmapData.htm">BitmapExtensions.GetReadWriteBitmapData</a>
        /// method for details and code samples. That method is for the GDI+ <a href="https://docs.microsoft.com/en-us/dotnet/api/system.drawing.bitmap" target="_blank">Bitmap</a> type but the main principles apply for this method, too.
        /// </summary>
        /// <param name="pixels">An <see cref="SKPixmap"/> instance, whose data is about to be accessed.</param>
        /// <param name="workingColorSpace">Determines the <see cref="IBitmapData.WorkingColorSpace"/> property of the result and
        /// specifies the preferred color space that should be used when working with the result bitmap data. The working color space
        /// can be different from the actual color space of the specified <paramref name="pixels"/>.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="WorkingColorSpace"/> enumeration for more details.</param>
        /// <param name="backColor">Determines the <see cref="IBitmapData.BackColor"/> property of the result.
        /// When setting pixels of bitmaps without alpha support, specifies the color of the background.
        /// Color values with alpha will be blended with this color before setting the pixel in the result bitmap data.
        /// The <see cref="SKColor.Alpha"/> property of the specified background color is ignored. This parameter is optional.
        /// <br/>Default value: The bitwise zero instance of <see cref="SKColor"/>, which has the same RGB values as <see cref="SKColors.Black"/>.</param>
        /// <param name="alphaThreshold">Determines the <see cref="IBitmapData.AlphaThreshold"/> property of the result.
        /// As SkiaSharp does not support indexed pixel formats with palette anymore, this parameter is relevant in very rare cases only, such as
        /// obtaining a quantizer by the <see cref="PredefinedColorsQuantizer.FromBitmapData"/> method. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <returns>An <see cref="IWritableBitmapData"/> instance, which provides fast write-only access to the actual data of the specified <paramref name="pixels"/>.</returns>
        /// <seealso cref="SKBitmapExtensions.GetWritableBitmapData(SKBitmap, WorkingColorSpace, SKColor, byte)"/>
        /// <seealso cref="SKSurfaceExtensions.GetWritableBitmapData(SKSurface, WorkingColorSpace, SKColor, byte)"/>
        /// <seealso cref="BitmapDataFactory.CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/>
        public static IWritableBitmapData GetWritableBitmapData(this SKPixmap pixels, WorkingColorSpace workingColorSpace, SKColor backColor = default, byte alphaThreshold = 128)
            => pixels.GetBitmapDataInternal(false, workingColorSpace, backColor.ToColor32(), alphaThreshold);

        /// <summary>
        /// Gets a managed read-write accessor for an <see cref="SKPixmap"/> instance.
        /// <br/>See the <strong>Remarks</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_GetReadWriteBitmapData.htm">BitmapExtensions.GetReadWriteBitmapData</a>
        /// method for details and code samples. That method is for the GDI+ <a href="https://docs.microsoft.com/en-us/dotnet/api/system.drawing.bitmap" target="_blank">Bitmap</a> type but the main principles apply for this method, too.
        /// </summary>
        /// <param name="pixels">An <see cref="SKPixmap"/> instance, whose data is about to be accessed.</param>
        /// <param name="backColor">Determines the <see cref="IBitmapData.BackColor"/> property of the result.
        /// When setting pixels of bitmaps without alpha support, specifies the color of the background.
        /// Color values with alpha, which are considered opaque will be blended with this color before setting the pixel in the result bitmap data.
        /// The <see cref="SKColor.Alpha"/> property of the specified background color is ignored. This parameter is optional.
        /// <br/>Default value: The bitwise zero instance of <see cref="SKColor"/>, which has the same RGB values as <see cref="SKColors.Black"/>.</param>
        /// <param name="alphaThreshold">Determines the <see cref="IBitmapData.AlphaThreshold"/> property of the result.
        /// Can be relevant in some operations such as when drawing another <see cref="IReadableBitmapData"/> instance with alpha into the returned bitmap data
        /// by the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.DrawInto">DrawInto</see> extension methods and the specified <paramref name="pixels"/>
        /// has no alpha support. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance, which provides fast read-write access to the actual data of the specified <paramref name="pixels"/>.</returns>
        /// <seealso cref="SKBitmapExtensions.GetReadWriteBitmapData(SKBitmap, SKColor, byte)"/>
        /// <seealso cref="SKSurfaceExtensions.GetReadWriteBitmapData(SKSurface, SKColor, byte)"/>
        /// <seealso cref="BitmapDataFactory.CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/>
        public static IReadWriteBitmapData GetReadWriteBitmapData(this SKPixmap pixels, SKColor backColor = default, byte alphaThreshold = 128)
            => pixels.GetBitmapDataInternal(false, WorkingColorSpace.Default, backColor.ToColor32(), alphaThreshold);

        /// <summary>
        /// Gets a managed read-write accessor for an <see cref="SKPixmap"/> instance.
        /// <br/>See the <strong>Remarks</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_GetReadWriteBitmapData.htm">BitmapExtensions.GetReadWriteBitmapData</a>
        /// method for details and code samples. That method is for the GDI+ <a href="https://docs.microsoft.com/en-us/dotnet/api/system.drawing.bitmap" target="_blank">Bitmap</a> type but the main principles apply for this method, too.
        /// </summary>
        /// <param name="pixels">An <see cref="SKPixmap"/> instance, whose data is about to be accessed.</param>
        /// <param name="workingColorSpace">Determines the <see cref="IBitmapData.WorkingColorSpace"/> property of the result and
        /// specifies the preferred color space that should be used when working with the result bitmap data. The working color space
        /// can be different from the actual color space of the specified <paramref name="pixels"/>.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="WorkingColorSpace"/> enumeration for more details.</param>
        /// <param name="backColor">Determines the <see cref="IBitmapData.BackColor"/> property of the result.
        /// When setting pixels of bitmaps without alpha support, specifies the color of the background.
        /// Color values with alpha, which are considered opaque will be blended with this color before setting the pixel in the result bitmap data.
        /// The <see cref="SKColor.Alpha"/> property of the specified background color is ignored. This parameter is optional.
        /// <br/>Default value: The bitwise zero instance of <see cref="SKColor"/>, which has the same RGB values as <see cref="SKColors.Black"/>.</param>
        /// <param name="alphaThreshold">Determines the <see cref="IBitmapData.AlphaThreshold"/> property of the result.
        /// Can be relevant in some operations such as when drawing another <see cref="IReadableBitmapData"/> instance with alpha into the returned bitmap data
        /// by the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.DrawInto">DrawInto</see> extension methods and the specified <paramref name="pixels"/>
        /// has no alpha support. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance, which provides fast read-write access to the actual data of the specified <paramref name="pixels"/>.</returns>
        /// <seealso cref="SKBitmapExtensions.GetReadWriteBitmapData(SKBitmap, WorkingColorSpace, SKColor, byte)"/>
        /// <seealso cref="SKSurfaceExtensions.GetReadWriteBitmapData(SKSurface, WorkingColorSpace, SKColor, byte)"/>
        /// <seealso cref="BitmapDataFactory.CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/>
        public static IReadWriteBitmapData GetReadWriteBitmapData(this SKPixmap pixels, WorkingColorSpace workingColorSpace, SKColor backColor = default, byte alphaThreshold = 128)
            => pixels.GetBitmapDataInternal(false, workingColorSpace, backColor.ToColor32(), alphaThreshold);

        #endregion

        #region Internal Methods

        internal static IReadWriteBitmapData GetBitmapDataInternal(this SKPixmap pixels, bool readOnly, WorkingColorSpace workingColorSpace, Color32 backColor = default, byte alphaThreshold = 128, Action? disposeCallback = null)
        {
            if (pixels == null)
                throw new ArgumentNullException(nameof(pixels), PublicResources.ArgumentNull);
            SKImageInfo imageInfo = pixels.Info;
            if (imageInfo.IsEmpty)
                throw new ArgumentException(PublicResources.ArgumentEmpty, nameof(pixels));
            if (workingColorSpace is < WorkingColorSpace.Default or > WorkingColorSpace.Srgb)
                throw new ArgumentOutOfRangeException(nameof(workingColorSpace), PublicResources.EnumOutOfRange(workingColorSpace));

            // shortcut: if pixel format is directly supported, then we can simply create a bitmap data for its back buffer
            if (NativeBitmapDataFactory.TryCreateBitmapData(pixels.GetPixels(), imageInfo, pixels.RowBytes, backColor, alphaThreshold, workingColorSpace, disposeCallback, out IReadWriteBitmapData? bitmapData))
                return bitmapData;

            // otherwise, we create an SKBitmap for it, so the fallback manipulation can be used
            var bitmap = new SKBitmap();
            if (!bitmap.InstallPixels(pixels))
            {
                bitmap.Dispose();
                disposeCallback?.Invoke();
                throw new ArgumentException(PublicResources.ArgumentInvalid, nameof(pixels));
            }

            Action disposeBitmap = disposeCallback == null
                ? bitmap.Dispose
                : () =>
                {
                    bitmap.Dispose();
                    disposeCallback.Invoke();
                };
            return bitmap.GetFallbackBitmapData(readOnly, workingColorSpace, backColor, alphaThreshold, disposeBitmap);
        }

        #endregion

        #endregion
    }
}
