#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BitmapDataExtensions.Readable.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2021 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.CompilerServices;
#if !NET35
using System.Threading.Tasks; 
#endif

using KGySoft.CoreLibraries;
using KGySoft.Reflection;

#endregion

#if NET35
#pragma warning disable CS1574 // XML comment has cref attribute that could not be resolved - in .NET 3.5 not all members are available
#endif

namespace KGySoft.Drawing.Imaging
{
    partial class BitmapDataExtensions
    {
        #region Methods

        #region Public Methods

        #region Clone

        #region Sync

        /// <summary>
        /// Gets the clone of the specified <paramref name="source"/> with identical size.
        /// </summary>
        /// <param name="source">An <see cref="IReadableBitmapData"/> instance to be cloned.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance that represents the clone of the specified <paramref name="source"/>.</returns>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. Use the <see cref="BeginClone(IReadableBitmapData, AsyncConfig)"/>
        /// or <see cref="CloneAsync(IReadableBitmapData,TaskConfig)"/> (in .NET Framework 4.0 and above) methods for asynchronous call and to adjust parallelization, set up cancellation and for reporting progress.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        public static IReadWriteBitmapData Clone(this IReadableBitmapData source)
        {
            ValidateArguments(source);
            return DoCloneExact(AsyncContext.Null, source)!;
        }

        /// <summary>
        /// Gets the clone of the specified portion of <paramref name="source"/>.
        /// </summary>
        /// <param name="source">An <see cref="IReadableBitmapData"/> instance to be cloned.</param>
        /// <param name="sourceRectangle">A <see cref="Rectangle"/> that specifies the portion of the <paramref name="source"/> to be cloned.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance that represents the clone of the specified <paramref name="source"/>.</returns>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. Use the <see cref="BeginClone(IReadableBitmapData, Rectangle, AsyncConfig?)"/>
        /// or <see cref="CloneAsync(IReadableBitmapData, Rectangle, TaskConfig?)"/> (in .NET Framework 4.0 and above) methods for asynchronous call and to adjust parallelization, set up cancellation and for reporting progress.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="sourceRectangle"/> has no overlapping region with source bounds.</exception>
        public static IReadWriteBitmapData Clone(this IReadableBitmapData source, Rectangle sourceRectangle)
        {
            ValidateArguments(source);
            return DoCloneDirect(AsyncContext.Null, source, sourceRectangle, source.PixelFormat)!;
        }

        /// <summary>
        /// Gets the clone of the specified <paramref name="source"/> with identical size and the specified <paramref name="pixelFormat"/> and color settings.
        /// This method is similar to <see cref="ImageExtensions.ConvertPixelFormat(Image, PixelFormat, Color[], Color, byte)"/> but as the result is a managed <see cref="IReadWriteBitmapData"/> instance
        /// every <see cref="PixelFormat"/> is supported on any platform.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="source">An <see cref="IReadableBitmapData"/> instance to be cloned.</param>
        /// <param name="pixelFormat">The desired pixel format of the result.</param>
        /// <param name="backColor">If <paramref name="pixelFormat"/> does not support alpha or supports only single-bit alpha, then specifies the color of the background.
        /// Source pixels with alpha, which will be opaque in the result will be blended with this color.
        /// The <see cref="Color32.A">Color32.A</see> property of the background color is ignored. This parameter is optional.
        /// <br/>Default value: The default value of the <see cref="Color32"/> type, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="alphaThreshold">If <paramref name="pixelFormat"/> can represent only single-bit alpha or <paramref name="pixelFormat"/> is an indexed format and the target palette contains a transparent color,
        /// then specifies a threshold value for the <see cref="Color32.A">Color32.A</see> property, under which the color is considered transparent. If 0,
        /// then the result will not have transparent pixels. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance that represents the clone of the specified <paramref name="source"/>.</returns>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. Use the <see cref="BeginClone(IReadableBitmapData, PixelFormat, Color32, byte, Rectangle?, AsyncConfig)"/>
        /// or <see cref="CloneAsync(IReadableBitmapData, PixelFormat, Color32, byte, Rectangle?, TaskConfig)"/> (in .NET Framework 4.0 and above) methods for asynchronous call and to adjust parallelization, set up cancellation and for reporting progress.</note>
        /// <para>This overload automatically quantizes colors if <paramref name="pixelFormat"/> represents a narrower set of colors than <paramref name="source"/>&#160;<see cref="IBitmapData.PixelFormat"/>.
        /// To use a custom quantizer use the overloads with an <see cref="IQuantizer"/> parameter.</para>
        /// <para>Color depth of wide-color formats (<see cref="PixelFormat.Format16bppGrayScale"/>, <see cref="PixelFormat.Format48bppRgb"/>, <see cref="PixelFormat.Format64bppArgb"/>, <see cref="PixelFormat.Format64bppPArgb"/>)
        /// can be preserved only between the same pixel formats. If they are different, or <paramref name="source"/> is from a <see cref="Bitmap"/> on Windows, which uses 13 bits-per-pixel channels, then colors might be quantized to 32bpp
        /// ones during the operation.</para>
        /// <para>If <paramref name="pixelFormat"/> represents an indexed format, then the target palette is taken from <paramref name="source"/> if it also has a palette of no more entries than the target indexed format can have;
        /// otherwise, a default palette will be used based on <paramref name="pixelFormat"/>. To specify the desired palette of the result use the <see cref="Clone(IReadableBitmapData, PixelFormat, Palette)"/> overload.</para>
        /// <note type="tip">See the <strong>Examples</strong> section of the <see cref="ImageExtensions.ConvertPixelFormat(Image, PixelFormat, Color, byte)"/> extension method
        /// for some examples. The <see cref="O:KGySoft.Drawing.ImageExtensions.ConvertPixelFormat">ConvertPixelFormat</see> extensions work the same way for <see cref="Image"/>s
        /// as the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.Clone">Clone</see> extensions for <see cref="IReadableBitmapData"/> instances.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="pixelFormat"/> does not specify a valid format.</exception>
        /// <seealso cref="ImageExtensions.ConvertPixelFormat(Image, PixelFormat, Color[], Color, byte)"/>
        public static IReadWriteBitmapData Clone(this IReadableBitmapData source, PixelFormat pixelFormat, Color32 backColor = default, byte alphaThreshold = 128)
        {
            ValidateArguments(source, pixelFormat);
            return DoCloneDirect(AsyncContext.Null, source, new Rectangle(Point.Empty, source.GetSize()), pixelFormat, backColor, alphaThreshold)!;
        }

        /// <summary>
        /// Gets the clone of the specified <paramref name="source"/> with identical size and the specified <paramref name="pixelFormat"/> and <paramref name="palette"/>.
        /// This method is similar to <see cref="ImageExtensions.ConvertPixelFormat(Image, PixelFormat, Color[], Color, byte)"/> but as the result is a managed <see cref="IReadWriteBitmapData"/> instance
        /// every <see cref="PixelFormat"/> is supported on any platform.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="source">An <see cref="IReadableBitmapData"/> instance to be cloned.</param>
        /// <param name="pixelFormat">The desired pixel format of the result.</param>
        /// <param name="palette">If <paramref name="pixelFormat"/> is an indexed format, then specifies the desired <see cref="IBitmapData.Palette"/> of the returned <see cref="IReadWriteBitmapData"/> instance.
        /// It determines also the <see cref="IBitmapData.BackColor"/> and <see cref="IBitmapData.AlphaThreshold"/> properties of the result.
        /// If <see langword="null"/>, then the target palette is taken from <paramref name="source"/> if it also has a palette of no more entries than the target indexed format can have;
        /// otherwise, a default palette will be used based on <paramref name="pixelFormat"/>.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance that represents the clone of the specified <paramref name="source"/>.</returns>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. Use the <see cref="BeginClone(IReadableBitmapData, PixelFormat, Palette, Rectangle?, AsyncConfig)"/>
        /// or <see cref="CloneAsync(IReadableBitmapData, PixelFormat, Palette, Rectangle?, TaskConfig)"/> (in .NET Framework 4.0 and above) methods for asynchronous call and to adjust parallelization, set up cancellation and for reporting progress.</note>
        /// <para>This overload automatically quantizes colors if <paramref name="pixelFormat"/> represents a narrower set of colors than <paramref name="source"/>&#160;<see cref="IBitmapData.PixelFormat"/>.
        /// To use a custom quantizer use the overloads with an <see cref="IQuantizer"/> parameter.</para>
        /// <para>Color depth of wide-color formats (<see cref="PixelFormat.Format16bppGrayScale"/>, <see cref="PixelFormat.Format48bppRgb"/>, <see cref="PixelFormat.Format64bppArgb"/>, <see cref="PixelFormat.Format64bppPArgb"/>)
        /// can be preserved only between the same pixel formats. If they are different, or <paramref name="source"/> is from a <see cref="Bitmap"/> on Windows, which uses 13 bits-per-pixel channels, then colors might be quantized to 32bpp
        /// ones during the operation.</para>
        /// <note type="tip">See the <strong>Examples</strong> section of the <see cref="ImageExtensions.ConvertPixelFormat(Image, PixelFormat, Color[], Color, byte)"/> extension method
        /// for some examples. The <see cref="O:KGySoft.Drawing.ImageExtensions.ConvertPixelFormat">ConvertPixelFormat</see> extensions work the same way for <see cref="Image"/>s
        /// as the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.Clone">Clone</see> extensions for <see cref="IReadableBitmapData"/> instances.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="pixelFormat"/> does not specify a valid format.</exception>
        /// <exception cref="ArgumentException"><paramref name="palette"/> contains too many colors for the specified <paramref name="pixelFormat"/>.</exception>
        /// <seealso cref="ImageExtensions.ConvertPixelFormat(Image, PixelFormat, Color[], Color, byte)"/>
        public static IReadWriteBitmapData Clone(this IReadableBitmapData source, PixelFormat pixelFormat, Palette? palette)
        {
            ValidateArguments(source, pixelFormat);
            return DoCloneDirect(AsyncContext.Null, source, new Rectangle(Point.Empty, source.GetSize()), pixelFormat, palette?.BackColor ?? default, palette?.AlphaThreshold ?? 128, palette)!;
        }

        /// <summary>
        /// Gets the clone of the specified portion of <paramref name="source"/> with the specified <paramref name="pixelFormat"/> and color settings.
        /// This method is similar to <see cref="ImageExtensions.ConvertPixelFormat(Image, PixelFormat, Color[], Color, byte)"/> but as the result is a managed <see cref="IReadWriteBitmapData"/> instance
        /// every <see cref="PixelFormat"/> is supported on any platform.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="source">An <see cref="IReadableBitmapData"/> instance to be cloned.</param>
        /// <param name="sourceRectangle">A <see cref="Rectangle"/> that specifies the portion of the <paramref name="source"/> to be cloned.</param>
        /// <param name="pixelFormat">The desired pixel format of the result.</param>
        /// <param name="backColor">If <paramref name="pixelFormat"/> does not support alpha or supports only single-bit alpha, then specifies the color of the background.
        /// Source pixels with alpha, which will be opaque in the result will be blended with this color.
        /// The <see cref="Color32.A">Color32.A</see> property of the background color is ignored. This parameter is optional.
        /// <br/>Default value: The default value of the <see cref="Color32"/> type, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="alphaThreshold">If <paramref name="pixelFormat"/> can represent only single-bit alpha or <paramref name="pixelFormat"/> is an indexed format and the target palette contains a transparent color,
        /// then specifies a threshold value for the <see cref="Color32.A">Color32.A</see> property, under which the color is considered transparent. If 0,
        /// then the result will not have transparent pixels. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance that represents the clone of the specified <paramref name="source"/>.</returns>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. Use the <see cref="BeginClone(IReadableBitmapData, PixelFormat, Color32, byte, Rectangle?, AsyncConfig)"/>
        /// or <see cref="CloneAsync(IReadableBitmapData, PixelFormat, Color32, byte, Rectangle?, TaskConfig)"/> (in .NET Framework 4.0 and above) methods for asynchronous call and to adjust parallelization, set up cancellation and for reporting progress.</note>
        /// <para>This overload automatically quantizes colors if <paramref name="pixelFormat"/> represents a narrower set of colors than <paramref name="source"/>&#160;<see cref="IBitmapData.PixelFormat"/>.
        /// To use a custom quantizer use the overloads with an <see cref="IQuantizer"/> parameter.</para>
        /// <para>Color depth of wide-color formats (<see cref="PixelFormat.Format16bppGrayScale"/>, <see cref="PixelFormat.Format48bppRgb"/>, <see cref="PixelFormat.Format64bppArgb"/>, <see cref="PixelFormat.Format64bppPArgb"/>)
        /// can be preserved only between the same pixel formats. If they are different, or <paramref name="source"/> is from a <see cref="Bitmap"/> on Windows, which uses 13 bits-per-pixel channels, then colors might be quantized to 32bpp
        /// ones during the operation.</para>
        /// <para>If <paramref name="pixelFormat"/> represents an indexed format, then the target palette is taken from <paramref name="source"/> if it also has a palette of no more entries than the target indexed format can have;
        /// otherwise, a default palette will be used based on <paramref name="pixelFormat"/>. To specify the desired palette of the result use the <see cref="Clone(IReadableBitmapData, Rectangle, PixelFormat, Palette)"/> overload.</para>
        /// <note type="tip">See the <strong>Examples</strong> section of the <see cref="ImageExtensions.ConvertPixelFormat(Image, PixelFormat, Color, byte)"/> extension method
        /// for some examples. The <see cref="O:KGySoft.Drawing.ImageExtensions.ConvertPixelFormat">ConvertPixelFormat</see> extensions work the same way for <see cref="Image"/>s
        /// as the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.Clone">Clone</see> extensions for <see cref="IReadableBitmapData"/> instances.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="pixelFormat"/> does not specify a valid format.
        /// <br/>-or-
        /// <br/><paramref name="sourceRectangle"/> has no overlapping region with source bounds.</exception>
        /// <seealso cref="ImageExtensions.ConvertPixelFormat(Image, PixelFormat, Color[], Color, byte)"/>
        public static IReadWriteBitmapData Clone(this IReadableBitmapData source, Rectangle sourceRectangle, PixelFormat pixelFormat, Color32 backColor = default, byte alphaThreshold = 128)
        {
            ValidateArguments(source, pixelFormat);
            return DoCloneDirect(AsyncContext.Null, source, sourceRectangle, pixelFormat, backColor, alphaThreshold)!;
        }

        /// <summary>
        /// Gets the clone of the specified portion of <paramref name="source"/> with the specified <paramref name="pixelFormat"/> and <paramref name="palette"/>.
        /// This method is similar to <see cref="ImageExtensions.ConvertPixelFormat(Image, PixelFormat, Color[], Color, byte)"/> but as the result is a managed <see cref="IReadWriteBitmapData"/> instance
        /// every <see cref="PixelFormat"/> is supported on any platform.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="source">An <see cref="IReadableBitmapData"/> instance to be cloned.</param>
        /// <param name="sourceRectangle">A <see cref="Rectangle"/> that specifies the portion of the <paramref name="source"/> to be cloned.</param>
        /// <param name="pixelFormat">The desired pixel format of the result.</param>
        /// <param name="palette">If <paramref name="pixelFormat"/> is an indexed format, then specifies the desired <see cref="IBitmapData.Palette"/> of the returned <see cref="IReadWriteBitmapData"/> instance.
        /// It determines also the <see cref="IBitmapData.BackColor"/> and <see cref="IBitmapData.AlphaThreshold"/> properties of the result.
        /// If <see langword="null"/>, then the target palette is taken from <paramref name="source"/> if it also has a palette of no more entries than the target indexed format can have;
        /// otherwise, a default palette will be used based on <paramref name="pixelFormat"/>.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance that represents the clone of the specified <paramref name="source"/>.</returns>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. Use the <see cref="BeginClone(IReadableBitmapData, PixelFormat, Palette, Rectangle?, AsyncConfig)"/>
        /// or <see cref="CloneAsync(IReadableBitmapData, PixelFormat, Palette, Rectangle?, TaskConfig)"/> (in .NET Framework 4.0 and above) methods for asynchronous call and to adjust parallelization, set up cancellation and for reporting progress.</note>
        /// <para>This overload automatically quantizes colors if <paramref name="pixelFormat"/> represents a narrower set of colors than <paramref name="source"/>&#160;<see cref="IBitmapData.PixelFormat"/>.
        /// To use a custom quantizer use the overloads with an <see cref="IQuantizer"/> parameter.</para>
        /// <para>Color depth of wide-color formats (<see cref="PixelFormat.Format16bppGrayScale"/>, <see cref="PixelFormat.Format48bppRgb"/>, <see cref="PixelFormat.Format64bppArgb"/>, <see cref="PixelFormat.Format64bppPArgb"/>)
        /// can be preserved only between the same pixel formats. If they are different, or <paramref name="source"/> is from a <see cref="Bitmap"/> on Windows, which uses 13 bits-per-pixel channels, then colors might be quantized to 32bpp
        /// ones during the operation.</para>
        /// <note type="tip">See the <strong>Examples</strong> section of the <see cref="ImageExtensions.ConvertPixelFormat(Image, PixelFormat, Color[], Color, byte)"/> extension method
        /// for some examples. The <see cref="O:KGySoft.Drawing.ImageExtensions.ConvertPixelFormat">ConvertPixelFormat</see> extensions work the same way for <see cref="Image"/>s
        /// as the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.Clone">Clone</see> extensions for <see cref="IReadableBitmapData"/> instances.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="pixelFormat"/> does not specify a valid format.
        /// <br/>-or-
        /// <br/><paramref name="sourceRectangle"/> has no overlapping region with source bounds.</exception>
        /// <exception cref="ArgumentException"><paramref name="palette"/> contains too many colors for the specified <paramref name="pixelFormat"/>.</exception>
        /// <seealso cref="ImageExtensions.ConvertPixelFormat(Image, PixelFormat, Color[], Color, byte)"/>
        public static IReadWriteBitmapData Clone(this IReadableBitmapData source, Rectangle sourceRectangle, PixelFormat pixelFormat, Palette? palette)
        {
            ValidateArguments(source, pixelFormat);
            return DoCloneDirect(AsyncContext.Null, source, sourceRectangle, pixelFormat, palette?.BackColor ?? default, palette?.AlphaThreshold ?? 128, palette)!;
        }

        /// <summary>
        /// Gets the clone of the specified <paramref name="source"/> with identical size and the specified <paramref name="pixelFormat"/>, using an optional <paramref name="quantizer"/> and <paramref name="ditherer"/>.
        /// This method is similar to <see cref="ImageExtensions.ConvertPixelFormat(Image, PixelFormat, IQuantizer, IDitherer)"/> but as the result is a managed <see cref="IReadWriteBitmapData"/> instance
        /// every <see cref="PixelFormat"/> is supported on any platform.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="source">An <see cref="IReadableBitmapData"/> instance to be cloned.</param>
        /// <param name="pixelFormat">The desired pixel format of the result.</param>
        /// <param name="quantizer">An optional <see cref="IQuantizer"/> instance to determine the colors of the result.
        /// If <see langword="null"/>&#160;and <paramref name="pixelFormat"/> is an indexed format, then a default palette and quantization logic will be used.</param>
        /// <param name="ditherer">The ditherer to be used. Might be ignored if <paramref name="quantizer"/> is not specified
        /// and <paramref name="pixelFormat"/> represents an at least 24 bits-per-pixel size. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance that represents the clone of the specified <paramref name="source"/>.</returns>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. Use the <see cref="BeginClone(IReadableBitmapData, PixelFormat, IQuantizer, IDitherer, Rectangle?, AsyncConfig)"/>
        /// or <see cref="CloneAsync(IReadableBitmapData, PixelFormat, IQuantizer, IDitherer, Rectangle?, TaskConfig)"/> (in .NET Framework 4.0 and above) methods for asynchronous call and to adjust parallelization, set up cancellation and for reporting progress.</note>
        /// <para>If <paramref name="pixelFormat"/> can represent a narrower set of colors, then the result will be automatically quantized to its color space,
        /// even if there is no <paramref name="quantizer"/> specified. To use dithering a <paramref name="ditherer"/> must be explicitly specified though.</para>
        /// <para>If <paramref name="quantizer"/> is specified but it uses more/different colors than <paramref name="pixelFormat"/> can represent,
        /// then the result will eventually quantized, though the result may have a poorer quality than expected.</para>
        /// <para>Color depth of wide-color formats (<see cref="PixelFormat.Format16bppGrayScale"/>, <see cref="PixelFormat.Format48bppRgb"/>, <see cref="PixelFormat.Format64bppArgb"/>, <see cref="PixelFormat.Format64bppPArgb"/>)
        /// can be preserved only between the same pixel formats if there is no <paramref name="quantizer"/> specified. If pixel formats are different, or <paramref name="source"/> is from a <see cref="Bitmap"/> on Windows,
        /// which uses 13 bits-per-pixel channels, then colors might be quantized to 32bpp ones during the operation.</para>
        /// <note type="tip">See the <strong>Examples</strong> section of the <see cref="ImageExtensions.ConvertPixelFormat(Image, PixelFormat, IQuantizer, IDitherer)"/> extension method
        /// for some examples. The <see cref="O:KGySoft.Drawing.ImageExtensions.ConvertPixelFormat">ConvertPixelFormat</see> extensions work the same way for <see cref="Image"/>s
        /// as the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.Clone">Clone</see> extensions for <see cref="IReadableBitmapData"/> instances.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="pixelFormat"/> does not specify a valid format.</exception>
        /// <exception cref="ArgumentException"><paramref name="quantizer"/> uses a palette with too many colors for the specified <paramref name="pixelFormat"/>.</exception>
        /// <seealso cref="ImageExtensions.ConvertPixelFormat(Image, PixelFormat, IQuantizer, IDitherer)"/>
        public static IReadWriteBitmapData Clone(this IReadableBitmapData source, PixelFormat pixelFormat, IQuantizer? quantizer, IDitherer? ditherer = null)
        {
            ValidateArguments(source, pixelFormat);
            return DoCloneWithQuantizer(AsyncContext.Null, source, new Rectangle(Point.Empty, source.GetSize()), pixelFormat, quantizer, ditherer)!;
        }

        /// <summary>
        /// Gets the clone of the specified <paramref name="source"/> with identical size and the specified <paramref name="pixelFormat"/>, using an optional <paramref name="ditherer"/>.
        /// This method is similar to <see cref="ImageExtensions.ConvertPixelFormat(Image, PixelFormat, IQuantizer, IDitherer)"/> but as the result is a managed <see cref="IReadWriteBitmapData"/> instance
        /// every <see cref="PixelFormat"/> is supported on any platform.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="source">An <see cref="IReadableBitmapData"/> instance to be cloned.</param>
        /// <param name="pixelFormat">The desired pixel format of the result.</param>
        /// <param name="ditherer">The ditherer to be used. Might be ignored if <paramref name="pixelFormat"/> represents an at least 24 bits-per-pixel size.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance that represents the clone of the specified <paramref name="source"/>.</returns>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. Use the <see cref="BeginClone(IReadableBitmapData, PixelFormat, IQuantizer, IDitherer, Rectangle?, AsyncConfig)"/>
        /// or <see cref="CloneAsync(IReadableBitmapData, PixelFormat, IQuantizer, IDitherer, Rectangle?, TaskConfig)"/> (in .NET Framework 4.0 and above) methods for asynchronous call and to adjust parallelization, set up cancellation and for reporting progress.</note>
        /// <para>If <paramref name="pixelFormat"/> can represent a narrower set of colors, then the result will be automatically quantized to its color space.
        /// To use dithering a <paramref name="ditherer"/> must be explicitly specified.</para>
        /// <para>Color depth of wide-color formats (<see cref="PixelFormat.Format16bppGrayScale"/>, <see cref="PixelFormat.Format48bppRgb"/>, <see cref="PixelFormat.Format64bppArgb"/>, <see cref="PixelFormat.Format64bppPArgb"/>)
        /// can be preserved only between the same pixel formats. If pixel formats are different, or <paramref name="source"/> is from a <see cref="Bitmap"/> on Windows,
        /// which uses 13 bits-per-pixel channels, then colors might be quantized to 32bpp ones during the operation.</para>
        /// <note type="tip">See the <strong>Examples</strong> section of the <see cref="ImageExtensions.ConvertPixelFormat(Image, PixelFormat, IQuantizer, IDitherer)"/> extension method
        /// for some examples. The <see cref="O:KGySoft.Drawing.ImageExtensions.ConvertPixelFormat">ConvertPixelFormat</see> extensions work the same way for <see cref="Image"/>s
        /// as the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.Clone">Clone</see> extensions for <see cref="IReadableBitmapData"/> instances.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="pixelFormat"/> does not specify a valid format.</exception>
        /// <seealso cref="ImageExtensions.ConvertPixelFormat(Image, PixelFormat, IQuantizer, IDitherer)"/>
        public static IReadWriteBitmapData Clone(this IReadableBitmapData source, PixelFormat pixelFormat, IDitherer? ditherer)
        {
            ValidateArguments(source, pixelFormat);
            return DoCloneWithQuantizer(AsyncContext.Null, source, new Rectangle(Point.Empty, source.GetSize()), pixelFormat, null, ditherer)!;
        }

