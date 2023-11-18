#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: WriteableBitmapExtensions.cs
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

#region Used Namespaces

using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using KGySoft.Drawing.Imaging;

#endregion

#region Used Aliases

using Size = System.Drawing.Size;

#endregion

#endregion

namespace KGySoft.Drawing.Wpf
{
    /// <summary>
    /// Contains extension methods for the <see cref="WriteableBitmap"/> type.
    /// </summary>
    public static class WriteableBitmapExtensions
    {
        #region Methods

        #region Public Methods

        /// <summary>
        /// Gets a managed read-write accessor for a <see cref="WriteableBitmap"/> instance.
        /// <br/>See the <strong>Remarks</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_GetReadWriteBitmapData.htm">BitmapExtensions.GetReadWriteBitmapData</a>
        /// method for details and code samples. That method is for the GDI+ <a href="https://docs.microsoft.com/en-us/dotnet/api/system.drawing.bitmap" target="_blank">Bitmap</a> type but the main principles apply for this method, too.
        /// </summary>
        /// <param name="bitmap">A <see cref="WriteableBitmap"/> instance, whose data is about to be accessed.</param>
        /// <param name="backColor">When setting pixels of indexed bitmaps and bitmaps without alpha support, specifies the color of the background.
        /// Color values with alpha, which are considered opaque will be blended with this color before setting the pixel in the result bitmap data.
        /// The alpha value (<see cref="Color.A">Color.A</see> property) of the specified background color is ignored. This parameter is optional.
        /// <br/>Default value: The bitwise zero instance of <see cref="Color"/>, which has the same RGB values as <see cref="Colors.Black"/>.</param>
        /// <param name="alphaThreshold">When setting pixels of bitmaps with a palette that has a transparent color,
        /// then specifies a threshold value for the <see cref="Color.A">Color.A</see> property, under which the color is considered transparent. If 0,
        /// then the pixels to be set will never be transparent. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance, which provides fast read-write access to the actual data of the specified <paramref name="bitmap"/>.</returns>
        /// <seealso cref="BitmapSourceExtensions.GetReadableBitmapData(BitmapSource, WorkingColorSpace, Color, byte)"/>
        /// <seealso cref="GetWritableBitmapData(WriteableBitmap, WorkingColorSpace, Color, byte)"/>
        /// <seealso cref="BitmapDataFactory.CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/>
        public static IReadWriteBitmapData GetReadWriteBitmapData(this WriteableBitmap bitmap, Color backColor = default, byte alphaThreshold = 128)
            => GetReadWriteBitmapData(bitmap, WorkingColorSpace.Default, backColor, alphaThreshold);

