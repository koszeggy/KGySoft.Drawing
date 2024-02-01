#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: SKBitmapExtensions.cs
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
using System.Threading.Tasks;

using KGySoft.CoreLibraries;
using KGySoft.Drawing.Imaging;
using KGySoft.Threading;

using SkiaSharp;

#endregion

namespace KGySoft.Drawing.SkiaSharp
{
    /// <summary>
    /// Contains extension methods for the <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap">SKBitmap</a> type.
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

        #region GetXXXBitmapData

        /// <summary>
        /// Gets a managed read-only accessor for an <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap">SKBitmap</a> instance.
        /// <br/>See the <strong>Remarks</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_GetReadWriteBitmapData.htm">BitmapExtensions.GetReadWriteBitmapData</a>
        /// method for details and code samples. That method is for the GDI+ <a href="https://docs.microsoft.com/en-us/dotnet/api/system.drawing.bitmap" target="_blank">Bitmap</a> type but the main principles apply for this method, too.
        /// </summary>
        /// <param name="bitmap">An <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap">SKBitmap</a> instance, whose data is about to be accessed.</param>
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
        /// <returns>An <see cref="IReadableBitmapData"/> instance, which provides fast read-only access to the actual data of the specified <paramref name="bitmap"/>.</returns>
        /// <seealso cref="SKPixmapExtensions.GetReadableBitmapData(SKPixmap, SKColor, byte)"/>
        /// <seealso cref="SKImageExtensions.GetReadableBitmapData(SKImage, SKColor, byte)"/>
        /// <seealso cref="SKSurfaceExtensions.GetReadableBitmapData(SKSurface, SKColor, byte)"/>
        /// <seealso cref="BitmapDataFactory.CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/>
        public static IReadableBitmapData GetReadableBitmapData(this SKBitmap bitmap, SKColor backColor = default, byte alphaThreshold = 128)
            => bitmap.GetBitmapDataInternal(true, WorkingColorSpace.Default, backColor.ToColor32(), alphaThreshold);

        /// <summary>
        /// Gets a managed read-only accessor for an <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap">SKBitmap</a> instance.
        /// <br/>See the <strong>Remarks</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_GetReadWriteBitmapData.htm">BitmapExtensions.GetReadWriteBitmapData</a>
        /// method for details and code samples. That method is for the GDI+ <a href="https://docs.microsoft.com/en-us/dotnet/api/system.drawing.bitmap" target="_blank">Bitmap</a> type but the main principles apply for this method, too.
        /// </summary>
        /// <param name="bitmap">An <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap">SKBitmap</a> instance, whose data is about to be accessed.</param>
        /// <param name="workingColorSpace">Determines the <see cref="IBitmapData.WorkingColorSpace"/> property of the result and
        /// specifies the preferred color space that should be used when working with the result bitmap data. The working color space
        /// can be different from the actual color space of the specified <paramref name="bitmap"/>.
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
        /// <returns>An <see cref="IReadableBitmapData"/> instance, which provides fast read-only access to the actual data of the specified <paramref name="bitmap"/>.</returns>
        /// <seealso cref="SKPixmapExtensions.GetReadableBitmapData(SKPixmap, WorkingColorSpace, SKColor, byte)"/>
        /// <seealso cref="SKImageExtensions.GetReadableBitmapData(SKImage, WorkingColorSpace, SKColor, byte)"/>
        /// <seealso cref="SKSurfaceExtensions.GetReadableBitmapData(SKSurface, WorkingColorSpace, SKColor, byte)"/>
        /// <seealso cref="BitmapDataFactory.CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/>
        public static IReadableBitmapData GetReadableBitmapData(this SKBitmap bitmap, WorkingColorSpace workingColorSpace, SKColor backColor = default, byte alphaThreshold = 128)
            => bitmap.GetBitmapDataInternal(true, workingColorSpace, backColor.ToColor32(), alphaThreshold);