        /// <summary>
        /// Gets the clone of the specified portion of <paramref name="source"/> with the specified <paramref name="pixelFormat"/>, using an optional <paramref name="ditherer"/>.
        /// This method is similar to <see cref="ImageExtensions.ConvertPixelFormat(Image, PixelFormat, IQuantizer, IDitherer)"/> but as the result is a managed <see cref="IReadWriteBitmapData"/> instance
        /// every <see cref="PixelFormat"/> is supported on any platform.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="source">An <see cref="IReadableBitmapData"/> instance to be cloned.</param>
        /// <param name="sourceRectangle">A <see cref="Rectangle"/> that specifies the portion of the <paramref name="source"/> to be cloned.</param>
        /// <param name="pixelFormat">The desired pixel format of the result.</param>
        /// <param name="ditherer">The ditherer to be used. Might be ignored if <paramref name="pixelFormat"/> represents an at least 24 bits-per-pixel size.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance that represents the clone of the specified <paramref name="source"/>.</returns>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. Use the <see cref="BeginClone(IReadableBitmapData, PixelFormat, IQuantizer, IDitherer, Rectangle?, AsyncConfig)"/>
        /// or <see cref="CloneAsync(IReadableBitmapData, PixelFormat, IQuantizer, IDitherer, Rectangle?, TaskConfig)"/> (in .NET Framework 4.0 and above) methods for asynchronous call and to adjust parallelization, set up cancellation and for reporting progress.</note>
        /// <para>If <paramref name="pixelFormat"/> can represent a narrower set of colors, then the result will be automatically quantized to its color space.
        /// To use dithering a <paramref name="ditherer"/> must be explicitly specified.</para>
        /// <para>Color depth of wide-color formats (<see cref="PixelFormat.Format16bppGrayScale"/>, <see cref="PixelFormat.Format48bppRgb"/>, <see cref="PixelFormat.Format64bppArgb"/>, <see cref="PixelFormat.Format64bppPArgb"/>)
        /// can be preserved only between the same pixel formats. If pixel formats are different, or <paramref name="source"/> is from a <see cref="Bitmap"/> on Windows,
        /// which uses 13 bits-per-pixel channels, then colors might be quantized to 32bpp ones during the operation.</para>
        /// <note type="tip">See the <strong>Examples</strong> section of the <see cref="ImageExtensions.ConvertPixelFormat(Image, PixelFormat, IQuantizer, IDitherer)"/> extension method
        /// for some examples. The <see cref="O:KGySoft.Drawing.ImageExtensions.ConvertPixelFormat">ConvertPixelFormat</see> extensions work the same way for <see cref="Image"/>s
        /// as the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.Clone">Clone</see> extensions for <see cref="IReadableBitmapData"/> instances.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="pixelFormat"/> does not specify a valid format.
        /// <br/>-or-
        /// <br/><paramref name="sourceRectangle"/> has no overlapping region with source bounds.</exception>
        /// <seealso cref="ImageExtensions.ConvertPixelFormat(Image, PixelFormat, IQuantizer, IDitherer)"/>
        public static IReadWriteBitmapData Clone(this IReadableBitmapData source, Rectangle sourceRectangle, PixelFormat pixelFormat, IDitherer? ditherer)
        {
            ValidateArguments(source, pixelFormat);
            return DoCloneWithQuantizer(AsyncContext.Null, source, sourceRectangle, pixelFormat, null, ditherer)!;
        }

        /// <summary>
        /// Gets the clone of the specified portion of <paramref name="source"/> with the specified <paramref name="pixelFormat"/>, using an optional <paramref name="quantizer"/> and <paramref name="ditherer"/>.
        /// This method is similar to <see cref="ImageExtensions.ConvertPixelFormat(Image, PixelFormat, IQuantizer, IDitherer)"/> but as the result is a managed <see cref="IReadWriteBitmapData"/> instance
        /// every <see cref="PixelFormat"/> is supported on any platform.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="source">An <see cref="IReadableBitmapData"/> instance to be cloned.</param>
        /// <param name="sourceRectangle">A <see cref="Rectangle"/> that specifies the portion of the <paramref name="source"/> to be cloned.</param>
        /// <param name="pixelFormat">The desired pixel format of the result.</param>
        /// <param name="quantizer">An optional <see cref="IQuantizer"/> instance to determine the colors of the result.
        /// If <see langword="null"/>&#160;and <paramref name="pixelFormat"/> is an indexed format, then a default palette and quantization logic will be used.</param>
        /// <param name="ditherer">The ditherer to be used. Might be ignored if <paramref name="quantizer"/> is not specified
        /// and <paramref name="pixelFormat"/> represents an at least 24 bits-per-pixel size. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance that represents the clone of the specified <paramref name="source"/>.</returns>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. Use the <see cref="BeginClone(IReadableBitmapData, PixelFormat, IQuantizer, IDitherer, Rectangle?, AsyncConfig)"/>
        /// or <see cref="CloneAsync(IReadableBitmapData, PixelFormat, IQuantizer, IDitherer, Rectangle?, TaskConfig)"/> (in .NET Framework 4.0 and above) methods for asynchronous call and to adjust parallelization, set up cancellation and for reporting progress.</note>
        /// <para>If <paramref name="pixelFormat"/> can represent a narrower set of colors, then the result will be automatically quantized to its color space,
        /// even if there is no <paramref name="quantizer"/> specified. To use dithering a <paramref name="ditherer"/> must be explicitly specified though.</para>
        /// <para>If <paramref name="quantizer"/> is specified but it uses more/different colors than <paramref name="pixelFormat"/> can represent,
        /// then the result will eventually quantized, though the result may have a poorer quality than expected.</para>
        /// <para>Color depth of wide-color formats (<see cref="PixelFormat.Format16bppGrayScale"/>, <see cref="PixelFormat.Format48bppRgb"/>, <see cref="PixelFormat.Format64bppArgb"/>, <see cref="PixelFormat.Format64bppPArgb"/>)
        /// can be preserved only between the same pixel formats if there is no <paramref name="quantizer"/> specified. If pixel formats are different, or <paramref name="source"/> is from a <see cref="Bitmap"/> on Windows,
        /// which uses 13 bits-per-pixel channels, then colors might be quantized to 32bpp ones during the operation.</para>
        /// <note type="tip">See the <strong>Examples</strong> section of the <see cref="ImageExtensions.ConvertPixelFormat(Image, PixelFormat, IQuantizer, IDitherer)"/> extension method
        /// for some examples. The <see cref="O:KGySoft.Drawing.ImageExtensions.ConvertPixelFormat">ConvertPixelFormat</see> extensions work the same way for <see cref="Image"/>s
        /// as the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.Clone">Clone</see> extensions for <see cref="IReadableBitmapData"/> instances.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="pixelFormat"/> does not specify a valid format.
        /// <br/>-or-
        /// <br/><paramref name="sourceRectangle"/> has no overlapping region with source bounds.</exception>
        /// <exception cref="ArgumentException"><paramref name="quantizer"/> uses a palette with too many colors for the specified <paramref name="pixelFormat"/>.</exception>
        /// <seealso cref="ImageExtensions.ConvertPixelFormat(Image, PixelFormat, IQuantizer, IDitherer)"/>
        public static IReadWriteBitmapData Clone(this IReadableBitmapData source, Rectangle sourceRectangle, PixelFormat pixelFormat, IQuantizer? quantizer, IDitherer? ditherer = null)
        {
            ValidateArguments(source, pixelFormat);
            return DoCloneWithQuantizer(AsyncContext.Null, source, sourceRectangle, pixelFormat, quantizer, ditherer)!;
        }

        #endregion

        #region Async APM

        /// <summary>
        /// Begins to clone the specified <paramref name="source"/> with identical size asynchronously.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="source">An <see cref="IReadableBitmapData"/> instance to be cloned.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="CloneAsync(IReadableBitmapData, TaskConfig)"/> method.</para>
        /// <para>To get the result or the exception that occurred during the operation you have to call the <see cref="EndClone">EndClone</see> method.</para>
        /// <para>This method is not a blocking call even if the <see cref="AsyncConfigBase.MaxDegreeOfParallelism"/> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        public static IAsyncResult BeginClone(this IReadableBitmapData source, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(source);
            return AsyncContext.BeginOperation(ctx => DoCloneExact(ctx, source), asyncConfig);
        }

        /// <summary>
        /// Begins to clone the specified portion of the specified <paramref name="source"/> asynchronously.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="source">An <see cref="IReadableBitmapData"/> instance to be cloned.</param>
        /// <param name="sourceRectangle">A <see cref="Rectangle"/> that specifies the portion of the <paramref name="source"/> to be cloned.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="CloneAsync(IReadableBitmapData, Rectangle, TaskConfig?)"/> method.</para>
        /// <para>To get the result or the exception that occurred during the operation you have to call the <see cref="EndClone">EndClone</see> method.</para>
        /// <para>This method is not a blocking call even if the <see cref="AsyncConfigBase.MaxDegreeOfParallelism"/> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="sourceRectangle"/> has no overlapping region with source bounds.</exception>
        public static IAsyncResult BeginClone(this IReadableBitmapData source, Rectangle sourceRectangle, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(source);
            return AsyncContext.BeginOperation(ctx => DoCloneDirect(ctx, source, sourceRectangle, source.PixelFormat), asyncConfig);
        }

        /// <summary>
        /// Begins to clone the specified portion of <paramref name="source"/> with the specified <paramref name="pixelFormat"/> and color settings asynchronously.
        /// This method is similar to <see cref="ImageExtensions.BeginConvertPixelFormat(Image, PixelFormat, Color, byte, AsyncConfig)"/> but as the result is a managed <see cref="IReadWriteBitmapData"/> instance
        /// every <see cref="PixelFormat"/> is supported on any platform.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="source">An <see cref="IReadableBitmapData"/> instance to be cloned.</param>
        /// <param name="pixelFormat">The desired pixel format of the result.</param>
        /// <param name="backColor">If <paramref name="pixelFormat"/> does not support alpha or supports only single-bit alpha, then specifies the color of the background.
        /// Source pixels with alpha, which will be opaque in the result will be blended with this color.
        /// The <see cref="Color32.A">Color32.A</see> property of the background color is ignored. This parameter is optional.
        /// <br/>Default value: The default value of the <see cref="Color32"/> type, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="alphaThreshold">If <paramref name="pixelFormat"/> can represent only single-bit alpha or <paramref name="pixelFormat"/> is an indexed format and the target palette contains a transparent color,
        /// then specifies a threshold value for the <see cref="Color32.A">Color32.A</see> property, under which the color is considered transparent. If 0,
        /// then the result will not have transparent pixels. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <param name="sourceRectangle">A <see cref="Rectangle"/> that specifies the portion of the <paramref name="source"/> to be cloned, or <see langword="null"/>&#160;to clone the entire <paramref name="source"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="CloneAsync(IReadableBitmapData, PixelFormat, Color32, byte, Rectangle?, TaskConfig)"/> method.</para>
        /// <para>To get the result or the exception that occurred during the operation you have to call the <see cref="EndClone">EndClone</see> method.</para>
        /// <para>This method is not a blocking call even if the <see cref="AsyncConfigBase.MaxDegreeOfParallelism"/> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="Clone(IReadableBitmapData, Rectangle, PixelFormat, Color32, byte)"/> method for more details.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="pixelFormat"/> does not specify a valid format.
        /// <br/>-or-
        /// <br/><paramref name="sourceRectangle"/> has no overlapping region with source bounds.</exception>
        /// <seealso cref="ImageExtensions.BeginConvertPixelFormat(Image, PixelFormat, Color, byte, AsyncConfig)"/>
        public static IAsyncResult BeginClone(this IReadableBitmapData source, PixelFormat pixelFormat, Color32 backColor = default, byte alphaThreshold = 128, Rectangle? sourceRectangle = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(source, pixelFormat);
            return AsyncContext.BeginOperation(ctx => DoCloneDirect(ctx, source, sourceRectangle ?? new Rectangle(Point.Empty, source.GetSize()), pixelFormat, backColor, alphaThreshold), asyncConfig);
        }

        /// <summary>
        /// Begins to clone the specified portion of <paramref name="source"/> with the specified <paramref name="pixelFormat"/> and <paramref name="palette"/> asynchronously.
        /// This method is similar to <see cref="ImageExtensions.BeginConvertPixelFormat(Image, PixelFormat, Color[], Color, byte, AsyncConfig)"/> but as the result is a managed <see cref="IReadWriteBitmapData"/> instance
        /// every <see cref="PixelFormat"/> is supported on any platform.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="source">An <see cref="IReadableBitmapData"/> instance to be cloned.</param>
        /// <param name="pixelFormat">The desired pixel format of the result.</param>
        /// <param name="palette">If <paramref name="pixelFormat"/> is an indexed format, then specifies the desired <see cref="IBitmapData.Palette"/> of the returned <see cref="IReadWriteBitmapData"/> instance.
        /// It determines also the <see cref="IBitmapData.BackColor"/> and <see cref="IBitmapData.AlphaThreshold"/> properties of the result.
        /// If <see langword="null"/>, then the target palette is taken from <paramref name="source"/> if it also has a palette of no more entries than the target indexed format can have;
        /// otherwise, a default palette will be used based on <paramref name="pixelFormat"/>.</param>
        /// <param name="sourceRectangle">A <see cref="Rectangle"/> that specifies the portion of the <paramref name="source"/> to be cloned, or <see langword="null"/>&#160;to clone the entire <paramref name="source"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="CloneAsync(IReadableBitmapData, PixelFormat, Palette, Rectangle?, TaskConfig)"/> method.</para>
        /// <para>To get the result or the exception that occurred during the operation you have to call the <see cref="EndClone">EndClone</see> method.</para>
        /// <para>This method is not a blocking call even if the <see cref="AsyncConfigBase.MaxDegreeOfParallelism"/> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="Clone(IReadableBitmapData, Rectangle, PixelFormat, Palette)"/> method for more details.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="pixelFormat"/> does not specify a valid format.
        /// <br/>-or-
        /// <br/><paramref name="sourceRectangle"/> has no overlapping region with source bounds.</exception>
        /// <exception cref="ArgumentException"><paramref name="palette"/> contains too many colors for the specified <paramref name="pixelFormat"/>.</exception>
        /// <seealso cref="ImageExtensions.BeginConvertPixelFormat(Image, PixelFormat, Color[], Color, byte, AsyncConfig)"/>
        public static IAsyncResult BeginClone(this IReadableBitmapData source, PixelFormat pixelFormat, Palette? palette, Rectangle? sourceRectangle = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(source, pixelFormat);
            return AsyncContext.BeginOperation(ctx => DoCloneDirect(ctx, source, sourceRectangle ?? new Rectangle(Point.Empty, source.GetSize()), pixelFormat, palette?.BackColor ?? default, palette?.AlphaThreshold ?? 128, palette), asyncConfig);
        }

        /// <summary>
        /// Begins to clone the specified portion of <paramref name="source"/> with the specified <paramref name="pixelFormat"/>, using an optional <paramref name="quantizer"/> and <paramref name="ditherer"/> asynchronously.
        /// This method is similar to <see cref="ImageExtensions.BeginConvertPixelFormat(Image, PixelFormat, IQuantizer, IDitherer, AsyncConfig)"/> but as the result is a managed <see cref="IReadWriteBitmapData"/> instance
        /// every <see cref="PixelFormat"/> is supported on any platform.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="source">An <see cref="IReadableBitmapData"/> instance to be cloned.</param>
        /// <param name="pixelFormat">The desired pixel format of the result.</param>
        /// <param name="quantizer">An optional <see cref="IQuantizer"/> instance to determine the colors of the result.
        /// If <see langword="null"/>&#160;and <paramref name="pixelFormat"/> is an indexed format, then a default palette and quantization logic will be used.</param>
        /// <param name="ditherer">The ditherer to be used. Might be ignored if <paramref name="quantizer"/> is not specified
        /// and <paramref name="pixelFormat"/> represents an at least 24 bits-per-pixel size. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="sourceRectangle">A <see cref="Rectangle"/> that specifies the portion of the <paramref name="source"/> to be cloned, or <see langword="null"/>&#160;to clone the entire <paramref name="source"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance that represents the clone of the specified <paramref name="source"/>.</returns>
        /// <remarks>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="CloneAsync(IReadableBitmapData, PixelFormat, Palette, Rectangle?, TaskConfig)"/> method.</para>
        /// <para>To get the result or the exception that occurred during the operation you have to call the <see cref="EndClone">EndClone</see> method.</para>
        /// <para>This method is not a blocking call even if the <see cref="AsyncConfigBase.MaxDegreeOfParallelism"/> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="Clone(IReadableBitmapData, Rectangle, PixelFormat, IQuantizer, IDitherer)"/> method for more details.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="pixelFormat"/> does not specify a valid format.
        /// <br/>-or-
        /// <br/><paramref name="sourceRectangle"/> has no overlapping region with source bounds.</exception>
        /// <exception cref="ArgumentException"><paramref name="quantizer"/> uses a palette with too many colors for the specified <paramref name="pixelFormat"/>.</exception>
        /// <seealso cref="ImageExtensions.BeginConvertPixelFormat(Image, PixelFormat, IQuantizer, IDitherer, AsyncConfig)"/>
        public static IAsyncResult BeginClone(this IReadableBitmapData source, PixelFormat pixelFormat, IQuantizer? quantizer, IDitherer? ditherer = null, Rectangle? sourceRectangle = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(source, pixelFormat);
            return AsyncContext.BeginOperation(ctx => DoCloneWithQuantizer(ctx, source, sourceRectangle ?? new Rectangle(Point.Empty, source.GetSize()), pixelFormat, quantizer, ditherer), asyncConfig);
        }

        /// <summary>
        /// Waits for the pending asynchronous operation started by the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.BeginClone">BeginClone</see> methods to complete.
        /// In .NET Framework 4.0 and above you can use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.CloneAsync">CloneAsync</see> methods instead.
        /// </summary>
        /// <param name="asyncResult">The reference to the pending asynchronous request to finish.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance that is the result of the operation,
        /// or <see langword="null"/>, if the operation was canceled and <see cref="AsyncConfigBase.ThrowIfCanceled"/> property of the <c>asyncConfig</c> parameter was <see langword="false"/>.</returns>
        public static IReadWriteBitmapData? EndClone(this IAsyncResult asyncResult) => AsyncContext.EndOperation<IReadWriteBitmapData>(asyncResult, nameof(BeginClone));

        #endregion

        #region Async TAP
#if !NET35

