#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BitmapSourceExtensions.cs
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

#region Used Namespaces

using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
#if !NET35
using System.Threading.Tasks;
#endif
using System.Windows.Media;
using System.Windows.Media.Imaging;

using KGySoft.Collections;
using KGySoft.Drawing.Imaging;
using KGySoft.Threading;

#endregion

#region Used Aliases

using Color = System.Windows.Media.Color;

#endregion

#endregion

#region Suppressions

#if NET35
#pragma warning disable CS1574 // XML comment has cref attribute that could not be resolved - in .NET 3.5 not all members are available
#endif

#endregion

namespace KGySoft.Drawing.Wpf
{
    /// <summary>
    /// Contains extension methods for the <see cref="BitmapSource"/> type.
    /// </summary>
    public static class BitmapSourceExtensions
    {
        #region Methods

        #region Public Methods

        #region GetReadableBitmapData

        /// <summary>
        /// Gets a managed read-only accessor for a <see cref="BitmapSource"/> instance.
        /// <br/>See the <strong>Remarks</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_GetReadWriteBitmapData.htm">BitmapExtensions.GetReadWriteBitmapData</a>
        /// method for details and code samples. That method is for the GDI+ <a href="https://docs.microsoft.com/en-us/dotnet/api/system.drawing.bitmap" target="_blank">Bitmap</a> type but the main principles apply for this method, too.
        /// </summary>
        /// <param name="bitmap">A <see cref="BitmapSource"/> instance, whose data is about to be accessed.</param>
        /// <param name="backColor">For an <see cref="IReadableBitmapData"/> instance the <paramref name="backColor"/> is relevant only for indexed bitmaps
        /// when <see cref="Palette.GetNearestColorIndex">GetNearestColorIndex</see> and <see cref="Palette.GetNearestColor">GetNearestColor</see> methods
        /// are called with an alpha color on the <see cref="IBitmapData.Palette"/> property. Queried colors with alpha, which are considered opaque will be blended
        /// with this color before performing a lookup. The alpha value (<see cref="Color.A">Color.A</see> property) of the specified background color is ignored. This parameter is optional.
        /// <br/>Default value: The bitwise zero instance of <see cref="Color"/>, which has the same RGB values as <see cref="Colors.Black"/>.</param>
        /// <param name="alphaThreshold">Similarly to <paramref name="backColor"/>, for an <see cref="IReadableBitmapData"/> instance the <paramref name="alphaThreshold"/> is relevant
        /// only for indexed bitmaps when <see cref="Palette.GetNearestColorIndex">GetNearestColorIndex</see> and <see cref="Palette.GetNearestColor">GetNearestColor</see> methods
        /// are called with an alpha color on the <see cref="IBitmapData.Palette"/> property. In such case determines the lowest alpha value of a color,
        /// which should not be considered as transparent. If 0, then a color lookup will never return a transparent color. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <returns>An <see cref="IReadableBitmapData"/> instance, which provides fast read-only access to the actual data of the specified <paramref name="bitmap"/>.</returns>
        /// <remarks>
        /// <note>In general, this method copies the content of the <paramref name="bitmap"/> to a new managed buffer to access its pixels. But if <paramref name="bitmap"/> is a non-frozen <see cref="WriteableBitmap"/>,
        /// then this method calls the <see cref="WriteableBitmapExtensions.GetReadWriteBitmapData(WriteableBitmap,WorkingColorSpace,Color,byte)">WriteableBitmapExtensions.GetReadWriteBitmapData</see> method,
        /// which avoids the need of copying the content; however, in this case the <see cref="IReadWriteBitmapData"/> instance is locked until the returned <see cref="IReadableBitmapData"/> instance is disposed.</note>
        /// </remarks>
        /// <seealso cref="WriteableBitmapExtensions.GetWritableBitmapData(WriteableBitmap, WorkingColorSpace, Color, byte)"/>
        /// <seealso cref="WriteableBitmapExtensions.GetReadWriteBitmapData(WriteableBitmap, WorkingColorSpace, Color, byte)"/>
        /// <seealso cref="BitmapDataFactory.CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/>
        public static IReadableBitmapData GetReadableBitmapData(this BitmapSource bitmap, Color backColor = default, byte alphaThreshold = 128)
            => GetReadableBitmapData(bitmap, WorkingColorSpace.Default, backColor, alphaThreshold);