        /// <summary>
        /// Gets a managed write-only accessor for an <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap">SKBitmap</a> instance.
        /// <br/>See the <strong>Remarks</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_GetReadWriteBitmapData.htm">BitmapExtensions.GetReadWriteBitmapData</a>
        /// method for details and code samples. That method is for the GDI+ <a href="https://docs.microsoft.com/en-us/dotnet/api/system.drawing.bitmap" target="_blank">Bitmap</a> type but the main principles apply for this method, too.
        /// </summary>
        /// <param name="bitmap">An <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap">SKBitmap</a> instance, whose data is about to be accessed.</param>
        /// <param name="backColor">Determines the <see cref="IBitmapData.BackColor"/> property of the result.
        /// When setting pixels of bitmaps without alpha support, specifies the color of the background.
        /// Color values with alpha will be blended with this color before setting the pixel in the result bitmap data.
        /// The <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcolor.alpha">Alpha</a> property of the specified background color is ignored. This parameter is optional.
        /// <br/>Default value: The bitwise zero instance of <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcolor">SKColor</a>, which has the same RGB values as <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcolors.black">Black</a>.</param>
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
        /// Gets a managed write-only accessor for an <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap">SKBitmap</a> instance.
        /// <br/>See the <strong>Remarks</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_GetReadWriteBitmapData.htm">BitmapExtensions.GetReadWriteBitmapData</a>
        /// method for details and code samples. That method is for the GDI+ <a href="https://docs.microsoft.com/en-us/dotnet/api/system.drawing.bitmap" target="_blank">Bitmap</a> type but the main principles apply for this method, too.
        /// </summary>
        /// <param name="bitmap">An <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap">SKBitmap</a> instance, whose data is about to be accessed.</param>
        /// <param name="workingColorSpace">Determines the <see cref="IBitmapData.WorkingColorSpace"/> property of the result and
        /// specifies the preferred color space that should be used when working with the result bitmap data. The working color space
        /// can be different from the actual color space of the specified <paramref name="bitmap"/>.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="WorkingColorSpace"/> enumeration for more details.</param>
        /// <param name="backColor">Determines the <see cref="IBitmapData.BackColor"/> property of the result.
        /// When setting pixels of bitmaps without alpha support, specifies the color of the background.
        /// Color values with alpha will be blended with this color before setting the pixel in the result bitmap data.
        /// The <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcolor.alpha">Alpha</a> property of the specified background color is ignored. This parameter is optional.
        /// <br/>Default value: The bitwise zero instance of <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcolor">SKColor</a>, which has the same RGB values as <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcolors.black">Black</a>.</param>
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
        /// Gets a managed read-write accessor for an <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap">SKBitmap</a> instance.
        /// <br/>See the <strong>Remarks</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_GetReadWriteBitmapData.htm">BitmapExtensions.GetReadWriteBitmapData</a>
        /// method for details and code samples. That method is for the GDI+ <a href="https://docs.microsoft.com/en-us/dotnet/api/system.drawing.bitmap" target="_blank">Bitmap</a> type but the main principles apply for this method, too.
        /// </summary>
        /// <param name="bitmap">An <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap">SKBitmap</a> instance, whose data is about to be accessed.</param>
        /// <param name="backColor">Determines the <see cref="IBitmapData.BackColor"/> property of the result.
        /// When setting pixels of bitmaps without alpha support, specifies the color of the background.
        /// Color values with alpha, which are considered opaque will be blended with this color before setting the pixel in the result bitmap data.
        /// The <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcolor.alpha">Alpha</a> property of the specified background color is ignored. This parameter is optional.
        /// <br/>Default value: The bitwise zero instance of <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcolor">SKColor</a>, which has the same RGB values as <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcolors.black">Black</a>.</param>
        /// <param name="alphaThreshold">Determines the <see cref="IBitmapData.AlphaThreshold"/> property of the result.
        /// Can be relevant in some operations such as when drawing another <see cref="IReadableBitmapData"/> instance with alpha into the returned bitmap data
        /// by the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.DrawInto">DrawInto</see> extension methods and the specified <paramref name="bitmap"/>
        /// has no alpha support. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance, which provides fast read-write access to the actual data of the specified <paramref name="bitmap"/>.</returns>
        /// <seealso cref="SKPixmapExtensions.GetReadWriteBitmapData(SKPixmap, SKColor, byte)"/>
        /// <seealso cref="SKSurfaceExtensions.GetReadWriteBitmapData(SKSurface, SKColor, byte)"/>
        /// <seealso cref="BitmapDataFactory.CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/>
        public static IReadWriteBitmapData GetReadWriteBitmapData(this SKBitmap bitmap, SKColor backColor = default, byte alphaThreshold = 128)
            => bitmap.GetBitmapDataInternal(false, WorkingColorSpace.Default, backColor.ToColor32(), alphaThreshold, bitmap.NotifyPixelsChanged);