        /// <summary>
        /// Gets the clone of the specified <paramref name="source"/> with identical size asynchronously.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="source">An <see cref="IReadableBitmapData"/> instance to be cloned.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. Its result is the new <see cref="IReadWriteBitmapData"/> instance that represents the clone of the specified <paramref name="source"/>,
        /// or <see langword="null"/>, if the operation was canceled and <see cref="AsyncConfigBase.ThrowIfCanceled"/> property of the <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>Alternatively, you can also use the <see cref="BeginClone(IReadableBitmapData, AsyncConfig)"/> method, which is available on every platform.</para>
        /// <para>This method is not a blocking call even if the <see cref="AsyncConfigBase.MaxDegreeOfParallelism"/> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        public static Task<IReadWriteBitmapData?> CloneAsync(this IReadableBitmapData source, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(source);
            return AsyncContext.DoOperationAsync(ctx => DoCloneExact(ctx, source), asyncConfig);
        }

        /// <summary>
        /// Gets the clone of the specified portion of the specified <paramref name="source"/> asynchronously.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="source">An <see cref="IReadableBitmapData"/> instance to be cloned.</param>
        /// <param name="sourceRectangle">A <see cref="Rectangle"/> that specifies the portion of the <paramref name="source"/> to be cloned.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. Its result is the new <see cref="IReadWriteBitmapData"/> instance that represents the clone of the specified <paramref name="source"/>,
        /// or <see langword="null"/>, if the operation was canceled and <see cref="AsyncConfigBase.ThrowIfCanceled"/> property of the <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>Alternatively, you can also use the <see cref="BeginClone(IReadableBitmapData, Rectangle, AsyncConfig?)"/> method, which is available on every platform.</para>
        /// <para>This method is not a blocking call even if the <see cref="AsyncConfigBase.MaxDegreeOfParallelism"/> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="sourceRectangle"/> has no overlapping region with source bounds.</exception>
        public static Task<IReadWriteBitmapData?> CloneAsync(this IReadableBitmapData source, Rectangle sourceRectangle, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(source);
            return AsyncContext.DoOperationAsync(ctx => DoCloneDirect(ctx, source, sourceRectangle, source.PixelFormat), asyncConfig);
        }

        /// <summary>
        /// Gets the clone of the specified portion of <paramref name="source"/> with the specified <paramref name="pixelFormat"/> and color settings asynchronously.
        /// This method is similar to <see cref="ImageExtensions.ConvertPixelFormatAsync(Image, PixelFormat, Color, byte, TaskConfig)"/> but as the result is a managed <see cref="IReadWriteBitmapData"/> instance
        /// every <see cref="PixelFormat"/> is supported on any platform.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="source">An <see cref="IReadableBitmapData"/> instance to be cloned.</param>
        /// <param name="pixelFormat">The desired pixel format of the result.</param>
        /// <param name="backColor">If <paramref name="pixelFormat"/> does not support alpha or supports only single-bit alpha, then specifies the color of the background.
        /// Source pixels with alpha, which will be opaque in the result will be blended with this color.
        /// The <see cref="Color32.A">Color32.A</see> property of the background color is ignored. This parameter is optional.
        /// <br/>Default value: The default value of the <see cref="Color32"/> type, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="alphaThreshold">If <paramref name="pixelFormat"/> can represent only single-bit alpha or <paramref name="pixelFormat"/> is an indexed format and the target palette contains a transparent color,
        /// then specifies a threshold value for the <see cref="Color32.A">Color32.A</see> property, under which the color is considered transparent. If 0,
        /// then the result will not have transparent pixels. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <param name="sourceRectangle">A <see cref="Rectangle"/> that specifies the portion of the <paramref name="source"/> to be cloned, or <see langword="null"/>&#160;to clone the entire <paramref name="source"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. Its result is the new <see cref="IReadWriteBitmapData"/> instance that represents the clone of the specified <paramref name="source"/>,
        /// or <see langword="null"/>, if the operation was canceled and <see cref="AsyncConfigBase.ThrowIfCanceled"/> property of the <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>This method is not a blocking call even if the <see cref="AsyncConfigBase.MaxDegreeOfParallelism"/> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="Clone(IReadableBitmapData, Rectangle, PixelFormat, Color32, byte)"/> method for more details.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="pixelFormat"/> does not specify a valid format.
        /// <br/>-or-
        /// <br/><paramref name="sourceRectangle"/> has no overlapping region with source bounds.</exception>
        /// <seealso cref="ImageExtensions.ConvertPixelFormatAsync(Image, PixelFormat, Color, byte, TaskConfig)"/>
        public static Task<IReadWriteBitmapData?> CloneAsync(this IReadableBitmapData source, PixelFormat pixelFormat, Color32 backColor = default, byte alphaThreshold = 128, Rectangle? sourceRectangle = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(source, pixelFormat);
            return AsyncContext.DoOperationAsync(ctx => DoCloneDirect(ctx, source, sourceRectangle ?? new Rectangle(Point.Empty, source.GetSize()), pixelFormat, backColor, alphaThreshold), asyncConfig);
        }

        /// <summary>
        /// Gets the clone of the specified portion of <paramref name="source"/> with the specified <paramref name="pixelFormat"/> and <paramref name="palette"/> asynchronously.
        /// This method is similar to <see cref="ImageExtensions.ConvertPixelFormatAsync(Image, PixelFormat, Color[], Color, byte, TaskConfig)"/> but as the result is a managed <see cref="IReadWriteBitmapData"/> instance
        /// every <see cref="PixelFormat"/> is supported on any platform.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="source">An <see cref="IReadableBitmapData"/> instance to be cloned.</param>
        /// <param name="pixelFormat">The desired pixel format of the result.</param>
        /// <param name="palette">If <paramref name="pixelFormat"/> is an indexed format, then specifies the desired <see cref="IBitmapData.Palette"/> of the returned <see cref="IReadWriteBitmapData"/> instance.
        /// It determines also the <see cref="IBitmapData.BackColor"/> and <see cref="IBitmapData.AlphaThreshold"/> properties of the result.
        /// If <see langword="null"/>, then the target palette is taken from <paramref name="source"/> if it also has a palette of no more entries than the target indexed format can have;
        /// otherwise, a default palette will be used based on <paramref name="pixelFormat"/>.</param>
        /// <param name="sourceRectangle">A <see cref="Rectangle"/> that specifies the portion of the <paramref name="source"/> to be cloned, or <see langword="null"/>&#160;to clone the entire <paramref name="source"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. Its result is the new <see cref="IReadWriteBitmapData"/> instance that represents the clone of the specified <paramref name="source"/>,
        /// or <see langword="null"/>, if the operation was canceled and <see cref="AsyncConfigBase.ThrowIfCanceled"/> property of the <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>This method is not a blocking call even if the <see cref="AsyncConfigBase.MaxDegreeOfParallelism"/> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="Clone(IReadableBitmapData, Rectangle, PixelFormat, Palette)"/> method for more details.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="pixelFormat"/> does not specify a valid format.
        /// <br/>-or-
        /// <br/><paramref name="sourceRectangle"/> has no overlapping region with source bounds.</exception>
        /// <exception cref="ArgumentException"><paramref name="palette"/> contains too many colors for the specified <paramref name="pixelFormat"/>.</exception>
        /// <seealso cref="ImageExtensions.ConvertPixelFormatAsync(Image, PixelFormat, Color[], Color, byte, TaskConfig)"/>
        public static Task<IReadWriteBitmapData?> CloneAsync(this IReadableBitmapData source, PixelFormat pixelFormat, Palette? palette, Rectangle? sourceRectangle = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(source, pixelFormat);
            return AsyncContext.DoOperationAsync(ctx => DoCloneDirect(ctx, source, sourceRectangle ?? new Rectangle(Point.Empty, source.GetSize()), pixelFormat, palette?.BackColor ?? default, palette?.AlphaThreshold ?? 128, palette), asyncConfig);
        }

        /// <summary>
        /// Gets the clone of the specified portion of <paramref name="source"/> with the specified <paramref name="pixelFormat"/>, using an optional <paramref name="quantizer"/> and <paramref name="ditherer"/> asynchronously.
        /// This method is similar to <see cref="ImageExtensions.ConvertPixelFormatAsync(Image, PixelFormat, IQuantizer, IDitherer, TaskConfig)"/> but as the result is a managed <see cref="IReadWriteBitmapData"/> instance
        /// every <see cref="PixelFormat"/> is supported on any platform.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="source">An <see cref="IReadableBitmapData"/> instance to be cloned.</param>
        /// <param name="pixelFormat">The desired pixel format of the result.</param>
        /// <param name="quantizer">An optional <see cref="IQuantizer"/> instance to determine the colors of the result.
        /// If <see langword="null"/>&#160;and <paramref name="pixelFormat"/> is an indexed format, then a default palette and quantization logic will be used.</param>
        /// <param name="ditherer">The ditherer to be used. Might be ignored if <paramref name="quantizer"/> is not specified
        /// and <paramref name="pixelFormat"/> represents an at least 24 bits-per-pixel size. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="sourceRectangle">A <see cref="Rectangle"/> that specifies the portion of the <paramref name="source"/> to be cloned, or <see langword="null"/>&#160;to clone the entire <paramref name="source"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. Its result is the new <see cref="IReadWriteBitmapData"/> instance that represents the clone of the specified <paramref name="source"/>,
        /// or <see langword="null"/>, if the operation was canceled and <see cref="AsyncConfigBase.ThrowIfCanceled"/> property of the <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>This method is not a blocking call even if the <see cref="AsyncConfigBase.MaxDegreeOfParallelism"/> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="Clone(IReadableBitmapData, Rectangle, PixelFormat, IQuantizer, IDitherer)"/> method for more details.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="pixelFormat"/> does not specify a valid format.
        /// <br/>-or-
        /// <br/><paramref name="sourceRectangle"/> has no overlapping region with source bounds.</exception>
        /// <exception cref="ArgumentException"><paramref name="quantizer"/> uses a palette with too many colors for the specified <paramref name="pixelFormat"/>.</exception>
        /// <seealso cref="ImageExtensions.ConvertPixelFormatAsync(Image, PixelFormat, IQuantizer, IDitherer, TaskConfig)"/>
        public static Task<IReadWriteBitmapData?> CloneAsync(this IReadableBitmapData source, PixelFormat pixelFormat, IQuantizer? quantizer, IDitherer? ditherer = null, Rectangle? sourceRectangle = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(source, pixelFormat);
            return AsyncContext.DoOperationAsync(ctx => DoCloneWithQuantizer(ctx, source, sourceRectangle ?? new Rectangle(Point.Empty, source.GetSize()), pixelFormat, quantizer, ditherer), asyncConfig);
        }

#endif
        #endregion

        #endregion

        #region CopyTo

        #region Sync

        /// <summary>
        /// Copies the <paramref name="source"/>&#160;<see cref="IReadableBitmapData"/> into the <paramref name="target"/>&#160;<see cref="IWritableBitmapData"/>
        /// without scaling and blending. This method works between any pair of source and target <see cref="PixelFormat"/>s and supports quantizing and dithering.
        /// To draw a bitmap data into another one with blending use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.DrawInto">DrawInto</see> methods instead.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="source">The source <see cref="IReadableBitmapData"/> to be copied into the <paramref name="target"/>.</param>
        /// <param name="target">The target <see cref="IWritableBitmapData"/> into which <paramref name="source"/> should be copied.</param>
        /// <param name="targetLocation">The target location. Target size will be always the same as the source size. This parameter is optional.
        /// <br/>Default value: <see cref="Point.Empty">Point.Empty</see>.</param>
        /// <param name="quantizer">An <see cref="IQuantizer"/> instance to be used. If not specified, then the copying operation might automatically
        /// pick a quantizer based on <paramref name="target"/>&#160;<see cref="IBitmapData.PixelFormat"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="ditherer">The ditherer to be used. Might be ignored if <paramref name="quantizer"/> is not specified
        /// and <paramref name="target"/>&#160;<see cref="IBitmapData.PixelFormat"/> format has at least 24 bits-per-pixel size. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. Use the <see cref="BeginCopyTo">BeginCopyTo</see>
        /// or <see cref="CopyToAsync">CopyToAsync</see> (in .NET Framework 4.0 and above) methods for asynchronous call and to adjust parallelization, set up cancellation and for reporting progress.</note>
        /// <para>The copied area is automatically clipped if its size or <paramref name="targetLocation"/> makes it impossible to completely fit in the <paramref name="target"/>.</para>
        /// <para>If <paramref name="target"/> can represent a narrower set of colors, then the result will be automatically quantized to the colors of the <paramref name="target"/>,
        /// even if there is no <paramref name="quantizer"/> specified. To use dithering a <paramref name="ditherer"/> must be explicitly specified though.</para>
        /// <para>If <paramref name="quantizer"/> is specified but it uses more/different colors than <paramref name="target"/> can represent,
        /// then the result will eventually quantized to <paramref name="target"/>, though the result may have a poorer quality than expected.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="target"/> is <see langword="null"/>.</exception>
        public static void CopyTo(this IReadableBitmapData source, IWritableBitmapData target, Point targetLocation = default, IQuantizer? quantizer = null, IDitherer? ditherer = null)
            // ReSharper disable once ConstantConditionalAccessQualifier - needed to avoid NullReferenceException if source is null so ArgumentNullException is thrown from ValidateArguments
            => CopyTo(source, target, new Rectangle(Point.Empty, source?.GetSize() ?? default), targetLocation, quantizer, ditherer);

        /// <summary>
        /// Copies the <paramref name="source"/>&#160;<see cref="IReadableBitmapData"/> into the <paramref name="target"/>&#160;<see cref="IWritableBitmapData"/>
        /// without scaling and blending. This method works between any pair of source and target <see cref="PixelFormat"/>s and supports quantizing and dithering.
        /// To draw a bitmap data into another one with blending use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.DrawInto">DrawInto</see> methods instead.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="source">The source <see cref="IReadableBitmapData"/> to be copied into the <paramref name="target"/>.</param>
        /// <param name="target">The target <see cref="IWritableBitmapData"/> into which <paramref name="source"/> should be copied.</param>
        /// <param name="targetLocation">The target location. Target size will be always the same as the source size.</param>
        /// <param name="ditherer">The ditherer to be used. Might be ignored if <paramref name="target"/>&#160;<see cref="IBitmapData.PixelFormat"/> format has at least 24 bits-per-pixel size.</param>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. Use the <see cref="BeginCopyTo">BeginCopyTo</see>
        /// or <see cref="CopyToAsync">CopyToAsync</see> (in .NET Framework 4.0 and above) methods for asynchronous call and to adjust parallelization, set up cancellation and for reporting progress.</note>
        /// <para>The copied area is automatically clipped if its size or <paramref name="targetLocation"/> makes it impossible to completely fit in the <paramref name="target"/>.</para>
        /// <para>If <paramref name="target"/> can represent a narrower set of colors, then the result will be automatically quantized to the colors of the <paramref name="target"/>.
        /// To use dithering a <paramref name="ditherer"/> must be explicitly specified.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="target"/> is <see langword="null"/>.</exception>
        public static void CopyTo(this IReadableBitmapData source, IWritableBitmapData target, Point targetLocation, IDitherer? ditherer)
            // ReSharper disable once ConstantConditionalAccessQualifier - needed to avoid NullReferenceException if source is null so ArgumentNullException is thrown from ValidateArguments
            => CopyTo(source, target, new Rectangle(Point.Empty, source?.GetSize() ?? default), targetLocation, null, ditherer);

        /// <summary>
        /// Copies the <paramref name="source"/>&#160;<see cref="IReadableBitmapData"/> into the <paramref name="target"/>&#160;<see cref="IWritableBitmapData"/>
        /// without scaling and blending. This method works between any pair of source and target <see cref="PixelFormat"/>s and supports quantizing and dithering.
        /// To draw a bitmap data into another one with blending use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.DrawInto">DrawInto</see> methods instead.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="source">The source <see cref="IReadableBitmapData"/> to be copied into the <paramref name="target"/>.</param>
        /// <param name="target">The target <see cref="IWritableBitmapData"/> into which <paramref name="source"/> should be copied.</param>
        /// <param name="sourceRectangle">A <see cref="Rectangle"/> that specifies the portion of the <paramref name="source"/> to be copied into the <paramref name="target"/>.</param>
        /// <param name="targetLocation">The target location. Target size will be always the same as the source size.</param>
        /// <param name="ditherer">The ditherer to be used. Might be ignored if <paramref name="target"/>&#160;<see cref="IBitmapData.PixelFormat"/> format has at least 24 bits-per-pixel size.</param>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. Use the <see cref="BeginCopyTo">BeginCopyTo</see>
        /// or <see cref="CopyToAsync">CopyToAsync</see> (in .NET Framework 4.0 and above) methods for asynchronous call and to adjust parallelization, set up cancellation and for reporting progress.</note>
        /// <para>The copied area is automatically clipped if its size or <paramref name="targetLocation"/> makes it impossible to completely fit in the <paramref name="target"/>.</para>
        /// <para>If <paramref name="target"/> can represent a narrower set of colors, then the result will be automatically quantized to the colors of the <paramref name="target"/>.
        /// To use dithering a <paramref name="ditherer"/> must be explicitly specified.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="target"/> is <see langword="null"/>.</exception>
        public static void CopyTo(this IReadableBitmapData source, IWritableBitmapData target, Rectangle sourceRectangle, Point targetLocation, IDitherer? ditherer)
            => CopyTo(source, target, sourceRectangle, targetLocation, null, ditherer);

        /// <summary>
        /// Copies the <paramref name="source"/>&#160;<see cref="IReadableBitmapData"/> into the <paramref name="target"/>&#160;<see cref="IWritableBitmapData"/>
        /// without scaling and blending. This method works between any pair of source and target <see cref="PixelFormat"/>s and supports quantizing and dithering.
        /// To draw a bitmap data into another one with blending use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.DrawInto">DrawInto</see> methods instead.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="source">The source <see cref="IReadableBitmapData"/> to be copied into the <paramref name="target"/>.</param>
        /// <param name="target">The target <see cref="IWritableBitmapData"/> into which <paramref name="source"/> should be copied.</param>
        /// <param name="sourceRectangle">A <see cref="Rectangle"/> that specifies the portion of the <paramref name="source"/> to be copied into the <paramref name="target"/>.</param>
        /// <param name="targetLocation">The target location. Target size will be always the same as the source size.</param>
        /// <param name="quantizer">An <see cref="IQuantizer"/> instance to be used. If not specified, then the copying operation might automatically
        /// pick a quantizer based on <paramref name="target"/>&#160;<see cref="IBitmapData.PixelFormat"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="ditherer">The ditherer to be used. Might be ignored if <paramref name="quantizer"/> is not specified
        /// and <paramref name="target"/>&#160;<see cref="IBitmapData.PixelFormat"/> format has at least 24 bits-per-pixel size. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. Use the <see cref="BeginCopyTo">BeginCopyTo</see>
        /// or <see cref="CopyToAsync">CopyToAsync</see> (in .NET Framework 4.0 and above) methods for asynchronous call and to adjust parallelization, set up cancellation and for reporting progress.</note>
        /// <para>The copied area is automatically clipped if its size or <paramref name="targetLocation"/> makes it impossible to completely fit in the <paramref name="target"/>.</para>
        /// <para>If <paramref name="target"/> can represent a narrower set of colors, then the result will be automatically quantized to the colors of the <paramref name="target"/>,
        /// even if there is no <paramref name="quantizer"/> specified. To use dithering a <paramref name="ditherer"/> must be explicitly specified though.</para>
        /// <para>If <paramref name="quantizer"/> is specified but it uses more/different colors than <paramref name="target"/> can represent,
        /// then the result will eventually quantized to <paramref name="target"/>, though the result may have a poorer quality than expected.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="target"/> is <see langword="null"/>.</exception>
        public static void CopyTo(this IReadableBitmapData source, IWritableBitmapData target, Rectangle sourceRectangle, Point targetLocation, IQuantizer? quantizer = null, IDitherer? ditherer = null)
        {
            ValidateArguments(source, target);
            DoCopy(AsyncContext.Null, source, target, sourceRectangle, targetLocation, quantizer, ditherer);
        }

        #endregion

        #region Async APM

        /// <summary>
        /// Begins to copy the <paramref name="source"/>&#160;<see cref="IReadableBitmapData"/> into the <paramref name="target"/>&#160;<see cref="IWritableBitmapData"/> asynchronously,
        /// without scaling and blending. This method works between any pair of source and target <see cref="PixelFormat"/>s and supports quantizing and dithering.
        /// To draw a bitmap data into another one with blending use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.BeginDrawInto">BeginDrawInto</see> methods instead.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="source">The source <see cref="IReadableBitmapData"/> to be copied into the <paramref name="target"/>.</param>
        /// <param name="target">The target <see cref="IWritableBitmapData"/> into which <paramref name="source"/> should be copied.</param>
        /// <param name="sourceRectangle">A <see cref="Rectangle"/> that specifies the portion of the <paramref name="source"/> to be copied, or <see langword="null"/>&#160;to copy the entire <paramref name="source"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="targetLocation">A <see cref="Point"/> that specifies the target location, or <see langword="null"/>&#160;top copy the <paramref name="source"/> to the top-left corner of the <paramref name="target"/>. Target size will be always the same as the source size. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="quantizer">An <see cref="IQuantizer"/> instance to be used. If not specified, then the copying operation might automatically
        /// pick a quantizer based on <paramref name="target"/>&#160;<see cref="IBitmapData.PixelFormat"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="ditherer">The ditherer to be used. Might be ignored if <paramref name="quantizer"/> is not specified
        /// and <paramref name="target"/>&#160;<see cref="IBitmapData.PixelFormat"/> format has at least 24 bits-per-pixel size. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="CopyToAsync">CopyToAsync</see> method.</para>
        /// <para>To finish the operation and to get the exception that occurred during the operation you have to call the <see cref="EndCopyTo">EndCopyTo</see> method.</para>
        /// <para>This method is not a blocking call even if the <see cref="AsyncConfigBase.MaxDegreeOfParallelism"/> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="CopyTo(IReadableBitmapData, IWritableBitmapData, Rectangle, Point, IQuantizer, IDitherer)"/> method for more details.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="target"/> is <see langword="null"/>.</exception>
        public static IAsyncResult BeginCopyTo(this IReadableBitmapData source, IWritableBitmapData target, Rectangle? sourceRectangle = null, Point? targetLocation = null, IQuantizer? quantizer = null, IDitherer? ditherer = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(source, target);
            return AsyncContext.BeginOperation(ctx => DoCopy(ctx, source, target, sourceRectangle ?? new Rectangle(Point.Empty, source.GetSize()), targetLocation ?? Point.Empty, quantizer, ditherer), asyncConfig);
        }

        /// <summary>
        /// Waits for the pending asynchronous operation started by the <see cref="BeginCopyTo">BeginCopyTo</see> method to complete.
        /// In .NET Framework 4.0 and above you can use the <see cref="CopyToAsync">CopyToAsync</see> method instead.
        /// </summary>
        /// <param name="asyncResult">The reference to the pending asynchronous request to finish.</param>
        public static void EndCopyTo(this IAsyncResult asyncResult) => AsyncContext.EndOperation(asyncResult, nameof(BeginCopyTo));

        #endregion

        #region Async TAP
#if !NET35

        /// <summary>
        /// Copies the <paramref name="source"/>&#160;<see cref="IReadableBitmapData"/> into the <paramref name="target"/>&#160;<see cref="IWritableBitmapData"/> asynchronously,
        /// without scaling and blending. This method works between any pair of source and target <see cref="PixelFormat"/>s and supports quantizing and dithering.
        /// To draw a bitmap data into another one with blending use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.DrawIntoAsync">DrawIntoAsync</see> methods instead.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="source">The source <see cref="IReadableBitmapData"/> to be copied into the <paramref name="target"/>.</param>
        /// <param name="target">The target <see cref="IWritableBitmapData"/> into which <paramref name="source"/> should be copied.</param>
        /// <param name="sourceRectangle">A <see cref="Rectangle"/> that specifies the portion of the <paramref name="source"/> to be copied, or <see langword="null"/>&#160;to copy the entire <paramref name="source"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="targetLocation">A <see cref="Point"/> that specifies the target location, or <see langword="null"/>&#160;top copy the <paramref name="source"/> to the top-left corner of the <paramref name="target"/>. Target size will be always the same as the source size. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="quantizer">An <see cref="IQuantizer"/> instance to be used. If not specified, then the copying operation might automatically
        /// pick a quantizer based on <paramref name="target"/>&#160;<see cref="IBitmapData.PixelFormat"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="ditherer">The ditherer to be used. Might be ignored if <paramref name="quantizer"/> is not specified
        /// and <paramref name="target"/>&#160;<see cref="IBitmapData.PixelFormat"/> format has at least 24 bits-per-pixel size. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>This method is not a blocking call even if the <see cref="AsyncConfigBase.MaxDegreeOfParallelism"/> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="CopyTo(IReadableBitmapData, IWritableBitmapData, Rectangle, Point, IQuantizer, IDitherer)"/> method for more details.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="target"/> is <see langword="null"/>.</exception>
        public static Task CopyToAsync(this IReadableBitmapData source, IWritableBitmapData target, Rectangle? sourceRectangle = null, Point? targetLocation = null, IQuantizer? quantizer = null, IDitherer? ditherer = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(source, target);
            return AsyncContext.DoOperationAsync(ctx => DoCopy(ctx, source, target, sourceRectangle ?? new Rectangle(Point.Empty, source.GetSize()), targetLocation ?? Point.Empty, quantizer, ditherer), asyncConfig);
        }