        /// <summary>
        /// Gets a managed read-write accessor for a <see cref="WriteableBitmap"/> instance.
        /// <br/>See the <strong>Remarks</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_GetReadWriteBitmapData.htm">BitmapExtensions.GetReadWriteBitmapData</a>
        /// method for details and code samples. That method is for the GDI+ <a href="https://docs.microsoft.com/en-us/dotnet/api/system.drawing.bitmap" target="_blank">Bitmap</a> type but the main principles apply for this method, too.
        /// </summary>
        /// <param name="bitmap">A <see cref="WriteableBitmap"/> instance, whose data is about to be accessed.</param>
        /// <param name="workingColorSpace">Specifies the preferred color space that should be used when working with the result bitmap data. Determines the behavior
        /// of some operations such as setting pixels with transparency when the pixel format of the source bitmap does not support transparency or alpha gradient.
        /// See also the <paramref name="backColor"/> and <paramref name="alphaThreshold"/> parameters.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="WorkingColorSpace"/> enumeration for more details.</param>
        /// <param name="backColor">When setting pixels of indexed bitmaps and bitmaps without alpha support, specifies the color of the background.
        /// Color values with alpha, which are considered opaque will be blended with this color before setting the pixel in the result bitmap data.
        /// The alpha value (<see cref="Color.A">Color.A</see> property) of the specified background color is ignored. This parameter is optional.
        /// <br/>Default value: The bitwise zero instance of <see cref="Color"/>, which has the same RGB values as <see cref="Colors.Black"/>.</param>
        /// <param name="alphaThreshold">When setting pixels of bitmaps with a palette that has a transparent color,
        /// then specifies a threshold value for the <see cref="Color.A">Color.A</see> property, under which the color is considered transparent. If 0,
        /// then the pixels to be set will never be transparent. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance, which provides fast read-write access to the actual data of the specified <paramref name="bitmap"/>.</returns>
        /// <seealso cref="BitmapSourceExtensions.GetReadableBitmapData(BitmapSource, WorkingColorSpace, Color, byte)"/>
        /// <seealso cref="GetWritableBitmapData(WriteableBitmap, WorkingColorSpace, Color, byte)"/>
        /// <seealso cref="BitmapDataFactory.CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/>
        public static IReadWriteBitmapData GetReadWriteBitmapData(this WriteableBitmap bitmap, WorkingColorSpace workingColorSpace, Color backColor = default, byte alphaThreshold = 128)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap), PublicResources.ArgumentNull);
            if (workingColorSpace is < WorkingColorSpace.Default or > WorkingColorSpace.Srgb)
                throw new ArgumentOutOfRangeException(nameof(workingColorSpace), PublicResources.EnumOutOfRange(workingColorSpace));
            if (bitmap.IsFrozen)
                throw new ArgumentException(Res.BitmapFrozen, nameof(bitmap));

            return GetBitmapDataInternal(bitmap, false, workingColorSpace, backColor, alphaThreshold);
        }

        /// <summary>
        /// Gets a managed write-only accessor for a <see cref="WriteableBitmap"/> instance.
        /// <br/>See the <strong>Remarks</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_GetReadWriteBitmapData.htm">BitmapExtensions.GetReadWriteBitmapData</a>
        /// method for details and code samples. That method is for the GDI+ <a href="https://docs.microsoft.com/en-us/dotnet/api/system.drawing.bitmap" target="_blank">Bitmap</a> type but the main principles apply for this method, too.
        /// </summary>
        /// <param name="bitmap">A <see cref="WriteableBitmap"/> instance, whose data is about to be accessed.</param>
        /// <param name="backColor">When setting pixels of indexed bitmaps and bitmaps without alpha support, specifies the color of the background.
        /// Color values with alpha, which are considered opaque will be blended with this color before setting the pixel in the result bitmap data.
        /// The alpha value (<see cref="Color.A">Color.A</see> property) of the specified background color is ignored. This parameter is optional.
        /// <br/>Default value: The bitwise zero instance of <see cref="Color"/>, which has the same RGB values as <see cref="Colors.Black"/>.</param>
        /// <param name="alphaThreshold">When setting pixels of bitmaps with a palette that has a transparent color,
        /// then specifies a threshold value for the <see cref="Color.A">Color.A</see> property, under which the color is considered transparent. If 0,
        /// then the pixels to be set will never be transparent. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <returns>An <see cref="IWritableBitmapData"/> instance, which provides fast write-only access to the actual data of the specified <paramref name="bitmap"/>.</returns>
        /// <seealso cref="BitmapSourceExtensions.GetReadableBitmapData(BitmapSource, WorkingColorSpace, Color, byte)"/>
        /// <seealso cref="GetReadWriteBitmapData(WriteableBitmap, WorkingColorSpace, Color, byte)"/>
        /// <seealso cref="BitmapDataFactory.CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/>
        public static IWritableBitmapData GetWritableBitmapData(this WriteableBitmap bitmap, Color backColor = default, byte alphaThreshold = 128)
            => GetReadWriteBitmapData(bitmap, WorkingColorSpace.Default, backColor, alphaThreshold);

        /// <summary>
        /// Gets a managed write-only accessor for a <see cref="WriteableBitmap"/> instance.
        /// <br/>See the <strong>Remarks</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_GetReadWriteBitmapData.htm">BitmapExtensions.GetReadWriteBitmapData</a>
        /// method for details and code samples. That method is for the GDI+ <a href="https://docs.microsoft.com/en-us/dotnet/api/system.drawing.bitmap" target="_blank">Bitmap</a> type but the main principles apply for this method, too.
        /// </summary>
        /// <param name="bitmap">A <see cref="WriteableBitmap"/> instance, whose data is about to be accessed.</param>
        /// <param name="workingColorSpace">Specifies the preferred color space that should be used when working with the result bitmap data. Determines the behavior
        /// of some operations such as setting pixels with transparency when the pixel format of the source bitmap does not support transparency or alpha gradient.
        /// See also the <paramref name="backColor"/> and <paramref name="alphaThreshold"/> parameters.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="WorkingColorSpace"/> enumeration for more details.</param>
        /// <param name="backColor">When setting pixels of indexed bitmaps and bitmaps without alpha support, specifies the color of the background.
        /// Color values with alpha, which are considered opaque will be blended with this color before setting the pixel in the result bitmap data.
        /// The alpha value (<see cref="Color.A">Color.A</see> property) of the specified background color is ignored. This parameter is optional.
        /// <br/>Default value: The bitwise zero instance of <see cref="Color"/>, which has the same RGB values as <see cref="Colors.Black"/>.</param>
        /// <param name="alphaThreshold">When setting pixels of bitmaps with a palette that has a transparent color,
        /// then specifies a threshold value for the <see cref="Color.A">Color.A</see> property, under which the color is considered transparent. If 0,
        /// then the pixels to be set will never be transparent. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <returns>An <see cref="IWritableBitmapData"/> instance, which provides fast write-only access to the actual data of the specified <paramref name="bitmap"/>.</returns>
        /// <seealso cref="BitmapSourceExtensions.GetReadableBitmapData(BitmapSource, WorkingColorSpace, Color, byte)"/>
        /// <seealso cref="GetReadWriteBitmapData(WriteableBitmap, WorkingColorSpace, Color, byte)"/>
        /// <seealso cref="BitmapDataFactory.CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/>
        public static IWritableBitmapData GetWritableBitmapData(this WriteableBitmap bitmap, WorkingColorSpace workingColorSpace, Color backColor = default, byte alphaThreshold = 128)
            => GetReadWriteBitmapData(bitmap, workingColorSpace, backColor, alphaThreshold);

        #endregion

        #region Internal Methods

        [SuppressMessage("Microsoft.Maintainability", "CA1502: Avoid excessive complexity", Justification = "Long but straightforward cases for the possible pixel formats.")]
        [SuppressMessage("VisualStudio.Style", "IDE0039: Use local function instead of lambda", Justification = "False alarm, it would be converted to a delegate anyway.")]
        [SuppressMessage("ReSharper", "ConvertToLocalFunction", Justification = "False alarm, it would be converted to a delegate anyway.")]
        internal static IReadWriteBitmapData GetBitmapDataInternal(this WriteableBitmap bitmap, bool readOnly,
            WorkingColorSpace workingColorSpace = WorkingColorSpace.Default, Color backColor = default, byte alphaThreshold = 128)
        {
            PixelFormat sourceFormat = bitmap.Format;
            KnownPixelFormat knownFormat = sourceFormat.AsKnownPixelFormat();

            bitmap.Lock();
            var size = new Size(bitmap.PixelWidth, bitmap.PixelHeight);
            Color32 backColor32 = backColor.ToColor32();
            Action dispose = () =>
            {
                if (!readOnly)
                    bitmap.AddDirtyRect(new Int32Rect(0, 0, size.Width, size.Height));
                bitmap.Unlock();
            };

            // Known pixel formats
            if (knownFormat != KnownPixelFormat.Undefined)
                return knownFormat.IsIndexed()
                    ? BitmapDataFactory.CreateBitmapData(bitmap.BackBuffer, size, bitmap.BackBufferStride, knownFormat,
                        bitmap.GetPalette(workingColorSpace, backColor, alphaThreshold), IndexedFormatsHelper.TrySetPalette, dispose)
                    : BitmapDataFactory.CreateBitmapData(bitmap.BackBuffer, size, bitmap.BackBufferStride, knownFormat, workingColorSpace, backColor32, alphaThreshold, dispose);

            // Custom pixel formats
            if (sourceFormat == PixelFormats.Rgb24)
                return BitmapDataFactory.CreateBitmapData(bitmap.BackBuffer, size, bitmap.BackBufferStride, new CustomBitmapDataConfig
                {
                    PixelFormat = new PixelFormatInfo(24),
                    BackColor = backColor.ToColor32(),
                    AlphaThreshold = alphaThreshold,
                    WorkingColorSpace = workingColorSpace,
                    DisposeCallback = dispose,
                    BackBufferIndependentPixelAccess = true,
                    RowGetColor32 = (row, x) => row.UnsafeGetRefAs<ColorRgb24>(x).ToColor32(),
                    RowSetColor32 = (row, x, c) => row.UnsafeGetRefAs<ColorRgb24>(x) =
                        new ColorRgb24(c.A == Byte.MaxValue ? c : c.Blend(row.BitmapData.BackColor, row.BitmapData.WorkingColorSpace)),
                });

            if (sourceFormat == PixelFormats.Indexed2)
                return BitmapDataFactory.CreateBitmapData(bitmap.BackBuffer, size, bitmap.BackBufferStride, new CustomIndexedBitmapDataConfig
                {
                    PixelFormat = new PixelFormatInfo(2) { Indexed = true },
                    Palette = bitmap.GetPalette(workingColorSpace, backColor, alphaThreshold),
                    TrySetPaletteCallback = IndexedFormatsHelper.TrySetPalette,
                    BackBufferIndependentPixelAccess = true,
                    DisposeCallback = dispose,
                    RowGetColorIndex = IndexedFormatsHelper.GetColorIndexI2,
                    RowSetColorIndex = IndexedFormatsHelper.SetColorIndexI2
                });

            if (sourceFormat == PixelFormats.BlackWhite)
            {
                Palette colors = Palette.BlackAndWhite(workingColorSpace, backColor32);
                return BitmapDataFactory.CreateBitmapData(bitmap.BackBuffer, size, bitmap.BackBufferStride, new CustomBitmapDataConfig
                {
                    PixelFormat = new PixelFormatInfo(1) { Grayscale = true },
                    BackColor = backColor.ToColor32(),
                    AlphaThreshold = alphaThreshold,
                    WorkingColorSpace = workingColorSpace,
                    BackBufferIndependentPixelAccess = true,
                    DisposeCallback = dispose,
                    RowGetColor32 = (row, x) => colors.GetColor(IndexedFormatsHelper.GetColorIndexI1(row, x)),
                    RowSetColor32 = (row, x, c) => IndexedFormatsHelper.SetColorIndexI1(row, x, colors.GetNearestColorIndex(c)),
                });
            }

            if (sourceFormat == PixelFormats.Gray2)
            {
                Palette colors = Palette.Grayscale4(workingColorSpace, backColor32);
                return BitmapDataFactory.CreateBitmapData(bitmap.BackBuffer, size, bitmap.BackBufferStride, new CustomBitmapDataConfig
                {
                    PixelFormat = new PixelFormatInfo(2) { Grayscale = true },
                    BackColor = backColor.ToColor32(),
                    AlphaThreshold = alphaThreshold,
                    WorkingColorSpace = workingColorSpace,
                    BackBufferIndependentPixelAccess = true,
                    DisposeCallback = dispose,
                    RowGetColor32 = (row, x) => colors.GetColor(IndexedFormatsHelper.GetColorIndexI2(row, x)),
                    RowSetColor32 = (row, x, c) => IndexedFormatsHelper.SetColorIndexI2(row, x, colors.GetNearestColorIndex(c)),
                });
            }

            if (sourceFormat == PixelFormats.Gray4)
            {
                Palette colors = Palette.Grayscale16(workingColorSpace, backColor32);
                return BitmapDataFactory.CreateBitmapData(bitmap.BackBuffer, size, bitmap.BackBufferStride, new CustomBitmapDataConfig
                {
                    PixelFormat = new PixelFormatInfo(4) { Grayscale = true },
                    BackColor = backColor.ToColor32(),
                    AlphaThreshold = alphaThreshold,
                    WorkingColorSpace = workingColorSpace,
                    BackBufferIndependentPixelAccess = true,
                    DisposeCallback = dispose,
                    RowGetColor32 = (row, x) => colors.GetColor(IndexedFormatsHelper.GetColorIndexI4(row, x)),
                    RowSetColor32 = (row, x, c) => IndexedFormatsHelper.SetColorIndexI4(row, x, colors.GetNearestColorIndex(c)),
                });
            }

            if (sourceFormat == PixelFormats.Bgr101010)
                return BitmapDataFactory.CreateBitmapData(bitmap.BackBuffer, size, bitmap.BackBufferStride, new CustomBitmapDataConfig
                {
                    PixelFormat = new PixelFormatInfo(32) { Prefers64BitColors = true },
                    BackColor = backColor.ToColor32(),
                    AlphaThreshold = alphaThreshold,
                    WorkingColorSpace = workingColorSpace,
                    BackBufferIndependentPixelAccess = true,
                    DisposeCallback = dispose,
                    RowGetColor32 = (row, x) => row.UnsafeGetRefAs<ColorBgr101010>(x).ToColor32(),
                    RowGetColor64 = (row, x) => row.UnsafeGetRefAs<ColorBgr101010>(x).ToColor64(),
                    RowSetColor64 = (row, x, c) => row.UnsafeGetRefAs<ColorBgr101010>(x) =
                        new ColorBgr101010(c.A == UInt16.MaxValue ? c : c.Blend(row.BitmapData.BackColor.ToColor64(), row.BitmapData.WorkingColorSpace)),
                });

            if (sourceFormat == PixelFormats.Rgb48)
                return BitmapDataFactory.CreateBitmapData(bitmap.BackBuffer, size, bitmap.BackBufferStride, new CustomBitmapDataConfig
                {
                    PixelFormat = new PixelFormatInfo(48) { Prefers64BitColors = true },
                    BackColor = backColor.ToColor32(),
                    AlphaThreshold = alphaThreshold,
                    WorkingColorSpace = workingColorSpace,
                    BackBufferIndependentPixelAccess = true,
                    DisposeCallback = dispose,
                    RowGetColor64 = (row, x) => row.UnsafeGetRefAs<ColorRgb48>(x).ToColor64(),
                    RowSetColor64 = (row, x, c) => row.UnsafeGetRefAs<ColorRgb48>(x) =
                        new ColorRgb48(c.A == UInt16.MaxValue ? c : c.Blend(row.BitmapData.BackColor.ToColor64(), row.BitmapData.WorkingColorSpace)),
                });

            if (sourceFormat == PixelFormats.Rgba64)
                return BitmapDataFactory.CreateBitmapData(bitmap.BackBuffer, size, bitmap.BackBufferStride, new CustomBitmapDataConfig
                {
                    PixelFormat = new PixelFormatInfo(64) { HasAlpha = true, Prefers64BitColors = true },
                    BackColor = backColor.ToColor32(),
                    AlphaThreshold = alphaThreshold,
                    WorkingColorSpace = workingColorSpace,
                    BackBufferIndependentPixelAccess = true,
                    DisposeCallback = dispose,
                    RowGetColor64 = (row, x) => row.UnsafeGetRefAs<ColorRgba64>(x).ToColor64(),
                    RowSetColor64 = (row, x, c) => row.UnsafeGetRefAs<ColorRgba64>(x) = new ColorRgba64(c),
                });

            if (sourceFormat == PixelFormats.Prgba64)
                return BitmapDataFactory.CreateBitmapData(bitmap.BackBuffer, size, bitmap.BackBufferStride, new CustomBitmapDataConfig
                {
                    PixelFormat = new PixelFormatInfo(64) { HasPremultipliedAlpha = true, Prefers64BitColors = true },
                    BackColor = backColor.ToColor32(),
                    AlphaThreshold = alphaThreshold,
                    WorkingColorSpace = workingColorSpace,
                    BackBufferIndependentPixelAccess = true,
                    DisposeCallback = dispose,
                    RowGetPColor64 = (row, x) => row.UnsafeGetRefAs<ColorPrgba64>(x).ToPColor64(),
                    RowSetPColor64 = (row, x, c) => row.UnsafeGetRefAs<ColorPrgba64>(x) = new ColorPrgba64(c),
                });

            if (sourceFormat == PixelFormats.Rgb128Float)
                return BitmapDataFactory.CreateBitmapData(bitmap.BackBuffer, size, bitmap.BackBufferStride, new CustomBitmapDataConfig
                {
                    PixelFormat = new PixelFormatInfo(128) { LinearGamma = true, Prefers128BitColors = true },
                    BackColor = backColor.ToColor32(),
                    AlphaThreshold = alphaThreshold,
                    WorkingColorSpace = workingColorSpace,
                    BackBufferIndependentPixelAccess = true,
                    DisposeCallback = dispose,
                    RowGetColorF = (row, x) => row.UnsafeGetRefAs<ColorF>(x).ToOpaque(),
                    RowSetColorF = (row, x, c) => row.UnsafeGetRefAs<ColorF>(x) =
                        c.A >= 1f ? c : c.Blend(row.BitmapData.BackColor.ToColorF(), row.BitmapData.WorkingColorSpace),
                });

            if (sourceFormat == PixelFormats.Cmyk32)
                return BitmapDataFactory.CreateBitmapData(bitmap.BackBuffer, size, bitmap.BackBufferStride, new CustomBitmapDataConfig
                {
                    PixelFormat = new PixelFormatInfo(32),
                    BackColor = backColor.ToColor32(),
                    AlphaThreshold = alphaThreshold,
                    WorkingColorSpace = workingColorSpace,
                    BackBufferIndependentPixelAccess = true,
                    DisposeCallback = dispose,
                    RowGetColor32 = (row, x) => row.UnsafeGetRefAs<ColorCmyk32>(x).ToColor32(),
                    RowSetColor32 = (row, x, c) => row.UnsafeGetRefAs<ColorCmyk32>(x) =
                        new ColorCmyk32(c.A == Byte.MaxValue ? c : c.Blend(row.BitmapData.BackColor, row.BitmapData.WorkingColorSpace))
                });

            bitmap.Unlock();
            throw new InvalidOperationException(Res.InternalError($"Unexpected PixelFormat {sourceFormat}"));
        }

        #endregion

        #endregion
    }
}