        /// <summary>
        /// Gets a managed read-write accessor for an <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap">SKBitmap</a> instance.
        /// <br/>See the <strong>Remarks</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_GetReadWriteBitmapData.htm">BitmapExtensions.GetReadWriteBitmapData</a>
        /// method for details and code samples. That method is for the GDI+ <a href="https://docs.microsoft.com/en-us/dotnet/api/system.drawing.bitmap" target="_blank">Bitmap</a> type but the main principles apply for this method, too.
        /// </summary>
        /// <param name="bitmap">An <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap">SKBitmap</a> instance, whose data is about to be accessed.</param>
        /// <param name="workingColorSpace">Determines the <see cref="IBitmapData.WorkingColorSpace"/> property of the result and
        /// specifies the preferred color space that should be used when working with the result bitmap data. The working color space
        /// can be different from the actual color space of the specified <paramref name="bitmap"/>.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="WorkingColorSpace"/> enumeration for more details.</param>
        /// <param name="backColor">Determines the <see cref="IBitmapData.BackColor"/> property of the result.
        /// When setting pixels of bitmaps without alpha support, specifies the color of the background.
        /// Color values with alpha, which are considered opaque will be blended with this color before setting the pixel in the result bitmap data.
        /// The <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcolor.alpha">Alpha</a> property of the specified background color is ignored. This parameter is optional.
        /// <br/>Default value: The bitwise zero instance of <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcolor">SKColor</a>, which has the same RGB values as <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcolors.black">Black</a>.</param>
        /// <param name="alphaThreshold">Determines the <see cref="IBitmapData.AlphaThreshold"/> property of the result.
        /// Can be relevant in some operations such as when drawing another <see cref="IReadableBitmapData"/> instance with alpha into the returned bitmap data
        /// by the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.DrawInto">DrawInto</see> extension methods and the specified <paramref name="bitmap"/>
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