#endif
        #endregion

        #endregion

        #region DrawInto

        #region Without resize

        #region Sync

        /// <summary>
        /// Draws the <paramref name="source"/>&#160;<see cref="IReadableBitmapData"/> into the <paramref name="target"/>&#160;<see cref="IReadWriteBitmapData"/>
        /// without scaling, using blending. This method is similar to <see cref="Graphics.DrawImage(Image,Point)">Graphics.DrawImage</see>
        /// methods, except that this one always preserves the source size in pixels, works between any pair of source and target <see cref="PixelFormat"/>s and supports quantizing and dithering.
        /// For scaling use the overloads with <c>targetRectangle</c> and <see cref="ScalingMode"/> parameters.
        /// To copy a bitmap data into another one without blending use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.CopyTo">CopyTo</see> methods instead.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="source">The source <see cref="IReadableBitmapData"/> to be drawn into the <paramref name="target"/>.</param>
        /// <param name="target">The target <see cref="IReadWriteBitmapData"/> into which <paramref name="source"/> should be drawn.</param>
        /// <param name="targetLocation">The target location. Target size will be always the same as the source size. This parameter is optional.
        /// <br/>Default value: <see cref="Point.Empty">Point.Empty</see>.</param>
        /// <param name="quantizer">An <see cref="IQuantizer"/> instance to be used for the drawing. If not specified, then the drawing operation might automatically
        /// pick a quantizer based on <paramref name="target"/>&#160;<see cref="IBitmapData.PixelFormat"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="ditherer">The ditherer to be used for the drawing. Might be ignored if <paramref name="quantizer"/> is not specified
        /// and <paramref name="target"/>&#160;<see cref="IBitmapData.PixelFormat"/> format has at least 24 bits-per-pixel size. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. Use the <see cref="BeginDrawInto(IReadableBitmapData, IReadWriteBitmapData, Rectangle?, Point?, IQuantizer, IDitherer, AsyncConfig)"/>
        /// or <see cref="DrawIntoAsync(IReadableBitmapData, IReadWriteBitmapData, Rectangle?, Point?, IQuantizer, IDitherer, TaskConfig)"/> (in .NET Framework 4.0 and above) methods for asynchronous call and to adjust parallelization, set up cancellation and for reporting progress.</note>
        /// <para>The image to be drawn is automatically clipped if its size or <paramref name="targetLocation"/> makes it impossible to completely fit in the <paramref name="target"/>.</para>
        /// <para><paramref name="target"/> must be an <see cref="IReadWriteBitmapData"/> because it must be readable if blending is necessary. For write-only <see cref="IWritableBitmapData"/> instances
        /// you can use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.CopyTo">CopyTo</see> methods.</para>
        /// <para>If <paramref name="target"/> can represent a narrower set of colors, then the result will be automatically quantized to the colors of the <paramref name="target"/>,
        /// even if there is no <paramref name="quantizer"/> specified. To use dithering a <paramref name="ditherer"/> must be explicitly specified though.</para>
        /// <para>If <paramref name="quantizer"/> is specified but it uses more/different colors than <paramref name="target"/> can represent,
        /// then the result will eventually quantized to <paramref name="target"/>, though the result may have a poorer quality than expected.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="target"/> is <see langword="null"/>.</exception>
        public static void DrawInto(this IReadableBitmapData source, IReadWriteBitmapData target, Point targetLocation = default, IQuantizer? quantizer = null, IDitherer? ditherer = null)
            // ReSharper disable once ConstantConditionalAccessQualifier - needed to avoid NullReferenceException if source is null so ArgumentNullException is thrown from ValidateArguments
            => DrawInto(source, target, new Rectangle(Point.Empty, source?.GetSize() ?? default), targetLocation, quantizer, ditherer);

        /// <summary>
        /// Draws the <paramref name="source"/>&#160;<see cref="IReadableBitmapData"/> into the <paramref name="target"/>&#160;<see cref="IReadWriteBitmapData"/>
        /// without scaling, using blending. This method is similar to <see cref="Graphics.DrawImage(Image,Point)">Graphics.DrawImage</see>
        /// methods, except that this one always preserves the source size in pixels, works between any pair of source and target <see cref="PixelFormat"/>s and supports quantizing and dithering.
        /// For scaling use the overloads with <c>targetRectangle</c> and <see cref="ScalingMode"/> parameters.
        /// To copy a bitmap data into another one without blending use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.CopyTo">CopyTo</see> methods instead.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="source">The source <see cref="IReadableBitmapData"/> to be drawn into the <paramref name="target"/>.</param>
        /// <param name="target">The target <see cref="IReadWriteBitmapData"/> into which <paramref name="source"/> should be drawn.</param>
        /// <param name="targetLocation">The target location. Target size will be always the same as the source size.</param>
        /// <param name="ditherer">The ditherer to be used for the drawing. Might be ignored if <paramref name="target"/>&#160;<see cref="IBitmapData.PixelFormat"/> format has at least 24 bits-per-pixel size.</param>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. Use the <see cref="BeginDrawInto(IReadableBitmapData, IReadWriteBitmapData, Rectangle?, Point?, IQuantizer, IDitherer, AsyncConfig)"/>
        /// or <see cref="DrawIntoAsync(IReadableBitmapData, IReadWriteBitmapData, Rectangle?, Point?, IQuantizer, IDitherer, TaskConfig)"/> (in .NET Framework 4.0 and above) methods for asynchronous call and to adjust parallelization, set up cancellation and for reporting progress.</note>
        /// <para>The image to be drawn is automatically clipped if its size or <paramref name="targetLocation"/> makes it impossible to completely fit in the <paramref name="target"/>.</para>
        /// <para><paramref name="target"/> must be an <see cref="IReadWriteBitmapData"/> because it must be readable if blending is necessary. For write-only <see cref="IWritableBitmapData"/> instances
        /// you can use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.CopyTo">CopyTo</see> methods.</para>
        /// <para>If <paramref name="target"/> can represent a narrower set of colors, then the result will be automatically quantized to the colors of the <paramref name="target"/>.
        /// To use dithering a <paramref name="ditherer"/> must be explicitly specified.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="target"/> is <see langword="null"/>.</exception>
        public static void DrawInto(this IReadableBitmapData source, IReadWriteBitmapData target, Point targetLocation, IDitherer? ditherer)
            // ReSharper disable once ConstantConditionalAccessQualifier - needed to avoid NullReferenceException if source is null so ArgumentNullException is thrown from ValidateArguments
            => DrawInto(source, target, new Rectangle(Point.Empty, source?.GetSize() ?? default), targetLocation, null, ditherer);

        /// <summary>
        /// Draws the <paramref name="source"/>&#160;<see cref="IReadableBitmapData"/> into the <paramref name="target"/>&#160;<see cref="IReadWriteBitmapData"/>
        /// without scaling, using blending. This method is similar to <see cref="Graphics.DrawImage(Image,Point)">Graphics.DrawImage</see>
        /// methods, except that this one always preserves the source size in pixels, works between any pair of source and target <see cref="PixelFormat"/>s and supports quantizing and dithering.
        /// For scaling use the overloads with <c>targetRectangle</c> and <see cref="ScalingMode"/> parameters.
        /// To copy a bitmap data into another one without blending use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.CopyTo">CopyTo</see> methods instead.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="source">The source <see cref="IReadableBitmapData"/> to be drawn into the <paramref name="target"/>.</param>
        /// <param name="target">The target <see cref="IReadWriteBitmapData"/> into which <paramref name="source"/> should be drawn.</param>
        /// <param name="sourceRectangle">A <see cref="Rectangle"/> that specifies the portion of the <paramref name="source"/> to be drawn into the <paramref name="target"/>.</param>
        /// <param name="targetLocation">The target location. Target size will be always the same as the source size.</param>
        /// <param name="ditherer">The ditherer to be used for the drawing. Might be ignored if <paramref name="target"/>&#160;<see cref="IBitmapData.PixelFormat"/> format has at least 24 bits-per-pixel size.</param>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. Use the <see cref="BeginDrawInto(IReadableBitmapData, IReadWriteBitmapData, Rectangle?, Point?, IQuantizer, IDitherer, AsyncConfig)"/>
        /// or <see cref="DrawIntoAsync(IReadableBitmapData, IReadWriteBitmapData, Rectangle?, Point?, IQuantizer, IDitherer, TaskConfig)"/> (in .NET Framework 4.0 and above) methods for asynchronous call and to adjust parallelization, set up cancellation and for reporting progress.</note>
        /// <para>The image to be drawn is automatically clipped if its size or <paramref name="targetLocation"/> makes it impossible to completely fit in the <paramref name="target"/>.</para>
        /// <para><paramref name="target"/> must be an <see cref="IReadWriteBitmapData"/> because it must be readable if blending is necessary. For write-only <see cref="IWritableBitmapData"/> instances
        /// you can use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.CopyTo">CopyTo</see> methods.</para>
        /// <para>If <paramref name="target"/> can represent a narrower set of colors, then the result will be automatically quantized to the colors of the <paramref name="target"/>.
        /// To use dithering a <paramref name="ditherer"/> must be explicitly specified.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="target"/> is <see langword="null"/>.</exception>
        public static void DrawInto(this IReadableBitmapData source, IReadWriteBitmapData target, Rectangle sourceRectangle, Point targetLocation, IDitherer? ditherer)
            => DrawInto(source, target, sourceRectangle, targetLocation, null, ditherer);

        /// <summary>
        /// Draws the <paramref name="source"/>&#160;<see cref="IReadableBitmapData"/> into the <paramref name="target"/>&#160;<see cref="IReadWriteBitmapData"/>
        /// without scaling, using blending. This method is similar to <see cref="Graphics.DrawImage(Image,Point)">Graphics.DrawImage</see>
        /// methods, except that this one always preserves the source size in pixels, works between any pair of source and target <see cref="PixelFormat"/>s and supports quantizing and dithering.
        /// For scaling use the overloads with <c>targetRectangle</c> and <see cref="ScalingMode"/> parameters.
        /// To copy a bitmap data into another one without blending use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.CopyTo">CopyTo</see> methods instead.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="source">The source <see cref="IReadableBitmapData"/> to be drawn into the <paramref name="target"/>.</param>
        /// <param name="target">The target <see cref="IReadWriteBitmapData"/> into which <paramref name="source"/> should be drawn.</param>
        /// <param name="sourceRectangle">A <see cref="Rectangle"/> that specifies the portion of the <paramref name="source"/> to be drawn into the <paramref name="target"/>.</param>
        /// <param name="targetLocation">The target location. Target size will be always the same as the source size.</param>
        /// <param name="quantizer">An <see cref="IQuantizer"/> instance to be used for the drawing. If not specified, then the drawing operation might automatically
        /// pick a quantizer based on <paramref name="target"/>&#160;<see cref="IBitmapData.PixelFormat"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="ditherer">The ditherer to be used for the drawing. Might be ignored if <paramref name="quantizer"/> is not specified
        /// and <paramref name="target"/>&#160;<see cref="IBitmapData.PixelFormat"/> format has at least 24 bits-per-pixel size. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. Use the <see cref="BeginDrawInto(IReadableBitmapData, IReadWriteBitmapData, Rectangle?, Point?, IQuantizer, IDitherer, AsyncConfig)"/>
        /// or <see cref="DrawIntoAsync(IReadableBitmapData, IReadWriteBitmapData, Rectangle?, Point?, IQuantizer, IDitherer, TaskConfig)"/> (in .NET Framework 4.0 and above) methods for asynchronous call and to adjust parallelization, set up cancellation and for reporting progress.</note>
        /// <para>The image to be drawn is automatically clipped if its size or <paramref name="targetLocation"/> makes it impossible to completely fit in the <paramref name="target"/>.</para>
        /// <para><paramref name="target"/> must be an <see cref="IReadWriteBitmapData"/> because it must be readable if blending is necessary. For write-only <see cref="IWritableBitmapData"/> instances
        /// you can use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.CopyTo">CopyTo</see> methods.</para>
        /// <para>If <paramref name="target"/> can represent a narrower set of colors, then the result will be automatically quantized to the colors of the <paramref name="target"/>,
        /// even if there is no <paramref name="quantizer"/> specified. To use dithering a <paramref name="ditherer"/> must be explicitly specified though.</para>
        /// <para>If <paramref name="quantizer"/> is specified but it uses more/different colors than <paramref name="target"/> can represent,
        /// then the result will eventually quantized to <paramref name="target"/>, though the result may have a poorer quality than expected.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="target"/> is <see langword="null"/>.</exception>
        public static void DrawInto(this IReadableBitmapData source, IReadWriteBitmapData target, Rectangle sourceRectangle, Point targetLocation, IQuantizer? quantizer = null, IDitherer? ditherer = null)
        {
            ValidateArguments(source, target);
            DoDrawInto(AsyncContext.Null, source, target, sourceRectangle, targetLocation, quantizer, ditherer);
        }

        #endregion

        #region Async APM

        /// <summary>
        /// Begins to draw the <paramref name="source"/>&#160;<see cref="IReadableBitmapData"/> into the <paramref name="target"/>&#160;<see cref="IReadWriteBitmapData"/> asynchronously,
        /// without scaling, using blending. This method is similar to <see cref="Graphics.DrawImage(Image,Point)">Graphics.DrawImage</see>
        /// methods, except that this one always preserves the source size in pixels, works between any pair of source and target <see cref="PixelFormat"/>s and supports quantizing and dithering.
        /// For scaling use the <see cref="BeginDrawInto(IReadableBitmapData, IReadWriteBitmapData, Rectangle, Rectangle, IQuantizer, IDitherer, ScalingMode, AsyncConfig)"/> overload.
        /// To copy a bitmap data into another one without blending use the <see cref="BeginCopyTo">BeginCopyTo</see> method instead.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="source">The source <see cref="IReadableBitmapData"/> to be drawn into the <paramref name="target"/>.</param>
        /// <param name="target">The target <see cref="IReadWriteBitmapData"/> into which <paramref name="source"/> should be drawn.</param>
        /// <param name="sourceRectangle">A <see cref="Rectangle"/> that specifies the portion of the <paramref name="source"/> to be drawn into the <paramref name="target"/>, or <see langword="null"/>&#160;to draw the entire <paramref name="source"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="targetLocation">A <see cref="Point"/> that specifies the target location, or <see langword="null"/>&#160;top draw the <paramref name="source"/> to the top-left corner of the <paramref name="target"/>. Target size will be always the same as the source size. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="quantizer">An <see cref="IQuantizer"/> instance to be used for the drawing. If not specified, then the drawing operation might automatically
        /// pick a quantizer based on <paramref name="target"/>&#160;<see cref="IBitmapData.PixelFormat"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="ditherer">The ditherer to be used for the drawing. Might be ignored if <paramref name="quantizer"/> is not specified
        /// and <paramref name="target"/>&#160;<see cref="IBitmapData.PixelFormat"/> format has at least 24 bits-per-pixel size. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="DrawIntoAsync(IReadableBitmapData, IReadWriteBitmapData, Rectangle?, Point?, IQuantizer, IDitherer, TaskConfig)"/> method.</para>
        /// <para>To finish the operation and to get the exception that occurred during the operation you have to call the <see cref="EndDrawInto">EndDrawInto</see> method.</para>
        /// <para>This method is not a blocking call even if the <see cref="AsyncConfigBase.MaxDegreeOfParallelism"/> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="DrawInto(IReadableBitmapData, IReadWriteBitmapData, Rectangle, Point, IQuantizer, IDitherer)"/> method for more details.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="target"/> is <see langword="null"/>.</exception>
        public static IAsyncResult BeginDrawInto(this IReadableBitmapData source, IReadWriteBitmapData target, Rectangle? sourceRectangle = null, Point? targetLocation = null, IQuantizer? quantizer = null, IDitherer? ditherer = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(source, target);
            return AsyncContext.BeginOperation(ctx => DoDrawInto(ctx, source, target, sourceRectangle ?? new Rectangle(Point.Empty, source.GetSize()), targetLocation ?? Point.Empty, quantizer, ditherer), asyncConfig);
        }

        /// <summary>
        /// Waits for the pending asynchronous operation started by the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.BeginDrawInto">BeginDrawInto</see> methods to complete.
        /// In .NET Framework 4.0 and above you can use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.DrawIntoAsync">DrawIntoAsync</see> methods instead.
        /// </summary>
        /// <param name="asyncResult">The reference to the pending asynchronous request to finish.</param>
        public static void EndDrawInto(this IAsyncResult asyncResult) => AsyncContext.EndOperation(asyncResult, nameof(BeginDrawInto));

        #endregion

        #region Async TAP
#if !NET35

        /// <summary>
        /// Draws the <paramref name="source"/>&#160;<see cref="IReadableBitmapData"/> into the <paramref name="target"/>&#160;<see cref="IReadWriteBitmapData"/> asynchronously,
        /// without scaling, using blending. This method is similar to <see cref="Graphics.DrawImage(Image,Point)">Graphics.DrawImage</see>
        /// methods, except that this one always preserves the source size in pixels, works between any pair of source and target <see cref="PixelFormat"/>s and supports quantizing and dithering.
        /// For scaling use the <see cref="DrawIntoAsync(IReadableBitmapData, IReadWriteBitmapData, Rectangle, Rectangle, IQuantizer, IDitherer, ScalingMode, TaskConfig)"/> overload.
        /// To copy a bitmap data into another one without blending use the <see cref="CopyToAsync">CopyToAsync</see> method instead.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="source">The source <see cref="IReadableBitmapData"/> to be drawn into the <paramref name="target"/>.</param>
        /// <param name="target">The target <see cref="IReadWriteBitmapData"/> into which <paramref name="source"/> should be drawn.</param>
        /// <param name="sourceRectangle">A <see cref="Rectangle"/> that specifies the portion of the <paramref name="source"/> to be drawn into the <paramref name="target"/>, or <see langword="null"/>&#160;to draw the entire <paramref name="source"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="targetLocation">A <see cref="Point"/> that specifies the target location, or <see langword="null"/>&#160;top draw the <paramref name="source"/> to the top-left corner of the <paramref name="target"/>. Target size will be always the same as the source size. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="quantizer">An <see cref="IQuantizer"/> instance to be used for the drawing. If not specified, then the drawing operation might automatically
        /// pick a quantizer based on <paramref name="target"/>&#160;<see cref="IBitmapData.PixelFormat"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="ditherer">The ditherer to be used for the drawing. Might be ignored if <paramref name="quantizer"/> is not specified
        /// and <paramref name="target"/>&#160;<see cref="IBitmapData.PixelFormat"/> format has at least 24 bits-per-pixel size. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>This method is not a blocking call even if the <see cref="AsyncConfigBase.MaxDegreeOfParallelism"/> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="DrawInto(IReadableBitmapData, IReadWriteBitmapData, Rectangle, Point, IQuantizer, IDitherer)"/> method for more details.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="target"/> is <see langword="null"/>.</exception>
        public static Task DrawIntoAsync(this IReadableBitmapData source, IReadWriteBitmapData target, Rectangle? sourceRectangle = null, Point? targetLocation = null, IQuantizer? quantizer = null, IDitherer? ditherer = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(source, target);
            Rectangle srcRect = sourceRectangle ?? new Rectangle(Point.Empty, source.GetSize());
            Point dstLoc = targetLocation ?? Point.Empty;
            return source.HasAlpha()
                ? AsyncContext.DoOperationAsync(ctx => DoDrawWithoutResize(ctx, source, target, srcRect, dstLoc, quantizer, ditherer), asyncConfig)
                : AsyncContext.DoOperationAsync(ctx => DoCopy(ctx, source, target, srcRect, dstLoc, quantizer, ditherer), asyncConfig);
        }