        /// <summary>
        /// Gets a managed read-only accessor for a <see cref="BitmapSource"/> instance.
        /// <br/>See the <strong>Remarks</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_GetReadWriteBitmapData.htm">BitmapExtensions.GetReadWriteBitmapData</a>
        /// method for details and code samples. That method is for the GDI+ <a href="https://docs.microsoft.com/en-us/dotnet/api/system.drawing.bitmap" target="_blank">Bitmap</a> type but the main principles apply for this method, too.
        /// </summary>
        /// <param name="bitmap">A <see cref="BitmapSource"/> instance, whose data is about to be accessed.</param>
        /// <param name="workingColorSpace">Specifies the preferred color space that should be used when working with the result bitmap data. Determines the behavior
        /// of some operations such as resizing or looking up for nearest colors if <paramref name="bitmap"/> has an indexed pixel format.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="WorkingColorSpace"/> enumeration for more details.</param>
        /// <param name="backColor">For an <see cref="IReadableBitmapData"/> instance the <paramref name="backColor"/> is relevant only for indexed bitmaps
        /// when <see cref="Palette.GetNearestColorIndex">GetNearestColorIndex</see> and <see cref="Palette.GetNearestColor">GetNearestColor</see> methods
        /// are called with an alpha color on the <see cref="IBitmapData.Palette"/> property. Queried colors with alpha, which are considered opaque will be blended
        /// with this color before performing a lookup. The alpha value (<see cref="Color.A">Color.A</see> property) of the specified background color is ignored. This parameter is optional.
        /// <br/>Default value: The bitwise zero instance of <see cref="Color"/>, which has the same RGB values as <see cref="Colors.Black"/>.</param>
        /// <param name="alphaThreshold">Similarly to <paramref name="backColor"/>, for an <see cref="IReadableBitmapData"/> instance the <paramref name="alphaThreshold"/> is relevant
        /// only for indexed bitmaps when <see cref="Palette.GetNearestColorIndex">GetNearestColorIndex</see> and <see cref="Palette.GetNearestColor">GetNearestColor</see> methods
        /// are called with an alpha color on the <see cref="IBitmapData.Palette"/> property. In such case determines the lowest alpha value of a color,
        /// which should not be considered as transparent. If 0, then a color lookup will never return a transparent color. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <returns>An <see cref="IReadableBitmapData"/> instance, which provides fast read-only access to the actual data of the specified <paramref name="bitmap"/>.</returns>
        /// <remarks>
        /// <note>In general, this method copies the content of the <paramref name="bitmap"/> to a new managed buffer to access its pixels. But if <paramref name="bitmap"/> is a non-frozen <see cref="WriteableBitmap"/>,
        /// then this method calls the <see cref="WriteableBitmapExtensions.GetReadWriteBitmapData(WriteableBitmap,WorkingColorSpace,Color,byte)">WriteableBitmapExtensions.GetReadWriteBitmapData</see> method,
        /// which avoids the need of copying the content; however, in this case the <see cref="IReadWriteBitmapData"/> instance is locked until the returned <see cref="IReadableBitmapData"/> instance is disposed.</note>
        /// </remarks>
        /// <seealso cref="WriteableBitmapExtensions.GetWritableBitmapData(WriteableBitmap, WorkingColorSpace, Color, byte)"/>
        /// <seealso cref="WriteableBitmapExtensions.GetReadWriteBitmapData(WriteableBitmap, WorkingColorSpace, Color, byte)"/>
        /// <seealso cref="BitmapDataFactory.CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/>
        [SuppressMessage("VisualStudio.Style", "IDE0039: Use local function instead of lambda", Justification = "False alarm, it would be converted to a delegate anyway.")]
        [SuppressMessage("Microsoft.Maintainability", "CA1502: Avoid excessive complexity", Justification = "Long but straightforward cases for the possible pixel formats.")]
        public static IReadableBitmapData GetReadableBitmapData(this BitmapSource bitmap, WorkingColorSpace workingColorSpace, Color backColor = default, byte alphaThreshold = 128)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap), PublicResources.ArgumentNull);
            if (workingColorSpace is < WorkingColorSpace.Default or > WorkingColorSpace.Srgb)
                throw new ArgumentOutOfRangeException(nameof(workingColorSpace), PublicResources.EnumOutOfRange(workingColorSpace));

            if (bitmap is WriteableBitmap { IsFrozen: false } writeableBitmap)
                return writeableBitmap.GetBitmapDataInternal(true, workingColorSpace, backColor, alphaThreshold);

            PixelFormat sourceFormat = bitmap.Format;
            KnownPixelFormat knownFormat = sourceFormat.AsKnownPixelFormat();
            var size = new Size(bitmap.PixelWidth, bitmap.PixelHeight);
            Color32 backColor32 = backColor.ToColor32();
            int stride = (size.Width * sourceFormat.BitsPerPixel + 7) >> 3;

            // using an ArraySection as buffer because it can array pooling depending on size and platform
            var buffer = new ArraySection<byte>(size.Height * stride, false);
            Action dispose = () => buffer.Release();

            bitmap.CopyPixels(buffer.UnderlyingArray!, stride, 0);

            // Known pixel formats
            if (knownFormat != KnownPixelFormat.Undefined)
                return knownFormat.IsIndexed()
                    ? BitmapDataFactory.CreateBitmapData(buffer, size, stride, knownFormat,
                        bitmap.GetPalette(workingColorSpace, backColor, alphaThreshold), IndexedFormatsHelper.TrySetPalette, dispose)
                    : BitmapDataFactory.CreateBitmapData(buffer, size, stride, knownFormat, workingColorSpace, backColor32, alphaThreshold, dispose);

            // NOTE: For custom formats that can be dithered and have no palette we set RoSetColor* so PredefinedColorsQuantizer.FromBitmapData can work correctly

            // Custom pixel formats
            if (sourceFormat == PixelFormats.Rgb24)
                return BitmapDataFactory.CreateBitmapData(buffer, size, stride, new CustomBitmapDataConfig
                {
                    PixelFormat = new PixelFormatInfo(24),
                    BackColor = backColor.ToColor32(),
                    AlphaThreshold = alphaThreshold,
                    WorkingColorSpace = workingColorSpace,
                    DisposeCallback = dispose,
                    BackBufferIndependentPixelAccess = true,
                    RowGetColor32 = (row, x) => row.UnsafeGetRefAs<ColorRgb24>(x).ToColor32(),
                });

            if (sourceFormat == PixelFormats.Indexed2)
                return BitmapDataFactory.CreateBitmapData(buffer, size, stride, new CustomIndexedBitmapDataConfig
                {
                    PixelFormat = new PixelFormatInfo(2) { Indexed = true },
                    Palette = bitmap.GetPalette(workingColorSpace, backColor, alphaThreshold),
                    TrySetPaletteCallback = IndexedFormatsHelper.TrySetPalette,
                    BackBufferIndependentPixelAccess = true,
                    DisposeCallback = dispose,
                    RowGetColorIndex = IndexedFormatsHelper.GetColorIndexI2,
                });

            if (sourceFormat == PixelFormats.BlackWhite)
            {
                Palette colors = Palette.BlackAndWhite(workingColorSpace, backColor32);
                return BitmapDataFactory.CreateBitmapData(buffer, size, stride, new CustomBitmapDataConfig
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
                return BitmapDataFactory.CreateBitmapData(buffer, size, stride, new CustomBitmapDataConfig
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
                return BitmapDataFactory.CreateBitmapData(buffer, size, stride, new CustomBitmapDataConfig
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
                return BitmapDataFactory.CreateBitmapData(buffer, size, stride, new CustomBitmapDataConfig
                {
                    PixelFormat = new PixelFormatInfo(32) { Prefers64BitColors = true },
                    BackColor = backColor.ToColor32(),
                    AlphaThreshold = alphaThreshold,
                    WorkingColorSpace = workingColorSpace,
                    BackBufferIndependentPixelAccess = true,
                    DisposeCallback = dispose,
                    RowGetColor32 = (row, x) => row.UnsafeGetRefAs<ColorBgr101010>(x).ToColor32(),
                    RowGetColor64 = (row, x) => row.UnsafeGetRefAs<ColorBgr101010>(x).ToColor64(),
                });

            if (sourceFormat == PixelFormats.Rgb48)
                return BitmapDataFactory.CreateBitmapData(buffer, size, stride, new CustomBitmapDataConfig
                {
                    PixelFormat = new PixelFormatInfo(48) { Prefers64BitColors = true },
                    BackColor = backColor.ToColor32(),
                    AlphaThreshold = alphaThreshold,
                    WorkingColorSpace = workingColorSpace,
                    BackBufferIndependentPixelAccess = true,
                    DisposeCallback = dispose,
                    RowGetColor64 = (row, x) => row.UnsafeGetRefAs<ColorRgb48>(x).ToColor64(),
                });

            if (sourceFormat == PixelFormats.Rgba64)
                return BitmapDataFactory.CreateBitmapData(buffer, size, stride, new CustomBitmapDataConfig
                {
                    PixelFormat = new PixelFormatInfo(64) { HasAlpha = true, Prefers64BitColors = true },
                    BackColor = backColor.ToColor32(),
                    AlphaThreshold = alphaThreshold,
                    WorkingColorSpace = workingColorSpace,
                    BackBufferIndependentPixelAccess = true,
                    DisposeCallback = dispose,
                    RowGetColor64 = (row, x) => row.UnsafeGetRefAs<ColorRgba64>(x).ToColor64(),
                });

            if (sourceFormat == PixelFormats.Prgba64)
                return BitmapDataFactory.CreateBitmapData(buffer, size, stride, new CustomBitmapDataConfig
                {
                    PixelFormat = new PixelFormatInfo(64) { HasPremultipliedAlpha = true, Prefers64BitColors = true },
                    BackColor = backColor.ToColor32(),
                    AlphaThreshold = alphaThreshold,
                    WorkingColorSpace = workingColorSpace,
                    BackBufferIndependentPixelAccess = true,
                    DisposeCallback = dispose,
                    RowGetPColor64 = (row, x) => row.UnsafeGetRefAs<ColorPrgba64>(x).ToPColor64(),
                });

            if (sourceFormat == PixelFormats.Rgb128Float)
                return BitmapDataFactory.CreateBitmapData(buffer, size, stride, new CustomBitmapDataConfig
                {
                    PixelFormat = new PixelFormatInfo(128) { LinearGamma = true, Prefers128BitColors = true },
                    BackColor = backColor.ToColor32(),
                    AlphaThreshold = alphaThreshold,
                    WorkingColorSpace = workingColorSpace,
                    BackBufferIndependentPixelAccess = true,
                    DisposeCallback = dispose,
                    RowGetColorF = (row, x) => row.UnsafeGetRefAs<ColorF>(x).ToOpaque(),
                });

            if (sourceFormat == PixelFormats.Cmyk32)
                return BitmapDataFactory.CreateBitmapData(buffer, size, stride, new CustomBitmapDataConfig
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

            throw new InvalidOperationException(Res.InternalError($"Unexpected PixelFormat {sourceFormat}"));
        }

        #endregion

        #region ConvertPixelFormat

        #region Sync

        /// <summary>
        /// Converts the specified <paramref name="bitmap"/> to a <see cref="WriteableBitmap"/> of the desired <see cref="PixelFormat"/>.
        /// </summary>
        /// <param name="bitmap">The original bitmap to convert.</param>
        /// <param name="newPixelFormat">The desired new pixel format.</param>
        /// <param name="backColor">If <paramref name="newPixelFormat"/> does not have alpha, then specifies the color of the background.
        /// Source pixels with alpha, which will be opaque in the result will be blended with this color.
        /// The <see cref="Color.A">Color.A</see> property of the background color is ignored. This parameter is optional.
        /// <br/>Default value: The bitwise zero instance of <see cref="Color"/>, which has the same RGB values as <see cref="Colors.Black"/>.</param>
        /// <param name="alphaThreshold">If <paramref name="newPixelFormat"/> is an indexed format and the target palette contains a transparent color,
        /// then specifies a threshold value for the <see cref="Color.A">Color.A</see> property, under which the color is considered transparent. If 0,
        /// then the result will not have transparent pixels. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <returns>A new <see cref="WriteableBitmap"/> instance with the desired pixel format.</returns>
        /// <remarks>
        /// <note><list type="bullet">
        /// <item>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. Use the <see cref="BeginConvertPixelFormat(BitmapSource, PixelFormat, Color, byte, AsyncConfig)"/>
        /// or <see cref="ConvertPixelFormatAsync(BitmapSource, PixelFormat, Color, byte, TaskConfig)"/> (in .NET Framework 4.0 and above) methods for asynchronous call and to adjust parallelization, set up cancellation and for reporting progress.</item>
        /// <item>If <paramref name="newPixelFormat"/> requires blending with <paramref name="backColor"/>, then this method selects the working color space automatically.
        /// To apply a specific color space use the <see cref="GetReadableBitmapData(BitmapSource, WorkingColorSpace, Color, byte)"/> on a <see cref="BitmapSource"/> instance,
        /// and then call the <see cref="ReadableBitmapDataExtensions.ToWriteableBitmap(IReadableBitmapData, PixelFormat, IQuantizer, IDitherer)">ToWriteableBitmap</see> extension method.</item>
        /// </list></note>
        /// <para>If <paramref name="newPixelFormat"/> is an indexed format, then this overload will either use the palette of the source <paramref name="bitmap"/> if applicable,
        /// or a default palette. To apply a custom palette use the of the <see cref="ConvertPixelFormat(BitmapSource,PixelFormat,Color[],Color,byte)"/> overload.</para>
        /// <para>If <paramref name="newPixelFormat"/> can represent fewer colors than the source format, then a default
        /// quantization will occur during the conversion. To use a specific quantizer (and optionally a ditherer) use the <see cref="ConvertPixelFormat(BitmapSource,PixelFormat,IQuantizer,IDitherer)"/> overload.
        /// To use a quantizer with a specific palette you can use the <see cref="PredefinedColorsQuantizer"/> class.</para>
        /// </remarks>
        /// <example>
        /// The following example demonstrates the possible results of this method compared to using WPF's <see cref="FormatConvertedBitmap"/> class:
        /// <code lang="C#"><![CDATA[
        /// public static BitmapSource Convert(BitmapSource source, PixelFormat targetPixelFormat, Color backColor, byte alphaThreshold)
        /// {
        ///     // a.) by KGy SOFT: can use a back color, handles alpha correctly, uses a default palette for indexed formats.
        ///     //     (to use specific quantizer or ditherer you can use an other overload)
        ///     return source.ConvertPixelFormat(targetPixelFormat, backColor, alphaThreshold);
        /// 
        ///     // b.) by WPF: no back color is used, alpha colors above the threshold suffer from color bleeding,
        ///     //     can use an optimized palette for indexed formats, a fixed dithering is forcibly used for <= 8 bpp formats
        ///     return new FormatConvertedBitmap(source, targetPixelFormat, GetDefaultPalette(), alphaThreshold / 255d * 100d);
        /// 
        ///     // Using the same colors for the WPF conversion as KGy SOFT conversion uses.
        ///     BitmapPalette? GetDefaultPalette() =>
        ///         source.Palette != null && source.Format.BitsPerPixel <= targetPixelFormat.BitsPerPixel ? source.Palette
        ///         : targetPixelFormat == PixelFormats.Indexed1 ? ToBitmapPalette(Palette.SystemDefault1BppPalette())
        ///         : targetPixelFormat == PixelFormats.Indexed2 ? new BitmapPalette(new[] { Colors.Black, Colors.Gray, Colors.Silver, Colors.White })
        ///         : targetPixelFormat == PixelFormats.Indexed4 ? ToBitmapPalette(Palette.SystemDefault4BppPalette())
        ///         : targetPixelFormat == PixelFormats.Indexed8 ? ToBitmapPalette(Palette.SystemDefault8BppPalette())
        ///         : null;
        /// 
        ///     static BitmapPalette ToBitmapPalette(Palette palette)
        ///         => new BitmapPalette(palette.GetEntries().Select(c => Color.FromArgb(c.A, c.R, c.G, c.B)).ToList());
        /// }]]></code>
        /// <table class="table is-hoverable">
        /// <thead><tr><th width="50%"><div style="text-align:center;">Original image</div></th><th width="50%"><div style="text-align:center;">Converted image</div></th></tr></thead>
        /// <tbody>
        /// <tr><td><div style="text-align:center;">
        /// <para><img src="../Help/Images/AlphaGradient.png" alt="Color hues with alpha gradient"/>
        /// <br/>Color hues with alpha gradient</para></div></td>
        /// <td><div style="text-align:center;">
        /// <para><img src="../Help/Images/AlphaGradientDefault8bppWhiteA16.png" alt="Alpha gradient converted to indexed 8 bit format by KGy SOFT conversion using default palette, white background, alpha threshold is 16"/>
        /// <br/>Using <see cref="ConvertPixelFormat(BitmapSource, PixelFormat, Color, byte)">ConvertPixelFormat</see> with <see cref="PixelFormats.Indexed8"/> format, white background, alpha threshold = 16.
        /// This overload does not use dithering, the bottom 16 rows are transparent, the alpha pixels above were blended with white.</para>
        /// <para><img src="../Help/Images/AlphaGradientDefault8bppA16_WPF.png" alt="Alpha gradient converted to indexed 8 bit format by FormatConvertedBitmap"/>
        /// <br/>Using WPF's <see cref="FormatConvertedBitmap"/> with the same parameters as above. The result is forcibly dithered and the alpha pixels above the threshold
        /// were not blended with any back color so the vertical gradient has been just disappeared.</para></div></td>
        /// </tr>
        /// <tr><td><div style="text-align:center;">
        /// <para><img src="../Help/Images/Shield256.png" alt="Shield icon with transparent background"/>
        /// <br/>Shield icon with transparency</para></div></td>
        /// <td><div style="text-align:center;">
        /// <para><img src="../Help/Images/ShieldRgb888Silver.png" alt="Shield icon with silver background"/>
        /// <br/>Using <see cref="ConvertPixelFormat(BitmapSource, PixelFormat, Color, byte)">ConvertPixelFormat</see> with <see cref="PixelFormats.Rgb24"/> format, silver background.
        /// The alpha pixels were blended with the silver color (alpha threshold is ignored because this format does not support alpha).</para>
        /// <para><img src="../Help/Images/ShieldRgb24_WPF.png" alt="Shield icon converted to RGB24 format by FormatConvertedBitmap"/>
        /// <br/>Using WPF's <see cref="FormatConvertedBitmap"/> with the same parameters as above. The alpha pixels were just turned opaque
        /// without blending them with any color. Some light pixels appeared where RGB values of the alpha pixels were not completely black.</para></div></td>
        /// </tr>
        /// </tbody></table>
        /// </example>
        /// <exception cref="ArgumentNullException"><paramref name="bitmap"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="newPixelFormat"/> does not specify a valid format.</exception>
        /// <exception cref="InvalidOperationException">A deadlock has been detected while attempting to create the result.</exception>
        /// <seealso cref="ConvertPixelFormat(BitmapSource, PixelFormat, IQuantizer, IDitherer)"/>
        /// <seealso cref="BitmapDataExtensions.Clone(IReadableBitmapData, KnownPixelFormat, Color32, byte)"/>
        public static WriteableBitmap ConvertPixelFormat(this BitmapSource bitmap, PixelFormat newPixelFormat, Color backColor = default, byte alphaThreshold = 128)
            => ConvertPixelFormat(bitmap, newPixelFormat, null, backColor, alphaThreshold);

        /// <summary>
        /// Converts the specified <paramref name="bitmap"/> to a <see cref="WriteableBitmap"/> of the desired <see cref="PixelFormat"/>.
        /// </summary>
        /// <param name="bitmap">The original bitmap to convert.</param>
        /// <param name="newPixelFormat">The desired new pixel format.</param>
        /// <param name="palette">The desired target palette if <paramref name="newPixelFormat"/> is an indexed format. If <see langword="null"/>,
        /// then the source palette is taken from the source image if it also has a palette of no more entries than the target indexed format can have;
        /// otherwise, a default palette will be used based on <paramref name="newPixelFormat"/>.</param>
        /// <param name="backColor">If <paramref name="newPixelFormat"/> does not support alpha, then specifies the color of the background.
        /// Source pixels with alpha, which will be opaque in the result will be blended with this color.
        /// The <see cref="Color.A">Color.A</see> property of the background color is ignored. This parameter is optional.
        /// <br/>Default value: The bitwise zero instance of <see cref="Color"/>, which has the same RGB values as <see cref="Colors.Black"/>.</param>
        /// <param name="alphaThreshold">If <paramref name="newPixelFormat"/> is an indexed format and the target palette contains a transparent color,
        /// then specifies a threshold value for the <see cref="Color.A">Color.A</see> property, under which the color is considered transparent. If 0,
        /// then the result will not have transparent pixels. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <returns>A new <see cref="WriteableBitmap"/> instance with the desired pixel format.</returns>
        /// <remarks>
        /// <note><list type="bullet">
        /// <item>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. Use the <see cref="BeginConvertPixelFormat(BitmapSource, PixelFormat, Color[], Color, byte, AsyncConfig)"/>
        /// or <see cref="ConvertPixelFormatAsync(BitmapSource, PixelFormat, Color[], Color, byte, TaskConfig)"/> (in .NET Framework 4.0 and above) methods for asynchronous call and to adjust parallelization, set up cancellation and for reporting progress.</item>
        /// <item>If <paramref name="newPixelFormat"/> requires blending with <paramref name="backColor"/>, then this method selects the working color space automatically.
        /// To apply a specific color space use the <see cref="GetReadableBitmapData(BitmapSource, WorkingColorSpace, Color, byte)"/> on a <see cref="BitmapSource"/> instance,
        /// and then call the <see cref="ReadableBitmapDataExtensions.ToWriteableBitmap(IReadableBitmapData, PixelFormat, IQuantizer, IDitherer)">ToWriteableBitmap</see> extension method.</item>
        /// </list></note>
        /// <para>If <paramref name="newPixelFormat"/> can represent fewer colors than the source format, then a default
        /// quantization will occur during the conversion. To use a specific quantizer (and optionally a ditherer) use the <see cref="ConvertPixelFormat(BitmapSource,PixelFormat,IQuantizer,IDitherer)"/> overload.
        /// To use a quantizer with a specific palette you can use the <see cref="PredefinedColorsQuantizer"/> class.</para>
        /// <para>If <paramref name="newPixelFormat"/> is <see cref="PixelFormats.Indexed8"/>, <paramref name="bitmap"/> has no palette and <paramref name="palette"/> is <see langword="null"/>, then a default 256 color palette will be used containing
        /// the 16 standard <a href="https://www.w3.org/TR/REC-html40/types.html#h-6.5" target="_blank">basic sRGB colors</a>, the "web-safe" palette of 216 colors as well as 24 transparent entries.</para>
        /// <para>If <paramref name="newPixelFormat"/> is <see cref="PixelFormats.Indexed4"/>, <paramref name="bitmap"/> has no palette and <paramref name="palette"/> is <see langword="null"/>,
        /// then the 16 standard <a href="https://www.w3.org/TR/REC-html40/types.html#h-6.5" target="_blank">basic sRGB colors</a> will be used.</para>
        /// <para>If <paramref name="newPixelFormat"/> is <see cref="PixelFormats.Indexed2"/>, <paramref name="bitmap"/> has no palette and <paramref name="palette"/> is <see langword="null"/>, then
        /// the palette will consist of 4 grayscale colors, containing black, white and the two gray entries that present in the default 4-bit palette.</para>
        /// <para>If <paramref name="newPixelFormat"/> is <see cref="PixelFormats.Indexed1"/>, <paramref name="bitmap"/> has no palette and <paramref name="palette"/> is <see langword="null"/>, then black and white colors will be used.</para>
        /// <note type="tip">See the <strong>Examples</strong> section of the <see cref="ConvertPixelFormat(BitmapSource, PixelFormat, Color, byte)"/>
        /// and <see cref="ConvertPixelFormat(BitmapSource, PixelFormat, IQuantizer?, IDitherer?)"/> overloads for image examples.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmap"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="newPixelFormat"/> does not specify a valid format.</exception>
        /// <exception cref="ArgumentException"><paramref name="palette"/> contains too many colors for the indexed format specified by <paramref name="newPixelFormat"/>.</exception>
        /// <exception cref="InvalidOperationException">A deadlock has been detected while attempting to create the result.</exception>
        /// <seealso cref="ConvertPixelFormat(BitmapSource, PixelFormat, IQuantizer, IDitherer)"/>
        /// <seealso cref="BitmapDataExtensions.Clone(IReadableBitmapData, KnownPixelFormat, Palette)"/>
        public static WriteableBitmap ConvertPixelFormat(this BitmapSource bitmap, PixelFormat newPixelFormat, Color[]? palette, Color backColor = default, byte alphaThreshold = 128)
        {
            ValidateConvertPixelFormat(bitmap, newPixelFormat);
            return DoConvertPixelFormatDirect(AsyncHelper.DefaultContext, new ConversionContext(bitmap, newPixelFormat, palette, backColor, alphaThreshold))!;
        }

        /// <summary>
        /// Converts the specified <paramref name="bitmap"/> to a <see cref="WriteableBitmap"/> with the desired <see cref="PixelFormat"/>.
        /// </summary>
        /// <param name="bitmap">The original bitmap to convert.</param>
        /// <param name="newPixelFormat">The desired new pixel format.</param>
        /// <param name="quantizer">An optional <see cref="IQuantizer"/> instance to determine the colors of the result.
        /// If <see langword="null"/> and <paramref name="newPixelFormat"/> is an indexed format, then a default palette and quantization logic will be used.</param>
        /// <param name="ditherer">The ditherer to be used. Might be ignored if <paramref name="quantizer"/> is not specified
        /// and <paramref name="newPixelFormat"/> represents an at least 24 bits-per-pixel size. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A new <see cref="WriteableBitmap"/> instance with the desired pixel format.</returns>
        /// <remarks>
        /// <note><list type="bullet">
        /// <item>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. Use the <see cref="BeginConvertPixelFormat(BitmapSource, PixelFormat, IQuantizer, IDitherer, AsyncConfig?)"/>
        /// or <see cref="ConvertPixelFormatAsync(BitmapSource, PixelFormat, IQuantizer?, IDitherer, TaskConfig)"/> (in .NET Framework 4.0 and above) methods for asynchronous call and to adjust parallelization, set up cancellation and for reporting progress.</item>
        /// <item>If <paramref name="quantizer"/> is <see langword="null"/> and <paramref name="newPixelFormat"/> requires blending, then this method selects the working color space automatically.
        /// To apply a specific color space use the <see cref="GetReadableBitmapData(BitmapSource, WorkingColorSpace, Color, byte)"/> on a <see cref="BitmapSource"/> instance,
        /// and then call the <see cref="ReadableBitmapDataExtensions.ToWriteableBitmap(IReadableBitmapData, PixelFormat, IQuantizer, IDitherer)">ToWriteableBitmap</see> extension method.</item>
        /// </list></note>
        /// <para>An unmatching <paramref name="quantizer"/> and <paramref name="newPixelFormat"/> may cause undesired results.</para>
        /// <para>The <paramref name="ditherer"/> may have no effect if the <paramref name="quantizer"/> uses too many colors.</para>
        /// <para>To produce a result with up to 256 colors best optimized for the source <paramref name="bitmap"/> you can use the <see cref="OptimizedPaletteQuantizer"/> class.</para>
        /// <para>To quantize a <see cref="WriteableBitmap"/> in place, without changing the pixel format you can use the <see cref="BitmapDataExtensions.Quantize(IReadWriteBitmapData, IQuantizer)">BitmapDataExtensions.Quantize</see> method.
        /// You can use the <see cref="WriteableBitmapExtensions.GetReadWriteBitmapData(WriteableBitmap, WorkingColorSpace, Color, byte)">GetReadWriteBitmapData</see> extension method to obtain an <see cref="IReadWriteBitmapData"/> for a <see cref="WriteableBitmap"/>.</para>
        /// <para>To dither a <see cref="WriteableBitmap"/> in place, without changing the pixel format you can use the <see cref="BitmapDataExtensions.Dither(IReadWriteBitmapData, IQuantizer, IDitherer)">BitmapDataExtensions.Dither</see> method.
        /// You can use the <see cref="WriteableBitmapExtensions.GetReadWriteBitmapData(WriteableBitmap, WorkingColorSpace, Color, byte)">GetReadWriteBitmapData</see> extension method to obtain an <see cref="IReadWriteBitmapData"/> for a <see cref="WriteableBitmap"/>.</para>
        /// </remarks>
        /// <example>
        /// The following example demonstrates the possible results of this method compared to using WPF's <see cref="FormatConvertedBitmap"/> class:
        /// <code lang="C#"><![CDATA[
        /// public static BitmapSource Convert(BitmapSource source, PixelFormat targetPixelFormat, IQuantizer quantizer, IDitherer ditherer)
        /// {
        ///     // a.) by KGy SOFT: can use a specific quantizer and ditherer.
        ///     //     Back color and alpha threshold is specified by the quantizer
        ///     return source.ConvertPixelFormat(targetPixelFormat, quantizer, ditherer);
        ///
        ///     // b.) by WPF: If no palette is specified, then optimizes colors for indexed images.
        ///     //     No back color is used, alpha colors above the threshold suffer from color bleeding.
        ///     //     A fixed ditherer is always applied to <= 8 bpp formats (but not for Bgr555, for example)
        ///     GetQuantizerData(out BitmapPalette? palette, out double alphaThreshold);
        ///     return new FormatConvertedBitmap(source, targetPixelFormat, palette, alphaThreshold);
        ///
        ///     // Extracting the possible palette and alpha threshold for the WPF conversion from the quantizer
        ///     void GetQuantizerData(out BitmapPalette? palette, out double alphaThreshold)
        ///     {
        ///         using IReadableBitmapData bitmapData = source.GetReadableBitmapData();
        ///         using IQuantizingSession session = quantizer.Initialize(bitmapData); // can be slow for OptimizedPaletteQuantizer
        ///         IList<Color>? colors = session.Palette?.GetEntries().Select(c => Color.FromArgb(c.A, c.R, c.G, c.B)).ToList();
        ///         palette = colors == null ? null : new BitmapPalette(colors);
        ///         alphaThreshold = session.AlphaThreshold / 255d * 100d;
        ///     }
        /// }]]></code>
        /// <table class="table is-hoverable">
        /// <thead><tr><th width="50%"><div style="text-align:center;">Original image</div></th><th width="50%"><div style="text-align:center;">Converted image</div></th></tr></thead>
        /// <tbody>
        /// <tr><td><div style="text-align:center;">
        /// <para><img src="../Help/Images/AlphaGradient.png" alt="Color hues with alpha gradient"/>
        /// <br/>Color hues with alpha gradient</para></div></td>
        /// <td><div style="text-align:center;">
        /// <para><img src="../Help/Images/AlphaGradientDefault8bppWhiteA16DitheredFS.png" alt="Alpha gradient converted to indexed 8 bit format by KGy SOFT conversion using default palette, white background and Floyd-Steinberg dithering. Alpha threshold is 16."/>
        /// <br/>Using <see cref="ConvertPixelFormat(BitmapSource, PixelFormat, IQuantizer?, IDitherer?)">ConvertPixelFormat</see> with <see cref="PixelFormats.Indexed8"/>
        /// format, <see cref="PredefinedColorsQuantizer.SystemDefault8BppPalette(Color32,byte)">SystemDefault8BppPalette</see> quantizer (white background, alpha threshold = 16)
        /// and <see cref="ErrorDiffusionDitherer.FloydSteinberg">Floyd-Steinberg</see> dithering. The bottom 16 rows are transparent, the alpha pixels above were blended with white.</para>
        /// <para><img src="../Help/Images/AlphaGradientDefault8bppA16_WPF.png" alt="Alpha gradient converted to indexed 8 bit format by FormatConvertedBitmap"/>
        /// <br/>Using WPF's <see cref="FormatConvertedBitmap"/> with the same parameters as above. The result is forcibly dithered and the alpha pixels above the threshold
        /// were not blended with any back color so the vertical gradient has been just disappeared.</para></div></td>
        /// </tr>
        /// <tr><td><div style="text-align:center;">
        /// <para><img src="../Help/Images/Shield256.png" alt="Shield icon with transparent background"/>
        /// <br/>Shield icon with transparency</para></div></td>
        /// <td><div style="text-align:center;">
        /// <para><img src="../Help/Images/ShieldBgr555BlackDitheredFS.png" alt="Shield icon converted to BGR555 format with black background and Floyd-Steinber dithering"/>
        /// <br/>Using <see cref="ConvertPixelFormat(BitmapSource, PixelFormat, IQuantizer?, IDitherer?)">ConvertPixelFormat</see> with <see cref="PixelFormats.Bgr555"/>
        /// format, <see cref="PredefinedColorsQuantizer.Rgb555(Color32,byte)">Rgb555</see> quantizer with default parameters (so the background is black)
        /// and <see cref="ErrorDiffusionDitherer.FloydSteinberg">Floyd-Steinberg</see> dithering.</para>
        /// <para><img src="../Help/Images/ShieldBgr555_WPF.png" alt="Shield icon converted to BGR555 format by FormatConvertedBitmap"/>
        /// <br/>Using WPF's <see cref="FormatConvertedBitmap"/> with <see cref="PixelFormats.Bgr555"/> format. The alpha pixels were just turned opaque
        /// without blending them with any color so some light pixels appeared where RGB values of the alpha pixels were not completely black.
        /// As <see cref="FormatConvertedBitmap"/> does not use dithering for this pixel format, the result has a quite noticeable banding.</para></div></td>
        /// </tr>
        /// <tr><td><div style="text-align:center;">
        /// <para><img src="../Help/Images/Information256.png" alt="Information icon with transparent background"/>
        /// <br/>Information icon with transparency</para></div></td>
        /// <td><div style="text-align:center;">
        /// <para><img src="../Help/Images/InformationWu4SilverA16DitheredB8.png" alt="Information icon converted to Indexed2 format with Wu quantizer using silver background and Bayer 8x8 dithering"/>
        /// <br/>Using <see cref="ConvertPixelFormat(BitmapSource, PixelFormat, IQuantizer?, IDitherer?)">ConvertPixelFormat</see> with <see cref="PixelFormats.Indexed2"/>
        /// format, <see cref="OptimizedPaletteQuantizer.Wu(int,Color32,byte)">Wu</see> quantizer (with 4 colors, silver background, alpha threshold = 16)
        /// and <see cref="OrderedDitherer.Bayer8x8">Bayer 8x8</see> dithering.</para>
        /// <para><img src="../Help/Images/Information4A16_WPF.png" alt="Information icon converted to Indexed2 format by FormatConvertedBitmap"/>
        /// <br/>Using WPF's <see cref="FormatConvertedBitmap"/> with <see cref="PixelFormats.Indexed2"/> format without specifying a palette so it was optimized by <see cref="FormatConvertedBitmap"/>.
        /// The alpha pixels above the threshold were not blended by any back color so the black shadow just consists of the original pixels after removing alpha. A default dithering was automatically applied.</para></div></td>
        /// </tr>
        /// </tbody></table>
        /// </example>
        /// <exception cref="ArgumentNullException"><paramref name="bitmap"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="newPixelFormat"/> does not specify a valid format.</exception>
        /// <exception cref="ArgumentException">The <paramref name="quantizer"/> palette contains too many colors for the indexed format specified by <paramref name="newPixelFormat"/>.</exception>
        /// <exception cref="InvalidOperationException">A deadlock has been detected while attempting to create the result.</exception>
        /// <seealso cref="IQuantizer"/>
        /// <seealso cref="IDitherer"/>
        /// <seealso cref="BitmapDataExtensions.Quantize(IReadWriteBitmapData, IQuantizer)"/>
        /// <seealso cref="BitmapDataExtensions.Dither(IReadWriteBitmapData, IQuantizer, IDitherer)"/>
        /// <seealso cref="BitmapDataExtensions.Clone(IReadableBitmapData, KnownPixelFormat, IQuantizer, IDitherer)"/>
        public static WriteableBitmap ConvertPixelFormat(this BitmapSource bitmap, PixelFormat newPixelFormat, IQuantizer? quantizer, IDitherer? ditherer = null)
        {
            ValidateConvertPixelFormat(bitmap, newPixelFormat);
            var context = new ConversionContext(bitmap, newPixelFormat, quantizer, ditherer);
            return context.Quantizer == null
                ? DoConvertPixelFormatDirect(AsyncHelper.DefaultContext, context)!
                : DoConvertPixelFormatWithQuantizer(AsyncHelper.DefaultContext, context)!;
        }

        #endregion

        #region Async APM

        /// <summary>
        /// Begins to convert the specified <paramref name="bitmap"/> to a <see cref="WriteableBitmap"/> of the desired <see cref="PixelFormat"/> asynchronously.
        /// </summary>
        /// <param name="bitmap">The original image to convert.</param>
        /// <param name="newPixelFormat">The desired new pixel format.</param>
        /// <param name="backColor">If <paramref name="newPixelFormat"/> does not support alpha or supports only single-bit alpha, then specifies the color of the background.
        /// Source pixels with alpha, which will be opaque in the result will be blended with this color.
        /// The <see cref="Color.A">Color.A</see> property of the background color is ignored. This parameter is optional.
        /// <br/>Default value: The bitwise zero instance of <see cref="Color"/>, which has the same RGB values as <see cref="Colors.Black"/>.</param>
        /// <param name="alphaThreshold">If <paramref name="newPixelFormat"/> is an indexed format and the target palette contains a transparent color,
        /// then specifies a threshold value for the <see cref="Color.A">Color.A</see> property, under which the color is considered transparent. If 0,
        /// then the result will not have transparent pixels. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="ConvertPixelFormatAsync(BitmapSource, PixelFormat, Color, byte, TaskConfig)"/> method.</para>
        /// <para>To get the result or the exception that occurred during the operation you have to call the <see cref="EndConvertPixelFormat">EndConvertPixelFormat</see> method.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="caution">This method can be called from any thread but if it is called from a different one from the thread of the source <paramref name="bitmap"/>'s dispatcher,
        /// then the result <see cref="WriteableBitmap"/> will be created by using a synchronized callback. To avoid possible deadlocks, the thread of the source <paramref name="bitmap"/>
        /// must not be blocked and the dispatcher must run. The result will be usable in the same thread as the source <paramref name="bitmap"/>.
        /// To avoid blocking waiting for the result set the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfig_CompletedCallback.htm">CompletedCallback</a>
        /// parameter of the <paramref name="asyncConfig"/> parameter and call the <see cref="EndConvertPixelFormat">EndConvertPixelFormat</see> method from there.</note>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="ConvertPixelFormat(BitmapSource, PixelFormat, Color, byte)"/> method for more details and image examples.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmap"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="newPixelFormat"/> does not specify a valid format.</exception>
        public static IAsyncResult BeginConvertPixelFormat(this BitmapSource bitmap, PixelFormat newPixelFormat, Color backColor = default, byte alphaThreshold = 128, AsyncConfig? asyncConfig = null)
            => BeginConvertPixelFormat(bitmap, newPixelFormat, null, backColor, alphaThreshold, asyncConfig);

        /// <summary>
        /// Begins to convert the specified <paramref name="bitmap"/> to a <see cref="WriteableBitmap"/> of the desired <see cref="PixelFormat"/> asynchronously.
        /// </summary>
        /// <param name="bitmap">The original image to convert.</param>
        /// <param name="newPixelFormat">The desired new pixel format.</param>
        /// <param name="palette">The desired target palette if <paramref name="newPixelFormat"/> is an indexed format. If <see langword="null"/>,
        /// then the source palette is taken from the source image if it also has a palette of no more entries than the target indexed format can have;
        /// otherwise, a default palette will be used based on <paramref name="newPixelFormat"/>.</param>
        /// <param name="backColor">If <paramref name="newPixelFormat"/> does not support alpha or supports only single-bit alpha, then specifies the color of the background.
        /// Source pixels with alpha, which will be opaque in the result will be blended with this color.
        /// The <see cref="Color.A">Color.A</see> property of the background color is ignored. This parameter is optional.
        /// <br/>Default value: The bitwise zero instance of <see cref="Color"/>, which has the same RGB values as <see cref="Colors.Black"/>.</param>
        /// <param name="alphaThreshold">If <paramref name="newPixelFormat"/> is an indexed format and the target palette contains a transparent color,
        /// then specifies a threshold value for the <see cref="Color.A">Color.A</see> property, under which the color is considered transparent. If 0,
        /// then the result will not have transparent pixels. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="ConvertPixelFormatAsync(BitmapSource, PixelFormat, Color[], Color, byte, TaskConfig)"/> method.</para>
        /// <para>To get the result or the exception that occurred during the operation you have to call the <see cref="EndConvertPixelFormat">EndConvertPixelFormat</see> method.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="caution">This method can be called from any thread but if it is called from a different one from the thread of the source <paramref name="bitmap"/>'s dispatcher,
        /// then the result <see cref="WriteableBitmap"/> will be created by using a synchronized callback. To avoid possible deadlocks, the thread of the source <paramref name="bitmap"/>
        /// must not be blocked and the dispatcher must run. The result will be usable in the same thread as the source <paramref name="bitmap"/>.
        /// To avoid blocking waiting for the result set the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfig_CompletedCallback.htm">CompletedCallback</a>
        /// parameter of the <paramref name="asyncConfig"/> parameter and call the <see cref="EndConvertPixelFormat">EndConvertPixelFormat</see> method from there.</note>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="ConvertPixelFormat(BitmapSource, PixelFormat, Color[], Color, byte)"/> method for more details, or the other overloads for image examples.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmap"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="newPixelFormat"/> does not specify a valid format.</exception>
        /// <exception cref="ArgumentException">The <paramref name="palette"/> contains too many colors for the indexed format specified by <paramref name="newPixelFormat"/>.</exception>
        public static IAsyncResult BeginConvertPixelFormat(this BitmapSource bitmap, PixelFormat newPixelFormat, Color[]? palette, Color backColor = default, byte alphaThreshold = 128, AsyncConfig? asyncConfig = null)
        {
            ValidateConvertPixelFormat(bitmap, newPixelFormat);
            var context = new ConversionContext(bitmap, newPixelFormat, palette, backColor, alphaThreshold);
            return AsyncHelper.BeginOperation(ctx => DoConvertPixelFormatDirect(ctx, context), asyncConfig);
        }

        /// <summary>
        /// Begins to convert the specified <paramref name="bitmap"/> to a <see cref="WriteableBitmap"/> of the desired <see cref="PixelFormat"/> asynchronously.
        /// </summary>
        /// <param name="bitmap">The original image to convert.</param>
        /// <param name="newPixelFormat">The desired new pixel format.</param>
        /// <param name="quantizer">An optional <see cref="IQuantizer"/> instance to determine the colors of the result.
        /// If <see langword="null"/> and <paramref name="newPixelFormat"/> is an indexed format, then a default palette and quantization logic will be used.</param>
        /// <param name="ditherer">The ditherer to be used. Might be ignored if <paramref name="quantizer"/> is not specified
        /// and <paramref name="newPixelFormat"/> represents an at least 24 bits-per-pixel size. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="ConvertPixelFormatAsync(BitmapSource, PixelFormat, IQuantizer, IDitherer, TaskConfig)"/> method.</para>
        /// <para>To get the result or the exception that occurred during the operation you have to call the <see cref="EndConvertPixelFormat">EndConvertPixelFormat</see> method.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="caution"><list type="bullet"><item>This method can be called from any thread but if it is called from a different one from the thread of the source <paramref name="bitmap"/>'s dispatcher,
        /// then the result <see cref="WriteableBitmap"/> will be created by using a synchronized callback. To avoid possible deadlocks, the thread of the source <paramref name="bitmap"/>
        /// must not be blocked and the dispatcher must run. The result will be usable in the same thread as the source <paramref name="bitmap"/>.
        /// To avoid blocking waiting for the result set the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfig_CompletedCallback.htm">CompletedCallback</a>
        /// parameter of the <paramref name="asyncConfig"/> parameter and call the <see cref="EndConvertPixelFormat">EndConvertPixelFormat</see> method from there.</item>
        /// <item>If <paramref name="quantizer"/> is not a <see cref="PredefinedColorsQuantizer"/>, then the result <see cref="WriteableBitmap"/> is created by a synchronized callback
        /// on the thread of the source <paramref name="bitmap"/> even if you call this method from the same thread. Do not block the thread of the source <paramref name="bitmap"/>;
        /// otherwise, a deadlock may occur.</item></list></note>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="ConvertPixelFormat(BitmapSource, PixelFormat, IQuantizer, IDitherer)"/> method for more details and image examples.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmap"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="newPixelFormat"/> does not specify a valid format.</exception>
        /// <exception cref="ArgumentException">The <paramref name="quantizer"/> palette contains too many colors for the indexed format specified by <paramref name="newPixelFormat"/>.</exception>
        public static IAsyncResult BeginConvertPixelFormat(this BitmapSource bitmap, PixelFormat newPixelFormat, IQuantizer? quantizer, IDitherer? ditherer = null, AsyncConfig? asyncConfig = null)
        {
            ValidateConvertPixelFormat(bitmap, newPixelFormat);
            var context = new ConversionContext(bitmap, newPixelFormat, quantizer, ditherer);
            return context.Quantizer == null
                ? AsyncHelper.BeginOperation(ctx => DoConvertPixelFormatDirect(ctx, context), asyncConfig)
                : AsyncHelper.BeginOperation(ctx => DoConvertPixelFormatWithQuantizer(ctx, context), asyncConfig);
        }

        /// <summary>
        /// Waits for the pending asynchronous operation started by the <see cref="O:KGySoft.Drawing.Wpf.BitmapSourceExtensions.BeginConvertPixelFormat">BeginConvertPixelFormat</see> methods to complete.
        /// In .NET Framework 4.0 and above you can use the <see cref="O:KGySoft.Drawing.Wpf.BitmapSourceExtensions.ConvertPixelFormatAsync">ConvertPixelFormatAsync</see> methods instead.
        /// To avoid blocking the source thread it is recommended to call this method from the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfig_CompletedCallback.htm">CompletedCallback</a>
        /// delegate of the <c>asyncConfig</c> parameter of the <see cref="O:KGySoft.Drawing.Wpf.BitmapSourceExtensions.BeginConvertPixelFormat">BeginConvertPixelFormat</see> methods.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="BeginConvertPixelFormat(BitmapSource, PixelFormat, IQuantizer?, IDitherer?, AsyncConfig?)"/> method for details.
        /// </summary>
        /// <param name="asyncResult">The reference to the pending asynchronous request to finish.</param>
        /// <returns>A <see cref="WriteableBitmap"/> instance that is the result of the operation,
        /// or <see langword="null"/>, if the operation was canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a>property of the <c>asyncConfig</c> parameter was <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="asyncResult"/> is <see langword="null"/>, or was not returned by a <see cref="O:KGySoft.Drawing.Wpf.BitmapSourceExtensions.BeginConvertPixelFormat">BeginConvertPixelFormat</see> overload.</exception>
        /// <exception cref="InvalidOperationException">A deadlock has been detected while attempting to create the result.</exception>
        public static WriteableBitmap? EndConvertPixelFormat(this IAsyncResult asyncResult) => AsyncHelper.EndOperation<WriteableBitmap?>(asyncResult, nameof(BeginConvertPixelFormat));

        #endregion

        #region Async TAP
#if !NET35

        /// <summary>
        /// Converts the specified <paramref name="bitmap"/> to a <see cref="WriteableBitmap"/> of the desired <see cref="PixelFormat"/> asynchronously.
        /// </summary>
        /// <param name="bitmap">The original bitmap to convert.</param>
        /// <param name="newPixelFormat">The desired new pixel format.</param>
        /// <param name="backColor">If <paramref name="newPixelFormat"/> does not support alpha or supports only single-bit alpha, then specifies the color of the background.
        /// Source pixels with alpha, which will be opaque in the result will be blended with this color.
        /// The <see cref="Color.A">Color.A</see> property of the background color is ignored. This parameter is optional.
        /// <br/>Default value: The bitwise zero instance of <see cref="Color"/>, which has the same RGB values as <see cref="Colors.Black"/>.</param>
        /// <param name="alphaThreshold">If <paramref name="newPixelFormat"/> can represent only single-bit alpha or <paramref name="newPixelFormat"/> is an indexed format and the target palette contains a transparent color,
        /// then specifies a threshold value for the <see cref="Color.A">Color.A</see> property, under which the color is considered transparent. If 0,
        /// then the result will not have transparent pixels. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. Its result is the new <see cref="WriteableBitmap"/> instance with the desired pixel format,
        /// or <see langword="null"/>, if the operation was canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a>property of the <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>Alternatively, you can also use the <see cref="BeginConvertPixelFormat(BitmapSource, PixelFormat, Color, byte, AsyncConfig)"/> method, which is available on every platform.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="caution">This method can be called from any thread but if it is called from a different one from the thread of the source <paramref name="bitmap"/>'s dispatcher,
        /// then the result <see cref="WriteableBitmap"/> will be created by using a synchronized callback. To avoid possible deadlocks, the thread of the source <paramref name="bitmap"/>
        /// must not be blocked and the dispatcher must run. The result will be usable in the same thread as the source <paramref name="bitmap"/>.
        /// To avoid blocking waiting for the result use the <see langword="await"/> keyword in C# (when using .NET Framework 4.5 or newer),
        /// or use the <see cref="Task{TResult}.ContinueWith(Action{Task{TResult}})">Task.ContinueWith</see> method to access
        /// the <see cref="Task{TResult}.Result">Result</see> of the completed task from there.</note>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="ConvertPixelFormat(BitmapSource, PixelFormat, Color, byte)"/> method for more details and image examples.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmap"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="newPixelFormat"/> does not specify a valid format.</exception>
        public static Task<WriteableBitmap?> ConvertPixelFormatAsync(this BitmapSource bitmap, PixelFormat newPixelFormat, Color backColor = default, byte alphaThreshold = 128, TaskConfig? asyncConfig = null)
            => ConvertPixelFormatAsync(bitmap, newPixelFormat, null, backColor, alphaThreshold, asyncConfig);

        /// <summary>
        /// Converts the specified <paramref name="bitmap"/> to a <see cref="WriteableBitmap"/> of the desired <see cref="PixelFormat"/> asynchronously.
        /// </summary>
        /// <param name="bitmap">The original bitmap to convert.</param>
        /// <param name="newPixelFormat">The desired new pixel format.</param>
        /// <param name="palette">The desired target palette if <paramref name="newPixelFormat"/> is an indexed format. If <see langword="null"/>,
        /// then the source palette is taken from the source image if it also has a palette of no more entries than the target indexed format can have;
        /// otherwise, a default palette will be used based on <paramref name="newPixelFormat"/>.</param>
        /// <param name="backColor">If <paramref name="newPixelFormat"/> does not support alpha or supports only single-bit alpha, then specifies the color of the background.
        /// Source pixels with alpha, which will be opaque in the result will be blended with this color.
        /// The <see cref="Color.A">Color.A</see> property of the background color is ignored. This parameter is optional.
        /// <br/>Default value: The bitwise zero instance of <see cref="Color"/>, which has the same RGB values as <see cref="Colors.Black"/>.</param>
        /// <param name="alphaThreshold">If <paramref name="newPixelFormat"/> can represent only single-bit alpha or <paramref name="newPixelFormat"/> is an indexed format and the target palette contains a transparent color,
        /// then specifies a threshold value for the <see cref="Color.A">Color.A</see> property, under which the color is considered transparent. If 0,
        /// then the result will not have transparent pixels. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. Its result is the new <see cref="WriteableBitmap"/> instance with the desired pixel format,
        /// or <see langword="null"/>, if the operation was canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a>property of the <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>Alternatively, you can also use the <see cref="BeginConvertPixelFormat(BitmapSource, PixelFormat, Color, byte, AsyncConfig)"/> method, which is available on every platform.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="caution">This method can be called from any thread but if it is called from a different one from the thread of the source <paramref name="bitmap"/>'s dispatcher,
        /// then the result <see cref="WriteableBitmap"/> will be created by using a synchronized callback. To avoid possible deadlocks, the thread of the source <paramref name="bitmap"/>
        /// must not be blocked and the dispatcher must run. The result will be usable in the same thread as the source <paramref name="bitmap"/>.
        /// To avoid blocking waiting for the result use the <see langword="await"/> keyword in C# (when using .NET Framework 4.5 or newer),
        /// or use the <see cref="Task{TResult}.ContinueWith(Action{Task{TResult}})">Task.ContinueWith</see> method to access
        /// the <see cref="Task{TResult}.Result">Result</see> of the completed task from there.</note>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="ConvertPixelFormat(BitmapSource, PixelFormat, Color[], Color, byte)"/> method for more details, or the other overloads for image examples.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmap"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="newPixelFormat"/> does not specify a valid format.</exception>
        /// <exception cref="ArgumentException">The <paramref name="palette"/> contains too many colors for the indexed format specified by <paramref name="newPixelFormat"/>.</exception>
        /// <exception cref="InvalidOperationException">A deadlock has been detected while attempting to create the result.</exception>
        public static Task<WriteableBitmap?> ConvertPixelFormatAsync(this BitmapSource bitmap, PixelFormat newPixelFormat, Color[]? palette, Color backColor = default, byte alphaThreshold = 128, TaskConfig? asyncConfig = null)
        {
            ValidateConvertPixelFormat(bitmap, newPixelFormat);
            var context = new ConversionContext(bitmap, newPixelFormat, palette, backColor, alphaThreshold);
            return AsyncHelper.DoOperationAsync(ctx => DoConvertPixelFormatDirect(ctx, context), asyncConfig);
        }

        /// <summary>
        /// Converts the specified <paramref name="bitmap"/> to a <see cref="WriteableBitmap"/> of the desired <see cref="PixelFormat"/> asynchronously.
        /// </summary>
        /// <param name="bitmap">The original bitmap to convert.</param>
        /// <param name="newPixelFormat">The desired new pixel format.</param>
        /// <param name="quantizer">An optional <see cref="IQuantizer"/> instance to determine the colors of the result.
        /// If <see langword="null"/> and <paramref name="newPixelFormat"/> is an indexed format, then a default palette and quantization logic will be used.</param>
        /// <param name="ditherer">The ditherer to be used. Might be ignored if <paramref name="quantizer"/> is not specified
        /// and <paramref name="newPixelFormat"/> represents an at least 24 bits-per-pixel size. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. Its result is the new <see cref="WriteableBitmap"/> instance with the desired pixel format,
        /// or <see langword="null"/>, if the operation was canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a>property of the <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>Alternatively, you can also use the <see cref="BeginConvertPixelFormat(BitmapSource, PixelFormat, Color, byte, AsyncConfig)"/> method, which is available on every platform.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="caution"><list type="bullet"><item>This method can be called from any thread but if it is called from a different one from the thread of the source <paramref name="bitmap"/>'s dispatcher,
        /// then the result <see cref="WriteableBitmap"/> will be created by using a synchronized callback. To avoid possible deadlocks, the thread of the source <paramref name="bitmap"/>
        /// must not be blocked and the dispatcher must run. The result will be usable in the same thread as the source <paramref name="bitmap"/>.
        /// To avoid blocking waiting for the result use the <see langword="await"/> keyword in C# (when using .NET Framework 4.5 or newer),
        /// or use the <see cref="Task{TResult}.ContinueWith(Action{Task{TResult}})">Task.ContinueWith</see> method to access
        /// the <see cref="Task{TResult}.Result">Result</see> of the completed task from there.</item>
        /// <item>If <paramref name="quantizer"/> is not a <see cref="PredefinedColorsQuantizer"/>, then the result <see cref="WriteableBitmap"/> is created by a synchronized callback
        /// on the thread of the source <paramref name="bitmap"/> even if you call this method from the same thread. Do not block the thread of the source <paramref name="bitmap"/>;
        /// otherwise, a deadlock may occur.</item></list></note>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="ConvertPixelFormat(BitmapSource, PixelFormat, Color, byte)"/> method for more details and image examples.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmap"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="newPixelFormat"/> does not specify a valid format.</exception>
        /// <exception cref="ArgumentException">The <paramref name="quantizer"/> palette contains too many colors for the indexed format specified by <paramref name="newPixelFormat"/>.</exception>
        /// <exception cref="InvalidOperationException">A deadlock has been detected while attempting to create the result.</exception>
        public static Task<WriteableBitmap?> ConvertPixelFormatAsync(this BitmapSource bitmap, PixelFormat newPixelFormat, IQuantizer? quantizer, IDitherer? ditherer = null, TaskConfig? asyncConfig = null)
        {
            ValidateConvertPixelFormat(bitmap, newPixelFormat);
            var context = new ConversionContext(bitmap, newPixelFormat, quantizer, ditherer);
            return context.Quantizer == null
                ? AsyncHelper.DoOperationAsync(ctx => DoConvertPixelFormatDirect(ctx, context), asyncConfig)
                : AsyncHelper.DoOperationAsync(ctx => DoConvertPixelFormatWithQuantizer(ctx, context), asyncConfig);
        }

#endif
        #endregion

        #endregion

        #endregion

        #region Internal Methods

        internal static Palette? GetPalette(this BitmapSource bitmap, WorkingColorSpace workingColorSpace, Color backColor, byte alphaThreshold)
        {
            BitmapPalette? palette = bitmap.Palette;
            return palette == null
                ? null
                : new Palette(palette.Colors.Select(c => c.ToColor32()), workingColorSpace, backColor.ToColor32(), alphaThreshold);
        }

        #endregion

        #region Private Methods

        private static void ValidateConvertPixelFormat(BitmapSource bitmap, PixelFormat newPixelFormat)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap), PublicResources.ArgumentNull);
            if (newPixelFormat == default)
                throw new ArgumentOutOfRangeException(nameof(newPixelFormat), PublicResources.ArgumentOutOfRange);
        }

        private static WriteableBitmap? DoConvertPixelFormatDirect(IAsyncContext asyncContext, ConversionContext conversionContext)
        {
            using (conversionContext)
            {
                if (asyncContext.IsCancellationRequested)
                    return null;

                IReadableBitmapData source = conversionContext.Source;
                return source.CopyTo(conversionContext.Target!, asyncContext, new Rectangle(Point.Empty, source.Size), Point.Empty)
                    ? conversionContext.Result
                    : null;
            }
        }

        private static WriteableBitmap? DoConvertPixelFormatWithQuantizer(IAsyncContext asyncContext, ConversionContext conversionContext)
        {
            using (conversionContext)
            {
                if (asyncContext.IsCancellationRequested)
                    return null;

                IReadableBitmapData source = conversionContext.Source;
                IQuantizer quantizer = conversionContext.Quantizer!;

                // we might have an uninitialized result if the quantizer is not a predefined one
                if (conversionContext.Result == null)
                {
                    Palette? palette;
                    Color32 backColor;
                    byte alphaThreshold;
                    asyncContext.Progress?.New(DrawingOperation.InitializingQuantizer);
                    using (IQuantizingSession quantizingSession = quantizer.Initialize(source, asyncContext))
                    {
                        if (asyncContext.IsCancellationRequested)
                            return null;
                        if (quantizingSession == null)
                            throw new InvalidOperationException(Res.QuantizerInitializeNull);

                        palette = quantizingSession.Palette;
                        backColor = quantizingSession.BackColor;
                        alphaThreshold = quantizingSession.AlphaThreshold;
                    }

                    conversionContext.Invoke(true, () =>
                    {
                        conversionContext.Result = new WriteableBitmap(source.Width, source.Height,
                            conversionContext.BitmapSource!.DpiX, conversionContext.BitmapSource.DpiX, conversionContext.PixelFormat,
                            conversionContext.GetTargetPalette(palette));
                        conversionContext.Target = conversionContext.Result.GetWritableBitmapData(backColor.ToMediaColor(), alphaThreshold);
                    });

                    // We have a palette from a potentially expensive quantizer: creating a predefined quantizer from the already generated palette to avoid generating it again.
                    if (palette != null && quantizer is not PredefinedColorsQuantizer)
                        quantizer = PredefinedColorsQuantizer.FromCustomPalette(palette);
                }

                if (asyncContext.IsCancellationRequested)
                    return null;

                return source.CopyTo(conversionContext.Target!, asyncContext, new Rectangle(Point.Empty, source.Size), Point.Empty, quantizer, conversionContext.Ditherer)
                    ? conversionContext.Result
                    : null;
            }
        }

        #endregion

        #endregion
    }
}