        /// <summary>
        /// Converts the pixel format of this <paramref name="bitmap"/> using the specified <paramref name="colorType"/>, <paramref name="alphaType"/> and <paramref name="targetColorSpace"/>.
        /// </summary>
        /// <param name="bitmap">The original bitmap to convert.</param>
        /// <param name="colorType">Determines the <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap.colortype">ColorType</a> property of the result <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap">SKBitmap</a>.
        /// Can be <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcolortype">Unknown</a> to use the original color type of the source <paramref name="bitmap"/>.</param>
        /// <param name="alphaType">Determines the <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap.alphatype">AlphaType</a> property of the result <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap">SKBitmap</a>.
        /// It might be ignored if the <paramref name="colorType"/> cannot have the specified alpha type.
        /// Can be <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skalphatype">Unknown</a> to use the original alpha type of the source <paramref name="bitmap"/>. This parameter is optional.
        /// <br/>Default value: <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skalphatype">Unknown</a>.</param>
        /// <param name="targetColorSpace">Determines both the <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap.colorspace">ColorSpace</a> property of the result <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap">SKBitmap</a>,
        /// and also the working color space if the result does not support transparency and source pixels needed to be blended with <paramref name="backColor"/>.
        /// Can be <see cref="WorkingColorSpace.Default"/> to preserve the original color space. This parameter is optional.
        /// <br/>Default value: <see cref="WorkingColorSpace.Default"/>.</param>
        /// <param name="backColor">If the result does not support alpha, then specifies the color of the background.
        /// Source pixels with alpha, which will be opaque in the result will be blended with this color.
        /// The <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcolor.alpha">Alpha</a> property of the background color is ignored. This parameter is optional.
        /// <br/>Default value: The bitwise zero instance of <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcolor">SKColor</a>, which has the same RGB values as <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcolors.black">Black</a>.</param>
        /// <param name="alphaThreshold">If the result supports alpha, then specifies a threshold value for the <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcolor.alpha">Alpha</a> property, under which
        /// the color is considered completely transparent. If 0, then the converted colors attempt to preserve their original alpha value. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <returns>A new <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap">SKBitmap</a> instance with the desired parameters.</returns>
        /// <remarks>
        /// <note><list type="bullet">
        /// <item>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. Use
        /// the <see cref="ConvertPixelFormatAsync(SKBitmap, SKColorType, SKAlphaType, WorkingColorSpace, SKColor, byte, TaskConfig?)"/> method for asynchronous call
        /// and to adjust parallelization, set up cancellation and for reporting progress.</item>
        /// <item>The <paramref name="targetColorSpace"/> parameter is purposely not an <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcolorspace">SKColorSpace</a> value because only sRGB and linear color spaces are supported directly.
        /// If its value is <see cref="WorkingColorSpace.Linear"/>, then both the actual color space of the result and the working color space of the conversion operation will be in
        /// the linear color space. To create a result with sRGB color space but perform the conversion in the linear color space use
        /// the <see cref="ConvertPixelFormat(SKBitmap, IQuantizer?, IDitherer?, SKColorType, SKAlphaType, WorkingColorSpace)"/> overload with an <see cref="IQuantizer"/>
        /// configured to work in the linear color space.</item>
        /// </list></note>
        /// <para>If the result <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap">SKBitmap</a> can represent fewer colors than the source <paramref name="bitmap"/>, then a default
        /// quantization will occur during the conversion. To use a specific quantizer (and optionally a ditherer) use the <see cref="ConvertPixelFormat(SKBitmap, IQuantizer?, IDitherer?, SKColorType, SKAlphaType, WorkingColorSpace)"/> overload.
        /// To use a quantizer with a specific palette you can use the <see cref="PredefinedColorsQuantizer"/> class.</para>
        /// </remarks>
        /// <example>
        /// <para>The method may produce the following results:
        /// <table class="table is-hoverable">
        /// <thead><tr><th width="50%"><div style="text-align:center;">Original image</div></th><th width="50%"><div style="text-align:center;">Quantized image</div></th></tr></thead>
        /// <tbody>
        /// <tr><td><div style="text-align:center;">
        /// <para><img src="../Help/Images/AlphaGradient.png" alt="Color hues with alpha gradient"/>
        /// <br/>Color hues with alpha gradient</para></div></td>
        /// <td><div style="text-align:center;">
        /// <para><img src="../Help/Images/AlphaGradientArgb4444Unpremul.png" alt="Color hues with unpremultiplied ARGB4444 sRGB pixel format"/>
        /// <br/><paramref name="colorType"/> = <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcolortype">Argb4444</a>, <paramref name="alphaType"/> = <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skalphatype">Unpremul</a>, <paramref name="targetColorSpace"/> = <see cref="WorkingColorSpace.Srgb"/>, the other parameters have their default value.</para>
        /// <para><img src="../Help/Images/AlphaGradientRgb565Black.png" alt="Color hues with RGB565 sRGB pixel format and black background"/>
        /// <br/><paramref name="colorType"/> = <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcolortype">Rgb565</a>, <paramref name="targetColorSpace"/> = <see cref="WorkingColorSpace.Srgb"/>, the other parameters have their default value.</para>
        /// <para><img src="../Help/Images/AlphaGradientRgb565LinearBlack.png" alt="Color hues with RGB565 linear pixel format and black background"/>
        /// <br/><paramref name="colorType"/> = <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcolortype">Rgb565</a>, <paramref name="targetColorSpace"/> = <see cref="WorkingColorSpace.Linear"/>, the other parameters have their default value.</para></div></td>
        /// </tr>
        /// </tbody></table></para>
        /// </example>
        /// <exception cref="ArgumentNullException"><paramref name="bitmap"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="colorType"/>, <paramref name="alphaType"/> or <paramref name="targetColorSpace"/> does not specify a defined value.</exception>
        /// <seealso cref="ConvertPixelFormat(SKBitmap, IQuantizer?, IDitherer?, SKColorType, SKAlphaType, WorkingColorSpace)"/>
        /// <seealso cref="BitmapDataExtensions.Clone(IReadableBitmapData, KnownPixelFormat, Color32, byte)"/>
        public static SKBitmap ConvertPixelFormat(this SKBitmap bitmap, SKColorType colorType, SKAlphaType alphaType = SKAlphaType.Unknown,
            WorkingColorSpace targetColorSpace = WorkingColorSpace.Default, SKColor backColor = default, byte alphaThreshold = 128)
        {
            ValidateArguments(bitmap, colorType, alphaType, targetColorSpace);
            return DoConvertPixelFormat(AsyncHelper.DefaultContext, bitmap, GetImageInfo(bitmap, colorType, alphaType, targetColorSpace), backColor.ToColor32(), alphaThreshold)!;
        }