#endif
        #endregion

        #endregion

        #region With resize

        #region Sync

        /// <summary>
        /// Draws the <paramref name="source"/>&#160;<see cref="IReadableBitmapData"/> into the <paramref name="target"/>&#160;<see cref="IReadWriteBitmapData"/>
        /// using scaling and blending. This method is similar to <see cref="O:System.Drawing.Graphics.DrawImage">Graphics.DrawImage</see>
        /// methods, except that this one works between any pair of source and target <see cref="PixelFormat"/>s and supports quantizing and dithering.
        /// To copy a bitmap data into another one without blending use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.CopyTo">CopyTo</see> methods instead.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="source">The source <see cref="IReadableBitmapData"/> to be drawn into the <paramref name="target"/>.</param>
        /// <param name="target">The target <see cref="IReadWriteBitmapData"/> into which <paramref name="source"/> should be drawn.</param>
        /// <param name="targetRectangle">A <see cref="Rectangle"/> that specifies the location and size of the drawn <paramref name="source"/>.</param>
        /// <param name="quantizer">An <see cref="IQuantizer"/> instance to be used for the drawing. If not specified, then the drawing operation might automatically
        /// pick a quantizer based on <paramref name="target"/>&#160;<see cref="IBitmapData.PixelFormat"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="ditherer">The ditherer to be used for the drawing. Might be ignored if <paramref name="quantizer"/> is not specified
        /// and <paramref name="target"/>&#160;<see cref="IBitmapData.PixelFormat"/> format has at least 24 bits-per-pixel size. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="scalingMode">Specifies the scaling mode if the bitmap data to be drawn needs to be resized. This parameter is optional.
        /// <br/>Default value: <see cref="ScalingMode.Auto"/>.</param>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. Use the <see cref="BeginDrawInto(IReadableBitmapData, IReadWriteBitmapData, Rectangle, Rectangle, IQuantizer, IDitherer, ScalingMode, AsyncConfig)"/>
        /// or <see cref="DrawIntoAsync(IReadableBitmapData, IReadWriteBitmapData, Rectangle, Rectangle, IQuantizer, IDitherer, ScalingMode, TaskConfig)"/> (in .NET Framework 4.0 and above) methods for asynchronous call and to adjust parallelization, set up cancellation and for reporting progress.</note>
        /// <para>The method has the best performance if <paramref name="source"/> and <paramref name="targetRectangle"/> have the same size, or when <paramref name="scalingMode"/> is <see cref="ScalingMode.NoScaling"/>.</para>
        /// <para>The image to be drawn is automatically clipped if <paramref name="targetRectangle"/> exceeds bounds, or <paramref name="scalingMode"/> is <see cref="ScalingMode.NoScaling"/>
        /// and <paramref name="source"/> and <paramref name="targetRectangle"/> have different sizes.</para>
        /// <para><paramref name="target"/> must be an <see cref="IReadWriteBitmapData"/> because it must be readable if blending is necessary. For write-only <see cref="IWritableBitmapData"/> instances
        /// you can use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.CopyTo">CopyTo</see> methods (without scaling).</para>
        /// <para>If <paramref name="target"/> can represent a narrower set of colors, then the result will be automatically quantized to the colors of the <paramref name="target"/>,
        /// even if there is no <paramref name="quantizer"/> specified. To use dithering a <paramref name="ditherer"/> must be explicitly specified though.</para>
        /// <para>If <paramref name="quantizer"/> is specified but it uses more/different colors than <paramref name="target"/> can represent,
        /// then the result will eventually quantized to <paramref name="target"/>, though the result may have a poorer quality than expected.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="target"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="scalingMode"/> has an unsupported value.</exception>
        public static void DrawInto(this IReadableBitmapData source, IReadWriteBitmapData target, Rectangle targetRectangle, IQuantizer? quantizer = null, IDitherer? ditherer = null, ScalingMode scalingMode = ScalingMode.Auto)
            // ReSharper disable once ConstantConditionalAccessQualifier - needed to avoid NullReferenceException if source is null so ArgumentNullException is thrown from ValidateArguments
            => DrawInto(source, target, new Rectangle(Point.Empty, source?.GetSize() ?? default), targetRectangle, quantizer, ditherer, scalingMode);

        /// <summary>
        /// Draws the <paramref name="source"/>&#160;<see cref="IReadableBitmapData"/> into the <paramref name="target"/>&#160;<see cref="IReadWriteBitmapData"/>
        /// using scaling and blending. This method is similar to <see cref="O:System.Drawing.Graphics.DrawImage">Graphics.DrawImage</see>
        /// methods, except that this one works between any pair of source and target <see cref="PixelFormat"/>s and supports quantizing and dithering.
        /// To copy a bitmap data into another one without blending use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.CopyTo">CopyTo</see> methods instead.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="source">The source <see cref="IReadableBitmapData"/> to be drawn into the <paramref name="target"/>.</param>
        /// <param name="target">The target <see cref="IReadWriteBitmapData"/> into which <paramref name="source"/> should be drawn.</param>
        /// <param name="targetRectangle">A <see cref="Rectangle"/> that specifies the location and size of the drawn <paramref name="source"/>.</param>
        /// <param name="ditherer">The ditherer to be used for the drawing. Might be ignored if <paramref name="target"/>&#160;<see cref="IBitmapData.PixelFormat"/> format has at least 24 bits-per-pixel size.</param>
        /// <param name="scalingMode">Specifies the scaling mode if the bitmap data to be drawn needs to be resized. This parameter is optional.
        /// <br/>Default value: <see cref="ScalingMode.Auto"/>.</param>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. Use the <see cref="BeginDrawInto(IReadableBitmapData, IReadWriteBitmapData, Rectangle, Rectangle, IQuantizer, IDitherer, ScalingMode, AsyncConfig)"/>
        /// or <see cref="DrawIntoAsync(IReadableBitmapData, IReadWriteBitmapData, Rectangle, Rectangle, IQuantizer, IDitherer, ScalingMode, TaskConfig)"/> (in .NET Framework 4.0 and above) methods for asynchronous call and to adjust parallelization, set up cancellation and for reporting progress.</note>
        /// <para>The method has the best performance if <paramref name="source"/> and <paramref name="targetRectangle"/> have the same size, or when <paramref name="scalingMode"/> is <see cref="ScalingMode.NoScaling"/>.</para>
        /// <para>The image to be drawn is automatically clipped if <paramref name="targetRectangle"/> exceeds bounds, or <paramref name="scalingMode"/> is <see cref="ScalingMode.NoScaling"/>
        /// and <paramref name="source"/> and <paramref name="targetRectangle"/> have different sizes.</para>
        /// <para><paramref name="target"/> must be an <see cref="IReadWriteBitmapData"/> because it must be readable if blending is necessary. For write-only <see cref="IWritableBitmapData"/> instances
        /// you can use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.CopyTo">CopyTo</see> methods (without scaling).</para>
        /// <para>If <paramref name="target"/> can represent a narrower set of colors, then the result will be automatically quantized to the colors of the <paramref name="target"/>.
        /// To use dithering a <paramref name="ditherer"/> must be explicitly specified.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="target"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="scalingMode"/> has an unsupported value.</exception>
        public static void DrawInto(this IReadableBitmapData source, IReadWriteBitmapData target, Rectangle targetRectangle, IDitherer? ditherer, ScalingMode scalingMode = ScalingMode.Auto)
            // ReSharper disable once ConstantConditionalAccessQualifier - needed to avoid NullReferenceException if source is null so ArgumentNullException is thrown from ValidateArguments
            => DrawInto(source, target, new Rectangle(Point.Empty, source?.GetSize() ?? default), targetRectangle, null, ditherer, scalingMode);

        /// <summary>
        /// Draws the <paramref name="source"/>&#160;<see cref="IReadableBitmapData"/> into the <paramref name="target"/>&#160;<see cref="IReadWriteBitmapData"/>
        /// using scaling and blending. This method is similar to <see cref="O:System.Drawing.Graphics.DrawImage">Graphics.DrawImage</see>
        /// methods, except that this one works between any pair of source and target <see cref="PixelFormat"/>s and supports quantizing.
        /// To copy a bitmap data into another one without blending use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.CopyTo">CopyTo</see> methods instead.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="source">The source <see cref="IReadableBitmapData"/> to be drawn into the <paramref name="target"/>.</param>
        /// <param name="target">The target <see cref="IReadWriteBitmapData"/> into which <paramref name="source"/> should be drawn.</param>
        /// <param name="targetRectangle">A <see cref="Rectangle"/> that specifies the location and size of the drawn <paramref name="source"/>.</param>
        /// <param name="scalingMode">Specifies the scaling mode if the bitmap data to be drawn needs to be resized.</param>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. Use the <see cref="BeginDrawInto(IReadableBitmapData, IReadWriteBitmapData, Rectangle, Rectangle, IQuantizer, IDitherer, ScalingMode, AsyncConfig)"/>
        /// or <see cref="DrawIntoAsync(IReadableBitmapData, IReadWriteBitmapData, Rectangle, Rectangle, IQuantizer, IDitherer, ScalingMode, TaskConfig)"/> (in .NET Framework 4.0 and above) methods for asynchronous call and to adjust parallelization, set up cancellation and for reporting progress.</note>
        /// <para>The method has the best performance if <paramref name="source"/> and <paramref name="targetRectangle"/> have the same size, or when <paramref name="scalingMode"/> is <see cref="ScalingMode.NoScaling"/>.</para>
        /// <para>The image to be drawn is automatically clipped if <paramref name="targetRectangle"/> exceeds bounds, or <paramref name="scalingMode"/> is <see cref="ScalingMode.NoScaling"/>
        /// and <paramref name="source"/> and <paramref name="targetRectangle"/> have different sizes.</para>
        /// <para><paramref name="target"/> must be an <see cref="IReadWriteBitmapData"/> because it must be readable if blending is necessary. For write-only <see cref="IWritableBitmapData"/> instances
        /// you can use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.CopyTo">CopyTo</see> methods (without scaling).</para>
        /// <para>If <paramref name="target"/> can represent a narrower set of colors, then the result will be automatically quantized to the colors of the <paramref name="target"/>.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="target"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="scalingMode"/> has an unsupported value.</exception>
        public static void DrawInto(this IReadableBitmapData source, IReadWriteBitmapData target, Rectangle targetRectangle, ScalingMode scalingMode)
            // ReSharper disable once ConstantConditionalAccessQualifier - needed to avoid NullReferenceException if source is null so ArgumentNullException is thrown from ValidateArguments
            => DrawInto(source, target, new Rectangle(Point.Empty, source?.GetSize() ?? default), targetRectangle, null, null, scalingMode);

        /// <summary>
        /// Draws the <paramref name="source"/>&#160;<see cref="IReadableBitmapData"/> into the <paramref name="target"/>&#160;<see cref="IReadWriteBitmapData"/>
        /// using scaling and blending. This method is similar to <see cref="O:System.Drawing.Graphics.DrawImage">Graphics.DrawImage</see>
        /// methods, except that this one works between any pair of source and target <see cref="PixelFormat"/>s and supports quantizing.
        /// To copy a bitmap data into another one without blending use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.CopyTo">CopyTo</see> methods instead.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="source">The source <see cref="IReadableBitmapData"/> to be drawn into the <paramref name="target"/>.</param>
        /// <param name="target">The target <see cref="IReadWriteBitmapData"/> into which <paramref name="source"/> should be drawn.</param>
        /// <param name="sourceRectangle">A <see cref="Rectangle"/> that specifies the portion of the <paramref name="source"/> to be drawn into the <paramref name="target"/>.</param>
        /// <param name="targetRectangle">A <see cref="Rectangle"/> that specifies the location and size of the drawn <paramref name="source"/>.</param>
        /// <param name="scalingMode">Specifies the scaling mode if the bitmap data to be drawn needs to be resized.</param>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. Use the <see cref="BeginDrawInto(IReadableBitmapData, IReadWriteBitmapData, Rectangle, Rectangle, IQuantizer, IDitherer, ScalingMode, AsyncConfig)"/>
        /// or <see cref="DrawIntoAsync(IReadableBitmapData, IReadWriteBitmapData, Rectangle, Rectangle, IQuantizer, IDitherer, ScalingMode, TaskConfig)"/> (in .NET Framework 4.0 and above) methods for asynchronous call and to adjust parallelization, set up cancellation and for reporting progress.</note>
        /// <para>The method has the best performance if <paramref name="sourceRectangle"/> and <paramref name="targetRectangle"/> have the same size, or when <paramref name="scalingMode"/> is <see cref="ScalingMode.NoScaling"/>.</para>
        /// <para>The image to be drawn is automatically clipped if <paramref name="sourceRectangle"/> or <paramref name="targetRectangle"/> exceed bounds, or <paramref name="scalingMode"/> is <see cref="ScalingMode.NoScaling"/>
        /// and <paramref name="sourceRectangle"/> and <paramref name="targetRectangle"/> are different.</para>
        /// <para><paramref name="target"/> must be an <see cref="IReadWriteBitmapData"/> because it must be readable if blending is necessary. For write-only <see cref="IWritableBitmapData"/> instances
        /// you can use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.CopyTo">CopyTo</see> methods (without scaling).</para>
        /// <para>If <paramref name="target"/> can represent a narrower set of colors, then the result will be automatically quantized to the colors of the <paramref name="target"/>.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="target"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="scalingMode"/> has an unsupported value.</exception>
        public static void DrawInto(this IReadableBitmapData source, IReadWriteBitmapData target, Rectangle sourceRectangle, Rectangle targetRectangle, ScalingMode scalingMode)
            => DrawInto(source, target, sourceRectangle, targetRectangle, null, null, scalingMode);

        /// <summary>
        /// Draws the <paramref name="source"/>&#160;<see cref="IReadableBitmapData"/> into the <paramref name="target"/>&#160;<see cref="IReadWriteBitmapData"/>
        /// using scaling and blending. This method is similar to <see cref="O:System.Drawing.Graphics.DrawImage">Graphics.DrawImage</see>
        /// methods, except that this one works between any pair of source and target <see cref="PixelFormat"/>s and supports quantizing and dithering.
        /// To copy a bitmap data into another one without blending use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.CopyTo">CopyTo</see> methods instead.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="source">The source <see cref="IReadableBitmapData"/> to be drawn into the <paramref name="target"/>.</param>
        /// <param name="target">The target <see cref="IReadWriteBitmapData"/> into which <paramref name="source"/> should be drawn.</param>
        /// <param name="sourceRectangle">A <see cref="Rectangle"/> that specifies the portion of the <paramref name="source"/> to be drawn into the <paramref name="target"/>.</param>
        /// <param name="targetRectangle">A <see cref="Rectangle"/> that specifies the location and size of the drawn <paramref name="source"/>.</param>
        /// <param name="ditherer">The ditherer to be used for the drawing. Might be ignored if <paramref name="target"/>&#160;<see cref="IBitmapData.PixelFormat"/>
        /// format has at least 24 bits-per-pixel size.</param>
        /// <param name="scalingMode">Specifies the scaling mode if the bitmap data to be drawn needs to be resized. This parameter is optional.
        /// <br/>Default value: <see cref="ScalingMode.Auto"/>.</param>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. Use the <see cref="BeginDrawInto(IReadableBitmapData, IReadWriteBitmapData, Rectangle, Rectangle, IQuantizer, IDitherer, ScalingMode, AsyncConfig)"/>
        /// or <see cref="DrawIntoAsync(IReadableBitmapData, IReadWriteBitmapData, Rectangle, Rectangle, IQuantizer, IDitherer, ScalingMode, TaskConfig)"/> (in .NET Framework 4.0 and above) methods for asynchronous call and to adjust parallelization, set up cancellation and for reporting progress.</note>
        /// <para>The method has the best performance if <paramref name="sourceRectangle"/> and <paramref name="targetRectangle"/> have the same size, or when <paramref name="scalingMode"/> is <see cref="ScalingMode.NoScaling"/>.</para>
        /// <para>The image to be drawn is automatically clipped if <paramref name="sourceRectangle"/> or <paramref name="targetRectangle"/> exceed bounds, or <paramref name="scalingMode"/> is <see cref="ScalingMode.NoScaling"/>
        /// and <paramref name="sourceRectangle"/> and <paramref name="targetRectangle"/> are different.</para>
        /// <para><paramref name="target"/> must be an <see cref="IReadWriteBitmapData"/> because it must be readable if blending is necessary. For write-only <see cref="IWritableBitmapData"/> instances
        /// you can use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.CopyTo">CopyTo</see> methods (without scaling).</para>
        /// <para>If <paramref name="target"/> can represent a narrower set of colors, then the result will be automatically quantized to the colors of the <paramref name="target"/>.
        /// To use dithering a <paramref name="ditherer"/> must be explicitly specified.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="target"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="scalingMode"/> has an unsupported value.</exception>
        public static void DrawInto(this IReadableBitmapData source, IReadWriteBitmapData target, Rectangle sourceRectangle, Rectangle targetRectangle, IDitherer? ditherer, ScalingMode scalingMode = ScalingMode.Auto)
            => DrawInto(source, target, sourceRectangle, targetRectangle, null, ditherer, scalingMode);

        /// <summary>
        /// Draws the <paramref name="source"/>&#160;<see cref="IReadableBitmapData"/> into the <paramref name="target"/>&#160;<see cref="IReadWriteBitmapData"/>
        /// using scaling and blending. This method is similar to <see cref="O:System.Drawing.Graphics.DrawImage">Graphics.DrawImage</see>
        /// methods, except that this one works between any pair of source and target <see cref="PixelFormat"/>s and supports quantizing and dithering.
        /// To copy a bitmap data into another one without blending use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.CopyTo">CopyTo</see> methods instead.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="source">The source <see cref="IReadableBitmapData"/> to be drawn into the <paramref name="target"/>.</param>
        /// <param name="target">The target <see cref="IReadWriteBitmapData"/> into which <paramref name="source"/> should be drawn.</param>
        /// <param name="sourceRectangle">A <see cref="Rectangle"/> that specifies the portion of the <paramref name="source"/> to be drawn into the <paramref name="target"/>.</param>
        /// <param name="targetRectangle">A <see cref="Rectangle"/> that specifies the location and size of the drawn <paramref name="source"/>.</param>
        /// <param name="quantizer">An <see cref="IQuantizer"/> instance to be used for the drawing. If not specified, then the drawing operation might automatically
        /// pick a quantizer based on <paramref name="target"/>&#160;<see cref="IBitmapData.PixelFormat"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="ditherer">The ditherer to be used for the drawing. Might be ignored if <paramref name="quantizer"/> is not specified
        /// and <paramref name="target"/>&#160;<see cref="IBitmapData.PixelFormat"/> format has at least 24 bits-per-pixel size. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="scalingMode">Specifies the scaling mode if the bitmap data to be drawn needs to be resized. This parameter is optional.
        /// <br/>Default value: <see cref="ScalingMode.Auto"/>.</param>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. Use the <see cref="BeginDrawInto(IReadableBitmapData, IReadWriteBitmapData, Rectangle, Rectangle, IQuantizer, IDitherer, ScalingMode, AsyncConfig)"/>
        /// or <see cref="DrawIntoAsync(IReadableBitmapData, IReadWriteBitmapData, Rectangle, Rectangle, IQuantizer, IDitherer, ScalingMode, TaskConfig)"/> (in .NET Framework 4.0 and above) methods for asynchronous call and to adjust parallelization, set up cancellation and for reporting progress.</note>
        /// <para>The method has the best performance if <paramref name="sourceRectangle"/> and <paramref name="targetRectangle"/> have the same size, or when <paramref name="scalingMode"/> is <see cref="ScalingMode.NoScaling"/>.</para>
        /// <para>The image to be drawn is automatically clipped if <paramref name="sourceRectangle"/> or <paramref name="targetRectangle"/> exceed bounds, or <paramref name="scalingMode"/> is <see cref="ScalingMode.NoScaling"/>
        /// and <paramref name="sourceRectangle"/> and <paramref name="targetRectangle"/> are different.</para>
        /// <para><paramref name="target"/> must be an <see cref="IReadWriteBitmapData"/> because it must be readable if blending is necessary. For write-only <see cref="IWritableBitmapData"/> instances
        /// you can use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.CopyTo">CopyTo</see> methods (without scaling).</para>
        /// <para>If <paramref name="target"/> can represent a narrower set of colors, then the result will be automatically quantized to the colors of the <paramref name="target"/>,
        /// even if there is no <paramref name="quantizer"/> specified. To use dithering a <paramref name="ditherer"/> must be explicitly specified though.</para>
        /// <para>If <paramref name="quantizer"/> is specified but it uses more/different colors than <paramref name="target"/> can represent,
        /// then the result will eventually quantized to <paramref name="target"/>, though the result may have a poorer quality than expected.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="target"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="scalingMode"/> has an unsupported value.</exception>
        public static void DrawInto(this IReadableBitmapData source, IReadWriteBitmapData target, Rectangle sourceRectangle, Rectangle targetRectangle, IQuantizer? quantizer = null, IDitherer? ditherer = null, ScalingMode scalingMode = ScalingMode.Auto)
        {
            ValidateArguments(source, target, scalingMode);
            DoDrawInto(AsyncContext.Null, source, target, sourceRectangle, targetRectangle, quantizer, ditherer, scalingMode);
        }

        #endregion

        #region Async APM

        /// <summary>
        /// Begins to draw the <paramref name="source"/>&#160;<see cref="IReadableBitmapData"/> into the <paramref name="target"/>&#160;<see cref="IReadWriteBitmapData"/> asynchronously,
        /// using scaling and blending. This method is similar to <see cref="O:System.Drawing.Graphics.DrawImage">Graphics.DrawImage</see>
        /// methods, except that this one works between any pair of source and target <see cref="PixelFormat"/>s and supports quantizing and dithering.
        /// To copy a bitmap data into another one without blending use the <see cref="BeginCopyTo">BeginCopyTo</see> method instead.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="source">The source <see cref="IReadableBitmapData"/> to be drawn into the <paramref name="target"/>.</param>
        /// <param name="target">The target <see cref="IReadWriteBitmapData"/> into which <paramref name="source"/> should be drawn.</param>
        /// <param name="sourceRectangle">A <see cref="Rectangle"/> that specifies the portion of the <paramref name="source"/> to be drawn into the <paramref name="target"/>.</param>
        /// <param name="targetRectangle">A <see cref="Rectangle"/> that specifies the location and size of the drawn <paramref name="source"/>.</param>
        /// <param name="quantizer">An <see cref="IQuantizer"/> instance to be used for the drawing. If not specified, then the drawing operation might automatically
        /// pick a quantizer based on <paramref name="target"/>&#160;<see cref="IBitmapData.PixelFormat"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="ditherer">The ditherer to be used for the drawing. Might be ignored if <paramref name="quantizer"/> is not specified
        /// and <paramref name="target"/>&#160;<see cref="IBitmapData.PixelFormat"/> format has at least 24 bits-per-pixel size. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="scalingMode">Specifies the scaling mode if the bitmap data to be drawn needs to be resized. This parameter is optional.
        /// <br/>Default value: <see cref="ScalingMode.Auto"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="DrawIntoAsync(IReadableBitmapData, IReadWriteBitmapData, Rectangle, Rectangle, IQuantizer, IDitherer, ScalingMode, TaskConfig)"/> method.</para>
        /// <para>To finish the operation and to get the exception that occurred during the operation you have to call the <see cref="EndDrawInto">EndDrawInto</see> method.</para>
        /// <para>This method is not a blocking call even if the <see cref="AsyncConfigBase.MaxDegreeOfParallelism"/> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="DrawInto(IReadableBitmapData, IReadWriteBitmapData, Rectangle, Rectangle, IQuantizer, IDitherer, ScalingMode)"/> method for more details.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="target"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="scalingMode"/> has an unsupported value.</exception>
        public static IAsyncResult BeginDrawInto(this IReadableBitmapData source, IReadWriteBitmapData target, Rectangle sourceRectangle, Rectangle targetRectangle, IQuantizer? quantizer = null, IDitherer? ditherer = null, ScalingMode scalingMode = ScalingMode.Auto, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(source, target, scalingMode);

            // no scaling is necessary
            if (sourceRectangle.Size == targetRectangle.Size || scalingMode == ScalingMode.NoScaling)
            {
                return source.HasAlpha()
                    ? AsyncContext.BeginOperation(ctx => DoDrawWithoutResize(ctx, source, target, sourceRectangle, targetRectangle.Location, quantizer, ditherer), asyncConfig)
                    : AsyncContext.BeginOperation(ctx => DoCopy(ctx, source, target, sourceRectangle, targetRectangle.Location, quantizer, ditherer), asyncConfig);
            }

            return AsyncContext.BeginOperation(ctx => DoDrawWithResize(ctx, source, target, sourceRectangle, targetRectangle, quantizer, ditherer, scalingMode), asyncConfig);
        }

        #endregion

        #region Async TAP
#if !NET35

        /// <summary>
        /// Draws the <paramref name="source"/>&#160;<see cref="IReadableBitmapData"/> into the <paramref name="target"/>&#160;<see cref="IReadWriteBitmapData"/> asynchronously,
        /// using scaling and blending. This method is similar to <see cref="O:System.Drawing.Graphics.DrawImage">Graphics.DrawImage</see>
        /// methods, except that this one works between any pair of source and target <see cref="PixelFormat"/>s and supports quantizing and dithering.
        /// To copy a bitmap data into another one without blending use the <see cref="CopyToAsync">CopyToAsync</see> method instead.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="source">The source <see cref="IReadableBitmapData"/> to be drawn into the <paramref name="target"/>.</param>
        /// <param name="target">The target <see cref="IReadWriteBitmapData"/> into which <paramref name="source"/> should be drawn.</param>
        /// <param name="sourceRectangle">A <see cref="Rectangle"/> that specifies the portion of the <paramref name="source"/> to be drawn into the <paramref name="target"/>.</param>
        /// <param name="targetRectangle">A <see cref="Rectangle"/> that specifies the location and size of the drawn <paramref name="source"/>.</param>
        /// <param name="quantizer">An <see cref="IQuantizer"/> instance to be used for the drawing. If not specified, then the drawing operation might automatically
        /// pick a quantizer based on <paramref name="target"/>&#160;<see cref="IBitmapData.PixelFormat"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="ditherer">The ditherer to be used for the drawing. Might be ignored if <paramref name="quantizer"/> is not specified
        /// and <paramref name="target"/>&#160;<see cref="IBitmapData.PixelFormat"/> format has at least 24 bits-per-pixel size. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="scalingMode">Specifies the scaling mode if the bitmap data to be drawn needs to be resized. This parameter is optional.
        /// <br/>Default value: <see cref="ScalingMode.Auto"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>This method is not a blocking call even if the <see cref="AsyncConfigBase.MaxDegreeOfParallelism"/> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="DrawInto(IReadableBitmapData, IReadWriteBitmapData, Rectangle, Rectangle, IQuantizer, IDitherer, ScalingMode)"/> method for more details.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="target"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="scalingMode"/> has an unsupported value.</exception>
        public static Task DrawIntoAsync(this IReadableBitmapData source, IReadWriteBitmapData target, Rectangle sourceRectangle, Rectangle targetRectangle, IQuantizer? quantizer = null, IDitherer? ditherer = null, ScalingMode scalingMode = ScalingMode.Auto, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(source, target, scalingMode);

            // no scaling is necessary
            if (sourceRectangle.Size == targetRectangle.Size || scalingMode == ScalingMode.NoScaling)
            {
                return source.HasAlpha()
                    ? AsyncContext.DoOperationAsync(ctx => DoDrawWithoutResize(ctx, source, target, sourceRectangle, targetRectangle.Location, quantizer, ditherer), asyncConfig)
                    : AsyncContext.DoOperationAsync(ctx => DoCopy(ctx, source, target, sourceRectangle, targetRectangle.Location, quantizer, ditherer), asyncConfig);
            }

            return AsyncContext.DoOperationAsync(ctx => DoDrawWithResize(ctx, source, target, sourceRectangle, targetRectangle, quantizer, ditherer, scalingMode), asyncConfig);
        }