        /// <summary>
        /// Converts the pixel format of this <paramref name="bitmap"/> using the specified <paramref name="colorType"/>, <paramref name="alphaType"/> and <paramref name="targetColorSpace"/>.
        /// </summary>
        /// <param name="bitmap">The original bitmap to convert.</param>
        /// <param name="quantizer">An optional <see cref="IQuantizer"/> instance to determine the colors of the result.
        /// Can be <see langword="null"/> to pick a quantizer automatically that matches the other parameters.
        /// If no further parameters are specified, then the original pixel format is preserved while colors are optionally quantized.</param>
        /// <param name="ditherer">The ditherer to be used. Might be ignored if <paramref name="quantizer"/> is not specified
        /// and <paramref name="colorType"/> represents a higher bits-per-pixel per color channel format. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="colorType">Determines the <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap.colortype">ColorType</a> property of the result <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap">SKBitmap</a>.
        /// Can be <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcolortype">Unknown</a> to use the original color type of the source <paramref name="bitmap"/>. This parameter is optional.
        /// <br/>Default value: <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcolortype">Unknown</a>.</param>
        /// <param name="alphaType">Determines the <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap.alphatype">AlphaType</a> property of the result <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap">SKBitmap</a>.
        /// It might be ignored if the <paramref name="colorType"/> cannot have the specified alpha type.
        /// Can be <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skalphatype">Unknown</a> to use the original alpha type of the source <paramref name="bitmap"/>. This parameter is optional.
        /// <br/>Default value: <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skalphatype">Unknown</a>.</param>
        /// <param name="targetColorSpace">Determines both the <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap.colorspace">ColorSpace</a> property of the result <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap">SKBitmap</a>,
        /// and also the working color space if <paramref name="quantizer"/> is <see langword="null"/>.
        /// Can be <see cref="WorkingColorSpace.Default"/> to preserve the original color space. This parameter is optional.
        /// <br/>Default value: <see cref="WorkingColorSpace.Default"/>.</param>
        /// <returns>A new <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap">SKBitmap</a> instance with the desired parameters.</returns>
        /// <remarks>
        /// <note><list type="bullet">
        /// <item>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. Use
        /// the <see cref="ConvertPixelFormatAsync(SKBitmap, SKColorType, SKAlphaType, WorkingColorSpace, SKColor, byte, TaskConfig?)"/> method for asynchronous call
        /// and to adjust parallelization, set up cancellation and for reporting progress.</item>
        /// <item>The <paramref name="targetColorSpace"/> parameter is purposely not an <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcolorspace">SKColorSpace</a> value because only sRGB and linear color spaces are supported directly.
        /// If its value is <see cref="WorkingColorSpace.Linear"/>, then both the actual color space of the result and the working color space of the conversion operation will be in
        /// the linear color space (unless <paramref name="quantizer"/> is specified, which determines the working color space).
        /// To create a result with sRGB color space but perform the conversion in the linear color space you can use
        /// a <paramref name="quantizer"/> and configure it to work in the linear color space.</item>
        /// </list></note>
        /// <para>If the result <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap">SKBitmap</a> can represent fewer colors than the source <paramref name="bitmap"/> and <paramref name="quantizer"/> is <see langword="null"/>,
        /// then a default quantization will occur during the conversion. To use a quantizer with a specific palette you can use the <see cref="PredefinedColorsQuantizer"/> class.</para>
        /// <para>If only the <paramref name="quantizer"/> parameter is specified, then the original pixel format will be preserved but the actual colors
        /// will be quantized. You can also specify the <paramref name="ditherer"/> parameter to preserve more details while reducing the colors.</para>
        /// <para>Using a <paramref name="quantizer"/> that can represent more colors than the result <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap">SKBitmap</a> may end up in a poor quality result.</para>
        /// </remarks>
        /// <example>
        /// <para>The method may produce the following results:
        /// <table class="table is-hoverable">
        /// <thead><tr><th width="50%"><div style="text-align:center;">Original image</div></th><th width="50%"><div style="text-align:center;">Quantized image</div></th></tr></thead>
        /// <tbody>
        /// <tr><td><div style="text-align:center;">
        /// <para><img src="../Help/Images/AlphaGradient.png" alt="Color hues with alpha gradient"/>
        /// <br/>Color hues with alpha gradient</para></div></td>
        /// <td><div style="text-align:center;">
        /// <para><img src="../Help/Images/AlphaGradientArgb4444Unpremul.png" alt="Color hues with unpremultiplied ARGB4444 sRGB pixel format"/>
        /// <br/>Converting without a <paramref name="quantizer"/> and <paramref name="ditherer"/>, <paramref name="colorType"/> = <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcolortype">Argb4444</a>, <paramref name="alphaType"/> = <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skalphatype">Unpremul</a>.</para>
        /// <para><img src="../Help/Images/AlphaGradientArgb4444UnpremulDitheredB8.png" alt="Color hues with unpremultiplied ARGB4444 sRGB pixel format dithered by Bayer 8x8 dithering and black back color."/>
        /// <br/><paramref name="colorType"/> = <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcolortype">Argb4444</a>, using <see cref="OrderedDitherer.Bayer8x8">Bayer 8x8</see> dithering. No quantizer is specified so a default black back color was applied.
        /// As this dithering does not support partial transparency, the alpha pixels were blended with black and the bottom 16 lines are fully transparent.</para>
        /// <para><img src="../Help/Images/AlphaGradientArgb1555WhiteA16Dithered.png" alt="Color hues quantized to ARGB1555 color space with white background and dithered by dotted halftone dithering."/>
        /// <br/>Using <see cref="PredefinedColorsQuantizer.Argb1555">Argb1555</see> quantizer with white background, alpha threshold = 16 along with <see cref="OrderedDitherer.DottedHalftone">Dotted Halftone</see> dithering.
        /// All of the other parameters have their default value so the result color type and alpha type are the same as the original, it's just the specified quantizer that reduces the number of colors
        /// and turns the bottom 16 lines completely transparent.</para></div></td>
        /// </tr>
        /// </tbody></table></para>
        /// </example>
        /// <exception cref="ArgumentNullException"><paramref name="bitmap"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="colorType"/>, <paramref name="alphaType"/> or <paramref name="targetColorSpace"/> does not specify a defined value.</exception>
        /// <seealso cref="ConvertPixelFormat(SKBitmap, SKColorType, SKAlphaType, WorkingColorSpace, SKColor, byte)"/>
        /// <seealso cref="BitmapDataExtensions.Clone(IReadableBitmapData, KnownPixelFormat, IQuantizer, IDitherer)"/>
        public static SKBitmap ConvertPixelFormat(this SKBitmap bitmap, IQuantizer? quantizer, IDitherer? ditherer = null,
            SKColorType colorType = SKColorType.Unknown, SKAlphaType alphaType = SKAlphaType.Unknown, WorkingColorSpace targetColorSpace = WorkingColorSpace.Default)
        {
            ValidateArguments(bitmap, colorType, alphaType, targetColorSpace);
            return DoConvertPixelFormat(AsyncHelper.DefaultContext, bitmap, GetImageInfo(bitmap, colorType, alphaType, targetColorSpace), quantizer, ditherer)!;
        }