#endif
        #endregion

        #endregion

        #endregion

        #region Clip

        /// <summary>
        /// Clips the specified <paramref name="source"/> using the specified <paramref name="clippingRegion"/>.
        /// Unlike the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.Clone">Clone</see> methods, this one returns a wrapper,
        /// providing access only to the specified region of the original <paramref name="source"/>.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="source">The source bitmap data to be clipped.</param>
        /// <param name="clippingRegion">A <see cref="Rectangle"/> that specifies a region within the <paramref name="source"/>.</param>
        /// <param name="disposeSource"><see langword="true"/>&#160;to dispose <paramref name="source"/> when the result is disposed; otherwise, <see langword="false"/>.</param>
        /// <returns>An <see cref="IReadableBitmapData"/> that provides access only to the specified region withing the <paramref name="source"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="clippingRegion"/> has no overlapping region with source bounds.</exception>
        /// <remarks>
        /// <para>The <see cref="IBitmapData.RowSize"/> property of the returned instance can be 0, indicating that the <see cref="IReadableBitmapDataRow.ReadRaw{T}">ReadRaw</see>
        /// method cannot be used. It can occur if the left edge of the clipping is not zero.</para>
        /// <para>Even if <see cref="IBitmapData.RowSize"/> property of the returned instance is a nonzero value it can happen that it is too low to access all columns
        /// by the <see cref="IReadableBitmapDataRow.ReadRaw{T}">ReadRaw</see> method. It can occur with indexed <see cref="IBitmapData.PixelFormat"/>s if the right edge of the clipping is not on byte boundary.</para>
        /// </remarks>
        public static IReadableBitmapData Clip(this IReadableBitmapData source, Rectangle clippingRegion, bool disposeSource)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source), PublicResources.ArgumentNull);
            return clippingRegion.Location.IsEmpty && clippingRegion.Size == source.GetSize()
                ? source
                : new ClippedBitmapData(source, clippingRegion, disposeSource);
        }

        /// <summary>
        /// Clips the specified <paramref name="source"/> using the specified <paramref name="clippingRegion"/>.
        /// Unlike the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.Clone">Clone</see> methods, this one returns a wrapper,
        /// providing access only to the specified region of the original <paramref name="source"/>.
        /// This overload does not dispose <paramref name="source"/> when the result is disposed.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="Clip(IReadableBitmapData,Rectangle,bool)"/> overload for details.
        /// </summary>
        /// <param name="source">The source bitmap data to be clipped.</param>
        /// <param name="clippingRegion">A <see cref="Rectangle"/> that specifies a region within the <paramref name="source"/>.</param>
        /// <returns>An <see cref="IReadableBitmapData"/> that provides access only to the specified region withing the <paramref name="source"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="clippingRegion"/> has no overlapping region with source bounds.</exception>
        public static IReadableBitmapData Clip(this IReadableBitmapData source, Rectangle clippingRegion)
            => Clip(source, clippingRegion, false);

        #endregion

        #region ToBitmap

        /// <summary>
        /// Converts the specified <paramref name="source"/> to a <see cref="Bitmap"/>.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="source">The source <see cref="IReadWriteBitmapData"/> instance to covert.</param>
        /// <returns>A <see cref="Bitmap"/> instance that has the same content as the specified <paramref name="source"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. Use the <see cref="BeginToBitmap">BeginToBitmap</see>
        /// or <see cref="ToBitmapAsync">ToBitmapAsync</see> (in .NET Framework 4.0 and above) methods for asynchronous call and to adjust parallelization, set up cancellation and for reporting progress.</note>
        /// <para>If supported on the current platform, the result <see cref="Bitmap"/> will have the same <see cref="PixelFormat"/> as <paramref name="source"/>.
        /// Otherwise, the result will have either <see cref="PixelFormat.Format24bppRgb"/> or <see cref="PixelFormat.Format32bppArgb"/> format, depending whether source has transparency.
        /// <note>On Windows every format is supported with more or less limitations. For details and further information about the possible usable <see cref="PixelFormat"/>s on different platforms
        /// see the <strong>Remarks</strong> section of the <see cref="ImageExtensions.ConvertPixelFormat(Image,PixelFormat,Color,byte)">ConvertPixelFormat</see> method.
        /// </note></para>
        /// </remarks>
        public static Bitmap ToBitmap(this IReadableBitmapData source)
        {
            ValidateArguments(source);
            return DoConvertToBitmap(AsyncContext.Null, source)!;
        }

        /// <summary>
        /// Begins to convert the specified <paramref name="source"/> to a <see cref="Bitmap"/> asynchronously.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="source">The source <see cref="IReadWriteBitmapData"/> instance to covert.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="ToBitmapAsync">ToBitmapAsync</see> method.</para>
        /// <para>To get the result or the exception that occurred during the operation you have to call the <see cref="EndToBitmap">EndToBitmap</see> method.</para>
        /// <para>This method is not a blocking call even if the <see cref="AsyncConfigBase.MaxDegreeOfParallelism"/> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="ToBitmap">ToBitmap</see> method for more details.</note>
        /// </remarks>
        public static IAsyncResult BeginToBitmap(this IReadableBitmapData source, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(source);
            return AsyncContext.BeginOperation(ctx => DoConvertToBitmap(ctx, source), asyncConfig);
        }

        /// <summary>
        /// Waits for the pending asynchronous operation started by the <see cref="BeginToBitmap">BeginToBitmap</see> method to complete.
        /// In .NET Framework 4.0 and above you can use the <see cref="ToBitmapAsync">ToBitmapAsync</see> method instead.
        /// </summary>
        /// <param name="asyncResult">The reference to the pending asynchronous request to finish.</param>
        /// <returns>A <see cref="Bitmap"/> instance that is the result of the operation,
        /// or <see langword="null"/>, if the operation was canceled and <see cref="AsyncConfigBase.ThrowIfCanceled"/> property of the <c>asyncConfig</c> parameter was <see langword="false"/>.</returns>
        public static Bitmap? EndToBitmap(this IAsyncResult asyncResult) => AsyncContext.EndOperation<Bitmap>(asyncResult, nameof(BeginToBitmap));

#if !NET35
        /// <summary>
        /// Converts the specified <paramref name="source"/> to a <see cref="Bitmap"/> asynchronously.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="source">The source <see cref="IReadWriteBitmapData"/> instance to covert.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. Its result is a <see cref="Bitmap"/> instance that has the same content as the specified <paramref name="source"/>,
        /// or <see langword="null"/>, if the operation was canceled and <see cref="AsyncConfigBase.ThrowIfCanceled"/> property of the <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// <para>This method is not a blocking call even if the <see cref="AsyncConfigBase.MaxDegreeOfParallelism"/> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="ToBitmap">ToBitmap</see> method for more details.</note>
        /// </remarks>
        public static Task<Bitmap?> ToBitmapAsync(this IReadableBitmapData source, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(source);
            return AsyncContext.DoOperationAsync(ctx => DoConvertToBitmap(ctx, source), asyncConfig);
        }
#endif

        #endregion

        #region GetColors

        /// <summary>
        /// Gets the colors used in the specified <paramref name="bitmapData"/>. A limit can be defined in <paramref name="maxColors"/>.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadableBitmapData"/>, whose colors have to be returned. If it is indexed and the <paramref name="forceScanningContent"/> parameter is <see langword="false"/>,
        /// then its palette entries are returned and <paramref name="maxColors"/> is ignored.</param>
        /// <param name="maxColors">A limit of the returned colors. If <paramref name="forceScanningContent"/> parameter is <see langword="false"/>, then
        /// this parameter is ignored for indexed bitmaps. Use 0 for no limit. This parameter is optional.
        /// <br/>Default value: <c>0</c>.</param>
        /// <param name="forceScanningContent"><see langword="true"/>&#160;to force scanning the actual image content even if the specified <paramref name="bitmapData"/> is
        /// indexed and has a palette. This parameter is optional.
        /// <br/>Default value: <see langword="false"/>.</param>
        /// <returns>An <see cref="ICollection{T}"/> of <see cref="Color32"/> entries.</returns>
        /// <remarks>
        /// <note>This method blocks the caller, and does not support cancellation or reporting progress. Use the <see cref="BeginGetColors">BeginGetColors</see>
        /// or <see cref="GetColorsAsync">GetColorsAsync</see> (in .NET Framework 4.0 and above) methods for asynchronous call and to set up cancellation or for reporting progress.</note>
        /// <para>Completely transparent pixels are considered the same regardless of their color information.</para>
        /// <para>Every <see cref="PixelFormat"/> is supported, though wide color formats (<see cref="PixelFormat.Format16bppGrayScale"/>, <see cref="PixelFormat.Format48bppRgb"/>,
        /// <see cref="PixelFormat.Format64bppArgb"/> and <see cref="PixelFormat.Format64bppPArgb"/>) are quantized to 32 bit during the processing.
        /// To get the actual <em>number</em> of colors, which can be accurate even for wide color formats, use the <see cref="GetColorCount">GetColorCount</see> method.
        /// </para>
        /// </remarks>
        public static ICollection<Color32> GetColors(this IReadableBitmapData bitmapData, int maxColors = 0, bool forceScanningContent = false)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);

            if (!forceScanningContent && bitmapData.PixelFormat.IsIndexed() && bitmapData.Palette != null)
                return bitmapData.Palette.GetEntries();

            return DoGetColors(AsyncContext.Null, bitmapData, maxColors);
        }

        /// <summary>
        /// Begins to get the colors used in the specified <paramref name="bitmapData"/> asynchronously. A limit can be defined in <paramref name="maxColors"/>.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadableBitmapData"/>, whose colors have to be returned. If it is indexed and the <paramref name="forceScanningContent"/> parameter is <see langword="false"/>,
        /// then its palette entries are returned and <paramref name="maxColors"/> is ignored.</param>
        /// <param name="maxColors">A limit of the returned colors. If <paramref name="forceScanningContent"/> parameter is <see langword="false"/>, then
        /// this parameter is ignored for indexed bitmaps. Use 0 for no limit. This parameter is optional.
        /// <br/>Default value: <c>0</c>.</param>
        /// <param name="forceScanningContent"><see langword="true"/>&#160;to force scanning the actual image content even if the specified <paramref name="bitmapData"/> is
        /// indexed and has a palette. This parameter is optional.
        /// <br/>Default value: <see langword="false"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as cancellation, reporting progress, etc. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="GetColorsAsync">GetColorsAsync</see> method.</para>
        /// <para>To get the result or the exception that occurred during the operation you have to call the <see cref="EndGetColors">EndGetColors</see> method.</para>
        /// <para>This method is not a blocking call, though the operation is not parallelized and the <see cref="AsyncConfigBase.MaxDegreeOfParallelism"/> property of the <paramref name="asyncConfig"/> parameter is ignored.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="GetColors">GetColors</see> method for more details.</note>
        /// </remarks>
        public static IAsyncResult BeginGetColors(this IReadableBitmapData bitmapData, int maxColors = 0, bool forceScanningContent = false, AsyncConfig? asyncConfig = null)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);

            return !forceScanningContent && bitmapData.PixelFormat.IsIndexed() && bitmapData.Palette != null
                ? AsyncContext.FromResult(bitmapData.Palette.GetEntries(), Reflector.EmptyArray<Color32>(), asyncConfig)
                : AsyncContext.BeginOperation(ctx => DoGetColors(ctx, bitmapData, maxColors), asyncConfig);
        }

        /// <summary>
        /// Waits for the pending asynchronous operation started by the <see cref="BeginGetColors">BeginGetColors</see> method to complete.
        /// In .NET Framework 4.0 and above you can use the <see cref="GetColorsAsync">GetColorsAsync</see> method instead.
        /// </summary>
        /// <param name="asyncResult">The reference to the pending asynchronous request to finish.</param>
        /// <returns>An <see cref="ICollection{T}"/> of <see cref="Color32"/> entries that is the result of the operation.
        /// If the operation was canceled and <see cref="AsyncConfigBase.ThrowIfCanceled"/> property of the <c>asyncConfig</c> parameter was <see langword="false"/>, then an empty collection is returned.</returns>
        public static ICollection<Color32> EndGetColors(this IAsyncResult asyncResult) => AsyncContext.EndOperation<ICollection<Color32>>(asyncResult, nameof(BeginGetColors))!;

#if !NET35
        /// <summary>
        /// Gets the colors used in the specified <paramref name="bitmapData"/> asynchronously. A limit can be defined in <paramref name="maxColors"/>.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadableBitmapData"/>, whose colors have to be returned. If it is indexed and the <paramref name="forceScanningContent"/> parameter is <see langword="false"/>,
        /// then its palette entries are returned and <paramref name="maxColors"/> is ignored.</param>
        /// <param name="maxColors">A limit of the returned colors. If <paramref name="forceScanningContent"/> parameter is <see langword="false"/>, then
        /// this parameter is ignored for indexed bitmaps. Use 0 for no limit. This parameter is optional.
        /// <br/>Default value: <c>0</c>.</param>
        /// <param name="forceScanningContent"><see langword="true"/>&#160;to force scanning the actual image content even if the specified <paramref name="bitmapData"/> is
        /// indexed and has a palette. This parameter is optional.
        /// <br/>Default value: <see langword="false"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as cancellation, reporting progress, etc. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. Its result is an <see cref="ICollection{T}"/> of <see cref="Color32"/> entries.
        /// If the operation was canceled and <see cref="AsyncConfigBase.ThrowIfCanceled"/> property of the <paramref name="asyncConfig"/> parameter was <see langword="false"/>, then the result of the task is an empty collection.</returns>
        /// <remarks>
        /// <para>This method is not a blocking call, though the operation is not parallelized and the <see cref="AsyncConfigBase.MaxDegreeOfParallelism"/> property of the <paramref name="asyncConfig"/> parameter is ignored.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="GetColors">GetColors</see> method for more details.</note>
        /// </remarks>
        public static Task<ICollection<Color32>> GetColorsAsync(this IReadableBitmapData bitmapData, int maxColors = 0, bool forceScanningContent = false, TaskConfig? asyncConfig = null)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);

            return !forceScanningContent && bitmapData.PixelFormat.IsIndexed() && bitmapData.Palette != null
                ? AsyncContext.FromResult((ICollection<Color32>)bitmapData.Palette.GetEntries(), Reflector.EmptyArray<Color32>(), asyncConfig)
                : AsyncContext.DoOperationAsync(ctx => DoGetColors(ctx, bitmapData, maxColors), asyncConfig)!;
        }
#endif

        #endregion

        #region GetColorCount

        /// <summary>
        /// Gets the actual number of colors of the specified <paramref name="bitmapData"/>. Colors are counted even for indexed bitmaps.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="bitmapData">The bitmap, whose colors have to be counted to count its colors.</param>
        /// <returns>The actual number of colors of the specified <paramref name="bitmapData"/>.</returns>
        /// <remarks>
        /// <note>This method blocks the caller, and does not support cancellation or reporting progress. Use the <see cref="BeginGetColorCount">BeginGetColorCount</see>
        /// or <see cref="GetColorCountAsync">GetColorCountAsync</see> (in .NET Framework 4.0 and above) methods for asynchronous call and to set up cancellation or for reporting progress.</note>
        /// <para>Completely transparent pixels are considered the same regardless of their color information.</para>
        /// <para>Every <see cref="PixelFormat"/> is supported, but an accurate result is returned for wide color formats only
        /// when <see cref="IBitmapData.RowSize"/> is large enough to access all pixels directly (might not be the case for a clipped bitmap data, for example).
        /// Otherwise, colors are quantized to 32 bits-per-pixel values while counting them.
        /// Wide pixel formats are <see cref="PixelFormat.Format16bppGrayScale"/>, <see cref="PixelFormat.Format48bppRgb"/>, <see cref="PixelFormat.Format64bppArgb"/> and <see cref="PixelFormat.Format64bppPArgb"/>.</para>
        /// </remarks>
        public static int GetColorCount(this IReadableBitmapData bitmapData)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            return DoGetColorCount(AsyncContext.Null, bitmapData);
        }

        /// <summary>
        /// Gets the actual number of colors of the specified <paramref name="bitmapData"/> asynchronously. Colors are counted even for indexed bitmaps.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="bitmapData">The bitmap, whose colors have to be counted to count its colors.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as cancellation, reporting progress, etc. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="GetColorCountAsync">GetColorCountAsync</see> method.</para>
        /// <para>To get the result or the exception that occurred during the operation you have to call the <see cref="EndGetColorCount">EndGetColorCount</see> method.</para>
        /// <para>This method is not a blocking call, though the operation is not parallelized and the <see cref="AsyncConfigBase.MaxDegreeOfParallelism"/> property of the <paramref name="asyncConfig"/> parameter is ignored.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="GetColorCount">GetColorCount</see> method for more details.</note>
        /// </remarks>
        public static IAsyncResult BeginGetColorCount(this IReadableBitmapData bitmapData, AsyncConfig? asyncConfig = null)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            return AsyncContext.BeginOperation<object>(ctx => DoGetColorCount(ctx, bitmapData), asyncConfig);
        }

        /// <summary>
        /// Waits for the pending asynchronous operation started by the <see cref="BeginGetColorCount">BeginGetColorCount</see> method to complete.
        /// In .NET Framework 4.0 and above you can use the <see cref="GetColorCountAsync">GetColorCountAsync</see> method instead.
        /// </summary>
        /// <param name="asyncResult">The reference to the pending asynchronous request to finish.</param>
        /// <returns>An <see cref="int">int</see> value that is the result of the operation,
        /// or <c>0</c>, if the operation was canceled and <see cref="AsyncConfigBase.ThrowIfCanceled"/> property of the <c>asyncConfig</c> parameter was <see langword="false"/>.</returns>
        public static int EndGetColorCount(this IAsyncResult asyncResult) => (int?)AsyncContext.EndOperation<object>(asyncResult, nameof(BeginGetColorCount)) ?? default;

#if !NET35
        /// <summary>
        /// Gets the actual number of colors of the specified <paramref name="bitmapData"/> asynchronously. Colors are counted even for indexed bitmaps.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="bitmapData">The bitmap, whose colors have to be counted to count its colors.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as cancellation, reporting progress, etc. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. Its result is the actual number of colors of the specified <paramref name="bitmapData"/>,
        /// or <c>0</c>, if the operation was canceled and <see cref="AsyncConfigBase.ThrowIfCanceled"/> property of the <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>This method is not a blocking call, though the operation is not parallelized and the <see cref="AsyncConfigBase.MaxDegreeOfParallelism"/> property of the <paramref name="asyncConfig"/> parameter is ignored.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="GetColorCount">GetColorCount</see> method for more details.</note>
        /// </remarks>
        public static Task<int> GetColorCountAsync(this IReadableBitmapData bitmapData, TaskConfig? asyncConfig = null)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            return AsyncContext.DoOperationAsync(ctx => DoGetColorCount(ctx, bitmapData), asyncConfig);
        }
#endif

        #endregion

        #region ToGrayscale

        /// <summary>
        /// Returns a new <see cref="IReadWriteBitmapData"/>, which is the grayscale version of the specified <paramref name="bitmapData"/>.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadableBitmapData"/> to convert to grayscale.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> containing the grayscale version of the original <paramref name="bitmapData"/>.</returns>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. Use the <see cref="BeginToGrayscale">BeginToGrayscale</see>
        /// or <see cref="ToGrayscaleAsync">ToGrayscaleAsync</see> (in .NET Framework 4.0 and above) methods for asynchronous call and to adjust parallelization, set up cancellation and for reporting progress.</note>
        /// <para>This method always returns a new <see cref="IReadWriteBitmapData"/> with <see cref="PixelFormat.Format32bppArgb"/> pixel format.</para>
        /// <para>To return an <see cref="IReadWriteBitmapData"/> with arbitrary <see cref="IBitmapData.PixelFormat"/> use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.Clone">Clone</see> overloads with a grayscale palette,
        /// quantizer (eg. <see cref="PredefinedColorsQuantizer.Grayscale">PredefinedColorsQuantizer.Grayscale</see>) or pixel format (<see cref="PixelFormat.Format16bppGrayScale"/>).</para>
        /// <para>To make an <see cref="IReadWriteBitmapData"/> grayscale without creating a new instance use the <see cref="MakeGrayscale">MakeGrayscale</see> method.</para>
        /// </remarks>
        /// <seealso cref="ImageExtensions.ToGrayscale"/>
        /// <seealso cref="MakeGrayscale"/>
        /// <seealso cref="BitmapExtensions.MakeGrayscale"/>
        public static IReadWriteBitmapData ToGrayscale(this IReadableBitmapData bitmapData)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            return DoCloneWithQuantizer(AsyncContext.Null, bitmapData, new Rectangle(Point.Empty, bitmapData.GetSize()), PixelFormat.Format32bppArgb,
                PredefinedColorsQuantizer.FromCustomFunction(TransformMakeGrayscale))!;
        }

        /// <summary>
        /// Begins to convert the specified <paramref name="bitmapData"/> to grayscale asynchronously.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadableBitmapData"/> to convert to grayscale.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. Its result is an <see cref="IReadWriteBitmapData"/> containing the grayscale version of the original <paramref name="bitmapData"/>.</returns>
        /// <remarks>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="ToGrayscaleAsync">ToGrayscaleAsync</see> method.</para>
        /// <para>To get the result or the exception that occurred during the operation you have to call the <see cref="EndToGrayscale">EndToGrayscale</see> method.</para>
        /// <para>This method is not a blocking call even if the <see cref="AsyncConfigBase.MaxDegreeOfParallelism"/> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="ToGrayscale">ToGrayscale</see> method for more details.</note>
        /// </remarks>
        /// <seealso cref="BeginMakeGrayscale"/>
        public static IAsyncResult BeginToGrayscale(this IReadableBitmapData bitmapData, AsyncConfig? asyncConfig = null)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            return AsyncContext.BeginOperation(ctx => DoCloneWithQuantizer(ctx, bitmapData, new Rectangle(Point.Empty, bitmapData.GetSize()), PixelFormat.Format32bppArgb,
                PredefinedColorsQuantizer.FromCustomFunction(TransformMakeGrayscale)), asyncConfig);
        }

        /// <summary>
        /// Waits for the pending asynchronous operation started by the <see cref="BeginToGrayscale">BeginToGrayscale</see> method to complete.
        /// In .NET Framework 4.0 and above you can use the <see cref="ToGrayscaleAsync">ToGrayscaleAsync</see> method instead.
        /// </summary>
        /// <param name="asyncResult">The reference to the pending asynchronous request to finish.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance that is the result of the operation,
        /// or <see langword="null"/>, if the operation was canceled and <see cref="AsyncConfigBase.ThrowIfCanceled"/> property of the <c>asyncConfig</c> parameter was <see langword="false"/>.</returns>
        public static IReadWriteBitmapData? EndToGrayscale(this IAsyncResult asyncResult) => AsyncContext.EndOperation<IReadWriteBitmapData>(asyncResult, nameof(BeginToGrayscale));

#if !NET35
        /// <summary>
        /// Returns a new <see cref="IReadWriteBitmapData"/> asynchronously, which is the grayscale version of the specified <paramref name="bitmapData"/>.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadableBitmapData"/> to convert to grayscale.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. Its result is an <see cref="IReadWriteBitmapData"/> containing the grayscale version of the original <paramref name="bitmapData"/>,
        /// or <see langword="null"/>, if the operation was canceled and <see cref="AsyncConfigBase.ThrowIfCanceled"/> property of the <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>This method is not a blocking call even if the <see cref="AsyncConfigBase.MaxDegreeOfParallelism"/> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="ToGrayscale">ToGrayscale</see> method for more details.</note>
        /// </remarks>
        /// <seealso cref="MakeGrayscaleAsync"/>
        public static Task<IReadWriteBitmapData?> ToGrayscaleAsync(this IReadWriteBitmapData bitmapData, TaskConfig? asyncConfig = null)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            return AsyncContext.DoOperationAsync(ctx => DoCloneWithQuantizer(ctx, bitmapData, new Rectangle(Point.Empty, bitmapData.GetSize()), PixelFormat.Format32bppArgb,
                PredefinedColorsQuantizer.FromCustomFunction(TransformMakeGrayscale)), asyncConfig);
        }
#endif

        #endregion

        #region ToTransparent

        /// <summary>
        /// Returns a new <see cref="IReadWriteBitmapData"/>, which is the clone of the specified <paramref name="bitmapData"/> with transparent background.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadableBitmapData"/> to convert to transparent.</param>
        /// <returns>A new <see cref="IReadWriteBitmapData"/>, which is the clone of the specified <paramref name="bitmapData"/> with transparent background.</returns>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. Use the <see cref="BeginToTransparent(IReadableBitmapData, AsyncConfig)"/>
        /// or <see cref="ToTransparentAsync(IReadableBitmapData, TaskConfig)"/> (in .NET Framework 4.0 and above) methods for asynchronous call and to adjust parallelization, set up cancellation and for reporting progress.</note>
        /// <para>Similarly to the <see cref="Bitmap.MakeTransparent()">Bitmap.MakeTransparent</see> method, this one uses the bottom-left pixel to determine
        /// the background color, which must be completely opaque; otherwise, just an exact clone of <paramref name="bitmapData"/> will be returned.</para>
        /// <para>This method always returns a new <see cref="IReadWriteBitmapData"/> with <see cref="PixelFormat.Format32bppArgb"/> pixel format.</para>
        /// <para>To attempt to make an <see cref="IReadWriteBitmapData"/> transparent without creating a new instance use the <see cref="MakeTransparent(IReadWriteBitmapData)">MakeTransparent</see> method.</para>
        /// <para>To force replacing even non-completely opaque pixels use the <see cref="ToTransparent(IReadableBitmapData, Color32)"/> overload instead.</para>
        /// <note>Please note that unlike the <see cref="MakeOpaque">MakeOpaque</see> method, this one changes exactly one color shade without any tolerance.
        /// For any customization use the <see cref="Clone(IReadableBitmapData, PixelFormat, IQuantizer, IDitherer)">Clone</see> method with a quantizer
        /// created by the <see cref="PredefinedColorsQuantizer.FromCustomFunction(Func{Color32, Color32}, PixelFormat)">PredefinedColorsQuantizer.FromCustomFunction</see> method.</note>
        /// </remarks>
        /// <seealso cref="MakeTransparent(IReadWriteBitmapData)"/>
        /// <seealso cref="MakeOpaque"/>
        public static IReadWriteBitmapData ToTransparent(this IReadableBitmapData bitmapData)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            return DoToTransparent(AsyncContext.Null, bitmapData)!;
        }

        /// <summary>
        /// Returns a new <see cref="IReadWriteBitmapData"/>, which is the clone of the specified <paramref name="bitmapData"/> with transparent background.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadableBitmapData"/> to convert to transparent.</param>
        /// <param name="transparentColor">Specifies the color to make transparent.</param>
        /// <returns>A new <see cref="IReadWriteBitmapData"/>, which is the clone of the specified <paramref name="bitmapData"/> with transparent background.</returns>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. Use the <see cref="BeginToTransparent(IReadableBitmapData, Color32, AsyncConfig)"/>
        /// or <see cref="ToTransparentAsync(IReadableBitmapData, Color32, TaskConfig)"/> (in .NET Framework 4.0 and above) methods for asynchronous call and to adjust parallelization, set up cancellation and for reporting progress.</note>
        /// <para>This method always returns a new <see cref="IReadWriteBitmapData"/> with <see cref="PixelFormat.Format32bppArgb"/> pixel format.</para>
        /// <para>To attempt to make an <see cref="IReadWriteBitmapData"/> transparent without creating a new instance use the <see cref="MakeTransparent(IReadWriteBitmapData,Color32)">MakeTransparent</see> method.</para>
        /// <para>To auto-detect the background color to be made transparent use the <see cref="ToTransparent(IReadableBitmapData)"/> overload instead.</para>
        /// <note>Please note that unlike the <see cref="MakeOpaque">MakeOpaque</see> method, this one changes exactly one color shade without any tolerance.
        /// For any customization use the <see cref="Clone(IReadableBitmapData, PixelFormat, IQuantizer, IDitherer)">Clone</see> method with a quantizer
        /// created by the <see cref="PredefinedColorsQuantizer.FromCustomFunction(Func{Color32, Color32}, PixelFormat)">PredefinedColorsQuantizer.FromCustomFunction</see> method.</note>
        /// </remarks>
        /// <seealso cref="MakeTransparent(IReadWriteBitmapData,Color32)"/>
        /// <seealso cref="MakeOpaque"/>
        public static IReadWriteBitmapData ToTransparent(this IReadableBitmapData bitmapData, Color32 transparentColor)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            return DoToTransparent(AsyncContext.Null, bitmapData, transparentColor)!;
        }

        /// <summary>
        /// Begins to convert the specified <paramref name="bitmapData"/> to another one with transparent background asynchronously.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadableBitmapData"/> to convert to transparent.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="ToTransparentAsync(IReadableBitmapData, TaskConfig)"/> method.</para>
        /// <para>To get the result or the exception that occurred during the operation you have to call the <see cref="EndToTransparent">EndToTransparent</see> method.</para>
        /// <para>This method is not a blocking call even if the <see cref="AsyncConfigBase.MaxDegreeOfParallelism"/> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="ToTransparent(IReadableBitmapData)"/> method for more details.</note>
        /// </remarks>
        /// <seealso cref="BeginMakeTransparent(IReadWriteBitmapData, AsyncConfig)"/>
        /// <seealso cref="BeginMakeOpaque"/>
        public static IAsyncResult BeginToTransparent(this IReadableBitmapData bitmapData, AsyncConfig? asyncConfig = null)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            return AsyncContext.BeginOperation(ctx => DoToTransparent(ctx, bitmapData), asyncConfig);
        }

        /// <summary>
        /// Begins to convert the specified <paramref name="bitmapData"/> to another one with transparent background asynchronously.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadableBitmapData"/> to convert to transparent.</param>
        /// <param name="transparentColor">Specifies the color to make transparent.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="ToTransparentAsync(IReadableBitmapData, Color32, TaskConfig)"/> method.</para>
        /// <para>To get the result or the exception that occurred during the operation you have to call the <see cref="EndToTransparent">EndToTransparent</see> method.</para>
        /// <para>This method is not a blocking call even if the <see cref="AsyncConfigBase.MaxDegreeOfParallelism"/> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="ToTransparent(IReadableBitmapData, Color32)"/> method for more details.</note>
        /// </remarks>
        /// <seealso cref="BeginMakeTransparent(IReadWriteBitmapData, Color32, AsyncConfig)"/>
        /// <seealso cref="BeginMakeOpaque"/>
        public static IAsyncResult BeginToTransparent(this IReadableBitmapData bitmapData, Color32 transparentColor, AsyncConfig? asyncConfig = null)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            return AsyncContext.BeginOperation(ctx => DoToTransparent(ctx, bitmapData, transparentColor), asyncConfig);
        }

        /// <summary>
        /// Waits for the pending asynchronous operation started by the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.BeginToTransparent">BeginToTransparent</see> methods to complete.
        /// In .NET Framework 4.0 and above you can use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.ToTransparentAsync">ToTransparentAsync</see> methods instead.
        /// </summary>
        /// <param name="asyncResult">The reference to the pending asynchronous request to finish.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance that is the result of the operation,
        /// or <see langword="null"/>, if the operation was canceled and <see cref="AsyncConfigBase.ThrowIfCanceled"/> property of the <c>asyncConfig</c> parameter was <see langword="false"/>.</returns>
        public static IReadWriteBitmapData? EndToTransparent(this IAsyncResult asyncResult) => AsyncContext.EndOperation<IReadWriteBitmapData>(asyncResult, nameof(BeginToTransparent));

#if !NET35
        /// <summary>
        /// Returns a new <see cref="IReadWriteBitmapData"/> asynchronously, which is the clone of the specified <paramref name="bitmapData"/> with transparent background.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadableBitmapData"/> to convert to transparent.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. Its result is an <see cref="IReadWriteBitmapData"/>, which is the clone of the specified <paramref name="bitmapData"/> with transparent background,
        /// or <see langword="null"/>, if the operation was canceled and <see cref="AsyncConfigBase.ThrowIfCanceled"/> property of the <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>This method is not a blocking call even if the <see cref="AsyncConfigBase.MaxDegreeOfParallelism"/> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="ToTransparent(IReadableBitmapData)"/> method for more details.</note>
        /// </remarks>
        /// <seealso cref="MakeTransparentAsync(IReadWriteBitmapData, TaskConfig)"/>
        /// <seealso cref="MakeOpaqueAsync"/>
        public static Task<IReadWriteBitmapData?> ToTransparentAsync(this IReadableBitmapData bitmapData, TaskConfig? asyncConfig = null)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            return AsyncContext.DoOperationAsync(ctx => DoToTransparent(ctx, bitmapData), asyncConfig);
        }

        /// <summary>
        /// Returns a new <see cref="IReadWriteBitmapData"/> asynchronously, which is the clone of the specified <paramref name="bitmapData"/> with transparent background.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadableBitmapData"/> to convert to transparent.</param>
        /// <param name="transparentColor">Specifies the color to make transparent.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. Its result is an <see cref="IReadWriteBitmapData"/>, which is the clone of the specified <paramref name="bitmapData"/> with transparent background,
        /// or <see langword="null"/>, if the operation was canceled and <see cref="AsyncConfigBase.ThrowIfCanceled"/> property of the <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>This method is not a blocking call even if the <see cref="AsyncConfigBase.MaxDegreeOfParallelism"/> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="ToTransparent(IReadableBitmapData, Color32)"/> method for more details.</note>
        /// </remarks>
        /// <seealso cref="MakeTransparentAsync(IReadWriteBitmapData, Color32, TaskConfig)"/>
        /// <seealso cref="MakeOpaqueAsync"/>
        public static Task<IReadWriteBitmapData?> ToTransparentAsync(this IReadableBitmapData bitmapData, Color32 transparentColor, TaskConfig? asyncConfig = null)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            return AsyncContext.DoOperationAsync(ctx => DoToTransparent(ctx, bitmapData, transparentColor), asyncConfig);
        }
#endif

        #endregion

        #region Save

        /// <summary>
        /// Saves the content of this <paramref name="bitmapData"/> into the specified <paramref name="stream"/>.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadableBitmapData"/> to save.</param>
        /// <param name="stream">The stream to save the bitmap data into.</param>
        /// <remarks>
        /// <note>This method blocks the caller, and does not support cancellation or reporting progress. Use the <see cref="BeginSave">BeginSave</see>
        /// or <see cref="SaveAsync">SaveAsync</see> (in .NET Framework 4.0 and above) methods for asynchronous call and to set up cancellation or for reporting progress.</note>
        /// <para>To reload the content use the <see cref="BitmapDataFactory.Load">BitmapDataFactory.Load</see> method.</para>
        /// <para>The saved content always preserves known <see cref="PixelFormat"/>s so the <see cref="BitmapDataFactory.Load">BitmapDataFactory.Load</see>
        /// method can restore it the same way on any platform. Custom pixel formats are saved by a compatible known pixel format.</para>
        /// <note>If <paramref name="bitmapData"/> represents the bitmap data of a native <see cref="Bitmap"/> with <see cref="PixelFormat.Format48bppRgb"/>,
        /// <see cref="PixelFormat.Format64bppArgb"/> or <see cref="PixelFormat.Format64bppPArgb"/>&#160;<see cref="PixelFormat"/>, then on Windows the actual
        /// color depth is quantized to 32bpp because these formats have a different raw interpretation than managed bitmaps used by <c>KGySoft Drawing Libraries</c>.</note>
        /// </remarks>
        public static void Save(this IReadableBitmapData bitmapData, Stream stream)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            if (stream == null)
                throw new ArgumentNullException(nameof(stream), PublicResources.ArgumentNull);
            DoSave(AsyncContext.Null, bitmapData, stream);
        }

        /// <summary>
        /// Begins to save the content of this <paramref name="bitmapData"/> into the specified <paramref name="stream"/> asynchronously.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadableBitmapData"/> to save.</param>
        /// <param name="stream">The stream to save the bitmap data into.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as cancellation, reporting progress, etc. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="SaveAsync">SaveAsync</see> method.</para>
        /// <para>To finish the operation and to get the exception that occurred during the operation you have to call the <see cref="EndSave">EndSave</see> method.</para>
        /// <para>This method is not a blocking call, though the operation is not parallelized and the <see cref="AsyncConfigBase.MaxDegreeOfParallelism"/> property of the <paramref name="asyncConfig"/> parameter is ignored.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="Save">Save</see> method for more details.</note>
        /// </remarks>
        public static IAsyncResult BeginSave(this IReadableBitmapData bitmapData, Stream stream, AsyncConfig? asyncConfig = null)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            if (stream == null)
                throw new ArgumentNullException(nameof(stream), PublicResources.ArgumentNull);
            return AsyncContext.BeginOperation(ctx => DoSave(ctx, bitmapData, stream), asyncConfig);
        }

        /// <summary>
        /// Waits for the pending asynchronous operation started by the <see cref="BeginSave">BeginSave</see> method to complete.
        /// In .NET Framework 4.0 and above you can use the <see cref="SaveAsync">SaveAsync</see> method instead.
        /// </summary>
        /// <param name="asyncResult">The reference to the pending asynchronous request to finish.</param>
        public static void EndSave(this IAsyncResult asyncResult) => AsyncContext.EndOperation(asyncResult, nameof(BeginSave));

#if !NET35
        /// <summary>
        /// Saves the content of this <paramref name="bitmapData"/> into the specified <paramref name="stream"/> asynchronously.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadableBitmapData"/> to save.</param>
        /// <param name="stream">The stream to save the bitmap data into.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as cancellation, reporting progress, etc. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>This method is not a blocking call, though the operation is not parallelized and the <see cref="AsyncConfigBase.MaxDegreeOfParallelism"/> property of the <paramref name="asyncConfig"/> parameter is ignored.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="Save">Save</see> method for more details.</note>
        /// </remarks>
        public static Task SaveAsync(this IReadableBitmapData bitmapData, Stream stream, TaskConfig? asyncConfig = null)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            if (stream == null)
                throw new ArgumentNullException(nameof(stream), PublicResources.ArgumentNull);
            return AsyncContext.DoOperationAsync(ctx => DoSave(ctx, bitmapData, stream), asyncConfig);
        }