        /// <summary>
        /// Converts the pixel format of this <paramref name="bitmap"/> asynchronously, using the specified <paramref name="colorType"/>, <paramref name="alphaType"/> and <paramref name="targetColorSpace"/>.
        /// </summary>
        /// <param name="bitmap">The original bitmap to convert.</param>
        /// <param name="colorType">Determines the <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap.colortype">ColorType</a> property of the result <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap">SKBitmap</a>.
        /// Can be <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcolortype">Unknown</a> to use the original color type of the source <paramref name="bitmap"/>.</param>
        /// <param name="alphaType">Determines the <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap.alphatype">AlphaType</a> property of the result <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap">SKBitmap</a>.
        /// It might be ignored if the <paramref name="colorType"/> cannot have the specified alpha type.
        /// Can be <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skalphatype">Unknown</a> to use the original alpha type of the source <paramref name="bitmap"/>. This parameter is optional.
        /// <br/>Default value: <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skalphatype">Unknown</a>.</param>
        /// <param name="targetColorSpace">Determines both the <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap.colorspace">ColorSpace</a> property of the result <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap">SKBitmap</a>,
        /// and also the working color space if the result does not support transparency and source pixels needed to be blended with <paramref name="backColor"/>.
        /// Can be <see cref="WorkingColorSpace.Default"/> to preserve the original color space. This parameter is optional.
        /// <br/>Default value: <see cref="WorkingColorSpace.Default"/>.</param>
        /// <param name="backColor">If the result does not support alpha, then specifies the color of the background.
        /// Source pixels with alpha, which will be opaque in the result will be blended with this color.
        /// The <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcolor.alpha">Alpha</a> property of the background color is ignored. This parameter is optional.
        /// <br/>Default value: The bitwise zero instance of <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcolor">SKColor</a>, which has the same RGB values as <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcolors.black">Black</a>.</param>
        /// <param name="alphaThreshold">If the result supports alpha, then specifies a threshold value for the <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcolor.alpha">Alpha</a> property, under which
        /// the color is considered completely transparent. If 0, then the converted colors attempt to preserve their original alpha value. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. Its result is an <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap">SKBitmap</a> instance converted from the specified <paramref name="bitmap"/>,
        /// or <see langword="null"/>, if the operation was canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property of the <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="ConvertPixelFormat(SKBitmap, SKColorType, SKAlphaType, WorkingColorSpace, SKColor, byte)"/> method for more details.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmap"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="colorType"/>, <paramref name="alphaType"/> or <paramref name="targetColorSpace"/> does not specify a defined value.</exception>
        public static Task<SKBitmap?> ConvertPixelFormatAsync(this SKBitmap bitmap, SKColorType colorType, SKAlphaType alphaType = SKAlphaType.Unknown,
            WorkingColorSpace targetColorSpace = WorkingColorSpace.Default, SKColor backColor = default, byte alphaThreshold = 128, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmap, colorType, alphaType, targetColorSpace);
            return AsyncHelper.DoOperationAsync(ctx => DoConvertPixelFormat(ctx, bitmap, GetImageInfo(bitmap, colorType, alphaType, targetColorSpace), backColor.ToColor32(), alphaThreshold), asyncConfig);
        }

        /// <summary>
        /// Converts the pixel format of this <paramref name="bitmap"/> asynchronously, using the specified <paramref name="colorType"/>, <paramref name="alphaType"/> and <paramref name="targetColorSpace"/>.
        /// </summary>
        /// <param name="bitmap">The original bitmap to convert.</param>
        /// <param name="quantizer">An optional <see cref="IQuantizer"/> instance to determine the colors of the result.
        /// Can be <see langword="null"/> to pick a quantizer automatically that matches the other parameters.
        /// If no further parameters are specified, then the original pixel format is preserved while colors are optionally quantized.</param>
        /// <param name="ditherer">The ditherer to be used. Might be ignored if <paramref name="quantizer"/> is not specified
        /// and <paramref name="colorType"/> represents a higher bits-per-pixel per color channel format. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="colorType">Determines the <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap.colortype">ColorType</a> property of the result <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap">SKBitmap</a>.
        /// Can be <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcolortype">Unknown</a> to use the original color type of the source <paramref name="bitmap"/>. This parameter is optional.
        /// <br/>Default value: <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcolortype">Unknown</a>.</param>
        /// <param name="alphaType">Determines the <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap.alphatype">AlphaType</a> property of the result <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap">SKBitmap</a>.
        /// It might be ignored if the <paramref name="colorType"/> cannot have the specified alpha type.
        /// Can be <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skalphatype">Unknown</a> to use the original alpha type of the source <paramref name="bitmap"/>. This parameter is optional.
        /// <br/>Default value: <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skalphatype">Unknown</a>.</param>
        /// <param name="targetColorSpace">Determines both the <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap.colorspace">ColorSpace</a> property of the result <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap">SKBitmap</a>,
        /// and also the working color space if <paramref name="quantizer"/> is <see langword="null"/>.
        /// Can be <see cref="WorkingColorSpace.Default"/> to preserve the original color space. This parameter is optional.
        /// <br/>Default value: <see cref="WorkingColorSpace.Default"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. Its result is an <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap">SKBitmap</a> instance converted from the specified <paramref name="bitmap"/>,
        /// or <see langword="null"/>, if the operation was canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property of the <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="ConvertPixelFormat(SKBitmap, IQuantizer?, IDitherer?, SKColorType, SKAlphaType, WorkingColorSpace)"/> method for more details.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmap"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="colorType"/>, <paramref name="alphaType"/> or <paramref name="targetColorSpace"/> does not specify a defined value.</exception>
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
                > SKColorType.Unknown and <= ColorExtensions.MaxColorType => info.ColorType,

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