#endif

        #endregion

        #endregion

        #region Internal Methods

        internal static IReadWriteBitmapData? DoClone(this IReadableBitmapData source, IAsyncContext context) => DoCloneExact(context, source);

        internal static IReadWriteBitmapData? DoClone(this IReadableBitmapData source, IAsyncContext context, PixelFormat pixelFormat, IQuantizer? quantizer, IDitherer? ditherer)
            => DoCloneWithQuantizer(context, source, new Rectangle(Point.Empty, source.GetSize()), pixelFormat, quantizer, ditherer);

        internal static IReadWriteBitmapData? DoClone(this IReadableBitmapData source, IAsyncContext context, Rectangle sourceRectangle, PixelFormat pixelFormat, IQuantizer? quantizer, IDitherer? ditherer)
            => DoCloneWithQuantizer(context, source, sourceRectangle, pixelFormat, quantizer, ditherer);

        internal static IReadWriteBitmapData DoClone(this IReadableBitmapData source, IAsyncContext context, Rectangle sourceRectangle, PixelFormat pixelFormat, Palette palette)
            => DoCloneDirect(context, source, sourceRectangle, pixelFormat, palette.BackColor, palette.AlphaThreshold, palette)!;

        internal static void DoCopyTo(this IReadableBitmapData source, IAsyncContext context, IWritableBitmapData target, Point targetLocation = default, IQuantizer? quantizer = null, IDitherer? ditherer = null)
            => DoCopy(context, source, target, new Rectangle(Point.Empty, source.GetSize()), targetLocation, quantizer, ditherer);

        internal static void DoCopyTo(this IReadableBitmapData source, IAsyncContext context, IWritableBitmapData target, Rectangle sourceRectangle, Point targetLocation, IQuantizer? quantizer = null, bool skipTransparent = false)
            => DoCopy(context, source, target, sourceRectangle, targetLocation, quantizer, null, skipTransparent);

        internal static void DoDrawInto(this IReadableBitmapData source, IAsyncContext context, IReadWriteBitmapData target, Rectangle targetRectangle)
            => DoDrawInto(context, source, target, new Rectangle(Point.Empty, source.GetSize()), targetRectangle, null, null, ScalingMode.Auto);

        #endregion

        #region Private Methods

        #region Validation
        // ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local - all of these methods are validations

        private static void ValidateArguments(IReadableBitmapData source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source), PublicResources.ArgumentNull);
            if (source.Width <= 0 || source.Height <= 0)
                throw new ArgumentException(Res.ImagingInvalidBitmapDataSize, nameof(source));
        }

        private static void ValidateArguments(IReadableBitmapData source, PixelFormat pixelFormat)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source), PublicResources.ArgumentNull);
            if (source.Width <= 0 || source.Height <= 0)
                throw new ArgumentException(Res.ImagingInvalidBitmapDataSize, nameof(source));
            if (!pixelFormat.IsValidFormat())
                throw new ArgumentOutOfRangeException(nameof(pixelFormat), Res.PixelFormatInvalid(pixelFormat));
        }

        private static void ValidateArguments(IReadableBitmapData source, IWritableBitmapData target)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source), PublicResources.ArgumentNull);
            if (source.Width <= 0 || source.Height <= 0)
                throw new ArgumentException(Res.ImagingInvalidBitmapDataSize, nameof(source));
            if (target == null)
                throw new ArgumentNullException(nameof(target), PublicResources.ArgumentNull);
            if (target.Width <= 0 || target.Height <= 0)
                throw new ArgumentException(Res.ImagingInvalidBitmapDataSize, nameof(target));
        }

        private static void ValidateArguments(IReadableBitmapData source, IReadWriteBitmapData target, ScalingMode scalingMode)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source), PublicResources.ArgumentNull);
            if (source.Width <= 0 || source.Height <= 0)
                throw new ArgumentException(Res.ImagingInvalidBitmapDataSize, nameof(source));
            if (target == null)
                throw new ArgumentNullException(nameof(target), PublicResources.ArgumentNull);
            if (target.Width <= 0 || target.Height <= 0)
                throw new ArgumentException(Res.ImagingInvalidBitmapDataSize, nameof(target));
            if (!scalingMode.IsDefined())
                throw new ArgumentOutOfRangeException(nameof(scalingMode), PublicResources.EnumOutOfRange(scalingMode));
        }

        // ReSharper restore ParameterOnlyUsedForPreconditionCheck.Local
        #endregion

        #region Copy

        [SuppressMessage("ReSharper", "AssignmentInConditionalExpression", Justification = "Intended")]
        [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "ReSharper issue")]
        private static IReadWriteBitmapData? DoCloneExact(IAsyncContext context, IReadableBitmapData source)
        {
            Size size = source.GetSize();
            var session = new CopySession(context) { SourceRectangle = new Rectangle(Point.Empty, size) };
            Unwrap(ref source, ref session.SourceRectangle);
            session.TargetRectangle = session.SourceRectangle;

            session.Source = source as IBitmapDataInternal ?? new BitmapDataWrapper(source, true, false);
            session.Target = source is ICustomBitmapData customBitmapData
                ? customBitmapData.CreateCompatibleBitmapDataFactory.Invoke(session.TargetRectangle.Size)
                : BitmapDataFactory.CreateManagedBitmapData(size, source.GetKnownPixelFormat(), source.BackColor, source.AlphaThreshold, source.Palette);
            bool canceled = false;
            try
            {
                session.PerformCopy();
                return (canceled = context.IsCancellationRequested) ? null : session.Target;
            }
            catch (Exception)
            {
                session.Target.Dispose();
                session.Target = null;
                throw;
            }
            finally
            {
                if (canceled)
                    session.Target?.Dispose();
            }
        }

        [SuppressMessage("ReSharper", "AssignmentInConditionalExpression", Justification = "Intended")]
        [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "ReSharper issue")]
        private static IReadWriteBitmapData? DoCloneDirect(IAsyncContext context, IReadableBitmapData source, Rectangle sourceRectangle, PixelFormat pixelFormat, Color32 backColor = default, byte alphaThreshold = 128, Palette? palette = null)
        {
            var session = new CopySession(context);
            var sourceBounds = new Rectangle(default, source.GetSize());
            Unwrap(ref source, ref sourceRectangle);
            (session.SourceRectangle, session.TargetRectangle) = GetActualRectangles(sourceBounds, sourceRectangle, new Rectangle(Point.Empty, sourceBounds.Size), Point.Empty);
            if (session.SourceRectangle.IsEmpty || session.TargetRectangle.IsEmpty)
                throw new ArgumentOutOfRangeException(nameof(sourceRectangle), PublicResources.ArgumentOutOfRange);

            if (palette == null)
            {
                int bpp = pixelFormat.ToBitsPerPixel();
                if (bpp <= 8 && source.Palette?.Entries.Length <= (1 << bpp))
                    palette = backColor == source.Palette!.BackColor && alphaThreshold == source.Palette.AlphaThreshold ? source.Palette : new Palette(source.Palette.Entries, backColor, alphaThreshold);
            }

            session.Source = source as IBitmapDataInternal ?? new BitmapDataWrapper(source, true, false);
            session.Target = source is ICustomBitmapData customBitmapData && customBitmapData.PixelFormat == pixelFormat
                ? customBitmapData.CreateCompatibleBitmapDataFactory.Invoke(session.TargetRectangle.Size)
                : BitmapDataFactory.CreateManagedBitmapData(session.TargetRectangle.Size, pixelFormat.IsValidFormat() ? pixelFormat : source.GetKnownPixelFormat(), backColor, alphaThreshold, palette);
            bool canceled = false;
            try
            {
                session.PerformCopy();
                return (canceled = context.IsCancellationRequested) ? null : session.Target;
            }
            catch (Exception)
            {
                session.Target.Dispose();
                session.Target = null;
                throw;
            }
            finally
            {
                if (canceled)
                    session.Target?.Dispose();
            }
        }

        [SuppressMessage("ReSharper", "AssignmentInConditionalExpression", Justification = "Intended")]
        [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "ReSharper issue")]
        private static IReadWriteBitmapData? DoCloneWithQuantizer(IAsyncContext context, IReadableBitmapData source, Rectangle sourceRectangle, PixelFormat pixelFormat, IQuantizer? quantizer, IDitherer? ditherer = null)
        {
            if (quantizer == null)
            {
                // copying without using a quantizer (even if only a ditherer is specified for a high-bpp pixel format)
                // Note: Not using source.BackColor/AlphaThreshold/Palette so the behavior will be compatible with the other Clone overloads with default parameters
                //       and even with ImageExtensions.ConvertPixelFormat where there are no BackColor/AlphaThreshold for source image
                if (ditherer == null || !pixelFormat.CanBeDithered())
                    return DoCloneDirect(context, source, sourceRectangle, pixelFormat);

                // here we need to pick a quantizer for the dithering
                int bpp = pixelFormat.ToBitsPerPixel();
                Color32[] paletteEntries = source.Palette?.Entries ?? Reflector.EmptyArray<Color32>();
                quantizer = bpp <= 8 && paletteEntries.Length > 0 && paletteEntries.Length <= (1 << bpp)
                    ? PredefinedColorsQuantizer.FromCustomPalette(source.Palette!)
                    : PredefinedColorsQuantizer.FromPixelFormat(pixelFormat);
            }

            var session = new CopySession(context);
            var sourceBounds = new Rectangle(default, source.GetSize());
            Unwrap(ref source, ref sourceRectangle);
            (session.SourceRectangle, session.TargetRectangle) = GetActualRectangles(sourceBounds, sourceRectangle, new Rectangle(Point.Empty, sourceBounds.Size), Point.Empty);
            if (session.SourceRectangle.IsEmpty || session.TargetRectangle.IsEmpty)
                throw new ArgumentOutOfRangeException(nameof(sourceRectangle), PublicResources.ArgumentOutOfRange);

            // Using a clipped source for quantizer/ditherer if needed. Note: the CopySession uses the original source for the best performance
            IReadableBitmapData initSource = session.SourceRectangle.Size == source.GetSize()
                ? source
                : source.Clip(session.SourceRectangle);

            bool canceled = false;
            try
            {
                context.Progress?.New(DrawingOperation.InitializingQuantizer);
                using (IQuantizingSession quantizingSession = quantizer.Initialize(initSource, context))
                {
                    if (canceled = context.IsCancellationRequested)
                        return null;
                    if (quantizingSession == null)
                        throw new InvalidOperationException(Res.ImagingQuantizerInitializeNull);

                    session.Source = source as IBitmapDataInternal ?? new BitmapDataWrapper(source, true, false);
                    session.Target = BitmapDataFactory.CreateManagedBitmapData(session.TargetRectangle.Size, pixelFormat, quantizingSession.BackColor, quantizingSession.AlphaThreshold, quantizingSession.Palette);

                    // quantizing without dithering
                    if (ditherer == null)
                        session.PerformCopyWithQuantizer(quantizingSession, false);
                    else
                    {
                        // quantizing with dithering
                        context.Progress?.New(DrawingOperation.InitializingDitherer);
                        using IDitheringSession ditheringSession = ditherer.Initialize(initSource, quantizingSession, context);
                        if (canceled = context.IsCancellationRequested)
                            return null;
                        if (ditheringSession == null)
                            throw new InvalidOperationException(Res.ImagingDithererInitializeNull);
                        session.PerformCopyWithDithering(quantizingSession, ditheringSession, false);
                    }

                    return (canceled = context.IsCancellationRequested) ? null : session.Target;
                }
            }
            catch (Exception)
            {
                session.Target?.Dispose();
                session.Target = null;
                throw;
            }
            finally
            {
                if (!ReferenceEquals(initSource, source))
                    initSource.Dispose();
                if (canceled)
                    session.Target?.Dispose();
            }
        }

        private static void DoCopy(IAsyncContext context, IReadableBitmapData source, IWritableBitmapData target, Rectangle sourceRectangle, Point targetLocation, IQuantizer? quantizer, IDitherer? ditherer, bool skipTransparent = false)
        {
            var session = new CopySession(context);
            var sourceBounds = new Rectangle(default, source.GetSize());
            var targetBounds = new Rectangle(default, target.GetSize());
            Unwrap(ref source, ref sourceBounds);
            Unwrap(ref target, ref targetBounds);

            (session.SourceRectangle, session.TargetRectangle) = GetActualRectangles(sourceBounds, sourceRectangle, targetBounds, targetLocation);
            if (session.SourceRectangle.IsEmpty || session.TargetRectangle.IsEmpty)
                return;

            AdjustQuantizerAndDitherer(target, ref quantizer, ref ditherer);

            // special handling for same references
            if (ReferenceEquals(source, target))
            {
                // same area without quantizing: nothing to do
                if (quantizer == null && session.SourceRectangle == session.TargetRectangle)
                    return;

                // overlap: clone source
                if (session.SourceRectangle.IntersectsWith(session.TargetRectangle))
                {
                    session.Source = (IBitmapDataInternal?)DoCloneDirect(context, source, session.SourceRectangle, source.PixelFormat);
                    if (context.IsCancellationRequested)
                    {
                        session.Source?.Dispose();
                        return;
                    }

                    session.SourceRectangle.Location = Point.Empty;
                }
            }

            if (session.Source == null)
                session.Source = source as IBitmapDataInternal ?? new BitmapDataWrapper(source, true, false);
            session.Target = target as IBitmapDataInternal ?? new BitmapDataWrapper(target, false, true);

            try
            {
                // processing without using a quantizer
                if (quantizer == null)
                {
                    Debug.Assert(!skipTransparent, "Skipping transparent source pixels is not expected without quantizing. Handle it if really needed.");
                    session.PerformCopy();
                    return;
                }

                // Using a clipped source for quantizer/ditherer if needed. Note: the CopySession uses the original source for the best performance
                IReadableBitmapData initSource = session.SourceRectangle.Size == source.GetSize()
                    ? source
                    : source.Clip(session.SourceRectangle);

                try
                {
                    context.Progress?.New(DrawingOperation.InitializingQuantizer);
                    using (IQuantizingSession quantizingSession = quantizer.Initialize(initSource, context))
                    {
                        if (context.IsCancellationRequested)
                            return;
                        if (quantizingSession == null)
                            throw new InvalidOperationException(Res.ImagingQuantizerInitializeNull);

                        // quantization without dithering
                        if (ditherer == null)
                        {
                            session.PerformCopyWithQuantizer(quantizingSession, skipTransparent);
                            return;
                        }

                        // quantization with dithering
                        context.Progress?.New(DrawingOperation.InitializingDitherer);
                        using (IDitheringSession ditheringSession = ditherer.Initialize(initSource, quantizingSession, context))
                        {
                            if (context.IsCancellationRequested)
                                return;
                            if (ditheringSession == null)
                                throw new InvalidOperationException(Res.ImagingDithererInitializeNull);
                            session.PerformCopyWithDithering(quantizingSession, ditheringSession, skipTransparent);
                        }
                    }
                }
                finally
                {
                    if (!ReferenceEquals(initSource, source))
                        initSource.Dispose();
                }
            }
            finally
            {
                if (!ReferenceEquals(session.Source, source))
                    session.Source.Dispose();
                if (!ReferenceEquals(session.Target, target))
                    session.Target.Dispose();
            }
        }

        #endregion

        #region Draw

        [MethodImpl(MethodImpl.AggressiveInlining)]
        private static void DoDrawInto(IAsyncContext context, IReadableBitmapData source, IReadWriteBitmapData target, Rectangle sourceRectangle, Point targetLocation, IQuantizer? quantizer, IDitherer? ditherer)
        {
            if (source.HasAlpha())
                DoDrawWithoutResize(context, source, target, sourceRectangle, targetLocation, quantizer, ditherer);
            else
                DoCopy(context, source, target, sourceRectangle, targetLocation, quantizer, ditherer);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        private static void DoDrawInto(IAsyncContext context, IReadableBitmapData source, IReadWriteBitmapData target, Rectangle sourceRectangle, Rectangle targetRectangle, IQuantizer? quantizer, IDitherer? ditherer, ScalingMode scalingMode)
        {
            // no scaling is necessary
            if (sourceRectangle.Size == targetRectangle.Size || scalingMode == ScalingMode.NoScaling)
            {
                if (source.HasAlpha())
                    DoDrawWithoutResize(context, source, target, sourceRectangle, targetRectangle.Location, quantizer, ditherer);
                else
                    DoCopy(context, source, target, sourceRectangle, targetRectangle.Location, quantizer, ditherer);
                return;
            }

            DoDrawWithResize(context, source, target, sourceRectangle, targetRectangle, quantizer, ditherer, scalingMode);
        }

        private static void DoDrawWithoutResize(IAsyncContext context, IReadableBitmapData source, IReadWriteBitmapData target, Rectangle sourceRectangle, Point targetLocation, IQuantizer? quantizer, IDitherer? ditherer)
        {
            Debug.Assert(source.HasAlpha(), "DoCopy could have been called");

            var sourceBounds = new Rectangle(default, source.GetSize());
            var targetBounds = new Rectangle(default, target.GetSize());
            Unwrap(ref source, ref sourceBounds);
            Unwrap(ref target, ref targetBounds);

            (Rectangle actualSourceRectangle, Rectangle actualTargetRectangle) = GetActualRectangles(sourceBounds, sourceRectangle, targetBounds, targetLocation);
            if (actualSourceRectangle.IsEmpty || actualTargetRectangle.IsEmpty)
                return;

            AdjustQuantizerAndDitherer(target, ref quantizer, ref ditherer);

            IBitmapDataInternal? sessionTarget;
            Rectangle sessionTargetRectangle = actualTargetRectangle;
            bool targetCloned = false;
            bool isTwoPass = source.HasMultiLevelAlpha() && (quantizer?.InitializeReliesOnContent == true || ditherer?.InitializeReliesOnContent == true);

            // if two pass is needed we create a temp result where we perform blending before quantizing/dithering
            if (isTwoPass)
            {
                sessionTarget = (IBitmapDataInternal?)DoCloneDirect(context, target, actualTargetRectangle, target.PixelFormat == PixelFormat.Format32bppArgb ? PixelFormat.Format32bppArgb : PixelFormat.Format32bppPArgb);
                if (context.IsCancellationRequested)
                {
                    sessionTarget?.Dispose();
                    return;
                }

                Debug.Assert(sessionTarget != null);
                sessionTargetRectangle.Location = Point.Empty;
                targetCloned = true;
            }
            else
                sessionTarget = target as IBitmapDataInternal ?? new BitmapDataWrapper(target, true, true);

            IBitmapDataInternal? sessionSource = null;

            // special handling for same references
            if (ReferenceEquals(source, target) && !targetCloned)
            {
                // same area without quantizing: nothing to do
                if (quantizer == null && actualSourceRectangle == actualTargetRectangle)
                    return;

                // overlap: clone source
                if (actualSourceRectangle.IntersectsWith(actualTargetRectangle))
                {
                    sessionSource = (IBitmapDataInternal?)DoCloneDirect(context, source, actualSourceRectangle, source.PixelFormat);
                    if (context.IsCancellationRequested)
                    {
                        sessionSource?.Dispose();
                        return;
                    }

                    actualSourceRectangle.Location = Point.Empty;
                }
            }

            if (sessionSource == null)
                sessionSource = source as IBitmapDataInternal ?? new BitmapDataWrapper(source, true, false);

            try
            {
                var session = new CopySession(context, sessionSource, sessionTarget!, actualSourceRectangle, sessionTargetRectangle);
                if (!isTwoPass)
                {
                    session.PerformDraw(quantizer, ditherer);
                    return;
                }

                // first pass: performing blending into transient result
                session.PerformDrawDirect();

                // second pass: copying the blended transient result to the actual target
                DoCopy(context, sessionTarget!, target, sessionTargetRectangle, actualTargetRectangle.Location, quantizer, ditherer, true);
            }
            finally
            {
                if (!ReferenceEquals(sessionSource, source))
                    sessionSource.Dispose();
                if (!ReferenceEquals(sessionTarget, target))
                    sessionTarget!.Dispose();
            }
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502: Avoid excessive complexity",
            Justification = "It would be OK without the frequent context.IsCancellationRequested checks, it's not worth the refactoring")]
        private static void DoDrawWithResize(IAsyncContext context, IReadableBitmapData source, IReadWriteBitmapData target, Rectangle sourceRectangle, Rectangle targetRectangle, IQuantizer? quantizer, IDitherer? ditherer, ScalingMode scalingMode)
        {
            Debug.Assert(sourceRectangle.Size != targetRectangle.Size || scalingMode == ScalingMode.NoScaling, $"{nameof(DoDrawWithoutResize)} could have been called");

            var sourceBounds = new Rectangle(default, source.GetSize());
            var targetBounds = new Rectangle(default, target.GetSize());
            Unwrap(ref source, ref sourceBounds);
            Unwrap(ref target, ref targetBounds);

            (Rectangle actualSourceRectangle, Rectangle actualTargetRectangle) = GetActualRectangles(sourceBounds, sourceRectangle, targetBounds, targetRectangle);
            if (actualSourceRectangle.IsEmpty || actualTargetRectangle.IsEmpty)
                return;

            AdjustQuantizerAndDitherer(target, ref quantizer, ref ditherer);

            IBitmapDataInternal? sessionTarget;
            Rectangle sessionTargetRectangle = actualTargetRectangle;
            bool targetCloned = false;

            // note: when resizing, we cannot trick the quantizer/ditherer with a single-bit alpha source because the source is needed to be resized
            bool isTwoPass = quantizer?.InitializeReliesOnContent == true || ditherer?.InitializeReliesOnContent == true;

            // if two pass is needed we create a temp result where we perform resize (with or without blending) before quantizing/dithering
            if (isTwoPass)
            {
                sessionTarget = source.HasMultiLevelAlpha()
                    ? (IBitmapDataInternal?)DoCloneDirect(context, target, actualTargetRectangle, target.PixelFormat == PixelFormat.Format32bppArgb ? PixelFormat.Format32bppArgb : PixelFormat.Format32bppPArgb)
                    : BitmapDataFactory.CreateManagedBitmapData(sessionTargetRectangle.Size, PixelFormat.Format32bppPArgb);
                if (context.IsCancellationRequested)
                {
                    sessionTarget?.Dispose();
                    return;
                }

                Debug.Assert(sessionTarget != null);
                sessionTargetRectangle.Location = Point.Empty;
                targetCloned = true;
            }
            else
                sessionTarget = target as IBitmapDataInternal ?? new BitmapDataWrapper(target, true, true);

            IBitmapDataInternal? sessionSource = null;

            // special handling for same references
            if (ReferenceEquals(source, target) && !targetCloned)
            {
                // same area without quantizing: nothing to do
                if (quantizer == null && actualSourceRectangle == actualTargetRectangle)
                    return;

                // overlap: clone source
                if (actualSourceRectangle.IntersectsWith(actualTargetRectangle))
                {
                    sessionSource = (IBitmapDataInternal?)DoCloneDirect(context, source, actualSourceRectangle, source.PixelFormat);
                    if (context.IsCancellationRequested)
                    {
                        sessionSource?.Dispose();
                        return;
                    }

                    actualSourceRectangle.Location = Point.Empty;
                }
            }

            if (sessionSource == null)
                sessionSource = source as IBitmapDataInternal ?? new BitmapDataWrapper(source, true, false);

            try
            {
                if (scalingMode == ScalingMode.NearestNeighbor)
                {
                    var session = new ResizingSessionNearestNeighbor(context, sessionSource, sessionTarget!, actualSourceRectangle, sessionTargetRectangle);
                    if (!isTwoPass)
                    {
                        session.PerformResize(quantizer, ditherer);
                        return;
                    }

                    // first pass: performing resizing into a transient result
                    session.PerformResizeDirect();
                }
                else
                {
                    using var session = new ResizingSessionInterpolated(context, sessionSource, sessionTarget!, actualSourceRectangle, sessionTargetRectangle, scalingMode);
                    if (context.IsCancellationRequested)
                        return;

                    if (!isTwoPass)
                    {
                        session.PerformResize(quantizer, ditherer);
                        return;
                    }

                    // first pass: performing blending into transient result
                    session.PerformResizeDirect();
                }

                if (context.IsCancellationRequested)
                    return;

                // second pass: copying the possibly blended transient result to the actual target with quantizing/dithering
                DoCopy(context, sessionTarget!, target, sessionTargetRectangle, actualTargetRectangle.Location, quantizer, ditherer, true);
            }
            finally
            {
                if (!ReferenceEquals(sessionSource, source))
                    sessionSource.Dispose();
                if (!ReferenceEquals(sessionTarget, target))
                    sessionTarget!.Dispose();
            }
        }

        #endregion

        #region Bounds

        private static (Rectangle Source, Rectangle Target) GetActualRectangles(Rectangle sourceBounds, Rectangle sourceRectangle, Rectangle targetBounds, Point targetLocation)
        {
            sourceRectangle.Offset(sourceBounds.Location);
            Rectangle actualSourceRectangle = Rectangle.Intersect(sourceRectangle, sourceBounds);
            if (actualSourceRectangle.IsEmpty)
                return default;
            targetLocation.Offset(targetBounds.Location);
            Rectangle targetRectangle = new Rectangle(targetLocation, sourceRectangle.Size);
            Rectangle actualTargetRectangle = Rectangle.Intersect(targetRectangle, targetBounds);
            if (actualTargetRectangle.IsEmpty)
                return default;

            // adjusting source by clipped target
            if (targetRectangle != actualTargetRectangle)
            {
                int x = actualTargetRectangle.X - targetRectangle.X + sourceRectangle.X;
                int y = actualTargetRectangle.Y - targetRectangle.Y + sourceRectangle.Y;
                actualSourceRectangle.Intersect(new Rectangle(x, y, actualTargetRectangle.Width, actualTargetRectangle.Height));
            }

            // adjusting target by clipped source
            if (sourceRectangle != actualSourceRectangle)
            {
                int x = actualSourceRectangle.X - sourceRectangle.X + targetRectangle.X;
                int y = actualSourceRectangle.Y - sourceRectangle.Y + targetRectangle.Y;
                actualTargetRectangle.Intersect(new Rectangle(x, y, actualSourceRectangle.Width, actualSourceRectangle.Height));
            }

            return (actualSourceRectangle, actualTargetRectangle);
        }

        private static (Rectangle Source, Rectangle Target) GetActualRectangles(Rectangle sourceBounds, Rectangle sourceRectangle, Rectangle targetBounds, Rectangle targetRectangle)
        {
            sourceRectangle.Offset(sourceBounds.Location);
            Rectangle actualSourceRectangle = Rectangle.Intersect(sourceRectangle, sourceBounds);
            if (actualSourceRectangle.IsEmpty)
                return default;
            targetRectangle.Offset(targetBounds.Location);
            Rectangle actualTargetRectangle = Rectangle.Intersect(targetRectangle, targetBounds);
            if (actualTargetRectangle.IsEmpty)
                return default;

            float widthRatio = (float)sourceRectangle.Width / targetRectangle.Width;
            float heightRatio = (float)sourceRectangle.Height / targetRectangle.Height;

            // adjusting source by clipped target
            if (targetRectangle != actualTargetRectangle)
            {
                int x = (int)MathF.Round((actualTargetRectangle.X - targetRectangle.X) * widthRatio + sourceRectangle.X);
                int y = (int)MathF.Round((actualTargetRectangle.Y - targetRectangle.Y) * heightRatio + sourceRectangle.Y);
                int w = (int)MathF.Round(actualTargetRectangle.Width * widthRatio);
                int h = (int)MathF.Round(actualTargetRectangle.Height * heightRatio);
                actualSourceRectangle.Intersect(new Rectangle(x, y, w, h));
            }

            // adjusting target by clipped source
            if (sourceRectangle != actualSourceRectangle)
            {
                int x = (int)MathF.Round((actualSourceRectangle.X - sourceRectangle.X) / widthRatio + targetRectangle.X);
                int y = (int)MathF.Round((actualSourceRectangle.Y - sourceRectangle.Y) / heightRatio + targetRectangle.Y);
                int w = (int)MathF.Round(actualSourceRectangle.Width / widthRatio);
                int h = (int)MathF.Round(actualSourceRectangle.Height / heightRatio);
                actualTargetRectangle.Intersect(new Rectangle(x, y, w, h));
            }

            return (actualSourceRectangle, actualTargetRectangle);
        }

        #endregion

        #region ToBitmap

        [SuppressMessage("ReSharper", "AssignmentInConditionalExpression", Justification = "Intended")]
        [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "ReSharper issue")]
        private static Bitmap? DoConvertToBitmap(IAsyncContext context, IReadableBitmapData source)
        {
            PixelFormat pixelFormat = source.PixelFormat.IsSupportedNatively() ? source.PixelFormat
                : source.HasAlpha() ? PixelFormat.Format32bppArgb
                : PixelFormat.Format24bppRgb;

            var result = new Bitmap(source.Width, source.Height, pixelFormat);
            bool canceled = false;
            try
            {
                if (pixelFormat.IsIndexed() && source.Palette != null)
                    result.TrySetPalette(source.Palette);

                if (canceled = context.IsCancellationRequested)
                    return null;
                using (IBitmapDataInternal target = BitmapDataFactory.CreateBitmapData(result, ImageLockMode.WriteOnly, source.BackColor, source.AlphaThreshold, source.Palette))
                    DoCopy(context, source, target, new Rectangle(Point.Empty, source.GetSize()), Point.Empty, null, null);
                return (canceled = context.IsCancellationRequested) ? null : result;
            }
            catch (Exception)
            {
                result.Dispose();
                result = null;
                throw;
            }
            finally
            {
                if (canceled)
                    result?.Dispose();
            }
        }

        #endregion

        #region GetColors

        private static ICollection<Color32> DoGetColors(IAsyncContext context, IReadableBitmapData bitmapData, int maxColors)
        {
            if (maxColors < 0)
                throw new ArgumentOutOfRangeException(nameof(maxColors), PublicResources.ArgumentOutOfRange);
            if (maxColors == 0)
                maxColors = bitmapData.Palette?.Count ?? bitmapData.PixelFormat.GetColorsLimit();

            var colors = new HashSet<Color32>();
            var data = bitmapData as IBitmapDataInternal ?? new BitmapDataWrapper(bitmapData, true, false);

            try
            {
                context.Progress?.New(DrawingOperation.ProcessingPixels, data.Height);
                IBitmapDataRowInternal line = data.DoGetRow(0);

                do
                {
                    if (context.IsCancellationRequested)
                        return Reflector.EmptyArray<Color32>();
                    for (int x = 0; x < data.Width; x++)
                    {
                        Color32 c = line.DoGetColor32(x);
                        colors.Add(c.A == 0 ? Color32.Transparent : c);
                        if (colors.Count == maxColors)
                        {
                            context.Progress?.Complete();
                            return colors;
                        }
                    }

                    context.Progress?.Increment();
                } while (line.MoveNextRow());

            }
            finally
            {
                if (!ReferenceEquals(data, bitmapData))
                    data.Dispose();
            }

            return colors;
        }

        private static int DoGetColorCount(IAsyncContext context, IReadableBitmapData bitmapData)
        {
            switch (bitmapData.PixelFormat)
            {
                case PixelFormat.Format16bppGrayScale:
                    return bitmapData.RowSize >= bitmapData.Width << 1
                        ? GetColorCount<Color16Gray>(context, bitmapData)
                        : DoGetColors(context, bitmapData, 0).Count;
                case PixelFormat.Format48bppRgb:
                    return bitmapData.RowSize >= bitmapData.Width * 6
                        ? GetColorCount<Color48>(context, bitmapData)
                        : DoGetColors(context, bitmapData, 0).Count;
                case PixelFormat.Format64bppArgb:
                case PixelFormat.Format64bppPArgb:
                    return bitmapData.RowSize >= bitmapData.Width << 3
                        ? GetColorCount<Color64>(context, bitmapData)
                        : DoGetColors(context, bitmapData, 0).Count;
                default:
                    return DoGetColors(context, bitmapData, 0).Count;
            }
        }

        private static int GetColorCount<T>(IAsyncContext context, IReadableBitmapData bitmapData) where T : unmanaged
        {
            var colors = new HashSet<T>();
            var data = bitmapData as IBitmapDataInternal ?? new BitmapDataWrapper(bitmapData, true, false);
            try
            {
                context.Progress?.New(DrawingOperation.ProcessingPixels, data.Height);
                IBitmapDataRowInternal line = data.DoGetRow(0);

                do
                {
                    if (context.IsCancellationRequested)
                        return default;
                    for (int x = 0; x < data.Width; x++)
                    {
                        T color = line.DoReadRaw<T>(x);

                        // The JIT compiler will optimize away these branches
                        if (color is Color64 c64 && c64.A == 0)
                            color = default;
                        colors.Add(color);
                        if (typeof(T) == typeof(Color16Gray) && colors.Count == UInt16.MaxValue)
                            return colors.Count;
                    }

                    context.Progress?.Increment();
                } while (line.MoveNextRow());
            }
            finally
            {
                if (!ReferenceEquals(data, bitmapData))
                    data.Dispose();
            }

            return colors.Count;
        }

        #endregion

        #region ToTransparent

        private static IReadWriteBitmapData? DoToTransparent(IAsyncContext context, IReadableBitmapData bitmapData)
        {
            var srcRect = new Rectangle(Point.Empty, bitmapData.GetSize());
            if (bitmapData.Width < 1 || bitmapData.Height < 1)
                return DoCloneDirect(context, bitmapData, srcRect, PixelFormat.Format32bppArgb);
            Color32 transparentColor = bitmapData[bitmapData.Height - 1][0];
            if (transparentColor.A < Byte.MaxValue)
                return DoCloneDirect(context, bitmapData, srcRect, PixelFormat.Format32bppArgb);
            return DoCloneWithQuantizer(context, bitmapData, srcRect, PixelFormat.Format32bppArgb,
                PredefinedColorsQuantizer.FromCustomFunction(c => TransformReplaceColor(c, transparentColor, default)));
        }

        private static IReadWriteBitmapData? DoToTransparent(IAsyncContext context, IReadableBitmapData bitmapData, Color32 transparentColor)
        {
            var srcRect = new Rectangle(Point.Empty, bitmapData.GetSize());
            if (transparentColor.A == 0)
                return DoCloneDirect(context, bitmapData, srcRect, PixelFormat.Format32bppArgb);
            return DoCloneWithQuantizer(context, bitmapData, srcRect, PixelFormat.Format32bppArgb,
                PredefinedColorsQuantizer.FromCustomFunction(c => TransformReplaceColor(c, transparentColor, default)));
        }

        #endregion

        #region Save

        private static void DoSave(IAsyncContext context, IReadableBitmapData bitmapData, Stream stream)
        {
            Size size = bitmapData.GetSize();
            var srcRect = new Rectangle(Point.Empty, size);
            Unwrap(ref bitmapData, ref srcRect);
            IBitmapDataInternal source = bitmapData as IBitmapDataInternal ?? new BitmapDataWrapper(bitmapData, true, false);

            try
            {
                BitmapDataFactory.DoSaveBitmapData(context, source, srcRect, stream);
            }
            finally
            {
                if (!ReferenceEquals(bitmapData, source))
                    source!.Dispose();
            }
        }

        #endregion

        #endregion

        #endregion
    }
}
